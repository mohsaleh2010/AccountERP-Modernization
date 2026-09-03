using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AccountModelsLib.Models
{
    public class ReceiptCLS
    {
        private operations opp = new operations();

        public List<Money_Transaction_Box> GetReceiptsNotFullyDistributed()
        {
            List<Money_Transaction_Box> rece = new List<Money_Transaction_Box>();
            //return opp.db.Money_Transaction_Box.Where(r => r.Amount > r.PaidDistributed).ToList();
            return rece;
        }

        public Money_Transaction_Box GetReceiptById(int receiptId)
        {
            return opp.db.Money_Transaction_Box.FirstOrDefault(r => r.Id == receiptId);
        }

        public void SaveDistribution(Money_Transaction_Box receipt, List<Money_Transaction_BoxForInvoice> distributionList)
        {
            //foreach (var dist in distributionList)
            //{
            //    opp.db.Money_Transaction_BoxForInvoices.Add(dist);
            //    var inv = opp.db.Invoices_List.Where(i => i.Invoice_Id == dist.Invoices_ListId).FirstOrDefault();
            //    if (inv != null)
            //    {
            //        inv.PaidAmount += dist.PaidAmount;
            //    }
            //    receipt.PaidDistributed += dist.PaidAmount;
            //}
            //opp.SaveChanges();
        }
    }
}
