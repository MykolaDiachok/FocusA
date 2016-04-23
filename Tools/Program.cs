using NDesk.Options;
using NLog;
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
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            bool showHelp = false;
            bool automatic = false;
            string fpnumber="";
            int setFPnumber=0;
            int setRealNumber = 0;
            string setDataServer = "";
            string setDataBaseName = "";

            var os = new OptionSet()
               .Add("fp|fpnumber=", "Set fpnumber", fp => fpnumber = fp)
               .Add("rn|realnumber=", "Set real number", rn => setRealNumber = int.Parse(rn))
               .Add("a|auto", "automatic service mode", a => automatic = a != null)
               .Add("sr|server=", "set server name, if not set - system will take current system server name", s => setDataServer = s)
               .Add("db|database=", "set data base name", db => setDataBaseName = db)
               .Add("?|h|help", "show help", h => showHelp = h != null);
            try
            {
                var p = os.Parse(args);
                if (showHelp)
                    DisplayHelp(os);
                setFPnumber = int.Parse(fpnumber);
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                logger.Info("Try '--help' for more information.");
                DisplayHelp(os);
                return;
            }


            
            
            
            
            bool typeEvery = false;
            int printEvery = 10;
            string ip = "192.168.255.132";
            int ipPort = 4008;
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
                    auto = automatic,
                    FPNumber = setFPnumber,
                    RealNumber = setRealNumber.ToString(),
                    SerialNumber = setRealNumber.ToString(),
                    DateTimeBegin = long.Parse(DateTime.Now.ToString("yyyyMMdd")+"000000"),
                    DateTimeStop = long.Parse(DateTime.Now.ToString("yyyyMMdd") + "235959"),
                    DeltaTime = -600,
                    DataServer = setDataServer,
                    DataBaseName = setDataBaseName,
                    MinSumm = 0,
                    MaxSumm = Int32.MaxValue,
                    TypeEvery = typeEvery,
                    PrintEvery = printEvery,
                    MoxaIP = ip,
                    MoxaPort = ipPort,
                    


                };
                focusA.tbl_ComInits.InsertOnSubmit(init);
                focusA.SubmitChanges();

            }
        }

        private static void DisplayHelp(OptionSet p)
        {
            Console.WriteLine("Help for add FP");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

    }
}
