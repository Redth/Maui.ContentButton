using AppKit;
using CoreGraphics;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace MauiContentButton;

public class MauiAppKitContentButton : NSButton
{
	internal IContentButton? ContentButton { get; set; }
	internal ContentButtonInput? Input { get; set; }

	public MauiAppKitContentButton()
	{
		Title = string.Empty;
		BezelStyle = NSBezelStyle.Rounded;
		SetButtonType(NSButtonType.MomentaryPushIn);
		WantsLayer = true;
	}

	public override bool IsFlipped => true;

	public override NSView? HitTest(CGPoint point)
	{
		var hit = base.HitTest(point);
		return hit is null || ContentButton?.InputTransparent == true ? null : this;
	}

	public override CGSize IntrinsicContentSize
	{
		get
		{
			if (ContentButton is not { } button)
				return base.IntrinsicContentSize;
			var size = DesktopContentButtonLayout.Measure(button, double.PositiveInfinity, double.PositiveInfinity);
			return new CGSize(size.Width, size.Height);
		}
	}

	public override void Layout()
	{
		base.Layout();
		if (ContentButton is { } button)
			DesktopContentButtonLayout.Arrange(button, new Rect(0, 0, Bounds.Width, Bounds.Height));
	}

	public override void MouseDown(NSEvent theEvent)
	{
		if (!Enabled)
			return;

		Input?.Press();
		try
		{
			// NSButton tracks the entire mouse sequence synchronously, including cancellation.
			base.MouseDown(theEvent);
		}
		finally
		{
			Input?.Release();
		}
	}
}
