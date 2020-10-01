using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для VesterosDeskItemView.xaml
    /// </summary>
    public partial class VesterosDeskItemView : UserControl
    {
        public int DecksNumber
        {
            get { return (int)GetValue(DecksNumberProperty); }
            set { SetValue(DecksNumberProperty, value); ViewModel.DecksNumber = value; }
        }

        public static readonly DependencyProperty DecksNumberProperty = DependencyProperty.Register("DecksNumber", typeof(int), typeof(VesterosDeskItemView), new PropertyMetadata(0, new PropertyChangedCallback((DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        {
            var value = (int)e.NewValue;
            if (value == 0)
                throw new Exception("Необходимо задать номер колоды");
            ((VesterosDeskItemView)d).DecksNumber = value;
        })));

        public VesterosDeskItemViewModel ViewModel { get; private set; }

        public VesterosDeskItemView()
        {
            InitializeComponent();
            ViewModel = new VesterosDeskItemViewModel();
            DataContext = this;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            ViewModel.Game.ClientStep.WCFStep.VesterosAction.ActionNumber = int.Parse(btn.Content.ToString());
            GameView.CompleteStep(ViewModel.Game);
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ViewModel.Game.ClientStep.ExtBattleUser.WCFBattleUser.AdditionalEffect = ViewModel.DecksNumber.ToString();
            GameView.CompleteStep(ViewModel.Game);
        }
    }

    public class VesterosDeskItemViewModel : MyMod.MyNotifyObj
    {
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
        public ObservableCollection<ExtVesterosDecks> Items { get; private set; }
        public int SelectedIndex { get; set; }
        public int DecksNumber { get; set; }
        public string Background
        {
            get { return string.Format("/Image/VesterosCard/{0}.png", DecksNumber); }
        }

        public VesterosDeskItemViewModel()
        {
            Items = new ObservableCollection<ExtVesterosDecks>();
            ClientInfo.ClientGameChanging += ClientInfo_ClientGameChanging;
        }

        private void ClientInfo_ClientGameChanging(ExtGame game)
        {
            if (Game != null)
            {
                Game.CurrentViewKeyCganged -= ExtGame_CurrentViewKeyCgange;
                Game.ClientStepCganged -= Game_ClientStepCganged;
            }
            if (game != null)
            {
                Game = game;
                Game.CurrentViewKeyCganged += ExtGame_CurrentViewKeyCgange;
                Game.ClientStepCganged += Game_ClientStepCganged;
            }
        }

        private void Game_ClientStepCganged(ExtStep clientStep)
        {
            if (clientStep != null && clientStep.WCFStep.StepType == "dragon_Rodrik_the_Reader"
                && string.IsNullOrEmpty(clientStep.WCFStep.BattleUser.AdditionalEffect))
                ExloreView = Visibility.Visible;
            else
                ExloreView = Visibility.Collapsed;
        }

        List<ExtVesterosDecks> curVesterosDecks;
        void ExtGame_CurrentViewKeyCgange()
        {
            List<ExtVesterosDecks> newVesterosDecks = Game.ViewGameInfo == null
                ? null
                : Game.ViewGameInfo.ExtVesterosDecks;

            if (curVesterosDecks == newVesterosDecks || (newVesterosDecks == null && curVesterosDecks.Count == 0))
                return;

            curVesterosDecks = newVesterosDecks;
            Items.Clear();
            if (curVesterosDecks != null)
            {
                curVesterosDecks.Where(p => p.WCFVesterosCardType.DecksNumber == DecksNumber).ToList().ForEach(p =>
                {
                    p.IndexOf = Items.Count + 1;
                    Items.Add(p);
                });
            }

            this.OnPropertyChanged("Items");
            SelectedIndex = Items.Count - 1;
            this.OnPropertyChanged("SelectedIndex");
        }
    }
}
