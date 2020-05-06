using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using static RfidStationControl.GlobalOperationsIdClass.TeamsPageState;

namespace RfidStationControl
{
    [Activity(Label = "Teams")]
    public class ActivityTeams : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_teams);

            // populate all controls
            Button getLastTeamsButton = FindViewById<Button>(Resource.Id.getLastTeamsButton);
            Button getAllTeamsButton = FindViewById<Button>(Resource.Id.getAllTeamsButton);
            Button getTeamButton = FindViewById<Button>(Resource.Id.getTeamButton);
            Button updateMaskButton = FindViewById<Button>(Resource.Id.updateMaskButton);
            Button eraseTeamButton = FindViewById<Button>(Resource.Id.eraseTeamButton);
            Button dumpButton = FindViewById<Button>(Resource.Id.dumpTeamsButton);
            Button clearGridButton = FindViewById<Button>(Resource.Id.clearGridButton);
            Button backButton = FindViewById<Button>(Resource.Id.backButton);

            EditText scanTeamNumberEditText = FindViewById<EditText>(Resource.Id.scanTeamNumberEditText);
            EditText teamNumberEditText = FindViewById<EditText>(Resource.Id.teamNumberEditText);
            EditText issuedEditText = FindViewById<EditText>(Resource.Id.issuedEditText);
            EditText maskEditText = FindViewById<EditText>(Resource.Id.maskEditText);
            EditText eraseTeamNumberEditText = FindViewById<EditText>(Resource.Id.eraseTeamNumberEditText);


            GridView teamsGridView = FindViewById<GridView>(Resource.Id.teamsGridView);

            if (table.Count == 0)
            {
                var row = new TeamsTableItem
                {
                    TeamNum = "Team#",
                    CheckTime = "Mark time",
                    InitTime = "Init time",
                    Mask = "Mask",
                    DumpSize = "Dump size"
                };
                table.Add(row);
            }

            teamsGridView.Adapter = new TeamsGridAdapter(this, table);

            //init page
            Title = "Station " + GlobalOperationsIdClass.StationSettings.Number.ToString() + " teams";
            scanTeamNumberEditText.Text = GlobalOperationsIdClass.TeamsPageState.ScanTeamNumber.ToString();
            teamNumberEditText.Text = GlobalOperationsIdClass.TeamsPageState.GetTeamNumber.ToString();
            issuedEditText.Text = Helpers.DateToString(GlobalOperationsIdClass.TeamsPageState.Issued);
            maskEditText.Text = Helpers.ConvertMaskToString(GlobalOperationsIdClass.TeamsPageState.Mask);
            eraseTeamNumberEditText.Text = GlobalOperationsIdClass.TeamsPageState.EraseTeam.ToString();

            if (GlobalOperationsIdClass.Bt.IsBtEnabled() && GlobalOperationsIdClass.Bt.IsBtConnected())
            {
                getLastTeamsButton.Enabled = true;
                getAllTeamsButton.Enabled = true;
                getTeamButton.Enabled = true;
                updateMaskButton.Enabled = true;
                eraseTeamButton.Enabled = true;
                dumpButton.Enabled = true;
            }
            else
            {
                getLastTeamsButton.Enabled = false;
                getAllTeamsButton.Enabled = false;
                getTeamButton.Enabled = false;
                updateMaskButton.Enabled = false;
                eraseTeamButton.Enabled = false;
                dumpButton.Enabled = false;
            }

            GlobalOperationsIdClass.timerActiveTasks = 0;

            getLastTeamsButton.Click += async (sender, e) =>
            {
                byte[] outBuffer = GlobalOperationsIdClass.Parser.getLastTeam();
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

            getAllTeamsButton.Click += async (sender, e) =>
            {
                scanTeamNumberEditText.ClearFocus();
                byte[] outBuffer = GlobalOperationsIdClass.Parser.scanTeams(GlobalOperationsIdClass.TeamsPageState.ScanTeamNumber);
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

            getTeamButton.Click += async (sender, e) =>
            {
                teamNumberEditText.ClearFocus();
                byte[] outBuffer = GlobalOperationsIdClass.Parser.getTeamRecord(GlobalOperationsIdClass.TeamsPageState.GetTeamNumber);
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

            updateMaskButton.Click += async (sender, e) =>
            {
                teamNumberEditText.ClearFocus();
                issuedEditText.ClearFocus();
                maskEditText.ClearFocus();
                byte[] outBuffer = GlobalOperationsIdClass.Parser.updateTeamMask(GlobalOperationsIdClass.TeamsPageState.GetTeamNumber, GlobalOperationsIdClass.TeamsPageState.Issued, GlobalOperationsIdClass.TeamsPageState.Mask);
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

            eraseTeamButton.Click += async (sender, e) =>
            {
                eraseTeamNumberEditText.ClearFocus();
                byte[] outBuffer = GlobalOperationsIdClass.Parser.eraseTeamFlash(GlobalOperationsIdClass.TeamsPageState.EraseTeam);
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
                var row = new TeamsTableItem { TeamNum = rowNum.ToString(), InitTime = "01.01.2019", CheckTime = "02.01.2019", Mask = "000000111", DumpSize = "60" };
                table.Add(row);
                teamsGridView.Adapter = new TeamsGridAdapter(this, table);*/
                dumpButton.Enabled = false;
                string tmp = dumpButton.Text;
                dumpButton.Text = "Dumping...";
                UInt16 teamNum = 1;
                int maxTeamNum = (int)(GlobalOperationsIdClass.StationSettings.FlashSize / GlobalOperationsIdClass.StationSettings.TeamBlockSize);
                GlobalOperationsIdClass.dumpCancellation = false;
                do
                {
                    dumpButton.Text = "Dumping " + teamNum.ToString() + "/" + maxTeamNum.ToString();
                    dumpButton.Invalidate();
                    //0-1: какую запись
                    byte[] outBuffer = GlobalOperationsIdClass.Parser.getTeamRecord(teamNum);
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

                    var startTime = DateTime.Now;
                    while (GlobalOperationsIdClass.timerActiveTasks > 0 && DateTime.Now.Subtract(startTime).TotalMilliseconds < 2000)
                    {
                        await Task.Delay(1);
                    }

                    teamNum++;
                } while (!GlobalOperationsIdClass.dumpCancellation && teamNum < maxTeamNum);
                dumpButton.Enabled = false;
                dumpButton.Text = tmp;
            };

            clearGridButton.Click += (sender, e) =>
            {
                GlobalOperationsIdClass.Teams.InitTable();
                teamsGridView.Adapter = new TeamsGridAdapter(this, table);
            };

            scanTeamNumberEditText.FocusChange += (sender, e) =>
            {
                UInt16.TryParse(scanTeamNumberEditText.Text, out UInt16 n);
                scanTeamNumberEditText.Text = n.ToString();
                GlobalOperationsIdClass.TeamsPageState.ScanTeamNumber = n;
            };

            teamNumberEditText.FocusChange += (sender, e) =>
            {
                UInt16.TryParse(teamNumberEditText.Text, out UInt16 n);
                teamNumberEditText.Text = n.ToString();
                GlobalOperationsIdClass.TeamsPageState.GetTeamNumber = n;
            };

            issuedEditText.FocusChange += (sender, e) =>
            {
                long t = Helpers.DateStringToUnixTime(issuedEditText.Text);
                issuedEditText.Text = Helpers.DateToString(Helpers.ConvertFromUnixTimestamp(t));
                GlobalOperationsIdClass.TeamsPageState.Issued = Helpers.ConvertFromUnixTimestamp(t);
            };

            maskEditText.FocusChange += (sender, e) =>
            {
                if (maskEditText.Text.Length > 16)
                    maskEditText.Text = maskEditText.Text.Substring(0, 16);
                else if (maskEditText.Text.Length < 16)
                {
                    while (maskEditText.Text.Length < 16)
                        maskEditText.Text = "0" + maskEditText.Text;
                }

                UInt16 n = Helpers.ConvertStringToMask(maskEditText.Text);
                maskEditText.Text = "";
                for (int i = 15; i >= 0; i--)
                    maskEditText.Text = Helpers.ConvertMaskToString(n);
                GlobalOperationsIdClass.TeamsPageState.Mask = n;
            };

            eraseTeamNumberEditText.FocusChange += (sender, e) =>
            {
                UInt16.TryParse(eraseTeamNumberEditText.Text, out UInt16 n);
                eraseTeamNumberEditText.Text = n.ToString();
                GlobalOperationsIdClass.TeamsPageState.EraseTeam = n;
            };

            backButton.Click += (sender, e) =>
            {
                Finish();
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
                            if (reply.ReplyCode == ProtocolParser.Reply.GET_LAST_TEAMS)
                            {
                                ProtocolParser.ReplyData.getLastTeamsReply replyDetails =
                                    new ProtocolParser.ReplyData.getLastTeamsReply(reply);
                                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());
                            }
                            else if (reply.ReplyCode == ProtocolParser.Reply.SCAN_TEAMS)
                            {
                                ProtocolParser.ReplyData.scanTeamsReply replyDetails =
                                    new ProtocolParser.ReplyData.scanTeamsReply(reply);
                                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());
                            }
                            else if (reply.ReplyCode == ProtocolParser.Reply.GET_TEAM_RECORD)
                            {
                                GridView teamsGridView = FindViewById<GridView>(Resource.Id.teamsGridView);

                                ProtocolParser.ReplyData.getTeamRecordReply replyDetails =
                                    new ProtocolParser.ReplyData.getTeamRecordReply(reply);
                                GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());
                                TeamsContainer.TeamData team = new TeamsContainer.TeamData
                                {
                                    LastCheckTime = replyDetails.LastMarkTime,
                                    DumpSize = replyDetails.DumpSize,
                                    InitTime = replyDetails.InitTime,
                                    TeamMask = replyDetails.Mask,
                                    TeamNumber = replyDetails.TeamNumber
                                };
                                GlobalOperationsIdClass.Teams.Add(team);
                                string[] tmp = GlobalOperationsIdClass.Teams.GetTablePage(replyDetails.TeamNumber);
                                var row = new TeamsTableItem
                                {
                                    TeamNum = tmp[0],
                                    InitTime = tmp[1],
                                    CheckTime = tmp[2],
                                    Mask = tmp[3],
                                    DumpSize = tmp[4]
                                };

                                bool flag = false;
                                for (int i = 0; i < table?.Count; i++)
                                {
                                    if (table[i].TeamNum == row.TeamNum)
                                    {
                                        table.RemoveAt(i);
                                        table.Insert(i, row);
                                        flag = true;
                                        break;
                                    }
                                }

                                if (!flag) table.Add(row);


                                teamsGridView.Adapter = new TeamsGridAdapter(this, table);
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

    public class TeamsGridAdapter : BaseAdapter<TeamsTableItem>
    {

        List<TeamsTableItem> items;
        Activity context;

        public TeamsGridAdapter(Activity context, List<TeamsTableItem> items)
        {
            this.context = context;
            this.items = items;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override TeamsTableItem this[int position]
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
                view = context.LayoutInflater.Inflate(Resource.Layout.teamsTable_view, null);
            }

            view.FindViewById<TextView>(Resource.Id.PageNumber).Text = item.TeamNum;
            view.FindViewById<TextView>(Resource.Id.PageDescription).Text = item.InitTime;
            view.FindViewById<TextView>(Resource.Id.HexData).Text = item.CheckTime;
            view.FindViewById<TextView>(Resource.Id.DecodedData).Text = item.Mask;
            view.FindViewById<TextView>(Resource.Id.DumpSize).Text = item.DumpSize;

            return view;
        }

    }
}