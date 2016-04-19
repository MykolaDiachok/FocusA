using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbHelperSQL
{
    public class DbHelperSQLStatus
    {
        public string CompName { get; private set; }
        public int? FPNumber { get; private set; }
        public string DataServer { get; private set; }
        public string DataBaseName { get; private set; }

        public DbHelperSQLStatus(string CompName, int? FPNumber, string DataServer, string DataBaseName)
        {
            this.CompName = CompName;
            this.FPNumber = FPNumber;
            this.DataServer = DataServer;
            this.DataBaseName = DataBaseName;
        }

        public DbHelperSQLStatus(string DataServer, int? FPNumber)
        {
            this.DataServer = DataServer;
            this.FPNumber = FPNumber;
        }

        public DbHelperSQLStatus(string DataServer)
        {
            this.DataServer = DataServer;
        }

        public DbHelperSQLStatus(int FPNumber)
        {
            this.FPNumber = FPNumber;
        }

        public void setStatus(string status, DateTime dateTimeSyncDB)
        {
            DataClassesFocusADataContext focusA = new DataClassesFocusADataContext();
            Table<tbl_SyncDBStatus> tbl_syncDBStatus = focusA.GetTable<tbl_SyncDBStatus>();

            var row = (from syncDBStatus in tbl_syncDBStatus
                       where 
                       object.Equals(syncDBStatus.FPNumber, this.FPNumber)
                       && object.Equals(syncDBStatus.CompName, this.CompName)
                       && object.Equals(syncDBStatus.DataServer, this.DataServer)
                       && object.Equals(syncDBStatus.DataBaseName, this.DataBaseName)                       
                       select syncDBStatus).OrderByDescending(x => x.DateTimeSyncDB).FirstOrDefault();
            if (row == null)
            {
                tbl_SyncDBStatus syncStatus = new tbl_SyncDBStatus
                {
                    CompName = this.CompName,
                    FPNumber = this.FPNumber,
                    DataServer = this.DataServer,
                    DataBaseName = this.DataBaseName,
                    Status = status,
                    DateTimeSyncDB = dateTimeSyncDB
                };
                focusA.tbl_SyncDBStatus.InsertOnSubmit(syncStatus);
            }
            else
            {
                row.Status = status;
                row.DateTimeSyncDB = dateTimeSyncDB;
            }
            focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
        }

        public void setStatus(string status)
        {
            setStatus(status, DateTime.Now);
        }

        public void setStatusOnLine(DateTime dateTimeSyncDB)
        {
            setStatus("Online", dateTimeSyncDB);
        }

        public void setStatusOnLine()
        {
            setStatus("Online");
        }

        public void setStatusOFFLine(DateTime dateTimeSyncDB)
        {
            setStatus("OFFLine", dateTimeSyncDB);
        }

        public void setStatusOFFLine()
        {
            setStatus("OFFLine");
        }

        public void setStatusInit(DateTime dateTimeSyncDB)
        {
            setStatus("Init", dateTimeSyncDB);
        }

        public void setStatusInit()
        {
            setStatus("Init");
        }

        public void setStatusWaiting(DateTime dateTimeSyncDB)
        {
            setStatus("Waiting", dateTimeSyncDB);
        }

        public void setStatusWaiting()
        {
            setStatus("Waiting");
        }





    }
}
