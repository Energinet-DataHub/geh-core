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

namespace SchemaValidation.Tests.Examples
{
    internal sealed class BookstoreExampleSchema : IXmlSchema
    {
        private const string ExampleSchemaXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<xs:schema attributeFormDefault=""unqualified"" elementFormDefault=""qualified"" targetNamespace=""http://www.contoso.com/books"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""bookstore"">
        <xs:complexType>
            <xs:sequence>
                <xs:element maxOccurs=""unbounded"" name=""book"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""title"" type=""xs:string"" />
                            <xs:element name=""author"">
                                <xs:complexType>
                                    <xs:sequence>
                                        <xs:element minOccurs=""0"" name=""name"" type=""xs:string"" />
                                        <xs:element minOccurs=""0"" name=""first-name"" type=""xs:string"" />
                                        <xs:element minOccurs=""0"" name=""last-name"" type=""xs:string"" />
                                    </xs:sequence>
                                </xs:complexType>
                            </xs:element>
                            <xs:element name=""price"" type=""xs:decimal"" />
                        </xs:sequence>
                        <xs:attribute name=""genre"" type=""xs:string"" use=""required"" />
                        <xs:attribute name=""publicationdate"" type=""xs:date"" use=""required"" />
                        <xs:attribute name=""ISBN"" type=""xs:string"" use=""required"" />
                    </xs:complexType>
                </xs:element>
            </xs:sequence>
        </xs:complexType>
    </xs:element>
</xs:schema>";

        public async Task<XmlSchema> GetXmlSchemaAsync()
        {
            await using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(ExampleSchemaXml));
            var xmlSchema = XmlSchema.Read(memoryStream, null);
            return xmlSchema!;
        }
    }
}
