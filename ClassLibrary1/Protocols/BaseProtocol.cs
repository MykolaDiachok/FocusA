using CentralLib.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CentralLib.Connections;

namespace CentralLib.Protocols
{
    public class BaseProtocol : IDisposable, IProtocols
    {
        public virtual UInt16 MaxStringLenght {
            get; set; 
        }

        private DefaultPortCom defaultPortCom;

        public BaseProtocol(DefaultPortCom dComPort)
        {
            this.defaultPortCom = dComPort;
            defaultInitial(dComPort);
        }

        public BaseProtocol(int port)
        {
            if (port == 0) // for test            
                return;

            DefaultPortCom initialPort = new DefaultPortCom((byte)port);
            this.defaultPortCom = initialPort;
            defaultInitial(initialPort);
        }

        private void defaultInitial(DefaultPortCom initialPort)
        {
            //base.currentProtocol = WorkProtocol.EP11;
            this.connFP = new CentralLib.Connections.ConnectFactory(initialPort,true,true);

            try
            {
                if (!connFP.IsOpen)
                    connFP.Open();
            }
            catch (AggregateException e)
            {
                this.statusOperation = false;
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Caught {0}, exception: {1}", e.InnerExceptions.Count, string.Join(", ", e.InnerExceptions.Select(x => x.Message)));
                throw new System.IO.IOException(sb.ToString());

#if Debug
                Console.WriteLine("Описание ошибки:{0}", this.errorInfo);
#endif
            }
            //this.useCRC16 = true;
            //initial();
        }


        public void Dispose()
        {
            if (connFP.IsOpen)
                connFP.Close();
            ((IDisposable)connFP).Dispose();
        }

        /// <summary>
        /// Статус последней операции true - завершено, false - сбой
        /// </summary>
        public bool statusOperation;

        /// <summary>
        /// Байт статуса
        /// </summary>
        public byte ByteStatus; // Возврат ФР статус

        public strByteStatus structStatus
        {
            get
            {
                return new strByteStatus(ByteStatus);
            }
        }

        /// <summary>
        /// Байт результат
        /// </summary>
        public byte ByteResult; // Возврат ФР результат

        public strByteResult structResult
        {
            get
            {
                return new strByteResult(ByteResult);
            }
        }

        /// <summary>
        /// Байт резерва
        /// </summary>
        public byte ByteReserv; // Возврат ФР результат

        public strByteReserv structReserv
        {
            get
            {
                return new strByteReserv(ByteReserv);
            }
        }

        public virtual Taxes currentTaxes
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual DayReport dayReport
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #region DateTime
        public virtual DateTime fpDateTime
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

        public virtual PapStat papStat { get; set; }


        private Status tStatus;

        public virtual Status status
        {
            get
            {
                if ((lastByteCommand != 0))
                    getStatus();
                return this.tStatus;
            }
        }

        /// <summary>
        /// Код: 0. SendStatus 	 	прочитать состояние регистратора 
        /// </summary>
        public virtual void getStatus()
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
                string fiscalNumber = new ByteHelper().EncodingBytes(answer, curCountByte, 10);
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


        public WorkProtocol currentProtocol;
        public CentralLib.Connections.ConnectFactory connFP = null;

        public string errorInfo;

        public byte? lastByteCommand = null;
        public bool useCRC16;

        public ByteHelper byteHelper = new ByteHelper();

        /// <summary>
        /// Основная функция обмена для протокола, сюда передаем массив байтов, на выходе массив байтов ответа ФР, при этом передаются только данные.
        /// Вся проверка, подготовка выподняется в  connFP.dataExchange
        /// </summary>
        /// <param name="inputByte"></param>
        /// <returns></returns>
        /// 
        public byte[] ExchangeWithFP(byte[] inputByte)
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

        #region GetMemmory
        public byte[] GetMemmory(byte[] AddressOfBlock, byte NumberOfPage, byte SizeOfBlock) //прочитать блок памяти регистратора
        {
            byte[] forsending = new byte[] { 28 };
            forsending = byteHelper.Combine(forsending, new byte[] { AddressOfBlock[1], AddressOfBlock[0] });
            forsending = byteHelper.Combine(forsending, new byte[] { NumberOfPage, SizeOfBlock });
            byte[] answer = ExchangeWithFP(forsending);
            return answer;
        }


        #endregion


        private string getstringProtocol()
        {
            byte[] forsending = new byte[] { 28, 00, 30 };
            byte[] answer = ExchangeWithFP(forsending);
            forsending = new byte[] { 0 };
            answer = ExchangeWithFP(forsending);
            string tCurProtocol="";
            if ((connFP.statusOperation) && (answer.Length > 21))
            {
                tCurProtocol = byteHelper.EncodingBytes(answer, answer.Length - 5, 5);
            }
            return tCurProtocol;
        }

