using System;
using CentralLib.Helper;

namespace CentralLib.Protocols
{
    public interface IProtocols
    {
        //byte ByteReserv { get; }
        //byte ByteResult { get; }
        //byte ByteStatus { get; }
        //WorkProtocol currentProtocol { get; }
        Taxes currentTaxes { get; }
        DayReport dayReport { get; }
        //string errorInfo { get; }
        DateTime fpDateTime { get; set; }
        PapStat papStat { get; }
        Status status { get; }
        //bool statusOperation { get; }
        strByteReserv structReserv { get; }
        strByteResult structResult { get; }
        strByteStatus structStatus { get; }
        
        //bool useCRC16 { get; }

        void Dispose();
        void FPArtReport(ushort pass = 0, uint? CodeBeginning = default(uint?), uint? CodeFinishing = default(uint?));
        uint FPCashIn(uint Summa);
        uint FPCashOut(uint Summa);
        void FPCommentLine(string CommentLine, bool OpenRefundReceipt = false);
        void FPNullCheck();
        void FPCplOnline();
        void FPDayClrReport(ushort pass = 0);
        void FPDayReport(ushort pass = 0);
        string FPGetPayName(byte PayType);
        void FPGetTaxRate();
        void FPLineFeed();
        void FPOpenBox(byte impulse = 0);
        PaymentInfo FPPayment(byte Payment_Status, uint Payment, bool CheckClose, bool FiscStatus, string AuthorizationCode = "");
        ReceiptInfo FPPayMoneyEx(ushort Amount, byte Amount_Status, bool IsOneQuant, int Price, ushort NalogGroup, bool MemoryGoodName, string GoodName, ulong StrCode, bool PrintingOfBarCodesOfGoods = false);
        void FPPeriodicReport(ushort pass, DateTime FirstDay, DateTime LastDay);
        void FPPeriodicReport2(ushort pass, ushort FirstNumber, ushort LastNumber);
        void FPPeriodicReportShort(ushort pass, DateTime FirstDay, DateTime LastDay);
        void FPPrintVer();
        uint FPPrintZeroReceipt();

        /// <summary>
        /// Код: 6.SetCashier               регистрация кассира (оператора)  в ЭККР
        /// После инициализации ЭККР значения паролей равны нулю (0). При длине имени 0 –  разрегистрация  
        /// кассира.Количество вводов пароля не более 10.
        /// </summary>
        /// <param name="CashierID">Номер</param>
        /// <param name="Name">Длина имени кассира (= n)0..15</param>
        /// <param name="Password">Пароль</param>
        void FPRegisterCashier(byte CashierID, string Name, ushort Password = 0);
        void FPResetOrder();
        ReceiptInfo FPSaleEx(ushort Amount, byte Amount_Status, bool IsOneQuant, int Price, ushort NalogGroup, bool MemoryGoodName, string GoodName, ulong StrCode, bool PrintingOfBarCodesOfGoods = false);
        void FPSetHeadLine(ushort Password, string StringInfo1, bool StringInfo1DoubleHeight, bool StringInfo1DoubleWidth, string StringInfo2, bool StringInfo2DoubleHeight, bool StringInfo2DoubleWidth, string StringInfo3, bool StringInfo3DoubleHeight, bool StringInfo3DoubleWidth, string TaxNumber, bool AddTaxInfo);
        void FPSetPassword(byte UserID, ushort OldPassword, ushort NewPassword);
        void FPSetTaxRate(ushort Password, Taxes tTaxes);
        bool showBottomString(string Info);
        bool showTopString(string Info);

        /// <summary>
        /// Код: 33.
        /// GetBox                          сумма наличных в денежном ящике
        /// </summary>
        /// <returns></returns>
        UInt32 GetMoneyInBox();
        /// <summary>
        /// ///Код: 46.  
        /// CplCutter запрет/разрешение на использование обрезчика
        ///Вызов команды меняет значение параметра на противоположный.
        /// </summary>
        /// <returns></returns>
        bool FPCplCutter();
    }
}