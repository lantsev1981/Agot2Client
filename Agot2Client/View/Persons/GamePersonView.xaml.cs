using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для GamePersonView.xaml
    /// </summary>
    public partial class GamePersonView : UserControl
    {
        public ExtGame Game { get; private set; }

        private ObservableCollection<GamePersonItemViewModel> Items = new ObservableCollection<GamePersonItemViewModel>();

        public GamePersonView()
        {
            InitializeComponent();
            usersMeetList.ItemsSource = Items;
            ClientInfo.ClientGameChanging += ClientInfo_ClientGameChanging;
        }

        private void ClientInfo_ClientGameChanging(ExtGame game)
        {
            if (Game != null)
            {
                Game.GameUserEvent -= ExtGame_GameUserEvent;
            }
            if (game != null)
            {
                Game = game;
                Items.Clear();
                game.ExtGameUser.Where(p => p.WCFGameUser.Login != null && p.WCFGameUser.Login != "Вестерос").ToList().ForEach(p => ExtGame_GameUserEvent(p, true));
                Game.GameUserEvent += ExtGame_GameUserEvent;
            }

        }

        private void ExtGame_GameUserEvent(ExtGameUser user, bool isAdd)
        {
            if (isAdd)
            {
                if (user.WCFGameUser.HomeType == null)
                    Items.Add(new GamePersonItemViewModel() { GameUserId = user.WCFGameUser.Id, User = user.GPUser });
                else
                    Items.Insert(0, new GamePersonItemViewModel() { GameUserId = user.WCFGameUser.Id, User = user.GPUser, HomeType = user.ExtHomeType });
            }
            else
                Items.Remove(Items.SingleOrDefault(p => p.GameUserId == user.WCFGameUser.Id));
        }
    }
}
