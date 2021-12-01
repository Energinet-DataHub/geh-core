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
using System.IO;
using System.Reflection;

namespace Energinet.DataHub.Core.SchemaValidation.Tests.Examples
{
    public static class ExampleResources
    {
        private static readonly Assembly _currentAssembly = Assembly.GetExecutingAssembly();

        public static Stream BookstoreXml =>
            GetStream("BookstoreExample.xml");

        public static Stream BookstoreSchema =>
            GetStream("BookstoreExample.xsd");

        public static Stream ReconstructedXml =>
            GetStream("Reconstructed.xml");

        public static Stream BrokenSchema =>
            GetStream("BrokenExample.xsd");

        public static Stream CimXmlGenericNotification =>
            GetStream("CimXmlGenericNotificationExample.xml");

        private static Stream GetStream(string resourceName)
        {
            var fullName = $"Energinet.DataHub.Core.SchemaValidation.Tests.Examples.{resourceName}";
            var stream = _currentAssembly.GetManifestResourceStream(fullName);
            if (stream != null)
            {
                return stream;
            }

            throw new InvalidOperationException($"{fullName} does not exist or was not embedded.");
        }
    }
}
