using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GameService;
using System.Windows.Threading;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для TerrainView.xaml
    /// </summary>
    public partial class TerrainView : UserControl
    {
        ExtTerrain _Terrain;

        public TerrainView()
        {
            InitializeComponent();
            DataContextChanged += TerrainView_DataContextChanged;
        }

        void TerrainView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _Terrain = (ExtTerrain)e.NewValue;
        }

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            if (TerrainDrop(e)) return;
            if (UnitTypeDrop(e)) return;
        }

        private bool UnitTypeDrop(DragEventArgs e)
        {
            ExtUnitType dropUnitType = (e.Data.GetData(typeof(ExtUnitType)) as ExtUnitType);
            if (dropUnitType == null)
                return false;

            if(_Terrain.ExtGarrison!=null && _Terrain.ExtHolderUser != _Terrain.Game.ClientGameUser)
                return true;

            if (dropUnitType.WCFUnitType.Cost > (_Terrain.Game.SelectedOrder.ExtTerrain.WCFTerrain.Strength - _Terrain.Game.ClientStep.ExtMarch.SumCost))
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_consolidate1"));
                return true;
            }

            if (dropUnitType.WCFUnitType.Name != "Корабль" && _Terrain.Game.SelectedOrder.ExtTerrain != _Terrain)
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_consolidate2"));
                return true;
            }

            if (dropUnitType.WCFUnitType.Name == "Корабль")
            {
                if (!_Terrain.Game.SelectedOrder.ExtTerrain.JoinTerrainCol.Contains(_Terrain) || _Terrain.WCFTerrain.TerrainType == "Земля")
                {
                    App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_consolidate3"));
                    return true;
                }
                if (_Terrain.ExtHolderUser != null && _Terrain.ExtHolderUser != _Terrain.Game.ClientGameUser)
                {
                    App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_consolidate4"));
                    return true;
                }
            }

            WCFUnit unit = new WCFUnit();
            unit.UnitType = dropUnitType.WCFUnitType.Name;
            unit.Terrain = _Terrain.WCFTerrain.Name;
            unit.Step = _Terrain.Game.ClientStep.WCFStep.Id;

            ExtUnit extUnit = new ExtUnit(_Terrain.Game.ClientStep, unit);
            if (!CheckMove(extUnit, _Terrain)) return true;

            WCFMarchUnit wcfMarchUnit = new WCFMarchUnit();
            wcfMarchUnit.Unit = extUnit.WCFUnit.Id;
            wcfMarchUnit.Terrain = extUnit.TempTerrain.WCFTerrain.Name;
            wcfMarchUnit.UnitType = extUnit.TempUnitType.WCFUnitType.Name;

            _Terrain.Game.ClientStep.ExtGameUserInfo.ExtUnit.Add(extUnit);
            _Terrain.Game.ClientStep.ExtMarch.WCFMarch.MarchUnit.Add(wcfMarchUnit);
            _Terrain.Game.ClientStep.ExtMarch.SumCost += dropUnitType.WCFUnitType.Cost;
            return true;
        }

        static public bool UpdateGamePoint(DragEventArgs e, IInputElement sender)
        {
            if (MainWindow.GamePortal.IsAdmin)
            {
                //Изменение точки
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    var item = e.Data.GetData(typeof(ExtUnit)) as IPosition;
                    if (item == null)
                        item = e.Data.GetData(typeof(ExtOrder)) as IPosition;
                    if (item == null)
                        item = e.Data.GetData(typeof(ExtPowerCounter)) as IPosition;
                    if (item == null)
                        item = e.Data.GetData(typeof(ExtGarrison)) as IPosition;

                    if (item != null)
                    {
                        var curPos = item.Position;
                        var newPos = e.GetPosition(sender);
                        curPos.X = newPos.X;
                        curPos.Y = newPos.Y;
                        item.OnPropertyChanged("Position");

                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            App.TaskFactory.StartNew(() => App.Service.UpdateGamePoint(App.ClientVersion, MainWindow.GamePortal.User.Login, curPos));
                        }), DispatcherPriority.ApplicationIdle);

                        return true;
                    }
                }
            }

            return false;
        }

        private bool TerrainDrop(DragEventArgs e)
        {
            if (TerrainView.UpdateGamePoint(e, this))
                return false;

            ExtTerrain dropTerrain = e.Data.GetData(typeof(ExtTerrain)) as ExtTerrain;
            if (dropTerrain == null)
                return false;

            _Terrain.Game.ClientStep.ExtMarch.WCFMarch.MarchUnit.Clear();
            foreach (var item in dropTerrain.Unit)
            {
                ExtTerrain moveTerrain = null;

                if (item.TempTerrain != item.ExtTerrain)
                    moveTerrain = item.TempTerrain;
                if (item.IsSelected)
                    moveTerrain = _Terrain;
                if (moveTerrain == null)
                    continue;

                if (CheckMove(item, moveTerrain))
                {
                    WCFMarchUnit _WCFMarchUnit = new WCFMarchUnit();
                    _WCFMarchUnit.Terrain = moveTerrain.WCFTerrain.Name;
                    _WCFMarchUnit.Unit = item.WCFUnit.Id;
                    _WCFMarchUnit.UnitType = item.WCFUnit.UnitType;
                    _Terrain.Game.ClientStep.ExtMarch.WCFMarch.MarchUnit.Add(_WCFMarchUnit);

                    item.TempTerrain = moveTerrain;
                }
                else
                    item.TempTerrain = null;

                item.IsSelected = false;
            }

            if (!HoldTerrain(dropTerrain))
            {
                _Terrain.Game.ClientStep.ExtGameUserInfo.ExtPowerCounter.Remove(dropTerrain.PowerCounter);
                _Terrain.Game.ClientStep.ExtMarch.WCFMarch.IsTerrainHold = false;
                _Terrain.Game.OnPropertyChanged("ViewPowerCounter");
            }

            return true;
        }

        //Проверка марша
        private bool CheckMove(ExtUnit unit, ExtTerrain toTerrain)
        {
            if (!_Terrain.Game.ClientStep.WCFStep.StepType.Contains("Усиление_власти"))
            {
                //запрет движение в никуда
                if (unit.ExtTerrain == toTerrain)
                    return false;

                if (unit.WCFUnit.UnitType == "Корабль" && toTerrain.WCFTerrain.TerrainType == "Земля")
                {
                    App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_move1"));
                    return false;
                }

                if (unit.WCFUnit.UnitType != "Корабль" && toTerrain.WCFTerrain.TerrainType != "Земля")
                {
                    App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_move2"));
                    return false;
                }

                if (!toTerrain.JoinTerrainCol.Contains(unit.ExtTerrain))
                {
                    if (unit.WCFUnit.UnitType == "Корабль")
                    {
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_move3"));
                        return false;
                    }
                    else
                    {
                        if (!rCheckTransfer(unit.ExtTerrain, toTerrain, new List<ExtTerrain>(), unit.Step.ExtGameUser))
                        {
                            App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_move4"));
                            return false;
                        }
                    }
                }
            }

            if (toTerrain.WCFTerrain.TerrainType == "Порт")
            {
                if (toTerrain.TempUnit.Count(p => p != unit) == 3)
                {
                    App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_move5"));
                    return false;
                }

                if (toTerrain.ExtHolderUser != unit.Step.ExtGameUser)
                {
                    App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_move6"));
                    return false;
                }
            }

            if (toTerrain.TempUnit.Count(p => p.Step == unit.Step && p != unit) == 4)
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_move7"));
                return false;
            }


            if (_Terrain.Game.ClientStep.WCFStep.StepType == "Отступление"
                || _Terrain.Game.ClientStep.WCFStep.StepType == "Робб_Старк")
            {
                if (_Terrain.Game.ClientStep.WCFStep.March.MarchUnit.Any(p => p.Terrain != toTerrain.WCFTerrain.Name))
                {
                    App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_move8"));
                    return false;
                }

                if (_Terrain.Game.ViewGameInfo.ExtBattle.AttackUser.LastStep.WCFStep.StepType != "Робб_Старк"
                    && toTerrain.WCFTerrain.Name == _Terrain.Game.ViewGameInfo.ExtBattle.WCFBattle.AttackTerrain)
                {
                    App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_move9"));
                    return false;
                }

                if ((toTerrain.ExtHolderUser != null && unit.Step.ExtGameUser != toTerrain.ExtHolderUser)
                    || (toTerrain.ExtHolderUser == null && _Terrain.ExtGarrison != null))
                {
                    App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_move10"));
                    return false;
                }
            }

            return true;
        }

        //проверяет возможность переброски
        private bool rCheckTransfer(ExtTerrain startTerrain, ExtTerrain endTerrain, List<ExtTerrain> checkedTerrain, ExtGameUser holder)
        {
            //цикл по своим соседним морям
            foreach (var item in startTerrain.JoinTerrainCol.Where(p => p.WCFTerrain.TerrainType == "Море" && p.ExtHolderUser == holder))
            {
                //игнорируем ранее проверенные моря
                if (checkedTerrain.Contains(item))
                    continue;

                //граничит?
                if (item.JoinTerrainCol.Contains(endTerrain))
                    return true;

                //добавляем в игнор
                checkedTerrain.Add(item);

                //рекурсия
                if (rCheckTransfer(item, endTerrain, checkedTerrain, holder))
                    return true;
            }

            return false;
        }

        //установить жетон власти
        private bool HoldTerrain(ExtTerrain dropTerrain)
        {
            //жетон власти установлен
            if (dropTerrain.PowerCounter != null && !dropTerrain.PowerCounter.IsTemp)
                return true;

            if (dropTerrain.WCFTerrain.TerrainType != "Земля")
                return false;
            //является родовой землёй
            //if (dropTerrain.ExtHomeType != null)
            if (dropTerrain.ExtHomeType == _Terrain.Game.ClientGameUser.ExtHomeType)
                return false;
            //Не своя территория
            if (dropTerrain.ExtHolderUser != _Terrain.Game.ClientGameUser)
                return false;
            //нет доступной власти
            if (_Terrain.Game.ClientStep.ExtGameUserInfo.WCFGameUserInfo.Power == 0)
                return false;
            //не все юниты ушли
            if (dropTerrain.TempUnit.Count != 0)
                return false;

            if (dropTerrain.PowerCounter != null)
                return true;

            //создаём временный знак власти
            WCFPowerCounter wcfPowerCounter = new WCFPowerCounter();
            wcfPowerCounter.Step = dropTerrain.ExtHolderUser.LastStep.WCFStep.Id;
            wcfPowerCounter.Terrain = dropTerrain.WCFTerrain.Name;
            wcfPowerCounter.TokenType = "Жетон_власти";

            ExtPowerCounter powerCounter = new ExtPowerCounter(dropTerrain.ExtHolderUser.LastStep, wcfPowerCounter);
            powerCounter.IsTemp = true;
            powerCounter.IsSelected = false;
            dropTerrain.ExtHolderUser.LastStep.ExtGameUserInfo.ExtPowerCounter.Add(powerCounter);

            _Terrain.Game.OnPropertyChanged("ViewPowerCounter");

            return true;
        }
    }
}
