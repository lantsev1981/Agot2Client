using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GameService;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для UnitView.xaml
    /// </summary>
    public partial class UnitView : UserControl
    {
        ExtUnit _Unit;
        public UnitView()
        {
            InitializeComponent();
            DataContextChanged += UnitView_DataContextChanged;
        }

        void UnitView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _Unit = (ExtUnit)e.NewValue;
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_Unit.Step.Game.ClientStep == null)
                return;

            if (_Unit.Step.Game.ClientStep.WCFStep.StepType == "dragon_Ser_Ilyn_Payne")
            {
                if (_Unit.WCFUnit.UnitType != "Пеший_воин")
                {
                    App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_ranly1"));
                    return;
                }
                
                _Unit.Step.Game.ClientStep.WCFStep.BattleUser.AdditionalEffect = _Unit.WCFUnit.Id.ToString();
                GameView.CompleteStep(_Unit.Step.Game);
                return;
            }

            //Только владелец в свой ход
            if (_Unit.Step != _Unit.Step.Game.ClientStep)
                return;

            switch (_Unit.Step.WCFStep.StepType)
            {
                case "Поход":
                    if (!_Unit.WCFUnit.IsWounded
                        && _Unit.Step.Game.SelectedOrder != null
                        && _Unit.WCFUnit.Terrain == _Unit.Step.Game.SelectedOrder.WCFOrder.Terrain)
                        _Unit.IsSelected = !_Unit.IsSelected;
                    break;

                case "Робб_Старк":
                case "Отступление":
                    if (_Unit.WCFUnit.Terrain == _Unit.Step.Game.ViewGameInfo.ExtBattle.WCFBattle.DefenceTerrain)
                        _Unit.IsSelected = !_Unit.IsSelected;
                    break;

                case "Убийцы_ворон":
                case "Наездники_на_мамонтах_роспуск_войск":
                case "Наступление_орды":
                case "Передовой_отряд":
                case "Роспуск_войск":
                    ExtMarchUnit result = _Unit.Step.ExtMarch.ExtMarchUnit.SingleOrDefault(p => p.ExtUnit == _Unit);
                    if (result != null)
                    {
                        _Unit.IsSelected = !_Unit.IsSelected;
                        result.WCFMarchUnit.UnitType = _Unit.IsSelected ? null : _Unit.WCFUnit.UnitType;
                    }
                    break;

                case "Ренли_Баратеон":
                    if (_Unit.WCFUnit.UnitType != "Пеший_воин")
                    {
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_ranly1"));
                        return;
                    }

                    if (!_Unit.Step.WCFStep.BattleUser.AdditionalEffect.Contains(_Unit.WCFUnit.Id.ToString()))
                    {
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_ranly2"));
                        break;
                    }

                    _Unit.IsSelected = !_Unit.IsSelected;
                    _Unit.Step.WCFStep.BattleUser.AdditionalEffect = _Unit.WCFUnit.Id.ToString();
                    GameView.CompleteStep(_Unit.Step.Game);
                    break;
            }
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            if (Keyboard.Modifiers == ModifierKeys.Control && e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(this, (IPosition)_Unit, DragDropEffects.All);
                return;
            }

            if (!_Unit.IsSelected)
                return;

            if (_Unit.Step.Game.ClientStep == null)
                return;

            switch (_Unit.Step.Game.ClientStep.WCFStep.StepType)
            {
                case "Поход":
                case "Робб_Старк":
                case "Отступление":
                    if (e.LeftButton == MouseButtonState.Pressed)
                        DragDrop.DoDragDrop(this, _Unit.ExtTerrain, DragDropEffects.All);
                    break;
            }

        }

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;

            if (TerrainView.UpdateGamePoint(e, this))
                return;

            if (_Unit.Step.Game.SelectedOrder == null || _Unit.Step.Game.SelectedOrder.WCFOrder.OrderType != "Усиление_власти_0_специальный")
                return;

            //только тип юнита
            ExtUnitType dropUnitType = (e.Data.GetData(typeof(ExtUnitType)) as ExtUnitType);
            if (dropUnitType == null)
                return;

            if (_Unit.Step.Game.SelectedOrder.ExtTerrain != _Unit.ExtTerrain)
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_consolidate5"));
                return;
            }
            if (_Unit.WCFUnit.UnitType != "Пеший_воин")
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_consolidate6"));
                return;
            }
            if (dropUnitType.WCFUnitType.Name == "Пеший_воин")
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"),App.GetResources("validation_consolidate7") );
                return;
            }
            if (dropUnitType.WCFUnitType.Name == "Корабль")
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_consolidate8"));
                return;
            }

            _Unit.TempUnitType = dropUnitType;

            WCFMarchUnit WCFMarchUnit = _Unit.Step.Game.ClientStep.ExtMarch.WCFMarch.MarchUnit.SingleOrDefault(p => p.Unit == _Unit.WCFUnit.Id);
            if (WCFMarchUnit == null)
            {
                WCFMarchUnit = new WCFMarchUnit();
                _Unit.Step.Game.ClientStep.ExtMarch.WCFMarch.MarchUnit.Add(WCFMarchUnit);
            }

            WCFMarchUnit.Unit = _Unit.WCFUnit.Id;
            WCFMarchUnit.Terrain = _Unit.TempTerrain.WCFTerrain.Name;
            WCFMarchUnit.UnitType = _Unit.TempUnitType.WCFUnitType.Name;

            _Unit.Step.Game.ClientStep.ExtMarch.SumCost++;
        }
    }
}
