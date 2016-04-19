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




            //logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                UseShellExecute = false, // change value to false
                FileName = AppDomain.CurrentDomain.BaseDirectory + @"PrintFp.exe",
                Arguments = string.Format("-a --fp={0}", 10014193, appGuid),
                //RedirectStandardError = true,
                //RedirectStandardInput = true,
                //RedirectStandardOutput = true,
                CreateNoWindow = true,
                ErrorDialog = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas"
            };
            Process process = new Process();
            process.StartInfo = processInfo;
            //bool active = process.Start();

            ThreadStart ths = new ThreadStart(() => process.Start());
            Thread th = new Thread(ths);
            th.Start();


            //byteHelper = new ByteHelper();
            //ConsecutiveNumber = 1;

            //BaseProtocol pr = SingletonProtocol.Instance("192.168.255.132", 4016).GetProtocols();            
            //pr.setFPCplCutter(false);
            //pr.FPNullCheck();
            //pr.FPDayClrReport();
            //pr.Dispose();

            Console.WriteLine("Enter....");
                Console.ReadKey();



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
