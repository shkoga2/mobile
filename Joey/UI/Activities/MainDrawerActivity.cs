using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Widget;
using Android.Text.Format;
using Android.Views;
using Android.Widget;
using Toggl.Joey.UI.Adapters;
using Toggl.Joey.UI.Components;
using Toggl.Joey.UI.Fragments;
using Toggl.Joey.UI.Views;
using Toggl.Phoebe;
using Toggl.Phoebe.Data;
using Toggl.Phoebe.Net;
using XPlatUtils;
using ActionBarDrawerToggle = Android.Support.V7.App.ActionBarDrawerToggle;
using Fragment = Android.Support.V4.App.Fragment;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Toggl.Joey.UI.Activities
{
    [Activity (
         ScreenOrientation = ScreenOrientation.Portrait,
         Name = "toggl.joey.ui.activities.MainDrawerActivity",
         Label = "@string/EntryName",
         Exported = true,
         #if DEBUG
         // The actual entry-point is defined in manifest via activity-alias, this here is just to
         // make adb launch the activity automatically when developing.
         #endif
         Theme = "@style/Theme.Toggl.App")]
    public class MainDrawerActivity : BaseActivity
    {
        private const string PageStackExtra = "com.toggl.timer.page_stack";
        private const string LastSyncArgument = "com.toggl.timer.last_sync";
        private const string LastSyncResultArgument = "com.toggl.timer.last_sync_result";
        private readonly TimerComponent barTimer = new TimerComponent ();
        private readonly Lazy<LogTimeEntriesListFragment> trackingFragment = new Lazy<LogTimeEntriesListFragment> ();
        private readonly Lazy<SettingsListFragment> settingsFragment = new Lazy<SettingsListFragment> ();
        private readonly Lazy<ReportsPagerFragment> reportFragment = new Lazy<ReportsPagerFragment> ();
        private readonly Lazy<FeedbackFragment> feedbackFragment = new Lazy<FeedbackFragment> ();
        private readonly List<int> pageStack = new List<int> ();
        private readonly Handler handler = new Handler ();
        private DrawerListAdapter drawerAdapter;
        private ImageButton syncRetryButton;
        private TextView syncStatusText;
        private long lastSyncInMillis;
        private int syncStatus;
        private Subscription<SyncStartedMessage> drawerSyncStarted;
        private Subscription<SyncFinishedMessage> drawerSyncFinished;
        private static readonly int syncing = 0;
        private static readonly int syncSuccessful = 1;
        private static readonly int syncHadErrors = 2;
        private static readonly int syncFatalError = 3;

        private ListView DrawerListView { get; set; }

        private TextView DrawerUserName { get; set; }

        private ProfileImageView DrawerImage { get; set; }

        private View DrawerUserView { get; set; }

        private DrawerLayout DrawerLayout { get; set; }

        protected ActionBarDrawerToggle DrawerToggle { get; private set; }

        private FrameLayout DrawerSyncView { get; set; }

        private Toolbar MainToolbar { get; set; }


        protected override void OnCreateActivity (Bundle state)
        {
            base.OnCreateActivity (state);

            SetContentView (Resource.Layout.MainDrawerActivity);

            DrawerListView = FindViewById<ListView> (Resource.Id.DrawerListView);
            DrawerUserView = LayoutInflater.Inflate (Resource.Layout.MainDrawerListHeader, null);
            DrawerUserName = DrawerUserView.FindViewById<TextView> (Resource.Id.TitleTextView);
            DrawerImage = DrawerUserView.FindViewById<ProfileImageView> (Resource.Id.IconProfileImageView);
            DrawerListView.AddHeaderView (DrawerUserView);
            DrawerListView.Adapter = drawerAdapter = new DrawerListAdapter ();
            DrawerListView.ItemClick += OnDrawerListViewItemClick;

            var authManager = ServiceContainer.Resolve<AuthManager> ();
            authManager.PropertyChanged += OnUserChangedEvent;

            DrawerLayout = FindViewById<DrawerLayout> (Resource.Id.DrawerLayout);
            DrawerToggle = new ActionBarDrawerToggle (this, DrawerLayout, MainToolbar, Resource.String.EntryName, Resource.String.EntryName);

            DrawerLayout.SetDrawerShadow (Resource.Drawable.drawershadow, (int)GravityFlags.Start);
            DrawerLayout.SetDrawerListener (DrawerToggle);

            Timer.OnCreate (this);

            var lp = new Android.Support.V7.App.ActionBar.LayoutParams (ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, (int)GravityFlags.Right);

            MainToolbar = FindViewById<Toolbar> (Resource.Id.MainToolbar);
            SetSupportActionBar (MainToolbar);
//            MainToolbar.SetNavigationIcon (Resource.Drawable.IcDrawer);
//            SupportActionBar.SetTitle (Resource.String.MainDrawerTimer);
//            SupportActionBar.SetCustomView (Timer.Root, lp);
//            SupportActionBar.SetDisplayShowCustomEnabled (true);

            var bus = ServiceContainer.Resolve<MessageBus> ();
            drawerSyncStarted = bus.Subscribe<SyncStartedMessage> (SyncStarted);
            drawerSyncFinished = bus.Subscribe<SyncFinishedMessage> (SyncFinished);

            DrawerSyncView = FindViewById<FrameLayout> (Resource.Id.DrawerSyncStatus);

            syncRetryButton = DrawerSyncView.FindViewById<ImageButton> (Resource.Id.SyncRetryButton);
            syncRetryButton.Click += OnSyncRetryClick;

            syncStatusText = DrawerSyncView.FindViewById<TextView> (Resource.Id.SyncStatusText);

            if (state == null) {
                OpenPage (DrawerListAdapter.TimerPageId);
            } else {
                // Restore page stack
                pageStack.Clear ();
                var arr = state.GetIntArray (PageStackExtra);
                if (arr != null) {
                    pageStack.AddRange (arr);
                }
            }
        }

        private void OnUserChangedEvent (object sender, PropertyChangedEventArgs args)
        {
            var userData = ServiceContainer.Resolve<AuthManager> ().User;
            if (userData == null) {
                return;
            }
            DrawerUserName.Text = userData.Name;
            DrawerImage.ImageUrl = userData.ImageUrl;
        }

        protected override void OnSaveInstanceState (Bundle outState)
        {
            outState.PutIntArray (PageStackExtra, pageStack.ToArray ());
            outState.PutLong (LastSyncArgument, lastSyncInMillis);
            outState.PutInt (LastSyncResultArgument, syncStatus);
            base.OnSaveInstanceState (outState);
        }

        protected override void OnPostCreate (Bundle savedInstanceState)
        {
            if (savedInstanceState != null) {
                lastSyncInMillis = savedInstanceState.GetLong (LastSyncArgument);
                syncStatus = savedInstanceState.GetInt (LastSyncResultArgument);
                UpdateSyncStatus ();
            }
            base.OnPostCreate (savedInstanceState);
        }

        public override void OnConfigurationChanged (Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged (newConfig);
            DrawerToggle.OnConfigurationChanged (newConfig);
        }

        public override bool OnOptionsItemSelected (IMenuItem item)
        {
            return DrawerToggle.OnOptionsItemSelected (item) || base.OnOptionsItemSelected (item);
        }

        protected override void OnStart ()
        {
            base.OnStart ();
            Timer.OnStart ();
        }

        protected override void OnStop ()
        {
            base.OnStop ();
            Timer.OnStop ();
        }

        protected override void OnDestroy ()
        {
            Timer.OnDestroy (this);

            var bus = ServiceContainer.Resolve<MessageBus> ();

            if (drawerSyncStarted != null) {
                bus.Unsubscribe (drawerSyncStarted);
                drawerSyncStarted = null;
            }

            if (drawerSyncFinished != null) {
                bus.Unsubscribe (drawerSyncFinished);
                drawerSyncFinished = null;
            }

            base.OnDestroy ();
        }

        public override void OnBackPressed ()
        {
            if (pageStack.Count > 0) {
                pageStack.RemoveAt (pageStack.Count - 1);
                var pageId = pageStack.Count > 0 ? pageStack [pageStack.Count - 1] : DrawerListAdapter.TimerPageId;
                OpenPage (pageId);
            } else {
                base.OnBackPressed ();
            }
        }

        private void SetMenuSelection (int pos)
        {
            int parentPos = drawerAdapter.GetParentPosition (pos -1);
            DrawerListView.ClearChoices ();
            if (parentPos > -1) {
                DrawerListView.ChoiceMode = (ChoiceMode)ListView.ChoiceModeMultiple;
                DrawerListView.SetItemChecked (parentPos, true);
                DrawerListView.SetItemChecked (pos, true);
            } else {
                DrawerListView.ChoiceMode = (ChoiceMode)ListView.ChoiceModeSingle;
                DrawerListView.SetItemChecked (pos, true);
            }
        }

        private void OpenPage (int id)
        {
            if (id == DrawerListAdapter.SettingsPageId) {
                OpenFragment (settingsFragment.Value);
                SupportActionBar.SetTitle (Resource.String.MainDrawerSettings);
            } else if (id == DrawerListAdapter.ReportsPageId) {
                drawerAdapter.ExpandCollapse (DrawerListAdapter.ReportsPageId);
                if (reportFragment.Value.ZoomLevel == ZoomLevel.Week) {
                    SupportActionBar.SetTitle (Resource.String.MainDrawerReportsWeek);
                    id = DrawerListAdapter.ReportsWeekPageId;
                } else if (reportFragment.Value.ZoomLevel == ZoomLevel.Month) {
                    SupportActionBar.SetTitle (Resource.String.MainDrawerReportsMonth);
                    id = DrawerListAdapter.ReportsMonthPageId;
                } else {
                    SupportActionBar.SetTitle (Resource.String.MainDrawerReportsYear);
                    id = DrawerListAdapter.ReportsYearPageId;
                }
                OpenFragment (reportFragment.Value);
            } else if (id == DrawerListAdapter.ReportsWeekPageId) {
                drawerAdapter.ExpandCollapse (DrawerListAdapter.ReportsPageId);
                SupportActionBar.SetTitle (Resource.String.MainDrawerReportsWeek);
                reportFragment.Value.ZoomLevel = ZoomLevel.Week;
                OpenFragment (reportFragment.Value);
            } else if (id == DrawerListAdapter.ReportsMonthPageId) {
                drawerAdapter.ExpandCollapse (DrawerListAdapter.ReportsPageId);
                SupportActionBar.SetTitle (Resource.String.MainDrawerReportsMonth);
                reportFragment.Value.ZoomLevel = ZoomLevel.Month;
                OpenFragment (reportFragment.Value);
            } else if (id == DrawerListAdapter.ReportsYearPageId) {
                drawerAdapter.ExpandCollapse (DrawerListAdapter.ReportsPageId);
                SupportActionBar.SetTitle (Resource.String.MainDrawerReportsYear);
                reportFragment.Value.ZoomLevel = ZoomLevel.Year;
                OpenFragment (reportFragment.Value);
            } else if (id == DrawerListAdapter.FeedbackPageId) {
                SupportActionBar.SetTitle (Resource.String.MainDrawerFeedback);
                drawerAdapter.ExpandCollapse (DrawerListAdapter.FeedbackPageId);
                OpenFragment (feedbackFragment.Value);
            } else {
                SupportActionBar.SetTitle (Resource.String.MainDrawerTimer);
                OpenFragment (trackingFragment.Value);
                drawerAdapter.ExpandCollapse (DrawerListAdapter.TimerPageId);
                Timer.HideAction = false;
            }
            SetMenuSelection (drawerAdapter.GetItemPosition (id));

            pageStack.Remove (id);
            pageStack.Add (id);
            // Make sure we don't store the timer page as the first page (this is implied)
            if (pageStack.Count == 1 && id == DrawerListAdapter.TimerPageId) {
                pageStack.Clear ();
            }
        }

        private void OpenFragment (Fragment fragment)
        {
            var old = FragmentManager.FindFragmentById (Resource.Id.ContentFrameLayout);
            if (old == null) {
                FragmentManager.BeginTransaction ()
                .Add (Resource.Id.ContentFrameLayout, fragment)
                .Commit ();
            } else if (old != fragment) {
                // The detach/attach is a workaround for https://code.google.com/p/android/issues/detail?id=42601
                FragmentManager.BeginTransaction ()
                .Detach (old)
                .Replace (Resource.Id.ContentFrameLayout, fragment)
                .Attach (fragment)
                .Commit ();
            }
        }

        private void OnDrawerListViewItemClick (object sender, ListView.ItemClickEventArgs e)
        {
            // If tap outside options just close drawer
            if (e.Id == -1) {
                DrawerLayout.CloseDrawers ();
                return;
            }

            // Configure timer component for selected page:
            if (e.Id != DrawerListAdapter.TimerPageId) {
                Timer.HideAction = true;
                Timer.HideDuration = false;
            } else {
                Timer.HideAction = false;
            }

            if (e.Id == DrawerListAdapter.TimerPageId) {
                OpenPage (DrawerListAdapter.TimerPageId);

            } else if (e.Id == DrawerListAdapter.LogoutPageId) {
                var authManager = ServiceContainer.Resolve<AuthManager> ();
                authManager.Forget ();
                StartAuthActivity ();
            } else if (e.Id == DrawerListAdapter.ReportsPageId) {
                OpenPage (DrawerListAdapter.ReportsPageId);
            } else if (e.Id == DrawerListAdapter.ReportsWeekPageId) {
                OpenPage (DrawerListAdapter.ReportsWeekPageId);
            } else if (e.Id == DrawerListAdapter.ReportsMonthPageId) {
                OpenPage (DrawerListAdapter.ReportsMonthPageId);
            } else if (e.Id == DrawerListAdapter.ReportsYearPageId) {
                OpenPage (DrawerListAdapter.ReportsYearPageId);
            } else if (e.Id == DrawerListAdapter.SettingsPageId) {
                OpenPage (DrawerListAdapter.SettingsPageId);

            } else if (e.Id == DrawerListAdapter.FeedbackPageId) {
                OpenPage (DrawerListAdapter.FeedbackPageId);
            }

            DrawerLayout.CloseDrawers ();
        }

        protected void SyncStarted (SyncStartedMessage msg)
        {
            syncRetryButton.Enabled = false;
            syncStatus = syncing;
            syncStatusText.SetText (Resource.String.CurrentlySyncingStatusText);
        }

        private void SyncFinished (SyncFinishedMessage msg)
        {
            syncRetryButton.Enabled = true;
            if (msg.FatalError != null) {
                syncStatus = syncFatalError;
            } else if (msg.HadErrors) {
                syncStatus = syncHadErrors;
            } else {
                syncStatus = syncSuccessful;
            }
            lastSyncInMillis = Toggl.Phoebe.Time.Now.Ticks / TimeSpan.TicksPerMillisecond;
            UpdateSyncStatus ();
        }

        private void OnSyncRetryClick (object sender, EventArgs e)
        {
            var syncManager = ServiceContainer.Resolve<ISyncManager> ();
            syncManager.Run ();
        }

        private void UpdateSyncStatus ()
        {
            if (syncStatus == syncing) {
                syncStatusText.SetText (Resource.String.CurrentlySyncingStatusText);
            } else if (syncStatus == syncHadErrors) {
                syncStatusText.SetText (Resource.String.LastSyncHadErrors);
            } else if (syncStatus == syncFatalError) {
                syncStatusText.SetText (Resource.String.LastSyncFatalError);
            } else {
                syncStatusText.Text = String.Format (Resources.GetString (Resource.String.LastSyncStatusText), ResolveLastSyncTime ());
            }
            handler.RemoveCallbacks (UpdateSyncStatus);
            handler.PostDelayed (UpdateSyncStatus, DateUtils.MinuteInMillis);
        }

        private String ResolveLastSyncTime ()
        {
            var NowInMillis = Toggl.Phoebe.Time.Now.Ticks / TimeSpan.TicksPerMillisecond;
            if (NowInMillis - lastSyncInMillis < DateUtils.MinuteInMillis) {
                return Resources.GetString (Resource.String.LastSyncJustNow);
            }
            return DateUtils.GetRelativeTimeSpanString (lastSyncInMillis, NowInMillis, 0L);
        }

        public TimerComponent Timer
        {
            get { return barTimer; }
        }
    }
}
