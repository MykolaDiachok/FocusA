//#define Debug
#define DebugErrorInfo

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using CentralLib;
using CentralLib.ConnectionFP;
using CentralLib.DefaultPortCom;
using System.Collections;
using System.Threading;

namespace CentralLib.Protocols
{
    /// <summary>
    /// протокол обмена с фискальным регистратором
    /// </summary>
    public class Protocols : IDisposable
    {
        /// <summary>
        /// Статус последней операцияя
        /// </summary>
        public bool statusOperation { get; private set; }
        /// <summary>
        /// Байт статуса
        /// </summary>
        public byte ByteStatus { get; private set; } // Возврат ФР статус
        /// <summary>
        /// Байт результат
        /// </summary>
        public byte ByteResult { get; private set; } // Возврат ФР результат
        /// <summary>
        /// Байт резерва
        /// </summary>
        public byte ByteReserv { get; private set; } // Возврат ФР результат
        public WorkProtocol currentProtocol { get; private set; }
        private ConnectionFP.ConnectionFP connFP = null;
        /// <summary>
        /// Заканчивать соединение при уничтожении
        /// </summary>
        private bool killConnFP = false;
        public string errorInfo { get; protected set; }
        private Status tStatus;
        public Taxes currentTaxes { get; private set; }
        private UInt32 Max3ArrayBytes = BitConverter.ToUInt32(new byte[] { 255, 255, 255, 0 }, 0);
        private UInt64 Max6ArrayBytes = BitConverter.ToUInt64(new byte[] { 255, 255, 255, 255, 255, 255, 0, 0 }, 0);

        public Status status
        {
            get
            {
                if ((lastByteCommand != 0))
                    getStatus();
                return this.tStatus;
            }
        }

        private byte? lastByteCommand = null;
        public bool useCRC16 { get; private set; }

        /// <summary>
        /// Основные комманды для первичной инициализации класса, такие как получить статус и получить данные по налогам
        /// </summary>
        private void initial()
        {
            getStatus();
            FPGetTaxRate();
        }


        /// <summary>
        /// Передаем класс для подключения. Рекомендую использовать сразу код порта
        /// </summary>
        /// <param name="connFP"></param>
        public Protocols(ConnectionFP.ConnectionFP connFP)
        {
            this.connFP = connFP;
            this.useCRC16 = true;
            initial();
        }

        /// <summary>
        /// Класс инициализации приложения
        /// </summary>
        /// <param name="serialPort"></param>
        public Protocols(int serialPort)
        {
            if (serialPort == 0) // for test
            {
                killConnFP = false;
                this.useCRC16 = true;
                //initial();
                return;
            }
            DefaultPortCom.DefaultPortCom initialPort = new DefaultPortCom.DefaultPortCom((byte)serialPort);
            this.connFP = new ConnectionFP.ConnectionFP(initialPort);
            try
            {
                connFP.Open();
            }
            catch (AggregateException e)
            {
                this.statusOperation = false;
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Caught {0}, exception: {1}", e.InnerExceptions.Count, string.Join(", ", e.InnerExceptions.Select(x => x.Message)));

#if Debug
                Console.WriteLine("Описание ошибки:{0}", this.errorInfo);
#endif
            }
            killConnFP = true;
            this.useCRC16 = true;
            initial();
        }

        public void Dispose()
        {
            if (killConnFP)
            {
                connFP.Close();
                ((IDisposable)connFP).Dispose();
            }
        }

        /// <summary>
        /// Основная функция обмена для протокола, сюда передаем массив байтов, на выходе массив байтов ответа ФР, при этом передаются только данные.
        /// Вся проверка, подготовка выподняется в  connFP.dataExchange
        /// </summary>
        /// <param name="inputByte"></param>
        /// <returns></returns>
        private byte[] ExchangeWithFP(byte[] inputByte)
        {
            byte[] answer;
            this.lastByteCommand = inputByte[0];
            answer = connFP.dataExchange(inputByte, useCRC16, false);
            if (!connFP.statusOperation) //repetition if error
            {
                Thread.Sleep(800);
#if Debug
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("ошибка первое ожидание");
                ///Console.ReadKey();
#endif
                answer = connFP.dataExchange(inputByte, useCRC16, true);
            }
            if (!connFP.statusOperation) //repetition if error
            {
                //TODO: большая проблема искать в чем причина
                Thread.Sleep(800);
                answer = connFP.dataExchange(inputByte, useCRC16, true);
#if Debug
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Вторая ошибка");
                Console.ReadKey();
#endif
            }
            this.ByteStatus = connFP.ByteStatus;
            this.ByteResult = connFP.ByteResult;
            this.ByteReserv = connFP.ByteReserv;
            this.errorInfo = connFP.errorInfo;

            this.statusOperation = connFP.statusOperation;
#if DebugErrorInfo
            Console.WriteLine("Send:{0}", PrintByteArrayX(inputByte));
            Console.WriteLine("Resive:{0}", PrintByteArrayX(answer));
            Console.WriteLine("statusOperation:{0}", statusOperation);
            Console.WriteLine("errorInfo:{0}", errorInfo);
            Console.WriteLine("ByteStatus:{0}", ByteStatus);
            Console.WriteLine("ByteResult:{0}", ByteResult);
            Console.WriteLine("ByteReserv:{0}", ByteReserv);
#endif
            return answer;
        }

