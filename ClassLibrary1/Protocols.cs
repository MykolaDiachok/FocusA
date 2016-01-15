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
using System.Threading;

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
        private Status tStatus;
        public Status status {
            get {
                if ((lastByteCommand !=0))
                    getStatus();
                return this.tStatus;
            }
            private set { }
        }
        private byte? lastByteCommand = null;
        public bool useCRC16 { get; private set; }

        private void initial()
        {
            getStatus();
        }


        public Protocols(ConnectionFP.ConnectionFP connFP)
        {
            this.connFP = connFP;
            this.useCRC16 = true;
            initial();
        }

        public Protocols(int serialPort)
        {
            DefaultPortCom.DefaultPortCom initialPort = new DefaultPortCom.DefaultPortCom((byte)serialPort);
            this.connFP = new ConnectionFP.ConnectionFP(initialPort);
            try {
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

        private byte[] ExchangeWithFP(byte[] inputByte)
        {
            byte[] answer;
            this.lastByteCommand = inputByte[0];
            answer = connFP.dataExchange(inputByte,useCRC16,false);
            if (!connFP.statusOperation) //repetition if error
            {
                Thread.Sleep(800);
#if Debug
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("ошибка первое ожидание");
                ///Console.ReadKey();
#endif
                answer = connFP.dataExchange(inputByte,useCRC16,true);
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
            return answer;
        }

        


        private void getStatus()
        {
            byte[] forsending = new byte[] { 0 };
            byte[] answer = ExchangeWithFP(forsending);

            if (connFP.statusOperation)
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

                int len1 = answer[25];
                string str1 = "";

                int len2 = 0;
                string str2 = "";

                int len3 = 0;
                string str3 = "";

                int len4 = 0;
                string str4 = "";

                //string ver = EncodingBytes(answer.Skip(answer.Length-6).Take(5).ToArray());
                byte[] verBytes = new byte[5];
                System.Buffer.BlockCopy(answer, answer.Length - 5, verBytes, 0, 5);
                string ver = EncodingBytes(verBytes);
                switch(ver)
                {
                    case "ЕП-11":
                        this.currentProtocol = WorkProtocol.EP11;
                        this.useCRC16 = true;
                        break;
                };

                this.tStatus = new Status(answer.Take(2).ToArray()
                    , EncodingBytes(answer.Skip(2).Take(19).ToArray())
                    , new DateTime(2000 + _year, _month, _day, _hour, _min, 0)
                    , EncodingBytes(answer.Skip(26).Take(10).ToArray())
                    , len1, str1
                    , len2, str2
                    , len3, str3
                    , len4, str4
                    , ver
                    , connFP.ConsecutiveNumber
                    );
            }
            
        }


        public void FPLineFeed()
        {
            byte[] forsending = new byte[] { 14 };
            byte[] answer = ExchangeWithFP(forsending);
        }

        #region Отчеты
        public void FPArtReport(ushort pass = 0,UInt64? CodeBeginning=null,UInt64? CodeFinishing=null)
        {
            byte[] forsending = new byte[] { 9 };
            forsending = Combine(forsending, BitConverter.GetBytes(pass));
            byte[] answer = ExchangeWithFP(forsending);
        }


        public void FPDayReport(ushort pass=0)
        {
            byte[] forsending = new byte[] { 9 };
            forsending = Combine(forsending, BitConverter.GetBytes(pass));
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
                        int _hour = Math.Min(Math.Max((int)Convert.ToInt16(hexhour),0),23);

                        string hexminute = answerTime[1].ToString("X");
                        int _minute = Math.Min(Math.Max((int)Convert.ToInt16(hexminute),0),59);

                        string hexsecond = answerTime[2].ToString("X");
                        int _second = Math.Min(Math.Max((int)Convert.ToInt16(hexsecond),0),59);

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


        public string EncodingBytes(byte[] inputBytes)
        {
            Encoding cp866 = Encoding.GetEncoding(866);
            return cp866.GetString(inputBytes);
        }

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
