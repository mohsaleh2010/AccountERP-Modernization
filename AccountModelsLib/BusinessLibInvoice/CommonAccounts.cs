using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
  using AccountModelsLib; using AccountModelsLib.Models;
 
 
namespace AccountModelsLib
{
    public class CommonAccounts
    {

        public static DataTable GetUnit_Item_ExpireDate(ref AccountModelsLib.operations opp, InvoiceUpdateExpireQnty invd)
        {
            string mysql = " Select * from Unit_Item_ExpireDate ";
            if (invd.Unit_Item_ExpireDate_Id > 0)
            {
                mysql = mysql + " Where Id=" + invd.Unit_Item_ExpireDate_Id.ToString() + " ";
            }
            else
            {
                mysql = mysql + " Where Item_Id=" + invd.Item_Id.ToString() + " And Unit_Item_Table_Id=" + invd.Unit_Item_Table_Id.ToString();
                if (InvoiceUpdateExpireQnty.Is_Item_Expire_Date_WithBalance == 1)
                {
                    mysql = mysql + "  And Item_Expire_Date ='" + AccountModelsLib.operations.dateFormat(invd.Item_Expire_Date) + "'";
                }
                if (InvoiceUpdateExpireQnty.Is_Tash3ela_WithBalance == 1)
                {
                    mysql = mysql + "  And Tash3ela ='" + invd.Tash3ela + "'";
                }

                if (InvoiceUpdateExpireQnty.Is_Bills_DetailsId_WithUnit_Item_ExpireDate == 1)
                {
                    mysql = mysql + "  And Bills_Details_Id = " + invd.Bills_Details_Id + " ";
                }
                if (InvoiceUpdateExpireQnty.Is_P_Price_WithUnit_Item_ExpireDate == 1)
                {
                    mysql = mysql + "  And P_Price = " + invd.Larg_Unit_Price + " ";
                }
            }
            DataTable dt = opp.return_DataTable(mysql);
            return dt;
        }
        public static void SetBalanceWithExpireDate_Tash3ela(ref AccountModelsLib.operations opp, int Transaction, InvoiceUpdateExpireQnty invd, decimal Updated_Larg_Qnty, decimal Updated_Small_Qnty, decimal Updated_Item_NoQnty, bool IsReverse = false)
        {
            string mysql = "";
            ///
            decimal Larg_Balance = invd.Larg_Qnty + invd.LargItemsQnty_Free;
            decimal Small_Balance = invd.Small_Qnty + invd.SmallItemsQnty_Free;
            decimal Item_No = invd.Qnty_Another + invd.Item_No_Free;
            DataTable dt = GetUnit_Item_ExpireDate(ref opp, invd);

            long Unit_Item_ExpireDateId = 0;
            if (Transaction == 0)
            {
                //decimal Larg_Balance_ = OLD_Larg_Qnty - Larg_Balance;
                //decimal Small_Balance_ = OLD_Small_Qnty - Small_Balance;
                //decimal Item_No_ = OLD_Item_NoQnty - Item_No;
                if (dt.Rows.Count > 0)
                {
                    Unit_Item_ExpireDateId = long.Parse(dt.Rows[0]["Id"].ToString());

                    //mysql = " Update Unit_Item_ExpireDate Set Larg_Balance=Larg_Balance + " + Updated_Larg_Qnty + " , Small_Balance=Small_Balance + " + Updated_Small_Qnty + ",Item_No=Item_No + " + Updated_Item_NoQnty + "  ";
                    //mysql = mysql + " Where Id=" + Unit_Item_ExpireDateId.ToString();


                    UpdatePLUS_Unit_Item_ExpireDate(ref opp, Unit_Item_ExpireDateId, Updated_Larg_Qnty, Updated_Small_Qnty, Updated_Item_NoQnty);
                }
                else
                {
                    Common.Get_PositiveNumber(ref Updated_Larg_Qnty);
                    Common.Get_PositiveNumber(ref Updated_Small_Qnty);
                    Common.Get_PositiveNumber(ref Updated_Item_NoQnty);

                    Unit_Item_ExpireDateId = InsertUnit_Item_ExpireDate(ref opp, invd, Updated_Larg_Qnty, Updated_Small_Qnty, Updated_Item_NoQnty);
                }
            }

            if (Transaction == 1)
            {
                if (dt.Rows.Count > 0)
                {
                    Unit_Item_ExpireDateId = long.Parse(dt.Rows[0]["Id"].ToString());
                    //mysql = " Update Unit_Item_ExpireDate Set Larg_Balance=Larg_Balance + " + Larg_Balance.ToString() + " , Small_Balance=Small_Balance + " + Small_Balance.ToString() + ",Item_No=Item_No + " + Item_No.ToString() + "  ";
                    //mysql = mysql + " Where Id=" + Unit_Item_ExpireDateId.ToString();

                    UpdatePLUS_Unit_Item_ExpireDate(ref opp, Unit_Item_ExpireDateId, Larg_Balance, Small_Balance, Item_No);

                }
                else
                {

                    //mysql = "INSERT INTO Unit_Item_ExpireDate(Item_id, Unit_Item_Table_Id, Tash3ela, Item_Expire_Date, Larg_Balance, Small_Balance, Item_No) ";
                    //mysql = mysql + " VALUES(" + invd.Item_Id + "," + invd.Unit_Item_Table_Id + ",'" + invd.Tash3ela + "','" + AccountModelsLib.operations.dateFormat(invd.Item_Expire_Date) + "'," + Larg_Balance + "," + Small_Balance + "," + Item_No + ")";

                    Unit_Item_ExpireDateId = InsertUnit_Item_ExpireDate(ref opp, invd, Larg_Balance, Small_Balance, Item_No);


                }
            }
            if ((Transaction == 2))
            {
                if (dt.Rows.Count > 0)
                {
                    Unit_Item_ExpireDateId = long.Parse(dt.Rows[0]["Id"].ToString());
                    //mysql = " Update Unit_Item_ExpireDate Set Larg_Balance=Larg_Balance - " + Larg_Balance.ToString() + " , Small_Balance=Small_Balance - " + Small_Balance.ToString() + ",Item_No=Item_No - " + Item_No.ToString() + "  ";
                    //mysql = mysql + " Where Id=" + Unit_Item_ExpireDateId.ToString();
                    UpdateMinus_Unit_Item_ExpireDate(ref opp, Unit_Item_ExpireDateId, Larg_Balance, Small_Balance, Item_No);


                }
            }

            UpdateUnit_Item_ExpireDate(ref opp, Unit_Item_ExpireDateId);
            //opp.insert_me(mysql);
        }
        static long InsertUnit_Item_ExpireDate(ref AccountModelsLib.operations opp, InvoiceUpdateExpireQnty invd, decimal Updated_Larg_Qnty, decimal Updated_Small_Qnty, decimal Updated_Item_NoQnty)
        {
            string Expire_Date = "";
            DateTime Item_Expire_Date = new DateTime();
            long Unit_Item_ExpireDateId = 0;
            string Tash3ela = "";
            long Bills_Details_Id = 0;
            decimal P_Price = 0;
            if (Common.Is_Item_Expire_Date_WithBalance == 1)
            {
                Expire_Date = "'" + AccountModelsLib.operations.dateFormat(invd.Item_Expire_Date) + "'";

            }
            else
            {
                Expire_Date = "NULL";
            }
            if (Common.Is_Tash3ela_WithBalance == 1)
            {
                Tash3ela = invd.Tash3ela;
            }

            if (Common.Is_Bills_DetailsId_WithUnit_Item_ExpireDate == 1)
            {
                Bills_Details_Id = invd.Bills_Details_Id;
            }
            if (Common.Is_P_Price_WithUnit_Item_ExpireDate == 1)
            {
                P_Price = invd.Larg_Unit_Price;
            }

            string mysql = "INSERT INTO Unit_Item_ExpireDate(Item_id, Unit_Item_Table_Id, Tash3ela, Item_Expire_Date, Larg_Balance, Small_Balance, Item_No,Bills_Details_Id,P_Price) ";
            mysql = mysql + " VALUES(" + invd.Item_Id + "," + invd.Unit_Item_Table_Id + ",'" + Tash3ela + "'," + Expire_Date + "," + Updated_Larg_Qnty + "," + Updated_Small_Qnty + "," + Updated_Item_NoQnty + "," + Bills_Details_Id + ", " + P_Price + ")";
            Unit_Item_ExpireDateId = opp.Insert_me_Return_Last_Identify(mysql);

            return Unit_Item_ExpireDateId;
        }

