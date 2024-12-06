#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
using Microsoft.Maui.Handlers;
using PlatformButtonView = object;

namespace MauiContentButton;

// All the code in this file is included in all platforms.
public partial class ContentButtonHandler : ViewHandler<IContentButton, PlatformButtonView>
{
	protected override PlatformButtonView CreatePlatformView() => new();

	private static void MapPadding(IContentButtonHandler handler, IContentButton view) { }

	private static void MapStrokeThickness(IContentButtonHandler handler, IContentButton view) { }

	private static void MapStrokeColor(IContentButtonHandler handler, IContentButton view) { }

	private static void MapCornerRadius(IContentButtonHandler handler, IContentButton view) { }

	private static void MapContent(IContentButtonHandler handler, IContentButton view) { }
}
#endif
