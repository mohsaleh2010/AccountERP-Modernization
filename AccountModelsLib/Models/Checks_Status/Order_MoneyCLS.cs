using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
namespace AccountModelsLib.Models
{
    public class Order_MoneyCLS
    {
        operations opp;
        public Order_MoneyCLS()
        {
            opp = new operations();
        }
        public Order_MoneyCLS(ref operations opp_)
        {
            opp = opp_;
        }

        public Order_Money om = new Order_Money();
        public List<Order_Money> order_MoneyLSt = new List<Order_Money>();

        /*
        #region Full Properties
        private long _Id;
        public long Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        private long _Serial;
        public long Serial
        {
            get { return _Serial; }
            set { _Serial = value; }
        }

        private long _Order_No;
        public long Order_No
        {
            get { return _Order_No; }
            set { _Order_No = value; }
        }

        private DateTime _Order_Date;
        public DateTime Order_Date
        {
            get { return _Order_Date; }
            set { _Order_Date = value; }
        }

        private int _Transaction_type;
        public int Transaction_type
        {
            get { return _Transaction_type; }
            set { _Transaction_type = value; }
        }

        private decimal _Total_Value;
        public decimal Total_Value
        {
            get { return _Total_Value; }
            set { _Total_Value = value; }
        }

        private int _Currency_Id = 1;
        public long Currency_Id
        {
            get { return _Currency_Id; }
            set { _Currency_Id = value; }
        }

        private int _Bank_Type;
        public int Bank_Type
        {
            get { return _Bank_Type; }
            set { _Bank_Type = value; }
        }

        private int _Bank_ID;
        public int Bank_ID
        {
            get { return _Bank_ID; }
            set { _Bank_ID = value; }
        }

        private string _Man_Name = "";
        public string Man_Name
        {
            get { return _Man_Name; }
            set { _Man_Name = value; }
        }

        private int _Is_Balances_Set = 0;
        public int Is_Balances_Set
        {
            get { return _Is_Balances_Set; }
            set { _Is_Balances_Set = value; }
        }

        private int _Day_Registration_flage = 0;
        public int Day_Registration_flage
        {
            get { return _Day_Registration_flage; }
            set { _Day_Registration_flage = value; }
        }

        private long _DayReg_List_Id = 0;
        public long DayReg_List_Id
        {
            get { return _DayReg_List_Id; }
            set { _DayReg_List_Id = value; }
        }

        private int _Order_mony_id_transform = 0;
        public int Order_mony_id_transform
        {
            get { return _Order_mony_id_transform; }
            set { _Order_mony_id_transform = value; }
        }

        private long _Invoice_Id = 0;
        public long Invoice_Id
        {
            get { return _Invoice_Id; }
            set { _Invoice_Id = value; }
        }
        public long CarsWorkOrdersId { get; set; } = 0;
         
        
        private int _User_Id;
        public long User_Id
        {
            get { return _User_Id; }
            set { _User_Id = value; }
        }

        private int _Branch_Id;
        public long Branch_Id
        {
            get { return _Branch_Id; }
            set { _Branch_Id = value; }
        }

        private int _Year_Financial_Period_Id = 0;
        public long Year_Financial_Period_Id
        {
            get { return _Year_Financial_Period_Id; }
            set { _Year_Financial_Period_Id = value; }
        }

        private int _Ticket_Supplier_Id = 0;
        public int Ticket_Supplier_Id
        {
            get { return _Ticket_Supplier_Id; }
            set { _Ticket_Supplier_Id = value; }
        }

        private string _Order_Tafkeet_Value = "";
        public string Order_Tafkeet_Value
        {
            get { return _Order_Tafkeet_Value; }
            set { _Order_Tafkeet_Value = value; }
        }

        private string _Order_Notes = "";
        public string Order_Notes
        {
            get { return _Order_Notes; }
            set { _Order_Notes = value; }
        }

        private string _Tafkeet_Value = "";
        public string Tafkeet_Value
        {
            get { return _Tafkeet_Value; }
            set { _Tafkeet_Value = value; }
        }

        private long _Sandouk_Cost_Center_Tree_Id = 0;
        public long Sandouk_Cost_Center_Tree_Id
        {
            get { return _Sandouk_Cost_Center_Tree_Id; }
            set { _Sandouk_Cost_Center_Tree_Id = value; }
        }

        private int _IS_Posted_Flage = 0;
        public int IS_Posted_Flage
        {
            get { return _IS_Posted_Flage; }
            set { _IS_Posted_Flage = value; }
        }

        private string _Entry_Currency_Name = "";
        public string Entry_Currency_Name
        {
            get { return _Entry_Currency_Name; }
            set { _Entry_Currency_Name = value; }
        }

        private decimal _Factor = 1;
        public decimal Factor
        {
            get { return _Factor; }
            set { _Factor = value; }
        }

        private int _Entry_Currency_ID = 1;
        public long Entry_Currency_ID
        {
            get { return _Entry_Currency_ID; }
            set { _Entry_Currency_ID = value; }
        }

        private long _Account_Entry_List_ID = 0;
        public long Account_Entry_List_ID
        {
            get { return _Account_Entry_List_ID; }
            set { _Account_Entry_List_ID = value; }
        }

        private int _Invoice_Type = 0;
        public int Invoice_Type
        {
            get { return _Invoice_Type; }
            set { _Invoice_Type = value; }
        }

        private DateTime? _OperationDate = DateTime.Now.Date;
        public DateTime? OperationDate
        {
            get { return _OperationDate; }
            set { _OperationDate = value; }
        }

        private DateTime? _Insert_ServerDateTime = DateTime.Now.Date;
        public DateTime? Insert_ServerDateTime
        {
            get { return _Insert_ServerDateTime; }
            set { _Insert_ServerDateTime = value; }
        }

        private DateTime? _Update_ServerDateTime = DateTime.Now.Date;
        public DateTime? Update_ServerDateTime
        {
            get { return _Update_ServerDateTime; }
            set { _Update_ServerDateTime = value; }
        }

        private DateTime? _EditOperationDate = DateTime.Now.Date;
        public DateTime? EditOperationDate
        {
            get { return _EditOperationDate; }
            set { _EditOperationDate = value; }
        }

        private int _Edit_User_Id = 0;
        public long Edit_User_Id
        {
            get { return _Edit_User_Id; }
            set { _Edit_User_Id = value; }
        }


        #endregion
      */
        public void Get_Order_Money(ref operations opp, long Id)
        {
            string mysql = " Select * from Order_Money Where Id=" + Id;
            DataTable dt = opp.return_DataTable(mysql);
            om = new Order_Money();
            om = opp.db.Order_Money.Where(o => o.Id == Id).FirstOrDefault();

        }
        public Order_Money Get_Order_MoneyById(long Id)
        {

            om = new Order_Money();
            om = opp.db.Order_Money.Where(o => o.Id == Id).FirstOrDefault();
            return om;
        }
        public DataTable Get_Order_MoneyByAccount(long SubAccount_Id)
        {
            DataTable dt = new DataTable();
            try
            {
                string mysql;

                mysql = @" SELECT DISTINCT Order_Money.*, Submain_Account.*
FROM            Submain_Account INNER JOIN
                         Order_Money ON Submain_Account.Id = Order_Money.SubAccount_Id
WHERE         (Submain_Account.Id =  " + SubAccount_Id + ")  ";


                operations opp = new operations();
                dt = opp.return_DataTable(mysql);

                return dt;
            }
            catch (Exception ee)
            {



            }
            return dt;
        }



