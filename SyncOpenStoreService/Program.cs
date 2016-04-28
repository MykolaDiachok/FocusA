using NDesk.Options;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace SyncOpenStoreService
{
    static class Program
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        //private static string fpnumber;
        private static string compname;
        private static string serviceName = "SyncOpenStoreService";
        private static bool debugservice = false;

        private static void DisplayHelp(OptionSet p)
        {
            Console.WriteLine("Help for service OpenStore sync");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }


        private static void installService()
        {
            ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
            //Console.WriteLine("Service install");
            logger.Info("Service install");
        }

        private static void uninstallService()
        {
            ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
            logger.Info("Service uninstall");
        }

        private static void startService()
        {
            ServiceController controller = new ServiceController(serviceName);
            if (controller.Status == ServiceControllerStatus.Stopped)
                controller.Start();
        }

        private static void stopService()
        {
            ServiceController controller = new ServiceController(serviceName);
            if (controller.Status == ServiceControllerStatus.Running)
                controller.Stop();
        }



        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(params string[] args)
        {
            NLog.GlobalDiagnosticsContext.Set("FPNumber", "service");
            if (System.Environment.UserInteractive)
            {

                bool showHelp = false;
                List<int> fpnumbers = new List<int>();
                List<string> databases = new List<string>();
                List<string> dataservers = new List<string>();
                var os = new OptionSet()
                       .Add("i|install", "Install service", i => installService())
                       .Add("u|uninstall", "Uninstall service", u => uninstallService())
                       .Add("r|start", "Start service", r => startService())
                       .Add("p|stop", "Stop service", p => stopService())
                       .Add("?|h|help", "Show this screen", h => showHelp = h != null)
                       .Add("d|debug", "Debug service", d => debugservice = d != null)
                       .Add("fp|fpnumber=", "set fp or ser array fp", a => fpnumbers.Add(int.Parse(a)))
                       .Add("cn|compname=", "set computer name", cn => compname = cn)
                       .Add("ds|dataserver=", "set data server name", ds => dataservers.Add(ds))
                       .Add("db|database=", "set database name", db=>databases.Add(db));
                try
                {
                    var p = os.Parse(args);
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                    logger.Info("Try '--help' for more information.");
                    DisplayHelp(os);
                    return;
                }
                if (string.IsNullOrEmpty(compname))
                {
                    compname = System.Environment.MachineName;
                    //logger.Error("Need set computer name");
                }

                if ((showHelp) || string.IsNullOrEmpty(compname))
                {
                    DisplayHelp(os);
                    return;
                }

                if (debugservice)
                {
                    List<string> newargs = new List<string>();
                    newargs.Add(string.Format("--cn={0}", compname));
                    foreach (var fpn in fpnumbers)
                        newargs.Add(string.Format("--fp={0}", fpn));
                    foreach (var ds in dataservers)
                        newargs.Add(string.Format("--ds={0}", ds));
                    foreach (var db in databases)
                        newargs.Add(string.Format("--db={0}", db));

                    ServiceSyncOpenStore app = new ServiceSyncOpenStore();
                    app.onDebug(newargs.ToArray());
                    Console.WriteLine("For Stop - press \"Enter\" key!");
                    Console.ReadLine();
                    app.onDebugStop();
                    Console.WriteLine("For Stop - press \"Enter\" key!");
                    Console.ReadLine();
                }
            }
            else
            {


                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new ServiceSyncOpenStore(args)
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
