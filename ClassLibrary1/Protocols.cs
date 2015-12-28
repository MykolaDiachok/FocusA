#define Debug

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralLib;
using CentralLib.ConnectionFP;
using CentralLib.DefaultPortCom;
using System.Collections;

namespace CentralLib.Protocols
{
    public class Protocols : IDisposable
    {
        public bool statusOperation { get; private set; }
        public byte ByteStatus { get; private set; } // Возврат ФР статус
        public byte ByteResult { get; private set; } // Возврат ФР результат
        public byte ByteReserv { get; private set; } // Возврат ФР результат
        public WorkProtocol currentProtocol { get; private set; }
        private ConnectionFP.ConnectionFP connFP = null;
        private bool killConnFP = false;
        public string errorInfo { get; protected set; }


        public Protocols(ConnectionFP.ConnectionFP connFP)
        {
            this.connFP = connFP;
        }

        public Protocols(int serialPort)
        {
            DefaultPortCom.DefaultPortCom initialPort = new DefaultPortCom.DefaultPortCom(4);
            this.connFP = new ConnectionFP.ConnectionFP(initialPort);
            try {
                connFP.Open();
            }
            catch (AggregateException e)
            {
                this.statusOperation = false;
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Caught {0}, exception: {1}", e.InnerExceptions.Count, string.Join(", ", e.InnerExceptions.Select(x => x.Message)));
                
                //#if Debug
                Console.WriteLine("Описание ошибки:{0}", this.errorInfo);
                //#endif
            }
            killConnFP = true;
        }

        public void Dispose()
        {
            if (killConnFP)
            {
                connFP.Close();
                ((IDisposable)connFP).Dispose();
            }
        }

        public struct Status
        {
            public bool usingCollection { get; private set; } //используются сборы
            public bool modeOfRegistrationsOfPayments { get; private set; } //режим регистраций оплат в чеке(запрещены все регистрации  кроме оплат и комментариев)
            public bool cashDrawerIsOpened { get; private set; } //закрыт денежный ящик
            public bool receiptSaleOrPayout { get; private set; } //чек: продажи/выплаты(0/1)
            public bool VATembeddedOrVATaddon { get; private set; } //НДС вложенный/НДС добавляемый(0/1)
            public bool sessionIsOpened { get; private set; } //смена открыта(были закрытые чеки; запрещены команды режима программирования)
            public bool receiptIsOpened { get; private set; } //открыт чек
            public bool usedFontB { get; private set; } //используется шрифт B
            public bool printingOfEndUserLogo { get; private set; } //печать логотипа торговой точки
            public bool paperCuttingForbidden { get; private set; } //запрет обрезчика бумаги
            public bool modeOfPrintingOfReceiptOfServiceReport { get; private set; } //режим печати чека служебного отчета
            public bool printerIsFiscalized { get; private set; } //принтер фискализирован
            public bool emergentFinishingOfLastCommand { get; private set; } //аварийное завершение последней команды
            public bool modeOnLineOfRegistrations { get; private set; } //режим OnLine регистраций
            public string serialNumber;
            public DateTime manufacturingDate;
            public DateTime DateTimeregistration;
            public string fiscalNumber;



        }

        #region customer display
        public bool showTopString(string Info)
        {
            Encoding cp866 = Encoding.GetEncoding(866);
            string tempStr = Info.Substring(0, Math.Min(20, Info.Length));
            byte[] forsending = Combine(new byte[] { 0x1b, 0x00, (byte)tempStr.Length }, cp866.GetBytes(tempStr));
            var answer = connFP.dataExchange(forsending);
            this.ByteStatus = connFP.ByteStatus;
            this.ByteResult = connFP.ByteResult;
            this.ByteReserv = connFP.ByteReserv;
            this.errorInfo = connFP.errorInfo;
            return connFP.statusOperation;
        }

        public bool showBottomString(string Info)
        {
            Encoding cp866 = Encoding.GetEncoding(866);
            string tempStr = Info.Substring(0, Math.Min(20, Info.Length));
            byte[] forsending = Combine(new byte[] { 0x1b, 0x01, (byte)tempStr.Length }, cp866.GetBytes(tempStr));
            var answer = connFP.dataExchange(forsending);
            this.ByteStatus = connFP.ByteStatus;
            this.ByteResult = connFP.ByteResult;
            this.ByteReserv = connFP.ByteReserv;
            this.errorInfo = connFP.errorInfo;
            return connFP.statusOperation;
        }
        #endregion


        #region helper
        #region byte
        private byte[] Combine(byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length + b.Length];
            System.Buffer.BlockCopy(a, 0, c, 0, a.Length);
            System.Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
            return c;
        }

        int ByteSearch(byte[] searchIn, byte[] searchBytes, int start = 0)
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

        byte[] returnWithOutDublicateDLE(byte[] source)
        {
            return returnWithOutDublicate(source, new byte[] { (byte)WorkByte.DLE, (byte)WorkByte.DLE });
        }



        byte[] returnWithOutDublicate(byte[] source, byte[] pattern)
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

        int? PatternAt(byte[] source, byte[] pattern)
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

        #endregion

        #region bit

        byte BitArrayToByte(BitArray ba)
        {
            byte result = 0;
            for (byte index = 0, m = 1; index < 8; index++, m *= 2)
                result += ba.Get(index) ? m : (byte)0;
            return result;
        }

        byte[] ToByteArray(BitArray bits)
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

        bool GetBit(byte val, int num)
        {
            if ((num > 7) || (num < 0))//Проверка входных данных
            {
                throw new ArgumentException();
            }
            return ((val >> num) & 1) > 0;//собственно все вычисления
        }

        byte SetBit(byte val, int num, bool bit)
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



        #endregion

        #endregion

    }
}
