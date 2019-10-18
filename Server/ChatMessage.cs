using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ChatMessage
    {
        public int      ID { get; set; }
        public int      Sender_ID { get; set; }
        public String   Sender_Nick { get; set; }
        public int      Receiver_ID { get; set; }
        public String   msgText { get; set; }
        /// <summary>
        /// Время приема сообщения сервером
        /// </summary>
        public DateTime Moment { get; set; }
        // СмайликИД, прикрепленные файлы и тд

        public char FIELD_SEPARATOR = '\u21ae';

        public ChatMessage()
        {
            ID = Sender_ID = Receiver_ID = 0;
            msgText = Sender_Nick = "";
            Moment = DateTime.Now;
        }

        public ChatMessage(String loadedString)
        { 
            String[] parts = loadedString.Split(FIELD_SEPARATOR);

            ID          = Convert.ToInt32(parts[0]);
            Sender_ID   = Convert.ToInt32(parts[1]);
            Sender_Nick = parts[2];
            Receiver_ID = Convert.ToInt32(parts[3]);
            msgText     = parts[4];
            Moment      = Convert.ToDateTime(parts[5]);
        }

        public String ToSendString()
        {
            return  "" + ID +
                    FIELD_SEPARATOR + Sender_ID +
                    FIELD_SEPARATOR + Sender_Nick +
                    FIELD_SEPARATOR + Receiver_ID +
                    FIELD_SEPARATOR + msgText +
                    FIELD_SEPARATOR + Moment;
        }
    }
}
