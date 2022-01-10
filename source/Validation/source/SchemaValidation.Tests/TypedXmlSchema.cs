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

namespace Energinet.DataHub.Core.SchemaValidation.Tests
{
    /// <summary>
    /// XML schema that specifies types under a <root></root> element.
    /// </summary>
    public sealed class TypedXmlSchema : IXmlSchema
    {
        private readonly XmlSchemaSet _anyRootAllowedSchemaSet;

        public TypedXmlSchema()
        {
            var typedSchema = @"<?xml version=""1.0"" encoding=""utf-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
 <xs:element name=""root"">
  <xs:complexType>
   <xs:sequence>
    <xs:element minOccurs=""0"" name=""typedAsDuration"" type=""xs:duration"" />
   </xs:sequence>
  </xs:complexType>
 </xs:element>
</xs:schema>";

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(typedSchema));
            var xmlSchema = XmlSchema.Read(stream, null);
            var xmlSchemaSet = new XmlSchemaSet();
            xmlSchemaSet.Add(xmlSchema!);
            _anyRootAllowedSchemaSet = xmlSchemaSet;
        }

        public Task<XmlSchemaSet> GetXmlSchemaSetAsync()
        {
            return Task.FromResult(_anyRootAllowedSchemaSet);
        }
    }
}
