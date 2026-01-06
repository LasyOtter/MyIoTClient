using System.Collections.Generic;
using System.Diagnostics;
using MyIoTClient.Core.Models;
using MyIoTClient.Protocols.Modbus;
using MyIoTClient.Protocols.MitsubishiMc;
using MyIoTClient.Protocols.OmronFins;
using MyIoTClient.Sample.Examples;
using MyIoTClient.Core.Factory;
using MyIoTClient.Core.Enums;

// ============================================================================
// MyIoTClient 示例程序 - 参照 IoTClient.Examples 改进版
// ============================================================================
// 本程序演示如何使用 MyIoTClient 库与各种工业通讯协议设备进行通信
// 参照开源项目: https://github.com/zhaopeiym/IoTClient.Examples
//
// 支持的协议:
// 1. Modbus TCP - 通用 Modbus TCP 协议
// 2. Modbus RTU - 串口 Modbus 协议
// 3. 三菱MC协议 - 三菱 PLC 通讯
// 4. 欧姆龙FINS协议 - 欧姆龙 PLC 通讯
//
// 本版本改进:
// - 更友好的交互界面
// - 配置文件管理
// - 批量读写测试
// - 日志显示功能
// - 连接状态监控
// ============================================================================

var appTitle = @"
╔═══════════════════════════════════════════════════════════════════════╗
║                                                                       ║
║              MyIoTClient 物联网协议客户端示例程序 v2.0               ║
║                                                                       ║
║                参照 IoTClient.Examples 项目优化                       ║
║                                                                       ║
╚═══════════════════════════════════════════════════════════════════════╝
";

var configManager = new ConfigManager();
var logger = new SimpleLogger();

while (true)
{
    Console.Clear();
    Console.WriteLine(appTitle);
    Console.WriteLine("\n【主菜单】\n");
    Console.WriteLine("  [1] Modbus TCP 协议 - 以太网通讯");
    Console.WriteLine("  [2] Modbus RTU 协议 - 串口通讯");
    Console.WriteLine("  [3] 三菱MC协议      - 三菱PLC通讯");
    Console.WriteLine("  [4] 欧姆龙FINS协议  - 欧姆龙PLC通讯");
    Console.WriteLine("  [5] 批量读写测试    - 多地址测试");
    Console.WriteLine("  [6] 配置管理        - 查看和编辑配置");
    Console.WriteLine("  [7] 运行实际连接示例 (需要真实设备)");
    Console.WriteLine("  [0] 退出程序");
    Console.Write("\n\n请选择功能 (0-7): ");

    var input = Console.ReadLine()?.Trim();
    Console.WriteLine();

    switch (input)
    {
        case "1":
            await DemoModbusTcp(configManager, logger);
            break;
        case "2":
            await DemoModbusRtu(configManager, logger);
            break;
        case "3":
            await DemoMitsubishiMc(configManager, logger);
            break;
        case "4":
            await DemoOmronFins(configManager, logger);
            break;
        case "5":
            await BatchReadWriteTest(configManager, logger);
            break;
        case "6":
            ShowConfigMenu(configManager);
            break;
        case "7":
            await RunActualConnectionDemo();
            break;
        case "0":
            Console.WriteLine("\n感谢使用 MyIoTClient!");
            Console.WriteLine("项目源码: https://github.com/zhaopeiym/IoTClient.Examples");
            return;
        default:
            Console.WriteLine("⚠️  无效的选项，请重新输入\n");
            break;
    }

    Console.WriteLine("\n按任意键返回主菜单...");
    Console.ReadKey();
}

// ============================================================================
// 配置管理器
// ============================================================================
class ConfigManager
{
    private readonly Dictionary<string, object> _configs = new();
    
    public ConfigManager()
    {
        LoadDefaultConfigs();
    }
    
    private void LoadDefaultConfigs()
    {
        _configs["ModbusTcp"] = new TcpConnectionConfig
        {
            IpAddress = "192.168.1.100",
            Port = 502,
            ConnectionTimeout = 5000,
            ReadTimeout = 3000,
            WriteTimeout = 3000
        };
        
        _configs["ModbusRtu"] = new SerialConnectionConfig
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
        
        _configs["MitsubishiMc"] = new MitsubishiMcConnectionConfig
        {
            IpAddress = "192.168.1.100",
            Port = 5007,
            NetworkNumber = 0,
            PcNumber = 0xFF,
            ConnectionTimeout = 5000,
            ReadTimeout = 3000,
            WriteTimeout = 3000
        };
        
        _configs["OmronFins"] = new OmronFinsConnectionConfig
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
    }
    
