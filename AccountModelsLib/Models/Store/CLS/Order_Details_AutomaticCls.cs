using System;
 
using System.Collections.Generic;
using System.Text;

using System.Data;
using System.Collections;
using System.Linq;

namespace AccountModelsLib.Models
{
    public class Order_Details_AutomaticCls
    {
        public Order_Details_AutomaticCls()
        {

        }



        public void Make_Order_From_Invoive_Details(ref int Excp, ref operations opp, ref ArrayList arr_Order_Purshing_Id_Automatic, ref bool MakeOrderOk, DataTable dt_Invoice, long Invoice_Id, long Customer_Id, DateTime Invoice_Date, string Notes)
        {
            OrderChoiceCLS oc = new OrderChoiceCLS();
            arr_Order_Purshing_Id_Automatic = new ArrayList();
            // Stores.OrdersNew.Order_DetailsCls or = new Stores.OrdersNew.Order_DetailsCls();
            Order_DetailsCls or = new Order_DetailsCls();
            long OrderTable_id = 0;

            or.Order_Date = Invoice_Date;
            or.Order_Type = 2;

            or.Type_From = 4;
            or.Id_From = 0;

            or.Type_To = 2;
            or.Id_To = Customer_Id;
            or.InvoiceTable_ID = Invoice_Id;
            string Order_Estlam_Sarf_Flage = "2";// 1 mean estlam  ---2 mean sarf

            //////////////////////////////////////////////////////////////////Get Store_Id
            ArrayList arr_Store_Id = new ArrayList();
            //arr_Store_Id.Add(0);
            DataTable dt_Store_Id = new DataTable();
            for (int x = 0; x < dt_Invoice.Rows.Count; x++)
            {
                if (dt_Invoice.Rows[x]["Make_Order"].ToString() == "0")   //// الصنف المراد صرفة من المخزن باذن اوتوماتيك
                {
                    arr_Store_Id.Add(dt_Invoice.Rows[x]["Store_Id"].ToString());
                }
            }
            string mysql = " SELECT id, Store_Name FROM Store WHERE (id IN ( ";
            foreach (string id in arr_Store_Id)
            {
                mysql = mysql + id + ",";
            }
            mysql = mysql + " 0)) ";
            dt_Store_Id = opp.return_DataTable(mysql);
            ////////////////////////////////////////////////////////////////////

            for (int y = 0; y < dt_Store_Id.Rows.Count; y++)
            {
                long Store_Id = long.Parse(dt_Store_Id.Rows[y]["Id"].ToString());
                or.Id_From = Store_Id;

                //string Expression = "Store_Id=" + Store_Id.ToString();
                string Expression = "Store_Id=" + Store_Id.ToString() + " AND Make_Order=0";
                DataRow[] dr_ = dt_Invoice.Select(Expression);

                or.Order_Purshing_No = oc.GetNewOrderNo(or.Order_Type);
                or.Make_New_Order_FromInvoice(ref Excp, ref opp, ref OrderTable_id);
                arr_Order_Purshing_Id_Automatic.Add(OrderTable_id);
                foreach (DataRow dr in dr_)
                {

                    long Unit_Item_Table_Id = long.Parse(dr["Unit_Item_Table_Id"].ToString());
                    long Item_id = long.Parse(dr["Item_id"].ToString());

                    //Larg_Qnty = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Larg_Qnty"].ToString());
                    //Small_Qnty = decimal.Parse(Invoice_ds.Tables[0].Rows[x]["Small_Qnty"].ToString());
                    //Qnty_Another = Invoice_ds.Tables[0].Rows[x]["Qnty_Another"].ToString();
                    //decimal Larg_Qnty = decimal.Parse(dr["Will_Export_Qnty"].ToString());
                    //decimal Small_Qnty = decimal.Parse(dr["Will_Export_QntySmall"].ToString());
                    //decimal ItemNO = 0;


                    decimal Larg_Qnty = decimal.Parse(dr["Larg_Qnty"].ToString());
                    decimal Small_Qnty = decimal.Parse(dr["Small_Qnty"].ToString());
                    decimal ItemNO = 0;
                    long FromStoreId = Common.GetColumnValueLong(dr, "FromStoreId");
                    long ToStoreId = Common.GetColumnValueLong(dr, "ToStoreId");


                    ////////////////////////Get_Balance
                    mysql = "Select * from Unit_item where Id=" + Unit_Item_Table_Id;
                    DataTable dt = opp.return_DataTable(mysql);

                    decimal Balance_Now_Larg_Qnty = decimal.Parse(dt.Rows[0]["Full_qua"].ToString());
                    decimal Balance_Now_Small_Qnty = decimal.Parse(dt.Rows[0]["D_Qua"].ToString());
                    decimal Balance_Now_Item_No = 0;
                    decimal S_unit_Value = decimal.Parse(dt.Rows[0]["Small_unit"].ToString());

                    string LabelFrom = "";
                    string Label_FromTo = "";
                    //////From
                    or.Get_OrderTypes_Label_FromTo(ref Excp, "From", or.Type_From, or.Id_From, ref LabelFrom, ref Label_FromTo);
                    string Notes_From = LabelFrom + " " + Label_FromTo;


                    ///////////To
                    or.Get_OrderTypes_Label_FromTo(ref Excp, "To", or.Type_To, or.Id_To, ref LabelFrom, ref Label_FromTo);
                    string Notes_To = LabelFrom + " " + Label_FromTo;
                    long Related_Detailes_Purshing_Id = 0;



                    or.Make_Order_Details_Automatic_Invoice(ref Excp, ref opp, ref Related_Detailes_Purshing_Id, Order_Estlam_Sarf_Flage, OrderTable_id, Unit_Item_Table_Id, Item_id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, Balance_Now_Item_No, S_unit_Value, Notes_From, Notes_To, Notes, FromStoreId, ToStoreId);
                    MakeOrderOk = true;
                    mysql = " Update Invoices_Details Set Make_Order=1 ,Order_Purshing_Id_Automatic = " + OrderTable_id + " Where Invoice_Id=" + Invoice_Id + " And Unit_Item_Table_Id=" + Unit_Item_Table_Id + "";
                    try
                    {
                        opp.insert_me(mysql);
                    }
                    catch (Exception ee)
                    {

                    }
                }
            }




        }
        public void Make_Order_From_EditInvoive_Details(ref int Excp, ref ArrayList mysql_Arr, ref operations opp, ref ArrayList arr_Order_Purshing_Id_Automatic, ref bool MakeOrderOk, DataTable dt_Invoice, long Invoice_Id, long Customer_Id, DateTime Invoice_Date, string Notes)
        {
            OrderChoiceCLS oc = new OrderChoiceCLS();
            arr_Order_Purshing_Id_Automatic = new ArrayList();
            // Stores.OrdersNew.Order_DetailsCls or = new Stores.OrdersNew.Order_DetailsCls();
            Order_DetailsCls or = new Order_DetailsCls();
            long OrderTable_id = 0;

            or.Order_Date = Invoice_Date;
            or.Order_Type = 2;

            or.Type_From = 4;
            or.Id_From = 0;

            or.Type_To = 2;
            or.Id_To = Customer_Id;
            or.InvoiceTable_ID = Invoice_Id;
            string Order_Estlam_Sarf_Flage = "2";// 1 mean estlam  ---2 mean sarf

            //////////////////////////////////////////////////////////////////Get Store_Id
            ArrayList arr_Store_Id = new ArrayList();
            //arr_Store_Id.Add(0);
            DataTable dt_Store_Id = new DataTable();
            for (int x = 0; x < dt_Invoice.Rows.Count; x++)
            {
                if (dt_Invoice.Rows[x]["Make_Order"].ToString() == "0")   //// الصنف المراد صرفة من المخزن باذن اوتوماتيك
                {
                    arr_Store_Id.Add(dt_Invoice.Rows[x]["Store_Id"].ToString());
                }
            }
            string mysql = " SELECT id, Store_Name FROM Store WHERE (id IN ( ";
            foreach (string id in arr_Store_Id)
            {
                mysql = mysql + id + ",";
            }
            mysql = mysql + " 0)) ";
            dt_Store_Id = opp.return_DataTable(mysql);
            ////////////////////////////////////////////////////////////////////

            for (int y = 0; y < dt_Store_Id.Rows.Count; y++)
            {
                long Store_Id = long.Parse(dt_Store_Id.Rows[y]["Id"].ToString());
                or.Id_From = Store_Id;

                //string Expression = "Store_Id=" + Store_Id.ToString();
                string Expression = "Store_Id=" + Store_Id.ToString() + " AND Make_Order=0";
                DataRow[] dr_ = dt_Invoice.Select(Expression);

                or.Order_Purshing_No = oc.GetNewOrderNo(or.Order_Type);
                or.Make_New_Order_FromInvoice(ref Excp, ref opp, ref OrderTable_id);
                arr_Order_Purshing_Id_Automatic.Add(OrderTable_id);
                foreach (DataRow dr in dr_)
                {

                    long Unit_Item_Table_Id = long.Parse(dr["Unit_Item_Table_Id"].ToString());
                    long Item_id = long.Parse(dr["Item_id"].ToString());
                    decimal Larg_Qnty = decimal.Parse(dr["Will_Export_Qnty"].ToString());
                    decimal Small_Qnty = decimal.Parse(dr["Will_Export_QntySmall"].ToString());
                    decimal ItemNO = 0;

                    long FromStoreId = Common.GetColumnValueLong(dr, "FromStoreId");
                    long ToStoreId = Common.GetColumnValueLong(dr, "ToStoreId");


                    ////////////////////////Get_Balance
                    mysql = "Select * from Unit_item where Id=" + Unit_Item_Table_Id;
                    DataTable dt = opp.return_DataTable(mysql);

                    decimal Balance_Now_Larg_Qnty = decimal.Parse(dt.Rows[0]["Full_qua"].ToString());
                    decimal Balance_Now_Small_Qnty = decimal.Parse(dt.Rows[0]["D_Qua"].ToString());
                    decimal Balance_Now_Item_No = 0;
                    decimal S_unit_Value = decimal.Parse(dt.Rows[0]["Small_unit"].ToString());

                    string LabelFrom = "";
                    string Label_FromTo = "";
                    //////From
                    or.Get_OrderTypes_Label_FromTo(ref Excp, "From", or.Type_From, or.Id_From, ref LabelFrom, ref Label_FromTo);
                    string Notes_From = LabelFrom + " " + Label_FromTo;


                    ///////////To
                    or.Get_OrderTypes_Label_FromTo(ref Excp, "To", or.Type_To, or.Id_To, ref LabelFrom, ref Label_FromTo);
                    string Notes_To = LabelFrom + " " + Label_FromTo;
                    long Related_Detailes_Purshing_Id = 0;

                    or.Make_Order_Details_Automatic_Invoice(ref Excp, ref opp, ref Related_Detailes_Purshing_Id, Order_Estlam_Sarf_Flage, OrderTable_id, Unit_Item_Table_Id, Item_id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, Balance_Now_Item_No, S_unit_Value, Notes_From, Notes_To, Notes, FromStoreId, ToStoreId);
                    MakeOrderOk = true;

                    //operations opp2 = new operations();
                    mysql = " Update Invoices_Details Set Make_Order=1 ,Order_Purshing_Id_Automatic = " + OrderTable_id + " Where Invoice_Id=" + Invoice_Id + " And Unit_Item_Table_Id=" + Unit_Item_Table_Id + "";
                    mysql_Arr.Add(mysql);
                    //opp2.insert_me(mysql);
                }
            }




        }

