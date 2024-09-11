#if IOS || MACCATALYST
using System.Reflection;
using Microsoft.Maui.Platform;

namespace Maui.Extras
{
    public static class WrapperViewExtensions
	{
		static PropertyInfo? wrapperViewCrossPlatformMeasureProperty;
		static PropertyInfo WrapperViewCrossPlatformMeasureProperty
			=> wrapperViewCrossPlatformMeasureProperty
				??= typeof(WrapperView).GetProperty("CrossPlatformLayout",
					BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!;

		public static ICrossPlatformLayout? GetCrossPlatformLayout(this WrapperView wrapperView)
			=> WrapperViewCrossPlatformMeasureProperty.GetValue(wrapperView) as ICrossPlatformLayout;

		public static void SetCrossPlatformLayout(this WrapperView wrapperView, ICrossPlatformLayout value)
			=> WrapperViewCrossPlatformMeasureProperty.SetValue(wrapperView, value);
	}
}

#endif