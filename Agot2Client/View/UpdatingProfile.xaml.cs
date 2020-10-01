using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
//using System.Windows.Media.Animation;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для UpdatingProfile.xaml
    /// </summary>
    public partial class UpdatingProfile : UserControl
    {
        static public bool PlayIntro { get; private set; }

        public UpdatingProfile()
        {
            InitializeComponent();
            intro.Source = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media/intro.wmv"), UriKind.Absolute);
            //intro.Source = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "D:/Tmp/clips/Обучающее видео/Обучающее видео.mp4"), UriKind.Absolute);
            //intro.Source = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "https://youtu.be/2rMjHmskTpA"), UriKind.Absolute);
            intro.MediaEnded += Intro_MediaEnded;
            intro.MediaFailed += Intro_MediaFailed;
            intro.MediaFailed += Intro_MediaEnded;
            intro.MediaOpened += Intro_MediaOpened;
        }

        private void Intro_MediaOpened(object sender, RoutedEventArgs e)
        {
            PlayIntro = true;
        }

        private void Intro_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exception");
            Directory.CreateDirectory(path);
            File.WriteAllText(string.Format("{0}/{1}.txt", path, Guid.NewGuid()), e.ErrorException.Message);
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            App.Agot2.leftPanelView.leaderBoardView.FinishUpdate();
        }

        private void Intro_MediaEnded(object sender, RoutedEventArgs e)
        {
            PlayIntro = false;
            if (App.Settings.Value.Vols[0].Value > 0)
                App.Agot2.mainTitle.Play();
            if (!LeaderBoardView.LoadingData)
                Hide();
        }

        public void Hide()
        {
            var sb = (this.Resources["Hide"]) as Storyboard;
            if (sb == null)
                return;

            sb.Begin(this);
        }
    }
}
