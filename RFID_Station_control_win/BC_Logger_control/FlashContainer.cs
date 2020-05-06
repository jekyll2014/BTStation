using System;
using System.Data;

namespace RfidStationControl
{
    public class FlashContainer
    {
        public byte[] Dump { get; private set; }

        public class TeamDumpData
        {
            public int TeamNumber;
            public DateTime InitTime;
            public int TeamMask;
            public DateTime LastCheckTime;
            public int DumpSize;
            public RfidContainer ChipDump;
        }

        public UInt32 Size { get; }
        public UInt32 TeamDumpSize { get; }

        public DataTable Table;
        public UInt32 _bytesPerRow { get; }

        public readonly UInt16 _dumpHeaderSize = 16;

        public static class TableColumns
        {
            public static string TeamNumber = "Team#";
            public static string ByteNumber = "Byte#";
            public static string DecodedData = "Decoded data";
            public static string RawData = "Raw data";
        }

        public FlashContainer(UInt32 size, UInt32 teamDumpSize = 0, UInt32 bytesPerRow = 0)
        {
            Size = size;

            if (teamDumpSize == 0)
                TeamDumpSize = 1024;
            else
                TeamDumpSize = teamDumpSize;

            if (bytesPerRow == 0)
                _bytesPerRow = TeamDumpSize;
            else
                _bytesPerRow = bytesPerRow;

            Dump = new byte[Size];
            Table = new DataTable("FLASH")
            {
                Columns =
                {
                    TableColumns.TeamNumber,
                    TableColumns.ByteNumber,
                    TableColumns.DecodedData,
                    TableColumns.RawData
                }
            };
            Table.Columns[3].MaxLength = (int)bytesPerRow * 3;
            InitTable(Size);
        }

        private void InitTable(long flashSize)
        {
            Dump = new byte[Size];

            int pageNum = (int)(flashSize / _bytesPerRow);
            Table.Rows.Clear();
            UInt32 k = TeamDumpSize / _bytesPerRow;
            for (long i = 0; i < pageNum; i++)
            {
                var row = Table.NewRow();
                row[TableColumns.TeamNumber] = ((int)(i * k)).ToString();
                row[TableColumns.ByteNumber] = (i * _bytesPerRow).ToString();
                row[TableColumns.DecodedData] = "";
                row[TableColumns.RawData] = "";
                Table.Rows.Add(row);
            }
        }

        public bool Add(long index, byte[] data, long size = 0)
        {
            if (index + size > Size)
                return false;
            if (size == 0)
                size = data.Length;
            for (long i = 0; i < size; i++)
            {
                Dump[index + i] = data[i];
            }
            int rowFrom = (int)(index / _bytesPerRow);
            int rowTo = (int)((index + size - 1) / _bytesPerRow);

            ParseToTable(rowFrom, rowTo);
            return true;
        }

        public TeamDumpData GetTeamBlock(UInt32 teamNumber)
        {
            UInt32 startAddress = teamNumber * TeamDumpSize;
            if (Dump[startAddress] == 0xff || (Dump[startAddress] + Dump[startAddress + 1]) != 0x00)
            {
                return null;
            }
            byte[] tmpData = new byte[_bytesPerRow];
            for (uint i = 0; i < _bytesPerRow; i++)
            {
                tmpData[i] = Dump[teamNumber * TeamDumpSize + i];
            }

            TeamDumpData teamBlock = new TeamDumpData();
            teamBlock.TeamNumber = tmpData[0] * 256 + tmpData[1];

            long timeNumber = tmpData[2] * 16777216 + tmpData[3] * 65536 + tmpData[4] * 256 + tmpData[5];
            teamBlock.InitTime = Helpers.ConvertFromUnixTimestamp(timeNumber);

            teamBlock.TeamMask = (tmpData[6] * 256 + tmpData[7]);

            timeNumber = tmpData[8] * 16777216 + tmpData[9] * 65536 + tmpData[10] * 256 + tmpData[11];
            teamBlock.LastCheckTime = Helpers.ConvertFromUnixTimestamp(timeNumber);

            teamBlock.DumpSize = tmpData[12];

            byte chipType = 0;
            for (byte i = 0; i < RfidContainer.ChipTypes.SystemIds.Count; i++)
            {
                if (RfidContainer.ChipTypes.SystemIds[i] == Dump[_dumpHeaderSize + 14])
                {
                    chipType = i;
                    break;
                }
            }

            teamBlock.ChipDump = new RfidContainer(chipType);
            byte[] chip = new byte[RfidContainer.ChipTypes.PageSizes[chipType] * RfidContainer.ChipTypes.PageSize];
            for (int i = 0; i < chip.Length; i++)
            {
                chip[i] = Dump[_dumpHeaderSize + i];
            }
            teamBlock.ChipDump.AddPages(0, chip);

            return teamBlock;
        }

        public string[] GetTablePage(int pageNumber)
        {
            string[] page = new string[Table.Columns.Count];
            for (int i = 0; i < page.Length; i++)
            {
                page[i] = Table.Rows[pageNumber].ItemArray[i].ToString();
            }
            return page;
        }

