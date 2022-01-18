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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using NodaTime;

namespace Energinet.DataHub.Core.SchemaValidation.Xml
{
    internal sealed class XmlSourceValidatingReader : ISourceValidatingReader
    {
        private readonly List<SchemaValidationError> _errors = new();
        private readonly Queue<Attribute> _attributes = new();
        private readonly Stream _inputStream;
        private readonly IEnumerable<IXmlSchema> _inputSchemas;

        private XmlReader? _xmlReader;
        private object? _currentValue;
        private string? _emptyElementTag;

        public XmlSourceValidatingReader(Stream stream, IEnumerable<IXmlSchema> xmlSchemas)
        {
            CurrentNodeName = string.Empty;
            CurrentNodeType = NodeType.None;
            CanReadValue = false;
            Errors = _errors.AsReadOnly();

            _inputStream = stream;
            _inputSchemas = xmlSchemas;
        }

        public string CurrentNodeName { get; private set; }

        public NodeType CurrentNodeType { get; private set; }

        public bool CanReadValue { get; private set; }

        public bool HasErrors => _errors.Count > 0;

        public IReadOnlyList<SchemaValidationError> Errors { get; }

        public async Task<bool> AdvanceAsync()
        {
            if (HasErrors)
            {
                return true;
            }

            await EnsureReaderAsync().ConfigureAwait(false);

            if (_attributes.TryDequeue(out var attribute))
            {
                _currentValue = attribute.Value;
                CurrentNodeName = attribute.Name;
                CurrentNodeType = NodeType.Attribute;
                CanReadValue = true;
                return true;
            }

            try
            {
                if (_emptyElementTag != null)
                {
                    ProcessEmptyElement(_emptyElementTag);
                    _emptyElementTag = null;
                    return true;
                }

                do
                {
                    _currentValue = null;

                    var finishedProcessingKnownElement = false;

                    switch (_xmlReader!.NodeType)
                    {
                        case XmlNodeType.Element:
                            await ProcessElementAsync().ConfigureAwait(false);
                            finishedProcessingKnownElement = true;
                            break;
                        case XmlNodeType.EndElement:
                            ProcessEndElement();
                            finishedProcessingKnownElement = true;
                            break;
                    }

                    if (finishedProcessingKnownElement && !HasErrors)
                    {
                        return true;
                    }
                }
                while (await ValidatingReadAsync().ConfigureAwait(false));
            }
            catch (XmlException ex)
            {
                _errors.Add(new SchemaValidationError(ex.LineNumber, ex.LinePosition, ex.Message));
            }

            CurrentNodeName = string.Empty;
            CurrentNodeType = NodeType.None;
            CanReadValue = false;

            return false;
        }

        public Task<string> ReadValueAsStringAsync()
        {
            return ReadValueAsAsync<string>();
        }

        public Task<string> ReadValueAsDurationAsync()
        {
            return ReadValueAsAsync<string>();
        }

        public Task<int> ReadValueAsIntAsync()
        {
            return ReadValueAsAsync<int>();
        }

        public Task<long> ReadValueAsLongAsync()
        {
            return ReadValueAsAsync<long>();
        }

        public Task<bool> ReadValueAsBoolAsync()
        {
            return ReadValueAsAsync<bool>();
        }

        public Task<decimal> ReadValueAsDecimalAsync()
        {
            return ReadValueAsAsync<decimal>();
        }

        public async Task<Instant> ReadValueAsNodaTimeAsync()
        {
            var dt = await ReadValueAsAsync<DateTimeOffset>().ConfigureAwait(false);
            return Instant.FromDateTimeOffset(dt);
        }

        internal async Task<XElement?> ReadIntoXElementAsync()
        {
            await EnsureReaderAsync().ConfigureAwait(false);

            try
            {
                var element = await XElement
                    .LoadAsync(_xmlReader!, LoadOptions.None, CancellationToken.None)
                    .ConfigureAwait(false);

                return HasErrors ? null : element;
            }
            catch (XmlException ex)
            {
                _errors.Add(new SchemaValidationError(ex.LineNumber, ex.LinePosition, ex.Message));
            }

            return null;
        }

        internal async Task<XmlReader> GetInternalReaderAsync()
        {
            await EnsureReaderAsync().ConfigureAwait(false);
            return _xmlReader!;
        }

