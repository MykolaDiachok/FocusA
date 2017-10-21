using NDesk.Options;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncOpenStoreService
{
    public partial class ServiceSyncOpenStore : ServiceBase
    {

        private Logger logger = LogManager.GetCurrentClassLogger();
        private SmartApps apps;

        private string[] args;

        public ServiceSyncOpenStore(params string[] args)
        {
            InitializeComponent();
            this.ServiceName = "SyncOpenStoreService";

            this.args = args;
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void onDebug(params string[] args)
        {

            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            OnStart(args);
        }

        public void onDebugStop()
        {
            OnStop();
        }



        protected override void OnStart(params string[] args)
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            string compname = "";
            List<long> fpnumbers = new List<long>();
            List<string> databases = new List<string>();
            List<string> dataservers = new List<string>();

            if (args.Length != 0)
            {
                logger.Trace("Next step 1");
                var os = new OptionSet()
                        .Add("fp|fpnumber=", "set fp or ser array fp", a => fpnumbers.Add(long.Parse(a)))
                       .Add("cn|compname=", "set computer name", cn => compname = cn)
                       .Add("ds|dataserver=", "set data server name", ds => dataservers.Add(ds))
                       .Add("db|database=", "set database name", db => databases.Add(db));
                try
                {
                    var p = os.Parse(args);
                }
                catch (Exception e)
                {
                    logger.Error(e);
                    throw e;
                }

                apps = new SmartApps(compname, fpnumbers, dataservers, databases);
                apps.OnStart();
                //Thread.Sleep(300);
            }
            else if (this.args.Length != 0)
            {
                logger.Trace("Next step 2");
                foreach (var arg in this.args)
                {
                    logger.Trace(arg);
                }

                var os = new OptionSet()
                        .Add("fp|fpnumber=", "set fp or ser array fp", a => fpnumbers.Add(long.Parse(a)))
                       .Add("cn|compname=", "set computer name", cn => compname = cn)
                       .Add("ds|dataserver=", "set data server name", ds => dataservers.Add(ds))
                       .Add("db|database=", "set database name", db => databases.Add(db));
                try
                {
                    var p = os.Parse(this.args);
                }
                catch (Exception e)
                {
                    logger.Error(e);
                    throw e;
                }

                apps = new SmartApps(compname, fpnumbers, dataservers, databases);
                apps.OnStart();
                //Thread.Sleep(300);

            }
        }

        protected override void OnStop()
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            if (apps != null)
                apps.OnStop();
        }

    }





    public class SmartApps
    {
        private Dictionary<long, StartApp> listApp;
        private string compname;
        private List<long> fpnumbers = new List<long>();
        private List<string> dataservers = new List<string>();
        private List<string> databases = new List<string>();
        private System.Timers.Timer _timer;
        private System.Object lockThis = new System.Object();
        private static Logger logger = LogManager.GetCurrentClassLogger();




        public SmartApps(string compname, List<long> fpnumbers, List<string> dataservers, List<string> databases)
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            listApp = new Dictionary<long, StartApp>();
            this.compname = compname;
            this.fpnumbers = fpnumbers;
            this.dataservers = dataservers;
            this.databases = databases;
            logger.Trace("SmartApps=>{0} =>{1};{2};{3}", compname, fpnumbers.ToString(), dataservers.ToString(), databases.ToString());

        }

        public void OnStart()
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            lock (lockThis)
            {
                InitApps();
            }
            setTimer();

        }

        public void OnStart(string compname)
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            this.compname = compname;
            OnStart();
        }

        public void OnStop()
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            lock (lockThis)
            {
                _timer.Stop();
                _timer.Close();

                Thread.Sleep(1000);
                deleteApps();
                _timer.Dispose();
            }
        }

        private void InitApps()
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            using (DataClassesFocusADataContext _focusA = new DataClassesFocusADataContext())
            {
                Table<tbl_ComInit> tblComInit = _focusA.GetTable<tbl_ComInit>();
                List<tbl_ComInit> comInit;


                comInit = (from table in tblComInit
                           where table.Init == true
                           && table.WorkOff != true
                           && table.auto == true
                           && table.CompName.ToLower() == compname.ToLower()
                           && ((fpnumbers.Count > 0 && fpnumbers.Contains((long)table.FPNumber)) || fpnumbers.Count == 0)
                           && ((dataservers.Count > 0 && dataservers.Contains(table.DataServer)) || dataservers.Count == 0)
                            && ((databases.Count > 0 && databases.Contains(table.DataBaseName)) || databases.Count == 0)
                           select table).ToList();

                foreach (var rowinit in comInit)
                {

                    if (!listApp.ContainsKey(rowinit.FPNumber.GetValueOrDefault()))//(listApp[rowinit.FPNumber.GetValueOrDefault()]==null)
                    {
                        StartApp newApp = new StartApp(Guid.NewGuid(), new string[] { string.Format("--fp={0}", rowinit.FPNumber) });
                        listApp.Add(rowinit.FPNumber.GetValueOrDefault(), newApp);
                        newApp.OnStart();
                        Thread.Sleep(500);
                        //init.Init = true;
                        if (!rowinit.WorkOff.HasValue)
                        {
                            rowinit.WorkOff = false;
                            _focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
                        }
                    }
                    else
                    {
                        var workAPP = listApp[rowinit.FPNumber.GetValueOrDefault()];
                        if (!workAPP.Active())
                        {
                            listApp.Remove(rowinit.FPNumber.GetValueOrDefault());
                            //workAPP.OnStop();
                            //Thread.Sleep(300);
                            //workAPP.OnStart();
                            //Thread.Sleep(300);
                        }
                    }

                }
            }
        }

        private void controlApps()
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            using (DataClassesFocusADataContext _focusA = new DataClassesFocusADataContext())
            {
                Table<tbl_ComInit> tblComInit = _focusA.GetTable<tbl_ComInit>();
                List<long> forDelete = new List<long>();
                foreach (var app in listApp)
                {
                    tbl_ComInit comInit;

                    comInit = (from table in tblComInit
                               where table.Init == true
                               && table.CompName.ToLower() == compname.ToLower()
                               && table.FPNumber == app.Key
                               && ((fpnumbers.Count > 0 && fpnumbers.Contains((int)table.FPNumber)) || fpnumbers.Count == 0)
                               && ((dataservers.Count > 0 && dataservers.Contains(table.DataServer)) || dataservers.Count == 0)
                               && ((databases.Count > 0 && databases.Contains(table.DataBaseName)) || databases.Count == 0)
                               select table).FirstOrDefault();

                    if (comInit == null)
                    {
                        forDelete.Add(app.Key);
                        if (app.Value.Active())
                            app.Value.OnStop();
                    }
                    if (comInit.WorkOff.GetValueOrDefault())
                    {
                        forDelete.Add(app.Key);
                        if (app.Value.Active())
                            app.Value.OnStop();
                    }
                    if (!app.Value.Active())
                    {
                        forDelete.Add(app.Key);
                        app.Value.OnStop();
                    }
                }
                foreach (var del in forDelete)
                {
                    listApp.Remove(del);
                }
            }
            //KillProc();
        }

        private void deleteApps()
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            using (DataClassesFocusADataContext _focusA = new DataClassesFocusADataContext())
            {
                Table<tbl_ComInit> tblComInit = _focusA.GetTable<tbl_ComInit>();
                foreach (var app in listApp)
                {

                    var comInit = (from table in tblComInit
                                   where table.CompName.ToLower() == compname.ToLower()
                                   && table.FPNumber == app.Key
                                   select table).FirstOrDefault();
                    if (comInit != null)
                    {
                        comInit.Error = true;
                        comInit.ErrorInfo = "остановка сервиса, завершение процессов";
                        logger.Warn(comInit.ErrorInfo);
                        _focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
                    }
                    var loginfo = (from log in _focusA.GetTable<tbl_SyncDBStatus>()
                                   where log.FPNumber == app.Key
                                   select log).FirstOrDefault();
                    if (loginfo != null)
                    {
                        loginfo.DateTimeSyncDB = DateTime.Now;
                        loginfo.Status = "offline";
                    }
                    //if (app.Value.Active())
                    app.Value.OnStop();
                }
                listApp.Clear();
            }
        }

        /// <summary>
        /// Поиск и уничтожение процессов которые могли подвиснуть
        /// и привести к возможности дублжа информации, поэтому смотрим все процессы PrintFp
        /// и если в словаре нет такого процесса, то киляем
        /// TODO возможно стоит смотреть и на именования которые еть в базе!!!!
        /// </summary>
        private void KillProc()
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            Process[] processlist = Process.GetProcesses().Where(x => x.ProcessName.ToLower() == "SyncOpenStore".ToLower()).ToArray();

            foreach (Process theprocess in processlist)
            {
                var getApp = listApp
                    .Where(x => x.Value.Active() && x.Value.proccesId == theprocess.Id)
                    .Select(e => (KeyValuePair<long, StartApp>?)e)
                    .FirstOrDefault();
                if (getApp == null)
                {
                    //TODO переделать анализ строки процессов под regex, с более правильным анализом
                    if (theprocess.GetCommandLine().Contains("-a --fp="))
                    {
                        theprocess.Kill();
                        theprocess.Close();
                    }
                }
            }
        }

        private void setTimer()
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);

            _timer = new System.Timers.Timer();
            _timer.Interval = (30000);
            //_timer.Interval = (100);
            _timer.Elapsed += (sender, e) => { HandleTimerElapsed(); };
            _timer.Enabled = true;
        }

        private void HandleTimerElapsed()
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            _timer.Stop();
            lock (lockThis)
            {
                InitApps();
                controlApps();
            }
            _timer.Start();
        }

        /// <summary>
        /// Для самостоятельного отключения app по возможности
        /// </summary>
        private void setAutoAllOFF()
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            using (DataClassesFocusADataContext focusA = new DataClassesFocusADataContext())
            {
                foreach (var app in listApp)
                {
                    var rowinit = (from tinit in focusA.GetTable<tbl_ComInit>()
                                   where tinit.FPNumber == app.Key
                                   select tinit).FirstOrDefault();
                    if (rowinit != null)
                    {
                        rowinit.auto = false;
                    }
                }
                focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
            }
        }


    }

    public class StartApp
    {
        private ProcessStartInfo processInfo;
        public Guid appGuid { get; private set; }
        private Process process;

        private Logger logger = LogManager.GetCurrentClassLogger();

        public int proccesId
        {
            get
            {
                return process.Id;
            }
        }
        private bool active;
        //private SyncHameleon.Postrgres post;
        public string fpnumber { get; private set; }
        public long FPNumber { get; private set; }
        public string compname { get; private set; }
        ///private System.Diagnostics.EventLog eventLog1;

        //private void baseInit()
        //{
        //    eventLog1 = new System.Diagnostics.EventLog();
        //    if (!System.Diagnostics.EventLog.SourceExists("ServiceFP"))
        //    {
        //        System.Diagnostics.EventLog.CreateEventSource(
        //            "ServiceFP", "ServiceFPLog");
        //    }
        //    eventLog1.Source = "ServiceFP";
        //    eventLog1.Log = "ServiceFPLog";
        //    eventLog1.WriteEntry("On init:" + fpnumber, EventLogEntryType.Information);
        //}

        public StartApp(Guid inGuid, long inFPNumber)
        {
            appGuid = inGuid;
            fpnumber = inFPNumber.ToString();
            FPNumber = inFPNumber;
            NLog.GlobalDiagnosticsContext.Set("FPNumber", fpnumber);
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            // baseInit();
            init(compname, fpnumber);
        }

        public StartApp(Guid inGuid, params string[] args)
        {
            appGuid = inGuid;


            //baseInit();
            var os = new OptionSet()
                .Add("fp|fpnumber=", "set fp or ser array fp", a => fpnumber = a)
                       .Add("cn|compname=", "set computer name", cn => compname = cn);
            try
            {
                var p = os.Parse(args);
                NLog.GlobalDiagnosticsContext.Set("FPNumber", fpnumber);
                FPNumber = long.Parse(fpnumber);
                logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
                //baseInit();
            }
            catch (Exception e)
            {
                logger.Error(e);
                //eventLog1.WriteEntry(e.Message, EventLogEntryType.Error);
            }

            init(compname, fpnumber);
        }

        private void init(string compname, string fpnumber)
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            processInfo = new ProcessStartInfo
            {
                //UseShellExecute = false, // change value to false
                UseShellExecute = false,
                FileName = AppDomain.CurrentDomain.BaseDirectory + @"SyncOpenStore.exe",
                Arguments = string.Format("-a --fp={0} --g={1}", fpnumber, appGuid),
                //RedirectStandardError = true,
                //RedirectStandardInput = true,
                //RedirectStandardOutput = true,
                //CreateNoWindow = true,
                CreateNoWindow = false,
                ErrorDialog = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                //ErrorDialog = false,
                //WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas"
            };
            process = new Process();
            process.StartInfo = processInfo;
            process.EnableRaisingEvents = true;
            process.Exited += new EventHandler(myProcess_Exited);
            active = true;
        }

        public bool Active()
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            try
            {
                var chosen = Process.GetProcessById(process.Id);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                //eventLog1.WriteEntry("Active:" + fpnumber+" get a no fatal error->"+ ex.Message, EventLogEntryType.Information);
                return false;
            }

            return active;
        }

        public void OnStart()
        {
            //eventLog1.WriteEntry("On start:"+fpnumber, EventLogEntryType.Information);
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            try
            {
                active = process.Start();
                logger.Trace("set active={0}", active);
            }
            catch (ObjectDisposedException ex)
            {
                init(compname, fpnumber);
                active = process.Start();

                logger.Error(ex);
                //eventLog1.WriteEntry(ex.Message, EventLogEntryType.Error);
            }

        }

        public void OnStop()
        {
            //eventLog1.WriteEntry("On stop:" + fpnumber, EventLogEntryType.Information);
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            if ((active) && (process != null) && (process.Id != 0))
                process.Kill();
            process.Close();
            process.Dispose();
            active = false;
        }

        private void myProcess_Exited(object sender, System.EventArgs e)
        {
            //eventLog1.WriteEntry("On myProcess_Exited:" + fpnumber, EventLogEntryType.Information);
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            using (DataClassesFocusADataContext _focusA = new DataClassesFocusADataContext())
            {
                var tinit = (from rowinit in _focusA.GetTable<tbl_ComInit>()
                             where rowinit.FPNumber == FPNumber
                             select rowinit).FirstOrDefault();
                if (tinit != null)
                {
                    tinit.Error = true;
                    tinit.ErrorInfo = "Завершение процесса";
                }
                var loginfo = (from log in _focusA.GetTable<tbl_SyncDBStatus>()
                               where log.FPNumber == FPNumber
                               select log).FirstOrDefault();
                if (loginfo != null)
                {
                    loginfo.DateTimeSyncDB = DateTime.Now;
                    loginfo.Status = "kill";
                }
                _focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
            }

            logger.Error(e);
            active = false;
        }

    }


    public static class MyExtensions
    {
        public static string GetCommandLine(this Process process)
        {
            //var commandLine = new StringBuilder(process.MainModule.FileName);
            var commandLine = new StringBuilder();

            commandLine.Append(" ");
            using (var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            {
                foreach (var @object in searcher.Get())
                {
                    commandLine.Append(@object["CommandLine"]);
                    commandLine.Append(" ");
                }
            }

            return commandLine.ToString();
        }
    }


}
