using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiChatServer
{
    class MySQL
    {
        Dictionary<int, Chat> updatingAllChats = new Dictionary<int, Chat>();
        private SqlConnection connection;
        public void close()
        {
            connection.Close();
        }
        public MySQL()
        {
            /**
             * I - None
             * O - Construct MySQL object that is connected to my SQL server I have created
             */
            Console.WriteLine("connecting SQL server...");
            // GN-100359\SQLEXPRESS
            // LIELPC
            Console.WriteLine("Enter SQL server name or choose one of this two:\n1.) LIELPC\n2.) GN-100359\\SQLEXPRESS\nEnter: ");
            while (true)
            {
                string SqlServerName = Console.ReadLine();
                if (SqlServerName == "1")
                {
                    SqlServerName = "LIELPC";
                }
                if (SqlServerName == "2")
                {
                    SqlServerName = "GN-100359\\SQLEXPRESS";
                }
                string connectionString = $"Server={SqlServerName};Database=SendMsgDataBase;Integrated Security=True;";
                connection = new SqlConnection(connectionString);
                try
                {
                    connection.Open();
                    break;
                } catch (SqlException)
                {
                    Console.Write($"Unable to connect to {SqlServerName} sql server, please enter a different server.\nEnter: ");
                }
            }
            Console.WriteLine("Connected successfuly!");
            Console.WriteLine("Do you want to delete history of the database? [y/n]");
            if (Console.ReadKey().Key == ConsoleKey.Y)
            {
                deleteAll();
                Console.WriteLine("Deleted all.");
            } else
            {
                Console.WriteLine("Restoring chats from data base...");
            }
            restoreChats();
            try
            {
                addChat("global", "everyone", getGlobalChat());
            } catch (SqlException) { }
            catch (ArgumentException) { }
        }
        public bool signUpSuccess(string username, string password)
        {
            /**
             * I - uesrname, password
             * O - Return true if managed to sign up
             */
            string getClientId = "SELECT * FROM  [User] WHERE Username=@name";
            SqlCommand command = new SqlCommand(getClientId, connection);
            command.Parameters.AddWithValue("@name", username);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return false;
                }
            }
            return addClient(username, password);
        }
        public bool loginSuccess(string username, string password)
        {
            /**
             * I - username, password
             * O - Return true if managed to log in
             */
            string getClientId = "SELECT * FROM  [User] WHERE Username=@name AND Password=@password";
            SqlCommand command = new SqlCommand(getClientId, connection);
            command.Parameters.AddWithValue("@name", username);
            command.Parameters.AddWithValue("@password", password);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return true;
                }
            }
            return false;
        }
        public void deleteAll()
        {
            /**
             * I - None
             * O - Initilizes the data base
             */
            string insertQuery = "DELETE FROM Message;DELETE FROM UserChat;DELETE FROM Chat;DELETE FROM[User]; ";
            SqlCommand command = new SqlCommand(insertQuery, connection);
            command.ExecuteNonQuery();
        }
        private bool chatInList(string chatName)
        {
            /**
             * I - chat name
             * O - Return true if exist a chat in the database with such name
             */
            string getChatId = "SELECT * FROM  [Chat] WHERE Chatname=@name";
            SqlCommand command = new SqlCommand(getChatId, connection);
            command.Parameters.AddWithValue("@name", chatName);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return true;
                }
            }
            return false;
        }
        public void addChat(string chatName, string owner, Chat chatToAdd)
        {
            /**
             * I - chat name, owner name, a chat to add
             * O - Adds a chat to the data base
             */
            if (chatInList(chatName))
            {
                return;
            }
            string insertQuery = "INSERT INTO [Chat] (Chatname, Owner) VALUES (@Chatname, @Owner)";
            SqlCommand command = new SqlCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@Chatname", chatName);
            command.Parameters.AddWithValue("@Owner", owner);
            try
            {
                int rowsAffected = command.ExecuteNonQuery();
            } catch (SqlException) { }

            string getChatId = "SELECT * FROM  [Chat] WHERE Chatname=@name";
            command = new SqlCommand(getChatId, connection);
            command.Parameters.AddWithValue("@name", chatName);
            int chatId = 0;
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    chatId = (int)reader["ID"];
                }
            }
            updatingAllChats.Add(chatId, chatToAdd);
        }
        public bool removeChat(string chatName)
        {
            /**
             * I - chat name
             * O - Remove the chat from the data base
             */
            string getChatId = "SELECT * FROM  [Chat] WHERE Chatname=@name";
            SqlCommand command = new SqlCommand(getChatId, connection);
            command.Parameters.AddWithValue("@name", chatName);
            int chatId = 0;
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    chatId = (int)reader["ID"];
                }
            }
            string removeQuery = "DELETE FROM [UserChat] WHERE ChatId=@chatId";
            command = new SqlCommand(removeQuery, connection);
            command.Parameters.AddWithValue("@chatId", chatId);
            command.ExecuteNonQuery();


            removeQuery = "DELETE FROM [Chat] WHERE ID=@chatId";
            command = new SqlCommand(removeQuery, connection);
            command.Parameters.AddWithValue("@chatId", chatId);
            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected == 1;
        }
        public bool removeClientFromChat(string client, string chat)
        {
            /**
             * I - client name, chat name
             * O - Remove the connection between the client and the chat from the data base
             */
            int clientId = 0;
            int chatId = 0;
            string getClientId = "SELECT * FROM  [User] WHERE Username=@name";
            SqlCommand command = new SqlCommand(getClientId, connection);
            command.Parameters.AddWithValue("@name", client);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    clientId = (int)reader["ID"];
                }
            }
            string getChatId = "SELECT * FROM  [Chat] WHERE Chatname=@name";
            command = new SqlCommand(getChatId, connection);
            command.Parameters.AddWithValue("@name", chat);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    chatId = (int)reader["ID"];
                }
            }
            if (clientId == 0 || chatId == 0)
            {
                return false;
            }
            string removeQuery = "DELETE FROM [UserChat] WHERE UserId=@userId AND ChatId=@chatId";
            command = new SqlCommand(removeQuery, connection);
            command.Parameters.AddWithValue("@userId", clientId);
            command.Parameters.AddWithValue("@chatId", chatId);
            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected == 1;
        }
        public bool addClientToChat(string client, string chat)
        {
            /**
             * I - client name, chat name
             * O - Adds a connection between the client and the chat to the data base
             */
            int clientId = 0;
            int chatId = 0;
            string getClientId = "SELECT * FROM  [User] WHERE Username=@name";
            SqlCommand command = new SqlCommand(getClientId, connection);
            command.Parameters.AddWithValue("@name", client);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    clientId = (int)reader["ID"];
                }
            }
            string getChatId = "SELECT * FROM  [Chat] WHERE Chatname=@name";
            command = new SqlCommand(getChatId, connection);
            command.Parameters.AddWithValue("@name", chat);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    chatId = (int)reader["ID"];
                }
            }
            if (clientId == 0 || chatId == 0)
            {
                return false;
            }
            string insertQuery = "INSERT INTO [UserChat] (UserID, ChatID) VALUES (@user, @chat)";
            command = new SqlCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@user", clientId);
            command.Parameters.AddWithValue("@chat", chatId);
            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected == 1;
        }
        public bool removeClient(string username)
        {
            /**
             * I - client name
             * O - Remove the client from the data base
             */
            string insertQuery = "DELETE FROM [User] WHERE Username=@name";
            SqlCommand command = new SqlCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@name", username);
            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected == 1;
        }
        private bool addClient(string username, string password)
        {
            /**
             * I - client name, password
             * O - Add client to the data base
             */
            string insertQuery = "INSERT INTO [User] (Username, Password) VALUES (@name, @password)";
            SqlCommand command = new SqlCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@name", username);
            command.Parameters.AddWithValue("@password", password);
            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected == 1;
        }
        public void addMessage(string message, string chat, string sender, bool isMessageOrFile)
        {
            /**
             * I - message, chat name, sender, wheter is message or file
             * O - Add a record for this message
             */
            string getChatId = "SELECT * FROM  [Chat] WHERE Chatname=@name";
            SqlCommand command = new SqlCommand(getChatId, connection);
            command.Parameters.AddWithValue("@name", chat);
            int chatId = 0;
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    chatId = (int)reader["ID"];
                }
            }
            string insertQuery = "INSERT INTO [Message] (Chat, Message, Sender, isString) VALUES (@chat, @message, @sender, @isstring)";
            command = new SqlCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@chat", chatId);
            command.Parameters.AddWithValue("@message", message);
            command.Parameters.AddWithValue("@sender", sender);
            command.Parameters.AddWithValue("@isstring", isMessageOrFile);
            command.ExecuteNonQuery();
        }
        private Dictionary<int, Chat> restoreChats()
        {
            /**
                * I - None
                * O - Restore all chats from the data base
                */
            var ret = updatingAllChats;
            string getChat = "SELECT * FROM  [Chat]";
            SqlCommand command = new SqlCommand(getChat, connection);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = (int)reader["ID"];
                    if (ret.Keys.ToList().Contains(id))
                    {
                        continue;
                    }
                    Console.WriteLine("new chat: " + id.ToString());
                    Console.WriteLine((int)(Encoding.UTF8.GetBytes((string)reader["Owner"])[20]));
                    Client c = new Client(castString((string)reader["Owner"]));
                    string chatName = castString((string)reader["Chatname"]);
                    Chat newChat;
                    if (chatName == "global")
                    {
                        newChat = getGlobalChat();
                    }
                    else
                    {
                        newChat = new Chat(chatName, c);
                    }
                    ret.Add(id, newChat);
                }
            }
            string getMsg = "SELECT * FROM  [Message]";
            SqlCommand getMsgCmd = new SqlCommand(getMsg, connection);
            using (SqlDataReader readMsg = getMsgCmd.ExecuteReader())
            {
                while (readMsg.Read())
                {
                    int id = (int)readMsg["Chat"];
                    string sender = castString((string)readMsg["Sender"]);
                    string msg = castString((string)readMsg["Message"]);
                    ret[id].addMsg(sender, msg, (bool)readMsg["isString"]);
                }
            }
            return ret;
        }
        private string castString(string data)
        {
            /**
             * I - data from the data base
             * O - removes all of the spaces that the data base formatting is adding to the end of each record
             */
            int i = data.Length - 1;
            while (i >= 0 && data[i] == ' ') { i -= 1; }
            return data.Substring(0, i + 1);
        }
        private List<int> getChats(string name)
        {
            /**
             * I - client name
             * O - return a list of the ids of the chats that the client is part of
             */
            var ret = new List<int>();
            string getUserId = "SELECT * FROM  [User] WHERE Username=@name";
            SqlCommand command = new SqlCommand(getUserId, connection);
            command.Parameters.AddWithValue("@name", name);
            int userId = 0;
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    userId = (int)reader["ID"];
                }
            }
            string getAllChats = "SELECT * FROM  [UserChat] WHERE USERID=@id";
            command = new SqlCommand(getAllChats, connection);
            command.Parameters.AddWithValue("@id", userId);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    ret.Add((int)reader["ChatID"]);
                }
            }
            return ret;
        }
        public void newJoin(Client c)
        {
            /**
             * I - client
             * O - handle new entrance when it accures
             */
            Dictionary<int, Chat> allChats = updatingAllChats;
            List<int> myChatsIndex = getChats(c.name);
            foreach (int n in myChatsIndex)
            {
                if (!allChats.Keys.ToList().Contains(n))
                {
                    continue;
                }
                Chat current = allChats[n];
                c.addChat(current);
                current.addClient(c);
                if (current.admin.name == c.name)
                {
                    current.admin = c;
                }
                c.conductAndSend(new string[] { "addedToChat", current.admin.name, current.name });
                Thread.Sleep(100);
            }

        }
        public Chat getGlobalChat()
        {
            /**
             * I - None
             * O - Return the global chat
             */
            Dictionary<int, Chat> allChats = updatingAllChats;
            foreach (Chat c in allChats.Values)
            {
                if (c.name == "global")
                {
                    return c;
                }
            }
            Chat global = new Chat("global", new Client("NoOne"));
            return global;
        }
    }
}
