using System.Windows;
using System.Windows.Controls;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для HomesForm.xaml
    /// </summary>
    public partial class HomesForm : UserControl
    {
        public HomesForm()
        {
            InitializeComponent();
        }

        private void okBtnClick(object sender, RoutedEventArgs e)
        {
            App.Agot2.homesForm.Visibility = Visibility.Collapsed;
        }
    }
}
