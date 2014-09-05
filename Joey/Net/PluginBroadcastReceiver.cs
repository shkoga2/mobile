
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

//            if (!com.twofortyfouram.locale.Intent.ACTION_FIRE_SETTING.equals(intent.getAction()))
//            {
//                if (Constants.IS_LOGGABLE)
//                {
//                    Log.e(Constants.LOG_TAG,
//                        String.format(Locale.US, "Received unexpected Intent action %s", intent.getAction())); //$NON-NLS-1$
//                }
//                return;
//            }
            Toast.MakeText (context, "Received intent!", ToastLength.Short).Show ();
        }
    }
}

