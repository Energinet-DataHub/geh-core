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

#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware;
using Microsoft.Azure.Functions.Worker.Http;
using Xunit;
using Xunit.Categories;

namespace RequestResponseMiddleware.Tests
{
    [UnitTest]
    public class RequestResponseLoggingMiddlewareTests
    {
        [Fact]
        public async Task RequestResponseLoggingMiddleware_Body_AllOk()
        {
            // Arrange
            var testStorage = new LocalLogStorage();
            var middleware = new RequestResponseLoggingMiddleware(testStorage);
            var functionContext = new MockedFunctionContext();

            var responseHeaderData = new List<KeyValuePair<string, string>>() { new("StatusCodeTest", "200") };

            functionContext.BindingContext
                .Setup(x => x.BindingData)
                .Returns(new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase));

            var expectedStatusCode = HttpStatusCode.Accepted;

            var (request, response) = SetUpContext(functionContext, responseHeaderData, expectedStatusCode);

            var expectedLogBody = "BODYTEXT";

            request.HttpRequestDataMock.SetupGet(e => e.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(expectedLogBody)));

            response.Body = new MemoryStream(Encoding.UTF8.GetBytes(expectedLogBody));

            // Act
            await middleware.Invoke(functionContext, _ => Task.CompletedTask).ConfigureAwait(false);

            // Assert
            var savedLogs = testStorage.GetLogs();
            Assert.Contains(savedLogs, e => e.Body.Equals(expectedLogBody));
        }

        [Fact]
        public async Task RequestResponseLoggingMiddleware_AllOk()
        {
            // Arrange
            var testStorage = new LocalLogStorage();
            var middleware = new RequestResponseLoggingMiddleware(testStorage);
            var functionContext = new MockedFunctionContext();

            var inputData = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                { "Headers", "{\"Authorization\":\"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0In0.vVkzbkZ6lB3srqYWXVA00ic5eXwy4R8oniHQyok0QWY\"}" },
                { "MarketOperator", "232323232" },
                { "Accept-Type", "232323232" },
            };

            var responseHeaderData = new List<KeyValuePair<string, string>>() { new("Statuscodetest", "200") };

            functionContext.BindingContext
                .Setup(x => x.BindingData)
                .Returns(inputData);

            var expectedStatusCode = HttpStatusCode.Accepted;

            var (request, response) = SetUpContext(functionContext, responseHeaderData, expectedStatusCode);

            var logBody = "BODYTEXT";
            request.HttpRequestDataMock.SetupGet(e => e.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(logBody)));
            response.Body = new MemoryStream(Encoding.UTF8.GetBytes(logBody));

            // Act
            await middleware.Invoke(functionContext, _ => Task.CompletedTask).ConfigureAwait(false);

            // Assert
            var savedLogs = testStorage.GetLogs();
            Assert.Contains(savedLogs, e => e.MetaData.ContainsKey("headers"));
            Assert.Contains(savedLogs, e => e.MetaData.ContainsKey("marketoperator"));
            Assert.Contains(savedLogs, l => l.MetaData.TryGetValue("statuscodetest", out var value) && value == "200");
            Assert.Contains(savedLogs, l => l.MetaData.TryGetValue("statuscode", out var value) && value == expectedStatusCode.ToString());
        }

        private (MockedHttpRequestData HttpRequestData, HttpResponseData HttpResponseData) SetUpContext(
            MockedFunctionContext functionContext,
            List<KeyValuePair<string, string>> responseHeader,
            HttpStatusCode statusCode)
        {
            var httpRequest = new MockedHttpRequestData(functionContext);
            var responseData = httpRequest.HttpResponseData;
            responseData.StatusCode = statusCode;
            httpRequest.SetResponseHeaderCollection(new HttpHeadersCollection(responseHeader));

            var invocationFeatures = new MockedFunctionInvocationFeatures();
            invocationFeatures.Set(new IFunctionBindingsFeature()
            {
                InvocationResult = responseData,
                InputData = new Dictionary<string, object> { { "request", httpRequest.HttpRequestData } },
            });
            functionContext.SetInvocationFeatures(invocationFeatures);

            return new(httpRequest, responseData);
        }

        private class IFunctionBindingsFeature
        {
            public HttpResponseData InvocationResult { get; set; }

            public IReadOnlyDictionary<string, object> InputData { get; set; }
        }
    }
}
