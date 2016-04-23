using CentralLib.Protocols;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintFP.Primary
{
    public partial class Init
    {


        /// <summary>
        /// если что то не так и смену нужно закрыть то закрываем, но добавляем запись о закрытии смены!!!!!
        /// </summary>
        /// <param name="pr"></param>
        public void onlyZReport(BaseProtocol pr)
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            long ldatetime = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
            setInfo(pr, 39, ldatetime); // обновляем инфо по смене....
            var returnpr = pr.FPDayClrReport();
            using (DataClasses1DataContext focus = new DataClasses1DataContext())
            {
                tbl_Operation newZ = new tbl_Operation()
                {
                    DateTime = ldatetime,
                    Operation = 39,
                    FPNumber = FPnumber,
                    InWork = true,
                    Closed = true,
                    Error= false,
                    ByteStatus = returnpr.ByteStatus,
                    ByteResult = returnpr.ByteResult,
                    ByteReserv = returnpr.ByteReserv,
                    CurentDateTime = DateTime.Now,
                    Disable= false

                };
                focus.tbl_Operations.InsertOnSubmit(newZ);
                focus.SubmitChanges(System.Data.Linq.ConflictMode.ContinueOnConflict);
            }
        }

        public void StartJob(BaseProtocol pr)
        {
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            pr.FPDayReport();
        }

    }
}
