﻿using System;
using System.Diagnostics;
using Rg.Plugins.Popup.Services;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Demo.Pages
{
    public partial class MainPage : ContentPage
    {
        private LoginPopupPage _loginPopup;

        public MainPage()
        {
            InitializeComponent();

            PopupNavigation.Instance.Pushing += (sender, e) => Debug.WriteLine($"[Popup] Pushing: {e.Page.GetType().Name}");
            PopupNavigation.Instance.Pushed += (sender, e) => Debug.WriteLine($"[Popup] Pushed: {e.Page.GetType().Name}");
            PopupNavigation.Instance.Popping += (sender, e) => Debug.WriteLine($"[Popup] Popping: {e.Page.GetType().Name}");
            PopupNavigation.Instance.Popped += (sender, e) => Debug.WriteLine($"[Popup] Popped: {e.Page.GetType().Name}");

            _loginPopup = new LoginPopupPage();
        }

        private async void OnOpenPupup(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(_loginPopup);
        }

        private async void OnUserAnimationPupup(object sender, EventArgs e)
        {
            var page = new UserAnimationPage();

            await PopupNavigation.Instance.PushAsync(page);
        }

        private async void OnOpenSystemOffsetPage(object sender, EventArgs e)
        {
            var page = new SystemOffsetPage();

            await PopupNavigation.Instance.PushAsync(page);
        }

        private async void OnOpenListViewPage(object sender, EventArgs e)
        {
            var page = new ListViewPage();

            await PopupNavigation.Instance.PushAsync(page);
        }

        private async void OnOpenUserAnimationFromResource(object sender, EventArgs e)
        {
            var page = new UserAnimationFromResourcePage();

            await PopupNavigation.Instance.PushAsync(page);
        }

        private async void OnOpenUserAnimationFromStyle(object sender, EventArgs e)
        {
            var page = new UserAnimationFromStylePage();

            await PopupNavigation.Instance.PushAsync(page);
        }

        private async void OnOpenSettingsPage(object sender, EventArgs e)
        {
            var page = new SettingsPage();

            await PopupNavigation.Instance.PushAsync(page);
        }
        
        private async void OnOpenMvvmPage(object sender, EventArgs e)
        {
            var page = new MvvmPage();
            
            await PopupNavigation.Instance.PushAsync(page);
        }

        private async void OnTestCurrentViewController(object sender, EventArgs e)
        {
            var page = new TestCurrentViewController(0);

            await PopupNavigation.Instance.PushAsync(page);
        }
    }
}
