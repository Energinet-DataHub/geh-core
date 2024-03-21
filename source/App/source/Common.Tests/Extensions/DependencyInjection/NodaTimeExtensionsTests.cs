﻿// Copyright 2020 Energinet DataHub A/S
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
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Core.App.Common.Tests.Extensions.DependencyInjection;

public class NodaTimeExtensionsTests
{
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

        using var assertionScope = new AssertionScope();
        var serviceProvider = Services.BuildServiceProvider();

        var actualClockService = serviceProvider.GetRequiredService<IClock>();

        var actualDateTimeZone = serviceProvider.GetRequiredService<DateTimeZone>();
        actualDateTimeZone.Id.Should().Be(NodaTimeOptions.DefaultTimeZone);
    }
}
