using NLog;
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
        public static Logger logger = LogManager.GetCurrentClassLogger();


        protected SingletonProtocol(int port, Int64 inFPNumber)
        {
            NLog.GlobalDiagnosticsContext.Set("FPNumber", inFPNumber);
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            singletonProtocol = new BaseProtocol(port, inFPNumber).getCurrentProtocol();
        }

        protected SingletonProtocol(string IpAdress, int port, Int64 inFPNumber)
        {
            NLog.GlobalDiagnosticsContext.Set("FPNumber", inFPNumber);
            logger.Trace(this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            singletonProtocol = new BaseProtocol(IpAdress, port, inFPNumber).getCurrentProtocol();
        }

        /// <summary>
        /// инициализация протокола через ком порт
        /// </summary>
        /// <param name="inport"></param>
        /// <returns></returns>
        public static SingletonProtocol Instance(int inport, Int64 inFPNumber)
        {
            NLog.GlobalDiagnosticsContext.Set("FPNumber", inFPNumber);
            logger.Trace("SingletonProtocol." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            if ((uniqueInstance == null))
            {

                uniqueInstance = new SingletonProtocol(inport, inFPNumber);
            }
            return uniqueInstance;
        }

        /// <summary>
        /// Инициализация протокола через ip
        /// </summary>
        /// <param name="IpAdress"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static SingletonProtocol Instance(string IpAdress, int port, Int64 inFPNumber)
        {
            NLog.GlobalDiagnosticsContext.Set("FPNumber", inFPNumber);
            logger.Trace("SingletonProtocol." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            if ((uniqueInstance == null))
            {

                uniqueInstance = new SingletonProtocol(IpAdress, port, inFPNumber);
            }
            return uniqueInstance;
        }

        public BaseProtocol GetProtocols()
        {
            return singletonProtocol;
        }
    }
}
