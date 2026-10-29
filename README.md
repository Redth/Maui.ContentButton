# Maui.ContentButton
Button that can take any content inside of it while still acting like a native button

![NuGet Version](https://img.shields.io/nuget/v/Plugin.Maui.ContentButton?style=flat&logo=nuget&logoColor=ffffff&color=0b6cff)



| Android | Windows | iOS |
|---------|---------|-----|
| ![Maui ContentButton Android](https://github.com/user-attachments/assets/1a9a8872-6901-411d-9e9f-c462f0fbd8d8) | ![Maui ContentButton Windows](https://github.com/user-attachments/assets/de9c5bef-d2c6-491e-a9f8-9d3f0f5bd773) | ![Maui ContentButton iOS](https://github.com/user-attachments/assets/46a9508c-43e8-4d68-bfa1-d4724bd92689) |


## Platform Support

### Windows
Windows allows content nested in the `Button` control, so that's what we are using here.  You have a real genuine native button with this control on Windows.

### iOS / MacCatalyst
Apple's platforms have a `UIButton` which also allows adding nested content (subviews).  Just like with windows, we are using a real native `UIButton` to implement this control.

`CornerRadius` uses a fixed radius, including on iOS 26 and later. Zero or an unset value produces square corners
(unless an app style sets a radius); large values produce a capsule, limited by the button's dimensions.

### Android
Android is the trickiest, since its `Button` (and `MaterialButton`) derive from `View` which does _not_ allow directly nested content.  Luckily Android is pretty flexible about making arbitrary views (and `ViewGroup`s) act like a button.  In this case we use `MaterialCardView` to help with the ripple effect, shape, etc and then add click/touch listeners to make it behave like a button.  Android seems to consider this a real authentic button as far as the system and accessibility interations are concerned, it even plays the system 'click' sound when you press it!

### Linux / GTK4 and macOS / AppKit

The experimental [GTK4](https://github.com/dotnet/maui-labs/tree/main/platforms/Linux.Gtk4) and
[AppKit](https://github.com/dotnet/maui-labs/tree/main/platforms/MacOS) backends in `dotnet/maui-labs`
are supported by separate companion packages. They require .NET 10; the original package and its
.NET 8 platform targets are unchanged.

| Backend | Companion package | Target framework | Registration |
|---------|-------------------|------------------|--------------|
| Linux GTK4 | `Plugin.Maui.ContentButton.Gtk4` | `net10.0` | `.AddMauiContentButtonGtk4Handler()` |
| macOS AppKit | `Plugin.Maui.ContentButton.MacOS` | `net10.0-macos` | `.AddMauiContentButtonMacOSHandler()` |

Reference the companion package for your backend; it also references `Plugin.Maui.ContentButton`.
Use its registration method **after** the backend's application setup, instead of the original
`.AddMauiContentButtonHandler()`:

```csharp
using MauiContentButton;

// Linux, with Microsoft.Maui.Platforms.Linux.Gtk4.Hosting:
builder.UseMauiAppLinuxGtk4<App>()
    .AddMauiContentButtonGtk4Handler();

// Or macOS, with Microsoft.Maui.Platforms.MacOS.Hosting:
builder.UseMauiAppMacOS<App>()
    .AddMauiContentButtonMacOSHandler();
```

The XAML namespace and `ContentButton` API are the same on all platforms. GTK uses a native
`Gtk.Button` with a MAUI layout panel as its child. AppKit uses a native `NSButton` subclass
with nested views and native hit testing, keyboard activation, and accessibility activation.
Both handlers support content replacement/removal, padding, stroke, corner radius, backgrounds,
enabled state, and `Pressed` / `Released` / `Clicked` events. Cancelling a press or disconnecting
the handler releases the button without executing its command. AppKit is a separate backend,
not Mac Catalyst, and requires macOS 14 or newer.

The GTK package currently targets `Microsoft.Maui.Platforms.Linux.Gtk4`
`0.1.0-preview.13.26526.5`; AppKit targets `Microsoft.Maui.Platforms.MacOS`
`0.1.0-preview.12.26421.1`. Both require MAUI 10.0.41 or newer. AppKit is pinned before the
preview.13 menu-localization change because those NuGet packages omit the required
`MauiMenuBar.bundle` resources and fail during application startup.
The backends are experimental and may introduce breaking changes.

## Usage

Add `.AddMauiContentButtonHandler()` to your app builder in your MauiProgram.cs:

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

Import the xml namespace in the XAML file you would like to use the Content Button in:
`xmlns:mcb="http://schemas.microsoft.com/dotnet/2024/maui/contentbutton"`

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


You may want to add a style to your app's `Resources/Styles/Styles.xaml` to make the defaults more like the normal `Button`
(remember to add the `xmlns:mcb="http://schemas.microsoft.com/dotnet/2024/maui/contentbutton"` namespace to your `<ResourceDictionary>` element):
```xml
<Style TargetType="mcb:ContentButton">
   <Setter Property="BackgroundColor" Value="{AppThemeBinding Light={StaticResource Primary}, Dark={StaticResource PrimaryDark}}" />
   <Setter Property="CornerRadius" Value="8"/>
   <Setter Property="MinimumHeightRequest" Value="44"/>
   <Setter Property="MinimumWidthRequest" Value="44"/>
   <Setter Property="VisualStateManager.VisualStateGroups">
       <VisualStateGroupList>
           <VisualStateGroup x:Name="CommonStates">
               <VisualState x:Name="Normal" />
               <VisualState x:Name="Disabled">
                   <VisualState.Setters>
                       <Setter Property="BackgroundColor" Value="{AppThemeBinding Light={StaticResource Gray200}, Dark={StaticResource Gray600}}" />
                   </VisualState.Setters>
               </VisualState>
               <VisualState x:Name="PointerOver" />
           </VisualStateGroup>
       </VisualStateGroupList>
   </Setter>
</Style>
```

## Corner radius sample

The sample's **Corner radius** section demonstrates unset, zero, 25, and 500 radii without the app's implicit
rounded-button style. Check the shapes on first display, then tap the pink button to cycle through 25, zero,
500, and the default (-1). Its corners should update immediately and remain correct after pressing or resizing.

## Known Issues
- Due to some recent changes in MAUI itself, this currently requires .NET MAUI 8.0.90 (SR9) or newer

## Building the desktop backends

The projects under `Backends/` have their own .NET 10 SDK selection so building the original
solution still uses .NET 8. Run desktop commands from that directory:

```sh
cd Backends
dotnet test Tests/Maui.ContentButton.Backends.Tests.csproj
dotnet build Gtk4/Maui.ContentButton.Gtk4.csproj
dotnet build MacOS/Maui.ContentButton.MacOS.csproj
```

The AppKit build requires the .NET `macos` workload and matching Xcode. GTK builds require no
MAUI workload, but running GTK apps requires the backend's native GTK4 dependencies and a display.
`Backends/Core/` builds the existing portable core sources without restoring legacy mobile
workloads; it is not a separate package.

Runnable samples share the same content and exercise nested layout, click counting, disabled
state, and content replacement:

```sh
# On Linux:
dotnet run --project Samples/Gtk4/ContentButton.Gtk4.Sample.csproj

# On macOS:
dotnet run --project Samples/MacOS/ContentButton.MacOS.Sample.csproj
```

The GTK sample also supports `-- --smoke-test` (use `xvfb-run -a` on headless Linux).
For AppKit smoke tests, build the sample and run its executable inside
`Samples/MacOS/bin/Debug/net10.0-macos/<runtime>/ContentButton AppKit.app/Contents/MacOS/ContentButton.MacOS.Sample`
with `--smoke-test`. For a default Release build, use the signed universal bundle under
`bin/Release/net10.0-macos/ContentButton AppKit.app` instead of its per-runtime intermediate bundles.
The smoke checks exercise real native handlers, registration, sizing,
activation, disabled state, content replacement/removal, styling reset, handler reuse, and cleanup.
The AppKit smoke test also starts the full MAUI application and verifies that it creates a native
window, so missing startup resources cannot be hidden by isolated handler checks.
