using System;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using IncidentPersistence;
using IncidentState;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON state to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AdminActionTask
{
    public class Function
    {

        private readonly IIncidentRepository _incidentRepository;

        public Function()
        {
            var tableName = Environment.GetEnvironmentVariable("TABLE_NAME");
            _incidentRepository = new IncidentRepository(tableName);
        }

        public Function(IIncidentRepository incidentRepository)
        {
            _incidentRepository = incidentRepository;
            AWSSDKHandler.RegisterXRayForAllServices();
        }

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="state"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public void FunctionHandler(State state, ILambdaContext context)
        {
            state.AdminActionRequired = true;
            state.IncidentResolved = false;
            state.ResolutionDate = DateTime.Now;
            
            _incidentRepository.SaveIncident(state);
            
        }

       
    }
}
