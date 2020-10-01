using System.Windows;
using System.Windows.Controls;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для AvatarView.xaml
    /// </summary>
    public partial class AvatarViewMin : UserControl
    {
        private GamePersonItemViewModel _ViewModel;

        public AvatarViewMin()
        {
            InitializeComponent();
            DataContextChanged += AvatarView_DataContextChanged;
        }

        private void AvatarView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _ViewModel = e.NewValue as GamePersonItemViewModel;
        }

        private void Mute_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GamePortal.UserMute(_ViewModel.User);
        }
    }
}
