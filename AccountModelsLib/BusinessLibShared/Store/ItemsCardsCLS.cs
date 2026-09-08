using System;
using System.Data;

namespace AccountModelsLib.BusinessLibShared.Store
{
    // 🟢 نسخة Blazor من Store_LIB.ItemsCardsCLS (مشروع WinForms فقط، UseWindowsForms=true) - نفس نمط نقل
    // CurrencyCLS/Journal_DaysListCLS سابقاً. النسخة الأصلية فى Store_LIB لم تُمَس ولا تزال تُستخدَم كما هى
    // من شاشات الويندوز فورمز الأربع (ItemsCardsStore/ItemsCardsStore_Days). الاستعلامات والشروط هنا هى
    // نفسها بالضبط بدون أي تغيير فى منطق العمل، تغيير واحد فقط: استبدال MessageBox.Show (فى
    // Get_Items_Data الأصلية) بـ ref string Msg (نفس نمط باقى الكلاسات المنقولة).
    public class ItemsCardsCLS
    {
        public DataSet Get_AllItemCardStore(ref int Excp, ref string Msg, int Store_Id, int StoreUsed_Type_Items, bool _dtFrom, DateTime dtFrom, bool _dtTo, DateTime dtTo, int Item_Category_ID, int SortBy)
        {
            //SortBy = 1; if  By Category
            //SortBy = 2;    SortBy Date

            string mysql = @"  SELECT        Item.Item_Name, Detailes_Purshing.qua, Detailes_Purshing.Part_qua, Detailes_Purshing.Balance_Id_From_Larg_Qnty, Detailes_Purshing.Balance_Id_From_Small_Qnty, Detailes_Purshing.Balance_Id_From_Item_No,
                         Detailes_Purshing.Balance_Id_To_Larg_Qnty, Detailes_Purshing.Balance_Id_To_Small_Qnty, Detailes_Purshing.Balance_Id_To_Item_No, unit.u_name, unit.B_Unit, unit.S_unit, unit.B_Unit_Value, unit.S_unit_Value,
                         Unit_item.id, Unit_item.Item_id, Unit_item.Store_id, Unit_item.Store_Type, Unit_item.Code, Unit_item.unit_id, Unit_item.Full_qua, Unit_item.D_Qua, Unit_item.Item_No, Unit_item.Reserved_Balance_Larg_Qnty,
                         Unit_item.Reserved_Balance_Small_Qnty, Unit_item.Big_unit, Unit_item.Small_unit, Unit_item.P_price, Unit_item.S_price, Unit_item.Specification, Unit_item.Upon_Request, Unit_item.Currency,
                         Detailes_Purshing.Item_No AS Detailes_Purshing_Item_No, Order_Purshing.Order_Purshing_No, Order_Purshing.Order_date, Order_Purshing.Order_Type, Detailes_Purshing.Unit_item_Table_Id,
                         Detailes_Purshing.To_Store_Unit_Item_Table_Id, Order_Purshing.type_From, Order_Purshing.Id_From, Order_Purshing.Type_To, Order_Purshing.Id_To, Order_Purshing.Invoice_ID, Order_Purshing.Cust_Supplier_id,
                         Order_Purshing.endd, Order_Purshing.end_order, Detailes_Purshing.OrderTable_id, Detailes_Purshing.Notes_From, Detailes_Purshing.Notes_To, Detailes_Purshing.Notes, Detailes_Purshing.Machine_Id,
                         Item_Category.Category_Name, Item.No3eboa, Item.Item_Category_ID, DataDictionaryProcessesLists.ProcessName AS Order_Name, DataDictionaryProcessesLists.OrderTypes_Value,
                         DataDictionaryProcessesLists.BtnRecive_Text, DataDictionaryProcessesLists.Order_Estlam_Sarf, DataDictionaryProcessesLists.Case_Name
FROM            Item INNER JOIN
                         Unit_item ON Item.id = Unit_item.Item_id INNER JOIN
                         unit ON Unit_item.unit_id = unit.id INNER JOIN
                         Order_Purshing INNER JOIN
                         Detailes_Purshing ON Order_Purshing.Id = Detailes_Purshing.OrderTable_id ON Unit_item.id = Detailes_Purshing.Unit_item_Table_Id INNER JOIN
                         Item_Category ON Item.Item_Category_ID = Item_Category.ID INNER JOIN
                         Store ON Unit_item.Store_id = Store.id INNER JOIN
                         DataDictionaryProcessesLists ON Order_Purshing.Order_Type = DataDictionaryProcessesLists.Id
";
            mysql = mysql + " WHERE Order_Purshing.Is_Converted  = 0 ";
            if (Store_Id > 0)
            {
                mysql = mysql + " AND ((Order_Purshing.Id_From =" + Store_Id + ") OR  (Order_Purshing.Id_To =" + Store_Id + "))  ";
            }

            if (StoreUsed_Type_Items > 0)
            {
                mysql = mysql + " AND (Store.StoreUsed_Type = " + StoreUsed_Type_Items + ") ";
            }
            if (_dtFrom == true)
            {
                mysql = mysql + " AND  (Order_Purshing.Order_date>='" + AccountModelsLib.operations.dateFormat(dtFrom) + "')   ";
            }
            if (_dtTo == true)
            {
                mysql = mysql + "   AND  (Order_Purshing.Order_date<='" + AccountModelsLib.operations.dateFormat(dtTo) + "')  ";
            }
            if (Item_Category_ID != -1)
            {
                mysql = mysql + " AND (Item.Item_Category_ID = " + Item_Category_ID + ") ";
            }

            if (SortBy == 1)
            {
                mysql = mysql + " ORDER BY Item.Item_Category_ID,Item.Id ";
            }
            else if (SortBy == 2)
            {
                mysql = mysql + " ORDER BY Order_Purshing.Order_date DESC ";
            }
            else
            {
                mysql = mysql + " ORDER BY Order_Purshing.Order_date DESC ";
            }

            return Get_Items_Data(ref Excp, ref Msg, mysql);
        }

