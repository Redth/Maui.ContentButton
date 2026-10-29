using System.Windows.Input;
using Microsoft.Maui.Controls;
using Xunit;

namespace MauiContentButton.Tests;

public class ContentButtonCommandTests
{
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void CommandAssignmentEvaluatesEligibility(bool canExecute)
	{
		var button = new ContentButton { Command = new TestCommand(canExecute) };

		Assert.Equal(canExecute, button.IsEnabled);
	}

	[Fact]
	public void CommandNotificationsRefreshEnabledState()
	{
		var command = new TestCommand();
		var button = new ContentButton { Command = command };
		var changes = new List<bool>();
		button.PropertyChanged += (_, args) =>
		{
			if (args.PropertyName == nameof(ContentButton.IsEnabled))
				changes.Add(button.IsEnabled);
		};

		command.IsExecutable = false;
		command.NotifyCanExecuteChanged();
		Assert.False(button.IsEnabled);
		command.IsExecutable = true;
		command.NotifyCanExecuteChanged();

		Assert.True(button.IsEnabled);
		Assert.Equal([false, true], changes);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void ParameterChangesRefreshEligibilityRegardlessOfAssignmentOrder(bool parameterFirst)
	{
		var command = new TestCommand { Predicate = parameter => Equals(parameter, "allowed") };
		var button = new ContentButton();
		if (parameterFirst)
			button.CommandParameter = "allowed";
		button.Command = command;
		Assert.Equal(parameterFirst, button.IsEnabled);
		button.CommandParameter = "allowed";
		Assert.True(button.IsEnabled);

		button.CommandParameter = "blocked";
		Assert.False(button.IsEnabled);
		button.CommandParameter = null;
		Assert.False(button.IsEnabled);
		button.CommandParameter = "allowed";
		Assert.True(button.IsEnabled);
	}

	[Fact]
	public void ReplacingAndRemovingCommandUpdatesSubscriptionsAndEligibility()
	{
		var oldCommand = new TestCommand(false);
		var newCommand = new TestCommand();
		var button = new ContentButton { Command = oldCommand };
		Assert.Equal(1, oldCommand.SubscriptionCount);
		Assert.False(button.IsEnabled);

		button.Command = newCommand;
		Assert.Equal(0, oldCommand.SubscriptionCount);
		Assert.Equal(1, newCommand.SubscriptionCount);
		Assert.True(button.IsEnabled);
		var calls = newCommand.CanExecuteCalls;
		oldCommand.NotifyCanExecuteChanged();
		Assert.Equal(calls, newCommand.CanExecuteCalls);

		button.Command = newCommand;
		Assert.Equal(1, newCommand.SubscriptionCount);
		newCommand.IsExecutable = false;
		newCommand.NotifyCanExecuteChanged();
		Assert.False(button.IsEnabled);

		button.Command = null;
		Assert.Equal(0, newCommand.SubscriptionCount);
		Assert.True(button.IsEnabled);
	}

	[Fact]
	public void CommandEligibilityPreservesExplicitDisabling()
	{
		var command = new TestCommand(false);
		var button = new ContentButton { IsEnabled = false, Command = command };

		command.IsExecutable = true;
		command.NotifyCanExecuteChanged();
		Assert.False(button.IsEnabled);
		button.IsEnabled = true;
		Assert.True(button.IsEnabled);

		command.IsExecutable = false;
		command.NotifyCanExecuteChanged();
		button.IsEnabled = true;
		Assert.False(button.IsEnabled);
		button.IsEnabled = false;
		button.Command = null;
		Assert.False(button.IsEnabled);
		button.IsEnabled = true;
		Assert.True(button.IsEnabled);
	}

	[Fact]
	public void CommandEligibilityPreservesEnabledBinding()
	{
		var source = new { IsEnabled = false };
		var command = new TestCommand(false);
		var button = new ContentButton { Command = command, BindingContext = source };
		button.SetBinding(VisualElement.IsEnabledProperty,
			new Binding(nameof(source.IsEnabled), mode: BindingMode.OneWay));

		command.IsExecutable = true;
		command.NotifyCanExecuteChanged();
		Assert.False(button.IsEnabled);
		Assert.False(source.IsEnabled);
		source = new { IsEnabled = true };
		button.BindingContext = source;
		Assert.True(button.IsEnabled);

		command.IsExecutable = false;
		command.NotifyCanExecuteChanged();
		Assert.False(button.IsEnabled);
		Assert.True(source.IsEnabled);
		source = new { IsEnabled = false };
		button.BindingContext = source;
		command.IsExecutable = true;
		command.NotifyCanExecuteChanged();
		Assert.False(button.IsEnabled);
		button.BindingContext = new { IsEnabled = true };
		Assert.True(button.IsEnabled);
	}

	[Fact]
	public void ParentAndCommandMustBothBeEnabled()
	{
		var command = new TestCommand(false);
		var button = new ContentButton { Command = command };
		var parent = new Grid { IsEnabled = false };
		parent.Children.Add(button);

		command.IsExecutable = true;
		command.NotifyCanExecuteChanged();
		Assert.False(button.IsEnabled);
		parent.IsEnabled = true;
		Assert.True(button.IsEnabled);

		command.IsExecutable = false;
		command.NotifyCanExecuteChanged();
		parent.IsEnabled = false;
		parent.IsEnabled = true;
		Assert.False(button.IsEnabled);
		command.IsExecutable = true;
		command.NotifyCanExecuteChanged();
		Assert.True(button.IsEnabled);
	}

	[Fact]
	public void NonExecutableCommandBlocksActivation()
	{
		var command = new TestCommand(false);
		var button = new ContentButton { Command = command };
		var clicks = 0;
		button.Clicked += (_, _) => clicks++;

		((IContentButton)button).Clicked();

		Assert.Equal(0, clicks);
		Assert.Equal(0, command.ExecuteCount);
	}

	[Fact]
	public void BusyCommandDisablesUntilCompletionNotification()
	{
		var command = new TestCommand();
		var button = new ContentButton { Command = command, CommandParameter = "parameter" };
		command.OnExecute = parameter =>
		{
			Assert.Equal("parameter", parameter);
			command.IsExecutable = false;
			command.NotifyCanExecuteChanged();
		};

		((IContentButton)button).Clicked();
		Assert.False(button.IsEnabled);
		((IContentButton)button).Clicked();
		Assert.Equal(1, command.ExecuteCount);

		command.IsExecutable = true;
		command.NotifyCanExecuteChanged();
		Assert.True(button.IsEnabled);
		((IContentButton)button).Clicked();
		Assert.Equal(2, command.ExecuteCount);
	}

	[Fact]
	public void CommandDisablingDuringPressReleasesWithoutActivation()
	{
		var command = new TestCommand();
		var button = new ContentButton { Command = command };
		var input = new ContentButtonInput(button);
		var clicks = 0;
		button.Clicked += (_, _) => clicks++;
		input.Press();
		Assert.True(button.IsPressed);

		command.IsExecutable = false;
		command.NotifyCanExecuteChanged();
		input.Click();

		Assert.False(button.IsPressed);
		Assert.Equal(0, clicks);
		Assert.Equal(0, command.ExecuteCount);
	}

	[Fact]
	public void ButtonWithoutCommandStillRaisesClicked()
	{
		var button = new ContentButton();
		var clicks = 0;
		button.Clicked += (_, _) => clicks++;

		((IContentButton)button).Clicked();

		Assert.True(button.IsEnabled);
		Assert.Equal(1, clicks);
	}

	sealed class TestCommand(bool canExecute = true) : ICommand
	{
		EventHandler? _canExecuteChanged;

		public event EventHandler? CanExecuteChanged
		{
			add => _canExecuteChanged += value;
			remove => _canExecuteChanged -= value;
		}

		public bool IsExecutable { get; set; } = canExecute;
		public Func<object?, bool>? Predicate { get; init; }
		public Action<object?>? OnExecute { get; set; }
		public int ExecuteCount { get; private set; }
		public int CanExecuteCalls { get; private set; }
		public int SubscriptionCount => _canExecuteChanged?.GetInvocationList().Length ?? 0;

		public bool CanExecute(object? parameter)
		{
			CanExecuteCalls++;
			return IsExecutable && (Predicate?.Invoke(parameter) ?? true);
		}

		public void Execute(object? parameter)
		{
			ExecuteCount++;
			OnExecute?.Invoke(parameter);
		}

		public void NotifyCanExecuteChanged() => _canExecuteChanged?.Invoke(this, EventArgs.Empty);
	}
}
