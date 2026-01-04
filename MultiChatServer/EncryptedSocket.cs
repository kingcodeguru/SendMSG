using System;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace MultiChatServer
{
    class EncryptedSocket
    {
        public EncryptedSocket(Socket socket)
        {
            /**
             * I - Socket
             * O - Construct an EncryptedSocket object
             */
            this.socket = socket;
            aes = Aes.Create();
            RSA temp = recvRSA();
            sendAes(temp);
        }

        private Aes aes;
        private Socket socket;
        private bool isEncrypted = true;


        public void setIsEncrypted(bool isEncrypted)
        {
            this.isEncrypted = isEncrypted;
        }

        private byte[] Receive()
        {
            /**
             * I - None
             * O - Receives from the socket as much information as there is
             */
            return Receive(socket.Available);
        }
        private byte[] Receive(int number)
        {
            /**
             * I - number of bytes to receive
             * O - Receive that number of bytes and return it
             */
            byte[] ret = new byte[number];
            socket.Receive(ret);
            return ret;
        }
        private void SendUnEncrypted(byte[] source)
        {
            /**
             * I - bytes to send
             * O - Send this bytes without the encryption stage
             */
            socket.Send(source);
        }
        private string FromAesKeyToXmlString()
        {
            /**
             * I - None
             * O - Convert the aes field to an xml string
             */
            XElement xml = new XElement("AESKey",
                new XElement("Key", Convert.ToBase64String(aes.Key)),
                new XElement("IV", Convert.ToBase64String(aes.IV))
            );
            return xml.ToString();
        }
        private void FromXmlStringToAesKey(string xmlString)
        {
            /**
             * I - xml string
             * O - Updating the aes field according to the xml string
             */
            XElement xml = XElement.Parse(xmlString);
            aes.Key = Convert.FromBase64String(xml.Element("Key").Value);
            aes.IV = Convert.FromBase64String(xml.Element("IV").Value);
        }
        private RSA recvRSA()
        {
            /**
             * I - None
             * O - Receives an RSA public key
             */
            RSA rsa = RSA.Create();
            byte[] key = Receive(243);
            rsa.FromXmlString(Encoding.UTF8.GetString(key));
            return rsa;
        }
        private void sendRSA(RSA rsa)
        {
            /**
             * I - RSA object
             * O - Send the RSA's public key
             */
            byte[] data = Encoding.UTF8.GetBytes(rsa.ToXmlString(false));
            SendUnEncrypted(data);
        }
        private void recvAes(RSA rsa)
        {
            /**
             * I - RSA object
             * O - Receives an AES key that is encrypted with the RSA algorithm, decrypt it and put it in aes field
             */
            byte[] key = Receive();
            key = rsa.Decrypt(key, RSAEncryptionPadding.Pkcs1);
            FromXmlStringToAesKey(Encoding.UTF8.GetString(key));
        }
        private void sendAes(RSA rsa)
        {
            /**
             * I - RSA object
             * O - send an AES key that is encrypted using the RSA algorithm
             */
            aes.GenerateKey();
            aes.GenerateIV();
            string stringToSend = FromAesKeyToXmlString();
            byte[] toSend = Encoding.UTF8.GetBytes(stringToSend);
            toSend = rsa.Encrypt(toSend, RSAEncryptionPadding.Pkcs1);
            SendUnEncrypted(toSend);
        }


        private byte[] Encrypt(string plaintext)
        {
            /**
             * I - plain text
             * O - Encrypt the text
             */
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(plaintext);
                }
                return ms.ToArray();
            }
        }
        private string Decrypt(byte[] encryptedData)
        {
            /**
             * I - encrypted text
             * O - Decrypt the text
             */
            using (MemoryStream ms = new MemoryStream(encryptedData))
            using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
            using (StreamReader sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }

        public void send(string s)
        {
            /**
             * I - plain text
             * O - send the encrypted text
             */
                Console.WriteLine($"Send: {Encrypt(s).Length.ToString().PadLeft(5, '0')}{s}");
            byte[] data;
            if (isEncrypted)
            {
                data = Encrypt(s);
            }
            else
            {
                data = Encoding.UTF8.GetBytes(s);
            }
            SendUnEncrypted(Encoding.UTF8.GetBytes(data.Length.ToString().PadLeft(5, '0')));
            SendUnEncrypted(data);
            }
        public string recv()
        {
            /**
             * I - None
             * O - Receive from socket and return decrypted message
             */
            byte[] length = Receive(5);
            Console.WriteLine(Encoding.UTF8.GetString(length));
            byte[] data = Receive(int.Parse(Encoding.UTF8.GetString(length)));
            if (isEncrypted)
            {
                string ret = Decrypt(data);
                Console.WriteLine(ret);
                return ret;
            } else
            {
                string ret = Encoding.UTF8.GetString(data);
                Console.WriteLine(ret);
                return ret;
            }
        }


        /**
         * This all are small helper methods:
         */
        public string getAddress() { return ((IPEndPoint)socket.RemoteEndPoint).Address.ToString() + ":" + ((IPEndPoint)socket.RemoteEndPoint).Port.ToString(); }
        public void clearSocketBuffer() { Receive(socket.Available); }
        public bool isFinished() { return socket.Poll(1000, SelectMode.SelectRead) && (socket.Available == 0); }
        public bool ContainSmth() { return socket.Available != 0; }
        public void WaitUntilMsg() { socket.ReceiveTimeout = -1; }

        public string[] ParseMsg()
        {
            /**
             * I - None
             * O - Receive from socket and return the message after parsing it using the lengths according to the protocol
             */
            var ret = new List<string>();
            string packet = recv();
            int n;
            while (packet != "")
            {
                n = int.Parse(packet.Substring(0, 5));
                ret.Add(packet.Substring(5, n));
                packet = packet.Substring(n + 5);
            }
            string[] arr = ret.ToArray();
            return ret.ToArray();
        }

        public void sendFile(string fileName)
        {
            /**
             * I - file name
             * O - send the file
             */
            int min(int x, int y)
            {
                if (x < y)
                {
                    return x;
                }
                return y;
            }
            string path = @"files\" + fileName;
            byte[] buffer = new byte[1024];
            Thread.Sleep(50);
            byte[] intro;
            FileStream fs;
            try
            {
                fs = new FileStream(path, FileMode.Open);
            }
            catch (FileNotFoundException)
            {
                return;
            }
            catch (IOException)
            {
                return;
            }
            BinaryReader br = new BinaryReader(fs, Encoding.ASCII);
            long fileLength = fs.Length;
            int offset = 0;
            int k;
            while (fileLength > 0)
            {
                k = min((int)fileLength, 1024);
                fs.Read(buffer, offset, k);
                fileLength -= k;
                intro = Encoding.UTF8.GetBytes("1" + k.ToString().PadLeft(4, '0'));
                SendUnEncrypted(intro.Concat(buffer).ToArray());
            }
            SendUnEncrypted(Encoding.UTF8.GetBytes("0"));
            br.Close();
        }
        public void recvFile(string fileName)
        {
            /**
             * I - file name
             * O - Download the file
             */
            Console.WriteLine("Trying to receive a file");
            string path = @"files\" + fileName;
            FileStream fs;
            try
            {
                fs = new FileStream(path, FileMode.OpenOrCreate);
            }
            catch (DirectoryNotFoundException)
            {
                clearSocketBuffer();
                return;
            }
            BinaryWriter bw = new BinaryWriter(fs, Encoding.ASCII);
            byte[] buffer = new byte[1024];
            byte[] length = new byte[4];
            byte[] con;
            while (true)
            {
                con = Receive(1);
                if (Encoding.UTF8.GetString(con) != "1")
                {
                    break;
                }
                length = Receive(4);
                int length2 = int.Parse(Encoding.UTF8.GetString(length));
                buffer = Receive(length2);
                bw.Write(buffer, 0, length2);
            }
            clearSocketBuffer();
            bw.Close();
        }
    }
}
