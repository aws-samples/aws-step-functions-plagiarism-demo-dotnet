using System;
using System.Threading.Tasks;
using IncidentState;

namespace IncidentPersistence
{
    public interface IIncidentRepository
    {
        Task<State> SaveIncident(State state);
        Task<State> GetIncidentById(Guid incidentId);
    }
}