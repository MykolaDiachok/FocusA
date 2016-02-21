using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncHameleon.DBHelper
{
    class tbl_ComInit
    {
        public string CompName { get; set; }
        public int Port { get; set; }
        public bool Init { get; set; }
        public bool Error { get; set; }
        public int FPNumber { get; set; }
        public string RealNumber { get; set; }
        public Int64 DateTimeBegin { get; set; }
        public Int64 DateTimeStop { get; set; }
        public Int64 DeltaTime { get; set; }
        public string DataServer { get; set; }
        public string DataBaseName { get; set; }
        public int MinSumm { get; set; }
        public int MaxSumm { get; set; }
        public bool TypeEvery { get; set; }
        public int PrintEvery { get; set; }
    }
}
