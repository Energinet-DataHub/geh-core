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
using System.Collections.Generic;
using System.Linq;
using Energinet.DataHub.Core.Messaging.MessageRouting;
using Energinet.DataHub.Core.Messaging.Tests.TestHelpers;
using Energinet.DataHub.Core.Messaging.Transport.SchemaValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Core.Messaging.Tests
{
    [UnitTest]
    public class HandlerExtensionsTests
    {
        [Fact]
        public void HandlerExtensionsWithReasonableDefaults_Should_Setup_MediatR()
        {
            const bool validateScopes = true;
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddGreenEnergyHub();

            var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes);
            var actual = serviceProvider.GetService(typeof(IMediator));

            Assert.NotNull(actual);
        }

        [Fact]
        public void HandlerExtension_Should_Locate_One_IngestionHandler()
        {
            const bool validateScopes = true;
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddGreenEnergyHub(typeof(TestIngestionHandler).Assembly);

            var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes);
            var messageRegistrations = serviceProvider.GetServices(typeof(MessageRegistration)).Count();

            const int expected = 3;

            Assert.Equal(expected, messageRegistrations);
        }

        [Fact]
        public void HandlerExtension_Should_inject_registrations_into_HubMessageTypeMap()
        {
            const bool validateScopes = true;
            var expectedType = typeof(TestMessage);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddGreenEnergyHub(expectedType.Assembly);

            var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes);
            var messageHub = serviceProvider.GetRequiredService<IHubMessageTypeMap>();

            var actualType = messageHub.GetTypeByCategory(expectedType.Name);

            Assert.NotNull(actualType);
            Assert.Equal(expectedType, actualType);
        }

        [Fact]
        public void HandlerExtension_ShouldRegisterAllIHubMessagesAsMessageRegistrations()
        {
            var expectedTypeOne = typeof(TestMessage);
            var expectedTypeTwo = typeof(StubMessage);
            var expectedTypeThree = typeof(SchemaValidatedInboundMessage<>);
            var expectedMessageRegistrationTypes = new List<Type> { expectedTypeOne, expectedTypeTwo, expectedTypeThree };

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddGreenEnergyHub(expectedTypeOne.Assembly, expectedTypeOne.Assembly); // Adding the same assembly twice to avoid false positives, where duplicates are added
            var messageRegistrations = serviceCollection.Where(_ => _.ServiceType == typeof(MessageRegistration));

            Assert.Equal(expectedMessageRegistrationTypes.Count, messageRegistrations.Count());
        }
    }
}
