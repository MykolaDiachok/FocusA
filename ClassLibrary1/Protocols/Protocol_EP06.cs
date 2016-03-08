using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralLib.Helper;

namespace CentralLib.Protocols
{
    class Protocol_EP06 : BaseProtocol, IProtocols
    {

        public Protocol_EP06(int serialPort):base(serialPort)
        {
            //initial();
        }

        public Protocol_EP06(CentralLib.Connections.DefaultPortCom dComPort):base(dComPort)
        {       

        }

        public Taxes currentTaxes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public DayReport dayReport
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        

        public DateTime fpDateTime
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public PapStat papStat
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Status status
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        public void FPArtReport(ushort pass = 0, uint? CodeBeginning = default(uint?), uint? CodeFinishing = default(uint?))
        {
            throw new NotImplementedException();
        }

        public uint FPCashIn(uint Summa)
        {
            throw new NotImplementedException();
        }

        public uint FPCashOut(uint Summa)
        {
            throw new NotImplementedException();
        }

        public void FPCommentLine(string CommentLine, bool OpenRefundReceipt = false)
        {
            throw new NotImplementedException();
        }

        public void FPCplOnline()
        {
            throw new NotImplementedException();
        }

        public void FPDayClrReport(ushort pass = 0)
        {
            throw new NotImplementedException();
        }

        public void FPDayReport(ushort pass = 0)
        {
            throw new NotImplementedException();
        }

        public string FPGetPayName(byte PayType)
        {
            throw new NotImplementedException();
        }

        public void FPGetTaxRate()
        {
            throw new NotImplementedException();
        }

        public void FPLineFeed()
        {
            throw new NotImplementedException();
        }

        public void FPOpenBox(byte impulse = 0)
        {
            throw new NotImplementedException();
        }

        public PaymentInfo FPPayment(byte Payment_Status, uint Payment, bool CheckClose, bool FiscStatus, string AuthorizationCode = "")
        {
            throw new NotImplementedException();
        }

        public ReceiptInfo FPPayMoneyEx(ushort Amount, byte Amount_Status, bool IsOneQuant, int Price, ushort NalogGroup, bool MemoryGoodName, string GoodName, ulong StrCode, bool PrintingOfBarCodesOfGoods = false)
        {
            throw new NotImplementedException();
        }

        public void FPPeriodicReport(ushort pass, DateTime FirstDay, DateTime LastDay)
        {
            throw new NotImplementedException();
        }

        public void FPPeriodicReport2(ushort pass, ushort FirstNumber, ushort LastNumber)
        {
            throw new NotImplementedException();
        }

        public void FPPeriodicReportShort(ushort pass, DateTime FirstDay, DateTime LastDay)
        {
            throw new NotImplementedException();
        }

        public void FPPrintVer()
        {
            throw new NotImplementedException();
        }

        public uint FPPrintZeroReceipt()
        {
            throw new NotImplementedException();
        }

        public void FPRegisterCashier(byte CashierID, string Name, ushort Password = 0)
        {
            throw new NotImplementedException();
        }

        public void FPResetOrder()
        {
            throw new NotImplementedException();
        }

        public ReceiptInfo FPSaleEx(ushort Amount, byte Amount_Status, bool IsOneQuant, int Price, ushort NalogGroup, bool MemoryGoodName, string GoodName, ulong StrCode, bool PrintingOfBarCodesOfGoods = false)
        {
            throw new NotImplementedException();
        }

        public void FPSetHeadLine(ushort Password, string StringInfo1, bool StringInfo1DoubleHeight, bool StringInfo1DoubleWidth, string StringInfo2, bool StringInfo2DoubleHeight, bool StringInfo2DoubleWidth, string StringInfo3, bool StringInfo3DoubleHeight, bool StringInfo3DoubleWidth, string TaxNumber, bool AddTaxInfo)
        {
            throw new NotImplementedException();
        }

        public void FPSetPassword(byte UserID, ushort OldPassword, ushort NewPassword)
        {
            throw new NotImplementedException();
        }

        public void FPSetTaxRate(ushort Password, Taxes tTaxes)
        {
            throw new NotImplementedException();
        }

        public bool showBottomString(string Info)
        {
            throw new NotImplementedException();
        }

        public bool showTopString(string Info)
        {
            throw new NotImplementedException();
        }
    }
}
