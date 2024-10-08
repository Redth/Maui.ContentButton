using Android.Content;
using Android.Content.Res;
using AView = Android.Views.View;
using System.Reflection;
using Microsoft.Maui;

namespace Maui.ContentButton;

internal static class MauiRippleDrawableExtensions
{
	static readonly Assembly parentAssembly = typeof(Microsoft.Maui.Platform.ButtonExtensions).Assembly;

	static Type? mauiRippleDrawableExtensionsType;

	static Type? MauiRippleDrawableExtensionsType
		=> mauiRippleDrawableExtensionsType ??= parentAssembly.GetType("Microsoft.Maui.Platform.MauiRippleDrawableExtensions");

	static MethodInfo? updateMauiRippleDrawableStrokeMethod;

	internal static bool UpdateMauiRippleDrawableStroke(this AView platformView, IButtonStroke buttonStroke)
	{
		updateMauiRippleDrawableStrokeMethod 
			??= MauiRippleDrawableExtensionsType?.GetMethod(nameof(UpdateMauiRippleDrawableStroke), BindingFlags.Static | BindingFlags.NonPublic);

		if (updateMauiRippleDrawableStrokeMethod is not null)
		{
			var r = updateMauiRippleDrawableStrokeMethod?.Invoke(null, [platformView, buttonStroke]);
			if (r is not null)
				return (bool)r;
		}

		return false;
	}


	static MethodInfo? updateMauiRippleDrawableBackgroundMethod;

	internal static void UpdateMauiRippleDrawableBackground(this AView platformView,
		Paint? background,
		IButtonStroke stroke,
		Func<int?>? getEmptyBackgroundColor = null,
		Func<ColorStateList?>? getDefaultRippleColor = null,
		Action? beforeSet = null)
	{
		updateMauiRippleDrawableBackgroundMethod
			??= MauiRippleDrawableExtensionsType?.GetMethod(nameof(UpdateMauiRippleDrawableBackground), BindingFlags.Static | BindingFlags.NonPublic);

		updateMauiRippleDrawableBackgroundMethod?.Invoke(null, [platformView, background, stroke, getEmptyBackgroundColor, getDefaultRippleColor, beforeSet]);
	}


	static MethodInfo? getStrokePropertiesMethod;

	internal static (int Thickness, ColorStateList Color, int Radius) GetStrokeProperties(this IButtonStroke button, Context context, bool defaultButtonLogic)
	{
		getStrokePropertiesMethod
			??= MauiRippleDrawableExtensionsType?.GetMethod(nameof(GetStrokeProperties), BindingFlags.Static | BindingFlags.NonPublic);

		if (getStrokePropertiesMethod is not null)
		{
			var r = getStrokePropertiesMethod.Invoke(null, [button, context, defaultButtonLogic]);
			if (r is not null)
				return ((int Thickness, ColorStateList Color, int Radius))r;
		}
		return default;
	}
}