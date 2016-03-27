using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncOpenStore
{
    public class SyncDB
    {
        private static string fpnumber;
        private static string sqlserver;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {


            DBHelper.DBLoaderSQLtoSQL dbsync = new DBHelper.DBLoaderSQLtoSQL("sql","123");
            dbsync.StartSync();
            Console.ReadKey();
            dbsync.StopSync();

        }
    }
}
