using System.Windows;
using System.Windows.Controls;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для VesterosView.xaml
    /// </summary>
    public partial class VesterosView : UserControl
    {
        public VesterosView()
        {
            InitializeComponent();
        }

        private void VkMessage_Button_Click(object sender, RoutedEventArgs e)
        {
            var model = new SendMessageViewModel()
            {
                Login = "147751339",
                TextLength = 250,
                AccountType = AccountType.vk
            };
            App.Agot2.sendMessageView.Load(model);
        }

        void donateButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GamePortal.Donate();
        }

        private void Invite_Button_Click(object sender, RoutedEventArgs e)
        {
            var model = new SendMessageViewModel()
            {
                Login = "263d269a-5671-4887-b29a-47192bd5385a",
                TextLength = 250,
                AccountType = AccountType.agot
            };
            App.Agot2.sendMessageView.Load(model);
        }
        private void HomesButton_Click(object sender, RoutedEventArgs e)
        {
            App.Agot2.homesForm.DataContext = DataContext;
            App.Agot2.homesForm.Visibility = Visibility.Visible;
        }
    }
}
