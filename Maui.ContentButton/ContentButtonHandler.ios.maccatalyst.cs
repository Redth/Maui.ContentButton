#if IOS || MACCATALYST
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System.Reflection;
using UIKit;
using MButton = UIKit.UIButton;

namespace Maui.Extras
{
    // All the code in this file is only included on Windows.
    public partial class ContentButtonHandler : ViewHandler<IContentButton, MButton>
    {
        static readonly UIControlState[] ControlStates = { UIControlState.Normal, UIControlState.Highlighted, UIControlState.Disabled };

        // This appears to be the padding that Xcode has when "Default" content insets are used
        public readonly static Thickness DefaultPadding = new Thickness(12, 7);

        // Because we can't inherit from Button we use the container to handle
        // Life cycle events and things like monitoring focus changed
        public override bool NeedsContainer => true;

        protected override UIButton CreatePlatformView()
        {
            var button = new UIButton(UIButtonType.System);
            SetControlPropertiesFromProxy(button);
            return button;
        }

        readonly ButtonEventProxy _proxy = new ButtonEventProxy();


        PropertyInfo? wrapperViewCrossPlatformMeasureProperty;
        PropertyInfo WrapperViewCrossPlatformMeasureProperty
            => wrapperViewCrossPlatformMeasureProperty 
                ??= typeof(WrapperView).GetProperty("CrossPlatformLayout", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

        protected override void SetupContainer()
        {
            base.SetupContainer();
            if (ContainerView is WrapperView wrapperView)
            {
                // TODO: Use the internal property properly eventually
                WrapperViewCrossPlatformMeasureProperty.SetValue(wrapperView, VirtualView as ICrossPlatformLayout);
                //wrapperView.CrossPlatformLayout = VirtualView as ICrossPlatformLayout;
            }
        }

        protected override void ConnectHandler(UIButton platformView)
        {
            _proxy.Connect(VirtualView, platformView);

            base.ConnectHandler(platformView);
        }

        protected override void DisconnectHandler(UIButton platformView)
        {
            _proxy.Disconnect(platformView);

            base.DisconnectHandler(platformView);
        }

#if MACCATALYST
		public static void MapBackground(IContentButtonHandler handler, IContentButton button)
		{
			//If this is a Mac optimized interface
			if (OperatingSystem.IsIOSVersionAtLeast(15) && UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Mac)
			{
				var config = handler.PlatformView?.Configuration ?? UIButtonConfiguration.BorderedButtonConfiguration;
				if (button?.Background is Paint paint)
				{
					if (paint is SolidPaint solidPaint)
					{
						Color backgroundColor = solidPaint.Color;

                        if (backgroundColor == null)
                        {
                            if (OperatingSystem.IsIOSVersionAtLeast(13))
                                config.BaseBackgroundColor = UIColor.SystemBackground;
                            else
                                config.BaseBackgroundColor = UIColor.White;
                        }
                        else
                        {
                            config.BaseBackgroundColor = backgroundColor.ToPlatform();
                        }

					}
				}
				if (handler.PlatformView != null)
					handler.PlatformView.Configuration = config;
			}
			else
			{
				handler.PlatformView?.UpdateBackground(button);
			}
		}
#endif

        public static void MapStrokeColor(IContentButtonHandler handler, IButtonStroke buttonStroke)
        {
            handler.PlatformView?.UpdateStrokeColor(buttonStroke);
        }

        public static void MapStrokeThickness(IContentButtonHandler handler, IButtonStroke buttonStroke)
        {
            handler.PlatformView?.UpdateStrokeThickness(buttonStroke);
        }

        public static void MapCornerRadius(IContentButtonHandler handler, IButtonStroke buttonStroke)
        {
            handler.PlatformView?.UpdateCornerRadius(buttonStroke);
        }

        public static void MapPadding(IContentButtonHandler handler, IPadding padding)
        {
            handler.PlatformView?.UpdatePadding(padding.Padding, DefaultPadding);
        }


        static void SetControlPropertiesFromProxy(UIButton platformView)
        {
            foreach (UIControlState uiControlState in ControlStates)
            {
                platformView.SetTitleColor(UIButton.Appearance.TitleColor(uiControlState), uiControlState); // If new values are null, old values are preserved.
                platformView.SetTitleShadowColor(UIButton.Appearance.TitleShadowColor(uiControlState), uiControlState);
                platformView.SetBackgroundImage(UIButton.Appearance.BackgroundImageForState(uiControlState), uiControlState);
            }
        }

        class ButtonEventProxy
        {
            WeakReference<IContentButton>? _virtualView;

            IContentButton? VirtualView => _virtualView is not null && _virtualView.TryGetTarget(out var v) ? v : null;

            public void Connect(IContentButton virtualView, UIButton platformView)
            {
                _virtualView = new(virtualView);

                platformView.TouchUpInside += OnButtonTouchUpInside;
                platformView.TouchUpOutside += OnButtonTouchUpOutside;
                platformView.TouchDown += OnButtonTouchDown;
            }

            public void Disconnect(UIButton platformView)
            {
                _virtualView = null;

                platformView.TouchUpInside -= OnButtonTouchUpInside;
                platformView.TouchUpOutside -= OnButtonTouchUpOutside;
                platformView.TouchDown -= OnButtonTouchDown;
            }

            void OnButtonTouchUpInside(object? sender, EventArgs e)
            {
                if (VirtualView is IContentButton virtualView)
                {
                    virtualView.Released();
                    virtualView.Clicked();
                }
            }

            void OnButtonTouchUpOutside(object? sender, EventArgs e)
            {
                VirtualView?.Released();
            }

            void OnButtonTouchDown(object? sender, EventArgs e)
            {
                VirtualView?.Pressed();
            }
        }


        static void UpdateContent(IContentButtonHandler handler)
        {
            _ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
            _ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
            _ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

            var existingWrapper = handler.PlatformView.FindDescendantView<WrapperView>();
            if (existingWrapper is not null)
            {
                existingWrapper.RemoveFromSuperview();
                existingWrapper.Dispose();
                existingWrapper = null;
            }


            if (handler.VirtualView.PresentedContent is IView view)
                handler.PlatformView.AddSubview(view.ToPlatform(handler.MauiContext));
        }

        public static void MapContent(IContentButtonHandler handler, IContentButton view)
        {
            UpdateContent(handler);
        }

    }
}

#endif