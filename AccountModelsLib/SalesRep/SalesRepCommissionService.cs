using System;
using System.Linq;
using AccountModelsLib.SalesRep;
using AccountModelsLib.Models;

namespace AccountModelsLib.SalesRep
{
    public class SalesRepCommissionService
    {
        private readonly operations _opp;

        public SalesRepCommissionService(operations opp)
        {
            _opp = opp;
        }

        // حساب العمولة بناءً على الأهداف المحققة (مرحلة 6)
        public decimal CalculateCommission(long repId, DateTime from, DateTime to)
        {
            var targets = _opp.db.SalesRep_Targets
                .Where(t => t.Rep_Id == repId && t.Period_From >= from && t.Period_To <= to)
                .ToList();

            decimal totalCommission = 0;

            foreach (var target in targets)
            {
                // 1. جلب ما تم تحقيقه
                decimal achieved = GetAchievedValue(repId, target.Scope, target.Scope_Id, from, to);

                // 2. تطبيق خطط العمولة والشرائح
                var plan = _opp.db.SalesRep_Commission_Plans.FirstOrDefault();
                if (plan != null)
                {
                    // [إضافة]: خصم المرتجعات إذا كانت الخطة تقضى بذلك
                    if (plan.Discount_Returns)
                    {
                        decimal returns = GetReturnedValue(repId, from, to);
                        achieved -= returns;
                    }

                    totalCommission += CalculateTieredCommission(plan.Id, achieved, target.Target_Value);
                }
            }

            return totalCommission;
        }

        private decimal GetReturnedValue(long repId, DateTime from, DateTime to)
        {
            // خصم المرتجعات المرتبطة بمندوب (عبر الوردية)
            return _opp.db.SalesRep_Document_Links
                .Where(l => l.Doc_Type == (int)SalesRepDocType.Return &&
                            _opp.db.SalesRep_Shifts.Any(s => s.Id == l.Shift_Id && s.Rep_Id == repId) &&
                            _opp.db.Invoices_List_Reverse.Any(i => i.Id == l.Doc_Id && i.OperationDate >= from && i.OperationDate <= to))
                .Sum(l => _opp.db.Invoices_List_Reverse.FirstOrDefault(i => i.Id == l.Doc_Id).Net_Total ?? 0);
        }

        private decimal GetAchievedValue(long repId, int scope, long scopeId, DateTime from, DateTime to)
        {
            // احتساب إجمالي المبيعات المحققة للمندوب من فواتير المبيعات المربوطة بوردياته خلال الفترة
            var saleDocIds = _opp.db.SalesRep_Document_Links
                .Where(l => l.Doc_Type == (int)SalesRepDocType.Invoice &&
                            _opp.db.SalesRep_Shifts.Any(s => s.Id == l.Shift_Id && s.Rep_Id == repId))
                .Select(l => l.Doc_Id)
                .ToList();

            if (!saleDocIds.Any()) return 0;

            var query = _opp.db.Invoices_List
                .Where(i => saleDocIds.Contains(i.Id) && i.OperationDate >= from && i.OperationDate <= to);

            return query.Sum(i => i.Net_Total ?? 0);
        }

        private decimal CalculateTieredCommission(long planId, decimal achieved, decimal target)
        {
            if (target == 0) return 0;
            decimal percent = (achieved / target) * 100;

            var tier = _opp.db.SalesRep_Commission_Plans_Tiers
                .Where(t => t.Plan_Id == planId && percent >= t.Target_Percent_From && percent <= t.Target_Percent_To)
                .FirstOrDefault();

            return tier != null ? (achieved * (tier.Commission_Rate / 100)) : 0;
        }
    }
}
