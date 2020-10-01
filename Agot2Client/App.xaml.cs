using GameService;
using Lantsev;
using MyLibrary;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly string Title = "AGOT: Online BG (v3.23)";
        //Настройка анимации кадры в секунду
        public static readonly Config<ConfigSettings> Config = new Config<ConfigSettings>("GameService");

        public static IService Service { get; private set; }
        public static TaskFactory TaskFactory { get; private set; }

#if !DEBUG
        public static CryptoFileJson<AppSettings> Settings { get; private set; }
#endif
#if DEBUG
        public static PublicFileJson<AppSettings> Settings { get; private set; }
#endif

        public static string ClientVersion { get; private set; }
        public static string ClientId { get; private set; }

        public static MainWindow Agot2 { get; set; }

        public static string GetResources(string key)
        {
            key = key.Replace(' ', '_').Replace('-', '_');
            string result = Agot2Client.Properties.Resources.ResourceManager.GetString(key);
            if (string.IsNullOrEmpty(result))
            {
                return key;
            }

            return result.Replace('|', '\n');
        }

        public static string TextDecoder(string text)
        {
            try
            {
                if (!text.StartsWith("dynamic_"))
                {
                    return text;
                }

                string[] split = text.Split('*').Select(p => App.GetResources(p)).ToArray();
                for (int i = split.Count() - 2; i >= 0; i--)
                {
                    split[i] = string.Format(split[i], split.Skip(i + 1).ToArray());
                }

                return split[0].Replace('_', ' ');
            }
            catch (Exception exp)
            {
                return string.Format("{0} {1} {2}", App.GetResources("error_decodeText"), exp.Message, text);
            }
        }

        public App()
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                TaskFactory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(1));
                ChannelFactory<IService> _CF = new ChannelFactory<IService>("AGOT2");
                Service = _CF.CreateChannel();

                ClientVersion = Crypto.Md5Hex(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Agot2Client.exe"));
                string mac = NetworkInterface.GetAllNetworkInterfaces().First(p => p.OperationalStatus == OperationalStatus.Up).GetPhysicalAddress().ToString();
                ClientId = string.Format("{0}\\{1}\\{2} {3} {4} {5}", Environment.MachineName, Environment.UserDomainName, Environment.UserName, Environment.OSVersion.VersionString, Environment.Version, mac);

#if !DEBUG
                Settings = new CryptoFileJson<AppSettings>("AppSettings", "W@NtUz81");
#endif
#if DEBUG
                Settings = new PublicFileJson<AppSettings>("AppSettings");
#endif
                if (Settings.Read() == null)
                    Settings.Value = new AppSettings();

                if (Settings.Value.Vols == null || Settings.Value.Vols.Count < 5 || string.IsNullOrEmpty(Settings.Value.Lang))
                    Settings.Value.SetDefault();

                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(Settings.Value.Lang);
            }
            catch (Exception exp)
            {
                //Удаление файла настроек
                if (File.Exists("AppSettings"))
                    File.Delete("AppSettings");

                WriteException(exp);
            }
        }

        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en");
            Exception exp = e.ExceptionObject as Exception ?? new Exception(e.ExceptionObject.ToString());
            App.SendException(exp);
        }

        public static void SendException(Exception exp)
        {
            try
            {
                //пробуем отправить ошибку на сервер на английском языке
                WCFGameException wcfGameException = new WCFGameException
                {
                    Game = Agot2Client.MainWindow.ClientInfo.ClientGameId.ToString(),
                    Login = Agot2Client.MainWindow.GamePortal?.User.Login,
                    Message = CombineMsg(exp, ""),
                    stackTrace = exp.StackTrace
                };
                App.Service.SendException(wcfGameException);
            }
            catch
            {
            }

            App.WriteException(exp);
        }

        public static string CombineMsg(Exception exp, string msg)
        {
            msg += $"\n{exp.Message}";
            return exp.InnerException != null ? CombineMsg(exp.InnerException, msg) : msg;
        }

        public static void WriteException(Exception exp)
        {
            if (MessageBox.Show(string.Format("{1}\n{0}\n\n{2}", $"{exp.Message}\n\n{exp.InnerException?.Message}",
                App.GetResources("text_closeApp"),
                App.GetResources("text_saveError")),
                App.GetResources("error_criticalError"),
                MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exception");
                Directory.CreateDirectory(path);
                File.WriteAllText(string.Format("{0}/{1}.txt", path, Guid.NewGuid()), exp.ToString());
            }

            App.Settings.Write();
            Environment.Exit(666);
        }

        public static string GetJsonString(string url)
        {
            string result = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new APIErrorException(string.Format("Server error (HTTP {0}: {1}).",
                        response.StatusCode,
                        response.StatusDescription));
                }

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                }
            }

            request.Abort();

            return result;
        }

        public class APIErrorException : Exception
        {
            public APIErrorException(string message)
                : base(message)
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "exception");
                Directory.CreateDirectory(path);
                File.WriteAllText(string.Format("{0}/{1}.txt", path, Guid.NewGuid()), message);
            }
        }

        //private void ToolTipOpenedHandler(object sender, RoutedEventArgs e)
        //{
        //    ToolTip toolTip = (ToolTip)sender;
        //    UIElement target = toolTip.PlacementTarget;
        //    Point adjust = target.TranslatePoint(new Point(8, 0), toolTip);
        //    if (adjust.Y > 0)
        //        toolTip.Placement = PlacementMode.Top;

        //    toolTip.Tag = new Thickness(adjust.X, -1.5, 0, -1.5);
        //}
    }

    public class GridLengthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double res = 0;

            switch ((string)parameter)
            {
                case "Sum":
                    res = (double)values[0] + (double)values[1];
                    break;
            }

            return new GridLength(res);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    }

    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri((string)value);
                bmp.EndInit();

                return bmp;
            }
            catch (Exception exp)
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), exp.Message);
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