        public DataTable GetOrder_MoneyByAccount(ref operations opp, long SubAccount_Id)
        {
            string mysql = "SELECT * FROM Order_Money where SubAccount_Id =" + SubAccount_Id;
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }





        public void InsertOrder_Money(ref int Excp, ref string Msg, ref operations opp, Order_Money om)
        {
            try
            {
                om.EditOperationDate = DateTime.Now.Date;
                om.Insert_ServerDateTime = DateTime.Now.Date;
                om.Update_ServerDateTime = DateTime.Now.Date;
                //om.OperationDate = DateTime.Now.Date;
                //om.OperationDate = Common.CheckValidDate(om.OperationDate.Value);

                long Id__ = 0;
                long B_Id__ = 0;

                string mysql = " ";
                if (Common.Use_Auto_Identity_Column == 0)
                {
                    if (om.Id == 0)
                    {
                        om.Id = Id__;
                        //om.Serial = B_Id__;
                        Common.GetTable_Id(ref opp, "Order_Money", "Id", ref Id__, ref B_Id__);

                         

                        om.Id = B_Id__;
                        //opp.insert_me(mysql);
                        opp.db.Order_Money.Add(om);
                        opp.db.SaveChanges();
                    }
                    else
                    {
                        opp.db.Entry(om).State = EntityState.Modified;
                        opp.db.SaveChanges();
                    }
                    this.om = om;
                }
                else
                {

                   // om.Serial = B_Id__;
                    if (om.Id == 0)
                    {
                        opp.db.Order_Money.Add(om);
                        opp.db.SaveChanges();
                    }
                    else
                    {
                        Order_Money orm = opp.db.Order_Money.Where(i => i.Id == om.Id).FirstOrDefault();
                        if (orm != null)
                        {
                            opp.db.Entry(orm).CurrentValues.SetValues(om);
                            //opp.db.Entry(om).State = EntityState.Modified;
                            opp.db.SaveChanges();
                        }
                    }
                    this.om = om;
                    //mysql = "INSERT INTO Order_Money( Serial, Order_No, Order_Date, Transaction_type, Total_Value, Currency_Id, Bank_Type, Bank_ID, Man_Name, Is_Balances_Set, Day_Registration_flage, DayReg_List_Id, Order_mony_id_transform, Invoice_Id, User_Id, Branch_Id, Year_Financial_Period_Id, Ticket_Supplier_Id, Order_Tafkeet_Value, Order_Notes, Tafkeet_Value, Sandouk_Cost_Center_Tree_Id, IS_Posted_Flage, Entry_Currency_Name, Factor, Entry_Currency_ID, Account_Entry_List_ID, Invoice_Type, OperationDate, Insert_ServerDateTime, Update_ServerDateTime, EditOperationDate, Edit_User_Id,CarsWorkOrdersId)";
                    //mysql = mysql + " VALUES(@Serial,@Order_No,@Order_Date,@Transaction_type,@Total_Value,@Currency_Id,@Bank_Type,@Bank_ID,@Man_Name,@Is_Balances_Set,@Day_Registration_flage,@DayReg_List_Id,@Order_mony_id_transform,@Invoice_Id,@User_Id,@Branch_Id,@Year_Financial_Period_Id,@Ticket_Supplier_Id,@Order_Tafkeet_Value,@Order_Notes,@Tafkeet_Value,@Sandouk_Cost_Center_Tree_Id,@IS_Posted_Flage,@Entry_Currency_Name,@Factor,@Entry_Currency_ID,@Account_Entry_List_ID,@Invoice_Type,@OperationDate,@Insert_ServerDateTime,@Update_ServerDateTime,@EditOperationDate,@Edit_User_Id,@CarsWorkOrdersId)";
                    //opp.cmd.Parameters.AddWithValue("@Serial", B_Id__);


                    //SetParameters(ref opp, om);

                    //om.Id = opp.Insert_me_Return_Last_Identify(mysql);

                    Excp = opp.Excp;
                }


                opp.cmd.Parameters.Clear();
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
                opp.cmd.Parameters.Clear();
            }


        }
        void SetParameters(ref operations opp, Order_Money om)
        {
            opp.cmd.Parameters.AddWithValue("@Order_No", om.Order_No);
            opp.cmd.Parameters.AddWithValue("@Order_Date", om.Order_Date);
            opp.cmd.Parameters.AddWithValue("@Transaction_type", om.Transaction_type);
            opp.cmd.Parameters.AddWithValue("@Total_Value", om.Total_Value);
            opp.cmd.Parameters.AddWithValue("@Currency_Id", om.Currency_Id);
            opp.cmd.Parameters.AddWithValue("@Bank_Type", om.Bank_Type);
            opp.cmd.Parameters.AddWithValue("@Bank_ID", om.Bank_Id);
            opp.cmd.Parameters.AddWithValue("@Man_Name", om.Man_Name);
            opp.cmd.Parameters.AddWithValue("@Is_Balances_Set", om.Is_Balances_Set);
            opp.cmd.Parameters.AddWithValue("@Day_Registration_flage", om.Day_Registration_flage);
            opp.cmd.Parameters.AddWithValue("@DayReg_List_Id", om.DayReg_List_Id);
            opp.cmd.Parameters.AddWithValue("@Order_mony_id_transform", om.Order_mony_id_transform);
            opp.cmd.Parameters.AddWithValue("@Invoice_Id", om.Invoice_Id);
            opp.cmd.Parameters.AddWithValue("@CarsWorkOrdersId", om.CarsWorkOrdersId);
            opp.cmd.Parameters.AddWithValue("@User_Id", om.User_Id);
            opp.cmd.Parameters.AddWithValue("@Branch_Id", om.Branch_Id);
            opp.cmd.Parameters.AddWithValue("@Year_Financial_Period_Id", om.Year_Financial_Period_Id);
            opp.cmd.Parameters.AddWithValue("@Ticket_Supplier_Id", om.Ticket_Supplier_Id);
            opp.cmd.Parameters.AddWithValue("@Order_Tafkeet_Value", om.Order_Tafkeet_Value);
            opp.cmd.Parameters.AddWithValue("@Order_Notes", om.Order_Notes);
            opp.cmd.Parameters.AddWithValue("@Tafkeet_Value", om.Tafkeet_Value);
            opp.cmd.Parameters.AddWithValue("@Sandouk_Cost_Center_Tree_Id", om.Sandouk_Cost_Center_Tree_Id);
            opp.cmd.Parameters.AddWithValue("@IS_Posted_Flage", om.IS_Posted_Flage);
            opp.cmd.Parameters.AddWithValue("@Entry_Currency_Name", om.Entry_Currency_Name);
            opp.cmd.Parameters.AddWithValue("@Factor", om.Factor);
            opp.cmd.Parameters.AddWithValue("@Entry_Currency_ID", om.Entry_Currency_ID);
            opp.cmd.Parameters.AddWithValue("@Account_Entry_List_ID", om.Account_Entry_List_ID);
            opp.cmd.Parameters.AddWithValue("@Invoice_Type", om.Invoice_Type);
            opp.cmd.Parameters.AddWithValue("@OperationDate", om.OperationDate);
            opp.cmd.Parameters.AddWithValue("@Insert_ServerDateTime", om.Insert_ServerDateTime);
            opp.cmd.Parameters.AddWithValue("@Update_ServerDateTime", DateTime.Now.Date);
            opp.cmd.Parameters.AddWithValue("@EditOperationDate", om.EditOperationDate);
            opp.cmd.Parameters.AddWithValue("@Edit_User_Id", om.Edit_User_Id);
        }

