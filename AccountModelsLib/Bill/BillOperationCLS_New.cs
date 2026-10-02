using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
using AccountModelsLib;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
namespace AccountModelsLib
{
    public class BillOperationCLS_New  
    {
        public AccountModelsLib.operations opp;
        public IBills_Details invD = new IBills_Details();
        public List<IBills_Details> invDlst = new List<IBills_Details>();
        public List<IBills_Details> invDlst_Deleted = new List<IBills_Details>();

        public BillOperationCLS_New()
        {
            opp = new AccountModelsLib.operations();
            invD = new Bills_Details();
        }
        public BillOperationCLS_New(ref AccountModelsLib.operations opp_)
        {
           
            opp = opp_;
            invD = new Bills_Details();
        }
        public BillOperationCLS_New(ref AccountModelsLib.operations opp_, int isReverse)
        {
            opp = opp_;
            if (isReverse == 1)
            {
                invD = new Bills_Details_Reverse();
            }
            else
            {
                invD = new Bills_Details();
            }

        }
        public BillOperationCLS_New(int isReverse)
        {
            if (opp == null)
            {
                opp = new AccountModelsLib.operations();
            }
            if (isReverse == 1)
            {
                invD = new Bills_Details_Reverse();
            }
            else
            {
                invD = new Bills_Details();
            }

        }
        public void GeDetails_ReverseToEdit(long Invoices_ListId)
        {
            Bills_ListCls bills_ListCls = new Bills_ListCls();
         
            List<Bills_Details_Reverse> invD = bills_ListCls.GetBills_Details_Reverse(ref opp, Invoices_ListId);
            //AccountModelsLib.operations opp = new AccountModelsLib.operations();
            invDlst = invD.Cast<IBills_Details>().ToList();
        }
        public void GeBills_DetailsToEdit(long Invoices_ListId)
        {
            Bills_ListCls bills_ListCls = new Bills_ListCls();
            List<Bills_Details> invD=bills_ListCls.GetBills_Details(ref opp, Invoices_ListId);
            //AccountModelsLib.operations opp = new AccountModelsLib.operations();
            invDlst = invD.Cast<IBills_Details>().ToList();
        }

        //public Bills_List invList = new    Bills_List ();
        //public Bills_List invList { get; set; }
        //public Bills_Details invDNewEdit { get; set; }

        //public List<Bills_Details> invD = new List<Bills_Details>();
        //public List<Bills_Details> invD { get; set; }   


        public long Invoices_List_Id { get; set; } = 0;
        public long Invoices_Details_Id { get; set; } = 0;


        #region propreties


        //public decimal Available_Larg_Balance;
       /*
        public decimal Available_Larg_Balance {
            get;
            set; }
        public decimal Available_Small_Balance;

        public string Item_Name;

        public decimal Larg_Sale_price;
        public decimal Small_Sale_price;
        public decimal Purchas_Price;

        public string Larg_Unit;
        public string Small_Unit;

        public int Is_Buy_From_Bills_Items_Balance = 0;
        public decimal No3eboa;

        public string Item_Code_Expire;


        public string Store_Name;
        public string Store_Code;


        public decimal Reserved_Balance_Larg_Qnty;
        public decimal Reserved_Balance_Small_Qnty;


        public decimal Big_Unit_Value;
        public decimal Small_Unit_Value = 1;


        public string Currency;



        public decimal Larg_Qnty_Balance = 0;
        public decimal Small_Qnty_Balance = 0;
        public decimal Qnty_Another_Balance = 0;
        public decimal Purchase_Price = 0;



        public decimal ATS05_List = 0;
        public decimal ATS05_Value_List = 0;
        public decimal TaxSales10_List = 0;
        public decimal TaxSales10_Value_List = 0;



        public decimal Item_Height_ = -1;
        public decimal Item_Width_ = -1;
        public int Items_Types_Name_Id;
        public string Items_Types_Name;

        public decimal Item_Somk = -1;
        public string Somk_Unit = "";
         */
        #endregion


