using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralLib.Helper;

namespace CentralLib.Protocols
{
    public class Protocol_EP06 : BaseProtocol, IProtocols
    {

        //UInt16 MaxStringLenght = 75;

        public Protocol_EP06(int serialPort, int inFPNumber):base(serialPort, inFPNumber)
        {
            MaxStringLenght = 75;
            useCRC16 = false;
            //initial();
        }

        public Protocol_EP06(CentralLib.Connections.DefaultPortCom dComPort,int inFPNumber) :base(dComPort, inFPNumber)
        {
            MaxStringLenght = 75;
            useCRC16 = false;
        }

        public Protocol_EP06(string IpAdress, int port, int inFPNumber):base(IpAdress,port, inFPNumber)
        {
            MaxStringLenght = 75;
            useCRC16 = false;
        }


      

    }
}
