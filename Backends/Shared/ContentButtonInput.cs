namespace MauiContentButton;

internal sealed class ContentButtonInput
{
	readonly IContentButton _button;
	bool _pressed;

	public ContentButtonInput(IContentButton button) => _button = button;

	public void Press()
	{
		if (_pressed || !_button.IsEnabled)
			return;

		_pressed = true;
		_button.Pressed();
	}

	public void Release()
	{
		if (!_pressed)
			return;

		_pressed = false;
		_button.Released();
	}

	public void Click()
	{
		Release();
		if (_button.IsEnabled)
			_button.Clicked();
	}
}
