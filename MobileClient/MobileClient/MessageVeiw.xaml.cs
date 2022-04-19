using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileClient
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MessageVeiw : ViewCell
    {
        public static readonly BindableProperty UsernameProperty =
        BindableProperty.Create("Username", typeof(string), typeof(MessageVeiw), "Name");

        public static readonly BindableProperty TextProperty =
        BindableProperty.Create("Text", typeof(string), typeof(MessageVeiw), "Text");

        public static readonly BindableProperty TimeStampProperty =
        BindableProperty.Create("TimeStamp", typeof(string), typeof(MessageVeiw), "TimeStamp");

        public string Username
        {
            get => (string)GetValue(UsernameProperty);
            set => SetValue(UsernameProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string TimeStamp
        {
            get => (string)GetValue(TimeStampProperty);
            set => SetValue(TimeStampProperty, value);
        }

        public MessageVeiw()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (BindingContext != null)
            {
                UsernameLabel.Text = Username;
                unchecked
                {
                    UsernameLabel.TextColor = Color.FromUint(((uint)Username.GetHashCode()) | 0xFF000000);
                }
                MessageTextLabel.Text = Text;
                MessageTimeStampLabel.Text = TimeStamp;
            }
        }
    }
}