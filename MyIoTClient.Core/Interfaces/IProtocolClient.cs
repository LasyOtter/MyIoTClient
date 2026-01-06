using MyIoTClient.Core.Models;

namespace MyIoTClient.Core.Interfaces;

/// <summary>
/// 协议客户端基础接口
/// </summary>
public interface IProtocolClient : IDisposable
{
    /// <summary>
    /// 连接到设备
    /// </summary>
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 断开连接
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// 检查连接状态
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 读取数据
    /// </summary>
    Task<ReadResult> ReadAsync(string address, int length = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入数据
    /// </summary>
    Task<WriteResult> WriteAsync(string address, object value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量读取数据
    /// </summary>
    Task<BatchReadResult> BatchReadAsync(IEnumerable<string> addresses, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量写入数据
    /// </summary>
    Task<BatchWriteResult> BatchWriteAsync(IDictionary<string, object> values, CancellationToken cancellationToken = default);
}
