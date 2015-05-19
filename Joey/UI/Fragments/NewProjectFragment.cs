﻿using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Toggl.Joey.UI.Activities;
using Toggl.Joey.UI.Views;
using Toggl.Phoebe.Data.ViewModels;
using ActionBar = Android.Support.V7.App.ActionBar;
using Activity = Android.Support.V7.App.AppCompatActivity;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Fragment = Android.Support.V4.App.Fragment;
using Toolbar = Android.Support.V7.Widget.Toolbar;


namespace Toggl.Joey.UI.Fragments
{
    public class NewProjectFragment : Fragment
    {
        private ActionBar Toolbar;
        private bool isSaving;
        private NewProjectViewModel viewModel;

        public TogglField ProjectBit { get; private set; }
        public ColorPickerRecyclerView ColorPicker { get; private set; }

        public NewProjectFragment ()
        {
        }

        public NewProjectFragment (IntPtr jref, Android.Runtime.JniHandleOwnership xfer) : base (jref, xfer)
        {
        }

        public NewProjectFragment (Guid workspaceId)
        {
            viewModel = new NewProjectViewModel (workspaceId);
        }

        public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate (Resource.Layout.NewProjectFragment, container, false);
            var activity = (Activity)Activity;

            var toolbar = view.FindViewById<Toolbar> (Resource.Id.NewProjectsFragmentToolbar);
            activity.SetSupportActionBar (toolbar);

            Toolbar = activity.SupportActionBar;
            Toolbar.SetDisplayHomeAsUpEnabled (true);
            Toolbar.SetTitle (Resource.String.NewProjectTitle);

            ProjectBit = view.FindViewById<TogglField> (Resource.Id.NewProjectProjectNameBit)
                         .DestroyAssistView().DestroyArrow()
                         .SetName (Resource.String.NewProjectProjectFieldName);
            ProjectBit.TextField.TextChanged += ProjectBitTextChangedHandler;

            ColorPicker = view.FindViewById<ColorPickerRecyclerView> (Resource.Id.NewProjectColorPickerRecyclerView);
            ColorPicker.SelectedColorChanged += (sender, e) => {
                viewModel.Model.Color = e;
            };

            HasOptionsMenu = true;
            return view;
        }

        public override void OnViewCreated (View view, Bundle savedInstanceState)
        {
            base.OnViewCreated (view, savedInstanceState);

            if (viewModel == null) {
                var workspaceId = NewProjectActivity.GetWorkspaceData (Activity.Intent);
                viewModel = new NewProjectViewModel (workspaceId);
            }

            viewModel.OnIsLoadingChanged += OnModelLoaded;
            viewModel.Init ();
        }

        private void OnModelLoaded (object sender, EventArgs e)
        {
            if (!viewModel.IsLoading) {
                if (viewModel == null) {
                    Activity.Finish ();
                }
            }
        }

        public override void OnStart ()
        {
            base.OnStart ();
            var inputService = (InputMethodManager)Activity.GetSystemService (Context.InputMethodService);
            ProjectBit.TextField.PostDelayed (delegate {
                inputService.ShowSoftInput (ProjectBit.TextField, ShowFlags.Implicit);
            }, 100);
        }

        public override void OnDestroyView ()
        {
            viewModel.OnIsLoadingChanged -= OnModelLoaded;
            viewModel.Dispose ();
            base.OnDestroyView ();
        }

        private async void SaveButtonHandler (object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace (viewModel.Model.Name)) {
                new AlertDialog.Builder (Activity)
                .SetTitle (Resource.String.NewProjectEmptyDialogTitle)
                .SetMessage (Resource.String.NewProjectEmptyDialogMessage)
                .SetPositiveButton (Resource.String.NewProjectEmptyDialogPositiveButtonTitle, (EventHandler<DialogClickEventArgs>)null)
                .Show();
                return;
            }

            if (isSaving) {
                return;
            }

            isSaving = true;

            try {
                var existWithName = await viewModel.ExistProjectWithName (ProjectBit.TextField.Text);
                if (existWithName) {
                    new AlertDialog.Builder (Activity)
                    .SetTitle (Resource.String.NewProjectDuplicateDialogTitle)
                    .SetMessage (Resource.String.NewProjectDuplicateDialogMessage)
                    .SetPositiveButton (Resource.String.NewProjectEmptyDialogPositiveButtonTitle, (EventHandler<DialogClickEventArgs>)null)
                    .Show();
                } else {
                    await viewModel.SaveProjectModel ();
                    FinishActivity (true);
                }
            } finally {
                isSaving = false;
            }
        }

        private void ProjectBitTextChangedHandler (object sender, TextChangedEventArgs e)
        {
            var t = ProjectBit.TextField.Text;
            if (t != viewModel.Model.Name) {
                viewModel.Model.Name = t;
            }
        }

        public override void OnCreateOptionsMenu (IMenu menu, MenuInflater inflater)
        {
            menu.Add (Resource.String.NewProjectSaveButtonText).SetShowAsAction (ShowAsAction.Always);
        }

        public override bool OnOptionsItemSelected (IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home) {
                FinishActivity (false);
            } else {
                SaveButtonHandler (this, null);
            }
            return base.OnOptionsItemSelected (item);
        }

        private void FinishActivity (bool isProjectCreated)
        {
            var resultIntent = new Intent ();
            if (isProjectCreated) {
                resultIntent.PutExtra (NewProjectActivity.ExtraWorkspaceId, viewModel.Model.Workspace.Id.ToString());
                resultIntent.PutExtra (NewProjectActivity.ExtraProjectId, viewModel.Model.Id.ToString());
            }
            Activity.SetResult (isProjectCreated ? Result.Ok : Result.Canceled, resultIntent);
            Activity.Finish();;
        }
    }
}

