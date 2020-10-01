using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GameService;
using MyMod;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для UserControlGame.xaml
    /// </summary>
    public partial class GameItemView2 : UserControl
    {
        GameItemViewModel _ViewModel;
        public GameItemView2()
        {
            this.InitializeComponent();
            DataContextChanged += GameItemView2_DataContextChanged;
        }

        void GameItemView2_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _ViewModel = (GameItemViewModel)e.NewValue;
        }

        private void PlayBtnClick(object sender, RoutedEventArgs e)
        {
            if (_ViewModel.Model.GameUser.Any(p => !string.IsNullOrEmpty(p.HomeType) && p.Login == MainWindow.GamePortal.User.Login))
            {
                MainWindow.ClientInfo.HomeSelected = null;
                MainWindow.ClientInfo.ClientGameId = _ViewModel.Model.Id;
            }
            else
            {
                App.Agot2.homeSelectView.Load(_ViewModel.Model);
            }
        }

        private void ViewBtnClick(object sender, RoutedEventArgs e)
        {
            MainWindow.ClientInfo.HomeSelected = null;
            MainWindow.ClientInfo.ClientGameId = _ViewModel.Model.Id;
        }

        private void TextBlock_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            YandexTranslate.Translate(_ViewModel.Model.Settings.Name, (str) =>
            {
                _ViewModel.Model.Settings.Name = str;
                _ViewModel.OnPropertyChanged("Model");
            });
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_ViewModel.User.Api.TryGetValue("FIO", out string fio))
            {
                YandexTranslate.Translate(fio, (str) =>
                {
                    _ViewModel.User.Api["FIO"] = str;
                    _ViewModel.User.OnPropertyChanged("Api");
                    MainWindow.GamePortal.UpdateTitle(_ViewModel.User);
                });
            }
        }
    }

    public class GameItemViewModel : MyNotifyObj
    {
        public WCFGame Model { get; private set; }
        public GPUser User { get; private set; }

        public string MindText
        {
            get { return string.Format("- {0} >= {1}", App.GetResources("ratingType_mind"), Model.Settings.RateSettings.MindRate); }
        }

        public string HonorText
        {
            get { return string.Format("- {0} >= {1}", App.GetResources("ratingType_honor"), Model.Settings.RateSettings.HonorRate); }
        }

        public string LikeText
        {
            get { return string.Format("- {0} >= {1}", App.GetResources("ratingType_like"), Model.Settings.RateSettings.LikeRate); }
        }

        public string MaxTime
        {
            get { return $"- max = {Model.Settings.MaxTime} sec"; }
        }

        public string AddTime
        {
            get { return $"- plus = {Model.Settings.AddTime} sec"; }
        }

        /*public string AbsoluteText
        {
            get { return string.Format("- {0} >= {1}", App.GetResources("ratingType_total"), Model.Settings.RateSettings.DurationRate); }
        }*/

        public string RandomText
        {
            get { return string.Format("{0} = {1}", App.GetResources("text_randomPower"), Model.Settings.RandomIndex); }
        }

        public GameTypeItem GameTypeItem { get; set; }

        public GameItemViewModel(WCFGame model)
        {
            Model = model;
            GameTypeItem = MainWindow.GameTypes.Single(p => p.Id == model.Settings.GameType);
            User = MainWindow.GamePortal.GetUser(model.Settings.CreatorLogin);
            UpdateGameStatus();
        }

        public GameStatusEnum GameStatus { get; private set; }

        public void Update(WCFGame p)
        {
            Model = p;
            UpdateGameStatus();
            this.OnPropertyChanged("");
        }

        private void UpdateGameStatus()
        {
            if (Model.OpenTime == null)
            {
                if (!MainWindow.GamePortal.User.CheckRateAllow(Model.Settings.RateSettings))
                    GameStatus = GameStatusEnum.Block;
                else
                    GameStatus = GameStatusEnum.New;
            }
            else if (Model.CloseTime != null)
                GameStatus = GameStatusEnum.Close;
            else if (Model.GameUser.Any(p =>!p.IsCapitulated && !string.IsNullOrEmpty(p.HomeType) && string.IsNullOrEmpty(p.Login)))
                GameStatus = GameStatusEnum.Replace;
            else
                GameStatus = GameStatusEnum.Open;
        }
    }

    public enum GameStatusEnum
    {
        Replace,
        New,
        Block,
        Open,
        Close
    }
}