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

namespace PrintFP.Primary
{
    partial class Init
    {
        private Logger logger = LogManager.GetCurrentClassLogger();
        public Init(string fpnumber, string server)
        {
            //logger.Trace("Init fp:{0}; server:{1}", fpnumber, server);
            using (DataClasses1DataContext _focusA = new DataClasses1DataContext())
            {
                Table<tbl_ComInit> tablePayment = _focusA.GetTable<tbl_ComInit>();
                var comInit = (from list in tablePayment
                               where list.Init == true
                               && list.CompName.ToLower() == server.ToLower()
                               && list.FPNumber == int.Parse(fpnumber)
                               select list);
                //logger.Trace("cominit:{0}",comInit.ToString());
                foreach (var initRow in comInit)
                {
                    DateTime tBegin = DateTime.ParseExact(initRow.DateTimeBegin.ToString(), "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).AddHours(-1);
                    DateTime tEnd = DateTime.ParseExact(initRow.DateTimeStop.ToString(), "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                    DateTime worktime = DateTime.Now.AddSeconds((double)initRow.DeltaTime);
                    // logger.Trace("Fp={0};Begin:{1};End:{2}", initRow.FPNumber, tBegin, getintDateTime(worktime));

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
                    if (operation != null)
                    {

                        try
                        {
                            //using (Protocol_EP11 pr = new Protocol_EP11(initRow.Port))
                            BaseProtocol searchProtocol;
                            if ((initRow.MoxaIP.Trim().Length != 0) && ((int)initRow.MoxaPort > 0))
                            {
                                searchProtocol = (BaseProtocol)SingletonProtocol.Instance(initRow.MoxaIP, (int)initRow.MoxaPort).GetProtocols();
                            }
                            else
                                searchProtocol = (BaseProtocol)SingletonProtocol.Instance(initRow.Port).GetProtocols();

                            using (var pr = searchProtocol)
                            {
                                if (!InitialSet(_focusA, initRow, pr, operation))
                                {
                                    return;
                                }
                                if (operation.Operation == 3) // set cachier
                                {

                                    var tblCashier = getCashier(_focusA, operation);
                                    logger.Trace("set cachier:{0}", tblCashier.Name_Cashier);
                                    pr.FPRegisterCashier(0, tblCashier.Name_Cashier);
                                    tblCashier.ByteReserv = pr.ByteReserv;
                                    tblCashier.ByteResult = pr.ByteResult;
                                    tblCashier.ByteStatus = pr.ByteStatus;

                                    //tblCashier.Error = pr.er
                                }
                                else if (operation.Operation == 10) //in money
                                {
                                    var tblCashIO = getCashIO(_focusA, operation);
                                    logger.Trace("in money:{0}, fp:{1}", tblCashIO.Money, 30000);
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
                                    logger.Trace("out money. in base:{0}; in box{1}, make:{2}", tblCashIO.Money, rest, outMoney);
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
                                    foreach (var rowCheck in tableCheck)
                                    {
                                        //string forPrint = rowCheck.GoodName;                                        
                                        ReceiptInfo rowSum;
                                        //if ((listgoods.ContainsKey((ulong)rowCheck.packname))&&(listgoods[(ulong)rowCheck.packname]!= rowCheck.Price))
                                        ulong packcode = (ulong)rowCheck.packname.GetValueOrDefault();
                                        if (listgoods.ContainsKey((ulong)rowCheck.packname))
                                        {
                                            packcode = (ulong)rowCheck.packname + ulong.Parse(rowCheck.StrCode) * ((ulong)rowCheck.SORT * 1000000);                                            
                                        }
                                        else
                                        {
                                            listgoods.Add((ulong)rowCheck.packname, rowCheck.Price);                                            
                                        }

                                        Art art = new Art(int.Parse(rowCheck.StrCode), rowCheck.GoodName, packcode, (ushort)rowCheck.NalogGroup, rowCheck.FPNumber,_focusA);
                                        logger.Trace("Check #{0} row#{1} name:{2} pr:{3}", headCheck.id, rowCheck.SORT, art.ARTNAME, rowCheck.Price);
                                        rowSum = pr.FPSaleEx((ushort)rowCheck.Amount, (byte)rowCheck.Amount_Status, false, rowCheck.Price, art.NalogGroup, false, art.ARTNAME, art.PackCode);


                                        rowCheck.ByteReserv = pr.ByteReserv;
                                        rowCheck.ByteResult = pr.ByteResult;
                                        rowCheck.ByteStatus = pr.ByteStatus;
                                        rowCheck.Error = !pr.statusOperation;
                                        rowCheck.FPSum = rowSum.CostOfGoodsOrService;
                                        headCheck.FPSumm = rowSum.SumAtReceipt;
                                        if (rowCheck.RowSum != rowSum.CostOfGoodsOrService)
                                        {
                                            logger.Error("Отличается сумма по строке чека, нужно {0}, в аппарате {1}. Строка:{2} Чек:{3}", rowCheck.RowSum, rowSum.CostOfGoodsOrService, rowCheck.id, rowCheck.NumPayment);
                                            throw new ApplicationException(String.Format("Отличается суммапо строке чека, нужно {0}, в аппарате {1}. Строка:{2} Чек:{3}", rowCheck.RowSum, rowSum.CostOfGoodsOrService, rowCheck.id, rowCheck.NumPayment));
                                        }

                                    }
                                    if (headCheck.FPSumm != headCheck.CheckSum)
                                    {
                                        logger.Error("Отличается общая сумма чека, нужно {0}, в аппарате {1}. id:{2}", headCheck.CheckSum, headCheck.FPSumm, headCheck.id);
                                        throw new ApplicationException(String.Format("Отличается сумма чека, нужно {0}, в аппарате {1}. id:{2}", headCheck.CheckSum, headCheck.FPSumm, headCheck.id));
                                    }
                                    if (headCheck.Payment0 > 0)
                                    {
                                        pr.FPPayment(0, (uint)headCheck.Payment0, false, true);
                                        logger.Trace("Check #{0} Payment0:{1}", headCheck.id, (uint)headCheck.Payment0);
                                    }
                                    if (headCheck.Payment1 > 0)
                                    {
                                        pr.FPPayment(1, (uint)headCheck.Payment1, false, true);
                                        logger.Trace("Check #{0} Payment1:{1}", headCheck.id, (uint)headCheck.Payment1);
                                    }
                                    if (headCheck.Payment2 > 0)
                                    {
                                        pr.FPPayment(2, (uint)headCheck.Payment2, false, true);
                                        logger.Trace("Check #{0} Payment2:{1}", headCheck.id, (uint)headCheck.Payment2);
                                    }
                                    if (headCheck.Payment3 > 0)
                                    {
                                        pr.FPPayment(3, (uint)headCheck.Payment3, false, true);
                                        logger.Trace("Check #{0} Payment3:{1}", headCheck.id, (uint)headCheck.Payment3);
                                    }
                                    headCheck.ByteReserv = pr.ByteReserv;
                                    headCheck.ByteResult = pr.ByteResult;
                                    headCheck.ByteStatus = pr.ByteStatus;
                                    headCheck.Error = !pr.statusOperation;
                                    headCheck.CheckClose = true;
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
                                    //logger.Trace("Check payment begin #{0}", headCheck.id);
                                    Dictionary<ulong, int> listgoods = new Dictionary<ulong, int>();
                                    foreach (var rowCheck in tableCheck)
                                    {                                        

                                        ulong packcode = (ulong)rowCheck.packname.GetValueOrDefault();
                                        if (listgoods.ContainsKey((ulong)rowCheck.packname))
                                        {
                                            packcode = (ulong)rowCheck.packname + ulong.Parse(rowCheck.StrCode) * ((ulong)rowCheck.SORT * 1000000);
                                        }
                                        else
                                        {
                                            listgoods.Add((ulong)rowCheck.packname, rowCheck.Price);
                                        }

                                        Art art = new Art(int.Parse(rowCheck.StrCode), rowCheck.GoodName, packcode, (ushort)rowCheck.NalogGroup, rowCheck.FPNumber, _focusA);
                                        logger.Trace("Check payment #{0} row#{1} name:{2} pr:{3}", headCheck.id, rowCheck.SORT, art.ARTNAME, rowCheck.Price);

                                        var rowSum = pr.FPPayMoneyEx((ushort)rowCheck.Amount, (byte)rowCheck.Amount_Status, false, rowCheck.Price, art.NalogGroup, false, art.ARTNAME, art.PackCode);

                                        if (rowCheck.RowSum != rowSum.CostOfGoodsOrService)
                                        {
                                            logger.Error("Отличается сумма по строке чека, нужно {0}, в аппарате {1}. Строка:{2} Чек:{3}", rowCheck.RowSum, rowSum.CostOfGoodsOrService, rowCheck.id, rowCheck.NumPayment);
                                            throw new ApplicationException(String.Format("Отличается суммапо строке чека, нужно {0}, в аппарате {1}. Строка:{2} Чек:{3}", rowCheck.RowSum, rowSum.CostOfGoodsOrService, rowCheck.id, rowCheck.NumPayment));
                                        }
                                        rowCheck.ByteReserv = pr.ByteReserv;
                                        rowCheck.ByteResult = pr.ByteResult;
                                        rowCheck.ByteStatus = pr.ByteStatus;
                                        rowCheck.Error = !pr.statusOperation;
                                        headCheck.FPSumm = rowSum.SumAtReceipt;
                                    }
                                    if (headCheck.FPSumm != headCheck.CheckSum)
                                    {
                                        logger.Error("Отличается сумма чека, нужно {0}, в аппарате {1}. id:{2}", headCheck.CheckSum, headCheck.FPSumm, headCheck.id);
                                        throw new ApplicationException(String.Format("Отличается сумма чека, нужно {0}, в аппарате {1}. id:{2}", headCheck.CheckSum, headCheck.FPSumm, headCheck.id));
                                    }
                                    if (headCheck.Payment0 > 0)
                                    {
                                        pr.FPPayment(0, (uint)headCheck.Payment0, false, true);
                                        logger.Trace("Check payment #{0} Payment0:{1}", headCheck.id, (uint)headCheck.Payment0);
                                    }
                                    if (headCheck.Payment1 > 0)
                                    {
                                        pr.FPPayment(1, (uint)headCheck.Payment1, false, true);
                                        logger.Trace("Check payment #{0} Payment1:{1}", headCheck.id, (uint)headCheck.Payment1);
                                    }
                                    if (headCheck.Payment2 > 0)
                                    {
                                        pr.FPPayment(2, (uint)headCheck.Payment2, false, true);
                                        logger.Trace("Check payment #{0} Payment2:{1}", headCheck.id, (uint)headCheck.Payment2);
                                    }
                                    if (headCheck.Payment3 > 0)
                                    {
                                        pr.FPPayment(3, (uint)headCheck.Payment3, false, true);
                                        logger.Trace("Check payment #{0} Payment3:{1}", headCheck.id, (uint)headCheck.Payment3);
                                    }
                                    headCheck.ByteReserv = pr.ByteReserv;
                                    headCheck.ByteResult = pr.ByteResult;
                                    headCheck.ByteStatus = pr.ByteStatus;
                                    headCheck.Error = !pr.statusOperation;

                                    headCheck.CheckClose = true;

                                    //logger.Trace("Check payment close #{0}", headCheck.id);
                                }
                                else if (operation.Operation == 35) //X
                                {
                                    logger.Trace("print X");
                                    pr.FPDayReport();
                                }
                                else if (operation.Operation == 39) //Z
                                {
                                    pr.setFPCplCutter(true);
                                    UInt32 rest = pr.GetMoneyInBox();
                                    if (rest != 0)
                                    {
                                        logger.Trace("out money:{0}", rest);
                                        pr.FPCashOut(rest);
                                    }
                                    Thread.Sleep(30 * 1000);
                                    //var status = pr.get
                                    logger.Trace("print Z");
                                    pr.FPDayClrReport();
                                    pr.setFPCplCutter(false);
                                    Table<tbl_ART> tbl_ART = _focusA.GetTable<tbl_ART>();
                                    tbl_ART.DeleteAllOnSubmit(tbl_ART.AsEnumerable().Where(r => r.FPNumber == int.Parse(fpnumber)).ToList());
                                    _focusA.SubmitChanges();
                                }
                                else if (operation.Operation == 40) //periodic report
                                {
                                    logger.Trace("print periodic report");
                                }

                                setStatuses(operation, initRow, pr);
                                _focusA.SubmitChanges();
                                //    if (initRow.Error)
                                //    {

                                //    }

                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "Error fiscal printer");
                            initRow.Error = true;
                            initRow.ErrorInfo = "Error info:" + "Fatal crash app;" + ex.Message;
                            initRow.ErrorCode = 9999; // ошибка которая привела к большому падению
                            _focusA.SubmitChanges();
                        }

                    }
                    initRow.DateTimeSyncFP = DateTime.Now;
                    _focusA.SubmitChanges();
                }

            }
        }

        private tbl_CashIO getCashIO(DataClasses1DataContext _focusA, tbl_Operation tOp)
        {
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
            initRow.Error = false;
            initRow.ErrorInfo = "";
            initRow.ErrorCode = 0;
            var status = pr.status;
            pr.setFPCplCutter(false);
            //var dayReport = pr.dayReport;
            var papstatus = pr.papStat;
            initRow.PapStat = papstatus.ToString();
            if ((papstatus.ControlPaperIsAlmostEnded != null) && ((bool)papstatus.ControlPaperIsAlmostEnded))
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

                _focusA.SubmitChanges();
                pr.showTopString("Контрольная лента закончилась!");

                return false;
            }
            if ((papstatus.ReceiptPaperIsAlmostEnded != null) && ((bool)papstatus.ReceiptPaperIsAlmostEnded))
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
                _focusA.SubmitChanges();
                pr.showTopString("Чековая лента закончилась!");
                return false;
            }

            var sStatus = pr.structStatus;
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
                    pr.fpDateTime = DateTime.Parse(String.Format("{0} 23:59:59",initRow.CurrentDate));
                    Thread.Sleep(2000);
                }
                if ((!status.sessionIsOpened)&&(ts.Minutes!=0))
                {
                    pr.fpDateTime = DateTime.Now.AddSeconds(initRow.DeltaTime.GetValueOrDefault());
                }
                initRow.CurrentDate = pr.fpDateTime.ToString("dd.MM.yy");
                initRow.CurrentTime = pr.fpDateTime.ToString("HH:mm:ss");
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

                _focusA.SubmitChanges();
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
            _focusA.SubmitChanges();
            return !initRow.Error;
        }


        private long getintDateTime(DateTime inDateTime)
        {
            //string sinDateTime = inDateTime.ToString("yyyyMMddHHmmss");

            return inDateTime.Year * 10000000000 + inDateTime.Month * 100000000 + inDateTime.Day * 1000000 + inDateTime.Hour * 10000 + inDateTime.Minute * 100 + inDateTime.Second;
        }
    }
}
