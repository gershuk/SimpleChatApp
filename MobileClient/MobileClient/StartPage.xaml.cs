using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileClient
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StartPage : ContentPage
    {
        public string Log { get; set; } = "ddddd";

        public StartPage() => InitializeComponent();//L.SetBinding(Entry.TextProperty, "Log"); // "Name" is the property on the view model

        private async void ToLoginPageButtonClicked(object sender, EventArgs e) => await Navigation.PushAsync(new LoginPage());

        private async void ToRegisterPageButtonClicked(object sender, EventArgs e) => await Navigation.PushAsync(new RegistrationPage());
    }
}