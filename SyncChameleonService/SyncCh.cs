using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SyncHameleon;
using NDesk.Options;

namespace SyncChameleonService
{
    public partial class SyncCh : ServiceBase
    {
        private System.Diagnostics.EventLog eventLog1;
        private StartApp app;

        private string[] args;

        public SyncCh(params string[] args)
        {
            InitializeComponent();
            this.ServiceName = "SyncChameleonService";
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("SyncChameleonService"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "SyncChameleonService", "SyncChameleonServiceLog");
            }
            eventLog1.Source = "SyncChameleonService";
            eventLog1.Log = "SyncChameleonServiceLog";
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
            if (args.Length != 0)
            {
                eventLog1.WriteEntry("Next1");
                if ((app == null))
                {
                    app = new StartApp(this.args);
                }
                if (!app.Active())
                {
                    app.OnStart();
                }
            }
            else if (this.args.Length != 0)
            {
                eventLog1.WriteEntry("Next2");
                foreach(var arg in this.args)
                {
                    eventLog1.WriteEntry(arg);
                }

                if ((app==null))
                {
                    eventLog1.WriteEntry("init app");
                    app = new StartApp(this.args);
                    eventLog1.WriteEntry("inited app");
                }
                if (!app.Active())
                {
                    eventLog1.WriteEntry("start app");
                    app.OnStart();
                    eventLog1.WriteEntry("started app");
                }
            }
        }

        protected override void OnStop()
        {            
            eventLog1.WriteEntry("In OnStop");         
            if ((app != null) && (app.Active()))
            {
                app.OnStop();
            }
        }

    }

    public class StartApp
    {
        //private ProcessStartInfo info;
        //private Process process;
        private SyncHameleon.Postrgres post;
        private static string fpnumber;
        private static string sqlserver;

        public StartApp(params string[] args)
        {

            new OptionSet()
                .Add("fp=|fpnumber=", fp => fpnumber = fp)
                .Add("s=|sqlserver=", s => sqlserver = s)                
                .Parse(args);

            post = new Postrgres(sqlserver, fpnumber);
            post.startSync();
        //    info = new ProcessStartInfo(@".\SyncHameleon.exe");
        //    info.Arguments = args[0];
        //    info.UseShellExecute = false;
        //    info.RedirectStandardError = true;
        //    info.RedirectStandardInput = true;
        //    info.RedirectStandardOutput = true;
        //    info.CreateNoWindow = true;
        //    info.ErrorDialog = false;
        //    info.WindowStyle = ProcessWindowStyle.Hidden;           
        }

        public bool Active()
        {
            if (post != null)
                return post.Active;
            return false;
        }

        public void OnStart()
        {
            //process = Process.Start(info);            
            if (post != null)
                post.startSync();
        }

        public void OnStop()
        {
            if (post != null)
                post.Dispose();
            //process.Kill();
            //process.Close();
        }
    }
}
