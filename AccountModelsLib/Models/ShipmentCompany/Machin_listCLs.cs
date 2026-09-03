using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AccountModelsLib.Models;

namespace AccountModelsLib
{
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