        public void Make_Order_From_Edit_Invoices_Details_Reverse(ref int Excp, ref operations opp, ref ArrayList mysql_Arr, DataTable dt_Invoice, long Invoice_Id, long Customer_Id, DateTime Invoice_Date, string Notes)
        {

            OrderChoiceCLS oc = new OrderChoiceCLS();

            // Stores.OrdersNew.Order_DetailsCls or = new Stores.OrdersNew.Order_DetailsCls();
            Order_DetailsCls or = new Order_DetailsCls();
            long OrderTable_id = 0;

            or.Order_Date = Invoice_Date;
            or.Order_Type = 3;

            or.Type_From = 2;
            or.Id_From = Customer_Id;
            or.Type_To = 4;
            or.Id_To = 0;
            or.InvoiceTable_ID = Invoice_Id;
            string Order_Estlam_Sarf_Flage = "1"; // 1 mean estlam  ---2 mean sarf
            or.Order_Purshing_No = oc.GetNewOrderNo(or.Order_Type);
            //////////////////////////////////////////////////////////////////Get Store_Id
            ArrayList arr_Store_Id = new ArrayList();
            DataTable dt_Store_Id = new DataTable();
            for (int x = 0; x < dt_Invoice.Rows.Count; x++)
            {
                arr_Store_Id.Add(dt_Invoice.Rows[x]["Store_Id"].ToString());
            }
            string mysql = " SELECT id, Store_Name FROM Store WHERE (id IN ( ";
            foreach (string id in arr_Store_Id)
            {
                mysql = mysql + id + ",";
            }
            mysql = mysql + " 0)) ";
            dt_Store_Id = opp.return_DataTable(mysql);
            ////////////////////////////////////////////////////////////////////

            for (int y = 0; y < dt_Store_Id.Rows.Count; y++)
            {
                long Store_Id = long.Parse(dt_Store_Id.Rows[y]["Id"].ToString());
                or.Id_To = Store_Id;

                string Expression = "Store_Id=" + Store_Id.ToString();
                DataRow[] dr_ = dt_Invoice.Select(Expression);


                foreach (DataRow dr in dr_)
                {
                    or.Make_New_Order_FromInvoice(ref Excp, ref opp, ref OrderTable_id);
                    long Unit_Item_Table_Id = long.Parse(dr["Unit_Item_Table_Id"].ToString());
                    long Item_id = long.Parse(dr["Item_id"].ToString());
                    decimal Larg_Qnty = decimal.Parse(dr["Larg_Qnty"].ToString());
                    decimal Small_Qnty = decimal.Parse(dr["Small_Qnty"].ToString());
                    decimal ItemNO = 0;

                    long FromStoreId = Common.GetColumnValueLong(dr, "FromStoreId");
                    long ToStoreId = Common.GetColumnValueLong(dr, "ToStoreId");



                    //Larg_Qnty = decimal.Parse(dss.Tables[0].Rows[x]["Larg_Qnty"].ToString());
                    //Small_Qnty = decimal.Parse(dss.Tables[0].Rows[x]["Small_Qnty"].ToString());
                    //Qnty_Another = decimal.Parse(dss.Tables[0].Rows[x]["Qnty_Another"].ToString());
                    ////////////////////////Get_Balance
                    mysql = "Select * from Unit_item where Id=" + Unit_Item_Table_Id;
                    DataTable dt = opp.return_DataTable(mysql);

                    decimal Balance_Now_Larg_Qnty = decimal.Parse(dt.Rows[0]["Full_qua"].ToString());
                    decimal Balance_Now_Small_Qnty = decimal.Parse(dt.Rows[0]["D_Qua"].ToString());
                    decimal Balance_Now_Item_No = 0;
                    decimal S_unit_Value = decimal.Parse(dt.Rows[0]["Small_unit"].ToString());

                    string LabelFrom = "";
                    string Label_FromTo = "";
                    //////From
                    or.Get_OrderTypes_Label_FromTo(ref Excp, "From", or.Type_From, or.Id_From, ref LabelFrom, ref Label_FromTo);
                    string Notes_From = LabelFrom + " " + Label_FromTo;


                    ///////////To
                    or.Get_OrderTypes_Label_FromTo(ref Excp, "To", or.Type_To, or.Id_To, ref LabelFrom, ref Label_FromTo);
                    string Notes_To = LabelFrom + " " + Label_FromTo;
                    long Related_Detailes_Purshing_Id = 0;
                    or.Make_Order_Details_Automatic_Invoice(ref Excp, ref opp, ref Related_Detailes_Purshing_Id, Order_Estlam_Sarf_Flage, OrderTable_id, Unit_Item_Table_Id, Item_id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, Balance_Now_Item_No, S_unit_Value, Notes_From, Notes_To, Notes, FromStoreId, ToStoreId);

                    //operations opp2 = new operations();
                    mysql = "Update Invoices_Details  Set Make_Order=0 , Order_Purshing_Id_Automatic=" + OrderTable_id + " Where Invoice_Id=" + Invoice_Id + " And Unit_Item_Table_Id=" + Unit_Item_Table_Id + "";
                    mysql_Arr.Add(mysql);
                    //opp2.insert_me(mysql);
                }
            }




        }
        public void Make_Order_From_Edit_Bill_Details(ref int Excp, ref ArrayList mysql_Arr, ref operations opp, ref ArrayList arr_Order_Purshing_Id_Automatic, ref bool MakeOrderOk, DataTable dt_Invoice, long Invoice_Id, long Supplier_Id, DateTime Invoice_Date, string Notes)
        {
            OrderChoiceCLS oc = new OrderChoiceCLS();
            arr_Order_Purshing_Id_Automatic = new ArrayList();
            // Stores.OrdersNew.Order_DetailsCls or = new Stores.OrdersNew.Order_DetailsCls();
            Order_DetailsCls or = new Order_DetailsCls();
            long OrderTable_id = 0;

            or.Order_Date = Invoice_Date;
            or.Order_Type = 4;

            or.Type_From = 1;
            or.Id_From = (int)Supplier_Id;

            or.Type_To = 4;
            or.Id_To = 0;
            or.InvoiceTable_ID = Invoice_Id;
            string Order_Estlam_Sarf_Flage = "1";// 1 mean estlam  ---2 mean sarf

            //////////////////////////////////////////////////////////////////Get Store_Id
            ArrayList arr_Store_Id = new ArrayList();
            //arr_Store_Id.Add(0);
            DataTable dt_Store_Id = new DataTable();
            for (int x = 0; x < dt_Invoice.Rows.Count; x++)
            {
                if (dt_Invoice.Rows[x]["Make_Order"].ToString() == "0")   //// الصنف المراد صرفة من المخزن باذن اوتوماتيك
                {
                    arr_Store_Id.Add(dt_Invoice.Rows[x]["Store_Id"].ToString());
                }
            }
            string mysql = " SELECT id, Store_Name FROM Store WHERE (id IN ( ";
            foreach (string id in arr_Store_Id)
            {
                mysql = mysql + id + ",";
            }
            mysql = mysql + " 0)) ";
            dt_Store_Id = opp.return_DataTable(mysql);
            ////////////////////////////////////////////////////////////////////

            for (int y = 0; y < dt_Store_Id.Rows.Count; y++)
            {
                long Store_Id = long.Parse(dt_Store_Id.Rows[y]["Id"].ToString());
                or.Id_From = Store_Id;

                //string Expression = "Store_Id=" + Store_Id.ToString();
                string Expression = "Store_Id=" + Store_Id.ToString() + " AND Make_Order=0";
                DataRow[] dr_ = dt_Invoice.Select(Expression);

                or.Order_Purshing_No = oc.GetNewOrderNo(or.Order_Type);
                or.Make_New_Order_FromInvoice(ref Excp, ref opp, ref OrderTable_id);
                arr_Order_Purshing_Id_Automatic.Add(OrderTable_id);
                foreach (DataRow dr in dr_)
                {

                    long Unit_Item_Table_Id = long.Parse(dr["Unit_Item_Table_Id"].ToString());
                    long Item_id = long.Parse(dr["Item_id"].ToString());
                    decimal Larg_Qnty = decimal.Parse(dr["Will_Export_Qnty"].ToString());
                    decimal Small_Qnty = decimal.Parse(dr["Will_Export_QntySmall"].ToString());
                    decimal ItemNO = 0;
                    long FromStoreId = Common.GetColumnValueLong(dr, "FromStoreId");
                    long ToStoreId = Common.GetColumnValueLong(dr, "ToStoreId");


                    ////////////////////////Get_Balance
                    mysql = "Select * from Unit_item where Id=" + Unit_Item_Table_Id;
                    DataTable dt = opp.return_DataTable(mysql);

                    decimal Balance_Now_Larg_Qnty = decimal.Parse(dt.Rows[0]["Full_qua"].ToString());
                    decimal Balance_Now_Small_Qnty = decimal.Parse(dt.Rows[0]["D_Qua"].ToString());
                    decimal Balance_Now_Item_No = 0;
                    decimal S_unit_Value = decimal.Parse(dt.Rows[0]["Small_unit"].ToString());

                    string LabelFrom = "";
                    string Label_FromTo = "";
                    //////From
                    or.Get_OrderTypes_Label_FromTo(ref Excp, "From", or.Type_From, or.Id_From, ref LabelFrom, ref Label_FromTo);
                    string Notes_From = LabelFrom + " " + Label_FromTo;


                    ///////////To
                    or.Get_OrderTypes_Label_FromTo(ref Excp, "To", or.Type_To, or.Id_To, ref LabelFrom, ref Label_FromTo);
                    string Notes_To = LabelFrom + " " + Label_FromTo;
                    long Related_Detailes_Purshing_Id = 0;
                    or.Make_Order_Details_Automatic_Invoice(ref Excp, ref opp, ref Related_Detailes_Purshing_Id, Order_Estlam_Sarf_Flage, OrderTable_id, Unit_Item_Table_Id, Item_id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, Balance_Now_Item_No, S_unit_Value, Notes_From, Notes_To, Notes, FromStoreId, ToStoreId);
                    MakeOrderOk = true;

                    //operations opp2 = new operations();
                    mysql = " Update Bill_Details Set Make_Order=1 ,Order_Purshing_Id_Automatic = " + OrderTable_id + " Where Invoice_Id=" + Invoice_Id + " And Unit_Item_Table_Id=" + Unit_Item_Table_Id + "";
                    mysql_Arr.Add(mysql);
                    //opp2.insert_me(mysql);
                }
            }




        }

