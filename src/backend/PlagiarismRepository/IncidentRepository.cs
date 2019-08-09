using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Plagiarism;

namespace PlagiarismRepository
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
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(Incident)] = new Amazon.Util.TypeMapping(typeof(Incident), tableName);
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
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(Incident)] = new Amazon.Util.TypeMapping(typeof(Incident), tableName);
            }
            
            var config = new DynamoDBContextConfig {Conversion = DynamoDBEntryConversion.V2};
            _dynamoDbContext = new DynamoDBContext(ddbClient, config);
        }

        public async Task<Incident> GetIncidentById(Guid incidentId)
        {
            Console.WriteLine($"Getting blog {incidentId}");
            var state = await _dynamoDbContext.LoadAsync<Incident>(incidentId);
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
        /// <param name="incident"></param>
        /// <returns>Instance of State </returns>
        public async Task<Incident> SaveIncident(Incident incident)
        {
            try
            {
                Console.WriteLine($"Saving blog with id {incident.IncidentId}");
                await _dynamoDbContext.SaveAsync(incident);
                return incident;
            }
            catch (AmazonDynamoDBException e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
    }
}