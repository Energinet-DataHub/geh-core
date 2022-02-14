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

using Energinet.DataHub.Core.SchemaValidation;

namespace Energinet.DataHub.Core.Schemas.CimXml
{
    public sealed class CimXmlSchemaCollection
    {
        internal CimXmlSchemaCollection()
        {
        }

        public IXmlSchema GeneralAcknowledgement { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-general-acknowledgement-0-1.xsd");

        public IXmlSchema MeasureNotifyValidatedMeasureData { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-notifyvalidatedmeasuredata-0-1.xsd");

        public IXmlSchema MeasureNotifyAggregatedMeasureData { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-notifyaggregatedmeasuredata-0-1.xsd");

        public IXmlSchema MeasureNotifyWholesaleServices { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-notifywholesaleservices-0-1.xsd");

        public IXmlSchema MeasureRejectRequestAggregatedMeasureData { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-rejectrequestaggregatedmeasuredata-0-1.xsd");

        public IXmlSchema MeasureRejectRequestForReminders { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-rejectrequestforreminders-0-1.xsd");

        public IXmlSchema MeasureRejectRequestValidatedMeasureData { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-rejectrequestvalidatedmeasuredata-0-1.xsd");

        public IXmlSchema MeasureRejectRequestWholesaleSettlement { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-rejectrequestwholesalesettlement-0-1.xsd");

        public IXmlSchema MeasureReminderOfMissingMeasureData { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-reminderofmissingmeasuredata-0-1.xsd");

        public IXmlSchema MeasureRequestAggregatedMeasureData { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-requestaggregatedmeasuredata-0-1.xsd");

        public IXmlSchema MeasureRequestForReminders { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-requestforreminders-0-1.xsd");

        public IXmlSchema MeasureRequestValidatedMeasureData { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-requestvalidatedmeasuredata-0-1.xsd");

        public IXmlSchema MeasureRequestWholesaleSettlement { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-requestwholesalesettlement-0-1.xsd");

        public IXmlSchema StructureAccountingPointCharacteristics { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-accountingpointcharacteristics-0-1.xsd");

        public IXmlSchema StructureCharacteristicsOfACustomerAtanap { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-characteristicsofacustomeratanap-0-1.xsd");

        public IXmlSchema StructureConfirmRequestCancellation { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-confirmrequestcancellation-0-1.xsd");

        public IXmlSchema StructureConfirmRequestChangeBillingMasterData { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-confirmrequestchangebillingmasterdata-0-1.xsd");

        public IXmlSchema StructureConfirmRequestChangeCustomerCharacteristics { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-confirmrequestchangecustomercharacteristics-0-1.xsd");

        public IXmlSchema StructureConfirmRequestChangeAccountingPointCharacteristics { get; }
            = new CimXmlSchemaContainer(
                "urn-ediel-org-structure-confirmrequestchangeaccountingpointcharacteristics-0-1.xsd");

        public IXmlSchema StructureConfirmRequestChangeOfPriceList { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-confirmrequestchangeofpricelist-0-1.xsd");

        public IXmlSchema StructureConfirmRequestChangeOfSupplier { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-confirmrequestchangeofsupplier-0-1.xsd");

        public IXmlSchema StructureConfirmRequestEndOfSupply { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-confirmrequestendofsupply-0-1.xsd");

        public IXmlSchema StructureConfirmRequestReallocateChangeOfSupplier { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-confirmrequestreallocatechangeofsupplier-0-1.xsd");

        public IXmlSchema StructureConfirmRequestService { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-confirmrequestservice-0-1.xsd");

        public IXmlSchema StructureGenericNotification { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-genericnotification-0-1.xsd");

        public IXmlSchema StructureNotifyBillingMasterData { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-notifybillingmasterdata-0-1.xsd");

        public IXmlSchema StructureNotifyCancellation { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-notifycancellation-0-1.xsd");

        public IXmlSchema StructureNotifyPriceList { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-notifypricelist-0-1.xsd");

        public IXmlSchema StructureRejectRequestAccountingPointCharacteristics { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-rejectrequestaccountingpointcharacteristics-0-1.xsd");

        public IXmlSchema StructureRejectRequestBillingMasterData { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-rejectrequestbillingmasterdata-0-1.xsd");

        public IXmlSchema StructureRejectRequestCancellation { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-rejectrequestcancellation-0-1.xsd");

        public IXmlSchema StructureRejectRequestChangeBillingMasterData { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-rejectrequestchangebillingmasterdata-0-1.xsd");

        public IXmlSchema StructureRejectRequestChangeCustomerCharacteristics { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-rejectrequestchangecustomercharacteristics-0-1.xsd");

        public IXmlSchema StructureRejectRequestChangeAccountingPointCharacteristics { get; }
            = new CimXmlSchemaContainer(
                "urn-ediel-org-structure-rejectrequestchangeaccountingpointcharacteristics-0-1.xsd");

        public IXmlSchema StructureRejectRequestChangeOfPriceList { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-rejectrequestchangeofpricelist-0-1.xsd");

        public IXmlSchema StructureRejectRequestChangeOfSupplier { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-rejectrequestchangeofsupplier-0-1.xsd");

        public IXmlSchema StructureRejectRequestEndOfSupply { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-rejectrequestendofsupply-0-1.xsd");

        public IXmlSchema StructureRejectRequestPriceList { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-rejectrequestpricelist-0-1.xsd");

        public IXmlSchema StructureRejectRequestRealLocateChangeOfSupplier { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-rejectrequestreallocatechangeofsupplier-0-1.xsd");

        public IXmlSchema StructureRejectRequestService { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-rejectrequestservice-0-1.xsd");

        public IXmlSchema StructureRequestAccountingPointCharacteristics { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestaccountingpointcharacteristics-0-1.xsd");

        public IXmlSchema StructureRequestBillingMasterData { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestbillingmasterdata-0-1.xsd");

        public IXmlSchema StructureRequestCancellation { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestcancellation-0-1.xsd");

        public IXmlSchema StructureRequestChangeAccountingPointCharacteristics { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestchangeaccountingpointcharacteristics-0-1.xsd");

        public IXmlSchema StructureRequestChangeBillingMasterData { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestchangebillingmasterdata-0-1.xsd");

        public IXmlSchema StructureRequestChangeCustomerCharacteristics { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestchangecustomercharacteristics-0-1.xsd");

        public IXmlSchema StructureRequestChangeOfPriceList { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestchangeofpricelist-0-1.xsd");

        public IXmlSchema StructureRequestChangeOfSupplier { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestchangeofsupplier-0-1.xsd");

        public IXmlSchema StructureRequestEndOfSupply { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestendofsupply-0-1.xsd");

        public IXmlSchema StructureRequestPriceList { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestpricelist-0-1.xsd");

        public IXmlSchema StructureRequestRealLocateChangeOfSupplier { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestreallocatechangeofsupplier-0-1.xsd");

        public IXmlSchema StructureRequestService { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestservice-0-1.xsd");

        public IXmlSchema LocalExtensionTypes { get; }
            = new CimXmlSchemaContainer("urn-entsoe-eu-local-extension-types.xsd");

        public IXmlSchema WgediCodelists { get; }
            = new CimXmlSchemaContainer("urn-entsoe-eu-wgedi-codelists.xsd");
    }
}
