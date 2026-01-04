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

namespace WpfMultiChat
{
    /// <summary>
    /// Interaction logic for Page2.xaml
    /// </summary>
    public partial class ChessLobby : Page
    {
        public ChessLobby()
        {
            InitializeComponent();
        }
        private void KeyEntered(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.conductAndSend(new string[] { "sendChessInvite", opponentName.Text });
                opponentName.Clear();
            }
        }

        private void sendInvite(object sender, RoutedEventArgs e)
        {
            MainWindow.conductAndSend(new string[] { "sendChessInvite", opponentName.Text });
            opponentName.Clear();
        }
    }
}
