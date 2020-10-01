using System.Windows.Controls;
using System.Windows.Input;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для DonateInfoView.xaml
    /// </summary>
    public partial class DonateInfoView : UserControl
    {
        public DonateInfoView()
        {
            InitializeComponent();
        }

        private void Comment_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            YandexTranslate.Translate(MainWindow.GamePortal.MasterOfDonate.DonateComment, (str) =>
            {
                MainWindow.GamePortal.MasterOfDonate.DonateComment = str;
                MainWindow.GamePortal.MasterOfDonate.OnPropertyChanged("DonateComment");
            });
        }
    }
}
