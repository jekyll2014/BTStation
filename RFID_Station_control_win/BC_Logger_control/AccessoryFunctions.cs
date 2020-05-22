using Microsoft.Win32;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

public static class Accessory
{
    public static string ConvertHexToString(string hexString, int cp = 866)
    {
        hexString = hexString.Replace(" ", "");
        if (hexString.Length % 2 == 1) return "";
        var strValue = new byte[hexString.Length / 2];
        var i = 0;
        while (hexString.Length > 1)
        {
            strValue[i] = Convert.ToByte(Convert.ToUInt32(hexString.Substring(0, 2), 16));
            hexString = hexString.Substring(2, hexString.Length - 2);
            i++;
        }
        return Encoding.GetEncoding(cp).GetString(strValue, 0, i);
    }

    public static byte[] ConvertHexToByteArray(string hexString)
    {
        hexString = hexString?.Replace(" ", "");
        if (hexString.Length % 2 == 1) hexString += "0";
        var byteValue = new byte[hexString.Length / 2];
        var i = 0;
        while (hexString.Length > 1)
        {
            byteValue[i] = Convert.ToByte(Convert.ToUInt32(hexString.Substring(0, 2), 16));
            hexString = hexString.Substring(2, hexString.Length - 2);
            i++;
        }
        return byteValue;
    }

    public static byte ConvertHexToByte(string hexString)
    {
        hexString = hexString.Trim();
        byte byteValue = 0;
        if (hexString.Length > 0 && hexString.Length < 3) byteValue = Convert.ToByte(Convert.ToUInt32(hexString, 16));
        return byteValue;
    }

    public static string ConvertHexToDec(string hexString)
    {
        return Convert.ToInt32(hexString, 16).ToString();
    }

    public static int ConvertHexToInt(string hexString)
    {
        hexString = hexString.Replace(" ", "");
        hexString = hexString.Replace("x", "");
        hexString = hexString.Replace("h", "");
        hexString = hexString.Replace("X", "");
        hexString = hexString.Replace("H", "");
        return int.Parse(hexString, NumberStyles.HexNumber);
    }

    public static string ConvertStringToHex(string utfString, int cp = 866)
    {
        var encodedBytes = Encoding.GetEncoding(cp).GetBytes(utfString);
        var hexStr = new StringBuilder();
        foreach (var b in encodedBytes)
        {
            var c = (char)b;
            hexStr.Append(((int)c).ToString("X2"));
            hexStr.Append(" ");
        }
        return hexStr.ToString();
    }

    //???? check negative values
    public static string ConvertDecToString(string decString, int cp = 866)
    {
        decString = decString.Replace(" ", "");
        if (decString.Length % 3 == 1) return "";
        var strValue = new byte[decString.Length / 3];
        var i = 0;
        while (decString.Length > 1)
        {
            if (Convert.ToUInt32(decString.Substring(0, 3), 10) < 256) strValue[i] = Convert.ToByte(Convert.ToUInt32(decString.Substring(0, 3), 10));
            else strValue[i] = 0xff;
            decString = decString.Substring(3, decString.Length - 3);
            i++;
        }
        return Encoding.GetEncoding(cp).GetString(strValue, 0, i);
    }

    public static string ConvertDecToHex(string decString)
    {
        return Convert.ToInt32(decString, 16).ToString();
    }

    public static string ConvertStringToDec(string utfString, int cp = 866)
    {
        var encodedBytes = Encoding.GetEncoding(cp).GetBytes(utfString);
        var decStr = new StringBuilder();
        foreach (var b in encodedBytes)
        {
            decStr.Append(((int)b).ToString("D3"));
            decStr.Append(" ");
        }
        return decStr.ToString();
    }

    //???? check negative values
    public static string ConvertBinToString(string binString, int cp = 866)
    {
        binString = binString.Replace(" ", "");
        if (binString.Length % 8 == 1) return "";
        var strValue = new byte[binString.Length / 8];
        var i = 0;
        while (binString.Length > 0)
        {
            strValue[i] = Convert.ToByte(Convert.ToUInt32(binString.Substring(0, 8), 2));
            binString = binString.Substring(8, binString.Length - 8);
            i++;
        }
        return Encoding.GetEncoding(cp).GetString(strValue, 0, i);
    }

    public static string ConvertStringToBin(string utfString, int cp = 866)
    {
        var encodedBytes = Encoding.GetEncoding(cp).GetBytes(utfString);
        var binStr = new StringBuilder();
        foreach (var b in encodedBytes)
        {
            var tmpStr = Convert.ToString(b, 2);
            while (tmpStr.Length < 8) tmpStr = "0" + tmpStr;
            binStr.Append(tmpStr);
            binStr.Append(" ");
        }
        binStr.Append(" ");
        return binStr.ToString();
    }