        public void Insert_Bills_Details(ref AccountModelsLib.operations opp, ref int Excp)
        {
            //Bills_Details_Id = 0;
            //try
            //{



            //    opp.cmd.Parameters.Clear();
            //    string mysql = "";

            //    mysql = " INSERT INTO Bills_Details (Unit_Item_ExpireDate_Id,OperationNo,StatementNo,Merchant_Name,Store_Id,Invoice_Item_Name,Invoice_Id,Item_Id, Larg_Qnty, Small_Qnty,Qnty_Another, Larg_Unit_Price, Small_Unit_Price, Qnty_value, ATS05, ATS05_Value,TaxSales10,TaxSales10_Value,Unit_Used_for_Price,unit_id,Unit_Item_Table_Id,code, Notes,Balance_Checked, Specification,NameSpecification,Make_Order , QntyWidth,QntyLenght,QntyItemNo, Is_InvoiceItem_Kta3y, Audience_price ,Tash3ela , Item_No_Free, LargItemsQnty_Free, SmallItemsQnty_Free ,Item_Expire_Date, Is_Item_CommissionValue,Item_CommissionValue,Unit_CommissionValue,Item_CommissionRate,Net_Total_Item_Commission,Is_Alternate_Item_CommissionValue, Alternate_Item_CommissionValue, Alternate_Unit_CommissionValue, Alternate_Item_CommissionRate, Alternate_ItemCommissionTotal,ItemCommissionTotal ,Item_ATS05,Item_ATS05_Value,Item_TaxSales10,Item_TaxSales10_Value,Item_Notes, Item_Total_WithoutTax , Item_Net_Total_Price ,Total,QntyStr,QntyStrColumn,TotalSmall_Qnty,TotalSmall_QntyStr,TotalSmall_QntyStrColumn, P_price_LargeUnit,P_price_SmallUnit) ";
            //    mysql = mysql + " Values (@Unit_Item_ExpireDate_Id,@OperationNo,@StatementNo,@Merchant_Name,@Store_Id,@Invoice_Item_Name,@Invoice_Id,@Item_Id,@Larg_Qnty,@Small_Qnty,@Qnty_Another,@Larg_Unit_Price,@Small_Unit_Price,@Qnty_value,@ATS05,@ATS05_Value,@TaxSales10,@TaxSales10_Value,@Unit_Used_for_Price,@unit_id,@Unit_Item_Table_Id,@code,@Notes,@Balance_Checked,@Specification,@NameSpecification,@Make_Order ,@QntyWidth,@QntyLenght,@QntyItemNo,@Is_InvoiceItem_Kta3y,@Audience_price ,@Tash3ela ,@Item_No_Free,@LargItemsQnty_Free,@SmallItemsQnty_Free ,@Item_Expire_Date,@Is_Item_CommissionValue,@Item_CommissionValue,@Unit_CommissionValue,@Item_CommissionRate,@Net_Total_Item_Commission,@Is_Alternate_Item_CommissionValue,@Alternate_Item_CommissionValue,@Alternate_Unit_CommissionValue,@Alternate_Item_CommissionRate,@Alternate_ItemCommissionTotal,@ItemCommissionTotal ,@Item_ATS05,@Item_ATS05_Value,@Item_TaxSales10,@Item_TaxSales10_Value,@Item_Notes,@Item_Total_WithoutTax ,@Item_Net_Total_Price,@Total,@QntyStr,@QntyStrColumn,@TotalSmall_Qnty,@TotalSmall_QntyStr,@TotalSmall_QntyStrColumn, @P_price_LargeUnit,@P_price_SmallUnit) ";

               
            //    P_price_LargeUnit = 0;
            //    P_price_SmallUnit = 0;
            //    if (Unit_Used_for_Price == 1)
            //    {

            //        P_price_LargeUnit = Larg_Unit_Price;

            //        try
            //        {
            //            //حساب سعر القطعة من الدستة
            //            P_price_SmallUnit = Larg_Unit_Price / Small_Unit_Value;
            //            P_price_SmallUnit = Common.RoundMe(P_price_SmallUnit);
            //        }
            //        catch (Exception)
            //        {

            //            P_price_SmallUnit = 0;
            //        }
            //    }
            //    if (Unit_Used_for_Price == 2)
            //    {
            //        P_price_SmallUnit = Larg_Unit_Price;

            //        //حساب سعر   الدستة
            //        P_price_LargeUnit = Larg_Unit_Price * Small_Unit_Value;
            //        P_price_LargeUnit = Common.RoundMe(P_price_LargeUnit);
            //    }


            //    opp.cmd.Parameters.AddWithValue("@P_price_LargeUnit", P_price_LargeUnit);
            //    opp.cmd.Parameters.AddWithValue("@P_price_SmallUnit", P_price_SmallUnit);

            //    opp.cmd.Parameters.AddWithValue("@Item_Id", Item_Id);

            //    opp.cmd.Parameters.AddWithValue("@Unit_Item_Table_Id", Unit_Item_Table_Id);
            //    opp.cmd.Parameters.AddWithValue("@unit_id", Unit_Id);
            //    opp.cmd.Parameters.AddWithValue("@Unit_Item_ExpireDate_Id", Unit_Item_ExpireDate_Id);
            //    //opp.cmd.Parameters.AddWithValue("@Bills_Details_Id", Bills_Details_Id);
            //    opp.cmd.Parameters.AddWithValue("@Invoice_Id", Bills_List_Id);

            //    opp.cmd.Parameters.AddWithValue("@Store_Id", Store_Id);
            //    opp.cmd.Parameters.AddWithValue("@Invoice_Item_Name", Invoice_Item_Name);


            //    opp.cmd.Parameters.AddWithValue("@Larg_Qnty", Larg_Qnty);
            //    opp.cmd.Parameters.AddWithValue("@Small_Qnty", Small_Qnty);
            //    opp.cmd.Parameters.AddWithValue("@Qnty_Another", Qnty_Another);

            //    opp.cmd.Parameters.AddWithValue("@LargItemsQnty_Free", LargItemsQnty_Free);
            //    opp.cmd.Parameters.AddWithValue("@SmallItemsQnty_Free", SmallItemsQnty_Free);
            //    opp.cmd.Parameters.AddWithValue("@Item_No_Free", Item_No_Free);
            //    opp.cmd.Parameters.AddWithValue("@Larg_Unit_Price", Larg_Unit_Price);
            //    opp.cmd.Parameters.AddWithValue("@Small_Unit_Price", Small_Unit_Price);
            //    opp.cmd.Parameters.AddWithValue("@Qnty_value", Qnty_value);


            //    opp.cmd.Parameters.AddWithValue("@ATS05", Item_ATS05);
            //    opp.cmd.Parameters.AddWithValue("@ATS05_Value", Item_ATS05_Value);
            //    opp.cmd.Parameters.AddWithValue("@TaxSales10", Item_TaxSales10);
            //    opp.cmd.Parameters.AddWithValue("@TaxSales10_Value", Item_TaxSales10_Value);
            //    opp.cmd.Parameters.AddWithValue("@Item_Total_WithoutTax", Item_Total_WithoutTax);
            //    opp.cmd.Parameters.AddWithValue("@Total", Item_Net_Total_Price);
            //    opp.cmd.Parameters.AddWithValue("@Item_Net_Total_Price", Item_Net_Total_Price);


            //    opp.cmd.Parameters.AddWithValue("@Item_ATS05", Item_ATS05);
            //    opp.cmd.Parameters.AddWithValue("@Item_ATS05_Value", Item_ATS05_Value);
            //    opp.cmd.Parameters.AddWithValue("@Item_TaxSales10", Item_TaxSales10);
            //    opp.cmd.Parameters.AddWithValue("@Item_TaxSales10_Value", Item_TaxSales10_Value);


            //    opp.cmd.Parameters.AddWithValue("@Item_Notes", Item_Notes);



            //    opp.cmd.Parameters.AddWithValue("@Unit_Used_for_Price", Unit_Used_for_Price);

            //    opp.cmd.Parameters.AddWithValue("@OperationNo", OperationNo);
            //    opp.cmd.Parameters.AddWithValue("@StatementNo", StatementNo);
            //    opp.cmd.Parameters.AddWithValue("@Merchant_Name", Merchant_Name);
            //    opp.cmd.Parameters.AddWithValue("@QntyWidth", QntyWidth);
            //    opp.cmd.Parameters.AddWithValue("@QntyLenght", QntyLenght);
            //    opp.cmd.Parameters.AddWithValue("@QntyItemNo", QntyItemNo);
            //    opp.cmd.Parameters.AddWithValue("@Specification", Specification);
            //    opp.cmd.Parameters.AddWithValue("@NameSpecification", NameSpecification);


            //    opp.cmd.Parameters.AddWithValue("@code", Item_Code);
            //    opp.cmd.Parameters.AddWithValue("@Notes", Item_Notes);
            //    opp.cmd.Parameters.AddWithValue("@Balance_Checked", Balance_Checked);

            //    opp.cmd.Parameters.AddWithValue("@Make_Order", Make_Order);

            //    opp.cmd.Parameters.AddWithValue("@Is_InvoiceItem_Kta3y", Is_InvoiceItem_Kta3y);

            //    opp.cmd.Parameters.AddWithValue("@Audience_price", Audience_price);
            //    opp.cmd.Parameters.AddWithValue("@Tash3ela", Tash3ela);

            //    opp.cmd.Parameters.AddWithValue("@Item_Expire_Date", Item_Expire_Date);
            //    opp.cmd.Parameters.AddWithValue("@Is_Item_CommissionValue", Is_Item_CommissionValue);
            //    opp.cmd.Parameters.AddWithValue("@Item_CommissionValue", Item_CommissionValue);
            //    opp.cmd.Parameters.AddWithValue("@Unit_CommissionValue", Unit_CommissionValue);
            //    opp.cmd.Parameters.AddWithValue("@Item_CommissionRate", Item_CommissionRate);

            //    opp.cmd.Parameters.AddWithValue("@Is_Alternate_Item_CommissionValue", Is_Alternate_Item_CommissionValue);
            //    opp.cmd.Parameters.AddWithValue("@Alternate_Item_CommissionValue", Alternate_Item_CommissionValue);
            //    opp.cmd.Parameters.AddWithValue("@Alternate_Unit_CommissionValue", Alternate_Unit_CommissionValue);
            //    opp.cmd.Parameters.AddWithValue("@Alternate_Item_CommissionRate", Alternate_Item_CommissionRate);
            //    opp.cmd.Parameters.AddWithValue("@Alternate_ItemCommissionTotal", Alternate_ItemCommissionTotal);
            //    opp.cmd.Parameters.AddWithValue("@ItemCommissionTotal", ItemCommissionTotal);
            //    opp.cmd.Parameters.AddWithValue("@Net_Total_Item_Commission", Net_Total_Item_Commission);

            //    opp.cmd.Parameters.AddWithValue("@QntyStr", QntyStr);
            //    opp.cmd.Parameters.AddWithValue("@QntyStrColumn", QntyStr);


            //    opp.cmd.Parameters.AddWithValue("@TotalSmall_Qnty", TotalSmall_Qnty);
            //    opp.cmd.Parameters.AddWithValue("@TotalSmall_QntyStr", TotalSmall_QntyStr);
            //    opp.cmd.Parameters.AddWithValue("@TotalSmall_QntyStrColumn", TotalSmall_QntyStrColumn);




            //    Bills_Details_Id = opp.Insert_me_Return_Last_Identify(mysql);
            //    Excp = opp.Excp == 0 ? 0 : 1;

            //}
            //catch (Exception ee)
            //{

            //    Excp = 1;
            //}
            //return Bills_Details_Id;
        }

