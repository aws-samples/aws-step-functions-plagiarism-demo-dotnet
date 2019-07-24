using System;
using Amazon.Lambda.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using IncidentPersistence;
using IncidentState;

// Assembly attribute to enable the Lambda function's JSON state to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace ResolveIncidentTask
{
    public class Function
    {
        private readonly IIncidentRepository _incidentRepository;

        public Function()
        {
            AWSSDKHandler.RegisterXRayForAllServices();
            _incidentRepository = new IncidentRepository(Environment.GetEnvironmentVariable("TABLE_NAME"));
        }

        public Function(IIncidentRepository incidentRepository)
        {
            _incidentRepository = incidentRepository;
        }

        /// <summary>
        /// Function to resolve the incident and cpmplete the workflow.
        /// All state data is persisted.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public void FunctionHandler(State state, ILambdaContext context)
        {
            state.AdminActionRequired = false;
            state.IncidentResolved = true;
            state.ResolutionDate = DateTime.Now;

            _incidentRepository.SaveIncident(state);
        }
    }


    
}