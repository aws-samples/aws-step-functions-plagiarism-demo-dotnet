using System;
using System.Threading.Tasks;
using Plagiarism;

namespace PlagiarismRepository
{
    public interface IIncidentRepository
    {
        Task<Incident> SaveIncident(Incident incident);
        Task<Incident> GetIncidentById(Guid incidentId);
    }
}