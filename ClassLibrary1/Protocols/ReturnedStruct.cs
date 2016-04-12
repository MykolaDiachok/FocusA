using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralLib.Protocols
{
    public struct ReturnedStruct
    {
        public byte command { get; set; }
        public byte[] bytesSend { get; set; }
        public byte[] bytesReturn { get; set; }
        
        public bool statusOperation { get; set; }
        public byte ByteStatus { get; set; } // Возврат ФР статус
        public byte ByteResult { get; set; } // Возврат ФР результат
        public byte ByteReserv { get; set; } // Возврат ФР результат
    }
}
