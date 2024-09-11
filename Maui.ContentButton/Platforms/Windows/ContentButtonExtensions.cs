using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using WButton = Microsoft.UI.Xaml.Controls.Button;

namespace MauiContentButton;

public static class ContentButtonExtensions
{
	// The built in extension method expects IButton to apply the correct resource key changes
	// however we don't use IButton here to avoid the Text and Image related properties
	// So we need to manually do this update:

	public static void UpdateContentButtonBackground(this WButton platformButton, IContentButton button)
	{
		var brush = button.Background?.ToPlatform();

		if (brush is null)
			platformButton.Resources.RemoveKeys(BackgroundResourceKeys);
		else
			platformButton.Resources.SetValueForAllKey(BackgroundResourceKeys, brush);

		RefreshThemeResources(platformButton);
	}

	internal static void RefreshThemeResources(FrameworkElement nativeView)
	{
		var previous = nativeView.RequestedTheme;

		// Workaround for https://github.com/dotnet/maui/issues/7820
		nativeView.RequestedTheme = nativeView.ActualTheme switch
		{
			ElementTheme.Dark => ElementTheme.Light,
			_ => ElementTheme.Dark
		};

		nativeView.RequestedTheme = previous;
	}

	static readonly string[] BackgroundResourceKeys =
	{
			"ButtonBackground",
			"ButtonBackgroundPointerOver",
			"ButtonBackgroundPressed",
			"ButtonBackgroundDisabled",
		};

	internal static void RemoveKeys(this Microsoft.UI.Xaml.ResourceDictionary resources, IEnumerable<string> keys)
	{
		foreach (string key in keys)
		{
			resources.Remove(key);
		}
	}

	internal static void SetValueForAllKey(this Microsoft.UI.Xaml.ResourceDictionary resources, IEnumerable<string> keys, object? value)
	{
		foreach (string key in keys)
		{
			resources[key] = value;
		}
	}

}
