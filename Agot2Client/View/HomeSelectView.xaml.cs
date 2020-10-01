using GameService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для HomeSelectView.xaml
    /// </summary>
    public partial class HomeSelectView : UserControl
    {
        public Guid GameId { get; private set; }
        static public IGameService GameService { get; private set; }

        //TODO сделать биндинг к content.Children
        List<HomeSelectItem> children = new List<HomeSelectItem>();

        public HomeSelectView()
        {
            InitializeComponent();
        }

        public void Load(WCFGame game)
        {
            GameId = game.Id;
            children.ForEach(p => p.MouseUp -= Item_MouseUp);
            children.Clear();
            content.Children.Clear();

            game.GameUser.Where(p => !string.IsNullOrEmpty(p.HomeType)).Select(p => p.HomeType).ToList()
                .ForEach(p => children.Add(new HomeSelectItem(p, string.Format("/Image/HomeSelect/{0}.jpg", ExtHomeType.Keys[p]))));
            children.Add(new HomeSelectItem("Random", "/Image/HomeSelect/random.jpg"));
            children.ForEach(p =>
            {
                content.Children.Add(p);
                p.MouseUp += Item_MouseUp;
            });

            var cf = new ChannelFactory<IGameService>("AGOT2Game");
            cf.Endpoint.Address = new EndpointAddress($"http://{App.Config.Settings.ServerAddress}:{App.Config.Settings.ServerPort}/GameHost/{GameId}");
            GameService = cf.CreateChannel();

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                App.TaskFactory.StartNew(() =>
                {
                    try
                    {
                        children.ForEach(p =>
                        {
                            var result = GameService.CheckAccessHome(MainWindow.GamePortal.User.Login, p.HomeType);
                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                p.Access = result;
                            }), DispatcherPriority.ApplicationIdle);
                        });

                        if (children.All(p => !p.Access))
                            App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("notify_connectionGame2"));
                        else
                        {
                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                this.Visibility = Visibility.Visible;
                            }), DispatcherPriority.ApplicationIdle);
                        }
                    }
                    catch (Exception exp)
                    {
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_checkHomeStatus") + exp.Message);
                    }
                });
            }), DispatcherPriority.ApplicationIdle);
        }

        private void Item_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var item = (HomeSelectItem)sender;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                App.TaskFactory.StartNew(() =>
                {
                    try
                    {
                        //проверка доступности
                        var access = GameService.CheckAccessHome(MainWindow.GamePortal.User.Login, item.HomeType);
                        if (item.Access != access)
                        {
                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                item.Access = access;
                            }), DispatcherPriority.ApplicationIdle);
                            return;
                        }

                        //подключение к игре
                        if (access)
                        {
                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                MainWindow.ClientInfo.HomeSelected = item.HomeType;
                                MainWindow.ClientInfo.ClientGameId = GameId;
                                this.Visibility = Visibility.Collapsed;

                            }), DispatcherPriority.ApplicationIdle);
                        }
                    }
                    catch (Exception exp)
                    {
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_checkHomeStatus") + exp.Message);
                    }
                });
            }), DispatcherPriority.ApplicationIdle);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}
