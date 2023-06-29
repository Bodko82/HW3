using System;
using System.Collections.Generic;
using System.Windows;
using CommonLibrary;
using Client;
using System.Windows.Input;

namespace DesktopClient
{
    public partial class MainWindow : Window
    {
        private Service service;
        private string login;

        public MainWindow()
        {
            InitializeComponent();
            service = new Service("127.0.0.1", 5000);
        }

        private void GetMessageCount_Click(object sender, RoutedEventArgs e)
        {
            string login = loginTextBox.Text;
            int messageCount = service.GetMessageCount(login);
            serverTextBlock.Text = $"Message count: {messageCount}";
        }

        private void GetMessages_Click(object sender, RoutedEventArgs e)
        {
            string login = loginTextBox.Text;
            List<Message> messages = service.GetMessages(login);
            messageListBox.ItemsSource = messages;
        }
        public void SetService(Service service)
        {
            this.service = service;
        }

        public void SetLogin(string login)
        {
            this.login = login;
        }
        private void textTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string login = loginTextBox.Text;
                string toLogin = toTextBox.Text;
                string text = textTextBox.Text;

                var message = new CommonLibrary.Message
                {
                    From = new CommonLibrary.Client { Login = login },
                    To = new CommonLibrary.Client { Login = toLogin },
                    Text = text,
                    CreatedAt = DateTime.Now
                };

                bool success = service.SendMessage(message);

                if (success)
                {
                    serverTextBlock.Text = "Message sent successfully.";
                }
                else
                {
                    serverTextBlock.Text = "Failed to send message.";
                }
            }
        }


        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            messageListBox.ItemsSource = null;
        }
    }
}