        static void UpdateUnit_Item_ExpireDate(ref AccountModelsLib.operations opp, long Unit_Item_ExpireDateId)
        {
            if (Common.Compute_ItemBalance_With_Fraction == 0)
            {
                string mysql = @"UPDATE Unit_Item_ExpireDate SET Larg_Balance = FLOOR((Unit_item.Small_unit * Unit_Item_ExpireDate.Larg_Balance + Unit_Item_ExpireDate.Small_Balance) / Unit_item.Small_unit),  Small_Balance = (Unit_item.Small_unit * Unit_Item_ExpireDate.Larg_Balance + Unit_Item_ExpireDate.Small_Balance) % Unit_item.Small_unit FROM Unit_item INNER JOIN  Unit_Item_ExpireDate ON Unit_item.id = Unit_Item_ExpireDate.Unit_Item_Table_Id ";
                mysql = mysql + " WHERE        (Unit_Item_ExpireDate.Id = " + Unit_Item_ExpireDateId + ")";
                opp.insert_me(mysql);

            }
            // mysql = " Update Unit_Item_ExpireDate Set Larg_Balance=0   ";
            //mysql = mysql + " Where Larg_Balance < 0 ";
            //opp.insert_me(mysql);

            //mysql = " Update Unit_Item_ExpireDate Set Small_Balance=0   ";
            //mysql = mysql + " Where Small_Balance < 0 ";
            //opp.insert_me(mysql);

        }


