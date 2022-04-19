namespace MobileClient.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();

            LoadApplication(new MobileClient.App());
        }
    }
}