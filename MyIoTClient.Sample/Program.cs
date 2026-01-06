using System.Collections.Generic;
using MyIoTClient.Core.Models;
using MyIoTClient.Protocols.Modbus;
using MyIoTClient.Protocols.MitsubishiMc;
using MyIoTClient.Protocols.OmronFins;
using MyIoTClient.Sample.Examples;

// ============================================================================
// MyIoTClient 示例程序
// ============================================================================
// 本程序演示如何使用 MyIoTClient 库与各种工业通讯协议设备进行通信
//
// 支持的协议:
// 1. Modbus TCP - 通用 Modbus TCP 协议
// 2. Modbus RTU - 串口 Modbus 协议
// 3. 三菱MC协议 - 三菱 PLC 通讯
// 4. 欧姆龙FINS协议 - 欧姆龙 PLC 通讯
//
// 注意: 本示例使用模拟地址，实际使用时请根据设备情况修改配置
// ============================================================================

Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
Console.WriteLine("║          MyIoTClient 物联网协议客户端示例程序            ║");
Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
Console.WriteLine();

// 显示主菜单
while (true)
{
    Console.WriteLine("请选择要演示的协议:");
    Console.WriteLine("  1. Modbus TCP");
    Console.WriteLine("  2. Modbus RTU");
    Console.WriteLine("  3. 三菱MC协议");
    Console.WriteLine("  4. 欧姆龙FINS协议");
    Console.WriteLine("  5. 运行实际连接示例 (需要真实设备)");
    Console.WriteLine("  0. 退出程序");
    Console.Write("\n请输入选项 (0-5): ");

    var input = Console.ReadLine();

    Console.WriteLine();

    switch (input)
    {
        case "1":
            await DemoModbusTcp();
            break;
        case "2":
            await DemoModbusRtu();
            break;
        case "3":
            await DemoMitsubishiMc();
            break;
        case "4":
            await DemoOmronFins();
            break;
        case "5":
            await RunActualConnectionDemo();
            break;
        case "0":
            Console.WriteLine("感谢使用 MyIoTClient!");
            return;
        default:
            Console.WriteLine("无效的选项，请重新输入\n");
            break;
    }

    Console.WriteLine("\n按任意键继续...");
    Console.ReadKey();
    Console.Clear();
}

// ============================================================================
// Modbus TCP 示例
// ============================================================================
static async Task DemoModbusTcp()
{
    Console.WriteLine("═══════════════════════════════════════════════════════════");
    Console.WriteLine("Modbus TCP 协议演示");
    Console.WriteLine("═══════════════════════════════════════════════════════════\n");

    // 创建配置
    var config = new TcpConnectionConfig
    {
        IpAddress = "192.168.1.100",
        Port = 502,
        ConnectionTimeout = 5000,
        ReadTimeout = 3000,
        WriteTimeout = 3000
    };

    Console.WriteLine($"设备地址: {config.IpAddress}:{config.Port}");
    Console.WriteLine($"连接超时: {config.ConnectionTimeout}ms");
    Console.WriteLine($"读取超时: {config.ReadTimeout}ms\n");

    // 创建客户端
    using var client = new ModbusTcpClient(config);
    Console.WriteLine("✓ Modbus TCP 客户端创建成功\n");

    // 显示支持的地址格式
    Console.WriteLine("【支持的地址格式】");
    Console.WriteLine("  - 直接使用十进制地址，例如: \"100\", \"200\"");
    Console.WriteLine("  - 地址从0开始，对应Modbus寄存器地址\n");

    // 显示可用的操作
    Console.WriteLine("【可用操作】");
    Console.WriteLine("  1. 读取保持寄存器 (功能码 0x03)");
    Console.WriteLine("  2. 写入单个寄存器 (功能码 0x06)");
    Console.WriteLine("  3. 写入多个寄存器 (功能码 0x10)\n");

    // 显示代码示例
    Console.WriteLine("【代码示例】");
    Console.WriteLine("  // 读取保持寄存器");
    Console.WriteLine("  var result = await client.ReadAsync(\"100\", 10);");
    Console.WriteLine("  if (result.IsSuccess)");
    Console.WriteLine("  {");
    Console.WriteLine("      Console.WriteLine($\"读取到的值: {BitConverter.ToString((byte[])result.Value)}\");");
    Console.WriteLine("  }\n");

    Console.WriteLine("  // 写入单个寄存器");
    Console.WriteLine("  var result = await client.WriteAsync(\"100\", 1234);");
    Console.WriteLine("  if (result.IsSuccess)");
    Console.WriteLine("  {");
    Console.WriteLine("      Console.WriteLine(\"写入成功\");");
    Console.WriteLine("  }\n");

    Console.WriteLine("  // 注意: 批量写入需单独实现或多次调用单个写入\n");

    await Task.CompletedTask;
}

