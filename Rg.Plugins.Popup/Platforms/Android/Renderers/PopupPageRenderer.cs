﻿using System;
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
}
