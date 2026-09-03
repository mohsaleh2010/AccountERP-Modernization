using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
 

namespace AccountModelsLib.Services
{
    public class DrawsServiceCls
    {
        private readonly operations _opp;

        public DrawsServiceCls()
        {
            _opp = new operations();
        }

        public DrawsServiceCls(operations opp)
        {
            _opp = opp;
        }

        /// <summary>
        /// جلب جميع الأسعار بدون تتبع في الذاكرة لضمان السرعة الأفضل
        /// </summary>
        public async Task<List<Draws_List>> GetAllAsync()
        {
            return await _opp.db.Set<Draws_List>()
                                .AsNoTracking()
                                .ToListAsync();
        }

        /// <summary>
        /// إضافة رسمة جديدة باستخدام سياق عمل مستقل لمنع مشاكل التتبع
        /// </summary>
        public async Task AddAsync(string name, decimal price)
        {
            using (var opp = new operations())
            {
                var item = new Draws_List
                {
                    Id = CallSnowflakeIdGenerate.GenerateNewId() , // توليد ID جديد بأمان
                    Draw_Name = name,
                    Price = price
                };//

                await opp.db.Set<Draws_List>().AddAsync(item);
                await opp.db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// تعديل بيانات الرسمة والسعر
        /// </summary>
        public async Task UpdateAsync(long id, string name, decimal price)
        {
            using (var opp = new operations())
            {
                var item = await opp.db.Set<Draws_List>().FirstOrDefaultAsync(d => d.Id == id);
                if (item != null)
                {
                    item.Draw_Name = name;
                    item.Price = price;
                    opp.db.Entry(item).State = EntityState.Modified;
                    await opp.db.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// حذف رسمة معينة حسب المعرف
        /// </summary>
        public async Task DeleteAsync(long id)
        {
            using (var opp = new operations())
            {
                var item = await opp.db.Set<Draws_List>().FirstOrDefaultAsync(d => d.Id == id);
                if (item != null)
                {
                    opp.db.Set<Draws_List>().Remove(item);
                    await opp.db.SaveChangesAsync();
                }
            }
        }/// <summary>
         /// جلب كافة البيانات وتجهيزها للعرض داخل تقرير RDLC
         /// </summary>
        public async Task<List<DrawsReportDTO>> GetReportDataAsync()
        {
            using (var opp = new operations())
            {
                var list = await opp.db.Set<Draws_List>()
                                   .AsNoTracking()
                                   .ToListAsync();

                int serial = 0;
                return list.Select(item => new DrawsReportDTO
                {
                    Serial = ++serial,
                    Id = item.Id,
                    Draw_Name = item.Draw_Name,
                    Price = item.Price
                }).ToList();
            }
        }
    }
}