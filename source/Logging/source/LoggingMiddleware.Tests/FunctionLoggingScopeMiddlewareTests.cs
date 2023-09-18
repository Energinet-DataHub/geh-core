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
using Energinet.DataHub.Core.Logging;
using Energinet.DataHub.Core.Logging.LoggingMiddleware.Internal;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Moq;

namespace LoggingMiddleware.Tests;

public class FunctionLoggingScopeMiddlewareTests
{
    [Fact]
    public async Task Invoke_ShouldSetKeysAndValues()
    {
        // Arrange
        var domain = "testing-domain";
        var mockedLogger = new Mock<ILogger<FunctionLoggingScopeMiddleware>>();
        var mockedRootLoggingScope = new RootLoggingScope(domain);
        var next = new FunctionExecutionDelegate(context =>
        {
            mockedLogger.Object.LogInformation("test");
            return Task.CompletedTask;
        });
        var mockedFunctionContext = new Mock<FunctionContext>();
        var sut = new FunctionLoggingScopeMiddleware(mockedLogger.Object, mockedRootLoggingScope);

        // Act
        await sut.Invoke(mockedFunctionContext.Object, next);

        // Assert
        var expectedApplicationEntry = Assembly.GetEntryAssembly()?.FullName;

        mockedRootLoggingScope["Domain"].Should().Be(domain);
        mockedRootLoggingScope["ApplicationEntry"].Should().Be(expectedApplicationEntry);
        mockedRootLoggingScope.Count.Should().Be(2);
    }
}
