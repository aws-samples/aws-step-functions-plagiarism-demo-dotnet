// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Plagiarism;
using PlagiarismRepository;
using Xunit;

namespace SubmitExam.Tests;

public class FunctionTests
{
    private const string Token =
        "AAAAKgAAAAIAAAAAAAAAAbdvA5UnsPbXk2HGkayUMygJK8eFJq3pnwBV/xTTDwiIbXvk246zL6Y1+UxXRWzPnbLD0mex2AEUEwMfjxjOj0lW0" +
        "g+6AwFv6gA0MW/gU2SAdkHZl7tQQ1o3uBL2eOlSSYakcvPvF35BJdXCFkhhKaoqB8CzpnzkJPr7KVSXumjMouy/C4KwJJMqcVpeIW2Xhjyxq6F" +
        "FT8+GRfNspJUaGE3aId15q/dK94xRTPG/Gidez7iuINk6Y7JpbA4/sj3T2hpUuDKyi4CcCkI8A4z93Hn2Tw2OMqWwhmserDGNfI3UgW3Um6pHR" +
        "YNvL1prARZ9DkGHHftGaaXXBU8IO1mxYij4TciyP2Cky4b/Dk6ImioM0s+xdIeFOfMprMg73KG5WPK0XAWF+coMC7zBKJTtHZmudk9wKzTPdiS" +
        "EZrwmPgeD3hVeWTQXwi7GF9hVbpS8wz/QrtI78HGPcbUdMi0Y79YihuGDo6iN4booO/5Tek3prcfDKhU3JtqqqVFRp9ugqQlOxhnkGmKaajp5mi";

    private readonly IAmazonStepFunctions _stepFunctions;
    private readonly IIncidentRepository _incidentRepository;
    private readonly Function _function;
    private readonly TestLambdaContext _context;

    public FunctionTests()
    {
        Environment.SetEnvironmentVariable("TABLE_NAME", "IncidentsTable");
        Environment.SetEnvironmentVariable("POWERTOOLS_METRICS_NAMESPACE", "Plagiarism");

        _stepFunctions = Substitute.For<IAmazonStepFunctions>();
        _incidentRepository = Substitute.For<IIncidentRepository>();
        _function = new Function(_stepFunctions, _incidentRepository);
        _context = new TestLambdaContext();
    }

    private static APIGatewayProxyRequest Request(object body)
    {
        return new APIGatewayProxyRequest
        {
            Body = body as string ?? JsonSerializer.Serialize(body)
        };
    }

    private static Incident IncidentWith(Guid incidentId, Guid examId)
    {
        return new Incident
        {
            IncidentId = incidentId,
            StudentId = Guid.NewGuid().ToString(),
            IncidentDate = DateTime.UtcNow,
            Exams = new List<Exam> { new(examId, DateTime.UtcNow.AddDays(7), 0) }
        };
    }

