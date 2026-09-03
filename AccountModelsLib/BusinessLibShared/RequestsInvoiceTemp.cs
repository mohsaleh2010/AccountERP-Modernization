using System;
using System.Collections.Generic;
using System.Text;
//using Connection4It;  using AccountModelsLib; using AccountModelsLib; using AccountModelsLib.Models;
using System.Data;
using System.Collections;
 
//using Store_LIB;
//using Account_Common_LIB;
using AccountModelsLib.Models;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Linq;
namespace AccountModelsLib
{
    public partial class RequestsInvoiceTemp
    {
        public long New_Invoice_Id;

        public long Make_New_Invoice_No_POS(int Invoice_Type, DateTime Invoice_Date)
        {
            string mysql = "SELECT MAX(Invoice_Id) AS Max_Invoice_Id FROM Invoices_ListTemp where Invoice_Type=" + Invoice_Type;
            DateTime dtStart = new DateTime(Invoice_Date.Year, Invoice_Date.Month, Invoice_Date.Day, 0, 0, 0);
            DateTime dtEnd = new DateTime(Invoice_Date.Year, Invoice_Date.Month, Invoice_Date.Day, 23, 59, 59);




            if (Common.UseInvoiceNoByDay_POS == 1)
            {

                //mysql = mysql + " And Invoice_Date >='" + operations.Get_DateTimeFormate(dtStart) + "'  And Invoice_Date <='" + operations.Get_DateTimeFormate(dtEnd) + "'";
                mysql = mysql + " AND(YEAR(Invoice_Date) = " + Invoice_Date.Year + ") AND(MONTH(Invoice_Date) = " + Invoice_Date.Month + ") AND(DAY(Invoice_Date) = " + Invoice_Date.Day + ") ";
            }

            long Max_Invoice_Id = 0;

            operations opp = new operations();
            DataSet ds = opp.return_dataset(mysql, "Invoices_ListTemp");
            try
            {
                Max_Invoice_Id = long.Parse(ds.Tables["Invoices_ListTemp"].Rows[0]["Max_Invoice_Id"].ToString()) + 1;
            }
            catch
            {
                Max_Invoice_Id = 1;
            }
            opp = null;
            ds = null;
            return Max_Invoice_Id;
        }

