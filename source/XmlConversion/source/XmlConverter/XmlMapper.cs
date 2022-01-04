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
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Energinet.DataHub.Core.XmlConversion.XmlConverter.Abstractions;
using Energinet.DataHub.Core.XmlConversion.XmlConverter.Configuration;

namespace Energinet.DataHub.Core.XmlConversion.XmlConverter
{
    public class XmlMapper
    {
        private readonly Func<string, XmlMappingConfigurationBase> _mappingConfigurationFactory;
        private readonly Func<string, string> _processTypeTranslator;

        public XmlMapper(
                Func<string, XmlMappingConfigurationBase> mappingConfigurationFactory,
                Func<string, string> processTypeTranslator)
        {
            _mappingConfigurationFactory = mappingConfigurationFactory;
            _processTypeTranslator = processTypeTranslator;
        }

        public XmlDeserializationResult Map(XElement rootElement)
        {
            if (rootElement == null) throw new ArgumentNullException(nameof(rootElement));

            XNamespace ns = rootElement.Attributes().FirstOrDefault(attr => attr.Name.LocalName == "cim")?.Value ?? throw new ArgumentException("Found no namespace for XML Document");

            var headerData = MapHeaderData(rootElement, ns);

            var currentMappingConfiguration = _mappingConfigurationFactory(headerData.Type);

            var elements = InternalMap(currentMappingConfiguration, rootElement, ns, headerData);

            return elements;
        }

        private static XmlDeserializationResult InternalMap(
            XmlMappingConfigurationBase xmlMappingConfigurationBase, XContainer rootElement, XNamespace ns, XmlHeaderData xmlHeaderData)
        {
            var configuration = xmlMappingConfigurationBase.Configuration;
            var properties = configuration.Properties;
            var messages = new List<IInternalMarketDocument>();
            var elements = rootElement.Elements(ns + configuration.XmlElementName);
            foreach (var element in elements)
            {
                var args = properties.Select(property =>
                {
                    if (property.Value is null)
                    {
                        throw new ArgumentNullException($"Missing map for property with name: {property.Key}");
                    }

                    var xmlHierarchyQueue = new Queue<string>(property.Value.XmlHierarchy);
                    var correspondingXmlElement = GetXmlElement(element, xmlHierarchyQueue, ns);

                    return Convert(correspondingXmlElement, property.Value.PropertyInfo.PropertyType, property.Value.TranslatorFunc);
                }).ToArray();

                var constructorArguments = CreateConstructorArguments(GetHeaderValuesToInclude(configuration, xmlHeaderData), args);
                if (configuration.CreateInstance(constructorArguments) is not IInternalMarketDocument instance)
                {
                    throw new InvalidOperationException("Could not create instance");
                }

                messages.Add(instance);
            }

            return new XmlDeserializationResult(messages, xmlHeaderData);
        }

        private static string ExtractElementValue(XContainer element, XName name)
        {
            return element.Element(name)?.Value ?? string.Empty;
        }

        private static string ExtractCodingScheme(XContainer element, XName name)
        {
            return element.Element(name)?.Attributes().SingleOrDefault(x => x.Name == "codingScheme")?.Value ?? string.Empty;
        }

        private static (string Value, string CodingScheme) ExtractElementValueAndCodingScheme(XContainer element, XName name)
        {
            var value = ExtractElementValue(element, name);
            var codingScheme = ExtractCodingScheme(element, name);

            return (value, codingScheme);
        }

        private static XElement? GetXmlElement(XContainer? container, Queue<string> hierarchyQueue, XNamespace ns)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            var elementName = hierarchyQueue.Dequeue();
            var element = container.Element(ns + elementName);

            if (element is null) return null;

            return hierarchyQueue.Any() ? GetXmlElement(element, hierarchyQueue, ns) : element;
        }

        private static IEnumerable<object?> GetHeaderValuesToInclude(ConverterMapperConfiguration configuration, XmlHeaderData xmlHeaderData)
        {
            var constructorParametersNames = configuration.Type.GetConstructors().SingleOrDefault()?.GetParameters().Select(p => p.Name).ToList();
            var includedHeaderValues = xmlHeaderData
                .GetType().GetProperties()
                .Where(p => constructorParametersNames!.Contains(p.Name))
                .Select(p => p.GetValue(xmlHeaderData)).ToList();

            return includedHeaderValues.ToArray();
        }

        private static object?[] CreateConstructorArguments(IEnumerable<object?> headerValues, IEnumerable<object?> mappedValues)
        {
            var constructorArguments = new List<object?>();
            constructorArguments.AddRange(headerValues);
            constructorArguments.AddRange(mappedValues);
            return constructorArguments.ToArray();
        }

        private static object? Convert(XElement? source, Type dest, Func<XmlElementInfo, object?>? valueTranslatorFunc)
        {
            if (source is null) return default;

            var xmlElementInfo = new XmlElementInfo(source.Value, source.Attributes());

            if (TryUseTranslatorFunc(dest))
            {
                return valueTranslatorFunc != null ? valueTranslatorFunc(xmlElementInfo) : source.Value;
            }

            return System.Convert.ChangeType(source.Value, dest, CultureInfo.InvariantCulture);
        }

        private static bool TryUseTranslatorFunc(Type dest)
        {
            var types = new HashSet<Type>
            {
                typeof(bool),
                typeof(bool?),
                typeof(string),
            };

            return types.Contains(dest);
        }

        private XmlHeaderData MapHeaderData(XContainer rootElement, XNamespace ns)
        {
            var mrid = ExtractElementValue(rootElement, ns + "mRID");
            var type = ExtractElementValue(rootElement, ns + "type");
            var processType = _processTypeTranslator(ExtractElementValue(rootElement, ns + "process.processType"));
            var (value, codingScheme) = ExtractElementValueAndCodingScheme(rootElement, ns + "sender_MarketParticipant.mRID");
            var senderRole = ExtractElementValue(rootElement, ns + "sender_MarketParticipant.marketRole.type");

            var headerData = new XmlHeaderData(mrid, type, processType, new XmlHeaderSender(value, codingScheme, senderRole));

            return headerData;
        }
    }
}
