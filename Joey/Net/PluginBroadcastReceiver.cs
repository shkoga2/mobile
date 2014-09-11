
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Toggl.Joey.Net
{
    [BroadcastReceiver]
    public class PluginBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive (Context context, Intent intent)
        {

            var taskerIntent = new Intent (context, typeof(TaskerService));
            taskerIntent.ReplaceExtras (intent.Extras);

            Toast.MakeText (context, "Received intent!", ToastLength.Short).Show ();
        }
    }
}

