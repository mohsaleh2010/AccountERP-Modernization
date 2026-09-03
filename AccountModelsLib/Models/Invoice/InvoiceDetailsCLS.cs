using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace AccountModelsLib.Models 
{
    public class InvoiceDetailsCLS  
    {
        public InvoiceDetailsCLS()
        {

        }
        public long Insert_Invoices_Details(ref operations opp, ref int Excp, Invoices_Details ind)
        {
            opp.db.Invoices_Details.Add(ind);
            opp.db.SaveChanges();

            return ind.Id;
        }
        public long Insert_Invoices_DetailsOld(ref operations opp, ref int Excp, IInvoices_Details invD)
        {
            long Invoices_Details_Id = 0;

            /*
            try
            {

                opp.cmd.Parameters.Clear();
                string mysql = "";

                mysql = " INSERT INTO Invoices_Details (Unit_Item_ExpireDate_Id,Purchase_Price,OperationNo,StatementNo,Merchant_Name,Store_Id,Invoice_Item_Name,Invoice_Id,Item_Id, Larg_Qnty, Small_Qnty,Qnty_Another, Larg_Unit_Price, Small_Unit_Price, Qnty_value, ATS05, ATS05_Value,TaxSales10,TaxSales10_Value,Unit_Used_for_Price,unit_id,Unit_Item_Table_Id,code, Notes,Balance_Checked, Specification,NameSpecification,Make_Order , QntyWidth,QntyLenght,QntyItemNo, Is_InvoiceItem_Kta3y,Bills_Details_Id, Audience_price ,Tash3ela , Item_No_Free, LargItemsQnty_Free, SmallItemsQnty_Free ,Item_Expire_Date, Is_Item_CommissionValue,Item_CommissionValue,Unit_CommissionValue,Item_CommissionRate,Net_Total_Item_Commission,Is_Alternate_Item_CommissionValue, Alternate_Item_CommissionValue, Alternate_Unit_CommissionValue, Alternate_Item_CommissionRate, Alternate_ItemCommissionTotal,ItemCommissionTotal ,Item_ATS05,Item_ATS05_Value,Item_TaxSales10,Item_TaxSales10_Value,Item_Notes, Item_Total_WithoutTax , Item_Net_Total_Price ,Total,QntyStr,QntyStrColumn,TotalSmall_Qnty,TotalSmall_QntyStr,TotalSmall_QntyStrColumn,S_price_LargeUnit,S_price_SmallUnit,Qnty) ";
                mysql = mysql + " Values (@Unit_Item_ExpireDate_Id,@Purchase_Price,@OperationNo,@StatementNo,@Merchant_Name,@Store_Id,@Invoice_Item_Name,@Invoice_Id,@Item_Id,@Larg_Qnty,@Small_Qnty,@Qnty_Another,@Larg_Unit_Price,@Small_Unit_Price,@Qnty_value,@ATS05,@ATS05_Value,@TaxSales10,@TaxSales10_Value,@Unit_Used_for_Price,@unit_id,@Unit_Item_Table_Id,@code,@Notes,@Balance_Checked,@Specification,@NameSpecification,@Make_Order ,@QntyWidth,@QntyLenght,@QntyItemNo,@Is_InvoiceItem_Kta3y,@Bills_Details_Id,@Audience_price ,@Tash3ela ,@Item_No_Free,@LargItemsQnty_Free,@SmallItemsQnty_Free ,@Item_Expire_Date,@Is_Item_CommissionValue,@Item_CommissionValue,@Unit_CommissionValue,@Item_CommissionRate,@Net_Total_Item_Commission,@Is_Alternate_Item_CommissionValue,@Alternate_Item_CommissionValue,@Alternate_Unit_CommissionValue,@Alternate_Item_CommissionRate,@Alternate_ItemCommissionTotal,@ItemCommissionTotal ,@Item_ATS05,@Item_ATS05_Value,@Item_TaxSales10,@Item_TaxSales10_Value,@Item_Notes,@Item_Total_WithoutTax ,@Item_Net_Total_Price,@Total,@QntyStr,@QntyStrColumn,@TotalSmall_Qnty,@TotalSmall_QntyStr,@TotalSmall_QntyStrColumn,@S_price_LargeUnit,@S_price_SmallUnit,@Qnty) ";




                invD.S_price_LargeUnit = 0;
                invD.S_price_SmallUnit = 0;
                if (invD.Unit_Used_for_Price == 1)
                {

                    invD.S_price_LargeUnit = invD.Larg_Unit_Price;

                    try
                    {
                        //حساب سعر القطعة من الدستة
                        invD.S_price_SmallUnit = invD.Larg_Unit_Price / Small_Unit_Value;
                        invD.S_price_SmallUnit = Common.RoundMe(invD.S_price_SmallUnit);
                    }
                    catch (Exception)
                    {

                        invD.S_price_SmallUnit = 0;
                    }
                }
                if (invD.Unit_Used_for_Price == 2)
                {
                    invD.S_price_SmallUnit = invD.Larg_Unit_Price;

                    //حساب سعر   الدستة
                    invD.S_price_LargeUnit = invD.Larg_Unit_Price * Small_Unit_Value;
                    invD.S_price_LargeUnit = Common.RoundMe(invD.S_price_LargeUnit);
                }
                //S_price_LargeUnit,S_price_SmallUnit

                opp.cmd.Parameters.AddWithValue("@S_price_LargeUnit ", invD.S_price_LargeUnit);
                opp.cmd.Parameters.AddWithValue("@S_price_SmallUnit ", invD.S_price_SmallUnit);



                opp.cmd.Parameters.AddWithValue("@Item_Id ", invD.Item_Id);

                opp.cmd.Parameters.AddWithValue("@Unit_Item_Table_Id ", invD.Unit_Item_Table_Id);
                opp.cmd.Parameters.AddWithValue("@unit_id ", invD.Unit_Id);
                opp.cmd.Parameters.AddWithValue("@Unit_Item_ExpireDate_Id ", invD.Unit_Item_ExpireDate_Id);
                opp.cmd.Parameters.AddWithValue("@Purchase_Price ", invD.Purchase_Price);


                opp.cmd.Parameters.AddWithValue("@Bills_Details_Id ", invD.Bills_Details_Id);
                opp.cmd.Parameters.AddWithValue("@Invoice_Id ", Invoices_List_Id);

                opp.cmd.Parameters.AddWithValue("@Store_Id ", invD.Store_Id);
                opp.cmd.Parameters.AddWithValue("@Invoice_Item_Name ", invD.Invoice_Item_Name);


                opp.cmd.Parameters.AddWithValue("@Larg_Qnty ", invD.Larg_Qnty);
                opp.cmd.Parameters.AddWithValue("@Small_Qnty ", invD.Small_Qnty);
                opp.cmd.Parameters.AddWithValue("@Qnty_Another ", invD.Qnty_Another);

                opp.cmd.Parameters.AddWithValue("@LargItemsQnty_Free ", invD.LargItemsQnty_Free);
                opp.cmd.Parameters.AddWithValue("@SmallItemsQnty_Free ", invD.SmallItemsQnty_Free);
                opp.cmd.Parameters.AddWithValue("@Item_No_Free ", invD.Item_No_Free);
                opp.cmd.Parameters.AddWithValue("@Larg_Unit_Price ", invD.Larg_Unit_Price);
                opp.cmd.Parameters.AddWithValue("@Small_Unit_Price ", invD.Small_Unit_Price);
                opp.cmd.Parameters.AddWithValue("@Qnty_value ", invD.Qnty_value);


                opp.cmd.Parameters.AddWithValue("@ATS05 ", invD.Item_ATS05);
                opp.cmd.Parameters.AddWithValue("@ATS05_Value ", invD.Item_ATS05_Value);
                opp.cmd.Parameters.AddWithValue("@TaxSales10 ", invD.Item_TaxSales10);
                opp.cmd.Parameters.AddWithValue("@TaxSales10_Value ", invD.Item_TaxSales10_Value);
                opp.cmd.Parameters.AddWithValue("@Item_Total_WithoutTax ", invD.Item_Total_WithoutTax);
                opp.cmd.Parameters.AddWithValue("@Total ", invD.Item_Net_Total_Price);
                opp.cmd.Parameters.AddWithValue("@Item_Net_Total_Price ", invD.Item_Net_Total_Price);


                opp.cmd.Parameters.AddWithValue("@Item_ATS05 ", invD.Item_ATS05);
                opp.cmd.Parameters.AddWithValue("@Item_ATS05_Value ", invD.Item_ATS05_Value);
                opp.cmd.Parameters.AddWithValue("@Item_TaxSales10 ", invD.Item_TaxSales10);
                opp.cmd.Parameters.AddWithValue("@Item_TaxSales10_Value ", invD.Item_TaxSales10_Value);


                opp.cmd.Parameters.AddWithValue("@Item_Notes ", invD.Item_Notes);

                //,Item_Total,Item_Notes
                //row["Item_ATS05"] = vs.Item_ATS05;
                //row["Item_ATS05_Value"] = vs.Item_ATS05_Value;
                //row["Item_TaxSales10"] = vs.Item_TaxSales10;
                //row["Item_TaxSales10_Value"] = vs.Item_TaxSales10_Value;


                opp.cmd.Parameters.AddWithValue("@Unit_Used_for_Price ", invD.Unit_Used_for_Price);

                opp.cmd.Parameters.AddWithValue("@OperationNo ", invD.OperationNo);
                opp.cmd.Parameters.AddWithValue("@StatementNo ", invD.StatementNo);
                opp.cmd.Parameters.AddWithValue("@Merchant_Name ", invD.Merchant_Name);
                opp.cmd.Parameters.AddWithValue("@QntyWidth ", invD.QntyWidth);
                opp.cmd.Parameters.AddWithValue("@QntyLenght ", invD.QntyLenght);
                opp.cmd.Parameters.AddWithValue("@QntyItemNo ", invD.QntyItemNo);
                opp.cmd.Parameters.AddWithValue("@Specification ", invD.Specification);
                opp.cmd.Parameters.AddWithValue("@NameSpecification ", invD.NameSpecification);


                opp.cmd.Parameters.AddWithValue("@code ", invD.code);
                opp.cmd.Parameters.AddWithValue("@Notes ", invD.Item_Notes);
                opp.cmd.Parameters.AddWithValue("@Balance_Checked ", invD.Balance_Checked);

                opp.cmd.Parameters.AddWithValue("@Make_Order ", invD.Make_Order);

                opp.cmd.Parameters.AddWithValue("@Is_InvoiceItem_Kta3y ", invD.Is_InvoiceItem_Kta3y);

                opp.cmd.Parameters.AddWithValue("@Audience_price ", invD.Audience_price);
                opp.cmd.Parameters.AddWithValue("@Tash3ela ", invD.Tash3ela);

                opp.cmd.Parameters.AddWithValue("@Item_Expire_Date ", invD.Item_Expire_Date);
                opp.cmd.Parameters.AddWithValue("@Is_Item_CommissionValue ", invD.Is_Item_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Item_CommissionValue ", invD.Item_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Unit_CommissionValue ", invD.Unit_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Item_CommissionRate ", invD.Item_CommissionRate);

                opp.cmd.Parameters.AddWithValue("@Is_Alternate_Item_CommissionValue ", invD.Is_Alternate_Item_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Alternate_Item_CommissionValue ", invD.Alternate_Item_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Alternate_Unit_CommissionValue ", invD.Alternate_Unit_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Alternate_Item_CommissionRate ", invD.Alternate_Item_CommissionRate);
                opp.cmd.Parameters.AddWithValue("@Alternate_ItemCommissionTotal ", invD.Alternate_ItemCommissionTotal);
                opp.cmd.Parameters.AddWithValue("@ItemCommissionTotal ", invD.ItemCommissionTotal);
                opp.cmd.Parameters.AddWithValue("@Net_Total_Item_Commission ", invD.Net_Total_Item_Commission);

                opp.cmd.Parameters.AddWithValue("@QntyStr ", invD.QntyStr);
                opp.cmd.Parameters.AddWithValue("@QntyStrColumn ", invD.QntyStr);
                opp.cmd.Parameters.AddWithValue("@TotalSmall_Qnty ", invD.TotalSmall_Qnty);
                opp.cmd.Parameters.AddWithValue("@TotalSmall_QntyStr ", invD.TotalSmall_QntyStr);
                opp.cmd.Parameters.AddWithValue("@TotalSmall_QntyStrColumn ", invD.TotalSmall_QntyStrColumn);
                opp.cmd.Parameters.AddWithValue("@Qnty ", invD.Qnty);



                Invoices_Details_Id = opp.Insert_me_Return_Last_Identify(mysql);
                Excp = 0;

            }
            catch (Exception ee)
            {

                Excp = 1;
            }

            */
            return Invoices_Details_Id;

            
        }



        public void Update_Invoices_DetailsOld(ref operations opp, ref int Excp, IInvoices_Details invD)
        {
            Excp = 0;
            /*
            try
            {
                string mysql = " Update Invoices_Details Set  OperationNo='" + invD.OperationNo + "',StatementNo='" + invD.StatementNo + "',Merchant_Name='" + invD.Merchant_Name + "', Larg_Qnty=" + invD.Larg_Qnty + ", Small_Qnty=" + invD.Small_Qnty + ",Qnty_Another='" + invD.Qnty_Another + "', Larg_Unit_Price=" + invD.Larg_Unit_Price + ",   Qnty_value=" + invD.Qnty_value + ",Total=" + invD.Item_Net_Total_Price + ",Item_Net_Total_Price=" + invD.Item_Net_Total_Price + ", ATS05=" + invD.Item_ATS05 + " ";
                mysql = mysql + " , TaxSales10=" + invD.Item_TaxSales10 + ",TaxSales10_Value=" + invD.Item_TaxSales10_Value + ",ATS05_Value=" + invD.Item_ATS05_Value + ",Unit_Used_for_Price=" + invD.Unit_Used_for_Price + " , Specification='" + invD.Specification + "' ,Invoice_Item_Name=N'" + invD.Invoice_Item_Name + "',NameSpecification=N'" + invD.NameSpecification + "'";


                mysql = mysql + " , Item_ATS05=" + invD.Item_ATS05 + ",Item_ATS05_Value=" + invD.Item_ATS05_Value + ",Item_TaxSales10=" + invD.Item_TaxSales10 + ",Item_TaxSales10_Value =" + invD.Item_TaxSales10_Value + " ";

                mysql = mysql + " , QntyWidth=" + invD.QntyWidth + ",QntyLenght=" + invD.QntyLenght + ",QntyItemNo=" + invD.QntyItemNo + " ";
                mysql = mysql + " , Audience_price=" + invD.Audience_price + ",Tash3ela='" + invD.Tash3ela + "' ,Item_No_Free= " + invD.Item_No_Free + ",LargItemsQnty_Free=" + invD.LargItemsQnty_Free + ", SmallItemsQnty_Free=" + invD.SmallItemsQnty_Free + " ,Item_Expire_Date='" + operations.dateFormat(invD.Item_Expire_Date) + "'";

                mysql = mysql + " ,Is_Item_CommissionValue= " + invD.Is_Item_CommissionValue + ",Item_CommissionValue=" + invD.Item_CommissionValue + "," + "Unit_CommissionValue=" + invD.Unit_CommissionValue + ",Item_CommissionRate=" + invD.Item_CommissionRate + "," + "Net_Total_Item_Commission=" + invD.Net_Total_Item_Commission + " , ItemCommissionTotal= " + invD.ItemCommissionTotal + " ";
                mysql = mysql + " ,Is_Alternate_Item_CommissionValue=" + invD.Is_Alternate_Item_CommissionValue + ", Alternate_Item_CommissionValue=" + invD.Alternate_Item_CommissionValue + ", Alternate_Unit_CommissionValue=" + invD.Alternate_Unit_CommissionValue + ", Alternate_Item_CommissionRate=" + invD.Alternate_Item_CommissionRate + ",Alternate_ItemCommissionTotal=" + invD.Alternate_ItemCommissionTotal + "";
                mysql = mysql + " , QntyStr =N'" + invD.QntyStr + "', QntyStrColumn=N'" + invD.QntyStr + "' ";
                mysql = mysql + " , Item_Total_WithoutTax =" + invD.Item_Total_WithoutTax + "";

                mysql = mysql + " , TotalSmall_Qnty =" + invD.TotalSmall_Qnty + "";
                mysql = mysql + " , TotalSmall_QntyStr ='" + invD.TotalSmall_QntyStr + "'";
                mysql = mysql + " , TotalSmall_QntyStrColumn ='" + invD.TotalSmall_QntyStrColumn + "'";

                mysql = mysql + " Where Id=" + invD.Id;


                opp.insert_me(mysql);


            }
            catch (Exception ee)
            {

                Excp = 1;
            }

            */

        }

        public void Update_Invoices_Details(ref operations opp, ref int Excp, Invoices_Details invD)
        {
            Excp = 0;

            try
            {

                opp.db.Entry(invD).State = EntityState.Modified;
                opp.db.SaveChanges();

            }
            catch (Exception ee)
            {

                Excp = 1;
            }



        }


        public long Insert_Invoices_Details_Reverse(ref operations opp, ref int Excp)
        {
            long Invoices_Details_Id = 0;
            /*
            try
            {

                opp.cmd.Parameters.Clear();
                string mysql = "";

                mysql = " INSERT INTO Invoices_Details_Reverse (Unit_Item_ExpireDate_Id,OperationNo,StatementNo,Merchant_Name,Store_Id,Invoice_Item_Name,Invoice_Id,Item_Id, Larg_Qnty, Small_Qnty,Qnty_Another, Larg_Unit_Price, Small_Unit_Price, Qnty_value, ATS05, ATS05_Value,TaxSales10,TaxSales10_Value,Unit_Used_for_Price,unit_id,Unit_Item_Table_Id,code, Notes,Balance_Checked, Specification,NameSpecification,Make_Order , QntyWidth,QntyLenght,QntyItemNo, Is_InvoiceItem_Kta3y,Bills_Details_Id, Audience_price ,Tash3ela , Item_No_Free, LargItemsQnty_Free, SmallItemsQnty_Free ,Item_Expire_Date, Is_Item_CommissionValue,Item_CommissionValue,Unit_CommissionValue,Item_CommissionRate,Net_Total_Item_Commission,Is_Alternate_Item_CommissionValue, Alternate_Item_CommissionValue, Alternate_Unit_CommissionValue, Alternate_Item_CommissionRate, Alternate_ItemCommissionTotal,ItemCommissionTotal ,Item_ATS05,Item_ATS05_Value,Item_TaxSales10,Item_TaxSales10_Value,Item_Notes, Item_Total_WithoutTax , Item_Net_Total_Price ,Total,QntyStr,QntyStrColumn,TotalSmall_Qnty,TotalSmall_QntyStr,TotalSmall_QntyStrColumn,S_price_LargeUnit,S_price_SmallUnit) ";
                mysql = mysql + " Values (@Unit_Item_ExpireDate_Id,@OperationNo,@StatementNo,@Merchant_Name,@Store_Id,@Invoice_Item_Name,@Invoice_Id,@Item_Id,@Larg_Qnty,@Small_Qnty,@Qnty_Another,@Larg_Unit_Price,@Small_Unit_Price,@Qnty_value,@ATS05,@ATS05_Value,@TaxSales10,@TaxSales10_Value,@Unit_Used_for_Price,@unit_id,@Unit_Item_Table_Id,@code,@Notes,@Balance_Checked,@Specification,@NameSpecification,@Make_Order ,@QntyWidth,@QntyLenght,@QntyItemNo,@Is_InvoiceItem_Kta3y,@Bills_Details_Id,@Audience_price ,@Tash3ela ,@Item_No_Free,@LargItemsQnty_Free,@SmallItemsQnty_Free ,@Item_Expire_Date,@Is_Item_CommissionValue,@Item_CommissionValue,@Unit_CommissionValue,@Item_CommissionRate,@Net_Total_Item_Commission,@Is_Alternate_Item_CommissionValue,@Alternate_Item_CommissionValue,@Alternate_Unit_CommissionValue,@Alternate_Item_CommissionRate,@Alternate_ItemCommissionTotal,@ItemCommissionTotal ,@Item_ATS05,@Item_ATS05_Value,@Item_TaxSales10,@Item_TaxSales10_Value,@Item_Notes,@Item_Total_WithoutTax ,@Item_Net_Total_Price,@Total,@QntyStr,@QntyStrColumn,@TotalSmall_Qnty,@TotalSmall_QntyStr,@TotalSmall_QntyStrColumn,@S_price_LargeUnit,@S_price_SmallUnit) ";



                invD.S_price_LargeUnit = 0;
                invD.S_price_SmallUnit = 0;
                if (invD.Unit_Used_for_Price == 1)
                {

                    invD.S_price_LargeUnit = invD.Larg_Unit_Price;

                    try
                    {
                        //حساب سعر القطعة من الدستة
                        invD.S_price_SmallUnit = invD.Larg_Unit_Price / Small_Unit_Value;
                        invD.S_price_SmallUnit = Common.RoundMe(invD.S_price_SmallUnit);
                    }
                    catch (Exception)
                    {

                        invD.S_price_SmallUnit = 0;
                    }
                }
                if (invD.Unit_Used_for_Price == 2)
                {
                    invD.S_price_SmallUnit = invD.Larg_Unit_Price;

                    //حساب سعر   الدستة
                    invD.S_price_LargeUnit = invD.Larg_Unit_Price * Small_Unit_Value;
                    invD.S_price_LargeUnit = Common.RoundMe(invD.S_price_LargeUnit);
                }
                //S_price_LargeUnit,S_price_SmallUnit

                opp.cmd.Parameters.AddWithValue("@S_price_LargeUnit ", invD.S_price_LargeUnit);
                opp.cmd.Parameters.AddWithValue("@S_price_SmallUnit ", invD.S_price_SmallUnit);



                opp.cmd.Parameters.AddWithValue("@Item_Id ", invD.Item_Id);

                opp.cmd.Parameters.AddWithValue("@Unit_Item_Table_Id ", invD.Unit_Item_Table_Id);
                opp.cmd.Parameters.AddWithValue("@unit_id ", invD.Unit_Id);
                opp.cmd.Parameters.AddWithValue("@Unit_Item_ExpireDate_Id ", invD.Unit_Item_ExpireDate_Id);
                opp.cmd.Parameters.AddWithValue("@Bills_Details_Id ", invD.Bills_Details_Id);
                opp.cmd.Parameters.AddWithValue("@Invoice_Id ", Invoices_List_Id);

                opp.cmd.Parameters.AddWithValue("@Store_Id ", invD.Store_Id);
                opp.cmd.Parameters.AddWithValue("@Invoice_Item_Name ", invD.Invoice_Item_Name);


                opp.cmd.Parameters.AddWithValue("@Larg_Qnty ", invD.Larg_Qnty);
                opp.cmd.Parameters.AddWithValue("@Small_Qnty ", invD.Small_Qnty);
                opp.cmd.Parameters.AddWithValue("@Qnty_Another ", invD.Qnty_Another);

                opp.cmd.Parameters.AddWithValue("@LargItemsQnty_Free ", invD.LargItemsQnty_Free);
                opp.cmd.Parameters.AddWithValue("@SmallItemsQnty_Free ", invD.SmallItemsQnty_Free);
                opp.cmd.Parameters.AddWithValue("@Item_No_Free ", invD.Item_No_Free);
                opp.cmd.Parameters.AddWithValue("@Larg_Unit_Price ", invD.Larg_Unit_Price);
                opp.cmd.Parameters.AddWithValue("@Small_Unit_Price ", invD.Small_Unit_Price);
                opp.cmd.Parameters.AddWithValue("@Qnty_value ", invD.Qnty_value);


                opp.cmd.Parameters.AddWithValue("@ATS05 ", invD.Item_ATS05);
                opp.cmd.Parameters.AddWithValue("@ATS05_Value ", invD.Item_ATS05_Value);
                opp.cmd.Parameters.AddWithValue("@TaxSales10 ", invD.Item_TaxSales10);
                opp.cmd.Parameters.AddWithValue("@TaxSales10_Value ", invD.Item_TaxSales10_Value);
                opp.cmd.Parameters.AddWithValue("@Item_Total_WithoutTax ", invD.Item_Total_WithoutTax);
                opp.cmd.Parameters.AddWithValue("@Total ", invD.Item_Net_Total_Price);
                opp.cmd.Parameters.AddWithValue("@Item_Net_Total_Price ", invD.Item_Net_Total_Price);


                opp.cmd.Parameters.AddWithValue("@Item_ATS05 ", invD.Item_ATS05);
                opp.cmd.Parameters.AddWithValue("@Item_ATS05_Value ", invD.Item_ATS05_Value);
                opp.cmd.Parameters.AddWithValue("@Item_TaxSales10 ", invD.Item_TaxSales10);
                opp.cmd.Parameters.AddWithValue("@Item_TaxSales10_Value ", invD.Item_TaxSales10_Value);


                opp.cmd.Parameters.AddWithValue("@Item_Notes ", invD.Item_Notes);

                //,Item_Total,Item_Notes
                //row["Item_ATS05"] = vs.Item_ATS05;
                //row["Item_ATS05_Value"] = vs.Item_ATS05_Value;
                //row["Item_TaxSales10"] = vs.Item_TaxSales10;
                //row["Item_TaxSales10_Value"] = vs.Item_TaxSales10_Value;


                opp.cmd.Parameters.AddWithValue("@Unit_Used_for_Price ", invD.Unit_Used_for_Price);

                opp.cmd.Parameters.AddWithValue("@OperationNo ", invD.OperationNo);
                opp.cmd.Parameters.AddWithValue("@StatementNo ", invD.StatementNo);
                opp.cmd.Parameters.AddWithValue("@Merchant_Name ", invD.Merchant_Name);
                opp.cmd.Parameters.AddWithValue("@QntyWidth ", invD.QntyWidth);
                opp.cmd.Parameters.AddWithValue("@QntyLenght ", invD.QntyLenght);
                opp.cmd.Parameters.AddWithValue("@QntyItemNo ", invD.QntyItemNo);
                opp.cmd.Parameters.AddWithValue("@Specification ", invD.Specification);
                opp.cmd.Parameters.AddWithValue("@NameSpecification ", invD.NameSpecification);


                opp.cmd.Parameters.AddWithValue("@code ", invD.code);
                opp.cmd.Parameters.AddWithValue("@Notes ", invD.Item_Notes);
                opp.cmd.Parameters.AddWithValue("@Balance_Checked ", invD.Balance_Checked);

                opp.cmd.Parameters.AddWithValue("@Make_Order ", invD.Make_Order);

                opp.cmd.Parameters.AddWithValue("@Is_InvoiceItem_Kta3y ", invD.Is_InvoiceItem_Kta3y);

                opp.cmd.Parameters.AddWithValue("@Audience_price ", invD.Audience_price);
                opp.cmd.Parameters.AddWithValue("@Tash3ela ", invD.Tash3ela);

                opp.cmd.Parameters.AddWithValue("@Item_Expire_Date ", invD.Item_Expire_Date);
                opp.cmd.Parameters.AddWithValue("@Is_Item_CommissionValue ", invD.Is_Item_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Item_CommissionValue ", invD.Item_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Unit_CommissionValue ", invD.Unit_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Item_CommissionRate ", invD.Item_CommissionRate);

                opp.cmd.Parameters.AddWithValue("@Is_Alternate_Item_CommissionValue ", invD.Is_Alternate_Item_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Alternate_Item_CommissionValue ", invD.Alternate_Item_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Alternate_Unit_CommissionValue ", invD.Alternate_Unit_CommissionValue);
                opp.cmd.Parameters.AddWithValue("@Alternate_Item_CommissionRate ", invD.Alternate_Item_CommissionRate);
                opp.cmd.Parameters.AddWithValue("@Alternate_ItemCommissionTotal ", invD.Alternate_ItemCommissionTotal);
                opp.cmd.Parameters.AddWithValue("@ItemCommissionTotal ", invD.ItemCommissionTotal);
                opp.cmd.Parameters.AddWithValue("@Net_Total_Item_Commission ", invD.Net_Total_Item_Commission);

                opp.cmd.Parameters.AddWithValue("@QntyStr ", invD.QntyStr);
                opp.cmd.Parameters.AddWithValue("@QntyStrColumn ", invD.QntyStr);
                opp.cmd.Parameters.AddWithValue("@TotalSmall_Qnty ", invD.TotalSmall_Qnty);
                opp.cmd.Parameters.AddWithValue("@TotalSmall_QntyStr ", invD.TotalSmall_QntyStr);
                opp.cmd.Parameters.AddWithValue("@TotalSmall_QntyStrColumn ", invD.TotalSmall_QntyStrColumn);

                Invoices_Details_Id = opp.Insert_me_Return_Last_Identify(mysql);
                Excp = 0;
                //
            }
            catch (Exception ee)
            {

                Excp = 1;
            }
            */
            return Invoices_Details_Id;
        }
        public long Insert_Invoices_Details_Reverse(ref operations opp, ref int Excp, Invoices_Details_Reverse invd_R)
        {
            opp.db.Invoices_Details_Reverse.Add(invd_R);
            opp.db.SaveChanges();

            return invd_R.Id;
        }
        public void Update_Invoices_Details_Reverse(ref operations opp, ref int Excp)
        {
            Excp = 0;
            /*
            try
            {
                string mysql = " Update Invoices_Details_Reverse Set  OperationNo='" + invD.OperationNo + "',StatementNo='" + invD.StatementNo + "',Merchant_Name='" + invD.Merchant_Name + "', Larg_Qnty =" + invD.Larg_Qnty + ", Small_Qnty =" + invD.Small_Qnty + ",Qnty_Another='" + invD.Qnty_Another + "', Larg_Unit_Price =" + invD.Larg_Unit_Price + ",   Qnty_value =" + invD.Qnty_value + ",Total =" + invD.Item_Net_Total_Price + ",Item_Net_Total_Price =" + invD.Item_Net_Total_Price + ", ATS05 =" + invD.Item_ATS05 + " ";
                mysql = mysql + " , TaxSales10 =" + invD.Item_TaxSales10 + ",TaxSales10_Value =" + invD.Item_TaxSales10_Value + ",ATS05_Value =" + invD.Item_ATS05_Value + ",Unit_Used_for_Price =" + invD.Unit_Used_for_Price + " , Specification='" + invD.Specification + "' ,Invoice_Item_Name=N'" + invD.Invoice_Item_Name + "',NameSpecification=N'" + invD.NameSpecification + "'";

                mysql = mysql + " , Item_ATS05 =" + invD.Item_ATS05 + ",Item_ATS05_Value =" + invD.Item_ATS05_Value + ",Item_TaxSales10 =" + invD.Item_TaxSales10 + ",Item_TaxSales10_Value  =" + invD.Item_TaxSales10_Value + " ";


                mysql = mysql + " , QntyWidth = " + invD.QntyWidth + ",QntyLenght = " + invD.QntyLenght + ",QntyItemNo = " + invD.QntyItemNo + " ";
                mysql = mysql + " , Audience_price=  " + invD.Audience_price + ",Tash3ela='" + invD.Tash3ela + "' ,Item_No_Free= " + invD.Item_No_Free + ",LargItemsQnty_Free =" + invD.LargItemsQnty_Free + ", SmallItemsQnty_Free =" + invD.SmallItemsQnty_Free + " ,Item_Expire_Date='" + operations.dateFormat(invD.Item_Expire_Date) + "'";

                mysql = mysql + " ,Is_Item_CommissionValue= " + invD.Is_Item_CommissionValue + ",Item_CommissionValue = " + invD.Item_CommissionValue + "," + "Unit_CommissionValue = " + invD.Unit_CommissionValue + ",Item_CommissionRate = " + invD.Item_CommissionRate + "," + "Net_Total_Item_Commission = " + invD.Net_Total_Item_Commission + " , ItemCommissionTotal= " + invD.ItemCommissionTotal + " ";
                mysql = mysql + " ,Is_Alternate_Item_CommissionValue = " + invD.Is_Alternate_Item_CommissionValue + ", Alternate_Item_CommissionValue = " + invD.Alternate_Item_CommissionValue + ", Alternate_Unit_CommissionValue = " + invD.Alternate_Unit_CommissionValue + ", Alternate_Item_CommissionRate = " + invD.Alternate_Item_CommissionRate + ",Alternate_ItemCommissionTotal = " + invD.Alternate_ItemCommissionTotal + "";
                mysql = mysql + " , QntyStr =N'" + invD.QntyStr + "', QntyStrColumn=N'" + invD.QntyStr + "' ";
                mysql = mysql + " , Item_Total_WithoutTax  =" + invD.Item_Total_WithoutTax + "";
                mysql = mysql + " , TotalSmall_Qnty  =" + invD.TotalSmall_Qnty + "";
                mysql = mysql + " , TotalSmall_QntyStr ='" + invD.TotalSmall_QntyStr + "'";
                mysql = mysql + " , TotalSmall_QntyStrColumn ='" + invD.TotalSmall_QntyStrColumn + "'";

                mysql = mysql + " Where Id =" + invD.Id;
                opp.insert_me(mysql);







            }
            catch (Exception ee)
            {

                Excp = 1;
            }

            */
        }
        public void Update_Invoices_Details_Reverse(ref operations opp, ref int Excp, Invoices_Details_Reverse invd_R)
        {
            Excp = 0;

            try
            {
                Invoices_Details_Reverse invd_FF = opp.db.Invoices_Details_Reverse.Find(invd_R.Id);
                if (invd_FF != null)
                {
                    opp.db.Entry(invd_FF).CurrentValues.SetValues(invd_R);
                    opp.db.SaveChanges();
                }
                //opp.db.Entry(invd_R).State = EntityState.Modified;


            }
            catch (Exception ee)
            {

                Excp = 1;
            }

        }
        public List<IInvoices_Details> GetIInvoiceDetails(long Invoices_ListId)
        {
            operations opp = new operations();

           // List<Invoices_Details> invD = invCls.GetInvoices_Details(ref opp, Invoices_ListId);
            List<Invoices_Details> invD = opp.db.Invoices_Details.AsNoTracking().Where(i => i.Invoice_Id == Invoices_ListId).ToList();

            List<IInvoices_Details> IInvDlst = invD.Cast<IInvoices_Details>().ToList();
            return IInvDlst;
        }
        
        public void GetInvoiceDetails_Reverse(long invoice_List_Table_Id)
        {
            //operations opp = new operations();

            //List<Invoices_Details_Reverse> invDR = invCls.GetInvoices_Details_Reverse(ref opp, invoice_List_Table_Id);

            //invDlst = invDR.Cast<IInvoices_Details>().ToList();
        }


        public List<Invoices_Details> GetInvoices_DetailsToEdit(ref operations opp, long Invoices_ListId)
        {
            List<Invoices_Details> invD = new List<Invoices_Details>();
            try
            {
                if (opp == null)
                {
                    opp = new operations();
                }
                //context.Configuration.ProxyCreationEnabled = false;
                opp.db.ChangeTracker.LazyLoadingEnabled = false;
                invD = opp.db.Invoices_Details.AsNoTracking().Where(i => i.Invoice_Id == Invoices_ListId).ToList();
            }
            catch (Exception ee)
            {

            }
            return invD;

        }
    }



    
}
