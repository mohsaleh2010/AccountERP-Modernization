using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
 
 
using System.Runtime.CompilerServices;
 
 
 
using System.Data;
namespace AccountModelsLib.Models
{
    public partial class CarsExaminationCLS
    {
        public AccountModelsLib.operations opp;
        public CarsExaminationCLS()
        {
            opp = new AccountModelsLib.operations();
        }
        public CarsExaminationCLS(ref AccountModelsLib.operations opp_)
        {
            opp = opp_;
        }
        public Car car = new Car();
        public CarsExamination carsExamination = new CarsExamination();

        public Customer customer = new Customer();


        public long GetNewExaminationNumber()
        {

            long ExaminationNumber = 0;
            int year = carsExamination.ExaminationDate.Value.Year;

            long? maxAge = opp.db.CarsExaminations.Where(p => p.ExaminationDate.Value.Year == year).Select(p => p.ExaminationNumber).DefaultIfEmpty(0).Max();

            //var estado = opp.db.CarsWorkOrders.Where(p => p.WorkOrdersDate.Value.Year == year).Max(c => c.WorkOrderNumber);

            //IQueryable<CarsWorkOrder> query = opp.db.CarsWorkOrders.Where(p => p.WorkOrdersDate.Value.Year == year);
            //long? v = query.Max(c => c.WorkOrderNumber);


            if (maxAge.HasValue)
            {
                if (maxAge.Value == 0)
                {
                    ExaminationNumber = 1;
                }
                else
                {
                    ExaminationNumber = maxAge.Value;
                    ExaminationNumber++;
                }

            }

            return ExaminationNumber;
        }
        public CarsExamination getCarsExaminationToPrint(long Id)
        {
            carsExamination = opp.db.CarsExaminations.AsNoTracking().Where(c => c.Id == Id).FirstOrDefault();
            return carsExamination;
        }
        public void getCarById(long Id)
        {
            car = opp.db.Cars.Where(c => c.Id == Id).FirstOrDefault();

        }
        public Customer getCarCustomerOwner(Car car)
        {

            CustomerCLS customerCLS = new CustomerCLS(ref opp);

            customerCLS.GetCustomerById(ref opp, car.CustomerId);
            return customerCLS.cs;

        }

        public void AddCarsExamination(ref int Excp, ref string msg)
        {

            try
            {
                opp.BeginTransaction(2);
                DataDictionaryProcessesNoCLS.GetProcessesNo(ref Excp, ref msg, ref opp, carsExamination);


                opp.db.CarsExaminations.Add(carsExamination);
                opp.db.SaveChanges();
                Excp = 0;

                if (carsExamination.CarsExaminationValue > 0)
                {
                    SetCustomerAccounts(ref Excp, ref msg, ref opp);
                }
                if (Excp == 0)
                {
                    opp.Commit();
                    Excp = 0;
                }
                else
                {
                    opp.Rollback();
                    Excp = 1;
                }
            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
                msg = ee.Message;
            }


        }
        void SetCustomerAccounts(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp)
        {
            AccountEntry_CLS aac = new AccountEntry_CLS();
            CustomersAccountCLS ca = new CustomersAccountCLS();

            aac.Reference_Type = Common.CarsExaminations_Reference_Type;
            aac.Reference_Type_Table_Id = carsExamination.Id;

            aac.Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, aac.Reference_Type_Table_Id, aac.Reference_Type, false);

            ca.Delete_Customer_Balance(ref Excp, ref opp, aac.Reference_Type_Table_Id, aac.Reference_Type);

            if (carsExamination.CarsExaminationValue == 0)
            {
                return;
            }


