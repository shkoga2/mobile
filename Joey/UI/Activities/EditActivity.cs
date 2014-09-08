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
        protected LinearLayout StopRunningButton { get; private set; }
        protected LinearLayout CancelButton { get; private set; }
        private Boolean canceled = false;

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
            StopRunningButton.Click += stopClicked;
            CancelButton = FindViewById<LinearLayout> (Resource.Id.PluginButtonCancel);
            CancelButton.Click += cancelClicked;
        }

        public override void Finish()
        {
            var resultIntent = new Intent ();
            var resultBundle = new Bundle ();
            resultBundle.PutString ("com.toggl.timer.extra.STRING_MESSAGE", "Testing tasker");
            resultIntent.PutExtra (BundleForTasker, resultBundle);
            String blurb = canceled ? "No event" : "Stop entry";
            resultIntent.PutExtra (BlurbForTasker, blurb);
            SetResult (Result.Ok, resultIntent);

            base.Finish ();
        }

        private void stopClicked (object sender, EventArgs e)
        {
            Finish ();
        }

        private void cancelClicked (object sender, EventArgs e)
        {
            canceled = true;
            Finish ();
        }
    }
}