// ============================================================================
// Modbus RTU 示例
// ============================================================================
static async Task DemoModbusRtu()
{
    Console.WriteLine("═══════════════════════════════════════════════════════════");
    Console.WriteLine("Modbus RTU 协议演示");
    Console.WriteLine("═══════════════════════════════════════════════════════════\n");

    // 创建配置
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

    Console.WriteLine($"串口配置: {config.PortName}");
    Console.WriteLine($"波特率: {config.BaudRate}");
    Console.WriteLine($"数据位: {config.DataBits}");
    Console.WriteLine($"停止位: {config.StopBits}");
    Console.WriteLine($"校验位: {config.Parity}\n");

    // 创建客户端
    using var client = new ModbusRtuClient(config);
    Console.WriteLine("✓ Modbus RTU 客户端创建成功\n");

    // 显示支持的地址格式
    Console.WriteLine("【支持的地址格式】");
    Console.WriteLine("  - 保持寄存器: 直接使用十进制地址，例如: \"100\"");
    Console.WriteLine("  - 线圈地址: 直接使用十进制地址，例如: \"0\"\n");

    // 显示可用的操作
    Console.WriteLine("【可用操作】");
    Console.WriteLine("  1. 读取保持寄存器 (功能码 0x03)");
    Console.WriteLine("  2. 读取输入寄存器 (功能码 0x04)");
    Console.WriteLine("  3. 读取线圈状态 (功能码 0x01)");
    Console.WriteLine("  4. 写入单个寄存器 (功能码 0x06)");
    Console.WriteLine("  5. 写入多个寄存器 (功能码 0x10)");
    Console.WriteLine("  6. 写入单个线圈 (功能码 0x05)");
    Console.WriteLine("  7. 写入多个线圈 (功能码 0x0F)\n");

    // 显示代码示例
    Console.WriteLine("【代码示例】");
    Console.WriteLine("  // 读取保持寄存器");
    Console.WriteLine("  var result = await client.ReadAsync(\"100\", 10);");
    Console.WriteLine("  if (result.IsSuccess)");
    Console.WriteLine("  {");
    Console.WriteLine("      var data = (short[])result.Value;");
    Console.WriteLine("      Console.WriteLine($\"读取到的值: {string.Join(\", \", data)}\");");
    Console.WriteLine("  }\n");

    Console.WriteLine("  // 读取输入寄存器");
    Console.WriteLine("  var result = await client.ReadInputRegistersAsync(\"0\", 5);");
    Console.WriteLine("  if (result.IsSuccess)");
    Console.WriteLine("  {");
    Console.WriteLine("      var data = (short[])result.Value;");
    Console.WriteLine("      Console.WriteLine($\"读取到的值: {string.Join(\", \", data)}\");");
    Console.WriteLine("  }\n");

    Console.WriteLine("  // 读取线圈状态");
    Console.WriteLine("  var result = await client.ReadCoilsAsync(\"0\", 16);");
    Console.WriteLine("  if (result.IsSuccess)");
    Console.WriteLine("  {");
    Console.WriteLine("      var data = (bool[])result.Value;");
    Console.WriteLine("      Console.WriteLine($\"读取到的状态: {string.Join(\", \", data)}\");");
    Console.WriteLine("  }\n");

    Console.WriteLine("  // 写入单个线圈");
    Console.WriteLine("  var result = await client.WriteCoilAsync(\"0\", true);");
    Console.WriteLine("  if (result.IsSuccess)");
    Console.WriteLine("  {");
    Console.WriteLine("      Console.WriteLine(\"写入成功\");");
    Console.WriteLine("  }\n");

    Console.WriteLine("  // 写入多个线圈");
    Console.WriteLine("  var coils = new bool[] { true, false, true, false };");
    Console.WriteLine("  var result = await client.WriteCoilsAsync(\"0\", coils);");
    Console.WriteLine("  if (result.IsSuccess)");
    Console.WriteLine("  {");
    Console.WriteLine("      Console.WriteLine($\"成功写入 {coils.Length} 个线圈\");");
    Console.WriteLine("  }\n");

    await Task.CompletedTask;
}

