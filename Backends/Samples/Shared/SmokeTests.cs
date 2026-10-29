using MauiContentButton;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ContentButtonDesktopSample;

static class SmokeTests
{
	public static ContentButton CreateButton() => new()
	{
		Content = new Label { Text = "Native content" },
		Padding = new Thickness(8, 6),
		StrokeThickness = 2,
		StrokeColor = Colors.DarkBlue,
		CornerRadius = 8,
		BackgroundColor = Colors.LightBlue,
	};

	public static void Run(ContentButton button, IViewHandler handler, Action activate, Func<int> childCount)
	{
		Require(handler is IContentButtonHandler contentHandler && contentHandler.VirtualView == button,
			"Desktop handlers must expose the shared ContentButton handler contract.");
		var events = new List<string>();
		button.Pressed += (_, _) => events.Add("pressed");
		button.Released += (_, _) => events.Add("released");
		button.Clicked += (_, _) => events.Add("clicked");
		button.Command = new Command(() => events.Add("command"));

		Require(childCount() == 1, "The native button must host the content.");
		var size = handler.GetDesiredSize(200, 100);
		Require(size.Width > 20 && size.Height > 16, "Nested content must contribute to measurement.");
		handler.PlatformArrange(new Rect(0, 0, 200, 100));
		Require(button.Content.Bounds.Width > 0, "Nested content must be arranged.");
		activate();
		Require(events.SequenceEqual(["pressed", "released", "clicked", "command"]),
			"Native activation must press, release, click and execute the command once, in order.");
		Require(!button.IsPressed, "Activation must leave the button released.");

		events.Clear();
		button.IsEnabled = false;
		activate();
		Require(events.Count == 0, "Disabled native buttons must not activate.");
		button.IsEnabled = true;

		var previous = button.Content;
		button.Content = new Label { Text = "Replacement" };
		Require(childCount() == 1, "Replacing content must remove the old native child.");
		Require(previous.Handler?.PlatformView != button.Content.Handler?.PlatformView, "Replacement must use a new native child.");
		button.Content = null!;
		Require(childCount() == 0, "Null content must clear the native child.");

		button.Background = new LinearGradientBrush(new GradientStopCollection
		{
			new GradientStop(Colors.Red, 0),
			new GradientStop(Colors.Blue, 1),
		}, new Point(0, 0), new Point(1, 1));
		button.Background = null;
		button.CornerRadius = -1;
		button.StrokeThickness = -1;
		button.StrokeColor = null!;
		button.Padding = 0;
		var emptySize = handler.GetDesiredSize(200, 100);
		Require(emptySize == Size.Zero, "Reset styling must restore an empty, zero-inset measurement.");

		var rebound = CreateButton();
		var reboundClicks = 0;
		rebound.Clicked += (_, _) => reboundClicks++;
		events.Clear();
		handler.SetVirtualView(rebound);
		activate();
		Require(reboundClicks == 1 && events.Count == 0, "Reusing a handler must activate only its current virtual view.");

		handler.DisconnectHandler();
		events.Clear();
		activate();
		Require(events.Count == 0 && reboundClicks == 1, "Disconnect must remove native event subscriptions.");
		Console.WriteLine("Native ContentButton smoke tests passed.");
	}

	static void Require(bool condition, string message)
	{
		if (!condition)
			throw new InvalidOperationException(message);
	}
}
