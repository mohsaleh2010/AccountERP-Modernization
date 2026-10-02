using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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
    /// 🟢 كود أعمال شاشة المخازن لواجهة Razor (بطلب المستخدم: فى AccountModelsLib). يقابل Store_LIB.Store_ListFrm ويعيد استخدام StoreCLS/Sub_Account القائمتين بلا تعديل،
    /// مع إصلاح ثلاث نقاط فى مسارات الأصل (بموافقة المستخدم): (1) StoreCLS.GetStoreAccount_Type_Value تقرأ الحقل store بدل الوسيط فيرجع دائماً حساب «إنتاج تام»؛
    /// هنا يُحسب نوع الحساب من نوع المخزن فعلاً (2=خامات، 3=غير مكتمل، غير ذلك=إنتاج تام). (2) StoreCLS.UpdateStore تنسخ كل الحقول فتمسح SubAccount_Id؛ هنا يُحدَّث
    // الحقول المعروضة فقط. (3) StoreCLS.EmptyItemsStore يحذف المخزن نفسه بعد حذف أصنافه؛ هنا تفريغ الأصناف فقط ويبقى المخزن. منع تكرار اسم المخزن فى نفس الفرع (طلب المستخدم).
    /// </summary>
    public class StoresGridCLS
    {
        /// <summary>أنواع المخزن (StoreUsed_Type) - نفس قائمة Stores_ItemType.StoreTypeList (هى نفسها لقيمتى Project_Market).</summary>
        public List<StoreLookupItem> GetStoreTypes()
        {
            return new List<StoreLookupItem>
            {
                new StoreLookupItem { Value = 1, Text = "إنتاج تام - مبيعات" },
                new StoreLookupItem { Value = 2, Text = "مشتريات أو خامات أو مستلزمات و ...." },
                new StoreLookupItem { Value = 3, Text = "منتج غير مكتمل التصنيع" },
            };
        }

        /// <summary>أنواع المخزن الرئيسية - LookUp_StoreOrderTypes حيث IsStoreMainType=1 (Stores_ListType.GetStoreMainType(1,false)).</summary>
        public List<StoreLookupItem> GetMainTypes()
        {
            var opp = new operations();
            var dt = opp.return_DataTable("Select Id, Name from LookUp_StoreOrderTypes Where IsStoreMainType = 1 ORDER BY Id");
            return dt.Rows.Cast<DataRow>().Select(r => new StoreLookupItem { Value = Convert.ToInt64(r["Id"]), Text = r["Name"]?.ToString() ?? "" }).ToList();
        }

        /// <summary>المخازن الفعالة للفرع (Stores_List.StoreListData(0)): IS_Default_Store DESC, Store_Name, StoreUsed_Type.</summary>
        public List<StoreRow> GetStores(long branchId)
        {
            var opp = new operations();
            var dt = opp.return_DataTable(
                @"SELECT s.Id, ISNULL(s.Store_Name, '') AS Store_Name, ISNULL(s.Store_Code, '') AS Store_Code, ISNULL(s.Store_location, '') AS Store_location,
                         ISNULL(s.Employee_Name, '') AS Employee_Name, ISNULL(s.StoreUsed_Type, 0) AS StoreUsed_Type, ISNULL(s.LookUp_StoreOrderTypesId, 0) AS LookUp_StoreOrderTypesId,
                         ISNULL(t.Name, '') AS MainTypeName, ISNULL(s.IS_Default_Store, 0) AS IS_Default_Store, ISNULL(s.IS_Sell_SecondUnit, 0) AS IS_Sell_SecondUnit,
                         ISNULL(s.IS_ItemSecond_degree, 0) AS IS_ItemSecond_degree, ISNULL(s.IS_Virtual_OuterStore, 0) AS IS_Virtual_OuterStore,
                         ISNULL(s.IS_ServicesVirtual_Item_NoBalance, 0) AS IS_ServicesVirtual_Item_NoBalance, ISNULL(s.SubAccount_Id, 0) AS SubAccount_Id, ISNULL(s.Branch_Id, 0) AS Branch_Id
                  FROM Store s LEFT OUTER JOIN LookUp_StoreOrderTypes t ON t.Id = s.LookUp_StoreOrderTypesId
                  WHERE s.Active = 1 AND s.Branch_Id = " + branchId + " ORDER BY ISNULL(s.IS_Default_Store, 0) DESC, s.Store_Name, s.StoreUsed_Type");
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

        public static int AccountTypeValueFor(int storeUsedType)
        {
            if (storeUsedType == 2) return Common.Account_Type_ForStoreKhamat;
            if (storeUsedType == 3) return Common.Account_Type_ForStoreNotComplete;
            return Common.Account_Type_ForStoreEntagTam;
        }

        private static bool NameExists(string name, long branchId, long exceptId)
        {
            var opp = new operations();
            var dt = opp.return_DataTable("Select Id, Store_Name from Store Where Active = 1 AND Branch_Id = " + branchId + " AND Id <> " + exceptId);
            return dt.Rows.Cast<DataRow>().Any(r => string.Equals((r["Store_Name"]?.ToString() ?? "").Trim(), name.Trim(), StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>إضافة مخزن (يقابل BtnAddNew_Click). Excp: 0 نجاح، 3 اسم مكرر فى نفس الفرع، غير ذلك خطأ (Msg).</summary>
        public void AddStore(ref int Excp, ref string Msg, Store store)
        {
            Excp = 0;
            Msg = "";
            if (NameExists(store.Store_Name ?? "", store.Branch_Id ?? 0, 0))
            {
                Excp = 3;
                return;
            }
            var opp = new operations(2);
            try
            {
                var cls = new StoreCLS();
                cls.AddNewStore(ref Excp, ref Msg, ref opp, store, AccountTypeValueFor(store.StoreUsed_Type ?? 0));
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

        /// <summary>تعديل مخزن (يقابل BtnEdit_Click) للحقول المعروضة فقط. لو أُعلن افتراضياً يُلغى الافتراضى عن باقى مخازن نفس النوع كما فى StoreCLS.UpdateStore.</summary>
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
                    return;
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
                existing.Edit_User_Id = store.User_Id;
                opp.db.SaveChanges();
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
            }
        }

        /// <summary>حذف مخزن (يقابل BtnDelete_Click): Sub_Account.Delete_Account بنوع الحساب الصحيح. Excp=10 ← لا يُحذف (Msg من الأصل).</summary>
        public void DeleteStore(ref int Excp, ref string Msg, long Store_Id)
        {
            Excp = 0;
            Msg = "";
            try
            {
                var st = new StoreCLS().GetStoreAsNotracking(Store_Id);
                if (st == null)
                {
                    Excp = 1;
                    Msg = "السجل غير موجود أو تم حذفه.";
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

        /// <summary>تفريغ أصناف المخزن (يقابل btnempty_Click) - نفس عبارتَى حذف الأصل لكن <b>دون</b> حذف صف المخزن نفسه.</summary>
        // 🐛 كانت تحذف الأصناف بلا أى تحقق سابق حتى لو كانت مستخدَمة فعلياً فى فواتير/مرتجعات/أذون صرف
        // واستلام - منع صريح بطلب المستخدم، نفس شروط StoreCLS.EmptyItemsStore (نسخة الويندوز فورمز).
        public void EmptyStoreItems(ref int Excp, ref string Msg, long Store_Id)
        {
            try
            {
                var opp = new operations();

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
    }
}
