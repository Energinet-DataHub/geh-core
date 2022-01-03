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
            metaData.TryGetValue("_marketoperator", out var marketOperator);
            metaData.TryGetValue("_recipient", out var recipient);
            metaData.TryGetValue("_gln", out var gln);
            metaData.TryGetValue("_glnnumber", out var glnNumber);
            metaData.TryGetValue("_invocationid", out var invocationId);
            metaData.TryGetValue("_traceparent", out var traceParent);
            metaData.TryGetValue("_correlationid", out var correlationId);
            metaData.TryGetValue("_functionid", out var functionId);

            var time = SystemClock.Instance.GetCurrentInstant().ToString();
            string name = $"{marketOperator ?? string.Empty}-" +
                          $"{recipient ?? string.Empty}-" +
                          $"{gln ?? string.Empty}-" +
                          $"{glnNumber ?? string.Empty}-" +
                          $"{invocationId ?? string.Empty}-" +
                          $"{traceParent ?? string.Empty}-" +
                          $"{correlationId ?? string.Empty}-" +
                          $"{functionId ?? string.Empty}-" +
                          $"{time}";
            return name.Replace("--", "-");
        }

        internal static Func<string, string> MetaNameFormatter => s => s.Replace("-", string.Empty).ToLower();
    }
}
