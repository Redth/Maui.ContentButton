using Google.Android.Material.Button;
using AColor = Android.Graphics.Color;
using R = Android.Resource;
using MauiContentButton;
using Google.Android.Material.Card;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;

namespace Maui.ContentButton;

internal static class ButtonExtensions
{
	internal static void UpdateButtonStroke(this MaterialCardView platformView, IButtonStroke buttonStroke)
	{
		if (!platformView.UpdateMauiRippleDrawableStroke(buttonStroke))
		{
			// Fall back to the default mechanism. This may be due to the fact that the background
			// is not a "MAUI" background, so we need to update the stroke on the button itself.

			var (width, color, radius) = buttonStroke.GetStrokeProperties(platformView.Context!, true);
			
			platformView.SetStrokeColor(color);

			platformView.StrokeWidth = width;

			platformView.Radius = radius;
		}
	}

	internal static void UpdateButtonBackground(this MaterialCardView platformView, IContentButton button)
	{
		platformView.UpdateMauiRippleDrawableBackground(
			button.Background,
			button,
			() =>
			{
				// Copy the tints from a temporary button.
				// TODO: optimize this to avoid creating a new button every time.

				var context = platformView.Context!;
				using var btn = new MaterialButton(context);
				var defaultTintList = btn.BackgroundTintList;
				var defaultColor = defaultTintList?.GetColorForState([R.Attribute.StateEnabled], AColor.Black);

				return defaultColor ?? AColor.Black;
			},
			() =>
			{
				// If some theme or user value has been set, we can override the default, white
				// ripple color using this button property.
				return platformView.RippleColor;
			},
			() =>
			{
				// We have a background, so we need to null out the tint list to avoid the tint
				// overriding the background.
				platformView.BackgroundTintList = null;
			});
	}
}