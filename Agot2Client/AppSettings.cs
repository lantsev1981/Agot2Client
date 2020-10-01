using GamePortal;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;

namespace Agot2Client
{
    public class AppSettings
    {
        public string LastClientVersion { get; set; }
        public string LastGameId { get; set; }

        [JsonIgnore]
        public string access_token { get; set; }
        public user User { get; set; }

        public string Lang { get; set; }
        private List<string> Langs = new List<string> { "en", "ru", "de", "fr", "pt", "it", "pl","bg","hu" };
        public List<Vol> Vols { get; set; }

        public void SetDefault()
        {
            Vols = new List<Vol>
            {
                new Vol() { FileName = "Media/main_title.mp3", Name = "settings_Главная_тема", Value = 0.1 },
                new Vol() { FileName = "Media/Windows Notify System Generic.wav", Name = "settings_Новый_ход", Value = 1 },
                new Vol() { FileName = "Media/Windows Message Nudge.wav", Name = "hint_chatTab", Value = 1 },
                new Vol() { FileName = "Media/Windows Message Nudge.wav", Name = "hint_gameChatTab", Value = 1 },
                new Vol() { FileName = "Media/StepTimer.wav", Name = "settings_Таймер", Value = 1 }
            };
            Lang = CultureInfo.CurrentUICulture.Name.Substring(0, 2);
            if (!Langs.Contains(Lang)) Lang = "en";
        }
    }

    public class Vol
    {
        public string FileName { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }

        [JsonIgnore]
        public string ViewName
        {
            get { return App.GetResources(Name); }
        }
    }
}
