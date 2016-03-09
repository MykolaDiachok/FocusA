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

        public virtual  DateTime fpDateTime
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

        public virtual PapStat papStat
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual Status status
        {
            get
            {
                throw new NotImplementedException();
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
            else if ((tPr == "ЕП-06")||((tPr.Length>2) &&(tPr.Substring(1,2)== "ЕП"))) //Если не 11 и есть инфо по ЕП то считаем что  это 6 протокол
            {
                this.useCRC16 = false;
                this.currentProtocol = WorkProtocol.EP06;
                this.connFP = new CentralLib.Connections.ConnectFP_EP06(this.defaultPortCom);
                return new Protocol_EP06(this.defaultPortCom);
            }

                throw new ApplicationException("Протокол не определен, работа программы не возможна");
            return null;
        }

        public virtual void FPArtReport(ushort pass = 0, uint? CodeBeginning = default(uint?), uint? CodeFinishing = default(uint?))
        {
            throw new NotImplementedException();
        }

        public virtual uint FPCashIn(uint Summa)
        {
            throw new NotImplementedException();
        }

        public virtual uint FPCashOut(uint Summa)
        {
            throw new NotImplementedException();
        }

        public virtual void FPCommentLine(string CommentLine, bool OpenRefundReceipt = false)
        {
            throw new NotImplementedException();
        }

        public virtual void FPCplOnline()
        {
            throw new NotImplementedException();
        }

        public virtual void FPDayClrReport(ushort pass = 0)
        {
            throw new NotImplementedException();
        }

        public virtual void FPDayReport(ushort pass = 0)
        {
            throw new NotImplementedException();
        }

        public virtual string FPGetPayName(byte PayType)
        {
            throw new NotImplementedException();
        }

        public virtual void FPGetTaxRate()
        {
            throw new NotImplementedException();
        }

        public virtual void FPLineFeed()
        {
            throw new NotImplementedException();
        }

        public virtual void FPOpenBox(byte impulse = 0)
        {
            throw new NotImplementedException();
        }

        public virtual PaymentInfo FPPayment(byte Payment_Status, uint Payment, bool CheckClose, bool FiscStatus, string AuthorizationCode = "")
        {
            throw new NotImplementedException();
        }

        public virtual ReceiptInfo FPPayMoneyEx(ushort Amount, byte Amount_Status, bool IsOneQuant, int Price, ushort NalogGroup, bool MemoryGoodName, string GoodName, ulong StrCode, bool PrintingOfBarCodesOfGoods = false)
        {
            throw new NotImplementedException();
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

        public virtual void FPPrintVer()
        {
            throw new NotImplementedException();
        }

        public virtual uint FPPrintZeroReceipt()
        {
            throw new NotImplementedException();
        }

        public virtual void FPRegisterCashier(byte CashierID, string Name, ushort Password = 0)
        {
            throw new NotImplementedException();
        }

        public virtual void FPResetOrder()
        {
            throw new NotImplementedException();
        }

        public virtual ReceiptInfo FPSaleEx(ushort Amount, byte Amount_Status, bool IsOneQuant, int Price, ushort NalogGroup, bool MemoryGoodName, string GoodName, ulong StrCode, bool PrintingOfBarCodesOfGoods = false)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public virtual bool showTopString(string Info)
        {
            throw new NotImplementedException();
        }
    }
}
