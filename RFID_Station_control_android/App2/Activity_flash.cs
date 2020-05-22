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
            var readFlashButton = FindViewById<Button>(Resource.Id.readFlashButton);
            var writeFlashButton = FindViewById<Button>(Resource.Id.writeFlashButton);
            var dumpFlashButton = FindViewById<Button>(Resource.Id.dumpFlashButton);
            var quickDumpFlashButton = FindViewById<Button>(Resource.Id.quickDumpFlashButton);
            var clearGridButton = FindViewById<Button>(Resource.Id.clearGridButton);
            var backButton = FindViewById<Button>(Resource.Id.backButton);

            var readFromEditText = FindViewById<EditText>(Resource.Id.readFromEditText);
            var lengthEditText = FindViewById<EditText>(Resource.Id.lengthEditText);
            var writeFromEditText = FindViewById<EditText>(Resource.Id.writeFromEditText);
            var dataEditText = FindViewById<EditText>(Resource.Id.dataEditText);

            var flashGridView = FindViewById<GridView>(Resource.Id.flashGridView);

            if (Table.Count == 0)
            {
                var row = new FlashTableItem
                {
                    TeamNum = "Team#",
                    Decoded = "Decoded data"
                };
                Table.Add(row);
            }

            flashGridView.Adapter = new FlashGridAdapter(this, Table);

            Title = "Station " + GlobalOperationsIdClass.StationSettings.Number.ToString() + " Flash";
            readFromEditText.Text = ReadAddress.ToString();
            lengthEditText.Text = ReadLength.ToString();
            writeFromEditText.Text = WriteAddress.ToString();
            dataEditText.Text = Helpers.ConvertByteArrayToHex(WriteData);

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
                var outBuffer = GlobalOperationsIdClass.Parser.ReadFlash(ReadAddress, ReadLength);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
            };

            writeFlashButton.Click += async (sender, e) =>
            {
                writeFromEditText.ClearFocus();
                dataEditText.ClearFocus();
                var outBuffer = GlobalOperationsIdClass.Parser.WriteFlash(WriteAddress, WriteData);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
            };

            dumpFlashButton.Click += async (sender, e) =>
            {
                dumpFlashButton.Enabled = false;
                var tmp = dumpFlashButton.Text;
                dumpFlashButton.Text = "Dumping...";

                byte maxFrameBytes = 256 - 7 - ProtocolParser.ReplyDataLength.READ_FLASH - 1;
                uint addrFrom = 0;
                uint addrTo = 0;
                GlobalOperationsIdClass.dumpCancellation = false;
                do
                {
                    addrTo = addrFrom + maxFrameBytes;
                    if (addrTo >= GlobalOperationsIdClass.FlashSizeLimit)
                        addrTo = GlobalOperationsIdClass.FlashSizeLimit;
                    dumpFlashButton.Text = "Dumping " + addrTo.ToString() + "bytes/" + GlobalOperationsIdClass.FlashSizeLimit.ToString() + "Mb";
                    dumpFlashButton.Invalidate();

                    var outBuffer = GlobalOperationsIdClass.Parser.ReadFlash(addrFrom, (byte)(addrTo - addrFrom));

                    if (!await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt)) break;

                    addrFrom = addrTo;

                    var startTime = DateTime.Now;
                    while (GlobalOperationsIdClass.timerActiveTasks > 0 && DateTime.Now.Subtract(startTime).TotalMilliseconds < 2000) await Task.Delay(1);
                } while (!GlobalOperationsIdClass.dumpCancellation && addrTo < GlobalOperationsIdClass.FlashSizeLimit);
                dumpFlashButton.Enabled = true;
                dumpFlashButton.Text = tmp;
            };

            quickDumpFlashButton.Click += async (sender, e) =>
            {
                quickDumpFlashButton.Enabled = false;
                var tmp = quickDumpFlashButton.Text;
                quickDumpFlashButton.Text = "Dumping...";

                byte maxFrameBytes = 256 - 7 - ProtocolParser.ReplyDataLength.READ_FLASH - 1;
                long addrFrom = 0;
                long addrTo = 0;
                GlobalOperationsIdClass.dumpCancellation = false;

                // do the job
                Toast.MakeText(this, "Not implemented yet!!!", ToastLength.Long).Show();

                quickDumpFlashButton.Enabled = true;
                quickDumpFlashButton.Text = tmp;
            };

            clearGridButton.Click += (sender, e) =>
            {
                Table.Clear();
                flashGridView.Adapter = new FlashGridAdapter(this, Table);
            };

            backButton.Click += (sender, e) =>
            {
                Finish();
            };

            readFromEditText.FocusChange += (sender, e) =>
            {
                uint.TryParse(readFromEditText.Text, out var from);
                readFromEditText.Text = from.ToString();
                ReadAddress = from;
            };

            lengthEditText.FocusChange += (sender, e) =>
            {
                byte.TryParse(lengthEditText.Text, out var n);
                lengthEditText.Text = n.ToString();
                ReadLength = n;
            };

            writeFromEditText.FocusChange += (sender, e) =>
            {
                uint.TryParse(writeFromEditText.Text, out var from);
                writeFromEditText.Text = from.ToString();
                WriteAddress = from;
            };

            dataEditText.FocusChange += (sender, e) =>
            {
                dataEditText.Text = Helpers.CheckHexString(dataEditText.Text);
                var n = Helpers.ConvertHexToByteArray(dataEditText.Text);
                //dataEditText.Text = Helpers.ConvertByteArrayToHex(n, 256 - CommandDataLength.WRITE_FLASH);
                dataEditText.Text = Helpers.ConvertByteArrayToHex(n, 256 - 11);
                WriteData = n;
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

            for (var n = 0; n < GlobalOperationsIdClass.Parser._repliesList.Count; n++)
            {
                var reply = GlobalOperationsIdClass.Parser._repliesList[n];
                GlobalOperationsIdClass.timerActiveTasks--;
                if (reply.ErrorCode == 0)
                {
                    GlobalOperationsIdClass.StatusPageState.TerminalText.Append(reply.ToString());

                    if (reply.ErrorCode == 0)
                    {
                        if (reply.ReplyCode == ProtocolParser.Reply.READ_FLASH)
                        {
                            var flashGridView = FindViewById<GridView>(Resource.Id.flashGridView);

                            var replyDetails =
                                new ProtocolParser.ReplyData.ReadFlashReply(reply);
                            GlobalOperationsIdClass.Flash.Add(replyDetails.Address, replyDetails.Data);
                            GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());
                            // refresh flash table
                            var startPage = (int)(replyDetails.Address / GlobalOperationsIdClass.Flash.TeamDumpSize);
                            var endPage = (int)((replyDetails.Address + replyDetails.Data.Length) /
                                                GlobalOperationsIdClass.Flash.TeamDumpSize);
                            for (var i = startPage; i <= endPage; i++)
                            {
                                var tmp = GlobalOperationsIdClass.Flash.GetTablePage(i);
                                var row = new FlashTableItem { TeamNum = tmp[0], Decoded = tmp[2] };

                                var flag = false;
                                for (var j = 0; j < Table?.Count; j++)
                                {
                                    if (Table[j].TeamNum != row.TeamNum) continue;

                                    Table.RemoveAt(j);
                                    Table.Insert(j, row);
                                    flag = true;
                                    break;
                                }
                                if (!flag) Table?.Add(row);
                            }
                            flashGridView.Adapter = new FlashGridAdapter(this, Table);
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.WRITE_FLASH)
                        {
                            var replyDetails =
                                new ProtocolParser.ReplyData.WriteFlashReply(reply);
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

            return packageReceived;
        }
    }

    public class FlashGridAdapter : BaseAdapter<FlashTableItem>
    {
        private readonly List<FlashTableItem> _items;
        private readonly Activity _context;

        public FlashGridAdapter(Activity context, List<FlashTableItem> items)
        {
            _context = context;
            _items = items;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override FlashTableItem this[int position] => _items[position];

        public override int Count => _items.Count;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = _items[position];

            var view = convertView;
            if (view == null) // no view to re-use, create new
                view = _context.LayoutInflater.Inflate(Resource.Layout.flashTable_view, null);

            view.FindViewById<TextView>(Resource.Id.PageNumber).Text = item.TeamNum;
            view.FindViewById<TextView>(Resource.Id.DecodedData).Text = item.Decoded;

            return view;
        }

    }
}