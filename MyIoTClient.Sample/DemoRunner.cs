namespace MyIoTClient.Sample.Examples;

/// <summary>
/// 示例运行器
/// 用于运行实际连接示例
/// </summary>
public static class DemoRunner
{
    /// <summary>
    /// 运行指定协议的实际连接示例
    /// </summary>
    public static async Task RunDemo(string protocol)
    {
        try
        {
            switch (protocol.ToLower())
            {
                case "modbustcp":
                case "modbus-tcp":
                    await ActualConnectionExample.ModbusTcpConnectionExample();
                    break;

                case "modbusrtu":
                case "modbus-rtu":
                    await ActualConnectionExample.ModbusRtuConnectionExample();
                    break;

                case "mitsubishi":
                case "mitsubishi-mc":
                case "mc":
                    await ActualConnectionExample.MitsubishiMcConnectionExample();
                    break;

                case "omron":
                case "omron-fins":
                case "fins":
                    await ActualConnectionExample.OmronFinsConnectionExample();
                    break;

                default:
                    Console.WriteLine($"未知的协议: {protocol}");
                    Console.WriteLine("支持的协议: modbustcp, modbusrtu, mitsubishi, omron");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"运行示例时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 显示所有可用的示例
    /// </summary>
    public static void ShowAvailableDemos()
    {
        Console.WriteLine("可用的实际连接示例:");
        Console.WriteLine("  modbustcp   - Modbus TCP 连接示例");
        Console.WriteLine("  modbusrtu   - Modbus RTU 连接示例");
        Console.WriteLine("  mitsubishi  - 三菱MC协议连接示例");
        Console.WriteLine("  omron       - 欧姆龙FINS协议连接示例");
        Console.WriteLine();
        Console.WriteLine("使用方法:");
        Console.WriteLine("  DemoRunner.RunDemo(\"modbustcp\")");
        Console.WriteLine();
    }
}
