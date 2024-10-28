using System.Collections.Generic;

namespace RfidStationControl
{
    public static class FlashPageState
    {
        public static uint ReadAddress = 0;
        public static byte ReadLength = 0;
        public static uint WriteAddress = 0;
        public static byte[] WriteData = new byte[0];

        public static readonly List<FlashTableItem> Table = new List<FlashTableItem>();
    }
}