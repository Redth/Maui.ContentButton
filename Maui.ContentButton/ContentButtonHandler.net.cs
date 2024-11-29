using Microsoft.Maui.Handlers;

namespace MauiContentButton;

#if !MACCATALYST && !IOS && !ANDROID && !WINDOWS
public partial class ContentButtonHandler : ViewHandler<IContentButton, object>
{
    protected override object CreatePlatformView()
    {
        throw new NotImplementedException();
    }
	
    public static void MapContent(IContentButtonHandler handler, IContentButton view)
    {
        throw new NotImplementedException();
    }

    public static void MapBackground(IContentButtonHandler handler, IContentButton view)
    {
        throw new NotImplementedException();
    }

    public static void MapStrokeColor(IContentButtonHandler handler, IButtonStroke buttonStroke)
    {
        throw new NotImplementedException();
    }

    public static void MapStrokeThickness(IContentButtonHandler handler, IButtonStroke buttonStroke)
    {
        throw new NotImplementedException();
    }

    public static void MapCornerRadius(IContentButtonHandler handler, IButtonStroke buttonStroke)
    {
        throw new NotImplementedException();
    }

    public static void MapPadding(IContentButtonHandler handler, IPadding padding)
    {
        throw new NotImplementedException();
    }
}
#endif
