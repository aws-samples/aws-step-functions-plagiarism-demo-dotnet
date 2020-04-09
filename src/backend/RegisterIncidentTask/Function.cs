using System;
using System.Collections.Generic;
using Amazon.Lambda.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Plagiarism;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.LambdaJsonSerializer))]

namespace RegisterIncidentTask
{
    public class Function
    {
        public Function()
        {
            AWSSDKHandler.RegisterXRayForAllServices();
        }
        
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="incident"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public Incident FunctionHandler(Incident incident, ILambdaContext context)
        {

            incident.IncidentId = Guid.NewGuid();
            incident.Exams = new List<Exam>();
            incident.IncidentResolved = false;

            return incident;

        }
    }
}
