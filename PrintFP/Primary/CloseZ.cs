﻿using CentralLib.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintFP.Primary
{
    partial class Init
    {
        
        public void onlyZReport(Protocol_EP11 pr)
        {
            pr.FPDayClrReport();
        }

        public void StartJob(Protocol_EP11 pr)
        {
            pr.FPDayReport();
        }

    }
}
