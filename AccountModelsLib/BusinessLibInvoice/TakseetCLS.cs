
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Data.OleDb;
 

namespace AccountModelsLib
{
    public class TakseetCLS
    {
        public TakseetCLS()
        {
        }
        public long Make_Takset_List(ref operations opp, long Customer_Id, long Invoice_List_Id, long Invoices_Details_Id, decimal Total_Money, bool Is_First_PaymentMoney, decimal First_PaymentMoney, DateTime DateOfFirst_PaymentMoney, string Statements_No, string Notebooks_No, string Guarantor_Name, string Guarantor_Id, int DateDay_Payment, string Notes, int Takset_Number, decimal kest_Value_needed, decimal Total_Takset_Value, DateTime DateOfProcess, DateTime DateOfFirst_Takset)
        {
            long Takset_List_Id = 0;
            decimal Increased_Values = 0;
            try
            {


                string mysql = " INSERT INTO Takset_List(Customer_Id,Invoice_List_Id, Invoices_Details_Id, Total_Money, First_PaymentMoney, Statements_No, Notebooks_No, Guarantor_Name, Guarantor_Id, DateDay_Payment, Notes, kest_Value_needed,Takset_Number, Total_Takset_Value,Increased_Values,DateOfProcess, DateOfFirst_Takset,DateOfFirst_PaymentMoney) ";
                mysql = mysql + " VALUES (" + Customer_Id + "  , " + Invoice_List_Id + "  ,  " + Invoices_Details_Id + "  ,  " + Total_Money + "  ,  " + First_PaymentMoney + "  ,  '" + Statements_No + "'  , ' " + Notebooks_No + "'  ,  '" + Guarantor_Name + "'  , ' " + Guarantor_Id + "'  ,  " + DateDay_Payment + "  ,  '" + Notes + "'  , " + kest_Value_needed + ", " + Takset_Number + "  ,  " + Total_Takset_Value + "," + Increased_Values + ",'" + operations.dateFormat(DateOfProcess) + "','" + operations.dateFormat(DateOfFirst_Takset) + "','" + operations.dateFormat(DateOfFirst_PaymentMoney) + "' ) ";
                Takset_List_Id = opp.Insert_me_Return_Last_Identify(mysql);

                mysql = " Delete From  Takset_Details Where Takset_List_Id=" + Takset_List_Id + "";
                opp.insert_me(mysql);


                Make_Takset_Details(ref opp, Takset_List_Id,   Is_First_PaymentMoney, First_PaymentMoney, DateOfFirst_PaymentMoney, Takset_Number, kest_Value_needed, DateOfFirst_Takset, DateDay_Payment);
            }
            catch (Exception e)
            {



            }
            return Takset_List_Id;
        }
        public void Update_Takset_List(ref operations opp, ref int Excp,ref string msg,long Takset_List_Id, long Invoice_List_Id, long Invoices_Details_Id, decimal Total_Money, bool Is_First_PaymentMoney, decimal First_PaymentMoney, DateTime DateOfFirst_PaymentMoney, string Statements_No, string Notebooks_No, string Guarantor_Name, string Guarantor_Id, int DateDay_Payment, string Notes, int Takset_Number, decimal kest_Value_needed, decimal Total_Takset_Value, DateTime DateOfProcess, DateTime DateOfFirst_Takset)
        {
            Excp = 0;

            try
            {
                ////check and mony paid 
                string mysql = "Select * from Takset_Details   ";
                mysql = mysql + " Where Takset_List_Id=" + Takset_List_Id + " And Order_Mony_Id <>0 ";
               DataTable dt= opp.return_DataTable(mysql);
                bool IsMonyPaid = false;
                for(int x=0;x< dt.Rows.Count;x++)
                {
                    try
                    {
                        decimal Order_Mony_Id = decimal.Parse(dt.Rows[x]["Order_Mony_Id"].ToString());
                        if(Order_Mony_Id > 0)
                        {
                            IsMonyPaid = true;
                            break;
                        }
                    }
                    catch { }
                }
                 
                if (IsMonyPaid == true)
                {
                    Excp = 1;
                    msg = "لا يمكن تعديل نظام اقسط للفاتوره يوجد مبلغ نقدى مدفوع";
                    return;
                }

                  mysql = "Select * from Invoices_List   Where Id=" + Invoice_List_Id + "   ";
                long Customer_Id = 0;
                DataTable dt2 = opp.return_DataTable(mysql);
                if(dt2.Rows.Count>0)
                {
                    Customer_Id= long.Parse(dt2.Rows[0]["Customer_Id"].ToString());
                }

                decimal Increased_Values = 0;
                //  mysql = " Update   Takset_List Set ";
                //mysql = mysql + " Invoice_List_Id =" + Invoice_List_Id + "  ,   Invoices_Details_Id =  " + Invoices_Details_Id + "  ,   Total_Money =  " + Total_Money + "  ,   First_PaymentMoney =  " + First_PaymentMoney + "   ,    DateDay_Payment=  " + DateDay_Payment + "   ,    kest_Value_needed= " + kest_Value_needed + ",Takset_Number= " + Takset_Number + "  ,   Total_Takset_Value =  " + Total_Takset_Value + ",Increased_Values=" + Increased_Values + ",DateOfProcess='" + operations.dateFormat(DateOfProcess) + "',DateOfFirst_Takset='" + operations.dateFormat(DateOfFirst_Takset) + "',DateOfFirst_PaymentMoney='" + operations.dateFormat(DateOfFirst_PaymentMoney) + "'  ";
                //mysql = mysql + " ,  Notes=  '" + Notes + "'  ,   Statements_No =  '" + Statements_No + "'  ,    Notebooks_No= ' " + Notebooks_No + "'  ,   Guarantor_Name =  '" + Guarantor_Name + "'  ,   Guarantor_Id = ' " + Guarantor_Id + "'  ";
                //mysql = mysql + " Where Id=" + Takset_List_Id + "";
                //opp.insert_me(mysql);

                mysql = " Delete from    Takset_Details   ";
                mysql = mysql + " Where Takset_List_Id=" + Takset_List_Id + "";
                opp.insert_me(mysql);

                mysql = " Delete From  Takset_List Where Id=" + Takset_List_Id + "";
                opp.insert_me(mysql);


                mysql = " INSERT INTO Takset_List(Customer_Id,Invoice_List_Id, Invoices_Details_Id, Total_Money, First_PaymentMoney, Statements_No, Notebooks_No, Guarantor_Name, Guarantor_Id, DateDay_Payment, Notes, kest_Value_needed,Takset_Number, Total_Takset_Value,Increased_Values,DateOfProcess, DateOfFirst_Takset,DateOfFirst_PaymentMoney) ";
                mysql = mysql + " VALUES (" + Customer_Id + "  , " + Invoice_List_Id + "  ,  " + Invoices_Details_Id + "  ,  " + Total_Money + "  ,  " + First_PaymentMoney + "  ,  '" + Statements_No + "'  , ' " + Notebooks_No + "'  ,  '" + Guarantor_Name + "'  , ' " + Guarantor_Id + "'  ,  " + DateDay_Payment + "  ,  '" + Notes + "'  , " + kest_Value_needed + ", " + Takset_Number + "  ,  " + Total_Takset_Value + "," + Increased_Values + ",'" + operations.dateFormat(DateOfProcess) + "','" + operations.dateFormat(DateOfFirst_Takset) + "','" + operations.dateFormat(DateOfFirst_PaymentMoney) + "' ) ";
              long  Takset_List_IdNew = opp.Insert_me_Return_Last_Identify(mysql);

                Make_Takset_Details(ref opp, Takset_List_IdNew, Is_First_PaymentMoney, First_PaymentMoney, DateOfFirst_PaymentMoney, Takset_Number, kest_Value_needed, DateOfFirst_Takset, DateDay_Payment);


            }
            catch (Exception e)
            {



            }

        }

