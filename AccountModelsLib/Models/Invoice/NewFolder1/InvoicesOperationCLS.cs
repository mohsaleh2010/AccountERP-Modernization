using AccountModelsLib.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AccountModelsLib.Models
{
    public class InvoicesOperationCLS
    {
        public operations opp;
        public InvoicesOperationCLS()
        {
            opp = new operations();
            invD = new Invoices_Details();
        }
        public InvoicesOperationCLS(ref operations opp_)
        {
            opp = opp_;
            invD = new Invoices_Details();
        }
        public InvoicesOperationCLS(ref operations opp_, int isReverse)
        {
            opp = opp_;
            if (isReverse == 1)
            {
                invD = new Invoices_Details_Reverse();
            }
            else
            {
                invD = new Invoices_Details();
            }

        }
        public InvoicesOperationCLS(int isReverse)
        {
            if (opp == null)
            {
                opp = new operations();
            }
            if (isReverse == 1)
            {
                invD = new Invoices_Details_Reverse();
            }
            else
            {
                invD = new Invoices_Details();
            }

        }
        public InvoicesOperationCLS(int isReverse,Invoices_DetailsTemp invoices_DetailsTemp)
        {
            if (opp == null)
            {
                opp = new operations();
            }
            //if (isReverse == 1)
            //{
            //    invD = new Invoices_Details_Reverse();
            //}
            //else
            //{
            //    invD = new Invoices_Details();
            //}

        }
        public IInvoices_Details invD = new IInvoices_Details();
        public List<IInvoices_Details> invDlst = new List<IInvoices_Details>();
        public List<IInvoices_Details> invDlst_Deleted = new List<IInvoices_Details>();


        public long Invoices_List_Id { get; set; } = 0;
        public long Invoices_Details_Id { get; set; } = 0;
        public Invoices_ListCls invCls = new Invoices_ListCls();
        public decimal Small_Unit_Value { get; set; } = 0;
        /// ////////////////////////////////////////////////  <summary>

        //public decimal No3eboa { get; set; }

        public int Is_Buy_From_Bills_Items_Balance { get; set; }


        public long Insert_Invoices_Details(ref operations opp, ref int Excp, Invoices_Details ind)
        {
            InvoiceDetailsCLS invoiceDetailsCLS = new InvoiceDetailsCLS();
            return invoiceDetailsCLS.Insert_Invoices_Details(ref opp, ref Excp, ind);

        }
        public Invoices_Details Insert_Invoices_Details0000(ref operations opp, ref int Excp, IInvoices_Details ind)
        {

            var settings = new JsonSerializerSettings
            {
                // تجاهل الدوران اللانهائي بدلاً من إلقاء خطأ
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                // منع الـ Entity Framework من عمل Proxies أثناء التحويل ليقرأ الكائن الأصلي
                ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver()
            };
            var serializedDetails = JsonConvert.SerializeObject(ind, settings);
            Invoices_Details invoices_Details = JsonConvert.DeserializeObject<Invoices_Details>(serializedDetails);



            InvoiceDetailsCLS invoiceDetailsCLS = new InvoiceDetailsCLS();
              invoiceDetailsCLS.Insert_Invoices_Details(ref opp, ref Excp, invoices_Details);

            ind.Id = invoices_Details.Id; // تحديث الـ Id في الكائن الأصلي بعد الإدراج
            return invoices_Details;
           // return invoices_Details.Id;


        }

        public Invoices_Details Insert_Invoices_Details(ref operations opp, ref int Excp, IInvoices_Details ind)
        {


        

            //Invoices_Details invoices_Details = new Invoices_Details();
            //Invoices_Details invoices_Details = MappingHelper.MapScalarPropertiesEFForNewAdd(ref    opp,   ind );

            //opp.db.Invoices_Details.Attach(invoices_Details);
            //var currentValues = opp.db.Entry(invoices_Details).CurrentValues;

            //foreach (var propertyName in currentValues.PropertyNames)
            //{
            //    // 🛑 الشرط الجوهري: إذا كان الحقل هو الـ Primary Key (اسم الحقل Id)، نتجاهله تماماً ولا نلمسه!
            //    if (propertyName == "Id") continue;

            //    // نسخ قيمة الحقل الحالي من الكائن القادم من الشاشة إلى الكائن الجديد
            //    currentValues[propertyName] = opp.db.Entry(ind).Property(propertyName).CurrentValue;
            //}

            ////opp.db.Entry(invoices_Details).CurrentValues.SetValues(ind);
            ////var values = opp.db.Entry(ind).CurrentValues;
            ////opp.db.Entry(invoices_DetailsTemp).CurrentValues.SetValues(values);

            ////opp.db.Entry(invoices_Details).State = EntityState.Added;
            Invoices_Details invoices_Details = MappingHelper.MapScalarProperties<Invoices_Details, IInvoices_Details>(ind);
            InvoiceDetailsCLS invoiceDetailsCLS = new InvoiceDetailsCLS();
            invoiceDetailsCLS.Insert_Invoices_Details(ref opp, ref Excp, invoices_Details);
            //Test2(ref opp,ind);
            ind.Id = invoices_Details.Id; // تحديث الـ Id في الكائن الأصلي بعد الإدراج
            return invoices_Details;
            // return invoices_Details.Id;

            


        }
  
        Invoices_Details Test(ref operations opp, IInvoices_Details ind)
        {
            Invoices_Details invoices_Details = new Invoices_Details();

            // جلب جميع الخصائص (Properties) الخاصة بالكائن مباشرة من الذاكرة
            var properties = invoices_Details.GetType().GetProperties();

            foreach (var prop in properties)
            {
                // الحصول على اسم الخاصية (مثل: Item_Id, Qty, Price)
                string propertyName = prop.Name;

                // 🛑 تجاهل الـ Primary Key
                if (propertyName == "Id" | prop.PropertyType.IsClass ) continue;

                if(prop.PropertyType.IsClass)
                {
                    continue;
                }

                
                // يمكنك الآن قراءة أو كتابة القيم للكائن مباشرة
                // مثال لقراءة القيمة من كائن الشاشة ind وضعه في الكائن الجديد:
                var sourceProp = ind.GetType().GetProperty(propertyName);
                if (sourceProp != null && sourceProp.CanRead && prop.CanWrite)
                {
                    var val = sourceProp.GetValue(ind, null);
                    prop.SetValue(invoices_Details, val, null);
                }
            }
            opp.db.Invoices_Details.Add(invoices_Details);
            opp.db.SaveChanges();
            return invoices_Details;
        }

        public long Insert_Invoices_DetailsTemp(ref operations opp, ref int Excp, Invoices_DetailsTemp ind)
        {
            InvoiceDetailsTempCLS invoiceDetailsCLS = new  InvoiceDetailsTempCLS ();
            return invoiceDetailsCLS.Insert_Invoices_Details(ref opp, ref Excp, ind);

        }
        public long Insert_Invoices_DetailsTemp(ref operations opp, ref int Excp, IInvoices_Details ind)
        {
            //var settings = new JsonSerializerSettings
            //{
            //    // تجاهل الدوران اللانهائي بدلاً من إلقاء خطأ
            //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            //    // منع الـ Entity Framework من عمل Proxies أثناء التحويل ليقرأ الكائن الأصلي
            //    ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver()
            //};
            //var serializedDetails = JsonConvert.SerializeObject(ind, settings);
            //Invoices_DetailsTemp invoices_DetailsTemp = JsonConvert.DeserializeObject<Invoices_DetailsTemp>(serializedDetails);

            Invoices_DetailsTemp invoices_DetailsTemp = MappingHelper.MapScalarProperties<Invoices_DetailsTemp, IInvoices_Details>(ind, false );

            InvoiceDetailsTempCLS invoiceDetailsCLS = new InvoiceDetailsTempCLS();
              invoiceDetailsCLS.Insert_Invoices_Details(ref opp, ref Excp, invoices_DetailsTemp);
            ind.Id= invoices_DetailsTemp.Id; // تحديث الـ Id في الكائن الأصلي بعد الإدراج

            return invoices_DetailsTemp.Id;
        }

       public void Insert_Or_Update_Invoices_DetailsLst(ref operations opp, ref int Excp, List<Invoices_DetailsTemp> inDLstNew,long New_Invoice_Id)
        {
            InvoiceDetailsTempCLS invoiceDetailsTempCLS = new InvoiceDetailsTempCLS();
            invoiceDetailsTempCLS.Insert_Or_Update_Invoices_DetailsLst(ref opp, ref Excp, inDLstNew, New_Invoice_Id);
        }
        public long Insert_Or_Update_Invoices_DetailsTemp(ref operations opp, ref int Excp, Invoices_DetailsTemp inDNew, long New_Invoice_Id)
        {
            InvoiceDetailsTempCLS invoiceDetailsTempCLS = new InvoiceDetailsTempCLS();
            return invoiceDetailsTempCLS.Insert_Or_Update_Invoices_Details(ref opp, ref Excp, inDNew, New_Invoice_Id);
        }
        public long Insert_Or_Update_Invoices_DetailsTemp(ref operations opp, ref int Excp, IInvoices_Details iinDNew, long New_Invoice_Id )
        {
            bool IsToUpdate = iinDNew.Item_ProcessedFlage != 0;
            Invoices_DetailsTemp invDTemp =  Get_Invoices_DetailsTemp(ref Excp, iinDNew, IsToUpdate);

            InvoiceDetailsTempCLS invoiceDetailsTempCLS = new InvoiceDetailsTempCLS();
            return invoiceDetailsTempCLS.Insert_Or_Update_Invoices_Details(ref opp, ref Excp, invDTemp, New_Invoice_Id);
        }
        public List<Invoices_DetailsTemp> Get_Invoices_DetailsTempLst( ref int Excp, List<IInvoices_Details> indLst)
        {
            List<Invoices_DetailsTemp> indTempList = new List<Invoices_DetailsTemp>();
            var settings = new JsonSerializerSettings
            {
                // تجاهل الدوران اللانهائي بدلاً من إلقاء خطأ
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                // منع الـ Entity Framework من عمل Proxies أثناء التحويل ليقرأ الكائن الأصلي
                ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver()
            };

            foreach (var  ind in indLst)
            {
                var serializedDetails = JsonConvert.SerializeObject(ind, settings);
                Invoices_DetailsTemp invoices_DetailsTemp = JsonConvert.DeserializeObject<Invoices_DetailsTemp>(serializedDetails);
                indTempList.Add(invoices_DetailsTemp);
            }

            return indTempList;
        }
        public Invoices_DetailsTemp Get_Invoices_DetailsTemp(ref int Excp,IInvoices_Details ind,bool IsToUpdate=false )
        {
            
            //var settings = new JsonSerializerSettings
            //{
            //    // تجاهل الدوران اللانهائي بدلاً من إلقاء خطأ
            //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            //    // منع الـ Entity Framework من عمل Proxies أثناء التحويل ليقرأ الكائن الأصلي
            //    ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver()
            //}; 
            //    var serializedDetails = JsonConvert.SerializeObject(ind, settings);
            //    Invoices_DetailsTemp invoices_DetailsTemp = JsonConvert.DeserializeObject<Invoices_DetailsTemp>(serializedDetails);

            Invoices_DetailsTemp invoices_DetailsTemp = MappingHelper.MapScalarProperties<Invoices_DetailsTemp, IInvoices_Details>(ind, IsToUpdate);

            return invoices_DetailsTemp;
        }
        public void Delete_Invoices_DetailsTemp(ref operations opp, ref int Excp, long Invoice_ListTemp_Id)
        {
            InvoiceDetailsTempCLS invoiceDetailsCLS = new InvoiceDetailsTempCLS();
              invoiceDetailsCLS.Delete_Invoices_DetailsTemp(ref opp, ref Excp, Invoice_ListTemp_Id);

        }

        

        public void Update_Invoices_Details(ref operations opp, ref int Excp)
        {
            InvoiceDetailsCLS invoiceDetailsCLS = new InvoiceDetailsCLS();
            invoiceDetailsCLS.Insert_Invoices_DetailsOld(ref opp, ref Excp, invD); 
        }

        public void Update_Invoices_Details(ref operations opp, ref int Excp, Invoices_Details invD)
        {
            InvoiceDetailsCLS invoiceDetailsCLS = new InvoiceDetailsCLS();
            invoiceDetailsCLS.Update_Invoices_Details(ref opp, ref Excp, invD);
            Excp = 0;

         
        }
        public Invoices_Details GetInvoiceDetails(  Invoices_Details invD)
        {
            Invoices_Details invoices_Details = (Invoices_Details)invD;
            return invoices_Details;


        }
        public Invoices_Details GetInvoiceDetails(IInvoices_Details invD)
        {
            var settings = new JsonSerializerSettings
            {
                // تجاهل الدوران اللانهائي بدلاً من إلقاء خطأ
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,

                // منع الـ Entity Framework من عمل Proxies أثناء التحويل ليقرأ الكائن الأصلي
                ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver()
            };
            var serializedParent = JsonConvert.SerializeObject(invD, settings);
            Invoices_Details invoices_DetailsToUpdate = JsonConvert.DeserializeObject<Invoices_Details>(serializedParent);

             
            return invoices_DetailsToUpdate;
             
        }
        public Invoices_DetailsTemp GetInvoiceDetailsTemp(Invoices_DetailsTemp invD)
        {
            Invoices_DetailsTemp invoices_DetailsTemp = (Invoices_DetailsTemp)invD;
            return invoices_DetailsTemp;

            


        }
        public Invoices_DetailsTemp GetInvoiceDetailsTemp(IInvoices_Details invD)
        {
            var settings = new JsonSerializerSettings
            {
                // تجاهل الدوران اللانهائي بدلاً من إلقاء خطأ
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,

                // منع الـ Entity Framework من عمل Proxies أثناء التحويل ليقرأ الكائن الأصلي
                ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver()
            };
            var serializedParent = JsonConvert.SerializeObject(invD, settings);
            Invoices_DetailsTemp invoices_DetailsTemp = JsonConvert.DeserializeObject<Invoices_DetailsTemp>(serializedParent);
            return invoices_DetailsTemp;

        }
        //public long Insert_Invoices_Details_Reverse(ref operations opp, ref int Excp)
        //{
        //    InvoiceDetailsReverseCLS invoiceDetailsReverseCLS = new InvoiceDetailsReverseCLS();
        //    Invoices_Details_Id= invoiceDetailsReverseCLS.Insert_Invoices_Details_ReverseOLD(ref opp, ref Excp);

        //    return Invoices_Details_Id;
        //}
        public long Insert_Invoices_Details_Reverse(ref int Excp, ref operations opp,  Invoices_Details_Reverse invd_R)
        {
            InvoiceDetailsReverseCLS invoiceDetailsReverseCLS = new InvoiceDetailsReverseCLS();
            invd_R.Id= Invoices_Details_Id = invoiceDetailsReverseCLS.Insert_Invoices_Details_Reverse(ref opp, ref Excp, invd_R);
     
           
            return invd_R.Id;
        }
        public Invoices_Details_Reverse Insert_Invoices_Details_Reverse(ref int Excp, ref operations opp, IInvoices_Details ind)
        {
             

            Invoices_Details_Reverse invoices_Details = MappingHelper.MapScalarProperties<Invoices_Details_Reverse, IInvoices_Details>(ind);

            Insert_Invoices_Details_Reverse(ref Excp, ref opp, invoices_Details);
            ind.Id = invoices_Details.Id;  
            return invoices_Details;
            
        }
        public void Update_Invoices_Details_Reverse( ref int Excp, ref operations opp, IInvoices_Details invd_R)
        {
            Invoices_Details_Reverse invoices_Details = MappingHelper.MapScalarProperties<Invoices_Details_Reverse, IInvoices_Details>(invd_R);

            InvoiceDetailsReverseCLS invoiceDetailsReverseCLS = new InvoiceDetailsReverseCLS();
            invoiceDetailsReverseCLS.Update_Invoices_Details_Reverse(ref opp, ref Excp, invoices_Details);


        }
        public void Update_Invoices_Details_Reverse(  ref int Excp, ref operations opp, Invoices_Details_Reverse invd_R)
        {
            InvoiceDetailsReverseCLS invoiceDetailsReverseCLS = new InvoiceDetailsReverseCLS();
            invoiceDetailsReverseCLS.Update_Invoices_Details_Reverse(ref opp, ref Excp, invd_R);

           
        }
        
        public void GetInvoiceDetailsToEdit(long Invoices_ListId)
        {
            InvoiceDetailsCLS invoiceDetailsCLS = new InvoiceDetailsCLS();
             

            List<Invoices_Details> invD = invoiceDetailsCLS.GetInvoices_DetailsToEdit(ref opp, Invoices_ListId);

            invDlst = invD.Cast<IInvoices_Details>().ToList();
        }
        public void GetIInvoiceDetailsTemp(long Invoices_ListId)
        {
            operations opp = new operations();
            InvoiceDetailsTempCLS invoiceDetailsTempCLS = new InvoiceDetailsTempCLS();
            invDlst = invoiceDetailsTempCLS.GetIInvoiceDetailsTemp(  Invoices_ListId);
             
        }
        public void GetInvoiceDetails_Reverse(long invoice_List_Table_Id)
        {
            operations opp = new operations();
            InvoiceDetailsReverseCLS invoiceDetailsReverseCLS = new InvoiceDetailsReverseCLS();
            List<Invoices_Details_Reverse> invDR = invoiceDetailsReverseCLS.GetInvoices_Details_Reverse(ref opp, invoice_List_Table_Id);

            invDlst = invDR.Cast<IInvoices_Details>().ToList();
        }
    }
}