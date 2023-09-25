﻿// Copyright 2020 Energinet DataHub A/S
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

namespace Energinet.DataHub.Core.App.Common.Reflection
{
    /// <summary>
    /// Responsible for reflecting and parsing the value of the <see cref="AssemblyInformationalVersionAttribute"/>.
    /// Our workflow in GitHub adds custom version information to this attribute, so we can identify
    /// the exact code version that aws used to build the assembly.
    /// </summary>
    public interface IAssemblyInformationalVersionParser
    {
        /// <summary>
        /// If the assembly contains our custom version information this will return a text
        /// like 'Version: *.*.* PR: * SHA: *'; otherwise it returns any value specified in
        /// the reflected attribute, or an empty string if the attribute is not available.
        /// </summary>
        string GetVersion(Assembly assembly);
    }
}
