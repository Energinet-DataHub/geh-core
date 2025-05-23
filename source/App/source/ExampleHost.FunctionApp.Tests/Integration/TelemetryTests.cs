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
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.TestCommon;
using ExampleHost.FunctionApp.Tests.Fixtures;
using ExampleHost.FunctionApp01.Functions;
using ExampleHost.FunctionApp02.Functions;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace ExampleHost.FunctionApp.Tests.Integration;

/// <summary>
/// Tests that documents and prooves how we should setup and configure our
/// Azure Function App's (host's) so they log expected telemetry events.
/// </summary>
[Collection(nameof(ExampleHostsCollectionFixture))]
public class TelemetryTests : IAsyncLifetime
{
    public TelemetryTests(ExampleHostsFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.SetTestOutputHelper(testOutputHelper);

        Fixture.App01HostManager.ClearHostLog();
        Fixture.App02HostManager.ClearHostLog();
    }

    private ExampleHostsFixture Fixture { get; }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Fixture.SetTestOutputHelper(null!);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Used for testing various values set in `traceparent` http header.
    /// See https://www.w3.org/TR/trace-context/#trace-context-http-headers-format
    /// </summary>
    public static IEnumerable<object[]> TraceParentParameters()
    {
        yield return new object[]
        {
            TraceParentTestData.Empty,
        };

        yield return new object[]
        {
            new TraceParentTestData
            {
                 Version = "00",
                 TraceId = Guid.NewGuid().ToString("N"),
                 ParentId = string.Format("{0:x16}", new Random().Next(0x1000000)),
                 TraceFlags = "01",
            },
        };
    }

    /// <summary>
    /// Verify both host's can run and FunctionApp01 can call FunctionApp02.
    /// </summary>
    [Fact]
    public async Task CallingTelemetryAsync_Should_CallReceiveMessage()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/telemetry");
        var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

        actualResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

        await Fixture.App01HostManager.AssertFunctionWasExecutedAsync("TelemetryAsync");
        await Fixture.App02HostManager.AssertFunctionWasExecutedAsync("ReceiveMessage");

