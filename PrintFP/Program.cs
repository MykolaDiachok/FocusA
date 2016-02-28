using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;
using System.Threading.Tasks;
using NDesk.Options;
using NLog;
using NLog.Config;
using CentralLib;
using CentralLib.Protocols;

namespace PrintFP
{
    class Program
    {
        private static string fpnumber, server;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static DateTime startJob;

        static int Main(string[] args)
        {
            //startJob = DateTime.Now;
#if (!DEBUG)
                logger.Info("Enable NBug");
                AppDomain.CurrentDomain.UnhandledException += Handler.UnhandledException;
                TaskScheduler.UnobservedTaskException += Handler.UnobservedTaskException;
#endif
            new OptionSet()
               .Add("fp=|fpnumber=", fp => fpnumber = fp)
               .Add("s=|server=", s => server = s)
               .Add("?|h|help", h => DisplayHelp())
               .Parse(args);

            if (String.IsNullOrEmpty(fpnumber))
            {
                Console.WriteLine("Need arg -fp=\"fpnumber\"");
                return (int)infoPr.Bad;
            }
            if (String.IsNullOrEmpty(server))
            {
                server = System.Environment.MachineName;
#if (DEBUG)
                    server = "focus-A";
#endif
            }
            NLog.GlobalDiagnosticsContext.Set("FPNumber", fpnumber);
            logger.Info("Set fp number:{0}", fpnumber);
            using (DataClasses1DataContext _focusA = new DataClasses1DataContext())
            {                
                Table<tbl_ComInit> tablePayment = _focusA.GetTable<tbl_ComInit>();
                var comInit = (from list in tablePayment
                               where list.Init == true
                               && list.CompName.ToLower()==server.ToLower()
                               && list.FPNumber==int.Parse(fpnumber)
                               select list);
                foreach(var initRow in comInit)
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

#if (!DEBUG)
                            initRow.FPNumber = pr.status.fiscalNumber;
#endif
                            initRow.FiscalNumber = pr.status.fiscalNumber;
                            initRow.SmenaOpened = pr.status.sessionIsOpened;
                            initRow.SerialNumber = pr.status.serialNumber;
                            initRow.Version = pr.status.VersionOfSWOfECR;
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
                                initRow.ErrorInfo = ex.Message;                                
                            }
                            initRow.CurrentSystemDateTime = DateTime.Now;                            
                            initRow.ByteStatus = pr.ByteStatus;
                            initRow.ByteReserv = pr.ByteReserv;
                            initRow.ByteResult = pr.ByteResult;
                            _focusA.SubmitChanges();
                            //    if (initRow.Error)
                            //    {

                            //    }

                        }
                    }
                    catch(Exception ex)
                    {
                        logger.Error(ex, "Error fiscal printer");
                        initRow.Error = true;
                        initRow.ErrorInfo = "Error info:" + "Fatal crash app;"+ex.Message;
                        initRow.ErrorCode = 9999; // ошибка которая привела к большому падению
                        _focusA.SubmitChanges();
                    }

                    
                }
                Console.ReadLine();
                Console.WriteLine("Time start:{0}", startJob);
                Console.WriteLine("Time stop:{0}", DateTime.Now);
            }
            return (int)infoPr.Good;
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("Help");
            Console.WriteLine("\t-h\tShow this screen");
            Console.WriteLine("\t-fp=\"numberfp\"\tSet fiscal printer");
            Console.WriteLine("\t-s=\"server\"\tSet server for work");
        }


        enum infoPr
        {
            Good=0,
            Bad=1,
            CriticalError=2
        }
    }
}
