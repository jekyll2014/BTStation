using System;
using System.Collections.Generic;
using System.Text;

namespace RfidStationControl
{
    public class ProtocolParser
    {
        public UInt16 MaxPacketLength
        {
            get { return _maxPacketLength; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _maxPacketLength = value;
                _uartBuffer = new byte[_maxPacketLength];
            }
        }

        private UInt16 _maxPacketLength;
        private byte[] _uartBuffer;
        private UInt32 _uartBufferPosition;
        private bool _receivingData;
        private byte _packageId;
        private readonly object _serialReceiveThreadLock = new object();
        private DateTime _receiveStartTime = DateTime.Now.ToUniversalTime().ToUniversalTime();
        public ulong ReceiveTimeOut = 2000;
        public byte StationNumber;

        public ProtocolParser(UInt16 maxPacketLength = 255, byte stationNumber = 0)
        {
            this.MaxPacketLength = maxPacketLength;
            _uartBuffer = new byte[maxPacketLength];
            this.StationNumber = stationNumber;
        }

        //коды команд
        private static class Command
        {
            public const byte SET_MODE = 0x80;
            public const byte SET_TIME = 0x81;
            public const byte RESET_STATION = 0x82;
            public const byte GET_STATUS = 0x83;
            public const byte INIT_CHIP = 0x84;
            public const byte GET_LAST_TEAMS = 0x85;
            public const byte GET_TEAM_RECORD = 0x86;
            public const byte READ_CARD_PAGE = 0x87;
            public const byte UPDATE_TEAM_MASK = 0x88;
            public const byte WRITE_CARD_PAGE = 0x89;
            public const byte READ_FLASH = 0x8a;
            public const byte WRITE_FLASH = 0x8b;
            public const byte ERASE_FLASH_SECTOR = 0x8c;
            public const byte GET_CONFIG = 0x8d;
            public const byte SET_V_COEFF = 0x8e;
            public const byte SET_GAIN = 0x8f;
            public const byte SET_CHIP_TYPE = 0x90;
            public const byte SET_TEAM_FLASH_SIZE = 0x91;
            public const byte SET_FLASH_BLOCK_SIZE = 0x92;
            public const byte SET_BT_NAME = 0x93;
            public const byte SET_BT_PIN = 0x94;
            public const byte SET_BATTERY_LIMIT = 0x95;
            public const byte SCAN_TEAMS = 0x96;
            public const byte SEND_BT_COMMAND = 0x97;
            public const byte GET_LAST_ERRORS = 0x98;
            public const byte SET_AUTOREPORT = 0x99;
        }

        //минимальные размеры данных для команд header[6]+crc[1]+params[?]
        public static class CommandDataLength
        {
            public const byte SET_MODE = 7 + 1;
            public const byte SET_TIME = 7 + 6;
            public const byte RESET_STATION = 7 + 7;
            public const byte GET_STATUS = 7 + 0;
            public const byte INIT_CHIP = 7 + 4;
            public const byte GET_LAST_TEAMS = 7 + 0;
            public const byte GET_TEAM_RECORD = 7 + 2;
            public const byte READ_CARD_PAGE = 7 + 2;
            public const byte UPDATE_TEAM_MASK = 7 + 8;
            public const byte WRITE_CARD_PAGE = 7 + 13;
            public const byte READ_FLASH = 7 + 6;
            public const byte WRITE_FLASH = 7 + 4;
            public const byte ERASE_FLASH_SECTOR = 7 + 2;
            public const byte GET_CONFIG = 7 + 0;
            public const byte SET_V_COEFF = 7 + 4;
            public const byte SET_GAIN = 7 + 1;
            public const byte SET_CHIP_TYPE = 7 + 1;
            public const byte SET_TEAM_FLASH_SIZE = 7 + 2;
            public const byte SET_FLASH_BLOCK_SIZE = 7 + 2;
            public const byte SET_BT_NAME = 7 + 1;
            public const byte SET_BT_PIN = 7 + 1;
            public const byte SET_BATTERY_LIMIT = 7 + 4;
            public const byte SCAN_TEAMS = 7 + 2;
            public const byte SEND_BT_COMMAND = 7 + 1;
            public const byte GET_LAST_ERRORS = 7 + 0;
            public const byte SET_AUTOREPORT = 7 + 1;
        }

        //коды ответов станции
        public static class Reply
        {
            public const byte SET_MODE = 0x90;
            public const byte SET_TIME = 0x91;
            public const byte RESET_STATION = 0x92;
            public const byte GET_STATUS = 0x93;
            public const byte INIT_CHIP = 0x94;
            public const byte GET_LAST_TEAMS = 0x95;
            public const byte GET_TEAM_RECORD = 0x96;
            public const byte READ_CARD_PAGE = 0x97;
            public const byte UPDATE_TEAM_MASK = 0x98;
            public const byte WRITE_CARD_PAGE = 0x99;
            public const byte READ_FLASH = 0x9a;
            public const byte WRITE_FLASH = 0x9b;
            public const byte ERASE_FLASH_SECTOR = 0x9c;
            public const byte GET_CONFIG = 0x9d;
            public const byte SET_V_COEFF = 0x9e;
            public const byte SET_GAIN = 0x9f;
            public const byte SET_CHIP_TYPE = 0xa0;
            public const byte SET_TEAM_FLASH_SIZE = 0xa1;
            public const byte SET_FLASH_BLOCK_SIZE = 0xa2;
            public const byte SET_BT_NAME = 0xa3;
            public const byte SET_BT_PIN = 0xa4;
            public const byte SET_BATTERY_LIMIT = 0xa5;
            public const byte SCAN_TEAMS = 0xa6;
            public const byte SEND_BT_COMMAND = 0xa7;
            public const byte GET_LAST_ERRORS = 0xa8;
            public const byte SET_AUTOREPORT = 0xa9;
        }

        //размеры данных для ответов
        public static class ReplyDataLength
        {
            public const byte SET_MODE = 1;
            public const byte SET_TIME = 5;
            public const byte RESET_STATION = 1;
            public const byte GET_STATUS = 15;
            public const byte INIT_CHIP = 13;
            public const byte GET_LAST_TEAMS = 1;
            public const byte GET_TEAM_RECORD = 14;
            public const byte READ_CARD_PAGE = 14;
            public const byte UPDATE_TEAM_MASK = 1;
            public const byte WRITE_CARD_PAGE = 1;
            public const byte READ_FLASH = 5;
            public const byte WRITE_FLASH = 2;
            public const byte ERASE_FLASH_SECTOR = 1;
            public const byte GET_CONFIG = 24;
            public const byte SET_V_COEFF = 1;
            public const byte SET_GAIN = 1;
            public const byte SET_CHIP_TYPE = 1;
            public const byte SET_TEAM_FLASH_SIZE = 1;
            public const byte SET_FLASH_BLOCK_SIZE = 1;
            public const byte SET_BT_NAME = 1;
            public const byte SET_BT_PIN = 1;
            public const byte SET_BATTERY_LIMIT = 1;
            public const byte SCAN_TEAMS = 1;
            public const byte SEND_BT_COMMAND = 1;
            public const byte GET_LAST_ERRORS = 1;
            public const byte SET_AUTOREPORT = 1;
        }

        public class ReplyData
        {
            public byte[] Packet;
            public DateTime ReplyTimeStamp;
            public byte ReplyId;
            public byte StationNumber;
            public UInt16 DataLength;
            public byte ReplyCode;
            public byte ErrorCode;

            internal DateTime Time1;
            internal DateTime Time2;
            internal float Float1;
            internal float Float2;
            internal UInt32 Long1;
            internal UInt16 Int1;
            internal UInt16 Int2;
            internal UInt16 Int3;
            internal byte[] ByteArray1;
            internal byte[] ByteArray2;
            internal UInt16[] IntArray;
            internal byte Byte1;
            internal byte Byte2;
            internal byte Byte3;
            internal byte Byte4;
            internal bool Bool1;

            public string Message;

            public override string ToString()
            {
                string result = "<< " + Helpers.ConvertByteArrayToHex(Packet) + "\r\n";
                result += "Station#: " + StationNumber + "\r\n";

                ReplyStrings.TryGetValue(ReplyCode, out string commandValue);
                result += "Command reply: " + commandValue + "\r\n";
                if (ErrorCode != 0)
                {
                    ErrorCodes.TryGetValue(ErrorCode, out string errorValue);
                    result += "\r\nError#: " + errorValue + "\r\n";
                }

                return result;
            }

