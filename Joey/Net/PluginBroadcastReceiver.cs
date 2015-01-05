using System;
using Android.App;
using Android.Content;
using Android.Support.V4.Content;
using Android.Widget;
using Toggl.Joey.UI.Activities;

namespace Toggl.Joey.Net
{
    [BroadcastReceiver]
    public class PluginBroadcastReceiver : WakefulBroadcastReceiver
    {
        public override void OnReceive (Context context, Intent intent)
        {
            var serviceIntent = new Intent (context, typeof (TaskerService));
            serviceIntent.ReplaceExtras (intent.Extras);
            var Gstring  = String.Format ("string message: {0}", serviceIntent.GetIntExtra (EditActivity.TaskCommand, 0));
            Toast.MakeText (context, Gstring, ToastLength.Short).Show ();
            StartWakefulService (context, serviceIntent);
            ResultCode = Result.Ok;
        }
    }
}

