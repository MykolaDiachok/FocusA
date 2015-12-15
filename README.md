# FocusA
First

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
            byte DLE = 0x10;
            byte STX = 0x02;
            byte ETX = 0x03;
            byte[] bBytes = new byte[] {DLE, STX };
            byte[] eBytes = new byte[] {DLE, ETX };

            byte[] test1 = new byte[] { 00, 00, 16, 2, 0, 27, 1, 1, 97, 130, 16, 16, 16, 16, 3, 16, 16, 16, 3, 28, 170 };
           PrintByteArray(test1);
            int positionPacketBegin = ByteSearch(test1, new byte[] { DLE, STX });
            Console.WriteLine(positionPacketBegin);
            int positionPacketEnd = 0;
            int tCurrentPos = positionPacketBegin;
            int tPostEnd = -1;
            do
            {
                tCurrentPos++;                
                tPostEnd = ByteSearch(test1, new byte[] { 16, 3 }, tCurrentPos); 
                if (tPostEnd != -1)
                {
                    tCurrentPos = tPostEnd;
                    
                    if (test1[tPostEnd - 1] != DLE)
                    {
                        
                        break;
                    }
                    else if ((test1[tPostEnd - 1] == DLE) && (test1[tPostEnd - 2] == DLE))
                    {
                        positionPacketEnd = tPostEnd;
                       // break;
                    }                        
                }                
            } while  (tCurrentPos < test1.Length) ;
            Console.WriteLine(positionPacketEnd);
            var unsigned = new byte[positionPacketEnd- positionPacketBegin+4];
            Buffer.BlockCopy(test1, positionPacketBegin, unsigned, 0, positionPacketEnd - positionPacketBegin+4);
            PrintByteArray(unsigned);
            Console.ReadKey();
            //DefaultPortCom initialPort = new DefaultPortCom(4);
            //ConnectionFP connFP = new ConnectionFP(initialPort);
            //connFP.Open();
            //Provision proConn = new Provision(connFP);



            //proConn.ExchangeData(new byte[] { 16, 2, 0, 27, 1, 1, 97, 130, 16, 3, 28, 170 });
            //Console.ReadKey();

            //proConn.Dispose();
            //connFP.Close();

        }


        static void PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("new byte[] { ");
            foreach (var b in bytes)
            {
                sb.Append(b + ", ");
            }
            sb.Append("}");
            Console.WriteLine(sb.ToString());
        }

        static int ByteSearch(byte[] searchIn, byte[] searchBytes, int start = 0)
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


    }
}
