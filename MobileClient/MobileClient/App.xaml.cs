using System;
using System.Linq;
using System.Text.RegularExpressions;

using Xamarin.Forms;

namespace MobileClient
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            //MainPage = ;
            MainPage = new NavigationPage(new StartPage());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }

    public class NumericValidationBehavior : Behavior<Entry>
    {
        protected override void OnAttachedTo(Entry entry)
        {
            entry.TextChanged += OnEntryTextChanged;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnEntryTextChanged;
            base.OnDetachingFrom(entry);
        }

        private static void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.NewTextValue))
            {
                ((Entry)sender).Text = "0";
                return;
            }

            bool isValid = args
                .NewTextValue
                .ToCharArray()
                .All(char.IsDigit)
                || (args.NewTextValue.Length > 1 && args.NewTextValue.StartsWith("-"));

            string current = args.NewTextValue;
            current = current.TrimStart('0');

            if (current.Length == 0)
            {
                current = "0";
            }

           ((Entry)sender).Text = isValid ? current : current.Remove(current.Length - 1);
        }
    }

    public class IpValidationBehavior : Behavior<Entry>
    {
        private static readonly Regex _regex = new Regex(@"[^\d.]+");

        protected override void OnAttachedTo(Entry entry)
        {
            entry.TextChanged += OnEntryTextChanged;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnEntryTextChanged;
            base.OnDetachingFrom(entry);
        }

        private static void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            Entry entery = (Entry)sender;
            string text = args.NewTextValue;
            if (text.Where(c => c is '.').Count() > 3)
            {
                text = args.OldTextValue;
            }

            text = _regex.Replace(text, string.Empty);
            if (text.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => Convert.ToInt32(s))
                    .Where(n => n > 255)
                    .Count() > 0)
            {
                text = args.OldTextValue;
            }

            entery.Text = text;
        }
    }
}