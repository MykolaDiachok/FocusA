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

            //4008 = 10013268
            //4003 = 10010014
            //4004 = 10011738

            //byteHelper = new ByteHelper();
            //ConsecutiveNumber = 1;
            int fpnumber = 0;
            BaseProtocol pr = SingletonProtocol.Instance("192.168.255.132", 4016, fpnumber).GetProtocols();

            //var dayReport = pr.dayReport;
            using (DataClassesFocusADataContext focus = new DataClassesFocusADataContext())
            {
                var t = pr.FPDayClrReport();
                var tmp = pr.GetMemmory(0x3000, 16, 1);
                var st = pr.status;
                var dayReport = pr.dayReport;
               
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
