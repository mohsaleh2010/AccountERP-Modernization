using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    [NotMapped]
    public  class CustomerViewModel:Customer
    {
        public int Serial { get; set; }
        //public long Id { get; set; }
        //public string Name { get; set; }

        //public string Code { get; set; }
        //public string Address { get; set; }
        //public string Tel { get; set; }
        //public string Tel2 { get; set; }
        //public string Mobile { get; set; }
        //public string E_Mail { get; set; }
        //public string Fax { get; set; }
        //public string Web_Site { get; set; }
        //public int? Customer_Type { get; set; }
        //public int? Active { get; set; }
        //public long SubAccount_Id { get; set; }
        public string SubAccount_Name { get; set; }
        //public int? IsSalesMan { get; set; }
        //public int? SalesMan_Id { get; set; }
        //public DateTime? Date_Balance_First { get; set; }
        //public decimal? Balance_First { get; set; }
        //public int? Location_Id { get; set; }
        //public long Price_Type_List_ID { get; set; }
        //public string Tax_No { get; set; }


        //public string Account_Code { get; set; }
        //public int? Currency_Id { get; set; }
        //public int? Static_Flage { get; set; }
        //public int? General_Customer_Flage { get; set; }
        //public string Govern_ListName { get; set; }
        //public long? Group_Id { get; set; }
        public string GroupName { get; set; }
        //public long? Ref_Cust_Supplier_Id { get; set; }
        //public string VisaNo { get; set; }
        //public string VisaPassword { get; set; }
        //public string CustomerNotes { get; set; }


        //public DateTime? OperationDate { get; set; }
        //public int? User_Id { get; set; }
        //public int? Branch_Id { get; set; }
        //public DateTime? Insert_ServerDateTime { get; set; }
        //public DateTime? Update_ServerDateTime { get; set; }
        //public DateTime? EditOperationDate { get; set; }
        //public int? Edit_User_Id { get; set; }
        //public string ContactPerson { get; set; }
        //public long? ProcessNo { get; set; }

        //public string CompanyName { get; set; }
        //public string CompanyContactName { get; set; }
        //public string CompanyContactData { get; set; }
        //public string Commercial_Register { get; set; }
        //public string NationalId { get; set; }
        //public string PassportNo { get; set; }
        //public string PostalNo { get; set; }


        //public long? Gov_Country_ListId { get; set; }
        //public long? Govern_Area_ListId { get; set; }
        //public long? Govern_ListId { get; set; }
        //public long? Gov_City_ListId { get; set; }

        public string CityName { get; set; }
        public string GovernName { get; set; }
        public string AreaName { get; set; }
        public string CountryName { get; set; }

    }
}