// ============================================================================
// 三菱MC协议示例
// ============================================================================
static async Task DemoMitsubishiMc()
{
    Console.WriteLine("═══════════════════════════════════════════════════════════");
    Console.WriteLine("三菱MC协议演示");
    Console.WriteLine("═══════════════════════════════════════════════════════════\n");

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

    Console.WriteLine($"设备地址: {config.IpAddress}:{config.Port}");
    Console.WriteLine($"网络号: {config.NetworkNumber}");
    Console.WriteLine($"PC号: {config.PcNumber:X2}");
    Console.WriteLine($"连接超时: {config.ConnectionTimeout}ms\n");

    // 创建客户端
    using var client = new MitsubishiMcClient(config);
    Console.WriteLine("✓ 三菱MC客户端创建成功\n");

    // 显示支持的设备类型
    Console.WriteLine("【支持的设备类型】");
    Console.WriteLine("  - D     数据寄存器 (16位)      例如: D0, D100");
    Console.WriteLine("  - M     辅助继电器 (位)        例如: M0, M100");
    Console.WriteLine("  - X     输入继电器 (位)        例如: X0, X10");
    Console.WriteLine("  - Y     输出继电器 (位)        例如: Y0, Y10");
    Console.WriteLine("  - L     锁存继电器 (位)        例如: L0, L100");
    Console.WriteLine("  - F     报警器 (位)            例如: F0, F100");
    Console.WriteLine("  - V     边界继电器 (16位)     例如: V0, V100");
    Console.WriteLine("  - B     链接继电器 (位)        例如: B0, B100");
    Console.WriteLine("  - W     链接寄存器 (16位)      例如: W0, W100");
    Console.WriteLine("  - R     文件寄存器 (16位)      例如: R0, R100");
    Console.WriteLine("  - Z     变址寄存器 (16位)      例如: Z0, Z100\n");

    // 显示可用的操作
    Console.WriteLine("【可用操作】");
    Console.WriteLine("  1. 批量读取");
    Console.WriteLine("  2. 批量写入");
    Console.WriteLine("  3. 位读取");
    Console.WriteLine("  4. 位写入");
    Console.WriteLine("  5. 支持ASCII和二进制帧格式\n");

    // 显示代码示例
    Console.WriteLine("【代码示例】");
    Console.WriteLine("  // 读取数据寄存器");
    Console.WriteLine("  var result = await client.ReadAsync(\"D0\", 10);");
    Console.WriteLine("  if (result.IsSuccess)");
    Console.WriteLine("  {");
    Console.WriteLine("      var data = (short[])result.Value;");
    Console.WriteLine("      Console.WriteLine($\"读取到的值: {string.Join(\", \", data)}\");");
    Console.WriteLine("  }\n");

    Console.WriteLine("  // 写入数据寄存器");
    Console.WriteLine("  var values = new short[] { 100, 200, 300 };");
    Console.WriteLine("  var result = await client.WriteAsync(\"D0\", values);");
    Console.WriteLine("  if (result.IsSuccess)");
    Console.WriteLine("  {");
    Console.WriteLine("      Console.WriteLine(\"写入成功\");");
    Console.WriteLine("  }\n");

    Console.WriteLine("  // 读取辅助继电器");
    Console.WriteLine("  var result = await client.ReadAsync(\"M0\", 10);");
    Console.WriteLine("  if (result.IsSuccess)");
    Console.WriteLine("  {");
    Console.WriteLine("      var data = (bool[])result.Value;");
    Console.WriteLine("      Console.WriteLine($\"读取到的值: {string.Join(\", \", data)}\");");
    Console.WriteLine("  }\n");

    Console.WriteLine("  // 批量读取");
    Console.WriteLine("  var addresses = new Dictionary<string, int>");
    Console.WriteLine("  {");
    Console.WriteLine("      { \"D0\", 5 },");
    Console.WriteLine("      { \"D100\", 3 },");
    Console.WriteLine("      { \"M0\", 10 }");
    Console.WriteLine("  };");
    Console.WriteLine("  var result = await client.BatchReadAsync(addresses);");
    Console.WriteLine("  if (result.IsSuccess)");
    Console.WriteLine("  {");
    Console.WriteLine("      foreach (var item in result.Results)");
    Console.WriteLine("      {");
    Console.WriteLine("          Console.WriteLine($\"{item.Address}: {item.Value}\");");
    Console.WriteLine("      }");
    Console.WriteLine("  }\n");

    await Task.CompletedTask;
}

