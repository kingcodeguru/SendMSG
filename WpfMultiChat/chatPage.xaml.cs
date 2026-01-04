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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;
using Microsoft.Win32;
using System.IO;

namespace WpfMultiChat
{
    /// <summary>
    /// Interaction logic for chatPage.xaml
    /// </summary>
    public partial class chatPage : Page
    {
        public string name;
        string admin;
        TextBox wn;
        bool t1focused = false;
        bool wnfocused = false;
        public chatPage(string chatName, string admin)
        {
            InitializeComponent();
            t1.GotFocus += (s, e) => { if (!t1focused) { t1.Clear(); t1focused = true; } };
            name = chatName;
            this.admin = admin;
            title.Text = chatName;
            // if I'm the admin - add a button that allow to add more users.
            if (admin == MainWindow.username)
            {
                TextBox writeName = new TextBox();
                writeName.TextAlignment = TextAlignment.Center;
                writeName.Width = 300;
                writeName.Background = Brushes.Beige;
                writeName.VerticalAlignment = VerticalAlignment.Top;
                writeName.HorizontalAlignment = HorizontalAlignment.Center;
                writeName.Text = "user to add";
                writeName.FontSize = 16;
                wn = writeName;
                wn.GotFocus += (s, e) => { if (!wnfocused) { wn.Clear(); wnfocused = true; } };

                Button b = new Button();
                b.Width = 300;
                b.Background = Brushes.Beige;
                b.VerticalAlignment = VerticalAlignment.Top;
                b.HorizontalAlignment = HorizontalAlignment.Center;
                b.Content = "Add User";
                b.Click += (s, e) => { MainWindow.conductAndSend(new string[] { "addClientToChat", wn.Text, name }); };

                chat.Children.Add(writeName);
                chat.Children.Add(b);
            }

            MainWindow.conductAndSend(new string[] { "sendMeAll", chatName });
            scrollDown();
        }
        public void scrollDown()
        {
            scroller.ScrollToVerticalOffset(scroller.ExtentHeight);
        }
        public void addFile(string name, string fileName)
        {
            Button b = new Button();
            b.Margin = new Thickness(0, 0, 0, 10);
            b.Background = Brushes.Transparent;
            TextBlock tb = new TextBlock();
            b.Content = tb;
            b.Content = tb;
            TextBlock deliver = new TextBlock();
            if (name == MainWindow.username)
            {
                deliver.HorizontalAlignment = HorizontalAlignment.Left;
                deliver.Background = Brushes.Transparent;
                tb.HorizontalAlignment = HorizontalAlignment.Left;
                tb.Background = Brushes.LightGreen;
            }
            else
            {
                deliver.HorizontalAlignment = HorizontalAlignment.Right;
                deliver.Background = Brushes.Transparent;
                tb.HorizontalAlignment = HorizontalAlignment.Right;
                tb.Background = Brushes.White;
            }
            deliver.TextAlignment = TextAlignment.Left;
            deliver.TextWrapping = TextWrapping.Wrap;
            deliver.Width = 320;
            deliver.Text = name + ":";
            tb.TextAlignment = TextAlignment.Left;
            tb.TextWrapping = TextWrapping.Wrap;
            tb.Text = fileName;
            chat.Children.Add(deliver);
            chat.Children.Add(b);
            b.Click += (s, e) => {
                MainWindow.conductAndSend(new string[] { "downloadFile", ((TextBlock)((Button)s).Content).Text, name });
            };

        }
        public void addMessaege(string name, string text)
        {
            TextBlock tb = new TextBlock();
            TextBlock deliver = new TextBlock();
            if (name == MainWindow.username)
            {
                deliver.HorizontalAlignment = HorizontalAlignment.Left;
                deliver.Background = Brushes.Transparent;
                tb.HorizontalAlignment = HorizontalAlignment.Left;
                tb.Background = Brushes.LightGreen;
            }
            else
            {
                deliver.HorizontalAlignment = HorizontalAlignment.Right;
                deliver.Background = Brushes.Transparent;
                tb.HorizontalAlignment = HorizontalAlignment.Right;
                tb.Background = Brushes.White;
            }
            deliver.TextAlignment = TextAlignment.Left;
            deliver.TextWrapping = TextWrapping.Wrap;
            deliver.Width = 320;
            deliver.Text = name + ":";
            tb.TextAlignment = TextAlignment.Left;
            tb.TextWrapping = TextWrapping.Wrap;
            tb.Width = 320;
            tb.Margin = new Thickness(0, 0, 0, 10);
            tb.Text = text;
            chat.Children.Add(deliver);
            chat.Children.Add(tb);
        }
        private void clearChat()
        {
            chat.Children.Clear();
        }
        private void exitChat(object sender, RoutedEventArgs e)
        {
            MainWindow.conductAndSend(new string[] { "leaveChat", name });
        }

        private void B1_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.conductAndSend(new string[] { "sendMsg", name, t1.Text });
            t1.Clear();
            t1.Focus();
        }
        private void Page_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                B1_Click(sender, null);
            }
        }
        private static int min(int x, int y)
        {
            if (x < y)
            {
                return x;
            }
            return y;
        }
        private void B2_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Title = "please pick a file";
            fileDialog.Multiselect = false;
            bool? result = fileDialog.ShowDialog();
            byte[] buffer = new byte[1024];
            MainWindow.conductAndSend(new string[] { "uploadFile", fileDialog.SafeFileName, name });
            MainWindow.sendFile2(fileDialog.FileName);
        }
    }
}