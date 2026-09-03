using IdGen;
using System;

namespace AccountModelsLib
{
    public class SnowflakeServiceNewSqlite
    {
        private readonly IIdGenerator<long> _generator;

        public SnowflakeServiceNewSqlite()
        {
              
            // استدعاء قراءة الإعدادات من SQLite
            SnowflakeSettings settings = SQLiteConfigStorage.LoadSettings();

            var structure = new IdStructure(
                (byte)settings.TimestampBits,
                (byte)settings.GeneratorIdBits,
                (byte)settings.SequenceBits
            );

            var timeSource = new DefaultTimeSource(settings.Epoch);
            var options = new IdGeneratorOptions(structure, timeSource);

            _generator = new IdGenerator(settings.BranchId, options);
        }

        public long GenerateNewId()
        {
            return _generator.CreateId();
        }

      
    }
}