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
            Button initButton = FindViewById<Button>(Resource.Id.initButton);
            Button readButton = FindViewById<Button>(Resource.Id.readButton);
            Button writeButton = FindViewById<Button>(Resource.Id.writeButton);
            Button eraseButton = FindViewById<Button>(Resource.Id.eraseButton);
            Button dumpButton = FindViewById<Button>(Resource.Id.dumpButton);
            Button clearGridButton = FindViewById<Button>(Resource.Id.clearGridButton);
            Button backButton = FindViewById<Button>(Resource.Id.backButton);

            EditText teamNumberEditText = FindViewById<EditText>(Resource.Id.teamNumberEditText);
            EditText maskEditText = FindViewById<EditText>(Resource.Id.maskEditText);
            EditText readFromEditText = FindViewById<EditText>(Resource.Id.readFromEditText);
            EditText readToEditText = FindViewById<EditText>(Resource.Id.readToEditText);
            EditText pageNumberEditText = FindViewById<EditText>(Resource.Id.pageNumberEditText);
            EditText dataEditText = FindViewById<EditText>(Resource.Id.dataEditText);
            EditText uidEditText = FindViewById<EditText>(Resource.Id.uidEditText);

            GridView rfidGridView = FindViewById<GridView>(Resource.Id.rfidGridView);

            if (table.Count == 0)
            {
                var row = new RfidTableItem
                {
                    PageNum = "Page#",
                    Data = "Hex data",
                    PageDesc = "Page description",
                    Decoded = "Decoded data"
                };
                table.Add(row);
            }

            rfidGridView.Adapter = new RfidGridAdapter(this, table);

            Title = "Station " + GlobalOperationsIdClass.StationSettings.Number.ToString() + " RFID";

            // just for test
            teamNumberEditText.Text = GlobalOperationsIdClass.RfidPageState.InitChipNumber.ToString();
            maskEditText.Text = Helpers.ConvertMaskToString(GlobalOperationsIdClass.RfidPageState.Mask);
            readFromEditText.Text = GlobalOperationsIdClass.RfidPageState.ReadFrom.ToString();
            readToEditText.Text = GlobalOperationsIdClass.RfidPageState.ReadTo.ToString();
            pageNumberEditText.Text = GlobalOperationsIdClass.RfidPageState.WriteFrom.ToString();
            dataEditText.Text = Helpers.ConvertByteArrayToHex(GlobalOperationsIdClass.RfidPageState.WriteData);
            uidEditText.Text = Helpers.ConvertByteArrayToHex(GlobalOperationsIdClass.RfidPageState.Uid);

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
                byte[] outBuffer = GlobalOperationsIdClass.Parser.initChip(GlobalOperationsIdClass.RfidPageState.InitChipNumber, GlobalOperationsIdClass.RfidPageState.Mask);
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

            readButton.Click += async (sender, e) =>
            {
                readFromEditText.ClearFocus();
                readToEditText.ClearFocus();
                byte[] outBuffer = GlobalOperationsIdClass.Parser.readCardPage(GlobalOperationsIdClass.RfidPageState.ReadFrom, GlobalOperationsIdClass.RfidPageState.ReadTo);
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

            writeButton.Click += async (sender, e) =>
            {
                pageNumberEditText.ClearFocus();
                dataEditText.ClearFocus();
                byte[] outBuffer = GlobalOperationsIdClass.Parser.writeCardPage(GlobalOperationsIdClass.RfidPageState.Uid, GlobalOperationsIdClass.RfidPageState.WriteFrom, GlobalOperationsIdClass.RfidPageState.WriteData);
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

            dumpButton.Click += async (sender, e) =>
            {
                /*rowNum++;
                var row = new RfidTableItem { PageNum = rowNum.ToString(), Data = "0x00", PageDesc = "Page #" + rowNum.ToString() + " description", Decoded = "none" };
                table.Add(row);
                rfidGridView.Adapter = new RfidGridAdapter(this, table);*/
                dumpButton.Enabled = false;
                string tmp = dumpButton.Text;
                dumpButton.Text = "Dumping...";

                byte chipSize = RfidContainer.ChipTypes._sizes[GlobalOperationsIdClass.StationSettings.ChipType];
                byte maxFramePages = 45;
                int pagesFrom = 0;
                int pagesTo;
                GlobalOperationsIdClass.dumpCancellation = false;
                do
                {
                    pagesTo = pagesFrom + maxFramePages - 1;
                    if (pagesTo >= chipSize)
                        pagesTo = chipSize - 1;
                    dumpButton.Text = "Dumping " + pagesTo.ToString() + "/" + chipSize.ToString();
                    dumpButton.Invalidate();

                    byte[] outBuffer = GlobalOperationsIdClass.Parser.readCardPage((byte)pagesFrom, (byte)pagesTo);
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
                    pagesFrom = pagesTo + 1;

                    var startTime = DateTime.Now;
                    while (GlobalOperationsIdClass.timerActiveTasks > 0 && DateTime.Now.Subtract(startTime).TotalMilliseconds < 2000)
                    {
                        await Task.Delay(1);
                    }
                } while (!GlobalOperationsIdClass.dumpCancellation && pagesTo < chipSize - 1);
                dumpButton.Enabled = false;
                dumpButton.Text = tmp;
            };

            clearGridButton.Click += (sender, e) =>
            {
                GlobalOperationsIdClass.Rfid.InitTable();
                rfidGridView.Adapter = new RfidGridAdapter(this, table);
            };

            backButton.Click += (sender, e) =>
            {
                Finish();
            };

            teamNumberEditText.FocusChange += (sender, e) =>
            {
                UInt16.TryParse(teamNumberEditText.Text, out UInt16 n);
                teamNumberEditText.Text = n.ToString();
                GlobalOperationsIdClass.RfidPageState.InitChipNumber = n;
            };

            maskEditText.FocusChange += (sender, e) =>
            {
                if (maskEditText.Text.Length > 16) maskEditText.Text = maskEditText.Text.Substring(0, 16);
                else if (maskEditText.Text.Length < 16)
                {
                    while (maskEditText.Text.Length < 16)
                        maskEditText.Text = "0" + maskEditText.Text;
                }

                UInt16 n = Helpers.ConvertStringToMask(maskEditText.Text);
                maskEditText.Text = "";
                for (int i = 15; i >= 0; i--) maskEditText.Text = Helpers.ConvertMaskToString(n);
                GlobalOperationsIdClass.RfidPageState.Mask = n;
            };

            readFromEditText.FocusChange += (sender, e) =>
            {
                byte.TryParse(readFromEditText.Text, out byte n);
                readFromEditText.Text = n.ToString();
                GlobalOperationsIdClass.RfidPageState.ReadFrom = n;
            };

            readToEditText.FocusChange += (sender, e) =>
            {
                byte.TryParse(readToEditText.Text, out byte n);
                readToEditText.Text = n.ToString();
                GlobalOperationsIdClass.RfidPageState.ReadTo = n;
            };

            pageNumberEditText.FocusChange += (sender, e) =>
            {
                byte.TryParse(pageNumberEditText.Text, out byte n);
                pageNumberEditText.Text = n.ToString();
                GlobalOperationsIdClass.RfidPageState.WriteFrom = n;
            };

            dataEditText.FocusChange += (sender, e) =>
            {
                dataEditText.Text = Helpers.CheckHexString(dataEditText.Text);
                byte[] n = Helpers.ConvertHexToByteArray(dataEditText.Text);
                dataEditText.Text = Helpers.ConvertByteArrayToHex(n, 4);
                GlobalOperationsIdClass.RfidPageState.WriteData = n;
            };

            uidEditText.FocusChange += (sender, e) =>
            {
                uidEditText.Text = Helpers.CheckHexString(uidEditText.Text);
                byte[] n = Helpers.ConvertHexToByteArray(uidEditText.Text);
                uidEditText.Text = Helpers.ConvertByteArrayToHex(n);
                if (uidEditText.Text.Length > 24)
                    uidEditText.Text = uidEditText.Text.Substring(0, 24);
                else if (uidEditText.Text.Length < 24)
                {
                    while (uidEditText.Text.Length < 24)
                        uidEditText.Text = "00 " + uidEditText.Text;
                }
                GlobalOperationsIdClass.RfidPageState.Uid = n;
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
                            if (reply.ReplyCode == ProtocolParser.Reply.INIT_CHIP)
                            {
                                ProtocolParser.ReplyData.initChipReply replyDetails =
                                    new ProtocolParser.ReplyData.initChipReply(reply);
                                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());
                            }
                            else if (reply.ReplyCode == ProtocolParser.Reply.READ_CARD_PAGE)
                            {
                                GridView rfidGridView = FindViewById<GridView>(Resource.Id.rfidGridView);

                                ProtocolParser.ReplyData.readCardPageReply replyDetails =
                                    new ProtocolParser.ReplyData.readCardPageReply(reply);
                                GlobalOperationsIdClass.Rfid.AddPages(replyDetails.startPage, replyDetails.PagesData);
                                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());
                                // refresh RFID table
                                int endpage = replyDetails.startPage + (int)(replyDetails.PagesData.Length / 4);
                                for (int i = replyDetails.startPage; i < endpage; i++)
                                {
                                    string[] tmp = GlobalOperationsIdClass.Rfid.GetTablePage((byte)i);
                                    var row = new RfidTableItem
                                    { PageNum = tmp[0], Data = tmp[1], PageDesc = tmp[2], Decoded = tmp[3] };

                                    bool flag = false;
                                    for (int j = 0; j < table?.Count; j++)
                                    {
                                        if (table[j].PageNum == row.PageNum)
                                        {
                                            table.RemoveAt(j);
                                            table.Insert(j, row);
                                            flag = true;
                                            break;
                                        }
                                    }
                                    if (!flag) table.Add(row);
                                    rfidGridView.Adapter = new RfidGridAdapter(this, table);
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
            }

            return packageReceived;
        }
    }

    public class RfidGridAdapter : BaseAdapter<RfidTableItem>
    {

        List<RfidTableItem> items;
        Activity context;

        public RfidGridAdapter(Activity context, List<RfidTableItem> items)
        {
            this.context = context;
            this.items = items;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override RfidTableItem this[int position]
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
                view = context.LayoutInflater.Inflate(Resource.Layout.rfidTable_view, null);
            }

            view.FindViewById<TextView>(Resource.Id.PageNumber).Text = item.PageNum;
            view.FindViewById<TextView>(Resource.Id.PageDescription).Text = item.PageDesc;
            view.FindViewById<TextView>(Resource.Id.HexData).Text = item.Data;
            view.FindViewById<TextView>(Resource.Id.DecodedData).Text = item.Decoded;

            return view;
        }

    }
}