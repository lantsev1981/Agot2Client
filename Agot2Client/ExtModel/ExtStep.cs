using GameService;
using System.Collections.Generic;
using System.Linq;

namespace Agot2Client
{
    public class ExtStep
    {
        public ExtGame Game { get; private set; }
        public WCFStep WCFStep { get; private set; }

        public ExtGameUser ExtGameUser { get; private set; }
        public ExtGameUserInfo ExtGameUserInfo { get; private set; }
        public ExtGameInfo ExtGameInfo { get; private set; }
        public ExtSupport ExtSupport { get; private set; }
        public ExtBattleUser ExtBattleUser { get; private set; }
        public ExtMarch ExtMarch { get; private set; }
        public string Name => string.Format(App.GetResources("text_clientStep"), App.GetResources("stepType_" + WCFStep.StepType));
        public string VotingName
        {
            get
            {
                if (WCFStep.StepType == "dragon_Aeron_Damphair")
                    return App.GetResources("stepType_dragon_Aeron_Damphair");

                if (WCFStep.Voting == null)
                    return null;

                if (WCFStep.Voting.Target == "Одичалые")
                    return App.GetResources("voting_" + WCFStep.Voting.Target);
                else
                    return string.Format(App.GetResources("text_voting"), App.GetResources("voting_" + WCFStep.Voting.Target));
            }
        }

        public ExtStep(WCFStep wcfStep, ExtGame game)
        {
            WCFStep = wcfStep;
            Game = game;
            ExtGameUser = game.ExtGameUser.Single(p => p.WCFGameUser.Id == WCFStep.GameUser);

            if (wcfStep.GameUserInfo != null)
                ExtGameUserInfo = new ExtGameUserInfo(this, wcfStep.GameUserInfo);

            if (wcfStep.GameInfo != null)
                ExtGameInfo = new ExtGameInfo(this, wcfStep.GameInfo);

            if (wcfStep.Support != null)
                ExtSupport = new ExtSupport(this, wcfStep.Support);

            if (wcfStep.BattleUser != null)
                ExtBattleUser = new ExtBattleUser(this, wcfStep.BattleUser);

            if (wcfStep.March != null)
                ExtMarch = new ExtMarch(this, wcfStep.March);

            if (wcfStep.StepType == "Замысел")
                Game.OnNewPlanningPhase();
        }

        public bool IsClientStep
        {
            get
            {
                if (!WCFStep.IsFull)
                {
                    if (WCFStep.StepType != "Робб_Старк")
                    {
                        if (Game.ClientGameUser == ExtGameUser)
                            return true;
                    }
                    else
                    {
                        //if (WCFStep.BattleUser.AdditionalEffect == Game.ClientGameUser.WCFGameUser.HomeType)
                        if (Game.ClientGameUser.LastStep.ExtBattleUser.WCFBattleUser.HomeCardType == "Робб_Старк")
                            return true;
                    }
                }
                return false;
            }
        }

        public IEnumerable<ExtTerrain> AttackTerrain { get; private set; }
        public bool CheckStep()
        {
            switch (WCFStep.StepType)
            {
                case "Событие_Вестероса":
                    if (WCFStep.VesterosAction.ActionNumber == null)
                    {
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), Name);
                        return false;
                    }
                    break;
                case "Доран_Мартелл":
                    if (WCFStep.BattleUser.AdditionalEffect == null)
                    {
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_Track"));
                        return false;
                    }
                    break;

                case "Набег":
                case "Усиление_власти":
                case "Усиление_власти_Вестерос":
                case "Поход":
                    if (Game.SelectedOrder == null)
                    {
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_order"));
                        return false;
                    }
                    break;

                case "Сражение":
                    if (string.IsNullOrEmpty(WCFStep.BattleUser.HomeCardType))
                    {
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_homeCard"));
                        return false;
                    }
                    break;
            }

            if (WCFStep.StepType == "Поход")
            {
                //Количество нападений
                IEnumerable<ExtTerrain> tempTerrain = ExtMarch.ExtMarchUnit.Select(p => p.ExtUnit.TempTerrain).Distinct();
                AttackTerrain = tempTerrain.Where(p => (p.ExtHolderUser != this.ExtGameUser && (p.Unit.Count() > 0 || p.ExtGarrison != null))
                    || (p.ExtHolderUser == null && p.ExtGarrison != null));
                switch (AttackTerrain.Count())
                {
                    case 0: break;
                    case 1:
                        //Нейтральные лорды
                        ExtTerrain garrisonTerrain = AttackTerrain.SingleOrDefault(p => p.ExtHolderUser == null && p.ExtGarrison != null);
                        if (garrisonTerrain != null)
                        {
                            int resultStrength = garrisonTerrain.TempUnit.Sum(p => p.WCFUnit.IsWounded ? 0 : p.ExtUnitType.WCFUnitType.Strength);
                            resultStrength += Game.SelectedOrder.ExtOrderType.WCFOrderType.Strength;

                            foreach (ExtTerrain item in garrisonTerrain.JoinTerrainCol)
                            {
                                if (item.ExtHolderUser != this.ExtGameUser)
                                    continue;

                                ExtOrder order = item.Order;
                                if (order == null)
                                    continue;
                                if (order.ExtOrderType.WCFOrderType.DoType == "Подмога")
                                {
                                    resultStrength += order.ExtOrderType.WCFOrderType.Strength;
                                    resultStrength += item.TempUnit.Sum(p => p.WCFUnit.IsWounded ? 0 : p.ExtUnitType.WCFUnitType.Strength);
                                }
                            }

                            if (resultStrength < garrisonTerrain.ExtGarrison.WCFGarrison.Strength)
                            {
                                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), string.Format(App.GetResources("validation_lordFalse"), resultStrength, garrisonTerrain.ExtGarrison.WCFGarrison.Strength));
                                return false;
                            }

                            App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), string.Format(App.GetResources("validation_lordTrue"), resultStrength, garrisonTerrain.ExtGarrison.WCFGarrison.Strength));
                        }
                        break;
                    default:
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_march"));
                        return false;
                }
            }

            //снабжение
            List<int> supplyArmy = null;
            switch (WCFStep.GameUserInfo.Supply)
            {
                case 0:
                    supplyArmy = new List<int>() { 2, 2 };
                    break;
                case 1:
                    supplyArmy = new List<int>() { 2, 3 };
                    break;
                case 2:
                    supplyArmy = new List<int>() { 2, 2, 3 };
                    break;
                case 3:
                    supplyArmy = new List<int>() { 2, 2, 2, 3 };
                    break;
                case 4:
                    supplyArmy = new List<int>() { 2, 2, 3, 3 };
                    break;
                case 5:
                    supplyArmy = new List<int>() { 2, 2, 3, 4 };
                    break;
                case 6:
                    supplyArmy = new List<int>() { 2, 2, 2, 3, 4 };
                    break;
            }

            foreach (ExtTerrain terrain in ExtGameUserInfo.ExtUnit.Select(p => p.TempTerrain).Distinct())
            {
                int unitCount = terrain.TempUnit.Count(p => p.Step == this && (p.Step.WCFStep.StepType != "Роспуск_войск" || !p.IsSelected));
                if (unitCount < 2) continue;

                int supply = supplyArmy.FirstOrDefault(p => (p - unitCount) >= 0);
                if (supplyArmy.Count == 0 || supply == 0)
                {
                    App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("validation_supply"));
                    return false;
                }

                supplyArmy.Remove(supply);
            }

            return true;
        }
    }
}
