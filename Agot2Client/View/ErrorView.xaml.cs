using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для ErrorView.xaml
    /// </summary>
    public partial class ErrorView : UserControl
    {
        DispatcherTimer _Timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
        public ErrorView()
        {
            InitializeComponent();
            _Timer.Tick += (s, a) =>
            {
                _Timer.Stop();
                this.Visibility = Visibility.Collapsed;
            };
        }

        public void ShowByDispatcher(string type, string text, int second = 10)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    this.typeName.Text = type;
                    this.text.Text = text;
                    this.Visibility = Visibility.Visible;

                    _Timer.Stop();
                    _Timer.Interval = TimeSpan.FromSeconds(second);
                    _Timer.Start();
                }
                catch (Exception exp)
                {
                    MessageBox.Show(exp.Message);
                }
            }), DispatcherPriority.ApplicationIdle);
        }
    }
}
