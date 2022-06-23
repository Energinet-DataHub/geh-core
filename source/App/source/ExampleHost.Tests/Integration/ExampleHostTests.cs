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
            ingestionResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

            await AssertFunctionExecuted(Fixture.App01HostManager, "CreatePetAsync");
            await AssertFunctionExecuted(Fixture.App02HostManager, "ReceiveMessage");

            AssertNoExceptionsThrown();

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
        public async Task CallingCreatePetAsync_Should_CallReceiveMessage()
        {
            for (var i = 0; i < 10; i++)
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/pet");
                var ingestionResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);
                //// TODO: Fix!
                ////var correlationId = ingestionResponse.Headers.TryGetValues("CorrelationId", out var values)
                ////    ? values.FirstOrDefault()
                ////    : null;

                // Assert
                //// TODO: Fix!
                ////Fixture.TestLogger.WriteLine(correlationId ?? string.Empty);
                ingestionResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

                await AssertFunctionExecuted(Fixture.App01HostManager, "CreatePetAsync");
                await AssertFunctionExecuted(Fixture.App02HostManager, "ReceiveMessage");

                AssertNoExceptionsThrown();

                await Task.Delay(TimeSpan.FromSeconds(3));
            }

            // TODO: Use library to query Log Analytics: https://docs.microsoft.com/en-us/dotnet/api/overview/azure/monitor.query-readme

            // Wait so tracing is sent to Application Insights before we close host's.
            await Task.Delay(TimeSpan.FromSeconds(60));
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

        private void AssertNoExceptionsThrown()
        {
            Fixture.App01HostManager.CheckIfFunctionThrewException().Should().BeFalse();
        }
    }
}
