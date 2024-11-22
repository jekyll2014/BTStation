using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views.InputMethods;
using Android.Widget;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace RfidStationControl
{
    [Activity(Label = "Config")]
    public class ActivityConfig : AppCompatActivity
    {
        #region UI controls

        private Button _backButton;
        private Button _getConfigButton;
        private Button _setTimeButton;
        private Button _setKoeffButton;
        private Button _setBatLimitButton;
        private Button _setAntennaButton;
        private Button _setChipButton;
        private Button _setTeamSizeButton;
        private Button _setEraseBlockSizeButton;
        private Button _setBtNameButton;
        private Button _setBtPinButton;
        private Button _sendBtCommandButton;
        private Button _setAutoreportButton;
        private Button _clearTerminalButton;

        private EditText _timeEditText;
        private EditText _koeffEditText;
        private EditText _batLimitEditText;
        private EditText _teamSizeEditText;
        private EditText _flashSizeEditText;
        private EditText _setBtNameEditText;
        private EditText _setBtPinEditText;
        private EditText _sendBtCommandEditText;

        private TextView _flashSizeText;
        private TextView _packetLengthTextView;
        private TextView _terminalTextView;

        private Spinner _antennaSpinner;
        private Spinner _chipTypeSpinner;
        private Spinner _dumpSizeSpinner;

        private CheckBox _currentCheckBox;
        private CheckBox _autoreportCheckBox;
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_config);

            // populate all controls
            _backButton = FindViewById<Button>(Resource.Id.backButton);
            _getConfigButton = FindViewById<Button>(Resource.Id.getConfigButton);
            _setTimeButton = FindViewById<Button>(Resource.Id.setTimeButton);
            _setKoeffButton = FindViewById<Button>(Resource.Id.setKoeffButton);
            _setBatLimitButton = FindViewById<Button>(Resource.Id.setBatLimitButton);
            _setAntennaButton = FindViewById<Button>(Resource.Id.setAntennaButton);
            _setChipButton = FindViewById<Button>(Resource.Id.setChipButton);
            _setTeamSizeButton = FindViewById<Button>(Resource.Id.setTeamSizeButton);
            _setEraseBlockSizeButton = FindViewById<Button>(Resource.Id.setFlashSizeButton);
            _setBtNameButton = FindViewById<Button>(Resource.Id.setBtNameButton);
            _setBtPinButton = FindViewById<Button>(Resource.Id.setBtPinButton);
            _sendBtCommandButton = FindViewById<Button>(Resource.Id.sendBtCommandButton);
            _setAutoreportButton = FindViewById<Button>(Resource.Id.setAutoreportButton);
            _clearTerminalButton = FindViewById<Button>(Resource.Id.clearTerminalButton);

            _timeEditText = FindViewById<EditText>(Resource.Id.timeEditText);
            _koeffEditText = FindViewById<EditText>(Resource.Id.koeffEditText);
            _batLimitEditText = FindViewById<EditText>(Resource.Id.batLimitEditText);
            _teamSizeEditText = FindViewById<EditText>(Resource.Id.teamSizeEditText);
            _flashSizeEditText = FindViewById<EditText>(Resource.Id.flashSizeEditText);
            _setBtNameEditText = FindViewById<EditText>(Resource.Id.setBtNameEditText);
            _setBtPinEditText = FindViewById<EditText>(Resource.Id.setBtPinEditText);
            _sendBtCommandEditText = FindViewById<EditText>(Resource.Id.sendBtCommandEditText);

            _flashSizeText = FindViewById<TextView>(Resource.Id.flashSizeTextView);
            _packetLengthTextView = FindViewById<TextView>(Resource.Id.packetLengthTextView);
            _terminalTextView = FindViewById<TextView>(Resource.Id.terminalTextView);

            _antennaSpinner = FindViewById<Spinner>(Resource.Id.antennaSpinner);
            _chipTypeSpinner = FindViewById<Spinner>(Resource.Id.chipSpinner);
            _dumpSizeSpinner = FindViewById<Spinner>(Resource.Id.dumpSizeSpinner);

            _currentCheckBox = FindViewById<CheckBox>(Resource.Id.currentCheckBox);
            _autoreportCheckBox = FindViewById<CheckBox>(Resource.Id.autoreportCheckBox);

            //Initialise page
            Title = "Station " + StationSettings.Number.ToString() + " configuration";

            _timeEditText.Text = Helpers.DateToString(DateTime.Now);
            _koeffEditText.Text = StationSettings.VoltageCoefficient.ToString("F5");
            _batLimitEditText.Text = StationSettings.BatteryLimit.ToString("F2");
            _teamSizeEditText.Text = StationSettings.TeamBlockSize.ToString();
            _flashSizeEditText.Text = StationSettings.EraseBlockSize.ToString();
            _flashSizeText.Text = (StationSettings.FlashSize / 1024 / 1024).ToString("F2") + "Mb";
            _packetLengthTextView.Text = (StationSettings.MaxPacketLength).ToString();
            _setBtNameEditText.Text = StationSettings.BtName;
            _setBtPinEditText.Text = StationSettings.BtPin;
            _sendBtCommandEditText.Text = StationSettings.BtCommand;
            _currentCheckBox.Checked = ConfigPageState.UseCurrentTime;
            _timeEditText.Enabled = !_currentCheckBox.Checked;

            _terminalTextView.Text = StatusPageState.TerminalText.ToString();

            var items = StationSettings.Gain.Keys.ToArray();
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, items);
            _antennaSpinner.Adapter = adapter;
            _antennaSpinner.SetSelection(5);
            var g = 0;
            foreach (var x in StationSettings.Gain)
            {
                if (x.Value == StationSettings.AntennaGain)
                    break;
                g++;
            }
            _antennaSpinner.SetSelection(g);

            items = RfidContainer.ChipTypes.Types.Keys.ToArray();
            adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, items);
            _chipTypeSpinner.Adapter = adapter;
            _chipTypeSpinner.SetSelection(StationSettings.ChipType);

            items = GlobalOperationsIdClass.FlashSizeLimitDictionary.Keys.ToArray();
            adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, items);
            _dumpSizeSpinner.Adapter = adapter;
            _dumpSizeSpinner.SetSelection(0);
            g = 0;
            foreach (var x in GlobalOperationsIdClass.FlashSizeLimitDictionary)
            {
                if (x.Value == GlobalOperationsIdClass.FlashSizeLimit)
                    break;
                g++;
            }
            _dumpSizeSpinner.SetSelection(g);

            if (GlobalOperationsIdClass.Bt.IsBtEnabled() && GlobalOperationsIdClass.Bt.IsBtConnected())
            {
                _getConfigButton.Enabled = true;
                _setTimeButton.Enabled = true;
                _setKoeffButton.Enabled = true;
                _setBatLimitButton.Enabled = true;
                _setAntennaButton.Enabled = true;
                _setChipButton.Enabled = true;
                _setTeamSizeButton.Enabled = true;
                _setEraseBlockSizeButton.Enabled = true;
                _setBtNameButton.Enabled = true;
                _setBtPinButton.Enabled = true;
                _sendBtCommandButton.Enabled = true;
                _setAutoreportButton.Enabled = true;
            }
            else
            {
                _getConfigButton.Enabled = false;
                _setTimeButton.Enabled = false;
                _setKoeffButton.Enabled = false;
                _setBatLimitButton.Enabled = false;
                _setAntennaButton.Enabled = false;
                _setChipButton.Enabled = false;
                _setTeamSizeButton.Enabled = false;
                _setEraseBlockSizeButton.Enabled = false;
                _setBtNameButton.Enabled = false;
                _setBtPinButton.Enabled = false;
                _sendBtCommandButton.Enabled = false;
                _setAutoreportButton.Enabled = false;
            }

            GlobalOperationsIdClass.TimerActiveTasks = 0;

            var inputManager = (InputMethodManager)GetSystemService(Context.InputMethodService);
            var currentFocus = Window.CurrentFocus;
            inputManager?.HideSoftInputFromWindow(currentFocus?.WindowToken, HideSoftInputFlags.None);

            _getConfigButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.GetConfig();
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text = StatusPageState.TerminalText.ToString();
            };

            _setTimeButton.Click += async (sender, e) =>
            {
                byte[] outBuffer;
                if (_currentCheckBox.Checked)
                {
                    outBuffer = GlobalOperationsIdClass.Parser.SetTime(DateTime.Now);
                }
                else
                {
                    _timeEditText.ClearFocus();
                    outBuffer = GlobalOperationsIdClass.Parser.SetTime(ConfigPageState.SetTime);
                }
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text = StatusPageState.TerminalText.ToString();
            };

            _setKoeffButton.Click += async (sender, e) =>
            {
                _koeffEditText.ClearFocus();
                float.TryParse(_koeffEditText.Text, out var batteryCoefficient);
                var outBuffer = GlobalOperationsIdClass.Parser.SetVCoeff(batteryCoefficient);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text = StatusPageState.TerminalText.ToString();
            };

            _setBatLimitButton.Click += async (sender, e) =>
            {
                _batLimitEditText.ClearFocus();
                float.TryParse(_batLimitEditText.Text, out var batteryLimit);
                var outBuffer = GlobalOperationsIdClass.Parser.SetBatteryLimit(batteryLimit);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text = StatusPageState.TerminalText.ToString();
            };

            _setAntennaButton.Click += async (sender, e) =>
            {
                if (!StationSettings.Gain.TryGetValue(_antennaSpinner.SelectedItem?.ToString(),
                    out var gain))
                {
                    Toast.MakeText(this, "Incorrect gain selected", ToastLength.Long)?.Show();
                    return;
                }
                var outBuffer = GlobalOperationsIdClass.Parser.SetGain(gain);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text = StatusPageState.TerminalText.ToString();
            };

            _setChipButton.Click += async (sender, e) =>
            {
                if (!RfidContainer.ChipTypes.Types.TryGetValue(_chipTypeSpinner.SelectedItem?.ToString(),
                    out var chipType))
                {
                    Toast.MakeText(this, "Incorrect chip type selected", ToastLength.Long)?.Show();
                    return;
                }
                var outBuffer = GlobalOperationsIdClass.Parser.SetChipType(chipType);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text = StatusPageState.TerminalText.ToString();
            };

            _setTeamSizeButton.Click += async (sender, e) =>
            {
                _teamSizeEditText.ClearFocus();
                uint.TryParse(_teamSizeEditText.Text, out var n);
                var outBuffer = GlobalOperationsIdClass.Parser.SetTeamFlashSize(n);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text = StatusPageState.TerminalText.ToString();
            };

            _setEraseBlockSizeButton.Click += async (sender, e) =>
            {
                _flashSizeEditText.ClearFocus();
                uint.TryParse(_flashSizeEditText.Text, out var n);
                var outBuffer = GlobalOperationsIdClass.Parser.SetEraseBlock(n);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text = StatusPageState.TerminalText.ToString();
            };

            _setBtNameButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.SetBtName(_setBtNameEditText.Text);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text = StatusPageState.TerminalText.ToString();
            };

            _setBtPinButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.SetBtPin(_setBtPinEditText.Text);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text = StatusPageState.TerminalText.ToString();
            };

            _sendBtCommandButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.SendBtCommand(_sendBtCommandEditText.Text);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text = StatusPageState.TerminalText.ToString();
            };

            _setAutoreportButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.SetAutoReport(_autoreportCheckBox.Checked);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text = StatusPageState.TerminalText.ToString();
            };

            _clearTerminalButton.Click += (sender, e) =>
            {
                StatusPageState.TerminalText.Clear();
                _terminalTextView.Text = StatusPageState.TerminalText.ToString();
            };

            _backButton.Click += (sender, e) =>
            {
                Finish();
            };

            _timeEditText.FocusChange += (sender, e) =>
            {
                if (!_timeEditText.HasFocus)
                {
                    var t = Helpers.DateStringToUnixTime(_timeEditText.Text);
                    _timeEditText.Text = Helpers.DateToString(Helpers.ConvertFromUnixTimestamp(t));
                    ConfigPageState.SetTime =
                        Helpers.ConvertFromUnixTimestamp(Helpers.DateStringToUnixTime(_timeEditText.Text));
                }
            };

            _koeffEditText.FocusChange += (sender, e) =>
            {
                if (!_koeffEditText.HasFocus)
                {
                    //koeffEditText.Text = koeffEditText.Text.Replace('.', ',');
                    float.TryParse(_koeffEditText.Text, out var batteryCoefficient);
                    _koeffEditText.Text = batteryCoefficient.ToString("F5");
                }
            };

            _batLimitEditText.FocusChange += (sender, e) =>
            {
                if (!_batLimitEditText.HasFocus)
                {
                    //batLimitEditText.Text = batLimitEditText.Text.Replace('.', ',');
                    float.TryParse(_batLimitEditText.Text, out var batteryLimit);
                    _batLimitEditText.Text = batteryLimit.ToString("F2");
                }
            };

            _teamSizeEditText.FocusChange += (sender, e) =>
            {
                if (!_teamSizeEditText.HasFocus)
                {
                    uint.TryParse(_teamSizeEditText.Text, out var n);
                    _teamSizeEditText.Text = n.ToString();
                }
            };

            _flashSizeEditText.FocusChange += (sender, e) =>
            {
                if (!_flashSizeEditText.HasFocus)
                {
                    uint.TryParse(_flashSizeEditText.Text, out var n);
                    _flashSizeEditText.Text = n.ToString();
                }
            };

            _setBtNameEditText.FocusChange += (sender, e) =>
            {
                if (!_setBtNameEditText.HasFocus)
                {
                    if (_setBtNameEditText.Text.Length > 32)
                        _setBtNameEditText.Text = _setBtNameEditText.Text.Substring(0, 32);
                }
            };

            _setBtPinEditText.FocusChange += (sender, e) =>
            {
                if (!_setBtPinEditText.HasFocus)
                {
                    if (_setBtPinEditText.Text.Length > 32)
                        _setBtPinEditText.Text = _setBtPinEditText.Text.Substring(0, 16);
                }
            };

            _currentCheckBox.CheckedChange += (sender, e) =>
            {
                _timeEditText.Enabled = !_currentCheckBox.Checked;
                ConfigPageState.UseCurrentTime = _currentCheckBox.Checked;
            };

            _dumpSizeSpinner.ItemSelected += (sender, e) =>
            {
                GlobalOperationsIdClass.FlashSizeLimitDictionary.TryGetValue(_dumpSizeSpinner.SelectedItem?.ToString(),
                    out var size);
                ConfigPageState.FlashLimitSize = size;
                GlobalOperationsIdClass.Flash = new FlashContainer(GlobalOperationsIdClass.FlashSizeLimit,
                    StationSettings.TeamBlockSize);
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
                        if (reply.ReplyCode == ProtocolParser.Reply.GET_CONFIG)
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

                            if (replyDetails.ChipTypeId == 213)
                                StationSettings.ChipType = RfidContainer.ChipTypes.Types["NTAG213"];
                            else if (replyDetails.ChipTypeId == 215)
                                StationSettings.ChipType = RfidContainer.ChipTypes.Types["NTAG215"];
                            else if (replyDetails.ChipTypeId == 216)
                                StationSettings.ChipType = RfidContainer.ChipTypes.Types["NTAG216"];

                            if (StationSettings.ChipType !=
                                GlobalOperationsIdClass.Rfid.ChipType)
                                GlobalOperationsIdClass.Rfid =
                                    new RfidContainer(StationSettings.ChipType);

                            StationSettings.AntennaGain = replyDetails.AntennaGain;
                            StationSettings.Mode = replyDetails.Mode;
                            StationSettings.BatteryLimit = replyDetails.BatteryLimit;
                            StationSettings.EraseBlockSize = replyDetails.EraseBlockSize;
                            StationSettings.FlashSize = replyDetails.FlashSize;
                            StationSettings.MaxPacketLength = replyDetails.MaxPacketLength;
                            StationSettings.TeamBlockSize = replyDetails.TeamBlockSize;
                            if (StationSettings.TeamBlockSize !=
                                GlobalOperationsIdClass.Flash.TeamDumpSize)
                                GlobalOperationsIdClass.Flash = new FlashContainer(
                                    GlobalOperationsIdClass.FlashSizeLimit,
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