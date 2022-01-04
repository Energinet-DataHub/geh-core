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
using System.Linq;
using System.Reflection;
using Energinet.DataHub.Core.XmlConversion.XmlConverter.Abstractions;

namespace Energinet.DataHub.Core.XmlConversion.XmlConverter.Configuration
{
    public static class ConverterMapperConfigurations
    {
        public static void AssertConfigurationValid(Type requestType, Assembly? configurationsAssembly = null)
        {
            var businessRequests = GetBusinessRequests(requestType);

            var configurations = GetAllConfigurations(configurationsAssembly ?? requestType.Assembly);

            foreach (var type in businessRequests)
            {
                AssertConfigurationForTypeValid(configurations, type);
            }
        }

        private static void AssertConfigurationForTypeValid(IEnumerable<XmlMappingConfigurationBase> configurations, Type type)
        {
            var configForType = configurations.SingleOrDefault(x => x.Configuration.Type == type);

            if (configForType == null) throw new InvalidOperationException($"Missing XmlMappingConfiguration for type: {type.Name}");

            var propertiesInConfig = configForType.Configuration.Properties;
            var propertiesInType = GetProperties(type);

            foreach (var propertyInfo in propertiesInType)
            {
                if (!propertiesInConfig.TryGetValue(propertyInfo.Name, out var propertyInConfig) || propertyInConfig is null)
                {
                    throw new InvalidOperationException($"Property {propertyInfo.Name} missing in XmlMappingConfiguration for type: {type.Name}");
                }
            }

            if (propertiesInType.Length != propertiesInConfig.Count) throw new InvalidOperationException($"Properties mismatch in XmlMappingConfiguration for type: {type.Name}");
        }

        private static PropertyInfo[] GetProperties(Type type)
        {
            var properties = ExcludePropertiesFromXmlHeader(type.GetProperties());
            return properties;
        }

        private static PropertyInfo[] ExcludePropertiesFromXmlHeader(IEnumerable<PropertyInfo> properties)
        {
            var excludedProperties = typeof(XmlHeaderData).GetProperties().Select(p => p.Name);
            return properties.Where(p => !excludedProperties.Contains(p.Name)).ToArray();
        }

        private static List<XmlMappingConfigurationBase> GetAllConfigurations(Assembly configurationsAssembly)
        {
            return configurationsAssembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(XmlMappingConfigurationBase)) && !t.IsAbstract)
                .Select(Activator.CreateInstance)
                .Cast<XmlMappingConfigurationBase>()
                .ToList();
        }

        private static IEnumerable<Type> GetBusinessRequests(Type requestType)
        {
            return requestType.Assembly.GetTypes()
                .Where(p => typeof(IInternalMarketDocument).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                .ToList();
        }
    }
}