            public class setTimeReply
            {
                public DateTime Time;

                public setTimeReply(ReplyData data)
                {
                    Time = data.Time1;
                }
                public override string ToString()
                {
                    string result = "Time set to: " + Helpers.DateToString(Time) + "\r\n";
                    return result;
                }
            }

            public class getStatusReply
            {
                public DateTime Time;
                public UInt16 MarksNumber;
                public DateTime LastMarkTime;
                public UInt16 BatteryLevel;
                public UInt16 Temperature;

                public getStatusReply(ReplyData data)
                {
                    Time = data.Time1;
                    MarksNumber = data.Int1;
                    LastMarkTime = data.Time2;
                    BatteryLevel = data.Int2;
                    Temperature = data.Int3;
                }

                public override string ToString()
                {
                    string result = "Current time: " + Helpers.DateToString(Time) + "\r\n";
                    result += "Marks number: " + MarksNumber + "\r\n";
                    result += "Last mark time: " + Helpers.DateToString(LastMarkTime) + "\r\n";
                    result += "Battery: " + "ADC=" + BatteryLevel + "\r\n";
                    result += "Temperature: " + Temperature + "\r\n";
                    return result;
                }
            }

            public class initChipReply
            {
                public DateTime InitTime;
                public byte[] Uid;

                public initChipReply(ReplyData data)
                {
                    InitTime = data.Time1;
                    Uid = data.ByteArray1;
                }

                public override string ToString()
                {
                    string result = "Init. time: " + Helpers.DateToString(InitTime) + "\r\n";
                    result += "UID: " + Helpers.ConvertByteArrayToHex(Uid);
                    return result;
                }
            }

            public class getLastTeamsReply
            {
                public UInt16[] TeamsList;

                public getLastTeamsReply(ReplyData data)
                {
                    TeamsList = data.IntArray;
                }

                public override string ToString()
                {
                    string result = "Latest teams:\r\n";
                    for (int i = 0; i < TeamsList?.Length; i++)
                    {
                        if (TeamsList[i] > 0)
                        {
                            result += "\t" + TeamsList[i].ToString() + "\r\n";
                        }
                    }
                    return result;
                }
            }

            public class getTeamRecordReply
            {
                public UInt16 TeamNumber;
                public DateTime InitTime;
                public UInt16 Mask;
                public DateTime LastMarkTime;
                public UInt16 DumpSize;

                public getTeamRecordReply(ReplyData data)
                {
                    TeamNumber = data.Int1;
                    InitTime = data.Time1;
                    Mask = data.Int2;
                    LastMarkTime = data.Time2;
                    DumpSize = data.Byte1;
                }

                public override string ToString()
                {
                    string result = "Team#: " + TeamNumber + "\r\n";
                    result += "Init. time: " + Helpers.DateToString(InitTime) + "\r\n";
                    result += "Team mask: " + Helpers.ConvertMaskToString(Mask) + "\r\n";
                    result += "Last mark time: " + Helpers.DateToString(LastMarkTime) + "\r\n";
                    result += "Dump pages: " + DumpSize + "\r\n";
                    return result;
                }
            }

            public class readCardPageReply
            {
                public byte[] Uid;
                public byte startPage;
                public byte[] PagesData;

                public readCardPageReply(ReplyData data)
                {
                    Uid = data.ByteArray1;
                    startPage = data.Byte1;
                    PagesData = data.ByteArray2;
                }

                public override string ToString()
                {
                    string result = "UID: " + Helpers.ConvertByteArrayToHex(Uid) + "\r\n";
                    result += "Card data:\r\n";
                    UInt16 i = 0;
                    while ((i + 3) < PagesData.Length)
                    {
                        result += "\tpage #" + ((UInt16)(startPage + i / 4)).ToString() + ": ";
                        result += Helpers.ConvertByteArrayToHex(PagesData, i, 4) + "\r\n";
                        i += 4;
                    }

                    return result;
                }
            }

            public class readFlashReply
            {
                public UInt32 Address;
                public byte[] Data;

                public readFlashReply(ReplyData data)
                {
                    Address = data.Long1;
                    Data = data.ByteArray1;
                }

                public override string ToString()
                {
                    string result = "Read start address: " + Address + "\r\n";
                    result += "Flash data: " + Helpers.ConvertByteArrayToHex(Data) + "\r\n";
                    return result;
                }
            }

            public class writeFlashReply
            {
                public UInt16 BytesWritten;

                public writeFlashReply(ReplyData data)
                {
                    BytesWritten = data.Int1;
                }

                public override string ToString()
                {
                    string result = "Bytes written: " + BytesWritten + "\r\n";
                    return result;
                }
            }

            public class getConfigReply
            {
                public byte FwVersion;
                public byte Mode;
                public byte ChipTypeId;
                public UInt32 FlashSize;
                public float VoltageKoeff;
                public byte AntennaGain;
                public UInt16 TeamBlockSize;
                public UInt16 EraseBlockSize;
                public float BatteryLimit;
                public UInt16 MaxPacketLength;
                public bool AutoreportMode;

                public getConfigReply(ReplyData data)
                {
                    FwVersion = data.Byte1;
                    Mode = data.Byte2;
                    ChipTypeId = data.Byte3;
                    FlashSize = data.Long1;
                    VoltageKoeff = data.Float1;
                    AntennaGain = data.Byte4;
                    TeamBlockSize = data.Int1;
                    EraseBlockSize = data.Int2;
                    BatteryLimit = data.Float2;
                    MaxPacketLength = data.Int3;
                    AutoreportMode = data.Bool1;
                }

                public override string ToString()
                {
                    string result = "FW Version: " + FwVersion.ToString() + "\r\n";
                    result += "Mode: " + Mode + "\r\n";
                    result += "Chip type: " + ChipTypeId + "\r\n";
                    result += "Flash size: " + FlashSize + " байт\r\n";
                    result += "Voltage calculate coefficient: " + VoltageKoeff.ToString("F5") + "\r\n";
                    result += "Antenna gain: " + AntennaGain.ToString() + "\r\n";
                    result += "Team block size: " + TeamBlockSize.ToString() + "\r\n";
                    result += "Erase block size: " + EraseBlockSize.ToString() + "\r\n";
                    result += "Min. battery voltage: " + BatteryLimit.ToString("F3") + "\r\n";
                    result += "Max. packet length: " + MaxPacketLength.ToString() + "\r\n";
                    result += "Autoreport mode: " + AutoreportMode.ToString() + "\r\n";
                    return result;
                }
            }

            public class scanTeamsReply
            {
                public UInt16[] TeamsList;

                public scanTeamsReply(ReplyData data)
                {
                    TeamsList = data.IntArray;
                }

                public override string ToString()
                {
                    string result = "Saved teams:\r\n";
                    for (int i = 0; i < TeamsList?.Length; i++)
                    {
                        result += "\t" + TeamsList[i].ToString() + "\r\n";
                    }
                    return result;
                }
            }

            public class sendBtCommandReply
            {
                public string reply;

                public sendBtCommandReply(ReplyData data)
                {
                    reply = Helpers.ConvertByteArrayToString(data.ByteArray1);
                }

                public override string ToString()
                {
                    return reply;
                }
            }

            public class getLastErrorsReply
            {
                public byte[] errorsList;

                public getLastErrorsReply(ReplyData data)
                {
                    errorsList = data.ByteArray1;
                }

                public override string ToString()
                {
                    string result = "Latest errors:\r\n";
                    for (int i = 0; i < errorsList?.Length; i++)
                    {
                        if (errorsList[i] > 0)
                        {
                            ProcessingErrorCodes.TryGetValue(errorsList[i], out string errorValue);
                            result += "\t[" + errorsList[i].ToString() + "] " + errorValue + "\r\n";
                        }
                    }
                    return result;
                }
            }
        }

