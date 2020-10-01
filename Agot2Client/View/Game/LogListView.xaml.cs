using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Agot2Client
{
    /// <summary>
    /// Interaction logic for LogListView.xaml
    /// </summary>
    public partial class LogListView : UserControl
    {
        public LogListViewModel ViewModel { get; private set; }

        public LogListView()
        {
            InitializeComponent();

            DataContextChanged += _DataContextChanged;
        }

        private void _DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ViewModel = (LogListViewModel)e.NewValue;
        }

        private void LogItemView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var indexOf = ViewModel.IndexOf(((LogItemView)sender).ViewModel);
            if (indexOf < 0)
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"),App.GetResources("error_logEvent") + indexOf);
                return;
            }

            while (ViewModel.Count != indexOf)
            {
                ViewModel.RemoveAt(indexOf);
            }
        }

        private void LogItemView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var indexOf = ViewModel.IndexOf(((LogItemView)sender).ViewModel);
            if (indexOf < 0)
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_logEvent")+ indexOf);
                return;
            }

            ViewModel.RemoveAt(indexOf);
        }
    }

    public class LogListViewModel : ObservableCollection<LogItemViewModel>
    {
        static public int MaxCount = 30;
        new public void Insert(int index, LogItemViewModel item)
        {
            base.Insert(index, item);
            while (this.Count > MaxCount)
            {
                this.RemoveAt(MaxCount);
            }
        }
    }
}
