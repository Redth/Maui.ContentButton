#if IOS || MACCATALYST
using UIKit;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Platform;

namespace MauiContentButton;

public class MauiAppleButton : UIButton, ICrossPlatformLayoutBacking
{
	bool _fireSetNeedsLayoutOnParentWhenWindowAttached;

	double _lastMeasureHeight = double.NaN;
	double _lastMeasureWidth = double.NaN;

	WeakReference<ICrossPlatformLayout>? _crossPlatformLayoutReference;

	public MauiAppleButtonTouches? TouchesHandlers { get; set; }

	protected bool IsMeasureValid(double widthConstraint, double heightConstraint)
	{
		// Check the last constraints this View was measured with; if they're the same,
		// then the current measure info is already correct and we don't need to repeat it
		return heightConstraint == _lastMeasureHeight && widthConstraint == _lastMeasureWidth;
	}

	protected void InvalidateConstraintsCache()
	{
		_lastMeasureWidth = double.NaN;
		_lastMeasureHeight = double.NaN;
	}

	protected void CacheMeasureConstraints(double widthConstraint, double heightConstraint)
	{
		_lastMeasureWidth = widthConstraint;
		_lastMeasureHeight = heightConstraint;
	}

	public ICrossPlatformLayout? CrossPlatformLayout
	{
		get => _crossPlatformLayoutReference != null && _crossPlatformLayoutReference.TryGetTarget(out var v) ? v : null;
		set => _crossPlatformLayoutReference = value == null ? null : new WeakReference<ICrossPlatformLayout>(value);
	}

	Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
	{
		return CrossPlatformLayout?.CrossPlatformMeasure(widthConstraint, heightConstraint) ?? Size.Zero;
	}

	Size CrossPlatformArrange(Rect bounds)
	{
		return CrossPlatformLayout?.CrossPlatformArrange(bounds) ?? Size.Zero;
	}

	public override CGSize SizeThatFits(CGSize size)
	{
		if (_crossPlatformLayoutReference == null)
		{
			return base.SizeThatFits(size);
		}

		var widthConstraint = size.Width;
		var heightConstraint = size.Height;

		var crossPlatformSize = CrossPlatformMeasure(widthConstraint, heightConstraint);

		CacheMeasureConstraints(widthConstraint, heightConstraint);

		return crossPlatformSize.ToCGSize();
	}

	// TODO: Possibly reconcile this code with ViewHandlerExtensions.LayoutVirtualView
	// If you make changes here please review if those changes should also
	// apply to ViewHandlerExtensions.LayoutVirtualView
	public override void LayoutSubviews()
	{
		base.LayoutSubviews();

		if (_crossPlatformLayoutReference == null)
		{
			return;
		}

		var bounds = Bounds.ToRectangle();

		var widthConstraint = bounds.Width;
		var heightConstraint = bounds.Height;

		// If the SuperView is a MauiView (backing a cross-platform ContentView or Layout), then measurement
		// has already happened via SizeThatFits and doesn't need to be repeated in LayoutSubviews. But we
		// _do_ need LayoutSubviews to make a measurement pass if the parent is something else (for example,
		// the window); there's no guarantee that SizeThatFits has been called in that case.

		if (!IsMeasureValid(widthConstraint, heightConstraint) && Superview is not MauiView)
		{
			CrossPlatformMeasure(widthConstraint, heightConstraint);
			CacheMeasureConstraints(widthConstraint, heightConstraint);
		}

		CrossPlatformArrange(bounds);
	}

	public override void SetNeedsLayout()
	{
		InvalidateConstraintsCache();
		base.SetNeedsLayout();
		TryToInvalidateSuperView(false);
	}

	private protected void TryToInvalidateSuperView(bool shouldOnlyInvalidateIfPending)
	{
		if (shouldOnlyInvalidateIfPending && !_fireSetNeedsLayoutOnParentWhenWindowAttached)
		{
			return;
		}

		// We check for Window to avoid scenarios where an invalidate might propagate up the tree
		// To a SuperView that's been disposed which will cause a crash when trying to access it
		if (Window is not null)
		{
			this.Superview?.SetNeedsLayout();
			_fireSetNeedsLayoutOnParentWhenWindowAttached = false;
		}
		else
		{
			_fireSetNeedsLayoutOnParentWhenWindowAttached = true;
		}
	}

	public override void MovedToWindow()
	{
		base.MovedToWindow();
		//_movedToWindow?.Invoke(this, EventArgs.Empty);
		TryToInvalidateSuperView(true);
	}


	public override void TouchesBegan(NSSet touches, UIEvent? evt)
	{
		base.TouchesBegan(touches, evt);

		TouchesHandlers?.PressedHandler?.Invoke();
	}

	public override void TouchesEnded(NSSet touches, UIEvent? evt)
	{
		base.TouchesEnded(touches, evt);

		TouchesHandlers?.ReleasedHandler?.Invoke();

		if (touches?.FirstOrDefault() is UITouch touch)
		{
			var point = touch.LocationInView(this);
			if (this.PointInside(point, evt)) {
				// didTouchUpInside
				TouchesHandlers?.ClickedHandler?.Invoke();
			}
		}
	}
}

public record MauiAppleButtonTouches(Action PressedHandler, Action ReleasedHandler, Action ClickedHandler)
{
}
#endif
