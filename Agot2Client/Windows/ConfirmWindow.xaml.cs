using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Agot2Client
{
    /// <summary>
    /// Логика взаимодействия для ConfirmWindow.xaml
    /// </summary>
    public partial class ConfirmWindow : Window
    {
        private List<Quotes> QuotesList = JsonConvert.DeserializeObject<List<Quotes>>(File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, App.GetResources("text_quotes")), Encoding.UTF8));
        public ConfirmWindow()
        {
            InitializeComponent();
            Quotes quotes = QuotesList[new Random().Next(QuotesList.Count)];
            quotesTB.Text = string.Format("\"{0}\"", quotes.quotes);
            fioTB.Text = string.Format("— {0} {1} {2}", quotes.title, quotes.first_name, quotes.last_name);
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.Settings.Value.User = null; App.Settings.Value.access_token = null;
            App.Settings.Write();

            this.DialogResult = false;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                okButton_Click(sender, e);
            }

            if (e.Key == Key.Escape)
            {
                cancelButton_Click(sender, e);
            }
        }
    }
}
