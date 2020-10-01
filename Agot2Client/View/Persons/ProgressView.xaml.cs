using GameService;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Agot2Client
{
    public partial class ProgressView : UserControl
    {
        public Brush ProgressColor
        {
            get => (Brush)GetValue(ProgressColorProperty);
            set => SetValue(ProgressColorProperty, value);
        }
        public static readonly DependencyProperty ProgressColorProperty =
            DependencyProperty.Register("ProgressColor", typeof(Brush), typeof(ProgressView), new UIPropertyMetadata(null, new PropertyChangedCallback(ProgressColorChangedCallback)));

        private static void ProgressColorChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ProgressView)d).progressColor.Background = e.NewValue as Brush;
        }

        public ProgressView()
        {
            InitializeComponent();
        }
    }

    public class ProgressViewModel
    {
        public int GameCount { get; private set; }
        public int VictoryCount { get; private set; }
        public double Value { get; private set; }
        public int Efficiency { get; private set; }

        public string ValueString { get; private set; }
        public string CountString { get; private set; }
        public ExtHomeType HomeType { get; private set; }
        public GameTypeItem GameTypeItem { get; set; }

        public ProgressViewModel(int gameType, string homeType, int victoryCount, int gameCount, double value, int efficiency = 0)
        {
            HomeType = MainWindow.ClientInfo.WorldData.HomeType.Single(p1 => p1.WCFHomeType.Name == homeType);

            GameCount = gameCount;
            VictoryCount = victoryCount;
            Value = value;
            Efficiency = efficiency;

            ValueString = string.Format("{0}: {1:.00}%", App.GetResources("homeType_" + homeType), value);
            CountString = string.Format("{0} {1}", App.GetResources("text_userGameCount"), gameCount);

            GameTypeItem = MainWindow.GameTypes.First(p => p.Id == gameType);
        }
    }
}