        /// <summary>
        /// Код: 0. SendStatus 	 	прочитать состояние регистратора 
        /// </summary>
        private void getStatus()
        {
            byte[] forsending = new byte[] { 0 };
            byte[] answer = ExchangeWithFP(forsending);

            if ((connFP.statusOperation) && (answer.Length > 0))
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
                string fiscalNumber = EncodingBytes(answer, curCountByte, 10);
                curCountByte = curCountByte + 10;


                byte tlen1 = answer[curCountByte];
                tlen1 = SetBit(tlen1, 6, false);
                tlen1 = SetBit(tlen1, 7, false);
                int len1 = tlen1;

                string str1 = "";
                if (len1 > 0)
                {
                    curCountByte++;
                    str1 = EncodingBytes(answer, curCountByte, len1);

                }
                curCountByte = curCountByte + len1;

                byte tlen2 = answer[curCountByte];
                tlen2 = SetBit(tlen2, 6, false);
                tlen2 = SetBit(tlen2, 7, false);
                int len2 = tlen2;
                string str2 = "";
                if (len2 > 0)
                {
                    curCountByte++;
                    str2 = EncodingBytes(answer, curCountByte, len2);

                }
                curCountByte = curCountByte + len2;

                byte tlen3 = answer[curCountByte];
                tlen3 = SetBit(tlen3, 6, false);
                tlen3 = SetBit(tlen3, 7, false);
                int len3 = tlen3;
                string str3 = "";
                if (len3 > 0)
                {
                    curCountByte++;
                    str3 = EncodingBytes(answer, curCountByte, len3);

                }
                curCountByte = curCountByte + len3;

                byte tlenTax = answer[curCountByte];
                tlenTax = SetBit(tlenTax, 6, false);
                tlenTax = SetBit(tlenTax, 7, false);
                int lenTax = tlenTax;
                string strTax = "";
                if (lenTax > 0)
                {
                    curCountByte++;
                    strTax = EncodingBytes(answer, curCountByte, lenTax);

                }
                curCountByte = curCountByte + len3;


                //string ver = EncodingBytes(answer.Skip(answer.Length-6).Take(5).ToArray());
                byte[] verBytes = new byte[5];
                System.Buffer.BlockCopy(answer, answer.Length - 5, verBytes, 0, 5);
                string ver = EncodingBytes(verBytes);
                switch (ver)
                {
                    case "ЕП-11":
                        this.currentProtocol = WorkProtocol.EP11;
                        this.useCRC16 = true;
                        break;
                };

                this.tStatus = new Status(answer.Take(2).ToArray()
                    , EncodingBytes(answer.Skip(2).Take(19).ToArray())
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

        public PapStat papStat
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

        /// <summary>
        ///Код: 14.LineFeed продвижение бумаги на одну строку
        /// </summary>
        public void FPLineFeed()
        {
            byte[] forsending = new byte[] { 14 };
            byte[] answer = ExchangeWithFP(forsending);
        }

        /// <summary>
        /// Код: 32.       PrintVer печать налогового номера и версии программного обеспечения
        /// Налоговый номер и дата регистрации ЭККР печатаются только в фискальном режиме.
        /// </summary>
        public void FPPrintVer()
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
        public void FPOpenBox(byte impulse = 0) //обнуление чека
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
        public void FPCplOnline() // Код: 38. запрет/разрешение режима OnLine регистраций
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
        public void FPSetPassword(byte UserID, ushort OldPassword, ushort NewPassword)
        {
            byte[] forsending = new byte[] { 5 };//SetCod
            forsending = Combine(forsending, BitConverter.GetBytes(OldPassword));
            forsending = Combine(forsending, new byte[] { UserID });
            forsending = Combine(forsending, BitConverter.GetBytes(NewPassword));
            byte[] answer = ExchangeWithFP(forsending);
        }

        /// <summary>
        /// Код: 6.SetCashier               регистрация кассира (оператора)  в ЭККР
        /// После инициализации ЭККР значения паролей равны нулю (0). При длине имени 0 –  разрегистрация  
        /// кассира.Количество вводов пароля не более 10.
        /// </summary>
        /// <param name="CashierID">Номер</param>
        /// <param name="Name">Длина имени кассира (= n)0..15</param>
        /// <param name="Password">Пароль</param>
        public void FPRegisterCashier(byte CashierID, string Name, ushort Password = 0)
        {
            byte[] forsending = new byte[] { 6 };//SetCashier
            forsending = Combine(forsending, BitConverter.GetBytes(Password));
            forsending = Combine(forsending, new byte[] { CashierID });
            byte length;
            byte[] stringBytes = CodingBytes(Name, 15, out length);

            forsending = Combine(forsending, new byte[] { length });
            forsending = Combine(forsending, stringBytes);
            byte[] answer = ExchangeWithFP(forsending);
        }

        #endregion


        #region внос и вынос денег

        /// <summary>
        /// Код: 24. Give                       служебная  выдача  наличных из денежного ящика 
        /// </summary>
        /// <param name="Summa">сумма инкассации в коп.</param>
        /// <returns>номер пакета чека в КЛЕФ</returns>
        public UInt32 FPCashOut(UInt32 Summa)
        {
            byte[] forsending = new byte[] { 24 };
            forsending = Combine(forsending, BitConverter.GetBytes(Summa));
            byte[] answer = ExchangeWithFP(forsending);
            if (answer.Length == 4)
                return BitConverter.ToUInt32(answer, 0);
            return 0;
        }

        /// <summary>
        /// Код: 16.Avans                          служебное внесение денег в денежный ящик
        /// </summary>
        /// <param name="Summa">сумма аванса в коп.</param>
        /// <returns>номер пакета чека в КЛЕФ</returns>
        public UInt32 FPCashIn(UInt32 Summa)
        {
            byte[] forsending = new byte[] { 16 };
            forsending = Combine(forsending, BitConverter.GetBytes(Summa));
            byte[] answer = ExchangeWithFP(forsending);
            if (answer.Length == 4)
                return BitConverter.ToUInt32(answer, 0);
            return 0;
        }

        #endregion


        #region Чеки

        /// <summary>
        /// Код: 15. ResetOrder                обнуление чека
        /// </summary>
        public void FPResetOrder() //обнуление чека
        {
            byte[] forsending = new byte[] { 15 };
            byte[] answer = ExchangeWithFP(forsending);
        }

        /// <summary>
        /// Печать нулевого чека
        /// </summary>
        /// <returns></returns>
        public UInt32 FPPrintZeroReceipt()
        {
            byte[] forsending = new byte[] { 11 };//Comment            
            byte length;
            byte[] stringBytes = CodingBytes("Нульовий чек", 27, out length);
            length = SetBit(length, 7, false);
            forsending = Combine(forsending, new byte[] { length });
            forsending = Combine(forsending, stringBytes);
            byte[] answer = ExchangeWithFP(forsending);
            if (statusOperation)
            {
                forsending = new byte[] { 20, 0x03 };//Payment 
                forsending = Combine(forsending, BitConverter.GetBytes(0 ^ (1 << 31)));
                answer = ExchangeWithFP(forsending);
                if (answer.Length == 4)
                    return BitConverter.ToUInt32(answer, 0);
            }
            return 0;
        }

        /// <summary>
        /// Код: 11. Comment                  регистрация комментария в фискальном чеке
        /// Если  бит  7  длины  строки  равен  единице  (1)  при  первой  регистрации  в  чеке,  то  открывается  чек                                                      
        ///  выплат, иначе будет открыт чек продаж.В остальных случаях бит 7 не устанавливать!  Открыв
        /// чек комментарием(например, строкой   “НУЛЕВОЙ ЧЕК”)   и закрыв   его командой   20, можно
        /// напечатать нулевой чек.
        /// </summary>
        /// <param name="CommentLine">Строка комментария</param>
        /// <param name="OpenRefundReceipt">= 1 – открытие чека выплаты</param>
        public void FPCommentLine(string CommentLine, bool OpenRefundReceipt = false)
        {
            byte[] forsending = new byte[] { 11 };//Comment            
            byte length;
            byte[] stringBytes = CodingBytes(CommentLine, 27, out length);
            length = SetBit(length, 7, OpenRefundReceipt);
            forsending = Combine(forsending, new byte[] { length });
            forsending = Combine(forsending, stringBytes);
            byte[] answer = ExchangeWithFP(forsending);
        }


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
        public ReceiptInfo FPPayMoneyEx(UInt16 Amount, byte Amount_Status, bool IsOneQuant, Int32 Price, ushort NalogGroup, bool MemoryGoodName, string GoodName, UInt64 StrCode, bool PrintingOfBarCodesOfGoods = false)
        {
            byte[] forsending = new byte[] { 8 };

            forsending = Combine(forsending, ConvertUint32ToArrayByte3(Amount));
            Amount_Status = SetBit(Amount_Status, 6, IsOneQuant);
            Amount_Status = SetBit(Amount_Status, 7, PrintingOfBarCodesOfGoods);
            forsending = Combine(forsending, new byte[] { Amount_Status });
            Int32 _price = Price;
            //BitArray b_price = new BitArray(BitConverter.GetBytes(_price));
            if (Price < 0)
            {
                _price = -_price;
                _price = _price ^ (1 << 31);
            }
            forsending = Combine(forsending, BitConverter.GetBytes(_price));
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
            forsending = Combine(forsending, VAT);

            if (MemoryGoodName)
                forsending = Combine(forsending, new byte[] { 255 });
            else
            {
                forsending = Combine(forsending, CodingStringToBytesWithLength(GoodName, 75));
            }
            forsending = Combine(forsending, ConvertUint64ToArrayByte6(StrCode));
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
        public ReceiptInfo FPSaleEx(UInt16 Amount, byte Amount_Status, bool IsOneQuant, Int32 Price, ushort NalogGroup, bool MemoryGoodName, string GoodName, UInt64 StrCode, bool PrintingOfBarCodesOfGoods = false)
        {
            byte[] forsending = new byte[] { 18 };

            forsending = Combine(forsending, ConvertUint32ToArrayByte3(Amount));
            Amount_Status = SetBit(Amount_Status, 6, IsOneQuant);
            Amount_Status = SetBit(Amount_Status, 7, PrintingOfBarCodesOfGoods);
            forsending = Combine(forsending, new byte[] { Amount_Status });
            Int32 _price = Price;
            //BitArray b_price = new BitArray(BitConverter.GetBytes(_price));
            if (Price < 0)
            {
                _price = -_price;
                _price = _price ^ (1 << 31);
            }
            forsending = Combine(forsending, BitConverter.GetBytes(_price));
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
            forsending = Combine(forsending, VAT);

            if (MemoryGoodName)
                forsending = Combine(forsending, new byte[] { 255 });
            else
            {
                forsending = Combine(forsending, CodingStringToBytesWithLength(GoodName, 75));
            }
            forsending = Combine(forsending, ConvertUint64ToArrayByte6(StrCode));
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
        public PaymentInfo FPPayment(byte Payment_Status, UInt32 Payment, bool CheckClose, bool FiscStatus, string AuthorizationCode="")
        {
            byte[] forsending = new byte[] { 20 };
            Payment_Status = SetBit(Payment_Status, 6, !FiscStatus);
            forsending = Combine(forsending, new byte[] { Payment_Status });
            byte[] bytePayment = BitConverter.GetBytes(WriteBitUInt32(Payment, 31, CheckClose));
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
            forsending = Combine(forsending, bytePayment);
            forsending = Combine(forsending, new byte[] { 0 });
            if (AuthorizationCode.Length!=0)
                forsending = Combine(forsending, CodingStringToBytesWithLength(AuthorizationCode, 50));
            byte[] answer = ExchangeWithFP(forsending);
            if ((statusOperation) && (answer.Length  > 3))
            {
                PaymentInfo _paymentInfo = new PaymentInfo();
                UInt32 tinfo = BitConverter.ToUInt32(answer, 0);
                if (GetBit(answer[3],7))
                {
                    tinfo = ClearBitUInt32(tinfo, 31);
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

        #region GetMemmory
        private byte[] GetMemmory(byte[] AddressOfBlock, byte NumberOfPage, byte SizeOfBlock) //прочитать блок памяти регистратора
        {
            byte[] forsending = new byte[] { 28 };
            forsending = Combine(forsending, new byte[] { AddressOfBlock[1], AddressOfBlock[0] });
            forsending = Combine(forsending, new byte[] { NumberOfPage, SizeOfBlock });
            byte[] answer = ExchangeWithFP(forsending);
            return answer;
        }


        #endregion

        #region Не рабочие
        public string FPGetPayName(byte PayType)
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
            return EncodingBytes(answer);
        }


        #endregion

        #region глобальные установки
        public void FPSetHeadLine(ushort Password, string StringInfo1, bool StringInfo1DoubleHeight, bool StringInfo1DoubleWidth
            , string StringInfo2, bool StringInfo2DoubleHeight, bool StringInfo2DoubleWidth
            , string StringInfo3, bool StringInfo3DoubleHeight, bool StringInfo3DoubleWidth
            , string TaxNumber, bool AddTaxInfo)
        {
            byte[] forsending = new byte[] { 22 };
            forsending = Combine(forsending, BitConverter.GetBytes(Password));

            byte length1;
            byte[] stringBytes1;
            if (StringInfo1DoubleHeight) stringBytes1 = CodingBytes(StringInfo1, 20, out length1);
            else stringBytes1 = CodingBytes(StringInfo1, 30, out length1);
            length1 = SetBit(length1, 6, StringInfo1DoubleHeight);
            length1 = SetBit(length1, 7, StringInfo1DoubleWidth);
            forsending = Combine(forsending, new byte[] { length1 });
            forsending = Combine(forsending, stringBytes1);

            byte length2;
            byte[] stringBytes2;
            if (StringInfo2DoubleHeight) stringBytes2 = CodingBytes(StringInfo2, 20, out length2);
            else stringBytes2 = CodingBytes(StringInfo2, 30, out length2);
            length2 = SetBit(length2, 6, StringInfo2DoubleHeight);
            length2 = SetBit(length2, 7, StringInfo2DoubleWidth);
            forsending = Combine(forsending, new byte[] { length2 });
            forsending = Combine(forsending, stringBytes2);

            byte length3;
            byte[] stringBytes3;
            if (StringInfo3DoubleHeight) stringBytes3 = CodingBytes(StringInfo3, 20, out length3);
            else stringBytes3 = CodingBytes(StringInfo3, 30, out length3);
            length3 = SetBit(length3, 6, StringInfo3DoubleHeight);
            length3 = SetBit(length3, 7, StringInfo3DoubleWidth);
            forsending = Combine(forsending, new byte[] { length3 });
            forsending = Combine(forsending, stringBytes3);



            byte legthTax;
            byte[] stringTax = CodingBytes(TaxNumber, 12, out legthTax);
            //legthTax = SetBit(legthTax, 7, AddTaxInfo); - не работает
            forsending = Combine(forsending, new byte[] { legthTax });
            forsending = Combine(forsending, stringTax);
            byte[] answer = ExchangeWithFP(forsending);
        }

        #endregion

        #region Налоговые ставки
        public void FPSetTaxRate(ushort Password, Taxes tTaxes)
        {


            byte[] forsending = new byte[] { 25 };
            forsending = Combine(forsending, BitConverter.GetBytes(Password)); //пароль программирования
            forsending = Combine(forsending, new byte[] { (byte)tTaxes.MaxGroup });
            if (tTaxes.MaxGroup > 0)
            {
                forsending = Combine(forsending, BitConverter.GetBytes(tTaxes.TaxA.TaxRate));
            }
            if (tTaxes.MaxGroup > 1)
            {
                forsending = Combine(forsending, BitConverter.GetBytes(tTaxes.TaxB.TaxRate));
            }
            if (tTaxes.MaxGroup > 2)
            {
                forsending = Combine(forsending, BitConverter.GetBytes(tTaxes.TaxC.TaxRate));
            }
            if (tTaxes.MaxGroup > 3)
            {
                forsending = Combine(forsending, BitConverter.GetBytes(tTaxes.TaxD.TaxRate));
            }
            if (tTaxes.MaxGroup > 4)
            {
                forsending = Combine(forsending, BitConverter.GetBytes(tTaxes.TaxE.TaxRate));
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

            tBit = SetBit(tBit, 4, tTaxes.VAT);
            tBit = SetBit(tBit, 5, tTaxes.ToProgramChargeRates);
            forsending = Combine(forsending, new byte[] { tBit });
            if (tTaxes.ToProgramChargeRates)
            {

                if (tTaxes.MaxGroup > 0)
                {
                    byte[] forSend = BitConverter.GetBytes(tTaxes.TaxA.ChargeRates);
                    byte firstByte = forSend[1];
                    firstByte = SetBit(firstByte, 7, tTaxes.TaxA.VATAtCharge);
                    firstByte = SetBit(firstByte, 6, tTaxes.TaxA.ChargeAtVAT);
                    forSend[1] = firstByte;
                    forsending = Combine(forsending, forSend);
                }
                if (tTaxes.MaxGroup > 1)
                {
                    byte[] forSend = BitConverter.GetBytes(tTaxes.TaxB.ChargeRates);
                    byte firstByte = forSend[1];
                    firstByte = SetBit(firstByte, 7, tTaxes.TaxB.VATAtCharge);
                    firstByte = SetBit(firstByte, 6, tTaxes.TaxB.ChargeAtVAT);
                    forSend[1] = firstByte;
                    forsending = Combine(forsending, forSend);
                }
                if (tTaxes.MaxGroup > 2)
                {
                    byte[] forSend = BitConverter.GetBytes(tTaxes.TaxC.ChargeRates);
                    byte firstByte = forSend[1];
                    firstByte = SetBit(firstByte, 7, tTaxes.TaxC.VATAtCharge);
                    firstByte = SetBit(firstByte, 6, tTaxes.TaxC.ChargeAtVAT);
                    forSend[1] = firstByte;
                    forsending = Combine(forsending, forSend);
                }
                if (tTaxes.MaxGroup > 3)
                {
                    byte[] forSend = BitConverter.GetBytes(tTaxes.TaxD.ChargeRates);
                    byte firstByte = forSend[1];
                    firstByte = SetBit(firstByte, 7, tTaxes.TaxD.VATAtCharge);
                    firstByte = SetBit(firstByte, 6, tTaxes.TaxD.ChargeAtVAT);
                    forSend[1] = firstByte;
                    forsending = Combine(forsending, forSend);
                }
                if (tTaxes.MaxGroup > 4)
                {
                    byte[] forSend = BitConverter.GetBytes(tTaxes.TaxE.ChargeRates);
                    byte firstByte = forSend[1];
                    firstByte = SetBit(firstByte, 7, tTaxes.TaxE.VATAtCharge);
                    firstByte = SetBit(firstByte, 6, tTaxes.TaxE.ChargeAtVAT);
                    forSend[1] = firstByte;
                    forsending = Combine(forsending, forSend);
                }

                forsending = Combine(forsending, BitConverter.GetBytes(tTaxes.ChargeRateOfGroupЕ));
            }

            forsending = Combine(forsending, BitConverter.GetBytes(tTaxes.TaxD.TaxRate));


            Console.WriteLine("{0}", PrintByteArrayX(forsending));
            byte[] answer = ExchangeWithFP(forsending);
        }

        public void FPGetTaxRate()
        {
            byte[] forsending = new byte[] { 44 };
            byte[] answer = ExchangeWithFP(forsending);
            if ((statusOperation) && (answer.Length > 5))
            {
                int tst = 0;
                Taxes tTax = new Taxes();
                tTax.MaxGroup = (short)answer[tst];
                tst++;
                tTax.DateSet = returnDatefromByte(answer, tst);
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
                if (GetBit(tByteStatus, 0))
                    tTax.quantityOfDecimalDigitsOfMoneySum = 1;
                if (GetBit(tByteStatus, 1))
                    tTax.quantityOfDecimalDigitsOfMoneySum = 2;
                if (GetBit(tByteStatus, 2))
                    tTax.quantityOfDecimalDigitsOfMoneySum = 3;


                tTax.VAT = GetBit(tByteStatus, 4);
                tTax.ToProgramChargeRates = GetBit(tByteStatus, 5);
                if (tTax.ToProgramChargeRates)
                {
                    if (tTax.MaxGroup > 0)
                    {
                        byte[] byteget = new byte[] { answer[tst], answer[tst + 1] };
                        tTax.TaxA.VATAtCharge = GetBit(byteget[1], 7);
                        tTax.TaxA.ChargeAtVAT = GetBit(byteget[1], 6);
                        byteget[1] = SetBit(byteget[1], 7, false);
                        byteget[1] = SetBit(byteget[1], 6, false);
                        tTax.TaxA.ChargeRates = BitConverter.ToUInt16(byteget, 0);
                        tst = tst + 2;
                    }

                    if (tTax.MaxGroup > 1)
                    {
                        byte[] byteget = new byte[] { answer[tst], answer[tst + 1] };
                        tTax.TaxB.VATAtCharge = GetBit(byteget[1], 7);
                        tTax.TaxB.ChargeAtVAT = GetBit(byteget[1], 6);
                        byteget[1] = SetBit(byteget[1], 7, false);
                        byteget[1] = SetBit(byteget[1], 6, false);
                        tTax.TaxB.ChargeRates = BitConverter.ToUInt16(byteget, 0);
                        tst = tst + 2;
                    }
                    if (tTax.MaxGroup > 2)
                    {
                        byte[] byteget = new byte[] { answer[tst], answer[tst + 1] };
                        tTax.TaxC.VATAtCharge = GetBit(byteget[1], 7);
                        tTax.TaxC.ChargeAtVAT = GetBit(byteget[1], 6);
                        byteget[1] = SetBit(byteget[1], 7, false);
                        byteget[1] = SetBit(byteget[1], 6, false);
                        tTax.TaxC.ChargeRates = BitConverter.ToUInt16(byteget, 0);
                        tst = tst + 2;
                    }
                    if (tTax.MaxGroup > 3)
                    {
                        byte[] byteget = new byte[] { answer[tst], answer[tst + 1] };
                        tTax.TaxD.VATAtCharge = GetBit(byteget[1], 7);
                        tTax.TaxD.ChargeAtVAT = GetBit(byteget[1], 6);
                        byteget[1] = SetBit(byteget[1], 7, false);
                        byteget[1] = SetBit(byteget[1], 6, false);
                        tTax.TaxD.ChargeRates = BitConverter.ToUInt16(byteget, 0);
                        tst = tst + 2;
                    }
                    if (tTax.MaxGroup > 4)
                    {
                        byte[] byteget = new byte[] { answer[tst], answer[tst + 1] };
                        tTax.TaxE.VATAtCharge = GetBit(byteget[1], 7);
                        tTax.TaxE.ChargeAtVAT = GetBit(byteget[1], 6);
                        byteget[1] = SetBit(byteget[1], 7, false);
                        byteget[1] = SetBit(byteget[1], 6, false);
                        tTax.TaxE.ChargeRates = BitConverter.ToUInt16(byteget, 0);
                        tst = tst + 2;
                    }

                    byte[] bytegetE = new byte[] { answer[tst], answer[tst + 1] };
                    //tTax.TaxE.VATAtCharge = GetBit(bytegetE[1], 7);
                    //tTax.TaxE.ChargeAtVAT = GetBit(bytegetE[1], 6);
                    bytegetE[1] = SetBit(bytegetE[1], 7, false);
                    bytegetE[1] = SetBit(bytegetE[1], 6, false);
                    tTax.ChargeRateOfGroupЕ = BitConverter.ToUInt16(bytegetE, 0);

                }

                currentTaxes = tTax;
            }
        }
        #endregion

        #region Отчеты
        public void FPArtReport(ushort pass = 0, UInt32? CodeBeginning = null, UInt32? CodeFinishing = null)
        {
            byte[] forsending = new byte[] { 10 };
            forsending = Combine(forsending, BitConverter.GetBytes(pass));
            if (CodeBeginning != null)
            {
                forsending = Combine(forsending, ConvertTobyte(CodeBeginning));
                forsending = Combine(forsending, ConvertTobyte(CodeFinishing));
            }
            byte[] answer = ExchangeWithFP(forsending);
        }

        public void FPDayReport(ushort pass = 0)
        {
            byte[] forsending = new byte[3];
            byte[] passByte = BitConverter.GetBytes(pass);
            forsending[0] = 9;
            forsending[1] = passByte[0];
            forsending[2] = passByte[1];
            //forsending = Combine(forsending, BitConverter.GetBytes(pass));
            byte[] answer = ExchangeWithFP(forsending);
        }

        //public UInt32 FPDayClrReport(ushort pass = 0)
        public void FPDayClrReport(ushort pass = 0)
        {
            byte[] forsending = new byte[] { 13 };
            forsending = Combine(forsending, BitConverter.GetBytes(pass));
            byte[] answer = ExchangeWithFP(forsending);
            //TODO
            //По документации тут должно быть ответ с № КЛЕФ
            // return BitConverter.ToUInt32(answer, 0);
        }

        public void FPPeriodicReport(ushort pass, DateTime FirstDay, DateTime LastDay)
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

        public void FPPeriodicReportShort(ushort pass, DateTime FirstDay, DateTime LastDay)
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

        public void FPPeriodicReport2(ushort pass, UInt16 FirstNumber, UInt16 LastNumber)
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

        #region DateTime
        public DateTime fpDateTime
        {
            get
            {

                byte[] answer = ExchangeWithFP(new byte[] { 1 });

                if (connFP.statusOperation)
                {
                    string hexday = answer[0].ToString("X");
                    int _day = Math.Min(Math.Max((int)Convert.ToInt16(hexday), 1), 31);

                    string hexmonth = answer[1].ToString("X");
                    int _month = Math.Min(Math.Max((int)Convert.ToInt16(hexmonth), 1), 12);

                    string hexyear = answer[2].ToString("X");
                    int _year = Convert.ToInt16(hexyear);


                    byte[] answerTime = ExchangeWithFP(new byte[] { 3 });
                    if (connFP.statusOperation)
                    {

                        string hexhour = answerTime[0].ToString("X");
                        int _hour = Math.Min(Math.Max((int)Convert.ToInt16(hexhour), 0), 23);

                        string hexminute = answerTime[1].ToString("X");
                        int _minute = Math.Min(Math.Max((int)Convert.ToInt16(hexminute), 0), 59);

                        string hexsecond = answerTime[2].ToString("X");
                        int _second = Math.Min(Math.Max((int)Convert.ToInt16(hexsecond), 0), 59);

                        return new DateTime(2000 + _year, _month, _day, _hour, _minute, _second);
                    }
                }
                return new DateTime();
            }
            set
            {
                byte dd = Convert.ToByte(Convert.ToInt32(value.ToString("dd"), 16));
                byte MM = Convert.ToByte(Convert.ToInt32(value.ToString("MM"), 16));
                byte yy = Convert.ToByte(Convert.ToInt32(value.ToString("yy"), 16));
                byte[] answer = ExchangeWithFP(new byte[] { 2, dd, MM, yy });
                if (connFP.statusOperation)
                {
                    byte hh = Convert.ToByte(Convert.ToInt32(value.ToString("HH"), 16));
                    byte mm = Convert.ToByte(Convert.ToInt32(value.ToString("mm"), 16));
                    byte ss = Convert.ToByte(Convert.ToInt32(value.ToString("ss"), 16));
                    byte[] answerTime = ExchangeWithFP(new byte[] { 4, hh, mm, ss });
                }
            }
        }
        #endregion


        #region customer display
        public bool showTopString(string Info)
        {
            Encoding cp866 = Encoding.GetEncoding(866);
            string tempStr = Info.Substring(0, Math.Min(20, Info.Length));
            byte[] forsending = Combine(new byte[] { 0x1b, 0x00, (byte)tempStr.Length }, cp866.GetBytes(tempStr));
            var answer = ExchangeWithFP(forsending);
            return connFP.statusOperation;
        }

        public bool showBottomString(string Info)
        {
            Encoding cp866 = Encoding.GetEncoding(866);
            string tempStr = Info.Substring(0, Math.Min(20, Info.Length));
            byte[] forsending = Combine(new byte[] { 0x1b, 0x01, (byte)tempStr.Length }, cp866.GetBytes(tempStr));
            var answer = ExchangeWithFP(forsending);
            return connFP.statusOperation;
        }
        #endregion


        #region helper
        #region byte

        /// <summary>
        /// из массива байт получаем дату, используется 3 байта подряд
        /// </summary>
        /// <param name="inputByte">Массив бай</param>
        /// <param name="index">начальный идекс для 1 байта</param>
        /// <returns></returns>
        private DateTime returnDatefromByte(byte[] inputByte, int index = 0)
        {
            string hexday = inputByte[index].ToString("X");
            int _day = Math.Min(Math.Max((int)Convert.ToInt16(hexday), 1), 31);
            index++;
            string hexmonth = inputByte[index].ToString("X");
            int _month = Math.Min(Math.Max((int)Convert.ToInt16(hexmonth), 1), 12);
            index++;
            string hexyear = inputByte[index].ToString("X");
            int _year = Convert.ToInt16(hexyear);

            return new DateTime(2000 + _year, _month, _day, 0, 0, 0);
        }

        private byte[] ConvertUint32ToArrayByte3(UInt32 inputValue)
        {
            if (inputValue > Max3ArrayBytes)
            {
                throw new System.ArgumentOutOfRangeException("input value", "Превышение максимального значения");
            }
            byte[] tByte = BitConverter.GetBytes(inputValue);
            return new byte[] { tByte[0], tByte[1], tByte[2] };
        }

        private byte[] ConvertUint64ToArrayByte6(UInt64 inputValue)
        {
            if (inputValue > Max6ArrayBytes)
            {
                throw new System.ArgumentOutOfRangeException("input value", "Превышение максимального значения");
            }
            byte[] tByte = BitConverter.GetBytes(inputValue);
            return new byte[] { tByte[0], tByte[1], tByte[2], tByte[3], tByte[4], tByte[5] };
        }

        /// <summary>
        /// Для конвертации uint32 в массив из 6 байт
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="needCountArray"></param>
        /// <returns></returns>
        private byte[] ConvertTobyte(UInt32? inputValue, int needCountArray = 6)
        {
            UInt32 tValue = inputValue.GetValueOrDefault();


            byte[] forreturn = BitConverter.GetBytes(tValue);
            if (forreturn.Length != needCountArray)
            {
                byte[] addzerobyte = new Byte[needCountArray - forreturn.Length];
                forreturn = Combine(forreturn, addzerobyte);
            }
            return forreturn;
        }

        /// <summary>
        /// Строку кодируем в байты
        /// </summary>
        /// <param name="InputString">Входящая строка</param>
        /// <param name="MaxVal">Максимальная длина строка</param>
        /// <param name="length">Возвращаем количество байт после кодировки</param>
        /// <returns></returns>
        private byte[] CodingBytes(string InputString, UInt16 MaxVal, out byte length)
        {
            Encoding cp866 = Encoding.GetEncoding(866);
            string tempStr = InputString.Substring(0, Math.Min(MaxVal, InputString.Length));
            length = (byte)tempStr.Length;
            return cp866.GetBytes(tempStr);
        }

        /// <summary>
        /// Из строки формируем массив байт
        /// </summary>
        /// <param name="InputString">Строка для преобразования</param>
        /// <param name="MaxVal">Макс длина строки</param>
        /// <returns>Возврат массив байт из строки + вначале байт с длиной строки</returns>
        private byte[] CodingStringToBytesWithLength(string InputString, UInt16 MaxVal)
        {
            Encoding cp866 = Encoding.GetEncoding(866);
            string tempStr = InputString.Substring(0, Math.Min(MaxVal, InputString.Length));
            //length = (byte)tempStr.Length;

            return Combine(new byte[] { (byte)tempStr.Length }, cp866.GetBytes(tempStr));
        }

        /// <summary>
        /// Раскодируем массив байт и возвращаем строку
        /// </summary>
        /// <param name="inputBytes"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private string EncodingBytes(byte[] inputBytes, int index = 0, int length = 0)
        {
            if (length == 0)
                length = inputBytes.Length;
            Encoding cp866 = Encoding.GetEncoding(866);
            return cp866.GetString(inputBytes, index, length);
        }


        /// <summary>
        /// Для объединение массивов байт
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private byte[] Combine(byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length + b.Length];
            System.Buffer.BlockCopy(a, 0, c, 0, a.Length);
            System.Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
            return c;
        }

        /// <summary>
        /// поиск в массиве байт первого вхождения из массива байт
        /// </summary>
        /// <param name="searchIn"></param>
        /// <param name="searchBytes"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        private int ByteSearch(byte[] searchIn, byte[] searchBytes, int start = 0)
        {
            int found = -1;
            bool matched = false;
            //only look at this if we have a populated search array and search bytes with a sensible start 
            if (searchIn.Length > 0 && searchBytes.Length > 0 && start <= (searchIn.Length - searchBytes.Length) && searchIn.Length >= searchBytes.Length)
            {
                //iterate through the array to be searched 
                for (int i = start; i <= searchIn.Length - searchBytes.Length; i++)
                {
                    //if the start bytes match we will start comparing all other bytes 
                    if (searchIn[i] == searchBytes[0])
                    {
                        if (searchIn.Length > 1)
                        {
                            //multiple bytes to be searched we have to compare byte by byte 
                            matched = true;
                            for (int y = 1; y <= searchBytes.Length - 1; y++)
                            {
                                if (searchIn[i + y] != searchBytes[y])
                                {
                                    matched = false;
                                    break;
                                }
                            }
                            //everything matched up 
                            if (matched)
                            {
                                found = i;
                                break;
                            }

                        }
                        else
                        {
                            //search byte is only one bit nothing else to do 
                            found = i;
                            break; //stop the loop 
                        }

                    }
                }

            }
            return found;
        }

        /// <summary>
        /// Возврат массива байт без дубликатов DLE {DLE, DLE}
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private byte[] returnWithOutDublicateDLE(byte[] source)
        {
            return returnWithOutDublicate(source, new byte[] { (byte)WorkByte.DLE, (byte)WorkByte.DLE });
        }

        /// <summary>
        /// Возврат массива байт без дубликатов
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        private byte[] returnWithOutDublicate(byte[] source, byte[] pattern)
        {

            List<byte> tReturn = new List<byte>();
            int sLenght = source.Length;
            for (int i = 0; i < sLenght; i++)
            {
                if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    tReturn.Add(source[i]);
                    i++;
                }
                else
                {
                    tReturn.Add(source[i]);
                }
            }
            return (byte[])tReturn.ToArray();
        }

        /// <summary>
        /// Поиск в массиве байт, исходя из массива байт
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        private int? PatternAt(byte[] source, byte[] pattern)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    return i;
                }
            }
            return null;
        }

        /// <summary>
        /// Еще один метод вывода массива байтов в строку, не оптимальный
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private string PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("");
            foreach (var b in bytes)
            {
                sb.Append(b + " ");
            }
            sb.Append("");
            return sb.ToString();
        }

        /// <summary>
        /// Ввыести массив байтов в строку, сделано для отладки
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private string PrintByteArrayX(byte[] bytes)
        {
            if (bytes.Length > 0)
                return BitConverter.ToString(bytes).Replace("-", " ");
            else
                return "";
        }
        #endregion

        #region bit

        private byte BitArrayToByte(BitArray ba)
        {
            byte result = 0;
            for (byte index = 0, m = 1; index < 8; index++, m *= 2)
                result += ba.Get(index) ? m : (byte)0;
            return result;
        }

        private byte[] ToByteArray(BitArray bits)
        {
            int numBytes = bits.Count / 8;
            if (bits.Count % 8 != 0) numBytes++;

            byte[] bytes = new byte[numBytes];
            int byteIndex = 0, bitIndex = 0;

            for (int i = 0; i < bits.Count; i++)
            {
                if (bits[i])
                    bytes[byteIndex] |= (byte)(1 << (7 - bitIndex));

                bitIndex++;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }

            return bytes;
        }

        private bool GetBit(byte val, int num)
        {
            if ((num > 7) || (num < 0))//Проверка входных данных
            {
                throw new ArgumentException();
            }
            return ((val >> num) & 1) > 0;//собственно все вычисления
        }

        private byte SetBit(byte val, int num, bool bit)
        {
            if ((num > 7) || (num < 0))//Проверка входных данных
            {
                throw new ArgumentException();
            }
            byte tmpval = 1;
            tmpval = (byte)(tmpval << num);//устанавливаем необходимый бит в единицу
            val = (byte)(val & (~tmpval));//сбрасываем в 0 необходимый бит

            if (bit)// если бит требуется установить в 1
            {
                val = (byte)(val | (tmpval));//то устанавливаем необходимый бит в 1
            }
            return val;
        }

        private UInt32 SetBitUInt32(UInt32 Value, byte bit)
        {
            if (bit >= 32)
            {
                throw new ArgumentException("bit must be between 0 and 31");
            }

            Value |= (UInt32)(1U << bit);
            return Value;
        }

        private UInt32 ClearBitUInt32(UInt32 Value, byte bit)
        {
            if (bit >= 32)
            {
                throw new ArgumentException("bit must be between 0 and 31");
            }

            Value &= ~(UInt32)(1U << bit);
            return Value;
        }

        private UInt32 WriteBitUInt32(UInt32 Value, byte bit, bool state)
        {
            if (bit >= 32)
            {
                throw new ArgumentException("bit must be between 0 and 31");
            }

            if (state)
            {
                Value |= (UInt32)(1U << bit);
            }
            else {
                Value &= ~(UInt32)(1U << bit);
            }

            return Value;
        }

        private UInt32 ToggleBitUInt32(UInt32 Value, byte bit)
        {
            if (bit >= 32)
            {
                throw new ArgumentException("bit must be between 0 and 31");
            }

            if ((Value & (1 << bit)) == (1 << bit))
            {
                Value &= ~(UInt32)(1U << bit);
            }
            else {
                Value |= (UInt32)(1U << bit);
            }

            return Value;
        }

        private bool ReadBitUInt32(UInt32 Value, byte bit)
        {
            if (bit >= 32)
            {
                throw new ArgumentException("bit must be between 0 and 31");
            }

            return ((Value & (1 << bit)) == (1 << bit));
        }


        #endregion

        #endregion

    }
}
