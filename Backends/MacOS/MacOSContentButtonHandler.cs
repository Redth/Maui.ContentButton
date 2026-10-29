using AppKit;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platforms.MacOS.Handlers;
using Microsoft.Maui.Platforms.MacOS.Platform;

namespace MauiContentButton;

public class MacOSContentButtonHandler : MacOSViewHandler<IContentButton, MauiAppKitContentButton>, IContentButtonHandler
{
	CAGradientLayer? _gradient;

	object IContentButtonHandler.PlatformView => PlatformView;
	IContentButton IContentButtonHandler.VirtualView => VirtualView;

	public static readonly IPropertyMapper<IContentButton, MacOSContentButtonHandler> Mapper =
		new PropertyMapper<IContentButton, MacOSContentButtonHandler>(ViewMapper)
		{
			[nameof(IContentButton.Content)] = MapContent,
			[nameof(IPadding.Padding)] = MapPadding,
			[nameof(IButtonStroke.StrokeThickness)] = MapStrokeThickness,
			[nameof(IButtonStroke.StrokeColor)] = MapStrokeColor,
			[nameof(IButtonStroke.CornerRadius)] = MapCornerRadius,
			[nameof(IView.Background)] = MapBackground,
			[nameof(IView.IsEnabled)] = MapIsEnabled,
		};

	public MacOSContentButtonHandler() : base(Mapper)
	{
	}

	protected override MauiAppKitContentButton CreatePlatformView() => new();

	protected override void ConnectHandler(MauiAppKitContentButton platformView)
	{
		base.ConnectHandler(platformView);
		platformView.ContentButton = VirtualView;
		platformView.Input = new ContentButtonInput(VirtualView);
		platformView.Activated += OnActivated;
	}

	protected override void DisconnectHandler(MauiAppKitContentButton platformView)
	{
		platformView.Activated -= OnActivated;
		platformView.Input?.Release();
		platformView.Input = null;
		platformView.ContentButton = null;
		ClearContent(platformView);
		ClearGradient();
		base.DisconnectHandler(platformView);
	}

	void OnActivated(object? sender, EventArgs args)
	{
		// Keyboard and accessibility activation can bypass MouseDown.
		PlatformView.Input?.Press();
		PlatformView.Input?.Click();
	}

	public override void SetVirtualView(IView view)
	{
		var changed = ((IElementHandler)this).VirtualView != view;
		if (changed && ((IElementHandler)this).PlatformView is MauiAppKitContentButton previous)
			previous.Input?.Release();
		base.SetVirtualView(view);
		if (changed)
		{
			PlatformView.ContentButton = VirtualView;
			PlatformView.Input = new ContentButtonInput(VirtualView);
		}
	}

	public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		=> DesktopContentButtonLayout.Measure(VirtualView, widthConstraint, heightConstraint);

	public override void PlatformArrange(Rect rect)
	{
		base.PlatformArrange(rect);
		if (_gradient is not null)
			_gradient.Frame = PlatformView.Bounds;
		DesktopContentButtonLayout.Arrange(VirtualView, new Rect(0, 0, rect.Width, rect.Height));
	}

	static void ClearContent(MauiAppKitContentButton platformView)
	{
		foreach (var subview in platformView.Subviews)
			subview.RemoveFromSuperview();
	}

	void ClearGradient()
	{
		_gradient?.RemoveFromSuperLayer();
		_gradient?.Dispose();
		_gradient = null;
	}

	void InvalidateContent()
	{
		PlatformView.InvalidateIntrinsicContentSize();
		PlatformView.NeedsLayout = true;
		VirtualView.InvalidateMeasure();
	}

	public static void MapContent(MacOSContentButtonHandler handler, IContentButton button)
	{
		var context = handler.MauiContext ?? throw new InvalidOperationException("MauiContext not set.");
		ClearContent(handler.PlatformView);
		if (button.PresentedContent is IView content)
			handler.PlatformView.AddSubview(content.ToMacOSPlatform(context));
		handler.InvalidateContent();
	}

	public static void MapPadding(MacOSContentButtonHandler handler, IContentButton button)
		=> handler.InvalidateContent();

	public static void MapStrokeThickness(MacOSContentButtonHandler handler, IContentButton button)
	{
		handler.PlatformView.Layer!.BorderWidth = (nfloat)DesktopContentButtonLayout.Stroke(button);
		UpdateBezel(handler, button);
		handler.InvalidateContent();
	}

	public static void MapStrokeColor(MacOSContentButtonHandler handler, IContentButton button)
	{
		handler.PlatformView.Layer!.BorderColor = button.StrokeColor is { } color ? ToCGColor(color) : null;
		UpdateBezel(handler, button);
	}

	public static void MapCornerRadius(MacOSContentButtonHandler handler, IContentButton button)
	{
		handler.PlatformView.Layer!.CornerRadius = Math.Max(0, button.CornerRadius);
		handler.PlatformView.Layer.MasksToBounds = button.CornerRadius >= 0;
		UpdateBezel(handler, button);
	}

	public static void MapIsEnabled(MacOSContentButtonHandler handler, IContentButton button)
	{
		handler.PlatformView.Enabled = button.IsEnabled;
		if (!button.IsEnabled)
			handler.PlatformView.Input?.Release();
	}

	public static void MapBackground(MacOSContentButtonHandler handler, IContentButton button)
	{
		handler.ClearGradient();
		var layer = handler.PlatformView.Layer!;
		layer.BackgroundColor = null;
		if (button.Background is SolidPaint { Color: { } color })
		{
			layer.BackgroundColor = ToCGColor(color);
		}
		else if (button.Background is GradientPaint paint)
		{
			var stops = paint.GradientStops.OrderBy(stop => stop.Offset).ToArray();
			var gradient = new CAGradientLayer
			{
				Frame = handler.PlatformView.Bounds,
				Colors = stops.Select(stop => ToCGColor(stop.Color)).ToArray(),
				Locations = stops.Select(stop => new Foundation.NSNumber(stop.Offset)).ToArray(),
			};
			if (paint is LinearGradientPaint linear)
			{
				gradient.StartPoint = new CGPoint(linear.StartPoint.X, linear.StartPoint.Y);
				gradient.EndPoint = new CGPoint(linear.EndPoint.X, linear.EndPoint.Y);
			}
			else if (paint is RadialGradientPaint radial)
			{
				gradient.LayerType = CAGradientLayerType.Radial;
				gradient.StartPoint = new CGPoint(radial.Center.X, radial.Center.Y);
				gradient.EndPoint = new CGPoint(radial.Center.X + radial.Radius, radial.Center.Y + radial.Radius);
			}
			layer.InsertSublayer(gradient, 0);
			handler._gradient = gradient;
		}
		UpdateBezel(handler, button);
	}

	static CGColor ToCGColor(Color color) => new(color.Red, color.Green, color.Blue, color.Alpha);

	static void UpdateBezel(MacOSContentButtonHandler handler, IContentButton button)
	{
		handler.PlatformView.Bordered = button.Background is null && button.CornerRadius < 0
			&& button.StrokeThickness < 0 && button.StrokeColor is null;
		handler.PlatformView.NeedsDisplay = true;
	}
}
