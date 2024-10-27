using System.Collections.Generic;

namespace RfidStationControl
{
    public static class RfidPageState
    {
        public static ushort InitChipNumber = 0;
        public static ushort Mask = 0;
        public static byte ReadFrom = 0;
        public static byte ReadTo = 0;
        public static byte WriteFrom = 0;
        public static byte[] WriteData = new byte[4];
        public static byte[] Uid = new byte[8];

        public static readonly List<RfidTableItem> Table = new List<RfidTableItem>();
    }
}