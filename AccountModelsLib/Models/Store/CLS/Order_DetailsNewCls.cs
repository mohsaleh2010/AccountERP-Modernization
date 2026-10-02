

//using AccountModelsLib.DBModels;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore;
 
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    public class Order_DetailsNewCls
    {
        public operations opp;
        public CarsWorkOrdersViewModel CWOVM_S = new CarsWorkOrdersViewModel();

        public Order_Purshing order_Purshing = new Order_Purshing();
        public Detailes_Purshing Detailes_Purshing = new Detailes_Purshing();
        public List<Detailes_Purshing> Detailes_PurshingList = new List<Detailes_Purshing>();
        Order_Purshing od_C;
        Detailes_Purshing dp_C;

        public Order_DetailsNewCls()
        {
            opp = new operations();
        }
        public Order_DetailsNewCls(operations opp_)
        {
            opp = opp_;
        }

        public void GetOrder_PurshingData(ref int Excp, ref string Msg, long Order_PurshingId)
        {
            Order_PurshingCLS opCls = new Order_PurshingCLS();
            order_Purshing = new Order_Purshing();

            Detailes_PurshingCLS dpCLS = new Detailes_PurshingCLS();
            Detailes_PurshingList = new List<Detailes_Purshing>();


            try
            {
                order_Purshing = opCls.GetOrder_PurshingId(ref opp, Order_PurshingId);
                Detailes_PurshingList = dpCLS.GetDetailes_PurshingListByOrderId(ref opp, Order_PurshingId);
                Excp = 0;

                order_Purshing.CarsWorkOrdersId = order_Purshing.CarsWorkOrdersId ?? 0;
                if (order_Purshing.CarsWorkOrdersId > 0)
                {
                    CarsWorkOrdersCLS crw = new CarsWorkOrdersCLS();
                    List<CarsWorkOrdersViewModel> CWOVM_LSt = crw.getCarsWorkOrder(order_Purshing.CarsWorkOrdersId ?? 0);
                    if (CWOVM_LSt.Count > 0)
                    {
                        CWOVM_S = CWOVM_LSt[0];
                    }

                }
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
            }
        }
        public void GetOrder_PurshingData__(ref int Excp, ref string Msg, long Order_ID)
        {

            //GetOrder_PurshingData_(ref Excp, ref Msg, Order_ID);

            //order_Purshing = new Order_Purshing();

            //Detailes_PurshingList = new List<Detailes_Purshing>();
            //try
            //{

            //    order_Purshing = opp.db.Order_Purshings.Where(o => o.Id == Order_ID).FirstOrDefault();

            //    Detailes_PurshingList = opp.db.Detailes_Purshings.Where(o => o.OrderTable_id == Order_ID).Include(i => i.Items).Include(i => i.Unit_item).ToList();
            //    Excp = 0;
            //}
            //catch (Exception ee)
            //{
            //    Excp = 1;
            //}
            //string mysql = " SELECT * FROM Order_Purshing ";
            //mysql = mysql + "  WHERE (Id = " + Order_ID + ")";
            //return Get_Items_Data(ref Excp, mysql);

        }
        public void Get_OrderTypes_Label_FromTo(ref int Excp, string FLAG, int Type_ID, long ID_From_To, ref string LabelFromTo, ref string Label_FromTo)
        {
            try
            {
                LabelFromTo = "";
                Label_FromTo = "";
                operations opp = new operations();
                string mysql = "Select * from OrderTypes_Label Where TypesID=" + Type_ID;

                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count == 0)
                {
                    LabelFromTo = "Error";
                    return;
                }
                if (FLAG == "From")
                {
                    LabelFromTo = dt.Rows[0]["LabelFrom"].ToString();

                }
                else
                {
                    LabelFromTo = dt.Rows[0]["LabelTo"].ToString();
                }
                Label_FromTo = dt.Rows[0]["Label_Name"].ToString();

                /////////////Get  txt Label_Name  if dept,store,customer,supplier
                if (Type_ID == 0)
                {
                    mysql = "Select * from Department Where ID=" + ID_From_To;
                    dt.Clear();
                    dt = opp.return_DataTable(mysql);
                    if (dt.Rows.Count > 0)
                    {
                        Label_FromTo = dt.Rows[0]["dname"].ToString();
                    }

                }
                if (Type_ID == 1)
                {
                    mysql = "Select * from Supplier Where ID=" + ID_From_To;
                    dt.Clear();
                    dt = opp.return_DataTable(mysql);
                    if (dt.Rows.Count > 0)
                    {
                        Label_FromTo = dt.Rows[0]["Name"].ToString();
                    }

                }
                if (Type_ID == 2)
                {
                    mysql = "Select * from Customer Where ID=" + ID_From_To;
                    dt.Clear();
                    dt = opp.return_DataTable(mysql);
                    if (dt.Rows.Count > 0)
                    {
                        Label_FromTo = dt.Rows[0]["Name"].ToString();
                    }

                }
                if (Type_ID == 4)
                {
                    mysql = "Select * from Store Where ID=" + ID_From_To;
                    dt.Clear();
                    dt = opp.return_DataTable(mysql);
                    if (dt.Rows.Count > 0)
                    {
                        Label_FromTo = dt.Rows[0]["Store_Name"].ToString();
                    }

                }





                opp = null;
                Excp = 0;

            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }

        }
        public void Get_Order_Label(ref int Excp, int OrderTypes, ref string Order_Name, ref string BtnRecive_Text, ref int Order_Estlam_Sarf_Flage)
        {
            try
            {


                operations opp = new operations();
                //string mysql = "Select * from OrderTypes Where OrderTypes_Value=" + OrderTypes;
                string mysql = "Select * from DataDictionaryProcessesLists Where Id=" + OrderTypes;

                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {

                    //Order_Name = dt.Rows[0]["Order_Name"].ToString();
                    Order_Name = dt.Rows[0]["ProcessName"].ToString();
                    BtnRecive_Text = dt.Rows[0]["BtnRecive_Text"].ToString();

                }
                else
                {
                    Order_Name = "Error";
                    BtnRecive_Text = "Error";
                }
                if (dt.Rows.Count > 0)
                {

                    Order_Estlam_Sarf_Flage = int.Parse(dt.Rows[0]["Order_Estlam_Sarf"].ToString());
                }
                else
                {
                    Order_Estlam_Sarf_Flage = 1;

                }



                opp = null;
                Excp = 0;

            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }

        }
        public DataSet Get_Item_List(ref int Excp, int Store_Type, long Store_id)
        {
            string mysql;
            mysql = @"SELECT  unit.u_name, unit.B_Unit, unit.S_unit, unit.B_Unit_Value, unit.S_unit_Value, Unit_item.id, Unit_item.Item_id, Unit_item.Store_id, Unit_item.Store_Type, 
                        Item.Code,  Unit_item.Code as Code1, Unit_item.unit_id, Unit_item.Full_qua, Unit_item.D_Qua, Unit_item.Item_No, Unit_item.Reserved_Balance_Larg_Qnty, 
                         Unit_item.Reserved_Balance_Small_Qnty, Unit_item.Big_unit, Unit_item.Small_unit, Unit_item.P_price, Unit_item.S_price, Unit_item.Specification, 
                         Unit_item.Upon_Request, Unit_item.Currency, Unit_item.active_flage, Unit_item.TaxSales10_Value, Item.Item_Name, Unit_item.id AS Unit_itemTable_id, Item.No3eboa, 
                         Item.Active_flage AS Expr1,CAST(Unit_item.Full_qua AS nvarchar) + N' : ' + N'' + unit.B_Unit AS Full_qua_B, CAST(Unit_item.D_Qua AS nvarchar) + N' : ' + N'' + unit.S_unit AS D_Qua_S
FROM            Item INNER JOIN
                         Unit_item ON Item.id = Unit_item.Item_id INNER JOIN
                         unit ON Unit_item.unit_id = unit.id";
            mysql = mysql + " WHERE (Unit_item.Store_Type = " + Store_Type + ") AND (Unit_item.Store_id = " + Store_id + ") AND (Item.Active_flage=1) AND (Unit_item.Active_flage=1) order by Item.id";
            DataSet ds = Get_Items_Data(ref Excp, mysql);

            //for(int x=0;x<ds.Tables[0].Rows.Count;x++)
            //{

            //    if(ds.Tables[0].Rows[x]["S_unit_Value"].ToString()== ds.Tables[0].Rows[x]["B_Unit_Value"].ToString())
            //    {
            //        ds.Tables[0].Rows[x]["D_Qua_S"]  = "";
            //        }
            //}
            return ds;

        }
        
        private DataSet Get_Items_Data(ref int Excp, string mysql)
        {
            DataSet G_DS = new DataSet();
            try
            {
                operations opp = new operations();
                G_DS = opp.return_dataset(mysql);
                Excp = 1;
                opp = null;
                return G_DS;
            }
            catch (Exception ee)
            {

                Excp = -1;
                //MessageBox.Show(ee.Message);
                return G_DS;
            }
        }
        public void Get_BalanceAfter_Befor(ref int Excp, ref Detailes_Purshing dp, int Order_Estlam_Sarf_Flage, long Unit_Item_Table_Id, long Item_id, decimal Larg_Qnty, decimal Small_Qnty, decimal ItemNO, decimal Balance_Now_Larg_Qnty, decimal Balance_Now_Small_Qnty, decimal Balance_Now_Item_No, decimal S_unit_Value)
        {
            //

            string mysql = "";

            decimal Qua_Befor = 0;
            decimal Part_qua_Befor = 0;
            decimal Item_No_Befor = 0;
            decimal Qua_After = 0;
            decimal Part_qua_After = 0;
            decimal Item_No_After = 0;



            DataSet dss;
            Cls_Conversion_Unit clc = new Cls_Conversion_Unit();
            ////فى حالة زيادة الرصيد

            decimal Balance_Id_From_Larg_Qnty = 0;
            decimal Balance_Id_From_Small_Qnty = 0;
            decimal Balance_Id_From_Item_No = Balance_Now_Item_No - ItemNO;

            ////فى حالة نقص الرصيد
            decimal Balance_Id_To_Larg_Qnty = 0;
            decimal Balance_Id_To_Small_Qnty = 0;
            decimal Balance_Id_To_Item_No = Balance_Now_Item_No + ItemNO;


            decimal Unit_Item_Table_Larg_Qnty = 0;
            decimal Unit_Item_Table__Small_Qnty = 0;
            decimal Unit_Item_Table_Item_No = 0;
            long To_Store_Unit_Item_Table_Id = 0;
            //decimal Larg_Qnty, decimal Small_Qnty, decimal 
            if (ItemNO == 0)
            {
                if (Common.Compute_3eboaFrom_EznSarf_Estlam == 1)
                {
                    ItemNO = Common.Get3ebowa(Larg_Qnty, Item_id);
                }
            }
            //الغاء الكسور من الكميات  
            Cls_Conversion_Unit.ConVert_By_S_UnitValue(ref Larg_Qnty, ref Small_Qnty, S_unit_Value);
            #region Transform          تحويل
            if (order_Purshing.Order_Type == (int)StoreOrderType.SarfTahweel_1 | order_Purshing.Order_Type == (int)StoreOrderType.MortagaSarfTahweel_6)             //التحويل
            {

                decimal To_Store_LargBalance = 0;
                decimal To_Store_Small_Balance = 0;
                decimal To_Store_Item_No = 0;

                if (!Check_ItemFoundOnStore(Item_id, ref dp, ref To_Store_Unit_Item_Table_Id, ref To_Store_LargBalance, ref To_Store_Small_Balance, ref To_Store_Item_No, Unit_Item_Table_Id))
                {
                    Excp = 1;
                    return;
                }

                ////تعديل بيانات المخزن المنصرف منة بنقص الرصيد

                clc.ConVersionQuntity_Subtract_ToItem(false, Larg_Qnty, Small_Qnty, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, ref Balance_Id_From_Larg_Qnty, ref Balance_Id_From_Small_Qnty, S_unit_Value);

                Unit_Item_Table_Larg_Qnty = Balance_Id_From_Larg_Qnty;
                Unit_Item_Table__Small_Qnty = Balance_Id_From_Small_Qnty;
                Unit_Item_Table_Item_No = Balance_Id_From_Item_No;
                ////mysql = " UPDATE Unit_item  SET Full_qua =" + Unit_Item_Table_Larg_Qnty + ", D_Qua =" + Unit_Item_Table__Small_Qnty + ", Item_No =" + Unit_Item_Table_Item_No + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                ////opp.insert_me(mysql);


                ////////////////////////////////////////////////////////////
                Qua_Befor = Balance_Now_Larg_Qnty;
                Part_qua_Befor = Balance_Now_Small_Qnty;
                Item_No_Befor = Balance_Now_Item_No;

                Qua_After = Unit_Item_Table_Larg_Qnty;
                Part_qua_After = Unit_Item_Table__Small_Qnty;
                Item_No_After = Balance_Id_To_Item_No;

                //////////////////////////////////////////////////////////

                ////تعديل بيانات المخزن المنصرف إلية بزيادة الرصيد
                clc.ConVersionQuntity_ADD_ToItem(Larg_Qnty, Small_Qnty, To_Store_LargBalance, To_Store_Small_Balance, ref Balance_Id_To_Larg_Qnty, ref Balance_Id_To_Small_Qnty, S_unit_Value);

                Unit_Item_Table_Larg_Qnty = Balance_Id_To_Larg_Qnty;
                Unit_Item_Table__Small_Qnty = Balance_Id_To_Small_Qnty;
                Unit_Item_Table_Item_No = To_Store_Item_No + ItemNO;
                Balance_Id_To_Item_No = Unit_Item_Table_Item_No;





                ////زيادة الرصيد وتفعيل الاذن اوتوماتيك اى انشاء الاذن كأنة اذن صرف من واذن استلام الى
                if (Common.Make_Store_Order_Sarf_Estlam_Automatic == 1)/////
                {
                    ////mysql = " UPDATE Unit_item  SET Full_qua =" + Unit_Item_Table_Larg_Qnty + ", D_Qua =" + Unit_Item_Table__Small_Qnty + ", Item_No =" + Unit_Item_Table_Item_No + "  WHERE (id = " + To_Store_Unit_Item_Table_Id + ") ";
                    ////opp.insert_me(mysql);
                }
                else
                {
                    To_Store_Unit_Item_Table_Id = 0;
                    Balance_Id_To_Larg_Qnty = 0;
                    Balance_Id_To_Small_Qnty = 0;
                    Balance_Id_To_Item_No = 0;

                }




            }
            #endregion

            #region Transform          - استلام من الانتاج تسوية بالزيادة

            #endregion

            #region    mortaga3 sarf or  estlam
            //else if (Order_Type == 3 | Order_Type == 4)
            else if (Order_Estlam_Sarf_Flage == 1)
            {
                ////تعديل بيانات المخزن المنصرف إلية بزيادة الرصيد
                clc.ConVersionQuntity_ADD_ToItem(Larg_Qnty, Small_Qnty, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, ref Balance_Id_To_Larg_Qnty, ref Balance_Id_To_Small_Qnty, S_unit_Value);

                Unit_Item_Table_Larg_Qnty = Balance_Id_To_Larg_Qnty;
                Unit_Item_Table__Small_Qnty = Balance_Id_To_Small_Qnty;
                Unit_Item_Table_Item_No = Balance_Id_To_Item_No;


                ////mysql = " UPDATE Unit_item  SET Full_qua =" + Unit_Item_Table_Larg_Qnty + ", D_Qua =" + Unit_Item_Table__Small_Qnty + ", Item_No =" + Unit_Item_Table_Item_No + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                ////opp.insert_me(mysql);

                To_Store_Unit_Item_Table_Id = -1;
                Balance_Id_From_Larg_Qnty = 0;
                Balance_Id_From_Small_Qnty = 0;
                Balance_Id_From_Item_No = 0;

                ////////////////////////////////////////////////////////////
                Qua_Befor = Balance_Now_Larg_Qnty;
                Part_qua_Befor = Balance_Now_Small_Qnty;
                Item_No_Befor = Balance_Now_Item_No;

                Qua_After = Unit_Item_Table_Larg_Qnty;
                Part_qua_After = Unit_Item_Table__Small_Qnty;
                Item_No_After = Balance_Id_To_Item_No;

                //////////////////////////////////////////////////////////



            }
            #endregion

            #region     sarf or mortaga3 estlam
            //else if (Order_Type == 2 | Order_Type == 5 | Order_Type == 8 | Order_Type == 10)
            else if (Order_Estlam_Sarf_Flage == 2)
            {

                ////تعديل بيانات المخزن المنصرف منة بنقص الرصيد
                clc.ConVersionQuntity_Subtract_ToItem(false, Larg_Qnty, Small_Qnty, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, ref Balance_Id_From_Larg_Qnty, ref Balance_Id_From_Small_Qnty, S_unit_Value);

                Unit_Item_Table_Larg_Qnty = Balance_Id_From_Larg_Qnty;
                Unit_Item_Table__Small_Qnty = Balance_Id_From_Small_Qnty;
                Unit_Item_Table_Item_No = Balance_Id_From_Item_No;

                ////mysql = " UPDATE Unit_item  SET Full_qua =" + Unit_Item_Table_Larg_Qnty + ", D_Qua =" + Unit_Item_Table__Small_Qnty + ", Item_No =" + Unit_Item_Table_Item_No + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                ////opp.insert_me(mysql);

                To_Store_Unit_Item_Table_Id = -1;
                Balance_Id_To_Larg_Qnty = 0;
                Balance_Id_To_Small_Qnty = 0;
                Balance_Id_To_Item_No = 0;


                ////////////////////////////////////////////////////////////
                Qua_Befor = Balance_Now_Larg_Qnty;
                Part_qua_Befor = Balance_Now_Small_Qnty;
                Item_No_Befor = Balance_Now_Item_No;

                Qua_After = Unit_Item_Table_Larg_Qnty;
                Part_qua_After = Unit_Item_Table__Small_Qnty;
                Item_No_After = Balance_Id_To_Item_No;

                //////////////////////////////////////////////////////////




            }
            #endregion


            dp.Qua_Befor = Qua_Befor;
            dp.Part_qua_Befor = Part_qua_Befor;
            dp.Item_No_Befor = Item_No_Befor;
            dp.Qua_After = Qua_After;
            dp.Part_qua_After = Part_qua_After;
            dp.Item_No_After = Item_No_After;



        }

        public bool Check_ItemFoundOnStore(long Item_id, ref Detailes_Purshing dp, ref long To_Store_Unit_Item_Table_Id, ref decimal To_Store_LargBalance, ref decimal To_Store_Small_Balance, ref decimal To_Store_Item_No, long Unit_Item_Table_Id)
        {

            try
            {
                if (dp.Type_To == 3 | dp.Type_To == 10 | dp.Type_To == 8)  //  هالك - تشغيل - تسوية بالنقص
                {
                    To_Store_Unit_Item_Table_Id = 0;
                    return true;
                }
                operations opp = new operations();
                string mysql = "";
                DataSet ds = new DataSet();
                ds = opp.return_dataset("select * from Unit_item WHERE (Item_id = " + Item_id + ") AND  (Store_id = " + dp.Id_To + ") AND (Store_Type = " + dp.Type_To + ")  ");

                if (ds.Tables[0].Rows.Count == 0)
                {
                    ds.Clear();
                    ds = opp.return_dataset("select * from Unit_item WHERE ( id = " + Unit_Item_Table_Id + ")");
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        mysql = " INSERT INTO Unit_item (Item_id, Code, unit_id, Full_qua, D_Qua, Item_No, Reserved_Balance_Larg_Qnty, Reserved_Balance_Small_Qnty, Big_unit, Small_unit, P_price, S_price, Specification, Upon_Request , Store_id, Store_Type, Currency) ";
                        mysql = mysql + " VALUES (" + ds.Tables[0].Rows[0]["Item_id"].ToString() + ",'" + ds.Tables[0].Rows[0]["Code"].ToString() + "'," + ds.Tables[0].Rows[0]["unit_id"].ToString() + ", 0, 0,0, 0, 0," + ds.Tables[0].Rows[0]["Big_unit"].ToString() + "," + ds.Tables[0].Rows[0]["Small_unit"].ToString() + "," + ds.Tables[0].Rows[0]["P_price"].ToString() + "," + ds.Tables[0].Rows[0]["S_price"].ToString() + " ,'" + ds.Tables[0].Rows[0]["Specification"].ToString() + "'," + ds.Tables[0].Rows[0]["Upon_Request"].ToString() + " ," + dp.Id_To + "," + dp.Type_To + ",'" + ds.Tables[0].Rows[0]["Currency"].ToString() + "') ";
                        int Excp = 0;
                        To_Store_Unit_Item_Table_Id = opp.Insert_me_Return_Last_Identify(mysql, ref Excp);
                        To_Store_LargBalance = 0;
                        To_Store_Small_Balance = 0;
                        To_Store_Item_No = 0;
                    }
                    else
                    {
                        To_Store_Unit_Item_Table_Id = -1;
                    }

                }
                else
                {
                    To_Store_Unit_Item_Table_Id = long.Parse(ds.Tables[0].Rows[0]["id"].ToString());
                    To_Store_LargBalance = decimal.Parse(ds.Tables[0].Rows[0]["Full_qua"].ToString());
                    To_Store_Small_Balance = decimal.Parse(ds.Tables[0].Rows[0]["D_Qua"].ToString());
                    To_Store_Item_No = decimal.Parse(ds.Tables[0].Rows[0]["Item_No"].ToString());
                }
                return true;
            }
            catch (Exception ee)
            {
                // MessageBox.Show(ee.Message);
                To_Store_Unit_Item_Table_Id = 0;
                return false;
            }


        }

        public void Edit_OrderFromOK(ref int Excp, ref string Msg, List<Detailes_Purshing> dp_NewList, List<Detailes_Purshing> dp_DeletingList, DataTable dt_RawMaterials, DataTable dt_SubItem, int Order_Estlam_Sarf_Flage, bool IS_EstlamRawMaterial)
        {

            //if (OrderType != 3 & OrderType != 4 & OrderType != 7)              /// ليست مرتجع صرف أو  استلام
            //{
            Excp = 0;
            string mysql = "";
            opp.BeginTransaction(2);
            //operations opp = new operations(2);
            Order_Purshing opOld = opp.db.Order_Purshings.AsNoTracking().Where(i => i.Id == order_Purshing.Id).FirstOrDefault();
            #region When Change the CarWorkOrder >> Change it in All details purchase

            if (opOld.CarsWorkOrdersId != order_Purshing.CarsWorkOrdersId)
            {
                foreach (Detailes_Purshing dp in Detailes_PurshingList)
                {
                    dp.CarNumber = order_Purshing.CarNumber;
                    dp.CarsId = order_Purshing.CarsId;
                    dp.CarsWorkOrdersId = order_Purshing.CarsWorkOrdersId;

                    opp.db.Entry(dp).State = EntityState.Modified;
                    opp.db.SaveChanges();

                }
            }
            #endregion

            //////حذف إذن الخامات المصروفة مع الانتاج
            ///
            Order_PurshingCLS op = new Order_PurshingCLS();
            List<Order_Purshing> opMainListId = op.GetByMainOrderTable_id(ref opp, order_Purshing.Id);
            foreach (var item in opMainListId)
            {
                long SubOrderTable_id = item.Id;// long.Parse(dttt.Rows[x]["Id"].ToString());
                int Order_Type = item.Order_Type.Value;// int.Parse(dttt.Rows[x]["Order_Type"].ToString());
                if (Order_Type == 10)
                {
                    DeleteOrder(ref Excp, ref Msg, ref opp, SubOrderTable_id);

                    mysql = "DELETE FROM Item_Raw_MaterialsFor_Entage WHERE(SubOrderTable_id =" + SubOrderTable_id + ")";
                    opp.insert_me(mysql);
                }
            }

            op.UpdateOrder_Purshing(ref Excp, ref opp, order_Purshing);
            //mysql = " Update Order_Purshing Set Order_date='" + operations.dateFormat(Order_Purshing.Order_date) + "',NotesAll=N'" + Order_Purshing.NotesAll + "' Where Id=" + Order_Purshing.Id;
            //opp.insert_me(mysql);
            try
            {
                ItemCLS itemCLS = new ItemCLS();

                /////////تعديل الاصناف  
                foreach (var dp_Edit in Detailes_PurshingList)
                {
                    if (dp_Edit.Id > 0)
                    {
                        if (dp_Edit.Item_ProcessedFlage == 2)
                        {

                            Edit_Order_DetailsFromEdit(ref Excp, ref opp, dp_Edit);
                            SetEditOrderForCarsWorkOrders(dp_Edit, order_Purshing);
                            if (order_Purshing.Order_Type == (int)StoreOrderType.EstlamFromEntag_9)
                            {
                                //Edit_Item_Raw_MaterialsFor_Entage_FromEdit(ref Excp, ref opp,x, dt_NewOrder,   dt_RawMaterials,   dt_SubItem);
                            }
                        }
                    }
                    else if (dp_Edit.Id == 0)
                    {
                        if (dp_NewList.Count == 0)
                        {
                            dp_NewList.Add(dp_Edit);

                            //DataDictionaryLogerProcessCLS ddlpCLS = new DataDictionaryLogerProcessCLS(ref opp);
                            //ddlpCLS.AddNew(order_Purshing.Order_Type.Value, 6, order_Purshing.Id, order_Purshing.Order_Purshing_No, dp_Edit.Item_Name);
                        }
                        foreach (var item in dp_NewList)
                        {
                            if (dp_Edit != item)
                            {
                                dp_NewList.Add(dp_Edit);


                                break;
                            }
                        }
                    }
                    decimal P_priceAvgCost = itemCLS.GetComputed_P_AvgCost(ref opp, dp_Edit.Item_Id);
                }
                /////////اضافة الاصناف الجديدة
                ///
                foreach (var dp_New in dp_NewList)
                {
                    //Add_NewItemIN_Order(ref Excp, ref opp, x, dt_NewOrder, OrderTable_id, Order_Estlam_Sarf_Flage);

                    Add_NewItemIN_Order(ref Excp, ref opp, order_Purshing, dp_New);
                    //SetEditOrderForCarsWorkOrders(dp_New);
                    DataDictionaryLogerProcessCLS ddlpCLS = new DataDictionaryLogerProcessCLS(ref opp);
                    //ddlpCLS.AddNew(order_Purshing.Order_Type.Value, 6, order_Purshing.Id, order_Purshing.Order_Purshing_No, dp_New.Id,dp_New.Item_Name);
                    ddlpCLS.AddNew(6, order_Purshing, dp_New);

                    decimal P_priceAvgCost = itemCLS.GetComputed_P_AvgCost(ref opp, dp_New.Item_Id);
                }
                #region Make Deleted Item 
                foreach (var dp_D in dp_DeletingList)
                {
                    if (dp_D.Item_DeletedFlage == 1)
                    {

                        long Detailes_Purshing_Id = dp_D.Id;// long.Parse(ArrDeleted_Id[x].ToString());
                        long Order_Purshing_Id = dp_D.OrderTable_id;
                        Detailes_PurshingCLS dpCLS = new Detailes_PurshingCLS();
                        Detailes_Purshing dp = dpCLS.GetDetailes_PurshingToDeleteByIdAsNoTracking(ref opp, Detailes_Purshing_Id);
                        ////Delete Main Item from Order
                        Delete_Order_DetailsFromEdit(ref Excp, ref opp, dp);

                        //DataDictionaryLogerProcessCLS ddlpCLS = new DataDictionaryLogerProcessCLS(ref opp);
                        //ddlpCLS.AddNew(order_Purshing.Order_Type.Value, 3, order_Purshing.Id, order_Purshing.Order_Purshing_No, dp.Item_Name);


                        DeleteOrderForCarsWorkOrders(dp);
                        if (order_Purshing.Order_Type == (int)StoreOrderType.EstlamFromEntag_9)
                        {

                            //Delete_Item_Raw_MaterialsFor_Entage_FromEdit(ref Excp, ref opp, Item_id, Order_Purshing_Id);
                        }

                    }
                    decimal P_priceAvgCost = itemCLS.GetComputed_P_AvgCost(ref opp, dp_D.Item_Id);
                }

                #endregion

                #region When Change the CarWorkOrder >> change it for all carworkorders tables
                if (opOld.CarsWorkOrdersId != order_Purshing.CarsWorkOrdersId)
                {
                    ChangeOrderForCarsWorkOrders(Detailes_PurshingList[0]);



                }
                #endregion
                //DeleteOrderFrom_Item_Raw_MaterialsFor_Entage(ref Excp, ref Msg, ref opp, OrderTable_id);


                if (Excp == 0)
                {
                    opp.Commit();
                    Msg = "تم تعديل الإذن بنجاح";
                    if (order_Purshing.Order_Type == (int)StoreOrderType.EstlamFromEntag_9)
                    {
                        operations opp2 = new operations();
                        mysql = "SELECT Item_Raw_MaterialsFor_Entage_List_Id FROM Item_Raw_MaterialsFor_Entage_Details WHERE(Order_Purshing_Id = " + order_Purshing.Id + ")";
                        DataTable dtt = opp2.return_DataTable(mysql);
                        long Item_Raw_MaterialsFor_Entage_List_Id = 0;
                        if (dtt.Rows.Count > 0)
                        {
                            Item_Raw_MaterialsFor_Entage_List_Id = long.Parse(dtt.Rows[0]["Item_Raw_MaterialsFor_Entage_List_Id"].ToString());
                        }

                        string Notes_List = "تم إصدار الإذن والإرتباط بشكل اوتوماتيكى من معادلة الانتاج";
                        Notes_List = Notes_List + " .. " + order_Purshing.NotesAll;
                        mysql = " Update Item_Raw_MaterialsFor_Entage_List  Set Process_Date='" + operations.dateFormat(order_Purshing.Order_date) + "',Notes_List=N'" + Notes_List + "' Where Id=" + Item_Raw_MaterialsFor_Entage_List_Id;
                        opp2.insert_me(mysql);

                        mysql = "DELETE FROM Item_Raw_MaterialsFor_Entage_Details WHERE(Order_Purshing_Id = " + order_Purshing.Id + ")";
                        opp2.insert_me(mysql);
                        /////////////////////////////////

                        //intsert Estlam from entag

                        foreach (var item in Detailes_PurshingList)
                        {
                            long Unit_Item_Table_Id = item.Unit_item_Table_Id;
                            long Item_id = item.Item_Id;
                            //decimal Larg_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Larg_Qnty"].ToString());
                            //decimal Small_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Small_Qnty"].ToString());
                            //decimal Item_NO = decimal.Parse(dt_NewOrder.Rows[index]["Item_NO"].ToString());

                            //decimal Balance_Now_Larg_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Balance_Now_Larg_Qnty"].ToString());
                            //decimal Balance_Now_Small_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Balance_Now_Small_Qnty"].ToString());
                            //decimal Balance_Now_Item_No = decimal.Parse(dt_NewOrder.Rows[index]["Balance_Now_Item_No"].ToString());
                            //decimal S_unit_Value = decimal.Parse(dt_NewOrder.Rows[index]["S_unit_Value"].ToString());

                            //string Item_Name = dt_NewOrder.Rows[index]["Item_Name"].ToString();
                            //string B_Unit = dt_NewOrder.Rows[index]["B_Unit"].ToString();
                            //string S_unit = dt_NewOrder.Rows[index]["S_unit"].ToString();
                            //string Notes_From = dt_NewOrder.Rows[index]["Notes_From"].ToString();
                            //string Notes_To = dt_NewOrder.Rows[index]["Notes_To"].ToString();
                            //string Notes = dt_NewOrder.Rows[index]["Notes"].ToString();

                            //Machine_Id = long.Parse(dt_NewOrder.Rows[index]["Machine_Id"].ToString());  
                            long Detailes_Purshing_Id_ = item.Id;// long.Parse(dt_NewOrder.Rows[index]["Detailes_Purshing_Id"].ToString());

                            long Related_Detailes_Purshing_Id = 0;

                            if (order_Purshing.Order_Type == (int)StoreOrderType.EstlamFromEntag_9 & IS_EstlamRawMaterial == true) //  استلام من الانتاج
                            {
                                mysql = "  INSERT INTO Item_Raw_MaterialsFor_Entage_Details(Item_Raw_MaterialsFor_Entage_List_Id, Unit_Item_Id, Item_Id, Store_Id, Larg_Qnty, Small_Qnty, Order_Purshing_Id, Detailes_Purshing_Id, Order_Estlam_Sarf_Flage)";
                                mysql = mysql + " VALUES(" + Item_Raw_MaterialsFor_Entage_List_Id + "," + Unit_Item_Table_Id + "," + Item_id + "," + item.Id_To + "," + item.qua + "," + item.Part_qua + "," + order_Purshing.Id + "," + Detailes_Purshing_Id_ + "," + Order_Estlam_Sarf_Flage + ") ";
                                opp2.insert_me(mysql);
                            }

                            if (Excp == 1)
                            {
                                Excp = 1;
                                Msg = "  خطأ فى إنشاء الإذن";
                                opp2.Rollback();
                                return;


                            }
                        }
                        /*
                        for (int index = 0; index < dt_NewOrder.Rows.Count; index++)
                        {

                            long Unit_Item_Table_Id = long.Parse(dt_NewOrder.Rows[index]["Unit_Item_Table_Id"].ToString());
                            long Item_id = long.Parse(dt_NewOrder.Rows[index]["Item_id"].ToString());
                            decimal Larg_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Larg_Qnty"].ToString());
                            decimal Small_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Small_Qnty"].ToString());
                            decimal Item_NO = decimal.Parse(dt_NewOrder.Rows[index]["Item_NO"].ToString());

                            decimal Balance_Now_Larg_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Balance_Now_Larg_Qnty"].ToString());
                            decimal Balance_Now_Small_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Balance_Now_Small_Qnty"].ToString());
                            decimal Balance_Now_Item_No = decimal.Parse(dt_NewOrder.Rows[index]["Balance_Now_Item_No"].ToString());
                            decimal S_unit_Value = decimal.Parse(dt_NewOrder.Rows[index]["S_unit_Value"].ToString());

                            string Item_Name = dt_NewOrder.Rows[index]["Item_Name"].ToString();
                            string B_Unit = dt_NewOrder.Rows[index]["B_Unit"].ToString();
                            string S_unit = dt_NewOrder.Rows[index]["S_unit"].ToString();
                            string Notes_From = dt_NewOrder.Rows[index]["Notes_From"].ToString();
                            string Notes_To = dt_NewOrder.Rows[index]["Notes_To"].ToString();
                            string Notes = dt_NewOrder.Rows[index]["Notes"].ToString();

                            Machine_Id = long.Parse(dt_NewOrder.Rows[index]["Machine_Id"].ToString()); ;
                            long Detailes_Purshing_Id_ = long.Parse(dt_NewOrder.Rows[index]["Detailes_Purshing_Id"].ToString());

                            long Related_Detailes_Purshing_Id = 0;

                            if (Order_Purshing.Order_Type == 9 & IS_EstlamRawMaterial == true) //  استلام من الانتاج
                            {
                                mysql = "  INSERT INTO Item_Raw_MaterialsFor_Entage_Details(Item_Raw_MaterialsFor_Entage_List_Id, Unit_Item_Id, Item_Id, Store_Id, Larg_Qnty, Small_Qnty, Order_Purshing_Id, Detailes_Purshing_Id, Order_Estlam_Sarf_Flage)";
                                mysql = mysql + " VALUES(" + Item_Raw_MaterialsFor_Entage_List_Id + "," + Unit_Item_Table_Id + "," + Item_id + "," + Id_To + "," + Larg_Qnty + "," + Small_Qnty + "," + OrderTable_id + "," + Detailes_Purshing_Id_ + "," + Order_Estlam_Sarf_Flage + ") ";
                                opp2.insert_me(mysql);
                            }

                            if (Excp == 1)
                            {
                                Excp = 1;
                                Msg = "  خطأ فى إنشاء الإذن";
                                opp2.Rollback();
                                return;


                            }
                        }
                        */
                        /////////////////////////////////
                        //MakeNewOrderTOEntag_RawMaterials(ref Excp, ref Msg, ref opp2, dt_RawMaterials, dt_SubItem, OrderTable_id, dt_Order, Item_Raw_MaterialsFor_Entage_List_Id);
                        MakeNewOrderTOEntag_RawMaterials(ref Excp, ref Msg, ref opp, dt_RawMaterials, dt_SubItem, order_Purshing, Item_Raw_MaterialsFor_Entage_List_Id);

                        if (Excp == 0)
                        {
                            opp2.Commit();

                            Msg = "تم تعديل الإذن بنجاح";

                        }
                        Msg = "تم تعديل الإذن بنجاح";

                    }


                }



                if (Excp == 1)
                {
                    Excp = 1;
                    Msg = "  خطأ فى إنشاء الإذن";
                    opp.Rollback();
                    return;
                }

            }
            catch (Exception ee)
            {

                Excp = 1;
                Msg = "  خطأ فى إنشاء الإذن";
                opp.Rollback();
                operations.DataDictionaryLogerException(ee);
                return;




            }
        }

        public void Make_NewOrderFromOK(ref int Excp, ref string Msg, DataTable dt_RawMaterials, DataTable dt_SubItem, ref long OrderTable_id, int Order_Estlam_Sarf_Flage, bool IS_EstlamRawMaterial)
        {
            //if (OrderType != 3 & OrderType != 4 & OrderType != 7)              /// ليست مرتجع صرف أو  استلام
            //{
            //opp = new operations();
            opp.BeginTransaction(2);
            //(IEntityWithChangeTracker)Order_Purshing).SetChangeTracker(null);
            try
            {
                ///////////////////////////////////////////////////////////////////////////////////////
                Excp = 0;
                // when creating new order ,it execute first time click button
                OrderTable_id = 0;
                long OrderTable_id_C = 0;
                long Item_Raw_MaterialsFor_Entage_List_Id = 0;
                OrderChoiceCLS oc = new OrderChoiceCLS();


                //التأكد من رصيد الصنف فى حالة الصرف
                if (Order_Estlam_Sarf_Flage == 2)
                {
                    foreach (var item in Detailes_PurshingList)
                    {
                        item.OrderTable_id = order_Purshing.Id;
                        ItemCLS itm = new ItemCLS();
                        Unit_item um = itm.GetUnit_item(ref opp, item.Unit_item_Table_Id);
                        if (um == null)
                        {
                            Excp = 1;
                            Msg = "خطأ فى إضافة صنف إلى الإذن";
                            opp.Rollback();
                            //MessageBox.Show();
                            return;
                        }
                        string Item_Name = item.Item_Name;// dt_NewOrder.Rows[index]["Item_Name"].ToString();
                        decimal S_unit_Value = um.Small_unit.Value;// decimal.Parse(dt.Rows[0]["Small_unit"].ToString());


                        //التأكد من رصيد الصنف فى حالة الصرف
                        if (um.Item.Is_ItemOrService == 2)
                        {
                            continue;
                        }

                        decimal X1 = 0;

                        Cls_Conversion_Unit clc = new Cls_Conversion_Unit();
                        if (!clc.ConVersionQuntity_Subtract_ToItem(true, item.qua.Value, item.Part_qua.Value, um.Full_qua.Value, um.D_Qua.Value, ref X1, ref X1, S_unit_Value))
                        {
                            Excp = 1;
                            Msg = " رصيد الصنف غير كافى " + Item_Name;
                            opp.Rollback();
                            return;
                        }




                    }
                    //// main order id
                }
                order_Purshing.Order_Purshing_No = GetNewOrderNo(ref opp, order_Purshing.Order_Type.Value);
                order_Purshing.IsActiveMainOrder_Purshing = 1;
                order_Purshing.OperationDate = DateTime.Now.Date;
                //add Order_Purshing 
                AddOrder_Purshing(ref Excp, ref opp, ref OrderTable_id, order_Purshing);




                if (order_Purshing.Order_Type.Value == (int)StoreOrderType.EstlamFromEntag_9 & IS_EstlamRawMaterial == true) //  استلام من الانتاج
                {
                    //OrderChoiceCLS oc = new OrderChoiceCLS();
                    string Process_No = oc.GetNewItem_Raw_MaterialsFor_Entage_List().ToString();
                    string Notes_List = "تم إصدار الإذن والإرتباط بشكل اوتوماتيكى من معادلة الانتاج";
                    Notes_List = Notes_List + " .. " + order_Purshing.NotesAll;

                    string mysql = " INSERT INTO Item_Raw_MaterialsFor_Entage_List (Process_No, Notes_List, Process_Date) ";
                    mysql = mysql + " VALUES(" + Process_No + ",'" + Notes_List + "','" + operations.dateFormat(order_Purshing.Order_date) + "')";
                    Item_Raw_MaterialsFor_Entage_List_Id = opp.Insert_me_Return_Last_Identify(mysql);
                }
                if (Excp > 0)
                {
                    Excp = 1;
                    Msg = "  خطأ فى إنشاء الإذن";
                    opp.Rollback();
                    return;
                }

                if (order_Purshing.Order_Type.Value == (int)StoreOrderType.SarfTahweel_1 | order_Purshing.Order_Type.Value == (int)StoreOrderType.MortagaSarfTahweel_6)
                {
                    if (Common.Make_Store_Order_Sarf_Estlam_Ta7weel_Automatic_Between_Stores == 1)
                    {
                        //Order_DetailsCls od_CC = new Order_DetailsCls();
                        ///converted Order_Purshing 
                        CreateConvertedOrderPurchase(ref Excp, ref Msg, Order_Estlam_Sarf_Flage, ref OrderTable_id_C);


                    }
                }
                ////Create Order Details

                foreach (var item in Detailes_PurshingList)
                {

                    item.OrderTable_id = order_Purshing.Id;
                    ItemCLS itm = new ItemCLS();
                    Unit_item um = itm.GetUnit_item(ref opp, item.Unit_item_Table_Id);


                    long Related_Detailes_Purshing_Id = 0;
                    //Make_Order_Details_AllOneClick(ref Excp, ref opp, ref Related_Detailes_Purshing_Id, Order_Estlam_Sarf_Flage, OrderTable_id, Unit_Item_Table_Id, Item_id, Larg_Qnty, Small_Qnty, Item_NO, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, Balance_Now_Item_No, S_unit_Value, Notes_From, Notes_To, Notes, "", FromStoreId, ToStoreId);
                    Make_Order_Details_AllOneClick(ref Excp, ref opp, order_Purshing, item, ref Related_Detailes_Purshing_Id, um);
                    if (order_Purshing.Order_Type.Value == (int)StoreOrderType.EstlamFromEntag_9 & IS_EstlamRawMaterial == true) //  استلام من الانتاج
                    {
                        string mysql = "  INSERT INTO Item_Raw_MaterialsFor_Entage_Details(Item_Raw_MaterialsFor_Entage_List_Id, Unit_Item_Id, Item_Id, Store_Id, Larg_Qnty, Small_Qnty, Order_Purshing_Id, Detailes_Purshing_Id, Order_Estlam_Sarf_Flage)";
                        mysql = mysql + " VALUES(" + Item_Raw_MaterialsFor_Entage_List_Id + "," + item.Unit_item_Table_Id + "," + item.Item_Id + "," + item.Id_To + "," + item.qua.Value + "," + item.Part_qua.Value + "," + order_Purshing.Id + "," + Related_Detailes_Purshing_Id + "," + Order_Estlam_Sarf_Flage + ") ";
                        opp.insert_me(mysql);
                    }

                    if (Excp == 1)
                    {
                        Excp = 1;
                        Msg = "  خطأ فى إنشاء الإذن";
                        opp.Rollback();
                        return;


                    }
                }

                ////////////////////////
                if (order_Purshing.Order_Type.Value == (int)StoreOrderType.EstlamFromEntag_9 & IS_EstlamRawMaterial == true) //  استلام من الانتاج
                {
                    MakeNewOrderTOEntag_RawMaterials(ref Excp, ref Msg, ref opp, dt_RawMaterials, dt_SubItem, order_Purshing, Item_Raw_MaterialsFor_Entage_List_Id);
                    if (Excp == 0)
                    {
                        //MakeItem_Raw_MaterialsFor_Entage(ref Excp, ref Msg, ref opp, dt_RawMaterials, OrderTable_id,0, dt_Order);
                    }

                }
                if (Excp == 1)
                {
                    Excp = 1;
                    Msg = "  خطأ فى إنشاء الإذن";
                    opp.Rollback();
                    return;
                }
                if (Excp == 0)
                {
                    opp.Commit();
                    Msg = "تم إصدار الإذن بنجاح";

                }
            }
            catch (Exception ee)
            {

                Excp = 1;
                Msg = "  خطأ فى إنشاء الإذن";


                opp.Rollback();
                return;




            }
        }

        private void CreateConvertedOrderPurchase(ref int Excp, ref string Msg, int Order_Estlam_Sarf_Flage, ref long OrderTable_id_C)
        {
            Excp = 0;
            od_C = new Order_Purshing();
            od_C.NotesAll = order_Purshing.NotesAll;
            long OrderNo_C = GetNewOrderNo(ref opp, order_Purshing.Order_Type.Value);
            od_C.Order_Purshing_No = OrderNo_C;
            od_C.Order_date = order_Purshing.Order_date;
            od_C.Order_Type = order_Purshing.Order_Type;

            //od_C.type_From = order_Purshing.Type_To;
            //od_C.Id_From = order_Purshing.Id_To;
            //od_C.Type_To = order_Purshing.type_From;
            //od_C.Id_To = order_Purshing.Id_From;
            //od_C.Id_FromStr = order_Purshing.Id_ToStr;
            //od_C.Id_ToStr = order_Purshing.Id_FromStr;

            od_C.type_From = order_Purshing.type_From;
            od_C.Id_From = order_Purshing.Id_From;
            od_C.Type_To = order_Purshing.Type_To;
            od_C.Id_To = order_Purshing.Id_To;
            od_C.Id_FromStr = order_Purshing.Id_FromStr;
            od_C.Id_ToStr = order_Purshing.Id_ToStr;

            od_C.Invoice_ID = order_Purshing.Invoice_ID;
            od_C.MainOrderTable_id = order_Purshing.Id;
            od_C.IsActiveMainOrder_Purshing = 0;
            od_C.Is_Converted = 1;
            od_C.User_ID = Common.User_Id;
            od_C.Branch_ID = Common.Branch_Id;
            od_C.IS_EstlamRawMaterial = order_Purshing.IS_EstlamRawMaterial;
            od_C.CarsWorkOrdersId = order_Purshing.CarsWorkOrdersId;
            od_C.OrderTitle = order_Purshing.OrderTitle;
            int Order_Estlam_Sarf_Flage_C = 1;
            if (Order_Estlam_Sarf_Flage == 2)
            {
                Order_Estlam_Sarf_Flage_C = 1;
            }
            else
            {
                Order_Estlam_Sarf_Flage_C = 2;
            }
            od_C.Transaction_Type = Order_Estlam_Sarf_Flage_C;
            try
            {
                //add Order_Purshing Converted
                AddOrder_Purshing(ref Excp, ref opp, ref OrderTable_id_C, od_C);

                ////Link Main Order To Converted Order
                Update_Order_FromMainId(ref Excp, ref opp, order_Purshing, od_C);

                //string mysql = "Update Order_Purshing Set  MainOrderTable_id =" + OrderTable_id_C + " Where Id=" + OrderTable_id;
                //opp.insert_me(mysql);
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }
        }

        void MakeNewOrderTOEntag_RawMaterials(ref int Excp, ref string Msg, ref operations opp, DataTable dt_RawMaterials, DataTable dt_subItem, Order_Purshing opMain, long Item_Raw_MaterialsFor_Entage_List_Id)
        {
            //TransferItems id = new TransferItems(10);
            //id.Text = "إذن صرف إلى الإنتاج";

            int Order_Estlam_Sarf_Flage = 2;

            Order_Purshing op_TOEntag = new Order_Purshing();
            op_TOEntag.Order_Type = (int)StoreOrderType.SarfToEntag_10;

            op_TOEntag.Type_To = 10;
            op_TOEntag.Id_To = 10;

            op_TOEntag.type_From = 4;
            op_TOEntag.Id_From = 0;

            /////البحث عن ارقام المخازن لعمل اذن لكل منها 
            DataView view = new DataView(dt_RawMaterials);
            DataTable dt_Stores = view.ToTable(true, "S_id");

            for (int y = 0; y < dt_Stores.Rows.Count; y++)
            {
                op_TOEntag.Id_From = int.Parse(dt_Stores.Rows[y]["S_Id"].ToString());
                Excp = 0;
                // when creating new order ,it execute first time click button

                OrderChoiceCLS oc = new OrderChoiceCLS();
                long Sub_OrderTable_idNew = 0;
                op_TOEntag.Order_Purshing_No = oc.GetNewOrderNo(ref opp, op_TOEntag.Order_Type.Value);
                //MainOrderTable_id = MainOrderTable_id_;
                op_TOEntag.Order_date = opMain.Order_date;
                long op_TOEntag_Id = 0;
                AddOrder_Purshing(ref Excp, ref opp, ref Sub_OrderTable_idNew, op_TOEntag);


                #region Insert Sub Item
                for (int index = 0; index < dt_subItem.Rows.Count; index++)
                {
                    Detailes_Purshing dp = new Detailes_Purshing();
                    long Store_id = long.Parse(dt_subItem.Rows[index]["S_id"].ToString());
                    string Store_Name = dt_subItem.Rows[index]["Store_Name"].ToString();
                    if (Store_id != op_TOEntag.Id_From)
                    {
                        continue;
                    }

                    decimal Item_id_ = decimal.Parse(dt_subItem.Rows[index]["SubItem_Id"].ToString());
                    int Item_id = Convert.ToInt32(Item_id_);

                    long Unit_Item_Table_Id = 0;
                    decimal Balance_Now_Larg_Qnty = 0;
                    decimal Balance_Now_Small_Qnty = 0;
                    decimal Balance_Now_Item_No = 0;

                    ItemCLS itm = new ItemCLS();
                    Unit_item um = itm.GetUnit_item(ref opp, Item_id, Store_id);
                    //string mysql = "Select * from Unit_Item Where Item_Id=" + Item_id + " And Store_Id=" + Store_id;
                    //operations opp2 = new operations();
                    //DataTable dt2 = opp2.return_DataTable(mysql);
                    if (um != null)
                    {
                        Unit_Item_Table_Id = um.id;// long.Parse(dt2.Rows[0]["Id"].ToString());
                        Balance_Now_Larg_Qnty = um.Full_qua.Value;// decimal.Parse(dt2.Rows[0]["Full_Qua"].ToString());
                        Balance_Now_Small_Qnty = um.D_Qua.Value;// decimal.Parse(dt2.Rows[0]["D_Qua"].ToString());
                        Balance_Now_Item_No = um.Item_No.Value;// decimal.Parse(dt2.Rows[0]["Item_No"].ToString());

                    }



                    decimal Larg_Qnty = decimal.Parse(dt_subItem.Rows[index]["Larg_Qnty"].ToString());
                    decimal Small_Qnty = decimal.Parse(dt_subItem.Rows[index]["Small_Qnty"].ToString());
                    decimal Item_NO = 0;
                    decimal Qnty = decimal.Parse(dt_subItem.Rows[index]["Qnty"].ToString());

                    decimal S_unit_Value = decimal.Parse(dt_subItem.Rows[index]["S_unit_Value"].ToString());

                    string Item_Name = dt_subItem.Rows[index]["SubItem_Name"].ToString();
                    string B_Unit = dt_subItem.Rows[index]["Big_unit"].ToString();
                    string S_unit = dt_subItem.Rows[index]["Small_unit"].ToString();
                    string Notes_From = "من مخزن  " + Store_Name; ;
                    string Notes_To = "صرف إلى الإنتاج";
                    string Notes = "تم إصدار الإذن اوتوماتيكيا ";


                    long Related_Detailes_Purshing_Id = 0;
                    long FromStoreId = Common.GetColumnValueLong(dt_subItem, "FromStoreId", index);
                    long ToStoreId = Common.GetColumnValueLong(dt_subItem, "ToStoreId", index);



                    dp.OrderTable_id = Sub_OrderTable_idNew;
                    dp.Unit_item_Table_Id = Unit_Item_Table_Id;
                    dp.Item_Id = Item_id;
                    dp.qua = Larg_Qnty;
                    dp.Part_qua = Small_Qnty;
                    dp.Item_No = Item_NO;
                    dp.Notes_From = Notes_From;
                    dp.Notes_To = Notes_To;
                    dp.Notes = Notes;
                    dp.Id_From = FromStoreId;
                    dp.Id_To = ToStoreId;

                    //Make_Order_Details_AllOneClick(ref Excp, ref opp, ref Related_Detailes_Purshing_Id, Order_Estlam_Sarf_Flage, Sub_OrderTable_idNew, Unit_Item_Table_Id, Item_id, Larg_Qnty, Small_Qnty, Item_NO, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, Balance_Now_Item_No, S_unit_Value, Notes_From, Notes_To, Notes, "", FromStoreId, ToStoreId);
                    Make_Order_Details_AllOneClick(ref Excp, ref opp, op_TOEntag, dp, ref Related_Detailes_Purshing_Id, um);
                    string mysql = "  INSERT INTO Item_Raw_MaterialsFor_Entage_Details(Item_Raw_MaterialsFor_Entage_List_Id, Unit_Item_Id, Item_Id, Store_Id, Larg_Qnty, Small_Qnty, Order_Purshing_Id, Detailes_Purshing_Id, Order_Estlam_Sarf_Flage)";
                    mysql = mysql + " VALUES(" + Item_Raw_MaterialsFor_Entage_List_Id + "," + Unit_Item_Table_Id + "," + Item_id + "," + Store_id + "," + Larg_Qnty + "," + Small_Qnty + "," + Sub_OrderTable_idNew + "," + Related_Detailes_Purshing_Id + "," + Order_Estlam_Sarf_Flage + ") ";

                    opp.insert_me(mysql);
                    //if (Excp == 1)
                    //{
                    //    Excp = 1;
                    //    Msg = "  خطأ فى إنشاء الإذن";
                    //    opp.Rollback();
                    //    return;


                    //}
                }
                #endregion


                DataRow[] foundRows;


                foundRows = dt_RawMaterials.Select("S_Id=" + op_TOEntag.Id_From + "");
                DataTable dt = foundRows.CopyToDataTable();
                MakeItem_Raw_MaterialsFor_Entage(ref Excp, ref Msg, ref opp, dt, opMain.Id, Sub_OrderTable_idNew, opMain.Order_date.Value);
            }


            ////////////////////////

            //if (Excp == 0)
            //{
            //    opp.Commit();
            //    Msg = "تم إصدار الإذن بنجاح";

            //}
        }
        void MakeItem_Raw_MaterialsFor_Entage(ref int Excp, ref string Msg, ref operations opp, DataTable dt_RawMaterials, long MainOrderTable_id_, long SubOrderTable_id, DateTime Process_Date)
        {
            //
            try
            {
                long MainOrderTable_id = MainOrderTable_id_;


                for (int x = 0; x < dt_RawMaterials.Rows.Count; x++)
                {
                    long SubUnit_item_id = long.Parse(dt_RawMaterials.Rows[x]["SubUnit_item_id"].ToString());

                    long MainItem_Id = long.Parse(dt_RawMaterials.Rows[x]["MainItem_Id"].ToString());
                    long SubItem_Id = long.Parse(dt_RawMaterials.Rows[x]["SubItem_Id"].ToString());
                    string MainItem_Name = dt_RawMaterials.Rows[x]["MainItem_Name"].ToString();
                    string SubItem_Name = dt_RawMaterials.Rows[x]["SubItem_Name"].ToString();
                    string Big_unit = dt_RawMaterials.Rows[x]["Big_unit"].ToString();
                    string Small_unit = dt_RawMaterials.Rows[x]["Small_unit"].ToString();
                    long Unit_Id = long.Parse(dt_RawMaterials.Rows[x]["Unit_Id"].ToString());
                    decimal S_unit_Value = decimal.Parse(dt_RawMaterials.Rows[x]["S_unit_Value"].ToString());

                    decimal Larg_Qnty = decimal.Parse(dt_RawMaterials.Rows[x]["Larg_Qnty"].ToString());
                    decimal Small_Qnty = decimal.Parse(dt_RawMaterials.Rows[x]["Small_Qnty"].ToString());
                    decimal Qnty = decimal.Parse(dt_RawMaterials.Rows[x]["Qnty"].ToString());


                    decimal Raw_Full_qua = decimal.Parse(dt_RawMaterials.Rows[x]["Full_qua"].ToString());
                    decimal Raw_D_Qua = decimal.Parse(dt_RawMaterials.Rows[x]["D_Qua"].ToString());
                    decimal Raw_Quantity = decimal.Parse(dt_RawMaterials.Rows[x]["Quantity"].ToString());

                    decimal Main_Larg_Qnty = decimal.Parse(dt_RawMaterials.Rows[x]["Entry_Larg_Qnty"].ToString());
                    decimal Main_Small_Qnty = decimal.Parse(dt_RawMaterials.Rows[x]["Entry_Small_Qnty"].ToString());
                    decimal Main_Qnty = decimal.Parse(dt_RawMaterials.Rows[x]["Entry_Qnty"].ToString());

                    string SubStore_Name = dt_RawMaterials.Rows[x]["Store_Name"].ToString();
                    string SubStore_Id = dt_RawMaterials.Rows[x]["S_Id"].ToString();
                    string MainStore_Id = dt_RawMaterials.Rows[x]["MainStore_Id"].ToString();
                    string mysql = "INSERT INTO Item_Raw_MaterialsFor_Entage(MainOrderTable_id,SubOrderTable_id, Process_Date, MainItem_Id, SubItem_Id, MainItem_Name, SubItem_Name, Big_unit, Small_unit, Unit_Id, S_unit_Value, Raw_Full_qua, Raw_D_Qua, Raw_Quantity, Larg_Qnty, Small_Qnty,Qnty, Main_Larg_Qnty, Main_Small_Qnty, Main_Qnty,SubUnit_item_id, MainStore_Id, SubStore_Id,SubStore_Name) ";
                    mysql = mysql + " VALUES(" + MainOrderTable_id + "," + SubOrderTable_id + ",'" + operations.dateFormat(Process_Date) + "'," + MainItem_Id + "," + SubItem_Id + ",'" + MainItem_Name + "','" + SubItem_Name + "','" + Big_unit + "','" + Small_unit + "'," + Unit_Id + "," + S_unit_Value + "," + Raw_Full_qua + "," + Raw_D_Qua + "," + Raw_Quantity + "," + Larg_Qnty + "," + Small_Qnty + "," + Qnty + "," + Main_Larg_Qnty + "," + Main_Small_Qnty + "," + Main_Qnty + "," + SubUnit_item_id + "," + MainStore_Id + ", " + SubStore_Id + ",'" + SubStore_Name + "')";
                    opp.insert_me(mysql);
                }
            }
            catch (Exception ee)
            {

                Excp = 1;
                Msg = ee.Message;
            }
        }
        public void Make_Order_Details_AllOneClick(ref int Excp, ref operations opp, Order_Purshing order_Purshing, Detailes_Purshing dp, ref long Related_Detailes_Purshing_Id, Unit_item uitm)
        {

            //Make_Order_Details_(ref Excp, ref opp, ref Related_Detailes_Purshing_Id, Order_Estlam_Sarf_Flage, OrderTable_id, Unit_Item_Table_Id, Item_id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, Balance_Now_Item_No, S_unit_Value, Notes_From, Notes_To, Notes, Notes_Item, FromStoreId, ToStoreId);

            //long OrderTable_id, long Unit_Item_Table_Id, long Item_id, decimal Larg_Qnty, decimal Small_Qnty, decimal ItemNO, decimal Balance_Now_Larg_Qnty, decimal Balance_Now_Small_Qnty, decimal Balance_Now_Item_No, decimal S_unit_Value, string Notes_From, string Notes_To, string Notes, string Notes_Item, long FromStoreId, long ToStoreId

            int Order_Estlam_Sarf_Flage = dp.Transaction_Type.Value;

            long Unit_Item_Table_Id = uitm.id;
            long Item_id = uitm.Item_Id.Value;
            decimal Larg_Qnty = dp.qua.Value;
            decimal Small_Qnty = dp.Part_qua.Value;
            decimal ItemNO = dp.Item_No.Value;
            decimal Balance_Now_Larg_Qnty = uitm.Full_qua.Value;
            decimal Balance_Now_Small_Qnty = uitm.D_Qua.Value;
            decimal Balance_Now_Item_No = uitm.Item_No.Value;
            decimal S_unit_Value = uitm.Small_unit.Value;

            //تعمل مع تغيير هوية صنف

            string mysql = "";

            decimal Qua_Befor = 0;
            decimal Part_qua_Befor = 0;
            decimal Item_No_Befor = 0;
            decimal Qua_After = 0;
            decimal Part_qua_After = 0;
            decimal Item_No_After = 0;
            Related_Detailes_Purshing_Id = 0;




            DataSet dss;
            Cls_Conversion_Unit clc = new Cls_Conversion_Unit();

            //الغاء الكسور من الكميات  
            Cls_Conversion_Unit.ConVert_By_S_UnitValue(ref Larg_Qnty, ref Small_Qnty, S_unit_Value);


            ////فى حالة زيادة الرصيد

            decimal Balance_Id_From_Larg_Qnty = 0;
            decimal Balance_Id_From_Small_Qnty = 0;
            decimal Balance_Id_From_Item_No = Balance_Now_Item_No - ItemNO;

            ////فى حالة نقص الرصيد
            decimal Balance_Id_To_Larg_Qnty = 0;
            decimal Balance_Id_To_Small_Qnty = 0;
            decimal Balance_Id_To_Item_No = Balance_Now_Item_No + ItemNO;


            decimal Unit_Item_Table_Larg_Qnty = 0;
            decimal Unit_Item_Table__Small_Qnty = 0;
            decimal Unit_Item_Table_Item_No = 0;
            long To_Store_Unit_Item_Table_Id = 0;
            //decimal Larg_Qnty, decimal Small_Qnty, decimal 
            if (ItemNO == 0)
            {
                if (Common.Compute_3eboaFrom_EznSarf_Estlam == 1)
                {
                    ItemNO = Common.Get3ebowa(Larg_Qnty, Item_id);
                }
            }




            #region    mortaga3 sarf or  estlam
            //else if (Order_Type == 3 | Order_Type == 4)
            if (Order_Estlam_Sarf_Flage == 1)
            {
                ////تعديل بيانات المخزن المنصرف إلية بزيادة الرصيد
                clc.ConVersionQuntity_ADD_ToItem(Larg_Qnty, Small_Qnty, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, ref Balance_Id_To_Larg_Qnty, ref Balance_Id_To_Small_Qnty, S_unit_Value);

                Unit_Item_Table_Larg_Qnty = Balance_Id_To_Larg_Qnty;
                Unit_Item_Table__Small_Qnty = Balance_Id_To_Small_Qnty;
                Unit_Item_Table_Item_No = Balance_Id_To_Item_No;


                UpdatePriceCostAvg(opp, order_Purshing, dp, uitm, Order_Estlam_Sarf_Flage, Unit_Item_Table_Id, Larg_Qnty, Small_Qnty, ItemNO);
                UpdateQnty(opp, order_Purshing, dp, uitm, Order_Estlam_Sarf_Flage, Unit_Item_Table_Id, Larg_Qnty, Small_Qnty, ItemNO);

                To_Store_Unit_Item_Table_Id = -1;

                Balance_Id_From_Larg_Qnty = Balance_Id_To_Larg_Qnty;
                Balance_Id_From_Small_Qnty = Balance_Id_To_Small_Qnty;
                Balance_Id_From_Item_No = Balance_Id_To_Item_No;
                ////////////////////////////////////////////////////////////
                Qua_Befor = Balance_Now_Larg_Qnty;
                Part_qua_Befor = Balance_Now_Small_Qnty;
                Item_No_Befor = Balance_Now_Item_No;

                Qua_After = Unit_Item_Table_Larg_Qnty;
                Part_qua_After = Unit_Item_Table__Small_Qnty;
                Item_No_After = Balance_Id_To_Item_No;

                //////////////////////////////////////////////////////////


                //Related_Detailes_Purshing_Id = InsertToDetailes_Purshing(ref opp, Order_Estlam_Sarf_Flage, Order_Type, OrderTable_id, Unit_Item_Table_Id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Id_From_Larg_Qnty, Balance_Id_From_Small_Qnty, Balance_Id_From_Item_No, Balance_Id_To_Larg_Qnty, Balance_Id_To_Small_Qnty, Balance_Id_To_Item_No, To_Store_Unit_Item_Table_Id, Notes_From, Notes_To, Notes, Machine_Id, Qua_Befor, Part_qua_Befor, Item_No_Befor, Qua_After, Part_qua_After, Item_No_After, S_unit_Value, 0, Notes_Item, FromStoreId, ToStoreId);
                Related_Detailes_Purshing_Id = InsertToDetailes_Purshing(ref opp, dp, S_unit_Value, order_Purshing);


            }
            #endregion

            #region     sarf or mortaga3 estlam
            //else if (Order_Type == 2 | Order_Type == 5 | Order_Type == 8 | Order_Type == 10)
            else if (Order_Estlam_Sarf_Flage == 2)
            {

                ////تعديل بيانات المخزن المنصرف منة بنقص الرصيد
                clc.ConVersionQuntity_Subtract_ToItem(false, Larg_Qnty, Small_Qnty, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, ref Balance_Id_From_Larg_Qnty, ref Balance_Id_From_Small_Qnty, S_unit_Value);

                Unit_Item_Table_Larg_Qnty = Balance_Id_From_Larg_Qnty;
                Unit_Item_Table__Small_Qnty = Balance_Id_From_Small_Qnty;
                Unit_Item_Table_Item_No = Balance_Id_From_Item_No;


                UpdatePriceCostAvg(opp, order_Purshing, dp, uitm, Order_Estlam_Sarf_Flage, Unit_Item_Table_Id, Larg_Qnty, Small_Qnty, ItemNO);
                UpdateQnty(opp, order_Purshing, dp, uitm, Order_Estlam_Sarf_Flage, Unit_Item_Table_Id, Larg_Qnty, Small_Qnty, ItemNO);





                To_Store_Unit_Item_Table_Id = -1;


                Balance_Id_To_Larg_Qnty = Balance_Id_From_Larg_Qnty;
                Balance_Id_To_Small_Qnty = Balance_Id_From_Larg_Qnty;
                Balance_Id_To_Item_No = Balance_Id_From_Item_No;

                ////////////////////////////////////////////////////////////
                Qua_Befor = Balance_Now_Larg_Qnty;
                Part_qua_Befor = Balance_Now_Small_Qnty;
                Item_No_Befor = Balance_Now_Item_No;

                Qua_After = Unit_Item_Table_Larg_Qnty;
                Part_qua_After = Unit_Item_Table__Small_Qnty;
                Item_No_After = Balance_Id_To_Item_No;

                //////////////////////////////////////////////////////////


                //Related_Detailes_Purshing_Id = InsertToDetailes_Purshing(ref opp, Order_Estlam_Sarf_Flage, Order_Type, OrderTable_id, Unit_Item_Table_Id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Id_From_Larg_Qnty, Balance_Id_From_Small_Qnty, Balance_Id_From_Item_No, Balance_Id_To_Larg_Qnty, Balance_Id_To_Small_Qnty, Balance_Id_To_Item_No, To_Store_Unit_Item_Table_Id, Notes_From, Notes_To, Notes, Machine_Id, Qua_Befor, Part_qua_Befor, Item_No_Befor, Qua_After, Part_qua_After, Item_No_After, S_unit_Value, 0, Notes_Item, FromStoreId, ToStoreId);
                Related_Detailes_Purshing_Id = InsertToDetailes_Purshing(ref opp, dp, S_unit_Value, order_Purshing);



            }
            #endregion

            #region Update CarsWorkOrders
            SetEditOrderForCarsWorkOrders(dp, order_Purshing);


            #endregion

            #region Transform          تحويل
            if (order_Purshing.Order_Type == (int)StoreOrderType.SarfTahweel_1 | order_Purshing.Order_Type == (int)StoreOrderType.MortagaSarfTahweel_6)             //التحويل
            {
                ////زيادة الرصيد وتفعيل الاذن اوتوماتيك اى انشاء الاذن كأنة اذن صرف من واذن استلام الى
                if (Common.Make_Store_Order_Sarf_Estlam_Ta7weel_Automatic_Between_Stores == 1)/////
                {//////////////////////////////////////////////////////////

                    if (Order_Estlam_Sarf_Flage == 1)
                    {
                        ////تعديل بيانات المخزن المنصرف منة بنقص الرصيد

                        clc.ConVersionQuntity_Subtract_ToItem(false, Larg_Qnty, Small_Qnty, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, ref Balance_Id_From_Larg_Qnty, ref Balance_Id_From_Small_Qnty, S_unit_Value);

                        Unit_Item_Table_Larg_Qnty = Balance_Id_From_Larg_Qnty;
                        Unit_Item_Table__Small_Qnty = Balance_Id_From_Small_Qnty;
                        Unit_Item_Table_Item_No = Balance_Id_From_Item_No;

                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                        //arr.Add(mysql);
                        opp.insert_me(mysql);

                    }


                    ////////////////////////////////////////////////////////////
                    Qua_Befor = Balance_Now_Larg_Qnty;
                    Part_qua_Befor = Balance_Now_Small_Qnty;
                    Item_No_Befor = Balance_Now_Item_No;

                    Qua_After = Unit_Item_Table_Larg_Qnty;
                    Part_qua_After = Unit_Item_Table__Small_Qnty;
                    Item_No_After = Balance_Id_To_Item_No;



                    //////////////////// البحث عن رصيد المخزن المحول اليه بزيادة الرصيد
                    ///
                    dp_C = new Detailes_Purshing();

                    decimal To_Store_LargBalance = 0;
                    decimal To_Store_Small_Balance = 0;
                    decimal To_Store_Item_No = 0;

                    if (!Check_ItemFoundOnStore(ref opp, dp, ref To_Store_Unit_Item_Table_Id, ref To_Store_LargBalance, ref To_Store_Small_Balance, ref To_Store_Item_No))
                    {
                        Excp = 1;
                        return;
                    }

                    ////تعديل بيانات المخزن المنصرف إلية بزيادة الرصيد
                    clc.ConVersionQuntity_ADD_ToItem(Larg_Qnty, Small_Qnty, To_Store_LargBalance, To_Store_Small_Balance, ref Balance_Id_To_Larg_Qnty, ref Balance_Id_To_Small_Qnty, S_unit_Value);

                    decimal To_Unit_Item_Table_Larg_Qnty = Balance_Id_To_Larg_Qnty;
                    decimal To_Unit_Item_Table__Small_Qnty = Balance_Id_To_Small_Qnty;
                    decimal To_Unit_Item_Table_Item_No = To_Store_Item_No + ItemNO;
                    decimal To_Balance_Id_To_Item_No = Unit_Item_Table_Item_No;
                    if (Order_Estlam_Sarf_Flage == 2)
                    {
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + "  WHERE (id = " + To_Store_Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                    }


                    dp_C.Unit_item_Table_Id = To_Store_Unit_Item_Table_Id;
                    dp_C.To_Store_Unit_Item_Table_Id = dp.Unit_item_Table_Id;

                    dp_C.Balance_Id_To_Larg_Qnty = To_Store_LargBalance + Larg_Qnty;

                    dp_C.Balance_Id_To_Small_Qnty = To_Store_Small_Balance + Small_Qnty;
                    dp_C.Balance_Id_To_Item_No = To_Store_Item_No + ItemNO;



                    dp_C.Qua_Befor = To_Store_LargBalance;
                    dp_C.Part_qua_Befor = To_Store_Small_Balance;
                    dp_C.Item_No_Befor = To_Store_Item_No;

                    dp_C.Qua_After = To_Unit_Item_Table_Larg_Qnty;
                    dp_C.Part_qua_After = To_Unit_Item_Table__Small_Qnty;
                    dp_C.Item_No_After = To_Balance_Id_To_Item_No;


                    dp_C.Balance_Id_From_Larg_Qnty = Balance_Now_Larg_Qnty - Larg_Qnty;

                    dp_C.Balance_Id_From_Small_Qnty = Balance_Now_Small_Qnty - Small_Qnty;
                    dp_C.Balance_Id_From_Item_No = Balance_Now_Item_No - ItemNO;

                    dp_C.CarsWorkOrdersId = dp.CarsWorkOrdersId;
                    dp_C.CarsWorkOrdersRequestedDetailsId = dp.CarsWorkOrdersRequestedDetailsId;
                    dp_C.CarsWorkOrdersDeliverDetailsId = dp.CarsWorkOrdersDeliverDetailsId;


                    ////////Create Details_Purchase Converted

                    CreateDetails_PurchaseConverted(ref dp_C, dp, uitm);
                    //long Sub_Related_Detailes_Purshing_Id = InsertToDetailes_Purshing(ref opp, Order_Estlam_Sarf_Flage_1, Order_Type, OrderTable_id_C, To_Store_Unit_Item_Table_Id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Id_To_Larg_Qnty, Balance_Id_To_Small_Qnty, Balance_Id_To_Item_No, Balance_Id_From_Larg_Qnty, Balance_Id_From_Small_Qnty, Balance_Id_From_Item_No, Unit_Item_Table_Id, Notes_From, Notes_To, Notes, Machine_Id, To_Qua_Befor, To_Part_qua_Befor, To_Item_No_Befor, To_Qua_After, To_Part_qua_After, To_Item_No_After, S_unit_Value, Related_Detailes_Purshing_Id, Notes_Item, FromStoreId, ToStoreId);
                    long Sub_Related_Detailes_Purshing_Id = InsertToDetailes_Purshing(ref opp, dp_C, S_unit_Value, order_Purshing);

                    mysql = " UPDATE Detailes_Purshing  SET  Related_Detailes_Purshing_Id=" + Related_Detailes_Purshing_Id + "    WHERE (id = " + Sub_Related_Detailes_Purshing_Id + ") ";
                    //arr.Add(mysql);
                    opp.insert_me(mysql);

                    mysql = " UPDATE Detailes_Purshing  SET  Related_Detailes_Purshing_Id=" + Sub_Related_Detailes_Purshing_Id + " ,To_Store_Unit_Item_Table_Id=" + To_Store_Unit_Item_Table_Id + "  WHERE (id = " + Related_Detailes_Purshing_Id + ") ";
                    //arr.Add(mysql);
                    opp.insert_me(mysql);

                }
                else
                {
                    To_Store_Unit_Item_Table_Id = 0;
                    Balance_Id_To_Larg_Qnty = 0;
                    Balance_Id_To_Small_Qnty = 0;
                    Balance_Id_To_Item_No = 0;

                }




            }
            #endregion

            #region Employee          صرف لموظف
            if (order_Purshing.Order_Type == (int)StoreOrderType.SarfToEmployee_20 | order_Purshing.Order_Type == (int)StoreOrderType.EstlamFromEmployee_21)             // 
            {
                long Unit_itemEmployeeBalanceId = 0;
                long Item_Id = dp.Item_Id;
                long EmployeeId = 0;


                if (Order_Estlam_Sarf_Flage == 1)
                {
                    EmployeeId = dp.Id_From;

                    if (Check_ItemFoundOnEmployee(ref opp, ref Unit_itemEmployeeBalanceId, Item_Id, EmployeeId))
                    {

                        mysql = " UPDATE Unit_itemEmployeeBalance  SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + ItemNO + " WHERE (id = " + Unit_itemEmployeeBalanceId + ") ";
                        //arr.Add(mysql);
                        opp.insert_me(mysql);
                    }
                }
                if (Order_Estlam_Sarf_Flage == 2)
                {
                    EmployeeId = dp.Id_To;
                    if (Check_ItemFoundOnEmployee(ref opp, ref Unit_itemEmployeeBalanceId, Item_Id, EmployeeId))
                    {
                        mysql = " UPDATE Unit_itemEmployeeBalance  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + "  WHERE (id = " + Unit_itemEmployeeBalanceId + ") ";
                        opp.insert_me(mysql);
                    }
                }


            }
            #endregion

            #region Suppliers           استلام من مورد
            if (order_Purshing.Order_Type == (int)StoreOrderType.EstlamFromSupplier_4)             // 
            {
                UpdateٍP_Purchase(opp, order_Purshing, dp, uitm, Order_Estlam_Sarf_Flage);

            }
            #endregion
            #region Suppliers           صرف الى عميل
            if (order_Purshing.Order_Type == (int)StoreOrderType.SarfToCustomer_2)             // 
            {
                UpdateٍS_Purchase(opp, order_Purshing, dp, uitm, Order_Estlam_Sarf_Flage);

            }
            #endregion

            ItemCLS itemCLS = new ItemCLS();
            decimal P_priceAvgCost = itemCLS.GetComputed_P_AvgCost(ref opp, dp.Item_Id);
        }

        void UpdatePriceCostAvg(operations opp, Order_Purshing order_Purshing, Detailes_Purshing dp, Unit_item uitm, int TransactionType, long unit_Item_Table_Id, decimal Larg_Qnty, decimal Small_Qnty, decimal ItemNO)
        {
            string mysql = "";
            decimal P_priceAvgCostNew = 0;
            if (TransactionType == 1)
            {
                if (order_Purshing.Order_Type == (int)StoreOrderType.EstlamFromSupplier_4)
                {
                    ////////UpdateAVGCostfor UnitItem
                    ///
                    ItemCLS itemCLS = new ItemCLS(ref opp);
                    P_priceAvgCostNew = itemCLS.ComputeUpdateP_AvgCost(ref opp, uitm, dp.Qnty ?? 0, dp.PriceUnit ?? 0, Larg_Qnty, Small_Qnty);


                }
                ////////UpdateAVGCostfor  Item 
            }
            if (TransactionType == 2)
            {

            }
        }

        void UpdateQnty(operations opp, Order_Purshing order_Purshing, Detailes_Purshing dp, Unit_item uitm, int TransactionType, long unit_Item_Table_Id, decimal Larg_Qnty, decimal Small_Qnty, decimal ItemNO)
        {
            string mysql = "";

            if (TransactionType == 1)
            {


                mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + " WHERE (id = " + uitm.id + ") ";
                opp.insert_me(mysql);
            }
            if (TransactionType == 2)
            {
                mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + ItemNO + " WHERE (id = " + uitm.id + ") ";
                //arr.Add(mysql);
                opp.insert_me(mysql);
            }
        }
        void UpdateٍP_Purchase(operations opp, Order_Purshing order_Purshing, Detailes_Purshing dp, Unit_item uitm, int TransactionType)
        {
            string mysql = "";

            mysql = " UPDATE Unit_item  SET  P_Price= " + dp.Larg_Unit_Price + ", P_price_LargeUnit =" + dp.Larg_Unit_Price + "   WHERE (id = " + uitm.id + ") ";
            opp.insert_me(mysql);

            mysql = " UPDATE  item  SET  P_Price= " + dp.Larg_Unit_Price + ", P_price_LargeUnit =" + dp.Larg_Unit_Price + "   WHERE (id = " + uitm.Item_Id + ") ";
            opp.insert_me(mysql);
        }
        void UpdateٍS_Purchase(operations opp, Order_Purshing order_Purshing, Detailes_Purshing dp, Unit_item uitm, int TransactionType)
        {
            string mysql = "";

            mysql = " UPDATE Unit_item  SET  S_Price= " + dp.Larg_Unit_Price + ", S_price_LargeUnit =" + dp.Larg_Unit_Price + "   WHERE (id = " + uitm.id + ") ";
            opp.insert_me(mysql);

            mysql = " UPDATE  item  SET  S_Price= " + dp.Larg_Unit_Price + ", S_price_LargeUnit =" + dp.Larg_Unit_Price + "   WHERE (id = " + uitm.Item_Id + ") ";
            opp.insert_me(mysql);
        }


        public void Make_Order_Details_AllOneClickOLD(ref int Excp, ref operations opp, Order_Purshing order_Purshing, Detailes_Purshing dp, ref long Related_Detailes_Purshing_Id, Unit_item uitm)
        {

            //Make_Order_Details_(ref Excp, ref opp, ref Related_Detailes_Purshing_Id, Order_Estlam_Sarf_Flage, OrderTable_id, Unit_Item_Table_Id, Item_id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, Balance_Now_Item_No, S_unit_Value, Notes_From, Notes_To, Notes, Notes_Item, FromStoreId, ToStoreId);

            //long OrderTable_id, long Unit_Item_Table_Id, long Item_id, decimal Larg_Qnty, decimal Small_Qnty, decimal ItemNO, decimal Balance_Now_Larg_Qnty, decimal Balance_Now_Small_Qnty, decimal Balance_Now_Item_No, decimal S_unit_Value, string Notes_From, string Notes_To, string Notes, string Notes_Item, long FromStoreId, long ToStoreId

            int Order_Estlam_Sarf_Flage = dp.Transaction_Type.Value;

            long Unit_Item_Table_Id = uitm.id;
            long Item_id = uitm.Item_Id.Value;
            decimal Larg_Qnty = dp.qua.Value;
            decimal Small_Qnty = dp.Part_qua.Value;
            decimal ItemNO = dp.Item_No.Value;
            decimal Balance_Now_Larg_Qnty = uitm.Full_qua.Value;
            decimal Balance_Now_Small_Qnty = uitm.D_Qua.Value;
            decimal Balance_Now_Item_No = uitm.Item_No.Value;
            decimal S_unit_Value = uitm.Small_unit.Value;

            //تعمل مع تغيير هوية صنف

            string mysql = "";

            decimal Qua_Befor = 0;
            decimal Part_qua_Befor = 0;
            decimal Item_No_Befor = 0;
            decimal Qua_After = 0;
            decimal Part_qua_After = 0;
            decimal Item_No_After = 0;
            Related_Detailes_Purshing_Id = 0;




            DataSet dss;
            Cls_Conversion_Unit clc = new Cls_Conversion_Unit();

            //الغاء الكسور من الكميات  
            Cls_Conversion_Unit.ConVert_By_S_UnitValue(ref Larg_Qnty, ref Small_Qnty, S_unit_Value);


            ////فى حالة زيادة الرصيد

            decimal Balance_Id_From_Larg_Qnty = 0;
            decimal Balance_Id_From_Small_Qnty = 0;
            decimal Balance_Id_From_Item_No = Balance_Now_Item_No - ItemNO;

            ////فى حالة نقص الرصيد
            decimal Balance_Id_To_Larg_Qnty = 0;
            decimal Balance_Id_To_Small_Qnty = 0;
            decimal Balance_Id_To_Item_No = Balance_Now_Item_No + ItemNO;


            decimal Unit_Item_Table_Larg_Qnty = 0;
            decimal Unit_Item_Table__Small_Qnty = 0;
            decimal Unit_Item_Table_Item_No = 0;
            long To_Store_Unit_Item_Table_Id = 0;
            //decimal Larg_Qnty, decimal Small_Qnty, decimal 
            if (ItemNO == 0)
            {
                if (Common.Compute_3eboaFrom_EznSarf_Estlam == 1)
                {
                    ItemNO = Common.Get3ebowa(Larg_Qnty, Item_id);
                }
            }




            #region    mortaga3 sarf or  estlam
            //else if (Order_Type == 3 | Order_Type == 4)
            if (Order_Estlam_Sarf_Flage == 1)
            {
                ////تعديل بيانات المخزن المنصرف إلية بزيادة الرصيد
                clc.ConVersionQuntity_ADD_ToItem(Larg_Qnty, Small_Qnty, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, ref Balance_Id_To_Larg_Qnty, ref Balance_Id_To_Small_Qnty, S_unit_Value);

                Unit_Item_Table_Larg_Qnty = Balance_Id_To_Larg_Qnty;
                Unit_Item_Table__Small_Qnty = Balance_Id_To_Small_Qnty;
                Unit_Item_Table_Item_No = Balance_Id_To_Item_No;

                mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                //arr.Add(mysql);
                opp.insert_me(mysql);
                //UpdateQnty(  opp,   Order_Type, int TransactionType, long Unit_Item_Table_Id, decimal Larg_Qnty, decimal Small_Qnty, decimal ItemNO);

                To_Store_Unit_Item_Table_Id = -1;

                Balance_Id_From_Larg_Qnty = Balance_Id_To_Larg_Qnty;
                Balance_Id_From_Small_Qnty = Balance_Id_To_Small_Qnty;
                Balance_Id_From_Item_No = Balance_Id_To_Item_No;
                ////////////////////////////////////////////////////////////
                Qua_Befor = Balance_Now_Larg_Qnty;
                Part_qua_Befor = Balance_Now_Small_Qnty;
                Item_No_Befor = Balance_Now_Item_No;

                Qua_After = Unit_Item_Table_Larg_Qnty;
                Part_qua_After = Unit_Item_Table__Small_Qnty;
                Item_No_After = Balance_Id_To_Item_No;

                //////////////////////////////////////////////////////////


                //Related_Detailes_Purshing_Id = InsertToDetailes_Purshing(ref opp, Order_Estlam_Sarf_Flage, Order_Type, OrderTable_id, Unit_Item_Table_Id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Id_From_Larg_Qnty, Balance_Id_From_Small_Qnty, Balance_Id_From_Item_No, Balance_Id_To_Larg_Qnty, Balance_Id_To_Small_Qnty, Balance_Id_To_Item_No, To_Store_Unit_Item_Table_Id, Notes_From, Notes_To, Notes, Machine_Id, Qua_Befor, Part_qua_Befor, Item_No_Befor, Qua_After, Part_qua_After, Item_No_After, S_unit_Value, 0, Notes_Item, FromStoreId, ToStoreId);
                Related_Detailes_Purshing_Id = InsertToDetailes_Purshing(ref opp, dp, S_unit_Value, order_Purshing);


            }
            #endregion

            #region     sarf or mortaga3 estlam
            //else if (Order_Type == 2 | Order_Type == 5 | Order_Type == 8 | Order_Type == 10)
            else if (Order_Estlam_Sarf_Flage == 2)
            {

                ////تعديل بيانات المخزن المنصرف منة بنقص الرصيد
                clc.ConVersionQuntity_Subtract_ToItem(false, Larg_Qnty, Small_Qnty, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, ref Balance_Id_From_Larg_Qnty, ref Balance_Id_From_Small_Qnty, S_unit_Value);

                Unit_Item_Table_Larg_Qnty = Balance_Id_From_Larg_Qnty;
                Unit_Item_Table__Small_Qnty = Balance_Id_From_Small_Qnty;
                Unit_Item_Table_Item_No = Balance_Id_From_Item_No;

                mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                //arr.Add(mysql);
                opp.insert_me(mysql);



                To_Store_Unit_Item_Table_Id = -1;


                Balance_Id_To_Larg_Qnty = Balance_Id_From_Larg_Qnty;
                Balance_Id_To_Small_Qnty = Balance_Id_From_Larg_Qnty;
                Balance_Id_To_Item_No = Balance_Id_From_Item_No;

                ////////////////////////////////////////////////////////////
                Qua_Befor = Balance_Now_Larg_Qnty;
                Part_qua_Befor = Balance_Now_Small_Qnty;
                Item_No_Befor = Balance_Now_Item_No;

                Qua_After = Unit_Item_Table_Larg_Qnty;
                Part_qua_After = Unit_Item_Table__Small_Qnty;
                Item_No_After = Balance_Id_To_Item_No;

                //////////////////////////////////////////////////////////


                //Related_Detailes_Purshing_Id = InsertToDetailes_Purshing(ref opp, Order_Estlam_Sarf_Flage, Order_Type, OrderTable_id, Unit_Item_Table_Id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Id_From_Larg_Qnty, Balance_Id_From_Small_Qnty, Balance_Id_From_Item_No, Balance_Id_To_Larg_Qnty, Balance_Id_To_Small_Qnty, Balance_Id_To_Item_No, To_Store_Unit_Item_Table_Id, Notes_From, Notes_To, Notes, Machine_Id, Qua_Befor, Part_qua_Befor, Item_No_Befor, Qua_After, Part_qua_After, Item_No_After, S_unit_Value, 0, Notes_Item, FromStoreId, ToStoreId);
                Related_Detailes_Purshing_Id = InsertToDetailes_Purshing(ref opp, dp, S_unit_Value, order_Purshing);



            }
            #endregion

            #region Update CarsWorkOrders
            SetEditOrderForCarsWorkOrders(dp, order_Purshing);


            #endregion

            #region Transform          تحويل
            if (order_Purshing.Order_Type == (int)StoreOrderType.SarfTahweel_1 | order_Purshing.Order_Type == (int)StoreOrderType.MortagaSarfTahweel_6)             //التحويل
            {
                ////زيادة الرصيد وتفعيل الاذن اوتوماتيك اى انشاء الاذن كأنة اذن صرف من واذن استلام الى
                if (Common.Make_Store_Order_Sarf_Estlam_Ta7weel_Automatic_Between_Stores == 1)/////
                {//////////////////////////////////////////////////////////

                    if (Order_Estlam_Sarf_Flage == 1)
                    {
                        ////تعديل بيانات المخزن المنصرف منة بنقص الرصيد

                        clc.ConVersionQuntity_Subtract_ToItem(false, Larg_Qnty, Small_Qnty, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, ref Balance_Id_From_Larg_Qnty, ref Balance_Id_From_Small_Qnty, S_unit_Value);

                        Unit_Item_Table_Larg_Qnty = Balance_Id_From_Larg_Qnty;
                        Unit_Item_Table__Small_Qnty = Balance_Id_From_Small_Qnty;
                        Unit_Item_Table_Item_No = Balance_Id_From_Item_No;

                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                        //arr.Add(mysql);
                        opp.insert_me(mysql);

                    }


                    ////////////////////////////////////////////////////////////
                    Qua_Befor = Balance_Now_Larg_Qnty;
                    Part_qua_Befor = Balance_Now_Small_Qnty;
                    Item_No_Befor = Balance_Now_Item_No;

                    Qua_After = Unit_Item_Table_Larg_Qnty;
                    Part_qua_After = Unit_Item_Table__Small_Qnty;
                    Item_No_After = Balance_Id_To_Item_No;



                    //////////////////// البحث عن رصيد المخزن المحول اليه بزيادة الرصيد
                    ///
                    dp_C = new Detailes_Purshing();

                    decimal To_Store_LargBalance = 0;
                    decimal To_Store_Small_Balance = 0;
                    decimal To_Store_Item_No = 0;

                    if (!Check_ItemFoundOnStore(ref opp, dp, ref To_Store_Unit_Item_Table_Id, ref To_Store_LargBalance, ref To_Store_Small_Balance, ref To_Store_Item_No))
                    {
                        Excp = 1;
                        return;
                    }

                    ////تعديل بيانات المخزن المنصرف إلية بزيادة الرصيد
                    clc.ConVersionQuntity_ADD_ToItem(Larg_Qnty, Small_Qnty, To_Store_LargBalance, To_Store_Small_Balance, ref Balance_Id_To_Larg_Qnty, ref Balance_Id_To_Small_Qnty, S_unit_Value);

                    decimal To_Unit_Item_Table_Larg_Qnty = Balance_Id_To_Larg_Qnty;
                    decimal To_Unit_Item_Table__Small_Qnty = Balance_Id_To_Small_Qnty;
                    decimal To_Unit_Item_Table_Item_No = To_Store_Item_No + ItemNO;
                    decimal To_Balance_Id_To_Item_No = Unit_Item_Table_Item_No;
                    if (Order_Estlam_Sarf_Flage == 2)
                    {
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + "  WHERE (id = " + To_Store_Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                    }


                    dp_C.Unit_item_Table_Id = To_Store_Unit_Item_Table_Id;
                    dp_C.To_Store_Unit_Item_Table_Id = dp.Unit_item_Table_Id;

                    dp_C.Balance_Id_To_Larg_Qnty = To_Store_LargBalance + Larg_Qnty;

                    dp_C.Balance_Id_To_Small_Qnty = To_Store_Small_Balance + Small_Qnty;
                    dp_C.Balance_Id_To_Item_No = To_Store_Item_No + ItemNO;



                    dp_C.Qua_Befor = To_Store_LargBalance;
                    dp_C.Part_qua_Befor = To_Store_Small_Balance;
                    dp_C.Item_No_Befor = To_Store_Item_No;

                    dp_C.Qua_After = To_Unit_Item_Table_Larg_Qnty;
                    dp_C.Part_qua_After = To_Unit_Item_Table__Small_Qnty;
                    dp_C.Item_No_After = To_Balance_Id_To_Item_No;


                    dp_C.Balance_Id_From_Larg_Qnty = Balance_Now_Larg_Qnty - Larg_Qnty;

                    dp_C.Balance_Id_From_Small_Qnty = Balance_Now_Small_Qnty - Small_Qnty;
                    dp_C.Balance_Id_From_Item_No = Balance_Now_Item_No - ItemNO;

                    dp_C.CarsWorkOrdersId = dp.CarsWorkOrdersId;
                    dp_C.CarsWorkOrdersRequestedDetailsId = dp.CarsWorkOrdersRequestedDetailsId;
                    dp_C.CarsWorkOrdersDeliverDetailsId = dp.CarsWorkOrdersDeliverDetailsId;


                    ////////Create Details_Purchase Converted

                    CreateDetails_PurchaseConverted(ref dp_C, dp, uitm);
                    //long Sub_Related_Detailes_Purshing_Id = InsertToDetailes_Purshing(ref opp, Order_Estlam_Sarf_Flage_1, Order_Type, OrderTable_id_C, To_Store_Unit_Item_Table_Id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Id_To_Larg_Qnty, Balance_Id_To_Small_Qnty, Balance_Id_To_Item_No, Balance_Id_From_Larg_Qnty, Balance_Id_From_Small_Qnty, Balance_Id_From_Item_No, Unit_Item_Table_Id, Notes_From, Notes_To, Notes, Machine_Id, To_Qua_Befor, To_Part_qua_Befor, To_Item_No_Befor, To_Qua_After, To_Part_qua_After, To_Item_No_After, S_unit_Value, Related_Detailes_Purshing_Id, Notes_Item, FromStoreId, ToStoreId);
                    long Sub_Related_Detailes_Purshing_Id = InsertToDetailes_Purshing(ref opp, dp_C, S_unit_Value, order_Purshing);

                    mysql = " UPDATE Detailes_Purshing  SET  Related_Detailes_Purshing_Id=" + Related_Detailes_Purshing_Id + "    WHERE (id = " + Sub_Related_Detailes_Purshing_Id + ") ";
                    //arr.Add(mysql);
                    opp.insert_me(mysql);

                    mysql = " UPDATE Detailes_Purshing  SET  Related_Detailes_Purshing_Id=" + Sub_Related_Detailes_Purshing_Id + " ,To_Store_Unit_Item_Table_Id=" + To_Store_Unit_Item_Table_Id + "  WHERE (id = " + Related_Detailes_Purshing_Id + ") ";
                    //arr.Add(mysql);
                    opp.insert_me(mysql);

                }
                else
                {
                    To_Store_Unit_Item_Table_Id = 0;
                    Balance_Id_To_Larg_Qnty = 0;
                    Balance_Id_To_Small_Qnty = 0;
                    Balance_Id_To_Item_No = 0;

                }




            }
            #endregion

            #region Employee          صرف لموظف
            if (order_Purshing.Order_Type == (int)StoreOrderType.SarfToEmployee_20 | order_Purshing.Order_Type == (int)StoreOrderType.EstlamFromEmployee_21)             // 
            {
                long Unit_itemEmployeeBalanceId = 0;
                long Item_Id = dp.Item_Id;
                long EmployeeId = 0;


                if (Order_Estlam_Sarf_Flage == 1)
                {
                    EmployeeId = dp.Id_From;

                    if (Check_ItemFoundOnEmployee(ref opp, ref Unit_itemEmployeeBalanceId, Item_Id, EmployeeId))
                    {

                        mysql = " UPDATE Unit_itemEmployeeBalance  SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + ItemNO + " WHERE (id = " + Unit_itemEmployeeBalanceId + ") ";
                        //arr.Add(mysql);
                        opp.insert_me(mysql);
                    }
                }
                if (Order_Estlam_Sarf_Flage == 2)
                {
                    EmployeeId = dp.Id_To;
                    if (Check_ItemFoundOnEmployee(ref opp, ref Unit_itemEmployeeBalanceId, Item_Id, EmployeeId))
                    {
                        mysql = " UPDATE Unit_itemEmployeeBalance  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + "  WHERE (id = " + Unit_itemEmployeeBalanceId + ") ";
                        opp.insert_me(mysql);
                    }
                }


            }
            #endregion



        }

        void UpdateQnty(operations opp, int Order_Type, int TransactionType, long Unit_Item_Table_Id, decimal Larg_Qnty, decimal Small_Qnty, decimal ItemNO)
        {
            string mysql = "";
            mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
            //arr.Add(mysql);
            opp.insert_me(mysql);
        }
        long InsertToDetailes_Purshing(ref operations opp, Detailes_Purshing dp, decimal S_Unit_Value, Order_Purshing op)
        {
            //, int Order_Type, long OrderTable_id, long Unit_Item_Table_Id, decimal Larg_Qnty, decimal Small_Qnty, decimal ItemNO, decimal Balance_Id_From_Larg_Qnty, decimal Balance_Id_From_Small_Qnty, decimal Balance_Id_From_Item_No, decimal Balance_Id_To_Larg_Qnty, decimal Balance_Id_To_Small_Qnty, decimal Balance_Id_To_Item_No, decimal To_Store_Unit_Item_Table_Id, string Notes_From, string Notes_To, string Notes, long Machine_Id, decimal Qua_Befor, decimal Part_qua_Befor, decimal Item_No_Befor, decimal Qua_After, decimal Part_qua_After, decimal Item_No_After, decimal S_Unit_Value, long Related_Detailes_Purshing_Id, string Notes_Item, long FromStoreId, long ToStoreId

            decimal qua_Import = 0;
            decimal Part_Qua_Import = 0;
            decimal Item_No_Import = 0;
            decimal Quantity_Import = 0;

            decimal qua_Export = 0;
            decimal Part_Qua_Export = 0;
            decimal Item_No_Export = 0;
            decimal Quantity_Export = 0;


            decimal Quantity_Balance = 0;

            dp.qua = Common.Get_PositiveNumberDecimal(dp.qua);
            dp.Part_qua = Common.Get_PositiveNumberDecimal(dp.Part_qua);
            dp.Item_No = Common.Get_PositiveNumberDecimal(dp.Item_No);
            dp.Balance_Id_From_Larg_Qnty = Common.Get_PositiveNumberDecimal(dp.Balance_Id_From_Larg_Qnty);
            dp.Balance_Id_From_Small_Qnty = Common.Get_PositiveNumberDecimal(dp.Balance_Id_From_Small_Qnty);
            dp.Balance_Id_From_Item_No = Common.Get_PositiveNumberDecimal(dp.Balance_Id_From_Item_No);
            dp.Balance_Id_To_Larg_Qnty = Common.Get_PositiveNumberDecimal(dp.Balance_Id_To_Larg_Qnty);
            dp.Balance_Id_To_Small_Qnty = Common.Get_PositiveNumberDecimal(dp.Balance_Id_To_Small_Qnty);
            dp.Balance_Id_To_Item_No = Common.Get_PositiveNumberDecimal(dp.Balance_Id_To_Item_No);


            dp.Related_Detailes_Purshing_Id = 0;
            if (dp.Transaction_Type == 1)
            {
                dp.Import_Qua = dp.qua;
                dp.Import_Part_Qua = dp.Part_qua;
                dp.Import_Item_No = dp.Item_No;
                dp.Export_Qua = 0;
                dp.Export_Part_Qua = 0;
                dp.Export_Item_No = 0;
            }
            else
            {
                dp.Import_Qua = 0;
                dp.Import_Part_Qua = 0;
                dp.Import_Item_No = 0;
                dp.Export_Qua = dp.qua;
                dp.Export_Part_Qua = dp.Part_qua;
                dp.Export_Item_No = dp.Item_No;
            }
            dp.ProcessNameId = order_Purshing.Order_Type;
            dp.ProcessName = order_Purshing.OrderTitle;
            dp.ProcessNameNumber = order_Purshing.Order_Purshing_No.ToString();

            string mysql = "";



            //mysql = " INSERT INTO Detailes_Purshing (OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Balance_Id_From_Larg_Qnty, Balance_Id_From_Small_Qnty,Balance_Id_From_Item_No,Balance_Id_To_Larg_Qnty,Balance_Id_To_Small_Qnty,Balance_Id_To_Item_No,To_Store_Unit_Item_Table_Id , Notes_From,Notes_To,Notes,Machine_Id, Qua_Befor,Part_qua_Befor,Item_No_Befor,Qua_After,Part_qua_After,Item_No_After,Related_Detailes_Purshing_Id,Transaction_Type,Notes_Item, FromStoreId,ToStoreId) ";
            //mysql = mysql + "  VALUES (" + OrderTable_id + "," + Unit_Item_Table_Id + "," + Larg_Qnty + "," + Small_Qnty + "," + ItemNO + "," + Balance_Id_From_Larg_Qnty + "," + Balance_Id_From_Small_Qnty + "," + Balance_Id_From_Item_No + "," + Balance_Id_To_Larg_Qnty + "," + Balance_Id_To_Small_Qnty + "," + Balance_Id_To_Item_No + "," + To_Store_Unit_Item_Table_Id + " , N'" + Notes_From + "',N'" + Notes_To + "',N'" + Notes + "'," + Machine_Id + " ," + Qua_Befor + "," + Part_qua_Befor + "," + Item_No_Befor + "," + Qua_After + "," + Part_qua_After + "," + Item_No_After + "," + Related_Detailes_Purshing_Id + "," + Order_Estlam_Sarf_Flage + ",N'" + Notes_Item + "'," + FromStoreId + "," + ToStoreId + ")";

            //long Detailes_Purshing_Id = opp.Insert_me_Return_Last_Identify(mysql);
            dp.P_priceAvgCost = dp.P_priceAvgCost ?? 0;
            dp.PriceUnit = Common.Get_PositiveNumberDecimal(dp.PriceUnit);
            dp.P_priceAvgCost = Common.Get_PositiveNumberDecimal(dp.P_priceAvgCost);
            Detailes_PurshingCLS dpCLS = new Detailes_PurshingCLS();
            dpCLS.AddDetailes_Purshing(ref opp, dp, op);



            dp.S_Price = dp.S_Price ?? 0;
            dp.Qnty_value = dp.Qnty_value ?? 0;
            dp.Larg_Unit_Price = dp.Larg_Unit_Price ?? 0;
            dp.P_priceAvgCost = dp.P_priceAvgCost ?? 0;
            dp.PriceUnit = dp.PriceUnit ?? 0;
            dp.P_Price = dp.P_Price ?? 0;
            dp.S_priceAvgCost = dp.S_priceAvgCost ?? 0;
            dp.Total = dp.Total ?? 0;
            dp.Item_Net_Total_Price = dp.Item_Net_Total_Price ?? 0;




            mysql = $"Update Detailes_Purshing  Set Qnty_value={dp.Qnty_value},Larg_Unit_Price={dp.Larg_Unit_Price}, P_priceAvgCost={dp.P_priceAvgCost}, PriceUnit={dp.PriceUnit}, S_Price={dp.S_Price}, P_Price={dp.P_Price},      S_priceAvgCost={dp.S_priceAvgCost}, Item_Net_Total_Price={dp.Item_Net_Total_Price}, Total={dp.Total}    Where Id={dp.Id} ";

            opp.insert_me(mysql);


            SetDetailes_Purshing_Import_Export(ref opp, dp, S_Unit_Value, op);

            return dp.Id;
        }
        void SetDetailes_Purshing_Import_Export(ref operations opp, Detailes_Purshing dp, decimal S_Unit_Value, Order_Purshing op)
        {
            Detailes_Purshing_Import_Export die = new Detailes_Purshing_Import_Export();

            //decimal Balance_Larg_Qnty = 0;
            //decimal Balance_Small_Qnty = 0;
            //decimal Balance_Item_No = 0;

            decimal Quantity = 0;
            decimal Quantity_Balance_From = 0;

            decimal Quantity_Balance_To = 0;
            Quantity = Common.Get_Item_Quantity(dp.qua.Value, dp.Part_qua.Value, S_Unit_Value);
            Quantity_Balance_From = Common.Get_Item_Quantity(dp.Balance_Id_From_Larg_Qnty.Value, dp.Balance_Id_From_Small_Qnty.Value, S_Unit_Value);
            Quantity_Balance_To = Common.Get_Item_Quantity(dp.Balance_Id_To_Larg_Qnty.Value, dp.Balance_Id_To_Small_Qnty.Value, S_Unit_Value);

            //die.Notes_From_To = "";
            if (dp.Transaction_Type == 1)
            {
                die.qua_Import = dp.qua.Value;
                die.Part_Qua_Import = dp.Part_qua.Value;
                die.Item_No_Import = dp.Item_No.Value;
                die.Quantity_Import = Common.Get_Item_Quantity(dp.qua.Value, dp.Part_qua.Value, S_Unit_Value);

                die.qua_Export = 0;
                die.Part_Qua_Export = 0;
                die.Item_No_Export = 0;
                die.Quantity_Export = 0;
                die.Quantity_Balance = Quantity_Balance_From;
                //die.Notes_From_To = dp.Notes_From;

                die.PriceUnitImport = dp.PriceUnit;
                die.Item_Net_Total_PriceImport = dp.Item_Net_Total_Price;

                die.PriceUnitExport = 0;
                die.Item_Net_Total_PriceExport = 0;

            }
            else
            {

                die.qua_Import = 0;
                die.Part_Qua_Import = 0;
                die.Item_No_Import = 0;
                die.Quantity_Import = 0;

                die.qua_Export = dp.qua.Value;
                die.Part_Qua_Export = dp.Part_qua.Value;// Small_Qnty;
                die.Item_No_Export = dp.Item_No.Value;// ItemNO;
                die.Quantity_Export = Common.Get_Item_Quantity(dp.qua.Value, dp.Part_qua.Value, S_Unit_Value);
                die.Quantity_Balance = Quantity_Balance_To;
                //die.Notes_From_To = dp.Notes_To;

                die.PriceUnitImport = 0;
                die.Item_Net_Total_PriceImport = 0;

                die.PriceUnitExport = dp.PriceUnit;
                die.Item_Net_Total_PriceExport = dp.Item_Net_Total_Price;

            }




            //die.PriceUnitImport =dp.PriceUnit;
            //die.Item_Net_Total_PriceImport = dp.Item_Net_Total_Price;

            //die.PriceUnitExport = dp.PriceUnit;
            //die.Item_Net_Total_PriceExport = dp.Item_Net_Total_Price;


            die.Item_Net_Total_Price = dp.Item_Net_Total_Price;
            die.PriceUnit = dp.PriceUnit;
            die.Qnty_value = dp.Qnty_value;


            /////////////////////////////
            ///



            die.P_Price = dp.P_Price;
            die.S_Price = dp.S_Price;
            die.S_priceAvgCost = dp.S_priceAvgCost ?? 0;
            die.P_priceAvgCost = dp.P_priceAvgCost ?? 0;
            ///

            //string NotesTitle = order_Purshing.OrderTitle;


            die.OrderTable_id = order_Purshing.Id;
            die.Detailes_Purshing_Id = dp.Id;
            die.Detail_Invoice_Id = dp.Id;

            die.Unit_item_Table_Id = dp.Unit_item_Table_Id;
            die.qua = dp.qua;
            die.Part_qua = dp.Part_qua;
            die.Item_No = dp.Item_No;
            die.Balance_Larg_Qnty = dp.Balance_Id_From_Larg_Qnty;
            die.Balance_Small_Qnty = dp.Balance_Id_From_Small_Qnty;
            die.Balance_Item_No = dp.Balance_Id_From_Item_No;

            die.Invoice_No = op.Order_Purshing_No;

            /////////////////hh
            ///

            die.Invoice_Type_Str = order_Purshing.OrderTitle;
            //die.ProcessName = order_Purshing.OrderTitle;

            die.ProcessNameId = dp.ProcessNameId;
            die.ProcessName = dp.ProcessName;
            die.ProcessNameNumber = dp.ProcessNameNumber;

            die.StoreId = dp.StoreId;
            die.StoreId_Name = dp.StoreId_Name;
            //die.Notes_From_To = dp.StoreId_Name;
            die.Notes_From_To = " من " + dp.Id_FromStr + " الى " + dp.Id_ToStr;

            die.Notes_To = dp.Notes_To;
            die.Notes_From = dp.Notes_From;
            die.Notes = order_Purshing.NotesAll;// NotesTitle;
            die.Notes_Item = dp.Notes_Item;//  Notes_Item;

            die.Type_From_Name = dp.Type_From_Name;
            die.Type_To_Name = dp.Type_To_Name;

            die.Invoice_Id = order_Purshing.Id;
            die.Invoice_Type_Id = (long)order_Purshing.Order_Type;



            die.Machine_Id = dp.Machine_Id;
            die.Transaction_Type = dp.Transaction_Type;
            //die.qua_Import =  ;
            //die.Part_Qua_Import =  ;
            //die.Item_No_Import =  ;
            //die.Quantity_Import =  ;
            //die.qua_Export =  ;
            //die.Part_Qua_Export =  ;
            //die.Item_No_Export =  ;
            //die.Quantity_Export =  ;
            die.Quantity = Quantity;
            //die.Quantity_Balance =  ;
            die.IS_To_Converted = 0;
            die.Process_Date = order_Purshing.Order_date;


            //die.Notes_Item = Order_Purshing.OrderTitle;
            die.User_Id = Common.User_Id;
            die.Branch_Id = Common.Branch_Id;
            die.ProcessNo = 0;
            die.OperationDate = DateTime.Now;

            die.CarsWorkOrdersId = order_Purshing.CarsWorkOrdersId;
            die.WorkOrderNumber = order_Purshing.WorkOrderNumber;
            die.CarNumber = order_Purshing.CarNumber;
            die.CarsId = order_Purshing.CarsId;

            die.CarsWorkOrdersDetailsId = 0;




            die.Id_FromStr = dp.Id_FromStr;
            die.Id_ToStr = dp.Id_ToStr;

            die.Type_From = dp.type_From;

            die.Type_To = dp.Type_To;
            die.Id_From = dp.Id_From;
            die.Id_To = dp.Id_To;


            die.EmployeeId = dp.EmployeeId;
            die.EmployeeName = dp.EmployeeName;




            Detailes_Purshing_Import_ExportCLS d = new Detailes_Purshing_Import_ExportCLS();
            d.Add(ref opp, die);


            string mysql = $"Update Detailes_Purshing_Import_Export Set   Qnty_value ={die.Qnty_value} ,P_priceAvgCost={die.P_priceAvgCost}, PriceUnit={die.PriceUnit}, S_Price={die.S_Price}, P_Price={die.P_Price},      S_priceAvgCost={die.S_priceAvgCost}, PriceUnitImport={die.PriceUnitImport}, PriceUnitExport={die.PriceUnitExport}, Item_Net_Total_PriceImport={die.Item_Net_Total_PriceImport}, Item_Net_Total_PriceExport ={die.Item_Net_Total_PriceExport}    Where Id={die.Id} ";
            opp.insert_me(mysql);

            //opp.db.Detailes_Purshing_Import_Exports.Add(die);
            //opp.db.SaveChanges();

            //mysql = "INSERT INTO Detailes_Purshing_Import_Export(OrderTable_id,Detailes_Purshing_Id, Unit_item_Table_Id, qua, Part_qua, Item_No, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Notes_From_To,Notes_From,Notes_To, Notes, Machine_Id,Transaction_Type, qua_Import ,Part_Qua_Import ,Item_No_Import,Quantity_Import ,qua_Export ,Part_Qua_Export,Item_No_Export,Quantity_Export ,Quantity ,Quantity_Balance,IS_To_Converted,Process_Date,Notes_Item, Invoice_Id,Invoice_Type_Id,Invoice_Type_Str,User_Id,OperationDate) ";
            //mysql = mysql + " VALUES (" + OrderTable_id + "," + Detailes_Purshing_Id + "," + Unit_Item_Table_Id + "," + Larg_Qnty + "," + Small_Qnty + "," + ItemNO + "," + Balance_Larg_Qnty + "," + Balance_Small_Qnty + "," + Balance_Item_No + ",'" + Notes_From_To + "  -  " + Notes_To + "','" + Notes_From + "','" + Notes_To + "','" + NotesTitle + "'," + Machine_Id + "," + Order_Estlam_Sarf_Flage + "," + qua_Import + "," + Part_Qua_Import + "," + Item_No_Import + "," + Quantity_Import + "," + qua_Export + "," + Part_Qua_Export + "," + Item_No_Export + "," + Quantity_Export + "," + Quantity + "," + Quantity_Balance + "," + IS_To_Converted + ",'" + operations.dateFormat(Order_Date) + "',N'" + Notes_Item + "', " + Invoice_Id + "," + Invoice_Type_Id + ",N'" + Invoice_Type_Str + "', " + Common.User_Id + "," + operations.Get_DateTimeToInsert(DateTime.Now) + ") ";
            //opp.insert_me(mysql);



        }

        void SetOrderForCarsWorkOrders(Detailes_Purshing dp)
        {
            /*
            if (order_Purshing.CarsWorkOrdersId > 0)
            {
                long CarsWorkOrdersDeliverDetailId = 0;
                long CarsWorkOrdersRequestedDetailsId = 0;
                long CarsWorkOrdersItemProcessedDetailId = 0;

                int Transaction_TypeForWarsha = GetTransaction_TypeForWarsha(dp);
                MappingCLS mp = new MappingCLS();
                CarsWorkOrdersDeliverItemBalanceCLS itmDeliverBalanceCLS = new CarsWorkOrdersDeliverItemBalanceCLS();


                ////////////////  Save the item sarf and deliver to workOrder warsha
                if (Transaction_TypeForWarsha == 1)
                {
                    ////Deliver Items
                    CarsWorkOrdersDeliverDetail itmDeliverDetail = new CarsWorkOrdersDeliverDetail();
                    itmDeliverDetail = mp.MappingGetCarsWorkOrdersDeliverDetail(ref itmDeliverDetail, dp);
                    //itmDeliverDetail.CarsWorkOrdersRequestedDetailsId = CarsWorkOrdersRequestedDetailsId;
                    CarsWorkOrdersDeliverDetailCLS deCls = new CarsWorkOrdersDeliverDetailCLS();
                    deCls.Add(ref opp, itmDeliverDetail);
                    //opp.db.CarsWorkOrdersDeliverDetails.Add(cwD);
                    //opp.db.SaveChanges();

                }

                //////////////////////////////
                ///Set    CarsWorkOrdersRequestedDetail
                CarsWorkOrdersRequestedDetailsId = itmDeliverBalanceCLS.SetCarsWorkOrdersRequestedDetail(ref opp, dp, Transaction_TypeForWarsha);


                ////Process Items
                CarsWorkOrdersItemProcessedDetailId = itmDeliverBalanceCLS.SetCarsWorkOrdersItemProcessedDetail(ref opp, dp, Transaction_TypeForWarsha);


                ////Set CarsWorkOrdersDeliverItemBalance
                ////

                itmDeliverBalanceCLS.Set_CarsWorkOrdersDeliverItemBalance(ref opp, dp, Transaction_TypeForWarsha);


                //////////////Update detail purchase with CarsWorkOrdersDeliverDetails

                dp.CarsWorkOrdersDeliverDetailsId = CarsWorkOrdersDeliverDetailId;
                dp.CarsWorkOrdersRequestedDetailsId = CarsWorkOrdersRequestedDetailsId;
                dp.CarsWorkOrdersItemProcessedDetailsId = CarsWorkOrdersItemProcessedDetailId;
                opp.db.Entry(dp).State = EntityState.Modified;
                opp.db.SaveChanges();
            }
        */
        }

        void SetEditOrderForCarsWorkOrders(Detailes_Purshing dp, Order_Purshing order_Purshing)
        {
            if (order_Purshing.CarsWorkOrdersId > 0)
            {
                long CarsWorkOrdersDeliverDetailId = 0;
                long CarsWorkOrdersRequestedDetailsId = 0;
                long CarsWorkOrdersItemProcessedDetailId = 0;

                int Transaction_TypeForWarsha = GetTransaction_TypeForWarsha(dp);
                MappingCLS mp = new MappingCLS();
                CarsWorkOrdersDeliverItemBalanceCLS itmDeliverBalanceCLS = new CarsWorkOrdersDeliverItemBalanceCLS();


                ////////////////  Save the item sarf and deliver to workOrder warsha
                if (Transaction_TypeForWarsha == 1)
                {
                    ////Deliver Items
                    //itmDeliverDetail.CarsWorkOrdersRequestedDetailsId = CarsWorkOrdersRequestedDetailsId;
                    CarsWorkOrdersDeliverDetailCLS deCls = new CarsWorkOrdersDeliverDetailCLS();
                    CarsWorkOrdersDeliverDetail itmDeliverDetail = deCls.Set_CarsWorkOrdersDeliverDetail(ref opp, dp, order_Purshing, Transaction_TypeForWarsha);

                    //opp.db.CarsWorkOrdersDeliverDetails.Add(cwD);
                    //opp.db.SaveChanges();

                }

                //////////////////////////////
                ///Set    CarsWorkOrdersRequestedDetail
                CarsWorkOrdersRequestedDetailsId = itmDeliverBalanceCLS.SetCarsWorkOrdersRequestedDetail(ref opp, dp, order_Purshing, Transaction_TypeForWarsha);


                ////Process Items
                CarsWorkOrdersItemProcessedDetailId = itmDeliverBalanceCLS.SetCarsWorkOrdersItemProcessedDetail(ref opp, dp, order_Purshing, Transaction_TypeForWarsha);


                ////Set CarsWorkOrdersDeliverItemBalance
                ////

                itmDeliverBalanceCLS.Set_CarsWorkOrdersDeliverItemBalance(ref opp, dp, Transaction_TypeForWarsha);


                //////////////Update detail purchase with CarsWorkOrdersDeliverDetails

                dp.CarsWorkOrdersDeliverDetailsId = CarsWorkOrdersDeliverDetailId;
                dp.CarsWorkOrdersRequestedDetailsId = CarsWorkOrdersRequestedDetailsId;
                dp.CarsWorkOrdersItemProcessedDetailsId = CarsWorkOrdersItemProcessedDetailId;
                opp.db.Entry(dp).State = EntityState.Modified;
                opp.db.SaveChanges();
            }
        }
        void DeleteOrderForCarsWorkOrders(Detailes_Purshing dp)
        {
            if (order_Purshing.CarsWorkOrdersId > 0)
            {


                int Transaction_TypeForWarsha = GetTransaction_TypeForWarsha(dp);

                CarsWorkOrdersDeliverItemBalanceCLS itmDeliverBalanceCLS = new CarsWorkOrdersDeliverItemBalanceCLS();


                ////////////////  Save the item sarf and deliver to workOrder warsha
                if (Transaction_TypeForWarsha == 1)
                {
                    ////Deliver Items
                    //itmDeliverDetail.CarsWorkOrdersRequestedDetailsId = CarsWorkOrdersRequestedDetailsId;
                    CarsWorkOrdersDeliverDetailCLS deCls = new CarsWorkOrdersDeliverDetailCLS();
                    deCls.Delete_CarsWorkOrdersDeliverDetail(ref opp, dp, Transaction_TypeForWarsha);

                    //opp.db.CarsWorkOrdersDeliverDetails.Add(cwD);
                    //opp.db.SaveChanges();

                }

                //////////////////////////////
                ///Set    CarsWorkOrdersRequestedDetail
                itmDeliverBalanceCLS.DeleteCarsWorkOrdersRequestedDetail(ref opp, dp, Transaction_TypeForWarsha);


                ////Process Items
                itmDeliverBalanceCLS.DeleteCarsWorkOrdersItemProcessedDetail(ref opp, dp, Transaction_TypeForWarsha);


                ////Set CarsWorkOrdersDeliverItemBalance
                ////

                itmDeliverBalanceCLS.Delete_CarsWorkOrdersDeliverItemBalance(ref opp, dp, Transaction_TypeForWarsha);

            }
        }

        void ChangeOrderForCarsWorkOrders(Detailes_Purshing dp)
        {
            if (order_Purshing.CarsWorkOrdersId > 0)
            {


                int Transaction_TypeForWarsha = GetTransaction_TypeForWarsha(dp);

                CarsWorkOrdersDeliverItemBalanceCLS itmDeliverBalanceCLS = new CarsWorkOrdersDeliverItemBalanceCLS();


                ////////////////  Save the item sarf and deliver to workOrder warsha
                if (Transaction_TypeForWarsha == 1)
                {
                    ////Deliver Items
                    //itmDeliverDetail.CarsWorkOrdersRequestedDetailsId = CarsWorkOrdersRequestedDetailsId;
                    CarsWorkOrdersDeliverDetailCLS deCls = new CarsWorkOrdersDeliverDetailCLS();
                    deCls.Change_CarsWorkOrdersDeliverDetail(ref opp, dp, Transaction_TypeForWarsha);

                    //opp.db.CarsWorkOrdersDeliverDetails.Add(cwD);
                    //opp.db.SaveChanges();

                }

                //////////////////////////////
                ///Set    CarsWorkOrdersRequestedDetail
                itmDeliverBalanceCLS.ChangeCarsWorkOrdersRequestedDetail(ref opp, dp, Transaction_TypeForWarsha);


                ////Process Items
                itmDeliverBalanceCLS.ChangeCarsWorkOrdersItemProcessedDetail(ref opp, dp, Transaction_TypeForWarsha);


                ////Set CarsWorkOrdersDeliverItemBalance
                ////

                itmDeliverBalanceCLS.Change_CarsWorkOrdersDeliverItemBalance(ref opp, dp, Transaction_TypeForWarsha);

                //DataDictionaryLogerProcessCLS ddlpCLS = new DataDictionaryLogerProcessCLS(ref opp);
                //ddlpCLS.AddNew(order_Purshing.Order_Type.Value, 2, order_Purshing.Id, order_Purshing.Order_Purshing_No, dp.CarsWorkOrdersId??0,0,0,  dp.Item_Name+" "+" تعديل امر الشغل ");

            }
        }


        int GetTransaction_TypeForWarsha(Detailes_Purshing dp)
        {

            int Transaction_TypeForWarsha = 1;
            if (dp.type_From == 18)   //صرف للورشة
            {
                Transaction_TypeForWarsha = 1;
            }
            if (dp.type_From == 19)   //إستلام من الورشة
            {
                Transaction_TypeForWarsha = 2;
            }

            if (dp.Transaction_Type == 1 & dp.type_From == 22)
            {
                Transaction_TypeForWarsha = 1;
            }
            if (dp.Transaction_Type == 2 & dp.type_From == 22)
            {
                Transaction_TypeForWarsha = 2;
            }
            return Transaction_TypeForWarsha;
        }
        void CreateDetails_PurchaseConverted(ref Detailes_Purshing dp_C, Detailes_Purshing dp_Main, Unit_item um)
        {

            //Order_Purshing od_C;
            //dp_C.Id = dp_Main.Id;
            dp_C.OrderTable_id = od_C.Id;



            dp_C.Item_Name = dp_Main.Item_Name;
            dp_C.Item_Id = dp_Main.Item_Id;
            dp_C.Item_Category_Id = dp_Main.Item_Category_Id;
            dp_C.Item_Category_Name = dp_Main.Item_Category_Name;


            dp_C.qua = dp_Main.qua;
            dp_C.Part_qua = dp_Main.Part_qua;
            dp_C.Item_No = dp_Main.Item_No;
            dp_C.Qnty = dp_Main.Qnty;

            dp_C.Notes = dp_Main.Notes;
            dp_C.Machine_Id = dp_Main.Machine_Id;


            dp_C.QntyStr = dp_Main.QntyStr;
            dp_C.Item_NoStr = dp_Main.Item_NoStr;



            dp_C.Notes_Item = dp_Main.Notes_Item;
            dp_C.OperationDate = dp_Main.OperationDate;
            dp_C.User_Id = dp_Main.User_Id;
            dp_C.Branch_Id = dp_Main.Branch_Id;
            dp_C.Insert_ServerDateTime = dp_Main.Insert_ServerDateTime;
            dp_C.Update_ServerDateTime = dp_Main.Update_ServerDateTime;
            dp_C.EditOperationDate = dp_Main.EditOperationDate;
            dp_C.Edit_User_Id = dp_Main.Edit_User_Id;
            dp_C.ProcessNo = dp_Main.ProcessNo;
            dp_C.PriceUnit = dp_Main.PriceUnit;
            dp_C.S_Price = dp_Main.S_Price;
            dp_C.S_priceAvgCost = dp_Main.S_priceAvgCost ?? 0;
            dp_C.P_Price = dp_Main.P_Price;
            dp_C.P_priceAvgCost = dp_Main.P_priceAvgCost ?? 0;
            dp_C.Qnty_value = dp_Main.Qnty_value;
            dp_C.Item_Total_WithoutTax = dp_Main.Item_Total_WithoutTax;
            dp_C.Item_ATS05_Value = dp_Main.Item_ATS05_Value;
            dp_C.Item_TaxSales10_Value = dp_Main.Item_TaxSales10_Value;
            dp_C.Total = dp_Main.Total;
            dp_C.Net_Total_Item_Commission = dp_Main.Net_Total_Item_Commission;
            dp_C.Item_Net_Total_Price = dp_Main.Item_Net_Total_Price;


            dp_C.Item_ATS05 = dp_Main.Item_ATS05;
            dp_C.Item_TaxSales10 = dp_Main.Item_TaxSales10;
            dp_C.Item_CommissionValue = dp_Main.Item_CommissionValue;
            dp_C.Unit_CommissionValue = dp_Main.Unit_CommissionValue;
            dp_C.Item_CommissionRate = dp_Main.Item_CommissionRate;
            dp_C.ItemCommissionTotal = dp_Main.ItemCommissionTotal;
            dp_C.Unit_Used_for_Price = dp_Main.Unit_Used_for_Price;
            dp_C.Larg_Unit_Price = dp_Main.Larg_Unit_Price;
            dp_C.Small_Unit_Price = dp_Main.Small_Unit_Price;


            int Order_Estlam_Sarf_Flage_C = 1;
            if (dp_Main.Transaction_Type == 2)
            {
                Order_Estlam_Sarf_Flage_C = 1;
                dp_C.StoreId = dp_Main.Id_To;
                //dp_C.StoreId_Name = dp_Main.Type_To_Name;
                dp_C.StoreId_Name = dp_Main.Id_ToStr;

            }
            else
            {
                Order_Estlam_Sarf_Flage_C = 2;
                dp_C.StoreId = dp_Main.Id_From;
                //dp_C.StoreId_Name = dp_Main.Type_From_Name;
                dp_C.StoreId_Name = dp_Main.Id_FromStr;
            }
            dp_C.Import_Qua = dp_Main.Export_Qua;
            dp_C.Import_Part_Qua = dp_Main.Export_Part_Qua;
            dp_C.Import_Item_No = dp_Main.Export_Item_No;


            dp_C.Export_Qua = dp_Main.Import_Qua;
            dp_C.Export_Part_Qua = dp_Main.Import_Part_Qua;
            dp_C.Export_Item_No = dp_Main.Import_Item_No;



            dp_C.Transaction_Type = Order_Estlam_Sarf_Flage_C;

            dp_C.Id_From = dp_Main.Id_From;
            dp_C.Id_To = dp_Main.Id_To;

            dp_C.type_From = dp_Main.type_From;
            dp_C.Type_To = dp_Main.Type_To;

            dp_C.Type_To_Name = dp_Main.Type_To_Name;
            dp_C.Type_From_Name = dp_Main.Type_From_Name;
            dp_C.Id_FromStr = dp_Main.Id_FromStr;
            dp_C.Id_ToStr = dp_Main.Id_ToStr;

            //dp_C.Balance_Id_From_Larg_Qnty = dp_Main.Balance_Id_From_Larg_Qnty;
            //dp_C.Balance_Id_From_Small_Qnty = dp_Main.Balance_Id_From_Small_Qnty;
            //dp_C.Balance_Id_From_Item_No = dp_Main.Balance_Id_From_Item_No;
            //dp_C.Balance_Id_To_Larg_Qnty = dp_Main.Balance_Id_To_Larg_Qnty;
            //dp_C.Balance_Id_To_Small_Qnty = dp_Main.Balance_Id_To_Small_Qnty;
            //dp_C.Balance_Id_To_Item_No = dp_Main.Balance_Id_To_Item_No;

            dp_C.Notes_To = dp_Main.Notes_To;
            dp_C.Notes_From = dp_Main.Notes_From;
            dp_C.Related_Detailes_Purshing_Id = dp_Main.Related_Detailes_Purshing_Id;
            //dp_C.Transaction_Type = dp_Main.Transaction_Type;
            //dp_C.Qua_Befor = dp_Main.Qua_Befor;
            //dp_C.Part_qua_Befor = dp_Main.Part_qua_Befor;
            //dp_C.Item_No_Befor = dp_Main.Item_No_Befor;
            //dp_C.Qua_After = dp_Main.Qua_After;
            //dp_C.Part_qua_After = dp_Main.Part_qua_After;
            //dp_C.Item_No_After = dp_Main.Item_No_After;
            dp_C.QntyStr_Befor = dp_Main.QntyStr_Befor;
            dp_C.Item_NoStr_Befor = dp_Main.Item_NoStr_Befor;


            dp_C.QntyStr_After = dp_Main.QntyStr_After;
            dp_C.Item_NoStr_After = dp_Main.Item_NoStr_After;

            dp_C.QntyStr_After = Common.GetQntyString(dp_C.Qua_After.ToString(), um.unit.B_Unit.ToString(), dp_C.Part_qua_After.ToString(), um.unit.S_unit.ToString());
            dp_C.Item_NoStr_After = Common.Get_Item_No(dp_C.Qua_After.Value, dp_C.Part_qua_After.Value, dp_C.Item_No_After.Value, um.Small_unit.Value, um.Item.No3eboa.Value).ToString();

            dp_C.QntyStr_Befor = Common.GetQntyString(dp_C.Qua_Befor.ToString(), um.unit.B_Unit.ToString(), dp_C.Part_qua_Befor.ToString(), um.unit.S_unit.ToString());
            dp_C.Item_NoStr_Befor = Common.Get_Item_No(dp_C.Qua_Befor.Value, dp_C.Part_qua_Befor.Value, dp_C.Item_No_Befor.Value, um.Small_unit.Value, um.Item.No3eboa.Value).ToString();
            dp_C.Item_DeletedFlage = -1;
            dp_C.Item_ProcessedFlage = -1;

            dp_C.WorkOrderNumber = dp_Main.WorkOrderNumber;
            dp_C.CarNumber = dp_Main.CarNumber;
            dp_C.CarsId = dp_Main.CarsId;
            dp_C.CarsWorkOrdersId = dp_Main.CarsWorkOrdersId;
            dp_C.CarsWorkOrdersDeliverDetailsId = dp_Main.CarsWorkOrdersDeliverDetailsId;
            dp_C.CarsWorkOrdersItemProcessedDetailsId = dp_Main.CarsWorkOrdersItemProcessedDetailsId;
            dp_C.CarsWorkOrdersRequestedDetailsId = dp_Main.CarsWorkOrdersRequestedDetailsId;

        }
        public bool Check_ItemFoundOnStore(ref operations opp, Detailes_Purshing dp, long Item_id, ref long To_Store_Unit_Item_Table_Id, ref decimal To_Store_LargBalance, ref decimal To_Store_Small_Balance, ref decimal To_Store_Item_No, long Unit_Item_Table_Id)
        {
            try
            {
                if (order_Purshing.Order_Type == (int)StoreOrderType.SarfTahweel_1)
                {

                }
                else if (order_Purshing.Order_Type == (int)StoreOrderType.MortagaSarfTahweel_6)
                {

                }
                else
                {
                    return true;
                }
                //if (dp.Type_To == 3 | dp.Type_To == 10 | dp.Type_To == 8)  //  هالك - تشغيل - تسوية بالنقص
                //{
                //    To_Store_Unit_Item_Table_Id = 0;
                //    return true;
                //}


                string mysql = "";
                DataSet ds = new DataSet();
                ds = opp.return_dataset("select * from Unit_item WHERE (Item_id = " + Item_id + ") AND  (Store_id = " + dp.Id_To + ")   ");

                if (ds.Tables[0].Rows.Count == 0)
                {
                    ds.Clear();
                    ds = opp.return_dataset("select * from Unit_item WHERE ( id = " + Unit_Item_Table_Id + ")");
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        mysql = " INSERT INTO Unit_item (Item_id, Code, unit_id, Full_qua, D_Qua, Item_No, Reserved_Balance_Larg_Qnty, Reserved_Balance_Small_Qnty, Big_unit, Small_unit, P_price, S_price, Specification, Upon_Request , Store_id, Store_Type, Currency) ";
                        mysql = mysql + " VALUES (" + ds.Tables[0].Rows[0]["Item_id"].ToString() + ",'" + ds.Tables[0].Rows[0]["Code"].ToString() + "'," + ds.Tables[0].Rows[0]["unit_id"].ToString() + ", 0, 0,0, 0, 0," + ds.Tables[0].Rows[0]["Big_unit"].ToString() + "," + ds.Tables[0].Rows[0]["Small_unit"].ToString() + "," + ds.Tables[0].Rows[0]["P_price"].ToString() + "," + ds.Tables[0].Rows[0]["S_price"].ToString() + " ,'" + ds.Tables[0].Rows[0]["Specification"].ToString() + "'," + ds.Tables[0].Rows[0]["Upon_Request"].ToString() + " ," + dp.Id_To + "," + dp.Type_To + ",'" + ds.Tables[0].Rows[0]["Currency"].ToString() + "') ";
                        int Excp = 0;
                        To_Store_Unit_Item_Table_Id = opp.Insert_me_Return_Last_Identify(mysql, ref Excp);
                        To_Store_LargBalance = 0;
                        To_Store_Small_Balance = 0;
                        To_Store_Item_No = 0;
                    }
                    else
                    {
                        To_Store_Unit_Item_Table_Id = -1;
                    }

                }
                else
                {
                    To_Store_Unit_Item_Table_Id = long.Parse(ds.Tables[0].Rows[0]["id"].ToString());
                    To_Store_LargBalance = decimal.Parse(ds.Tables[0].Rows[0]["Full_qua"].ToString());
                    To_Store_Small_Balance = decimal.Parse(ds.Tables[0].Rows[0]["D_Qua"].ToString());
                    To_Store_Item_No = decimal.Parse(ds.Tables[0].Rows[0]["Item_No"].ToString());
                }
                return true;
            }
            catch (Exception ee)
            {
                // MessageBox.Show(ee.Message);
                To_Store_Unit_Item_Table_Id = 0;
                return false;
            }


        }
        public bool Check_ItemFoundOnStore(ref operations opp, Detailes_Purshing dp, ref long To_Store_Unit_Item_Table_Id, ref decimal To_Store_LargBalance, ref decimal To_Store_Small_Balance, ref decimal To_Store_Item_No)
        {
            try
            {
                if (order_Purshing.Order_Type == (int)StoreOrderType.SarfTahweel_1)
                {

                }
                else if (order_Purshing.Order_Type == (int)StoreOrderType.MortagaSarfTahweel_6)
                {

                }
                else
                {
                    return true;
                }
                //if (dp.Type_To == 3 | dp.Type_To == 10 | dp.Type_To == 8)  //  هالك - تشغيل - تسوية بالنقص
                //{
                //    To_Store_Unit_Item_Table_Id = 0;
                //    return true;
                //}


                string mysql = "";
                DataSet ds = new DataSet();
                ds = opp.return_dataset("select * from Unit_item WHERE (Item_id = " + dp.Item_Id + ") AND  (Store_id = " + dp.Id_To + ")   ");

                //ItemCLS itemCLS = new ItemCLS();
                //Unit_item uitm = itemCLS.GetUnit_item(ref   opp, dp.Unit_item_Table_Id);
                //if (uitm != null)
                //{

                //}
                if (ds.Tables[0].Rows.Count == 0)
                {
                    ds.Clear();
                    ds = opp.return_dataset("select * from Unit_item WHERE ( id = " + dp.Unit_item_Table_Id + ")");
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        mysql = " INSERT INTO Unit_item (Item_id, Code, unit_id, Full_qua, D_Qua, Item_No, Reserved_Balance_Larg_Qnty, Reserved_Balance_Small_Qnty, Big_unit, Small_unit, P_price, S_price, Specification, Upon_Request , Store_id, Store_Type, Currency) ";
                        mysql = mysql + " VALUES (" + ds.Tables[0].Rows[0]["Item_id"].ToString() + ",'" + ds.Tables[0].Rows[0]["Code"].ToString() + "'," + ds.Tables[0].Rows[0]["unit_id"].ToString() + ", 0, 0,0, 0, 0," + ds.Tables[0].Rows[0]["Big_unit"].ToString() + "," + ds.Tables[0].Rows[0]["Small_unit"].ToString() + "," + ds.Tables[0].Rows[0]["P_price"].ToString() + "," + ds.Tables[0].Rows[0]["S_price"].ToString() + " ,'" + ds.Tables[0].Rows[0]["Specification"].ToString() + "'," + ds.Tables[0].Rows[0]["Upon_Request"].ToString() + " ," + dp.Id_To + "," + dp.Type_To + ",'" + ds.Tables[0].Rows[0]["Currency"].ToString() + "') ";
                        int Excp = 0;
                        To_Store_Unit_Item_Table_Id = opp.Insert_me_Return_Last_Identify(mysql, ref Excp);
                        To_Store_LargBalance = 0;
                        To_Store_Small_Balance = 0;
                        To_Store_Item_No = 0;
                    }
                    else
                    {
                        To_Store_Unit_Item_Table_Id = long.Parse(ds.Tables[0].Rows[0]["id"].ToString());
                    }

                }
                else
                {
                    To_Store_Unit_Item_Table_Id = long.Parse(ds.Tables[0].Rows[0]["id"].ToString());
                    To_Store_LargBalance = decimal.Parse(ds.Tables[0].Rows[0]["Full_qua"].ToString());
                    To_Store_Small_Balance = decimal.Parse(ds.Tables[0].Rows[0]["D_Qua"].ToString());
                    To_Store_Item_No = decimal.Parse(ds.Tables[0].Rows[0]["Item_No"].ToString());
                }
                return true;
            }
            catch (Exception ee)
            {
                // MessageBox.Show(ee.Message);
                To_Store_Unit_Item_Table_Id = 0;
                return false;
            }


        }
        public bool Check_ItemFoundOnEmployee(ref operations opp, ref long Unit_itemEmployeeBalanceId, long Item_Id, long EmployeeId)
        {
            try
            {
                if (order_Purshing.Order_Type == (int)StoreOrderType.SarfToEmployee_20)
                {

                }
                else if (order_Purshing.Order_Type == (int)StoreOrderType.EstlamFromEmployee_21)
                {

                }
                else
                {
                    return true;
                }

                decimal Full_qua = 0;
                decimal D_Qua = 0;
                decimal Item_No = 0;
                string mysql = "";
                DataSet ds = new DataSet();
                ds = opp.return_dataset("select * from Unit_itemEmployeeBalance WHERE (Item_id = " + Item_Id + ") AND  (EmployeeId = " + EmployeeId + ")   ");


                if (ds.Tables[0].Rows.Count == 0)
                {
                    ds.Clear();
                    ds = opp.return_dataset("select * from Item WHERE ( id = " + Item_Id + ")");
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        mysql = " INSERT INTO Unit_itemEmployeeBalance (EmployeeId,Item_id, Code, unit_id, Full_qua, D_Qua, Item_No) ";
                        mysql = mysql + " VALUES (" + EmployeeId + "," + ds.Tables[0].Rows[0]["id"].ToString() + ",'" + ds.Tables[0].Rows[0]["Code"].ToString() + "'," + ds.Tables[0].Rows[0]["Unit_Id"].ToString() + ", 0, 0,0) ";
                        int Excp = 0;
                        Unit_itemEmployeeBalanceId = opp.Insert_me_Return_Last_Identify(mysql, ref Excp);
                        Full_qua = 0;
                        D_Qua = 0;
                        Item_No = 0;
                    }
                    else
                    {
                        Unit_itemEmployeeBalanceId = long.Parse(ds.Tables[0].Rows[0]["id"].ToString());
                    }

                }
                else
                {
                    Unit_itemEmployeeBalanceId = long.Parse(ds.Tables[0].Rows[0]["id"].ToString());
                    Full_qua = decimal.Parse(ds.Tables[0].Rows[0]["Full_qua"].ToString());
                    D_Qua = decimal.Parse(ds.Tables[0].Rows[0]["D_Qua"].ToString());
                    Item_No = decimal.Parse(ds.Tables[0].Rows[0]["Item_No"].ToString());
                }
                return true;
            }
            catch (Exception ee)
            {
                // MessageBox.Show(ee.Message);
                Unit_itemEmployeeBalanceId = 0;
                return false;
            }


        }

        string Get_mySql_Make_New_Order()
        {
            string mysql = " ";
            //string mysql = " INSERT INTO Order_Purshing (Order_Purshing_No, Order_Date, Order_Type, Type_From, Id_From, Type_To, Id_To, Invoice_ID,User_Id,Branch_ID,Id_FromStr,Id_ToStr,MainOrderTable_id,Is_Converted,NotesAll) ";
            //mysql = mysql + " VALUES   (" + Order_Purshing_No + ",'" + operations.dateFormat(Order_Date) + "'," + Order_Type + "," + Type_From + "," + Id_From + "," + Type_To + "," + Id_To + "," + InvoiceTable_ID + "," + Common.User_Id + "," + Common.Branch_Id + ",'" + Id_FromStr + "','" + Id_ToStr + "'," + MainOrderTable_id + "," + Is_Converted + ",N'" + NotesAll + "') ";

            return mysql;


        }

        public void AddOrder_Purshing(ref int Excp, ref operations opp, ref long ID, Order_Purshing order_Purshing)
        {
            try
            {
                Order_PurshingCLS op = new Order_PurshingCLS();

                op.AddOrder_Purshing(ref opp, order_Purshing);
                ID = this.order_Purshing.Id;
                //string mysql = Get_mySql_Make_New_Order();
                //ID = opp.Insert_me_Return_Last_Identify(mysql, ref Excp);


                Excp = 0;

            }
            catch (Exception e)
            {
                Excp = 1;
                opp.db.Entry(order_Purshing).State = EntityState.Detached;
                this.order_Purshing.Id = 0;

                throw (e);
            }

        }
        public void Update_Order_FromMainId(ref int Excp, ref operations opp, Order_Purshing order_Purshing, Order_Purshing order_Purshing_C)
        {
            try
            {
                Order_PurshingCLS op = new Order_PurshingCLS();
                op.Update_Order_FromMainId(ref Excp, ref opp, order_Purshing, order_Purshing_C);



                Excp = 0;

            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }

        }


        private DataTable Get_Items_Data(ref int Excp, ref operations opp, string mysql)
        {
            DataTable dt = new DataTable();
            try
            {

                dt = opp.return_DataTable(mysql);
                Excp = 0;

                return dt;
            }
            catch (Exception ee)
            {

                Excp = 1;
                // MessageBox.Show(ee.Message);
                return dt;
            }
        }
        public long GetNewOrderNo(ref operations opp, int Order_Type)
        {
            string mysql;
            int Excp = 0;
            long Order_Purshing_No = 1;
            /* if (Order_Type == 1)
             {
                 Order_Type = 2;
             }
             * */
            mysql = " SELECT MAX(Order_Purshing_No) AS Order_Purshing_No FROM Order_Purshing WHERE (Order_Type = " + Order_Type + ") ";
            DataTable dt = Get_Items_Data(ref Excp, ref opp, mysql);

            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["Order_Purshing_No"].ToString() == "")
                {
                    Order_Purshing_No = 1;
                    return Order_Purshing_No;
                }
                else
                {
                    Order_Purshing_No = long.Parse(dt.Rows[0]["Order_Purshing_No"].ToString());
                    if (Order_Purshing_No < 0)
                    {
                        Order_Purshing_No = 1;// Order_Purshing_No + 100;
                    }
                    else
                    {
                        Order_Purshing_No = Order_Purshing_No + 1;
                    }
                    return Order_Purshing_No;
                }

            }
            else
            {
                Order_Purshing_No = 1;
                return Order_Purshing_No;
            }
        }



        public DataTable Get_Order_Items_listEdit(ref int Excp, long OrderTable_id)
        {
            string mysql;
            mysql = @"  SELECT    Store.Store_Name,    unit.u_name, unit.B_Unit, unit.S_unit, Unit_item.id, Unit_item.Item_id, Item.No3eboa, Unit_item.Store_id, Unit_item.Store_Type, Unit_item.Code, Unit_item.unit_id, Unit_item.Reserved_Balance_Larg_Qnty, 
                         Unit_item.Reserved_Balance_Small_Qnty, Unit_item.Big_unit, Unit_item.Small_unit, Unit_item.P_price, Unit_item.S_price, Unit_item.Specification, Unit_item.Upon_Request, Unit_item.Currency, Item.Item_Name, 
                         Unit_item.id AS Unit_itemTable_id, Unit_item.Item_No AS Balance_Now_Item_No, Unit_item.D_Qua AS Balance_Now_Small_Qnty, Unit_item.Full_qua AS Balance_Now_Larg_Qnty, 
                         Order_Purshing.Order_Purshing_No,Order_Purshing.NotesAll, Order_Purshing.Order_date, Order_Purshing.Order_Type, Order_Purshing.type_From, Order_Purshing.Id_From, Order_Purshing.Type_To, Order_Purshing.Id_To, 
                         Order_Purshing.Invoice_ID, Detailes_Purshing.Balance_Id_From_Larg_Qnty, Detailes_Purshing.Balance_Id_From_Small_Qnty, Detailes_Purshing.Balance_Id_From_Item_No, 
                         Detailes_Purshing.Balance_Id_To_Larg_Qnty, Detailes_Purshing.Balance_Id_To_Small_Qnty, Detailes_Purshing.Balance_Id_To_Item_No, Detailes_Purshing.Id AS Detailes_Purshing_Id, 
                         Detailes_Purshing.To_Store_Unit_Item_Table_Id, Detailes_Purshing.Unit_item_Table_Id, Detailes_Purshing.Notes_From, Detailes_Purshing.Notes_To, Detailes_Purshing.Notes, Detailes_Purshing.Qua_Befor, 
                         Detailes_Purshing.Part_qua_Befor, Detailes_Purshing.Item_No_Befor, Detailes_Purshing.Qua_After, Detailes_Purshing.Part_qua_After, Detailes_Purshing.Item_No_After, 
                         Detailes_Purshing.Item_No AS Detailes_Purshing_Item_No, Detailes_Purshing.Part_qua AS Detailes_Purshing_Part_qua, Detailes_Purshing.qua AS Detailes_Purshing_qua, unit.B_Unit_Value, unit.S_unit_Value, 
                         ROUND(Detailes_Purshing.qua + Detailes_Purshing.Part_qua / unit.S_unit_Value, 2) AS Qantity, ROUND(Detailes_Purshing.Qua_After + Detailes_Purshing.Part_qua_After / unit.S_unit_Value, 2) AS Quantity_After, 
                         ROUND(Detailes_Purshing.Qua_Befor + Detailes_Purshing.Part_qua_Befor / unit.S_unit_Value, 2) AS Quantity_Befor, ROUND((Detailes_Purshing.qua + Detailes_Purshing.Part_qua / unit.S_unit_Value) 
                         / Item.No3eboa, 2) AS Item_No_No3eboa, ROUND((Detailes_Purshing.Qua_After + Detailes_Purshing.Part_qua_After / unit.S_unit_Value) / Item.No3eboa, 2) AS Item_No_No3eboa_After, 
                         ROUND((Detailes_Purshing.Qua_Befor + Detailes_Purshing.Part_qua_Befor / unit.S_unit_Value) / Item.No3eboa, 2) AS Item_No_No3eboa_Befor, Detailes_Purshing.OrderTable_id, Detailes_Purshing.Machine_Id, 
                         Detailes_Purshing.qua AS Larg_Qnty, Detailes_Purshing.Part_qua AS Small_Qnty, Detailes_Purshing.Item_No, '' AS Order_Estlam_Sarf_Flage, '' AS QntyStr, '' AS Item_NoStr, '' AS QntyStr_After, 
                         '' AS Item_NoStr_After, '' AS QntyStr_Befor, '' AS Item_NoStr_Befor,0 AS  IS_Editable , Detailes_Purshing.Related_Detailes_Purshing_Id, Detailes_Purshing.Transaction_Type ,    Order_Purshing.MainOrderTable_id,
Detailes_Purshing.Notes_Item
FROM            Item INNER JOIN
                         Unit_item ON Item.id = Unit_item.Item_id INNER JOIN
                         unit ON Unit_item.unit_id = unit.id INNER JOIN
                         Detailes_Purshing ON Unit_item.id = Detailes_Purshing.Unit_item_Table_Id INNER JOIN
                         Order_Purshing ON Detailes_Purshing.OrderTable_id = Order_Purshing.Id INNER JOIN
                         Store ON Unit_item.Store_id = Store.id
            ";
            mysql = mysql + " WHERE (Detailes_Purshing.OrderTable_id = " + OrderTable_id + ")  ";

            //  mysql = mysql + " AND (Detailes_Purshing.Flage_Not_Show <= 0)  ";


            return Get_Items_DataTable(ref Excp, mysql);

        }
        public DataTable Get_Item_Raw_MaterialsFor_Entage(ref int Excp, long OrderTable_id)
        {
            string mysql;
            mysql = @"  SELECT        Item.S_id AS MainStore_Id, Item_Raw_MaterialsFor_Entage.Id, Item_Raw_MaterialsFor_Entage.MainOrderTable_id, Item_Raw_MaterialsFor_Entage.SubOrderTable_id, Item_Raw_MaterialsFor_Entage.Process_Date, 
                         Item_Raw_MaterialsFor_Entage.MainItem_Id, Item_Raw_MaterialsFor_Entage.SubItem_Id, Item_Raw_MaterialsFor_Entage.MainItem_Name, Item_Raw_MaterialsFor_Entage.SubItem_Name, 
                         Item_Raw_MaterialsFor_Entage.Unit_Id, Item_Raw_MaterialsFor_Entage.S_unit_Value, Item_Raw_MaterialsFor_Entage.Raw_Full_qua, Item_Raw_MaterialsFor_Entage.Raw_D_Qua, 
                         Item_Raw_MaterialsFor_Entage.Raw_Quantity, Item_Raw_MaterialsFor_Entage.Larg_Qnty, Item_Raw_MaterialsFor_Entage.Small_Qnty, Item_Raw_MaterialsFor_Entage.Qnty, 
                         Item_Raw_MaterialsFor_Entage.Main_Larg_Qnty, Item_Raw_MaterialsFor_Entage.Main_Small_Qnty, Item_Raw_MaterialsFor_Entage.Main_Qnty, Item_Raw_MaterialsFor_Entage.Raw_Full_qua AS Full_qua, 
                         Item_Raw_MaterialsFor_Entage.Raw_D_Qua AS D_Qua, Item_Raw_MaterialsFor_Entage.Raw_Quantity AS Quantity, Item_Raw_MaterialsFor_Entage.Main_Larg_Qnty AS Entry_Larg_Qnty, 
                         Item_Raw_MaterialsFor_Entage.Main_Small_Qnty AS Entry_Small_Qnty, Item_Raw_MaterialsFor_Entage.Main_Qnty AS Entry_Qnty, Unit_item.Full_qua AS SubFull_qua, Unit_item.D_Qua AS SubD_Qua, 
                         Unit_item.Item_No AS SubItem_No, Item_Raw_MaterialsFor_Entage.Big_unit, Item_Raw_MaterialsFor_Entage.Small_unit, 0 AS SubQntyBalance, 1 AS CheckBalanceAllows, Unit_item.id AS SubUnit_item_id,0 AS  IS_Editable, 0 AS Entry_Edit_Diff,0 AS Raw_Diff_Balance,0 AS Qnty_AfterEdit , Cast(Item_Raw_MaterialsFor_Entage.SubStore_Id as int ) AS S_Id, Item_Raw_MaterialsFor_Entage.SubStore_Name AS Store_Name , Item_Raw_MaterialsFor_Entage.SubUnit_item_id 
FROM            Unit_item INNER JOIN
                         Item_Raw_MaterialsFor_Entage ON Unit_item.id = Item_Raw_MaterialsFor_Entage.SubUnit_item_id INNER JOIN
                         Item ON Unit_item.Item_id = Item.id
            ";
            mysql = mysql + " WHERE (Item_Raw_MaterialsFor_Entage.MainOrderTable_id = " + OrderTable_id + ")  ";

            //  mysql = mysql + " AND (Detailes_Purshing.Flage_Not_Show <= 0)  ";


            return Get_Items_DataTable(ref Excp, mysql);

        }

        private DataTable Get_Items_DataTable(ref int Excp, string mysql)
        {
            DataTable dt = new DataTable();
            try
            {
                operations opp = new operations();
                dt = opp.return_DataTable(mysql);
                Excp = 0;
                opp = null;
                return dt;
            }
            catch (Exception ee)
            {

                Excp = 1;
                // MessageBox.Show(ee.Message);
                return dt;
            }
        }
        public void Update_No3eboa_To_1_From0()
        {
            ItemCLS itm = new ItemCLS();
            itm.Update_No3eboa_To_1_From0(ref opp);
        }

        public void DeleteDetailes_Purshing(Detailes_Purshing dp)
        {
            Detailes_PurshingList.Remove(dp);

            Detailes_PurshingCLS dpcls = new Detailes_PurshingCLS();
            dpcls.DeleteDetailes_Purshing(ref opp, dp);
        }

        public Unit_item GetUnit_item(long Unit_item_Id)
        {
            ItemCLS itm = new ItemCLS();
            Unit_item um = itm.GetUnit_item(ref opp, Unit_item_Id);
            return um;
        }



        void Add_NewItemIN_Order(ref int Excp, ref operations opp, Order_Purshing order_Purshing, Detailes_Purshing dp)
        {
            ItemCLS itm = new ItemCLS();
            Unit_item um = itm.GetUnit_item(ref opp, dp.Unit_item_Table_Id);
            if (um == null)
            {

                return;
            }

            long Related_Detailes_Purshing_Id = 0;

            Make_Order_Details_AllOneClick(ref Excp, ref opp, order_Purshing, dp, ref Related_Detailes_Purshing_Id, um);
        }

        public void Edit_Order_DetailsFromEditOLD(ref int Excp, ref operations opp, Detailes_Purshing dpEdit)
        {
            decimal New_Larg_Qnty = dpEdit.qua.Value;
            decimal New_Small_Qnty = dpEdit.Part_qua.Value;
            decimal New_ItemNO = dpEdit.Item_No.Value;

            long Detailes_Purshing_Import_Export_Id = 0;
            long Detailes_Purshing_Id = dpEdit.Id;// long.Parse(dt_NewOrder.Rows[index]["Detailes_Purshing_Id"].ToString());
            long Related_Detailes_Purshing_Id = dpEdit.Related_Detailes_Purshing_Id;
            Detailes_PurshingCLS dpCLS = new Detailes_PurshingCLS();
            Detailes_Purshing dpOld = dpCLS.GetDetailes_PurshingById(ref opp, Detailes_Purshing_Id);



            long Unit_Item_Table_Id = dpEdit.Unit_item_Table_Id;// long.Parse(dt_NewOrder.Rows[index]["Unit_item_Table_Id"].ToString());
            long To_Store_Unit_Item_Table_Id = dpEdit.To_Store_Unit_Item_Table_Id;// long.Parse(dt_NewOrder.Rows[index]["To_Store_Unit_Item_Table_Id"].ToString());
                                                                                  //decimal Larg_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Detailes_Purshing_qua"].ToString());
                                                                                  //decimal Small_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Detailes_Purshing_Part_qua"].ToString());
                                                                                  //decimal ItemNO = decimal.Parse(dt_NewOrder.Rows[index]["Detailes_Purshing_Item_No"].ToString());
                                                                                  //decimal S_unit_Value = decimal.Parse(dt_NewOrder.Rows[index]["S_unit_Value"].ToString());
                                                                                  //long Item_Id = long.Parse(dt_NewOrder.Rows[index]["Item_id"].ToString());
                                                                                  //int Order_Type = int.Parse(dt_NewOrder.Rows[index]["Order_Type"].ToString());
                                                                                  //int Transaction_Type = int.Parse(dt_NewOrder.Rows[index]["Transaction_Type"].ToString());
                                                                                  //long Related_Detailes_Purshing_Id = long.Parse(dt_NewOrder.Rows[index]["Related_Detailes_Purshing_Id"].ToString());
            long MainOrderTable_id = dpEdit.Order_Purshing.MainOrderTable_id;// long.Parse(dt_NewOrder.Rows[index]["MainOrderTable_id"].ToString());
            long OrderTable_id = dpEdit.OrderTable_id;// long.Parse(dt_NewOrder.Rows[index]["OrderTable_id"].ToString());
                                                      //decimal Qua_After = 0;
                                                      //try
                                                      //{
                                                      //    Qua_After = long.Parse(dt_NewOrder.Rows[index]["Qua_After"].ToString());
                                                      //}
                                                      //catch { }
                                                      //if (New_Larg_Qnty == 0 & New_Small_Qnty == 0)
                                                      //{
                                                      //    New_Larg_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Larg_Qnty"].ToString());
                                                      //    New_Small_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Small_Qnty"].ToString());
                                                      //    New_ItemNO = decimal.Parse(dt_NewOrder.Rows[index]["Item_NO"].ToString());
                                                      //}

            decimal Larg_Qnty = dpOld.qua.Value;
            decimal Small_Qnty = dpOld.Part_qua.Value;
            decimal ItemNO = dpOld.Item_No.Value;
            decimal S_unit_Value = dpOld.Unit_item.Small_unit.Value;


            decimal Diff_Larg_Qnty = New_Larg_Qnty - Larg_Qnty;
            decimal Diff_Small_Qnty = New_Small_Qnty - Small_Qnty;
            decimal Diff_ItemNO = New_ItemNO - ItemNO;
            decimal Diff_Larg_Qnty_ = Diff_Larg_Qnty;
            decimal Diff_Small_Qnty_ = Diff_Small_Qnty;
            Common.Get_PositiveNumber(ref Diff_Larg_Qnty_);
            Common.Get_PositiveNumber(ref Diff_Small_Qnty_);
            decimal Diff_Quantity = Common.Get_Item_Quantity(Diff_Larg_Qnty_, Diff_Small_Qnty_, S_unit_Value);
            decimal Quantity = Common.Get_Item_Quantity(New_Larg_Qnty, New_Small_Qnty, S_unit_Value);
            decimal Quantity_Balance = 0;


            dpEdit.qua = New_Larg_Qnty;
            dpEdit.Part_qua = New_Small_Qnty;
            dpEdit.Item_No = New_ItemNO;

            string mysql = "";


            //mysql = " Update Detailes_Purshing Set qua=" + New_Larg_Qnty + " ,Part_qua=" + New_Small_Qnty + ",Item_No=" + New_ItemNO + "   ";
            //mysql = mysql + " ,  Qua_After =Qua_After + " + Diff_Larg_Qnty + " , ";
            //mysql = mysql + " Part_qua_After =Part_qua_After + " + Diff_Small_Qnty + " , ";
            //mysql = mysql + " Item_No_After = Item_No_After + " + Diff_ItemNO + "   ";

            //mysql = mysql + "   where(id = " + Detailes_Purshing_Id + ") ";
            //opp.insert_me(mysql);

            ////////Related_Detailes_Purshing_Id
            Detailes_Purshing dpRelated = dpCLS.GetDetailes_PurshingById(ref opp, Related_Detailes_Purshing_Id);
            dpRelated.qua = New_Larg_Qnty;
            dpRelated.Part_qua = New_Small_Qnty;
            dpRelated.Item_No = New_ItemNO;


            //mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty - " + Diff_Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty - " + Diff_Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  - " + Diff_ItemNO + "  where (id>  " + Related_Detailes_Purshing_Id + ")    And Unit_item_Table_Id=" + To_Store_Unit_Item_Table_Id + " ";
            //opp.insert_me(mysql);



            //mysql = " Update Detailes_Purshing Set qua=" + New_Larg_Qnty + " ,Part_qua=" + New_Small_Qnty + ",Item_No=" + New_ItemNO + " ";
            //mysql = mysql + " ,  Qua_After =Qua_After - " + Diff_Larg_Qnty + " , ";
            //mysql = mysql + " Part_qua_After =Part_qua_After - " + Diff_Small_Qnty + " , ";
            //mysql = mysql + " Item_No_After = Item_No_After - " + Diff_ItemNO + "   ";

            //mysql = mysql + " where (id=" + Related_Detailes_Purshing_Id + ") ";
            //opp.insert_me(mysql);


            //mysql = " Update Detailes_Purshing_Import_Export Set qua=" + New_Larg_Qnty + " ,Part_qua=" + New_Small_Qnty + ",Item_No=" + New_ItemNO + "  where (Detailes_Purshing_Id=" + Detailes_Purshing_Id + ") ";
            //opp.insert_me(mysql);

            //mysql = " Update Detailes_Purshing_Import_Export Set  Balance_Larg_Qnty = Balance_Larg_Qnty + " + Diff_Larg_Qnty+", Balance_Small_Qnty = Balance_Small_Qnty + "+Diff_Small_Qnty+ ", Balance_Item_No = Balance_Item_No  + "+Diff_ItemNO+" where(Detailes_Purshing_Id = " + Detailes_Purshing_Id + ") ";
            //opp.insert_me(mysql);

            try
            {
                ////////////////////Detailes_Purshing_Import_Export ////////////////////
                #region Transform          تحويل
                if (order_Purshing.Order_Type == (int)StoreOrderType.SarfTahweel_1 | order_Purshing.Order_Type == (int)StoreOrderType.MortagaSarfTahweel_6)             //التحويل
                {
                    //mysql = " Update Detailes_Purshing_Import_Export Set  Balance_Larg_Qnty = Balance_Larg_Qnty - " + Diff_Larg_Qnty + ", Balance_Small_Qnty = Balance_Small_Qnty - " + Diff_Small_Qnty + ", Balance_Item_No = Balance_Item_No  - " + Diff_ItemNO + " where(Detailes_Purshing_Id >= " + Detailes_Purshing_Id + ") ";
                    //opp.insert_me(mysql);

                    if (dpEdit.Transaction_Type == 1)
                    {
                        dpEdit.Qua_After = dpOld.Qua_After.Value + Diff_Larg_Qnty;
                        dpEdit.Part_qua_After = dpOld.Part_qua_After + Diff_Small_Qnty;
                        dpEdit.Item_No_After = dpOld.Item_No_After + Diff_ItemNO;

                        dpEdit.Balance_Id_From_Larg_Qnty = dpOld.Balance_Id_From_Larg_Qnty + Diff_Larg_Qnty;
                        dpEdit.Balance_Id_From_Small_Qnty = dpOld.Balance_Id_From_Small_Qnty + Diff_Small_Qnty;
                        dpEdit.Balance_Id_From_Item_No = dpOld.Balance_Id_From_Item_No + Diff_ItemNO;

                        dpEdit.Balance_Id_To_Larg_Qnty = dpOld.Balance_Id_To_Larg_Qnty - Diff_Larg_Qnty;
                        dpEdit.Balance_Id_To_Small_Qnty = dpOld.Balance_Id_To_Small_Qnty - Diff_Small_Qnty;
                        dpEdit.Balance_Id_To_Item_No = dpOld.Balance_Id_To_Item_No - Diff_ItemNO;

                        dpEdit.Qua_Befor = dpOld.Qua_Befor.Value - Diff_Larg_Qnty;
                        dpEdit.Part_qua_Befor = dpOld.Part_qua_Befor - Diff_Small_Qnty;
                        dpEdit.Item_No_Befor = dpOld.Item_No_Befor - Diff_ItemNO;

                        dpCLS.UpdateDetailes_Purshing(ref Excp, ref opp, dpEdit);
                        //mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty + " + Diff_Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty + " + Diff_Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  + " + Diff_ItemNO + "  where (id>   " + Detailes_Purshing_Id + ") And Unit_item_Table_Id=" + Unit_Item_Table_Id + " ";
                        //opp.insert_me(mysql);



                        //////mysql = " UPDATE Detailes_Purshing SET Balance_Id_From_Larg_Qnty =Balance_Id_From_Larg_Qnty + " + Diff_Larg_Qnty + " , ";
                        //////mysql = mysql + "  Balance_Id_From_Small_Qnty = Balance_Id_From_Small_Qnty  + " + Diff_Small_Qnty + " , ";
                        //////mysql = mysql + " Balance_Id_From_Item_No = Balance_Id_From_Item_No + " + Diff_ItemNO + " , ";
                        //////mysql = mysql + " Balance_Id_To_Larg_Qnty =Balance_Id_To_Larg_Qnty-  " + Diff_Larg_Qnty + " , ";
                        //////mysql = mysql + "Balance_Id_To_Small_Qnty =Balance_Id_To_Small_Qnty - " + Diff_Small_Qnty + " , ";
                        //////mysql = mysql + " Balance_Id_To_Item_No =Balance_Id_To_Item_No -  " + Diff_ItemNO + " , ";
                        //////mysql = mysql + " Qua_After =Qua_After + " + Diff_Larg_Qnty + " , ";
                        //////mysql = mysql + " Part_qua_After =Part_qua_After + " + Diff_Small_Qnty + " , ";
                        //////mysql = mysql + " Item_No_After = Item_No_After + " + Diff_ItemNO + "   ";
                        //////mysql = mysql + " WHERE( Id = " + Detailes_Purshing_Id + ")";
                        //////opp.insert_me(mysql);




                        decimal Quantity_Import = Common.Get_Item_Quantity(New_Larg_Qnty, New_Small_Qnty, S_unit_Value);


                        mysql = "Select * from Detailes_Purshing_Import_Export Where Detailes_Purshing_Id  =" + Detailes_Purshing_Id + " ORDER BY Id DESC ";

                        DataTable dt = opp.return_DataTable(mysql);
                        decimal Balance_Larg_Qnty = 0;
                        decimal Balance_Small_Qnty = 0;
                        decimal Balance_Item_No = 0;
                        if (dt.Rows.Count > 0)
                        {
                            Detailes_Purshing_Import_Export_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                            Balance_Larg_Qnty = decimal.Parse(dt.Rows[0]["Balance_Larg_Qnty"].ToString());
                            Balance_Small_Qnty = decimal.Parse(dt.Rows[0]["Balance_Small_Qnty"].ToString());
                            Balance_Item_No = decimal.Parse(dt.Rows[0]["Balance_Item_No"].ToString());

                            Balance_Larg_Qnty = Balance_Larg_Qnty + Diff_Larg_Qnty;
                            Balance_Small_Qnty = Balance_Small_Qnty + Diff_Small_Qnty;
                            Balance_Item_No = Balance_Item_No + Diff_ItemNO;
                            Quantity_Balance = Common.Get_Item_Quantity(Balance_Larg_Qnty, Balance_Small_Qnty, S_unit_Value);

                            string Notes_From_To = dt.Rows[0]["Notes_From_To"].ToString();
                            string Notes = dt.Rows[0]["Notes"].ToString();
                            string Notes_From = dt.Rows[0]["Notes_From"].ToString();
                            string Notes_To = dt.Rows[0]["Notes_To"].ToString();
                            Notes = " تم تعديل القيمة المنصرفه من الصنف";

                            long Machine_Id = long.Parse(dt.Rows[0]["Machine_Id"].ToString());
                            mysql = " INSERT INTO Detailes_Purshing_Import_Export(Detailes_Purshing_Id, OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Quantity, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Quantity_Balance, qua_Import, Part_Qua_Import,Item_No_Import, Quantity_Import, qua_Export, Part_Qua_Export, Item_No_Export, Quantity_Export, Notes_From_To, Notes, Notes_From, Notes_To, Machine_Id, Transaction_Type,Process_Date,User_Id,OperationDate)";
                            mysql = mysql + " VALUES(" + Detailes_Purshing_Id + "," + order_Purshing.Id + "," + Unit_Item_Table_Id + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ", " + Balance_Larg_Qnty + "," + Balance_Small_Qnty + ", " + Balance_Item_No + "," + Quantity_Balance + ",0,0,0,0, " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ",'" + Notes_From_To + "','" + Notes + "','" + Notes_From + "','" + Notes_To + "'," + Machine_Id + ",1,'" + operations.dateFormat(DateTime.Now.Date) + "',," + Common.User_Id + "," + operations.Get_DateTimeToInsert(DateTime.Now) + ")";
                            opp.insert_me(mysql);

                            mysql = "Delete from Detailes_Purshing_Import_Export Where ID  =" + Detailes_Purshing_Import_Export_Id + "   ";
                            opp.insert_me(mysql);
                        }

                        /////////////////////////////////////////////////////////////////////////////
                        ///
                        dpRelated.Qua_After = dpRelated.Qua_After.Value - Diff_Larg_Qnty;
                        dpRelated.Part_qua_After = dpRelated.Part_qua_After - Diff_Small_Qnty;
                        dpRelated.Item_No_After = dpRelated.Item_No_After - Diff_ItemNO;


                        dpRelated.Balance_Id_From_Larg_Qnty = dpRelated.Balance_Id_From_Larg_Qnty - Diff_Larg_Qnty;
                        dpRelated.Balance_Id_From_Small_Qnty = dpRelated.Balance_Id_From_Small_Qnty - Diff_Small_Qnty;
                        dpRelated.Balance_Id_From_Item_No = dpRelated.Balance_Id_From_Item_No - Diff_ItemNO;

                        dpRelated.Balance_Id_To_Larg_Qnty = dpRelated.Balance_Id_To_Larg_Qnty + Diff_Larg_Qnty;
                        dpRelated.Balance_Id_To_Small_Qnty = dpRelated.Balance_Id_To_Small_Qnty + Diff_Small_Qnty;
                        dpRelated.Balance_Id_To_Item_No = dpRelated.Balance_Id_To_Item_No + Diff_ItemNO;

                        dpRelated.Qua_Befor = dpRelated.Qua_Befor.Value + Diff_Larg_Qnty;
                        dpRelated.Part_qua_Befor = dpRelated.Part_qua_Befor + Diff_Small_Qnty;
                        dpRelated.Item_No_Befor = dpRelated.Item_No_Befor + Diff_ItemNO;

                        dpCLS.UpdateDetailes_Purshing(ref Excp, ref opp, dpRelated);
                        ////تعديل الصنف الاخر Related_Detailes_Purshing_Id 
                        //////mysql = " UPDATE Detailes_Purshing SET Balance_Id_From_Larg_Qnty =Balance_Id_From_Larg_Qnty - " + Diff_Larg_Qnty + " , ";
                        //////mysql = mysql + "  Balance_Id_From_Small_Qnty = Balance_Id_From_Small_Qnty  - " + Diff_Small_Qnty + " , ";
                        //////mysql = mysql + " Balance_Id_From_Item_No = Balance_Id_From_Item_No - " + Diff_ItemNO + " , ";
                        //////mysql = mysql + " Balance_Id_To_Larg_Qnty =Balance_Id_To_Larg_Qnty +  " + Diff_Larg_Qnty + " , ";
                        //////mysql = mysql + "Balance_Id_To_Small_Qnty =Balance_Id_To_Small_Qnty + " + Diff_Small_Qnty + " , ";
                        //////mysql = mysql + " Balance_Id_To_Item_No =Balance_Id_To_Item_No +  " + Diff_ItemNO + " , ";
                        //////mysql = mysql + " Qua_After =Qua_After - " + Diff_Larg_Qnty + " , ";
                        //////mysql = mysql + " Part_qua_After =Part_qua_After - " + Diff_Small_Qnty + " , ";
                        //////mysql = mysql + " Item_No_After = Item_No_After - " + Diff_ItemNO + "   ";
                        //////mysql = mysql + " WHERE( Id = " + Related_Detailes_Purshing_Id + ")";
                        //////opp.insert_me(mysql);


                        mysql = "Select * from Detailes_Purshing_Import_Export Where Detailes_Purshing_Id  =" + Related_Detailes_Purshing_Id + " ORDER BY Id DESC "; ;
                        dt = opp.return_DataTable(mysql);
                        Balance_Larg_Qnty = 0;
                        Balance_Small_Qnty = 0;
                        Balance_Item_No = 0;
                        if (dt.Rows.Count > 0)
                        {
                            Detailes_Purshing_Import_Export_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                            Balance_Larg_Qnty = decimal.Parse(dt.Rows[0]["Balance_Larg_Qnty"].ToString());
                            Balance_Small_Qnty = decimal.Parse(dt.Rows[0]["Balance_Small_Qnty"].ToString());
                            Balance_Item_No = decimal.Parse(dt.Rows[0]["Balance_Item_No"].ToString());

                            Balance_Larg_Qnty = Balance_Larg_Qnty - Diff_Larg_Qnty;
                            Balance_Small_Qnty = Balance_Small_Qnty - Diff_Small_Qnty;
                            Balance_Item_No = Balance_Item_No - Diff_ItemNO;
                            Quantity_Balance = Common.Get_Item_Quantity(Balance_Larg_Qnty, Balance_Small_Qnty, S_unit_Value);

                            string Notes_From_To = dt.Rows[0]["Notes_From_To"].ToString();
                            string Notes = dt.Rows[0]["Notes"].ToString();
                            string Notes_From = dt.Rows[0]["Notes_From"].ToString();
                            string Notes_To = dt.Rows[0]["Notes_To"].ToString();

                            Notes = " تم تعديل القيمة الوارة من الصنف";
                            long Machine_Id = long.Parse(dt.Rows[0]["Machine_Id"].ToString());
                            mysql = " INSERT INTO Detailes_Purshing_Import_Export(Detailes_Purshing_Id, OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Quantity, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Quantity_Balance, qua_Import, Part_Qua_Import,Item_No_Import, Quantity_Import, qua_Export, Part_Qua_Export, Item_No_Export, Quantity_Export, Notes_From_To, Notes, Notes_From, Notes_To, Machine_Id, Transaction_Type,Process_Date,user_Id,OperationDate)";
                            mysql = mysql + " VALUES(" + Related_Detailes_Purshing_Id + "," + MainOrderTable_id + "," + To_Store_Unit_Item_Table_Id + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ", " + Balance_Larg_Qnty + "," + Balance_Small_Qnty + ", " + Balance_Item_No + "," + Quantity_Balance + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ",0,0,0,0,'" + Notes_From_To + "','" + Notes + "','" + Notes_From + "','" + Notes_To + "'," + Machine_Id + ",2,'" + operations.dateFormat(DateTime.Now.Date) + "'," + Common.User_Id + "," + operations.Get_DateTimeToInsert(DateTime.Now) + ")";
                            opp.insert_me(mysql);
                            //Detailes_Purshing_Import_Export_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                            mysql = "Delete from Detailes_Purshing_Import_Export Where ID  =" + Detailes_Purshing_Import_Export_Id + "   ";
                            opp.insert_me(mysql);
                        }


                        //mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty + " + Diff_Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty + " + Diff_Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  + " + Diff_ItemNO + "  where (id>   " + Detailes_Purshing_Id + ") And Unit_item_Table_Id=" + Unit_Item_Table_Id + " ";
                        //opp.insert_me(mysql);


                        //mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty - " + Diff_Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty - " + Diff_Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  - " + Diff_ItemNO + "  where (id>  " + Related_Detailes_Purshing_Id + ")    And Unit_item_Table_Id=" + To_Store_Unit_Item_Table_Id + " ";
                        //opp.insert_me(mysql);


                    }
                    if (dpEdit.Transaction_Type == 2)
                    {
                        //Diff_Larg_Qnty = Diff_Larg_Qnty * -1;


                        dpEdit.Qua_After = dpOld.Qua_After.Value - Diff_Larg_Qnty;
                        dpEdit.Part_qua_After = dpOld.Part_qua_After - Diff_Small_Qnty;
                        dpEdit.Item_No_After = dpOld.Item_No_After - Diff_ItemNO;

                        dpEdit.Balance_Id_From_Larg_Qnty = dpOld.Balance_Id_From_Larg_Qnty - Diff_Larg_Qnty;
                        dpEdit.Balance_Id_From_Small_Qnty = dpOld.Balance_Id_From_Small_Qnty - Diff_Small_Qnty;
                        dpEdit.Balance_Id_From_Item_No = dpOld.Balance_Id_From_Item_No - Diff_ItemNO;

                        dpEdit.Balance_Id_To_Larg_Qnty = dpOld.Balance_Id_To_Larg_Qnty + Diff_Larg_Qnty;
                        dpEdit.Balance_Id_To_Small_Qnty = dpOld.Balance_Id_To_Small_Qnty + Diff_Small_Qnty;
                        dpEdit.Balance_Id_To_Item_No = dpOld.Balance_Id_To_Item_No + Diff_ItemNO;

                        dpEdit.Qua_Befor = dpOld.Qua_Befor.Value + Diff_Larg_Qnty;
                        dpEdit.Part_qua_Befor = dpOld.Part_qua_Befor + Diff_Small_Qnty;
                        dpEdit.Item_No_Befor = dpOld.Item_No_Befor + Diff_ItemNO;

                        dpCLS.UpdateDetailes_Purshing(ref Excp, ref opp, dpEdit);


                        //mysql = " UPDATE Detailes_Purshing SET Balance_Id_From_Larg_Qnty =Balance_Id_From_Larg_Qnty - " + Diff_Larg_Qnty + " , ";
                        //mysql = mysql + "  Balance_Id_From_Small_Qnty = Balance_Id_From_Small_Qnty  - " + Diff_Small_Qnty + " , ";
                        //mysql = mysql + " Balance_Id_From_Item_No = Balance_Id_From_Item_No - " + Diff_ItemNO + " , ";
                        //mysql = mysql + " Balance_Id_To_Larg_Qnty =Balance_Id_To_Larg_Qnty +  " + Diff_Larg_Qnty + " , ";
                        //mysql = mysql + "Balance_Id_To_Small_Qnty =Balance_Id_To_Small_Qnty + " + Diff_Small_Qnty + " , ";
                        //mysql = mysql + " Balance_Id_To_Item_No =Balance_Id_To_Item_No +  " + Diff_ItemNO + " , ";
                        //mysql = mysql + " Qua_After =Qua_After - " + Diff_Larg_Qnty + " , ";
                        //mysql = mysql + " Part_qua_After =Part_qua_After - " + Diff_Small_Qnty + " , ";
                        //mysql = mysql + " Item_No_After = Item_No_After - " + Diff_ItemNO + "   ";
                        //mysql = mysql + " WHERE( Id = " + Detailes_Purshing_Id + ")";
                        //opp.insert_me(mysql);




                        decimal Quantity_Import = Common.Get_Item_Quantity(New_Larg_Qnty, New_Small_Qnty, S_unit_Value);


                        mysql = "Select * from Detailes_Purshing_Import_Export Where Detailes_Purshing_Id  =" + Detailes_Purshing_Id + " ORDER BY Id DESC "; ;
                        DataTable dt = opp.return_DataTable(mysql);
                        decimal Balance_Larg_Qnty = 0;
                        decimal Balance_Small_Qnty = 0;
                        decimal Balance_Item_No = 0;
                        if (dt.Rows.Count > 0)
                        {
                            Balance_Larg_Qnty = decimal.Parse(dt.Rows[0]["Balance_Larg_Qnty"].ToString());
                            Balance_Small_Qnty = decimal.Parse(dt.Rows[0]["Balance_Small_Qnty"].ToString());
                            Balance_Item_No = decimal.Parse(dt.Rows[0]["Balance_Item_No"].ToString());

                            Balance_Larg_Qnty = Balance_Larg_Qnty - Diff_Larg_Qnty;
                            Balance_Small_Qnty = Balance_Small_Qnty - Diff_Small_Qnty;
                            Balance_Item_No = Balance_Item_No - Diff_ItemNO;
                            Quantity_Balance = Common.Get_Item_Quantity(Balance_Larg_Qnty, Balance_Small_Qnty, S_unit_Value);

                            string Notes_From_To = dt.Rows[0]["Notes_From_To"].ToString();
                            string Notes = dt.Rows[0]["Notes"].ToString();
                            string Notes_From = dt.Rows[0]["Notes_From"].ToString();
                            string Notes_To = dt.Rows[0]["Notes_To"].ToString();
                            Notes = " تم تعديل القيمة المنصرفه من الصنف";

                            long Machine_Id = long.Parse(dt.Rows[0]["Machine_Id"].ToString());
                            mysql = " INSERT INTO Detailes_Purshing_Import_Export(Detailes_Purshing_Id, OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Quantity, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Quantity_Balance, qua_Import, Part_Qua_Import,Item_No_Import, Quantity_Import, qua_Export, Part_Qua_Export, Item_No_Export, Quantity_Export, Notes_From_To, Notes, Notes_From, Notes_To, Machine_Id, Transaction_Type,Process_Date,User_Id,OperationDate)";
                            mysql = mysql + " VALUES(" + Detailes_Purshing_Id + "," + order_Purshing.Id + "," + Unit_Item_Table_Id + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ", " + Balance_Larg_Qnty + "," + Balance_Small_Qnty + ", " + Balance_Item_No + "," + Quantity_Balance + ",0,0,0,0, " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ",'" + Notes_From_To + "','" + Notes + "','" + Notes_From + "','" + Notes_To + "'," + Machine_Id + ",2,'" + operations.dateFormat(DateTime.Now.Date) + "',," + Common.User_Id + "," + operations.Get_DateTimeToInsert(DateTime.Now) + ")";
                            opp.insert_me(mysql);

                            Detailes_Purshing_Import_Export_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                            mysql = "Delete from Detailes_Purshing_Import_Export Where ID  =" + Detailes_Purshing_Import_Export_Id + "   ";
                            opp.insert_me(mysql);
                        }

                        /////////////////////////////////////////////////////////////////////////////
                        ////تعديل الصنف الاخر Related_Detailes_Purshing_Id 

                        dpRelated.Qua_After = dpRelated.Qua_After.Value + Diff_Larg_Qnty;
                        dpRelated.Part_qua_After = dpRelated.Part_qua_After + Diff_Small_Qnty;
                        dpRelated.Item_No_After = dpRelated.Item_No_After + Diff_ItemNO;


                        dpRelated.Balance_Id_From_Larg_Qnty = dpRelated.Balance_Id_From_Larg_Qnty + Diff_Larg_Qnty;
                        dpRelated.Balance_Id_From_Small_Qnty = dpRelated.Balance_Id_From_Small_Qnty + Diff_Small_Qnty;
                        dpRelated.Balance_Id_From_Item_No = dpRelated.Balance_Id_From_Item_No + Diff_ItemNO;

                        dpRelated.Balance_Id_To_Larg_Qnty = dpRelated.Balance_Id_To_Larg_Qnty - Diff_Larg_Qnty;
                        dpRelated.Balance_Id_To_Small_Qnty = dpRelated.Balance_Id_To_Small_Qnty - Diff_Small_Qnty;
                        dpRelated.Balance_Id_To_Item_No = dpRelated.Balance_Id_To_Item_No - Diff_ItemNO;

                        dpRelated.Qua_Befor = dpRelated.Qua_Befor.Value - Diff_Larg_Qnty;
                        dpRelated.Part_qua_Befor = dpRelated.Part_qua_Befor - Diff_Small_Qnty;
                        dpRelated.Item_No_Befor = dpRelated.Item_No_Befor - Diff_ItemNO;

                        dpCLS.UpdateDetailes_Purshing(ref Excp, ref opp, dpRelated);
                        //mysql = " UPDATE Detailes_Purshing SET Balance_Id_From_Larg_Qnty =Balance_Id_From_Larg_Qnty + " + Diff_Larg_Qnty + " , ";
                        //mysql = mysql + "  Balance_Id_From_Small_Qnty = Balance_Id_From_Small_Qnty  + " + Diff_Small_Qnty + " , ";
                        //mysql = mysql + " Balance_Id_From_Item_No = Balance_Id_From_Item_No + " + Diff_ItemNO + " , ";
                        //mysql = mysql + " Balance_Id_To_Larg_Qnty =Balance_Id_To_Larg_Qnty -  " + Diff_Larg_Qnty + " , ";
                        //mysql = mysql + "Balance_Id_To_Small_Qnty =Balance_Id_To_Small_Qnty - " + Diff_Small_Qnty + " , ";
                        //mysql = mysql + " Balance_Id_To_Item_No =Balance_Id_To_Item_No -  " + Diff_ItemNO + " , ";
                        //mysql = mysql + " Qua_After =Qua_After + " + Diff_Larg_Qnty + " , ";
                        //mysql = mysql + " Part_qua_After =Part_qua_After + " + Diff_Small_Qnty + " , ";
                        //mysql = mysql + " Item_No_After = Item_No_After + " + Diff_ItemNO + "   ";
                        //mysql = mysql + " WHERE( Id = " + Related_Detailes_Purshing_Id + ")";
                        //opp.insert_me(mysql);


                        mysql = "Select * from Detailes_Purshing_Import_Export Where Detailes_Purshing_Id  =" + Related_Detailes_Purshing_Id + " ORDER BY Id DESC "; ;
                        dt = opp.return_DataTable(mysql);
                        Balance_Larg_Qnty = 0;
                        Balance_Small_Qnty = 0;
                        Balance_Item_No = 0;
                        if (dt.Rows.Count > 0)
                        {
                            Balance_Larg_Qnty = decimal.Parse(dt.Rows[0]["Balance_Larg_Qnty"].ToString());
                            Balance_Small_Qnty = decimal.Parse(dt.Rows[0]["Balance_Small_Qnty"].ToString());
                            Balance_Item_No = decimal.Parse(dt.Rows[0]["Balance_Item_No"].ToString());

                            Balance_Larg_Qnty = Balance_Larg_Qnty + Diff_Larg_Qnty;
                            Balance_Small_Qnty = Balance_Small_Qnty + Diff_Small_Qnty;
                            Balance_Item_No = Balance_Item_No + Diff_ItemNO;
                            Quantity_Balance = Common.Get_Item_Quantity(Balance_Larg_Qnty, Balance_Small_Qnty, S_unit_Value);

                            string Notes_From_To = dt.Rows[0]["Notes_From_To"].ToString();
                            string Notes = dt.Rows[0]["Notes"].ToString();
                            string Notes_From = dt.Rows[0]["Notes_From"].ToString();
                            string Notes_To = dt.Rows[0]["Notes_To"].ToString();

                            Notes = " تم تعديل القيمة المنصرفه من الصنف";
                            long Machine_Id = long.Parse(dt.Rows[0]["Machine_Id"].ToString());
                            mysql = " INSERT INTO Detailes_Purshing_Import_Export(Detailes_Purshing_Id, OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Quantity, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Quantity_Balance, qua_Import, Part_Qua_Import,Item_No_Import, Quantity_Import, qua_Export, Part_Qua_Export, Item_No_Export, Quantity_Export, Notes_From_To, Notes, Notes_From, Notes_To, Machine_Id, Transaction_Type,Process_Date,User_Id,OperationDate)";
                            mysql = mysql + " VALUES(" + Related_Detailes_Purshing_Id + "," + MainOrderTable_id + "," + To_Store_Unit_Item_Table_Id + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ", " + Balance_Larg_Qnty + "," + Balance_Small_Qnty + ", " + Balance_Item_No + "," + Quantity_Balance + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ",0,0,0,0,'" + Notes_From_To + "','" + Notes + "','" + Notes_From + "','" + Notes_To + "'," + Machine_Id + ",1,'" + operations.dateFormat(DateTime.Now.Date) + "'," + Common.User_Id + "," + operations.Get_DateTimeToInsert(DateTime.Now) + ")";
                            opp.insert_me(mysql);

                            Detailes_Purshing_Import_Export_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                            mysql = "Delete from Detailes_Purshing_Import_Export Where ID  =" + Detailes_Purshing_Import_Export_Id + "   ";
                            opp.insert_me(mysql);
                        }
                        mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty - " + Diff_Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty -" + Diff_Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  -  " + Diff_ItemNO + "  where (id>   " + Detailes_Purshing_Id + ") And Unit_item_Table_Id=" + Unit_Item_Table_Id + " ";
                        opp.insert_me(mysql);

                        mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty + " + Diff_Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty + " + Diff_Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  + " + Diff_ItemNO + "  where (id>  " + Related_Detailes_Purshing_Id + ")    And Unit_item_Table_Id=" + To_Store_Unit_Item_Table_Id + " ";
                        opp.insert_me(mysql);

                    }
                    if (dpEdit.Transaction_Type == 1)
                    {


                        ////تعديل بيانات المخزن المنصرف إلية بنقص  الرصيد
                        //To_Store_Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + "  WHERE (id = " + Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);


                        ////تعديل بيانات المخزن المنصرف منة بزيادة الرصيد
                        //Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Diff_Larg_Qnty + ", D_Qua =D_Qua - " + Diff_Small_Qnty + ", Item_No =Item_No - " + Diff_ItemNO + " WHERE (id = " + To_Store_Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);



                    }
                    if (dpEdit.Transaction_Type == 2)
                    {

                        ////تعديل بيانات المخزن المنصرف منة بزيادة الرصيد
                        //Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Diff_Larg_Qnty + ", D_Qua =D_Qua - " + Diff_Small_Qnty + ", Item_No =Item_No - " + Diff_ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                        ////تعديل بيانات المخزن المنصرف إلية بنقص  الرصيد
                        //To_Store_Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + "  WHERE (id = " + To_Store_Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);
                    }

                }
                #endregion

                #region     sarf or mortaga3 estlam
                //else if (Order_Type == 2 | Order_Type == 5)
                else if (dpEdit.Transaction_Type == 2)
                {
                    ////تعديل بيانات المخزن المنصرف منة بزيادة الرصيد
                    //Unit_Item_Table_Id
                    mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Diff_Larg_Qnty + ", D_Qua =D_Qua - " + Diff_Small_Qnty + ", Item_No =Item_No - " + Diff_ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                    opp.insert_me(mysql);

                    mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty + " + Diff_Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty + " + Diff_Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  + " + Diff_ItemNO + "  where (id>   " + Detailes_Purshing_Id + ") And Unit_item_Table_Id=" + Unit_Item_Table_Id + " ";
                    opp.insert_me(mysql);


                    decimal Quantity_Import = Common.Get_Item_Quantity(New_Larg_Qnty, New_Small_Qnty, S_unit_Value);


                    mysql = "Select * from Detailes_Purshing_Import_Export Where Detailes_Purshing_Id  =" + Detailes_Purshing_Id + " ORDER BY Id DESC "; ;
                    DataTable dt = opp.return_DataTable(mysql);
                    decimal Balance_Larg_Qnty = 0;
                    decimal Balance_Small_Qnty = 0;
                    decimal Balance_Item_No = 0;
                    if (dt.Rows.Count > 0)
                    {
                        Balance_Larg_Qnty = decimal.Parse(dt.Rows[0]["Balance_Larg_Qnty"].ToString());
                        Balance_Small_Qnty = decimal.Parse(dt.Rows[0]["Balance_Small_Qnty"].ToString());
                        Balance_Item_No = decimal.Parse(dt.Rows[0]["Balance_Item_No"].ToString());

                        Balance_Larg_Qnty = Balance_Larg_Qnty - Diff_Larg_Qnty;
                        Balance_Small_Qnty = Balance_Small_Qnty - Diff_Small_Qnty;
                        Balance_Item_No = Balance_Item_No - Diff_ItemNO;
                        Quantity_Balance = Common.Get_Item_Quantity(Balance_Larg_Qnty, Balance_Small_Qnty, S_unit_Value);

                        string Notes_From_To = dt.Rows[0]["Notes_From_To"].ToString();
                        string Notes = dt.Rows[0]["Notes"].ToString();
                        string Notes_From = dt.Rows[0]["Notes_From"].ToString();
                        string Notes_To = dt.Rows[0]["Notes_To"].ToString();
                        Notes = " تم تعديل القيمة المنصرفه من الصنف";

                        long Machine_Id = long.Parse(dt.Rows[0]["Machine_Id"].ToString());
                        mysql = " INSERT INTO Detailes_Purshing_Import_Export(Detailes_Purshing_Id, OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Quantity, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Quantity_Balance, qua_Import, Part_Qua_Import,Item_No_Import, Quantity_Import, qua_Export, Part_Qua_Export, Item_No_Export, Quantity_Export, Notes_From_To, Notes, Notes_From, Notes_To, Machine_Id, Transaction_Type,Process_Date,User_Id,OperationDate)";
                        mysql = mysql + " VALUES(" + Detailes_Purshing_Id + "," + order_Purshing.Id + "," + Unit_Item_Table_Id + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ", " + Balance_Larg_Qnty + "," + Balance_Small_Qnty + ", " + Balance_Item_No + "," + Quantity_Balance + ",0,0,0,0, " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ",'" + Notes_From_To + "','" + Notes + "','" + Notes_From + "','" + Notes_To + "'," + Machine_Id + ",2,'" + operations.dateFormat(DateTime.Now.Date) + "'," + Common.User_Id + "," + operations.Get_DateTimeToInsert(DateTime.Now) + ")";
                        opp.insert_me(mysql);

                        Detailes_Purshing_Import_Export_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                        mysql = "Delete from Detailes_Purshing_Import_Export Where ID  =" + Detailes_Purshing_Import_Export_Id + "   ";
                        opp.insert_me(mysql);
                    }

                }
                #endregion

                #region    mortaga3 sarf or  estlam
                //else if (Order_Type == 3 | Order_Type == 4 | Order_Type == 9)
                else if (dpEdit.Transaction_Type == 1)
                {

                    ////تعديل بيانات المخزن المنصرف إلية بنقص  الرصيد
                    //To_Store_Unit_Item_Table_Id
                    mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Diff_Larg_Qnty + ", D_Qua =D_Qua + " + Diff_Small_Qnty + ", Item_No =Item_No + " + Diff_ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                    opp.insert_me(mysql);

                    mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty - " + Diff_Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty -" + Diff_Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  -  " + Diff_ItemNO + "  where (id>   " + Detailes_Purshing_Id + ") And Unit_item_Table_Id=" + Unit_Item_Table_Id + " ";
                    opp.insert_me(mysql);


                    decimal Quantity_Import = Common.Get_Item_Quantity(New_Larg_Qnty, New_Small_Qnty, S_unit_Value);


                    mysql = "Select * from Detailes_Purshing_Import_Export Where Detailes_Purshing_Id  =" + Detailes_Purshing_Id + " ORDER BY Id DESC ";
                    DataTable dt = opp.return_DataTable(mysql);
                    decimal Balance_Larg_Qnty = 0;
                    decimal Balance_Small_Qnty = 0;
                    decimal Balance_Item_No = 0;
                    if (dt.Rows.Count > 0)
                    {
                        Balance_Larg_Qnty = decimal.Parse(dt.Rows[0]["Balance_Larg_Qnty"].ToString());
                        Balance_Small_Qnty = decimal.Parse(dt.Rows[0]["Balance_Small_Qnty"].ToString());
                        Balance_Item_No = decimal.Parse(dt.Rows[0]["Balance_Item_No"].ToString());

                        Balance_Larg_Qnty = Balance_Larg_Qnty + Diff_Larg_Qnty;
                        Balance_Small_Qnty = Balance_Small_Qnty + Diff_Small_Qnty;
                        Balance_Item_No = Balance_Item_No + Diff_ItemNO;
                        Quantity_Balance = Common.Get_Item_Quantity(Balance_Larg_Qnty, Balance_Small_Qnty, S_unit_Value);

                        string Notes_From_To = dt.Rows[0]["Notes_From_To"].ToString();
                        string Notes = dt.Rows[0]["Notes"].ToString();
                        string Notes_From = dt.Rows[0]["Notes_From"].ToString();
                        string Notes_To = dt.Rows[0]["Notes_To"].ToString();
                        Notes = " تم تعديل القيمة المنصرفه من الصنف";

                        long Machine_Id = long.Parse(dt.Rows[0]["Machine_Id"].ToString());
                        mysql = " INSERT INTO Detailes_Purshing_Import_Export(Detailes_Purshing_Id, OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Quantity, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Quantity_Balance, qua_Import, Part_Qua_Import,Item_No_Import, Quantity_Import, qua_Export, Part_Qua_Export, Item_No_Export, Quantity_Export, Notes_From_To, Notes, Notes_From, Notes_To, Machine_Id, Transaction_Type,Process_Date,User_Id,OperationDate)";
                        mysql = mysql + " VALUES(" + Detailes_Purshing_Id + "," + order_Purshing.Id + "," + Unit_Item_Table_Id + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ", " + Balance_Larg_Qnty + "," + Balance_Small_Qnty + ", " + Balance_Item_No + "," + Quantity_Balance + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ",0,0,0,0,'" + Notes_From_To + "','" + Notes + "','" + Notes_From + "','" + Notes_To + "'," + Machine_Id + ",1,'" + operations.dateFormat(DateTime.Now.Date) + "'," + Common.User_Id + "," + operations.Get_DateTimeToInsert(DateTime.Now) + ")";
                        opp.insert_me(mysql);

                        Detailes_Purshing_Import_Export_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                        mysql = "Delete from Detailes_Purshing_Import_Export Where ID  =" + Detailes_Purshing_Import_Export_Id + "   ";
                        opp.insert_me(mysql);
                    }
                }
                #endregion

                #region Edit Rest




                ////////

                mysql = " UPDATE Detailes_Purshing SET Balance_Id_From_Larg_Qnty = FLOOR((" + S_unit_Value + " * Balance_Id_From_Larg_Qnty + Balance_Id_From_Small_Qnty) / " + S_unit_Value + ") ,  ";
                mysql = mysql + " Balance_Id_From_Small_Qnty =FlOOR(((" + S_unit_Value + " * Balance_Id_From_Larg_Qnty + Balance_Id_From_Small_Qnty)   / " + S_unit_Value + " - FLOOR((" + S_unit_Value + " * Balance_Id_From_Larg_Qnty + Balance_Id_From_Small_Qnty) / " + S_unit_Value + ")) * " + S_unit_Value + " )";
                opp.insert_me(mysql);

                mysql = " UPDATE Detailes_Purshing SET Balance_Id_To_Larg_Qnty = FLOOR((" + S_unit_Value + " * Balance_Id_To_Larg_Qnty + Balance_Id_To_Small_Qnty) / " + S_unit_Value + ") ,  ";
                mysql = mysql + " Balance_Id_To_Small_Qnty = FLOOR(((" + S_unit_Value + " * Balance_Id_To_Larg_Qnty + Balance_Id_To_Small_Qnty)   / " + S_unit_Value + " - FLOOR((" + S_unit_Value + " * Balance_Id_To_Larg_Qnty + Balance_Id_To_Small_Qnty) / " + S_unit_Value + ")) * " + S_unit_Value + " )";
                opp.insert_me(mysql);


                #endregion



            }
            catch (Exception e)
            {

                Excp = 1;

            }

        }
        public void Edit_Order_DetailsFromEdit(ref int Excp, ref operations opp, Detailes_Purshing dpEdit)
        {
            decimal New_Larg_Qnty = dpEdit.qua.Value;
            decimal New_Small_Qnty = dpEdit.Part_qua.Value;
            decimal New_ItemNO = dpEdit.Item_No.Value;

            long Detailes_Purshing_Import_Export_Id = 0;
            long Detailes_Purshing_Id = dpEdit.Id;// long.Parse(dt_NewOrder.Rows[index]["Detailes_Purshing_Id"].ToString());
            long Related_Detailes_Purshing_Id = dpEdit.Related_Detailes_Purshing_Id;
            Detailes_PurshingCLS dpCLS = new Detailes_PurshingCLS();
            Detailes_Purshing dpOld = dpCLS.GetDetailes_PurshingASNoTrackingById(ref opp, Detailes_Purshing_Id);



            long Unit_Item_Table_Id = dpEdit.Unit_item_Table_Id;// long.Parse(dt_NewOrder.Rows[index]["Unit_item_Table_Id"].ToString());
            long To_Store_Unit_Item_Table_Id = dpEdit.To_Store_Unit_Item_Table_Id;// long.Parse(dt_NewOrder.Rows[index]["To_Store_Unit_Item_Table_Id"].ToString());

            long MainOrderTable_id = dpEdit.Order_Purshing.MainOrderTable_id;// long.Parse(dt_NewOrder.Rows[index]["MainOrderTable_id"].ToString());
            long OrderTable_id = dpEdit.OrderTable_id;// long.Parse(dt_NewOrder.Rows[index]["OrderTable_id"].ToString());

            decimal Larg_QntyOld = 0;
            decimal Small_QntyOld = 0;
            decimal ItemNOOld = 0;
            decimal S_unit_Value = 1;
            Larg_QntyOld = dpOld.qua.Value;
            Small_QntyOld = dpOld.Part_qua.Value;
            ItemNOOld = dpOld.Item_No.Value;

            if (dpOld.Transaction_Type == 1)
            {
                Larg_QntyOld = dpOld.Import_Qua.Value;
                Small_QntyOld = dpOld.Import_Part_Qua.Value;
                ItemNOOld = dpOld.Import_Item_No.Value;

            }
            else
            {
                Larg_QntyOld = dpOld.Export_Qua.Value;
                Small_QntyOld = dpOld.Export_Part_Qua.Value;
                ItemNOOld = dpOld.Export_Item_No.Value;
            }
            S_unit_Value = dpOld.Unit_item.Small_unit.Value;

            decimal Diff_Larg_Qnty = New_Larg_Qnty - Larg_QntyOld;
            decimal Diff_Small_Qnty = New_Small_Qnty - Small_QntyOld;
            decimal Diff_ItemNO = New_ItemNO - ItemNOOld;
            decimal Diff_Larg_Qnty_ = Diff_Larg_Qnty;
            decimal Diff_Small_Qnty_ = Diff_Small_Qnty;
            Common.Get_PositiveNumber(ref Diff_Larg_Qnty_);
            Common.Get_PositiveNumber(ref Diff_Small_Qnty_);
            decimal Diff_Quantity = Common.Get_Item_Quantity(Diff_Larg_Qnty_, Diff_Small_Qnty_, S_unit_Value);
            //decimal Quantity = Common.Get_Item_Quantity(New_Larg_Qnty, New_Small_Qnty, S_unit_Value);
            decimal Quantity_Balance = 0;


            dpEdit.qua = New_Larg_Qnty;
            dpEdit.Part_qua = New_Small_Qnty;
            dpEdit.Item_No = New_ItemNO;

            string mysql = "";

            try
            {
                if (dpEdit.Transaction_Type == 1)
                {
                    dpEdit.Import_Qua = New_Larg_Qnty;
                    dpEdit.Import_Part_Qua = New_Small_Qnty;
                    dpEdit.Import_Item_No = New_ItemNO;
                    dpEdit.Export_Qua = 0;
                    dpEdit.Export_Part_Qua = 0;
                    dpEdit.Export_Item_No = 0;


                    dpEdit.Qua_After = dpOld.Qua_After.Value + Diff_Larg_Qnty;
                    dpEdit.Part_qua_After = dpOld.Part_qua_After + Diff_Small_Qnty;
                    dpEdit.Item_No_After = dpOld.Item_No_After + Diff_ItemNO;

                    dpEdit.Balance_Id_From_Larg_Qnty = dpOld.Balance_Id_From_Larg_Qnty + Diff_Larg_Qnty;
                    dpEdit.Balance_Id_From_Small_Qnty = dpOld.Balance_Id_From_Small_Qnty + Diff_Small_Qnty;
                    dpEdit.Balance_Id_From_Item_No = dpOld.Balance_Id_From_Item_No + Diff_ItemNO;

                    dpEdit.Balance_Id_To_Larg_Qnty = dpOld.Balance_Id_To_Larg_Qnty - Diff_Larg_Qnty;
                    dpEdit.Balance_Id_To_Small_Qnty = dpOld.Balance_Id_To_Small_Qnty - Diff_Small_Qnty;
                    dpEdit.Balance_Id_To_Item_No = dpOld.Balance_Id_To_Item_No - Diff_ItemNO;

                    dpEdit.Qua_Befor = dpOld.Qua_Befor.Value - Diff_Larg_Qnty;
                    dpEdit.Part_qua_Befor = dpOld.Part_qua_Befor - Diff_Small_Qnty;
                    dpEdit.Item_No_Befor = dpOld.Item_No_Befor - Diff_ItemNO;

                    dpCLS.UpdateDetailes_Purshing(ref Excp, ref opp, dpEdit);


                    //////////
                    ///

                    ////تعديل بيانات المخزن المنصرف إلية بنقص  الرصيد
                    //To_Store_Unit_Item_Table_Id
                    //mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_QntyOld + ", D_Qua =D_Qua + " + Small_QntyOld + ", Item_No =Item_No + " + ItemNOOld + "  WHERE (id = " + Unit_Item_Table_Id + ") ";
                    //opp.insert_me(mysql);


                    mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Diff_Larg_Qnty + ", D_Qua =D_Qua + " + Diff_Small_Qnty + ", Item_No =Item_No + " + Diff_ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                    opp.insert_me(mysql);

                }


                if (dpEdit.Transaction_Type == 2)
                {
                    dpEdit.Import_Qua = 0;
                    dpEdit.Import_Part_Qua = 0;
                    dpEdit.Import_Item_No = 0;
                    dpEdit.Export_Qua = New_Larg_Qnty;
                    dpEdit.Export_Part_Qua = New_Small_Qnty;
                    dpEdit.Export_Item_No = New_ItemNO;

                    //Diff_Larg_Qnty = Diff_Larg_Qnty * -1;


                    dpEdit.Qua_After = dpOld.Qua_After.Value - Diff_Larg_Qnty;
                    dpEdit.Part_qua_After = dpOld.Part_qua_After - Diff_Small_Qnty;
                    dpEdit.Item_No_After = dpOld.Item_No_After - Diff_ItemNO;

                    dpEdit.Balance_Id_From_Larg_Qnty = dpOld.Balance_Id_From_Larg_Qnty - Diff_Larg_Qnty;
                    dpEdit.Balance_Id_From_Small_Qnty = dpOld.Balance_Id_From_Small_Qnty - Diff_Small_Qnty;
                    dpEdit.Balance_Id_From_Item_No = dpOld.Balance_Id_From_Item_No - Diff_ItemNO;

                    dpEdit.Balance_Id_To_Larg_Qnty = dpOld.Balance_Id_To_Larg_Qnty + Diff_Larg_Qnty;
                    dpEdit.Balance_Id_To_Small_Qnty = dpOld.Balance_Id_To_Small_Qnty + Diff_Small_Qnty;
                    dpEdit.Balance_Id_To_Item_No = dpOld.Balance_Id_To_Item_No + Diff_ItemNO;

                    dpEdit.Qua_Befor = dpOld.Qua_Befor.Value + Diff_Larg_Qnty;
                    dpEdit.Part_qua_Befor = dpOld.Part_qua_Befor + Diff_Small_Qnty;
                    dpEdit.Item_No_Befor = dpOld.Item_No_Befor + Diff_ItemNO;

                    dpCLS.UpdateDetailes_Purshing(ref Excp, ref opp, dpEdit);


                    ///////////////////////
                    ///
                    ////تعديل بيانات المخزن المنصرف منة بزيادة الرصيد
                    //Unit_Item_Table_Id
                    mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Diff_Larg_Qnty + ", D_Qua =D_Qua - " + Diff_Small_Qnty + ", Item_No =Item_No - " + Diff_ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                    opp.insert_me(mysql);



                }


                EditDetailes_Purshing_Import_Export(dpEdit, S_unit_Value, Diff_Larg_Qnty, Diff_Small_Qnty, Diff_ItemNO);

                ////////////////////Detailes_Purshing_Import_Export ////////////////////
                #region Transform          تحويل
                if (order_Purshing.Order_Type == (int)StoreOrderType.SarfTahweel_1 | order_Purshing.Order_Type == (int)StoreOrderType.MortagaSarfTahweel_6)             //التحويل
                {
                    ////////Related_Detailes_Purshing_Id
                    Detailes_Purshing dpRelated = dpCLS.GetDetailes_PurshingById(ref opp, Related_Detailes_Purshing_Id);
                    dpRelated.qua = New_Larg_Qnty;
                    dpRelated.Part_qua = New_Small_Qnty;
                    dpRelated.Item_No = New_ItemNO;
                    //mysql = " Update Detailes_Purshing_Import_Export Set  Balance_Larg_Qnty = Balance_Larg_Qnty - " + Diff_Larg_Qnty + ", Balance_Small_Qnty = Balance_Small_Qnty - " + Diff_Small_Qnty + ", Balance_Item_No = Balance_Item_No  - " + Diff_ItemNO + " where(Detailes_Purshing_Id >= " + Detailes_Purshing_Id + ") ";
                    //opp.insert_me(mysql);

                    if (dpRelated.Transaction_Type == 1)
                    {

                        dpRelated.Import_Qua = New_Larg_Qnty;
                        dpRelated.Import_Part_Qua = New_Small_Qnty;
                        dpRelated.Import_Item_No = New_ItemNO;
                        dpRelated.Export_Qua = 0;
                        dpRelated.Export_Part_Qua = 0;
                        dpRelated.Export_Item_No = 0;
                        /////////////////////////////////////////////////////////////////////////////
                        ///
                        dpRelated.Qua_After = dpRelated.Qua_After.Value - Diff_Larg_Qnty;
                        dpRelated.Part_qua_After = dpRelated.Part_qua_After - Diff_Small_Qnty;
                        dpRelated.Item_No_After = dpRelated.Item_No_After - Diff_ItemNO;


                        dpRelated.Balance_Id_From_Larg_Qnty = dpRelated.Balance_Id_From_Larg_Qnty - Diff_Larg_Qnty;
                        dpRelated.Balance_Id_From_Small_Qnty = dpRelated.Balance_Id_From_Small_Qnty - Diff_Small_Qnty;
                        dpRelated.Balance_Id_From_Item_No = dpRelated.Balance_Id_From_Item_No - Diff_ItemNO;

                        dpRelated.Balance_Id_To_Larg_Qnty = dpRelated.Balance_Id_To_Larg_Qnty + Diff_Larg_Qnty;
                        dpRelated.Balance_Id_To_Small_Qnty = dpRelated.Balance_Id_To_Small_Qnty + Diff_Small_Qnty;
                        dpRelated.Balance_Id_To_Item_No = dpRelated.Balance_Id_To_Item_No + Diff_ItemNO;

                        dpRelated.Qua_Befor = dpRelated.Qua_Befor.Value + Diff_Larg_Qnty;
                        dpRelated.Part_qua_Befor = dpRelated.Part_qua_Befor + Diff_Small_Qnty;
                        dpRelated.Item_No_Befor = dpRelated.Item_No_Befor + Diff_ItemNO;

                        dpCLS.UpdateDetailes_Purshing(ref Excp, ref opp, dpRelated);
                        ////تعديل الصنف الاخر Related_Detailes_Purshing_Id 


                        ////تعديل بيانات المخزن المنصرف منة بزيادة الرصيد
                        //Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Diff_Larg_Qnty + ", D_Qua =D_Qua - " + Diff_Small_Qnty + ", Item_No =Item_No - " + Diff_ItemNO + " WHERE (id = " + To_Store_Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                    }
                    if (dpRelated.Transaction_Type == 2)
                    {

                        dpRelated.Import_Qua = 0;
                        dpRelated.Import_Part_Qua = 0;
                        dpRelated.Import_Item_No = 0;
                        dpRelated.Export_Qua = New_Larg_Qnty;
                        dpRelated.Export_Part_Qua = New_Small_Qnty;
                        dpRelated.Export_Item_No = New_ItemNO;
                        /////////////////////////////////////////////////////////////////////////////
                        ////تعديل الصنف الاخر Related_Detailes_Purshing_Id 

                        dpRelated.Qua_After = dpRelated.Qua_After.Value + Diff_Larg_Qnty;
                        dpRelated.Part_qua_After = dpRelated.Part_qua_After + Diff_Small_Qnty;
                        dpRelated.Item_No_After = dpRelated.Item_No_After + Diff_ItemNO;


                        dpRelated.Balance_Id_From_Larg_Qnty = dpRelated.Balance_Id_From_Larg_Qnty + Diff_Larg_Qnty;
                        dpRelated.Balance_Id_From_Small_Qnty = dpRelated.Balance_Id_From_Small_Qnty + Diff_Small_Qnty;
                        dpRelated.Balance_Id_From_Item_No = dpRelated.Balance_Id_From_Item_No + Diff_ItemNO;

                        dpRelated.Balance_Id_To_Larg_Qnty = dpRelated.Balance_Id_To_Larg_Qnty - Diff_Larg_Qnty;
                        dpRelated.Balance_Id_To_Small_Qnty = dpRelated.Balance_Id_To_Small_Qnty - Diff_Small_Qnty;
                        dpRelated.Balance_Id_To_Item_No = dpRelated.Balance_Id_To_Item_No - Diff_ItemNO;

                        dpRelated.Qua_Befor = dpRelated.Qua_Befor.Value - Diff_Larg_Qnty;
                        dpRelated.Part_qua_Befor = dpRelated.Part_qua_Befor - Diff_Small_Qnty;
                        dpRelated.Item_No_Befor = dpRelated.Item_No_Befor - Diff_ItemNO;

                        dpCLS.UpdateDetailes_Purshing(ref Excp, ref opp, dpRelated);

                        ////تعديل بيانات المخزن المنصرف إلية بنقص  الرصيد
                        //To_Store_Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_QntyOld + ", D_Qua =D_Qua + " + Small_QntyOld + ", Item_No =Item_No + " + ItemNOOld + "  WHERE (id = " + To_Store_Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                    }
                    EditDetailes_Purshing_Import_Export(dpRelated, S_unit_Value, Diff_Larg_Qnty, Diff_Small_Qnty, Diff_ItemNO);


                }
                #endregion

                #region Employee          صرف لموظف
                if (order_Purshing.Order_Type == (int)StoreOrderType.SarfToEmployee_20 | order_Purshing.Order_Type == (int)StoreOrderType.EstlamFromEmployee_21)             // 
                {
                    long Unit_itemEmployeeBalanceId = 0;
                    long Item_Id_ = dpEdit.Item_Id;
                    long EmployeeId = 0;


                    if (dpEdit.Transaction_Type == 1)
                    {
                        EmployeeId = dpEdit.Id_From;

                        if (Check_ItemFoundOnEmployee(ref opp, ref Unit_itemEmployeeBalanceId, Item_Id_, EmployeeId))
                        {
                            mysql = " UPDATE Unit_itemEmployeeBalance  SET Full_qua =Full_qua - " + Diff_Larg_Qnty + ", D_Qua =D_Qua - " + Diff_Small_Qnty + ", Item_No =Item_No - " + Diff_ItemNO + " WHERE (id = " + Unit_itemEmployeeBalanceId + ") ";
                            opp.insert_me(mysql);


                        }
                    }
                    if (dpEdit.Transaction_Type == 2)
                    {
                        EmployeeId = dpEdit.Id_To;
                        if (Check_ItemFoundOnEmployee(ref opp, ref Unit_itemEmployeeBalanceId, Item_Id_, EmployeeId))
                        {


                            mysql = " UPDATE Unit_itemEmployeeBalance  SET Full_qua =Full_qua + " + Diff_Larg_Qnty + ", D_Qua =D_Qua + " + Diff_Small_Qnty + ", Item_No =Item_No + " + Diff_ItemNO + " WHERE (id = " + Unit_itemEmployeeBalanceId + ") ";
                            opp.insert_me(mysql);
                        }
                    }


                }
                #endregion

                /*
                  #region Transform          تحويل
                if (Order_Purshing.Order_Type == 1 | Order_Purshing.Order_Type == 6)             //التحويل
                {
                    //mysql = " Update Detailes_Purshing_Import_Export Set  Balance_Larg_Qnty = Balance_Larg_Qnty - " + Diff_Larg_Qnty + ", Balance_Small_Qnty = Balance_Small_Qnty - " + Diff_Small_Qnty + ", Balance_Item_No = Balance_Item_No  - " + Diff_ItemNO + " where(Detailes_Purshing_Id >= " + Detailes_Purshing_Id + ") ";
                    //opp.insert_me(mysql);

                    if (dpEdit.Transaction_Type == 1)
                    {
                        dpEdit.Qua_After = dpOld.Qua_After.Value + Diff_Larg_Qnty;
                        dpEdit.Part_qua_After = dpOld.Part_qua_After + Diff_Small_Qnty;
                        dpEdit.Item_No_After = dpOld.Item_No_After + Diff_ItemNO;

                        dpEdit.Balance_Id_From_Larg_Qnty = dpOld.Balance_Id_From_Larg_Qnty + Diff_Larg_Qnty;
                        dpEdit.Balance_Id_From_Small_Qnty = dpOld.Balance_Id_From_Small_Qnty + Diff_Small_Qnty;
                        dpEdit.Balance_Id_From_Item_No = dpOld.Balance_Id_From_Item_No + Diff_ItemNO;

                        dpEdit.Balance_Id_To_Larg_Qnty = dpOld.Balance_Id_To_Larg_Qnty - Diff_Larg_Qnty;
                        dpEdit.Balance_Id_To_Small_Qnty = dpOld.Balance_Id_To_Small_Qnty - Diff_Small_Qnty;
                        dpEdit.Balance_Id_To_Item_No = dpOld.Balance_Id_To_Item_No - Diff_ItemNO;

                        dpEdit.Qua_Befor = dpOld.Qua_Befor.Value - Diff_Larg_Qnty;
                        dpEdit.Part_qua_Befor = dpOld.Part_qua_Befor - Diff_Small_Qnty;
                        dpEdit.Item_No_Befor = dpOld.Item_No_Befor - Diff_ItemNO;

                        dpCLS.UpdateDetailes_Purshing(ref Excp, ref opp, dpEdit);
                        //mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty + " + Diff_Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty + " + Diff_Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  + " + Diff_ItemNO + "  where (id>   " + Detailes_Purshing_Id + ") And Unit_item_Table_Id=" + Unit_Item_Table_Id + " ";
                        //opp.insert_me(mysql);



                        //////mysql = " UPDATE Detailes_Purshing SET Balance_Id_From_Larg_Qnty =Balance_Id_From_Larg_Qnty + " + Diff_Larg_Qnty + " , ";
                        //////mysql = mysql + "  Balance_Id_From_Small_Qnty = Balance_Id_From_Small_Qnty  + " + Diff_Small_Qnty + " , ";
                        //////mysql = mysql + " Balance_Id_From_Item_No = Balance_Id_From_Item_No + " + Diff_ItemNO + " , ";
                        //////mysql = mysql + " Balance_Id_To_Larg_Qnty =Balance_Id_To_Larg_Qnty-  " + Diff_Larg_Qnty + " , ";
                        //////mysql = mysql + "Balance_Id_To_Small_Qnty =Balance_Id_To_Small_Qnty - " + Diff_Small_Qnty + " , ";
                        //////mysql = mysql + " Balance_Id_To_Item_No =Balance_Id_To_Item_No -  " + Diff_ItemNO + " , ";
                        //////mysql = mysql + " Qua_After =Qua_After + " + Diff_Larg_Qnty + " , ";
                        //////mysql = mysql + " Part_qua_After =Part_qua_After + " + Diff_Small_Qnty + " , ";
                        //////mysql = mysql + " Item_No_After = Item_No_After + " + Diff_ItemNO + "   ";
                        //////mysql = mysql + " WHERE( Id = " + Detailes_Purshing_Id + ")";
                        //////opp.insert_me(mysql);




                        decimal Quantity_Import = Common.Get_Item_Quantity(New_Larg_Qnty, New_Small_Qnty, S_unit_Value);


                        mysql = "Select * from Detailes_Purshing_Import_Export Where Detailes_Purshing_Id  =" + Detailes_Purshing_Id + " ORDER BY Id DESC ";

                        DataTable dt = opp.return_DataTable(mysql);
                        decimal Balance_Larg_Qnty = 0;
                        decimal Balance_Small_Qnty = 0;
                        decimal Balance_Item_No = 0;
                        if (dt.Rows.Count > 0)
                        {
                            Detailes_Purshing_Import_Export_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                            Balance_Larg_Qnty = decimal.Parse(dt.Rows[0]["Balance_Larg_Qnty"].ToString());
                            Balance_Small_Qnty = decimal.Parse(dt.Rows[0]["Balance_Small_Qnty"].ToString());
                            Balance_Item_No = decimal.Parse(dt.Rows[0]["Balance_Item_No"].ToString());

                            Balance_Larg_Qnty = Balance_Larg_Qnty + Diff_Larg_Qnty;
                            Balance_Small_Qnty = Balance_Small_Qnty + Diff_Small_Qnty;
                            Balance_Item_No = Balance_Item_No + Diff_ItemNO;
                            Quantity_Balance = Common.Get_Item_Quantity(Balance_Larg_Qnty, Balance_Small_Qnty, S_unit_Value);

                            string Notes_From_To = dt.Rows[0]["Notes_From_To"].ToString();
                            string Notes = dt.Rows[0]["Notes"].ToString();
                            string Notes_From = dt.Rows[0]["Notes_From"].ToString();
                            string Notes_To = dt.Rows[0]["Notes_To"].ToString();
                            Notes = " تم تعديل القيمة المنصرفه من الصنف";

                            long Machine_Id = long.Parse(dt.Rows[0]["Machine_Id"].ToString());
                            mysql = " INSERT INTO Detailes_Purshing_Import_Export(Detailes_Purshing_Id, OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Quantity, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Quantity_Balance, qua_Import, Part_Qua_Import,Item_No_Import, Quantity_Import, qua_Export, Part_Qua_Export, Item_No_Export, Quantity_Export, Notes_From_To, Notes, Notes_From, Notes_To, Machine_Id, Transaction_Type,Process_Date,User_Id,OperationDate)";
                            mysql = mysql + " VALUES(" + Detailes_Purshing_Id + "," + Order_Purshing.Id + "," + Unit_Item_Table_Id + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ", " + Balance_Larg_Qnty + "," + Balance_Small_Qnty + ", " + Balance_Item_No + "," + Quantity_Balance + ",0,0,0,0, " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ",'" + Notes_From_To + "','" + Notes + "','" + Notes_From + "','" + Notes_To + "'," + Machine_Id + ",1,'" + operations.dateFormat(DateTime.Now.Date) + "',," + Common.User_Id + "," + operations.Get_DateTimeToInsert(DateTime.Now) + ")";
                            opp.insert_me(mysql);

                            mysql = "Delete from Detailes_Purshing_Import_Export Where ID  =" + Detailes_Purshing_Import_Export_Id + "   ";
                            opp.insert_me(mysql);
                        }

                        /////////////////////////////////////////////////////////////////////////////
                        ///
                        dpRelated.Qua_After = dpRelated.Qua_After.Value - Diff_Larg_Qnty;
                        dpRelated.Part_qua_After = dpRelated.Part_qua_After - Diff_Small_Qnty;
                        dpRelated.Item_No_After = dpRelated.Item_No_After - Diff_ItemNO;


                        dpRelated.Balance_Id_From_Larg_Qnty = dpRelated.Balance_Id_From_Larg_Qnty - Diff_Larg_Qnty;
                        dpRelated.Balance_Id_From_Small_Qnty = dpRelated.Balance_Id_From_Small_Qnty - Diff_Small_Qnty;
                        dpRelated.Balance_Id_From_Item_No = dpRelated.Balance_Id_From_Item_No - Diff_ItemNO;

                        dpRelated.Balance_Id_To_Larg_Qnty = dpRelated.Balance_Id_To_Larg_Qnty + Diff_Larg_Qnty;
                        dpRelated.Balance_Id_To_Small_Qnty = dpRelated.Balance_Id_To_Small_Qnty + Diff_Small_Qnty;
                        dpRelated.Balance_Id_To_Item_No = dpRelated.Balance_Id_To_Item_No + Diff_ItemNO;

                        dpRelated.Qua_Befor = dpRelated.Qua_Befor.Value + Diff_Larg_Qnty;
                        dpRelated.Part_qua_Befor = dpRelated.Part_qua_Befor + Diff_Small_Qnty;
                        dpRelated.Item_No_Befor = dpRelated.Item_No_Befor + Diff_ItemNO;

                        dpCLS.UpdateDetailes_Purshing(ref Excp, ref opp, dpRelated);
                        ////تعديل الصنف الاخر Related_Detailes_Purshing_Id 
                        //////mysql = " UPDATE Detailes_Purshing SET Balance_Id_From_Larg_Qnty =Balance_Id_From_Larg_Qnty - " + Diff_Larg_Qnty + " , ";
                        //////mysql = mysql + "  Balance_Id_From_Small_Qnty = Balance_Id_From_Small_Qnty  - " + Diff_Small_Qnty + " , ";
                        //////mysql = mysql + " Balance_Id_From_Item_No = Balance_Id_From_Item_No - " + Diff_ItemNO + " , ";
                        //////mysql = mysql + " Balance_Id_To_Larg_Qnty =Balance_Id_To_Larg_Qnty +  " + Diff_Larg_Qnty + " , ";
                        //////mysql = mysql + "Balance_Id_To_Small_Qnty =Balance_Id_To_Small_Qnty + " + Diff_Small_Qnty + " , ";
                        //////mysql = mysql + " Balance_Id_To_Item_No =Balance_Id_To_Item_No +  " + Diff_ItemNO + " , ";
                        //////mysql = mysql + " Qua_After =Qua_After - " + Diff_Larg_Qnty + " , ";
                        //////mysql = mysql + " Part_qua_After =Part_qua_After - " + Diff_Small_Qnty + " , ";
                        //////mysql = mysql + " Item_No_After = Item_No_After - " + Diff_ItemNO + "   ";
                        //////mysql = mysql + " WHERE( Id = " + Related_Detailes_Purshing_Id + ")";
                        //////opp.insert_me(mysql);


                        mysql = "Select * from Detailes_Purshing_Import_Export Where Detailes_Purshing_Id  =" + Related_Detailes_Purshing_Id + " ORDER BY Id DESC "; ;
                        dt = opp.return_DataTable(mysql);
                        Balance_Larg_Qnty = 0;
                        Balance_Small_Qnty = 0;
                        Balance_Item_No = 0;
                        if (dt.Rows.Count > 0)
                        {
                            Detailes_Purshing_Import_Export_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                            Balance_Larg_Qnty = decimal.Parse(dt.Rows[0]["Balance_Larg_Qnty"].ToString());
                            Balance_Small_Qnty = decimal.Parse(dt.Rows[0]["Balance_Small_Qnty"].ToString());
                            Balance_Item_No = decimal.Parse(dt.Rows[0]["Balance_Item_No"].ToString());

                            Balance_Larg_Qnty = Balance_Larg_Qnty - Diff_Larg_Qnty;
                            Balance_Small_Qnty = Balance_Small_Qnty - Diff_Small_Qnty;
                            Balance_Item_No = Balance_Item_No - Diff_ItemNO;
                            Quantity_Balance = Common.Get_Item_Quantity(Balance_Larg_Qnty, Balance_Small_Qnty, S_unit_Value);

                            string Notes_From_To = dt.Rows[0]["Notes_From_To"].ToString();
                            string Notes = dt.Rows[0]["Notes"].ToString();
                            string Notes_From = dt.Rows[0]["Notes_From"].ToString();
                            string Notes_To = dt.Rows[0]["Notes_To"].ToString();

                            Notes = " تم تعديل القيمة الوارة من الصنف";
                            long Machine_Id = long.Parse(dt.Rows[0]["Machine_Id"].ToString());
                            mysql = " INSERT INTO Detailes_Purshing_Import_Export(Detailes_Purshing_Id, OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Quantity, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Quantity_Balance, qua_Import, Part_Qua_Import,Item_No_Import, Quantity_Import, qua_Export, Part_Qua_Export, Item_No_Export, Quantity_Export, Notes_From_To, Notes, Notes_From, Notes_To, Machine_Id, Transaction_Type,Process_Date,user_Id,OperationDate)";
                            mysql = mysql + " VALUES(" + Related_Detailes_Purshing_Id + "," + MainOrderTable_id + "," + To_Store_Unit_Item_Table_Id + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ", " + Balance_Larg_Qnty + "," + Balance_Small_Qnty + ", " + Balance_Item_No + "," + Quantity_Balance + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ",0,0,0,0,'" + Notes_From_To + "','" + Notes + "','" + Notes_From + "','" + Notes_To + "'," + Machine_Id + ",2,'" + operations.dateFormat(DateTime.Now.Date) + "'," + Common.User_Id + "," + operations.Get_DateTimeToInsert(DateTime.Now) + ")";
                            opp.insert_me(mysql);
                            //Detailes_Purshing_Import_Export_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                            mysql = "Delete from Detailes_Purshing_Import_Export Where ID  =" + Detailes_Purshing_Import_Export_Id + "   ";
                            opp.insert_me(mysql);
                        }


                        //mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty + " + Diff_Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty + " + Diff_Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  + " + Diff_ItemNO + "  where (id>   " + Detailes_Purshing_Id + ") And Unit_item_Table_Id=" + Unit_Item_Table_Id + " ";
                        //opp.insert_me(mysql);


                        //mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty - " + Diff_Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty - " + Diff_Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  - " + Diff_ItemNO + "  where (id>  " + Related_Detailes_Purshing_Id + ")    And Unit_item_Table_Id=" + To_Store_Unit_Item_Table_Id + " ";
                        //opp.insert_me(mysql);

                        //////////
                        ///

                        ////تعديل بيانات المخزن المنصرف إلية بنقص  الرصيد
                        //To_Store_Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + "  WHERE (id = " + Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);


                        ////تعديل بيانات المخزن المنصرف منة بزيادة الرصيد
                        //Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Diff_Larg_Qnty + ", D_Qua =D_Qua - " + Diff_Small_Qnty + ", Item_No =Item_No - " + Diff_ItemNO + " WHERE (id = " + To_Store_Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                    }
                    if (dpEdit.Transaction_Type == 2)
                    {
                        //Diff_Larg_Qnty = Diff_Larg_Qnty * -1;


                        dpEdit.Qua_After = dpOld.Qua_After.Value - Diff_Larg_Qnty;
                        dpEdit.Part_qua_After = dpOld.Part_qua_After - Diff_Small_Qnty;
                        dpEdit.Item_No_After = dpOld.Item_No_After - Diff_ItemNO;

                        dpEdit.Balance_Id_From_Larg_Qnty = dpOld.Balance_Id_From_Larg_Qnty - Diff_Larg_Qnty;
                        dpEdit.Balance_Id_From_Small_Qnty = dpOld.Balance_Id_From_Small_Qnty - Diff_Small_Qnty;
                        dpEdit.Balance_Id_From_Item_No = dpOld.Balance_Id_From_Item_No - Diff_ItemNO;

                        dpEdit.Balance_Id_To_Larg_Qnty = dpOld.Balance_Id_To_Larg_Qnty + Diff_Larg_Qnty;
                        dpEdit.Balance_Id_To_Small_Qnty = dpOld.Balance_Id_To_Small_Qnty + Diff_Small_Qnty;
                        dpEdit.Balance_Id_To_Item_No = dpOld.Balance_Id_To_Item_No + Diff_ItemNO;

                        dpEdit.Qua_Befor = dpOld.Qua_Befor.Value + Diff_Larg_Qnty;
                        dpEdit.Part_qua_Befor = dpOld.Part_qua_Befor + Diff_Small_Qnty;
                        dpEdit.Item_No_Befor = dpOld.Item_No_Befor + Diff_ItemNO;

                        dpCLS.UpdateDetailes_Purshing(ref Excp, ref opp, dpEdit);


                        //mysql = " UPDATE Detailes_Purshing SET Balance_Id_From_Larg_Qnty =Balance_Id_From_Larg_Qnty - " + Diff_Larg_Qnty + " , ";
                        //mysql = mysql + "  Balance_Id_From_Small_Qnty = Balance_Id_From_Small_Qnty  - " + Diff_Small_Qnty + " , ";
                        //mysql = mysql + " Balance_Id_From_Item_No = Balance_Id_From_Item_No - " + Diff_ItemNO + " , ";
                        //mysql = mysql + " Balance_Id_To_Larg_Qnty =Balance_Id_To_Larg_Qnty +  " + Diff_Larg_Qnty + " , ";
                        //mysql = mysql + "Balance_Id_To_Small_Qnty =Balance_Id_To_Small_Qnty + " + Diff_Small_Qnty + " , ";
                        //mysql = mysql + " Balance_Id_To_Item_No =Balance_Id_To_Item_No +  " + Diff_ItemNO + " , ";
                        //mysql = mysql + " Qua_After =Qua_After - " + Diff_Larg_Qnty + " , ";
                        //mysql = mysql + " Part_qua_After =Part_qua_After - " + Diff_Small_Qnty + " , ";
                        //mysql = mysql + " Item_No_After = Item_No_After - " + Diff_ItemNO + "   ";
                        //mysql = mysql + " WHERE( Id = " + Detailes_Purshing_Id + ")";
                        //opp.insert_me(mysql);




                        decimal Quantity_Import = Common.Get_Item_Quantity(New_Larg_Qnty, New_Small_Qnty, S_unit_Value);


                        mysql = "Select * from Detailes_Purshing_Import_Export Where Detailes_Purshing_Id  =" + Detailes_Purshing_Id + " ORDER BY Id DESC "; ;
                        DataTable dt = opp.return_DataTable(mysql);
                        decimal Balance_Larg_Qnty = 0;
                        decimal Balance_Small_Qnty = 0;
                        decimal Balance_Item_No = 0;
                        if (dt.Rows.Count > 0)
                        {
                            Balance_Larg_Qnty = decimal.Parse(dt.Rows[0]["Balance_Larg_Qnty"].ToString());
                            Balance_Small_Qnty = decimal.Parse(dt.Rows[0]["Balance_Small_Qnty"].ToString());
                            Balance_Item_No = decimal.Parse(dt.Rows[0]["Balance_Item_No"].ToString());

                            Balance_Larg_Qnty = Balance_Larg_Qnty - Diff_Larg_Qnty;
                            Balance_Small_Qnty = Balance_Small_Qnty - Diff_Small_Qnty;
                            Balance_Item_No = Balance_Item_No - Diff_ItemNO;
                            Quantity_Balance = Common.Get_Item_Quantity(Balance_Larg_Qnty, Balance_Small_Qnty, S_unit_Value);

                            string Notes_From_To = dt.Rows[0]["Notes_From_To"].ToString();
                            string Notes = dt.Rows[0]["Notes"].ToString();
                            string Notes_From = dt.Rows[0]["Notes_From"].ToString();
                            string Notes_To = dt.Rows[0]["Notes_To"].ToString();
                            Notes = " تم تعديل القيمة المنصرفه من الصنف";

                            long Machine_Id = long.Parse(dt.Rows[0]["Machine_Id"].ToString());
                            mysql = " INSERT INTO Detailes_Purshing_Import_Export(Detailes_Purshing_Id, OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Quantity, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Quantity_Balance, qua_Import, Part_Qua_Import,Item_No_Import, Quantity_Import, qua_Export, Part_Qua_Export, Item_No_Export, Quantity_Export, Notes_From_To, Notes, Notes_From, Notes_To, Machine_Id, Transaction_Type,Process_Date,User_Id,OperationDate)";
                            mysql = mysql + " VALUES(" + Detailes_Purshing_Id + "," + Order_Purshing.Id + "," + Unit_Item_Table_Id + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ", " + Balance_Larg_Qnty + "," + Balance_Small_Qnty + ", " + Balance_Item_No + "," + Quantity_Balance + ",0,0,0,0, " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ",'" + Notes_From_To + "','" + Notes + "','" + Notes_From + "','" + Notes_To + "'," + Machine_Id + ",2,'" + operations.dateFormat(DateTime.Now.Date) + "',," + Common.User_Id + "," + operations.Get_DateTimeToInsert(DateTime.Now) + ")";
                            opp.insert_me(mysql);

                            Detailes_Purshing_Import_Export_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                            mysql = "Delete from Detailes_Purshing_Import_Export Where ID  =" + Detailes_Purshing_Import_Export_Id + "   ";
                            opp.insert_me(mysql);
                        }

                        /////////////////////////////////////////////////////////////////////////////
                        ////تعديل الصنف الاخر Related_Detailes_Purshing_Id 

                        dpRelated.Qua_After = dpRelated.Qua_After.Value + Diff_Larg_Qnty;
                        dpRelated.Part_qua_After = dpRelated.Part_qua_After + Diff_Small_Qnty;
                        dpRelated.Item_No_After = dpRelated.Item_No_After + Diff_ItemNO;


                        dpRelated.Balance_Id_From_Larg_Qnty = dpRelated.Balance_Id_From_Larg_Qnty + Diff_Larg_Qnty;
                        dpRelated.Balance_Id_From_Small_Qnty = dpRelated.Balance_Id_From_Small_Qnty + Diff_Small_Qnty;
                        dpRelated.Balance_Id_From_Item_No = dpRelated.Balance_Id_From_Item_No + Diff_ItemNO;

                        dpRelated.Balance_Id_To_Larg_Qnty = dpRelated.Balance_Id_To_Larg_Qnty - Diff_Larg_Qnty;
                        dpRelated.Balance_Id_To_Small_Qnty = dpRelated.Balance_Id_To_Small_Qnty - Diff_Small_Qnty;
                        dpRelated.Balance_Id_To_Item_No = dpRelated.Balance_Id_To_Item_No - Diff_ItemNO;

                        dpRelated.Qua_Befor = dpRelated.Qua_Befor.Value - Diff_Larg_Qnty;
                        dpRelated.Part_qua_Befor = dpRelated.Part_qua_Befor - Diff_Small_Qnty;
                        dpRelated.Item_No_Befor = dpRelated.Item_No_Befor - Diff_ItemNO;

                        dpCLS.UpdateDetailes_Purshing(ref Excp, ref opp, dpRelated);
                        //mysql = " UPDATE Detailes_Purshing SET Balance_Id_From_Larg_Qnty =Balance_Id_From_Larg_Qnty + " + Diff_Larg_Qnty + " , ";
                        //mysql = mysql + "  Balance_Id_From_Small_Qnty = Balance_Id_From_Small_Qnty  + " + Diff_Small_Qnty + " , ";
                        //mysql = mysql + " Balance_Id_From_Item_No = Balance_Id_From_Item_No + " + Diff_ItemNO + " , ";
                        //mysql = mysql + " Balance_Id_To_Larg_Qnty =Balance_Id_To_Larg_Qnty -  " + Diff_Larg_Qnty + " , ";
                        //mysql = mysql + "Balance_Id_To_Small_Qnty =Balance_Id_To_Small_Qnty - " + Diff_Small_Qnty + " , ";
                        //mysql = mysql + " Balance_Id_To_Item_No =Balance_Id_To_Item_No -  " + Diff_ItemNO + " , ";
                        //mysql = mysql + " Qua_After =Qua_After + " + Diff_Larg_Qnty + " , ";
                        //mysql = mysql + " Part_qua_After =Part_qua_After + " + Diff_Small_Qnty + " , ";
                        //mysql = mysql + " Item_No_After = Item_No_After + " + Diff_ItemNO + "   ";
                        //mysql = mysql + " WHERE( Id = " + Related_Detailes_Purshing_Id + ")";
                        //opp.insert_me(mysql);


                        mysql = "Select * from Detailes_Purshing_Import_Export Where Detailes_Purshing_Id  =" + Related_Detailes_Purshing_Id + " ORDER BY Id DESC "; ;
                        dt = opp.return_DataTable(mysql);
                        Balance_Larg_Qnty = 0;
                        Balance_Small_Qnty = 0;
                        Balance_Item_No = 0;
                        if (dt.Rows.Count > 0)
                        {
                            Balance_Larg_Qnty = decimal.Parse(dt.Rows[0]["Balance_Larg_Qnty"].ToString());
                            Balance_Small_Qnty = decimal.Parse(dt.Rows[0]["Balance_Small_Qnty"].ToString());
                            Balance_Item_No = decimal.Parse(dt.Rows[0]["Balance_Item_No"].ToString());

                            Balance_Larg_Qnty = Balance_Larg_Qnty + Diff_Larg_Qnty;
                            Balance_Small_Qnty = Balance_Small_Qnty + Diff_Small_Qnty;
                            Balance_Item_No = Balance_Item_No + Diff_ItemNO;
                            Quantity_Balance = Common.Get_Item_Quantity(Balance_Larg_Qnty, Balance_Small_Qnty, S_unit_Value);

                            string Notes_From_To = dt.Rows[0]["Notes_From_To"].ToString();
                            string Notes = dt.Rows[0]["Notes"].ToString();
                            string Notes_From = dt.Rows[0]["Notes_From"].ToString();
                            string Notes_To = dt.Rows[0]["Notes_To"].ToString();

                            Notes = " تم تعديل القيمة المنصرفه من الصنف";
                            long Machine_Id = long.Parse(dt.Rows[0]["Machine_Id"].ToString());
                            mysql = " INSERT INTO Detailes_Purshing_Import_Export(Detailes_Purshing_Id, OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Quantity, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Quantity_Balance, qua_Import, Part_Qua_Import,Item_No_Import, Quantity_Import, qua_Export, Part_Qua_Export, Item_No_Export, Quantity_Export, Notes_From_To, Notes, Notes_From, Notes_To, Machine_Id, Transaction_Type,Process_Date,User_Id,OperationDate)";
                            mysql = mysql + " VALUES(" + Related_Detailes_Purshing_Id + "," + MainOrderTable_id + "," + To_Store_Unit_Item_Table_Id + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ", " + Balance_Larg_Qnty + "," + Balance_Small_Qnty + ", " + Balance_Item_No + "," + Quantity_Balance + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ",0,0,0,0,'" + Notes_From_To + "','" + Notes + "','" + Notes_From + "','" + Notes_To + "'," + Machine_Id + ",1,'" + operations.dateFormat(DateTime.Now.Date) + "'," + Common.User_Id + "," + operations.Get_DateTimeToInsert(DateTime.Now) + ")";
                            opp.insert_me(mysql);

                            Detailes_Purshing_Import_Export_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                            mysql = "Delete from Detailes_Purshing_Import_Export Where ID  =" + Detailes_Purshing_Import_Export_Id + "   ";
                            opp.insert_me(mysql);
                        }
                        mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty - " + Diff_Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty -" + Diff_Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  -  " + Diff_ItemNO + "  where (id>   " + Detailes_Purshing_Id + ") And Unit_item_Table_Id=" + Unit_Item_Table_Id + " ";
                        opp.insert_me(mysql);

                        mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty + " + Diff_Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty + " + Diff_Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  + " + Diff_ItemNO + "  where (id>  " + Related_Detailes_Purshing_Id + ")    And Unit_item_Table_Id=" + To_Store_Unit_Item_Table_Id + " ";
                        opp.insert_me(mysql);



                        ///////////////////////
                        ///
                         ////تعديل بيانات المخزن المنصرف منة بزيادة الرصيد
                        //Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Diff_Larg_Qnty + ", D_Qua =D_Qua - " + Diff_Small_Qnty + ", Item_No =Item_No - " + Diff_ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                        ////تعديل بيانات المخزن المنصرف إلية بنقص  الرصيد
                        //To_Store_Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + "  WHERE (id = " + To_Store_Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                    }
                   

                }
                #endregion
                 */




                #region Edit Rest

                ////////

                mysql = " UPDATE Detailes_Purshing SET Balance_Id_From_Larg_Qnty = FLOOR((" + S_unit_Value + " * Balance_Id_From_Larg_Qnty + Balance_Id_From_Small_Qnty) / " + S_unit_Value + ") ,  ";
                mysql = mysql + " Balance_Id_From_Small_Qnty =FlOOR(((" + S_unit_Value + " * Balance_Id_From_Larg_Qnty + Balance_Id_From_Small_Qnty)   / " + S_unit_Value + " - FLOOR((" + S_unit_Value + " * Balance_Id_From_Larg_Qnty + Balance_Id_From_Small_Qnty) / " + S_unit_Value + ")) * " + S_unit_Value + " )";
                opp.insert_me(mysql);

                mysql = " UPDATE Detailes_Purshing SET Balance_Id_To_Larg_Qnty = FLOOR((" + S_unit_Value + " * Balance_Id_To_Larg_Qnty + Balance_Id_To_Small_Qnty) / " + S_unit_Value + ") ,  ";
                mysql = mysql + " Balance_Id_To_Small_Qnty = FLOOR(((" + S_unit_Value + " * Balance_Id_To_Larg_Qnty + Balance_Id_To_Small_Qnty)   / " + S_unit_Value + " - FLOOR((" + S_unit_Value + " * Balance_Id_To_Larg_Qnty + Balance_Id_To_Small_Qnty) / " + S_unit_Value + ")) * " + S_unit_Value + " )";
                opp.insert_me(mysql);


                #endregion

            }
            catch (Exception e)
            {

                Excp = 1;
                throw;
            }

        }
        private void EditDetailes_Purshing_Import_Export(Detailes_Purshing dpEdit, decimal S_unit_Value, decimal Diff_Larg_Qnty, decimal Diff_Small_Qnty, decimal Diff_ItemNO)
        {

            decimal Quantity = Common.Get_Item_Quantity(dpEdit.qua.Value, dpEdit.Part_qua.Value, S_unit_Value);
            decimal Quantity_Import = Common.Get_Item_Quantity(dpEdit.qua.Value, dpEdit.Part_qua.Value, S_unit_Value);


            //string mysql = "Select * from Detailes_Purshing_Import_Export Where Detailes_Purshing_Id  =" + dpEdit.Id + " ORDER BY Id DESC ";


            Detailes_Purshing_Import_Export die = opp.db.Detailes_Purshing_Import_Exports.Where(i => i.Detailes_Purshing_Id == dpEdit.Id).FirstOrDefault();

            if (die != null)
            {
                decimal Balance_Larg_Qnty = 0;
                decimal Balance_Small_Qnty = 0;
                decimal Balance_Item_No = 0;

                long Detailes_Purshing_Import_Export_Id = die.Id;
                Balance_Larg_Qnty = die.Balance_Larg_Qnty ?? 0;
                Balance_Small_Qnty = die.Balance_Small_Qnty ?? 0;
                Balance_Item_No = die.Balance_Item_No ?? 0;
                if (dpEdit.Transaction_Type == 1)
                {
                    Balance_Larg_Qnty = Balance_Larg_Qnty + Diff_Larg_Qnty;
                    Balance_Small_Qnty = Balance_Small_Qnty + Diff_Small_Qnty;
                    Balance_Item_No = Balance_Item_No + Diff_ItemNO;

                    die.qua_Import = dpEdit.qua;
                    die.Part_Qua_Import = dpEdit.Part_qua;
                    die.Item_No_Import = dpEdit.Item_No;
                    die.Quantity_Import = Quantity;

                    die.qua_Export = 0;
                    die.Part_Qua_Export = 0;
                    die.Item_No_Export = 0;
                    die.Quantity_Export = 0;

                }
                else
                {
                    Balance_Larg_Qnty = Balance_Larg_Qnty - Diff_Larg_Qnty;
                    Balance_Small_Qnty = Balance_Small_Qnty - Diff_Small_Qnty;
                    Balance_Item_No = Balance_Item_No - Diff_ItemNO;

                    die.qua_Import = 0;
                    die.Part_Qua_Import = 0;
                    die.Item_No_Import = 0;
                    die.Quantity_Import = 0;

                    die.qua_Export = dpEdit.qua;
                    die.Part_Qua_Export = dpEdit.Part_qua;
                    die.Item_No_Export = dpEdit.Item_No;
                    die.Quantity_Export = Quantity;

                }
                decimal Quantity_Balance = Common.Get_Item_Quantity(Balance_Larg_Qnty, Balance_Small_Qnty, S_unit_Value);

                string Notes_From_To = dpEdit.Notes_From + "  " + dpEdit.Notes_To;//  dt.Rows[0]["Notes_From_To"].ToString();

                string Notes = dpEdit.Notes + " تم تعديل القيمة ";


                die.Detailes_Purshing_Id = dpEdit.Id;
                die.OrderTable_id = order_Purshing.Id;
                die.Unit_item_Table_Id = dpEdit.Unit_item_Table_Id;
                die.qua = dpEdit.qua;
                die.Part_qua = dpEdit.Part_qua;
                die.Item_No = dpEdit.Item_No;
                die.Quantity = Quantity;

                die.Balance_Larg_Qnty = Balance_Larg_Qnty;
                die.Balance_Small_Qnty = Balance_Small_Qnty;
                die.Balance_Item_No = Balance_Item_No;
                die.Quantity_Balance = Quantity_Balance;

                die.Notes_From_To = Notes_From_To;
                die.Notes = Notes;
                die.Notes_From = dpEdit.Notes_From;
                die.Notes_To = dpEdit.Notes_To;
                die.Machine_Id = dpEdit.Machine_Id;
                die.Transaction_Type = dpEdit.Transaction_Type;
                //die.Process_Date  = DateTime.Now.Date ;
                die.User_Id = Common.User_Id;
                die.OperationDate = DateTime.Now;


                opp.db.Entry(die).State = EntityState.Modified;
                opp.db.SaveChanges();


                //mysql = " INSERT INTO Detailes_Purshing_Import_Export(Detailes_Purshing_Id, OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Quantity, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Quantity_Balance, qua_Import, Part_Qua_Import,Item_No_Import, Quantity_Import, qua_Export, Part_Qua_Export, Item_No_Export, Quantity_Export, Notes_From_To, Notes, Notes_From, Notes_To, Machine_Id, Transaction_Type,Process_Date,User_Id,OperationDate)";
                //mysql = mysql + " VALUES(" + dpEdit.Id + "," + order_Purshing.Id + "," + dpEdit.Unit_item_Table_Id + ", " + dpEdit.qua + "," + dpEdit.Part_qua + "," + dpEdit.Item_No + "," + Quantity + ", " + Balance_Larg_Qnty + "," + Balance_Small_Qnty + ", " + Balance_Item_No + "," + Quantity_Balance + ",0,0,0,0, " + dpEdit.qua + "," + dpEdit.Part_qua + "," + dpEdit.Item_No + "," + Quantity + ",'" + Notes_From_To + "','" + Notes + "','" + dpEdit.Notes_From + "','" + dpEdit.Notes_To + "'," + dpEdit.Machine_Id + "," + dpEdit.Transaction_Type + ",'" + operations.dateFormat(DateTime.Now.Date) + "', " + Common.User_Id + "," + operations.Get_DateTimeToInsert(DateTime.Now) + ")";
                //opp.insert_me(mysql);

                //mysql = "Delete from Detailes_Purshing_Import_Export Where ID  =" + Detailes_Purshing_Import_Export_Id + "   ";
                //opp.insert_me(mysql);

            }
        }

        private void EditDetailes_Purshing_Import_ExportOld(Detailes_Purshing dpEdit, decimal S_unit_Value, decimal Diff_Larg_Qnty, decimal Diff_Small_Qnty, decimal Diff_ItemNO)
        {

            decimal Quantity = Common.Get_Item_Quantity(dpEdit.qua.Value, dpEdit.Part_qua.Value, S_unit_Value);
            decimal Quantity_Import = Common.Get_Item_Quantity(dpEdit.qua.Value, dpEdit.Part_qua.Value, S_unit_Value);


            string mysql = "Select * from Detailes_Purshing_Import_Export Where Detailes_Purshing_Id  =" + dpEdit.Id + " ORDER BY Id DESC ";

            DataTable dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count > 0)
            {
                decimal Balance_Larg_Qnty = 0;
                decimal Balance_Small_Qnty = 0;
                decimal Balance_Item_No = 0;

                long Detailes_Purshing_Import_Export_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                Balance_Larg_Qnty = decimal.Parse(dt.Rows[0]["Balance_Larg_Qnty"].ToString());
                Balance_Small_Qnty = decimal.Parse(dt.Rows[0]["Balance_Small_Qnty"].ToString());
                Balance_Item_No = decimal.Parse(dt.Rows[0]["Balance_Item_No"].ToString());
                if (dpEdit.Transaction_Type == 1)
                {
                    Balance_Larg_Qnty = Balance_Larg_Qnty + Diff_Larg_Qnty;
                    Balance_Small_Qnty = Balance_Small_Qnty + Diff_Small_Qnty;
                    Balance_Item_No = Balance_Item_No + Diff_ItemNO;
                }
                else
                {
                    Balance_Larg_Qnty = Balance_Larg_Qnty - Diff_Larg_Qnty;
                    Balance_Small_Qnty = Balance_Small_Qnty - Diff_Small_Qnty;
                    Balance_Item_No = Balance_Item_No - Diff_ItemNO;
                }
                decimal Quantity_Balance = Common.Get_Item_Quantity(Balance_Larg_Qnty, Balance_Small_Qnty, S_unit_Value);

                string Notes_From_To = dpEdit.Notes_From + "  " + dpEdit.Notes_To;//  dt.Rows[0]["Notes_From_To"].ToString();

                string Notes = dpEdit.Notes + " تم تعديل القيمة ";


                mysql = " INSERT INTO Detailes_Purshing_Import_Export(Detailes_Purshing_Id, OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Quantity, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Quantity_Balance, qua_Import, Part_Qua_Import,Item_No_Import, Quantity_Import, qua_Export, Part_Qua_Export, Item_No_Export, Quantity_Export, Notes_From_To, Notes, Notes_From, Notes_To, Machine_Id, Transaction_Type,Process_Date,User_Id,OperationDate)";
                mysql = mysql + " VALUES(" + dpEdit.Id + "," + order_Purshing.Id + "," + dpEdit.Unit_item_Table_Id + ", " + dpEdit.qua + "," + dpEdit.Part_qua + "," + dpEdit.Item_No + "," + Quantity + ", " + Balance_Larg_Qnty + "," + Balance_Small_Qnty + ", " + Balance_Item_No + "," + Quantity_Balance + ",0,0,0,0, " + dpEdit.qua + "," + dpEdit.Part_qua + "," + dpEdit.Item_No + "," + Quantity + ",'" + Notes_From_To + "','" + Notes + "','" + dpEdit.Notes_From + "','" + dpEdit.Notes_To + "'," + dpEdit.Machine_Id + "," + dpEdit.Transaction_Type + ",'" + operations.dateFormat(DateTime.Now.Date) + "', " + Common.User_Id + "," + operations.Get_DateTimeToInsert(DateTime.Now) + ")";
                opp.insert_me(mysql);

                mysql = "Delete from Detailes_Purshing_Import_Export Where ID  =" + Detailes_Purshing_Import_Export_Id + "   ";
                opp.insert_me(mysql);

            }
        }

        public DataTable Get_Order_Detailes_Purshing_Id(ref int Excp, ref operations opp, long OrderDetailes_Table_id)
        {
            string mysql;
            mysql = Get_QueryOrder();

            mysql = mysql + " WHERE (Detailes_Purshing.Id = " + OrderDetailes_Table_id + ")  ";

            //  mysql = mysql + " AND (Detailes_Purshing.Flage_Not_Show <= 0)  ";


            return Get_Items_DataTable(ref Excp, ref opp, mysql);

        }
        private DataTable Get_Items_DataTable(ref int Excp, ref operations opp, string mysql)
        {
            DataTable dt = new DataTable();
            try
            {

                dt = opp.return_DataTable(mysql);
                Excp = 0;

                return dt;
            }
            catch (Exception ee)
            {

                Excp = 1;
                //MessageBox.Show(ee.Message);
                return dt;
            }
        }
        void DeleteOrder(ref int Excp, ref string Msg, ref operations opp, long OrderTable_id)
        {
            //DataTable dt = Get_Order_Items_list(ref Excp, ref opp, OrderTable_id);
            Detailes_PurshingCLS dpCLS = new Detailes_PurshingCLS();
            List<Detailes_Purshing> dpList = dpCLS.GetDetailes_PurshingList(ref opp, OrderTable_id);
            foreach (var dp in dpList)
            {
                Delete_Order_DetailsFromEdit(ref Excp, ref opp, dp);
            }
            //for (int x = 0; x < dt.Rows.Count; x++)
            //{
            //    Delete_Order_DetailsFromEdit(ref Excp, ref opp, dt, x);
            //}

        }

        public void Delete_Order_DetailsFromEdit(ref int Excp, ref operations opp, Detailes_Purshing dp)
        {
            long Order_Purshing_Id = dp.OrderTable_id;// long.Parse(dt_NewOrder.Rows[index]["OrderTable_id"].ToString());
            long Detailes_Purshing_Id = dp.Id;// long.Parse(dt_NewOrder.Rows[index]["Detailes_Purshing_Id"].ToString());
            long Unit_Item_Table_Id = dp.Unit_item_Table_Id;// long.Parse(dt_NewOrder.Rows[index]["Unit_item_Table_Id"].ToString());
            long To_Store_Unit_Item_Table_Id = dp.To_Store_Unit_Item_Table_Id;// long.Parse(dt_NewOrder.Rows[index]["To_Store_Unit_Item_Table_Id"].ToString());
            decimal Larg_Qnty = dp.qua.Value;// decimal.Parse(dt_NewOrder.Rows[index]["Detailes_Purshing_qua"].ToString());
            decimal Small_Qnty = dp.Part_qua.Value;// decimal.Parse(dt_NewOrder.Rows[index]["Detailes_Purshing_Part_qua"].ToString());
            decimal ItemNO = dp.Item_No.Value;// decimal.Parse(dt_NewOrder.Rows[index]["Detailes_Purshing_Item_No"].ToString());
            decimal S_unit_Value = dp.Unit_item.Small_unit.Value;// decimal.Parse(dt_NewOrder.Rows[index]["S_unit_Value"].ToString());
            long Item_Id = dp.Item_Id;// long.Parse(dt_NewOrder.Rows[index]["Item_id"].ToString());
            int Order_Type = dp.Order_Purshing.Order_Type.Value;// int.Parse(dt_NewOrder.Rows[index]["Order_Type"].ToString());
            int Transaction_Type = dp.Transaction_Type.Value;// int.Parse(dt_NewOrder.Rows[index]["Transaction_Type"].ToString());
            long Related_Detailes_Purshing_Id = dp.Related_Detailes_Purshing_Id;// long.Parse(dt_NewOrder.Rows[index]["Related_Detailes_Purshing_Id"].ToString());
            long MainOrderTable_id = dp.Order_Purshing.MainOrderTable_id;

            string mysql = "";



            try
            {


                /////////////حذف الصنف الذى تم تحديد حذفة
                mysql = " Delete from Detailes_Purshing  where (id=" + Detailes_Purshing_Id + ") ";
                opp.insert_me(mysql);

                mysql = " Delete from Detailes_Purshing_Import_Export  where (Detailes_Purshing_Id=" + Detailes_Purshing_Id + ") ";
                opp.insert_me(mysql);

                DataDictionaryLogerProcessCLS ddlpCLS = new DataDictionaryLogerProcessCLS(ref opp);
                //ddlpCLS.AddNew(order_Purshing.Order_Type.Value, 3, order_Purshing.Id, order_Purshing.Order_Purshing_No,dp.Id, dp.Item_Name);
                ddlpCLS.AddNew(3, order_Purshing, dp);


                if (Transaction_Type == 1)
                {


                    ////تعديل بيانات المخزن المنصرف إلية بنقص  الرصيد
                    //To_Store_Unit_Item_Table_Id
                    mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + ItemNO + "  WHERE (id = " + Unit_Item_Table_Id + ") ";
                    opp.insert_me(mysql);

                }
                else
                {

                    ////تعديل بيانات المخزن المنصرف منة بزيادة الرصيد
                    //Unit_Item_Table_Id
                    mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                    opp.insert_me(mysql);

                }



                #region Transform          تحويل
                if (Order_Type == (int)StoreOrderType.SarfTahweel_1 | Order_Type == (int)StoreOrderType.MortagaSarfTahweel_6)             //التحويل
                {


                    /////////////حذف الصنف الذى تم تحويله من الاذن مباشر
                    mysql = " Delete from Detailes_Purshing  where (  Id=" + Related_Detailes_Purshing_Id + ") ";
                    opp.insert_me(mysql);

                    mysql = " Delete from Detailes_Purshing_Import_Export  where (Detailes_Purshing_Id=" + Related_Detailes_Purshing_Id + ") ";
                    opp.insert_me(mysql);


                    if (Transaction_Type == 1)
                    {
                        ////تعديل بيانات المخزن المنصرف منة بزيادة الرصيد
                        //Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + " WHERE (id = " + To_Store_Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);



                    }
                    else
                    {

                        ////تعديل بيانات المخزن المنصرف إلية بنقص  الرصيد
                        //To_Store_Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + ItemNO + "  WHERE (id = " + To_Store_Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);


                    }

                }
                #endregion

                #region Employee          صرف لموظف
                if (order_Purshing.Order_Type == (int)StoreOrderType.SarfToEmployee_20 | order_Purshing.Order_Type == (int)StoreOrderType.EstlamFromEmployee_21)             // 
                {
                    long Unit_itemEmployeeBalanceId = 0;
                    long Item_Id_ = dp.Item_Id;
                    long EmployeeId = 0;


                    if (Transaction_Type == 1)
                    {
                        EmployeeId = dp.Id_From;

                        if (Check_ItemFoundOnEmployee(ref opp, ref Unit_itemEmployeeBalanceId, Item_Id_, EmployeeId))
                        {
                            mysql = " UPDATE Unit_itemEmployeeBalance  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + "  WHERE (id = " + Unit_itemEmployeeBalanceId + ") ";
                            opp.insert_me(mysql);

                        }
                    }
                    if (Transaction_Type == 2)
                    {
                        EmployeeId = dp.Id_To;
                        if (Check_ItemFoundOnEmployee(ref opp, ref Unit_itemEmployeeBalanceId, Item_Id_, EmployeeId))
                        {
                            mysql = " UPDATE Unit_itemEmployeeBalance  SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + ItemNO + " WHERE (id = " + Unit_itemEmployeeBalanceId + ") ";
                            //arr.Add(mysql);
                            opp.insert_me(mysql);
                        }
                    }


                }
                #endregion
                /*
                #region Transform          تحويل
                if (Order_Type == 1 | Order_Type == 6)             //التحويل
                {
                    /////////////حذف الصنف الذى تم تحديد حذفة
                    mysql = " Delete from Detailes_Purshing  where (id=" + Detailes_Purshing_Id + ") ";
                    opp.insert_me(mysql);

                    mysql = " Delete from Detailes_Purshing_Import_Export  where (Detailes_Purshing_Id=" + Detailes_Purshing_Id + ") ";
                    opp.insert_me(mysql);

                    /////////////حذف الصنف الذى تم تحويله من الاذن مباشر
                    mysql = " Delete from Detailes_Purshing  where (  Id=" + Related_Detailes_Purshing_Id + ") ";
                    opp.insert_me(mysql);

                    mysql = " Delete from Detailes_Purshing_Import_Export  where (Detailes_Purshing_Id=" + Related_Detailes_Purshing_Id + ") ";
                    opp.insert_me(mysql);


                    if (Transaction_Type == 1)
                    {

                        ////تعديل بيانات المخزن المنصرف إلية بنقص  الرصيد
                        //To_Store_Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + ItemNO + "  WHERE (id = " + Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                        ////تعديل بيانات المخزن المنصرف منة بزيادة الرصيد
                        //Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + " WHERE (id = " + To_Store_Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);



                    }
                    else
                    {

                        ////تعديل بيانات المخزن المنصرف إلية بنقص  الرصيد
                        //To_Store_Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + ItemNO + "  WHERE (id = " + To_Store_Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                        ////تعديل بيانات المخزن المنصرف منة بزيادة الرصيد
                        //Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);



                    }

                }
                #endregion
                */




                #region Edit Rest



                ////////

                mysql = " UPDATE Detailes_Purshing SET Balance_Id_From_Larg_Qnty = FLOOR((" + S_unit_Value + " * Balance_Id_From_Larg_Qnty + Balance_Id_From_Small_Qnty) / " + S_unit_Value + ") ,  ";
                mysql = mysql + " Balance_Id_From_Small_Qnty =FlOOR(((" + S_unit_Value + " * Balance_Id_From_Larg_Qnty + Balance_Id_From_Small_Qnty)   / " + S_unit_Value + " - FLOOR((" + S_unit_Value + " * Balance_Id_From_Larg_Qnty + Balance_Id_From_Small_Qnty) / " + S_unit_Value + ")) * " + S_unit_Value + " )";
                opp.insert_me(mysql);

                mysql = " UPDATE Detailes_Purshing SET Balance_Id_To_Larg_Qnty = FLOOR((" + S_unit_Value + " * Balance_Id_To_Larg_Qnty + Balance_Id_To_Small_Qnty) / " + S_unit_Value + ") ,  ";
                mysql = mysql + " Balance_Id_To_Small_Qnty = FLOOR(((" + S_unit_Value + " * Balance_Id_To_Larg_Qnty + Balance_Id_To_Small_Qnty)   / " + S_unit_Value + " - FLOOR((" + S_unit_Value + " * Balance_Id_To_Larg_Qnty + Balance_Id_To_Small_Qnty) / " + S_unit_Value + ")) * " + S_unit_Value + " )";
                opp.insert_me(mysql);


                #endregion


                #region Delete Order_Purshing Where all Item Deleted In Detailes_Purshing

                //Order_PurshingCLS orCLS = new Order_PurshingCLS();
                //Order_Purshing orr = orCLS.GetOrder_PurshingId(ref opp, Order_Purshing_Id);
                Detailes_PurshingCLS dpFoundCLS = new Detailes_PurshingCLS();
                List<Detailes_Purshing> dpFound = dpFoundCLS.GetDetailes_PurshingAsNoTrackingListToCheckFound(ref opp, Order_Purshing_Id);
                if (dpFound.Count == 0)
                {
                    mysql = "Delete From Order_Purshing Where Id=" + Order_Purshing_Id;
                    opp.insert_me(mysql);

                    //DataDictionaryLogerProcessCLS ddlpCLS = new DataDictionaryLogerProcessCLS(ref opp);
                    //ddlpCLS.AddNew(order_Purshing.Order_Type.Value, 3, order_Purshing.Id, order_Purshing.Order_Purshing_No,0, "");
                    ddlpCLS.AddNew(3, order_Purshing, "");
                    if (Order_Type == (int)StoreOrderType.SarfToEntag_10)
                    {
                        mysql = "DELETE FROM  Item_Raw_MaterialsFor_Entage WHERE(SubOrderTable_id =" + Order_Purshing_Id + ")";
                        opp.insert_me(mysql);
                    }


                    if (Order_Type == (int)StoreOrderType.SarfTahweel_1 | Order_Type == (int)StoreOrderType.MortagaSarfTahweel_6)             //التحويل
                    {
                        Order_PurshingCLS orCLS = new Order_PurshingCLS();
                        Order_Purshing orr = new Order_Purshing();
                        orr = orCLS.GetOrder_PurshingId(ref opp, MainOrderTable_id);
                        if (orr != null)
                        {
                            mysql = "Delete From Order_Purshing Where Id=" + MainOrderTable_id;
                            opp.insert_me(mysql);
                        }

                    }
                }




                #endregion

                //try
                //{
                #region Delete Item_Raw_MaterialsFor_Entage_Details


                mysql = @"SELECT        Item_Raw_MaterialsFor_Entage_List_Id FROM Item_Raw_MaterialsFor_Entage_Details 
WHERE(Item_Raw_MaterialsFor_Entage_Details.Detailes_Purshing_Id = " + Detailes_Purshing_Id + ")";
                long Item_Raw_MaterialsFor_Entage_List_Id = 0;
                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    Item_Raw_MaterialsFor_Entage_List_Id = long.Parse(dt.Rows[0]["Item_Raw_MaterialsFor_Entage_List_Id"].ToString());
                }
                mysql = "DELETE FROM Item_Raw_MaterialsFor_Entage_Details WHERE(Detailes_Purshing_Id =" + Detailes_Purshing_Id + ")";
                opp.insert_me(mysql);


                mysql = @"SELECT Item_Raw_MaterialsFor_Entage_List_Id FROM Item_Raw_MaterialsFor_Entage_Details 
WHERE(Item_Raw_MaterialsFor_Entage_Details.Item_Raw_MaterialsFor_Entage_List_Id = " + Item_Raw_MaterialsFor_Entage_List_Id + ")";

                DataTable dt_ = opp.return_DataTable(mysql);
                if (dt_.Rows.Count == 0)
                {
                    mysql = "Delete From Item_Raw_MaterialsFor_Entage_List Where Id=" + Item_Raw_MaterialsFor_Entage_List_Id;
                    opp.insert_me(mysql);
                }

                //}catch { }


                #endregion
            }
            catch (Exception e)
            {

                Excp = 1;

            }

        }

        public DataTable Get_Order_Items_list(ref int Excp, ref operations opp, long OrderTable_id)
        {
            string mysql;
            mysql = Get_QueryOrder();

            mysql = mysql + " WHERE (Detailes_Purshing.OrderTable_id = " + order_Purshing.Id + ")  ";


            return Get_Items_Data(ref Excp, ref opp, mysql);

        }
        string Get_QueryOrder()
        {

            string mysql = @"  SELECT    ' '  AS QntyStr ,     unit.u_name, unit.B_Unit, unit.S_unit, Unit_item.id, Unit_item.Item_id, Item.No3eboa, Unit_item.Store_id, Unit_item.Store_Type, Unit_item.Code, Unit_item.unit_id, Unit_item.Reserved_Balance_Larg_Qnty, 
                         Unit_item.Reserved_Balance_Small_Qnty, Unit_item.Big_unit, Unit_item.Small_unit, Unit_item.P_price, Unit_item.S_price, Unit_item.Specification, Unit_item.Upon_Request, Unit_item.Currency, Item.Item_Name, 
                         Unit_item.id AS Unit_itemTable_id, Unit_item.Item_No AS Balance_Now_Item_No, Unit_item.D_Qua AS Balance_Now_Small_Qnty, Unit_item.Full_qua AS Balance_Now_Larg_Qnty, 
                         Order_Purshing.Order_Purshing_No,Order_Purshing.NotesAll, Order_Purshing.Order_date, Order_Purshing.Order_Type, Order_Purshing.type_From, Order_Purshing.Id_From, Order_Purshing.Type_To, Order_Purshing.Id_To, 
                         Order_Purshing.Invoice_ID, Detailes_Purshing.Balance_Id_From_Larg_Qnty, Detailes_Purshing.Balance_Id_From_Small_Qnty, Detailes_Purshing.Balance_Id_From_Item_No, 
                         Detailes_Purshing.Balance_Id_To_Larg_Qnty, Detailes_Purshing.Balance_Id_To_Small_Qnty, Detailes_Purshing.Balance_Id_To_Item_No, Detailes_Purshing.Id AS Detailes_Purshing_Id, 
                         Detailes_Purshing.To_Store_Unit_Item_Table_Id, Detailes_Purshing.Unit_item_Table_Id, Detailes_Purshing.Notes_From, Detailes_Purshing.Notes_To, Detailes_Purshing.Notes, Detailes_Purshing.Qua_Befor, 
                         Detailes_Purshing.Part_qua_Befor, Detailes_Purshing.Item_No_Befor, Detailes_Purshing.Qua_After, Detailes_Purshing.Part_qua_After, Detailes_Purshing.Item_No_After, 
                         Detailes_Purshing.Item_No AS Detailes_Purshing_Item_No, Detailes_Purshing.Part_qua AS Detailes_Purshing_Part_qua, Detailes_Purshing.qua AS Detailes_Purshing_qua, unit.B_Unit_Value, unit.S_unit_Value, 
                         ROUND(Detailes_Purshing.qua + Detailes_Purshing.Part_qua / unit.S_unit_Value, 2) AS Qantity, ROUND(Detailes_Purshing.Qua_After + Detailes_Purshing.Part_qua_After / unit.S_unit_Value, 2) AS Quantity_After, 
                         ROUND(Detailes_Purshing.Qua_Befor + Detailes_Purshing.Part_qua_Befor / unit.S_unit_Value, 2) AS Quantity_Befor, ROUND((Detailes_Purshing.qua + Detailes_Purshing.Part_qua / unit.S_unit_Value) 
                         / Item.No3eboa, 2) AS Item_No_No3eboa, ROUND((Detailes_Purshing.Qua_After + Detailes_Purshing.Part_qua_After / unit.S_unit_Value) / Item.No3eboa, 2) AS Item_No_No3eboa_After, 
                         ROUND((Detailes_Purshing.Qua_Befor + Detailes_Purshing.Part_qua_Befor / unit.S_unit_Value) / Item.No3eboa, 2) AS Item_No_No3eboa_Befor, Detailes_Purshing.OrderTable_id, Detailes_Purshing.Machine_Id, 
                         Detailes_Purshing.qua AS Larg_Qnty, Detailes_Purshing.Part_qua AS Small_Qnty, Detailes_Purshing.Item_No, '' AS Order_Estlam_Sarf_Flage, '' AS QntyStr, '' AS Item_NoStr, '' AS QntyStr_After, 
                         '' AS Item_NoStr_After, '' AS QntyStr_Befor, '' AS Item_NoStr_Befor,0 AS  IS_Editable , Detailes_Purshing.Related_Detailes_Purshing_Id, Detailes_Purshing.Transaction_Type 
FROM            Item INNER JOIN
                         Unit_item ON Item.id = Unit_item.Item_id INNER JOIN
                         unit ON Unit_item.unit_id = unit.id INNER JOIN
                         Detailes_Purshing ON Unit_item.id = Detailes_Purshing.Unit_item_Table_Id INNER JOIN
                         Order_Purshing ON Detailes_Purshing.OrderTable_id = Order_Purshing.Id
            ";
            return mysql;
        }
    }
}
