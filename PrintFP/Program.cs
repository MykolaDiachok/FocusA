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
using System.Threading;

namespace PrintFP
{
    public static class Program 
    {
        private static string fpnumber, server;
        private static int FPnumber;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static DateTime startJob;
        //private static System.Timers.Timer _timer;
        private static infoPr rStatus;
        private static bool run, automatic;
        private static System.Object lockThis = new System.Object();
        private static MyEventLog eventLog1;
        

        static int Main(params string[] args)
        {

            startJob = DateTime.Now;
            logger.Info("Time start:{0}", startJob);
#if (!DEBUG)
            //logger.Info("Enable NBug");
            //AppDomain.CurrentDomain.UnhandledException += Handler.UnhandledException;
            //TaskScheduler.UnobservedTaskException += Handler.UnobservedTaskException;
#endif
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
            eventLog1 = new MyEventLog(automatic, fpnumber);

            if (String.IsNullOrEmpty(server))
            {
                server = System.Environment.MachineName;
#if (DEBUG)
                server = "focus-A";
#endif
            }
            NLog.GlobalDiagnosticsContext.Set("FPNumber", fpnumber);
            logger.Info("Set fp number:{0}", fpnumber);
            //eventLog1.WriteEntry("Timer start");
            //rStatus = infoPr.Good;
            //_timer = new System.Timers.Timer();
            //_timer.Interval = (Properties.Settings.Default.TimerIntervalSec * 1000);
            ////_timer.Interval = (100);
            //_timer.Elapsed += (sender, e) => { HandleTimerElapsed(); };
            //_timer.Enabled = true;



            //shutdownEvent = new ManualResetEvent(false);
           
            //else if (automatic)
            //{
            //    new Thread(() =>
            //    {
            //        TimeSpan delay = new TimeSpan(0, 0, 10);
            //        var shutdownEventauto = new ManualResetEvent(false);
            //        while (shutdownEventauto.WaitOne(delay, true) == false)
            //        {
            //            using (DataClasses1DataContext focus = new DataClasses1DataContext())
            //            {
            //                var rowinit = (from tinit in focus.GetTable<tbl_ComInit>()
            //                            where tinit.FPNumber == int.Parse(fpnumber)
            //                            select tinit).FirstOrDefault();
            //                if (!(bool)rowinit.auto)
            //                {
            //                    shutdownEvent.Set();
            //                    shutdownEventauto.Set();
            //                }
            //            }
            //        }
            //    }).Start();
            //}

            //ManualReset();
            
            Init init = new Init(fpnumber, server, automatic,run);
            init.Work();
            UpdateStatusFP.setStatusFP(FPnumber, "outwork");
            logger.Info("Exit program, status={0}", rStatus);
            return (int)rStatus;
        }

      


        //private static void ManualReset()
        //{
        //    UpdateStatusFP.setStatusFP(FPnumber, "waiting out");
        //    TimeSpan delay = new TimeSpan(0,0, Properties.Settings.Default.TimerIntervalSec);
        //    shutdownEvent = new ManualResetEvent(false);
        //    while (shutdownEvent.WaitOne(delay, true) == false)
        //    {
        //        //logger.Trace("lockthis in {0}", DateTime.Now);
        //        lock (lockThis)
        //        {
        //            //logger.Trace("lockthis {0}", DateTime.Now);
        //            Init init = new Init(fpnumber, server, automatic);
        //            init.Work();
        //        }
        //        //logger.Trace("lockthis out {0}", DateTime.Now);
        //    }
        //    UpdateStatusFP.setStatusFP(FPnumber, "waiting...");
        //}

        //private static void HandleTimerElapsed()
        //{
        //    lock (lockThis)
        //    {

        //        //eventLog1.WriteEntry("in HandleTimerElapsed");
        //        //_timer.Stop();
        //        //ImpatientMethod();
        //        try
        //        {
        //            Init init = new Init(fpnumber, server, automatic);
        //            init.Work();
        //        }
        //        catch (Exception ex)
        //        {
        //            eventLog1.WriteEntry("Завершена работа FP:" + ex.Message);
        //            logger.Fatal(ex, "Завершена работа FP");
        //            rStatus = infoPr.CriticalError;
        //            Thread.Sleep(30 * 1000);
        //        }
        //        finally
        //        {

        //        }
        //        //_timer.Start();
        //        //eventLog1.WriteEntry("out HandleTimerElapsed");
        //    }
        //}


        private static void LongMethod()
        {
            //changeStatus.setStatusOnLine();
            logger.Trace("in LongMethod");
            try
            {
                Init init = new Init(fpnumber, server);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Завершена работа FP");
                rStatus = infoPr.CriticalError;
                Thread.Sleep(30 * 1000);
            }
            finally
            {

            }
            logger.Trace("out LongMethod");
            //changeStatus.setStatusWaiting();
        }

        private static void ImpatientMethod()
        {
            Action longMethod = LongMethod; //use Func if you need a return value

            ManualResetEvent mre = new ManualResetEvent(false);

            Thread actionThread = new Thread(new ThreadStart(() =>
            {
                var iar = longMethod.BeginInvoke(null, null);
                longMethod.EndInvoke(iar); //always call endinvoke
                mre.Set();
            }));

            actionThread.Start();
            var start = DateTime.Now;
            mre.WaitOne(10 * 60 * 1000); // waiting 10 min (or less)
            if (actionThread.IsAlive)
            {
                actionThread.Abort();
                logger.Fatal("Waiting out, start waiting:{0}, current:{1}", start, DateTime.Now);
            }
        }

        private static void DisplayHelp(OptionSet p)
        {
            Console.WriteLine("Help for print FP");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        //public void Dispose()
        //{
        //    lock (lockThis)
        //    {
        //        logger.Trace("Dispose");
        //        shutdownEvent.Set();                
        //        //_timer.Stop();
        //        //_timer.Dispose();
        //        logger.Info("Time stop:{0}", DateTime.Now);
        //    }
        //    //throw new NotImplementedException();
        //}

        enum infoPr
        {
            Good = 0,
            Bad = 1,
            CriticalError = 2
        }
    }
}
