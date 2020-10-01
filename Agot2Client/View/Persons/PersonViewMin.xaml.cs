using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для PersonView.xaml
    /// </summary>
    public partial class PersonViewMin : UserControl
    {
        GamePersonItemViewModel _ViewModel;
        public PersonViewMin()
        {
            InitializeComponent();

            DataContextChanged += PersonView_DataContextChanged;
        }

        void PersonView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _ViewModel = e.NewValue as GamePersonItemViewModel;
        }

        private void Block_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GamePortal.CheckSpecialUser(_ViewModel.User, true);
        }

        private void Friend_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GamePortal.CheckSpecialUser(_ViewModel.User, false);
        }

        private void Invite_Button_Click(object sender, RoutedEventArgs e)
        {
            var model = new SendMessageViewModel()
            {
                Login = _ViewModel.User.Login,
                TextLength = 250,
                AccountType = AccountType.agot
            };
            App.Agot2.sendMessageView.Load(model);
        }

        private void Mute_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GamePortal.UserMute(_ViewModel.User);
        }

        private void HomesButton_Click(object sender, RoutedEventArgs e)
        {
            App.Agot2.homesForm.DataContext = _ViewModel.User;
            App.Agot2.homesForm.Visibility = Visibility.Visible;
        }

        private void AwardsButton_Click(object sender, RoutedEventArgs e)
        {
            App.Agot2.awardsForm.DataContext = _ViewModel.User;
            App.Agot2.awardsForm.Visibility = Visibility.Visible;
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            if (_ViewModel.User.Api.TryGetValue("FIO", out string fio))
            {
                YandexTranslate.Translate(fio, (str) =>
                {
                    _ViewModel.User.Api["FIO"] = str;
                    MainWindow.GamePortal.UpdateTitle(_ViewModel.User);
                });
            }
        }
    }
}
