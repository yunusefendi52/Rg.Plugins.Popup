using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui;

namespace Rg.Plugins.Popup.Animations.Base
{
    public abstract class FadeBackgroundAnimation : BaseAnimation
    {
        private Color _backgroundColor;

        public bool HasBackgroundAnimation { get; set; } = true;

        public override void Preparing(View content, PopupPage page)
        {
            if (HasBackgroundAnimation && page.BackgroundImageSource == null)
            {
                _backgroundColor = page.BackgroundColor;
                page.BackgroundColor = GetColor(0);
            }
        }

        public override void Disposing(View content, PopupPage page)
        {
            if (HasBackgroundAnimation && page.BackgroundImageSource == null)
            {
                page.BackgroundColor = _backgroundColor;
            }
        }

        public override Task Appearing(View content, PopupPage page)
        {
            if (HasBackgroundAnimation && page.BackgroundImageSource == null)
            {
                TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
                _backgroundColor.ToRgba(out var r, out var g, out var b, out var a);
                page.Animate("backgroundFade", d =>
                {
                    page.BackgroundColor = GetColor(d);
                }, 0, a, length: DurationIn, finished: (d, b) =>
                {
                    task.SetResult(true);
                });

                return task.Task;
            }

            return Task.FromResult(0);
        }

        public override Task Disappearing(View content, PopupPage page)
        {
            if (HasBackgroundAnimation && page.BackgroundImageSource == null)
            {
                TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();

                _backgroundColor = page.BackgroundColor;

                _backgroundColor.ToRgba(out var r, out var g, out var b, out var a);
                page.Animate("backgroundFade", d =>
                {
                    page.BackgroundColor = GetColor(d);
                }, a, 0, length: DurationOut, finished: (d, b) =>
                {
                    task.SetResult(true);
                });

                return task.Task;
            }

            return Task.FromResult(0);
        }

        private Color GetColor(double transparent)
        {
            return _backgroundColor.WithAlpha((float)transparent);
        }
    }
}
