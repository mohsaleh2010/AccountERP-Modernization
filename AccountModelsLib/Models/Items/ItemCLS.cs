using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using AccountModelsLib.Models.Items;
//using AccountModelsLib.DBModels;


using System.Xml.Linq;

namespace AccountModelsLib.Models
{
    public class ItemCLS
    {
        public ItemCLS()
        {
            if (opp == null)
            {
                opp = new operations();
            }
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }
        public ItemCLS(ref operations opp_)
        {
            opp = opp_;
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }
        public operations opp;//= new operations();
        public ItemCodeTaxNotUploadedCLS itcCls = new ItemCodeTaxNotUploadedCLS();
        public Unit_item um = new Unit_item();
        public Unit_Item_ExpireDate umEx = new Unit_Item_ExpireDate();
        public Items im = new Items();

        public void UpdateS_Price(ref int excp, ref string msg, Bills_Details umVM)
        {

            try
            {
                long Unit_Item_Id = umVM.Unit_Item_Table_Id ?? 0;
                long Item_Id = umVM.Item_Id ?? 0;

                //um = new Unit_item();
                //um = opp.db.Unit_item.Where(i => i.id == Unit_Item_Id).FirstOrDefault();

                im = new Items();
                im = opp.db.Item.Where(i => i.Id == Item_Id).FirstOrDefault();

                im.S_Price = umVM.S_price;
                im.S_price_LargeUnit = umVM.S_price_LargeUnit;
                im.Edit_User_Id = Common.User_Id;
                im.EditOperationDate = DateTime.Now;
                //um.S_price = umVM.S_price;
                //um.S_price_LargeUnit = umVM.S_price_LargeUnit;

                opp.db.Entry(im).State = EntityState.Modified;
                opp.db.SaveChanges();


                string mysql = "Update Unit_Item Set S_price_LargeUnit=" + umVM.S_price_LargeUnit + ", S_price=" + umVM.S_price + ", Edit_User_Id=" + Common.User_Id + ", EditOperationDate='" + operations.dateFormat(DateTime.Now) + "' Where Item_Id=" + Item_Id;
                opp.insert_me(mysql);
                //opp.db.Entry(um).State = EntityState.Modified;
                //opp.db.SaveChanges();
                excp = 0;
            }
            catch (Exception ee)
            {
                msg = ee.Message;
                excp = 1;
            }

        }
        public void UpdateItemPriceFromInvoice(ref int excp, ref string msg, Invoices_Details invD)
        {

            try
            {
                long Unit_Item_Id = invD.Unit_Item_Table_Id ?? 0;
                long Item_Id = invD.Item_Id ?? 0;


                im = new Items();
                im = opp.db.Item.Where(i => i.Id == Item_Id).FirstOrDefault();

                //im.S_Price = invD.S_price_LargeUnit;
                //im.S_price_LargeUnit = invD.S_price_LargeUnit;

                im.S_Price = invD.Larg_Unit_Price;
                im.S_price_LargeUnit = invD.Larg_Unit_Price;
                im.Edit_User_Id = Common.User_Id; im.EditOperationDate = DateTime.Now; // تدقيق التعديل

                opp.db.Entry(im).State = EntityState.Modified;
                opp.db.SaveChanges();

                string mysql = "Update Unit_Item Set S_price_LargeUnit=" + invD.Larg_Unit_Price + ", S_price=" + invD.Larg_Unit_Price + ", Edit_User_Id=" + Common.User_Id + ", EditOperationDate=GETDATE() Where Item_Id=" + Item_Id;
                opp.insert_me(mysql);
                excp = 0;
            }
            catch (Exception ee)
            {
                msg = ee.Message;
                excp = 1;
            }

        }
        public unit un = new unit();

        public List<UnitItemsModelView> UnitItemsVMList = new List<UnitItemsModelView>();
        public List<ItemsOrderViewModel> itmsOrderVMList = new List<ItemsOrderViewModel>();

        public List<Items> itmList = new List<Items>();
        public List<Unit_item> umList = new List<Unit_item>();


        public List<Unit_Item_ExpireDate> umExList = new List<Unit_Item_ExpireDate>();
        public void Add_Item(ref int Excp, ref string Msg, Items im, Unit_item um)
        {
            operations opp = new operations();
            Add_Item(ref Excp, ref Msg, ref opp, im, um);
        }


        public void Add_Item(ref int Excp, ref string Msg, ref operations opp, Items im, Unit_item um)
        {
            //صلاحية حذف بيانات الصنف  
            bool IsHavePermission = Common.IsHavePermission(Common.User_Id, 916, Common.Project_ID, Common.Branch_Id);
            if (IsHavePermission == false)
            {
                Excp = 916;
                Msg = "ليس لديك صلاحية إضافة صنف جديد";
                //MessageBox.Show("ليس لديك صلاحية إضافة صنف جديد");
                return;
            }
            Excp = 0;

            opp.BeginTransaction(2);

            Save_Item(ref Excp, ref Msg, ref opp, im, um);
            if (Excp == 0)
            {
                opp.Commit();
                Excp = 0;
                Msg = "";
            }
            else
            {
                opp.Rollback();
                Excp = 1;

            }

        }

        public void Save_Item(ref int Excp, ref string Msg, ref operations opp, Items im, Unit_item um)
        {

            try
            {
                if (Common.UseAutomaticItemCode == 1 | im.code == "0")
                {
                    im.code = GetNewItemCode(ref opp).ToString();
                    long InternalCode = 0;

                    bool tt = long.TryParse(im.code, out InternalCode);
                    im.InternalCode = InternalCode;
                    um.Code = im.code;
                }
                /////GETITEM CODE
                im.Active_flage = 1;
                um.active_flage = 1;
                opp.db.Item.Add(im);
                opp.db.SaveChanges();
                um.Item_Id = im.Id;
                opp.db.Unit_item.Add(um);
                opp.db.SaveChanges();

                if (im.Is_UploadToTaxPortal == 0)
                {
                    if (itcCls.itemCodeTaxNotUploaded != null)
                    {
                        itcCls.itemCodeTaxNotUploaded.Item_Id = im.Id;
                        itcCls.AddItemCodeTaxNotUploaded(ref opp);
                    }
                }

                UpdateItemCode(ref opp, im, um);


                SetItem_First_Period_Balance(ref opp, im, um);

                if (Common.IsAddAllItemInAllStores == 1)
                {
                    ////إضافة الصنف فى كل المخازن اذا كان من منتجات المصنع
                    SetItemOnAllStore(ref opp, im, um);
                    //////////////////////////
                    ///
                }
                // SetItem_First_Period_Detailes_Purshing(ref opp, im, um);
                if (Common.IsAddAllItemForAllCars == 1)
                {
                    ItemsForCarsCLS itemsForCarsCLS = new ItemsForCarsCLS(ref opp);
                    itemsForCarsCLS.AddItemsForCarWhenAddItems(im);
                }
                Excp = 0;
                Msg = "";
            }
            catch (Exception ee)
            {


                Excp = 1;
                Msg = ee.Message;
            }
        }

        public long GetNewItemCode(ref operations opp)
        {
            long InternalCode = 0;
            Items itm = opp.db.Item.AsNoTracking().OrderByDescending(i => i.Id).Take(1).FirstOrDefault();
            if (Common.Use_Auto_Identity_Column == 1)
            {
                
                
                if (itm == null)
                {
                    InternalCode = 1000;
                }
                else
                {
                    if (itm.Id == null)
                    {
                        InternalCode = 1000;
                    }
                    else
                    {
                        InternalCode = itm.Id + 1;
                    }
                }
            }
            else
            {
                if (itm == null)
                {
                    InternalCode = 1000;
                }
                else
                {
                    if (itm.Id == null)
                    {
                        InternalCode = 1000;
                    }
                    else
                    {
                        long Code = 0;
                        long.TryParse(itm.code, out Code);
                        if (Code == 0)
                        {
                            Code = 1000;
                        }
                        InternalCode = Code + 1;
                    }
                }
            }
            return InternalCode;
        }
        void UpdateItemCode(ref operations opp, Items im, Unit_item um)
        {
            string mysql = "";
            if (Common.UseAutomaticItemCode == 1 | im.code == "0")
            {
                long Code = 0;
                if (Common.Use_Auto_Identity_Column == 1)
                {
                    Code = im.Id;
                }
                else
                {
                    long.TryParse(im.code, out Code);
                    if (Code == 0)
                    {
                        Code = GetNewItemCode(ref opp);
                    }
                }

                mysql = " Update  Item Set   Code= " + Code;
                mysql = mysql + " where Id=" + Code + " ";

                opp.insert_me(mysql);

                mysql = " Update Unit_Item Set  Code= " + Code;
                mysql = mysql + " where  Item_id=" + Code;
                opp.insert_me(mysql);

                //mysql = mysql + " where Id=" + im.id + " ";


                //mysql = " Update Unit_Item Set  Code= InternalCode ";
                //mysql = mysql + " where  id=" + um.id + "  ";
                //opp.insert_me(mysql);

                //mysql = " Update  Item Set   Code= InternalCode ";
                //mysql = mysql + " where Id=" + im.id + " ";
                //opp.insert_me(mysql);
                //mysql = " Update Unit_Item Set  Code= Item_Id ";
                //mysql = mysql + " where  id=" + um.id + "  ";
                //opp.insert_me(mysql);

                //mysql = " Update  Item Set   Code= Id ";
                //mysql = mysql + " where Id=" + im.id + " ";
                //opp.insert_me(mysql);
            }
            if (Common.UseAutomaticItemCode == 10)
            {



                string Code = im.Items_TypesDegreeFarz_NameId.ToString() +
                                 im.Items_TypesColor_NameId.ToString() +
                                 im.Items_TypesSize_NameId.ToString() +
                                 im.Items_TypesFactory_NameId.ToString() +
                                 im.Items_TypesDescriptionNameOtherId.ToString() +
                                 im.Items_TypesDescription_NameId.ToString() +
                                 im.Items_Types_Name_Id.ToString();

                mysql = " Update Unit_Item Set  Code= ' " + Code + "'";
                mysql = mysql + " where  id=" + um.id + "  ";
                opp.insert_me(mysql);

                mysql = " Update  Item Set   Code= ' " + Code + "'";
                mysql = mysql + " where Id=" + im.Id + " ";
                opp.insert_me(mysql);
            }

        }
        public void Save_Item(ref int Excp, ref string Msg, UnitItemsModelView umVM)
        {//صلاحية تعديل بيانات الصنف  
            bool IsHavePermission = Common.IsHavePermission(Common.User_Id, 917, Common.Project_ID, Common.Branch_Id);
            if (IsHavePermission == false)
            {
                Excp = 917;
                //MessageBox.Show("ليس لديك صلاحية تعديل بيانات الصنف");
                return;
            }
            operations opp = new operations(2);
            //opp.BeginTransaction();




            try
            {
                string mysql = "Update Unit_item Set Full_qua=" + umVM.Full_qua + " , D_Qua = " + umVM.D_Qua + ", Item_No=" + umVM.Item_No + ",S_Price=" + umVM.S_price + ",P_Price=" + umVM.P_price + " Where Item_Id=" + umVM.Item_Id;
                opp.insert_me(mysql);

                mysql = "Update Item Set ItemCode ='" + umVM.ItemCode + "',Item_Category_ID=" + umVM.Item_Category_Id + ", S_Price=" + umVM.S_price + ",P_Price=" + umVM.P_price + " Where  Id=" + umVM.Item_Id;
                opp.insert_me(mysql);
                opp.Commit();
                Excp = 0;
            }
            catch (Exception ee)
            {

                opp.Rollback();
                Excp = 1;
                Msg = ee.Message;
            }
        }

        void SetItemOnAllStore(ref operations opp, Items im, Unit_item um)
        {
            string mysql = "";
            int StoreUsed_Type = 1;
            DataTable dt = new DataTable();

            StoreCLS st = new StoreCLS();

            //mysql = "Select * from Store Where  id <> " + im.S_id  + " AND StoreUsed_Type=" + StoreUsed_Type + " And IS_ItemSecond_degree=0";
            dt = st.GetAllStoresWithout(im.S_id, StoreUsed_Type);

            for (int x = 0; x < dt.Rows.Count; x++)
            {
                long Store_id_All = int.Parse(dt.Rows[x]["Id"].ToString());

                mysql = "INSERT INTO Unit_item (S_price_LargeUnit,S_price_SmallUnit  ,P_price_LargeUnit  , P_price_SmallUnit  , Unit_Used_For_S_price_Default,Unit_Used_For_P_price_Default,Is_ItemWithoutBalance,Item_id, Store_id, Store_Type, Code, unit_id, Full_qua, D_Qua, Item_No, Reserved_Balance_Larg_Qnty, Reserved_Balance_Small_Qnty, Big_unit,Small_unit, P_price, S_price, Upon_Request,TaxSales10_Value)";
                mysql = mysql + "VALUES(" + um.S_price_LargeUnit + "," + um.S_price_SmallUnit + " ," + um.P_price_LargeUnit + " , " + um.P_price_SmallUnit + " ," + um.Unit_Used_For_S_price_Default + ", " + um.Unit_Used_For_P_price_Default + "," + um.Is_ItemWithoutBalance + "," + im.Id + ", " + Store_id_All + ",4,'" + um.Code + "', " + um.unit_id + ", 0,0,0,0, 0, " + um.Big_unit + "," + um.Small_unit + "," + um.P_price + "," + um.S_price + ",0," + um.TaxSales10_Value + ")";
                long Unit_Item_Table_Id = opp.Insert_me_Return_Last_Identify(mysql);

                SetItem_First_Period_Balance(ref opp, im, um, Unit_Item_Table_Id);

            }

            ////إضافة الصنف فى كل المخازن اذا كان من منتجات المصنع

        }
        public void SetItemIsMasna3iatKhamat()
        {
            operations opp = new operations();
            SetItemIsMasna3iatKhamat(ref opp);
        }

