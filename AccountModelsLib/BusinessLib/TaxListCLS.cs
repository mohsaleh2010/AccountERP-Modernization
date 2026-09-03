namespace AccountModelsLib
{
    using AccountModelsLib.Models;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    public class TaxListCLS
    {
        public AccountTaxInfo accTaxCLS = new AccountTaxInfo();
        public DataTable Get_TaxInfo(ref operations opp)
        {
            opp = new operations();
            string mysql = "Select * from AccountTaxInfo Order By TaxName_Ar ";
            return opp.return_DataTableWithBuilder(mysql);
        }
        public AccountTaxInfo Get_TaxInfoRatio(ref operations opp,string MainCode, string SubCode)
        {
            accTaxCLS = new AccountTaxInfo();
            accTaxCLS=opp.db.AccountTaxInfo.Where(a=>a.MainCode== MainCode & a.SubCode==SubCode).FirstOrDefault();
            return accTaxCLS;
        }
        public List<AccountTaxInfo> Get_TaxInfoRatioList(ref operations opp )
        {
            List<AccountTaxInfo> accTaxList = new   List<AccountTaxInfo> ();
            accTaxList = opp.db.AccountTaxInfo.AsNoTracking().ToList();
            return accTaxList;
        }
        public decimal Get_TaxInfo(int AccountTaxInfo_Id)
        {
            string mysql = "Select * from AccountTaxInfo Where Id=" + AccountTaxInfo_Id.ToString();
            decimal num = 0M;
            DataTable table = new operations().return_DataTableWithBuilder(mysql);
            if (table.Rows.Count != 1)
            {
                AlterDB_CLS.Set_TaxInfo();
                this.Get_TaxInfo(AccountTaxInfo_Id);
            }
            else
            {
                try
                {
                    num = decimal.Parse(table.Rows[0]["TaxRate"].ToString());
                }
                catch
                {
                    AlterDB_CLS.Set_TaxInfo();
                    this.Get_TaxInfo(AccountTaxInfo_Id);
                }
            }
            return num;
        }
    }
}

