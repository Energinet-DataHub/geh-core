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

            var metaData = headersCollection
                .ToDictionary(e => e.Key, e => string.Join(",", e.Value));

            return metaData;
        }

        public static (Dictionary<string, string> MetaData, Dictionary<string, string> IndexTags) GetMetaDataAndIndexTagsDictionaries(FunctionContext context, bool isRequest)
        {
            var metaData = context.BindingContext.BindingData
                .ToDictionary(e => MetaNameFormatter(e.Key), pair => pair.Value as string ?? string.Empty);

            var indexTags =
                new Dictionary<string, string>(metaData.Where(e => e.Key != "headers" && e.Key != "query").Take(4));

            var jwtTokenGln = JwtTokenParsing.ReadJwtGln(context);
            var glnToWrite = string.IsNullOrWhiteSpace(jwtTokenGln) ? "nojwtgln" : jwtTokenGln;

            metaData.TryAdd(MetaNameFormatter("JwtGln"), glnToWrite);
            metaData.TryAdd(MetaNameFormatter("FunctionId"), context.FunctionId);
            metaData.TryAdd(MetaNameFormatter("FunctionName"), context.FunctionDefinition.Name);
            metaData.TryAdd(MetaNameFormatter("InvocationId"), context.InvocationId);
            metaData.TryAdd(MetaNameFormatter("TraceContext"), context.TraceContext?.TraceParent ?? string.Empty);
            metaData.TryAdd(MetaNameFormatter("HttpDataType"), isRequest ? "request" : "response");

            indexTags.TryAdd(MetaNameFormatter("JwtGln"), glnToWrite);
            indexTags.TryAdd(MetaNameFormatter("FunctionName"), context.FunctionDefinition.Name);
            indexTags.TryAdd(MetaNameFormatter("InvocationId"), context.InvocationId);
            indexTags.TryAdd(MetaNameFormatter("TraceContext"), context.TraceContext?.TraceParent ?? string.Empty);
            indexTags.TryAdd(MetaNameFormatter("HttpDataType"), isRequest ? "request" : "response");

            return (metaData, indexTags);
        }

        public static (string Name, string Folder) BuildLogName(Dictionary<string, string> metaData)
        {
            var time = SystemClock.Instance.GetCurrentInstant();
            var subfolder = time.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            string name = $"{LookUpInDictionary("functionname", metaData)}" +
                          $"{LookUpInDictionary("jwtgln", metaData)}" +
                          $"{LookUpInDictionary("invocationid", metaData)}" +
                          $"{LookUpInDictionary("traceparent", metaData)}" +
                          $"{LookUpInDictionary("correlationid", metaData)}" +
                          $"{time.ToString()}";

            return (name, subfolder);
        }

        internal static Func<string, string> MetaNameFormatter => s => s.Replace("-", string.Empty).ToLower();

        private static Func<string, Dictionary<string, string>, string> LookUpInDictionary
            => (n, d) =>
                d.TryGetValue(n, out var value)
                    ? string.IsNullOrWhiteSpace(value) ? string.Empty : $"{value}_"
                    : string.Empty;
    }
}
