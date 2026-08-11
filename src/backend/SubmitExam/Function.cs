// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using Plagiarism;
using PlagiarismRepository;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SubmitExam;

public class Function
{
    private const int MaxTaskTokenLength = 1024;

    private readonly IIncidentRepository _incidentRepository;
    private readonly IAmazonStepFunctions _stepFunctionsClient;

    /// <summary>
    /// Default constructor
    /// </summary>
    public Function()
    {
        Tracing.RegisterForAllServices();
        _incidentRepository = new IncidentRepository(Environment.GetEnvironmentVariable("TABLE_NAME"));
        _stepFunctionsClient = new AmazonStepFunctionsClient();
    }

    /// <summary>
    /// Constructor used for testing purposes
    /// </summary>
    /// <param name="stepFunctions">Step Functions client</param>
    /// <param name="incidentRepository">Incident repository</param>
    public Function(IAmazonStepFunctions stepFunctions, IIncidentRepository incidentRepository)
    {
        Tracing.RegisterForAllServices();
        _incidentRepository = incidentRepository;
        _stepFunctionsClient = stepFunctions;
    }

    /// <summary>
    /// Records the student's exam score and resumes the waiting Step Functions
    /// execution via the task token callback.
    /// </summary>
    /// <param name="request">Instance of APIGatewayProxyRequest</param>
    /// <param name="context">AWS Lambda Context</param>
    /// <returns>Instance of APIGatewayProxyResponse</returns>
    [Logging]
    [Tracing(CaptureMode = TracingCaptureMode.ResponseAndError)]
    [Metrics(CaptureColdStart = true)]
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        if (string.IsNullOrWhiteSpace(request?.Body))
        {
            return ApiGatewayResponse(HttpStatusCode.BadRequest, "Request body is required.");
        }

        if (!TryParseSubmission(request.Body, out var submission, out var validationError))
        {
            Logger.LogWarning("Invalid exam submission: {ValidationError}", validationError);
            return ApiGatewayResponse(HttpStatusCode.BadRequest, validationError);
        }

        Logger.LogInformation("IncidentId: {IncidentId}, ExamId: {ExamId}, Score: {Score}",
            submission.IncidentId, submission.ExamId, submission.Score);

        Incident incident;
        try
        {
            incident = await _incidentRepository.GetIncidentByIdAsync(submission.IncidentId);
        }
        catch (IncidentNotFoundException)
        {
            return ApiGatewayResponse(HttpStatusCode.NotFound, $"Incident {submission.IncidentId} not found.");
        }

        var exam = incident.Exams?.Find(e => e.ExamId == submission.ExamId);
        if (exam == null)
        {
            return ApiGatewayResponse(HttpStatusCode.NotFound,
                $"Exam {submission.ExamId} not found for incident {submission.IncidentId}.");
        }

        exam.Score = submission.Score;

        // Persist the score before resuming the workflow: the next state
        // (Schedule exam) reads the incident back from DynamoDB, so the score
        // must be saved before SendTaskSuccess triggers the state transition.
        await _incidentRepository.SaveIncidentAsync(incident);

        var sendTaskSuccessRequest = new SendTaskSuccessRequest
        {
            TaskToken = submission.TaskToken,
            Output = JsonSerializer.Serialize(incident)
        };

        try
        {
            await _stepFunctionsClient.SendTaskSuccessAsync(sendTaskSuccessRequest);
        }
        catch (TaskTimedOutException e)
        {
            Logger.LogWarning(e, "Task token expired or was already used.");
            return ApiGatewayResponse(HttpStatusCode.Gone,
                "This exam can no longer be submitted: the submission window has closed or the exam was already submitted.");
        }
        catch (InvalidTokenException e)
        {
            Logger.LogWarning(e, "Invalid task token.");
            return ApiGatewayResponse(HttpStatusCode.BadRequest, "The supplied task token is invalid.");
        }
        catch (Exception e)
        {
            Logger.LogError(e);
            return ApiGatewayResponse(HttpStatusCode.InternalServerError, "Failed to resume the workflow.");
        }

        return ApiGatewayResponse(HttpStatusCode.OK, "Exam submitted.");
    }

    /// <summary>
    /// Parses and validates the exam submission request body.
    /// Accepts Score as either a JSON number or a numeric string.
    /// </summary>
    private static bool TryParseSubmission(string body, out ExamSubmission submission, out string error)
    {
        submission = null;

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(body);
        }
        catch (JsonException)
        {
            error = "Request body is not valid JSON.";
            return false;
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                error = "Request body must be a JSON object.";
                return false;
            }

            if (!TryGetGuid(root, "IncidentId", out var incidentId))
            {
                error = "IncidentId is required and must be a valid GUID.";
                return false;
            }

            if (!TryGetGuid(root, "ExamId", out var examId))
            {
                error = "ExamId is required and must be a valid GUID.";
                return false;
            }

            if (!TryGetScore(root, out var score))
            {
                error = "Score is required and must be a number between 0 and 100.";
                return false;
            }

            if (!root.TryGetProperty("TaskToken", out var tokenElement)
                || tokenElement.ValueKind != JsonValueKind.String
                || string.IsNullOrEmpty(tokenElement.GetString())
                || tokenElement.GetString()!.Length > MaxTaskTokenLength)
            {
                error = $"TaskToken is required and must be between 1 and {MaxTaskTokenLength} characters.";
                return false;
            }

            submission = new ExamSubmission(incidentId, examId, score, tokenElement.GetString());
            error = null;
            return true;
        }
    }

    private static bool TryGetGuid(JsonElement root, string propertyName, out Guid value)
    {
        value = Guid.Empty;
        return root.TryGetProperty(propertyName, out var element)
               && element.ValueKind == JsonValueKind.String
               && Guid.TryParse(element.GetString(), out value);
    }

    private static bool TryGetScore(JsonElement root, out int score)
    {
        score = 0;

        if (!root.TryGetProperty("Score", out var element))
        {
            return false;
        }

        double parsed;
        switch (element.ValueKind)
        {
            case JsonValueKind.Number when element.TryGetDouble(out parsed):
                break;
            case JsonValueKind.String when double.TryParse(element.GetString(), out parsed):
                break;
            default:
                return false;
        }

        if (parsed is < 0 or > 100)
        {
            return false;
        }

        score = (int)Math.Round(parsed);
        return true;
    }

    /// <summary>
    /// Returns an APIGatewayProxyResponse with the specified status code, a JSON
    /// message body, and CORS headers.
    /// </summary>
    private static APIGatewayProxyResponse ApiGatewayResponse(HttpStatusCode statusCode, string message)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)statusCode,
            Body = JsonSerializer.Serialize(new { message }),
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Headers", "Content-Type,X-Requested-With,Accept" },
                { "Access-Control-Allow-Methods", "OPTIONS,POST" }
            }
        };
    }

    private sealed record ExamSubmission(Guid IncidentId, Guid ExamId, int Score, string TaskToken);
}
