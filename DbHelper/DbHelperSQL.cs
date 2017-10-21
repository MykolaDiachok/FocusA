using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbHelperSQL
{
    /// <summary>
    /// если изменяем таблицу то запись должна отобразиться в скуле
    /// </summary>
    public class DbHelperSQL
    {
        public string CompName { get; private set; }
        public long FPNumber { get; private set; }
        public string DataServer { get; private set; }
        public string DataBaseName { get; private set; }
        public int port { get; private set; }
        public string MoxaIP { get; private set; }
        public int MoxaPort { get; private set; }

        public  DbHelperSQL(string CompName, long FPNumber, string DataServer, string DataBaseName, int port, string MoxaIP, int MoxaPort)
        {
            this.CompName = CompName;
            this.FPNumber = FPNumber;
            this.DataServer = DataServer;
            this.DataBaseName = DataBaseName;
            this.port = port;
            this.MoxaIP = MoxaIP;
            this.MoxaPort = MoxaPort;
        }

        public void ChangeTable(string TableName, DateTime DateTimeSyncDB)
        {
            DataClassesFocusADataContext focusA = new DataClassesFocusADataContext();
            Table<tbl_SyncDB> tbl_SyncDB = focusA.GetTable<tbl_SyncDB>();
            var row = (from syncDB in tbl_SyncDB
                       where syncDB.FPNumber == this.FPNumber
                       && syncDB.TableName == TableName
                       && syncDB.CompName ==this.CompName
                       && syncDB.DataServer == this.DataServer
                       && syncDB.DataBaseName == this.DataBaseName
                       
                       select syncDB).OrderByDescending(x=>x.DateTimeSyncDB).FirstOrDefault();

            if (row==null)
            {
                tbl_SyncDB sync = new tbl_SyncDB
                {
                    CompName = this.CompName,
                    FPNumber = this.FPNumber,
                    DataServer = this.DataServer,
                    DataBaseName = this.DataBaseName,
                    Port = this.port,
                    MoxaIP = this.MoxaIP,
                    MoxaPort = this.MoxaPort,
                    TableName = TableName,
                    DateTimeSyncDB = DateTimeSyncDB,
                };
                focusA.tbl_SyncDBs.InsertOnSubmit(sync);
            }
            else
            {
                row.DateTimeSyncDB = DateTimeSyncDB;
            }
            focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
        }

        public void ChangeTable(string TableName)
        {
            ChangeTable(TableName, DateTime.Now);
        }

        public void Change_tbl_ART(DateTime DateTimeSyncDB)
        {
            ChangeTable("tbl_ART", DateTimeSyncDB);
        }

        public void Change_tbl_ART()
        {
            ChangeTable("tbl_ART");
        }

        public void Change_tbl_Cashiers(DateTime DateTimeSyncDB)
        {
            ChangeTable("tbl_Cashiers", DateTimeSyncDB);
        }

        public void Change_tbl_Cashiers()
        {
            ChangeTable("tbl_Cashiers");
        }

        public void Change_tbl_CashIn(DateTime DateTimeSyncDB)
        {
            ChangeTable("tbl_CashIn", DateTimeSyncDB);
        }

        public void Change_tbl_CashIn()
        {
            ChangeTable("tbl_CashIn");
        }

        public void Change_tbl_CashIO(DateTime DateTimeSyncDB)
        {
            ChangeTable("tbl_CashIO", DateTimeSyncDB);
        }

        public void Change_tbl_CashIO()
        {
            ChangeTable("tbl_CashIO");
        }

        public void Change_tbl_ComInit(DateTime DateTimeSyncDB)
        {
            ChangeTable("tbl_ComInit", DateTimeSyncDB);
        }

        public void Change_tbl_ComInit()
        {
            ChangeTable("tbl_ComInit");
        }

        public void Change_tbl_Info(DateTime DateTimeSyncDB)
        {
            ChangeTable("tbl_Info", DateTimeSyncDB);
        }

        public void Change_tbl_Info()
        {
            ChangeTable("tbl_Info");
        }

        public void Change_tbl_Operations(DateTime DateTimeSyncDB)
        {
            ChangeTable("tbl_Operations", DateTimeSyncDB);
        }

        public void Change_tbl_Operations()
        {
            ChangeTable("tbl_Operations");
        }

        public void Change_tbl_Payment(DateTime DateTimeSyncDB)
        {
            ChangeTable("tbl_Payment", DateTimeSyncDB);
        }

        public void Change_tbl_Payment()
        {
            ChangeTable("tbl_Payment");
        }

        public void Change_tbl_SALES(DateTime DateTimeSyncDB)
        {
            ChangeTable("tbl_SALES", DateTimeSyncDB);
        }

        public void Change_tbl_SALES()
        {
            ChangeTable("tbl_SALES");
        }

        public void Change_tbl_Tax(DateTime DateTimeSyncDB)
        {
            ChangeTable("tbl_Tax", DateTimeSyncDB);
        }

        public void Change_tbl_Tax()
        {
            ChangeTable("tbl_Tax");
        }

        public void Change_tbl_SyncDB(DateTime DateTimeSyncDB)
        {
            ChangeTable("tbl_SyncDB", DateTimeSyncDB);
        }

        public void Change_tbl_SyncDB()
        {
            ChangeTable("tbl_SyncDB");
        }

    }
}
