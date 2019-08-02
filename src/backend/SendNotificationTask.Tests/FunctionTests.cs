using System;
using System.Collections.Generic;
using Amazon.Lambda.TestUtilities;
using IncidentState;
using Xunit;

namespace SendNotificationTask.Tests
{
    public class FunctionTests
    {
        private readonly TestLambdaContext _context;
        private readonly StateWrapper _stateIn;

        public FunctionTests()
        {
            _context = new TestLambdaContext();

            _stateIn = new StateWrapper()
            {
                Input = new State
                {
                    IncidentId = Guid.NewGuid(),
                    StudentId = "123",
                    IncidentDate = new DateTime(2018, 02, 03),
                    Exams = new List<Exam>()
                    {
                        new Exam(Guid.NewGuid(), new DateTime(2018, 02, 17), 0),
                        new Exam(Guid.NewGuid(), new DateTime(2018, 02, 10), 65)
                    },
                    ResolutionDate = null
                },
                TaskToken = "TASKTOKEN"
            };
        }


        [Fact]
        public void NotificationSentSouldBeFalseIfSnsPublishSucceeds()
        {
            var function = new Function();
            function.FunctionHandler(_stateIn, _context);

            //Assert.True(response.Exams[0].NotificationSent == true);
        }

        [Fact]
        public void NotificationSentSouldBeFalseIfSnsPublishFails()
        {

            var function = new Function();

            function.FunctionHandler(_stateIn, _context);

             // Assert.True(response.Exams[0].NotificationSent == false);
        }
    }
}