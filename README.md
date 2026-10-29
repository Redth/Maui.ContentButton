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

## Commands and enabled state

`ContentButton` supports `ICommand` through its `Command` and `CommandParameter` properties.
Like MAUI's standard `Button`, it is enabled only when its explicit or bound `IsEnabled` value,
its parent's enabled state, and `Command.CanExecute(CommandParameter)` all allow it.
Without a command, only the normal `IsEnabled` rules apply.

The button re-evaluates command eligibility when the command or parameter changes, or when
the command raises `CanExecuteChanged`. This preserves `IsEnabled` bindings and updates the
native enabled state and the `Disabled` visual state (when configured in your style).
A disabled button does not raise `Clicked` or execute its command.

For automatic disabling during asynchronous work, the command must return `false` from
`CanExecute` while busy and raise `CanExecuteChanged` when eligibility changes. The button
does not infer busy state or require a particular MVVM toolkit.

## Android ripple colors

On Android, the default ripple follows the current native `MaterialCardView` theme/style,
including after a system light/dark change without restarting the app. A theme can supply
`rippleColor` through `materialCardViewStyle`; otherwise Material uses `colorControlHighlight`.
Both the card's foreground ripple (visible over nested content) and the MAUI background
ripple are refreshed without replacing the button or its content.

Set the optional `RippleColor` property to override this default:

```xml
<mcb:ContentButton
    BackgroundColor="Gray"
    RippleColor="{AppThemeBinding Light=#44000000, Dark=#66FFFFFF}">
    <Label Text="Theme-aware feedback" TextColor="White" />
</mcb:ContentButton>
```

The color's alpha is used as supplied; no additional opacity multiplier is applied.
`Transparent` explicitly hides the ripple. Set `RippleColor` back to `null` (or clear
`RippleColorProperty`) to restore the **current** native theme default.
The property supports bindings and dynamic resources, but affects Android only; other
platforms retain their native feedback.

Native theme defaults do not necessarily contrast with every image or view nested inside
a content button. Use an explicit color for content that needs different feedback. Also,
setting MAUI's `Application.UserAppTheme` does not necessarily change Android's native theme:
use `AppThemeBinding` when you want the ripple to follow that application-level choice.

The sample's **Android ripple colors** section compares native, solid, opaque-child, and
gradient backgrounds, a resettable override, and an `AppThemeBinding` override.

### Android ripple smoke checks

The Debug sample includes opt-in native regression checks. Build/install it for an Android
emulator/device, embedding assemblies in the APK when installing with `adb` rather than an IDE:

```sh
dotnet build Sample/Sample.csproj -f net8.0-android \
    -p:TargetFrameworks=net8.0-android -p:RuntimeIdentifier=android-arm64 \
    -p:EmbedAssembliesIntoApk=true
adb install -r Sample/bin/Debug/net8.0-android/android-arm64/com.companyname.sample-Signed.apk
adb shell cmd package resolve-activity --brief com.companyname.sample
# Use the activity component printed by resolve-activity:
adb shell am start -n <activity-component> --ez ripple-smoke-test true
adb logcat -s RippleSmoke
```

After the `READY` message, switch the system theme to the opposite mode and back, waiting for
each mode's `PASS` before switching again. For example, use
`adb shell cmd uimode night yes` and `adb shell cmd uimode night no` on an isolated emulator.
The checks use a test-only styled context with distinct day/night state lists, compare surviving
controls against fresh native cards, compare foreground/background focus-ripple rendering, and
exercise override/reset, background replacement, reattachment, and handler replacement.
A final `PASS` reports completion; failures are logged and thrown. Run the normal sample as
well to inspect actual press feedback over nested content.

## Corner radius sample

The sample's **Corner radius** section demonstrates unset, zero, 25, and 500 radii without the app's implicit
rounded-button style. Check the shapes on first display, then tap the pink button to cycle through 25, zero,
500, and the default (-1). Its corners should update immediately and remain correct after pressing or resizing.

