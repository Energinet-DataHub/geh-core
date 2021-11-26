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
        public IXmlSchema Codelists { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-codelists.xsd");

        public IXmlSchema GeneralAcknowledgement { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-general-acknowledgement-0-1.xsd");

        public IXmlSchema GeneralAcknowledgementdokument { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-general-acknowledgementdokument-0-1.xsd");

        public IXmlSchema MeasureNotifyaggregatedtimeseries { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-notifyaggregatedtimeseries-0-1.xsd");

        public IXmlSchema MeasureNotifyvalidatedmeasuredata { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-notifyvalidatedmeasuredata-0-1.xsd");

        public IXmlSchema MeasureNotifywholesaleservices { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-notifywholesaleservices-0-1.xsd");

        public IXmlSchema MeasureRejectrequestaggregatedmeasuredata { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-rejectrequestaggregatedmeasuredata-0-1.xsd");

        public IXmlSchema MeasureRejectrequestforreminders { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-rejectrequestforreminders-0-1.xsd");

        public IXmlSchema MeasureRejectrequestvalidatedmeasuredata { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-rejectrequestvalidatedmeasuredata-0-1.xsd");

        public IXmlSchema MeasureRejectrequestwholesalesettlement { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-rejectrequestwholesalesettlement-0-1.xsd");

        public IXmlSchema MeasureReminderofmissingmeasuredata { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-reminderofmissingmeasuredata-0-1.xsd");

        public IXmlSchema MeasureRequestaggregatedmeasuredata { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-requestaggregatedmeasuredata-0-1.xsd");

        public IXmlSchema MeasureRequestforreminders { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-requestforreminders-0-1.xsd");

        public IXmlSchema MeasureRequestvalidatedmeasuredata { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-requestvalidatedmeasuredata-0-1.xsd");

        public IXmlSchema MeasureRequestwholesalesettlement { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-measure-requestwholesalesettlement-0-1.xsd");

        public IXmlSchema StructureAccountingpointcharacteristics { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-accountingpointcharacteristics-0-1.xsd");

        public IXmlSchema StructureCharacteristicsofacustomeratanap { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-characteristicsofacustomeratanap-0-1.xsd");

        public IXmlSchema StructureConfirmrequestcancellation { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-confirmrequestcancellation-0-1.xsd");

        public IXmlSchema StructureConfirmrequestchangebillingmasterdata { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-confirmrequestchangebillingmasterdata-0-1.xsd");

        public IXmlSchema StructureConfirmrequestchangecustomercharacteristics { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-confirmrequestchangecustomercharacteristics-0-1.xsd");

        public IXmlSchema StructureConfirmrequestchangeofaccountingpointcharacteristics { get; }
            = new CimXmlSchemaContainer(
                "urn-ediel-org-structure-confirmrequestchangeofaccountingpointcharacteristics-0-1.xsd");

        public IXmlSchema StructureConfirmrequestchangeofpricelist { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-confirmrequestchangeofpricelist-0-1.xsd");

        public IXmlSchema StructureConfirmrequestchangeofsupplier { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-confirmrequestchangeofsupplier-0-1.xsd");

        public IXmlSchema StructureConfirmrequestendofsupply { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-confirmrequestendofsupply-0-1.xsd");

        public IXmlSchema StructureConfirmrequestreallocatechangeofsupplier { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-confirmrequestreallocatechangeofsupplier-0-1.xsd");

        public IXmlSchema StructureConfirmrequestservice { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-confirmrequestservice-0-1.xsd");

        public IXmlSchema StructureGenericnotification { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-genericnotification-0-1.xsd");

        public IXmlSchema StructureNotifybillingmasterdata { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-notifybillingmasterdata-0-1.xsd");

        public IXmlSchema StructureNotifycancellation { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-notifycancellation-0-1.xsd");

        public IXmlSchema StructureNotifypricelist { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-notifypricelist-0-1.xsd");

        public IXmlSchema StructureRejectrequestaccountingpointcharacteristics { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-rejectrequestaccountingpointcharacteristics-0-1.xsd");

        public IXmlSchema StructureRejectrequestbillingmasterdata { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-rejectrequestbillingmasterdata-0-1.xsd");

        public IXmlSchema StructureRejectrequestcancellation { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-rejectrequestcancellation-0-1.xsd");

        public IXmlSchema StructureRejectrequestchangebillingmasterdata { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-rejectrequestchangebillingmasterdata-0-1.xsd");

        public IXmlSchema StructureRejectrequestchangecustomercharacteristics { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-rejectrequestchangecustomercharacteristics-0-1.xsd");

        public IXmlSchema StructureRejectrequestchangeofaccountingpointcharacteristics { get; }
            = new CimXmlSchemaContainer(
                "urn-ediel-org-structure-rejectrequestchangeofaccountingpointcharacteristics-0-1.xsd");

        public IXmlSchema StructureRejectrequestchangeofpricelist { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-rejectrequestchangeofpricelist-0-1.xsd");

        public IXmlSchema StructureRejectrequestchangeofsupplier { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-rejectrequestchangeofsupplier-0-1.xsd");

        public IXmlSchema StructureRejectrequestendofsupply { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-rejectrequestendofsupply-0-1.xsd");

        public IXmlSchema StructureRejectrequestpricelist { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-rejectrequestpricelist-0-1.xsd");

        public IXmlSchema StructureRejectrequestreallocatechangeofsupplier { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-rejectrequestreallocatechangeofsupplier-0-1.xsd");

        public IXmlSchema StructureRejectrequestservice { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-rejectrequestservice-0-1.xsd");

        public IXmlSchema StructureRequestaccountingpointcharacteristics { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestaccountingpointcharacteristics-0-1.xsd");

        public IXmlSchema StructureRequestbillingmasterdata { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestbillingmasterdata-0-1.xsd");

        public IXmlSchema StructureRequestcancellation { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestcancellation-0-1.xsd");

        public IXmlSchema StructureRequestchangeaccountingpointcharacteristics { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestchangeaccountingpointcharacteristics-0-1.xsd");

        public IXmlSchema StructureRequestchangebillingmasterdata { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestchangebillingmasterdata-0-1.xsd");

        public IXmlSchema StructureRequestchangecustomercharacteristics { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestchangecustomercharacteristics-0-1.xsd");

        public IXmlSchema StructureRequestchangeofpricelist { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestchangeofpricelist-0-1.xsd");

        public IXmlSchema StructureRequestchangeofsupplier { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestchangeofsupplier-0-1.xsd");

        public IXmlSchema StructureRequestendofsupply { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestendofsupply-0-1.xsd");

        public IXmlSchema StructureRequestpricelist { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestpricelist-0-1.xsd");

        public IXmlSchema StructureRequestreallocatechangeofsupplier { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestreallocatechangeofsupplier-0-1.xsd");

        public IXmlSchema StructureRequestservice { get; }
            = new CimXmlSchemaContainer("urn-ediel-org-structure-requestservice-0-1.xsd");

        public IXmlSchema LocalExtensionTypes { get; }
            = new CimXmlSchemaContainer("urn-entsoe-eu-local-extension-types.xsd");

        public IXmlSchema WgediCodelists { get; }
            = new CimXmlSchemaContainer("urn-entsoe-eu-wgedi-codelists.xsd");
    }
}
