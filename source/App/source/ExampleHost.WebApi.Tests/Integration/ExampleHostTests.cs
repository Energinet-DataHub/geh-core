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
using ExampleHost.WebApi.Tests.Fixtures;
using ExampleHost.WebApi01;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ExampleHost.WebApi.Tests.Integration
{
    /// <summary>
    /// Tests that documents and prooves how we should setup and configure our
    /// Asp.Net Core Web Api's (host's) so they behave as we expect.
    /// </summary>
    public class ExampleHostTests : WebHostTestBase
    {
        public ExampleHostTests(WebApplicationFactory<Startup> factory)
            : base(factory)
        {
        }

        /// <summary>
        /// Verify sunshine scenario.
        /// </summary>
        [Fact]
        public async Task CallingApi01Get_Should_CallApi02Get()
        {
            // Arrange
            var url = "weatherforecast";

            // Act
            var actualResponse = await HttpClient.GetAsync(url);

            // Assert
            using var assertionScope = new AssertionScope();
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            actualResponse.Content.Headers.ContentType!.MediaType.Should().Be("application/json");

            var content = await actualResponse.Content.ReadAsStringAsync();
            content.Should().Contain("\"temperatureC\":");
        }
    }
}
