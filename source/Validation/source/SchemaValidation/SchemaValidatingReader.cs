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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Schema;
using Energinet.DataHub.Core.SchemaValidation.Xml;
using NodaTime;

namespace Energinet.DataHub.Core.SchemaValidation
{
    [DebuggerDisplay("Node {CurrentNodeName}, Type: {CurrentNodeType}, HasValue: {CanReadValue}, HasErrors: {HasErrors}")]
    public sealed class SchemaValidatingReader : ISchemaValidatingReader
    {
        private readonly ISourceValidatingReader _sourceReader;

        public SchemaValidatingReader(Stream stream, IXmlSchema validationSchema, params IXmlSchema[] additionalSchemas)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (validationSchema == null)
            {
                throw new ArgumentNullException(nameof(validationSchema));
            }

            _sourceReader = new XmlSourceValidatingReader(stream, additionalSchemas.Prepend(validationSchema));
        }

        public string CurrentNodeName => _sourceReader.CurrentNodeName;

        public NodeType CurrentNodeType => _sourceReader.CurrentNodeType;

        public bool CanReadValue => _sourceReader.CanReadValue;

        public bool HasErrors => _sourceReader.HasErrors;

        public IReadOnlyList<SchemaValidationError> Errors => _sourceReader.Errors;

        public Task<bool> AdvanceAsync()
        {
            return _sourceReader.AdvanceAsync();
        }

        public Task<string> ReadValueAsStringAsync()
        {
            return _sourceReader.ReadValueAsStringAsync();
        }

        public Task<int> ReadValueAsIntAsync()
        {
            return _sourceReader.ReadValueAsIntAsync();
        }

        public Task<long> ReadValueAsLongAsync()
        {
            return _sourceReader.ReadValueAsLongAsync();
        }

        public Task<bool> ReadValueAsBoolAsync()
        {
            return _sourceReader.ReadValueAsBoolAsync();
        }

        public Task<decimal> ReadValueAsDecimalAsync()
        {
            return _sourceReader.ReadValueAsDecimalAsync();
        }

        public Task<Instant> ReadValueAsNodaTimeAsync()
        {
            return _sourceReader.ReadValueAsNodaTimeAsync();
        }
    }
}
