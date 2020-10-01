using GamePortal;
using GameService;
using MyLibrary;
using MyMod;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Agot2Client
{
    public class GamePortal : MyNotifyObj
    {
        private string _Filter;
        public string Filter
        {
            get => _Filter;
            set { _Filter = string.IsNullOrEmpty(value) ? null : value.ToLower(); OnPropertyChanged("Filter"); }
        }

        private bool _OnlineOnly;
        public bool OnlineOnly
        {
            get => _OnlineOnly;
            set { _OnlineOnly = value; OnPropertyChanged("OnlineOnly"); }
        }

        public List<WCFUserGame> MindUserGames { get; private set; }
        public Dictionary<string, double> RateValues { get; private set; }

        public Dictionary<string, ProgressViewModel> ProgressViewModels { get; private set; }

        private bool _isAdmin;
        public bool IsAdmin
        {
            get => _isAdmin;
            private set { _isAdmin = value; OnPropertyChanged("IsAdmin"); }
        }
        public GPUser User { get; private set; }
        public GPUser Vesteros { get; private set; }

        private GPUser _MasterOfDonate;
        public GPUser MasterOfDonate
        {
            get => _MasterOfDonate;
            private set
            {
                _MasterOfDonate = value;
                OnPropertyChanged("MasterOfDonate");
            }
        }

        //public double MaximumRate { get; private set; }
        //public double MaximumRateScale { get; private set; }

        private IGamePortalServer _Service;
        private DispatcherTimer _UserOnlineTimer;

#if !DEBUG
        private CryptoFileJson<PortalSettings> _Settings;
#endif
#if DEBUG
        private PublicFileJson<PortalSettings> _Settings;
#endif

        public GamePortal()
        {
            ChannelFactory<IGamePortalServer> cf = new ChannelFactory<IGamePortalServer>("GamePortal");
            _Service = cf.CreateChannel();

            _UserOnlineTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle) { Interval = TimeSpan.FromSeconds(30) };
            _UserOnlineTimer.Tick += UserOnlineTickAsync;
        }

        public void Load()
        {
#if !DEBUG
            _Settings = new CryptoFileJson<PortalSettings>("Settings", "W@NtUz81");
#endif
#if DEBUG
            _Settings = new PublicFileJson<PortalSettings>("Settings");
#endif
            if (_Settings.Read() == null)
            {
                _Settings.Value = new PortalSettings();
            }

            Vesteros = new GPUser(new WCFUser() { Login = "Вестерос" }) { Title = App.GetResources("text_vesteros") };

#if DEBUG
            if (!DebugAuthorize())
#endif
            {
                if (!LocalAuthorize())
                {
                    ApiAuthorize();
                }
            }
        }

        #region Авторизация
#if DEBUG
        private bool DebugAuthorize()
        {
            AccauntWindow accauntWindow = new AccauntWindow();

            if (accauntWindow.ShowDialog() != true)
                Environment.Exit(555);

            if (App.Settings.Value.User?.login != null && App.Settings.Value.User?.uid == null)
            {
                LogIn();
                return true;
            }

            return false;
        }
#endif

        private bool LocalAuthorize()
        {
            if (App.Settings.Value.User?.email == null)
            {
                return false;
            }

            try
            {
                NameValueCollection param = new NameValueCollection
                {
                    { "email", App.Settings.Value.User.email},
                    { "clientId", App.ClientId }
                };
                AuthorizeResult item = ExtHttp.ExecuteCommand<AuthorizeResult>($"http://{App.Config.Settings.ServerAddress}:{App.Config.Settings.ServerPort}/WebHttpService/GetLogin", param);

                if (!string.IsNullOrWhiteSpace(item.Error))
                {
                    return false;
                }
                else if (!string.IsNullOrWhiteSpace(item.Login))
                {
                    App.Settings.Value.User = new user() { login = item.Login, email = item.Email };
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(App.GetResources("error_profileUpdate1"), App.GetResources("text_error"), MessageBoxButton.OK, MessageBoxImage.Stop);
                Environment.Exit(666);
            }

            LogIn();
            return true;

            /*//если данные о пользователе отсутствуют
            if (App.Settings.Value.User?.login == null || App.Settings.Value.access_token == null)
                return false;

            return Authorize();*/
        }

        /*private bool Authorize()
        {
            if (!App.Settings.Value.User.isFacebook)
            {
                if (!VkAuthWindow.Connect())
                {
                    //MessageBox.Show(App.GetResources("error_profileUpdate2"), App.GetResources("text_error"), MessageBoxButton.OK, MessageBoxImage.Stop);
                    return false;
                }
            }
            else
            {
                if (!FacebookWindow.Connect())
                {
                    //MessageBox.Show(App.GetResources("error_profileUpdate2"), App.GetResources("text_error"), MessageBoxButton.OK, MessageBoxImage.Stop);
                    return false;
                }
            }

            App.Settings.Value.User.clientId = App.ClientId;
            AuthorizeResult result = _Service.VKAuthorize(App.Settings.Value.User, null);

            while (result.Login == null)
            {
                LoginWindow _LoginWindow = new LoginWindow() { User = App.Settings.Value.User };
                _LoginWindow.error.Text = result.Error;
                _LoginWindow.email.Text = result.Email;
                _LoginWindow.fio.Text = $"{App.Settings.Value.User.first_name} {App.Settings.Value.User.last_name}";

                if (_LoginWindow.ShowDialog() != true)
                    Environment.Exit(555);

                App.Settings.Value.User.email = _LoginWindow.email.Text;
                result = _Service.VKAuthorize(App.Settings.Value.User, Crypto.SHA1Hex(_LoginWindow.password.Password));

            }

            App.Settings.Value.User.login = result.Login;
            LogIn();
            return true;
        }*/

        private void ApiAuthorize()
        {
            ApiWindow apiWindows = new ApiWindow();
            if (apiWindows.ShowDialog() != true)
            {
                Environment.Exit(555);
            }

            if (!string.IsNullOrWhiteSpace(App.Settings.Value.User?.login))
            {
                LogIn();
            }
            else
            //if (!Authorize())
            {
                MessageBox.Show(App.GetResources("error_profileUpdate1"), App.GetResources("text_error"), MessageBoxButton.OK, MessageBoxImage.Stop);
                Environment.Exit(666);
            }
        }

        private void LogIn()
        {

            try
            {
                User = GetUser(App.Settings.Value.User.login, Guid.Empty);
            }
            catch (Exception exp)
            {
                MessageBox.Show($"{exp.Message}\n\n{exp.InnerException?.Message}");
                App.Settings.Value.User = null; App.Settings.Value.access_token = null;
                Load();
                return;
            }

            App.Settings.Value.LastClientVersion = App.ClientVersion;
            App.Settings.Write();

            IsAdmin = User.Title.Contains(App.GetResources("titleType_Создатель")) ? true : false;
        }
        #endregion

        #region AsyncFunc
        public void UpdateUsersAsync(Action<int, int> report, Action<Exception> completed)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                App.TaskFactory.StartNew(() =>
                {
                    try
                    {
                        List<ProfileVersion> items = _Service.GetProfilesVersion();
                        //удаляем устаревшие профили
                        _Settings.Value.GamePortal.Where(p => !items.Any(p1 => p1.Login == p.Key)).ToList().ForEach(p =>
                        {
                            while (_Settings.Value.GamePortal.TryRemove(p.Key, out GPUser removeItem)) { }
                        });

                        List<GPUser> updateItems = items.Select(p =>
                        {
                            if (!_Settings.Value.GamePortal.TryGetValue(p.Login, out GPUser value))
                            {
                                value = new GPUser() { Login = p.Login };
                                _Settings.Value.GamePortal.TryAdd(p.Login, value);
                                return value;
                            }
                            else if (p.Version != value.Version)
                            {
                                return value;
                            }
                            else
                            {
                                return null;
                            }
                        }).Where(p => p != null).ToList();

                        int progress = -1;
                        report?.Invoke(progress, updateItems.Count);

                        foreach (GPUser item in updateItems)
                        {
                            report?.Invoke(++progress, updateItems.Count);
                            UpdateUserFromServer(item);
                        }

                        report?.Invoke(++progress, items.Count);

                        Filter = string.Empty;
                        OnlineOnly = false;
                        GPUser.ResetStaticValue();
                        Awards.ResetStaticValue();

                        _Settings.Value.GamePortal.Values.ToList().ForEach(p => p.Update());
                        _Settings.Value.GamePortal.Values.ToList().ForEach(p => UpdateTitle(p));
                        Awards.AwardsUpdate(_Settings.Value.GamePortal);
                        UsersSortedBy("0");
                        MasterOfDonate = _Settings.Value.GamePortal.Values.Where(p => p.LastPayment != null).OrderBy(p => p.LastPayment.Time).LastOrDefault();

                        //Расчёт данных за Вестерос
                        RateValues = new Dictionary<string, double>
                        {
                            { "DurationDay", _Settings.Value.GamePortal.Values.Sum(p => p.RateValues["DurationHours"]) / 24 },
                            { "MindRate", (int)_Settings.Value.GamePortal.Values.Average(p => p.RateValues["MindRate"]) },
                            { "HonorRate", (int)_Settings.Value.GamePortal.Values.Average(p => p.RateValues["HonorRate"]) },
                            { "LikeRate", (int)_Settings.Value.GamePortal.Values.Average(p => p.RateValues["LikeRate"]) }
                        };

                        MindUserGames = _Settings.Value.GamePortal.Values.SelectMany(p => p.MindUserGames.Where(p1 => p1.MindPosition == 1)).ToList();

                        ProgressViewModels = new Dictionary<string, ProgressViewModel>();
                        for (int i = 0; i <= 10; i += 2)
                        {
                            List<WCFUserGame> winTypeGames = MindUserGames.Where(p => p.GameType == i || p.GameType == i + 1).ToList();
                            int gameCount = winTypeGames.Count();
                            if (i != 2)
                            {
                                ProgressViewModels.Add($"Баратеон_{i}", MindByHome(i, "Баратеон", gameCount, winTypeGames));
                                ProgressViewModels.Add($"Ланнистер_{i}", MindByHome(i, "Ланнистер", gameCount, winTypeGames));
                                ProgressViewModels.Add($"Старк_{i}", MindByHome(i, "Старк", gameCount, winTypeGames));
                                if (i == 0 || i == 10)
                                {
                                    ProgressViewModels.Add($"Мартелл_{i}", MindByHome(i, "Мартелл", gameCount, winTypeGames));
                                }

                                if (i != 8)
                                {
                                    ProgressViewModels.Add($"Грейджой_{i}", MindByHome(i, "Грейджой", gameCount, winTypeGames));
                                }

                                if (i < 6)
                                {
                                    ProgressViewModels.Add($"Тирелл_{i}", MindByHome(i, "Тирелл", gameCount, winTypeGames));
                                }
                            }
                            else
                            {
                                ProgressViewModels.Add($"dragon_Баратеон_{i}", MindByHome(i, "dragon_Баратеон", gameCount, winTypeGames));
                                ProgressViewModels.Add($"dragon_Ланнистер_{i}", MindByHome(i, "dragon_Ланнистер", gameCount, winTypeGames));
                                ProgressViewModels.Add($"dragon_Болтон_{i}", MindByHome(i, "dragon_Болтон", gameCount, winTypeGames));
                                ProgressViewModels.Add($"dragon_Мартелл_{i}", MindByHome(i, "dragon_Мартелл", gameCount, winTypeGames));
                                ProgressViewModels.Add($"dragon_Грейджой_{i}", MindByHome(i, "dragon_Грейджой", gameCount, winTypeGames));
                                ProgressViewModels.Add($"dragon_Тирелл_{i}", MindByHome(i, "dragon_Тирелл", gameCount, winTypeGames));
                            }
                        }

                        //this.MaximumRate = ProgressViewModels.Values.Max(p => p.Value);
                        //this.MaximumRateScale = 30 / this.MaximumRate;

                        //биндинг профиля (не Background)
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            OnPropertyChanged("");
                            User.OnPropertyChanged("");
                        }), DispatcherPriority.ApplicationIdle).Wait();

                        SettingsSave();

                        completed?.Invoke(null);

                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            UserOnlineTickAsync(null, null);
                        }), DispatcherPriority.ApplicationIdle);

