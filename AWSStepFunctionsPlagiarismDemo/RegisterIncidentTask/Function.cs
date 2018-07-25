using System;
using System.Collections.Generic;
using Amazon.Lambda.Core;
using IncidentState;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace RegisterIncidentTask
{
    public class Function
    {
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="state"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public State FunctionHandler(State state, ILambdaContext context)
        {

            state.IncidentId = Guid.NewGuid();
            state.Exams = new List<Exam>();
            state.IncidentResolved = false;

            return state;

        }
    }
}