        public BaseProtocol getCurrentProtocol()
        {
            string tPr = getstringProtocol();
            


            if (connFP.IsOpen)
            {
                connFP.Close();
            }
            if (tPr == "ЕП-11")
            {
                this.useCRC16 = true;
                this.currentProtocol = WorkProtocol.EP11;
                this.connFP = new CentralLib.Connections.ConnectFP_EP11(this.defaultPortCom);
                return new Protocol_EP11(this.defaultPortCom);
            }
            else if ((tPr.Length > 2)&&((tPr.Substring(0, 2) == "ЕП")))
            //((tPr == "ЕП-06")||((tPr.Length>2) &&((tPr.Substring(1,2)== "ЕП")||(tPr.Substring(1, 2) == "ОП")))) //Если не 11 и есть инфо по ЕП то считаем что  это 6 протокол
            {
                this.useCRC16 = false;
                this.currentProtocol = WorkProtocol.EP06;
                this.connFP = new CentralLib.Connections.ConnectFP_EP06(this.defaultPortCom);
                return new Protocol_EP06(this.defaultPortCom);
            }
            else if (tPr == "ОП-02")
            {
                this.useCRC16 = false;
                this.currentProtocol = WorkProtocol.OP02;
                this.connFP = new CentralLib.Connections.ConnectFP_EP06(this.defaultPortCom);
                return new Protocol_OP02(this.defaultPortCom);
            }

                throw new ApplicationException("Протокол не определен, работа программы не возможна");
            return null;
        }

        public virtual void FPArtReport(ushort pass = 0, uint? CodeBeginning = default(uint?), uint? CodeFinishing = default(uint?))
        {
            throw new NotImplementedException();
        }

        #region внос и вынос денег

        /// <summary>
        /// Код: 24. Give                       служебная  выдача  наличных из денежного ящика 
        /// </summary>
        /// <param name="Summa">сумма инкассации в коп.</param>
        /// <returns>номер пакета чека в КЛЕФ</returns>
        public virtual UInt32 FPCashOut(UInt32 Summa)
        {
            byte[] forsending = new byte[] { 24 };
            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(Summa));
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
        public virtual UInt32 FPCashIn(UInt32 Summa)
        {
            byte[] forsending = new byte[] { 16 };
            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(Summa));
            byte[] answer = ExchangeWithFP(forsending);
            if (answer.Length == 4)
                return BitConverter.ToUInt32(answer, 0);
            return 0;
        }

        #endregion

        /// <summary>
        /// Код: 11. Comment                  регистрация комментария в фискальном чеке
        /// Если  бит  7  длины  строки  равен  единице  (1)  при  первой  регистрации  в  чеке,  то  открывается  чек                                                      
        ///  выплат, иначе будет открыт чек продаж.В остальных случаях бит 7 не устанавливать!  Открыв
        /// чек комментарием(например, строкой   “НУЛЕВОЙ ЧЕК”)   и закрыв   его командой   20, можно
        /// напечатать нулевой чек.
        /// </summary>
        /// <param name="CommentLine">Строка комментария</param>
        /// <param name="OpenRefundReceipt">= 1 – открытие чека выплаты</param>
        public virtual void FPCommentLine(string CommentLine, bool OpenRefundReceipt = false)
        {
            byte[] forsending = new byte[] { 11 };//Comment            
            byte length;
            byte[] stringBytes = byteHelper.CodingBytes(CommentLine, 27, out length);
            length = byteHelper.SetBit(length, 7, OpenRefundReceipt);
            forsending = byteHelper.Combine(forsending, new byte[] { length });
            forsending = byteHelper.Combine(forsending, stringBytes);
            byte[] answer = ExchangeWithFP(forsending);
        }

