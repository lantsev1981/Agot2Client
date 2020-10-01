using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для PersonView.xaml
    /// </summary>
    public partial class PersonView : UserControl
    {
        GPUser user;
        public PersonView()
        {
            InitializeComponent();

            DataContextChanged += PersonView_DataContextChanged;
        }

        void PersonView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            user = (GPUser)e.NewValue;
        }

        private void Block_Button_Click(object sender, RoutedEventArgs e)
        {
            var user = (GPUser)((Button)sender).DataContext;
            MainWindow.GamePortal.CheckSpecialUser(user, true);
        }

        private void Friend_Button_Click(object sender, RoutedEventArgs e)
        {
            var user = (GPUser)((Button)sender).DataContext;
            MainWindow.GamePortal.CheckSpecialUser(user, false);
        }

        private void Invite_Button_Click(object sender, RoutedEventArgs e)
        {
            var user = (GPUser)((Button)sender).DataContext;

            var model = new SendMessageViewModel()
            {
                Login = user.Login,
                TextLength = 250,
                AccountType = AccountType.agot
            };
            App.Agot2.sendMessageView.Load(model);
        }

        private void Mute_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GamePortal.UserMute(user);
        }


        private void HomesButton_Click(object sender, RoutedEventArgs e)
        {
            App.Agot2.homesForm.DataContext = user;
            App.Agot2.homesForm.Visibility = Visibility.Visible;
        }

        private void AwardsButton_Click(object sender, RoutedEventArgs e)
        {
            App.Agot2.awardsForm.DataContext = user;
            App.Agot2.awardsForm.Visibility = Visibility.Visible;
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            if (user.Api.TryGetValue("FIO", out string fio))
            {
                YandexTranslate.Translate(fio, (str) =>
                {
                    user.Api["FIO"] = str;
                    MainWindow.GamePortal.UpdateTitle(user);
                });
            }
        }
    }
}
