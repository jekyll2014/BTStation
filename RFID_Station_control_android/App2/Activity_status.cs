using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;

using System.Linq;
using System.Threading.Tasks;
using Android.Content;

namespace RfidStationControl
{
    [Activity(Label = "Status")]
    public class ActivityStatus : AppCompatActivity
    {
        private TextView _terminalTextView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_status);

            // populate all controls
            var getStatusButton = FindViewById<Button>(Resource.Id.getStatusButton);
            var getConfigButton = FindViewById<Button>(Resource.Id.getConfigButton);
            var getErrorsButton = FindViewById<Button>(Resource.Id.getErrorsButton);
            var setModeButton = FindViewById<Button>(Resource.Id.setModeButton);
            var resetButton = FindViewById<Button>(Resource.Id.resetStationButton);
            var clearTerminalButton = FindViewById<Button>(Resource.Id.clearTerminalButton);
            var backButton = FindViewById<Button>(Resource.Id.backButton);

            var newStationNumberText = FindViewById<EditText>(Resource.Id.newStationNumberEditText);
            var chipsCheckedText = FindViewById<EditText>(Resource.Id.chipsCheckedEditText);
            var lastCheckEditText = FindViewById<EditText>(Resource.Id.lastCheckEditText);

            var modeListSpinner = FindViewById<Spinner>(Resource.Id.modeListSpinner);

            _terminalTextView = FindViewById<TextView>(Resource.Id.terminalTextView);

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
                setModeButton.Enabled = true;
                resetButton.Enabled = true;
            }
            else
            {
                getStatusButton.Enabled = false;
                getConfigButton.Enabled = false;
                setModeButton.Enabled = false;
                resetButton.Enabled = false;
            }

            GlobalOperationsIdClass.timerActiveTasks = 0;

            getStatusButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.GetStatus();
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
            };

            getConfigButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.GetConfig();
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
            };

            getErrorsButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.GetLastErrors();
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
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
                _terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
            };

            resetButton.Click += async (sender, e) =>
            {
                chipsCheckedText.ClearFocus();
                lastCheckEditText.ClearFocus();
                newStationNumberText.ClearFocus();
                var outBuffer = GlobalOperationsIdClass.Parser.ResetStation(GlobalOperationsIdClass.StatusPageState.CheckedChipsNumber, GlobalOperationsIdClass.StatusPageState.LastCheck, GlobalOperationsIdClass.StatusPageState.NewStationNumber);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
            };

            clearTerminalButton.Click += (sender, e) =>
            {
                _terminalTextView.Text = "";
                GlobalOperationsIdClass.StatusPageState.TerminalText.Clear();
            };

            backButton.Click += (sender, e) =>
            {
                Finish();
            };

            newStationNumberText.FocusChange += (sender, e) =>
            {
                byte.TryParse(newStationNumberText.Text, out var n);
                newStationNumberText.Text = n.ToString();
                GlobalOperationsIdClass.StatusPageState.NewStationNumber = n;
            };

            chipsCheckedText.FocusChange += (sender, e) =>
            {
                byte.TryParse(chipsCheckedText.Text, out var n);
                chipsCheckedText.Text = n.ToString();
                GlobalOperationsIdClass.StatusPageState.CheckedChipsNumber = n;
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
            var packageReceived = false;
            lock (GlobalOperationsIdClass.Bt.SerialReceiveThreadLock)
            {
                packageReceived = GlobalOperationsIdClass.Parser.AddData(GlobalOperationsIdClass.Bt.BtInputBuffer);
            }

            if (GlobalOperationsIdClass.Parser._repliesList.Count <= 0) return packageReceived;

            _terminalTextView = FindViewById<TextView>(Resource.Id.terminalTextView);
            var modeListSpinner = FindViewById<Spinner>(Resource.Id.modeListSpinner);

            for (var i = 0; i < GlobalOperationsIdClass.Parser._repliesList.Count; i++)
            {
                var reply = GlobalOperationsIdClass.Parser._repliesList[i];
                GlobalOperationsIdClass.timerActiveTasks--;
                if (reply.ReplyCode != 0)
                {
                    _terminalTextView.Text += reply.ToString();
                    GlobalOperationsIdClass.StatusPageState.TerminalText.Append(reply.ToString());

                    if (reply.ErrorCode == 0)
                    {
                        if (reply.ReplyCode == ProtocolParser.Reply.GET_STATUS)
                        {
                            var replyDetails =
                                new ProtocolParser.ReplyData.GetStatusReply(reply);
                            _terminalTextView.Text += replyDetails.ToString();
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
                            var replyDetails =
                                new ProtocolParser.ReplyData.GetConfigReply(reply);
                            _terminalTextView.Text += replyDetails.ToString();
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

                            GlobalOperationsIdClass.StationSettings.TeamBlockSize = replyDetails.TeamBlockSize;
                            if (GlobalOperationsIdClass.StationSettings.TeamBlockSize !=
                                GlobalOperationsIdClass.Flash.TeamDumpSize)
                                GlobalOperationsIdClass.Flash = new FlashContainer(
                                    GlobalOperationsIdClass.FlashSizeLimit,
                                    GlobalOperationsIdClass.StationSettings.TeamBlockSize,
                                    0); // GlobalOperationsIdClass.StationSettings.TeamBlockSize

                            GlobalOperationsIdClass.StationSettings.VoltageCoefficient = replyDetails.VoltageKoeff;
                        }

                        _terminalTextView.Invalidate();
                    }

                    GlobalOperationsIdClass.Parser._repliesList.Remove(reply);
                    Toast.MakeText(this,
                        ProtocolParser.ReplyStrings[reply.ReplyCode] + " replied: " +
                        ProtocolParser.ErrorCodes[reply.ErrorCode], ToastLength.Long).Show();
                }
                else
                {
                    _terminalTextView.Text += reply.Message;
                    GlobalOperationsIdClass.StatusPageState.TerminalText.Append(reply.Message);
                }
            }
            return packageReceived;
        }
    }
}