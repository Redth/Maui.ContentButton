#if DEBUG
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Widget;
using Google.Android.Material.Card;
using MauiContentButton;
using Microsoft.Maui.Platform;
using AViewGroup = Android.Views.ViewGroup;
using Bitmap = Android.Graphics.Bitmap;
using Canvas = Android.Graphics.Canvas;

namespace Sample;

internal static class RippleSmokeTests
{
	static readonly int[][] States =
	[
		[],
		[Android.Resource.Attribute.StateEnabled],
		[Android.Resource.Attribute.StateEnabled, Android.Resource.Attribute.StatePressed],
		[Android.Resource.Attribute.StateEnabled, Android.Resource.Attribute.StateFocused],
		[Android.Resource.Attribute.StateEnabled, Android.Resource.Attribute.StateHovered]
	];

	internal static async Task RunAsync(ContentButton sampleButton)
	{
		var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
		if (activity?.Intent?.GetBooleanExtra("ripple-smoke-test", false) != true)
			return;

		using var context = new AndroidX.AppCompat.View.ContextThemeWrapper(activity, Resource.Style.RippleTestTheme);
		var mauiContext = new MauiContext(sampleButton.Handler!.MauiContext!.Services, context);
		using var container = new FrameLayout(context);
		var decor = (AViewGroup)activity.Window!.DecorView;
		decor.AddView(container, new AViewGroup.LayoutParams(1, 1));
		var buttons = new[]
		{
			new ContentButton(),
			new ContentButton { BackgroundColor = Colors.Gray, CornerRadius = 12 },
			new ContentButton { Content = new Grid { BackgroundColor = Colors.Gray } },
			new ContentButton
			{
				Background = new LinearGradientBrush
				{
					GradientStops = [new GradientStop(Colors.LightBlue, 0), new GradientStop(Colors.LightPink, 1)]
				}
			}
		};
		var fixedColor = Color.FromArgb("#4488CCFF");
		var fixedButton = new ContentButton { BackgroundColor = Colors.Gray, RippleColor = fixedColor };
		var themedButton = new ContentButton { BackgroundColor = Colors.Gray };
		themedButton.Parent = sampleButton.Parent;
		themedButton.SetAppThemeColor(ContentButton.RippleColorProperty, Colors.Red, Colors.Blue);
		var allButtons = buttons.Concat([fixedButton, themedButton]).ToArray();
		foreach (var button in allButtons)
			container.AddView(button.ToPlatform(mauiContext));

		try
		{
			await Task.Delay(300);
			await VerifyOverrides(buttons, context);
			await VerifyColorComparison((RippleDrawable)((MauiMaterialCardView)buttons[1].Handler!.PlatformView!).Background!);
			var lastMode = UiMode.TypeUndefined;
			var transitions = 0;
			int? firstDefault = null;
			var changedDefault = false;
			for (var i = 0; i < 120; i++)
			{
				await Task.Delay(500);
				var mode = activity.Resources!.Configuration!.UiMode & UiMode.NightMask;
				if (mode == lastMode)
					continue;

				if (lastMode != UiMode.TypeUndefined)
					transitions++;
				lastMode = mode;
				using var freshContext = new AndroidX.AppCompat.View.ContextThemeWrapper(activity, Resource.Style.RippleTestTheme);
				using var fresh = new MaterialCardView(freshContext);
				var expected = fresh.RippleColor!;
				firstDefault ??= expected.DefaultColor;
				changedDefault |= firstDefault != expected.DefaultColor;
				foreach (var button in buttons)
				{
					await AssertColors(button, expected, render: true);
					await VerifyOverrides([button], freshContext);
				}
				if (fixedButton.RippleColor != fixedColor)
					throw new InvalidOperationException("System theme changed the explicit override.");
				using var fixedExpected = ColorStateList.ValueOf(fixedColor.ToPlatform());
				await AssertColors(fixedButton, fixedExpected, render: true);
				var themedColor = mode == UiMode.NightYes ? Colors.Blue : Colors.Red;
				if (themedButton.RippleColor != themedColor)
					throw new InvalidOperationException("AppThemeBinding did not follow the theme.");
				using var themedExpected = ColorStateList.ValueOf(themedColor.ToPlatform());
				await AssertColors(themedButton, themedExpected, render: true);

				// A detached view misses configuration callbacks; attaching it must refresh.
				var native = (MauiMaterialCardView)buttons[0].Handler!.PlatformView!;
				if (transitions > 0)
				{
					container.RemoveView(native);
					native.RippleColor = ColorStateList.ValueOf(Android.Graphics.Color.Magenta);
					container.AddView(native);
					await Task.Delay(100);
					await AssertColors(buttons[0], expected);
				}

				Log.Info("RippleSmoke", $"PASS mode={mode} default={expected.DefaultColor:X8} transitions={transitions}");
				if (transitions == 0)
					Log.Info("RippleSmoke", "READY: switch the emulator system theme; wait for each PASS before switching again");
				if (transitions >= 2 && changedDefault)
				{
					container.RemoveView(native);
					buttons[0].Handler!.DisconnectHandler();
					buttons[0].Handler = null;
					container.AddView(buttons[0].ToPlatform(mauiContext));
					await Task.Delay(100);
					await AssertColors(buttons[0], expected);
					Log.Info("RippleSmoke", "PASS: native/background ripples, overrides, AppThemeBinding, reset, background replacement, reattachment, handler replacement, and both theme directions");
					return;
				}
			}

			throw new InvalidOperationException("Ripple smoke test timed out before two theme transitions with distinct default colors.");
		}
		catch (Exception exception)
		{
			Log.Error("RippleSmoke", exception.ToString());
			throw;
		}
		finally
		{
			foreach (var button in allButtons)
				button.Handler?.DisconnectHandler();
			themedButton.Parent = null;
			decor.RemoveView(container);
		}
	}

