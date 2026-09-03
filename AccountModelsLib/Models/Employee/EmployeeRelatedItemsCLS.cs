using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;
 
 

namespace AccountModelsLib
{
    public class EmployeeRelatedItemsCLS
    {

        operations opp;
        public EmployeeRelatedItemsCLS()
        {
            opp = new operations();
        }
        public EmployeeRelatedItemsCLS(operations opp_)
        {
            opp = opp_;
        }


        //public List<EmployeeViewModel> GetEmployeeRelatedItemsList(long Item_Id)
        //{

        //    List<EmployeeViewModel> EmpVM = new List<EmployeeViewModel>();


        //    List<EmployeeRelatedItem> EmployeeRelatedItem = empRelatedItemCLS.GetEmployeeRelatedItemsList(Item_Id);

        //    return GetEmpVM(EmployeeRelatedItem);
        //}
        public List<EmployeeRelatedItem> GetEmployeeRelatedItemsList(long Item_Id)
        {
            List<EmployeeViewModel> EmpVM = new List<EmployeeViewModel>();


            //List<Employee> employee = opp.db.Employee.AsNoTracking().Include(e=>e.EmployeeRelatedItem.Where(i=>i.Item_Id== Item_Id)).Include(j => j.jobsList).ToList();
            List<EmployeeRelatedItem> EmployeeRelatedItem = opp.db.EmployeeRelatedItems.AsNoTracking().Where(i => i.Item_Id == Item_Id).Include(e => e.Employee).Include(j => j.jobslist).ToList();

            return EmployeeRelatedItem;
        }
        public List<EmployeeRelatedItemsViewModel> GetEmployeeRelatedItemsVMList(long Item_Id)
        {
            List<EmployeeViewModel> EmpVM = new List<EmployeeViewModel>();


            //List<Employee> employee = opp.db.Employee.AsNoTracking().Include(e=>e.EmployeeRelatedItem.Where(i=>i.Item_Id== Item_Id)).Include(j => j.jobsList).ToList();
            List<EmployeeRelatedItem> EmployeeRelatedItem = opp.db.EmployeeRelatedItems.AsNoTracking().Where(i => i.Item_Id == Item_Id).Include(e => e.Employee).Include(j => j.jobslist).ToList();

            return GetEmpRelatedItemVM(EmployeeRelatedItem);
        }
        public List<EmployeeRelatedItemsViewModel> GetEmployeeRelatedItemsVMList( )
        {
            List<EmployeeViewModel> EmpVM = new List<EmployeeViewModel>();


            //List<Employee> employee = opp.db.Employee.AsNoTracking().Include(e=>e.EmployeeRelatedItem.Where(i=>i.Item_Id== Item_Id)).Include(j => j.jobsList).ToList();
            List<EmployeeRelatedItem> EmployeeRelatedItem = opp.db.EmployeeRelatedItems.AsNoTracking().Include(e => e.Employee).Include(j => j.jobslist).ToList();

            return GetEmpRelatedItemVM(EmployeeRelatedItem);
        }
        List<EmployeeRelatedItemsViewModel> GetEmpRelatedItemVM(List<EmployeeRelatedItem> EmployeeRelatedItem)
        {
            List<EmployeeRelatedItemsViewModel> EmpVMList = new List<EmployeeRelatedItemsViewModel>();
            foreach (var emp in EmployeeRelatedItem)
            {
                EmployeeRelatedItemsViewModel EmpVM = new EmployeeRelatedItemsViewModel();
                if (emp.Employee != null)
                {

                    EmpVM.Emp_Name = emp.Employee.name;
                }
                if (emp.jobslist != null)
                {
                    EmpVM.Job_Name = emp.jobslist.jobname;
                }
                else
                {
                    EmpVM.Job_Name = "";

                }
                if (emp.Item != null)
                {

                    EmpVM.Item_Name = emp.Item.Item_Name;
                }
                EmpVM.Id = emp.Id;
                EmpVM.jobsListId = emp.jobsListId;
                EmpVM.EmployeesId = emp.EmployeesId;
                EmpVM.Item_Id = emp.Item_Id;
                EmpVMList.Add(EmpVM);
            }

            return EmpVMList;
        }
        public void Add(EmployeeRelatedItem empItem)
        {
            opp.db.EmployeeRelatedItems.Add(empItem);
            opp.db.SaveChanges();
        }
        public void Delete(EmployeeRelatedItem empItem)
        {
            opp.db.EmployeeRelatedItems.Remove(empItem);
            opp.db.SaveChanges();
        }
        public void Delete(long EmployeeRelatedItemId)
        {
            EmployeeRelatedItem emp = opp.db.EmployeeRelatedItems.Where(e => e.Id == EmployeeRelatedItemId).FirstOrDefault();
            if (emp != null)
            {
                opp.db.EmployeeRelatedItems.Remove(emp);
                opp.db.SaveChanges();
            }
        }

    }
}
