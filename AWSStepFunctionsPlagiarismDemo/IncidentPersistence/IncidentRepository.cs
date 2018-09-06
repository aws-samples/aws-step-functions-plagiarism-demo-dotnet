using System;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;
using IncidentState;

namespace IncidentPersistence
{
    
    public interface IIncidentRepository
    {
        void SaveIncident(State state);
    }

    public class IncidentRepository : IIncidentRepository
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly string _tableName;

        public IncidentRepository(string tableName)
        {
            _dynamoDb = new AmazonDynamoDBClient();
            _tableName = tableName;
        }

        public void SaveIncident(State state)
        {
            var incidentDocument = new IncidentDocument().CreateDocumentFromState(state);

            try
            {
                Console.WriteLine("Saving incident:{0} to {1}", state.IncidentId.ToString("N"), _tableName);
                var table = Table.LoadTable(_dynamoDb, _tableName);
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
   
    internal class IncidentDocument
    {
        internal Document CreateDocumentFromState(State state)
        {
            var examsList = new DynamoDBList();

            foreach (var exam in state.Exams)
            {
                var examMap = new Document
                {
                    {"ExamId", exam.ExamId},
                    {"ExamDate", exam.ExamDate},
                    {"Score", exam.Score}
                };

                examsList.Add(examMap);
            }

            var incidentDocument = new Document
            {
                ["IncidentId"] = state.IncidentId,
                ["StudentId"] = state.StudentId,
                ["IncidentDate"] = state.IncidentDate,
                ["AdminActionRequired"] = new DynamoDBBool(state.AdminActionRequired),
                ["IncidentResolved"] = new DynamoDBBool(state.IncidentResolved),
                ["ResolutionDate"] = state.ResolutionDate,
                ["Exams"] = examsList
            };

            return incidentDocument;
        }

    }
}
