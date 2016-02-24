using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace SyncHameleon
{
    class Postrgres
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static bool stopProcessor = false;
        private static bool Terminate = false;
        private static System.Timers.Timer _timer;
        private static string _SQLServer, _FPNumber;
        private static string _DateWork;


        public static void startSync(string sqlserver, string fpnumber)
        {
            _SQLServer = sqlserver;
            _FPNumber = fpnumber;
            _timer = new System.Timers.Timer();
            _timer.Interval = (Properties.Settings.Default.TimerIntervalSec * 1000);
            _timer.Elapsed += (sender, e) => { HandleTimerElapsed(); };
            _timer.Enabled = true;
            Console.ReadLine();
            _timer.Dispose();
        }

        private static void HandleTimerElapsed()
        {
            _DateWork = DateTime.Now.ToString("dd.MM.yyyy");
            SelectOperation();
        }


        private static void SelectOperation()
        {
            _timer.Stop();
            StopwatchHelper.Start("Begin select");

            



            var connection = Properties.Settings.Default.Npgsql;//System.Configuration.ConfigurationManager.ConnectionStrings["Test"].ConnectionString;
            using (var conn = new NpgsqlConnection(connection))
            {
                logger.Trace("NpgsqlConnection:{0}", connection);
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = getQueryPost();
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

        private static string getQueryPost()
        {
            string ret =@"select * 
			                                from sales.sales_log 
			                                where id_registrar = '" + _FPNumber + @"'
                                                and id_action in (1,2, 12, 13, 14, 1001)
                                                and date_trunc('day',time_create)='" + _DateWork + @"'
			                               ";

            return ret;
        }

        private enum LogOperations
        {
            Launch  =  1,
            InCash  =  2,
            Check   =  3,
            OutCash = 12,
            Xreport = 13,
            Zreport = 14,
            SetCashier = 1001
        }
    }
}