        public void Delete_Takset(ref operations opp, long Takset_List_Id)
        {

            try
            {


                string mysql = " Delete from    Takset_List   ";
                mysql = mysql + " Where Id=" + Takset_List_Id + "";
                opp.insert_me(mysql);

                mysql = " Delete from    Takset_Details   ";
                mysql = mysql + " Where Takset_List_Id=" + Takset_List_Id + "";
                opp.insert_me(mysql);

            }
            catch (Exception e)
            {



            }

        }
        public long Make_Takset_Details(ref operations opp, long Takset_List_Id, bool Is_First_PaymentMoney, decimal First_PaymentMoney, DateTime DateOfFirst_PaymentMoney, int Takset_Number, decimal kest_Value_needed, DateTime DateOfFirst_Takset, int DateDay_Payment)
        {

            DateTime DateOfDue = new DateTime(); ;
            decimal Takset_Value_needed = 0;
            decimal Payed_Value = 0;
            decimal Rest_Value = 0;
            DateTime DateOfPayment;
            decimal Increased_Values = 0;
            int Payment_Type = 0;
            string Payment_TypeStr = "";

            string mysql = "";
            try
            {
                ///////////////// تسجيل المقدم المدفوع
                ///
                decimal Mokadam_Value = 0;
                DateOfDue = DateOfFirst_PaymentMoney;
                Payment_Type = 0;
                Payment_TypeStr = "مقدم";
                decimal Mokadam_Payed_Value = 0;
                decimal Mokadam_Rest_Value = 0;
                if (Is_First_PaymentMoney == true)
                {
                    Mokadam_Value = First_PaymentMoney;
                    Mokadam_Payed_Value = First_PaymentMoney;
                    Mokadam_Rest_Value = 0;
                }
                else
                {
                    Mokadam_Value = First_PaymentMoney;
                    Mokadam_Payed_Value = 0;
                    Mokadam_Rest_Value = First_PaymentMoney;
                }

               

                mysql = " INSERT INTO Takset_Details(Takset_List_Id, DateOfDue, DateOfPayment,kest_Value_needed, Payed_Value, Rest_Value,Increased_Values ,Payment_Type,Payment_TypeStr) ";
                mysql = mysql + " VALUES (" + Takset_List_Id + "  ,  '" + operations.dateFormat(DateOfDue) + "',  '" + operations.dateFormat(DateOfDue) + "'  ,  " + Mokadam_Value + "  ,  " + Mokadam_Payed_Value + "  ,  " + Mokadam_Rest_Value + " ,  0," + Payment_Type + ",'" + Payment_TypeStr + "') ";
                opp.insert_me(mysql);
                ////////////////////////////////////////////////////////////

                ////////////////////////////////
                /////// الأقساط
                ///
                DateOfDue = DateOfFirst_Takset;
                //int DateDay_Payment
                for (int x = 0; x < Takset_Number; x++)
                {
                    Payment_Type = 1;
                    Payment_TypeStr = "قسط";
                    mysql = " INSERT INTO Takset_Details(Takset_List_Id, DateOfDue, kest_Value_needed, Payed_Value, Rest_Value,Increased_Values ,Payment_Type) ";
                    mysql = mysql + " VALUES (" + Takset_List_Id + "  ,  '" + operations.dateFormat(DateOfDue) + "'  ,  " + kest_Value_needed + "  ,  " + Payed_Value + "  ,  " + Rest_Value + " ,  " + Increased_Values + "," + Payment_Type + ") ";
                    opp.insert_me(mysql);

                    DateOfDue = DateOfDue.AddMonths(1);
                }


            }
            catch (Exception e)
            {



            }
            return Takset_List_Id;
        }
        public DataTable Get_Takset_List(long Takset_List_Id, ref DataTable dtTakset_Details, ref DataTable dtItems)
        {
            DataTable dtTakset_List = new DataTable();

            try
            {
                operations opp = new operations();

                string mysql = " Select *  From  Takset_List Where Id=" + Takset_List_Id + "";

                dtTakset_List = opp.return_DataTable(mysql);

                mysql = " Select *  From Takset_Details Where Takset_List_Id=" + Takset_List_Id + "";

                dtTakset_Details = opp.return_DataTable(mysql);
                if (dtTakset_List.Rows.Count > 0)
                {
                    long Invoice_List_Id = long.Parse(dtTakset_List.Rows[0]["Invoice_List_Id"].ToString());
                    mysql = @" SELECT        Item.Item_Name, Invoices_Details.Larg_Qnty, Invoices_Details.Qnty_value, Invoices_Details.Total, Customer.Name, Invoices_List.Invoice_Date
FROM            Invoices_Details INNER JOIN
                         Item ON Invoices_Details.Item_Id = Item.id INNER JOIN
                         Invoices_List ON Invoices_Details.Invoice_Id = Invoices_List.ID INNER JOIN
                         Customer ON Invoices_List.Customer_Id = Customer.Id  Where Invoices_Details.Invoice_Id=" + Invoice_List_Id + "";
                    dtItems = opp.return_DataTable(mysql);
                }
            }
            catch (Exception e)
            {

            }
            return dtTakset_List;
        }
        public DataTable Get_Takset_Details(long Takset_List_Id)
        {
            DataTable dtTakset_Details = new DataTable();

            operations opp = new operations();

            string mysql = " Select *  From Takset_Details Where Takset_List_Id=" + Takset_List_Id + "";

            dtTakset_Details = opp.return_DataTable(mysql);
            return dtTakset_Details;
        }

