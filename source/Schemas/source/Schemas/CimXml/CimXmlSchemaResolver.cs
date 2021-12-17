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
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Energinet.DataHub.Core.Schemas.CimXml
{
    internal sealed class CimXmlSchemaResolver
    {
        private static readonly Assembly _currentAssembly = Assembly.GetExecutingAssembly();

        public Task<Stream> ResolveAsync(string? resourceName)
        {
            var resourceStream = _currentAssembly.GetManifestResourceStream($"Energinet.DataHub.Core.Schemas.CimXml.Resources.{resourceName}");
            if (resourceStream == null)
            {
                throw new XmlSchemaException($"Could not resolve XML Schema named {resourceName}.");
            }

            return Task.FromResult(resourceStream);
        }
    }
}
