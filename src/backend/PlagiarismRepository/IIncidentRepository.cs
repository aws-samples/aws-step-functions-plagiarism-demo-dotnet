// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using System.Threading.Tasks;
using Plagiarism;

namespace PlagiarismRepository;

public interface IIncidentRepository
{
    /// <summary>
    /// Saves incident
    /// </summary>
    /// <param name="incident">Incident instance</param>
    /// <returns>Incident instance saved to the table</returns>
    Task<Incident> SaveIncidentAsync(Incident incident);

    /// <summary>
    /// Gets incident by id
    /// </summary>
    /// <param name="incidentId">Incident Id</param>
    /// <returns>Incident instance</returns>
    /// <exception cref="IncidentNotFoundException">Thrown when no incident exists with the given id</exception>
    Task<Incident> GetIncidentByIdAsync(Guid incidentId);
}
