using System;

namespace RFID_Station_control
{
    public class TeamData
    {
        public ushort TeamNumber;
        public ushort TeamMask;
        public DateTime InitTime;
        public DateTime LastCheckTime;
        public uint DumpSize;
    }
}