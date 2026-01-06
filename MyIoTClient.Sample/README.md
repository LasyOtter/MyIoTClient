# MyIoTClient 示例项目

本示例项目演示如何使用 MyIoTClient 库与各种工业通讯协议设备进行通信。

## 支持的协议

1. **Modbus TCP** - 通用 Modbus TCP 协议
2. **Modbus RTU** - 串口 Modbus 协议
3. **三菱MC协议** - 三菱 PLC 通讯
4. **欧姆龙FINS协议** - 欧姆龙 PLC 通讯

## 运行示例

### 前提条件

- .NET 8.0 或更高版本
- 对应的硬件设备或模拟器（可选，示例中不包含实际连接）

### 编译和运行

```bash
# 编译项目
dotnet build

# 运行示例程序
dotnet run
```

## 使用说明

示例程序启动后会显示一个交互式菜单，您可以选择要演示的协议：

```
请选择要演示的协议:
  1. Modbus TCP
  2. Modbus RTU
  3. 三菱MC协议
  4. 欧姆龙FINS协议
  0. 退出程序
```

选择相应的数字即可查看该协议的详细说明和代码示例。

## 配置文件

项目包含 `appsettings.json` 配置文件，您可以在此修改默认的连接参数：

```json
{
  "ConnectionSettings": {
    "ModbusTcp": {
      "IpAddress": "192.168.1.100",
      "Port": 502,
      "ConnectionTimeout": 5000,
      "ReadTimeout": 3000,
      "WriteTimeout": 3000
    },
    ...
  }
}
```

## 代码示例

### Modbus TCP

```csharp
using MyIoTClient.Core.Models;
using MyIoTClient.Protocols.Modbus;

// 创建配置
var config = new TcpConnectionConfig
{
    IpAddress = "192.168.1.100",
    Port = 502,
    ConnectionTimeout = 5000,
    ReadTimeout = 3000,
    WriteTimeout = 3000
};

// 创建客户端
using var client = new ModbusTcpClient(config);

// 连接到设备
await client.ConnectAsync();

// 读取保持寄存器
var result = await client.ReadAsync("100", 10);
if (result.IsSuccess)
{
    var data = (byte[])result.Value;
    Console.WriteLine($"读取到的值: {BitConverter.ToString(data)}");
}

// 写入单个寄存器
var writeResult = await client.WriteAsync("100", 1234);
if (writeResult.IsSuccess)
{
    Console.WriteLine("写入成功");
}

// 断开连接
await client.DisconnectAsync();
```

### Modbus RTU

```csharp
using MyIoTClient.Core.Models;
using MyIoTClient.Protocols.Modbus;

// 创建串口配置
var config = new SerialConnectionConfig
{
    PortName = "COM1",
    BaudRate = 9600,
    DataBits = 8,
    StopBits = "One",
    Parity = "None",
    ConnectionTimeout = 5000,
    ReadTimeout = 3000,
    WriteTimeout = 3000
};

// 创建客户端
using var client = new ModbusRtuClient(config);

// 连接到设备
await client.ConnectAsync();

// 读取保持寄存器
var result = await client.ReadAsync("100", 10);
if (result.IsSuccess)
{
    var data = (short[])result.Value;
    Console.WriteLine($"读取到的值: {string.Join(", ", data)}");
}

// 读取输入寄存器
var inputResult = await client.ReadInputRegistersAsync("0", 5);
if (inputResult.IsSuccess)
{
    var data = (short[])inputResult.Value;
    Console.WriteLine($"输入寄存器值: {string.Join(", ", data)}");
}

// 读取线圈状态
var coilResult = await client.ReadCoilsAsync("0", 16);
if (coilResult.IsSuccess)
{
    var data = (bool[])coilResult.Value;
    Console.WriteLine($"线圈状态: {string.Join(", ", data)}");
}

// 写入单个线圈
var writeCoil = await client.WriteCoilAsync("0", true);
if (writeCoil.IsSuccess)
{
    Console.WriteLine("写入线圈成功");
}

// 断开连接
await client.DisconnectAsync();
```

### 三菱MC协议

```csharp
using MyIoTClient.Core.Models;
using MyIoTClient.Protocols.MitsubishiMc;

// 创建配置
var config = new MitsubishiMcConnectionConfig
{
    IpAddress = "192.168.1.100",
    Port = 5007,
    NetworkNumber = 0,
    PcNumber = 0xFF,
    ConnectionTimeout = 5000,
    ReadTimeout = 3000,
    WriteTimeout = 3000
};

// 创建客户端
using var client = new MitsubishiMcClient(config);

// 连接到设备
await client.ConnectAsync();

// 读取数据寄存器
var result = await client.ReadAsync("D0", 10);
if (result.IsSuccess)
{
    var data = (short[])result.Value;
    Console.WriteLine($"读取到的值: {string.Join(", ", data)}");
}

// 写入数据寄存器
var values = new short[] { 100, 200, 300 };
var writeResult = await client.WriteAsync("D0", values);
if (writeResult.IsSuccess)
{
    Console.WriteLine("写入成功");
}

// 读取辅助继电器
var mResult = await client.ReadAsync("M0", 10);
if (mResult.IsSuccess)
{
    var data = (bool[])mResult.Value;
    Console.WriteLine($"辅助继电器状态: {string.Join(", ", data)}");
}

// 批量读取
var addresses = new Dictionary<string, int>
{
    { "D0", 5 },
    { "D100", 3 },
    { "M0", 10 }
};
var batchResult = await client.BatchReadAsync(addresses);
if (batchResult.IsSuccess)
{
    foreach (var item in batchResult.Results)
    {
        Console.WriteLine($"{item.Address}: {item.Value}");
    }
}

// 断开连接
await client.DisconnectAsync();
```

