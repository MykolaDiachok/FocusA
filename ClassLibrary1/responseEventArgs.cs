using System;

namespace CentralLib.ConnectionFP
{
    public class responseEventArgs : EventArgs
    {
        byte[] bytesResponse;
        public byte[] BytesResponse { get { return bytesResponse; } }

        public responseEventArgs(byte[] bytesResponse) 
        {
            this.bytesResponse = bytesResponse;
        }
    }
}