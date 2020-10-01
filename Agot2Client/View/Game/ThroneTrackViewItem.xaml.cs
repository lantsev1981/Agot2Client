using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для TrackViewItem.xaml
    /// </summary>
    public partial class ThroneTrackViewItem : UserControl
    {
        ExtGameUserInfo _GameUserInfo;

        public ThroneTrackViewItem()
        {
            InitializeComponent();
            DataContextChanged += TrackViewItem_DataContextChanged;
        }

        void TrackViewItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _GameUserInfo = (ExtGameUserInfo)DataContext;
        }

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            ExtGameUserInfo gameUserInfo = (e.Data.GetData(typeof(ExtGameUserInfo)) as ExtGameUserInfo);

            if (_GameUserInfo.Step.Game.ClientStep.WCFStep.Voting.Target != "Железный_трон" 
                || gameUserInfo == null || _GameUserInfo.ThroneVoting != gameUserInfo.ThroneVoting)
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_power"));
                return;
            }

            int curentThroneInfluence = _GameUserInfo.WCFGameUserInfo.ThroneInfluence;
            int dropThroneInfluence = gameUserInfo.WCFGameUserInfo.ThroneInfluence;
            _GameUserInfo.WCFGameUserInfo.ThroneInfluence = dropThroneInfluence;
            gameUserInfo.WCFGameUserInfo.ThroneInfluence = curentThroneInfluence;
            _GameUserInfo.OnPropertyChanged("ThronePosition");
            gameUserInfo.OnPropertyChanged("ThronePosition");

            StringBuilder sb = new StringBuilder();
            foreach (var item in _GameUserInfo.Step.Game.ViewGameUserInfo.OrderBy(p => p.WCFGameUserInfo.ThroneInfluence))
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
                if (_GameUserInfo.Step.Game.ClientStep.WCFStep.Voting.Target == "Железный_трон")
                    DragDrop.DoDragDrop(this, _GameUserInfo, DragDropEffects.All);
        }
    }
}
