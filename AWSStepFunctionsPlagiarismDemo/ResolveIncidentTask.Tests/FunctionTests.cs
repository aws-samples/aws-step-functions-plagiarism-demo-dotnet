using System;
using System.Collections.Generic;
using Xunit;
using Amazon.Lambda.TestUtilities;
using IncidentPersistence;
using IncidentState;
using NSubstitute;

namespace ResolveIncidentTask.Tests
{
    public class FunctionTests
    {
        public FunctionTests()
        {
        }

        [Fact]
        public void ResolveIncidentFunctionTest()
        {
            var incidentRepository
                = Substitute.For<IIncidentRepository>();


            var function = new Function(incidentRepository);
            var context = new TestLambdaContext();

            var state = new State
            {
                IncidentId = Guid.NewGuid(),
                StudentId = "123",
                IncidentDate = new DateTime(2018, 02, 03),
                Exams = new List<Exam>()
                {
                    new Exam(Guid.NewGuid(), new DateTime(2018, 02, 10), 10),
                    new Exam(Guid.NewGuid(), new DateTime(2018, 02, 17), 65)
                },
                ResolutionDate = null
            };


            incidentRepository.SaveIncident(state);
            incidentRepository.ReceivedCalls();

            function.FunctionHandler(state, context);
        }
    }
}