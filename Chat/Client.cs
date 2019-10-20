using System;
using System.Drawing;
using System.Net.Sockets;
using System.Windows.Forms;
using Server;

namespace Chat
{
    public partial class Client : Form
    {

        Socket clientSocket;
        byte[] buf;
        String str, msg;
        /* ID пользователя после регистриции */
        private int user_ID;
        private String userName;

        //private const String AUTH_CMD = "auth"; // Команда авторизации ( проверки ника )
        //private const String MSG_CMD = "msg"; // Команда сообщения
        //private const String LOGOUT_CMD = "logout"; // Команда выйти
        //private const String Sync_CMD = "syncronization";

        //private const char  CMD_SEPARATOR   = '~'; // Разделитель команд и данных
        //private const char  ARG_SEPARATOR   = '\u21AD'; // Разделитель аргументов (внутри данных)
        //public char         FIELD_SEPARATOR = '\u21ae';

        ////private const String STATUS_LOGIN_FREE = "Логин свободен!";
        ////private const String STATUS_LOGIN_USED = "Логин уже используется!";
        ////private const String STATUS_LOGOUT_OK = "Вы успешно вышли с чата";
        ////private const String STATUS_LOGOUT_FAIL = "Выход с чата провален";

        //private const String STATUS_LOGIN_FREE = "free";
        //private const String STATUS_LOGIN_USED = "used";

        //private const String STATUS_LOGOUT_OK = "logoutOK";
        //private const String STATUS_LOGOUT_FAIL = "logoutFAIL";


        public Client()
        {
            InitializeComponent();
            user_ID = -1;
            userName = String.Empty;
            tbNickname.ForeColor = Color.Red;
        }

        private void Client_Load(object sender, EventArgs e)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            // (сервер должен быть запущен и слушать порт)
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (tbMessage.Text.Equals(String.Empty))
            {
                MessageBox.Show("Введите сообщение!");
                return;
            }

            if (user_ID.Equals(0))
            {
                MessageBox.Show("Войдите в систему");
                return;
            }

            

            // Формируем команду отправки сообщения
            String msg =
                          Configs.MSG_CMD + Configs.CMD_SEPARATOR
                        + userName + Configs.ARG_SEPARATOR + tbMessage.Text
                        + Configs.ARG_SEPARATOR + user_ID.ToString();

            // Обмениваемся данными с сервером
            String serverAnswer = null;
            try
            {
                serverAnswer = Exchange(msg);
            }
            catch (Exception ex)
            {
                MessageLog.Items.Add("Exception: " + ex.Message); return;
            }



            // Анализируем ответ от сервера
            Server.ChatMessage message = null;

            try
            {
                message = new Server.ChatMessage(serverAnswer);
            }
            catch (Exception ex)
            {
                MessageLog.Items.Add(ex.Message);
                return;
            }


            MessageLog.Items.Add(
                $"\t{message.msgText} | [{message.Moment.ToShortTimeString()}]");
            tbMessage.Clear();

        }

