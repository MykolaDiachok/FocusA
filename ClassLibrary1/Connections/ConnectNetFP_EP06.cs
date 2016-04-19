using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralLib.Connections
{
    class ConnectNetFP_EP06 :ConnectNetFactory
    {
        public ConnectNetFP_EP06(string IpAdress, int port, int inFPnumber) :base(IpAdress, port, 400, inFPnumber)
        {
            
            base.useCRC16 = false;
        }
    }
}
