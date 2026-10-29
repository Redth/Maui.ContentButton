using System.Windows.Input;
using Xunit;

namespace MauiContentButton.Tests;

public class ContentButtonInputTests
{
	[Fact]
	public void ActivationReleasesBeforeClickAndExecutesCommandOnce()
	{
		var events = new List<string>();
		var button = new ContentButton();
		button.Pressed += (_, _) => events.Add("pressed");
		button.Released += (_, _) => events.Add("released");
		button.Clicked += (_, _) => events.Add("clicked");
		button.CommandParameter = "parameter";
		button.Command = new CallbackCommand(parameter =>
		{
			Assert.Equal("parameter", parameter);
			events.Add("command");
		});
		var input = new ContentButtonInput(button);

		input.Press();
		input.Press();
		Assert.True(button.IsPressed);
		input.Click();
		input.Release();

		Assert.False(button.IsPressed);
		Assert.Equal(["pressed", "released", "clicked", "command"], events);
	}

	[Fact]
	public void CancellationReleasesWithoutClicking()
	{
		var button = new ContentButton();
		var clicks = 0;
		var releases = 0;
		button.Clicked += (_, _) => clicks++;
		button.Released += (_, _) => releases++;
		var input = new ContentButtonInput(button);

		input.Press();
		input.Release();
		input.Release();

		Assert.False(button.IsPressed);
		Assert.Equal(0, clicks);
		Assert.Equal(1, releases);
	}

	[Fact]
	public void DisabledButtonDoesNotPressOrActivate()
	{
		var button = new ContentButton { IsEnabled = false };
		var events = 0;
		button.Pressed += (_, _) => events++;
		button.Clicked += (_, _) => events++;
		var input = new ContentButtonInput(button);

		input.Press();
		input.Click();

		Assert.False(button.IsPressed);
		Assert.Equal(0, events);
	}

	[Fact]
	public void DisablingDuringPressReleasesWithoutActivation()
	{
		var button = new ContentButton();
		var clicks = 0;
		button.Clicked += (_, _) => clicks++;
		var input = new ContentButtonInput(button);

		input.Press();
		button.IsEnabled = false;
		input.Click();

		Assert.False(button.IsPressed);
		Assert.Equal(0, clicks);
	}

	[Fact]
	public void NativeActivationWithoutPointerStillClicks()
	{
		var button = new ContentButton();
		var clicks = 0;
		button.Clicked += (_, _) => clicks++;

		new ContentButtonInput(button).Click();

		Assert.Equal(1, clicks);
		Assert.False(button.IsPressed);
	}

	sealed class CallbackCommand(Action<object?> execute) : ICommand
	{
		public event EventHandler? CanExecuteChanged { add { } remove { } }
		public bool CanExecute(object? parameter) => true;
		public void Execute(object? parameter) => execute(parameter);
	}
}
