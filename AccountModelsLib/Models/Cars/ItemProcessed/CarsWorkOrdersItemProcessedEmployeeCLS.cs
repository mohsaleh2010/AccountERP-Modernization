using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    public class CarsWorkOrdersItemProcessedEmployeeCLS
    {
        public List<CarsWorkOrdersItemProcessedEmployee> GetCarsWorkOrdersItemProcessedEmployeeList(ref AccountModelsLib.operations opp, long CarsWorkOrdersItemProcessedDetailsId)
        {
            List<CarsWorkOrdersItemProcessedEmployee> crsWorkOrdersItemProcessedEmployee = opp.db.CarsWorkOrdersItemProcessedEmployees.Where(c => c.CarsWorkOrdersItemProcessedDetailsId == CarsWorkOrdersItemProcessedDetailsId).ToList();
            return crsWorkOrdersItemProcessedEmployee;
        }

        public void Add(ref AccountModelsLib.operations opp, List<CarsWorkOrdersItemProcessedEmployee> newEmpList, long CarsWorkOrdersItemProcessedDetailsId)
        {
            foreach (var itmEmp in newEmpList)
            {

                itmEmp.CarsWorkOrdersItemProcessedDetailsId = CarsWorkOrdersItemProcessedDetailsId;
                opp.db.CarsWorkOrdersItemProcessedEmployees.Add(itmEmp);
                opp.db.SaveChanges();
            }
        }
        public void AddRevsrse(ref AccountModelsLib.operations opp, List<CarsWorkOrdersItemProcessedEmployee> newEmpList, long CarsWorkOrdersItemReversedProcessedDetailsId)
        {
            foreach (var itmEmp in newEmpList)
            {

                itmEmp.CarsWorkOrdersItemReversedProcessedDetailsId = CarsWorkOrdersItemReversedProcessedDetailsId;
                opp.db.CarsWorkOrdersItemProcessedEmployees.Add(itmEmp);
                opp.db.SaveChanges();
            }
        }
        public void Add(ref AccountModelsLib.operations opp, CarsWorkOrdersItemProcessedEmployee newEmp)
        {
            opp.db.CarsWorkOrdersItemProcessedEmployees.Add(newEmp);
            opp.db.SaveChanges();

        }

        public void DeleteCarsWorkOrdersItemProcessedEmployee(ref AccountModelsLib.operations opp, long id)
        {
            CarsWorkOrdersItemProcessedEmployee emp = opp.db.CarsWorkOrdersItemProcessedEmployees.Where(i => i.Id == id).FirstOrDefault();
            if (emp != null)
            {
                opp.db.CarsWorkOrdersItemProcessedEmployees.Remove(emp);
                opp.db.SaveChanges();
            }

        }

        public void Edit(ref int excp, ref string msg, ref AccountModelsLib.operations opp, List<CarsWorkOrdersItemProcessedEmployee> empProcessList)
        {
            try
            {
                foreach (var itmEmp in empProcessList)
                {
                    opp.db.Entry(itmEmp).State = EntityState.Modified;
                    //opp.db.CarsWorkOrdersItemProcessedEmployees.Add(itmEmp);
                    opp.db.SaveChanges();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<ItemProcessedEmployeeViewModel> GetItemProcessedEmployeeViewModel(ref int excp, ref string msg, ref AccountModelsLib.operations opp, bool IsHaveValue, long Item_Category_Id, long Emp_Id, DateTime WorkOrdersDateFrom, DateTime WorkOrdersDateTo)
        {
            List<ItemProcessedEmployeeViewModel> itmEmpProcessedVMLst = new List<ItemProcessedEmployeeViewModel>();
            //List<CarsWorkOrdersItemProcessedEmployee> empProcessList = opp.db.CarsWorkOrdersItemProcessedEmployees.Where(e => e.Emp_Id == Emp_Id).Include(e => e.CarsWorkOrder).Include(e => e.carsWorkOrdersItemProcessedDetail).ToList();
            IQueryable<CarsWorkOrdersItemProcessedEmployee> empProcessList = opp.db.CarsWorkOrdersItemProcessedEmployees.AsNoTracking();
            empProcessList = empProcessList.Where(i=>i.CarsWorkOrder.IsWorkOrderClosed==1);
            if (Emp_Id > 0)
            {
                empProcessList = empProcessList.Where(e => e.Emp_Id == Emp_Id);
            }
            if (IsHaveValue == true)
            {
                empProcessList = empProcessList.Where(i => i.Value > 0);

            }
            if (Item_Category_Id > 0)
            {
                empProcessList = empProcessList.Where(i => i.carsWorkOrdersItemProcessedDetail.Item_Category_Id == Item_Category_Id);
            }
            DateTime datFrom = WorkOrdersDateFrom.Date;
            empProcessList = empProcessList.Where(i => i.CarsWorkOrder.WorkOrdersDate.Value.Date >= datFrom);
            DateTime datTo = WorkOrdersDateTo.Date;
            empProcessList = empProcessList.Where(i => i.CarsWorkOrder.WorkOrdersDate.Value.Date <= datTo);

            empProcessList = empProcessList.Include(e => e.CarsWorkOrder).Include(e => e.carsWorkOrdersItemProcessedDetail);
            empProcessList = empProcessList.OrderBy(o => o.CarsWorkOrder.WorkOrderNumber);
            empProcessList = empProcessList.OrderBy(o => o.carsWorkOrdersItemReversedProcessedDetail.Item_Category_Id);
            empProcessList = empProcessList.OrderBy(o => o.carsWorkOrdersItemReversedProcessedDetail.Item_Id);

            decimal TotalValue = 0;
            decimal QntyValue = 0;
            long Serial = 1;
            foreach (var emp in empProcessList.ToList())
            {
                ItemProcessedEmployeeViewModel itm = new ItemProcessedEmployeeViewModel();
                itm.Serial = Serial;
                Serial++;
                if (emp.carsWorkOrdersItemProcessedDetail != null)
                {
                    itm.Qnty = emp.carsWorkOrdersItemProcessedDetail.Qnty;
                    itm.Item_Category_Name = emp.carsWorkOrdersItemProcessedDetail.Item_Category_Name;
                    itm.Item_Category_Id = emp.carsWorkOrdersItemProcessedDetail.Item_Category_Id;
                    itm.Item_Net_Total_Price = emp.carsWorkOrdersItemProcessedDetail.Item_Net_Total_Price ?? 0;
                    itm.Qnty_value = emp.carsWorkOrdersItemProcessedDetail.Qnty_value ?? 0;


                    itm.PriceUnit = emp.carsWorkOrdersItemProcessedDetail.PriceUnit;
                    itm.UnitStr = emp.carsWorkOrdersItemProcessedDetail.UnitStr;

                    itm.Invoice_Item_Name = emp.carsWorkOrdersItemProcessedDetail.Invoice_Item_Name + " " + emp.carsWorkOrdersItemProcessedDetail.Specification;

                    itm.Specification = emp.carsWorkOrdersItemProcessedDetail.Specification;

                    QntyValue = QntyValue + emp.carsWorkOrdersItemProcessedDetail.Qnty_value ?? 0;
                }
                if (emp.CarsWorkOrder != null)
                {

                    itm.WorkOrderNumber = emp.CarsWorkOrder.WorkOrderNumber;
                    itm.WorkOrdersDate = emp.CarsWorkOrder.WorkOrdersDate;

                }
                itm.CarsWorkOrdersId = emp.CarsWorkOrdersId;
                itm.CarsWorkOrdersItemProcessedDetailsId = emp.CarsWorkOrdersItemProcessedDetailsId;
                itm.CarsWorkOrdersItemReversedProcessedDetailsId = emp.CarsWorkOrdersItemReversedProcessedDetailsId;


                itm.Id = emp.Id;
                itm.CarsId = emp.CarsWorkOrdersId;
                itm.Emp_Id = emp.Emp_Id;
                itm.Emp_Name = emp.Emp_Name;
                itm.Ratio = emp.Ratio ?? 0;
                itm.Value = emp.Value ?? 0;
                itm.Notes = emp.Notes;

                TotalValue = TotalValue + emp.Value ?? 0;



                itmEmpProcessedVMLst.Add(itm);
            }

            ItemProcessedEmployeeViewModel itmT = new ItemProcessedEmployeeViewModel();
            itmT.Emp_Name = "الإجمالى";
            itmT.Ratio = 0;
            itmT.Value = TotalValue;
            itmT.Qnty_value = QntyValue;
            itmT.Notes = "";
            itmT.Id = 0;
            itmEmpProcessedVMLst.Add(itmT);

            return itmEmpProcessedVMLst;
        }
        public List<ItemProcessedEmployeeViewModel> GetItemProcessedEmployeeViewModel1(ref int excp, ref string msg, ref AccountModelsLib.operations opp, bool IsHaveValue, long Item_Category_Id, long Emp_Id, DateTime WorkOrdersDateFrom, DateTime WorkOrdersDateTo)
        {
            string mysql = "SELECT     0 As Serial,   CarsWorkOrdersItemProcessedEmployees.*, CarsWorkOrders.*, CarsWorkOrdersItemProcessedDetails.* FROM            CarsWorkOrders INNER JOIN\r\n                         CarsWorkOrdersItemProcessedDetails ON CarsWorkOrders.Id = CarsWorkOrdersItemProcessedDetails.CarsWorkOrdersId LEFT OUTER JOIN\r\n                         CarsWorkOrdersItemProcessedEmployees ON CarsWorkOrdersItemProcessedDetails.Id = CarsWorkOrdersItemProcessedEmployees.CarsWorkOrdersItemProcessedDetailsId";

            mysql = mysql + " Where  CarsWorkOrders.Id  > 0";

            if (Emp_Id > 0)
            {
                mysql = mysql + " And CarsWorkOrdersItemProcessedEmployees.Emp_Id  = " + Emp_Id + "";
            }
            if (IsHaveValue == true)
            {
                mysql = mysql + " And  CarsWorkOrdersItemProcessedEmployees.Value  >0  ";


            }
            if (Item_Category_Id > 0)
            {
                mysql = mysql + " And  CarsWorkOrdersItemProcessedDetails.Item_Category_Id  = " + Item_Category_Id + "";
            }

            mysql = mysql + " AND (CONVERT(DATE, CarsWorkOrders.WorkOrdersDate)  >= '" + AccountModelsLib.operations.dateFormat(WorkOrdersDateFrom.Date) + "') ";

            mysql = mysql + " AND (CONVERT(DATE, CarsWorkOrders.WorkOrdersDate)  <= '" + AccountModelsLib.operations.dateFormat(WorkOrdersDateTo.Date) + "') ";


            mysql = mysql + " ORDER BY CarsWorkOrders.WorkOrderNumber, CarsWorkOrdersItemProcessedDetails.Item_Category_Id, CarsWorkOrdersItemProcessedDetails.Item_Id ";
            decimal TotalValue = 0;
            decimal QntyValue = 0;
            DataTable dt = opp.return_DataTable(mysql);

            List<ItemProcessedEmployeeViewModel> itmEmpProcessedVMLst = new List<ItemProcessedEmployeeViewModel>();
            
            for (int x = 0; x < dt.Rows.Count; x++)
            {
                ItemProcessedEmployeeViewModel itm = new ItemProcessedEmployeeViewModel();


                itm.Serial = x + 1;
                itm.Qnty = decimal.Parse(dt.Rows[x]["Qnty"].ToString());//emp.carsWorkOrdersItemProcessedDetail.Qnty;
                itm.Item_Category_Name = dt.Rows[x]["Item_Category_Name"].ToString();//emp.carsWorkOrdersItemProcessedDetail.Item_Category_Name;
                itm.Item_Category_Id = long.Parse(dt.Rows[x]["Item_Category_Id"].ToString());// emp.carsWorkOrdersItemProcessedDetail.Item_Category_Id;
                itm.Item_Net_Total_Price = decimal.Parse(dt.Rows[x]["Item_Net_Total_Price"].ToString());// emp.carsWorkOrdersItemProcessedDetail.Item_Net_Total_Price ?? 0;
                itm.Qnty_value = decimal.Parse(dt.Rows[x]["Qnty_value"].ToString());//emp.carsWorkOrdersItemProcessedDetail.Qnty_value ?? 0;


                itm.PriceUnit = decimal.Parse(dt.Rows[x]["PriceUnit"].ToString());//emp.carsWorkOrdersItemProcessedDetail.PriceUnit;
                itm.UnitStr = dt.Rows[x]["UnitStr"].ToString();//emp.carsWorkOrdersItemProcessedDetail.UnitStr;

                itm.Specification = dt.Rows[x]["Specification"].ToString();//emp.carsWorkOrdersItemProcessedDetail.Specification;

                itm.Invoice_Item_Name = dt.Rows[x]["Invoice_Item_Name"].ToString() + " " + itm.Specification;//emp.carsWorkOrdersItemProcessedDetail.Invoice_Item_Name + " " + emp.carsWorkOrdersItemProcessedDetail.Specification;


                QntyValue = QntyValue + itm.Qnty_value ?? 0;


                itm.WorkOrderNumber = long.Parse(dt.Rows[x]["WorkOrderNumber"].ToString());//emp.CarsWorkOrder.WorkOrderNumber;
                itm.WorkOrdersDate = DateTime.Parse(dt.Rows[x]["WorkOrdersDate"].ToString());//emp.CarsWorkOrder.WorkOrdersDate;


                itm.CarsWorkOrdersId = long.Parse(dt.Rows[x]["CarsWorkOrdersId"].ToString());// emp.CarsWorkOrdersId;
                itm.CarsWorkOrdersItemProcessedDetailsId = long.Parse(dt.Rows[x]["CarsWorkOrdersItemProcessedDetailsId"].ToString());// emp.CarsWorkOrdersItemProcessedDetailsId;
                itm.CarsWorkOrdersItemReversedProcessedDetailsId = long.Parse(dt.Rows[x]["CarsWorkOrdersItemReversedProcessedDetailsId"].ToString());//emp.CarsWorkOrdersItemReversedProcessedDetailsId;


                itm.CarsId = long.Parse(dt.Rows[x]["CarsId"].ToString());// emp.CarsId;
                itm.Emp_Id = long.Parse(dt.Rows[x]["Emp_Id"].ToString());//  emp.Emp_Id;
                itm.Emp_Name = dt.Rows[x]["Emp_Name"].ToString();// emp.Emp_Name;
                itm.Ratio = decimal.Parse(dt.Rows[x]["Ratio"].ToString());//emp.Ratio ?? 0;
                itm.Value = decimal.Parse(dt.Rows[x]["Value"].ToString());//emp.Value ?? 0;
                itm.Notes = dt.Rows[x]["Notes"].ToString();// emp.Notes;

                TotalValue = TotalValue + itm.Value ?? 0;



                itmEmpProcessedVMLst.Add(itm);
            }

            ItemProcessedEmployeeViewModel itmT = new ItemProcessedEmployeeViewModel();
            itmT.Emp_Name = "الإجمالى";
            itmT.Ratio = 0;
            itmT.Value = TotalValue;
            itmT.Qnty_value = QntyValue;
            itmT.Notes = "";
            itmT.Id = 0;
            itmEmpProcessedVMLst.Add(itmT);

            return itmEmpProcessedVMLst;
        }

        public DataTable GetItemProcessedTotalItemsMasna3iat(ref int Excp, ref string msg, bool IsHaveValue, long Emp_Id, bool _DateFrom, DateTime DateFrom, bool _DateTo, DateTime DateTo)
        {

            AccountModelsLib.operations opp = new AccountModelsLib.operations();

            DataTable dt = new DataTable();
            try
            {
                string mysql = @"  SELECT        0 AS Serial, SUM(ISNULL(CarsWorkOrdersItemProcessedEmployees.[Value], 0)) AS ItemValues, CarsWorkOrdersItemProcessedEmployees.Emp_Id, CarsWorkOrdersItemProcessedEmployees.Emp_Name, 
                         SUM(CarsWorkOrdersItemProcessedDetails.Qnty_value) AS Qnty_value
FROM            CarsWorkOrdersItemProcessedEmployees INNER JOIN
                         CarsWorkOrders ON CarsWorkOrdersItemProcessedEmployees.CarsWorkOrdersId = CarsWorkOrders.Id INNER JOIN
                         CarsWorkOrdersItemProcessedDetails ON CarsWorkOrdersItemProcessedEmployees.CarsWorkOrdersItemProcessedDetailsId = CarsWorkOrdersItemProcessedDetails.Id  ";
                //mysql = mysql + " WHERE (Invoices_List.Invoice_Type = " + Invoice_Type + ") ";
                mysql = mysql + " WHERE (CarsWorkOrdersItemProcessedEmployees.Id > 0  ) AND (CarsWorkOrders.IsWorkOrderClosed = 1)";
                

                if (_DateFrom == true)
                {
                    //mysql = mysql + " AND  (Invoices_List.Invoice_Date >='" + AccountModelsLib.operations.dateFormat(DateFrom) + "')   ";
                    mysql = mysql + " AND (CONVERT(DATE, CarsWorkOrders.WorkOrdersDate)  >= '" + AccountModelsLib.operations.dateFormat(DateFrom.Date) + "') ";

                }
                if (_DateTo == true)
                {
                    //mysql = mysql + "   AND (Invoices_List.Invoice_Date <='" + AccountModelsLib.operations.dateFormat(DateTo) + "') ";
                    mysql = mysql + " AND (CONVERT(DATE, CarsWorkOrders.WorkOrdersDate)  <= '" + AccountModelsLib.operations.dateFormat(DateTo.Date) + "') ";

                }
                mysql = mysql + " AND (CarsWorkOrdersItemProcessedDetails.Item_Category_Id = "+(long)ItemCategoryEnum.Masnaheaiat+") ";
                 
                         

                mysql = mysql + "   GROUP BY CarsWorkOrdersItemProcessedEmployees.Emp_Id, CarsWorkOrdersItemProcessedEmployees.Emp_Name ";

                if (IsHaveValue == true)
                {
                    mysql = mysql + " HAVING         (SUM(ISNULL(CarsWorkOrdersItemProcessedEmployees.[Value], 0)) > 0) ";

                }

                dt = opp.return_DataTable(mysql);

                Excp = 0;
                return dt;
            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }

        }

    }
}
