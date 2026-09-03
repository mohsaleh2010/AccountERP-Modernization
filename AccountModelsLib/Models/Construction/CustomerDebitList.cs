using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace AccountModelsLib.Models 
{
    public class CustomerDebitList
    {
        public CustomerDebitList()
        {

        }
        public long Id { get; set; }
        public long PeriodId { get; set; } = 0;
        public DateTime DebitDate { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public DateTime DueDate { get; set; }

        public long ServicesGroupListId { get; set; } = 0;
        public long ServicesId { get; set; } = 0;
        
        public long ProjectSpacesGroupId { get; set; }
        public long ProjectId { get; set; }
        public decimal ListDebitTotal { get; set; } = 0;
        public decimal ListNetDebitTotal { get; set; } = 0;
        public int PeriodServiceTypeId { get; set; } = 1;

        public DataTable Get_CustomerDebitList(ref operations opp, long Id = 0)
        {
            string mysql = " Select * from ConsCustomerDebitList  ";
            if (Id > 0)
            {
                mysql = mysql + " Where Id=" + Id;
            }
            DataTable dt = opp.return_DataTable(mysql);
            return dt;
        }
        public void GetCustomerDebitListId(ref operations opp, long Id)
        {
            DataTable dt = Get_CustomerDebitList(ref opp, Id);
            ReadData(dt, 0);
        }

        public void ReadData(DataTable dt, int index)
        {

            Id = long.Parse(dt.Rows[index]["Id"].ToString());
            DebitDate = DateTime.Parse(dt.Rows[index]["DebitDate"].ToString());
            DateFrom = DateTime.Parse(dt.Rows[index]["DateFrom"].ToString());
            DateTo = DateTime.Parse(dt.Rows[index]["DateTo"].ToString());
            DueDate = DateTime.Parse(dt.Rows[index]["DueDate"].ToString());

            ServicesGroupListId = int.Parse(dt.Rows[index]["ServicesGroupListId"].ToString());
            ProjectSpacesGroupId = int.Parse(dt.Rows[index]["ProjectSpacesGroupId"].ToString());
            ProjectId = int.Parse(dt.Rows[index]["ProjectId"].ToString());

            ListDebitTotal = decimal.Parse(dt.Rows[index]["ListDebitTotal"].ToString());
            ListNetDebitTotal = decimal.Parse(dt.Rows[index]["ListNetDebitTotal"].ToString());
            PeriodServiceTypeId= int.Parse(dt.Rows[index]["PeriodServiceTypeId"].ToString());
        }

        public void InsertCustomerDebitList(ref operations opp)
        {

            string mysql = @"INSERT INTO ConsCustomerDebitList(DebitDate, DateFrom, DateTo, DueDate, ServicesGroupListId,ServicesId, ProjectSpacesGroupId, ProjectId, ListDebitTotal,ListNetDebitTotal,PeriodServiceTypeId,PeriodId) ";
                                    mysql = mysql + " VALUES (@DebitDate,@DateFrom,@DateTo,@DueDate,@ServicesGroupListId,@ServicesId,@ProjectSpacesGroupId,@ProjectId,@ListDebitTotal,@ListNetDebitTotal,@PeriodServiceTypeId,@PeriodId)";
            
            opp.cmd.Parameters.AddWithValue("@DebitDate", DebitDate);
            opp.cmd.Parameters.AddWithValue("@DateFrom", DateFrom);
            opp.cmd.Parameters.AddWithValue("@DateTo", DateTo);
            opp.cmd.Parameters.AddWithValue("@DueDate", DueDate);
            opp.cmd.Parameters.AddWithValue("@ServicesGroupListId", ServicesGroupListId);
            opp.cmd.Parameters.AddWithValue("@ServicesId", ServicesId);

            
            opp.cmd.Parameters.AddWithValue("@ProjectSpacesGroupId", ProjectSpacesGroupId);
            opp.cmd.Parameters.AddWithValue("@ProjectId", ProjectId);
            opp.cmd.Parameters.AddWithValue("@ListDebitTotal", ListDebitTotal);
            opp.cmd.Parameters.AddWithValue("@ListNetDebitTotal", ListNetDebitTotal);
            opp.cmd.Parameters.AddWithValue("@PeriodServiceTypeId", PeriodServiceTypeId);
            opp.cmd.Parameters.AddWithValue("@PeriodId", PeriodId);
            
            Id = opp.Insert_me_Return_Last_Identify(mysql);
            opp.cmd.Parameters.Clear();
        }

        public void UpdateCustomerDebitList(ref operations opp)
        {

            string mysql = @" Update ConsCustomerDebitList  Set DebitDate=@DebitDate , DateFrom=@DateFrom , DateTo=@DateTo , DueDate=@DueDate , ServicesGroupListId=@ServicesGroupListId , ProjectSpacesGroupId=@ProjectSpacesGroupId , ProjectId=@ProjectId , ListDebitTotal=@ListDebitTotal ,ListNetDebitTotal=@ListNetDebitTotal,PeriodServiceTypeId=@PeriodServiceTypeId, PeriodId=@PeriodId";
            mysql = mysql + "  Where Id=@Id";

            opp.cmd.Parameters.AddWithValue("@Id", Id);
            opp.cmd.Parameters.AddWithValue("@DebitDate", DebitDate);
            opp.cmd.Parameters.AddWithValue("@DateFrom", DateFrom);
            opp.cmd.Parameters.AddWithValue("@DateTo", DateTo);
            opp.cmd.Parameters.AddWithValue("@DueDate", DueDate);
            opp.cmd.Parameters.AddWithValue("@ServicesGroupListId", ServicesGroupListId);
            opp.cmd.Parameters.AddWithValue("@ProjectSpacesGroupId", ProjectSpacesGroupId);
            opp.cmd.Parameters.AddWithValue("@ProjectId", ProjectId);
            opp.cmd.Parameters.AddWithValue("@ListDebitTotal", ListDebitTotal);
            opp.cmd.Parameters.AddWithValue("@ListNetDebitTotal", ListNetDebitTotal);
            opp.cmd.Parameters.AddWithValue("@PeriodServiceTypeId", PeriodServiceTypeId);
            opp.cmd.Parameters.AddWithValue("@PeriodId", PeriodId);
            opp.insert_me(mysql);
            opp.cmd.Parameters.Clear();
        }

        public void UpdateCustomerDebitListDebitTotal(ref operations opp)
        {

            string mysql = @" Update ConsCustomerDebitList  Set   ListDebitTotal=@ListDebitTotal ,ListNetDebitTotal=@ListNetDebitTotal ";
            mysql = mysql + "  Where Id=@Id";

            opp.cmd.Parameters.AddWithValue("@Id", Id);
             
            opp.cmd.Parameters.AddWithValue("@ListDebitTotal", ListDebitTotal);
            opp.cmd.Parameters.AddWithValue("@ListNetDebitTotal", ListNetDebitTotal);
          
            opp.insert_me(mysql);
            opp.cmd.Parameters.Clear();
        }
        public void DeleteCustomerDebitList(ref operations opp)
        {

            string mysql = @"Delete From ConsCustomerDebitList   ";
            mysql = mysql + " Where Id=" + Id;
            opp.insert_me(mysql);

        }


    }
}
