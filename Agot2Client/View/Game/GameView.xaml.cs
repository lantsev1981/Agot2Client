using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для GameView.xaml
    /// </summary>
    public partial class GameView : UserControl
    {

        public GameView()
        {
            InitializeComponent();
        }

        static public void CompleteStep(ExtGame game)
        {
            if (game == null || game.ClientStep == null)
                return;

            if (game.ClientStep.WCFStep.StepType == "Замысел")
            {
                if (game.ClientStep.WCFStep.GameUserInfo.Order.Any(p => p.OrderType == null))
                {
                    App.Agot2.questionView.Show(MessageBoxButton.YesNo, App.GetResources("text_orderFree"), () => SendStepTask.AddTask(game));
                    return;
                }
            }


            if (game.ClientStep.CheckStep())
            {
                if (game.ClientStep.AttackTerrain?.SingleOrDefault(p => p.ExtHolderUser != null)?
                    .JoinTerrainCol.Any(p => p.Order?.ExtOrderType.WCFOrderType.DoType == "Подмога") ?? false)
                {
                    App.Agot2.questionView.Show(MessageBoxButton.YesNo, App.GetResources("text_RequestSupport"),
                          () => { game.ClientStep.WCFStep.IsNeedSupport = true; SendStepTask.AddTask(game); },
                          () => { game.ClientStep.WCFStep.IsNeedSupport = false; SendStepTask.AddTask(game); });
                    return;
                }

                SendStepTask.AddTask(game);
            }
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ///ToolTip qwerty = new System.Windows.Controls.ToolTip();
            
        }
    }
}
