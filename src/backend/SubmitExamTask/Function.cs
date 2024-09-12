// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using Newtonsoft.Json;
using PlagiarismRepository;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SubmitExamTask;

public class Function
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly AmazonStepFunctionsClient _amazonStepFunctionsClient;

    /// <summary>
    /// Default constructor
    /// </summary>
    public Function()
    {
        Tracing.RegisterForAllServices();
        _incidentRepository = new IncidentRepository(Environment.GetEnvironmentVariable("TABLE_NAME"));
        _amazonStepFunctionsClient = new AmazonStepFunctionsClient();
    }

    /// <summary>
    /// Constructor used for testing purposes
    /// </summary>
    /// <param name="stepFunctions"></param>
    /// <param name="incidentRepository"></param>
    public Function(IAmazonStepFunctions stepFunctions, IIncidentRepository incidentRepository)
    {
        Tracing.RegisterForAllServices();
        _incidentRepository = incidentRepository;
        _amazonStepFunctionsClient = (AmazonStepFunctionsClient)stepFunctions;
    }

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="request">Instance of APIGatewayProxyRequest</param>
    /// <param name="context">AWS Lambda Context</param>
    /// <returns>Instance of APIGatewayProxyResponse</returns>
    [Logging(LogEvent = true)]
    [Tracing(CaptureMode = TracingCaptureMode.ResponseAndError)]
    [Metrics(CaptureColdStart = true)]
    public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var body = JsonConvert.DeserializeObject<Dictionary<string, string>>(request?.Body);

        var isIncidentId = Guid.TryParse(body["IncidentId"], out var incidentId);
        var isExamId = Guid.TryParse(body["ExamId"], out var examId);
        var isScore = int.TryParse(body["Score"], out var score);

        var token = body["TaskToken"];

            if (!isIncidentId || !isExamId | !isScore | !(token.Length >= 1 & token.Length <= 1024))
            {
                Logger.LogInformation($"Invalid request: {request?.Body}\n\nIncidentId {incidentId} ExamId {examId} Score {score} Token {token}");

                return ApiGatewayResponse(HttpStatusCode.BadRequest);
                
            }

        Logger.LogInformation("IncidentId: {incidentId}, ExamId: {examId}, Score: {score}, Token: {token}",
            incidentId, examId, score, token);

        var incident = _incidentRepository.GetIncidentById(incidentId);
        var exam = incident.Exams.Find(e => e.ExamId == examId);
        exam.Score = score;

        _incidentRepository.SaveIncident(incident);

        Logger.LogInformation(JsonConvert.SerializeObject(incident));

        var sendTaskSuccessRequest = new SendTaskSuccessRequest
        {
            TaskToken = token,
            Output = JsonConvert.SerializeObject(incident)
        };

        try
        {
            _amazonStepFunctionsClient.SendTaskSuccessAsync(sendTaskSuccessRequest).Wait();
        }
        catch (Exception e)
        {
            Logger.LogError(e);
            return ApiGatewayResponse(HttpStatusCode.InternalServerError);
        }

        return ApiGatewayResponse(HttpStatusCode.OK);
    }

    /// <summary>
    /// Returns ApiGatewayResponse with specified status code
    /// </summary>
    /// <param name="statusCode">HttpStatusCode</param>
    /// <returns>Instance of ApiGatewayResponse</returns>
    private APIGatewayProxyResponse ApiGatewayResponse(HttpStatusCode statusCode)
    {
        
        return new APIGatewayProxyResponse
        {   
            StatusCode = (int)statusCode,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Headers", "Content-Type" },
                { "Access-Control-Allow-Methods", "OPTIONS,POST" }
            }
        };
    }
    
}