        private DataSet Get_Items_Data(ref int Excp, ref string Msg, string mysql)
        {
            DataSet G_DS = new DataSet();
            try
            {
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                G_DS = opp.return_dataset(mysql);
                Excp = 1;
                return G_DS;
            }
            catch (Exception ee)
            {
                Excp = -1;
                Msg = ee.Message;
                return G_DS;
            }
        }

        // 🟢 جديدتان: تقابلان استعلامَى Connection4It.Stores_List.Store_DataByStoreUsed_Type و
        // Connection4It.ItemCategory.Item_Data حرفياً (كلا الكلاسين عنصرا تحكم WinForms فقط، وليسا كلاسَى
        // منطق عمل CLS - لم تكونا موجودتين كدالة CLS مستقلة أصلاً) - ضرورة تقنية لتعبئة قوائم الفلترة فى
        // Blazor بنفس بيانات الأصل بالضبط، بدون أي قاعدة عمل جديدة.
        public DataTable Get_Stores_By_UsedType(int StoreUsed_Type)
        {
            var opp = new AccountModelsLib.operations();
            string mysql = "Select * from Store   Where     Active =1   ";
            if (StoreUsed_Type > 0)
            {
                mysql = mysql + "and  StoreUsed_Type= " + StoreUsed_Type;
            }
            mysql = mysql + "  ORDER BY StoreUsed_Type, Store_Name ";
            return opp.return_DataTable(mysql);
        }

        public DataTable Get_Item_Categories(int Category_Type_Id, bool FilterByType)
        {
            var opp = new AccountModelsLib.operations();
            string mysql = " Select * from Item_Category Where   (IS_Active = 1) ";
            if (FilterByType && AccountModelsLib.Common.Divid_ItemCatygoryByType_Id == 1)
            {
                mysql = mysql + " and Category_Type_Id=" + Category_Type_Id + "";
            }
            return opp.return_DataTable(mysql);
        }
    }
}
