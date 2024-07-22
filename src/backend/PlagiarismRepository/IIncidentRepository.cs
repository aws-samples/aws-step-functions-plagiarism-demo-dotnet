// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using Plagiarism;

namespace PlagiarismRepository;

public interface IIncidentRepository
{
    /// <summary>
    /// Saves incident
    /// </summary>
    /// <param name="incident">Incident instance</param>
    /// <returns>Incident instance saved to the table</returns>
    Incident SaveIncident(Incident incident);

    /// <summary>
    /// Gets incident by id
    /// </summary>
    /// <param name="incidentId">Incident Id</param>
    /// <returns>Incident instance</returns>
    Incident GetIncidentById(Guid incidentId);
}