using System;

namespace RFID_Station_control
{
    public class TeamDumpData
    {
        public int TeamNumber;
        public DateTime InitTime;
        public int TeamMask;
        public DateTime LastCheckTime;
        public int DumpSize;
        public RfidContainer ChipDump;
    }
}