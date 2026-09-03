using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Linq;

namespace AccountModelsLib.Models
{
    public class PrinterSettingsDBCls
    {
        private readonly operations opp; // استبدله بنوع الـ DbContext الخاص بمشروعك

        public PrinterSettingsDBCls(operations opp_)
        {
            opp = opp_;
        }
        
        public PrinterSettingsDBCls()
        {
            opp = new operations();
        }
        /// <summary>
        /// جلب الإعدادات الحالية من قاعدة البيانات أو إنشاء إعدادات افتراضية إذا كان الجدول فارغاً
        /// </summary>
        public SysPrinterSettings GetSettings()
        {
            try
            {
                var settings = opp.db.PrinterSettings.FirstOrDefault(x => x.Id == 1);

                if (settings == null)
                {
                    settings = new SysPrinterSettings(); // كائن افتراضي
                    opp.db.PrinterSettings.Add(settings);
                    opp.db.SaveChanges();
                }

                return settings;
            }
            catch (Exception)
            {
                // منع انهيار البرنامج وإرجاع إعدادات افتراضية في حال عدم وجود اتصال
                return new SysPrinterSettings();
            }
        }

        /// <summary>
        /// حفظ أو تحديث الإعدادات داخل قاعدة البيانات
        /// </summary>
        public bool SaveSettings(SysPrinterSettings updatedSettings)
        {
            try
            {
                var dbSettings = opp.db.PrinterSettings.FirstOrDefault(x => x.Id == 1);

                if (dbSettings == null)
                {
                    updatedSettings.Id = 1;
                    opp.db.PrinterSettings.Add(updatedSettings);
                }
                else
                {
                    // تحديث الحقول
                    dbSettings.InvoicePrinterName = updatedSettings.InvoicePrinterName;
                    dbSettings.KitchenPrinterName = updatedSettings.KitchenPrinterName;
                    dbSettings.PaperWidth = updatedSettings.PaperWidth;
                    dbSettings.InvoiceWidth = updatedSettings.InvoiceWidth;
                    dbSettings.MarginLeft = updatedSettings.MarginLeft;
                    dbSettings.StartHeightY = updatedSettings.StartHeightY;
                    dbSettings.ShowPreview = updatedSettings.ShowPreview;
                    dbSettings.PrintKitchenSlip = updatedSettings.PrintKitchenSlip;
                }

                return opp.db.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"فشل حفظ الإعدادات في قاعدة البيانات: {ex.Message}");
            }
        }
    }
}
