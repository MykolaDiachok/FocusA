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
        IProtocols singletonProtocol;


        protected SingletonProtocol(int port)
        {
            
            singletonProtocol = new BaseProtocol(port).getCurrentProtocol();
        }

        public static SingletonProtocol Instance(int inport)
        {
            if ((uniqueInstance == null))
            {
                
                uniqueInstance = new SingletonProtocol(inport);                
            }
            return uniqueInstance;
        }
        

        public IProtocols GetProtocols()
        {
            return singletonProtocol;
        }
    }
}
