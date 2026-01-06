using MyIoTClient.Core.Models;
using MyIoTClient.Protocols.Modbus;
using MyIoTClient.Protocols.OpcUa;
using MyIoTClient.Protocols.Plc;

Console.WriteLine("=== MyIoTClient 示例程序 ===");
Console.WriteLine("这是一个物联网设备通讯协议实现客户端示例\n");

// 示例1: Modbus TCP客户端
Console.WriteLine("1. Modbus TCP 示例:");
await DemoModbusTcp();

Console.WriteLine("\n2. OPC UA 示例:");
await DemoOpcUa();

Console.WriteLine("\n3. 西门子S7 PLC 示例:");
await DemoSiemensS7();

Console.WriteLine("\n程序执行完成，按任意键退出...");

static async Task DemoModbusTcp()
{
    var config = new TcpConnectionConfig
    {
        IpAddress = "192.168.1.100",
        Port = 502,
        ConnectionTimeout = 5000,
        ReadTimeout = 3000
    };

    using var client = new ModbusTcpClient(config);

    Console.WriteLine($"  正在连接到 Modbus TCP 设备 {config.IpAddress}:{config.Port}...");

    // 实际场景中这里会尝试连接，但因为没有真实设备，这里只是演示
    // var connected = await client.ConnectAsync();

    Console.WriteLine("  Modbus TCP 客户端已创建");
    Console.WriteLine("  支持功能: 读取保持寄存器(0x03), 写入单个寄存器(0x06)");
    Console.WriteLine("  使用示例:");
    Console.WriteLine("    - await client.ReadAsync(\"100\", 10);  // 读取地址100开始的10个寄存器");
    Console.WriteLine("    - await client.WriteAsync(\"100\", 1234);  // 写入值1234到地址100");
}

static async Task DemoOpcUa()
{
    var config = new OpcUaConnectionConfig
    {
        EndpointUrl = "opc.tcp://localhost:4840",
        Username = "admin",
        Password = "password",
        SecurityPolicy = "None"
    };

    using var client = new OpcUaClient(config);

    Console.WriteLine($"  OPC UA 服务器地址: {config.EndpointUrl}");
    Console.WriteLine("  OPC UA 客户端已创建");
    Console.WriteLine("  支持功能: 读取节点, 写入节点, 订阅数据变化");
    Console.WriteLine("  使用示例:");
    Console.WriteLine("    - await client.ReadAsync(\"ns=2;s=Device1.Temperature\");");
    Console.WriteLine("    - await client.WriteAsync(\"ns=2;s=Device1.SetPoint\", 25.5);");

    await Task.CompletedTask;
}

static async Task DemoSiemensS7()
{
    var config = new TcpConnectionConfig
    {
        IpAddress = "192.168.1.200",
        Port = 102,
        ConnectionTimeout = 5000
    };

    using var client = new SiemensS7Client(config);

    Console.WriteLine($"  正在连接到西门子 S7 PLC {config.IpAddress}:{config.Port}...");
    Console.WriteLine("  西门子 S7 客户端已创建");
    Console.WriteLine("  支持地址类型: DB块(DB), 位存储器(M), 输入(I), 输出(Q)");
    Console.WriteLine("  使用示例:");
    Console.WriteLine("    - await client.ReadAsync(\"DB1.DBW0\");  // 读取DB1的Word 0");
    Console.WriteLine("    - await client.WriteAsync(\"M0.0\", true);  // 写入位存储器M0.0");

    await Task.CompletedTask;
}
