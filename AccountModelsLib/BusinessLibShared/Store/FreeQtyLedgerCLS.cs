using System;
using System.Linq;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// صف دفتر للكمية المجانية فى فاتورة المبيعات ومرتجعها (قرار المستخدم 2026-10-16: للمستندات الجديدة فقط).
    /// محرك الفاتورة/المرتجع يحرّك Unit_item بالمجانى (LargItemsQnty_Free/SmallItemsQnty_Free) لكن صف الدفتر الذى يكتبه
    /// (Set_ItemTransactionFromInvoice/R) يحمل الكمية المدفوعة فقط؛ فالرصيد المحسوب من الدفتر يزيد عن الفعلى بالمجانى المباع
    /// (وفى المندوبين يظهر عجزاً على المندوب عند الجرد). مسار المستند الجديد فى المحرك يكتب بعد صف المدفوع صفاً جديداً للمجانى
    /// (Engine_Posted، Qty_Small بالإشارة) بنفس مفاتيح سطر المستند (نوع المستند + رقمه + السطر)، فيُحذف مع صفوف السطر عند تعديله
    /// أو حذفه كما يفعل المحرك. قيمة المجانى = 0، والتكلفة للعلم. لا يحرك Unit_item (المحرك حرّكه) ولا يمس أى صف قائم.
    /// مسارات التعديل القديمة لا تكتبه (تحريكها لـ Unit_item بالمجانى غير متسق)، فالمستند المعدَّل يعود دفتره بالمدفوع فقط.
    /// </summary>
    public static class FreeQtyLedgerCLS
    {
        /// <summary>المجانى فى السطر بالوحدة الصغرى.</summary>
        public static decimal FreeSmall(IInvoices_Details d, decimal factor)
            => (d.LargItemsQnty_Free ?? 0) * factor + (d.SmallItemsQnty_Free ?? 0);

        /// <summary>
        /// يكتب صف المجانى لسطر مستند محفوظ. transactionType: 1 وارد (مرتجع)، 2 صادر (بيع).
        /// invoiceTypeId: نوع المستند كما يكتبه المحرك (Common.Invoice_Type_Id أو Common.InvoiceReverse_Type_Id).
        /// </summary>
        public static long Post(operations opp, IInvoices_Details d, long docId, long docNo, int transactionType, int invoiceTypeId,
            long partyId, string partyName, DateTime? date, string processName)
        {
            long unitItemId = d.Unit_Item_Table_Id ?? 0;
            var um = opp.db.Unit_item.AsNoTracking().Where(u => u.id == unitItemId).Select(u => new { u.id, u.Item_Id, u.Store_id, u.Small_unit, u.Full_qua, u.D_Qua, u.Item_No }).FirstOrDefault();
            if (um == null) return 0;
            decimal factor = (um.Small_unit ?? 0) <= 0 ? 1 : um.Small_unit.Value;
            decimal free = FreeSmall(d, factor);
            if (free <= 0) return 0;

            long branchId = opp.db.Stores.AsNoTracking().Where(s => s.Id == um.Store_id).Select(s => s.Branch_Id).FirstOrDefault() ?? Common.Branch_Id;
            decimal avg = opp.db.Item_Branch_Costs.AsNoTracking().Where(c => c.Item_Id == um.Item_Id && c.Branch_Id == branchId).Select(c => (decimal?)c.Avg_Cost).FirstOrDefault() ?? 0;
            decimal large = Math.Floor(free / factor), small = free - large * factor;
            bool import = transactionType == 1;
            DateTime now = DateTime.Now;
            string store = d.Store_Name ?? "";
            var dpie = new Detailes_Purshing_Import_Export
            {
                Id = CallSnowflakeIdGenerate.GenerateNewId(),
                Detailes_Purshing_Id = 0, OrderTable_id = 0,
                Invoice_Id = docId, Invoice_Type_Id = invoiceTypeId, Detail_Invoice_Id = d.Id, Invoice_No = docNo, Invoice_Type_Str = processName,
                Unit_item_Table_Id = um.id,
                qua = large, Part_qua = small, Item_No = d.Item_No_Free ?? 0,
                qua_Import = import ? large : 0, Part_Qua_Import = import ? small : 0, Item_No_Import = import ? d.Item_No_Free ?? 0 : 0, Quantity_Import = import ? free / factor : 0,
                qua_Export = import ? 0 : large, Part_Qua_Export = import ? 0 : small, Item_No_Export = import ? 0 : d.Item_No_Free ?? 0, Quantity_Export = import ? 0 : free / factor,
                Quantity = free / factor, Quantity_Balance = 0, Balance_Larg_Qnty = 0, Balance_Small_Qnty = 0, Balance_Item_No = 0,
                Transaction_Type = transactionType, Process_Date = date ?? now, IS_To_Converted = 0, Machine_Id = 0,
                ProcessNameId = invoiceTypeId, ProcessName = processName, ProcessNameNumber = docNo.ToString(),
                Type_From = import ? Common.Customer_StoreTransactionCode : Common.Store_StoreTransactionCode,
                Type_To = import ? Common.Store_StoreTransactionCode : Common.Customer_StoreTransactionCode,
                Type_From_Name = import ? "عميل" : "مخزن", Type_To_Name = import ? "مخزن" : "عميل",
                Id_From = import ? partyId : d.Store_Id, Id_To = import ? d.Store_Id : partyId,
                Id_FromStr = import ? partyName : store, Id_ToStr = import ? store : partyName,
                Notes_From = import ? partyName : store, Notes_To = import ? store : partyName,
                Notes_From_To = " من " + (import ? partyName : store) + " إلى " + (import ? store : partyName),
                Notes = "كمية مجانية", Notes_Item = "مجانى " + (d.Invoice_Item_Name ?? ""),
                StoreId = d.Store_Id, StoreId_Name = store,
                PriceUnit = 0, PriceUnitImport = 0, PriceUnitExport = 0, Qnty_value = 0, Item_Net_Total_Price = 0, Item_Net_Total_PriceImport = 0, Item_Net_Total_PriceExport = 0,
                S_Price = 0, P_Price = 0, P_priceAvgCost = Math.Round(avg * factor, 2), S_priceAvgCost = 0,
                CarNumber = "", CarsId = 0, CarsWorkOrdersDetailsId = 0, CarsWorkOrdersId = 0, WorkOrderNumber = "", ProcessNo = 0,
                Branch_Id = branchId, User_Id = Common.User_Id, OperationDate = now.Date, Insert_ServerDateTime = now,
            };
            opp.db.Detailes_Purshing_Import_Exports.Add(dpie);
            opp.db.SaveChanges();
            opp.db.Entry(dpie).State = EntityState.Detached;
            decimal signed = import ? free : -free;
            opp.db.Database.ExecuteSqlInterpolated($@"UPDATE Detailes_Purshing_Import_Export SET Unit_Cost = {Math.Round(avg, 6)}, Qty_Small = {Math.Round(signed, 4)},
    Total_Value = {Math.Round(avg * free, 2)}, Avg_Cost_Before = {Math.Round(avg, 6)}, Avg_Cost_After = {Math.Round(avg, 6)}, Engine_Posted = 1 WHERE Id = {dpie.Id}");
            return dpie.Id;
        }

        /// <summary>صف المجانى لسطر فاتورة/مرتجع محفوظ من مسار المستند الجديد فى المحرك (Invoice_Id = invL.Id).</summary>
        public static long PostForInvoice(operations opp, IInvoices_Details d, InvoicesOperationListCLS invL_O, int transactionType, int invoiceTypeId, string processName)
        {
            var h = invL_O.invL;
            return Post(opp, d, h.Id, h.Invoice_Id ?? 0, transactionType, invoiceTypeId, h.Customer_Id ?? 0, h.CustomerName ?? "", h.Invoice_Date, processName);
        }
    }
}