        public long Insert_Invoices_Details(ref AccountModelsLib.operations opp, ref int Excp, Bills_Details ind)
        {
            opp.db.Bills_Details.Add(ind);
            opp.db.SaveChanges();

            return ind.Id;
        }
        public void Get_Item_Details(DataSet Invoice_ds, int x)
        {
            ////Bills_List_Id = long.Parse(Invoice_ds.Tables[0].Rows[x]["Id"].ToString());
            //Invoice_Item_Name = Invoice_ds.Tables[0].Rows[x]["Item_Name"].ToString();
            //Specification = Invoice_ds.Tables[0].Rows[x]["Specification"].ToString();
            //NameSpecification = Invoice_ds.Tables[0].Rows[x]["Item_Name"].ToString() + "  " + Invoice_ds.Tables[0].Rows[x]["Specification"].ToString();
            //Item_Id = long.Parse(Invoice_ds.Tables[0].Rows[x]["Item_id"].ToString());
            //Store_Id = long.Parse(Invoice_ds.Tables[0].Rows[x]["Store_Id"].ToString());

            //Larg_Qnty = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Larg_Qnty"].ToString());
            //Small_Qnty = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Small_Qnty"].ToString());
            //Qnty_Another = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Qnty_Another"].ToString());
            //Larg_Unit_Price = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Larg_Unit_price"].ToString());
            //Small_Unit_Price = 0;
            //Qnty_value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Qnty_value"].ToString());
            //Item_Net_Total_Price = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_Net_Total_Price"].ToString());
            //Item_Total_WithoutTax = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_Total_WithoutTax"].ToString());

            //Item_ATS05 = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_ATS05"].ToString());
            //Item_ATS05_Value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_ATS05_Value"].ToString());
            //Item_TaxSales10 = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_TaxSales10"].ToString());
            //Item_TaxSales10_Value = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_TaxSales10_Value"].ToString());
            //try
            //{
            //    Unit_Used_for_Price = int.Parse(Invoice_ds.Tables[0].Rows[x]["Unit_Used_for_Price"].ToString());
            //}
            //catch { Unit_Used_for_Price = 0; }

            //Unit_Id = long.Parse(Invoice_ds.Tables[0].Rows[x]["unit_id"].ToString());
            //Unit_Item_Table_Id = long.Parse(Invoice_ds.Tables[0].Rows[x]["Unit_Item_Table_Id"].ToString());
            //Item_Code = Invoice_ds.Tables[0].Rows[x]["Code"].ToString();


            //StatementNo = Invoice_ds.Tables[0].Rows[x]["StatementNo"].ToString();
            //OperationNo = Invoice_ds.Tables[0].Rows[x]["OperationNo"].ToString();
            //Merchant_Name = Invoice_ds.Tables[0].Rows[x]["Merchant_Name"].ToString();

            //Tash3ela = Invoice_ds.Tables[0].Rows[x]["Tash3ela"].ToString();
            //LargItemsQnty_Free = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["LargItemsQnty_Free"].ToString());
            //SmallItemsQnty_Free = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["SmallItemsQnty_Free"].ToString());
            //Item_No_Free = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_No_Free"].ToString());


            //try
            //{
            //    Item_Expire_Date = DateTime.Parse(Invoice_ds.Tables[0].Rows[x]["Item_Expire_Date"].ToString());
            //}
            //catch { }






            //try
            //{
            //    Audience_price = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Audience_price"].ToString());
            //}
            //catch { }

            //Is_Item_CommissionValue = int.Parse(Invoice_ds.Tables[0].Rows[x]["Is_Item_CommissionValue"].ToString());
            //Item_CommissionValue = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_CommissionValue"].ToString());
            //Unit_CommissionValue = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Unit_CommissionValue"].ToString());
            //Item_CommissionRate = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Item_CommissionRate"].ToString());
            //Is_Alternate_Item_CommissionValue = int.Parse(Invoice_ds.Tables[0].Rows[x]["Is_Alternate_Item_CommissionValue"].ToString());
            //Alternate_Item_CommissionValue = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Alternate_Item_CommissionValue"].ToString());
            //Alternate_Unit_CommissionValue = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Alternate_Unit_CommissionValue"].ToString());
            //Alternate_Item_CommissionRate = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Alternate_Item_CommissionRate"].ToString());
            //Alternate_ItemCommissionTotal = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Alternate_ItemCommissionTotal"].ToString());
            //ItemCommissionTotal = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["ItemCommissionTotal"].ToString());

            //Net_Total_Item_Commission = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Net_Total_Item_Commission"].ToString());


            //QntyStr = Invoice_ds.Tables[0].Rows[x]["QntyStr"].ToString();
            //try
            //{
            //    TotalSmall_Qnty = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["TotalSmall_Qnty"].ToString());
            //}
            //catch (Exception)
            //{

            //}
            //TotalSmall_QntyStr = Invoice_ds.Tables[0].Rows[x]["TotalSmall_QntyStr"].ToString();
            //TotalSmall_QntyStrColumn = Invoice_ds.Tables[0].Rows[x]["TotalSmall_QntyStrColumn"].ToString();
            //Unit_Item_ExpireDate_Id = long.Parse(Invoice_ds.Tables[0].Rows[x]["Unit_Item_ExpireDate_Id"].ToString());

            //try
            //{
            //    Balance_Checked = int.Parse(Invoice_ds.Tables[0].Rows[x]["Balance_Checked"].ToString());
            //}
            //catch
            //{
            //    Balance_Checked = 0;
            //}


            //try
            //{
            //    Is_InvoiceItem_Kta3y = int.Parse(Invoice_ds.Tables[0].Rows[x]["Is_InvoiceItem_Kta3y"].ToString());
            //}
            //catch
            //{
            //    Is_InvoiceItem_Kta3y = -1;
            //}
            //try
            //{
            //    QntyWidth = 0;
            //    QntyLenght = 0;
            //    QntyItemNo = 0;

            //    QntyWidth = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["QntyWidth"].ToString());
            //    QntyLenght = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["QntyLenght"].ToString());
            //    QntyItemNo = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["QntyItemNo"].ToString());
            //}
            //catch
            //{

            //}
            //Item_Notes = Invoice_ds.Tables[0].Rows[x]["Item_Notes"].ToString();

            //try
            //{
            //    Bills_Details_Id = long.Parse(Invoice_ds.Tables[0].Rows[x]["Invoices_Details_Id"].ToString());
            //}
            //catch (Exception ee)
            //{
            //    Bills_Details_Id = 0;
            //}


        }
        public void Update_Bills_Details(ref AccountModelsLib.operations opp, ref int Excp, Bills_Details bd)
        {
            opp.db.Entry(bd).State = EntityState.Modified;
            opp.db.SaveChanges();
            //Excp = 0;

            //try
            //{
            //    string mysql = " Update Bills_Details Set  OperationNo='" + OperationNo + "',StatementNo='" + StatementNo + "',Merchant_Name='" + Merchant_Name + "', Larg_Qnty=" + Larg_Qnty + ", Small_Qnty=" + Small_Qnty + ",Qnty_Another='" + Qnty_Another + "', Larg_Unit_Price=" + Larg_Unit_Price + ",   Qnty_value=" + Qnty_value + ",Total=" + Item_Net_Total_Price + ",Item_Net_Total_Price=" + Item_Net_Total_Price + ", ATS05=" + Item_ATS05 + " ";
            //    mysql = mysql + " , TaxSales10=" + Item_TaxSales10 + ",TaxSales10_Value=" + Item_TaxSales10_Value + ",ATS05_Value=" + Item_ATS05_Value + ",Unit_Used_for_Price=" + Unit_Used_for_Price + " , Specification='" + Specification + "' ,Invoice_Item_Name=N'" + Invoice_Item_Name + "',NameSpecification=N'" + NameSpecification + "'";


            //    mysql = mysql + " , Item_ATS05=" + Item_TaxSales10 + ",Item_ATS05_Value=" + Item_TaxSales10 + ",Item_TaxSales10=" + Item_TaxSales10 + ",Item_TaxSales10_Value =" + Item_TaxSales10 + " ";

            //    mysql = mysql + " , QntyWidth = " + QntyWidth + ",QntyLenght = " + QntyLenght + ",QntyItemNo = " + QntyItemNo + " ";
            //    mysql = mysql + " , Audience_price=  " + Audience_price + ",Tash3ela='" + Tash3ela + "' ,Item_No_Free= " + Item_No_Free + ",LargItemsQnty_Free=" + LargItemsQnty_Free + ", SmallItemsQnty_Free=" + SmallItemsQnty_Free + " ,Item_Expire_Date='" + AccountModelsLib.operations.dateFormat(Item_Expire_Date) + "'";

            //    mysql = mysql + " ,Is_Item_CommissionValue= " + Is_Item_CommissionValue + ",Item_CommissionValue = " + Item_CommissionValue + "," + "Unit_CommissionValue = " + Unit_CommissionValue + ",Item_CommissionRate = " + Item_CommissionRate + "," + "Net_Total_Item_Commission = " + Net_Total_Item_Commission + " , ItemCommissionTotal= " + ItemCommissionTotal + " ";
            //    mysql = mysql + " ,Is_Alternate_Item_CommissionValue = " + Is_Alternate_Item_CommissionValue + ", Alternate_Item_CommissionValue = " + Alternate_Item_CommissionValue + ", Alternate_Unit_CommissionValue = " + Alternate_Unit_CommissionValue + ", Alternate_Item_CommissionRate = " + Alternate_Item_CommissionRate + ",Alternate_ItemCommissionTotal = " + Alternate_ItemCommissionTotal + "";
            //    mysql = mysql + " , QntyStr =N'" + QntyStr + "', QntyStrColumn=N'" + QntyStr + "' ";
            //    mysql = mysql + " , TotalSmall_Qnty =" + TotalSmall_Qnty + "";
            //    mysql = mysql + " , TotalSmall_QntyStr ='" + TotalSmall_QntyStr + "'";
            //    mysql = mysql + " , TotalSmall_QntyStrColumn ='" + TotalSmall_QntyStrColumn + "'";

            //    mysql = mysql + " Where Id=" + Bills_Details_Id;


            //    opp.insert_me(mysql);



            //}
            //catch (Exception ee)
            //{

            //    Excp = 1;
            //}



        }
        public long Insert_Bills_Details_Reverse(ref AccountModelsLib.operations opp, ref int Excp,Bills_Details_Reverse bdR)
        { 
        
        opp.db.Bills_Details_Reverse.Add(bdR);
            opp.db.SaveChanges();
            return bdR.Id;
        }
        public long Insert_Bills_Details (ref AccountModelsLib.operations opp, ref int Excp, Bills_Details  bd)
        {

            opp.db.Bills_Details.Add(bd);
            opp.db.SaveChanges();
            return bd.Id;
        }

