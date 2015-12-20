using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralLib.ConnectionFP;
using CentralLib.DefaultPortCom;


namespace TestComm
{
    class Program
    {
        static void Main(string[] args)
        {
            DefaultPortCom initialPort = new DefaultPortCom(4);
            ConnectionFP connFP = new ConnectionFP(initialPort);
            connFP.Open();
            //Provision proConn = new Provision(connFP);


            connFP.WriteAsync(new byte[] { 16, 2, 0, 27, 1, 1, 97, 130, 16, 3, 28, 170 });
            
            //proConn.ExchangeData(new byte[] { 16, 2, 0, 27, 1, 1, 97, 130, 16, 3, 28, 170 });
            Console.ReadKey();
            
            //proConn.Dispose();
            connFP.Close();
            
        }
     
    }
}
