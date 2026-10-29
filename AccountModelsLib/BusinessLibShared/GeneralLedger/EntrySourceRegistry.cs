using System.Collections.Generic;

namespace AccountModelsLib.BusinessLibShared.GeneralLedger
{
    /// <summary>
    /// سجل مركزى لمصادر القيود (Account_Entry_List.Reference_Type) — للقراءة والعرض فقط (لا يغيّر قيم أى مصدر قائم).
    /// القيم مأخوذة من الكود الحى؛ القيمة غير المسجَّلة تُعرض «غير معروف» ولا تُرفض.
    /// </summary>
    public static class EntrySourceRegistry
    {
        public class Source
        {
            public int ReferenceType { get; set; }
            public string Name { get; set; }
            public string SourceTable { get; set; }
            public bool BuiltAutomatically { get; set; }
        }

        private static readonly Dictionary<int, Source> _all = Build();

        private static Dictionary<int, Source> Build()
        {
            var d = new Dictionary<int, Source>();
            void Add(int rt, string name, string table, bool auto) => d[rt] = new Source { ReferenceType = rt, Name = name, SourceTable = table, BuiltAutomatically = auto };
            Add(0, "قيد يدوى", "", false);
            Add(1, "فاتورة مبيعات", "Invoices", true);
            Add(2, "فاتورة مشتريات", "Bills", true);
            Add(5, "شيك", "Checks_Memory", true);
            Add(6, "سند صندوق", "Order_Money", true);
            Add(7, "مرتجع مبيعات", "Invoices", true);
            Add(8, "مرتجع مشتريات", "Bills", true);
            Add(26, "تسوية مالية", "Money_Financial_Notice", true);
            Add(29, "تسوية مالية بالأصناف", "Money_Financial_Notice", true);
            Add(54, "فحص سيارة", "CarsExaminations", true);
            return d;
        }

        public static IReadOnlyCollection<Source> All => _all.Values;

        public static bool IsKnown(int referenceType) => _all.ContainsKey(referenceType);

        public static string NameOf(int referenceType) => _all.TryGetValue(referenceType, out var s) ? s.Name : "غير معروف (" + referenceType + ")";
    }
}
