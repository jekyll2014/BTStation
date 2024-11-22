using System;
using System.Collections.Generic;
using System.Data;

namespace RFID_Station_control
{
    public class RfidContainer
    {
        public class ChipTypes
        {
            public const byte PageSize = 4;

            public static Dictionary<string, byte> Types = new Dictionary<string, byte>
            {
                {"NTAG213", 0},
                {"NTAG215", 1},
                {"NTAG216", 2}
            };
            public static readonly Dictionary<byte, string> Names = new Dictionary<byte, string>
            {
                { 0, "NTAG213" },
                { 1, "NTAG215" },
                { 2, "NTAG216" }
            };
            public static readonly Dictionary<byte, byte> PageSizes = new Dictionary<byte, byte>
            {
                { 0, 45 },
                { 1, 135 },
                { 2, 231 }
            };
            public static readonly Dictionary<byte, ushort> ByteSizes = new Dictionary<byte, ushort>
            {
                { 0, 144 },
                { 1, 496 },
                { 2, 872 }
            };
            public static readonly Dictionary<byte, byte> SystemIds = new Dictionary<byte, byte>
            {
                { 0, 0x12 },
                { 1, 0x3e },
                { 2, 0x6d }
            };
        }

        public enum RfidPageType
        {
            PAGE_UID1 = 0, // UID 0..3
            PAGE_UID2 = 1, //UID 4..8
            PAGE_CHIP_SYS1 = 2, //serial number, internal, lock bytes, lock bytes
            PAGE_CHIP_SYS2 = 3, // system[2] + тип_чипа[1] + system[1]
            PAGE_CHIP_NUM = 4, // номер_чипа[2] + тип_чипа[1] + версия_прошивки[1]
            PAGE_INIT_TIME = 5, // время инициализации[4]
            PAGE_TEAM_MASK = 6, // маска команды[2] + resserved[2]
            PAGE_RESERVED = 7, // reserved for future use[4]
            PAGE_DATA_START = 8, // 1st data page: номер КП[1] + время посещения КП[3]
        }

        public enum RfidConfigPageType
        {
            PAGE_DYNAMIC_LOCK = 5, // dynamic lock bytes, RFUI
            PAGE_CFG0 = 4, // CFG 0
            PAGE_CFG1 = 3, // CFG 1
            PAGE_PWD = 2, // PWD
            PAGE_PACK = 1, // PACK, RFUI1
        }

        private int[] _dump;

        public int[] Dump => _dump;

        private byte[] _uid = new byte[8];
        public byte[] Uid => _uid;

        private byte _sysChipId = 0;
        public byte SystemChipId => _sysChipId;

        private ushort _teamNum = 0;
        public ushort TeamNumber => _teamNum;

        private byte _chipType = 0;
        public byte ChipType => _chipType;

        private byte _fwVer = 0;
        public byte FwVer => _fwVer;

        private DateTime _initTime = new DateTime();
        public DateTime InitTime => _initTime;

        private ushort _teamMask = 0;
        public ushort TeamMask => _teamMask;

        public byte ReferenceDateByte = (byte)(Helpers.ConvertToUnixTimestamp(DateTime.Now.ToUniversalTime()) >> 24);

        public readonly byte CurrentChipType;

        public DataTable Table;

        public RfidContainer(byte chipTypeId)
        {
            if (!ChipTypes.Names.TryGetValue(chipTypeId, out _))
                throw new Exception("Chip type not exists: " + chipTypeId.ToString());

            CurrentChipType = chipTypeId;

            _dump = new int[ChipTypes.PageSizes[chipTypeId] * 4];
            for (var i = 0; i < _dump.Length; i++)
                _dump[i] = -1;

            Table = new DataTable("RFID") { Columns = { "Page#", "Description", "Raw data", "Decoded data" } };
            Table.Columns[2].MaxLength = ChipTypes.PageSize * 3;
            InitTable();
        }

