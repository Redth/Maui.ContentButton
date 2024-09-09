#if WINDOWS
#elif ANDROID
using PlatformButtonView = Android.Widget.Button;
#elif IOS || MACCATALYST
using PlatformButtonView = UIKit.UIButton;
#else
using PlatformButtonView = object;
#endif


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Primitives;

namespace Maui.Extras
{
    [ContentProperty(nameof(Content))]
    public class ContentButton : View, IContentButton
    {

        /// <summary>Bindable property for <see cref="Content"/>.</summary>
        public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(View), typeof(ContentView), null);

        /// <include file="../../docs/Microsoft.Maui.Controls/ContentView.xml" path="//Member[@MemberName='Content']/Docs/*" />
        public View Content
        {
            get { return (View)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            IView content = Content;

            if (content == null && (this as IContentView)?.PresentedContent is IView presentedContent)
                content = presentedContent;
        }

        object IContentButton.Content => Content;

        IView IContentButton.PresentedContent => Content;


        public static readonly BindableProperty PaddingProperty =
            BindableProperty.Create(nameof(Padding), typeof(Thickness), typeof(ContentButton), new Thickness(), propertyChanged: (bindableObject, oldValue, newValue) =>
            {
                if (bindableObject is VisualElement ve)
                {
                    // measureinvalidate
                }
            });

        public Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }



        public const int DefaultCornerRadius = -1;

        public static readonly BindableProperty BorderColorProperty =
            BindableProperty.Create(nameof(IBorderElement.BorderColor), typeof(Color), typeof(IBorderElement), null);

        /// <summary>Bindable property for <see cref="IBorderElement.BorderWidth"/>.</summary>
        public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(nameof(IBorderElement.BorderWidth), typeof(double), typeof(IBorderElement), -1d);

        /// <summary>Bindable property for <see cref="IBorderElement.CornerRadius"/>.</summary>
        public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(IBorderElement.CornerRadius), typeof(int), typeof(IBorderElement), defaultValue: DefaultCornerRadius);

        

        public Color StrokeColor
        {
            get => (Color)GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }

        public double StrokeThickness
        {
            get => (double)GetValue(BorderWidthProperty);
            set => SetValue(BorderWidthProperty, value);
        }

        public int CornerRadius
        {
            get => (int)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        void IContentButton.Clicked()
        {
            //throw new NotImplementedException();
        }

        public Size CrossPlatformArrange(Rect bounds)
        {
            var thisContentButton = this as IContentButton;

            if (thisContentButton?.PresentedContent is null)
            {
                return bounds.Size;
            }

            var padding = Padding;

            var targetBounds = new Rect(
                bounds.Left + padding.Left,
                bounds.Top + padding.Top,
                bounds.Width - padding.HorizontalThickness,
                bounds.Height - padding.VerticalThickness);

            _ = thisContentButton.PresentedContent.Arrange(targetBounds);

            return bounds.Size;
        }

        public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
        {
            var thisContentButton = this as IContentButton;

            var content = thisContentButton?.PresentedContent;

            if (thisContentButton is not null)
            {

                if (Dimension.IsExplicitSet(thisContentButton.Width))
                {
                    widthConstraint = thisContentButton.Width;
                }

                if (Dimension.IsExplicitSet(thisContentButton.Height))
                {
                    heightConstraint = thisContentButton.Height;
                }
            }

            var contentSize = Size.Zero;
            var inset = this.Padding;

            if (content is not null)
            {
                contentSize = content.Measure(widthConstraint - inset.HorizontalThickness,
                    heightConstraint - inset.VerticalThickness);
            }

            return new Size(contentSize.Width + inset.HorizontalThickness, contentSize.Height + inset.VerticalThickness);
        }

        public void Pressed()
        {
           //throw new NotImplementedException();
        }

        public void Released()
        {
            //throw new NotImplementedException();
        }
    }
}