        static void UpdatePLUS_Unit_Item_ExpireDate(ref AccountModelsLib.operations opp, long Unit_Item_ExpireDateId, decimal Updated_Larg_Qnty, decimal Updated_Small_Qnty, decimal Updated_Item_NoQnty)
        {
            string mysql = " Update Unit_Item_ExpireDate Set Larg_Balance=Larg_Balance + " + Updated_Larg_Qnty + " , Small_Balance=Small_Balance + " + Updated_Small_Qnty + ",Item_No=Item_No + " + Updated_Item_NoQnty + "  ";
            mysql = mysql + " Where Id=" + Unit_Item_ExpireDateId.ToString();
            opp.insert_me(mysql);
        }
        static void UpdateMinus_Unit_Item_ExpireDate(ref AccountModelsLib.operations opp, long Unit_Item_ExpireDateId, decimal Larg_Balance, decimal Small_Balance, decimal Item_No)
        {
            string mysql = " Update Unit_Item_ExpireDate Set Larg_Balance=Larg_Balance - " + Larg_Balance.ToString() + " , Small_Balance=Small_Balance - " + Small_Balance.ToString() + ",Item_No=Item_No - " + Item_No.ToString() + "  ";
            mysql = mysql + " Where Id=" + Unit_Item_ExpireDateId.ToString();
            opp.insert_me(mysql);
        }

