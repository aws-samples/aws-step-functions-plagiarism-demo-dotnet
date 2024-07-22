// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using System.Collections.Generic;
using Amazon.Lambda.Core;
using Plagiarism;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using PlagiarismRepository;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RegisterIncidentTask;

public class Function
{
    private readonly IIncidentRepository _incidentRepository;

    public Function()
    {
        Tracing.RegisterForAllServices();
        _incidentRepository = new IncidentRepository(Environment.GetEnvironmentVariable("TABLE_NAME"));
    }

    /// <summary>
    /// The register incident function simply creates a new incident and returned to the
    /// workflow as state data 
    /// </summary>
    /// <param name="incident">Instance of <see cref="Incident"/></param>
    /// <param name="context">Instance of AWS Lambda context. See <see cref="ILambdaContext"/></param>
    /// <returns></returns>
    [Logging(LogEvent = true)]
    [Tracing(CaptureMode = TracingCaptureMode.ResponseAndError)]
    [Metrics(CaptureColdStart = true)]
    public Incident FunctionHandler(Incident incident, ILambdaContext context)
    {
        if (string.IsNullOrEmpty(incident.StudentId))
        {
            throw new ArgumentException("StudentId cannot be empty.");
        }

        if (incident.IncidentDate == DateTime.MinValue)
        {
            incident.IncidentDate = DateTime.Now;
        }
        
        incident.Exams = new List<Exam>();
        incident.IncidentResolved = false;

        _incidentRepository.SaveIncident(incident);
        
        Logger.LogInformation("Registering new incident: {incident}", incident);
        return incident;
    }
}