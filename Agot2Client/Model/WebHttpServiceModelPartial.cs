using Agot2Client;
using MyLibrary;
using System;
using System.Collections.Specialized;

namespace Agot2Server
{
    public class ExtWebSockService
    {
        static public string GetPort(string serviceName, Guid game)
        {
            try
            {
                var param = new NameValueCollection
                {
                    { "name", serviceName },
                    { "game", game.ToString() }
                };
                var item = ExtHttp.ExecuteCommand<string>($"http://{App.Config.Settings.ServerAddress}:{App.Config.Settings.ServerPort}/WebHttpService/GetWebSocket", param);
                return item;
            }
            catch (Exception exp)
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), exp.Message);
                return null;
            }
        }
    }
}
