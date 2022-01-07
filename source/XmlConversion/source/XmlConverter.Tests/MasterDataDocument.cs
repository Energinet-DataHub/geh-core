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

using Energinet.DataHub.Core.XmlConversion.XmlConverter.Abstractions;

namespace Energinet.DataHub.Core.XmlConversion.XmlConverter.Tests
{
    public record MasterDataDocument(
            string ProcessType = "",
            string? StreetName = null,
            string? BuildingNumber = null,
            string? PostCode = null,
            string? CityName = null,
            string? CitySubDivisionName = null,
            string MunicipalityCode = "",
            string CountryCode = "",
            string StreetCode = "",
            string? FloorIdentification = null,
            string? RoomIdentification = null,
            bool? IsActualAddress = null,
            string GsrnNumber = "",
            string TypeOfMeteringPoint = "",
            string? MeteringMethod = null,
            string MeterReadingOccurrence = "",
            int? MaximumCurrent = null,
            int MaximumPower = 0,
            string MeteringGridArea = "",
            string? PowerPlant = null,
            string? LocationDescription = null,
            string? SettlementMethod = null,
            string DisconnectionType = "",
            string EffectiveDate = "",
            string? MeterNumber = "",
            string TransactionId = "",
            string PhysicalStatusOfMeteringPoint = "",
            string? NetSettlementGroup = null,
            string? ConnectionType = null,
            string? FromGrid = "",
            string? ToGrid = "",
            string? ParentRelatedMeteringPoint = null,
            string? PhysicalConnectionCapacity = null,
            string? GeoInfoReference = null,
            string MeasureUnitType = "",
            string? ScheduledMeterReadingDate = "")
        : IInternalMarketDocument;
}
