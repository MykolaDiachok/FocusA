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
        static int ConsecutiveNumber = 0;
        static ByteHelper byteHelper = new ByteHelper();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            NLog.GlobalDiagnosticsContext.Set("FPNumber", 1);
            System.Data.SqlClient.SqlConnection sqlConnection1 = new System.Data.SqlClient.SqlConnection("Data Source=focus-a;Initial Catalog=FPWork;User ID=sa;Password=1СПредприятие82");

            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = "INSERT tbl_Log (FPNumber, Timestamp) VALUES (5, getdate())";
            cmd.Connection = sqlConnection1;

            sqlConnection1.Open();
            cmd.ExecuteNonQuery();
            sqlConnection1.Close();


            Process[] processlist = Process.GetProcesses().Where(x=>x.ProcessName.ToLower()=="PrintFp".ToLower()).ToArray();

            foreach (Process theprocess in processlist)
            {
                try
                {
                    logger.Trace("id={0}, name={1}", theprocess.Id, theprocess.ProcessName);
                    logger.Trace(theprocess.GetCommandLine());
                    logger.Trace(new string('-',50));
                }
                catch
                {

                }
            }

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
