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
using System.Threading.Tasks;
using System.Xml;

namespace Energinet.DataHub.Core.SchemaValidation.Errors
{
    public readonly struct ErrorResponse
    {
        public ErrorResponse(IEnumerable<SchemaValidationError> validationErrors)
        {
            var errors = validationErrors
                .Select(x => new Error(x))
                .ToList();

            if (errors.Count == 0)
            {
                throw new InvalidOperationException("The list of validation errors must not be empty.");
            }

            Error = new Error(errors);
        }

        public Error Error { get; }

        internal async Task WriteXmlContentsAsync(XmlWriter writer)
        {
            await Error.WriteXmlContentsAsync(writer).ConfigureAwait(false);
        }
    }
}
