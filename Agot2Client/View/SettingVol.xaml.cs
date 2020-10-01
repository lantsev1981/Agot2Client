using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Agot2Client.View
{
    /// <summary>
    /// Логика взаимодействия для SettingVol.xaml
    /// </summary>
    public partial class SettingVol : UserControl
    {
        public SettingVol()
        {
            InitializeComponent();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.OldValue == 0) return;

            test.Stop();
            test.Position = TimeSpan.Zero;
            test.Play();
        }

        private void Slider_MouseLeave(object sender, MouseEventArgs e)
        {
            test.Stop();
        }
    }
}
