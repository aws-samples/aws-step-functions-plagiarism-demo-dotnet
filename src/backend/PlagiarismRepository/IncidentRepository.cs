// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Plagiarism;
using AWS.Lambda.Powertools.Logging;

namespace PlagiarismRepository;

public class IncidentRepository : IIncidentRepository
{
    private readonly DynamoDBContext _dynamoDbContext;
    private readonly string _tableName;

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="tableName">DynamoDb table name</param>
    public IncidentRepository(string tableName)
    {
        if (!string.IsNullOrEmpty(tableName))
        {
            _tableName = tableName;
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(Incident)] =
                new Amazon.Util.TypeMapping(typeof(Incident), tableName);
        }

        _dynamoDbContext = new DynamoDBContextBuilder()
            .WithDynamoDBClient(() => new AmazonDynamoDBClient())
            .ConfigureContext(config => config.Conversion = DynamoDBEntryConversion.V2)
            .Build();
    }

    /// <summary>
    /// Constructor used for testing passing in a preconfigured DynamoDB client.
    /// </summary>
    /// <param name="ddbClient"></param>
    /// <param name="tableName"></param>
    public IncidentRepository(AmazonDynamoDBClient ddbClient, string tableName)
    {
        if (!string.IsNullOrEmpty(tableName))
        {
            _tableName = tableName;
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(Incident)] =
                new Amazon.Util.TypeMapping(typeof(Incident), tableName);
        }

        _dynamoDbContext = new DynamoDBContextBuilder()
            .WithDynamoDBClient(() => ddbClient)
            .ConfigureContext(config => config.Conversion = DynamoDBEntryConversion.V2)
            .Build();
    }

    public async Task<Incident> GetIncidentByIdAsync(Guid incidentId)
    {
        Logger.LogInformation("Getting {IncidentId}", incidentId);
        var incident = await _dynamoDbContext.LoadAsync<Incident>(incidentId);
        Logger.LogInformation($"Found Incident: {incident != null}");

        if (incident == null)
        {
            throw new IncidentNotFoundException($"Could not locate {incidentId} in table {_tableName}");
        }

        return incident;
    }

    /// <summary>
    /// Saves or updates an incident.
    /// </summary>
    /// <param name="incident"></param>
    /// <returns>The saved incident</returns>
    public async Task<Incident> SaveIncidentAsync(Incident incident)
    {
        try
        {
            Logger.LogInformation($"Saving incident with id {incident.IncidentId}");

            await _dynamoDbContext.SaveAsync(incident);
            return incident;
        }
        catch (AmazonDynamoDBException e)
        {
            Logger.LogError(e);
            throw;
        }
    }
}