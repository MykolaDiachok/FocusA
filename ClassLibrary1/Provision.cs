using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkComPort.ConnectionFP
{
    public class Provision : IDisposable
    {
        private ConnectionFP _serial;
        private Queue<byte[]> _recvBuffer;

        public Provision(ConnectionFP connection)
        {
            _recvBuffer = new Queue<byte[]>();
            _serial = connection;
            _serial.DataRead += ReceiveData;
        }

        public async void ExchangeData(byte[] outputbyte)
        {
            await _serial.Write(outputbyte);
            
        }


        private void ReceiveData(object sender, ConnectionFP.SerialReadEventArgs e)
        {
            _recvBuffer.Enqueue(e.ReadBytes);
        }

        public void Dispose()
        {
            if (_serial != null)
            {
                _serial.DataRead -= ReceiveData;
            }
        }


    }
}
