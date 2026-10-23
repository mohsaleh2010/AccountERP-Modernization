using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AccountModelsLib.Models;

namespace AccountModelsLib
{
    public class DuplicateMachineNameException : InvalidOperationException
    {
        public string MachineName { get; }

        public DuplicateMachineNameException(string machineName)
            : base("اسم الماكينة \"" + machineName + "\" مستخدم من قبل")
        {
            MachineName = machineName;
        }
    }

    public class Machin_listCLs
    {
        private readonly operations opp;

        public Machin_listCLs()
        {
            opp = new operations();
        }

        /// <summary>
        /// جلب البيانات - يدعم Async لمشاريع الـ Razor Pages والـ WinForms
        /// </summary>
        public async Task<List<Machin_list>> GetDataAsync()
        {
            return await opp.db.Machin_list
                               .AsNoTracking()
                               .Where(m => m.Active == 1)
                               .ToListAsync();
        }

        // نسخة العادية لـ WinForms القديم إذا لم ترد تغيير شاشات WinForms للـ Async
        public List<Machin_list> GetData()
        {
            return opp.db.Machin_list
                         .AsNoTracking()
                         .Where(m => m.Active == 1)
                         .ToList();
        }

        /// <summary>
        /// حفظ/تحديث قائمة الماكينات من الجدول
        /// </summary>
        public async Task UpdateDataTableAsync(List<Machin_list> machines)
        {
            await EnsureUniqueNamesAsync(machines);

            foreach (var item in machines)
            {
                item.Active = 1;

                if (item.Id == 0)
                {
                    // التحديث هنا: استخدام SnowflakeIdGenerate.GenerateNewId()
                    item.Id =CallSnowflakeIdGenerate.GenerateNewId();
                    await opp.db.Machin_list.AddAsync(item);
                }
                else
                {
                    var local = opp.db.Machin_list.Local.FirstOrDefault(e => e.Id == item.Id);
                    if (local != null)
                    {
                        opp.db.Entry(local).State = EntityState.Detached;
                    }

                    opp.db.Machin_list.Update(item);
                }
            }

            await opp.db.SaveChangesAsync();
        }

        /// <summary>
        /// يمنع تكرار اسم الماكينة بين الماكينات الفعالة (بعد حذف المسافات وبدون تفرقة حالة الأحرف)،
        /// سواء داخل نفس الدفعة أو مع المحفوظ بالقاعدة
        /// </summary>
        public async Task EnsureUniqueNamesAsync(List<Machin_list> machines)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in machines)
            {
                item.Machin_Name = (item.Machin_Name ?? "").Trim();
                if (!seen.Add(item.Machin_Name))
                    throw new DuplicateMachineNameException(item.Machin_Name);
            }

            var names = seen.ToList();
            var ids = machines.Where(m => m.Id != 0).Select(m => m.Id).ToList();
            var existing = await opp.db.Machin_list
                                       .AsNoTracking()
                                       .Where(m => m.Active == 1 && names.Contains(m.Machin_Name.Trim()) && !ids.Contains(m.Id))
                                       .Select(m => m.Machin_Name)
                                       .FirstOrDefaultAsync();
            if (existing != null)
                throw new DuplicateMachineNameException(existing.Trim());
        }

        /// <summary>
        /// حذف منطقي (Soft Delete)
        /// </summary>
        public async Task DeleteMachin_listAsync(long id)
        {
            var machine = await opp.db.Machin_list.FirstOrDefaultAsync(m => m.Id == id);
            if (machine != null)
            {
                machine.Active = 0;
                await opp.db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// إضافة ماكينة مباشرة (للاستخدام في نماذج الإدخال المباشرة)
        /// </summary>
        public async Task Add_MachinAsync(string machineName, decimal priceOfNazlla, string code = null, decimal? speed = null, int? machineWidth = null, string description = null, decimal? abstractHadafat = null)
        {
            machineName = (machineName ?? "").Trim();
            await EnsureUniqueNamesAsync(new List<Machin_list> { new Machin_list { Id = 0, Machin_Name = machineName } });

            var newMachine = new Machin_list
            {
                Id = CallSnowflakeIdGenerate.GenerateNewId(), // استخدام الميثود الجديدة
                Machin_Name = machineName,
                price_of_nazlla = priceOfNazlla,
                Code = code,
                Speed = speed,
                Machine_Width = machineWidth,
                Description = description,
                Abstract_Hadafat = abstractHadafat,
                Active = 1
            };

            await opp.db.Machin_list.AddAsync(newMachine);
            await opp.db.SaveChangesAsync();
        }

        /// <summary>
        /// تعديل ماكينة مباشرة
        /// </summary>
        public async Task Update_MachineAsync(long id, string machineName, decimal priceOfNazlla, string code = null, decimal? speed = null, int? machineWidth = null, string description = null, decimal? abstractHadafat = null)
        {
            machineName = (machineName ?? "").Trim();
            await EnsureUniqueNamesAsync(new List<Machin_list> { new Machin_list { Id = id, Machin_Name = machineName } });

            var machine = await opp.db.Machin_list.FirstOrDefaultAsync(m => m.Id == id);
            if (machine != null)
            {
                machine.Machin_Name = machineName;
                machine.price_of_nazlla = priceOfNazlla;
                machine.Code = code;
                machine.Speed = speed;
                machine.Machine_Width = machineWidth;
                machine.Description = description;
                machine.Abstract_Hadafat = abstractHadafat;

                await opp.db.SaveChangesAsync();
            }
        }
    }
}