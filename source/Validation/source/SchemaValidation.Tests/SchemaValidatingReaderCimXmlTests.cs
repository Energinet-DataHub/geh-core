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
using Energinet.DataHub.Core.Schemas;
using Energinet.DataHub.Core.SchemaValidation;
using SchemaValidation.Tests.Examples;
using Xunit;
using Xunit.Categories;

namespace SchemaValidation.Tests
{
    [UnitTest]
    public sealed class SchemaValidatingReaderCimXmlTests
    {
        [Fact]
        public async Task AdvanceAsync_ValidXml_ValidatesToEnd()
        {
            // Arrange
            var xmlStream = LoadStringIntoStream(CimXmlGenericNotificationExample.ExampleXml);
            var target = new SchemaValidatingReader(xmlStream, Schemas.CimXml.StructureGenericnotification);

            // Act
            while (await target.AdvanceAsync())
            {
            }

            // Assert
            Assert.False(target.HasErrors);
        }

        private static Stream LoadStringIntoStream(string contents)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(contents));
        }
    }
}
