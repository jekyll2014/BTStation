using System;
using System.Collections.Generic;
using System.Data;

namespace RfidStationControl
{
    public class TeamsContainer
    {
        public class TeamData
        {
            public ushort TeamNumber;
            public ushort TeamMask;
            public DateTime InitTime;
            public DateTime LastCheckTime;
            public uint DumpSize;
        }

        private List<TeamData> _dump = new List<TeamData>();

        public TeamData[] Dump => _dump.ToArray();

        public DataTable Table = new DataTable("Teams") { Columns = { "Team#", "Init time", "Check-in time", "Mask", "Dump size" } };

        public void InitTable()
        {
            Table.Rows.Clear();
        }

        public bool Add(TeamData newTeam)
        {
            var flag = false;
            for (var i = 0; i < _dump?.Count; i++)
                if (_dump[i].TeamNumber == newTeam.TeamNumber)
                {
                    _dump.RemoveAt(i);
                    _dump.Insert(i, newTeam);
                    flag = true;
                    break;
                }

            if (!flag) _dump.Add(newTeam);
            ParseToGrid(newTeam.TeamNumber);
            return true;
        }

        public string[] GetTablePage(int team)
        {
            var page = new string[Table.Columns.Count];

            foreach (DataRow row in Table.Rows)
                if (row.ItemArray[0].ToString() == team.ToString())
                    for (var i = 0; i < page.Length; i++) page[i] = row.ItemArray[i].ToString();
            return page;
        }

        public void ParseToGrid(int teamFrom, int teamTo = -1)
        {
            if (teamTo == -1) teamTo = teamFrom;
            while (teamFrom <= teamTo)
            {
                var flag = false;
                var tmpTeam = new TeamData();
                for (var i = 0; i < _dump.Count; i++)
                    if (_dump[i].TeamNumber == teamFrom)
                    {
                        tmpTeam = _dump[i];
                        flag = true;
                    }

                teamFrom++;

                if (!flag) continue;

                flag = false;

                for (var i = 0; i < Table?.Rows.Count; i++)
                    if (Table.Rows[i][0].ToString() == tmpTeam.TeamNumber.ToString())
                    {
                        Table.Rows[i][1] = Helpers.DateToString(tmpTeam.InitTime);
                        Table.Rows[i][2] = Helpers.DateToString(tmpTeam.LastCheckTime);
                        Table.Rows[i][3] = Helpers.ConvertMaskToString(tmpTeam.TeamMask);
                        Table.Rows[i][4] = tmpTeam.DumpSize.ToString();
                        flag = true;
                        break;
                    }

                if (!flag)
                    Table.Rows.Add(
                        tmpTeam.TeamNumber.ToString(),
                        Helpers.DateToString(tmpTeam.InitTime),
                        Helpers.DateToString(tmpTeam.LastCheckTime),
                        Helpers.ConvertMaskToString(tmpTeam.TeamMask),
                        tmpTeam.DumpSize);
            }
        }
    }
}