    [Fact]
    public async Task ValidRequest_Returns200_AndSavesBeforeCallback()
    {
        var incidentId = Guid.NewGuid();
        var examId = Guid.NewGuid();

        _incidentRepository.GetIncidentByIdAsync(incidentId).Returns(IncidentWith(incidentId, examId));
        _incidentRepository.SaveIncidentAsync(Arg.Any<Incident>()).Returns(x => x.Arg<Incident>());
        _stepFunctions.SendTaskSuccessAsync(Arg.Any<SendTaskSuccessRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SendTaskSuccessResponse());

        var response = await _function.FunctionHandler(Request(new
        {
            IncidentId = incidentId,
            ExamId = examId,
            Score = 99,
            TaskToken = Token
        }), _context);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("*", response.Headers["Access-Control-Allow-Origin"]);
        await _incidentRepository.Received(1)
            .SaveIncidentAsync(Arg.Is<Incident>(i => i.Exams[0].Score == 99));
        await _stepFunctions.Received(1)
            .SendTaskSuccessAsync(Arg.Is<SendTaskSuccessRequest>(r => r.TaskToken == Token), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NumericStringScore_IsAccepted()
    {
        var incidentId = Guid.NewGuid();
        var examId = Guid.NewGuid();

        _incidentRepository.GetIncidentByIdAsync(incidentId).Returns(IncidentWith(incidentId, examId));
        _incidentRepository.SaveIncidentAsync(Arg.Any<Incident>()).Returns(x => x.Arg<Incident>());
        _stepFunctions.SendTaskSuccessAsync(Arg.Any<SendTaskSuccessRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SendTaskSuccessResponse());

        var response = await _function.FunctionHandler(Request(new
        {
            IncidentId = incidentId,
            ExamId = examId,
            Score = "65",
            TaskToken = Token
        }), _context);

        Assert.Equal(200, response.StatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not json at all")]
    [InlineData("[1,2,3]")]
    public async Task MissingOrMalformedBody_Returns400(string body)
    {
        var response = await _function.FunctionHandler(new APIGatewayProxyRequest { Body = body }, _context);

        Assert.Equal(400, response.StatusCode);
        Assert.Equal("*", response.Headers["Access-Control-Allow-Origin"]);
        await _incidentRepository.DidNotReceive().SaveIncidentAsync(Arg.Any<Incident>());
    }

    [Fact]
    public async Task NullRequest_Returns400()
    {
        var response = await _function.FunctionHandler(null, _context);

        Assert.Equal(400, response.StatusCode);
    }

    [Fact]
    public async Task InvalidIncidentId_Returns400()
    {
        var response = await _function.FunctionHandler(Request(new
        {
            IncidentId = "not-a-guid",
            ExamId = Guid.NewGuid(),
            Score = 65,
            TaskToken = Token
        }), _context);

        Assert.Equal(400, response.StatusCode);
    }

    [Fact]
    public async Task InvalidExamId_Returns400()
    {
        var response = await _function.FunctionHandler(Request(new
        {
            IncidentId = Guid.NewGuid(),
            ExamId = "dc-not-a-guid",
            Score = 65,
            TaskToken = Token
        }), _context);

        Assert.Equal(400, response.StatusCode);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task OutOfRangeScore_Returns400(int score)
    {
        var response = await _function.FunctionHandler(Request(new
        {
            IncidentId = Guid.NewGuid(),
            ExamId = Guid.NewGuid(),
            Score = score,
            TaskToken = Token
        }), _context);

        Assert.Equal(400, response.StatusCode);
    }

    [Fact]
    public async Task MissingTaskToken_Returns400()
    {
        var response = await _function.FunctionHandler(Request(new
        {
            IncidentId = Guid.NewGuid(),
            ExamId = Guid.NewGuid(),
            Score = 65
        }), _context);

        Assert.Equal(400, response.StatusCode);
    }

    [Fact]
    public async Task UnknownIncident_Returns404()
    {
        var incidentId = Guid.NewGuid();
        _incidentRepository.GetIncidentByIdAsync(incidentId)
            .ThrowsAsync(new IncidentNotFoundException("not found"));

        var response = await _function.FunctionHandler(Request(new
        {
            IncidentId = incidentId,
            ExamId = Guid.NewGuid(),
            Score = 65,
            TaskToken = Token
        }), _context);

        Assert.Equal(404, response.StatusCode);
    }

    [Fact]
    public async Task UnknownExam_Returns404()
    {
        var incidentId = Guid.NewGuid();
        _incidentRepository.GetIncidentByIdAsync(incidentId)
            .Returns(IncidentWith(incidentId, Guid.NewGuid()));

        var response = await _function.FunctionHandler(Request(new
        {
            IncidentId = incidentId,
            ExamId = Guid.NewGuid(),
            Score = 65,
            TaskToken = Token
        }), _context);

        Assert.Equal(404, response.StatusCode);
        await _incidentRepository.DidNotReceive().SaveIncidentAsync(Arg.Any<Incident>());
    }

    [Fact]
    public async Task ExpiredTaskToken_Returns410()
    {
        var incidentId = Guid.NewGuid();
        var examId = Guid.NewGuid();

        _incidentRepository.GetIncidentByIdAsync(incidentId).Returns(IncidentWith(incidentId, examId));
        _incidentRepository.SaveIncidentAsync(Arg.Any<Incident>()).Returns(x => x.Arg<Incident>());
        _stepFunctions.SendTaskSuccessAsync(Arg.Any<SendTaskSuccessRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new TaskTimedOutException("Provided task does not exist."));

        var response = await _function.FunctionHandler(Request(new
        {
            IncidentId = incidentId,
            ExamId = examId,
            Score = 65,
            TaskToken = Token
        }), _context);

        Assert.Equal(410, response.StatusCode);
    }

    [Fact]
    public async Task StepFunctionsFailure_Returns500()
    {
        var incidentId = Guid.NewGuid();
        var examId = Guid.NewGuid();

        _incidentRepository.GetIncidentByIdAsync(incidentId).Returns(IncidentWith(incidentId, examId));
        _incidentRepository.SaveIncidentAsync(Arg.Any<Incident>()).Returns(x => x.Arg<Incident>());
        _stepFunctions.SendTaskSuccessAsync(Arg.Any<SendTaskSuccessRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new AmazonStepFunctionsException("boom"));

        var response = await _function.FunctionHandler(Request(new
        {
            IncidentId = incidentId,
            ExamId = examId,
            Score = 65,
            TaskToken = Token
        }), _context);

        Assert.Equal(500, response.StatusCode);
    }
}
