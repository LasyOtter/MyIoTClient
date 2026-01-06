# MyIoTClient

这是一个高性能、轻量级的物联网设备通讯协议实现客户端库，支持主流 PLC 通信、串口通信、OPC UA、Modbus 等工业通讯协议。

## 项目特点

- **高性能**: 采用 `.NET 8` 异步编程模型，结合 `ArrayPool` 和 `Span<T>` 优化内存分配与处理性能。
- **易于使用**: 提供统一的接口设计，不同协议间的使用逻辑一致。
- **协议丰富**: 完整支持 Modbus RTU/TCP、三菱 MC、欧姆龙 FINS 等常用工业协议。
- **可扩展性**: 灵活的工厂模式，方便集成自定义协议。

## 技术栈

- **.NET 8.0** - 使用最新的 .NET 平台
- **C# 12** - 现代化的 C# 语言特性

## 项目结构

```
MyIoTClient/
├── MyIoTClient.Core/              # 核心库
│   ├── Interfaces/                # 接口定义
│   │   └── IProtocolClient.cs    # 协议客户端基础接口
│   ├── Models/                    # 数据模型
│   │   ├── ConnectionConfig.cs   # 连接配置
│   │   ├── OperationResult.cs    # 操作结果
│   │   └── DataType.cs           # 数据类型
│   └── Enums/                     # 枚举类型
│       └── ProtocolType.cs       # 协议类型枚举
├── MyIoTClient.Protocols/         # 协议实现库
│   ├── Base/                      # 基础类
│   │   └── ProtocolClientBase.cs # 协议客户端基类
│   ├── Modbus/                    # Modbus协议
│   │   └── ModbusTcpClient.cs    # Modbus TCP客户端
│   ├── OpcUa/                     # OPC UA协议
│   │   └── OpcUaClient.cs        # OPC UA客户端
│   └── Plc/                       # PLC协议
│       └── SiemensS7Client.cs    # 西门子S7客户端
└── MyIoTClient.Sample/            # 示例程序
    └── Program.cs                 # 示例代码

```

## 支持的协议

### 已实现（完整版本）
- ✅ **Modbus TCP** - 工业标准的通讯协议
- ✅ **Modbus RTU** - 串口版本的Modbus协议（完整功能：读写保持寄存器、输入寄存器、线圈等）
- ✅ **OPC UA** - 统一架构的开放平台通讯协议（框架）
- ✅ **BACnet** - 楼宇自控网络协议（框架）
- ✅ **西门子 S7** - 西门子PLC通讯协议（框架）
- ✅ **三菱MC** - 三菱PLC通讯协议（完整实现）
- ✅ **欧姆龙FINS** - 欧姆龙PLC通讯协议（完整实现）

### 计划支持
- 🔄 **三菱MC** - 三菱PLC通讯协议（✅ 已完成完整实现）
- 🔄 **欧姆龙FINS** - 欧姆龙PLC通讯协议（✅ 已完成完整实现）

## 快速开始

### 1. 克隆项目

```bash
git clone <repository-url>
cd MyIoTClient
```

### 2. 构建项目

```bash
dotnet build
```

### 3. 运行示例

```bash
dotnet run --project MyIoTClient.Sample
```

## 使用示例

### Modbus TCP 示例

```csharp
using MyIoTClient.Core.Models;
using MyIoTClient.Protocols.Modbus;

// 创建连接配置
var config = new TcpConnectionConfig
{
    IpAddress = "192.168.1.100",
    Port = 502,
    ConnectionTimeout = 5000,
    ReadTimeout = 3000
};

// 创建客户端
using var client = new ModbusTcpClient(config);

// 连接到设备
var connected = await client.ConnectAsync();
if (connected)
{
    // 读取保持寄存器
    var readResult = await client.ReadAsync("100", 10);
    if (readResult.IsSuccess)
    {
        Console.WriteLine($"读取成功: {readResult.Value}");
    }

    // 写入单个寄存器
    var writeResult = await client.WriteAsync("100", 1234);
    if (writeResult.IsSuccess)
    {
        Console.WriteLine("写入成功");
    }
}

// 断开连接
await client.DisconnectAsync();
```

### OPC UA 示例

```csharp
using MyIoTClient.Protocols.OpcUa;

var config = new OpcUaConnectionConfig
{
    EndpointUrl = "opc.tcp://localhost:4840",
    Username = "admin",
    Password = "password",
    SecurityPolicy = "None"
};

using var client = new OpcUaClient(config);
await client.ConnectAsync();

// 读取节点
var result = await client.ReadAsync("ns=2;s=Device1.Temperature");

// 写入节点
await client.WriteAsync("ns=2;s=Device1.SetPoint", 25.5);
```

### 西门子 S7 PLC 示例

