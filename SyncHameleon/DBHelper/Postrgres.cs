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
using System.Data;
using NpgsqlTypes;

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
            _timer.Stop();
            SelectChecksOperation();
            SelectLogOperation();
            _timer.Start();
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
        /// Выборка и обработка чеков
        /// </summary>
        private static void SelectChecksOperation()
        {

            StopwatchHelper.Start("Begin select CHECKS");

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
                        //logger.Trace("NpgsqlConnection:{0}", connection);
                        conn.Open();


                        using (var cmd = new NpgsqlCommand())
                        {
                            cmd.Connection = conn;
                            cmd.CommandText = @"select *
			                                from sales.checks checks                                            
			                                where checks.id_workplace = :RealNumber                                                
                                                and type_payment in (1,2)
                                                and checks.time_check>='" + tBegin.ToString("dd.MM.yyyy HH:mm:ss") + @"'
                                                and checks.time_check<'" + tEnd.AddSeconds(1).ToString("dd.MM.yyyy HH:mm:ss") + @"'
			                               ";
                            cmd.Parameters.Add(new NpgsqlParameter("RealNumber", DbType.Int32));
                            cmd.Parameters[0].Value = initRow.RealNumber;
                            //logger.Trace("Select from base:{0}", cmd.CommandText);
                            StopwatchHelper.Start("ExecuteReader");
                            List<DBHelper.Checks> listChecks = new List<DBHelper.Checks>();
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {                                    
                                    listChecks.Add(new DBHelper.Checks(reader));
                                    //long _dt = getintDateTime((DateTime)reader["time_check"]);
                                    //int _fp = (int)initRow.FPNumber;
                                    //logger.Trace("Time:{0}\tOp:{1}", reader["time_check"], reader["id_employee"]);
                                }
                            }
                            StopwatchHelper.Stop("ExecuteReader");

                            Table<tbl_Payment> tablePayment = _focusA.GetTable<tbl_Payment>();
                            //var query =
                            //    from list in tablePayment
                            //    where list.FPNumber == (int)initRow.FPNumber
                            //    && list.DATETIME >= initRow.DateTimeBegin && list.DATETIME < initRow.DateTimeStop                                
                            //    select list;
                            //List<tbl_Payment> listPayment = query.ToList<tbl_Payment>();

                            var linked = (from lC in listChecks
                                          join lP in (from list in tablePayment
                                                      where list.FPNumber == (int)initRow.FPNumber
                                                      && list.DATETIME >= initRow.DateTimeBegin && list.DATETIME < initRow.DateTimeStop
                                                      select list)
                                          on    new { Operation = lC.Operation, DATETIME = lC.DATETIME, id_workplace = lC.id_workplace, id_session =lC.id_session, id_scheck = (int)lC.id_scheck,   id_check = lC.id_check } 
                                          equals 
                                                new { Operation = lP.Operation, DATETIME = lP.DATETIME, id_workplace = lP.SAREAID,      id_session = lP.SESSID,     id_scheck = (int)lP.SRECNUM,     id_check = lP.SYSTEMID }
                                          select lC);
                            //var T = linked.ToList();
                            var notLinked = listChecks.Except(linked);
                            List<tbl_Payment> listPayment = new List<tbl_Payment>();
                            foreach (DBHelper.Checks check in notLinked)
                            {
                                listPayment.Add(insertPayment(_focusA, initRow, check));
                            }


                            using (var cmd1 = new NpgsqlCommand())
                            {
                                cmd1.Connection = conn;
                                cmd1.CommandText = @"select check_lines.*,goods_attrs.print_name_goods , series.name_series, goods.id_tax
			                                            from sales.check_lines check_lines
											            left join front.goods_attrs goods_attrs
											                on goods_attrs.id_goods = check_lines.id_goods
                                                        left join front.goods goods
											                on goods.id_goods = check_lines.id_goods
											            left join front.series series
											                on check_lines.id_series = series.id_series and check_lines.id_goods = series.id_goods 
											                    and  check_lines.id_series != '-'                                              
                                                        where true  and check_lines.id_workplace = :RealNumber                                                
                                                            --and check_lines.id_check in (:arrayChecks)
                                                            and check_lines.time_create>='" + tBegin.ToString("dd.MM.yyyy HH:mm:ss") + @"'
                                                            and check_lines.time_create<'" + tEnd.AddSeconds(1).ToString("dd.MM.yyyy HH:mm:ss") + @"'
                                  ";
                                cmd1.Parameters.Add(new NpgsqlParameter("RealNumber", DbType.Int32));
                                cmd1.Parameters.Add(new NpgsqlParameter("arrayChecks", NpgsqlDbType.Array| NpgsqlDbType.Bigint));
                                cmd1.Parameters[0].Value = initRow.RealNumber;
                                cmd1.Parameters[1].Value = listPayment.Select(x=>x.SYSTEMID).ToArray();
                                StopwatchHelper.Start("ExecuteReader1");
                                List<DBHelper.Check_Lines> listChecks_Lines = new List<DBHelper.Check_Lines>();

                                using (var reader1 = cmd1.ExecuteReader())
                                {
                                    while (reader1.Read())
                                    {
                                        listChecks_Lines.Add(new DBHelper.Check_Lines(reader1));
                                    }
                                }
                                StopwatchHelper.Stop("ExecuteReader1");
                            }


                        }
                    }
                }
            }
            StopwatchHelper.Stop("Begin select CHECKS");
        }

        private static tbl_Payment insertPayment(DataClassesFocusADataContext _focusA, tbl_ComInit initRow, DBHelper.Checks check)
        {
            //TODO Скорее всего нужно будет смотреть в логи и искать сколько денег дал покупатель, пока данных нет
            // код в логах =19
            DBHelper.Payment tPay = new DBHelper.Payment(check.attrs, check.type_payment);
            tbl_Payment payment = new tbl_Payment
            {
                //NumOperation
                FPNumber = (int)initRow.FPNumber,
                DATETIME = check.DATETIME,
                Operation = check.Operation,
                SESSID = check.id_session,
                SYSTEMID = check.id_check,
                SAREAID = check.id_workplace,
                Type = check.Type,
                FRECNUM = check.id_fcheck.ToString(),
                SRECNUM = check.id_scheck,
                Payment_Status = 11,
                Payment = tPay.PaymentSum,//returnMoney(check.attrs),
                Payment0 = tPay.Payment0,
                Payment1 = tPay.Payment1,
                Payment2 = tPay.Payment2,
                Payment3 = tPay.Payment3,
                Payment4 = tPay.Payment4,
                Payment5 = tPay.Payment5,
                Payment6 = tPay.Payment6,
                Payment7 = tPay.Payment7,
                CheckClose = false,
                //[FiscStatus]
                CommentUp = "",
                Comment = "",
                //[Old_Payment]                                    
                CheckSum = check.sum_check - check.sum_discount,
                //[PayBonus]
                //[BousInAcc]
                //[BonusCalc]
                Card = check.id_discount_card,//TODO дописать определитель карточек при оплате
                ForWork = true,
                RowCount = 0,
                Disable = false

            };
            _focusA.tbl_Payments.InsertOnSubmit(payment);
            _focusA.SubmitChanges();
            return payment;
        }



        /// <summary>
        /// Процедура анализа лога и обновление базы
        /// </summary>
        private static void SelectLogOperation()
        {

            StopwatchHelper.Start("Begin select LOG");

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
                            cmd.CommandText = @"select sales_log.*, employees.name_employee 
			                                from sales.sales_log sales_log
                                            left join front.employees as employees  
											on employees.id_employee=sales_log.id_employee
			                                where sales_log.id_workplace = '" + initRow.RealNumber + @"'
                                                and sales_log.id_action in (1,2, 12, 13, 14, 1001)                                                
                                                and sales_log.time_create>='" + tBegin.ToString("dd.MM.yyyy HH:mm:ss") + @"'
                                                and sales_log.time_create<'" + tEnd.AddSeconds(1).ToString("dd.MM.yyyy HH:mm:ss") + @"'
			                               ";
                            //logger.Trace("Select from base:{0}", cmd.CommandText);
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
                                    //logger.Trace("Time:{0}\tOp:{1}", reader["time_sales"], reader["id_employee"]);
                                    //Console.WriteLine(reader.GetString(0));
                                }
                            }
                            StopwatchHelper.Stop("ExecuteReader");
                        }
                    }
                }
            }
            StopwatchHelper.Stop("Begin select LOG");


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
                DATETIME = _dt,
                FPNumber = _fp,
                Num_Cashier = 0,
                Name_Cashier = inName_Cashier.TrimStart().Substring(0, 15).Trim(), //TODO тут должно быть имя
                Pass_Cashier = 0,
                TakeProgName = false,
                Operation = 3
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
        private static string getCashier(NpgsqlConnection conn, int id_employee)
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
                        return reader["name_employee"].ToString().Substring(1, 15);
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
			                                where sales_log.id_workplace = '" + inFPNumber + @"'
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
