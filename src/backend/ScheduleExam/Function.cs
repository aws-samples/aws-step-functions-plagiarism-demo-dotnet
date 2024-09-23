// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Tracing;
using Plagiarism;
using PlagiarismRepository;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ScheduleExam;

public class Function
{
    private readonly IIncidentRepository _incidentRepository;

    public Function() : this(new IncidentRepository(Environment.GetEnvironmentVariable("TABLE_NAME")))
    {
    }

    public Function(AmazonDynamoDBClient ddbClient, string tableName)
        : this(new IncidentRepository(ddbClient, tableName))
    {
    }

    public Function(IIncidentRepository incidentRepository)
    {
        Tracing.RegisterForAllServices();
        _incidentRepository = incidentRepository;
    }

    [Logging(LogEvent = true)]
    [Tracing(CaptureMode = TracingCaptureMode.ResponseAndError)]
    [Metrics(CaptureColdStart = true)]
    public Incident FunctionHandler(Incident incident, ILambdaContext context)
    {
        ValidateIncident(incident);

        var existingIncident = _incidentRepository.GetIncidentById(incident.IncidentId)
            ?? throw new InvalidOperationException($"Incident with ID {incident.IncidentId} not found.");

        Logger.LogInformation("Scheduling exam for incident {IncidentId}", existingIncident.IncidentId);

        if (existingIncident.Exams?.Count >= 3)
        {
            Logger.LogInformation("Student has already completed 3 exams for incident {IncidentId}", existingIncident.IncidentId);
            throw new StudentExceededAllowableExamRetries("Student cannot take more than 3 exams.");
        }

        existingIncident.Exams ??= new List<Exam>();
        existingIncident.Exams.Insert(0, new Exam(Guid.NewGuid(), DateTime.Now.AddDays(7), 0));

        _incidentRepository.SaveIncident(existingIncident);
        Logger.LogInformation("Exam for incident {IncidentId} scheduled.", existingIncident.IncidentId);
        return existingIncident;
    }

    private static void ValidateIncident(Incident incident)
    {
        if (incident == null)
        {
            throw new ArgumentNullException(nameof(incident), "Incident cannot be null.");
        }

        if (incident.IncidentId == Guid.Empty)
        {
            throw new ArgumentException("IncidentId cannot be null or empty.", nameof(incident));
        }
    }
}