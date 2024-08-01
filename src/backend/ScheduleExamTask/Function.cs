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

namespace ScheduleExamTask;

public class Function
{
    private readonly IIncidentRepository _incidentRepository;

    /// <summary>
    /// Default Constructor
    /// </summary>
    public Function()
    {
        Tracing.RegisterForAllServices();
        _incidentRepository = new IncidentRepository(Environment.GetEnvironmentVariable("TABLE_NAME"));
    }


    /// <summary>
    /// Constructor used for testing purposes
    /// </summary>
    /// <param name="ddbClient">Instance of DynamoDB client</param>
    /// <param name="tableName">DynamoDB table name</param>
    public Function(AmazonDynamoDBClient ddbClient, string tableName)
    {
        Tracing.RegisterForAllServices();
        _incidentRepository = new IncidentRepository(ddbClient, tableName);
    }

    public Function(IIncidentRepository incidentRepository)
    {
        _incidentRepository = incidentRepository;
    }

    /// <summary>
    /// Function to schedule the next exam fr the student.
    /// Student cannot take more than 3 exams so throw customer exception
    /// if this business rule is met.
    /// </summary>
    /// <param name="incident">Incident State object</param>
    /// <param name="context">Lambda Context</param>
    /// <returns></returns>
    [Logging(LogEvent = true)]
    [Tracing(CaptureMode = TracingCaptureMode.ResponseAndError)]
    [Metrics(CaptureColdStart = true)]
    public Incident FunctionHandler(Incident incident, ILambdaContext context)
    {
        
        Logger.LogInformation("Scheduling exam for incident {incidentData.IncidentId}", incident.IncidentId);
        
        if (incident.Exams != null && incident.Exams.Count >= 3)
        {
            Logger.LogInformation($"Student has already completed 3 exams for incident {incident.IncidentId}");
            throw new StudentExceededAllowableExamRetries("Student cannot take more that 3 exams.");
        }
        
        incident.Exams ??= new List<Exam>();
        
        // Always add latest exam to the top of the list so we can reference it in the state-machine definition.
        incident.Exams.Insert(0, new Exam(Guid.NewGuid(), DateTime.Now.AddDays(7), 0));

        _incidentRepository.SaveIncident(incident);
        Logger.LogInformation("Exam for incident {incidentData.IncidentId} scheduled.", incident.IncidentId);
        return incident;
    }
}