        using var assertionScope = new AssertionScope();
        Fixture.App01HostManager.CheckIfFunctionThrewException().Should().BeFalse();
        Fixture.App02HostManager.CheckIfFunctionThrewException().Should().BeFalse();
    }

    /// <summary>
    /// Requirements for this test:
    ///  * <see cref="RestApiExampleFunction"/> must use <see cref="ILogger{RestApiExampleFunction}"/>.
    ///  * <see cref="IntegrationEventExampleFunction"/> must use <see cref="ILoggerFactory"/>.
    /// </summary>
    [Fact]
    public async Task ILoggerAndILoggerFactory_Should_BeRegisteredByDefault()
    {
        const string ExpectedLogMessage = "We should be able to find this log message by following the trace of the request.";

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/telemetry");
        await Fixture.App01HostManager.HttpClient.SendAsync(request);

        await Fixture.App01HostManager.AssertFunctionWasExecutedAsync("TelemetryAsync");
        await Fixture.App02HostManager.AssertFunctionWasExecutedAsync("ReceiveMessage");

        Fixture.App01HostManager.GetHostLogSnapshot()
            .First(log => log.Contains(ExpectedLogMessage, StringComparison.OrdinalIgnoreCase));
        Fixture.App02HostManager.GetHostLogSnapshot()
            .First(log => log.Contains(ExpectedLogMessage, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifies:
    ///  * Logging using ILogger{T} will work, and we have configured the default level
    ///    for logging to Application Insights as "Information" in the test fixture.
    ///  * We can see Trace, Request, Dependencies and other entries in Application Insights.
    ///  * Only logs emitted from the isolated worker (not the host) are enriched with the property "Subsystem".
    ///
    /// Requirements for this test:
    ///
    /// 1: Both hosts must call "ConfigureServices" with the following:
    /// <code>
    ///     services.AddApplicationInsightsForIsolatedWorker(subsystemName: "MySubsystem");
    /// </code>
    ///
    /// 2: And call "ConfigureLogging" with the following:
    /// <code>
    ///     logging.AddLoggingConfigurationForIsolatedWorker(hostingContext.Configuration);
    /// </code>
    ///
    /// 3: And configure "Logging:ApplicationInsights:LogLevel:Default" to "Information"; otherwise default level is "Warning".
    /// </summary>
    [Theory]
    [MemberData(nameof(TraceParentParameters))]
    public async Task Configuration_Should_CauseExpectedEventsToBeLogged(TraceParentTestData traceParentTestData)
    {
        var expectedEvents = new List<QueryResult>
        {
            new() { Type = "AppRequests", Name = "TelemetryAsync" },
            new() { Type = "AppTraces", EventName = "FunctionStarted", Subsystem = null!, Message = "Executing 'Functions.TelemetryAsync'" },
            new() { Type = "AppDependencies", Subsystem = "ExampleHost.FunctionApp", Name = "Invoke", DependencyType = "InProc" },
            new() { Type = "AppTraces", EventName = null!, Subsystem = "ExampleHost.FunctionApp", Message = "ExampleHost TelemetryAsync: We should be able to find this log message by following the trace of the request." },
            new() { Type = "AppDependencies", Subsystem = "ExampleHost.FunctionApp", Name = "Message", DependencyType = "Queue Message | Azure Service Bus" },
            new() { Type = "AppDependencies", Subsystem = "ExampleHost.FunctionApp", Name = "ServiceBusSender.Send", DependencyType = "Azure Service Bus" },

            new() { Type = "AppRequests", Name = "ReceiveMessage" },
            new() { Type = "AppTraces", EventName = "FunctionCompleted", Subsystem = null!, Message = "Executed 'Functions.TelemetryAsync' (Succeeded" },
            new() { Type = "AppTraces", EventName = "FunctionStarted", Subsystem = null!, Message = "Executing 'Functions.ReceiveMessage'" },
            new() { Type = "AppTraces", EventName = null!, Subsystem = null!, Message = "Trigger Details" },
            new() { Type = "AppDependencies", Subsystem = "ExampleHost.FunctionApp", Name = "Invoke", DependencyType = "InProc" },
            new() { Type = "AppTraces", EventName = null!, Subsystem = "ExampleHost.FunctionApp", Message = "ExampleHost ReceiveMessage: We should be able to find this log message by following the trace of the request." },
            new() { Type = "AppTraces", EventName = "FunctionCompleted", Subsystem = null!, Message = "Executed 'Functions.ReceiveMessage' (Succeeded" },
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/telemetry");
        if (traceParentTestData != TraceParentTestData.Empty)
        {
            var traceParent = $"{traceParentTestData.Version}-{traceParentTestData.TraceId}-{traceParentTestData.ParentId}-{traceParentTestData.TraceFlags}";
            request.Headers.Add("traceparent", traceParent);
        }

        await Fixture.App01HostManager.HttpClient.SendAsync(request);

        await Fixture.App01HostManager.AssertFunctionWasExecutedAsync("TelemetryAsync");
        await Fixture.App02HostManager.AssertFunctionWasExecutedAsync("ReceiveMessage");

        var telemetryInvocationId = GetFunctionsInvocationId(Fixture.App01HostManager, "TelemetryAsync");
        var receiveMessageInvocationId = GetFunctionsInvocationId(Fixture.App02HostManager, "ReceiveMessage");

        var queryWithParameters = @"
                let OperationIds = AppRequests
                  | where AppRoleInstance == '{{$Environment.MachineName}}'
                  | extend parsedProp = parse_json(Properties)
                  | where parsedProp.InvocationId == '{{$telemetryInvocationId}}' or parsedProp.InvocationId == '{{$receiveMessageInvocationId}}'
                  | project OperationId;
                OperationIds
                  | join(union AppRequests, AppDependencies, AppTraces) on OperationId
                  | extend parsedProp = parse_json(Properties)
                  | project TimeGenerated, OperationId, ParentId, Id, Type, Name, DependencyType, Subsystem=parsedProp.Subsystem, EventName=parsedProp.EventName, Message, Properties
                  | order by TimeGenerated asc";

        var query = queryWithParameters
            .Replace("{{$Environment.MachineName}}", Environment.MachineName)
            .Replace("{{$telemetryInvocationId}}", telemetryInvocationId)
            .Replace("{{$receiveMessageInvocationId}}", receiveMessageInvocationId)
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
                    return ContainsExpectedEvents(expectedEvents, actualResponse.Value, traceParentTestData);
                },
                waitLimit,
                delay);

        wasEventsLogged.Should().BeTrue($"'Was expected to log {expectedEvents.Count} number of events, but found {actualCount}. See log output for details.'");
    }

    private bool ContainsExpectedEvents(IList<QueryResult> expectedEvents, IReadOnlyList<QueryResult> actualResults, TraceParentTestData traceParentTestData)
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
                    var appRequestsExists = actualResults.Any(actual =>
                        actual.Name == expected.Name);

                    if (!appRequestsExists)
                    {
                        Fixture.TestLogger.WriteLine($"Did not find expected AppRequests: Name='{expected.Name}'");
                        return false;
                    }

                    break;

                case "AppDependencies":
                    var appDependenciesExists = actualResults.Any(actual =>
                        actual.Subsystem == expected.Subsystem
                        && actual.Name == expected.Name
                        && actual.DependencyType == expected.DependencyType);

                    if (!appDependenciesExists)
                    {
                        Fixture.TestLogger.WriteLine($"Did not find expected AppDependencies: Name='{expected.Name}' DependencyType='{expected.DependencyType}'");
                        return false;
                    }

                    break;

                // "AppTraces"
                default:
                    var appTracesExists = actualResults.Any(actual =>
                        actual.Subsystem == expected.Subsystem
                        && actual.EventName == expected.EventName
                        && actual.Message.StartsWith(expected.Message));

                    if (!appTracesExists)
                    {
                        Fixture.TestLogger.WriteLine($"Did not find expected AppTrace: EventName='{expected.EventName}' Message='{expected.Message}'");
                        return false;
                    }

                    break;
            }
        }

        if (traceParentTestData == TraceParentTestData.Empty)
        {
            return true;
        }

        // If we added ´traceparent´ header while requesting, the Trace Id and Parent Id will be set for the first activity.
        return actualResults.Any(actual =>
            actual.OperationId == traceParentTestData.TraceId
            && actual.ParentId == traceParentTestData.ParentId);
    }

    private static string GetFunctionsInvocationId(FunctionAppHostManager hostManager, string functionName)
    {
        var executedStatement = hostManager.GetHostLogSnapshot()
            .First(log => log.Contains($"Executed 'Functions.{functionName}'", StringComparison.OrdinalIgnoreCase));

        return executedStatement.Substring(executedStatement.IndexOf('=') + 1, 36);
    }

    public record TraceParentTestData
    {
        public static TraceParentTestData Empty { get; }
            = new TraceParentTestData();

        public string Version { get; set; }
            = string.Empty;

        public string TraceId { get; set; }
            = string.Empty;

        public string ParentId { get; set; }
            = string.Empty;

        public string TraceFlags { get; set; }
            = string.Empty;
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

        public string Properties { get; set; }
            = string.Empty;
    }
}
