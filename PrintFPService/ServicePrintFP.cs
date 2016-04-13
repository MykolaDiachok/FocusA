using NDesk.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Management;

namespace PrintFPService
{
    public partial class ServicePrintFP : ServiceBase
    {
        private System.Diagnostics.EventLog eventLog1;
        private SmartApps apps;

        private string[] args;

        public ServicePrintFP(params string[] args)
        {
            InitializeComponent();
            this.ServiceName = "ServicePrintFP";
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("ServiceFP"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "ServiceFP", "ServiceFPLog");
            }
            eventLog1.Source = "ServiceFP";
            eventLog1.Log = "ServiceFPLog";
            this.args = args;
        }

        public void onDebug(params string[] args)
        {

            eventLog1.WriteEntry("In onDebug");
            OnStart(args);
        }

        public void onDebugStop()
        {
            OnStop();
        }



        protected override void OnStart(params string[] args)
        {
            eventLog1.WriteEntry("In OnStart");
            string compname = "";
            List<int> fpnumbers = new List<int>();
            if (args.Length != 0)
            {
                eventLog1.WriteEntry("Next1");
                var os = new OptionSet()
                        .Add("fp|fpnumber=", "set fp or ser array fp", a => fpnumbers.Add(int.Parse(a)))
                       .Add("cn|compname=", "set computer name", cn => compname = cn);
                try
                {
                    var p = os.Parse(args);
                }
                catch (Exception e)
                {
                    eventLog1.WriteEntry(e.Message, EventLogEntryType.Error);
                    throw e;
                }

                apps = new SmartApps(compname, fpnumbers);
                apps.OnStart();
                //Thread.Sleep(300);
            }
            else if (this.args.Length != 0)
            {
                eventLog1.WriteEntry("Next2");
                foreach (var arg in this.args)
                {
                    eventLog1.WriteEntry(arg);
                }

                var os = new OptionSet()
                        .Add("fp|fpnumber=", "set fp or ser array fp", a => fpnumbers.Add(int.Parse(a)))
                       .Add("cn|compname=", "set computer name", cn => compname = cn);
                try
                {
                    var p = os.Parse(this.args);
                }
                catch (Exception e)
                {
                    eventLog1.WriteEntry(e.Message, EventLogEntryType.Error);
                    throw e;
                }

                apps = new SmartApps(compname, fpnumbers);
                apps.OnStart();
                //Thread.Sleep(300);

            }
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("In OnStop");
            if (apps != null)
                apps.OnStop();
        }
    }


    public class SmartApps
    {
        private Dictionary<int, StartApp> listApp;
        private string compname;
        private List<int> fpnumbers = new List<int>();
        private System.Timers.Timer _timer;
        private System.Object lockThis = new System.Object();

        public SmartApps(string compname)
        {
            listApp = new Dictionary<int, StartApp>();
            this.compname = compname;

        }

        public SmartApps(string compname, List<int> fpnumbers)
        {
            listApp = new Dictionary<int, StartApp>();
            this.compname = compname;
            this.fpnumbers = fpnumbers;

        }

        public void OnStart()
        {
            lock (lockThis)
            {
                InitApps();
            }
                setTimer();
            
        }

        public void OnStart(string compname)
        {
            this.compname = compname;
            OnStart();
        }

        public void OnStop()
        {
            lock (lockThis)
            {
                _timer.Stop();
                _timer.Close();

                Thread.Sleep(300);
                deleteApps();
                _timer.Dispose();
            }
        }

        private void InitApps()
        {
            using (DataClassesFocusADataContext _focusA = new DataClassesFocusADataContext())
            {
                Table<tbl_ComInit> tblComInit = _focusA.GetTable<tbl_ComInit>();
                List<tbl_ComInit> comInit;
                var tfp = fpnumbers.ToArray();
                if (fpnumbers.Count > 0)
                {
                    comInit = (from table in tblComInit                                                                     
                                   where table.Init == true                                   
                                   && table.WorkOff != true
                                   && table.auto==true
                                   && table.CompName.ToLower() == compname.ToLower()
                                   select table).Where(x=>fpnumbers.Contains(x.FPNumber.GetValueOrDefault())).ToList();
                }
                else
                {
                    comInit = (from table in tblComInit                                   
                                   where table.Init == true
                                   && table.WorkOff != true
                                   && table.auto == true
                                   && table.CompName.ToLower() == compname.ToLower()
                                   select table).ToList();
                }
                foreach (var rowinit in comInit)
                {
                    
                    if (!listApp.ContainsKey(rowinit.FPNumber.GetValueOrDefault()))//(listApp[rowinit.FPNumber.GetValueOrDefault()]==null)
                    {
                        StartApp newApp = new StartApp(new Guid(), new string[] { string.Format("--fp={0}", rowinit.FPNumber) });
                        listApp.Add(rowinit.FPNumber.GetValueOrDefault(), newApp);
                        newApp.OnStart();
                        Thread.Sleep(300);
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
            using (DataClassesFocusADataContext _focusA = new DataClassesFocusADataContext())
            {
                Table<tbl_ComInit> tblComInit = _focusA.GetTable<tbl_ComInit>();
                List<int> forDelete = new List<int>();
                foreach (var app in listApp)
                {
                    tbl_ComInit comInit;
                    if (fpnumbers.Count != 0)
                    {
                        comInit = (from table in tblComInit                                   
                                   where table.Init == true
                                   && table.CompName.ToLower() == compname.ToLower()
                                   && table.FPNumber == app.Key
                                   select table).Where(x => fpnumbers.Contains(x.FPNumber.GetValueOrDefault())).FirstOrDefault();
                    }
                    else {
                        comInit = (from table in tblComInit
                                   where table.Init == true
                                   && table.CompName.ToLower() == compname.ToLower()
                                   && table.FPNumber == app.Key
                                   select table).FirstOrDefault();
                    }
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
            KillProc();
        }

        private void deleteApps()
        {
            using (DataClassesFocusADataContext _focusA = new DataClassesFocusADataContext())
            {
                Table<tbl_ComInit> tblComInit = _focusA.GetTable<tbl_ComInit>();
                foreach (var app in listApp)
                {

                    var comInit = (from table in tblComInit
                                   where  table.CompName.ToLower() == compname.ToLower()
                                   && table.FPNumber == app.Key
                                   select table).FirstOrDefault();
                    if (comInit != null)
                    {
                        comInit.Error = true;
                        comInit.ErrorInfo = "остановка сервиса";
                        _focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
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
            Process[] processlist = Process.GetProcesses().Where(x => x.ProcessName.ToLower() == "PrintFp".ToLower()).ToArray();

            foreach (Process theprocess in processlist)
            {
                var getApp = listApp
                    .Where(x => x.Value.Active() && x.Value.proccesId == theprocess.Id)
                    .Select(e => (KeyValuePair<int, StartApp>?)e)
                    .FirstOrDefault();
               if (getApp==null)
                {
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
            _timer = new System.Timers.Timer();
            _timer.Interval = (30000);
            //_timer.Interval = (100);
            _timer.Elapsed += (sender, e) => { HandleTimerElapsed(); };
            _timer.Enabled = true;
        }

        private void HandleTimerElapsed()
        {
            _timer.Stop();
            lock (lockThis)
            {
                InitApps();
                controlApps();
            }
            _timer.Start();
        }

    }

    public class StartApp
    {
        private ProcessStartInfo processInfo;
        public Guid appGuid { get; private set; }
        private Process process;
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
        public string compname { get; private set; }
        private System.Diagnostics.EventLog eventLog1;

        private void baseInit()
        {
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("ServiceFP"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "ServiceFP", "ServiceFPLog");
            }
            eventLog1.Source = "ServiceFP";
            eventLog1.Log = "ServiceFPLog";
            eventLog1.WriteEntry("On init:" + fpnumber, EventLogEntryType.Information);
        }

        public StartApp (Guid inGuid, int inFPNumber)
        {
            appGuid = inGuid;
            fpnumber = inFPNumber.ToString();
            baseInit();
            init(compname, fpnumber);
        }

        public StartApp(Guid inGuid, params string[] args)
        {


            baseInit();
            var os = new OptionSet()
                .Add("fp|fpnumber=", "set fp or ser array fp", a => fpnumber = a)
                       .Add("cn|compname=", "set computer name", cn => compname = cn);
            try
            {
                var p = os.Parse(args);
                baseInit();
            }
            catch (Exception e)
            {
                eventLog1.WriteEntry(e.Message, EventLogEntryType.Error);
            }
            
            init(compname, fpnumber);
        }

        private void init(string compname, string fpnumber)
        {
            processInfo = new ProcessStartInfo
            {
                UseShellExecute = false, // change value to false
                FileName = AppDomain.CurrentDomain.BaseDirectory + @"PrintFp.exe",
                Arguments = string.Format("-a --fp={0} --g={1}", fpnumber, appGuid),
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                ErrorDialog = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas"
            };
            process = new Process();            
            process.StartInfo = processInfo;
            process.EnableRaisingEvents = true;
            process.Exited += new EventHandler(myProcess_Exited);
        }

        public bool Active()
        {
            try
            {
                var chosen = Process.GetProcessById(process.Id);                
            }
            catch(Exception ex)
            {
                eventLog1.WriteEntry("Active:" + fpnumber+" get a no fatal error->"+ ex.Message, EventLogEntryType.Information);
                return false;
            }
                        
            return active;
        }

        public void OnStart()
        {
            eventLog1.WriteEntry("On start:"+fpnumber, EventLogEntryType.Information);
            try
            {
                active = process.Start();
            }
            catch(ObjectDisposedException ex)
            {
                init(compname, fpnumber);
                active = process.Start();
                eventLog1.WriteEntry(ex.Message, EventLogEntryType.Error);
            }

        }

        public void OnStop()
        {
            eventLog1.WriteEntry("On stop:" + fpnumber, EventLogEntryType.Information);
            if ((active) && (process != null) && (process.Id != 0))
                process.Kill();
            process.Close();
            process.Dispose();
            active = false;
        }

        private void myProcess_Exited(object sender, System.EventArgs e)
        {
            eventLog1.WriteEntry("On myProcess_Exited:" + fpnumber, EventLogEntryType.Information);
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
