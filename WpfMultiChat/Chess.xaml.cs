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
    /// Interaction logic for Chess.xaml
    /// </summary>
    public partial class Chess : Page
    {
        private string id;
        private string color;
        private string other;
        bool isWhite = true;
        private Button lastButtonClicked = null;
        private List<Button> possibleMoves = new List<Button>();
        private Button[,] buttonsMap = new Button[8,8];
        private Image createPiece(string name, bool myPiece = false)
        {
            if (name != "Nothing")
            {
                if (myPiece)
                {
                    name = color + name;
                }
                else
                {
                    name = other + name;
                }
            }
            Image im = new Image();
            string filename = name;
            if (name == "Nothing")
            {
                filename= "BLA";
            }
            im.Source = new BitmapImage(new Uri(@"pack://application:,,,/piecesImages/" + filename + ".png", UriKind.RelativeOrAbsolute));
            im.VerticalAlignment = VerticalAlignment.Stretch;
            im.HorizontalAlignment = HorizontalAlignment.Stretch;
            im.Name = name;
            return im;
        }
        public Chess(string color, string id)
        {
            this.id = id;
            this.color = color;
            if (color == "B")
            {
                isWhite = false;
                other = "W";
            } else
            {
                other = "B";
            }
            InitializeComponent();
            foreach (UIElement ob in gameBoard.Children)
            {
                if (ob is Button)
                {
                    Button b = (Button)ob;
                    b.Content = createPiece("Nothing");
                    buttonsMap[Grid.GetRow(b), Grid.GetColumn(b)] = b;
                }
            }
            buttonsMap[7, 0].Content = createPiece("RO", true);
            buttonsMap[7, 1].Content = createPiece("KN", true);
            buttonsMap[7, 2].Content = createPiece("BI", true);
            if (color == "W")
            {
                buttonsMap[7, 3].Content = createPiece("QU", true);
                buttonsMap[7, 4].Content = createPiece("KI", true);
            } else
            {
                buttonsMap[7, 4].Content = createPiece("QU", true);
                buttonsMap[7, 3].Content = createPiece("KI", true);
            }
            buttonsMap[7, 5].Content = createPiece("BI", true);
            buttonsMap[7, 6].Content = createPiece("KN", true);
            buttonsMap[7, 7].Content = createPiece("RO", true);
            for (int i = 0; i < 8; i++)
            {
                buttonsMap[6, i].Content = createPiece("PA", true);
            }

            buttonsMap[0, 0].Content = createPiece("RO", false);
            buttonsMap[0, 1].Content = createPiece("KN", false);
            buttonsMap[0, 2].Content = createPiece("BI", false);
            if (color == "W")
            {
                buttonsMap[0, 3].Content = createPiece("QU", false);
                buttonsMap[0, 4].Content = createPiece("KI", false);
            }
            else
            {
                buttonsMap[0, 4].Content = createPiece("QU", false);
                buttonsMap[0, 3].Content = createPiece("KI", false);
            }
            buttonsMap[0, 5].Content = createPiece("BI", false);
            buttonsMap[0, 6].Content = createPiece("KN", false);
            buttonsMap[0, 7].Content = createPiece("RO", false);
            for (int i = 0; i < 8; i++)
            {
                buttonsMap[1, i].Content = createPiece("PA", false);
            }
            p1.Text = "0";
            p2.Text = "0";
        }
        private void movePeice(Button source, Button target)
        {
            target.Content = source.Content;
            source.Content = createPiece("Nothing");
        }
        private List<int[]> getPosibleMoves(string piece, int row, int column)
        {
            piece = piece.Substring(1);
            List<int[]> ret = new List<int[]>();
            if (piece == "PA")
            {
                if (row == 0)
                {
                    return new List<int[]>();
                }
                if (row == 6)
                {
                    if (((Image)buttonsMap[row - 1, column].Content).Name == "Nothing" && ((Image)buttonsMap[row - 2, column].Content).Name == "Nothing")
                    {
                        ret.Add(new int[] { row - 2, column });
                    }
                }
                if (((Image)buttonsMap[row - 1, column].Content).Name == "Nothing")
                {
                    ret.Add(new int[] { row - 1, column });
                }
                if (column > 0 && ((Image)buttonsMap[row - 1, column - 1].Content).Name[0] == other[0])
                {
                    ret.Add(new int[] { row - 1, column - 1 });
                }
                if (column < 7 && ((Image)buttonsMap[row - 1, column + 1].Content).Name[0] == other[0])
                {
                    ret.Add(new int[] { row - 1, column + 1 });
                }
                return ret;
            }
            if (piece == "RO")
            {
                for (int i = row + 1; i < 8; i++)
                {
                    if (i < 0 || i >= 8)
                    {
                        break;
                    }
                    if (((Image)buttonsMap[i, column].Content).Name == "Nothing")
                    {
                        ret.Add(new int[] { i, column });
                    }
                    else
                    {
                        if (((Image)buttonsMap[i, column].Content).Name[0] == other[0])
                        {
                            ret.Add(new int[] { i, column });
                        }
                        break;
                    }
                }
                for (int i = row - 1; i < 8; i--)
                {
                    if (i < 0 || i >= 8)
                    {
                        break;
                    }
                    if (((Image)buttonsMap[i, column].Content).Name == "Nothing")
                    {
                        ret.Add(new int[] { i, column });
                    }
                    else
                    {
                        if (((Image)buttonsMap[i, column].Content).Name[0] == other[0])
                        {
                            ret.Add(new int[] { i, column });
                        }
                        break;
                    }
                }
                for (int i = column + 1; i < 8; i++)
                {
                    if (i < 0 || i >= 8)
                    {
                        break;
                    }
                    if (((Image)buttonsMap[row, i].Content).Name == "Nothing")
                    {
                        ret.Add(new int[] { row, i });
                    }
                    else
                    {
                        if (((Image)buttonsMap[row, i].Content).Name[0] == other[0])
                        {
                            ret.Add(new int[] { row, i});
                        }
                        break;
                    }
                }
                for (int i = column - 1; i < 8; i--)
                {
                    if (i < 0 || i >= 8)
                    {
                        break;
                    }
                    if (((Image)buttonsMap[row, i].Content).Name == "Nothing")
                    {
                        ret.Add(new int[] { row, i });
                    }
                    else
                    {
                        if (((Image)buttonsMap[row, i].Content).Name[0] == other[0])
                        {
                            ret.Add(new int[] { row, i });
                        }
                        break;
                    }
                }
                return ret;
            }
            if (piece == "BI")
            {
                for (int i = 1; i < 8; i++)
                {
                    try
                    {
                        if (((Image)buttonsMap[row + i, column + i].Content).Name == "Nothing")
                        {
                            ret.Add(new int[] { row + i, column + i });
                        }
                        else
                        {
                            if (((Image)buttonsMap[row + i, column + i].Content).Name[0] == other[0])
                            {
                                ret.Add(new int[] { row + i, column + i });
                            }
                            break;
                        }
                    } catch (IndexOutOfRangeException) { break; }
                }
                for (int i = 1; i < 8; i++)
                {
                    try
                    {
                        if (((Image)buttonsMap[row + i, column - i].Content).Name == "Nothing")
                        {
                            ret.Add(new int[] { row + i, column - i });
                        }
                        else
                        {
                            if (((Image)buttonsMap[row + i, column - i].Content).Name[0] == other[0])
                            {
                                ret.Add(new int[] { row + i, column - i });
                            }
                            break;
                        }
                    }
                    catch (IndexOutOfRangeException) { break; }
                }
                for (int i = 1; i < 8; i++)
                {
                    try
                    {
                        if (((Image)buttonsMap[row - i, column + i].Content).Name == "Nothing")
                        {
                            ret.Add(new int[] { row - i, column + i });
                        }
                        else
                        {
                            if (((Image)buttonsMap[row - i, column + i].Content).Name[0] == other[0])
                            {
                                ret.Add(new int[] { row - i, column + i });
                            }
                            break;
                        }
                    }
                    catch (IndexOutOfRangeException) { break; }
                }
                for (int i = 1; i < 8; i++)
                {
                    try
                    {
                        if (((Image)buttonsMap[row - i, column - i].Content).Name == "Nothing")
                        {
                            ret.Add(new int[] { row - i, column - i });
                        }
                        else
                        {
                            if (((Image)buttonsMap[row - i, column - i].Content).Name[0] == other[0])
                            {
                                ret.Add(new int[] { row - i, column - i });
                            }
                            break;
                        }
                    }
                    catch (IndexOutOfRangeException) { break; }
                }
                return ret;
            }
            if (piece == "QU")
            {
                return (getPosibleMoves("BRO", row, column).Concat(getPosibleMoves("BBI", row, column))).ToList();
            }
            if (piece == "KI")
            {
                for (int i = -1; i <= 1; i ++ )
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        try
                        {
                            if ((i == 0 && j == 0) || (((Image)buttonsMap[row + i, column + j].Content).Name != "Nothing" && ((Image)buttonsMap[row + i, column + j].Content).Name[0] != other[0]))
                            {
                                continue;
                            }
                            ret.Add(new int[] { row + i, column + j });
                        } catch (IndexOutOfRangeException) { continue; }
                    }
                }
                return ret;
            }
            if (piece == "KN")
            {
                int[] longstep = new int[] { -2, 2 };
                int[] shortstep = new int[] { -1, 1 };
                foreach (int i in longstep)
                {
                    foreach (int j in shortstep)
                    {
                        try
                        {
                            if (((Image)buttonsMap[row + i, column + j].Content).Name == "Nothing" || ((Image)buttonsMap[row + i, column + j].Content).Name[0] == other[0])
                            {
                                ret.Add(new int[] { row + i, column + j });
                            }
                        }
                        catch (IndexOutOfRangeException) { }
                        try
                        {
                            if (((Image)buttonsMap[row + j, column + i].Content).Name == "Nothing" || ((Image)buttonsMap[row + j, column + i].Content).Name[0] == other[0])
                            {
                                ret.Add(new int[] { row + j, column + i });
                            }
                        }
                        catch (IndexOutOfRangeException) { }
                    }
                }
                return ret;
            }
            return new List<int[]>();
        }
        private void clearPM()
        {
            foreach (Button b in possibleMoves)
            {
                gameBoard.Children.Remove(b);
            }
            possibleMoves.Clear();
        }
        public void actualMove(int si, int sj, int di, int dj)
        {
            clearPM();
            buttonsMap[di, dj].Content = buttonsMap[si, sj].Content;
            buttonsMap[si, sj].Content = createPiece("Nothing");
            handlePawn(di, dj);
        }
        private void handlePawn(int di, int dj)
        {
            if (di != 0 && di != 7)
            {
                return;
            }
            bool isMe = (isWhite == (((Image)buttonsMap[di, dj].Content).Name[0] == 'W'));
            bool isPawn = ((Image)buttonsMap[di, dj].Content).Name.Substring(1) == "PA";
            if (isPawn)
            {
                buttonsMap[di, dj].Content = createPiece("QU", isMe);
            }
        }
        private void clickedSquare(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            string piece = ((Image)b.Content).Name;
            if (piece != "Nothing" && piece[0] == color[0])
            {
                clearPM();
                lastButtonClicked = b;
                List<int[]> pm = getPosibleMoves(piece, Grid.GetRow(b), Grid.GetColumn(b));
                foreach (int[] cor in pm)
                {
                    Button move = new Button
                    {
                        Content = new Ellipse
                        {
                            Width = 10,
                            Height = 10,
                            Fill = Brushes.DarkOrange
                        },
                        Background = Brushes.Transparent
                    };
                    move.Focusable = false;
                    move.Click += (s, eventt) => {
                        MainWindow.conductAndSend(new string[] { "ChessMovePiece", Grid.GetRow(lastButtonClicked).ToString(), Grid.GetColumn(lastButtonClicked).ToString(), Grid.GetRow((Button)s).ToString(), Grid.GetColumn((Button)s).ToString(), id});
                        clearPM();
                    };
                    possibleMoves.Add(move);
                    Grid.SetRow(move, cor[0]);
                    Grid.SetColumn(move, cor[1]);
                    gameBoard.Children.Add(move);
                }
            }
        }
        public void end(string whoWon)
        {
            wonMessage.Visibility = Visibility.Visible;
            gameBoard.Visibility = Visibility.Hidden;
            if ((whoWon == "W" && isWhite) || (whoWon == "B" && !isWhite))
            {
                wonMessage.Text = "YOU WON!!!";
                return;
            }
            if ((whoWon == "W" && !isWhite) || (whoWon == "B" && isWhite))
            {
                wonMessage.Text = "YOU LOST\n:((";
                return;
            }
            wonMessage.Text = "It's a tie\n:)";
        }
    }
}
