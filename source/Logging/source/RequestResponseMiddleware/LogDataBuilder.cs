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
using System.Globalization;
using System.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using NodaTime;

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
                headerDataToSelect.Add("Authorization", "Bearer ****");
            }

            return headerDataToSelect;
        }

        public static (Dictionary<string, string> MetaData, Dictionary<string, string> IndexTags) BuildMetaDataAndIndexTagsDictionariesFromContext(
            FunctionContext context,
            bool isRequest)
        {
            var metaData = context.BindingContext.BindingData
                .Where(m =>
                    !string.IsNullOrWhiteSpace(m.Key) &&
                    !m.Key.Equals("headers", StringComparison.InvariantCultureIgnoreCase) &&
                    !m.Key.Equals("query", StringComparison.InvariantCultureIgnoreCase))
                .ToDictionary(e => MetaNameFormatter(e.Key), pair => pair.Value as string ?? string.Empty);

            // Add Query params to index tags, take 3 because max 10 index tags
            var indexTags = new Dictionary<string, string>(metaData.Take(3));

            AddBaseInfoFromContextToDictionary(context, isRequest, metaData);
            AddBaseInfoFromContextToDictionary(context, isRequest, indexTags);

            return (metaData, indexTags);
        }

        public static (string Name, string Folder) BuildLogName(Dictionary<string, string> metaData)
        {
            var time = SystemClock.Instance.GetCurrentInstant();
            var subfolder = time.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            string name = $"{LookUpInDictionary("functionname", metaData)}" +
                          $"{LookUpInDictionary("jwtactorid", metaData)}" +
                          $"{LookUpInDictionary("invocationid", metaData)}" +
                          $"{LookUpInDictionary("traceparent", metaData)}" +
                          $"{LookUpInDictionary("correlationid", metaData)}" +
                          $"{time.ToString("yyyy-MM-ddTHH-mm-ss'Z'", CultureInfo.InvariantCulture)}";

            return (name, subfolder);
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

        private static void AddBaseInfoFromContextToDictionary(
            FunctionContext context,
            bool isRequest,
            Dictionary<string, string> dictionary)
        {
            var jwtTokenActorId = JwtTokenParsing.ReadJwtActorId(context);
            var actorIdToWrite = string.IsNullOrWhiteSpace(jwtTokenActorId) ? "noactoridfound" : jwtTokenActorId;

            var traceParentParts = TraceParentSplit(context.TraceContext?.TraceParent ?? string.Empty);
            var traceId = traceParentParts?.Traceid;

            if (isRequest && !string.IsNullOrWhiteSpace(jwtTokenActorId))
            {
                dictionary.TryAdd(MetaNameFormatter("JwtActorId"), actorIdToWrite);
            }

            dictionary.TryAdd(MetaNameFormatter("FunctionId"), context.FunctionId);
            dictionary.TryAdd(MetaNameFormatter("FunctionName"), context.FunctionDefinition.Name);
            dictionary.TryAdd(MetaNameFormatter("InvocationId"), context.InvocationId);
            dictionary.TryAdd(MetaNameFormatter("TraceParent"), context.TraceContext?.TraceParent ?? string.Empty);
            dictionary.TryAdd(MetaNameFormatter("TraceId"), traceId ?? string.Empty);
            dictionary.TryAdd(MetaNameFormatter("HttpDataType"), isRequest ? "request" : "response");
        }
    }
}
