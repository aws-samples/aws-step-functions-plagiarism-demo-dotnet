using System;
using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.TestUtilities;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using IncidentState;
using NSubstitute;
using Xunit;

namespace SendNotificationTask.Tests
{
    public class FunctionTests
    {
        private readonly TestLambdaContext _context;
        private readonly State _stateIn;

        public FunctionTests()
        {
            _context = new TestLambdaContext();
            
            _stateIn = new State
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
            };
            
        }
        
        
        [Fact]
        public void NotificationSentSouldBeFalseIfSnsPublishSucceeds()
        {
            var simpleNotificationService
                = Substitute.For<IAmazonSimpleNotificationService>();

            var function = new Function(simpleNotificationService);


            simpleNotificationService.PublishAsync(Arg.Any<PublishRequest>()).Returns(new PublishResponse()
            {
                MessageId = Guid.NewGuid().ToString("N"), 
                ContentLength = 0, 
                HttpStatusCode = HttpStatusCode.OK, 
                ResponseMetadata = null
            });

            var response = function.FunctionHandler(_stateIn, _context);

            Assert.True(response.Exams[0].NotifcationSent == true);

        }
        
        [Fact]
        public void NotificationSentSouldBeFalseIfSnsPublishFails()
        {
            var simpleNotificationService
                = Substitute.For<IAmazonSimpleNotificationService>();
            
            var function = new Function(simpleNotificationService);
            
            simpleNotificationService.PublishAsync(Arg.Any<PublishRequest>()).Returns(new PublishResponse()
            {
                MessageId = Guid.NewGuid().ToString("N"), 
                ContentLength = 0, 
                HttpStatusCode = HttpStatusCode.BadRequest, 
                ResponseMetadata = null
            });

            var response = function.FunctionHandler(_stateIn, _context);

            Assert.True(response.Exams[0].NotifcationSent == false);

        }
    }
}