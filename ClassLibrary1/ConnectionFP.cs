//#define Debug


using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace CentralLib.ConnectionFP
{
    public class ConnectionFP : SerialPort
    {
        //https://github.com/cyungmann/telem/blob/58f96c34f67edf5c0a0a82682ed5afdd5e7d5bca/NUSolarTelemetry_Car/Program.cs - like serila port realisation
        //https://github.com/brianzinn/marineNavigation/blob/1e45347b763bdf5dfcdce7379281bfe9cf399742/Communication/SerialPort.cs - good serial
        //https://github.com/chrispyduck/sharpberry/blob/d0e7311303b7656f67813420bb59f3bdd37a5a72/sharpberry.obd/SerialPort.cs - raspery p serail



        private byte[] bytesBegin = { (byte)WorkByte.DLE, (byte)WorkByte.STX };
        private byte[] bytesEnd = { (byte)WorkByte.DLE, (byte)WorkByte.ETX };
        public byte[] glbytesForSend { get; protected set; }
        public byte[] glbytesPrepare { get; protected set; }
        private byte[] glbytesResponse;       
        private int waiting; // waiting time for serial port answer
        public int ConsecutiveNumber { get; private set; }

        public bool statusOperation { get; private set; }
        public byte ByteStatus { get; private set; } // Возврат ФР статус
        public byte ByteResult { get; private set; } // Возврат ФР результат
        public byte ByteReserv { get; private set; } // Возврат ФР результат
        public string errorInfo { get; protected set; }




        public ConnectionFP(DefaultPortCom.DefaultPortCom defPortCom):base()
        {
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
            ConsecutiveNumber = 0;
        }

        private byte[] prepareForSend(byte[] BytesForSend, bool useCRC16=true) // тут передают только код и параметры, получают готовую строку для отправки
        {
            this.glbytesPrepare = BytesForSend;
            byte[] prBytes = Combine(new byte[] { (byte)WorkByte.DLE, (byte)WorkByte.STX,(byte)ConsecutiveNumber}, BytesForSend);
            prBytes = Combine(prBytes, new byte[] {0x00, (byte)WorkByte.DLE, (byte)WorkByte.ETX });
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

        private void setError(string errorInfo="Unknown error")
        {
            this.ByteStatus = 255;
            this.ByteResult = 255;
            this.ByteReserv = 255;
            this.statusOperation = false;
            this.errorInfo += errorInfo+"; ";
        }

        private async Task<byte[]> ExchangeFP(byte[] inputbyte, bool useCRC16 = true)
        {

            this.ByteStatus = 0;
            this.ByteResult = 0;
            this.ByteReserv = 0;
            this.statusOperation = false;
            this.errorInfo = "";

            this.ConsecutiveNumber++;
            this.glbytesForSend = inputbyte;

            if (!base.IsOpen)
                base.Open();
            if (!base.IsOpen)
            {
                setError("Не возможно подключиться к порту:" + base.PortName.ToString());
                throw new ArgumentException(this.errorInfo);
            }
#if Debug
            Console.WriteLine("подготовка к отправке:{0}", PrintByteArray(inputbyte));         
#endif
            await base.BaseStream.WriteAsync(inputbyte, 0, inputbyte.Length);
#if Debug
            Console.WriteLine("отправлено");
#endif
            do
            {
                Thread.Sleep(this.waiting);
                int buferSize = base.BytesToRead;
                byte[] result = new byte[buferSize];
                if (buferSize == 0)
                {
                    setError("Нулевой ответ с порта:" + base.PortName.ToString());
                    // throw new ArgumentException(this.errorInfo);
                }
#if Debug
            Console.WriteLine("подготовка к к получению");
#endif
                int x = await base.BaseStream.ReadAsync(result, 0, buferSize);
#if Debug
            Console.WriteLine("получено:{0}", PrintByteArray(result));
#endif
                byte[] BytesBegin = new byte[4];
                Buffer.BlockCopy(inputbyte, 0, BytesBegin, 0, 4);

                int positionPacketBegin = ByteSearch(result, BytesBegin);
                if (positionPacketBegin < 0)
                {
                    Thread.Sleep(2 * this.waiting);
                    buferSize = base.BytesToRead;
                    if (buferSize == 0)
                    {
                        setError("Нулевой ответ(вторая попытка) с порта:" + base.PortName.ToString());
                        ///throw new ArgumentNullException();
                    }
                    result = new byte[buferSize];
                    x = await base.BaseStream.ReadAsync(result, 0, buferSize);
                    positionPacketBegin = ByteSearch(result, BytesBegin) - 1;
                    if (positionPacketBegin < 0)
                    {
                        Thread.Sleep(4 * this.waiting);
                        buferSize = base.BytesToRead;
                        if (buferSize == 0)
                        {
                            setError("Нулевой ответ(вторая попытка) с порта:" + base.PortName.ToString());
                            throw new ArgumentNullException();
                        }
                        result = new byte[buferSize];
                        x = await base.BaseStream.ReadAsync(result, 0, buferSize);
                        positionPacketBegin = ByteSearch(result, BytesBegin) - 1;

                    }
                    if (positionPacketBegin < 0)
                    {
                        setError("В байтах ответа не найдено начало, порт:" + base.PortName.ToString());
                        throw new ArgumentException(this.errorInfo);
                        // return null;
                    }
                }
                int positionPacketEnd = -1;
                int tCurrentPos = positionPacketBegin + 7;
                int tPostEnd = -1;
                do
                {
                    tCurrentPos++;
                    tPostEnd = ByteSearch(result, bytesEnd, tCurrentPos);
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
            } while (base.BytesToRead>0);

            byte[] unsigned = null;
            if (useCRC16)
            {
                unsigned = new byte[positionPacketEnd - positionPacketBegin + 4];
                Buffer.BlockCopy(result, positionPacketBegin, unsigned, 0, positionPacketEnd - positionPacketBegin + 4); }
            else
            {
                unsigned = new byte[positionPacketEnd - positionPacketBegin + 2];
                Buffer.BlockCopy(result, positionPacketBegin, unsigned, 0, positionPacketEnd - positionPacketBegin + 2);
            }
            //this.bytesOutput = unsigned;
            unsigned = returnWithOutDublicateDLE(unsigned);
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

        private byte[] returnBytesWithoutSufixAndPrefix(byte[] inputbytes,bool useCRC16 = true)
        {
            int lenght = inputbytes.Length-7-3 -((useCRC16)?2:0);
            byte[] outputBytes = new byte[lenght];
            System.Buffer.BlockCopy(inputbytes, 7, outputBytes, 0, lenght);
            return outputBytes;
        }


        public byte[] dataExchange(byte[] input, bool useCRC16 = true)
        {
            Func<byte[], Task<byte[]>> function = async (byte[] inByte) =>
             {
                 return await ExchangeFP(prepareForSend(inByte, useCRC16));
             };

            Task<byte[]> answer = function(input);
            //Task<byte[]> answer = ExchangeFP(prepareForSend(input, useCRC16));
            //return await answer;
            try
            {
                return returnBytesWithoutSufixAndPrefix(answer.Result);
            }
            catch(AggregateException e)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Caught {0}, exception: {1}", e.InnerExceptions.Count, string.Join(", ", e.InnerExceptions.Select(x=>x.Message)));
                
                setError(sb.ToString());
                //#if Debug
                Console.WriteLine("Описание ошибки:{0}", this.errorInfo);
                //#endif
            }
            return null;
        }



        //private async void ConnectionFP_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    var port = (SerialPort)sender;
        //    if ((port != null) && (port.IsOpen))
        //    {

        //        int buferSize = port.BytesToRead;
        //        byte[] result = new byte[buferSize];
        //        int read = await base.BaseStream.ReadAsync(result, 0, buferSize);

        //        bytesBuffered = Combine(bytesBuffered, result);

        //        int positionPacketBegin = ByteSearch(bytesBuffered, bytesBegin) -1;
        //        if (positionPacketBegin < 0)
        //        {
        //            Thread.Sleep(40);
        //            // для проверки возможно вызывать еще раз ConnectionFP_DataReceived если все закончилось
        //            return; // waiting begin string
        //        }
        //        int positionPacketEnd = -1;
        //        int tCurrentPos = positionPacketBegin;
        //        int tPostEnd = -1;
        //        do
        //        {
        //            tCurrentPos++;
        //            tPostEnd = ByteSearch(bytesBuffered, bytesEnd, tCurrentPos);
        //            if (tPostEnd != -1)
        //            {
        //                tCurrentPos = tPostEnd;

        //                if (bytesBuffered[tPostEnd - 1] != (byte)WorkByte.DLE)
        //                {
        //                    positionPacketEnd = tPostEnd;
        //                    break;
        //                }
        //                else if ((bytesBuffered[tPostEnd - 1] == (byte)WorkByte.DLE) && (bytesBuffered[tPostEnd - 2] == (byte)WorkByte.DLE))
        //                {
        //                    positionPacketEnd = tPostEnd;
        //                    // break; 
        //                }
        //            }
        //        } while (tCurrentPos < bytesBuffered.Length);
        //        if (positionPacketEnd < 0)
        //        {
        //            Thread.Sleep(40);                   
        //            return; //waiting end postion
        //        }
        //        byte[] unsigned = new byte[positionPacketEnd - positionPacketBegin + 4];
        //        Buffer.BlockCopy(bytesBuffered, positionPacketBegin, unsigned, 0, positionPacketEnd - positionPacketBegin + 4);
        //        //this.bytesOutput = unsigned;
        //        this.bytesResponse = unsigned;

        //        //if (DataRead != null)
        //        //{
        //        //    DataRead(this, new SerialReadEventArgs(unsigned));
        //        //}




        //    }
        //    //
        //    //var searchEnd = PatternAt(result, byteend);

        //}

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
                Console.WriteLine("Описание ошибки:{0}", this.errorInfo);
                //#endif
            }
            catch (AggregateException e)
            {
                this.statusOperation = false;
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Caught {0}, exception: {1}", e.InnerExceptions.Count, string.Join(", ", e.InnerExceptions.Select(x => x.Message)));

                setError(sb.ToString());
                //#if Debug
                Console.WriteLine("Описание ошибки:{0}", this.errorInfo);
                //#endif
            }
        }

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

#region checksum
        byte getchecksum(List<byte> buf)
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

        byte getchecksum(byte[] buf)
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

        int getchecksum(byte[] buf, int len)
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
        ushort[] table = new ushort[256];

        ushort ComputeChecksum(params byte[] bytes)
        {
            ushort crc = 0;
            for (int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(crc ^ bytes[i]);
                crc = (ushort)((crc >> 8) ^ table[index]);
            }
            return crc;
        }

        byte[] ComputeChecksumBytes(params byte[] bytes)
        {
            ushort crc = ComputeChecksum(bytes);
            return BitConverter.GetBytes(crc);
        }

        void initialCrc16()
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


        private byte[] returnArrayBytesWithCRC16(byte[] inBytes)
        {
           
            //byte[] bytebegin = { DLE, STX };
            //byte[] byteend = { DLE, ETX };

            byte[] tempb = returnWithOutDublicateDLE(inBytes);
            var searchBegin = PatternAt(tempb, bytesBegin);
            if (searchBegin == null)
                return null;

            var searchEnd = PatternAt(tempb, bytesEnd);
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

        string PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("new byte[] { ");
            foreach (var b in bytes)
            {
                sb.Append(b + ", ");
            }
            sb.Append("}");
            return sb.ToString();
        }


    }
}
