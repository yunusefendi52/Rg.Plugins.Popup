using System;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Microsoft.Maui.Controls.Xaml;

namespace Demo.Pages
{
    public partial class MvvmPage : PopupPage
    {
        public MvvmPage()
        {
            InitializeComponent();
            BindingContext = new MvvmPageViewModel();
        }

        private void OnClose(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }
    }
}
