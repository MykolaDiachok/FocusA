using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncOpenStore.DBHelper
{
    interface ISync
    {
       
        string sFPNumber { get;  }
        string sRealNumber { get;  }
        DateTime startJob { get;  }
        DateTime stopJob { get;  }

        void StartSync();
        void StartSync(string sqlserver, string fpnumber, string RealNumber, Int64 DateTimeBegin, Int64 DateTimeStop);

        void StopSync();

    }
}
