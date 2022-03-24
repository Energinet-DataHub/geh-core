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

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;

namespace RequestResponseMiddleware.Tests
{
    public sealed class MockedHttpRequestData
    {
        public MockedHttpRequestData(FunctionContext functionContext)
        {
            HttpResponseDataMock = new Mock<HttpResponseData>(functionContext);
            HttpResponseDataMock.SetupProperty(x => x.StatusCode);
            HttpResponseDataMock.SetupProperty(x => x.Body);

            HttpRequestDataMock = new Mock<HttpRequestData>(functionContext);
            HttpResponseDataMock.SetupProperty(x => x.Body);
            HttpRequestDataMock.Setup(x => x.CreateResponse()).Returns(HttpResponseDataMock.Object);
        }

        public Mock<HttpRequestData> HttpRequestDataMock { get; }

        public Mock<HttpResponseData> HttpResponseDataMock { get; }

        public HttpRequestData HttpRequestData => HttpRequestDataMock.Object;

        public HttpResponseData HttpResponseData => HttpResponseDataMock.Object;

#pragma warning disable
        public static implicit operator HttpRequestData(MockedHttpRequestData mock)
        {
            return mock.HttpRequestData;
        }
#pragma warning restore

        public void SetResponseHeaderCollection(HttpHeadersCollection headers)
        {
            HttpResponseDataMock
                .Setup(h => h.Headers)
                .Returns(headers);
        }

        public void SetRequestHeaderCollection(HttpHeadersCollection headers)
        {
            HttpRequestDataMock
                .Setup(h => h.Headers)
                .Returns(headers);
        }
    }
}