        public void Make_Order_From_Edit_Bill_Details_Reverse(ref int Excp, ref operations opp, ref ArrayList mysql_Arr, DataTable dt_Invoice, long Invoice_Id, long Customer_Id, DateTime Invoice_Date, string Notes)
        {

            OrderChoiceCLS oc = new OrderChoiceCLS();

            // Stores.OrdersNew.Order_DetailsCls or = new Stores.OrdersNew.Order_DetailsCls();
            Order_DetailsCls or = new Order_DetailsCls();
            long OrderTable_id = 0;

            or.Order_Date = Invoice_Date;
            or.Order_Type = 5;

            or.Type_From = 4;
            or.Id_From = 0;
            or.Type_To = 1;
            or.Id_To = (long)Customer_Id;
            or.InvoiceTable_ID = Invoice_Id;
            string Order_Estlam_Sarf_Flage = "2"; // 1 mean estlam  ---2 mean sarf
            or.Order_Purshing_No = oc.GetNewOrderNo(or.Order_Type);
            //////////////////////////////////////////////////////////////////Get Store_Id
            ArrayList arr_Store_Id = new ArrayList();
            DataTable dt_Store_Id = new DataTable();
            for (int x = 0; x < dt_Invoice.Rows.Count; x++)
            {
                arr_Store_Id.Add(dt_Invoice.Rows[x]["Store_Id"].ToString());
            }
            string mysql = " SELECT id, Store_Name FROM Store WHERE (id IN ( ";
            foreach (string id in arr_Store_Id)
            {
                mysql = mysql + id + ",";
            }
            mysql = mysql + " 0)) ";
            dt_Store_Id = opp.return_DataTable(mysql);
            ////////////////////////////////////////////////////////////////////

            for (int y = 0; y < dt_Store_Id.Rows.Count; y++)
            {
                long Store_Id = long.Parse(dt_Store_Id.Rows[y]["Id"].ToString());
                or.Id_To = Store_Id;

                string Expression = "Store_Id=" + Store_Id.ToString();
                DataRow[] dr_ = dt_Invoice.Select(Expression);


                foreach (DataRow dr in dr_)
                {
                    or.Make_New_Order_FromInvoice(ref Excp, ref opp, ref OrderTable_id);
                    long Unit_Item_Table_Id = long.Parse(dr["Unit_Item_Table_Id"].ToString());
                    long Item_id = long.Parse(dr["Item_id"].ToString());
                    decimal Larg_Qnty = decimal.Parse(dr["Will_Export_Qnty"].ToString());
                    decimal Small_Qnty = decimal.Parse(dr["Will_Export_QntySmall"].ToString());
                    decimal ItemNO = 0;
                    long FromStoreId = Common.GetColumnValueLong(dr, "FromStoreId");
                    long ToStoreId = Common.GetColumnValueLong(dr, "ToStoreId");


                    ////////////////////////Get_Balance
                    mysql = "Select * from Unit_item where Id=" + Unit_Item_Table_Id;
                    DataTable dt = opp.return_DataTable(mysql);

                    decimal Balance_Now_Larg_Qnty = decimal.Parse(dt.Rows[0]["Full_qua"].ToString());
                    decimal Balance_Now_Small_Qnty = decimal.Parse(dt.Rows[0]["D_Qua"].ToString());
                    decimal Balance_Now_Item_No = 0;
                    decimal S_unit_Value = decimal.Parse(dt.Rows[0]["Small_unit"].ToString());

                    string LabelFrom = "";
                    string Label_FromTo = "";
                    //////From
                    or.Get_OrderTypes_Label_FromTo(ref Excp, "From", or.Type_From, or.Id_From, ref LabelFrom, ref Label_FromTo);
                    string Notes_From = LabelFrom + " " + Label_FromTo;


                    ///////////To
                    or.Get_OrderTypes_Label_FromTo(ref Excp, "To", or.Type_To, or.Id_To, ref LabelFrom, ref Label_FromTo);
                    string Notes_To = LabelFrom + " " + Label_FromTo;
                    long Related_Detailes_Purshing_Id = 0;
                    or.Make_Order_Details_Automatic_Invoice(ref Excp, ref opp, ref Related_Detailes_Purshing_Id, Order_Estlam_Sarf_Flage, OrderTable_id, Unit_Item_Table_Id, Item_id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, Balance_Now_Item_No, S_unit_Value, Notes_From, Notes_To, Notes, FromStoreId, ToStoreId);

                    //operations opp2 = new operations();
                    mysql = "Update Bills_Details  Set Make_Order=0 , Order_Purshing_Id_Automatic=" + OrderTable_id + " Where Invoice_Id=" + Invoice_Id + " And Unit_Item_Table_Id=" + Unit_Item_Table_Id + "";
                    mysql_Arr.Add(mysql);
                    //opp2.insert_me(mysql);
                }
            }




        }
        public void Make_Order_From_Invoices_Details_Reverse(ref int Excp, ref operations opp, DataTable dt_Invoice, long Invoice_Id, long Customer_Id, DateTime Invoice_Date, string Notes)
        {
            OrderChoiceCLS oc = new OrderChoiceCLS();

            // Stores.OrdersNew.Order_DetailsCls or = new Stores.OrdersNew.Order_DetailsCls();
            Order_DetailsCls or = new Order_DetailsCls();
            long OrderTable_id = 0;

            or.Order_Date = Invoice_Date;
            or.Order_Type = 3;

            or.Type_From = 2;
            or.Id_From = Customer_Id;

            or.Type_To = 4;
            or.Id_To = 0;
            or.InvoiceTable_ID = Invoice_Id;
            string Order_Estlam_Sarf_Flage = "1"; // 1 mean estlam  ---2 mean sarf
            or.Order_Purshing_No = oc.GetNewOrderNo(or.Order_Type);
            //////////////////////////////////////////////////////////////////Get Store_Id
            ArrayList arr_Store_Id = new ArrayList();
            DataTable dt_Store_Id = new DataTable();
            for (int x = 0; x < dt_Invoice.Rows.Count; x++)
            {
                arr_Store_Id.Add(dt_Invoice.Rows[x]["Store_Id"].ToString());
            }
            string mysql = " SELECT id, Store_Name FROM Store WHERE (id IN ( ";
            foreach (string id in arr_Store_Id)
            {
                mysql = mysql + id + ",";
            }
            mysql = mysql + " 0)) ";
            dt_Store_Id = opp.return_DataTable(mysql);
            ////////////////////////////////////////////////////////////////////

            for (int y = 0; y < dt_Store_Id.Rows.Count; y++)
            {
                long Store_Id = long.Parse(dt_Store_Id.Rows[y]["Id"].ToString());
                or.Id_To = Store_Id;

                string Expression = "Store_Id=" + Store_Id.ToString();
                DataRow[] dr_ = dt_Invoice.Select(Expression);


                foreach (DataRow dr in dr_)
                {
                    or.Make_New_Order_FromInvoice(ref Excp, ref opp, ref OrderTable_id);
                    long Unit_Item_Table_Id = long.Parse(dr["Unit_Item_Table_Id"].ToString());
                    long Item_id = long.Parse(dr["Item_id"].ToString());
                    decimal Larg_Qnty = decimal.Parse(dr["Will_Export_Qnty"].ToString());
                    decimal Small_Qnty = decimal.Parse(dr["Will_Export_QntySmall"].ToString());
                    decimal ItemNO = 0;
                    long FromStoreId = Common.GetColumnValueLong(dr, "FromStoreId");
                    long ToStoreId = Common.GetColumnValueLong(dr, "ToStoreId");


                    ////////////////////////Get_Balance
                    mysql = "Select * from Unit_item where Id=" + Unit_Item_Table_Id;
                    DataTable dt = opp.return_DataTable(mysql);

                    decimal Balance_Now_Larg_Qnty = decimal.Parse(dt.Rows[0]["Full_qua"].ToString());
                    decimal Balance_Now_Small_Qnty = decimal.Parse(dt.Rows[0]["D_Qua"].ToString());
                    decimal Balance_Now_Item_No = 0;
                    decimal S_unit_Value = decimal.Parse(dt.Rows[0]["Small_unit"].ToString());

                    string LabelFrom = "";
                    string Label_FromTo = "";
                    //////From
                    or.Get_OrderTypes_Label_FromTo(ref Excp, "From", or.Type_From, or.Id_From, ref LabelFrom, ref Label_FromTo);
                    string Notes_From = LabelFrom + " " + Label_FromTo;


                    ///////////To
                    or.Get_OrderTypes_Label_FromTo(ref Excp, "To", or.Type_To, or.Id_To, ref LabelFrom, ref Label_FromTo);
                    string Notes_To = LabelFrom + " " + Label_FromTo;
                    long Related_Detailes_Purshing_Id = 0;
                    or.Make_Order_Details_Automatic_Invoice(ref Excp, ref opp, ref Related_Detailes_Purshing_Id, Order_Estlam_Sarf_Flage, OrderTable_id, Unit_Item_Table_Id, Item_id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, Balance_Now_Item_No, S_unit_Value, Notes_From, Notes_To, Notes, FromStoreId, ToStoreId);

                    mysql = "Update Invoices_Details_Reverse Set Make_Order=0 , Order_Purshing_Id_Automatic=" + OrderTable_id + " Where Invoice_Id=" + Invoice_Id + " And Unit_Item_Table_Id=" + Unit_Item_Table_Id + "";
                    opp.insert_me(mysql);
                }
            }




        }
        public void Make_Order_From_Bill_Details(ref int Excp, ref operations opp, DataTable dt_Invoice, long Invoice_Id, long Customer_Id, DateTime Invoice_Date, string Notes = "")
        {
            OrderChoiceCLS oc = new OrderChoiceCLS();

            // Stores.OrdersNew.Order_DetailsCls or = new Stores.OrdersNew.Order_DetailsCls();
            Order_DetailsCls or = new Order_DetailsCls();
            long OrderTable_id = 0;

            or.Order_Date = Invoice_Date;
            or.Order_Type = 4;

            or.Type_From = 1;
            or.Id_From = (long)Customer_Id;

            or.Type_To = 4;
            or.Id_To = 0;
            or.InvoiceTable_ID = Invoice_Id;
            string Order_Estlam_Sarf_Flage = "1";
            or.Order_Purshing_No = oc.GetNewOrderNo(or.Order_Type);
            //////////////////////////////////////////////////////////////////Get Store_Id
            ArrayList arr_Store_Id = new ArrayList();
            DataTable dt_Store_Id = new DataTable();
            for (int x = 0; x < dt_Invoice.Rows.Count; x++)
            {
                arr_Store_Id.Add(dt_Invoice.Rows[x]["Store_Id"].ToString());
            }
            string mysql = " SELECT id, Store_Name FROM Store WHERE (id IN ( ";
            foreach (string id in arr_Store_Id)
            {
                mysql = mysql + id + ",";
            }
            mysql = mysql + " 0)) ";
            dt_Store_Id = opp.return_DataTable(mysql);
            ////////////////////////////////////////////////////////////////////

            for (int y = 0; y < dt_Store_Id.Rows.Count; y++)
            {
                long Store_Id = long.Parse(dt_Store_Id.Rows[y]["Id"].ToString());
                or.Id_To = Store_Id;

                string Expression = "Store_Id=" + Store_Id.ToString();
                DataRow[] dr_ = dt_Invoice.Select(Expression);


                foreach (DataRow dr in dr_)
                {
                    or.Make_New_Order_FromInvoice(ref Excp, ref opp, ref OrderTable_id);
                    long Unit_Item_Table_Id = long.Parse(dr["Unit_Item_Table_Id"].ToString());
                    long Item_id = long.Parse(dr["Item_id"].ToString());
                    decimal Larg_Qnty = decimal.Parse(dr["Will_Export_Qnty"].ToString());
                    decimal Small_Qnty = decimal.Parse(dr["Will_Export_QntySmall"].ToString());
                    decimal ItemNO = 0;

                    long FromStoreId = Common.GetColumnValueLong(dr, "FromStoreId");
                    long ToStoreId = Common.GetColumnValueLong(dr, "ToStoreId");


                    ////////////////////////Get_Balance
                    mysql = "Select * from Unit_item where Id=" + Unit_Item_Table_Id;
                    DataTable dt = opp.return_DataTable(mysql);

                    decimal Balance_Now_Larg_Qnty = decimal.Parse(dt.Rows[0]["Full_qua"].ToString());
                    decimal Balance_Now_Small_Qnty = decimal.Parse(dt.Rows[0]["D_Qua"].ToString());
                    decimal Balance_Now_Item_No = 0;
                    decimal S_unit_Value = decimal.Parse(dt.Rows[0]["Small_unit"].ToString());

                    string LabelFrom = "";
                    string Label_FromTo = "";
                    //////From
                    or.Get_OrderTypes_Label_FromTo(ref Excp, "From", or.Type_From, or.Id_From, ref LabelFrom, ref Label_FromTo);
                    string Notes_From = LabelFrom + " " + Label_FromTo;


                    ///////////To
                    or.Get_OrderTypes_Label_FromTo(ref Excp, "To", or.Type_To, or.Id_To, ref LabelFrom, ref Label_FromTo);
                    string Notes_To = LabelFrom + " " + Label_FromTo;
                    long Related_Detailes_Purshing_Id = 0;
                    or.Make_Order_Details_Automatic_Invoice(ref Excp, ref opp, ref Related_Detailes_Purshing_Id, Order_Estlam_Sarf_Flage, OrderTable_id, Unit_Item_Table_Id, Item_id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, Balance_Now_Item_No, S_unit_Value, Notes_From, Notes_To, Notes, FromStoreId, ToStoreId);

                    mysql = "Update Invoices_Details Set Make_Order=0 Where Invoice_Id=" + Invoice_Id + " And Unit_Item_Table_Id=" + Unit_Item_Table_Id + "";
                    opp.insert_me(mysql);
                }
            }
        }

