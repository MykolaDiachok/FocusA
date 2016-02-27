using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.SqlClient;
using System.Data.Linq.Mapping;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Npgsql;
using System.Text.RegularExpressions;
using System.Collections;

namespace SyncHameleon
{
    class Postrgres
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static System.Timers.Timer _timer;
        private static string _SQLServer, _FPNumber;



        /// <summary>
        /// Процедура запускает таймер для синхронизации данных
        /// </summary>
        /// <param name="sqlserver">Строка ссылка на sqlсервер, если есть</param>
        /// <param name="fpnumber">Если указан то № ФР</param>
        public static void startSync(string sqlserver, string fpnumber)
        {
            _SQLServer = sqlserver;
            _FPNumber = fpnumber;
            _timer = new System.Timers.Timer();
            _timer.Interval = (Properties.Settings.Default.TimerIntervalSec * 1000);
            _timer.Elapsed += (sender, e) => { HandleTimerElapsed(); };
            _timer.Enabled = true;
            Console.ReadLine();
            _timer.Dispose();
        }

        private static void HandleTimerElapsed()
        {
            SelectLogOperation();
        }


        /// <summary>
        /// Функция получение рабочих аппаратов для синхронизации
        /// </summary>
        /// <param name="_focusA">Ссылка на базу</param>
        /// <returns></returns>
        public static List<tbl_ComInit> connectToFocusA(DataClassesFocusADataContext _focusA)
        {

            //DataClassesFocusADataContext _focusA = new DataClassesFocusADataContext();
            Table<tbl_ComInit> tbl_ComInit = _focusA.GetTable<tbl_ComInit>();
            IQueryable<tbl_ComInit> initQuery =
                from cominit in tbl_ComInit
                where (cominit.Init == true // для синхронизации обязательно должно быть инициализирован
                && (!String.IsNullOrEmpty(_SQLServer) ? cominit.DataServer.ToLower() == _SQLServer.ToLower() : true)
               && (!String.IsNullOrEmpty(_FPNumber) ? cominit.RealNumber == _FPNumber : true)
               && (String.IsNullOrEmpty(_SQLServer) && String.IsNullOrEmpty(_FPNumber) ? false : true)

               )
                select cominit;

            return initQuery.ToList<tbl_ComInit>();
        }

