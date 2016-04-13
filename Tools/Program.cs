using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            int fpnumber = 406;
            string dataServer = "focus2";
            string dataBaseName = "CashDesk_OS";
            bool typeEvery = false;
            int printEvery = 1;
            string ip = "192.168.255.132";
            int ipPort = 4006;
            using (DataClassesFocusADataContext focusA = new DataClassesFocusADataContext())
            {
                Table<tbl_ComInit> tbl_ComInit = focusA.GetTable<tbl_ComInit>();
                tbl_ComInit init = new tbl_ComInit()
                {
                    
                    CompName="FOCUS-A",
                    Port=0,
                    Init=true,
                    Error=true,
                    WorkOff=false,
                    FPNumber = fpnumber,
                    RealNumber = fpnumber.ToString(),
                    SerialNumber = fpnumber.ToString(),
                    DateTimeBegin = 0,
                    DateTimeStop = 0,
                    DeltaTime = -180,
                    DataServer = dataServer,
                    DataBaseName = dataBaseName,
                    MinSumm = 0,
                    MaxSumm = Int32.MaxValue,
                    TypeEvery = typeEvery,
                    PrintEvery = printEvery,
                    MoxaIP = ip,
                    MoxaPort = ipPort,
                    auto=true


                };
                focusA.tbl_ComInits.InsertOnSubmit(init);
                focusA.SubmitChanges();

            }
        }
    }
}
