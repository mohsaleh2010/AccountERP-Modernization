using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace AccountModelsLib.Models 
{
    public class CustomerDebitDetails
    {
        public CustomerDebitDetails()
        {

        }
        public long Id { get; set; }
        public long CustomerDebitListId { get; set; }
        public int CustomerId { get; set; }
        public int ProjectUnitesId { get; set; }
        public int ServicesId { get; set; }
        public decimal UnitSize { get; set; }
        public decimal ServicePrice { get; set; }

        public decimal TotalDebit { get; set; }
        public decimal latePrice { get; set; }

        public decimal NetTotalDebit { get; set; }
        public long SubAccount_Id_Customer { get; set; }
        public long SubAccount_Id_Service { get; set; }
        public long Account_Entry_ListId { get; set; }
        public long PeriodId { get; set; } = 0;

        
        public DataTable Get_CustomerDebitDetailsByList(ref operations opp, long CustomerDebitListId = 0)
        {
            string mysql = " Select * from ConsCustomerDebitDetails  ";
            if (Id > 0)
            {
                mysql = mysql + " Where CustomerDebitListId=" + CustomerDebitListId;
            }
            DataTable dt = opp.return_DataTable(mysql);
            return dt;
        }
        public DataTable Get_CustomerDebitDetailsById(ref operations opp, long Id = 0)
        {
            string mysql = " Select * from ConsCustomerDebitDetails  ";
            if (Id > 0)
            {
                mysql = mysql + " Where Id=" + Id;
            }
            DataTable dt = opp.return_DataTable(mysql);
            return dt;
        }
        public void GetCustomerDebitDetailsId(ref operations opp, long Id)
        {
            DataTable dt = Get_CustomerDebitDetailsById(ref opp, Id);
            ReadData(dt, 0);
        }

        public void ReadData(DataTable dt, int index)
        { 
            Id = long.Parse(dt.Rows[index]["Id"].ToString());
            CustomerDebitListId = long.Parse(dt.Rows[index]["CustomerDebitListId"].ToString());
            CustomerId = int.Parse(dt.Rows[index]["CustomerId"].ToString());
            ProjectUnitesId = int.Parse(dt.Rows[index]["ProjectUnitesId"].ToString());
            ServicesId = int.Parse(dt.Rows[index]["ServicesId"].ToString());

            UnitSize = decimal.Parse(dt.Rows[index]["UnitSize"].ToString());
            ServicePrice = decimal.Parse(dt.Rows[index]["ServicePrice"].ToString());
            TotalDebit = decimal.Parse(dt.Rows[index]["TotalDebit"].ToString());
            latePrice = decimal.Parse(dt.Rows[index]["latePrice"].ToString());
            NetTotalDebit = decimal.Parse(dt.Rows[index]["NetTotalDebit"].ToString());

            SubAccount_Id_Customer = long.Parse(dt.Rows[index]["SubAccount_Id_Customer"].ToString());
            SubAccount_Id_Service = long.Parse(dt.Rows[index]["SubAccount_Id_Service"].ToString());
            Account_Entry_ListId = long.Parse(dt.Rows[index]["Account_Entry_ListId"].ToString());

        }
        public bool Is_DebitSetFound(ref operations opp)
        {
            bool IsDebitSet = false;
            string mysql = @" Select Id from ConsCustomerDebitDetails  Where CustomerId=@CustomerId 
                                  And ProjectUnitesId=@ProjectUnitesId And  ServicesId=@ServicesId 
                                   AND PeriodId=@PeriodId ";



            opp.cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
            opp.cmd.Parameters.AddWithValue("@ProjectUnitesId", ProjectUnitesId);
            opp.cmd.Parameters.AddWithValue("@ServicesId", ServicesId);
            opp.cmd.Parameters.AddWithValue("@PeriodId", PeriodId);

            DataTable dt = opp.return_DataTable(mysql);
            opp.cmd.Parameters.Clear();
            if (dt.Rows.Count > 0)
            {
                IsDebitSet = true;
            }
            return IsDebitSet; 
        }
        public void InsertCustomerDebitDetails(ref operations opp)
        {

            string mysql = @" INSERT INTO ConsCustomerDebitDetails(CustomerDebitListId, CustomerId, ProjectUnitesId, ServicesId, UnitSize, ServicePrice, TotalDebit, latePrice, NetTotalDebit, SubAccount_Id_Customer, SubAccount_Id_Service, Account_Entry_ListId,PeriodId) ";
            mysql = mysql + " VALUES (@CustomerDebitListId,@CustomerId,@ProjectUnitesId,@ServicesId,@UnitSize,@ServicePrice,@TotalDebit,@latePrice,@NetTotalDebit,@SubAccount_Id_Customer,@SubAccount_Id_Service,@Account_Entry_ListId,@PeriodId)";
 
            opp.cmd.Parameters.AddWithValue("@CustomerDebitListId", CustomerDebitListId);
            opp.cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
            opp.cmd.Parameters.AddWithValue("@ProjectUnitesId", ProjectUnitesId);
            opp.cmd.Parameters.AddWithValue("@ServicesId", ServicesId);
            opp.cmd.Parameters.AddWithValue("@UnitSize", UnitSize);
            opp.cmd.Parameters.AddWithValue("@ServicePrice", ServicePrice);
            opp.cmd.Parameters.AddWithValue("@TotalDebit", TotalDebit);
            opp.cmd.Parameters.AddWithValue("@latePrice", latePrice);
            opp.cmd.Parameters.AddWithValue("@NetTotalDebit", NetTotalDebit);
            opp.cmd.Parameters.AddWithValue("@SubAccount_Id_Customer", SubAccount_Id_Customer);
            opp.cmd.Parameters.AddWithValue("@SubAccount_Id_Service", SubAccount_Id_Service);
            opp.cmd.Parameters.AddWithValue("@Account_Entry_ListId", Account_Entry_ListId);
            opp.cmd.Parameters.AddWithValue("@PeriodId", PeriodId);
            
          Id =  opp.Insert_me_Return_Last_Identify(mysql);
            opp.cmd.Parameters.Clear();
        }

        public void UpdateCustomerDebitDetails(ref operations opp)
        {

            string mysql = @" Update ConsCustomerDebitDetails  Set CustomerDebitListId=@CustomerDebitListId , CustomerId=@CustomerId , ProjectUnitesId=@ProjectUnitesId , ServicesId=@ServicesId , UnitSize=@UnitSize , ServicePrice=@ServicePrice , TotalDebit=@TotalDebit , latePrice=@latePrice , NetTotalDebit=@NetTotalDebit , SubAccount_Id_Customer=@SubAccount_Id_Customer , SubAccount_Id_Service=@SubAccount_Id_Service , Account_Entry_ListId=@Account_Entry_ListId,PeriodId=@PeriodId  ";
            
            mysql = mysql + "  Where Id=@Id";

            opp.cmd.Parameters.AddWithValue("@Id", Id);
            opp.cmd.Parameters.AddWithValue("@@CustomerDebitListId", @CustomerDebitListId);
            opp.cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
            opp.cmd.Parameters.AddWithValue("@ProjectUnitesId", ProjectUnitesId);
            opp.cmd.Parameters.AddWithValue("@ServicesId", ServicesId);
            opp.cmd.Parameters.AddWithValue("@UnitSize", UnitSize);
            opp.cmd.Parameters.AddWithValue("@ServicePrice", ServicePrice);
            opp.cmd.Parameters.AddWithValue("@TotalDebit", TotalDebit);
            opp.cmd.Parameters.AddWithValue("@latePrice", latePrice);
            opp.cmd.Parameters.AddWithValue("@NetTotalDebit", NetTotalDebit);
            opp.cmd.Parameters.AddWithValue("@SubAccount_Id_Customer", SubAccount_Id_Customer);
            opp.cmd.Parameters.AddWithValue("@SubAccount_Id_Service", SubAccount_Id_Service);
            opp.cmd.Parameters.AddWithValue("@Account_Entry_ListId", Account_Entry_ListId);
            opp.cmd.Parameters.AddWithValue("@PeriodId", PeriodId);
            opp.insert_me(mysql);
            opp.cmd.Parameters.Clear();
        }
        public void DeleteCustomerDebitDetails(ref operations opp)
        {

            string mysql = @"Delete From ConsCustomerDebitDetails   ";
            mysql = mysql + " Where Id=" + Id;
            opp.insert_me(mysql);

        }
    }
}
