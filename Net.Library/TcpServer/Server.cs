using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SomeProject.Library.Server
{
    public class Server
    {
        // <summary>
        /// Единый путь для всех файлов
        /// </summary>
        const string CommonPath = "C:\\Users\\Marina\\Downloads\\prog\\Клиент-серверное приложение\\ПРИМЕР\\Tcp.Server\\";

        /// <summary>
        /// Количество полученных файлов
        /// </summary>
        int file_cnt = 0;

        /// <summary>
        /// Текущее количество подключений
        /// </summary>
        int cur_cnt = 0;

        /// <summary>
        /// Максимальное количество подключений
        /// </summary>
        int max_cnt = 0;
        TcpListener serverListener;

        public Server(int max_cnt)
        {
            this.max_cnt = max_cnt;
            serverListener = new TcpListener(IPAddress.Loopback, 8080);
        }

        /// <summary>
        /// Выбор обработчика данных
        /// </summary>
        private async Task<OperationResult> chooseOperation()
        {
            try
            {
                if (cur_cnt >= max_cnt) return new OperationResult(Result.Fail, "Server is busy. Try to connect later.");
                TcpClient client = serverListener.AcceptTcpClient();
                Interlocked.Increment(ref cur_cnt);
                NetworkStream stream = client.GetStream();

                BinaryFormatter formatter = new BinaryFormatter();
                bool operation = (bool)formatter.Deserialize(stream);

                if (operation)
                {
                    OperationResult result = await ReceiveMessageFromClient(stream);
                    if (result.Result == Result.Fail)
                        Console.WriteLine("Unexpected error: " + result.Message);
                    else
                        Console.WriteLine("New message from client: " + result.Message);
                }
                else
                {
                    OperationResult result = await ReceiveFileFromClient(stream);
                    if (result.Result == Result.Fail)
                        Console.WriteLine("Unexpected error: " + result.Message);
                    else
                        Console.WriteLine("New file recieved from client: " + result.Message);
                }

                stream.Close();
                client.Close();
                Interlocked.Decrement(ref cur_cnt);

                return new OperationResult(Result.OK, "");
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }
        /// <summary>
        /// Выключить Listener
        /// </summary>
        public bool TurnOffListener()
        {
            try
            {
                if (serverListener != null)
                    serverListener.Stop();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot turn off listener: " + e.Message);
                return false;
            }
        }
        /// <summary>
        /// Включить Listener
        /// </summary>
        public async Task TurnOnListener()
        {
            try
            {
                if (serverListener != null)
                    serverListener.Start();
                while (true)
                {
                    OperationResult result = await chooseOperation();
                    if (result.Result == Result.Fail)
                        Console.WriteLine("Unexpected error: " + result.Message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot turn on listener: " + e.Message);
            }
        }

        /// <summary>
        /// Принять сообщение на сервер
        /// </summary>
        /// <param name="stream">Поток</param>
        public async Task<OperationResult> ReceiveMessageFromClient(NetworkStream stream)
        {
            try
            {
                Console.WriteLine("Waiting for connections...");
                StringBuilder recievedMessage = new StringBuilder();
                byte[] data = new byte[256];

                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    recievedMessage.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);
                stream.Close();

                return new OperationResult(Result.OK, recievedMessage.ToString());
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }

        }

        /// <summary>
        /// Загрузить файл на сервер
        /// </summary>
        /// <param name="stream">Поток</param>
        public async Task<OperationResult> ReceiveFileFromClient(NetworkStream stream)
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                string extension = (string)formatter.Deserialize(stream);
                byte[] data = (byte[])formatter.Deserialize(stream);
                string newPath = GetNewPath(extension);
                File.WriteAllBytes(newPath, data);
                Interlocked.Increment(ref file_cnt);

                stream.Close();

                return new OperationResult(Result.OK, Path.GetFileName(newPath));
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }

        /// <summary>
        /// Выдать файлу имя
        /// </summary>
        /// <param name="extension">Расширение файла</param>
        private string GetNewPath(string extension)
        {
            string path = CommonPath + DateTime.Today.Year.ToString() + "-"
            + DateTime.Today.Month.ToString() + "-"
            + DateTime.Today.Day.ToString();

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path + "\\" + file_cnt + extension;
        }
        /// <summary>
        ///  Отправить сообщение клиенту
        /// </summary>
        /// <param name="message">Отправляемое сообщение</param>
        public OperationResult SendMessageToClient(string message)
        {
            try
            {
                TcpClient client = serverListener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();

                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);

                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
            return new OperationResult(Result.OK, "");
        }
    }
}