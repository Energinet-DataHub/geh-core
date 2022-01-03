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

using Energinet.DataHub.Core.XmlConversion.XmlConverter.Configuration;

namespace Energinet.DataHub.Core.XmlConversion.XmlConverter.Tests
{
    public class MasterDataDocumentXmlMappingConfiguration : XmlMappingConfigurationBase
    {
        public MasterDataDocumentXmlMappingConfiguration()
        {
            CreateMapping<MasterDataDocument>("MktActivityRecord", mapper => mapper
                .AddProperty(x => x.GsrnNumber, "MarketEvaluationPoint", "mRID")
                .AddProperty(x => x.MaximumPower, "MarketEvaluationPoint", "contractedConnectionCapacity")
                .AddProperty(x => x.MaximumCurrent, "MarketEvaluationPoint", "ratedCurrent")
                .AddProperty(x => x.TypeOfMeteringPoint, TranslateMeteringPointType, "MarketEvaluationPoint", "type")
                .AddProperty(x => x.MeteringMethod, TranslateMeteringPointSubType, "MarketEvaluationPoint", "meteringMethod")
                .AddProperty(x => x.MeterReadingOccurrence, TranslateMeterReadingOccurrence, "MarketEvaluationPoint", "readCycle")
                .AddProperty(x => x.MeteringGridArea, "MarketEvaluationPoint", "meteringGridArea_Domain.mRID")
                .AddProperty(x => x.PowerPlant, "MarketEvaluationPoint", "linked_MarketEvaluationPoint.mRID")
                .AddProperty(x => x.LocationDescription, "MarketEvaluationPoint", "description")
                .AddProperty(x => x.SettlementMethod, TranslateSettlementMethod, "MarketEvaluationPoint", "settlementMethod")
                .AddProperty(x => x.DisconnectionType, TranslateDisconnectionType, "MarketEvaluationPoint", "disconnectionMethod")
                .AddProperty(x => x.EffectiveDate, "validityStart_DateAndOrTime.dateTime")
                .AddProperty(x => x.MeterNumber, "MarketEvaluationPoint", "meter.mRID")
                .AddProperty(x => x.TransactionId, "mRID")
                .AddProperty(x => x.PhysicalStatusOfMeteringPoint, TranslatePhysicalState, "MarketEvaluationPoint", "connectionState")
                .AddProperty(x => x.NetSettlementGroup, TranslateNetSettlementGroup, "MarketEvaluationPoint", "netSettlementGroup")
                .AddProperty(x => x.ConnectionType, TranslateConnectionType, "MarketEvaluationPoint", "mPConnectionType")
                .AddProperty(x => x.StreetName, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "streetDetail", "name")
                .AddProperty(x => x.CityName, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "townDetail", "code")
                .AddProperty(x => x.CitySubDivisionName, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "townDetail", "name")
                .AddProperty(x => x.MunicipalityCode, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "townDetail", "code")
                .AddProperty(x => x.PostCode, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "postalCode")
                .AddProperty(x => x.StreetCode, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "streetDetail", "code")
                .AddProperty(x => x.CountryCode, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "townDetail", "country")
                .AddProperty(x => x.FloorIdentification, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "streetDetail", "floorIdentification")
                .AddProperty(x => x.RoomIdentification, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "streetDetail", "suiteNumber")
                .AddProperty(x => x.BuildingNumber, "MarketEvaluationPoint", "usagePointLocation.mainAddress", "streetDetail", "number")
                .AddProperty(x => x.FromGrid, "MarketEvaluationPoint", "inMeteringGridArea_Domain.mRID")
                .AddProperty(x => x.ToGrid, "MarketEvaluationPoint", "outMeteringGridArea_Domain.mRID")
                .AddProperty(x => x.ParentRelatedMeteringPoint, "MarketEvaluationPoint", "parent_MarketEvaluationPoint.mRID")
                .AddProperty(x => x.PhysicalConnectionCapacity, "MarketEvaluationPoint", "physicalConnectionCapacity")
                .AddProperty(x => x.IsActualAddress, ActualAddressIndicator, "MarketEvaluationPoint", "usagePointLocation.actualAddressIndicator")
                .AddProperty(x => x.GeoInfoReference, "MarketEvaluationPoint", "usagePointLocation.geoInfoReference")
                .AddProperty(x => x.MeasureUnitType, TranslateMeasureUnitType, "MarketEvaluationPoint", "Series", "quantity_Measure_Unit.name")
                .AddProperty(x => x.ScheduledMeterReadingDate, "MarketEvaluationPoint", "nextReadingDate"));
        }

        private static bool? ActualAddressIndicator(XmlElementInfo element)
        {
            if (bool.TryParse(element?.SourceValue, out var result))
            {
                return result;
            }

            return null;
        }

        private static string TranslateSettlementMethod(XmlElementInfo element)
        {
            return element.SourceValue.ToUpperInvariant() switch
            {
                "D01" => "Flex",
                "E02" => "NonProfiled",
                "E01" => "Profiled",
                _ => element.SourceValue,
            };
        }

        private static string TranslateNetSettlementGroup(XmlElementInfo element)
        {
            return element.SourceValue switch
            {
                "0" => "Zero",
                "1" => "One",
                "2" => "Two",
                "3" => "Three",
                "6" => "Six",
                "99" => "Ninetynine",
                _ => element.SourceValue,
            };
        }

        private static string TranslateMeteringPointType(XmlElementInfo element)
        {
            return element.SourceValue.ToUpperInvariant() switch
            {
                "E17" => "Consumption",
                _ => element.SourceValue,
            };
        }

        private static string TranslateMeteringPointSubType(XmlElementInfo element)
        {
            return element.SourceValue.ToUpperInvariant() switch
            {
                "D01" => "Physical",
                _ => element.SourceValue,
            };
        }

        private static string TranslatePhysicalState(XmlElementInfo element)
        {
            return element.SourceValue.ToUpperInvariant() switch
            {
                "D03" => "New",
                _ => element.SourceValue,
            };
        }

        private static string TranslateConnectionType(XmlElementInfo element)
        {
            return element.SourceValue.ToUpperInvariant() switch
            {
                "D01" => "Direct",
                "D02" => "Installation",
                _ => element.SourceValue,
            };
        }

        private static string TranslateDisconnectionType(XmlElementInfo element)
        {
            return element.SourceValue.ToUpperInvariant() switch
            {
                "D01" => "Remote",
                "D02" => "Manual",
                _ => element.SourceValue,
            };
        }

        private static string TranslateMeterReadingOccurrence(XmlElementInfo element)
        {
            return element.SourceValue.ToUpperInvariant() switch
            {
                "P1Y" => "Yearly",
                "P1M" => "Monthly",
                "PT1H" => "Hourly",
                "PT15M" => "Quarterly",
                _ => element.SourceValue,
            };
        }

        private static string TranslateMeasureUnitType(XmlElementInfo element)
        {
            return element.SourceValue.ToUpperInvariant() switch
            {
                "K3" => "KVArh",
                "KWH" => "KWh",
                "KWT" => "KW",
                "MAW" => "MW",
                "MWH" => "MWh",
                "TNE" => "Tonne",
                "Z03" => "MVAr",
                "AMP" => "Ampere",
                "H87" => "STK",
                "Z14" => "DanishTariffCode",
                _ => element.SourceValue,
            };
        }
    }
}