        /// <summary>
        /// Процедура анализа лога и обновление базы
        /// </summary>
        private static void SelectLogOperation()
        {
            _timer.Stop();
            StopwatchHelper.Start("Begin select");

            using (DataClassesFocusADataContext _focusA = new DataClassesFocusADataContext())
            {
                //#if DEBUG
                //                _focusA.Log = Console.Out;
                //#endif
                List<tbl_ComInit> tbl_ComInit = connectToFocusA(_focusA);
                foreach (tbl_ComInit initRow in tbl_ComInit)
                {
                    DateTime tBegin = DateTime.ParseExact(initRow.DateTimeBegin.ToString(), "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                    DateTime tEnd = DateTime.ParseExact(initRow.DateTimeStop.ToString(), "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                    var connection = Properties.Settings.Default.Npgsql;//System.Configuration.ConfigurationManager.ConnectionStrings["Test"].ConnectionString;
                    using (var conn = new NpgsqlConnection(connection))
                    {
                        logger.Trace("NpgsqlConnection:{0}", connection);
                        conn.Open();
                        //getCashier(conn);

                        using (var cmd = new NpgsqlCommand())
                        {
                            cmd.Connection = conn;
                            cmd.CommandText = getQueryLog(initRow.RealNumber, tBegin, tEnd);
                            logger.Trace("Select from base:{0}", cmd.CommandText);
                            StopwatchHelper.Start("ExecuteReader");
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    long _dt = getintDateTime((DateTime)reader["time_sales"]);
                                    int _fp = (int)initRow.FPNumber;

                                    switch ((int)reader["id_action"])
                                    {
                                        case (int)LogOperations.Launch:
                                            {
                                                //string nameCashier = getCashier(conn, (int)reader["id_action"]);
                                                InsertCashier(_focusA, reader["name_employee"].ToString(), _dt, _fp);
                                            }
                                            logger.Trace("Operation Launch");
                                            break;
                                        case (int)LogOperations.InCash:
                                            //TODO не всегда кассиры инициализуются, берем пользователя из текущего значения, и уменьшаем время записи на 1 секунду
                                            {
                                                long _dtCashiers = getintDateTime(((DateTime)reader["time_sales"]).AddSeconds(-1));
                                                //string nameCashier = getCashier(conn, (int)reader["id_action"]);
                                                InsertCashier(_focusA, reader["name_employee"].ToString(), _dtCashiers, _fp);
                                            }
                                            insertCashIO(_focusA, reader["attrs"], _dt, _fp, false, 10); //внесение 10
                                            logger.Trace("End operation InCash");
                                            break;
                                        case (int)LogOperations.Check:
                                            logger.Trace("Operation Check");
                                            break;
                                        case (int)LogOperations.OutCash:
                                            insertCashIO(_focusA, reader["attrs"], _dt, _fp, true, 15); // вынос 15
                                            logger.Trace("Operation OutCash");
                                            break;
                                        case (int)LogOperations.Xreport: // Х отчет = 35
                                            {
                                                var trow = _focusA.GetTable<tbl_Operation>().FirstOrDefault(i => i.FPNumber == _fp && i.DateTime == _dt && i.Operation == 35);
                                                if (trow != null) break;

                                                tbl_Operation op = new tbl_Operation
                                                {
                                                    DateTime = _dt,
                                                    FPNumber = _fp,
                                                    Operation = 35,
                                                    InWork = false,
                                                    Closed = false,
                                                    Disable = false,
                                                    CurentDateTime = DateTime.Now
                                                };
                                                _focusA.tbl_Operations.InsertOnSubmit(op);
                                                _focusA.SubmitChanges();
                                            }
                                            logger.Trace("Operation Xreport");
                                            break;
                                        case (int)LogOperations.Zreport: // Z отчет = 39
                                            {
                                                var trow = _focusA.GetTable<tbl_Operation>().FirstOrDefault(i => i.FPNumber == _fp && i.DateTime == _dt && i.Operation == 39);
                                                if (trow != null) break;

                                                tbl_Operation op = new tbl_Operation
                                                {
                                                    DateTime = _dt,
                                                    FPNumber = _fp,
                                                    Operation = 39,
                                                    InWork = false,
                                                    Closed = false,
                                                    Disable = false,
                                                    CurentDateTime = DateTime.Now
                                                };
                                                _focusA.tbl_Operations.InsertOnSubmit(op);
                                                _focusA.SubmitChanges();
                                            }
                                            logger.Trace("Operation Zreport");
                                            break;
                                        case (int)LogOperations.SetCashier:
                                            {
                                                //string nameCashier = getCashier(conn,(int)reader["id_action"]);
                                                InsertCashier(_focusA, reader["name_employee"].ToString(), _dt, _fp);
                                            }
                                            logger.Trace("Operation SetCashier");
                                            break;
                                    }
                                    logger.Trace("Time:{0}\tOp:{1}", reader["time_sales"], reader["id_employee"]);
                                    //Console.WriteLine(reader.GetString(0));
                                }
                            }
                            StopwatchHelper.Stop("ExecuteReader");
                        }
                    }
                }
            }
            StopwatchHelper.Stop("Begin select");
            _timer.Start();

        }

        /// <summary>
        /// insert записи регистрации кассира в базу
        /// </summary>
        /// <param name="_focusA">Linq база</param>
        /// <param name="inName_Cashier">Имя кассира 15 символов</param>
        /// <param name="_dt">дата и время в int<</param>
        /// <param name="_fp">фискальный регистратор</param>
        private static void InsertCashier(DataClassesFocusADataContext _focusA, string inName_Cashier, long _dt, int _fp)
        {
            var trow = _focusA.GetTable<tbl_Cashier>().FirstOrDefault(i => i.FPNumber == _fp && i.DATETIME == _dt && i.Operation == 3);
            if (trow != null)
            {
                logger.Trace("Operation SetCashier in base");
                return;
            }
            tbl_Cashier cashier = new tbl_Cashier
            {
                DATETIME=_dt,
                FPNumber=_fp,
                Num_Cashier = 0,
                Name_Cashier= inName_Cashier.TrimStart().Substring(0,15).Trim(), //TODO тут должно быть имя
                Pass_Cashier = 0,
                TakeProgName = false,
                Operation=3
            };
            _focusA.tbl_Cashiers.InsertOnSubmit(cashier);
            _focusA.SubmitChanges();

            tbl_Operation op = new tbl_Operation
            {
                NumSlave = cashier.id,
                DateTime = _dt,
                FPNumber = _fp,
                Operation = 3,
                InWork = false,
                Closed = false,
                Disable = false,
                CurentDateTime = DateTime.Now
            };
            _focusA.tbl_Operations.InsertOnSubmit(op);
            _focusA.SubmitChanges();
        }

        /// <summary>
        /// Возврат имени кассира по id
        /// </summary>
        /// <param name="conn">соединение с postgres</param>
        /// <param name="id_employee">id кассира</param>
        /// <returns></returns>
        private static string getCashier(NpgsqlConnection conn,int id_employee)
        {
            using (var cmd = new NpgsqlCommand())
            {
                //TODO не работает в этом гребанном postgress!!!!!!!!!!!!!!!!!!

                cmd.Connection = conn;
                cmd.CommandText = "select id_employee, name_employee from front.employees where id_employee=@id_employee";
                cmd.Parameters.AddWithValue("id_employee", id_employee);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader["name_employee"].ToString().Substring(1,15);
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// Процедура вставки в базу поступлений и снятий денег код (10 и 15)
        /// </summary>
        /// <param name="_focusA">Linq база</param>
        /// <param name="tAttrs">объект атрибутов с суммой</param>
        /// <param name="_dt">дата и время в int</param>
        /// <param name="_fp">фискальный регистратор</param>
        /// <param name="TypeOfMoney">тип денег</param>
        /// <param name="TypeOfOperation">тип операции</param>
        private static void insertCashIO(DataClassesFocusADataContext _focusA, object tAttrs, long _dt, int _fp, bool TypeOfMoney, int TypeOfOperation)
        {
            var trow = _focusA.GetTable<tbl_CashIO>().FirstOrDefault(i => i.FPNumber == _fp && i.DATETIME == _dt && i.Operation == TypeOfOperation);
            if (trow != null)
            {
                logger.Trace("Operation InCash in base");
                return;
            }

            tbl_CashIO cashIO = new tbl_CashIO
            {
                DATETIME = _dt,
                FPNumber = _fp,
                Operation = TypeOfOperation, //24,
                Type = TypeOfMoney,
                Money = returnMoney(tAttrs)// reader["attrs"]),
            };
            cashIO.Old_Money = cashIO.Money;
            _focusA.tbl_CashIOs.InsertOnSubmit(cashIO);
            _focusA.SubmitChanges();
            tbl_Operation op = new tbl_Operation
            {
                NumSlave = cashIO.id,
                DateTime = _dt,
                FPNumber = _fp,
                Operation = TypeOfOperation,
                InWork = false,
                Closed = false,
                Disable = false,
                CurentDateTime = DateTime.Now
            };
            _focusA.tbl_Operations.InsertOnSubmit(op);
            _focusA.SubmitChanges();
        }

        /// <summary>
        /// Анализ на словарь и возвращение суммы по значению
        /// </summary>
        /// <param name="inDic">объектное поле</param>
        /// <returns></returns>
        private static int returnMoney(object inDic)
        {
            Type t = inDic.GetType();
            bool isDict = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>);
            if (isDict)
            {
                Dictionary<string, string> idic = (Dictionary<string, string>)inDic;
                return idic.Sum(s => int.Parse(s.Value));
            }

            return 0;
        }

        /// <summary>
        /// Преобразование даты в long
        /// </summary>
        /// <param name="inDateTime">Дата время DateTime</param>
        /// <returns></returns>
        private static long getintDateTime(DateTime inDateTime)
        {
            //string sinDateTime = inDateTime.ToString("yyyyMMddHHmmss");

            return inDateTime.Year * 10000000000 + inDateTime.Month * 100000000 + inDateTime.Day * 1000000 + inDateTime.Hour * 10000 + inDateTime.Minute * 100 + inDateTime.Second;
        }

        /// <summary>
        /// Запрос в postgresql для выборки данны в логе
        /// </summary>
        /// <param name="inFPNumber">Код аппарата из postgres</param>
        /// <param name="dBegin">Дата начала</param>
        /// <param name="dEnd">Дата окончания</param>
        /// <returns></returns>
        private static string getQueryLog(string inFPNumber, DateTime dBegin, DateTime dEnd)
        {
            //TODO не забуть про +1 секунду вконце
            string ret = @"select sales_log.*, employees.name_employee 
			                                from sales.sales_log sales_log
                                            left join front.employees as employees  
											on employees.id_employee=sales_log.id_employee
			                                where sales_log.id_workplace = " + inFPNumber + @"
                                                and sales_log.id_action in (1,2, 12, 13, 14, 1001)                                                
                                                and sales_log.time_create>='" + dBegin.ToString("dd.MM.yyyy HH:mm:ss") + @"'
                                                and sales_log.time_create<'" + dEnd.AddSeconds(1).ToString("dd.MM.yyyy HH:mm:ss") + @"'
			                               ";

            return ret;
        }

        private enum LogOperations
        {
            Launch = 1,
            InCash = 2,
            Check = 3,
            OutCash = 12,
            Xreport = 13,
            Zreport = 14,
            SetCashier = 1001
        }
    }
}
