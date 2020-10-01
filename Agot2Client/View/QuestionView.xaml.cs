using System;
using System.Windows;
using System.Windows.Controls;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для QuestionView.xaml
    /// </summary>
    public partial class QuestionView : UserControl
    {
        Action[] _Act;

        public QuestionView()
        {
            InitializeComponent();
        }

        public void Show(MessageBoxButton btnSettings, string text, params Action[] act)
        {
            this.panelOK.Visibility = Visibility.Collapsed;
            this.panelYesNo.Visibility = Visibility.Collapsed;
            this.panelYesNoCancel.Visibility = Visibility.Collapsed;
            switch (btnSettings)
            {
                case MessageBoxButton.OKCancel:
                case MessageBoxButton.YesNo:
                    this.panelYesNo.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNoCancel:
                    this.panelYesNoCancel.Visibility = Visibility.Visible;
                    break;
                default:
                    this.panelOK.Visibility = Visibility.Visible;
                    break;
            }

            this.text.Text = text;
            _Act = act;
            this.Visibility = Visibility.Visible;
        }

        private void YesBtnClick(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            DoAction(0);
        }

        private void NoBtnClick(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            DoAction(1);
        }

        private void CancelBtnClick(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            DoAction(2);
        }

        private void DoAction(int index)
        {
            if (_Act.Length > index)
                _Act[index]();
        }
    }
}
