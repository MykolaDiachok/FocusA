using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncHameleon.DBHelper
{
    class tbl_Operations
    {
        public Int32 id { get; set; }
        public Int32 NumSlave { get; set; }
        public Int32 DateTime { get; set; }
        public int Operation { get; set; }
        public bool InWork { get; set; }
        public bool Closed { get; set; }
        public DateTime CurentDateTime { get; set; }
        public bool Disable { get; set; }
    }
}
