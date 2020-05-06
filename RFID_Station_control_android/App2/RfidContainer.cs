using System;
using System.Collections.Generic;
using System.Data;

namespace RfidStationControl
{
    public class RfidContainer
    {
        private int[] _dump;

        public int[] Dump
        {
            get
            {
                return _dump;
            }
        }

        private byte[] _uid = new byte[8];
        public byte[] Uid
        {
            get
            {
                return _uid;
            }
        }

        private byte _sysChipId = 0;
        public byte SystemChipId
        {
            get
            {
                return _sysChipId;
            }
        }

        private UInt16 _teamNum = 0;
        public UInt16 TeamNumber
        {
            get
            {
                return _teamNum;
            }
        }

        private byte _chipType = 0;
        public byte ChipType
        {
            get
            {
                return _chipType;
            }
        }

        private byte _fwVer = 0;
        public byte FwVer
        {
            get
            {
                return _fwVer;
            }
        }

        private DateTime _initTime = new DateTime();
        public DateTime InitTime
        {
            get
            {
                return _initTime;
            }
        }

        private UInt16 _teamMask = 0;
        public UInt16 TeamMask
        {
            get
            {
                return _teamMask;
            }
        }

        public byte ReferenceDateByte = (byte)(Helpers.ConvertToUnixTimestamp(DateTime.Now.ToUniversalTime()) >> 24);
        public class ChipTypes
        {
            public static Dictionary<string, byte> Types = new Dictionary<string, byte>
            {
                {"NTAG213", 0},
                {"NTAG215", 1},
                {"NTAG216", 2}
            };

            public const byte PageSize = 4;


            public readonly static Dictionary<byte, string> Names = new Dictionary<byte, string>
            {
                { 0, "NTAG213" },
                { 1, "NTAG215" },
                { 2, "NTAG216" }
            };

            public readonly static Dictionary<byte, byte> PageSizes = new Dictionary<byte, byte>
            {
                { 0, 44 },
                { 1, 134 },
                { 2, 230 }
            };
            public readonly static Dictionary<byte, UInt16> ByteSizes = new Dictionary<byte, UInt16>
            {
                { 0, 144 },
                { 1, 496 },
                { 2, 872 }
            };
            public readonly static Dictionary<byte, byte> Ids = new Dictionary<byte, byte>
            {
                { 0, 213 },
                { 1, 215 },
                { 2, 216 }
            };
            public readonly static Dictionary<byte, byte> SystemIds = new Dictionary<byte, byte>
            {
                { 0, 0x12 },
                { 1, 0x3e },
                { 2, 0x6d }
            };
        }

        public readonly byte CurrentChipType;

        public DataTable Table;

        public RfidContainer(byte chipTypeId)
        {
            if (!ChipTypes.Names.TryGetValue(chipTypeId, out _))
            {
                throw new Exception("Chip type not exists: " + chipTypeId.ToString());
            }
            CurrentChipType = chipTypeId;

            _dump = new int[ChipTypes.PageSizes[chipTypeId] * 4];
            for (int i = 0; i < _dump.Length; i++)
            {
                _dump[i] = -1;
            }

            Table = new DataTable("RFID") { Columns = { "Page#", "Description", "Raw data", "Decoded data" } };
            Table.Columns[2].MaxLength = ChipTypes.PageSize * 3;
            InitTable();
        }

        public void InitTable()
        {
            Table.Rows.Clear();

            byte chipPagesNumber = ChipTypes.PageSizes[CurrentChipType];
            for (int i = 0; i < chipPagesNumber; i++)
            {
                var row = Table.NewRow();
                row[0] = i.ToString();
                if (i == 0)
                {
                    row[1] = "UID0[4]";
                }
                else if (i == 1)
                {
                    row[1] = "UID1[4]";
                }
                else if (i == 2)
                {
                    row[1] = "SN,Int,Lock bytes[2]";
                }
                else if (i == 3)
                {
                    row[1] = "CC0,CC1,Size,CC3";
                }
                else if (i == 4)
                {
                    row[1] = "Team#[2],ChipTypes,FwVer";
                }
                else if (i == 5)
                {
                    row[1] = "InitDate[4]";
                }
                else if (i == 6)
                {
                    row[1] = "TeamMask[2],Reserved[2]";
                }
                else if (i == 7)
                {
                    row[1] = "Reserved[4]";
                }
                else if (i == chipPagesNumber - 4)
                {
                    row[1] = "MIRROR,RFUI,MIRROR PAGE,AUTH0";
                }
                else if (i == chipPagesNumber - 3)
                {
                    row[1] = "ACCESS,RFUI,RFUI,RFUI";
                }
                else if (i == chipPagesNumber - 2)
                {
                    row[1] = "PWD[4]";
                }
                else if (i == chipPagesNumber - 1)
                {
                    row[1] = "PACK[2],RFUI,RFUI";
                }
                else if (i > 7)
                {
                    row[1] = "Record " + (i - 8).ToString("D2");
                }
                Table.Rows.Add(row);
            }
        }

