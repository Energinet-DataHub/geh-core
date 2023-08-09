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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.WebApp.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.WebApp.Hosting;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Core.App.Hosting.Tests.Diagnostics.HealthChecks;

public class RepeatingTriggerHealthCheckRegistrationTests
{
    [Theory]
    [InlineAutoMoqData]
    public void Given_ATrigger_When_HealthCheckIsRegistered_Then_HealthCheckCanBeResolvedFromContainer(
        TimeSpan anyTimeSpan)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddHealthChecks();

        // Act
        sut.AddRepeatingTriggerHealthCheck<SomeRepeatingTrigger>(anyTimeSpan);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var healthCheck = serviceProvider.GetRequiredService<RepeatingTriggerHealthCheck<SomeRepeatingTrigger>>();
        healthCheck.Should().NotBeNull();
    }

    private class SomeRepeatingTrigger : RepeatingTrigger<object>
    {
        public SomeRepeatingTrigger(IServiceProvider serviceProvider, ILogger logger, TimeSpan delayBetweenExecutions)
            : base(serviceProvider, logger, delayBetweenExecutions)
        {
        }

        protected override Task ExecuteAsync(object scopedService, CancellationToken cancellationToken, Action isAliveCallback)
        {
            throw new NotImplementedException();
        }
    }
}
