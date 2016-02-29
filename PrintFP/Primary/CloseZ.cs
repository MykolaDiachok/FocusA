using CentralLib.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintFP.Primary
{
    partial class Init
    {
        
        public void onlyZReport(Protocols pr)
        {
            pr.FPDayClrReport();
        }

        public void StartJob(Protocols pr)
        {
            pr.FPDayReport();
        }

    }
}
