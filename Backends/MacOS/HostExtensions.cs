using Microsoft.Maui.Hosting;

namespace MauiContentButton;

public static class MacOSContentButtonHostExtensions
{
	public static IMauiHandlersCollection AddMauiContentButtonMacOSHandler(this IMauiHandlersCollection handlers)
	{
		handlers.AddHandler<ContentButton, MacOSContentButtonHandler>();
		return handlers;
	}

	public static MauiAppBuilder AddMauiContentButtonMacOSHandler(this MauiAppBuilder builder)
	{
		builder.ConfigureMauiHandlers(handlers => handlers.AddMauiContentButtonMacOSHandler());
		return builder;
	}
}
