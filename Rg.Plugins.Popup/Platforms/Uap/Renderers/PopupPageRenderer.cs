﻿using System;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Windows.Renderers;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui;
using Microsoft.Maui.ControlsInternals;
using Microsoft.Maui.Controls.Platform.UWP;
using Size = Windows.Foundation.Size;
using WinPopup = global::Windows.UI.Xaml.Controls.Primitives.Popup;

[assembly: ExportRenderer(typeof(PopupPage), typeof(PopupPageRenderer))]
namespace Rg.Plugins.Popup.Windows.Renderers
{
    [Preserve(AllMembers = true)]
    public class PopupPageRenderer : PageRenderer
    {
        private Rect _keyboardBounds;

        internal WinPopup? Container { get; private set; }

        private PopupPage CurrentElement => (PopupPage)Element;

        [Preserve]
        public PopupPageRenderer()
        {

        }

        private void OnKeyboardHiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            _keyboardBounds = Rect.Empty;
            UpdateElementSize();
        }

        private void OnKeyboardShowing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            _keyboardBounds = sender.OccludedRect;
            UpdateElementSize();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            UpdateElementSize();

            return base.ArrangeOverride(finalSize);
        }

        internal void Prepare(WinPopup container)
        {
            Container = container;

            Window.Current.SizeChanged += OnSizeChanged;
            DisplayInformation.GetForCurrentView().OrientationChanged += OnOrientationChanged;

            var inputPane = InputPane.GetForCurrentView();
            inputPane.Showing += OnKeyboardShowing;
            inputPane.Hiding += OnKeyboardHiding;

            ContainerElement.PointerPressed += OnBackgroundClick;
        }

        internal void Destroy()
        {
            Container = null;

            Window.Current.SizeChanged -= OnSizeChanged;
            DisplayInformation.GetForCurrentView().OrientationChanged -= OnOrientationChanged;

            var inputPane = InputPane.GetForCurrentView();
            inputPane.Showing -= OnKeyboardShowing;
            inputPane.Hiding -= OnKeyboardHiding;

            ContainerElement.PointerPressed -= OnBackgroundClick;
        }

        private void OnOrientationChanged(DisplayInformation sender, object args)
        {
            UpdateElementSize();
        }

        private void OnSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            UpdateElementSize();
        }

        private void OnBackgroundClick(object sender, PointerRoutedEventArgs e)
        {
            if (e.OriginalSource == this)
            {
                CurrentElement.SendBackgroundClick();
            }
        }

        private void UpdateElementSize()
        {
            if (CurrentElement != null)
            {
                var capturedElement = CurrentElement;

                var windowBound = Window.Current.Bounds;
                var visibleBounds = ApplicationView.GetForCurrentView().VisibleBounds;
                var keyboardHeight = _keyboardBounds != Rect.Empty ? _keyboardBounds.Height : 0;

                var top = Math.Max(0, visibleBounds.Top - windowBound.Top);
                var bottom = Math.Max(0, windowBound.Bottom - visibleBounds.Bottom);
                var left = Math.Max(0, visibleBounds.Left - windowBound.Left);
                var right = Math.Max(0, windowBound.Right - visibleBounds.Right);

                var systemPadding = new Xamarin.Forms.Thickness(left, top, right, bottom);

                capturedElement.SetValue(PopupPage.SystemPaddingProperty, systemPadding);
                capturedElement.SetValue(PopupPage.KeyboardOffsetProperty, keyboardHeight);
                //if its not invoked on MainThread when the popup is showed it will be blank until the user manually resizes of owner window
                Device.BeginInvokeOnMainThread(() =>
                {
                    capturedElement.Layout(new Rectangle(windowBound.X, windowBound.Y, windowBound.Width, windowBound.Height));
                    capturedElement.ForceLayout();
                });
            }
        }
    }
}