        public void Make_Order_From_Bill_Details(ref int Excp, ref operations opp, List<IBills_Details> invDlst, long Invoice_Id, long Customer_Id, DateTime Invoice_Date, string Notes = "")
        {
            OrderChoiceCLS oc = new OrderChoiceCLS();

            // Stores.OrdersNew.Order_DetailsCls or = new Stores.OrdersNew.Order_DetailsCls();
            Order_DetailsCls or = new Order_DetailsCls();
            long OrderTable_id = 0;

            or.Order_Date = Invoice_Date;
            or.Order_Type = 4;

            or.Type_From = 1;
            or.Id_From = (long)Customer_Id;

            or.Type_To = 4;
            or.Id_To = 0;
            or.InvoiceTable_ID = Invoice_Id;
            string Order_Estlam_Sarf_Flage = "1";
            or.Order_Purshing_No = oc.GetNewOrderNo(or.Order_Type);
            //////////////////////////////////////////////////////////////////Get Store_Id
            ArrayList arr_Store_Id = new ArrayList();
            DataTable dt_Store_Id = new DataTable();
            for (int x = 0; x < invDlst.Count; x++)
            {
                arr_Store_Id.Add(invDlst[x].Store_Id.ToString());
            }
            string mysql = " SELECT id, Store_Name FROM Store WHERE (id IN ( ";
            foreach (string id in arr_Store_Id)
            {
                mysql = mysql + id + ",";
            }
            mysql = mysql + " 0)) ";
            dt_Store_Id = opp.return_DataTable(mysql);
            ////////////////////////////////////////////////////////////////////

            for (int y = 0; y < dt_Store_Id.Rows.Count; y++)
            {
                long Store_Id = long.Parse(dt_Store_Id.Rows[y]["Id"].ToString());
                or.Id_To = Store_Id;

                string Expression = "Store_Id=" + Store_Id.ToString();
                //DataRow[] dr_ = dt_Invoice.Select(Expression);
                List<IBills_Details> _invDlst = invDlst.Where(s => s.Store_Id == Store_Id).ToList();

                foreach (IBills_Details dr in _invDlst)
                {
                    or.Make_New_Order_FromInvoice(ref Excp, ref opp, ref OrderTable_id);
                    long Unit_Item_Table_Id = long.Parse(dr.Unit_Item_Table_Id.ToString());
                    int Item_id = int.Parse(dr.Item_Id.ToString());
                    decimal Larg_Qnty = decimal.Parse(dr.Larg_Qnty.ToString());
                    decimal Small_Qnty = decimal.Parse(dr.Small_Qnty.ToString());
                    decimal ItemNO = 0;

                    //long FromStoreId =  dr.FromStoreId;
                    //long ToStoreId =  dr.ToStoreId;

                    long FromStoreId = 0;
                    long ToStoreId = 0;
                    ////////////////////////Get_Balance
                    mysql = "Select * from Unit_item where Id=" + Unit_Item_Table_Id;
                    DataTable dt = opp.return_DataTable(mysql);

                    decimal Balance_Now_Larg_Qnty = decimal.Parse(dt.Rows[0]["Full_qua"].ToString());
                    decimal Balance_Now_Small_Qnty = decimal.Parse(dt.Rows[0]["D_Qua"].ToString());
                    decimal Balance_Now_Item_No = 0;
                    decimal S_unit_Value = decimal.Parse(dt.Rows[0]["Small_unit"].ToString());

                    string LabelFrom = "";
                    string Label_FromTo = "";
                    //////From
                    or.Get_OrderTypes_Label_FromTo(ref Excp, "From", or.Type_From, or.Id_From, ref LabelFrom, ref Label_FromTo);
                    string Notes_From = LabelFrom + " " + Label_FromTo;


                    ///////////To
                    or.Get_OrderTypes_Label_FromTo(ref Excp, "To", or.Type_To, or.Id_To, ref LabelFrom, ref Label_FromTo);
                    string Notes_To = LabelFrom + " " + Label_FromTo;
                    long Related_Detailes_Purshing_Id = 0;
                    or.Make_Order_Details_Automatic_Invoice(ref Excp, ref opp, ref Related_Detailes_Purshing_Id, Order_Estlam_Sarf_Flage, OrderTable_id, Unit_Item_Table_Id, Item_id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, Balance_Now_Item_No, S_unit_Value, Notes_From, Notes_To, Notes, FromStoreId, ToStoreId);

                    mysql = "Update Invoices_Details Set Make_Order=0 Where Invoice_Id=" + Invoice_Id + " And Unit_Item_Table_Id=" + Unit_Item_Table_Id + "";
                    opp.insert_me(mysql);
                }
            }
        }
        public void Make_Order_From_Bills_Details_Reverse(ref int Excp, ref operations opp, DataTable dt_Invoice, long Invoice_Id, long Customer_Id, DateTime Invoice_Date, string Notes = "")
        {
            OrderChoiceCLS oc = new OrderChoiceCLS();

            // Stores.OrdersNew.Order_DetailsCls or = new Stores.OrdersNew.Order_DetailsCls();
            Order_DetailsCls or = new Order_DetailsCls();
            long OrderTable_id = 0;

            or.Order_Date = Invoice_Date;
            or.Order_Type = 5;

            or.Type_From = 4;
            or.Id_From = 0;

            or.Type_To = 1;
            or.Id_To = (long)Customer_Id;

            long FromStoreId = or.Id_From;
            long ToStoreId = or.Id_To;


            or.InvoiceTable_ID = Invoice_Id;
            string Order_Estlam_Sarf_Flage = "2";// 1 mean estlam  ---2 mean sarf
            or.Order_Purshing_No = oc.GetNewOrderNo(or.Order_Type);
            //////////////////////////////////////////////////////////////////Get Store_Id
            ArrayList arr_Store_Id = new ArrayList();
            DataTable dt_Store_Id = new DataTable();
            for (int x = 0; x < dt_Invoice.Rows.Count; x++)
            {
                arr_Store_Id.Add(dt_Invoice.Rows[x]["Store_Id"].ToString());
            }
            string mysql = " SELECT id, Store_Name FROM Store WHERE (id IN ( ";
            foreach (string id in arr_Store_Id)
            {
                mysql = mysql + id + ",";
            }
            mysql = mysql + " 0)) ";
            dt_Store_Id = opp.return_DataTable(mysql);
            ////////////////////////////////////////////////////////////////////

            for (int y = 0; y < dt_Store_Id.Rows.Count; y++)
            {
                long Store_Id = long.Parse(dt_Store_Id.Rows[y]["Id"].ToString());
                or.Id_From = Store_Id;

                string Expression = "Store_Id=" + Store_Id.ToString();
                DataRow[] dr_ = dt_Invoice.Select(Expression);


                foreach (DataRow dr in dr_)
                {
                    or.Make_New_Order_FromInvoice(ref Excp, ref opp, ref OrderTable_id);
                    long Unit_Item_Table_Id = long.Parse(dr["Unit_Item_Table_Id"].ToString());
                    long Item_id = long.Parse(dr["Item_id"].ToString());
                    decimal Larg_Qnty = decimal.Parse(dr["Will_Export_Qnty"].ToString());
                    decimal Small_Qnty = decimal.Parse(dr["Will_Export_QntySmall"].ToString());
                    decimal ItemNO = 0;

                    ////////////////////////Get_Balance
                    mysql = "Select * from Unit_item where Id=" + Unit_Item_Table_Id;
                    DataTable dt = opp.return_DataTable(mysql);

                    decimal Balance_Now_Larg_Qnty = decimal.Parse(dt.Rows[0]["Full_qua"].ToString());
                    decimal Balance_Now_Small_Qnty = decimal.Parse(dt.Rows[0]["D_Qua"].ToString());
                    decimal Balance_Now_Item_No = 0;
                    decimal S_unit_Value = decimal.Parse(dt.Rows[0]["Small_unit"].ToString());

                    string LabelFrom = "";
                    string Label_FromTo = "";
                    //////From
                    or.Get_OrderTypes_Label_FromTo(ref Excp, "From", or.Type_From, or.Id_From, ref LabelFrom, ref Label_FromTo);
                    string Notes_From = LabelFrom + " " + Label_FromTo;


                    ///////////To
                    or.Get_OrderTypes_Label_FromTo(ref Excp, "To", or.Type_To, or.Id_To, ref LabelFrom, ref Label_FromTo);
                    string Notes_To = LabelFrom + " " + Label_FromTo;
                    long Related_Detailes_Purshing_Id = 0;
                    or.Make_Order_Details_Automatic_Invoice(ref Excp, ref opp, ref Related_Detailes_Purshing_Id, Order_Estlam_Sarf_Flage, OrderTable_id, Unit_Item_Table_Id, Item_id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, Balance_Now_Item_No, S_unit_Value, Notes_From, Notes_To, Notes, FromStoreId, ToStoreId);

                    mysql = "Update Invoices_Details Set Make_Order=0 Where Invoice_Id=" + Invoice_Id + " And Unit_Item_Table_Id=" + Unit_Item_Table_Id + "";
                    opp.insert_me(mysql);
                }
            }




        }


