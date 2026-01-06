namespace MyIoTClient.Core.Enums;

/// <summary>
/// 协议类型枚举
/// </summary>
public enum ProtocolType
{
    /// <summary>
    /// Modbus TCP协议
    /// </summary>
    ModbusTcp,

    /// <summary>
    /// Modbus RTU协议
    /// </summary>
    ModbusRtu,

    /// <summary>
    /// OPC UA协议
    /// </summary>
    OpcUa,

    /// <summary>
    /// BACnet协议
    /// </summary>
    BacNet,

    /// <summary>
    /// 西门子S7协议
    /// </summary>
    SiemensS7,

    /// <summary>
    /// 三菱MC协议
    /// </summary>
    MitsubishiMc,

    /// <summary>
    /// 欧姆龙FINS协议
    /// </summary>
    OmronFins,

    /// <summary>
    /// 自定义串口协议
    /// </summary>
    Custom
}
