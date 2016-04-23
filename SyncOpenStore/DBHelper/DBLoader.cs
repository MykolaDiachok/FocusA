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
    abstract class DBLoader:ILoadDataOS,ISync
    {
        
        private string fpnumber;
        public NLog.Logger logger;
        private DateTime startjob;
        private DateTime stopjob;
        private System.Timers.Timer _timer;
        public DataClassesOSDataContext OS;



        public DBLoader(string FPNumber)
        {           
            Init(FPNumber);
        }

        public string FPNumber
        {
            get
            {
                return fpnumber;
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

        public void Init(string FPNumber)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            
            this.fpnumber = FPNumber;
            logger.Trace("Init db exchange fp number:{0}",fpnumber);
            OS = new DataClassesOSDataContext();
        }




        #region LoadDataFor_tbl_Cashier

        public void LoadDataFor_tbl_CashierBefore()
        {
            StopwatchHelper.Start(String.Format("LoadDataFor_tbl_Cashier:{0}", fpnumber));
        }

        public void LoadDataFor_tbl_CashierAfter()
        {
            StopwatchHelper.Stop(String.Format("LoadDataFor_tbl_Cashier:{0}", fpnumber));
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
            StopwatchHelper.Start(String.Format("LoadDataFor_tbl_CashIO:{0}", fpnumber));
        }

        public void LoadDataFor_tbl_CashIOAfter()
        {
            StopwatchHelper.Stop(String.Format("LoadDataFor_tbl_CashIO:{0}", fpnumber));
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
            StopwatchHelper.Start(String.Format("LoadDataFor_tbl_Operations:{0}", fpnumber));
        }

        public void LoadDataFor_tbl_OperationsAfter()
        {
            StopwatchHelper.Stop(String.Format("LoadDataFor_tbl_Operations:{0}", fpnumber));
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
            StopwatchHelper.Start(String.Format("LoadDataFor_tbl_Payment:{0}", fpnumber));
        }

        public void LoadDataFor_tbl_PaymentAfter()
        {
            StopwatchHelper.Stop(String.Format("LoadDataFor_tbl_Payment:{0}", fpnumber));
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
            StopwatchHelper.Start(String.Format("LoadDataFor_tbl_SALES:{0}", fpnumber));
        }

        public void LoadDataFor_tbl_SALESAfter()
        {
            StopwatchHelper.Stop(String.Format("LoadDataFor_tbl_SALES:{0}",fpnumber));
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
        }

        public virtual void SyncData()
        {
            LoadDataFor_tbl_Cashier();
            LoadDataFor_tbl_CashIO();
            LoadDataFor_tbl_Payment();
            LoadDataFor_tbl_SALES();
            LoadDataFor_tbl_Operations();
        }

        public virtual void HandleTimerElapsed()
        {
            SyncData();
                        //Thread.Sleep(1000);
            _timer.Start();
        }

        public virtual void StartSync(string sqlserver, string fpnumber)
        {
            Init(fpnumber);
            StartSync();
        }

        public virtual void StopSync()
        {
            logger.Trace("Stop sync");
            
            _timer.Stop();
            stopjob = DateTime.Now;
            logger.Trace("Start job:{0}",startjob);
            logger.Trace("Stop job:{0}", startjob);
        }

        #endregion

    }
}
