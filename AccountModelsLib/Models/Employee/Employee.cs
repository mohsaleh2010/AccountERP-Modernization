using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [Table("Employee")]
    public class Employee
    {
        public long Id { get; set; }
        public string? name { get; set; }
        public DateTime? birth_date { get; set; }
        public string? address { get; set; }
        public long? Department { get; set; }
        public string? National_Card_Id { get; set; }
        public string? Marriage_Status { get; set; }
        public long? Child_NO { get; set; }
        public string? Insurance_NO { get; set; }
        public DateTime? Joining_job_Date { get; set; }
        public long? jobsListId { get; set; }
         
        public decimal? Actual_Salary { get; set; }
        public long? Salary_Type { get; set; }
        public int? Salary_Type_Houre { get; set; }
        public string? Contract_NO { get; set; }
        public string? Work_Node_NO { get; set; }
        public DateTime? Work_Node_Date { get; set; }
        public DateTime? Medical_Checkup_Date { get; set; }
        public string? Medicinal_Card_NO { get; set; }
        public string? Criminal_Status_Card_NO { get; set; }
        public string? Duty_Certificate_NO { get; set; }
        public decimal? Loan_Balance { get; set; }
        public decimal? Installment_Value { get; set; }
        public long? License_Days_Balance { get; set; }
        public long? License_Total_Days { get; set; }
        public long? Changeable { get; set; }
        public long? Alternate_Cost_Of_Living { get; set; }
        public decimal? Surtax_Salary { get; set; }
        public decimal? Friends_Box { get; set; }
        public decimal? Stamp { get; set; }
        public decimal? Hour_Value { get; set; }
        public decimal? Insurance_Salary { get; set; }
        public decimal? Insurance_employee_Rate { get; set; }
        public decimal? Insurance_Institute_Rate { get; set; }
        public string? Military_Status { get; set; }
        public string? Education { get; set; }
        public string? Graduation_year { get; set; }
        public string? Grade { get; set; }
        public string? Education_Class { get; set; }
        public string? Education_Specification { get; set; }
        public string? restraint_CertificateNo { get; set; }
        public string? restrain_Issued_From { get; set; }
        public DateTime? restrain_Issued_Date { get; set; }
        public string? religion { get; set; }
        public string? nationality { get; set; }
        public string? Gender { get; set; }
        public decimal? MatWabara { get; set; }
        public decimal? MatLehaf { get; set; }
        public decimal? MatArdia { get; set; }
        public decimal? Emp_Day_Value { get; set; }
        public int? active { get; set; }
        public decimal? badl_Eat { get; set; }
        public decimal? badl_House { get; set; }
        public decimal? badl_Transport { get; set; }
        public decimal? First_PeriodHour_Value { get; set; }
        public decimal? Second_PeriodHour_Value { get; set; }
        public decimal? First_PeriodHourFrom { get; set; }
        public decimal? First_PeriodMinuteFrom { get; set; }
        public decimal? First_PeriodHourTo { get; set; }
        public decimal? First_PeriodMinuteTo { get; set; }
        public decimal? Second_PeriodHourFrom { get; set; }
        public decimal? Second_PeriodMinuteFrom { get; set; }
        public decimal? Second_PeriodHourTo { get; set; }
        public decimal? Second_PeriodMinuteTo { get; set; }
        public decimal? equivalent_presence { get; set; }
        public long? SubDeptID { get; set; }
        public decimal? Stick_No_price { get; set; }
        public long? SubAccount_Id { get; set; }
        public string? Account_Code { get; set; }
        public long? Branch_Id { get; set; }
        public long? User_Id { get; set; }
        public string? Color_Name { get; set; }
        public string? code { get; set; }
        public string? Tel { get; set; }
        public string? Tel2 { get; set; }
        public string? Mobile { get; set; }
        public string? E_Mail { get; set; }
        public string? Fax { get; set; }
        public string? Web_Site { get; set; }

        public DateTime? OperationDate { get; set; }
        
        public DateTime? Insert_ServerDateTime { get; set; }
        public DateTime? Update_ServerDateTime { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public long? Edit_User_Id { get; set; }
        public long? ProcessNo { get; set; }

        [ForeignKey("jobsListId")]
        public jobsList jobsList { get; set; }

        public ICollection<EmployeeRelatedItem> EmployeeRelatedItem { get; set; }
    }

}
