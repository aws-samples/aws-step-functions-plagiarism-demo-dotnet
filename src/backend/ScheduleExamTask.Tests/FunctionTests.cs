// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using System.Collections.Generic;
using Xunit;
using Amazon.Lambda.TestUtilities;
using NSubstitute;
using Plagiarism;
using PlagiarismRepository;
using Xunit.Abstractions;


namespace ScheduleExamTask.Tests;

public class FunctionTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public FunctionTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        // Set env variable for Powertools Metrics 
        Environment.SetEnvironmentVariable("TABLE_NAME", "IncidentsTable");
        Environment.SetEnvironmentVariable("POWERTOOLS_METRICS_NAMESPACE", "Plagiarism");
    }

    [Fact]
    public void Returns_Exam()
    {
        var mockIncidentRepository
            = Substitute.For<IIncidentRepository>();


        var function = new Function(mockIncidentRepository);
        var context = new TestLambdaContext();

        var state = new Incident();
        state.IncidentId = Guid.NewGuid();
        state.StudentId = "123";
        state.IncidentDate = new DateTime(2018, 02, 03);
        
        
        // Call function with mock repository
        var response = function.FunctionHandler(state, context);
        
        Assert.NotNull(response.Exams);
        Assert.Single(response.Exams);
    }
    
    [Fact]
    public void Returns_two_exams()
    {
        var mockIncidentRepository
            = Substitute.For<IIncidentRepository>();


        var function = new Function(mockIncidentRepository);
        var context = new TestLambdaContext();

        var state = new Incident();
        state.IncidentId = Guid.NewGuid();
        state.StudentId = "123";
        state.IncidentDate = new DateTime(2018, 02, 03);
        state.Exams = new List<Exam>
        {
            new Exam() { ExamId = Guid.NewGuid(), ExamDeadline = DateTime.Now, Score = 50, NotificationSent = true }
        };
        
        
        // Call function with mock repository
        var response = function.FunctionHandler(state, context);
        
        Assert.NotNull(response.Exams);
        Assert.Equal(2, response.Exams.Count);
    }
    
}