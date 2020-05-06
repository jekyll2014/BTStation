using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RfidStationControl
{
    [Activity(Label = "Config")]
    public class ActivityConfig : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_config);

            // populate all controls
            Button backButton = FindViewById<Button>(Resource.Id.backButton);
            Button getConfigButton = FindViewById<Button>(Resource.Id.getConfigButton);
            Button setTimeButton = FindViewById<Button>(Resource.Id.setTimeButton);
            Button setKoeffButton = FindViewById<Button>(Resource.Id.setKoeffButton);
            Button setBatLimitButton = FindViewById<Button>(Resource.Id.setBatLimitButton);
            Button setAntennaButton = FindViewById<Button>(Resource.Id.setAntennaButton);
            Button setChipButton = FindViewById<Button>(Resource.Id.setChipButton);
            Button setTeamSizeButton = FindViewById<Button>(Resource.Id.setTeamSizeButton);
            Button setEraseBlockSizeButton = FindViewById<Button>(Resource.Id.setFlashSizeButton);
            Button setBtNameButton = FindViewById<Button>(Resource.Id.setBtNameButton);
            Button setBtPinButton = FindViewById<Button>(Resource.Id.setBtPinButton);
            Button sendBtCommandButton = FindViewById<Button>(Resource.Id.sendBtCommandButton);
            Button setAutoreportButton = FindViewById<Button>(Resource.Id.setAutoreportButton);
            Button clearTerminalButton = FindViewById<Button>(Resource.Id.clearTerminalButton);

            EditText timeEditText = FindViewById<EditText>(Resource.Id.timeEditText);
            EditText koeffEditText = FindViewById<EditText>(Resource.Id.koeffEditText);
            EditText batLimitEditText = FindViewById<EditText>(Resource.Id.batLimitEditText);
            EditText teamSizeEditText = FindViewById<EditText>(Resource.Id.teamSizeEditText);
            EditText flashSizeEditText = FindViewById<EditText>(Resource.Id.flashSizeEditText);
            EditText setBtNameEditText = FindViewById<EditText>(Resource.Id.setBtNameEditText);
            EditText setBtPinEditText = FindViewById<EditText>(Resource.Id.setBtPinEditText);
            EditText sendBtCommandEditText = FindViewById<EditText>(Resource.Id.sendBtCommandEditText);

            TextView flashSizeText = FindViewById<TextView>(Resource.Id.flashSizeTextView);
            TextView terminalTextView = FindViewById<TextView>(Resource.Id.terminalTextView);

            Spinner antennaSpinner = FindViewById<Spinner>(Resource.Id.antennaSpinner);
            Spinner chipTypeSpinner = FindViewById<Spinner>(Resource.Id.chipSpinner);
            Spinner dumpSizeSpinner = FindViewById<Spinner>(Resource.Id.dumpSizeSpinner);

            CheckBox currentCheckBox = FindViewById<CheckBox>(Resource.Id.currentCheckBox);
            CheckBox autoreportCheckBox = FindViewById<CheckBox>(Resource.Id.autoreportCheckBox);

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

            terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();

            string[] items = GlobalOperationsIdClass.Gain.Keys.ToArray();
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, items);
            antennaSpinner.Adapter = adapter;
            antennaSpinner.SetSelection(5);
            int g = 0;
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
                byte[] outBuffer = GlobalOperationsIdClass.Parser.getConfig();
                // Write data to the BtDevice
                try
                {
                    await GlobalOperationsIdClass.Bt.WriteBtAsync(outBuffer);
                }
                catch (Exception exception)
                {
                    Toast.MakeText(this, "Can't write to Bluetooth.", ToastLength.Long).Show();
                    //throw;
                }
                GlobalOperationsIdClass.timerActiveTasks++;
                if (GlobalOperationsIdClass.timerActiveTasks == 1)
                    StartTimer(GlobalOperationsIdClass.BtReadPeriod, readBt);

                terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n");
            };

            setTimeButton.Click += async (sender, e) =>
            {
                byte[] outBuffer;
                if (currentCheckBox.Checked)
                    outBuffer = GlobalOperationsIdClass.Parser.setTime(DateTime.Now);
                else
                {
                    timeEditText.ClearFocus();
                    outBuffer = GlobalOperationsIdClass.Parser.setTime(GlobalOperationsIdClass.ConfigPageState.SetTime);
                }
                // Write data to the BtDevice
                try
                {
                    await GlobalOperationsIdClass.Bt.WriteBtAsync(outBuffer);
                }
                catch (Exception exception)
                {
                    Toast.MakeText(this, "Can't write to Bluetooth.", ToastLength.Long).Show();
                    //throw;
                }
                terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n");
                GlobalOperationsIdClass.timerActiveTasks++;
                if (GlobalOperationsIdClass.timerActiveTasks == 1)
                    StartTimer(GlobalOperationsIdClass.BtReadPeriod, readBt);
                //terminalTextView.Invalidate();
            };

            setKoeffButton.Click += async (sender, e) =>
            {
                koeffEditText.ClearFocus();
                float.TryParse(koeffEditText.Text, out float batteryCoefficient);
                byte[] outBuffer = GlobalOperationsIdClass.Parser.setVCoeff(batteryCoefficient);
                // Write data to the BtDevice
                try
                {
                    await GlobalOperationsIdClass.Bt.WriteBtAsync(outBuffer);
                }
                catch (Exception exception)
                {
                    Toast.MakeText(this, "Can't write to Bluetooth.", ToastLength.Long).Show();
                    //throw;
                }
                terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n");
                GlobalOperationsIdClass.timerActiveTasks++;
                if (GlobalOperationsIdClass.timerActiveTasks == 1)
                    StartTimer(GlobalOperationsIdClass.BtReadPeriod, readBt);
                //terminalTextView.Invalidate();
            };

            setBatLimitButton.Click += async (sender, e) =>
            {
                batLimitEditText.ClearFocus();
                float.TryParse(batLimitEditText.Text, out float batteryLimit);
                byte[] outBuffer = GlobalOperationsIdClass.Parser.setBatteryLimit(batteryLimit);
                // Write data to the BtDevice
                try
                {
                    await GlobalOperationsIdClass.Bt.WriteBtAsync(outBuffer);
                }
                catch (Exception exception)
                {
                    Toast.MakeText(this, "Can't write to Bluetooth.", ToastLength.Long).Show();
                    //throw;
                }
                terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n");
                GlobalOperationsIdClass.timerActiveTasks++;
                if (GlobalOperationsIdClass.timerActiveTasks == 1)
                    StartTimer(GlobalOperationsIdClass.BtReadPeriod, readBt);
                //terminalTextView.Invalidate();
            };

            setAntennaButton.Click += async (sender, e) =>
            {
                if (!GlobalOperationsIdClass.Gain.TryGetValue(antennaSpinner.SelectedItem.ToString(),
                    out byte gain))
                {
                    Toast.MakeText(this, "Incorrect gain selected", ToastLength.Long).Show();
                    return;
                }
                byte[] outBuffer = GlobalOperationsIdClass.Parser.setGain(gain);
                // Write data to the BtDevice
                try
                {
                    await GlobalOperationsIdClass.Bt.WriteBtAsync(outBuffer);
                }
                catch (Exception exception)
                {
                    Toast.MakeText(this, "Can't write to Bluetooth.", ToastLength.Long).Show();
                    //throw;
                }
                terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n");
                GlobalOperationsIdClass.timerActiveTasks++;
                if (GlobalOperationsIdClass.timerActiveTasks == 1)
                    StartTimer(GlobalOperationsIdClass.BtReadPeriod, readBt);
                //terminalTextView.Invalidate();
            };

            setChipButton.Click += async (sender, e) =>
            {
                if (!RfidContainer.ChipTypes.Types.TryGetValue(chipTypeSpinner.SelectedItem.ToString(),
                    out byte chipType))
                {
                    Toast.MakeText(this, "Incorrect chip type selected", ToastLength.Long).Show();
                    return;
                }
                byte[] outBuffer = GlobalOperationsIdClass.Parser.setChipType(chipType);
                // Write data to the BtDevice
                try
                {
                    await GlobalOperationsIdClass.Bt.WriteBtAsync(outBuffer);
                }
                catch (Exception exception)
                {
                    Toast.MakeText(this, "Can't write to Bluetooth.", ToastLength.Long).Show();
                    //throw;
                }
                terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n");
                GlobalOperationsIdClass.timerActiveTasks++;
                if (GlobalOperationsIdClass.timerActiveTasks == 1)
                    StartTimer(GlobalOperationsIdClass.BtReadPeriod, readBt);
                //terminalTextView.Invalidate();
            };

            setTeamSizeButton.Click += async (sender, e) =>
            {
                teamSizeEditText.ClearFocus();
                UInt32.TryParse(teamSizeEditText.Text, out UInt32 n);
                byte[] outBuffer = GlobalOperationsIdClass.Parser.setTeamFlashSize(n);
                // Write data to the BtDevice
                try
                {
                    await GlobalOperationsIdClass.Bt.WriteBtAsync(outBuffer);
                }
                catch (Exception exception)
                {
                    Toast.MakeText(this, "Can't write to Bluetooth.", ToastLength.Long).Show();
                    //throw;
                }
                terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n");
                GlobalOperationsIdClass.timerActiveTasks++;
                if (GlobalOperationsIdClass.timerActiveTasks == 1)
                    StartTimer(GlobalOperationsIdClass.BtReadPeriod, readBt);
                //terminalTextView.Invalidate();
            };

            setEraseBlockSizeButton.Click += async (sender, e) =>
            {
                flashSizeEditText.ClearFocus();
                UInt32.TryParse(flashSizeEditText.Text, out UInt32 n);
                byte[] outBuffer = GlobalOperationsIdClass.Parser.setEraseBlock(n);
                // Write data to the BtDevice
                try
                {
                    await GlobalOperationsIdClass.Bt.WriteBtAsync(outBuffer);
                }
                catch (Exception exception)
                {
                    Toast.MakeText(this, "Can't write to Bluetooth.", ToastLength.Long).Show();
                    //throw;
                }
                terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n");
                GlobalOperationsIdClass.timerActiveTasks++;
                if (GlobalOperationsIdClass.timerActiveTasks == 1)
                    StartTimer(GlobalOperationsIdClass.BtReadPeriod, readBt);
                //terminalTextView.Invalidate();
            };

            setBtNameButton.Click += async (sender, e) =>
            {
                byte[] outBuffer = GlobalOperationsIdClass.Parser.SetBtName(setBtNameEditText.Text);
                // Write data to the BtDevice
                try
                {
                    await GlobalOperationsIdClass.Bt.WriteBtAsync(outBuffer);
                }
                catch (Exception exception)
                {
                    Toast.MakeText(this, "Can't write to Bluetooth.", ToastLength.Long).Show();
                    //throw;
                }
                terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n");
                GlobalOperationsIdClass.timerActiveTasks++;
                if (GlobalOperationsIdClass.timerActiveTasks == 1)
                    StartTimer(GlobalOperationsIdClass.BtReadPeriod, readBt);
                //terminalTextView.Invalidate();
            };

            setBtPinButton.Click += async (sender, e) =>
            {
                byte[] outBuffer = GlobalOperationsIdClass.Parser.SetBtPin(setBtPinEditText.Text);
                // Write data to the BtDevice
                try
                {
                    await GlobalOperationsIdClass.Bt.WriteBtAsync(outBuffer);
                }
                catch (Exception exception)
                {
                    Toast.MakeText(this, "Can't write to Bluetooth.", ToastLength.Long).Show();
                    //throw;
                }
                terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n");
                GlobalOperationsIdClass.timerActiveTasks++;
                if (GlobalOperationsIdClass.timerActiveTasks == 1)
                    StartTimer(GlobalOperationsIdClass.BtReadPeriod, readBt);
                //terminalTextView.Invalidate();
            };

            sendBtCommandButton.Click += async (sender, e) =>
            {
                byte[] outBuffer = GlobalOperationsIdClass.Parser.sendBtCommand(sendBtCommandEditText.Text);
                // Write data to the BtDevice
                try
                {
                    await GlobalOperationsIdClass.Bt.WriteBtAsync(outBuffer);
                }
                catch (Exception exception)
                {
                    Toast.MakeText(this, "Can't write to Bluetooth.", ToastLength.Long).Show();
                    //throw;
                }
                terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n");
                GlobalOperationsIdClass.timerActiveTasks++;
                if (GlobalOperationsIdClass.timerActiveTasks == 1)
                    StartTimer(GlobalOperationsIdClass.BtReadPeriod, readBt);
                //terminalTextView.Invalidate();
            };

            setAutoreportButton.Click += async (sender, e) =>
            {
                byte[] outBuffer = GlobalOperationsIdClass.Parser.setAutoReport(autoreportCheckBox.Checked);
                // Write data to the BtDevice
                try
                {
                    await GlobalOperationsIdClass.Bt.WriteBtAsync(outBuffer);
                }
                catch (Exception exception)
                {
                    Toast.MakeText(this, "Can't write to Bluetooth.", ToastLength.Long).Show();
                    //throw;
                }
                terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n");
                GlobalOperationsIdClass.timerActiveTasks++;
                if (GlobalOperationsIdClass.timerActiveTasks == 1)
                    StartTimer(GlobalOperationsIdClass.BtReadPeriod, readBt);
                //terminalTextView.Invalidate();
            };

            clearTerminalButton.Click += (sender, e) =>
            {
                terminalTextView.Text = "";
                GlobalOperationsIdClass.StatusPageState.TerminalText.Clear();
            };

            backButton.Click += (sender, e) =>
            {
                Finish();
            };

            timeEditText.FocusChange += (sender, e) =>
            {
                long t = Helpers.DateStringToUnixTime(timeEditText.Text);
                timeEditText.Text = Helpers.DateToString(Helpers.ConvertFromUnixTimestamp(t));
                GlobalOperationsIdClass.ConfigPageState.SetTime = Helpers.ConvertFromUnixTimestamp(Helpers.DateStringToUnixTime(timeEditText.Text));
            };

            koeffEditText.FocusChange += (sender, e) =>
            {
                //koeffEditText.Text = koeffEditText.Text.Replace('.', ',');
                float.TryParse(koeffEditText.Text, out float batteryCoefficient);
                koeffEditText.Text = batteryCoefficient.ToString("F5");
            };

            batLimitEditText.FocusChange += (sender, e) =>
            {
                //batLimitEditText.Text = batLimitEditText.Text.Replace('.', ',');
                float.TryParse(batLimitEditText.Text, out float batteryLimit);
                batLimitEditText.Text = batteryLimit.ToString("F2");
            };

            teamSizeEditText.FocusChange += (sender, e) =>
            {
                UInt32.TryParse(teamSizeEditText.Text, out UInt32 n);
                teamSizeEditText.Text = n.ToString();
            };

            flashSizeEditText.FocusChange += (sender, e) =>
            {
                UInt32.TryParse(flashSizeEditText.Text, out UInt32 n);
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
                    out UInt32 size);
                GlobalOperationsIdClass.ConfigPageState.FlashLimitSize = size;
                GlobalOperationsIdClass.Flash = new FlashContainer(GlobalOperationsIdClass.FlashSizeLimit,
                    GlobalOperationsIdClass.StationSettings.TeamBlockSize);
            };
        }

        public void StartTimer(long gap, Func<Task<bool>> callback)
        {
            Handler handler = new Handler(Looper.MainLooper);
            handler.PostDelayed(() =>
            {
                if (GlobalOperationsIdClass.Bt.BtInputBuffer.Count > 0)
                    callback();

                if (GlobalOperationsIdClass.timerActiveTasks > 0)
                    StartTimer(gap, callback);
                //else StartTimer(gap * 10, callback);

                handler.Dispose();
                handler = null;
            }, gap);
        }

        async Task<bool> readBt()
        {
            bool packageReceived = false;
            lock (GlobalOperationsIdClass.Bt.SerialReceiveThreadLock)
            {
                packageReceived = GlobalOperationsIdClass.Parser.AddData(GlobalOperationsIdClass.Bt.BtInputBuffer);
            }

            if (GlobalOperationsIdClass.Parser._repliesList.Count > 0)
            {
                TextView terminalTextView = FindViewById<TextView>(Resource.Id.terminalTextView);

                foreach (var reply in GlobalOperationsIdClass.Parser._repliesList)
                {
                    GlobalOperationsIdClass.timerActiveTasks--;
                    if (reply.ReplyCode != 0)
                    {
                        GlobalOperationsIdClass.StatusPageState.TerminalText.Append(reply.ToString());

                        if (reply.ErrorCode == 0)
                        {
                            if (reply.ReplyCode == ProtocolParser.Reply.SET_TIME)
                            {
                                ProtocolParser.ReplyData.setTimeReply replyDetails =
                                    new ProtocolParser.ReplyData.setTimeReply(reply);
                                terminalTextView.Text += replyDetails.ToString();
                                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());
                            }
                            else if (reply.ReplyCode == ProtocolParser.Reply.SEND_BT_COMMAND)
                            {
                                ProtocolParser.ReplyData.sendBtCommandReply replyDetails =
                                    new ProtocolParser.ReplyData.sendBtCommandReply(reply);
                                terminalTextView.Text += replyDetails.ToString();
                                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());
                            }
                            terminalTextView.Invalidate();
                        }
                        GlobalOperationsIdClass.Parser._repliesList.Remove(reply);
                        Toast.MakeText(this,
                            ProtocolParser.ReplyStrings[reply.ReplyCode] + " replied: " +
                            ProtocolParser.ErrorCodes[reply.ErrorCode], ToastLength.Long).Show();
                    }
                    else
                    {
                        terminalTextView.Text += reply.Message;
                        GlobalOperationsIdClass.StatusPageState.TerminalText.Append(reply.Message);
                    }
                }
            }

            return packageReceived;
        }
    }
}