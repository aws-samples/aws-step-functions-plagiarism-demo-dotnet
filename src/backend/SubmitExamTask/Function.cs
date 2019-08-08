using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using IncidentPersistence;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SubmitExamTask
{
    public class Function
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly AmazonStepFunctionsClient _amazonStepFunctionsClient;

        public Function()
        {
            AWSSDKHandler.RegisterXRayForAllServices();
            _incidentRepository = new IncidentRepository(Environment.GetEnvironmentVariable("TABLE_NAME"));
            _amazonStepFunctionsClient = new AmazonStepFunctionsClient();
        }

        
        public Function(IAmazonDynamoDB ddbClient, IAmazonStepFunctions stepFunctions, string tablename)
        {
            AWSSDKHandler.RegisterXRayForAllServices();
            _incidentRepository = new IncidentRepository(ddbClient, tablename);
            _amazonStepFunctionsClient = (AmazonStepFunctionsClient) stepFunctions;
        }

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="request">API Gateway Request object</param>
        /// <param name="context"></param>
        /// <returns></returns>
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var body = JsonConvert.DeserializeObject<Dictionary<string, string>>(request?.Body);
            var incidentId = Guid.Parse(body["iid"]);
            var examId = Guid.Parse(body["eid"]);
            var score = Convert.ToInt32(body["score"]);
            var token = body["tt"];

            var incident = _incidentRepository.GetIncidentById(incidentId).Result;
            var exam = incident.Exams.Find(e => e.ExamId == examId);
            exam.Score = score;

            _incidentRepository.SaveIncident(incident);

            _amazonStepFunctionsClient.SendTaskSuccessAsync(new SendTaskSuccessRequest
            {
                TaskToken = token, 
                Output = JsonConvert.SerializeObject(incident)
            });

            return new APIGatewayProxyResponse
            {
                Body = JsonConvert.SerializeObject("OK"),
                StatusCode = 200,
                Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
            };
        }
    }
}