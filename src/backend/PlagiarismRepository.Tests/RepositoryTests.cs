// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Plagiarism;
using Xunit;
using Xunit.Abstractions;

namespace PlagiarismRepository.Tests;

public class RepositoryTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private const string TablePrefix = "IncidentsTestTable-";
    private string _tableName;
    private readonly AmazonDynamoDBClient _dynamoDbClient;
    private IIncidentRepository _incidentRepository;


    public RepositoryTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        
        // dynamodb client using DynamoDB local
        var dynamoDbConfig = new AmazonDynamoDBConfig
        {
            ServiceURL = "http://localhost:8000", 
            Timeout = new TimeSpan(0,0,5), 
        };
        _dynamoDbClient = new AmazonDynamoDBClient(dynamoDbConfig);
        EnsureDockerContainerRunning();
        SetupTableAsync().Wait();
    }


    private void EnsureDockerContainerRunning()
    {
        var retryCount = 0;
        const int maxRetries = 3; // Reduced from 5
        const int retryDelayMs = 1000; // Reduced from 2000

        while (retryCount < maxRetries)
        {
            try
            {
                // Attempt to list tables to check if DynamoDB is responsive
                _dynamoDbClient.ListTablesAsync().Wait(TimeSpan.FromSeconds(2)); // Added timeout
                _testOutputHelper.WriteLine("DynamoDB Local container is running.");
                return;
            }
            catch (Exception ex)
            {
                _testOutputHelper.WriteLine($"Attempt {retryCount + 1}: DynamoDB Local container is not ready. Retrying in {retryDelayMs}ms...");
                _testOutputHelper.WriteLine($"Error: {ex.Message}");
                Thread.Sleep(retryDelayMs);
                retryCount++;
            }
        }

        throw new Exception("Failed to connect to DynamoDB Local after multiple attempts. Ensure the Docker container is running.");

    }

    [Fact]
    public async Task SaveIncidentAsync()
    {
        _incidentRepository = new IncidentRepository(_dynamoDbClient, _tableName);

        var newIncident = new Incident
        {
            IncidentId = Guid.NewGuid(),
            StudentId = "123",
            IncidentDate = new DateTime(2018, 02, 03),
            ResolutionDate = null
        };

        var incident = await _incidentRepository.SaveIncidentAsync(newIncident);

        Assert.NotNull(incident);
    }

    [Fact]
    public async Task UpdateIncidentAsync()
    {
        _incidentRepository = new IncidentRepository(_dynamoDbClient, _tableName);

        var state = new Incident
        {
            IncidentId = Guid.NewGuid(),
            StudentId = "123",
            IncidentDate = new DateTime(2018, 02, 03),
            ResolutionDate = null
        };

        var incident = await _incidentRepository.SaveIncidentAsync(state);

        var updatedIncident = await _incidentRepository.SaveIncidentAsync(state);

        Assert.NotNull(incident);
        Assert.NotNull(updatedIncident);
    }


    [Fact]
    public async Task FindIncidentAsync()
    {
        _incidentRepository = new IncidentRepository(_dynamoDbClient, _tableName);

        var incidentDate = DateTime.Now;

        var state = new Incident
        {
            IncidentId = Guid.NewGuid(),
            StudentId = "123",
            IncidentDate = incidentDate,
            ResolutionDate = DateTime.Now
        };

        var incident = await _incidentRepository.SaveIncidentAsync(state);

        var newIncident = await _incidentRepository.GetIncidentByIdAsync(incident.IncidentId);

        Assert.NotNull(newIncident);
        Assert.True(newIncident.IncidentId == incident.IncidentId, "Should be the same incident");
    }

    /// <summary>
    /// Helper function to create a testing table
    /// </summary>
    /// <returns></returns>
    private async Task SetupTableAsync()
    {
        var listTablesResponse = await _dynamoDbClient.ListTablesAsync(new ListTablesRequest());

        var existingTestTable =
            listTablesResponse.TableNames.FindAll(s => s.StartsWith(TablePrefix)).FirstOrDefault();

        if (existingTestTable == null)
        {
            _tableName = TablePrefix + DateTime.Now.Ticks;

            var request = new CreateTableRequest
            {
                TableName = _tableName,
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 2,
                    WriteCapacityUnits = 2
                },
                KeySchema =
                [
                    new KeySchemaElement
                    {
                        AttributeName = "IncidentId",
                        KeyType = KeyType.HASH
                    }
                ],
                AttributeDefinitions =
                [
                    new AttributeDefinition
                    {
                        AttributeName = "IncidentId",
                        AttributeType = ScalarAttributeType.S
                    }
                ]
            };

            try
            {
                await _dynamoDbClient.CreateTableAsync(request);
            }
            catch (Exception e)
            {
                _testOutputHelper.WriteLine(e.Message);
                throw;
            }

            var describeRequest = new DescribeTableRequest { TableName = _tableName };
            DescribeTableResponse response;

            do
            {
                Thread.Sleep(1000);
                response = await _dynamoDbClient.DescribeTableAsync(describeRequest);
            } while (response.Table.TableStatus != TableStatus.ACTIVE);
        }
        else
        {
            _testOutputHelper.WriteLine($"Using existing test table {existingTestTable}");
            _tableName = existingTestTable;
        }
    }
}