            try
            {
                int Transaction_Type = carsExamination.Transaction_Type;
                long Customer_Id = carsExamination.CustomerId;
                string mysql = "";
                string Notes = " تقرير فحص رقم " + carsExamination.ExaminationNumber.ToString();
                string OrderNo = carsExamination.ExaminationNumber.ToString();

                long Reference_Type_Table_Id = carsExamination.Id;
                long Reference_Type = Common.CarsExaminations_Reference_Type;  // 14  اصدار تقرير فحص سيارة

                decimal Order_Value = carsExamination.CarsExaminationValue ?? 0;
                decimal Balance = 0;
                DateTime OrderDate = carsExamination.ExaminationDate.Value;


                ca.Set_Customer_Balance_With_CarsExaminations(ref Excp, ref opp, carsExamination.CustomerId, Transaction_Type, carsExamination.Id, Order_Value, Notes, OrderDate, true);

                Msg = "";
                //انشاء القيد
                Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                aa.IsNotDeletePrevEntry = false;
                aa.Make_EntryForCarsExaminations(ref Excp, ref Msg, ref opp, Reference_Type_Table_Id, OrderDate, OrderDate, -1);

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }
        }


        public void EditCarsExamination(ref int Excp, ref string msg)
        {


            try
            {
                opp.BeginTransaction(2);
                opp.db.Entry(carsExamination).State = EntityState.Modified;
                opp.db.SaveChanges();
                Excp = 0;

                SetCustomerAccounts(ref Excp, ref msg, ref opp);

                opp.Commit();
                Excp = 0;
            }
            catch (Exception ee)
            {
                opp.Commit();
                Excp = 1;
                msg = ee.Message;
            }


        }
        public void getCarsExamination(long Id)
        {
            try
            {
                carsExamination = new CarsExamination();
                carsExamination = opp.db.CarsExaminations.Where(c => c.Id == Id).FirstOrDefault();

                if (carsExamination != null)
                {
                    if (carsExamination.CarsId == 0 | carsExamination.CarsId == null)
                    {
                        car = null;
                        customer = null;
                    }
                    else
                    {
                        GetCarData();

                    }
                }
            }
            catch (Exception ee)
            {


            }
        }
        public void GetCarData()
        {
            CarsCLS carCLS = new CarsCLS(ref opp);
            car = carCLS.getCar(carsExamination.CarsId ?? 0);
            customer = carCLS.getCarCustomerOwner(car);
        }
        public void DeletetCarsExamination(ref int excp, ref string msg)
        {
            try
            {
                opp.db.Entry(carsExamination).State = EntityState.Deleted;
                opp.db.SaveChanges();
                excp = 0;
            }
            catch (Exception ee)
            {
                excp = 1;
                msg = ee.Message;
            }
        }
          public DataTable  getCarsExamination(string searchStr, int Is_Net_Total_Paid, long Customer_Id,long ExaminationNumber,  bool IsEntryDateTimeFrom, DateTime EntryDateTimeFrom, bool IsEntryDateTimeTo, DateTime EntryDateTimeTo, int IsCarsExaminationClosed)
        {
            IQueryable<CarsExamination> query = opp.db.CarsExaminations.Where(i => i.Id > 0);
            string mysql = @" SELECT      '' as Serial,     USER_DETAILES.USER_NAME, Customer.Name, Customer.Id AS Customer_Id, CarsExaminations.* ";
            
            mysql = mysql + @" , ISNULL((SELECT SUM(Import_Value) - SUM(Export_Value) AS Expr1 FROM Money_Transaction_Box WHERE  (DataDictionaryProcessesListsId = "+Common.CarsExaminations_Reference_Type+") AND (Reference_Type_Table_Id = CarsExaminations.Id) ), 0) AS PaymentValue ";
             string StillValues = @"    CarsExaminations.CarsExaminationValue - ISNULL((SELECT SUM(Import_Value) - SUM(Export_Value) AS Expr1 FROM Money_Transaction_Box AS Money_Transaction_Box_1 WHERE  (DataDictionaryProcessesListsId = "+Common.CarsExaminations_Reference_Type+") AND (Reference_Type_Table_Id = CarsExaminations.Id)), 0)   ";

            mysql = mysql + @" , IIF (" + StillValues + " > 0 , " + StillValues + ", 0 ) As StillValues ";

            mysql = mysql + @" FROM            CarsExaminations INNER JOIN
                         Customer ON CarsExaminations.CustomerId = Customer.Id INNER JOIN
                         USER_DETAILES ON CarsExaminations.User_Id = USER_DETAILES.USER_ID ";
            //mysql = mysql + " WHERE (Invoices_List.Invoice_Type = " + Invoice_Type + ") ";
            mysql = mysql + " WHERE (CarsExaminations.Id > 0  )  ";

            //AND(Invoices_List.Is_DeletedProcessed<> 1)


                if (IsEntryDateTimeFrom == true)
                {
                    //mysql = mysql + " AND  (Invoices_List.Invoice_Date >='" + AccountModelsLib.operations.dateFormat(DateFrom) + "')   ";
                    mysql = mysql + " AND (CONVERT(DATE, CarsExaminations.ExaminationDate)  >= '" + AccountModelsLib.operations.dateFormat(EntryDateTimeFrom.Date) + "') ";

                }
           
                if (IsEntryDateTimeTo == true)
                {
                    //mysql = mysql + "   AND (Invoices_List.Invoice_Date <='" + AccountModelsLib.operations.dateFormat(DateTo) + "') ";
                    mysql = mysql + " AND (CONVERT(DATE, CarsExaminations.ExaminationDate)  <= '" + AccountModelsLib.operations.dateFormat(EntryDateTimeTo.Date) + "') ";

                }

                if (Customer_Id > 0)
                {
                    mysql = mysql + "  AND (Customer.Id = " + Customer_Id + ")";
                }
                 
                if (ExaminationNumber > 0)
                {
                    mysql = mysql + "  AND (CarsExaminations.ExaminationNumber=" + ExaminationNumber + ") ";
                }
            if (IsCarsExaminationClosed > -1)
            {
                mysql = mysql + "  AND (CarsExaminations.IsCarsExaminationClosed=" + IsCarsExaminationClosed + ") ";
            
            }

            if (Is_Net_Total_Paid > -1)
                { 

                mysql = mysql + @" AND (CarsExaminations.CarsExaminationValue -  ISNULL((SELECT        SUM(Import_Value) - SUM(Export_Value) AS Expr1 FROM Money_Transaction_Box AS Money_Transaction_Box_1 ";
                    //مدفوعة
                    if (Is_Net_Total_Paid == 1)
                    {

                        mysql = mysql + @" WHERE (DataDictionaryProcessesListsId = "+Common.CarsExaminations_Reference_Type+") AND (Reference_Type_Table_Id = CarsExaminations.Id)), 0)  = 0)";
                    }

                    // غير مدفوعة
                    if (Is_Net_Total_Paid == 0)
                    {

                        mysql = mysql + @" WHERE (DataDictionaryProcessesListsId = ""+Common.CarsExaminations_Reference_Type+"") AND (Reference_Type_Table_Id = CarsExaminations.Id) ), 0)  <> 0)";
                    }
                }
            
            return opp.return_DataTable(mysql);
        }