        public void Make_Order_From_Bills_Details_Reverse(ref int Excp, ref operations opp, List<IBills_Details> indLst, long Invoice_Id, long Customer_Id, DateTime Invoice_Date, string Notes = "")
        {
            OrderChoiceCLS oc = new OrderChoiceCLS();

            // Stores.OrdersNew.Order_DetailsCls or = new Stores.OrdersNew.Order_DetailsCls();
            Order_DetailsCls or = new Order_DetailsCls();
            long OrderTable_id = 0;

            or.Order_Date = Invoice_Date;
            or.Order_Type = 5;

            or.Type_From = 4;
            or.Id_From = 0;

            or.Type_To = 1;
            or.Id_To = (long)Customer_Id;

            long FromStoreId = or.Id_From;
            long ToStoreId = or.Id_To;


            or.InvoiceTable_ID = Invoice_Id;
            string Order_Estlam_Sarf_Flage = "2";// 1 mean estlam  ---2 mean sarf
            or.Order_Purshing_No = oc.GetNewOrderNo(or.Order_Type);
            //////////////////////////////////////////////////////////////////Get Store_Id
            ArrayList arr_Store_Id = new ArrayList();
            DataTable dt_Store_Id = new DataTable();
            //for (int x = 0; x < dt_Invoice.Rows.Count; x++)
            //{
            //    arr_Store_Id.Add(dt_Invoice.Rows[x]["Store_Id"].ToString());
            //}
            foreach (var item in indLst)
            {
                arr_Store_Id.Add(item.Store_Id ?? 0);
            }
            string mysql = " SELECT id, Store_Name FROM Store WHERE (id IN ( ";
            foreach (string id in arr_Store_Id)
            {
                mysql = mysql + id + ",";
            }
            mysql = mysql + " 0)) ";
            dt_Store_Id = opp.return_DataTable(mysql);
            ////////////////////////////////////////////////////////////////////

            for (int y = 0; y < dt_Store_Id.Rows.Count; y++)
            {
                long Store_Id = long.Parse(dt_Store_Id.Rows[y]["Id"].ToString());
                or.Id_From = Store_Id;

                //string Expression = "Store_Id=" + Store_Id.ToString();
                //DataRow[] dr_ = dt_Invoice.Select(Expression);

                var indLst_ = indLst.Where(s => s.Store_Id == Store_Id).ToList();

                foreach (var item in indLst_)
                {
                    or.Make_New_Order_FromInvoice(ref Excp, ref opp, ref OrderTable_id);
                    long Unit_Item_Table_Id = item.Unit_Item_Table_Id ?? 0;
                    long Item_id = item.Item_Id ?? 0;
                    decimal Larg_Qnty = item.Larg_Qnty ?? 0;
                    decimal Small_Qnty = item.Small_Qnty ?? 0;
                    decimal ItemNO = 0;

                    ////////////////////////Get_Balance
                    mysql = "Select * from Unit_item where Id=" + Unit_Item_Table_Id;
                    DataTable dt = opp.return_DataTable(mysql);

                    decimal Balance_Now_Larg_Qnty = decimal.Parse(dt.Rows[0]["Full_qua"].ToString());
                    decimal Balance_Now_Small_Qnty = decimal.Parse(dt.Rows[0]["D_Qua"].ToString());
                    decimal Balance_Now_Item_No = 0;
                    decimal S_unit_Value = decimal.Parse(dt.Rows[0]["Small_unit"].ToString());

                    string LabelFrom = "";
                    string Label_FromTo = "";
                    //////From
                    or.Get_OrderTypes_Label_FromTo(ref Excp, "From", or.Type_From, or.Id_From, ref LabelFrom, ref Label_FromTo);
                    string Notes_From = LabelFrom + " " + Label_FromTo;


                    ///////////To
                    or.Get_OrderTypes_Label_FromTo(ref Excp, "To", or.Type_To, or.Id_To, ref LabelFrom, ref Label_FromTo);
                    string Notes_To = LabelFrom + " " + Label_FromTo;
                    long Related_Detailes_Purshing_Id = 0;
                    or.Make_Order_Details_Automatic_Invoice(ref Excp, ref opp, ref Related_Detailes_Purshing_Id, Order_Estlam_Sarf_Flage, OrderTable_id, Unit_Item_Table_Id, Item_id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, Balance_Now_Item_No, S_unit_Value, Notes_From, Notes_To, Notes, FromStoreId, ToStoreId);

                    mysql = "Update Invoices_Details Set Make_Order=0 Where Invoice_Id=" + Invoice_Id + " And Unit_Item_Table_Id=" + Unit_Item_Table_Id + "";
                    opp.insert_me(mysql);
                }
            }




        }

