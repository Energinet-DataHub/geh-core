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

using ExampleHost;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ExampleHost.WebApi.Tests.Fixtures
{
    public class ExampleHostFixture : IAsyncLifetime
    {
        public ExampleHostFixture()
        {
            Web01Factory = new WebApplicationFactory<WebApi01.Startup>();
            Web01HttpClient = Web01Factory.CreateClient();
        }

        public HttpClient Web01HttpClient { get; }

        private WebApplicationFactory<WebApi01.Startup> Web01Factory { get; }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            // Disposing factory will dispose any created http clients.
            Web01Factory.Dispose();

            return Task.CompletedTask;
        }
    }
}