## Known Issues
- Due to some recent changes in MAUI itself, this currently requires .NET MAUI 8.0.90 (SR9) or newer

### iOS buttons inside CommunityToolkit Popup v2

Inside CommunityToolkit.Maui Popup v2, an ancestor tap recognizer can cancel touches that start on
a ContentButton's child view on iOS, preventing `Clicked` and command execution
([#15](https://github.com/Redth/Maui.ContentButton/issues/15)). If the content is purely decorative,
set `InputTransparent` on that content so touches target the native button instead:

```xml
<mcb:ContentButton Command="{Binding ConfirmCommand}">
    <Label Text="Confirm"
           InputTransparent="{OnPlatform iOS=True, Default=False}" />
</mcb:ContentButton>
```

For decorative nested content, apply it to the content layout:

```xml
<mcb:ContentButton Command="{Binding ConfirmCommand}">
    <Grid InputTransparent="{OnPlatform iOS=True, Default=False}"
          CascadeInputTransparent="True">
        <Label Text="Confirm" />
    </Grid>
</mcb:ContentButton>
```

Set this on the **content**, not the ContentButton itself. The platform-specific value leaves
other platforms unchanged. A layout's `CascadeInputTransparent="True"` also makes its descendants
input-transparent, so use this only where neither the content nor its descendants need their own
interaction. Do not apply it globally or to content containing interactive controls, scrolling,
or child gestures that must keep working. ContentButton does not force child input transparency
or redirect all child hits to itself, because doing so would take input away from those children.

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
CI pins workload set `10.0.203` and Xcode `26.3`, matching the maui-labs AppKit workflow.
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

## Publishing packages

The [Build and Publish workflow](.github/workflows/build-publish.yml) builds packages for pull requests
and pushes to `main`, but publishes to nuget.org only when a GitHub Release is published. The
release tag is used verbatim as the package version, so use a NuGet version such as `0.4.0` or
`0.4.0-preview.1` (without a `v` prefix). All build jobs must succeed before publishing the main,
GTK4, and macOS packages.

Publishing uses [NuGet Trusted Publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing)
with GitHub Actions OIDC instead of a stored API key. Complete this one-time setup before releasing:

1. Sign in to nuget.org with the account that will publish the packages. Under your username,
   open **Trusted Publishing** and add a GitHub policy with these values:

   | Field | Value |
   |-------|-------|
   | Policy owner | The NuGet user or organization that owns the packages |
   | Repository owner | `Redth` |
   | Repository | `Maui.ContentButton` |
   | Workflow file | `build-publish.yml` (filename only) |
   | Environment | Leave empty; the workflow does not use a GitHub environment |
   | Package pattern | `Plugin.Maui.ContentButton*` |
   | Scopes | Push new packages and package versions |

   The package pattern includes `Plugin.Maui.ContentButton`, `Plugin.Maui.ContentButton.Gtk4`,
   and `Plugin.Maui.ContentButton.MacOS`. Allowing new packages is needed for the companion
   packages' first publication. Select an owner with permission to publish all three packages.
2. In this GitHub repository, open **Settings > Secrets and variables > Actions > Variables**
   and add a repository variable named `NUGET_USER`. Set it to the publishing account's
   **NuGet username**, not its email address or an organization name. The existing package
   owner's username is `redth`. This is not a secret.
3. Merge the workflow change before publishing a release. The publish job requests a
   short-lived API key immediately before pushing the built packages; pull requests and
   ordinary branch pushes cannot run the login or publish steps.
4. After the first successful OIDC publication, delete the unused `NUGET_ORG_API_KEY` GitHub
   Actions secret and revoke its old API key on nuget.org if it is not used elsewhere.

The workflow does not fall back to the old API key. An absent `NUGET_USER`, a mismatched policy,
or insufficient package permissions will fail publishing rather than silently skip it.
