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

using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;

namespace Energinet.DataHub.Core.SchemaValidation
{
    /// <summary>
    /// A single-pass validating reader for DFS-traversal of a document.
    /// </summary>
    public interface ISchemaValidatingReader
    {
        /// <summary>
        /// Gets the name of the current node.
        /// Contains the element name if CurrentNodeType is StartElement or EndElement.
        /// Contains attribute name if CurrentNodeType is Attribute.
        /// An empty string otherwise.
        /// </summary>
        string CurrentNodeName { get; }

        /// <summary>
        /// Gets the type of the current node.
        /// </summary>
        NodeType CurrentNodeType { get; }

        /// <summary>
        /// Gets a value indicating whether the value of the current node can be read.
        /// Note that this does not indicate whether the value is valid in given context; an empty string can still be returned,
        /// i.e. attribute="" will have CanReadValue=true but ReadValueAsStringAsync will return string.Empty.
        /// </summary>
        bool CanReadValue { get; }

        /// <summary>
        /// Gets a value indicating whether there are validation errors.
        /// </summary>
        bool HasErrors { get; }

        /// <summary>
        /// Gets a list of validation errors found.
        /// </summary>
        IReadOnlyList<SchemaValidationError> Errors { get; }

        /// <summary>
        /// Advances the reader to the next node.
        /// If a validation error occurs, the method will read to end and return false.
        /// </summary>
        /// <returns>Returns true if there is a next node; false if read to end or a validation error occurred.</returns>
        Task<bool> AdvanceAsync();

        /// <summary>
        /// Reads the current value as a string.
        /// </summary>
        /// <returns>A string representation of the current node value.</returns>
        Task<string> ReadValueAsStringAsync();

        /// <summary>
        /// Reads the current value as a xs:duration type.
        /// </summary>
        /// <returns>A string representation of the current node value.</returns>
        Task<string> ReadValueAsDurationAsync();

        /// <summary>
        /// Reads the current value as an int.
        /// </summary>
        /// <returns>An int representation of the current node value.</returns>
        Task<int> ReadValueAsIntAsync();

        /// <summary>
        /// Reads the current value as a long.
        /// </summary>
        /// <returns>A long representation of the current node value.</returns>
        Task<long> ReadValueAsLongAsync();

        /// <summary>
        /// Reads the current value as a boolean.
        /// </summary>
        /// <returns>A boolean representation of the current node value.</returns>
        Task<bool> ReadValueAsBoolAsync();

        /// <summary>
        /// Reads the current value as a decimal.
        /// </summary>
        /// <returns>A decimal representation of the current node value.</returns>
        Task<decimal> ReadValueAsDecimalAsync();

        /// <summary>
        /// Reads the current value as an <see cref="Instant"/>.
        /// </summary>
        /// <returns>An <see cref="Instant"/> representation of the current node value.</returns>
        Task<Instant> ReadValueAsNodaTimeAsync();
    }
}