        //текстовые обозначения кодов команд
        public static readonly Dictionary<byte, string> CommandStrings = new Dictionary<byte, string>
        {
            { 0x80, "SET_MODE"},
            { 0x81, "SET_TIME"},
            { 0x82, "RESET_STATION"},
            { 0x83, "GET_STATUS"},
            { 0x84, "INIT_CHIP"},
            { 0x85, "GET_LAST_TEAMS"},
            { 0x86, "GET_TEAM_RECORD"},
            { 0x87, "READ_CARD_PAGE"},
            { 0x88, "UPDATE_TEAM_MASK"},
            { 0x89, "WRITE_CARD_PAGE"},
            { 0x8a, "READ_FLASH"},
            { 0x8b, "WRITE_FLASH"},
            { 0x8c, "ERASE_FLASH_SECTOR"},
            { 0x8d, "GET_CONFIG"},
            { 0x8e, "SET_V_COEFF"},
            { 0x8f, "SET_GAIN"},
            { 0x90, "SET_CHIP_TYPE"},
            { 0x91, "SET_TEAM_FLASH_SIZE"},
            { 0x92, "SET_FLASH_BLOCK_SIZE"},
            { 0x93, "SET_BT_NAME"},
            { 0x94, "SET_BT_PIN"},
            { 0x95, "SET_BATTERY_LIMIT"},
            { 0x96, "SCAN_TEAMS"},
            { 0x97, "SEND_BT_COMMAND"},
            { 0x98, "GET_LAST_ERRORS"},
            { 0x99, "SET_AUTOREPORT"},
        };

        //текстовые обозначения кодов ответов
        public static readonly Dictionary<byte, string> ReplyStrings = new Dictionary<byte, string>
        {
            { 0x90, "SET_MODE"},
            { 0x91, "SET_TIME"},
            { 0x92, "RESET_STATION"},
            { 0x93, "GET_STATUS"},
            { 0x94, "INIT_CHIP"},
            { 0x95, "GET_LAST_TEAMS"},
            { 0x96, "GET_TEAM_RECORD"},
            { 0x97, "READ_CARD_PAGE"},
            { 0x98, "UPDATE_TEAM_MASK"},
            { 0x99, "WRITE_CARD_PAGE"},
            { 0x9a, "READ_FLASH"},
            { 0x9b, "WRITE_FLASH"},
            { 0x9c, "ERASE_FLASH_SECTOR"},
            { 0x9d, "GET_CONFIG"},
            { 0x9e, "SET_V_COEFF"},
            { 0x9f, "SET_GAIN"},
            { 0xa0, "SET_CHIP_TYPE"},
            { 0xa1, "SET_TEAM_FLASH_SIZE"},
            { 0xa2, "SET_FLASH_BLOCK_SIZE"},
            { 0xa3, "SET_BT_NAME"},
            { 0xa4, "SET_BT_PIN"},
            { 0xa5, "SET_BATTERY_LIMIT"},
            { 0xa6, "SCAN_TEAMS"},
            { 0xa7, "SEND_BT_COMMAND"},
            { 0xa8, "GET_LAST_ERRORS"},
            { 0xa9, "SET_AUTOREPORT"},
        };

        // текстовые обозначения кодов ошибок станции
        public static readonly Dictionary<byte, string> ErrorCodes = new Dictionary<byte, string>
        {
            {0, "OK"},
            {1, "WRONG_STATION"},
            {2, "RFID_READ_ERROR"},
            {3, "RFID_WRITE_ERROR"},
            {4, "LOW_INIT_TIME"},
            {5, "WRONG_CHIP"},
            {6, "NO_CHIP"},
            {7, "BUFFER_OVERFLOW"},
            {8, "WRONG_DATA"},
            {9, "WRONG_UID"},
            {10, "WRONG_TEAM"},
            {11, "NO_DATA"},
            {12, "WRONG_COMMAND"},
            {13, "ERASE_ERROR"},
            {14, "WRONG_CHIP_TYPE"},
            {15, "WRONG_MODE"},
            {16, "WRONG_SIZE"},
            {17, "WRONG_FW_VERSION"},
            {18, "WRONG_PACKET_LENGTH"},
            {19, "FLASH_READ_ERROR"},
            {20, "FLASH_WRITE_ERROR"},
            {21, "EEPROM_READ_ERROR"},
            {22, "EEPROM_WRITE_ERROR"},
            {23, "BT_ERROR"},
            {24, "PACKET_LENGTH"},
        };

        // текстовые обозначения кодов ошибок
        public static readonly Dictionary<byte, string> ProcessingErrorCodes = new Dictionary<byte, string>
        {
            //коды ошибок STARTUP
            {50, "STARTUP: incorrect station number in EEPROM"},
            {51, "STARTUP: incorrect station mode in EEPROM"},
            {52, "STARTUP: incorrect Vkoeff in EEPROM"},
            {53, "STARTUP: incorrect gain in EEPROM"},
            {54, "STARTUP: incorrect chip type in EEPROM"},
            {55, "STARTUP: incorrect team block size in EEPROM"},
            {56, "STARTUP: incorrect erase block size in EEPROM"},
            {57, "STARTUP: incorrect battery limit in EEPROM"},
            {58, "STARTUP: incorrect autoreport mode in EEPROM"},
            //коды ошибок UART
            {60, "UART: receive timeout"},
            {61, "UART: incorrect packet length"},
            {62, "UART: CRC incorrect"},
            {63, "UART: unexpected byte"},
            {64, "UART: incorrect station number"},
            //коды ошибок обработки чипа
            {70, "CARD PROCESSING: error reading chip"},
            {71, "CARD PROCESSING: wrong hw chip type"},
            {72, "CARD PROCESSING: wrong sw chip type"},
            {73, "CARD PROCESSING: wrong fw. version"},
            {74, "CARD PROCESSING: chip init time is due"},
            {75, "CARD PROCESSING: wrong chip number"},
            {76, "CARD PROCESSING: error writing to chip"},
            {77, "CARD PROCESSING: chip already checked"},
            {78, "CARD PROCESSING: error finding free page"},
            {79, "CARD PROCESSING: error saving dump"},
            {80, "CARD PROCESSING: error sending autoreport"},
        };

        //header = 0xFE[0] + packetID[1] + station#[2] + cmd#[3] + lenHigh[4] + lenLow[5] + data[6-...] + crc
        private static class PacketBytes
        {
            public const byte HEADER_BYTE = 0;
            public const byte PACKET_ID_BYTE = 1;
            public const byte STATION_NUMBER_BYTE = 2;
            public const byte COMMAND_BYTE = 3;
            public const byte DATA_LENGTH_HIGH_BYTE = 4;
            public const byte DATA_LENGTH_LOW_BYTE = 5;
            public const byte DATA_START_BYTE = 6;
        }

        public List<ReplyData> _repliesList = new List<ReplyData>();

        public bool AddData(List<byte> data)
        {
            bool isReady = false;
            lock (_serialReceiveThreadLock)
            {
                if (_receivingData && DateTime.Now.ToUniversalTime().Subtract(_receiveStartTime).TotalMilliseconds > ReceiveTimeOut)
                {
                    _uartBufferPosition = 0;
                    _uartBuffer = new byte[MaxPacketLength];
                    _receivingData = false;
                }
                isReady = ParsePackage(data);
            }

            return isReady;
        }

