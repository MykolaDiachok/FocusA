using NDesk.Options;
using NLog;
using SyncOpenStore.DBHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncOpenStore
{
    public class SyncDB
    {
        
        private static string fpnumber, server;
        private static int FPnumber;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static DateTime startJob;
        //private static System.Timers.Timer _timer;
        private static infoPr rStatus;
        private static bool run, automatic;
        private static System.Object lockThis = new System.Object();

        static int Main(string[] args)
        {
            startJob = DateTime.Now;
            rStatus = infoPr.Good;
            logger.Info("Time start:{0}", startJob);

            bool showHelp = false;
            var os = new OptionSet()
               .Add("fp|fpnumber=", "Set fpnumber", fp => fpnumber = fp)
               .Add("r|run", "run and waiting press \"Enter\" key", r => run = r != null)
               .Add("a|auto", "automatic service mode", a => automatic = a != null)
               .Add("sr|server=", "set server name, if not set - system will take current system server name", s => server = s)
               .Add("?|h|help", "show help", h => showHelp = h != null);
            try
            {
                var p = os.Parse(args);
                FPnumber = int.Parse(fpnumber);
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                logger.Info("Try '--help' for more information.");
                DisplayHelp(os);
            }

            if ((showHelp) || string.IsNullOrEmpty(fpnumber))
            {
                if (string.IsNullOrEmpty(fpnumber))
                    logger.Error("Need set fp");
                DisplayHelp(os);
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


            ForeachFPNumber mysync = new ForeachFPNumber(fpnumber, automatic, run);
            mysync.Work();
            
            logger.Info("Exit program, status={0}", rStatus);
            return (int)rStatus;
        }


        private static void DisplayHelp(OptionSet p)
        {
            Console.WriteLine("Help for sync Openstore");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

    }

    enum infoPr
    {
        Good = 0,
        Bad = 1,
        CriticalError = 2
    }

   

}
