using MauiContentButton;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ContentButtonDesktopSample;

public class DemoApp : Application
{
	protected override Window CreateWindow(IActivationState? activationState)
	{
		var count = 0;
		var status = new Label { Text = "Click the content, or focus the button and press Space." };
		var button = new ContentButton
		{
			AutomationId = "ContentButton",
			Content = new HorizontalStackLayout
			{
				Spacing = 12,
				Children =
				{
					new Label { Text = "+", FontSize = 28 },
					new Label { Text = "Native content button", VerticalOptions = LayoutOptions.Center },
				},
			},
			Padding = new Thickness(16, 10),
			StrokeThickness = 2,
			StrokeColor = Colors.DarkBlue,
			CornerRadius = 12,
			BackgroundColor = Colors.LightBlue,
			HorizontalOptions = LayoutOptions.Start,
		};
		button.Clicked += (_, _) => status.Text = $"Clicked {++count} times";
		var replace = new Button { Text = "Replace content" };
		replace.Clicked += (_, _) => button.Content = new Label { Text = "Replacement content", FontSize = 24 };
		var enabled = new Button { Text = "Toggle enabled" };
		enabled.Clicked += (_, _) => button.IsEnabled = !button.IsEnabled;

		return new Window(new ContentPage
		{
			Title = "ContentButton desktop backends",
			Content = new VerticalStackLayout
			{
				Padding = 24,
				Spacing = 16,
				Children = { button, status, replace, enabled },
			},
		}) { Width = 640, Height = 400 };
	}
}
