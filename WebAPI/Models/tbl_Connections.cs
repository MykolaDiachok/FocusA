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
    
    public partial class tbl_Connections
    {
        public long id { get; set; }
        public string GroupName { get; set; }
        public int FPNumber { get; set; }
        public string DataServer { get; set; }
        public string DataServerIP { get; set; }
        public string DataBaseName { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
    }
}