	static async Task VerifyOverrides(ContentButton[] buttons, Android.Content.Context context)
	{
		using var fresh = new MaterialCardView(context);
		foreach (var button in buttons)
		{
			var native = (MauiMaterialCardView)button.Handler!.PlatformView!;
			var originalBackground = native.Background;
			var originalContent = native.GetChildAt(0);
			var originalRadius = native.Radius;
			foreach (var color in new[] { Colors.Transparent, Color.FromArgb("#6688CCFF") })
			{
				button.RippleColor = color;
				using var expected = ColorStateList.ValueOf(color.ToPlatform());
				await AssertColors(button, expected);
				if (native.Background?.Handle != originalBackground?.Handle ||
					native.GetChildAt(0)?.Handle != originalContent?.Handle || native.Radius != originalRadius)
					throw new InvalidOperationException("Changing ripple color replaced the background, content, or radius.");
				var background = button.Background;
				if (background is not null)
				{
					button.Background = new SolidColorBrush(Colors.LightGray);
					await AssertColors(button, expected);
					button.Background = background;
					await AssertColors(button, expected);
				}
			}
			button.RippleColor = null;
			await AssertColors(button, fresh.RippleColor!);
			var clicks = 0;
			void OnClicked(object? sender, EventArgs e) => clicks++;
			button.Clicked += OnClicked;
			native.PerformClick();
			button.IsEnabled = false;
			native.PerformClick();
			button.IsEnabled = true;
			button.Clicked -= OnClicked;
			if (clicks != 1)
				throw new InvalidOperationException("Enabled/disabled click behavior changed.");
		}
	}

