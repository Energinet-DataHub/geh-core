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
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using Energinet.DataHub.Core.SchemaValidation;

namespace SchemaValidation.Tests
{
    /// <summary>
    /// XML schema that allows everything under a <root></root> element, e.g. <root><anytag /></root>.
    /// </summary>
    internal sealed class RootXmlSchema : IXmlSchema
    {
        private readonly XmlSchema _anyRootAllowed;

        public RootXmlSchema(string rootName = "root")
        {
            var anyRootSchema = @$"<?xml version=""1.0"" encoding=""utf-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:element name=""{rootName}"">
    <xs:complexType>
      <xs:sequence>
        <xs:any processContents=""skip"" minOccurs=""0"" maxOccurs=""unbounded""/>
      </xs:sequence>
      <xs:anyAttribute processContents=""skip""/>
    </xs:complexType>
  </xs:element>
</xs:schema>";

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(anyRootSchema));
            var xmlSchema = XmlSchema.Read(stream, null);
            _anyRootAllowed = xmlSchema!;
        }

        public Task<XmlSchema> GetXmlSchemaAsync()
        {
            return Task.FromResult(_anyRootAllowed);
        }
    }
}
