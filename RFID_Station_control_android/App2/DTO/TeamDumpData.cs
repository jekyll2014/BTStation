using System;

namespace RfidStationControl
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