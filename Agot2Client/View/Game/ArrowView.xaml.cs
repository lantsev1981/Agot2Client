using GameService;
using System;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для arrowView.xaml
    /// </summary>
    public partial class ArrowView : UserControl
    {
        public ArrowView()
        {
            InitializeComponent();
            DataContextChanged += MainWindow_DataContextChanged;
        }

        private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (ArrowViewModel)e.NewValue;
            viewModel.LoadView(this);
        }
    }

    public class ArrowViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propName)
        {
            var @event = PropertyChanged;
            if (@event != null)
                @event(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        public ArrowModel Model { get; private set; }


        //Настраивает время анимации у всех экземпляров
        static public Duration GlobalDuration { get; set; }
        public Duration LocalDuration { get { return ArrowViewModel.GlobalDuration; } }


        //Путь
        Point[] _PathPoints = new Point[2];
        public string Path { get { return CreatePath(_PathPoints, false); } }


        Point[] _PassedWayPoints = new Point[2];
        public string PassedWayPath { get { return CreatePath(_PassedWayPoints, true); } }

        public Point[] LinearGradientBrushPoints
        {
            get
            {
                var result = new Point[] { new Point(-0.5, 0.5), new Point(1, 0.5) };
                var matrix = Matrix.Identity;
                matrix.RotateAt(_ArrowAngle, 0.5, 0.5);
                matrix.Transform(result);
                return result;
            }
        }


        //static public double BezierLength = 200;//коэффициэнт закругления
        static Matrix _RightRotationMatrix;
        static readonly Vector _IdentityVector = new Vector(1, 0);
        double _ArrowAngle;

        static ArrowViewModel()
        {
            _RightRotationMatrix = Matrix.Identity;
            _RightRotationMatrix.Rotate(-90);
        }

        public ArrowViewModel(ArrowModel model)
        {
            Model = model;
            _PathPoints[0] = _PassedWayPoints[0] = _PassedWayPoints[1] = ExtTokenPoint.GetPoint(Model.StartTerrainName, "Приказ");
            _PathPoints[1] = ExtTokenPoint.GetPoint(Model.EndTerrainName, "Приказ");
        }


        ArrowView _View;
        public void LoadView(ArrowView view)
        {
            _View = view;
            _View.ellipse.Changed += Elips_Changed;
        }


        void Elips_Changed(object sender, EventArgs e)
        {
            //определяем частоту рендеринга анимации (пройденое расстояние за такт анимации) и угол стрелки
            Vector _PassedWayDirection = _View.ellipse.Center - _PassedWayPoints[1];
            _ArrowAngle = Vector.AngleBetween(_IdentityVector, _PassedWayDirection);

            //Определяем растояние между текущим положением и конечной точкой
            if ((_View.ellipse.Center - _PathPoints[1]).Length <= _PassedWayDirection.Length + 75)
                _View.ellipse.Changed -= Elips_Changed;

            _PassedWayPoints[1] = _View.ellipse.Center;

            OnPropertyChanged("PassedWayPath");
            OnPropertyChanged("LinearGradientBrushPoints");
        }

        private string CreatePath(Point[] pathPoints, bool isPassedWayPath)
        {
            //todo new CultureInfo("ru-Ru")

            //определяем вектор к точке безье
            var pathDirection = (pathPoints[1] - pathPoints[0]) / 2;
            var bezierDirection = _RightRotationMatrix.Transform(pathDirection);

            //определяем точку безье
            var pathCenterPoint = pathPoints[0] + pathDirection;
            var bezierPoint = pathCenterPoint + bezierDirection * (pathDirection.Length / ((_PathPoints[1] - _PathPoints[0]) / 2).Length);

            //задаём путь
            StringBuilder sb = new StringBuilder();
            if (!isPassedWayPath)
            {
                sb.AppendFormat("M {0}", pathPoints[0]);
                sb.AppendFormat(" Q {0};{1}", bezierPoint, pathPoints[1]);
            }
            else
            {
                bezierDirection.Normalize();
                if (double.IsNaN(bezierDirection.Length))
                    return string.Empty;

                //путь выше просчитанного
                var startDirection = bezierDirection * 25;//половина ширины хвостика
                var centerDirection = startDirection / 2;//половина ширины середины

                //стрелка
                var arrowPoints = new Point[] { new Point(0, -6), new Point(0, -25), new Point(50, 0), new Point(0, 25), new Point(0, 6) };
                var tmpMatrix = Matrix.Identity;
                tmpMatrix.Rotate(_ArrowAngle);
                tmpMatrix.Translate(pathPoints[1].X, pathPoints[1].Y);
                tmpMatrix.Transform(arrowPoints);

                //путь
                sb.AppendFormat("M {0}", pathPoints[0] + startDirection);
                sb.AppendFormat(" Q {0};{1}", bezierPoint + centerDirection, arrowPoints[0]);
                sb.AppendFormat("L {0} L {1} L {2} L {3}", arrowPoints[1], arrowPoints[2], arrowPoints[3], arrowPoints[4]);

                //обратный путь
                startDirection.Negate();
                centerDirection.Negate();
                sb.AppendFormat(" Q {0};{1} Z", bezierPoint + centerDirection, pathPoints[0] + startDirection);
            }

            return sb.Replace(',', '.').Replace(';', ',').ToString();
        }
    }
}
