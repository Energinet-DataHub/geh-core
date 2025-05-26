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

using Azure.Core;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Energinet.DataHub.Core.Messaging.Communication.UnitTests.Extensions.DependencyInjection;

public class ServiceBusExtensionsTests
{
    private ServiceCollection Services { get; } = new();

    #region AddServiceBusClientForApplication

    [Fact]
    public void AddServiceBusClientForApplication_WhenCalledWithConfiguredSection_ServicesCanBeCreated()
    {
        // Arrange
        var fullyQualifiedNamespace = "namespace.servicebus.windows.net";
        var configuration = CreateInMemoryConfigurations(new Dictionary<string, string?>()
        {
            [$"{ServiceBusNamespaceOptions.SectionName}:{nameof(ServiceBusNamespaceOptions.FullyQualifiedNamespace)}"] = fullyQualifiedNamespace,
        });

        // Act
        Services.AddServiceBusClientForApplication(
            configuration,
            _ => Mock.Of<TokenCredential>());

        // Assert
        using var assertionScope = new AssertionScope();
        var serviceProvider = Services.BuildServiceProvider();

        var actualOptions = serviceProvider.GetRequiredService<IOptions<ServiceBusNamespaceOptions>>();
        actualOptions.Value.FullyQualifiedNamespace.Should().Be(fullyQualifiedNamespace);

        var actualClient = serviceProvider.GetRequiredService<ServiceBusClient>();
        actualClient.FullyQualifiedNamespace.Should().Be(fullyQualifiedNamespace);
    }

    [Fact]
    public void AddServiceBusClientForApplication_WhenCalledAndNoConfiguredSection_ExceptionIsThrown()
    {
        // Arrange
        var configuration = CreateInMemoryConfigurations([]);

        // Act
        var act = () => Services.AddServiceBusClientForApplication(
            configuration,
            _ => Mock.Of<TokenCredential>());

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Section 'ServiceBus' not found in configuration*");
    }

    /// <summary>
    /// This test documents what might be a surprise for some.
    /// During registration we will NOT get an exception if the required property
    /// <see cref="ServiceBusNamespaceOptions.FullyQualifiedNamespace"/> is empty.
    /// The reason for this is that validation of this value wont happen until runtime
    /// when the registered client is about to be created.
    /// </summary>
    [Fact]
    public void AddServiceBusClientForApplication_WhenCalledAndRequiredPropertyValueIsMissing_ShouldThrowExceptionsDuringValidationAndCreation()
    {
        // Arrange
        var configuration = CreateInMemoryConfigurations(new Dictionary<string, string?>()
        {
            [$"{ServiceBusNamespaceOptions.SectionName}:{nameof(ServiceBusNamespaceOptions.FullyQualifiedNamespace)}"] = string.Empty,
        });

        // Act
        Services.AddServiceBusClientForApplication(
            configuration,
            _ => Mock.Of<TokenCredential>());

        // Assert
        using var assertionScope = new AssertionScope();
        var serviceProvider = Services.BuildServiceProvider();

        var actualOptions = serviceProvider.GetRequiredService<IOptions<ServiceBusNamespaceOptions>>();
        var optionsValidationAct = () => actualOptions.Value;
        optionsValidationAct.Should()
            .Throw<OptionsValidationException>()
            .WithMessage("*The FullyQualifiedNamespace field is required*");

        var clientCreationAct = serviceProvider.GetRequiredService<ServiceBusClient>;
        clientCreationAct.Should()
            .Throw<ArgumentException>()
            .WithMessage("The value '' is not a well-formed Service Bus fully qualified namespace*");
    }

    #endregion

    #region AddIntegrationEventsPublisher

    [Fact]
    public void AddIntegrationEventsPublisher_WhenCalledWithConfiguredSections_ServicesCanBeCreated()
    {
        // Arrange
        var fullyQualifiedNamespace = "namespace.servicebus.windows.net";
        var topic = "topic";
        var configuration = CreateInMemoryConfigurations(new Dictionary<string, string?>()
        {
            [$"{ServiceBusNamespaceOptions.SectionName}:{nameof(ServiceBusNamespaceOptions.FullyQualifiedNamespace)}"] = fullyQualifiedNamespace,
            [$"{IntegrationEventsOptions.SectionName}:{nameof(IntegrationEventsOptions.TopicName)}"] = topic,
            [$"{IntegrationEventsOptions.SectionName}:{nameof(IntegrationEventsOptions.SubscriptionName)}"] = "subscription",
        });

        Services.AddServiceBusClientForApplication(
            configuration,
            _ => Mock.Of<TokenCredential>());

        // Act
        Services.AddIntegrationEventsPublisher<IntegrationEventProviderStub>(configuration);

        // Assert
        using var assertionScope = new AssertionScope();
        var serviceProvider = Services.BuildServiceProvider();

        var actualOptions = serviceProvider.GetRequiredService<IOptions<IntegrationEventsOptions>>();
        actualOptions.Value.TopicName.Should().Be(topic);

        var actualSenderFactory = serviceProvider.GetRequiredService<IAzureClientFactory<ServiceBusSender>>();
        var actualSender = actualSenderFactory.CreateClient(actualOptions.Value.TopicName);
        actualSender.FullyQualifiedNamespace.Should().Be(fullyQualifiedNamespace);
        actualSender.EntityPath.Should().Be(topic);
    }

