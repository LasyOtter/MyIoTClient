using MyIoTClient.Core.Models;
using MyIoTClient.Protocols.Modbus;
using MyIoTClient.Protocols.MitsubishiMc;
using MyIoTClient.Protocols.OmronFins;

namespace MyIoTClient.Sample.Examples;

/// <summary>
/// 实际连接示例
/// 演示如何实际连接到设备并执行读写操作
/// </summary>
public class ActualConnectionExample
{
    /// <summary>
    /// Modbus TCP 实际连接示例
    /// </summary>
    public static async Task ModbusTcpConnectionExample()
    {
        Console.WriteLine("=== Modbus TCP 实际连接示例 ===\n");

        var config = new TcpConnectionConfig
        {
            IpAddress = "192.168.1.100", // 修改为实际设备IP
            Port = 502,
            ConnectionTimeout = 5000,
            ReadTimeout = 3000,
            WriteTimeout = 3000
        };

        using var client = new ModbusTcpClient(config);

        try
        {
            Console.WriteLine($"正在连接到 {config.IpAddress}:{config.Port}...");
            var connected = await client.ConnectAsync();

            if (!connected)
            {
                Console.WriteLine("连接失败！请检查设备IP地址和网络连接。");
                return;
            }

            Console.WriteLine("✓ 连接成功！\n");

            // 读取保持寄存器
            Console.WriteLine("读取保持寄存器...");
            var readResult = await client.ReadAsync("0", 10);
            if (readResult.IsSuccess)
            {
                var data = (byte[])readResult.Value;
                Console.WriteLine($"读取成功，数据: {BitConverter.ToString(data)}\n");
            }
            else
            {
                Console.WriteLine($"读取失败: {readResult.ErrorMessage}\n");
            }

            // 写入测试值
            Console.WriteLine("写入测试值...");
            var writeResult = await client.WriteAsync("0", 1234);
            if (writeResult.IsSuccess)
            {
                Console.WriteLine("写入成功！\n");

                // 读取验证
                var verifyResult = await client.ReadAsync("0", 1);
                if (verifyResult.IsSuccess)
                {
                    var verifyData = (byte[])verifyResult.Value;
                    var value = BitConverter.ToUInt16(verifyData, 0);
                    Console.WriteLine($"验证读取: 值 = {value}");
                }
            }
            else
            {
                Console.WriteLine($"写入失败: {writeResult.ErrorMessage}\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发生异常: {ex.Message}\n");
        }
        finally
        {
            await client.DisconnectAsync();
            Console.WriteLine("已断开连接");
        }
    }

    /// <summary>
    /// Modbus RTU 实际连接示例
    /// </summary>
    public static async Task ModbusRtuConnectionExample()
    {
        Console.WriteLine("=== Modbus RTU 实际连接示例 ===\n");

        var config = new SerialConnectionConfig
        {
            PortName = "COM1", // 修改为实际串口
            BaudRate = 9600,
            DataBits = 8,
            StopBits = "One",
            Parity = "None",
            ConnectionTimeout = 5000,
            ReadTimeout = 3000,
            WriteTimeout = 3000
        };

        using var client = new ModbusRtuClient(config);

        try
        {
            Console.WriteLine($"正在连接到串口 {config.PortName}...");
            Console.WriteLine($"波特率: {config.BaudRate}, 数据位: {config.DataBits}");

            var connected = await client.ConnectAsync();

            if (!connected)
            {
                Console.WriteLine("连接失败！请检查串口是否被占用或设备是否连接。");
                return;
            }

            Console.WriteLine("✓ 连接成功！\n");

            // 读取保持寄存器
            Console.WriteLine("读取保持寄存器...");
            var readResult = await client.ReadAsync("0", 10);
            if (readResult.IsSuccess)
            {
                var data = (short[])readResult.Value;
                Console.WriteLine($"读取成功，数据: {string.Join(", ", data)}\n");
            }
            else
            {
                Console.WriteLine($"读取失败: {readResult.ErrorMessage}\n");
            }

            // 读取输入寄存器
            Console.WriteLine("读取输入寄存器...");
            var inputResult = await client.ReadInputRegistersAsync("0", 5);
            if (inputResult.IsSuccess)
            {
                var data = (short[])inputResult.Value;
                Console.WriteLine($"读取成功，输入寄存器: {string.Join(", ", data)}\n");
            }
            else
            {
                Console.WriteLine($"读取失败: {inputResult.ErrorMessage}\n");
            }

            // 读取线圈
            Console.WriteLine("读取线圈状态...");
            var coilResult = await client.ReadCoilsAsync("0", 8);
            if (coilResult.IsSuccess)
            {
                var data = (bool[])coilResult.Value;
                Console.WriteLine($"读取成功，线圈状态: {string.Join(", ", data)}\n");
            }
            else
            {
                Console.WriteLine($"读取失败: {coilResult.ErrorMessage}\n");
            }

            // 写入线圈
            Console.WriteLine("写入线圈状态...");
            var writeCoilResult = await client.WriteCoilAsync("0", true);
            if (writeCoilResult.IsSuccess)
            {
                Console.WriteLine("写入成功！\n");
            }
            else
            {
                Console.WriteLine($"写入失败: {writeCoilResult.ErrorMessage}\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发生异常: {ex.Message}");
            Console.WriteLine("提示: 请确保以管理员权限运行程序以访问串口。\n");
        }
        finally
        {
            await client.DisconnectAsync();
            Console.WriteLine("已断开连接");
        }
    }

    /// <summary>
    /// 三菱MC协议实际连接示例
    /// </summary>
    public static async Task MitsubishiMcConnectionExample()
    {
        Console.WriteLine("=== 三菱MC协议实际连接示例 ===\n");

        var config = new MitsubishiMcConnectionConfig
        {
            IpAddress = "192.168.1.100", // 修改为实际设备IP
            Port = 5007,
            NetworkNumber = 0,
            PcNumber = 0xFF,
            ConnectionTimeout = 5000,
            ReadTimeout = 3000,
            WriteTimeout = 3000
        };

        using var client = new MitsubishiMcClient(config);

        try
        {
            Console.WriteLine($"正在连接到 {config.IpAddress}:{config.Port}...");
            var connected = await client.ConnectAsync();

            if (!connected)
            {
                Console.WriteLine("连接失败！请检查设备IP地址和网络连接。");
                return;
            }

            Console.WriteLine("✓ 连接成功！\n");

            // 读取数据寄存器
            Console.WriteLine("读取数据寄存器 D0-D9...");
            var readResult = await client.ReadAsync("D0", 10);
            if (readResult.IsSuccess)
            {
                var data = (short[])readResult.Value;
                Console.WriteLine($"读取成功，数据: {string.Join(", ", data)}\n");
            }
            else
            {
                Console.WriteLine($"读取失败: {readResult.ErrorMessage}\n");
            }

            // 写入数据寄存器
            Console.WriteLine("写入数据到 D0-D2...");
            var values = new short[] { 100, 200, 300 };
            var writeResult = await client.WriteAsync("D0", values);
            if (writeResult.IsSuccess)
            {
                Console.WriteLine("写入成功！\n");

                // 读取验证
                var verifyResult = await client.ReadAsync("D0", 3);
                if (verifyResult.IsSuccess)
                {
                    var verifyData = (short[])verifyResult.Value;
                    Console.WriteLine($"验证读取: {string.Join(", ", verifyData)}");
                }
            }
            else
            {
                Console.WriteLine($"写入失败: {writeResult.ErrorMessage}\n");
            }

            // 读取辅助继电器
            Console.WriteLine("读取辅助继电器 M0-M9...");
            var mResult = await client.ReadAsync("M0", 10);
            if (mResult.IsSuccess)
            {
                var data = (bool[])mResult.Value;
                Console.WriteLine($"读取成功，状态: {string.Join(", ", data)}\n");
            }
            else
            {
                Console.WriteLine($"读取失败: {mResult.ErrorMessage}\n");
            }

            // 批量读取
            Console.WriteLine("批量读取多个地址...");
            var addresses = new Dictionary<string, int>
            {
                { "D0", 5 },
                { "D10", 3 }
            };
            var batchResult = await client.BatchReadAsync(addresses);
            if (batchResult.IsSuccess)
            {
                Console.WriteLine("批量读取成功:");
                foreach (var item in batchResult.Results)
                {
                    if (item.Value is short[] shortArray)
                        Console.WriteLine($"  {item.Address}: {string.Join(", ", shortArray)}");
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine($"批量读取失败: {batchResult.ErrorMessage}\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发生异常: {ex.Message}\n");
        }
        finally
        {
            await client.DisconnectAsync();
            Console.WriteLine("已断开连接");
        }
    }

    /// <summary>
    /// 欧姆龙FINS协议实际连接示例
    /// </summary>
    public static async Task OmronFinsConnectionExample()
    {
        Console.WriteLine("=== 欧姆龙FINS协议实际连接示例 ===\n");

        var config = new OmronFinsConnectionConfig
        {
            IpAddress = "192.168.1.50", // 修改为实际设备IP
            Port = 9600,
            RemoteNodeNumber = 1,
            LocalNodeNumber = 0,
            LocalNetworkNumber = 0,
            RemoteNetworkNumber = 0,
            ConnectionTimeout = 5000,
            ReadTimeout = 3000,
            WriteTimeout = 3000
        };

        using var client = new OmronFinsClient(config);

        try
        {
            Console.WriteLine($"正在连接到 {config.IpAddress}:{config.Port}...");
            var connected = await client.ConnectAsync();

            if (!connected)
            {
                Console.WriteLine("连接失败！请检查设备IP地址和网络连接。");
                return;
            }

            Console.WriteLine("✓ 连接成功！\n");

            // 读取CIO区域
            Console.WriteLine("读取 CIO0-CIO9...");
            var cioResult = await client.ReadAsync("CIO0", 10);
            if (cioResult.IsSuccess)
            {
                var data = (short[])cioResult.Value;
                Console.WriteLine($"读取成功，数据: {string.Join(", ", data)}\n");
            }
            else
            {
                Console.WriteLine($"读取失败: {cioResult.ErrorMessage}\n");
            }

            // 读取DM区域
            Console.WriteLine("读取 DM0-DM9...");
            var dmResult = await client.ReadAsync("DM0", 10);
            if (dmResult.IsSuccess)
            {
                var data = (short[])dmResult.Value;
                Console.WriteLine($"读取成功，数据: {string.Join(", ", data)}\n");
            }
            else
            {
                Console.WriteLine($"读取失败: {dmResult.ErrorMessage}\n");
            }

            // 写入DM区域
            Console.WriteLine("写入数据到 DM0-DM2...");
            var values = new short[] { 100, 200, 300 };
            var writeResult = await client.WriteAsync("DM0", values);
            if (writeResult.IsSuccess)
            {
                Console.WriteLine("写入成功！\n");

                // 读取验证
                var verifyResult = await client.ReadAsync("DM0", 3);
                if (verifyResult.IsSuccess)
                {
                    var verifyData = (short[])verifyResult.Value;
                    Console.WriteLine($"验证读取: {string.Join(", ", verifyData)}");
                }
            }
            else
            {
                Console.WriteLine($"写入失败: {writeResult.ErrorMessage}\n");
            }

            // 读取CPU状态
            Console.WriteLine("读取CPU状态...");
            var cpuResult = await client.ReadCpuStatusAsync();
            if (cpuResult.IsSuccess)
            {
                Console.WriteLine($"CPU运行模式: {cpuResult.Value.RunMode}");
                Console.WriteLine($"CPU状态: {cpuResult.Value.Status}\n");
            }
            else
            {
                Console.WriteLine($"读取CPU状态失败: {cpuResult.ErrorMessage}\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发生异常: {ex.Message}\n");
        }
        finally
        {
            await client.DisconnectAsync();
            Console.WriteLine("已断开连接");
        }
    }
}