        public static void SetBalanceWithExpireDate_Tash3ela_Bill(ref AccountModelsLib.operations opp, int Transaction, BillOperationCLS invd, decimal Updated_Larg_Qnty, decimal Updated_Small_Qnty, decimal Updated_Item_NoQnty, bool IsReverse = false)
        {
            SetBalanceWithExpireDate_Tash3ela(ref opp, Transaction, InvoiceUpdateExpireQnty.GetInvoiceUpdateExpireQnty(invd, IsReverse), Updated_Larg_Qnty, Updated_Small_Qnty, Updated_Item_NoQnty, IsReverse);
        }
        public static void SetBalanceWithExpireDate_Tash3ela_Bill(ref AccountModelsLib.operations opp, int Transaction, IBills_Details invd, decimal Updated_Larg_Qnty, decimal Updated_Small_Qnty, decimal Updated_Item_NoQnty, bool IsReverse = false)
        {
            SetBalanceWithExpireDate_Tash3ela(ref opp, Transaction, InvoiceUpdateExpireQnty.GetInvoiceUpdateExpireQnty(invd, IsReverse), Updated_Larg_Qnty, Updated_Small_Qnty, Updated_Item_NoQnty, IsReverse);
        }
        public static void SetBalanceWithExpireDate_Tash3ela_Bill(ref AccountModelsLib.operations opp, int Transaction, Bills_Details  invd, decimal Updated_Larg_Qnty, decimal Updated_Small_Qnty, decimal Updated_Item_NoQnty, bool IsReverse = false)
        {
            SetBalanceWithExpireDate_Tash3ela(ref opp, Transaction, InvoiceUpdateExpireQnty.GetInvoiceUpdateExpireQnty(invd, IsReverse), Updated_Larg_Qnty, Updated_Small_Qnty, Updated_Item_NoQnty, IsReverse);
        }

        public static void SetBalanceWithExpireDate_Tash3ela_Invoice(ref AccountModelsLib.operations opp, int Transaction, IInvoices_Details  invd, decimal Updated_Larg_Qnty, decimal Updated_Small_Qnty, decimal Updated_Item_NoQnty)
        {
            SetBalanceWithExpireDate_Tash3ela(ref opp, Transaction, InvoiceUpdateExpireQnty.GetInvoiceUpdateExpireQnty(invd), Updated_Larg_Qnty, Updated_Small_Qnty, Updated_Item_NoQnty);
        }

        public static void SetBalanceWithExpireDate_Tash3ela_Invoice_(ref AccountModelsLib.operations opp, int Transaction, Invoices_Details invd, decimal Updated_Larg_Qnty, decimal Updated_Small_Qnty, decimal Updated_Item_NoQnty)
        {
            SetBalanceWithExpireDate_Tash3ela(ref opp, Transaction, InvoiceUpdateExpireQnty.GetInvoiceUpdateExpireQnty(invd), Updated_Larg_Qnty, Updated_Small_Qnty, Updated_Item_NoQnty);
        }

