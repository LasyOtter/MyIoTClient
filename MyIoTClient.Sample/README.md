# MyIoTClient 示例项目 v2.0

本示例项目演示如何使用 MyIoTClient 库与各种工业通讯协议设备进行通信。

参照开源项目: [IoTClient.Examples](https://github.com/zhaopeiym/IoTClient.Examples)

## ✨ v2.0 新特性

- **更友好的交互界面** - 清晰的主菜单和子菜单系统
- **配置管理功能** - 实时查看和编辑连接配置
- **批量读写测试** - 多地址批量测试功能
- **日志显示功能** - 带时间戳和颜色的详细日志输出
- **连接状态监控** - 实时显示连接状态和测试结果
- **性能计时** - 显示操作耗时统计

## 支持的协议

1. **Modbus TCP** - 通用 Modbus TCP 协议
2. **Modbus RTU** - 串口 Modbus 协议
3. **三菱MC协议** - 三菱 PLC 通讯
4. **欧姆龙FINS协议** - 欧姆龙 PLC 通讯

## 快速开始

### 前提条件

- .NET 8.0 或更高版本
- 对应的硬件设备或模拟器（可选）

### 编译和运行

```bash
# 编译项目
dotnet build

# 运行示例程序
dotnet run --project MyIoTClient.Sample
```

### 主菜单

```
╔═══════════════════════════════════════════════════════════════════════╗
║                                                                       ║
║              MyIoTClient 物联网协议客户端示例程序 v2.0               ║
║                                                                       ║
╚═══════════════════════════════════════════════════════════════════════╝

【主菜单】

  [1] Modbus TCP 协议 - 以太网通讯
  [2] Modbus RTU 协议 - 串口通讯
  [3] 三菱MC协议      - 三菱PLC通讯
  [4] 欧姆龙FINS协议  - 欧姆龙PLC通讯
  [5] 批量读写测试    - 多地址测试
  [6] 配置管理        - 查看和编辑配置
  [7] 运行实际连接示例 (需要真实设备)
  [0] 退出程序


请选择功能 (0-7):
```

## 功能说明

### 1. 协议演示 (选项 1-4)

显示各协议的详细信息：
- 支持的地址格式
- 可用操作说明
- 代码示例
- 配置参数

### 2. 批量读写测试 (选项 5)

批量测试功能支持：
- **多地址连续读取** - 测试多个连续地址的读取性能
- **多功能测试** (Modbus RTU) - 测试不同功能码的操作
- **批量读写** (三菱MC) - 批量写入后读取验证
- **多区域测试** (欧姆龙FINS) - 测试CIO/DM/WR等不同区域
- **CPU状态读取** (欧姆龙FINS) - 读取CPU运行状态
- **性能计时** - 显示总耗时统计

示例输出：
```
[15:30:45.123] [INFO] 开始批量测试 Modbus TCP - 192.168.1.100:502
[15:30:46.234] [INFO] 连接成功
[15:30:46.345] [INFO] 开始批量读取测试...
[15:30:46.456] [INFO] 读取地址 100: 成功
[15:30:46.567] [INFO] 读取地址 110: 成功
[15:30:46.678] [INFO] 读取地址 120: 成功
[15:30:46.789] [INFO] 批量测试完成, 耗时: 1567ms
```

### 3. 配置管理 (选项 6)

配置管理功能：
- **查看所有配置** - 显示当前所有协议的连接参数
- **编辑配置** - 修改IP地址、端口、串口号、波特率等参数
- **配置生效** - 修改后立即生效，下次使用自动应用

配置菜单：
```
╔═══════════════════════════════════════════════════════════╗
║                    配置管理                              ║
╚═══════════════════════════════════════════════════════════╝

  [1] 查看所有配置
  [2] 编辑 Modbus TCP 配置
  [3] 编辑 Modbus RTU 配置
  [4] 编辑 三菱MC协议 配置
  [5] 编辑 欧姆龙FINS协议 配置
  [0] 返回主菜单
```

### 4. 实际连接示例 (选项 7)

运行需要真实设备的连接测试，支持：
- Modbus TCP/RTU
- 三菱MC协议
- 欧姆龙FINS协议

**注意**: 需要先在配置管理中设置正确的设备参数

## 日志输出说明

日志使用颜色编码以便于识别：

-  **🟢 [INFO]**  - 一般信息（绿色）
-  **🟡 [WARN]**  - 警告信息（黄色）
-  **🔴 [ERROR]** - 错误信息（红色）
-  **🔵 [DEBUG]** - 调试信息（青色）

格式：`[时间戳] [级别] 消息内容`

示例：
```
[15:30:45.123] [INFO] ✓ Modbus TCP 客户端创建成功
[15:30:46.234] [ERROR] 连接失败: 无法连接到设备
[15:30:47.345] [WARN] 读取超时，正在重试...
```

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
    "ModbusRtu": {
      "PortName": "COM1",
      "BaudRate": 9600,
      "DataBits": 8,
      "StopBits": "One",
      "Parity": "None"
    },
    "MitsubishiMc": {
      "IpAddress": "192.168.1.100",
      "Port": 5007,
      "NetworkNumber": 0,
      "PcNumber": 255
    },
    "OmronFins": {
      "IpAddress": "192.168.1.50",
      "Port": 9600,
      "RemoteNodeNumber": 1,
      "LocalNodeNumber": 0
    }
  }
}
```

## 使用示例

### 示例 1: 测试 Modbus TCP 连接

```bash
# 1. 启动程序
dotnet run

# 2. 选择配置管理 [6]
# 3. 编辑 Modbus TCP 配置 [2]
# 4. 输入实际设备的IP和端口

# 5. 返回主菜单，选择批量读写测试 [5]
# 6. 选择 Modbus TCP [1]
# 7. 查看测试结果和日志输出
```

### 示例 2: 测试三菱MC协议

```bash
# 1. 启动程序
dotnet run

# 2. 选择配置管理 [6]
# 3. 编辑三菱MC协议配置 [4]
# 4. 输入PLC的IP地址和端口

# 5. 返回主菜单，选择三菱MC协议演示 [3]
# 6. 查看协议说明和代码示例

# 7. 运行实际连接示例 [7]
```

## 代码示例

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