        //public void  Insert_Bills_Details_Reverse(ref AccountModelsLib.operations opp, ref int Excp)
        //{
        //    //Bills_Details_Id = 0;
        //    //try
        //    //{


        //    //    opp.cmd.Parameters.Clear();
        //    //    string mysql = "";

        //    //    mysql = " INSERT INTO Bills_Details_Reverse (Unit_Item_ExpireDate_Id,OperationNo,StatementNo,Merchant_Name,Store_Id,Invoice_Item_Name,Invoice_Id,Item_Id, Larg_Qnty, Small_Qnty,Qnty_Another, Larg_Unit_Price, Small_Unit_Price, Qnty_value, ATS05, ATS05_Value,TaxSales10,TaxSales10_Value,Unit_Used_for_Price,unit_id,Unit_Item_Table_Id,code, Notes,Balance_Checked, Specification,NameSpecification,Make_Order , QntyWidth,QntyLenght,QntyItemNo, Is_InvoiceItem_Kta3y, Audience_price ,Tash3ela , Item_No_Free, LargItemsQnty_Free, SmallItemsQnty_Free ,Item_Expire_Date, Is_Item_CommissionValue,Item_CommissionValue,Unit_CommissionValue,Item_CommissionRate,Net_Total_Item_Commission,Is_Alternate_Item_CommissionValue, Alternate_Item_CommissionValue, Alternate_Unit_CommissionValue, Alternate_Item_CommissionRate, Alternate_ItemCommissionTotal,ItemCommissionTotal ,Item_ATS05,Item_ATS05_Value,Item_TaxSales10,Item_TaxSales10_Value,Item_Notes, Item_Total_WithoutTax , Item_Net_Total_Price ,Total,QntyStr,QntyStrColumn,TotalSmall_Qnty,TotalSmall_QntyStr,TotalSmall_QntyStrColumn,P_price_LargeUnit,P_price_SmallUnit) ";
        //    //    mysql = mysql + " Values (@Unit_Item_ExpireDate_Id,@OperationNo,@StatementNo,@Merchant_Name,@Store_Id,@Invoice_Item_Name,@Invoice_Id,@Item_Id,@Larg_Qnty,@Small_Qnty,@Qnty_Another,@Larg_Unit_Price,@Small_Unit_Price,@Qnty_value,@ATS05,@ATS05_Value,@TaxSales10,@TaxSales10_Value,@Unit_Used_for_Price,@unit_id,@Unit_Item_Table_Id,@code,@Notes,@Balance_Checked,@Specification,@NameSpecification,@Make_Order ,@QntyWidth,@QntyLenght,@QntyItemNo,@Is_InvoiceItem_Kta3y,@Audience_price ,@Tash3ela ,@Item_No_Free,@LargItemsQnty_Free,@SmallItemsQnty_Free ,@Item_Expire_Date,@Is_Item_CommissionValue,@Item_CommissionValue,@Unit_CommissionValue,@Item_CommissionRate,@Net_Total_Item_Commission,@Is_Alternate_Item_CommissionValue,@Alternate_Item_CommissionValue,@Alternate_Unit_CommissionValue,@Alternate_Item_CommissionRate,@Alternate_ItemCommissionTotal,@ItemCommissionTotal ,@Item_ATS05,@Item_ATS05_Value,@Item_TaxSales10,@Item_TaxSales10_Value,@Item_Notes,@Item_Total_WithoutTax ,@Item_Net_Total_Price,@Total,@QntyStr,@QntyStrColumn,@TotalSmall_Qnty,@TotalSmall_QntyStr,@TotalSmall_QntyStrColumn,@P_price_LargeUnit,@P_price_SmallUnit) ";


