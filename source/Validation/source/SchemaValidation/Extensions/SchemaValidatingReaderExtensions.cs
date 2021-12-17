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
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Energinet.DataHub.Core.SchemaValidation.Extensions
{
    public static class SchemaValidatingReaderExtensions
    {
        /// <summary>
        /// Advances the reader to the next node, skipping EndElements, and returns true.
        /// If the next node is a EndElement with the specified name, stops and returns false.
        /// </summary>
        /// <param name="reader">The reader to advance.</param>
        /// <param name="nodeName">The name of the EndElement to stop advancing at.</param>
        /// <returns>Returns true until the specified EndElement is found; false otherwise.</returns>
        public static async Task<bool> AdvanceUntilClosedAsync(this ISchemaValidatingReader reader, string nodeName)
        {
            while (await reader.AdvanceAsync().ConfigureAwait(false))
            {
                if (reader.CurrentNodeType == NodeType.EndElement)
                {
                    if (reader.CurrentNodeName.Equals(nodeName, StringComparison.Ordinal))
                    {
                        break;
                    }

                    if (reader.CanReadValue)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Do not use this method, it is provided for compatibility.
        /// Uses the specified reader to validate and read XML into an XElement.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>A loaded XElement.</returns>
        public static Task<XElement?> AsXElementAsync(this SchemaValidatingReader reader)
        {
            var innerReader = reader.GetXmlValidatingReader();
            return innerReader.ReadIntoXElementAsync();
        }

        /// <summary>
        /// Do not use this method, it is provided for compatibility.
        /// Provides access to the underlying asynchronous XmlReader.
        /// Schema validation errors are still stored in the SchemaValidatingReader,
        /// but do note that malformed XML errors are not and will be thrown as XmlException from the returned XmlReader.
        /// Do not use anything from SchemaValidatingReader, except error-related functionality.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The internal XmlReader.</returns>
        public static Task<XmlReader> AsXmlReaderAsync(this SchemaValidatingReader reader)
        {
            var innerReader = reader.GetXmlValidatingReader();
            return innerReader.GetInternalReaderAsync();
        }
    }
}
