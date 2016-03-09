using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncHameleon.DBHelper
{


    class Checks
    {
        public long id_check { get; set; }
        public string id_registrar { get; set; }
        public Guid guid { get; set; }
        public int id_workplace { get; set; }
        public int id_session { get; set; }
        public int id_scheck { get; set; }
        public int id_fcheck { get; set; }
        public DateTime time_check { get; set; }
        public long DATETIME { get; set; }
        public int Type { get; set; }
        public int id_employee { get; set; }
        public int sum_discount { get; set; }
        public int sum_check { get; set; }
        public int type_payment { get; set; }
        public int Operation { get; set; }
        public int id_discount_card { get; set; }
        public Dictionary<string, string> attrs { get; set; }
        public DateTime time_create { get; set; }
        public DateTime time_change { get; set; }

        public Checks(Npgsql.NpgsqlDataReader inReader)
        {
            this.id_check = (long)inReader["id_check"];
            this.id_registrar = inReader["id_registrar"].ToString();
            this.id_workplace = (int)inReader["id_workplace"];
            this.id_session = (int)inReader["id_session"];
            this.id_scheck = (int)inReader["id_scheck"];
            this.id_fcheck = (int)inReader["id_fcheck"];
            this.time_check = (DateTime)inReader["time_check"];
            this.DATETIME = getintDateTime(this.time_check);
            this.id_employee = (int)inReader["id_employee"];
            this.type_payment = (int)inReader["type_payment"];
            if ((int)inReader["sum_check"] < 0)
            {
                this.Type = 1;
                this.sum_discount = -(int)inReader["sum_discount"];
                this.sum_check = -(int)inReader["sum_check"];
                this.Operation = 5;
            }
            else
            {
                this.Type = 0;
                this.sum_discount = (int)inReader["sum_discount"];
                this.sum_check = (int)inReader["sum_check"];
                this.Operation = 12;
            }
            this.id_discount_card = (int)inReader["id_discount_card"];
            if (inReader["attrs"].GetType().Name.ToLower() != "dbnull")
                this.attrs = (Dictionary<string, string>)inReader["attrs"];
            this.time_create = (DateTime)inReader["time_create"];
            this.time_change = (DateTime)inReader["time_change"];

        }

        /// <summary>
        /// Преобразование даты в long
        /// </summary>
        /// <param name="inDateTime">Дата время DateTime</param>
        /// <returns></returns>
        private long getintDateTime(DateTime inDateTime)
        {
            return inDateTime.Year * 10000000000 + inDateTime.Month * 100000000 + inDateTime.Day * 1000000 + inDateTime.Hour * 10000 + inDateTime.Minute * 100 + inDateTime.Second;
        }
    }

    class Check_Lines
    {
        public long id_check_line { get; set; }
        public Guid guid { get; set; }
        public long id_check { get; set; }
        public int id_goods { get; set; }
        public string print_name_goods { get; set; }
        public int id_tax { get; set; }
        public int id_unit { get; set; }
        public string name_unit { get; set; }
        public int type_unit { get; set; }
        public string id_series { get; set; }
        public string name_series { get; set; }
        public decimal quantity { get; set; }
        public int price { get; set; }
        public int discount { get; set; }
        public int summ { get; set; }
        public Dictionary<string, string> attrs { get; set; }
        public DateTime time_create { get; set; }
        public DateTime time_change { get; set; }
        //
        //Совместимость
        //
        public int Type { get; set; }
        public long SORT { get; set; }
        public int Amount { get; set; }
        public int Amount_Status { get; set; }
        public int NalogGroup { get; set; }
        public string GoodName { get; set; }
        public string StrCode { get; set; }
        public int packname { get; set; }
        public Guid PackGuid { get; set; }
        public int RowSum { get; set; }



        public Check_Lines(Npgsql.NpgsqlDataReader inReader)
        {
            this.id_check_line = (long)inReader["id_check_line"];
            this.id_check = (long)inReader["id_check"];
            this.id_goods = (int)inReader["id_goods"];
            this.print_name_goods = inReader["print_name_goods"].ToString();
            this.id_tax = (int)inReader["id_tax"];
            this.id_unit = (int)inReader["id_unit"];
            this.name_unit = inReader["name_unit"].ToString();
            this.type_unit = (short)inReader["type_unit"];
            this.quantity = Math.Abs((decimal)inReader["quantity"]);
            
            this.id_series = inReader["id_series"].ToString();
            this.name_series = inReader["name_series"].ToString();
            
            this.price = (int)inReader["price"];
            this.discount = (int)inReader["discount"];
            this.summ = (int)inReader["summ"];
            if (inReader["attrs"].GetType().Name.ToLower() != "dbnull")
                this.attrs = (Dictionary<string, string>)inReader["attrs"];
            this.time_create = (DateTime)inReader["time_create"];
            this.time_change = (DateTime)inReader["time_change"];
            this.Type = 0;

            if ((int)inReader["summ"] < 0)
            {
                this.Type = 1;                
                this.summ = -(int)inReader["summ"];
            }
            //
            //
            //
            this.Amount_Status = 0;
            this.Amount = (int)quantity;
            if (this.type_unit != 1)
            {
                this.Amount_Status = 3;
                this.Amount = (int)Math.Abs((decimal)inReader["quantity"] * 1000);
            }
            
            //
            //
            //
            this.NalogGroup = this.id_tax - 1;
            this.GoodName = this.print_name_goods+" "+ this.name_unit;
            //Серии отключаем от пробивки из-за ошибок у postgres
            //if ((this.name_series!=null)&&(this.name_series.Length>0))
            //{
            //    this.GoodName += " "+this.name_series;
            //}
            this.GoodName = this.GoodName.Substring(0,Math.Min(75,this.GoodName.Length));
            this.StrCode = inReader["id_goods"].ToString();
            this.packname = (int)inReader["id_unit"];
            this.SORT = (long)inReader["id_check_line"];

        }

        
    }

    struct Payment
    {
        public int Payment0 { get; set; }
        public int Payment1 { get; set; }
        public int Payment2 { get; set; }
        public int Payment3 { get; set; }
        public int Payment4 { get; set; }
        public int Payment5 { get; set; }
        public int Payment6 { get; set; }
        public int Payment7 { get; set; }
        public int PaymentSum { get; set; }

        public Payment(Dictionary<string, string> inDic, int TypePayment)
        {
            this.Payment0 = 0;
            this.Payment1 = 0;
            this.Payment2 = 0;
            this.Payment3 = 0;
            this.Payment4 = 0;
            this.Payment5 = 0;
            this.Payment6 = 0;
            this.Payment7 = 0;


            if (TypePayment == 999)
            {
                this.Payment0 = int.Parse(inDic["card"]);
                this.Payment3 = int.Parse(inDic["cash"]);
            }
            else if (TypePayment == 1)
            {
                this.Payment3 = int.Parse(inDic["cash"]);
            }
            else if (TypePayment == 2)
            {
                this.Payment0 = int.Parse(inDic["card"]);
            }
            this.PaymentSum = Payment0
                            + Payment1
                            + Payment2
                            + Payment3
                            + Payment4
                            + Payment5
                            + Payment6
                            + Payment7;

        }
    }
}
