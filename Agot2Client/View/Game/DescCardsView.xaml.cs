using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для DescCardsView.xaml
    /// </summary>
    public partial class DescCardsView : UserControl, INotifyPropertyChanged
    {
        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propName)
        {
            var @event = PropertyChanged;
            if (@event != null)
                @event(this, new PropertyChangedEventArgs(propName));
        }
        #endregion
        public Visibility ExloreView
        {
            get { return _ExloreView; }
            set
            {
                _ExloreView = value;
                this.OnPropertyChanged("ExloreView");
            }
        }
        private Visibility _ExloreView = Visibility.Collapsed;

        public ExtGame Game { get; private set; }
        public ExtStep ClientStep { get; private set; }

        public List<ViewModel> ViewModels
        {
            get { return _ViewModels; }
            set
            {
                _ViewModels = value;
                this.OnPropertyChanged("ViewModels");
                this.OnPropertyChanged("ClientStep");
            }
        }
        private List<ViewModel> _ViewModels;


        public DescCardsView()
        {
            InitializeComponent();
            ClientInfo.ClientGameChanging += ClientInfo_ClientGameChanging;
            this.DataContext = this;
        }

        private void ClientInfo_ClientGameChanging(ExtGame game)
        {
            if (Game != null)
            {
                Game.ClientStepCganged -= ExtGame_ClientStepCganged;
            }
            if (game != null)
            {
                Game = game;
                Game.ClientStepCganged += ExtGame_ClientStepCganged;
            }
        }

        private void ExtGame_ClientStepCganged(ExtStep step)
        {
            ClientStep = step;

            if (ClientStep != null && ClientStep.WCFStep.StepType == "dragon_Rodrik_the_Reader"
                && !string.IsNullOrEmpty(ClientStep.WCFStep.BattleUser.AdditionalEffect))
            {
                var cards = JsonConvert.DeserializeObject<List<string>>(ClientStep.WCFStep.BattleUser.AdditionalEffect);
                ViewModels = cards.Select(p => new ViewModel() { path = App.GetResources("image_" + p), id = p }).ToList();
                ExloreView = Visibility.Visible;
            }
            else
                ExloreView = Visibility.Collapsed;
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var img = sender as Image;
            if (img != null)
            {
                var viewModel = img.DataContext as ViewModel;
                if (viewModel != null)
                {
                    ClientStep.WCFStep.BattleUser.AdditionalEffect = viewModel.id;
                    GameView.CompleteStep(Game);
                }
            }
        }

        public class ViewModel
        {
            public string path { get; set; }
            public string id { get; set; }
        }
    }
}
