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

using System.Net;
using System.Security.Claims;
using System.Text;
using AutoFixture;
using ExampleHost.FunctionApp.Tests.Fixtures;
using ExampleHost.FunctionApp01.FeatureManagement;
using ExampleHost.FunctionApp01.Functions;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.FeatureManagement;
using Moq;
using Xunit;

namespace ExampleHost.FunctionApp.Tests.Unit;

/// <summary>
/// Tests demonstrating how to unit test a class depending on <see cref="IFeatureManager"/>.
/// </summary>
public class FeatureManagementTests
{
    private readonly FeatureManagerStub _featureManageStub;
    private readonly FeatureManagementFunction _sut;

    public FeatureManagementTests()
    {
        _featureManageStub = new();
        _sut = new FeatureManagementFunction(_featureManageStub);
    }

    [Theory]
    [InlineData(false, "Disabled")]
    [InlineData(true, "Enabled")]
    public async Task Given_FeatureFlagValueIs_When_Requested_Then_ExpectedContentIsReturned(
        bool featureFlagValue,
        string expectedContent)
    {
        // Arrange
        // Configure the feature flag through the stub
        _featureManageStub.SetFeatureFlag(FeatureFlagNames.UseGetMessage, featureFlagValue);

        var contextMock = new Mock<FunctionContext>();
        var httpRequestStub = new HttpRequestDataStub(
            contextMock.Object,
            new Uri("https://notused.com"));

        // Act
        var actualResponse = await _sut.GetMessage(httpRequestStub);

        // Assert
        actualResponse.Body.Position = 0;
        using var reader = new StreamReader(actualResponse.Body);
        var content = await reader.ReadToEndAsync();
        content.Should().Be(expectedContent);
    }

    public class HttpRequestDataStub(
        FunctionContext functionContext,
        Uri url,
        string? method = null,
        Stream? body = null) : HttpRequestData(functionContext)
    {
        public override Stream Body { get; } = body ?? new MemoryStream();

        public override HttpHeadersCollection Headers { get; } = new HttpHeadersCollection();

        public override IReadOnlyCollection<IHttpCookie> Cookies { get; } = null!;

        public override Uri Url { get; } = url;

        public override IEnumerable<ClaimsIdentity> Identities { get; } = null!;

        public override string Method { get; } = method ?? "get";

        public override HttpResponseData CreateResponse()
        {
            return new HttpResponseDataStub(FunctionContext);
        }
    }

    public class HttpResponseDataStub(FunctionContext functionContext)
        : HttpResponseData(functionContext)
    {
        public override HttpStatusCode StatusCode { get; set; }

        public override HttpHeadersCollection Headers { get; set; } = new HttpHeadersCollection();

        public override Stream Body { get; set; } = new MemoryStream();

        public override HttpCookies Cookies { get; } = null!;
    }
}
