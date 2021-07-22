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

[assembly: XamlCompilationAttribute(XamlCompilationOptions.Compile)]

namespace Demo
{
    public class Startup : IStartup
    {
        public void Configure(IAppHostBuilder appBuilder)
        {
            appBuilder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                })
                .ConfigureLifecycleEvents(lifecycle =>
                {
					Android.Util.Log.Info("DemoPopupMaui", "lifecycle init");

					lifecycle.AddAndroid(d =>
					{
						d.OnCreate((a,b) =>
						{
							Android.Util.Log.Info("DemoPopupMaui", "OnCreate startup init");
							// Rg.Plugins.Popup.Popup.Init(a);
						});
					});
                })
				.ConfigureMauiHandlers(handlers =>
            {
				Android.Util.Log.Info("DemoPopupMaui", "AddCompatibilityRenderer");
				handlers.AddCompatibilityRenderers(Microsoft.Maui.Controls.Device.GetAssemblies());
                handlers.AddCompatibilityRenderer(typeof(PopupPage), typeof(PopupPageRenderer));
            });
        }
    }
}