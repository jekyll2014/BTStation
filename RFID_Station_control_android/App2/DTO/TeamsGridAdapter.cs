using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

namespace RfidStationControl
{
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

            View view = convertView;
            if (view == null) // no view to re-use, create new
                view = _context.LayoutInflater.Inflate(Resource.Layout.teamsTable_view, null);

            view.FindViewById<TextView>(Resource.Id.TeamNumber).Text = item.TeamNum;
            view.FindViewById<TextView>(Resource.Id.TeamMask).Text = item.Mask;
            view.FindViewById<TextView>(Resource.Id.InitTime).Text = item.InitTime;
            view.FindViewById<TextView>(Resource.Id.LastCheckTime).Text = item.CheckTime;

            return view;
        }
    }
}