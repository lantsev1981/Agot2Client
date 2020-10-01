using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для NewGameMenuView.xaml
    /// </summary>
    public partial class NewGameMenuView : UserControl
    {
        public NewGameMenuView()
        {
            InitializeComponent();
        }

        private void NewGameButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.IsEnabled = false;
            MainWindow.ClientInfo.GameSettings.GameType = MainWindow.GameTypes[gameTypeCB.SelectedIndex].Id;
            NewGameTask.AddTask();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.ClientInfo.GameSettings.Lang = ((CheckBox)sender).IsChecked == true ? App.Settings.Value.Lang : null;
            MainWindow.ClientInfo.OnPropertyChanged("LangImage");
        }

        //TODO Storyboard by Code
        /*var StartAnimation = new Storyboard();

        var showAnimation = new DoubleAnimation(-550, -410, new Duration(new TimeSpan(0, 0, 1)));
        showAnimation.BeginTime = new TimeSpan(0, 0, 5);
        Storyboard.SetTargetName(showAnimation, "YPos");
        Storyboard.SetTargetProperty(showAnimation, new PropertyPath(TranslateTransform.YProperty));
        StartAnimation.Children.Add(showAnimation);

        showAnimation = new DoubleAnimation(0, 1, new Duration(new TimeSpan(0, 0, 2)));
        Storyboard.SetTargetName(showAnimation, "rootGrid");
        Storyboard.SetTargetProperty(showAnimation, new PropertyPath(FrameworkElement.OpacityProperty));
        StartAnimation.Children.Add(showAnimation);

        StartAnimation.Begin(this);*/
    }
}
