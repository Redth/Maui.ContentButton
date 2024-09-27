#if IOS || MACCATALYST
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
using MButton = UIKit.UIButton;

namespace MauiContentButton;

public partial class ContentButtonHandler : ViewHandler<IContentButton, MButton>
{
	static readonly UIControlState[] ControlStates = { UIControlState.Normal, UIControlState.Highlighted, UIControlState.Disabled };

	//public readonly static Thickness DefaultPadding = new Thickness(12, 7);

	public const int ContentButtonHandlerContentViewTag = 23123;

	protected override UIButton CreatePlatformView()
	{
		var button = new MauiAppleButton {
			CrossPlatformLayout = VirtualView,
			ClipsToBounds = true,
			Configuration = UIButtonConfiguration.BorderedButtonConfiguration,
			TouchesHandlers = new MauiAppleButtonTouches(VirtualView.Pressed, VirtualView.Released, VirtualView.Clicked)
		};

		SetControlPropertiesFromProxy(button);

		return button;
	}

	public static void MapBackground(IContentButtonHandler handler, IContentButton button)
	{
		if (handler.PlatformView.Configuration is not null)
		{
			var con = handler.PlatformView.Configuration;
			con.Background.BackgroundColor = button.Background?.ToColor()?.ToPlatform();
			handler.PlatformView.Configuration = con;
		}
	}

	public static void MapStrokeColor(IContentButtonHandler handler, IButtonStroke buttonStroke)
	{
		if (handler.PlatformView.Configuration is not null)
		{
			var con = handler.PlatformView.Configuration;
			con.Background.StrokeColor = buttonStroke.StrokeColor?.ToPlatform();
			handler.PlatformView.Configuration = con;
		}
	}

	public static void MapStrokeThickness(IContentButtonHandler handler, IButtonStroke buttonStroke)
	{
		if (handler.PlatformView.Configuration is not null)
		{
			var con = handler.PlatformView.Configuration;
			con.Background.StrokeWidth = (float)buttonStroke.StrokeThickness;
			handler.PlatformView.Configuration = con;
		}
	}

	public static void MapCornerRadius(IContentButtonHandler handler, IButtonStroke buttonStroke)
	{
		if (handler.PlatformView.Configuration is not null)
		{
			var con = handler.PlatformView.Configuration;
			con.Background.CornerRadius = buttonStroke.CornerRadius;
			handler.PlatformView.Configuration = con;
		}
	}

	// public static void MapPadding(IContentButtonHandler handler, IPadding padding)
	// {
	// 	handler.PlatformView?.UpdatePadding(padding.Padding, DefaultPadding);
	// }

	static void SetControlPropertiesFromProxy(UIButton platformView)
	{
		foreach (UIControlState uiControlState in ControlStates)
		{
			platformView.SetBackgroundImage(UIButton.Appearance.BackgroundImageForState(uiControlState), uiControlState);
		}
	}

	public static void MapContent(IContentButtonHandler handler, IContentButton view)
	{
		_ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
		_ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
		_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

		// Remove the known content subview (by tag) if it exists
		for (var i = 0; i < handler.PlatformView.Subviews.Length; i++)
		{
			var subview = handler.PlatformView.Subviews[i];
			if (subview.Tag == ContentButtonHandlerContentViewTag)
			{
				subview.RemoveFromSuperview();
				break;
			}
		}

		if (handler.VirtualView.PresentedContent is IView presentedView)
		{
			var inner = presentedView.ToPlatform(handler.MauiContext);
			inner.Tag = ContentButtonHandlerContentViewTag;
			handler.PlatformView.AddSubview(inner);
		}
	}
}

#endif