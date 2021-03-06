﻿using System;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Toggl.Phoebe;
using Toggl.Phoebe.Data.DataObjects;
using Toggl.Phoebe.Data.Utils;

namespace Toggl.Joey.UI.Adapters
{
    public class GroupedEditAdapter : RecyclerView.Adapter
    {
        private readonly TimeEntryGroup entryGroup;
        public Action<TimeEntryData> HandleTapTimeEntry { get; set; }

        public GroupedEditAdapter (TimeEntryGroup entryGroup)
        {
            this.entryGroup = entryGroup;
        }

        // Provide a reference to the type of views that you are using (custom ViewHolder)
        public class ViewHolder : RecyclerView.ViewHolder
        {
            private View color;
            private TextView period;
            private TextView duration;

            public View ColorView
            {
                get { return color; }
            }

            public TextView PeriodTextView
            {
                get { return period; }
            }

            public TextView DurationTextView
            {
                get { return duration; }
            }

            public ViewHolder (View v, Action<int> listener) : base (v)
            {
                color = v.FindViewById (Resource.Id.GroupedEditTimeEntryItemTimeColorView);
                period = (TextView)v.FindViewById (Resource.Id.GroupedEditTimeEntryItemTimePeriodTextView);
                duration = (TextView)v.FindViewById (Resource.Id.GroupedEditTimeEntryItemDurationTextView);
                v.Click += (sender, e) => listener (LayoutPosition);
            }
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder (ViewGroup parent, int viewType)
        {
            View v = LayoutInflater.From (parent.Context).Inflate (Resource.Layout.EditGroupedTimeEntryItem, parent, false);
            var vh = new ViewHolder (v, OnClick);
            return vh;
        }

        void OnClick (int position)
        {
            var entry = entryGroup.TimeEntryList [position];
            if (HandleTapTimeEntry != null && entry != null) {
                HandleTapTimeEntry (entry);
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public async override void OnBindViewHolder (RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as ViewHolder;
            var entry = entryGroup.TimeEntryList [position];

            if (entryGroup.Project != null) {
                await entryGroup.Project.LoadAsync ();
                var color = Color.ParseColor (entryGroup.Project.GetHexColor ());
                vh.ColorView.SetBackgroundColor (color);
            } else {
                vh.ColorView.SetBackgroundColor (Color.Transparent);
            }

            var stopTime = (entry.StopTime != null) ? " – " + entry.StopTime.Value.ToLocalTime().ToShortTimeString () : "";
            vh.PeriodTextView.Text = entry.StartTime.ToLocalTime().ToShortTimeString () + stopTime;
            vh.DurationTextView.Text = GetDuration (entry, Time.UtcNow).ToString (@"hh\:mm\:ss");
        }

        // Return the size of your dataset (invoked by the layout manager)
        public override int ItemCount
        {
            get { return  entryGroup.TimeEntryList.Count; }
        }

        private static TimeSpan GetDuration (TimeEntryData data, DateTime now)
        {
            if (data.StartTime == DateTime.MinValue) {
                return TimeSpan.Zero;
            }

            var duration = (data.StopTime ?? now) - data.StartTime;
            if (duration < TimeSpan.Zero) {
                duration = TimeSpan.Zero;
            }
            return duration;
        }


    }
}