	static async Task AssertColors(ContentButton button, ColorStateList expected, bool render = false)
	{
		var native = (MauiMaterialCardView)button.Handler!.PlatformView!;
		foreach (var state in States)
		{
			var color = expected.GetColorForState(state, new Android.Graphics.Color(expected.DefaultColor));
			if (native.RippleColor?.GetColorForState(state, new Android.Graphics.Color(native.RippleColor.DefaultColor)) != color)
				throw new InvalidOperationException(
					$"Native ripple mismatch view={native.Handle} for {string.Join(",", state)}: expected={color:X8}, actual={native.RippleColor?.DefaultColor:X8}, override={button.RippleColor}, mode={native.Resources!.Configuration!.UiMode & UiMode.NightMask}.");
		}
		if (!render)
			return;
		if (native.Background is RippleDrawable background)
			await AssertDrawableColors(background, expected);
		await AssertForegroundColors(native.Foreground, expected);
	}

	static async Task AssertForegroundColors(Drawable? drawable, ColorStateList expected)
	{
		switch (drawable)
		{
			case RippleDrawable ripple:
				await AssertDrawableColors(ripple, expected);
				break;
			case InsetDrawable inset:
				await AssertForegroundColors(inset.Drawable, expected);
				break;
			case LayerDrawable layers:
				for (var i = 0; i < layers.NumberOfLayers; i++)
					await AssertForegroundColors(layers.GetDrawable(i), expected);
				break;
		}
	}

	static async Task VerifyColorComparison(RippleDrawable ripple)
	{
		using var incorrect = ColorStateList.ValueOf(Android.Graphics.Color.Magenta);
		try
		{
			await AssertDrawableColors(ripple, incorrect);
		}
		catch (InvalidOperationException exception) when (exception.Message.StartsWith("Ripple rendering mismatch", StringComparison.Ordinal))
		{
			return;
		}
		throw new InvalidOperationException("The color comparison failed to detect a deliberately incorrect ripple color.");
	}

	static async Task AssertDrawableColors(RippleDrawable ripple, ColorStateList expected)
	{
		// RippleDrawable has no color getter. Compare its settled focus feedback with
		// an independent clone recolored to the expected state list. Uniform clone masks
		// isolate color from the original view's shape and layout.
		using var actual = (RippleDrawable)ripple.GetConstantState()!.NewDrawable()!.Mutate();
		using var comparison = (RippleDrawable)ripple.GetConstantState()!.NewDrawable()!.Mutate();
		comparison.SetColor(expected);
		actual.SetDrawableByLayerId(Android.Resource.Id.Mask, new ColorDrawable(Android.Graphics.Color.White));
		comparison.SetDrawableByLayerId(Android.Resource.Id.Mask, new ColorDrawable(Android.Graphics.Color.White));
		actual.SetBounds(0, 0, 80, 40);
		comparison.SetBounds(0, 0, 80, 40);
		actual.SetHotspotBounds(0, 0, 80, 40);
		comparison.SetHotspotBounds(0, 0, 80, 40);
		int[] focused = [Android.Resource.Attribute.StateEnabled, Android.Resource.Attribute.StateFocused, Android.Resource.Attribute.StateWindowFocused];
		actual.SetState(focused);
		comparison.SetState(focused);
		using var actualBitmap = Bitmap.CreateBitmap(80, 40, Bitmap.Config.Argb8888!)!;
		using var expectedBitmap = Bitmap.CreateBitmap(80, 40, Bitmap.Config.Argb8888!)!;
		using var actualCanvas = new Canvas(actualBitmap);
		using var expectedCanvas = new Canvas(expectedBitmap);
		actual.Draw(actualCanvas);
		comparison.Draw(expectedCanvas);
		await Task.Delay(200);
		actualBitmap.EraseColor(Android.Graphics.Color.Transparent);
		expectedBitmap.EraseColor(Android.Graphics.Color.Transparent);
		actual.Draw(actualCanvas);
		comparison.Draw(expectedCanvas);
		if (!actualBitmap.SameAs(expectedBitmap))
			throw new InvalidOperationException("Ripple rendering mismatch.");
	}
}
#endif
