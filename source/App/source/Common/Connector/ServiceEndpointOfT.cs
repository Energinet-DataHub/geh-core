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

namespace Energinet.DataHub.Core.App.Common.Connector;

public class ServiceEndpoint<TService>
    where TService : ServiceEndpoint
{
    private readonly HttpClient _httpClient = null!;
    private readonly SendRequest _sendRequest;

    private delegate Task<HttpResponseMessage> SendRequest(HttpRequestMessage request, CancellationToken cancellationToken = default);

    internal ServiceEndpoint(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _sendRequest = SendWithHttpClient;
    }

    protected ServiceEndpoint()
    {
        _sendRequest = (_, _) => throw new InvalidOperationException($"{nameof(SendAsync)} must be overridden in a derived class");
    } // Used for mocking

    public virtual async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        return await _sendRequest(request, cancellationToken).ConfigureAwait(false);
    }

    private Task<HttpResponseMessage> SendWithHttpClient(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        return _httpClient.SendAsync(request, cancellationToken);
    }
}
