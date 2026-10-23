﻿
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
    /// <summary>صف قائمة المخازن (يقابل بيانات Store_LIB.Store_ListFrm).</summary>
    public class StoreRow
    {
        public long Id { get; set; }
        public string Store_Name { get; set; } = "";
        public string Store_Code { get; set; } = "";
        public string Store_location { get; set; } = "";
        public string Employee_Name { get; set; } = "";
        public int StoreUsed_Type { get; set; }
        public long LookUp_StoreOrderTypesId { get; set; }
        public string MainTypeName { get; set; } = "";
        public int IS_Default_Store { get; set; }
        public int IS_Sell_SecondUnit { get; set; }
        public int IS_ItemSecond_degree { get; set; }
        public int IS_Virtual_OuterStore { get; set; }
        public int IS_ServicesVirtual_Item_NoBalance { get; set; }
        public long SubAccount_Id { get; set; }
        public long Branch_Id { get; set; }
    }

    public class StoreLookupItem
    {
        public long Value { get; set; }
        public string Text { get; set; } = "";
    }

    /// <summary>
    /// 🟢 كود أعمال المخازن الموحَّد للويندوز فورمز (Store_LIB.Store_ListFrm) وواجهة Razor (StoresList.razor) - كان مقسوماً على
    /// StoreCLS (الديسك توب) وStoresGridCLS (الويب) فدُمج هنا بطلب المستخدم حتى يمر الاثنان بنفس الكود. أُصلح أثناء الدمج:
    /// (1) نوع الحساب من نوع المخزن: كان النوع 1 (إنتاج تام) يقع فى else فيأخذ حساب «إنتاج تحت التشغيل» 59 بدل 56، ولا يُنشأ له صف مخزن
    ///     لأن Sub_Account لا يعرف 59 كنوع مخزن - الآن 1=56، 2=57، 3=58، 4=59، وSub_Account يعامل 59 كمخزن.
    /// (2) التعديل يحدِّث الحقول المعروضة فقط (كان SetValues ينسخ كل الحقول فيمسح SubAccount_Id) ويمنع تكرار الاسم فى نفس الفرع.
    /// (3) تفريغ الأصناف لا يحذف المخزن نفسه. (4) الحذف كان يستعلم عن جدول Store_Id غير الموجود فيفشل دائماً.
    /// </summary>
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

        /// <summary>أنواع المخزن (StoreUsed_Type) - نفس قائمة Stores_ItemType.StoreTypeList (هى نفسها لقيمتى Project_Market).</summary>
        public List<StoreLookupItem> GetStoreTypes()
        {
            return new List<StoreLookupItem>
            {
                new StoreLookupItem { Value = 1, Text = "إنتاج تام - مبيعات" },
                new StoreLookupItem { Value = 2, Text = "مشتريات أو خامات أو مستلزمات و ...." },
                new StoreLookupItem { Value = 3, Text = "منتج غير مكتمل التصنيع" },
                new StoreLookupItem { Value = 4, Text = "الإنتاج تحت التشغيل" },
            };
        }

        /// <summary>أنواع المخزن الرئيسية - LookUp_StoreOrderTypes حيث IsStoreMainType=1 (Stores_ListType.GetStoreMainType(1,false)).</summary>
        public List<StoreLookupItem> GetMainTypes()
        {
            var dt = new operations().return_DataTable("Select Id, Name from LookUp_StoreOrderTypes Where IsStoreMainType = 1 ORDER BY Id");
            return dt.Rows.Cast<DataRow>().Select(r => new StoreLookupItem { Value = Convert.ToInt64(r["Id"]), Text = r["Name"]?.ToString() ?? "" }).ToList();
        }

        /// <summary>المخازن الفعالة للفرع (Stores_List.StoreListData(0)): IS_Default_Store DESC, Store_Name. storeUsedType=0 = كل الأنواع.</summary>
        public List<StoreRow> GetStores(long branchId, int storeUsedType = 0)
        {
            string typeFilter = storeUsedType > 0 ? " AND s.StoreUsed_Type = " + storeUsedType : "";
            var dt = new operations().return_DataTable(
                @"SELECT s.Id, ISNULL(s.Store_Name, '') AS Store_Name, ISNULL(s.Store_Code, '') AS Store_Code, ISNULL(s.Store_location, '') AS Store_location,
                         ISNULL(s.Employee_Name, '') AS Employee_Name, ISNULL(s.StoreUsed_Type, 0) AS StoreUsed_Type, ISNULL(s.LookUp_StoreOrderTypesId, 0) AS LookUp_StoreOrderTypesId,
                         ISNULL(t.Name, '') AS MainTypeName, ISNULL(s.IS_Default_Store, 0) AS IS_Default_Store, ISNULL(s.IS_Sell_SecondUnit, 0) AS IS_Sell_SecondUnit,
                         ISNULL(s.IS_ItemSecond_degree, 0) AS IS_ItemSecond_degree, ISNULL(s.IS_Virtual_OuterStore, 0) AS IS_Virtual_OuterStore,
                         ISNULL(s.IS_ServicesVirtual_Item_NoBalance, 0) AS IS_ServicesVirtual_Item_NoBalance, ISNULL(s.SubAccount_Id, 0) AS SubAccount_Id, ISNULL(s.Branch_Id, 0) AS Branch_Id
                  FROM Store s LEFT OUTER JOIN LookUp_StoreOrderTypes t ON t.Id = s.LookUp_StoreOrderTypesId
                  WHERE s.Active = 1 AND s.Branch_Id = " + branchId + typeFilter + " ORDER BY StoreUsed_Type,ISNULL(s.IS_Default_Store, 0) DESC, s.Store_Name, s.StoreUsed_Type");
            return dt.Rows.Cast<DataRow>().Select(r => new StoreRow
            {
                Id = Convert.ToInt64(r["Id"]),
                Store_Name = r["Store_Name"].ToString() ?? "",
                Store_Code = r["Store_Code"].ToString() ?? "",
                Store_location = r["Store_location"].ToString() ?? "",
                Employee_Name = r["Employee_Name"].ToString() ?? "",
                StoreUsed_Type = Convert.ToInt32(r["StoreUsed_Type"]),
                LookUp_StoreOrderTypesId = Convert.ToInt64(r["LookUp_StoreOrderTypesId"]),
                MainTypeName = r["MainTypeName"].ToString() ?? "",
                IS_Default_Store = Convert.ToInt32(r["IS_Default_Store"]),
                IS_Sell_SecondUnit = Convert.ToInt32(r["IS_Sell_SecondUnit"]),
                IS_ItemSecond_degree = Convert.ToInt32(r["IS_ItemSecond_degree"]),
                IS_Virtual_OuterStore = Convert.ToInt32(r["IS_Virtual_OuterStore"]),
                IS_ServicesVirtual_Item_NoBalance = Convert.ToInt32(r["IS_ServicesVirtual_Item_NoBalance"]),
                SubAccount_Id = Convert.ToInt64(r["SubAccount_Id"]),
                Branch_Id = Convert.ToInt64(r["Branch_Id"]),
            }).ToList();
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

        /// <summary>نوع حساب المخزن فى شجرة الحسابات حسب نوع المخزن (StoreUsed_Type).</summary>
        public static int AccountTypeValueFor(int StoreUsed_Type)
        {
            switch (StoreUsed_Type)
            {
                case 2: return Common.Account_Type_ForStoreKhamat;
                case 3: return Common.Account_Type_ForStoreNotComplete;
                case 4: return Common.Account_Type_ForWIP;
                default: return Common.Account_Type_ForStoreEntagTam;
            }
        }

        public int GetStoreAccount_Type_Value(int StoreUsed_Type)
        {
            return AccountTypeValueFor(StoreUsed_Type);
        }

        private static bool NameExists(string name, long branchId, long exceptId)
        {
            var dt = new operations().return_DataTable("Select Id, Store_Name from Store Where Active = 1 AND Branch_Id = " + branchId + " AND Id <> " + exceptId);
            return dt.Rows.Cast<DataRow>().Any(r => string.Equals((r["Store_Name"]?.ToString() ?? "").Trim(), name.Trim(), StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>إضافة مخزن مع حسابه فى الشجرة فى معاملة واحدة. Excp: 0 نجاح، 3 اسم مكرر فى نفس الفرع، غير ذلك خطأ (Msg).</summary>
        public void AddStore(ref int Excp, ref string Msg, Store store)
        {
            Excp = 0;
            Msg = "";
            if (NameExists(store.Store_Name ?? "", store.Branch_Id ?? 0, 0))
            {
                Excp = 3;
                Msg = "اسم المخزن مستخدم من قبل فى نفس الفرع";
                return;
            }
            var opp = new operations(2);
            try
            {
                AddNewStore(ref Excp, ref Msg, ref opp, store, AccountTypeValueFor(store.StoreUsed_Type ?? 0));
                var acc = new Sub_Account();
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
            catch (Exception ee)
            {
                try { opp.Rollback(); } catch { }
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
            }
        }

        // يُستدعى من شاشة الويندوز فورمز - نفس مسار الويب
        public void AddNewStore(ref int Excp, ref string Msg, Store store, bool IsUse_SubAccount_Id_Select)
        {
            AddStore(ref Excp, ref Msg, store);
        }
        public void AddNewStore(ref int Excp, ref string Msg, ref operations opp, Store store, int Account_Type_Value)
        {
            Excp = 0;
            Msg = "";
            string Account_Code = "";
            Sub_Account acc = new Sub_Account();

            long    Currency_Id = 1;
            long Account_Branch_Id = store.Branch_Id ?? 1;
            long New_Submain_Account_Id = -1;

            acc.store = store;
            acc.Add_AccountSupplierCustomerOther(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, ref Account_Code, Account_Type_Value, store.Store_Name, Account_Branch_Id, Currency_Id);
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

        /// <summary>تعديل مخزن من شاشة المخازن (ويب وديسك توب) للحقول المعروضة فقط، مع منع تكرار الاسم فى نفس الفرع.
        /// لو أُعلن افتراضياً يُلغى الافتراضى عن باقى مخازن نفس النوع. Excp: 0 نجاح، 3 اسم مكرر، غير ذلك خطأ (Msg).</summary>
        public void UpdateStore(ref int Excp, ref string Msg, Store store, long Store_Id)
        {
            Excp = 0;
            Msg = "";
            try
            {
                var opp = new operations();
                var existing = opp.db.Stores.Where(s => s.Id == Store_Id).FirstOrDefault();
                if (existing == null)
                {
                    Excp = 1;
                    Msg = "السجل غير موجود أو تم حذفه.";
                    return;
                }
                if (NameExists(store.Store_Name ?? "", existing.Branch_Id ?? 0, Store_Id))
                {
                    Excp = 3;
                    Msg = "اسم المخزن مستخدم من قبل فى نفس الفرع";
                    return;
                }
                if ((existing.StoreUsed_Type ?? 0) != (store.StoreUsed_Type ?? 0) && (existing.SubAccount_Id ?? 0) > 0)
                {
                    MoveStoreAccountToType(ref Excp, ref Msg, existing.SubAccount_Id ?? 0, store.StoreUsed_Type ?? 0, existing.Branch_Id ?? 1);
                    if (Excp != 0) return;
                }
                if (store.IS_Default_Store == 1)
                {
                    foreach (var s in opp.db.Stores.Where(s => s.StoreUsed_Type == store.StoreUsed_Type && s.IS_Default_Store == 1 && s.Id != Store_Id).ToList())
                    {
                        s.IS_Default_Store = 0;
                    }
                }
                existing.Store_Name = store.Store_Name;
                existing.Store_Code = store.Store_Code;
                existing.Store_location = store.Store_location;
                existing.Employee_Name = store.Employee_Name;
                existing.StoreUsed_Type = store.StoreUsed_Type;
                existing.LookUp_StoreOrderTypesId = store.LookUp_StoreOrderTypesId;
                existing.IS_Default_Store = store.IS_Default_Store;
                existing.IS_Sell_SecondUnit = store.IS_Sell_SecondUnit;
                existing.IS_ItemSecond_degree = store.IS_ItemSecond_degree;
                existing.IS_Virtual_OuterStore = store.IS_Virtual_OuterStore;
                existing.IS_ServicesVirtual_Item_NoBalance = store.IS_ServicesVirtual_Item_NoBalance;
                existing.Update_ServerDateTime = DateTime.Now;
                existing.EditOperationDate = DateTime.Now;
                existing.Edit_User_Id = Common.User_Id;
                opp.db.SaveChanges();
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
            }
        }

        /// <summary>
        /// عند تغيير نوع المخزن: يصير نوع حسابه نوع المخزن الجديد ويُنقل تحت الحساب الأب لهذا النوع (Account_Type_Main_Account_Id)
        /// مع إعادة تكويده عبر Sub_Account.Move_Account. لو لا أب معرَّف للنوع فى الفرع يتغير النوع فقط ويبقى الحساب مكانه.
        /// </summary>
        private static void MoveStoreAccountToType(ref int Excp, ref string Msg, long SubAccount_Id, int StoreUsed_Type, long Branch_Id)
        {
            var opp = new operations();
            int accountType = AccountTypeValueFor(StoreUsed_Type);
            long parentId = 0;
            int costCenter = 0;
            Common.Get_Account_Type_Code(ref opp, accountType, 0, ref parentId, ref costCenter, Branch_Id);
            var account = opp.db.Submain_Account.FirstOrDefault(a => a.Id == SubAccount_Id);
            if (account == null) return;
            if (parentId > 0 && account.Main_Account_Id != parentId)
            {
                new Sub_Account().Move_Account(ref Excp, ref Msg, SubAccount_Id, parentId);
                if (Excp != 0) return;
                account = opp.db.Submain_Account.FirstOrDefault(a => a.Id == SubAccount_Id);
            }
            account.Account_Customer_Type_Value = accountType;
            opp.db.SaveChanges();
        }

        // يُستدعى من Sub_Account عند تعديل حساب المخزن من شجرة الحسابات
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

        /// <summary>تفريغ أصناف المخزن (أصناف المخزن الافتراضية وأرصدة Store_Type=4) دون حذف المخزن نفسه.</summary>
        // 🐛 كانت تحذف الأصناف بلا أى تحقق سابق حتى لو كانت مستخدَمة فعلياً فى فواتير/مرتجعات/أذون صرف واستلام -
        // المنع صريح بطلب المستخدم: لا تفريغ لأى صنف له صف فى الجداول الخمسة (بيع، شراء، مرتجع بيع، مرتجع شراء، حركة صرف/استلام مخزنى).
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

                opp.insert_me(" DELETE FROM item where S_id=" + Store_Id + "; DELETE FROM Unit_item where Store_Type=4 and Store_id=" + Store_Id + ";");
                Excp = 0;

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
            }
        }

        /// <summary>حذف مخزن من شاشة المخازن مع حسابه: Sub_Account.Delete_Account بنوع الحساب الصحيح من نوع المخزن المحفوظ.
        /// Excp=10 ← لا يُحذف (يوجد أصناف/أرصدة).</summary>
        public void DeleteStore(ref int Excp, ref string Msg, long Store_Id)
        {
            Excp = 0;
            Msg = "";
            try
            {
                var st = GetStoreAsNotracking(Store_Id);
                if (st == null)
                {
                    Excp = 1;
                    Msg = "السجل غير موجود أو تم حذفه.";
                    return;
                }
                // التحقق قبل حذف الحساب: Sub_Account يحذف الحساب أولاً ثم المخزن بلا معاملة، فرفض حذف المخزن بعدها كان يترك مخزناً بلا حساب
                var checkOpp = new operations();
                if (!CanDeleteStore(ref Excp, ref Msg, ref checkOpp, Store_Id, st.SubAccount_Id ?? 0))
                {
                    return;
                }
                new Sub_Account().Delete_Account(ref Excp, ref Msg, AccountTypeValueFor(st.StoreUsed_Type ?? 0), Store_Id, st.SubAccount_Id ?? 0);
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
            }
        }
        /// <summary>هل يمكن حذف المخزن: لا أرصدة أصناف فعالة، وليس مخزناً افتراضياً لأصناف، وليس المخزن الافتراضى لنوعه. Excp=10 + Msg عند الرفض.</summary>
        public bool CanDeleteStore(ref int Excp, ref string Msg, ref operations opp, long Store_Id, long SubAccount_Id = 0)
        {
            string mysql = "Select * from Unit_Item Where active_flage=1 and Store_Id=" + Store_Id;
            DataTable dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count > 0)
            {
                Excp = 10;
                Msg = "لايمكن حذف المخزن بسبب وجود أصناف داخل المخزن";
                return false;
            }

            mysql = "Select * from Item Where S_id=" + Store_Id + " ";
            dt = new DataTable();
            dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                Excp = 10;
                Msg = "لايمكن حذف المخزن - المخزن إفتراضى للأصناف";
                return false;
            }

            // المخزن الافتراضى لا يُحذف - يُرفض صراحة بدل أن يبقى المخزن ويُحذف حسابه
            mysql = "Select Id from Store Where ISNULL(IS_Default_Store, 0) = 1 AND (Id=" + Store_Id + " OR (" + SubAccount_Id + " <> 0 AND SubAccount_Id=" + SubAccount_Id + "))";
            if (opp.return_DataTable(mysql).Rows.Count > 0)
            {
                Excp = 10;
                Msg = "لايمكن حذف المخزن الافتراضى - حدد مخزناً افتراضياً آخر لنفس النوع أولاً";
                return false;
            }

            return true;
        }

        public void DeleteStore(ref int Excp, ref string Msg, ref operations opp, long Store_Id, long SubAccount_Id = 0)
        {
            try
            {

                if (!CanDeleteStore(ref Excp, ref Msg, ref opp, Store_Id, SubAccount_Id))
                {
                    return;
                }

                // كان الجدول مكتوباً Store_Id (غير موجود) فيفشل الحذف دائماً
                string mysql = "delete from Store where Id= " + Store_Id;
                if (SubAccount_Id != 0)
                    mysql = mysql + " delete from Store where SubAccount_Id= " + SubAccount_Id;
                opp.insert_me(mysql);
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
            // كان Excp يُصفَّر فى النهاية فيضيع رفض الحذف (أصناف داخل المخزن) ويُحذف الحساب مع بقاء المخزن
            Excp = 0;
            try
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
            st.Edit_User_Id = Common.User_Id;

            st.ProcessNo = 0;

            opp.db.Stores.Add(st);
            opp.db.SaveChanges();

            return st;
        }
    }

}