        private void ParseToTable(int rowFrom, int rowTo)
        {
            while (rowFrom <= rowTo)
            {
                byte[] tmpData = new byte[_bytesPerRow];
                for (uint i = 0; i < _bytesPerRow; i++)
                {
                    tmpData[i] = Dump[rowFrom * _bytesPerRow + i];
                }

                Table.Rows[rowFrom][TableColumns.RawData] = Helpers.ConvertByteArrayToHex(tmpData);

                //parse dump data
                if (tmpData[0] != 0xff && (tmpData[0] + tmpData[1]) != 0x00)
                {
                    //1-2: номер команды
                    int teamNum = tmpData[0] * 256 + tmpData[1];

                    //3-6: время инициализации
                    long timeNumber = tmpData[2] * 16777216 + tmpData[3] * 65536 + tmpData[4] * 256 + tmpData[5];
                    DateTime initTime = Helpers.ConvertFromUnixTimestamp(timeNumber);

                    //7-8: маска команды
                    UInt16 maskNumber = (UInt16)(tmpData[6] * 256 + tmpData[7]);

                    //9-12: время последней отметки на станции
                    timeNumber = tmpData[8] * 16777216 + tmpData[9] * 65536 + tmpData[10] * 256 + tmpData[11];
                    DateTime lastCheck = Helpers.ConvertFromUnixTimestamp(timeNumber);

                    //13: размер дампа
                    int dumpSize = tmpData[12];
                    //int dumpSize = tmpData[12] * 256 + tmpData[13];
                    if (dumpSize * 4 + 16 >= _bytesPerRow)
                        dumpSize = 0;

                    string result = "Team #" + teamNum +
                       ", InitTime: " + Helpers.DateToString(initTime) +
                       ", Mask: " + Helpers.ConvertMaskToString(maskNumber) +
                       ", Last check: " + Helpers.DateToString(lastCheck) +
                       ", Dump size: " + dumpSize + ", ";

                    //1st byte of time
                    byte todayByte = (byte)(Helpers.ConvertToUnixTimestamp(DateTime.Now.ToUniversalTime()) >> 24);

                    result += "Dump data: ";
                    int page = 4;
                    while (page < dumpSize + 4)
                    {
                        // page 4+(0..1): UID
                        if (page == 4)
                        {
                            byte[] uid = { tmpData[page * 4 + 0],
                                tmpData[page * 4 + 1],
                                tmpData[page * 4 + 2],
                                tmpData[page * 4 + 3],
                                tmpData[page * 4 + 4],
                                tmpData[page * 4 + 5],
                                tmpData[page * 4 + 6],
                                tmpData[page * 4 + 7] };
                            result += "UID " + Helpers.ConvertByteArrayToHex(uid) + ", ";
                        }
                        // page 4+3: chip type
                        else if (page == 7)
                        {
                            string tagSize = "Ntag";
                            if (tmpData[page * 4 + 2] == 0x12)
                                tagSize += "213(144 bytes)";
                            else if (tmpData[page * 4 + 2] == 0x3e)
                                tagSize += "215(496 bytes)";
                            else if (tmpData[page * 4 + 2] == 0x6d)
                                tagSize += "216(872 bytes)";
                            result += tagSize + ", ";
                        }
                        // page 4+4: team#, chip type, fw ver.
                        else if (page == 8)
                        {
                            uint m = (uint)(tmpData[page * 4 + 0] * 256 + tmpData[page * 4 + 1]);
                            result += "Team #" + m + ", " + "Ntag" + tmpData[page * 4 + 2] + ", fw v." + tmpData[page * 4 + 3] + ", ";
                        }
                        // page 4+5: init time
                        else if (page == 9)
                        {
                            todayByte = tmpData[page * 4 + 0];
                            long m = tmpData[page * 4 + 0] * 16777216 + tmpData[page * 4 + 1] * 65536 + tmpData[page * 4 + 2] * 256 +
                                     tmpData[page * 4 + 3];
                            DateTime t = Helpers.ConvertFromUnixTimestamp(m);
                            result += "InitTime: " + Helpers.DateToString(t) + ", ";
                        }
                        // page 4+6: team mask
                        else if (page == 10)
                        {
                            byte[] mask = { tmpData[page * 4 + 0], tmpData[page * 4 + 1] };
                            result += "Mask: " + Helpers.ConvertMaskToString(mask) + ", ";
                        }
                        // page4+7: reserved
                        else if (page == 10)
                            ;
                        // page4+8...: marks
                        else if (page > 11)
                        {
                            long m = todayByte * 16777216 + tmpData[page * 4 + 1] * 65536 +
                                     tmpData[page * 4 + 2] * 256 + tmpData[page * 4 + 3];
                            DateTime t = Helpers.ConvertFromUnixTimestamp(m);
                            result += "KP#" + tmpData[page * 4 + 0] + ", " + Helpers.DateToString(t) + ", ";
                        }
                        page++;
                    }
                    Table.Rows[rowFrom][TableColumns.DecodedData] = result;
                }
                else
                {
                    Table.Rows[rowFrom][TableColumns.DecodedData] = "-";
                }
                rowFrom++;
            }
        }
    }
}
