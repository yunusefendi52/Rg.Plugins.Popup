using Android.App;
using Android.Content.PM;
using Microsoft.Maui;
using Android.OS;

namespace Demo
{
	[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class MainActivity : MauiAppCompatActivity
	{
		protected override void OnCreate(Bundle? savedInstanceState)
		{
			Android.Util.Log.Info("DemoPopupMaui", "OnCreate Iniiiiit");
			Rg.Plugins.Popup.Popup.Init(this);

			base.OnCreate(savedInstanceState);
		}

		protected override void OnResume()
		{
			base.OnResume();

			Android.Util.Log.Info("DemoPopupMaui", "Resumeeee");
		}
	}
}