        public DataTable Get_Takset_List(string Customer_Name, long Customer_Id, string Guarantor_Name, string Item_Name, string Statements_No, string Mobile)
        {
            DataTable dtTakset_List = new DataTable();


            operations opp = new operations();

            string mysql = @"  SELECT        Invoices_List.Invoice_Id, Invoices_List.Invoice_Date, Customer.Name, Takset_List.*, Customer.Tel, Customer.Tel2, Customer.Mobile, Item.Item_Name, Invoices_Details.Larg_Qnty
FROM            Item INNER JOIN
                         Invoices_Details ON Item.id = Invoices_Details.Item_Id INNER JOIN
                         Customer INNER JOIN
                         Invoices_List ON Customer.Id = Invoices_List.Customer_Id INNER JOIN
                         Takset_List ON Invoices_List.Invoice_Id = Takset_List.Invoice_List_Id ON Invoices_Details.Invoice_Id = Invoices_List.Invoice_Id ";
            mysql = mysql + " Where Takset_List.Id  > 0";

            if (Customer_Id > 0)
            {
                mysql = mysql + " AND (Customer.Id = " + Customer_Id + ") ";
            }

            if (Customer_Name.Trim() != "")
            {
                mysql = mysql + " AND (Customer.Name like N'%" + Customer_Name + "%') ";
            }
            if (Mobile.Trim() != "")
            {
                mysql = mysql + " AND ((Customer.Mobile like N'%" + Mobile + "%') OR  (Customer.Tel like N'%" + Mobile + "%') OR  (Customer.Tel2 like N'%" + Mobile + "%')  ) ";
            }


            if (Guarantor_Name.Trim() != "")
            {
                mysql = mysql + " AND (Takset_List.Guarantor_Name like N'%" + Guarantor_Name + "%') ";
            }
            if (Statements_No.Trim() != "")
            {
                mysql = mysql + " AND (Takset_List.Statements_No like N'%" + Statements_No + "%') ";
            }

            if (Item_Name.Trim() != "")
            {
                mysql = mysql + " AND (Item.Item_Name like N'%" + Item_Name + "%') ";
            }
            dtTakset_List = opp.return_DataTable(mysql);



            return dtTakset_List;
        }
        public void Pay_Takset_Details(ref operations opp, bool IsTrasfeer_Increased_Values, DataTable dtDetails, int index, long Order_Mony_Id, decimal OrderValue, DateTime DateOfPayment,decimal Total_Value_Payed,string OrderNo)
        {
            long Takset_Details_Id = long.Parse(dtDetails.Rows[index]["Id"].ToString());
            decimal Payed_Value_Old = decimal.Parse(dtDetails.Rows[index]["Payed_Value"].ToString());
            decimal Rest_Value_Old = decimal.Parse(dtDetails.Rows[index]["Rest_Value"].ToString());
            decimal Increased_Values_Old = decimal.Parse(dtDetails.Rows[index]["Increased_Values"].ToString());
            decimal kest_Value_needed = decimal.Parse(dtDetails.Rows[index]["kest_Value_needed"].ToString());


            decimal Payed_Value_New = Payed_Value_Old + OrderValue;
            decimal Rest_Value_New = 0;
            decimal Increased_Values_New = 0;
            string Notes = "";
            if (Payed_Value_New == kest_Value_needed)
            {

                Rest_Value_New = 0;
                Increased_Values_New = 0;
                Notes = "تم سداد القسط";
            }
            if (Payed_Value_New < kest_Value_needed)
            {

                Rest_Value_New = kest_Value_needed - Payed_Value_New;
                Increased_Values_New = 0;
                Notes = "تم سداد جزء من القسط";
            }
            if (Payed_Value_New > kest_Value_needed)
            {
                decimal Payed_Value_New_ = Payed_Value_New;
                Payed_Value_New = kest_Value_needed - Payed_Value_Old;
                Rest_Value_New = 0;
                Increased_Values_New = Payed_Value_New_ - kest_Value_needed;

                Notes = " يوجد مبلغ زائد عن القسط";
            }

            string mysql = " Update Takset_Details Set Total_Value_Payed="+ Total_Value_Payed + ",OrderNo='"+ OrderNo + "' ,Increased_ValuesSaved=Increased_Values,Payed_ValueSaved=  Payed_Value ,Rest_ValueSaved=Rest_Value,Order_Mony_Id=" + Order_Mony_Id + ",  DateOfPayment='" + operations.dateFormat(DateOfPayment) + "' ,  Payed_Value=Payed_Value + " + Payed_Value_New + ", Rest_Value=" + Rest_Value_New + ",Increased_Values =" + Increased_Values_New + " ,Notes='" + Notes + "' ";
            mysql = mysql + " Where Id=" + Takset_Details_Id + "";

            opp.insert_me(mysql);


            if (Increased_Values_New > 0 & IsTrasfeer_Increased_Values == true)
            {

                int indexNew = index + 1;
                long Takset_Details_IdNew = 0;
                try
                {
                    Takset_Details_IdNew = long.Parse(dtDetails.Rows[indexNew]["Id"].ToString());
                }
                catch
                {
                    Takset_Details_IdNew = 0;
                }

                if (Takset_Details_IdNew > 0)
                {
                    Pay_Takset_Details(ref opp, IsTrasfeer_Increased_Values, dtDetails, indexNew, Order_Mony_Id, Increased_Values_New, DateOfPayment, Total_Value_Payed   ,OrderNo);
                }

            }
        }
        public void Update_Takset_Details(ref operations opp, bool IsTrasfeer_Increased_Values, DataTable dtDetails, int index, long Order_Mony_Id , decimal OrderValue, DateTime DateOfPayment, decimal Total_Value_Payed,string  OrderNo)
        {
            
            
            Pay_Takset_Details(ref opp, IsTrasfeer_Increased_Values, dtDetails, index, Order_Mony_Id , OrderValue, DateOfPayment, Total_Value_Payed, OrderNo);

        }

        public void Delete_Pay_Takset_Details(ref int Excp, ref operations opp, long Order_Mony_Id )
        {
            try
            {
                Excp = 0;
                string mysql = " Update Takset_Details Set Total_Value_Payed=0,OrderNo='', Payed_ValueSaved=0,Rest_ValueSaved=0,  Payed_Value=Payed_ValueSaved ,Increased_ValuesSaved=0, Rest_Value=Rest_ValueSaved,Order_Mony_Id=0,  DateOfPayment=NULL ,  Increased_Values =  Increased_ValuesSaved  ,Notes='تم الغاء السداد' ";
                mysql = mysql + " Where Order_Mony_Id=" + Order_Mony_Id + "";
                opp.insert_me(mysql);
            }
            catch { Excp = 1; }
        }
    }
}

