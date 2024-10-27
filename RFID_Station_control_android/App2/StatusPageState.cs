using System;
using System.Text;

namespace RfidStationControl
{
    public static class StatusPageState
    {
        public static ushort CheckedChipsNumber = 0;
        public static byte NewStationNumber = 0;
        public static DateTime LastCheck = new DateTime();
        public static readonly StringBuilder TerminalText = new StringBuilder();
    }
}