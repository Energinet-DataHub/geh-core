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

using System.Diagnostics.CodeAnalysis;

namespace Energinet.DataHub.Core.App.FunctionApp.FunctionTelemetryScope
{
    /// <summary>
    /// Implementation of W3C Trace Context ('traceparent' header).
    /// </summary>
    /// <remarks>
    /// For now, the implementation doesn't handle all validation nor uses flags.
    /// Specification can be found here: https://www.w3.org/TR/trace-context/#trace-id
    /// </remarks>
    internal sealed class TraceParent
    {
        private TraceParent(string traceId, string parentId, bool isValid)
        {
            TraceId = traceId;
            ParentId = parentId;
            IsValid = isValid;
        }

        public string TraceId { get; }

        public string ParentId { get; }

        public bool IsValid { get; }

        public static TraceParent Parse(string traceParent)
        {
            if (string.IsNullOrWhiteSpace(traceParent)) return Invalid();

            // 55 is the valid length of trace context.
            // An example looks like this: 00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01
            if (traceParent.Length != 55) return Invalid();

            var parts = traceParent.Split('-');

            // Trace context is made up of four parts: version-format, trace-id, parent-id and trace-flags.
            if (parts.Length != 4) return Invalid();

            var version = parts[0];
            var traceId = parts[1];
            var parentId = parts[2];

            if (version != "00") return Invalid();

            // 32 is the valid length of trace-id
            if (traceId.Length != 32) return Invalid();

            // 16 is the valid length of parent-id
            if (parentId.Length != 16) return Invalid();

            return Create(traceId, parentId);
        }

        private static TraceParent Create(string traceId, string parentId)
        {
            return new TraceParent(
                traceId,
                parentId,
                true);
        }

        private static TraceParent Invalid()
        {
            return new TraceParent(
                string.Empty,
                string.Empty,
                false);
        }
    }
}
