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

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;

namespace Energinet.DataHub.Core.App.FunctionApp.Tests.Common;

public sealed class MockedFunctionContext
{
    private readonly Mock<FunctionContext> _functionContextMock = new();
    private readonly Mock<ILoggerFactory> _loggerFactoryMock = new();

    public MockedFunctionContext()
    {
        _loggerFactoryMock
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger>().Object);

        Services
            .Setup(x => x.GetService(It.IsAny<Type>()))
            .Returns((Type t) => CreateMockOfType(t).Object);

        Services
            .Setup(x => x.GetService(typeof(ILoggerFactory)))
            .Returns(_loggerFactoryMock.Object);

        _functionContextMock
            .Setup(x => x.InstanceServices)
            .Returns(Services.Object);

        _functionContextMock
            .Setup(f => f.BindingContext)
            .Returns(BindingContext.Object);

        _functionContextMock
            .Setup(f => f.FunctionDefinition)
            .Returns(FunctionDefinitionMock.Object);
    }

    public FunctionContext FunctionContext => _functionContextMock.Object;

    public Mock<IServiceProvider> Services { get; } = new();

    public Mock<BindingContext> BindingContext { get; } = new();

    public Mock<FunctionDefinition> FunctionDefinitionMock { get; } = new();

    private static Mock CreateMockOfType(Type t)
    {
        return (Mock)Activator.CreateInstance(typeof(Mock<>).MakeGenericType(t))!;
    }
}
