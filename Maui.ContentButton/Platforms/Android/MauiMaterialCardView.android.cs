using Android.Content;
using Android.Content.Res;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Google.Android.Material.Card;
using AndroidX.Core.Content.Resources;
using Google.Android.Material.Color;
using Google.Android.Material.Resources;
using Microsoft.Maui.Platform;
using MaterialResource = Maui.ContentButton.Resource;

namespace MauiContentButton;

public class MauiMaterialCardView : MaterialCardView, ICrossPlatformLayoutBacking
{
	readonly Context _context;
	UiMode _nightMode;

	internal event EventHandler? ThemeChanged;

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

	internal ColorStateList ResolveDefaultRippleColor()
	{
		var context = Context!;
		// Material overlays retain their Theme, and AndroidX caches resolved selector colors.
		ResourcesCompat.ThemeCompat.Rebase(context.Theme!);
		ResourcesCompat.ClearCachesForTheme(context.Theme!);
		using var attributes = context.ObtainStyledAttributes(
			null, MaterialResource.Styleable.MaterialCardView,
			MaterialResource.Attribute.materialCardViewStyle, MaterialResource.Style.Widget_MaterialComponents_CardView);
		try
		{
			return MaterialResources.GetColorStateList(context, attributes, MaterialResource.Styleable.MaterialCardView_rippleColor)
				?? ColorStateList.ValueOf(new Android.Graphics.Color(
					MaterialColors.GetColor(this, MaterialResource.Attribute.colorControlHighlight)));
		}
		finally
		{
			attributes.Recycle();
		}
	}

	protected override void OnAttachedToWindow()
	{
		base.OnAttachedToWindow();
		_nightMode = Resources!.Configuration!.UiMode & UiMode.NightMask;
		ThemeChanged?.Invoke(this, EventArgs.Empty);
	}

	protected override void OnConfigurationChanged(Configuration? newConfig)
	{
		base.OnConfigurationChanged(newConfig);
		if (newConfig is null)
			return;

		var nightMode = newConfig.UiMode & UiMode.NightMask;
		if (_nightMode == nightMode)
			return;

		_nightMode = nightMode;
		ThemeChanged?.Invoke(this, EventArgs.Empty);
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

		var destination = _context.ToCrossPlatformRectInReferenceFrame(
			left,
			top,
			right,
			bottom);

		CrossPlatformArrange(destination);
	}
}