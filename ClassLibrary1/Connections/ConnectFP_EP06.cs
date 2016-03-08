using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralLib.Connections
{
    class ConnectFP_EP06 : ConnectFactory
    {
        public ConnectFP_EP06(DefaultPortCom defPortCom):base(defPortCom)
        {
            base.useCRC16 = false;
        }

        
    }
}
