using Microsoft.Maui.Platform;
using WButton = Microsoft.Maui.Platform.MauiButton;
using WVerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;
using WHorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using Microsoft.UI.Xaml.Automation.Peers;

namespace MauiContentButton;

public class MauiContentButton : WButton
{
	public MauiContentButton() : base()
	{
		VerticalAlignment = WVerticalAlignment.Stretch;
		HorizontalAlignment = WHorizontalAlignment.Stretch;
	}

	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new MauiButtonAutomationPeer(this);
	}
}
