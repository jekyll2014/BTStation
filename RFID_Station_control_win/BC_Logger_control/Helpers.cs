using System;
using System.Text;

namespace RfidStationControl
{
    internal static class Helpers
    {
        public static byte CrcCalc(byte[] data, uint startPos, uint endPos)
        {
            if (data == null) return 0;
            byte crc = 0x00;
            while (startPos <= endPos)
            {
                var tmp = data[startPos];
                for (byte tempI = 8; tempI > 0; tempI--)
                {
                    //byte sum = (byte)((crc & 0xFF) ^ (tmp & 0xFF));
                    //sum = (byte)((sum & 0xFF) & 0x01);
                    var sum = (byte)((crc ^ tmp) & 0x01);
                    crc >>= 1;
                    if (sum != 0) crc ^= 0x8C;
                    tmp >>= 1;
                }
                startPos++;
            }
            return crc;
        }

        public static bool GetBit(long b, byte bitNumber)
        {
            return (b & (1 << bitNumber)) != 0;
        }

        public static long SetBit(long b, byte bitNumber)
        {
            long v1 = 1 << bitNumber;
            return b | v1;
        }

        public static string ConvertByteArrayToHex(byte[] byteArr, uint length = 0)
        {
            if (byteArr == null)
                return "";
            if (length == 0)
                length = (uint)byteArr.Length;
            if (length > byteArr.Length)
                length = (uint)byteArr.Length;
            var hexStr = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                hexStr.Append(byteArr[i].ToString("X2"));
                hexStr.Append(" ");
            }
            return hexStr.ToString();
        }

        public static string ConvertByteArrayToHex(byte[] byteArr, uint startByte, uint length)
        {
            if (byteArr == null) return "";
            if (length == 0 || length > byteArr.Length) length = (uint)byteArr.Length;
            var hexStr = new StringBuilder();
            var endByte = startByte + length - 1;
            for (; startByte < endByte; startByte++)
            {
                hexStr.Append(byteArr[startByte].ToString("X2"));
                hexStr.Append(" ");
            }
            return hexStr.ToString();
        }

        public static string ConvertByteArrayToString(byte[] byteArr, int codePage = 866)
        {
            return Encoding.GetEncoding(codePage).GetString(byteArr);
        }

        public static bool PrintableByteArray(byte[] str)
        {
            if (str == null) return true;
            foreach (var t in str)
                if (t < 32 && t != '\r' && t != '\n' && t != '\t') return false;
            return true;
        }

        public static string DateToString(DateTime date)
        {
            var dateString = date.Year.ToString("D4")
                                + "." + date.Month.ToString("D2")
                                + "." + date.Day.ToString("D2")
                                + " " + date.Hour.ToString("D2")
                                + ":" + date.Minute.ToString("D2")
                                + ":" + date.Second.ToString("D2");
            return dateString;
        }

        public static byte[] DateStringToByteArray(string dateString)
        {
            //"2019.04.01 12:00:00";
            byte[] dateArray = { 0, 1, 1, 0, 0, 0 };
            if (dateString.Length == 19
                && dateString[4] == '.'
                && dateString[7] == '.'
                && dateString[10] == ' '
                && dateString[13] == ':'
                && dateString[16] == ':')
            {
                byte.TryParse(dateString.Substring(2, 2), out dateArray[0]);
                if (dateArray[0] > 99)
                    dateArray[0] = 0;
                else if (dateArray[0] < 1)
                    dateArray[0] = 1;
                byte.TryParse(dateString.Substring(5, 2), out dateArray[1]);
                if (dateArray[1] > 12)
                    dateArray[1] = 1;
                else if (dateArray[1] < 1)
                    dateArray[1] = 1;
                byte.TryParse(dateString.Substring(8, 2), out dateArray[2]);
                if (dateArray[2] > 31)
                    dateArray[2] = 1;
                else if (dateArray[2] < 1)
                    dateArray[2] = 1;
                byte.TryParse(dateString.Substring(11, 2), out dateArray[3]);
                if (dateArray[3] > 23)
                    dateArray[3] = 0;
                byte.TryParse(dateString.Substring(14, 2), out dateArray[4]);
                if (dateArray[4] > 59)
                    dateArray[4] = 0;
                byte.TryParse(dateString.Substring(17, 2), out dateArray[5]);
                if (dateArray[5] > 59)
                    dateArray[5] = 0;
            }

            return dateArray;
        }

        public static long DateStringToUnixTime(string dateString)
        {
            //"2019.04.01 12:00:00";
            long unixTime = 0;
            if (dateString.Length == 19
                && dateString[4] == '.'
                && dateString[7] == '.'
                && dateString[10] == ' '
                && dateString[13] == ':'
                && dateString[16] == ':')
            {
                int.TryParse(dateString.Substring(0, 4), out var year);
                int.TryParse(dateString.Substring(5, 2), out var mon);
                if (mon > 12 || mon < 1)
                    mon = 1;

                int.TryParse(dateString.Substring(8, 2), out var day);
                if (day > 31 || day < 1)
                    day = 1;

                int.TryParse(dateString.Substring(11, 2), out var hh);
                if (hh > 23 || hh < 0)
                    hh = 0;

                int.TryParse(dateString.Substring(14, 2), out var mm);
                if (mm > 59 || mm < 0)
                    mm = 0;

                int.TryParse(dateString.Substring(17, 2), out var ss);
                if (ss > 59 || ss < 0)
                    ss = 0;

                var date = new DateTime(year, mon, day, hh, mm, ss);
                unixTime = ConvertToUnixTimestamp(date);
            }

            return unixTime;
        }

        public static DateTime ConvertFromUnixTimestamp(long timestamp)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }

        public static long ConvertToUnixTimestamp(DateTime date)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var diff = date - origin;
            return (long)diff.TotalSeconds;
        }

        public static float FloatConversion(byte[] bytes)
        {
            var myFloat = BitConverter.ToSingle(bytes, 0);
            return myFloat;
        }

        public static string ConvertMaskToString(byte[] mask)
        {
            var tmp = "";
            for (var i = 7; i >= 0; i--)
                tmp += GetBit(mask[0], (byte)i) ? "1" : "0";
            for (var i = 7; i >= 0; i--)
                tmp += GetBit(mask[1], (byte)i) ? "1" : "0";
            return tmp;
        }

        public static string ConvertMaskToString(ushort mask)
        {
            var tmpMask = "";
            for (var i = 15; i >= 0; i--) tmpMask += GetBit(mask, (byte)i) ? "1" : "0";
            return tmpMask;
        }

        public static ushort ConvertStringToMask(string mask)
        {
            ushort n = 0;
            byte j = 15;
            for (byte i = 0; i < 16; i++)
            {
                if (mask[i] == '1')
                    n = (ushort)SetBit(n, j);
                j--;
            }
            return n;
        }
    }
}