        //    //    P_price_LargeUnit = 0;
        //    //    P_price_SmallUnit = 0;
        //    //    if (Unit_Used_for_Price == 1)
        //    //    {

        //    //        P_price_LargeUnit = Larg_Unit_Price;

        //    //        try
        //    //        {
        //    //            //حساب سعر القطعة من الدستة
        //    //            P_price_SmallUnit = Larg_Unit_Price / Small_Unit_Value;
        //    //            P_price_SmallUnit = Common.RoundMe(P_price_SmallUnit);
        //    //        }
        //    //        catch (Exception)
        //    //        {

        //    //            P_price_SmallUnit = 0;
        //    //        }
        //    //    }
        //    //    if (Unit_Used_for_Price == 2)
        //    //    {
        //    //        P_price_SmallUnit = Larg_Unit_Price;

        //    //        //حساب سعر   الدستة
        //    //        P_price_LargeUnit = Larg_Unit_Price * Small_Unit_Value;
        //    //        P_price_LargeUnit = Common.RoundMe(P_price_LargeUnit);
        //    //    }
        //    //    //P_price_LargeUnit,P_price_SmallUnit

        //    //    opp.cmd.Parameters.AddWithValue("@P_price_LargeUnit", P_price_LargeUnit);
        //    //    opp.cmd.Parameters.AddWithValue("@P_price_SmallUnit", P_price_SmallUnit);


