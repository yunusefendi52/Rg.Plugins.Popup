using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using XFPlatform = Microsoft.Maui.Controls.Compatibility.Platform.Android.Platform;
using Android.Content;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Compatibility;

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
                // renderer = CreateRenderer(bindable, Popup.Context);
                // XFPlatform.SetRenderer(bindable, renderer);
            }
            return renderer;
        }

        static IVisualElementRenderer CreateRenderer(
            VisualElement element,
            Context context,
            AndroidX.Fragment.App.FragmentManager fragmentManager = null,
            global::Android.Views.LayoutInflater layoutInflater = null)
        {
            IVisualElementRenderer renderer = null;

            // This code is duplicated across all platforms currently
            // So if any changes are made here please make sure to apply them to other platform.cs files
            if (renderer == null)
            {
                IViewHandler handler = null;

                //TODO: Handle this with AppBuilderHost
                // try
                // {
                //     var mauiContext = Forms.MauiContext;

                //     // if (fragmentManager != null || layoutInflater != null)
                //     //     mauiContext = new ScopedMauiContext(mauiContext, null, null, layoutInflater, fragmentManager);

                //     handler = mauiContext.Handlers.GetHandler(element.GetType()) as IViewHandler;
                //     handler.SetMauiContext(mauiContext);
                // }
                // catch
                // {
                //     // TODO define better catch response or define if this is needed?
                // }

                if (handler == null)
                {
                    renderer = Registrar.Registered.GetHandlerForObject<IVisualElementRenderer>(element, context);
                }
                // This means the only thing registered is the RendererToHandlerShim
                // Which is only used when you are running a .NET MAUI app
                // This indicates that the user hasn't registered a specific handler for this given type
                else if (handler is RendererToHandlerShim shim)
                {
#if __ANDROID__
                    renderer = null;
#endif

                    if (renderer == null)
                    {
                        renderer = Registrar.Registered.GetHandlerForObject<IVisualElementRenderer>(element, context);
                    }
                }
                else if (handler is IVisualElementRenderer ver)
                    renderer = ver;
                else if (handler is INativeViewHandler vh)
                {
                    renderer = new HandlerToRendererShim(vh);
                    element.Handler = handler;
                    XFPlatform.SetRenderer(element, renderer);
                }
            }

            renderer.SetElement(element);

            if (fragmentManager != null)
            {
                // var managesFragments = renderer as IManageFragments;
                // managesFragments?.SetFragmentManager(fragmentManager);
            }

            return renderer;
        }
    }
}