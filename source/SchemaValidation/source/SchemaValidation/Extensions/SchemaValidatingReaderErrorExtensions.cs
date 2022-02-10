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
using System.Xml;
using Energinet.DataHub.Core.SchemaValidation.Errors;

namespace Energinet.DataHub.Core.SchemaValidation.Extensions
{
    public static class SchemaValidatingReaderErrorExtensions
    {
        /// <summary>
        /// Converts the validation errors into a response suitable for returning to the caller.
        /// </summary>
        /// <param name="schemaValidatingReader">The reader containing the errors to convert into a response.</param>
        /// <returns>An error response that can be returned to the caller.</returns>
        public static ErrorResponse CreateErrorResponse(this ISchemaValidatingReader schemaValidatingReader)
        {
            return new ErrorResponse(schemaValidatingReader.Errors);
        }

        /// <summary>
        /// Writes the specified error response into the destination stream as XML.
        /// </summary>
        /// <param name="errorResponse">The error response to write.</param>
        /// <param name="destination">The stream to write the response into.</param>
        public static async Task WriteAsXmlAsync(this ErrorResponse errorResponse, Stream destination)
        {
            var xmlWriterSettings = new XmlWriterSettings
            {
                Async = true,
                Encoding = Encoding.UTF8,
                Indent = true,
            };

            await using var xmlWriter = XmlWriter.Create(destination, xmlWriterSettings);

            await xmlWriter.WriteStartDocumentAsync().ConfigureAwait(false);
            await errorResponse.WriteXmlContentsAsync(xmlWriter).ConfigureAwait(false);
            await xmlWriter.WriteEndDocumentAsync().ConfigureAwait(false);
            await xmlWriter.FlushAsync().ConfigureAwait(false);
        }
    }
}
