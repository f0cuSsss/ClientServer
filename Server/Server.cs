using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

// ORM

namespace Server
{
    public partial class Server : Form
    {
        Socket serverSocket;
        Thread serverThread = null;

        List<ChatMessage> messages;
        List<User> users;


        //private const String AUTH_CMD = "auth"; // Команда авторизации ( проверки ника )
        //private const String MSG_CMD = "msg"; // Команда сообщения ( проверки ника )
        //private const String LOGOUT_CMD = "logout"; // Команда выйти

        //private const char CMD_SEPARATOR = '~'; // Разделитель команд и данных
        //private const char ARG_SEPARATOR = '\u21AD'; // Разделитель аргументов (внутри данных)

        //private const String STATUS_LOGIN_FREE = "Логин свободен!";
        //private const String STATUS_LOGIN_USED = "Логин уже используется!";
        //private const String STATUS_LOGOUT_OK = "Вы успешно вышли с чата";
        //private const String STATUS_LOGOUT_FAIL = "Выход с чата провален";

        public Server()
        {
            InitializeComponent();
            messages = new List<ChatMessage>();
            users = new List<User>();

        }

        void StartServer()
        {
            Ini.setSettings(tbIP.Text, Convert.ToInt32(tbPort.Text), tbEnc.Text.ToString());

            try
            {
                serverSocket = new Socket( // Заготавливаем TCP сокет
                                        AddressFamily.InterNetwork, // IPv4 адрессация)
                                        SocketType.Stream, // потоковый двухсторонний сокет
                                        ProtocolType.Tcp);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                lbStatus.Text = "ON";
                lbStatus.ForeColor = Color.Green;
                btnStart.Text = "Stop";
            }

            Socket request = null; // сокет для установленного соединения
            try
            {
                serverSocket.Bind(Ini.endPoint); // привязываем сокет к пункту назначения
                serverSocket.Listen(100); // случаем порт, допускаем очередь из 100 запросов, на 101 получит смс - сервер занят
                ServerLog.Items.Add($"[{DateTime.Now.ToString()}] Сервер успешно запущен...");

                byte[] buf = new byte[256]; // буфер для считаных данных
                string str; // строка для перевода байт в символы
                int n; // кол-во сиволов в буфере
                while (true) // Постоянно считываем данные с порта
                {
                    // Ожидаем запрос и создаем сокет для соединения - обработки запроса
                    request = serverSocket.Accept(); // зависание потока до прихода запроса от
                    str = "";
                    // начинаем прием данных
                    do
                    {
                        n = request.Receive(buf); // получаем данные в байт-буфер
                        // Переводим байты в символы и дописываем к строке. Чтобы не зачищать байт-буфер указываем кол-во принятых байт n
                        str += Ini.communicationEncoding.GetString(buf, 0, n);
                    } while (request.Available > 0); // пока есть доступные байты

                    // Выводим данные о полученном запросе
                    String receiveTime = DateTime.Now.ToString();
                    //ServerLog.Items.Add($"[{receiveTime}]: {str}");

                    // в str - сообщение от клиента
                    // Разделяем на команду и данные
                    String[] parts = str.Split(Configs.CMD_SEPARATOR);
                    String msg = String.Empty;
                    switch (parts[0])
                    {
                        case Configs.AUTH_CMD:
                            {
                                String SRVitem = String.Empty;
                                if (users.Any(u => u.Name == $"{parts[1]}"))
                                {
                                    msg = SRVitem = Configs.STATUS_LOGIN_USED;
                                }
                                else
                                {
                                    int newID = (users.Count > 0) ? users[users.Count - 1].ID + 1 : 1;
                                    users.Add(new User()
                                    {
                                        ID = newID, // TODO: Изменяющиеся ИД
                                        Name = parts[1]
                                    });
                                    msg = Configs.STATUS_LOGIN_FREE + Configs.CMD_SEPARATOR + newID;
                                    SRVitem = $"[{receiveTime}]: Пользователь {parts[1]} авторизовался"; // TODO: Переписать! Если ИД присвоено - то пользователь авторизовался
                                }
                                ServerLog.Items.Add(SRVitem.ToString());

                                /* TODO: Добавить сообщение, что пользователь вошел на сервер */
                            }
                            break;
                        case Configs.MSG_CMD:
                            {
                                String[] args = parts[1].Split(Configs.ARG_SEPARATOR);
                                String author, message;
                                int user_ID = 0;

                                /* Проверяем на пустоту args(может прийти без сепаратора) */
                                try
                                {
                                    author = args[0];
                                    message = args[1];
                                    user_ID = Convert.ToInt32(args[2]);
                                }
                                catch
                                {
                                    msg = "Сообщение не распознано" + str;
                                    break;
                                }

                                if (user_ID.Equals(0))
                                {
                                    ServerLog.Items.Add("Проникновене в систему!!!");
                                    msg = "Проникновене в систему!!!";
                                    break;
                                }

                                /* Добавляем сообщение в list */
                                ChatMessage newMSG = new ChatMessage()
                                {
                                    ID = (messages.Count > 0) ? messages[messages.Count - 1].ID + 1 : 1,
                                    Sender_ID = user_ID,
                                    Sender_Nick = author,
                                    Receiver_ID = -1,
                                    msgText = message,

                                    Moment = DateTime.Now
                                };


                                messages.Add(newMSG);
                                msg = newMSG.ToSendString();
                                //msg = author + ": " + message;
                                ServerLog.Items.Add($"[{receiveTime}]: ({args[0]}): {args[1]}");
                            }
                            break;
                        case Configs.LOGOUT_CMD:
                            {

                                String[] args = parts[1].Split(Configs.ARG_SEPARATOR);
                                int user_ID = -1;

                                // Безопасная проверка ответа от сервера
                                try
                                {
                                    user_ID = Convert.ToInt32(args[0]);
                                }
                                catch
                                {
                                    msg = Configs.STATUS_LOGOUT_FAIL + Configs.CMD_SEPARATOR + "Сообщение не распознано " + str;
                                    break;
                                }

                                ServerLog.Items.Add($"Пользователь {args[1]} покинул нас!");
                                // Удаления пользователя user_ID из списка
                                users.RemoveAll(u => u.ID == user_ID);

                                // Подготовка запроса к отправке на сервер
                                msg = Configs.STATUS_LOGOUT_OK + Configs.CMD_SEPARATOR + user_ID;

                            }
                            break;
                        case Configs.SYNC_CMD:
                            {
                                int user_ID = -1;
                                try // Безопасная проверка ответа от сервера
                                {
                                    user_ID = Convert.ToInt32(parts[1]);
                                }
                                catch
                                {
                                    msg = Configs.STATUS_SYNC_FAIL + Configs.CMD_SEPARATOR + "Не распознано: " + str;
                                    break;
                                }

                                User curUser = null;
                                String mes = String.Empty;
                                String fullMessagesSync = String.Empty;

                                try
                                {
                                        /* Пользователь, который запросил синхронизацию */
                                    curUser = users.Where(u => u.ID == user_ID).First();

                                    IEnumerable<ChatMessage> messages = MessagesGroup(user_ID);
                                    if (!messages.Count().Equals(0))
                                    {
                                        foreach (ChatMessage chatMessage in messages)
                                        {
                                            if (curUser.Sync < chatMessage.Moment)
                                            {
                                                mes = $"{chatMessage.Sender_Nick}: {chatMessage.msgText} [{chatMessage.Moment.ToShortTimeString()}]";
                                                fullMessagesSync += mes + Configs.MSG_SEPARATOR;
                                            }
                                        }
                                        curUser.Sync = DateTime.Now;
                                    }
                                    else
                                    {
                                        ServerLog.Items.Add($"Нечего синхронизировать");
                                        msg = Configs.STATUS_SYNC_NOTHING;
                                        break;
                                    }
                                }
                                catch
                                {
                                    msg = Configs.STATUS_SYNC_FAIL + Configs.CMD_SEPARATOR + "Ошибка!";
                                    break;
                                }

                                msg = Configs.STATUS_SYNC_OK + Configs.CMD_SEPARATOR + fullMessagesSync;
                                ServerLog.Items.Add($"Пользователь {curUser.Name} успешно синхронизировался!");
                            }
                            break;
                        default:
                                /* Ответ сервера по-умолчанию */
                            msg = $"[{receiveTime}]: Сообщение отправлено";
                            break;
                    }


                    // Переводим в байты
                    buf = Ini.communicationEncoding.GetBytes(msg);
                    // Отправляем ответ
                    request.Send(buf);
                    // Закрываем сокет
                    request.Shutdown(SocketShutdown.Both);
                    request.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
                throw new Exception(ex.Message);
            }
        }

        //private IEnumerable<ChatMessage> MessagesGroup()
        //{
        //    return messages.Where(msg => msg.Receiver_ID == -1); 
        //}

        private IEnumerable<ChatMessage> MessagesGroup(int _userID)
        {
            return messages.Where(msg => msg.Receiver_ID == -1 && msg.Sender_ID != _userID);
        }

        private IEnumerable<ChatMessage> MessageToUser(int _userID)
        {
            return messages.Where(msg => msg.Receiver_ID == _userID);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {

            if (serverThread != null) // Обработка кнопки STOP
            {
                serverSocket.Close();
                serverThread = null;
                lbStatus.Text = "OFF";
                btnStart.Text = "Start";
                lbStatus.ForeColor = Color.Red;
                return;
            }

            // Обработка конпки Start

            if (tbIP.Equals(String.Empty))
            {
                MessageBox.Show("Введите IP");
                return;
            }

            if (tbPort.Equals(String.Empty))
            {
                MessageBox.Show("Введите Port");
                return;
            }

            if (tbEnc.Equals(String.Empty))
            {
                MessageBox.Show("Введите Encoding");
                return;
            }

            if (serverThread == null)
            {
                serverThread = new Thread(StartServer);
            }
            serverThread.Start();

        }

        private void Server_FormClosing(object sender, FormClosingEventArgs e)
        {
            serverSocket?.Close();
        }
    }
}


