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
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public static bool stopProcessor = false;
        public static bool Terminate = false;
        public static System.Timers.Timer _timer;

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
                .Add("?|h|help",h=>DisplayHelp())
                .Parse(args);

            if (fpnumber == null)
            {
                DisplayHelp();            
                return;
            }
            
            GlobalDiagnosticsContext.Set("FPNumber", fpnumber);
            logger.Info("Set fp number:{0}", fpnumber);


            _timer = new System.Timers.Timer();
            _timer.Interval = (Properties.Settings.Default .TimerIntervalSec * 1000);
            _timer.Elapsed += (sender, e) => { HandleTimerElapsed(fpnumber); };
            _timer.Enabled = true;
            Console.ReadLine();
            _timer.Dispose();

          

            logger.Trace("End Main");
        }

        static void HandleTimerElapsed(string fpnumber)
        {
            _timer.Stop();
            StopwatchHelper.Start("Begin select");
            
            String DateWork = DateTime.Now.ToString("dd.MM.yyyy");



            var connection = Properties.Settings.Default.Npgsql;//System.Configuration.ConfigurationManager.ConnectionStrings["Test"].ConnectionString;
            using (var conn = new NpgsqlConnection(connection))
            {
                logger.Trace("NpgsqlConnection:{0}", connection);
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"select * 
			                                from sales.sales_log 
			                                where id_registrar = '"+ fpnumber + @"'
                                                and date_trunc('day',time_create)='"+ DateWork + @"'
			                               ";
                    logger.Trace("Select from base:{0}", cmd.CommandText);
                    StopwatchHelper.Start("ExecuteReader");
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            
                            logger.Trace("Time Sales:{0}", reader["time_sales"]);
                            //Console.WriteLine(reader.GetString(0));
                        }
                    }
                   StopwatchHelper.Stop("ExecuteReader");
                }
            }
            
            StopwatchHelper.Stop("Begin select");
            _timer.Start();
        }


        static void DisplayHelp()
        {
            string showInfo = @"====================HELP========================== \n\r
                '-fp' or '-fpnumer' введите номер аппарата для выборки";
            logger.Trace("Show info:{0}", showInfo);
            Console.WriteLine(showInfo);
            
        }
    }
}
