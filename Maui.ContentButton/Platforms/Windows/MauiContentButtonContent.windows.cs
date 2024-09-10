using Microsoft.Maui.Platform;
using WVerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;
using WHorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using WThickness = Microsoft.UI.Xaml.Thickness;
using WSize = Windows.Foundation.Size;
using WRect = Windows.Foundation.Rect;

namespace Maui.Extras
{
	public class MauiContentButtonContent : MauiPanel
    {
        public MauiContentButtonContent() : base()
		{
			VerticalAlignment = WVerticalAlignment.Stretch;
			HorizontalAlignment = WHorizontalAlignment.Stretch;
            Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            Margin = new WThickness(0);
		}

		protected override WSize MeasureOverride(WSize availableSize)
		{
			double measuredHeight = 0;
			double measuredWidth = 0;
			
            foreach (var c in Children)
            {
                c.Measure(availableSize);
				measuredHeight = Math.Max(measuredHeight, c.DesiredSize.Height);
				measuredWidth = Math.Max(measuredWidth, c.DesiredSize.Width);
			}

            if (!double.IsInfinity(availableSize.Width) && HorizontalAlignment == WHorizontalAlignment.Stretch)
            {
                measuredWidth = Math.Max(measuredWidth, availableSize.Width);
			}

			if (!double.IsInfinity(availableSize.Height) && VerticalAlignment == WVerticalAlignment.Stretch)
			{
				measuredHeight = Math.Max(measuredHeight, availableSize.Height);
			}

			return new WSize(measuredWidth, measuredHeight);
		}

		protected override WSize ArrangeOverride(WSize finalSize)
		{
            foreach (var c in Children)
                c.Arrange(new WRect(0, 0, finalSize.Width, finalSize.Height));

			return new WSize(finalSize.Width, finalSize.Height);
		}
	}
}
