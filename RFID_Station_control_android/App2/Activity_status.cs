using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace RfidStationControl
{
    [Activity(Label = "Status")]
    public class ActivityStatus : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_status);

            // populate all controls
            Button getStatusButton = FindViewById<Button>(Resource.Id.getStatusButton);
            Button getConfigButton = FindViewById<Button>(Resource.Id.getConfigButton);
            Button getErrorsButton = FindViewById<Button>(Resource.Id.getErrorsButton);
            Button setModeButton = FindViewById<Button>(Resource.Id.setModeButton);
            Button resetButton = FindViewById<Button>(Resource.Id.resetStationButton);
            Button clearTerminalButton = FindViewById<Button>(Resource.Id.clearTerminalButton);
            Button backButton = FindViewById<Button>(Resource.Id.backButton);

            EditText newStationNumberText = FindViewById<EditText>(Resource.Id.newStationNumberEditText);
            EditText chipsCheckedText = FindViewById<EditText>(Resource.Id.chipsCheckedEditText);
            EditText lastCheckEditText = FindViewById<EditText>(Resource.Id.lastCheckEditText);

            Spinner modeListSpinner = FindViewById<Spinner>(Resource.Id.modeListSpinner);

            TextView terminalTextView = FindViewById<TextView>(Resource.Id.terminalTextView);

            Title = "Station " + GlobalOperationsIdClass.StationSettings.Number.ToString() + " status";

            newStationNumberText.Text = GlobalOperationsIdClass.StatusPageState.NewStationNumber.ToString();
            chipsCheckedText.Text = GlobalOperationsIdClass.StatusPageState.CheckedChipsNumber.ToString();
            lastCheckEditText.Text = Helpers.DateToString(GlobalOperationsIdClass.StatusPageState.LastCheck);

            terminalTextView.Text = GlobalOperationsIdClass.StatusPageState.TerminalText.ToString();

            string[] items = GlobalOperationsIdClass.StationMode.Keys.ToArray();
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
                byte[] outBuffer = GlobalOperationsIdClass.Parser.getStatus();
                // Write data to the BtDevice
                if (!await GlobalOperationsIdClass.Bt.WriteBtAsync(outBuffer))
                {
                    Toast.MakeText(this, "Can't write to Bluetooth.", ToastLength.Long).Show();
                    //throw;
                }
                GlobalOperationsIdClass.timerActiveTasks++;
                StartTimer(GlobalOperationsIdClass.BtReadPeriod, readBt);

                terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n");
                //terminalTextView.Invalidate();
            };

            getConfigButton.Click += async (sender, e) =>
            {
                byte[] outBuffer = GlobalOperationsIdClass.Parser.getConfig();
                // Write data to the BtDevice
                if (!await GlobalOperationsIdClass.Bt.WriteBtAsync(outBuffer))
                {
                    Toast.MakeText(this, "Can't write to Bluetooth.", ToastLength.Long).Show();
                    //throw;
                }
                GlobalOperationsIdClass.timerActiveTasks++;
                if (GlobalOperationsIdClass.timerActiveTasks == 1)
                    StartTimer(GlobalOperationsIdClass.BtReadPeriod, readBt);

                terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n");
                //terminalTextView.Invalidate();
            };

            getErrorsButton.Click += async (sender, e) =>
            {
                byte[] outBuffer = GlobalOperationsIdClass.Parser.getLastErrors();
                // Write data to the BtDevice
                if (!await GlobalOperationsIdClass.Bt.WriteBtAsync(outBuffer))
                {
                    Toast.MakeText(this, "Can't write to Bluetooth.", ToastLength.Long).Show();
                    //throw;
                }
                GlobalOperationsIdClass.timerActiveTasks++;
                if (GlobalOperationsIdClass.timerActiveTasks == 1)
                    StartTimer(GlobalOperationsIdClass.BtReadPeriod, readBt);

                terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n");
                //terminalTextView.Invalidate();
            };

            setModeButton.Click += async (sender, e) =>
            {
                if (!GlobalOperationsIdClass.StationMode.TryGetValue(
                    modeListSpinner.SelectedItem.ToString(), out byte modeNumber))
                {
                    Toast.MakeText(this, "Incorrect mode selected", ToastLength.Long).Show();
                    return;
                }
                byte[] outBuffer = GlobalOperationsIdClass.Parser.setMode(modeNumber);
                // Write data to the BtDevice
                if (!await GlobalOperationsIdClass.Bt.WriteBtAsync(outBuffer))
                {
                    Toast.MakeText(this, "Can't write to Bluetooth.", ToastLength.Long).Show();
                    //throw;
                }
                GlobalOperationsIdClass.timerActiveTasks++;
                if (GlobalOperationsIdClass.timerActiveTasks == 1)
                    StartTimer(GlobalOperationsIdClass.BtReadPeriod, readBt);

                terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n");
                //terminalTextView.Invalidate();
            };

            resetButton.Click += async (sender, e) =>
            {
                chipsCheckedText.ClearFocus();
                lastCheckEditText.ClearFocus();
                newStationNumberText.ClearFocus();
                byte[] outBuffer = GlobalOperationsIdClass.Parser.resetStation(GlobalOperationsIdClass.StatusPageState.CheckedChipsNumber, GlobalOperationsIdClass.StatusPageState.LastCheck, GlobalOperationsIdClass.StatusPageState.NewStationNumber);
                // Write data to the BtDevice
                if (!await GlobalOperationsIdClass.Bt.WriteBtAsync(outBuffer))
                {
                    Toast.MakeText(this, "Can't write to Bluetooth.", ToastLength.Long).Show();
                    //throw;
                }
                GlobalOperationsIdClass.timerActiveTasks++;
                if (GlobalOperationsIdClass.timerActiveTasks == 1)
                    StartTimer(GlobalOperationsIdClass.BtReadPeriod, readBt);

                terminalTextView.Text += ">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n";
                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(">> " + Helpers.ConvertByteArrayToHex(outBuffer) + "\r\n");
                terminalTextView.Invalidate();
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

            newStationNumberText.FocusChange += (sender, e) =>
            {
                byte.TryParse(newStationNumberText.Text, out byte n);
                newStationNumberText.Text = n.ToString();
                GlobalOperationsIdClass.StatusPageState.NewStationNumber = n;
            };

            chipsCheckedText.FocusChange += (sender, e) =>
            {
                byte.TryParse(chipsCheckedText.Text, out byte n);
                chipsCheckedText.Text = n.ToString();
                GlobalOperationsIdClass.StatusPageState.CheckedChipsNumber = n;
            };

            lastCheckEditText.FocusChange += (sender, e) =>
            {
                long t = Helpers.DateStringToUnixTime(lastCheckEditText.Text);
                lastCheckEditText.Text = Helpers.DateToString(Helpers.ConvertFromUnixTimestamp(t));
                GlobalOperationsIdClass.StatusPageState.LastCheck = Helpers.ConvertFromUnixTimestamp(t);
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
                Spinner modeListSpinner = FindViewById<Spinner>(Resource.Id.modeListSpinner);

                foreach (var reply in GlobalOperationsIdClass.Parser._repliesList)
                {
                    GlobalOperationsIdClass.timerActiveTasks--;
                    if (reply.ReplyCode != 0)
                    {
                        terminalTextView.Text += reply.ToString();
                        GlobalOperationsIdClass.StatusPageState.TerminalText.Append(reply.ToString());

                        if (reply.ErrorCode == 0)
                        {
                            if (reply.ReplyCode == ProtocolParser.Reply.GET_STATUS)
                            {
                                ProtocolParser.ReplyData.getStatusReply replyDetails =
                                    new ProtocolParser.ReplyData.getStatusReply(reply);
                                terminalTextView.Text += replyDetails.ToString();
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
                                ProtocolParser.ReplyData.getConfigReply replyDetails =
                                    new ProtocolParser.ReplyData.getConfigReply(reply);
                                terminalTextView.Text += replyDetails.ToString();
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
                                foreach (var x in RfidContainer.ChipTypes._ids)
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
                                {
                                    GlobalOperationsIdClass.Rfid =
                                        new RfidContainer(GlobalOperationsIdClass.StationSettings.ChipType);
                                }

                                GlobalOperationsIdClass.StationSettings.AntennaGain = replyDetails.AntennaGain;

                                GlobalOperationsIdClass.StationSettings.Mode = replyDetails.Mode;
                                modeListSpinner.SetSelection(GlobalOperationsIdClass.StationSettings.Mode);

                                GlobalOperationsIdClass.StationSettings.BatteryLimit = replyDetails.BatteryLimit;

                                GlobalOperationsIdClass.StationSettings.EraseBlockSize = replyDetails.EraseBlockSize;

                                GlobalOperationsIdClass.StationSettings.FlashSize = replyDetails.FlashSize;

                                GlobalOperationsIdClass.StationSettings.TeamBlockSize = replyDetails.TeamBlockSize;
                                if (GlobalOperationsIdClass.StationSettings.TeamBlockSize !=
                                    GlobalOperationsIdClass.Flash.TeamDumpSize)
                                {
                                    GlobalOperationsIdClass.Flash = new FlashContainer(
                                        GlobalOperationsIdClass.FlashSizeLimit,
                                        GlobalOperationsIdClass.StationSettings.TeamBlockSize,
                                        0); // GlobalOperationsIdClass.StationSettings.TeamBlockSize
                                }

                                GlobalOperationsIdClass.StationSettings.VoltageCoefficient = replyDetails.VoltageKoeff;
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