    public T GetConfig<T>(string name) where T : class
    {
        return _configs.TryGetValue(name, out var config) ? config as T : null;
    }
    
    public void SetConfig(string name, object config)
    {
        _configs[name] = config;
    }
    
    public void ShowAllConfigs()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("当前配置");
        Console.WriteLine("═══════════════════════════════════════════════════════════\n");
        
        foreach (var pair in _configs)
        {
            Console.WriteLine($"【{pair.Key}】");
            Console.WriteLine(pair.Value);
            Console.WriteLine();
        }
    }
}

// ============================================================================
// 简单日志器
// ============================================================================
class SimpleLogger
{
    public void Info(string message) => Log("INFO", message, ConsoleColor.Green);
    public void Warning(string message) => Log("WARN", message, ConsoleColor.Yellow);
    public void Error(string message) => Log("ERROR", message, ConsoleColor.Red);
    public void Debug(string message) => Log("DEBUG", message, ConsoleColor.Cyan);
    
    private void Log(string level, string message, ConsoleColor color)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write($"[{timestamp}] ");
        Console.ForegroundColor = color;
        Console.WriteLine($"[{level}] {message}");
        Console.ResetColor();
    }
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
// 配置菜单
// ============================================================================
static void ShowConfigMenu(ConfigManager configManager)
{
    while (true)
    {
        Console.Clear();
        Console.WriteLine("\n╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    配置管理                              ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");
        
        Console.WriteLine("  [1] 查看所有配置");
        Console.WriteLine("  [2] 编辑 Modbus TCP 配置");
        Console.WriteLine("  [3] 编辑 Modbus RTU 配置");
        Console.WriteLine("  [4] 编辑 三菱MC协议 配置");
        Console.WriteLine("  [5] 编辑 欧姆龙FINS协议 配置");
        Console.WriteLine("  [0] 返回主菜单");
        Console.Write("\n\n请选择操作: ");

        var input = Console.ReadLine()?.Trim();
        Console.WriteLine();

        switch (input)
        {
            case "1":
                configManager.ShowAllConfigs();
                break;
            case "2":
                EditTcpConfig(configManager);
                break;
            case "3":
                EditSerialConfig(configManager, "ModbusRtu");
                break;
            case "4":
                EditMitsubishiConfig(configManager);
                break;
            case "5":
                EditOmronFinsConfig(configManager);
                break;
            case "0":
                return;
            default:
                Console.WriteLine("⚠️  无效的选项\n");
                break;
        }

        Console.WriteLine("\n按任意键继续...");
        Console.ReadKey();
    }
}

static void EditTcpConfig(ConfigManager configManager)
{
    Console.WriteLine("═══════════════════════════════════════════════════════════");
    Console.WriteLine("编辑 Modbus TCP 配置");
    Console.WriteLine("═══════════════════════════════════════════════════════════\n");
    
    var config = configManager.GetConfig<TcpConnectionConfig>("ModbusTcp") ?? new TcpConnectionConfig();
    
    Console.Write($"IP地址 ({config.IpAddress}): ");
    var ip = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(ip)) config.IpAddress = ip;
    
    Console.Write($"端口 ({config.Port}): ");
    var port = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(port) && int.TryParse(port, out var portNum)) config.Port = portNum;
    
    Console.Write($"连接超时 ({config.ConnectionTimeout}ms): ");
    var timeout = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(timeout) && int.TryParse(timeout, out var timeoutNum)) config.ConnectionTimeout = timeoutNum;
    
    configManager.SetConfig("ModbusTcp", config);
    Console.WriteLine("\n✓ 配置已更新");
}

static void EditSerialConfig(ConfigManager configManager, string configName)
{
    Console.WriteLine($"═══════════════════════════════════════════════════════════");
    Console.WriteLine($"编辑 {configName} 配置");
    Console.WriteLine("═══════════════════════════════════════════════════════════\n");
    
    var config = configManager.GetConfig<SerialConnectionConfig>(configName) ?? new SerialConnectionConfig();
    
    Console.Write($"串口号 ({config.PortName}): ");
    var port = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(port)) config.PortName = port;
    
    Console.Write($"波特率 ({config.BaudRate}): ");
    var baud = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(baud) && int.TryParse(baud, out var baudNum)) config.BaudRate = baudNum;
    
    Console.Write($"数据位 ({config.DataBits}): ");
    var dataBits = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(dataBits) && int.TryParse(dataBits, out var bits)) config.DataBits = bits;
    
    configManager.SetConfig(configName, config);
    Console.WriteLine("\n✓ 配置已更新");
}

