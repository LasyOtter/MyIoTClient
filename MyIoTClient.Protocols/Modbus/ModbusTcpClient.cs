using MyIoTClient.Core.Models;
using MyIoTClient.Protocols.Base;
using System.Net.Sockets;

namespace MyIoTClient.Protocols.Modbus;

/// <summary>
/// Modbus TCP客户端
/// </summary>
public class ModbusTcpClient : ProtocolClientBase
{
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private readonly TcpConnectionConfig _tcpConfig;
    private ushort _transactionId;

    public ModbusTcpClient(TcpConnectionConfig config) : base(config)
    {
        _tcpConfig = config;
    }

    /// <summary>
    /// 连接到Modbus TCP设备
    /// </summary>
    public override async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(_tcpConfig.IpAddress, _tcpConfig.Port, cancellationToken);
            _stream = _tcpClient.GetStream();
            _isConnected = true;
            return true;
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
        _stream?.Close();
        _tcpClient?.Close();
        _isConnected = false;
        await Task.CompletedTask;
    }

    /// <summary>
    /// 读取保持寄存器
    /// </summary>
    public override async Task<ReadResult> ReadAsync(string address, int length = 1, CancellationToken cancellationToken = default)
    {
        if (!_isConnected || _stream == null)
        {
            return OperationResult.Fail<ReadResult>("未连接到设备");
        }

        try
        {
            if (!ushort.TryParse(address, out var startAddress))
            {
                return OperationResult.Fail<ReadResult>("地址格式错误");
            }

            var request = BuildReadRequest(startAddress, (ushort)length);
            await _stream.WriteAsync(request, cancellationToken);

            var response = new byte[1024];
            var bytesRead = await _stream.ReadAsync(response, cancellationToken);

            if (bytesRead < 9)
            {
                return OperationResult.Fail<ReadResult>("响应数据不完整");
            }

            var dataLength = response[8];
            var data = new byte[dataLength];
            Array.Copy(response, 9, data, 0, dataLength);

            var result = OperationResult.Success<ReadResult>();
            result.Address = address;
            result.Value = data;
            result.DataType = typeof(byte[]);
            return result;
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<ReadResult>($"读取失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 写入单个保持寄存器
    /// </summary>
    public override async Task<WriteResult> WriteAsync(string address, object value, CancellationToken cancellationToken = default)
    {
        if (!_isConnected || _stream == null)
        {
            return OperationResult.Fail<WriteResult>("未连接到设备");
        }

        try
        {
            if (!ushort.TryParse(address, out var registerAddress))
            {
                return OperationResult.Fail<WriteResult>("地址格式错误");
            }

            ushort registerValue = value switch
            {
                ushort us => us,
                short s => (ushort)s,
                int i => (ushort)i,
                _ => (ushort)Convert.ToInt32(value)
            };

            var request = BuildWriteRequest(registerAddress, registerValue);
            await _stream.WriteAsync(request, cancellationToken);

            var response = new byte[12];
            var bytesRead = await _stream.ReadAsync(response, cancellationToken);

            if (bytesRead < 12)
            {
                return OperationResult.Fail<WriteResult>("响应数据不完整");
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

    private byte[] BuildReadRequest(ushort startAddress, ushort length, byte unitId = 1)
    {
        _transactionId++;
        var request = new byte[12];

        // MBAP Header
        request[0] = (byte)(_transactionId >> 8);
        request[1] = (byte)_transactionId;
        request[2] = 0; // Protocol ID
        request[3] = 0;
        request[4] = 0; // Length
        request[5] = 6;
        request[6] = unitId;

        // PDU
        request[7] = 0x03; // Function code: Read Holding Registers
        request[8] = (byte)(startAddress >> 8);
        request[9] = (byte)startAddress;
        request[10] = (byte)(length >> 8);
        request[11] = (byte)length;

        return request;
    }

    private byte[] BuildWriteRequest(ushort address, ushort value, byte unitId = 1)
    {
        _transactionId++;
        var request = new byte[12];

        // MBAP Header
        request[0] = (byte)(_transactionId >> 8);
        request[1] = (byte)_transactionId;
        request[2] = 0; // Protocol ID
        request[3] = 0;
        request[4] = 0; // Length
        request[5] = 6;
        request[6] = unitId;

        // PDU
        request[7] = 0x06; // Function code: Write Single Register
        request[8] = (byte)(address >> 8);
        request[9] = (byte)address;
        request[10] = (byte)(value >> 8);
        request[11] = (byte)value;

        return request;
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _stream?.Dispose();
            _tcpClient?.Dispose();
        }
        base.Dispose(disposing);
    }
}
