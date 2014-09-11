using System;
using Android.App;
using Android.Content;
using Android.OS;
using Toggl.Phoebe;
using Toggl.Phoebe.Net;
using XPlatUtils;

namespace Toggl.Joey.Net
{
    [Service (Exported = false)]
    public class TaskerService : Service
    {
        public TaskerService () : base ()
        {
        }

        public TaskerService (IntPtr javaRef, Android.Runtime.JniHandleOwnership transfer)
            : base (javaRef, transfer)
        {
        }

    }
}
