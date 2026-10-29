using AppKit;
using Foundation;
using MauiContentButton;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platforms.MacOS.Hosting;
using Microsoft.Maui.Platforms.MacOS.Platform;

namespace ContentButtonDesktopSample;

static class Program
{
	internal static MauiApp CreateApp() => MauiApp.CreateBuilder()
		.UseMauiAppMacOS<DemoApp>()
		.AddMauiContentButtonMacOSHandler()
		.Build();

	static void Main(string[] args)
	{
		NSApplication.Init();
		var smokeTest = args.Contains("--smoke-test");
		using var timeout = smokeTest ? new System.Threading.Timer(_ =>
		{
			Console.Error.WriteLine("AppKit sample startup timed out.");
			Environment.Exit(1);
		}, null, TimeSpan.FromSeconds(30), Timeout.InfiniteTimeSpan) : null;
		using var appDelegate = new SampleAppDelegate(smokeTest);
		NSApplication.SharedApplication.Delegate = appDelegate;
		NSApplication.Main(args);
	}
}

[Register("ContentButtonSampleAppDelegate")]
public class SampleAppDelegate : MacOSMauiApplication
{
	readonly bool _smokeTest;

	public SampleAppDelegate() : this(false)
	{
	}

	public SampleAppDelegate(bool smokeTest) => _smokeTest = smokeTest;

	protected override MauiApp CreateMauiApp() => Program.CreateApp();

	public override void DidFinishLaunching(NSNotification notification)
	{
		try
		{
			base.DidFinishLaunching(notification);
		}
		catch (Exception exception)
		{
			Console.Error.WriteLine($"AppKit sample startup failed: {exception}");
			Environment.Exit(1);
		}
	}

	protected override void OnStarted()
	{
		base.OnStarted();
		if (!_smokeTest)
			return;

		if (Windows.Count != 1 || Windows[0].Handler?.PlatformView is not NSWindow)
			throw new InvalidOperationException("The sample must create a native AppKit window.");
		var button = SmokeTests.CreateButton();
		var native = (MauiAppKitContentButton)button.ToMacOSPlatform(new MauiContext(Services));
		if (button.Handler is not MacOSContentButtonHandler handler)
			throw new InvalidOperationException("The AppKit ContentButton handler was not registered.");
		SmokeTests.Run(button, handler, () => native.PerformClick(native), () => native.Subviews.Length, () => native.Enabled);
		Console.WriteLine("AppKit application startup smoke test passed.");
		Environment.Exit(0);
	}
}
