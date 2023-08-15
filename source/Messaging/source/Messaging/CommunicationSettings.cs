namespace Energinet.DataHub.Core.Messaging.Communication;

/// <summary>
/// 
/// </summary>
public class CommunicationSettings
{
    /// <summary>
    /// 
    /// </summary>
    public string ServiceBusIntegrationEventWriteConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string IntegrationEventTopicName { get; set; } = string.Empty;
}
