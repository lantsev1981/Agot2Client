using GamePortal;
using MyLibrary;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public user User { get; set; }

        public LoginWindow()
        {
            InitializeComponent();
        }

        private readonly Regex regex = new Regex(@"(.+)@([^.]+)[.](.+)");
        private void Ok_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!regex.IsMatch(email.Text))
                error.Text = "Error! Email is not correct. Check your email.";
            else if (string.IsNullOrWhiteSpace(password.Password))
                error.Text = "Error! Password is not set. Set password.";
            else
                this.DialogResult = true;
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            App.Settings.Value.User = null; App.Settings.Value.access_token = null;
            App.Settings.Write();

            this.DialogResult = false;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Ok_Button_Click(sender, e);
            if (e.Key == Key.Escape)
                Cancel_Button_Click(sender, e);
        }

        private void Help_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start($"https://lantsev1981.ru/{CultureInfo.CurrentUICulture.Name.Substring(0, 2)}/wiki/index/99e1439a-0f0b-4a85-8085-ff8c355c4d81#auth");
            }
            catch { }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NameValueCollection param = new NameValueCollection
                {
                    { "uid", User.uid },
                    { "isFacebook", User.isFacebook.ToString() }
                };
                string item = ExtHttp.ExecuteCommand<string>($"http://{App.Config.Settings.ServerAddress}:{App.Config.Settings.ServerPort}/WebHttpService/TryResetPasswordByUid", param);
                error.Text = item;
            }
            catch (Exception exp)
            {
                error.Text = exp.Message;
            }
        }

        //private void Button_Click2(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        Process.Start("https://vk.com/gim56500250");
        //    }
        //    catch { }
        //}
    }
}
