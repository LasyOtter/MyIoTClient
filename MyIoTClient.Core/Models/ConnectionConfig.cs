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
