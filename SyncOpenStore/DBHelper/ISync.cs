using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncOpenStore.DBHelper
{
    interface ISync
    {
        string SQLServer { get; }
        string FPNumber { get;  }
        DateTime startJob { get;  }
        DateTime stopJob { get;  }

        void StartSync();
        void StartSync(string sqlserver, string fpnumber);

        void StopSync();

    }
}
