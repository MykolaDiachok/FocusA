//#define Debug
//#define DebugErrorInfo

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using CentralLib;
using CentralLib.Connections;
using System.Collections;
using System.Threading;
using CentralLib.Helper;


namespace CentralLib.Protocols
{
    /// <summary>
    /// протокол обмена с фискальным регистратором
    /// </summary>
    /// public partial class Protocols!!!!!!!!
    public class Protocol_EP11 : BaseProtocol, IProtocols
    {
        private IConnectFactory connFP = null;

        /// <summary>
        /// Статус последней операции true - завершено, false - сбой
        /// </summary>
        //public bool statusOperation { get; private set; }

        /// <summary>
        /// Байт статуса
        /// </summary>
        //public byte ByteStatus { get; private set; } // Возврат ФР статус

        //public strByteStatus structStatus
        //{
        //    get
        //    {               
        //        return new strByteStatus(ByteStatus);
        //    }
        //}

        /// <summary>
        /// Байт результат
        /// </summary>
        //public byte ByteResult { get; private set; } // Возврат ФР результат

        //public strByteResult structResult
        //{
        //    get
        //    {
        //        return new strByteResult(ByteResult);
        //    }
        //}

        /// <summary>
        /// Байт резерва
        /// </summary>
        //public byte ByteReserv { get; private set; } // Возврат ФР результат

        //public strByteReserv structReserv
        //{
        //    get
        //    {
        //        return new strByteReserv(ByteReserv);
        //    }
        //}
        //public WorkProtocol currentProtocol { get; private set; }
        //private CentralLib.Connections.ConnectFactory connFP = null;

        //public string errorInfo { get; protected set; }
        private Status tStatus;
        public override Taxes currentTaxes
        {
            get;  set;
        }

        //private ByteHelper byteHelper;

        public override Status status
        {
            get
            {
                if ((lastByteCommand != 0))
                    getStatus();
                return this.tStatus;
            }
        }

        //private byte? lastByteCommand = null;
        //public bool useCRC16 { get; private set; }

        /// <summary>
        /// Основные комманды для первичной инициализации класса, такие как получить статус и получить данные по налогам
        /// </summary>
        private void initial()
        {            
            getStatus();
            if (!(bool)structStatus.ExceedingOfWorkingShiftDuration)
                FPGetTaxRate();
        }


        /// <summary>
        /// Передаем класс для подключения. Рекомендую использовать сразу код порта
        /// </summary>
        /// <param name="connFP"></param>
        public Protocol_EP11(CentralLib.Connections.DefaultPortCom dComPort):base(dComPort)
        {
            useCRC16 = true;
            MaxStringLenght = 75;
            initial();


        }



        /// <summary>
        /// Класс инициализации приложения
        /// </summary>
        /// <param name="serialPort"></param>
        public Protocol_EP11(int serialPort):base(serialPort)
        {
            useCRC16 = true;
            MaxStringLenght = 75;
            initial();
            
        }

        public Protocol_EP11(string IpAdress,int port):base(IpAdress,port)
        {
            useCRC16 = true;
            MaxStringLenght = 75;
            initial();

        }

        /// <summary>
        /// Основная функция обмена для протокола, сюда передаем массив байтов, на выходе массив байтов ответа ФР, при этом передаются только данные.
        /// Вся проверка, подготовка выподняется в  connFP.dataExchange
        /// </summary>
        /// <param name="inputByte"></param>
        /// <returns></returns>
        //        private byte[] ExchangeWithFP(byte[] inputByte)
        //        {
        //            byte[] answer;
        //            base.lastByteCommand = inputByte[0];
        //            answer = connFP.dataExchange(inputByte, useCRC16, false);
        //            if (!connFP.statusOperation) //repetition if error
        //            {
        //                Thread.Sleep(800);
        //#if Debug
        //                Console.ForegroundColor = ConsoleColor.Yellow;
        //                Console.WriteLine("ошибка первое ожидание");
        //                ///Console.ReadKey();
        //#endif
        //                answer = connFP.dataExchange(inputByte, useCRC16, true);
        //            }
        //            if (!connFP.statusOperation) //repetition if error
        //            {
        //                //TODO: большая проблема искать в чем причина
        //                Thread.Sleep(800);
        //                answer = connFP.dataExchange(inputByte, useCRC16, true);
        //#if Debug
        //                Console.ForegroundColor = ConsoleColor.DarkYellow;
        //                Console.WriteLine("Вторая ошибка");
        //                Console.ReadKey();
        //#endif
        //            }
        //            this.ByteStatus = connFP.ByteStatus;
        //            this.ByteResult = connFP.ByteResult;
        //            this.ByteReserv = connFP.ByteReserv;
        //            this.errorInfo = connFP.errorInfo;

        //            this.statusOperation = connFP.statusOperation;
        //#if DebugErrorInfo
        //            Console.WriteLine("Send:{0}", PrintByteArrayX(inputByte));
        //            Console.WriteLine("Resive:{0}", PrintByteArrayX(answer));
        //            Console.WriteLine("statusOperation:{0}", statusOperation);
        //            Console.WriteLine("errorInfo:{0}", errorInfo);
        //            Console.WriteLine("ByteStatus:{0}", ByteStatus);
        //            Console.WriteLine("ByteResult:{0}", ByteResult);
        //            Console.WriteLine("ByteReserv:{0}", ByteReserv);
        //#endif
        //            return answer;
        //        }

