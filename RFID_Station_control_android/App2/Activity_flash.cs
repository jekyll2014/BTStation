using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using static RfidStationControl.GlobalOperationsIdClass.FlashPageState;

namespace RfidStationControl
{
    [Activity(Label = "Flash")]
    public class ActivityFlash : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_flash);

            // populate all controls
            Button readFlashButton = FindViewById<Button>(Resource.Id.readFlashButton);
            Button writeFlashButton = FindViewById<Button>(Resource.Id.writeFlashButton);
            Button dumpFlashButton = FindViewById<Button>(Resource.Id.dumpFlashButton);
            Button quickDumpFlashButton = FindViewById<Button>(Resource.Id.quickDumpFlashButton);
            Button clearGridButton = FindViewById<Button>(Resource.Id.clearGridButton);
            Button backButton = FindViewById<Button>(Resource.Id.backButton);

            EditText readFromEditText = FindViewById<EditText>(Resource.Id.readFromEditText);
            EditText lengthEditText = FindViewById<EditText>(Resource.Id.lengthEditText);
            EditText writeFromEditText = FindViewById<EditText>(Resource.Id.writeFromEditText);
            EditText dataEditText = FindViewById<EditText>(Resource.Id.dataEditText);

            GridView flashGridView = FindViewById<GridView>(Resource.Id.flashGridView);

            if (table.Count == 0)
            {
                var row = new FlashTableItem
                {
                    TeamNum = "Team#",
                    Decoded = "Decoded data"
                };
                table.Add(row);
            }

            flashGridView.Adapter = new FlashGridAdapter(this, table);

            Title = "Station " + GlobalOperationsIdClass.StationSettings.Number.ToString() + " Flash";
            readFromEditText.Text = GlobalOperationsIdClass.FlashPageState.ReadAddress.ToString();
            lengthEditText.Text = GlobalOperationsIdClass.FlashPageState.ReadLength.ToString();
            writeFromEditText.Text = GlobalOperationsIdClass.FlashPageState.WriteAddress.ToString();
            dataEditText.Text = Helpers.ConvertByteArrayToHex(GlobalOperationsIdClass.FlashPageState.WriteData);

            if (GlobalOperationsIdClass.Bt.IsBtEnabled() && GlobalOperationsIdClass.Bt.IsBtConnected())
            {
                readFlashButton.Enabled = true;
                writeFlashButton.Enabled = true;
                dumpFlashButton.Enabled = true;
                quickDumpFlashButton.Enabled = true;
            }
            else
            {
                readFlashButton.Enabled = false;
                writeFlashButton.Enabled = false;
                dumpFlashButton.Enabled = false;
                quickDumpFlashButton.Enabled = false;
            }

            GlobalOperationsIdClass.timerActiveTasks = 0;

            readFlashButton.Click += async (sender, e) =>
            {
                readFromEditText.ClearFocus();
                lengthEditText.ClearFocus();
                byte[] outBuffer = GlobalOperationsIdClass.Parser.readFlash(GlobalOperationsIdClass.FlashPageState.ReadAddress, GlobalOperationsIdClass.FlashPageState.ReadLength);
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
                if (GlobalOperationsIdClass.timerActiveTasks == 1) StartTimer(GlobalOperationsIdClass.BtReadPeriod, readBt);
            };

            writeFlashButton.Click += async (sender, e) =>
            {
                writeFromEditText.ClearFocus();
                dataEditText.ClearFocus();
                byte[] outBuffer = GlobalOperationsIdClass.Parser.writeFlash(GlobalOperationsIdClass.FlashPageState.WriteAddress, GlobalOperationsIdClass.FlashPageState.WriteData);
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
                if (GlobalOperationsIdClass.timerActiveTasks == 1) StartTimer(GlobalOperationsIdClass.BtReadPeriod, readBt);
            };

            dumpFlashButton.Click += async (sender, e) =>
            {
                dumpFlashButton.Enabled = false;
                string tmp = dumpFlashButton.Text;
                dumpFlashButton.Text = "Dumping...";

                byte maxFrameBytes = 256 - 7 - ProtocolParser.ReplyDataLength.READ_FLASH - 1;
                UInt32 addrFrom = 0;
                UInt32 addrTo = 0;
                GlobalOperationsIdClass.dumpCancellation = false;
                do
                {
                    addrTo = addrFrom + maxFrameBytes;
                    if (addrTo >= GlobalOperationsIdClass.FlashSizeLimit)
                        addrTo = GlobalOperationsIdClass.FlashSizeLimit;
                    dumpFlashButton.Text = "Dumping " + addrTo.ToString() + "bytes/" + (GlobalOperationsIdClass.FlashSizeLimit).ToString() + "Mb";
                    dumpFlashButton.Invalidate();

                    byte[] outBuffer = GlobalOperationsIdClass.Parser.readFlash(addrFrom, (byte)(addrTo - addrFrom));
                    // Write data to the BtDevice
                    try
                    {
                        await GlobalOperationsIdClass.Bt.WriteBtAsync(outBuffer);
                    }
                    catch (Exception exception)
                    {
                        Toast.MakeText(this, "Can't write to Bluetooth.", ToastLength.Long).Show();
                        break;
                        //throw;
                    }
                    GlobalOperationsIdClass.timerActiveTasks++;
                    if (GlobalOperationsIdClass.timerActiveTasks == 1) StartTimer(GlobalOperationsIdClass.BtReadPeriod, readBt);

                    addrFrom = addrTo;

                    var startTime = DateTime.Now;
                    while (GlobalOperationsIdClass.timerActiveTasks > 0 && DateTime.Now.Subtract(startTime).TotalMilliseconds < 2000)
                    {
                        await Task.Delay(1);
                    }
                } while (!GlobalOperationsIdClass.dumpCancellation && addrTo < GlobalOperationsIdClass.FlashSizeLimit);
                dumpFlashButton.Enabled = false;
                dumpFlashButton.Text = tmp;
            };

            quickDumpFlashButton.Click += async (sender, e) =>
            {
                quickDumpFlashButton.Enabled = false;
                string tmp = quickDumpFlashButton.Text;
                quickDumpFlashButton.Text = "Dumping...";

                byte maxFrameBytes = 256 - 7 - ProtocolParser.ReplyDataLength.READ_FLASH - 1;
                long addrFrom = 0;
                long addrTo = 0;
                GlobalOperationsIdClass.dumpCancellation = false;

                quickDumpFlashButton.Enabled = false;
                quickDumpFlashButton.Text = tmp;
            };

            clearGridButton.Click += (sender, e) =>
            {
                GlobalOperationsIdClass.FlashPageState.table.Clear();
                flashGridView.Adapter = new FlashGridAdapter(this, GlobalOperationsIdClass.FlashPageState.table);
            };

            backButton.Click += (sender, e) =>
            {
                Finish();
            };

            readFromEditText.FocusChange += (sender, e) =>
            {
                UInt32.TryParse(readFromEditText.Text, out UInt32 from);
                readFromEditText.Text = from.ToString();
                GlobalOperationsIdClass.FlashPageState.ReadAddress = from;
            };

            lengthEditText.FocusChange += (sender, e) =>
            {
                byte.TryParse(lengthEditText.Text, out byte n);
                lengthEditText.Text = n.ToString();
                GlobalOperationsIdClass.FlashPageState.ReadLength = n;
            };

            writeFromEditText.FocusChange += (sender, e) =>
            {
                UInt32.TryParse(writeFromEditText.Text, out UInt32 from);
                writeFromEditText.Text = from.ToString();
                GlobalOperationsIdClass.FlashPageState.WriteAddress = from;
            };

            dataEditText.FocusChange += (sender, e) =>
            {
                dataEditText.Text = Helpers.CheckHexString(dataEditText.Text);
                byte[] n = Helpers.ConvertHexToByteArray(dataEditText.Text);
                //dataEditText.Text = Helpers.ConvertByteArrayToHex(n, 256 - CommandDataLength.WRITE_FLASH);
                dataEditText.Text = Helpers.ConvertByteArrayToHex(n, 256 - 11);
                GlobalOperationsIdClass.FlashPageState.WriteData = n;
            };
        }

        public void StartTimer(long gap, Func<Task<bool>> callback)
        {
            Handler handler = new Handler(Looper.MainLooper);
            handler.PostDelayed(() =>
            {
                if (GlobalOperationsIdClass.Bt.BtInputBuffer.Count > 0) callback();
                if (GlobalOperationsIdClass.timerActiveTasks > 0) StartTimer(gap, callback);
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
                foreach (var reply in GlobalOperationsIdClass.Parser._repliesList)
                {
                    GlobalOperationsIdClass.timerActiveTasks--;
                    if (reply.ErrorCode == 0)
                    {
                        GlobalOperationsIdClass.StatusPageState.TerminalText.Append(reply.ToString());

                        if (reply.ErrorCode == 0)
                        {
                            if (reply.ReplyCode == ProtocolParser.Reply.READ_FLASH)
                            {
                                GridView flashGridView = FindViewById<GridView>(Resource.Id.flashGridView);

                                ProtocolParser.ReplyData.readFlashReply replyDetails =
                                    new ProtocolParser.ReplyData.readFlashReply(reply);
                                GlobalOperationsIdClass.Flash.Add(replyDetails.Address, replyDetails.Data);
                                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());
                                // refresh flash table
                                int startPage = (int)(replyDetails.Address / GlobalOperationsIdClass.Flash.TeamDumpSize);
                                int endpage = (int)((replyDetails.Address + replyDetails.Data.Length) /
                                                     GlobalOperationsIdClass.Flash.TeamDumpSize);
                                for (int i = startPage; i <= endpage; i++)
                                {
                                    string[] tmp = GlobalOperationsIdClass.Flash.GetTablePage(i);
                                    var row = new FlashTableItem { TeamNum = tmp[0], Decoded = tmp[2] };

                                    bool flag = false;
                                    for (int j = 0; j < table?.Count; j++)
                                    {
                                        if (table[j].TeamNum == row.TeamNum)
                                        {
                                            table.RemoveAt(j);
                                            table.Insert(j, row);
                                            flag = true;
                                            break;
                                        }
                                    }
                                    if (!flag) table.Add(row);
                                }
                                flashGridView.Adapter = new FlashGridAdapter(this, table);
                            }
                            else if (reply.ReplyCode == ProtocolParser.Reply.WRITE_FLASH)
                            {
                                ProtocolParser.ReplyData.writeFlashReply replyDetails =
                                    new ProtocolParser.ReplyData.writeFlashReply(reply);
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
            }

            return packageReceived;
        }
    }

    public class FlashGridAdapter : BaseAdapter<GlobalOperationsIdClass.FlashPageState.FlashTableItem>
    {

        List<GlobalOperationsIdClass.FlashPageState.FlashTableItem> items;
        Activity context;

        public FlashGridAdapter(Activity context, List<GlobalOperationsIdClass.FlashPageState.FlashTableItem> items)
        {
            this.context = context;
            this.items = items;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override GlobalOperationsIdClass.FlashPageState.FlashTableItem this[int position]
        {
            get { return items[position]; }
        }

        public override int Count
        {
            get { return items.Count; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];

            View view = convertView;
            if (view == null) // no view to re-use, create new
            {
                view = context.LayoutInflater.Inflate(Resource.Layout.flashTable_view, null);
            }

            view.FindViewById<TextView>(Resource.Id.PageNumber).Text = item.TeamNum;
            view.FindViewById<TextView>(Resource.Id.DecodedData).Text = item.Decoded;

            return view;
        }

    }
}