        //    //    opp.cmd.Parameters.AddWithValue("@Item_Id", Item_Id);

        //    //    opp.cmd.Parameters.AddWithValue("@Unit_Item_Table_Id", Unit_Item_Table_Id);
        //    //    opp.cmd.Parameters.AddWithValue("@unit_id", Unit_Id);
        //    //    opp.cmd.Parameters.AddWithValue("@Unit_Item_ExpireDate_Id", Unit_Item_ExpireDate_Id);
        //    //    //opp.cmd.Parameters.AddWithValue("@Bills_Details_Id", Bills_Details_Id);
        //    //    opp.cmd.Parameters.AddWithValue("@Invoice_Id", Bills_List_Id);

        //    //    opp.cmd.Parameters.AddWithValue("@Store_Id", Store_Id);
        //    //    opp.cmd.Parameters.AddWithValue("@Invoice_Item_Name", Invoice_Item_Name);


        //    //    opp.cmd.Parameters.AddWithValue("@Larg_Qnty", Larg_Qnty);
        //    //    opp.cmd.Parameters.AddWithValue("@Small_Qnty", Small_Qnty);
        //    //    opp.cmd.Parameters.AddWithValue("@Qnty_Another", Qnty_Another);

        //    //    opp.cmd.Parameters.AddWithValue("@LargItemsQnty_Free", LargItemsQnty_Free);
        //    //    opp.cmd.Parameters.AddWithValue("@SmallItemsQnty_Free", SmallItemsQnty_Free);
        //    //    opp.cmd.Parameters.AddWithValue("@Item_No_Free", Item_No_Free);
        //    //    opp.cmd.Parameters.AddWithValue("@Larg_Unit_Price", Larg_Unit_Price);
        //    //    opp.cmd.Parameters.AddWithValue("@Small_Unit_Price", Small_Unit_Price);
        //    //    opp.cmd.Parameters.AddWithValue("@Qnty_value", Qnty_value);