        /// <summary>
        /// Код: 0. SendStatus 	 	прочитать состояние регистратора 
        /// </summary>
        public override void getStatus()
        {
            byte[] forsending = new byte[] { 0 };
            byte[] answer = ExchangeWithFP(forsending);

            if ((connFP.statusOperation) && (answer.Length > 21))
            {
                string hexday = answer[21].ToString("X");
                int _day = Math.Min(Math.Max((int)Convert.ToInt16(hexday), 1), 31);

                string hexmonth = answer[22].ToString("X");
                int _month = Math.Min(Math.Max((int)Convert.ToInt16(hexmonth), 1), 12);

                string hexyear = answer[23].ToString("X");
                int _year = Convert.ToInt16(hexyear);

                string hexhour = answer[24].ToString("X");
                int _hour = Math.Min(Math.Max((int)Convert.ToInt16(hexhour), 0), 23);

                string hexmin = answer[25].ToString("X");
                int _min = Math.Min(Math.Max((int)Convert.ToInt16(hexmin), 0), 59);

                int curCountByte = 26;
                string fiscalNumber =new ByteHelper().EncodingBytes(answer, curCountByte, 10);
                curCountByte = curCountByte + 10;


                byte tlen1 = answer[curCountByte];
                tlen1 = byteHelper.SetBit(tlen1, 6, false);
                tlen1 = byteHelper.SetBit(tlen1, 7, false);
                int len1 = tlen1;

                string str1 = "";
                if (len1 > 0)
                {
                    curCountByte++;
                    str1 = byteHelper.EncodingBytes(answer, curCountByte, len1);

                }
                curCountByte = curCountByte + len1;

                byte tlen2 = answer[curCountByte];
                tlen2 = byteHelper.SetBit(tlen2, 6, false);
                tlen2 = byteHelper.SetBit(tlen2, 7, false);
                int len2 = tlen2;
                string str2 = "";
                if (len2 > 0)
                {
                    curCountByte++;
                    str2 = byteHelper.EncodingBytes(answer, curCountByte, len2);

                }
                curCountByte = curCountByte + len2;

                byte tlen3 = answer[curCountByte];
                tlen3 = byteHelper.SetBit(tlen3, 6, false);
                tlen3 = byteHelper.SetBit(tlen3, 7, false);
                int len3 = tlen3;
                string str3 = "";
                if (len3 > 0)
                {
                    curCountByte++;
                    str3 = byteHelper.EncodingBytes(answer, curCountByte, len3);

                }
                curCountByte = curCountByte + len3;

                byte tlenTax = answer[curCountByte];
                tlenTax = byteHelper.SetBit(tlenTax, 6, false);
                tlenTax = byteHelper.SetBit(tlenTax, 7, false);
                int lenTax = tlenTax;
                string strTax = "";
                if (lenTax > 0)
                {
                    curCountByte++;
                    strTax = byteHelper.EncodingBytes(answer, curCountByte, lenTax);

                }
                curCountByte = curCountByte + len3;


                //string ver = EncodingBytes(answer.Skip(answer.Length-6).Take(5).ToArray());
                byte[] verBytes = new byte[5];
                System.Buffer.BlockCopy(answer, answer.Length - 5, verBytes, 0, 5);
                string ver = byteHelper.EncodingBytes(verBytes);
                //switch (ver)
                //{
                //    case "ЕП-11":
                //        this.currentProtocol = WorkProtocol.EP11;
                //        //this.useCRC16 = true;
                //        break;
                //};

                this.tStatus = new Status(answer.Take(2).ToArray()
                    , byteHelper.EncodingBytes(answer.Skip(2).Take(19).ToArray())
                    , new DateTime(2000 + _year, _month, _day, _hour, _min, 0)
                    , fiscalNumber
                    , str1
                    , str2
                    , str3
                    , strTax
                    , ver
                    , connFP.ConsecutiveNumber
                    );
            }
            else
            {
                this.statusOperation = false;
            }

        }

        private PapStat tpapStat;

        public override PapStat papStat
        {
            get
            {
                if ((lastByteCommand != 48))
                    getGetPapStat();
                return tpapStat;
            }
        }

        /// <summary>
        /// Код: 48.GetPapStat прочитать состояние бумаги в принтере
        /// </summary>
        private void getGetPapStat()
        {
            byte[] forsending = new byte[] { 48 };
            byte[] answer = ExchangeWithFP(forsending);

            if ((connFP.statusOperation) && (answer.Length == 1))
            {
                this.tpapStat = new PapStat(answer[0]);
            }
            else
            {
                this.statusOperation = false;
            }
        }

        private DayReport tDayReport;
        public override DayReport dayReport
        {
            get
            {
                if (lastByteCommand != 42)
                    tDayReport = getDayReport();
                return tDayReport;
            }
        }

        private DayReport getDayReport()
        {
            byte[] bytesReturn, bytesReturn0, bytesReturn1, bytesReturn2, bytesReturn3;
            {
                byte[] forsending = new byte[] { 42 };
                byte[] answer = ExchangeWithFP(forsending);
                if ((connFP.statusOperation) && (answer.Length > 0))
                {
                    bytesReturn = answer;
                }
                else
                {
                    this.statusOperation = false;
                    return new DayReport();
                }
            }
            {
                byte[] forsending = new byte[] { 42, 0 };
                byte[] answer = ExchangeWithFP(forsending);
                if ((connFP.statusOperation) && (answer.Length > 0))
                {
                    bytesReturn0 = answer;
                }
                else
                {
                    this.statusOperation = false;
                    return new DayReport();
                }
            }
            {
                byte[] forsending = new byte[] { 42, 1 };
                byte[] answer = ExchangeWithFP(forsending);
                if ((connFP.statusOperation) && (answer.Length > 0))
                {
                    bytesReturn1 = answer;
                }
                else
                {
                    this.statusOperation = false;
                    return new DayReport();
                }
            }
            {
                byte[] forsending = new byte[] { 42, 2 };
                byte[] answer = ExchangeWithFP(forsending);
                if ((connFP.statusOperation) && (answer.Length > 0))
                {
                    bytesReturn2 = answer;
                }
                else
                {
                    this.statusOperation = false;
                    return new DayReport();
                }
            }
            {
                byte[] forsending = new byte[] { 42, 3 };
                byte[] answer = ExchangeWithFP(forsending);
                if ((connFP.statusOperation) && (answer.Length > 0))
                {
                    bytesReturn3 = answer;
                }
                else
                {
                    this.statusOperation = false;
                    return new DayReport();
                }
            }

            return new DayReport(bytesReturn, bytesReturn0, bytesReturn1, bytesReturn2, bytesReturn3);
        }

       