static void EditMitsubishiConfig(ConfigManager configManager)
{
    Console.WriteLine("═══════════════════════════════════════════════════════════");
    Console.WriteLine("编辑 三菱MC协议 配置");
    Console.WriteLine("═══════════════════════════════════════════════════════════\n");
    
    var config = configManager.GetConfig<MitsubishiMcConnectionConfig>("MitsubishiMc") ?? new MitsubishiMcConnectionConfig();
    
    Console.Write($"IP地址 ({config.IpAddress}): ");
    var ip = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(ip)) config.IpAddress = ip;
    
    Console.Write($"端口 ({config.Port}): ");
    var port = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(port) && int.TryParse(port, out var portNum)) config.Port = portNum;
    
    configManager.SetConfig("MitsubishiMc", config);
    Console.WriteLine("\n✓ 配置已更新");
}

static void EditOmronFinsConfig(ConfigManager configManager)
{
    Console.WriteLine("═══════════════════════════════════════════════════════════");
    Console.WriteLine("编辑 欧姆龙FINS协议 配置");
    Console.WriteLine("═══════════════════════════════════════════════════════════\n");
    
    var config = configManager.GetConfig<OmronFinsConnectionConfig>("OmronFins") ?? new OmronFinsConnectionConfig();
    
    Console.Write($"IP地址 ({config.IpAddress}): ");
    var ip = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(ip)) config.IpAddress = ip;
    
    Console.Write($"端口 ({config.Port}): ");
    var port = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(port) && int.TryParse(port, out var portNum)) config.Port = portNum;
    
    configManager.SetConfig("OmronFins", config);
    Console.WriteLine("\n✓ 配置已更新");
}

// ============================================================================
// 批量读写测试
// ============================================================================
static async Task BatchReadWriteTest(ConfigManager configManager, SimpleLogger logger)
{
    Console.WriteLine("═══════════════════════════════════════════════════════════");
    Console.WriteLine("批量读写测试");
    Console.WriteLine("═══════════════════════════════════════════════════════════\n");
    
    Console.WriteLine("选择协议进行批量测试:");
    Console.WriteLine("  [1] Modbus TCP");
    Console.WriteLine("  [2] Modbus RTU");
    Console.WriteLine("  [3] 三菱MC协议");
    Console.WriteLine("  [4] 欧姆龙FINS协议");
    Console.Write("\n选择协议: ");
    
    var choice = Console.ReadLine()?.Trim();
    Console.WriteLine();
    
    switch (choice)
    {
        case "1":
            await BatchTestModbusTcp(configManager, logger);
            break;
        case "2":
            await BatchTestModbusRtu(configManager, logger);
            break;
        case "3":
            await BatchTestMitsubishi(configManager, logger);
            break;
        case "4":
            await BatchTestOmronFins(configManager, logger);
            break;
        default:
            Console.WriteLine("⚠️  无效的选择");
            break;
    }
}

static async Task BatchTestModbusTcp(ConfigManager configManager, SimpleLogger logger)
{
    var config = configManager.GetConfig<TcpConnectionConfig>("ModbusTcp");
    if (config == null) return;
    
    logger.Info($"开始批量测试 Modbus TCP - {config.IpAddress}:{config.Port}");
    
    using var client = new ModbusTcpClient(config);
    var watch = Stopwatch.StartNew();
    
    // 测试连接
    try
    {
        await client.ConnectAsync();
        logger.Info("连接成功");
        
        // 批量读取测试
        logger.Info("开始批量读取测试...");
        for (int i = 0; i < 5; i++)
        {
            var result = await client.ReadAsync($"{100 + i * 10}", 5);
            if (result.IsSuccess)
            {
                logger.Info($"读取地址 {100 + i * 10}: 成功");
            }
            else
            {
                logger.Error($"读取地址 {100 + i * 10}: 失败 - {result.ErrorMessage}");
            }
        }
        
        watch.Stop();
        logger.Info($"批量测试完成, 耗时: {watch.ElapsedMilliseconds}ms");
    }
    catch (Exception ex)
    {
        logger.Error($"连接失败: {ex.Message}");
    }
}

static async Task BatchTestModbusRtu(ConfigManager configManager, SimpleLogger logger)
{
    var config = configManager.GetConfig<SerialConnectionConfig>("ModbusRtu");
    if (config == null) return;
    
    logger.Info($"开始批量测试 Modbus RTU - {config.PortName}");
    
    using var client = new ModbusRtuClient(config);
    var watch = Stopwatch.StartNew();
    
    try
    {
        await client.ConnectAsync();
        logger.Info("连接成功");
        
        // 测试多种操作
        logger.Info("开始功能测试...");
        
        var readResult = await client.ReadAsync("100", 5);
        logger.Info(readResult.IsSuccess ? "读取保持寄存器: 成功" : $"读取保持寄存器: 失败");
        
        var inputResult = await client.ReadInputRegistersAsync("0", 5);
        logger.Info(inputResult.IsSuccess ? "读取输入寄存器: 成功" : $"读取输入寄存器: 失败");
        
        var coilResult = await client.ReadCoilsAsync("0", 8);
        logger.Info(coilResult.IsSuccess ? "读取线圈状态: 成功" : $"读取线圈状态: 失败");
        
        watch.Stop();
        logger.Info($"批量测试完成, 耗时: {watch.ElapsedMilliseconds}ms");
    }
    catch (Exception ex)
    {
        logger.Error($"连接失败: {ex.Message}");
    }
}

