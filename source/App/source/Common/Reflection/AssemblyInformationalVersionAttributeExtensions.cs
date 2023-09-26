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

using System.Reflection;
using System.Text.RegularExpressions;

namespace Energinet.DataHub.Core.App.Common.Reflection
{
    public static class AssemblyInformationalVersionAttributeExtensions
    {
        /// <summary>
        /// Our workflow in GitHub adds DH3 source version information to the <see cref="AssemblyInformationalVersionAttribute"/>
        /// during build. This method parses the value of the attribute to an <see cref="SourceVersionInformation"/>.
        /// If the attribute contains the full DH3 source version information in the format '[productVersion]+PR_[prNumber]+SHA_[sha]'
        /// then all properties of <see cref="SourceVersionInformation"/> will be used; otherwise the raw attribute value is
        /// set as <see cref="SourceVersionInformation.ProductVersion"/>.
        /// </summary>
        public static SourceVersionInformation GetSourceVersionInformation(this AssemblyInformationalVersionAttribute attribute)
        {
            var sourceVersionMatchPattern = @"^(.+)\+PR_(.+)\+SHA_(.+)$";
            var match = Regex.Match(attribute.InformationalVersion, sourceVersionMatchPattern);
            return match.Success
                ? new SourceVersionInformation(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value)
                : new SourceVersionInformation(attribute.InformationalVersion);
        }
    }
}
