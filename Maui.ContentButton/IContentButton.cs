#if WINDOWS
#elif ANDROID
using PlatformButtonView = Android.Widget.Button;
#elif IOS || MACCATALYST
using PlatformButtonView = UIKit.UIButton;
#else
using PlatformButtonView = object;
#endif

namespace MauiContentButton;

public interface IContentButton : IView, IPadding, IButtonStroke, ICrossPlatformLayout
{
	/// <summary>
	/// Occurs when the button is pressed.
	/// </summary>
	void Pressed();

	/// <summary>
	/// Occurs when the button is released.
	/// </summary>
	void Released();

	/// <summary>
	/// Occurs when the button is clicked/tapped.
	/// </summary>
	void Clicked();

	/// <summary>
	/// Gets the raw content of this view.
	/// </summary>
	object? Content { get; }

	/// <summary>
	/// Gets the content of this view as it will be rendered in the user interface, including any transformations or applied templates.
	/// </summary>
	IView? PresentedContent { get; }

	/// <summary>
	/// This interface method is provided for backward compatibility with previous versions. 
	/// Implementing classes should implement the ICrossPlatformLayout interface rather than directly implementing this method.
	/// </summary>
	new Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
	=> (Content as IView)?.Measure(widthConstraint, heightConstraint) ?? Size.Zero;

	/// <summary>
	/// This interface method is provided for backward compatibility with previous versions. 
	/// Implementing classes should implement the ICrossPlatformLayout interface rather than directly implementing this method.
	/// </summary>
	new Size CrossPlatformArrange(Rect bounds)
		=> (Content as IView)?.Arrange(bounds) ?? Size.Zero;

#if !NETSTANDARD2_0
	Size ICrossPlatformLayout.CrossPlatformArrange(Microsoft.Maui.Graphics.Rect bounds) => CrossPlatformArrange(bounds);
	Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint) => CrossPlatformMeasure(widthConstraint, heightConstraint);
#endif
}
