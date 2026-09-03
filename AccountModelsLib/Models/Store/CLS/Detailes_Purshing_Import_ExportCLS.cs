using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace AccountModelsLib.Models 
{
    public  class Detailes_Purshing_Import_ExportCLS
    {
        public Detailes_Purshing_Import_ExportCLS()
        {
            
        }
        public void Add(ref operations opp, Detailes_Purshing_Import_Export dpie)
        {
            if (dpie != null)
            {
                opp.db.Detailes_Purshing_Import_Exports.Add(dpie);
                try
                {
                    opp.db.SaveChanges();
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
                {
                    // فحص السجلات المعلقة بداخل الـ Context والتي سببت الأزمة
                    foreach (var entry in ex.Entries)
                    {
                        var entityName = entry.Entity.GetType().Name;
                        var entityState = entry.State;
                        // ضع هنا Breakpoint وسترى اسم الجدول المسبب للأزمة وحالته (هل هو Invoice_Details مثلاً؟)
                        string cc = $"الكيان المنهار: {entityName} وحالته البرمجية: {entityState}";
                        System.Diagnostics.Debug.WriteLine($"الكيان المنهار: {entityName} وحالته البرمجية: {entityState}");
                    }

                }
            }

        }
        public void Update(ref operations opp, Detailes_Purshing_Import_Export dpie)
        {
            if (dpie != null)
            {
                opp.db.Entry(dpie).State = EntityState.Modified;
                
                opp.db.SaveChanges();
            }

        }
    }
}
