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

        //public EventHandler<SerialReadEventArgs> DataRead;
        //public EventHandler<SerialErrorEventArgs> PortError;
        //private SerialPort _port = null;

        //public class SerialReadEventArgs : EventArgs
        //{
        //    public SerialReadEventArgs(byte[] readBytes)
        //    {
        //        ReadBytes = readBytes;
        //    }
        //    public byte[] ReadBytes { get; private set; }
        //}
        //public class SerialErrorEventArgs : EventArgs
        //{
        //    public SerialErrorEventArgs(IOException ex)
        //    {
        //        Exception = ex;
        //    }
        //    public IOException Exception { get; private set; }
        //}

        //public static string[] GetPorts()
        //{
        //    return SerialPort.GetPortNames();
        //}

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
            //_port = new SerialPort(defPortCom.sPortNumber, defPortCom.baudRate,defPortCom.parity,defPortCom.dataBits,defPortCom.stopBits);
            //_port.WriteTimeout = defPortCom.writeTimeOut;
            //_port.ReadTimeout = defPortCom.readTimeOut;
        }

        private void ConnectionFP_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var port = (SerialPort)sender;
            try
            {
                //  узнаем сколько байт пришло
                int buferSize = port.BytesToRead;
                for (int i = 0; i < buferSize; ++i)
                {
                    //  читаем по одному байту
                    byte bt = (byte)port.ReadByte();
                    //  если встретили начало кадра (0xFF) - начинаем запись в _bufer
                    if (0xFF == bt)
                    {
                        _stepIndex = 0;
                        _startRead = true;
                        //  раскоментировать если надо сохранять этот байт
                        //_bufer[_stepIndex] = bt;
                        //++_stepIndex;
                    }
                    //  дописываем в буфер все остальное
                    if (_startRead)
                    {
                        _bufer[_stepIndex] = bt;
                        ++_stepIndex;
                    }
                    //  когда буфер наполнлся данными
                    if (_stepIndex == DataSize && _startRead)
                    {
                        //  по идее тут должны быть все ваши данные.

                        //  .. что то делаем ...
                        //  var item = _bufer[7];

                        _startRead = false;
                    }
                }
            }
            catch { }
        }
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
            byte[] buffer = new byte[64];
            var toWriteLength = content.Length;
            var offset = 0;

            while (offset < toWriteLength)
            {
                var currentLength = Math.Min(toWriteLength - offset, 64);

                Buffer.BlockCopy(content, offset, buffer, 0, currentLength);

                await base.BaseStream.WriteAsync(buffer, 0, currentLength);
                //_port.Write(content, 0, content.Length);

                offset = offset + currentLength;

                //System.Threading.Thread.Sleep(100); // sleep 100 ms
            }
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