    [Fact]
    public void AddIntegrationEventsPublisher_WhenCalledAndNoConfiguredSection_ExceptionIsThrown()
    {
        // Arrange
        var configuration = CreateInMemoryConfigurations([]);

        // Act
        var act = () => Services.AddIntegrationEventsPublisher<IntegrationEventProviderStub>(configuration);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Section 'IntegrationEvents' not found in configuration*");
    }

    [Fact]
    public void AddIntegrationEventsPublisher_WhenNoCallToAddServiceBusClientForApplication_ShouldThrowExceptionsDuringCreation()
    {
        // Arrange
        var topic = "topic";
        var configuration = CreateInMemoryConfigurations(new Dictionary<string, string?>()
        {
            [$"{IntegrationEventsOptions.SectionName}:{nameof(IntegrationEventsOptions.TopicName)}"] = topic,
            [$"{IntegrationEventsOptions.SectionName}:{nameof(IntegrationEventsOptions.SubscriptionName)}"] = "subscription",
        });

        // Act
        Services.AddIntegrationEventsPublisher<IntegrationEventProviderStub>(configuration);

        // Assert
        using var assertionScope = new AssertionScope();
        var serviceProvider = Services.BuildServiceProvider();

        var actualOptions = serviceProvider.GetRequiredService<IOptions<IntegrationEventsOptions>>();
        var actualSenderFactory = serviceProvider.GetRequiredService<IAzureClientFactory<ServiceBusSender>>();
        var senderCreationAct = () => actualSenderFactory.CreateClient(actualOptions.Value.TopicName);
        senderCreationAct.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("No service for type 'Azure.Messaging.ServiceBus.ServiceBusClient' has been registered*");
    }

    #endregion

    #region AddDeadLetterHandlerForIsolatedWorker

    [Fact]
    public void AddDeadLetterHandlerForIsolatedWorker_WhenCalledWithConfiguredSection_ServicesCanBeCreated()
    {
        // Arrange
        var storageAccountUrl = "https://storage-account.blob.core.windows.net";
        var containerName = "any-lowercase";
        var configuration = CreateInMemoryConfigurations(new Dictionary<string, string?>()
        {
            [$"{BlobDeadLetterLoggerOptions.SectionName}:{nameof(BlobDeadLetterLoggerOptions.StorageAccountUrl)}"] = storageAccountUrl,
            [$"{BlobDeadLetterLoggerOptions.SectionName}:{nameof(BlobDeadLetterLoggerOptions.ContainerName)}"] = containerName,
        });

        // Act
        Services.AddDeadLetterHandlerForIsolatedWorker(
            configuration,
            _ => Mock.Of<TokenCredential>());

        // Assert
        using var assertionScope = new AssertionScope();
        var serviceProvider = Services.BuildServiceProvider();

        var actualOptions = serviceProvider.GetRequiredService<IOptions<BlobDeadLetterLoggerOptions>>();
        actualOptions.Value.StorageAccountUrl.Should().Be(storageAccountUrl);
        actualOptions.Value.ContainerName.Should().Be(containerName);

        var actualClientFactory = serviceProvider.GetRequiredService<IAzureClientFactory<BlobServiceClient>>();
        var actualClient = actualClientFactory.CreateClient(containerName);
        actualClient.Uri.Should().Be(storageAccountUrl);
    }

    [Fact]
    public void AddDeadLetterHandlerForIsolatedWorker_WhenCalledAndNoConfiguredSection_ExceptionIsThrown()
    {
        // Arrange
        var configuration = CreateInMemoryConfigurations([]);

        // Act
        var act = () => Services.AddDeadLetterHandlerForIsolatedWorker(
            configuration,
            _ => Mock.Of<TokenCredential>());

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Section 'DeadLetterLogging' not found in configuration*");
    }

    #endregion

    protected IConfiguration CreateInMemoryConfigurations(Dictionary<string, string?> configurations)
    {
        var configurationRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(configurations)
            .Build();

        Services.AddScoped<IConfiguration>(_ => configurationRoot);

        return configurationRoot;
    }
}
