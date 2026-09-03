using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
 using AccountModelsLib;  
using AccountModelsLib.Models;
namespace AccountModelsLib
{
    public class Add_Account_CodesCLS 
    {
        public Add_Account_CodesCLS()
        {

        }

        public void AddNewAccount(ref int Excp, ref string Msg, Submain_AccountCLS sa, ref long Submain_Account_Id, ref int Account_Branch_Id, ref string New__Submain_Account_No, bool IS_Have_Child_Account)
        {

            Excp = 0;
            string Account_Name = "";
            AccountModelsLib.operations opp = new AccountModelsLib.operations(2);
            try
            {

                Sub_Account acc = new Sub_Account();


                if (acc.Check_SubAccount_ID_Found(ref Excp, ref opp, Submain_Account_Id, ref Account_Name, Account_Branch_Id))
                {
                    Account_Name = Account_Name + "  :  " + "هذا الحساب موجود من قبل";
                    Msg = Account_Name;

                    return;
                }
                if (acc.Check_Account_No_Found(ref Excp, ref opp, New__Submain_Account_No, Account_Branch_Id))
                {

                    Msg = "كود الحساب موجود من قبل";

                    return;
                }

                acc.Add_Account_FromAccountTree(ref Excp, ref Msg, ref opp, ref Submain_Account_Id, sa, IS_Have_Child_Account);

                opp.Commit();
                acc.UpdateSubmain_SubAccount_Id(ref   Excp, ref   Msg);
                Excp = 0;
            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
                Msg = ee.Message;
            }
        }



        public void EditAccount(ref int Excp, ref string Msg, Submain_AccountCLS sa)
        {
            Sub_Account acc = new Sub_Account();
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            if (acc.Check_Account_No_FoundToEdit(ref Excp, ref opp, sa.sb.SubAccount_NO, sa.sb.Id))
            {
                Msg = "كود الحساب موجود من قبل";

                Excp = 1;
                return;
            }
            try
            {
                opp.BeginTransaction(2);
                acc.Edit_Account(ref Excp,ref Msg,ref opp, sa);
                //if (Excp == 0)
                //{
                //    opp.Commit();
                //}
                //else
                //{
                //    opp.Rollback();
                //    Excp = 1;

                //}
                 
            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
                Msg = ee.Message;
            }
        }
    }

}
