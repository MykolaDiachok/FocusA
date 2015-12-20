using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralLib
{
    public enum WorkByte:byte
    {
        //Коды служебных символов
        DLE = (byte)0x10,
        STX = (byte)0x02,
        ETX = (byte)0x03,
        ACK = (byte)0x06,
        NAK = (byte)0x15,
        SYN = (byte)0x16,
        ENQ = (byte)0x05
    }

}