        public void UpdateOrder_Money(ref int Excp, ref string Msg, Order_Money om)
        {
            operations opp = new operations(1);

            string mysql = " ";



            mysql = "Update Order_Money Set  CarsWorkOrdersId=@CarsWorkOrdersId, Order_No=@Order_No ,Order_Date=@Order_Date ,Transaction_type=@Transaction_type ,Total_Value=@Total_Value ,Currency_Id=@Currency_Id ,Bank_Type=@Bank_Type ,Bank_ID=@Bank_ID ,Man_Name=@Man_Name ,Is_Balances_Set=@Is_Balances_Set ,Day_Registration_flage=@Day_Registration_flage ,DayReg_List_Id=@DayReg_List_Id ,Order_mony_id_transform=@Order_mony_id_transform ,Invoice_Id=@Invoice_Id ,User_Id=@User_Id ,Branch_Id=@Branch_Id ,Year_Financial_Period_Id=@Year_Financial_Period_Id ,Ticket_Supplier_Id=@Ticket_Supplier_Id ,Order_Tafkeet_Value=@Order_Tafkeet_Value ,Order_Notes=@Order_Notes ,Tafkeet_Value=@Tafkeet_Value ,Sandouk_Cost_Center_Tree_Id=@Sandouk_Cost_Center_Tree_Id ,IS_Posted_Flage=@IS_Posted_Flage ,Entry_Currency_Name=@Entry_Currency_Name ,Factor=@Factor ,Entry_Currency_ID=@Entry_Currency_ID ,Account_Entry_List_ID=@Account_Entry_List_ID ,Invoice_Type=@Invoice_Type ,OperationDate=@OperationDate ,Insert_ServerDateTime=@Insert_ServerDateTime ,Update_ServerDateTime=@Update_ServerDateTime ,EditOperationDate=@EditOperationDate ,Edit_User_Id=@Edit_User_Id ";
            mysql = mysql + " Where Id=" + om.Id;
            SetParameters(ref opp, om);
            opp.cmd.Parameters.Clear();

            opp.insert_me(mysql);




        }

