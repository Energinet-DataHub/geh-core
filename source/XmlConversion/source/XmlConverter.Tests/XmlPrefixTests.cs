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
using System.Xml.Linq;
using Energinet.DataHub.Core.XmlConversion.XmlConverter.Configuration;
using Xunit;

namespace Energinet.DataHub.Core.XmlConversion.XmlConverter.Tests
{
    public class XmlPrefixTests
    {
        [Fact]
        public void Converting_xml_should_support_custom_xml_namespace_prefix()
        {
            var mapper = new XmlMapper(_ => new HeroMapper(), _ => "Superhero");
            var document = XmlContent.GetXDocumentWithRandomPrefix();

            Assert.NotNull(document.Root);
            Assert.NotNull(mapper.Map(document.Root!));
        }

        internal record Hero(string Name);

        internal class HeroMapper : XmlMappingConfigurationBase
        {
            public HeroMapper()
            {
                CreateMapping<Hero>("groot", map => map
                    .AddProperty(x => x.Name, "hero"));
            }
        }

        private static class XmlContent
        {
            private const string RandomPrefix = @"
<#REPLACE#:groot xmlns:#REPLACE#=""urn:ebix.org:structure:heros:0:1"">
    <#REPLACE#:hero>Starlord</#REPLACE#:hero>
</#REPLACE#:groot>";

            public static XDocument GetXDocumentWithRandomPrefix()
                => XDocument.Parse(GetXmlContentWithRandomPrefix());

            public static string GetXmlContentWithRandomPrefix()
                => CreateContent(Guid.NewGuid().ToString("N"));

            private static string CreateContent(string prefixValue)
            {
                return RandomPrefix.Replace("#REPLACE#", $"a{prefixValue}");
            }
        }
    }
}