        public List<CarsExamination> getCarsExamination(string searchStr, bool IsEntryDateTimeFrom, DateTime EntryDateTimeFrom, bool IsEntryDateTimeTo, DateTime EntryDateTimeTo, int IsCarsExaminationClosed)
        {
            IQueryable<CarsExamination> query = opp.db.CarsExaminations.Where(i => i.Id > 0);
            if (searchStr.Trim() != "")
            {
                query = query.Where(i => i.CustomerName.Contains(searchStr) | i.CarNumber.Contains(searchStr) | i.ChassisNumber.Contains(searchStr));
            }
            if (IsCarsExaminationClosed > -1)
            {
                DateTime dat = EntryDateTimeFrom.Date;
                query = query.Where(i => i.IsCarsExaminationClosed == IsCarsExaminationClosed);
            }


            if (IsEntryDateTimeFrom == true)
            {
                DateTime dat = EntryDateTimeFrom.Date;
                query = query.Where(i =>  i.ExaminationDate.Value.Date >= dat);
            }
            if (IsEntryDateTimeTo == true)
            {
                DateTime dat = EntryDateTimeTo.Date;
                query = query.Where(i =>  i.ExaminationDate.Value.Date <= dat);
                //query = query.Where(i => i.EntryDateTime <= EntryDateTimeTo);
            }

            return query.ToList();
        }
    }
}
