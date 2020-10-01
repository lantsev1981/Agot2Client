using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

using MyMod;
using GameService;

namespace Agot2Client
{
    public class ExtSymbolic : MyNotifyObj
    {
        public ExtGame Game { get; private set; }
        public WCFSymbolic WCFSymbolic { get; private set; }

        public IEnumerable<ExtObjectPoint> ObjectPoint { get; private set; }
        public PointCollection Points { get; private set; }

        static Brush _DefaultBackground = new SolidColorBrush(Colors.Red);

        public ExtSymbolic(ExtStaticData extStaticData, WCFSymbolic wcfSymbolic)
        {
            WCFSymbolic = wcfSymbolic;

            ObjectPoint = extStaticData.ObjectPoint.Where(p => p.WCFObjectPoint.Symbolic == WCFSymbolic.Name);
            ObjectPoint = ObjectPoint.OrderBy(p => p.WCFObjectPoint.Sort).ToList();

            Points = new PointCollection(this.ObjectPoint.Select(p => new Point(p.GamePoint.X, p.GamePoint.Y)));

            Opacity = 0.66;
            Background = _DefaultBackground;

            ClientInfo.ClientGameChanging += ClientInfo_ClientGameChanging;
        }

        void ClientInfo_ClientGameChanging(ExtGame game)
        {
            if (Game != null)
            {
                Game.ClientStepCganged -= ExtGame_ClientStepCgange;
                Game.CurrentViewKeyCganged -= ExtGame_CurrentViewKeyCganged;
            }
            if (game != null)
            {
                Game = game;
                Game.ClientStepCganged += ExtGame_ClientStepCgange;
                Game.CurrentViewKeyCganged += ExtGame_CurrentViewKeyCganged;
            }

            Visibility = Visibility.Collapsed;
        }

        void ExtGame_ClientStepCgange(ExtStep newValue)
        {
            if (newValue != null)
            {
                switch (newValue.WCFStep.StepType)
                {
                    case "Посыльный_ворон":
                        if (this.WCFSymbolic.Name == "Посыльный_ворон" || this.WCFSymbolic.Name == "Карта_одичалых")
                        {
                            Background = _DefaultBackground;
                            this.Visibility = Visibility.Visible;
                            return;
                        }
                        break;
                        
                    case "dragon_Ser_Gerris_Drinkwater":
                    case "Доран_Мартелл":
                        if (this.WCFSymbolic.Name != "Карта_одичалых")
                        {
                            Background = _DefaultBackground;
                            this.Visibility = Visibility.Visible;
                            return;
                        }
                        break;

                    case "Король-за-Стеной":
                        if (newValue.WCFStep.Raven.StepType == "First")
                        {
                            if (this.WCFSymbolic.Name != "Карта_одичалых")
                            {
                                Background = _DefaultBackground;
                                this.Visibility = Visibility.Visible;
                                return;
                            }
                        }
                        else
                        {
                            if (this.WCFSymbolic.Name != "Карта_одичалых" && this.WCFSymbolic.Name != "Железный_трон")
                            {
                                Background = _DefaultBackground;
                                this.Visibility = Visibility.Visible;
                                return;
                            }
                        }
                        break;

                    case "Передовой_отряд":
                        if (newValue.WCFStep.Raven.StepType.Contains(this.WCFSymbolic.Name))
                        {
                            Background = _DefaultBackground;
                            this.Visibility = Visibility.Visible;
                            return;
                        }
                        break;
                }
            }

            ExtGame_CurrentViewKeyCganged();
        }

        #region Отображение последней карты одичалых
        static string lastBarbarianCardId;
        private void ExtGame_CurrentViewKeyCganged()
        {
            if (this.WCFSymbolic.Name == "Карта_одичалых")
            {
                if (Game.ViewGameInfo == null)
                {
                    lastBarbarianCardId = null;
                    this.Visibility = Visibility.Collapsed;
                }
                else
                {
                    var currentBarbarianCard = Game.ViewGameInfo.ExtVesterosDecks.LastOrDefault(p => p.WCFVesterosCardType.DecksNumber == 4);
                    if (currentBarbarianCard == null)
                    {
                        lastBarbarianCardId = null;
                        this.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        if (lastBarbarianCardId != currentBarbarianCard.WCFVesterosDecks.VesterosCardType || !(Background is ImageBrush))
                        {
                            lastBarbarianCardId = currentBarbarianCard.WCFVesterosDecks.VesterosCardType;
                            this.Background = new ImageBrush(ImageExt.Load(currentBarbarianCard.ImageName, Int32Rect.Empty));
                            this.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
            else
                this.Visibility = Visibility.Collapsed;
        }
        #endregion

        Visibility _Visibility = Visibility.Collapsed;
        public Visibility Visibility
        {
            get { return _Visibility; }
            set
            {
                _Visibility = value;

                if (_Visibility == Visibility.Visible)
                {
                    if (Background is SolidColorBrush)
                        this.Opacity = 0.66;
                    if (Background is ImageBrush)
                        this.Opacity = 1;

                    this.OnPropertyChanged("Opacity");
                    this.OnPropertyChanged("Background");
                }

                OnPropertyChanged("Visibility");
            }
        }

        public Brush Background { get; private set; }

        public double Opacity { get; private set; }
    }
}
