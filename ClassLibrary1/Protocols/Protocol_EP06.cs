using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralLib.Helper;

namespace CentralLib.Protocols
{
    class Protocol_EP06 : BaseProtocol, IProtocols
    {

        //UInt16 MaxStringLenght = 75;

        public Protocol_EP06(int serialPort):base(serialPort)
        {
            MaxStringLenght = 75;
            useCRC16 = false;
            //initial();
        }

        public Protocol_EP06(CentralLib.Connections.DefaultPortCom dComPort):base(dComPort)
        {
            MaxStringLenght = 75;
            useCRC16 = false;
        }

    }
}
