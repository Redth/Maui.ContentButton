# Maui.ContentButton
WIP: Button that can take any content inside of it while still acting like a native button

| Android | Windows | iOS |
|---------|---------|-----|
| ![Maui ContentButton Android](https://github.com/user-attachments/assets/1a9a8872-6901-411d-9e9f-c462f0fbd8d8) | ![Maui ContentButton Windows](https://github.com/user-attachments/assets/de9c5bef-d2c6-491e-a9f8-9d3f0f5bd773) | ![Maui ContentButton iOS](https://github.com/user-attachments/assets/46a9508c-43e8-4d68-bfa1-d4724bd92689) |


## Platform Support

### Windows
Windows allows content nested in the `Button` control, so that's what we are using here.  You have a real genuine native button with this control on Windows.

### iOS / MacCatalyst
Apple's platforms have a `UIButton` which also allows adding nested content (subviews).  Just like with windows, we are using a real native `UIButton` to implement this control.

### Android
Android is the trickiest, since its `Button` (and `MaterialButton`) derive from `View` which does _not_ allow directly nested content.  Luckily Android is pretty flexible about making arbitrary views (and `ViewGroup`s) act like a button.  In this case we use `MaterialCardView` to help with the ripple effect, shape, etc and then add click/touch listeners to make it behave like a button.  Android seems to consider this a real authentic button as far as the system and accessibility interations are concerned, it even plays the system 'click' sound when you press it!

## Usage

Example of a MauiProgram.cs:

```csharp
 builder
     .UseMauiApp<App>()
      // Register the handler
     .AddMauiContentButtonHandler()
     .ConfigureFonts(fonts =>
     {
         fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
         fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
     });
```

Add your xml namespace:
`xmlns:mcb="clr-namespace:MauiContentButton;assembly=Maui.ContentButton"`

Use the button with whatever content you wish!

```xml
<mcb:ContentButton
    x:Name="CounterBtn"
    Clicked="OnCounterClicked"
    HorizontalOptions="Fill">
    <Grid ColumnDefinitions="Auto,*,Auto,Auto" RowDefinitions="*,*">
        <Image
            Source="dotnet_bot.png"
            Grid.Row="0" Grid.RowSpan="2" Grid.Column="0"
            HeightRequest="30" Margin="6,0,2,0" />
        <Label 
            Text="Content Button"
            FontSize="Subtitle"
            TextColor="{DynamicResource White}"
            FontAttributes="Bold"
            Padding="0,6,0,0"
            Grid.Column="1" Grid.Row="0"
            VerticalOptions="End"
            HorizontalOptions="Start" />

        <Label 
            x:Name="labelCounter" 
            Text="Click the button..."
            FontSize="Body"
            TextColor="{DynamicResource Gray100}"
            Padding="0,0,0,6"
            Grid.Column="1" Grid.Row="1"
            VerticalOptions="Start"
            HorizontalOptions="Start" />

        <ContentView
            WidthRequest="1.1" 
            VerticalOptions="FillAndExpand" BackgroundColor="{DynamicResource Tertiary}"
            Grid.Column="2" Grid.Row="0" Grid.RowSpan="2" />

        <Ellipse
            x:Name="ellipseState"
            WidthRequest="14" HeightRequest="14"
            Grid.Column="3" Grid.Row="0" Grid.RowSpan="2"
            VerticalOptions="Center"
            Margin="10,0,12,0"
            Fill="{DynamicResource Tertiary}"
            />
    </Grid>
</mcb:ContentButton>
```

## Known Issues
- Setting Padding/Margin on the button or direct button content itself doesn't work as expected yet on every platform
- Android uses MaterialCardView which doesn't have a simple way to set a complex background, so currently it can only set a background color (no gradients, etc)
