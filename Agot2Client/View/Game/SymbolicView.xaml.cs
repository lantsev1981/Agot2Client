using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для SymbolicView.xaml
    /// </summary>
    public partial class SymbolicView : UserControl
    {
        ExtSymbolic _Symbolic;

        public SymbolicView()
        {
            InitializeComponent();
            DataContextChanged += SymbolicView_DataContextChanged;
        }

        void SymbolicView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _Symbolic = (ExtSymbolic)DataContext;
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var clientStep = _Symbolic.Game.ClientStep;
            if (clientStep == null)
                return;

            switch (clientStep.WCFStep.StepType)
            {
                case "dragon_Ser_Gerris_Drinkwater":
                case "Доран_Мартелл":
                    clientStep.ExtBattleUser.WCFBattleUser.AdditionalEffect = _Symbolic.WCFSymbolic.Name;
                    GameView.CompleteStep(_Symbolic.Game);
                    break;

                case "Посыльный_ворон":
                    switch (_Symbolic.WCFSymbolic.Name)
                    {
                        case "Посыльный_ворон":
                            clientStep.WCFStep.Raven.StepType = "Неожиданный_шаг";
                            GameView.CompleteStep(_Symbolic.Game);
                            break;
                        case "Карта_одичалых":
                            clientStep.WCFStep.Raven.StepType = "Разведка_за_стеной";
                            GameView.CompleteStep(_Symbolic.Game);
                            break;
                    }
                    break;

                default:
                    if (_Symbolic.Background is ImageBrush)
                        break;

                    clientStep.WCFStep.Raven.StepType = _Symbolic.WCFSymbolic.Name;
                    GameView.CompleteStep(_Symbolic.Game);
                    break;
            }
        }
    }
}