        private void btnSignIN_Click(object sender, EventArgs e)
        {
            if (tbNickname.Text.Equals(String.Empty))
            {
                MessageBox.Show("Enter nickname!");
                return;
            }
            
                /* Формируем команду на авторизацию */
            String msg = Configs.AUTH_CMD + Configs.CMD_SEPARATOR + tbNickname.Text;

                /* Обмениваемся данными с сервером */
            String serverAnswer = null;
            try
            {
                serverAnswer = Exchange(msg);
            }
            catch (Exception ex)
            {
                MessageLog.Items.Add("Exception: " + ex.Message); return;
            }


                /* Анализируем ответ сервера */
            String[] parts = serverAnswer.Split(Configs.CMD_SEPARATOR);
            if (Configs.STATUS_LOGIN_FREE.Equals(parts[0]))
            {
                user_ID = int.Parse(parts[1]);
                userName = tbNickname.Text;
                MessageLog.Items.Add($"Ник зарегестрирован с ID = {user_ID}");
                tbNickname.ForeColor = Color.Green;
                btnLogout.Visible = true;
            }
            else if (Configs.STATUS_LOGIN_USED.Equals(parts[0]))
            {
                MessageLog.Items.Add("Пользователь с таким ником уже зарегестрирован");
            }
            else
            {
                MessageLog.Items.Add($"Ответ сервера не распознан {serverAnswer}");
            }
            timerSync.Enabled = true;
           // btnSignIN.Enabled = false;
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            btnLogout.Visible = false;
            tbNickname.ForeColor = Color.Red;
            btnSignIN.Enabled = true;

            // Создаем 

            String msg =
              Configs.LOGOUT_CMD + Configs.CMD_SEPARATOR
                + user_ID + Configs.ARG_SEPARATOR + userName;

                /* Обмениваемся данными с сервером */
            String serverAnswer = null;
            try
            {
                serverAnswer = Exchange(msg);
            }
            catch (Exception ex)
            {
                MessageLog.Items.Add("Exception: " + ex.Message); return;
            }

                /* Анализируем ответ от сервера */
            String[] parts = serverAnswer.Split(Configs.CMD_SEPARATOR);
            if (Configs.STATUS_LOGOUT_OK.Equals(parts[0]))
            {
                user_ID = 0;
                MessageLog.Items.Add($"Пользователь {userName} отключился.");
                userName = String.Empty;
            }
            else if (Configs.STATUS_LOGOUT_FAIL.Equals(parts[0]))
            {
                MessageLog.Items.Add("Возникла ошибка при отключении");
            }
            else
            {
                MessageLog.Items.Add($"Ответ сервера не распознан {serverAnswer}");
            }

            timerSync.Enabled = false;
        }

        private void btnSync_Click(object sender, EventArgs e)
        {
            startSync();
        }

        private void timerSync_Tick(object sender, EventArgs e)
        {
            //startSync();
        }

        private void startSync()
        {
            String msg =
              Configs.SYNC_CMD + Configs.CMD_SEPARATOR
                + user_ID;

            /* Обмениваемся данными с сервером */
            String serverAnswer = null;
            try
            {
                serverAnswer = Exchange(msg);
            }
            catch (Exception ex)
            {
                MessageLog.Items.Add("Exception: " + ex.Message); return;
            }



            /* Анализируем ответ от сервера */
            String[] parts = serverAnswer.Split(Configs.CMD_SEPARATOR);
            if (Configs.STATUS_SYNC_FAIL.Equals(parts[0]))
            {
                //MessageLog.Items.Add($"{parts[1]}");
            }
            else if (Configs.STATUS_SYNC_OK.Equals(parts[0]))
            {
                String[] args = null;
                try
                {
                    args = parts[1].Split(Configs.MSG_SEPARATOR);
                    foreach (var item in args)
                    {
                        if (!item.Equals(String.Empty))
                        {
                            MessageLog.Items.Add($"{item}");
                        }
                    }

                }
                catch
                {
                    //throw;
                    return;
                }
            }
            else
            {
                MessageLog.Items.Add($"Ответ сервера не распознан {serverAnswer}");
            }
        }

        /// <summary>
        /// Отправка запроса на сервер и получение ответа
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private String Exchange(String msg)
        {
            clientSocket = null; // Переменная для цикла сообщений
            buf = new byte[256]; // буфер для обмена
            str = String.Empty; // Перевод буфера в строку

            try
            {
                Ini.setSettings(tbIP.Text, Convert.ToInt32(tbPort.Text), tbEnc.Text.ToString());
            }
            catch
            {
                throw new Exception("Некоректная настройка");
            }

            // Переводим сообщение в байты 
            buf = Ini.communicationEncoding.GetBytes(msg);
            try
            { // Подключаемся к пункту назначения - код как у сервера
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Соединяемся...
                clientSocket.Connect(Ini.endPoint);
                // Отправляем сообщение
                clientSocket.Send(buf);
                // Получаем ответ и переводим в строку
                str = "";
                do
                {
                    int n = clientSocket.Receive(buf);
                    str += Ini.communicationEncoding.GetString(buf, 0, n);
                } while (clientSocket.Available > 0);


                // Закрываем сокет
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return str;
        }
    }
}




