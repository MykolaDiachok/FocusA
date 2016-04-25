using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using System.Data.Linq;
using CentralLib.Protocols;
using System.Threading;
using System.Data;
using System.Transactions;

namespace PrintFP.Primary
{
    public partial class Init
    {
        private Logger logger = LogManager.GetCurrentClassLogger();
        private string fpnumber, server;
        private int FPnumber;
        //private static MyEventLog eventLog1;
        private System.Object lockThis = new System.Object();
        private bool automatic, manual;
        private static ManualResetEvent shutdownEvent;

        public Init(string fpnumber, string server, bool automatic = false, bool manual = false)
        {
            NLog.GlobalDiagnosticsContext.Set("FPNumber", fpnumber);
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            //logger.Trace("Init");
            this.fpnumber = fpnumber;
            this.FPnumber = int.Parse(fpnumber);
            this.server = server;
            //eventLog1 = new MyEventLog(automatic, fpnumber);
            this.automatic = automatic;
            this.manual = manual;
            //logger.Trace("Init fp:{0}; server:{1}", fpnumber, server);
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
            UpdateStatusFP.setStatusFP(FPnumber, "waiting out");
            TimeSpan delay = new TimeSpan(0, 0, Properties.Settings.Default.TimerIntervalSec);
            shutdownEvent = new ManualResetEvent(false);
            while (shutdownEvent.WaitOne(delay, true) == false)
            {
                if (automatic)
                {
                    using (DataClasses1DataContext focus = new DataClasses1DataContext())
                    {
                        var rowinit = (from tinit in focus.GetTable<tbl_ComInit>()
                                       where tinit.FPNumber == int.Parse(fpnumber)
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
                        Do();
                        UpdateStatusFP.setStatusFP(FPnumber, "waiting...");
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                        UpdateStatusFP.setStatusFP(FPnumber, "Была ошибка, waiting...");
                        Thread.Sleep(Properties.Settings.Default.TimerIntervalSec * 1000);
                    }
                }
                //logger.Trace("lockthis out {0}", DateTime.Now);
            }
            UpdateStatusFP.setStatusFP(FPnumber, "Exit...");
        }


        public void Do()
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            setStatusFP("in work");
            using (DataClasses1DataContext _focusA = new DataClasses1DataContext())
            //using (var trans = new TransactionScope(TransactionScopeOption.Required,new TransactionOptions{IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted,Timeout= new TimeSpan(0,0,25)}))
            {

                _focusA.CommandTimeout = 30;
                //_focusA.Transaction = _focusA.Connection.BeginTransaction(IsolationLevel.ReadCommitted);

                //eventLog1.WriteEntry("start tbl_ComInit");
                Table<tbl_ComInit> tablePayment = _focusA.GetTable<tbl_ComInit>();
                var initRow = (from list in tablePayment
                               where list.Init == true
                               && list.CompName.ToLower() == server.ToLower()
                               && list.FPNumber == int.Parse(fpnumber)
                               select list).FirstOrDefault();
                //logger.Trace("cominit:{0}",comInit.ToString());
                //eventLog1.WriteEntry("start foreach initRow");
                //foreach (var initRow in comInit)
                if (initRow != null)
                {
                    DateTime tBegin = DateTime.ParseExact(initRow.DateTimeBegin.ToString(), "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).AddHours(-1);
                    DateTime tEnd = DateTime.ParseExact(initRow.DateTimeStop.ToString(), "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                    DateTime worktime = DateTime.Now.AddSeconds((double)initRow.DeltaTime);
                    // logger.Trace("Fp={0};Begin:{1};End:{2}", initRow.FPNumber, tBegin, getintDateTime(worktime));
                    //logger.Trace("start tbl_Operation");
                    Table<tbl_Operation> tableOperation = _focusA.GetTable<tbl_Operation>();
                    var operation = (from op in tableOperation
                                     where op.FPNumber == (int)initRow.FPNumber
                                     && !op.Closed && !(bool)op.Disable
                                     //&& op.DateTime >= initRow.DateTimeBegin && op.DateTime <= initRow.DateTimeStop
                                     && op.DateTime >= getintDateTime(tBegin) && op.DateTime <= initRow.DateTimeStop
                                      && op.DateTime <= getintDateTime(worktime)
                                     //TODO добавить определение текущего времени и разницы
                                     select op).OrderBy(o => o.DateTime).ThenBy(o => o.Operation).FirstOrDefault();
                    //logger.Trace("init:{0}", worktime);
                    //eventLog1.WriteEntry("Get operation");
                    if (operation != null)
                    {

                        operation.InWork = true;

                        //try
                        //{
                        //    //using (Protocol_EP11 pr = new Protocol_EP11(initRow.Port))
                        BaseProtocol searchProtocol;
                        try
                        {
                            if (initRow.Version == "ЕП-06")
                            {
                                if ((initRow.MoxaIP.Trim().Length != 0) && ((int)initRow.MoxaPort > 0))
                                {
                                    searchProtocol = new Protocol_EP06(initRow.MoxaIP, (int)initRow.MoxaPort, FPnumber);
                                }
                                else
                                {
                                    searchProtocol = new Protocol_EP06(initRow.Port, FPnumber);
                                }
                            }
                            else
                            {
                                setStatusFP("start get protocols....");
                                if ((initRow.MoxaIP.Trim().Length != 0) && ((int)initRow.MoxaPort > 0))
                                {
                                    searchProtocol = (BaseProtocol)SingletonProtocol.Instance(initRow.MoxaIP, (int)initRow.MoxaPort, FPnumber).GetProtocols();
                                }
                                else
                                    searchProtocol = (BaseProtocol)SingletonProtocol.Instance(initRow.Port, FPnumber).GetProtocols();
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex);
                            setStatusFP(string.Format("Error in protocol:{0}", ex.Message));

                            initRow.Error = true;
                            initRow.ErrorInfo = "Error info:" + "Fatal crash app;" + ex.Message;
                            initRow.ErrorCode = 9999; // ошибка которая привела к большому падению
                            _focusA.SubmitChanges(ConflictMode.ContinueOnConflict);

                            throw new ApplicationException(initRow.ErrorInfo);
                        }
                        if (searchProtocol == null)
                        {
                            initRow.Error = true;
                            initRow.ErrorInfo = "Error info:" + "Fatal crash app; Протокол не определен";
                            setStatusFP(initRow.ErrorInfo);
                            initRow.ErrorCode = 9999; // ошибка которая привела к большому падению
                            _focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
                            throw new ApplicationException(initRow.ErrorInfo);
                        }

                        using (var pr = searchProtocol)
                        {
                            try
                            {
                                setInfo(pr, operation.Operation, operation.DateTime);
                            }
                            catch (Exception ex)
                            {
                                logger.Warn(ex);
                            }
                            setStatusFP(searchProtocol.GetType().ToString() + operation.Operation.ToString());

                            if (!InitialSet(_focusA, initRow, pr, operation))
                            {
                                setStatusFP(string.Format("Problem init!!!! Operation={0},id={1}", operation.Operation, operation.id));
                                return;
                            }
                            setStatusFP(string.Format("Operation={0},id={1}", operation.Operation, operation.id));
                            if (operation.Operation == 3) // set cachier
                            {

                                var tblCashier = getCashier(_focusA, operation);
                                logger.Trace(string.Format("set cachier:{0}", tblCashier.Name_Cashier));
                                pr.FPRegisterCashier(0, tblCashier.Name_Cashier);
                                tblCashier.ByteReserv = pr.ByteReserv;
                                tblCashier.ByteResult = pr.ByteResult;
                                tblCashier.ByteStatus = pr.ByteStatus;

                                //tblCashier.Error = pr.er
                            }
                            else if (operation.Operation == 10) //in money
                            {
                                var tblCashIO = getCashIO(_focusA, operation);
                                setStatusFP(string.Format("in money:{0}, fp:{1}", tblCashIO.Money, 30000));
                                //pr.FPCashIn((uint)tblCashIO.Money);
                                pr.FPCashIn(30000);
                                //tblCashIO.ByteReserv = pr.ByteReserv;
                                tblCashIO.MoneyFP = 30000;
                                tblCashIO.ByteResult = pr.ByteResult;
                                tblCashIO.ByteStatus = pr.ByteStatus;
                                tblCashIO.Error = !pr.statusOperation;
                                //var tbl
                            }
                            else if (operation.Operation == 15) //out money
                            {
                                UInt32 rest = pr.GetMoneyInBox();

                                var tblCashIO = getCashIO(_focusA, operation);
                                var outMoney = Math.Min(rest, (uint)tblCashIO.Money);
                                setStatusFP(string.Format("out money. in base:{0}; in box{1}, make:{2}", tblCashIO.Money, rest, outMoney));
                                pr.FPCashOut(outMoney);
                                tblCashIO.MoneyFP = (int)outMoney;
                                //tblCashIO.ByteReserv = pr.ByteReserv;
                                tblCashIO.ByteResult = pr.ByteResult;
                                tblCashIO.ByteStatus = pr.ByteStatus;
                                tblCashIO.Error = !pr.statusOperation;
                            }
                            else if (operation.Operation == 12) //check
                            {
                                Table<tbl_Payment> tblPayment = _focusA.GetTable<tbl_Payment>();
                                Table<tbl_SALE> tblSales = _focusA.GetTable<tbl_SALE>();
                                var headCheck = (from table in tblPayment
                                                 where table.FPNumber == operation.FPNumber
                                                 && table.DATETIME == operation.DateTime
                                                 && table.id == operation.NumSlave
                                                 && table.Operation == operation.Operation
                                                 select table).FirstOrDefault();
                                if (headCheck == null)
                                {
                                    setStatusFP(string.Format("Строка заголовки чека пустая!!!! Operation={0},id=", operation.Operation, operation.id));
                                    string errorinfo = "Строка заголовки чека пустая";
                                    initRow.ErrorInfo = errorinfo;
                                    _focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
                                    throw new ApplicationException(errorinfo);
                                }
                                setStatusFP(string.Format("HEAD CHECK!!!! Operation={0},id={1}, row count={2}", operation.Operation, operation.id, headCheck.RowCount));
                                headCheck.ForWork = true;
                                var tableCheck = (from tableSales in tblSales
                                                  where tableSales.DATETIME == headCheck.DATETIME
                                                  && tableSales.FPNumber == headCheck.FPNumber
                                                  && tableSales.FRECNUM == headCheck.FRECNUM
                                                  && tableSales.SAREAID == headCheck.SAREAID
                                                  && tableSales.SESSID == headCheck.SESSID
                                                  && tableSales.SRECNUM == headCheck.SRECNUM
                                                  && tableSales.NumPayment == headCheck.id
                                                  select tableSales);
                                //logger.Trace("Check begin #{0}", headCheck.id);
                                //List<string, int> listGoodsName = new List<string, int>();
                                Dictionary<ulong, int> listgoods = new Dictionary<ulong, int>();
                                if (tableCheck.Count() != 0)
                                {
                                    if (headCheck.CommentUp.Length != 0)
                                    {
                                        string str = headCheck.CommentUp;
                                        string[] strs = str.Split('\n');
                                        foreach (var com in strs)
                                        {
                                            if (com.Trim().Length > 0)
                                                pr.FPCommentLine(com);
                                        }
                                    }
                                    UInt16 index = 0;
                                    foreach (var rowCheck in tableCheck)
                                    {
                                        //string forPrint = rowCheck.GoodName;                                        
                                        ReceiptInfo rowSum;
                                        //if ((listgoods.ContainsKey((ulong)rowCheck.packname))&&(listgoods[(ulong)rowCheck.packname]!= rowCheck.Price))
                                        ulong packcode = (ulong)rowCheck.packname.GetValueOrDefault();
                                        var tlistg = (from listg in listgoods
                                                      where listg.Key == (ulong)rowCheck.packname
                                                      && listg.Value == rowCheck.Price
                                                      select listg);
                                        if (tlistg.Count() == 0)
                                        {
                                            if ((listgoods.ContainsKey((ulong)rowCheck.packname)))
                                            {
                                                packcode = (ulong)rowCheck.packname + ulong.Parse(rowCheck.StrCode) * index * 1000000;
                                            }
                                            else
                                            {
                                                listgoods.Add((ulong)rowCheck.packname, rowCheck.Price);
                                            }
                                        }
                                        Art art = new Art(int.Parse(rowCheck.StrCode), rowCheck.GoodName, packcode, (ushort)rowCheck.NalogGroup, rowCheck.FPNumber, _focusA);
                                        setStatusFP(string.Format("Check #{0} row#{1} name:{2} pr:{3}", headCheck.id, rowCheck.SORT, art.ARTNAME, rowCheck.Price));
                                        rowSum = pr.FPSaleEx((ushort)rowCheck.Amount, (byte)rowCheck.Amount_Status, false, rowCheck.Price, art.NalogGroup, false, art.ARTNAME, art.PackCode);
                                        DiscountInfo rowDiscount;
                                        if (rowCheck.discount.GetValueOrDefault() != 0)
                                            rowDiscount = pr.Discount(CentralLib.Helper.FPDiscount.AbsoluteDiscountMarkupAtLastGoods, (short)rowCheck.discount, rowCheck.DiscountComment);

                                        rowCheck.ByteReserv = pr.ByteReserv;
                                        rowCheck.ByteResult = pr.ByteResult;
                                        rowCheck.ByteStatus = pr.ByteStatus;
                                        rowCheck.Error = !pr.statusOperation;
                                        rowCheck.FPSum = rowSum.CostOfGoodsOrService;
                                        headCheck.FPSumm = rowSum.SumAtReceipt;
                                        var razn = (rowCheck.RowSum.GetValueOrDefault() + rowCheck.discount.GetValueOrDefault()) - rowSum.CostOfGoodsOrService;
                                        if (Math.Abs(razn) > 5)
                                        {
                                            string errorinfo = String.Format("Отличается сумма по строке чека, нужно {0}, в аппарате {1}. Строка:{2} Чек:{3}", rowCheck.RowSum, rowSum.CostOfGoodsOrService, rowCheck.id, rowCheck.NumPayment);
                                            setStatusFP(string.Format("{2}!!!! Operation={0},id={1}", operation.Operation, operation.id, errorinfo));
                                            initRow.ErrorInfo = errorinfo;
                                            _focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
                                            logger.Error(errorinfo);
                                            throw new ApplicationException(errorinfo);
                                        }
                                        index++;
                                    }

                                    DiscountInfo disc = new DiscountInfo();
                                    if ((headCheck.PayBonus.GetValueOrDefault() != 0) || (headCheck.Discount.GetValueOrDefault() != 0))
                                    {
                                        int val = Math.Max(headCheck.PayBonus.GetValueOrDefault(), headCheck.Discount.GetValueOrDefault());

                                        disc = pr.Discount(CentralLib.Helper.FPDiscount.AbsoluteDiscountMarkupAtIntermediateSum, (short)(val), "");
                                        headCheck.FPSumm = disc.SumOfReceipt;
                                    }

                                    if (headCheck.Comment.Length != 0)
                                    {
                                        string str = headCheck.Comment;
                                        string[] strs = str.Split('\n');
                                        foreach (var com in strs)
                                        {
                                            if (com.Trim().Length > 0)
                                                pr.FPCommentLine(com);
                                        }
                                    }

                                    int checkRazn = headCheck.FPSumm.GetValueOrDefault() - headCheck.CheckSum.GetValueOrDefault();
                                    if (Math.Abs(checkRazn) > 5)
                                    {
                                        string errorinfo = String.Format("Отличается сумма чека, нужно {0}, в аппарате {1}. id:{2}", headCheck.CheckSum, headCheck.FPSumm, headCheck.id);
                                        setStatusFP(string.Format("{2}!!!! Operation={0},id={1}", operation.Operation, operation.id, errorinfo));
                                        _focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
                                        logger.Error(errorinfo);
                                        throw new ApplicationException(errorinfo);
                                    }
                                    if (headCheck.Payment0 > 0)
                                    {
                                        pr.FPPayment(0, (uint)headCheck.Payment0, false, true);
                                        setStatusFP(string.Format("Check #{0} Payment0:{1}", headCheck.id, (uint)headCheck.Payment0));
                                    }
                                    if (headCheck.Payment1 > 0)
                                    {
                                        pr.FPPayment(1, (uint)headCheck.Payment1, false, true);
                                        setStatusFP(string.Format("Check #{0} Payment1:{1}", headCheck.id, (uint)headCheck.Payment1));
                                    }
                                    if (headCheck.Payment2 > 0)
                                    {
                                        pr.FPPayment(2, (uint)headCheck.Payment2, false, true);
                                        setStatusFP(string.Format("Check #{0} Payment2:{1}", headCheck.id, (uint)headCheck.Payment2));
                                    }
                                    if (headCheck.Payment3 > 0)
                                    {
                                        pr.FPPayment(3, (uint)headCheck.Payment3 + (uint)checkRazn, false, true);
                                        setStatusFP(string.Format("Check #{0} Payment3:{1}", headCheck.id, (uint)headCheck.Payment3));
                                    }
                                    headCheck.ByteReserv = pr.ByteReserv;
                                    headCheck.ByteResult = pr.ByteResult;
                                    headCheck.ByteStatus = pr.ByteStatus;
                                    headCheck.Error = !pr.statusOperation;
                                    headCheck.CheckClose = true;
                                }
                                //logger.Trace("Check close #{0}", headCheck.id);
                                //pr.FPPayment();
                            }
                            else if (operation.Operation == 5) //payment
                            {
                                Table<tbl_Payment> tblPayment = _focusA.GetTable<tbl_Payment>();
                                Table<tbl_SALE> tblSales = _focusA.GetTable<tbl_SALE>();
                                var headCheck = (from table in tblPayment
                                                 where table.FPNumber == operation.FPNumber
                                                 && table.DATETIME == operation.DateTime
                                                 && table.id == operation.NumSlave
                                                 && table.Operation == operation.Operation
                                                 select table).FirstOrDefault();
                                if (headCheck == null)
                                {
                                    setStatusFP(string.Format("Строка заголовки чека пустая!!!! Operation={0},id=", operation.Operation, operation.id));
                                    string errorinfo = "Строка заголовки чека пустая";
                                    initRow.ErrorInfo = errorinfo;
                                    _focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
                                    throw new ApplicationException(errorinfo);
                                }
                                setStatusFP(string.Format("HEAD CHECK!!!! Operation={0},id={1}, row count={2}", operation.Operation, operation.id, headCheck.RowCount));
                                headCheck.ForWork = true;
                                var tableCheck = (from tableSales in tblSales
                                                  where tableSales.DATETIME == headCheck.DATETIME
                                                  && tableSales.FPNumber == headCheck.FPNumber
                                                  && tableSales.FRECNUM == headCheck.FRECNUM
                                                  && tableSales.SAREAID == headCheck.SAREAID
                                                  && tableSales.SESSID == headCheck.SESSID
                                                  && tableSales.SRECNUM == headCheck.SRECNUM
                                                  && tableSales.NumPayment == headCheck.id
                                                  select tableSales);
                                //logger.Trace("Check begin #{0}", headCheck.id);
                                //List<string, int> listGoodsName = new List<string, int>();
                                Dictionary<ulong, int> listgoods = new Dictionary<ulong, int>();
                                if (tableCheck.Count() != 0)
                                {
                                    if (headCheck.CommentUp.Length != 0)
                                    {
                                        string str = headCheck.CommentUp;
                                        string[] strs = str.Split('\n');
                                        foreach (var com in strs)
                                        {
                                            if (com.Trim().Length > 0)
                                                pr.FPCommentLine(com);
                                        }
                                    }
                                    UInt16 index = 0;
                                    foreach (var rowCheck in tableCheck)
                                    {
                                        //string forPrint = rowCheck.GoodName;                                        
                                        ReceiptInfo rowSum;
                                        //if ((listgoods.ContainsKey((ulong)rowCheck.packname))&&(listgoods[(ulong)rowCheck.packname]!= rowCheck.Price))
                                        ulong packcode = (ulong)rowCheck.packname.GetValueOrDefault();
                                        var tlistg = (from listg in listgoods
                                                      where listg.Key == (ulong)rowCheck.packname
                                                      && listg.Value == rowCheck.Price
                                                      select listg);
                                        if (tlistg.Count() == 0)
                                        {
                                            if ((listgoods.ContainsKey((ulong)rowCheck.packname)))
                                            {
                                                packcode = (ulong)rowCheck.packname + ulong.Parse(rowCheck.StrCode) * index * 1000000;
                                            }
                                            else
                                            {
                                                listgoods.Add((ulong)rowCheck.packname, rowCheck.Price);
                                            }
                                        }
                                        Art art = new Art(int.Parse(rowCheck.StrCode), rowCheck.GoodName, packcode, (ushort)rowCheck.NalogGroup, rowCheck.FPNumber, _focusA);
                                        setStatusFP(string.Format("Check #{0} row#{1} name:{2} pr:{3}", headCheck.id, rowCheck.SORT, art.ARTNAME, rowCheck.Price));
                                        rowSum = pr.FPPayMoneyEx((ushort)rowCheck.Amount, (byte)rowCheck.Amount_Status, false, rowCheck.Price, art.NalogGroup, false, art.ARTNAME, art.PackCode);
                                        DiscountInfo rowDiscount;
                                        if (rowCheck.discount.GetValueOrDefault() != 0)
                                            rowDiscount = pr.Discount(CentralLib.Helper.FPDiscount.AbsoluteDiscountMarkupAtLastGoods, (short)rowCheck.discount, rowCheck.DiscountComment);

                                        rowCheck.ByteReserv = pr.ByteReserv;
                                        rowCheck.ByteResult = pr.ByteResult;
                                        rowCheck.ByteStatus = pr.ByteStatus;
                                        rowCheck.Error = !pr.statusOperation;
                                        rowCheck.FPSum = rowSum.CostOfGoodsOrService;
                                        headCheck.FPSumm = rowSum.SumAtReceipt;
                                        int razn = (rowCheck.RowSum.GetValueOrDefault() + rowCheck.discount.GetValueOrDefault()) - rowSum.CostOfGoodsOrService;
                                        if (Math.Abs(razn) > 5)
                                        {
                                            string errorinfo = String.Format("Отличается сумма по строке чека, нужно {0}, в аппарате {1}. Строка:{2} Чек:{3}", rowCheck.RowSum, rowSum.CostOfGoodsOrService, rowCheck.id, rowCheck.NumPayment);
                                            setStatusFP(string.Format("{2}!!!! Operation={0},id={1}", operation.Operation, operation.id, errorinfo));
                                            initRow.ErrorInfo = errorinfo;
                                            _focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
                                            logger.Error(errorinfo);
                                            throw new ApplicationException(errorinfo);
                                        }
                                        index++;
                                    }

                                    DiscountInfo disc = new DiscountInfo();
                                    if ((headCheck.PayBonus.GetValueOrDefault() != 0) || (headCheck.Discount.GetValueOrDefault() != 0))
                                    {
                                        int val = Math.Max(headCheck.PayBonus.GetValueOrDefault(), headCheck.Discount.GetValueOrDefault());

                                        disc = pr.Discount(CentralLib.Helper.FPDiscount.AbsoluteDiscountMarkupAtIntermediateSum, (short)(val), "");
                                        headCheck.FPSumm = disc.SumOfReceipt;
                                    }

                                    if (headCheck.Comment.Length != 0)
                                    {
                                        string str = headCheck.Comment;
                                        string[] strs = str.Split('\n');
                                        foreach (var com in strs)
                                        {
                                            if (com.Trim().Length > 0)
                                                pr.FPCommentLine(com);
                                        }
                                    }

                                    int checkRazn = headCheck.FPSumm.GetValueOrDefault() - headCheck.CheckSum.GetValueOrDefault();
                                    if (Math.Abs(checkRazn) > 5)
                                    {
                                        string errorinfo = String.Format("Отличается сумма чека, нужно {0}, в аппарате {1}. id:{2}", headCheck.CheckSum, headCheck.FPSumm, headCheck.id);
                                        setStatusFP(string.Format("{2}!!!! Operation={0},id={1}", operation.Operation, operation.id, errorinfo));
                                        _focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
                                        logger.Error(errorinfo);
                                        throw new ApplicationException(errorinfo);
                                    }
                                    if (headCheck.Payment0 > 0)
                                    {
                                        pr.FPPayment(0, (uint)headCheck.Payment0, false, true);
                                        setStatusFP(string.Format("Check #{0} Payment0:{1}", headCheck.id, (uint)headCheck.Payment0));
                                    }
                                    if (headCheck.Payment1 > 0)
                                    {
                                        pr.FPPayment(1, (uint)headCheck.Payment1, false, true);
                                        setStatusFP(string.Format("Check #{0} Payment1:{1}", headCheck.id, (uint)headCheck.Payment1));
                                    }
                                    if (headCheck.Payment2 > 0)
                                    {
                                        pr.FPPayment(2, (uint)headCheck.Payment2, false, true);
                                        setStatusFP(string.Format("Check #{0} Payment2:{1}", headCheck.id, (uint)headCheck.Payment2));
                                    }
                                    if (headCheck.Payment3 > 0)
                                    {
                                        pr.FPPayment(3, (uint)headCheck.Payment3 + (uint)checkRazn, false, true);
                                        setStatusFP(string.Format("Check #{0} Payment3:{1}", headCheck.id, (uint)headCheck.Payment3));
                                    }
                                    headCheck.ByteReserv = pr.ByteReserv;
                                    headCheck.ByteResult = pr.ByteResult;
                                    headCheck.ByteStatus = pr.ByteStatus;
                                    headCheck.Error = !pr.statusOperation;
                                    headCheck.CheckClose = true;
                                }
                                //logger.Trace("Check close #{0}", headCheck.id);
                                //pr.FPPayment();
                            }
                            else if (operation.Operation == 35) //X
                            {
                                logger.Trace("print X");
                                pr.FPDayReport();
                            }
                            else if (operation.Operation == 39) //Z
                            {
                                //pr.
                                pr.setFPCplCutter(true);
                                pr.FPNullCheck();
                                UInt32 rest = pr.GetMoneyInBox();
                                if (rest != 0)
                                {
                                    logger.Trace("out money:{0}", rest);
                                    pr.FPCashOut(rest);
                                }
                                setInfo(pr, operation.Operation, operation.DateTime);
                                Thread.Sleep(30 * 1000);
                                //var status = pr.get
                                logger.Trace("print Z");
                                var rreport = pr.FPDayClrReport();
                                if (rreport.statusOperation)
                                {
                                    operation.CurentDateTime = DateTime.Now;
                                    operation.ByteStatus = rreport.ByteStatus;
                                    operation.ByteReserv = rreport.ByteReserv;
                                    operation.ByteResult = rreport.ByteResult;
                                    operation.Error = !rreport.statusOperation;
                                    if (rreport.statusOperation)
                                        operation.Closed = true;
                                    operation.InWork = true;
                                    _focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
                                }
                                if (DateTime.Now.Day == 1)
                                {
                                    var now = DateTime.Now.AddMonths(-1);
                                    var startOfMonth = new DateTime(now.Year, now.Month, 1);
                                    var DaysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                                    var lastDay = new DateTime(now.Year, now.Month, DaysInMonth);
                                    var report = pr.FPPeriodicReport(0, startOfMonth, lastDay);
                                }
                                if (DateTime.Now.Day == 11)
                                {
                                    var now = DateTime.Now;
                                    var startOfMonth = new DateTime(now.Year, now.Month, 1);
                                    var lastDay = new DateTime(now.Year, now.Month, 10);
                                    var report = pr.FPPeriodicReport(0, startOfMonth, lastDay);
                                }
                                if (DateTime.Now.Day == 21)
                                {
                                    var now = DateTime.Now;
                                    var startOfMonth = new DateTime(now.Year, now.Month, 1);
                                    var lastDay = new DateTime(now.Year, now.Month, 20);
                                    var report = pr.FPPeriodicReport(0, startOfMonth, lastDay);
                                }
                                pr.setFPCplCutter(false);
                                deleteArt();
                            }
                            else if (operation.Operation == 40) //periodic report
                            {
                                pr.setFPCplCutter(true);
                                if (DateTime.Now.Day < 10)
                                {
                                    var now = DateTime.Now.AddMonths(-1);
                                    var startOfMonth = new DateTime(now.Year, now.Month, 1);
                                    var DaysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                                    var lastDay = new DateTime(now.Year, now.Month, DaysInMonth);
                                    pr.FPPeriodicReport(0, startOfMonth, lastDay);
                                }
                                //if (DateTime.Now.Day >= 10)
                                //{
                                {
                                    var now = DateTime.Now;
                                    var startOfMonth = new DateTime(now.Year, now.Month, 1);
                                    var lastDay = new DateTime(now.Year, now.Month, 10);
                                    pr.FPPeriodicReport(0, startOfMonth, lastDay);
                                }
                                pr.setFPCplCutter(false);
                                //}
                                //logger.Trace("print periodic report");
                            }

                            setStatuses(operation, initRow, pr);
                            //initRow.DateTimeSyncFP = DateTime.Now;
                            _focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
                            //    if (initRow.Error)
                            //    {

                            //    }

                        }
                        //}
                        //catch (Exception ex)
                        //{
                        //    eventLog1.WriteEntry("Error fiscal printer:" + ex.Message);
                        //    initRow.Error = true;
                        //    initRow.ErrorInfo = "Error info:" + "Fatal crash app;" + ex.Message;
                        //    initRow.ErrorCode = 9999; // ошибка которая привела к большому падению
                        //    _focusA.SubmitChanges();
                        //    //trans.Complete();
                        //}

                    }
                    //logger.Trace("out Get operation");
                    //initRow.DateTimeSyncFP = DateTime.Now;
                    //_focusA.SubmitChanges();
                    //trans.Complete();
                    //_focusA.Transaction.Commit();
                }

            }
            //eventLog1.WriteEntry("out init");
            setStatusFP("out work, waiting...");
        }


        private void setInfo(BaseProtocol pr, int Operation, long datetime)
        {
            var dayReport = pr.dayReport;
            using (DataClasses1DataContext focus = new DataClasses1DataContext())
            {
                var rowinfo = (from row in focus.GetTable<tbl_Info>()
                               where row.FPNumber == FPnumber
                               && row.NumZReport == dayReport.CurrentNumberOfZReport
                               && row.LastDateZReport == dayReport.dtDateOfTheLastDailyReport
                               select row).FirstOrDefault();
                if (rowinfo == null)
                {
                    rowinfo = new tbl_Info() { FPNumber = FPnumber };
                    focus.tbl_Infos.InsertOnSubmit(rowinfo);
                }
                rowinfo.Operation = Operation;
                rowinfo.DateTime = datetime; //long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
                rowinfo.NumZReport = dayReport.CurrentNumberOfZReport;
                rowinfo.SaleCheckNumber = dayReport.CounterOfSaleReceipts;
                rowinfo.PayCheckNumber = dayReport.CounterOfPayoutReceipts;
                rowinfo.LastDateZReport = dayReport.dtDateOfTheLastDailyReport;
                rowinfo.DateTimeOfEndOfShift = dayReport.DateTimeOfEndOfShift;
                uint MoneyInBox = 0;
                try
                {
                    MoneyInBox = pr.GetMoneyInBox();
                    rowinfo.MoneyInBox = MoneyInBox;
                }
                catch (Exception ex)
                {
                    logger.Warn(ex);
                }

                rowinfo.DiscountSale = dayReport.DailyDiscountBySale;
                rowinfo.ExtraChargeSale = dayReport.DailyMarkupBySale;
                rowinfo.DiscountPay = dayReport.DailyDiscountByPayouts;
                rowinfo.ExtraChargePay = dayReport.DailyMarkupByPayouts;
                rowinfo.AvansSum = dayReport.DailySumOfServiceCashEntering;
                rowinfo.PaymentSum = dayReport.DailySumOfServiceCashGivingOut;

                rowinfo.TurnSaleTax_A = dayReport.CounterOfSalesByTaxGroupsAndTypesOfPayments.TaxA;
                rowinfo.TurnSaleTax_B = dayReport.CounterOfSalesByTaxGroupsAndTypesOfPayments.TaxB;
                rowinfo.TurnSaleTax_C = dayReport.CounterOfSalesByTaxGroupsAndTypesOfPayments.TaxC;
                rowinfo.TurnSaleTax_D = dayReport.CounterOfSalesByTaxGroupsAndTypesOfPayments.TaxD;
                rowinfo.TurnSaleTax_E = dayReport.CounterOfSalesByTaxGroupsAndTypesOfPayments.TaxE;
                rowinfo.TurnSaleTax_F = dayReport.CounterOfSalesByTaxGroupsAndTypesOfPayments.TaxF;
                rowinfo.TurnSaleCard = dayReport.CounterOfSalesByTaxGroupsAndTypesOfPayments.Card;
                rowinfo.TurnSaleCash = dayReport.CounterOfSalesByTaxGroupsAndTypesOfPayments.Cash;
                rowinfo.TurnSaleCredit = dayReport.CounterOfSalesByTaxGroupsAndTypesOfPayments.Credit;
                rowinfo.TurnSaleCheck = dayReport.CounterOfSalesByTaxGroupsAndTypesOfPayments.Check;

                rowinfo.TurnPayTax_A = dayReport.CountersOfPayoutByTaxGroupsAndTypesOfPayments.TaxA;
                rowinfo.TurnPayTax_B = dayReport.CountersOfPayoutByTaxGroupsAndTypesOfPayments.TaxB;
                rowinfo.TurnPayTax_C = dayReport.CountersOfPayoutByTaxGroupsAndTypesOfPayments.TaxC;
                rowinfo.TurnPayTax_D = dayReport.CountersOfPayoutByTaxGroupsAndTypesOfPayments.TaxD;
                rowinfo.TurnPayTax_E = dayReport.CountersOfPayoutByTaxGroupsAndTypesOfPayments.TaxE;
                rowinfo.TurnPayTax_F = dayReport.CountersOfPayoutByTaxGroupsAndTypesOfPayments.TaxF;
                rowinfo.TurnPayCard = dayReport.CountersOfPayoutByTaxGroupsAndTypesOfPayments.Card;
                rowinfo.TurnPayCash = dayReport.CountersOfPayoutByTaxGroupsAndTypesOfPayments.Cash;
                rowinfo.TurnPayCheck = dayReport.CountersOfPayoutByTaxGroupsAndTypesOfPayments.Check;
                rowinfo.TurnPayCredit = dayReport.CountersOfPayoutByTaxGroupsAndTypesOfPayments.Credit;
                rowinfo.DateTimeUpdate = DateTime.Now;

                focus.SubmitChanges(ConflictMode.ContinueOnConflict);
            }
        }


        private void setStatusFP(string infoStatus)
        {
            PrintFP.UpdateStatusFP.setStatusFP(FPnumber, infoStatus);
        }

        private void deleteArt()
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            using (DataClasses1DataContext focusA = new DataClasses1DataContext())
            {
                Table<tbl_ART> tbl_ART = focusA.GetTable<tbl_ART>();
                tbl_ART.DeleteAllOnSubmit(tbl_ART.AsEnumerable().Where(r => r.FPNumber == int.Parse(fpnumber)).ToList());
                focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
            }
        }

        private tbl_CashIO getCashIO(DataClasses1DataContext _focusA, tbl_Operation tOp)
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            Table<tbl_CashIO> tableCashIO = _focusA.GetTable<tbl_CashIO>();
            var tReturn = (from table in tableCashIO
                           where table.FPNumber == tOp.FPNumber
                           && table.DATETIME == tOp.DateTime
                           && table.id == tOp.NumSlave
                           && table.Operation == tOp.Operation
                           select table).FirstOrDefault();
            return tReturn;
        }


