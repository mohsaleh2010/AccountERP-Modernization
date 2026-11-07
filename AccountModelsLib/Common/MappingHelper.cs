using AccountModelsLib.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib 
{
    public static class MappingHelper
    {
        // قاموس لتخزين الخصائص في الذاكرة بعد فحصها لأول مرة (كاش)
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertiesCache =
            new ConcurrentDictionary<Type, PropertyInfo[]>();

        public static TTarget MapScalarProperties<TTarget, TSource>(TSource source,bool IsUpdate=false) where TTarget : new()
        {
            if (source == null) return default(TTarget);

            TTarget target = new TTarget();

            //if (typeof(TTarget) == typeof(TSource))
            //{
               
            //    target =(TTarget)(object)source; // إذا كان النوعان متطابقين، قم بإرجاع المصدر مباشرةً بعد التحويل
             
            //    return target;
            //}
            // جلب الخصائص من الكاش مباشرة بدلاً من إعادة فحص الكلاس في كل عملية حفظ
            var targetProperties = PropertiesCache.GetOrAdd(typeof(TTarget), t => t.GetProperties());
            var sourceType = source.GetType();

            foreach (var prop in targetProperties)
            {
                string propertyName = prop.Name;

                if (IsUpdate == false)
                {
                    if (propertyName == "Id") continue;


                    // 🛑 خط الدفاع الأول والأقوى: إذا كان الحقل نصاً صافياً، لا تفحصه ولا تطبق عليه أي شروط.. دعه يمر فوراً!
                    if (prop.PropertyType == typeof(string))
                    {
                        // نتركه يمر ليكمل الكود لأسفل ويقوم بالنسخ بنجاح
                    }
                    // 🎯 الآن فلاتر استبعاد العلاقات والكولكشنز تعمل بأمان تآم على بقية الأنواع دون المساس بالنصوص
                    else if (prop.PropertyType.IsClass || typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType))
                    {
                        continue; // تخطي الكلاسات وجداول الربط والقوائم الفعليه الأخرى (مثل List<T>)
                    }
                }
                var sourceProp = sourceType.GetProperty(propertyName);
                if (sourceProp != null && sourceProp.CanRead && prop.CanWrite)
                {
                    var val = sourceProp.GetValue(source, null);
                    prop.SetValue(target, val, null);
                }
            }

            return target;
        }
        public static void CopyScalarProperties<TSource, TTarget>(TSource source, TTarget target)
        {
            if (source == null || target == null) return;

            // Use your existing property cache
            var targetProperties = PropertiesCache.GetOrAdd(typeof(TTarget), t => t.GetProperties());
            var sourceType = source.GetType();

            foreach (var prop in targetProperties)
            {
                string propertyName = prop.Name;

                // Skip Primary Key, classes, and collections (navigation properties)
                if (propertyName == "Id") continue;

                if (prop.PropertyType != typeof(string) &&
                    (prop.PropertyType.IsClass || typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType)))
                {
                    continue;
                }

                var sourceProp = sourceType.GetProperty(propertyName);
                if (sourceProp != null && sourceProp.CanRead && prop.CanWrite)
                {
                    var val = sourceProp.GetValue(source, null);
                    prop.SetValue(target, val, null);
                }
            }
        }

        /// <summary>
        /// دالة عامة لنسخ الحقول الأساسية وتجاهل العلاقات والمفاتيح الأساسية بين كائنين متشابهين
        /// </summary>
        /// <typeparam name="TTarget">نوع الكائن المراد الحفظ فيه (الهدف)</typeparam>
        /// <typeparam name="TSource">نوع الكائن القادم بالبيانات (المصدر)</typeparam>
        /// <param name="source">الكائن المصدر المحتوي على البيانات الفعليه</param>
        /// <returns>نسخة جديدة من كائن الهدف محملة بالبيانات الصافية</returns>
        public static TTarget MapScalarPropertiesForNewAdd<TTarget, TSource>(TSource source)
            where TTarget : new() // شرط لضمان إمكانية عمل new() لكائن الهدف داخلياً
        {
            if (source == null) return default(TTarget);

            if (typeof(TTarget) == typeof(TSource))
            {
                 return (TTarget)(object)source; // إذا كان النوعان متطابقين، قم بإرجاع المصدر مباشرةً بعد التحويل
            }
             
            // 1. إنشاء كائن جديد فارغ تماماً من نوع الهدف (Target)
            TTarget target = new TTarget();

            // 2. جلب جميع الخصائص الخاصة بكائن الهدف
            var targetProperties = typeof(TTarget).GetProperties();
            var sourceType = source.GetType();

            foreach (var prop in targetProperties)
            {
                string propertyName = prop.Name;

                // 🛑 1️⃣ تجاهل الـ Primary Key
                if (propertyName == "Id"  ) continue;

                // 🛑 2️⃣ استبعاد العلاقات (Navigation Properties) ديناميكياً
                if ((prop.PropertyType.IsClass && prop.PropertyType != typeof(string)) ||
                    typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType))
                {
                    continue; // تخطي الكلاسات وجداول الربط فوراً
                }

                // 🎯 3️⃣ قراءة القيم من المصدر ونسخها إلى الهدف للحقول العادية (Scalar Properties)
                var sourceProp = sourceType.GetProperty(propertyName);
                if (sourceProp != null && sourceProp.CanRead && prop.CanWrite)
                {
                    var val = sourceProp.GetValue(source, null);
                    prop.SetValue(target, val, null);
                }
            }

            return target;
        }
        //public static TTarget MapScalarPropertiesEFForNewAdd<TTarget, TSource>(TSource source)
        //         where TTarget : new() // شرط لضمان إمكانية عمل new() لكائن الهدف داخلياً
        //{

        //}
        public static Invoices_Details MapScalarPropertiesEFForNewAdd(ref operations  opp, IInvoices_Details source )        
        {
            Invoices_Details invoices_Details = new Invoices_Details();
             
            
            opp.db.Invoices_Details.Attach(invoices_Details);
            

            var entry = opp.db.Entry(invoices_Details);
            var sourceEntry = opp.db.Entry(source);

            foreach (var property in entry.CurrentValues.Properties)
            {
                string propertyName = property.Name;

                // 🛑 تجاهل المفتاح الرئيسي Id
                if (propertyName == "Id") continue;

                // نسخ القيمة مباشرة من المصدر إلى الكائن الهدف
                entry.CurrentValues[propertyName] = sourceEntry.CurrentValues[propertyName];
            }

            return invoices_Details;
        }
    }
}
