using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfMultiChat
{
    /// <summary>
    /// Interaction logic for EnterWindow.xaml
    /// </summary>
    public partial class EnterWindow : Window
    {
        private bool onlogin = true;
        private bool serverIPfocused = false;
        private bool usernamefocused = false;
        private bool passwordfocused = false;
        public EnterWindow()
        {
            InitializeComponent();
            serverIP.GotFocus += (s, e) => { if (!serverIPfocused) { serverIP.Text = "127.0.0.1"; serverIPfocused = true; } };
            username.GotFocus += (s, e) => { if (!usernamefocused) { username.Text = "liel"; usernamefocused = true; } };
            password.GotFocus += (s, e) => { if (!passwordfocused) { password.Text = "password"; passwordfocused = true; } };
        }
        private void loginSucc(bool isEncrypted)
        {
            MainWindow.username = username.Text;
            MainWindow.setIsEncrypted(isEncrypted);
            Console.WriteLine(isEncrypted? "YESSS" : "NOO");
            this.Close();
        }
        private void log(object sender, RoutedEventArgs e)
        {
            if (username.Text == "" || password.Text == "")
            {
                return;
            }
            if (MainWindow.tryToConnect(serverIP.Text, errorBox))
            {
                onlogin = true;
                login.Background = Brushes.Turquoise;
                signup.Background = Brushes.LightGray;
                bool isEncrypted = yes.IsChecked == true;
                string sringEncrypted = isEncrypted ? "yesEncrypted" : "noEncrypted";
                MainWindow.conductAndSend(new string[] { "login", username.Text, password.Text, sringEncrypted });
                string valid = MainWindow.loginSuccssesfuly();
                if (valid == "Nice!")
                {
                    errorBox.Text = valid;
                    loginSucc(isEncrypted);
                } else
                {
                    errorBox.Text = valid;
                }
            }
        }
        private void sign(object sender, RoutedEventArgs e)
        {
            if (username.Text == "" || password.Text == "")
            {
                return;
            }
            if (MainWindow.tryToConnect(serverIP.Text, errorBox))
            {
                onlogin = false;
                login.Background = Brushes.LightGray;
                signup.Background = Brushes.Turquoise;
                bool isEncrypted = yes.IsChecked == true;
                string sringEncrypted = isEncrypted ? "yesEncrypted" : "noEncrypted";
                MainWindow.conductAndSend(new string[] { "signup", username.Text, password.Text, sringEncrypted });
                string valid = MainWindow.loginSuccssesfuly();
                if (valid == "Nice!")
                {
                    errorBox.Text = valid;
                    loginSucc(isEncrypted);
                }
                else
                {
                    errorBox.Text = valid;
                }
            }
        }
        private void PressedKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (onlogin)
                {
                    log(null, null);
                }
                else
                {
                    sign(null, null);
                }
            }
        }
    }
}


