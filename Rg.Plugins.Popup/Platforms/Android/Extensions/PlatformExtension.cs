using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using XFPlatform = Microsoft.Maui.Controls.Compatibility.Platform.Android.Platform;

namespace Rg.Plugins.Popup.Droid.Extensions
{
    internal static class PlatformExtension
    {
        public static IVisualElementRenderer GetOrCreateRenderer(this VisualElement bindable)
        {
            var renderer = XFPlatform.GetRenderer(bindable);
            if (renderer == null)
            {
                renderer = XFPlatform.CreateRendererWithContext(bindable, Popup.Context);
                XFPlatform.SetRenderer(bindable, renderer);
            }
            return renderer;
        }
    }
}