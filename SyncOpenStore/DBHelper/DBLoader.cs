using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using SyncHameleon;
using System.Data.Linq;

namespace SyncOpenStore.DBHelper
{
    abstract class DBLoader : ILoadDataOS, ISync
    {

        /// <summary>
        /// Номер аппарата в текущей программе и базе
        /// </summary>
        public string sFPNumber { get; set; }
        public int iFPNumber { get; set; }
        /// <summary>
        /// Номер аппарта откуда берем данные
        /// </summary>
        public string sRealNumber { get; set; }
        public int iRealNumber { get; set; }
        public Int64 DateTimeBegin { get; set; }
        public Int64 DateTimeStop { get; set; }
        public NLog.Logger logger;
        private DateTime startjob;
        private DateTime stopjob;
        private System.Timers.Timer _timer;
        public DbHelperSQL.DbHelperSQL changeTable;
        public DbHelperSQL.DbHelperSQLStatus changeStatus;
        // public DataClassesOSDataContext OS;



        public DBLoader(string FPNumber, string RealNumber, Int64 DateTimeBegin, Int64 DateTimeStop)
        {
            Init(FPNumber, RealNumber, DateTimeBegin, DateTimeStop);
        }

        public string FPNumber
        {
            get
            {
                return sFPNumber;
            }
        }


        public DateTime startJob
        {
            get
            {
                return startjob;
            }

        }

        public DateTime stopJob
        {
            get
            {
                return stopjob;
            }
        }

        public void Init(string FPNumber, string RealNumber, Int64 DateTimeBegin, Int64 DateTimeStop)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            NLog.GlobalDiagnosticsContext.Set("FPNumber", FPNumber);
            this.DateTimeBegin = DateTimeBegin;
            this.DateTimeStop = DateTimeStop;
            this.sRealNumber = RealNumber;
            this.iRealNumber = int.Parse(RealNumber);
            this.iFPNumber = int.Parse(FPNumber);
            this.sFPNumber = FPNumber;
            using (DataClassesFocusADataContext focusA = new DataClassesFocusADataContext())
            {
                var initRow = (from init in focusA.GetTable<tbl_ComInit>()
                               where init.FPNumber == iFPNumber
                               select init).FirstOrDefault();

                changeTable = new DbHelperSQL.DbHelperSQL(initRow.CompName, iFPNumber, initRow.DataServer, initRow.DataBaseName, initRow.Port, initRow.MoxaIP, (int)initRow.MoxaPort);
                if (DateTimeBegin==0)
                {
                    DateTimeBegin = (Int64)initRow.DateTimeBegin;
                }
                if (DateTimeStop == 0)
                {
                    DateTimeStop = (Int64)initRow.DateTimeStop;
                }
            }
            
            logger.Trace("Init db exchange fp number:{0}", sFPNumber);
            //OS = new DataClassesOSDataContext();
        }




        #region LoadDataFor_tbl_Cashier

        public void LoadDataFor_tbl_CashierBefore()
        {
            StopwatchHelper.Start(String.Format("LoadDataFor_tbl_Cashier:{0}", sFPNumber));
        }

        public void LoadDataFor_tbl_CashierAfter()
        {
            StopwatchHelper.Stop(String.Format("LoadDataFor_tbl_Cashier:{0}", sFPNumber));
        }

        public virtual void LoadDataFor_tbl_Cashier()
        {
            LoadDataFor_tbl_CashierBefore();


            //throw new NotImplementedException();
            LoadDataFor_tbl_CashierAfter();
        }

        #endregion

        #region LoadDataFor_tbl_CashIO

        public void LoadDataFor_tbl_CashIOBefore()
        {
            StopwatchHelper.Start(String.Format("LoadDataFor_tbl_CashIO:{0}", sFPNumber));
        }

        public void LoadDataFor_tbl_CashIOAfter()
        {
            StopwatchHelper.Stop(String.Format("LoadDataFor_tbl_CashIO:{0}", sFPNumber));
        }

        public virtual void LoadDataFor_tbl_CashIO()
        {
            LoadDataFor_tbl_CashIOBefore();

            LoadDataFor_tbl_CashIOAfter();
        }

        #endregion


        #region LoadDataFor_tbl_Operations

        public void LoadDataFor_tbl_OperationsBefore()
        {
            StopwatchHelper.Start(String.Format("LoadDataFor_tbl_Operations:{0}", sFPNumber));
        }

        public void LoadDataFor_tbl_OperationsAfter()
        {
            StopwatchHelper.Stop(String.Format("LoadDataFor_tbl_Operations:{0}", sFPNumber));
        }

        public virtual void LoadDataFor_tbl_Operations()
        {
            LoadDataFor_tbl_OperationsBefore();

            LoadDataFor_tbl_OperationsAfter();
        }


        #endregion

        #region LoadDataFor_tbl_Payment

        public void LoadDataFor_tbl_PaymentBefore()
        {
            StopwatchHelper.Start(String.Format("LoadDataFor_tbl_Payment:{0}", sFPNumber));
        }

        public void LoadDataFor_tbl_PaymentAfter()
        {
            StopwatchHelper.Stop(String.Format("LoadDataFor_tbl_Payment:{0}", sFPNumber));
        }

        public virtual void LoadDataFor_tbl_Payment()
        {
            LoadDataFor_tbl_PaymentBefore();

            LoadDataFor_tbl_PaymentAfter();
        }

        #endregion


        #region LoadDataFor_tbl_SALES

        public void LoadDataFor_tbl_SALESBefore()
        {
            StopwatchHelper.Start(String.Format("LoadDataFor_tbl_SALES:{0}", sFPNumber));
        }

        public void LoadDataFor_tbl_SALESAfter()
        {
            StopwatchHelper.Stop(String.Format("LoadDataFor_tbl_SALES:{0}", sFPNumber));
        }

        public virtual void LoadDataFor_tbl_SALES()
        {
            LoadDataFor_tbl_SALESBefore();

            LoadDataFor_tbl_SALESAfter();
        }

        #endregion




        #region sync

        public virtual void StartSync()
        {

            logger.Trace("Start sync");
            startjob = DateTime.Now;
            _timer = new System.Timers.Timer();
            _timer.Interval = (Properties.Settings.Default.TimerIntervalSec * 1000);
            _timer.Elapsed += (sender, e) => { HandleTimerElapsed(); };
            _timer.Enabled = true;
            _timer.Start();
        }

        public virtual void SyncData()
        {
            _timer.Stop();
            LoadDataFor_tbl_Cashier();
            LoadDataFor_tbl_CashIO();
            LoadDataFor_tbl_Payment();
            LoadDataFor_tbl_SALES();
            LoadDataFor_tbl_Operations();
            _timer.Start();
        }

        public virtual void HandleTimerElapsed()
        {
            SyncData();
            //Thread.Sleep(1000);

        }

        public virtual void StartSync(string sqlserver, string fpnumber, string RealNumber, Int64 DateTimeBegin, Int64 DateTimeStop)
        {
            Init(fpnumber, RealNumber, DateTimeBegin, DateTimeStop);
            StartSync();
        }

        public virtual void StopSync()
        {
            logger.Trace("Stop sync");

            _timer.Stop();
            stopjob = DateTime.Now;
            logger.Trace("Start job:{0}", startjob);
            logger.Trace("Stop job:{0}", startjob);
        }

        #endregion

    }
}
