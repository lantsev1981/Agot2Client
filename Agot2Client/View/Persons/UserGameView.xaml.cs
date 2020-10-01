using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GamePortal;
using System.Windows.Threading;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для UserGameView.xaml
    /// </summary>
    public partial class UserGameView : UserControl
    {
        public UserGameViewModel Value { get; private set; }

        public UserGameView()
        {
            InitializeComponent();
            DataContextChanged += UserGameView_DataContextChanged;
        }

        void UserGameView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Value = (UserGameViewModel)e.NewValue;
        }

        private void PassRate_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            MainWindow.GamePortal.PassRateAsync(Value.Model.Id, () =>
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var game = MainWindow.GamePortal.User.EndedUserGames.Single(p => p.Id == Value.Model.Id);
                    DataContext = new UserGameViewModel(game);
                }), DispatcherPriority.ApplicationIdle);
            });
        }
    }

    public class UserGameViewModel
    {
        public WCFUserGame Model { get; private set; }

        public string ToolTipText { get; private set; }
        public Brush Background { get; private set; }
        public string MindRate { get; private set; }
        public string HonorRate { get; private set; }
        public ExtHomeType HomeType { get; private set; }

        public UserGameViewModel(WCFUserGame model)
        {
            Model = model;
            HomeType = MainWindow.ClientInfo.WorldData.HomeType.Single(p1 => p1.WCFHomeType.Name == Model.HomeType);
            MindRate = model.MindPosition == 1 ? "100" : "0";
            HonorRate = model.HonorPosition == 5 ? "100" : "0";

            //игра не доигранна
            if (Model.MindPosition == 0)
            {
                if (Model.IsIgnoreHonor)
                {
                    ToolTipText = App.GetResources("hint_ignoreHonor");
                    Background = new SolidColorBrush(Colors.Yellow) { Opacity = .1 };
                    MindRate = "";
                    HonorRate = "";
                }
                else
                {
                    ToolTipText = App.GetResources("hint_mindZero");
                    Background = new SolidColorBrush(Colors.Orange) { Opacity = .1 };
                }
            }
            else
            {
                if (Model.IsIgnoreMind)
                {
                    ToolTipText = App.GetResources("hint_ignoreMind");
                    Background = new SolidColorBrush(Colors.Green) { Opacity = .1 };
                }
                else
                {
                    ToolTipText = App.GetResources("hint_goodGame");
                    Background = new SolidColorBrush(Colors.WhiteSmoke) { Opacity = .1 };
                }
            }
        }
    }
}
