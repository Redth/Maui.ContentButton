using System.Globalization;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Platforms.Linux.Gtk4.Handlers;
using Microsoft.Maui.Platforms.Linux.Gtk4.Platform;

namespace MauiContentButton;

public class GtkContentButtonHandler : GtkViewHandler<IContentButton, Gtk.Button>, IContentButtonHandler
{
	GtkLayoutPanel? _contentPanel;
	ContentButtonInput? _input;
	Size _arrangedSize;

	object IContentButtonHandler.PlatformView => PlatformView;
	IContentButton IContentButtonHandler.VirtualView => VirtualView;

	public static readonly IPropertyMapper<IContentButton, GtkContentButtonHandler> Mapper =
		new PropertyMapper<IContentButton, GtkContentButtonHandler>(ViewMapper)
		{
			[nameof(IContentButton.Content)] = MapContent,
			[nameof(IPadding.Padding)] = MapPadding,
			[nameof(IButtonStroke.StrokeThickness)] = MapStrokeThickness,
			[nameof(IButtonStroke.StrokeColor)] = MapStrokeColor,
			[nameof(IButtonStroke.CornerRadius)] = MapCornerRadius,
		};

	public GtkContentButtonHandler() : base(Mapper)
	{
	}

	protected override Gtk.Button CreatePlatformView()
	{
		var button = Gtk.Button.New();
		_contentPanel = new GtkLayoutPanel { IsExternallyManaged = true };
		button.SetChild(_contentPanel);
		return button;
	}

	protected override void ConnectHandler(Gtk.Button platformView)
	{
		base.ConnectHandler(platformView);
		_input = new ContentButtonInput(VirtualView);
		platformView.OnClicked += OnClicked;
		platformView.OnStateFlagsChanged += OnStateFlagsChanged;
	}

	protected override void DisconnectHandler(Gtk.Button platformView)
	{
		platformView.OnClicked -= OnClicked;
		platformView.OnStateFlagsChanged -= OnStateFlagsChanged;
		_input?.Release();
		_input = null;
		ClearContent();
		platformView.SetChild(null);
		_contentPanel?.Dispose();
		_contentPanel = null;
		_arrangedSize = Size.Zero;
		base.DisconnectHandler(platformView);
	}

	void OnClicked(Gtk.Button sender, EventArgs args) => _input?.Click();

	public override void SetVirtualView(IView view)
	{
		var changed = ((IElementHandler)this).VirtualView != view;
		if (changed)
			_input?.Release();
		base.SetVirtualView(view);
		if (changed)
			_input = new ContentButtonInput(VirtualView);
	}

	void OnStateFlagsChanged(Gtk.Widget sender, Gtk.Widget.StateFlagsChangedSignalArgs args)
	{
		if (sender.GetStateFlags().HasFlag(Gtk.StateFlags.Active) && VirtualView.IsEnabled)
			_input?.Press();
		else
			_input?.Release();
	}

	public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		=> DesktopContentButtonLayout.Measure(VirtualView, widthConstraint, heightConstraint);

	public override void PlatformArrange(Rect rect)
	{
		base.PlatformArrange(rect);
		_arrangedSize = rect.Size;
		ArrangeContent();
	}

	void ArrangeContent()
	{
		var size = DesktopContentButtonLayout.ContentSize(VirtualView, _arrangedSize);
		VirtualView.PresentedContent?.Arrange(new Rect(0, 0, size.Width, size.Height));
	}

	void InvalidateContent()
	{
		VirtualView.InvalidateMeasure();
		PlatformView.QueueResize();
		if (_arrangedSize.Width > 0 && _arrangedSize.Height > 0)
			ArrangeContent();
	}

	void ClearContent()
	{
		if (_contentPanel is not { } panel)
			return;
		while (panel.GetFirstChild() is Gtk.Widget child)
			panel.RemoveChild(child);
	}

	public static void MapContent(GtkContentButtonHandler handler, IContentButton button)
	{
		var context = handler.MauiContext ?? throw new InvalidOperationException("MauiContext not set.");
		handler.ClearContent();
		if (button.PresentedContent is IView content)
		{
			var platformContent = content.ToPlatform(context) as Gtk.Widget
				?? throw new InvalidOperationException("Content must have a GTK handler.");
			handler._contentPanel!.AddChild(platformContent);
		}
		handler.InvalidateContent();
	}

	public static void MapPadding(GtkContentButtonHandler handler, IContentButton button)
	{
		var padding = button.Padding;
		handler.UpdateCss(handler.PlatformView, FormattableString.Invariant(
			$"padding: {padding.Top}px {padding.Right}px {padding.Bottom}px {padding.Left}px; min-width: 0; min-height: 0;"));
		handler.InvalidateContent();
	}

	public static void MapStrokeThickness(GtkContentButtonHandler handler, IContentButton button)
	{
		handler.UpdateCss(handler.PlatformView,
			$"border-width: {DesktopContentButtonLayout.Stroke(button).ToString(CultureInfo.InvariantCulture)}px; border-style: solid;");
		handler.InvalidateContent();
	}

	public static void MapStrokeColor(GtkContentButtonHandler handler, IContentButton button)
	{
		handler.UpdateCss(handler.PlatformView, button.StrokeColor is { } color
			? FormattableString.Invariant($"border-color: rgba({color.Red * 255},{color.Green * 255},{color.Blue * 255},{color.Alpha});")
			: null);
	}

	public static void MapCornerRadius(GtkContentButtonHandler handler, IContentButton button)
	{
		handler.UpdateCss(handler.PlatformView, button.CornerRadius >= 0
			? $"border-radius: {button.CornerRadius}px;" : null);
	}
}
