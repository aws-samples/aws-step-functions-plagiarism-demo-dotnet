using System;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using IncidentPersistence;
using IncidentState;

// Assembly attribute to enable the Lambda function's JSON state to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AdminActionTask
{
    public class Function
    {
        private readonly AmazonDynamoDBClient _client;
        private readonly string _table_name;

        public Function()
        {
            AWSSDKHandler.RegisterXRayForAllServices();
            _client = new AmazonDynamoDBClient(RegionEndpoint.APSoutheast2);
            _table_name = Environment.GetEnvironmentVariable("TABLE_NAME");
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

            Document incidentDocument = IncidentDocument.BuildDynamoDbDocument(state);
            
            try
            {
                Console.WriteLine("");
                var table = Table.LoadTable(_client, _table_name);
                table.PutItemAsync(incidentDocument);
            }
            catch (AmazonDynamoDBException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            catch (AmazonServiceException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

       
    }
}
