using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml;
using WVerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;
using WHorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;

namespace MauiContentButton;

public partial class ContentButtonHandler : ViewHandler<IContentButton, MauiContentButton>
{
    PointerEventHandler? _pointerPressedHandler;
    PointerEventHandler? _pointerReleasedHandler;

    protected override MauiContentButton CreatePlatformView()
    {
        if (VirtualView == null)
        {
            throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a LayoutView");
        }

        var button = new MauiContentButton()
        {
				VerticalAlignment = WVerticalAlignment.Stretch,
				HorizontalAlignment = WHorizontalAlignment.Stretch,
				Content = new MauiContentButtonContent
            {
                //CrossPlatformLayout = VirtualView
            }
			};

        return button;
    }

    protected override void ConnectHandler(MauiContentButton platformView)
    {
        _pointerPressedHandler = new PointerEventHandler(OnPointerPressed);
        _pointerReleasedHandler = new PointerEventHandler(OnPointerReleased);

        platformView.Click += OnClick;
        platformView.AddHandler(UIElement.PointerPressedEvent, _pointerPressedHandler, true);
        platformView.AddHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler, true);

        base.ConnectHandler(platformView);
    }


    protected override void DisconnectHandler(MauiContentButton platformView)
    {
        if (platformView.Content is ContentPanel buttonContentPanel)
        {
            buttonContentPanel.CrossPlatformLayout = null;
            buttonContentPanel.Children?.Clear();
        }

        platformView.Click -= OnClick;
        platformView.RemoveHandler(UIElement.PointerPressedEvent, _pointerPressedHandler);
        platformView.RemoveHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler);

        _pointerPressedHandler = null;
        _pointerReleasedHandler = null;

        base.DisconnectHandler(platformView);
    }

    public static void MapContent(IContentButtonHandler handler, IContentButton view)
    {
			_ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");


			if (handler.PlatformView.Content is MauiContentButtonContent buttonContentPanel)
			{
            buttonContentPanel.Children.Clear();

            if (handler.VirtualView.PresentedContent is IView presentedView)
            {
                var plat = presentedView.ToPlatform(handler.MauiContext);
                buttonContentPanel.Children.Add(plat);
            }
			}
		}

    // This is a Windows-specific mapping
    public static void MapBackground(IContentButtonHandler handler, IContentButton button)
    {
        handler.PlatformView.UpdateContentButtonBackground(button);
    }		

		public static void MapStrokeColor(IContentButtonHandler handler, IButtonStroke buttonStroke)
    {
        handler.PlatformView?.UpdateStrokeColor(buttonStroke);
    }

    public static void MapStrokeThickness(IContentButtonHandler handler, IButtonStroke buttonStroke)
    {
        handler.PlatformView?.UpdateStrokeThickness(buttonStroke);
    }

    public static void MapCornerRadius(IContentButtonHandler handler, IButtonStroke buttonStroke)
    {
        handler.PlatformView?.UpdateCornerRadius(buttonStroke);
    }

    public static void MapPadding(IContentButtonHandler handler, IPadding padding)
    {
        handler.PlatformView?.UpdatePadding(padding);
    }

    void OnClick(object sender, RoutedEventArgs e)
    {
        VirtualView?.Clicked();
    }

    void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        VirtualView?.Pressed();
    }

    void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        VirtualView?.Released();
    }
}
