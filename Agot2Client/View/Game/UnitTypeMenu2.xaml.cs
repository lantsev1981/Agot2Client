using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для UnitTypeMenu2.xaml
    /// </summary>
    public partial class UnitTypeMenu2 : UserControl, IDisposable
    {
        ExtOrder _ExtOrder;

        public UnitTypeMenu2()
        {

            InitializeComponent();
            DataContextChanged += OrderTypeMenu2_DataContextChanged;
        }

        void OrderTypeMenu2_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _ExtOrder = (ExtOrder)e.NewValue;
            _ExtOrder.IsDisposeEvent += _ExtOrder_IsDispose;
            _ExtOrder.Step.Game.SelecteOrderChanged += ExtGame_SelectedOrderChange;
        }

        void _ExtOrder_IsDispose()
        {
            Dispose();
        }
        
        public void Dispose()
        {
            _ExtOrder.Step.Game.SelecteOrderChanged -= ExtGame_SelectedOrderChange;
            _ExtOrder.IsDisposeEvent -= _ExtOrder_IsDispose;
        }

        void ExtGame_SelectedOrderChange(ExtOrder newValue)
        {
            ((Popup)this.Parent).IsOpen = false;
        }

        private void Item_MouseLeave(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ExtUnitType unitType = (ExtUnitType)((Image)sender).DataContext;
                DragDrop.DoDragDrop(this, unitType, DragDropEffects.All);
                _ExtOrder.Step.Game.OnPropertyChanged("ViewUnit");

                if (_ExtOrder.Step.Game.ClientStep.ExtMarch.SumCost == _ExtOrder.ExtTerrain.WCFTerrain.Strength)
                    ((Popup)this.Parent).IsOpen = false;

                _ExtOrder.OnPropertyChanged("UnitTypeMenu");
            }
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
