using System;
using Amazon.DynamoDBv2.DocumentModel;
using IncidentState;

namespace IncidentPersistence
{
    public static class IncidentDocument
    {
        public static Document BuildIncidentDocument(State state)
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
