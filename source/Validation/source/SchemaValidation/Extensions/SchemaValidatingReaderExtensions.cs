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

namespace Energinet.DataHub.Core.SchemaValidation.Extensions
{
    public static class SchemaValidatingReaderExtensions
    {
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
    }
}