static async Task BatchTestMitsubishi(ConfigManager configManager, SimpleLogger logger)
{
    var config = configManager.GetConfig<MitsubishiMcConnectionConfig>("MitsubishiMc");
    if (config == null) return;
    
    logger.Info($"开始批量测试 三菱MC协议 - {config.IpAddress}:{config.Port}");
    
    using var client = new MitsubishiMcClient(config);
    var watch = Stopwatch.StartNew();
    
    try
    {
        await client.ConnectAsync();
        logger.Info("连接成功");
        
        // 批量读写测试
        logger.Info("开始批量读写测试...");
        
        var testData = new short[] { 111, 222, 333, 444, 555 };
        var writeResult = await client.WriteAsync("D0", testData);
        logger.Info(writeResult.IsSuccess ? "批量写入: 成功" : $"批量写入: 失败");
        
        if (writeResult.IsSuccess)
        {
            var readResult = await client.ReadAsync("D0", 5);
            if (readResult.IsSuccess)
            {
                logger.Info("批量读取: 成功");
            }
        }
        
        // 测试位操作
        var boolResult = await client.ReadAsync("M0", 8);
        logger.Info(boolResult.IsSuccess ? "位读取: 成功" : $"位读取: 失败");
        
        watch.Stop();
        logger.Info($"批量测试完成, 耗时: {watch.ElapsedMilliseconds}ms");
    }
    catch (Exception ex)
    {
        logger.Error($"连接失败: {ex.Message}");
    }
}

static async Task BatchTestOmronFins(ConfigManager configManager, SimpleLogger logger)
{
    var config = configManager.GetConfig<OmronFinsConnectionConfig>("OmronFins");
    if (config == null) return;
    
    logger.Info($"开始批量测试 欧姆龙FINS协议 - {config.IpAddress}:{config.Port}");
    
    using var client = new OmronFinsClient(config);
    var watch = Stopwatch.StartNew();
    
    try
    {
        await client.ConnectAsync();
        logger.Info("连接成功");
        
        // 多区域测试
        logger.Info("开始多区域批量测试...");
        
        var cioResult = await client.ReadAsync("CIO0", 10);
        logger.Info(cioResult.IsSuccess ? "CIO区域读取: 成功" : "CIO区域读取: 失败");
        
        var dmResult = await client.ReadAsync("DM0", 10);
        logger.Info(dmResult.IsSuccess ? "DM区域读取: 成功" : "DM区域读取: 失败");
        
        var wrResult = await client.ReadAsync("WR0", 5);
        logger.Info(wrResult.IsSuccess ? "WR区域读取: 成功" : "WR区域读取: 失败");
        
        // CPU状态测试
        var cpuResult = await client.ReadCpuStatusAsync();
        if (cpuResult.IsSuccess)
        {
            logger.Info($"CPU状态读取: 成功 - 模式: {cpuResult.Value.RunMode}");
        }
        else
        {
            logger.Error("CPU状态读取: 失败");
        }
        
        watch.Stop();
        logger.Info($"批量测试完成, 耗时: {watch.ElapsedMilliseconds}ms");
    }
    catch (Exception ex)
    {
        logger.Error($"连接失败: {ex.Message}");
    }
}

// ============================================================================
// 改进的演示函数
// ============================================================================
static async Task DemoModbusTcp(ConfigManager configManager, SimpleLogger logger)
{
    Console.WriteLine("═══════════════════════════════════════════════════════════");
    Console.WriteLine("Modbus TCP 协议演示");
    Console.WriteLine("═══════════════════════════════════════════════════════════\n");

    var config = configManager.GetConfig<TcpConnectionConfig>("ModbusTcp");
    if (config == null) return;

    logger.Info($"加载配置: {config.IpAddress}:{config.Port}");
    
    // 创建客户端
    using var client = new ModbusTcpClient(config);
    logger.Info("✓ Modbus TCP 客户端创建成功");

    // 显示支持的地址格式
    Console.WriteLine("\n【支持的地址格式】");
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
