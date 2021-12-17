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

using System.Threading.Tasks;
using Energinet.DataHub.Core.SchemaValidation.Tests.Examples;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Core.SchemaValidation.Tests
{
    [UnitTest]
    public sealed class SchemaValidatingReaderCimXmlTests
    {
        [Fact]
        public async Task AdvanceAsync_ValidXml_ValidatesToEnd()
        {
            // Arrange
            var xmlStream = ExampleResources.CimXmlGenericNotification;

            var target = new SchemaValidatingReader(xmlStream, Schemas.Schemas.CimXml.StructureGenericNotification);

            // Act
            while (await target.AdvanceAsync())
            {
            }

            // Assert
            Assert.False(target.HasErrors);
        }
    }
}
