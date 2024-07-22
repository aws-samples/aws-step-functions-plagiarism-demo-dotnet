// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Amazon.Lambda.TestUtilities;
using Plagiarism;
using Xunit.Abstractions;

namespace RegisterIncidentTask.Tests;

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
    public void RegisterIncident_throws_argument_exception_if_student_id_is_missing()
    {
        var function = new Function();
        var context = new TestLambdaContext();

        var state = new Incident();
        state.IncidentId = Guid.NewGuid();
        state.StudentId = null;

        // assert the call to incident repository has
        Assert.Throws<ArgumentException>(() => function.FunctionHandler(state, context));

        _testOutputHelper.WriteLine("Success");
    }
}