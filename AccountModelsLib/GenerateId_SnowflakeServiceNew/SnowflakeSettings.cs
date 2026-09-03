using IdGen;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AccountModelsLib
{
      

    public class SnowflakeSettings
    {
        public int BranchId { get; set; } = 1;
        public DateTime Epoch { get; set; } = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public int TimestampBits { get; set; } = 41;
        public int GeneratorIdBits { get; set; } = 10;
        public int SequenceBits { get; set; } = 12;
        public string ConnectionString { get; set; } = "Server=***;Database=***";
        public string Theme { get; set; } = "Dark";
    }
}