#if !DEBUG
                        //запуск клиента уведомлений
                        NotifiStart();
#endif
                    }
                    catch (Exception exp)
                    {
                        completed?.Invoke(exp);
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_profileLoad") + exp.Message);
                    }
                });
            }), DispatcherPriority.ApplicationIdle);
        }

        private void UserOnlineTickAsync(object sender, EventArgs e)
        {
            _UserOnlineTimer.Stop();
            _Settings.Value.GamePortal.Values.ToList().ForEach(p => p.OnLineStatus = false);

            App.TaskFactory.StartNew(() =>
            {
                List<string> result = _Service.GetOnlineUsers(User.Login);
                if (result == null)
                {
                    return;
                }

                foreach (string item in result)
                {
                    GPUser user = GetUser(item);

                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        user.OnLineStatus = true;
                    }), DispatcherPriority.ApplicationIdle);
                }

                _UserOnlineTimer.Start();
            });
        }

        private static bool IsBusy;

        public void LikeUserAsync(GPUser likeUser, bool? isLike)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                App.TaskFactory.StartNew(() =>
                {
                    try
                    {
                        _Service.LikeRate(User.Login, likeUser.Login, isLike);
                        UpdateUserFromServer(likeUser);
                    }
                    catch (Exception exp)
                    {
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_profileLoad") + exp.Message);
                    }

                    IsBusy = false;
                });
            }), DispatcherPriority.ApplicationIdle);
        }

        public void PassRateAsync(Guid id, Action completed)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                App.TaskFactory.StartNew(() =>
                {
                    try
                    {
                        _Service.PassRate(User.Login, id);
                        UpdateUserFromServer();

                        completed?.Invoke();
                    }
                    catch (Exception exp)
                    {
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_rateChange") + exp.Message);
                    }

                    IsBusy = false;
                });
            }), DispatcherPriority.ApplicationIdle);
        }

        public void SpecialUserAsync(GPUser specialUser, bool? isBlock)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                App.TaskFactory.StartNew(() =>
                {
                    try
                    {
                        bool result = _Service.SpecialUser(User.Login, specialUser.Login, isBlock);
                        if (result)
                        {
                            UpdateUserFromServer();
                            UpdateUserFromServer(specialUser);
                        }
                        else
                        {
                            Donate();
                        }
                    }
                    catch (Exception exp)
                    {
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_specialUsers") + exp.Message);
                    }

                    IsBusy = false;
                });
            }), DispatcherPriority.ApplicationIdle);
        }

        public void InviteUserAsync(string login, string text)
        {
            if (User.Login != login && User.AllPower < 200)
            {
                Donate();
                return;
            }

            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                App.TaskFactory.StartNew(() =>
                {
                    try
                    {
                        bool? result = _Service.InviteUser(User.Login, login, text);

                        switch (result)
                        {
                            case null:
                                Donate();
                                break;
                            case false:
                                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_notify"), App.GetResources("notify_inviteUser"));
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception exp)
                    {
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_inviteUser") + exp.Message);
                    }

                    IsBusy = false;
                });
            }), DispatcherPriority.ApplicationIdle);
        }

        public void GetLikeProfileAsync(Action<List<string>> completed)
        {
            if (completed == null)
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                App.TaskFactory.StartNew(() =>
                {
                    try
                    {
                        List<string> result = _Service.GetLikeProfile(User.Login);

                        if (result == null)
                        {
                            throw new Exception(App.GetResources("text_accessDenied"));
                        }

                        completed(result);

                    }
                    catch (Exception exp)
                    {
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_getLinkAccount") + exp.Message);
                    }
                });
            }), DispatcherPriority.ApplicationIdle);
        }

        public void LinkAccountsAsync(string login, string password, Action completed)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                App.TaskFactory.StartNew(() =>
                {
                    try
                    {
                        bool result = _Service.LinkAccounts(User.Login, login, password);

                        if (!result)
                        {
                            throw new Exception(App.GetResources("text_accessDenied"));
                        }
                        else
                        {
                            completed?.Invoke();
                        }
                    }
                    catch (Exception exp)
                    {
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_linkAccount") + exp.Message);
                    }
                });
            }), DispatcherPriority.ApplicationIdle);
        }

        public void ClearProfileAsync(Action completed)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                App.TaskFactory.StartNew(() =>
                {
                    if (MessageBox.Show(App.GetResources("text_clearRating"),
                        App.GetResources("text_warning"),
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Exclamation,
                        MessageBoxResult.No) != MessageBoxResult.Yes)
                    {
                        return;
                    }

                    try
                    {
                        bool result = _Service.ClearProfile(User.Login);
                        if (!result)
                        {
                            throw new Exception(App.GetResources("text_accessDenied"));
                        }
                        else
                        {
                            completed?.Invoke();
                        }
                    }
                    catch (Exception exp)
                    {
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_clearProfile") + exp.Message);
                    }
                });
            }), DispatcherPriority.ApplicationIdle);
        }
        #endregion

        /// <summary>
        /// подготавливает список пользователей
        /// </summary>
        /// <param name="param">параметр сортировки</param>
        /// <param name="userGame">фильтр по игре</param>
        /// <param name="filterText">фильтр по ФИО</param>
        /// <returns></returns>
        public List<GPUser> GetUsers(string param, WCFUserGame userGame = null)
        {
            List<GPUser> sortedUsers = UsersSortedBy(param);
            if (userGame == null)
            {
                sortedUsers.ForEach(p => p.Position = string.Empty);
            }

            IEnumerable<GPUser> result = sortedUsers.Where(p => !p.IsIgnore);
            if (userGame != null)
            {
                if (userGame.EndTime.HasValue && userGame.HonorPosition == 5)
                {
                    result = result.Where(p => p != User && p.CheckCrossedBy(userGame));
                }
                else
                {
                    return new List<GPUser>();
                }
            }
            else
            {
                if (OnlineOnly)
                {
                    result = result.Where(p => p.OnLineStatus == true);
                }

                if (Filter != null)
                {
                    result = result.Where(p => p.Title.ToLower().Contains(Filter));
                }
                else if (!OnlineOnly)
                {
                    result = result.Where(p => p.inRate == true);
                }
            }

            return result.Take(100).ToList();
        }

        private List<GPUser> UsersSortedBy(string param = "")
        {
            List<GPUser> result = _Settings.Value.GamePortal.Values.ToList();
            switch (param)
            {
                case "1":
                    result = result.OrderByDescending(p => p.RateValues["HonorRate"])
                        .ThenByDescending(p => p.RateValues["DurationHours"]).ToList();
                    break;

                case "2":
                    result = result.OrderByDescending(p => p.RateValues["LikeRate"])
                        .ThenByDescending(p => p.RateValues["DurationHours"]).ToList();
                    break;

                case "3":
                    result = result.OrderByDescending(p => p.Awards.Values["AwardsCount"])
                        .ThenByDescending(p => p.RateValues["DurationHours"]).ToList();
                    break;

                default:
                    result = result.OrderByDescending(p => p.RateValues["MindRate"])
                        .ThenByDescending(p => p.RateValues["DurationHours"]).ToList();
                    break;
            }

            return result;
        }

        public GPUser GetUser(string login, Guid? version = null)
        {
            if (string.IsNullOrEmpty(login))
            {
                return null;
            }

            if (login == "System" || login == "Вестерос")
            {
                return null;
            }

            if (_Settings.Value.GamePortal.TryGetValue(login, out GPUser result))
            {
                //проверяем версию
                if (version != null && result.Version != version)
                {
                    //обновляем данные
                    UpdateUserFromServer(result);
                }
            }
            else
            {
                GPUser gpUser = new GPUser(new WCFUser() { Login = login });
                UpdateUserFromServer(gpUser);

                _Settings.Value.GamePortal.TryAdd(login, gpUser);
                result = gpUser;
            }

            return result;
        }

        public void SetLeaderBoardVisibility(List<WCFGameUser> gameUsers, bool clearOld)
        {
            if (clearOld)
            {
                _Settings.Value.GamePortal.Values.Where(p => p.LeaderBoardVisibility != Visibility.Visible && !gameUsers.Any(p1 => p1.Login == p.Login)).ToList()
                    .ForEach(p => { p.LeaderBoardVisibility = Visibility.Visible; p.OnPropertyChanged("LeaderBoardVisibility"); });
            }

            gameUsers.Select(p => GetUser(p.Login)).ToList()
                .ForEach(p => { p.LeaderBoardVisibility = Visibility.Collapsed; p.OnPropertyChanged("LeaderBoardVisibility"); });
        }

        public void UpdateUserFromServer(GPUser user = null, Action completed = null)
        {
            //если пусто то текущий пользователь
            if (user == null)
            {
                user = User;
            }

            //запрашиваем новые данные
            WCFUser value = _Service.GetProfileByLogin(user.Login);
            if (value == null)
            {
                throw new Exception("Логин не найден");
            }

            //обновляем
            user.Update(value);
            user.Update();
            UpdateTitle(user);

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                user.OnPropertyChanged("");
            }), DispatcherPriority.ApplicationIdle);

            completed?.Invoke();
        }

        public void UpdateTitle(GPUser user)
        {
            //специальные титулы
            StringBuilder sb = new StringBuilder();
            ((WCFUser)user).Title.ForEach(p =>
            {
                sb.AppendFormat("{0}, ", App.TextDecoder(p));
            });

            //переходящие титулы
            if (user.RateValues["AllPower"] == GPUser.MaxRateValues["AllPower"])
            {
                sb.AppendFormat("{0}, ", App.GetResources("titleType_мастерНадМонетой"));
            }

            if (user.inRate == true)
            {
                if (user.RateValues["MindRate"] == GPUser.MaxRateValues["MindRate"])
                {
                    sb.AppendFormat("{0}, ", App.GetResources("titleType_лордКомандующий"));
                }

                if (user.RateValues["HonorRate"] == GPUser.MaxRateValues["HonorRate"])
                {
                    sb.AppendFormat("{0}, ", App.GetResources("titleType_мастерНадЗаконами"));
                }

                if (user.RateValues["LikeRate"] == GPUser.MaxRateValues["LikeRate"])
                {
                    sb.AppendFormat("{0}, ", App.GetResources("titleType_мастерНадШептунами"));
                }
            }

            //базовые титулы
            sb.AppendFormat("{0}, ", user.TitleMin);

            sb.Append(user.Api["FIO"]);
            user.Title = sb.ToString();

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                user.OnPropertyChanged("Title");
            }), DispatcherPriority.ApplicationIdle);
        }

        public ProgressViewModel MindByHome(int gameType, string homeType, int typeGameCount, List<WCFUserGame> winTypeGames)
        {
            int homeVictoryCount = winTypeGames.Count(p => p.HomeType == homeType);
            double value = typeGameCount == 0 ? 0 : (homeVictoryCount / (double)typeGameCount) * 100;
            return new ProgressViewModel(gameType, homeType, homeVictoryCount, homeVictoryCount, value);
        }

        private void NotifiStart()
        {
            try
            {
                if (Process.GetProcesses(".").Count(p => p.ProcessName == "NotifiClient") > 0)
                {
                    return;
                }

                Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"NotifiClient\Updater.exe"));
            }
            catch (Exception exp)
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_notifyLoad") + exp.Message);
            }
        }

        #region donate
        public void Donate()
        {
            try
            {
                if (CultureInfo.CurrentUICulture.Name.Substring(0, 2) == "ru")
                {
                    Process.Start(string.Format("http://lantsev1981.ru/Donate/donate_ru.html?lang=ru&id={0}", User.Login));
                }
                else
                {
                    Process.Start(string.Format("http://lantsev1981.ru/Donate/donate_en.html?lang=en&id={0}", User.Login));
                }
            }
            catch (Exception exp)
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("text_uriError") + exp.Message);
            }
        }
        #endregion

        public void UserMute(GPUser user)
        {
            if (user == null || user.Login == "Вестерос")
            {
                return;
            }

            if (User.Login != user.Login && User.AllPower < 300)
            {
                Donate();
                return;
            }

            user.ChatVisibility = user.ChatVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            user.OnPropertyChanged("ChatVisibility");
        }

        public void CheckSpecialUser(GPUser specialUser, bool isBlock)
        {
            WCFSpecialUser value = User.SpecialUsers.SingleOrDefault(p => p.SpecialLogin == specialUser.Login);
            if (value != null)
            {
                if (value.IsBlock == isBlock)
                {
                    MainWindow.GamePortal.SpecialUserAsync(specialUser, null);
                    return;
                }
            }

            MainWindow.GamePortal.SpecialUserAsync(specialUser, isBlock);
        }

        public void Settings()
        {
            if (User.AllPower < 100)
            {
                Donate();
                return;
            }

            App.Agot2.settingsView.Show(App.Settings.Value);
        }

        public void SettingsSave()
        {
            if (_Settings.Value == null)
            {
                return;
            }

            if (!_Settings.Write())
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_profileSave") + _Settings.Exp.Message);
            }
        }
    }
}
