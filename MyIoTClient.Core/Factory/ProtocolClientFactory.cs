using MyIoTClient.Core.Enums;
using MyIoTClient.Core.Interfaces;
using MyIoTClient.Core.Models;
using MyIoTClient.Protocols.BacNet;
using MyIoTClient.Protocols.Modbus;
using MyIoTClient.Protocols.MitsubishiMc;
using MyIoTClient.Protocols.OmronFins;
using MyIoTClient.Protocols.OpcUa;
using MyIoTClient.Protocols.Plc;

namespace MyIoTClient.Core.Factory;

/// <summary>
/// 协议客户端工厂实现
/// </summary>
public class ProtocolClientFactory : IProtocolClientFactory
{
    /// <summary>
    /// 创建协议客户端
    /// </summary>
    public IProtocolClient CreateClient(ProtocolType protocolType, object config)
    {
        return protocolType switch
        {
            ProtocolType.ModbusTcp => CreateModbusTcpClient(config),
            ProtocolType.ModbusRtu => CreateModbusRtuClient(config),
            ProtocolType.OpcUa => CreateOpcUaClient(config),
            ProtocolType.BacNet => CreateBacNetClient(config),
            ProtocolType.SiemensS7 => CreateSiemensS7Client(config),
            ProtocolType.MitsubishiMc => CreateMitsubishiMcClient(config),
            ProtocolType.OmronFins => CreateOmronFinsClient(config),
            _ => throw new NotSupportedException($"不支持的协议类型: {protocolType}")
        };
    }

    private IProtocolClient CreateModbusTcpClient(object config)
    {
        if (config is TcpConnectionConfig tcpConfig)
        {
            return new ModbusTcpClient(tcpConfig);
        }
        throw new ArgumentException("Modbus TCP需要TcpConnectionConfig配置");
    }

    private IProtocolClient CreateModbusRtuClient(object config)
    {
        if (config is SerialConnectionConfig serialConfig)
        {
            return new ModbusRtuClient(serialConfig);
        }
        throw new ArgumentException("Modbus RTU需要SerialConnectionConfig配置");
    }

    private IProtocolClient CreateOpcUaClient(object config)
    {
        if (config is OpcUaConnectionConfig opcConfig)
        {
            return new OpcUaClient(opcConfig);
        }
        throw new ArgumentException("OPC UA需要OpcUaConnectionConfig配置");
    }

    private IProtocolClient CreateBacNetClient(object config)
    {
        if (config is BacNetConnectionConfig bacConfig)
        {
            return new BacNetClient(bacConfig);
        }
        throw new ArgumentException("BACnet需要BacNetConnectionConfig配置");
    }

    private IProtocolClient CreateSiemensS7Client(object config)
    {
        if (config is TcpConnectionConfig tcpConfig)
        {
            return new SiemensS7Client(tcpConfig);
        }
        throw new ArgumentException("西门子S7需要TcpConnectionConfig配置");
    }

    private IProtocolClient CreateMitsubishiMcClient(object config)
    {
        if (config is MitsubishiMcConnectionConfig mcConfig)
        {
            return new MitsubishiMcClient(mcConfig);
        }
        throw new ArgumentException("三菱MC需要MitsubishiMcConnectionConfig配置");
    }

    private IProtocolClient CreateOmronFinsClient(object config)
    {
        if (config is OmronFinsConnectionConfig finsConfig)
        {
            return new OmronFinsClient(finsConfig);
        }
        throw new ArgumentException("欧姆龙FINS需要OmronFinsConnectionConfig配置");
    }
}
