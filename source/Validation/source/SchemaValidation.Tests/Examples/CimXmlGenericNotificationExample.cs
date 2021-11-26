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

namespace SchemaValidation.Tests.Examples
{
    internal static class CimXmlGenericNotificationExample
    {
        public const string ExampleXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<cim:GenericNotification_MarketDocument xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:cim=""urn:ediel.org:structure:genericnotification:0:1"" xsi:schemaLocation=""urn:ediel.org:structure:genericnotification:0:1 urn-ediel-org-structure-genericnotification-0-1.xsd"">
	<cim:mRID>12898725</cim:mRID>
	<cim:type>E44</cim:type>
	<cim:process.processType>E01</cim:process.processType>
	<cim:businessSector.type>23</cim:businessSector.type>
	<cim:sender_MarketParticipant.mRID codingScheme=""A10"">5799999933318</cim:sender_MarketParticipant.mRID>
	<cim:sender_MarketParticipant.marketRole.type>DDZ</cim:sender_MarketParticipant.marketRole.type>
	<cim:receiver_MarketParticipant.mRID codingScheme=""A10"">5790001330552</cim:receiver_MarketParticipant.mRID>
	<cim:receiver_MarketParticipant.marketRole.type>DDQ</cim:receiver_MarketParticipant.marketRole.type>
	<cim:createdDateTime>2021-12-17T09:30:47Z</cim:createdDateTime>
	<cim:MktActivityRecord>
		<cim:mRID>78952366</cim:mRID>
		<cim:originalTransactionIDReference_MktActivityRecord.mRID>12587925</cim:originalTransactionIDReference_MktActivityRecord.mRID> <!-- Ny attribut -->
		<cim:validityStart_DateAndOrTime.dateTime>2021-12-17T23:00:00Z</cim:validityStart_DateAndOrTime.dateTime>
		<cim:marketEvaluationPoint.mRID codingScheme=""A10"">579999993331812345</cim:marketEvaluationPoint.mRID>
	</cim:MktActivityRecord>
</cim:GenericNotification_MarketDocument>";
    }
}
