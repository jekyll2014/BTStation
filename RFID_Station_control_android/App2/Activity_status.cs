using Android.App;
using Android.OS;
using Android.Support.V7.App;
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

            Title = "Station " + StationSettings.Number + " status";

            newStationNumberText.Text = StatusPageState.NewStationNumber.ToString();
            chipsCheckedText.Text = StatusPageState.CheckedChipsNumber.ToString();
            lastCheckEditText.Text = Helpers.DateToString(StatusPageState.LastCheck);

            _terminalTextView = FindViewById<TextView>(Resource.Id.terminalTextView);
            _terminalTextView.Text = StatusPageState.TerminalText.ToString();

            var items = StationSettings.StationMode.Keys.ToArray();
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, items);
            modeListSpinner = FindViewById<Spinner>(Resource.Id.modeListSpinner);
            if (modeListSpinner != null)
            {
                modeListSpinner.Adapter = adapter;
                modeListSpinner.SetSelection(StationSettings.Mode);
            }

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
                _terminalTextView.Text = StatusPageState.TerminalText.ToString();
            };

            getConfigButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.GetConfig();
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text = StatusPageState.TerminalText.ToString();
            };

            getErrorsButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.GetLastErrors();
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text = StatusPageState.TerminalText.ToString();
            };

            setModeButton.Click += async (sender, e) =>
            {
                if (!StationSettings.StationMode.TryGetValue(
                    modeListSpinner.SelectedItem.ToString(), out var modeNumber))
                {
                    Toast.MakeText(this, "Incorrect mode selected", ToastLength.Long)?.Show();

                    return;
                }

                var outBuffer = GlobalOperationsIdClass.Parser.SetMode(modeNumber);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text = StatusPageState.TerminalText.ToString();
            };

            resetButton.Click += async (sender, e) =>
            {
                chipsCheckedText.ClearFocus();
                lastCheckEditText.ClearFocus();
                newStationNumberText.ClearFocus();
                var outBuffer = GlobalOperationsIdClass.Parser.ResetStation(StatusPageState.CheckedChipsNumber,
                    StatusPageState.LastCheck,
                    StatusPageState.NewStationNumber);

                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text = StatusPageState.TerminalText.ToString();
            };

            clearTerminalButton.Click += (sender, e) =>
            {
                StatusPageState.TerminalText.Clear();
                _terminalTextView.Text = StatusPageState.TerminalText.ToString();
            };

            backButton.Click += (sender, e) =>
            {
                Finish();
            };

            newStationNumberText.FocusChange += (sender, e) =>
            {
                if (!newStationNumberText.HasFocus)
                {
                    byte.TryParse(newStationNumberText.Text, out var n);
                    newStationNumberText.Text = n.ToString();
                    StatusPageState.NewStationNumber = n;
                }
            };

            chipsCheckedText.FocusChange += (sender, e) =>
            {
                if (!chipsCheckedText.HasFocus)
                {
                    byte.TryParse(chipsCheckedText.Text, out var n);
                    chipsCheckedText.Text = n.ToString();
                    StatusPageState.CheckedChipsNumber = n;
                }
            };

            lastCheckEditText.FocusChange += (sender, e) =>
            {
                if (!lastCheckEditText.HasFocus)
                {
                    var t = Helpers.DateStringToUnixTime(lastCheckEditText.Text);
                    lastCheckEditText.Text = Helpers.DateToString(Helpers.ConvertFromUnixTimestamp(t));
                    StatusPageState.LastCheck = Helpers.ConvertFromUnixTimestamp(t);
                }
            };
        }

        private async Task<bool> ReadBt()
        {
            bool packageReceived;
            lock (GlobalOperationsIdClass.Bt.SerialReceiveThreadLock)
            {
                packageReceived = GlobalOperationsIdClass.Parser.AddData(GlobalOperationsIdClass.Bt.BtInputBuffer);
            }

            if (GlobalOperationsIdClass.Parser._repliesList.Count <= 0)
                return packageReceived;

            for (var n = 0; n < GlobalOperationsIdClass.Parser._repliesList.Count; n++)
            {
                var reply = GlobalOperationsIdClass.Parser._repliesList[n];
                GlobalOperationsIdClass.TimerActiveTasks--;
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
                                GlobalOperationsIdClass.Parser =
                                    new ProtocolParser(StationSettings.Number);
                                Title = "Station " + StationSettings.Number +
                                        " status";
                            }
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.GET_CONFIG)
                        {
                            var replyDetails = new ProtocolParser.ReplyData.GetConfigReply(reply);
                            StatusPageState.TerminalText.Append(replyDetails);

                            if (reply.StationNumber != StationSettings.Number)
                            {
                                StationSettings.Number = reply.StationNumber;
                                GlobalOperationsIdClass.Parser =
                                    new ProtocolParser(StationSettings.Number);
                                Title = "Station " + StationSettings.Number +
                                        " status";
                            }

                            byte g = 0;
                            foreach (var x in RfidContainer.ChipTypes.Ids)
                            {
                                if (x.Value == replyDetails.ChipTypeId)
                                {
                                    StationSettings.ChipType = g;
                                    break;
                                }
                                g++;
                            }

                            if (StationSettings.ChipType !=
                                GlobalOperationsIdClass.Rfid.ChipType)
                                GlobalOperationsIdClass.Rfid =
                                    new RfidContainer(StationSettings.ChipType);
                            StationSettings.AntennaGain = replyDetails.AntennaGain;
                            StationSettings.Mode = replyDetails.Mode;
                            modeListSpinner.SetSelection(StationSettings.Mode);
                            StationSettings.BatteryLimit = replyDetails.BatteryLimit;
                            StationSettings.EraseBlockSize = replyDetails.EraseBlockSize;
                            StationSettings.FlashSize = replyDetails.FlashSize;
                            StationSettings.PacketLengthSize = replyDetails.MaxPacketLength;
                            StationSettings.TeamBlockSize = replyDetails.TeamBlockSize;
                            if (StationSettings.TeamBlockSize !=
                                GlobalOperationsIdClass.Flash.TeamDumpSize)
                                GlobalOperationsIdClass.Flash = new FlashContainer(
                                    GlobalOperationsIdClass.FlashSizeLimit,
                                    StationSettings.TeamBlockSize,
                                    0);
                            StationSettings.VoltageCoefficient = replyDetails.VoltageKoeff;
                        }
                    }

                    GlobalOperationsIdClass.Parser._repliesList.Remove(reply);
                    Toast.MakeText(this,
                        ProtocolParser.ReplyStrings[reply.ReplyCode] + " replied: " +
                        ProtocolParser.ErrorCodes[reply.ErrorCode], ToastLength.Long)?.Show();
                }
                else
                {
                    StatusPageState.TerminalText.Append(reply.Message);
                }
            }

            _terminalTextView.Text = StatusPageState.TerminalText.ToString();
            _terminalTextView.Invalidate();
            return packageReceived;
        }
    }
}