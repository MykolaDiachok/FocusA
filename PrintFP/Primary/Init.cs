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
                    try
                    {
                        using (Protocols pr = new Protocols(initRow.Port))
                        {
                            initRow.Error = false;
                            initRow.ErrorInfo = "";
                            initRow.ErrorCode = 0;
                            var d = pr.dayReport;
#if (!DEBUG)
                            initRow.FPNumber = pr.status.fiscalNumber;
#endif
                            initRow.FiscalNumber = pr.status.fiscalNumber;
                            initRow.SmenaOpened = pr.status.sessionIsOpened;
                            initRow.SerialNumber = pr.status.serialNumber;
                            initRow.Version = pr.status.VersionOfSWOfECR;
                            initRow.CurrentSystemDateTime = DateTime.Now;
                            if (pr.structResult.ByteResult == 2)
                            {
                                pr.showBottomString(pr.structResult.ToString());
                            }
                            else
                            {
                                pr.showBottomString("");
                            }
                            if ((pr.status.sessionIsOpened) && ((bool)pr.structStatus.ExceedingOfWorkingShiftDuration))
                            {
                                onlyZReport(pr);
                            }
                            if (pr.structStatus.ByteStatus!=0)
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
                                initRow.PapStat = pr.papStat.ToString();
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
}