### 欧姆龙FINS协议

```csharp
using MyIoTClient.Core.Models;
using MyIoTClient.Protocols.OmronFins;

// 创建配置
var config = new OmronFinsConnectionConfig
{
    IpAddress = "192.168.1.50",
    Port = 9600,
    RemoteNodeNumber = 1,
    LocalNodeNumber = 0,
    LocalNetworkNumber = 0,
    RemoteNetworkNumber = 0,
    ConnectionTimeout = 5000,
    ReadTimeout = 3000,
    WriteTimeout = 3000
};

// 创建客户端
using var client = new OmronFinsClient(config);

// 连接到设备
await client.ConnectAsync();

// 读取CIO区域
var cioResult = await client.ReadAsync("CIO0", 10);
if (cioResult.IsSuccess)
{
    var data = (short[])cioResult.Value;
    Console.WriteLine($"CIO值: {string.Join(", ", data)}");
}

// 读取DM区域
var dmResult = await client.ReadAsync("DM0", 10);
if (dmResult.IsSuccess)
{
    var data = (short[])dmResult.Value;
    Console.WriteLine($"DM值: {string.Join(", ", data)}");
}

// 写入DM区域
var values = new short[] { 100, 200, 300 };
var writeResult = await client.WriteAsync("DM0", values);
if (writeResult.IsSuccess)
{
    Console.WriteLine("写入成功");
}

// 读取CPU状态
var cpuResult = await client.ReadCpuStatusAsync();
if (cpuResult.IsSuccess)
{
    Console.WriteLine($"CPU运行模式: {cpuResult.Value.RunMode}");
    Console.WriteLine($"CPU状态: {cpuResult.Value.Status}");
}

// 断开连接
await client.DisconnectAsync();
```

## 地址格式说明

### Modbus

- **地址格式**: 直接使用十进制地址
- **示例**: "100", "200"
- **地址映射**: 地址从0开始，对应Modbus寄存器地址

### 三菱MC协议

| 设备类型 | 描述 | 数据类型 | 示例 |
|---------|------|---------|------|
| D | 数据寄存器 | 16位 | D0, D100 |
| M | 辅助继电器 | 位 | M0, M100 |
| X | 输入继电器 | 位 | X0, X10 |
| Y | 输出继电器 | 位 | Y0, Y10 |
| L | 锁存继电器 | 位 | L0, L100 |
| F | 报警器 | 位 | F0, F100 |
| V | 边界继电器 | 16位 | V0, V100 |
| B | 链接继电器 | 位 | B0, B100 |
| W | 链接寄存器 | 16位 | W0, W100 |
| R | 文件寄存器 | 16位 | R0, R100 |
| Z | 变址寄存器 | 16位 | Z0, Z100 |

### 欧姆龙FINS

| 内存区域 | 描述 | 示例 |
|---------|------|------|
| CIO | 输入输出区 | CIO0, CIO100 |
| WR | 工作区 | WR0, WR100 |
| HR | 保持区 | HR0, HR100 |
| AR | 辅助区 | AR0, AR100 |
| DM | 数据区 | DM0, DM100 |
| DR | 扩展数据区 | DR0, DR100 |
| TIM | 定时器完成标志 | TIM0, TIM100 |
| CNT | 计数器完成标志 | CNT0, CNT100 |
| IR | 中断区 | IR0, IR100 |

## 错误处理

所有操作都返回 OperationResult 对象，包含以下属性：

- `IsSuccess`: 操作是否成功
- `ErrorMessage`: 错误消息（如果失败）
- `ErrorCode`: 错误代码（如果失败）

```csharp
var result = await client.ReadAsync("D0", 10);
if (result.IsSuccess)
{
    // 处理成功情况
}
else
{
    // 处理错误情况
    Console.WriteLine($"读取失败: {result.ErrorMessage}");
}
```

## 注意事项

1. 本示例使用模拟地址，实际使用时请根据设备情况修改配置
2. 建议在实际使用前先在测试环境中验证
3. 确保网络连接正常，防火墙规则允许相应端口
4. 串口操作需要管理员权限（在Windows上）
5. 注意设备地址范围，超出范围可能导致错误

## 更多信息

- 项目主页: https://github.com/zhaopeiym/IoTClient.Examples
- MyIoTClient 文档: [待补充]

## 许可证

本项目遵循原项目许可证。
