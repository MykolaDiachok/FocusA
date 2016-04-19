using CentralLib.Protocols;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintFP.Primary
{
    public partial class Init
    {
        

        public void onlyZReport(BaseProtocol pr)
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            pr.FPDayClrReport();
        }

        public void StartJob(BaseProtocol pr)
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            pr.FPDayReport();
        }

    }
}
