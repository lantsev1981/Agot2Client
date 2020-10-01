using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Agot2Client
{
    class YandexTranslate
    {
        static readonly string key = "trnsl.1.1.20150922T121717Z.45b0b3f64a14c1fb.88a0d3c5c5a3dddb6d043598e12ac800f9a44b65";
        static readonly string uri = "https://translate.yandex.net/api/v1.5/tr.json/{0}?key={1}&{2}";
        static GetLangsResp translationDirection;
        static string defaultLang;

        static YandexTranslate()
        {
            var langName = string.Empty;
            defaultLang = CultureInfo.CurrentUICulture.Name.Substring(0, 2);
            translationDirection = GetLangs();
            if (translationDirection == null)
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_getLangs"));
                defaultLang = "en";
            }
            else if (translationDirection.Langs == null || !translationDirection.Langs.TryGetValue(defaultLang, out langName))
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), string.Format(App.GetResources("error_undetectedLang"), defaultLang));
                defaultLang = "en";
            }
            else if (translationDirection.Dirs == null || !translationDirection.Dirs.Any(p => p.Contains(defaultLang)))
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), string.Format(App.GetResources("error_translateLng"), defaultLang));
                defaultLang = "en";
            }
        }

        static GetLangsResp GetLangs()
        {
            try
            {
                NameValueCollection qs = new NameValueCollection
                {
                    ["ui"] = defaultLang
                };
                return ExecuteJsonCommand<GetLangsResp>("getLangs", qs);
            }
            catch (APIErrorException) { }
            catch (Exception exp)
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), exp.Message);
                SaveException(exp);
            }

            App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_decodeText"));
            return null;
        }

        static bool IsBusy;
        static public void Translate(string text, Action<string> completed)
        {
            if (completed == null || IsBusy || string.IsNullOrEmpty(text))
                return;

            IsBusy = true;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                new Task(() =>
                {
                    try
                    {
                        NameValueCollection qs = new NameValueCollection
                        {
                            ["text"] = text,
                            ["lang"] = defaultLang,
                            ["format"] = "plain",
                            ["options"] = "1"
                        };

                        var response = ExecuteJsonCommand<TranslateResp>("translate", qs);

                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            completed(response.Text[0]);
                        }), DispatcherPriority.ApplicationIdle);
                    }
                    catch (APIErrorException)
                    {
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_decodeText"));
                    }
                    catch (Exception exp)
                    {
                        SaveException(exp);
                        App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), App.GetResources("error_decodeText"));
                    }

                    IsBusy = false;
                }).Start();
            }), DispatcherPriority.ApplicationIdle);
        }

        static T ExecuteJsonCommand<T>(string name, NameValueCollection qs) where T : Response
        {
            T result;

            var url = String.Format(uri,
                name, key, String.Join("&", (from item in qs.AllKeys select item + "=" + qs[item]).ToArray()));

            var request = (HttpWebRequest)WebRequest.Create(url);
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new APIErrorException(String.Format("Server error (HTTP {0}: {1}).",
                        response.StatusCode,
                        response.StatusDescription));

                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    var text = sr.ReadToEnd();
                    result = JsonConvert.DeserializeObject<T>(text);
                    if (result.Code > 200)
                        throw new APIErrorException(String.Format("Server error (code {0}).",
                        result.Code));
                }
            }
            request.Abort();

            return result;
        }

        static void SaveException(Exception exp)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "YaException");
            Directory.CreateDirectory(path);
            File.WriteAllText(string.Format("{0}/{1}.txt", path, Guid.NewGuid()), exp.Message);
        }

        class APIErrorException : Exception
        {
            public APIErrorException(string message)
                : base(message)
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "YaException");
                Directory.CreateDirectory(path);
                File.WriteAllText(string.Format("{0}/{1}.txt", path, Guid.NewGuid()), message);
            }
        }

        class GetLangsResp : Response
        {
            public List<string> Dirs { get; set; }
            public Dictionary<string, string> Langs { get; set; }
        }

        class TranslateResp : Response
        {
            public Detected Detected { get; set; }
            public string Lang { get; set; }
            public List<string> Text { get; set; }
        }

        class Detected
        {
            public string Lang { get; set; }
        }

        class Response
        {
            public int Code { get; set; }
        }
    }
}
