using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SyncOpenStore.DBHelper
{
    /// <summary>
    /// Класс для обхода таблицы и выборки всех аппаратов
    /// </summary>
    public class ForeachFPNumber
    {
        private Logger logger = LogManager.GetCurrentClassLogger();
        public string inFpNumber { get; private set; }
        public int iFPNumber { get; private set; }
        private bool automatic, manual;
        private static ManualResetEvent shutdownEvent;
        private System.Object lockThis = new System.Object();

        public ForeachFPNumber(string inFpNumber, bool automatic = false, bool manual = false)
        {
            NLog.GlobalDiagnosticsContext.Set("FPNumber", inFpNumber);
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            this.inFpNumber = inFpNumber;
            this.iFPNumber = int.Parse(inFpNumber);
            this.automatic = automatic;
            this.manual = manual;
        }

        private void ReadDataFromConsole(object state)
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            Console.WriteLine("Enter \"x\" to exit.");

            while (Console.ReadKey().KeyChar != 'x')
            {
                Console.Out.WriteLine("");
                Console.Out.WriteLine("Enter again!");
            }

            shutdownEvent.Set();
        }

        public void Work()
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            //TODO TRY CATCH
            logger.Trace("manual={0}, automatic={1}", manual, automatic);
            if (manual && !automatic)
            {
                Thread status = new Thread(ReadDataFromConsole);
                status.Start();
            }

            ManualReset();
        }


        private void ManualReset()
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);            
            TimeSpan delay = new TimeSpan(0, 0, Properties.Settings.Default.TimerIntervalSec);
            shutdownEvent = new ManualResetEvent(false);
            while (shutdownEvent.WaitOne(delay, true) == false)
            {
                if (automatic)
                {
                    using (DataClassesFocusADataContext focus = new DataClassesFocusADataContext())
                    {
                        var rowinit = (from tinit in focus.GetTable<tbl_ComInit>()
                                       where tinit.FPNumber == iFPNumber
                                       select tinit).FirstOrDefault();
                        if (!(bool)rowinit.auto)
                        {
                            logger.Trace("set shutdownEvent");
                            shutdownEvent.Set();
                            return;
                        }
                    }
                }
                //logger.Trace("lockthis in {0}", DateTime.Now);
                lock (lockThis)
                {
                    try
                    {
                        MakeForeach();
                        
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                        //Thread.Sleep(Properties.Settings.Default.TimerIntervalSec * 1000);
                        Thread.Sleep(30 * 1000);
                    }
                }
                //logger.Trace("lockthis out {0}", DateTime.Now);
            }            
        }


        public void MakeForeach()
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            //TODO TRY CATCH
            logger.Trace("manual={0}, automatic={1}", manual, automatic);

            using (DataClassesFocusADataContext focusA = new DataClassesFocusADataContext())            
            {

                var tbl_ComInit = focusA.GetTable<tbl_ComInit>();
                var init =
                    (from cominit in tbl_ComInit
                    where (cominit.Init == true // для синхронизации обязательно должно быть инициализирован                
                   && cominit.FPNumber == iFPNumber)
                    select cominit).FirstOrDefault();

                if (init!=null)
                {
                    //TODO TRY CATCH
                    string connstr = Properties.Settings.Default.CashDesk_OSConnectionString;
                    //init.DataServer
                    //init.DataBaseName
                    Regex rgx = new Regex("Data Source=([^;]*);");
                    connstr = rgx.Replace(connstr, "Data Source=" + init.DataServer + ";");
                    rgx = new Regex("Initial Catalog=([^;]*);");
                    connstr = rgx.Replace(connstr, "Initial Catalog=" + init.DataBaseName + ";");

                    DBLoaderSQLtoSQL syncdb = new DBLoaderSQLtoSQL(init.FPNumber.ToString(), init.RealNumber, (Int64)init.DateTimeBegin, (Int64)init.DateTimeStop, connstr);
                    
                    syncdb.SyncData();                    
                }
            }
        }




    }
}
