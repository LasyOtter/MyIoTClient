namespace MyIoTClient.Core.Models;

/// <summary>
/// 操作结果基类
/// </summary>
public class OperationResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 错误代码
    /// </summary>
    public int ErrorCode { get; set; }

    /// <summary>
    /// 创建成功结果
    /// </summary>
    public static T Success<T>() where T : OperationResult, new()
    {
        return new T { IsSuccess = true };
    }

    /// <summary>
    /// 创建失败结果
    /// </summary>
    public static T Fail<T>(string errorMessage, int errorCode = -1) where T : OperationResult, new()
    {
        return new T
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode
        };
    }
}

/// <summary>
/// 读取结果
/// </summary>
public class ReadResult : OperationResult
{
    /// <summary>
    /// 读取的地址
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// 读取的数据
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// 数据类型
    /// </summary>
    public Type? DataType { get; set; }
}

/// <summary>
/// 写入结果
/// </summary>
public class WriteResult : OperationResult
{
    /// <summary>
    /// 写入的地址
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// 写入的值
    /// </summary>
    public object? Value { get; set; }
}

/// <summary>
/// 批量读取结果
/// </summary>
public class BatchReadResult : OperationResult
{
    /// <summary>
    /// 读取结果列表
    /// </summary>
    public List<ReadResult> Results { get; set; } = new();
}

/// <summary>
/// 批量写入结果
/// </summary>
public class BatchWriteResult : OperationResult
{
    /// <summary>
    /// 写入结果列表
    /// </summary>
    public List<WriteResult> Results { get; set; } = new();
}
