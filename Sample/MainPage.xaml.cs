namespace Sample
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
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
