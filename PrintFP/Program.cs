using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;
using System.Threading.Tasks;
using NDesk.Options;
using NLog;
using NLog.Config;
using CentralLib;
using CentralLib.Protocols;
using PrintFP.Primary;

namespace PrintFP
{
    class Program
    {
        private static string fpnumber, server;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static DateTime startJob;

        static int Main(string[] args)
        {
            //startJob = DateTime.Now;
#if (!DEBUG)
                logger.Info("Enable NBug");
                AppDomain.CurrentDomain.UnhandledException += Handler.UnhandledException;
                TaskScheduler.UnobservedTaskException += Handler.UnobservedTaskException;
#endif
            new OptionSet()
               .Add("fp=|fpnumber=", fp => fpnumber = fp)
               .Add("s=|server=", s => server = s)
               .Add("?|h|help", h => DisplayHelp())
               .Parse(args);

            if (String.IsNullOrEmpty(fpnumber))
            {
                Console.WriteLine("Need arg -fp=\"fpnumber\"");
                return (int)infoPr.Bad;
            }
            if (String.IsNullOrEmpty(server))
            {
                server = System.Environment.MachineName;
#if (DEBUG)
                    server = "focus-A";
#endif
            }
            NLog.GlobalDiagnosticsContext.Set("FPNumber", fpnumber);
            logger.Info("Set fp number:{0}", fpnumber);
            infoPr rStatus = infoPr.Good;
            try
            {
                Init init = new Init(fpnumber, server);
            }
            catch(Exception ex)
            {
                logger.Fatal(ex, "Завершена работа FP");
                rStatus = infoPr.CriticalError;
            }
            finally
            {

            }
            Console.ReadLine();
            Console.WriteLine("Time start:{0}", startJob);
            Console.WriteLine("Time stop:{0}", DateTime.Now);
            return (int)rStatus;
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("Help");
            Console.WriteLine("\t-h\tShow this screen");
            Console.WriteLine("\t-fp=\"numberfp\"\tSet fiscal printer");
            Console.WriteLine("\t-s=\"server\"\tSet server for work");
        }


        enum infoPr
        {
            Good=0,
            Bad=1,
            CriticalError=2
        }
    }
}
