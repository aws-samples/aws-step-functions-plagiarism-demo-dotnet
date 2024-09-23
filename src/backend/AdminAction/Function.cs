// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using Plagiarism;
using PlagiarismRepository;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AdminAction;

public class Function
{
    private readonly IIncidentRepository _incidentRepository;

    public Function()
    {
        var tableName = Environment.GetEnvironmentVariable("TABLE_NAME");
        _incidentRepository = new IncidentRepository(tableName);
    }

    public Function(IIncidentRepository incidentRepository)
    {
        _incidentRepository = incidentRepository;
        Tracing.RegisterForAllServices();
    }

    /// <summary>
    /// Indicates that the Incident needs administrative action
    /// </summary>
    /// <param name="incident">Instance of Incident class</param>
    /// <param name="context">AWS Lambda context</param>
    /// <returns></returns>
    [Logging(LogEvent = true)]
    [Tracing(CaptureMode = TracingCaptureMode.ResponseAndError)]
    [Metrics(CaptureColdStart = true)]
    public void FunctionHandler(Incident incident, ILambdaContext context)
    {
        incident.AdminActionRequired = true;
        incident.IncidentResolved = false;
        incident.ResolutionDate = DateTime.Now;

        SaveIncident(incident);
    }

    [Tracing(SegmentName = "Save incident")]
    protected virtual void SaveIncident(Incident incident)
    {
        _incidentRepository.SaveIncident(incident);
    }
}