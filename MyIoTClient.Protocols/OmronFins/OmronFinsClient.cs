using MyIoTClient.Core.Models;
using MyIoTClient.Protocols.Base;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MyIoTClient.Protocols.OmronFins;

/// <summary>
/// 欧姆龙FINS协议客户端
/// 支持FINS协议的欧姆龙PLC通信
/// </summary>
public class OmronFinsClient : ProtocolClientBase
{
    private readonly OmronFinsConnectionConfig _finsConfig;
    private TcpClient? _tcpClient;
    private UdpClient? _udpClient;
    private NetworkStream? _networkStream;
    private int _commandId = 0;

    public OmronFinsClient(OmronFinsConnectionConfig config) : base(config)
    {
        _finsConfig = config;
    }

    /// <summary>
    /// 连接到欧姆龙PLC
    /// </summary>
    public override async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_finsConfig.UseTcp)
            {
                // TCP连接
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(_finsConfig.IpAddress, _finsConfig.Port, cancellationToken);
                
                if (_tcpClient.Connected)
                {
                    _networkStream = _tcpClient.GetStream();
                    _networkStream.ReadTimeout = _finsConfig.ReadTimeout;
                    _networkStream.WriteTimeout = _finsConfig.WriteTimeout;
                    _isConnected = true;
                    return true;
                }
            }
            else
            {
                // UDP连接
                _udpClient = new UdpClient(_finsConfig.IpAddress, _finsConfig.FinsUdpPort);
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
            _udpClient?.Close();
        }
        finally
        {
            _isConnected = false;
        }
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// 读取欧姆龙PLC数据
    /// </summary>
    /// <param name="address">地址格式: D0, D100, CIO0, W0, H0, A0 等</param>
    /// <param name="length">读取长度</param>
    public override async Task<ReadResult> ReadAsync(string address, int length = 1, CancellationToken cancellationToken = default)
    {
        if (!_isConnected)
        {
            return OperationResult.Fail<ReadResult>("未连接到欧姆龙PLC");
        }

        try
        {
            // 解析FINS地址
            var (areaCode, addressOffset, dataType) = ParseFinsAddress(address);
            
            // 构建FINS读取命令
            var command = BuildFinsReadCommand(areaCode, addressOffset, length);
            
            // 发送命令并接收响应
            var response = await SendFinsCommandAsync(command, cancellationToken);
            
            if (!response.IsSuccess)
            {
                return response;
            }
            
            // 解析响应数据
            var data = ParseFinsReadResponse(response.Value as byte[] ?? Array.Empty<byte>(), dataType);
            
            var result = OperationResult.Success<ReadResult>();
            result.Address = address;
            result.Value = data;
            result.DataType = dataType;
            return result;
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<ReadResult>($"读取失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 写入欧姆龙PLC数据
    /// </summary>
    public override async Task<WriteResult> WriteAsync(string address, object value, CancellationToken cancellationToken = default)
    {
        if (!_isConnected)
        {
            return OperationResult.Fail<WriteResult>("未连接到欧姆龙PLC");
        }

        try
        {
            // 解析FINS地址
            var (areaCode, addressOffset, dataType) = ParseFinsAddress(address);
            
            // 构建FINS写入命令
            var command = BuildFinsWriteCommand(areaCode, addressOffset, value, dataType);
            
            // 发送命令并接收响应
            var response = await SendFinsCommandAsync(command, cancellationToken);
            
            if (!response.IsSuccess)
            {
                return OperationResult.Fail<WriteResult>(response.ErrorMessage ?? "写入失败");
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
    /// 读取PLC状态信息
    /// </summary>
    public async Task<ReadResult> ReadCpuStatusAsync(CancellationToken cancellationToken = default)
    {
        if (!_isConnected)
        {
            return OperationResult.Fail<ReadResult>("未连接到欧姆龙PLC");
        }

        try
        {
            // FINS命令: CPU状态读取 (Command Code: 07 01)
            var command = new List<byte>();
            command.Add(0x07); // CPU状态读取命令
            command.Add(0x01);
            
            var response = await SendFinsCommandAsync(command.ToArray(), cancellationToken);
            
            if (!response.IsSuccess)
            {
                return response;
            }
            
            var responseData = response.Value as byte[] ?? Array.Empty<byte>();
            
            // 解析CPU状态数据
            var status = new Dictionary<string, object>();
            if (responseData.Length >= 10)
            {
                status["Mode"] = responseData[8]; // 运行模式
                status["RunSwitch"] = responseData[9]; // 运行开关状态
                status["ErrorFlag"] = responseData[10] > 0; // 错误标志
            }
            
            var result = OperationResult.Success<ReadResult>();
            result.Address = "CPU_STATUS";
            result.Value = status;
            result.DataType = typeof(Dictionary<string, object>);
            return result;
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<ReadResult>($"读取CPU状态失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 解析FINS地址
    /// </summary>
    private (byte areaCode, int addressOffset, Type dataType) ParseFinsAddress(string address)
    {
        var areaCode = address[..3]; // 前3个字符作为区域代码
        
        var addressPart = address[3..];
        if (!int.TryParse(addressPart, out var offset))
        {
            throw new ArgumentException($"地址格式错误: {address}");
        }
        
        return areaCode.ToUpper() switch
        {
            "CIO" => (0xB0, offset, typeof(ushort[])), // CIO区域
            "WR" => (0xB1, offset, typeof(ushort[])), // 工作区域
            "HR" => (0xB2, offset, typeof(ushort[])), // 保持区域
            "AR" => (0xB3, offset, typeof(ushort[])), // 辅助区域
            "DM" => (0x82, offset, typeof(ushort[])), // 数据内存
            "DR" => (0xC0, offset, typeof(ushort[])), // 数据寄存器
            "IR" => (0xDD, offset, typeof(ushort[])), // 索引寄存器
            "TIM" => (0x09, offset, typeof(ushort[])), // 定时器
            "CNT" => (0x0C, offset, typeof(ushort[])), // 计数器
            "TIMH" => (0x19, offset, typeof(ushort[])), // 高速定时器
            _ => throw new ArgumentException($"不支持的区域: {areaCode}")
        };
    }

    /// <summary>
    /// 构建FINS读取命令
    /// </summary>
    private byte[] BuildFinsReadCommand(byte areaCode, int addressOffset, int length)
    {
        var command = new List<byte>();
        
        // FINS头部
        command.Add(0x80); // FINS版本
        command.Add(_finsConfig.LocalNetworkNumber);
        command.Add(_finsConfig.LocalNodeNumber);
        command.Add(_finsConfig.LocalUnitNumber);
        command.Add(_finsConfig.RemoteNetworkNumber);
        command.Add(_finsConfig.RemoteNodeNumber);
        command.Add(_finsConfig.RemoteUnitNumber);
        
        // 命令代码
        command.Add(0x01); // 内存读取
        command.Add(0x02);
        
        // 命令ID
        command.Add((byte)(_commandId >> 8));
        command.Add((byte)(_commandId & 0xFF));
        _commandId = (_commandId + 1) % 65536;
        
        // 内存区域代码
        command.Add(areaCode);
        
        // 起始地址
        command.Add((byte)(addressOffset >> 8));
        command.Add((byte)(addressOffset & 0xFF));
        
        // 读取长度
        command.Add((byte)(length >> 8));
        command.Add((byte)(length & 0xFF));
        
        return command.ToArray();
    }

    /// <summary>
    /// 构建FINS写入命令
    /// </summary>
    private byte[] BuildFinsWriteCommand(byte areaCode, int addressOffset, object value, Type dataType)
    {
        var command = new List<byte>();
        
        // FINS头部
        command.Add(0x80); // FINS版本
        command.Add(_finsConfig.LocalNetworkNumber);
        command.Add(_finsConfig.LocalNodeNumber);
        command.Add(_finsConfig.LocalUnitNumber);
        command.Add(_finsConfig.RemoteNetworkNumber);
        command.Add(_finsConfig.RemoteNodeNumber);
        command.Add(_finsConfig.RemoteUnitNumber);
        
        // 命令代码
        command.Add(0x01); // 内存写入
        command.Add(0x03);
        
        // 命令ID
        command.Add((byte)(_commandId >> 8));
        command.Add((byte)(_commandId & 0xFF));
        _commandId = (_commandId + 1) % 65536;
        
        // 内存区域代码
        command.Add(areaCode);
        
        // 起始地址
        command.Add((byte)(addressOffset >> 8));
        command.Add((byte)(addressOffset & 0xFF));
        
        // 写入长度
        if (dataType == typeof(ushort[]))
        {
            var values = value as ushort[] ?? throw new ArgumentException("值类型不正确");
            command.Add((byte)(values.Length >> 8));
            command.Add((byte)(values.Length & 0xFF));
            
            // 写入数据
            foreach (var val in values)
            {
                command.Add((byte)(val >> 8));
                command.Add((byte)(val & 0xFF));
            }
        }
        else
        {
            throw new ArgumentException($"不支持的数据类型: {dataType}");
        }
        
        return command.ToArray();
    }

    /// <summary>
    /// 发送FINS命令
    /// </summary>
    private async Task<ReadResult> SendFinsCommandAsync(byte[] command, CancellationToken cancellationToken = default)
    {
        for (int retry = 0; retry < _finsConfig.RetryCount; retry++)
        {
            try
            {
                if (_finsConfig.UseTcp && _networkStream != null)
                {
                    // TCP发送
                    await _networkStream.WriteAsync(command, 0, command.Length, cancellationToken);
                    await _networkStream.FlushAsync(cancellationToken);
                    
                    // 接收响应
                    var responseBuffer = new byte[8192];
                    var bytesRead = await _networkStream.ReadAsync(responseBuffer, 0, responseBuffer.Length, cancellationToken);
                    
                    if (bytesRead > 0)
                    {
                        var response = new byte[bytesRead];
                        Array.Copy(responseBuffer, response, bytesRead);
                        
                        // 验证响应
                        if (VerifyFinsResponse(response))
                        {
                            return OperationResult.Success<ReadResult> { Value = response };
                        }
                    }
                }
                else if (_udpClient != null)
                {
                    // UDP发送
                    var endpoint = new IPEndPoint(IPAddress.Parse(_finsConfig.IpAddress), _finsConfig.FinsUdpPort);
                    await _udpClient.SendAsync(command, command.Length, endpoint);
                    
                    // 接收响应
                    var response = await _udpClient.ReceiveAsync(cancellationToken);
                    
                    // 验证响应
                    if (VerifyFinsResponse(response.Buffer))
                    {
                        return OperationResult.Success<ReadResult> { Value = response.Buffer };
                    }
                }
            }
            catch (Exception ex)
            {
                if (retry == _finsConfig.RetryCount - 1)
                {
                    return OperationResult.Fail<ReadResult>($"通信失败: {ex.Message}");
                }
            }
        }
        
        return OperationResult.Fail<ReadResult>("通信失败，已达到最大重试次数");
    }

    /// <summary>
    /// 验证FINS响应
    /// </summary>
    private bool VerifyFinsResponse(byte[] response)
    {
        if (response.Length < 10)
        {
            return false;
        }
        
        // 检查FINS头部
        if (response[0] != 0x80) // FINS版本
        {
            return false;
        }
        
        // 检查响应代码
        var responseCode = (response[8] << 8) | response[9];
        return responseCode == 0x0000; // 0x0000表示成功
    }

    /// <summary>
    /// 解析FINS读取响应
    /// </summary>
    private object ParseFinsReadResponse(byte[] response, Type dataType)
    {
        if (dataType == typeof(ushort[]))
        {
            var dataLength = (response.Length - 10) / 2;
            var values = new ushort[dataLength];
            
            for (int i = 0; i < dataLength; i++)
            {
                values[i] = (ushort)((response[10 + i * 2] << 8) | response[11 + i * 2]);
            }
            
            return values;
        }
        
        throw new ArgumentException($"不支持的数据类型: {dataType}");
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _networkStream?.Dispose();
            _tcpClient?.Dispose();
            _udpClient?.Dispose();
        }
        base.Dispose(disposing);
    }
}