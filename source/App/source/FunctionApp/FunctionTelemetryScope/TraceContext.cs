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
    /// Implementation of w3c trace context
    /// </summary>
    /// <remarks>
    /// For now, the implementation doesn't handle all validation nor uses version or flags.
    /// Specification can be found here: https://www.w3.org/TR/trace-context/#trace-id
    /// </remarks>
    internal sealed class TraceContext
    {
        private TraceContext(string traceId, string parentId, bool isValid)
        {
            TraceId = traceId;
            ParentId = parentId;
            IsValid = isValid;
        }

        public string TraceId { get; }

        public string ParentId { get; }

        public bool IsValid { get; }

        public static TraceContext Parse(string traceContext)
        {
            if (string.IsNullOrWhiteSpace(traceContext)) return Invalid();

            // 55 is the valid length of trace context.
            // An example looks like this: 00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01
            if (traceContext.Length != 55) return Invalid();

            var parts = traceContext.Split('-');

            // Trace context is made up of four parts: version-format, trace-id, parent-id and trace-flags.
            if (parts.Length != 4) return Invalid();

            var traceId = parts[1];
            var parentId = parts[2];

            // 32 is the valid length of trace-id
            if (traceId.Length != 32) return Invalid();

            // 16 is the valid length of parent-id
            if (parentId.Length != 16) return Invalid();

            return Create(traceId, parentId);
        }

        private static TraceContext Create(string traceId, string parentId)
        {
            return new TraceContext(
                traceId,
                parentId,
                true);
        }

        private static TraceContext Invalid()
        {
            return new TraceContext(
                string.Empty,
                string.Empty,
                false);
        }
    }
}
