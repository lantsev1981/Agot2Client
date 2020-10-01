using GamePortal;
using MyLibrary;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для ConfirmWindow.xaml
    /// </summary>
    public partial class ApiWindow : Window
    {
        public ApiWindow()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.Settings.Value.User = null; App.Settings.Value.access_token = null;
            App.Settings.Write();

            DialogResult = false;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("rundll32.exe", "InetCpl.cpl,ClearMyTracksByProcess 255"); //— полная очистка кэша браузера
        }

        private void ResetPswButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(email.Text))
                error.Text = "Error! Email is empty. Set your email.";
            else if (!email.Text.Contains('@'))
                error.Text = "Error! Email is not correct. Check your email.";
            else
            {
                try
                {
                    NameValueCollection param = new NameValueCollection
                    {
                        { "email", email.Text }
                    };
                    string item = ExtHttp.ExecuteCommand<string>($"http://{App.Config.Settings.ServerAddress}:{App.Config.Settings.ServerPort}/WebHttpService/TryResetPasswordByEmail", param);
                    error.Text = item;
                }
                catch (Exception exp)
                {
                    error.Text = exp.Message;
                }
            }
        }

        private readonly Regex regex = new Regex(@"(.+)@([^.]+)[.](.+)");
        private void Ok_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!regex.IsMatch(email.Text))
                error.Text = "Error! Email is not correct. Check your email.";
            /*else if (string.IsNullOrWhiteSpace(password.Password))
                error.Text = "Error! Password is not set. Set password.";*/
            else
            {
                try
                {
                    NameValueCollection param = new NameValueCollection
                    {
                        { "email", email.Text},
                        { "password",Crypto.SHA1Hex(password.Password)},
                        { "clientId", App.ClientId }
                    };
                    AuthorizeResult item = ExtHttp.ExecuteCommand<AuthorizeResult>($"http://{App.Config.Settings.ServerAddress}:{App.Config.Settings.ServerPort}/WebHttpService/GetLogin", param);

                    if (!string.IsNullOrWhiteSpace(item.Error))
                        error.Text = item.Error;
                    else if (!string.IsNullOrWhiteSpace(item.Login))
                    {
                        App.Settings.Value.User = new user() { login = item.Login, email = item.Email };
                        DialogResult = true;
                    }
                }
                catch (Exception exp)
                {
                    error.Text = exp.Message;
                }
            }
        }

        internal class Result
        {
            public string msg;
            public string res;
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Ok_Button_Click(sender, e);

            if (e.Key == Key.Escape)
                CancelButton_Click(sender, e);
        }

        private void Help_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start($"https://lantsev1981.ru/{CultureInfo.CurrentUICulture.Name.Substring(0, 2)}/wiki/index/99e1439a-0f0b-4a85-8085-ff8c355c4d81#errorList");
            }
            catch { }
        }
    }
}
