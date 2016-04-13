using CentralLib.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CentralLib.Connections
{

    

    class ConnectNetFactory : IConnectFactory, IDisposable
    {
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
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

        private string IpAdress;
        private int port;
        private ByteHelper byteHelper;

        public bool IsOpen
        {
            get
            {
                return true;
            }
        }

        public ConnectNetFactory(string IpAdress, int port, int waiting)
        {
            this.IpAdress = IpAdress;
            this.port = port;
            byteHelper = new ByteHelper();
            byteHelper.initialCrc16();
            this.errorInfo = "";
            this.ConsecutiveNumber = 0;
            this.waiting = 200;
        }


        public void Open()
        {
            //нет смысла открывать, так как не ком порт
            //сохранено для поддержки
        }

        public void Close()
        {
            //нет смысла Закрывать, так как не ком порт
            //сохранено для поддержки
        }

        public bool PingHost(string _HostURI, int _PortNumber)
        {
            try
            {
                TcpClient client = new TcpClient(_HostURI, _PortNumber);
                client.Close();
                return true;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error pinging host:'" + _HostURI + ":" + _PortNumber.ToString() + "'");
                logger.Fatal(ex, "Error pinging host:'" + _HostURI + ":" + _PortNumber.ToString() + "'");
                return false;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputbyte"></param>
        /// <param name="useCRC16"></param>
        /// <param name="repeatError"></param>
        /// <returns></returns>
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
            int taskTry = -1;
            //if (!base.IsOpen)
            //    base.Open();
            //if (!base.IsOpen)
            //{
            //    setError("Не возможно подключиться к порту:" + base.PortName.ToString());
            //    throw new ArgumentException(this.errorInfo);
            //}
            //logger.Trace("Begin>>>>");
            if (!PingHost(IpAdress, port))
            {
                setError("Ошибка подключения к серверу ip:" + this.IpAdress + ":" + port.ToString());
                throw new ApplicationException("Ошибка подключения к серверу");
            }
            byte[] unsigned = null;
            using (TcpClient client = new TcpClient())
            {

                await client.ConnectAsync(IPAddress.Parse(IpAdress), port);
                client.ReceiveTimeout = 5000;
                client.SendTimeout = 5000;
#if Debug
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("подготовка к отправке:{0}", byteHelper.PrintByteArrayX(inputbyte));
#endif
                using (var networkStream = client.GetStream())
                {
                    networkStream.WriteTimeout = 5000;
                    networkStream.ReadTimeout = 5000;
                Begin:
                    if (taskTry > 5)
                    {
                        string sf = String.Format("Выполнено {0} циклов, ответа нет {1}:{2}", this.IpAdress, port.ToString());
                        setError(sf);
                        logger.Trace(sf);
                        throw new ApplicationException(this.errorInfo);
                    }
                    taskTry++;
                    //logger.Trace("try{1}, send to FP:{0}", byteHelper.PrintByteArrayX(inputbyte), taskTry);
                    await networkStream.WriteAsync(inputbyte, 0, inputbyte.Length);
                    await networkStream.FlushAsync();
                    //base.Write(inputbyte, 0, inputbyte.Length);

#if Debug
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("отправлено");
#endif
                    byte[] BytesBegin = new byte[4];
                    Buffer.BlockCopy(inputbyte, 0, BytesBegin, 0, 4);

                    int positionPacketEnd = -1;
                    int tCurrentPos = 7;
                    //int tPostEnd = -1;



                    byte[] result = new byte[] { };

                    for (int x = 1; x < 5; x++)
                    {
                        int coef = x;
                        if ((inputbyte[2] == 13) || (inputbyte[2] == 9)) // Если отчеты то ждем 2 раза дольше
                        {
                            coef = x * 2;


                        }
                        Thread.Sleep(coef * this.waiting);
                        //logger.Trace("ожидание в цикле {1} = {0}", coef * this.waiting,x);


                        int twait = 0;
                        while (networkStream.DataAvailable)
                        {
                            //logger.Trace("DataAvailable");
                            byte[] result_fromPort = new byte[1024];
                            int bufferSize = await networkStream.ReadAsync(result_fromPort, 0, result_fromPort.Length);
                            if (bufferSize <= 0)
                                break;
                            byte[] byteRead = result_fromPort.Take(bufferSize).ToArray();
                            //logger.Trace("DataAvailable:{0}",byteHelper.PrintByteArrayX(byteRead));
                            if ((byteRead.Length == 1) && (byteRead[0] == (byte)WorkByte.NAK))
                            {
                                //logger.Trace("NAK={0}", byteRead[0]);
                                Thread.Sleep(100);
                                await networkStream.FlushAsync();
                                goto Begin;
                            }
                            int count_wait = 0;
                            for (int tByte = 0; tByte < bufferSize; tByte++)
                            {
                                if ((result_fromPort[tByte] == (byte)WorkByte.ACK) || (result_fromPort[tByte] == (byte)WorkByte.SYN))
                                {
                                    count_wait++;
                                }
                            }
                            if (bufferSize == 1 && ((result_fromPort[0] == (byte)WorkByte.ACK) || (result_fromPort[0] == (byte)WorkByte.SYN)))
                            {
                                Thread.Sleep(200);
                                //logger.Trace("byte read:{0}, sleep:{1}", result_fromPort[0], 200);
                            }
                            else if ((bufferSize < 10) || (bufferSize == count_wait) || ((count_wait > 0) && (bufferSize / count_wait < 2)))
                            {
                                twait++;
                                Thread.Sleep(twait * 400);
                                //logger.Trace("bytes read:{0}, sleep:{1}",  byteHelper.PrintByteArrayX(byteRead), 400);
                            }
                            if (twait > 10) break;
                            result = byteHelper.Combine(result, byteRead);
                        };

                        int psPacketBegin = byteHelper.ByteSearch(result, BytesBegin);
                        int psPacketEnd = byteHelper.ByteSearch(result, bytesEnd, psPacketBegin);
                        if ((result.Length>9) && (psPacketBegin > 0) && (psPacketEnd > 0))
                        {
                            //logger.Trace("Good read");
                            break;
                        }
                        if ((x > 5) && (result.Length < 10))
                        {
                            Thread.Sleep(100);
                            await networkStream.FlushAsync();
                            goto Begin;
                        }
                    }

                    if (result.Length<9)
                    {
                        setError("Не полный ответ сервера");
                        throw new ArgumentException(this.errorInfo);
                    }
                    //networkStream.Close();
                    //client.Close();

                    //logger.Warn("full bytes read:{0}", byteHelper.PrintByteArrayX(result));
                    int positionPacketBegin = byteHelper.ByteSearch(result, BytesBegin);

                    if (positionPacketBegin < 0)
                    {
                        if (taskTry < 10)
                        {
                            setError("В байтах ответа по операции " + inputbyte[2].ToString() + " не найдено \"начало\", ip:" + this.IpAdress + ":" + port.ToString() + " ответ:" + byteHelper.PrintByteArrayX(result));
                            logger.Trace("Повторяем отправку байт");
                            Thread.Sleep(100);
                            await networkStream.FlushAsync();
                            goto Begin;
                        }
                        setError("В байтах ответа по операции " + inputbyte[2].ToString() + " не найдено \"начало\", ip:" + this.IpAdress + ":" + port.ToString() + " ответ:" + byteHelper.PrintByteArrayX(result));
                        throw new ArgumentException(this.errorInfo);
                        // return null;
                    }
                    //}
                    positionPacketEnd = -1;
                    tCurrentPos = positionPacketBegin + 7;
                    //tPostEnd = -1;
                    //do
                    //{
                    //    tCurrentPos++;
                    //    tPostEnd = byteHelper.ByteSearch(result, bytesEnd, tCurrentPos);
                    //    if (tPostEnd != -1)
                    //    {
                    //        tCurrentPos = tPostEnd;

                    //        if (result[tPostEnd - 1] != (byte)WorkByte.DLE)
                    //        {
                    //            positionPacketEnd = tPostEnd;
                    //            break;
                    //        }
                    //        else if ((result[tPostEnd - 1] == (byte)WorkByte.DLE) && (result[tPostEnd - 2] == (byte)WorkByte.DLE))
                    //        {
                    //            positionPacketEnd = tPostEnd;
                    //            // break; 
                    //        }
                    //    }
                    //} while (tCurrentPos < result.Length);
                    for (int curPos = result.Length - 1; curPos > tCurrentPos; curPos--)
                    {
                        if ((result[curPos] == (byte)WorkByte.ETX) && (result[curPos - 1] == (byte)WorkByte.DLE))
                        {
                            positionPacketEnd = curPos - 1;
                            break;
                        }
                    }

                    if (positionPacketEnd < 0)
                    {
                        setError("В байтах ответа по операции " + inputbyte[2].ToString() + " не найдено \"конец\", ip:" + this.IpAdress + ":" + port.ToString() + " ответ:" + byteHelper.PrintByteArrayX(result));
                        throw new ArgumentException(this.errorInfo);
                    }
                    //e  } while (base.BytesToRead>0);


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

                    byte byteCheckSum = unsigned[unsigned.Length - 3];
                    unsigned[unsigned.Length - 3] = 0;

                    if (byteCheckSum != byteHelper.getchecksum(unsigned))
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("Не правильная check сумма {2}!={3} обмена, ip:{0}:{1}", this.IpAdress, port, byteCheckSum, byteHelper.getchecksum(unsigned));
                        this.statusOperation = false;
                        setError(sb.ToString());
                        await networkStream.FlushAsync();
                        goto Begin;
                        //не совпала чек сумма


                        //throw new ArgumentException(this.errorInfo);
                    }
                    //logger.Warn("End<<<<<<<<");
                    this.statusOperation = true;
                    this.ByteStatus = unsigned[4];
                    this.ByteResult = unsigned[5];
                    this.ByteReserv = unsigned[6];
                    this.errorInfo = "";
                }
            }
            //Console.WriteLine(PrintByteArray(unsigned));
            //Console.WriteLine(PrintByteArray(unsigned.Skip(8).Take(unsigned.Length - 7 - 3 - ((useCRC16) ? 2 : 0)).ToArray()));
            return unsigned;//.Skip(8).Take(unsigned.Length-7-3- ((useCRC16)?2:0) ).ToArray();
        }


        
        /// <summary>
        /// Если что то начинает не работать рекомендую заменить async на этот метод
        /// </summary>
        /// <param name="inputbyte"></param>
        /// <param name="useCRC16"></param>
        /// <param name="repeatError"></param>
        /// <returns></returns>
        private ReturnedStruct ExchangeFPW(byte[] inputbyte, bool useCRC16 = false, bool repeatError = false)
        {
            ReturnedStruct rRs = new ReturnedStruct() {
                ByteStatus = 0,
                ByteResult = 0,
                ByteReserv = 0,
                statusOperation=false,
                errorInfo = "",
                fullBytesSend = inputbyte,
                command = inputbyte[0]
            };
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
            int taskTry = -1;
            //if (!base.IsOpen)
            //    base.Open();
            //if (!base.IsOpen)
            //{
            //    setError("Не возможно подключиться к порту:" + base.PortName.ToString());
            //    throw new ArgumentException(this.errorInfo);
            //}
            //logger.Trace("Begin>>>>");
            if (!PingHost(IpAdress, port))
            {
                setError("Ошибка подключения к серверу ip:" + this.IpAdress + ":" + port.ToString());
                rRs.ByteStatus = ByteStatus;
                rRs.ByteResult = ByteResult;
                rRs.ByteReserv = ByteReserv;
                rRs.statusOperation = false;
                rRs.errorInfo += errorInfo + "; ";
                return rRs;
                //throw new ApplicationException("Ошибка подключения к серверу");
            }
            byte[] unsigned = null;
            using (TcpClient client = new TcpClient())
            {

                client.Connect(IPAddress.Parse(IpAdress), port);
                
                client.ReceiveTimeout = 5000;
                client.SendTimeout = 5000;
#if Debug
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("подготовка к отправке:{0}", byteHelper.PrintByteArrayX(inputbyte));
#endif
                using (var networkStream = client.GetStream())
                {
                    networkStream.WriteTimeout = 5000;
                    networkStream.ReadTimeout = 5000;
                Begin:
                    if (taskTry > 5)
                    {
                        string sf = String.Format("Выполнено {0} циклов, ответа нет {1}:{2}", this.IpAdress, port.ToString());
                        setError(sf);
                        logger.Trace(sf);
                        rRs.ByteStatus = ByteStatus;
                        rRs.ByteResult = ByteResult;
                        rRs.ByteReserv = ByteReserv;
                        rRs.statusOperation = false;
                        rRs.errorInfo += errorInfo + "; ";
                        return rRs;
                        //throw new ApplicationException(this.errorInfo);
                    }
                    taskTry++;
                    //logger.Trace("try{1}, send to FP:{0}", byteHelper.PrintByteArrayX(inputbyte), taskTry);
                    networkStream.Write(inputbyte, 0, inputbyte.Length);
                    networkStream.Flush();
                    //base.Write(inputbyte, 0, inputbyte.Length);

#if Debug
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("отправлено");
#endif
                    byte[] BytesBegin = new byte[4];
                    Buffer.BlockCopy(inputbyte, 0, BytesBegin, 0, 4);

                    int positionPacketEnd = -1;
                    int tCurrentPos = 7;
                    //int tPostEnd = -1;



                    byte[] result = new byte[] { };

                    for (int x = 1; x < 5; x++)
                    {
                        int coef = x;
                        if ((inputbyte[2] == 13) || (inputbyte[2] == 9)) // Если отчеты то ждем 2 раза дольше
                        {
                            coef = x * 2;


                        }
                        Thread.Sleep(coef * this.waiting);
                        //logger.Trace("ожидание в цикле {1} = {0}", coef * this.waiting,x);


                        int twait = 0;
                        while (networkStream.DataAvailable)
                        {
                            //logger.Trace("DataAvailable");
                            byte[] result_fromPort = new byte[1024];
                            int bufferSize = networkStream.Read(result_fromPort, 0, result_fromPort.Length);
                            if (bufferSize <= 0)
                                break;
                            byte[] byteRead = result_fromPort.Take(bufferSize).ToArray();
                            //logger.Trace("DataAvailable:{0}",byteHelper.PrintByteArrayX(byteRead));
                            if ((byteRead.Length == 1) && (byteRead[0] == (byte)WorkByte.NAK))
                            {
                                //logger.Trace("NAK={0}", byteRead[0]);
                                Thread.Sleep(100);
                                networkStream.Flush();
                                goto Begin;
                            }
                            int count_wait = 0;
                            for (int tByte = 0; tByte < bufferSize; tByte++)
                            {
                                if ((result_fromPort[tByte] == (byte)WorkByte.ACK) || (result_fromPort[tByte] == (byte)WorkByte.SYN))
                                {
                                    count_wait++;
                                }
                            }
                            if (bufferSize == 1 && ((result_fromPort[0] == (byte)WorkByte.ACK) || (result_fromPort[0] == (byte)WorkByte.SYN)))
                            {
                                Thread.Sleep(200);
                                //logger.Trace("byte read:{0}, sleep:{1}", result_fromPort[0], 200);
                            }
                            else if ((bufferSize < 10) || (bufferSize == count_wait) || ((count_wait > 0) && (bufferSize / count_wait < 2)))
                            {
                                twait++;
                                Thread.Sleep(twait * 400);
                                //logger.Trace("bytes read:{0}, sleep:{1}",  byteHelper.PrintByteArrayX(byteRead), 400);
                            }
                            if (twait > 10) break;
                            result = byteHelper.Combine(result, byteRead);
                        };

                        int psPacketBegin = byteHelper.ByteSearch(result, BytesBegin);
                        int psPacketEnd = byteHelper.ByteSearch(result, bytesEnd, psPacketBegin);
                        if ((result.Length > 9) && (psPacketBegin > 0) && (psPacketEnd > 0))
                        {
                            //logger.Trace("Good read");
                            break;
                        }
                        if ((x > 5) && (result.Length < 10))
                        {
                            Thread.Sleep(100);
                            networkStream.Flush();
                            goto Begin;
                        }
                    }

                    if (result.Length < 9)
                    {
                        setError("Не полный ответ сервера");
                        rRs.ByteStatus = ByteStatus;
                        rRs.ByteResult = ByteResult;
                        rRs.ByteReserv = ByteReserv;
                        rRs.statusOperation = false;
                        rRs.bytesReturn = result;
                        rRs.errorInfo += errorInfo + "; ";
                        return rRs;
                        //throw new ArgumentException(this.errorInfo);
                    }
                    //networkStream.Close();
                    //client.Close();

                    //logger.Warn("full bytes read:{0}", byteHelper.PrintByteArrayX(result));
                    int positionPacketBegin = byteHelper.ByteSearch(result, BytesBegin);

                    if (positionPacketBegin < 0)
                    {
                        if (taskTry < 10)
                        {
                            setError("В байтах ответа по операции " + inputbyte[2].ToString() + " не найдено \"начало\", ip:" + this.IpAdress + ":" + port.ToString() + " ответ:" + byteHelper.PrintByteArrayX(result));
                            logger.Trace("Повторяем отправку байт");
                            Thread.Sleep(100);
                            networkStream.Flush();
                            goto Begin;
                        }
                        setError("В байтах ответа по операции " + inputbyte[2].ToString() + " не найдено \"начало\", ip:" + this.IpAdress + ":" + port.ToString() + " ответ:" + byteHelper.PrintByteArrayX(result));
                        rRs.ByteStatus = ByteStatus;
                        rRs.ByteResult = ByteResult;
                        rRs.ByteReserv = ByteReserv;
                        rRs.statusOperation = false;
                        rRs.bytesReturn = result;
                        rRs.errorInfo += errorInfo + "; ";
                        return rRs;
                        //throw new ArgumentException(this.errorInfo);
                        // return null;
                    }
                    //}
                    positionPacketEnd = -1;
                    tCurrentPos = positionPacketBegin + 7;
                    //tPostEnd = -1;
                    //do
                    //{
                    //    tCurrentPos++;
                    //    tPostEnd = byteHelper.ByteSearch(result, bytesEnd, tCurrentPos);
                    //    if (tPostEnd != -1)
                    //    {
                    //        tCurrentPos = tPostEnd;

                    //        if (result[tPostEnd - 1] != (byte)WorkByte.DLE)
                    //        {
                    //            positionPacketEnd = tPostEnd;
                    //            break;
                    //        }
                    //        else if ((result[tPostEnd - 1] == (byte)WorkByte.DLE) && (result[tPostEnd - 2] == (byte)WorkByte.DLE))
                    //        {
                    //            positionPacketEnd = tPostEnd;
                    //            // break; 
                    //        }
                    //    }
                    //} while (tCurrentPos < result.Length);
                    for (int curPos = result.Length - 1; curPos > tCurrentPos; curPos--)
                    {
                        if ((result[curPos] == (byte)WorkByte.ETX) && (result[curPos - 1] == (byte)WorkByte.DLE))
                        {
                            positionPacketEnd = curPos - 1;
                            break;
                        }
                    }

                    if (positionPacketEnd < 0)
                    {
                        setError("В байтах ответа по операции " + inputbyte[2].ToString() + " не найдено \"конец\", ip:" + this.IpAdress + ":" + port.ToString() + " ответ:" + byteHelper.PrintByteArrayX(result));
                        rRs.ByteStatus = ByteStatus;
                        rRs.ByteResult = ByteResult;
                        rRs.ByteReserv = ByteReserv;
                        rRs.statusOperation = false;
                        rRs.errorInfo += errorInfo + "; ";
                        rRs.bytesReturn = result;
                        return rRs;
                        //throw new ArgumentException(this.errorInfo);
                    }
                    //e  } while (base.BytesToRead>0);


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

                    byte byteCheckSum = unsigned[unsigned.Length - 3];
                    unsigned[unsigned.Length - 3] = 0;

                    if (byteCheckSum != byteHelper.getchecksum(unsigned))
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("Не правильная check сумма {2}!={3} обмена, ip:{0}:{1}", this.IpAdress, port, byteCheckSum, byteHelper.getchecksum(unsigned));
                        this.statusOperation = false;
                        setError(sb.ToString());
                        networkStream.Flush();
                        goto Begin;
                        //не совпала чек сумма


                        //throw new ArgumentException(this.errorInfo);
                    }
                    //logger.Warn("End<<<<<<<<");
                    this.statusOperation = true;
                    this.ByteStatus = unsigned[4];
                    this.ByteResult = unsigned[5];
                    this.ByteReserv = unsigned[6];
                    //this.errorInfo = "";
                    rRs.ByteStatus = unsigned[4];
                    rRs.ByteResult = unsigned[5];
                    rRs.ByteReserv = unsigned[6];
                    rRs.bytesReturn = unsigned;
                    rRs.statusOperation = true;
                    //rRs.errorInfo += errorInfo + "; ";                    
                }
            }
            //Console.WriteLine(PrintByteArray(unsigned));
            //Console.WriteLine(PrintByteArray(unsigned.Skip(8).Take(unsigned.Length - 7 - 3 - ((useCRC16) ? 2 : 0)).ToArray()));
            return rRs;//.Skip(8).Take(unsigned.Length-7-3- ((useCRC16)?2:0) ).ToArray();
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
                this.glbytesPrepare = inByte;
                return await ExchangeFP(byteHelper.prepareForSend(ConsecutiveNumber, inByte, useCRC16, repeatError), useCRC16, repeatError);
            };

            Task<byte[]> answer = function(input);
            //byte[] answer = ExchangeFP(prepareForSend(input, useCRC16));
            //return await answer;
            try
            {
                return byteHelper.returnBytesWithoutSufixAndPrefix(answer.Result);
            }
            catch (AggregateException e)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Caught {0}, exception: {1}", e.InnerExceptions.Count, string.Join(", ", e.InnerExceptions.Select(x => x.Message)));
                setError(sb.ToString());
                throw new ArgumentException(this.errorInfo);
#if Debug
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Описание ошибки:{0}", this.errorInfo);
#endif
            }
            finally
            {

            }
            
            return null;
        }


        private void setError(string errorInfo = "Unknown error", byte ByteStatus = 255, byte ByteResult = 255, byte ByteReserv = 255)
        {
            this.ByteStatus = ByteStatus;
            this.ByteResult = ByteResult;
            this.ByteReserv = ByteReserv;
            this.statusOperation = false;
            this.errorInfo += errorInfo + "; ";
            logger.Error(errorInfo);
        }

        void IDisposable.Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
