using System;
using Amazon.Lambda.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using IncidentState;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace ScheduleExamTask
{
    public class Function
    {

        public Function()
        {
            AWSSDKHandler.RegisterXRayForAllServices();
        }

        /// <summary>
        /// Function to schedule the next exam fr the student.
        /// Student cannot take more than 3 exams so throw customer exception
        /// if this business rule is met.
        /// </summary>
        /// <param name="state">Incident State object</param>
        /// <param name="context">Lambda Context</param>
        /// <returns></returns>
        public State FunctionHandler(State state, ILambdaContext context)
        {
            var exam = new Exam(Guid.NewGuid(), DateTime.Now.AddSeconds(10), 0);

            if (state.Exams != null && state.Exams.Count >= 3)
            {
                throw new StudentExceededAllowableExamRetries("Student cannot take more that 3 exams.");
            }

            // Always add latest exam to the top of the list so we can reference it in the state-machine definition.
            state.Exams?.Insert(0, exam);

            return state;
        }
    }
}
