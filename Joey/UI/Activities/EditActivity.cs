using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Toggl.Phoebe.Net;
using XPlatUtils;

namespace Toggl.Joey.UI.Activities
{
    [Activity (Label = "EditActivity")]
    public class EditActivity : Activity
    {
        public static readonly String BlurbForTasker = "com.twofortyfouram.locale.intent.extra.BLURB";
        public static readonly String BundleForTasker = "com.twofortyfouram.locale.intent.extra.BUNDLE";
        public static readonly String TaskCommand = "com.toggl.timer.extra.COMMAND";
        public static readonly int TaskNoAction = 0;
        public static readonly int TaskStopRunning = 1;
        protected LinearLayout StopRunningButton { get; private set; }
        protected LinearLayout NoActionButton { get; private set; }
        private Bundle CommandBundle = new Bundle();
        private Intent ResultIntent = new Intent ();

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);
            var authManager = ServiceContainer.Resolve<AuthManager> ();
            if (!authManager.IsAuthenticated) {
                SetContentView (Resource.Layout.TaskerPluginNotLoggedIn);
                return;
            }

            SetContentView (Resource.Layout.TaskerPlugin);
            StopRunningButton = FindViewById<LinearLayout> (Resource.Id.PluginButtonStop);
            StopRunningButton.Click += StopClicked;
            NoActionButton = FindViewById<LinearLayout> (Resource.Id.PluginButtonNoAction);
            NoActionButton.Click += NoActionClicked;
        }

        public override void Finish ()
        {
            ResultIntent.PutExtra (BundleForTasker, CommandBundle);
            SetResult (Result.Ok, ResultIntent);
            base.Finish ();
        }

        private void StopClicked (object sender, EventArgs e)
        {
            CommandBundle.PutInt (TaskCommand, TaskStopRunning);
            ResultIntent.PutExtra (BlurbForTasker, "Stop running time entry");
            Finish ();
        }

        private void NoActionClicked (object sender, EventArgs e)
        {
            CommandBundle.PutInt (TaskCommand, TaskNoAction);
            ResultIntent.PutExtra (BlurbForTasker, "No action what-so-ever.");
            Finish ();
        }
    }
}

