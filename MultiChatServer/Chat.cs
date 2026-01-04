using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChatServer
{
    class Chess:Chat
    {
        string[,] board = new string[8, 8];
        public long id;
        Client black, white;
        bool iswhiteturn = true;
        public Chess(Client p1, Client p2) : base((++Server.gameId).ToString(), new Client(""))
        {
            id = Server.gameId;
            if ((new Random()).Next(1, 3) == 1)
            {
                black = p1;
                white = p2;
            }
            else
            {
                black = p2;
                white = p1;
            }
            black.sendMsg(black.conductMsg(new string[] { "startChess", "B", id.ToString(), white.name }));
            white.sendMsg(white.conductMsg(new string[] { "startChess", "W", id.ToString(), black.name }));
            for (int i = 0; i < 8; i ++)
            {
                for (int j = 0; j < 8; j ++)
                {
                    board[i, j] = "Nothing";
                    if (i == 6)
                    {
                        board[i, j] = "WPA";
                    }
                    if (i == 1)
                    {
                        board[i, j] = "BPA";
                    }
                }
            }
            board[0, 0] = "BRO";
            board[0, 1] = "BKN";
            board[0, 2] = "BBI";
            board[0, 3] = "BQU";
            board[0, 4] = "BKI";
            board[0, 5] = "BBI";
            board[0, 6] = "BKN";
            board[0, 7] = "BRO";

            board[7, 0] = "WRO";
            board[7, 1] = "WKN";
            board[7, 2] = "WBI";
            board[7, 3] = "WQU";
            board[7, 4] = "WKI";
            board[7, 5] = "WBI";
            board[7, 6] = "WKN";
            board[7, 7] = "WRO";
        }
        public void PrintBoard()
        {
            for (int i = 0; i < 8; i ++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Console.Write(board[i, j].Substring(0, 3) + " ");
                }
                Console.WriteLine("");
            }
        }
        public void MovePeice(int si, int sj, int di, int dj, Client player)
        {
            bool iswhite = (player == white);
            if (!iswhite)
            {
                si = 7 - si;
                sj = 7 - sj;
                di = 7 - di;
                dj = 7 - dj;
            }
            if (iswhite != iswhiteturn)
            {
                Console.WriteLine("Not your turn");
                return;
            }
            if ((board[si, sj][0] != 'W' && iswhite) || (board[si, sj][0] != 'B' && !iswhite))
            {
                return;
            }
            bool contains = false;
            List<int[]> pm = possibleMoves(si, sj, iswhite);
            foreach (int[] move in pm)
            {
                if (move[0] == di && move[1] == dj) {
                    contains = true;
                }
            }
            if (contains)
            {
                if (!isValidMove(si, sj, di, dj, iswhite))
                {
                    return;
                }
                board[di, dj] = board[si, sj];
                board[si, sj] = "Nothing";
                HandlePawn(di, dj);
                if (!existValidMove(!iswhite))
                {
                    if (onCheck(!iswhite))
                    {
                        if (iswhite)
                        {
                            string[] message = new string[] { "chessGaveOver", "W", id.ToString() };
                            white.conductAndSend(message);
                            black.conductAndSend(message);
                            Console.WriteLine("WHITE WINSSSSS");
                        } else
                        {
                            string[] message = new string[] { "chessGaveOver", "B", id.ToString() };
                            white.conductAndSend(message);
                            black.conductAndSend(message);
                            Console.WriteLine("BLACK WINSSSSS");
                        }
                    } else
                    {
                        string[] message = new string[] { "chessGaveOver", "T", id.ToString() };
                        white.conductAndSend(message);
                        black.conductAndSend(message);
                        Console.WriteLine("ITS A TIEEEEEE");
                    }
                    return;
                }
                iswhiteturn = !iswhiteturn;
                white.conductAndSend(new string[] { "moveChessPiece", si.ToString(), sj.ToString(), di.ToString(), dj.ToString(), id.ToString() });
                black.conductAndSend(new string[] { "moveChessPiece", (7-si).ToString(), (7-sj).ToString(), (7-di).ToString(), (7-dj).ToString(), id.ToString() });
            }
        }
        private void HandlePawn(int di, int dj)
        {
            if (board[di, dj] == "WPA" && di == 0)
            {
                board[di, dj] = "WQU";
            }
            if (board[di, dj] == "BPA" && di == 7)
            {
                board[di, dj] = "BQU";
            }
        }
        private int[] getKingCord(bool isWhite)
        {
            char me = 'W';
            if (!isWhite)
            {
                me = 'B';
            }
            for (int i = 0; i < 8; i ++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (board[i, j] == me + "KI")
                    {
                        return new int[2] { i, j };
                    }
                }
            }
            return new int[2] { 0, 0 };
        }
        private bool onCheck(bool isWhite)
        {
            int[] kings = getKingCord(isWhite);
            char other = 'W';
            if (isWhite)
            {
                other = 'B';
            }
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (board[i, j][0] == other)
                    {
                        List<int[]> moves = possibleMoves(i, j, !isWhite);
                        foreach (var move in moves)
                        {
                            if (move[0] == kings[0] && move[1] == kings[1])
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        private bool existValidMove(bool isWhite)
        {
            char me = 'B';
            if (isWhite)
            {
                me = 'W';
            }
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (board[i, j][0] == me)
                    {
                        List<int[]> temp = possibleMoves(i, j, isWhite);
                        foreach (var move in temp)
                        {
                            if (isValidMove(i, j, move[0], move[1], isWhite))
                            {
                                return true;
                            }
                        }

                    }
                }
            }
            return false;
        }
        private bool isValidMove(int si, int sj, int di, int dj, bool isWhite)
        {
            string saves = board[si, sj];
            string saved = board[di, dj];
            board[di, dj] = board[si, sj];
            board[si, sj] = "Nothing";
            bool now = onCheck(isWhite);
            board[di, dj] = saved;
            board[si, sj] = saves;
            return !now;

        }
        private List<int[]> possibleMoves(int row, int column, bool iswhite)
        {
            string piece = board[row, column].Substring(1);
            char other;
            if (board[row, column][0] == 'W')
            {
                other = 'B';
            } else
            {
                other = 'W';
            }

            List<int[]> ret = new List<int[]>();
            if (piece == "PA")
            {
                if ((iswhite && row == 0) || (!iswhite && row == 7))
                {
                    return new List<int[]>();
                }
                if (iswhite)
                {
                    if (row == 6)
                    {
                        if (board[row - 1, column] == "Nothing" && board[row - 2, column] == "Nothing")
                        {
                            ret.Add(new int[] { row - 2, column });
                        }
                    }
                    if (board[row - 1, column] == "Nothing")
                    {
                        ret.Add(new int[] { row - 1, column });
                    }
                    if (row > 0 && column > 0 && board[row - 1, column - 1][0] == other)
                    {
                        ret.Add(new int[] { row - 1, column - 1 });
                    }
                    if (row > 0 && column < 7 && board[row - 1, column + 1][0] == other)
                    {
                        ret.Add(new int[] { row - 1, column + 1 });
                    }
                    return ret;
                }
                if (row == 1)
                {
                    if (board[row + 1, column] == "Nothing" && board[row + 2, column] == "Nothing")
                    {
                        ret.Add(new int[] { row + 2, column });
                    }
                }
                if (board[row + 1, column] == "Nothing")
                {
                    ret.Add(new int[] { row + 1, column });
                }
                if (column < 7 && board[row + 1, column + 1][0] == other)
                {
                    ret.Add(new int[] { row + 1, column + 1 });
                }
                if (column > 0 && board[row + 1, column - 1][0] == other)
                {
                    ret.Add(new int[] { row + 1, column - 1 });
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
                    if (board[i, column] == "Nothing")
                    {
                        ret.Add(new int[] { i, column });
                    }
                    else
                    {
                        if (board[i, column][0] == other)
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
                    if (board[i, column] == "Nothing")
                    {
                        ret.Add(new int[] { i, column });
                    }
                    else
                    {
                        if (board[i, column][0] == other)
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
                    if (board[row, i] == "Nothing")
                    {
                        ret.Add(new int[] { row, i });
                    }
                    else
                    {
                        if (board[row, i][0] == other)
                        {
                            ret.Add(new int[] { row, i });
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
                    if (board[row, i] == "Nothing")
                    {
                        ret.Add(new int[] { row, i });
                    }
                    else
                    {
                        if (board[row, i][0] == other)
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
                        if (board[row + i, column + i] == "Nothing")
                        {
                            ret.Add(new int[] { row + i, column + i });
                        }
                        else
                        {
                            if (board[row + i, column + i][0] == other)
                            {
                                ret.Add(new int[] { row + i, column + i });
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
                        if (board[row + i, column - i] == "Nothing")
                        {
                            ret.Add(new int[] { row + i, column - i });
                        }
                        else
                        {
                            if (board[row + i, column - i][0] == other)
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
                        if (board[row - i, column + i] == "Nothing")
                        {
                            ret.Add(new int[] { row - i, column + i });
                        }
                        else
                        {
                            if (board[row - i, column + i][0] == other)
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
                        if (board[row - i, column - i] == "Nothing")
                        {
                            ret.Add(new int[] { row - i, column - i });
                        }
                        else
                        {
                            if (board[row - i, column - i][0] == other)
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
                for (int i = row + 1; i < 8; i++)
                {
                    if (i < 0 || i >= 8)
                    {
                        break;
                    }
                    if (board[i, column] == "Nothing")
                    {
                        ret.Add(new int[] { i, column });
                    }
                    else
                    {
                        if (board[i, column][0] == other)
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
                    if (board[i, column] == "Nothing")
                    {
                        ret.Add(new int[] { i, column });
                    }
                    else
                    {
                        if (board[i, column][0] == other)
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
                    if (board[row, i] == "Nothing")
                    {
                        ret.Add(new int[] { row, i });
                    }
                    else
                    {
                        if (board[row, i][0] == other)
                        {
                            ret.Add(new int[] { row, i });
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
                    if (board[row, i] == "Nothing")
                    {
                        ret.Add(new int[] { row, i });
                    }
                    else
                    {
                        if (board[row, i][0] == other)
                        {
                            ret.Add(new int[] { row, i });
                        }
                        break;
                    }
                }
                for (int i = 1; i < 8; i++)
                {
                    try
                    {
                        if (board[row + i, column + i] == "Nothing")
                        {
                            ret.Add(new int[] { row + i, column + i });
                        }
                        else
                        {
                            if (board[row + i, column + i][0] == other)
                            {
                                ret.Add(new int[] { row + i, column + i });
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
                        if (board[row + i, column - i] == "Nothing")
                        {
                            ret.Add(new int[] { row + i, column - i });
                        }
                        else
                        {
                            if (board[row + i, column - i][0] == other)
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
                        if (board[row - i, column + i] == "Nothing")
                        {
                            ret.Add(new int[] { row - i, column + i });
                        }
                        else
                        {
                            if (board[row - i, column + i][0] == other)
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
                        if (board[row - i, column - i] == "Nothing")
                        {
                            ret.Add(new int[] { row - i, column - i });
                        }
                        else
                        {
                            if (board[row - i, column - i][0] == other)
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
            if (piece == "KI")
            {
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        try
                        {
                            if ((i == 0 && j == 0) || (board[row + i, column + j] != "Nothing" && (board[row + i, column + j][0] != other)))
                            {
                                continue;
                            }
                            ret.Add(new int[] { row + i, column + j });
                        }
                        catch (IndexOutOfRangeException) { continue; }
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
                            if (board[row + i, column + j] == "Nothing" || board[row + i, column + j][0] == other)
                            {
                                ret.Add(new int[] { row + i, column + j });
                            }
                        }
                        catch (IndexOutOfRangeException) { }
                        try
                        {
                            if (board[row + j, column + i] == "Nothing" || board[row + j, column + i][0] == other)
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

    }
    class Chat
    {
        public Client admin;
        private string adminName;
        public string name;
        private List<string[]> messages = new List<string[]>();
        private List<Client> cl; // client list
        public Chat(string name, Client admin)
        {
            this.admin = admin;
            this.name = name;
            this.cl = new List<Client>();
        }
        public Chat(string name, string admin)
        {
            adminName = admin;
            this.name = name;
            cl = new List<Client>();
        }
        public bool isEmpty()
        {
            return !cl.Any();
        }
        public void addClient(Client c)
        {
            cl.Add(c);
        }
        public void removeClient(Client c)
        {
            cl.Remove(c);
        }
        public void sendToall(string msg)
        {
            foreach (Client c in cl)
            {
                c.sendMsg(msg);
            }
        }
        public void sendAllChat(Client client)
        {
            client.sendMsgFromSenderAndData(messages, name);
        }
        public void addMsg(string sender, string data, bool isMsg)
        {
            if (isMsg)
            {
                messages.Add(new string[] { sender, data, "message"});
            } else
            {
                messages.Add(new string[] { sender, data, "file"});
            }
        }
    }
}
