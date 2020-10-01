using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для ProfilePanel.xaml
    /// </summary>
    public partial class RightPanelView : UserControl
    {
        public ExtGame Game { get; private set; }

        public RightPanelView()
        {
            InitializeComponent();
            Loaded += RightPanelView_Loaded;

            gameInfoView.chatView.ViewModel.GetChatEvent += (count) =>
            {
#if !DEBUG
                if (tabControl.SelectedIndex == 4)
                    return; 
#endif

                int.TryParse(msgCount.Text, out int curCount);
                msgCount.Text = (curCount + count).ToString();

                var story = ((Storyboard)chatViewTabItem.Resources["animation"]);
                story.Begin();

                App.Agot2.gameChatMessage.Position = TimeSpan.Zero;
                App.Agot2.gameChatMessage.Play();
            };

            ClientInfo.ClientGameChanging += ClientInfo_ClientGameChanging;
        }

        private void RightPanelView_Loaded(object sender, RoutedEventArgs e)
        {
            SelecteMenu(5);
        }

        private void ClientInfo_ClientGameChanging(ExtGame game)
        {
            if (Game != null)
            {
                Game.ClientStepCganged -= ExtGame_ClientStepCganged;
                Game.NewWesterosPhase -= _OpenEventTab;
                Game.NewPlanningPhase -= _CloseEventsTab;
                Game.ToHomeCardEvent -= _OpenHomesTab;
                Game.BattleDataChanged -= _OpenBattleTab;
                Game.HomeCardSelected -= _OpenBattleTab;
            }
            if (game != null)
            {
                Game = game;
                Game.ClientStepCganged += ExtGame_ClientStepCganged;
                Game.NewWesterosPhase += _OpenEventTab;
                Game.NewPlanningPhase += _CloseEventsTab;
                Game.ToHomeCardEvent += _OpenHomesTab;
                Game.BattleDataChanged += _OpenBattleTab;
                Game.HomeCardSelected += _OpenBattleTab;
            }
        }

        void SelecteMenu(int selectedIndex)
        {
            tabControl.SelectedIndex = selectedIndex;
            showAnimation.Begin();
        }

        void _OpenEventTab()
        {
            SelecteMenu(1);
        }

        void _OpenHomesTab()
        {
            SelecteMenu(2);
        }
        void _OpenBattleTab()
        {
            SelecteMenu(3);
        }

        void _OpenBattleTab(bool isCompleate)
        {
            if (isCompleate)
            {
                //если бой завершён и открыта вкладка боя то закрываем вкладку боя
                if (tabControl.SelectedIndex == 3)
                    SelecteMenu(tabControl.Items.Count - 1);
            }
            //если бой не завершён и открыта вкладка ниже боя то открываем вкладку боя
            else if (tabControl.SelectedIndex > 3)
                SelecteMenu(3);
        }

        void _CloseEventsTab()
        {
            //если события вестероса разыграны и открыта вкладка событий то закрываем её
            if (tabControl.SelectedIndex == 1)
                SelecteMenu(tabControl.Items.Count - 1);
        }

        private void ExtGame_ClientStepCganged(ExtStep step)
        {
            stepName.Visibility = step == null ? Visibility.Collapsed : Visibility.Visible;
        }

        private void TabItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            msgCount.Text = string.Empty;
        }

        private void DoubleAnimation_Completed(object sender, EventArgs e)
        {
            stepName.Visibility = Visibility.Collapsed;
        }
    }
}
