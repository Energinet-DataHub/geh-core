// Copyright 2020 Energinet DataHub A/S
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
using Azure;
using Azure.Monitor.Query;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.TestCommon;
using ExampleHost.FunctionApp01.Functions;
using ExampleHost.FunctionApp02.Functions;
using ExampleHost.Tests.Extensions;
using ExampleHost.Tests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace ExampleHost.Tests.Integration
{
    /// <summary>
    /// Tests that documents and prooves how we should setup and configure our
    /// Azure Function App's (host's) so they behave as we expect.
    /// </summary>
    [Collection(nameof(ExampleHostCollectionFixture))]
    public class ExampleHostTests : IAsyncLifetime
    {
        public ExampleHostTests(ExampleHostFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.SetTestOutputHelper(testOutputHelper);
        }

        private ExampleHostFixture Fixture { get; }

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
        /// Verify sunshine scenario.
        /// </summary>
        [Fact]
        public async Task CallingCreatePetAsync_Should_CallReceiveMessage()
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/pet");
            var ingestionResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

            ingestionResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

            await AssertFunctionExecuted(Fixture.App01HostManager, "CreatePetAsync");
            await AssertFunctionExecuted(Fixture.App02HostManager, "ReceiveMessage");

            AssertNoExceptionsThrown();
        }

        /// <summary>
        /// Requirements for this test:
        ///  * <see cref="RestApiExampleFunction"/> must use <see cref="ILogger{RestApiExampleFunction}"/>.
        ///  * <see cref="IntegrationEventExampleFunction"/> must use <see cref="ILoggerFactory"/>.
        /// </summary>
        [Fact]
        public async Task IloggerAndILoggerFactory_Should_BeRegisteredByDefault()
        {
            const string ExpectedLogMessage = "We should be able to find this log message by following the trace of the request.";

            using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/pet");
            var ingestionResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

            await AssertFunctionExecuted(Fixture.App01HostManager, "CreatePetAsync");
            await AssertFunctionExecuted(Fixture.App02HostManager, "ReceiveMessage");

            Fixture.App01HostManager.GetHostLogSnapshot()
                .First(log => log.Contains(ExpectedLogMessage, StringComparison.OrdinalIgnoreCase));
            Fixture.App02HostManager.GetHostLogSnapshot()
                .First(log => log.Contains(ExpectedLogMessage, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Requirements for this test:
        ///
        /// 1: Both hosts must call "ConfigureFunctionsWorkerDefaults" with the following:
        /// <code>
        ///     builder.UseMiddleware{CorrelationIdMiddleware}();
        ///     builder.UseMiddleware{FunctionTelemetryScopeMiddleware}();
        /// </code>
        ///
        /// 2: Both hosts must call "ConfigureServices" with the following:
        /// <code>
        ///     services.AddApplicationInsightsTelemetryWorkerService();
        ///     services.AddScoped{ICorrelationContext, CorrelationContext}();
        ///     services.AddScoped{CorrelationIdMiddleware}();
        ///     services.AddScoped{FunctionTelemetryScopeMiddleware}();
        /// </code>
        /// </summary>
        [Fact]
        public async Task Middleware_Should_CauseExpectedEventsToBeLogged()
        {
            const int ExpectedEventsCount = 13;

            using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/pet");
            var ingestionResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

            await AssertFunctionExecuted(Fixture.App01HostManager, "CreatePetAsync");
            await AssertFunctionExecuted(Fixture.App02HostManager, "ReceiveMessage");

            var createPetInvocationId = GetFunctionsInvocationId(Fixture.App01HostManager, "CreatePetAsync");
            var receiveMessageInvocationId = GetFunctionsInvocationId(Fixture.App02HostManager, "ReceiveMessage");

            var queryWithParameters = @"
                let OperationIds = AppRequests
                  | where AppRoleInstance == '{{$Environment.MachineName}}'
                  | extend parsedProp = parse_json(Properties)
                  | where parsedProp.InvocationId == '{{$createPetInvocationId}}' or parsedProp.InvocationId == '{{$receiveMessageInvocationId}}'
                  | project OperationId;
                OperationIds
                  | join(union AppRequests, AppDependencies, AppTraces) on OperationId
                  | extend parsedProp = parse_json(Properties)
                  | project TimeGenerated, OperationId, Id, Type, Name, DependencyType, EventName=parsedProp.EventName, Message, Properties
                  | order by TimeGenerated asc";

            var query = queryWithParameters
                .Replace("{{$Environment.MachineName}}", Environment.MachineName)
                .Replace("{{$createPetInvocationId}}", createPetInvocationId)
                .Replace("{{$receiveMessageInvocationId}}", receiveMessageInvocationId)
                .Replace("\n", string.Empty);

            var queryTimerange = new QueryTimeRange(TimeSpan.FromMinutes(10));
            var waitLimit = TimeSpan.FromMinutes(5);
            var delay = TimeSpan.FromSeconds(30);

            await Task.Delay(delay);

            var wasEventsLogged = await Awaiter
                .TryWaitUntilConditionAsync(
                    async () =>
                    {
                        var response = await Fixture.LogsQueryClient.QueryWorkspaceAsync<QueryResult>(
                            Fixture.LogAnalyticsWorkspaceId,
                            query,
                            queryTimerange);

                        return ContainsExpectedEvents(ExpectedEventsCount, response.Value);
                    },
                    waitLimit,
                    delay);

            wasEventsLogged.Should().BeTrue($"Was expected to log {ExpectedEventsCount} number of events.");
        }

        private bool ContainsExpectedEvents(int expectedEventsCount, IReadOnlyList<QueryResult> queryResults)
        {
            if (queryResults.Count != expectedEventsCount)
            {
                return false;
            }

            return
                queryResults.Count(x => x.Type == "AppRequests") == 2
                && queryResults.Count(x => x.Type == "AppDependencies") == 4
                && queryResults.Count(x => x.Type == "AppTraces") == 7;
        }

        private static async Task AssertFunctionExecuted(FunctionAppHostManager hostManager, string functionName)
        {
            var waitTimespan = TimeSpan.FromSeconds(30);

            var functionExecuted = await Awaiter
                .TryWaitUntilConditionAsync(
                    () => hostManager.CheckIfFunctionWasExecuted(
                        $"Functions.{functionName}"),
                    waitTimespan);
            functionExecuted.Should().BeTrue($"{functionName} was expected to run.");
        }

        private static string GetFunctionsInvocationId(FunctionAppHostManager hostManager, string functionName)
        {
            var executedStatement = hostManager.GetHostLogSnapshot()
                .First(log => log.Contains($"Executed 'Functions.{functionName}'", StringComparison.OrdinalIgnoreCase));

            return executedStatement.Substring(executedStatement.IndexOf('=') + 1, 36);
        }

        private void AssertNoExceptionsThrown()
        {
            Fixture.App01HostManager.CheckIfFunctionThrewException().Should().BeFalse();
        }

        private class QueryResult
        {
            public string TimeGenerated { get; set; }
                = string.Empty;

            public string OperationId { get; set; }
                = string.Empty;

            public string Id { get; set; }
                = string.Empty;

            public string Type { get; set; }
                = string.Empty;

            public string Name { get; set; }
                = string.Empty;

            public string DependencyType { get; set; }
                = string.Empty;

            public string EventName { get; set; }
                = string.Empty;

            public string Message { get; set; }
                = string.Empty;

            public string Properties { get; set; }
                = string.Empty;
        }
    }
}
