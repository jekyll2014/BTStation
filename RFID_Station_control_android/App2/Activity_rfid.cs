using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using static RfidStationControl.RfidPageState;

namespace RfidStationControl
{
    [Activity(Label = "RFID")]
    public class ActivityRfid : AppCompatActivity
    {
        #region UI controls

        private Button initButton;
        private Button readButton;
        private Button writeButton;
        private Button eraseButton;
        private Button dumpButton;
        private Button clearGridButton;
        private Button backButton;

        private EditText teamNumberEditText;
        private EditText maskEditText;
        private EditText readFromEditText;
        private EditText readToEditText;
        private EditText pageNumberEditText;
        private EditText dataEditText;
        private EditText uidEditText;

        private GridView rfidGridView;

        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_rfid);

            // populate all controls
            initButton = FindViewById<Button>(Resource.Id.initButton);
            readButton = FindViewById<Button>(Resource.Id.readButton);
            writeButton = FindViewById<Button>(Resource.Id.writeButton);
            eraseButton = FindViewById<Button>(Resource.Id.eraseButton);
            dumpButton = FindViewById<Button>(Resource.Id.dumpButton);
            clearGridButton = FindViewById<Button>(Resource.Id.clearGridButton);
            backButton = FindViewById<Button>(Resource.Id.backButton);

            teamNumberEditText = FindViewById<EditText>(Resource.Id.teamNumberEditText);
            maskEditText = FindViewById<EditText>(Resource.Id.maskEditText);
            readFromEditText = FindViewById<EditText>(Resource.Id.readFromEditText);
            readToEditText = FindViewById<EditText>(Resource.Id.readToEditText);
            pageNumberEditText = FindViewById<EditText>(Resource.Id.pageNumberEditText);
            dataEditText = FindViewById<EditText>(Resource.Id.dataEditText);
            uidEditText = FindViewById<EditText>(Resource.Id.uidEditText);

            rfidGridView = FindViewById<GridView>(Resource.Id.rfidGridView);

            if (Table.Count == 0)
            {
                var row = new RfidTableItem
                {
                    PageNum = "#",
                    PageDesc = "Page description",
                    Decoded = "Decoded data"
                };
                Table.Add(row);
            }

            if (rfidGridView != null)
            {
                rfidGridView.Adapter = new RfidGridAdapter(this, Table);

                Title = "Station " + StationSettings.Number + " RFID";

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

                GlobalOperationsIdClass.TimerActiveTasks = 0;

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
                    const int rowNum = 0;
                    var row = new RfidTableItem
                    {
                        PageNum = rowNum.ToString(),
                        Data = "0x00",
                        PageDesc = "Page #" + rowNum + " description",
                        Decoded = "none"
                    };
                    Table.Add(row);
                    rfidGridView.Adapter = new RfidGridAdapter(this, Table);
                    dumpButton.Enabled = false;
                    var tmp = dumpButton.Text;
                    dumpButton.Text = "Dumping...";

                    var chipSize = RfidContainer.ChipTypes.PageSizes[StationSettings.ChipType];
                    const byte maxFramePages = 45;
                    var pagesFrom = 0;
                    int pagesTo;
                    GlobalOperationsIdClass.DumpCancellation = false;
                    do
                    {
                        pagesTo = pagesFrom + maxFramePages - 1;
                        if (pagesTo >= chipSize)
                            pagesTo = chipSize - 1;
                        dumpButton.Text = "Dumping " + pagesTo + "/" + chipSize;
                        dumpButton.Invalidate();

                        var outBuffer = GlobalOperationsIdClass.Parser.ReadCardPage((byte)pagesFrom, (byte)pagesTo);

                        if (!await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt)) break;

                        pagesFrom = pagesTo + 1;

                        var startTime = DateTime.Now;
                        while (GlobalOperationsIdClass.TimerActiveTasks > 0 &&
                               DateTime.Now.Subtract(startTime).TotalMilliseconds < 2000) await Task.Delay(1);
                    } while (!GlobalOperationsIdClass.DumpCancellation && pagesTo < chipSize - 1);

