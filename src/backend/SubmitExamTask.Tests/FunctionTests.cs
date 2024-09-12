// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using System.Collections.Generic;
using System.Threading;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Newtonsoft.Json;
using NSubstitute;
using Plagiarism;
using Xunit;
using Xunit.Abstractions;
using PlagiarismRepository;

namespace SubmitExamTask.Tests;

public class FunctionTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    private const string Token =
        "AAAAKgAAAAIAAAAAAAAAAbdvA5UnsPbXk2HGkayUMygJK8eFJq3pnwBV/xTTDwiIbXvk246zL6Y1+UxXRWzPnbLD0mex2AEUEwMfjxjOj0lW0" +
        "g+6AwFv6gA0MW/gU2SAdkHZl7tQQ1o3uBL2eOlSSYakcvPvF35BJdXCFkhhKaoqB8CzpnzkJPr7KVSXumjMouy/C4KwJJMqcVpeIW2Xhjyxq6F" +
        "FT8+GRfNspJUaGE3aId15q/dK94xRTPG/Gidez7iuINk6Y7JpbA4/sj3T2hpUuDKyi4CcCkI8A4z93Hn2Tw2OMqWwhmserDGNfI3UgW3Um6pHR" +
        "YNvL1prARZ9DkGHHftGaaXXBU8IO1mxYij4TciyP2Cky4b/Dk6ImioM0s+xdIeFOfMprMg73KG5WPK0XAWF+coMC7zBKJTtHZmudk9wKzTPdiS" +
        "EZrwmPgeD3hVeWTQXwi7GF9hVbpS8wz/QrtI78HGPcbUdMi0Y79YihuGDo6iN4booO/5Tek3prcfDKhU3JtqqqVFRp9ugqQlOxhnkGmKaajp5mi" +
        "RFDcgrghxvP8Fp4D1DDY+/5vUxHFS+tOqvrp24YpSfO51xQxp7GWeg0k9qSnSWntOKdJRjmE7gyvIhKC9XMnlLktJEeBpCQa/B3pqzIr31sPB9ooDTS7m97REIl6Gf0VOtOx4=";

    public FunctionTests(ITestOutputHelper testOutputHelper)
    {
        // Set env variable for Powertools Metrics 
        Environment.SetEnvironmentVariable("TABLE_NAME", "IncidentsTable");
        Environment.SetEnvironmentVariable("POWERTOOLS_METRICS_NAMESPACE", "Plagiarism");
        _testOutputHelper = testOutputHelper;
    }


    [Fact]
    public void SubmitExamHandlerReturns200ForValidRequest()
    {
        var mockStepFunctionsClient = Substitute.ForPartsOf<AmazonStepFunctionsClient>();
        var mockIncidentRepository = Substitute.For<IIncidentRepository>();

        var incidentId = Guid.NewGuid();
        var examId = Guid.NewGuid();
        var request = new APIGatewayProxyRequest
        {
            Body = JsonConvert.SerializeObject(new Dictionary<string, string>
            {
                { "IncidentId", incidentId.ToString() },
                { "ExamId", examId.ToString() },
                { "Score", "99" },
                { "TaskToken", Token }
            })
        };

        var context = new TestLambdaContext();

        var expectedResponse = new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Headers", "Content-Type" },
                { "Access-Control-Allow-Methods", "OPTIONS,POST" }
            }
        };

        // create a new incident with one exam
        var existingIncident = new Incident();
        existingIncident.IncidentId = incidentId;
        existingIncident.IncidentDate = DateTime.UtcNow;
        existingIncident.StudentId = Guid.NewGuid().ToString();
        existingIncident.Exams = new List<Exam>
        {
            new Exam(examId, DateTime.Now, 0)
        };

        var savedIncident = new Incident();
        savedIncident.IncidentId = incidentId;
        savedIncident.IncidentDate = DateTime.UtcNow;
        savedIncident.StudentId = Guid.NewGuid().ToString();
        savedIncident.Exams = new List<Exam>
        {
            new Exam(examId, DateTime.Now, 99)
        };

        mockIncidentRepository.GetIncidentById(incidentId).Returns(info =>
            existingIncident
        );

        mockIncidentRepository.SaveIncident(Arg.Any<Incident>()).Returns(info =>
            savedIncident
        );

        mockStepFunctionsClient.SendTaskSuccessAsync(Arg.Any<SendTaskSuccessRequest>(), CancellationToken.None)
            .Returns(new SendTaskSuccessResponse());

        var function = new Function(mockStepFunctionsClient, mockIncidentRepository);
        var response = function.FunctionHandler(request, context);

        _testOutputHelper.WriteLine("Lambda Response: \n" + response.StatusCode);
        _testOutputHelper.WriteLine("Expected Response: \n" + expectedResponse.StatusCode);

        Assert.Equal(expectedResponse.Body, response.Body);
        Assert.Equal(expectedResponse.Headers, response.Headers);
        Assert.Equal(expectedResponse.StatusCode, response.StatusCode);
    }

    [Fact]
    public void SubmitExamHandlerReturns400ForInvalidExamIdRequest()
    {
        var mockStepFunctionsClient = Substitute.ForPartsOf<AmazonStepFunctionsClient>();
        var mockIncidentRepository = Substitute.For<IIncidentRepository>();

        var request = new APIGatewayProxyRequest
        {
            Body = JsonConvert.SerializeObject(
                new Dictionary<string, string>
                {
                    { "ExamId", "dc-not-a-guid" },
                    { "IncidentId", "6b44fd97-1af3-42f6-9a0b-0138fffa8cf4" },
                    { "Score", "65" },
                    { "TaskToken", Token }
                })
        };

        var context = new TestLambdaContext();

        var expectedResponse = new APIGatewayProxyResponse
        {
            StatusCode = 400,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Headers", "Content-Type" },
                { "Access-Control-Allow-Methods", "OPTIONS,POST" },
            }
        };

        var function = new Function(mockStepFunctionsClient, mockIncidentRepository);
        var response = function.FunctionHandler(request, context);

        _testOutputHelper.WriteLine("Lambda Response: \n" + response.StatusCode);
        _testOutputHelper.WriteLine("Expected Response: \n" + expectedResponse.StatusCode);

        Assert.Equal(expectedResponse.Headers, response.Headers);
        Assert.Equal(expectedResponse.StatusCode, response.StatusCode);
    }

    [Fact]
    public void SubmitExamHandlerReturns400ForInvalidIncidentIdRequest()
    {
        var mockStepFunctionsClient = Substitute.ForPartsOf<AmazonStepFunctionsClient>();
        var mockIncidentRepository = Substitute.For<IIncidentRepository>();

        var request = new APIGatewayProxyRequest
        {
            Body = JsonConvert.SerializeObject(new Dictionary<string, string>
            {
                { "ExamId", "dc149d4b-ce6d-435a-b922-9da90f7c3eed" },
                { "IncidentId", "not-a-guid-0138fffa8cf4" },
                { "Score", "65" },
                { "TaskToken", Token }
            })
        };

        var context = new TestLambdaContext();

        var expectedResponse = new APIGatewayProxyResponse
        {
            StatusCode = 400,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Headers", "Content-Type" },
                { "Access-Control-Allow-Methods", "OPTIONS,POST" },
            }
        };

        var function = new Function(mockStepFunctionsClient, mockIncidentRepository);
        var response = function.FunctionHandler(request, context);

        _testOutputHelper.WriteLine("Lambda Response: \n" + response.StatusCode);
        _testOutputHelper.WriteLine("Expected Response: \n" + expectedResponse.StatusCode);

        Assert.Equal(expectedResponse.Headers, response.Headers);
        Assert.Equal(expectedResponse.StatusCode, response.StatusCode);
    }
}