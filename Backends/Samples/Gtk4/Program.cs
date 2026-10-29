using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using MauiContentButton;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Platforms.Linux.Gtk4.Hosting;
using Microsoft.Maui.Platforms.Linux.Gtk4.Platform;

[assembly: SupportedOSPlatform("linux")]

namespace ContentButtonDesktopSample;

class Program : GtkMauiApplication
{
	protected override MauiApp CreateMauiApp() => CreateApp();

	static MauiApp CreateApp() => MauiApp.CreateBuilder()
		.UseMauiAppLinuxGtk4<DemoApp>()
		.AddMauiContentButtonGtk4Handler()
		.Build();

	public static void Main(string[] args)
	{
		if (args.Contains("--smoke-test"))
		{
			Gtk.Module.Initialize();
			using var app = CreateApp();
			var button = SmokeTests.CreateButton();
			var native = (Gtk.Button)button.ToPlatform(new MauiContext(app.Services));
			if (button.Handler is not GtkContentButtonHandler handler)
				throw new InvalidOperationException("The GTK ContentButton handler was not registered.");
			SmokeTests.Run(button, handler,
				() =>
				{
					native.SetStateFlags(Gtk.StateFlags.Active, false);
					g_signal_emit_by_name(native.Handle.DangerousGetHandle(), "clicked");
					native.UnsetStateFlags(Gtk.StateFlags.Active);
				},
				() => native.GetChild()?.GetFirstChild() is null ? 0 : 1,
				() => native.GetSensitive());
			return;
		}
		new Program().Run(args);
	}

	[DllImport("libgobject-2.0.so.0", CallingConvention = CallingConvention.Cdecl)]
	static extern void g_signal_emit_by_name(nint instance, string signal);
}
