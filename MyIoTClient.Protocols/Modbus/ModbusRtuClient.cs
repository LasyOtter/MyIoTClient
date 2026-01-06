using MyIoTClient.Core.Models;
using MyIoTClient.Protocols.Base;
using System.IO.Ports;

namespace MyIoTClient.Protocols.Modbus;

/// <summary>
/// Modbus RTU客户端（串口通讯）
/// </summary>
public class ModbusRtuClient : ProtocolClientBase
{
    private SerialPort? _serialPort;
    private readonly SerialConnectionConfig _serialConfig;

    public ModbusRtuClient(SerialConnectionConfig config) : base(config)
    {
        _serialConfig = config;
    }

    /// <summary>
    /// 连接到Modbus RTU设备
    /// </summary>
    public override async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _serialPort = new SerialPort
            {
                PortName = _serialConfig.PortName,
                BaudRate = _serialConfig.BaudRate,
                DataBits = _serialConfig.DataBits,
                StopBits = ParseStopBits(_serialConfig.StopBits),
                Parity = ParseParity(_serialConfig.Parity),
                ReadTimeout = _serialConfig.ReadTimeout,
                WriteTimeout = _serialConfig.WriteTimeout
            };

            _serialPort.Open();
            _isConnected = true;
            await Task.CompletedTask;
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
        if (_serialPort?.IsOpen == true)
        {
            _serialPort.Close();
        }
        _isConnected = false;
        await Task.CompletedTask;
    }

    /// <summary>
    /// 读取保持寄存器
    /// </summary>
    public override async Task<ReadResult> ReadAsync(string address, int length = 1, CancellationToken cancellationToken = default)
    {
        if (!_isConnected || _serialPort == null || !_serialPort.IsOpen)
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
            _serialPort.Write(request, 0, request.Length);

            await Task.Delay(50, cancellationToken); // 等待设备响应

            var responseLength = 5 + length * 2;
            var response = new byte[responseLength];
            var bytesRead = _serialPort.Read(response, 0, responseLength);

            if (bytesRead < 5)
            {
                return OperationResult.Fail<ReadResult>("响应数据不完整");
            }

            // 验证CRC
            if (!VerifyCrc(response, bytesRead))
            {
                return OperationResult.Fail<ReadResult>("CRC校验失败");
            }

            var dataLength = response[2];
            var data = new byte[dataLength];
            Array.Copy(response, 3, data, 0, dataLength);

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
        if (!_isConnected || _serialPort == null || !_serialPort.IsOpen)
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
                float f => (ushort)BitConverter.ToUInt16(BitConverter.GetBytes(f), 0),
                _ => (ushort)Convert.ToInt32(value)
            };

            var request = BuildWriteRequest(registerAddress, registerValue);
            _serialPort.Write(request, 0, request.Length);

            await Task.Delay(50, cancellationToken); // 等待设备响应

            var response = new byte[8];
            var bytesRead = _serialPort.Read(response, 0, 8);

            if (bytesRead < 8)
            {
                return OperationResult.Fail<WriteResult>("响应数据不完整");
            }

            // 验证CRC
            if (!VerifyCrc(response, bytesRead))
            {
                return OperationResult.Fail<WriteResult>("CRC校验失败");
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
    /// 批量读取保持寄存器
    /// </summary>
    public override async Task<BatchReadResult> BatchReadAsync(Dictionary<string, int> addresses, CancellationToken cancellationToken = default)
    {
        if (!_isConnected || _serialPort == null || !_serialPort.IsOpen)
        {
            return OperationResult.Fail<BatchReadResult>("未连接到设备");
        }

        var results = new Dictionary<string, ReadResult>();

        try
        {
            foreach (var address in addresses)
            {
                var result = await ReadAsync(address.Key, address.Value, cancellationToken);
                results[address.Key] = result;
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
    /// 批量写入保持寄存器
    /// </summary>
    public override async Task<BatchWriteResult> BatchWriteAsync(Dictionary<string, object> values, CancellationToken cancellationToken = default)
    {
        if (!_isConnected || _serialPort == null || !_serialPort.IsOpen)
        {
            return OperationResult.Fail<BatchWriteResult>("未连接到设备");
        }

        var results = new Dictionary<string, WriteResult>();

        try
        {
            foreach (var value in values)
            {
                var result = await WriteAsync(value.Key, value.Value, cancellationToken);
                results[value.Key] = result;
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
    /// 读取输入寄存器
    /// </summary>
    public async Task<ReadResult> ReadInputRegistersAsync(string address, int length = 1, CancellationToken cancellationToken = default)
    {
        if (!_isConnected || _serialPort == null || !_serialPort.IsOpen)
        {
            return OperationResult.Fail<ReadResult>("未连接到设备");
        }

        try
        {
            if (!ushort.TryParse(address, out var startAddress))
            {
                return OperationResult.Fail<ReadResult>("地址格式错误");
            }

            var request = BuildReadInputRegistersRequest(startAddress, (ushort)length);
            _serialPort.Write(request, 0, request.Length);

            await Task.Delay(50, cancellationToken);

            var responseLength = 5 + length * 2;
            var response = new byte[responseLength];
            var bytesRead = _serialPort.Read(response, 0, responseLength);

            if (bytesRead < 5)
            {
                return OperationResult.Fail<ReadResult>("响应数据不完整");
            }

            if (!VerifyCrc(response, bytesRead))
            {
                return OperationResult.Fail<ReadResult>("CRC校验失败");
            }

            var dataLength = response[2];
            var data = new byte[dataLength];
            Array.Copy(response, 3, data, 0, dataLength);

            var result = OperationResult.Success<ReadResult>();
            result.Address = address;
            result.Value = data;
            result.DataType = typeof(byte[]);
            return result;
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<ReadResult>($"读取输入寄存器失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 读取线圈状态（离散输入）
    /// </summary>
    public async Task<ReadResult> ReadCoilsAsync(string address, int length = 1, CancellationToken cancellationToken = default)
    {
        if (!_isConnected || _serialPort == null || !_serialPort.IsOpen)
        {
            return OperationResult.Fail<ReadResult>("未连接到设备");
        }

        try
        {
            if (!ushort.TryParse(address, out var startAddress))
            {
                return OperationResult.Fail<ReadResult>("地址格式错误");
            }

            var request = BuildReadCoilsRequest(startAddress, (ushort)length);
            _serialPort.Write(request, 0, request.Length);

            await Task.Delay(50, cancellationToken);

            var responseLength = 5 + (length + 7) / 8;
            var response = new byte[responseLength];
            var bytesRead = _serialPort.Read(response, 0, responseLength);

            if (bytesRead < 5)
            {
                return OperationResult.Fail<ReadResult>("响应数据不完整");
            }

            if (!VerifyCrc(response, bytesRead))
            {
                return OperationResult.Fail<ReadResult>("CRC校验失败");
            }

            var result = OperationResult.Success<ReadResult>();
            result.Address = address;
            result.Value = response[2..bytesRead];
            result.DataType = typeof(byte[]);
            return result;
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<ReadResult>($"读取线圈失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 写入多个线圈
    /// </summary>
    public async Task<WriteResult> WriteMultipleCoilsAsync(string address, bool[] values, CancellationToken cancellationToken = default)
    {
        if (!_isConnected || _serialPort == null || !_serialPort.IsOpen)
        {
            return OperationResult.Fail<WriteResult>("未连接到设备");
        }

        try
        {
            if (!ushort.TryParse(address, out var startAddress))
            {
                return OperationResult.Fail<WriteResult>("地址格式错误");
            }

            var request = BuildWriteMultipleCoilsRequest(startAddress, values);
            _serialPort.Write(request, 0, request.Length);

            await Task.Delay(50, cancellationToken);

            var response = new byte[8];
            var bytesRead = _serialPort.Read(response, 0, 8);

            if (bytesRead < 8)
            {
                return OperationResult.Fail<WriteResult>("响应数据不完整");
            }

            if (!VerifyCrc(response, bytesRead))
            {
                return OperationResult.Fail<WriteResult>("CRC校验失败");
            }

            var result = OperationResult.Success<WriteResult>();
            result.Address = address;
            result.Value = values;
            return result;
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<WriteResult>($"写入多个线圈失败: {ex.Message}");
        }
    }

    private byte[] BuildReadRequest(ushort startAddress, ushort length, byte slaveId = 1)
    {
        var request = new byte[8];
        request[0] = slaveId;
        request[1] = 0x03; // Function code: Read Holding Registers
        request[2] = (byte)(startAddress >> 8);
        request[3] = (byte)startAddress;
        request[4] = (byte)(length >> 8);
        request[5] = (byte)length;

        var crc = CalculateCrc(request, 6);
        request[6] = (byte)(crc & 0xFF);
        request[7] = (byte)(crc >> 8);

        return request;
    }

    private byte[] BuildReadInputRegistersRequest(ushort startAddress, ushort length, byte slaveId = 1)
    {
        var request = new byte[8];
        request[0] = slaveId;
        request[1] = 0x04; // Function code: Read Input Registers
        request[2] = (byte)(startAddress >> 8);
        request[3] = (byte)startAddress;
        request[4] = (byte)(length >> 8);
        request[5] = (byte)length;

        var crc = CalculateCrc(request, 6);
        request[6] = (byte)(crc & 0xFF);
        request[7] = (byte)(crc >> 8);

        return request;
    }

    private byte[] BuildReadCoilsRequest(ushort startAddress, ushort length, byte slaveId = 1)
    {
        var request = new byte[8];
        request[0] = slaveId;
        request[1] = 0x01; // Function code: Read Coils
        request[2] = (byte)(startAddress >> 8);
        request[3] = (byte)startAddress;
        request[4] = (byte)(length >> 8);
        request[5] = (byte)length;

        var crc = CalculateCrc(request, 6);
        request[6] = (byte)(crc & 0xFF);
        request[7] = (byte)(crc >> 8);

        return request;
    }

    private byte[] BuildWriteMultipleCoilsRequest(ushort startAddress, bool[] values, byte slaveId = 1)
    {
        var byteCount = (values.Length + 7) / 8;
        var request = new byte[7 + byteCount + 2];
        
        request[0] = slaveId;
        request[1] = 0x0F; // Function code: Write Multiple Coils
        request[2] = (byte)(startAddress >> 8);
        request[3] = (byte)startAddress;
        request[4] = (byte)(values.Length >> 8);
        request[5] = (byte)values.Length;
        request[6] = (byte)byteCount;

        // 填充线圈数据
        for (int i = 0; i < byteCount; i++)
        {
            byte coilByte = 0;
            for (int j = 0; j < 8 && (i * 8 + j) < values.Length; j++)
            {
                if (values[i * 8 + j])
                {
                    coilByte |= (byte)(1 << j);
                }
            }
            request[7 + i] = coilByte;
        }

        var crc = CalculateCrc(request, 7 + byteCount);
        request[7 + byteCount] = (byte)(crc & 0xFF);
        request[7 + byteCount + 1] = (byte)(crc >> 8);

        return request;
    }

    private byte[] BuildWriteRequest(ushort address, ushort value, byte slaveId = 1)
    {
        var request = new byte[8];
        request[0] = slaveId;
        request[1] = 0x06; // Function code: Write Single Register
        request[2] = (byte)(address >> 8);
        request[3] = (byte)address;
        request[4] = (byte)(value >> 8);
        request[5] = (byte)value;

        var crc = CalculateCrc(request, 6);
        request[6] = (byte)(crc & 0xFF);
        request[7] = (byte)(crc >> 8);

        return request;
    }

    private ushort CalculateCrc(byte[] data, int length)
    {
        ushort crc = 0xFFFF;
        for (int i = 0; i < length; i++)
        {
            crc ^= data[i];
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x0001) != 0)
                {
                    crc >>= 1;
                    crc ^= 0xA001;
                }
                else
                {
                    crc >>= 1;
                }
            }
        }
        return crc;
    }

    private bool VerifyCrc(byte[] data, int length)
    {
        if (length < 2) return false;
        var calculatedCrc = CalculateCrc(data, length - 2);
        var receivedCrc = (ushort)(data[length - 2] | (data[length - 1] << 8));
        return calculatedCrc == receivedCrc;
    }

    private StopBits ParseStopBits(string stopBits)
    {
        return stopBits.ToLower() switch
        {
            "1" or "one" => StopBits.One,
            "1.5" or "onepointfive" => StopBits.OnePointFive,
            "2" or "two" => StopBits.Two,
            _ => StopBits.One
        };
    }

    private Parity ParseParity(string parity)
    {
        return parity.ToLower() switch
        {
            "none" => Parity.None,
            "odd" => Parity.Odd,
            "even" => Parity.Even,
            "mark" => Parity.Mark,
            "space" => Parity.Space,
            _ => Parity.None
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _serialPort?.Dispose();
        }
        base.Dispose(disposing);
    }
}