```csharp
using MyIoTClient.Protocols.Plc;

var config = new TcpConnectionConfig
{
    IpAddress = "192.168.1.200",
    Port = 102
};

using var client = new SiemensS7Client(config);
await client.ConnectAsync();

// 读取DB块数据
var result = await client.ReadAsync("DB1.DBW0");

// 写入位存储器
await client.WriteAsync("M0.0", true);
```

### 三菱MC协议示例

```csharp
using MyIoTClient.Protocols.MitsubishiMc;

// 创建连接配置
var config = new MitsubishiMcConnectionConfig
{
    IpAddress = "192.168.1.100",
    Port = 5007,
    NetworkNumber = 0,
    PcNumber = 0xFF,
    UseBinaryFormat = false // 使用ASCII格式
};

using var client = new MitsubishiMcClient(config);
await client.ConnectAsync();

// 读取数据寄存器
var dReadResult = await client.ReadAsync("D0", 10);
if (dReadResult.IsSuccess)
{
    var dValues = dReadResult.Value as ushort[];
    Console.WriteLine($"D0-D9: {string.Join(", ", dValues)}");
}

// 写入辅助继电器
var mWriteResult = await client.WriteAsync("M0", true);
if (mWriteResult.IsSuccess)
{
    Console.WriteLine("M0写入成功");
}

// 批量读取
var batchReadResult = await client.BatchReadAsync(new Dictionary<string, int>
{
    {"D0", 5},
    {"D100", 3}
});
```

### 欧姆龙FINS协议示例

```csharp
using MyIoTClient.Protocols.OmronFins;

var config = new OmronFinsConnectionConfig
{
    IpAddress = "192.168.1.50",
    Port = 9600,
    RemoteNodeNumber = 1,
    UseTcp = true // 使用TCP
};

using var client = new OmronFinsClient(config);
await client.ConnectAsync();

// 读取CIO区域
var cioResult = await client.ReadAsync("CIO0", 10);
if (cioResult.IsSuccess)
{
    var values = cioResult.Value as ushort[];
    Console.WriteLine($"CIO0-CIO9: {string.Join(", ", values)}");
}

// 写入数据内存
var dmWriteResult = await client.WriteAsync("DM0", 12345);
if (dmWriteResult.IsSuccess)
{
    Console.WriteLine("DM0写入成功");
}

// 读取CPU状态
var cpuStatus = await client.ReadCpuStatusAsync();
if (cpuStatus.IsSuccess)
{
    var status = cpuStatus.Value as Dictionary<string, object>;
    Console.WriteLine($"CPU状态: {status["Mode"]}");
}
```

## 核心功能

### IProtocolClient 接口

所有协议客户端都实现此接口，提供统一的API：

- `ConnectAsync()` - 连接到设备
- `DisconnectAsync()` - 断开连接
- `ReadAsync()` - 读取数据
- `WriteAsync()` - 写入数据
- `BatchReadAsync()` - 批量读取
- `BatchWriteAsync()` - 批量写入

### 连接配置

- `TcpConnectionConfig` - TCP连接配置（IP地址、端口）
- `SerialConnectionConfig` - 串口连接配置（端口名、波特率、数据位等）
- `OpcUaConnectionConfig` - OPC UA连接配置（端点URL、认证信息）

### 操作结果

- `ReadResult` - 读取操作结果
- `WriteResult` - 写入操作结果
- `BatchReadResult` - 批量读取结果
- `BatchWriteResult` - 批量写入结果

## 性能优化

本项目针对工业场景下的高频通讯进行了多项优化：

1. **零/低分配 (Zero/Low Allocation)**:
   - 在协议数据帧构建和解析过程中，广泛使用 `ArrayPool<byte>` 租用缓冲区，显著降低了 GC 压力。
   - 利用 `Span<byte>` 和 `ReadOnlySpan<byte>` 进行高效的数据切片和操作，避免不必要的内存拷贝。

2. **异步 I/O (Asynchronous I/O)**:
   - 所有网络和串口通讯均采用原生的异步 API (`ReadAsync`/`WriteAsync`)，确保在高并发连接下系统依然保持响应。

3. **高效校验算法**:
   - Modbus RTU 等协议的校验算法使用了查表法 (Lookup Table)，大幅提升了计算速度。

4. **连接管理**:
   - 优化的连接重试机制和超时处理，保证了通讯的稳定性。

## 开发计划

- [x] 项目架构搭建
- [x] 核心接口定义
- [x] Modbus TCP 基础实现
- [x] Modbus RTU 完整实现
- [x] OPC UA 框架实现
- [x] BACnet 协议实现
- [x] 西门子 S7 框架实现
- [x] 三菱MC协议完整实现
- [x] 欧姆龙FINS协议完整实现
- [ ] 单元测试
- [ ] 性能优化
- [ ] 文档完善

## 贡献

欢迎提交 Issue 和 Pull Request！

## 许可证

本项目采用 MIT 许可证。详见 LICENSE 文件。

## 联系方式

如有问题或建议，请提交 Issue。
