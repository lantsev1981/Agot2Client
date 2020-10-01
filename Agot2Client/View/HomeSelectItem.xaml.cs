using ShaderEffectLibrary;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TransitionEffects;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для HomeSelectItem.xaml
    /// </summary>
    public partial class HomeSelectItem : UserControl
    {
        public string Slogan { get; private set; }
        public string Source { get; private set; }
        public string FontForeground { get; private set; }

        string _HomeType;
        public string HomeType
        {
            get { return _HomeType; }
            private set
            {
                _HomeType = value;
                switch (_HomeType)
                {
                    case "dragon_Баратеон":
                    case "Баратеон":
                        Slogan = App.GetResources("text_sloganBaratheon");
                        FontForeground = "#040108";
                        break;
                    case "dragon_Ланнистер":
                    case "Ланнистер":
                        Slogan = App.GetResources("text_sloganLannister");
                        FontForeground = "#E5AD02";
                        break;
                    case "Старк":
                        Slogan = App.GetResources("text_sloganStark");
                        FontForeground = "#909193";
                        break;
                    case "dragon_Мартелл":
                    case "Мартелл":
                        Slogan = App.GetResources("text_sloganMartell");
                        FontForeground = "#9B0504";
                        break;
                    case "dragon_Грейджой":
                    case "Грейджой":
                        Slogan = App.GetResources("text_sloganGreyjoy");
                        FontForeground = "#E4B40A";
                        break;
                    case "dragon_Тирелл":
                    case "Тирелл":
                        Slogan = App.GetResources("text_sloganTyrell");
                        FontForeground = "#FFD81B";
                        break;
                    case "Random":
                        Slogan = App.GetResources("text_sloganRandom");
                        FontForeground = "#0196C1";
                        break;
                    case "dragon_Болтон":
                        Slogan = App.GetResources("text_sloganBolton");
                        FontForeground = "#7C0308";
                        break;
                }
            }
        }
        public Guid GameId { get; private set; }

        bool _Access = true;
        public bool Access
        {
            get { return _Access; }
            set
            {
                if (_Access != value)
                {
                    var effect = new ColorToneEffect();
                    effect.DarkColor = Colors.Black;
                    effect.LightColor = Colors.White;
                    effect.Toned = value ? 1 : -1;
                    image.Effect = effect;

                    DoubleAnimation da = new DoubleAnimation(effect.Toned, value ? -1 : 1, new Duration(TimeSpan.FromSeconds(1)));
                    effect.BeginAnimation(ColorToneEffect.TonedProperty, da);
                }

                _Access = value;
            }
        }

        public HomeSelectItem(string homeType, string source)
        {
            HomeType = homeType;
            Source = source;

            InitializeComponent();
            DataContext = this;
        }

        private void _MouseEnter(object sender, MouseEventArgs e)
        {
            if (!Access) return;

            var effect = new RippleTransitionEffect();
            image.Effect = effect;

            DoubleAnimation da = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(.5)));
            effect.BeginAnimation(RippleTransitionEffect.ProgressProperty, da);
        }
    }
}
