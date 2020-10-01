using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для RavenTrackViewItem.xaml
    /// </summary>
    public partial class RavenTrackViewItem : UserControl
    {
        ExtGameUserInfo _GameUserInfo;

        public RavenTrackViewItem()
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
            ExtGameUserInfo gameUserInfo = e.Data.GetData(typeof(ExtGameUserInfo)) as ExtGameUserInfo;
            if (_GameUserInfo.Step.Game.ClientStep.WCFStep.Voting.Target != "Королевский_двор"
                ||gameUserInfo == null || _GameUserInfo.RavenVoting != gameUserInfo.RavenVoting)
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"),App.GetResources("validation_power") );
                return;
            }

            int curentRavenInfluence = _GameUserInfo.WCFGameUserInfo.RavenInfluence;
            int dropRavenInfluence = gameUserInfo.WCFGameUserInfo.RavenInfluence;
            _GameUserInfo.WCFGameUserInfo.RavenInfluence = dropRavenInfluence;
            gameUserInfo.WCFGameUserInfo.RavenInfluence = curentRavenInfluence;
            _GameUserInfo.OnPropertyChanged("RavenPosition");
            gameUserInfo.OnPropertyChanged("RavenPosition");

            StringBuilder sb = new StringBuilder();
            foreach (var item in _GameUserInfo.Step.Game.ViewGameUserInfo.OrderBy(p => p.WCFGameUserInfo.RavenInfluence))
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
                if (_GameUserInfo.Step.Game.ClientStep.WCFStep.Voting.Target == "Королевский_двор")
                    DragDrop.DoDragDrop(this, _GameUserInfo, DragDropEffects.All);
        }
    }
}
