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

namespace Energinet.DataHub.Core.App.FunctionApp.Middleware.IntegrationEventContext
{
    /// <summary>
    /// Describes an integration event.
    /// </summary>
    public interface IIntegrationEventContext
    {
        /// <summary>
        /// The metadata that travels along with the message when it is sent as an integration event.
        /// </summary>
        public IntegrationEventMetadata EventMetadata { get; }

        /// <summary>
        /// Sets the metadata properties of the received event.
        /// </summary>
        void SetMetadata(string messageType, Instant operationTimeStamp);
    }
}
