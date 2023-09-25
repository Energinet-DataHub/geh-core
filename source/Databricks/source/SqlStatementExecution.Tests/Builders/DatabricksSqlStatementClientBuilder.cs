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

using System.Net;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.AppSettings;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Constants;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Tests.Builders;

public class DatabricksSqlStatementClientBuilder
{
    private readonly List<HttpResponseMessage> _responseMessages = new();
    private readonly List<HttpResponseMessage> _externalResponseMessages = new();
    private ISqlResponseParser? _parser;

    public DatabricksSqlStatementClientBuilder AddHttpClientResponse(string content, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
    {
        _responseMessages.Add(new HttpResponseMessage(httpStatusCode) { Content = new StringContent(content), });
        return this;
    }

    public DatabricksSqlStatementClientBuilder AddExternalHttpClientResponse(string content, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
    {
        _externalResponseMessages.Add(new HttpResponseMessage(httpStatusCode) { Content = new StringContent(content), });
        return this;
    }

    public DatabricksSqlStatementClientBuilder UseParser(ISqlResponseParser parser)
    {
        _parser = parser;
        return this;
    }

    public DatabricksSqlStatementClient Build()
    {
        var httpClientFactory = new Mock<IHttpClientFactory>();

        SetupHttpClient(httpClientFactory, _responseMessages, HttpClientNameConstants.Databricks);
        SetupHttpClient(httpClientFactory, _externalResponseMessages, HttpClientNameConstants.External);

        var options = new Mock<IOptions<DatabricksSqlStatementOptions>>();
        options.Setup(o => o.Value).Returns(new DatabricksSqlStatementOptions
        {
            WorkspaceUrl = "https://foo.com",
        });
        var parser = _parser ?? new Mock<ISqlResponseParser>().Object;
        var logger = new Mock<ILogger<DatabricksSqlStatementClient>>();
        return new DatabricksSqlStatementClient(httpClientFactory.Object, options.Object, parser, logger.Object);
    }

    private void SetupHttpClient(Mock<IHttpClientFactory> httpClientFactory, List<HttpResponseMessage> responseMessages, string clientName)
    {
        var handlerMock = new HttpMessageHandlerMock(responseMessages);
        var httpClient = new HttpClient(handlerMock);
        httpClient.BaseAddress = new Uri("https://foo.com");
        httpClientFactory.Setup(f => f.CreateClient(clientName))
            .Returns(httpClient);
    }

    private class HttpMessageHandlerMock : HttpMessageHandler
    {
        private readonly List<HttpResponseMessage> _messages;
        private int _index;

        public HttpMessageHandlerMock(List<HttpResponseMessage> messages)
        {
            _index = 0;
            _messages = messages;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_messages[_index++]);
        }
    }
}