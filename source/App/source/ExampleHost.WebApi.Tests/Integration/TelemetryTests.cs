﻿// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Net;
using Azure.Monitor.Query;
using Energinet.DataHub.Core.TestCommon;
using ExampleHost.WebApi.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace ExampleHost.WebApi.Tests.Integration;

/// <summary>
/// Tests that documents and proves how we should setup and configure our
/// Asp.Net Core Web Api's (host's) so they log expected telemetry events.
/// </summary>
[Collection(nameof(ExampleHostCollectionFixture))]
public class TelemetryTests
{
    public TelemetryTests(ExampleHostFixture fixture)
    {
        Fixture = fixture;
    }

    private ExampleHostFixture Fixture { get; }

    /// <summary>
    /// Verify both host's can run and WebHost01 can call WebHost02.
    /// </summary>
    [Fact]
    public async Task CallingApi01Get_Should_CallApi02Get()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi01/telemetry/{requestIdentification}");
        var actualResponse = await Fixture.Web01HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Contain(requestIdentification);
    }

    /// <summary>
    /// Verifies:
    ///  * Logging using ILogger{T} will work, and we have configured the default level
    ///    for logging to Application Insights as "Information" in the test fixture.
    ///  * We can see Trace, Request, Dependencies and other entries in Application Insights.
    ///  * Telemetry events are enriched with the property "Subsystem".
    ///
    /// Requirements for this test:
    ///
    /// 1: Both hosts must call "ConfigureServices" with the following:
    /// <code>
    ///     services.AddApplicationInsightsTelemetry();
    /// </code>
    ///
    /// 2: And configure "Logging:ApplicationInsights:LogLevel:Default" to "Information"; otherwise default level is "Warning".
    /// </summary>
    [Fact]
    public async Task Configuration_Should_CauseExpectedEventsToBeLogged()
    {
        var requestIdentification = Guid.NewGuid().ToString();

        var expectedEvents = new List<QueryResult>
        {
            new() { Type = "AppDependencies", Subsystem = "ExampleHost.WebApi", Name = $"GET /webapi01/telemetry/{requestIdentification}", DependencyType = "HTTP" },
            new() { Type = "AppRequests", Subsystem = "ExampleHost.WebApi", Name = "GET Telemetry/Get [identification]", Url = $"http://localhost:5000/webapi01/telemetry/{requestIdentification}" },
            new() { Type = "AppTraces", Subsystem = "ExampleHost.WebApi", EventName = null!, Message = $"ExampleHost WebApi01 {requestIdentification} Information: We should be able to find this log message by following the trace of the request" },
            new() { Type = "AppTraces", Subsystem = "ExampleHost.WebApi", EventName = null!, Message = $"ExampleHost WebApi01 {requestIdentification} Warning: We should be able to find this log message by following the trace of the request" },

            new() { Type = "AppDependencies", Subsystem = "ExampleHost.WebApi", Name = $"GET /webapi02/telemetry/{requestIdentification}", DependencyType = "HTTP" },
            new() { Type = "AppRequests", Subsystem = "ExampleHost.WebApi", Name = "GET Telemetry/Get [identification]", Url = $"http://localhost:5001/webapi02/telemetry/{requestIdentification}" },
            new() { Type = "AppTraces", Subsystem = "ExampleHost.WebApi", EventName = null!, Message = $"ExampleHost WebApi02 {requestIdentification} Information: We should be able to find this log message by following the trace of the request" },
            new() { Type = "AppTraces", Subsystem = "ExampleHost.WebApi", EventName = null!, Message = $"ExampleHost WebApi02 {requestIdentification} Warning: We should be able to find this log message by following the trace of the request" },
        };

        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi01/telemetry/{requestIdentification}");
        await Fixture.Web01HttpClient.SendAsync(request);

        var queryWithParameters = @"
                let OperationIds = AppRequests
                | where AppRoleInstance == '{{$Environment.MachineName}}'
                | where Url contains '{{requestIdentification}}'
                | project OperationId;
                OperationIds
                | join(union AppRequests, AppDependencies, AppTraces) on OperationId
                | extend parsedProp = parse_json(Properties)
                | project TimeGenerated, OperationId, ParentId, Id, Type, Name, DependencyType, Subsystem=parsedProp.Subsystem, EventName=parsedProp.EventName, Message, Url, Properties
                | order by TimeGenerated asc";

        var query = queryWithParameters
            .Replace("{{$Environment.MachineName}}", Environment.MachineName)
            .Replace("{{requestIdentification}}", requestIdentification)
            .Replace("\n", string.Empty);

        var queryTimeRange = new QueryTimeRange(TimeSpan.FromMinutes(20));
        var waitLimit = TimeSpan.FromMinutes(20);
        var delay = TimeSpan.FromSeconds(50);

        await Task.Delay(delay);

        var actualCount = 0;
        var wasEventsLogged = await Awaiter
            .TryWaitUntilConditionAsync(
                async () =>
                {
                    var actualResponse = await Fixture.LogsQueryClient.QueryWorkspaceAsync<QueryResult>(
                        Fixture.LogAnalyticsWorkspaceId,
                        query,
                        queryTimeRange);

                    actualCount = actualResponse.Value.Count;
                    return ContainsExpectedEvents(expectedEvents, actualResponse.Value);
                },
                waitLimit,
                delay);

        wasEventsLogged.Should().BeTrue($"'Was expected to log {expectedEvents.Count} number of events, but found {actualCount}.'");
    }

    private bool ContainsExpectedEvents(IList<QueryResult> expectedEvents, IReadOnlyList<QueryResult> actualResults)
    {
        if (actualResults.Count < expectedEvents.Count)
        {
            return false;
        }

        foreach (var expected in expectedEvents)
        {
            switch (expected.Type)
            {
                case "AppRequests":
                    actualResults.First(actual =>
                        actual.Subsystem == expected.Subsystem
                        && actual.Name == expected.Name
                        && actual.Url == expected.Url);
                    break;

                case "AppDependencies":
                    actualResults.First(actual =>
                        actual.Subsystem == expected.Subsystem
                        && actual.Name == expected.Name
                        && actual.DependencyType == expected.DependencyType);
                    break;

                // "AppTraces"
                default:
                    actualResults.First(actual =>
                        actual.Subsystem == expected.Subsystem
                        && actual.EventName == expected.EventName
                        && actual.Message.StartsWith(expected.Message));
                    break;
            }
        }

        return true;
    }

    private class QueryResult
    {
        public string TimeGenerated { get; set; }
            = string.Empty;

        public string OperationId { get; set; }
            = string.Empty;

        public string ParentId { get; set; }
            = string.Empty;

        public string Id { get; set; }
            = string.Empty;

        public string Type { get; set; }
            = string.Empty;

        public string Name { get; set; }
            = string.Empty;

        public string DependencyType { get; set; }
            = string.Empty;

        public string Subsystem { get; set; }
            = string.Empty;

        public string EventName { get; set; }
            = string.Empty;

        public string Message { get; set; }
            = string.Empty;

        public string Url { get; set; }
            = string.Empty;

        public string Properties { get; set; }
            = string.Empty;
    }
}
