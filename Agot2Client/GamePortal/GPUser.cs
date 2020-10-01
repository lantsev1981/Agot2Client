using GamePortal;
using Lantsev;
using MyMod;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Agot2Client
{
    public class GPUser : WCFUser, INotifyPropertyChanged
    {
        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        #region static values
        public static Dictionary<string, int> MaxRateValues;
        public static Dictionary<string, int> MinRateValues;
        static GPUser()
        {
            ResetStaticValue();
        }

        public static void ResetStaticValue()
        {
            MaxRateValues = new Dictionary<string, int>();
            MinRateValues = new Dictionary<string, int>();
        }
        #endregion

        public string DonateComment { get; set; }
        public bool? inRate;

        public Dictionary<string, int> RateValues { get; private set; }
        public Dictionary<string, ProgressViewModel> ProgressViewModels { get; private set; }

        /// <summary>
        /// Завершённые игры - игры по которым не королевского прощения
        /// (перезагрузка сервера)
        /// </summary>
        public List<WCFUserGame> EndedUserGames { get; private set; }
        public List<WCFUserGame> MindUserGames { get; private set; }

        public Awards Awards { get; private set; }

        public new string Title { get; set; }
        public string TitleMin
        {
            get
            {
                switch (this.AllPower / 100)
                {
                    case 0: return App.GetResources("titleType_Бездомный");
                    case 1: return App.GetResources("titleType_Крестьянин");
                    case 2: return App.GetResources("titleType_Купец");
                    case 3: return App.GetResources("titleType_Рыцарь");
                    default: return App.GetResources("titleType_Лорд");
                }
            }
        }

        public string FriendImage => MainWindow.GamePortal.User.SpecialUsers.Any(p => p.SpecialLogin == this.Login && !p.IsBlock)
                    ? "/Image/friend.png"
                    : "/Image/unfriend.png";

        public string BlockImage => MainWindow.GamePortal.User.SpecialUsers.Any(p => p.SpecialLogin == this.Login && p.IsBlock)
                    ? "/Image/block.png"
                    : "/Image/unblock.png";

        public bool LikeBtnEnable
        {
            get
            {
                WCFUserLike userLike = UserLikes.SingleOrDefault(p => p.LikeLogin == MainWindow.GamePortal.User.Login);
                if (userLike != null && userLike.IsLike)
                    return false;
                else
                    return true;
            }
        }

        public bool UnLikeBtnEnable
        {
            get
            {
                WCFUserLike userLike = UserLikes.SingleOrDefault(p => p.LikeLogin == MainWindow.GamePortal.User.Login);
                if (userLike != null && !userLike.IsLike)
                    return false;
                else
                    return true;
            }
        }

        public bool ClearBtnEnable
        {
            get
            {
                WCFUserLike userLike = UserLikes.SingleOrDefault(p => p.LikeLogin == MainWindow.GamePortal.User.Login);
                if (userLike == null)
                    return false;
                else
                    return true;
            }
        }

        private bool _OnLineStatus;
        public bool OnLineStatus
        {
            get => _OnLineStatus;
            set
            {
                _OnLineStatus = value;
                OnPropertyChanged("OnLineStatus");
            }
        }

        public string Position { get; set; }

        public Visibility ChatVisibility { get; set; }

        public Visibility LeaderBoardVisibility { get; set; }
        public GPUser() { }

        public GPUser(WCFUser value)
        {
            this.Update(value);
        }

        private void SetRateValue(string key, int value, bool inRateIgnore = false)
        {
            RateValues.Add(key, value);

            if (!inRate.HasValue)
                inRate = RateValues["DurationHours"] > 48 && MindUserGames.Count >= 10 && DateTimeOffset.UtcNow - this.LastConnection < TimeSpan.FromDays(7) ? true : false;

            if (inRateIgnore || inRate == true)
            {
                if (!MaxRateValues.ContainsKey(key))
                {
                    MaxRateValues.Add(key, value);
                    MinRateValues.Add(key, value);
                }
                else
                {
                    RateValues[key] = value;
                    if (value > MaxRateValues[key])
                        MaxRateValues[key] = value;
                    if (value < MinRateValues[key])
                        MinRateValues[key] = value;
                }
            }
        }

        /// <summary>
        /// Расчитывает рейтинг
        /// </summary>
        public void Update()
        {
            DonateComment = LastPayment == null || LastPayment.Comment == null ? null : string.Format(App.GetResources("text_donateComment"), LastPayment.Comment);
            RateValues = new Dictionary<string, int>();
            EndedUserGames = UserGames.Where(p => !p.IsIgnoreHonor && p.EndTime.HasValue).ToList();
            MindUserGames = EndedUserGames.Where(p => !p.IsIgnoreMind).ToList();
            SetRateValue("DurationHours", (int)EndedUserGames.Where(p => !p.IsIgnoreDurationHours).Sum(p => (p.EndTime.Value - p.StartTime).TotalHours));
            SetRateValue("MindRate", MindUserGames.Count == 0 ? 0 : (int)(MindUserGames.Count(p => p.MindPosition == 1) / (float)MindUserGames.Count * 100));
            SetRateValue("HonorRate", EndedUserGames.Count == 0 ? 0 : (int)(EndedUserGames.Count(p => p.HonorPosition == 5) / (float)EndedUserGames.Count * 100));
            SetRateValue("LikeRate", UserLikes.Count(p => p.IsLike) - this.UserLikes.Count(p => !p.IsLike));
            SetRateValue("AllPower", AllPower, true);

            ProgressViewModels = new Dictionary<string, ProgressViewModel>();
            for (int i = 0; i <= 10; i += 2)
            {
                List<WCFUserGame> typeGames = MindUserGames.Where(p => p.GameType == i || p.GameType == i + 1).ToList();
                if (i != 2)
                {
                    ProgressViewModels.Add($"Баратеон_{i}", MindByHome(i, "Баратеон", typeGames));
                    ProgressViewModels.Add($"Ланнистер_{i}", MindByHome(i, "Ланнистер", typeGames));
                    ProgressViewModels.Add($"Старк_{i}", MindByHome(i, "Старк", typeGames));
                    if (i == 0 || i == 10) ProgressViewModels.Add($"Мартелл_{i}", MindByHome(i, "Мартелл", typeGames));
                    if (i != 8) ProgressViewModels.Add($"Грейджой_{i}", MindByHome(i, "Грейджой", typeGames));
                    if (i < 6) ProgressViewModels.Add($"Тирелл_{i}", MindByHome(i, "Тирелл", typeGames));
                }
                else
                {
                    ProgressViewModels.Add($"dragon_Баратеон_{i}", MindByHome(i, "dragon_Баратеон", typeGames));
                    ProgressViewModels.Add($"dragon_Ланнистер_{i}", MindByHome(i, "dragon_Ланнистер", typeGames));
                    ProgressViewModels.Add($"dragon_Болтон_{i}", MindByHome(i, "dragon_Болтон", typeGames));
                    ProgressViewModels.Add($"dragon_Мартелл_{i}", MindByHome(i, "dragon_Мартелл", typeGames));
                    ProgressViewModels.Add($"dragon_Грейджой_{i}", MindByHome(i, "dragon_Грейджой", typeGames));
                    ProgressViewModels.Add($"dragon_Тирелл_{i}", MindByHome(i, "dragon_Тирелл", typeGames));
                }
            }

            Awards = new Awards(this);
        }

        private ProgressViewModel MindByHome(int gameType, string homeType, List<WCFUserGame> typeGames)
        {
            List<WCFUserGame> homeGame = typeGames.Where(p => p.HomeType == homeType).ToList();
            int homeVictoryCount = homeGame.Count(p => p.MindPosition == 1);
            float efficiency = homeGame.Count == 0 ? 0 : homeVictoryCount / (float)homeGame.Count * 100;
            double value = homeGame.Count == 0 ? 0 : homeGame.Average(p => p.MindPosition == 0 ? 0 : 100 - 20 * (p.MindPosition - 1));
            return new ProgressViewModel(gameType, homeType, homeVictoryCount, homeGame.Count, value, (int)efficiency);
        }

        public bool CheckRateAllow(WCFRateSettings rateSettings)
        {
            //TODO проверить на сервере проходит ли пользователь по рейтингу
            return this.RateValues["MindRate"] >= rateSettings.MindRate
                && this.RateValues["HonorRate"] >= rateSettings.HonorRate
                && this.RateValues["LikeRate"] >= rateSettings.LikeRate
                ? true : false;
        }

        public bool CheckCrossedBy(WCFUserGame userGame)
        {
            if (!userGame.EndTime.HasValue)
                return false;

            if (EndedUserGames == null)
                return false;

            return EndedUserGames.Where(p => p.GameId == userGame.GameId).Any(p =>
            {
                IEnumerable<WCFUserGame> CheckingGames = new WCFUserGame[2] { p, userGame };
                CheckingGames = CheckingGames.OrderBy(p1 => p1.EndTime.Value - p1.StartTime);

                if (CheckingGames.ElementAt(0).StartTime >= CheckingGames.ElementAt(1).StartTime && CheckingGames.ElementAt(0).StartTime <= CheckingGames.ElementAt(1).EndTime.Value)
                    return true;
                if (CheckingGames.ElementAt(0).EndTime.Value >= CheckingGames.ElementAt(1).StartTime && CheckingGames.ElementAt(0).EndTime.Value <= CheckingGames.ElementAt(1).EndTime.Value)
                    return true;

                return false;
            });
        }

        #region photo
#if !DEBUG
        private int tryCount = 3;
#endif
        private static readonly BitmapImage Vesteros = ImageExt.Load("/Image/logo.ico", Int32Rect.Empty);
        private static readonly BitmapImage UpdateImage = ImageExt.Load("/Image/update.png", Int32Rect.Empty);
        private static readonly BitmapImage ErrorImage = ImageExt.Load("/Image/block.png", Int32Rect.Empty);
        private static readonly BitmapImage DefaultImage = ImageExt.Load("/Image/barbarian.png", Int32Rect.Empty);
        public static TaskFactory TaskFactory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(1));
        private bool isUpdating;
        private BitmapImage _Photo;

        public BitmapImage Photo
        {
            get
            {
                if (Login == "Вестерос")
                    return Vesteros;

#if DEBUG
                else
                    return ErrorImage;
#endif

#if !DEBUG
                if (_Photo != null)
                    return _Photo;

                if (isUpdating)
                    return UpdateImage;

                if(string.IsNullOrWhiteSpace(Api["photo"]))
                    return DefaultImage;

                if (tryCount >= 0)
                {
                    tryCount--;
                    isUpdating = true;
                    //дожидаемся завершения других операций
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //запускаем процесс
                        TaskFactory.StartNew(() =>
                        {
                            try
                            {
                                WebRequest request = HttpWebRequest.Create(Api["photo"]);
                                using (WebResponse response = request.GetResponse())
                                using (Stream responseStream = response.GetResponseStream())
                                using (MemoryStream memoryStream = new MemoryStream())
                                using (BinaryWriter writer = new BinaryWriter(memoryStream))
                                {
                                    for (int i = 0; ; i++)
                                    {
                                        int b = responseStream.ReadByte();
                                        if (b == -1)
                                            break;
                                        writer.Write((byte)b);
                                    }

                                    byte[] result = memoryStream.ToArray();
                                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        _Photo = ImageExt.Load(result);
                                        isUpdating = false;
                                        OnPropertyChanged("Photo");
                                    }), DispatcherPriority.ApplicationIdle);
                                }
                            }
                            catch (Exception exp)
                            {
                                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), exp.Message);
                                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    isUpdating = false;
                                }), DispatcherPriority.ApplicationIdle);
                            }
                        });
                    }), DispatcherPriority.ApplicationIdle);

                    return UpdateImage;
                }
                
                return ErrorImage;
#endif
            }
        }
        #endregion
    }
}
