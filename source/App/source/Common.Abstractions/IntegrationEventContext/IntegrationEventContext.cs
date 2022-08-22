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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using NodaTime;

namespace Energinet.DataHub.Core.App.Common.Abstractions.IntegrationEventContext
{
    public sealed class IntegrationEventContext : IIntegrationEventContext
    {
        private IntegrationEventMetadata? _eventMetadata;

        public IntegrationEventMetadata ReadMetadata()
        {
            var eventMetadata = _eventMetadata ?? throw new InvalidOperationException("Metadata for integration event has not been set.");

            VerifyExists(eventMetadata.MessageType);
            VerifyExists(eventMetadata.OperationTimestamp);
            VerifyExists(eventMetadata.OperationCorrelationId);

            return eventMetadata;
        }

        public bool TryReadMetadata([NotNullWhen(true)] out IntegrationEventMetadata? metadata)
        {
            metadata = _eventMetadata;
            return _eventMetadata != null;
        }

        public void SetMetadata(string messageType, Instant operationTimeStamp, string operationCorrelationId)
        {
            _eventMetadata = new IntegrationEventMetadata(messageType, operationTimeStamp, operationCorrelationId);
        }

        private static void VerifyExists(string value, [CallerArgumentExpression("value")] string? paramName = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"{paramName} metadata is missing.");
            }
        }

        private static void VerifyExists(Instant value, [CallerArgumentExpression("value")] string? paramName = null)
        {
            if (value == default)
            {
                throw new InvalidOperationException($"{paramName} metadata is missing.");
            }
        }
    }
}
