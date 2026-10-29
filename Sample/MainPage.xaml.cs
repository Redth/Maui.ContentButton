namespace Sample
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
#if ANDROID && DEBUG
            Loaded += async (_, _) => await RippleSmokeTests.RunAsync(CounterBtn);
#endif
        }

        private void OnRippleOverrideClicked(object sender, EventArgs e)
        {
            RippleOverrideButton.RippleColor = RippleOverrideButton.RippleColor is null
                ? Color.FromArgb("#66FFFFFF")
                : null;
            RippleOverrideLabel.Text = RippleOverrideButton.RippleColor is null
                ? "Native default: tap to set white ripple"
                : "White ripple: tap to restore native default";
        }

        private void OnRadiusTestClicked(object sender, EventArgs e)
        {
            RadiusTestButton.CornerRadius = RadiusTestButton.CornerRadius switch
            {
                25 => 0,
                0 => 500,
                500 => MauiContentButton.ContentButton.DefaultCornerRadius,
                _ => 25
            };
            RadiusTestLabel.Text = $"CornerRadius = {RadiusTestButton.CornerRadius}: tap to change";
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;


            if (count == 1)
                labelCounter.Text = $"Clicked {count} time";
            else
				labelCounter.Text = $"Clicked {count} times";

            ellipseState.Fill = (count % 2 == 0 ? App.Current.Resources["Tertiary"] : App.Current.Resources["Secondary"]) as Color;
            //SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }

}