        public void SetItemIsMasna3iatKhamat(ref operations opp)
        {

            Items im = new Items();
            Unit_item um = new Unit_item();

            List<Unit_item> umList = opp.db.Unit_item.AsNoTracking().Where(i => i.Item.Item_Category_ID == (long)ItemCategoryEnum.Masnaheaiat).Include(i => i.Item).ToList();

            if (umList.Count > 0)
            {
                um = umList[0];
                um.id = 0;

                im = umList[0].Item;
                im.Id = 0;
                im.IsMasna3iatKhamat = 1;

                im.Item_Name = Common.ItemNameForMasna3iatKhamat;
                im.item_Name_ = Common.ItemNameForMasna3iatKhamat;
                im.ItemName_OnInvoice = Common.ItemNameForMasna3iatKhamat;
                im.Is_NotShowOnInvoice = 1;
                im.AnotherName = Common.ItemNameForMasna3iatKhamat;
            }

            opp.db.Item.Add(im);
            opp.db.SaveChanges();

            um.Item_Id = im.Id;

            opp.db.Unit_item.Add(um);
            opp.db.SaveChanges();
        }
        void SetItem_First_Period_Balance(ref operations opp, Items im, Unit_item um)
        {
            um.S_priceAvgCost = um.S_priceAvgCost ?? 0;
            um.P_price = um.P_price ?? 0;
            um.S_price = um.S_price ?? 0;
            um.P_priceAvgCost = um.P_priceAvgCost ?? 0;
            DateTime? Balance_Date = im.OperationDate;
            string mysql = "INSERT INTO Item_First_Period_Balance(Item_Id, Unit_Item_Id, Larg_Qnty, Small_Qnty, Item_No, Balance_Date,Active ,  P_priceAvgCost, S_priceAvgCost, P_price, S_price) ";
            mysql = mysql + " VALUES(" + im.Id + ", " + um.id + " , " + um.Full_qua + ", " + um.D_Qua + ", " + um.Item_No + ",'" + operations.dateFormat(Balance_Date) + "',1, " + um.P_priceAvgCost + ", " + um.S_priceAvgCost + ", " + um.P_price + ", " + um.S_price + ")";
            opp.insert_me(mysql);
        }

        void SetItem_First_Period_Detailes_Purshing(ref operations opp, Items im, Unit_item um)
        {

            DateTime? firstBalance_Date = im.OperationDate;
            string mysql = " INSERT INTO Order_Purshing (Order_Purshing_No, Order_Date, Order_Type, Type_From, Id_From, Type_To, Id_To, Invoice_ID) ";
            mysql = mysql + " VALUES   (-1,'" + operations.dateFormat(firstBalance_Date) + "',-1,-1,-1,-1,-1,-1) ";

            long OrderTable_id = opp.Insert_me_Return_Last_Identify(mysql);
            string Notes_From = "رصيد أول المدة";
            mysql = " INSERT INTO Detailes_Purshing (OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Balance_Id_To_Larg_Qnty, Balance_Id_TO_Small_Qnty,Balance_Id_TO_Item_No,Balance_Id_From_Larg_Qnty,Balance_Id_From_Small_Qnty,Balance_Id_From_Item_No,To_Store_Unit_Item_Table_Id,Notes_From,Notes_To) ";
            mysql = mysql + "  VALUES (" + OrderTable_id + "," + um.id + "," + um.Full_qua + "," + um.D_Qua + "," + um.Item_No + ",0,0,0," + um.Full_qua + "," + um.D_Qua + "," + um.Item_No + ",0,'" + global::AccountModelsLib.SqlText.Q(Notes_From) + "','" + global::AccountModelsLib.SqlText.Q(Notes_From) + "')";

        }

        void SetItem_First_Period_Balance(ref operations opp, Items im, Unit_item um, long Unit_Item_Table_Id)
        {

            DateTime Balance_Date = DateTime.Now.Date;
            string mysql = "INSERT INTO Item_First_Period_Balance(Item_Id, Unit_Item_Id, Larg_Qnty, Small_Qnty, Item_No, Balance_Date,Active ) ";
            mysql = mysql + " VALUES(" + im.Id + ", " + Unit_Item_Table_Id + " ,  0,0,0,'" + operations.dateFormat(Balance_Date) + "',1)";
            opp.insert_me(mysql);
        }
        void EditItemPrice(ref operations opp, ref Unit_item um)
        {
            string mysql = $@" Update Unit_Item Set P_price ={um.P_price} , 
            P_price_LargeUnit ={um.P_price_LargeUnit ?? 0} ,
            P_price_SmallUnit ={um.P_price_SmallUnit ?? 0} ,
            P_price_Item_No ={um.P_price_Item_No ?? 0} ,
            S_price ={um.S_price ?? 0} ,
            S_price_LargeUnit ={um.S_price_LargeUnit ?? 0} ,
            S_price_SmallUnit ={um.S_price_SmallUnit ?? 0} ,
            S_price_Item_No ={um.S_price_Item_No ?? 0} ,
            Edit_User_Id ={Common.User_Id} ,
            EditOperationDate ='{operations.dateFormat(DateTime.Now)}' ";

            mysql = mysql + $" Where Item_Id={um.Item_Id}";
            opp.insert_me(mysql);

        }
        public void Edit_Item(ref int Excp, ref string Msg, operations opp, Items im, Unit_item um)
        {

            Edit_Item(ref Excp, ref Msg, ref opp, im, um);
        }

        public void Edit_Item(ref int Excp, ref string Msg, ref operations opp, Items im, Unit_item um)
        {
            //صلاحية تعديل بيانات الصنف  
            bool IsHavePermission = Common.IsHavePermission(Common.User_Id, 917, Common.Project_ID, Common.Branch_Id);
            if (IsHavePermission == false)
            {
                Excp = 917;
                //MessageBox.Show("ليس لديك صلاحية تعديل بيانات الصنف");
                return;
            }
            string mysql = "";

            //int StoreUsed_Type = 1;

            opp.BeginTransaction(2);


            try
            {

                im.Edit_User_Id = Common.User_Id;
                im.EditOperationDate = DateTime.Now;
                um.Edit_User_Id = Common.User_Id;
                um.EditOperationDate = DateTime.Now;

                opp.db.Entry(im).State = EntityState.Modified;
                opp.db.Entry(um).State = EntityState.Modified;

                opp.db.SaveChanges();

                UpdateItemCode(ref opp, im, um);

                ItemsForCarsCLS itemsForCarsCLS = new ItemsForCarsCLS(ref opp);
                itemsForCarsCLS.AddItemsForCarWhenEditItems(im);

                if (itcCls.itemCodeTaxNotUploaded != null)
                {
                    itcCls.itemCodeTaxNotUploaded.Item_Id = im.Id;
                    itcCls.EditItemCodeTaxNotUploaded(ref opp);
                }

                EditItemPrice(ref opp, ref um);
                opp.Commit();
                Excp = 0;

            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
                opp.Rollback();
            }
            opp.db.Entry(im).State = EntityState.Detached;
            opp.db.Entry(um).State = EntityState.Detached;

            return;
            try
            {

                mysql = " Update item set No3eboa=" + im.No3eboa + ",Item_Category_ID=" + im.Item_Category_ID + ",  code='" + im.code + "',item_name='" + im.Item_Name + "', s_id=" + im.S_id + " , unit_Id=" + im.Unit_Id + ",UnitStr='" + im.UnitStr + "' where id=" + im.Id;
                opp.insert_me(mysql);

                mysql = " Update Unit_Item Set  Upon_Request=" + um.Upon_Request + " ,Full_qua=" + um.Full_qua + ",D_Qua=" + um.D_Qua + ",Item_No=" + um.Item_No + " ,TaxSales10_Value=" + um.TaxSales10_Value + "";
                mysql = mysql + " where  id=" + um.id + " AND Store_id=" + um.Store_id + " AND  Store_Type= 4";
                opp.insert_me(mysql);

                mysql = " Update Unit_Item Set  unit_id=" + um.unit_id + " ,Big_unit=" + um.Big_unit + ",Small_unit=" + um.Small_unit + " ,      P_price=" + um.P_price + ", S_price=" + um.S_price + ",  code='" + um.Code + "' ";
                mysql = mysql + " where Item_id=" + um.Item_Id + " AND Store_id=" + um.Store_id + " AND  Store_Type= 4";
                opp.insert_me(mysql);

                opp.Commit();
            }
            catch (Exception)
            {
                opp.Rollback();

            }
        }
        public void Edit_Item(ref int Excp, ref string Msg, Items im, Unit_item um)
        {
            operations opp = new operations();
            Edit_Item(ref Excp, ref Msg, ref opp, im, um);
            opp = null;

        }

        public void Edit_Item(ref int Excp, ref string Msg)
        {

            string mysql = "";

            int StoreUsed_Type = 1;
            operations opp = new operations(2);
            //My_DbContext.ConnStr = opp.str_sql_conn;
            //My_DbContext db = new My_DbContext(opp.conn,true);
            ////My_DbContext db = new My_DbContext();
            //opp.conn=(SqlConnection) db.Database.Connection;
            //db.Database.UseTransaction(opp.tns);
            //My_DbContext db = new My_DbContext(opp.conn, true);



            try
            {

                im.Edit_User_Id = Common.User_Id;
                im.EditOperationDate = DateTime.Now;
                um.Edit_User_Id = Common.User_Id;
                um.EditOperationDate = DateTime.Now;

                opp.db.Entry(im).State = EntityState.Modified;
                opp.db.Entry(um).State = EntityState.Modified;

                opp.db.SaveChanges();


                opp.Commit();
                Excp = 0;

            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
                opp.Rollback();
            }




        }
        // 🟢 دالة جديدة (تقنية فقط، لا تقابل أى استعلام أصلى حرفياً): استعلام مُبسَّط "أى صنف نشط" لعنصر اختيار
        // صنف عام فى شاشات إشعارات التسوية بالصنف (بند 16) - الأصل يستخدم Store_LIB.SearchUSR/Connection4It.
        // ItemsSearchAny (عناصر تحكم WinForms معقَّدة تدعم بحث متعدد المعايير عبر شاشات كثيرة خارج نطاق هذا
        // البند) وبناء نظير Blazor كامل لها خارج النطاق هنا؛ اكتُفِى بحد أدنى كافٍ لهذه الشاشة تحديداً (بحث
        // بالاسم فقط على الأصناف النشطة) - مُوثَّق صراحة هنا لأنه الاستثناء الوحيد فى هذا البند لمبدأ "نفس
        // الاستعلام الأصلى حرفياً" المُتبَع فى كل مكان آخر.
        public DataTable Get_ActiveItemsForPicker()
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            string mysql = " SELECT Id, Item_Name FROM Item WHERE Active_flage=1 ORDER BY Item_Name";
            return opp.return_DataTable(mysql);
        }
        public bool IsFoundtemByCodeToEdit(string Code, ref DataTable dt, long Item_Id = 0)
        {
            bool IsFound = false;

            dt = GetItemByCode(Code, Item_Id);

            if (dt.Rows.Count > 0)
            {
                IsFound = true;
            }

            return IsFound;
        }
        public bool IsFoundtemByCode(string Code, ref DataTable dt)
        {
            bool IsFound = false;

            dt = GetItemByCode(Code);

            if (dt.Rows.Count > 0)
            {
                IsFound = true;
            }

            return IsFound;
        }
        public DataTable GetItemByCode(string Code, long Item_Id = 0)
        {
            bool IsFound = false;
            string mysql = "";
            int Excp = 0;
            operations opp = new operations();

            //mysql = " SELECT Item.id, Item.code, Item.Item_Name, Item.S_id, Unit_item.id AS Unit_item_id FROM Item INNER JOIN  Unit_item ON Item.id = Unit_item.Item_id ";
            //mysql = mysql + " Where  Item.code='" + Code + "' and Item.Active_flage=1";

            mysql = " SELECT id, code, Item_Name, S_id FROM Item ";
            mysql = mysql + " Where  code='" + Code + "' and Active_flage=1";
            if (Item_Id > 0)
            {
                mysql = mysql + " And  id <> " + Item_Id + "";
            }
            DataTable dt = new DataTable();
            dt = opp.return_DataTable(mysql);



            return dt;
        }

