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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Energinet.DataHub.Core.XmlConversion.XmlConverter.Configuration;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Core.XmlConversion.XmlConverter.Tests
{
    [UnitTest]
    public class XmlConverterTests
    {
        private Stream _xmlStream;

        public XmlConverterTests()
        {
            _xmlStream = GetResourceStream("CreateMeteringPointCimXml.xml");
        }

        [Fact]
        public void AssertConfigurationsValid()
        {
            Action assertConfigurationValid = () => ConverterMapperConfigurations.AssertConfigurationValid(typeof(MasterDataDocument), typeof(MasterDataDocumentXmlMappingConfiguration).Assembly);
            assertConfigurationValid.Should().NotThrow();
        }

        [Fact]
        public async Task ValidateValuesFromEachElementTest()
        {
            var xmlMapper = new XmlMapper((type) => new MasterDataDocumentXmlMappingConfiguration(), (processType) => "CreateMeteringPoint");

            var xmlConverter = new XmlDeserializer(xmlMapper);

            var deserializationResult = await xmlConverter.DeserializeAsync(_xmlStream).ConfigureAwait(false);
            var headerData = deserializationResult.HeaderData;
            var commands = deserializationResult.Documents.Cast<MasterDataDocument>();
            var command = commands.First();

            headerData.Sender.Id.Should().Be("123456789");
            headerData.Sender.Role.Should().Be("DDM");
            headerData.Sender.CodingScheme.Should().Be("A10");

            command.TypeOfMeteringPoint.Should().Be("Consumption");
            command.GsrnNumber.Should().Be("571234567891234605");
            command.MaximumPower.Should().Be(0);
            command.MeasureUnitType.Should().Be("KWh");
            command.PowerPlant.Should().Be("571234567891234636");
            command.SettlementMethod.Should().Be("Flex");
            command.TypeOfMeteringPoint.Should().Be("Consumption");
            command.MeteringMethod.Should().Be("Physical");
            command.PhysicalStatusOfMeteringPoint.Should().Be("New");
            command.ConnectionType.Should().Be("Direct");
            command.DisconnectionType.Should().Be("Remote");
            command.MeterReadingOccurrence.Should().Be("Hourly");

            command.LocationDescription.Should().Be("String");
            command.MeterNumber.Should().Be("123456789");
            command.EffectiveDate.Should().Be("2021-07-13T22:00:00Z");
            command.MeteringGridArea.Should().Be("870");
            command.NetSettlementGroup.Should().Be("Zero");
            command.MaximumCurrent.Should().BeNull();
            command.TransactionId.Should().Be("1234");
            command.PostCode.Should().Be("8000");
            command.StreetName.Should().Be("Test street name");
            command.CityName.Should().Be("Test city");
            command.CountryCode.Should().Be("DK");
            command.CitySubDivisionName.Should().BeEmpty();
            command.MunicipalityCode.Should().Be("12");

            command.FromGrid.Should().Be("869");
            command.ToGrid.Should().Be("871");
            command.IsActualAddress.Should().BeNull();
            Assert.Null(command.ParentRelatedMeteringPoint);
        }

        [Fact]
        public async Task ValidateTranslationOfCimXmlValuesToDomainSpecificValuesTest()
        {
            var xmlMapper = new XmlMapper((type) => new MasterDataDocumentXmlMappingConfiguration(), (processType) => "CreateMeteringPoint");

            var xmlConverter = new XmlDeserializer(xmlMapper);
            var deserializationResult = await xmlConverter.DeserializeAsync(_xmlStream).ConfigureAwait(false);
            var commands = deserializationResult.Documents.Cast<MasterDataDocument>();

            var command = commands.First();

            command.SettlementMethod.Should().Be("Flex");
            command.TypeOfMeteringPoint.Should().Be("Consumption");
            command.MeteringMethod.Should().Be("Physical");
            command.PhysicalStatusOfMeteringPoint.Should().Be("New");
            command.ConnectionType.Should().Be("Direct");
            command.DisconnectionType.Should().Be("Remote");
            command.MeterReadingOccurrence.Should().Be("Hourly");
        }

        private static Stream GetResourceStream(string resourcePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = new List<string>(assembly.GetManifestResourceNames());

            var resourceName = resourcePath.Replace(@"/", ".", StringComparison.Ordinal);
            var resource = resourceNames.FirstOrDefault(r => r.Contains(resourceName, StringComparison.Ordinal))
                ?? throw new FileNotFoundException("Resource not found");

            return assembly.GetManifestResourceStream(resource)
                   ?? throw new InvalidOperationException($"Couldn't get requested resource: {resourcePath}");
        }
    }
}
