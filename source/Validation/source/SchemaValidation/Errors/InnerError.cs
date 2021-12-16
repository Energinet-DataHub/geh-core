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

using System.Globalization;
using System.Threading.Tasks;
using System.Xml;

namespace Energinet.DataHub.Core.SchemaValidation.Errors
{
    public readonly struct InnerError
    {
        public InnerError(int lineNumber, int linePosition)
        {
            LineNumber = lineNumber;
            LinePosition = linePosition;
        }

        public int LineNumber { get; }

        public int LinePosition { get; }

        internal async Task WriteXmlContentsAsync(XmlWriter writer)
        {
            await writer.WriteStartElementAsync(null, "InnerError", null).ConfigureAwait(false);

            var lineNumberValue = LineNumber.ToString(CultureInfo.InvariantCulture);
            await writer.WriteElementStringAsync(null, "LineNumber", null, lineNumberValue).ConfigureAwait(false);

            var linePositionValue = LinePosition.ToString(CultureInfo.InvariantCulture);
            await writer.WriteElementStringAsync(null, "LinePosition", null, linePositionValue).ConfigureAwait(false);

            await writer.WriteEndElementAsync().ConfigureAwait(false);
        }
    }
}
