using System;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;

using System.Threading.Tasks;
using Android.Bluetooth;

namespace RfidStationControl
{
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

            GlobalOperationsIdClass.DumpCancellation = true;
            GlobalOperationsIdClass.TimerActiveTasks = 0;

            // Get our UI controls from the loaded layout
            deviceListSpinner = FindViewById<Spinner>(Resource.Id.DeviceListSpinner);
            terminalTextView = FindViewById<TextView>(Resource.Id.terminalTextView);

            stationNumberText = FindViewById<EditText>(Resource.Id.stationNumberEditText);
            stationNumberText.Text = StationSettings.Number.ToString();
            stationNumberText.TextChanged += (sender, e) =>
            {
                byte.TryParse(stationNumberText.Text, out var n);
                StationSettings.Number = n;
                GlobalOperationsIdClass.Parser = new ProtocolParser(StationSettings.Number);
            };

            refreshButton = FindViewById<Button>(Resource.Id.refreshButton);
            refreshButton.Click += async (sender, e) =>
            {
                await StartBt();
            };

            connectButton = FindViewById<Button>(Resource.Id.connectButton);
            connectButton.Click += (sender, e) =>
            {
                connectButton.Enabled = false;
                refreshButton.Enabled = false;
                deviceListSpinner.Enabled = false;
                if (connectButton.Text == "Connect")
                {
                    connectButton.Text = "Connecting...";
                    connectButton.Invalidate();
                    var deviceName = deviceListSpinner?.SelectedItem?.ToString();
                    GlobalOperationsIdClass.Bt.Connect(deviceName);

                    var connectEnd = DateTime.Now.AddMilliseconds(5000);
                    while (!GlobalOperationsIdClass.Bt.IsBtConnected() && connectEnd > DateTime.Now) ;

                    if (GlobalOperationsIdClass.Bt.IsBtConnected())
                    {
                        Toast.MakeText(this, "Connected to: " + deviceName, ToastLength.Long)?.Show();
                        connectButton.Text = "Disconnect";
                    }
                    else
                    {
                        Toast.MakeText(this,
                                "Can not connect to: " + deviceName,
                                ToastLength.Long)?
                            .Show();
                        connectButton.Text = "Connect";
                        refreshButton.Enabled = true;
                        deviceListSpinner.Enabled = true;
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

            statusButton = FindViewById<Button>(Resource.Id.statusButton);
            statusButton.Click += (sender, e) =>
            {
                StartActivity(typeof(ActivityStatus));
            };

            configurationButton = FindViewById<Button>(Resource.Id.configurationButton);
            configurationButton.Click += (sender, e) =>
            {
                StartActivity(typeof(ActivityConfig));
            };

            teamsButton = FindViewById<Button>(Resource.Id.teamsButton);
            teamsButton.Click += (sender, e) =>
            {
                StartActivity(typeof(ActivityTeams));
            };

            rfidButton = FindViewById<Button>(Resource.Id.rfidButton);
            rfidButton.Click += (sender, e) =>
            {
                StartActivity(typeof(ActivityRfid));
            };

            flashButton = FindViewById<Button>(Resource.Id.flashButton);
            flashButton.Click += (sender, e) =>
            {
                StartActivity(typeof(ActivityFlash));
            };

            await StartBt();
        }

        private async Task<bool> StartBt()
        {
            bool result;
            GlobalOperationsIdClass.Bt = new BtConnector(this);
            await GlobalOperationsIdClass.Bt.Enable(true);
            if (GlobalOperationsIdClass.Bt.IsBtEnabled())
            {
                Toast.MakeText(this, "Bluetooth adapter enabled.", ToastLength.Long)?.Show();
                var items = GlobalOperationsIdClass.Bt.PairedDevices();
                var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, items);
                deviceListSpinner.Adapter = adapter;
                connectButton.Enabled = GlobalOperationsIdClass.Bt.IsBtEnabled();
                result = true;
            }
            else
            {
                if (!await GlobalOperationsIdClass.Bt.Enable(true))
                {
                    Toast.MakeText(this, "Bluetooth adapter not enabled.", ToastLength.Long)?.Show();
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
            stationNumberText.Text = StationSettings.Number.ToString();
            terminalTextView.Text = StatusPageState.TerminalText.ToString();

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
