using MyMod;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для WorldLayer.xaml
    /// </summary>
    public partial class WorldLayer : UserControl
    {
        public static double ViewboxScale;
        public static double MapLayerScale = 1;
        private Matrix LayerMatrix;
        public ExtGame Game { get; private set; }

        /// <summary>
        /// Определяет старую позицию мыши
        /// </summary>
        private Point oldMousePosition;

        /// <summary>
        /// Конструктор инициализирует компонент
        /// </summary>
        public WorldLayer()
        {
            InitializeComponent();
            MainWindow.ClientInfo.PlayStepsTimer.Tick += PlaySteps_Tick;
            ClientInfo.ClientGameChanging += ClientInfo_ClientGameChanging;
        }

        private void ClientInfo_ClientGameChanging(ExtGame game)
        {
            Game = game;
        }

        /// <summary>
        /// Обработчик события MouseWheel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapLayerGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point point = e.GetPosition(mapLayerGrid);
            if (e.Delta > 0)
            {
                MapLayerScale *= 1.1;
                LayerMatrix.ScaleAt(1.1, 1.1, point.X, point.Y);
            }
            else
            {
                MapLayerScale *= 0.9;
                LayerMatrix.ScaleAt(0.9, 0.9, point.X, point.Y);
            }

            mapLayer.RenderTransform = new MatrixTransform(LayerMatrix);
        }


        /// <summary>
        /// Обработчик события MouseMove
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapLayerGrid_MouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(mapLayerGrid);

            if ((e.OriginalSource is Polygon) && e.LeftButton == MouseButtonState.Pressed)
            {
                Vector _SubMove = Point.Subtract(point, oldMousePosition);
                LayerMatrix.Translate(_SubMove.X, _SubMove.Y);
                mapLayer.RenderTransform = new MatrixTransform(LayerMatrix);
            }

            oldMousePosition = point;
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            LayerMatrix.SetIdentity();
            MapLayerScale = 1;
            MainWindow.ClientInfo.WorldAngle = 0;

            mapLayer.RenderTransform = new MatrixTransform(LayerMatrix);
        }

        private void HomeButton2_Click(object sender, RoutedEventArgs e)
        {
            LayerMatrix.SetIdentity();
            MapLayerScale = 1;
            MainWindow.ClientInfo.WorldAngle = 90;

            LayerMatrix.RotateAt(-90, mapLayer.ActualWidth / 2d, mapLayer.ActualHeight / 2d);
            mapLayer.RenderTransform = new MatrixTransform(LayerMatrix);
        }

        private void Viewbox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewboxScale = viewbox.ActualHeight / mapLayer.ActualHeight;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ClientInfo.DisConnect();
        }

        private void NextViewKey_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Game.GetStepTask.IsBusy)
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("notify_Loading"));
                return;
            }

            int nextViewKey = Game.CurrentViewKey + 1;
            if (nextViewKey > Game.LastStepIndex)
                nextViewKey = 1;

            if (Game.CheckAllowStep(nextViewKey))
                Game.CurrentViewKey = nextViewKey;
            else
                Game.GetStepTask.AddTask(nextViewKey);
        }

        void PlaySteps_Tick(object sender, EventArgs e)
        {
            NextViewKey_Button_Click(null, null);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ClientInfo.HomeSelected = null;
            MainWindow.ClientInfo.ClientGameId = MainWindow.ClientInfo.ClientGameId;
        }

        private void PlayPause_Button_Click(object sender, RoutedEventArgs e)
        {
            ArrowViewModel.GlobalDuration = new Duration(TimeSpan.FromSeconds(1));

            MainWindow.ClientInfo.PlayStepsTimer.IsEnabled = !MainWindow.ClientInfo.PlayStepsTimer.IsEnabled;
            MainWindow.ClientInfo.PlayPauseImageName = MainWindow.ClientInfo.PlayStepsTimer.IsEnabled
                ? System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "/Image/pause.png")
                : System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "/Image/play.png");
        }

        public void Photo_Button_Click(object sender, RoutedEventArgs e)
        {
            string filename = SaveScreen();
        }

        private static string SaveScreen()
        {
            DateTime time = DateTime.Now;
            ExtGame game = MainWindow.ClientInfo.ClientGame;
            if (game != null)
            {
                time = game.WCFGame.CreateTime.LocalDateTime;
            }

            string dirName = string.Format("{0}{1}\\{2}", AppDomain.CurrentDomain.BaseDirectory, "Screenshot", time.ToString("yyyy.MM.dd_HH.mm"));
            Directory.CreateDirectory(dirName);
            string fileName = string.Format("{0}\\{1}.png", dirName, Guid.NewGuid());
            ImageExt.SaveToPNG(App.Agot2.gameView.worldLayer.mapLayer, new Size(1920, 1920 * 0.668), fileName);
            return fileName;
        }
    }
}
