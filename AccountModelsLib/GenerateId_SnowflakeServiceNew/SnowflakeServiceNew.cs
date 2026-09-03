using IdGen;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AccountModelsLib
{
    public class SnowflakeServiceNew
    {
        private readonly IIdGenerator<long> _generator;

        public SnowflakeServiceNew()
        {
            // 1. قراءة الإعدادات باستخدام SnowflakeConfigStorage المتقاطع بين المنصات
            AppConfig config = SnowflakeConfigStorage.LoadConfig();
            SnowflakeSettings settings = config.SnowflakeSettings;

            // 2. إنشاء IdStructure بالتحويل الصريح إلى byte
            var structure = new IdStructure(
                (byte)settings.TimestampBits,
                (byte)settings.GeneratorIdBits,
                (byte)settings.SequenceBits
            );

            // 3. إعداد مصدر الوقت والتاريخ
            var timeSource = new DefaultTimeSource(settings.Epoch);
            var options = new IdGeneratorOptions(structure, timeSource);

            // 4. إنشاء مولّد الأرقام باستخدام BranchId المخصص
            _generator = new IdGenerator(settings.BranchId, options);
        }

        /// <summary>
        /// توليد ID فريد جديد بنظام Snowflake
        /// </summary>
        public long GenerateNewId()
        {
            return _generator.CreateId();
        }

       
    }
    public static class SnowflakeConfigStorage
    {
        private const string FileName = "appsettings.json";

        // جلب مسار الحفظ الآمن المناسب للبيئة التشغيلية (Windows أو Android)
        private static string GetFilePath()
        {
            //string folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            // return Path.Combine(folder, FileName);

            string folder = AppContext.BaseDirectory;

            return Path.Combine(folder, FileName);

        }

        /// <summary>
        /// قراءة الإعدادات من ملف JSON، وإنشاؤه إن لم يكن موجوداً
        /// </summary>
        public static AppConfig LoadConfig() // 👈 تم إضافة static هنا
        {
            try
            {
                string filePath = GetFilePath();
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var config = JsonSerializer.Deserialize<AppConfig>(json);
                    if (config != null) return config;
                }
            }
            catch
            {
                // في حال وجود خلل في قراءة الملف، يتم استخدام الإعدادات الافتراضية
            }

            // إنشاء الملف بالقيم الافتراضية إذا لم يكن موجوداً
            var defaultConfig = new AppConfig();
            SaveConfig(defaultConfig);
            return defaultConfig;
        }

        /// <summary>
        /// حفظ الإعدادات الحالية إلى ملف JSON
        /// </summary>
        public static void SaveConfig(AppConfig config)
        {
            try
            {
                string filePath = GetFilePath();
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true // لتنسيق الـ JSON بشرطات مسافية مريحة للعين
                };

                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving config: {ex.Message}");
            }
        }
    }

   

    public class AppConfig
    {
        [JsonPropertyName("SnowflakeSettings")]
        public SnowflakeSettings SnowflakeSettings { get; set; } = new SnowflakeSettings();
    }
}