using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.LifecycleEvents;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Rg.Plugins.Popup.Pages;
using PopupPageRenderer = Rg.Plugins.Popup.Droid.Renderers.PopupPageRenderer;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls;

[assembly: XamlCompilationAttribute(XamlCompilationOptions.Compile)]

namespace Demo
{
    public class MauiProgram
    {
        public static Context? AppContext;

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                })
                .ConfigureLifecycleEvents(lifecycle =>
                {
                    lifecycle.AddAndroid(d =>
                    {
                        d.OnCreate((a, b) =>
                        {
                            AppContext = a;
                        });
                    });
                })
                .ConfigureMauiHandlers(handlers =>
                {
                    handlers.AddCompatibilityRenderer(typeof(PopupPage), typeof(PopupPageRenderer));
                });
            return builder.Build();
        }
    }
}