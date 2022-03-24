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
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware;
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
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
            var logger = Mock.Of<ILogger<RequestResponseLoggingMiddleware>>();
            var middleware = new RequestResponseLoggingMiddleware(testStorage, logger);
            var functionContext = new MockedFunctionContext();

            var responseHeaderData = new List<KeyValuePair<string, string>>() { new("StatusCodeTest", "200") };

            functionContext.BindingContext
                .Setup(x => x.BindingData)
                .Returns(new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase));

            var bindingData = new Dictionary<string, BindingMetadata>();
            bindingData.Add("request", new FunctionBindingMetaData("httpTrigger", BindingDirection.In));
            functionContext.SetBindingMetaData(bindingData);

            var expectedStatusCode = HttpStatusCode.Accepted;

            var (request, response) = SetUpContext(functionContext, responseHeaderData, expectedStatusCode);

            var expectedLogBody = "BODYTEXT";

            request.HttpRequestDataMock.SetupGet(e => e.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(expectedLogBody)));

            response.Body = new MemoryStream(Encoding.UTF8.GetBytes(expectedLogBody));

            // Act
            await middleware.Invoke(functionContext.FunctionContext, _ => Task.CompletedTask).ConfigureAwait(false);

            // Assert
            var savedLogs = testStorage.GetLogs().ToList();
            Assert.Equal(expectedLogBody, savedLogs[0].Body);
            Assert.Equal(expectedLogBody, savedLogs[1].Body);
        }

        [Fact]
        public async Task RequestResponseLoggingMiddleware_AllOk()
        {
            // Arrange
            var testStorage = new LocalLogStorage();
            var logger = Mock.Of<ILogger<RequestResponseLoggingMiddleware>>();
            var middleware = new RequestResponseLoggingMiddleware(testStorage, logger);
            var functionContext = new MockedFunctionContext();

            var inputData = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                { "Headers", "{\"Authorization\":\"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0In0.vVkzbkZ6lB3srqYWXVA00ic5eXwy4R8oniHQyok0QWY\"}" },
                { "Query", "{ BundleId: 123 }" },
                { "BundleId", "132" },
                { string.Empty, "error skipped" },
                { "Correlationid", "2aaa720a-a7b9-4fe4-a004-f222ad932c7a" },
            };

            var responseHeaderData = new List<KeyValuePair<string, string>>()
            {
                new("Statuscodetest", "200"),
                new("Accept-Type", "232323232"),
                new("Authorization", "Bearer ****"),
                new(string.Empty, "error skipped"),
                new("CorrelationidRes", "1aaa720a-a7b9-4fe4-a004-f222ad932c7a"),
            };

            functionContext.BindingContext
                .Setup(x => x.BindingData)
                .Returns(inputData);

            var bindingData = new Dictionary<string, BindingMetadata>();
            bindingData.Add("request", new FunctionBindingMetaData("httpTrigger", BindingDirection.In));
            functionContext.SetBindingMetaData(bindingData);

            var expectedStatusCode = HttpStatusCode.Accepted;

            var (request, response) = SetUpContext(functionContext, responseHeaderData, expectedStatusCode);

            var logBody = "BODYTEXT";
            request.HttpRequestDataMock.SetupGet(e => e.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(logBody)));
            response.Body = new MemoryStream(Encoding.UTF8.GetBytes(logBody));

            // Act
            await middleware.Invoke(functionContext.FunctionContext, _ => Task.CompletedTask).ConfigureAwait(false);

            // Assert
            var savedLogs = testStorage.GetLogs();
            Assert.DoesNotContain(savedLogs, e => e.MetaData.ContainsKey("headers"));
            Assert.Contains(savedLogs, e => e.MetaData.ContainsKey("bundleid"));
            Assert.Contains(savedLogs, l => l.MetaData.TryGetValue("statuscodetest", out var value) && value == "200");
            Assert.Contains(savedLogs, l => l.MetaData.TryGetValue("statuscode", out var value) && value == expectedStatusCode.ToString());
            Assert.Contains(savedLogs, l => l.MetaData.TryGetValue("authorization", out var value) && value == "Bearer ****");
            Assert.DoesNotContain(savedLogs, t => t.MetaData.ContainsKey(string.Empty));
            Assert.DoesNotContain(savedLogs, t => t.MetaData.ContainsValue("error skipped"));
            Assert.Contains(savedLogs, l => l.MetaData.TryGetValue("correlationid", out var value) && value == "2aaa720a-a7b9-4fe4-a004-f222ad932c7a");
            Assert.Contains(savedLogs, l => l.MetaData.TryGetValue("correlationidres", out var value) && value == "1aaa720a-a7b9-4fe4-a004-f222ad932c7a");
        }

        [Fact]
        public async Task RequestResponseLoggingMiddleware_ExpectNoLog_NotHttpTrigger()
        {
            // Arrange
            var testStorage = new LocalLogStorage();
            var logger = Mock.Of<ILogger<RequestResponseLoggingMiddleware>>();
            var middleware = new RequestResponseLoggingMiddleware(testStorage, logger);
            var functionContext = new MockedFunctionContext();

            var bindingData = new Dictionary<string, BindingMetadata>();
            bindingData.Add("request", new FunctionBindingMetaData("httpTrigger", BindingDirection.In));
            functionContext.SetBindingMetaData(bindingData);

            var responseHeaderData = new List<KeyValuePair<string, string>>() { new("Statuscodetest", "200") };

            var expectedStatusCode = HttpStatusCode.Accepted;

            SetUpContext(functionContext, responseHeaderData, expectedStatusCode);
            functionContext.SetInvocationFeatures(new MockedFunctionInvocationFeatures());

            // Act
            await middleware.Invoke(functionContext.FunctionContext, _ => Task.CompletedTask).ConfigureAwait(false);

            // Assert
            var savedLogs = testStorage.GetLogs();
            Assert.False(savedLogs.Any());
        }

        [Fact]
        public async Task RequestResponseLoggingMiddleware_ExpectNoLog_MonitorPath()
        {
            // Arrange
            var testStorage = new LocalLogStorage();
            var logger = Mock.Of<ILogger<RequestResponseLoggingMiddleware>>();
            var middleware = new RequestResponseLoggingMiddleware(testStorage, logger);
            var functionContext = new MockedFunctionContext();

            var inputData = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { { "ok", "{\"ok\":\"ok\"}" }, };
            functionContext.BindingContext.Setup(x => x.BindingData).Returns(inputData);

            var (httpRequestData, httpResponseData) = SetUpContext(functionContext, new List<KeyValuePair<string, string>>(), HttpStatusCode.Accepted);
            httpRequestData.HttpRequestDataMock.SetupGet(e => e.Url).Returns(new Uri("https://localhost/monitor/"));

            // Act
            await middleware.Invoke(functionContext.FunctionContext, _ => Task.CompletedTask).ConfigureAwait(false);

            // Assert
            var savedLogs = testStorage.GetLogs();
            Assert.False(savedLogs.Any());
        }

        [Fact]
        public async Task RequestResponseLoggingMiddleware_SelectedTagsOk()
        {
            // Arrange
            var testStorage = new LocalLogStorage();
            var logger = Mock.Of<ILogger<RequestResponseLoggingMiddleware>>();
            var middleware = new RequestResponseLoggingMiddleware(testStorage, logger);
            var functionContext = new MockedFunctionContext();

            var token1 = "Bearer ";
            var token2 = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwI";
            var token3 = "iwibmFtZSI6IkpvaG4gRG9lIiwiZXhwIjoxNDQ0MjY4ODEwLCJhenAiOiIxMTE5YjliMi1lZGNlLTRmNzQtYjQ2Ni05OGQwYmJiMGE5NGEiLCJpYXQiOjE1MTYyMzkwMjJ9.Kia6V3YOtfwjauBRZbOswXq4beyeNLHPAKJ0aqZhqDg";
            var inputData = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                { "Headers", "{\"Authorization\":\"" + token1 + token2 + token3 + "\"}" },
                { "Query", "{ BundleId: 123 }" },
                { "BundleId", "132" },
                { string.Empty, "error skipped" },
            };

            var responseHeaderData = new List<KeyValuePair<string, string>>
            {
                new(IndexTagsKeys.StatusCode, "200"),
                new(string.Empty, "error skipped"),
                new(IndexTagsKeys.CorrelationId, "1aaa720a-a7b9-4fe4-a004-f222ad932c7a"),
            };

            functionContext.BindingContext
                .Setup(x => x.BindingData)
                .Returns(inputData);

            var bindingData = new Dictionary<string, BindingMetadata>();
            bindingData.Add("request", new FunctionBindingMetaData("httpTrigger", BindingDirection.In));
            functionContext.SetBindingMetaData(bindingData);

            var (request, response) = SetUpContext(functionContext, responseHeaderData, HttpStatusCode.Accepted);
            request.SetRequestHeaderCollection(new HttpHeadersCollection(new[]
            {
                new KeyValuePair<string, string>("Authorization", "Bearer onlyTestString"),
                new KeyValuePair<string, string>("Accept-Type", "*/*"),
                new KeyValuePair<string, string>(IndexTagsKeys.CorrelationId, "2aaa720a-a7b9-4fe4-a004-f222ad932c7a"),
            }));

            var logBody = "BODYTEXT";
            request.HttpRequestDataMock.SetupGet(e => e.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(logBody)));
            response.Body = new MemoryStream(Encoding.UTF8.GetBytes(logBody));

            var functionName = "TestFunctionName";
            var functionId = Guid.NewGuid().ToString();
            var invocationId = Guid.NewGuid().ToString();
            var traceParent = "00-d3ff2b9ea8e4b3488ef1b0cd785851b5-4aeeadc281bd0940-00";
            var traceId = "d3ff2b9ea8e4b3488ef1b0cd785851b5";

            var traceContext = new Mock<TraceContext>();
            traceContext.Setup(e => e.TraceParent).Returns(traceParent);

            functionContext.FunctionDefinitionMock.Setup(e => e.Name).Returns(functionName);
            functionContext.FunctionContextMock.Setup(e => e.FunctionId).Returns(functionId);
            functionContext.FunctionContextMock.Setup(e => e.InvocationId).Returns(invocationId);
            functionContext.FunctionContextMock.Setup(e => e.TraceContext).Returns(traceContext.Object);

            // Act
            await middleware.Invoke(functionContext.FunctionContext, _ => Task.CompletedTask).ConfigureAwait(false);

            // Assert
            var savedRequestLogs = testStorage.GetLogs().Where(e => e.MetaData.ContainsValue("request")).ToList();
            var savedResponseLogs = testStorage.GetLogs().Where(e => e.MetaData.ContainsValue("response")).ToList();

            Assert.DoesNotContain(savedRequestLogs, l => l.MetaData.ContainsKey("headers"));
            Assert.DoesNotContain(savedRequestLogs, t => t.MetaData.ContainsKey(string.Empty));
            Assert.DoesNotContain(savedRequestLogs, t => t.MetaData.ContainsValue("error skipped"));

            Assert.Contains(savedRequestLogs, l => l.MetaData.ContainsKey("bundleid"));
            Assert.Contains(savedRequestLogs, l => l.MetaData.ContainsKey("accepttype"));
            Assert.Contains(savedRequestLogs, l => l.MetaData.TryGetValue("uniquelogname", out var value) && Guid.TryParse(value, out var t));
            Assert.Contains(savedRequestLogs, l => l.MetaData.TryGetValue("authorization", out var value) && value == "Bearer ****");
            Assert.Contains(savedRequestLogs, l => l.MetaData.TryGetValue("correlationid", out var value) && value == "2aaa720a-a7b9-4fe4-a004-f222ad932c7a");
            Assert.Contains(savedRequestLogs, l => l.MetaData.TryGetValue("jwtactorid", out var value) && value == "1119b9b2-edce-4f74-b466-98d0bbb0a94a");
            Assert.Contains(savedRequestLogs, l => l.MetaData.TryGetValue("httpdatatype", out var value) && value == "request");

            Assert.Contains(savedRequestLogs, l => l.MetaData.TryGetValue("functionname", out var value) && value.Equals(functionName));
            Assert.Contains(savedRequestLogs, l => l.MetaData.TryGetValue("functionid", out var value) && value.Equals(functionId));
            Assert.Contains(savedRequestLogs, l => l.MetaData.TryGetValue("invocationid", out var value) && value.Equals(invocationId));
            Assert.Contains(savedRequestLogs, l => l.MetaData.TryGetValue("traceparent", out var value) && value.Equals(traceParent));
            Assert.Contains(savedRequestLogs, l => l.MetaData.TryGetValue("traceid", out var value) && value.Equals(traceId));

            Assert.Contains(savedResponseLogs, l => l.MetaData.TryGetValue("uniquelogname", out var value) && Guid.TryParse(value, out var t));
            Assert.Contains(savedResponseLogs, l => l.MetaData.TryGetValue("statuscode", out var value) && value == HttpStatusCode.Accepted.ToString());
            Assert.Contains(savedResponseLogs, l => l.MetaData.TryGetValue("correlationid", out var value) && value == "1aaa720a-a7b9-4fe4-a004-f222ad932c7a");
            Assert.Contains(savedResponseLogs, l => l.MetaData.TryGetValue("httpdatatype", out var value) && value == "response");

            Assert.Contains(savedResponseLogs, l => l.MetaData.TryGetValue("functionname", out var value) && value.Equals(functionName));
            Assert.Contains(savedResponseLogs, l => l.MetaData.TryGetValue("functionid", out var value) && value.Equals(functionId));
            Assert.Contains(savedResponseLogs, l => l.MetaData.TryGetValue("invocationid", out var value) && value.Equals(invocationId));
            Assert.Contains(savedResponseLogs, l => l.MetaData.TryGetValue("traceparent", out var value) && value.Equals(traceParent));
            Assert.Contains(savedResponseLogs, l => l.MetaData.TryGetValue("traceid", out var value) && value.Equals(traceId));
        }

        private (MockedHttpRequestData HttpRequestData, HttpResponseData HttpResponseData) SetUpContext(
            MockedFunctionContext functionContext,
            List<KeyValuePair<string, string>> responseHeader,
            HttpStatusCode statusCode)
        {
            var httpRequest = new MockedHttpRequestData(functionContext.FunctionContext);
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

            functionContext.FunctionDefinitionMock
                .Setup(e => e.Name)
                .Returns("TestFunction");

            httpRequest.HttpRequestDataMock.SetupGet(e => e.Url).Returns(new Uri("https://localhost/testpath/"));

            return new(httpRequest, responseData);
        }

        private class IFunctionBindingsFeature
        {
            public HttpResponseData InvocationResult { get; set; } = null!;

            public IReadOnlyDictionary<string, object> InputData { get; set; } = null!;
        }
    }
}
