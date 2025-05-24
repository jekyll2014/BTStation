using System.Collections.Generic;

namespace RFID_Station_control
{
    public static class StationSettings
    {
        public static byte FwVersion = 0;
        public static byte Number = 0;
        public static byte Mode = 0;
        public static ushort MaxPacketLength = 255;
        public static float VoltageCoefficient = 0.00578F;
        public static float BatteryLimit = 3.0F;
        public static byte AntennaGain = 80;
        public static byte ChipType = RfidContainer.ChipTypes.Types["NTAG215"];
        public static uint FlashSize = 1 * 1024 * 1024;
        public static ushort TeamBlockSize = 1024;
        public static ushort EraseBlockSize = 4096;
        public static bool AutoReport = false;
        public static string BtName = "Sportduino-xx";
        public static string BtPin = "1111";
        public static string BtCommand = "AT";

        public static byte[] AuthPwd = new byte[4];
        public static byte[] AuthPack = new byte[2];
        public static bool AuthEnabled = false;

        //режимы станции
        public static readonly Dictionary<string, byte> StationMode = new Dictionary<string, byte>
        {
            {"Init", 0},
            {"Start", 1},
            {"Finish", 2}
        };

        //усиление антенны
        public static readonly Dictionary<string, byte> Gain = new Dictionary<string, byte>
        {
            {"Level 0", 0},
            {"Level 16", 16},
            {"Level 32", 32},
            {"Level 48", 48},
            {"Level 64", 64},
            {"Level 80", 80},
            {"Level 96", 96},
            {"Level 112", 112}
        };
    }
}