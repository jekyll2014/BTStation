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
            var getLastTeamsButton = FindViewById<Button>(Resource.Id.getLastTeamsButton);
            var getAllTeamsButton = FindViewById<Button>(Resource.Id.getAllTeamsButton);
            var getTeamButton = FindViewById<Button>(Resource.Id.getTeamButton);
            var updateMaskButton = FindViewById<Button>(Resource.Id.updateTeamMaskButton);
            var eraseTeamButton = FindViewById<Button>(Resource.Id.eraseTeamButton);
            var dumpButton = FindViewById<Button>(Resource.Id.dumpTeamsButton);
            var clearGridButton = FindViewById<Button>(Resource.Id.clearGridButton);
            var backButton = FindViewById<Button>(Resource.Id.backButton);

            var scanTeamNumberEditText = FindViewById<EditText>(Resource.Id.scanTeamNumberEditText);
            var teamNumberEditText = FindViewById<EditText>(Resource.Id.teamNumberEditText);
            var issuedEditText = FindViewById<EditText>(Resource.Id.issuedEditText);
            var maskEditText = FindViewById<EditText>(Resource.Id.maskEditText);
            var eraseTeamNumberEditText = FindViewById<EditText>(Resource.Id.eraseTeamNumberEditText);


            var teamsGridView = FindViewById<GridView>(Resource.Id.teamsGridView);

            if (Table.Count == 0)
            {
                var row = new TeamsTableItem
                {
                    TeamNum = "Team#",
                    CheckTime = "Mark time",
                    InitTime = "Init time",
                    Mask = "TeamMask",
                    DumpSize = "Dump size"
                };
                Table.Add(row);
            }

            teamsGridView.Adapter = new TeamsGridAdapter(this, Table);

            //init page
            Title = "Station " + GlobalOperationsIdClass.StationSettings.Number.ToString() + " teams";
            scanTeamNumberEditText.Text = ScanTeamNumber.ToString();
            teamNumberEditText.Text = GetTeamNumber.ToString();
            issuedEditText.Text = Helpers.DateToString(Issued);
            maskEditText.Text = Helpers.ConvertMaskToString(TeamMask);
            eraseTeamNumberEditText.Text = EraseTeam.ToString();

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
                var outBuffer = GlobalOperationsIdClass.Parser.GetLastTeam();
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
            };

            getAllTeamsButton.Click += async (sender, e) =>
            {
                scanTeamNumberEditText.ClearFocus();
                var outBuffer = GlobalOperationsIdClass.Parser.ScanTeams(ScanTeamNumber);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
            };

            getTeamButton.Click += async (sender, e) =>
            {
                teamNumberEditText.ClearFocus();
                var outBuffer = GlobalOperationsIdClass.Parser.GetTeamRecord(GetTeamNumber);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
            };

            updateMaskButton.Click += async (sender, e) =>
            {
                teamNumberEditText.ClearFocus();
                issuedEditText.ClearFocus();
                maskEditText.ClearFocus();
                var outBuffer = GlobalOperationsIdClass.Parser.UpdateTeamMask(GetTeamNumber, Issued, TeamMask);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
            };

            eraseTeamButton.Click += async (sender, e) =>
            {
                eraseTeamNumberEditText.ClearFocus();
                var outBuffer = GlobalOperationsIdClass.Parser.EraseTeamFlash(EraseTeam);
                await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt);
            };

            dumpButton.Click += async (sender, e) =>
            {
                /*rowNum++;
                var row = new TeamsTableItem { TeamNum = rowNum.ToString(), InitTime = "01.01.2019", CheckTime = "02.01.2019", TeamMask = "000000111", DumpSize = "60" };
                table.Add(row);
                teamsGridView.Adapter = new TeamsGridAdapter(this, table);*/
                dumpButton.Enabled = false;
                var tmp = dumpButton.Text;
                dumpButton.Text = "Dumping...";
                ushort teamNum = 1;
                var maxTeamNum = (int)(GlobalOperationsIdClass.StationSettings.FlashSize / GlobalOperationsIdClass.StationSettings.TeamBlockSize);
                GlobalOperationsIdClass.dumpCancellation = false;
                do
                {
                    dumpButton.Text = "Dumping " + teamNum.ToString() + "/" + maxTeamNum.ToString();
                    dumpButton.Invalidate();
                    //0-1: какую запись
                    var outBuffer = GlobalOperationsIdClass.Parser.GetTeamRecord(teamNum);
                    if (!await GlobalOperationsIdClass.SendToBtAsync(outBuffer, this, ReadBt)) break;

                    var startTime = DateTime.Now;
                    while (GlobalOperationsIdClass.timerActiveTasks > 0 && DateTime.Now.Subtract(startTime).TotalMilliseconds < 2000) await Task.Delay(1);

                    teamNum++;
                } while (!GlobalOperationsIdClass.dumpCancellation && teamNum < maxTeamNum);
                dumpButton.Enabled = false;
                dumpButton.Text = tmp;
            };

            clearGridButton.Click += (sender, e) =>
            {
                GlobalOperationsIdClass.Teams.InitTable();
                teamsGridView.Adapter = new TeamsGridAdapter(this, Table);
            };

            scanTeamNumberEditText.FocusChange += (sender, e) =>
            {
                ushort.TryParse(scanTeamNumberEditText.Text, out var n);
                scanTeamNumberEditText.Text = n.ToString();
                ScanTeamNumber = n;
            };

            teamNumberEditText.FocusChange += (sender, e) =>
            {
                ushort.TryParse(teamNumberEditText.Text, out var n);
                teamNumberEditText.Text = n.ToString();
                GetTeamNumber = n;
            };

            issuedEditText.FocusChange += (sender, e) =>
            {
                var t = Helpers.DateStringToUnixTime(issuedEditText.Text);
                issuedEditText.Text = Helpers.DateToString(Helpers.ConvertFromUnixTimestamp(t));
                Issued = Helpers.ConvertFromUnixTimestamp(t);
            };

            maskEditText.FocusChange += (sender, e) =>
            {
                if (maskEditText.Text.Length > 16)
                    maskEditText.Text = maskEditText.Text.Substring(0, 16);
                else if (maskEditText.Text.Length < 16)
                    while (maskEditText.Text.Length < 16)
                        maskEditText.Text = "0" + maskEditText.Text;

                var n = Helpers.ConvertStringToMask(maskEditText.Text);
                maskEditText.Text = "";
                for (var i = 15; i >= 0; i--)
                    maskEditText.Text = Helpers.ConvertMaskToString(n);
                TeamMask = n;
            };

            eraseTeamNumberEditText.FocusChange += (sender, e) =>
            {
                ushort.TryParse(eraseTeamNumberEditText.Text, out var n);
                eraseTeamNumberEditText.Text = n.ToString();
                EraseTeam = n;
            };

            backButton.Click += (sender, e) =>
            {
                Finish();
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
                        if (reply.ReplyCode == ProtocolParser.Reply.GET_LAST_TEAMS)
                        {
                            var replyDetails =
                                new ProtocolParser.ReplyData.GetLastTeamsReply(reply);
                            GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.SCAN_TEAMS)
                        {
                            var replyDetails =
                                new ProtocolParser.ReplyData.ScanTeamsReply(reply);
                            GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());
                        }
                        else if (reply.ReplyCode == ProtocolParser.Reply.GET_TEAM_RECORD)
                        {
                            var teamsGridView = FindViewById<GridView>(Resource.Id.teamsGridView);

                            var replyDetails =
                                new ProtocolParser.ReplyData.GetTeamRecordReply(reply);
                            GlobalOperationsIdClass.StatusPageState.TerminalText.Append(replyDetails.ToString());
                            var team = new TeamsContainer.TeamData
                            {
                                LastCheckTime = replyDetails.LastMarkTime,
                                DumpSize = replyDetails.DumpSize,
                                InitTime = replyDetails.InitTime,
                                TeamMask = replyDetails.Mask,
                                TeamNumber = replyDetails.TeamNumber
                            };
                            GlobalOperationsIdClass.Teams.Add(team);
                            var tmp = GlobalOperationsIdClass.Teams.GetTablePage(replyDetails.TeamNumber);
                            var row = new TeamsTableItem
                            {
                                TeamNum = tmp[0],
                                InitTime = tmp[1],
                                CheckTime = tmp[2],
                                Mask = tmp[3],
                                DumpSize = tmp[4]
                            };

                            var flag = false;
                            for (var i = 0; i < Table?.Count; i++)
                            {
                                if (Table[i].TeamNum != row.TeamNum) continue;

                                Table.RemoveAt(i);
                                Table.Insert(i, row);
                                flag = true;
                                break;
                            }

                            if (!flag) Table?.Add(row);


                            teamsGridView.Adapter = new TeamsGridAdapter(this, Table);
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

    public class TeamsGridAdapter : BaseAdapter<TeamsTableItem>
    {
        private readonly List<TeamsTableItem> _items;
        private readonly Activity _context;

        public TeamsGridAdapter(Activity context, List<TeamsTableItem> items)
        {
            _context = context;
            _items = items;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override TeamsTableItem this[int position] => _items[position];

        public override int Count => _items.Count;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = _items[position];

            var view = convertView;
            if (view == null) // no view to re-use, create new
                view = _context.LayoutInflater.Inflate(Resource.Layout.teamsTable_view, null);

            view.FindViewById<TextView>(Resource.Id.PageNumber).Text = item.TeamNum;
            view.FindViewById<TextView>(Resource.Id.PageDescription).Text = item.InitTime;
            view.FindViewById<TextView>(Resource.Id.HexData).Text = item.CheckTime;
            view.FindViewById<TextView>(Resource.Id.DecodedData).Text = item.Mask;
            view.FindViewById<TextView>(Resource.Id.DumpSize).Text = item.DumpSize;

            return view;
        }

    }
}