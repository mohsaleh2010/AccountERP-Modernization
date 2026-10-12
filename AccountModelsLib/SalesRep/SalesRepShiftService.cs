using System;
using System.Linq;
using System.Collections.Generic;
using AccountModelsLib.SalesRep;

namespace AccountModelsLib.SalesRep
{
    public class SalesRepShiftService
    {
        private readonly operations _opp;
        private readonly StoreOrderService _orderService;

        public SalesRepShiftService(operations opp)
        {
            _opp = opp;
            _orderService = new StoreOrderService(opp);
        }

        public SalesRep_Shift OpenShift(long repId, DateTime shiftDate)
        {
            var shift = new SalesRep_Shift
            {
                Id = CallSnowflakeIdGenerate.GenerateNewId(),
                Rep_Id = repId,
                Shift_Date = shiftDate,
                Status = (int)SalesRepShiftStatus.Open,
                Opened_At = DateTime.Now
            };

            _opp.db.SalesRep_Shifts.Add(shift);
            _opp.db.SaveChanges();
            return shift;
        }

        public long LoadVan(long shiftId, StoreOrderService.OrderInput input)
        {
            // إنشاء إذن التحميل باستخدام محرك الأذون الحالي
            long orderId = _orderService.SaveDraft(input);
            _orderService.Post(orderId); // ترحيل الإذن لتحريك المخزون

            // تسجيل الربط مع الوردية
            var link = new SalesRep_Document_Link
            {
                Id = CallSnowflakeIdGenerate.GenerateNewId(),
                Doc_Type = (int)SalesRepDocType.StoreOrder,
                Doc_Id = orderId,
                Shift_Id = shiftId
            };

            _opp.db.SalesRep_Document_Links.Add(link);
            _opp.db.SaveChanges();
            return orderId;
        }

        public long UnloadVan(long shiftId, StoreOrderService.OrderInput input)
        {
            // إنشاء إذن التفريغ (تحويل عكسي)
            long orderId = _orderService.SaveDraft(input);
            _orderService.Post(orderId);

            // تسجيل الربط
            var link = new SalesRep_Document_Link
            {
                Id = CallSnowflakeIdGenerate.GenerateNewId(),
                Doc_Type = (int)SalesRepDocType.StoreOrder,
                Doc_Id = orderId,
                Shift_Id = shiftId
            };

            _opp.db.SalesRep_Document_Links.Add(link);
            _opp.db.SaveChanges();
            return orderId;
        }

        public void CloseShift(long shiftId)
        {
            var shift = _opp.db.SalesRep_Shifts.FirstOrDefault(s => s.Id == shiftId);
            if (shift != null)
            {
                shift.Status = (int)SalesRepShiftStatus.Closed;
                shift.Closed_At = DateTime.Now;
                _opp.db.SaveChanges();
            }
        }
    }
}
