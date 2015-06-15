﻿using System;
using Android.Content;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

namespace Toggl.Joey.UI.Views
{
    public class TogglField : RelativeLayout
    {
        public EditText TextField;
        private TextView titleText;
        private TextView assistView;
        private ImageView arrow;

        public TogglField (Context context) : base (context)
        {
            Initialize ();
        }

        public TogglField (Context context, IAttributeSet attrs) : base (context, attrs)
        {
            Initialize ();
        }

        void Initialize ()
        {
            var inflater = (LayoutInflater)Context.GetSystemService (Context.LayoutInflaterService);
            inflater.Inflate (Resource.Layout.TogglField, this);

            TextField = FindViewById<EditText> (Resource.Id.EditTimeEntryBitText);
            titleText= FindViewById<TextView> (Resource.Id.EditTimeEntryBitTitle);
            assistView = FindViewById<TextView> (Resource.Id.EditTimeEntryBitAssistView);
            arrow = FindViewById<ImageView> (Resource.Id.EditTimeEntryBitArrow);

            TextField.EditorAction += OnTextFieldEditorActionListener;
            TextField.FocusChange += (sender, e) => {
                titleText.Selected = TextField.HasFocus;
                Selected = TextField.HasFocus;
            };
        }

        private void OnTextFieldEditorActionListener (object sender, TextView.EditorActionEventArgs e)
        {
            if (e.ActionId == ImeAction.Done) {
                TextField.ClearFocus ();
            }
        }

        public TogglField SetName (string name)
        {
            titleText.Text = name;
            TextField.Hint = name;
            return this;
        }

        public TogglField SetName (int resourceId)
        {
            titleText.SetText (resourceId);
            TextField.SetHint (resourceId);
            return this;
        }

        public TogglField ShowTitle (bool show)
        {
            titleText.Visibility = show ? ViewStates.Visible : ViewStates.Gone;
            return this;
        }

        public TogglField SetAssistViewTitle (string title)
        {
            assistView.Text = title;
            assistView.Visibility = ViewStates.Visible;
            ClipText();
            return this;
        }

        public TogglField DestroyAssistView()
        {
            assistView.Visibility = ViewStates.Gone;
            ClipText();
            return this;
        }

        public TogglField RestoreAssistView()
        {
            assistView.Visibility = ViewStates.Visible;
            return this;
        }

        public TogglField DestroyArrow()
        {
            arrow.Visibility = ViewStates.Gone;
            return this;
        }

        public TogglField SimulateButton()
        {
            TextField.Touch += (sender, e) => {
                e.Handled = false;
                titleText.Pressed = e.Event.Action != MotionEventActions.Up;
            };
            TextField.Focusable = TextField.Clickable = false;
            return this;
        }

        private void ClipText()
        {
            int paddingLeft = 0;
            if (assistView.Visibility == ViewStates.Visible) {
                assistView.Measure (0, 0);
                paddingLeft = assistView.MeasuredWidth + 20;
            }
            if (arrow.Visibility == ViewStates.Visible) {
                arrow.Measure (0, 0);
                paddingLeft += arrow.MeasuredWidth;
            }
            TextField.SetPadding (TextField.PaddingLeft, TextField.PaddingTop, paddingLeft, TextField.PaddingBottom);
        }
    }
}

