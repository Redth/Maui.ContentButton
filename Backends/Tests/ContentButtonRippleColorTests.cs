using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui;
using Xunit;

namespace MauiContentButton.Tests;

public class ContentButtonRippleColorTests
{
	[Fact]
	public void DefaultUsesNativeTheme()
	{
		var button = new ContentButton();

		Assert.Null(button.RippleColor);
		Assert.Null(((IContentButton)button).RippleColor);
		Assert.Null(ContentButton.RippleColorProperty.DefaultValue);
		Assert.Equal(typeof(ContentButton), ContentButton.RippleColorProperty.DeclaringType);
	}

	[Fact]
	public void AssignmentPreservesAlphaAndNotifiesOnce()
	{
		var button = new ContentButton();
		var notifications = 0;
		button.PropertyChanged += (_, args) =>
		{
			if (args.PropertyName == nameof(ContentButton.RippleColor))
				notifications++;
		};
		var color = new Color(0.2f, 0.4f, 0.6f, 0.25f);

		button.RippleColor = color;
		button.RippleColor = color;

		Assert.Equal(color, button.RippleColor);
		Assert.Equal(color, ((IContentButton)button).RippleColor);
		Assert.Equal(0.25f, button.RippleColor.Alpha);
		Assert.Equal(1, notifications);
	}

	[Fact]
	public void TransparentIsAnOverrideAndClearingRestoresDefault()
	{
		var button = new ContentButton { RippleColor = Colors.Transparent };

		Assert.Equal(Colors.Transparent, button.RippleColor);
		button.ClearValue(ContentButton.RippleColorProperty);
		Assert.Null(button.RippleColor);
		button.RippleColor = Colors.Red;
		button.RippleColor = null;
		Assert.Null(button.RippleColor);
	}

	[Fact]
	public void BindingResolvesColorsAndNull()
	{
		var target = new ContentButton { BindingContext = new { RippleColor = Colors.Red } };
		target.SetBinding(ContentButton.RippleColorProperty, nameof(ContentButton.RippleColor));

		Assert.Equal(Colors.Red, target.RippleColor);
		target.BindingContext = new { RippleColor = Colors.Transparent };
		Assert.Equal(Colors.Transparent, target.RippleColor);
		target.BindingContext = new { RippleColor = (Color?)null };
		Assert.Null(target.RippleColor);
	}

	[Fact]
	public void DynamicResourceTracksReplacements()
	{
		var button = new ContentButton();
		button.Resources["Ripple"] = Colors.Red;
		button.SetDynamicResource(ContentButton.RippleColorProperty, "Ripple");

		Assert.Equal(Colors.Red, button.RippleColor);
		button.Resources["Ripple"] = Colors.Blue;
		Assert.Equal(Colors.Blue, button.RippleColor);
	}

	[Fact]
	public void ExistingInterfaceImplementationsNeedNoNewMember()
	{
		IContentButton button = new LegacyContentButton();

		Assert.Null(button.RippleColor);
	}

	sealed class LegacyContentButton : ContentView, IContentButton
	{
		public Color StrokeColor => Colors.Black;
		public double StrokeThickness => 0;
		public int CornerRadius => 0;
		object? IContentButton.Content => Content;
		IView? IContentButton.PresentedContent => Content;
		public void Pressed() { }
		public void Released() { }
		public void Clicked() { }
	}
}
