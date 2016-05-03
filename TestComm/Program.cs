using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralLib;
using System.IO.Ports;

using System.Threading;
using CentralLib.Protocols;
using System.Net.Sockets;
using CentralLib.Helper;
using System.Net;
using System.IO;
using System.Data.Linq;
using System.Diagnostics;
using System.Management;
using NLog;

namespace TestComm
{


    class Program
    {
        //static int ConsecutiveNumber = 0;
        static ByteHelper byteHelper = new ByteHelper();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        //private static ProcessStartInfo processInfo;
        public static Guid appGuid = Guid.NewGuid();
        //private static Process process;


        static void Main(string[] args)
        {

            //20160426163835
            //4001 = 10011172 
            //4002 = 10011171 
            //4003 = 10014223 

            //byteHelper = new ByteHelper();
            //ConsecutiveNumber = 1;
            int fpnumber = 0;
            string server = "192.168.254.185";
            int port = 4002;

            //string setDataServer = SearchServer("10011171");
            BaseProtocol pr = SingletonProtocol.Instance(server, port, fpnumber).GetProtocols();

            //var dayReport = pr.dayReport;
            using (DataClassesFocusADataContext focus = new DataClassesFocusADataContext())
            {



                //var t = pr.FPDayClrReport();
                //var tmp = pr.GetMemmory(0x3000, 16, 1);


                var st = pr.status;
                var bbb = pr.dayReport;
                var ddd = pr.FPDayReport();
                //Table<tbl_ComInit> tbl_ComInit = focus.GetTable<tbl_ComInit>();
                //tbl_ComInit init = new tbl_ComInit()
                //{

                //    CompName = "FOCUS-A",
                //    Port = 0,
                //    Init = true,
                //    Error = true,
                //    WorkOff = false,
                //    auto = true,
                //    FPNumber = int.Parse(st.serialNumber),
                //    RealNumber = st.serialNumber,
                //    SerialNumber = st.serialNumber,
                //    DateTimeBegin = long.Parse(DateTime.Now.ToString("yyyyMMdd") + "000000"),
                //    DateTimeStop = long.Parse(DateTime.Now.ToString("yyyyMMdd") + "235959"),
                //    DeltaTime = -600,
                //    DataServer = "chameleonserver",//SearchServer(st.serialNumber.Trim()),
                //    DataBaseName = "chameleonserver",
                //    MinSumm = 0,
                //    MaxSumm = Int32.MaxValue,
                //    TypeEvery = false,
                //    PrintEvery = 10,
                //    MoxaIP = server,
                //    MoxaPort = port,
                //    Version = st.VersionOfSWOfECR
                //};
                //focus.tbl_ComInits.InsertOnSubmit(init);
                //focus.SubmitChanges();

                //tbl_Operation op = new tbl_Operation
                //{
                //    Operation=39,
                //    DateTime = 20160427193730,
                //    CurentDateTime = DateTime.Now,
                //    DateTimeCreate = DateTime.Now,
                //    FPNumber = int.Parse(st.serialNumber),
                //    InWork = false,
                //    Closed =false,
                //    Error=false,
                //    Disable=false
                //};
                //focus.tbl_Operations.InsertOnSubmit(op);
                //focus.SubmitChanges();
                //var dayReport = pr.dayReport;

                //pr.FPResetOrder();
                //var A = pr.FPSaleEx(1, 0, false, 2000, 0, false, "tA", 450);

                //var discount = pr.Discount(FPDiscount.AbsoluteDiscountMarkupAtIntermediateSum, 100, "Bonus");
                ////var E = pr.FPSaleEx(1, 0, false, 50000, 5, false, "tE", 464);
                ////var F = pr.FPSaleEx(1, 0, false, 60000, 6, false, "tF", 465);

                //var p3 = pr.FPPayment(3, 5000, true, true);

                //var _A = pr.FPPayMoneyEx(1, 0, false, 100, 0, false, "tA", 450);
                //var _B = pr.FPPayMoneyEx(1, 0, false, 100, 1, false, "tB", 451);
                //var _C = pr.FPPayMoneyEx(1, 0, false, 100, 2, false, "tC", 452);
                //var _D = pr.FPPayMoneyEx(1, 0, false, 100, 3, false, "tD", 453);
                ////var E = pr.FPSaleEx(1, 0, false, 50000, 5, false, "tE", 464);
                ////var F = pr.FPSaleEx(1, 0, false, 60000, 6, false, "tF", 465);
                //var _p0 = pr.FPPayment(0, 1, false, true);
                //var _p1 = pr.FPPayment(1, 2, false, true);
                //var _p2 = pr.FPPayment(2, 3, false, true);
                //var _p4 = pr.FPPayment(4, 4, false, true);
                //var _p5 = pr.FPPayment(5, 5, false, true);
                //var _p6 = pr.FPPayment(6, 6, false, true);
                //var _p7 = pr.FPPayment(7, 7, false, true);
                //var _p8 = pr.FPPayment(8, 8, false, true);
                //var _p9 = pr.FPPayment(9, 9, false, true);
                //var _p3 = pr.FPPayment(3, 500, true, true);


                ////pr.FPDayClrReport();
                //var tmp  = pr.GetMemmory(0x301D, 16, 80);
                //var tmp1 = pr.GetMemmory(0x3079, 16, 80);
                //var tmp2 = pr.GetMemmory(0x2A, 0, 2);

                ////var tmp = pr.GetMemmory()
                //var tstatus = pr.dayReport;
                //var ts = pr.getDayReport(false);
                //pr.setFPCplCutter(false);
                //pr.FPNullCheck();
                //pr.FPDayClrReport();
                pr.Dispose();

                Console.WriteLine("Enter....");
                Console.ReadKey();



            }


        }


