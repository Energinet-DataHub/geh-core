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
        }

        public string GetVersion(Assembly assembly)
        {
            var attribute = GetAttribute(assembly);
            return attribute == null
                ? string.Empty
                : ParseInformationalVersion(attribute);
        }

        internal static AssemblyInformationalVersionAttribute? GetAttribute(Assembly assembly)
        {
            return Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute))
                is AssemblyInformationalVersionAttribute versionAttribute
                ? versionAttribute
                : null;
        }

        /// <summary>
        /// Parse the value of the AssemblyInformationalVersionAttribute to an easy-to-read text.
        /// </summary>
        /// <returns>If the attribute contains our custom version information in the
        /// format 'version+PR_prNumber+SHA_sha' this returns a text like 'Version: *.*.* PR: * SHA: *';
        /// otherwise it returns any value specified in the reflected attribute.</returns>
        internal static string ParseInformationalVersion(AssemblyInformationalVersionAttribute attribute)
        {
            var sections = attribute.InformationalVersion
                .Replace("_", ": ")
                .Split('+');

            return sections.Length == 3
                ? $"Version: {sections[0]} PR: {sections[1]} SHA: {sections[2]}"
                : attribute.InformationalVersion;
        }
    }
}
