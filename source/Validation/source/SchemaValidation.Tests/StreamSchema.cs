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

using System.IO;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Energinet.DataHub.Core.SchemaValidation.Tests
{
    public sealed class StreamSchema : IXmlSchema
    {
        private readonly Stream _schemaStream;

        public StreamSchema(Stream schemaStream)
        {
            _schemaStream = schemaStream;
        }

        public Task<XmlSchema> GetXmlSchemaAsync()
        {
            var xmlSchema = XmlSchema.Read(_schemaStream, null);
            return Task.FromResult(xmlSchema!);
        }
    }
}