        private bool ParsePackage(List<byte> input)
        {
            StringBuilder result = new StringBuilder();
            List<byte> unrecognizedBytes = new List<byte>();
            bool completed = false;
            while (input.Count > 0)
            {
                //0 byte = FE
                if (_uartBufferPosition == PacketBytes.HEADER_BYTE && input[0] == 0xfe)
                {
                    _uartBuffer[_uartBufferPosition] = input[0];
                    _receiveStartTime = DateTime.Now.ToUniversalTime();
                    _receivingData = true;
                    _uartBufferPosition++;
                }
                //1st byte = ID, station#, length, command, and data
                else if (_receivingData)
                {
                    _uartBuffer[_uartBufferPosition] = input[0];
                    // incorrect length
                    if (_uartBufferPosition == PacketBytes.DATA_LENGTH_LOW_BYTE &&
                        (UInt16)(_uartBuffer[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + _uartBuffer[PacketBytes.DATA_LENGTH_LOW_BYTE]) > MaxPacketLength - PacketBytes.DATA_START_BYTE)
                    {
                        result.Append("\r\nIncorrect message length: ");
                        result.Append(Helpers.ConvertByteArrayToHex(_uartBuffer, _uartBufferPosition));
                        _receivingData = false;
                        _uartBufferPosition = 0;
                        _uartBuffer = new byte[MaxPacketLength];
                        input.RemoveAt(0);
                        continue;
                    }

                    // packet is received
                    if (_uartBufferPosition >= PacketBytes.DATA_START_BYTE + (UInt16)(_uartBuffer[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + _uartBuffer[PacketBytes.DATA_LENGTH_LOW_BYTE]))
                    {
                        //crc matching
                        byte crc = Helpers.CrcCalc(_uartBuffer, PacketBytes.PACKET_ID_BYTE,
                            _uartBufferPosition - 1);
                        if (_uartBuffer[_uartBufferPosition] == crc)
                        {
                            // incorrect station number
                            if (_uartBuffer[PacketBytes.STATION_NUMBER_BYTE] != StationNumber && _uartBuffer[PacketBytes.COMMAND_BYTE] != Reply.GET_STATUS && _uartBuffer[PacketBytes.COMMAND_BYTE] != Reply.GET_CONFIG)
                            {
                                result.Append("\r\nIncorrect station number: ");
                                result.Append(Helpers.ConvertByteArrayToHex(_uartBuffer, _uartBufferPosition));
                                /*_receivingData = false;
                                _uartBufferPosition = 0;
                                _uartBuffer = new byte[MaxPacketLength];
                                input.RemoveAt(0);
                                continue;*/
                            }

                            ReplyData parsedReply = ParseReply(_uartBuffer);
                            if (parsedReply != null)
                            {
                                completed = true;
                                parsedReply.ReplyTimeStamp = _receiveStartTime;
                                _repliesList.Add(parsedReply);
                            }

                            _receivingData = true;
                            _uartBufferPosition = 0;
                            _uartBuffer = new byte[MaxPacketLength];
                            input.RemoveAt(0);
                            continue;
                        }

                        result.Append("\r\nCRC not correct: " + _uartBuffer[_uartBufferPosition].ToString("X2") +
                                " instead of " + crc.ToString("X2") + "\r\n");
                        _receivingData = false;
                        _uartBufferPosition = 0;
                        _uartBuffer = new byte[MaxPacketLength];
                        input.RemoveAt(0);
                        continue;
                    }

                    _uartBufferPosition++;
                }
                else
                {
                    if (_uartBufferPosition > 0)
                    {
                        List<byte> error = new List<byte>();
                        for (int i = 0; i < _uartBufferPosition; i++)
                        {
                            error.Add(_uartBuffer[i]);
                        }
                        error.Add(input[0]);
                        _receivingData = false;
                        _uartBufferPosition = 0;
                        _uartBuffer = new byte[MaxPacketLength];
                        //SetText("\r\nIncorrect bytes: [" + Accessory.ConvertByteArrayToHex(error.ToArray()) + "]\r\n");
                    }
                    else
                        unrecognizedBytes.Add(input[0]);
                }
                input.RemoveAt(0);
            }

            if (unrecognizedBytes.Count > 0)
            {
                if (Helpers.PrintableByteArray(unrecognizedBytes.ToArray()))
                {
                    result.Append("Comment: " + Helpers.ConvertByteArrayToString(unrecognizedBytes.ToArray()) + "\r\n");
                }
                else
                {
                    result.Append("Comment: " + Helpers.ConvertByteArrayToHex(unrecognizedBytes.ToArray()) + "\r\n");
                }
            }

            if (result.Length > 0)
            {
                _repliesList.Add(new ReplyData()
                {
                    Message = result.ToString()
                });
            }
            //SetText(result.ToString());
            return completed;
        }

        private byte[] generateCommand(byte[] command, int staNum = -1)
        {
            if (staNum < 0 || staNum > 0xff) staNum = StationNumber;
            command[PacketBytes.HEADER_BYTE] = 0xFE;
            command[PacketBytes.PACKET_ID_BYTE] = _packageId++;
            command[PacketBytes.STATION_NUMBER_BYTE] = (byte)staNum;

            if (command.Length > MaxPacketLength)
                return null;

            command[PacketBytes.DATA_LENGTH_HIGH_BYTE] = (byte)((command.Length - PacketBytes.DATA_START_BYTE - 1) >> 8);
            command[PacketBytes.DATA_LENGTH_LOW_BYTE] = (byte)((command.Length - PacketBytes.DATA_START_BYTE - 1) & 0x00ff);
            command[command.Length - 1] = Helpers.CrcCalc(
                command,
                PacketBytes.PACKET_ID_BYTE,
                (UInt16)(command.Length - 2));
            return command;
        }

        #region Generate commands

        public byte[] setMode(byte stationMode)
        {
            byte[] setMode = new byte[CommandDataLength.SET_MODE];
            setMode[PacketBytes.COMMAND_BYTE] = Command.SET_MODE;

            //0: новый номер режима
            setMode[PacketBytes.DATA_START_BYTE] = stationMode;
            return generateCommand(setMode);
        }

        public byte[] setTime(DateTime time)
        {
            byte[] setTime = new byte[CommandDataLength.SET_TIME];
            setTime[PacketBytes.COMMAND_BYTE] = Command.SET_TIME;

            //0-5: дата и время[yy.mm.dd hh: mm:ss]
            string timeStr = Helpers.DateToString(time.ToUniversalTime());
            byte[] date = Helpers.DateStringToByteArray(timeStr);
            for (int i = 0; i < 6; i++)
                setTime[PacketBytes.DATA_START_BYTE + i] = date[i];

            return generateCommand(setTime);
        }

        public byte[] resetStation(UInt16 chipsNumber, DateTime lastMarkTime, byte newStationNumber)
        {
            byte[] resetStation = new byte[CommandDataLength.RESET_STATION];
            resetStation[PacketBytes.COMMAND_BYTE] = Command.RESET_STATION;

            /*0-1: кол-во отмеченных карт (для сверки)
            2-5: время последней отметки unixtime(для сверки)
            6: новый номер станции*/
            resetStation[PacketBytes.DATA_START_BYTE + 0] = (byte)(chipsNumber >> 8);
            resetStation[PacketBytes.DATA_START_BYTE + 1] = (byte)(chipsNumber & 0x00ff);

            long tmpTime = Helpers.ConvertToUnixTimestamp(lastMarkTime);
            resetStation[PacketBytes.DATA_START_BYTE + 2] = (byte)((tmpTime & 0xFF000000) >> 24);
            resetStation[PacketBytes.DATA_START_BYTE + 3] = (byte)((tmpTime & 0x00FF0000) >> 16);
            resetStation[PacketBytes.DATA_START_BYTE + 4] = (byte)((tmpTime & 0x0000FF00) >> 8);
            resetStation[PacketBytes.DATA_START_BYTE + 5] = (byte)(tmpTime & 0x000000FF);

            resetStation[PacketBytes.DATA_START_BYTE + 6] = newStationNumber;

            return generateCommand(resetStation);
        }

        public byte[] getStatus()
        {
            byte[] getStatus = new byte[CommandDataLength.GET_STATUS];
            getStatus[PacketBytes.COMMAND_BYTE] = Command.GET_STATUS;

            return generateCommand(getStatus, 0);
        }

        public byte[] initChip(UInt16 teamNumber, UInt16 mask)
        {
            byte[] initChip = new byte[CommandDataLength.INIT_CHIP];
            initChip[PacketBytes.COMMAND_BYTE] = Command.INIT_CHIP;

            /*0-1: номер команды
            2-3: маска участников*/
            initChip[PacketBytes.DATA_START_BYTE] = (byte)(teamNumber >> 8);
            initChip[PacketBytes.DATA_START_BYTE + 1] = (byte)teamNumber;

            initChip[PacketBytes.DATA_START_BYTE + 2] = (byte)(mask >> 8);
            initChip[PacketBytes.DATA_START_BYTE + 3] = (byte)(mask & 0x00ff);

            return generateCommand(initChip);
        }

        public byte[] getLastTeam()
        {
            byte[] getLastTeam = new byte[CommandDataLength.GET_LAST_TEAMS];
            getLastTeam[PacketBytes.COMMAND_BYTE] = Command.GET_LAST_TEAMS;

            return generateCommand(getLastTeam);
        }

        public byte[] getTeamRecord(UInt16 teamNumber)
        {
            byte[] getTeamRecord = new byte[CommandDataLength.GET_TEAM_RECORD];
            getTeamRecord[PacketBytes.COMMAND_BYTE] = Command.GET_TEAM_RECORD;

            //0-1: какую запись
            getTeamRecord[PacketBytes.DATA_START_BYTE] = (byte)(teamNumber >> 8);
            getTeamRecord[PacketBytes.DATA_START_BYTE + 1] = (byte)(teamNumber & 0x00ff);

            return generateCommand(getTeamRecord);
        }

        public byte[] readCardPage(byte fromPage, byte toPage)
        {
            byte[] readCardPage = new byte[CommandDataLength.READ_CARD_PAGE];
            readCardPage[PacketBytes.COMMAND_BYTE] = Command.READ_CARD_PAGE;

            //0: с какой страницу карты
            readCardPage[PacketBytes.DATA_START_BYTE] = fromPage;
            //1: по какую страницу карты включительно
            readCardPage[PacketBytes.DATA_START_BYTE + 1] = toPage;

            return generateCommand(readCardPage);
        }

        public byte[] updateTeamMask(UInt16 teamNumber, DateTime issueTimeStr, UInt16 mask)
        {
            byte[] updateTeamMask = new byte[CommandDataLength.UPDATE_TEAM_MASK];
            updateTeamMask[PacketBytes.COMMAND_BYTE] = Command.UPDATE_TEAM_MASK;

            /*0-1: номер команды
            2-5: время выдачи чипа
            6-7: маска участников*/
            updateTeamMask[PacketBytes.DATA_START_BYTE] = (byte)(teamNumber >> 8);
            updateTeamMask[PacketBytes.DATA_START_BYTE + 1] = (byte)teamNumber;

            //card issue time - 4 byte
            long issueTime = Helpers.ConvertToUnixTimestamp(issueTimeStr);
            updateTeamMask[PacketBytes.DATA_START_BYTE + 2] = (byte)((issueTime & 0xFF000000) >> 24);
            updateTeamMask[PacketBytes.DATA_START_BYTE + 3] = (byte)((issueTime & 0x00FF0000) >> 16);
            updateTeamMask[PacketBytes.DATA_START_BYTE + 4] = (byte)((issueTime & 0x0000FF00) >> 8);
            updateTeamMask[PacketBytes.DATA_START_BYTE + 5] = (byte)(issueTime & 0x000000FF);

            updateTeamMask[PacketBytes.DATA_START_BYTE + 6] = (byte)(mask >> 8);
            updateTeamMask[PacketBytes.DATA_START_BYTE + 7] = (byte)(mask & 0x00ff);

            return generateCommand(updateTeamMask);
        }

        public byte[] writeCardPage(byte[] uid, byte pageNumber, byte[] data)
        {
            byte[] writeCardPage = new byte[CommandDataLength.WRITE_CARD_PAGE];
            writeCardPage[PacketBytes.COMMAND_BYTE] = Command.WRITE_CARD_PAGE;

            //0-7: UID чипа
            //8: номер страницы
            //9-12: данные из страницы карты (4 байта)
            if (uid.Length != 8)
                return new byte[0];
            for (int i = 0; i <= 7; i++)
                writeCardPage[PacketBytes.DATA_START_BYTE + i] = uid[i];

            writeCardPage[PacketBytes.DATA_START_BYTE + 8] = pageNumber;

            if (data.Length != 4)
                return new byte[0];
            for (int i = 0; i <= 3; i++)
                writeCardPage[PacketBytes.DATA_START_BYTE + 9 + i] = data[i];

            return generateCommand(writeCardPage);
        }

        public byte[] readFlash(UInt32 fromAddr, UInt16 readLength)
        {
            if (readLength > MaxPacketLength - CommandDataLength.READ_FLASH)
                return new byte[0];
            byte[] readFlash = new byte[CommandDataLength.READ_FLASH];
            readFlash[PacketBytes.COMMAND_BYTE] = Command.READ_FLASH;

            //0-3: адрес начала чтения
            //4-5: размер блока

            readFlash[PacketBytes.DATA_START_BYTE] = (byte)((fromAddr & 0xFF000000) >> 24);
            readFlash[PacketBytes.DATA_START_BYTE + 1] = (byte)((fromAddr & 0x00FF0000) >> 16);
            readFlash[PacketBytes.DATA_START_BYTE + 2] = (byte)((fromAddr & 0x0000FF00) >> 8);
            readFlash[PacketBytes.DATA_START_BYTE + 3] = (byte)(fromAddr & 0x000000FF);

            readFlash[PacketBytes.DATA_START_BYTE + 4] = (byte)((readLength & 0xFF00) >> 8);
            readFlash[PacketBytes.DATA_START_BYTE + 5] = (byte)(readLength & 0x00FF);

            return generateCommand(readFlash);
        }

        public byte[] writeFlash(UInt32 startAddress, byte[] data)
        {
            if (data.Length > MaxPacketLength - CommandDataLength.WRITE_FLASH)
                return new byte[0];
            byte[] writeFlash = new byte[CommandDataLength.WRITE_FLASH + data.Length];
            writeFlash[PacketBytes.COMMAND_BYTE] = Command.WRITE_FLASH;

            //0-3: адрес начала записи
            //4...: данные для записи
            writeFlash[PacketBytes.DATA_START_BYTE] = (byte)((startAddress & 0xFF000000) >> 24);
            writeFlash[PacketBytes.DATA_START_BYTE + 1] = (byte)((startAddress & 0x00FF0000) >> 16);
            writeFlash[PacketBytes.DATA_START_BYTE + 2] = (byte)((startAddress & 0x0000FF00) >> 8);
            writeFlash[PacketBytes.DATA_START_BYTE + 3] = (byte)(startAddress & 0x000000FF);

            for (byte i = 0; i < data.Length; i++)
            {
                writeFlash[PacketBytes.DATA_START_BYTE + 4 + i] = data[i];
            }

            return generateCommand(writeFlash);
        }

        public byte[] eraseTeamFlash(UInt16 teamNum)
        {
            byte[] eraseFlashSector = new byte[CommandDataLength.ERASE_FLASH_SECTOR];
            eraseFlashSector[PacketBytes.COMMAND_BYTE] = Command.ERASE_FLASH_SECTOR;

            //0-1: какой сектор
            eraseFlashSector[PacketBytes.DATA_START_BYTE] = (byte)(teamNum >> 8);
            eraseFlashSector[PacketBytes.DATA_START_BYTE + 1] = (byte)(teamNum & 0x00ff);

            return generateCommand(eraseFlashSector);
        }

        public byte[] getConfig()
        {
            byte[] getConfig = new byte[CommandDataLength.GET_CONFIG];
            getConfig[PacketBytes.COMMAND_BYTE] = Command.GET_CONFIG;

            return generateCommand(getConfig, 0);
        }

        public byte[] setVCoeff(float koeff)
        {
            byte[] setKoeff = new byte[CommandDataLength.SET_V_COEFF];
            setKoeff[PacketBytes.COMMAND_BYTE] = Command.SET_V_COEFF;

            //0-3: коэффициент пересчета напряжения
            byte[] k = BitConverter.GetBytes(koeff);
            for (int i = 0; i < 4; i++)
            {
                setKoeff[PacketBytes.DATA_START_BYTE + i] = k[i];
            }

            return generateCommand(setKoeff);
        }

        public byte[] setGain(byte gain)
        {
            byte[] setGain = new byte[CommandDataLength.SET_GAIN];
            setGain[PacketBytes.COMMAND_BYTE] = Command.SET_GAIN;

            //0: новый коэфф.
            setGain[PacketBytes.DATA_START_BYTE] = gain;
            return generateCommand(setGain);
        }

        public byte[] setChipType(byte chipSystemId)
        {
            byte[] setChipType = new byte[CommandDataLength.SET_CHIP_TYPE];
            setChipType[PacketBytes.COMMAND_BYTE] = Command.SET_CHIP_TYPE;

            //0: новый тип чипа
            setChipType[PacketBytes.DATA_START_BYTE] = chipSystemId;
            return generateCommand(setChipType);
        }

        public byte[] setTeamFlashSize(UInt32 blockSize)
        {
            byte[] setTeamFlashSize = new byte[CommandDataLength.SET_TEAM_FLASH_SIZE];
            setTeamFlashSize[PacketBytes.COMMAND_BYTE] = Command.SET_TEAM_FLASH_SIZE;

            //0-1: новый размер блока команды
            setTeamFlashSize[PacketBytes.DATA_START_BYTE] = (byte)(blockSize >> 8);
            setTeamFlashSize[PacketBytes.DATA_START_BYTE + 1] = (byte)(blockSize & 0x00ff);
            return generateCommand(setTeamFlashSize);
        }

        public byte[] setEraseBlock(UInt32 blockSize)
        {
            byte[] setEraseBlockSize = new byte[CommandDataLength.SET_FLASH_BLOCK_SIZE];
            setEraseBlockSize[PacketBytes.COMMAND_BYTE] = Command.SET_FLASH_BLOCK_SIZE;

            //0-1: новый размер стираемого блока
            setEraseBlockSize[PacketBytes.DATA_START_BYTE] = (byte)(blockSize >> 8);
            setEraseBlockSize[PacketBytes.DATA_START_BYTE + 1] = (byte)(blockSize & 0x00ff);
            return generateCommand(setEraseBlockSize);
        }

        public byte[] SetBtName(string btName)
        {
            byte[] data = Encoding.ASCII.GetBytes(btName);
            if (data.Length > 32 || data.Length < 1)
                return new byte[0];
            ;

            byte[] setBtName = new byte[CommandDataLength.SET_BT_NAME + data.Length];
            setBtName[PacketBytes.COMMAND_BYTE] = Command.SET_BT_NAME;

            //0...: данные для записи
            for (byte i = 0; i < data.Length; i++)
            {
                setBtName[PacketBytes.DATA_START_BYTE + i] = data[i];
            }

            return generateCommand(setBtName);
        }

        public byte[] SetBtPin(string btPin)
        {
            byte[] data = Encoding.ASCII.GetBytes(btPin);
            if (data.Length > 16 || data.Length < 1)
                return new byte[0];
            ;

            byte[] setBtName = new byte[CommandDataLength.SET_BT_PIN + data.Length];
            setBtName[PacketBytes.COMMAND_BYTE] = Command.SET_BT_PIN;

            //0...: данные для записи
            for (byte i = 0; i < data.Length; i++)
            {
                setBtName[PacketBytes.DATA_START_BYTE + i] = data[i];
            }

            return generateCommand(setBtName);
        }

        public byte[] setBatteryLimit(float limit)
        {
            byte[] setBatteryLimit = new byte[CommandDataLength.SET_BATTERY_LIMIT];
            setBatteryLimit[PacketBytes.COMMAND_BYTE] = Command.SET_BATTERY_LIMIT;

            //0-3: коэффициент пересчета напряжения
            byte[] k = BitConverter.GetBytes(limit);
            for (int i = 0; i < 4; i++)
            {
                setBatteryLimit[PacketBytes.DATA_START_BYTE + i] = k[i];
            }

            return generateCommand(setBatteryLimit);
        }

        public byte[] scanTeams(UInt16 startTeamNumber)
        {
            byte[] scanTeams = new byte[CommandDataLength.SCAN_TEAMS];
            scanTeams[PacketBytes.COMMAND_BYTE] = Command.SCAN_TEAMS;

            //0-1: начальный номер команды
            scanTeams[PacketBytes.DATA_START_BYTE] = (byte)(startTeamNumber >> 8);
            scanTeams[PacketBytes.DATA_START_BYTE + 1] = (byte)(startTeamNumber & 0x00ff);
            return generateCommand(scanTeams);
        }

        public byte[] sendBtCommand(string btCommand)
        {
            byte[] data = Encoding.ASCII.GetBytes(btCommand);
            if (data.Length > MaxPacketLength - CommandDataLength.SEND_BT_COMMAND || data.Length < 1)
                return new byte[0];

            byte[] sendBtCommand = new byte[CommandDataLength.SEND_BT_COMMAND + data.Length];
            sendBtCommand[PacketBytes.COMMAND_BYTE] = Command.SEND_BT_COMMAND;

            //0...: команда
            for (byte i = 0; i < data.Length; i++)
            {
                sendBtCommand[PacketBytes.DATA_START_BYTE + i] = data[i];
            }

            return generateCommand(sendBtCommand);
        }

        public byte[] getLastErrors()
        {
            byte[] getLastErrors = new byte[CommandDataLength.GET_LAST_ERRORS];
            getLastErrors[PacketBytes.COMMAND_BYTE] = Command.GET_LAST_ERRORS;

            return generateCommand(getLastErrors);
        }

        public byte[] setAutoReport(bool autoreportMode)
        {
            byte[] setAutoReport = new byte[CommandDataLength.SET_AUTOREPORT];
            setAutoReport[PacketBytes.COMMAND_BYTE] = Command.SET_AUTOREPORT;

            //0: новый номер режима
            setAutoReport[PacketBytes.DATA_START_BYTE] = autoreportMode == true ? (byte)1 : (byte)0;
            return generateCommand(setAutoReport);
        }

        #endregion

        #region Parse replies

        private ReplyData ParseReply(byte[] data)
        {
            if (data.Length <= PacketBytes.DATA_START_BYTE)
            {
                return null;
            }

            ReplyData result = new ReplyData();
            result.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            result.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            result.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            result.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            result.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(result.DataLength + PacketBytes.DATA_START_BYTE + 1);
            result.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                result.Packet[i] = data[i];
            }

            if (result.DataLength == 1 && result.ErrorCode > 0)
                return result;

            switch (result.ReplyCode)
            {
                case Reply.SET_MODE:
                    if (result.DataLength == ReplyDataLength.SET_MODE)
                    {
                        result = reply_setMode(data);
                    }
                    break;
                case Reply.SET_TIME:
                    if (result.DataLength == ReplyDataLength.SET_TIME)
                    {
                        result = reply_setTime(data);
                    }
                    break;
                case Reply.RESET_STATION:
                    if (result.DataLength == ReplyDataLength.RESET_STATION)
                    {
                        result = reply_resetStation(data);
                    }
                    break;
                case Reply.GET_STATUS:
                    if (result.DataLength == ReplyDataLength.GET_STATUS)
                    {
                        result = reply_getStatus(data);
                    }
                    break;
                case Reply.INIT_CHIP:
                    if (result.DataLength == ReplyDataLength.INIT_CHIP)
                    {
                        result = reply_initChip(data);
                    }
                    break;
                case Reply.GET_LAST_TEAMS:
                    if (result.DataLength >= ReplyDataLength.GET_LAST_TEAMS)
                    {
                        result = reply_getLastTeams(data);
                    }
                    break;
                case Reply.GET_TEAM_RECORD:
                    if (result.DataLength == ReplyDataLength.GET_TEAM_RECORD)
                    {
                        result = reply_getTeamRecord(data);
                    }
                    break;
                case Reply.READ_CARD_PAGE:
                    if (result.DataLength >= ReplyDataLength.READ_CARD_PAGE)
                    {
                        result = reply_readCardPages(data);
                    }
                    break;
                case Reply.UPDATE_TEAM_MASK:
                    if (result.DataLength == ReplyDataLength.UPDATE_TEAM_MASK)
                    {
                        result = reply_updateTeamMask(data);
                    }
                    break;
                case Reply.WRITE_CARD_PAGE:
                    if (result.DataLength == ReplyDataLength.WRITE_CARD_PAGE)
                    {
                        result = reply_writeCardPage(data);
                    }
                    break;
                case Reply.READ_FLASH:
                    if (result.DataLength >= ReplyDataLength.READ_FLASH)
                    {
                        result = reply_readFlash(data);
                    }
                    break;
                case Reply.WRITE_FLASH:
                    if (result.DataLength == ReplyDataLength.WRITE_FLASH)
                    {
                        result = reply_writeFlash(data);
                    }
                    break;
                case Reply.ERASE_FLASH_SECTOR:
                    if (result.DataLength == ReplyDataLength.ERASE_FLASH_SECTOR)
                    {
                        result = reply_eraseTeamFlash(data);
                    }
                    break;
                case Reply.GET_CONFIG:
                    if (result.DataLength == ReplyDataLength.GET_CONFIG)
                    {
                        result = reply_getConfig(data);
                    }
                    break;
                case Reply.SET_V_COEFF:
                    if (result.DataLength == ReplyDataLength.SET_V_COEFF)
                    {
                        result = reply_setVCoeff(data);
                    }
                    break;
                case Reply.SET_GAIN:
                    if (result.DataLength == ReplyDataLength.SET_GAIN)
                    {
                        result = reply_setGain(data);
                    }
                    break;
                case Reply.SET_CHIP_TYPE:
                    if (result.DataLength == ReplyDataLength.SET_CHIP_TYPE)
                    {
                        result = reply_setChipType(data);
                    }
                    break;
                case Reply.SET_TEAM_FLASH_SIZE:
                    if (result.DataLength == ReplyDataLength.SET_TEAM_FLASH_SIZE)
                    {
                        result = reply_setTeamFlashSize(data);
                    }
                    break;
                case Reply.SET_FLASH_BLOCK_SIZE:
                    if (result.DataLength == ReplyDataLength.SET_FLASH_BLOCK_SIZE)
                    {
                        result = reply_setFlashBlockSize(data);
                    }
                    break;
                case Reply.SET_BT_NAME:
                    if (result.DataLength == ReplyDataLength.SET_BT_NAME)
                    {
                        result = reply_setBtName(data);
                    }
                    break;
                case Reply.SET_BT_PIN:
                    if (result.DataLength == ReplyDataLength.SET_BT_PIN)
                    {
                        result = reply_setBtPin(data);
                    }
                    break;
                case Reply.SET_BATTERY_LIMIT:
                    if (result.DataLength == ReplyDataLength.SET_BATTERY_LIMIT)
                    {
                        result = reply_setBatteryLimit(data);
                    }
                    break;
                case Reply.SCAN_TEAMS:
                    if (result.DataLength >= ReplyDataLength.SCAN_TEAMS)
                    {
                        result = reply_scanTeams(data);
                    }
                    break;
                case Reply.SEND_BT_COMMAND:
                    if (result.DataLength >= ReplyDataLength.SEND_BT_COMMAND)
                    {
                        result = reply_sendBtCommand(data);
                    }
                    break;
                case Reply.GET_LAST_ERRORS:
                    if (result.DataLength >= ReplyDataLength.GET_LAST_ERRORS)
                    {
                        result = reply_getLastErrors(data);
                    }
                    break;
                case Reply.SET_AUTOREPORT:
                    if (result.DataLength == ReplyDataLength.SET_AUTOREPORT)
                    {
                        result = reply_setAutoreport(data);
                    }
                    break;

                default:
                    ReplyStrings.TryGetValue(data[PacketBytes.COMMAND_BYTE], out string commandValue);
                    result.Message = "Incorrect reply: " + commandValue + "\r\n";
                    break;
            }

            return result;
        }

        private ReplyData reply_setMode(byte[] data)
        {
            //0: код ошибки
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            return reply;
        }

        private ReplyData reply_setTime(byte[] data)
        {
            //0: код ошибки
            //1-4: текущее время
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            long t = data[PacketBytes.DATA_START_BYTE + 1] * 16777216 + data[PacketBytes.DATA_START_BYTE + 2] * 65536 +
                     data[PacketBytes.DATA_START_BYTE + 3] * 256 + data[PacketBytes.DATA_START_BYTE + 4];
            reply.Time1 = Helpers.ConvertFromUnixTimestamp(t);

            return reply;
        }

        private ReplyData reply_resetStation(byte[] data)
        {
            //0: код ошибки
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            return reply;
        }

        private ReplyData reply_getStatus(byte[] data)
        {
            //0: код ошибки
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            //1-4: текущее время
            long t = data[PacketBytes.DATA_START_BYTE + 1] * 16777216 +
                data[PacketBytes.DATA_START_BYTE + 2] * 65536 +
                data[PacketBytes.DATA_START_BYTE + 3] * 256 +
                data[PacketBytes.DATA_START_BYTE + 4];
            reply.Time1 = Helpers.ConvertFromUnixTimestamp(t);

            //5-6: количество отметок на станции
            reply.Int1 = (UInt16)(data[PacketBytes.DATA_START_BYTE + 5] * 256 +
                data[PacketBytes.DATA_START_BYTE + 6]);

            //7-10: время последней отметки на станции
            t = data[PacketBytes.DATA_START_BYTE + 7] * 16777216 +
                data[PacketBytes.DATA_START_BYTE + 8] * 65536 +
                data[PacketBytes.DATA_START_BYTE + 9] * 256 +
                data[PacketBytes.DATA_START_BYTE + 10];
            reply.Time2 = Helpers.ConvertFromUnixTimestamp(t);

            //11-12: напряжение батареи в условных единицах[0..1023] ~ [0..1.1В]
            reply.Int2 = (UInt16)(data[PacketBytes.DATA_START_BYTE + 11] * 256 +
                data[PacketBytes.DATA_START_BYTE + 12]);

            //13-14: температура чипа DS3231 (чуть выше окружающей среды)
            reply.Int3 = (UInt16)(data[PacketBytes.DATA_START_BYTE + 13] * 256 +
                data[PacketBytes.DATA_START_BYTE + 14]);

            return reply;
        }

        private ReplyData reply_initChip(byte[] data)
        {
            //0: код ошибки
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            // 1-4: время инициализации
            long t = data[PacketBytes.DATA_START_BYTE + 1] * 16777216 + data[PacketBytes.DATA_START_BYTE + 2] * 65536 +
                     data[PacketBytes.DATA_START_BYTE + 3] * 256 + data[PacketBytes.DATA_START_BYTE + 4];
            reply.Time1 = Helpers.ConvertFromUnixTimestamp(t);

            // 5-12: UID карточки
            reply.ByteArray1 = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                reply.ByteArray1[i] = data[PacketBytes.DATA_START_BYTE + 5 + i];
            }

            return reply;
        }

