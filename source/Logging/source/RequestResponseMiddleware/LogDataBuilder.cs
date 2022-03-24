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
using System.Linq;
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Energinet.DataHub.Core.Logging.RequestResponseMiddleware
{
    internal static class LogDataBuilder
    {
        public static Dictionary<string, string> ReadHeaderDataFromCollection(HttpHeadersCollection headersCollection)
        {
            if (headersCollection is null)
            {
                return new Dictionary<string, string>();
            }

            var headerDataToSelect = headersCollection
                .Where(m =>
                    !string.IsNullOrWhiteSpace(m.Key) &&
                    !m.Key.Equals("authorization", StringComparison.InvariantCultureIgnoreCase) &&
                    !m.Key.Equals("headers", StringComparison.InvariantCultureIgnoreCase))
                .ToDictionary(e => e.Key, e => string.Join(",", e.Value));

            if (headersCollection.Any(e => e.Key.Equals("authorization", StringComparison.InvariantCultureIgnoreCase)))
            {
                headerDataToSelect.TryAdd("Authorization", "Bearer ****");
            }

            if (!headersCollection.Any(e => e.Key.Equals(IndexTagsKeys.CorrelationId, StringComparison.InvariantCultureIgnoreCase)))
            {
                headerDataToSelect.TryAdd(IndexTagsKeys.CorrelationId, "no correlation id found");
            }

            return headerDataToSelect;
        }

        public static LogTags BuildDictionaryFromContext(
            FunctionContext context,
            bool isRequest)
        {
            var bindingdata = context.BindingContext.BindingData
                .Where(m =>
                    !string.IsNullOrWhiteSpace(m.Key) &&
                    !m.Key.Equals("headers", StringComparison.InvariantCultureIgnoreCase) &&
                    !m.Key.Equals("query", StringComparison.InvariantCultureIgnoreCase))
                .ToDictionary(e => MetaNameFormatter(e.Key), pair => pair.Value as string ?? string.Empty);

            var logTags = new LogTags();
            logTags.AddContextTagsCollection(AddBaseInfoFromContextToDictionary(context, isRequest));
            logTags.AddQueryTagsCollection(bindingdata);

            return logTags;
        }

        public static string BuildLogName()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// https://w3c.github.io/trace-context/#trace-context-http-request-headers-format
        /// </summary>
        /// <returns>TraceParent parts or null on parse error</returns>
        public static (string Version, string Traceid, string Spanid, string Traceflags)? TraceParentSplit(string traceParent)
        {
            var traceSpilt = traceParent.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (traceSpilt.Length == 4)
            {
                return (traceSpilt[0], traceSpilt[1], traceSpilt[2], traceSpilt[3]);
            }

            return null;
        }

        internal static Func<string, string> MetaNameFormatter => s => s.Replace("-", string.Empty).ToLower();

        private static Func<string, Dictionary<string, string>, string> LookUpInDictionary
            => (n, d) =>
                d.TryGetValue(n, out var value)
                    ? string.IsNullOrWhiteSpace(value) ? string.Empty : $"{value}_"
                    : string.Empty;

        private static Dictionary<string, string> AddBaseInfoFromContextToDictionary(
            FunctionContext context,
            bool isRequest)
        {
            var jwtTokenActorId = JwtTokenParsing.ReadJwtActorId(context);
            var actorIdToWrite = string.IsNullOrWhiteSpace(jwtTokenActorId) ? "noactoridfound" : jwtTokenActorId;

            var traceParentParts = TraceParentSplit(context.TraceContext?.TraceParent ?? string.Empty);
            var traceId = traceParentParts?.Traceid;

            var dictionary = new Dictionary<string, string>();
            dictionary.TryAdd(MetaNameFormatter(IndexTagsKeys.JwtActorId), actorIdToWrite);
            dictionary.TryAdd(MetaNameFormatter(IndexTagsKeys.FunctionId), context.FunctionId);
            dictionary.TryAdd(MetaNameFormatter(IndexTagsKeys.FunctionName), context.FunctionDefinition.Name);
            dictionary.TryAdd(MetaNameFormatter(IndexTagsKeys.InvocationId), context.InvocationId);
            dictionary.TryAdd(MetaNameFormatter(IndexTagsKeys.TraceParent), context.TraceContext?.TraceParent ?? string.Empty);
            dictionary.TryAdd(MetaNameFormatter(IndexTagsKeys.TraceId), traceId ?? string.Empty);
            dictionary.TryAdd(MetaNameFormatter(IndexTagsKeys.HttpDataType), isRequest ? "request" : "response");
            return dictionary;
        }
    }
}
