using MyIoTClient.Core.Models;
using MyIoTClient.Protocols.Modbus;
using MyIoTClient.Protocols.MitsubishiMc;
using MyIoTClient.Protocols.OmronFins;
using MyIoTClient.Protocols.OpcUa;
using MyIoTClient.Protocols.Plc;

Console.WriteLine("=== MyIoTClient 示例程序 ===");
Console.WriteLine("这是一个物联网设备通讯协议实现客户端示例\n");

// 示例1: Modbus TCP客户端
Console.WriteLine("1. Modbus TCP 示例:");
await DemoModbusTcp();

Console.WriteLine("\n2. Modbus RTU 示例:");
await DemoModbusRtu();

Console.WriteLine("\n3. OPC UA 示例:");
await DemoOpcUa();

Console.WriteLine("\n4. 西门子S7 PLC 示例:");
await DemoSiemensS7();

Console.WriteLine("\n5. 三菱MC协议示例:");
await DemoMitsubishiMc();

Console.WriteLine("\n6. 欧姆龙FINS协议示例:");
await DemoOmronFins();

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

static async Task DemoModbusRtu()
{
    var config = new SerialConnectionConfig
    {
        PortName = "COM1",
        BaudRate = 9600,
        DataBits = 8,
        StopBits = "One",
        Parity = "None",
        ConnectionTimeout = 5000
    };

    using var client = new ModbusRtuClient(config);

    Console.WriteLine($"  正在连接到 Modbus RTU 设备端口 {config.PortName}...");
    Console.WriteLine("  Modbus RTU 客户端已创建");
    Console.WriteLine("  支持功能: 读取保持寄存器, 读取输入寄存器, 读写线圈");
    Console.WriteLine("  使用示例:");
    Console.WriteLine("    - await client.ReadAsync(\"100\", 10);  // 读取地址100开始的10个寄存器");
    Console.WriteLine("    - await client.ReadCoilsAsync(\"0\", 16);  // 读取16个线圈");
    Console.WriteLine("    - await client.WriteAsync(\"100\", 1234);  // 写入值1234到地址100");

    await Task.CompletedTask;
}

static async Task DemoMitsubishiMc()
{
    var config = new MitsubishiMcConnectionConfig
    {
        IpAddress = "192.168.1.100",
        Port = 5007,
        NetworkNumber = 0,
        PcNumber = 0xFF,
        ConnectionTimeout = 5000
    };

    using var client = new MitsubishiMcClient(config);

    Console.WriteLine($"  正在连接到三菱 PLC {config.IpAddress}:{config.Port}...");
    Console.WriteLine("  三菱 MC 客户端已创建");
    Console.WriteLine("  支持设备: D(数据寄存器), M(辅助继电器), X(输入), Y(输出)");
    Console.WriteLine("  支持ASCII和二进制格式");
    Console.WriteLine("  使用示例:");
    Console.WriteLine("    - await client.ReadAsync(\"D0\", 10);  // 读取D0开始的10个寄存器");
    Console.WriteLine("    - await client.WriteAsync(\"M0\", true);  // 写入M0继电器");
    Console.WriteLine("    - await client.BatchReadAsync(new Dictionary<string, int> { {\"D0\", 5}, {\"D10\", 3} });");

    await Task.CompletedTask;
}

static async Task DemoOmronFins()
{
    var config = new OmronFinsConnectionConfig
    {
        IpAddress = "192.168.1.50",
        Port = 9600,
        RemoteNodeNumber = 1,
        LocalNetworkNumber = 0,
        RemoteNetworkNumber = 0,
        ConnectionTimeout = 5000
    };

    using var client = new OmronFinsClient(config);

    Console.WriteLine($"  正在连接到欧姆龙 PLC {config.IpAddress}:{config.Port}...");
    Console.WriteLine("  欧姆龙 FINS 客户端已创建");
    Console.WriteLine("  支持区域: CIO, WR, HR, DM, TIM, CNT等");
    Console.WriteLine("  支持TCP和UDP通信");
    Console.WriteLine("  使用示例:");
    Console.WriteLine("    - await client.ReadAsync(\"CIO0\", 10);  // 读取CIO0开始的10个字");
    Console.WriteLine("    - await client.WriteAsync(\"DM0\", 1234);  // 写入DM0");
    Console.WriteLine("    - await client.ReadCpuStatusAsync();  // 读取CPU状态");

    await Task.CompletedTask;
}
