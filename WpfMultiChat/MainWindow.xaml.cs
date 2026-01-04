using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
using System.Windows.Threading;
using System.IO;
using System.Threading;

namespace WpfMultiChat
{
    public partial class MainWindow : Window
    {
        private List<Button> newButtonList = new List<Button>();
        public static string username = "";
        private static EncryptedSocket socket;
        private DispatcherTimer timer;

        public static void setIsEncrypted(bool isEncrypted)
        {
            socket.setIsEncrypted(isEncrypted);
        }

        public MainWindow()
        {
            
            Directory.CreateDirectory("files");
            Window enterWindow = new EnterWindow();
            enterWindow.ShowDialog();

            InitializeComponent();
            if (username == "")
            {
                this.Close();
            }
            signedAs.Text = "signed as " + username;
            timer = new DispatcherTimer();

            timer.Interval = new TimeSpan(0, 0, 0, 0, 40);
            timer.Tick += recvMsg;
            timer.Start();
        }

        private static bool connected = false;
        public static bool tryToConnect(string ip, TextBlock errors)
        {
            if (connected)
            {
                return true;
            }
            try
            {
                Socket s = new Socket(SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ip), 9000);
                Thread timeout = new Thread(() => {
                    while (!s.Connected)
                    {
                        try
                        {
                            s.Connect(endpoint);
                        }
                        catch (SocketException) { }
                    }
                    connected = true;
                });
                timeout.Start();
                // 10 times try to connect
                for (int i = 0; i < 10; i++)
                {
                    if (connected)
                    {
                        socket = new EncryptedSocket(s);
                        return true;
                    }
                    Thread.Sleep(100);
                }
                timeout.Abort();
                errors.Text = "Wrong IP";
                return false;
            }
            catch (FormatException)
            {
                errors.Text = "Invalid IP";
                return false;
            }
        }
        public static string loginSuccssesfuly()
        {
            string s = socket.recv();
            if (s == ":)")
            {
                return "Nice!";
            }
            else
            {
                return socket.ParseMsg()[0];
            }
        }
        public static void conductAndSend(string[] s)
        {
            socket.send(conductMsg(s));
        }
        private void recvMsg(object sender, EventArgs e)
        {
            if (socket.ContainSmth())
            {
                string[] command = socket.ParseMsg();
                handleCmd(command);
            }
        }
        private void downloadFile(string chatName, string fileName)
        {
            Console.WriteLine($"trying to download {fileName} from chat {chatName}");
            socket.recvFile(fileName);
        }
        private void allChat(string[] command)
        {
            int i = 1;
            while (i < command.Length)
            {
                if (command[i++] == "spreadFile")
                {
                    spreadFile(command[i++], command[i++], command[i++]);
                } else
                {
                    spreadMsg(command[i++], command[i++], command[i++]);
                }
            }
        }
        private void spreadFile(string sender, string fileName, string chatName)
        {
            try
            {
                if (((chatPage)frame.Content).name == chatName)
                {
                    chatPage cp = (chatPage)frame.Content;
                    cp.addFile(sender, fileName);
                    cp.scrollDown();
                }
            }
            catch (InvalidCastException) { }
            catch (NullReferenceException) { }
        }
        private void spreadMsg(string sender, string msg, string chatName)
        {
            try
            {
                if (((chatPage)frame.Content).name == chatName)
                {
                    chatPage cp = (chatPage)frame.Content;
                    cp.addMessaege(sender, msg);
                    cp.scrollDown();
                }
            }
            catch (InvalidCastException) { }
            catch (NullReferenceException) { }
        }
        private void createChatSuccessfully(string chatName)
        {
            string admin = MainWindow.username;
            Button newButton = new Button();
            newButton.Background = Brushes.White;
            newButton.Foreground = Brushes.DarkBlue;
            newButton.BorderThickness = new Thickness(0);
            newButton.HorizontalAlignment = HorizontalAlignment.Stretch;
            newButton.Content = chatName;
            newButton.Width = 143;
            newButton.Margin = new Thickness(0, 10, 0, 0);
            newButton.Click += (s, a) => { frame.Content = new chatPage(chatName, admin); };
            newButtonList.Add(newButton);
            chatButtonList.Children.Add(newButton);
        }
        private void addedToChat(string admin, string chatName)
        {
            Button newButton = new Button();

            newButton.Background = Brushes.White;
            newButton.Foreground = Brushes.DarkBlue;
            newButton.BorderThickness = new Thickness(0);
            newButton.HorizontalAlignment = HorizontalAlignment.Stretch;
            newButton.Content = chatName;
            newButton.Width = 143;
            newButton.Margin = new Thickness(0, 10, 0, 0);
            newButton.Click += (s, a) => { frame.Content = new chatPage(chatName, admin); };
            newButtonList.Add(newButton);
            chatButtonList.Children.Add(newButton);
        }
        private void leaveChatSuccessfully(string chatName)
        {
            Button buttonToRemove = null;
            foreach (Button b in newButtonList)
            {
                if ((string)b.Content == chatName)
                {
                    buttonToRemove = b;
                }
            }
            if (buttonToRemove != null)
            {
                chatButtonList.Children.Remove(buttonToRemove);
                newButtonList.Remove(buttonToRemove);
                frame.Content = null;
            }
        }
        private void chessInvite(string whoInvited)
        {
            Button newButton = new Button();
            newButton.Background = Brushes.White;
            newButton.Foreground = Brushes.DarkBlue;
            newButton.BorderThickness = new Thickness(0);
            newButton.HorizontalAlignment = HorizontalAlignment.Stretch;
            newButton.Content = whoInvited;
            newButton.Width = 143;
            newButton.Margin = new Thickness(0, 10, 0, 0);
            newButton.Click += (s, a) => { conductAndSend(new string[] { "acceptChessInvite", (string)((Button)s).Content }); chessButtonList.Children.Remove((Button)s); };
            chessButtonList.Children.Add(newButton);
        }
        private void startChess(string myColor, string gameId, string opponentName)
        {
            Button newButton = new Button();
            newButton.Background = Brushes.White;
            newButton.Foreground = Brushes.Black;
            newButton.BorderThickness = new Thickness(0);
            newButton.HorizontalAlignment = HorizontalAlignment.Stretch;
            newButton.Name = "game" + gameId;
            newButton.Content = opponentName;
            newButton.Width = 143;
            newButton.Margin = new Thickness(0, 10, 0, 0);
            newButton.Click += (s, a) => { frame.Content = getGamePage(myColor, gameId); };
            chessButtonList.Children.Add(newButton);
        }
        private void moveChessPiece(int di, int dj, int si, int sj, string gameId)
        {
            Chess game = runningGames[gameId];
            game.actualMove(di, dj, si, sj);
        }
        private void chessGaveOver(string msg, string gameId)
        {
            Chess game = runningGames[gameId];
            game.end(msg);
            runningGames.Remove(gameId);
            foreach (Button b in chessButtonList.Children)
            {
                if (b.Name == gameId)
                {
                    chessButtonList.Children.Remove(b);
                    break;
                }
            }
        }

