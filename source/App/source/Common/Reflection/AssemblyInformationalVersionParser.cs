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
using System.Reflection;

namespace Energinet.DataHub.Core.App.Common.Reflection
{
    public class AssemblyInformationalVersionParser : IAssemblyInformationalVersionParser
    {
        public AssemblyInformationalVersionParser()
        {
            Version = ParseInformationalVersion(GetAttribute());
        }

        public string Version { get; }

        private static AssemblyInformationalVersionAttribute? GetAttribute()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                if (Attribute.GetCustomAttribute(entryAssembly, typeof(AssemblyInformationalVersionAttribute))
                    is AssemblyInformationalVersionAttribute versionAttribute)
                {
                    return versionAttribute;
                }
            }

            return null;
        }

        /// <summary>
        /// Parse the value of the AssemblyInformationalVersionAttribute to an easy-to-read text.
        /// </summary>
        /// <param name="attribute">If this .NET assembly was builded using our 'dotnet-build-prerelease.yml' workflow
        /// then this attribute will contain information in the format 'version+PR_prNumber+SHA_sha'.</param>
        /// <returns>A text like 'Version: *.*.* PR: * SHA: *'; otherwise it returns any value
        /// specified in the reflected attribute, or an empty string if the attribute is not available.</returns>
        private static string ParseInformationalVersion(AssemblyInformationalVersionAttribute? attribute)
        {
            if (attribute != null)
            {
                var sections = attribute.InformationalVersion
                    .Replace("_", ": ")
                    .Split('+');
                if (sections.Length == 3)
                {
                    return $"Version: {sections[0]} PR: {sections[1]} SHA: {sections[2]}";
                }
                else
                {
                    return attribute.InformationalVersion;
                }
            }

            return string.Empty;
        }
    }
}