        private ReplyData reply_getLastTeams(byte[] data)
        {
            //0: код ошибки
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            //1-2: номер 1й команды
            //3-4: номер 2й команды
            //...
            //(n - 1) - n: номер последней команды
            List<UInt16> a = new List<UInt16>();
            for (int i = 1; i < reply.DataLength - 1; i++)
            {
                UInt16 n = (UInt16)(data[PacketBytes.DATA_START_BYTE + i] * 256);
                i++;
                n += data[PacketBytes.DATA_START_BYTE + i];
                if (n > 0)
                {
                    a.Add(n);
                }
            }
            reply.IntArray = a.ToArray();

            return reply;
        }

        private ReplyData reply_getTeamRecord(byte[] data)
        {
            // 0: код ошибки

            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            // 1-2[Uint16]: номер команды
            reply.Int1 = (UInt16)(data[PacketBytes.DATA_START_BYTE + 1] * 256 + data[PacketBytes.DATA_START_BYTE + 2]);

            // 3 - 6[Uint32]: время инициализации
            long t = data[PacketBytes.DATA_START_BYTE + 3] * 16777216 + data[PacketBytes.DATA_START_BYTE + 4] * 65536 +
                     data[PacketBytes.DATA_START_BYTE + 5] * 256 + data[PacketBytes.DATA_START_BYTE + 6];
            reply.Time1 = Helpers.ConvertFromUnixTimestamp(t);

            // 7 - 8[Uint16]: маска команды
            reply.Int2 = (UInt16)(data[PacketBytes.DATA_START_BYTE + 7] * 256 + data[PacketBytes.DATA_START_BYTE + 8]);

            // 9 - 12[Uint32]: время последней отметки на станции
            t = data[PacketBytes.DATA_START_BYTE + 9] * 16777216 + data[PacketBytes.DATA_START_BYTE + 10] * 65536 +
                data[PacketBytes.DATA_START_BYTE + 11] * 256 + data[PacketBytes.DATA_START_BYTE + 12];
            reply.Time2 = Helpers.ConvertFromUnixTimestamp(t);

            // 13[byte]: счетчик сохраненных страниц
            reply.Byte1 = data[PacketBytes.DATA_START_BYTE + 13];
            // 13 - 14[Uint16]: счетчик сохраненных байт
            //reply.Int3 = (UInt16)(data[PacketBytes.DATA_START + 13] * 256 + data[PacketBytes.DATA_START + 14]);

            return reply;
        }

