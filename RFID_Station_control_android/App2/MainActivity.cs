using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;

using System.Threading.Tasks;

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
                Toast.MakeText(this, "Bluetooth adapter enabled.", ToastLength.Long)?.Show();
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
