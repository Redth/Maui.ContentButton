[assembly: XmlnsDefinition(MauiContentButton.Constants.XamlNamespace, MauiContentButton.Constants.MauiContentButtonNamespace)]

[assembly: Microsoft.Maui.Controls.XmlnsPrefix(MauiContentButton.Constants.XamlNamespace, "contentbutton")]

namespace MauiContentButton;

static class Constants
{
	public const string XamlNamespace = "http://schemas.microsoft.com/dotnet/2024/maui/contentbutton";
	public const string MauiContentButtonNamespace = $"{nameof(MauiContentButton)}";
}