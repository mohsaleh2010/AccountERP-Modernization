using IdGen;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace AccountModelsLib
{

    public class CallSnowflakeIdGenerate
    {
        // 1. جعل المولد static لضمان وجود نسخة واحدة ثابتة تمنع تكرار الـ IDs
        private static readonly SnowflakeServiceNewSqlite _staticSqliteService;
        private static readonly SnowflakeServiceNew _staticService;
        // حفظ آخر Id تم إصداره بنجاح
        private static long _lastGeneratedId = -1;
        // كائن قفل لحماية المتغير الأخير من التداخل بين الـ Threads
        private static readonly object _syncLock = new object();
        static CallSnowflakeIdGenerate()
        {
            try
            {
                _staticSqliteService = new SnowflakeServiceNewSqlite();
            }
            catch (Exception)
            {
                // Log exception if needed
            }

            try
            {
                _staticService = new SnowflakeServiceNew();
            }
            catch (Exception)
            {
                // Log exception if needed
            }
        }

        // Constructor العادي للأنماط غير الـ Static
        public CallSnowflakeIdGenerate()
        {
        }
        
        public long GenerateId()
        {
            try
            {
                if (_staticSqliteService != null)
                {
                    return _staticSqliteService.GenerateNewId();
                }

                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        // 3. استدعاء المولد مباشرة بشكل Static (حل خطأ CS0120)
        public static long GenerateNewId()
        {
            lock (_syncLock)
            {
                try
                {

                    long newId = _staticSqliteService != null
                    ? _staticSqliteService.GenerateNewId()
                    : -1;
                    // إذا حدث تكرار وطابق الرقم الجديد الرقم الأخير
                    while (newId == _lastGeneratedId && newId != -1)
                    {
                        // انتظر أجزاء صغيرة جداً من الميكروثانية (بدون إيقاف الـ CPU)
                        Thread.SpinWait(10);

                        // أعد المحاولة لحين الانتقال للتوقيت/الـ Sequence التالي
                        newId = _staticSqliteService.GenerateNewId();
                    }
                    // حفظ الرقم الجديد المضمون
                    _lastGeneratedId = newId;

                    return newId;
                }
                catch (Exception)
                {
                    return -1;
                }
            }
        }
    }
}