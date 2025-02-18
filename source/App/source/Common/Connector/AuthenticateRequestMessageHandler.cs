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

using System.Net.Http.Headers;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.App.Common.Connector;

internal class AuthenticateRequestMessageHandler<TService>
    : DelegatingHandler
    where TService : ServiceEndpoint
{
    private delegate Task<HttpResponseMessage> RequestHandler(HttpRequestMessage request, CancellationToken cancellationToken);

    private readonly ServiceEndpoint _options;
    private readonly IMemoryCache _cache;
    private readonly RequestHandler _handler;

    public AuthenticateRequestMessageHandler(IOptions<TService> options, IMemoryCache cache)
    {
        _options = options.Value;
        _cache = cache;

        _handler = _options.Identity != null ? AuthenticateRequest : PassthroughRequest;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => _handler(request, cancellationToken);

    private async Task<HttpResponseMessage> AuthenticateRequest(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(accessToken)) throw new InvalidOperationException("Failed to get access token");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private Task<HttpResponseMessage> PassthroughRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        => base.SendAsync(request, cancellationToken);

    private static string GetCacheKey() => typeof(TService).Name;

    private async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var token = await _cache.GetOrCreateAsync(
            GetCacheKey(),
            async entry =>
        {
            var identity = _options.Identity ?? throw new InvalidOperationException("Identity must be set");
            if (identity.ApplicationIds == null || identity.ApplicationIds.Length == 0) throw new InvalidOperationException("ApplicationIds must be set");

            var cred = new DefaultAzureCredential();
            var requestContext = new TokenRequestContext(identity.ApplicationIds);

            var token = await cred.GetTokenAsync(requestContext, cancellationToken).ConfigureAwait(false);
            entry.AbsoluteExpiration = token.ExpiresOn.AddMinutes(-2);
            return token.Token;
        }).ConfigureAwait(false);

        return token;
    }
}
