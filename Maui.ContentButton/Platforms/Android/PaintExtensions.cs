using System.Reflection;
using Android.Graphics.Drawables;

namespace Maui.ContentButton;

internal static class PaintExtensions
{
    static Type? paintExtensionsType;
    static MethodInfo? applyMethodInfo;

    internal static void ApplyTo(this Paint paint, GradientDrawable gradientDrawable, int height, int width)
    {
        if (applyMethodInfo is null)
        {
            paintExtensionsType ??= typeof(Microsoft.Maui.Graphics.PaintExtensions);

            var methods = paintExtensionsType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);

            foreach (var m in methods)
            {
                if (m.Name != "ApplyTo")
                    continue;

                var p = m.GetParameters();
                if (p.Length != 4)
                    continue;

                if (p[0].ParameterType == typeof(Paint) && p[1].ParameterType == typeof(GradientDrawable)
                    && p[2].ParameterType == typeof(int) && p[3].ParameterType == typeof(int))
                {
                    applyMethodInfo = m;
                    break;
                }
            }
        }

        if (applyMethodInfo != null)
        {
			applyMethodInfo.Invoke(null, new object[] { paint, gradientDrawable, height, width });
		}
    }

}