        public bool AddPages(byte startPageNumber, byte[] data)
        {
            if (startPageNumber + data.Length / 4 > ChipTypes.PageSizes[CurrentChipType] * 4)
                return false;
            //if (data.Length != ChipTypes.PageSize) return false;
            for (int i = 0; i < data.Length; i++)
            {
                _dump[startPageNumber * 4 + i] = data[i];
            }
            ParseChipToTable(startPageNumber, startPageNumber + (data.Length / 4) - 1);
            return true;
        }

        public byte[] GetPage(byte pageNumber)
        {
            byte[] page = new byte[ChipTypes.PageSize];
            for (int i = 0; i < page.Length; i++)
            {
                page[i] = (byte)_dump[pageNumber * 4 + i];
            }
            return page;
        }

        public string[] GetTablePage(byte pageNumber)
        {
            string[] page = new string[Table.Columns.Count];
            for (int i = 0; i < page.Length; i++)
            {
                page[i] = Table.Rows[pageNumber].ItemArray[i].ToString();
            }
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

        public UInt16 GetTeamNumber()
        {
            return (UInt16)(_dump[16] * 256 + _dump[17]);
        }

        public byte GetFwVersion()
        {
            return (byte)_dump[19];
        }

        public DateTime GetInitTime()
        {
            long m = _dump[20] * 16777216 + _dump[21] * 65536 + _dump[22] * 256 + _dump[23];
            DateTime initTime = Helpers.ConvertFromUnixTimestamp(m);
            return initTime;
        }

        public UInt16 GetMask()
        {
            return (UInt16)(_dump[24] * 256 + _dump[25]);
        }

        private void ParseChipToTable(int pageFrom, int pageTo)
        {
            byte[] tmp = new byte[4];
            while (pageFrom <= pageTo && Table.Rows.Count > pageFrom)
            {
                for (int i = 0; i < 4; i++)
                {
                    tmp[i] = (byte)_dump[pageFrom * 4 + i];
                }
                Table.Rows[pageFrom][2] =
                    Helpers.ConvertByteArrayToHex(tmp);

                string result = "";

                if (pageFrom == 0)
                {
                    _uid[0] = tmp[0];
                    _uid[1] = tmp[1];
                    _uid[2] = tmp[2];
                    _uid[3] = tmp[3];
                }
                else if (pageFrom == 1)
                {
                    _uid[4] = tmp[0];
                    _uid[5] = tmp[1];
                    _uid[6] = tmp[2];
                    _uid[7] = tmp[3];
                    for (int i = 0; i < _uid.Length; i++)
                    {
                        result += _uid[i].ToString("X2") + ":";
                    }

                    result = result.TrimEnd(new[] { ':' });
                }
                if (pageFrom == 2)
                    ;
                else if (pageFrom == 3)
                {
                    _sysChipId = tmp[2];
                    string tagSize = "";
                    if (_sysChipId == 0x12)
                        tagSize += ChipTypes.Names[0] + " = " + ChipTypes.ByteSizes[0] + " bytes";
                    else if (_sysChipId == 0x3e)
                        tagSize += ChipTypes.Names[1] + " = " + ChipTypes.ByteSizes[1] + " bytes";
                    else if (_sysChipId == 0x6d)
                        tagSize += ChipTypes.Names[2] + " = " + ChipTypes.ByteSizes[2] + " bytes";
                    result = tagSize;
                }
                else if (pageFrom == 4)
                {
                    _teamNum = (UInt16)(tmp[0] * 256 + tmp[1]);
                    _chipType = tmp[2];
                    _fwVer = tmp[3];
                    result = "Team #" + _teamNum + ", " + "Ntag" + _chipType + ", fw v." + _fwVer;
                }
                else if (pageFrom == 5)
                {
                    ReferenceDateByte = tmp[0];
                    long m = tmp[0] * 16777216 + tmp[1] * 65536 + tmp[2] * 256 + tmp[3];
                    _initTime = Helpers.ConvertFromUnixTimestamp(m);
                    result = Helpers.DateToString(_initTime);
                }
                else if (pageFrom == 6)
                {
                    _teamMask = (UInt16)(tmp[0] * 256 + tmp[1]);
                    result = Helpers.ConvertMaskToString(_teamMask);
                }
                else if (pageFrom > 7 && pageFrom < ChipTypes.PageSizes[CurrentChipType] - 4)
                {
                    if (tmp[0] != 0)
                    {
                        long m = ReferenceDateByte * 16777216 + tmp[1] * 65536 + tmp[2] * 256 + tmp[3];
                        DateTime t = Helpers.ConvertFromUnixTimestamp(m);
                        result =
                            "KP#" + tmp[0] + ", " + Helpers.DateToString(t);
                    }
                    else
                        result = "-";
                }

                Table.Rows[pageFrom][3] = result;
                pageFrom++;
            }
        }
    }
}
