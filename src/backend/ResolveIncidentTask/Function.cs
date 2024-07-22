// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using Amazon.Lambda.Core;
using Plagiarism;
using PlagiarismRepository;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ResolveIncidentTask;

public class Function
{
    private readonly IIncidentRepository _incidentRepository;

    /// <summary>
    /// Constructs a new instance with the specified repository.
    /// </summary>
    /// <param name="repository"></param>
    /// <exception cref="ArgumentNullException">The repository parameter cannot be null.</exception>
    /// </summary>
    public Function()
    {
        Tracing.RegisterForAllServices();
        _incidentRepository = new IncidentRepository(Environment.GetEnvironmentVariable("TABLE_NAME"));
    }

    /// <summary>
    /// Constructor used for testing purposes
    /// </summary>
    /// <param name="repository"></param>
    public Function(IIncidentRepository repository)
    {
        if (repository == null)
        {
            throw new ArgumentNullException(nameof(repository));
        }

        Tracing.RegisterForAllServices();
        _incidentRepository = repository;
    }

    /// <summary>
    /// Function to resolve the incident and complete the workflow.
    /// All state data is persisted.
    /// </summary>
    /// <param name="incident">Instance of an Incident</param>
    /// <param name="context">AWS Lambda context</param>
    /// <returns></returns>
    [Logging(LogEvent = true)]
    [Tracing(CaptureMode = TracingCaptureMode.ResponseAndError)]
    [Metrics(CaptureColdStart = true)]
    public void FunctionHandler(Incident incident, ILambdaContext context)
    {
        SaveIncident(incident);
    }

    [Tracing(SegmentName = "Saving incident")]
    private void SaveIncident(Incident incident)
    {
        incident.AdminActionRequired = false;
        incident.IncidentResolved = true;
        incident.ResolutionDate = DateTime.Now;

        _incidentRepository.SaveIncident(incident);
    }
}