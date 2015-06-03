using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;

namespace Toggl.Joey.UI.Views
{
    public class NotificationImageView : ImageView
    {
        private Context ctx;
        private int bubbleCount = 0;
        private readonly Drawable originalDrawable;
        private TextView bubbleText;
        private Bitmap baseBitmap;
        private Drawable endDrawable;
        private Canvas baseCanvas;

        public NotificationImageView (Context context) : base (context)
        {
            ctx = context;
            originalDrawable = Drawable;
        }

        public NotificationImageView (Context context, IAttributeSet attrs) : base (context, attrs)
        {
            ctx = context;
            originalDrawable = Drawable;
        }

        public NotificationImageView (Context context, IAttributeSet attrs, int defStyle) : base (context, attrs, defStyle)
        {
            ctx = context;
            originalDrawable = Drawable;
        }

        public int BubbleCount
        {
            get {
                return bubbleCount;
            } set {
                bubbleCount = value;
                UpdateBubble ();
            }
        }

        public void UpdateBubble()
        {
            MakeBubbleText ();
            if (baseCanvas == null
                    || (bubbleText != null && bubbleText.Text.Length > bubbleCount.ToString ().Length)) {
                MakeBaseBitmap ();
            }
            baseCanvas.DrawBitmap (bubbleText.DrawingCache, baseBitmap.Width - bubbleText.Width, 0, null);
            endDrawable = new BitmapDrawable (ctx.Resources, baseBitmap);
            SetImageDrawable (endDrawable);
        }

        private bool renewBase
        {
            get {
                return (baseCanvas == null || (bubbleText != null && bubbleText.Text.Length > bubbleCount.ToString ().Length)) ? true : false;
            }
        }

        private void MakeBubbleText ()
        {
            var dm = ctx.Resources.DisplayMetrics;
            bubbleText = new TextView (ctx);
            bubbleText.SetTextSize (ComplexUnitType.Sp, 7.5f);
            bubbleText.Gravity = GravityFlags.Center;
            bubbleText.SetTextColor (ctx.Resources.GetColor (Resource.Color.dark_gray_text));
            bubbleText.SetBackgroundResource (Resource.Drawable.NotificationBubble);
            int padding = (int)TypedValue.ApplyDimension (ComplexUnitType.Dip, 3, dm);
            bubbleText.SetPadding (padding, 0, padding, 0);
            bubbleText.Text = bubbleCount.ToString();
            bubbleText.DrawingCacheEnabled = true;
            bubbleText.Measure (0, 0);
            bubbleText.Layout (0, 0, bubbleText.MeasuredWidth, bubbleText.MeasuredHeight);
            bubbleText.BuildDrawingCache();
        }

        private void MakeBaseBitmap ()
        {
            var iconBitmap = ((BitmapDrawable)originalDrawable).Bitmap;
            iconBitmap = iconBitmap.Copy (Bitmap.Config.Argb8888, true);
            var dm = ctx.Resources.DisplayMetrics;
            int top = (int)TypedValue.ApplyDimension (ComplexUnitType.Dip, 4, dm);
            int left = (int)TypedValue.ApplyDimension (ComplexUnitType.Dip, 2, dm);
            baseBitmap = Bitmap.CreateBitmap (iconBitmap.Width + left, iconBitmap.Height + top, Bitmap.Config.Argb8888);
            baseCanvas = new Canvas (baseBitmap);
            baseCanvas.DrawBitmap (iconBitmap, 0, top, null);
        }
    }
}

