using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;

using System;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Views.InputMethods;

namespace RfidStationControl
{
    [Activity(Label = "Config")]
    public class ActivityConfig : AppCompatActivity
    {
        #region UI controls

        private Button backButton;
        private Button getConfigButton;
        private Button setTimeButton;
        private Button setKoeffButton;
        private Button setBatLimitButton;
        private Button setAntennaButton;
        private Button setChipButton;
        private Button setTeamSizeButton;
        private Button setEraseBlockSizeButton;
        private Button setBtNameButton;
        private Button setBtPinButton;
        private Button sendBtCommandButton;
        private Button setAutoreportButton;
        private Button clearTerminalButton;

        private EditText timeEditText;
        private EditText koeffEditText;
        private EditText batLimitEditText;
        private EditText teamSizeEditText;
        private EditText flashSizeEditText;
        private EditText setBtNameEditText;
        private EditText setBtPinEditText;
        private EditText sendBtCommandEditText;

        private TextView flashSizeText;
        private TextView packetLengthTextView;
        private TextView terminalTextView;

        private Spinner antennaSpinner;
        private Spinner chipTypeSpinner;
        private Spinner dumpSizeSpinner;

        private CheckBox currentCheckBox;
        private CheckBox autoreportCheckBox;
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_config);

            // populate all controls
            backButton = FindViewById<Button>(Resource.Id.backButton);
            getConfigButton = FindViewById<Button>(Resource.Id.getConfigButton);
            setTimeButton = FindViewById<Button>(Resource.Id.setTimeButton);
            setKoeffButton = FindViewById<Button>(Resource.Id.setKoeffButton);
            setBatLimitButton = FindViewById<Button>(Resource.Id.setBatLimitButton);
            setAntennaButton = FindViewById<Button>(Resource.Id.setAntennaButton);
            setChipButton = FindViewById<Button>(Resource.Id.setChipButton);
            setTeamSizeButton = FindViewById<Button>(Resource.Id.setTeamSizeButton);
            setEraseBlockSizeButton = FindViewById<Button>(Resource.Id.setFlashSizeButton);
            setBtNameButton = FindViewById<Button>(Resource.Id.setBtNameButton);
            setBtPinButton = FindViewById<Button>(Resource.Id.setBtPinButton);
            sendBtCommandButton = FindViewById<Button>(Resource.Id.sendBtCommandButton);
            setAutoreportButton = FindViewById<Button>(Resource.Id.setAutoreportButton);
            clearTerminalButton = FindViewById<Button>(Resource.Id.clearTerminalButton);

            timeEditText = FindViewById<EditText>(Resource.Id.timeEditText);
            koeffEditText = FindViewById<EditText>(Resource.Id.koeffEditText);
            batLimitEditText = FindViewById<EditText>(Resource.Id.batLimitEditText);
            teamSizeEditText = FindViewById<EditText>(Resource.Id.teamSizeEditText);
            flashSizeEditText = FindViewById<EditText>(Resource.Id.flashSizeEditText);
            setBtNameEditText = FindViewById<EditText>(Resource.Id.setBtNameEditText);
            setBtPinEditText = FindViewById<EditText>(Resource.Id.setBtPinEditText);
            sendBtCommandEditText = FindViewById<EditText>(Resource.Id.sendBtCommandEditText);

            flashSizeText = FindViewById<TextView>(Resource.Id.flashSizeTextView);
            packetLengthTextView = FindViewById<TextView>(Resource.Id.packetLengthTextView);
            terminalTextView = FindViewById<TextView>(Resource.Id.terminalTextView);

            antennaSpinner = FindViewById<Spinner>(Resource.Id.antennaSpinner);
            chipTypeSpinner = FindViewById<Spinner>(Resource.Id.chipSpinner);
            dumpSizeSpinner = FindViewById<Spinner>(Resource.Id.dumpSizeSpinner);

            currentCheckBox = FindViewById<CheckBox>(Resource.Id.currentCheckBox);
            autoreportCheckBox = FindViewById<CheckBox>(Resource.Id.autoreportCheckBox);