                    dumpButton.Enabled = false;
                    dumpButton.Text = tmp;
                };

                clearGridButton.Click += (sender, e) =>
                {
                    GlobalOperationsIdClass.Rfid.InitTable();
                    rfidGridView.Adapter = new RfidGridAdapter(this, Table);
                };
            }

            backButton.Click += (sender, e) =>
            {
                Finish();
            };

            teamNumberEditText.FocusChange += (sender, e) =>
            {
                if (!teamNumberEditText.HasFocus)
                {
                    ushort.TryParse(teamNumberEditText.Text, out var n);
                    teamNumberEditText.Text = n.ToString();
                    InitChipNumber = n;
                }
            };

            maskEditText.FocusChange += (sender, e) =>
            {
                if (!maskEditText.HasFocus)
                {
                    if (maskEditText.Text?.Length > 16) maskEditText.Text = maskEditText.Text.Substring(0, 16);
                    else if (maskEditText.Text?.Length < 16)
                        while (maskEditText.Text.Length < 16)
                            maskEditText.Text = "0" + maskEditText.Text;

                    var n = Helpers.ConvertStringToMask(maskEditText.Text);
                    maskEditText.Text = "";
                    for (var i = 15; i >= 0; i--) maskEditText.Text = Helpers.ConvertMaskToString(n);
                    Mask = n;
                }
            };

            readFromEditText.FocusChange += (sender, e) =>
            {
                if (!readFromEditText.HasFocus)
                {
                    byte.TryParse(readFromEditText.Text, out var n);
                    readFromEditText.Text = n.ToString();
                    ReadFrom = n;
                }
            };

            readToEditText.FocusChange += (sender, e) =>
            {
                if (!readToEditText.HasFocus)
                {
                    byte.TryParse(readToEditText.Text, out var n);
                    readToEditText.Text = n.ToString();
                    ReadTo = n;
                }
            };

            pageNumberEditText.FocusChange += (sender, e) =>
            {
                if (!pageNumberEditText.HasFocus)
                {
                    byte.TryParse(pageNumberEditText.Text, out var n);
                    pageNumberEditText.Text = n.ToString();
                    WriteFrom = n;
                }
            };

            dataEditText.FocusChange += (sender, e) =>
            {
                if (!dataEditText.HasFocus)
                {
                    dataEditText.Text = Helpers.CheckHexString(dataEditText.Text);
                    var n = Helpers.ConvertHexToByteArray(dataEditText.Text);
                    dataEditText.Text = Helpers.ConvertByteArrayToHex(n, 4);
                    WriteData = n;
                }
            };

            uidEditText.FocusChange += (sender, e) =>
            {
                if (!uidEditText.HasFocus)
                {
                    uidEditText.Text = Helpers.CheckHexString(uidEditText.Text);
                    var n = Helpers.ConvertHexToByteArray(uidEditText.Text);
                    uidEditText.Text = Helpers.ConvertByteArrayToHex(n);
                    if (uidEditText.Text?.Length > 24)
                        uidEditText.Text = uidEditText.Text.Substring(0, 24);
                    else if (uidEditText.Text?.Length < 24)
                        while (uidEditText.Text.Length < 24)
                            uidEditText.Text = "00 " + uidEditText.Text;
                    Uid = n;
                }
            };
        }

        protected override void OnResume()
        {
            base.OnResume();
            rfidGridView.Adapter = new RfidGridAdapter(this, Table);
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
                Interlocked.Decrement(ref GlobalOperationsIdClass.TimerActiveTasks);
                if (reply.ReplyCode != 0)
                {
                    StatusPageState.TerminalText.Append(reply);

                    if (reply.ErrorCode == 0)
                    {
                        switch (reply.ReplyCode)
                        {
                            case ProtocolParser.Reply.INIT_CHIP:
                                {
                                    var replyDetails = new ProtocolParser.ReplyData.InitChipReply(reply);
                                    StatusPageState.TerminalText.Append(replyDetails);
                                    break;
                                }
                            case ProtocolParser.Reply.READ_CARD_PAGE:
                                {
                                    var replyDetails = new ProtocolParser.ReplyData.ReadCardPageReply(reply);
                                    StatusPageState.TerminalText.Append(replyDetails);

                                    GlobalOperationsIdClass.Rfid.AddPages(replyDetails.startPage, replyDetails.PagesData);
                                    // refresh RFID table
                                    var endPage = replyDetails.startPage + replyDetails.PagesData.Length / 4;
                                    for (var i = replyDetails.startPage; i < endPage; i++)
                                    {
                                        var tmp = GlobalOperationsIdClass.Rfid.GetTablePage(i);
                                        var row = new RfidTableItem
                                        { PageNum = tmp[0], PageDesc = tmp[1], Decoded = tmp[3] };

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

                                    break;
                                }
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
                if (_context.LayoutInflater != null)
                    view = _context.LayoutInflater.Inflate(Resource.Layout.rfidTable_view, null);

            view.FindViewById<TextView>(Resource.Id.PageNumber).Text = item.PageNum;
            view.FindViewById<TextView>(Resource.Id.PageDescription).Text = item.PageDesc;
            view.FindViewById<TextView>(Resource.Id.DecodedData).Text = item.Decoded;

            return view;
        }

    }
}