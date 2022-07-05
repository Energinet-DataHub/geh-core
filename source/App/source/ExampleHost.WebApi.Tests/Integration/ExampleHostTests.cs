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
using Azure.Monitor.Query;
using Energinet.DataHub.Core.TestCommon;
using ExampleHost.WebApi.Tests.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace ExampleHost.WebApi.Tests.Integration
{
    /// <summary>
    /// Tests that documents and prooves how we should setup and configure our
    /// Asp.Net Core Web Api's (host's) so they behave as we expect.
    /// </summary>
    [Collection(nameof(ExampleHostCollectionFixture))]
    public class ExampleHostTests
    {
        public ExampleHostTests(ExampleHostFixture fixture)
        {
            Fixture = fixture;
        }

        private ExampleHostFixture Fixture { get; }

        /// <summary>
        /// Verify sunshine scenario.
        /// </summary>
        [Fact]
        public async Task CallingApi01Get_Should_CallApi02Get()
        {
            // Arrange
            var requestIdentification = Guid.NewGuid().ToString();

            // Act
            using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi01/weatherforecast/{requestIdentification}");
            var actualResponse = await Fixture.Web01HttpClient.SendAsync(request);

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await actualResponse.Content.ReadAsStringAsync();
            content.Should().Contain("\"temperatureC\":");
        }

        [Fact]
        public async Task Configuration_Should_CauseExpectedEventsToBeLogged()
        {
            var requestIdentification = Guid.NewGuid().ToString();

            var expectedEvents = new List<QueryResult>
            {
                new QueryResult { Type = "AppDependencies", Name = $"GET /webapi01/weatherforecast/{requestIdentification}", DependencyType = "HTTP" },
                new QueryResult { Type = "AppRequests", Name = "GET WeatherForecast/Get [identification]" },
                new QueryResult { Type = "AppTraces", EventName = null!, Message = $"ExampleHost WebApi01 {requestIdentification}: We should be able to find this log message by following the trace of the request." },
            };

            using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi01/weatherforecast/{requestIdentification}");
            await Fixture.Web01HttpClient.SendAsync(request);

            var queryWithParameters = @"
                let OperationIds = AppRequests
                | where AppRoleInstance == '{{$Environment.MachineName}}'
                | where Url contains '{{requestIdentification}}'
                | project OperationId;
                OperationIds
                | join(union AppRequests, AppDependencies, AppTraces) on OperationId
                | extend parsedProp = parse_json(Properties)
                | project TimeGenerated, OperationId, Id, Type, Name, DependencyType, EventName=parsedProp.EventName, Message, Properties
                | order by TimeGenerated asc";

            var query = queryWithParameters
                .Replace("{{$Environment.MachineName}}", Environment.MachineName)
                .Replace("{{requestIdentification}}", requestIdentification)
                .Replace("\n", string.Empty);

            var queryTimerange = new QueryTimeRange(TimeSpan.FromMinutes(10));
            var waitLimit = TimeSpan.FromMinutes(6);
            var delay = TimeSpan.FromSeconds(50);

            await Task.Delay(delay);

            var wasEventsLogged = await Awaiter
                .TryWaitUntilConditionAsync(
                    async () =>
                    {
                        var actualResponse = await Fixture.LogsQueryClient.QueryWorkspaceAsync<QueryResult>(
                            Fixture.LogAnalyticsWorkspaceId,
                            query,
                            queryTimerange);

                        return ContainsExpectedEvents(expectedEvents, actualResponse.Value);
                    },
                    waitLimit,
                    delay);

            wasEventsLogged.Should().BeTrue($"Was expected to log {expectedEvents.Count} number of events.");
        }

        private bool ContainsExpectedEvents(IList<QueryResult> expectedEvents, IReadOnlyList<QueryResult> actualResults)
        {
            if (actualResults.Count != expectedEvents.Count)
            {
                return false;
            }

            foreach (var expected in expectedEvents)
            {
                switch (expected.Type)
                {
                    case "AppRequests":
                        actualResults.First(actual =>
                            actual.Name == expected.Name);
                        break;

                    case "AppDependencies":
                        actualResults.First(actual =>
                            actual.Name == expected.Name
                            && actual.DependencyType == expected.DependencyType);
                        break;

                    // "AppTraces"
                    default:
                        actualResults.First(actual =>
                            actual.EventName == expected.EventName
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