        /// <summary>
        /// Код: 32.       PrintVer печать налогового номера и версии программного обеспечения
        /// Налоговый номер и дата регистрации ЭККР печатаются только в фискальном режиме.
        /// </summary>
        public override void FPPrintVer()
        {
            byte[] forsending = new byte[] { 32 };
            byte[] answer = ExchangeWithFP(forsending);
        }

        /// <summary>
        /// Код: 29. OpenBox открытие денежного ящика
        ///
        /// При отсутствии параметра на денежный ящик подается импульс 200мс.
        /// </summary>
        /// <param name="impulse"> длительность импульса открытия в 2мс </param>
        public override void FPOpenBox(byte impulse = 0) //обнуление чека
        {
            byte[] forsending = new byte[] { 29 };
            if (impulse != 0)
                forsending = new byte[] { 29, impulse };
            byte[] answer = ExchangeWithFP(forsending);
        }

        /// <summary>
        ///  Код: 38. запрет/разрешение режима OnLine регистраций
        ///  В режиме OnLine регистрация продажи, выплаты, оплаты, комментариев, скидок\наценок
        ///  сопровождается печатью в чеке.Команда запрещена при открытом чеке. Вызов команды меняет
        ///  значение параметра на противоположный.
        /// </summary>
        public override void FPCplOnline() // Код: 38. запрет/разрешение режима OnLine регистраций
        {
            byte[] forsending = new byte[] { 38 };
            byte[] answer = ExchangeWithFP(forsending);
        }

        #region установка кассира

