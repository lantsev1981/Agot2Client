using System.Windows;
using System.Windows.Controls;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для AvatarView.xaml
    /// </summary>
    public partial class AvatarView : UserControl
    {
        GPUser _ViewModel;

        public AvatarView()
        {
            InitializeComponent();
            DataContextChanged += AvatarView_DataContextChanged;

            //if (MainWindow.GamePortal.User.Api["isFacebook"] == "True")
            //    vkMsgGrid.Visibility = Visibility.Collapsed;
        }

        void AvatarView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var user = e.NewValue as GPUser;
            if (user == null)
                return;

            _ViewModel = user;
        }
        //private void VK_Button_Click(object sender, RoutedEventArgs e)
        //{
        //    if (_ViewModel.Api.ContainsKey("isFacebook") && _ViewModel.Api["isFacebook"] == "True")
        //    {
        //        return;
        //    }

        //    if (string.IsNullOrEmpty(_ViewModel.Login))
        //    {
        //        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("error_sendMessage"));
        //        return;
        //    }

        //    var model = new SendMessageViewModel()
        //    {
        //        Login = _ViewModel.Login == "Вестерос" ? "147751339" : _ViewModel.Api["uid"],
        //        TextLength = 250,
        //        AccountType = AccountType.vk
        //    };
        //    App.Agot2.sendMessageView.Load(model);
        //}

        private void Mute_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GamePortal.UserMute(_ViewModel);
        }
    }
}