        public void InitTable()
        {
            Table.Rows.Clear();

            var chipPagesNumber = ChipTypes.PageSizes[CurrentChipType];
            for (var i = 0; i < chipPagesNumber; i++)
            {
                var row = Table.NewRow();
                row[0] = i.ToString();
                if (i == (int)RfidPageType.PAGE_UID1)
                    row[1] = "UID0[3],CRC0";
                else if (i == (int)RfidPageType.PAGE_UID2)
                    row[1] = "UID1[4]";
                else if (i == (int)RfidPageType.PAGE_CHIP_SYS1)
                    row[1] = "CRC1,Int,Lock bytes[2]";
                else if (i == (int)RfidPageType.PAGE_CHIP_SYS2)
                    row[1] = "CC0,CC1,Size,CC3";
                else if (i == (int)RfidPageType.PAGE_CHIP_NUM)
                    row[1] = "Team#[2],ChipTypes,FwVer";
                else if (i == (int)RfidPageType.PAGE_INIT_TIME)
                    row[1] = "InitDate[4]";
                else if (i == (int)RfidPageType.PAGE_TEAM_MASK)
                    row[1] = "TeamMask[2],Reserved[2]";
                else if (i == (int)RfidPageType.PAGE_RESERVED)
                    row[1] = "Reserved[4]";
                else if (i == chipPagesNumber - (int)RfidConfigPageType.PAGE_DYNAMIC_LOCK)
                    row[1] = "Dyn.lock[3],RFUI";
                else if (i == chipPagesNumber - (int)RfidConfigPageType.PAGE_CFG0)
                    row[1] = "MIRROR,RFUI,MIRROR PAGE,AUTH0";
                else if (i == chipPagesNumber - (int)RfidConfigPageType.PAGE_CFG1)
                    row[1] = "ACCESS,RFUI,RFUI,RFUI";
                else if (i == chipPagesNumber - (int)RfidConfigPageType.PAGE_PWD)
                    row[1] = "PWD[4]";
                else if (i == chipPagesNumber - (int)RfidConfigPageType.PAGE_PACK)
                    row[1] = "PACK[2],RFUI,RFUI";
                else
                    row[1] = "Record " + (i - 8).ToString("D2");

                Table.Rows.Add(row);
            }
        }

        public bool AddPages(byte startPageNumber, byte[] data)
        {
            if (startPageNumber + data.Length / 4 > ChipTypes.PageSizes[CurrentChipType] * 4)
                return false;

            //if (data.Length != ChipTypes.PageSize) return false;
            for (var i = 0; i < data.Length; i++)
                _dump[startPageNumber * 4 + i] = data[i];

            ParseChipToTable(startPageNumber, startPageNumber + data.Length / 4 - 1);

            return true;
        }

        public byte[] GetPage(byte pageNumber)
        {
            var page = new byte[ChipTypes.PageSize];
            for (var i = 0; i < page.Length; i++)
                page[i] = (byte)_dump[pageNumber * 4 + i];

            return page;
        }

        public string[] GetTablePage(byte pageNumber)
        {
            var page = new string[Table.Columns.Count];
            for (var i = 0; i < page.Length; i++)
                page[i] = Table.Rows[pageNumber].ItemArray[i].ToString();

            return page;
        }

        public byte[] GetUid()
        {
            return new byte[]
            {
                (byte)_dump[0],
                (byte)_dump[1],
                (byte)_dump[2],
                (byte)_dump[3],
                (byte)_dump[4],
                (byte)_dump[5],
                (byte)_dump[6],
                (byte)_dump[7]
            };
        }

        public byte GetChipType()
        {
            return (byte)_dump[14];
        }

        public ushort GetTeamNumber()
        {
            return (ushort)(_dump[16] * 256 + _dump[17]);
        }

        public byte GetFwVersion()
        {
            return (byte)_dump[19];
        }

        public DateTime GetInitTime()
        {
            long m = _dump[20] * 16777216 + _dump[21] * 65536 + _dump[22] * 256 + _dump[23];
            var initTime = Helpers.ConvertFromUnixTimestamp(m);
            return initTime;
        }

