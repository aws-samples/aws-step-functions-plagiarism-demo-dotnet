using System;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Plagiarism;
using PlagiarismRepository;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace ScheduleExamTask
{
    public class Function
    {
        private readonly IIncidentRepository _incidentRepository;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Function()
        {
            AWSSDKHandler.RegisterXRayForAllServices();
            _incidentRepository = new IncidentRepository(Environment.GetEnvironmentVariable("TABLE_NAME"));
        }
        

        /// <summary>
        /// Constructor used for testing purposes
        /// </summary>
        /// <param name="ddbClient">Instance of DynamoDB client</param>
        /// <param name="tablename">DynamoDB table name</param>
        public Function(IAmazonDynamoDB ddbClient, string tablename)
        {
            AWSSDKHandler.RegisterXRayForAllServices();
            _incidentRepository = new IncidentRepository(ddbClient, tablename);
        }

        /// <summary>
        /// Function to schedule the next exam fr the student.
        /// Student cannot take more than 3 exams so throw customer exception
        /// if this business rule is met.
        /// </summary>
        /// <param name="incident">Incident State object</param>
        /// <param name="context">Lambda Context</param>
        /// <returns></returns>
        public Incident FunctionHandler(Incident incident, ILambdaContext context)
        {
            var exam = new Exam(Guid.NewGuid(), DateTime.Now.AddSeconds(10), 0);

            if (incident.Exams != null && incident.Exams.Count >= 3)
            {
                throw new StudentExceededAllowableExamRetries("Student cannot take more that 3 exams.");
            }

            // Always add latest exam to the top of the list so we can reference it in the state-machine definition.
            incident.Exams?.Insert(0, exam);

            _incidentRepository.SaveIncident(incident);

            return incident;
        }
    }
}