        public void GetItemDataByNameEF(ref int Excp, string Name)
        {

            try
            {
                Excp = 0;

                um = new Unit_item();
                im = new Items();
                un = new unit();
                umList = new List<Unit_item>();
                itmList = new List<Items>();
                umExList = new List<Unit_Item_ExpireDate>();


                //My_DbContext.ConnStr = opp.str_sql_conn;
                //My_DbContext db = new My_DbContext();

                //imList = opp.db.Item.Where(p => p.Item_Name.Contains(Name) || p.AnotherName.Contains(Name)).ToList();

                umList = opp.db.Unit_item.Include(i => i.Item).Where(i => i.Item.Item_Name.Contains(Name) || i.Item.AnotherName.Contains(Name)).OrderBy(i => i.Item.Item_Name).ToList();


                //Include(p=>p.Unit_item).Include(p=>p.Unit_Item_ExpireDate)
                //if (imList != null)
                //{
                //    umExList = db.Unit_item.Where(s => s.Item_id == imList.id).ToList();
                //}
                //if (imList != null)
                //{
                //    umExList = db.Unit_Item_ExpireDate.Where(s => s.Item_id == im.id).ToList();
                //}

                Excp = 0;


            }
            catch (Exception ee)
            {
                Excp = 1;


            }


        }
        public List<UnitItemsModelView> GetUnit_ItemBYItem_Id(ref int Excp, long Item_Id)
        {
            //List<ItemsModelView> ItemsModelView = new List<ItemsModelView>();
            try
            {
                Excp = 0;

                //um = new Unit_item();
                //im = new Items();
                //un = new unit();
                //umList = new List<Unit_item>();
                //imList = new List<Items>();
                //umExList = new List<Unit_Item_ExpireDate>();



                umList = opp.db.Unit_item.Where(i => i.Item_Id == Item_Id).Include(i => i.Item).Include(s => s.Store).Include(u => u.unit).OrderBy(i => i.Item.Item_Name).ToList();

                UnitItemsVMList = GetStoresItemsModelView(umList);
                Excp = 0;


            }
            catch (Exception ee)
            {
                operations.DataDictionaryLogerException(ee);
                Excp = 1;


            }
            return UnitItemsVMList;


        }
        public void SetPageSizeData()
        {
            PageSize = 50;
            PageNo = 1;
            TotalCount = 3000;
            TotalPage = 0;
            Serial = 1;
        }
        public void ResetPageSizeData()
        {
            PageSize = -1;
            PageNo = 1;
            TotalCount = -1;
            TotalPage = 0;
            Serial = 1;
        }
        public int PageSize { get; set; } = -1;
        public int PageNo { get; set; } = 1;
        public int TotalCount = -1;
        public int TotalPage = 0;
        public int Serial = 1;
        IQueryable<Unit_item> GetUnit_ItemBYItemOnlyItems_Q(ref operations opp, long StoreId, string ItemsNameSearch, long ItemCategory_Id, int Active_flage)
        {

            IQueryable<Unit_item> query = opp.db.Unit_item.AsNoTracking().Include(i => i.Item).Include(s => s.Store).Include(u => u.unit).OrderBy(i => i.Item.Item_Name);
            query = query.Where(i => i.active_flage == Active_flage);
            if (StoreId > 0)
            {
                query = query.Where(i => i.Store_id == StoreId);
            }

            if (ItemCategory_Id > 0)
            {
                query = query.Where(i => i.Item.Item_Category_ID == ItemCategory_Id);
            }
            if (ItemsNameSearch.Trim() != "")
            {
                query = query.Where(i => i.Item.Item_Name.Contains(ItemsNameSearch) | i.Item.ItemName_OnInvoice.Contains(ItemsNameSearch));
            }


            return query;

        }
        public int GetUnit_ItemBYItemOnlyItemsCount(ref operations opp, long StoreId, string ItemsNameSearch, long ItemCategory_Id, int Active_flage)
        {
            PageNo = 1;
            IQueryable<Unit_item> query = GetUnit_ItemBYItemOnlyItems_Q(ref opp, StoreId, ItemsNameSearch, ItemCategory_Id, Active_flage);
            TotalCount = query.Count();

            return TotalCount;
        }
        public List<UnitItemsModelView> GetUnit_ItemBYItemOnlyItems_SetPaging(ref operations opp, long StoreId, string ItemsNameSearch, long ItemCategory_Id, int Active_flage)
        {
            if (PageSize > -1)
            {
                return GetUnit_ItemBYItemOnlyItems_Paging(ref opp, StoreId, ItemsNameSearch, ItemCategory_Id, Active_flage);
            }
            else
            {
                return GetUnit_ItemBYItemOnlyItems(ref opp, StoreId, ItemsNameSearch, ItemCategory_Id, Active_flage);

            }
        }
        public List<UnitItemsModelView> GetUnit_ItemBYItemOnlyItems_Paging(ref operations opp, long StoreId, string ItemsNameSearch, long ItemCategory_Id, int Active_flage)
        {

            IQueryable<Unit_item> query = GetUnit_ItemBYItemOnlyItems_Q(ref opp, StoreId, ItemsNameSearch, ItemCategory_Id, Active_flage);


            query = query.Skip((PageNo - 1) * PageSize);
            query = query.Take(PageSize);
            UnitItemsVMList = GetStoresItemsModelView(query.ToList());

            return UnitItemsVMList;

        }
        public List<UnitItemsModelView> GetUnit_ItemBYItemOnlyItems(ref operations opp, long StoreId, string ItemsNameSearch, long ItemCategory_Id, int Active_flage)
        {

            IQueryable<Unit_item> query = GetUnit_ItemBYItemOnlyItems_Q(ref opp, StoreId, ItemsNameSearch, ItemCategory_Id, Active_flage);


            //query = query.Skip((PageNo - 1) * PageSize);
            //query = query.Take(PageSize);
            UnitItemsVMList = GetStoresItemsModelView(query.ToList());

            return UnitItemsVMList;

        }
        public List<UnitItemsModelView> GetUnit_ItemBYIsMasna3iatKhamat(ref int Excp, ref operations opp)
        {
            //List<ItemsModelView> ItemsModelView = new List<ItemsModelView>();
            try
            {
                Excp = 0;

                umList = opp.db.Unit_item.Where(i => i.Item.IsMasna3iatKhamat == 1).Include(i => i.Item).Include(s => s.Store).Include(u => u.unit).OrderBy(i => i.Item.Item_Name).ToList();

                UnitItemsVMList = GetStoresItemsModelView(umList);
                Excp = 0;


            }
            catch (Exception ee)
            {
                operations.DataDictionaryLogerException(ee);
                Excp = 1;


            }
            return UnitItemsVMList;


        }
        public List<UnitItemsModelView> GetUnit_ItemBYItem_Id(ref int Excp, long Item_Id, long StoreId)
        {
            //List<ItemsModelView> ItemsModelView = new List<ItemsModelView>();
            try
            {
                Excp = 0;


                umList = opp.db.Unit_item.AsNoTracking().Where(i => i.Item_Id == Item_Id & i.Store_id == StoreId).Include(i => i.Item).Include(s => s.Store).Include(u => u.unit).OrderBy(i => i.Item.Item_Name).ToList();
                if (umList.Count == 0)
                {
                    SetAllItemOnStore(Item_Id, StoreId);
                    UnitItemsVMList = GetUnit_ItemBYItem_Id(ref Excp, Item_Id, StoreId);
                    return UnitItemsVMList;
                }
                UnitItemsVMList = GetStoresItemsModelView(umList);
                Excp = 0;


            }
            catch (Exception ee)
            {
                Excp = 1;


            }
            return UnitItemsVMList;


        }

        public UnitItemsModelView GetUnit_ItemBYUnit_ItemId(ref int Excp, long Unit_item_Id)
        {
            UnitItemsModelView umV = new UnitItemsModelView();
            try
            {
                Excp = 0;
                Unit_item um = opp.db.Unit_item.AsNoTracking().Where(i => i.id == Unit_item_Id).Include(i => i.Item).Include(s => s.Store).Include(u => u.unit).FirstOrDefault();
                if (um != null)
                {
                    umV = GetUnitItemsModelView(um);
                }

                Excp = 0;


            }
            catch (Exception ee)
            {
                Excp = 1;


            }
            return umV;


        }

        public void SetAllItemOnStore(long Item_Id, long StoreId)
        {
            //Items im = opp.db.Item.Where(p => p.id == Item_Id).FirstOrDefault();
            Unit_item umNCheck = opp.db.Unit_item.AsNoTracking().Where(p => p.Item_Id == Item_Id & p.Store_id == StoreId).FirstOrDefault();
            if (umNCheck != null)
            {
                return;
            }

            Unit_item umN = opp.db.Unit_item.AsNoTracking().Where(p => p.Item_Id == Item_Id).FirstOrDefault();
            if (umN != null)
            {
                Unit_item um = new Unit_item();
                um.Item_Id = Item_Id;
                um.Store_id = StoreId;

                um.Store_Type = umN.Store_Type;
                um.Code = umN.Code;
                um.unit_id = umN.unit_id;
                um.Full_qua = 0;
                um.D_Qua = 0;
                um.Item_No = 0;
                um.Reserved_Balance_Larg_Qnty = 0;
                um.Reserved_Balance_Small_Qnty = 0;
                um.Big_unit = umN.Big_unit; ;
                um.Small_unit = umN.Small_unit;
                um.P_price = im.P_Price;
                um.S_price = im.S_Price;
                um.Upon_Request = 1;
                um.active_flage = 1;


                opp.db.Unit_item.Add(um);
                opp.db.SaveChanges();

            }
        }
        string GetItemCategory(long Item_Category_ID)
        {
            if (opp == null)
            {
                opp = new operations();
            }
            string Category_Name = "";
            Item_Category itmCat = opp.db.item_Category.AsNoTracking().Where(i => i.ID == Item_Category_ID).FirstOrDefault();
            if (itmCat != null)
            {
                Category_Name = itmCat.Category_Name;
            }
            return Category_Name;
        }
        public List<UnitItemsModelView> GetItemsModelView(List<Unit_item> umList)
        {
            UnitItemsVMList = new List<UnitItemsModelView>();
            //imList = new List<Items>();

            foreach (var s in umList)
            {
                UnitItemsModelView iv = new UnitItemsModelView();
                iv.Item_Name = s.Item.Item_Name;
                iv.AnotherName = s.Item.AnotherName;
                iv.Item_Category_Id = s.Item.Item_Category_ID;
                try
                {
                    iv.Item_Category_Name = GetItemCategory(s.Item.Item_Category_ID ?? 0);
                }
                catch (Exception)
                {
                }
                iv.Code = s.Item.code;
                iv.BarCode2 = s.Item.BarCode2;
                iv.BarCode3 = s.Item.BarCode3;
                iv.BarCode4 = s.Item.BarCode4;
                iv.BarCode5 = s.Item.BarCode5;
                iv.Item_Id = (long)s.Item_Id;
                iv.Unit_Item_Id = s.id;
                iv.Upon_Request = s.Upon_Request;
                iv.S_price = s.S_price;
                iv.P_price = s.P_price;
                iv.S_priceAvgCost = s.S_priceAvgCost;
                iv.P_priceAvgCost = s.P_priceAvgCost;

                iv.BlockNo = s.Item.BlockNo;
                iv.shelfNo = s.Item.shelfNo;
                iv.OrderOnshelfNo = s.Item.OrderOnshelfNo;
                iv.Small_unit = s.Small_unit;


                UnitItemsVMList.Add(iv);
            }
            //var um_List = from p in umList
            //              select new
            //              {
            //                  Unit_Item_Id = p.id,
            //                  Item_Id = p.Item_Id,
            //                  Item_Name = p.Item.Item_Name,
            //                  AnotherName = p.Item.AnotherName

            //              }; 
            //ItemsViewList = (List<ItemsModelView>)um_List ;

            return UnitItemsVMList;
        }

        public List<UnitItemsModelView> GetStoresItemsModelView(List<Unit_item> umList)
        {
            UnitItemsVMList = new List<UnitItemsModelView>();
            //imList = new List<Items>();
            Serial = Serial * PageNo;
            foreach (var s in umList)
            {
                UnitItemsModelView iv = GetUnitItemsModelView(s);
                //iv.Serial = Serial;
                //Serial++;
                UnitItemsVMList.Add(iv);
            }


            return UnitItemsVMList;
        }

        public UnitItemsModelView GetUnitItemsModelView(Unit_item s)
        {

            //imList = new List<Items>();
            Serial = Serial * PageNo;

            UnitItemsModelView iv = new UnitItemsModelView();
            iv.Serial = Serial;
            Serial++;
            iv.Item_Name = s.Item.Item_Name;
            iv.AnotherName = s.Item.AnotherName;
            iv.ItemCode = s.Item.ItemCode;
            if (s.Item.ItemName_OnInvoice != null)
            {
                if (s.Item.ItemName_OnInvoice.Trim() == "")
                {
                    iv.ItemName_OnInvoice = s.Item.Item_Name;
                }
                else
                {
                    iv.ItemName_OnInvoice = s.Item.ItemName_OnInvoice;
                }
            }
            else
            {
                iv.ItemName_OnInvoice = s.Item.Item_Name;
            }
            if (s.unit != null)
            {
                iv.UnitStr = s.unit.B_Unit;
            }
            else
            {
                iv.UnitStr = "";
            }
            iv.Item_Category_Id = s.Item.Item_Category_ID;
            try
            {
                iv.Item_Category_Name = GetItemCategory(s.Item.Item_Category_ID ?? 0);
            }
            catch (Exception)
            {
            }

            if (s.Item != null)
            {
                iv.S_price = s.Item.S_Price;
                iv.P_price = s.Item.P_Price;

                iv.S_priceAvgCost = s.Item.S_priceAvgCost;
                iv.P_priceAvgCost = s.Item.P_priceAvgCost;
            }
            else
            {


                iv.S_priceAvgCost = s.S_priceAvgCost;
                iv.P_priceAvgCost = s.P_priceAvgCost;
            }
            iv.S_price = s.S_price;
            iv.P_price = s.P_price;

            iv.No3eboa = s.Item.No3eboa;
            iv.Code = s.Item.code;
            iv.Item_Id = (long)s.Item_Id;
            iv.Unit_Item_Id = s.id;
            iv.Upon_Request = s.Upon_Request;
            iv.Store_id = s.Store_id;




            iv.BlockNo = s.Item.BlockNo;
            iv.shelfNo = s.Item.shelfNo;
            iv.OrderOnshelfNo = s.Item.OrderOnshelfNo;

            iv.Items_Types_Name = s.Item.Items_Types_Name;
            iv.Somk = s.Item.Somk;
            iv.Somk_Unit = s.Item.Somk_Unit;
            //iv.QntyLenght = s.Item.QntyLenght;
            //iv.QntyWidth = s.Item.QntyWidth;
            //iv.QntyItemNo = s.Item.QntyItemNo;


            iv.Small_unit = s.Small_unit;
            iv.Full_qua = s.Full_qua;
            iv.D_Qua = s.D_Qua;
            iv.Item_No = s.Item_No;
            iv.Specification = s.Specification;
            iv.Unit_Used_For_P_price_Default = s.Unit_Used_For_P_price_Default;
            iv.Unit_Used_For_S_price_Default = s.Unit_Used_For_S_price_Default;


            if (s.Store != null)
            {
                iv.Store_Name = s.Store.Store_Name;
            }
            iv.Unit_Id = s.unit_id;
            if (s.unit != null)
            {
                iv.B_Unit = s.unit.B_Unit;
                iv.S_unit = s.unit.S_unit;
                iv.Small_unit = s.unit.S_unit_Value;
            }
            else
            {
                iv.B_Unit = "";
                iv.S_unit = "";
                iv.Small_unit = 1;
            }

            iv.QnyStr = Common.GetQntyString(iv.Full_qua.ToString(), iv.B_Unit, iv.D_Qua.ToString(), iv.S_unit);

            //UnitItemsVMList.Add(iv);



            return iv;
        }
        public void GetItemByCodeEF(ref int Excp, string Code)
        {

            try
            {
                Excp = 0;

                um = new Unit_item();
                im = new Items();
                un = new unit();


                //My_DbContext.ConnStr = opp.str_sql_conn;
                //My_DbContext db = new My_DbContext();

                im = opp.db.Item.Where(p => p.code == Code | p.BarCode2 == Code | p.BarCode3 == Code | p.BarCode4 == Code | p.BarCode5 == Code).FirstOrDefault();
                if (im != null)
                {
                    umExList = opp.db.Unit_Item_ExpireDates.Where(s => s.Item_Id == im.Id & (s.Larg_Balance > 0 | s.Small_Balance > 0)).OrderBy(o => o.Item_Expire_Date).ToList();
                }
                if (im != null)
                {
                    long Id = im.Id;

                    um = opp.db.Unit_item.Where(s => s.Item_Id == Id).Include(i => i.Item).FirstOrDefault();
                    itmList.Add(im);
                    if (um != null)
                    {
                        umList.Add(um);
                    }
                }

                string mysql = "Update Unit_Item_ExpireDate Set Larg_Balance= 0 WHere Larg_Balance <0 ";
                mysql = mysql + "Update Unit_Item_ExpireDate Set Small_Balance= 0 WHere Small_Balance <0 ";
                opp.insert_me(mysql);
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
            }


        }
        public void GetItemByUnit_Item_Id(ref int Excp, long? Unit_Item_Table_Id)
        {

            try
            {
                Excp = 0;

                um = new Unit_item();
                im = new Items();
                un = new unit();

                long Id = im.Id;

                um = opp.db.Unit_item.Where(s => s.id == Unit_Item_Table_Id).Include(i => i.Item).FirstOrDefault();

                if (um != null)
                {
                    im = um.Item;
                    umExList = opp.db.Unit_Item_ExpireDates.Where(s => s.Item_Id == im.Id).OrderBy(o => o.Item_Expire_Date).ToList();

                    itmList.Add(im);
                    if (um != null)
                    {
                        umList.Add(um);
                    }

                }


                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
            }


        }
        public void GetExpireItemByUnitItemIdEF(ref int Excp, long? Unit_Item_Table_Id)
        {

            try
            {
                Excp = 0;


                umExList = new List<Unit_Item_ExpireDate>();

                //operations opp = new operations();
                //My_DbContext.ConnStr = opp.str_sql_conn;
                //My_DbContext db = new My_DbContext();



                umExList = opp.db.Unit_Item_ExpireDates.Where(s => s.Unit_Item_Table_Id == Unit_Item_Table_Id & (s.Larg_Balance > 0 | s.Small_Balance > 0)).OrderBy(a => a.Item_Expire_Date).ToList();

                Excp = 0;


            }
            catch (Exception ee)
            {
                Excp = 1;


            }


        }
        public void GetExpireItemByItemIdEF(ref int Excp, long? Item_Id)
        {

            try
            {
                Excp = 0;


                umExList = new List<Unit_Item_ExpireDate>();

                operations opp = new operations();
                My_DbContext.ConnStr = opp.str_sql_conn;
                My_DbContext db = new My_DbContext();



                umExList = db.Unit_Item_ExpireDates.Where(s => s.Item_Id == Item_Id).ToList();

                Excp = 0;


            }
            catch (Exception ee)
            {
                Excp = 1;


            }


        }
        public void GetItemAsNoTrackingByUnit_item_Id(ref int Excp, ref string Msg, long Unit_item_Table_Id)
        {
            try
            {
                Excp = 0;

                um = new Unit_item();
                im = new Items();
                un = new unit();


                //operations opp = new operations();

                // um = db.Unit_item.Find(Unit_item_Table_Id);
                um = opp.db.Unit_item.AsNoTracking().Where(s => s.id == Unit_item_Table_Id).FirstOrDefault();
                if (um != null)
                {
                    im = opp.db.Item.AsNoTracking().Where(p => p.Id == um.Item_Id).FirstOrDefault();
                }
                if (um != null)
                {
                    un = opp.db.unit.AsNoTracking().Where(p => p.id == um.unit_id).FirstOrDefault();

                }
                if (um != null)
                {
                    itcCls.itemCodeTaxNotUploaded = opp.db.ItemCodeTaxNotUploaded.AsNoTracking().Where(p => p.Item_Id == um.Item_Id).FirstOrDefault();

                }

                Excp = 0;


            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;

            }



        }

