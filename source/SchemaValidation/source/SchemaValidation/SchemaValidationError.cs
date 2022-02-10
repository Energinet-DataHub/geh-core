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
    public readonly struct SchemaValidationError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaValidationError"/> struct.
        /// </summary>
        /// <param name="lineNumber">The line number at which the error occurred.</param>
        /// <param name="linePosition">The line position at which the error occurred.</param>
        /// <param name="description">The description of the occurred error.</param>
        public SchemaValidationError(int lineNumber, int linePosition, string description)
        {
            LineNumber = lineNumber;
            LinePosition = linePosition;
            Description = description;
        }

        /// <summary>
        /// Gets the line number at which the error occurred.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Gets the line position at which the error occurred.
        /// </summary>
        public int LinePosition { get; }

        /// <summary>
        /// Gets the description of the occurred error.
        /// </summary>
        public string Description { get; }
    }
}
