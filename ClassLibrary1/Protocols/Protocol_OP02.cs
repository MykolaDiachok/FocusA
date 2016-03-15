using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralLib.Protocols
{
    public class Protocol_OP02 :BaseProtocol, IProtocols
    {
        

        public Protocol_OP02(int serialPort):base(serialPort)
        {
            MaxStringLenght = 50;
            useCRC16 = false;
            //initial();
        }

        public Protocol_OP02(CentralLib.Connections.DefaultPortCom dComPort):base(dComPort)
        {
            MaxStringLenght = 50;
            useCRC16 = false;
        }

        public Protocol_OP02(string IpAdress, int port):base(IpAdress,port)
        {
            MaxStringLenght = 50;
            useCRC16 = false;
        }
    }
}
