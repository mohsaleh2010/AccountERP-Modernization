// Run SalesRepTestRunner in a scratch console (rollback-only; no production data write)
using System;
using AccountModelsLib.SalesRep;

class ScratchSalesRepTests
{
    static void Main()
    {
        // Point to the local ClientNew instance via the XML config path (Encrypt=False),
        // so we never hardcode a connection string (rule 6). TestConnStr is optional —
        // if empty, operations(2) uses the standard path.
        try
        {
            SalesRepTestRunner.RunAllTests();
            Console.WriteLine("=== TESTS COMPLETED ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine("TESTS FAILED: " + ex.Message);
            if (ex.InnerException != null)
                Console.WriteLine("INNER: " + ex.InnerException.Message);
        }
    }
}
