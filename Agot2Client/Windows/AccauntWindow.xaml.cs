using System.Windows;
using System.Windows.Input;
using GamePortal;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для AccauntWindow.xaml
    /// </summary>

    public partial class AccauntWindow : Window
    {
        public AccauntWindow()
        {
            InitializeComponent();
            login.Focus();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            if (string.IsNullOrEmpty(login.Text))
                return;

            App.Settings.Value.User = new user() { login = login.Text };
            this.DialogResult = true;
#endif
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                OKButton_Click(sender, e);
            if (e.Key == Key.Escape)
                CancelButton_Click(sender, e);
        }

        private void VKButton_Click(object sender, RoutedEventArgs e)
        {
            login.Text = null;
            this.DialogResult = true;
        }

        private void FBButton_Click(object sender, RoutedEventArgs e)
        {
            login.Text = null;
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
