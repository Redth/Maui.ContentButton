using Android.Views;
using Google.Android.Material.Shape;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using AButton = Maui.Extras.MauiMaterialCardView;
using AView = Android.Views.View;

namespace Maui.Extras
{
    // All the code in this file is only included on Windows.
    public partial class ContentButtonHandler : ViewHandler<IContentButton, AButton>
    {
        protected override AButton CreatePlatformView()
        {
            if (VirtualView == null)
            {
                throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a LayoutView");
            }

            var buttonViewGroup = new MauiMaterialCardView(Context)
            {
                CrossPlatformLayout = VirtualView
            };

            buttonViewGroup.SetClipChildren(true);

            return buttonViewGroup;
        }

        ButtonClickListener ClickListener { get; } = new ButtonClickListener();
        ButtonTouchListener TouchListener { get; } = new ButtonTouchListener();


        protected override void ConnectHandler(AButton platformView)
        {
            ClickListener.Handler = this;
            platformView.SetOnClickListener(ClickListener);

            TouchListener.Handler = this;
            platformView.SetOnTouchListener(TouchListener);

            platformView.FocusChange += OnNativeViewFocusChange;
            platformView.LayoutChange += OnPlatformViewLayoutChange;

            base.ConnectHandler(platformView);
        }


        protected override void DisconnectHandler(AButton platformView)
        {
            platformView.CrossPlatformLayout = null;
            platformView.RemoveAllViews();
            
            ClickListener.Handler = null;
            platformView.SetOnClickListener(null);

            TouchListener.Handler = null;
            platformView.SetOnTouchListener(null);

            platformView.FocusChange -= OnNativeViewFocusChange;
            platformView.LayoutChange -= OnPlatformViewLayoutChange;

            base.DisconnectHandler(platformView);
        }

        static void UpdateContent(IContentButtonHandler handler)
        {
            _ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
            _ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
            _ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");


            handler.PlatformView.RemoveAllViews();

            if (handler.VirtualView.PresentedContent is IView view)
                handler.PlatformView.AddView(view.ToPlatform(handler.MauiContext));
        }

        public static void MapContent(IContentButtonHandler handler, IContentButton view)
        {
            UpdateContent(handler);
        }

        public static void MapBackground(IContentButtonHandler handler, IContentButton view)
        {
            if (handler.PlatformView is not null)
               handler.PlatformView.SetCardBackgroundColor(view.Background.ToColor().ToAndroid());
        }

        public static void MapStrokeColor(IContentButtonHandler handler, IButtonStroke buttonStroke)
        {
            if (handler.PlatformView is not null)
                handler.PlatformView.StrokeColor = buttonStroke.StrokeColor.ToAndroid();
        }

        public static void MapStrokeThickness(IContentButtonHandler handler, IButtonStroke buttonStroke)
        {
            if (handler.PlatformView is not null)
            {
                var density = (handler.PlatformView.Resources?.DisplayMetrics?.Density ?? 1f);
                handler.PlatformView.StrokeWidth = (int)Math.Ceiling(buttonStroke.StrokeThickness) * 4;
            }
        }

        public static void MapCornerRadius(IContentButtonHandler handler, IButtonStroke buttonStroke)
        {
            if (handler.PlatformView is not null)
            {
                var density = (handler.PlatformView.Resources?.DisplayMetrics?.Density ?? 1f);

                handler.PlatformView.ShapeAppearanceModel = handler.PlatformView.ShapeAppearanceModel.ToBuilder()
                    .SetAllCorners(CornerFamily.Rounded, density * buttonStroke.CornerRadius).Build();
            }
        }

        public static void MapPadding(IContentButtonHandler handler, IPadding padding)
        {
            if (handler.PlatformView is not null)
            {
                var density = (handler.PlatformView.Resources?.DisplayMetrics?.Density ?? 1f);
                handler.PlatformView.SetPadding(
                    (int)(padding.Padding.Left * density),
                    (int)(padding.Padding.Top * density),
                    (int)(padding.Padding.Right * density),
                    (int)(padding.Padding.Bottom * density));
            }
        }

        static bool OnTouch(IContentButton? button, AView? v, MotionEvent? e)
        {
            switch (e?.ActionMasked)
            {
                case MotionEventActions.Down:
                    button?.Pressed();
                    break;
                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    button?.Released();
                    break;
            }

            return false;
        }

        static void OnClick(IContentButton? button, AView? v)
        {
            button?.Clicked();
        }

        void OnNativeViewFocusChange(object? sender, AView.FocusChangeEventArgs e)
        {
            if (VirtualView != null)
                VirtualView.IsFocused = e.HasFocus;
        }

        void OnPlatformViewLayoutChange(object? sender, AView.LayoutChangeEventArgs e)
        {
            // TODO: 
        }

        class ButtonClickListener : Java.Lang.Object, Android.Views.View.IOnClickListener
        {
            public ContentButtonHandler? Handler { get; set; }

            public void OnClick(Android.Views.View? v)
            {
                ContentButtonHandler.OnClick(Handler?.VirtualView, v);
            }
        }

        class ButtonTouchListener : Java.Lang.Object, AView.IOnTouchListener
        {
            public ContentButtonHandler? Handler { get; set; }

            public bool OnTouch(AView? v, global::Android.Views.MotionEvent? e) =>
                ContentButtonHandler.OnTouch(Handler?.VirtualView, v, e);
        }
    }
}
