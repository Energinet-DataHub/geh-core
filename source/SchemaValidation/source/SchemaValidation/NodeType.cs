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

namespace Energinet.DataHub.Core.SchemaValidation
{
    public enum NodeType
    {
        /// <summary>
        /// The reader is not positioned at a node. Occurs before reading has begun or once the reader has read to end.
        /// </summary>
        None,

        /// <summary>
        /// The current node is a start element.
        /// </summary>
        StartElement,

        /// <summary>
        /// The current node is an end element.
        /// </summary>
        EndElement,

        /// <summary>
        /// The current node is an attribute node.
        /// </summary>
        Attribute,
    }
}
