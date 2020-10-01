using GameService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Agot2Client
{
    /// <summary>
    /// Награды
    /// </summary>
    public class Awards
    {
        #region static values
        private static Dictionary<string, AwardItem> MaxValues;

        static Awards()
        {
            ResetStaticValue();
        }

        public static void ResetStaticValue()
        {
            MaxValues = new Dictionary<string, AwardItem>();

            foreach (GameTypeItem gameType in MainWindow.GameTypes)
            {
                string imageUri = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "/Image/Awards/2.jpg");
                if (gameType.Id != 2)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        KeyValuePair<string, string> homeType = ExtHomeType.Keys.ElementAt(i);
                        string homeImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"/Image/{homeType.Value}/{homeType.Value}.png");
                        MaxValues.Add($"MaxTotalVictory*{homeType.Key}_{gameType.Id}", new AwardItem() {GameTypeItem=gameType, Name = App.TextDecoder($"dynamic_MaxTotalVictory2*homeType_{homeType.Key}"), Image = homeImagePath });
                        MaxValues.Add($"MaxEfficiency*{homeType.Key}_{gameType.Id}", new AwardItem() { GameTypeItem = gameType, Name = App.TextDecoder($"dynamic_MaxEfficiency*homeType_{homeType.Key}"), Image = imageUri, SecondImage = homeImagePath });
                    }
                }
                else
                {
                    for (int i = 6; i < 12; i++)
                    {
                        KeyValuePair<string, string> homeType = ExtHomeType.Keys.ElementAt(i);
                        string homeImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"/Image/{homeType.Value}/{homeType.Value}.png");
                        MaxValues.Add($"MaxTotalVictory*{homeType.Key}_{gameType.Id}", new AwardItem() { GameTypeItem = gameType, Name = App.TextDecoder($"dynamic_MaxTotalVictory2*homeType_{homeType.Key}"), Image = homeImagePath });
                        MaxValues.Add($"MaxEfficiency*{homeType.Key}_{gameType.Id}", new AwardItem() { GameTypeItem = gameType, Name = App.TextDecoder($"dynamic_MaxEfficiency*homeType_{homeType.Key}"), Image = imageUri, SecondImage = homeImagePath });
                    }
                }
            }
            MaxValues.Add("dynamic_MaxTotalVictory1", new AwardItem() { Name = App.TextDecoder("dynamic_MaxTotalVictory1"), Image = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "/Image/Awards/1.jpg") });
            MaxValues.Add("awardsType_SerLorasMax", new AwardItem() { Name = App.GetResources("awardsType_SerLorasMax"), Image = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "/Image/Awards/SerLoras.png") });
        }

        /// <summary>
        /// присваивает медали заслуженным игрокам
        /// </summary>
        /// <param name="users"></param>
        public static void AwardsUpdate(ConcurrentDictionary<string, GPUser> users)
        {
            foreach (KeyValuePair<string, AwardItem> item in MaxValues)
            {
                users.Values.Where(p => item.Value.Value > 0 && p.Awards.Values[item.Key] == item.Value.Value).ToList()
                    .ForEach(p => { p.Awards.PassingList.Add(item.Key, item.Value); p.Awards.Values["AwardsCount"]++; });
            }

            string imageUri = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "/Image/Awards/3.jpg");
            if (GPUser.MaxRateValues.ContainsKey("DurationHours"))
                users.Values.Where(p => p.RateValues["DurationHours"] == GPUser.MaxRateValues["DurationHours"]).ToList()
                    .ForEach(p => { p.Awards.PassingList.Add("awardsType_MaxDurationHours", new AwardItem() { Name = App.GetResources("awardsType_MaxDurationHours"), Image = imageUri, Value = GPUser.MaxRateValues["DurationHours"] }); p.Awards.Values["AwardsCount"]++; });
            

        }
        #endregion

        public Dictionary<string, double> Values { get; private set; }

        public Visibility VisibilityAllHouseVictory { get; private set; }

        public AwardItem SerLoras { get; private set; }


        /// <summary>
        /// Переходящие
        /// </summary>
        public Dictionary<string, AwardItem> PassingList { get; private set; }

        public Awards(GPUser user)
        {
            VisibilityAllHouseVictory = user.ProgressViewModels.All(p => p.Value.VictoryCount != 0) ? Visibility.Visible : Visibility.Collapsed;

            Values = new Dictionary<string, double>
            {
                { "AwardsCount", VisibilityAllHouseVictory == Visibility.Visible ? 1 : 0 }
            };

            foreach (KeyValuePair<string, AwardItem> item in MaxValues)
                Values.Add(item.Key, 0);

            PassingList = new Dictionary<string, AwardItem>();

            foreach (KeyValuePair<string, ProgressViewModel> item in user.ProgressViewModels)
            {
                if (item.Value.VictoryCount > 0)
                {
                    string key = $"MaxTotalVictory*{item.Key}";
                    //if (Values.ContainsKey(key))
                    //{
                    Values["AwardsCount"]++;
                    SetValue(key, item.Value.VictoryCount);
                    //}
                    //else
                    //{

                    //}
                }
            }

            SetValue("dynamic_MaxTotalVictory1", user.ProgressViewModels.Sum(p => p.Value.VictoryCount));

            foreach (KeyValuePair<string, ProgressViewModel> item in user.ProgressViewModels)
            {
                if (item.Value.Efficiency > 0 && item.Value.VictoryCount > 2)
                {
                    string key = $"MaxEfficiency*{item.Key}";
                    //if (Values.ContainsKey(key))
                    SetValue(key, item.Value.Efficiency);
                    //else
                    //{

                    //}
                }
            }


            var SerLorasCount = user.EndedUserGames.Where(p => p.HonorPosition < 5).Count();
            if (SerLorasCount > 0)
            {
                Values["AwardsCount"]++;
                SerLoras = new AwardItem() { Name = App.GetResources($"awardsType_SerLoras"), Image = "/Image/Awards/SerLoras.png", Value = SerLorasCount };
                SetValue("awardsType_SerLorasMax", SerLorasCount);
            }

            ////особые медали только рыцарям и выше
            //if (user.AllPower >= 300)
            //{
            //}
        }

        private void SetValue(string key, int value)
        {
            Values[key] = value;
            if (value > MaxValues[key].Value)
                MaxValues[key].Value = value;
        }

        public class AwardItem
        {
            public string Name { get; set; }
            public int Value { get; set; }
            public string Image { get; set; }
            public string SecondImage { get; set; }
            public GameTypeItem GameTypeItem { get; set; }
        }
    }
}
