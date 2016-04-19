using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralLib.Protocols
{
    public class Protocol_OP02 :BaseProtocol, IProtocols
    {
        

        public Protocol_OP02(int serialPort, int inFPNumber) :base(serialPort, inFPNumber)
        {
            MaxStringLenght = 50;
            useCRC16 = false;
            
            //initial();
        }

        public Protocol_OP02(CentralLib.Connections.DefaultPortCom dComPort, int inFPNumber) :base(dComPort, inFPNumber)
        {
            MaxStringLenght = 50;
            useCRC16 = false;
        }

        public Protocol_OP02(string IpAdress, int port, int inFPNumber) :base(IpAdress,port, inFPNumber)
        {
            MaxStringLenght = 50;
            useCRC16 = false;
        }
    }
}
