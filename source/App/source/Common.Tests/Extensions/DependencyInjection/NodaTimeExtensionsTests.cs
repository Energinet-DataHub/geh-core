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
using Energinet.DataHub.Core.App.Common.Extensions.Options;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Core.App.Common.Tests.Extensions.DependencyInjection;

public class NodaTimeExtensionsTests
{
    private const string DateTimeZoneLondon = "Europe/London";
    private const string DateTimeZoneHonolulu = "Pacific/Honolulu";

    public NodaTimeExtensionsTests()
    {
        Services = new ServiceCollection();
    }

    private ServiceCollection Services { get; }

    [Fact]
    public void AddNodaTimeForApplication_WhenCalled_RegistrationsArePerformedWithDefaultTimeZone()
    {
        // Act
        Services.AddNodaTimeForApplication();

        // Assert
        using var assertionScope = new AssertionScope();
        var serviceProvider = Services.BuildServiceProvider();

        var actualClockService = serviceProvider.GetRequiredService<IClock>();
        actualClockService.Should().Be(SystemClock.Instance);

        var actualDateTimeZone = serviceProvider.GetRequiredService<DateTimeZone>();
        actualDateTimeZone.Id.Should().Be(NodaTimeOptions.DefaultTimeZone);
    }

    [Fact]
    public void AddNodaTimeForApplication_WhenCalledWithSectionName_RegistrationsArePerformedWithConfiguredTimeZone()
    {
        // Arrange
        AddInMemoryConfigurations(new Dictionary<string, string?>()
        {
            [$"{NodaTimeOptions.SectionName}:{nameof(NodaTimeOptions.TimeZone)}"] = DateTimeZoneLondon,
        });

        // Act
        Services.AddNodaTimeForApplication(configSectionPath: NodaTimeOptions.SectionName);

        // Assert
        using var assertionScope = new AssertionScope();
        var serviceProvider = Services.BuildServiceProvider();

        var actualClockService = serviceProvider.GetRequiredService<IClock>();
        actualClockService.Should().Be(SystemClock.Instance);

        var actualDateTimeZone = serviceProvider.GetRequiredService<DateTimeZone>();
        actualDateTimeZone.Id.Should().Be(DateTimeZoneLondon);
    }

    [Fact]
    public void AddNodaTimeForApplication_WhenUsingOverloadsAndCalledMultipleTimes_RegistrationsArePerformedOnlyOnce()
    {
        // Arrange
        AddInMemoryConfigurations(new Dictionary<string, string?>()
        {
            [$"A:{nameof(NodaTimeOptions.TimeZone)}"] = DateTimeZoneLondon,
            [$"B:{nameof(NodaTimeOptions.TimeZone)}"] = DateTimeZoneHonolulu,
        });

        // Act
        Services.AddNodaTimeForApplication();
        Services.AddNodaTimeForApplication(configSectionPath: "A");
        Services.AddNodaTimeForApplication(configSectionPath: "B");

        // Assert
        using var assertionScope = new AssertionScope();
        var serviceProvider = Services.BuildServiceProvider();

        var actualClockService = serviceProvider.GetServices<IClock>();
        actualClockService.Count().Should().Be(1);

        var actualDateTimeZone = serviceProvider.GetServices<DateTimeZone>();
        actualDateTimeZone.Single().Id.Should().Be(NodaTimeOptions.DefaultTimeZone);
    }

    private void AddInMemoryConfigurations(Dictionary<string, string?> configurations)
    {
        Services.AddScoped<IConfiguration>(_ =>
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(configurations)
                .Build();
        });
    }
}