        static string SearchServer(string fpnumber)
        {
            Dictionary<string, string> listAP = new Dictionary<string, string>()
            {
                 { "10014212","Shar_kassa2" },
                { "10013660","FON_KASSA3" },
                {"10013649","FON_KASSA1"},
                {"10013940","FON_KASSA2"},
                {"10013294","CELEN_KASSA1"},
                {"10013295","CELEN_KASSA2"},
                {"10011427","CELEN_KASSA3"},
                {"10011463","SALT_KASSA4"},
                {"10011820","SALT_KASSA5"},
                {"10011459","SALT_KASSA2"},
{"10011460","SALT_KASSA1"},
{"10013947","SALT_KASSA6"},
{"10011455","SALT_KASSA3"},
{"10011454","SALT_KASSA7"},
{"10013496","TR_KASSA2"},
{"10013495","TR_KASSA4"},
{"10013505","TR_KASSA3"},
{"10013628","TR_KASSA1"},
{"10014360","LENIN_KASSA4"},
{"10014138","LENIN_KASSA3"},
{"10014143","LENIN_KASSA2"},
{"10014141","LENIN_KASSA1"},
{"10011236","STUDION_KASSA5"},
{"10011356","STUDION_KASSA2"},
{"10010360","STUDION_KASSA3"},
{"10011352","STUDION_KASSA1"},
{"10011348","STUDION_KASSA4"},
{"10011190","23AVG_KASSA1"},
{"10011185","23AVG_KASSA4"},
{"10012838","23AVG_KASSA2"},
{"10011160","23AVG_KASSA3"},
{"10011191","AHSAROVA_KASSA2"},
{"10011018","AHSAROVA_KASSA3"},
{"10010161","AHSAROVA_KASSA1"},
{"10014142","BL23_KASSA3"},
{"10013948","BL23_KASSA2"},
{"10013677","BL23_KASSA1"},
{"10014203","MIRA_KASSA1"},
{"10014355","MIRA_KASSA4"},
{"10014356","MIRA_KASSA3"},
{"10014205","MIRA_KASSA2"},
{"10014362","BL18_KASSA4"},
{"10014363","BL18_KASSA3"},
{"10014357","BL18_KASSA2"},
{"10014361","BL18_KASSA1"},
{ "10013972","UBI_KASSA1"},
{"10013984","UBI_KASSA6"},
{"10011281","UBI_KASSA7"},
{"10013958","UBI_KASSA2"},
{"10013951","UBI_KASSA4"},
{"10013640","UBI_KASSA3"},
{"10014244","COLD_KASSA1"},
{"10014144","COLD_KASSA4"},
{"10013941","COLD_KASSA5"},
{"10014254","COLD_KASSA2"},
{"10014235","COLD_KASSA3"},
{"10013943","COLD_KASSA6"},
{"10014230","STUD_KASSA2"},
{"10014190","STUD_KASSA1"},
{"10014186","STUD_KASSA3"},
{"10010542","TOBOL_KASSA3"},
{"10013960","TOBOL_KASSA1"},
{"10013643","TOBOL_KASSA2"},
{"10013978","SHIRON_KASSA5"},
{"10013852","SHIRON_KASSA2"},
{"10013848","SHIRON_KASSA1"},
{"10013937","SHIRON_KASSA3"},
{"10013970","SHIRON_KASSA4"},
{"10014358","MINI_KASSA2"},
{"10014359","MINI_KASSA1"},
{"10014193","SHAR_KASSA1"},
{"10014191","SHAR_KASSA6"},
{"10014204","SHAR_KASSA4"},
{"10014209","SHAR_KASSA5"},
{"10014196","SHAR_KASSA3"},
{"10014197","POBEDY_KASSA2"},
{"10014211","POBEDY_KASSA3"},
{"10014198","POBEDY_KASSA1"},
{"10013966","TIT_KASSA1"},
{"10013957","TIT_KASSA2"},
{"10011826","KOSIOR_KASSA6"},
{"10014192","KOSIOR_KASSA3"},
{"10014199","KOSIOR_KASSA4"},
{"10011172","KOSIOR_KASSA2"},
{"10011171","KOSIOR_KASSA1"},


            };

            return listAP[fpnumber];
        }
    }

    public static class MyT
    {
        public static string GetCommandLine(this Process process)
        {
            //var commandLine = new StringBuilder(process.MainModule.FileName);
            var commandLine = new StringBuilder();

            commandLine.Append(" ");
            using (var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            {
                foreach (var @object in searcher.Get())
                {
                    commandLine.Append(@object["CommandLine"]);
                    commandLine.Append(" ");
                }
            }

            return commandLine.ToString();
        }
    }
}