        private async Task<T> ReadValueAsAsync<T>()
        {
            await EnsureReaderAsync().ConfigureAwait(false);

            if (_currentValue == null)
            {
                if (CanReadValue)
                {
                    throw new InvalidOperationException("Internal Error: CanReadValue is true, but there is no value.");
                }

                throw new InvalidOperationException("Cannot ReadValueAs when there is no value (CanReadValue is false).");
            }

            if (_currentValue is T typed)
            {
                return typed;
            }

            var targetType = typeof(T);

            // xs:integer is returned as decimal from ReadContentAsObjectAsync.
            if (targetType == typeof(int) && _currentValue is decimal d)
            {
                return (dynamic)(int)d;
            }

            // Parse xs:dateTime manually to preserve time zone information.
            if (targetType == typeof(DateTimeOffset))
            {
                return (dynamic)XmlConvert.ToDateTimeOffset((string)_currentValue);
            }

            return (dynamic)Convert.ChangeType(_currentValue, targetType, CultureInfo.InvariantCulture);
        }

        private async Task<bool> ValidatingReadAsync()
        {
            await EnsureReaderAsync().ConfigureAwait(false);

            bool couldRead;

            do
            {
                couldRead = await _xmlReader!.ReadAsync().ConfigureAwait(false);

                // If could read without errors, return true.
                // Otherwise, read to end to get all the errors.
                if (couldRead && !HasErrors)
                {
                    return true;
                }
            }
            while (couldRead);

            return false;
        }

        private async Task ProcessElementAsync()
        {
            CurrentNodeName = _xmlReader!.LocalName;
            CurrentNodeType = NodeType.StartElement;
            CanReadValue = false;

            if (_xmlReader.HasAttributes)
            {
                while (_xmlReader.MoveToNextAttribute())
                {
                    var attrValue = await ReadTypedContentAsync().ConfigureAwait(false);
                    _attributes.Enqueue(new Attribute { Name = _xmlReader.LocalName, Value = attrValue });
                }

                // Once we are out of attributes, read to next element.
                _xmlReader.MoveToElement();
            }

            if (HandleEmptyElement())
            {
                return;
            }

            if (_xmlReader.NodeType == XmlNodeType.Text)
            {
                CanReadValue = true;
                _currentValue = await ReadTypedContentAsync().ConfigureAwait(false);
            }
        }

        private async Task<object> ReadTypedContentAsync()
        {
            var rawString = _xmlReader!.Value;
            var typedValue = await _xmlReader.ReadContentAsObjectAsync().ConfigureAwait(false);

            // Internal XmlReader implementation does not handle xs:duration and xs:dateTime correctly.
            // xs:duration converts P1M to P30D.
            // xs:dateTime does not preserve time zone information.
            // Therefore, the raw value is kept instead for conversion later.
            if (typedValue is DateTime or TimeSpan)
            {
                return rawString.Trim();
            }

            return typedValue;
        }

        private void ProcessEmptyElement(string closedTag)
        {
            CurrentNodeName = closedTag;
            CurrentNodeType = NodeType.EndElement;
            CanReadValue = false;
            _xmlReader!.ReadStartElement();
        }

        private void ProcessEndElement()
        {
            CurrentNodeName = _xmlReader!.LocalName;
            CurrentNodeType = NodeType.EndElement;
            CanReadValue = false;
            _xmlReader.ReadEndElement();
        }

        private bool HandleEmptyElement()
        {
            if (_xmlReader!.IsEmptyElement)
            {
                _emptyElementTag = _xmlReader.LocalName;
                return true;
            }

            _xmlReader.ReadStartElement();
            return false;
        }

        private void OnXmlReaderValidationEventHandler(object? sender, ValidationEventArgs e)
        {
            var xmlSchemaException = e.Exception;
            var schemaValidationError = new SchemaValidationError(
                xmlSchemaException.LineNumber,
                xmlSchemaException.LinePosition,
                xmlSchemaException.Message);

            _errors.Add(schemaValidationError);
        }

        private async Task EnsureReaderAsync()
        {
            if (_xmlReader == null)
            {
                var xmlReaderSettings = new XmlReaderSettings
                {
                    Async = true,
                    ValidationType = ValidationType.Schema,
                    ValidationFlags = XmlSchemaValidationFlags.ProcessInlineSchema | XmlSchemaValidationFlags.ReportValidationWarnings,
                };

                xmlReaderSettings.ValidationEventHandler += OnXmlReaderValidationEventHandler;

                foreach (var schema in _inputSchemas)
                {
                    var xmlSchema = await schema.GetXmlSchemaSetAsync().ConfigureAwait(false);
                    xmlReaderSettings.Schemas.Add(xmlSchema);
                }

                _xmlReader = XmlReader.Create(_inputStream, xmlReaderSettings);
            }
        }

        private readonly struct Attribute
        {
            public string Name { get; init; }

            public object Value { get; init; }
        }
    }
}
