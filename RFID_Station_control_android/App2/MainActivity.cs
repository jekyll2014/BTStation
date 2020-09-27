using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RfidStationControl
{
    public class GlobalOperationsIdClass
    {
        public static GlobalOperationsIdClass GetInstance()
        {
            if (_instance == null) _instance = new GlobalOperationsIdClass();
            return _instance;
        }

        private static GlobalOperationsIdClass _instance;

        public static class StationSettings
        {
            public static byte FwVersion = 0;
            public static byte Number = 0;
            public static byte Mode = 0;
            public static uint packetLengthkSize = 255;
            public static float VoltageCoefficient = 0.00578F;
            public static float BatteryLimit = 3.0F;
            public static byte AntennaGain = 80;
            public static byte ChipType = RfidContainer.ChipTypes.Types["NTAG215"];
            public static uint FlashSize = 1 * 1024 * 1024;
            public static uint TeamBlockSize = 1024;
            public static uint EraseBlockSize = 4096;
            public const string BtName = "Sportduino-xx";
            public const string BtPin = "1111";
            public const string BtCommand = "AT";
        }

        //режимы станции
        public static readonly Dictionary<string, byte> StationMode = new Dictionary<string, byte>
            {
                {"Init" , 0},
                {"Start" , 1},
                {"Finish" , 2}
            };

        //усиление антенны
        public static readonly Dictionary<string, byte> Gain = new Dictionary<string, byte>
            {
                {"Level 0", 0},
                {"Level 16", 16},
                {"Level 32", 32},
                {"Level 48", 48},
                {"Level 64", 64},
                {"Level 80", 80},
                {"Level 96", 96},
                {"Level 112", 112}
            };

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
        public static uint FlashSizeLimit = 32 * 1024;

        public static class StatusPageState
        {
            public static ushort CheckedChipsNumber = 0;
            public static byte NewStationNumber = 0;
            public static DateTime LastCheck = new DateTime();
            public static readonly StringBuilder TerminalText = new StringBuilder();
        }

        public static class ConfigPageState
        {
            public static bool UseCurrentTime = true;
            public static DateTime SetTime = DateTime.Now;
            public static uint FlashLimitSize = 32 * 1024;
        }

        public static class FlashPageState
        {
            public static uint ReadAddress = 0;
            public static byte ReadLength = 0;
            public static uint WriteAddress = 0;
            public static byte[] WriteData = new byte[0];

            public class FlashTableItem
            {
                public string TeamNum { get; set; }
                public string Decoded { get; set; }
            }

            public static readonly List<FlashTableItem> Table = new List<FlashTableItem>();
        }

        public static class RfidPageState
        {
            public static ushort InitChipNumber = 0;
            public static ushort Mask = 0;
            public static byte ReadFrom = 0;
            public static byte ReadTo = 0;
            public static byte WriteFrom = 0;
            public static byte[] WriteData = new byte[4];
            public static byte[] Uid = new byte[8];

            public class RfidTableItem
            {
                public string PageNum { get; set; }
                public string PageDesc { get; set; }
                public string Data { get; set; }
                public string Decoded { get; set; }
            }

            public static readonly List<RfidTableItem> Table = new List<RfidTableItem>();
        }

        public static class TeamsPageState
        {
            public static ushort ScanTeamNumber = 1;
            public static ushort GetTeamNumber = 1;
            public static DateTime Issued = new DateTime();
            public static ushort TeamMask = 0;
            public static ushort EraseTeam = 0;

            public class TeamsTableItem
            {
                public string TeamNum { get; set; }
                public string InitTime { get; set; }
                public string CheckTime { get; set; }
                public string Mask { get; set; }
                public string DumpSize { get; set; }
            }

            public static readonly List<TeamsTableItem> Table = new List<TeamsTableItem>();
        }

        public static readonly BtConnector Bt = new BtConnector();

        public static ProtocolParser Parser = new ProtocolParser(StationSettings.Number);

        public static FlashContainer Flash = new FlashContainer(FlashSizeLimit, StationSettings.TeamBlockSize);

        public static RfidContainer Rfid = new RfidContainer(StationSettings.ChipType);

        public static readonly TeamsContainer Teams = new TeamsContainer();

        public static volatile byte TimerActiveTasks;
        private const int BtReadPeriod = 500;
        public static volatile bool DumpCancellation;


        // Write data to the BtDevice
        public static async Task<bool> SendToBtAsync(byte[] outBuffer, Context currentContext, Func<Task<bool>> callBack)
        {
            if (!await GlobalOperationsIdClass.Bt.WriteBtAsync(outBuffer))
            {
                Toast.MakeText(currentContext, "Can't write to Bluetooth.", ToastLength.Long).Show();
                return false;
            }

            GlobalOperationsIdClass.TimerActiveTasks++;
            GlobalOperationsIdClass.StartTimer(GlobalOperationsIdClass.BtReadPeriod, callBack);
            GlobalOperationsIdClass.StatusPageState.TerminalText.Append(">> " + Helpers.ConvertByteArrayToHex(outBuffer) + System.Environment.NewLine);

            return true;
        }

        private static void StartTimer(long gap, Func<Task<bool>> callback)
        {
            var handler = new Handler(Looper.MainLooper);
            handler.PostDelayed(() =>
            {
                if (Bt.BtInputBuffer.Count > 0) callback();
                if (TimerActiveTasks > 0) StartTimer(gap, callback);
                handler.Dispose();
                handler = null;
            }, gap);
        }

        // need to implement fully
        private async Task<bool> ReadBt()
        {
            bool packageReceived;
            lock (GlobalOperationsIdClass.Bt.SerialReceiveThreadLock)
            {
                packageReceived = GlobalOperationsIdClass.Parser.AddData(GlobalOperationsIdClass.Bt.BtInputBuffer);
            }

            if (GlobalOperationsIdClass.Parser._repliesList.Count <= 0) return packageReceived;

            for (var n = 0; n < GlobalOperationsIdClass.Parser._repliesList.Count; n++)
            {
                var reply = GlobalOperationsIdClass.Parser._repliesList[n];
                GlobalOperationsIdClass.TimerActiveTasks--;
                if (reply.ReplyCode != 0)
                {
                    GlobalOperationsIdClass.StatusPageState.TerminalText.Append(reply.ToString());

                    if (reply.ErrorCode == 0)
                    {
                        if (reply.ReplyCode == ProtocolParser.Reply.GET_STATUS)
                        {
                            var replyDetails = new ProtocolParser.ReplyData.GetStatusReply(reply);
                            GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());

                            if (reply.StationNumber != GlobalOperationsIdClass.StationSettings.Number)
                            {
                                GlobalOperationsIdClass.StationSettings.Number = reply.StationNumber;
                                GlobalOperationsIdClass.Parser =
                                    new ProtocolParser(GlobalOperationsIdClass.StationSettings.Number);
                            }
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.GET_CONFIG)
                        {
                            var replyDetails = new ProtocolParser.ReplyData.GetConfigReply(reply);
                            GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());

                            if (reply.StationNumber != GlobalOperationsIdClass.StationSettings.Number)
                            {
                                GlobalOperationsIdClass.StationSettings.Number = reply.StationNumber;
                                GlobalOperationsIdClass.Parser =
                                    new ProtocolParser(GlobalOperationsIdClass.StationSettings.Number);
                            }

                            byte g = 0;
                            foreach (var x in RfidContainer.ChipTypes.Ids)
                            {
                                if (x.Value == replyDetails.ChipTypeId)
                                {
                                    GlobalOperationsIdClass.StationSettings.ChipType = g;
                                    break;
                                }
                                g++;
                            }

                            if (GlobalOperationsIdClass.StationSettings.ChipType !=
                                GlobalOperationsIdClass.Rfid.ChipType)
                                GlobalOperationsIdClass.Rfid =
                                    new RfidContainer(GlobalOperationsIdClass.StationSettings.ChipType);

                            GlobalOperationsIdClass.StationSettings.AntennaGain = replyDetails.AntennaGain;

                            GlobalOperationsIdClass.StationSettings.Mode = replyDetails.Mode;

                            GlobalOperationsIdClass.StationSettings.BatteryLimit = replyDetails.BatteryLimit;

                            GlobalOperationsIdClass.StationSettings.EraseBlockSize = replyDetails.EraseBlockSize;

                            GlobalOperationsIdClass.StationSettings.FlashSize = replyDetails.FlashSize;

                            GlobalOperationsIdClass.StationSettings.packetLengthkSize = replyDetails.MaxPacketLength;

                            GlobalOperationsIdClass.StationSettings.TeamBlockSize = replyDetails.TeamBlockSize;
                            if (GlobalOperationsIdClass.StationSettings.TeamBlockSize !=
                                GlobalOperationsIdClass.Flash.TeamDumpSize)
                                GlobalOperationsIdClass.Flash = new FlashContainer(
                                    GlobalOperationsIdClass.FlashSizeLimit,
                                    GlobalOperationsIdClass.StationSettings.TeamBlockSize,
                                    0);

                            GlobalOperationsIdClass.StationSettings.VoltageCoefficient = replyDetails.VoltageKoeff;
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.SET_TIME)
                        {
                            var replyDetails = new ProtocolParser.ReplyData.SetTimeReply(reply);
                            GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.SEND_BT_COMMAND)
                        {
                            var replyDetails = new ProtocolParser.ReplyData.SendBtCommandReply(reply);
                            GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.GET_LAST_TEAMS)
                        {
                            var replyDetails = new ProtocolParser.ReplyData.GetLastTeamsReply(reply);
                            GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.SCAN_TEAMS)
                        {
                            var replyDetails = new ProtocolParser.ReplyData.ScanTeamsReply(reply);
                            GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.GET_TEAM_RECORD)
                        {

                            var replyDetails = new ProtocolParser.ReplyData.GetTeamRecordReply(reply);
                            GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());

                            var team = new TeamsContainer.TeamData
                            {
                                LastCheckTime = replyDetails.LastMarkTime,
                                DumpSize = replyDetails.DumpSize,
                                InitTime = replyDetails.InitTime,
                                TeamMask = replyDetails.Mask,
                                TeamNumber = replyDetails.TeamNumber
                            };
                            GlobalOperationsIdClass.Teams.Add(team);
                            var tmp = GlobalOperationsIdClass.Teams.GetTablePage(replyDetails.TeamNumber);
                            var row = new TeamsPageState.TeamsTableItem
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
                                if (TeamsPageState.Table[i].TeamNum != row.TeamNum) continue;

                                TeamsPageState.Table.RemoveAt(i);
                                TeamsPageState.Table.Insert(i, row);
                                flag = true;
                                break;
                            }

                            if (!flag) TeamsPageState.Table?.Add(row);
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.INIT_CHIP)
                        {
                            var replyDetails = new ProtocolParser.ReplyData.InitChipReply(reply);
                            GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.READ_CARD_PAGE)
                        {

                            var replyDetails = new ProtocolParser.ReplyData.ReadCardPageReply(reply);
                            GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());

                            GlobalOperationsIdClass.Rfid.AddPages(replyDetails.startPage, replyDetails.PagesData);
                            // refresh RFID table
                            var endPage = replyDetails.startPage + (int)(replyDetails.PagesData.Length / 4);
                            for (var i = replyDetails.startPage; i < endPage; i++)
                            {
                                var tmp = GlobalOperationsIdClass.Rfid.GetTablePage((byte)i);
                                var row = new RfidPageState.RfidTableItem
                                { PageNum = tmp[0], Data = tmp[1], PageDesc = tmp[2], Decoded = tmp[3] };

                                var flag = false;
                                for (var j = 0; j < RfidPageState.Table?.Count; j++)
                                {
                                    if (RfidPageState.Table[j].PageNum != row.PageNum) continue;
                                    RfidPageState.Table.RemoveAt(j);
                                    RfidPageState.Table.Insert(j, row);
                                    flag = true;
                                    break;
                                }
                                if (!flag) RfidPageState.Table?.Add(row);
                            }
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.READ_FLASH)
                        {
                            var replyDetails = new ProtocolParser.ReplyData.ReadFlashReply(reply);
                            GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());

                            GlobalOperationsIdClass.Flash.Add(replyDetails.Address, replyDetails.Data);
                            // refresh flash table
                            var startPage = (int)(replyDetails.Address / GlobalOperationsIdClass.Flash.TeamDumpSize);
                            var endPage = (int)((replyDetails.Address + replyDetails.Data.Length) /
                                                GlobalOperationsIdClass.Flash.TeamDumpSize);
                            for (var i = startPage; i <= endPage; i++)
                            {
                                var tmp = GlobalOperationsIdClass.Flash.GetTablePage(i);
                                var row = new FlashPageState.FlashTableItem { TeamNum = tmp[0], Decoded = tmp[2] };

                                var flag = false;
                                for (var j = 0; j < FlashPageState.Table?.Count; j++)
                                {
                                    if (FlashPageState.Table[j].TeamNum != row.TeamNum) continue;

                                    FlashPageState.Table.RemoveAt(j);
                                    FlashPageState.Table.Insert(j, row);
                                    flag = true;
                                    break;
                                }
                                if (!flag) FlashPageState.Table?.Add(row);
                            }
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.WRITE_FLASH)
                        {
                            var replyDetails = new ProtocolParser.ReplyData.WriteFlashReply(reply);
                            GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());
                        }
                    }

                    GlobalOperationsIdClass.Parser._repliesList.Remove(reply);
                }
                else
                {
                    GlobalOperationsIdClass.StatusPageState.TerminalText.Append(reply.Message);
                }
            }
            return packageReceived;
        }
    }

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        #region UI controls

        private Button refreshButton;
        private Button connectButton;
        private Button statusButton;
        private Button configurationButton;
        private Button teamsButton;
        private Button rfidButton;
        private Button flashButton;
        
        private EditText stationNumberText;
        private TextView terminalTextView;

        private Spinner deviceListSpinner;

        #endregion

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            // Get our UI controls from the loaded layout
            refreshButton = FindViewById<Button>(Resource.Id.refreshButton);
            connectButton = FindViewById<Button>(Resource.Id.connectButton);
            statusButton = FindViewById<Button>(Resource.Id.statusButton);
            configurationButton = FindViewById<Button>(Resource.Id.configurationButton);
            teamsButton = FindViewById<Button>(Resource.Id.teamsButton);
            rfidButton = FindViewById<Button>(Resource.Id.rfidButton);
            flashButton = FindViewById<Button>(Resource.Id.flashButton);

            stationNumberText = FindViewById<EditText>(Resource.Id.stationNumberEditText);
            
            terminalTextView = FindViewById<TextView>(Resource.Id.terminalTextView);

            deviceListSpinner = FindViewById<Spinner>(Resource.Id.DeviceListSpinner);

            GlobalOperationsIdClass.GetInstance();

            GlobalOperationsIdClass.DumpCancellation = true;
            GlobalOperationsIdClass.TimerActiveTasks = 0;

            await StartBt();

            stationNumberText.Text = GlobalOperationsIdClass.StationSettings.Number.ToString();            

            refreshButton.Click += async (sender, e) =>
            {
                await StartBt();
            };

            connectButton.Click += async (sender, e) =>
            {
                connectButton.Enabled = false;
                refreshButton.Enabled = false;
                deviceListSpinner.Enabled = false;
                if (connectButton.Text == "Connect")
                {
                    connectButton.Text = "Connecting...";
                    connectButton.Invalidate();
                    GlobalOperationsIdClass.Bt.Connecting = true;
                    var deviceName = deviceListSpinner?.SelectedItem?.ToString();
                    GlobalOperationsIdClass.Bt.Connect(deviceName);
                    while (GlobalOperationsIdClass.Bt.Connecting) await Task.Delay(1);
                    if (GlobalOperationsIdClass.Bt.IsBtConnected())
                    {
                        Toast.MakeText(this, "Connected to: " + deviceName, ToastLength.Long).Show();
                        connectButton.Text = "Disconnect";
                    }
                    else
                    {
                        Toast.MakeText(this,
                                "Can not connect to: " + deviceName,
                                ToastLength.Long)
                            .Show();
                        connectButton.Text = "Connect";
                    }
                }
                else if (connectButton.Text == "Disconnect")
                {
                    if (GlobalOperationsIdClass.Bt.IsBtConnected()) GlobalOperationsIdClass.Bt.Close();
                    connectButton.Text = "Connect";
                    refreshButton.Enabled = true;
                    deviceListSpinner.Enabled = true;
                }
                connectButton.Enabled = true;
            };

            stationNumberText.TextChanged += (sender, e) =>
            {
                byte.TryParse(stationNumberText.Text, out var n);
                GlobalOperationsIdClass.StationSettings.Number = n;
                GlobalOperationsIdClass.Parser = new ProtocolParser(GlobalOperationsIdClass.StationSettings.Number);
            };

            statusButton.Click += (sender, e) =>
            {
                StartActivity(typeof(ActivityStatus));
            };

            configurationButton.Click += (sender, e) =>
            {
                StartActivity(typeof(ActivityConfig));
            };

            teamsButton.Click += (sender, e) =>
            {
                StartActivity(typeof(ActivityTeams));
            };

            rfidButton.Click += (sender, e) =>
            {
                StartActivity(typeof(ActivityRfid));
            };

            flashButton.Click += (sender, e) =>
            {
                StartActivity(typeof(ActivityFlash));
            };
        }

        private async Task<bool> StartBt()
        {
            bool result;
            await GlobalOperationsIdClass.Bt.Enable();
            if (GlobalOperationsIdClass.Bt.IsBtEnabled())
            {
                Toast.MakeText(this, "Bluetooth adapter enabled.", ToastLength.Long).Show();
                var items = GlobalOperationsIdClass.Bt.PairedDevices();
                var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, items);
                deviceListSpinner.Adapter = adapter;
                connectButton.Enabled = GlobalOperationsIdClass.Bt.IsBtEnabled();
                result = true;
            }
            else
            {
                if (!await GlobalOperationsIdClass.Bt.Enable())
                {
                    Toast.MakeText(this, "Bluetooth adapter not enabled.", ToastLength.Long).Show();
                    result = false;
                }
                else result = true;
            }
            return result;
        }

        protected override void OnResume()
        {
            base.OnResume();

            GlobalOperationsIdClass.DumpCancellation = true;
            GlobalOperationsIdClass.TimerActiveTasks = 0;
            stationNumberText.Text = GlobalOperationsIdClass.StationSettings.Number.ToString();
            terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();

            if (GlobalOperationsIdClass.Bt.IsBtConnected())
            {
                connectButton.Text = "Disconnect";
                refreshButton.Enabled = false;
                deviceListSpinner.Enabled = false;
            }
            else
            {
                connectButton.Text = "Connect";
                refreshButton.Enabled = true;
                deviceListSpinner.Enabled = true;
            }
        }
    }
}
