using MyIoTClient.Core.Models;
using MyIoTClient.Protocols.Base;
using System.Net.Sockets;
using System.Text;
using System.Buffers;

namespace MyIoTClient.Protocols.MitsubishiMc;

/// <summary>
/// 三菱MC协议客户端
/// 支持MC协议（3E帧）的三菱PLC通信
/// </summary>
public class MitsubishiMcClient : ProtocolClientBase
{
    private readonly MitsubishiMcConnectionConfig _mcConfig;
    private TcpClient? _tcpClient;
    private NetworkStream? _networkStream;

    public MitsubishiMcClient(MitsubishiMcConnectionConfig config) : base(config)
    {
        _mcConfig = config;
    }

    /// <summary>
    /// 连接到三菱PLC
    /// </summary>
    public override async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(_mcConfig.IpAddress, _mcConfig.Port, cancellationToken);
            
            if (_tcpClient.Connected)
            {
                _networkStream = _tcpClient.GetStream();
                _networkStream.ReadTimeout = _mcConfig.ReadTimeout;
                _networkStream.WriteTimeout = _mcConfig.WriteTimeout;
                _isConnected = true;
                return true;
            }
            
            return false;
        }
        catch (Exception)
        {
            _isConnected = false;
            return false;
        }
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public override async Task DisconnectAsync()
    {
        try
        {
            _networkStream?.Close();
            _tcpClient?.Close();
        }
        finally
        {
            _isConnected = false;
        }
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// 读取三菱PLC数据
    /// </summary>
    /// <param name="address">地址格式: D0, D100, M0, X0, Y0, R0 等</param>
    /// <param name="length">读取长度（寄存器数量或位数量）</param>
    public override async Task<ReadResult> ReadAsync(string address, int length = 1, CancellationToken cancellationToken = default)
    {
        if (!_isConnected || _networkStream == null)
        {
            return OperationResult.Fail<ReadResult>("未连接到三菱PLC");
        }

        try
        {
            // 解析地址
            var (deviceCode, addressOffset, dataType) = ParseMcAddress(address);
            
            // 构建MC请求帧
            var requestFrame = BuildReadRequestFrame(deviceCode, addressOffset, length);
            
            // 发送请求
            await _networkStream.WriteAsync(requestFrame, cancellationToken);
            await _networkStream.FlushAsync(cancellationToken);
            
            // 接收响应
            var responseBuffer = ArrayPool<byte>.Shared.Rent(2048);
            try
            {
                var bytesRead = await _networkStream.ReadAsync(responseBuffer, cancellationToken);
                
                if (bytesRead <= 0)
                {
                    return OperationResult.Fail<ReadResult>("PLC未响应");
                }
                
                // 验证响应
                if (!_mcConfig.UseBinaryFormat)
                {
                    // ASCII格式处理
                    var response = VerifyAsciiResponse(responseBuffer, bytesRead);
                    if (!response.IsSuccess)
                    {
                        return response;
                    }
                }
                else
                {
                    // 二进制格式处理
                    var response = VerifyBinaryResponse(responseBuffer, bytesRead);
                    if (!response.IsSuccess)
                    {
                        return response;
                    }
                }
                
                // 解析响应数据
                var data = ParseReadResponse(responseBuffer, bytesRead, length, dataType);
                
                var result = OperationResult.Success<ReadResult>();
                result.Address = address;
                result.Value = data;
                result.DataType = dataType;
                return result;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(responseBuffer);
            }
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<ReadResult>($"读取失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 写入三菱PLC数据
    /// </summary>
    public override async Task<WriteResult> WriteAsync(string address, object value, CancellationToken cancellationToken = default)
    {
        if (!_isConnected || _networkStream == null)
        {
            return OperationResult.Fail<WriteResult>("未连接到三菱PLC");
        }

        try
        {
            // 解析地址
            var (deviceCode, addressOffset, dataType) = ParseMcAddress(address);
            
            // 构建MC写入请求帧
            var requestFrame = BuildWriteRequestFrame(deviceCode, addressOffset, value, dataType);
            
            // 发送请求
            await _networkStream.WriteAsync(requestFrame, 0, requestFrame.Length, cancellationToken);
            await _networkStream.FlushAsync(cancellationToken);
            
            // 接收响应
            var responseBuffer = new byte[2048];
            var bytesRead = await _networkStream.ReadAsync(responseBuffer, 0, responseBuffer.Length, cancellationToken);
            
            if (bytesRead <= 0)
            {
                return OperationResult.Fail<WriteResult>("PLC未响应");
            }
            
            // 验证响应
            if (!_mcConfig.UseBinaryFormat)
            {
                // ASCII格式处理
                var response = VerifyAsciiResponse(responseBuffer, bytesRead);
                if (!response.IsSuccess)
                {
                    return OperationResult.Fail<WriteResult>(response.ErrorMessage ?? "写入失败");
                }
            }
            else
            {
                // 二进制格式处理
                var response = VerifyBinaryResponse(responseBuffer, bytesRead);
                if (!response.IsSuccess)
                {
                    return OperationResult.Fail<WriteResult>(response.ErrorMessage ?? "写入失败");
                }
            }
            
            var result = OperationResult.Success<WriteResult>();
            result.Address = address;
            result.Value = value;
            return result;
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<WriteResult>($"写入失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 批量读取
    /// </summary>
    public override async Task<BatchReadResult> BatchReadAsync(Dictionary<string, int> addresses, CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, ReadResult>();
        
        try
        {
            foreach (var address in addresses)
            {
                var result = await ReadAsync(address.Key, address.Value, cancellationToken);
                results[address.Key] = result;
                
                if (!result.IsSuccess)
                {
                    break;
                }
            }
            
            var batchResult = OperationResult.Success<BatchReadResult>();
            batchResult.Results = results;
            return batchResult;
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<BatchReadResult>($"批量读取失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 批量写入
    /// </summary>
    public override async Task<BatchWriteResult> BatchWriteAsync(Dictionary<string, object> values, CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, WriteResult>();
        
        try
        {
            foreach (var value in values)
            {
                var result = await WriteAsync(value.Key, value.Value, cancellationToken);
                results[value.Key] = result;
                
                if (!result.IsSuccess)
                {
                    break;
                }
            }
            
            var batchResult = OperationResult.Success<BatchWriteResult>();
            batchResult.Results = results;
            return batchResult;
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<BatchWriteResult>($"批量写入失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 解析MC地址
    /// </summary>
    private (string deviceCode, int addressOffset, Type dataType) ParseMcAddress(string address)
    {
        string deviceCode;
        string addressPart;

        if (address.Length >= 2 && char.IsLetter(address[0]) && char.IsLetter(address[1]))
        {
            deviceCode = address[..2].ToUpper();
            addressPart = address[2..];
        }
        else
        {
            deviceCode = address[..1].ToUpper();
            addressPart = address[1..];
        }
        
        if (!int.TryParse(addressPart, out var offset))
        {
            throw new ArgumentException($"地址格式错误: {address}");
        }
        
        return deviceCode switch
        {
            "D" => ("D*", offset, typeof(ushort[])),
            "M" => ("M*", offset, typeof(bool[])),
            "X" => ("X*", offset, typeof(bool[])),
            "Y" => ("Y*", offset, typeof(bool[])),
            "L" => ("L*", offset, typeof(bool[])),
            "F" => ("F*", offset, typeof(bool[])),
            "V" => ("V*", offset, typeof(bool[])),
            "B" => ("B*", offset, typeof(ushort[])),
            "W" => ("W*", offset, typeof(ushort[])),
            "R" => ("R*", offset, typeof(ushort[])),
            "Z" => ("Z*", offset, typeof(ushort[])),
            _ => throw new ArgumentException($"不支持的设备类型: {deviceCode}")
        };
    }

    /// <summary>
    /// 构建MC读取请求帧（ASCII格式）
    /// </summary>
    private byte[] BuildReadRequestFrame(string deviceCode, int addressOffset, int length)
    {
        var deviceCodeBytes = Encoding.ASCII.GetBytes(deviceCode);
        var addressBytes = Encoding.ASCII.GetBytes(addressOffset.ToString("D6"));
        var lengthBytes = Encoding.ASCII.GetBytes(length.ToString("D4"));
        
        var frame = new List<byte>();
        
        // 副头部
        frame.AddRange(Encoding.ASCII.GetBytes("5000"));
        // 网络号
        frame.AddRange(Encoding.ASCII.GetBytes(_mcConfig.NetworkNumber.ToString("X2")));
        // PC号
        frame.AddRange(Encoding.ASCII.GetBytes(_mcConfig.PcNumber.ToString("X2")));
        // 请求目标模块I/O号
        frame.AddRange(Encoding.ASCII.GetBytes(_mcConfig.TargetModuleIoNumber.ToString("X4")));
        // 请求目标模块站号
        frame.AddRange(Encoding.ASCII.GetBytes(_mcConfig.TargetModuleStationNumber.ToString("X2")));
        
        // 请求数据长度（稍后计算）
        var dataLengthPos = frame.Count;
        frame.AddRange(Encoding.ASCII.GetBytes("0000"));
        
        // CPU监视定时器
        frame.AddRange(Encoding.ASCII.GetBytes(_mcConfig.CpuWatchTimer.ToString("X4")));
        
        // 命令
        frame.AddRange(Encoding.ASCII.GetBytes("0401"));
        // 子命令
        frame.AddRange(Encoding.ASCII.GetBytes("0000"));
        
        // 设备类型
        frame.AddRange(deviceCodeBytes);
        // 设备点数
        frame.AddRange(lengthBytes);
        // 起始地址
        frame.AddRange(addressBytes);
        
        // 计算数据长度
        var dataLength = frame.Count - dataLengthPos;
        var dataLengthStr = dataLength.ToString("D4");
        var dataLengthBytes = Encoding.ASCII.GetBytes(dataLengthStr);
        frame.RemoveRange(dataLengthPos, 4);
        frame.InsertRange(dataLengthPos, dataLengthBytes);
        
        return frame.ToArray();
    }

    /// <summary>
    /// 构建MC写入请求帧（ASCII格式）
    /// </summary>
    private byte[] BuildWriteRequestFrame(string deviceCode, int addressOffset, object value, Type dataType)
    {
        var deviceCodeBytes = Encoding.ASCII.GetBytes(deviceCode);
        var addressBytes = Encoding.ASCII.GetBytes(addressOffset.ToString("D6"));
        
        var frame = new List<byte>();
        
        // 副头部
        frame.AddRange(Encoding.ASCII.GetBytes("5000"));
        // 网络号
        frame.AddRange(Encoding.ASCII.GetBytes(_mcConfig.NetworkNumber.ToString("X2")));
        // PC号
        frame.AddRange(Encoding.ASCII.GetBytes(_mcConfig.PcNumber.ToString("X2")));
        // 请求目标模块I/O号
        frame.AddRange(Encoding.ASCII.GetBytes(_mcConfig.TargetModuleIoNumber.ToString("X4")));
        // 请求目标模块站号
        frame.AddRange(Encoding.ASCII.GetBytes(_mcConfig.TargetModuleStationNumber.ToString("X2")));
        
        // 请求数据长度（稍后计算）
        var dataLengthPos = frame.Count;
        frame.AddRange(Encoding.ASCII.GetBytes("0000"));
        
        // CPU监视定时器
        frame.AddRange(Encoding.ASCII.GetBytes(_mcConfig.CpuWatchTimer.ToString("X4")));
        
        // 命令
        frame.AddRange(Encoding.ASCII.GetBytes("1401"));
        // 子命令
        frame.AddRange(Encoding.ASCII.GetBytes("0000"));
        
        // 设备类型
        frame.AddRange(deviceCodeBytes);
        
        // 处理写入数据
        int pointCount;
        if (dataType == typeof(bool[]) && value is bool[] boolValueArray)
        {
            // 位设备写入
            var boolBytes = new List<byte>();
            foreach (var bit in boolValueArray)
            {
                boolBytes.AddRange(Encoding.ASCII.GetBytes(bit ? "1" : "0"));
            }
            frame.AddRange(boolBytes);
            pointCount = boolValueArray.Length;
        }
        else if (dataType == typeof(ushort[]) && value is ushort[] ushortValueArray)
        {
            // 字设备写入
            var dataBytes = new List<byte>();
            foreach (var word in ushortValueArray)
            {
                dataBytes.AddRange(Encoding.ASCII.GetBytes(word.ToString("X4")));
            }
            frame.AddRange(dataBytes);
            pointCount = ushortValueArray.Length;
        }
        else
        {
            throw new ArgumentException($"不支持的数据类型: {dataType}");
        }
        
        // 设备点数
        frame.AddRange(Encoding.ASCII.GetBytes(pointCount.ToString("D4")));
        // 起始地址
        frame.AddRange(addressBytes);
        
        // 计算数据长度
        var dataLength = frame.Count - dataLengthPos;
        var dataLengthStr = dataLength.ToString("D4");
        var dataLengthBytes = Encoding.ASCII.GetBytes(dataLengthStr);
        frame.RemoveRange(dataLengthPos, 4);
        frame.InsertRange(dataLengthPos, dataLengthBytes);
        
        return frame.ToArray();
    }

    /// <summary>
    /// 验证ASCII响应
    /// </summary>
    private ReadResult VerifyAsciiResponse(byte[] response, int length)
    {
        if (length < 22)
        {
            return OperationResult.Fail<ReadResult>("响应数据太短");
        }
        
        var responseStr = Encoding.ASCII.GetString(response, 0, length);
        
        // 检查结束代码
        var endCode = responseStr.Substring(18, 4);
        if (endCode != "0000")
        {
            return OperationResult.Fail<ReadResult>($"PLC响应错误，错误代码: {endCode}");
        }
        
        return OperationResult.Success<ReadResult>();
    }

    /// <summary>
    /// 验证二进制响应
    /// </summary>
    private ReadResult VerifyBinaryResponse(byte[] response, int length)
    {
        if (length < 11)
        {
            return OperationResult.Fail<ReadResult>("响应数据太短");
        }
        
        // 检查结束代码
        var endCode = BitConverter.ToUInt16(response, 9);
        if (endCode != 0)
        {
            return OperationResult.Fail<ReadResult>($"PLC响应错误，错误代码: {endCode:X4}");
        }
        
        return OperationResult.Success<ReadResult>();
    }

    /// <summary>
    /// 解析读取响应
    /// </summary>
    private object ParseReadResponse(byte[] response, int length, int count, Type dataType)
    {
        if (dataType == typeof(bool[]))
        {
            // 位设备数据
            var bitData = new bool[count];
            var dataStart = 18; // ASCII格式数据开始位置
            
            for (int i = 0; i < count; i++)
            {
                if (dataStart + i < length)
                {
                    var bitChar = Encoding.ASCII.GetString(response, dataStart + i, 1);
                    bitData[i] = bitChar == "1";
                }
            }
            
            return bitData;
        }
        else if (dataType == typeof(ushort[]))
        {
            // 字设备数据
            var wordData = new ushort[count];
            var dataStart = 22; // ASCII格式数据开始位置
            
            for (int i = 0; i < count; i++)
            {
                var wordStr = Encoding.ASCII.GetString(response, dataStart + i * 4, 4);
                if (ushort.TryParse(wordStr, System.Globalization.NumberStyles.HexNumber, null, out var word))
                {
                    wordData[i] = word;
                }
            }
            
            return wordData;
        }
        
        throw new ArgumentException($"不支持的数据类型: {dataType}");
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _networkStream?.Dispose();
            _tcpClient?.Dispose();
        }
        base.Dispose(disposing);
    }
}