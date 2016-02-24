using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Npgsql;
using NBug;
using NLog;
using NLog.Config;

using NDesk.Options;
using System.Threading;

namespace SyncHameleon
{
    class Program
    {
        private static string fpnumber;
        private static string sqlserver;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {

            logger.Trace("Start Main");            

#if (!DEBUG)
                logger.Info("Enable NBug");
                AppDomain.CurrentDomain.UnhandledException += Handler.UnhandledException;
                TaskScheduler.UnobservedTaskException += Handler.UnobservedTaskException;
#endif

            new OptionSet()
                .Add("fp=|fpnumber=", f => fpnumber = f)
                .Add("s=|sqlserver=", s => sqlserver = s)
                .Add("?|h|help",h=>DisplayHelp())
                .Parse(args);

            if ((fpnumber == null)&&(sqlserver == null))
            {
                DisplayHelp();            
                return;
            }
            
            GlobalDiagnosticsContext.Set("FPNumber", fpnumber);
            logger.Info("Set fp number:{0}", fpnumber);
            GlobalDiagnosticsContext.Set("sqlserver", sqlserver);
            logger.Info("Set sqlserver:{0}", sqlserver);

            Postrgres.startSync(sqlserver, fpnumber);
                    

            logger.Trace("End Main");
        }

        


        static void DisplayHelp()
        {
            Console.WriteLine("====================HELP==========================");
            Console.WriteLine("'-fp' or '-fpnumer' введите номер аппарата для выборки");
            Console.WriteLine("'-s' or '-sqlserver' sql сервер для синхронизации");
            //logger.Trace("Show info:{0}", showInfo);
            //Console.WriteLine(showInfo);
            
        }
    }
}
