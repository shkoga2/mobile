using System;
using Android.App;
using Android.Content;
using Toggl.Phoebe.Data;
using XPlatUtils;
using Toggl.Phoebe.Data.Models;
using Toggl.Phoebe.Data.DataObjects;
using Toggl.Joey.UI.Activities;

namespace Toggl.Joey.Net
{
    [Service (Exported = false)]
    public class TaskerService : Service
    {
        private ActiveTimeEntryManager timeEntryManager;

        public TaskerService () : base ()
        {
        }

        public TaskerService (IntPtr javaRef, Android.Runtime.JniHandleOwnership transfer)
        : base (javaRef, transfer)
        {
        }

        public override void OnStart (Intent intent, int startId)
        {
            if (timeEntryManager == null) {
                timeEntryManager = ServiceContainer.Resolve<ActiveTimeEntryManager> ();
            }
        }

        public override StartCommandResult OnStartCommand (Intent intent, StartCommandFlags flags, int startId)
        {
            OnStart (intent, startId);
            var commandBundle = intent.Extras.GetBundle (EditActivity.BundleForTasker);
            var command = commandBundle.GetInt (EditActivity.TaskCommand);

            if (command == EditActivity.TaskStopRunning) {
                Console.WriteLine ("intent test: {0}", commandBundle.GetInt (EditActivity.TaskCommand));
                StopActiveTimeEntry ();
            } else {
                Console.WriteLine ("no action");
            }

            return StartCommandResult.Sticky;
        }

        private async void StopActiveTimeEntry ()
        {
            var activeTimeEntryData = timeEntryManager.Active;
            var activeTimeEntry = new TimeEntryModel (new TimeEntryData (activeTimeEntryData));
            await activeTimeEntry.StopAsync ();
        }

        public override Android.OS.IBinder OnBind (Intent intent)
        {
            return null;
        }

        public override void OnCreate ()
        {
            base.OnCreate ();
            ((AndroidApp)Application).InitializeComponents ();
        }
    }
}
