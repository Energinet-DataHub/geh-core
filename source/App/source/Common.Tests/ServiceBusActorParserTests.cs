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

using System;
using Energinet.DataHub.Core.App.Common.Parsers.Helpers;
using Xunit;

namespace Energinet.DataHub.Core.App.Common.Tests
{
    public class UserIdentityFactoryTests
    {
        [Theory]
        [InlineData("{\"geh_userIdentity\":\"{\\\"ActorId\\\":\\\"c6043cb9-b79c-407e-b8e8-f1d87d9f7b50\\\"}\"}", "geh_userIdentity", "c6043cb9-b79c-407e-b8e8-f1d87d9f7b50")]
        [InlineData("{\"other_userIdentity\":\"{\\\"ActorId\\\":\\\"c6043cb9-b79c-407e-b8e8-f1d87d9f7b50\\\"}\", \"geh_userIdentity\":\"{\\\"ActorId\\\":\\\"c6043cb9-b79c-407e-b8e8-f1d87d9f7b50\\\"}\"}", "geh_userIdentity", "c6043cb9-b79c-407e-b8e8-f1d87d9f7b50")]
        public void ConvertToUserIdentityFromDictionaryString(string inputText, string propertyKey, string expectedUserId)
        {
            var userIdentityParsed = ServiceBusActorParser.FromDictionaryString(inputText, propertyKey);

            Assert.Equal(Guid.Parse(expectedUserId), userIdentityParsed?.ActorId);
        }
    }
}