        //    //    opp.cmd.Parameters.AddWithValue("@ATS05", Item_ATS05);
        //    //    opp.cmd.Parameters.AddWithValue("@ATS05_Value", Item_ATS05_Value);
        //    //    opp.cmd.Parameters.AddWithValue("@TaxSales10", Item_TaxSales10);
        //    //    opp.cmd.Parameters.AddWithValue("@TaxSales10_Value", Item_TaxSales10_Value);
        //    //    opp.cmd.Parameters.AddWithValue("@Item_Total_WithoutTax", Item_Total_WithoutTax);
        //    //    opp.cmd.Parameters.AddWithValue("@Total", Item_Net_Total_Price);
        //    //    opp.cmd.Parameters.AddWithValue("@Item_Net_Total_Price", Item_Net_Total_Price);


        //    //    opp.cmd.Parameters.AddWithValue("@Item_ATS05", Item_ATS05);
        //    //    opp.cmd.Parameters.AddWithValue("@Item_ATS05_Value", Item_ATS05_Value);
        //    //    opp.cmd.Parameters.AddWithValue("@Item_TaxSales10", Item_TaxSales10);
        //    //    opp.cmd.Parameters.AddWithValue("@Item_TaxSales10_Value", Item_TaxSales10_Value);


        //    //    opp.cmd.Parameters.AddWithValue("@Item_Notes", Item_Notes);

        //    //    //,Item_Total,Item_Notes
        //    //    //row["Item_ATS05"] = vs.Item_ATS05;
        //    //    //row["Item_ATS05_Value"] = vs.Item_ATS05_Value;
        //    //    //row["Item_TaxSales10"] = vs.Item_TaxSales10;
        //    //    //row["Item_TaxSales10_Value"] = vs.Item_TaxSales10_Value;


        //    //    opp.cmd.Parameters.AddWithValue("@Unit_Used_for_Price", Unit_Used_for_Price);

        //    //    opp.cmd.Parameters.AddWithValue("@OperationNo", OperationNo);
        //    //    opp.cmd.Parameters.AddWithValue("@StatementNo", StatementNo);
        //    //    opp.cmd.Parameters.AddWithValue("@Merchant_Name", Merchant_Name);
        //    //    opp.cmd.Parameters.AddWithValue("@QntyWidth", QntyWidth);
        //    //    opp.cmd.Parameters.AddWithValue("@QntyLenght", QntyLenght);
        //    //    opp.cmd.Parameters.AddWithValue("@QntyItemNo", QntyItemNo);
        //    //    opp.cmd.Parameters.AddWithValue("@Specification", Specification);
        //    //    opp.cmd.Parameters.AddWithValue("@NameSpecification", NameSpecification);


        //    //    opp.cmd.Parameters.AddWithValue("@code", Item_Code);
        //    //    opp.cmd.Parameters.AddWithValue("@Notes", Item_Notes);
        //    //    opp.cmd.Parameters.AddWithValue("@Balance_Checked", Balance_Checked);

        //    //    opp.cmd.Parameters.AddWithValue("@Make_Order", Make_Order);

        //    //    opp.cmd.Parameters.AddWithValue("@Is_InvoiceItem_Kta3y", Is_InvoiceItem_Kta3y);

        //    //    opp.cmd.Parameters.AddWithValue("@Audience_price", Audience_price);
        //    //    opp.cmd.Parameters.AddWithValue("@Tash3ela", Tash3ela);

        //    //    opp.cmd.Parameters.AddWithValue("@Item_Expire_Date", Item_Expire_Date);
        //    //    opp.cmd.Parameters.AddWithValue("@Is_Item_CommissionValue", Is_Item_CommissionValue);
        //    //    opp.cmd.Parameters.AddWithValue("@Item_CommissionValue", Item_CommissionValue);
        //    //    opp.cmd.Parameters.AddWithValue("@Unit_CommissionValue", Unit_CommissionValue);
        //    //    opp.cmd.Parameters.AddWithValue("@Item_CommissionRate", Item_CommissionRate);