    public static string CheckHexString(string inStr)
    {
        var outStr = new StringBuilder();
        if (inStr != "")
        {
            var str = inStr.ToCharArray(0, inStr.Length);
            var tmpStr = new StringBuilder();
            for (var i = 0; i < inStr.Length; i++)
            {
                if (str[i] >= 'A' && str[i] <= 'F' || str[i] >= 'a' && str[i] <= 'f' || str[i] >= '0' && str[i] <= '9')
                {
                    tmpStr.Append(str[i].ToString());
                }
                else if (str[i] == ' ' && tmpStr.Length > 0)
                {
                    for (var i1 = 0; i1 < 2 - tmpStr.Length; i1++) outStr.Append("0");
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
                for (var i = 0; i < 2 - tmpStr.Length; i++) outStr.Append("0");
                outStr.Append(tmpStr);
                outStr.Append(" ");
            }
            return outStr.ToString().ToUpperInvariant();
        }

        return "";
    }

    //добавить фильтрацию значений >255
    public static string CheckDecString(string inStr)
    {
        var outStr = new StringBuilder();
        if (inStr != "")
        {
            var str = inStr.ToCharArray(0, inStr.Length);
            var tmpStr = new StringBuilder();
            for (var i = 0; i < inStr.Length; i++)
            {
                if (str[i] >= '0' && str[i] <= '9')
                {
                    tmpStr.Append(str[i].ToString());
                }
                else if (str[i] == ' ' && tmpStr.Length > 0)
                {
                    for (var i1 = 0; i1 < inStr.Length - tmpStr.Length; i1++) outStr.Append("0");
                    outStr.Append(tmpStr);
                    outStr.Append(" ");
                    tmpStr.Length = 0;
                }
                if (tmpStr.Length == inStr.Length)
                {
                    outStr.Append(tmpStr);
                    outStr.Append(" ");
                    tmpStr.Length = 0;
                }
            }
            if (tmpStr.Length > 0)
            {
                for (var i = 0; i < inStr.Length - tmpStr.Length; i++) outStr.Append("0");
                outStr.Append(tmpStr);
                outStr.Append(" ");
            }
            return outStr.ToString();
        }

        return "";
    }

    //добавить фильтрацию значений >255
    public static string CheckDecString(string inStr, int length)
    {
        var outStr = new StringBuilder();
        if (inStr != "")
        {
            var str = inStr.ToCharArray(0, inStr.Length);
            var tmpStr = new StringBuilder();
            for (var i = 0; i < inStr.Length; i++)
            {
                if (str[i] >= '0' && str[i] <= '9')
                {
                    tmpStr.Append(str[i].ToString());
                }
                else if (str[i] == ' ' && tmpStr.Length > 0)
                {
                    for (var i1 = 0; i1 < length - tmpStr.Length; i1++) outStr.Append("0");
                    outStr.Append(tmpStr);
                    outStr.Append(" ");
                    tmpStr.Length = 0;
                }
                if (tmpStr.Length == length)
                {
                    outStr.Append(tmpStr);
                    outStr.Append(" ");
                    tmpStr.Length = 0;
                }
            }
            if (tmpStr.Length > 0)
            {
                for (var i = 0; i < length - tmpStr.Length; i++) outStr.Append("0");
                outStr.Append(tmpStr);
                outStr.Append(" ");
            }
            return outStr.ToString();
        }

        return "";
    }

    // проверить
    public static string CheckBinString(string inStr)
    {
        var outStr = new StringBuilder();
        if (inStr != "")
        {
            var str = inStr.ToCharArray(0, inStr.Length);
            var tmpStr = new StringBuilder();
            for (var i = 0; i < inStr.Length; i++)
            {
                if (str[i] >= '0' && str[i] <= '1')
                {
                    tmpStr.Append(str[i].ToString());
                }
                else if (str[i] == ' ' && tmpStr.Length > 0)
                {
                    for (var i1 = 0; i1 < 8 - tmpStr.Length; i1++) outStr.Append("0");
                    outStr.Append(tmpStr);
                    outStr.Append(" ");
                    tmpStr.Length = 0;
                }
                if (tmpStr.Length == 8)
                {
                    outStr.Append(tmpStr);
                    outStr.Append(" ");
                    tmpStr.Length = 0;
                }
            }
            if (tmpStr.Length > 0)
            {
                for (var i = 0; i < 8 - tmpStr.Length; i++) outStr.Append("0");
                outStr.Append(tmpStr);
                outStr.Append(" ");
            }
            return outStr.ToString();
        }

        return "";
    }

    //определение формата строки
    public static int GetStringFormat(string inString)
    {
        int i = 0, i1 = 0;
        //bool xbin = true, xdec = true;
        var l = new List<int>();
        while (i < inString.Length)
            if (inString[i] >= '0' && inString[i] <= '9' || inString[i] >= 'a' && inString[i] <= 'f' || inString[i] >= 'A' && inString[i] <= 'F' || inString[i] == ' ')
            {
                //if (inString[i] < '0' && inString[i] > '1' && inString[i] != ' ') xbin=false; //not BIN
                //if (inString[i] < '0' && inString[i] > '9' && inString[i] != ' ') xdec = false; //not DEC
                if (inString[i] == ' ')
                {
                    if (i1 != 0) l.Add(i1);
                    i1 = 0;
                }
                else
                {
                    i1++;
                }

                i++;
            }
            else
            {
                return 0; //TEXT
            }

        if (i1 != 0) l.Add(i1);
        //if (xbin == false && xdec == false) return 16;
        for (i = 0; i < l.Count - 1; i++)
            if (l[i + 1] != l[i]) return 0;
        if (l[0] == 2) return 16;
        if (l[0] == 3) return 10;
        if (l[0] == 8) return 2;
        return 0;
    }

    public static string ConvertByteArrayToHex(byte[] byteArr)
    {
        if (byteArr == null) return "";
        var hexStr = new StringBuilder();
        for (var i = 0; i < byteArr.Length; i++)
        {
            hexStr.Append(byteArr[i].ToString("X2"));
            hexStr.Append(" ");
        }
        return hexStr.ToString();
    }

    public static string ConvertByteArrayToHex(byte[] byteArr, int length)
    {
        if (byteArr == null) return "";
        if (length > byteArr.Length) length = byteArr.Length;
        var hexStr = new StringBuilder();
        for (var i = 0; i < length; i++)
        {
            hexStr.Append(byteArr[i].ToString("X2"));
            hexStr.Append(" ");
        }
        return hexStr.ToString();
    }

    public static string ConvertByteToHex(byte byteVal)
    {
        return byteVal.ToString("X2") + " ";
    }

    public static bool PrintableByteArray(byte[] str)
    {
        if (str == null) return true;
        for (var i = 0; i < str.Length; i++)
            if (str[i] < 32) return false;
        return true;
    }

    public static bool PrintableByte(byte b)
    {
        if (b < 32) return false;
        return true;
    }

    public static bool PrintableHex(string str)
    {
        for (var i = 0; i < str.Length; i += 3)
        {
            if (!byte.TryParse(str.Substring(i, 3), NumberStyles.HexNumber, null, out var n)) return false;
            if (n < 32) return false;
        }
        return true;
    }

    public static string ConvertByteArrayToString(byte[] byteArr, int codePage = 866)
    {
        return Encoding.GetEncoding(codePage).GetString(byteArr);
    }

    public static string FilterZeroChar(string m, bool replaceWithSpace = true)
    {
        var n = new StringBuilder();
        for (var i = 0; i < m.Length; i++)
            if (m[i] != 0) n.Append(m[i]);
            else if (replaceWithSpace) n.Append(" ");
        return n.ToString();
    }

    public static int CountSubString(string str, string subStr)
    {
        var count = 0;
        var n = 0;
        while ((n = str.IndexOf(subStr, n, StringComparison.InvariantCulture)) != -1)
        {
            n += subStr.Length;
            ++count;
        }
        return count;
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

    public static long ClearBit(long b, byte bitNumber)
    {
        b = (long)(b & ~(1 << bitNumber));
        return b;
    }

    public static long Evaluate(string expression)  //calculate string formula
    {
        var loDataTable = new DataTable();
        var loDataColumn = new DataColumn("Eval", typeof(long), expression);
        loDataTable.Columns.Add(loDataColumn);
        loDataTable.Rows.Add(0);
        return (long)loDataTable.Rows[0]["Eval"];
    }

    public static long EvaluateVariables(string expression, string[] variables = null, string[] values = null)  //calculate string formula
    {
        if (variables != null && values != null)
        {
            if (variables.Length != values.Length) return 0;
            for (var i = 0; i < variables.Length; i++) expression = expression.Replace(variables[i], values[i]);
        }
        var loDataTable = new DataTable();
        var loDataColumn = new DataColumn("Eval", typeof(long), expression);
        loDataTable.Columns.Add(loDataColumn);
        loDataTable.Rows.Add(0);
        return (long)loDataTable.Rows[0]["Eval"];
    }

    public static void Delay_ms(long milisec)
    {
        var start = DateTime.Now;

        while (DateTime.Now.Subtract(start).TotalMilliseconds < milisec)
        {
            Application.DoEvents();
            Thread.Sleep(1);
        }
    }

    public static bool ByteArrayCompare(byte[] a1, byte[] b1)
    {
        if (a1 == null && b1 == null) return true;
        if (a1 == null || b1 == null) return false;
        if (a1.Length != b1.Length) return false;

        for (var i = 0; i < a1.Length; i++)
            if (a1[i] != b1[i]) return false;
        return true;
    }

    public static byte[] CombineByteArrays(byte[] first, byte[] second)
    {
        var ret = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, ret, 0, first.Length);
        Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
        return ret;
    }

    public static byte CrcCalc(byte[] instr)
    {
        if (instr == null) return 0;
        byte crc = 0x00;
        var i = 0;
        while (i < instr.Length)
        {
            var tmp = instr[i];
            for (byte tempI = 8; tempI > 0; tempI--)
            {
                //byte sum = (byte)((crc & 0xFF) ^ (tmp & 0xFF));
                //sum = (byte)((sum & 0xFF) & 0x01);
                var sum = (byte)((crc ^ tmp) & 0x01);
                crc >>= 1;
                if (sum != 0) crc ^= 0x8C;
                tmp >>= 1;
            }
            i++;
        }
        return crc;
    }

    public static byte CrcCalc(byte[] instr, int startPos, int endPos)
    {
        if (instr == null) return 0;
        byte crc = 0x00;
        var i = startPos;
        while (i <= endPos)
        {
            var tmp = instr[i];
            for (byte tempI = 8; tempI > 0; tempI--)
            {
                //byte sum = (byte)((crc & 0xFF) ^ (tmp & 0xFF));
                //sum = (byte)((sum & 0xFF) & 0x01);
                var sum = (byte)((crc ^ tmp) & 0x01);
                crc >>= 1;
                if (sum != 0) crc ^= 0x8C;
                tmp >>= 1;
            }
            i++;
        }
        return crc;
    }

    public static string CorrectFloatPoint(string s)
    {
        if (NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator == ".")
            s = s.Replace(",", NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator);
        else
            s = s.Replace(".", NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator);
        return s;
    }

    /* Example:
    string[] ports = System.IO.Ports.SerialPort.GetPortNames();
    Hashtable PortNames = BuildPortNameHash(ports);
    foreach (String s in PortNames.Keys)
    {
        portDesc.Add(PortNames[s].ToString() + ": " + s);
    } */

    public static Hashtable BuildPortNameHash(string[] oPortsToMap)
    {
        var oReturnTable = new Hashtable();
        MineRegistryForPortName("SYSTEM\\CurrentControlSet\\Enum", oReturnTable, oPortsToMap);
        return oReturnTable;
    }

    private static void MineRegistryForPortName(string strStartKey, Hashtable oTargetMap, string[] oPortNamesToMatch)
    {
        if (oTargetMap.Count >= oPortNamesToMatch.Length)
            return;
        var oCurrentKey = Registry.LocalMachine;

        try
        {
            oCurrentKey = oCurrentKey.OpenSubKey(strStartKey);

            if (oCurrentKey != null)
            {
                var oSubKeyNames = oCurrentKey.GetSubKeyNames();
                if (((IList<string>)oSubKeyNames).Contains("Device Parameters") && strStartKey != "SYSTEM\\CurrentControlSet\\Enum")
                {
                    var oPortNameValue = Registry.GetValue("HKEY_LOCAL_MACHINE\\" + strStartKey + "\\Device Parameters", "PortName", null);

                    if (oPortNameValue == null || ((IList<string>)oPortNamesToMatch).Contains(oPortNameValue.ToString()) == false)
                        return;
                    var oFriendlyName = Registry.GetValue("HKEY_LOCAL_MACHINE\\" + strStartKey, "FriendlyName", null);

                    var strFriendlyName = "N/A";

                    if (oFriendlyName != null)
                        strFriendlyName = oFriendlyName.ToString();
                    if (strFriendlyName.Contains(oPortNameValue.ToString()) == false)
                        strFriendlyName = $"{strFriendlyName} ({oPortNameValue})";
                    oTargetMap[strFriendlyName] = oPortNameValue;
                }
                else
                {
                    foreach (var strSubKey in oSubKeyNames)
                        MineRegistryForPortName(strStartKey + "\\" + strSubKey, oTargetMap, oPortNamesToMatch);
                }
            }
        }
        catch
        {
        }
    }

    public static string AssemblyVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }

    public static string ProductVersion()
    {
        return Application.ProductVersion.ToString();
    }
}