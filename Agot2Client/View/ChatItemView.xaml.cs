using ChatServer;
using GamePortal;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для MessageView.xaml
    /// </summary>
    public partial class ChatItemView : UserControl
    {
        private ChatItemViewModel _ViewModel;

        public ChatItemView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) => _ViewModel = (ChatItemViewModel)e.NewValue;
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_ViewModel.GamePersonModel.User.Api.TryGetValue("FIO", out string fio))
            {
                YandexTranslate.Translate(fio, (str) =>
                {
                    _ViewModel.GamePersonModel.User.Api["FIO"] = str;
                    MainWindow.GamePortal.UpdateTitle(_ViewModel.GamePersonModel.User);
                });
            }
        }

        private void TextBlock_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            YandexTranslate.Translate(_ViewModel.Model.Message, (str) =>
            {
                _ViewModel.Model.Message = str;
                _ViewModel.OnPropertyChanged("Model");
            });
        }
    }

    public class ChatItemViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        public Chat Model { get; private set; }
        public GamePersonItemViewModel GamePersonModel { get; private set; }

        public ChatItemViewModel(Chat model, ExtGame game)
        {
            Model = model;
            GamePersonModel = new GamePersonItemViewModel();

            try
            {
                GamePersonModel.User = model.Creator == "Вестерос" ? MainWindow.GamePortal.Vesteros : MainWindow.GamePortal.GetUser(model.Creator);
            }
            catch
            {
                //если пользователь не найден (написали с сайта)
                GamePersonModel.User = new GPUser(new WCFUser() { Login = "Вестерос" }) { Title = $"{App.GetResources("titleType_Ghost")}, {model.Creator}" };
            }

            if (game != null)
            {
                //ищем игру в истории профиля
                GamePersonModel.HomeType = game.ExtGameUser.SingleOrDefault(p => p.WCFGameUser.Login == Model.Creator)?.ExtHomeType;
                if (GamePersonModel.HomeType == null)
                {
                    string hometype = GamePersonModel.User.UserGames.SingleOrDefault(p => p.GameId == game.WCFGame.Id && model.Time > p.StartTime && (!p.EndTime.HasValue || model.Time < p.EndTime))?.HomeType;
                    if (hometype != null)
                        GamePersonModel.HomeType = MainWindow.ClientInfo.WorldData.HomeType.Single(p => p.WCFHomeType.Name == hometype);
                }

                //string hometype = GamePersonModel.User.UserGames.SingleOrDefault(p => p.GameId == game.WCFGame.Id && model.Time > p.StartTime && (!p.EndTime.HasValue || model.Time < p.EndTime))?.HomeType;
                //GamePersonModel.HomeType = hometype != null? MainWindow.ClientInfo.WorldData.HomeType.SingleOrDefault(p => p.WCFHomeType.Name == hometype)
                //    : game.ExtGameUser.SingleOrDefault(p => p.WCFGameUser.Login == Model.Creator)?.ExtHomeType;

                //viewModel.ImageName = hometype != null
                //    ? $"/image/{ExtHomeType.Keys[hometype]}/{ExtHomeType.Keys[hometype]}.png"
                //    : Game?.ExtGameUser.SingleOrDefault(p => p.WCFGameUser.Login == viewModel.Model.Creator)?.ExtHomeType?.ImageName;
            }

            Model.IsVisible = GamePersonModel.User.ChatVisibility == Visibility.Visible ? true : false;
        }
    }
}