        //    //    opp.cmd.Parameters.AddWithValue("@Is_Alternate_Item_CommissionValue", Is_Alternate_Item_CommissionValue);
        //    //    opp.cmd.Parameters.AddWithValue("@Alternate_Item_CommissionValue", Alternate_Item_CommissionValue);
        //    //    opp.cmd.Parameters.AddWithValue("@Alternate_Unit_CommissionValue", Alternate_Unit_CommissionValue);
        //    //    opp.cmd.Parameters.AddWithValue("@Alternate_Item_CommissionRate", Alternate_Item_CommissionRate);
        //    //    opp.cmd.Parameters.AddWithValue("@Alternate_ItemCommissionTotal", Alternate_ItemCommissionTotal);
        //    //    opp.cmd.Parameters.AddWithValue("@ItemCommissionTotal", ItemCommissionTotal);
        //    //    opp.cmd.Parameters.AddWithValue("@Net_Total_Item_Commission", Net_Total_Item_Commission);

        //    //    opp.cmd.Parameters.AddWithValue("@QntyStr", QntyStr);
        //    //    opp.cmd.Parameters.AddWithValue("@QntyStrColumn", QntyStr);

        //    //    opp.cmd.Parameters.AddWithValue("@TotalSmall_Qnty", TotalSmall_Qnty);
        //    //    opp.cmd.Parameters.AddWithValue("@TotalSmall_QntyStr", TotalSmall_QntyStr);
        //    //    opp.cmd.Parameters.AddWithValue("@TotalSmall_QntyStrColumn", TotalSmall_QntyStrColumn);


        //    //    Bills_Details_Id = opp.Insert_me_Return_Last_Identify(mysql);
        //    //    Excp = opp.Excp == 0 ? 0 : 1;

        //    //}
        //    //catch (Exception ee)
        //    //{

        //    //    Excp = 1;
        //    //}
        //    //return Bills_Details_Id;
        //}


        public void Update_Bills_Details_Reverse(ref AccountModelsLib.operations opp, ref int Excp, Bills_Details_Reverse bd)
        {
            opp.db.Entry(bd).State = EntityState.Modified;   
            opp.db.SaveChanges();
            //Excp = 0;

            //try
            //{
            //    string mysql = " Update Invoices_Details_Reverse Set  OperationNo='" + OperationNo + "',StatementNo='" + StatementNo + "',Merchant_Name='" + Merchant_Name + "', Larg_Qnty=" + Larg_Qnty + ", Small_Qnty=" + Small_Qnty + ",Qnty_Another='" + Qnty_Another + "', Larg_Unit_Price=" + Larg_Unit_Price + ",   Qnty_value=" + Qnty_value + ",Total=" + Item_Net_Total_Price + ",Item_Net_Total_Price=" + Item_Net_Total_Price + ", ATS05=" + Item_ATS05 + " ";
            //    mysql = mysql + " , TaxSales10=" + Item_TaxSales10 + ",TaxSales10_Value=" + Item_TaxSales10_Value + ",ATS05_Value=" + Item_ATS05_Value + ",Unit_Used_for_Price=" + Unit_Used_for_Price + " , Specification='" + Specification + "' ,Invoice_Item_Name=N'" + Invoice_Item_Name + "',NameSpecification=N'" + NameSpecification + "'";


            //    mysql = mysql + " , Item_ATS05=" + Item_TaxSales10 + ",Item_ATS05_Value=" + Item_TaxSales10 + ",Item_TaxSales10=" + Item_TaxSales10 + ",Item_TaxSales10_Value =" + Item_TaxSales10 + " ";

            //    mysql = mysql + " , QntyWidth = " + QntyWidth + ",QntyLenght = " + QntyLenght + ",QntyItemNo = " + QntyItemNo + " ";
            //    mysql = mysql + " , Audience_price=  " + Audience_price + ",Tash3ela='" + Tash3ela + "' ,Item_No_Free= " + Item_No_Free + ",LargItemsQnty_Free=" + LargItemsQnty_Free + ", SmallItemsQnty_Free=" + SmallItemsQnty_Free + " ,Item_Expire_Date='" + AccountModelsLib.operations.dateFormat(Item_Expire_Date) + "'";

            //    mysql = mysql + " ,Is_Item_CommissionValue= " + Is_Item_CommissionValue + ",Item_CommissionValue = " + Item_CommissionValue + "," + "Unit_CommissionValue = " + Unit_CommissionValue + ",Item_CommissionRate = " + Item_CommissionRate + "," + "Net_Total_Item_Commission = " + Net_Total_Item_Commission + " , ItemCommissionTotal= " + ItemCommissionTotal + " ";
            //    mysql = mysql + " ,Is_Alternate_Item_CommissionValue = " + Is_Alternate_Item_CommissionValue + ", Alternate_Item_CommissionValue = " + Alternate_Item_CommissionValue + ", Alternate_Unit_CommissionValue = " + Alternate_Unit_CommissionValue + ", Alternate_Item_CommissionRate = " + Alternate_Item_CommissionRate + ",Alternate_ItemCommissionTotal = " + Alternate_ItemCommissionTotal + "";
            //    mysql = mysql + " , QntyStr =N'" + QntyStr + "', QntyStrColumn=N'" + QntyStr + "' ";

            //    mysql = mysql + " , TotalSmall_Qnty =" + TotalSmall_Qnty + "";
            //    mysql = mysql + " , TotalSmall_QntyStr ='" + TotalSmall_QntyStr + "'";
            //    mysql = mysql + " , TotalSmall_QntyStrColumn ='" + TotalSmall_QntyStrColumn + "'";

            //    mysql = mysql + " Where Id=" + Bills_Details_Id;
            //    opp.insert_me(mysql);



            //}
            //catch (Exception ee)
            //{

            //    Excp = 1;
            //}


        }

    }
}
