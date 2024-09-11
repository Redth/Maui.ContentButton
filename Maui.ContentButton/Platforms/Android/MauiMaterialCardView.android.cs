using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Google.Android.Material.Card;
using Microsoft.Maui.Platform;

namespace MauiContentButton;

public class MauiMaterialCardView : MaterialCardView, ICrossPlatformLayoutBacking
{
	readonly Context _context;

	public MauiMaterialCardView(Context context) : base(context)
	{
		_context = context;
	}

	public MauiMaterialCardView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
	{
		var context = Context;
		ArgumentNullException.ThrowIfNull(context);
		_context = context;
	}

	public MauiMaterialCardView(Context context, IAttributeSet attrs) : base(context, attrs)
	{
		_context = context;
	}

	public MauiMaterialCardView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
	{
		_context = context;
	}

	public ICrossPlatformLayout? CrossPlatformLayout
	{
		get; set;
	}

	Microsoft.Maui.Graphics.Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
	{
		return CrossPlatformLayout?.CrossPlatformMeasure(widthConstraint, heightConstraint) ?? Microsoft.Maui.Graphics.Size.Zero;
	}

	Microsoft.Maui.Graphics.Size CrossPlatformArrange(Microsoft.Maui.Graphics.Rect bounds)
	{
		return CrossPlatformLayout?.CrossPlatformArrange(bounds) ?? Microsoft.Maui.Graphics.Size.Zero;
	}

	protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
	{
		if (CrossPlatformMeasure == null)
		{
			base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
			return;
		}

		var deviceIndependentWidth = widthMeasureSpec.ToDouble(_context);
		var deviceIndependentHeight = heightMeasureSpec.ToDouble(_context);

		var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
		var heightMode = MeasureSpec.GetMode(heightMeasureSpec);

		var measure = CrossPlatformMeasure(deviceIndependentWidth, deviceIndependentHeight);

		// If the measure spec was exact, we should return the explicit size value, even if the content
		// measure came out to a different size
		var width = widthMode == MeasureSpecMode.Exactly ? deviceIndependentWidth : measure.Width;
		var height = heightMode == MeasureSpecMode.Exactly ? deviceIndependentHeight : measure.Height;

		var platformWidth = _context.ToPixels(width);
		var platformHeight = _context.ToPixels(height);

		// Minimum values win over everything
		platformWidth = Math.Max(MinimumWidth, platformWidth);
		platformHeight = Math.Max(MinimumHeight, platformHeight);

		SetMeasuredDimension((int)platformWidth, (int)platformHeight);
	}

	protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
	{
		if (CrossPlatformArrange == null)
		{
			return;
		}

		var destination = _context.ToCrossPlatformRectInReferenceFrame(left, top, right, bottom);

		CrossPlatformArrange(destination);
	}
}