        public virtual void FPCplOnline()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Код: 13.DayClrReport   печать и регистрация дневного отчета по финансовым операциям с обнулением дневных регистров
        /// Печать Z-отчета.
        /// </summary>
        /// <param name="pass"></param>
        public virtual void FPDayClrReport(ushort pass = 0)
        {
            byte[] forsending = new byte[] { 13 };
            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(pass));
            byte[] answer = ExchangeWithFP(forsending);            
        }

        /// <summary>
        /// Код: 9. DayReport                 печать дневного отчета по финансовым операциям 
        /// Печать X-отчета
        /// </summary>
        /// <param name="pass">пароль отчетов</param>
        public virtual void FPDayReport(ushort pass = 0)
        {
            byte[] forsending = new byte[3];
            byte[] passByte = BitConverter.GetBytes(pass);
            forsending[0] = 9;
            forsending[1] = passByte[0];
            forsending[2] = passByte[1];
            //forsending = Combine(forsending, BitConverter.GetBytes(pass));
            byte[] answer = ExchangeWithFP(forsending);
        }

        public virtual string FPGetPayName(byte PayType)
        {
            throw new NotImplementedException();
        }

        public virtual void FPGetTaxRate()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///Код: 14.LineFeed продвижение бумаги на одну строку
        /// </summary>
        public virtual void FPLineFeed()
        {
            byte[] forsending = new byte[] { 14 };
            byte[] answer = ExchangeWithFP(forsending);
        }

        public virtual void FPOpenBox(byte impulse = 0)
        {
            throw new NotImplementedException();
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
        /// <param name="AuthorizationCode">код авторизации при оплате картой через платёжный терминал - !!! работает только в 11 протоколе</param>
        /// <returns>остаток или сдача (бит 31 = 1 – сдача), номер пакета чека в КЛЕФ</returns>
        public virtual PaymentInfo FPPayment(byte Payment_Status, uint Payment, bool CheckClose, bool FiscStatus, string AuthorizationCode = "")
        {
            byte[] forsending = new byte[] { 20 };
            Payment_Status = byteHelper.SetBit(Payment_Status, 6, !FiscStatus);
            forsending = byteHelper.Combine(forsending, new byte[] { Payment_Status });
            byte[] bytePayment = BitConverter.GetBytes(byteHelper.WriteBitUInt32(Payment, 31, CheckClose));            
            forsending = byteHelper.Combine(forsending, bytePayment);
            forsending = byteHelper.Combine(forsending, new byte[] { 0 });
            //if (AuthorizationCode.Length != 0)
            //    forsending = byteHelper.Combine(forsending, byteHelper.CodingStringToBytesWithLength(AuthorizationCode, 50));
            byte[] answer = ExchangeWithFP(forsending);
            if ((statusOperation) && (answer.Length > 3))
            {
                PaymentInfo _paymentInfo = new PaymentInfo();
                UInt32 tinfo = BitConverter.ToUInt32(answer, 0);
                if (byteHelper.GetBit(answer[3], 7))
                {
                    tinfo = byteHelper.ClearBitUInt32(tinfo, 31);
                    _paymentInfo.Renting = tinfo;
                }
                else
                    _paymentInfo.Rest = tinfo;
                //if (answer.Length >= 8)
                //    _paymentInfo.NumberOfReceiptPackageInCPEF = BitConverter.ToUInt32(answer, 4);
                return _paymentInfo;
            }
            return new PaymentInfo();
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
        /// <param name="PrintingOfBarCodesOfGoods">=1 – печать штрих-кода товара (EAN13) - !!!! работает только в 11 протоколе</param>
        /// <returns></returns>
        public virtual ReceiptInfo FPPayMoneyEx(ushort Amount, byte Amount_Status, bool IsOneQuant, int Price, ushort NalogGroup, bool MemoryGoodName, string GoodName, ulong StrCode, bool PrintingOfBarCodesOfGoods = false)
        {
            byte[] forsending = new byte[] { 8 };

            forsending = byteHelper.Combine(forsending, byteHelper.ConvertUint32ToArrayByte3(Amount));
            Amount_Status = byteHelper.SetBit(Amount_Status, 6, IsOneQuant);
            //Amount_Status = byteHelper.SetBit(Amount_Status, 7, PrintingOfBarCodesOfGoods);
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

        public virtual void FPPeriodicReport(ushort pass, DateTime FirstDay, DateTime LastDay)
        {
            throw new NotImplementedException();
        }

        public virtual void FPPeriodicReport2(ushort pass, ushort FirstNumber, ushort LastNumber)
        {
            throw new NotImplementedException();
        }

        public virtual void FPPeriodicReportShort(ushort pass, DateTime FirstDay, DateTime LastDay)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Код: 32.       PrintVer печать налогового номера и версии программного обеспечения
        /// Налоговый номер и дата регистрации ЭККР печатаются только в фискальном режиме.
        /// </summary>
        public virtual void FPPrintVer()
        {
            byte[] forsending = new byte[] { 32 };
            byte[] answer = ExchangeWithFP(forsending);
        }

        public virtual uint FPPrintZeroReceipt()
        {
            byte[] forsending = new byte[] { 11 };//Comment            
            byte length;
            byte[] stringBytes = byteHelper.CodingBytes("Нульовий чек", 27, out length);
            length = byteHelper.SetBit(length, 7, false);
            forsending = byteHelper.Combine(forsending, new byte[] { length });
            forsending = byteHelper.Combine(forsending, stringBytes);
            byte[] answer = ExchangeWithFP(forsending);
            if (statusOperation)
            {
                forsending = new byte[] { 20, 0x03 };//Payment 
                forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(0 ^ (1 << 31)));
                answer = ExchangeWithFP(forsending);
                if (answer.Length == 4)
                    return BitConverter.ToUInt32(answer, 0);
            }
            return 0;
        }

        /// <summary>
        /// Код: 6.SetCashier               регистрация кассира (оператора)  в ЭККР
        /// После инициализации ЭККР значения паролей равны нулю (0). При длине имени 0 –  разрегистрация  
        /// кассира.Количество вводов пароля не более 10.
        /// </summary>
        /// <param name="CashierID">Номер</param>
        /// <param name="Name">Длина имени кассира (= n)0..15</param>
        /// <param name="Password">Пароль</param>
        public virtual void FPRegisterCashier(byte CashierID, string Name, ushort Password = 0)
        {
            byte[] forsending = new byte[] { 6 };//SetCashier
            forsending = byteHelper.Combine(forsending, BitConverter.GetBytes(Password));
            forsending = byteHelper.Combine(forsending, new byte[] { CashierID });
            byte length;
            byte[] stringBytes = byteHelper.CodingBytes(Name, 15, out length);

            forsending = byteHelper.Combine(forsending, new byte[] { length });
            forsending = byteHelper.Combine(forsending, stringBytes);
            byte[] answer = ExchangeWithFP(forsending);
        }

        /// <summary>
        /// Код: 15. ResetOrder                обнуление чека
        /// </summary>
        public virtual void FPResetOrder() //обнуление чека
        {
            byte[] forsending = new byte[] { 15 };
            byte[] answer = ExchangeWithFP(forsending);
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
        /// <param name="PrintingOfBarCodesOfGoods">печать штрих-кода товара (EAN13) - !!!!! Используется только в 11 протоколе</param>
        public virtual ReceiptInfo FPSaleEx(ushort Amount, byte Amount_Status, bool IsOneQuant, int Price, ushort NalogGroup, bool MemoryGoodName, string GoodName, ulong StrCode, bool PrintingOfBarCodesOfGoods = false)
        {
            byte[] forsending = new byte[] { 18 };

            forsending = byteHelper.Combine(forsending, byteHelper.ConvertUint32ToArrayByte3(Amount));
            Amount_Status = byteHelper.SetBit(Amount_Status, 6, IsOneQuant);
            //Amount_Status = byteHelper.SetBit(Amount_Status, 7, PrintingOfBarCodesOfGoods);
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

        public virtual void FPSetHeadLine(ushort Password, string StringInfo1, bool StringInfo1DoubleHeight, bool StringInfo1DoubleWidth, string StringInfo2, bool StringInfo2DoubleHeight, bool StringInfo2DoubleWidth, string StringInfo3, bool StringInfo3DoubleHeight, bool StringInfo3DoubleWidth, string TaxNumber, bool AddTaxInfo)
        {
            throw new NotImplementedException();
        }

        public virtual void FPSetPassword(byte UserID, ushort OldPassword, ushort NewPassword)
        {
            throw new NotImplementedException();
        }

        public virtual void FPSetTaxRate(ushort Password, Taxes tTaxes)
        {
            throw new NotImplementedException();
        }

        public virtual bool showBottomString(string Info)
        {
            //throw new NotImplementedException();
            return true;
        }

        public virtual bool showTopString(string Info)
        {
            //throw new NotImplementedException();
            return true;
        }

        /// <summary>
        /// Код: 33.
        /// GetBox  сумма наличных в денежном ящике
        /// </summary>
        /// <returns></returns>
        public UInt32 GetMoneyInBox()
        {
            byte[] forsending = new byte[] { 33 };
            byte[] answer = ExchangeWithFP(forsending);
            if (answer.Length==5)
            {
                return BitConverter.ToUInt32(answer, 0);
            }
            throw new ApplicationException("Сумма в кассе не определена");
            return 0;
        }
        /// <summary>
        /// ///Код: 46.  
        /// CplCutter запрет/разрешение на использование обрезчика
        ///Вызов команды меняет значение параметра на противоположный.
        /// </summary>
        /// <returns></returns>
        public bool FPCplCutter()
        {
            byte[] forsending = new byte[] { 46 };
            byte[] answer = ExchangeWithFP(forsending);
            return statusOperation;
        }

        public void FPNullCheck()
        {
            FPPrintZeroReceipt();
            
        }
    }
}