        public void Make_InvoiceTemp(ref int Excp, ref string Msg, InvoicesOperationListCLS invL_O, InvoicesOperationCLS invD_O)
        {
            operations opp = new operations(1);

            try
            {
                string mysql = "";
                try
                {
                    invL_O.invL.CustomerName = Common.GetCustomerName(ref opp, invL_O.invL.Customer_Id.Value);
                }
                catch { }
                //var serializedParent = JsonConvert.SerializeObject(invL_O.invL);
                //Invoices_ListTemp invoices_ListTemp = JsonConvert.DeserializeObject<Invoices_ListTemp>(serializedParent);

                Add_UpdateInvoice_List(ref Excp, ref opp, invL_O);
                //if (invL_O.invL.Id == 0)
                //{

                //    //New_Invoice_Id = invL_O.Insert_Invoices_ListTemp(ref opp, ref Excp, invoices_ListTemp);
                //    //New_Invoice_Id = invL_O.Insert_Invoices_ListTemp(ref opp, ref Excp, (Invoices_ListTemp)invL_O.invL);
                //    New_Invoice_Id = invL_O.Insert_Invoices_ListTemp(ref opp, ref Excp, invL_O.invL);
                //}
                //else
                //{
                //    Invoices_ListTemp invlst = invL_O.Get_Invoices_ListTemp(ref opp, invL_O.invL.Id);
                //    if (invlst == null)
                //    {
                //        //New_Invoice_Id = invL_O.Insert_Invoices_ListTemp(ref opp, ref Excp, invoices_ListTemp);
                //        New_Invoice_Id = invL_O.Insert_Invoices_ListTemp(ref opp, ref Excp, invL_O.invL);
                //    }
                //    else
                //    {
                //        opp.db.Invoices_ListTemp.Attach(invlst);
                //        opp.db.Entry(invlst).CurrentValues.SetValues(invL_O.invL);
                //        opp.db.SaveChanges();
                //        //opp.db.Entry((Invoices_List)invL_O.invL).State = EntityState.Modified;
                //        New_Invoice_Id = invL_O.invL.Id;
                //    }
                //}

                //New_Invoice_Id = 1000000;
                if (Excp == 1)
                {
                    opp.db.Entry((Invoices_ListTemp)invL_O.invL).State = EntityState.Detached;
                    New_Invoice_Id = 0;
                    invL_O.Invoices_List_Id = 0;
                    opp.Rollback();
                    return;
                }


                InvoicesOperationCLS invoicesOperationCLS = new InvoicesOperationCLS();

                //invoicesOperationCLS.Delete_Invoices_DetailsTemp(ref opp, ref Excp, invL_O.invL.Id);
                 

                for (int x = 0; x < invD_O.invDlst.Count; x++)
                {

                    invD_O.invDlst[x].Invoice_Id = New_Invoice_Id;

                    try
                    {
                        if (invD_O.invDlst[x].Store_Name.ToString().Trim() == "")
                        {
                            invD_O.invDlst[x].Store_Name = Common.GetStoreName(ref opp, invD_O.invDlst[x].Store_Id ?? 0);
                        }
                    }
                    catch { }
                    invD_O.invDlst[x].Item_DeletedFlage = 100;
                    invD_O.invDlst[x].Item_ProcessedFlage = 100;

                    //var settings = new JsonSerializerSettings
                    //{
                    //    // ÊÌÇåá ÇáÏæÑÇä ÇááÇäåÇÆí ÈÏáÇð ãä ÅáÞÇÁ ÎØÃ
                    //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,

                    //    // ãäÚ ÇáÜ Entity Framework ãä Úãá Proxies ÃËäÇÁ ÇáÊÍæíá áíÞÑÃ ÇáßÇÆä ÇáÃÕáí
                    //    ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver()
                    //};
                    //var serializedDetails = JsonConvert.SerializeObject(invD_O.invDlst[x], settings);
                    //Invoices_DetailsTemp invoices_Details = JsonConvert.DeserializeObject<Invoices_DetailsTemp>(serializedDetails);


                    invD_O.Invoices_Details_Id = invD_O.Insert_Invoices_DetailsTemp(ref opp, ref Excp, invD_O.invDlst[x]);


                    if (Excp == 1)
                    {
                        New_Invoice_Id = 0;
                        invL_O.Invoices_List_Id = 0;
                        opp.Rollback();
                        return;
                    }


                }

                if (Excp == 1)
                {
                    New_Invoice_Id = 0;
                    invL_O.Invoices_List_Id = 0;
                    opp.Rollback();
                }
                else
                {
                    opp.Commit();
                    Excp = 0;
                }

            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
                Msg = ee.Message;
            }
        }
        void Add_UpdateInvoice_List(ref int Excp, ref operations opp, InvoicesOperationListCLS invL_O)
        {
            if (invL_O.invL.Id == 0)
            {

                New_Invoice_Id = invL_O.Insert_Invoices_ListTemp(ref opp, ref Excp, invL_O.invL);

            }
            else
            {

               
                    invL_O.Update_Invoices_ListTemp(ref opp, ref Excp, invL_O.invL);

                    //opp.db.Entry((Invoices_List)invL_O.invL).State = EntityState.Modified;
                    New_Invoice_Id = invL_O.invL.Id;
                 

                //else
                //{

                //    Invoices_ListTemp invlst = invL_O.Get_Invoices_ListTemp(ref opp, invL_O.invL.Id);
                //    if (invlst == null)
                //    {
                //        //New_Invoice_Id = invL_O.Insert_Invoices_List(ref opp, ref Excp, (Invoices_List)invL_O.invL);
                //        New_Invoice_Id = invL_O.Insert_Invoices_ListTemp(ref opp, ref Excp, invL_O.invL);
                //    }
                //    else
                //    {
                //        invL_O.Update_Invoices_ListTemp(ref opp, ref Excp, invL_O.invL);

                //        //opp.db.Entry((Invoices_List)invL_O.invL).State = EntityState.Modified;
                //        New_Invoice_Id = invL_O.invL.Id;
                //    }


            }
        }
        public void MakeEdit_InvoiceTemp(ref int Excp, ref string Msg, InvoicesOperationListCLS invL_O, InvoicesOperationCLS invD_O)
        {
            operations opp = new operations(1);

            try
            {
                string mysql = "";
                try
                {
                    if (invL_O.invL.CustomerName.ToString().Trim() == "")
                    {
                        invL_O.invL.CustomerName = Common.GetCustomerName(ref opp, invL_O.invL.Customer_Id.Value);
                    }
                }
                catch { }
                //var serializedParent = JsonConvert.SerializeObject(invL_O.invL);
                //Invoices_ListTemp invoices_ListTemp = JsonConvert.DeserializeObject<Invoices_ListTemp>(serializedParent);

                Add_UpdateInvoice_List(ref Excp, ref opp, invL_O);
                //if (invL_O.invL.Id == 0)
                //{

                //    //New_Invoice_Id = invL_O.Insert_Invoices_ListTemp(ref opp, ref Excp, invoices_ListTemp);
                //    //New_Invoice_Id = invL_O.Insert_Invoices_ListTemp(ref opp, ref Excp, (Invoices_ListTemp)invL_O.invL);
                //    New_Invoice_Id = invL_O.Insert_Invoices_ListTemp(ref opp, ref Excp, invL_O.invL);
                //}
                //else
                //{
                //    Invoices_ListTemp invlst = invL_O.Get_Invoices_ListTemp(ref opp, invL_O.invL.Id);
                //    if (invlst == null)
                //    {
                //        //New_Invoice_Id = invL_O.Insert_Invoices_ListTemp(ref opp, ref Excp, invoices_ListTemp);
                //        New_Invoice_Id = invL_O.Insert_Invoices_ListTemp(ref opp, ref Excp, invL_O.invL);
                //    }
                //    else
                //    {
                //        opp.db.Invoices_ListTemp.Attach(invlst);
                //        opp.db.Entry(invlst).CurrentValues.SetValues(invL_O.invL);
                //        opp.db.SaveChanges();
                //        //opp.db.Entry((Invoices_List)invL_O.invL).State = EntityState.Modified;
                //        New_Invoice_Id = invL_O.invL.Id;
                //    }
                //}

                //New_Invoice_Id = 1000000;
                if (Excp == 1)
                {
                    opp.db.Entry((Invoices_ListTemp)invL_O.invL).State = EntityState.Detached;
                    New_Invoice_Id = 0;
                    invL_O.Invoices_List_Id = 0;
                    opp.Rollback();
                    return;
                }


                InvoicesOperationCLS invoicesOperationCLS = new InvoicesOperationCLS();

                //invoicesOperationCLS.Delete_Invoices_DetailsTemp(ref opp, ref Excp, invL_O.invL.Id);
                //InvoiceDataSetCLS ii = new InvoiceDataSetCLS();
                //List<Invoices_DetailsTemp> invDTemplst = invD_O.Get_Invoices_DetailsTempLst(ref Excp, invD_O.invDlst);




                for (int x = 0; x < invD_O.invDlst.Count; x++)
                {

                    invD_O.invDlst[x].Invoice_Id = New_Invoice_Id;



                    //invD_O.Invoices_Details_Id = invD_O.Insert_Invoices_DetailsTemp(ref opp, ref Excp, invD_O.invDlst[x]);
                    //Invoices_DetailsTemp invDTemp= invD_O.Get_Invoices_DetailsTemp(ref Excp, invD_O.invDlst[x]);
                    invD_O.invDlst[x].Id = invD_O.Insert_Or_Update_Invoices_DetailsTemp(ref opp, ref Excp, invD_O.invDlst[x], New_Invoice_Id);

                    if (Excp == 1)
                    {
                        New_Invoice_Id = 0;
                        invL_O.Invoices_List_Id = 0;
                        opp.Rollback();
                        return;
                    }


                }
                for (int x = 0; x < invD_O.invDlst_Deleted.Count; x++)
                { 
                    if (invD_O.invDlst_Deleted[x].Item_ProcessedFlage == 1)
                    {
                        mysql = "Delete From Invoices_DetailsTemp  where id=" + invD_O.invDlst_Deleted[x].Id;
                        opp.insert_me(mysql); 
                          }
               
                }

                List<Invoices_DetailsTemp> invoices_DetailsLst = opp.db.Invoices_DetailsTemp.AsNoTracking().Where(i => i.Invoice_Id == invL_O.invL.Id).ToList();

                if (invoices_DetailsLst.Count == 0)
                {
                    //IsDeleteInvoices_List = true;
                    mysql = "Delete From Invoices_ListTemp  where id=" + invL_O.invL.Id;
                    opp.insert_me(mysql);

                    //Invoice_List_Table_Id = 0;
                    //invL.Invoices_List_Id = 0;
                }
                else
                {
 
                }
                opp.db.SaveChanges();
                if (Excp == 1)
                {
                    New_Invoice_Id = 0;
                    invL_O.Invoices_List_Id = 0;
                    opp.Rollback();
                }
                else
                {
                    opp.Commit();
                    Excp = 0;
                }

            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
                Msg = ee.Message;
            }
        }