        private ReplyData reply_readCardPages(byte[] data)
        {
            //0: код ошибки
            //1-8: UID чипа
            //9: номер начальной страницы
            //10-...: данные из страниц карты(4 байта на страницу)
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            reply.ByteArray1 = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                reply.ByteArray1[i] = data[PacketBytes.DATA_START_BYTE + 1 + i];
            }

            reply.Byte1 = data[PacketBytes.DATA_START_BYTE + 9];
            reply.ByteArray2 = new byte[reply.DataLength - 10];
            for (int i = 10; i < reply.DataLength; i++)
            {
                reply.ByteArray2[i - 10] = data[PacketBytes.DATA_START_BYTE + i];
            }

            return reply;
        }

        private ReplyData reply_updateTeamMask(byte[] data)
        {
            //0: код ошибки
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            return reply;
        }

        private ReplyData reply_writeCardPage(byte[] data)
        {
            //0: код ошибки
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            return reply;
        }

        private ReplyData reply_readFlash(byte[] data)
        {
            //0: код ошибки
            //1...: данные из флэша
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            reply.Long1 = (UInt32)(data[PacketBytes.DATA_START_BYTE + 1] * 16777216 +
                data[PacketBytes.DATA_START_BYTE + 2] * 65536 +
                data[PacketBytes.DATA_START_BYTE + 3] * 256 +
                data[PacketBytes.DATA_START_BYTE + 4]);

            reply.ByteArray1 = new byte[reply.DataLength - 5];
            for (int i = 0; i < reply.ByteArray1.Length; i++)
            {
                reply.ByteArray1[i] = data[PacketBytes.DATA_START_BYTE + 5 + i];
            }

            return reply;
        }

