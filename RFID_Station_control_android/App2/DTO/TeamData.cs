using System;

namespace RfidStationControl
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