using System;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SomeProject.Library.Client
{
    public class Client
    {
        public TcpClient tcpClient;
        /// <summary>
        /// Получить сообщение с сервера
        /// </summary>
        public OperationResult ReceiveMessageFromServer()
        {
            try
            {
                tcpClient = new TcpClient("127.0.0.1", 8080);
                StringBuilder recievedMessage = new StringBuilder();
                byte[] data = new byte[256];
                NetworkStream stream = tcpClient.GetStream();

                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    recievedMessage.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);
                stream.Close();
                tcpClient.Close();

                return new OperationResult(Result.OK, recievedMessage.ToString());
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.ToString());
            }
        }

        /// <summary>
        /// Отправить сообщение на сервер
        /// </summary>
        /// <param name="message"></param>
        public OperationResult SendMessageToServer(string message)
        {
            try
            {
                tcpClient = new TcpClient("127.0.0.1", 8080);
                NetworkStream stream = tcpClient.GetStream();

                BinaryFormatter formatter = new BinaryFormatter();
                // Указываем, что передаем сообщение
                formatter.Serialize(stream, true);

                byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                stream.Close();
                tcpClient.Close();
                return new OperationResult(Result.OK, "Сообщение отправлено.");
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }

        /// <summary>
        /// Отправить файл на сервер
        /// </summary>
        /// <param name="path"></param>
        public OperationResult SendFileToServer(string path)
        {
            try
            {
                tcpClient = new TcpClient("127.0.0.1", 8080);
                NetworkStream stream = tcpClient.GetStream();
                byte[] data = File.ReadAllBytes(path);

                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, false);
                formatter.Serialize(stream, Path.GetExtension(path));
                formatter.Serialize(stream, data);

                stream.Close();
                tcpClient.Close();
                return new OperationResult(Result.OK, "Файл " + Path.GetFileName(path) + '.' + Path.GetExtension(path) + " отправлен.");
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }
    }
}
