using Grpc.Core;

using SimpleChatApp.CommonTypes;

using System;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using static SimpleChatApp.GrpcService.ChatService;

namespace MobileClient
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegistrationPage : ContentPage
    {
        private string _login;
        private string _password;
        private string _ip;
        private string _port;

        public ChatServiceClient ChatServiceClient { get; set; }

        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public string Ip
        {
            get => _ip;
            set
            {
                _ip = value;
                OnPropertyChanged();
            }
        }

        public string Port
        {
            get => _port;
            set
            {
                _port = value;
                OnPropertyChanged();
            }
        }

        public RegistrationPage()
        {
            InitializeComponent();
            Entry[] entryes = new[] { LoginEntry, PasswordEntry, ServerIpEntry, ServerPortEntry };
            foreach (Entry entry in entryes)
            {
                entry.BindingContext = this;
            }

            Ip = "127.0.0.1";
            Port = "30051";
        }

        public RegistrationPage(ChatServiceClient chatServiceClient = default,
                                string login = default,
                                string password = default,
                                string ip = default,
                                string port = default) : this()
        {
            ChatServiceClient = chatServiceClient;
            Login = login ?? Login;
            Password = password ?? Password;
            Ip = ip ?? Ip;
            Port = port ?? Port;
        }

        private async void RegisterButtonClicked(object sender, EventArgs e)
        {
            Entry[] entries = new[] { LoginEntry, PasswordEntry, ServerIpEntry, ServerPortEntry };
            foreach (Entry entry in entries)
            {
                if (string.IsNullOrEmpty(entry.Text))
                {
                    await DisplayAlert("Alert", $"{entry.Placeholder} is empty", "OK");
                    return;
                }
            }
            await Register();
        }

        private async Task Register()
        {
            try
            {
                ChatServiceClient = new ChatServiceClient(new Channel($"{Ip}:{Port}", ChannelCredentials.Insecure));
                SimpleChatApp.GrpcService.RegistrationAnswer ans = await ChatServiceClient.RegisterNewUserAsync(new SimpleChatApp.GrpcService.UserData()
                {
                    Login = Login,
                    PasswordHash = SHA256.GetStringHash(Password)
                });
                string alertText;
                switch (ans.Status)
                {
                    case SimpleChatApp.GrpcService.RegistrationStatus.RegistrationSuccessfull:
                        alertText = "Registration successfull!";
                        break;

                    case SimpleChatApp.GrpcService.RegistrationStatus.LoginAlreadyExist:
                        alertText = "Login already exist!";
                        break;

                    case SimpleChatApp.GrpcService.RegistrationStatus.BadInput:
                        alertText = "Bad login or password!";
                        break;

                    case SimpleChatApp.GrpcService.RegistrationStatus.RegistratioError:
                        alertText = "Server error!";
                        break;

                    default:
                        throw new NotImplementedException();
                }
                await DisplayAlert("Alert", alertText, "OK");

                if (ans.Status is SimpleChatApp.GrpcService.RegistrationStatus.RegistrationSuccessfull)
                {
                    await Navigation.PushAsync(new LoginPage(ChatServiceClient, Login, Password, Ip, Port));
                }
            }
            catch (RpcException ex)
            {
                await DisplayAlert("Error", $"Status: {ex.Status.StatusCode}{System.Environment.NewLine}Detail: {ex.Status.Detail}", "OK");
            }
        }
    }
}