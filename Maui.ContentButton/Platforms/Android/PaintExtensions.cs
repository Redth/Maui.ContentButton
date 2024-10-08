using System.Reflection;
using Android.Graphics.Drawables;

namespace Maui.ContentButton;

internal static class PaintExtensions
{
    internal static void ApplyTo(this Paint paint, GradientDrawable gradientDrawable, int height, int width)
    {
        var methods = typeof(Microsoft.Maui.Graphics.PaintExtensions).GetMethod("ApplyTo", BindingFlags.Static | BindingFlags.NonPublic);
            
            
        methods.Invoke(null, new object[] { paint, gradientDrawable, height, width });
    }

}