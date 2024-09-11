#if IOS || MACCATALYST
using System.Reflection;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
using MButton = UIKit.UIButton;

namespace Maui.Extras
{
	public static class WrapperViewExtensions
	{
		static PropertyInfo? wrapperViewCrossPlatformMeasureProperty;
		static PropertyInfo WrapperViewCrossPlatformMeasureProperty
			=> wrapperViewCrossPlatformMeasureProperty
				??= typeof(WrapperView).GetProperty("CrossPlatformLayout",
					BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!;

		public static ICrossPlatformLayout? GetCrossPlatformLayout(this WrapperView wrapperView)
			=> WrapperViewCrossPlatformMeasureProperty.GetValue(wrapperView) as ICrossPlatformLayout;

		public static void SetCrossPlatformLayout(this WrapperView wrapperView, ICrossPlatformLayout value)
			=> WrapperViewCrossPlatformMeasureProperty.SetValue(wrapperView, value);
	}

	public class ContentButtonMauiView : Microsoft.Maui.Platform.MauiView
	{
		public override void LayoutSubviews()
		{
			// This seems like extra work since MauiView does calculations then we do them again
			base.LayoutSubviews();

			// Use the superview's bounds for the measurement constraints
			var bounds = Superview.Bounds;

			var widthConstraint = bounds.Width;
			var heightConstraint = bounds.Height;

			// Copied from the base implementation:
			// If the SuperView is a MauiView (backing a cross-platform ContentView or Layout), then measurement
			// has already happened via SizeThatFits and doesn't need to be repeated in LayoutSubviews. But we
			// _do_ need LayoutSubviews to make a measurement pass if the parent is something else (for example,
			// the window); there's no guarantee that SizeThatFits has been called in that case.

			if (!IsMeasureValid(widthConstraint, heightConstraint) && Superview is not MauiView)
			{
				CrossPlatformLayout?.CrossPlatformMeasure(widthConstraint, heightConstraint);
				CacheMeasureConstraints(widthConstraint, heightConstraint);
			}

			CrossPlatformLayout?.CrossPlatformArrange(bounds.ToRectangle());
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

			button.ClipsToBounds = true;
			button.AddSubview(new ContentButtonMauiView
			{
				CrossPlatformLayout = VirtualView,
			});

			return button;
		}

		readonly ButtonEventProxy _proxy = new ButtonEventProxy();

		protected override void SetupContainer()
		{
			base.SetupContainer();

			// We need to use reflection on this internal only property
			(ContainerView as WrapperView)?.SetCrossPlatformLayout(VirtualView);
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

			var contentSubview = handler.PlatformView.FindDescendantView<ContentButtonMauiView>();
			if (contentSubview is not null)
			{
				contentSubview.ClearSubviews();

				if (handler.VirtualView.PresentedContent is IView presentedView)
				{
					var inner = presentedView.ToPlatform(handler.MauiContext);
					contentSubview.AddSubview(inner);
				}

				contentSubview.SetNeedsLayout();
			}
		}

	}
}

#endif