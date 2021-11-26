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

using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Xml.Schema;
using Energinet.DataHub.Core.SchemaValidation;

namespace Energinet.DataHub.Core.Schemas.CimXml
{
    internal sealed class CimXmlSchemaContainer : IXmlSchema
    {
        private static readonly CimXmlSchemaResolver _xmlResolver = new ();
        private static readonly ConcurrentDictionary<string, Task<XmlSchema>> _schemaCache = new ();

        private readonly string _resourceName;

        public CimXmlSchemaContainer(string resourceName)
        {
            _resourceName = resourceName;
        }

        public Task<XmlSchema> GetXmlSchemaAsync()
        {
            return LoadSchemaRecusivelyAsync(_resourceName);
        }

        private static Task<XmlSchema> GetXmlSchemaAsync(string location)
        {
            return _schemaCache.GetOrAdd(location, LoadSchemaRecusivelyAsync);
        }

        private static async Task<XmlSchema> LoadSchemaRecusivelyAsync(string location)
        {
            var schemaStream = await _xmlResolver
                .ResolveAsync(location)
                .ConfigureAwait(false);

            // Read(..) cannot return null if EventHandler is null.
            // EventHandler is null, because we treat schema loading errors as code errors.
            var xmlSchema = XmlSchema.Read(schemaStream, null);

            foreach (XmlSchemaExternal external in xmlSchema!.Includes)
            {
                if (external.SchemaLocation == null)
                {
                    continue;
                }

                external.Schema = await GetXmlSchemaAsync(external.SchemaLocation).ConfigureAwait(false);
            }

            return xmlSchema;
        }
    }
}
