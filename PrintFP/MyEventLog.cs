using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PrintFP
{
    public class MyEventLog
    {
        private System.Diagnostics.EventLog eventLog1;
        public bool SaveToLog { get; private set; }
        private string fpnumber;

        public MyEventLog(bool SaveToLog, string fpnumber)
        {
            if (SaveToLog)
            {
                this.SaveToLog = SaveToLog;
                this.fpnumber = fpnumber;
                eventLog1 = new System.Diagnostics.EventLog();
                if (!System.Diagnostics.EventLog.SourceExists("ServiceFP"))
                {
                    System.Diagnostics.EventLog.CreateEventSource(
                        "ServiceFP", "ServiceFPLog");
                }
                eventLog1.Source = "ServiceFP";
                eventLog1.Log = "ServiceFPLog";
            }
        }

        public void WriteEntry(string Message)
        {
            WriteEntry(Message, EventLogEntryType.Information);
        }

        public void WriteEntryError(string Message)
        {
            WriteEntry(Message, EventLogEntryType.Error);
        }

        public void WriteEntryWarning(string Message)
        {
            WriteEntry(Message, EventLogEntryType.Warning);
        }

        public void WriteEntry(string Message, EventLogEntryType entryType)
        {
            if (this.SaveToLog)
            {
                eventLog1.WriteEntry(string.Format("FP:{0}\n{1}", fpnumber, Message), entryType);
            }
        }
    }
}
