using Microsoft.Maui.Primitives;

namespace MauiContentButton;

// These are mirrors of the LayoutExtensions in MAUI Core but those take IContentView so aren't usable here
public static class ContentButtonLayoutExtensions
{
    public static Size MeasureContent(this IContentButton contentButton, Thickness inset, double widthConstraint, double heightConstraint)
    {
        var content = contentButton.PresentedContent;

        if (Dimension.IsExplicitSet(contentButton.Width))
        {
            widthConstraint = contentButton.Width;
        }

        if (Dimension.IsExplicitSet(contentButton.Height))
        {
            heightConstraint = contentButton.Height;
        }

        var contentSize = Size.Zero;

        if (content != null)
        {
            contentSize = content.Measure(widthConstraint - inset.HorizontalThickness,
                heightConstraint - inset.VerticalThickness);
        }

        return new Size(contentSize.Width + inset.HorizontalThickness, contentSize.Height + inset.VerticalThickness);
    }

    public static void ArrangeContent(this IContentButton contentButton, Rect bounds)
    {
        if (contentButton.PresentedContent == null)
        {
            return;
        }

        var padding = contentButton.Padding;

        var targetBounds = new Rect(bounds.Left + padding.Left, bounds.Top + padding.Top,
            bounds.Width - padding.HorizontalThickness, bounds.Height - padding.VerticalThickness);

        _ = contentButton.PresentedContent.Arrange(targetBounds);
    }
	
    public static Rect Inset(this Rect rectangle, double inset)
    {
        if (inset == 0)
        {
            return rectangle;
        }

        return new Rect(rectangle.Left + inset, rectangle.Top + inset,
            rectangle.Width - (2 * inset), rectangle.Height - (2 * inset));
    }
}