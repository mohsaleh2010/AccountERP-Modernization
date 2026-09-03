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
    public class Money_Transaction_Box__
    {
        public Money_Transaction_Box__()
        { }

        #region Full Properties
        public long _Id;
        public long Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        public long _Serial;
        public long Serial
        {
            get { return _Serial; }
            set { _Serial = value; }
        }

        public long _Order_Mony_Id;
        public long Order_Mony_Id
        {
            get { return _Order_Mony_Id; }
            set { _Order_Mony_Id = value; }
        }

        private DateTime _Transaction_Date;
        public DateTime Transaction_Date
        {
            get { return _Transaction_Date; }
            set { _Transaction_Date = value; }
        }

        private int _Transaction_Type = 1;
        public int Transaction_Type
        {
            get { return _Transaction_Type; }
            set { _Transaction_Type = value; }
        }

        private decimal _Import_Value = 0;
        public decimal Import_Value
        {
            get { return _Import_Value; }
            set { _Import_Value = value; }
        }

        private decimal _Export_Value = 0;
        public decimal Export_Value
        {
            get { return _Export_Value; }
            set { _Export_Value = value; }
        }

        private decimal _Transaction_Value;
        public decimal Transaction_Value
        {
            get { return _Transaction_Value; }
            set { _Transaction_Value = value; }
        }

        private string _Transaction_Value_;
        public string Transaction_Value_
        {
            get { return _Transaction_Value_; }
            set { _Transaction_Value_ = value; }
        }

        private decimal _Equivalent_Value = 0;
        public decimal Equivalent_Value
        {
            get { return _Equivalent_Value; }
            set { _Equivalent_Value = value; }
        }

        private long _Bank_Id;
        public long Bank_Id
        {
            get { return _Bank_Id; }
            set { _Bank_Id = value; }
        }

        private decimal _Bank_Balance;
        public decimal Bank_Balance
        {
            get { return _Bank_Balance; }
            set { _Bank_Balance = value; }
        }

        private long _Supplier_Id = 0;
        public long Supplier_Id
        {
            get { return _Supplier_Id; }
            set { _Supplier_Id = value; }
        }

        private long _Customer_Id = 0;
        public long Customer_Id
        {
            get { return _Customer_Id; }
            set { _Customer_Id = value; }
        }

        private long _Account_Id = 0;
        public long Account_Id
        {
            get { return _Account_Id; }
            set { _Account_Id = value; }
        }
        private long _Invoice_Id = 0;
        public long Invoice_Id
        {
            get { return _Invoice_Id; }
            set { _Invoice_Id = value; }
        }
        public long CarsWorkOrdersId { get; set; } = 0;
        private long _Invoice_TypeId = 0;
        public long Invoice_TypeId
        {
            get { return _Invoice_TypeId; }
            set { _Invoice_TypeId = value; }
        }

        private long _SubAccount_Id = 0;
        public long SubAccount_Id
        {
            get { return _SubAccount_Id; }
            set { _SubAccount_Id = value; }
        }

        private string _Notes;
        public string Notes
        {
            get { return _Notes; }
            set { _Notes = value; }
        }

        private decimal _Balance_Before;
        public decimal Balance_Before
        {
            get { return _Balance_Before; }
            set { _Balance_Before = value; }
        }

        private decimal _Balance_After;
        public decimal Balance_After
        {
            get { return _Balance_After; }
            set { _Balance_After = value; }
        }




        private string _Balance_AfterStr;
        public string Balance_AfterStr
        {
            get { return _Balance_AfterStr; }
            set { _Balance_AfterStr = value; }
        }

        private string _Balance_BeforStr;
        public string Balance_BeforStr
        {
            get { return _Balance_BeforStr; }
            set { _Balance_BeforStr = value; }
        }
        private long _BankAccount_Id = 0;
        public long BankAccount_Id
        {
            get { return _BankAccount_Id; }
            set { _BankAccount_Id = value; }
        }

        private int _DestructionSales;
        public int DestructionSales
        {
            get { return _DestructionSales; }
            set { _DestructionSales = value; }
        }

        private decimal _Order_mony_id_transform = 0;
        public decimal Order_mony_id_transform
        {
            get { return _Order_mony_id_transform; }
            set { _Order_mony_id_transform = value; }
        }

        private long _Account_Cost_Center_Tree_Id = 0;
        public long Account_Cost_Center_Tree_Id
        {
            get { return _Account_Cost_Center_Tree_Id; }
            set { _Account_Cost_Center_Tree_Id = value; }
        }

        private int _User_Id = 0;
        public int User_Id
        {
            get { return _User_Id; }
            set { _User_Id = value; }
        }

        private int _Branch_Id = 0;
        public int Branch_Id
        {
            get { return _Branch_Id; }
            set { _Branch_Id = value; }
        }

        private string _Tafkeet_Value;
        public string Tafkeet_Value
        {
            get { return _Tafkeet_Value; }
            set { _Tafkeet_Value = value; }
        }

        private decimal _TotalValueWillPaid;
        public decimal TotalValueWillPaid
        {
            get { return _TotalValueWillPaid; }
            set { _TotalValueWillPaid = value; }
        }

        private decimal _Rest_Value;
        public decimal Rest_Value
        {
            get { return _Rest_Value; }
            set { _Rest_Value = value; }
        }

        private string _Account_Name;
        public string Account_Name
        {
            get { return _Account_Name; }
            set { _Account_Name = value; }
        }

        private string _Account_Code = "";
        public string Account_Code
        {
            get { return _Account_Code; }
            set { _Account_Code = value; }
        }

        private int _Account_Type;
        public int Account_Type
        {
            get { return _Account_Type; }
            set { _Account_Type = value; }
        }

        private string _Cost_Center = "";
        public string Cost_Center
        {
            get { return _Cost_Center; }
            set { _Cost_Center = value; }
        }

        private int _Currency_Id = 1;
        public int Currency_Id
        {
            get { return _Currency_Id; }
            set { _Currency_Id = value; }
        }

        private decimal _Purchase_Ticket_Value = 0;
        public decimal Purchase_Ticket_Value
        {
            get { return _Purchase_Ticket_Value; }
            set { _Purchase_Ticket_Value = value; }
        }

        private decimal _Commission_Value;
        public decimal Commission_Value
        {
            get { return _Commission_Value; }
            set { _Commission_Value = value; }
        }

        private decimal _Factor = 1;
        public decimal Factor
        {
            get { return _Factor; }
            set { _Factor = value; }
        }

        private int _Entry_Currency_ID = 1;
        public int Entry_Currency_ID
        {
            get { return _Entry_Currency_ID; }
            set { _Entry_Currency_ID = value; }
        }

        private string _CostCenter_Name = "";
        public string CostCenter_Name
        {
            get { return _CostCenter_Name; }
            set { _CostCenter_Name = value; }
        }

        private DateTime _OperationDate = DateTime.Now.Date;
        public DateTime OperationDate
        {
            get { return _OperationDate; }
            set { _OperationDate = value; }
        }

        private DateTime _Insert_ServerDateTime = DateTime.Now.Date;
        public DateTime Insert_ServerDateTime
        {
            get { return _Insert_ServerDateTime; }
            set { _Insert_ServerDateTime = value; }
        }

        private DateTime _Update_ServerDateTime = DateTime.Now.Date;
        public DateTime Update_ServerDateTime
        {
            get { return _Update_ServerDateTime; }
            set { _Update_ServerDateTime = value; }
        }

        private DateTime _EditOperationDate = DateTime.Now.Date;
        public DateTime EditOperationDate
        {
            get { return _EditOperationDate; }
            set { _EditOperationDate = value; }
        }

        private int _Edit_User_Id;
        public int Edit_User_Id
        {
            get { return _Edit_User_Id; }
            set { _Edit_User_Id = value; }
        }

        public long Lookup_CashPaymentMethodId { get; set; } = 0;
        public string Lookup_CashPaymentMethodName { get; set; } = "";

        #endregion

        
    }
}
