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
using System.Collections.Immutable;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;

namespace RequestResponseMiddleware.Tests
{
    public sealed class MockedFunctionContext
    {
        private readonly Mock<FunctionContext> _functionContextMock = new();
        private readonly Mock<BindingContext> _bindingContextMock = new();
        private readonly Mock<FunctionDefinition> _functionDefinitionMock = new();
        private readonly Mock<IServiceProvider> _serviceProviderMock = new();
        private readonly Mock<ILoggerFactory> _loggerFactoryMock = new();
        private readonly Mock<IInvocationFeatures> _features = new();

        public MockedFunctionContext()
        {
            _loggerFactoryMock
                .Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            _serviceProviderMock
                .Setup(x => x.GetService(It.IsAny<Type>()))
                .Returns((Type t) => CreateMockOfType(t).Object);

            _serviceProviderMock
                .Setup(x => x.GetService(typeof(ILoggerFactory)))
                .Returns(_loggerFactoryMock.Object);

            _functionContextMock
                .Setup(x => x.InstanceServices)
                .Returns(_serviceProviderMock.Object);

            _functionContextMock
                .Setup(f => f.BindingContext)
                .Returns(_bindingContextMock.Object);

            _functionContextMock
                .Setup(f => f.FunctionDefinition)
                .Returns(_functionDefinitionMock.Object);
        }

        public FunctionContext FunctionContext => _functionContextMock.Object;

        public Mock<FunctionContext> FunctionContextMock => _functionContextMock;

        public Mock<IServiceProvider> Services => _serviceProviderMock;

        public Mock<BindingContext> BindingContext => _bindingContextMock;

        public Mock<FunctionDefinition> FunctionDefinitionMock => _functionDefinitionMock;

        public void SetInvocationFeatures(IInvocationFeatures features)
        {
            _functionContextMock
                .Setup(f => f.Features)
                .Returns(features);
        }

        public void SetBindingMetaData(Dictionary<string, BindingMetadata> metaData)
        {
            _functionDefinitionMock
                .Setup(e => e.InputBindings)
                .Returns(metaData.ToImmutableDictionary);
        }

        private static Mock CreateMockOfType(Type t)
        {
            return (Mock)Activator.CreateInstance(typeof(Mock<>).MakeGenericType(t))!;
        }
    }
}
