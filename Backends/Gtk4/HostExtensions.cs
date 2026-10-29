using Microsoft.Maui.Hosting;

namespace MauiContentButton;

public static class GtkContentButtonHostExtensions
{
	public static IMauiHandlersCollection AddMauiContentButtonGtk4Handler(this IMauiHandlersCollection handlers)
	{
		handlers.AddHandler<ContentButton, GtkContentButtonHandler>();
		return handlers;
	}

	public static MauiAppBuilder AddMauiContentButtonGtk4Handler(this MauiAppBuilder builder)
	{
		builder.ConfigureMauiHandlers(handlers => handlers.AddMauiContentButtonGtk4Handler());
		return builder;
	}
}
