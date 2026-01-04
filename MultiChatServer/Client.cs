using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;

namespace MultiChatServer
{
    class Client
    {

        private bool signed = false;
        public void removeChat(Chat chat)
        {
            lchat.Remove(chat);
        }
        public void addChat(Chat chat)
        {
            lchat.Add(chat);
        }
        private bool exit = false;
        private EncryptedSocket s;
        public string name;
        private List<Chat> lchat;
        public Client(EncryptedSocket s)
        {
            this.s = s;
            lchat = new List<Chat>();
            Thread read = new Thread(readFromClient);
            read.Name = "read" + name;
            read.Start();
        }
        public Client(EncryptedSocket s, List<Chat> lc)
        {
            this.s = s;
            lchat = lc;
            Thread read = new Thread(readFromClient);
            read.Name = "read" + name;
            read.Start();
        }
        public Client(string name)
        {
            this.name = name;
        }
        public void conductAndSend(string [] lst)
        {
            s.send(conductMsg(lst));
        }
        /*
         * This method constantly working.
         * Trying to read from the client and reacting to it's requests.
         * When reciving "SocketException" we can conclude the client has left.
         */
        private void readFromClient()
        {
            int i = 1;
            while (true)
            {
                try
                {
                    Thread.Sleep(100); // so that it won't crash
                    if (s.isFinished())
                    {
                        exit = true;
                    }
                    if (exit == true)
                    {
                        Console.WriteLine("client " + s.getAddress() + " has left");
                        foreach (Chat chat in lchat)
                        {
                            chat.removeClient(this);
                        }
                        lchat.Clear();
                        Server.removeClient(this);
                        break;
                    }
                    if (!s.ContainSmth())
                    {
                        continue;
                    }
                    s.WaitUntilMsg();
                    string[] parsedPacket = s.ParseMsg();
                    Console.WriteLine("command = " + parsedPacket[0]);
                    Console.WriteLine($"starting to handle command number {i}");
                    handleCmd(parsedPacket);
                    Console.WriteLine($"finished handling  command number {i++}");
                }
                catch (SocketException)
                {
                    foreach (Chat chat in lchat)
                    {
                        chat.removeClient(this);
                    }
                    lchat.Clear();
                    Console.WriteLine("client " + s.getAddress() + " has left");
                    break;
                }
            }
        }

        /*
         * The main function that handle the command given from the client.
         */
        public void handleCmd(string [] command)
        {
            string cmd = command[0];
            if (signed)
            {
                switch (cmd)
                {
                    case "uploadFile":
                        uploadFile(command[1], command[2]);
                        break;
                    case "downloadFile":
                        downloadFile(command[1], command[2]);
                        break;
                    case "sendMsg":
                        sendMsg(command[2], command[1]);
                        break;
                    case "newChat":
                        newChat(command[1]);
                        break;
                    case "sendMeAll":
                        sendMeAll(command[1]);
                        break;
                    case "addClientToChat":
                        addClientToChat(command[1], command[2]);
                        break;
                    case "leaveChat":
                        leaveChat(command[1]);
                        break;
                    case "sendChessInvite":
                        sendChessInvite(command[1]);
                        break;
                    case "acceptChessInvite":
                        acceptChessInvite(command[1]);
                        break;
                    case "ChessMovePiece":
                        ChessMovePiece(int.Parse(command[1]), int.Parse(command[2]), int.Parse(command[3]), int.Parse(command[4]), int.Parse(command[5]));
                        break;
                    default:
                        Console.WriteLine("Invalid command.");
                        break;
                }
            } else
            {
                // not signed in yet.
                string respond = "";
                if (cmd == "login")
                {
                    respond = Program.login(command[1], command[2]);
                    if (respond == "OK")
                    {
                        name = command[1];
                        s.send(":)");
                        s.setIsEncrypted(command[3] == "yesEncrypted");
                        signed = true;
                        Program.sql.newJoin(this);
                    } else
                    {
                        s.send(":(");
                        s.send(conductMsg(new string[] { respond }));
                    }
                }
                if (cmd == "signup")
                {
                    respond = Program.signup(command[1], command[2]);
                    if (respond == "OK")
                    {
                        name = command[1];
                        s.send(":)");
                        s.setIsEncrypted(command[3] == "yesEncrypted");
                        signed = true;
                    } else
                    {
                        s.send(":(");
                        s.send(conductMsg(new string[] { respond }));
                    }
                }
            }
        }

        // Files functions:
        public void uploadFile(string fileName, string chatName)
        {
            s.recvFile(fileName);
            Chat chat = findChat(chatName);
            if (chat == null)
            {
                Console.WriteLine("Didn't found " + chatName + " inside list of chats.");
                return;
            }
            chat.addMsg(name, fileName, false);
            chat.sendToall(conductMsg(new string[] { "spreadFile", name, fileName, chatName }));
            Program.sql.addMessage(fileName, chatName, name, false);
        }
        public void downloadFile(string fileName, string chatName)
        {
            conductAndSend(new string[] { "downloadFile", chatName, fileName });
            s.sendFile(fileName);
        }

