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
using System.Threading.Tasks;
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using Xunit;
using Xunit.Categories;

namespace RequestResponseMiddleware.Tests
{
    [UnitTest]
    public class RequestResponseLoggingMiddlewareTests
    {
        [Fact]
        public async Task RequestResponseLoggingMiddleware_AllOk()
        {
            // Arrange
            var blobStorage = new Mock<IRequestResponseLogging>();
            var middleware = new RequestResponseLoggingMiddleware(blobStorage.Object);
            var functionContext = new MockedFunctionContext();

            var data = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                { "Headers", "{\"Authorization\":\"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0In0.vVkzbkZ6lB3srqYWXVA00ic5eXwy4R8oniHQyok0QWY\"}" },
            };

            functionContext.BindingContext.Setup(x => x.BindingData)
                .Returns(data);

            SetUpTest(functionContext);

            // Act
            await middleware.Invoke(functionContext, _ => Task.CompletedTask).ConfigureAwait(false);

            // Assert
            await Task.Delay(1);
        }

        private void SetUpTest(MockedFunctionContext functionContext)
        {
            var httpRequest = new MockedHttpRequestData(functionContext);
            var responseData = httpRequest.HttpResponseData;
            httpRequest.SetResponseHeaderCollection(new HttpHeadersCollection(
                new List<KeyValuePair<string, string>>() { new("TestId", "200") }));

            var invocationFeatures = new MockedFunctionInvocationFeatures();
            invocationFeatures.Set(new IFunctionBindingsFeature()
            {
                InvocationResult = responseData,
                InputData = new Dictionary<string, object> { { "request", httpRequest.HttpRequestData } },
            });
            functionContext.SetInvocationFeatures(invocationFeatures);
        }

        private class IFunctionBindingsFeature
        {
            public HttpResponseData InvocationResult { get; set; }

            public IReadOnlyDictionary<string, object> InputData { get; set; }
        }
    }
}
