# MyIoTClient 快速开始指南

## 项目结构

```
MyIoTClient.Sample/
├── Program.cs                   # 主程序入口，包含交互式菜单和演示代码
├── ActualConnectionExample.cs   # 实际连接示例（需要真实设备）
├── DemoRunner.cs               # 示例运行器
├── appsettings.json            # 配置文件
├── MyIoTClient.Sample.csproj   # 项目文件
├── README.md                   # 详细文档
└── QuickStart.md               # 本文件（快速开始指南）
```

## 快速运行

### 1. 编译项目

```bash
cd /path/to/MyIoTClient
dotnet build
```

### 2. 运行示例程序

```bash
dotnet run --project MyIoTClient.Sample
```

## 主菜单说明

启动程序后，您将看到以下菜单：

```
╔═══════════════════════════════════════════════════════════╗
║          MyIoTClient 物联网协议客户端示例程序            ║
╚═══════════════════════════════════════════════════════════╝

请选择要演示的协议:
  1. Modbus TCP
  2. Modbus RTU
  3. 三菱MC协议
  4. 欧姆龙FINS协议
  5. 运行实际连接示例 (需要真实设备)
  0. 退出程序

请输入选项 (0-5):
```

### 选项说明

- **选项 1-4**: 查看各协议的演示说明和代码示例（不需要实际设备）
- **选项 5**: 运行实际连接示例（需要真实硬件设备或模拟器）
- **选项 0**: 退出程序

## 协议详解

### Modbus TCP

**特点**:
- 基于 TCP/IP 的标准 Modbus 协议
- 适用于以太网连接的设备
- 标准端口: 502

**主要功能**:
- 读取保持寄存器 (功能码 0x03)
- 写入单个寄存器 (功能码 0x06)

**适用场景**:
- 工业自动化设备
- 能源管理系统
- 环境监测设备

### Modbus RTU

**特点**:
- 基于 RS-485/RS-232 串口通信
- 使用 CRC 校验
- 串口参数可配置（波特率、数据位等）

**主要功能**:
- 读取保持寄存器 (功能码 0x03)
- 读取输入寄存器 (功能码 0x04)
- 读取线圈状态 (功能码 0x01)
- 写入单个/多个寄存器
- 写入单个/多个线圈

**适用场景**:
- 传统串口设备
- 传感器网络
- 现场总线系统

### 三菱MC协议

**特点**:
- 三菱 PLC 专用协议
- 支持 3E 帧
- 可选择 ASCII 或二进制格式

**支持的设备类型**:
- D: 数据寄存器 (16位)
- M: 辅助继电器 (位)
- X: 输入继电器 (位)
- Y: 输出继电器 (位)
- L: 锁存继电器 (位)
- F: 报警器 (位)
- V: 边界继电器 (16位)
- B: 链接继电器 (位)
- W: 链接寄存器 (16位)
- R: 文件寄存器 (16位)
- Z: 变址寄存器 (16位)

**适用场景**:
- 三菱 PLC 设备
- 工业控制系统

### 欧姆龙FINS协议

**特点**:
- 欧姆龙 PLC 专用协议
- 支持 TCP 和 UDP 通信
- 自动命令ID管理

**支持的内存区域**:
- CIO: 输入输出区
- WR: 工作区
- HR: 保持区
- AR: 辅助区
- DM: 数据区
- DR: 扩展数据区
- TIM: 定时器完成标志
- CNT: 计数器完成标志
- IR: 中断区

**适用场景**:
- 欧姆龙 PLC 设备
- 工厂自动化系统

## 配置说明

### 修改连接参数

编辑 `appsettings.json` 文件，修改连接参数：

```json
{
  "ConnectionSettings": {
    "ModbusTcp": {
      "IpAddress": "192.168.1.100",  // 修改为实际设备IP
      "Port": 502
    }
  }
}
```

### 在代码中使用配置

```csharp
var config = new TcpConnectionConfig
{
    IpAddress = "192.168.1.100",  // 您的设备IP
    Port = 502
};
```

## 代码示例模板

### 基本读写操作模板

```csharp
// 1. 创建配置
var config = new TcpConnectionConfig
{
    IpAddress = "192.168.1.100",
    Port = 502
};

// 2. 创建客户端
using var client = new ModbusTcpClient(config);

// 3. 连接
await client.ConnectAsync();

// 4. 读取数据
var result = await client.ReadAsync("0", 10);
if (result.IsSuccess)
{
    // 处理读取的数据
}

// 5. 写入数据
var writeResult = await client.WriteAsync("0", 1234);
if (writeResult.IsSuccess)
{
    // 写入成功
}

// 6. 断开连接
await client.DisconnectAsync();
```

### 错误处理模板

```csharp
try
{
    var result = await client.ReadAsync("D0", 10);

    if (result.IsSuccess)
    {
        // 成功处理
        var data = (short[])result.Value;
        Console.WriteLine($"数据: {string.Join(", ", data)}");
    }
    else
    {
        // 业务错误处理
        Console.WriteLine($"读取失败: {result.ErrorMessage}");
        Console.WriteLine($"错误代码: {result.ErrorCode}");
    }
}
catch (Exception ex)
{
    // 异常处理
    Console.WriteLine($"发生异常: {ex.Message}");
}
```

## 常见问题

### Q1: 连接失败怎么办？

**检查清单**:
1. 确认设备IP地址是否正确
2. 检查网络连接是否正常
3. 确认防火墙是否允许相应端口
4. 检查设备是否在线
5. 验证端口是否正确（如Modbus TCP默认端口为502）

### Q2: 读取返回空数据或错误

**检查清单**:
1. 确认地址格式是否正确
2. 检查读取长度是否超出设备范围
3. 确认设备是否支持该地址类型
4. 检查设备权限设置

### Q3: 串口操作失败

**检查清单**:
1. 确认串口名称是否正确（Windows为COMx，Linux为/dev/ttyUSBx）
2. 检查串口是否被其他程序占用
3. 验证串口参数（波特率、数据位等）是否与设备匹配
4. 在Windows上以管理员权限运行程序

### Q4: 如何查看详细的调试信息？

在代码中添加日志输出：

```csharp
Console.WriteLine($"连接到 {config.IpAddress}:{config.Port}");
var connected = await client.ConnectAsync();
Console.WriteLine($"连接状态: {connected}");
```

## 测试工具

### 推荐的模拟器

- **Modbus**: Modbus Slave, ModSim
- **三菱MC**: GX Works2 模拟器
- **欧姆龙FINS**: CX-Programmer 模拟器

### 网络工具

- Wireshark - 数据包分析
- Packet Sender - 数据包发送
- PuTTY - 串口/网络连接测试

## 下一步

1. 阅读详细文档: `README.md`
2. 查看实际连接示例: `ActualConnectionExample.cs`
3. 修改配置参数适配您的设备
4. 运行实际连接测试

## 获取帮助

- 项目文档: [待补充]
- 示例代码: 查看 Examples 目录
- 问题反馈: 通过项目Issue提交

## 许可证

本项目遵循原项目许可证。
