using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Rg.Plugins.Popup.Droid.Gestures;
using Rg.Plugins.Popup.Droid.Renderers;
using Rg.Plugins.Popup.Pages;
using Point = Microsoft.Maui.Graphics.Point;
using Rectangle = Microsoft.Maui.Graphics.Rectangle;
using View = Android.Views.View;
using ACompatibility = Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui;
using ContextExtensions = Microsoft.Maui.Controls.Compatibility.Platform.Android.ContextExtensions;
using System.ComponentModel;

namespace Rg.Plugins.Popup.Droid.Renderers
{
    [Preserve(AllMembers = true)]
    public class PopupPageRenderer : PageRenderer
    {
        private readonly RgGestureDetectorListener _gestureDetectorListener;
        private readonly GestureDetector _gestureDetector;
        private DateTime _downTime;
        private Point _downPosition;
        private bool _disposed;

        private PopupPage CurrentElement => (PopupPage)Element;

        #region Main Methods

        public PopupPageRenderer(Context context) : base(context)
        {
            _gestureDetectorListener = new RgGestureDetectorListener();

            _gestureDetectorListener.Clicked += OnBackgroundClick;

            _gestureDetector = new GestureDetector(Context, _gestureDetectorListener);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disposed = true;

                if (_gestureDetectorListener != null)
                {
                    _gestureDetectorListener.Clicked -= OnBackgroundClick;
                    _gestureDetectorListener.Dispose();
                }
                _gestureDetector?.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Layout Methods

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            var activity = (Activity?)Context;

            Thickness systemPadding;
            var keyboardOffset = 0d;

            var decoreView = activity?.Window?.DecorView;

            var visibleRect = new Android.Graphics.Rect();

            decoreView?.GetWindowVisibleDisplayFrame(visibleRect);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M && RootWindowInsets != null)
            {
                var h = b - t;

                var windowInsets = RootWindowInsets;
                var bottomPadding = Math.Min(windowInsets.StableInsetBottom, windowInsets.SystemWindowInsetBottom);

                if (h - visibleRect.Bottom > windowInsets.StableInsetBottom)
                {
                    keyboardOffset = ContextExtensions.FromPixels(Context, h - visibleRect.Bottom);
                }

                systemPadding = new Thickness
                {
                    Left = ContextExtensions.FromPixels(Context, windowInsets.SystemWindowInsetLeft),
                    Top = ContextExtensions.FromPixels(Context, windowInsets.SystemWindowInsetTop),
                    Right = ContextExtensions.FromPixels(Context, windowInsets.SystemWindowInsetRight),
                    Bottom = ContextExtensions.FromPixels(Context, bottomPadding)
                };
            }
            else if (Build.VERSION.SdkInt < BuildVersionCodes.M && decoreView != null)
            {
                var screenSize = new Android.Graphics.Point();
                activity?.WindowManager?.DefaultDisplay?.GetSize(screenSize);

                var keyboardHeight = 0d;

                var decoreHeight = decoreView.Height;
                var decoreWidht = decoreView.Width;

                if (visibleRect.Bottom < screenSize.Y)
                {
                    keyboardHeight = screenSize.Y - visibleRect.Bottom;
                    keyboardOffset = ContextExtensions.FromPixels(Context, decoreHeight - visibleRect.Bottom);
                }

                systemPadding = new Thickness
                {
                    Left = ContextExtensions.FromPixels(Context, visibleRect.Left),
                    Top = ContextExtensions.FromPixels(Context, visibleRect.Top),
                    Right = ContextExtensions.FromPixels(Context, decoreWidht - visibleRect.Right),
                    Bottom = ContextExtensions.FromPixels(Context, decoreHeight - visibleRect.Bottom - keyboardHeight)
                };
            }
            else
            {
                systemPadding = new Thickness();
            }

            CurrentElement.SetValue(PopupPage.SystemPaddingProperty, systemPadding);
            CurrentElement.SetValue(PopupPage.KeyboardOffsetProperty, keyboardOffset);

