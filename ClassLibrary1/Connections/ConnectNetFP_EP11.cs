using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralLib.Connections
{
    public class ConnectNetFP_EP11 : ConnectNetFactory
    {
        public ConnectNetFP_EP11(string IpAdress, int port, int inFPnumber) :base(IpAdress, port, 400, inFPnumber)
        {

            base.useCRC16 = true;
        }
    }
}
