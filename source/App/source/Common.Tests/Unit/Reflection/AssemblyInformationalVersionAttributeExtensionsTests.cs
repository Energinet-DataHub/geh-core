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

using System.Reflection;
using Energinet.DataHub.Core.App.Common.Reflection;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Energinet.DataHub.Core.App.Common.Tests.Unit.Reflection;

public class AssemblyInformationalVersionAttributeExtensionsTests
{
    public AssemblyInformationalVersionAttributeExtensionsTests()
    {
    }

    [Theory]
    [InlineData("1.0.0+PR_4+SHA_1234", "1.0.0", "4", "1234")] // Exact DH3 source version information
    [InlineData("canBeAnything+PR_dontCare+SHA_notVerified", "canBeAnything", "dontCare", "notVerified")] // Exact DH3 source version information, we do not verify if each group contains certain characters
    [InlineData("1.0.0+pr_4+SHA_1234", "1.0.0+pr_4+SHA_1234", "", "")] // Incorrect casing, so not exact DH3 source version information
    [InlineData("1.0.0", "1.0.0", "", "")] // Not DH3 source version information, its a standard product version
    [InlineData("1.0.0+1234", "1.0.0+1234", "", "")] // Not DH3 source version information, instead its similar to a NuGet package source information
    public void ExactDH3SourceVersionInformation_Should_ParseExpectedValues(string informationalVersion, string productVersion, string pullRequestNumber, string sha)
    {
        // Arrange
        var attribute = new AssemblyInformationalVersionAttribute(informationalVersion);

        // Act
        var actualSourceVersionInformation = attribute.GetSourceVersionInformation();

        // Assert
        using var assertionScope = new AssertionScope();
        actualSourceVersionInformation.ProductVersion.Should().Be(productVersion);
        actualSourceVersionInformation.PullRequestNumber.Should().Be(pullRequestNumber);
        actualSourceVersionInformation.LastMergeCommitSha.Should().Be(sha);
    }
}
