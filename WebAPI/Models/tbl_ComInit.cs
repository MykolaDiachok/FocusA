//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebAPI.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbl_ComInit
    {
        public long id { get; set; }
        public string CompName { get; set; }
        public int Port { get; set; }
        public bool Init { get; set; }
        public bool Error { get; set; }
        public Nullable<bool> WorkOff { get; set; }
        public Nullable<bool> auto { get; set; }
        public Nullable<int> ErrorCode { get; set; }
        public string ErrorInfo { get; set; }
        public Nullable<int> FPNumber { get; set; }
        public string RealNumber { get; set; }
        public string SerialNumber { get; set; }
        public string FiscalNumber { get; set; }
        public Nullable<long> DateTimeBegin { get; set; }
        public Nullable<long> DateTimeStop { get; set; }
        public Nullable<long> DeltaTime { get; set; }
        public string CurrentDate { get; set; }
        public string CurrentTime { get; set; }
        public Nullable<System.DateTime> CurrentSystemDateTime { get; set; }
        public string Version { get; set; }
        public Nullable<bool> SmenaOpened { get; set; }
        public string PapStat { get; set; }
        public Nullable<int> ByteStatus { get; set; }
        public string ByteStatusInfo { get; set; }
        public Nullable<int> ByteResult { get; set; }
        public string ByteResultInfo { get; set; }
        public Nullable<int> ByteReserv { get; set; }
        public string ByteReservInfo { get; set; }
        public string DataServer { get; set; }
        public string DataBaseName { get; set; }
        public Nullable<int> MinSumm { get; set; }
        public Nullable<int> MaxSumm { get; set; }
        public Nullable<bool> TypeEvery { get; set; }
        public Nullable<int> PrintEvery { get; set; }
        public Nullable<int> KlefMem { get; set; }
        public Nullable<System.DateTime> DateTimeSyncFP { get; set; }
        public Nullable<System.DateTime> DateTimeSyncDB { get; set; }
        public string MoxaIP { get; set; }
        public Nullable<int> MoxaPort { get; set; }
    }
}
