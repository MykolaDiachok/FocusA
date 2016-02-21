using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncHameleon.DBHelper
{
    class tbl_Payment
    {
        public Int32 id { get; set; }
        public Int32 NumOperation { get; set; }
        public Int32 DATETIME { get; set; }
        public int FPNumber { get; set; }
        public int SESSID { get; set; }
        public int SYSTEMID { get; set; }
        public int SAREAID { get; set; }        
        public int Type { get; set; }
        public string FRECNUM { get; set; }
        public int SRECNUM { get; set; }
        public int Payment_Status { get; set; }
        public int Payment { get; set; }
        public int Payment0 { get; set; }
        public int Payment1 { get; set; }
        public int Payment2 { get; set; }
        public int Payment3 { get; set; }
        public int Payment4 { get; set; }
        public int Payment5 { get; set; }
        public int Payment6 { get; set; }
        public int Payment7 { get; set; }
        public bool CheckClose { get; set; }
        public bool FiscStatus { get; set; }
        public string CommentUp { get; set; }
        public string Comment { get; set; }
        public int Old_Payment { get; set; }
        public int FPSumm { get; set; }
        public int PayBonus { get; set; }
        public int BousInAcc { get; set; }
        public int BonusCalc { get; set; }
        public Int64 Card { get; set; }
        public bool ForWork { get; set; }
        public int RowCount { get; set; }
        public bool Disable { get; set; }

    }
}
