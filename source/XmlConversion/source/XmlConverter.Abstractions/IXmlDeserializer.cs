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
using System.Xml.Linq;

namespace Energinet.DataHub.Core.XmlConversion.XmlConverter.Abstractions
{
    /// <summary>
    /// XML deserializer
    /// </summary>
    public interface IXmlDeserializer
    {
        /// <summary>
        /// Deserializes an EDI message in XML format
        /// </summary>
        /// <param name="body"></param>
        /// <returns>An XML deserialization result <seealso cref="XmlDeserializationResult"/></returns>
        public Task<XmlDeserializationResult> DeserializeAsync(Stream body);

        /// <summary>
        /// Deserializes an EDI message in XML format
        /// </summary>
        /// <param name="rootElement"></param>
        /// <returns>An XML deserialization result <seealso cref="XmlDeserializationResult"/></returns>
        XmlDeserializationResult Deserialize(XElement rootElement);
    }
}
