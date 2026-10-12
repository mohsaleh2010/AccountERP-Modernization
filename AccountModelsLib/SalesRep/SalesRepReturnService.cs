using System;
using System.Collections.Generic;
using System.Linq;
using AccountModelsLib.SalesRep;
using AccountModelsLib.Models;

namespace AccountModelsLib.SalesRep
{
    public class SalesRepReturnService
    {
        private readonly operations _opp;

        public SalesRepReturnService(operations opp)
        {
            _opp = opp;
        }

        public long CreateReturnInvoice(long shiftId, long originalInvoiceId, List<Invoices_Details_Reverse> returnLines)
        {
            // 1. إنشاء المرتجع عبر محرك ERP الحالي
            // ... (استخدام Make_EntryForInvoices_Reverse)
            long returnInvoiceId = CallSnowflakeIdGenerate.GenerateNewId();

            // 2. ربط أسطر المرتجع بالأسطر الأصلية
            foreach (var returnLine in returnLines)
            {
                // التحقق من صلاحية المرتجع (التشغيلة، الكمية، نافذة الـ 30 يوماً)

                var link = new SalesRep_Invoice_Return_Links
                {
                    Id = CallSnowflakeIdGenerate.GenerateNewId(),
                    Return_Detail_Id = returnLine.Id,
                    Original_Detail_Id = 0, // يُحدد من طلب الإرجاع عند الحفظ الفعلي
                    Original_Invoice_Id = originalInvoiceId,
                    Qty_Large = returnLine.Larg_Qnty ?? 0,
                    Condition = 0 // 0: Good, 1: Damaged
                };
                _opp.db.SalesRep_Invoice_Return_Links.Add(link);
            }

            // 3. تسجيل الربط بالوردية
            var shiftLink = new SalesRep_Document_Link
            {
                Id = CallSnowflakeIdGenerate.GenerateNewId(),
                Doc_Type = (int)SalesRepDocType.Return,
                Doc_Id = returnInvoiceId,
                Shift_Id = shiftId
            };
            _opp.db.SalesRep_Document_Links.Add(shiftLink);

            _opp.db.SaveChanges();
            return returnInvoiceId;
        }
    }
}
