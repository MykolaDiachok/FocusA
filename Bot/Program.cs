using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SKYPE4COMLib;
using System.Threading;
using NLog;
using NDesk.Options;
using System.IO;

namespace Bot
{
    class Program
    {
        private static Skype skype = new Skype();
        private static ManualResetEvent shutdownEvent;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static int TimerIntervalSec = 60;

        private static void ReadDataFromConsole(object state)
        {
            logger.Trace("Bot." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            Console.WriteLine("Enter \"x\" to exit.");

            while (Console.ReadKey().KeyChar != 'x')
            {
                Console.Out.WriteLine("");
                Console.Out.WriteLine("Enter again!");
            }

            shutdownEvent.Set();
        }

        private static void OurAttachmentStatus(TAttachmentStatus status)
        {
            if (status == TAttachmentStatus.apiAttachSuccess)
            {
                logger.Trace("Attached to skype");
                ManualReset(); // Если подключились то включаем рассылку

            }
        }
        private static Chat errorsChat;
        static void Main(string[] args)
        {



            Thread status = new Thread(ReadDataFromConsole);
            status.Start();

            try
            {
                if (!skype.Client.IsRunning)
                    skype.Client.Start(true, true);
                skype.MessageStatus += OnMessageReceived;
                ((_ISkypeEvents_Event)skype).AttachmentStatus += OurAttachmentStatus;
                skype.Attach(7, false);
                //Thread.Sleep(5000);

                Console.WriteLine(skype.ApiWrapperVersion);
                //Group mygroup = skype.CreateGroup("MyGroup");



                //foreach (Group group in skype.Groups)
                //{
                //    if (group.DisplayName.ToLower()== "Errors".ToLower())
                //    {

                //        //group.Share("Hello");
                //        Chat mychat = skype.CreateChatMultiple(group.OnlineUsers);
                //        mychat.Bookmark();
                //        mychat.Description = "Bot Errors";
                //        mychat.Topic = "Info about focus errors";                        
                //        mychat.SendMessage("Hello!!!");
                //        foreach (User user in group.OnlineUsers)
                //        {
                //            Console.WriteLine($"online - {user.DisplayName} {user.FullName}");
                //        }
                //        foreach (User user in group.Users)
                //        {
                //            Console.WriteLine($"all - {user.DisplayName} {user.FullName}");
                //        }

                //    }
                //}

                foreach (Chat tChat in skype.BookmarkedChats)
                {
                    if (tChat.Description == "Bot Errors")
                    {
                        errorsChat = tChat;
                        Console.WriteLine($"Chat found={tChat.Description} {tChat.FriendlyName} {tChat.Name} ");
                        foreach (User user in tChat.ActiveMembers)
                        {
                            Console.WriteLine($"ActiveMembers - {user.DisplayName} {user.FullName}");
                        }
                        foreach (User user in tChat.Members)
                        {
                            Console.WriteLine($"Members - {user.DisplayName} {user.FullName}");
                        }
                        break;
                    }
                }

                Console.WriteLine("skype attached");
            }
            catch (Exception ex)
            {
                Console.WriteLine("top lvl exception : " + ex.ToString());
            }


        }

        private static void ManualReset()
        {
            logger.Trace("Bot." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            //int TimerIntervalSec = 60;
            TimeSpan delay = new TimeSpan(0, 0, TimerIntervalSec);
            shutdownEvent = new ManualResetEvent(false);
            while (shutdownEvent.WaitOne(delay, true) == false)
            {
                try
                {
                    Do();
                }
                catch (NullReferenceException ex)
                {
                    logger.Error(ex);
                    throw ex;
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    Thread.Sleep(TimerIntervalSec * 1000);
                }
            }
        }

        private static string DisplayHelp(OptionSet p)
        {
            using (StringWriter nt = new StringWriter())
            {
                nt.WriteLine("Help for focus bot");
                nt.WriteLine("Options:");
                p.WriteOptionDescriptions(nt);
                nt.Flush();
                return nt.ToString();
            }
        }

        private static void Do()
        {
            if (errorsChat != null)
            {
                DriveInfo drive = new DriveInfo("C");
                double percentFree = 100 * (double)drive.TotalFreeSpace / drive.TotalSize;
                if (percentFree < 10)
                {
                    errorsChat.SendMessage($"Внимание! C:\\ свободно {percentFree}%");
                }

                List<string> fpnumbers = new List<string>();
                using (DataClassesFocusADataContext focusA = new DataClassesFocusADataContext())
                {
                    var initerror = (from tblinit in focusA.GetTable<tbl_ComInit>()
                                     where tblinit.Error
                                     select tblinit);
                    foreach (var rowinit in initerror)
                    {
                        if (!fpnumbers.Contains(rowinit.FPNumber.GetValueOrDefault().ToString()))
                        {
                            fpnumbers.Add(rowinit.FPNumber.GetValueOrDefault().ToString());
                        }
                    }

                    var fpsdb = (from fp in focusA.GetTable<tbl_SyncFP>()
                                 join init in focusA.GetTable<tbl_ComInit>()
                                 on fp.FPNumber equals init.FPNumber
                                 where init.Init && init.auto.GetValueOrDefault()
                                 select fp);
                    foreach (var fpdb in fpsdb)
                    {
                        TimeSpan razn = DateTime.Now - fpdb.DateTimeSync;
                        if (razn.TotalMinutes > 30)
                        {
                            if (!fpnumbers.Contains(fpdb.FPNumber.ToString()))
                            {
                                fpnumbers.Add(fpdb.FPNumber.ToString());
                            }
                        }
                    }

                    var dbsync = (from fp in focusA.GetTable<tbl_SyncDB>()
                                 join init in focusA.GetTable<tbl_ComInit>()
                                 on fp.FPNumber equals init.FPNumber
                                 where init.Init && init.auto.GetValueOrDefault()
                                 group fp by new {fp.FPNumber} into grouping
                                 select new { FPNumber=grouping.Key.FPNumber, DateTimeSyncDB =grouping.Max(x=>x.DateTimeSyncDB)});
                    foreach (var fpdb in dbsync)
                    {
                        TimeSpan razn = DateTime.Now - fpdb.DateTimeSyncDB;
                        if (razn.TotalMinutes > 30)
                        {
                            if (!fpnumbers.Contains(fpdb.FPNumber.ToString()))
                            {
                                fpnumbers.Add(fpdb.FPNumber.ToString());
                            }
                        }
                    }

                    var rowslog = (from logs in focusA.GetTable<tbl_Log>()
                                   where logs.SkypeInfo != true
                                   && logs.Timestamp > DateTime.Now.AddSeconds(-TimerIntervalSec * 10)
                                   && logs.Loglevel.ToLower() != "Trace".ToLower()
                                   orderby logs.Timestamp descending
                                   group logs by new { logs.FPNumber, logs.Timestamp } into g
                                   select new { FPNumber = g.Key.FPNumber, Timestamp = g.Key.Timestamp });
                    foreach (var logrow in rowslog)
                    {
                        var infoErrors = (from logs in focusA.GetTable<tbl_Log>()
                                          where logs.FPNumber == logrow.FPNumber
                                          && logs.Timestamp == logrow.Timestamp
                                          orderby logs.Timestamp
                                          select logs);
                        string errorsstring = $"fpnumber={logrow.FPNumber} datetime operation={logrow.Timestamp}:\n";
                        bool firstinfo = false;
                        foreach (var infoError in infoErrors)
                        {
                            if (!firstinfo)
                            {
                                errorsstring += $"\t-{infoError.Message} {infoError.Timestamp}";
                                firstinfo = true;
                            }
                            infoError.SkypeInfo = true;
                        }
                        errorsChat.SendMessage(errorsstring);
                    }
                    focusA.SubmitChanges(System.Data.Linq.ConflictMode.ContinueOnConflict);
                }
            }
        }


        private static void OnMessageReceived(ChatMessage pMessage, TChatMessageStatus status)
        {
            if (status == TChatMessageStatus.cmsReceived)
            {
                Console.WriteLine(pMessage.Body);
                string[] args = pMessage.Body.Split(' ');

                bool showHelp = false, statusFP = false;
                string fpnumber = ""; bool? automatic = null;

                Func<string> ShowList = () =>
                {
                    using (DataClassesFocusADataContext focus = new DataClassesFocusADataContext())
                    {
                        var listInit = (from init in focus.GetTable<tbl_ComInit>()
                                        select init);
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("{0,8} {1,7} {2,7} {3,7}\n", "FPNumber", "Init", "auto", "WorkOff");
                        foreach (var rowinit in listInit)
                        {
                            if (rowinit.Error)
                                sb.AppendFormat("{0}", rowinit.Error ? "(anger) " : " ");
                            sb.AppendFormat("{0,8} {1,7} {2,7} {3,7}", rowinit.FPNumber, rowinit.Init ? ":)" : ":(", (bool)rowinit.auto ? ":)" : ":(", (bool)!rowinit.WorkOff ? ":)" : ":(");

                            sb.Append("\n");
                        }
                        return sb.ToString();
                    }
                };

                Func<string> ShowListErrors = () =>
                {
                    using (DataClassesFocusADataContext focus = new DataClassesFocusADataContext())
                    {
                        var listInit = (from init in focus.GetTable<tbl_ComInit>()
                                        where init.Error
                                        select init);
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("{0,8} error code & error info\n", "FPNumber");
                        foreach (var rowinit in listInit)
                        {

                            sb.AppendFormat("{0}", rowinit.Error ? "(anger) " : " ");
                            sb.AppendFormat("{0,8} {1}={2}"
                                , rowinit.FPNumber
                                , rowinit.ErrorCode
                                , rowinit.ErrorInfo);

                            sb.Append("\n");
                        }
                        return sb.ToString();
                    }
                };

                Func<string> ShowStackOperations = () =>
                {
                    using (DataClassesFocusADataContext focus = new DataClassesFocusADataContext())
                    {
                        var listOp = (from op in focus.GetTable<tbl_Operation>()
                                      join init in focus.GetTable<tbl_ComInit>()
                                      on op.FPNumber equals init.FPNumber
                                      where op.Error == null
                                      && op.DateTime > Int64.Parse(DateTime.Now.ToString("yyyyMMdd") + "000000")
                                      orderby op.DateTime
                                      select new { op.DateTime, op.FPNumber, op.Operation, init.DeltaTime });
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("{0,14} {1,8}, {2}\n", "Date time", "FPNumber", "operation");
                        int index = 0;
                        foreach (var rowOp in listOp)
                        {
                            index++;
                            DateTime dt = rowOp.DateTime.getDate();
                            TimeSpan ts = DateTime.Now.AddSeconds((long)rowOp.DeltaTime) - dt;
                            string smile = "";
                            if (ts.TotalSeconds < 360)
                                smile = ":) ";
                            else
                                smile = ":( ";
                            sb.AppendFormat("{0} {1} {2} {3} #{4}"
                                , index
                                , smile
                                , dt.ToShortTimeString()
                                , rowOp.FPNumber
                                , rowOp.Operation);

                            sb.Append("\n");
                        }
                        return sb.ToString();
                    }
                };

                Func<string> ShowPaperInfo = () =>
                {
                    using (DataClassesFocusADataContext focus = new DataClassesFocusADataContext())
                    {
                        var listInit = (from init in focus.GetTable<tbl_ComInit>()
                                        where init.PapStat.Length > 0
                                        select init);
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("{0} {1} {2,8}, {3}\n", "#", " ", "FPNumber", "paper info");
                        int index = 0;
                        foreach (var rowinit in listInit)
                        {
                            index++;
                            string smile = "";
                            if (rowinit.ErrorInfo.Contains(rowinit.PapStat))
                                smile = ":( ";
                            else
                                smile = ":) ";
                            sb.AppendFormat("{0} {1} {2} #{3}"
                                , index
                                , smile
                                , rowinit.FPNumber
                                , rowinit.PapStat);

                            sb.Append("\n");
                        }
                        return sb.ToString();
                    }
                };

                var os = new OptionSet()
                    .Add("listall", "Show all fpnumbers", list => pMessage.Chat.SendMessage(ShowList()))
                    .Add("listerrors", "Show all fpnumbers with errors", list => pMessage.Chat.SendMessage(ShowListErrors()))
                    .Add("listpapers", "Show all fpnumbers with paper info", list => pMessage.Chat.SendMessage(ShowPaperInfo()))
                    .Add("liststack", "Show all stack operations for all fpnumbers", list => pMessage.Chat.SendMessage(ShowStackOperations()))
                   .Add("fp|fpnumber=", "Set fpnumber", fp => fpnumber = fp)
                   .Add("ss|showstatus", "Show status fpnumber", sst => statusFP = sst != null)
                   .Add("ae", "enable auto", a => automatic = a != null)
                   .Add("ad", "disable auto", a => automatic = !(a != null))
                   .Add("?|h|help", "show help", h => showHelp = h != null);
                try
                {
                    var p = os.Parse(args);
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                    pMessage.Chat.SendMessage(DisplayHelp(os));
                }
                if (showHelp)
                    pMessage.Chat.SendMessage(DisplayHelp(os));
                if (fpnumber.Trim().Length == 0)
                {
                    pMessage.Chat.SendMessage(DisplayHelp(os));
                    return;
                }
                Func<long, bool, bool> setAuto = (fp, value) =>
                 {
                     using (DataClassesFocusADataContext focusa = new DataClassesFocusADataContext())
                     {
                         var init = (from tblinit in focusa.GetTable<tbl_ComInit>()
                                     where tblinit.FPNumber == fp
                                     select tblinit).FirstOrDefault();
                         if (init != null)
                         {
                             init.auto = value;
                             focusa.SubmitChanges(System.Data.Linq.ConflictMode.ContinueOnConflict);
                             return true;
                         }
                     }
                     return false;
                 };
                if (automatic == true)
                    pMessage.Chat.SendMessage($"Set enable auto on fp={fpnumber}, status={setAuto(long.Parse(fpnumber), automatic.GetValueOrDefault())}");
                if (automatic == false)
                    pMessage.Chat.SendMessage($"Set disable auto on fp={fpnumber}, status={setAuto(long.Parse(fpnumber), automatic.GetValueOrDefault())}");

                Func<int, string> ShowStatus = (fp) =>
                {

                    return "";
                };


            }
        }
    }

    public static class MyExtensions
    {
        public static DateTime getDate(this long inLong)
        {
            string dt = inLong.ToString();
            return new DateTime(int.Parse(dt.Substring(0, 4))
                , int.Parse(dt.Substring(4, 2))
                , int.Parse(dt.Substring(6, 2))
                , int.Parse(dt.Substring(8, 2))
                , int.Parse(dt.Substring(10, 2))
                , int.Parse(dt.Substring(12, 2))
                );
        }

        private static long getLong(this DateTime inDateTime)
        {
            //string sinDateTime = inDateTime.ToString("yyyyMMddHHmmss");

            return inDateTime.Year * 10000000000 + inDateTime.Month * 100000000 + inDateTime.Day * 1000000 + inDateTime.Hour * 10000 + inDateTime.Minute * 100 + inDateTime.Second;
        }
    }
}