        private ReplyData reply_writeFlash(byte[] data)
        {
            //0: код ошибки
            //1...: данные из флэша
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            reply.Int1 = data[PacketBytes.DATA_START_BYTE + 1];

            return reply;
        }

        private ReplyData reply_eraseTeamFlash(byte[] data)
        {
            //0: код ошибки
            ReplyData reply = new ReplyData();
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            return reply;
        }

        private ReplyData reply_getConfig(byte[] data)
        {
            //0: код ошибки
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            //1: версия прошивки
            reply.Byte1 = data[PacketBytes.DATA_START_BYTE + 1];

            //2: номер режима
            reply.Byte2 = data[PacketBytes.DATA_START_BYTE + 2];

            //3: тип чипов (емкость разная, а распознать их программно можно только по ошибкам чтения "дальних" страниц)
            reply.Byte3 = data[PacketBytes.DATA_START_BYTE + 3];

            //4-7: емкость флэш-памяти
            reply.Long1 = (UInt32)(data[PacketBytes.DATA_START_BYTE + 4] * 16777216 +
                data[PacketBytes.DATA_START_BYTE + 5] * 65536 +
                data[PacketBytes.DATA_START_BYTE + 6] * 256 +
                data[PacketBytes.DATA_START_BYTE + 7]);

            //8-11: коэффициент пересчета напряжения (float, 4 bytes) - просто умножаешь коэффициент на полученное в статусе число и будет температура
            byte[] b =
            {
                data[PacketBytes.DATA_START_BYTE + 8],
                data[PacketBytes.DATA_START_BYTE + 9],
                data[PacketBytes.DATA_START_BYTE + 10],
                data[PacketBytes.DATA_START_BYTE + 11]
            };
            reply.Float1 = Helpers.FloatConversion(b);

            //12: коэфф. усиления антенны
            reply.Byte4 = data[PacketBytes.DATA_START_BYTE + 12];

            //13-14: размер блока хранения команды
            reply.Int1 = (UInt16)(data[PacketBytes.DATA_START_BYTE + 13] * 256 + data[PacketBytes.DATA_START_BYTE + 14]);

            //15-16: размер стираемого блока
            reply.Int2 = (UInt16)(data[PacketBytes.DATA_START_BYTE + 15] * 256 + data[PacketBytes.DATA_START_BYTE + 16]);

            //17-20: минимальное значение напряжения батареи
            b = new[]
            {
                data[PacketBytes.DATA_START_BYTE + 17],
                data[PacketBytes.DATA_START_BYTE + 18],
                data[PacketBytes.DATA_START_BYTE + 19],
                data[PacketBytes.DATA_START_BYTE + 20]
            };
            reply.Float2 = Helpers.FloatConversion(b);

            //21-22: максимальный размер пакета
            reply.Int3 = (UInt16)(data[PacketBytes.DATA_START_BYTE + 21] * 256 + data[PacketBytes.DATA_START_BYTE + 22]);

            //23: автоотчет о новой отсканированной команде
            reply.Bool1 = (data[PacketBytes.DATA_START_BYTE + 23] != 0);

            return reply;
        }

