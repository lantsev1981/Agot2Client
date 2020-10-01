using Agot2Server;
using ChatServer;
using MyLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WebSocket4Net;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для ChatView.xaml
    /// </summary>
    public partial class ChatView : UserControl, IDisposable
    {
        #region DependencyProperty
        public Guid? RoomId
        {
            get => (Guid?)GetValue(RoomIdProperty);
            set
            {
                SetValue(RoomIdProperty, value);
                ViewModel.RoomId = value;
            }
        }
        public static readonly DependencyProperty RoomIdProperty =
            DependencyProperty.Register("RoomId", typeof(Guid?), typeof(ChatView), new FrameworkPropertyMetadata(null, new PropertyChangedCallback((d, e) =>
            {
                ((ChatView)d).RoomId = (Guid?)e.NewValue;
            }))
            { BindsTwoWayByDefault = true });

        public string Login
        {
            get => (string)GetValue(LoginProperty);
            set
            {
                SetValue(LoginProperty, value);
                ViewModel.Creator = value;
            }
        }
        public static readonly DependencyProperty LoginProperty =
            DependencyProperty.Register("Login", typeof(string), typeof(ChatView), new PropertyMetadata(null, new PropertyChangedCallback((d, e) =>
            {
                if (e.NewValue == null)
                    throw new Exception("Отсутствует обязательный параметр Login");
                ((ChatView)d).Login = e.NewValue as string;
            })));
        #endregion

        public ChatViewModel ViewModel { get; private set; }


        public ChatView()
        {
            InitializeComponent();
            ViewModel = new ChatViewModel();
            itemsControl.ItemsSource = ViewModel.Items;
        }

        public void Dispose()
        {
            ViewModel.Dispose();
        }

        private void AddChat_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SendChat(messageTextBox.Text))
                messageTextBox.Text = string.Empty;
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                AddChat_Button_Click(null, null);

            e.Handled = true;
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            messageTextBox.Focus();
        }
    }

    public class ChatViewModel : IDisposable
    {
        private WebSocket websocketClient;

        public event Action<int> GetChatEvent;
        private void OnGetChatEvent(int count)
        {
            GetChatEvent?.Invoke(count);
        }

        public ObservableCollection<ChatItemViewModel> Items { get; private set; }

        private Guid? _RoomId;
        public Guid? RoomId
        {
            get => _RoomId;
            set
            {
                _RoomId = value;
                if (_RoomId == null)
                    Dispose();
                else
                {
                    Game = MainWindow.ClientInfo.ClientGame;
                    Connect();
                }
            }
        }

        public ExtGame Game { get; private set; }

        public string Creator { get; set; }

        public ChatViewModel()
        {
            Items = new ObservableCollection<ChatItemViewModel>();
        }

        private void Connect()
        {
            try
            {
                string port = ExtWebSockService.GetPort("ChatService", RoomId.Value);
                if (string.IsNullOrEmpty(port))
                    return;

                Dispose();

                websocketClient = new WebSocket($"ws://{App.Config.Settings.ServerAddress}:{port}", "basic", WebSocketVersion.Rfc6455);
                websocketClient.Opened += WebsocketClient_ChangeState;
                websocketClient.Closed += WebsocketClient_ChangeState;
                websocketClient.Error += WebsocketClient_Error;
                websocketClient.MessageReceived += WebsocketClient_MessageReceived;
                websocketClient.Open();
            }
            catch (Exception exp)
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), exp.Message);
            }
        }

        public void Dispose()
        {
            if (websocketClient != null)
            {
                websocketClient.Opened -= WebsocketClient_ChangeState;
                websocketClient.Closed -= WebsocketClient_ChangeState;
                websocketClient.Error -= WebsocketClient_Error;
                websocketClient.MessageReceived -= WebsocketClient_MessageReceived;
                websocketClient.Close();
                websocketClient.Dispose();
                websocketClient = null;
                Application.Current.Dispatcher.BeginInvoke(new Action(() => Items.Clear()), DispatcherPriority.ApplicationIdle);
            }
        }

        private void WebsocketClient_ChangeState(object sender, EventArgs e)
        {
            Chat chat = new Chat() { Creator = "Вестерос", Message = $"ChatService: {websocketClient.State}", Time = DateTimeOffset.UtcNow };
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                AddItem(chat);
                OnGetChatEvent(1);
            }), DispatcherPriority.ApplicationIdle);
        }

        private void WebsocketClient_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), e.Exception.Message);
            WebsocketClient_ChangeState(null, null);
        }

        private void WebsocketClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            List<Chat> value = new List<Chat>();
            ChatServiceMethod data = JsonConvert.DeserializeObject<ChatServiceMethod>(e.Message);
            switch (data.Method)
            {
                case "item":
                    value.Add(JsonConvert.DeserializeObject<Chat>(data.Value));
                    break;
                case "items":
                    value.AddRange(JsonConvert.DeserializeObject<List<Chat>>(data.Value));
                    break;
            }

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (Chat item in value.OrderBy(p => p.Time))
                    AddItem(item);

                int count = value.Count(p => p.IsVisible);
                if (count > 0) OnGetChatEvent(count);
            }), DispatcherPriority.ApplicationIdle);
        }

        public bool SendChat(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return false;

            if (websocketClient != null && websocketClient.State != WebSocketState.Open)
            {
                Connect();
                return false;
            }
            else
            {
                Chat chat = new Chat()
                {
                    Creator = Creator,
                    Message = message
                };
                chat.SHA1Hex = Crypto.SHA1Hex($"{chat.Creator}{chat.Message}{Guid.Empty}");

                websocketClient.Send(JsonConvert.SerializeObject(chat));
                return true;
            }
        }

        private void AddItem(Chat item)
        {
            try
            {
                if (Items.Count == 100)
                    Items.RemoveAt(99);
                if (item.Creator == "Вестерос")
                    item.Message = App.TextDecoder(item.Message);
                Items.Insert(0, new ChatItemViewModel(item, Game));
            }
            catch (Exception exp)
            {
                App.Agot2.errorView.ShowByDispatcher(App.GetResources("text_error"), exp.Message);
            }
        }
    }
}
