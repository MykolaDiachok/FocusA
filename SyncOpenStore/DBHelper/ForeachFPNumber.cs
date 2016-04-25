using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncOpenStore.DBHelper
{
    /// <summary>
    /// Класс для обхода таблицы и выборки всех аппаратов
    /// </summary>
    public class ForeachFPNumber
    {
        public string inFpNumber { get; private set; }
        public int iFPNumber { get; private set; }

        public ForeachFPNumber(string inFpNumber)
        {
            this.inFpNumber = inFpNumber;
            this.iFPNumber = int.Parse(inFpNumber);
        }

        public void MakeForeach()
        {
            using (DataClassesFocusADataContext focusA = new DataClassesFocusADataContext())            
            {

                var tbl_ComInit = focusA.GetTable<tbl_ComInit>();
                var init =
                    (from cominit in tbl_ComInit
                    where (cominit.Init == true // для синхронизации обязательно должно быть инициализирован                
                   && cominit.FPNumber == iFPNumber)
                    select cominit).FirstOrDefault();

                if (init!=null)
                {
                    //TODO TRY CATCH
                    DBLoaderSQLtoSQL syncdb = new DBLoaderSQLtoSQL(init.FPNumber.ToString(), init.RealNumber, (Int64)init.DateTimeBegin, (Int64)init.DateTimeStop);
                    
                    syncdb.SyncData();                    
                }
            }
        }
    }
}
