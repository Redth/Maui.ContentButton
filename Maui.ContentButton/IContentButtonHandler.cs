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

public interface IContentButtonHandler : IViewHandler
{
	new PlatformButtonView PlatformView { get; }
	new IContentButton VirtualView { get; }
}
