using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace RfidStationControl
{
    [Activity(Label = "Config")]
    public class ActivityConfig : AppCompatActivity
    {
        private TextView _terminalTextView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_config);

            // populate all controls
            var backButton = FindViewById<Button>(Resource.Id.backButton);
            var getConfigButton = FindViewById<Button>(Resource.Id.getConfigButton);
            var setTimeButton = FindViewById<Button>(Resource.Id.setTimeButton);
            var setKoeffButton = FindViewById<Button>(Resource.Id.setKoeffButton);
            var setBatLimitButton = FindViewById<Button>(Resource.Id.setBatLimitButton);
            var setAntennaButton = FindViewById<Button>(Resource.Id.setAntennaButton);
            var setChipButton = FindViewById<Button>(Resource.Id.setChipButton);
            var setTeamSizeButton = FindViewById<Button>(Resource.Id.setTeamSizeButton);
            var setEraseBlockSizeButton = FindViewById<Button>(Resource.Id.setFlashSizeButton);
            var setBtNameButton = FindViewById<Button>(Resource.Id.setBtNameButton);
            var setBtPinButton = FindViewById<Button>(Resource.Id.setBtPinButton);
            var sendBtCommandButton = FindViewById<Button>(Resource.Id.sendBtCommandButton);
            var setAutoreportButton = FindViewById<Button>(Resource.Id.setAutoreportButton);
            var clearTerminalButton = FindViewById<Button>(Resource.Id.clearTerminalButton);

            var timeEditText = FindViewById<EditText>(Resource.Id.timeEditText);
            var koeffEditText = FindViewById<EditText>(Resource.Id.koeffEditText);
            var batLimitEditText = FindViewById<EditText>(Resource.Id.batLimitEditText);
            var teamSizeEditText = FindViewById<EditText>(Resource.Id.teamSizeEditText);
            var flashSizeEditText = FindViewById<EditText>(Resource.Id.flashSizeEditText);
            var setBtNameEditText = FindViewById<EditText>(Resource.Id.setBtNameEditText);
            var setBtPinEditText = FindViewById<EditText>(Resource.Id.setBtPinEditText);
            var sendBtCommandEditText = FindViewById<EditText>(Resource.Id.sendBtCommandEditText);

            var flashSizeText = FindViewById<TextView>(Resource.Id.flashSizeTextView);
            _terminalTextView = FindViewById<TextView>(Resource.Id.terminalTextView);

            var antennaSpinner = FindViewById<Spinner>(Resource.Id.antennaSpinner);
            var chipTypeSpinner = FindViewById<Spinner>(Resource.Id.chipSpinner);
            var dumpSizeSpinner = FindViewById<Spinner>(Resource.Id.dumpSizeSpinner);

            var currentCheckBox = FindViewById<CheckBox>(Resource.Id.currentCheckBox);
            var autoreportCheckBox = FindViewById<CheckBox>(Resource.Id.autoreportCheckBox);

            //Initialise page
            Title = "Station " + GlobalOperationsIdClass.StationSettings.Number.ToString() + " configuration";

            timeEditText.Text = Helpers.DateToString(DateTime.Now);
            koeffEditText.Text = GlobalOperationsIdClass.StationSettings.VoltageCoefficient.ToString("F5");
            batLimitEditText.Text = GlobalOperationsIdClass.StationSettings.BatteryLimit.ToString("F2");
            teamSizeEditText.Text = GlobalOperationsIdClass.StationSettings.TeamBlockSize.ToString();
            flashSizeEditText.Text = GlobalOperationsIdClass.StationSettings.EraseBlockSize.ToString();
            flashSizeText.Text = (GlobalOperationsIdClass.StationSettings.FlashSize / 1024 / 1024).ToString("F2") + "Mb";
            setBtNameEditText.Text = GlobalOperationsIdClass.StationSettings.BtName;
            setBtPinEditText.Text = GlobalOperationsIdClass.StationSettings.BtPin;
            sendBtCommandEditText.Text = GlobalOperationsIdClass.StationSettings.BtCommand;
            currentCheckBox.Checked = GlobalOperationsIdClass.ConfigPageState.UseCurrentTime;
            timeEditText.Enabled = !currentCheckBox.Checked;

            _terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();

            var items = GlobalOperationsIdClass.Gain.Keys.ToArray();
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, items);
            antennaSpinner.Adapter = adapter;
            antennaSpinner.SetSelection(5);
            var g = 0;
            foreach (var x in GlobalOperationsIdClass.Gain)
            {
                if (x.Value == GlobalOperationsIdClass.StationSettings.AntennaGain)
                    break;
                g++;
            }
            antennaSpinner.SetSelection(g);

            items = RfidContainer.ChipTypes.Types.Keys.ToArray();
            adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, items);
            chipTypeSpinner.Adapter = adapter;
            chipTypeSpinner.SetSelection(GlobalOperationsIdClass.StationSettings.ChipType);

            items = GlobalOperationsIdClass.FlashSizeLimitDictionary.Keys.ToArray();
            adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, items);
            dumpSizeSpinner.Adapter = adapter;
            dumpSizeSpinner.SetSelection(0);
            g = 0;
            foreach (var x in GlobalOperationsIdClass.FlashSizeLimitDictionary)
            {
                if (x.Value == GlobalOperationsIdClass.FlashSizeLimit)
                    break;
                g++;
            }
            dumpSizeSpinner.SetSelection(g);

            if (GlobalOperationsIdClass.Bt.IsBtEnabled() && GlobalOperationsIdClass.Bt.IsBtConnected())
            {
                setTimeButton.Enabled = true;
                setKoeffButton.Enabled = true;
                setBatLimitButton.Enabled = true;
                setAntennaButton.Enabled = true;
                setChipButton.Enabled = true;
                setTeamSizeButton.Enabled = true;
                setEraseBlockSizeButton.Enabled = true;
                setBtNameButton.Enabled = true;
                setBtPinButton.Enabled = true;
                sendBtCommandButton.Enabled = true;
            }
            else
            {
                setTimeButton.Enabled = false;
                setKoeffButton.Enabled = false;
                setBatLimitButton.Enabled = false;
                setAntennaButton.Enabled = false;
                setChipButton.Enabled = false;
                setTeamSizeButton.Enabled = false;
                setEraseBlockSizeButton.Enabled = false;
                setBtNameButton.Enabled = false;
                setBtPinButton.Enabled = false;
                sendBtCommandButton.Enabled = false;
            }

            GlobalOperationsIdClass.timerActiveTasks = 0;

            getConfigButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.GetConfig();
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
            };

            setTimeButton.Click += async (sender, e) =>
            {
                byte[] outBuffer;
                if (currentCheckBox.Checked)
                {
                    outBuffer = GlobalOperationsIdClass.Parser.SetTime(DateTime.Now);
                }
                else
                {
                    timeEditText.ClearFocus();
                    outBuffer = GlobalOperationsIdClass.Parser.SetTime(GlobalOperationsIdClass.ConfigPageState.SetTime);
                }
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
            };

            setKoeffButton.Click += async (sender, e) =>
            {
                koeffEditText.ClearFocus();
                float.TryParse(koeffEditText.Text, out var batteryCoefficient);
                var outBuffer = GlobalOperationsIdClass.Parser.SetVCoeff(batteryCoefficient);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
            };

            setBatLimitButton.Click += async (sender, e) =>
            {
                batLimitEditText.ClearFocus();
                float.TryParse(batLimitEditText.Text, out var batteryLimit);
                var outBuffer = GlobalOperationsIdClass.Parser.SetBatteryLimit(batteryLimit);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
            };

            setAntennaButton.Click += async (sender, e) =>
            {
                if (!GlobalOperationsIdClass.Gain.TryGetValue(antennaSpinner.SelectedItem.ToString(),
                    out var gain))
                {
                    Toast.MakeText(this, "Incorrect gain selected", ToastLength.Long).Show();
                    return;
                }
                var outBuffer = GlobalOperationsIdClass.Parser.SetGain(gain);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
            };

            setChipButton.Click += async (sender, e) =>
            {
                if (!RfidContainer.ChipTypes.Types.TryGetValue(chipTypeSpinner.SelectedItem.ToString(),
                    out var chipType))
                {
                    Toast.MakeText(this, "Incorrect chip type selected", ToastLength.Long).Show();
                    return;
                }
                var outBuffer = GlobalOperationsIdClass.Parser.SetChipType(chipType);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
            };

            setTeamSizeButton.Click += async (sender, e) =>
            {
                teamSizeEditText.ClearFocus();
                uint.TryParse(teamSizeEditText.Text, out var n);
                var outBuffer = GlobalOperationsIdClass.Parser.SetTeamFlashSize(n);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
            };

            setEraseBlockSizeButton.Click += async (sender, e) =>
            {
                flashSizeEditText.ClearFocus();
                uint.TryParse(flashSizeEditText.Text, out var n);
                var outBuffer = GlobalOperationsIdClass.Parser.SetEraseBlock(n);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
            };

            setBtNameButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.SetBtName(setBtNameEditText.Text);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
            };

            setBtPinButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.SetBtPin(setBtPinEditText.Text);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
            };

            sendBtCommandButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.SendBtCommand(sendBtCommandEditText.Text);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                _terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
            };

            setAutoreportButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.SetAutoReport(autoreportCheckBox.Checked);
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

            timeEditText.FocusChange += (sender, e) =>
            {
                var t = Helpers.DateStringToUnixTime(timeEditText.Text);
                timeEditText.Text = Helpers.DateToString(Helpers.ConvertFromUnixTimestamp(t));
                GlobalOperationsIdClass.ConfigPageState.SetTime = Helpers.ConvertFromUnixTimestamp(Helpers.DateStringToUnixTime(timeEditText.Text));
            };

            koeffEditText.FocusChange += (sender, e) =>
            {
                //koeffEditText.Text = koeffEditText.Text.Replace('.', ',');
                float.TryParse(koeffEditText.Text, out var batteryCoefficient);
                koeffEditText.Text = batteryCoefficient.ToString("F5");
            };

            batLimitEditText.FocusChange += (sender, e) =>
            {
                //batLimitEditText.Text = batLimitEditText.Text.Replace('.', ',');
                float.TryParse(batLimitEditText.Text, out var batteryLimit);
                batLimitEditText.Text = batteryLimit.ToString("F2");
            };

            teamSizeEditText.FocusChange += (sender, e) =>
            {
                uint.TryParse(teamSizeEditText.Text, out var n);
                teamSizeEditText.Text = n.ToString();
            };

            flashSizeEditText.FocusChange += (sender, e) =>
            {
                uint.TryParse(flashSizeEditText.Text, out var n);
                flashSizeEditText.Text = n.ToString();
            };

            setBtNameEditText.FocusChange += (sender, e) =>
            {
                if (setBtNameEditText.Text.Length > 32)
                    setBtNameEditText.Text = setBtNameEditText.Text.Substring(0, 32);
            };

            setBtPinEditText.FocusChange += (sender, e) =>
            {
                if (setBtPinEditText.Text.Length > 32)
                    setBtPinEditText.Text = setBtPinEditText.Text.Substring(0, 16);
            };

            currentCheckBox.CheckedChange += (sender, e) =>
            {
                timeEditText.Enabled = !currentCheckBox.Checked;
                GlobalOperationsIdClass.ConfigPageState.UseCurrentTime = currentCheckBox.Checked;
            };

            dumpSizeSpinner.ItemSelected += (sender, e) =>
            {
                GlobalOperationsIdClass.FlashSizeLimitDictionary.TryGetValue(dumpSizeSpinner.SelectedItem.ToString(),
                    out var size);
                GlobalOperationsIdClass.ConfigPageState.FlashLimitSize = size;
                GlobalOperationsIdClass.Flash = new FlashContainer(GlobalOperationsIdClass.FlashSizeLimit,
                    GlobalOperationsIdClass.StationSettings.TeamBlockSize);
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

            for (var n = 0; n < GlobalOperationsIdClass.Parser._repliesList.Count; n++)
            {
                var reply = GlobalOperationsIdClass.Parser._repliesList[n];
                GlobalOperationsIdClass.timerActiveTasks--;
                if (reply.ReplyCode != 0)
                {
                    GlobalOperationsIdClass.StatusPageState.TerminalText.Append(reply.ToString());

                    if (reply.ErrorCode == 0)
                    {
                        if (reply.ReplyCode == ProtocolParser.Reply.SET_TIME)
                        {
                            var replyDetails =
                                new ProtocolParser.ReplyData.SetTimeReply(reply);
                            _terminalTextView.Text += replyDetails.ToString();
                            GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.SEND_BT_COMMAND)
                        {
                            var replyDetails =
                                new ProtocolParser.ReplyData.SendBtCommandReply(reply);
                            _terminalTextView.Text += replyDetails.ToString();
                            GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());
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