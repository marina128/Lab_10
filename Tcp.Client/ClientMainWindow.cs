using System;
using System.Windows.Forms;
using SomeProject.Library.Client;
using SomeProject.Library;

namespace SomeProject.TcpClient
{
    public partial class ClientMainWindow : Form
    {
        public ClientMainWindow()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Отправить сообщение
        /// </summary>
        private void OnMsgBtnClick(object sender, EventArgs e)
        {
            Client client = new Client();
            Result res = client.SendMessageToServer(textBox.Text).Result;
            if(res == Result.OK)
            {
                textBox.Text = "";
                labelRes.Text = "Message was sent succefully!";
            }
            else
            {
                labelRes.Text = "Cannot send the message to the server.";
            }
            timer.Interval = 2000;
            timer.Start();
        }

        /// <summary>
        /// Очистка по истечении времени
        /// </summary>
        private void OnTimerTick(object sender, EventArgs e)
        {
            labelRes.Text = "";
            timer.Stop();
        }

        /// <summary>
        /// Отправить файл
        /// </summary>
        private void button_send_file_Click(object sender, EventArgs e)
        {
            string path = SelectFile();
            Client client = new Client();
            OperationResult res;
            if(path == null) return;
                res = client.SendFileToServer(path);
            labelRes.Text = res.Message;
            timer.Interval = 2000;
            timer.Start();
        }

        /// <summary>
        /// Выбор файла
        /// </summary>
        private string SelectFile()
        {
            using (OpenFileDialog OFileDialog = new OpenFileDialog())
            {
                if (OFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    return OFileDialog.FileName;
                }
                else
                {
                    labelRes.Text = "Неверный путь к файлу";
                    timer.Interval = 2000;
                    timer.Start();
                    return null;
                }
            }
        }
    }
}
