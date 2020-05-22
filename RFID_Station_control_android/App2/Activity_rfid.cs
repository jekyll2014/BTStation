using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using static RfidStationControl.GlobalOperationsIdClass.RfidPageState;

namespace RfidStationControl
{
    [Activity(Label = "RFID")]
    public class ActivityRfid : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_rfid);

            // populate all controls
            var initButton = FindViewById<Button>(Resource.Id.initButton);
            var readButton = FindViewById<Button>(Resource.Id.readButton);
            var writeButton = FindViewById<Button>(Resource.Id.writeButton);
            var eraseButton = FindViewById<Button>(Resource.Id.eraseButton);
            var dumpButton = FindViewById<Button>(Resource.Id.dumpButton);
            var clearGridButton = FindViewById<Button>(Resource.Id.clearGridButton);
            var backButton = FindViewById<Button>(Resource.Id.backButton);

            var teamNumberEditText = FindViewById<EditText>(Resource.Id.teamNumberEditText);
            var maskEditText = FindViewById<EditText>(Resource.Id.maskEditText);
            var readFromEditText = FindViewById<EditText>(Resource.Id.readFromEditText);
            var readToEditText = FindViewById<EditText>(Resource.Id.readToEditText);
            var pageNumberEditText = FindViewById<EditText>(Resource.Id.pageNumberEditText);
            var dataEditText = FindViewById<EditText>(Resource.Id.dataEditText);
            var uidEditText = FindViewById<EditText>(Resource.Id.uidEditText);

            var rfidGridView = FindViewById<GridView>(Resource.Id.rfidGridView);

            if (Table.Count == 0)
            {
                var row = new RfidTableItem
                {
                    PageNum = "Page#",
                    Data = "Hex data",
                    PageDesc = "Page description",
                    Decoded = "Decoded data"
                };
                Table.Add(row);
            }

            rfidGridView.Adapter = new RfidGridAdapter(this, Table);

            Title = "Station " + GlobalOperationsIdClass.StationSettings.Number.ToString() + " RFID";

            // just for test
            teamNumberEditText.Text = InitChipNumber.ToString();
            maskEditText.Text = Helpers.ConvertMaskToString(Mask);
            readFromEditText.Text = ReadFrom.ToString();
            readToEditText.Text = ReadTo.ToString();
            pageNumberEditText.Text = WriteFrom.ToString();
            dataEditText.Text = Helpers.ConvertByteArrayToHex(WriteData);
            uidEditText.Text = Helpers.ConvertByteArrayToHex(Uid);

            if (GlobalOperationsIdClass.Bt.IsBtEnabled() && GlobalOperationsIdClass.Bt.IsBtConnected())
            {
                initButton.Enabled = true;
                readButton.Enabled = true;
                writeButton.Enabled = true;
                eraseButton.Enabled = true;
                dumpButton.Enabled = true;
            }
            else
            {
                initButton.Enabled = false;
                readButton.Enabled = false;
                writeButton.Enabled = false;
                eraseButton.Enabled = false;
                dumpButton.Enabled = false;
            }

            GlobalOperationsIdClass.timerActiveTasks = 0;

            initButton.Click += async (sender, e) =>
            {
                teamNumberEditText.ClearFocus();
                maskEditText.ClearFocus();
                var outBuffer = GlobalOperationsIdClass.Parser.InitChip(InitChipNumber, Mask);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
            };

            readButton.Click += async (sender, e) =>
            {
                readFromEditText.ClearFocus();
                readToEditText.ClearFocus();
                var outBuffer = GlobalOperationsIdClass.Parser.ReadCardPage(ReadFrom, ReadTo);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
            };

            writeButton.Click += async (sender, e) =>
            {
                pageNumberEditText.ClearFocus();
                dataEditText.ClearFocus();
                var outBuffer = GlobalOperationsIdClass.Parser.WriteCardPage(Uid, WriteFrom, WriteData);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
            };

            dumpButton.Click += async (sender, e) =>
            {
                /*rowNum++;
                var row = new RfidTableItem { PageNum = rowNum.ToString(), Data = "0x00", PageDesc = "Page #" + rowNum.ToString() + " description", Decoded = "none" };
                table.Add(row);
                rfidGridView.Adapter = new RfidGridAdapter(this, table);*/
                dumpButton.Enabled = false;
                var tmp = dumpButton.Text;
                dumpButton.Text = "Dumping...";

                var chipSize = RfidContainer.ChipTypes.PageSizes[GlobalOperationsIdClass.StationSettings.ChipType];
                const byte maxFramePages = 45;
                var pagesFrom = 0;
                int pagesTo;
                GlobalOperationsIdClass.dumpCancellation = false;
                do
                {
                    pagesTo = pagesFrom + maxFramePages - 1;
                    if (pagesTo >= chipSize)
                        pagesTo = chipSize - 1;
                    dumpButton.Text = "Dumping " + pagesTo.ToString() + "/" + chipSize.ToString();
                    dumpButton.Invalidate();

                    var outBuffer = GlobalOperationsIdClass.Parser.ReadCardPage((byte)pagesFrom, (byte)pagesTo);

                    if (!await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt)) break;

                    pagesFrom = pagesTo + 1;

                    var startTime = DateTime.Now;
                    while (GlobalOperationsIdClass.timerActiveTasks > 0 && DateTime.Now.Subtract(startTime).TotalMilliseconds < 2000) await Task.Delay(1);
                } while (!GlobalOperationsIdClass.dumpCancellation && pagesTo < chipSize - 1);
                dumpButton.Enabled = false;
                dumpButton.Text = tmp;
            };

            clearGridButton.Click += (sender, e) =>
            {
                GlobalOperationsIdClass.Rfid.InitTable();
                rfidGridView.Adapter = new RfidGridAdapter(this, Table);
            };

            backButton.Click += (sender, e) =>
            {
                Finish();
            };

            teamNumberEditText.FocusChange += (sender, e) =>
            {
                ushort.TryParse(teamNumberEditText.Text, out var n);
                teamNumberEditText.Text = n.ToString();
                InitChipNumber = n;
            };

            maskEditText.FocusChange += (sender, e) =>
            {
                if (maskEditText.Text.Length > 16) maskEditText.Text = maskEditText.Text.Substring(0, 16);
                else if (maskEditText.Text.Length < 16)
                    while (maskEditText.Text.Length < 16)
                        maskEditText.Text = "0" + maskEditText.Text;

                var n = Helpers.ConvertStringToMask(maskEditText.Text);
                maskEditText.Text = "";
                for (var i = 15; i >= 0; i--) maskEditText.Text = Helpers.ConvertMaskToString(n);
                Mask = n;
            };

            readFromEditText.FocusChange += (sender, e) =>
            {
                byte.TryParse(readFromEditText.Text, out var n);
                readFromEditText.Text = n.ToString();
                ReadFrom = n;
            };

            readToEditText.FocusChange += (sender, e) =>
            {
                byte.TryParse(readToEditText.Text, out var n);
                readToEditText.Text = n.ToString();
                ReadTo = n;
            };

            pageNumberEditText.FocusChange += (sender, e) =>
            {
                byte.TryParse(pageNumberEditText.Text, out var n);
                pageNumberEditText.Text = n.ToString();
                WriteFrom = n;
            };

            dataEditText.FocusChange += (sender, e) =>
            {
                dataEditText.Text = Helpers.CheckHexString(dataEditText.Text);
                var n = Helpers.ConvertHexToByteArray(dataEditText.Text);
                dataEditText.Text = Helpers.ConvertByteArrayToHex(n, 4);
                WriteData = n;
            };

            uidEditText.FocusChange += (sender, e) =>
            {
                uidEditText.Text = Helpers.CheckHexString(uidEditText.Text);
                var n = Helpers.ConvertHexToByteArray(uidEditText.Text);
                uidEditText.Text = Helpers.ConvertByteArrayToHex(n);
                if (uidEditText.Text.Length > 24)
                    uidEditText.Text = uidEditText.Text.Substring(0, 24);
                else if (uidEditText.Text.Length < 24)
                    while (uidEditText.Text.Length < 24)
                        uidEditText.Text = "00 " + uidEditText.Text;
                Uid = n;
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
                        if (reply.ReplyCode == ProtocolParser.Reply.INIT_CHIP)
                        {
                            var replyDetails =
                                new ProtocolParser.ReplyData.InitChipReply(reply);
                            GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.READ_CARD_PAGE)
                        {
                            var rfidGridView = FindViewById<GridView>(Resource.Id.rfidGridView);

                            var replyDetails =
                                new ProtocolParser.ReplyData.ReadCardPageReply(reply);
                            GlobalOperationsIdClass.Rfid.AddPages(replyDetails.startPage, replyDetails.PagesData);
                            GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());
                            // refresh RFID table
                            var endPage = replyDetails.startPage + (int)(replyDetails.PagesData.Length / 4);
                            for (var i = replyDetails.startPage; i < endPage; i++)
                            {
                                var tmp = GlobalOperationsIdClass.Rfid.GetTablePage((byte)i);
                                var row = new RfidTableItem
                                    { PageNum = tmp[0], Data = tmp[1], PageDesc = tmp[2], Decoded = tmp[3] };

                                var flag = false;
                                for (var j = 0; j < Table?.Count; j++)
                                {
                                    if (Table[j].PageNum != row.PageNum) continue;
                                    Table.RemoveAt(j);
                                    Table.Insert(j, row);
                                    flag = true;
                                    break;
                                }
                                if (!flag) Table?.Add(row);
                                rfidGridView.Adapter = new RfidGridAdapter(this, Table);
                            }
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

    public class RfidGridAdapter : BaseAdapter<RfidTableItem>
    {
        private readonly List<RfidTableItem> _items;
        private readonly Activity _context;

        public RfidGridAdapter(Activity context, List<RfidTableItem> items)
        {
            _context = context;
            _items = items;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override RfidTableItem this[int position] => _items[position];

        public override int Count => _items.Count;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = _items[position];

            var view = convertView;
            if (view == null) // no view to re-use, create new
                view = _context.LayoutInflater.Inflate(Resource.Layout.rfidTable_view, null);

            view.FindViewById<TextView>(Resource.Id.PageNumber).Text = item.PageNum;
            view.FindViewById<TextView>(Resource.Id.PageDescription).Text = item.PageDesc;
            view.FindViewById<TextView>(Resource.Id.HexData).Text = item.Data;
            view.FindViewById<TextView>(Resource.Id.DecodedData).Text = item.Decoded;

            return view;
        }

    }
}