// ============================================================================
// 欧姆龙FINS协议示例
// ============================================================================
static async Task DemoOmronFins()
{
    Console.WriteLine("═══════════════════════════════════════════════════════════");
    Console.WriteLine("欧姆龙FINS协议演示");
    Console.WriteLine("═══════════════════════════════════════════════════════════\n");

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

    Console.WriteLine($"设备地址: {config.IpAddress}:{config.Port}");
    Console.WriteLine($"本地节点号: {config.LocalNodeNumber}");
    Console.WriteLine($"远程节点号: {config.RemoteNodeNumber}");
    Console.WriteLine($"连接超时: {config.ConnectionTimeout}ms\n");

    // 创建客户端
    using var client = new OmronFinsClient(config);
    Console.WriteLine("✓ 欧姆龙FINS客户端创建成功\n");

    // 显示支持的内存区域
    Console.WriteLine("【支持的内存区域】");
    Console.WriteLine("  - CIO   输入输出区        例如: CIO0, CIO100");
    Console.WriteLine("  - WR    工作区            例如: WR0, WR100");
    Console.WriteLine("  - HR    保持区            例如: HR0, HR100");
    Console.WriteLine("  - AR    辅助区            例如: AR0, AR100");
    Console.WriteLine("  - DM    数据区            例如: DM0, DM100");
    Console.WriteLine("  - DR    扩展数据区        例如: DR0, DR100");
    Console.WriteLine("  - TIM   定时器完成标志    例如: TIM0, TIM100");
    Console.WriteLine("  - CNT   计数器完成标志    例如: CNT0, CNT100");
    Console.WriteLine("  - IR    中断区            例如: IR0, IR100\n");

    // 显示可用的操作
    Console.WriteLine("【可用操作】");
    Console.WriteLine("  1. 批量读取");
    Console.WriteLine("  2. 批量写入");
    Console.WriteLine("  3. CPU状态读取");
    Console.WriteLine("  4. 支持TCP和UDP通信");
    Console.WriteLine("  5. 命令ID自动管理");
    Console.WriteLine("  6. 内置重试机制\n");

    // 显示代码示例
    Console.WriteLine("【代码示例】");
    Console.WriteLine("  // 读取CIO区域");
    Console.WriteLine("  var result = await client.ReadAsync(\"CIO0\", 10);");
    Console.WriteLine("  if (result.IsSuccess)");
    Console.WriteLine("  {");
    Console.WriteLine("      var data = (short[])result.Value;");
    Console.WriteLine("      Console.WriteLine($\"读取到的值: {string.Join(\", \", data)}\");");
    Console.WriteLine("  }\n");

    Console.WriteLine("  // 读取DM区域");
    Console.WriteLine("  var result = await client.ReadAsync(\"DM0\", 10);");
    Console.WriteLine("  if (result.IsSuccess)");
    Console.WriteLine("  {");
    Console.WriteLine("      var data = (short[])result.Value;");
    Console.WriteLine("      Console.WriteLine($\"读取到的值: {string.Join(\", \", data)}\");");
    Console.WriteLine("  }\n");

    Console.WriteLine("  // 写入DM区域");
    Console.WriteLine("  var values = new short[] { 100, 200, 300 };");
    Console.WriteLine("  var result = await client.WriteAsync(\"DM0\", values);");
    Console.WriteLine("  if (result.IsSuccess)");
    Console.WriteLine("  {");
    Console.WriteLine("      Console.WriteLine(\"写入成功\");");
    Console.WriteLine("  }\n");

    Console.WriteLine("  // 读取CPU状态");
    Console.WriteLine("  var result = await client.ReadCpuStatusAsync();");
    Console.WriteLine("  if (result.IsSuccess)");
    Console.WriteLine("  {");
    Console.WriteLine("      Console.WriteLine($\"CPU运行模式: {result.Value.RunMode}\");");
    Console.WriteLine("      Console.WriteLine($\"CPU状态: {result.Value.Status}\");");
    Console.WriteLine("  }\n");

    await Task.CompletedTask;
}

// ============================================================================
// 运行实际连接示例
// ============================================================================
static async Task RunActualConnectionDemo()
{
    Console.WriteLine("═══════════════════════════════════════════════════════════");
    Console.WriteLine("实际连接示例");
    Console.WriteLine("═══════════════════════════════════════════════════════════\n");

    DemoRunner.ShowAvailableDemos();

    Console.WriteLine("注意: 运行此示例需要真实的硬件设备或模拟器！");
    Console.WriteLine("请确保已修改连接参数为您的实际设备配置。\n");

    Console.WriteLine("请输入要运行的协议 (modbustcp/modbusrtu/mitsubishi/omron)，或按返回回到主菜单:");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine("返回主菜单");
        return;
    }

    Console.WriteLine();
    await DemoRunner.RunDemo(input);
}
