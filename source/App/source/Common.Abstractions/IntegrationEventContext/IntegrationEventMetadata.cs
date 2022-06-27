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

using NodaTime;

namespace Energinet.DataHub.Core.App.Common.Abstractions.IntegrationEventContext
{
    /// <summary>
    /// The metadata that travels along with the message when it is sent as an integration event.
    /// </summary>
    public sealed class IntegrationEventMetadata
    {
        public IntegrationEventMetadata(
            string messageType,
            Instant operationTimestamp,
            string operationCorrelationId)
        {
            OperationCorrelationId = operationCorrelationId;
            MessageType = messageType;
            OperationTimestamp = operationTimestamp;
        }

        /// <summary>
        /// The name of message/event.
        /// See: https://github.com/Energinet-DataHub/green-energy-hub/blob/main/docs/architecture-decision-record/ADR-0008%20Integration%20events.md#message-type
        /// </summary>
        public string MessageType { get; }

        /// <summary>
        /// Represents the point in time, when the sending application created the event.
        /// See: https://github.com/Energinet-DataHub/green-energy-hub/blob/main/docs/architecture-decision-record/ADR-0008%20Integration%20events.md#timestamp
        /// </summary>
        public Instant OperationTimestamp { get; }

        /// <summary>
        /// Represents the id for the current operation.
        /// See: https://github.com/Energinet-DataHub/green-energy-hub/blob/main/docs/architecture-decision-record/ADR-0008%20Integration%20events.md#correlationid
        /// </summary>
        public string OperationCorrelationId { get; }
    }
}
