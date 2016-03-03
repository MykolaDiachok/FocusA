using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using System.Data.Linq;
using CentralLib.Protocols;

namespace PrintFP.Primary
{
    partial class Init
    {
        private Logger logger = LogManager.GetCurrentClassLogger();
        public Init(string fpnumber, string server)
        {
            using (DataClasses1DataContext _focusA = new DataClasses1DataContext())
            {
                Table<tbl_ComInit> tablePayment = _focusA.GetTable<tbl_ComInit>();
                var comInit = (from list in tablePayment
                               where list.Init == true
                               && list.CompName.ToLower() == server.ToLower()
                               && list.FPNumber == int.Parse(fpnumber)
                               select list);
                foreach (var initRow in comInit)
                {
                    DateTime tBegin = DateTime.ParseExact(initRow.DateTimeBegin.ToString(), "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                    DateTime tEnd = DateTime.ParseExact(initRow.DateTimeStop.ToString(), "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                    Table<tbl_Operation> tableOperation = _focusA.GetTable<tbl_Operation>();
                    var operation = (from op in tableOperation
                                     where op.FPNumber == (int)initRow.FPNumber
                                     && !op.Closed && !(bool)op.Disable
                                     && op.DateTime >= initRow.DateTimeBegin && op.DateTime <= initRow.DateTimeStop
                                     //TODO добавить определение текущего времени и разницы
                                     select op).OrderBy(o => o.DateTime).ThenBy(o=>o.Operation).FirstOrDefault();
                    if (operation != null)
                    {

                        try
                        {
                            using (Protocols pr = new Protocols(initRow.Port))
                            {
                                InitialSet(_focusA, initRow, pr);
                                if (operation.Operation==3) // set cachier
                                {
                                    var tblCashier = getCashier(_focusA, operation);
                                    pr.FPRegisterCashier(0, tblCashier.Name_Cashier);
                                    tblCashier.ByteReserv = pr.ByteReserv;
                                    tblCashier.ByteResult = pr.ByteResult;
                                    tblCashier.ByteStatus = pr.ByteStatus;
                                    
                                    //tblCashier.Error = pr.er
                                }
                                else if (operation.Operation==10) //in money
                                {

                                }
                                else if (operation.Operation == 15) //out money
                                {

                                }
                                else if (operation.Operation == 12) //check
                                {

                                }
                                else if (operation.Operation == 5) //payment
                                {

                                }
                                else if (operation.Operation == 35) //X
                                {

                                }
                                else if (operation.Operation == 39) //Z
                                {

                                }
                                else if (operation.Operation == 40) //periodic report
                                {

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
                }

            }
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
                           where table.FPNumber==tOp.FPNumber
                           && table.DATETIME==tOp.DateTime
                           && table.id==tOp.NumSlave
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
        private void setStatuses(tbl_Operation tOp, tbl_ComInit tInit, Protocols pr)
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
            if (!pr.statusOperation)
                tOp.Closed = true;
            tOp.InWork = true;
            
        }

        /// <summary>
        /// Первичная инициализация и подготовка ФР
        /// </summary>
        /// <param name="_focusA"></param>
        /// <param name="initRow"></param>
        /// <param name="pr"></param>
        private void InitialSet(DataClasses1DataContext _focusA, tbl_ComInit initRow, Protocols pr)
        {
            initRow.Error = false;
            initRow.ErrorInfo = "";
            initRow.ErrorCode = 0;
            var status = pr.status;
            var dayReport = pr.dayReport;
            var papstatus = pr.papStat;
            var sStatus = pr.structStatus;
#if (!DEBUG)
                            initRow.FPNumber = pr.status.fiscalNumber;
#endif
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
                initRow.CurrentDate = pr.fpDateTime.ToString("dd.MM.yy");
                initRow.CurrentTime = pr.fpDateTime.ToString("HH:mm:ss");
                initRow.PapStat = papstatus.ToString();
            }
            catch (Exception ex)
            {

                logger.Error(ex, "Error get date from fiscal printer");
                initRow.Error = true;
                initRow.ErrorInfo = string.Format("Error:{0};St={1};Rt={2};Rv={3}", ex.Message, pr.structStatus.ToString(), pr.structResult.ToString(), pr.structReserv.ToString());
            }
            initRow.CurrentSystemDateTime = DateTime.Now;
            initRow.ByteStatus = pr.ByteStatus;
            initRow.ByteStatusInfo = pr.structStatus.ToString();
            initRow.ByteReserv = pr.ByteReserv;
            initRow.ByteReservInfo = pr.structReserv.ToString();
            initRow.ByteResult = pr.ByteResult;
            initRow.ByteResultInfo = pr.structResult.ToString();
            _focusA.SubmitChanges();
        }
    }
}