        public void Make_Order_From_Estlam_Production(ref int Excp, ref operations opp, long Item_id, long Store_Id, DateTime Invoice_Date, decimal Larg_Qnty, decimal Small_Qnty, string Notes = "")
        {
            OrderChoiceCLS oc = new OrderChoiceCLS();

            // Stores.OrdersNew.Order_DetailsCls or = new Stores.OrdersNew.Order_DetailsCls();
            Order_DetailsCls or = new Order_DetailsCls();
            long OrderTable_id = 0;

            or.Order_Date = Invoice_Date;
            or.Order_Type = 9;

            or.Type_From = 9;
            or.Id_From = 9;

            or.Type_To = 4;
            or.Id_To = Store_Id;
            or.InvoiceTable_ID = 0;
            string Order_Estlam_Sarf_Flage = "1";
            or.Order_Purshing_No = oc.GetNewOrderNo(or.Order_Type);
            //////////////////////////////////////////////////////////////////Get Store_Id
            long FromStoreId = or.Id_From;
            long ToStoreId = or.Id_To;


            ////////////////////////////////////////////////////////////////////


            or.Make_New_Order_FromInvoice(ref Excp, ref opp, ref OrderTable_id);




            decimal ItemNO = 0;

            ////////////////////////Get_Balance
            string mysql = "Select * from Unit_item where Item_Id=" + Item_id + " And Store_Id=" + Store_Id;
            DataTable dt = opp.return_DataTable(mysql);
            long Unit_Item_Table_Id = long.Parse(dt.Rows[0]["Id"].ToString());
            decimal Balance_Now_Larg_Qnty = decimal.Parse(dt.Rows[0]["Full_qua"].ToString());
            decimal Balance_Now_Small_Qnty = decimal.Parse(dt.Rows[0]["D_Qua"].ToString());
            decimal Balance_Now_Item_No = 0;
            decimal S_unit_Value = decimal.Parse(dt.Rows[0]["Small_unit"].ToString());


            string LabelFrom = "";
            string Label_FromTo = "";

            //////From
            or.Get_OrderTypes_Label_FromTo(ref Excp, "From", or.Type_From, or.Id_From, ref LabelFrom, ref Label_FromTo);
            string Notes_From = LabelFrom + " " + Label_FromTo;


            ///////////To
            or.Get_OrderTypes_Label_FromTo(ref Excp, "To", or.Type_To, or.Id_To, ref LabelFrom, ref Label_FromTo);
            string Notes_To = LabelFrom + " " + Label_FromTo;
            long Related_Detailes_Purshing_Id = 0;
            or.Make_Order_Details_Automatic_Invoice(ref Excp, ref opp, ref Related_Detailes_Purshing_Id, Order_Estlam_Sarf_Flage, OrderTable_id, Unit_Item_Table_Id, Item_id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, Balance_Now_Item_No, S_unit_Value, Notes_From, Notes_To, Notes, FromStoreId, ToStoreId);








        }

    }
}