        public void DeleteOrder_Money(long Id)
        {
            operations opp = new operations();
            //Set insert query
            string mysql = "Delete From   Order_Money_ImagesStore Where  Id=" + Id;//
            opp.insert_me(mysql);

        }
        public long Make_New_Order_CheckNo(ref int Excp, int Transaction_type, int Bank_Type = 1)
        {
            long Order_No = 1;
            try
            {

                string mysql = " SELECT MAX(Order_No) AS Order_No FROM Order_Money_Check GROUP BY Bank_Type, Transaction_type HAVING (Bank_Type = " + Bank_Type + ") AND (Transaction_type = " + Transaction_type + ")  ";
                operations opp = new operations();
                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    if (dt.Rows[0]["Order_No"] == null)
                    {
                        Order_No = 1;
                    }
                    else
                    {
                        Order_No = long.Parse(dt.Rows[0]["Order_No"].ToString()) + 1;
                    }
                }
                else
                {

                    Order_No = 1;

                }
                opp = null;
                Excp = 0;


            }
            catch (Exception e)
            {
                Excp = 1;
                Order_No = 1;

            }
            return Order_No;
        }
        public bool Found_Check_InOrder(ref int Excp, long Order_Mony_Id, string Check_No, int Bank_id)
        {
            DataSet ds = null;
            try
            {
                string mysql = " SELECT Order_Mony_Id, Check_No FROM Checks_Memory WHERE (Order_Mony_Id = " + Order_Mony_Id + ") AND (Check_No = '" + Check_No + "')  AND (Bank_id = " + Bank_id + ")";
                operations opp = new operations();
                ds = opp.return_dataset(ref Excp, mysql, "Order_Money");
                opp = null;
                Excp = 0;
                if (ds.Tables[0].Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Excp = 1;
                return false;
                throw (e);

            }
        }

        public long Insert_Order_Money(ref int Excp, ref string Msg, ref operations opp, long Order_No, DateTime Order_Date, int Transaction_type, decimal Total_Value, long Currency_Id, int Bank_Type, long Bank_ID, string Man_Name, int Is_Balances_Set, int Day_Registration_flage, long DayReg_List_Id, int Order_mony_id_Transform, long Invoice_Id, long  Ticket_Supplier_Id, string Tafkeet_Value, string Order_Notes, long Sandouk_Cost_Center_Tree_Id, decimal Factor, long    Entry_Currency_ID, string Entry_Currency_Name, int Invoice_Type, int IS_Posted_Flage, long CarsWorkOrdersId, int IsMoreTheOneBoxShow)
        {

            try
            {


                long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(Order_Date);

                om.IsMoreTheOneBoxShow = IsMoreTheOneBoxShow;
                om.Order_No = Order_No;
                om.Transaction_type = Transaction_type;
                om.Order_Date = Order_Date;
                om.Total_Value = Total_Value;
                om.Currency_Id = Currency_Id;
                om.Bank_Type = Bank_Type;
                om.Man_Name = Man_Name;
                om.Bank_Id = Bank_ID;
                om.Order_Notes = Order_Notes;
                om.User_Id = Common.User_Id;
                om.Branch_Id = Common.Branch_Id;
                om.OperationDate = DateTime.Now.Date;
                om.Insert_ServerDateTime = DateTime.Now.Date;
                om.Factor = Factor;
                om.Year_Financial_Period_Id = Year_Financial_Period_Id;


                om.Is_Balances_Set = Is_Balances_Set;
                om.Day_Registration_flage = Day_Registration_flage;
                om.DayReg_List_Id = DayReg_List_Id;
                om.Order_mony_id_transform = Order_mony_id_Transform;
                om.Invoice_Id = Invoice_Id;
                om.Invoice_Type = Invoice_Type;
                om.CarsWorkOrdersId = CarsWorkOrdersId;


                om.Ticket_Supplier_Id = Ticket_Supplier_Id;
                om.Order_Tafkeet_Value = Tafkeet_Value;
                om.Sandouk_Cost_Center_Tree_Id = Sandouk_Cost_Center_Tree_Id;
                om.IS_Posted_Flage = IS_Posted_Flage;
                om.Entry_Currency_ID = Entry_Currency_ID;
                om.Entry_Currency_Name = Entry_Currency_Name;


                InsertOrder_Money(ref Excp, ref Msg, ref opp, om);

                return om.Id;


            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }
        }


    }
}
