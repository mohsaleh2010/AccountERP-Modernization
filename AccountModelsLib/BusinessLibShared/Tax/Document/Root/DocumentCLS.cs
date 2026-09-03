using Newtonsoft.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Linq;
using RestSharp;
//using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using Microsoft.EntityFrameworkCore;
 
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
 
using System.Xml.Linq;
namespace AccountModelsLib.Models.Tax
{


    public class DocumentCLS
    {
        public AccountModelsLib.operations opp;
        long CustomerId;

        public DocumentCLS()
        {
            opp = new AccountModelsLib.operations();
        }
        public DocumentCLS(ref AccountModelsLib.operations opp_)
        {
            opp = opp_;
        }


        public AccountModelsLib.Models.Tax.Document GetDocument(long TaxInvoices_ListId)
        {
            AccountModelsLib.Models.TaxInvoices_List tx = opp.db.TaxInvoices_List.AsNoTracking().Where(i => i.Id == TaxInvoices_ListId).FirstOrDefault();
            CustomerId = tx.CustomerId ?? 0;
            AccountModelsLib.Models.Tax.Document doc = new AccountModelsLib.Models.Tax.Document();
            doc.documentType = "I";   //InvoiceType

            doc.documentTypeVersion = AccountModelsLib.Common.CommonTaxConfig.documentTypeVersion;
            //doc.dateTimeIssued = inv.Invoice_Date.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") ,
            doc.dateTimeIssued = tx.dateTimeIssued.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

            doc.internalID = tx.internalID;
            doc.taxpayerActivityCode = AccountModelsLib.Common.CommonTaxConfig.ActivityCode;
            doc.purchaseOrderReference = "";
            doc.purchaseOrderDescription = "";
            doc.salesOrderReference = "";
            doc.salesOrderDescription = "";
            doc.proformaInvoiceNumber = "";
            doc.totalDiscountAmount = tx.totalDiscountAmount;
            doc.totalSalesAmount = tx.totalSalesAmount;
            doc.netAmount = tx.netAmount;
            
            doc.totalAmount = tx.totalAmount;
            doc.extraDiscountAmount = tx.extraDiscountAmount;
            doc.totalItemsDiscountAmount = tx.totalItemsDiscountAmount;
            //doc.taxTotals =
            doc.issuer = GetIssuer();
            doc.receiver = GetReceiver(CustomerId);
            doc.payment = GetPayment();
            doc.delivery = GetDelivery();
            doc.invoiceLines = GetInvoiceLines(TaxInvoices_ListId);
            doc.taxTotals = GetTaxTotals(TaxInvoices_ListId);
            //doc.signatures = GetSignatures(doc);

            return doc;
        }
        public List<Document> GetDocumentList(long TaxInvoices_ListId)
        {
            List<Document> rr = new List<Document>();

            Document D = GetDocument(TaxInvoices_ListId);
            rr.Add(D);


            return rr;
        }
        public Document GetDocumentOne(long TaxInvoices_ListId)
        {


            Document D = GetDocument(TaxInvoices_ListId);


            return D;
        }
     
        private List<AccountModelsLib.Models.Tax.TaxTotal> GetTaxTotals(long TaxInvoices_ListId)
        {
            AccountModelsLib.Models.Tax.TaxInvoices_TaxesTotalCLS txTotalCls = new AccountModelsLib.Models.Tax.TaxInvoices_TaxesTotalCLS();
            List<AccountModelsLib.Models.Tax.TaxTotal> taxTotalList = txTotalCls.GetTaxTotals(TaxInvoices_ListId);

            return taxTotalList;
        }

        private List<AccountModelsLib.Models.Tax.InvoiceLine> GetInvoiceLines(long TaxInvoices_ListId)
        {
            //= new List<InvoiceLine>();
            AccountModelsLib.Models.Tax.InvoiceLineCls invCls = new AccountModelsLib.Models.Tax.InvoiceLineCls();
            List<AccountModelsLib.Models.Tax.InvoiceLine> invoiceLines = invCls.GetInvoiceLines(TaxInvoices_ListId);
            return invoiceLines;
        }

        private Issuer GetIssuer()
        {
            IssuerCls iss = new IssuerCls();
            return iss.GetIssuer();
        }
        private Receiver GetReceiver(long CustomerId)
        {
            ReceiverCls rss = new ReceiverCls();
            return rss.GetReceiver(CustomerId);
        }
        private Payment GetPayment()
        {
            return new Payment();
        }
        private Delivery GetDelivery()
        {
            return new Delivery();
        }
    }

}
