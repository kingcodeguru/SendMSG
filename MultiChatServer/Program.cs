using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Cryptography;

namespace MultiChatServer
{
    class Program
    {
        public static MySQL sql;
        public static string login(string username, string password)
        {
            /**
             * I - username, password
             * O - Validate the username and the password and return "OK" if it is valid
             */
            if (username.Length > 32 || password.Length > 32)
            {
                return "Username and password must be at most 32 characters long.";
            }
            if (sql.loginSuccess(username, password))
            {
                return "OK";
            } else
            {
                return "Wrong username or password.";
            }
        }
        public static string signup(string username, string password)
        {
            /**
             * I - username, password
             * O - Try to sign up the username and the password and return "OK" if managed to do that
             */
            if (username.Length > 32 || password.Length > 32)
            {
                return "Username and password must be at most 32 characters long.";
            }
            if (sql.signUpSuccess(username, password))
            {
                return "OK";
            } else
            {
                return "already exist such username.";
            }
        }
        static void Main(string[] args)
        {
            /**
             * I - None
             * O - The main function
             */
            sql = new MySQL();
            Server s = new Server();
            Directory.CreateDirectory("files");
            Thread acceptNewClients = new Thread(s.initialize);
            acceptNewClients.Name = "Accept new clients";
            acceptNewClients.Start();
            wantToExit(); // doesn't suppose to return until the program is over
            sql.close();
            Console.WriteLine("Exiting Program");
            Environment.Exit(1);
        }


        // These are helper functions that handle exiting the program.
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int nVirtKey);
        // https://stackoverflow.com/questions/7162834/determine-if-current-application-is-activated-has-focus
        private static bool ApplicationIsActivated()
        {
            /**
             * I - None
             * O - Return true if this window's application is focused
             */
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }
            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);
            return activeProcId == procId;
        }
        private static int letter = 0;
        private static void wantToExit()
        {
            /**
             * I - Keyboard inputs
             * O - Finishing if the user has typed 'exit'
             */
            while (true)
            {
                Thread.Sleep(500);
                if (ApplicationIsActivated())
                {
                    if (GetAsyncKeyState((int)ConsoleKey.E) != 0)
                    {
                        letter = 1;
                    }
                    if (letter == 1 && GetAsyncKeyState((int)ConsoleKey.X) != 0)
                    {
                        letter = 2;
                    }
                    if (letter == 2 && GetAsyncKeyState((int)ConsoleKey.I) != 0)
                    {
                        letter = 3;
                    }
                    if (letter == 3 && GetAsyncKeyState((int)ConsoleKey.T) != 0)
                    {
                        letter = 4;
                    }
                    if (GetAsyncKeyState((int)ConsoleKey.Q) != 0)
                    {
                        letter = 0;
                    }
                    if (letter == 4)
                    {
                        break;
                    }
                }
            }
        }
    }
    class Server
    {
        public static long gameId = 0;
        private Socket server;
        private EncryptedSocket client;
        private static List<Client> clientList = new List<Client>();

        public Server()
        {
            server = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public void initialize()
        {
            /**
             * I - None
             * O - Waiting for clients and connecting them to the server
             */
            Chat globalChat = Program.sql.getGlobalChat();
            Console.WriteLine("creating socket");
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 9000);
            server.Bind(endpoint);
            Console.WriteLine("waiting for clients");
            Console.Title = "Server's Log";
            Console.WriteLine("restoring chats...");
            while (true)
            {
                server.Listen(10);
                client = new EncryptedSocket(server.Accept());
                Console.WriteLine("client " + client.getAddress() + " has joined");

                Client c = new Client(client);
                globalChat.addClient(c);
                c.addChat(globalChat);
                clientList.Add(c);
            }
        }
        public static Client findClient(string name)
        {
            /**
             * I - name
             * O - Return the client with {name} as the name
             */
            foreach (Client c in clientList)
            {
                if (c.name == name)
                {
                    return c;
                }
            }
            return null;
        }
        public static void removeClient(Client toRemove)
        {
            /**
             * I - client to remove
             * O - remove the client from list of clients
             */
            clientList.Remove(toRemove);
        }
    }
}
