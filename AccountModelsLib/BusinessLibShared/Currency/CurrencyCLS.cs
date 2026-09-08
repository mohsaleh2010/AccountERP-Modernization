using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountModelsLib.BusinessLibShared.Currency
{
    public class CurrencyCLS
    {
        private readonly operations  opp;

        public Models.Currency? SelectedCurrency { get; set; }
        public List<Models.Currency> CurrenciesList { get; private set; } = new();

        public CurrencyCLS(operations opp_)
        {
            opp = opp_;
        }
        public CurrencyCLS( )
        {
            opp =  new operations();
        }
        #region Read Operations

        /// <summary>
        /// جلب كافة العملات من قاعدة البيانات مرتبة حسب قيمة الـ Enum.
        /// </summary>
        public async Task<List<Models.Currency>> GetAllCurrenciesAsync()
        {
            CurrenciesList = await opp.db.Currency
                .AsNoTracking()
                .OrderBy(c => c.Value)
                .ToListAsync();

            return CurrenciesList;
        }

        /// <summary>
        /// جلب العملات النشطة فقط (Active == 1).
        /// </summary>
        public async Task<List<Models.Currency>> GetActiveCurrenciesAsync()
        {
            return await opp.db.Currency
                .AsNoTracking()
                .Where(c => c.Active == 1)
                .OrderBy(c => c.Value)
                .ToListAsync();
        }

        /// <summary>
        /// جلب مصفوفة قيم العملات النشطة للعمليات المالية والحسابية.
        /// </summary>
        public async Task<List<int>> GetActiveCurrencyValuesAsync()
        {
            return await opp.db.Currency
                .AsNoTracking()
                .Where(c => c.Active == 1)
                .OrderBy(c => c.Value)
                .Select(c => c.Value)
                .ToListAsync();
        }

        /// <summary>
        /// استخراج المعرف التالي تلقائياً للعملة الجديدة.
        /// </summary>
        public async Task<long> GetNextIdAsync()
        {
            if (await opp.db.Currency.AnyAsync())
            {
                return await opp.db.Currency.MaxAsync(c => c.Id) + 1;
            }
            return 1;
        }

        /// <summary>
        /// فلترة العملات المحملة بالذاكرة بناءً على نص البحث.
        /// </summary>
        public List<Models.Currency> FilterCurrencies(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return CurrenciesList;

            keyword = keyword.Trim().ToLower();

            return CurrenciesList.Where(c =>
                (c.Currency_Name != null && c.Currency_Name.ToLower().Contains(keyword)) ||
                (c.Currency_Name_EN != null && c.Currency_Name_EN.ToLower().Contains(keyword)) ||
                (c.code != null && c.code.ToLower().Contains(keyword)) ||
                c.Value.ToString().Contains(keyword)
            ).ToList();
        }

        #endregion

        #region Write Operations (Add / Edit / Delete)

        /// <summary>
        /// حفظ أو تعديل سجل العملة وتحديث عداد العملات في جدول SysConfig.
        /// </summary>
        public async Task<(bool Success, string Message)> SaveCurrencyAsync(Models.Currency currencyInput)
        {
            var validation = Validate(currencyInput);
            if (!validation.IsValid)
                return (false, validation.ErrorMessage);

            try
            {
                var existing = await opp.db.Currency.FindAsync(currencyInput.Id);

                if (existing == null)
                {
                    // إضافة عملة جديدة
                    currencyInput.Insert_ServerDateTime = DateTime.Now;
                    currencyInput.Update_ServerDateTime = DateTime.Now;
                    currencyInput.OperationDate = DateTime.Now;

                    await opp.db.Currency.AddAsync(currencyInput);
                }
                else
                {
                    // تعديل عملة قائمة
                    existing.Currency_Name = currencyInput.Currency_Name?.Trim();
                    existing.Currency_Name_EN = currencyInput.Currency_Name_EN?.Trim();
                    existing.code = currencyInput.code?.Trim();
                    existing.Value = currencyInput.Value;
                    existing.rate = currencyInput.rate;
                    existing.Desc_ar = currencyInput.Desc_ar?.Trim();
                    existing.Desc_en = currencyInput.Desc_en?.Trim();
                    existing.Active = currencyInput.Active;
                    existing.IS_GL_Currency_Flage = currencyInput.IS_GL_Currency_Flage;
                    existing.Update_ServerDateTime = DateTime.Now;
                    existing.EditOperationDate = DateTime.Now;
                    existing.Edit_User_Id = currencyInput.Edit_User_Id;
                }

                await opp.db.SaveChangesAsync();

                // تحديث عداد العملات في SysConfig
                await UpdateCurrencyCountConfigAsync();

                return (true, "تم حفظ بيانات العملة بنجاح وتحديث الإعدادات.");
            }
            catch (Exception ex)
            {
                return (false, $"حدث خطأ أثناء الحفظ:\n{ex.Message}");
            }
        }

        /// <summary>
        /// حذف العملة وتحديث إعدادات النظام.
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteCurrencyAsync(long currencyId)
        {
            try
            {
                var existing = await opp.db.Currency.FindAsync(currencyId);
                if (existing == null)
                    return (false, "العملة المحددة غير موجودة.");

                opp.db.Currency.Remove(existing);
                await opp.db.SaveChangesAsync();

                await UpdateCurrencyCountConfigAsync();

                return (true, "تم حذف العملة بنجاح وتحديث الإعدادات.");
            }
            catch (Exception ex)
            {
                return (false, $"تعذر الحذف (قد تكون العملة مرتبطة بحركات مالية):\n{ex.Message}");
            }
        }

        #endregion

        #region Business Validation & Rules

        /// <summary>
        /// تحديث مفتاح Currency_Count في جدول SysConfig.
        /// </summary>
        public async Task UpdateCurrencyCountConfigAsync()
        {
            try
            {
                int activeCount = await opp.db.Currency.CountAsync(c => c.Active == 1);
                string KeysData = "Currency_Count";
                string NewValue = activeCount.ToString();
                AccountModelsLib.Common.Save_SystemParametersDB(KeysData, NewValue);
            }
            catch
            {
                // يمكن تسجيل الخطأ في ALogError
            }
        }

        /// <summary>
        /// التحقق من صحة وقواعد إدخال العملة.
        /// </summary>
        public (bool IsValid, string ErrorMessage) Validate(Models.Currency currency)
        {
            if (currency == null)
                return (false, "بيانات العملة غير موجودة.");

            if (string.IsNullOrWhiteSpace(currency.Currency_Name))
                return (false, "يرجى إدخال اسم العملة بالعربي.");

            if (currency.Value <= 0)
                return (false, "يجب تحديد القيمة المعيارية للعملة (Enum Value).");

            if (!currency.rate.HasValue || currency.rate.Value <= 0)
                return (false, "يجب أن يكون سعر الصرف (المعامل) أكبر من الصفر.");

            return (true, string.Empty);
        }

        #endregion
    }
}
