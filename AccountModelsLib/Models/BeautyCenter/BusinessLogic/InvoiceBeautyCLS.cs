using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
using Microsoft.EntityFrameworkCore;
namespace AccountModelsLib.Models
{
    public class InvoiceBeautyCLS
    {
        public long Invoice_List_Table_Id = 0;
        //private BindingList<Invoices_Details> invoiceDetails;
        public long SaveInvoiceAndLinkServices(ref int Excp, ref string Msg, int Is_Invoice_Agile, Appointment appointment, List<AppointmentService> appointmentServiceLst, operations opp)
        {
            Excp = 0;
            Msg = "";
            InvoicesOperationCLS invD_O = new InvoicesOperationCLS(0);
            InvoicesOperationListCLS invL_O = new InvoicesOperationListCLS(0);
            Requests_ModelCls rc = new Requests_ModelCls();
            invL_O.appServiceLst = appointmentServiceLst;
            rc.Make_Invoice_Agile = Is_Invoice_Agile;
            invL_O.Invoice_Type_Id = Common.Invoice_Type_Id;

            MapInvoices mapInvoices = new MapInvoices();
            
            invL_O.invL = mapInvoices.MapSalesToInvoiceHeader(appointmentServiceLst, appointment.Customer_Id ?? 0, Is_Invoice_Agile);

            // استخراج التفاصيل المحددة من الجريد
            var selectedDetails = new List<Invoices_Details>();

            foreach (var sales in appointmentServiceLst)
            {
                if (sales.IsProcessedAsInvoice == 0)
                {
                    continue;
                }

                Invoices_Details invD = mapInvoices.MapSalesToInvoiceDetail(sales, ref opp);
                invD_O.invDlst.Add(invD);

                sales.IsProcessedAsInvoice = 1; // تعيين الحالة على أنها تمت معالجتها كفاتورة
            }
            rc.Make_Invoice_Update_CustomerAccount(ref Excp, ref Msg, invL_O, invD_O);
            Invoice_List_Table_Id = invL_O.invL.Id;
            return Invoice_List_Table_Id;




        }
        public void SaveInvoiceAndLinkServices(ref int Excp, ref string Msg, Invoices_List invoice, long appointmentId, List<Invoices_Details> selectedDetails, operations opp)
        {
            try
            {
                opp.BeginTransaction();

                // ✅ حفظ الفاتورة
                opp.db.Invoices_List.Add(invoice);
                opp.db.SaveChanges();

                // ✅ ربط الخدمات بالفاتورة
                var appointment = opp.db.Appointments
                    .Include(a => a.appointmentService)
                    .FirstOrDefault(a => a.Id == appointmentId);

                foreach (var detail in selectedDetails)
                {
                    var service = appointment.appointmentService
                        .FirstOrDefault(s => s.ServiceItem_Name == detail.Invoice_Item_Name);

                    if (service != null)
                    {
                        service.IsProcessedAsInvoice = 1;
                        service.Invoices_List_Id = invoice.Id;
                        service.Invoices_Details_Id = detail.Id;
                    }
                }

                opp.db.SaveChanges();
                opp.Commit();
                Excp = 0;
                Msg = "تم الحفظ وربط الخدمات بنجاح.";
            }
            catch (Exception ex)
            {
                opp.Rollback();
                Excp = 1;
                Msg = ex.Message;
            }
        }



    }

}
