using System.Windows;
using Client;
using CommonLibrary;

namespace DesktopClient
{
    public partial class LoginWindow : Window
    {
        private readonly Service service;

        public LoginWindow()
        {
            InitializeComponent();
            service = new Service("127.0.0.1", 5000);
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = loginTextBox.Text;
            string password = passwordBox.Password;

            bool isAuthenticated = AuthenticateUser(login, password);

            if (isAuthenticated)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.SetService(service);
                mainWindow.SetLogin(login);
                mainWindow.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Invalid login or password.");
            }
        }

        private bool AuthenticateUser(string login, string password)
        {
            return service.Authenticate(login, password);
        }
    }
}
