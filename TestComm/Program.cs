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

namespace TestComm
{


    class Program
    {
        static int ConsecutiveNumber = 0;
        static ByteHelper byteHelper = new ByteHelper();

        static void Main(string[] args)
        {

            Process[] processlist = Process.GetProcesses().Where(x=>x.ProcessName.ToLower()=="PrintFp".ToLower()).ToArray();

            foreach (Process theprocess in processlist)
            {
                try
                {
                    Console.WriteLine("id={0}, name={1}", theprocess.Id, theprocess.ProcessName);
                    Console.WriteLine(theprocess.GetCommandLine());
                    Console.WriteLine(new string('-',50));
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
