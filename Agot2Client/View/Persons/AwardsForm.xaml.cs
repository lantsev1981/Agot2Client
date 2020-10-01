using System.Windows;
using System.Windows.Controls;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для AwardsForm.xaml
    /// </summary>
    public partial class AwardsForm : UserControl
    {
        public AwardsForm()
        {
            InitializeComponent();
        }

        private void okBtnClick(object sender, RoutedEventArgs e)
        {
            App.Agot2.awardsForm.Visibility = Visibility.Collapsed;
        }
    }
}