        public ushort GetMask()
        {
            return (ushort)(_dump[24] * 256 + _dump[25]);
        }

        private void ParseChipToTable(int pageFrom, int pageTo)
        {
            var tmp = new byte[4];
            while (pageFrom <= pageTo && Table.Rows.Count > pageFrom)
            {
                for (var i = 0; i < 4; i++) tmp[i] = (byte)_dump[pageFrom * 4 + i];
                Table.Rows[pageFrom][2] =
                    Helpers.ConvertByteArrayToHex(tmp);

                var result = "";

                if (pageFrom == (int)RfidPageType.PAGE_UID1)
                {
                    _uid[0] = tmp[0];
                    _uid[1] = tmp[1];
                    _uid[2] = tmp[2];
                    _uid[3] = tmp[3];
                }
                else if (pageFrom == (int)RfidPageType.PAGE_UID2)
                {
                    _uid[4] = tmp[0];
                    _uid[5] = tmp[1];
                    _uid[6] = tmp[2];
                    _uid[7] = tmp[3];
                    for (var i = 0; i < _uid.Length; i++) result += _uid[i].ToString("X2") + ":";

                    result = result.TrimEnd(new[] { ':' });
                }
                if (pageFrom == (int)RfidPageType.PAGE_CHIP_SYS1)
                {
                    ;
                }
                else if (pageFrom == (int)RfidPageType.PAGE_CHIP_SYS2)
                {
                    _sysChipId = tmp[2];
                    var tagSize = "";
                    if (_sysChipId == 0x12)
                        tagSize += ChipTypes.Names[0] + " = " + ChipTypes.ByteSizes[0] + " bytes";
                    else if (_sysChipId == 0x3e)
                        tagSize += ChipTypes.Names[1] + " = " + ChipTypes.ByteSizes[1] + " bytes";
                    else if (_sysChipId == 0x6d)
                        tagSize += ChipTypes.Names[2] + " = " + ChipTypes.ByteSizes[2] + " bytes";
                    result = tagSize;
                }
                else if (pageFrom == (int)RfidPageType.PAGE_CHIP_NUM)
                {
                    _teamNum = (ushort)(tmp[0] * 256 + tmp[1]);
                    _chipType = tmp[2];
                    _fwVer = tmp[3];
                    result = "Team #" + _teamNum + ", " + "Ntag" + _chipType + ", fw v." + _fwVer;
                }
                else if (pageFrom == (int)RfidPageType.PAGE_INIT_TIME)
                {
                    ReferenceDateByte = tmp[0];
                    long m = tmp[0] * 16777216 + tmp[1] * 65536 + tmp[2] * 256 + tmp[3];
                    _initTime = Helpers.ConvertFromUnixTimestamp(m);
                    result = Helpers.DateToString(_initTime);
                }
                else if (pageFrom == (int)RfidPageType.PAGE_TEAM_MASK)
                {
                    _teamMask = (ushort)(tmp[0] * 256 + tmp[1]);
                    result = Helpers.ConvertMaskToString(_teamMask);
                }
                else if (pageFrom > (int)RfidPageType.PAGE_RESERVED
                         && pageFrom < ChipTypes.PageSizes[CurrentChipType] - (int)RfidConfigPageType.PAGE_DYNAMIC_LOCK)
                {
                    if (tmp[0] != 0)
                    {
                        long m = ReferenceDateByte * 16777216 + tmp[1] * 65536 + tmp[2] * 256 + tmp[3];
                        var t = Helpers.ConvertFromUnixTimestamp(m);
                        result =
                            "KP#" + tmp[0] + ", " + Helpers.DateToString(t);
                    }
                    else
                    {
                        result = "-";
                    }
                }
                else if (pageFrom == ChipTypes.PageSizes[CurrentChipType] - (int)RfidConfigPageType.PAGE_CFG0)
                {
                    result = "Chip " + (tmp[3] == 0xff ? "not " : "") + "locked";
                }

                Table.Rows[pageFrom][3] = result;
                pageFrom++;
            }
        }
    }
}
