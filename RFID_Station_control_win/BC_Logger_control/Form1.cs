// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using RFID_Station_control.Properties;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace RFID_Station_control
{
    public partial class Form1 : Form
    {
        private const int INPUT_CODE_PAGE = 866;
        private int _portSpeed = 38400;
        private const ulong _receiveTimeOut = 1000;
        private string _decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        private bool _receivingData;

        //private byte _packageId;
        //private DateTime _getStatusTime = DateTime.Now.ToUniversalTime();

        private readonly object _serialReceiveThreadLock = new object();
        private readonly object _serialSendThreadLock = new object();

        private volatile ushort _asyncFlag;
        private volatile bool _needMore;

        private TextLogger.TextLogger _logger;

        private enum DataDirection
        {
            Received,
            Sent,
            Info,
            Error
        }

        private readonly Dictionary<byte, string> _directions = new Dictionary<byte, string>
        {
            {(byte) DataDirection.Received, "<<"},
            {(byte) DataDirection.Sent, ">>"},
            {(byte) DataDirection.Info, ""},
            {(byte) DataDirection.Error, "!!"}
        };

        private readonly DateTime _receiveStartTime = DateTime.Now.ToUniversalTime().ToUniversalTime();

        private uint _selectedFlashSize = 4 * 1024 * 1024;
        private readonly uint _bytesPerRow = 1024;

        private static readonly Dictionary<string, long> FlashSizeLimit = new Dictionary<string, long>
        {
            {"32 kb", 32 * 1024},
            {"64 kb", 64 * 1024},
            {"128 kb", 128 * 1024},
            {"256 kb", 256 * 1024},
            {"512 kb", 512 * 1024},
            {"1 Mb", 1024 * 1024},
            {"2 Mb", 2048 * 1024},
            {"4 Mb", 4096 * 1024},
            {"8 Mb", 8192 * 1024}
        };

        private FlashContainer _stationFlash;

        private byte _selectedChipType = RfidContainer.ChipTypes.Types["NTAG215"];
        private RfidContainer _rfidCard;

        private TeamsContainer _teams;

        public static ProtocolParser Parser;

        #region COM_port_handling

        private void Button_refresh_Click(object sender, EventArgs e)
        {
            comboBox_portName.Items.Clear();
            comboBox_portName.Items.Add("None");
            var ports = SerialPort.GetPortNames();
            foreach (var portname in ports)
                comboBox_portName.Items.Add(portname); //добавить порт в список

            if (comboBox_portName.Items.Count == 1)
            {
                comboBox_portName.SelectedIndex = 0;
                button_openPort.Enabled = false;
            }
            else
            {
                comboBox_portName.SelectedIndex = comboBox_portName.Items.Count - 1;
            }


            if (ports.Length == 0)
            {
                textBox_terminal.Text += "ERROR: No COM ports exist\n\r";
            }
            else
            {
                var portNames = Helpers.BuildPortNameHash(ports);
                foreach (string s in portNames.Keys) textBox_terminal.Text += "\n\r" + portNames[s] + ": " + s + "\n\r";
            }
        }

        private void Button_openPort_Click(object sender, EventArgs e)
        {
            if (comboBox_portName.SelectedIndex != 0)
            {
                serialPort1.PortName = comboBox_portName.Text;
                serialPort1.BaudRate = _portSpeed;
                serialPort1.DataBits = 8;
                serialPort1.Parity = Parity.None;
                serialPort1.StopBits = StopBits.One;
                serialPort1.Handshake = Handshake.None;
                serialPort1.ReadTimeout = 500;
                serialPort1.WriteTimeout = 500;
                try
                {
                    serialPort1.Open();
                }
                catch (Exception ex)
                {
                    _logger.AddText(
                        "Error opening port " + serialPort1.PortName + ": " + ex.Message + Environment.NewLine,
                        (byte)DataDirection.Error, DateTime.Now);
                }

                if (!serialPort1.IsOpen)
                    return;

                button_getLastTeam.Enabled = true;
                button_getTeamRecord.Enabled = true;
                button_updTeamMask.Enabled = true;
                button_initChip.Enabled = true;
                button_readChipPage.Enabled = true;
                button_writeChipPage.Enabled = true;

                button_setMode.Enabled = true;
                button_resetStation.Enabled = true;
                button_setTime.Enabled = true;
                button_getStatus.Enabled = true;
                button_getStatus.Enabled = true;
                button_readFlash.Enabled = true;
                button_writeFlash.Enabled = true;
                button_dumpTeams.Enabled = true;
                button_dumpChip.Enabled = true;
                button_dumpFlash.Enabled = true;
                button_eraseTeamFlash.Enabled = true;
                button_getConfig.Enabled = true;
                button_setKoeff.Enabled = true;
                button_setGain.Enabled = true;
                button_setChipType.Enabled = true;
                button_eraseChip.Enabled = true;
                button_unlockChip.Enabled = true;
                button_setTeamFlashSize.Enabled = true;
                button_setEraseBlock.Enabled = true;
                button_SetBtName.Enabled = true;
                button_SetBtPin.Enabled = true;
                button_setBatteryLimit.Enabled = true;
                button_getTeamsList.Enabled = true;
                button_sendBtCommand.Enabled = true;
                button_quickDump.Enabled = true;
                button_getLastErrors.Enabled = true;
                button_getConfig2.Enabled = true;
                button_setAutoReport.Enabled = true;
                button_setAuth.Enabled = true;
                button_setPwd.Enabled = true;
                button_setPack.Enabled = true;

                button_closePort.Enabled = true;
                button_openPort.Enabled = false;
                button_refresh.Enabled = false;
                comboBox_portName.Enabled = false;
                serialPort1.DtrEnable = true;
            }
        }

        private void Button_closePort_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
                try
                {
                    serialPort1.Close();
                }
                catch (Exception ex)
                {
                    _logger.AddText(
                        "Error closing port " + serialPort1.PortName + ": " + ex.Message + Environment.NewLine,
                        (byte)DataDirection.Error, DateTime.Now);
                }

            button_getLastTeam.Enabled = false;
            button_getTeamRecord.Enabled = false;
            button_updTeamMask.Enabled = false;
            button_initChip.Enabled = false;
            button_readChipPage.Enabled = false;
            button_writeChipPage.Enabled = false;

            button_setMode.Enabled = false;
            button_resetStation.Enabled = false;
            button_setTime.Enabled = false;
            button_getStatus.Enabled = false;
            button_getStatus.Enabled = false;
            button_readFlash.Enabled = false;
            button_writeFlash.Enabled = false;
            button_dumpTeams.Enabled = false;
            button_dumpChip.Enabled = false;
            button_dumpFlash.Enabled = false;
            button_eraseTeamFlash.Enabled = false;
            button_getConfig.Enabled = false;
            button_setKoeff.Enabled = false;
            button_setGain.Enabled = false;
            button_setChipType.Enabled = false;
            button_eraseChip.Enabled = false;
            button_unlockChip.Enabled = false;
            button_setTeamFlashSize.Enabled = false;
            button_setEraseBlock.Enabled = false;
            button_SetBtName.Enabled = false;
            button_SetBtPin.Enabled = false;
            button_setBatteryLimit.Enabled = false;
            button_getTeamsList.Enabled = false;
            button_sendBtCommand.Enabled = false;
            button_quickDump.Enabled = false;
            button_getLastErrors.Enabled = false;
            button_getConfig2.Enabled = false;
            button_setAutoReport.Enabled = false;
            button_setAuth.Enabled = false;
            button_setPwd.Enabled = false;
            button_setPack.Enabled = false;

            button_closePort.Enabled = false;
            button_openPort.Enabled = true;
            button_refresh.Enabled = true;
            comboBox_portName.Enabled = true;
        }

        //rewrite to validate packet runtime
        private void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (_serialReceiveThreadLock)
            {
                if (_receivingData && DateTime.Now.ToUniversalTime().Subtract(_receiveStartTime).TotalMilliseconds >
                    _receiveTimeOut)
                    _receivingData = false;

                var input = new List<byte>();
                while (serialPort1.BytesToRead > 0)
                    try
                    {
                        var c = serialPort1.ReadByte();
                        if (c != -1)
                            input.Add((byte)c);
                    }
                    catch (Exception ex)
                    {
                        _logger.AddText("COM port read error: " + ex + Environment.NewLine,
                            (byte)DataDirection.Error, DateTime.Now);
                    }

                Parser.AddData(input);
                if (Parser._repliesList.Count > 0)
                {
                    foreach (var reply in Parser._repliesList)
                    {
                        // command reply from station
                        if (reply.ReplyCode != 0)
                        {
                            _logger.AddText(reply.ToString(),
                                (byte)DataDirection.Received, DateTime.Now);

                            if (reply.ErrorCode == 0)
                            {
                                if (reply.ReplyCode == ProtocolParser.Reply.SET_TIME)
                                    Reply_setTime(reply);
                                else if (reply.ReplyCode == ProtocolParser.Reply.GET_STATUS)
                                    Reply_getStatus(reply);
                                else if (reply.ReplyCode == ProtocolParser.Reply.INIT_CHIP)
                                    Reply_initChip(reply);
                                else if (reply.ReplyCode == ProtocolParser.Reply.GET_LAST_TEAMS)
                                    Reply_getLastTeams(reply);
                                else if (reply.ReplyCode == ProtocolParser.Reply.GET_TEAM_RECORD)
                                    Reply_getTeamRecord(reply);
                                else if (reply.ReplyCode == ProtocolParser.Reply.READ_CARD_PAGE)
                                    Reply_readCardPages(reply);
                                else if (reply.ReplyCode == ProtocolParser.Reply.READ_FLASH)
                                    Reply_readFlash(reply);
                                else if (reply.ReplyCode == ProtocolParser.Reply.WRITE_FLASH)
                                    Reply_writeFlash(reply);
                                else if (reply.ReplyCode == ProtocolParser.Reply.GET_CONFIG)
                                    Reply_getConfig(reply);
                                else if (reply.ReplyCode == ProtocolParser.Reply.SCAN_TEAMS)
                                    Reply_scanTeams(reply);
                                else if (reply.ReplyCode == ProtocolParser.Reply.SEND_BT_COMMAND)
                                    Reply_sendBtCommand(reply);
                                else if (reply.ReplyCode == ProtocolParser.Reply.GET_LAST_ERRORS)
                                    Reply_getLastErrors(reply);
                            }
                        }
                        // text message from station
                        else
                        {
                            _logger.AddText(reply.Message,
                                (byte)DataDirection.Received, DateTime.Now);
                        }

                        _asyncFlag--;
                    }

                    Parser._repliesList.Clear();
                }
            }
        }

        private void SerialPort1_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            _logger.AddText("COM port error: " + e + Environment.NewLine,
                (byte)DataDirection.Error, DateTime.Now);
        }

        private void SendCommand(byte[] command)
        {
            if (!serialPort1.IsOpen)
            {
                Button_closePort_Click(this, EventArgs.Empty);
                return;
            }

            lock (_serialSendThreadLock)
            {
                if (command == null)
                {
                    _logger.AddText(Environment.NewLine + "Error generating command data" + Environment.NewLine,
                        (byte)DataDirection.Error, DateTime.Now);
                    return;
                }

                //_getStatusTime = DateTime.Now.ToUniversalTime();
                try
                {
                    serialPort1.Write(command, 0, command.Length);
                }
                catch (Exception e)
                {
                    _logger.AddText(Environment.NewLine + "COM port write error: " + e + Environment.NewLine,
                        (byte)DataDirection.Error, DateTime.Now);
                    return;
                }

                _logger.AddText(Helpers.ConvertByteArrayToHex(command),
                    (byte)DataDirection.Sent,
                    DateTime.Now);
            }
        }

        #endregion

        #region Generate commands

        private void Button_setMode_Click(object sender, EventArgs e)
        {
            //0: новый номер режима
            if (!StationSettings.StationMode.TryGetValue(comboBox_mode.SelectedItem.ToString(), out var mode))
                return;

            var setMode = Parser.SetMode(mode);
            SendCommand(setMode);
        }

        private void Button_setTime_Click(object sender, EventArgs e)
        {
            //0-5: дата и время[yy.mm.dd hh: mm:ss]
            var tmpTime = Helpers.DateStringToUnixTime(textBox_setTime.Text);
            var timeToSet = Helpers.ConvertFromUnixTimestamp(tmpTime);

            if (checkBox_autoTime.Checked)
            {
                timeToSet = DateTime.Now;
                textBox_setTime.Text = Helpers.DateToString(timeToSet);
            }

            var setTime = Parser.SetTime(timeToSet);
            SendCommand(setTime);
        }

        private void Button_resetStation_Click(object sender, EventArgs e)
        {
            //0-1: кол-во отмеченных карт (для сверки)
            ushort.TryParse(textBox_checkedChips.Text, out var chipsNumber);

            //2-5: время последней отметки unixtime(для сверки)
            var tmpTime = Helpers.DateStringToUnixTime(textBox_lastCheck.Text);
            var lastCheck = Helpers.ConvertFromUnixTimestamp(tmpTime);

            //6: новый номер станции
            byte.TryParse(textBox_newStationNumber.Text, out var newStationNumber);

            var resetStation = Parser.ResetStation(chipsNumber, lastCheck, newStationNumber);
            SendCommand(resetStation);
        }

        private void Button_getStatus_Click(object sender, EventArgs e)
        {
            var getStatus = Parser.GetStatus();
            SendCommand(getStatus);
        }

        private void Button_initChip_Click(object sender, EventArgs e)
        {
            /*0-1: номер команды
            2-3: маска участников*/
            ushort.TryParse(textBox_initTeamNum.Text, out var teamNumber);

            ushort mask = 0;
            byte j = 0;
            for (var i = 15; i >= 0; i--)
            {
                if (textBox_initMask.Text[i] == '1')
                    mask = (ushort)Helpers.SetBit(mask, j);
                else
                    mask = (ushort)Helpers.ClearBit(mask, j);
                j++;
            }

            var initChip = Parser.InitChip(teamNumber, mask);
            SendCommand(initChip);
        }

        private void Button_getLastTeam_Click(object sender, EventArgs e)
        {
            var getLastTeam = Parser.GetLastTeam();
            SendCommand(getLastTeam);
        }

        private void Button_getTeamRecord_Click(object sender, EventArgs e)
        {
            //0-1: какую запись
            ushort.TryParse(textBox_TeamNumber.Text, out var teamNumber);
            var getTeamRecord = Parser.GetTeamRecord(teamNumber);
            SendCommand(getTeamRecord);
        }

        private void Button_readCardPage_Click(object sender, EventArgs e)
        {
            //0: с какой страницу карты
            byte.TryParse(
                textBox_readChipPage.Text.Substring(0, textBox_readChipPage.Text.IndexOf("-", StringComparison.Ordinal))
                    .Trim(), out var fromPage);
            //1: по какую страницу карты включительно
            byte.TryParse(textBox_readChipPage.Text
                .Substring(textBox_readChipPage.Text.IndexOf("-", StringComparison.Ordinal) + 1)
                .Trim(), out var toPage);

            var readCardPage = Parser.ReadCardPage(fromPage, toPage);
            SendCommand(readCardPage);
        }

        private void Button_updateTeamMask_Click(object sender, EventArgs e)
        {
            /*0-1: номер команды
            2-5: время выдачи чипа
            6-7: маска участников*/
            ushort.TryParse(textBox_TeamNumber.Text, out var teamNumber);

            //card issue time - 4 byte
            //textBox_issueTime.Text
            var tmpTime = Helpers.DateStringToUnixTime(textBox_issueTime.Text);
            var issueTime = Helpers.ConvertFromUnixTimestamp(tmpTime);

            ushort mask = 0;
            byte j = 0;
            for (var i = 15; i >= 0; i--)
            {
                if (textBox_teamMask.Text[i] == '1')
                    mask = (ushort)Helpers.SetBit(mask, j);
                else
                    mask = (ushort)Helpers.ClearBit(mask, j);
                j++;
            }

            var updateTeamMask = Parser.UpdateTeamMask(teamNumber, issueTime, mask);
            SendCommand(updateTeamMask);
        }

        private void Button_writeCardPage_Click(object sender, EventArgs e)
        {
            //0-7: UID чипа
            var uid = Helpers.ConvertHexToByteArray(textBox_uid.Text);
            if (uid.Length != 8)
                return;

            //8: номер страницы
            byte.TryParse(textBox_writeChipPage.Text, out var pageNumber);

            //9-12: данные из страницы карты (4 байта)
            var data = Helpers.ConvertHexToByteArray(textBox_data.Text);
            if (data.Length != 4)
                return;

            var writeCardPage = Parser.WriteCardPage(uid, pageNumber, data);
            SendCommand(writeCardPage);
        }

        private void Button_readFlash_Click(object sender, EventArgs e)
        {
            //0-3: адрес начала чтения
            uint.TryParse(textBox_readFlashAddress.Text, out var fromAddr);

            //4-5: размер блока
            ushort.TryParse(textBox_readFlashLength.Text, out var readLength);

            var readFlash = Parser.ReadFlash(fromAddr, readLength);
            SendCommand(readFlash);
        }

        private void Button_writeFlash_Click(object sender, EventArgs e)
        {
            //0-3: адрес начала записи
            uint.TryParse(textBox_writeAddr.Text, out var startAddress);

            //4...: данные для записи
            var data = Helpers.ConvertHexToByteArray(textBox_flashData.Text);

            var writeFlash = Parser.WriteFlash(startAddress, data);
            SendCommand(writeFlash);
        }

        private void Button_eraseTeamFlash_Click(object sender, EventArgs e)
        {
            //0-1: какой сектор
            ushort.TryParse(textBox_TeamNumber.Text, out var teamNumber);

            var eraseTeamFlash = Parser.EraseTeamFlash(teamNumber);
            SendCommand(eraseTeamFlash);
        }

        private void Button_getConfig_Click(object sender, EventArgs e)
        {
            var getConfig = Parser.GetConfig();
            SendCommand(getConfig);
        }

        private void Button_setVCoeff_Click(object sender, EventArgs e)
        {
            //0-3: коэффициент пересчета напряжения
            textBox_koeff.Text =
                textBox_koeff.Text.Replace(".", _decimalSeparator);
            textBox_koeff.Text =
                textBox_koeff.Text.Replace(",", _decimalSeparator);

            if (float.TryParse(textBox_koeff.Text, out StationSettings.VoltageCoefficient))
            {
                textBox_koeff.Text = StationSettings.VoltageCoefficient.ToString();
                var setKoeff = Parser.SetVCoeff(StationSettings.VoltageCoefficient);
                SendCommand(setKoeff);
            }
        }

        private void Button_setGain_Click(object sender, EventArgs e)
        {
            //0: новый коэфф.
            if (!StationSettings.Gain.TryGetValue(comboBox_setGain.SelectedItem.ToString(), out var gainValue))
                return;
            var setGain = Parser.SetGain(gainValue);
            SendCommand(setGain);
        }

        private void Button_setChipType_Click(object sender, EventArgs e)
        {
            //0: новый тип чипа
            var newChipType = RfidContainer.ChipTypes.SystemIds[_selectedChipType];
            var setChipType = Parser.SetChipType(newChipType);
            SendCommand(setChipType);
        }

        private void Button_setTeamFlashSize_Click(object sender, EventArgs e)
        {
            //0-1: новый размер блока команды
            ushort.TryParse(textBox_teamFlashSize.Text, out StationSettings.TeamBlockSize);

            var setTeamFlashSize = Parser.SetTeamFlashSize(StationSettings.TeamBlockSize);
            SendCommand(setTeamFlashSize);
        }

        private void Button_setEraseBlock_Click(object sender, EventArgs e)
        {
            //0-1: новый размер стираемого блока
            ushort.TryParse(textBox_eraseBlock.Text, out StationSettings.EraseBlockSize);

            var setEraseBlockSize = Parser.SetEraseBlock(StationSettings.EraseBlockSize);
            SendCommand(setEraseBlockSize);
        }

        private void Button_SetBtName_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_BtName.Text))
                return;
            //0...: данные для записи
            var setBtName = Parser.SetBtName(textBox_BtName.Text);
            SendCommand(setBtName);
        }

        private void Button_SetBtPin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_BtPin.Text))
                return;
            //0...: данные для записи
            var setBtName = Parser.SetBtPin(textBox_BtPin.Text);
            SendCommand(setBtName);
        }

        private void Button_setBatteryLimit_Click(object sender, EventArgs e)
        {
            //0-3: коэффициент пересчета напряжения
            textBox_setBatteryLimit.Text =
                textBox_setBatteryLimit.Text.Replace(".", _decimalSeparator);
            textBox_setBatteryLimit.Text =
                textBox_setBatteryLimit.Text.Replace(",", _decimalSeparator);
            if (float.TryParse(textBox_setBatteryLimit.Text, out StationSettings.BatteryLimit))
            {
                textBox_setBatteryLimit.Text = StationSettings.BatteryLimit.ToString();
                var setBatteryLimit = Parser.SetBatteryLimit(StationSettings.BatteryLimit);
                SendCommand(setBatteryLimit);
            }
        }

        private void Button_getTeamsList_Click(object sender, EventArgs e)
        {
            //0-1: начальный номер команды
            ushort.TryParse(textBox_TeamNumber.Text, out var teamNumber);

            var scanTeams = Parser.ScanTeams(teamNumber);
            SendCommand(scanTeams);
        }

        private void Button_sendBtCommand_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_sendBtCommand.Text))
                return;
            //0...: команда
            var sendBtCommand = Parser.SendBtCommand(textBox_sendBtCommand.Text + Environment.NewLine);
            SendCommand(sendBtCommand);
        }

        private void Button_getLastErrors_Click(object sender, EventArgs e)
        {
            var geLastErrors = Parser.GetLastErrors();
            SendCommand(geLastErrors);
        }

        private void Button_setAutoReport_Click(object sender, EventArgs e)
        {
            //0: новый режим автоответа
            var setMode = Parser.SetAutoReport(checkBox_AutoReport.Checked);
            SendCommand(setMode);
        }

        private void Button_setAuth_Click(object sender, EventArgs e)
        {
            //0: новый режим авторизации
            var setAuth = Parser.SetAuth(checkBox_setAuth.Checked);
            SendCommand(setAuth);
        }

        private void Button_setPwd_Click(object sender, EventArgs e)
        {
            //0..3: ключ авторизации
            var data = Helpers.ConvertHexToByteArray(textBox_setPwd.Text);

            var setPwd = Parser.SetPwd(data);
            SendCommand(setPwd);
        }

        private void Button_setPack_Click(object sender, EventArgs e)
        {
            //0..1: ответ авторизации
            var data = Helpers.ConvertHexToByteArray(textBox_setPack.Text);

            var setPack = Parser.SetPack(data);
            SendCommand(setPack);
        }
        #endregion

        #region Parse replies

        private void Reply_setTime(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            //1-4: текущее время
            var replyDetails =
                new ProtocolParser.ReplyData.SetTimeReply(reply);
            _logger.AddText(replyDetails.ToString(), (byte)DataDirection.Info, TextLogger.TextLogger.TimeFormat.None);
        }

        private void Reply_getStatus(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            //1-4: текущее время
            //5-6: количество отметок на станции
            //7-10: время последней отметки на станции
            //11-12: напряжение батареи в условных единицах[0..1023] ~ [0..1.1В]
            //13-14: температура чипа DS3231 (чуть выше окружающей среды)

            var replyDetails =
                new ProtocolParser.ReplyData.GetStatusReply(reply);
            _logger.AddText(replyDetails.ToString(), (byte)DataDirection.Info, DateTime.Now,
                TextLogger.TextLogger.TimeFormat.None);
            _logger.AddText(
                "Battery voltage: " + (replyDetails.BatteryLevel * StationSettings.VoltageCoefficient).ToString("F3") + " V." +
                Environment.NewLine, (byte)DataDirection.Info);

            Invoke((MethodInvoker)delegate
           {
               StationSettings.Number = reply.StationNumber;
               Parser.StationNumber = StationSettings.Number;
               textBox_stationNumber.Text = StationSettings.Number.ToString();
           });
        }

        private void Reply_initChip(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            // убрать 1-7: UID чипа

            var replyDetails =
                new ProtocolParser.ReplyData.InitChipReply(reply);
            _logger.AddText(replyDetails.ToString(), (byte)DataDirection.Info, DateTime.Now,
                TextLogger.TextLogger.TimeFormat.None);
        }

        private void Reply_getLastTeams(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            //1-2: номер 1й команды
            //3-4: номер 2й команды
            //...
            //(n - 1) - n: номер последней команды
            var replyDetails =
                new ProtocolParser.ReplyData.GetLastTeamsReply(reply);
            _logger.AddText(replyDetails.ToString(), (byte)DataDirection.Info, DateTime.Now,
                TextLogger.TextLogger.TimeFormat.None);
        }

        private void Reply_getTeamRecord(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            //1: данные отметившейся команды
            var replyDetails =
                new ProtocolParser.ReplyData.GetTeamRecordReply(reply);
            _logger.AddText(replyDetails.ToString(), (byte)DataDirection.Info, DateTime.Now,
                TextLogger.TextLogger.TimeFormat.None);

            var tmpTeam = new TeamData
            {
                TeamNumber = replyDetails.TeamNumber,
                InitTime = replyDetails.InitTime,
                TeamMask = replyDetails.Mask,
                LastCheckTime = replyDetails.LastMarkTime,
                DumpSize = replyDetails.DumpSize
            };
            _teams.Add(tmpTeam);
        }

        private void Reply_readCardPages(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            //1-7: UID чипа
            //8-11: данные из страницы карты(4 байта)
            var replyDetails =
                new ProtocolParser.ReplyData.ReadCardPageReply(reply);
            _logger.AddText(replyDetails.ToString(), (byte)DataDirection.Info, DateTime.Now,
                TextLogger.TextLogger.TimeFormat.None);

            _rfidCard.AddPages(replyDetails.startPage, replyDetails.PagesData);
        }

        private void Reply_readFlash(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            //1...: данные из флэша
            var replyDetails =
                new ProtocolParser.ReplyData.ReadFlashReply(reply);
            _logger.AddText(replyDetails.ToString(), (byte)DataDirection.Info, DateTime.Now,
                TextLogger.TextLogger.TimeFormat.None);

            _stationFlash.Add(replyDetails.Address, replyDetails.Data);
        }

        private void Reply_writeFlash(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            //1...: данные из флэша
            var replyDetails =
                new ProtocolParser.ReplyData.WriteFlashReply(reply);
            _logger.AddText(replyDetails.ToString(), (byte)DataDirection.Info, DateTime.Now,
                TextLogger.TextLogger.TimeFormat.None);
        }

        private void Reply_getConfig(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            //1: версия прошивки
            //2: номер режима
            //3: тип чипов (емкость разная, а распознать их программно можно только по ошибкам чтения "дальних" страниц)
            //4-7: емкость флэш-памяти
            //12-15: коэффициент пересчета напряжения (float, 4 bytes) - просто умножаешь коэффициент на полученное в статусе число и будет температура
            //16: коэфф. усиления антенны
            //17-18: размер блока хранения команды
            //19-20: размер стираемого блока
            //21-24: минимальное значение напряжения батареи

            var replyDetails = new ProtocolParser.ReplyData.GetConfigReply(reply);
            _logger.AddText(replyDetails.ToString(), (byte)DataDirection.Info, DateTime.Now,
                TextLogger.TextLogger.TimeFormat.None);

            StationSettings.Number = reply.StationNumber;
            Parser.StationNumber = StationSettings.Number;

            StationSettings.FwVersion = replyDetails.FwVersion;
            StationSettings.Mode = replyDetails.Mode;

            if (replyDetails.ChipTypeId == 213)
                StationSettings.ChipType = RfidContainer.ChipTypes.Types["NTAG213"];
            else if (replyDetails.ChipTypeId == 215)
                StationSettings.ChipType = RfidContainer.ChipTypes.Types["NTAG215"];
            else if (replyDetails.ChipTypeId == 216)
                StationSettings.ChipType = RfidContainer.ChipTypes.Types["NTAG216"];

            StationSettings.FlashSize = replyDetails.FlashSize;
            if (StationSettings.FlashSize < _stationFlash.Size)
                // check _selectedFlashSize
                RefreshFlashGrid(_selectedFlashSize, StationSettings.TeamBlockSize, _bytesPerRow);

            StationSettings.VoltageCoefficient = replyDetails.VoltageKoeff;
            StationSettings.AntennaGain = replyDetails.AntennaGain;
            StationSettings.TeamBlockSize = replyDetails.TeamBlockSize;
            StationSettings.EraseBlockSize = replyDetails.EraseBlockSize;
            StationSettings.BatteryLimit = replyDetails.BatteryLimit;
            StationSettings.MaxPacketLength = replyDetails.MaxPacketLength;
            Parser.MaxPacketLength = StationSettings.MaxPacketLength;
            StationSettings.AutoReport = replyDetails.AutoreportMode;

            Invoke((MethodInvoker)delegate
           {
               textBox_stationNumber.Text = StationSettings.Number.ToString();
               textBox_fwVersion.Text = StationSettings.FwVersion.ToString();
               comboBox_mode.SelectedItem =
                   StationSettings.StationMode.FirstOrDefault(x => x.Value == StationSettings.Mode).Key;
               comboBox_chipType.SelectedIndex = StationSettings.ChipType;
               textBox_flashSize.Text = (int)(StationSettings.FlashSize / 1024 / 1024) + " Mb";
               // switch flash size combobox to new value if bigger than new FlashSize
               textBox_koeff.Text = StationSettings.VoltageCoefficient.ToString("F5");
               comboBox_setGain.SelectedItem =
                   StationSettings.Gain.FirstOrDefault(x => x.Value == StationSettings.AntennaGain).Key;
               textBox_teamFlashSize.Text = StationSettings.TeamBlockSize.ToString();
               textBox_eraseBlock.Text = StationSettings.EraseBlockSize.ToString();
               textBox_setBatteryLimit.Text = StationSettings.BatteryLimit.ToString("F3");
               checkBox_AutoReport.Checked = StationSettings.AutoReport;
               textBox_packetLength.Text = StationSettings.MaxPacketLength.ToString();
           });
        }

        private void Reply_scanTeams(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            //1-2: номер 1й команды
            //3-4: номер 2й команды           
            //...	                        
            //(n - 1) - n: номер последней команды
            var replyDetails =
                new ProtocolParser.ReplyData.ScanTeamsReply(reply);
            _logger.AddText(replyDetails.ToString(), (byte)DataDirection.Info, DateTime.Now,
                TextLogger.TextLogger.TimeFormat.None);

            foreach (var n in replyDetails.TeamsList)
            {
                var tmpTeam = new TeamData { TeamNumber = n };
                _teams.Add(tmpTeam);
            }

            if (replyDetails.TeamsList.Length * 2 < 252 - 7)
                _needMore = false;
        }

        private void Reply_sendBtCommand(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            //1-n: ответ BT модуля
            var replyDetails =
                new ProtocolParser.ReplyData.SendBtCommandReply(reply);
            _logger.AddText("BT reply: " + replyDetails + Environment.NewLine, (byte)DataDirection.Info, DateTime.Now,
                TextLogger.TextLogger.TimeFormat.None);
        }

        private void Reply_getLastErrors(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            //1-2: номер 1й ошибки
            //3-4: номер 2й ошибки
            //...
            //(n - 1) - n: номер последней ошибки
            var replyDetails =
                new ProtocolParser.ReplyData.GetLastErrorsReply(reply);
            _logger.AddText(replyDetails.ToString(), (byte)DataDirection.Info, DateTime.Now,
                TextLogger.TextLogger.TimeFormat.None);
        }
        #endregion

        #region Helpers

        private void RefreshFlashGrid(uint flashSize, uint teamDumpSize, uint bytesPerRow)
        {
            _stationFlash = new FlashContainer(flashSize, teamDumpSize, bytesPerRow);
            dataGridView_flashRawData.DataSource = null;
            dataGridView_flashRawData.Columns.Clear();
            dataGridView_flashRawData.AutoGenerateColumns = true;
            dataGridView_flashRawData.DataSource = _stationFlash.Table;
            dataGridView_flashRawData.AutoGenerateColumns = false;
            dataGridView_flashRawData.Columns.Remove(FlashContainer.TableColumns.RawData);
            dataGridView_flashRawData.Columns.Remove(FlashContainer.TableColumns.DecodedData);
            dataGridView_flashRawData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView_flashRawData.AutoResizeColumns();
            dataGridView_flashRawData.ScrollBars = ScrollBars.Both;
            dataGridView_flashRawData.AllowUserToResizeColumns = true;
            dataGridView_flashRawData.AllowUserToOrderColumns = false;
            for (var i = 0; i < dataGridView_flashRawData.Columns.Count; i++)
                dataGridView_flashRawData.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private void RefreshChipGrid(byte chipTypeId)
        {
            _rfidCard = new RfidContainer(chipTypeId);
            dataGridView_chipRawData.DataSource = _rfidCard.Table;
            dataGridView_chipRawData.AutoGenerateColumns = true;
            dataGridView_chipRawData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView_chipRawData.AutoResizeColumns();
            dataGridView_chipRawData.ScrollBars = ScrollBars.Both;
            dataGridView_chipRawData.AllowUserToResizeColumns = true;
            dataGridView_chipRawData.AllowUserToOrderColumns = false;
            for (var i = 0; i < dataGridView_chipRawData.Columns.Count; i++)
                dataGridView_chipRawData.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private void RefreshTeamsGrid()
        {
            _teams = new TeamsContainer();
            dataGridView_teams.DataSource = _teams.Table;
            dataGridView_teams.AutoGenerateColumns = true;
            dataGridView_teams.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView_teams.AutoResizeColumns();
            dataGridView_teams.ScrollBars = ScrollBars.Both;
            dataGridView_teams.AllowUserToResizeColumns = true;
            dataGridView_teams.AllowUserToOrderColumns = false;
            for (var i = 0; i < dataGridView_teams.Columns.Count; i++)
                dataGridView_teams.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        #endregion

        #region GUI

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _logger = new TextLogger.TextLogger(this)
            {
                Channels = _directions,
                FilterZeroChar = false
            };
            textBox_terminal.DataBindings.Add("Text", _logger, "Text", false, DataSourceUpdateMode.OnPropertyChanged);

            _logger.LineTimeLimit = 100;
            _logger.LineLimit = Settings.Default.LogLinesLimit;
            _logger.LogFileName = Settings.Default.LogAutoSaveFile;
            _logger.AutoSave = !string.IsNullOrEmpty(_logger.LogFileName);
            _logger.DefaultTextFormat = TextLogger.TextLogger.TextFormat.AutoReplaceHex;
            _logger.DefaultTimeFormat = TextLogger.TextLogger.TimeFormat.LongTime;
            _logger.DefaultDateFormat = TextLogger.TextLogger.DateFormat.None;
            _logger.AutoScroll = checkBox_autoScroll.Checked;
            CheckBox_autoScroll_CheckedChanged(null, EventArgs.Empty);

            comboBox_portSpeed.Items.AddRange(new object[] { 9600, 38400, 57600, 115200, 230400, 256000, 512000, 921600 });

            _portSpeed = Settings.Default.BaudRate;
            comboBox_portSpeed.SelectedItem = _portSpeed;

            serialPort1.Encoding = Encoding.GetEncoding(INPUT_CODE_PAGE);
            //Serial init
            Button_refresh_Click(null, EventArgs.Empty);

            foreach (var item in StationSettings.StationMode) comboBox_mode.Items.Add(item.Key);

            foreach (var item in StationSettings.Gain) comboBox_setGain.Items.Add(item.Key);

            foreach (var item in RfidContainer.ChipTypes.Types) comboBox_chipType.Items.Add(item.Key);

            foreach (var item in FlashSizeLimit) comboBox_flashSize.Items.Add(item.Key);

            textBox_setTime.Text = Helpers.DateToString(DateTime.Now.ToUniversalTime());
            comboBox_mode.SelectedIndex = 0;
            comboBox_setGain.SelectedIndex = 0;

            Parser = new ProtocolParser();

            _teams = new TeamsContainer();
            RefreshTeamsGrid();

            _rfidCard = new RfidContainer(_selectedChipType);
            RefreshChipGrid(StationSettings.ChipType);

            _stationFlash = new FlashContainer(_selectedFlashSize, StationSettings.TeamBlockSize, _bytesPerRow);

            textBox_flashSize.Text = (int)(StationSettings.FlashSize / 1024 / 1024) + " Mb";
            textBox_teamFlashSize.Text = StationSettings.TeamBlockSize.ToString();

            comboBox_flashSize.SelectedIndex = 0;
            comboBox_chipType.SelectedIndex = 1;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
        }

        private void CheckBox_autoTime_CheckedChanged(object sender, EventArgs e)
        {
            textBox_setTime.Enabled = !checkBox_autoTime.Checked;
            textBox_setTime.Text = Helpers.DateToString(DateTime.Now.ToUniversalTime());
        }

        private void TextBox_stationNumber_Leave(object sender, EventArgs e)
        {
            byte.TryParse(textBox_stationNumber.Text, out StationSettings.Number);
            Parser.StationNumber = StationSettings.Number;
            textBox_stationNumber.Text = StationSettings.Number.ToString();
        }

        private void TextBox_teamMask_Leave(object sender, EventArgs e)
        {
            if (textBox_teamMask.Text.Length > 16)
                textBox_teamMask.Text = textBox_teamMask.Text.Substring(0, 16);
            else if (textBox_teamMask.Text.Length < 16)
                while (textBox_teamMask.Text.Length < 16)
                    textBox_teamMask.Text = "0" + textBox_teamMask.Text;

            var n = Helpers.ConvertStringToMask(textBox_teamMask.Text);
            textBox_teamMask.Clear();
            for (var i = 15; i >= 0; i--)
                textBox_teamMask.Text = Helpers.ConvertMaskToString(n);
        }

        private void TextBox_setTime_Leave(object sender, EventArgs e)
        {
            var t = Helpers.DateStringToUnixTime(textBox_setTime.Text);
            textBox_setTime.Text = Helpers.DateToString(Helpers.ConvertFromUnixTimestamp(t));
        }

        private void TextBox_newStationNumber_Leave(object sender, EventArgs e)
        {
            byte.TryParse(textBox_newStationNumber.Text, out var n);
            textBox_newStationNumber.Text = n.ToString();
        }

        private void TextBox_checkedChips_Leave(object sender, EventArgs e)
        {
            ushort.TryParse(textBox_checkedChips.Text, out var n);
            textBox_checkedChips.Text = n.ToString();
        }

        private void TextBox_lastCheck_Leave(object sender, EventArgs e)
        {
            var t = Helpers.DateStringToUnixTime(textBox_lastCheck.Text);
            textBox_lastCheck.Text = Helpers.DateToString(Helpers.ConvertFromUnixTimestamp(t));
        }

        private void TextBox_issueTime_Leave(object sender, EventArgs e)
        {
            var t = Helpers.DateStringToUnixTime(textBox_issueTime.Text);
            textBox_issueTime.Text = Helpers.DateToString(Helpers.ConvertFromUnixTimestamp(t));
        }

        private void TextBox_readChipPage_Leave(object sender, EventArgs e)
        {
            if (!textBox_readChipPage.Text.Contains('-'))
                textBox_readChipPage.Text = "0-" + textBox_readChipPage.Text;
            ushort.TryParse(
                textBox_readChipPage.Text.Substring(0, textBox_readChipPage.Text.IndexOf("-", StringComparison.Ordinal))
                    .Trim(), out var from);
            ushort.TryParse(
                textBox_readChipPage.Text
                    .Substring(textBox_readChipPage.Text.IndexOf("-", StringComparison.Ordinal) + 1).Trim(),
                out var to);
            if (to - from > (StationSettings.MaxPacketLength - 7 - ProtocolParser.ReplyDataLength.READ_CARD_PAGE) / 4)
                to = (ushort)((StationSettings.MaxPacketLength - 7 - ProtocolParser.ReplyDataLength.READ_CARD_PAGE) / 4);
            textBox_readChipPage.Text = from + "-" + to;
        }

        private void TextBox_writeChipPage_Leave(object sender, EventArgs e)
        {
            byte.TryParse(textBox_writeChipPage.Text, out var n);
            textBox_writeChipPage.Text = n.ToString();
        }

        private void TextBox_data_Leave(object sender, EventArgs e)
        {
            textBox_data.Text = Helpers.CheckHexString(textBox_data.Text);
            var n = Helpers.ConvertHexToByteArray(textBox_data.Text);
            textBox_data.Text = Helpers.ConvertByteArrayToHex(n, 4);
        }

        private void TextBox_uid_Leave(object sender, EventArgs e)
        {
            textBox_uid.Text = Helpers.CheckHexString(textBox_uid.Text);
            var n = Helpers.ConvertHexToByteArray(textBox_uid.Text);
            textBox_uid.Text = Helpers.ConvertByteArrayToHex(n);
            if (textBox_uid.Text.Length > 24)
                textBox_uid.Text = textBox_uid.Text.Substring(0, 24);
            else if (textBox_uid.Text.Length < 24)
                while (textBox_uid.Text.Length < 24)
                    textBox_uid.Text = "00 " + textBox_uid.Text;
        }

        private void TextBox_readFlash_Leave(object sender, EventArgs e)
        {
            long.TryParse(textBox_readFlashAddress.Text, out var from);
            textBox_readFlashAddress.Text = from.ToString();
        }

        private void TextBox_writeAddr_Leave(object sender, EventArgs e)
        {
            uint.TryParse(textBox_writeAddr.Text, out var n);
            textBox_writeAddr.Text = n.ToString();
        }

        private void TextBox_flashData_Leave(object sender, EventArgs e)
        {
            textBox_flashData.Text = Helpers.CheckHexString(textBox_flashData.Text);
            var n = Helpers.ConvertHexToByteArray(textBox_flashData.Text);
            textBox_flashData.Text = Helpers.ConvertByteArrayToHex(n,
                (uint)StationSettings.MaxPacketLength - ProtocolParser.CommandDataLength.WRITE_FLASH);
        }

        private void TabControl_teamData_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl_teamData.SelectedIndex == 0 && checkBox_autoScroll.Checked)
            {
                textBox_terminal.SelectionStart = textBox_terminal.Text.Length;
                textBox_terminal.ScrollToCaret();
            }
        }

        private void TextBox_koeff_Leave(object sender, EventArgs e)
        {
            textBox_koeff.Text =
                textBox_koeff.Text.Replace(".", _decimalSeparator);
            textBox_koeff.Text =
                textBox_koeff.Text.Replace(",", _decimalSeparator);
            float.TryParse(textBox_koeff.Text, out var koeff);
            textBox_koeff.Text = koeff.ToString("F5");
        }

        private void Button_clearLog_Click(object sender, EventArgs e)
        {
            _logger.Clear();
        }

        private void Button_clearTeams_Click(object sender, EventArgs e)
        {
            RefreshTeamsGrid();
        }

        private void Button_clearRfid_Click(object sender, EventArgs e)
        {
            RefreshChipGrid(StationSettings.ChipType);
        }

        private void Button_clearFlash_Click(object sender, EventArgs e)
        {
            RefreshFlashGrid(_selectedFlashSize, StationSettings.TeamBlockSize, _bytesPerRow);
        }

        private void Button_saveLog_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "station_" + StationSettings.Number + ".log";
            saveFileDialog1.Title = "Save log to file";
            saveFileDialog1.DefaultExt = "txt";
            saveFileDialog1.Filter = "Text files|*.txt|All files|*.*";
            saveFileDialog1.ShowDialog();
        }

        private void Button_saveTeams_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "station_" + StationSettings.Number + "_teams.csv";
            saveFileDialog1.Title = "Save teams to file";
            saveFileDialog1.DefaultExt = "csv";
            saveFileDialog1.Filter = "CSV files|*.csv";
            saveFileDialog1.ShowDialog();
        }

        private void Button_saveRfid_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "uid_" + dataGridView_chipRawData.Rows[0].Cells[2].Value +
                                       dataGridView_chipRawData.Rows[1].Cells[2].Value.ToString().Trim() + ".bin";
            saveFileDialog1.FileName = saveFileDialog1.FileName.Replace(' ', '_');
            saveFileDialog1.Title = "Save card dump to file";
            saveFileDialog1.DefaultExt = "bin";
            saveFileDialog1.Filter = "Binary files|*.bin|CSV files|*.csv";
            saveFileDialog1.ShowDialog();
        }

        private void Button_saveFlash_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "station_" + StationSettings.Number + "_flash.bin";
            saveFileDialog1.Title = "Save flash dump to file";
            saveFileDialog1.DefaultExt = "txt";
            saveFileDialog1.Filter = "Binary files|*.bin|CSV files|*.csv";
            saveFileDialog1.ShowDialog();
        }

        private void SaveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (saveFileDialog1.Title == "Save log to file")
            {
                File.WriteAllText(saveFileDialog1.FileName, textBox_terminal.Text);
            }
            else if (saveFileDialog1.Title == "Save teams to file")
            {
                var sb = new StringBuilder();

                var headers = dataGridView_teams.Columns.Cast<DataGridViewColumn>();
                sb.AppendLine(string.Join(",", headers.Select(column => "\"" + column.HeaderText + "\"").ToArray()));

                foreach (DataGridViewRow row in dataGridView_teams.Rows)
                {
                    var cells = row.Cells.Cast<DataGridViewCell>();
                    sb.AppendLine(string.Join(",", cells.Select(cell => "\"" + cell.Value + "\"").ToArray()));
                }

                File.WriteAllText(saveFileDialog1.FileName, sb.ToString());
            }
            else if (saveFileDialog1.Title == "Save card dump to file")
            {
                if (saveFileDialog1.FilterIndex == 1)
                {
                    var tmp = new byte[_rfidCard.Dump.Length];
                    for (var i = 0; i < _rfidCard.Dump.Length; i++) tmp[i] = (byte)_rfidCard.Dump[i];
                    File.WriteAllBytes(saveFileDialog1.FileName, tmp);
                }
                else if (saveFileDialog1.FilterIndex == 2)
                {
                    var sb = new StringBuilder();

                    var headers = _rfidCard.Table.Columns.Cast<DataColumn>();
                    sb.AppendLine(string.Join(",",
                        headers.Select(column => "\"" + column.ColumnName + "\"").ToArray()));

                    foreach (DataRow page in _rfidCard.Table.Rows)
                    {
                        for (var i = 0; i < page.ItemArray.Count(); i++) sb.Append(page.ItemArray[i] + ";");
                        sb.AppendLine();
                    }

                    File.WriteAllText(saveFileDialog1.FileName, sb.ToString());
                }
            }
            else if (saveFileDialog1.Title == "Save flash dump to file")
            {
                if (saveFileDialog1.FilterIndex == 1)
                {
                    File.WriteAllBytes(saveFileDialog1.FileName, _stationFlash.Dump);
                }
                else if (saveFileDialog1.FilterIndex == 2)
                {
                    var sb = new StringBuilder();

                    var headers = _stationFlash.Table.Columns.Cast<DataColumn>();
                    sb.AppendLine(string.Join(",",
                        headers.Select(column => "\"" + column.ColumnName + "\"").ToArray()));

                    foreach (DataRow team in _stationFlash.Table.Rows)
                    {
                        for (var i = 0; i < team.ItemArray.Count(); i++) sb.Append(team.ItemArray[i] + ";");
                        sb.AppendLine();
                    }

                    File.WriteAllText(saveFileDialog1.FileName, sb.ToString());
                }
            }
        }

        private void Button_dumpTeams_Click(object sender, EventArgs e)
        {
            button_dumpTeams.Enabled = false;
            button_getTeamRecord.Enabled = false;
            RefreshTeamsGrid();

            var maxTeams = (ushort)(_stationFlash.Size / StationSettings.TeamBlockSize);

            // get list of commands in flash
            ushort teamNum = 1;
            _logger.NoScreenOutput = true;

            _asyncFlag = 0;
            _needMore = false;
            var startTime = DateTime.Now.ToUniversalTime();
            do
            {
                //0-1: какую запись
                var scanTeams = Parser.ScanTeams(teamNum);
                _asyncFlag++;
                SendCommand(scanTeams);

                long timeout = 1000;
                while (_asyncFlag > 0)
                {
                    Helpers.DelayMs(1);
                    if (timeout <= 0)
                        break;
                    timeout--;
                }

                if (dataGridView_teams.RowCount == 0 || !ushort.TryParse(
                    dataGridView_teams.Rows[dataGridView_teams.RowCount - 1].Cells[0].Value.ToString(),
                    out teamNum))
                {
                    _logger.NoScreenOutput = false;
                    button_dumpTeams.Enabled = true;
                    button_getTeamRecord.Enabled = true;
                    return;
                }

                if (!_needMore)
                    teamNum = maxTeams;
            } while (teamNum < maxTeams);

            _logger.AddText(Environment.NewLine + "Teams list time=" +
                            DateTime.Now.ToUniversalTime().Subtract(startTime).TotalMilliseconds + " ms." +
                            Environment.NewLine, (byte)DataDirection.Info, DateTime.Now,
                TextLogger.TextLogger.TimeFormat.None);

            // load every command data
            var rowNum = 0;
            _logger.NoScreenOutput = true;
            _asyncFlag = 0;
            startTime = DateTime.Now.ToUniversalTime();
            while (rowNum < _teams.Table.Rows.Count)
            {
                if (!ushort.TryParse(
                    _teams.Table.Rows[rowNum][0].ToString(),
                    out teamNum))
                    break;

                //0-1: какую запись
                var getTeamRecord = Parser.GetTeamRecord(teamNum);
                _asyncFlag++;
                SendCommand(getTeamRecord);
                rowNum++;

                long timeout = 1000;
                while (_asyncFlag > 0)
                {
                    Helpers.DelayMs(1);
                    if (timeout <= 0)
                        break;
                    timeout--;
                }
            }

            _logger.AddText(Environment.NewLine + "Teams dump time=" +
                            DateTime.Now.ToUniversalTime().Subtract(startTime).TotalMilliseconds + " ms." +
                            Environment.NewLine, (byte)DataDirection.Info, DateTime.Now,
                TextLogger.TextLogger.TimeFormat.None);

            dataGridView_teams.Refresh();
            dataGridView_teams.PerformLayout();
            dataGridView_teams.Invalidate();

            _logger.NoScreenOutput = false;
            button_dumpTeams.Enabled = true;
            button_getTeamRecord.Enabled = true;
        }

        private void Button_dumpChip_Click(object sender, EventArgs e)
        {
            RefreshChipGrid(StationSettings.ChipType);
            button_dumpChip.Enabled = false;
            button_readChipPage.Enabled = false;

            var chipSize = RfidContainer.ChipTypes.PageSizes[_rfidCard.CurrentChipType];
            byte maxFramePages = 45;
            ushort pagesFrom = 0;
            ushort pagesTo;
            _logger.NoScreenOutput = true;
            _asyncFlag = 0;
            var startTime = DateTime.Now.ToUniversalTime();
            do
            {
                pagesTo = (ushort)(pagesFrom + maxFramePages - 1);
                if (pagesTo >= chipSize)
                    pagesTo = (ushort)(chipSize - 1);

                //0: с какой страницу карты
                //1: по какую страницу карты включительно
                var readCardPage = Parser.ReadCardPage((byte)pagesFrom, (byte)pagesTo);
                _asyncFlag++;
                SendCommand(readCardPage);
                pagesFrom = (byte)(pagesTo + 1);
                long timeout = 1000;
                while (_asyncFlag > 0)
                {
                    Helpers.DelayMs(1);
                    if (timeout <= 0)
                        break;
                    timeout--;
                }
            } while (pagesTo < chipSize - 1);

            _logger.AddText(Environment.NewLine + "RFID dump time=" +
                            DateTime.Now.ToUniversalTime().Subtract(startTime).TotalMilliseconds +
                            " ms." + Environment.NewLine, (byte)DataDirection.Info, DateTime.Now,
                TextLogger.TextLogger.TimeFormat.None);

            dataGridView_chipRawData.Refresh();
            dataGridView_chipRawData.PerformLayout();

            _logger.NoScreenOutput = false;
            button_dumpChip.Enabled = true;
            button_readChipPage.Enabled = true;
        }

        private void Button_dumpFlash_Click(object sender, EventArgs e)
        {
            RefreshFlashGrid(_selectedFlashSize, StationSettings.TeamBlockSize, _bytesPerRow);
            button_dumpFlash.Enabled = false;
            button_readFlash.Enabled = false;

            var maxFrameBytes = (ushort)(StationSettings.MaxPacketLength - 7 - ProtocolParser.ReplyDataLength.READ_FLASH - 1);
            var currentRow = 0;
            uint addrFrom = 0;
            uint addrTo;
            _logger.NoScreenOutput = true;
            _asyncFlag = 0;
            var startTime = DateTime.Now.ToUniversalTime();
            do
            {
                addrTo = addrFrom + maxFrameBytes;
                if (addrTo >= _stationFlash.Size)
                    addrTo = _stationFlash.Size;

                var readFlash = Parser.ReadFlash(addrFrom, (ushort)(addrTo - addrFrom));
                _asyncFlag++;
                SendCommand(readFlash);
                addrFrom = addrTo;

                long timeout = 1000;
                while (_asyncFlag > 0)
                {
                    Helpers.DelayMs(1);
                    if (timeout <= 0)
                        break;
                    timeout--;
                }

                //check if it's time to update grid
                if ((int)(addrFrom / _bytesPerRow) > currentRow) currentRow = (int)(addrFrom / _bytesPerRow);
            } while (addrTo < _stationFlash.Size);

            _logger.AddText(Environment.NewLine + "Flash dump time=" +
                            DateTime.Now.ToUniversalTime().Subtract(startTime).TotalMilliseconds + " ms." +
                            Environment.NewLine, (byte)DataDirection.Info, DateTime.Now,
                TextLogger.TextLogger.TimeFormat.None);

            _logger.NoScreenOutput = false;
            dataGridView_flashRawData.Refresh();
            dataGridView_flashRawData.PerformLayout();

            button_dumpFlash.Enabled = true;
            button_readFlash.Enabled = true;
        }

        private void Button_eraseChip_Click(object sender, EventArgs e)
        {
            button_eraseChip.Enabled = false;

            //0-7: UID чипа
            // read uid from card or use default
            var uid = Helpers.ConvertHexToByteArray(textBox_uid.Text);

            if (uid.Length != 8)
                return;

            //9-12: данные страницы карты (4 байта)
            byte[] data = { 0, 0, 0, 0 };

            var chipSize = RfidContainer.ChipTypes.PageSizes[_rfidCard.CurrentChipType];
            byte page;
            _asyncFlag = 0;
            var startTime = DateTime.Now.ToUniversalTime();
            for (page = 4; page < chipSize - 4; page++)
            {
                //8: номер страницы
                var writeCardPage = Parser.WriteCardPage(uid, page, data);
                _asyncFlag++;
                SendCommand(writeCardPage);

                long timeout = 1000;
                while (_asyncFlag > 0)
                {
                    Helpers.DelayMs(1);
                    if (timeout <= 0)
                        break;
                    timeout--;
                }
            }

            _logger.AddText(Environment.NewLine + "RFID clear time=" +
                            DateTime.Now.ToUniversalTime().Subtract(startTime).TotalMilliseconds + " ms." +
                            Environment.NewLine, (byte)DataDirection.Info, DateTime.Now,
                TextLogger.TextLogger.TimeFormat.None);

            button_eraseChip.Enabled = true;
        }

        private void Button_unlockChip_Click(object sender, EventArgs e)
        {
            button_unlockChip.Enabled = false;

            var setPack = Parser.UnlockChip();
            SendCommand(setPack);

            button_unlockChip.Enabled = true;
        }

        private void Button_quickDump_Click(object sender, EventArgs e)
        {
            button_quickDump.Enabled = false;
            button_dumpFlash.Enabled = false;

            Button_dumpTeams_Click(this, EventArgs.Empty);

            // load every command data
            ushort rowNum = 0;
            _logger.NoScreenOutput = true;
            _asyncFlag = 0;
            var startTime = DateTime.Now.ToUniversalTime();
            while (rowNum < dataGridView_teams.RowCount)
            {
                if (!ushort.TryParse(
                    dataGridView_teams.Rows[rowNum].Cells[0].Value.ToString(),
                    out var teamNum) || teamNum >= dataGridView_flashRawData.RowCount)
                    break;
                DataGridView_flashRawData_CellDoubleClick(this, new DataGridViewCellEventArgs(0, teamNum));
                rowNum++;
            }

            _logger.AddText(Environment.NewLine + "Teams dump time=" +
                            DateTime.Now.ToUniversalTime().Subtract(startTime).TotalMilliseconds + " ms." +
                            Environment.NewLine, (byte)DataDirection.Info, DateTime.Now,
                TextLogger.TextLogger.TimeFormat.None);

            dataGridView_teams.Refresh();
            dataGridView_teams.PerformLayout();

            _logger.NoScreenOutput = false;
            button_quickDump.Enabled = true;
            button_dumpFlash.Enabled = true;
        }

        private void DataGridView_teams_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!serialPort1.IsOpen || e.ColumnIndex < 0 || e.RowIndex < 0)
                return;

            //0-1: какую запись
            ushort.TryParse(dataGridView_teams?.Rows[e.RowIndex].Cells[0]?.Value?.ToString(), out var teamNum);
            if (teamNum <= 0)
                return;

            _asyncFlag = 0;
            var getTeamRecord = Parser.GetTeamRecord(teamNum);
            _asyncFlag++;
            SendCommand(getTeamRecord);

            long timeout = 1000;
            while (_asyncFlag > 0)
            {
                Helpers.DelayMs(1);
                if (timeout <= 0)
                    break;
                timeout--;
            }

            dataGridView_teams.Refresh();
            dataGridView_teams.PerformLayout();
        }

        private void DataGridView_chipRawData_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!serialPort1.IsOpen || e.ColumnIndex < 0 || e.RowIndex < 0)
                return;

            var page = (byte)e.RowIndex;

            //0: с какой страницу карты
            //1: по какую страницу карты включительно
            var readCardPage = Parser.ReadCardPage(page, page);
            _asyncFlag = 0;
            _asyncFlag++;
            SendCommand(readCardPage);

            long timeout = 1000;
            while (_asyncFlag > 0)
            {
                Helpers.DelayMs(1);
                if (timeout <= 0)
                    break;
                timeout--;
            }

            dataGridView_chipRawData.Refresh();
            dataGridView_chipRawData.PerformLayout();
        }

        private void DataGridView_flashRawData_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!serialPort1.IsOpen || e.ColumnIndex < 0 || e.RowIndex < 0)
                return;
            button_dumpFlash.Enabled = false;

            var rowFrom = (ushort)e.RowIndex;
            var maxFrameBytes = (ushort)(StationSettings.MaxPacketLength - 7 - ProtocolParser.ReplyDataLength.READ_FLASH - 1);
            var addrFrom = rowFrom * _bytesPerRow;
            uint addrTo;
            var flashSize = addrFrom + _bytesPerRow;
            _asyncFlag = 0;
            do
            {
                addrTo = addrFrom + maxFrameBytes;
                if (addrTo >= flashSize)
                    addrTo = flashSize;

                var readFlash = Parser.ReadFlash(addrFrom, (ushort)(addrTo - addrFrom));
                _asyncFlag++;
                SendCommand(readFlash);
                addrFrom = addrTo;

                long timeout = 1000;
                while (_asyncFlag > 0)
                {
                    Helpers.DelayMs(1);
                    if (timeout <= 0)
                        break;
                    timeout--;
                }
            } while (addrTo < flashSize);

            dataGridView_flashRawData.Refresh();
            dataGridView_flashRawData.PerformLayout();

            button_dumpFlash.Enabled = true;
            DataGridView_flashRawData_RowEnter(this, new DataGridViewCellEventArgs(e.ColumnIndex, e.RowIndex));
        }

        private void ComboBox_flashSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_flashSize.SelectedIndex == 0)
                _selectedFlashSize = 32 * 1024;
            else if (comboBox_flashSize.SelectedIndex == 1)
                _selectedFlashSize = 64 * 1024;
            else if (comboBox_flashSize.SelectedIndex == 2)
                _selectedFlashSize = 128 * 1024;
            else if (comboBox_flashSize.SelectedIndex == 3)
                _selectedFlashSize = 256 * 1024;
            else if (comboBox_flashSize.SelectedIndex == 4)
                _selectedFlashSize = 512 * 1024;
            else if (comboBox_flashSize.SelectedIndex == 5)
                _selectedFlashSize = 1024 * 1024;
            else if (comboBox_flashSize.SelectedIndex == 6)
                _selectedFlashSize = 2048 * 1024;
            else if (comboBox_flashSize.SelectedIndex == 7)
                _selectedFlashSize = 4096 * 1024;
            else if (comboBox_flashSize.SelectedIndex == 8)
                _selectedFlashSize = 8192 * 1024;

            if (_selectedFlashSize > StationSettings.FlashSize)
            {
                _selectedFlashSize = StationSettings.FlashSize;
                comboBox_flashSize.SelectedIndex--;
            }

            RefreshFlashGrid(_selectedFlashSize, StationSettings.TeamBlockSize, _bytesPerRow);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                serialPort1.Close();
            }
            catch (Exception)
            {
            }
        }

        private void TextBox_teamFlashSize_Leave(object sender, EventArgs e)
        {
            uint.TryParse(textBox_teamFlashSize.Text, out var teamFlashSize);
            textBox_teamFlashSize.Text = teamFlashSize.ToString();
        }

        private void TextBox_eraseBlock_Leave(object sender, EventArgs e)
        {
            uint.TryParse(textBox_eraseBlock.Text, out var teamFlashSize);
            textBox_eraseBlock.Text = teamFlashSize.ToString();
        }

        private void TextBox_initTeamNum_Leave(object sender, EventArgs e)
        {
            ushort.TryParse(textBox_initTeamNum.Text, out var n);
            textBox_initTeamNum.Text = n.ToString();
        }

        private void TextBox_initMask_Leave(object sender, EventArgs e)
        {
            if (textBox_initMask.Text.Length > 16)
                textBox_initMask.Text = textBox_initMask.Text.Substring(0, 16);
            else if (textBox_initMask.Text.Length < 16)
                while (textBox_initMask.Text.Length < 16)
                    textBox_initMask.Text = "0" + textBox_initMask.Text;

            var n = Helpers.ConvertStringToMask(textBox_initMask.Text);
            textBox_initMask.Clear();
            for (var i = 15; i >= 0; i--)
                textBox_initMask.Text = Helpers.ConvertMaskToString(n);
        }

        private void TextBox_readFlashLength_Leave(object sender, EventArgs e)
        {
            ushort.TryParse(textBox_readFlashLength.Text, out var toAddr);
            if (toAddr > StationSettings.MaxPacketLength - 7 - ProtocolParser.ReplyDataLength.READ_FLASH - 1)
                toAddr = (ushort)(StationSettings.MaxPacketLength - 7 - ProtocolParser.ReplyDataLength.READ_FLASH - 1);
            textBox_readFlashLength.Text = toAddr.ToString();
        }

        private void TextBox_BtName_Leave(object sender, EventArgs e)
        {
            if (textBox_BtName.Text == "")
                textBox_BtName.Text = "Sportduino-xx";
        }

        private void TextBox_BtPin_Leave(object sender, EventArgs e)
        {
            if (textBox_BtPin.Text == "")
                textBox_BtPin.Text = "1234";
            var pin = new List<byte>();
            pin.AddRange(Encoding.ASCII.GetBytes(textBox_BtPin.Text));
            for (var i = 0; i < pin.Count; i++)
                if (pin[i] < 0x30 || pin[i] > 0x39)
                {
                    pin.RemoveAt(i);
                    i--;
                }

            if (pin.Count > 16)
                pin.RemoveRange(16, pin.Count - 16);
            textBox_BtPin.Text = Encoding.UTF8.GetString(pin.ToArray());
        }

        private void ComboBox_chipType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!RfidContainer.ChipTypes.Types.TryGetValue(comboBox_chipType.SelectedItem.ToString(), out var n))
            {
                comboBox_chipType.SelectedItem = RfidContainer.ChipTypes.Names[_rfidCard.CurrentChipType];
                _selectedChipType = _rfidCard.CurrentChipType;
            }
            else
            {
                _selectedChipType = n;
            }
        }

        private void TextBox_setBatteryLimit_Leave(object sender, EventArgs e)
        {
            textBox_setBatteryLimit.Text =
                textBox_setBatteryLimit.Text.Replace(".", _decimalSeparator);
            textBox_setBatteryLimit.Text =
                textBox_setBatteryLimit.Text.Replace(",", _decimalSeparator);
            float.TryParse(textBox_setBatteryLimit.Text, out var limit);
            textBox_setBatteryLimit.Text = limit.ToString("F3");
        }

        private void Button_loadFlash_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Load flash dump";
            openFileDialog1.DefaultExt = "bin";
            openFileDialog1.Filter = "Binary files|*.bin";
            openFileDialog1.ShowDialog();
        }

        private void Button_loadRfid_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Load card dump";
            openFileDialog1.DefaultExt = "bin";
            openFileDialog1.Filter = "Binary files|*.bin";
            openFileDialog1.FileName = "";
            openFileDialog1.ShowDialog();
        }

        private void OpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (openFileDialog1.Title == "Load card dump")
            {
                var data = File.ReadAllBytes(openFileDialog1.FileName);

                if (data.Length < 16)
                    return;

                if (data[14] == 0x12)
                    StationSettings.ChipType = RfidContainer.ChipTypes.Types["NTAG213"];
                else if (data[14] == 0x3e)
                    StationSettings.ChipType = RfidContainer.ChipTypes.Types["NTAG215"];
                else if (data[14] == 0x6d)
                    StationSettings.ChipType = RfidContainer.ChipTypes.Types["NTAG216"];
                else
                    return;

                RefreshChipGrid(StationSettings.ChipType);

                var pages = (byte)(data.Length / RfidContainer.ChipTypes.PageSize);
                for (byte i = 0; i < pages; i++)
                {
                    var tmp = new byte[RfidContainer.ChipTypes.PageSize];
                    for (var j = 0; j < tmp.Length; j++) tmp[j] = data[i * RfidContainer.ChipTypes.PageSize + j];
                    _rfidCard.AddPages(i, tmp);
                }

                dataGridView_chipRawData.Refresh();
                dataGridView_chipRawData.PerformLayout();
            }
            else if (openFileDialog1.Title == "Load flash dump")
            {
                var data = File.ReadAllBytes(openFileDialog1.FileName);
                RefreshFlashGrid((uint)data.Length, StationSettings.TeamBlockSize, _bytesPerRow);
                _stationFlash.Add(0, data);
            }
        }

        private void TextBox_getTeamsList_Leave(object sender, EventArgs e)
        {
            ushort.TryParse(textBox_TeamNumber.Text, out var n);
            textBox_TeamNumber.Text = n.ToString();
        }

        private void CheckBox_AutoReport_CheckedChanged(object sender, EventArgs e)
        {
            StationSettings.AutoReport = checkBox_AutoReport.Checked;
        }

        private void DataGridView_flashRawData_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            textBox_checkPoints.Text =
                _stationFlash.Table.Rows[e.RowIndex][FlashContainer.TableColumns.DecodedData].ToString();
            textBox_rawData.Text = _stationFlash.Table.Rows[e.RowIndex][FlashContainer.TableColumns.RawData].ToString();
        }

        private void CheckBox_autoScroll_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_autoScroll.Checked)
            {
                _logger.AutoScroll = true;
                textBox_terminal.TextChanged += TextBox_terminal_TextChanged;
            }
            else
            {
                _logger.AutoScroll = false;
                textBox_terminal.TextChanged -= TextBox_terminal_TextChanged;
            }
        }

        private void TextBox_terminal_TextChanged(object sender, EventArgs e)
        {
            textBox_terminal.SelectionStart = textBox_terminal.Text.Length;
            textBox_terminal.ScrollToCaret();
        }

        private void CheckBox_portMon_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_portMon.Checked) this.serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.SerialPort1_DataReceived);
            else this.serialPort1.DataReceived -= new System.IO.Ports.SerialDataReceivedEventHandler(this.SerialPort1_DataReceived);
        }

        private void comboBox_portSpeed_SelectedIndexChanged(object sender, EventArgs e)
        {
            _portSpeed = (int)comboBox_portSpeed.SelectedItem;
        }

        private void checkBox_dtr_CheckedChanged(object sender, EventArgs e)
        {
            serialPort1.DtrEnable = checkBox_dtr.Checked;
        }

        private void checkBox_rts_CheckedChanged(object sender, EventArgs e)
        {
            serialPort1.RtsEnable = checkBox_rts.Checked;
        }

        private void textBox_setPwd_Leave(object sender, EventArgs e)
        {
            textBox_setPwd.Text = Helpers.CheckHexString(textBox_setPwd.Text);
            var n = Helpers.ConvertHexToByteArray(textBox_setPwd.Text);
            textBox_setPwd.Text = Helpers.ConvertByteArrayToHex(n, 4);
        }

        private void textBox_setPack_Leave(object sender, EventArgs e)
        {
            textBox_setPack.Text = Helpers.CheckHexString(textBox_setPack.Text);
            var n = Helpers.ConvertHexToByteArray(textBox_setPack.Text);
            textBox_setPack.Text = Helpers.ConvertByteArrayToHex(n, 2);
        }

        #endregion
    }
}
