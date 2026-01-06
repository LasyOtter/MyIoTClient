using MyIoTClient.Core.Interfaces;
using MyIoTClient.Core.Models;

namespace MyIoTClient.Protocols.Base;

/// <summary>
/// 协议客户端基类
/// </summary>
public abstract class ProtocolClientBase : IProtocolClient
{
    protected bool _disposed;
    protected bool _isConnected;

    /// <summary>
    /// 是否已连接
    /// </summary>
    public bool IsConnected => _isConnected;

    /// <summary>
    /// 连接配置
    /// </summary>
    protected ConnectionConfig Config { get; }

    protected ProtocolClientBase(ConnectionConfig config)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// 连接到设备
    /// </summary>
    public abstract Task<bool> ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 断开连接
    /// </summary>
    public abstract Task DisconnectAsync();

    /// <summary>
    /// 读取数据
    /// </summary>
    public abstract Task<ReadResult> ReadAsync(string address, int length = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入数据
    /// </summary>
    public abstract Task<WriteResult> WriteAsync(string address, object value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量读取数据
    /// </summary>
    public virtual async Task<BatchReadResult> BatchReadAsync(IEnumerable<string> addresses, CancellationToken cancellationToken = default)
    {
        var result = new BatchReadResult { IsSuccess = true };

        foreach (var address in addresses)
        {
            var readResult = await ReadAsync(address, 1, cancellationToken);
            result.Results.Add(readResult);

            if (!readResult.IsSuccess)
            {
                result.IsSuccess = false;
            }
        }

        return result;
    }

    /// <summary>
    /// 批量写入数据
    /// </summary>
    public virtual async Task<BatchWriteResult> BatchWriteAsync(IDictionary<string, object> values, CancellationToken cancellationToken = default)
    {
        var result = new BatchWriteResult { IsSuccess = true };

        foreach (var kvp in values)
        {
            var writeResult = await WriteAsync(kvp.Key, kvp.Value, cancellationToken);
            result.Results.Add(writeResult);

            if (!writeResult.IsSuccess)
            {
                result.IsSuccess = false;
            }
        }

        return result;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                DisconnectAsync().GetAwaiter().GetResult();
            }
            _disposed = true;
        }
    }
}