        private void handleCmd(string[] command)
        {
            string cmd = command[0];
            switch (cmd)
            {
                case "downloadFile":
                    downloadFile(command[1], command[2]);
                    break;
                case "AllChat":
                    allChat(command);
                    break;
                case "spreadFile":
                    spreadFile(command[1], command[2], command[3]);
                    break;
                case "spreadMsg":
                    spreadMsg(command[1], command[2], command[3]);
                    break;
                case "createChatSuccessfully":
                    createChatSuccessfully(command[1]);
                    break;
                case "addedToChat":
                    addedToChat(command[1], command[2]);
                    break;
                case "leaveChatSuccessfully":
                    leaveChatSuccessfully(command[1]);
                    break;
                case "chessInvite":
                    chessInvite(command[1]);
                    break;
                case "startChess":
                    startChess(command[1], command[2], command[3]);
                    break;
                case "moveChessPiece":
                    moveChessPiece(int.Parse(command[1]), int.Parse(command[2]), int.Parse(command[3]), int.Parse(command[4]), command[5]);
                    break;
                case "chessGaveOver":
                    chessGaveOver(command[1], command[2]);
                    break;
                default:
                    Console.WriteLine("Received Invalid command.");
                    break;
            }
        }
        public static void sendFile2(string fileName)
        {
            socket.sendFile(fileName);
        }
        Dictionary<string, Chess> runningGames = new Dictionary<string, Chess>();
        private Chess getGamePage(string color, string id)
        {
            try
            {
                return runningGames[id];
            }
            catch (KeyNotFoundException)
            {
                Chess game = new Chess(color, id);
                runningGames.Add(id, game);
                return game;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            frame.Content = new chatPage("global", "everyone! :)");
        }

        private void AddChat(object sender, RoutedEventArgs e)
        {
            frame.Content = new Page2();
        }

        private void TextBlock_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                frame.Content = null;
            }
        }
        public static string conductMsg(string[] strings)
        {
            string ret = "";
            foreach (string lable in strings)
            {
                ret += lable.Length.ToString().PadLeft(5, '0') + lable;
            }
            return ret;
        }

        private void StartGame(object sender, RoutedEventArgs e)
        {
            frame.Content = new ChessLobby();
        }
    }
}