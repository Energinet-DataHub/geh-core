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
using ExampleHost.Tests.Extensions;
using ExampleHost.Tests.Fixtures;
using FluentAssertions;
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
        /// TODO: Add to documentation
        /// - "Configure categories" lists different logging categories that can be useful to understand [https://docs.microsoft.com/en-us/azure/azure-functions/configure-monitoring?tabs=v2#configure-categories].
        /// </summary>
        [Fact]
        public async Task CreatePet_flow_should_succeed()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/pet");

            // Act
            var ingestionResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request)
                .ConfigureAwait(false);
            //// TODO: Fix!
            ////var correlationId = ingestionResponse.Headers.TryGetValues("CorrelationId", out var values)
            ////    ? values.FirstOrDefault()
            ////    : null;

            // Assert
            //// TODO: Fix!
            ////Fixture.TestLogger.WriteLine(correlationId ?? string.Empty);
            ingestionResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

            await AssertFunctionExecuted(Fixture.App01HostManager, "CreatePetAsync").ConfigureAwait(false);

            AssertNoExceptionsThrown();

            // Wait so tracing is sent to Application Insights before we close host's.
            await Task.Delay(TimeSpan.FromSeconds(30));
        }

        private static async Task AssertFunctionExecuted(FunctionAppHostManager hostManager, string functionName)
        {
            var waitTimespan = TimeSpan.FromSeconds(1000);

            var functionExecuted = await Awaiter
                .TryWaitUntilConditionAsync(
                    () => hostManager.CheckIfFunctionWasExecuted(
                        $"Functions.{functionName}"),
                    waitTimespan)
                .ConfigureAwait(false);
            functionExecuted.Should().BeTrue($"{functionName} was expected to run.");
        }

        private void AssertNoExceptionsThrown()
        {
            Fixture.App01HostManager.CheckIfFunctionThrewException().Should().BeFalse();
        }
    }
}
