#if IOS || MACCATALYST
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System.Reflection;
using CoreGraphics;
using UIKit;
using ContentView = Microsoft.Maui.Controls.ContentView;
using MButton = UIKit.UIButton;

namespace Maui.Extras
{
    public class ContentButtonWrapperView : WrapperView
    {
        public ContentButtonWrapperView(RectF frame) : base(frame)
        {
            
        }
        
        PropertyInfo? wrapperViewCrossPlatformMeasureProperty;
        PropertyInfo WrapperViewCrossPlatformMeasureProperty
            => wrapperViewCrossPlatformMeasureProperty 
                ??= typeof(WrapperView).GetProperty("CrossPlatformLayout",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!;

        public ICrossPlatformLayout CrossPlatformLayout
        {
            get => WrapperViewCrossPlatformMeasureProperty.GetValue(this) as ICrossPlatformLayout;
            set => WrapperViewCrossPlatformMeasureProperty.SetValue(this, value);
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            var baseSizeThatFits = base.SizeThatFits(size);

            var width = baseSizeThatFits.Width;
            var height = baseSizeThatFits.Height;

            if (!nfloat.IsInfinity(size.Width))
                width = new nfloat(Math.Max(width.Value,  size.Width));
            
            if (!nfloat.IsInfinity(size.Height))
                height = new nfloat(Math.Max(height.Value,  size.Height));

            return new CGSize(width, height);
        }
    }
    
    
    public class ContentWrapper : Microsoft.Maui.Platform.ContentView
    {
        public override CGSize SizeThatFits(CGSize size)
        {
            var baseSizeThatFits = base.SizeThatFits(size);

            var width = baseSizeThatFits.Width;
            var height = baseSizeThatFits.Height;

            if (!nfloat.IsInfinity(size.Width))
                width = new nfloat(Math.Max(width.Value,  size.Width));
            
            if (!nfloat.IsInfinity(size.Height))
                height = new nfloat(Math.Max(height.Value,  size.Height));

            return new CGSize(width, height);
        }
    }
    
    public partial class ContentButtonHandler : ViewHandler<IContentButton, MButton>
    {
        static readonly UIControlState[] ControlStates = { UIControlState.Normal, UIControlState.Highlighted, UIControlState.Disabled };

        public readonly static Thickness DefaultPadding = new Thickness(12, 7);

        // Because we can't inherit from Button we use the container to handle
        // Life cycle events and things like monitoring focus changed
        public override bool NeedsContainer => true;

        
        protected override UIButton CreatePlatformView()
        {
            var button = new UIButton(UIButtonType.System);
            SetControlPropertiesFromProxy(button);
            var contentSubview = new ContentWrapper
            {
                CrossPlatformLayout = VirtualView,
                AutoresizingMask = UIViewAutoresizing.All,
                Frame = button.Bounds
            };

            button.ClipsToBounds = true;
            button.AddSubview(contentSubview);
            return button;
        }

        readonly ButtonEventProxy _proxy = new ButtonEventProxy();
        
        protected override void SetupContainer()
        {
            if (PlatformView == null || ContainerView != null)
                return;

            var oldParent = (UIView?)PlatformView.Superview;

            var oldIndex = oldParent?.IndexOfSubview(PlatformView);
            PlatformView.RemoveFromSuperview();

            ContainerView ??= new ContentButtonWrapperView(PlatformView.Bounds.ToRectangle())
            {
                CrossPlatformLayout = VirtualView
            };
            ContainerView.AddSubview(PlatformView);

            if (oldIndex is int idx && idx >= 0)
                oldParent?.InsertSubview(ContainerView, idx);
            else
                oldParent?.AddSubview(ContainerView);
        }

        protected override void ConnectHandler(MButton platformView)
        {
            _proxy.Connect(VirtualView, platformView);

            base.ConnectHandler(platformView);
        }

        protected override void DisconnectHandler(MButton platformView)
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

        public static void MapContent(IContentButtonHandler handler, IContentButton view)
        {
            _ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
            _ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
            _ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

            var contentSubview = handler.PlatformView.FindDescendantView<ContentWrapper>();
            if (contentSubview is not null)
            {
                contentSubview.ClearSubviews();

                if (handler.VirtualView.PresentedContent is IView presentedView)
                {
                    var inner = presentedView.ToPlatform(handler.MauiContext);
                    inner.Frame = handler.PlatformView.Bounds;
                    inner.AutoresizingMask = UIViewAutoresizing.All;
                    contentSubview.AddSubview(inner);
                }
            }
        }

    }
}

#endif