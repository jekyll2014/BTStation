// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using RFID_Station_control.Properties;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RfidStationControl;

namespace RFID_Station_control
{
    public partial class Form1 : Form
    {
        private const int INPUT_CODE_PAGE = 866;
        private int _portSpeed = 38400;
        private const ulong _receiveTimeOut = 1000;

        private bool _receivingData;
        private byte _packageId;

        private readonly object _serialReceiveThreadLock = new object();
        private readonly object _serialSendThreadLock = new object();
        private readonly object _textOutThreadLock = new object();

        private volatile ushort _asyncFlag;
        private volatile bool _noTerminalOutputFlag;
        private volatile bool _needMore;


        private int _logLinesLimit = 500;
        private string _logAutoSaveFile = "";
        private bool _logAutoSaveFlag;

        private DateTime _receiveStartTime = DateTime.Now.ToUniversalTime().ToUniversalTime();

        private uint _selectedFlashSize = 4 * 1024 * 1024;
        private uint _bytesPerRow = 1024;

        private static readonly Dictionary<string, long> FlashSizeLimit = new Dictionary<string, long>
        {
            {"32 kb", 32 * 1024},
            { "64 kb" , 64 * 1024},
            { "128 kb" , 128 * 1024},
            { "256 kb" , 256 * 1024},
            { "512 kb" , 512 * 1024},
            { "1 Mb" , 1024 * 1024},
            { "2 Mb" , 2048 * 1024},
            { "4 Mb" , 4096 * 1024},
            { "8 Mb" , 8192 * 1024}
        };

        public class StationSettings
        {
            public byte FwVersion = 0;
            public byte Number = 0;
            public byte Mode = StationMode["Init"];
            public float VoltageCoefficient = 0.00578F;
            public float BatteryLimit = 3.0F;
            public byte AntennaGain = Gain["Level 80"];
            public byte ChipType = RfidContainer.ChipTypes.Types["NTAG215"];
            public uint FlashSize = 4 * 1024 * 1024;
            public ushort TeamBlockSize = 1024;
            public ushort EraseBlockSize = 4096;
            public ushort MaxPacketLength = 255;
            public bool AutoReport = false;
            public string BtName = "Sportduino-xx";
            public string BtPin = "1111";

            //режимы станции
            public static readonly Dictionary<string, byte> StationMode = new Dictionary<string, byte>
            {
            {"Init" , 0},
            { "Start" , 1},
            { "Finish" , 2}
            };

            public static readonly Dictionary<string, byte> Gain = new Dictionary<string, byte>
            {
            {"Level 0", 0},
            { "Level 16", 16},
            { "Level 32", 32},
            { "Level 48", 48},
            { "Level 64", 64},
            { "Level 80", 80},
            { "Level 96", 96},
            { "Level 112", 112}
            };
        }

        private DateTime _getStatusTime = DateTime.Now.ToUniversalTime();

        private StationSettings _station;

        private FlashContainer _stationFlash;

        private byte _selectedChipType = RfidContainer.ChipTypes.Types["NTAG215"];
        private RfidContainer _rfidCard;

        private TeamsContainer _teams;

        public static ProtocolParser Parser;

        #region COM_port_handling

        private void button_refresh_Click(object sender, EventArgs e)
        {
            comboBox_portName.Items.Clear();
            comboBox_portName.Items.Add("None");
            foreach (var portname in SerialPort.GetPortNames()) comboBox_portName.Items.Add(portname); //добавить порт в список

            if (comboBox_portName.Items.Count == 1)
            {
                comboBox_portName.SelectedIndex = 0;
                button_openPort.Enabled = false;
            }
            else
            {
                comboBox_portName.SelectedIndex = 0;
            }


            var ports = SerialPort.GetPortNames();
            if (ports.Length == 0)
            {
                textBox_terminal.Text += "ERROR: No COM ports exist\n\r";
            }
            else
            {
                var portNames = Accessory.BuildPortNameHash(ports);
                foreach (string s in portNames.Keys) textBox_terminal.Text += "\n\r" + portNames[s] + ": " + s + "\n\r";
            }
        }

        private void button_openPort_Click(object sender, EventArgs e)
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
                    SetText("Error opening port " + serialPort1.PortName + ": " + ex.Message);
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