        /// <summary>
        /// Получение кассира по операции
        /// </summary>
        /// <param name="_focusA"></param>
        /// <param name="tOp"></param>
        /// <returns></returns>
        private tbl_Cashier getCashier(DataClasses1DataContext _focusA, tbl_Operation tOp)
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            Table<tbl_Cashier> tableCashier = _focusA.GetTable<tbl_Cashier>();
            var tReturn = (from table in tableCashier
                           where table.FPNumber == tOp.FPNumber
                           && table.DATETIME == tOp.DateTime
                           && table.id == tOp.NumSlave
                           && table.Operation == tOp.Operation
                           select table).FirstOrDefault();
            return tReturn;
        }


        /// <summary>
        /// Установка общих статусов
        /// </summary>
        /// <param name="tOp"></param>
        /// <param name="tInit"></param>
        /// <param name="pr"></param>
        private void setStatuses(tbl_Operation tOp, tbl_ComInit tInit, BaseProtocol pr)
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            tInit.CurrentSystemDateTime = DateTime.Now;
            tInit.ByteStatus = pr.ByteStatus;
            tInit.ByteStatusInfo = pr.structStatus.ToString();
            tInit.ByteReserv = pr.ByteReserv;
            tInit.ByteReservInfo = pr.structReserv.ToString();
            tInit.ByteResult = pr.ByteResult;
            tInit.ByteResultInfo = pr.structResult.ToString();
            tInit.Error = !pr.statusOperation;
            tInit.ErrorInfo = pr.errorInfo;

