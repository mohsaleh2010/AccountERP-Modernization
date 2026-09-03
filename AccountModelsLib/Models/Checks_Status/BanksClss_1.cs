using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.EntityFrameworkCore;
namespace AccountModelsLib.Models
{
    public class BanksClss_1
    {
        public Bank bank = new Bank();
        public List<Bank> bnkList = new List<Bank>();
        AccountModelsLib.operations opp;
        public BanksClss_1(AccountModelsLib.operations opp)
        {
            this.opp = opp;
        }
        public BanksClss_1()
        {
            this.opp = new AccountModelsLib.operations();
        }
        /*
        public long Id { get; set; }
        public int Account_Type_Value { get; set; }
        public int Account_Branches_ID { get; set; }
        public int Is_Bank { get; set; }
        public int Currency_Id { get; set; }
        public DateTime FirstBalanceDate { get; set; }
        public long SubAccount_Id { get; set; }
        public int Supplier_Bank { get; set; }
        public string Account_No { get; set; }
        public string Account_Code { get; set; }
        public string Branch_Name { get; set; }
        public string Bank_Name { get; set; }
        public decimal Primary_Balance { get; set; }
        public decimal Last_Balance { get; set; }

        */
        DataTable dt = new DataTable();
        public int Is_Bank = 0;
        public void Get_Bank(ref AccountModelsLib.operations opp, long Id)
        {
            string mysql = " Select * from Banks where Id=" + Id;
            dt = opp.return_DataTable(mysql);

            bank = new Bank();
            bank = opp.db.Banks.Where(b => b.Id == Id).FirstOrDefault();
            //ReadBankData(dt, 0);


        }
        public DataTable Bank_Data(int Is_Bank_)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            return Bank_Data(ref opp, Is_Bank_);
        }
        public DataTable Bank_Data(ref AccountModelsLib.operations opp, int Is_Bank_)
        {
            Is_Bank = Is_Bank_;
            string mysql = "Select * from Banks  where Is_Bank=" + Is_Bank_;

            if (Is_Bank_ == 1)
            {
                mysql = mysql + " AND Supplier_Bank=0";
            }
            dt = opp.return_DataTable(mysql);
            return dt;
        }
        public DataTable Get_AllBanks(int Is_Bank)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            string mysql = " Select * from Banks  Where Is_Bank=" + Is_Bank;
            dt = opp.return_DataTable(mysql);
            return dt;
        }

        public void InsertBank(ref int Excp, ref AccountModelsLib.operations opp, Bank bnk)
        {
            if (Common.Use_AutoNumeric_Bank_Id_With_Submain_Account_Id == 1)
            {

                //string mysql = " INSERT INTO Banks (Account_Type_Value,Branch_Name,Account_Branches_ID,Bank_Name, Primary_balance,Last_Balance,Account_No,Is_Bank,SubAccount_Id,currency_Id,firstBalanceDate,Supplier_Bank,Account_Code) ";
                //mysql = mysql + " VALUES (" + Account_Type_Value + ",'" + Branch_Name + "'," + Account_Branches_ID + ",'" + Bank_Name + "'," + Primary_Balance + "," + Primary_Balance + ", '" + Account_No + "'," + Is_Bank + "," + SubAccount_Id + "," + Currency_Id + ",'" + AccountModelsLib.operations.dateFormat(FirstBalanceDate) + "'," + Supplier_Bank + ",'" + Account_Code + "') ";
                //Id = opp.Insert_me_Return_Last_Identify(mysql, ref Excp);
            }
            else
            {
                bnk.Id = bnk.SubAccount_Id;
                //string mysql = " INSERT INTO Banks (Id,Account_Type_Value,Branch_Name,Account_Branches_ID,Bank_Name, Primary_balance,Last_Balance,Account_No,Is_Bank,SubAccount_Id,currency_Id,firstBalanceDate,Supplier_Bank,Account_Code) ";
                //mysql = mysql + " VALUES (" + SubAccount_Id + "," + Account_Type_Value + ",'" + Branch_Name + "'," + Account_Branches_ID + ",'" + Bank_Name + "'," + Primary_Balance + "," + Primary_Balance + ", '" + Account_No + "'," + Is_Bank + "," + SubAccount_Id + "," + Currency_Id + ",'" + AccountModelsLib.operations.dateFormat(FirstBalanceDate) + "'," + Supplier_Bank + ",'" + Account_Code + "') ";
                //opp.insert_me(mysql);
            }
            opp.db.Banks.Add(bnk);
            opp.db.SaveChanges();
        }
        public Bank GetBank(ref AccountModelsLib.operations opp, long SubAccount_Id)
        {
            Bank bnk = opp.db.Banks.Where(b => b.SubAccount_Id == SubAccount_Id).FirstOrDefault();
            //string mysql = "SELECT * FROM Banks where SubAccount_Id =" + SubAccount_Id;
            //DataTable dt = opp.return_DataTable(mysql);

            return bnk;
        }
        public void UpdateBank(ref int Excp, ref AccountModelsLib.operations opp, Submain_Account  sa)
        {

            //DataTable dt = GetBank(ref opp, sa.SubAccount_Id);
            Bank bnk = GetBank(ref opp, sa.Id);
            if (bnk == null)
            {
                bnk.Bank_Name = sa.SubAccount_Name;
                bnk.Primary_balance = 0;
                bnk.Last_Balance = 0;
                bnk.SubAccount_Id = sa.Id;
                bnk.Currency_Id = sa.Currency_Id;

                bnk.firstBalanceDate = DateTime.Now.Date;
                bnk.Account_Code = sa.SubAccount_NO;
                bnk.Branch_Id = sa.Branch_Id;
                bnk.Account_Type_Value = sa.Account_Customer_Type_Value;
                bnk.Id = sa.Id;

                InsertBank(ref Excp, ref opp, bnk);
            }
            else
            {
                //Id = long.Parse(dt.Rows[0]["Id"].ToString());
                //Get_Bank(ref opp, Id);

                bnk.Bank_Name = sa.SubAccount_Name;
                bnk.Currency_Id = sa.Currency_Id;
                bnk.Account_Code = sa.Account_Code;
                bnk.SubAccount_Id = sa.Id;

                UpdateBank(ref opp, bnk);
            }
        }
        public void UpdateBank(ref AccountModelsLib.operations opp, Bank bnk)
        {
            //string mysql = " Update Banks Set Account_Type_Value=" + Account_Type_Value + ",Branch_Name='" + Branch_Name + "',Account_Branches_ID=" + Account_Branches_ID + ",Bank_Name='" + Bank_Name + "', Primary_balance=" + Primary_Balance + ",Last_Balance=" + Last_Balance + ",Account_No='" + Account_No + "',Is_Bank=" + Is_Bank + ",SubAccount_Id=" + SubAccount_Id + ",currency_Id=" + Currency_Id + ",firstBalanceDate='" + AccountModelsLib.operations.dateFormat(FirstBalanceDate) + "',Supplier_Bank=" + Supplier_Bank + ",Account_Code='" + Account_Code + "' ";
            //mysql = mysql + " Where Id=" + Id;
            //opp.insert_me(mysql);
            opp.db.Entry(bnk).State = EntityState.Modified;
            opp.db.SaveChanges();

        }
        public void AddNewBank(ref int Excp, ref string Msg, Bank ba)
        {
            Excp = 0;
            Msg = "";
            AccountModelsLib.operations opp = new AccountModelsLib.operations(1);
            try
            {

                string Account_Code = "";
                Sub_Account acc = new Sub_Account();

                long New_Submain_Account_Id = ba.SubAccount_Id;
                acc.ba = ba;
                int Account_Type_Value = Common.Bank_Account_Type;
                acc.Add_AccountSupplierCustomerOther(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, ref Account_Code, Account_Type_Value, ba.Bank_Name, Common.Branch_Id, ba.Currency_Id ?? 1);

                ba.Id = acc.New_Account_Id;
                ba.SubAccount_Id = New_Submain_Account_Id;

                opp.Commit();
                Excp = 0;

                Excp = 0;
                acc.UpdateSubmain_SubAccount_Id(ref Excp, ref Msg);

            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
                Msg = ee.Message;
            }
        }
        public void DeleteBank(long Id)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DeleteBank(ref opp, Id);
        }
        public void DeleteBank(ref AccountModelsLib.operations opp, long Id)
        {

            string mysql = "";
            if (Common.Update_DeleteSupplier == 0)
            {
                mysql = "Update Banks set active=0 Where Id= " + Id;
            }
            if (Common.Update_DeleteSupplier == 1)
            {
                mysql = "delete from Banks where Id= " + Id;
            }
            opp.insert_me(mysql);
        }
        public void MakeDeleteBanks(ref AccountModelsLib.operations opp, ref int Excp, ref string Msg, long Id, long SubAccount_Id)
        {
            string mysql = "";


            mysql = "delete from Banks where Id= " + Id;
            mysql = mysql + "delete from Banks where SubAccount_Id= " + SubAccount_Id;

            try
            {
                opp.insert_me(mysql);
                Excp = 0;
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
            }
        }
    }
}
