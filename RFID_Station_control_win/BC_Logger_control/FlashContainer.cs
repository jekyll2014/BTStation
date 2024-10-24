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

        public uint Size { get; }
        public uint TeamDumpSize { get; }

        public DataTable Table;
        public uint _bytesPerRow { get; }

        public readonly ushort _dumpHeaderSize = 16;

        public static class TableColumns
        {
            public static string TeamNumber = "Team#";
            public static string ByteNumber = "Byte#";
            public static string ChipInfo = "Chip info#";
            public static string DecodedData = "Decoded data";
            public static string RawData = "Raw data";
        }

        public FlashContainer(uint size, uint teamDumpSize = 0, uint bytesPerRow = 0)
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
                    TableColumns.ChipInfo,
                    TableColumns.DecodedData,
                    TableColumns.RawData
                }
            };
            Table.Columns[TableColumns.RawData].MaxLength = (int)bytesPerRow * 3;
            InitTable(Size);
        }

        private void InitTable(long flashSize)
        {
            Dump = new byte[Size];

            var pageNum = (int)(flashSize / _bytesPerRow);
            Table.Rows.Clear();
            var k = TeamDumpSize / _bytesPerRow;
            for (long i = 0; i < pageNum; i++)
            {
                var row = Table.NewRow();
                row[TableColumns.TeamNumber] = ((int)(i * k)).ToString();
                row[TableColumns.ByteNumber] = (i * _bytesPerRow).ToString();
                row[TableColumns.ChipInfo] = "";
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
            for (long i = 0; i < size; i++) Dump[index + i] = data[i];
            var rowFrom = (int)(index / _bytesPerRow);
            var rowTo = (int)((index + size - 1) / _bytesPerRow);

            ParseToTable(rowFrom, rowTo);
            return true;
        }

        public TeamDumpData GetTeamBlock(uint teamNumber)
        {
            var startAddress = teamNumber * TeamDumpSize;
            if (Dump[startAddress] == 0xff || Dump[startAddress] + Dump[startAddress + 1] != 0x00) return null;
            var tmpData = new byte[_bytesPerRow];
            for (uint i = 0; i < _bytesPerRow; i++) tmpData[i] = Dump[teamNumber * TeamDumpSize + i];

            var teamBlock = new TeamDumpData();
            teamBlock.TeamNumber = tmpData[0] * 256 + tmpData[1];

            long timeNumber = tmpData[2] * 16777216 + tmpData[3] * 65536 + tmpData[4] * 256 + tmpData[5];
            teamBlock.InitTime = Helpers.ConvertFromUnixTimestamp(timeNumber);

            teamBlock.TeamMask = tmpData[6] * 256 + tmpData[7];

            timeNumber = tmpData[8] * 16777216 + tmpData[9] * 65536 + tmpData[10] * 256 + tmpData[11];
            teamBlock.LastCheckTime = Helpers.ConvertFromUnixTimestamp(timeNumber);

            teamBlock.DumpSize = tmpData[12];

            byte chipType = 0;
            for (byte i = 0; i < RfidContainer.ChipTypes.SystemIds.Count; i++)
                if (RfidContainer.ChipTypes.SystemIds[i] == Dump[_dumpHeaderSize + 14])
                {
                    chipType = i;
                    break;
                }

            teamBlock.ChipDump = new RfidContainer(chipType);
            var chip = new byte[RfidContainer.ChipTypes.PageSizes[chipType] * RfidContainer.ChipTypes.PageSize];
            for (var i = 0; i < chip.Length; i++) chip[i] = Dump[_dumpHeaderSize + i];
            teamBlock.ChipDump.AddPages(0, chip);

            return teamBlock;
        }

        public string[] GetTablePage(int pageNumber)
        {
            var page = new string[Table.Columns.Count];
            for (var i = 0; i < page.Length; i++) page[i] = Table.Rows[pageNumber].ItemArray[i].ToString();
            return page;
        }

        private void ParseToTable(int rowFrom, int rowTo)
        {
            while (rowFrom <= rowTo)
            {
                var tmpData = new byte[_bytesPerRow];
                for (uint i = 0; i < _bytesPerRow; i++) tmpData[i] = Dump[rowFrom * _bytesPerRow + i];

                Table.Rows[rowFrom][TableColumns.RawData] = Helpers.ConvertByteArrayToHex(tmpData);

                //parse dump data
                if (tmpData[0] != 0xff && tmpData[0] + tmpData[1] != 0x00)
                {
                    //1-2: номер команды
                    var teamNum = tmpData[0] * 256 + tmpData[1];

                    //3-6: время инициализации
                    long timeNumber = tmpData[2] * 16777216 + tmpData[3] * 65536 + tmpData[4] * 256 + tmpData[5];
                    var initTime = Helpers.ConvertFromUnixTimestamp(timeNumber);

                    //7-8: маска команды
                    var maskNumber = (ushort)(tmpData[6] * 256 + tmpData[7]);

                    //9-12: время последней отметки на станции
                    timeNumber = tmpData[8] * 16777216 + tmpData[9] * 65536 + tmpData[10] * 256 + tmpData[11];
                    var lastCheck = Helpers.ConvertFromUnixTimestamp(timeNumber);

                    //13: размер дампа
                    int dumpSize = tmpData[12];
                    //int dumpSize = tmpData[12] * 256 + tmpData[13];
                    if (dumpSize * 4 + 16 >= _bytesPerRow)
                        dumpSize = 0;

                    var chipInfo = "Team #" + teamNum +
                       ", InitTime: " + Helpers.DateToString(initTime) +
                       ", Mask: " + Helpers.ConvertMaskToString(maskNumber) +
                       ", Last check: " + Helpers.DateToString(lastCheck) +
                       ", Dump size: " + dumpSize;

                    //1st byte of time
                    var todayByte = (byte)(Helpers.ConvertToUnixTimestamp(DateTime.Now.ToUniversalTime()) >> 24);

                    var checkPointsList = "";
                    var page = 4;
                    while (page < dumpSize + 4)
                    {
                        // page (0..1): UID
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
                            checkPointsList += "UID " + Helpers.ConvertByteArrayToHex(uid) + Environment.NewLine;
                        }
                        // page 3: chip type
                        else if (page == 7)
                        {
                            var tagSize = "Ntag";
                            if (tmpData[page * 4 + 2] == 0x12)
                                tagSize += "213(144 bytes)";
                            else if (tmpData[page * 4 + 2] == 0x3e)
                                tagSize += "215(496 bytes)";
                            else if (tmpData[page * 4 + 2] == 0x6d)
                                tagSize += "216(872 bytes)";
                            checkPointsList += tagSize + Environment.NewLine;
                        }
                        // page 4: team#, chip type, fw ver.
                        else if (page == 8)
                        {
                            var m = (uint)(tmpData[page * 4 + 0] * 256 + tmpData[page * 4 + 1]);
                            checkPointsList += "Team #" + m + ", " + "Ntag" + tmpData[page * 4 + 2] + ", fw v." + tmpData[page * 4 + 3] + Environment.NewLine;
                        }
                        // page 5: init time
                        else if (page == 9)
                        {
                            todayByte = tmpData[page * 4 + 0];
                            long m = tmpData[page * 4 + 0] * 16777216 + tmpData[page * 4 + 1] * 65536 + tmpData[page * 4 + 2] * 256 +
                                     tmpData[page * 4 + 3];
                            var t = Helpers.ConvertFromUnixTimestamp(m);
                            checkPointsList += "InitTime: " + Helpers.DateToString(t) + Environment.NewLine;
                        }
                        // page 6: team mask
                        else if (page == 10)
                        {
                            byte[] mask = { tmpData[page * 4 + 0], tmpData[page * 4 + 1] };
                            checkPointsList += "Mask: " + Helpers.ConvertMaskToString(mask) + Environment.NewLine;
                        }
                        // page 7: reserved
                        else if (page == 10)
                        {
                            ;
                        }
                        // page 8...: marks
                        else if (page > 11)
                        {
                            long m = todayByte * 16777216 + tmpData[page * 4 + 1] * 65536 +
                                     tmpData[page * 4 + 2] * 256 + tmpData[page * 4 + 3];
                            var t = Helpers.ConvertFromUnixTimestamp(m);
                            checkPointsList += "KP#" + tmpData[page * 4 + 0] + ", " + Helpers.DateToString(t) + Environment.NewLine;
                        }
                        page++;
                    }
                    Table.Rows[rowFrom][TableColumns.ChipInfo] = chipInfo;
                    Table.Rows[rowFrom][TableColumns.DecodedData] = checkPointsList;
                }
                else
                {
                    Table.Rows[rowFrom][TableColumns.ChipInfo] = "-";
                    Table.Rows[rowFrom][TableColumns.DecodedData] = "-";
                }
                rowFrom++;
            }
        }
    }
}
