using GamePortal;
using MyMod;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для GamePersonView.xaml
    /// </summary>
    public partial class GamePersonItemView : UserControl
    {
        private GamePersonItemViewModel ViewModel;

        public GamePersonItemView()
        {
            InitializeComponent();
        }

        private void GamePersonView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ViewModel = (GamePersonItemViewModel)e.NewValue;
        }

        //TODO Переделать
        private void likeButton_Click(object sender, RoutedEventArgs e)
        {
            Button likeButton = (Button)sender;

            switch (likeButton.Tag.ToString())
            {
                case "1": MainWindow.GamePortal.LikeUserAsync(ViewModel.User, true); break;
                case "0": MainWindow.GamePortal.LikeUserAsync(ViewModel.User, false); break;
                default: MainWindow.GamePortal.LikeUserAsync(ViewModel.User, null); break;
            }
        }
    }

    public class GamePersonItemViewModel : MyNotifyObj
    {
        public Guid GameUserId { get; set; }

        private WCFUserGame _UserGame;
        public WCFUserGame UserGame
        {
            get => _UserGame;
            set { _UserGame = value; this.OnPropertyChanged("UserGame"); }
        }

        private GPUser _User;
        public GPUser User
        {
            get => _User;
            set { _User = value; this.OnPropertyChanged("GPUser"); }
        }

        private ExtHomeType _HomeType;
        public ExtHomeType HomeType
        {
            get => _HomeType;
            set { _HomeType = value; this.OnPropertyChanged("HomeType"); }
        }

        private Visibility _IsLikePanelVisible = Visibility.Collapsed;

        public Visibility IsLikePanelVisible
        {
            get => _IsLikePanelVisible;
            set { _IsLikePanelVisible = value; this.OnPropertyChanged("IsLikePanelVisible"); }
        }
    }
}
