using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using IncidentState;

namespace IncidentPersistence
{
    public class IncidentRepository : IIncidentRepository
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly string _tableName;

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="tableName">DynamoDb table name</param>
        public IncidentRepository(string tableName)
        {
            if (!string.IsNullOrEmpty(tableName))
            {     
                _tableName = tableName;
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(State)] = new Amazon.Util.TypeMapping(typeof(State), tableName);
            }
            
            var config = new DynamoDBContextConfig {Conversion = DynamoDBEntryConversion.V2};
            _dynamoDbContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        /// <summary>
        /// Constructor used for testing passing in a preconfigured DynamoDB client.
        /// </summary>
        /// <param name="ddbClient"></param>
        /// <param name="tableName"></param>
        public IncidentRepository(IAmazonDynamoDB ddbClient, string tableName)
        {
            if (!string.IsNullOrEmpty(tableName))
            {  
                _tableName = tableName;
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(State)] = new Amazon.Util.TypeMapping(typeof(State), tableName);
            }
            
            var config = new DynamoDBContextConfig {Conversion = DynamoDBEntryConversion.V2};
            _dynamoDbContext = new DynamoDBContext(ddbClient, config);
        }

        public async Task<State> GetIncidentById(Guid incidentId)
        {
            Console.WriteLine($"Getting blog {incidentId}");
            var state = await _dynamoDbContext.LoadAsync<State>(incidentId);
            Console.WriteLine($"Found blog: {state != null}");

            if (state == null)
            {
                throw new IncidentNotFoundException($"Could not locate {incidentId} in table {_tableName}");
            }
            
            return state;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <returns>Instance of State </returns>
        public async Task<State> SaveIncident(State state)
        {
            try
            {
                Console.WriteLine($"Saving blog with id {state.IncidentId}");
                await _dynamoDbContext.SaveAsync(state);
                return state;
            }
            catch (AmazonDynamoDBException e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
    }
}