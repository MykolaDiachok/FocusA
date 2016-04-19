using NLog;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintFP
{
    public static class UpdateStatusFP
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void setStatusFP(int FPnumber, string infoStatus)
        {
            using (DataClasses1DataContext focusA = new DataClasses1DataContext())
            {
                var st = (from tblSyncFP in focusA.GetTable<tbl_SyncFP>()
                          where tblSyncFP.FPNumber == FPnumber
                          select tblSyncFP).FirstOrDefault();
                if (st == null)
                {
                    tbl_SyncFP newSyncFP = new tbl_SyncFP()
                    {
                        FPNumber = FPnumber,
                        DateTimeSync = DateTime.Now,
                        Status = infoStatus
                    };
                    focusA.tbl_SyncFPs.InsertOnSubmit(newSyncFP);
                }
                else
                {
                    st.DateTimeSync = DateTime.Now;
                    st.Status = infoStatus;
                }
                NLog.GlobalDiagnosticsContext.Set("FPNumber", FPnumber);
                logger.Trace(infoStatus);
                focusA.SubmitChanges(ConflictMode.ContinueOnConflict);
            }
        }
    }
}
