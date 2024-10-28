using System;
using System.Collections.Generic;

namespace RfidStationControl
{
    public static class TeamsPageState
    {
        public static ushort ScanTeamNumber = 1;
        public static ushort GetTeamNumber = 1;
        public static DateTime Issued = new DateTime();
        public static ushort TeamMask = 0;
        public static ushort EraseTeam = 0;

        public static readonly List<TeamsTableItem> Table = new List<TeamsTableItem>();
    }
}