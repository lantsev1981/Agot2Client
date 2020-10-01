using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для ProfilePanel.xaml
    /// </summary>
    public partial class LeftPanelView2 : UserControl
    {
        public LeftPanelView2()
        {
            InitializeComponent();

            SelecteMenu(5);

            chatView.ViewModel.GetChatEvent += (count) =>
            {
#if !DEBUG
                if (tabControl.SelectedIndex == 4)
                    return; 
#endif

                int.TryParse(msgCount.Text, out int curCount);
                msgCount.Text = (curCount + count).ToString();

                var story = ((Storyboard)chatViewTabItem.Resources["animation"]);
                story.Begin();

                App.Agot2.chatMessage.Position = TimeSpan.Zero;
                App.Agot2.chatMessage.Play();
            };
            
            Loaded += LeftPanelView2_Loaded;
        }

        private void LeftPanelView2_Loaded(object sender, RoutedEventArgs e)
        {
            SelecteMenu(2);
        }

        void SelecteMenu(int selectedIndex)
        {
            tabControl.SelectedIndex = selectedIndex;
            showAnimation.Begin();
        }

        private void TabItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            msgCount.Text = string.Empty;
        }
    }
}
