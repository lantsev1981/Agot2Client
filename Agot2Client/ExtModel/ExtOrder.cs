using GameService;
using MyMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Agot2Client
{
    public class ExtOrder : MyNotifyObj, IDisposable, IPosition
    {
        public event Action IsDisposeEvent;
        private void OnIsDisposeEvent()
        {
            var @event = IsDisposeEvent;
            if (@event != null)
                @event();
        }

        public ExtStep Step { get; private set; }
        public WCFOrder WCFOrder { get; private set; }

        public ExtTerrain ExtTerrain { get; private set; }
        public WCFGamePoint Position { get; set; }

        public ExtOrder(ExtStep step, WCFOrder wcfOrder)
        {
            Step = step;
            WCFOrder = wcfOrder;

            ExtTerrain = MainWindow.ClientInfo.WorldData.Terrain.Single(p => p.WCFTerrain.Name == WCFOrder.Terrain);
            Position = this.ExtTerrain.ExtTokenPoint.Single(p => p.WCFTokenPoint.TokenType == "Приказ").WCFGamePoint;

            Step.Game.CurrentViewKeyCganged += ExtGame_CurrentViewKeyCgange;
            Step.Game.SelecteOrderChanged += ExtOrder_SelectedOrderChange;
            ClientInfo.ClientGameChanging += ClientInfo_ClientGameChanging;
        }

        public void Dispose()
        {
            ClientInfo.ClientGameChanging -= ClientInfo_ClientGameChanging;
            Step.Game.SelecteOrderChanged -= ExtOrder_SelectedOrderChange;
            Step.Game.CurrentViewKeyCganged -= ExtGame_CurrentViewKeyCgange;

            OnIsDisposeEvent();
        }

        private void ClientInfo_ClientGameChanging(ExtGame game)
        {
            Dispose();
        }

        private void ExtGame_CurrentViewKeyCgange()
        {
            //Если приказ не из текущей отрисовки
            if (!Step.Game.ViewOrder.Contains(this))
                Dispose();
            else
            {
                //Если ход приказа невыполнен
                if (!this.Step.WCFStep.IsFull)
                {
                    //TODO реакция приказов на текущий ход
                    switch (this.Step.WCFStep.StepType)
                    {
                        case "Замысел":
                        case "Неожиданный_шаг":
                            this.Opacity = 1;
                            break;

                        case "Подмога":
                            //не подмога
                            if (this.ExtOrderType.WCFOrderType.DoType != "Подмога")
                                break;
                            //не соседний
                            if (!Step.Game.ViewGameInfo.ExtBattle.DefenceTerrain.JoinTerrainCol.Any(p => p == this.ExtTerrain))
                                break;
                            //с земли на море нельзя
                            if (Step.Game.ViewGameInfo.ExtBattle.DefenceTerrain.WCFTerrain.TerrainType == "Море"
                               && this.ExtTerrain.WCFTerrain.TerrainType == "Земля")
                                break;
                            //с порта на землю нельзя
                            if (Step.Game.ViewGameInfo.ExtBattle.DefenceTerrain.WCFTerrain.TerrainType == "Земля"
                               && this.ExtTerrain.WCFTerrain.TerrainType == "Порт")
                                break;

                            this.Opacity = 1;
                            break;

                        default:
                            //все приказы удовлетворяющие типу хода
                            if (this.Step.WCFStep.StepType.Contains(this.ExtOrderType.WCFOrderType.DoType))
                                this.Opacity = 1;
                            break;
                    }
                }
                else
                    this.IsSelected = false;
            }
        }

        private void ExtOrder_SelectedOrderChange(ExtOrder newValue)
        {
            if (string.IsNullOrEmpty(newValue.WCFOrder.OrderType)
                || string.IsNullOrEmpty(this.WCFOrder.OrderType))
                return;

            if (newValue == this)
            {
                switch (newValue.ExtOrderType.WCFOrderType.DoType)
                {
                    case "Набег":
                        Step.Game.ClientStep.WCFStep.Raid = new WCFRaid
                        {
                            SourceOrder = newValue.WCFOrder.Id
                        };
                        break;

                    case "Поход":
                    case "Отступление":
                    case "Усиление_власти":
                    case "Усиление_власти_Вестерос":
                        Step.Game.ClientStep.ExtMarch.Clear();
                        Step.Game.ClientStep.ExtMarch.WCFMarch.SourceOrder = newValue.WCFOrder.Id.ToString();
                        break;

                    default:
                        break;
                }
            }
            else
            {
                this.IsSelected = false;

                switch (newValue.ExtOrderType.WCFOrderType.DoType)
                {
                    case "Набег":
                        //только чужие приказы
                        if (this.Step.WCFStep.GameUser == newValue.Step.WCFStep.GameUser)
                            return;
                        //только соседние приказы
                        if (this.ExtTerrain.JoinTerrainCol
                            .SingleOrDefault(p => p.Order == newValue) == null)
                            return;
                        if (newValue.ExtTerrain.WCFTerrain.TerrainType == "Земля"
                            && this.ExtTerrain.WCFTerrain.TerrainType != "Земля")
                            return;
                        //не походы
                        if (this.ExtOrderType.WCFOrderType.DoType == "Поход")
                            return;
                        //если усиленный приказ то можно снять оборону
                        if (!newValue.ExtOrderType.WCFOrderType.IsSpecial
                            && this.ExtOrderType.WCFOrderType.DoType == "Оборона")
                            return;

                        this.Opacity = 1;
                        this.RaidVisibility = Visibility.Visible;
                        break;

                    default:
                        break;
                }
            }
        }

        public string ImageName
        {
            get
            {
                if (string.IsNullOrEmpty(WCFOrder.OrderType))
                    return this.Step.ExtGameUser.ExtHomeType.InfluenceImageName;

                return this.ExtOrderType.ImageName;
            }
        }

        public ExtOrderType ExtOrderType => MainWindow.ClientInfo.WorldData.OrderType
                    .SingleOrDefault(p => p.WCFOrderType.Name == WCFOrder.OrderType);

        private Visibility _RaidVisibility = Visibility.Collapsed;
        public Visibility RaidVisibility
        {
            get => _RaidVisibility;
            set
            {
                _RaidVisibility = value;
                OnPropertyChanged("RaidVisibility");
            }
        }

        private double _Opacity = .66;
        public double Opacity
        {
            get => _Opacity;
            set
            {
                _Opacity = value;
                OnPropertyChanged("Opacity");
            }
        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get => _IsSelected;
            set
            {
                _IsSelected = value;

                if (value)
                    this.Opacity = 1;
                else
                {
                    this.Opacity = .66;
                    RaidVisibility = Visibility.Collapsed;
                }

                if (value)
                    Step.Game.SelectedOrder = this;
            }
        }

        public IEnumerable<ExtOrderType> OrderTypeMenu
        {
            get
            {
                //отбираем список доступных приказов
                IEnumerable<ExtOrderType> result = MainWindow.ClientInfo.WorldData.OrderType
                    .Where(p => Step.ExtGameUserInfo.ExtOrder
                        .Count(p1 => p.WCFOrderType.Name == p1.WCFOrder.OrderType) < p.WCFOrderType.Count);

                //убираем запрещённые приказы в этом туре
                ExtVesterosDecks vesterosDecks = Step.Game.ViewGameInfo.ExtVesterosDecks
                    //.OrderBy(p => p.WCFVesterosDecks.Sort)
                    .LastOrDefault(p => p.WCFVesterosDecks.IsFull
                        && p.WCFVesterosCardType.DecksNumber == 3);
                if (vesterosDecks != null && vesterosDecks.WCFVesterosDecks.VesterosActionType != null)
                    result = result.Where(p => !p.WCFOrderType.Name
                        .Contains(vesterosDecks.WCFVesterosDecks.VesterosActionType));

                //Убираем приказы неимеющие смысла на данной территории
                switch (ExtTerrain.WCFTerrain.TerrainType)
                {
                    case "Море":
                        result = result.Where(p => p.WCFOrderType.DoType != "Усиление_власти");
                        break;
                    case "Порт":
                        result = result.Where(p => p.WCFOrderType.DoType != "Оборона");
                        break;

                    default:
                        break;
                }

                //Позволяет заменить один приказ со * на другой приказ со *                
                int maxSpecial = Step.WCFStep.GameUserInfo.SpecialOrderCount(this.Step.Game.GameTypeItem.PlayerCount);
                if (!string.IsNullOrEmpty(this.WCFOrder.OrderType) && this.ExtOrderType.WCFOrderType.IsSpecial)
                    maxSpecial++;

                //убираем специальные приказы при достижении максимума
                if (Step.ExtGameUserInfo.ExtOrder.Count(p => !string.IsNullOrEmpty(p.WCFOrder.OrderType)
                        && p.ExtOrderType.WCFOrderType.IsSpecial) >= maxSpecial)
                    result = result.Where(p => !p.WCFOrderType.IsSpecial);

                return result;
            }
        }

        public IEnumerable<ExtUnitType> UnitTypeMenu => MainWindow.ClientInfo.WorldData.UnitType
                    .Where(p => p.CheckUnitTypeCount(this));
    }
}
