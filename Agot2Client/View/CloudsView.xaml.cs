using System.Windows;
using System.Windows.Controls;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для CloudsView.xaml
    /// </summary>
    public partial class CloudsView : UserControl
    {
        public CloudsView()
        {
            InitializeComponent();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            cloudsAnimation.SpeedRatio = (1920 / this.ActualWidth);
        }
    }
}
