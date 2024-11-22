using Android.Content;
using Android.OS;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RfidStationControl
{
    public class GlobalOperationsIdClass
    {
        public static readonly Dictionary<string, uint> FlashSizeLimitDictionary = new Dictionary<string, uint>
        {
            {"32 kb", 32*1024},
            {"64 kb", 32*1024},
            {"128 kb", 32*1024},
            {"256 kb", 32*1024},
            {"512 kb", 32*1024},
            {"1 Mb", 1*1024*1024},
            {"2 Mb", 1*1024*1024},
            {"4 Mb", 1*1024*1024},
            {"8 Mb", 1*1024*1024}
        };
        public static readonly uint FlashSizeLimit = 32 * 1024;

        public static BtConnector Bt;

        public static ProtocolParser Parser = new ProtocolParser(StationSettings.Number);

        public static FlashContainer Flash = new FlashContainer(FlashSizeLimit, StationSettings.TeamBlockSize);

        public static RfidContainer Rfid = new RfidContainer(StationSettings.ChipType);

        public static readonly TeamsContainer Teams = new TeamsContainer();

        public static volatile byte TimerActiveTasks;
        private const int BT_READ_PERIOD = 500;
        public static volatile bool DumpCancellation;

        // Write data to the BtDevice
        public static async Task<bool> SendToBtAsync(byte[] outBuffer, Context currentContext, Func<Task<bool>> callBack)
        {
            if (!await Bt.WriteBtAsync(outBuffer))
            {
                Toast.MakeText(currentContext, "Can't write to Bluetooth.", ToastLength.Long)?.Show();

                return false;
            }

            TimerActiveTasks++;
            StartTimer(BT_READ_PERIOD, callBack);
            StatusPageState.TerminalText.Append(">> " + Helpers.ConvertByteArrayToHex(outBuffer) + System.Environment.NewLine);

            return true;
        }

        private static void StartTimer(long gap, Func<Task<bool>> callback)
        {
            var ml = Looper.MainLooper;
            if (ml == null)
                return;

            var handler = new Handler(ml);
            handler.PostDelayed(() =>
            {
                if (Bt.BtInputBuffer.Count > 0)
                    callback();

                if (TimerActiveTasks > 0)
                    StartTimer(gap, callback);

                handler.Dispose();
                handler = null;
            }, gap);
        }

        // need to implement fully
        private async Task<bool> ReadBt()
        {
            bool packageReceived;
            lock (Bt.SerialReceiveThreadLock)
            {
                packageReceived = Parser.AddData(Bt.BtInputBuffer);
            }

            if (Parser._repliesList.Count <= 0)
                return packageReceived;

            for (var n = 0; n < Parser._repliesList.Count; n++)
            {
                var reply = Parser._repliesList[n];
                TimerActiveTasks--;
                if (reply.ReplyCode != 0)
                {
                    StatusPageState.TerminalText.Append(reply);

                    if (reply.ErrorCode == 0)
                    {
                        if (reply.ReplyCode == ProtocolParser.Reply.GET_STATUS)
                        {
                            var replyDetails = new ProtocolParser.ReplyData.GetStatusReply(reply);
                            StatusPageState.TerminalText.Append(replyDetails);

                            if (reply.StationNumber != StationSettings.Number)
                            {
                                StationSettings.Number = reply.StationNumber;
                                Parser =
                                    new ProtocolParser(StationSettings.Number);
                            }
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.GET_CONFIG)
                        {
                            var replyDetails = new ProtocolParser.ReplyData.GetConfigReply(reply);
                            StatusPageState.TerminalText.Append(replyDetails);

                            if (reply.StationNumber != StationSettings.Number)
                            {
                                StationSettings.Number = reply.StationNumber;
                                Parser = new ProtocolParser(StationSettings.Number);
                            }

                            if (replyDetails.ChipTypeId == 213)
                                StationSettings.ChipType = RfidContainer.ChipTypes.Types["NTAG213"];
                            else if (replyDetails.ChipTypeId == 215)
                                StationSettings.ChipType = RfidContainer.ChipTypes.Types["NTAG215"];
                            else if (replyDetails.ChipTypeId == 216)
                                StationSettings.ChipType = RfidContainer.ChipTypes.Types["NTAG216"];

                            if (StationSettings.ChipType != Rfid.ChipType)
                                Rfid = new RfidContainer(StationSettings.ChipType);

                            StationSettings.AntennaGain = replyDetails.AntennaGain;

                            StationSettings.Mode = replyDetails.Mode;

                            StationSettings.BatteryLimit = replyDetails.BatteryLimit;

                            StationSettings.EraseBlockSize = replyDetails.EraseBlockSize;

                            StationSettings.FlashSize = replyDetails.FlashSize;

                            StationSettings.MaxPacketLength = replyDetails.MaxPacketLength;

                            StationSettings.TeamBlockSize = replyDetails.TeamBlockSize;
                            if (StationSettings.TeamBlockSize != Flash.TeamDumpSize)
                                Flash = new FlashContainer(FlashSizeLimit,
                                    StationSettings.TeamBlockSize,
                                    0);

                            StationSettings.VoltageCoefficient = replyDetails.VoltageKoeff;
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.SET_TIME)
                        {
                            var replyDetails = new ProtocolParser.ReplyData.SetTimeReply(reply);
                            StatusPageState.TerminalText.Append(replyDetails);
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.SEND_BT_COMMAND)
                        {
                            var replyDetails = new ProtocolParser.ReplyData.SendBtCommandReply(reply);
                            StatusPageState.TerminalText.Append(replyDetails);
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.GET_LAST_TEAMS)
                        {
                            var replyDetails = new ProtocolParser.ReplyData.GetLastTeamsReply(reply);
                            StatusPageState.TerminalText.Append(replyDetails);
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.SCAN_TEAMS)
                        {
                            var replyDetails = new ProtocolParser.ReplyData.ScanTeamsReply(reply);
                            StatusPageState.TerminalText.Append(replyDetails);
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.GET_TEAM_RECORD)
                        {
                            var replyDetails = new ProtocolParser.ReplyData.GetTeamRecordReply(reply);
                            StatusPageState.TerminalText.Append(replyDetails);

                            var team = new TeamData
                            {
                                LastCheckTime = replyDetails.LastMarkTime,
                                DumpSize = replyDetails.DumpSize,
                                InitTime = replyDetails.InitTime,
                                TeamMask = replyDetails.Mask,
                                TeamNumber = replyDetails.TeamNumber
                            };
                            Teams.Add(team);
                            var tmp = Teams.GetTablePage(replyDetails.TeamNumber);
                            var row = new TeamsTableItem
                            {
                                TeamNum = tmp[0],
                                InitTime = tmp[1],
                                CheckTime = tmp[2],
                                Mask = tmp[3],
                                DumpSize = tmp[4]
                            };

                            var flag = false;
                            for (var i = 0; i < TeamsPageState.Table?.Count; i++)
                            {
                                if (TeamsPageState.Table[i].TeamNum != row.TeamNum)
                                    continue;

                                TeamsPageState.Table.RemoveAt(i);
                                TeamsPageState.Table.Insert(i, row);
                                flag = true;

                                break;
                            }

                            if (!flag)
                                TeamsPageState.Table?.Add(row);
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.INIT_CHIP)
                        {
                            var replyDetails = new ProtocolParser.ReplyData.InitChipReply(reply);
                            StatusPageState.TerminalText.Append(replyDetails);
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.READ_CARD_PAGE)
                        {
                            var replyDetails = new ProtocolParser.ReplyData.ReadCardPageReply(reply);
                            StatusPageState.TerminalText.Append(replyDetails);

                            Rfid.AddPages(replyDetails.startPage, replyDetails.PagesData);
                            // refresh RFID table
                            var endPage = replyDetails.startPage + replyDetails.PagesData.Length / 4;
                            for (var i = replyDetails.startPage; i < endPage; i++)
                            {
                                var tmp = Rfid.GetTablePage(i);
                                var row = new RfidTableItem
                                { PageNum = tmp[0], Data = tmp[1], PageDesc = tmp[2], Decoded = tmp[3] };

                                var flag = false;
                                for (var j = 0; j < RfidPageState.Table?.Count; j++)
                                {
                                    if (RfidPageState.Table[j].PageNum != row.PageNum)
                                        continue;

                                    RfidPageState.Table.RemoveAt(j);
                                    RfidPageState.Table.Insert(j, row);
                                    flag = true;

                                    break;
                                }

                                if (!flag)
                                    RfidPageState.Table?.Add(row);
                            }
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.READ_FLASH)
                        {
                            var replyDetails = new ProtocolParser.ReplyData.ReadFlashReply(reply);
                            StatusPageState.TerminalText.Append(replyDetails);

                            Flash.Add(replyDetails.Address, replyDetails.Data);
                            // refresh flash table
                            var startPage = (int)(replyDetails.Address / Flash.TeamDumpSize);
                            var endPage = (int)((replyDetails.Address + replyDetails.Data.Length) /
                                                Flash.TeamDumpSize);
                            for (var i = startPage; i <= endPage; i++)
                            {
                                var tmp = Flash.GetTablePage(i);
                                var row = new FlashTableItem { TeamNum = tmp[0], Decoded = tmp[2] };

                                var flag = false;
                                for (var j = 0; j < FlashPageState.Table?.Count; j++)
                                {
                                    if (FlashPageState.Table[j].TeamNum != row.TeamNum)
                                        continue;

                                    FlashPageState.Table.RemoveAt(j);
                                    FlashPageState.Table.Insert(j, row);
                                    flag = true;

                                    break;
                                }
                                if (!flag)
                                    FlashPageState.Table?.Add(row);
                            }
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.WRITE_FLASH)
                        {
                            var replyDetails = new ProtocolParser.ReplyData.WriteFlashReply(reply);
                            StatusPageState.TerminalText.Append(replyDetails);
                        }
                    }

                    Parser._repliesList.Remove(reply);
                }
                else
                {
                    StatusPageState.TerminalText.Append(reply.Message);
                }
            }

            return packageReceived;
        }
    }
}