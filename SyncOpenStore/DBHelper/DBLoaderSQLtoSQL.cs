using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncOpenStore.DBHelper
{
    class DBLoaderSQLtoSQL : DBLoader
    {
        private DataClassesFocusADataContext focusA;
        private DataClassesOS_ROOTDataContext root;
        private long DateTimeBegin, DateTimeStop;
        private Table<tbl_ComInit> tbl_ComInit;

        //private DataClassesOSDataContext OS;

        public DBLoaderSQLtoSQL(string FPNumber) : base(FPNumber)
        {
            //focusA = new DataClassesFocusADataContext();
            /*root = new DataClassesOS_ROOTDataContext()*/;
        }

        /// <summary>
        ///Добавляем  проверку перед синхронизацией на действующий и работчий fp
        /// </summary>
        public override void SyncData()
        {
            using (focusA = new DataClassesFocusADataContext())
            using (root = new DataClassesOS_ROOTDataContext())
            {
                tbl_ComInit = focusA.GetTable<tbl_ComInit>();
                IQueryable<tbl_ComInit> initQuery =
                    from cominit in tbl_ComInit
                    where (cominit.Init == true // для синхронизации обязательно должно быть инициализирован                
                   && (!String.IsNullOrEmpty(FPNumber) ? cominit.RealNumber == FPNumber : true)
                   && (String.IsNullOrEmpty(FPNumber) ? false : true)

                   )
                    select cominit;

                foreach (tbl_ComInit init in initQuery)
                {

                    this.DateTimeBegin = init.DateTimeBegin.GetValueOrDefault();
                    this.DateTimeStop = init.DateTimeStop.GetValueOrDefault();
                    LoadDataFor_tbl_Cashier();
                    LoadDataFor_tbl_SALES();
                    LoadDataFor_tbl_Payment();
                    LoadDataFor_tbl_CashIO();
                    LoadDataFor_tbl_Operations();
                }
            }
            //base.SyncData();
        }

        public override void LoadDataFor_tbl_Cashier()
        {
            LoadDataFor_tbl_CashierBefore();
            //Table<SESS> sess = OS.GetTable<SESS>();
            //Table<CASHIER> cashier = OS.GetTable<CASHIER>();
            //Table<SALE> sales = OS.GetTable<SALE>();
            //Table<tbl_Cashier> tbl_Cashier = focusA.GetTable<tbl_Cashier>();
            //long outt;
            var getCachier = (from tsess in OS.GetTable<SESS>()
                              join tcashier in OS.GetTable<CASHIER>()
                              on tsess.CASHIERID equals tcashier.CASHIERID
                              join tsales in OS.GetTable<SALE>()
                              on new { SAREAID = tsess.SAREAID, SESSID = tsess.SESSID } equals new { SAREAID = tsales.SAREAID, SESSID = tsales.SESSID }
                              where tsess.DELFLAG == 0
                                 //&& long.Parse(tsales.PACKNAME)!=null
                                 && (long.Parse(tsess.SESSSTART) >= this.DateTimeBegin) 
                                 && (long.Parse(tsess.SESSSTART) <= this.DateTimeStop)                                
                                 && tsales.PACKNAME == FPNumber && tsales.DELFLAG == 0 && tsales.SALESTAG == 1
                              && (long.Parse(tsales.SALESTIME) >= this.DateTimeBegin)
                              && (long.Parse(tsales.SALESTIME) <= this.DateTimeStop)                             
                              select new { DATETIME = tsess.SESSSTART, FPNumber = FPNumber, Num_Cashier = 0, Name_Cashier = tcashier.CASHIERNAME.Substring(0, 15), Pass_Cashier = 0, TakeProgName = false }).GroupBy(x => x.DATETIME, (key, g) => g.OrderBy(e => e.Name_Cashier).First()).OrderBy(o => o.DATETIME).ToList();

            //var s2 = (from t in tbl_Cashier select new { t.DATETIME, t.FPNumber, t.Num_Cashier, t.Name_Cashier, t.Pass_Cashier, t.TakeProgName });

            var result = (from tlist in getCachier
                          select new { DATETIME = Int64.Parse(tlist.DATETIME), FPNumber = int.Parse(tlist.FPNumber), tlist.Num_Cashier, tlist.Name_Cashier, tlist.Pass_Cashier, tlist.TakeProgName })
                         .Except((from t in focusA.GetTable<tbl_Cashier>() select new { t.DATETIME, t.FPNumber, t.Num_Cashier, t.Name_Cashier, t.Pass_Cashier, t.TakeProgName }));
            //var result = getCachier.Except((from t in tbl_Cashier select new { t.DATETIME, t.FPNumber, t.Num_Cashier, t.Name_Cashier, t.Pass_Cashier, t.TakeProgName })).ToList();
            foreach (var rowCachier in result)
            {
                tbl_Cashier addNewCashier = new tbl_Cashier()
                {
                    DATETIME = rowCachier.DATETIME,
                    FPNumber = rowCachier.FPNumber,
                    Num_Cashier = rowCachier.Num_Cashier,
                    Name_Cashier = rowCachier.Name_Cashier.TrimStart().Substring(0, Math.Min(15, rowCachier.Name_Cashier.TrimStart().Length)).Trim(), //TODO тут должно быть имя
                    Pass_Cashier = rowCachier.Pass_Cashier,
                    TakeProgName = rowCachier.TakeProgName,
                    Operation = 3
                };
                focusA.tbl_Cashiers.InsertOnSubmit(addNewCashier);
                focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
                tbl_Operation op = new tbl_Operation
                {
                    NumSlave = addNewCashier.id,
                    DateTime = rowCachier.DATETIME,
                    FPNumber = rowCachier.FPNumber,
                    Operation = 3,
                    InWork = false,
                    Closed = false,
                    Disable = false,
                    CurentDateTime = DateTime.Now
                };
                focusA.tbl_Operations.InsertOnSubmit(op);
                focusA.SubmitChanges(ConflictMode.ContinueOnConflict);

                //Console.WriteLine("{0}", rowCachier.Name_Cashier);

            }

            //base.LoadDataFor_tbl_Cashier();

            LoadDataFor_tbl_CashierAfter();
        }

        public override void LoadDataFor_tbl_CashIO()
        {
            LoadDataFor_tbl_CashIOBefore();
            //base.LoadDataFor_tbl_CashIO();
            LoadDataFor_tbl_CashIOAfter();
        }

        public override void LoadDataFor_tbl_Operations()
        {
            LoadDataFor_tbl_OperationsBefore();
            //base.LoadDataFor_tbl_Operations();
            LoadDataFor_tbl_OperationsAfter();
        }

        public override void LoadDataFor_tbl_SALES()
        {
            LoadDataFor_tbl_SALESBefore();
            //base.LoadDataFor_tbl_SALES();
            LoadDataFor_tbl_SALESAfter();
        }

        public override void LoadDataFor_tbl_Payment()
        {
            LoadDataFor_tbl_PaymentBefore();
            //base.LoadDataFor_tbl_Payment();
            LoadDataFor_tbl_PaymentAfter();
        }


    }
}
