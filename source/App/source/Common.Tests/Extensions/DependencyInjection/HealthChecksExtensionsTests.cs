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

using Energinet.DataHub.Core.App.Common.Extensions.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Energinet.DataHub.Core.App.Common.Tests.Extensions.DependencyInjection;

public class HealthChecksExtensionsTests
{
    public HealthChecksExtensionsTests()
    {
        Services = new ServiceCollection();
    }

    private ServiceCollection Services { get; }

    [Fact]
    public void TryAddHealthChecks_WhenCalledWithRegistrationKey_RegistrationsArePerformedWithRegistrationKey()
    {
        var registrationKey = "MyKey";
        var actualKey = string.Empty;

        // Act
        Services.TryAddHealthChecks(
            registrationKey,
            (key, builder) =>
            {
                // Any registrations can be performed here
                actualKey = key;
            });

        // Assert
        actualKey.Should().Be(registrationKey);
    }

    [Fact]
    public void TryAddHealthChecks_WhenCalledMultipleTimesWithSameRegistrationKey_BuilderDelegateIsCalledOnlyOnce()
    {
        var registrationKey = "MyKey";
        var count = 0;

        Services.TryAddHealthChecks(
            registrationKey,
            (key, builder) =>
            {
                count++;
            });

        // Act
        Services.TryAddHealthChecks(
            registrationKey,
            (key, builder) =>
            {
                count++;
            });

        // Assert
        count.Should().Be(1);
    }
}
