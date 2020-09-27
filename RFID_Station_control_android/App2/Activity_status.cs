using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views.InputMethods;
using Android.Widget;

using System.Linq;
using System.Threading.Tasks;

namespace RfidStationControl
{
    [Activity(Label = "Status")]
    public class ActivityStatus : AppCompatActivity
    {
        #region UI controls

        private Button getStatusButton;
        private Button getConfigButton;
        private Button getErrorsButton;
        private Button setModeButton;
        private Button resetButton;
        private Button clearTerminalButton;
        private Button backButton;

        private EditText newStationNumberText;
        private EditText chipsCheckedText;
        private EditText lastCheckEditText;

        private TextView _terminalTextView;

        private Spinner modeListSpinner;

        #endregion

        private volatile bool isFirstRun = true;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_status);

            // populate all controls
            getStatusButton = FindViewById<Button>(Resource.Id.getStatusButton);
            getConfigButton = FindViewById<Button>(Resource.Id.getConfigButton);
            getErrorsButton = FindViewById<Button>(Resource.Id.getErrorsButton);
            setModeButton = FindViewById<Button>(Resource.Id.setModeButton);
            resetButton = FindViewById<Button>(Resource.Id.resetStationButton);
            clearTerminalButton = FindViewById<Button>(Resource.Id.clearTerminalButton);
            backButton = FindViewById<Button>(Resource.Id.backButton);

            newStationNumberText = FindViewById<EditText>(Resource.Id.newStationNumberEditText);
            chipsCheckedText = FindViewById<EditText>(Resource.Id.chipsCheckedEditText);
            lastCheckEditText = FindViewById<EditText>(Resource.Id.lastCheckEditText);

            _terminalTextView = FindViewById<TextView>(Resource.Id.terminalTextView);

            modeListSpinner = FindViewById<Spinner>(Resource.Id.modeListSpinner);

            Title = "Station " + GlobalOperationsIdClass.StationSettings.Number.ToString() + " status";

            newStationNumberText.Text = GlobalOperationsIdClass.StatusPageState.NewStationNumber.ToString();
            chipsCheckedText.Text = GlobalOperationsIdClass.StatusPageState.CheckedChipsNumber.ToString();
            lastCheckEditText.Text = Helpers.DateToString(GlobalOperationsIdClass.StatusPageState.LastCheck);

            _terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();

            var items = GlobalOperationsIdClass.StationMode.Keys.ToArray();
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, items);
            modeListSpinner.Adapter = adapter;
            modeListSpinner.SetSelection(GlobalOperationsIdClass.StationSettings.Mode);

            if (GlobalOperationsIdClass.Bt.IsBtEnabled() && GlobalOperationsIdClass.Bt.IsBtConnected())
            {
                getStatusButton.Enabled = true;
                getConfigButton.Enabled = true;
                getErrorsButton.Enabled = true;
                setModeButton.Enabled = true;
                resetButton.Enabled = true;
            }
            else
            {
                getStatusButton.Enabled = false;
                getConfigButton.Enabled = false;
                getErrorsButton.Enabled = false;
                setModeButton.Enabled = false;
                resetButton.Enabled = false;
            }

            GlobalOperationsIdClass.TimerActiveTasks = 0;

            getStatusButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.GetStatus();
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();
            };

            getConfigButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.GetConfig();
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();
            };

            getErrorsButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.GetLastErrors();
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();
            };

            setModeButton.Click += async (sender, e) =>
            {
                if (!GlobalOperationsIdClass.StationMode.TryGetValue(
                    modeListSpinner.SelectedItem.ToString(), out var modeNumber))
                {
                    Toast.MakeText(this, "Incorrect mode selected", ToastLength.Long).Show();
                    return;
                }
                var outBuffer = GlobalOperationsIdClass.Parser.SetMode(modeNumber);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();
            };

            resetButton.Click += async (sender, e) =>
            {
                chipsCheckedText.ClearFocus();
                lastCheckEditText.ClearFocus();
                newStationNumberText.ClearFocus();
                var outBuffer = GlobalOperationsIdClass.Parser.ResetStation(GlobalOperationsIdClass.StatusPageState.CheckedChipsNumber, GlobalOperationsIdClass.StatusPageState.LastCheck, GlobalOperationsIdClass.StatusPageState.NewStationNumber);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();
            };

            clearTerminalButton.Click += (sender, e) =>
            {
                GlobalOperationsIdClass.StatusPageState.TerminalText.Clear();
                _terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();
            };

            backButton.Click += (sender, e) =>
            {
                Finish();
            };

            newStationNumberText.FocusChange += (sender, e) =>
            {
                if (isFirstRun && newStationNumberText.HasFocus)
                {
                    var inputManager = (InputMethodManager)GetSystemService(Context.InputMethodService);
                    inputManager.HideSoftInputFromWindow(newStationNumberText.WindowToken, HideSoftInputFlags.None);
                    isFirstRun = false;
                }

                if (newStationNumberText.HasFocus)
                {
                    byte.TryParse(newStationNumberText.Text, out var n);
                    newStationNumberText.Text = n.ToString();
                    GlobalOperationsIdClass.StatusPageState.NewStationNumber = n;
                }
            };

            chipsCheckedText.FocusChange += (sender, e) =>
            {
                if (newStationNumberText.HasFocus)
                {

                    byte.TryParse(chipsCheckedText.Text, out var n);
                    chipsCheckedText.Text = n.ToString();
                    GlobalOperationsIdClass.StatusPageState.CheckedChipsNumber = n;
                }
            };

            lastCheckEditText.FocusChange += (sender, e) =>
            {
                var t = Helpers.DateStringToUnixTime(lastCheckEditText.Text);
                lastCheckEditText.Text = Helpers.DateToString(Helpers.ConvertFromUnixTimestamp(t));
                GlobalOperationsIdClass.StatusPageState.LastCheck = Helpers.ConvertFromUnixTimestamp(t);
            };
        }

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
                                Title = "Station " + GlobalOperationsIdClass.StationSettings.Number.ToString() +
                                        " status";
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
                                Title = "Station " + GlobalOperationsIdClass.StationSettings.Number.ToString() +
                                        " status";
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
                            modeListSpinner.SetSelection(GlobalOperationsIdClass.StationSettings.Mode);

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
                    }

                    GlobalOperationsIdClass.Parser._repliesList.Remove(reply);
                    Toast.MakeText(this,
                        ProtocolParser.ReplyStrings[reply.ReplyCode] + " replied: " +
                        ProtocolParser.ErrorCodes[reply.ErrorCode], ToastLength.Long).Show();
                }
                else
                {
                    GlobalOperationsIdClass.StatusPageState.TerminalText.Append(reply.Message);
                }
            }

            _terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();
            _terminalTextView.Invalidate();
            return packageReceived;
        }
    }
}