        public void Delete_InvoiceTem(ref int Excp, ref string Msg, long Invoices_ListTempId)
        {
            operations opp = new operations(1);
            Delete_InvoiceTem(ref Excp, ref Msg, ref opp, Invoices_ListTempId);
        }
        public void Delete_InvoiceTem(ref int Excp, ref string Msg, ref operations opp, long Invoices_ListTempId)
        {
            
            Invoices_ListTempCls invoices_ListTempCls = new Invoices_ListTempCls();

            
            try
            {

                invoices_ListTempCls.DeleteInvoices_ListTemp(ref Excp, ref Msg, ref opp, Invoices_ListTempId);
                 

            }
            catch (Exception ee)
            {
                 
                Excp = 1;
                Msg = ee.Message;
            }
        }
        public void ChangeStatus_InvoiceTem(ref int Excp, ref string Msg, long Invoices_ListTempId,int Status)
        {
            operations opp = new operations(1);
            ChangeStatus_InvoiceTem(ref Excp, ref Msg, ref opp, Invoices_ListTempId, Status);
        }
        public void ChangeStatus_InvoiceTem(ref int Excp, ref string Msg, ref operations opp, long Invoices_ListTempId, int Status)
        {

            Invoices_ListTempCls invoices_ListTempCls = new Invoices_ListTempCls();


            try
            {

                invoices_ListTempCls.ChangeStatus_Invoices_ListTemp(ref Excp, ref Msg, ref opp, Invoices_ListTempId, Status);
                 

            }
            catch (Exception ee)
            {
                 
                Excp = 1;
                Msg = ee.Message;
            }
        }

    }
}
