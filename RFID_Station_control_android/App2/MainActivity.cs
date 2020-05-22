using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Android.Content;

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

        public static volatile byte timerActiveTasks = 0;
        public const int BtReadPeriod = 500;
        public static volatile bool dumpCancellation = false;

        private static void StartTimer(long gap, Func<Task<bool>> callback)
        {
            var handler = new Handler(Looper.MainLooper);
            handler.PostDelayed(() =>
            {
                if (Bt.BtInputBuffer.Count > 0) callback();
                if (timerActiveTasks > 0) StartTimer(gap, callback);
                handler.Dispose();
                handler = null;
            }, gap);
        }

        // Write data to the BtDevice
        public static async Task<bool> SendToBtAsync(byte[] outBuffer, Context currentContext, Func<Task<bool>> callBack)
        {
            var isSuccess = true;
            if (!await GlobalOperationsIdClass.Bt.WriteBtAsync(outBuffer))
            {
                Toast.MakeText(currentContext, "Can't write to Bluetooth.", ToastLength.Long).Show();
                isSuccess = false;
            }
            GlobalOperationsIdClass.timerActiveTasks++;
            GlobalOperationsIdClass.StartTimer(GlobalOperationsIdClass.BtReadPeriod, callBack);

            //_terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
            GlobalOperationsIdClass.StatusPageState.TerminalText.Append(">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n");

            return isSuccess;
        }

    }

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            // Get our UI controls from the loaded layout
            var refreshButton = FindViewById<Button>(Resource.Id.refreshButton);
            var connectButton = FindViewById<Button>(Resource.Id.connectButton);
            var statusButton = FindViewById<Button>(Resource.Id.statusButton);
            var configurationButton = FindViewById<Button>(Resource.Id.configurationButton);
            var teamsButton = FindViewById<Button>(Resource.Id.teamsButton);
            var rfidButton = FindViewById<Button>(Resource.Id.rfidButton);
            var flashButton = FindViewById<Button>(Resource.Id.flashButton);

            var stationNumberText = FindViewById<EditText>(Resource.Id.stationNumberEditText);

            var deviceListSpinner = FindViewById<Spinner>(Resource.Id.DeviceListSpinner);

            GlobalOperationsIdClass.GetInstance();

            await GlobalOperationsIdClass.Bt.Enable();
            if (GlobalOperationsIdClass.Bt.IsBtEnabled())
            {
                Toast.MakeText(this, "Bluetooth adapter enabled.", ToastLength.Long).Show();
                var items = GlobalOperationsIdClass.Bt.PairedDevices();
                var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, items);
                deviceListSpinner.Adapter = adapter;
            }
            else
            {
                await GlobalOperationsIdClass.Bt.Enable();
                Toast.MakeText(this, "Bluetooth adapter not enabled.", ToastLength.Long).Show();
            }
            connectButton.Enabled = GlobalOperationsIdClass.Bt.IsBtEnabled();

            GlobalOperationsIdClass.timerActiveTasks = 0;

            stationNumberText.Text = GlobalOperationsIdClass.StationSettings.Number.ToString();

            refreshButton.Click += async (sender, e) =>
            {
                await GlobalOperationsIdClass.Bt.Enable();

                if (GlobalOperationsIdClass.Bt.IsBtEnabled())
                {
                    Toast.MakeText(this, "Bluetooth adapter enabled.", ToastLength.Long).Show();
                    var items = GlobalOperationsIdClass.Bt.PairedDevices();
                    var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, items);
                    deviceListSpinner.Adapter = adapter;
                }
                else
                {
                    Toast.MakeText(this, "Bluetooth adapter not enabled.", ToastLength.Long).Show();
                }
                connectButton.Enabled = GlobalOperationsIdClass.Bt.IsBtEnabled();
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
                    var deviceName = deviceListSpinner.SelectedItem.ToString();
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

        protected override void OnResume()
        {
            base.OnResume();
            GlobalOperationsIdClass.dumpCancellation = true;
            GlobalOperationsIdClass.timerActiveTasks = 0;

            var refreshButton = FindViewById<Button>(Resource.Id.refreshButton);
            var connectButton = FindViewById<Button>(Resource.Id.connectButton);
            var deviceListSpinner = FindViewById<Spinner>(Resource.Id.DeviceListSpinner);
            var stationNumberText = FindViewById<EditText>(Resource.Id.stationNumberEditText);
            stationNumberText.Text = GlobalOperationsIdClass.StationSettings.Number.ToString();
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
