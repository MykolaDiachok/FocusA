using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralLib
{
    public class ReturnedStruct
    {
        public byte command { get; set; }
        /// <summary>
        /// Байты которые посылаем
        /// </summary>
        public byte[] bytesSend { get; set; }
        public byte[] fullBytesSend { get; set; }
        /// <summary>
        /// обработаный ответ
        /// </summary>
        public byte[] bytesReturn { get; set; }
        /// <summary>
        /// полный ответ
        /// </summary>
        public byte[] fullBytesReturn { get; set; }

        public bool statusOperation { get; set; }
        public byte ByteStatus { get; set; } // Возврат ФР статус
        public byte ByteResult { get; set; } // Возврат ФР результат
        public byte ByteReserv { get; set; } // Возврат ФР результат
        public string errorInfo { get; set; }
    }
}
