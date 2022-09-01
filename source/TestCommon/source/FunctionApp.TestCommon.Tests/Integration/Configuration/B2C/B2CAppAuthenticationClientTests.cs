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

using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.Configuration.B2C
{
    public class B2CAppAuthenticationClientTests : IClassFixture<B2CFixture>
    {
        public B2CAppAuthenticationClientTests(B2CFixture fixture)
        {
            Fixture = fixture;
        }

        private B2CFixture Fixture { get; }

        [Fact]
        public async Task BackendAppAuthenticationClient_Should_RetrieveToken()
        {
            var authenticationResult = await Fixture.BackendAppAuthenticationClient.GetAuthenticationTokenAsync();
            authenticationResult.Should().NotBeNull();
            authenticationResult.AccessToken.Should().NotBeNullOrWhiteSpace();
        }
    }
}
