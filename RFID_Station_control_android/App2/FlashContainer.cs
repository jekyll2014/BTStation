using System;
using System.Data;

namespace RfidStationControl
{
    public class FlashContainer
    {
        public byte[] Dump { get; private set; }

        private uint Size { get; }
        public uint TeamDumpSize { get; }

        private readonly DataTable Table;
        private uint BytesPerRow { get; }

        private const ushort DUMP_HEADER_SIZE = 16;

        private static class TableColumns
        {
            public const string TeamNumber = "Team#";
            public const string ByteNumber = "Byte#";
            public const string DecodedData = "Decoded data";
            public const string RawData = "Raw data";
        }

        public FlashContainer(uint size, uint teamDumpSize = 0, uint bytesPerRow = 0)
        {
            Size = size;

            TeamDumpSize = teamDumpSize == 0 ? 1024 : teamDumpSize;

            BytesPerRow = bytesPerRow == 0 ? TeamDumpSize : bytesPerRow;

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

            var pageNum = (int)(flashSize / BytesPerRow);
            Table.Rows.Clear();
            var k = TeamDumpSize / BytesPerRow;
            for (long i = 0; i < pageNum; i++)
            {
                var row = Table.NewRow();
                row[TableColumns.TeamNumber] = ((int)(i * k)).ToString();
                row[TableColumns.ByteNumber] = (i * BytesPerRow).ToString();
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
            var rowFrom = (int)(index / BytesPerRow);
            var rowTo = (int)((index + size - 1) / BytesPerRow);

            ParseToTable(rowFrom, rowTo);
            return true;
        }

        public TeamDumpData GetTeamBlock(uint teamNumber)
        {
            var startAddress = teamNumber * TeamDumpSize;
            if (Dump[startAddress] == 0xff || Dump[startAddress] + Dump[startAddress + 1] != 0x00) return null;
            var tmpData = new byte[BytesPerRow];
            for (uint i = 0; i < BytesPerRow; i++) tmpData[i] = Dump[teamNumber * TeamDumpSize + i];

            var teamBlock = new TeamDumpData
            {
                TeamNumber = tmpData[0] * 256 + tmpData[1]
            };

            long timeNumber = tmpData[2] * 16777216 + tmpData[3] * 65536 + tmpData[4] * 256 + tmpData[5];
            teamBlock.InitTime = Helpers.ConvertFromUnixTimestamp(timeNumber);

            teamBlock.TeamMask = tmpData[6] * 256 + tmpData[7];

            timeNumber = tmpData[8] * 16777216 + tmpData[9] * 65536 + tmpData[10] * 256 + tmpData[11];
            teamBlock.LastCheckTime = Helpers.ConvertFromUnixTimestamp(timeNumber);

            teamBlock.DumpSize = tmpData[12];

            byte chipType = 0;
            for (byte i = 0; i < RfidContainer.ChipTypes.SystemIds.Count; i++)
                if (RfidContainer.ChipTypes.SystemIds[i] == Dump[DUMP_HEADER_SIZE + 14])
                {
                    chipType = i;
                    break;
                }

            teamBlock.ChipDump = new RfidContainer(chipType);
            var chip = new byte[RfidContainer.ChipTypes.PageSizes[chipType] * RfidContainer.ChipTypes.PageSize];
            for (var i = 0; i < chip.Length; i++) chip[i] = Dump[DUMP_HEADER_SIZE + i];
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
                var tmpData = new byte[BytesPerRow];
                for (uint i = 0; i < BytesPerRow; i++) tmpData[i] = Dump[rowFrom * BytesPerRow + i];

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
                    if (dumpSize * 4 + 16 >= BytesPerRow)
                        dumpSize = 0;

                    var result = "Team #" + teamNum +
                       ", InitTime: " + Helpers.DateToString(initTime) +
                       ", Mask: " + Helpers.ConvertMaskToString(maskNumber) +
                       ", Last check: " + Helpers.DateToString(lastCheck) +
                       ", Dump size: " + dumpSize + ", ";

                    //1st byte of time
                    var todayByte = (byte)(Helpers.ConvertToUnixTimestamp(DateTime.Now.ToUniversalTime()) >> 24);

                    result += "Dump data: ";
                    var page = 4;
                    while (page < dumpSize + 4)
                    {
                        switch (page)
                        {
                            // page 4+(0..1): UID
                            // page 4+3: chip type
                            case 4:
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
                                    break;
                                }
                            // page 4+4: team#, chip type, fw ver.
                            case 7:
                                {
                                    var tagSize = "Ntag";
                                    if (tmpData[page * 4 + 2] == 0x12)
                                        tagSize += "213(144 bytes)";
                                    else if (tmpData[page * 4 + 2] == 0x3e)
                                        tagSize += "215(496 bytes)";
                                    else if (tmpData[page * 4 + 2] == 0x6d)
                                        tagSize += "216(872 bytes)";
                                    result += tagSize + ", ";
                                    break;
                                }
                            // page 4+5: init time
                            case 8:
                                {
                                    var m = (uint)(tmpData[page * 4 + 0] * 256 + tmpData[page * 4 + 1]);
                                    result += "Team #" + m + ", " + "Ntag" + tmpData[page * 4 + 2] + ", fw v." + tmpData[page * 4 + 3] + ", ";
                                    break;
                                }
                            // page 4+6: team mask
                            case 9:
                                {
                                    todayByte = tmpData[page * 4 + 0];
                                    long m = tmpData[page * 4 + 0] * 16777216 + tmpData[page * 4 + 1] * 65536 + tmpData[page * 4 + 2] * 256 +
                                             tmpData[page * 4 + 3];
                                    var t = Helpers.ConvertFromUnixTimestamp(m);
                                    result += "InitTime: " + Helpers.DateToString(t) + ", ";
                                    break;
                                }
                            // page4+7: reserved
                            case 10:
                                {
                                    byte[] mask = { tmpData[page * 4 + 0], tmpData[page * 4 + 1] };
                                    result += "Mask: " + Helpers.ConvertMaskToString(mask) + ", ";
                                    break;
                                }
                            // page4+8...: marks
                            case 11:
                                ;
                                break;
                            default:
                                {
                                    if (page > 11)
                                    {
                                        long m = todayByte * 16777216 + tmpData[page * 4 + 1] * 65536 +
                                                 tmpData[page * 4 + 2] * 256 + tmpData[page * 4 + 3];
                                        var t = Helpers.ConvertFromUnixTimestamp(m);
                                        result += "KP#" + tmpData[page * 4 + 0] + ", " + Helpers.DateToString(t) + ", ";
                                    }

                                    break;
                                }
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
