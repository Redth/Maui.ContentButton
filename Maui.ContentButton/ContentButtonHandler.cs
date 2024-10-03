using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;


#if WINDOWS
using PlatformButtonView = Microsoft.Maui.Platform.MauiButton;
#elif ANDROID
using PlatformButtonView = MauiContentButton.MauiMaterialCardView;
#elif IOS || MACCATALYST
using PlatformButtonView = UIKit.UIButton;
#else
using PlatformButtonView = object;
#endif

namespace MauiContentButton;


// All the code in this file is included in all platforms.
public partial class ContentButtonHandler : IContentButtonHandler
{
	public static IPropertyMapper<IContentButton, IContentButtonHandler> Mapper = new PropertyMapper<IContentButton, IContentButtonHandler>(ViewMapper, ViewHandler.ViewMapper)
	{
		[nameof(IPadding.Padding)] = MapPadding,
		[nameof(IButtonStroke.StrokeThickness)] = MapStrokeThickness,
		[nameof(IButtonStroke.StrokeColor)] = MapStrokeColor,
		[nameof(IButtonStroke.CornerRadius)] = MapCornerRadius,
		[nameof(IContentButton.Content)] = MapContent,

#if ANDROID || MACCATALYST || WINDOWS || IOS
		[nameof(IContentButton.Background)] = MapBackground,
#endif
	};

	public static CommandMapper<IButton, IButtonHandler> CommandMapper = new(ViewCommandMapper);

	public ContentButtonHandler()
		: base(Mapper, CommandMapper)
	{

	}

	public ContentButtonHandler(IPropertyMapper? mapper)
		: base(mapper ?? Mapper, CommandMapper)
	{
	}

	public ContentButtonHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
	{
	}

	IContentButton IContentButtonHandler.VirtualView => VirtualView;

	PlatformButtonView IContentButtonHandler.PlatformView => PlatformView;
}
