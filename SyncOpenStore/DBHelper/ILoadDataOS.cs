using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncOpenStore.DBHelper
{
    /// <summary>
    /// Загрузка данных с open store
    /// </summary>
    interface ILoadDataOS
    {
        
        string SQLServer { get;  }
        string FPNumber { get;  }

        void Init(string sqlServer, string FPNumber);
        void LoadDataFor_tbl_Cashier();
        void LoadDataFor_tbl_CashIO();
        void LoadDataFor_tbl_Payment();
        void LoadDataFor_tbl_SALES();
        void LoadDataFor_tbl_Operations();

    }
}
