using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace CentralLib.ConnectionFP
{
    public class ConnectionFP : SerialPort
    {
        const byte DLE = 0x10;
        const byte STX = 0x02;
        const byte ETX = 0x03;
        const byte ACK = 0x06;
        const byte NAK = 0x15;
        const byte SYN = 0x16;
        const byte ENQ = 0x05;

        private byte[] bytebegin = { DLE, STX };
        private byte[] byteend = { DLE, ETX };
        public byte[] forsend { get; protected set; }
        public byte[] output { get; protected set; }
        private byte[] buffered;
        public DateTimeOffset timeSend { get; protected set; }
        public DateTimeOffset timeGet { get; protected set; }


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
                byte[] withoutDDLE = returnWithOutDublicateDLE(result);
                var searchBegin = PatternAt(withoutDDLE, bytebegin);


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


        public void Open()
        {
            if (base.IsOpen)
            {
                base.Close();
            }
            base.Open();
        }


        public async Task WriteAsync(byte[] content)
        {
            byte[] buffer = new byte[1024];
            var toWriteLength = content.Length;
            var offset = 0;
            forsend = content;
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
