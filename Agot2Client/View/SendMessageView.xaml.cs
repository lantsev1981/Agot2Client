using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для SendMessageView.xaml
    /// </summary>
    public partial class SendMessageView : UserControl
    {
        SendMessageViewModel Model;

        public SendMessageView()
        {
            InitializeComponent();
        }

        public void Load(SendMessageViewModel model)
        {
            Model = model;

            this.DataContext = Model;
            this.Visibility = Visibility.Visible;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;

            switch (Model.AccountType)
            {
                case AccountType.agot:
                    MainWindow.GamePortal.InviteUserAsync(Model.Login, Model.Text);
                    break;
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                okButton_Click(sender, e);
            if (e.Key == Key.Escape)
                cancelButton_Click(sender, e);
        }
    }

    public class SendMessageViewModel
    {
        public int TextLength { get; set; }
        public string Text { get; set; }
        public string Login { get; set; }
        public AccountType AccountType { get; set; }
    }

    public enum AccountType
    {
        agot = 0,
        vk = 1
    }
}