        public void GetItemByUnit_item_Id(ref int Excp, ref string Msg, long Unit_item_Table_Id)
        {
            try
            {
                Excp = 0;

                um = new Unit_item();
                im = new Items();
                un = new unit();


                //operations opp = new operations();

                // um = db.Unit_item.Find(Unit_item_Table_Id);
                um = opp.db.Unit_item.Where(s => s.id == Unit_item_Table_Id).FirstOrDefault();
                if (um != null)
                {
                    im = opp.db.Item.Where(p => p.Id == um.Item_Id).FirstOrDefault();
                }
                if (um != null)
                {
                    un = opp.db.unit.Where(p => p.id == um.unit_id).FirstOrDefault();

                }
                if (um != null)
                {
                    itcCls.itemCodeTaxNotUploaded = opp.db.ItemCodeTaxNotUploaded.Where(p => p.Item_Id == um.Item_Id).FirstOrDefault();

                }

                Excp = 0;


            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;

            }



        }
        public List<Items> GetItemsStore(long StoreId)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            List<Items> itmsLst = opp.db.Item.AsNoTracking().Where(i => i.S_id == StoreId).ToList();
            return itmsLst;
        }
        public void GetAllItems(ref int Excp, ref string Msg, long ItemCategoryId)
        {
            try
            {
                Excp = 0;


                im = new Items();
                IQueryable<Items> query = opp.db.Item.AsNoTracking().Include(i => i.Item_Category).Include(s => s.Store).Include(u => u.unit);

                if (ItemCategoryId > 0)
                {
                    query = query.Where(i => i.Item_Category_ID == ItemCategoryId);
                }
                itmList = query.ToList();
                Set_ItemsViewModel();
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;

            }
        }
        public List<ItemsOrderViewModel> GetAllItemsViewModel(ref int Excp, ref string Msg, long ItemCategoryId)
        {
            try
            {
                Excp = 0;

                itmsOrderVMList = new List<ItemsOrderViewModel>();
                GetAllItems(ref Excp, ref Msg, ItemCategoryId);


                Excp = 0;

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;

            }
            return itmsOrderVMList;
        }
        public void GetAllItemsByNameSearch(ref int Excp, ref string Msg, string Item_Name, long ItemCategoryId)
        {
            try
            {
                Excp = 0;


                im = new Items();
                IQueryable<Items> query = opp.db.Item.Include(i => i.Item_Category).Include(s => s.Store).Include(u => u.unit);

                if (ItemCategoryId > 0)
                {
                    query = query.Where(i => i.Item_Category_ID == ItemCategoryId);
                }
                if (Item_Name.Trim() != "")
                {
                    query = query.Where(i => i.Item_Name.Contains(Item_Name.Trim()));
                }

                itmList = query.ToList();
                Set_ItemsViewModel();
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;

            }
        }
        public List<Detailes_Purshing> GetAllItemsByNameTo_DB(ref int Excp, ref string Msg, string Item_Name, long ItemCategoryId)
        {
            List<Detailes_Purshing> dpList = new List<Detailes_Purshing>();
            try
            {
                Excp = 0;


                im = new Items();
                IQueryable<Items> query = opp.db.Item.AsNoTracking().Include(i => i.Item_Category).Include(s => s.Store).Include(u => u.unit);

                if (ItemCategoryId > 0)
                {
                    query = query.Where(i => i.Item_Category_ID == ItemCategoryId);
                }
                if (Item_Name.Trim() != "")
                {
                    query = query.Where(i => i.Item_Name.Contains(Item_Name.Trim()));
                }

                //itmList = query.ToList();


                dpList = MappingToDetailes_Purshing(query);
                Excp = 0;
                return dpList;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
                return dpList;

            }
        }
        List<Detailes_Purshing> MappingToDetailes_Purshing(IQueryable<Items> itemquery)
        {
            List<Detailes_Purshing> dpList = new List<Detailes_Purshing>();
            MappingCLS mm = new MappingCLS();
            foreach (var item in itemquery)
            {
                Detailes_Purshing dp = new Detailes_Purshing();
                dp = mm.MappingToDetailes_Purshing((Items)item);
                dpList.Add(dp);
            }

            return dpList;


        }
        public List<Items> GetAllOnlyItems()
        {


            itmList = GetAllOnlyItems(ref opp);
            return itmList;

        }

        public List<Items> GetAllOnlyItems(ref operations opp)
        {
            im = new Items();
            IQueryable<Items> query = opp.db.Item.AsNoTracking().Where(i => i.Active_flage == 1).OrderBy(i => i.Item_Name);

            itmList = query.ToList();
            return itmList;

        }
        public List<SupplierPriceVM> GetSupplierItems(long Supplier_Id)
        {
            List<SupplierPriceVM> vm = new List<SupplierPriceVM>();
            im = new Items();
            IQueryable<SupplierPrice> query = opp.db.SupplierPrice.AsNoTracking().Where(i => i.Supplier_Id == Supplier_Id);

            List<SupplierPrice> supplierPrices = query.ToList();
            foreach (var item in supplierPrices)
            {
                SupplierPriceVM sp = new SupplierPriceVM();
                sp.Id = item.Id;
                sp.Item_Id = item.Item_Id;
                sp.Supplier_Id = item.Supplier_Id;

                sp.Item_Name = item.Item.Item_Name;
                sp.Supplier_Name = item.Supplier.Name;

                vm.Add(sp);
            }
            return vm;

        }
        public List<Items> GetAllOnlyItems(ref operations opp, string ItemsNameSearch, long ItemCategory_Id)
        {
            IQueryable<Items> query = opp.db.Item.AsNoTracking();
            if (ItemCategory_Id > 0)
            {
                query = query.Where(i => i.Item_Category_ID == ItemCategory_Id);
            }
            if (ItemsNameSearch.Trim() != "")
            {
                query = query.Where(i => i.Item_Name.Contains(ItemsNameSearch) | i.ItemName_OnInvoice.Contains(ItemsNameSearch));
            }



            return query.ToList();

        }
        public Items GetItemById(ref int Excp, ref string Msg, long Item_Id)
        {
            Items im_ = new Items();
            try
            {
                Excp = 0;
                im_ = opp.db.Item.Where(i => i.Id == Item_Id).Include(s => s.Store).Include(u => u.unit).FirstOrDefault();

                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;

            }

            return im_;
        }
        public Items GetItemByIdNoTracking(long Item_Id)
        {
            //Items im_ = new Items();

            var im_ = opp.db.Item.Where(i => i.Id == Item_Id).Include(s => s.Store).Include(u => u.unit).FirstOrDefault();

            return im_;
        }
        public long GetUnit_itemIdForDefaultStore(long Item_Id)
        {
            long Unit_item_Id = 0;
            //Items im_ = new Items();
            try
            {

                im = opp.db.Item.Where(i => i.Id == Item_Id).AsNoTracking().FirstOrDefault();
                if (im != null)
                {
                    Unit_item um = opp.db.Unit_item.AsNoTracking().Where(i => i.Item_Id == Item_Id & i.Store_id == im.S_id).FirstOrDefault();
                    if (um != null)
                    {
                        Unit_item_Id = um.id;
                    }

                }
                return Unit_item_Id;
            }
            catch (Exception ee)
            {
                return Unit_item_Id;

            }


        }
        public int GetItem_Is_NotShowOnInvoice(ref int Excp, ref string Msg, long Item_Id)
        {
            int Is_NotShowOnInvoice = 0;

            try
            {
                Excp = 0;
                Items im_ = opp.db.Item.AsNoTracking().Where(i => i.Id == Item_Id).FirstOrDefault();
                if (im_ != null)
                {
                    Is_NotShowOnInvoice = im_.Is_NotShowOnInvoice ?? 0;
                }
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;

            }

            return Is_NotShowOnInvoice;
        }
        public Items GetItemOnlyById(long Item_Id)
        {
            Items im_ = new Items();



            im_ = opp.db.Item.AsNoTracking().Where(i => i.Id == Item_Id).FirstOrDefault();



            return im_;
        }
        public Items GetItemByItemCode(string ItemCode)
        {
            Items im_ = new Items();
            im_ = opp.db.Item.AsNoTracking().Where(i => i.ItemCode == ItemCode).FirstOrDefault();

            return im_;
        }

        public void GetItemAndUnitItemPrices(long Unit_Item_Id, ref Unit_item um, ref Items im)
        {
            try
            {

                um = new Unit_item();

                um = opp.db.Unit_item.Where(i => i.id == Unit_Item_Id).FirstOrDefault();

                im = new Items();
                long Item_Id = um.Item_Id ?? 0;
                im = opp.db.Item.Where(i => i.Id == Item_Id).FirstOrDefault();


            }
            catch (Exception ee)
            {


            }
        }

        public void GetItemAndUnitItemAsNoTracting(long Unit_Item_Id, ref Unit_item um, ref Items im)
        {
            try
            {
                if (Unit_Item_Id == 0)
                {
                    return;
                }
                um = new Unit_item();
                //var ss = opp.db.Stores.ToList();
                um = opp.db.Unit_item.AsNoTracking().Where(i => i.id == Unit_Item_Id).Include(u => u.unit).Include(s => s.Store).FirstOrDefault();
                if (um != null)
                {
                    um.Full_qua = um.Full_qua ?? 0;
                    um.D_Qua = um.D_Qua ?? 0;
                    um.Item_No = um.Item_No ?? 0;
                }
                im = new Items();
                long Item_Id = um.Item_Id ?? 0;
                im = opp.db.Item.AsNoTracking().Where(i => i.Id == Item_Id).Include(u => u.unit).FirstOrDefault();


            }
            catch (Exception ee)
            {


            }
        }
        public void UpdateItemAndUnitItemPrices(Unit_item um_, Items im)
        {
            try
            {
                im.Edit_User_Id = Common.User_Id;
                im.EditOperationDate = DateTime.Now;
                opp.db.Entry(im).State = EntityState.Modified;
                opp.db.SaveChanges();
                //opp.db.Entry(um_).State = EntityState.Modified;
                //opp.db.SaveChanges();

                List<Unit_item> umList = new List<Unit_item>();

                umList = opp.db.Unit_item.Where(i => i.Item_Id == im.Id).ToList();

                foreach (Unit_item um in umList)
                {



                    um.S_price = um_.S_price;
                    um.P_price = um_.P_price;


                    um.S_price_LargeUnit = um_.S_price_LargeUnit;
                    um.S_price_SmallUnit = um_.S_price_SmallUnit;
                    um.P_price_LargeUnit = um_.P_price_LargeUnit;
                    um.P_price_SmallUnit = um_.P_price_SmallUnit;
                    um.S_price_Item_No = um_.S_price_Item_No;
                    um.P_price_Item_No = um_.P_price_Item_No;

                    um.Unit_Used_For_S_price_Default = um_.Unit_Used_For_S_price_Default;
                    um.Unit_Used_For_P_price_Default = um_.Unit_Used_For_P_price_Default;
                    um.Edit_User_Id = Common.User_Id;
                    um.EditOperationDate = DateTime.Now;



                    opp.db.Entry(um).State = EntityState.Modified;
                    opp.db.SaveChanges();
                }
            }
            catch (Exception ee)
            {


            }
        }
        public void GetItemsByCode(ref int Excp, ref string Msg, string Code)
        {
            try
            {
                Excp = 0;


                im = new Items();
                itmList = opp.db.Item.Where(i => i.code == Code).Include(s => s.Store).Include(u => u.unit).ToList();
                Set_ItemsViewModel();
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;

            }
        }
        public async void Set_ItemsViewModelAsync()
        {
            // Eager load related data to avoid lazy loading overhead
            var itmList = await opp.db.Item
                .AsNoTracking() // Boost performance (read-only)
                .Include(i => i.unit)
                .Include(i => i.Item_Category)
                .Include(i => i.ItemServiceCategory)
                .ToListAsync();

            // Map using LINQ projection
            itmsOrderVMList = itmList.Select(itm => new ItemsOrderViewModel
            {
                Serial = 0,
                Id = 0,
                Item_Id = itm.Id,
                Unit_Item_Id = 0,
                qua = 0,
                Part_Qua = 0,
                Item_No = 0,
                Item_Name = itm.Item_Name,
                AnotherName = itm.AnotherName,
                ItemName_OnInvoice = itm.ItemName_OnInvoice,
                Store_id = itm.S_id,
                Store_Name = "",
                Store_Type = itm.StoreUsed_Type,
                Specification = "",
                QnyStr = "",
                Code = itm.code,
                ItemCode = itm.ItemCode,
                Unit_Id = itm.Unit_Id,
                UnitStr = itm.UnitStr,
                B_Unit = itm.unit?.B_Unit ?? " ",
                S_unit = itm.unit?.S_unit ?? " ",
                Upon_Request = itm.Upon_Request,
                No3eboa = itm.No3eboa,
                S_Price = itm.S_Price,
                BlockNo = itm.BlockNo,
                shelfNo = itm.shelfNo,
                OrderOnshelfNo = itm.OrderOnshelfNo,
                Unit_Used_For_P_price_Default = itm.Unit_Used_For_P_price_Default,
                Unit_Used_For_S_price_Default = itm.Unit_Used_For_S_price_Default,
                Item_Category_Id = itm.Item_Category?.ID ?? 0,
                Item_Category_Name = itm.Item_Category?.Category_Name ?? "غير محدد",
                CarsWorkOrdersId = -1,
                CarsWorkOrdersRequestedDetailsId = -1,
                CarsWorkOrdersDeliverDetailsId = -1,
                ServiceDurationInMinutes = itm.ServiceDurationInMinutes ?? 0,
                IsCommissionEmployeeBased = itm.IsCommissionEmployeeBased ?? 0,
                IsCommissionEmployeeBased_True = itm.IsCommissionEmployeeBased ?? 0,
                CommissionEmployeeValue = itm.CommissionEmployeeValue ?? 0,
                ItemServiceCategoryName = itm.ItemServiceCategory?.ServiceCategoryName ?? "غير محدد",
                ItemServiceCategoryId = itm.ItemServiceCategoryId ?? 0,
                GenderTypeDescription = itm.ItemServiceCategory?.GenderTypeDescription ?? "غير محدد",
                GenderTypeId = itm.ItemServiceCategory?.GenderTypeId ?? 0,
                Is_ItemOrService = itm.Is_ItemOrService ?? 0
            }).ToList();
        }

        public void Set_ItemsViewModel()
        {
            itmsOrderVMList = itmList.Select(itm => new ItemsOrderViewModel
            {
                Serial = 0,
                Id = 0,
                Item_Id = itm.Id,
                Unit_Item_Id = 0,
                qua = 0,
                Part_Qua = 0,
                Item_No = 0,
                Item_Name = itm.Item_Name,
                AnotherName = itm.AnotherName,
                ItemName_OnInvoice = itm.ItemName_OnInvoice,
                Store_id = itm.S_id,
                Store_Name = "",
                Store_Type = itm.StoreUsed_Type,
                Specification = "",
                QnyStr = "",
                Code = itm.code,
                ItemCode = itm.ItemCode,
                Unit_Id = itm.Unit_Id,
                UnitStr = itm.UnitStr,
                B_Unit = itm.unit?.B_Unit ?? " ",
                S_unit = itm.unit?.S_unit ?? " ",
                Upon_Request = itm.Upon_Request,
                No3eboa = itm.No3eboa,
                S_Price = itm.S_Price,
                BlockNo = itm.BlockNo,
                shelfNo = itm.shelfNo,
                OrderOnshelfNo = itm.OrderOnshelfNo,
                Unit_Used_For_P_price_Default = itm.Unit_Used_For_P_price_Default,
                Unit_Used_For_S_price_Default = itm.Unit_Used_For_S_price_Default,
                Item_Category_Id = itm.Item_Category?.ID ?? 0,
                Item_Category_Name = itm.Item_Category?.Category_Name ?? "غير محدد",
                CarsWorkOrdersId = -1,
                CarsWorkOrdersRequestedDetailsId = -1,
                CarsWorkOrdersDeliverDetailsId = -1,
                ServiceDurationInMinutes = itm.ServiceDurationInMinutes ?? 0,
                IsCommissionEmployeeBased = itm.IsCommissionEmployeeBased ?? 0,
                IsCommissionEmployeeBased_True = itm.IsCommissionEmployeeBased ?? 0,
                CommissionEmployeeValue = itm.CommissionEmployeeValue ?? 0,
                ItemServiceCategoryName = itm.ItemServiceCategory?.ServiceCategoryName ?? "غير محدد",
                ItemServiceCategoryId = itm.ItemServiceCategoryId ?? 0,
                GenderTypeDescription = itm.ItemServiceCategory?.GenderTypeDescription ?? "غير محدد",
                GenderTypeId = itm.ItemServiceCategory?.GenderTypeId ?? 0,
                Is_ItemOrService = itm.Is_ItemOrService ?? 0,
                TotalSessions = itm.TotalSessions ?? 1,

            }).ToList();
        }

        public void Set_ItemsViewModel2()
        {
            // تحميل كل العلاقات الملاحية مسبقاً لتجنب Lazy Loading
            itmList = itmList
                .Where(i => i != null)
                .ToList();

            itmsOrderVMList = itmList.AsParallel() // المعالجة المتوازية تزيد السرعة مع القوائم الكبيرة
                .Select(itm => new ItemsOrderViewModel
                {
                    Serial = 0,
                    Id = 0,
                    Item_Id = itm.Id,
                    Unit_Item_Id = 0,
                    qua = 0,
                    Part_Qua = 0,
                    Item_No = 0,
                    Item_Name = itm.Item_Name,
                    AnotherName = itm.AnotherName,
                    ItemName_OnInvoice = itm.ItemName_OnInvoice,
                    Store_id = itm.S_id,
                    Store_Name = "",
                    Store_Type = itm.StoreUsed_Type,
                    Specification = "",
                    QnyStr = "",
                    Code = itm.code,
                    ItemCode = itm.ItemCode,
                    Unit_Id = itm.Unit_Id,
                    UnitStr = itm.UnitStr,
                    B_Unit = itm.unit?.B_Unit,
                    S_unit = itm.unit?.S_unit,
                    Upon_Request = itm.Upon_Request,
                    No3eboa = itm.No3eboa,
                    S_Price = itm.S_Price,
                    BlockNo = itm.BlockNo,
                    shelfNo = itm.shelfNo,
                    OrderOnshelfNo = itm.OrderOnshelfNo,
                    Unit_Used_For_P_price_Default = itm.Unit_Used_For_P_price_Default,
                    Unit_Used_For_S_price_Default = itm.Unit_Used_For_S_price_Default,
                    Item_Category_Id = itm.Item_Category?.ID ?? 0,
                    Item_Category_Name = itm.Item_Category?.Category_Name ?? "غير محدد",
                    CarsWorkOrdersId = -1,
                    CarsWorkOrdersRequestedDetailsId = -1,
                    CarsWorkOrdersDeliverDetailsId = -1,
                    ServiceDurationInMinutes = itm.ServiceDurationInMinutes ?? 0,
                    IsCommissionEmployeeBased = itm.IsCommissionEmployeeBased ?? 0,
                    IsCommissionEmployeeBased_True = itm.IsCommissionEmployeeBased ?? 0,
                    CommissionEmployeeValue = itm.CommissionEmployeeValue ?? 0,
                    ItemServiceCategoryName = itm.ItemServiceCategory?.ServiceCategoryName ?? "غير محدد",
                    ItemServiceCategoryId = itm.ItemServiceCategoryId ?? 0,
                    GenderTypeDescription = itm.ItemServiceCategory?.GenderTypeDescription ?? "غير محدد",
                    GenderTypeId = itm.ItemServiceCategory?.GenderTypeId ?? 0,
                    Is_ItemOrService = itm.Is_ItemOrService ?? 0
                }).ToList();
        }

        public void Set_ItemsViewModel1()
        {
            //Items itm = new Items();
            itmsOrderVMList = new List<ItemsOrderViewModel>();

            foreach (Items itm in itmList)
            {
                ItemsOrderViewModel itmOrder_VM = new ItemsOrderViewModel();





                itmOrder_VM.Serial = 0;
                itmOrder_VM.Id = 0;
                itmOrder_VM.Item_Id = itm.Id;

                itmOrder_VM.Unit_Item_Id = 0;

                itmOrder_VM.qua = 0;
                itmOrder_VM.Part_Qua = 0;
                itmOrder_VM.Item_No = 0;

                itmOrder_VM.Item_Name = itm.Item_Name;
                itmOrder_VM.AnotherName = itm.AnotherName;
                itmOrder_VM.ItemName_OnInvoice = itm.ItemName_OnInvoice;


                itmOrder_VM.Store_id = itm.S_id;

                itmOrder_VM.Store_Name = "";
                itmOrder_VM.Store_Type = itm.StoreUsed_Type;

                itmOrder_VM.Specification = "";// itm.Specification;
                itmOrder_VM.QnyStr = "";
                itmOrder_VM.Code = itm.code;
                itmOrder_VM.ItemCode = itm.ItemCode;
                itmOrder_VM.Unit_Id = itm.Unit_Id;
                itmOrder_VM.UnitStr = itm.UnitStr;
                itmOrder_VM.B_Unit = itm.unit.B_Unit;

                itmOrder_VM.S_unit = itm.unit.S_unit;
                itmOrder_VM.Upon_Request = itm.Upon_Request;
                itmOrder_VM.No3eboa = itm.No3eboa;

                itmOrder_VM.S_Price = itm.S_Price;

                //itm_VMList.Small_unit = itm.Unit_item;
                itmOrder_VM.BlockNo = itm.BlockNo;
                itmOrder_VM.shelfNo = itm.shelfNo;
                itmOrder_VM.OrderOnshelfNo = itm.OrderOnshelfNo;


                itmOrder_VM.Unit_Used_For_P_price_Default = itm.Unit_Used_For_P_price_Default;
                itmOrder_VM.Unit_Used_For_S_price_Default = itm.Unit_Used_For_S_price_Default;
                if (itm.Item_Category != null)
                {
                    itmOrder_VM.Item_Category_Id = itm.Item_Category.ID;
                    itmOrder_VM.Item_Category_Name = itm.Item_Category.Category_Name;

                }

                itmOrder_VM.CarsWorkOrdersId = -1;
                itmOrder_VM.CarsWorkOrdersRequestedDetailsId = -1;
                itmOrder_VM.CarsWorkOrdersDeliverDetailsId = -1;




                try
                {
                    itmOrder_VM.ServiceDurationInMinutes = itm.ServiceDurationInMinutes ?? 0;
                    itmOrder_VM.IsCommissionEmployeeBased = itm.IsCommissionEmployeeBased ?? 0;
                    itmOrder_VM.IsCommissionEmployeeBased_True = itm.IsCommissionEmployeeBased ?? 0;
                    itmOrder_VM.CommissionEmployeeValue = itm.CommissionEmployeeValue ?? 0;
                    itmOrder_VM.ItemServiceCategoryName = itm.ItemServiceCategory != null ? itm.ItemServiceCategory.ServiceCategoryName : "غير محدد";
                    itmOrder_VM.ItemServiceCategoryId = itm.ItemServiceCategoryId ?? 0;

                    itmOrder_VM.GenderTypeDescription = itm.ItemServiceCategory != null ? itm.ItemServiceCategory.GenderTypeDescription : "غير محدد";
                    itmOrder_VM.GenderTypeId = itm.ItemServiceCategory != null ? itm.ItemServiceCategory.GenderTypeId : 0;

                    itmOrder_VM.Is_ItemOrService = itm.Is_ItemOrService ?? 0;


                    //if (itm.ItemServiceCategory != null)
                    //{
                    //    itmOrder_VM.GenderTypeDescription = itm.ItemServiceCategory.GenderTypeDescription ?? "";
                    //    itmOrder_VM.GenderTypeId = itm.ItemServiceCategory.GenderTypeId ?? 0;
                    //}
                }
                catch (Exception ee)
                {
                }


                itmsOrderVMList.Add(itmOrder_VM);
            }



        }
        public void GetItemsByName(ref int Excp, ref string Msg, string Name, long ItemCategoryId)
        {
            try
            {
                Excp = 0;


                im = new Items();
                //itmList = opp.db.Item.Where(i => i.Item_Name.Contains(Name) | i.AnotherName.Contains(Name) | i.ItemName_OnInvoice.Contains(Name)).Include(s => s.Store).Include(u => u.unit).ToList();

                IQueryable<Items> query = opp.db.Item.Where(i => i.Item_Name.Contains(Name) | i.AnotherName.Contains(Name) | i.ItemName_OnInvoice.Contains(Name)).Include(s => s.Store).Include(u => u.unit);
                query = query.Where(i => i.Active_flage == 1);

                if (ItemCategoryId > 0)
                {
                    query = query.Where(i => i.Item_Category_ID == ItemCategoryId);
                }
                itmList = query.ToList();
                Set_ItemsViewModel();
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;

            }
        }

        public void GetItemsByOldData(ref int Excp, ref string Msg, AICarsWorkOrdersItemProcessedDetail carsWorkOrdersDetail)
        {
            GetItemsByOldData(ref Excp, ref Msg, carsWorkOrdersDetail.Item_Id, carsWorkOrdersDetail.Unit_ItemsId);


        }
        public void GetItemsByOldData(ref int Excp, ref string Msg, CarsWorkOrdersRequestedDetail carsWorkOrdersDetail)
        {
            GetItemsByOldData(ref Excp, ref Msg, carsWorkOrdersDetail.Item_Id, carsWorkOrdersDetail.Unit_ItemsId);


        }
        public void GetItemsByOldData(ref int Excp, ref string Msg, CarsWorkOrdersItemProcessedDetail carsWorkOrdersDetail)
        {
            GetItemsByOldData(ref Excp, ref Msg, carsWorkOrdersDetail.Item_Id, carsWorkOrdersDetail.Unit_ItemsId);

            //try
            //{
            //    Excp = 0;

            //    ///////////// get  item row data
            //    im = new Items();
            //    im = opp.db.Item.Where(i => i.id == carsWorkOrdersDetail.Item_Id).Include(s => s.Store).Include(u => u.unit).FirstOrDefault();

            //    itmList.Add(im);
            //    ///////////// get unit_item row data
            //    um = opp.db.Unit_item.Where(i => i.id == carsWorkOrdersDetail.Unit_ItemsId).Include(i => i.Item).Include(s => s.Store).Include(u => u.unit).OrderBy(i => i.Item.Item_Name).FirstOrDefault();
            //    umList = new List<Unit_item>();
            //    umList.Add(um);
            //    UnitItemsVMList = GetStoresItemsModelView(umList);
            //    Excp = 0;
            //}
            //catch (Exception ee)
            //{
            //    Excp = 1;
            //    Msg = ee.Message;

            //}
        }
        public void GetItemsByOldData(ref int Excp, ref string Msg, long Item_Id, long Unit_ItemsId)
        {
            try
            {
                Excp = 0;

                ///////////// get  item row data
                im = new Items();
                im = opp.db.Item.Where(i => i.Id == Item_Id).Include(s => s.Store).Include(u => u.unit).FirstOrDefault();

                itmList.Add(im);
                ///////////// get unit_item row data
                um = opp.db.Unit_item.Where(i => i.id == Unit_ItemsId).Include(i => i.Item).Include(s => s.Store).Include(u => u.unit).OrderBy(i => i.Item.Item_Name).FirstOrDefault();
                umList = new List<Unit_item>();
                umList.Add(um);
                UnitItemsVMList = GetStoresItemsModelView(umList);
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;

            }
        }


        public void GetItemsByOldData(ref int Excp, ref string Msg, Detailes_Purshing dp)
        {

            try
            {
                Excp = 0;

                ///////////// get  item row data
                im = new Items();
                itmList = new List<Items>();
                um = new Unit_item();
                umList = new List<Unit_item>();

                im = opp.db.Item.Where(i => i.Id == dp.Item_Id).Include(s => s.Store).Include(u => u.unit).FirstOrDefault();

                itmList.Add(im);
                Set_ItemsViewModel();
                ///////////// get unit_item row data
                um = opp.db.Unit_item.Where(i => i.id == dp.Unit_item_Table_Id).Include(i => i.Item).Include(s => s.Store).Include(u => u.unit).OrderBy(i => i.Item.Item_Name).FirstOrDefault();
                umList = new List<Unit_item>();
                umList.Add(um);
                UnitItemsVMList = GetStoresItemsModelView(umList);
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;

            }
        }
        public void GetAllItemsForCars(ref int Excp, ref string Msg, ref operations opp, long carsWorkOrdersId, long ItemCategoryId)
        {
            try
            {
                Excp = 0;
                CarsWorkOrder carsWorkOrder = opp.db.CarsWorkOrders.Include(c => c.Car).Where(w => w.Id == carsWorkOrdersId).FirstOrDefault();
                Car car = carsWorkOrder.Car;
                ItemsForCarsCLS itemsForCarsCLS = new ItemsForCarsCLS(ref opp);
                itmList = itemsForCarsCLS.GetAllItemsForCars(ref Excp, ref Msg, ref opp, car, ItemCategoryId);


                Set_ItemsViewModel();
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;

            }
        }
        public void GetAllItemsForCarsWorkOrdersRequestedDetail(ref int Excp, ref string Msg, ref operations opp, long carsWorkOrdersId, long ItemCategoryId)
        {
            try
            {
                Excp = 0;
                List<CarsWorkOrdersRequestedDetail> carsWorkOrdersRequestedDetail = opp.db.CarsWorkOrdersRequestedDetails.Where(w => w.Id == carsWorkOrdersId).ToList();

                ItemsForCarsCLS itemsForCarsCLS = new ItemsForCarsCLS(ref opp);
                itmList = itemsForCarsCLS.GetAllItemsForCarsWorkOrdersRequestedDetail(ref Excp, ref Msg, ref opp, carsWorkOrdersId, carsWorkOrdersRequestedDetail, ItemCategoryId);


                Set_ItemsViewModel();
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;

            }
        }


        public int UpdateItemByUnit_itemId(ref int Excp, ref string Msg, ArrayList Arr_Unit_item_Table_Id, int Active_flageStatus)
        {
            //صلاحية تعديل بيانات الصنف  
            bool IsHavePermission = Common.IsHavePermission(Common.User_Id, 917, Common.Project_ID, Common.Branch_Id);
            if (IsHavePermission == false)
            {
                Excp = 917;
                //MessageBox.Show("ليس لديك صلاحية تعديل بيانات الصنف");
                return 0;
            }
            int DeletedCount = 0;
            try
            {
                Excp = 0;

                string mysql = "";
                opp = new operations(1);

                //My_DbContext.ConnStr = opp.str_sql_conn;
                //My_DbContext db = new My_DbContext();
                for (int i = 0; i < Arr_Unit_item_Table_Id.Count; i++)
                {

                    long Unit_item_Table_Id = long.Parse(Arr_Unit_item_Table_Id[i].ToString());


                    string msql = "Select * from Unit_Item Where Id=" + Unit_item_Table_Id;
                    DataTable dt = opp.return_DataTable(msql);
                    if (dt.Rows.Count > 0)
                    {
                        long Item_Id = long.Parse(dt.Rows[0]["Item_Id"].ToString());


                        mysql = mysql + " Update Unit_Item Set Active_Flage =" + Active_flageStatus + "  where Item_Id=" + Item_Id;
                        mysql = mysql + " Update  Item Set Active_Flage = " + Active_flageStatus + " ,User_Id=" + Common.User_Id + ",OperationDate='" + operations.dateFormat(DateTime.Now.Date) + "' where Id=" + Item_Id;
                        DeletedCount++;

                        opp.insert_me(mysql);

                    }
                    if (DeletedCount > 0)
                    {
                        Msg = "تم تغيير حالة الصنف";
                    }
                    opp.Commit();
                    Excp = 0;
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
                Msg = ee.Message;

            }

            return DeletedCount;

        }

        public int UpdateItemByUnit_item_Id(ref int Excp, ref string Msg, ArrayList Arr_Unit_item_Table_Id, int Active_flageStatus)
        {
            /*
            try
            {
                Excp = 0;

                operations opp = new operations(2);
                //My_DbContext.ConnStr = opp.str_sql_conn;
                //My_DbContext db = new My_DbContext();
                for (int i = 0; i < Arr_Unit_item_Table_Id.Count; i++)
                {
                    um = new Unit_item();
                    im = new Items();
                    long Unit_item_Table_Id = long.Parse(Arr_Unit_item_Table_Id[i].ToString());
                    um = opp.db.Unit_item.Where(s => s.id == Unit_item_Table_Id).FirstOrDefault();
                    if (um != null)
                    {
                        um.active_flage = Active_flageStatus;
                        im = opp.db.Item.Where(p => p.id == um.Item_Id).FirstOrDefault();
                        if (im != null)
                        {
                            im.Active_flage = Active_flageStatus;
                        }
                        opp.db.Entry(im).State = EntityState.Modified;
                        opp.db.SaveChanges();
                        opp.db.Entry(um).State = EntityState.Modified;
                        opp.db.SaveChanges();
                    }


                }


                opp.Commit();
                Excp = 0;


            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
                Msg = ee.Message;

            }
            */
            int DeletedCount = 0;
            try
            {
                Excp = 0;

                string mysql = "";
                opp = new operations(1);

                //My_DbContext.ConnStr = opp.str_sql_conn;
                //My_DbContext db = new My_DbContext();
                for (int i = 0; i < Arr_Unit_item_Table_Id.Count; i++)
                {

                    long Unit_item_Table_Id = long.Parse(Arr_Unit_item_Table_Id[i].ToString());


                    string msql = "Select * from Unit_Item Where Id=" + Unit_item_Table_Id;
                    DataTable dt = opp.return_DataTable(msql);
                    if (dt.Rows.Count > 0)
                    {
                        long Item_Id = long.Parse(dt.Rows[0]["Item_Id"].ToString());


                        mysql = mysql + " Update Unit_Item Set Active_Flage =" + Active_flageStatus + "  where Item_Id=" + Item_Id;
                        mysql = mysql + " Update  Item Set Active_Flage = " + Active_flageStatus + " ,User_Id=" + Common.User_Id + ",OperationDate='" + operations.dateFormat(DateTime.Now.Date) + "' where Id=" + Item_Id;
                        DeletedCount++;

                        opp.insert_me(mysql);

                    }
                    if (DeletedCount > 0)
                    {
                        Msg = "تم تغيير حالة الصنف";
                    }
                    opp.Commit();
                    Excp = 0;
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
                Msg = ee.Message;

            }

            return DeletedCount;

        }

        bool CheckToDeleteItemByUnit_item_Id(ref int Excp, ref string Msg, long Unit_item_Id)
        {
            bool IsDeleteValid = true;
            try
            {

                string mysql = "";


                DataTable dt = new DataTable();
                mysql = "Select * from Invoices_Details Where Unit_Item_Table_Id=" + Unit_item_Id + "";
                dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    Excp = 1;
                    Msg = "لا يمكن  حذف الصنف نظرا لوجود فاتورة مبيعات له  ";
                    IsDeleteValid = false;
                    return IsDeleteValid;
                }
                dt = new DataTable();
                mysql = "Select * from Bills_Details Where Unit_Item_Table_Id=" + Unit_item_Id + "";
                dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {

                    Excp = 1;
                    Msg = "لا يمكن  حذف الصنف نظرا لوجود فاتورة مشتريات له  ";
                    IsDeleteValid = false;
                    return IsDeleteValid;
                }
                dt = new DataTable();
                mysql = "Select * from Detailes_Purshing Where Unit_item_Table_Id=" + Unit_item_Id + "";
                dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {

                    Excp = 1;
                    Msg = "لا يمكن  حذف الصنف نظرا لوجود حركات مخزنية  ";
                    IsDeleteValid = false;
                    return IsDeleteValid;
                }
                mysql = "Select * from CarsWorkOrdersItemProcessedDetails Where Unit_ItemsId=" + Unit_item_Id + "";
                dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {

                    Excp = 1;
                    Msg = "لا يمكن  حذف الصنف نظرا لوجودة بأوامر الشغل  ";
                    IsDeleteValid = false;
                    return IsDeleteValid;
                }

                Excp = 0;
                return IsDeleteValid;
                //mysql = " Update Unit_Item Set Active_Flage =-1  where Item_Id=" + Unit_item_Id;
                //arr.Add(mysql);

                //mysql = " Update  Item Set Active_Flage =-1 ,User_Id=" + Common.User_Id + ",OperationDate='" + operations.dateFormat(DateTime.Now.Date) + "' where Id=" + Unit_item_Id;
                //arr.Add(mysql);

                //opp.insert_me_transaction(arr);

            }
            catch (Exception fff)
            {
                Excp = 1;
                Msg = fff.Message.ToString();
                return IsDeleteValid;

            }
        }

        public int DeleteItemByUnit_itemId(ref int Excp, ref string Msg, long Unit_item_Table_Id)
        {
            ArrayList Arr_Unit_item_Table_Id = new ArrayList();
            Arr_Unit_item_Table_Id.Add(Unit_item_Table_Id);

            return DeleteItemByUnit_itemId(ref Excp, ref Msg, Arr_Unit_item_Table_Id);
        }
        public int DeleteItemByUnit_itemId(ref int Excp, ref string Msg, ArrayList Arr_Unit_item_Table_Id)
        {
            //صلاحية حذف بيانات الصنف  
            bool IsHavePermission = Common.IsHavePermission(Common.User_Id, 918, Common.Project_ID, Common.Branch_Id);
            if (IsHavePermission == false)
            {
                Excp = 918;
                //MessageBox.Show("ليس لديك صلاحية حذف بيانات الصنف");
                return 0;
            }
            int DeletedCount = 0;
            try
            {
                Excp = 0;

                string mysql = "";
                opp = new operations(1);

                //My_DbContext.ConnStr = opp.str_sql_conn;
                //My_DbContext db = new My_DbContext();
                for (int i = 0; i < Arr_Unit_item_Table_Id.Count; i++)
                {
                    bool IsDeleteValid = false;
                    long Unit_item_Table_Id = long.Parse(Arr_Unit_item_Table_Id[i].ToString());
                    IsDeleteValid = CheckToDeleteItemByUnit_item_Id(ref Excp, ref Msg, Unit_item_Table_Id);
                    um = new Unit_item();
                    im = new Items();

                    string msql = "Select * from Unit_Item Where Id=" + Unit_item_Table_Id;
                    DataTable dt = opp.return_DataTable(msql);
                    if (dt.Rows.Count > 0)
                    {
                        long Item_Id = long.Parse(dt.Rows[0]["Item_Id"].ToString());

                        if (IsDeleteValid == true)
                        {
                            mysql = mysql + " Delete from Unit_Item    where Item_Id=" + Item_Id;
                            mysql = mysql + " Delete from  Item   where Id=" + Item_Id;

                            DeletedCount++;

                        }
                        else
                        {
                            mysql = mysql + " Update Unit_Item Set Active_Flage =-1  where Item_Id=" + Item_Id;
                            mysql = mysql + " Update  Item Set Active_Flage =-1 ,User_Id=" + Common.User_Id + ",OperationDate='" + operations.dateFormat(DateTime.Now.Date) + "' where Id=" + Item_Id;
                            DeletedCount++;
                        }
                        opp.insert_me(mysql);

                    }
                    if (DeletedCount > 0)
                    {
                        Msg = "تم حذف الصنف";
                    }
                    opp.Commit();
                    Excp = 0;
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
                Msg = ee.Message;

            }

            return DeletedCount;

        }
        public int DeleteItemByUnit_itemId_FromStore(ref int Excp, ref string Msg, long Unit_item_Table_Id)
        {
            int DeletedCount = 0;
            try
            {
                Excp = 0;

                string mysql = "";
                opp = new operations(1);

                //My_DbContext.ConnStr = opp.str_sql_conn;
                //My_DbContext db = new My_DbContext();

                bool IsDeleteValid = false;

                IsDeleteValid = CheckToDeleteItemByUnit_item_Id(ref Excp, ref Msg, Unit_item_Table_Id);


                string msql = "Select * from Unit_Item Where Id=" + Unit_item_Table_Id;
                DataTable dt = opp.return_DataTable(msql);
                if (dt.Rows.Count > 0)
                {
                    long Item_Id = long.Parse(dt.Rows[0]["Item_Id"].ToString());

                    ///Check If the item count in unit_Item
                    msql = "Select * from Unit_Item Where Item_Id=" + Item_Id;
                    dt = opp.return_DataTable(msql);
                    //////فى حالة الصنف متواجد فى مخزن واحد وغير موجود فى مخازن اخري
                    if (dt.Rows.Count == 1)
                    {
                        if (IsDeleteValid == true)
                        {
                            mysql = mysql + " Delete from Unit_Item    where Item_Id=" + Item_Id;
                            mysql = mysql + " Delete from  Item   where Id=" + Item_Id;

                            DeletedCount++;

                        }
                        else
                        {
                            mysql = mysql + " Update Unit_Item Set Active_Flage =-1  where Item_Id=" + Item_Id;
                            mysql = mysql + " Update  Item Set Active_Flage =-1 ,User_Id=" + Common.User_Id + ",OperationDate='" + operations.dateFormat(DateTime.Now.Date) + "' where Id=" + Item_Id;
                            DeletedCount++;
                        }
                    }
                    if (dt.Rows.Count > 1)  //////فى حالة الصنف متواجد فى اكثر من مخزن 
                    {
                        if (IsDeleteValid == true)
                        {
                            mysql = mysql + " Delete from Unit_Item    where Id=" + Unit_item_Table_Id;


                            DeletedCount++;

                        }
                        else
                        {
                            mysql = mysql + " Update Unit_Item Set Active_Flage =-1  where Id=" + Unit_item_Table_Id;
                            DeletedCount++;
                        }
                    }


                    opp.insert_me(mysql);

                }
                if (DeletedCount > 0)
                {
                    Msg = "تم حذف الصنف";
                }
                opp.Commit();
                Excp = 0;
                Thread.Sleep(1000);

            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
                Msg = ee.Message;

            }

            return DeletedCount;

        }
        public void DeleteItemByUnit_item_Id(ref int Excp, ref string Msg, ArrayList Arr_Unit_item_Table_Id, int Active_flageStatus)
        {
            try
            {
                Excp = 0;

                operations opp = new operations();
                //My_DbContext.ConnStr = opp.str_sql_conn;
                //My_DbContext db = new My_DbContext();
                for (int i = 0; i < Arr_Unit_item_Table_Id.Count; i++)
                {
                    um = new Unit_item();
                    im = new Items();
                    long Unit_item_Table_Id = long.Parse(Arr_Unit_item_Table_Id[i].ToString());
                    um = opp.db.Unit_item.Where(s => s.id == Unit_item_Table_Id).FirstOrDefault();
                    if (um != null)
                    {

                        im = opp.db.Item.Where(p => p.Id == um.Item_Id).FirstOrDefault();
                        if (im != null)
                        {
                            opp.db.Unit_item.Remove(um);
                            opp.db.Item.Remove(im);

                            opp.db.SaveChanges();
                        }

                    }


                }


                opp.Commit();
                Excp = 0;


            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
                Msg = ee.Message;

            }



        }

        public void GetUnit_item(Items im, Unit_item um)
        {
            operations opp = new operations();
            string mysql = "";
            mysql = " SELECT  * FROM Unit_item Where Id=" + um.id;
            DataTable dt = new DataTable();
            dt = opp.return_DataTable(mysql);

        }
        public Unit_item GetUnit_item(ref operations opp, long Unit_item_Id)
        {
            um = new Unit_item();
            um = opp.db.Unit_item.Where(u => u.id == Unit_item_Id).Include(i => i.Item).Include(u => u.unit).FirstOrDefault();

            return um;
        }
        public Unit_item GetUnit_item(ref operations opp, long Item_Id, long StoreId)
        {
            um = new Unit_item();
            um = opp.db.Unit_item.Where(u => u.Item_Id == Item_Id & u.Store_id == StoreId).FirstOrDefault();

            return um;
        }

        public void UpdateItemBalance(ref int Excp, ref string Msg, DataTable dt)
        {
            string mysql = " ";
            decimal Full_Qua = 0;
            decimal D_Qua = 0;
            decimal Item_No = 0;
            decimal Upon_Request = 0;
            decimal P_Price = 0;
            int x = 0;
            foreach (DataRow dr in dt.Rows)
            {
                x++;
                try
                {
                    Full_Qua = decimal.Parse(dr["Full_qua"].ToString());
                }
                catch (Exception ee)
                {
                    Full_Qua = 0;

                }
                try
                {
                    D_Qua = decimal.Parse(dr["D_Qua"].ToString());
                }
                catch (Exception ee)
                {

                    D_Qua = 0;
                }
                try
                {
                    Item_No = decimal.Parse(dr["Item_No"].ToString());
                }
                catch (Exception ee)
                {
                    Item_No = 0;
                }
                try
                {
                    Upon_Request = decimal.Parse(dr["Upon_Request"].ToString());
                }
                catch (Exception ee)
                {
                    Upon_Request = 0;
                }
                try
                {
                    P_Price = decimal.Parse(dr["P_Price"].ToString());
                }
                catch (Exception ee)
                {
                    P_Price = 0;
                }

                long Id = long.Parse(dr["Id"].ToString());
                long Item_id = long.Parse(dr["Item_id"].ToString());
                mysql = mysql + " UPDATE Unit_item SET Full_qua =" + Full_Qua + ", D_Qua =" + D_Qua + ", Item_No =" + Item_No + ",Upon_Request=" + Upon_Request + ",P_Price=" + P_Price + " ,P_priceAvgCost=" + P_Price + ", Edit_User_Id=" + Common.User_Id + ", EditOperationDate=GETDATE() WHERE(id = " + Id + ") ";
                mysql = mysql + " UPDATE  item SET   Upon_Request=" + Upon_Request + ",P_Price=" + P_Price + " ,P_price_LargeUnit=" + P_Price + ", P_price_SmallUnit=" + P_Price + " ,P_priceAvgCost=" + P_Price + ", Edit_User_Id=" + Common.User_Id + ", EditOperationDate=GETDATE() WHERE(id = " + Item_id + ") ";


                if (Common.Compute_ItemBalance_With_Fraction == 0)
                {
                    mysql = mysql + " UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
                    mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
                    mysql = mysql + " WHERE (id = " + Id + ")  ";

                }
                if (x == 100)
                {
                    UpdateBalance(ref Excp, ref Msg, mysql);
                    if (Excp == 1)
                    {
                        break;
                    }
                    else
                    {
                        Excp = 0;
                        mysql = "";
                    }
                    x = 0;
                    Thread.Sleep(3000);
                }

            }
            if (mysql.Trim() != "")
            {
                UpdateBalance(ref Excp, ref Msg, mysql);
            }
            //if (Excp == 1)
            //{

            //}
            //else
            //{
            //    Excp = 0;
            //    mysql = "";
            //}

            //if (Common.Compute_ItemBalance_With_Fraction == 0)
            //{
            //    mysql = "  UPDATE Unit_item SET Full_qua = FLOOR((Small_unit * Full_qua + D_Qua) / Small_unit), ";
            //    mysql = mysql + " D_Qua =    (Small_unit * Full_qua + D_Qua) % Small_unit  ";
            //   // mysql = mysql + " WHERE (id = " + invd.Unit_Item_Table_Id + ")  ";
            //    opp.insert_me(mysql);
            //}

        }

        void UpdateBalance(ref int Excp, ref string Msg, string mysql)
        {
            operations opp = new operations(1);
            try
            {

                opp.insert_me(mysql);
                Msg = "تم حفظ الأرصدة";
                opp.Commit();
                Excp = 0;
            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
                Msg = "خطأ فى حفظ الأرصدة";
                Msg = "\n\r" + Msg;
            }
        }
        public bool CkeckItemPrice(ref int Excp, ref string Msg, DataTable dt)
        {
            bool IsPriceValide = true;
            string mysql = " ";
            decimal Full_Qua = 0;
            decimal D_Qua = 0;
            decimal Item_No = 0;
            decimal Upon_Request = 0;
            decimal P_Price = 0;
            int x = 0;
            foreach (DataRow dr in dt.Rows)
            {
                x++;
                try
                {
                    Full_Qua = decimal.Parse(dr["Full_qua"].ToString());
                }
                catch (Exception ee)
                {
                    Full_Qua = 0;

                }
                try
                {
                    D_Qua = decimal.Parse(dr["D_Qua"].ToString());
                }
                catch (Exception ee)
                {

                    D_Qua = 0;
                }
                try
                {
                    Item_No = decimal.Parse(dr["Item_No"].ToString());
                }
                catch (Exception ee)
                {
                    Item_No = 0;
                }
                try
                {
                    Upon_Request = decimal.Parse(dr["Upon_Request"].ToString());
                }
                catch (Exception ee)
                {
                    Upon_Request = 0;
                }
                try
                {
                    P_Price = decimal.Parse(dr["P_Price"].ToString());
                }
                catch (Exception ee)
                {
                    P_Price = 0;
                }

                long Id = long.Parse(dr["Id"].ToString());
                string Item_Name = dr["Item_Name"].ToString();


                if (Full_Qua > 0 | D_Qua > 0)
                {
                    if (P_Price == 0)
                    {
                        Msg = "يجب ضبط أسعار الصنف";
                        Msg = Msg + "\n\r";
                        Msg = Msg + Item_Name;
                        IsPriceValide = false;
                        break;
                    }
                }


            }

            return IsPriceValide;

        }
        void FillUnit_Item(Unit_item um, DataTable dt, int index)
        {
            //Due_Date = Common.GetValueNullDateTime(dt, index, "Due_Date");

            //Bank_id = Common.GetValueInt(dt, index, "Bank_id");

            //Check_Value = Common.GetValueDecimal(dt, index, "Check_Value");

        }

        public DataTable GetAllItemsImage_ByCategory_ID(long Item_Category_ID)
        {
            operations opp = new operations();
            string mysql = @" SELECT        Item_ImagesStore.ImageId, Item_ImagesStore.Description, Item_ImagesStore.OriginalPath, Item_ImagesStore.ImageData, Item_ImagesStore.Default_Img_Flage, Item_ImagesStore.Item_Type, Item.Item_Name, 
                         Item_Category.Category_Name, Item.id AS Item_Id
FROM            Item INNER JOIN
                         Item_Category ON Item.Item_Category_ID = Item_Category.ID LEFT OUTER JOIN
                         Item_ImagesStore ON Item.id = Item_ImagesStore.Item_Id
  ";
            mysql = mysql + @" WHERE (Item.Active_flage = 1) AND ( (Item_ImagesStore.Default_Img_Flage = 1) OR
                         (Item_ImagesStore.Default_Img_Flage IS NULL)) ";
            if (Item_Category_ID > 0)
            {
                mysql = mysql + " AND (Item.Item_Category_ID = " + Item_Category_ID + ") ";
            }
            mysql = mysql + " Order By Item.Item_Name ";
            DataTable dt = opp.return_DataTable(mysql); ;
            return dt;

        }

        public DataTable GetDataByMainCategory_StoreType(int Active_flage, int StoreUsed_Type, long Item_Category_Id)
        {
            DataTable dt = new DataTable();
            try
            {
                string mysql;

                mysql = @" SELECT DISTINCT 0 As Serial_,
                         Store.Store_Name, Item.id AS Item_Id, Item.code, Item.Item_Name, Item.S_id, Item.Item_Category_ID, Item.No3eboa, Item.Active_flage, 
                         Item_Category.Category_Name, unit.u_name, unit.B_Unit, unit.S_unit, unit.B_Unit_Value, unit.S_unit_Value, Store.StoreUsed_Type, Unit_item.id AS Unit_item_Id, 
                         Item_Category.Main_Item_Category_Id
FROM            Item INNER JOIN
                         Store ON Item.S_id = Store.id INNER JOIN
                         Unit_item ON Item.id = Unit_item.Item_id AND Item.S_id = Unit_item.Store_id INNER JOIN
                         Item_Category ON Item.Item_Category_ID = Item_Category.ID INNER JOIN
                         unit ON Unit_item.unit_id = unit.id ";

                mysql = mysql + " WHERE (Item_Category.IS_Active = 1) AND (Item.Active_flage = " + Active_flage + ")  ";

                if (StoreUsed_Type > 0)
                {
                    mysql = mysql + "  AND(Store.StoreUsed_Type = " + StoreUsed_Type + ") ";
                }

                if (Item_Category_Id > 0)
                {
                    mysql = mysql + "    AND(Item_Category.id = " + Item_Category_Id + ")";
                }
                mysql = mysql + "  ORDER BY Item.Item_Category_ID ";

                operations opp = new operations();
                dt = opp.return_DataTable(mysql);

                return dt;
            }
            catch (Exception ee)
            {



            }
            return dt;
        }

        public DataTable GetDataByCategory_StoreType(int Active_flage, int StoreUsed_Type, long Item_Category_ID)
        {
            DataTable dt = new DataTable();
            try
            {
                string mysql;

                mysql = @" SELECT DISTINCT 
                         Store.Store_Name, Item.id AS Item_Id, Item.code, Item.Item_Name, Item.S_id, Item.Item_Category_ID, Item.No3eboa, Item.Active_flage, 
                         Item_Category.Category_Name, unit.u_name, unit.B_Unit, unit.S_unit, unit.B_Unit_Value, unit.S_unit_Value, Store.StoreUsed_Type, 
                         Unit_item.id AS Unit_item_Id
FROM            Item INNER JOIN
                         Store ON Item.S_id = Store.id INNER JOIN
                         Unit_item ON Item.id = Unit_item.Item_id AND Item.S_id = Unit_item.Store_id INNER JOIN
                         Item_Category ON Item.Item_Category_ID = Item_Category.ID INNER JOIN
                         unit ON Unit_item.unit_id = unit.id   ";

                mysql = mysql + "WHERE    (Item_Category.IS_Active = 1)  AND (Item.Active_flage = " + Active_flage + ") AND (Store.StoreUsed_Type = " + StoreUsed_Type + ") AND (Item.Item_Category_ID = " + Item_Category_ID + ")   ";

                mysql = mysql + "  ORDER BY Item.Item_Category_ID ";

                operations opp = new operations();
                dt = opp.return_DataTable(mysql);

                return dt;
            }
            catch (Exception ee)
            {



            }
            return dt;
        }
        public void Update_No3eboa_To_1_From0(ref operations opp)
        {
            try
            {
                //operations opp = new operations();
                string mysql = " Update Item Set No3eboa=1 where No3eboa=0";
                opp.insert_me(mysql);
            }
            catch { }
        }

        public void AddUnitItem(ref int excp, ref string msg, long Store_Id_From, long Store_Id_To, DataSet ds_item)
        {

            string mysql = "";
            operations opp = new operations(1);
            mysql = "SELECT  Unit_item.* FROM Unit_item WHERE (Store_id = " + Store_Id_From + ") AND (active_flage = 1)";
            DataTable dt = opp.return_DataTable(mysql);

            try
            {
                for (int x = 0; x < ds_item.Tables[0].Rows.Count; x++)
                {

                    long Item_Id = long.Parse(ds_item.Tables[0].Rows[x]["Item_Id"].ToString());

                    mysql = "SELECT  Unit_item.* FROM Unit_item WHERE (Store_id = " + Store_Id_To + ") AND (Item_Id=" + Item_Id + ") AND(active_flage = 1)";
                    DataTable dt_2 = opp.return_DataTable(mysql);
                    if (dt_2.Rows.Count > 0)
                    {
                        continue;
                    }

                    int Store_Type = int.Parse(dt.Rows[x]["Store_Type"].ToString());

                    string Code = dt.Rows[x]["Code"].ToString();

                    long unit_id = long.Parse(dt.Rows[x]["unit_id"].ToString());
                    decimal Big_unit = decimal.Parse(dt.Rows[x]["Big_unit"].ToString());
                    decimal Small_unit = decimal.Parse(dt.Rows[x]["Small_unit"].ToString());


                    decimal Upon_Request = decimal.Parse(dt.Rows[x]["Upon_Request"].ToString());
                    decimal S_Price = decimal.Parse(dt.Rows[x]["S_Price"].ToString());
                    decimal P_Price = decimal.Parse(dt.Rows[x]["P_Price"].ToString());
                    int active_flage = 1;

                    mysql = "INSERT INTO Unit_item (Item_id, Store_id, Store_Type, Code, unit_id, Full_qua, D_Qua, Item_No, Reserved_Balance_Larg_Qnty, Reserved_Balance_Small_Qnty, Big_unit,Small_unit, P_price, S_price, Upon_Request,active_flage)";
                    mysql = mysql + "VALUES(" + Item_Id + ", " + Store_Id_To + ", " + Store_Type + ",'" + Code + "', " + unit_id + ", 0, 0,0, 0,0, " + Big_unit + "," + Small_unit + ", " + P_Price + "," + S_Price + "," + Upon_Request + "," + active_flage + ")";
                    opp.insert_me(mysql);
                }
                opp.Commit();
                excp = 0;
                msg = "تمت العملية بنجاح";

            }
            catch (Exception ee)
            {
                opp.Rollback();
                excp = 1;

                msg = "خطأ فى إجراء العملية";
                msg = msg + "\n\r";

                msg = msg + "\n\r" + ee.Message;


            }
        }
        public static void Add_ALLItemFromTo_Store(long Store_Id_To)
        {
            string mysql = "";
            operations opp = new operations(1);

            try
            {
                mysql = @" INSERT INTO Unit_item (Item_id, Store_id, Store_Type, Code, unit_id, Full_qua, D_Qua, Item_No, Reserved_Balance_Larg_Qnty, Reserved_Balance_Small_Qnty, Big_unit, Small_unit, P_price, S_price, Upon_Request)";
                mysql = mysql + @" SELECT  Item_id," + Store_Id_To + " as Store_id, Store_Type, Code, unit_id, 0, 0, 0, Reserved_Balance_Larg_Qnty, Reserved_Balance_Small_Qnty, Big_unit, Small_unit, P_price, S_price, Upon_Request ";
                mysql = mysql + @" FROM(SELECT Item_id, Store_id, Store_Type, Code, unit_id, Full_qua, D_Qua, Item_No, Reserved_Balance_Larg_Qnty, Reserved_Balance_Small_Qnty, Big_unit, Small_unit, P_price, S_price, Upon_Request,
                ROW_NUMBER() OVER (PARTITION BY Item_Id ORDER BY Item_Id) AS RowNumber
        FROM            Unit_item AS Unit_item_1 INNER JOIN
                         Store ON Unit_item_1.Store_id = Store.id ";

                if (Common.Dived_ByStore_Type_In_Invoice_Type == 0)
                {
                    mysql = mysql + @"   WHERE        Store.StoreUsed_Type = 1 ";
                }

                mysql = mysql + @"  ) AS a WHERE   a.RowNumber = 1 ";

                opp.Commit();
                // MessageBox.Show("تمت العملية بنجاح");
            }
            catch (Exception ee)
            {
                opp.Rollback();
                string Msg = "خطأ فى إجراء العملية";
                Msg = Msg + "\n\r";

                Msg = Msg + "\n\r" + ee.Message;

                // MessageBox.Show(Msg);
            }
        }

        public decimal ComputeUpdateP_AvgCost(ref operations opp, Unit_item uitm, decimal Qnty, decimal PriceUnit, decimal Larg_Qnty, decimal Small_Qnty)
        {
            decimal P_priceAvgCostNew = 0;
            decimal P_priceAvgCost = uitm.P_priceAvgCost ?? 0;
            string mysql = "";

            //checked costAvg If value =0
            Items itm = opp.db.Item.AsNoTracking().Where(i => i.Id == uitm.Item_Id).FirstOrDefault();
            if (itm != null)
            {
                itm.P_priceAvgCost = itm.P_priceAvgCost ?? 0;
                if (itm.P_priceAvgCost > 0)
                {
                    //يتم تحديث متوسط التكلفة بتكلفة الصنف فى حالة صفر او نال
                    mysql = $@"UPDATE       Unit_item
SET                P_priceAvgCost = Item.P_priceAvgCost
FROM            Item INNER JOIN
                         Unit_item ON Item.id = Unit_item.Item_id
WHERE        (Unit_item.P_priceAvgCost = 0 OR
                         Unit_item.P_priceAvgCost IS NULL) ";
                    //AND (Unit_item.Item_id = {uitm.Item_Id}
                    opp.insert_me(mysql);
                }
                else
                {
                    //يتم تحديث متوسط التكلفة بسعر الشراء الجديد فى حالة صفر او نال
                    List<Unit_item> uitmLst = opp.db.Unit_item.AsNoTracking().Where(i => i.Item_Id == uitm.Item_Id & (i.P_priceAvgCost == 0 | i.P_priceAvgCost == null)).ToList();
                    foreach (var itmL in uitmLst)
                    {
                        if (itmL.P_priceAvgCost == 0 | itmL.P_priceAvgCost == null)
                        {
                            mysql = " UPDATE Unit_item  SET P_priceAvgCost =    " + PriceUnit + "  WHERE (Id = " + itmL.id + ") ";
                            opp.insert_me(mysql);
                        }
                    }
                }
            }

            if (P_priceAvgCost > 0)
            {

                decimal Full_qua = uitm.Full_qua ?? 0;
                decimal D_Qua = uitm.D_Qua ?? 0;
                decimal TotalQntyOld = Common.Get_Item_Quantity(Full_qua, D_Qua, uitm.Small_unit ?? 1);


                decimal NewFull_qua = Full_qua + Larg_Qnty;
                decimal NewD_Qua = D_Qua + Small_Qnty;

                decimal TotalQntyNew = Common.Get_Item_Quantity(NewFull_qua, NewD_Qua, uitm.Small_unit ?? 1);
                //decimal TotalQnty = TotalQntyNew + TotalQntyOld;

                decimal Full_qua_CostValueOld = TotalQntyOld * P_priceAvgCost;
                decimal Full_qua_CostValueNew = Qnty * PriceUnit;
                decimal Total_CostValueNew = Full_qua_CostValueNew + Full_qua_CostValueOld;

                if (TotalQntyNew > 0)
                {
                    P_priceAvgCostNew = Total_CostValueNew / TotalQntyNew;
                }
                else
                {
                    P_priceAvgCostNew = PriceUnit;

                }
                P_priceAvgCostNew = Common.RoundMe(P_priceAvgCostNew);
                mysql = " UPDATE Unit_item  SET P_priceAvgCost =    " + P_priceAvgCostNew + "  WHERE (id = " + uitm.id + ") ";
                opp.insert_me(mysql);
            }
            else
            {
                P_priceAvgCostNew = PriceUnit;
                mysql = " UPDATE Unit_item  SET P_priceAvgCost =    " + P_priceAvgCostNew + "  WHERE (Item_Id = " + uitm.Item_Id + ") ";
                opp.insert_me(mysql);
            }

            //////// Update Item
            try
            {
                decimal TotalValue = 0;
                decimal TotalQnty = 0;
                List<Unit_item> uitmLst = opp.db.Unit_item.AsNoTracking().Where(i => i.Item_Id == uitm.Item_Id).ToList();
                foreach (var itmL in uitmLst)
                {
                    Qnty = Common.Get_Item_Quantity(itmL.Full_qua ?? 0, itmL.D_Qua ?? 0, itmL.Small_unit ?? 1);
                    P_priceAvgCost = itmL.P_priceAvgCost ?? 0;
                    decimal Total = Qnty * P_priceAvgCost;

                    TotalValue = TotalValue + Total;
                    TotalQnty = TotalQnty + Qnty;

                }
                decimal ItemP_priceAvgCost = 0;
                if (TotalQnty > 0)
                {
                    ItemP_priceAvgCost = TotalValue / TotalQnty;
                }
                else
                {
                    ItemP_priceAvgCost = PriceUnit;
                }
                ItemP_priceAvgCost = Common.RoundMe(ItemP_priceAvgCost);
                mysql = $@"UPDATE Item SET P_priceAvgCost ={ItemP_priceAvgCost} WHERE (Id = {uitm.Item_Id})";
                opp.insert_me(mysql);

            }
            catch (Exception ee)
            {

            }
            return P_priceAvgCostNew;

        }
        public decimal GetComputed_P_AvgCost(ref operations opp, long Item_Id)
        {
            decimal P_priceAvgCostNew = 0;
            int SupplierFromTo = 1;
            decimal TotalQntyNew = 0;
            decimal TotalCost = 0;

            //IQueryable<Detailes_Purshing_Import_Export> dpLst = opp.db.Detailes_Purshing_Import_Exports.Where(d => d.Type_From == SupplierFromTo | d.Type_To == SupplierFromTo);
            //dpLst = dpLst.Where(d => d.Unit_item.Item_Id == Item_Id).Include(u => u.Unit_item);

            IQueryable<Detailes_Purshing_Import_Export> dpLst = opp.db.Detailes_Purshing_Import_Exports.Where(d => d.Unit_item.Item_Id == Item_Id);
            dpLst = dpLst.Where(d => d.Unit_item.Item_Id == Item_Id).Include(u => u.Unit_item);

            foreach (var item in dpLst)
            {
                decimal Quantity = item.Quantity ?? 0;
                decimal PriceUnit = item.PriceUnit ?? 0;
                decimal Qnty_value = item.Qnty_value ?? 0;

                Qnty_value = Quantity * PriceUnit;

                if (item.Transaction_Type == 1)
                {
                    TotalQntyNew = TotalQntyNew + Quantity;
                    TotalCost = TotalCost + Qnty_value;
                }
                if (item.Transaction_Type == 2)
                {
                    TotalQntyNew = TotalQntyNew - Quantity;
                    TotalCost = TotalCost - Qnty_value;
                }

            }


            //////// get first Balance

            IQueryable<Item_First_Period_Balance> FirstBalanceLst = opp.db.Item_First_Period_Balance;
            FirstBalanceLst = FirstBalanceLst.Where(d => d.Unit_item.Item_Id == Item_Id).Include(u => u.Unit_item);
            foreach (var item in FirstBalanceLst)
            {
                decimal Quantity = Common.Get_Item_Quantity(item.Larg_Qnty, item.Small_Qnty, item.Unit_item.Small_unit ?? 1);
                decimal PriceUnit = item.S_price ?? 0;
                decimal P_priceAvgCost = item.P_priceAvgCost ?? 0;
                decimal P_price = item.P_price ?? 0;
                decimal Qnty_value = Quantity * P_priceAvgCost;


                TotalQntyNew = TotalQntyNew + Quantity;
                TotalCost = TotalCost + Qnty_value;
            }

            if (TotalQntyNew != 0)
            {
                P_priceAvgCostNew = TotalCost / TotalQntyNew;
            }
            else
            {
                P_priceAvgCostNew = 0;
            }
            P_priceAvgCostNew = Common.RoundMe(P_priceAvgCostNew);
            string mysql = " UPDATE  item  SET P_priceAvgCost = " + P_priceAvgCostNew + "  WHERE (id = " + Item_Id + ") ";
            opp.insert_me(mysql);

            return P_priceAvgCostNew;
        }

        public void AddService(ref int excp, ref string msg, Items item, Unit_item um)
        {
            if (Common.UseAutomaticItemCode == 1)
            {
                im.code = GetNewItemCode(ref opp).ToString();
                long InternalCode = 0;

                bool tt = long.TryParse(im.code, out InternalCode);
                im.InternalCode = InternalCode;
                um.Code = im.code;
            }
            /////GETITEM CODE
            item.Active_flage = 1;
            um.active_flage = 1;
            opp.db.Item.Add(item);
            opp.db.SaveChanges();
            um.Item_Id = item.Id;
            opp.db.Unit_item.Add(um);
            opp.db.SaveChanges();

            if (item.Is_UploadToTaxPortal == 0)
            {
                if (itcCls.itemCodeTaxNotUploaded != null)
                {
                    itcCls.itemCodeTaxNotUploaded.Item_Id = item.Id;
                    itcCls.AddItemCodeTaxNotUploaded(ref opp);
                }
            }

            UpdateItemCode(ref opp, item, um);
        }

        public void EditService(ref int excp, ref string msg, Items item, Unit_item um)
        {
            item.Edit_User_Id = Common.User_Id;
            item.EditOperationDate = DateTime.Now;
            um.Edit_User_Id = Common.User_Id;
            um.EditOperationDate = DateTime.Now;
            opp.db.Entry(item).State = EntityState.Modified;
            opp.db.SaveChanges();

            opp.db.Entry(um).State = EntityState.Modified;
            opp.db.SaveChanges();
        }

        public List<ItemsOrderViewModel> GetServicesItemsOnly(long ItemServiceCategoryId, int Is_ItemOrService)
        {
            itmsOrderVMList = new List<ItemsOrderViewModel>();
            try
            {
                im = new Items();
                //itmList = opp.db.Item.Where(i => i.Item_Name.Contains(Name) | i.AnotherName.Contains(Name) | i.ItemName_OnInvoice.Contains(Name)).Include(s => s.Store).Include(u => u.unit).ToList();

                IQueryable<Items> query = opp.db.Item.AsNoTracking()

               .Include(i => i.unit)
               .Include(i => i.Item_Category)
               .Include(i => i.ItemServiceCategory)
              .Where(i => i.Active_flage == 1);

                if (Is_ItemOrService > 0)
                {
                    query = query.Where(i => i.Is_ItemOrService == Is_ItemOrService);
                }



                if (ItemServiceCategoryId > 0)
                {
                    query = query.Where(i => i.ItemServiceCategoryId == ItemServiceCategoryId);
                }
                itmList = query.ToList();
                Set_ItemsViewModel();
                return itmsOrderVMList;
            }
            catch (Exception ee)
            {

                return itmsOrderVMList;

            }
        }
        public async Task<List<ItemsOrderViewModel>> GetServicesItemsOnly_Async(long ItemServiceCategoryId, int Is_ItemOrService)
        {
            // Eager load related data to avoid lazy loading overhead
            var itmList = await opp.db.Item
                .AsNoTracking() // Boost performance (read-only)
                .Include(i => i.unit)
                .Include(i => i.Item_Category)
                .Include(i => i.ItemServiceCategory)
                .ToListAsync();

            // Map using LINQ projection
            Set_ItemsViewModel();
            return itmsOrderVMList; ;
        }

        public List<(long Id, string DisplayName)> GetAllItemsForCombo1(int Is_ItemOrService)
        {



            var q = opp.db.Item.AsQueryable();
            if (Is_ItemOrService > 0)
                q = q.Where(i => i.Is_ItemOrService == Is_ItemOrService & i.Active_flage == 1);

            return q.Select(i => new DisplayItemService { Id = i.Id, Item_Name = i.Item_Name, Is_ItemOrService = i.Is_ItemOrService ?? 0 })
                    .AsEnumerable()
                    .Select(x => (x.Id, x.Item_Name + (x.Is_ItemOrService == 1 ? " (منتج)" : " (خدمة)")))
                    .ToList();
        }
        public List<DisplayItemService> GetAllItemsForCombo(int Is_ItemOrService)
        {



            var q = opp.db.Item.AsQueryable();
            if (Is_ItemOrService > 0)
                q = q.Where(i => i.Is_ItemOrService == Is_ItemOrService & i.Active_flage == 1);

            return q.Select(i => new DisplayItemService { Id = i.Id, Item_Name = i.Item_Name, Is_ItemOrService = i.Is_ItemOrService ?? 0 })
                    .AsEnumerable()
                    .ToList();
        }
    }
}