                button_closePort.Enabled = true;
                button_openPort.Enabled = false;
                button_refresh.Enabled = false;
                comboBox_portName.Enabled = false;
                serialPort1.DtrEnable = true;
            }
        }

        private void button_closePort_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
                try
                {
                    serialPort1.Close();
                }
                catch (Exception ex)
                {
                    SetText("Error closing port " + serialPort1.PortName + ": " + ex.Message);
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

            button_closePort.Enabled = false;
            button_openPort.Enabled = true;
            button_refresh.Enabled = true;
            comboBox_portName.Enabled = true;
        }

        //rewrite to validate packet runtime
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (_serialReceiveThreadLock)
            {
                if (checkBox_portMon.Checked)
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
                            SetText("COM port read error: " + ex + "\r\n");
                        }

                    Parser.AddData(input);
                    if (Parser._repliesList.Count > 0)
                    {
                        foreach (var reply in Parser._repliesList)
                        {
                            // command reply from station
                            if (reply.ReplyCode != 0)
                            {
                                SetText(reply.ToString());

                                if (reply.ErrorCode == 0)
                                {
                                    if (reply.ReplyCode == ProtocolParser.Reply.SET_TIME)
                                        reply_setTime(reply);
                                    else if (reply.ReplyCode == ProtocolParser.Reply.GET_STATUS)
                                        reply_getStatus(reply);
                                    else if (reply.ReplyCode == ProtocolParser.Reply.INIT_CHIP)
                                        reply_initChip(reply);
                                    else if (reply.ReplyCode == ProtocolParser.Reply.GET_LAST_TEAMS)
                                        reply_getLastTeams(reply);
                                    else if (reply.ReplyCode == ProtocolParser.Reply.GET_TEAM_RECORD)
                                        reply_getTeamRecord(reply);
                                    else if (reply.ReplyCode == ProtocolParser.Reply.READ_CARD_PAGE)
                                        reply_readCardPages(reply);
                                    else if (reply.ReplyCode == ProtocolParser.Reply.READ_FLASH)
                                        reply_readFlash(reply);
                                    else if (reply.ReplyCode == ProtocolParser.Reply.WRITE_FLASH)
                                        reply_writeFlash(reply);
                                    else if (reply.ReplyCode == ProtocolParser.Reply.GET_CONFIG)
                                        reply_getConfig(reply);
                                    else if (reply.ReplyCode == ProtocolParser.Reply.SCAN_TEAMS)
                                        reply_scanTeams(reply);
                                    else if (reply.ReplyCode == ProtocolParser.Reply.SEND_BT_COMMAND)
                                        reply_sendBtCommand(reply);
                                    else if (reply.ReplyCode == ProtocolParser.Reply.GET_LAST_ERRORS) reply_getLastErrors(reply);
                                }
                            }
                            // text message from station
                            else
                            {
                                SetText(reply.Message);
                            }
                            _asyncFlag--;
                        }
                        Parser._repliesList.Clear();
                    }
                }
            }
        }

        private void serialPort1_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            SetText("COM port error: " + e + "\r\n");
        }

        private void SendCommand(byte[] command)
        {
            if (!serialPort1.IsOpen)
            {
                button_closePort_Click(this, EventArgs.Empty);
                return;
            }
            lock (_serialSendThreadLock)
            {
                if (command == null)
                {
                    SetText("\r\nError generating command data\r\n");
                    return;
                }
                _getStatusTime = DateTime.Now.ToUniversalTime();
                try
                {
                    serialPort1.Write(command, 0, command.Length);
                }
                catch (Exception e)
                {
                    SetText("\r\nCOM port write error: " + e + "\r\n");
                    return;
                }

                SetText("\r\n>> "
                        + Accessory.ConvertByteArrayToHex(command)
                        + "\r\n");
            }
        }

        #endregion

        #region Terminal_window

        private void textBox_terminal_TextChanged(object sender, EventArgs e)
        {
            if (checkBox_autoScroll.Checked)
            {
                textBox_terminal.SelectionStart = textBox_terminal.Text.Length;
                textBox_terminal.ScrollToCaret();
            }
        }

        private delegate void SetTextCallback1(string text);

        private void SetText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;
            lock (_textOutThreadLock)
            {
                if (_noTerminalOutputFlag)
                {
                    if (_logAutoSaveFlag)
                        File.AppendAllText(_logAutoSaveFile, text);
                    return;
                }

                //text = Accessory.FilterZeroChar(text);
                // InvokeRequired required compares the thread ID of the
                // calling thread to the thread ID of the creating thread.
                // If these threads are different, it returns true.
                //if (this.textBox_terminal1.InvokeRequired)
                if (textBox_terminal.InvokeRequired)
                {
                    SetTextCallback1 d = SetText;
                    BeginInvoke(d, text);
                }
                else
                {
                    if (_logAutoSaveFlag) File.AppendAllText(_logAutoSaveFile, text);

                    var pos = textBox_terminal.SelectionStart;
                    textBox_terminal.AppendText(text);
                    if (textBox_terminal.Lines.Length > _logLinesLimit)
                    {
                        var tmp = new StringBuilder();
                        for (var i = textBox_terminal.Lines.Length - _logLinesLimit;
                            i < textBox_terminal.Lines.Length;
                            i++)
                            tmp.Append(textBox_terminal.Lines[i] + "\r\n");

                        textBox_terminal.Text = tmp.ToString();
                    }

                    if (checkBox_autoScroll.Checked)
                    {
                        textBox_terminal.SelectionStart = textBox_terminal.Text.Length;
                        textBox_terminal.ScrollToCaret();
                    }
                    else
                    {
                        textBox_terminal.SelectionStart = pos;
                        textBox_terminal.ScrollToCaret();
                    }
                }
            }
        }

        #endregion

        #region Generate commands

        private void button_setMode_Click(object sender, EventArgs e)
        {
            //0: новый номер режима
            if (!StationSettings.StationMode.TryGetValue(comboBox_mode.SelectedItem.ToString(), out var mode))
                return;

            var setMode = Parser.setMode(mode);
            SendCommand(setMode);
        }

        private void button_setTime_Click(object sender, EventArgs e)
        {
            //0-5: дата и время[yy.mm.dd hh: mm:ss]
            var tmpTime = Helpers.DateStringToUnixTime(textBox_setTime.Text);
            var timeToSet = Helpers.ConvertFromUnixTimestamp(tmpTime);

            if (checkBox_autoTime.Checked)
            {
                timeToSet = DateTime.Now;
                textBox_setTime.Text = Helpers.DateToString(timeToSet);
            }

            var setTime = Parser.setTime(timeToSet);
            SendCommand(setTime);
        }

        private void button_resetStation_Click(object sender, EventArgs e)
        {
            //0-1: кол-во отмеченных карт (для сверки)
            ushort.TryParse(textBox_checkedChips.Text, out var chipsNumber);

            //2-5: время последней отметки unixtime(для сверки)
            var tmpTime = Helpers.DateStringToUnixTime(textBox_lastCheck.Text);
            var lastCheck = Helpers.ConvertFromUnixTimestamp(tmpTime);

            //6: новый номер станции
            byte.TryParse(textBox_newStationNumber.Text, out var newStationNumber);

            var resetStation = Parser.resetStation(chipsNumber, lastCheck, newStationNumber);
            SendCommand(resetStation);
        }

        private void button_getStatus_Click(object sender, EventArgs e)
        {
            var getStatus = Parser.getStatus();
            SendCommand(getStatus);
        }

        private void button_initChip_Click(object sender, EventArgs e)
        {
            /*0-1: номер команды
            2-3: маска участников*/
            ushort.TryParse(textBox_initTeamNum.Text, out var teamNumber);

            ushort mask = 0;
            byte j = 0;
            for (var i = 15; i >= 0; i--)
            {
                if (textBox_initMask.Text[i] == '1')
                    mask = (ushort)Accessory.SetBit(mask, j);
                else
                    mask = (ushort)Accessory.ClearBit(mask, j);
                j++;
            }

            var initChip = Parser.initChip(teamNumber, mask);
            SendCommand(initChip);
        }

        private void button_getLastTeam_Click(object sender, EventArgs e)
        {
            var getLastTeam = Parser.getLastTeam();
            SendCommand(getLastTeam);
        }

        private void button_getTeamRecord_Click(object sender, EventArgs e)
        {
            //0-1: какую запись
            ushort.TryParse(textBox_TeamNumber.Text, out var teamNumber);
            var getTeamRecord = Parser.getTeamRecord(teamNumber);
            SendCommand(getTeamRecord);
        }

        private void button_readCardPage_Click(object sender, EventArgs e)
        {
            //0: с какой страницу карты
            byte.TryParse(
                textBox_readChipPage.Text.Substring(0, textBox_readChipPage.Text.IndexOf("-", StringComparison.Ordinal)).Trim(), out var fromPage);
            //1: по какую страницу карты включительно
            byte.TryParse(textBox_readChipPage.Text.Substring(textBox_readChipPage.Text.IndexOf("-", StringComparison.Ordinal) + 1)
                .Trim(), out var toPage);

            var readCardPage = Parser.readCardPage(fromPage, toPage);
            SendCommand(readCardPage);
        }

        private void button_updateTeamMask_Click(object sender, EventArgs e)
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
                    mask = (ushort)Accessory.SetBit(mask, j);
                else
                    mask = (ushort)Accessory.ClearBit(mask, j);
                j++;
            }

            var updateTeamMask = Parser.updateTeamMask(teamNumber, issueTime, mask);
            SendCommand(updateTeamMask);
        }

        private void button_writeCardPage_Click(object sender, EventArgs e)
        {
            //0-7: UID чипа
            var uid = Accessory.ConvertHexToByteArray(textBox_uid.Text);
            if (uid.Length != 8)
                return;

            //8: номер страницы
            byte.TryParse(textBox_writeChipPage.Text, out var pageNumber);

            //9-12: данные из страницы карты (4 байта)
            var data = Accessory.ConvertHexToByteArray(textBox_data.Text);
            if (data.Length != 4)
                return;

            var writeCardPage = Parser.writeCardPage(uid, pageNumber, data);
            SendCommand(writeCardPage);
        }

        private void button_readFlash_Click(object sender, EventArgs e)
        {
            //0-3: адрес начала чтения
            uint.TryParse(textBox_readFlashAddress.Text, out var fromAddr);

            //4-5: размер блока
            ushort.TryParse(textBox_readFlashLength.Text, out var readLength);

            var readFlash = Parser.readFlash(fromAddr, readLength);
            SendCommand(readFlash);
        }

        private void button_writeFlash_Click(object sender, EventArgs e)
        {
            //0-3: адрес начала записи
            uint.TryParse(textBox_writeAddr.Text, out var startAddress);

            //4...: данные для записи
            var data = Accessory.ConvertHexToByteArray(textBox_flashData.Text);

            var writeFlash = Parser.writeFlash(startAddress, data);
            SendCommand(writeFlash);
        }

        private void button_eraseTeamFlash_Click(object sender, EventArgs e)
        {
            //0-1: какой сектор
            ushort.TryParse(textBox_TeamNumber.Text, out var teamNumber);

            var eraseTeamFlash = Parser.eraseTeamFlash(teamNumber);
            SendCommand(eraseTeamFlash);
        }

        private void button_getConfig_Click(object sender, EventArgs e)
        {
            var getConfig = Parser.getConfig();
            SendCommand(getConfig);
        }

        private void button_setVCoeff_Click(object sender, EventArgs e)
        {
            //0-3: коэффициент пересчета напряжения
            float.TryParse(textBox_koeff.Text, out _station.VoltageCoefficient);

            var setKoeff = Parser.setVCoeff(_station.VoltageCoefficient);
            SendCommand(setKoeff);
        }

        private void Button_setGain_Click(object sender, EventArgs e)
        {
            //0: новый коэфф.
            if (!StationSettings.Gain.TryGetValue(comboBox_setGain.SelectedItem.ToString(), out var gainValue))
                return;
            var setGain = Parser.setGain(gainValue);
            SendCommand(setGain);
        }

        private void button_setChipType_Click(object sender, EventArgs e)
        {
            //0: новый тип чипа
            var newChipType = RfidContainer.ChipTypes.SystemIds[_selectedChipType];
            var setChipType = Parser.setChipType(newChipType);
            SendCommand(setChipType);
        }

        private void Button_setTeamFlashSize_Click(object sender, EventArgs e)
        {
            //0-1: новый размер блока команды
            ushort.TryParse(textBox_teamFlashSize.Text, out _station.TeamBlockSize);

            var setTeamFlashSize = Parser.setTeamFlashSize(_station.TeamBlockSize);
            SendCommand(setTeamFlashSize);
        }

        private void Button_setEraseBlock_Click(object sender, EventArgs e)
        {//0-1: новый размер стираемого блока
            ushort.TryParse(textBox_eraseBlock.Text, out _station.EraseBlockSize);

            var setEraseBlockSize = Parser.setEraseBlock(_station.EraseBlockSize);
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
            float.TryParse(textBox_setBatteryLimit.Text, out _station.BatteryLimit);
            var setBatteryLimit = Parser.setBatteryLimit(_station.BatteryLimit);
            SendCommand(setBatteryLimit);
        }

        private void button_getTeamsList_Click(object sender, EventArgs e)
        {
            //0-1: начальный номер команды
            ushort.TryParse(textBox_TeamNumber.Text, out var teamNumber);

            var scanTeams = Parser.scanTeams(teamNumber);
            SendCommand(scanTeams);
        }

        private void button_sendBtCommand_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_sendBtCommand.Text))
                return;
            //0...: команда
            var sendBtCommand = Parser.sendBtCommand(textBox_sendBtCommand.Text + "\r\n");
            SendCommand(sendBtCommand);
        }

        private void button_getLastErrors_Click(object sender, EventArgs e)
        {
            var geLastErrors = Parser.getLastErrors();
            SendCommand(geLastErrors);
        }

        private void button_setAutoReport_Click(object sender, EventArgs e)
        {
            //0: новый режим автоответа
            var setMode = Parser.setAutoReport(checkBox_AutoReport.Checked);
            SendCommand(setMode);
        }

        #endregion

        #region Parse replies

        private void reply_setTime(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            //1-4: текущее время
            var replyDetails =
                new ProtocolParser.ReplyData.setTimeReply(reply);
            SetText(replyDetails.ToString());
        }

        private void reply_getStatus(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            //1-4: текущее время
            //5-6: количество отметок на станции
            //7-10: время последней отметки на станции
            //11-12: напряжение батареи в условных единицах[0..1023] ~ [0..1.1В]
            //13-14: температура чипа DS3231 (чуть выше окружающей среды)

            var replyDetails =
                new ProtocolParser.ReplyData.getStatusReply(reply);
            SetText(replyDetails.ToString());
            SetText("Battery voltage: " + (replyDetails.BatteryLevel * _station.VoltageCoefficient).ToString("F3") + " V.\r\n");

            Invoke((MethodInvoker)delegate
            {
                _station.Number = reply.StationNumber;
                Parser.StationNumber = _station.Number;
                textBox_stationNumber.Text = _station.Number.ToString();
            });
        }

        private void reply_initChip(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            // убрать 1-7: UID чипа

            var replyDetails =
                new ProtocolParser.ReplyData.initChipReply(reply);
            SetText(replyDetails.ToString());
        }

        private void reply_getLastTeams(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            //1-2: номер 1й команды
            //3-4: номер 2й команды
            //...
            //(n - 1) - n: номер последней команды
            var replyDetails =
                new ProtocolParser.ReplyData.getLastTeamsReply(reply);
            SetText(replyDetails.ToString());
        }

        private void reply_getTeamRecord(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            //1: данные отметившейся команды
            var replyDetails =
                new ProtocolParser.ReplyData.getTeamRecordReply(reply);
            SetText(replyDetails.ToString());

            var tmpTeam = new TeamsContainer.TeamData();
            tmpTeam.TeamNumber = replyDetails.TeamNumber;
            tmpTeam.InitTime = replyDetails.InitTime;
            tmpTeam.TeamMask = replyDetails.Mask;
            tmpTeam.LastCheckTime = replyDetails.LastMarkTime;
            tmpTeam.DumpSize = replyDetails.DumpSize;
            _teams.Add(tmpTeam);
        }

        private void reply_readCardPages(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            //1-7: UID чипа
            //8-11: данные из страницы карты(4 байта)
            var replyDetails =
                new ProtocolParser.ReplyData.readCardPageReply(reply);
            SetText(replyDetails.ToString());

            _rfidCard.AddPages(replyDetails.startPage, replyDetails.PagesData);
        }

        private void reply_readFlash(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            //1...: данные из флэша
            var replyDetails =
                new ProtocolParser.ReplyData.readFlashReply(reply);
            SetText(replyDetails.ToString());

            _stationFlash.Add(replyDetails.Address, replyDetails.Data);
        }

        private void reply_writeFlash(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            //1...: данные из флэша
            var replyDetails =
                new ProtocolParser.ReplyData.writeFlashReply(reply);
            SetText(replyDetails.ToString());
        }

        private void reply_getConfig(ProtocolParser.ReplyData reply)
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

            var replyDetails =
                new ProtocolParser.ReplyData.getConfigReply(reply);
            SetText(replyDetails.ToString());

            _station.Number = reply.StationNumber;
            Parser.StationNumber = _station.Number;

            _station.FwVersion = replyDetails.FwVersion;
            SetText("Режим: " + StationSettings.StationMode.FirstOrDefault(x => x.Value == replyDetails.Mode).Key + "\r\n");
            _station.Mode = replyDetails.Mode;

            if (replyDetails.ChipTypeId == 213)
                _station.ChipType = RfidContainer.ChipTypes.Types["NTAG213"];
            else if (replyDetails.ChipTypeId == 215)
                _station.ChipType = RfidContainer.ChipTypes.Types["NTAG215"];
            else if (replyDetails.ChipTypeId == 216)
                _station.ChipType = RfidContainer.ChipTypes.Types["NTAG216"];

            _station.FlashSize = replyDetails.FlashSize;
            if (_station.FlashSize < _stationFlash.Size)
            // check _selectedFlashSize
                RefreshFlashGrid(_selectedFlashSize, _station.TeamBlockSize, _bytesPerRow);

            _station.VoltageCoefficient = replyDetails.VoltageKoeff;
            _station.AntennaGain = replyDetails.AntennaGain;
            _station.TeamBlockSize = replyDetails.TeamBlockSize;
            _station.EraseBlockSize = replyDetails.EraseBlockSize;
            _station.BatteryLimit = replyDetails.BatteryLimit;
            _station.MaxPacketLength = replyDetails.MaxPacketLength;
            Parser.MaxPacketLength = _station.MaxPacketLength;
            _station.AutoReport = replyDetails.AutoreportMode;

            Invoke((MethodInvoker)delegate
            {
                textBox_stationNumber.Text = _station.Number.ToString();
                textBox_fwVersion.Text = _station.FwVersion.ToString();
                comboBox_mode.SelectedItem = StationSettings.StationMode.FirstOrDefault(x => x.Value == _station.Mode).Key;
                comboBox_chipType.SelectedIndex = _station.ChipType;
                textBox_flashSize.Text = (int)(_station.FlashSize / 1024 / 1024) + " Mb";
                // switch flash size combobox to new value if bigger than new FlashSize
                textBox_koeff.Text = _station.VoltageCoefficient.ToString("F5");
                comboBox_setGain.SelectedItem = StationSettings.Gain.FirstOrDefault(x => x.Value == _station.AntennaGain).Key;
                textBox_teamFlashSize.Text = _station.TeamBlockSize.ToString();
                textBox_eraseBlock.Text = _station.EraseBlockSize.ToString();
                textBox_setBatteryLimit.Text = _station.BatteryLimit.ToString("F3");
                checkBox_AutoReport.Checked = _station.AutoReport;
                textBox_packetLength.Text = _station.MaxPacketLength.ToString();
            });
        }

        private void reply_scanTeams(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            //1-2: номер 1й команды
            //3-4: номер 2й команды           
            //...	                        
            //(n - 1) - n: номер последней команды
            var replyDetails =
                new ProtocolParser.ReplyData.scanTeamsReply(reply);
            SetText(replyDetails.ToString());

            foreach (var n in replyDetails.TeamsList)
            {
                var tmpTeam = new TeamsContainer.TeamData();
                tmpTeam.TeamNumber = n;
                _teams.Add(tmpTeam);
            }

            if (replyDetails.TeamsList.Length * 2 < 252 - 7)
                _needMore = false;
        }

        private void reply_sendBtCommand(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            //1-n: ответ BT модуля
            var replyDetails =
                new ProtocolParser.ReplyData.sendBtCommandReply(reply);
            SetText("BT reply: " + replyDetails.ToString() + "\r\n");
        }

        private void reply_getLastErrors(ProtocolParser.ReplyData reply)
        {
            //0: код ошибки
            //1-2: номер 1й ошибки
            //3-4: номер 2й ошибки
            //...
            //(n - 1) - n: номер последней ошибки
            var replyDetails =
                new ProtocolParser.ReplyData.getLastErrorsReply(reply);
            SetText(replyDetails.ToString());
        }

        #endregion

        #region Helpers

        private void RefreshFlashGrid(uint flashSize, uint teamDumpSize, uint bytesPerRow)
        {
            _stationFlash = new FlashContainer(flashSize, teamDumpSize, bytesPerRow);
            dataGridView_flashRawData.DataSource = _stationFlash.Table;
            dataGridView_flashRawData.AutoGenerateColumns = true;
            //dataGridView_flashRawData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView_flashRawData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dataGridView_flashRawData.AutoResizeColumns();
            dataGridView_flashRawData.ScrollBars = ScrollBars.Both;
            dataGridView_flashRawData.AllowUserToResizeColumns = true;
            dataGridView_flashRawData.AllowUserToOrderColumns = false;
            for (var i = 0; i < dataGridView_flashRawData.Columns.Count; i++) dataGridView_flashRawData.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;

            var rawColumnFont = new Font(dataGridView_flashRawData.DefaultCellStyle.Font.FontFamily, 10, FontStyle.Regular);
            dataGridView_flashRawData.Columns[FlashContainer.TableColumns.RawData].DefaultCellStyle.Font =
                rawColumnFont;
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
            for (var i = 0; i < dataGridView_chipRawData.Columns.Count; i++) dataGridView_chipRawData.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
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
            for (var i = 0; i < dataGridView_teams.Columns.Count; i++) dataGridView_teams.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        #endregion

        #region GUI

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _logAutoSaveFile = Settings.Default.LogAutoSaveFile;
            if (_logAutoSaveFile != "")
                _logAutoSaveFlag = true;
            _logLinesLimit = Settings.Default.LogLinesLimit;
            _portSpeed = Settings.Default.BaudRate;
            serialPort1.Encoding = Encoding.GetEncoding(INPUT_CODE_PAGE);
            //Serial init
            comboBox_portName.Items.Add("None");
            foreach (var portname in SerialPort.GetPortNames()) comboBox_portName.Items.Add(portname); //добавить порт в список
            if (comboBox_portName.Items.Count == 1)
            {
                comboBox_portName.SelectedIndex = 0;
                button_openPort.Enabled = false;
            }
            else
            {
                comboBox_portName.SelectedIndex = comboBox_portName.Items.Count - 1;
            }

            foreach (var item in StationSettings.StationMode) comboBox_mode.Items.Add(item.Key);

            foreach (var item in StationSettings.Gain) comboBox_setGain.Items.Add(item.Key);

            foreach (var item in RfidContainer.ChipTypes.Types) comboBox_chipType.Items.Add(item.Key);

            foreach (var item in FlashSizeLimit) comboBox_flashSize.Items.Add(item.Key);

            textBox_setTime.Text = Helpers.DateToString(DateTime.Now.ToUniversalTime());
            comboBox_mode.SelectedIndex = 0;
            comboBox_setGain.SelectedIndex = 0;

            _station = new StationSettings();

            Parser = new ProtocolParser();

            _teams = new TeamsContainer();
            RefreshTeamsGrid();

            _rfidCard = new RfidContainer(_selectedChipType);
            RefreshChipGrid(_station.ChipType);

            _stationFlash = new FlashContainer(_selectedFlashSize, _station.TeamBlockSize, _bytesPerRow);
            RefreshFlashGrid(_selectedFlashSize, _station.TeamBlockSize, _bytesPerRow);

            textBox_flashSize.Text = (int)(_station.FlashSize / 1024 / 1024) + " Mb";
            textBox_teamFlashSize.Text = _station.TeamBlockSize.ToString();

            comboBox_flashSize.SelectedIndex = 0;
            comboBox_chipType.SelectedIndex = 1;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
        }

        private void checkBox_autoTime_CheckedChanged(object sender, EventArgs e)
        {
            textBox_setTime.Enabled = !checkBox_autoTime.Checked;
            textBox_setTime.Text = Helpers.DateToString(DateTime.Now.ToUniversalTime());
        }

        private void textBox_stationNumber_Leave(object sender, EventArgs e)
        {
            byte.TryParse(textBox_stationNumber.Text, out _station.Number);
            Parser.StationNumber = _station.Number;
            textBox_stationNumber.Text = _station.Number.ToString();
        }

        private void textBox_teamMask_Leave(object sender, EventArgs e)
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

        private void textBox_setTime_Leave(object sender, EventArgs e)
        {
            var t = Helpers.DateStringToUnixTime(textBox_setTime.Text);
            textBox_setTime.Text = Helpers.DateToString(Helpers.ConvertFromUnixTimestamp(t));
        }

        private void textBox_newStationNumber_Leave(object sender, EventArgs e)
        {
            byte.TryParse(textBox_newStationNumber.Text, out var n);
            textBox_newStationNumber.Text = n.ToString();
        }

        private void textBox_checkedChips_Leave(object sender, EventArgs e)
        {
            ushort.TryParse(textBox_checkedChips.Text, out var n);
            textBox_checkedChips.Text = n.ToString();
        }

        private void textBox_lastCheck_Leave(object sender, EventArgs e)
        {
            var t = Helpers.DateStringToUnixTime(textBox_lastCheck.Text);
            textBox_lastCheck.Text = Helpers.DateToString(Helpers.ConvertFromUnixTimestamp(t));
        }

        private void textBox_issueTime_Leave(object sender, EventArgs e)
        {
            var t = Helpers.DateStringToUnixTime(textBox_issueTime.Text);
            textBox_issueTime.Text = Helpers.DateToString(Helpers.ConvertFromUnixTimestamp(t));
        }

        private void textBox_readChipPage_Leave(object sender, EventArgs e)
        {
            if (!textBox_readChipPage.Text.Contains('-'))
                textBox_readChipPage.Text = "0-" + textBox_readChipPage.Text;
            ushort.TryParse(textBox_readChipPage.Text.Substring(0, textBox_readChipPage.Text.IndexOf("-", StringComparison.Ordinal)).Trim(), out var from);
            ushort.TryParse(textBox_readChipPage.Text.Substring(textBox_readChipPage.Text.IndexOf("-", StringComparison.Ordinal) + 1).Trim(), out var to);
            if (to - from > (_station.MaxPacketLength - 7 - ProtocolParser.ReplyDataLength.READ_CARD_PAGE) / 4)
                to = (ushort)((_station.MaxPacketLength - 7 - ProtocolParser.ReplyDataLength.READ_CARD_PAGE) / 4);
            textBox_readChipPage.Text = from + "-" + to;
        }

        private void textBox_writeChipPage_Leave(object sender, EventArgs e)
        {
            byte.TryParse(textBox_writeChipPage.Text, out var n);
            textBox_writeChipPage.Text = n.ToString();
        }

        private void textBox_data_Leave(object sender, EventArgs e)
        {
            textBox_data.Text = Accessory.CheckHexString(textBox_data.Text);
            var n = Accessory.ConvertHexToByteArray(textBox_data.Text);
            textBox_data.Text = Accessory.ConvertByteArrayToHex(n, 4);
        }

        private void textBox_uid_Leave(object sender, EventArgs e)
        {
            textBox_uid.Text = Accessory.CheckHexString(textBox_uid.Text);
            var n = Accessory.ConvertHexToByteArray(textBox_uid.Text);
            textBox_uid.Text = Accessory.ConvertByteArrayToHex(n);
            if (textBox_uid.Text.Length > 24)
                textBox_uid.Text = textBox_uid.Text.Substring(0, 24);
            else if (textBox_uid.Text.Length < 24)
                while (textBox_uid.Text.Length < 24)
                    textBox_uid.Text = "00 " + textBox_uid.Text;
        }

        private void textBox_readFlash_Leave(object sender, EventArgs e)
        {
            long.TryParse(textBox_readFlashAddress.Text, out var from);
            textBox_readFlashAddress.Text = from.ToString();
        }

        private void textBox_writeAddr_Leave(object sender, EventArgs e)
        {
            uint.TryParse(textBox_writeAddr.Text, out var n);
            textBox_writeAddr.Text = n.ToString();
        }

        private void textBox_flashData_Leave(object sender, EventArgs e)
        {
            textBox_flashData.Text = Accessory.CheckHexString(textBox_flashData.Text);
            var n = Accessory.ConvertHexToByteArray(textBox_flashData.Text);
            textBox_flashData.Text = Accessory.ConvertByteArrayToHex(n, _station.MaxPacketLength - ProtocolParser.CommandDataLength.WRITE_FLASH);
        }

        private void tabControl_teamData_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl_teamData.SelectedIndex == 0 && checkBox_autoScroll.Checked)
            {
                textBox_terminal.SelectionStart = textBox_terminal.Text.Length;
                textBox_terminal.ScrollToCaret();
            }
        }

        private void textBox_koeff_Leave(object sender, EventArgs e)
        {
            textBox_koeff.Text =
                textBox_koeff.Text.Replace('.', ',');
            float.TryParse(textBox_koeff.Text, out var koeff);
            textBox_koeff.Text = koeff.ToString("F5");
        }

        private void button_clearLog_Click(object sender, EventArgs e)
        {
            textBox_terminal.Clear();
        }

        private void button_clearTeams_Click(object sender, EventArgs e)
        {
            RefreshTeamsGrid();
        }

        private void button_clearRfid_Click(object sender, EventArgs e)
        {
            RefreshChipGrid(_station.ChipType);
        }

        private void button_clearFlash_Click(object sender, EventArgs e)
        {
            RefreshFlashGrid(_selectedFlashSize, _station.TeamBlockSize, _bytesPerRow);
        }

        private void button_saveLog_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "station_" + _station.Number.ToString() + ".log";
            saveFileDialog1.Title = "Save log to file";
            saveFileDialog1.DefaultExt = "txt";
            saveFileDialog1.Filter = "Text files|*.txt|All files|*.*";
            saveFileDialog1.ShowDialog();
        }

        private void button_saveTeams_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "station_" + _station.Number.ToString() + "_teams.csv";
            saveFileDialog1.Title = "Save teams to file";
            saveFileDialog1.DefaultExt = "csv";
            saveFileDialog1.Filter = "CSV files|*.csv";
            saveFileDialog1.ShowDialog();
        }

        private void button_saveRfid_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "uid_" + dataGridView_chipRawData.Rows[0].Cells[2].Value + dataGridView_chipRawData.Rows[1].Cells[2].Value.ToString().Trim() + ".bin";
            saveFileDialog1.FileName = saveFileDialog1.FileName.Replace(' ', '_');
            saveFileDialog1.Title = "Save card dump to file";
            saveFileDialog1.DefaultExt = "bin";
            saveFileDialog1.Filter = "Binary files|*.bin|CSV files|*.csv";
            saveFileDialog1.ShowDialog();
        }

        private void button_saveFlash_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "station_" + _station.Number.ToString() + "_flash.bin";
            saveFileDialog1.Title = "Save flash dump to file";
            saveFileDialog1.DefaultExt = "txt";
            saveFileDialog1.Filter = "Binary files|*.bin|CSV files|*.csv";
            saveFileDialog1.ShowDialog();

        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
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
                    sb.AppendLine(string.Join(",", headers.Select(column => "\"" + column.ColumnName + "\"").ToArray()));

                    foreach (DataRow page in _rfidCard.Table.Rows)
                    {
                        for (var i = 0; i < page.ItemArray.Count(); i++) sb.Append(page.ItemArray[i].ToString() + ";");
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
                    sb.AppendLine(string.Join(",", headers.Select(column => "\"" + column.ColumnName + "\"").ToArray()));

                    foreach (DataRow team in _stationFlash.Table.Rows)
                    {
                        for (var i = 0; i < team.ItemArray.Count(); i++) sb.Append(team.ItemArray[i].ToString() + ";");
                        sb.AppendLine();
                    }
                    File.WriteAllText(saveFileDialog1.FileName, sb.ToString());
                }
            }
        }

        private void button_dumpTeams_Click(object sender, EventArgs e)
        {
            button_dumpTeams.Enabled = false;
            button_getTeamRecord.Enabled = false;
            RefreshTeamsGrid();

            var maxTeams = (ushort)(_stationFlash.Size / _station.TeamBlockSize);

            // get list of commands in flash
            ushort teamNum = 1;
            _noTerminalOutputFlag = true;
            _asyncFlag = 0;
            _needMore = false;
            var startTime = DateTime.Now.ToUniversalTime();
            do
            {
                //0-1: какую запись
                var scanTeams = Parser.scanTeams(teamNum);
                _asyncFlag++;
                SendCommand(scanTeams);

                long timeout = 1000;
                while (_asyncFlag > 0)
                {
                    Accessory.Delay_ms(1);
                    if (timeout <= 0)
                        break;
                    timeout--;
                }

                if (dataGridView_teams.RowCount == 0 || !ushort.TryParse(dataGridView_teams.Rows[dataGridView_teams.RowCount - 1].Cells[0].Value.ToString(),
                    out teamNum))
                {
                    _noTerminalOutputFlag = false;
                    button_dumpTeams.Enabled = true;
                    button_getTeamRecord.Enabled = true;
                    return;
                }
                if (!_needMore)
                    teamNum = maxTeams;

            } while (teamNum < maxTeams);
            SetText("\r\nTeams list time=" +
                    DateTime.Now.ToUniversalTime().Subtract(startTime).TotalMilliseconds + " ms.\r\n");

            // load every command data
            var rowNum = 0;
            _noTerminalOutputFlag = true;
            _asyncFlag = 0;
            startTime = DateTime.Now.ToUniversalTime();
            while (rowNum < dataGridView_teams.RowCount)
            {
                if (!ushort.TryParse(
                        dataGridView_teams.Rows[rowNum].Cells[0].Value.ToString(),
                        out teamNum))
                    break;

                //0-1: какую запись
                var getTeamRecord = Parser.getTeamRecord(teamNum);
                _asyncFlag++;
                SendCommand(getTeamRecord);
                rowNum++;

                long timeout = 1000;
                while (_asyncFlag > 0)
                {
                    Accessory.Delay_ms(1);
                    if (timeout <= 0)
                        break;
                    timeout--;
                }
            }

            SetText("\r\nTeams dump time=" +
                    DateTime.Now.ToUniversalTime().Subtract(startTime).TotalMilliseconds + " ms.\r\n");

            dataGridView_teams.Refresh();
            dataGridView_teams.PerformLayout();
            dataGridView_teams.Invalidate();

            _noTerminalOutputFlag = false;
            button_dumpTeams.Enabled = true;
            button_getTeamRecord.Enabled = true;
        }

        private void button_dumpChip_Click(object sender, EventArgs e)
        {
            RefreshChipGrid(_station.ChipType);
            button_dumpChip.Enabled = false;
            button_readChipPage.Enabled = false;

            var chipSize = RfidContainer.ChipTypes.PageSizes[_rfidCard.CurrentChipType];
            byte maxFramePages = 45;
            ushort pagesFrom = 0;
            ushort pagesTo;
            _noTerminalOutputFlag = true;
            _asyncFlag = 0;
            var startTime = DateTime.Now.ToUniversalTime();
            do
            {
                pagesTo = (ushort)(pagesFrom + maxFramePages - 1);
                if (pagesTo >= chipSize)
                    pagesTo = (ushort)(chipSize - 1);

                //0: с какой страницу карты
                //1: по какую страницу карты включительно
                var readCardPage = Parser.readCardPage((byte)pagesFrom, (byte)pagesTo);
                _asyncFlag++;
                SendCommand(readCardPage);
                pagesFrom = (byte)(pagesTo + 1);
                long timeout = 1000;
                while (_asyncFlag > 0)
                {
                    Accessory.Delay_ms(1);
                    if (timeout <= 0)
                        break;
                    timeout--;
                }
            } while (pagesTo < chipSize - 1);
            SetText("\r\nRFID dump time=" +
                    DateTime.Now.ToUniversalTime().Subtract(startTime).TotalMilliseconds +
                    " ms.\r\n");

            dataGridView_chipRawData.Refresh();
            dataGridView_chipRawData.PerformLayout();

            _noTerminalOutputFlag = false;
            button_dumpChip.Enabled = true;
            button_readChipPage.Enabled = true;
        }

        private void button_dumpFlash_Click(object sender, EventArgs e)
        {
            RefreshFlashGrid(_selectedFlashSize, _station.TeamBlockSize, _bytesPerRow);
            button_dumpFlash.Enabled = false;
            button_readFlash.Enabled = false;

            var maxFrameBytes = (ushort)(_station.MaxPacketLength - 7 - ProtocolParser.ReplyDataLength.READ_FLASH - 1);
            var currentRow = 0;
            uint addrFrom = 0;
            uint addrTo;
            _noTerminalOutputFlag = true;
            _asyncFlag = 0;
            var startTime = DateTime.Now.ToUniversalTime();
            do
            {
                addrTo = addrFrom + maxFrameBytes;
                if (addrTo >= _stationFlash.Size)
                    addrTo = _stationFlash.Size;

                var readFlash = Parser.readFlash(addrFrom, (ushort)(addrTo - addrFrom));
                _asyncFlag++;
                SendCommand(readFlash);
                addrFrom = addrTo;

                long timeout = 1000;
                while (_asyncFlag > 0)
                {
                    Accessory.Delay_ms(1);
                    if (timeout <= 0)
                        break;
                    timeout--;
                }
                //check if it's time to update grid
                if ((int)(addrFrom / _bytesPerRow) > currentRow) currentRow = (int)(addrFrom / _bytesPerRow);
            } while (addrTo < _stationFlash.Size);

            SetText("\r\nFlash dump time=" +
                    DateTime.Now.ToUniversalTime().Subtract(startTime).TotalMilliseconds + " ms.\r\n");

            _noTerminalOutputFlag = false;
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
            var uid = Accessory.ConvertHexToByteArray(textBox_uid.Text);

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
                var writeCardPage = Parser.writeCardPage(uid, page, data);
                _asyncFlag++;
                SendCommand(writeCardPage);

                long timeout = 1000;
                while (_asyncFlag > 0)
                {
                    Accessory.Delay_ms(1);
                    if (timeout <= 0)
                        break;
                    timeout--;
                }
            }
            SetText("\r\nRFID clear time=" +
                    DateTime.Now.ToUniversalTime().Subtract(startTime).TotalMilliseconds + " ms.\r\n");
            button_eraseChip.Enabled = true;
        }

        private void button_quickDump_Click(object sender, EventArgs e)
        {
            button_quickDump.Enabled = false;
            button_dumpFlash.Enabled = false;

            button_dumpTeams_Click(this, EventArgs.Empty);

            // load every command data
            ushort rowNum = 0;
            _noTerminalOutputFlag = true;
            _asyncFlag = 0;
            var startTime = DateTime.Now.ToUniversalTime();
            while (rowNum < dataGridView_teams.RowCount)
            {
                if (!ushort.TryParse(
                    dataGridView_teams.Rows[rowNum].Cells[0].Value.ToString(),
                    out var teamNum) || teamNum >= dataGridView_flashRawData.RowCount)
                    break;
                dataGridView_flashRawData_CellDoubleClick(this, new DataGridViewCellEventArgs(0, teamNum));
                rowNum++;
            }

            SetText("\r\nTeams dump time=" +
                    DateTime.Now.ToUniversalTime().Subtract(startTime).TotalMilliseconds + " ms.\r\n");

            dataGridView_teams.Refresh();
            dataGridView_teams.PerformLayout();

            _noTerminalOutputFlag = false;
            button_quickDump.Enabled = true;
            button_dumpFlash.Enabled = true;
        }

        private void dataGridView_teams_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!serialPort1.IsOpen || e.ColumnIndex < 0 || e.RowIndex < 0)
                return;

            //0-1: какую запись
            ushort.TryParse(dataGridView_teams?.Rows[e.RowIndex].Cells[0]?.Value?.ToString(), out var teamNum);
            if (teamNum <= 0)
                return;

            _asyncFlag = 0;
            var getTeamRecord = Parser.getTeamRecord(teamNum);
            _asyncFlag++;
            SendCommand(getTeamRecord);

            long timeout = 1000;
            while (_asyncFlag > 0)
            {
                Accessory.Delay_ms(1);
                if (timeout <= 0)
                    break;
                timeout--;
            }

            dataGridView_teams.Refresh();
            dataGridView_teams.PerformLayout();
        }

        private void dataGridView_chipRawData_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!serialPort1.IsOpen || e.ColumnIndex < 0 || e.RowIndex < 0)
                return;

            var page = (byte)e.RowIndex;

            //0: с какой страницу карты
            //1: по какую страницу карты включительно
            var readCardPage = Parser.readCardPage(page, page);
            _asyncFlag = 0;
            _asyncFlag++;
            SendCommand(readCardPage);

            long timeout = 1000;
            while (_asyncFlag > 0)
            {
                Accessory.Delay_ms(1);
                if (timeout <= 0)
                    break;
                timeout--;
            }
            dataGridView_chipRawData.Refresh();
            dataGridView_chipRawData.PerformLayout();
        }

        private void dataGridView_flashRawData_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!serialPort1.IsOpen || e.ColumnIndex < 0 || e.RowIndex < 0)
                return;
            button_dumpFlash.Enabled = false;

            var rowFrom = (ushort)e.RowIndex;
            var maxFrameBytes = (ushort)(_station.MaxPacketLength - 7 - ProtocolParser.ReplyDataLength.READ_FLASH - 1);
            var addrFrom = rowFrom * _bytesPerRow;
            uint addrTo;
            var flashSize = addrFrom + _bytesPerRow;
            _asyncFlag = 0;
            do
            {
                addrTo = addrFrom + maxFrameBytes;
                if (addrTo >= flashSize)
                    addrTo = flashSize;

                var readFlash = Parser.readFlash(addrFrom, (ushort)(addrTo - addrFrom));
                _asyncFlag++;
                SendCommand(readFlash);
                addrFrom = addrTo;

                long timeout = 1000;
                while (_asyncFlag > 0)
                {
                    Accessory.Delay_ms(1);
                    if (timeout <= 0)
                        break;
                    timeout--;
                }
            } while (addrTo < flashSize);

            dataGridView_flashRawData.Refresh();
            dataGridView_flashRawData.PerformLayout();

            button_dumpFlash.Enabled = true;
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

            if (_selectedFlashSize > _station.FlashSize)
            {
                _selectedFlashSize = _station.FlashSize;
                comboBox_flashSize.SelectedIndex--;
            }
            RefreshFlashGrid(_selectedFlashSize, _station.TeamBlockSize, _bytesPerRow);
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
            if (toAddr > _station.MaxPacketLength - 7 - ProtocolParser.ReplyDataLength.READ_FLASH - 1)
                toAddr = (ushort)(_station.MaxPacketLength - 7 - ProtocolParser.ReplyDataLength.READ_FLASH - 1);
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
                textBox_setBatteryLimit.Text.Replace('.', ',');
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
                    _station.ChipType = RfidContainer.ChipTypes.Types["NTAG213"];
                else if (data[14] == 0x3e)
                    _station.ChipType = RfidContainer.ChipTypes.Types["NTAG215"];
                else if (data[14] == 0x6d)
                    _station.ChipType = RfidContainer.ChipTypes.Types["NTAG216"];
                else
                    return;

                RefreshChipGrid(_station.ChipType);

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
                RefreshFlashGrid((uint)data.Length, _station.TeamBlockSize, _bytesPerRow);
                _stationFlash.Add(0, data);
            }
        }

        private void textBox_getTeamsList_Leave(object sender, EventArgs e)
        {
            ushort.TryParse(textBox_TeamNumber.Text, out var n);
            textBox_TeamNumber.Text = n.ToString();
        }

        private void checkBox_AutoReport_CheckedChanged(object sender, EventArgs e)
        {
            _station.AutoReport = checkBox_AutoReport.Checked;
        }

        #endregion

    }
}
