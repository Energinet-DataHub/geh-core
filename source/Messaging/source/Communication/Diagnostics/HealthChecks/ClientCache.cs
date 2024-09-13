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

using System.Collections.Concurrent;

namespace Energinet.DataHub.Core.Messaging.Communication.Diagnostics.HealthChecks;

/// <summary>
/// This is a re-implementation of the ClientCache class from <see href="https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/blob/master/src/HealthChecks.AzureServiceBus/ClientCache.cs"/>.
/// </summary>
internal static class ClientCache
{
    public static T GetOrAdd<T>(string key, Func<string, T> clientFactory)
    {
        return Cache<T>.Instance.GetOrAdd(key, clientFactory);
    }

    public static T GetOrAddDisposable<T>(string key, Func<string, T> clientFactory)
        where T : IDisposable
    {
        if (Cache<T>.Instance.TryGetValue(key, out var value))
            return value;

        value = clientFactory(key);

        if (!Cache<T>.Instance.TryAdd(key, value))
        {
            value.Dispose();
            return Cache<T>.Instance[key];
        }

        return value;
    }

    public static async ValueTask<T> GetOrAddDisposableAsync<T>(string key, Func<string, ValueTask<T>> clientFactory)
        where T : IDisposable
    {
        if (Cache<T>.Instance.TryGetValue(key, out var value))
            return value;

        value = await clientFactory(key).ConfigureAwait(false);

        if (!Cache<T>.Instance.TryAdd(key, value))
        {
            value.Dispose();
            return Cache<T>.Instance[key];
        }

        return value;
    }

    public static async ValueTask<T> GetOrAddAsyncDisposableAsync<T>(string key, Func<string, T> clientFactory)
        where T : IAsyncDisposable
    {
        if (Cache<T>.Instance.TryGetValue(key, out var value))
            return value;

        value = clientFactory(key);

        if (!Cache<T>.Instance.TryAdd(key, value))
        {
            await value.DisposeAsync().ConfigureAwait(false);
            return Cache<T>.Instance[key];
        }

        return value;
    }

    public static async ValueTask<T> GetOrAddAsyncDisposableAsync<T>(
        string key,
        Func<string, ValueTask<T>> clientFactory)
        where T : IAsyncDisposable
    {
        if (Cache<T>.Instance.TryGetValue(key, out var value))
            return value;

        value = await clientFactory(key).ConfigureAwait(false);

        if (!Cache<T>.Instance.TryAdd(key, value))
        {
            await value.DisposeAsync().ConfigureAwait(false);
            return Cache<T>.Instance[key];
        }

        return value;
    }

    private static class Cache<T>
    {
        public static ConcurrentDictionary<string, T> Instance { get; } = new();
    }
}
