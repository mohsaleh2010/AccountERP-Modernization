using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
 

namespace AccountModelsLib
{
    public class EmployeeListCLS
    {
        private DataTable dataTable = null;
        operations opp  ;
        public EmployeeRelatedItemsCLS empRelatedItemCLS;
        public Employee emp = new Employee();
        public EmployeeListCLS()
        {
            opp = new operations();
            empRelatedItemCLS = new EmployeeRelatedItemsCLS(opp);
        }
        public EmployeeListCLS(bool AdoDotNet)
        {
            
        }
        public EmployeeListCLS(operations opp_)
        {
            opp = opp_;
            empRelatedItemCLS = new EmployeeRelatedItemsCLS(opp);
        }
       
        public List<EmployeeViewModel> GetEmployeeList(long Emp_Id )
        {
            List<EmployeeViewModel> EmpVM = new List<EmployeeViewModel>();

            //List<Employee> employee = opp.db.Employee.AsNoTracking().Include(j => j.jobsList).ToList();
            IQueryable<Employee> employee = opp.db.Employees.AsNoTracking().Include(j => j.jobsList) ;
            if(Emp_Id>0)
            {
                employee = employee.Where(i=>i.Id== Emp_Id);
            }
            return GetEmpVM(employee);
        }

        public List<EmployeeViewModel> GetEmployeeRelatedItemsList(long Item_Id)
        {
           
            List<EmployeeViewModel> EmpVM = new List<EmployeeViewModel>();

  
              List<EmployeeRelatedItem> EmployeeRelatedItem = empRelatedItemCLS.GetEmployeeRelatedItemsList(Item_Id);

            return GetEmpVM(EmployeeRelatedItem);
        }
        public List<EmployeeViewModel> GetEmployeeByJob(long job_Id)
        {
            List<EmployeeViewModel> EmpVM=new List<EmployeeViewModel>();

            var employee = opp.db.Employees.Where(e=>e.jobsListId==job_Id).Include(j=>j.jobsList);

            return GetEmpVM((IQueryable<Employee>)employee);
        }
        List<EmployeeViewModel> GetEmpVM(IQueryable<Employee> EmpList)
        {
            List<EmployeeViewModel> EmpVMList = new List<EmployeeViewModel>();
            foreach (var emp in EmpList)
            {
                EmployeeViewModel  EmpVM = new  EmployeeViewModel();
                EmpVM.Id = emp.Id;
                EmpVM.name = emp.name;
                EmpVM.jobsListId = emp.jobsListId;
                if (emp.jobsList != null)
                {
                    EmpVM.JobName = emp.jobsList.jobname;
                }
                else
                {
                    EmpVM.JobName = "";

                }
                EmpVMList.Add(EmpVM);
            }

           return  EmpVMList;
        }
        List<EmployeeViewModel> GetEmpVM(List<EmployeeRelatedItem> EmployeeRelatedItem)
        {
            List<EmployeeViewModel> EmpVMList = new List<EmployeeViewModel>();
            foreach (var emp in EmployeeRelatedItem)
            {
                EmployeeViewModel EmpVM = new EmployeeViewModel();
                if (emp.Employee != null)
                {
                    EmpVM.Id = emp.Employee.Id;
                    EmpVM.name = emp.Employee.name;
                }
                if (emp.jobslist != null)
                {
                    EmpVM.JobName = emp.jobslist.jobname;
                }
                else
                {
                    EmpVM.JobName = "";

                }
                EmpVM.EmployeeRelatedItemsId = EmpVM.Id;
                EmpVMList.Add(EmpVM);
            }

            return EmpVMList;
        }
        public DataTable GetData()
        {
           
            string mysql = "Select * from Employee Where Active = 1 ";
            dataTable = opp.return_DataTableWithBuilder(mysql);

            return dataTable;

        }

        public void  UpdateDataTable()
        {
            opp.ada.Update(dataTable);

            
        }
        public void MakeDeleteEmployee(ref operations opp, ref int Excp, ref string Msg, long Id, long SubAccount_Id)
        {
            string mysql = "";
            if (Common.Update_DeleteSupplier == 0)
            {
                mysql = "Update Employee set active=0 Where Id= " + Id;
                mysql = mysql + "Update Employee set active=0 Where SubAccount_Id= " + SubAccount_Id;
            }
            if (Common.Update_DeleteSupplier == 1)
            {
                mysql = "delete from Employee where Id= " + Id;
                mysql = mysql + "delete from Employee where SubAccount_Id= " + SubAccount_Id;
            }
            try
            {
                opp.insert_me(mysql);
                Excp = 0;
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
            }
        }

        public void AddEmployeeRelatedItem(EmployeeRelatedItem empRelatedItem)
        {
            empRelatedItemCLS.Add(empRelatedItem);
        }

        public object GetAll()
        {
            throw new NotImplementedException();
        }

        public  void AddUpdteEmployee(ref operations opp, long SubAccount_Id, string SubAccount_NO, string SubAccount_Name)
        {
           string  mysql = "Select * from Employee Where SubAccount_Id=" + SubAccount_Id;
            DataTable dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count == 0)
            {
                if (Common.Use_AutoNumeric_Employee_Id_With_Submain_Account_Id == 1)
                {
                    mysql = "INSERT INTO Employee (Name, SubAccount_Id,Account_Code) ";
                    mysql = mysql + " VALUES ('" + SubAccount_Name + "', " + SubAccount_Id + ",'" + SubAccount_NO + "')";
                    opp.insert_me(mysql);
                }
                else
                {
                    mysql = "INSERT INTO Employee (Id,Name, SubAccount_Id,Account_Code) ";
                    mysql = mysql + " VALUES (" + SubAccount_Id + ",'" + SubAccount_Name + "', " + SubAccount_Id + ",'" + SubAccount_NO + "')";
                    opp.insert_me(mysql);
                }
            }
            else
            {
                mysql = "Update Employee   Set Name=N'" + SubAccount_Name + "' Where SubAccount_Id=" + SubAccount_Id;
                opp.insert_me(mysql);
            }
        }

        public long AddUpdteEmployee(ref int Excp,ref string Msg, ref operations opp, Submain_Account sb)
        {
            Employee emp  = opp.db.Employees.AsNoTracking().Where(e => e.name == sb.SubAccount_Name).FirstOrDefault();
            if (emp  == null)
            {
                emp = new Employee();
                emp.name = sb.SubAccount_Name;
                emp.SubAccount_Id = sb.Id;
                emp.Account_Code = sb.Account_Code;
                if (Common.Use_AutoNumeric_Employee_Id_With_Submain_Account_Id == 0 | Common.Use_Auto_Identity_Column == 0)
                {
                    emp.Id = sb.Id;
                   
                }
                opp.db.Employees.Add(emp);
               
            }
            else
            {
                
                emp.name = sb.SubAccount_Name;
                emp.SubAccount_Id = sb.Id;
                emp.Account_Code = sb.Account_Code;
                opp.db.Entry(emp).State= EntityState.Modified;

            }
            opp.db.SaveChanges();
            return emp.Id;
        }
        public long  UpdateEmployee(ref int Excp, ref string Msg, ref operations opp, Submain_Account sb)
        {
           
            opp.db.Entry(emp).State = EntityState.Modified;
            opp.db.SaveChanges();
            return emp.Id;
        }
    }
}
