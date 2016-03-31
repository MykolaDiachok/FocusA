using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintFP.Primary
{
    public class Art
    {
        public int Code {get;}
        public string ARTNAME { get; private set; }
        public ulong PackCode { get; set; }
        /// <summary>
        /// not use
        /// </summary>
        public Guid PackGuid { get;  }
        public ushort NalogGroup { get; private set; }
        public string NameForCheck { get;  }
        public int FPNumber { get;  }
        private DataClasses1DataContext _focusA;

        public Art(int Code, string ARTNAME, ulong PackCode, ushort NalogGroup, int FPNumber, DataClasses1DataContext _focusA)
        {
            this.Code = Code;
            this.ARTNAME = ARTNAME;
            this.PackCode = PackCode;
            //this.PackGuid = PackGuid;
            this.NalogGroup = NalogGroup;
            this.NameForCheck = ARTNAME;
            this.FPNumber = FPNumber;
            this._focusA = _focusA;
            InsertToTableAndUpdateName();
        }

        public void InsertToTableAndUpdateName()
        {
            Table<tbl_ART> tbl_ART = _focusA.GetTable<tbl_ART>();
            var rowArt = (from tArt in tbl_ART
                          where (ulong)tArt.PackCode == PackCode
                          && tArt.FPNumber == FPNumber
                          select tArt).FirstOrDefault();
            if (rowArt == null)
            {
                tbl_ART newArt = new tbl_ART
                {
                    Code = Code,
                    ARTNAME = ARTNAME,
                    PackCode = (int)PackCode,
                    //PackGuid = rowart.PackGuid,
                    NalogGroup = NalogGroup,
                    NameForCheck = NameForCheck,
                    FPNumber = FPNumber
                };
                _focusA.tbl_ARTs.InsertOnSubmit(newArt);
                _focusA.SubmitChanges();
            }
            else
            {
                this.ARTNAME = rowArt.ARTNAME;
                this.NalogGroup = (ushort)rowArt.NalogGroup;
            }
        }
}
}
