using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;

namespace MauiContentButton.Tests;

public class DesktopContentButtonLayoutTests
{
	[Theory]
	[InlineData(-1, 0)]
	[InlineData(0, 0)]
	[InlineData(3, 3)]
	public void UnsetStrokeDoesNotShrinkContent(double stroke, double expectedStroke)
	{
		var button = new ContentButton { StrokeThickness = stroke, Padding = new Thickness(4) };

		var size = DesktopContentButtonLayout.Measure(button, 100, 100);

		Assert.Equal(new Size(8 + expectedStroke * 2, 8 + expectedStroke * 2), size);
	}

	[Fact]
	public void MeasurementIncludesAsymmetricPaddingAndStrokeOnce()
	{
		var content = new MeasuredView();
		var button = new ContentButton
		{
			Content = content,
			Padding = new Thickness(2, 3, 4, 5),
			StrokeThickness = 2,
		};

		var size = DesktopContentButtonLayout.Measure(button, 100, 80);

		Assert.Equal(new Size(90, 68), content.Constraints);
		Assert.Equal(new Size(30, 22), size);
	}

	[Fact]
	public void LayoutClampsSmallBoundsAndInsetsContentOnce()
	{
		var content = new MeasuredView();
		var button = new ContentButton
		{
			Content = content,
			Padding = new Thickness(2, 3, 4, 5),
			StrokeThickness = 2,
		};
		DesktopContentButtonLayout.Measure(button, 100, 80);

		DesktopContentButtonLayout.Arrange(button, new Rect(0, 0, 100, 80));
		Assert.Equal(new Rect(4, 5, 90, 68), content.ArrangedBounds);
		DesktopContentButtonLayout.Arrange(button, new Rect(0, 0, 1, 1));
		Assert.Equal(new Rect(4, 5, 0, 0), content.ArrangedBounds);
	}

	[Fact]
	public void SmallMeasurementNeverPassesNegativeConstraints()
	{
		var content = new MeasuredView();
		var button = new ContentButton { Content = content, Padding = new Thickness(20) };

		DesktopContentButtonLayout.Measure(button, 5, 5);

		Assert.Equal(Size.Zero, content.Constraints);
	}

	[Fact]
	public void MeasurementHonorsExplicitAndMinimumSizes()
	{
		var button = new ContentButton { WidthRequest = 80, MinimumHeightRequest = 44 };

		var size = DesktopContentButtonLayout.Measure(button, 100, 100);

		Assert.Equal(new Size(80, 44), size);
	}

	[Fact]
	public void MaximumSizesConstrainContentMeasurement()
	{
		var content = new MeasuredView();
		var button = new ContentButton
		{
			Content = content,
			Padding = 4,
			MaximumWidthRequest = 50,
			MaximumHeightRequest = 30,
		};

		DesktopContentButtonLayout.Measure(button, 200, 100);

		Assert.Equal(new Size(42, 22), content.Constraints);
	}

	[Fact]
	public void ReplacementContentIsMeasuredInsteadOfOldContent()
	{
		var oldContent = new MeasuredView();
		var button = new ContentButton { Content = oldContent };
		DesktopContentButtonLayout.Measure(button, 100, 100);
		var replacement = new MeasuredView();

		button.Content = replacement;
		DesktopContentButtonLayout.Measure(button, 80, 60);
		DesktopContentButtonLayout.Arrange(button, new Rect(0, 0, 80, 60));

		Assert.Equal(new Size(100, 100), oldContent.Constraints);
		Assert.Equal(new Size(80, 60), replacement.Constraints);
		Assert.Equal(new Rect(0, 0, 80, 60), replacement.ArrangedBounds);
	}

	sealed class MeasuredView : View
	{
		public Size Constraints { get; private set; }
		public Rect ArrangedBounds { get; private set; }

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			Constraints = new Size(widthConstraint, heightConstraint);
			return new Size(Math.Min(20, widthConstraint), Math.Min(10, heightConstraint));
		}

		protected override Size ArrangeOverride(Rect bounds)
		{
			ArrangedBounds = bounds;
			return bounds.Size;
		}
	}
}