        // Chat rooms functions:
        public void sendMsg(string msg, string chatName)
        {
            Chat chat = this.findChat(chatName);
            if (chat == null)
            {
                Console.WriteLine("Didn't fount " + chatName + " inside list of chats.");
                return;
            }
            if (msg.Length >= 1024)
            {
                return;
            }
            chat.addMsg(name, msg, true);
            chat.sendToall(conductMsg(new string[] { "spreadMsg", name, msg, chatName }));
            Program.sql.addMessage(msg, chatName, name, true);
        }
        public void newChat(string chatName)
        {
            if (chatName.Length >= 32)
            {
                return;
            }
            Chat newChat = new Chat(chatName, this);
            newChat.addClient(this);
            addChat(newChat);
            Console.WriteLine("sending msg: createChatSuccessfully, " + chatName);
            conductAndSend(new string[] { "createChatSuccessfully", chatName });
            Program.sql.addChat(chatName, name, newChat);
            Program.sql.addClientToChat(name, chatName);
        }
        public void sendMeAll(string chatName)
        {
            Chat c = findChat(chatName);
            if (c != null)
            {
                c.sendAllChat(this);
            }
        }
        public void addClientToChat(string clientName, string chatName)
        {
            Chat chat = findChat(chatName);
            if (name != chat.admin.name)
            {
                return;
            }
            Client client = Server.findClient(clientName);
            if (client != null && client != this)
            {
                chat.addClient(client);
                client.addChat(chat);
                client.conductAndSend(new string[] { "addedToChat", this.name, chatName });
            }
            Program.sql.addClientToChat(clientName, chatName);
        }
        public void leaveChat(string chatName)
        {
            Chat chat = findChat(chatName);
            Client client = Server.findClient(name);
            if (chat == null)
            {
                Console.Write("cannot find chat: " + chatName);
                return;
            }
            Console.Write("removing client: " + client.name + " from chat: " + chatName);
            chat.removeClient(client);
            client.removeChat(chat);
            client.conductAndSend(new string[] { "leaveChatSuccessfully", chatName });
            Program.sql.removeClientFromChat(name, chatName);
            if (chat.isEmpty())
            {
                Program.sql.removeChat(chat.name);
            }
        }

        // Chess functions:
        List<Chess> chessGames = new List<Chess>();
        List<Client> chessInvites = new List<Client>();
        public void sendChessInvite(string clientName)
        {
            Client opponent = Server.findClient(clientName);
            if (opponent == null)
            {
                Console.Write("cannot find gamer: " + clientName);
                return;
            }
            if (opponent == this)
            {
                return;
            }
            Console.Write("asking to play with client: " + opponent.name);
            opponent.conductAndSend(new string[] { "chessInvite", name });
            chessInvites.Add(this);
        }
        public void acceptChessInvite(string clientName)
        {
            Client opponent = Server.findClient(clientName);
            try
            {
                chessInvites.Remove(opponent);
            }
            catch (KeyNotFoundException) { return; }
            if (this == opponent)
            {
                // You can't invite yourself.
                return;
            }
            Console.Write("Starting game between: " + name + " and " + opponent.name);
            Chess game = new Chess(this, opponent);
            chessGames.Add(game);
            opponent.chessGames.Add(game);
        }
        public void ChessMovePiece(int si, int sj, int di, int dj, int gameId)
        {
            foreach (Chess game in chessGames)
            {
                if (game.id == gameId)
                {
                    game.MovePeice(si, sj, di, dj, this);
                    break;
                }
            }
        }


        public void sendMsgFromSenderAndData(List<string[]> messages, string chatName)
        {
            Console.WriteLine("Sending the entire chat...");
            string toSend = conductMsg(new string[] {"AllChat"});
            foreach (string[] msg in messages)
            {
                if (msg[2] == "message")
                {
                    toSend += conductMsg(new string[] { "spreadMsg", msg[0], msg[1], chatName });
                }
                if (msg[2] == "file")
                {
                    toSend += conductMsg(new string[] { "spreadFile", msg[0], msg[1], chatName });
                }
            }
            s.send(toSend);
        }
        private Chat findChat(string name)
        {
            foreach (Chat c in lchat)
            {
                if (c.name == name)
                {
                    return c;
                }
            }
            return null;
        }
        public string conductMsg(string[] strings)
        {
            string ret = "";
            foreach (string lable in strings)
            {
                ret += lable.Length.ToString().PadLeft(5, '0') + lable;
            }
            return ret;
        }
        public void sendMsg(string msg)
        {
            s.send(msg);
        }
    }
}