            tOp.CurentDateTime = DateTime.Now;
            tOp.ByteStatus = pr.ByteStatus;
            tOp.ByteReserv = pr.ByteReserv;
            tOp.ByteResult = pr.ByteResult;
            tOp.Error = !pr.statusOperation;
            if (pr.statusOperation)
                tOp.Closed = true;
            tOp.InWork = true;

        }

        /// <summary>
        /// Первичная инициализация и подготовка ФР
        /// </summary>
        /// <param name="_focusA">база</param>
        /// <param name="initRow">строка инициализации</param>
        /// <param name="pr">протокол обмена</param>
        /// /// <param name="operation">текущая операция</param>
        private bool InitialSet(DataClasses1DataContext _focusA, tbl_ComInit initRow, BaseProtocol pr, tbl_Operation operation)
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            initRow.Error = false;
            initRow.ErrorInfo = "";
            initRow.ErrorCode = 0;
            var status = pr.status;
            pr.setFPCplCutter(false);
            //var dayReport = pr.dayReport;
            PapStat papstatus = pr.papStat;
            if (papstatus != null)
                initRow.PapStat = papstatus.ToString();
            if ((papstatus != null) && (papstatus.ControlPaperIsAlmostEnded != null) && ((bool)papstatus.ControlPaperIsAlmostEnded))
            {
                initRow.Error = true;
                initRow.ErrorInfo = papstatus.ToString();
                initRow.CurrentSystemDateTime = DateTime.Now;
                initRow.ByteStatus = pr.ByteStatus;
                initRow.ByteStatusInfo = pr.structStatus.ToString();
                initRow.ByteReserv = pr.ByteReserv;
                initRow.ByteReservInfo = pr.structReserv.ToString();
                initRow.ByteResult = pr.ByteResult;
                initRow.ByteResultInfo = pr.structResult.ToString();

                _focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
                pr.showTopString("Контрольная лента закончилась!");

                return false;
            }
            if ((papstatus != null) && (papstatus.ReceiptPaperIsAlmostEnded != null) && ((bool)papstatus.ReceiptPaperIsAlmostEnded))
            {
                initRow.Error = true;
                initRow.ErrorInfo = papstatus.ToString();
                initRow.CurrentSystemDateTime = DateTime.Now;
                initRow.ByteStatus = pr.ByteStatus;
                initRow.ByteStatusInfo = pr.structStatus.ToString();
                initRow.ByteReserv = pr.ByteReserv;
                initRow.ByteReservInfo = pr.structReserv.ToString();
                initRow.ByteResult = pr.ByteResult;
                initRow.ByteResultInfo = pr.structResult.ToString();
                _focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
                pr.showTopString("Чековая лента закончилась!");
                return false;
            }

            strByteStatus sStatus = pr.structStatus;
            //#if (!DEBUG)
            //                            initRow.FPNumber = Int32.Parse(status.fiscalNumber);
            //#endif
            initRow.FiscalNumber = status.fiscalNumber;
            initRow.SmenaOpened = status.sessionIsOpened;
            initRow.SerialNumber = status.serialNumber;
            initRow.Version = status.VersionOfSWOfECR;
            initRow.CurrentSystemDateTime = DateTime.Now;
            if (pr.structResult.ByteResult == 2)
            {
                pr.showBottomString(pr.structResult.ToString());
            }
            else
            {
                pr.showBottomString("");
            }
            //if ((status.sessionIsOpened) && ())
            if ((status.sessionIsOpened) && ((bool)sStatus.ExceedingOfWorkingShiftDuration))
            {
                onlyZReport(pr);
            }
            if (status.receiptIsOpened)
            {
                pr.FPResetOrder();
            }
            if (sStatus.ByteStatus != 0)
            {
                pr.showTopString(pr.structResult.ToString());
            }
            else
            {
                pr.showTopString("");
            }
            try
            {
                string curdate = pr.fpDateTime.ToString("dd.MM.yy");
                initRow.CurrentDate = curdate;
                initRow.CurrentTime = pr.fpDateTime.ToString("HH:mm:ss");

                TimeSpan ts = pr.fpDateTime - DateTime.Now.AddSeconds(initRow.DeltaTime.GetValueOrDefault());
                //TODO подумать как если день назад, а не только вперед....
                if ((!status.sessionIsOpened) && (DateTime.Now.AddSeconds(initRow.DeltaTime.GetValueOrDefault()).ToString("dd.MM.yy") != curdate))
                {
                    pr.fpDateTime = DateTime.Parse(String.Format("{0} 23:59:59", initRow.CurrentDate));
                    Thread.Sleep(2000);
                }
                if ((!status.sessionIsOpened) && (ts.Minutes != 0))
                {
                    pr.fpDateTime = DateTime.Now.AddSeconds(initRow.DeltaTime.GetValueOrDefault());
                }
                initRow.CurrentDate = pr.fpDateTime.ToString("dd.MM.yy");
                initRow.CurrentTime = pr.fpDateTime.ToString("HH:mm:ss");
                if (papstatus != null)
                    initRow.PapStat = papstatus.ToString();
            }
            catch (Exception ex)
            {

                logger.Error(ex, "Error get date from fiscal printer");
                initRow.Error = true;
                initRow.ErrorInfo = string.Format("Error:{0};St={1};Rt={2};Rv={3}", ex.Message + " #" + "Error get date from fiscal printer", pr.structStatus.ToString(), pr.structResult.ToString(), pr.structReserv.ToString());
                initRow.CurrentSystemDateTime = DateTime.Now;
                initRow.ByteStatus = pr.ByteStatus;
                initRow.ByteStatusInfo = pr.structStatus.ToString();
                initRow.ByteReserv = pr.ByteReserv;
                initRow.ByteReservInfo = pr.structReserv.ToString();
                initRow.ByteResult = pr.ByteResult;
                initRow.ByteResultInfo = pr.structResult.ToString();

                _focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
            }

            if ((((operation.Operation != 3) || (operation.Operation != 40) || (operation.Operation != 35))) && (!status.sessionIsOpened))
            {
                pr.FPNullCheck();
            }
            initRow.CurrentSystemDateTime = DateTime.Now;
            initRow.ByteStatus = pr.ByteStatus;
            initRow.ByteStatusInfo = pr.structStatus.ToString();
            initRow.ByteReserv = pr.ByteReserv;
            initRow.ByteReservInfo = pr.structReserv.ToString();
            initRow.ByteResult = pr.ByteResult;
            initRow.ByteResultInfo = pr.structResult.ToString();
            _focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
            return !initRow.Error;
        }


        private long getintDateTime(DateTime inDateTime)
        {
            //string sinDateTime = inDateTime.ToString("yyyyMMddHHmmss");

            return inDateTime.Year * 10000000000 + inDateTime.Month * 100000000 + inDateTime.Day * 1000000 + inDateTime.Hour * 10000 + inDateTime.Minute * 100 + inDateTime.Second;
        }
    }
}
