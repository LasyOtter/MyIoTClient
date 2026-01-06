using MyIoTClient.Core.Enums;
using MyIoTClient.Core.Interfaces;

namespace MyIoTClient.Core.Factory;

/// <summary>
/// 协议客户端工厂
/// </summary>
public interface IProtocolClientFactory
{
    /// <summary>
    /// 创建协议客户端
    /// </summary>
    IProtocolClient CreateClient(ProtocolType protocolType, object config);
}
