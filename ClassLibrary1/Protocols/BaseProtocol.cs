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
    public class BaseProtocol : IDisposable
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

        private string getstringProtocol()
        {
            byte[] forsending = new byte[] { 0 };
            byte[] answer = ExchangeWithFP(forsending);
            string tCurProtocol="";
            if ((connFP.statusOperation) && (answer.Length > 21))
            {
                tCurProtocol = byteHelper.EncodingBytes(answer, answer.Length - 5, 5);
            }
            return tCurProtocol;
        }

        public IProtocols getCurrentProtocol()
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
            else if ((tPr == "ЕП-06")||(tPr.Substring(1,2)== "ЕП")) //Если не 11 и есть инфо по ЕП то считаем что  это 6 протокол
            {
                this.useCRC16 = false;
                this.currentProtocol = WorkProtocol.EP06;
                this.connFP = new CentralLib.Connections.ConnectFP_EP06(this.defaultPortCom);
                return new Protocol_EP06(this.defaultPortCom);
            }

                throw new ApplicationException("Протокол не определен, работа программы не возможна");
            return null;
        }

    }
}
