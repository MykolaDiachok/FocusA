using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace CentralLib.ConnectionFP
{
    public class ConnectionFP : SerialPort,INotifyPropertyChanged

    {
        const byte DLE = 0x10;
        const byte STX = 0x02;
        const byte ETX = 0x03;
        const byte ACK = 0x06;
        const byte NAK = 0x15;
        const byte SYN = 0x16;
        const byte ENQ = 0x05;

        private byte[] bytesBegin = { DLE, STX };
        private byte[] bytesEnd = { DLE, ETX };
        public byte[] bytesForSend { get; protected set; }
        private byte[] bytesOutput;
        public byte[] bytesResponse
        {
            get { return bytesOutput; }
            set
            {
                if (value != bytesResponse)
                {
                    OnResponseChange(bytesResponse);
                }
            }
        }


        private void OnResponseChange(byte[] bytesResponse)
        {
            
            if (responseLevelChanged != null)
                responseLevelChanged(new responseEventArgs(bytesResponse)); 
        }

        private byte[] bytesBuffered;


        public DateTimeOffset timeSend { get; protected set; }
        public DateTimeOffset timeGet { get; protected set; }

        public delegate void responseEventHandler(responseEventArgs arg);
        public event responseEventHandler responseLevelChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public ConnectionFP(DefaultPortCom.DefaultPortCom defPortCom):base()
        {
            base.PortName = defPortCom.sPortNumber;
            base.BaudRate = defPortCom.baudRate;
            base.Parity = defPortCom.parity;
            base.DataBits = defPortCom.dataBits;
            base.StopBits = defPortCom.stopBits;
            base.WriteTimeout = defPortCom.writeTimeOut;
            base.ReadTimeout = defPortCom.readTimeOut;
            base.DataReceived += ConnectionFP_DataReceived;            
        }

        private async void ConnectionFP_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var port = (SerialPort)sender;
            if ((port != null) && (port.IsOpen))
            {
                
                int buferSize = port.BytesToRead;
                byte[] result = new byte[buferSize];
                int read = await base.BaseStream.ReadAsync(result, 0, buferSize);

                bytesBuffered = Combine(bytesBuffered, result);

                int positionPacketBegin = ByteSearch(bytesBuffered, bytesBegin) -1;
                if (positionPacketBegin < 0)
                {
                    Thread.Sleep(40);                  
                    return; // waiting begin string
                }
                int positionPacketEnd = 0;
                int tCurrentPos = positionPacketBegin;
                int tPostEnd = -1;
                do
                {
                    tCurrentPos++;
                    tPostEnd = ByteSearch(bytesBuffered, bytesEnd, tCurrentPos);
                    if (tPostEnd != -1)
                    {
                        tCurrentPos = tPostEnd;

                        if (bytesBuffered[tPostEnd - 1] != DLE)
                        {
                            positionPacketEnd = tPostEnd;
                            break;
                        }
                        else if ((bytesBuffered[tPostEnd - 1] == DLE) && (bytesBuffered[tPostEnd - 2] == DLE))
                        {
                            positionPacketEnd = tPostEnd;
                            // break; 
                        }
                    }
                } while (tCurrentPos < bytesBuffered.Length);
                if (positionPacketEnd < 0)
                {
                    Thread.Sleep(40);                   
                    return; //waiting end postion
                }
                byte[] unsigned = new byte[positionPacketEnd - positionPacketBegin + 4];
                Buffer.BlockCopy(bytesBuffered, positionPacketBegin, unsigned, 0, positionPacketEnd - positionPacketBegin + 4);
                this.bytesOutput = unsigned;
                this.bytesResponse = unsigned;
                this.timeGet = DateTimeOffset.UtcNow;


            }
            //
            //var searchEnd = PatternAt(result, byteend);
            
        }

        byte[] returnWithOutDublicateDLE(byte[] source)
        {
            return returnWithOutDublicate(source, new byte[] { DLE, DLE });
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


        public new void Open()
        {
            if (base.IsOpen)
            {
                base.Close();
            }
            base.Open();
        }


        public async Task WriteAsync(byte[] content)
        {
            this.bytesBuffered = new byte[] { };
            this.bytesForSend = new byte[] { };
            this.bytesOutput = new byte[] { };
            byte[] buffer = new byte[1024];
            var toWriteLength = content.Length;
            var offset = 0;
            this.bytesForSend = content;
            while (offset < toWriteLength)
            {
                var currentLength = Math.Min(toWriteLength - offset, 1024);

                Buffer.BlockCopy(content, offset, buffer, 0, currentLength);

                await base.BaseStream.WriteAsync(buffer, 0, currentLength);
                //_port.Write(content, 0, content.Length);

                offset = offset + currentLength;

                //System.Threading.Thread.Sleep(100); // sleep 100 ms
            }
            this.timeSend = DateTimeOffset.UtcNow;
            this.timeGet = new DateTimeOffset();
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


        //private async void ReadLoop()
        //{
        //    byte[] allresult = new byte[1024];
        //    while (_port != null)
        //    {
        //        Thread.Sleep(40);                
        //        try
        //        {
        //            byte[] result = new byte[_port.BytesToRead];
        //            int read = await _port.BaseStream.ReadAsync(result, 0, result.Length);
        //            allresult = Combine(allresult, result);
        //            var searchBegin = PatternAt(allresult, bytebegin);


        //            var searchEnd = PatternAt(allresult, byteend);


        //            if ((searchBegin != null)&& (searchEnd != null))
        //            {
        //                DataRead(this, new SerialReadEventArgs(allresult.Skip((int)searchBegin).Take(read).ToArray()));
        //            }
        //        }
        //        catch (IOException ex)
        //        {
        //            if (PortError != null)
        //            {
        //                PortError(this, new SerialErrorEventArgs(ex));
        //            }
        //        }
        //        Thread.Sleep(40);
        //    }
        //}
    }
}