        /// <summary>
        ///  Код: 5.SetCod                   установка пароля
        /// </summary>
        /// <param name="UserID">номер (0-7 – пароли кассиров, 8 – пароль режима программирования, 9 – пароль режима отчетов)</param>
        /// <param name="OldPassword"> старый пароль</param>
        /// <param name="NewPassword">новый пароль</param>
        public override void FPSetPassword(byte UserID, ushort OldPassword, ushort NewPassword)
        {
            byte[] forsending = new byte[] { 5 };//SetCod
            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(OldPassword));
            forsending = byteHelper.Combine(forsending, new byte[] { UserID });
            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(NewPassword));
            byte[] answer = ExchangeWithFP(forsending);
        }

        
        #endregion


        #region Чеки

            


        /// <summary>
        /// Код: 8. PayMoney                  регистрация выплаты 
        /// Команда  запрещена,  если  не  зарегистрированы  налоговые  ставки.  Рассчитанная  стоимость  не  
        /// должна превышать  999.999,99  грн.Сумма по  чеку не  должна превышать  21.474.836,47  грн.При
        /// отрицательной цене(для скидки, отказа от  предыдущей регистрации  и пр.)  стоимость не  должна
        /// превышать промежуточную сумму  по предыдущим  выплатам.После закрытия  чека в  параметрах
        /// артикулов соответствующих кодов   меняются значения   статусов на   больший(с увеличением
        /// разрядности меньшего),     увеличивается его    количество и   стоимость,      если артикулы
        /// запрограммированы,  или полностью  заносится описание  артикула,  если не  запрограммированы.
        /// ЭККР запрещает  изменение налоговой  группы,  название выплаты, а  в пределах  чека,  и цены.        
        /// Группа Е – непрограммируемая необлагаемая группа.
        /// </summary>
        /// <param name="Amount">количество или вес</param>
        /// <param name="Amount_Status">число десятичных разрядов в количестве</param>
        /// <param name="IsOneQuant">количество 1 не печатается в чеке)</param>
        /// <param name="Price">цена в коп (бит 31 = 1 – отрицательная цена)</param>
        /// <param name="NalogGroup">налоговая группа</param>
        /// <param name="MemoryGoodName">n=255 – название взять из памяти</param>
        /// <param name="GoodName">название товара или услуги (для n # 255)</param>
        /// <param name="StrCode">код товара</param>
        /// <param name="PrintingOfBarCodesOfGoods">=1 – печать штрих-кода товара (EAN13)</param>
        /// <returns></returns>
        public override ReceiptInfo FPPayMoneyEx(UInt16 Amount, byte Amount_Status, bool IsOneQuant, Int32 Price, ushort NalogGroup, bool MemoryGoodName, string GoodName, UInt64 StrCode, bool PrintingOfBarCodesOfGoods = false)
        {
            byte[] forsending = new byte[] { 8 };

            forsending = byteHelper.Combine(forsending, byteHelper.ConvertUint32ToArrayByte3(Amount));
            Amount_Status = byteHelper.SetBit(Amount_Status, 6, IsOneQuant);
            Amount_Status = byteHelper.SetBit(Amount_Status, 7, PrintingOfBarCodesOfGoods);
            forsending = byteHelper.Combine(forsending, new byte[] { Amount_Status });
            Int32 _price = Price;
            //BitArray b_price = new BitArray(BitConverter.GetBytes(_price));
            if (Price < 0)
            {
                _price = -_price;
                _price = _price ^ (1 << 31);
            }
            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(_price));
            byte[] VAT = new byte[] { 0x80 }; ;
            if (NalogGroup == 1)
                VAT = new byte[] { 0x81 };
            else if (NalogGroup == 2)
                VAT = new byte[] { 0x82 };
            else if (NalogGroup == 3)
                VAT = new byte[] { 0x83 };
            else if (NalogGroup == 4)
                VAT = new byte[] { 0x84 };
            else if (NalogGroup == 5)
                VAT = new byte[] { 0x85 };
            forsending = byteHelper.Combine(forsending, VAT);

            if (MemoryGoodName)
                forsending = byteHelper.Combine(forsending, new byte[] { 255 });
            else
            {
                forsending = byteHelper.Combine(forsending, byteHelper.CodingStringToBytesWithLength(GoodName, MaxStringLenght));
            }
            forsending = byteHelper.Combine(forsending, byteHelper.ConvertUint64ToArrayByte6(StrCode));
            byte[] answer = ExchangeWithFP(forsending);
            if ((statusOperation) && (answer.Length == 8))
            {
                ReceiptInfo _checkinfo = new ReceiptInfo();
                _checkinfo.CostOfGoodsOrService = BitConverter.ToInt32(answer, 0);
                _checkinfo.SumAtReceipt = BitConverter.ToInt32(answer, 4);
                return _checkinfo;
            }
            return new ReceiptInfo();
        }


        /// <summary>
        /// Код: 18.Sale                      регистрация продажи товара или услуги
        /// Команда  запрещена,  если  не  зарегистрированы  налоговые  ставки.  Рассчитанная  стоимость  не  
        /// должна превышать  999.999,99  грн.Сумма по  чеку не  должна превышать  21.474.836,47  грн.При
        /// отрицательной цене(для скидки, отказа от  предыдущей регистрации  и пр.)  стоимость не  должна
        /// превышать промежуточную сумму  по предыдущим  продажам.После закрытия  чека в  параметрах
        /// артикулов соответствующих кодов   меняется статус  на больший(с увеличением  разрядности
        /// меньшего),  увеличивается его  количество и  стоимость,  если артикулы  запрограммированы,  или
        /// полностью заносится описание  артикула, если не запрограммированы.ЭККР запрещает изменение
        /// налоговой группы, имени  товара,  а в  пределах чека, и  цены.Группа Е  –  непрограммируемая
        /// необлагаемая группа.
        /// </summary>
        /// <param name="Amount">количество или вес </param>
        /// <param name="Amount_Status">число десятичных разрядов в количестве,</param>
        /// <param name="IsOneQuant">количество 1 не печатается в чеке)</param>
        /// <param name="Price">цена в коп (бит 31 = 1 – отрицательная цена)</param>
        /// <param name="NalogGroup">налоговая группа</param>
        /// <param name="MemoryGoodName"></param>
        /// <param name="GoodName">название товара или услуги (для n # 255) </param>
        /// <param name="StrCode">код товара</param>
        /// <param name="PrintingOfBarCodesOfGoods">печать штрих-кода товара (EAN13)</param>
        public override ReceiptInfo FPSaleEx(UInt16 Amount, byte Amount_Status, bool IsOneQuant, Int32 Price, ushort NalogGroup, bool MemoryGoodName, string GoodName, UInt64 StrCode, bool PrintingOfBarCodesOfGoods = false)
        {
            byte[] forsending = new byte[] { 18 };

            forsending = byteHelper.Combine(forsending, byteHelper.ConvertUint32ToArrayByte3(Amount));
            Amount_Status = byteHelper.SetBit(Amount_Status, 6, IsOneQuant);
            Amount_Status = byteHelper.SetBit(Amount_Status, 7, PrintingOfBarCodesOfGoods);
            forsending = byteHelper.Combine(forsending, new byte[] { Amount_Status });
            Int32 _price = Price;
            //BitArray b_price = new BitArray(BitConverter.GetBytes(_price));
            if (Price < 0)
            {
                _price = -_price;
                _price = _price ^ (1 << 31);
            }
            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(_price));
            byte[] VAT = new byte[] { 0x80 }; ;
            if (NalogGroup == 1)
                VAT = new byte[] { 0x81 };
            else if (NalogGroup == 2)
                VAT = new byte[] { 0x82 };
            else if (NalogGroup == 3)
                VAT = new byte[] { 0x83 };
            else if (NalogGroup == 4)
                VAT = new byte[] { 0x84 };
            else if (NalogGroup == 5)
                VAT = new byte[] { 0x85 };
            forsending = byteHelper.Combine(forsending, VAT);

            if (MemoryGoodName)
                forsending = byteHelper.Combine(forsending, new byte[] { 255 });
            else
            {
                forsending = byteHelper.Combine(forsending, byteHelper.CodingStringToBytesWithLength(GoodName, MaxStringLenght));
            }
            forsending = byteHelper.Combine(forsending, byteHelper.ConvertUint64ToArrayByte6(StrCode));
            byte[] answer = ExchangeWithFP(forsending);
            if ((statusOperation) && (answer.Length == 8))
            {
                ReceiptInfo _checkinfo = new ReceiptInfo();
                _checkinfo.CostOfGoodsOrService = BitConverter.ToInt32(answer, 0);
                _checkinfo.SumAtReceipt = BitConverter.ToInt32(answer, 4);
                return _checkinfo;
            }
            return new ReceiptInfo();
        }

        /// <summary>
        /// Код: 20.Payment          регистрация оплаты и печать чека, если сума оплат не меньше
        /// Команда  запрещена при закрытом чеке. Чек закрывается автоматически и печатается, если  
        /// сумма оплат больше или равна сумме продаж или выплат, или установлен бит 31 в сумме оплат.В
        /// последнем случае сумма данной оплаты вычисляется ЭККР.Если сумма наличными больше суммы
        /// продаж, то будет  печататься сумма  сдачи.Оплата со  сдачей разрешена  только для  наличных.В
        /// чеке выплат оплата    наличными должна     быть не  более суммы     в денежном     ящике.Для
        /// нефискального чека  (обороты чека  не сохраняются  в дневных  счетчиках и  счетчиках артикулов)        
        /// рекомендуется  открывать чек  продаж.Нулевая оплата  не печатается  в чеках.Номер  пакета
        /// возвращается в случае закрытия чека.
        /// </summary>
        /// <param name="Payment_Status">статус (биты 0..3 - тип оплаты (см. команду 50);</param>
        /// <param name="Payment">оплата в коп. </param>
        /// <param name="CheckClose">автоматическое закрытие</param>
        /// <param name="FiscStatus">= 1 – закрытие чека как нефискальный</param>
        /// <param name="AuthorizationCode">код авторизации при оплате картой через платёжный терминал</param>
        /// <returns>остаток или сдача (бит 31 = 1 – сдача), номер пакета чека в КЛЕФ</returns>
        public override PaymentInfo FPPayment(byte Payment_Status, UInt32 Payment, bool CheckClose, bool FiscStatus, string AuthorizationCode="")
        {
            byte[] forsending = new byte[] { 20 };
            Payment_Status = byteHelper.SetBit(Payment_Status, 6, !FiscStatus);
            forsending = byteHelper.Combine(forsending, new byte[] { Payment_Status });
            byte[] bytePayment = BitConverter.GetBytes(byteHelper.WriteBitUInt32(Payment, 31, CheckClose));
            //byte[] bytePayment = BitConverter.GetBytes(Payment);
            //if (CheckClose)
            //{
            //    bytePayment[3] = SetBit(bytePayment[3], 7, CheckClose);
            //}
            ////forsending = Combine(forsending, bytePayment);
            //int _Payment = (int)Payment;
            //if (CheckClose)
            //    //b_Payment[31] = true;
            //    _Payment = _Payment ^ (1 << 31);
            forsending = byteHelper.Combine(forsending, bytePayment);
            forsending = byteHelper.Combine(forsending, new byte[] { 0 });
            if (AuthorizationCode.Length!=0)
                forsending = byteHelper.Combine(forsending, byteHelper.CodingStringToBytesWithLength(AuthorizationCode, 50));
            byte[] answer = ExchangeWithFP(forsending);
            if ((statusOperation) && (answer.Length  > 3))
            {
                PaymentInfo _paymentInfo = new PaymentInfo();
                UInt32 tinfo = BitConverter.ToUInt32(answer, 0);
                if (byteHelper.GetBit(answer[3],7))
                {
                    tinfo = byteHelper.ClearBitUInt32(tinfo, 31);
                    _paymentInfo.Renting = tinfo;
                }
                else
                    _paymentInfo.Rest = tinfo;
                if (answer.Length>=8)
                    _paymentInfo.NumberOfReceiptPackageInCPEF = BitConverter.ToUInt32(answer, 4);
                return _paymentInfo;
            }
            return new PaymentInfo();
        }


        #endregion

        

        #region Не рабочие
        public override string FPGetPayName(byte PayType)
        {
            if (WorkProtocol.EP06 != currentProtocol)
            {
                statusOperation = false;

                throw new System.ArgumentException("Это работает только на 6 протоколе", "Error");
            }
            byte[] forsending = new byte[] { 0x2D, 0x00 };
            switch (PayType)
            {
                case (byte)FPTypePays.Card:
                    forsending = new byte[] { 0x2D, 0x00 };
                    break;
                case (byte)FPTypePays.Credit:
                    forsending = new byte[] { 0x2D, 0x10 };
                    break;
                case (byte)FPTypePays.Check:
                    forsending = new byte[] { 0x2D, 0x20 };
                    break;
                case (byte)FPTypePays.Cash:
                    forsending = new byte[] { 0x2D, 0x30 };
                    break;
                    //case (byte)FPTypePays.Splata4:
                    //    forsending = new byte[] { 0x2D, 0x40 };
                    //    break;
                    //case (byte)FPTypePays.Splata5:
                    //    forsending = new byte[] { 0x2D, 0x50 };
                    //    break;
                    //case (byte)FPTypePays.Splata6:
                    //    forsending = new byte[] { 0x2D, 0x60 };
                    //    break;
                    //case (byte)FPTypePays.Splata7:
                    //    forsending = new byte[] { 0x2D, 0x70 };
                    //    break;
                    //case (byte)FPTypePays.Splata8:
                    //    forsending = new byte[] { 0x2D, 0x80 };
                    //    break;
                    //case (byte)FPTypePays.Splata9:
                    //    forsending = new byte[] { 0x2D, 0x90 };
                    //    break;
            }
            byte[] answer = GetMemmory(forsending, 16, 16);

            if (answer.Length != 16)
            {
                this.statusOperation = false;
                return "";
            }
            return byteHelper.EncodingBytes(answer);
        }


        #endregion

        #region глобальные установки
        public override void FPSetHeadLine(ushort Password, string StringInfo1, bool StringInfo1DoubleHeight, bool StringInfo1DoubleWidth
            , string StringInfo2, bool StringInfo2DoubleHeight, bool StringInfo2DoubleWidth
            , string StringInfo3, bool StringInfo3DoubleHeight, bool StringInfo3DoubleWidth
            , string TaxNumber, bool AddTaxInfo)
        {
            byte[] forsending = new byte[] { 22 };
            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(Password));

            byte length1;
            byte[] stringBytes1;
            if (StringInfo1DoubleHeight) stringBytes1 = byteHelper.CodingBytes(StringInfo1, 20, out length1);
            else stringBytes1 = byteHelper.CodingBytes(StringInfo1, 30, out length1);
            length1 = byteHelper.SetBit(length1, 6, StringInfo1DoubleHeight);
            length1 = byteHelper.SetBit(length1, 7, StringInfo1DoubleWidth);
            forsending = byteHelper.Combine(forsending, new byte[] { length1 });
            forsending = byteHelper.Combine(forsending, stringBytes1);

            byte length2;
            byte[] stringBytes2;
            if (StringInfo2DoubleHeight) stringBytes2 = byteHelper.CodingBytes(StringInfo2, 20, out length2);
            else stringBytes2 = byteHelper.CodingBytes(StringInfo2, 30, out length2);
            length2 = byteHelper.SetBit(length2, 6, StringInfo2DoubleHeight);
            length2 = byteHelper.SetBit(length2, 7, StringInfo2DoubleWidth);
            forsending = byteHelper.Combine(forsending, new byte[] { length2 });
            forsending = byteHelper.Combine(forsending, stringBytes2);

            byte length3;
            byte[] stringBytes3;
            if (StringInfo3DoubleHeight) stringBytes3 = byteHelper.CodingBytes(StringInfo3, 20, out length3);
            else stringBytes3 = byteHelper.CodingBytes(StringInfo3, 30, out length3);
            length3 = byteHelper.SetBit(length3, 6, StringInfo3DoubleHeight);
            length3 = byteHelper.SetBit(length3, 7, StringInfo3DoubleWidth);
            forsending = byteHelper.Combine(forsending, new byte[] { length3 });
            forsending = byteHelper.Combine(forsending, stringBytes3);



            byte legthTax;
            byte[] stringTax = byteHelper.CodingBytes(TaxNumber, 12, out legthTax);
            //legthTax = SetBit(legthTax, 7, AddTaxInfo); - не работает
            forsending = byteHelper.Combine(forsending, new byte[] { legthTax });
            forsending = byteHelper.Combine(forsending, stringTax);
            byte[] answer = ExchangeWithFP(forsending);
        }

        #endregion

        #region Налоговые ставки
        public override void FPSetTaxRate(ushort Password, Taxes tTaxes)
        {


            byte[] forsending = new byte[] { 25 };
            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(Password)); //пароль программирования
            forsending = byteHelper.Combine(forsending, new byte[] { (byte)tTaxes.MaxGroup });
            if (tTaxes.MaxGroup > 0)
            {
                forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(tTaxes.TaxA.TaxRate));
            }
            if (tTaxes.MaxGroup > 1)
            {
                forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(tTaxes.TaxB.TaxRate));
            }
            if (tTaxes.MaxGroup > 2)
            {
                forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(tTaxes.TaxC.TaxRate));
            }
            if (tTaxes.MaxGroup > 3)
            {
                forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(tTaxes.TaxD.TaxRate));
            }
            if (tTaxes.MaxGroup > 4)
            {
                forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(tTaxes.TaxE.TaxRate));
            }
            byte tBit = 0;
            //TODO - в ходе экспериментов это не работало
            //tBit = SetBit(tBit, 1, true);
            //if (tTaxes.quantityOfDecimalDigitsOfMoneySum == 0)
            //    tBit = SetBit(tBit, 0, true);
            //if (tTaxes.quantityOfDecimalDigitsOfMoneySum == 1)
            //    tBit = SetBit(tBit, 1, true);
            //if (tTaxes.quantityOfDecimalDigitsOfMoneySum == 2)
            //    tBit = SetBit(tBit, 2, true);
            //if (tTaxes.quantityOfDecimalDigitsOfMoneySum == 3)
            //    tBit = SetBit(tBit, 3, true);

            tBit = byteHelper.SetBit(tBit, 4, tTaxes.VAT);
            tBit = byteHelper.SetBit(tBit, 5, tTaxes.ToProgramChargeRates);
            forsending = byteHelper.Combine(forsending, new byte[] { tBit });
            if (tTaxes.ToProgramChargeRates)
            {

                if (tTaxes.MaxGroup > 0)
                {
                    byte[] forSend = BitConverter.GetBytes(tTaxes.TaxA.ChargeRates);
                    byte firstByte = forSend[1];
                    firstByte = byteHelper.SetBit(firstByte, 7, tTaxes.TaxA.VATAtCharge);
                    firstByte = byteHelper.SetBit(firstByte, 6, tTaxes.TaxA.ChargeAtVAT);
                    forSend[1] = firstByte;
                    forsending = byteHelper.Combine(forsending, forSend);
                }
                if (tTaxes.MaxGroup > 1)
                {
                    byte[] forSend = BitConverter.GetBytes(tTaxes.TaxB.ChargeRates);
                    byte firstByte = forSend[1];
                    firstByte = byteHelper.SetBit(firstByte, 7, tTaxes.TaxB.VATAtCharge);
                    firstByte = byteHelper.SetBit(firstByte, 6, tTaxes.TaxB.ChargeAtVAT);
                    forSend[1] = firstByte;
                    forsending = byteHelper.Combine(forsending, forSend);
                }
                if (tTaxes.MaxGroup > 2)
                {
                    byte[] forSend = BitConverter.GetBytes(tTaxes.TaxC.ChargeRates);
                    byte firstByte = forSend[1];
                    firstByte = byteHelper.SetBit(firstByte, 7, tTaxes.TaxC.VATAtCharge);
                    firstByte = byteHelper.SetBit(firstByte, 6, tTaxes.TaxC.ChargeAtVAT);
                    forSend[1] = firstByte;
                    forsending = byteHelper.Combine(forsending, forSend);
                }
                if (tTaxes.MaxGroup > 3)
                {
                    byte[] forSend = BitConverter.GetBytes(tTaxes.TaxD.ChargeRates);
                    byte firstByte = forSend[1];
                    firstByte = byteHelper.SetBit(firstByte, 7, tTaxes.TaxD.VATAtCharge);
                    firstByte = byteHelper.SetBit(firstByte, 6, tTaxes.TaxD.ChargeAtVAT);
                    forSend[1] = firstByte;
                    forsending = byteHelper.Combine(forsending, forSend);
                }
                if (tTaxes.MaxGroup > 4)
                {
                    byte[] forSend = BitConverter.GetBytes(tTaxes.TaxE.ChargeRates);
                    byte firstByte = forSend[1];
                    firstByte = byteHelper.SetBit(firstByte, 7, tTaxes.TaxE.VATAtCharge);
                    firstByte = byteHelper.SetBit(firstByte, 6, tTaxes.TaxE.ChargeAtVAT);
                    forSend[1] = firstByte;
                    forsending = byteHelper.Combine(forsending, forSend);
                }

                forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(tTaxes.ChargeRateOfGroupЕ));
            }

            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(tTaxes.TaxD.TaxRate));


            Console.WriteLine("{0}", byteHelper.PrintByteArrayX(forsending));
            byte[] answer = ExchangeWithFP(forsending);
        }

        public override void FPGetTaxRate()
        {
            byte[] forsending = new byte[] { 44 };
            byte[] answer = ExchangeWithFP(forsending);
            if ((statusOperation) && (answer.Length > 5))
            {
                int tst = 0;
                Taxes tTax = new Taxes();
                tTax.MaxGroup = (short)answer[tst];
                tst++;
                tTax.DateSet = byteHelper.returnDatefromByte(answer, tst);
                tst = tst + 3;
                if (tTax.MaxGroup > 0)
                {
                    tTax.TaxA.TaxGroup = (byte)FPTaxgroup.A;
                    tTax.TaxA.TaxNumber = 1;
                    tTax.TaxA.TaxRate = BitConverter.ToUInt16(answer, tst);
                    tst = tst + 2;
                }
                if (tTax.MaxGroup > 1)
                {
                    tTax.TaxB.TaxGroup = (byte)FPTaxgroup.B;
                    tTax.TaxB.TaxNumber = 2;
                    tTax.TaxB.TaxRate = BitConverter.ToUInt16(answer, tst);
                    tst = tst + 2;
                }
                if (tTax.MaxGroup > 2)
                {
                    tTax.TaxC.TaxGroup = (byte)FPTaxgroup.C;
                    tTax.TaxC.TaxNumber = 3;
                    tTax.TaxC.TaxRate = BitConverter.ToUInt16(answer, tst);
                    tst = tst + 2;
                }
                if (tTax.MaxGroup > 3)
                {
                    tTax.TaxD.TaxGroup = (byte)FPTaxgroup.D;
                    tTax.TaxD.TaxNumber = 4;
                    tTax.TaxD.TaxRate = BitConverter.ToUInt16(answer, tst);
                    tst = tst + 2;
                }
                if (tTax.MaxGroup > 4)
                {
                    tTax.TaxE.TaxGroup = (byte)FPTaxgroup.E;
                    tTax.TaxE.TaxNumber = 5;
                    tTax.TaxE.TaxRate = BitConverter.ToUInt16(answer, tst);
                    tst = tst + 2;
                }
                byte tByteStatus = answer[tst];
                tst++;
                tTax.quantityOfDecimalDigitsOfMoneySum = 0;
                if (byteHelper.GetBit(tByteStatus, 0))
                    tTax.quantityOfDecimalDigitsOfMoneySum = 1;
                if (byteHelper.GetBit(tByteStatus, 1))
                    tTax.quantityOfDecimalDigitsOfMoneySum = 2;
                if (byteHelper.GetBit(tByteStatus, 2))
                    tTax.quantityOfDecimalDigitsOfMoneySum = 3;


                tTax.VAT = byteHelper.GetBit(tByteStatus, 4);
                tTax.ToProgramChargeRates = byteHelper.GetBit(tByteStatus, 5);
                if (tTax.ToProgramChargeRates)
                {
                    if (tTax.MaxGroup > 0)
                    {
                        byte[] byteget = new byte[] { answer[tst], answer[tst + 1] };
                        tTax.TaxA.VATAtCharge = byteHelper.GetBit(byteget[1], 7);
                        tTax.TaxA.ChargeAtVAT = byteHelper.GetBit(byteget[1], 6);
                        byteget[1] = byteHelper.SetBit(byteget[1], 7, false);
                        byteget[1] = byteHelper.SetBit(byteget[1], 6, false);
                        tTax.TaxA.ChargeRates = BitConverter.ToUInt16(byteget, 0);
                        tst = tst + 2;
                    }

                    if (tTax.MaxGroup > 1)
                    {
                        byte[] byteget = new byte[] { answer[tst], answer[tst + 1] };
                        tTax.TaxB.VATAtCharge = byteHelper.GetBit(byteget[1], 7);
                        tTax.TaxB.ChargeAtVAT = byteHelper.GetBit(byteget[1], 6);
                        byteget[1] = byteHelper.SetBit(byteget[1], 7, false);
                        byteget[1] = byteHelper.SetBit(byteget[1], 6, false);
                        tTax.TaxB.ChargeRates = BitConverter.ToUInt16(byteget, 0);
                        tst = tst + 2;
                    }
                    if (tTax.MaxGroup > 2)
                    {
                        byte[] byteget = new byte[] { answer[tst], answer[tst + 1] };
                        tTax.TaxC.VATAtCharge = byteHelper.GetBit(byteget[1], 7);
                        tTax.TaxC.ChargeAtVAT = byteHelper.GetBit(byteget[1], 6);
                        byteget[1] = byteHelper.SetBit(byteget[1], 7, false);
                        byteget[1] = byteHelper.SetBit(byteget[1], 6, false);
                        tTax.TaxC.ChargeRates = BitConverter.ToUInt16(byteget, 0);
                        tst = tst + 2;
                    }
                    if (tTax.MaxGroup > 3)
                    {
                        byte[] byteget = new byte[] { answer[tst], answer[tst + 1] };
                        tTax.TaxD.VATAtCharge = byteHelper.GetBit(byteget[1], 7);
                        tTax.TaxD.ChargeAtVAT = byteHelper.GetBit(byteget[1], 6);
                        byteget[1] = byteHelper.SetBit(byteget[1], 7, false);
                        byteget[1] = byteHelper.SetBit(byteget[1], 6, false);
                        tTax.TaxD.ChargeRates = BitConverter.ToUInt16(byteget, 0);
                        tst = tst + 2;
                    }
                    if (tTax.MaxGroup > 4)
                    {
                        byte[] byteget = new byte[] { answer[tst], answer[tst + 1] };
                        tTax.TaxE.VATAtCharge = byteHelper.GetBit(byteget[1], 7);
                        tTax.TaxE.ChargeAtVAT = byteHelper.GetBit(byteget[1], 6);
                        byteget[1] = byteHelper.SetBit(byteget[1], 7, false);
                        byteget[1] = byteHelper.SetBit(byteget[1], 6, false);
                        tTax.TaxE.ChargeRates = BitConverter.ToUInt16(byteget, 0);
                        tst = tst + 2;
                    }

                    byte[] bytegetE = new byte[] { answer[tst], answer[tst + 1] };
                    //tTax.TaxE.VATAtCharge = GetBit(bytegetE[1], 7);
                    //tTax.TaxE.ChargeAtVAT = GetBit(bytegetE[1], 6);
                    bytegetE[1] = byteHelper.SetBit(bytegetE[1], 7, false);
                    bytegetE[1] = byteHelper.SetBit(bytegetE[1], 6, false);
                    tTax.ChargeRateOfGroupЕ = BitConverter.ToUInt16(bytegetE, 0);

                }

                this.currentTaxes = tTax;
            }
        }
        #endregion

        #region Отчеты
        public override void FPArtReport(ushort pass = 0, UInt32? CodeBeginning = null, UInt32? CodeFinishing = null)
        {
            byte[] forsending = new byte[] { 10 };
            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(pass));
            if (CodeBeginning != null)
            {
                forsending = byteHelper.Combine(forsending, byteHelper.ConvertTobyte(CodeBeginning));
                forsending = byteHelper.Combine(forsending, byteHelper.ConvertTobyte(CodeFinishing));
            }
            byte[] answer = ExchangeWithFP(forsending);
        }


        //public UInt32 FPDayClrReport(ushort pass = 0)
        /// <summary>
        /// Код: 13.DayClrReport   печать и регистрация дневного отчета по финансовым операциям с обнулением дневных регистров
        /// Печать Z-отчета.
        /// </summary>
        /// <param name="pass"></param>
        public override void FPDayClrReport(ushort pass = 0)
        {
            byte[] forsending = new byte[] { 13 };
            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(pass));
            byte[] answer = ExchangeWithFP(forsending);

            FPGetTaxRate(); // читаем ставки
            //TODO
            //По документации тут должно быть ответ с № КЛЕФ
            // return BitConverter.ToUInt32(answer, 0);
        }

        public override void FPPeriodicReport(ushort pass, DateTime FirstDay, DateTime LastDay)
        {
            byte[] forsending = new byte[9];
            byte[] passByte = BitConverter.GetBytes(pass);
            forsending[0] = 17;
            forsending[1] = passByte[0];
            forsending[2] = passByte[1];

            forsending[3] = Convert.ToByte(Convert.ToInt32(FirstDay.ToString("dd"), 16));
            forsending[4] = Convert.ToByte(Convert.ToInt32(FirstDay.ToString("MM"), 16));
            forsending[5] = Convert.ToByte(Convert.ToInt32(FirstDay.ToString("yy"), 16));
            forsending[6] = Convert.ToByte(Convert.ToInt32(LastDay.ToString("dd"), 16));
            forsending[7] = Convert.ToByte(Convert.ToInt32(LastDay.ToString("MM"), 16));
            forsending[8] = Convert.ToByte(Convert.ToInt32(LastDay.ToString("yy"), 16));
            byte[] answer = ExchangeWithFP(forsending);
        }

        public override void FPPeriodicReportShort(ushort pass, DateTime FirstDay, DateTime LastDay)
        {
            byte[] forsending = new byte[9];
            byte[] passByte = BitConverter.GetBytes(pass);
            forsending[0] = 26;
            forsending[1] = passByte[0];
            forsending[2] = passByte[1];

            forsending[3] = Convert.ToByte(Convert.ToInt32(FirstDay.ToString("dd"), 16));
            forsending[4] = Convert.ToByte(Convert.ToInt32(FirstDay.ToString("MM"), 16));
            forsending[5] = Convert.ToByte(Convert.ToInt32(FirstDay.ToString("yy"), 16));
            forsending[6] = Convert.ToByte(Convert.ToInt32(LastDay.ToString("dd"), 16));
            forsending[7] = Convert.ToByte(Convert.ToInt32(LastDay.ToString("MM"), 16));
            forsending[8] = Convert.ToByte(Convert.ToInt32(LastDay.ToString("yy"), 16));
            byte[] answer = ExchangeWithFP(forsending);
        }

        public override void FPPeriodicReport2(ushort pass, UInt16 FirstNumber, UInt16 LastNumber)
        {
            byte[] forsending = new byte[7];
            byte[] passByte = BitConverter.GetBytes(pass);
            forsending[0] = 31;
            forsending[1] = passByte[0];
            forsending[2] = passByte[1];
            byte[] passFirstNumber = BitConverter.GetBytes(FirstNumber);
            forsending[3] = passFirstNumber[0];
            forsending[4] = passFirstNumber[1];
            byte[] passLastNumber = BitConverter.GetBytes(LastNumber);
            forsending[5] = passFirstNumber[0];
            forsending[6] = passFirstNumber[1];

            byte[] answer = ExchangeWithFP(forsending);
        }
        #endregion

        
        #region customer display
        public override bool showTopString(string Info)
        {
            Encoding cp866 = Encoding.GetEncoding(866);
            string tempStr = Info.Substring(0, Math.Min(20, Info.Length));
            byte[] forsending = byteHelper.Combine(new byte[] { 0x1b, 0x00, (byte)tempStr.Length }, cp866.GetBytes(tempStr));
            var answer = ExchangeWithFP(forsending);
            return connFP.statusOperation;
        }

        public override bool showBottomString(string Info)
        {
            Encoding cp866 = Encoding.GetEncoding(866);
            string tempStr = Info.Substring(0, Math.Min(20, Info.Length));
            byte[] forsending = byteHelper.Combine(new byte[] { 0x1b, 0x01, (byte)tempStr.Length }, cp866.GetBytes(tempStr));
            var answer = ExchangeWithFP(forsending);
            return connFP.statusOperation;
        }
        #endregion

    }
}
