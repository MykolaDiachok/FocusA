using CentralLib.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CentralLib.Connections
{
    public class ConnectFactory : SerialPort, IConnectFactory
    {

        public byte[] bytesBegin = { (byte)WorkByte.DLE, (byte)WorkByte.STX };
        public byte[] bytesEnd = { (byte)WorkByte.DLE, (byte)WorkByte.ETX };

        private byte[] glbytesResponse;
        private int waiting; // waiting time for serial port answer

        public bool statusOperation { get; set; }
        public byte ByteStatus { get; set; } // Возврат ФР статус
        public byte ByteResult { get; set; } // Возврат ФР результат
        public byte ByteReserv { get; set; } // Возврат ФР результат
        public string errorInfo { get; set; }
        public int ConsecutiveNumber { get; set; }
        public byte[] glbytesForSend { get; set; }
        public byte[] glbytesPrepare { get; set; }
        public bool useCRC16 { get; set; }

        private bool autoOpen=false, autoClose=false;

        private ByteHelper byteHelper;


        public ConnectFactory(DefaultPortCom defPortCom,bool autoOpen=false, bool autoClose=false)
        {
            this.autoOpen = autoOpen;
            this.autoClose = autoClose;

            byteHelper = new ByteHelper();
            initialCrc16();
            base.PortName = defPortCom.sPortNumber;
            base.BaudRate = defPortCom.baudRate;
            base.Parity = defPortCom.parity;
            base.DataBits = defPortCom.dataBits;
            base.StopBits = defPortCom.stopBits;
            base.WriteTimeout = defPortCom.writeTimeOut;
            base.ReadTimeout = defPortCom.readTimeOut;
            this.waiting = defPortCom.waiting;
            this.errorInfo = "";
            this.ConsecutiveNumber = 0;
            if (autoOpen)
                Open();
        }

        private byte[] prepareForSend(byte[] BytesForSend, bool useCRC16 = false, bool repeatError = false) // тут передают только код и параметры, получают готовую строку для отправки
        {
            this.glbytesPrepare = BytesForSend;

            byte[] prBytes = byteHelper.Combine(new byte[] { (byte)WorkByte.DLE, (byte)WorkByte.STX, (byte)ConsecutiveNumber }, BytesForSend);
            prBytes = byteHelper.Combine(prBytes, new byte[] { 0x00, (byte)WorkByte.DLE, (byte)WorkByte.ETX });
            prBytes[prBytes.Length - 3] = getchecksum(prBytes);

            for (int pos = 2; pos < prBytes.Length - 2; pos++)
            //for (int pos = 2; pos <= _out.Count - 3; pos++)
            {
                if (prBytes[pos] == (byte)WorkByte.DLE)
                {
                    prBytes = prBytes.Take(pos)
                    .Concat(new byte[] { (byte)WorkByte.DLE })
                    .Concat(prBytes.Skip(pos))
                    .ToArray();
                    //   prBytes..Insert(pos + 1, DLE);
                    pos++;
                }
            }
            if (useCRC16)
                prBytes = returnArrayBytesWithCRC16(prBytes);
            return prBytes;

        }

        private void setError(string errorInfo = "Unknown error", byte ByteStatus = 255, byte ByteResult = 255, byte ByteReserv = 255)
        {
            this.ByteStatus = ByteStatus;
            this.ByteResult = ByteResult;
            this.ByteReserv = ByteReserv;
            this.statusOperation = false;
            this.errorInfo += errorInfo + "; ";            
        }

        private async Task<byte[]> ExchangeFP(byte[] inputbyte, bool useCRC16 = false, bool repeatError = false)
        {

            this.ByteStatus = 0;
            this.ByteResult = 0;
            this.ByteReserv = 0;
            this.statusOperation = false;
            this.errorInfo = "";
            if (!repeatError)
            {
                this.ConsecutiveNumber++;
            }
            this.glbytesForSend = inputbyte;

            if (!base.IsOpen)
                base.Open();
            if (!base.IsOpen)
            {
                setError("Не возможно подключиться к порту:" + base.PortName.ToString());
                throw new ArgumentException(this.errorInfo);
            }
#if Debug
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("подготовка к отправке:{0}", PrintByteArray(inputbyte));         
#endif
            await base.BaseStream.WriteAsync(inputbyte, 0, inputbyte.Length);
#if Debug
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("отправлено");
#endif
            byte[] result = new byte[] { };
            Thread.Sleep(this.waiting);
            int bufferSize = base.BytesToRead;
            int twait = 0;
            do
            {
                byte[] result_fromPort = new byte[bufferSize];

#if Debug
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("подготовка к к получению {0}", bufferSize);
#endif
                if (bufferSize == 0)
                {
#if Debug
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("bufferSize == 0;время ожидания:{0}", 600);
#endif
                    Thread.Sleep(600);
                    bufferSize = base.BytesToRead;
                    if (bufferSize == 0)
                    {
                        break;
                    }
                }
                int x = await base.BaseStream.ReadAsync(result_fromPort, 0, bufferSize);
#if Debug
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("получено:{0}", PrintByteArray(result_fromPort));
#endif
                int count_wait = 0;
                for (int tByte = 0; tByte < bufferSize; tByte++)
                {
                    if ((result_fromPort[tByte] == (byte)WorkByte.ACK) || (result_fromPort[tByte] == (byte)WorkByte.SYN))
                    {
                        count_wait++;
                    }
                }
                if (bufferSize == 1 && result_fromPort[0] == (byte)WorkByte.ACK)
                {
#if Debug
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("bufferSize==1&&ACK Ждем:{0}", 1000);
#endif
                    Thread.Sleep(1000);
                }
                else if ((bufferSize < 10) || (bufferSize == count_wait) || ((count_wait > 0) && (bufferSize / count_wait < 2)))
                {
#if Debug
                    //Console.ForegroundColor = ConsoleColor.Green;
                    //Console.WriteLine("коефициент байтов ожидания:{0}", bufferSize / count_wait);
#endif  
                    twait++;

#if Debug
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("время ожидания:{0}", twait * 300);
#endif
                    Thread.Sleep(twait * 300);
                }
                if (twait > 10) break;
                result = byteHelper.Combine(result, result_fromPort);

                bufferSize = base.BytesToRead;
            } while (bufferSize > 0);

            byte[] BytesBegin = new byte[4];
            Buffer.BlockCopy(inputbyte, 0, BytesBegin, 0, 4);

            int positionPacketBegin = byteHelper.ByteSearch(result, BytesBegin);

            if (positionPacketBegin < 0)
            {
                setError("В байтах ответа не найдено начало, порт:" + base.PortName.ToString());
                throw new ArgumentException(this.errorInfo);
                // return null;
            }
            //}
            int positionPacketEnd = -1;
            int tCurrentPos = positionPacketBegin + 7;
            int tPostEnd = -1;
            do
            {
                tCurrentPos++;
                tPostEnd = byteHelper.ByteSearch(result, bytesEnd, tCurrentPos);
                if (tPostEnd != -1)
                {
                    tCurrentPos = tPostEnd;

                    if (result[tPostEnd - 1] != (byte)WorkByte.DLE)
                    {
                        positionPacketEnd = tPostEnd;
                        break;
                    }
                    else if ((result[tPostEnd - 1] == (byte)WorkByte.DLE) && (result[tPostEnd - 2] == (byte)WorkByte.DLE))
                    {
                        positionPacketEnd = tPostEnd;
                        // break; 
                    }
                }
            } while (tCurrentPos < result.Length);
            if (positionPacketEnd < 0)
            {
                setError("В байтах ответа не найдено конец, порт:" + base.PortName.ToString());
                throw new ArgumentException(this.errorInfo);
            }
            //e  } while (base.BytesToRead>0);

            byte[] unsigned = null;
            if (useCRC16)
            {
                unsigned = new byte[positionPacketEnd - positionPacketBegin + 4];
                Buffer.BlockCopy(result, positionPacketBegin, unsigned, 0, positionPacketEnd - positionPacketBegin + 4);
            }
            else
            {
                unsigned = new byte[positionPacketEnd - positionPacketBegin + 2];
                Buffer.BlockCopy(result, positionPacketBegin, unsigned, 0, positionPacketEnd - positionPacketBegin + 2);
            }
            //this.bytesOutput = unsigned;
            //TODO: доработать проверку CRC && CRC16
            unsigned = byteHelper.returnWithOutDublicateDLE(unsigned);
            this.glbytesResponse = unsigned;
            this.statusOperation = true;
            this.ByteStatus = unsigned[4];
            this.ByteResult = unsigned[5];
            this.ByteReserv = unsigned[6];
            this.errorInfo = "";
            //Console.WriteLine(PrintByteArray(unsigned));
            //Console.WriteLine(PrintByteArray(unsigned.Skip(8).Take(unsigned.Length - 7 - 3 - ((useCRC16) ? 2 : 0)).ToArray()));
            return unsigned;//.Skip(8).Take(unsigned.Length-7-3- ((useCRC16)?2:0) ).ToArray();
        }

        private byte[] returnBytesWithoutSufixAndPrefix(byte[] inputbytes, bool useCRC16 = false)
        {
            int lenght = inputbytes.Length - 7 - 3 - ((useCRC16) ? 2 : 0);
            byte[] outputBytes = new byte[lenght];
            System.Buffer.BlockCopy(inputbytes, 7, outputBytes, 0, lenght);
            return outputBytes;
        }


        public virtual byte[] dataExchange(byte[] input, bool repeatError = false)
        {
            return this.dataExchange(input, this.useCRC16, repeatError);
        }

        public virtual byte[] dataExchange(byte[] input)
        {
            return this.dataExchange(input, this.useCRC16, false);
        }

        public virtual byte[] dataExchange(byte[] input, bool useCRC16 = false, bool repeatError = false)
        {
            Func<byte[], Task<byte[]>> function = async (byte[] inByte) =>
            {
                return await ExchangeFP(prepareForSend(inByte, useCRC16, repeatError), useCRC16, repeatError);
            };

            Task<byte[]> answer = function(input);
            //Task<byte[]> answer = ExchangeFP(prepareForSend(input, useCRC16));
            //return await answer;
            try
            {
                return returnBytesWithoutSufixAndPrefix(answer.Result);
            }
            catch (AggregateException e)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Caught {0}, exception: {1}", e.InnerExceptions.Count, string.Join(", ", e.InnerExceptions.Select(x => x.Message)));

                setError(sb.ToString());
#if Debug
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Описание ошибки:{0}", this.errorInfo);
#endif
            }
            finally
            {
                if (this.autoClose)
                    Close();
            }
            return null;
        }

        public new void Open()
        {
            if (base.IsOpen)
            {
                base.Close();
            }
            try
            {
                base.Open();
                this.statusOperation = true;
            }
            catch (IOException e)
            {
                this.statusOperation = false;


                setError(e.Message);
                //#if Debug
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Описание ошибки:{0}", this.errorInfo);
                //#endif
            }
            catch (AggregateException e)
            {
                this.statusOperation = false;
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Caught {0}, exception: {1}", e.InnerExceptions.Count, string.Join(", ", e.InnerExceptions.Select(x => x.Message)));

                setError(sb.ToString());
#if Debug
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Описание ошибки:{0}", this.errorInfo);
#endif
            }
        }

        public new void Close()
        {
            base.Close();
        }


        #region checksum
        public byte getchecksum(List<byte> buf)
        {
            int i, n;
            byte lobyte, cs;
            uint sum, res;

            n = buf.Count - 3;
            sum = 0;
            cs = 0x00;
            lobyte = 0x00;

            for (i = 2; i < n; i++)
                sum += buf[i];

            do
            {
                res = sum + cs;
                cs++;
                lobyte = (byte)(res & 0xFF);
            }
            while (lobyte != 0x00);
            return (byte)(cs - 1);
        }

        public byte getchecksum(byte[] buf)
        {
            int i, sum, n, res;
            byte lobyte, cs;

            n = buf.Length - 3;
            sum = 0;
            cs = 0x00;
            lobyte = 0x00;

            for (i = 2; i < n; i++)
                //for (i = 0; i < buf.Length; ++i)
                sum += Convert.ToInt16(buf[i]);

            do
            {
                res = sum + cs;
                cs++;
                lobyte = (byte)(res & 0xFF);
            }
            while (lobyte != 0x00);
            return (byte)(cs - 1);
        }

        public int getchecksum(byte[] buf, int len)
        {
            int i, sum, n, res;
            byte lobyte, cs;

            n = len - 3;
            sum = 0;
            cs = 0x00;
            lobyte = 0x00;

            for (i = 2; i < n; i++)
                //for (i = 0; i < buf.Length; ++i)
                sum += Convert.ToInt16(buf[i]);

            do
            {
                res = sum + cs;
                cs++;
                lobyte = (byte)(res & 0xFF);
            }
            while (lobyte != 0x00);
            return cs - 1;
        }
        #endregion

        #region CRC16
        private ushort[] table = new ushort[256];

        public ushort ComputeChecksum(params byte[] bytes)
        {
            ushort crc = 0;
            for (int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(crc ^ bytes[i]);
                crc = (ushort)((crc >> 8) ^ table[index]);
            }
            return crc;
        }

        public byte[] ComputeChecksumBytes(params byte[] bytes)
        {
            ushort crc = ComputeChecksum(bytes);
            return BitConverter.GetBytes(crc);
        }

        public void initialCrc16()
        {
            ushort polynomial = (ushort)0x8408;
            ushort value;
            ushort temp;
            for (ushort i = 0; i < table.Length; ++i)
            {
                value = 0;
                temp = i;
                for (byte j = 0; j < 8; ++j)
                {
                    if (((value ^ temp) & 0x0001) != 0)
                    {
                        value = (ushort)((value >> 1) ^ polynomial);
                    }
                    else
                    {
                        value >>= 1;
                    }
                    temp >>= 1;
                }
                table[i] = value;
            }
        }


        public byte[] returnArrayBytesWithCRC16(byte[] inBytes)
        {

            //byte[] bytebegin = { DLE, STX };
            //byte[] byteend = { DLE, ETX };

            byte[] tempb = byteHelper.returnWithOutDublicateDLE(inBytes);
            var searchBegin = byteHelper.PatternAt(tempb, bytesBegin);
            if (searchBegin == null)
                return null;

            var searchEnd = byteHelper.PatternAt(tempb, bytesEnd);
            if (searchEnd == null)
                return null;

            var newArr = tempb.Skip((int)searchBegin + 2).Take((int)searchEnd - 2).ToArray();

            byte[] a = new byte[newArr.Length + 1];
            newArr.CopyTo(a, 0);
            a[newArr.Length] = (byte)WorkByte.ETX;


            //var control = tempb.Skip((int)searchEnd + 2).Take(2).ToArray();


            byte[] crcBytes = ComputeChecksumBytes(a);
            byte[] retBytes = new byte[inBytes.Length + 2];
            inBytes.CopyTo(retBytes, 0);
            retBytes[retBytes.Length - 2] = crcBytes[0];
            retBytes[retBytes.Length - 1] = crcBytes[1];
            return retBytes;
        }

     

        #endregion

    }
}
