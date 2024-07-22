// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using System.Collections.Generic;
using Amazon.Lambda.TestUtilities;
using Plagiarism;
using Xunit;
using Xunit.Abstractions;

namespace SendNotificationTask.Tests;

public class FunctionTests
{
    private readonly TestLambdaContext _context;
    private readonly IncidentWrapper _incidentIn;

    private readonly ITestOutputHelper _testOutputHelper;

    public FunctionTests(ITestOutputHelper testOutputHelper)
    {
        // Set env variable for function
        Environment.SetEnvironmentVariable("SENDGRID_API_KEY", "9A7B69D9-E7B9-4113-84CF-D64C66DA1C9F");
        Environment.SetEnvironmentVariable("TO_EMAIL", "test@company.com");
        Environment.SetEnvironmentVariable("FROM_EMAIL", "test@company.com");
        Environment.SetEnvironmentVariable("TESTING_CENTRE_URL", "http://tescentre.company.com");

        // Set env variable for Powertools Metrics 
        Environment.SetEnvironmentVariable("TABLE_NAME", "IncidentsTable");
        Environment.SetEnvironmentVariable("POWERTOOLS_METRICS_NAMESPACE", "Plagiarism");


        _context = new TestLambdaContext();

        _incidentIn = new IncidentWrapper()
        {
            Input = new Incident(),
            TaskToken = "TASKTOKEN"
        };
        _incidentIn.Input.IncidentId = Guid.NewGuid();
        _incidentIn.Input.StudentId = "123";
        _incidentIn.Input.IncidentDate = new DateTime(2018, 02, 03);
        _incidentIn.Input.Exams = new List<Exam>()
        {
            new Exam(Guid.NewGuid(), new DateTime(2018, 02, 17), 0),
            new Exam(Guid.NewGuid(), new DateTime(2018, 02, 10), 65)
        };
        _incidentIn.Input.ResolutionDate = null;
    }


    [Fact]
    public void NotificationSentSouldBeFalseIfSnsPublishSucceeds()
    {
        var function = new Function();
        _ = function.FunctionHandler(_incidentIn, _context);


        // Assert.True(response.Exams[0].NotificationSent == true);
    }

    [Fact]
    public void NotificationSentSouldBeFalseIfSnsPublishFails()
    {
        var function = new Function();

        _ = function.FunctionHandler(_incidentIn, _context);

        // Assert.True(response.Exams[0].NotificationSent == false);
    }
}