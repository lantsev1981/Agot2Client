using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Agot2Client
{
    /// <summary>
    /// Interaction logic for LogItemView.xaml
    /// </summary>
    public partial class LogItemView : UserControl
    {
        public LogItemViewModel ViewModel { get; private set; }

        public LogItemView()
        {
            InitializeComponent();
            DataContextChanged += _DataContextChanged;
        }

        private void _DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ViewModel = (LogItemViewModel)e.NewValue;
        }
    }

    public class LogItemViewModel : ObservableCollection<string>
    {
        public ExtStep Step { get; private set; }

        public LogItemViewModel(ExtStep model)
            : base(Convert(model))
        {
            Step = model;
        }

        static private IEnumerable<string> Convert(ExtStep model)
        {
            var items = model.WCFStep.Message.Select(p => App.TextDecoder(p)).Reverse().ToList();

            model.WCFStep.Message = items;
            return model.WCFStep.Message;
        }
    }
}
