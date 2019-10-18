using System;
using System.Net;
using System.Text;

namespace Server
{   
    public class Configs
    {
        public const String AUTH_CMD = "auth"; // Команда авторизации ( проверки ника )
        public const String MSG_CMD = "msg"; // Команда сообщения ( проверки ника )
        public const String LOGOUT_CMD = "logout"; // Команда выйти
        public const String SYNC_CMD = "sync"; // Команда синхронизации - получение последних сообщений

        public const char CMD_SEPARATOR = '~'; // Разделитель команд и данных
        public const char ARG_SEPARATOR = '\u21AD'; // Разделитель аргументов (внутри данных)

        public const String STATUS_LOGIN_FREE = "Логин свободен!";
        public const String STATUS_LOGIN_USED = "Логин уже используется!";
        public const String STATUS_LOGOUT_OK = "Вы успешно вышли с чата";
        public const String STATUS_LOGOUT_FAIL = "Выход с чата провален";
    }

    public class Ini
    {
        public static string host;
        public static IPAddress IP;
        public static int port;
        public static System.Text.Encoding communicationEncoding;
        public static IPEndPoint endPoint;

        public static void setSettings(String _IP, int _port, String _enc)
        {
            host = _IP;
            IP = IPAddress.Parse(host);
            port = _port;
            if (_enc.Equals("Unicode"))
            {
                communicationEncoding = Encoding.Unicode;
            }
            endPoint = new IPEndPoint(IP, port);
        }
    }
}
