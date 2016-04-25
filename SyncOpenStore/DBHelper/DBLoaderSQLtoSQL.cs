using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SyncOpenStore.DBHelper
{
    class DBLoaderSQLtoSQL : DBLoader
    {


        public DBLoaderSQLtoSQL(string FPNumber, string RealNumber, Int64 DateTimeBegin, Int64 DateTimeStop) : base(FPNumber, RealNumber, DateTimeBegin, DateTimeStop)
        {

        }

        /// <summary>
        ///Sync our data
        /// </summary>
        public override void SyncData()
        {
            LoadDataFor_tbl_Cashier();
            LoadDataFor_tbl_SALES();
            LoadDataFor_tbl_Payment();
            LoadDataFor_tbl_CashIO();
            LoadDataFor_tbl_Operations();
        }

        public override void LoadDataFor_tbl_Cashier()
        {
            LoadDataFor_tbl_CashierBefore();

            using (DataClassesFocusADataContext focusA = new DataClassesFocusADataContext())
            using (DataClassesOSDataContext OS = new DataClassesOSDataContext())
            {

                var getCachier = (from tsess in OS.GetTable<SESS>()
                                  join tcashier in OS.GetTable<CASHIER>()
                                  on tsess.CASHIERID equals tcashier.CASHIERID
                                  join tsales in OS.GetTable<SALE>()
                                  on new { SAREAID = tsess.SAREAID, SESSID = tsess.SESSID } equals new { SAREAID = tsales.SAREAID, SESSID = tsales.SESSID }
                                  where tsess.DELFLAG == 0
                                     //&& long.Parse(tsales.PACKNAME)!=null
                                     && (Convert.ToInt64(tsess.SESSSTART) >= this.DateTimeBegin)
                                     && (Convert.ToInt64(tsess.SESSSTART) <= this.DateTimeStop)
                                     && tsales.PACKNAME == sRealNumber && tsales.DELFLAG == 0 && tsales.SALESTAG == 1
                                  && (Convert.ToInt64(tsales.SALESTIME) >= this.DateTimeBegin)
                                  && (Convert.ToInt64(tsales.SALESTIME) <= this.DateTimeStop)
                                  select new { DATETIME = tsess.SESSSTART, FPNumber = FPNumber, Num_Cashier = 0, Name_Cashier = tcashier.CASHIERNAME.Substring(0, 15), Pass_Cashier = 0, TakeProgName = false })
                                    .GroupBy(x => x.DATETIME, (key, g) => g.OrderBy(e => e.Name_Cashier).First())
                                    .OrderBy(o => o.DATETIME)
                                    .ToList();

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
                        FPNumber = iFPNumber,
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
                        FPNumber = iFPNumber,
                        Operation = 3,
                        InWork = false,
                        Closed = false,
                        Disable = false,
                        CurentDateTime = DateTime.Now
                    };
                    focusA.tbl_Operations.InsertOnSubmit(op);
                    focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
                }
            }
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

            using (DataClassesFocusADataContext focusA = new DataClassesFocusADataContext())
            {
                var allOp = (from tOperation in focusA.GetTable<tbl_Operation>()
                             where tOperation.FPNumber == iFPNumber
                                              // && tOperation.DateTime >= initRow.DateTimeBegin && tOperation.DateTime < initRow.DateTimeStop
                                              && tOperation.DateTime >= DateTimeBegin && tOperation.DateTime <= DateTimeStop
                             select tOperation);

                var allPayment = (from list1 in focusA.GetTable<tbl_Payment>()
                                  where list1.FPNumber == iFPNumber
                                      //&& list1.DATETIME >= initRow.DateTimeBegin && list1.DATETIME < initRow.DateTimeStop
                                      && list1.DATETIME >= DateTimeBegin && list1.DATETIME <= DateTimeStop
                                      && !((bool)list1.Disable)
                                  select list1);

                var preOp = (from list1 in focusA.GetTable<tbl_Payment>()
                             where list1.FPNumber == iFPNumber
                                 //&& list1.DATETIME >= initRow.DateTimeBegin && list1.DATETIME < initRow.DateTimeStop
                                 && list1.DATETIME >= DateTimeBegin && list1.DATETIME <= DateTimeStop
                                 && !((bool)list1.Disable)
                             select list1).Except(
                                from tPayment in allPayment
                                join tOperation in allOp

                               on new { DATETIME = tPayment.DATETIME, FPNumber = tPayment.FPNumber, Op = tPayment.Operation, Num = tPayment.id }
                                    equals new { DATETIME = tOperation.DateTime, FPNumber = tOperation.FPNumber, Op = tOperation.Operation, Num = (long)tOperation.NumSlave }
                                select tPayment);
                foreach (var rowPayment in preOp)
                {
                    tbl_Operation newOp = new tbl_Operation
                    {
                        NumSlave = rowPayment.id,
                        DateTime = rowPayment.DATETIME,
                        FPNumber = rowPayment.FPNumber,
                        Operation = rowPayment.Operation,
                        InWork = false,
                        Closed = false,
                        CurentDateTime = DateTime.Now,
                        Disable = false
                    };
                    focusA.tbl_Operations.InsertOnSubmit(newOp);
                    focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
                    rowPayment.NumOperation = newOp.id;
                    focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
                    changeTable.Change_tbl_Operations();
                }
            }



            LoadDataFor_tbl_OperationsAfter();
        }

        public override void LoadDataFor_tbl_SALES()
        {
            LoadDataFor_tbl_SALESBefore();
            using (DataClassesFocusADataContext focusA = new DataClassesFocusADataContext())
            using (DataClassesOSDataContext OS = new DataClassesOSDataContext())
            using (DataClassesOS_ROOTDataContext OS_ROOT = new DataClassesOS_ROOTDataContext())
            {
                var initrow = (from cominit in focusA.GetTable<tbl_ComInit>()
                               where FPNumber == sFPNumber
                               select cominit).FirstOrDefault();

                var prepareHeaders = (from headers in OS.GetTable<SALE>()
                                      where headers.PACKNAME == sRealNumber
                                      && headers.DELFLAG == 0
                                      && headers.SALESTAG == 2
                                      && headers.SALESCANC == 0
                                      && Convert.ToInt32(headers.FRECNUM) != 0
                                      && Convert.ToInt64(headers.SALESTIME) >= DateTimeBegin && Convert.ToInt64(headers.SALESTIME) <= DateTimeStop
                                      select headers).OrderBy(x => x.SALESTIME);





                foreach (var header in prepareHeaders)
                {
                    var payment = (from pay in focusA.GetTable<tbl_Payment>()
                                   where pay.FPNumber == iFPNumber
                                   && pay.SESSID == header.SESSID
                                   && pay.DATETIME == Convert.ToInt64(header.SALESTIME)
                                   && pay.SYSTEMID == header.SYSTEMID
                                   && pay.SAREAID == header.SAREAID
                                   && pay.FRECNUM == header.FRECNUM
                                   && pay.SRECNUM == header.SRECNUM
                                   select pay).FirstOrDefault();
                    if (payment != null)
                        continue;

                    


                    var SALESH1_0 = (from headers2 in OS.GetTable<SALE>()
                                     where headers2.PACKNAME == sRealNumber
                                     //&& ((headers2.SALESBARC.Substring(0,3)!="291") && (headers2.SALESBARC.Length!=14))//если определять карточки
                                     && headers2.SALESTAG == 1
                                     && headers2.SALESTYPE == 1
                                     && headers2.DELFLAG == 0
                                     && headers2.SESSID == header.SESSID
                                     && headers2.SALESTIME == header.SALESTIME
                                     && headers2.SYSTEMID == header.SYSTEMID
                                     && headers2.SAREAID == header.SAREAID
                                     && headers2.FRECNUM == header.FRECNUM
                                     && headers2.SRECNUM == header.SRECNUM
                                     select headers2
                                         ).Sum(x => x.SALESSUM);

                 


                    var SALESH1_3 = (from headers2 in OS.GetTable<SALE>()
                                     where headers2.PACKNAME == sRealNumber
                                     && headers2.SALESTAG == 1
                                     && headers2.SALESTYPE == 0
                                     && headers2.DELFLAG == 0
                                     && headers2.SESSID == header.SESSID
                                     && headers2.SALESTIME == header.SALESTIME
                                     && headers2.SYSTEMID == header.SYSTEMID
                                     && headers2.SAREAID == header.SAREAID
                                     && headers2.FRECNUM == header.FRECNUM
                                     && headers2.SRECNUM == header.SRECNUM
                                     select headers2
                                         ).Sum(x => x.SALESSUM);

                    var DCARD = (from dcards in OS.GetTable<DCARD>()
                                 join clnt in OS.GetTable<CLNT>()
                                 on dcards.CLNTID equals clnt.CLNTID
                                 where dcards.CLNTID == header.CLNTID
                                 select new { dcards.DCARDCODE, dcards.DCARDNAME, clnt.CLNTGRPID }).FirstOrDefault();

                    var acc = (from root in OS_ROOT.GetTable<ACCOUNT>()
                               where root.ACCOUNTTYPEID == 1
                               && root.CLNTID == header.CLNTID
                               select root).FirstOrDefault();

                    tbl_Payment newpay = new tbl_Payment()
                    {
                        DATETIME = Int64.Parse(header.SALESTIME),
                        FPNumber = iFPNumber,
                        SESSID = header.SESSID,
                        SYSTEMID = header.SYSTEMID,
                        SAREAID = header.SAREAID,
                        Type = header.SALESREFUND,
                        Operation = header.SALESREFUND == 0 ? 12 : 5,
                        FRECNUM = header.FRECNUM,
                        SRECNUM = header.SRECNUM,
                        Payment_Status = 11,
                        Payment = Convert.ToInt32(header.SALESSUM),
                        Payment0 = Convert.ToInt32(SALESH1_0),
                        Payment3 = Convert.ToInt32(SALESH1_3),
                        FiscStatus = true,
                        CheckSum = Convert.ToInt32(header.SALESSUM),
                        PayBonus = Convert.ToInt32(header.SALESBONUS),
                        BousInAcc = acc == null ? 0 : Convert.ToInt32(acc.ACCOUNTSUM),
                        Card = DCARD == null ? 0 : Int64.Parse(DCARD.DCARDCODE),
                        ForWork = false,
                        Disable = false,
                        Comment = "",
                        CommentUp = "",
                        Discount = Convert.ToInt32(header.SALESDISC),
                        DiscountComment = ""

                    };


                    if ((header.BONUSSUM != 0)&&(header.SALESBONUS!=0))
                    {
                        string tOPENTIME = acc.OPENTIME.ToString();
                        DateTime bondatetime = new DateTime(int.Parse(tOPENTIME.Substring(0, 4)),
                            int.Parse(tOPENTIME.Substring(4, 2)),
                            int.Parse(tOPENTIME.Substring(6, 2)),
                            int.Parse(tOPENTIME.Substring(8, 2)),
                            int.Parse(tOPENTIME.Substring(10, 2)), 0);

                        newpay.Comment = string.Format("Бонусів на {0: dd.MM.yy HH:mm}\n{1:F}\nСплачено бонусами:\n{4:F}\nНараховано бонусів:\n{2:F}\nПокупатель:{3}"
                           , bondatetime
                           , (double)acc.ACCOUNTSUM / 100
                           , (double)header.BONUSSUM / 100
                           , DCARD.DCARDNAME
                           ,header.SALESBONUS/100);
                    }
                    else if (header.BONUSSUM != 0)
                    {
                        string tOPENTIME = acc.OPENTIME.ToString();
                        DateTime bondatetime = new DateTime(int.Parse(tOPENTIME.Substring(0, 4)),
                            int.Parse(tOPENTIME.Substring(4, 2)),
                            int.Parse(tOPENTIME.Substring(6, 2)),
                            int.Parse(tOPENTIME.Substring(8, 2)),
                            int.Parse(tOPENTIME.Substring(10, 2)), 0);

                        newpay.Comment = string.Format("Бонусів на {0: dd.MM.yy HH:mm}\n{1:F}\nНараховано бонусів:\n{2:F}\nПокупатель:{3}"
                            , bondatetime
                            , (double)acc.ACCOUNTSUM / 100
                            , (double)header.BONUSSUM / 100
                            , DCARD.DCARDNAME);
                    }
                    if ((int)header.SALESSUM > initrow.MinSumm && (int)header.SALESSUM < initrow.MaxSumm)
                    {
                        newpay.ForWork = true;
                    }
                    if ((DCARD != null) && (DCARD.CLNTGRPID == 4))//TKS
                    {
                        newpay.ForWork = false;
                    }
                    if ((header.SALESREFUND == 1) || (Convert.ToInt32(SALESH1_0) != 0))
                    {
                        newpay.ForWork = true;
                    }

                    focusA.tbl_Payments.InsertOnSubmit(newpay);
                    focusA.SubmitChanges(ConflictMode.ContinueOnConflict);

                    var prepareSales = (from sales in OS.GetTable<SALE>()
                                        join art in OS.GetTable<ART>()
                                        on sales.ARTID equals art.ARTID
                                        //join discoffer in OS.GetTable<DISCOFFER>()
                                        //on sales.OFFERIDFORDISC equals discoffer.DISCOFFERID
                                        where sales.SALESTAG == 0
                                        && sales.SALESFLAGS == 0
                                        //&& Convert.ToInt32(sales.FRECNUM) != 0 // Проверить зачем он
                                        && sales.SALESCANC == 0
                                        && sales.DELFLAG == 0
                                        && sales.SALESTIME == header.SALESTIME
                                        //&& Convert.ToInt64(sales.SALESTIME)>=DateTimeBegin && Convert.ToInt64(sales.SALESTIME)<= DateTimeStop    
                                        && sales.SYSTEMID == header.SYSTEMID
                                        && sales.SAREAID == header.SAREAID
                                        && sales.FRECNUM == header.FRECNUM
                                        && sales.SRECNUM == header.SRECNUM
                                        select new
                                        {
                                            DATETIME = Convert.ToInt64(sales.SALESTIME),
                                            FPNumber = iFPNumber,
                                            SESSID = sales.SESSID,
                                            SYSTEMID = sales.SYSTEMID,
                                            SAREAID = sales.SAREAID,
                                            SORT = sales.SALESNUM,
                                            Type = 0,
                                            FRECNUM = sales.FRECNUM,
                                            SRECNUM = sales.SRECNUM,
                                            Amount = sales.SALESCOUNT,
                                            SALESTYPE = sales.SALESTYPE,
                                            Amount_Status = sales.SALESTYPE == 1 ? 0 : 3,
                                            IsOneQuant = false,
                                            Price = sales.SALESPRICE,
                                            Old_Price = sales.SALESPRICE,
                                            NalogGroup = (Convert.ToInt32(sales.SALESATTRS) - 1),
                                            MemoryGoodName = false,
                                            GoodName = (sales.SALESCODE + "-" + art.ARTSNAME).Substring(0, 75),
                                            StrCode = sales.SALESCODE,
                                            PACKID = sales.PACKID,
                                            PackGuid = "",
                                            RowSum = sales.SALESCOUNT * sales.SALESPRICE,
                                            SALESCOUNT = sales.SALESCOUNT,
                                            SALESPRICE = sales.SALESPRICE,
                                            discount = sales.SALESDISC - sales.SALESBONUS,
                                            OFFERIDFORDISC = sales.OFFERIDFORDISC
                                            //DiscountComment = discoffer==null ? "": discoffer.DISCOFFERNAME

                                        }).OrderBy(x => x.DATETIME).ThenBy(x => x.SORT);

                    List<tbl_SALE> salesList = new List<tbl_SALE>();
                    int rowsum = 0;
                    foreach (var sale in prepareSales)
                    {
                        var loaded = (from tblsales in focusA.GetTable<tbl_SALE>()
                                      where tblsales.DATETIME == sale.DATETIME
                                      && tblsales.FPNumber == iFPNumber
                                      && tblsales.FRECNUM == sale.FRECNUM
                                      && tblsales.SRECNUM == sale.SRECNUM
                                      && tblsales.SAREAID == sale.SAREAID
                                      && tblsales.SESSID == sale.SESSID
                                      && tblsales.SORT == sale.SORT
                                      && tblsales.packname == sale.PACKID
                                      select tblsales).FirstOrDefault();
                        if (loaded != null)
                            continue;

                        var disccomment = (from discoffer in OS.GetTable<DISCOFFER>()
                                           where discoffer.DISCOFFERID == sale.OFFERIDFORDISC
                                           select discoffer).FirstOrDefault();
                        int discount = 0;
                        if (sale.OFFERIDFORDISC != null)
                            discount = (int)sale.discount.GetValueOrDefault();
                        tbl_SALE newsale = new tbl_SALE()
                        {
                            NumPayment = newpay.id,
                            DATETIME = sale.DATETIME,
                            FPNumber = sale.FPNumber,
                            SESSID = sale.SESSID,
                            SYSTEMID = sale.SYSTEMID,
                            SAREAID = sale.SAREAID,
                            SORT = sale.SORT,
                            Type = sale.Type,
                            FRECNUM = sale.FRECNUM,
                            SRECNUM = sale.SRECNUM,
                            Amount = (int)sale.Amount,
                            Amount_Status = sale.Amount_Status,
                            IsOneQuant = (sale.IsOneQuant),
                            Price = Convert.ToInt32(sale.Price),
                            NalogGroup = sale.NalogGroup,
                            MemoryGoodName = sale.MemoryGoodName,
                            GoodName = sale.GoodName,
                            //CommentUp
                            //CommentDown
                            StrCode = sale.StrCode.ToString(),
                            Old_Price = Convert.ToInt32(sale.Old_Price),
                            packname = sale.PACKID,
                            //PackGuid = sale.PackGuid,                            
                            RowSum = getRowSum((int)sale.SALESTYPE, (int)sale.SALESCOUNT, (int)sale.SALESPRICE) - discount,
                            ForWork = true,
                            discount = discount,
                            DiscountComment = disccomment==null? "":disccomment.DISCOFFERNAME
                        };
                        rowsum += ((int)newsale.RowSum);
                        salesList.Add(newsale);

                    }
                    int sumdisc = Math.Max(newpay.Discount.GetValueOrDefault(),newpay.PayBonus.GetValueOrDefault());
                    int razn = Math.Abs(newpay.CheckSum.GetValueOrDefault()+ sumdisc - rowsum);
                    if (razn == 0)
                    {
                        focusA.tbl_SALEs.InsertAllOnSubmit(salesList);
                    }
                    else if (razn < 5)
                    {
                        logger.Error("Разница {3} в чеке меньше 5 копеек загружаем, корректируем CheckSum. DATETIME:{0} FRECNUM:{1}, SRECNUM:{2}", newpay.DATETIME, newpay.FRECNUM, newpay.SRECNUM, razn);
                        newpay.CheckSum = (Convert.ToInt32(newpay.CheckSum) - (((int)newpay.CheckSum - Convert.ToInt32(newpay.Discount)) - (int)rowsum));
                        focusA.tbl_SALEs.InsertAllOnSubmit(salesList);

                    }
                    else
                    {
                        logger.Fatal("Разница {3} в чеке больше 5 копеек не загружаем. DATETIME:{0} FRECNUM:{1}, SRECNUM:{2}", newpay.DATETIME, newpay.FRECNUM, newpay.SRECNUM, razn);
                        focusA.tbl_Payments.DeleteOnSubmit(newpay);
                    }
                    focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
                }

            }
            LoadDataFor_tbl_SALESAfter();
        }

        public Int32 getRowSum(int SALESTYPE, int SALESCOUNT, int SALESPRICE)
        {
            if (SALESTYPE == 1)
            {
                return SALESCOUNT * SALESPRICE;
            }
            else
            {
                return Convert.ToInt32(((decimal)SALESCOUNT / 1000) * SALESPRICE);
            }
        }

        public override void LoadDataFor_tbl_Payment()
        {
            LoadDataFor_tbl_PaymentBefore();
            using (DataClassesFocusADataContext focusA = new DataClassesFocusADataContext())
            {
                var preparePayment = (from list in focusA.GetTable<tbl_Payment>()
                                      where list.FPNumber == iFPNumber
                                       && list.DATETIME >= DateTimeBegin && list.DATETIME <= DateTimeStop
                                       //&& (((list.Payment - list.Payment3) == 0) || (list.Type != 0))
                                      select list).OrderBy(x => x.DATETIME);
                int index = 0;
                var initRow = (from init in focusA.GetTable<tbl_ComInit>()
                               where init.FPNumber == iFPNumber
                               select init).FirstOrDefault();

                int PrintEvery = (int)initRow.PrintEvery;
                if (PrintEvery == 0) PrintEvery = 1;
                bool TypeEvery = (bool)initRow.TypeEvery;
                //bool disable = false;

                foreach (var rowPayment in preparePayment)
                {
                    index++;
                    if (TypeEvery)
                    {
                        if (index % PrintEvery == 0)
                            rowPayment.Disable = true;
                        else
                            rowPayment.Disable = false;
                    }
                    else
                    {
                        if (index % PrintEvery == 0)
                            rowPayment.Disable = false;
                        else
                            rowPayment.Disable = true;
                    }
                    if (!rowPayment.ForWork.GetValueOrDefault()) // если ТКС отменяем
                    {
                        rowPayment.Disable = true;
                    }
                    if (rowPayment.Type != 0)
                        rowPayment.Disable = false;
                    if (rowPayment.Payment0 != 0)
                        rowPayment.Disable = false;
                }
                focusA.SubmitChanges(ConflictMode.ContinueOnConflict);

            }
            LoadDataFor_tbl_PaymentAfter();
        }


    }
}
