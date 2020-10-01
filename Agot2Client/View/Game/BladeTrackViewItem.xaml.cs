using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для BladeTrackViewItem.xaml
    /// </summary>
    public partial class BladeTrackViewItem : UserControl
    {
        ExtGameUserInfo _GameUserInfo;

        public BladeTrackViewItem()
        {
            InitializeComponent();
            DataContextChanged += _DataContextChanged;
        }

        void _DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _GameUserInfo = (ExtGameUserInfo)DataContext;
        }

        private void _Drop(object sender, DragEventArgs e)
        {
            if (_GameUserInfo.Step.Game.ClientStep == null)
                return;

            ExtGameUserInfo gameUserInfo = (e.Data.GetData(typeof(ExtGameUserInfo)) as ExtGameUserInfo);
            if (_GameUserInfo.Step.Game.ClientStep.WCFStep.Voting.Target != "Вотчины"
                || gameUserInfo == null || _GameUserInfo.BladeVoting != gameUserInfo.BladeVoting)
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_power"));
                return;
            }

            int curentBladeInfluence = _GameUserInfo.WCFGameUserInfo.BladeInfluence;
            int dropBladeInfluence = gameUserInfo.WCFGameUserInfo.BladeInfluence;
            _GameUserInfo.WCFGameUserInfo.BladeInfluence = dropBladeInfluence;
            gameUserInfo.WCFGameUserInfo.BladeInfluence = curentBladeInfluence;
            _GameUserInfo.OnPropertyChanged("BladePosition");
            gameUserInfo.OnPropertyChanged("BladePosition");

            StringBuilder sb = new StringBuilder();
            foreach (var item in _GameUserInfo.Step.Game.ViewGameUserInfo.OrderBy(p => p.WCFGameUserInfo.BladeInfluence))
            {
                sb.Append(item.Step.ExtGameUser.WCFGameUser.HomeType);
                sb.Append('|');
            }

            _GameUserInfo.Step.Game.ClientStep.WCFStep.Raven.StepType = sb.ToString();
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
            if (_GameUserInfo.Step.Game.ClientStep == null)
                return;

            if (_GameUserInfo.Step.Game.ClientStep.WCFStep.StepType == "Железный_трон")
                if (_GameUserInfo.Step.Game.ClientStep.WCFStep.Voting.Target == "Вотчины")
                    DragDrop.DoDragDrop(this, _GameUserInfo, DragDropEffects.All);
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_GameUserInfo.Step.Game.ClientStep == null)
                return;

            _GameUserInfo.Step.Game.ClientStep.WCFStep.Raven.StepType = _GameUserInfo.Step.ExtGameUser.WCFGameUser.HomeType;
            GameView.CompleteStep(_GameUserInfo.Step.Game);
        }
    }
}
