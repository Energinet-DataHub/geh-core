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

using System.Reflection;
using Energinet.DataHub.Core.Logging.LoggingMiddleware.Internal;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Energinet.DataHub.Core.Logging.LoggingMiddleware.Tests;

public class HttpLoggingScopeMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldSetKeysAndValues()
    {
        // Arrange
        var domain = "testing-domain";
        var mockedLogger = new Mock<ILogger<HttpLoggingScopeMiddleware>>();
        var mockedRootLoggingScope = new RootLoggingScope(domain);
        var httpContext = new DefaultHttpContext();
        var next = new RequestDelegate(context => Task.CompletedTask);
        var sut = new HttpLoggingScopeMiddleware(mockedLogger.Object, mockedRootLoggingScope);

        // Act
        await sut.InvokeAsync(httpContext, next);

        // Assert
        var expectedApplicationEntry = Assembly.GetEntryAssembly()?.FullName;

        mockedRootLoggingScope["Domain"].Should().Be(domain);
        mockedRootLoggingScope["ApplicationEntry"].Should().Be(expectedApplicationEntry);
        mockedRootLoggingScope.Count.Should().Be(2);
    }
}
