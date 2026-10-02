
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AccountModelsLib.Models
{
    public class StoreCLS
    {
        public Store store = new Store();
        operations opp;
        public StoreCLS()
        {
            opp = new operations();

        }
        public StoreCLS(ref operations opp_)
        {
            opp = opp_;

        }
        public DataTable GetAllStoresWithout(long Store_Id, int StoreUsed_Type)
        {

            string mysql = "";

            operations opp = new operations();
            mysql = "Select * from Store Where  id <> " + Store_Id + " AND StoreUsed_Type=" + StoreUsed_Type + " And IS_ItemSecond_degree=0";
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }


        public Store GetStoreService()
        {
            Store store = new Store();
            store = opp.db.Stores.Where(s => s.IS_ServicesVirtual_Item_NoBalance == 1 & s.IS_Default_Store == 1).FirstOrDefault();

            return store;
        }
        public Store GetStoreAsNotracking(long Id)
        {
            Store store = new Store();
            store = opp.db.Stores.AsNoTracking().Where(s => s.Id==Id).FirstOrDefault();

            return store;
        }

        public Store CheckServiceStoreFound()
        {
            Store st = GetStoreService();

            if (st != null)
            {
                return st;
            }
            else
            {
                st = AddNewServiceStore();
                return st;
            }
        }

        public int GetStoreAccount_Type_Value(int StoreUsed_Type)
        {
            int Account_Type_Value = 0;
            if (store.StoreUsed_Type == 2)
            {
                Account_Type_Value = Common.Account_Type_ForStoreKhamat;
            }
            else if (store.StoreUsed_Type == 3)
            {
                Account_Type_Value = Common.Account_Type_ForStoreNotComplete;
            }
            else
            {
                Account_Type_Value = Common.Account_Type_ForStoreEntagTam;

            }

            return Account_Type_Value;
        }
        public void AddNewStore(ref int Excp, ref string Msg, Store store, bool IsUse_SubAccount_Id_Select)
        {
            //cs.opp.BeginTransaction(2);
            operations opp = new operations(2);
            int Account_Type_Value = GetStoreAccount_Type_Value(store.StoreUsed_Type ?? 0);
            AddNewStore(ref Excp, ref Msg, ref opp, store, Account_Type_Value);
            Sub_Account acc = new Sub_Account();
            acc.UpdateSubmain_SubAccount_Id(ref Excp, ref opp, ref Msg);

            if (Excp == 0)
            {
                opp.Commit();
            }
            else
            {
                opp.Rollback();

            }
        }
        public void AddNewStore(ref int Excp, ref string Msg, ref operations opp, Store store, int Account_Type_Value)
        {
            //Emp.Add(new Employees(" إنتاج تام - مبيعات", "1"));

            //Emp.Add(new Employees("مشتريات أو  خامات أو مستلزمات و .... ـ", "2"));
            //Emp.Add(new Employees(" منتج غير مكتمل التصنيع", "3"));

            // int Account_Type_Value = 0;
            //if (store.StoreUsed_Type==2)
            // {
            //     Account_Type_Value = Common.Account_Type_ForStoreKhamat ;
            // } 
            // else if (store.StoreUsed_Type == 3)
            // {
            //     Account_Type_Value = Common.Account_Type_ForStoreNotComplete;
            // }
            // else
            // {
            //     Account_Type_Value = Common.Account_Type_ForStoreEntagTam;

            // }
            Excp = 0;
            Msg = "";
            string Account_Code = "";
            Sub_Account acc = new Sub_Account();

            long    Currency_Id = 1;
            long Account_Branch_Id = store.Branch_Id ?? 1;
            long New_Submain_Account_Id = -1;

            acc.store = store;
            acc.Add_AccountSupplierCustomerOther(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, ref Account_Code, Account_Type_Value, store.Store_Name, Account_Branch_Id, Currency_Id);

            //store.Id = acc.New_Account_Id;
            //store.SubAccount_Id = New_Submain_Account_Id;


        }
        public long AddNewServiceStore(ref int excp, ref string msg, ref operations opp, Submain_Account sa_)
        {

            if (store.IS_Default_Store == 1)
            {
                var sts = opp.db.Stores.Where(s => s.StoreUsed_Type == store.StoreUsed_Type && s.IS_Default_Store == 1);
                foreach (var s in sts)
                {
                    s.IS_Default_Store = 0;
                }
            }
            if (Common.Use_Auto_Identity_Column == 0)
            {
                store.Id = sa_.Id;

            }
            store.Store_Name = sa_.SubAccount_Name;
            store.SubAccount_Id = sa_.Id;



            opp.db.Stores.Add(store);
            opp.db.SaveChanges();

            return store.Id;
        }
        public void UpdateStore(ref int Excp, ref string Msg, Store store, long Store_Id)
        {
            try
            {
                operations opp = new operations();
                if (store.IS_Default_Store == 1)
                {
                    var sts = opp.db.Stores.Where(s => s.StoreUsed_Type == store.StoreUsed_Type && s.IS_Default_Store == 1);
                    foreach (var s in sts)
                    {
                        s.IS_Default_Store = 0;
                    }
                }

                var existingEntity = opp.db.Stores.Where(s => s.Id == Store_Id).FirstOrDefault();
                if (existingEntity == null)
                {
                    Excp = 1;
                    Msg = "السجل غير موجود أو تم حذفه.";
                    return;
                }

                // تحديث القيم المطلوبة فقط
                opp.db.Entry(existingEntity).CurrentValues.SetValues(store);

                //opp.db.Entry(store).State = EntityState.Modified;
                opp.db.SaveChanges();
                Excp = 0;

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }
        }
        public long UpdateStore(ref int Excp, ref string Msg, ref operations opp, Store store)
        {
            try
            {

                if (store.IS_Default_Store == 1)
                {
                    var sts = opp.db.Stores.Where(s => s.StoreUsed_Type == store.StoreUsed_Type && s.IS_Default_Store == 1);
                    foreach (var s in sts)
                    {
                        s.IS_Default_Store = 0;
                    }
                }

                var existingEntity = opp.db.Stores.Where(s => s.Id == store.Id).FirstOrDefault();
                if (existingEntity == null)
                {
                    Excp = 1;
                    Msg = "السجل غير موجود أو تم حذفه.";
                    return 0;
                }

                // تحديث القيم المطلوبة فقط
                opp.db.Entry(existingEntity).CurrentValues.SetValues(store);

                //opp.db.Entry(store).State = EntityState.Modified;
                opp.db.SaveChanges();
                Excp = 0;
                return store.Id;

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
                return 0;
            }
        }
        // 🐛 هذه العملية تحذف أصناف المخزن (جدول Item) والمخزن نفسه نهائياً بلا أى تحقق سابق - قبل هذا
        // الإصلاح كانت تحذف حتى لو كانت الأصناف مستخدَمة فعلياً فى فواتير/مرتجعات/أذون صرف واستلام، مما
        // يفقد تلك المستندات مرجعها للصنف. المنع هنا صريح بطلب المستخدم: لا تفريغ لأى صنف تابع لهذا المخزن
        // له أى صف فى الجداول الخمسة التالية (بيع، شراء، مرتجع بيع، مرتجع شراء، حركة صرف/استلام مخزنى).
        public void EmptyItemsStore(ref int Excp, ref string Msg, long Store_Id)
        {
            try
            {
                operations opp = new operations();

                string checkSql = @"
SELECT TOP 1 Item.id
FROM Item
WHERE Item.S_id = " + Store_Id + @"
AND (
    EXISTS (SELECT 1 FROM Invoices_Details WHERE Invoices_Details.Item_Id = Item.id)
    OR EXISTS (SELECT 1 FROM Invoices_Details_Reverse WHERE Invoices_Details_Reverse.Item_Id = Item.id)
    OR EXISTS (SELECT 1 FROM Bills_Details WHERE Bills_Details.Item_Id = Item.id)
    OR EXISTS (SELECT 1 FROM Bills_Details_Reverse WHERE Bills_Details_Reverse.Item_Id = Item.id)
    OR EXISTS (SELECT 1 FROM Detailes_Purshing WHERE Detailes_Purshing.Item_Id = Item.id)
)";
                DataTable dtCheck = opp.return_DataTable(checkSql);
                if (dtCheck.Rows.Count > 0)
                {
                    Excp = 1;
                    Msg = "لا يمكن تفريغ أصناف هذا المخزن - يوجد صنف واحد على الأقل مرتبط بفاتورة بيع أو شراء أو مرتجع أو إذن صرف/استلام مخزنى.";
                    return;
                }

                string mysql = " DELETE FROM  item where  S_id=" + Store_Id;
                mysql = mysql + " DELETE FROM  Unit_item where Store_Type=4 and  Store_id=" + Store_Id;
                mysql = mysql + "DELETE FROM  Store where  Id=" + Store_Id;
                opp.insert_me(mysql);

                Excp = 0;

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }
        }

        public void DeleteStore(ref int Excp, ref string Msg, long Store_Id)
        {
            operations opp = new operations();
            DeleteStore(ref Excp, ref Msg, ref opp, Store_Id);
        }
        public void DeleteStore(ref int Excp, ref string Msg, ref operations opp, long Store_Id, long SubAccount_Id = 0)
        {
            try
            {

                string mysql = "Select * from Unit_Item Where active_flage=1 and Store_Id=" + Store_Id;
                DataTable dt = opp.return_DataTable(mysql);

                if (dt.Rows.Count > 0)
                {
                    Excp = 10;
                    Msg = "لايمكن حذف المخزن بسبب وجود أصناف داخل المخزن";
                    return;
                }

                mysql = "Select * from Item Where S_id=" + Store_Id + " ";
                dt = new DataTable();
                dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    Excp = 10;
                    Msg = "لايمكن حذف المخزن - المخزن إفتراضى للأصناف";
                    return;
                }



                //mysql = "DELETE FROM Store WHERE (id =" + Store_Id + " )";
                //mysql = "Update  Store Set Active =0 WHERE (id =" + Store_Id + " ) AND (IS_Default_Store <> 1)";
                //int c = opp.insert_me(mysql);

                mysql = "delete from Store_Id where  (IS_Default_Store <> 1) AND  Id= " + Store_Id;
                mysql = mysql + "delete from Store_Id where (IS_Default_Store <> 1) AND  SubAccount_Id= " + SubAccount_Id;
                int c = opp.insert_me(mysql);
                Excp = 0;

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }



        }
        public void MakeDeleteStore(ref operations opp, ref int Excp, ref string Msg, long Store_Id, long SubAccount_Id)
        {
            string mysql = "";
            if (Common.Update_DeleteSupplier == 0)
            {
                mysql = " Update Store set active=0 Where Id= " + Store_Id;
                mysql = mysql + " Update Store set active=0 Where SubAccount_Id= " + SubAccount_Id;
                opp.insert_me(mysql);
            }
            if (Common.Update_DeleteSupplier == 1)
            {


                DeleteStore(ref Excp, ref Msg, ref opp, Store_Id, SubAccount_Id);
            }
            try
            {

                Excp = 0;
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
            }
        }

        private Store AddNewServiceStore()
        {
            Store st = new Store();
            st.Store_Name = "قائمة الخدمات";
            st.IS_ServicesVirtual_Item_NoBalance = 1;
            st.IS_Default_Store = 1;
            st.Store_location = "";
            st.Emp_id = 0;
            st.Job_id = 0;
            st.StoreUsed_Type = 1;
            st.Branch_Id = Common.Branch_Id;
            st.User_Id = Common.User_Id;
            st.Employee_Name = "";
            st.Store_Code = "0";

            st.IS_Sell_SecondUnit = 0;
            st.IS_ItemSecond_degree = 0;
            st.Active = 1;
            st.IsCar = 0;
            st.IS_Virtual_OuterStore = 0;
            st.OperationDate = DateTime.Now;

            st.Insert_ServerDateTime = DateTime.Now;
            st.Update_ServerDateTime = DateTime.Now;
            st.EditOperationDate = DateTime.Now;
            st.Edit_User_Id = 1;

            st.ProcessNo = 0;

            opp.db.Stores.Add(st);
            opp.db.SaveChanges();

            return st;
        }
    }

}
