using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для OrderView.xaml
    /// </summary>
    public partial class OrderView : UserControl
    {
        ExtOrder _ExtOrder;

        public OrderView()
        {
            InitializeComponent();
            DataContextChanged += OrderView_DataContextChanged;
            this.Drop += _Drop;
            this.AllowDrop = true;
        }

        private void _Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            TerrainView.UpdateGamePoint(e, this);
        }

        private void OrderView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _ExtOrder = (ExtOrder)DataContext;
        }

        //!!!ошибка не содержит элементов
        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Если у клиента нет текущего хода
            if (_ExtOrder.Step.Game.ClientStep == null)
                return;

            if (_ExtOrder.Step.WCFStep.StepType == "Подмога")
                return;

            //Отмена приказа другого пользователя
            if (_ExtOrder.Step.Game.ClientStep.WCFStep.StepType == "Королева_Шипов"
                || _ExtOrder.Step.Game.ClientStep.WCFStep.StepType == "Серсея_Ланнистер")
            {
                if (Opacity != 1)
                    return;

                _ExtOrder.Step.Game.ClientStep.WCFStep.BattleUser.AdditionalEffect = _ExtOrder.WCFOrder.Id.ToString();
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), string.Format(App.GetResources("text_selectedOrder"), App.GetResources("orderType_" + _ExtOrder.ExtOrderType.WCFOrderType.Name)), 2);
                GameView.CompleteStep(_ExtOrder.Step.Game);

                return;
            }

            //Только владелец в свой ход
            if (_ExtOrder.Step != _ExtOrder.Step.Game.ClientStep)
                return;

            //Этап замысла
            if (_ExtOrder.Step.WCFStep.StepType == "Замысел"
                || _ExtOrder.Step.WCFStep.StepType == "Неожиданный_шаг")
            {
                if (_ExtOrder.Step.WCFStep.StepType == "Замысел")
                    _ExtOrder.WCFOrder.OrderType = null;
                _ExtOrder.OnPropertyChanged("ImageName");
                OrderTypePopup.IsOpen = true;
                return;
            }

            //ход должен соответствовать типу приказа
            if (_ExtOrder.Step.WCFStep.StepType.Contains(_ExtOrder.ExtOrderType.WCFOrderType.DoType))
            {
                _ExtOrder.IsSelected = true;

                if (_ExtOrder.Step.WCFStep.StepType.Contains("Усиление_власти"))
                    UnitTypePopup.IsOpen = true;
            }
        }

        private void RaidImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (_ExtOrder.Step.Game.ClientStep == null)
                return;

            _ExtOrder.Step.Game.ClientStep.WCFStep.Raid.TargetOrder = _ExtOrder.WCFOrder.Id;
            GameView.CompleteStep(_ExtOrder.Step.Game);
        }

        private void OrderMenu_Opened(object sender, EventArgs e)
        {
            Matrix scaleMatrix = Matrix.Identity;
            scaleMatrix.Scale(MainWindow.ClientInfo.WorldScale, MainWindow.ClientInfo.WorldScale);
            scaleMatrix.Invert();

            OrderTypeMenu.LayoutTransform = new MatrixTransform(scaleMatrix);
            UnitTypeMenu.LayoutTransform = new MatrixTransform(scaleMatrix);

            _ExtOrder.OnPropertyChanged("OrderTypeMenu");
            _ExtOrder.OnPropertyChanged("UnitTypeMenu");
        }

        private void OrderView_MouseLeave(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            if (Keyboard.Modifiers == ModifierKeys.Control && e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(this, (IPosition)_ExtOrder, DragDropEffects.All);
                return;
            }

            if (!this.IsMouseOver && !OrderTypeMenu.IsMouseOver)
                OrderTypePopup.IsOpen = false;
        }
    }
}