        public static void SetBalanceWithExpireDate_Tash3ela_InvoiceReverse(ref AccountModelsLib.operations opp, int Transaction, long Unit_Item_ExpireDate_Id, long Invoices_Details_Id, long Item_Id, long Unit_Item_Table_Id, DateTime Item_Expire_Date, string Tash3ela, decimal Larg_Balance, decimal Small_Balance, decimal Item_No, decimal LargItemsQnty_Free, decimal SmallItemsQnty_Free, decimal Item_No_Free, decimal OLD_Larg_Qnty, decimal OLD_Small_Qnty, decimal OLD_Item_NoQnty)
        {
            Larg_Balance += LargItemsQnty_Free;
            Small_Balance += SmallItemsQnty_Free;
            Item_No += Item_No_Free;
            string mysql = " Select * from Unit_Item_ExpireDate Where Item_Id=" + Item_Id.ToString() + " And Unit_Item_Table_Id=" + Unit_Item_Table_Id.ToString();
            if (Common.Is_Item_Expire_Date_WithBalance == 1)
            {
                mysql = mysql + "  And Item_Expire_Date ='" + AccountModelsLib.operations.dateFormat(Item_Expire_Date) + "'";
            }
            if (Common.Is_Tash3ela_WithBalance == 1)
            {
                mysql = mysql + "  And Tash3ela ='" + Tash3ela + "'";
            }
            DataTable table = opp.return_DataTable(mysql);
            if (Transaction == 0)
            {
                if (table.Rows.Count > 0)
                {
                    long num = long.Parse(table.Rows[0]["Id"].ToString());
                    decimal num2 = Larg_Balance - OLD_Larg_Qnty;
                    decimal num3 = Small_Balance - OLD_Small_Qnty;
                    decimal num4 = Item_No - OLD_Item_NoQnty;
                    string[] textArray1 = new string[] { " Update Unit_Item_ExpireDate Set Larg_Balance=Larg_Balance + ", num2.ToString(), " , Small_Balance=Small_Balance + ", num3.ToString(), ",Item_No=Item_No + ", num4.ToString(), "  " };
                    mysql = string.Concat(textArray1) + " Where Id=" + num.ToString();
                }
                else
                {
                    decimal num5 = OLD_Larg_Qnty - Larg_Balance;
                    decimal num6 = OLD_Small_Qnty - Small_Balance;
                    decimal num7 = OLD_Item_NoQnty - Item_No;
                    if (((num5 > -1M) & (num6 > -1M)) & (num7 > -1M))
                    {
                        mysql = "INSERT INTO Unit_Item_ExpireDate(Item_id, Unit_Item_Table_Id, Tash3ela, Item_Expire_Date, Larg_Balance, Small_Balance, Item_No) ";
                    }
                    string[] textArray2 = new string[0x10];
                    textArray2[0] = mysql;
                    textArray2[1] = " VALUES(";
                    textArray2[2] = Item_Id.ToString();
                    textArray2[3] = ",";
                    textArray2[4] = Unit_Item_Table_Id.ToString();
                    textArray2[5] = ",'";
                    textArray2[6] = Tash3ela;
                    textArray2[7] = "','";
                    textArray2[8] = AccountModelsLib.operations.dateFormat(Item_Expire_Date);
                    textArray2[9] = "',";
                    textArray2[10] = num5.ToString();
                    textArray2[11] = ",";
                    textArray2[12] = num6.ToString();
                    textArray2[13] = ",";
                    textArray2[14] = num7.ToString();
                    textArray2[15] = ")";
                    mysql = string.Concat(textArray2);
                }
            }
            if (Transaction == 1)
            {
                if (table.Rows.Count > 0)
                {
                    long num8 = long.Parse(table.Rows[0]["Id"].ToString());
                    string[] textArray3 = new string[] { " Update Unit_Item_ExpireDate Set Larg_Balance=Larg_Balance + ", Larg_Balance.ToString(), " , Small_Balance=Small_Balance + ", Small_Balance.ToString(), ",Item_No=Item_No + ", Item_No.ToString(), "  " };
                    mysql = string.Concat(textArray3) + " Where Id=" + num8.ToString();
                }
                else
                {
                    mysql = "INSERT INTO Unit_Item_ExpireDate(Item_id, Unit_Item_Table_Id, Tash3ela, Item_Expire_Date, Larg_Balance, Small_Balance, Item_No) ";
                    string[] textArray4 = new string[0x10];
                    textArray4[0] = mysql;
                    textArray4[1] = " VALUES(";
                    textArray4[2] = Item_Id.ToString();
                    textArray4[3] = ",";
                    textArray4[4] = Unit_Item_Table_Id.ToString();
                    textArray4[5] = ",'";
                    textArray4[6] = Tash3ela;
                    textArray4[7] = "','";
                    textArray4[8] = AccountModelsLib.operations.dateFormat(Item_Expire_Date);
                    textArray4[9] = "',";
                    textArray4[10] = Larg_Balance.ToString();
                    textArray4[11] = ",";
                    textArray4[12] = Small_Balance.ToString();
                    textArray4[13] = ",";
                    textArray4[14] = Item_No.ToString();
                    textArray4[15] = ")";
                    mysql = string.Concat(textArray4);
                }
            }
            if ((Transaction == 2) && (table.Rows.Count > 0))
            {
                long num9 = long.Parse(table.Rows[0]["Id"].ToString());
                string[] textArray5 = new string[] { " Update Unit_Item_ExpireDate Set Larg_Balance=Larg_Balance - ", Larg_Balance.ToString(), " , Small_Balance=Small_Balance - ", Small_Balance.ToString(), ",Item_No=Item_No - ", Item_No.ToString(), "  " };
                mysql = string.Concat(textArray5) + " Where Id=" + num9.ToString();
            }
            opp.insert_me(mysql);
        }

    }
}