            if (changed)
            {
                CurrentElement.Layout(new Rectangle(ContextExtensions.FromPixels(Context, l), ContextExtensions.FromPixels(Context, t), ContextExtensions.FromPixels(Context, r), ContextExtensions.FromPixels(Context, b)));
                // CurrentElement.ForceLayout();
            }
            else
                CurrentElement.ForceLayout();

            base.OnLayout(changed, l, t, r, b);
        }

        #endregion

        #region Life Cycle Methods

        protected override void OnAttachedToWindow()
        {
            ContextExtensions.HideKeyboard(Context, ((Activity?)Context)?.Window?.DecorView);
            base.OnAttachedToWindow();
        }

        protected override void OnDetachedFromWindow()
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(0), () =>
            {
                ContextExtensions.HideKeyboard(Popup.Context, ((Activity?)Popup.Context)?.Window?.DecorView);
                return false;
            });
            base.OnDetachedFromWindow();
        }

        protected override void OnWindowVisibilityChanged(ViewStates visibility)
        {
            base.OnWindowVisibilityChanged(visibility);

            // It is needed because a size of popup has not updated on Android 7+. See #209
            if (visibility == ViewStates.Visible)
                RequestLayout();
        }

        #endregion

        #region Touch Methods

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            if (e.Action == MotionEventActions.Down)
            {
                _downTime = DateTime.UtcNow;
                _downPosition = new Point(e.RawX, e.RawY);
            }
            if (e.Action != MotionEventActions.Up)
                return base.DispatchTouchEvent(e);

            if (_disposed)
                return false;

            View? currentFocus1 = ((Activity?)Context)?.CurrentFocus;

            if (currentFocus1 is EditText)
            {
                View? currentFocus2 = ((Activity?)Context)?.CurrentFocus;
                if (currentFocus1 == currentFocus2 && _downPosition.Distance(new Point(e.RawX, e.RawY)) <= ContextExtensions.ToPixels(Context, 20.0) && !(DateTime.UtcNow - _downTime > TimeSpan.FromMilliseconds(200.0)))
                {
                    var location = new int[2];
                    currentFocus1.GetLocationOnScreen(location);
                    var num1 = e.RawX + currentFocus1.Left - location[0];
                    var num2 = e.RawY + currentFocus1.Top - location[1];
                    if (!new Rectangle(currentFocus1.Left, currentFocus1.Top, currentFocus1.Width, currentFocus1.Height).Contains(num1, num2))
                    {
                        ContextExtensions.HideKeyboard(Context, currentFocus1);
                        currentFocus1.ClearFocus();
                    }
                }
            }

            if (_disposed)
                return false;

            var flag = base.DispatchTouchEvent(e);

            return flag;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (_disposed)
                return false;

            var baseValue = base.OnTouchEvent(e);

            _gestureDetector.OnTouchEvent(e);

            if (CurrentElement != null && CurrentElement.BackgroundInputTransparent)
            {
                if ((ChildCount > 0 && !IsInRegion(e.RawX, e.RawY, GetChildAt(0)!)) || ChildCount == 0)
                {
                    CurrentElement.SendBackgroundClick();
                    return false;
                }
            }

            return baseValue;
        }

        private void OnBackgroundClick(object sender, MotionEvent e)
        {
            if (ChildCount == 0)
                return;

            var isInRegion = IsInRegion(e.RawX, e.RawY, GetChildAt(0)!);

            if (!isInRegion)
                CurrentElement.SendBackgroundClick();
        }

        // Fix for "CloseWhenBackgroundIsClicked not works on Android with Xamarin.Forms 2.4.0.280" #173
        private static bool IsInRegion(float x, float y, View v)
        {
            var mCoordBuffer = new int[2];

            v.GetLocationOnScreen(mCoordBuffer);
            return mCoordBuffer[0] + v.Width > x &&    // right edge
                   mCoordBuffer[1] + v.Height > y &&   // bottom edge
                   mCoordBuffer[0] < x &&              // left edge
                   mCoordBuffer[1] < y;                // top edge
        }

        #endregion
    }

    public class PageRenderer : VisualElementRenderer<Page>
	{
		public PageRenderer(Context context) : base(context)
		{
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			base.OnTouchEvent(e);

			return true;
		}

		IPageController PageController => Element as IPageController;

		double _previousHeight;
		bool _isDisposed = false;

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				PageController?.SendDisappearing();
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();
			// var pageContainer = Parent as PageContainer;
			// if (pageContainer != null && (pageContainer.IsInFragment || pageContainer.Visibility == ViewStates.Gone))
			// 	return;
			PageController.SendAppearing();
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();
			// var pageContainer = Parent as PageContainer;
			// if (pageContainer != null && pageContainer.IsInFragment)
			// 	return;
			PageController.SendDisappearing();
		}

		// protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
		// {
		// 	base.OnElementChanged(e);

		// 	// if (Id == NoId)
		// 	// {
		// 	// 	Id = Platform.GenerateViewId();
		// 	// }

		// 	UpdateBackground(false);

		// 	// if (!Flags.IsAccessibilityExperimentalSet())
		// 	// 	Clickable = true;
		// }

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Page.BackgroundImageSourceProperty.PropertyName)
				UpdateBackground(true);
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackground(false);
			else if (e.PropertyName == VisualElement.BackgroundProperty.PropertyName)
				UpdateBackground(false);
			else if (e.PropertyName == VisualElement.HeightProperty.PropertyName)
				UpdateHeight();
		}

		void UpdateHeight()
		{
			// Handle size changes because of the soft keyboard (there's probably a more elegant solution to this)

			// This is only necessary if:
			// - we're navigating back from a page where the soft keyboard was open when the user hit the Navigation Bar 'back' button
			// - the Application's content height has changed because WindowSoftInputModeAdjust was set to Resize
			// - the height has increased (in other words, the last layout was with the keyboard open, and now it's closed)
			var newHeight = Element.Height;

			if (_previousHeight > 0 && newHeight > _previousHeight)
			{
				var nav = Element.Navigation;

				// This update check will fire for all the pages on the stack, but we only need to request a layout for the top one
				if (nav?.NavigationStack != null && nav.NavigationStack.Count > 0 && Element == nav.NavigationStack[nav.NavigationStack.Count - 1])
				{
					// The Forms layout stuff is already correct, we just need to force Android to catch up
					RequestLayout();
				}
			}

			// Cache the height for next time
			_previousHeight = newHeight;
		}

		void UpdateBackground(bool setBkndColorEvenWhenItsDefault)
		{
			// Page page = Element;

			// _ = this.ApplyDrawableAsync(page, Page.BackgroundImageSourceProperty, Context, drawable =>
			// {
			// 	if (drawable != null)
			// 	{
			// 		this.SetBackground(drawable);
			// 	}
			// 	else
			// 	{
			// 		Brush background = Element.Background;

			// 		if (!Brush.IsNullOrEmpty(background))
			// 			this.UpdateBackground(background);
			// 		else
			// 		{
			// 			Color backgroundColor = page.BackgroundColor;
			// 			bool isDefaultBackgroundColor = backgroundColor.IsDefault;

			// 			// A TabbedPage has no background. See Github6384.
			// 			bool isInShell = page.Parent is BaseShellItem ||
			// 			(page.Parent is TabbedPage && page.Parent?.Parent is BaseShellItem);

			// 			if (isInShell && isDefaultBackgroundColor)
			// 			{
			// 				var color = Forms.IsMarshmallowOrNewer ?
			// 					Context.Resources.GetColor(AColorRes.BackgroundLight, Context.Theme) :
			// 					new AColor(ContextCompat.GetColor(Context, AColorRes.BackgroundLight));
			// 				SetBackgroundColor(color);
			// 			}
			// 			else if (!isDefaultBackgroundColor || setBkndColorEvenWhenItsDefault)
			// 			{
			// 				SetBackgroundColor(backgroundColor.ToAndroid());
			// 			}
			// 		}
			// 	}
			// });
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			base.OnLayout(changed, l, t, r, b);
		}
	}
}
