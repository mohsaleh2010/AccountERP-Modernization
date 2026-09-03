using AccountModelsLib.Models;
using AccountModelsLib;
 
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 

namespace AccountModelsLib.Models
{
    public class InvoicesFromBatch
    {

        //public List<IInvoices_Details> MapSalesListToInvoiceDetails(List<Unit_Item_SalesDate> salesList)
        //{
        //    return salesList?.Select(MapSalesToInvoiceDetail_).ToList();
        //}
        /*
        public long CreateInvoiceFromSales1(List<Unit_Item_SalesDate> salesList, DbContext context)
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    // إنشاء الفاتورة
                    var invoiceHeader = MapSalesToInvoiceHeader(salesList);
                    context.Set<IInvoices_List>().Add(invoiceHeader);
                    context.SaveChanges();

                    // تحويل كل عملية بيع إلى تفصيلة فاتورة
                    var invoiceDetails = salesList.Select(s =>
                    {
                        var detail = MapSalesToInvoiceDetail(s); // دالة سبق شرحها
                        detail.Invoice_Id = invoiceHeader.Id;
                        return detail;
                    }).ToList();

                    context.Set<IInvoices_Details>().AddRange(invoiceDetails);

                    // تحديث عمليات البيع وربطها بالفاتورة
                    foreach (var s in salesList)
                    {
                        s.Invoices_ListId = invoiceHeader.Id;
                        s.IsProcessedAsInvoice = 1;
                        context.Entry(s).State = EntityState.Modified;
                    }

                    context.SaveChanges();
                    transaction.Commit();
                    return invoiceHeader.Id;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("فشل إنشاء الفاتورة: " + ex.Message);
                }
            }
        }
       
        public long CreateInvoiceFromSales(List<Unit_Item_SalesDate> salesList, operations opp)
        {
            opp.BeginTransaction(2);
            try
            {
                // إنشاء الفاتورة
                var Invoices_List = MapSalesToInvoiceHeader(salesList);
                opp.db.Invoices_List.Add((Invoices_List)Invoices_List);
                
                // تحويل كل عملية بيع إلى تفصيلة فاتورة
                var Invoices_Details = salesList.Select(s =>
                {
                    var detail = MapSalesToInvoiceDetail(s); // دالة سبق شرحها
                    detail.Invoice_Id = Invoices_List.Id;
                    return detail;
                }).ToList();

                //opp.db.Invoices_Details.AddRange( Invoices_Details);

                // تحديث عمليات البيع وربطها بالفاتورة
                foreach (var s in salesList)
                {
                    s.Invoices_ListId = Invoices_List.Id;
                    s.IsProcessedAsInvoice = 1;
                    opp.db.Entry(s).State = EntityState.Modified;
                }


                opp.db.SaveChanges();
                opp.Commit();
                return Invoices_List.Id;
            }
            catch (Exception ex)
            {
                opp.Rollback();
                throw new Exception("فشل إنشاء الفاتورة: " + ex.Message);
            }

        }
 */
        public void CreateInvoiceFromSales(ref List<Unit_Item_SalesDate> salesList, ref  IInvoices_List invoices_List, ref List<IInvoices_Details> invDlst,   operations opp)
        {
            try
            {
                MapInvoices mapInvoices = new MapInvoices();
                // إنشاء الفاتورة
                IInvoices_List invoices_List_ = mapInvoices.MapSalesToInvoiceHeader(salesList, invoices_List);
                invoices_List = invoices_List_;

                // تحويل كل عملية بيع إلى تفصيلة فاتورة
                List<Invoices_Details> Invoices_Details = salesList.Select(s =>
                {
                    var detail = mapInvoices.MapSalesToInvoiceDetail(s,ref opp); // دالة سبق شرحها
                    detail.Invoice_Id = invoices_List_.Id;
                    return detail;
                }).ToList();
                //invDlst = Invoices_Details;
                decimal Invoices_DetailsTotal = 0;
                foreach (var item in Invoices_Details)
                {
                    invDlst.Add(item);
                    decimal Item_Net_Total_Price = item.Item_Net_Total_Price ?? 0;
                    decimal Total = item.Total ?? 0;
                    Invoices_DetailsTotal = Invoices_DetailsTotal + Total;
                }

                //invoices_List_.Total = Invoices_DetailsTotal;
                //invoices_List_.NetInvoiceTotal = Invoices_DetailsTotal;
                //invoices_List_.Net_Total = Invoices_DetailsTotal;
                //invoices_List_.Item_Net_Total = Invoices_DetailsTotal;
                 
                // تحديث عمليات البيع وربطها بالفاتورة
                foreach (var s in salesList)
                {
                    s.Invoices_ListId = invoices_List.Id;
                    s.IsProcessedAsInvoice = 1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("فشل إنشاء الفاتورة: " + ex.Message);
            }
        }

        public void EditUpdateInvoice(ref int Excp, ref string Msg, operations opp, long Invoice_List_Table_Id, List<Unit_Item_SalesDate> salesList, IInvoices_List invoices_List)
        {
            InvoicesOperationCLS invD_O = new InvoicesOperationCLS();
            InvoicesOperationListCLS invL_O = new InvoicesOperationListCLS();
            //invL_O.Get_Invoices_List(Invoice_List_Table_Id);
            invL_O.invL = invoices_List;
            invD_O.GetInvoiceDetailsToEdit(Invoice_List_Table_Id);
            Excp = 0;
            Msg = "";
            Requests_ModelCls  rc = new Requests_ModelCls();
            rc.Delete_Invoices_Details_CustomerAccount(ref Excp, ref Msg, ref opp, invL_O, invD_O);

            if (Excp == 0)
            {
                SetInvoice(ref Excp, ref Msg, Invoice_List_Table_Id, salesList, invoices_List, opp);
            }
        }
        public void DeleteInvoice_Finaly(ref int Excp, ref string Msg, operations opp, long Invoice_List_Table_Id)
        {
            //if (!AccountCommonDataLIB.Common.MsgBoxYesNo("هل تريد حذف الفاتورة", "حذف فاتورة"))
            //{
            //    return;
            //}

            if (Invoice_List_Table_Id > 0)
            { 
                InvoicesOperationCLS invD_O = new InvoicesOperationCLS();
                InvoicesOperationListCLS invL_O = new InvoicesOperationListCLS();
                invL_O.Get_Invoices_List(Invoice_List_Table_Id);
                invD_O.GetInvoiceDetailsToEdit(Invoice_List_Table_Id);
                Excp = 0;
                Msg = "";

                Requests_ModelCls rc = new Requests_ModelCls();
                 
                //rc.Make_Invoice_Update_CustomerAccount(ref Excp, ref New_Items_ds, ref Msg, invL_O);
                rc.Delete_Finaly_Invoice_CustomerAccount(ref Excp, ref Msg, invL_O, invD_O);
                //if (Excp == 0)
                //{

                //    MessageBox.Show("تم حذف الفاتورة بنجاح", "حذف فاتورة", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //}
                //else
                //{
                //    MessageBox.Show("حدث خطأ أثناء حذف الفاتورة" + Environment.NewLine + Msg, "حذف فاتورة", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
            }
        }
        public long SetInvoice(ref int Excp, ref string Msg, long Invoice_List_Table_Id,  List<Unit_Item_SalesDate> salesList,IInvoices_List invoices_List , operations opp)
        {
            //long Invoice_List_Table_Id = 0;
            try
            { 
                Excp = 0;
                Msg = "";
                InvoicesOperationCLS invD_O = new InvoicesOperationCLS(0);
                InvoicesOperationListCLS invL_O = new InvoicesOperationListCLS(0);

                invL_O.invL = invoices_List;

                Requests_ModelCls rc = new Requests_ModelCls();
                invL_O.salesList = salesList;
                rc.Make_Invoice_Agile = invoices_List.Is_Invoice_Agile??1;
                invL_O.Invoice_Type_Id = Common.Invoice_Type_Id;
                CreateInvoiceFromSales(ref invL_O.salesList, ref invL_O.invL, ref invD_O.invDlst , opp);

                if(invL_O.invL == null || invD_O.invDlst == null)
                {
                    Excp = 1;
                    Msg = "خطأ في إنشاء الفاتورة";
                    return Invoice_List_Table_Id;
                }   

                if(Invoice_List_Table_Id >0)
                {
                    invL_O.invL.Id = Invoice_List_Table_Id;
                }
                
                //invL_O.invL = invoices_List;
                //invD_O.invDlst = invDlst;
                //Get_MakeInvoiceListData(Common.Invoice_Type_Id);
                 
                rc.Make_Invoice_Update_CustomerAccount(ref Excp, ref Msg, invL_O, invD_O);
                Invoice_List_Table_Id = invL_O.invL.Id;
                return Invoice_List_Table_Id;
            }
            catch (Exception ee)
            { 
                Msg = ee.Message;
                Excp = 1;
                return Invoice_List_Table_Id;
            } 
        } 
    }
}
