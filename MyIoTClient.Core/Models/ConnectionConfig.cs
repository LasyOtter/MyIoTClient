namespace MyIoTClient.Core.Models;

/// <summary>
/// 连接配置基类
/// </summary>
public abstract class ConnectionConfig
{
    /// <summary>
    /// 连接超时时间（毫秒）
    /// </summary>
    public int ConnectionTimeout { get; set; } = 5000;

    /// <summary>
    /// 读取超时时间（毫秒）
    /// </summary>
    public int ReadTimeout { get; set; } = 3000;

    /// <summary>
    /// 写入超时时间（毫秒）
    /// </summary>
    public int WriteTimeout { get; set; } = 3000;

    /// <summary>
    /// 重试次数
    /// </summary>
    public int RetryCount { get; set; } = 3;
}

/// <summary>
/// TCP连接配置
/// </summary>
public class TcpConnectionConfig : ConnectionConfig
{
    /// <summary>
    /// IP地址
    /// </summary>
    public required string IpAddress { get; set; }

    /// <summary>
    /// 端口
    /// </summary>
    public required int Port { get; set; }
}

/// <summary>
/// 串口连接配置
/// </summary>
public class SerialConnectionConfig : ConnectionConfig
{
    /// <summary>
    /// 串口名称
    /// </summary>
    public required string PortName { get; set; }

    /// <summary>
    /// 波特率
    /// </summary>
    public int BaudRate { get; set; } = 9600;

    /// <summary>
    /// 数据位
    /// </summary>
    public int DataBits { get; set; } = 8;

    /// <summary>
    /// 停止位
    /// </summary>
    public string StopBits { get; set; } = "One";

    /// <summary>
    /// 校验位
    /// </summary>
    public string Parity { get; set; } = "None";
}

/// <summary>
/// 三菱MC协议连接配置
/// </summary>
public class MitsubishiMcConnectionConfig : TcpConnectionConfig
{
    /// <summary>
    /// 网络号（默认0）
    /// </summary>
    public byte NetworkNumber { get; set; } = 0;

    /// <summary>
    /// PC号（默认255）
    /// </summary>
    public byte PcNumber { get; set; } = 0xFF;

    /// <summary>
    /// 请求目标模块I/O号（默认0）
    /// </summary>
    public ushort TargetModuleIoNumber { get; set; } = 0;

    /// <summary>
    /// 请求目标模块站号（默认0）
    /// </summary>
    public byte TargetModuleStationNumber { get; set; } = 0;

    /// <summary>
    /// CPU监视定时器（默认8000毫秒）
    /// </summary>
    public ushort CpuWatchTimer { get; set; } = 8000;

    /// <summary>
    /// 是否使用二进制格式（默认false，使用ASCII格式）
    /// </summary>
    public bool UseBinaryFormat { get; set; } = false;
}

/// <summary>
/// 欧姆龙FINS协议连接配置
/// </summary>
public class OmronFinsConnectionConfig : TcpConnectionConfig
{
    /// <summary>
    /// 本地网络号（默认0）
    /// </summary>
    public byte LocalNetworkNumber { get; set; } = 0;

    /// <summary>
    /// 本地节点号（默认0）
    /// </summary>
    public byte LocalNodeNumber { get; set; } = 0;

    /// <summary>
    /// 本地单元号（默认0）
    /// </summary>
    public byte LocalUnitNumber { get; set; } = 0;

    /// <summary>
    /// 远程网络号（默认0）
    /// </summary>
    public byte RemoteNetworkNumber { get; set; } = 0;

    /// <summary>
    /// 远程节点号（PLC节点号）
    /// </summary>
    public required byte RemoteNodeNumber { get; set; }

    /// <summary>
    /// 远程单元号（默认0）
    /// </summary>
    public byte RemoteUnitNumber { get; set; } = 0;

    /// <summary>
    /// FINS/UDP端口（默认9600）
    /// </summary>
    public int FinsUdpPort { get; set; } = 9600;

    /// <summary>
    /// 使用TCP还是UDP（默认TCP）
    /// </summary>
    public bool UseTcp { get; set; } = true;
}

/// <summary>
/// BACnet连接配置
/// </summary>
public class BacNetConnectionConfig : ConnectionConfig
{
    /// <summary>
    /// 本地IP地址
    /// </summary>
    public required string LocalIpAddress { get; set; }

    /// <summary>
    /// BACnet端口（默认47808）
    /// </summary>
    public int Port { get; set; } = 47808;

    /// <summary>
    /// 设备实例ID
    /// </summary>
    public uint DeviceId { get; set; } = 1234;
}

/// <summary>
/// OPC UA客户端配置
/// </summary>
public class OpcUaConnectionConfig : ConnectionConfig
{
    /// <summary>
    /// 服务器地址 (opc.tcp://localhost:4840)
    /// </summary>
    public required string EndpointUrl { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// 安全策略
    /// </summary>
    public string SecurityPolicy { get; set; } = "None";
}
