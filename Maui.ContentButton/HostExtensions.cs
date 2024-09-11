using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiContentButton;

public static class MauiContentButtonHostExtensions
{

	public static IMauiHandlersCollection AddMauiContentButtonHandler(this IMauiHandlersCollection handlersCollection)
	{
		handlersCollection.AddHandler<ContentButton, ContentButtonHandler>();

		return handlersCollection;
	}

	public static MauiAppBuilder AddMauiContentButtonHandler(this MauiAppBuilder builder)
	{
		builder.ConfigureMauiHandlers(handlers => handlers.AddMauiContentButtonHandler());

		return builder;
	}
}
