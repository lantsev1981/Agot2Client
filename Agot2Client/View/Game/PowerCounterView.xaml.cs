using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для PowerCounterView.xaml
    /// </summary>
    public partial class PowerCounterView : UserControl
    {
        ExtPowerCounter _PowerCounter;

        public PowerCounterView()
        {
            InitializeComponent();
            DataContextChanged += PowerCounterView_DataContextChanged;
            this.MouseLeave += _MouseLeave;
            this.Drop += _Drop;
            this.AllowDrop = true;
        }

        private void _Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            TerrainView.UpdateGamePoint(e, this);
        }

        private void _MouseLeave(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            if (Keyboard.Modifiers == ModifierKeys.Control && e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(this, (IPosition)_PowerCounter, DragDropEffects.All);
                return;
            }
        }

        void PowerCounterView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _PowerCounter = (ExtPowerCounter)e.NewValue;
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (_PowerCounter.Step.Game.ClientStep != _PowerCounter.Step)
                return;

            if (_PowerCounter.IsTemp)
            {
                _PowerCounter.IsSelected = !_PowerCounter.IsSelected;
                _PowerCounter.Step.Game.ClientStep.ExtMarch.WCFMarch.IsTerrainHold = _PowerCounter.IsSelected;
            }
        }
    }
}