            //Initialise page
            Title = "Station " + GlobalOperationsIdClass.StationSettings.Number.ToString() + " configuration";

            timeEditText.Text = Helpers.DateToString(DateTime.Now);
            koeffEditText.Text = GlobalOperationsIdClass.StationSettings.VoltageCoefficient.ToString("F5");
            batLimitEditText.Text = GlobalOperationsIdClass.StationSettings.BatteryLimit.ToString("F2");
            teamSizeEditText.Text = GlobalOperationsIdClass.StationSettings.TeamBlockSize.ToString();
            flashSizeEditText.Text = GlobalOperationsIdClass.StationSettings.EraseBlockSize.ToString();
            flashSizeText.Text = (GlobalOperationsIdClass.StationSettings.FlashSize / 1024 / 1024).ToString("F2") + "Mb";
            packetLengthTextView.Text = (GlobalOperationsIdClass.StationSettings.packetLengthkSize).ToString();
            setBtNameEditText.Text = GlobalOperationsIdClass.StationSettings.BtName;
            setBtPinEditText.Text = GlobalOperationsIdClass.StationSettings.BtPin;
            sendBtCommandEditText.Text = GlobalOperationsIdClass.StationSettings.BtCommand;
            currentCheckBox.Checked = GlobalOperationsIdClass.ConfigPageState.UseCurrentTime;
            timeEditText.Enabled = !currentCheckBox.Checked;

            terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();

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
                getConfigButton.Enabled = true;
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
                setAutoreportButton.Enabled = true;
            }
            else
            {
                getConfigButton.Enabled = false;
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
                setAutoreportButton.Enabled = false;
            }

            GlobalOperationsIdClass.TimerActiveTasks = 0;

            var inputManager = (InputMethodManager)GetSystemService(Context.InputMethodService);
            var currentFocus = Window.CurrentFocus;
            inputManager.HideSoftInputFromWindow(currentFocus?.WindowToken, HideSoftInputFlags.None);

            getConfigButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.GetConfig();
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();
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
                terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();
            };

            setKoeffButton.Click += async (sender, e) =>
            {
                koeffEditText.ClearFocus();
                float.TryParse(koeffEditText.Text, out var batteryCoefficient);
                var outBuffer = GlobalOperationsIdClass.Parser.SetVCoeff(batteryCoefficient);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();
            };

            setBatLimitButton.Click += async (sender, e) =>
            {
                batLimitEditText.ClearFocus();
                float.TryParse(batLimitEditText.Text, out var batteryLimit);
                var outBuffer = GlobalOperationsIdClass.Parser.SetBatteryLimit(batteryLimit);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();
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
                terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();
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
                terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();
            };

            setTeamSizeButton.Click += async (sender, e) =>
            {
                teamSizeEditText.ClearFocus();
                uint.TryParse(teamSizeEditText.Text, out var n);
                var outBuffer = GlobalOperationsIdClass.Parser.SetTeamFlashSize(n);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();
            };

            setEraseBlockSizeButton.Click += async (sender, e) =>
            {
                flashSizeEditText.ClearFocus();
                uint.TryParse(flashSizeEditText.Text, out var n);
                var outBuffer = GlobalOperationsIdClass.Parser.SetEraseBlock(n);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();
            };

            setBtNameButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.SetBtName(setBtNameEditText.Text);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();
            };

            setBtPinButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.SetBtPin(setBtPinEditText.Text);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();
            };

            sendBtCommandButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.SendBtCommand(sendBtCommandEditText.Text);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();
            };

            setAutoreportButton.Click += async (sender, e) =>
            {
                var outBuffer = GlobalOperationsIdClass.Parser.SetAutoReport(autoreportCheckBox.Checked);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
                terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();
            };

            clearTerminalButton.Click += (sender, e) =>
            {
                GlobalOperationsIdClass.StatusPageState.TerminalText.Clear();
                terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();
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
                        if (reply.ReplyCode == ProtocolParser.Reply.GET_CONFIG)
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

            terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();
            terminalTextView.Invalidate();
            return packageReceived;
        }
    }
}