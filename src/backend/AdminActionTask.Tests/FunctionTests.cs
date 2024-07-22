// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Xunit;
using Amazon.Lambda.TestUtilities;
using NSubstitute;
using Plagiarism;
using PlagiarismRepository;
using Xunit.Abstractions;

namespace AdminActionTask.Tests;

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
    public void ResolveIncidentFunctionTest()
    {
        var mockIncidentRepository
            = Substitute.For<IIncidentRepository>();


        var function = new Function(mockIncidentRepository);
        var context = new TestLambdaContext();

        var state = new Incident();
        state.IncidentId = Guid.NewGuid();
        state.StudentId = "123";
        state.IncidentDate = new DateTime(2018, 02, 03);
        state.Exams = new List<Exam>()
        {
            new Exam(Guid.NewGuid(), new DateTime(2018, 02, 10), 10),
            new Exam(Guid.NewGuid(), new DateTime(2018, 02, 17), 65)
        };
        state.ResolutionDate = null;

        // Call function with mock repository
        function.FunctionHandler(state, context);

        // assert the call to incident repository had state with Resolution date not set to null
        mockIncidentRepository.Received().SaveIncident(Arg.Is<Incident>(i => i.ResolutionDate != null));
        mockIncidentRepository.Received().SaveIncident(Arg.Is<Incident>(i => i.IncidentResolved == false));
        mockIncidentRepository.Received().SaveIncident(Arg.Is<Incident>(i => i.AdminActionRequired == true));

        _testOutputHelper.WriteLine("Success");
    }
}