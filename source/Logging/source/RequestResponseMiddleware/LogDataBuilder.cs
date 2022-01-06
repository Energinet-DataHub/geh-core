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
using System.Text;
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

        public static string BuildLogName(Dictionary<string, string> metaData)
        {
            var time = SystemClock.Instance.GetCurrentInstant().ToString();

            string name = $"{LookUpInDictionary("marketoperator", metaData)}" +
                          $"{LookUpInDictionary("recipient", metaData)}" +
                          $"{LookUpInDictionary("gln", metaData)}" +
                          $"{LookUpInDictionary("glnnumber", metaData)}" +
                          $"{LookUpInDictionary("invocationid", metaData)}" +
                          $"{LookUpInDictionary("traceparent", metaData)}" +
                          $"{LookUpInDictionary("correlationid", metaData)}" +
                          $"{LookUpInDictionary("functionid", metaData)}" +
                          $"{time}";
            return name;
        }

        internal static Func<string, string> MetaNameFormatter => s => s.Replace("-", string.Empty).ToLower();

        private static Func<string, Dictionary<string, string>, string> LookUpInDictionary
            => (n, d) => d.TryGetValue(n, out var value) ? $"{value}_" : string.Empty;
    }
}
