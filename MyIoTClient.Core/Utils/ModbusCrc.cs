namespace MyIoTClient.Core.Utils;

public static class ModbusCrc
{
    private static readonly ushort[] CrcTable = new ushort[256];

    static ModbusCrc()
    {
        for (ushort i = 0; i < 256; i++)
        {
            ushort crc = i;
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x0001) != 0)
                {
                    crc >>= 1;
                    crc ^= 0xA001;
                }
                else
                {
                    crc >>= 1;
                }
            }
            CrcTable[i] = crc;
        }
    }

    public static ushort Calculate(ReadOnlySpan<byte> data)
    {
        ushort crc = 0xFFFF;
        foreach (byte b in data)
        {
            crc = (ushort)((crc >> 8) ^ CrcTable[(crc ^ b) & 0xFF]);
        }
        return crc;
    }
}
