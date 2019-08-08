using System;
using System.Collections.Generic;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Amazon.StepFunctions;
using IncidentState;
using Newtonsoft.Json;
using SubmitExamTask;
using Xunit;
using Xunit.Abstractions;

namespace ValidateExamTask.Tests
{
    public class FunctionTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IAmazonDynamoDB _dynamoDbClient;
        
        public FunctionTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _dynamoDbClient = new AmazonDynamoDBClient(RegionEndpoint.APSoutheast2);
        }

        [Fact]
        public void TestResults()
        {
            var state = new State
            {
                IncidentId = Guid.NewGuid(),
                StudentId = "123",
                IncidentDate = new DateTime(2018, 02, 03),
                Exams = new List<Exam>()
                {
                    new Exam(Guid.NewGuid(), new DateTime(2018, 02, 10), 99),
                    new Exam(Guid.NewGuid(), new DateTime(2018, 02, 10), 25),
                    new Exam(Guid.NewGuid(), new DateTime(2018, 02, 10), 0)
                }
            };
            
            Assert.True(state.Exams[0].Result == ExamResult.Pass, "Result should be pass");
            Assert.True(state.Exams[1].Result == ExamResult.Fail, "Result should be fail");
            Assert.True(state.Exams[2].Result == ExamResult.DidNotSitExam, "Result should be did not sit");
        }

        [Fact]
        public void TestValidateExamFunctionHandler()
        {
            var request = new APIGatewayProxyRequest();
            var body= new Dictionary<string, string>
            {
                {"eid", "dc149d4b-ce6d-435a-b922-9da90f7c3eed"},
                {"iid", "6b44fd97-1af3-42f6-9a0b-0138fffa8cf4"},
                {"score", "65" },
                {
                    "tt",
                    "AAAAKgAAAAIAAAAAAAAAAQdie+0BRsJTasJDmB3SEAFgUrGkrPgmXgbyknPligwbY5BzLBHDSnLqqerB5B5r0V8V1813I1iHhGd" +
                    "mBD5pYXORPRsx86sNgN+Pa7I/1ANeyArkZf6n1qjGJvR0D+8rdhEnssXkWpAEC8MBA1VHH7zGhJILvG5SfaI8PseNWtKS2TuKj3" +
                    "o5qCdOmDdkGBS8er5mUIG5biDp07KTRAHCOqqjxRkVgCZBMWI6WFSDNkV1r7kqellXj+BjV1K/8DBrvTIbyi6vX4uYuXPOnW8xU" +
                    "4DKwf5prx+wMyb/hWLV0JKl273ggAjvWlWlRDUenPR0dKT20Gw4THkcZRL3ytJ3j1mJRu4Jdu0JQGMzcYqH6Q0t8DZfqezkFXMW" +
                    "ahu7977xKeRNmgkW1qUgGDdyT4tbT8w+s57QoByyQvUKrfLseW+WK2jBICFWEnyt6wI9ISTOQ+lQh+pZFBnfXMvyraw1zqlyXfFu" +
                    "DtPv+r8N/gotVnRe+Q1RtAz2JQf9gJNWH0UdNZtFfOe4vcT03U6t/CgDzTFdaByr8js8pj+BQlfzkD0MmRbbhvK6yhmgZiMP76Jx" +
                    "LsqYf3BLKbk61gvLaNgeJAKzBUF0KA/FATq9gsYh1JSufuiHUokjn18agp4SwEoGFp7kddgS0niFwQesrxieyug="
                }
            };
            request.Body = JsonConvert.SerializeObject(body);
            
            var context = new TestLambdaContext();
            
            var expectedResponse = new APIGatewayProxyResponse
            {
                Body = JsonConvert.SerializeObject("OK"),
                StatusCode = 200,
                Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
            };

            var function = new Function(_dynamoDbClient, new AmazonStepFunctionsClient(RegionEndpoint.APSoutheast1), "IncidentsTestTable-637008741220204940" );
            var response = function.FunctionHandler(request, context);

            _testOutputHelper.WriteLine("Lambda Response: \n" + response.Body);
            _testOutputHelper.WriteLine("Expected Response: \n" + expectedResponse.Body);

            Assert.Equal(expectedResponse.Body, response.Body);
            Assert.Equal(expectedResponse.Headers, response.Headers);
            Assert.Equal(expectedResponse.StatusCode, response.StatusCode);
        }
    }
}