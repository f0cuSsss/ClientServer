using System;
using System.Net;
using System.Text;

namespace Server
{   
        /* Запросы/Ответы сервера */
    public class Configs
    {
            /* Команды сервера */
        public const String AUTH_CMD    = "auth"; // Команда авторизации ( проверки ника )
        public const String MSG_CMD     = "msg"; // Команда сообщения ( проверки ника )
        public const String LOGOUT_CMD  = "logout"; // Команда выйти
        public const String SYNC_CMD    = "sync"; // Команда синхронизации - получение последних сообщений

            /* Разделители */
        public const char CMD_SEPARATOR = '~'; // Разделитель команд и данных
        public const char ARG_SEPARATOR = '\u21AD'; // Разделитель аргументов (внутри данных)
        public const char MSG_SEPARATOR = '\u20AE'; // Разделитель сообщений синхронизации

            /* Статусы работы команд */
        public const String STATUS_LOGIN_FREE   = "Логин свободен!";
        public const String STATUS_LOGIN_USED   = "Логин уже используется!";

        public const String STATUS_LOGOUT_OK    = "Вы успешно вышли с чата";
        public const String STATUS_LOGOUT_FAIL  = "Выход с чата провален";

        public const String STATUS_SYNC_OK      = "Синхронизировано успешно";
        public const String STATUS_SYNC_NOTHING = "Синхронизировать нечего";
        public const String STATUS_SYNC_FAIL    = "Ошбика синхронизации";
    }

        /* Настройки сервера */
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
