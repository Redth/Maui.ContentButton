using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Primitives;

namespace MauiContentButton;

internal static class DesktopContentButtonLayout
{
	public static double Stroke(IContentButton button) => Math.Max(0, button.StrokeThickness);

	public static Size Measure(IContentButton button, double widthConstraint, double heightConstraint)
	{
		var inset = button.Padding + Stroke(button);
		var width = Math.Min(Dimension.IsExplicitSet(button.Width) ? button.Width : widthConstraint, button.MaximumWidth);
		var height = Math.Min(Dimension.IsExplicitSet(button.Height) ? button.Height : heightConstraint, button.MaximumHeight);
		var content = button.PresentedContent?.Measure(
			Math.Max(0, width - inset.HorizontalThickness),
			Math.Max(0, height - inset.VerticalThickness)) ?? Size.Zero;
		var size = new Size(content.Width + inset.HorizontalThickness, content.Height + inset.VerticalThickness);
		return new Size(
			LayoutManager.ResolveConstraints(widthConstraint, button.Width, size.Width, button.MinimumWidth, button.MaximumWidth),
			LayoutManager.ResolveConstraints(heightConstraint, button.Height, size.Height, button.MinimumHeight, button.MaximumHeight));
	}

	public static void Arrange(IContentButton button, Rect bounds)
	{
		var inset = button.Padding + Stroke(button);
		var size = ContentSize(button, bounds.Size);
		button.PresentedContent?.Arrange(new Rect(bounds.Left + inset.Left, bounds.Top + inset.Top, size.Width, size.Height));
	}

	public static Size ContentSize(IContentButton button, Size size)
	{
		var inset = button.Padding + Stroke(button);
		return new Size(
			Math.Max(0, size.Width - inset.HorizontalThickness),
			Math.Max(0, size.Height - inset.VerticalThickness));
	}
}
