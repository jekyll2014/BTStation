using System;
using System.Text;

namespace RfidStationControl
{
    static class Helpers
    {
        public static byte CrcCalc(byte[] data, uint startPos, uint endPos)
        {
            if (data == null) return 0;
            byte crc = 0x00;
            while (startPos <= endPos)
            {
                byte tmp = data[startPos];
                for (byte tempI = 8; tempI > 0; tempI--)
                {
                    //byte sum = (byte)((crc & 0xFF) ^ (tmp & 0xFF));
                    //sum = (byte)((sum & 0xFF) & 0x01);
                    byte sum = (byte)((crc ^ tmp) & 0x01);
                    crc >>= 1;
                    if (sum != 0)
                    {
                        crc ^= 0x8C;
                    }
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
            StringBuilder hexStr = new StringBuilder();
            for (int i = 0; i < length; i++)
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
            StringBuilder hexStr = new StringBuilder();
            uint endByte = startByte + length - 1;
            for (; startByte < endByte; startByte++)
            {
                hexStr.Append(byteArr[startByte].ToString("X2"));
                hexStr.Append(" ");
            }
            return hexStr.ToString();
        }

        public static byte[] ConvertHexToByteArray(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if (hexString.Length % 2 == 1) hexString += "0";
            byte[] byteValue = new byte[hexString.Length / 2];
            int i = 0;
            while (hexString.Length > 1)
            {
                byteValue[i] = Convert.ToByte(Convert.ToUInt32(hexString.Substring(0, 2), 16));
                hexString = hexString.Substring(2, hexString.Length - 2);
                i++;
            }
            return byteValue;
        }

        public static string CheckHexString(string inStr)
        {
            StringBuilder outStr = new StringBuilder();
            if (inStr != "")
            {
                char[] str = inStr.ToCharArray(0, inStr.Length);
                StringBuilder tmpStr = new StringBuilder();
                for (int i = 0; i < inStr.Length; i++)
                {
                    if ((str[i] >= 'A' && str[i] <= 'F') || (str[i] >= 'a' && str[i] <= 'f') || (str[i] >= '0' && str[i] <= '9'))
                    {
                        tmpStr.Append(str[i].ToString());
                    }
                    else if (str[i] == ' ' && tmpStr.Length > 0)
                    {
                        for (int i1 = 0; i1 < 2 - tmpStr.Length; i1++) outStr.Append("0");
                        outStr.Append(tmpStr);
                        outStr.Append(" ");
                        tmpStr.Length = 0;
                    }
                    if (tmpStr.Length == 2)
                    {
                        outStr.Append(tmpStr);
                        outStr.Append(" ");
                        tmpStr.Length = 0;
                    }
                }
                if (tmpStr.Length > 0)
                {
                    for (int i = 0; i < 2 - tmpStr.Length; i++) outStr.Append("0");
                    outStr.Append(tmpStr);
                    outStr.Append(" ");
                }
                return outStr.ToString().ToUpperInvariant();
            }

            return ("");
        }

        public static string ConvertByteArrayToString(byte[] byteArr, int codePage = 866)
        {
            return Encoding.GetEncoding(codePage).GetString(byteArr);
        }

        public static bool PrintableByteArray(byte[] str)
        {
            if (str == null) return true;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] < 32 && str[i] != '\r' && str[i] != '\n' && str[i] != '\t') return false;
            }
            return true;
        }

        public static string DateToString(DateTime date)
        {
            string dateString = date.Year.ToString("D4")
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
                int.TryParse(dateString.Substring(0, 4), out int year);
                int.TryParse(dateString.Substring(5, 2), out int mon);
                if (mon > 12 || mon < 1)
                    mon = 1;

                int.TryParse(dateString.Substring(8, 2), out int day);
                if (day > 31 || day < 1)
                    day = 1;

                int.TryParse(dateString.Substring(11, 2), out int hh);
                if (hh > 23 || hh < 0)
                    hh = 0;

                int.TryParse(dateString.Substring(14, 2), out int mm);
                if (mm > 59 || mm < 0)
                    mm = 0;

                int.TryParse(dateString.Substring(17, 2), out int ss);
                if (ss > 59 || ss < 0)
                    ss = 0;

                DateTime date = new DateTime(year, mon, day, hh, mm, ss);
                unixTime = ConvertToUnixTimestamp(date);
            }

            return unixTime;
        }

        public static DateTime ConvertFromUnixTimestamp(long timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }

        public static long ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            return (long)diff.TotalSeconds;
        }

        public static float FloatConversion(byte[] bytes)
        {
            float myFloat = BitConverter.ToSingle(bytes, 0);
            return myFloat;
        }

        public static string ConvertMaskToString(byte[] mask)
        {
            string tmp = "";
            for (int i = 7; i >= 0; i--)
                tmp += Helpers.GetBit(mask[0], (byte)i) ? "1" : "0";
            for (int i = 7; i >= 0; i--)
                tmp += Helpers.GetBit(mask[1], (byte)i) ? "1" : "0";
            return tmp;
        }

        public static string ConvertMaskToString(UInt16 mask)
        {
            string tmpMask = "";
            for (int i = 15; i >= 0; i--)
            {
                tmpMask += Helpers.GetBit(mask, (byte)i) ? "1" : "0";
            }
            return tmpMask;
        }

        public static UInt16 ConvertStringToMask(string mask)
        {
            UInt16 n = 0;
            byte j = 15;
            for (byte i = 0; i < 16; i++)
            {
                if (mask[i] == '1')
                    n = (UInt16)Helpers.SetBit(n, j);
                j--;
            }
            return n;
        }
    }
}
