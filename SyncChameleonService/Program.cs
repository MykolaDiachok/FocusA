using NDesk.Options;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SyncChameleonService
{
    static class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static string server;
        private static string serviceName = "SyncChameleon";

        private static void DisplayHelp()
        {
            Console.WriteLine("Help");
            Console.WriteLine("\t-h\tShow this screen");
            Console.WriteLine("\t-i\tInstall service");
            Console.WriteLine("\t-u\tUninstall service");
            Console.WriteLine("\t-r\tStart service");
            Console.WriteLine("\t-p\tStop service");
            Console.WriteLine("==============================");
            Console.WriteLine("\t-s=\t-server=\"ServerName for sync\"");
            Console.WriteLine("\t-d\tDebug");
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

        static void Main(params string[] args)
        {

            if (System.Environment.UserInteractive)
            {

                string parameter = string.Concat(args);
                if (args.Length > 0)
                {

                    new OptionSet()
                       .Add("i|install", i => installService())
                       .Add("u|uninstall", u => uninstallService())
                       .Add("r|start", r => startService())
                       .Add("p|stop", p => stopService())
                       .Add("?|h|help", h => DisplayHelp())
                       .Add("s=|server=", a => server = a)
                       .Parse(args);
                    if((server!=null) &&(server.Length>0))
                    {                        
                        SyncCh app = new SyncCh(new string[] { "-s=" + server });
                        app.onDebug();
                        Console.WriteLine("For Stop - press key!");
                        Console.ReadLine();
                        app.onDebugStop();
                    }
                }
                else
                    DisplayHelp();                
            }
            else
            {

                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new SyncCh(args)
                };
                ServiceBase.Run(ServicesToRun);

            }
        }
    }
}
