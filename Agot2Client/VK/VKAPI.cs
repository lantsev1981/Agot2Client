using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Agot2Client
{
    public class VKAPI
    {
        public const string client_id = "4521453";
        public const string Scope = "wall,photos";//email,messages,notify,notifications,ads,offline

        public bool UpdateProfile()
        {
            if (!TrackVisitor())
                return false;

            try
            {
                var qs = new NameValueCollection
                {
                    ["uids"] = App.Settings.Value.User.uid,
                    ["fields"] = "photo"
                };
                var result = ExecuteJsonCommand<ListDictResponse>("getProfiles", qs);

                App.Settings.Value.User.first_name = result.response[0]["first_name"];
                App.Settings.Value.User.last_name = result.response[0]["last_name"];
                App.Settings.Value.User.photo = result.response[0]["photo"];

                return true;
            }
            //catch (WebException) {}
            catch (APIErrorException) { }
            catch (Exception exp)
            {
                SaveException(exp);
            }

            return false;
        }

        private bool TrackVisitor()
        {
            try
            {
                NameValueCollection qs = new NameValueCollection();
                var result = ExecuteJsonCommand<StringResponse>("stats.trackVisitor", qs);

                return true;
            }
            catch (APIErrorException) { }
            catch (Exception exp)
            {
                SaveException(exp);
            }

            return false;
        }

        /// <summary>
        /// Публикует текстовое сообщение на стене указанного пользователя
        /// </summary>
        /// <param name="uid">Идентификатор пользователя</param>
        /// <param name="message">Текст сообщения</param>
        public void WallPost(string message, string owner_id = null, string attachments = null)
        {
            try
            {
                NameValueCollection qs = new NameValueCollection
                {
                    ["owner_id"] = string.IsNullOrEmpty(owner_id) ? App.Settings.Value.User.uid : owner_id,
                    ["message"] = message
                };
                if (attachments != null)
                    qs["attachments"] = attachments;

                var result = ExecuteJsonCommand<DictResponse>("wall.post", qs);

            }
            catch (APIErrorException) { }
            catch (Exception exp)
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), exp.Message);
                SaveException(exp);
            }
        }

        public void MessagesSend(string message, string uid)
        {
            try
            {
                NameValueCollection qs = new NameValueCollection
                {
                    ["uid"] = uid,
                    ["message"] = message,
                    ["title"] = "A Game of Thrones: The Online Board Game"
                };

                var result = ExecuteJsonCommand<StringResponse>("messages.send", qs);
            }
            //catch (WebException) { }
            catch (APIErrorException) { }
            catch (Exception exp)
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), exp.Message);
                SaveException(exp);
            }
        }

        public string SendPhoto(string filename)
        {
            try
            {
                NameValueCollection qs = new NameValueCollection
                {
                    ["user_id "] = App.Settings.Value.User.uid
                };

                //получаем адрес загрузки
                var urlResponse = ExecuteJsonCommand<DictResponse>("photos.getWallUploadServer", qs);

                //загружаем фото
                WebClient myWebClient = new WebClient();
                byte[] responseArray = myWebClient.UploadFile(urlResponse.response["upload_url"].ToString(), filename);
                var json = JObject.Parse(System.Text.Encoding.ASCII.GetString(responseArray));

                //сохраняем фото
                qs = new NameValueCollection
                {
                    ["server"] = json["server"].ToString(),
                    ["photo"] = json["photo"].ToString(),
                    ["hash"] = json["hash"].ToString()
                };

                var saveResponse = ExecuteJsonCommand<ListDictResponse>("photos.saveWallPhoto", qs);

                return $"photo{saveResponse.response[0]["owner_id"]}_{saveResponse.response[0]["id"]}";
            }
            catch (APIErrorException) { }
            catch (Exception exp)
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), exp.Message);
                SaveException(exp);
            }

            return null;
        }

        private static void SaveException(Exception exp)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VkException");
            Directory.CreateDirectory(path);
            File.WriteAllText(string.Format("{0}/{1}.txt", path, Guid.NewGuid()), exp.Message);
        }

        public class APIErrorException : Exception
        {
            public APIErrorException(string message)
                : base(message)
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VkException");
                Directory.CreateDirectory(path);
                File.WriteAllText(string.Format("{0}/{1}.txt", path, Guid.NewGuid()), message);
            }
        }

        public T ExecuteJsonCommand<T>(string name, NameValueCollection qs) where T : Response
        {
            T result;

            var url = String.Format("https://api.vk.com/method/{0}?access_token={1}&{2}&v=5.73",
                name, App.Settings.Value.access_token, String.Join("&", (from item in qs.AllKeys select item + "=" + qs[item]).ToArray()));

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
                    if (result.error != null)
                        throw new APIErrorException(String.Format("Server error (code {0}: {1}).",
                        result.error.error_code,
                        result.error.error_msg));
                }
            }
            request.Abort();

            return result;
        }

        public class ListDictResponse : Response
        {
            public List<Dictionary<string, string>> response;
        }

        public class DictResponse : Response
        {
            public Dictionary<string, string> response;
        }

        public class StringResponse : Response
        {
            public string response;
        }

        public class Response
        {
            public Error error;
        }

        public class Error
        {
            public string error_code, error_msg;
        }
    }
}
