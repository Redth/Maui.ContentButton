#if WINDOWS
#elif ANDROID
using PlatformButtonView = Android.Widget.Button;
#elif IOS || MACCATALYST
using PlatformButtonView = UIKit.UIButton;
#else
using PlatformButtonView = object;
#endif

using System.Windows.Input;


namespace MauiContentButton;

[ContentProperty(nameof(Content))]
public class ContentButton : View, IContentButton, ICrossPlatformLayout
{

	/// <summary>Bindable property for <see cref="Content"/>.</summary>
	public static readonly BindableProperty ContentProperty 
		= BindableProperty.Create(nameof(Content), typeof(View), typeof(ContentView), null,
		propertyChanged: (bindableObject, oldValue, newValue) =>
		{
			if (bindableObject is ContentButton contentButton)
			{
				
				if (oldValue is View oldView)
				{
					contentButton.RemoveLogicalChild(oldView);
				}

				if (newValue is View newView)
				{
					contentButton.AddLogicalChild(newView);
				}
			}
		});

	public View Content
	{
		get { return (View)GetValue(ContentProperty); }
		set { SetValue(ContentProperty, value); }
	}

	protected override void OnBindingContextChanged()
	{
		base.OnBindingContextChanged();

		IView content = Content;

		if (content == null && (this as IContentView)?.PresentedContent is IView presentedContent)
			content = presentedContent;

		if (content is BindableObject bindableContent)
			bindableContent.BindingContext = BindingContext;
	}

	object IContentButton.Content => Content;

	IView IContentButton.PresentedContent => Content;


	// public static readonly BindableProperty PaddingProperty =
	// 	BindableProperty.Create(nameof(Padding), typeof(Thickness), typeof(ContentButton), new Thickness(),
	// 	propertyChanged: (bindable, oldValue, newValue) =>
	// 	{
	// 		if (bindable is ContentButton contentButton)
	// 		{
	// 			contentButton.InvalidateMeasure();
	// 		}
	// 	});

	// public Thickness Padding
	// {
	// 	get => (Thickness)GetValue(PaddingProperty);
	// 	set => SetValue(PaddingProperty, value);
	// }



	public const int DefaultCornerRadius = -1;

	public static readonly BindableProperty BorderColorProperty =
		BindableProperty.Create(nameof(IBorderElement.BorderColor), typeof(Color), typeof(IBorderElement), null);

	/// <summary>Bindable property for <see cref="IBorderElement.BorderWidth"/>.</summary>
	public static readonly BindableProperty BorderWidthProperty =
		BindableProperty.Create(nameof(IBorderElement.BorderWidth), typeof(double), typeof(IBorderElement), -1d);

	/// <summary>Bindable property for <see cref="IBorderElement.CornerRadius"/>.</summary>
	public static readonly BindableProperty CornerRadiusProperty =
		BindableProperty.Create(nameof(IBorderElement.CornerRadius), typeof(int), typeof(IBorderElement), defaultValue: DefaultCornerRadius);



	public Color StrokeColor
	{
		get => (Color)GetValue(BorderColorProperty);
		set => SetValue(BorderColorProperty, value);
	}

	public double StrokeThickness
	{
		get => (double)GetValue(BorderWidthProperty);
		set => SetValue(BorderWidthProperty, value);
	}

	public int CornerRadius
	{
		get => (int)GetValue(CornerRadiusProperty);
		set => SetValue(CornerRadiusProperty, value);
	}

	static readonly BindablePropertyKey IsPressedPropertyKey = BindableProperty.CreateReadOnly(nameof(IsPressed), typeof(bool), typeof(ContentButton), default(bool));

	public static readonly BindableProperty IsPressedProperty = IsPressedPropertyKey.BindableProperty;

	public bool IsPressed => (bool)GetValue(IsPressedProperty);

	void IContentButton.Clicked()
	{
		if (IsEnabled)
		{
			this.ChangeVisualState();
			Clicked?.Invoke(this, EventArgs.Empty);
			Command?.Execute(CommandParameter);
		}
	}

	void IContentButton.Pressed()
	{
		SetValue(IsPressedPropertyKey, true);
		ChangeVisualState();
		Pressed?.Invoke(this, EventArgs.Empty);
	}

	void IContentButton.Released()
	{
		SetValue(IsPressedPropertyKey, false);
		ChangeVisualState();
		Released?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Occurs when the button is clicked/tapped.
	/// </summary>
	public event EventHandler Clicked;

	/// <summary>
	/// Occurs when the button is pressed.
	/// </summary>
	public event EventHandler Pressed;

	/// <summary>
	/// Occurs when the button is released.
	/// </summary>
	public event EventHandler Released;

	public const string PressedVisualState = "Pressed";


	protected override void ChangeVisualState()
	{
		if (IsEnabled && IsPressed)
		{
			VisualStateManager.GoToState(this, PressedVisualState);
		}
		else
		{
			base.ChangeVisualState();
		}
	}

	public static readonly BindableProperty CommandProperty = BindableProperty.Create(
		nameof(Command), typeof(ICommand), typeof(ContentButton), null);

	public ICommand Command
	{
		get => (ICommand)GetValue(CommandProperty);
		set => SetValue(CommandProperty, value);
	}

	public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
		nameof(CommandParameter), typeof(object), typeof(ContentButton), null);

	public object CommandParameter
	{
		get => GetValue(CommandParameterProperty);
		set => SetValue(CommandParameterProperty, value);
	}

}