        private ReplyData reply_setVCoeff(byte[] data)
        {
            //0: код ошибки
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            return reply;
        }

        private ReplyData reply_setGain(byte[] data)
        {
            //0: код ошибки
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            return reply;
        }

        private ReplyData reply_setChipType(byte[] data)
        {
            //0: код ошибки
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            return reply;
        }

        private ReplyData reply_setTeamFlashSize(byte[] data)
        {
            //0: код ошибки
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            return reply;
        }

        private ReplyData reply_setFlashBlockSize(byte[] data)
        {
            //0: код ошибки
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            return reply;
        }

        private ReplyData reply_setBtName(byte[] data)
        {
            //0: код ошибки
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            return reply;
        }

        private ReplyData reply_setBtPin(byte[] data)
        {
            //0: код ошибки
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            return reply;
        }

        private ReplyData reply_setBatteryLimit(byte[] data)
        {
            //0: код ошибки
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            return reply;
        }

        private ReplyData reply_scanTeams(byte[] data)
        {
            //0: код ошибки
            //1-2: номер 1й команды
            //3-4: номер 2й команды           
            //...	                        
            //(n - 1) - n: номер последней команды
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            reply.IntArray = new UInt16[(reply.DataLength - 1) / 2];
            for (int i = 1; i < reply.DataLength; i = i + 2)
            {
                reply.IntArray[i / 2] = (ushort)(data[PacketBytes.DATA_START_BYTE + i] * 256 + data[PacketBytes.DATA_START_BYTE + 1 + i]);
            }
            return reply;
        }

        private ReplyData reply_sendBtCommand(byte[] data)
        {
            //0: код ошибки
            //1-n: ответ BT модуля
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            reply.ByteArray1 = new byte[reply.DataLength - 1];
            for (int i = 0; i < reply.ByteArray1.Length; i++)
            {
                reply.ByteArray1[i] = data[PacketBytes.DATA_START_BYTE + i + 1];
            }
            return reply;
        }

        private ReplyData reply_getLastErrors(byte[] data)
        {
            //0: код ошибки
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            //1-2: номер 1й ошибки
            //3-4: номер 2й ошибки
            //...
            //(n - 1) - n: номер последней ошибки
            List<byte> a = new List<byte>();
            for (int i = 1; i <= reply.DataLength - 1; i++)
            {
                byte n = data[PacketBytes.DATA_START_BYTE + i];
                if (n > 0)
                {
                    a.Add(n);
                }
            }
            reply.ByteArray1 = a.ToArray();
            return reply;
        }

        private ReplyData reply_setAutoreport(byte[] data)
        {
            //0: код ошибки
            ReplyData reply = new ReplyData();
            reply.ReplyId = data[PacketBytes.PACKET_ID_BYTE];
            reply.StationNumber = data[PacketBytes.STATION_NUMBER_BYTE];
            reply.ReplyCode = data[PacketBytes.COMMAND_BYTE];
            reply.ErrorCode = data[PacketBytes.DATA_START_BYTE];
            reply.DataLength = (UInt16)(data[PacketBytes.DATA_LENGTH_HIGH_BYTE] * 256 + data[PacketBytes.DATA_LENGTH_LOW_BYTE]);
            byte packetLength = (byte)(reply.DataLength + PacketBytes.DATA_START_BYTE + 1);
            reply.Packet = new byte[packetLength];
            for (byte i = 0; i < packetLength; i++)
            {
                reply.Packet[i] = data[i];
            }

            return reply;
        }

        #endregion
    }
}
