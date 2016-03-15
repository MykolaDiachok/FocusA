using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralLib.Protocols
{
    public class SingletonProtocol
    {
        static SingletonProtocol uniqueInstance;
        BaseProtocol singletonProtocol;


        protected SingletonProtocol(int port)
        {
            
            singletonProtocol = new BaseProtocol(port).getCurrentProtocol();
        }

        protected SingletonProtocol(string IpAdress, int port)
        {

            singletonProtocol = new BaseProtocol(IpAdress, port).getCurrentProtocol();
        }

        public static SingletonProtocol Instance(int inport)
        {
            if ((uniqueInstance == null))
            {
                
                uniqueInstance = new SingletonProtocol(inport);                
            }
            return uniqueInstance;
        }

        public static SingletonProtocol Instance(string IpAdress, int port)
        {
            if ((uniqueInstance == null))
            {

                uniqueInstance = new SingletonProtocol(IpAdress,port);
            }
            return uniqueInstance;
        }

        public BaseProtocol GetProtocols()
        {
            return singletonProtocol;
        }
    }
}
