using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
   
using System.IO;
namespace AccountModelsLib.Models
{
    public class Order_Money_Check
    {
        public Order_Money_Check()
        { }

        #region Full Properties
        private long _Id;
        public long Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        //private long _Serial;
        //public long Serial
        //{
        //    get { return _Serial; }
        //    set { _Serial = value; }
        //}

        private long _Order_No;
        public long Order_No
        {
            get { return _Order_No; }
            set { _Order_No = value; }
        }

        private DateTime? _Order_Date;
        public DateTime? Order_Date
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

        private long? _Currency_Id = 1;
        public long? Currency_Id
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

        private long _Bank_ID;
        public long Bank_ID
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

        private long _User_Id;
        public long User_Id
        {
            get { return _User_Id; }
            set { _User_Id = value; }
        }

        private long _Branch_Id;
        public long Branch_Id
        {
            get { return _Branch_Id; }
            set { _Branch_Id = value; }
        }

        private long _Year_Financial_Period_Id = 0;
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

        private long _Entry_Currency_ID = 1;
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

        private long _Edit_User_Id = 0;
        public long Edit_User_Id
        {
            get { return _Edit_User_Id; }
            set { _Edit_User_Id = value; }
        }


        #endregion

        
        public void GetOrder_Money_Check(ref operations opp, long Id)
        {
            string mysql = " Select * from Order_Money_Check Where Id=" + Id;
            DataTable dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count > 0)
            {
                ReadData(dt, 0);
            }
        }
        public void GetOrder_Money_Check(long Id)
        {
            operations opp = new operations();
            string mysql = " Select * from Order_Money_Check Where Id=" + Id;
            DataTable dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count > 0)
            {
                ReadData(dt, 0);
            }
        }
        public DataTable GetOrder_Money_CheckByAccount(long SubAccount_Id)
        {
            DataTable dt = new DataTable();
            try
            {
                string mysql;

                mysql = @" SELECT DISTINCT Order_Money_Check.*, Submain_Account.*
FROM            Submain_Account INNER JOIN
                         Order_Money_Check ON Submain_Account.Id = Order_Money_Check.SubAccount_Id
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



        public DataTable GetOrder_Money_CheckByAccount(ref operations opp, long SubAccount_Id)
        {
            string mysql = "SELECT * FROM Order_Money_Check where SubAccount_Id =" + SubAccount_Id;
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public void Get_Order_Money_CheckByAccount(ref operations opp, long SubAccount_Id)
        {
            string mysql = "SELECT * FROM Order_Money_Check where SubAccount_Id =" + SubAccount_Id;
            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                ReadData(dt, 0);
            }

        }
        void ReadData(DataTable dt, int index)
        {
            Id=Common.GetValueLong(dt, index,"Id");
           // Serial=Common.GetValueLong(dt, index,"Serial");
            Order_No= Common.GetValueLong(dt, index,"Order_No");
            Order_Date= Common.GetValueNullDateTime(dt, index,"Order_Date");
            Transaction_type= Common.GetValueInt(dt, index,"Transaction_type");
            Total_Value= Common.GetValueDecimal(dt, index,"Total_Value");
            Currency_Id= Common.GetValueInt(dt, index,"Currency_Id");
            Bank_Type= Common.GetValueInt(dt, index,"Bank_Type");
            Bank_ID= Common.GetValueInt(dt, index,"Bank_ID");
            Man_Name= Common.GetValueString(dt, index,"Man_Name");
            Is_Balances_Set= Common.GetValueInt(dt, index,"Is_Balances_Set");
            Day_Registration_flage= Common.GetValueInt(dt, index,"Day_Registration_flage");
            DayReg_List_Id= Common.GetValueLong(dt, index,"DayReg_List_Id");
            Order_mony_id_transform= Common.GetValueInt(dt, index,"Order_mony_id_transform");
            Invoice_Id= Common.GetValueLong(dt, index,"Invoice_Id");
            User_Id=Common.GetValueInt(dt, index,"User_Id");
            Branch_Id= Common.GetValueInt(dt, index,"Branch_Id");
            Year_Financial_Period_Id=Common.GetValueInt(dt, index,"Year_Financial_Period_Id");
            Ticket_Supplier_Id= Common.GetValueInt(dt, index,"Ticket_Supplier_Id");
            Order_Tafkeet_Value= Common.GetValueString(dt, index,"Order_Tafkeet_Value");
            Order_Notes=Common.GetValueString(dt, index,"Order_Notes");
            Tafkeet_Value=Common.GetValueString(dt, index,"Tafkeet_Value");
            Sandouk_Cost_Center_Tree_Id=Common.GetValueLong(dt, index,"Sandouk_Cost_Center_Tree_Id");
            IS_Posted_Flage= Common.GetValueInt(dt, index,"IS_Posted_Flage");
            Entry_Currency_Name=Common.GetValueString(dt, index,"Entry_Currency_Name");
            Factor=Common.GetValueDecimal(dt, index,"Factor");
            Entry_Currency_ID= Common.GetValueInt(dt, index,"Entry_Currency_ID");
            Account_Entry_List_ID= Common.GetValueLong(dt, index,"Account_Entry_List_ID");
            Invoice_Type= Common.GetValueInt(dt, index,"Invoice_Type");
            OperationDate= Common.GetValueNullDateTime(dt, index,"OperationDate");
            Insert_ServerDateTime=Common.GetValueNullDateTime(dt, index,"Insert_ServerDateTime");
            Update_ServerDateTime= Common.GetValueNullDateTime(dt, index,"Update_ServerDateTime");
            EditOperationDate=Common.GetValueNullDateTime(dt, index,"EditOperationDate");
            Edit_User_Id= Common.GetValueInt(dt, index,"Edit_User_Id");
        }



        public void InsertOrder_Money_Check(ref int Excp, ref string Msg, ref operations opp)
        {
            try
            {

                long Id__ = 0;
                long B_Id__ = 0;

                string mysql = " ";
                if (Common.Use_Auto_Identity_Column == 0)
                {


                    Common.GetTable_Id(ref opp, "Order_Money_Check", "Id", ref Id__, ref B_Id__);
                    Id__ = CallSnowflakeIdGenerate.GenerateNewId();
                    mysql = "INSERT INTO Order_Money_Check(Id, Order_No, Order_Date, Transaction_type, Total_Value, Currency_Id, Bank_Type, Bank_ID, Man_Name, Is_Balances_Set, Day_Registration_flage, DayReg_List_Id, Order_mony_id_transform, Invoice_Id, User_Id, Branch_Id, Year_Financial_Period_Id, Ticket_Supplier_Id, Order_Tafkeet_Value, Order_Notes, Tafkeet_Value, Sandouk_Cost_Center_Tree_Id, IS_Posted_Flage, Entry_Currency_Name, Factor, Entry_Currency_ID, Account_Entry_List_ID, Invoice_Type, OperationDate, Insert_ServerDateTime, Update_ServerDateTime, EditOperationDate, Edit_User_Id)";
                    mysql = mysql + " VALUES(@Id,@Order_No,@Order_Date,@Transaction_type,@Total_Value,@Currency_Id,@Bank_Type,@Bank_ID,@Man_Name,@Is_Balances_Set,@Day_Registration_flage,@DayReg_List_Id,@Order_mony_id_transform,@Invoice_Id,@User_Id,@Branch_Id,@Year_Financial_Period_Id,@Ticket_Supplier_Id,@Order_Tafkeet_Value,@Order_Notes,@Tafkeet_Value,@Sandouk_Cost_Center_Tree_Id,@IS_Posted_Flage,@Entry_Currency_Name,@Factor,@Entry_Currency_ID,@Account_Entry_List_ID,@Invoice_Type,@OperationDate,@Insert_ServerDateTime,@Update_ServerDateTime,@EditOperationDate,@Edit_User_Id)";

                    opp.cmd.Parameters.AddWithValue("@Id", Id__);
                    //opp.cmd.Parameters.AddWithValue("@Serial", B_Id__);

                    SetParameters(ref opp);
                    _Id = B_Id__;
                    opp.insert_me(mysql);
                }
                else
                {
                    mysql = "INSERT INTO Order_Money_Check(  Order_No, Order_Date, Transaction_type, Total_Value, Currency_Id, Bank_Type, Bank_ID, Man_Name, Is_Balances_Set, Day_Registration_flage, DayReg_List_Id, Order_mony_id_transform, Invoice_Id, User_Id, Branch_Id, Year_Financial_Period_Id, Ticket_Supplier_Id, Order_Tafkeet_Value, Order_Notes, Tafkeet_Value, Sandouk_Cost_Center_Tree_Id, IS_Posted_Flage, Entry_Currency_Name, Factor, Entry_Currency_ID, Account_Entry_List_ID, Invoice_Type, OperationDate, Insert_ServerDateTime, Update_ServerDateTime, EditOperationDate, Edit_User_Id)";
                    mysql = mysql + " VALUES(@Order_No,@Order_Date,@Transaction_type,@Total_Value,@Currency_Id,@Bank_Type,@Bank_ID,@Man_Name,@Is_Balances_Set,@Day_Registration_flage,@DayReg_List_Id,@Order_mony_id_transform,@Invoice_Id,@User_Id,@Branch_Id,@Year_Financial_Period_Id,@Ticket_Supplier_Id,@Order_Tafkeet_Value,@Order_Notes,@Tafkeet_Value,@Sandouk_Cost_Center_Tree_Id,@IS_Posted_Flage,@Entry_Currency_Name,@Factor,@Entry_Currency_ID,@Account_Entry_List_ID,@Invoice_Type,@OperationDate,@Insert_ServerDateTime,@Update_ServerDateTime,@EditOperationDate,@Edit_User_Id)";
                    //opp.cmd.Parameters.AddWithValue("@Serial", B_Id__);


                    SetParameters(ref opp);

                    _Id = opp.Insert_me_Return_Last_Identify(mysql);
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

        public void Update_Order_Money_Check(ref int Excp, ref string Msg, ref operations opp)
        {
            try
            {
                string mysql = "";

                mysql = mysql + " Update  Order_Money_Check Set  Order_No=@Order_No   , Order_Date= @Order_Date   , Transaction_type= @Transaction_type   , Total_Value= @Total_Value   , Currency_Id= @Currency_Id   , Bank_Type= @Bank_Type   , Bank_ID= @Bank_ID   , Man_Name= @Man_Name   , Is_Balances_Set= @Is_Balances_Set   , Day_Registration_flage= @Day_Registration_flage   ,DayReg_List_Id = @DayReg_List_Id   , Order_mony_id_transform= @Order_mony_id_transform   , Invoice_Id= @Invoice_Id   , User_Id= @User_Id   , Branch_Id= @Branch_Id   , Year_Financial_Period_Id= @Year_Financial_Period_Id   ,Ticket_Supplier_Id = @Ticket_Supplier_Id   , Order_Tafkeet_Value= @Order_Tafkeet_Value   ,Order_Notes = @Order_Notes   , Tafkeet_Value= @Tafkeet_Value   , Sandouk_Cost_Center_Tree_Id= @Sandouk_Cost_Center_Tree_Id   , IS_Posted_Flage= @IS_Posted_Flage   , Entry_Currency_Name= @Entry_Currency_Name   , Factor= @Factor   , Entry_Currency_ID= @Entry_Currency_ID   , Account_Entry_List_ID= @Account_Entry_List_ID   , Invoice_Type= @Invoice_Type   , OperationDate= @OperationDate   ,Insert_ServerDateTime = @Insert_ServerDateTime   , Update_ServerDateTime= @Update_ServerDateTime   ,EditOperationDate = @EditOperationDate   , Edit_User_Id= @Edit_User_Id ";
                mysql = mysql + " Where Id=@Id";
                opp.cmd.Parameters.AddWithValue("@Id", Id);

                SetParameters(ref opp);

                opp.insert_me(mysql);
                opp.cmd.Parameters.Clear();
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
                opp.cmd.Parameters.Clear();
            }


        }

        void SetParameters(ref operations opp)
        {
            
            opp.cmd.Parameters.AddWithValue("@Order_No", Order_No);
            opp.cmd.Parameters.AddWithValue("@Order_Date", Order_Date.Value.Date);
            opp.cmd.Parameters.AddWithValue("@Transaction_type", Transaction_type);
            opp.cmd.Parameters.AddWithValue("@Total_Value", Total_Value);
            opp.cmd.Parameters.AddWithValue("@Currency_Id", Currency_Id);
            opp.cmd.Parameters.AddWithValue("@Bank_Type", Bank_Type);
            opp.cmd.Parameters.AddWithValue("@Bank_ID", Bank_ID);
            opp.cmd.Parameters.AddWithValue("@Man_Name", Man_Name);
            opp.cmd.Parameters.AddWithValue("@Is_Balances_Set", Is_Balances_Set);
            opp.cmd.Parameters.AddWithValue("@Day_Registration_flage", Day_Registration_flage);
            opp.cmd.Parameters.AddWithValue("@DayReg_List_Id", DayReg_List_Id);
            opp.cmd.Parameters.AddWithValue("@Order_mony_id_transform", Order_mony_id_transform);
            opp.cmd.Parameters.AddWithValue("@Invoice_Id", Invoice_Id);
            opp.cmd.Parameters.AddWithValue("@User_Id", User_Id);
            opp.cmd.Parameters.AddWithValue("@Branch_Id", Branch_Id);
            opp.cmd.Parameters.AddWithValue("@Year_Financial_Period_Id", Year_Financial_Period_Id);
            opp.cmd.Parameters.AddWithValue("@Ticket_Supplier_Id", Ticket_Supplier_Id);
            opp.cmd.Parameters.AddWithValue("@Order_Tafkeet_Value", Order_Tafkeet_Value);
            opp.cmd.Parameters.AddWithValue("@Order_Notes", Order_Notes);
            opp.cmd.Parameters.AddWithValue("@Tafkeet_Value", Tafkeet_Value);
            opp.cmd.Parameters.AddWithValue("@Sandouk_Cost_Center_Tree_Id", Sandouk_Cost_Center_Tree_Id);
            opp.cmd.Parameters.AddWithValue("@IS_Posted_Flage", IS_Posted_Flage);
            opp.cmd.Parameters.AddWithValue("@Entry_Currency_Name", Entry_Currency_Name);
            opp.cmd.Parameters.AddWithValue("@Factor", Factor);
            opp.cmd.Parameters.AddWithValue("@Entry_Currency_ID", Entry_Currency_ID);
            opp.cmd.Parameters.AddWithValue("@Account_Entry_List_ID", Account_Entry_List_ID);
            opp.cmd.Parameters.AddWithValue("@Invoice_Type", Invoice_Type);
            opp.cmd.Parameters.AddWithValue("@OperationDate", ((object)OperationDate ?? DBNull.Value));
            opp.cmd.Parameters.AddWithValue("@Insert_ServerDateTime", ((object)Insert_ServerDateTime ?? DBNull.Value));
            opp.cmd.Parameters.AddWithValue("@Update_ServerDateTime", ((object)Update_ServerDateTime ?? DBNull.Value));
            opp.cmd.Parameters.AddWithValue("@EditOperationDate", ((object)EditOperationDate ?? DBNull.Value));
            opp.cmd.Parameters.AddWithValue("@Edit_User_Id", Edit_User_Id);
        }

        public void UpdateOrder_Money_Check(ref int Excp, ref string Msg)
        {
            operations opp = new operations(1);

            string mysql = " ";



            mysql = "Update Order_Money_Check Set   Order_No=@Order_No ,Order_Date=@Order_Date ,Transaction_type=@Transaction_type ,Total_Value=@Total_Value ,Currency_Id=@Currency_Id ,Bank_Type=@Bank_Type ,Bank_ID=@Bank_ID ,Man_Name=@Man_Name ,Is_Balances_Set=@Is_Balances_Set ,Day_Registration_flage=@Day_Registration_flage ,DayReg_List_Id=@DayReg_List_Id ,Order_mony_id_transform=@Order_mony_id_transform ,Invoice_Id=@Invoice_Id ,User_Id=@User_Id ,Branch_Id=@Branch_Id ,Year_Financial_Period_Id=@Year_Financial_Period_Id ,Ticket_Supplier_Id=@Ticket_Supplier_Id ,Order_Tafkeet_Value=@Order_Tafkeet_Value ,Order_Notes=@Order_Notes ,Tafkeet_Value=@Tafkeet_Value ,Sandouk_Cost_Center_Tree_Id=@Sandouk_Cost_Center_Tree_Id ,IS_Posted_Flage=@IS_Posted_Flage ,Entry_Currency_Name=@Entry_Currency_Name ,Factor=@Factor ,Entry_Currency_ID=@Entry_Currency_ID ,Account_Entry_List_ID=@Account_Entry_List_ID ,Invoice_Type=@Invoice_Type ,OperationDate=@OperationDate ,Insert_ServerDateTime=@Insert_ServerDateTime ,Update_ServerDateTime=@Update_ServerDateTime ,EditOperationDate=@EditOperationDate ,Edit_User_Id=@Edit_User_Id ";
            mysql = mysql + " Where Id=" + Id;
            opp.cmd.Parameters.AddWithValue("@Id", Id);

            SetParameters(ref opp);
            opp.cmd.Parameters.Clear();

            opp.insert_me(mysql);




        }

        public void DeleteOrder_Money_Check(ref int Excp, ref string Msg, ref operations opp, long Id)
        {
            //operations opp = new operations();
            //Set insert query
            string mysql = "Delete From   Order_Money_Check  Where  Id=" + Id;//
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
                string mysql = " SELECT Order_Mony_Id, Check_No FROM Checks_Memory WHERE   (Check_No = '" + Check_No + "')  AND (Bank_id = " + Bank_id + ")";
                operations opp = new operations();
                ds = opp.return_dataset(ref Excp, mysql, "Order_Money_Check");
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



    }
}
