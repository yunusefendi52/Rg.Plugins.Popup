﻿using System;

using ElmSharp;

using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Tizen.Renderers;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Platform.Tizen;

using EColor = ElmSharp.Color;
using EPopup = ElmSharp.Popup;

[assembly: ExportRenderer(typeof(PopupPage), typeof(PopupPageRenderer))]
namespace Rg.Plugins.Popup.Tizen.Renderers
{
    public class PopupPageRenderer : PageRenderer
    {
        private GestureLayer? _gestureLayer;
        private EPopup? _popup;
        public PopupPageRenderer()
        {
            RegisterPropertyHandler(Page.TitleProperty, UpdateTitle);
        }

        private PopupPage PopupPage => (Element as PopupPage)!;
        private ElmSharp.Rect? ContentBound => Platform.GetRenderer(PopupPage.Content)?.NativeView.Geometry;


        public void ShowPopup()
        {
            if (_popup != null)
            {
                _popup.BackButtonPressed += OnBackButtonPressed;
                _popup.Show();
            }
        }
        public void ClosePopup()
        {
            if (_popup != null)
            {
                _popup.BackButtonPressed -= OnBackButtonPressed;
                _popup.Hide();
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            if (_popup == null)
            {
                _popup = new EPopup(Forms.NativeParent)
                {
                    AlignmentX = -1,
                    AlignmentY = -1,
                    WeightX = 1.0,
                    WeightY = 1.0,
                };
                _popup.SetPartColor("bg_content", EColor.Transparent);
                _popup.OutsideClicked += OnOutsideClicked;

                _gestureLayer = new GestureLayer(_popup);
                _gestureLayer.Attach(_popup);
                _gestureLayer.SetTapCallback(GestureLayer.GestureType.Tap, GestureLayer.GestureState.End, (data) =>
                {
                    if (ContentBound.HasValue)
                    {
                        var contentBound = ContentBound.Value;
                        if (!new Xamarin.Forms.Rectangle(contentBound.X, contentBound.Y, contentBound.Width, contentBound.Height).Contains(data.X, data.Y))
                        {
                            OnOutsideClicked(_popup, EventArgs.Empty);
                        }
                    }
                });
            }
            base.OnElementChanged(e);
            _popup.SetPartContent("default", NativeView);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_popup != null)
                {
                    _popup.Unrealize();
                    _popup = null;
                }
            }
            base.Dispose(disposing);
        }

        private void OnBackButtonPressed(object sender, EventArgs e)
        {
            PopupPage?.SendBackgroundClick();
        }

        private void OnOutsideClicked(object sender, EventArgs e)
        {
            PopupPage?.SendBackgroundClick();
        }

        private void UpdateTitle()
        {
            _popup?.SetPartText("title,text", PopupPage.Title);
        }
    }
}
