using System;

namespace RfidStationControl
{
    public static class ConfigPageState
    {
        public static bool UseCurrentTime = true;
        public static DateTime SetTime = DateTime.Now;
        public static uint FlashLimitSize = 32 * 1024;
    }
}