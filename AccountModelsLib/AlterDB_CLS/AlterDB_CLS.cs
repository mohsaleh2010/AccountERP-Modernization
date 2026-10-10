namespace AccountModelsLib
{
    using AccountModelsLib.Models;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;


    //using AccountModelsLib.DBModels;

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Data;
    using System.Drawing;
    using System.Net.NetworkInformation;
    using System.Reflection.Metadata;
 
    using System.Threading;
    using System.Xml.Linq;



    public class AlterDB_CLS
    {
        public static string IDENTITY = " IDENTITY(1,1) ";
        static int xmiliseconds = 100;
        static bool IsColumnAltered = false;
        public static void AlterDBOld(int d)
        {
            operations opp = new operations();
            string str = "";

            TruncateALogError(ref opp);
            AlterDBLastYear(ref opp, d);
            //AlterDBThisYear(ref opp);
        }
        //public static  int d = 2;
        static void AlterDBThisYear(ref operations opp, int d)
        {

            CreateTable(ref opp);

            ChangeTypeToLongUserId(ref opp);
            Create_Lookup_OrderDeliveryTypes(ref opp);
            SetDataDictionaryProcessesNo(ref opp);
            Update(ref opp, "Customer", "Customer_Type", "1", " (Customer_Type IS NULL)");

            ChangeType(ref opp, "numeric", d);
            ChangeType(ref opp, "float", d);
            //ChangeType(ref opp, "decimal(18, 2)");
            AlterColumnAddKey(ref opp, "SysConfigMenus", "Id");

            AlterColumn(ref opp, "Employee", "jobsListId", "bigint");
            AlterColumn(ref opp, "Employee", "Edit_User_Id", "bigint");
            AlterColumn(ref opp, "Employee", "SubDeptID", "bigint");
            AlterColumn(ref opp, "Employee", "SubAccount_Id", "bigint");
            AlterColumn(ref opp, "Employee", "Branch_Id", "bigint");
            AlterColumn(ref opp, "Employee", "User_Id", "bigint");
            AlterColumn(ref opp, "Account_Entry_Master_Balance", "Year_Financial_Period_Id", "bigint");
            AlterColumn(ref opp, "Account_Entry_Master_Balance", "Branch_Id", "bigint");
            AlterColumn(ref opp, "Account_Entry_Master_Balance", "Account_Currency_Id", "bigint");
            AlterColumn(ref opp, "Account_Entry_Master_Balance", "Equivalent_Account_Currency_Id", "bigint");
            AlterColumn(ref opp, "Account_Entry_Master_Balance", "User_Id", "bigint");
            AlterColumn(ref opp, "Account_Entry_Master_Balance", "Cost_Center_Tree_ID", "bigint");
            AlterColumn(ref opp, "Account_Entry_Master_Balance", "Edit_User_Id", "bigint");

            AlterColumn(ref opp, "Unit_item", "Branch_Id", "bigint");
            AlterColumn(ref opp, "Unit_item", "User_Id", "bigint");
            AlterColumn(ref opp, "Unit_item", "Edit_User_Id", "bigint");

            AlterColumn(ref opp, "unit", "Branch_Id", "bigint");
            AlterColumn(ref opp, "unit", "User_Id", "bigint");
            AlterColumn(ref opp, "unit", "Edit_User_Id", "bigint");

            AlterBD_(ref opp, "Machin_list", "Abstract_Hadafat", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Machin_list", "Speed", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Machin_list", "Machine_Width", "int", "0", "0");

            
            AlterBD_(ref opp, "DataDictionaryDbSyncLogs", "RecordIdStr", "nvarchar(MAX)");
            AlterBD_(ref opp, "DataDictionaryDbSyncLogs", "IsStatus", "int", "0", "0");
            AlterBD_(ref opp, "DataDictionaryDbSyncLogs", "User_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "DataDictionaryDbSyncLogs", "Is_EF_Or_Adonet", "int", "0", "0");

           

            AlterBD_(ref opp, "Cars", "Active", "int", "0", "0");
            AlterBD_(ref opp, "Cars", "IsChangeOwner", "int", "0", "0");
            AlterBD_(ref opp, "Cars", "Lookup_CarTypeId", "bigint", "0", "0");
            AlterBD_(ref opp, "Cars", "Lookup_CarTypeModelId", "bigint", "0", "0");
            AlterBD_(ref opp, "Cars", "Lookup_TransmissionTypeId", "bigint", "0", "0");
            AlterBD_(ref opp, "Cars", "NewId", "bigint", "0", "0");
            AlterBD_(ref opp, "Cars", "OldId", "bigint", "0", "0");
            AlterBD_(ref opp, "Cars", "CarAddress", "nvarchar(500)");
            AlterBD_(ref opp, "Cars", "DriverName", "nvarchar(500)");
            AlterBD_(ref opp, "Cars", "DriverTel", "nvarchar(500)");
            AlterBD_(ref opp, "Cars", "DriverTel", "nvarchar(500)");
            AlterColumn(ref opp, "Cars", "Active", "int","1","1");
            AlterColumn(ref opp, "Cars", "IsChangeOwner", "int", "0","0");

            //Microsoft.Data.SqlClient.SqlException: 'Invalid column name 'Active'.
//Invalid column name 'CarAddress'.
//Invalid column name 'DriverName'.
//Invalid column name 'DriverTel'.
//Invalid column name 'IsChangeOwner'.



            AlterColumn(ref opp, "Invoices_Details", "Bills_Details_Id", "bigint");
            AlterColumn(ref opp, "Invoices_Details_Reverse", "Bills_Details_Id", "bigint");

            AlterBD_(ref opp, "Invoices_Details", "Store_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Store_Name", "nvarchar(500)");


            AlterBD_(ref opp, "SysConfigMenus", "RazorPath", "nvarchar(500)");
            AlterBD_(ref opp, "SysConfigMenus", "RazorPathTemp", "nvarchar(500)");
            AlterBD_(ref opp, "SysConfigMenus", "Description_En", "nvarchar(500)");
            AlterBD_(ref opp, "SysConfigMenus", "Description_Fr", "nvarchar(500)");
            AlterBD_(ref opp, "SysConfigMenus", "Description_DE", "nvarchar(500)");
            AlterBD_(ref opp, "SysConfigMenus", "UserSells", "nvarchar(500)");
            AlterBD_(ref opp, "SysConfigMenus", "IsShowToUser", "int", "0", "0");


            AlterBD_(ref opp, "USER_DETAILES_Branches_Permission", "Branch_Id", "bigint", "0", "0");


            AlterBD_(ref opp, "Machin_list", "Code", "nvarchar(max)");
            AlterBD_(ref opp, "Machin_list", "Description", "nvarchar(max)");

            AlterBD_(ref opp, "Invoices_List", "CustomerPhone", "nvarchar(max)");
            AlterBD_(ref opp, "Invoices_List_Reverse", "CustomerPhone", "nvarchar(max)");
            AlterBD_(ref opp, "Invoices_List", "Delivery_Address", "nvarchar(max)");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Delivery_Address", "nvarchar(max)");

            AlterBD_(ref opp, "Invoices_List", "AppointmentsId", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "AppointmentsId", "bigint", "0", "0");


            AlterBD_(ref opp, "Invoices_List", "WaiterManId", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "WaiterManId", "bigint", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "Money_Transaction_BoxId", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Money_Transaction_BoxId", "bigint", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "Invoices_ListTempId", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Invoices_ListTempId", "bigint", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "IsDeliveryFeeAddedToInvoiceTotal", "int", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "IsDeliveryFeeAddedToInvoiceTotal", "int", "0", "0");


            AlterBD_(ref opp, "Invoices_List", "IsDeliveryFeeAddedToInvoiceTotalAfterTax", "int", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "IsDeliveryFeeAddedToInvoiceTotalAfterTax", "int", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Is_Reconciled_With_ManDriver", "int", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Is_Reconciled_With_ManDriver", "int", "0", "0");


            AlterBD_(ref opp, "Invoices_List", "CustomerAccountAfterNakdia", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "CustomerAccountAfterNakdia", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Invoices_List", "PaymentValueFor_Is_Invoice_Agile", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "PaymentValueFor_Is_Invoice_Agile", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "OverPaidAmount", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "OverPaidAmount", "decimal(18, " + d + ")", "0", "0");



            AlterBD_(ref opp, "Invoices_List", "IsUseExtraAmountToPayDebt", "bit", "1", "1");
            AlterBD_(ref opp, "Invoices_List_Reverse", "IsUseExtraAmountToPayDebt", "bit", "1", "1");


            AlterBD_(ref opp, "Invoices_Details", "AppointmentsId", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "AppointmentsId", "bigint", "0", "0");

            AlterBD_(ref opp, "Invoices_Details", "AppointmentServicesId", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "AppointmentServicesId", "bigint", "0", "0");

            AlterBD_(ref opp, "Invoices_Details", "InvoiceKitchenStatusId", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "InvoiceKitchenStatusId", "bigint", "0", "0");


            AlterBD_(ref opp, "Invoices_List", "IsInvoiceTaxInfo", "int", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "IsInvoiceTaxInfo", "int", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "Is_DeletedProcessed", "int", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Is_DeletedProcessed", "int", "0", "0");


            AlterBD_(ref opp, "Invoices_List", "RestValueFromCashNakdia", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "RestValueFromCashNakdia", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "PaidValueFromCashNakdy", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "PaidValueFromCashNakdy", "decimal(18, " + d + ")", "0", "0");



            AlterBD_(ref opp, "Invoices_List", "IsMaintainceInvoice", "int", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "IsMaintainceInvoice", "int", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "TaxInvoices_ListId", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "TaxInvoices_ListId", "bigint", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "TaxInvoices_ListId", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "TaxInvoices_ListId", "bigint", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "IsUploadToPortal", "int", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "IsUploadToPortal", "int", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "TaxInternalId", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_List_Reverse", "TaxInternalId", "nvarchar(500)");


            AlterBD_(ref opp, "Invoices_List", "CarNumber", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_List_Reverse", "CarNumber", "nvarchar(500)");

            AlterBD_(ref opp, "Invoices_List", "CarsId", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "CarsId", "bigint", "0", "0");





            AlterBD_(ref opp, "Invoices_List", "CarsWorkOrdersId", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "CarsWorkOrdersId", "bigint", "0", "0");


            AlterBD_(ref opp, "Invoices_List", "WorkOrderNumber", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "WorkOrderNumber", "bigint", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "CustomerName", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_List_Reverse", "CustomerName", "nvarchar(500)");

            AlterBD_(ref opp, "Invoices_List", "CustomerDeliveryName", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_List_Reverse", "CustomerDeliveryName", "nvarchar(500)");


            AlterBD_(ref opp, "Invoices_List", "TaxInternalId", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_List_Reverse", "TaxInternalId", "nvarchar(500)");

            AlterBD_(ref opp, "Invoices_List", "Service_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Service_Amount", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Delivery_Fee", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Invoices_List_Reverse", "Service_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Service_Amount", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Delivery_Fee", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "Lookup_ShiftsId", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Lookup_ShiftsId", "bigint", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "TableId", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "TableId", "bigint", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "DeliveryMan_DriverId", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "DeliveryMan_DriverId", "bigint", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "Lookup_DiscountTypesId", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Lookup_DiscountTypesId", "bigint", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "Is_Printed", "int", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Is_Printed", "int", "0", "0");



            AlterBD_(ref opp, "Invoices_List", "InvoiceStatus", "int", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "InvoiceStatus", "int", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "InvoiceStatus_Reason", "nvarchar(max)");
            AlterBD_(ref opp, "Invoices_List_Reverse", "InvoiceStatus_Reason", "nvarchar(max)");

            AlterBD_(ref opp, "Invoices_List", "Qnty_ValueSparePartsOils", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "ATS05ListSparePartsOils_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "ATS05ListSparePartsOils_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "TaxSalesListSparePartsOils_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "TaxSalesListSparePartsOils_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "CommissionListSparePartsOils_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "CommissionListSparePartsOils_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Total_CommissionListSparePartsOils", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Total_ListPriceWithoutTax_SparePartsOils", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "NetInvoiceTotal_SparePartsOils", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Item_Net_TotalSparePartsOils", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Total_ListPriceAfterTaxSales_SparePartsOils", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Qnty_ValueRawMaterials", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Qnty_ValueRawMaterials", "decimal(18, " + d + ")", "0", "0");



            AlterBD_(ref opp, "Invoices_List_Reverse", "Qnty_ValueSparePartsOils", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "ATS05ListSparePartsOils_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "ATS05ListSparePartsOils_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "TaxSalesListSparePartsOils_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "TaxSalesListSparePartsOils_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "CommissionListSparePartsOils_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "CommissionListSparePartsOils_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Total_CommissionListSparePartsOils", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Total_ListPriceWithoutTax_SparePartsOils", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "NetInvoiceTotal_SparePartsOils", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Item_Net_TotalSparePartsOils", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Total_ListPriceAfterTaxSales_SparePartsOils", "decimal(18, " + d + ")", "0", "0");




            AlterBD_(ref opp, "Invoices_List", "TaxInvoiceNo", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "TaxInvoiceNo", "bigint", "0", "0");

            ///////


            AlterBD_(ref opp, "Invoices_List", "Total_ListPriceWithoutTax_RawMaterial", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Total_ListPriceWithoutTax_RawMaterial", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "Total_ListPriceWithoutTax_RawMaterial", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Total_ListPriceWithoutTax_RawMaterial", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "NetInvoiceTotal_RawMaterial", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "NetInvoiceTotal_RawMaterial", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "Total_ListPriceAfterTaxSales_RawMaterial", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Total_ListPriceAfterTaxSales_RawMaterial", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "Customer_Id_RelatedToAccount", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Customer_Id_RelatedToAccount", "bigint", "0", "0");



            AlterBD_(ref opp, "Invoices_List_Reverse", "Items_GroupsId", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "CarsWorkOrdersItemProcessedDetailsId", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "CarsWorkOrdersItemProcessedDetailsId", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "TafkeetNet_Total", "nvarchar(max)");
            AlterBD_(ref opp, "Invoices_List_Reverse", "TafkeetNet_Total", "nvarchar(max)");
            AlterBD_(ref opp, "Invoices_List", "Qnty_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Qnty_Value", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Invoices_Details", "WaiterManId", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "WaiterManId", "bigint", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "Qnty_ValueWorkmanship", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Qnty_ValueSpareParts", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Qnty_ValueOils", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "ATS05ListWorkmanship_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "ATS05ListSpareParts_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "ATS05ListOils_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "ATS05ListWorkmanship_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "ATS05ListSpareParts_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "ATS05ListOils_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "TotalListATS05_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "TaxSalesListWorkmanship_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "TaxSalesListSpareParts_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "TaxSalesListOils_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "TaxSalesListWorkmanship_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "TaxSalesListSpareParts_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "TaxSalesListOils_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "TotalListTaxSales10_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "CommissionListWorkmanship_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "CommissionListSpareParts_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "CommissionListOils_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "CommissionListWorkmanship_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "CommissionListSpareParts_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "CommissionListOils_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Total_CommissionListWorkmanship", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Total_CommissionListSpareParts", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Total_CommissionListOils", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Total_CommissionList", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "List_Final_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Net_Total_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Total_ListPriceWithoutTax_Workmanship", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Total_ListPriceWithoutTax_SpareParts", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Total_ListPriceWithoutTax_Oils", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Total_ListPriceWithoutTax", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "NetInvoiceTotal_Workmanship", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "NetInvoiceTotal_SpareParts", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "NetInvoiceTotal_Oils", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "NetInvoiceTotal", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "Item_Net_TotalWorkmanship", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Item_Net_TotalSpareParts", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Item_Net_TotalOils", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Invoices_List", "Item_Net_Total", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Total_ItemPriceWithoutTax", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Total_ListPriceAfterTaxSales_Workmanship", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Total_ListPriceAfterTaxSales_SpareParts", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Total_ListPriceAfterTaxSales_Oils", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Total_ListPriceAfterTaxSales", "decimal(18, " + d + ")", "0", "0");


            ///////////////////
            ///


            AlterBD_(ref opp, "Invoices_List_Reverse", "Qnty_ValueWorkmanship", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Qnty_ValueSpareParts", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Qnty_ValueOils", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "ATS05ListWorkmanship_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "ATS05ListSpareParts_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "ATS05ListOils_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "ATS05ListWorkmanship_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "ATS05ListSpareParts_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "ATS05ListOils_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "TotalListATS05_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "TaxSalesListWorkmanship_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "TaxSalesListSpareParts_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "TaxSalesListOils_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "TaxSalesListWorkmanship_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "TaxSalesListSpareParts_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "TaxSalesListOils_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "TotalListTaxSales10_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "CommissionListWorkmanship_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "CommissionListSpareParts_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "CommissionListOils_Rate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "CommissionListWorkmanship_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "CommissionListSpareParts_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "CommissionListOils_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Total_CommissionListWorkmanship", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Total_CommissionListSpareParts", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Total_CommissionListOils", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Total_CommissionList", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "List_Final_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Net_Total_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Total_ListPriceWithoutTax_Workmanship", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Total_ListPriceWithoutTax_SpareParts", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Total_ListPriceWithoutTax_Oils", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Total_ListPriceWithoutTax", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "NetInvoiceTotal_Workmanship", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "NetInvoiceTotal_SpareParts", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "NetInvoiceTotal_Oils", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "NetInvoiceTotal", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Invoices_List_Reverse", "Item_Net_TotalWorkmanship", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Item_Net_TotalSpareParts", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Item_Net_TotalOils", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Invoices_List_Reverse", "Item_Net_Total", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Total_ItemPriceWithoutTax", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Total_ListPriceAfterTaxSales_Workmanship", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Total_ListPriceAfterTaxSales_SpareParts", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Total_ListPriceAfterTaxSales_Oils", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Total_ListPriceAfterTaxSales", "decimal(18, " + d + ")", "0", "0");




            AlterBD_(ref opp, "Invoices_List", "Item_Net_TotalRawMaterials", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Item_Net_TotalRawMaterials", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "Total_List3Mola", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Total_List3Mola", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Invoices_List", "WahbaValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "WahbaValue", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "NawlounValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "NawlounValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Total3MolaRate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Total3MolaRate", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Invoices_List", "Value_3Mola", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Value_3Mola", "decimal(18, " + d + ")", "0", "0");



            AlterBD_(ref opp, "Invoices_List", "ShipmentCompany_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "ShipmentCompany_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_List", "Shipment_Value", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Invoices_List_Reverse", "ShipmentCompany_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "ShipmentCompany_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Shipment_Value", "decimal(18, " + d + ")", "0", "0");




            AlterBD_(ref opp, "Invoices_List", "InvoiceDueDate", "datetime", "getdate()", "null");
            AlterBD_(ref opp, "Invoices_List_Reverse", "InvoiceDueDate", "datetime", "getdate()", "null");
            AlterBD_(ref opp, "Bills_List", "InvoiceDueDate", "datetime", "getdate()", "null");
            AlterBD_(ref opp, "Bills_List_Reverse", "InvoiceDueDate", "datetime", "getdate()", "null");


            //////////////////////
            ///



            AlterBD_(ref opp, "Bills_List", "CarsWorkOrdersId", "bigint", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "CarsWorkOrdersId", "bigint", "0", "0");





            AlterBD_(ref opp, "AppointmentServices", "Invoices_List_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "AppointmentServices", "Invoices_Details_Id", "bigint", "0", "0");




            AlterBD_(ref opp, "Bills_Details", "Qnty_Another_Balance", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Qnty_Another_Balance", "decimal(18, " + d + ")", "0", "0");




            AlterBD_(ref opp, "Bills_List", "CustomerAccountBefor", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "CustomerAccountBefor", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Bills_List", "CustomerAccountAfter", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "CustomerAccountAfter", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Bills_List", "Total_ItemPriceWithoutTax", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "Total_ItemPriceWithoutTax", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Bills_List", "Total_ListPriceWithoutTax", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "Total_ListPriceWithoutTax", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Bills_List", "IsMaintainceInvoice", "int", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "IsMaintainceInvoice", "int", "0", "0");
            AlterBD_(ref opp, "Bills_List", "IsInvoiceTaxInfo", "int", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "IsInvoiceTaxInfo", "int", "0", "0");

            AlterBD_(ref opp, "Item_BarCodeSetting", "TextUnderCode", "nvarchar(500)");
            AlterBD_(ref opp, "Item_BarCodeSetting", "IsPrintPrice", "nvarchar(500)");

            AlterBD_(ref opp, "CarsExaminations", "IsCarsExaminationClosed", "int", "0", "0");
            AlterBD_(ref opp, "CarsExaminations", "Transaction_Type", "int", "0", "2");
            AlterBD_(ref opp, "CarsExaminations", "IS_Posted_Flage", "int", "0", "0");
            AlterBD_(ref opp, "Supplier_Price", "Price_PurchaseType", "int", "0", "0");




            AlterBD_(ref opp, "CarsExaminations", "Account_Entry_List_ID", "bigint", "0", "0");
            AlterBD_(ref opp, "CarsExaminations", "SubAccount_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "CarsExaminations", "Currency_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "CarsExaminations", "CarsExaminationValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "CarsExaminations", "CarsExaminationClosedDate", "datetime", "getdate()", "null");
            AlterBD_(ref opp, "CarsExaminations", "CarsExaminationStatus", "nvarchar(500)");
            AlterBD_(ref opp, "CarsExaminations", "Notes", "nvarchar(MAX)");

            AlterBD_(ref opp, "CarsExaminations", "ProcessNo", "bigint", "0", "0");
            AlterBD_(ref opp, "CarsExaminations", "Order_MoneyId", "bigint", "0", "0");



            AlterBD_(ref opp, "CarsExaminations", "Lookup_CarTypeId", "bigint", "0", "0");

            AlterBD_(ref opp, "CarsExaminations", "Lookup_CarTypeModelId", "bigint", "0", "0");
            AlterBD_(ref opp, "CarsExaminations", "CarType", "nvarchar(500)");
            AlterBD_(ref opp, "CarsExaminations", "Model", "nvarchar(500)");
            AlterBD_(ref opp, "CarsExaminations", "Year", "nvarchar(500)");



            AlterBD_(ref opp, "CarsWorkOrdersRequestedDetails", "P_Price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "CarsWorkOrdersDeliverDetails", "P_Price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "CarsWorkOrdersDeliverItemBalances", "P_Price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "CarsWorkOrdersItemProcessedDetails", "P_Price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "CarsWorkOrdersItemReversedProcessedDetails", "P_Price", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "CarsWorkOrdersItemProcessedDetails", "Qnty_Value_P_Price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "CarsWorkOrdersItemReversedProcessedDetails", "Qnty_Value_P_Price", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "CarsWorkOrdersItemProcessedDetails", "Profits", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "CarsWorkOrdersItemReversedProcessedDetails", "Profits", "decimal(18, " + d + ")", "0", "0");




            AlterBD_(ref opp, "CarsWorkOrdersRequestedDetails", "P_priceAvgCost", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "CarsWorkOrdersDeliverDetails", "P_priceAvgCost", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "CarsWorkOrdersDeliverItemBalances", "P_priceAvgCost", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "CarsWorkOrdersItemProcessedDetails", "P_priceAvgCost", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "CarsWorkOrdersItemReversedProcessedDetails", "P_priceAvgCost", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "CarsWorkOrdersRequestedDetails", "Order_PurshingId", "bigint", "0", "0");
            AlterBD_(ref opp, "CarsWorkOrdersDeliverDetails", "Order_PurshingId", "bigint", "0", "0");
            AlterBD_(ref opp, "CarsWorkOrdersItemProcessedDetails", "Order_PurshingId", "bigint", "0", "0");
            AlterBD_(ref opp, "CarsWorkOrdersItemReversedProcessedDetails", "Order_PurshingId", "bigint", "0", "0");

            //Order_PurshingId Order_Purshing_No  Order_Purshing_Date

            AlterBD_(ref opp, "CarsWorkOrdersRequestedDetails", "Order_Purshing_No", "bigint", "0", "0");
            AlterBD_(ref opp, "CarsWorkOrdersDeliverDetails", "Order_Purshing_No", "bigint", "0", "0");
            AlterBD_(ref opp, "CarsWorkOrdersItemProcessedDetails", "Order_Purshing_No", "bigint", "0", "0");
            AlterBD_(ref opp, "CarsWorkOrdersItemReversedProcessedDetails", "Order_Purshing_No", "bigint", "0", "0");

            AlterBD_(ref opp, "CarsWorkOrdersRequestedDetails", "Order_Purshing_Date", "datetime", "getdate()", "null");
            AlterBD_(ref opp, "CarsWorkOrdersDeliverDetails", "Order_Purshing_Date", "datetime", "getdate()", "null");

            AlterBD_(ref opp, "CarsWorkOrdersItemProcessedDetails", "Order_Purshing_Date", "datetime", "getdate()", "null");
            AlterBD_(ref opp, "CarsWorkOrdersItemReversedProcessedDetails", "Order_Purshing_Date", "datetime", "getdate()", "null");


            AlterBD_(ref opp, "USER_DETAILES", "Emp_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "AccountTaxInfo", "MinAllowedValueToComputeTax", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "CarsWorkOrders", "Repairs_NotDone", "nvarchar(max)");


            AlterBD_(ref opp, "CarsWorkOrders", "ExaminationReport", "nvarchar(max)");
            AlterBD_(ref opp, "CarsWorkOrders", "WorkOrderClosed_Reason", "nvarchar(max)");
            AlterBD_(ref opp, "CarsWorkOrders", "CustomerOrderNumberPO", "nvarchar(max)");
            AlterBD_(ref opp, "CarsWorkOrders", "CustomerOrderNumberGR", "nvarchar(max)");
            AlterBD_(ref opp, "CarsWorkOrders", "CustomerId_CarOwner", "bigint", "0", "CustomerId");
            AlterBD_(ref opp, "CarsWorkOrders", "IsInsuranceCompanyStaus", "int", "0", "1");
            AlterBD_(ref opp, "CarsWorkOrders", "IsInvoiceTaxInfo", "int", "0", "0");


            AlterBD_(ref opp, "Machin_list", "active", "int", "0", "1");



            AlterBD_(ref opp, "Currency", "code", "nvarchar(500)");

            AlterBD_(ref opp, "Currency", "rate", "decimal(18, " + d + ")");




            AlterBD_(ref opp, "", "TafkeetNet_Total", "nvarchar(max)");

            AlterBD_(ref opp, "Bills_List", "IsInvoiceTaxInfo", "int", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "IsInvoiceTaxInfo", "int", "0", "0");


            AlterBD_(ref opp, "FixedAssetData", "Active", "int", "1", "1");
            AlterBD_(ref opp, "Store", "Active", "int", "1");
            AlterBD_(ref opp, "Store", "IsCar", "int", "0");
            AlterBD_(ref opp, "Store", "IS_Virtual_OuterStore", "int", "0", "0");
            AlterBD_(ref opp, "Store", "IS_ServicesVirtual_Item_NoBalance", "int", "0", "0");
            AlterBD_(ref opp, "Store", "LookUp_StoreOrderTypesId", "int", "0", "4");
            AlterBD_(ref opp, "Store", "SubAccount_Id", "bigint", "0", "0");


            AlterBD_(ref opp, "Detailes_Purshing", "WorkOrderNumber", "nvarchar(500)");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "WorkOrderNumber", "nvarchar(500)");

            AlterBD_(ref opp, "Detailes_Purshing", "CarNumber", "nvarchar(500)");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "CarNumber", "nvarchar(500)");

            AlterBD_(ref opp, "Detailes_Purshing", "CarsWorkOrdersId", "bigint", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "CarsWorkOrdersId", "bigint", "0", "0");

            AlterBD_(ref opp, "Detailes_Purshing", "CarsId", "bigint", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "CarsId", "bigint", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "ProcessName", "nvarchar(500)", "''", "Notes");


            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Id_FromStr", "nvarchar(500)", "''", "Notes");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Id_ToStr", "nvarchar(500)", "''", "Notes");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "ProcessNameId", "int", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Type_From", "int", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Id_From", "bigint", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Type_To", "int", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Id_To", "bigint", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "StoreId", "bigint", "0", "0");

            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "StoreId_Name", "nvarchar(500)", "''", "Notes");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Type_From_Name", "nvarchar(500)", "''", "Notes");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Type_To_Name", "nvarchar(500)", "''", "Notes");



            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "ProcessNameNumber", "nvarchar(500)", "''", "Notes");
            AlterBD_(ref opp, "Detailes_Purshing", "ProcessNameNumber", "nvarchar(500)", "''", "Notes");


            AlterBD_(ref opp, "Detailes_Purshing", "ProcessName", "nvarchar(500)", "''", "Notes");

            AlterBD_(ref opp, "Detailes_Purshing", "Id_From", "bigint", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing", "Id_To", "bigint", "0", "0");


            AlterBD_(ref opp, "Detailes_Purshing", "Id_FromStr", "nvarchar(500)", "''", "Notes");
            AlterBD_(ref opp, "Detailes_Purshing", "Id_ToStr", "nvarchar(500)", "''", "Notes");
            AlterBD_(ref opp, "Detailes_Purshing", "ProcessNameId", "int", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing", "Type_From", "int", "0", "0");

            AlterBD_(ref opp, "Detailes_Purshing", "Type_To", "int", "0", "0");

            AlterBD_(ref opp, "Detailes_Purshing", "EmployeeId", "bigint", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "EmployeeId", "bigint", "0", "0");
            AlterBD_(ref opp, "Order_Purshing", "EmployeeId", "bigint", "0", "0");

            AlterBD_(ref opp, "Detailes_Purshing", "EmployeeName", "nvarchar(500)");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "EmployeeName", "nvarchar(500)");
            AlterBD_(ref opp, "Order_Purshing", "EmployeeName", "nvarchar(500)");



            AlterBD_(ref opp, "Customer", "Customer_Id_RelatedToAccount", "bigint", "0", "0");
            AlterBD_(ref opp, "CarsWorkOrders", "Customer_Id_RelatedToAccount", "bigint", "0", "0");




            AlterBD_(ref opp, "Customers_Accounts", "CarsWorkOrdersId", "bigint", "0", "0");
            AlterBD_(ref opp, "Customers_Accounts_Mokhtalat", "Customer_Id_RelatedToAccount", "bigint", "0", "0");
            AlterBD_(ref opp, "Customers_Accounts", "Customer_Id_RelatedToAccount", "bigint", "0", "Customer_Id");
            //AlterBD_(ref opp, "Customers_Accounts_Mokhtalat", "CarsWorkOrdersId", "bigint", "0", "0");
            //     CarNumber   CarsId
            //        WorkOrderNumber bigint  Checked
            //CustomerName    nvarchar(500)   Checked
            //CarsWorkOrdersId    bigint Checked

            AlterBD_(ref opp, "Customers_Accounts", "CarsId", "bigint", "0", "0");
            //AlterBD_(ref opp, "Customers_Accounts_Mokhtalat", "CarsId", "bigint", "0", "0");
            AlterBD_(ref opp, "Suppliers_Accounts", "CarsId", "bigint", "0", "0");

            AlterBD_(ref opp, "Customers_Accounts", "CarNumber", "nvarchar(500)");
            //AlterBD_(ref opp, "Customers_Accounts_Mokhtalat", "CarNumber", "nvarchar(500)");
            AlterBD_(ref opp, "Suppliers_Accounts", "CarNumber", "nvarchar(500)");


            AlterBD_(ref opp, "Customers_Accounts", "WorkOrderNumber", "bigint", "0", "0");
            //AlterBD_(ref opp, "Customers_Accounts_Mokhtalat", "WorkOrderNumber", "bigint", "0", "0");
            //AlterBD_(ref opp, "Customers_Accounts_Mokhtalat", "Serial", "int", "0", "0");

            AlterBD_(ref opp, "Customers_Accounts", "CustomerName", "nvarchar(500)");
            //AlterBD_(ref opp, "Customers_Accounts_Mokhtalat", "CustomerName", "nvarchar(500)");

            AlterBD_(ref opp, "Customers_Accounts", "TaxInternalId", "nvarchar(500)");
            //AlterBD_(ref opp, "Customers_Accounts_Mokhtalat", "TaxInternalId", "nvarchar(500)");
            AlterBD_(ref opp, "Suppliers_Accounts", "TaxInternalId", "nvarchar(500)");


            AlterBD_(ref opp, "Suppliers_Accounts", "SupplierName", "nvarchar(500)");
            AlterBD_(ref opp, "Suppliers_Accounts", "WorkOrderNumber", "bigint", "0", "0");
            AlterBD_(ref opp, "Suppliers_Accounts", "CarsWorkOrdersId", "bigint", "0", "0");





            AlterBD_(ref opp, "Bills_Details", "UnitSmallStr", "nvarchar(500)");
            AlterBD_(ref opp, "Bills_Details_Reverse", "UnitSmallStr", "nvarchar(500)");

            AlterBD_(ref opp, "Bills_Details", "Available_Larg_Balance", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Available_Larg_Balance", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Available_Small_Balance", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Available_Small_Balance", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Bills_Details", "PriceUnit", "decimal(18, " + d + ")", "0", "Larg_Unit_Price");



            AlterBD_(ref opp, "Invoices_Details", "Unit_Item_SalesDateId", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Unit_Item_SalesDateId", "bigint", "0", "0");

            AlterBD_(ref opp, "Invoices_Details", "Qnty", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Qnty", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Invoices_Details", "Available_Larg_Balance", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Available_Larg_Balance", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "Available_Small_Balance", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Available_Small_Balance", "decimal(18, " + d + ")", "0", "0");

            ////////
            ///
            AlterBD_(ref opp, "Invoices_Details", "ProductCost_Price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "ProductCost_Price", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Invoices_Details", "P_priceAvgCost", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "P_priceAvgCost", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "S_priceAvgCost", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "S_priceAvgCost", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Invoices_Details", "Larg_Qnty_Balance", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Larg_Qnty_Balance", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Invoices_Details", "Small_Qnty_Balance", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Small_Qnty_Balance", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Invoices_Details", "PriceUnit", "decimal(18, " + d + ")", "0", "Larg_Unit_Price");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "PriceUnit", "decimal(18, " + d + ")", "0", "Larg_Unit_Price");




            AlterBD_(ref opp, "Invoices_Details", "CarsWorkOrdersItemProcessedDetailsId", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "CarsWorkOrdersItemProcessedDetailsId", "bigint", "0", "0");


            AlterBD_(ref opp, "Bills_List", "TafkeetNet_Total", "nvarchar(max)");
            AlterBD_(ref opp, "Bills_List_Reverse", "TafkeetNet_Total", "nvarchar(max)");



            AlterBD_(ref opp, "Item", "IsCommissionEmployeeBased", "int", "0", "0");
            AlterBD_(ref opp, "Item", "IsRetailItem", "int", "0", "0");
            AlterBD_(ref opp, "Item", "RetailPrice", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Item", "CommissionEmployeeValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Item", "ItemServiceCategoryId", "bigint", "0", "0");
            AlterBD_(ref opp, "Item", "TotalSessions", "int", "1", "1");


            AlterBD_(ref opp, "Item", "ServiceDurationInMinutes", "int", "0", "0");
            AlterBD_(ref opp, "Item", "LookUp_CodeTaxTypesId", "bigint", "0", "0");
            AlterBD_(ref opp, "Item", "LookUp_TaxableTypesId", "bigint", "0", "0");
            AlterBD_(ref opp, "Item", "LookUp_TaxableSubTypesId", "bigint", "0", "0");
            AlterBD_(ref opp, "Item", "LookUp_NonTaxableTypesId", "bigint", "0", "0");
            AlterBD_(ref opp, "Item", "Items_NaturesId", "bigint", "0", "0");
            AlterBD_(ref opp, "Item", "LookUp_CodeTaxTypesId", "bigint", "0", "0");
            AlterBD_(ref opp, "Item", "Is_NotShowOnInvoice", "int", "0", "0");
            AlterBD_(ref opp, "Item", "IsMasna3iatKhamat", "int", "0", "0");
            AlterBD_(ref opp, "Item", "Is_ItemOrService", "int", "0", "1");
            AlterBD_(ref opp, "Item", "InternalCode", "bigint", "0", "0");

            AlterBD_(ref opp, "Item", "ItemName_OnInvoice", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "ItemSizeDescription", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "CountryOfOrigin", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "InternationalTaxNumber", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "ItemCodeType", "nvarchar(500)", "", "EGS");
            AlterBD_(ref opp, "Item", "UnitCodeTax", "nvarchar(500)", "", "SET");
            AlterBD_(ref opp, "Item", "CodeNameAr", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "CodeName", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "FactoryCode", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "ItemCodeType", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "UnitCodeTax", "nvarchar(500)");


            AlterBD_(ref opp, "CarsWorkOrdersDeliverDetails", "P_priceAvgCost", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "CarsWorkOrdersItemProcessedDetails", "P_priceAvgCost", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "CarsWorkOrdersItemReversedProcessedDetails", "P_priceAvgCost", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "CarsWorkOrdersRequestedDetails", "P_priceAvgCost", "decimal(18, " + d + ")", "0", "0");



            AlterBD_(ref opp, "Item_First_Period_Balance", "P_priceAvgCost", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Item_First_Period_Balance", "S_priceAvgCost", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Item_First_Period_Balance", "P_price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Item_First_Period_Balance", "S_price", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Item", "P_priceAvgCost", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Item", "S_priceAvgCost", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Unit_Item", "P_priceAvgCost", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Unit_Item", "S_priceAvgCost", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Unit_Item", "ProductCost_Price", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Item", "ATS05_Value", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Item", "TaxSales10_Value", "decimal(18, " + d + ")");

            AlterBD_(ref opp, "Item", "LookUp_CodeTaxTypeStr", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "description", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "descriptionAr", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "ItemCode", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "linkedCode", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "ParentCode", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "requestReason", "nvarchar(500)");


            AlterBD_(ref opp, "Item", "Items_GroupsId", "bigint", "0", "0");
            AlterBD_(ref opp, "Item", "ItemCodeTaxAcceptedId", "bigint", "0", "0");
            AlterBD_(ref opp, "Item", "Is_UploadToTaxPortal", "int", "0", "0");


            //AlterBD_(ref opp, "Item", "items_Group_Id", "bigint", "0", "0");
            //AlterBD_(ref opp, "Item", "lookUp_NonTaxableType_Id", "bigint", "0", "0");
            //AlterBD_(ref opp, "Item", "lookUp_TaxableSubType_Id", "bigint", "0", "0");
            //AlterBD_(ref opp, "Item", "lookUp_TaxableType_Id", "bigint", "0", "0");




            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "S_Price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "P_Price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "PriceUnit", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Qnty_value", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "P_priceAvgCost", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "S_priceAvgCost", "decimal(18, " + d + ")", "0", "0");



            AlterBD_(ref opp, "Detailes_Purshing", "P_priceAvgCost", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing", "S_priceAvgCost", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Detailes_Purshing", "QntyStr", "nvarchar(500)");
            AlterBD_(ref opp, "Detailes_Purshing", "Item_NoStr", "nvarchar(500)");
            AlterBD_(ref opp, "Detailes_Purshing", "QntyStr_Befor", "nvarchar(500)");
            AlterBD_(ref opp, "Detailes_Purshing", "Item_NoStr_Befor", "nvarchar(500)");
            AlterBD_(ref opp, "Detailes_Purshing", "QntyStr_After", "nvarchar(500)");
            AlterBD_(ref opp, "Detailes_Purshing", "Item_NoStr_After", "nvarchar(500)");
            AlterBD_(ref opp, "Detailes_Purshing", "Item_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Detailes_Purshing", "WorkOrderNumber", "nvarchar(500)");
            AlterBD_(ref opp, "Detailes_Purshing", "Invoice_No", "nvarchar(500)");
            AlterBD_(ref opp, "Detailes_Purshing", "Code", "nvarchar(500)");


            AlterBD_(ref opp, "Detailes_Purshing", "PriceUnit", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Detailes_Purshing", "Qnty_value", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Detailes_Purshing", "Item_Total_WithoutTax", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Detailes_Purshing", "Item_ATS05_Value", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Detailes_Purshing", "Item_TaxSales10_Value", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Detailes_Purshing", "Total", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Detailes_Purshing", "Net_Total_Item_Commission", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Detailes_Purshing", "Item_Net_Total_Price", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Detailes_Purshing", "S_Price", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Detailes_Purshing", "P_Price", "decimal(18, " + d + ")");

            AlterBD_(ref opp, "Detailes_Purshing", "FromStoreId", "bigint", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing", "ToStoreId", "bigint", "0", "0");

            AlterBD_(ref opp, "Detailes_Purshing", "Item_Id", "bigint", "0", "0");

            AlterBD_(ref opp, "Detailes_Purshing", "Item_DeletedFlage", "int", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing", "Item_ProcessedFlage", "int", "0", "0");


            AlterBD_(ref opp, "Detailes_Purshing", "CarsWorkOrdersItemProcessedDetailsId", "bigint", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing", "Item_Category_Id", "bigint", "0", "0");

            AlterBD_(ref opp, "Detailes_Purshing", "Item_Category_Name", "nvarchar(500)");



            AlterBD_(ref opp, "unit", "IS_Default", "int", "0", "0");

            AlterBD_(ref opp, "unit", "Code", "nvarchar(500)");
            AlterBD_(ref opp, "unit", "Desc_ar", "nvarchar(500)");
            AlterBD_(ref opp, "unit", "Desc_en", "nvarchar(500)");
            AlterBD_(ref opp, "Currency", "Code", "nvarchar(500)");
            AlterBD_(ref opp, "Currency", "Desc_en", "nvarchar(500)");
            AlterBD_(ref opp, "Currency", "Desc_ar", "nvarchar(500)");


            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "CarsWorkOrdersId", "bigint", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "CarsWorkOrdersDetailsId", "bigint", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "CarsWorkOrdersItemProcessedDetailsId", "bigint", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing", "Item_ATS05", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Detailes_Purshing", "Item_TaxSales10", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Detailes_Purshing", "Item_CommissionValue", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Detailes_Purshing", "Unit_CommissionValue", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Detailes_Purshing", "Item_CommissionRate", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Detailes_Purshing", "ItemCommissionTotal", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Detailes_Purshing", "Unit_Used_for_Price", "int", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing", "Larg_Unit_Price", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Detailes_Purshing", "Small_Unit_Price", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Detailes_Purshing", "Import_Qua", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Detailes_Purshing", "Import_Part_Qua", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Detailes_Purshing", "Import_Item_No", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Detailes_Purshing", "Export_Qua", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Detailes_Purshing", "Export_Part_Qua", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Detailes_Purshing", "Export_Item_No", "decimal(18, " + d + ")");


            AlterBD_(ref opp, "Detailes_Purshing", "StoreId_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Detailes_Purshing", "StoreId", "bigint", "0", "0");

            AlterBD_(ref opp, "Detailes_Purshing", "Unit_Id", "bigint", "0", "0");
            AlterBD__(ref opp, "Detailes_Purshing", "UnitStr", "nvarchar(500)", @$" UPDATE       Detailes_Purshing
SET                UnitStr = unit.B_Unit
FROM            Detailes_Purshing INNER JOIN
                         Unit_item ON Detailes_Purshing.Unit_item_Table_Id = Unit_item.id INNER JOIN
                         Item ON Unit_item.Item_id = Item.id INNER JOIN
                         unit ON Item.Unit_Id = unit.id
WHERE        (Detailes_Purshing.UnitStr IS NULL) ");



            AlterBD_(ref opp, "Detailes_Purshing", "CarsWorkOrdersId", "bigint", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing", "CarsWorkOrdersRequestedDetailsId", "bigint", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing", "CarsWorkOrdersDeliverDetailsId", "bigint", "0", "0");


            AlterBD_(ref opp, "Detailes_Purshing", "Type_From_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Detailes_Purshing", "Type_To_Name", "nvarchar(500)");



            AlterBD_(ref opp, "Order_Purshing", "Transaction_Type", "int", "0", "0");
            AlterBD_(ref opp, "Order_Purshing", "IsMoreTheOneOrderShow", "int", "0", "2");
            AlterBD_(ref opp, "Order_Purshing", "CarNumber", "nvarchar(500)");
            AlterBD_(ref opp, "Order_Purshing", "WorkOrderNumber", "nvarchar(500)");
            AlterBD_(ref opp, "Order_Purshing", "CarsId", "bigint", "0", "0");
            AlterBD_(ref opp, "Order_Purshing", "IsActiveMainOrder_Purshing", "int", "1", "1");

            AlterBD_(ref opp, "Detailes_Purshing", "Qnty", "decimal(18, " + d + ")");


            AlterBD_(ref opp, "Order_Purshing", "CarsWorkOrdersId", "bigint", "0", "0");


            AlterBD_(ref opp, "Order_Purshing", "OrderTitle", "nvarchar(500)");

            AlterBD_(ref opp, "Order_Purshing", "IS_EstlamRawMaterial", "int", "0", "0");
            AlterBD_(ref opp, "Order_Purshing", "WorkOrderNumber", "nvarchar(500)");
            AlterBD_(ref opp, "Order_Purshing", "Invoice_No", "nvarchar(500)");
            AlterBD__(ref opp, "Invoices_Details", "UnitStr", "nvarchar(500)", @$" UPDATE       Invoices_Details
SET                UnitStr = unit.B_Unit
FROM            Invoices_Details INNER JOIN
                         Item ON Invoices_Details.Item_Id = Item.id INNER JOIN
                         unit ON Item.Unit_Id = unit.id WHERE (Invoices_Details.UnitStr IS NULL)");

            AlterBD_(ref opp, "Invoices_Details_Reverse", "UnitStr", "nvarchar(500)");

            AlterBD_(ref opp, "Invoices_Details", "UnitSmallStr", "nvarchar(500)");
            AlterBD__(ref opp, "Invoices_Details_Reverse", "UnitSmallStr", "nvarchar(500)", @"						 UPDATE       Invoices_Details_Reverse
SET                UnitStr = unit.B_Unit
FROM            Invoices_Details_Reverse INNER JOIN
                         Item ON Invoices_Details_Reverse.Item_Id = Item.id INNER JOIN
                         unit ON Item.Unit_Id = unit.id");


            AlterBD__(ref opp, "Item", "UnitStr", "nvarchar(500)", @"UPDATE       Item
SET                UnitStr = unit.B_Unit
FROM            Item INNER JOIN
                         unit ON Item.Unit_Id = unit.id ");

            AlterBD_(ref opp, "Item", "Pharmacology", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "ScientificName", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "Mark", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "Licensing", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "CommisionPurchase", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Item", "Offers", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "hiddenItem", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "IsPrintBarCode", "int", "1", "1");
            AlterBD_(ref opp, "Item", "IsStopBuying", "int", "1", "1");
            AlterBD_(ref opp, "Item", "StorageTempratureDegree", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Item", "TableNo", "nvarchar(500)");

            AlterBD_(ref opp, "Item", "Code", "nvarchar(MAX)");
            AlterBD_(ref opp, "Unit_Item", "Code", "nvarchar(MAX)");
            AlterBD_(ref opp, "Item", "BarCode2", "nvarchar(MAX)");
            AlterBD_(ref opp, "Item", "BarCode3", "nvarchar(MAX)");
            AlterBD_(ref opp, "Item", "BarCode4", "nvarchar(MAX)");
            AlterBD_(ref opp, "Item", "BarCode5", "nvarchar(MAX)");

            AlterBD_(ref opp, "Item", "BlockNo", "int", "0", "0");
            AlterBD_(ref opp, "Item", "shelfNo", "int", "0", "0");
            AlterBD_(ref opp, "Item", "OrderOnshelfNo", "int", "0", "0");

            AlterBD_(ref opp, "Items_Types_Name", "IsActive", "int", "1", "1");

            AlterBD_(ref opp, "Unit_Item_ExpireDate", "Bills_Details_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Unit_Item_ExpireDate", "Supplier_Id", "bigint", "0", "0");
            //AlterBD_(ref opp, "Unit_Item_ExpireDate", "Unit_Item_BatchListId", "bigint", "0", "0");
            //AlterBD_(ref opp, "Unit_Item_ExpireDate", "Unit_Item_BatchGroupListId", "bigint", "0", "Unit_Item_BatchListId");

            //AlterBD_drop(ref opp, "Unit_Item_ExpireDate", "Unit_Item_BatchListId");
            AlterBD_drop(ref opp, "Unit_Item_ExpireDate", "Unit_Item_BatchListId", "Unit_Item_BatchGroupListId", "bigint", "0", "Unit_Item_BatchListId");
            AlterBD_drop(ref opp, "Unit_Item_SalesDate", "Unit_Item_BatchListId", "Unit_Item_BatchGroupListId", "bigint", "0", "Unit_Item_BatchListId");
            AlterBD_(ref opp, "Unit_Item_SalesDate", "Item_ProcessedFlage", "int", "100", "100");


            AlterBD_(ref opp, "Unit_Item_ExpireDate", "BatchNo", "bigint", "0", "0");
            AlterBD_(ref opp, "Unit_Item_ExpireDate", "P_Price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Unit_Item_ExpireDate", "QntyValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Unit_Item_ExpireDate", "IsActive", "int", "0", "1");
            //AlterBD_(ref opp, "Unit_Item_ExpireDate", "Unit_Item_BatchGroupId", "bigint", "0", "1");


            AlterBD_(ref opp, "Unit_Item_BatchList", "BatchGroupId", "uniqueidentifier");

            AlterBD_(ref opp, "Unit_Item_BatchList", "Invoices_ListId", "bigint", "0", "1");
            AlterBD_(ref opp, "Unit_Item_BatchList", "GroupName", "nvarchar(MAX)");
            AlterBD_(ref opp, "Unit_Item_BatchList", "User_Id", "bigint", "0", "1");
            AlterBD_(ref opp, "Unit_Item_BatchList", "Branch_Id", "bigint", "0", "1");
            AlterBD_(ref opp, "Unit_Item_BatchList", "CreatedDate", "datetime");

            AlterBD_(ref opp, "Unit_Item_BatchList", "WahbaValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Unit_Item_BatchList", "NawlounValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Unit_Item_BatchList", "Total3MolaRate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Unit_Item_BatchList", "Total3Mola", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Unit_Item_BatchList", "Value_3Mola", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Unit_Item_BatchList", "IsActive", "int", "1", "1");

            ChangeTableName(ref opp, "Unit_Item_BatchList", "Unit_Item_BatchGroupLists");

            AlterBD_(ref opp, "unit", "SS_unit_Value", "int", "1", "1");
            AlterBD_(ref opp, "Customer", "Street", "nvarchar(200)");
            AlterBD_(ref opp, "Customer", "BuildingNumber", "nvarchar(200)");
            AlterBD_(ref opp, "Customer", "floor", "nvarchar(200)");
            AlterBD_(ref opp, "Customer", "room", "nvarchar(200)");
            AlterBD_(ref opp, "Customer", "landmark", "nvarchar(200)");
            AlterBD_(ref opp, "Customer", "additionalInformation", "nvarchar(200)");
            AlterBD_(ref opp, "Customer", "TaxCompanyName", "nvarchar(500)");
            AlterBD_(ref opp, "Customer", "LookUp_PersonCompany_TypeCodeTax", "nvarchar(500)");



            AlterBD_(ref opp, "Bills_Details", "PriceUnit", "decimal(18, " + d + ")", "0", "Larg_Unit_Price");
            AlterBD_(ref opp, "Bills_Details_Reverse", "PriceUnit", "decimal(18, " + d + ")", "0", "Larg_Unit_Price");

            AlterBD_(ref opp, "Bills_Details", "Unit_Item_SalesDateId", "bigint", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Unit_Item_SalesDateId", "bigint", "0", "0");


            AlterBD_(ref opp, "Bills_Details", "Store_Name", "nvarchar(MAX)");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Store_Name", "nvarchar(MAX)");


            AlterBD_(ref opp, "Money_Transaction_Box", "Balance_BeforStr", "nvarchar(MAX)");
            AlterBD_(ref opp, "Money_Transaction_Box", "Balance_AfterStr", "nvarchar(MAX)");

            AlterBD_(ref opp, "Money_Transaction_Box", "CarsWorkOrdersId", "bigint", "0", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "Lookup_CashPaymentMethodId", "bigint", "0", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "Lookup_CashPaymentMethodName", "nvarchar(500)");
            AlterBD_(ref opp, "Money_Transaction_Box", "Order_No", "bigint", "0", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "Bank_Name", "nvarchar(MAX)");
            AlterBD_(ref opp, "Money_Transaction_Box", "BankSubAccount_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "IsDefault_Bank", "int", "0", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "Factor", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Money_Transaction_Box", "CarsId", "bigint", "0", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "CarsWorkOrdersId", "bigint", "0", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "Invoice_No", "bigint", "0", "0");

            AlterBD_(ref opp, "Money_Transaction_Box", "CarNumber", "nvarchar(MAX)");
            AlterBD_(ref opp, "Money_Transaction_Box", "WorkOrderNumber", "bigint", "0", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "Account_Entry_List_ID", "bigint", "0", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "IS_Posted_Flage", "int", "0", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "SubOrder_Money_Id", "bigint", "0", "0");
            //AlterColumn(ref opp, "Money_Transaction_Box", "SubOrder_Mony_Id", "bigint" );
            AlterBD_(ref opp, "Money_Transaction_Box", "TaxInternalId", "nvarchar(MAX)");


            AlterBD_(ref opp, "Money_Transaction_Box", "DataDictionaryProcessesListsId", "bigint", "0", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "Reference_Type_Table_Id", "bigint", "0", "0");


            AlterBD_(ref opp, "Order_Money", "DataDictionaryProcessesListsId", "bigint", "0", "0");
            AlterBD_(ref opp, "Order_Money", "Reference_Type_Table_Id", "bigint", "0", "0");




            AlterBD_(ref opp, "DataDictionary", "MethodName", "nvarchar(MAX)");
            AlterBD_(ref opp, "DataDictionary", "TableName", "nvarchar(1000)");
            //AlterBD_(ref opp, "Customers_Accounts_Mokhtalat", "Customers_AccountsId", "bigint", "0", "0");
            //            WorkOrderNumber bigint  Checked
            //CustomerName    nvarchar(500)   Checked
            //CarsWorkOrdersId    bigint Checked
            //TaxInternalId nvarchar(500)	Checked
            //CarsId  bigint Checked
            //CarNumber nvarchar(500)	Checked

            AlterBD_(ref opp, "Checks_Memory", "Checks_MemoryMainDataListId", "bigint", "0", "0");
            AlterBD_(ref opp, "Checks_Memory", "ProcessNo", "bigint", "0", "0");
            AlterBD_(ref opp, "Checks_MemoryHistory", "HistoryReceipt", "int", "0", "0");
            AlterBD_(ref opp, "Checks_MemoryHistory", "HistoryDate", "datetime");

            AlterBD_(ref opp, "Invoices_Details", "Qnty_FreeStr", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_Details", "Qnty_Free", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "Qnty", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Invoices_Details_Reverse", "Qnty_FreeStr", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Qnty_Free", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Qnty", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Bills_Details", "Qnty_FreeStr", "nvarchar(500)");
            AlterBD_(ref opp, "Bills_Details", "Qnty_Free", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Qnty", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Bills_Details_Reverse", "Qnty_FreeStr", "nvarchar(500)");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Qnty_Free", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Qnty", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Bills_Details", "S_price_LargeUnit", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "S_price_LargeUnit", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Bills_Details", "S_price_SmallUnit", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "S_price_SmallUnit", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Bills_Details", "S_price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "S_price", "decimal(18, " + d + ")", "0", "0");



            AlterBD_(ref opp, "Unit_Item_SalesDate", "Unit3Mola", "decimal(18, " + d + ")", "0", "3");
            AlterBD_(ref opp, "Unit_Item_SalesDate", "Total3Mola", "decimal(18, " + d + ")", "0", " Unit3Mola * Item_No ");

            AlterBD_(ref opp, "Unit_Item_SalesDate", "Total", "decimal(18, " + d + ")", "0", "QntyValue + Total3Mola");
            AlterBD_(ref opp, "Unit_Item_SalesDate", "IsSelect", "bit", "1", "1");



            AlterBD_(ref opp, "Invoices_Details", "Total3Mola", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "Unit3Mola", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Total3Mola", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Unit3Mola", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Bills_Details", "Total3Mola", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Unit3Mola", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Total3Mola", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Unit3Mola", "decimal(18, " + d + ")", "0", "0");



            AlterBD__(ref opp, "Bills_Details", "UnitStr", "nvarchar(500)", @"	  UPDATE       Bills_Details
SET                UnitStr = unit.B_Unit
FROM            Bills_Details INNER JOIN
                         Item ON Bills_Details.Item_Id = Item.id INNER JOIN
                         unit ON Item.Unit_Id = unit.id  WHERE (Bills_Details.UnitStr IS NULL)");
            AlterBD__(ref opp, "Bills_Details_Reverse", "UnitStr", "nvarchar(500)", @"	  UPDATE       Bills_Details_Reverse
SET                UnitStr = unit.B_Unit
FROM            Bills_Details_Reverse INNER JOIN
                         Item ON Bills_Details_Reverse.Item_Id = Item.id INNER JOIN
                         unit ON Item.Unit_Id = unit.id  WHERE (Bills_Details_Reverse.UnitStr IS NULL)");


            AlterBD_(ref opp, "Invoices_Details", "Item_DeletedFlage", "int", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Item_DeletedFlage", "int", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Item_DeletedFlage", "int", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Item_DeletedFlage", "int", "0", "0");

            AlterBD_(ref opp, "Invoices_Details", "Item_ProcessedFlage", "int", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Item_ProcessedFlage", "int", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Item_ProcessedFlage", "int", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Item_ProcessedFlage", "int", "0", "0");




            AlterBD_(ref opp, "Bills_List", "Total_List3Mola", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "Total_List3Mola", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Bills_List", "WahbaValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "WahbaValue", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Bills_List", "NawlounValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "NawlounValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List", "Total3MolaRate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "Total3MolaRate", "decimal(18, " + d + ")", "0", "0");




            AlterBD_(ref opp, "Bills_List", "Value_3Mola", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "Value_3Mola", "decimal(18, " + d + ")", "0", "0");



            AlterBD_(ref opp, "Govern_Area_List", "Gov_Country_ListId", "bigint", "0", "0");

            AlterBD_(ref opp, "Govern_List", "Gov_Country_ListId", "bigint", "0", "0");
            AlterBD_(ref opp, "Govern_List", "Govern_Area_ListId", "bigint", "0", "0");


            AlterBD_(ref opp, "Gov_City_List", "Gov_Country_ListId", "bigint", "0", "0");
            AlterBD_(ref opp, "Gov_City_List", "Govern_Area_ListId", "bigint", "0", "0");
            AlterBD_(ref opp, "Gov_City_List", "Govern_ListId", "bigint", "0", "0");
            AlterBD_(ref opp, "Order_Money", "CarsWorkOrdersId", "bigint", "0", "0");
            AlterBD_(ref opp, "Order_Money", "MainOrder_MoneyId", "bigint", "0", "0");
            AlterBD_(ref opp, "Order_Money", "Reverse_Order", "int", "0", "0");
            AlterBD_(ref opp, "Order_Money", "Account_Type", "int", "0", "0");
            AlterBD_(ref opp, "Order_Money", "Account_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Order_Money", "IsMoreTheOneBoxShow", "int", "0", "0");


            AlterBD_(ref opp, "Order_Money", "RequirementValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Order_Money", "StillValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Order_Money", "paidTotal", "decimal(18, " + d + ")", "0", "0");




            AlterBD_(ref opp, "Customer", "CompanyName", "nvarchar(200)");
            AlterBD_(ref opp, "Customer", "CompanyContactName", "nvarchar(200)");
            AlterBD_(ref opp, "Customer", "Gov_Country_ListName", "nvarchar(200)");
            AlterBD_(ref opp, "Customer", "Govern_Area_ListName", "nvarchar(200)");
            AlterBD_(ref opp, "Customer", "Govern_ListName", "nvarchar(200)");
            AlterBD_(ref opp, "Customer", "Gov_City_ListName", "nvarchar(200)");
            AlterBD_(ref opp, "Customer", "Street", "nvarchar(200)");
            AlterBD_(ref opp, "Customer", "BuildingNumber", "nvarchar(200)");



            AlterBD_(ref opp, "Customer", "CompanyContactData", "nvarchar(200)");
            AlterBD_(ref opp, "Customer", "NationalId", "nvarchar(200)");
            AlterBD_(ref opp, "Customer", "PassportNo", "nvarchar(200)");
            AlterBD_(ref opp, "Customer", "PostalNo", "nvarchar(200)");
            AlterBD_(ref opp, "Customer", "LookUp_PersonCompany_TypeId", "bigInt");
            AlterBD_(ref opp, "Customer", "LookUp_PersonCompany_Type", "nvarchar(200)");

            AlterBD_(ref opp, "Customer", "RegistrationDate", "datetime", "getdate()", "null");

            AlterBD_(ref opp, "Customer", "gov_Country_ListId", "bigint");
            AlterBD_(ref opp, "Customer", "Govern_Area_ListId", "bigint");
            AlterBD_(ref opp, "Customer", "govern_ListId", "bigint");
            AlterBD_(ref opp, "Customer", "Gov_City_ListId", "bigint");
            AlterBD_(ref opp, "Currency", "Tax_BranchesId", "bigint", "0", "0");
            AlterBD_(ref opp, "Customer", "Tax_BranchesId", "bigint");
            AlterBD_(ref opp, "Customer", "IsInsuranceCompany", "int", "0", "0");



            AlterBD_(ref opp, "Supplier", "CompanyName", "nvarchar(200)");
            AlterBD_(ref opp, "Supplier", "CompanyContactName", "nvarchar(200)");


            AlterBD_(ref opp, "Supplier", "CompanyContactData", "nvarchar(200)");
            AlterBD_(ref opp, "Supplier", "NationalId", "nvarchar(200)");
            AlterBD_(ref opp, "Supplier", "PassportNo", "nvarchar(200)");
            AlterBD_(ref opp, "Supplier", "PostalNo", "nvarchar(200)");
            //AlterBD_(ref opp, "Supplier", "PersonCompanyType", "bigInt");
            AlterBD_(ref opp, "Supplier", "RegistrationDate", "datetime", "getdate()", "null");





            AlterBD_(ref opp, "Supplier", "LookUp_PersonCompany_TypeId", "bigInt");
            AlterBD_(ref opp, "Supplier", "LookUp_PersonCompany_Type", "nvarchar(200)");


            AlterBD_(ref opp, "Account_Entry_List", "CarsWorkOrdersId", "bigint");
            AlterBD_(ref opp, "Account_Entry_List", "CarsId", "bigint");
            AlterBD_(ref opp, "Account_Entry_List", "WorkOrderNumber", "bigint");
            AlterBD_(ref opp, "Account_Entry_List", "CarNumber", "nvarchar(500)");

            AlterBD_(ref opp, "Account_Entry_List", "IsAutomaticEntryOrManualy", "bigint");


            AlterBD_(ref opp, "Account_Entry_Details", "DocType", "nvarchar(500)");

            AlterBD_(ref opp, "Account_Entry_Details", "DocNo", "bigint");
            AlterBD_(ref opp, "Account_Entry_Details", "CarsWorkOrdersId", "bigint");
            AlterBD_(ref opp, "Account_Entry_Details", "CarsId", "bigint");
            AlterBD_(ref opp, "Account_Entry_Details", "WorkOrderNumber", "bigint");
            AlterBD_(ref opp, "Account_Entry_Details", "CarNumber", "nvarchar(500)");


            AlterBD_(ref opp, "Account_Entry_Details", "DataDictionaryProcessesListsId", "bigint");
            AlterBD_(ref opp, "Account_Entry_Details", "Reference_Type_Table_Id", "bigint");
            AlterBD_(ref opp, "Account_Entry_Details", "MainAccount_Id", "bigint", "0", "0", "UPDATE       Account_Entry_Details SET                MainAccount_Id = Submain_Account.Main_Account_Id FROM            Account_Entry_Details INNER JOIN                          Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id");





            //AlterBD_(ref opp, "Customers_Accounts_Mokhtalat", "Customers_AccountsId", "bigint", "0", "0");



            AlterBD_(ref opp, "Checks_Memory", "Transfer_Check_Id_TO", "bigint", "0", "0");
            AlterBD_(ref opp, "Checks_Memory", "Transfer_Check_Id_From", "bigint", "0", "0");


            AlterBD_(ref opp, "Submain_Account", "AccountCode1", "int", "0", "0");
            AlterBD_(ref opp, "Submain_Account", "AccountCode2", "int", "0", "0");
            AlterBD_(ref opp, "Submain_Account", "AccountCode3", "int", "0", "0");
            AlterBD_(ref opp, "Submain_Account", "AccountCode4", "int", "0", "0");
            AlterBD_(ref opp, "Submain_Account", "AccountCode5", "int", "0", "0");
            AlterBD_(ref opp, "Submain_Account", "AccountCode6", "int", "0", "0");
            AlterBD_(ref opp, "Submain_Account", "Is_MasrofatAffect", "int", "1", "1");
            AlterBD_(ref opp, "Submain_Account", "Accept_EntryNew_Flage", "int", "1", "1");
            AlterBD_(ref opp, "Submain_Account", "Cost_Center_Tree_ID", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Submain_Account", "IS_SubAccount_FollowingAccount", "int", "0");
            AlterBD_(ref opp, "Submain_Account", "IS_SubAccount_AccountType", "int", "0", "0");
            AlterBD_(ref opp, "Submain_Account", "FirstSubCode", "int", "0", "0");
            AlterBD_(ref opp, "Submain_Account", "code", "int", "0", "0");
            AlterBD_(ref opp, "Submain_Account", "Is_Last_Level", "int", "0", "0");
            AlterBD_(ref opp, "Submain_Account", "CashFlowType", "int", "0", "0");
            AlterBD_(ref opp, "Submain_Account", "User_Id", "bigint", "0", "0");


            //////////////
            ///




            AlterColumn(ref opp, "FixedAssetData", "Branch_Id", "bigint");
            AlterColumn(ref opp, "Submain_Account", "Branch_Id", "bigint");
            AlterColumn(ref opp, "SysPrinterSettings", "Id", "bigint");

            // MenuViewModel
            //AlterColumn(ref opp, "MenuViewModel", "Id", "bigint");
            //AlterColumn(ref opp, "MenuViewModel", "ParentId", "bigint");
            //AlterColumn(ref opp, "MenuViewModel", "Menu_ID", "bigint");

            // ConsPeriodServiceType
            AlterColumn(ref opp, "ConsPeriodServiceType", "Id", "bigint");

            // Project
            AlterColumn(ref opp, "ConsProject", "Id", "bigint");

            // ProjectSpacesGroup
            AlterColumn(ref opp, "ConsProjectSpacesGroup", "Id", "bigint");
            AlterColumn(ref opp, "ConsProjectSpacesGroup", "ProjectId", "bigint");

            // Residentialbuilding
            AlterColumn(ref opp, "ConsResidentialbuilding", "Id", "bigint");
            AlterColumn(ref opp, "ConsResidentialbuilding", "ProjectId", "bigint");

            // Service
            AlterColumn(ref opp, "ConsService", "Id", "bigint");
            AlterColumn(ref opp, "ConsService", "PeriodServiceTypeId", "bigint");

            // ServicesGroupList
            AlterColumn(ref opp, "ConsServicesGroupList", "Id", "bigint");
            AlterColumn(ref opp, "ConsServicesGroupList", "ProjectId", "bigint");

            // UnitType
            AlterColumn(ref opp, "ConsUnitType", "Id", "bigint");

            // WaterCounterType
            AlterColumn(ref opp, "ConsWaterCounterType", "Id", "bigint");

            // Lookup_OrderTypes
            //AlterColumn(ref opp, "Lookup_OrderTypes", "Id", "bigint");

            // Base
            //AlterColumn(ref opp, "Base", "Id", "bigint");
            //AlterColumn(ref opp, "Base", "UserId", "bigint");


            ///////////////////////
            AlterColumn(ref opp, "Item", "Code", "nvarchar(MAX)");
            AlterColumn(ref opp, "Unit_Item", "Code", "nvarchar(MAX)");
            AlterColumn(ref opp, "Item", "BarCode2", "nvarchar(MAX)");
            AlterColumn(ref opp, "Item", "BarCode3", "nvarchar(MAX)");
            AlterColumn(ref opp, "Item", "BarCode4", "nvarchar(MAX)");
            AlterColumn(ref opp, "Item", "BarCode5", "nvarchar(MAX)");


            AlterColumn(ref opp, "Item", "Unit_Id", "bigint");
            AlterColumn(ref opp, "Item", "Item_Category_ID", "bigint");
            AlterColumn(ref opp, "Order_Purshing", "Order_Type", "int");

            AlterColumn(ref opp, "Order_Purshing", "Id_From", "bigint");
            AlterColumn(ref opp, "Order_Purshing", "Id_To", "bigint");
            AlterColumn(ref opp, "Order_Purshing", "Cust_Supplier_id", "bigint");
            AlterColumn(ref opp, "Order_Purshing", "IS_EstlamRawMaterial", "bit");
            AlterColumn(ref opp, "Order_Purshing", "Invoice_ID", "bigint");
            AlterColumn(ref opp, "Detailes_Purshing", "qua", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Detailes_Purshing", "Part_qua", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Detailes_Purshing", "Item_No", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Detailes_Purshing", "Qua_Befor", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Detailes_Purshing", "Part_qua_Befor", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Detailes_Purshing", "Item_No_Befor", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Detailes_Purshing", "Qua_After", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Detailes_Purshing", "Part_qua_After", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Detailes_Purshing", "Item_No_After", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Detailes_Purshing", "Unit_Used_for_Price", "int");


            AlterColumn(ref opp, "Detailes_Purshing_Import_Export", "qua", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Detailes_Purshing_Import_Export", "Part_qua", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Detailes_Purshing_Import_Export", "Item_No", "decimal(18, " + d + ")");



            AlterColumn(ref opp, "Invoices_List_Reverse", "IsMaintainceInvoice", "int");
            AlterColumn(ref opp, "Invoices_List_Reverse", "Store_Id", "bigint");
            AlterColumn(ref opp, "Invoices_List_Reverse", "Bills_Details_Id", "bigint");





            AlterColumn(ref opp, "Supplier", "Group_Id", "bigint");

            AlterColumn(ref opp, "Customer", "Group_Id", "bigint");
            AlterColumn(ref opp, "Customer", "CustomerTypeTax", "bigint");
            AlterColumn(ref opp, "Customer", "LookUp_CustomerTypeTaxId", "int");
            AlterColumn(ref opp, "Customer", "CountryId", "int");
            AlterColumn(ref opp, "Customer", "CountryName", "nvarchar(200)");
            AlterColumn(ref opp, "Customer", "floor", "nvarchar(200)");
            AlterColumn(ref opp, "Customer", "room", "nvarchar(200)");
            AlterColumn(ref opp, "Customer", "landmark", "nvarchar(200)");
            AlterColumn(ref opp, "Customer", "additionalInformation", "nvarchar(200)");
            AlterColumn(ref opp, "Customer", "Id", "bigint");
            AlterColumn(ref opp, "Customer", "SubAccount_Id", "bigint");
            AlterColumn(ref opp, "Customer", "Price_Type_List_ID", "bigint");
            AlterColumn(ref opp, "Customer", "gov_Country_ListId", "bigint");
            AlterColumn(ref opp, "Customer", "Govern_Area_ListId", "bigint");
            AlterColumn(ref opp, "Customer", "govern_ListId", "bigint");
            AlterColumn(ref opp, "Customer", "Gov_City_ListId", "bigint");
            AlterColumn(ref opp, "Customer", "Ref_Cust_Supplier_Id", "bigint");

            AlterColumn(ref opp, "Customer", "Tax_BranchesId", "bigint");
            AlterColumn(ref opp, "Customer", "Location_Id", "bigint");
            

            AlterColumn(ref opp, "Customer", "Balance", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Customer", "Balance_Actual", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Customer", "Balance_First", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Customer", "Balance_Actual", "decimal(18, " + d + ")");

            AlterColumn(ref opp, "Invoices_Details", "Bills_Details_Id", "bigint");




            AlterColumn(ref opp, "Bills_Details", "Store_Id", "bigint");

            AlterColumn(ref opp, "Bills_Details_Reverse", "Store_Id", "bigint");


            AlterColumn(ref opp, "Invoices_Details", "Store_Id", "bigint");
            AlterColumn(ref opp, "Invoices_Details_Reverse", "Store_Id", "bigint");
            AlterColumn(ref opp, "Bills_Details", "Unit_Item_Table_Id", "bigint");
            AlterColumn(ref opp, "Bills_Details_Reverse", "Unit_Item_Table_Id", "bigint");


            AlterColumn(ref opp, "Invoices_Details", "Item_Notes", "nvarchar(MAX)");
            AlterColumn(ref opp, "Bills_Details", "Item_Notes", "nvarchar(MAX)");
            AlterColumn(ref opp, "Invoices_Details", "Notes", "nvarchar(MAX)");
            AlterColumn(ref opp, "Bills_Details", "Notes", "nvarchar(MAX)");


            AlterColumn(ref opp, "Money_Transaction_Box", "Lookup_CashPaymentMethodName", "nvarchar(500)");
            AlterColumn(ref opp, "Submain_Account", "code", "bigint");




            AlterColumn(ref opp, "Submain_Account", "Cost_Center_Tree_ID", "bigint");
            AlterColumn(ref opp, "Submain_Account", "AccountCode1", "bigint");
            AlterColumn(ref opp, "Submain_Account", "AccountCode2", "bigint");
            AlterColumn(ref opp, "Submain_Account", "AccountCode3", "bigint");
            AlterColumn(ref opp, "Submain_Account", "AccountCode4", "bigint");
            AlterColumn(ref opp, "Submain_Account", "AccountCode5", "bigint");
            AlterColumn(ref opp, "Submain_Account", "AccountCode6", "bigint");

            AlterColumn(ref opp, "Submain_Account", "Account_Customer_Type_ID", "bigint");
            AlterColumn(ref opp, "Submain_Account", "Account_Type_ValueGeneralCustomer", "int", "0", "0");


            AlterColumn(ref opp, "Item", "CommisionPurchase", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Item", "IsPrintBarCode", "int", "1", "1");
            AlterColumn(ref opp, "Item", "IsStopBuying", "int", "1", "1");
            AlterColumn(ref opp, "Item", "StorageTempratureDegree", "decimal(18, " + d + ")");

            AlterColumn(ref opp, "Order_Money", "Order_No", "bigint");
            AlterColumn(ref opp, "Order_Money", "Bank_ID", "bigint");
            AlterColumn(ref opp, "Order_Money", "User_Id", "bigint");
            AlterColumn(ref opp, "Order_Money", "Branch_Id", "bigint");
            AlterColumn(ref opp, "Order_Money", "Year_Financial_Period_Id", "bigint");
            AlterColumn(ref opp, "Order_Money", "Ticket_Supplier_Id", "bigint");
            AlterColumn(ref opp, "Order_Money", "Edit_User_Id", "bigint");



            AlterColumn(ref opp, "Bills_Details", "Unit_Id", "bigint");
            AlterColumn(ref opp, "Bills_Details_Reverse", "Unit_Id", "bigint");

            AlterColumn(ref opp, "Invoices_Details", "Unit_Id", "bigint");

            AlterColumn(ref opp, "Invoices_Details_Reverse", "Unit_Id", "bigint");
            AlterColumn(ref opp, "Invoices_Details_Reverse", "Bills_Details_Id", "bigint");
            AlterColumn(ref opp, "Invoices_Details_Reverse", "Store_Id", "bigint");



            AlterColumn(ref opp, "Invoices_List", "Order_No", "bigint");
            AlterColumn(ref opp, "Invoices_List", "ID", "bigint");
            AlterColumn(ref opp, "Invoices_List", "Invoice_Id", "bigint");
            AlterColumn(ref opp, "Invoices_List", "CarsWorkOrdersId", "bigint");
            AlterColumn(ref opp, "Invoices_List", "DayReg_List_Id", "bigint");
            AlterColumn(ref opp, "Invoices_List", "Account_Entry_List_ID", "bigint");
            AlterColumn(ref opp, "Invoices_List", "Order_MoneyId", "bigint");



            AlterColumn(ref opp, "Invoices_List", "Total", "decimal(18, " + d + ")");
            //AlterColumn(ref opp, "Invoices_List", "commission", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_List", "TaxSales10", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_List", "ATS05", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_List", "TaxSales10_Value", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_List", "ATS05_Value", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_List", "commissionValue", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_List", "CustomerAccountBefor", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_List", "Net_Total", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_List", "CustomerAccountAfter", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_List", "Net_Total", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_List", "commissionValue", "decimal(18, " + d + ")");

            ///////////////////////
            ///


            AlterColumn(ref opp, "Invoices_List_Reverse", "ID", "bigint");
            AlterColumn(ref opp, "Invoices_List_Reverse", "Invoice_Id", "bigint");
            AlterColumn(ref opp, "Invoices_List_Reverse", "DayReg_List_Id", "bigint");
            AlterColumn(ref opp, "Invoices_List_Reverse", "Account_Entry_List_ID", "bigint");
            AlterColumn(ref opp, "Invoices_List_Reverse", "Order_MoneyId", "bigint");


            AlterColumn(ref opp, "Invoices_List_Reverse", "Total", "decimal(18, " + d + ")");
            //AlterColumn(ref opp, "Invoices_List_Reverse", "commission", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_List_Reverse", "TaxSales10", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_List_Reverse", "ATS05", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_List_Reverse", "TaxSales10_Value", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_List_Reverse", "ATS05_Value", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_List_Reverse", "commissionValue", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_List_Reverse", "CustomerAccountBefor", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_List_Reverse", "Net_Total", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_List_Reverse", "CustomerAccountAfter", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_List_Reverse", "Net_Total", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_List_Reverse", "commissionValue", "decimal(18, " + d + ")");
            ///////////////////
            ///

            AlterColumn(ref opp, "Invoices_List", "User_Id", "bigint");
            AlterColumn(ref opp, "Invoices_List", "Branch_Id", "bigint");
            AlterColumn(ref opp, "Invoices_List", "Salesman_Id", "bigint");
            AlterColumn(ref opp, "Invoices_List", "Edit_User_Id", "bigint");
            AlterColumn(ref opp, "Invoices_List", "BanksId", "bigint");
            AlterColumn(ref opp, "Invoices_List", "Emp_Id", "bigint");
            AlterColumn(ref opp, "Invoices_List", "License_Id", "bigint");
            AlterColumn(ref opp, "Invoices_List", "Currency_id", "bigint");

            AlterColumn(ref opp, "Invoices_List_Reverse", "User_Id", "bigint");
            AlterColumn(ref opp, "Invoices_List_Reverse", "Branch_Id", "bigint");
            AlterColumn(ref opp, "Invoices_List_Reverse", "Salesman_Id", "bigint");
            AlterColumn(ref opp, "Invoices_List_Reverse", "Edit_User_Id", "bigint");
            AlterColumn(ref opp, "Invoices_List_Reverse", "BanksId", "bigint");
            AlterColumn(ref opp, "Invoices_List_Reverse", "Emp_Id", "bigint");
            AlterColumn(ref opp, "Invoices_List_Reverse", "License_Id", "bigint");
            AlterColumn(ref opp, "Invoices_List_Reverse", "Currency_id", "bigint");


            AlterColumn(ref opp, "Bills_List", "User_Id", "bigint");
            AlterColumn(ref opp, "Bills_List", "Branch_Id", "bigint");
            AlterColumn(ref opp, "Bills_List", "Salesman_Id", "bigint");
            AlterColumn(ref opp, "Bills_List", "Edit_User_Id", "bigint");
            AlterColumn(ref opp, "Bills_List", "BanksId", "bigint");
            AlterColumn(ref opp, "Bills_List", "Emp_Id", "bigint");
            AlterColumn(ref opp, "Bills_List", "License_Id", "bigint");
            AlterColumn(ref opp, "Bills_List", "Currency_id", "bigint");


            AlterColumn(ref opp, "Bills_List_Reverse", "User_Id", "bigint");
            AlterColumn(ref opp, "Bills_List_Reverse", "Branch_Id", "bigint");
            AlterColumn(ref opp, "Bills_List_Reverse", "Salesman_Id", "bigint");
            AlterColumn(ref opp, "Bills_List_Reverse", "Edit_User_Id", "bigint");
            AlterColumn(ref opp, "Bills_List_Reverse", "BanksId", "bigint");
            AlterColumn(ref opp, "Bills_List_Reverse", "Emp_Id", "bigint");
            AlterColumn(ref opp, "Bills_List_Reverse", "License_Id", "bigint");
            AlterColumn(ref opp, "Bills_List_Reverse", "Currency_id", "bigint");



            AlterColumn(ref opp, "Store", "User_Id", "bigint");
            AlterColumn(ref opp, "Store", "Branch_Id", "bigint");
            AlterColumn(ref opp, "Store", "Edit_User_Id", "bigint");
            AlterColumn(ref opp, "Store", "Job_id", "bigint");
            AlterColumn(ref opp, "Store", "LookUp_StoreOrderTypesId", "bigint");



            AlterColumn(ref opp, "Banks_Balance", "User_Id", "bigint");
            AlterColumn(ref opp, "Banks_Balance", "Branch_Id", "bigint");
            AlterColumn(ref opp, "Banks_Balance", "Bank_Id", "bigint");
            AlterColumn(ref opp, "Banks_Balance", "Edit_User_Id", "bigint");


            AlterColumn(ref opp, "Invoices_Details", "Id", "bigint");
            AlterColumn(ref opp, "Invoices_Details", "Invoice_Id", "bigint");
            AlterColumn(ref opp, "Invoices_Details", "Item_Id", "bigint");
            AlterColumn(ref opp, "Invoices_Details", "Unit_Item_Table_Id", "bigint");

            AlterColumn(ref opp, "Invoices_Details", "Larg_Qnty", "decimal(18, " + d + ")");

            AlterColumn(ref opp, "Invoices_Details", "Small_Qnty", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_Details", "Qnty_Another", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_Details", "Larg_Unit_Price", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_Details", "Small_Unit_Price", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_Details", "Qnty_value", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_Details", "ATS05", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_Details", "TaxSales10", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_Details", "ATS05_Value", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_Details", "TaxSales10_Value", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_Details", "Total", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_Details", "commissionValue", "decimal(18, " + d + ")");
            /////////
            ///
            AlterColumn(ref opp, "Invoices_Details_Reverse", "Id", "bigint");
            AlterColumn(ref opp, "Invoices_Details_Reverse", "Invoice_Id", "bigint");
            AlterColumn(ref opp, "Invoices_Details_Reverse", "Item_Id", "bigint");
            AlterColumn(ref opp, "Invoices_Details_Reverse", "Unit_Item_Table_Id", "bigint");

            AlterColumn(ref opp, "Invoices_Details_Reverse", "Larg_Qnty", "decimal(18, " + d + ")");

            AlterColumn(ref opp, "Invoices_Details_Reverse", "Small_Qnty", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_Details_Reverse", "Qnty_Another", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_Details_Reverse", "Larg_Unit_Price", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_Details_Reverse", "Small_Unit_Price", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_Details_Reverse", "Qnty_value", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_Details_Reverse", "ATS05", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_Details_Reverse", "TaxSales10", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_Details_Reverse", "ATS05_Value", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_Details_Reverse", "TaxSales10_Value", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_Details_Reverse", "Total", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Invoices_Details_Reverse", "commissionValue", "decimal(18, " + d + ")");


       
                 AlterColumn(ref opp, "Supplier", "Ref_Cust_Supplier_Id", "bigint");
            AlterColumn(ref opp, "Supplier", "Tax_BranchesId", "bigint");
            AlterColumn(ref opp, "Account_BranchesimagesLogo", "Id", "bigint");
            AlterColumn(ref opp, "ConsService", "Id", "bigint");
            AlterColumn(ref opp, "Department", "Id", "bigint");



            ////////////


            AlterColumn(ref opp, "item", "id", "bigint");
            AlterColumn(ref opp, "item", "S_id", "bigint");

            AlterColumn(ref opp, "Item", "ServiceDurationInMinutes", "int");

            AlterColumn(ref opp, "Checks_Memory", "id", "bigint");
            AlterColumn(ref opp, "Unit_item", "id", "bigint");
            AlterColumn(ref opp, "Unit_item", "Item_id", "bigint");

            AlterColumn(ref opp, "Unit_item", "Store_id", "bigint");
            AlterColumn(ref opp, "Unit_item", "unit_id", "bigint");





            AlterColumn(ref opp, "Checks_Memory", "Order_Mony_Id", "bigint");
            AlterColumn(ref opp, "Checks_Memory", "Bank_id", "bigint");
            AlterColumn(ref opp, "Checks_Memory", "Customer_Id", "bigint");
            AlterColumn(ref opp, "Checks_Memory", "Supplier_Id", "bigint");
            AlterColumn(ref opp, "Checks_Memory", "Account_Id", "bigint");
            AlterColumn(ref opp, "Checks_Memory", "SubAccount_Id", "bigint");
            AlterColumn(ref opp, "Checks_Memory", "Balance_BeginingPeriod", "decimal(18, " + d + ")");

            AlterColumn(ref opp, "Checks_Memory", "Import_Value", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Checks_Memory", "Export_Value", "decimal(18, " + d + ")");





            AlterColumn(ref opp, "Checks_Memory", "Id", "bigint");
            AlterColumn(ref opp, "Supplier", "Id", "bigint");
            AlterColumn(ref opp, "Supplier", "SubAccount_Id", "bigint");

           


            AlterColumn(ref opp, "Customers_Accounts", "Id", "bigint");
            AlterColumn(ref opp, "Customers_Accounts", "Customer_Id", "bigint");
            AlterColumn(ref opp, "Customers_Accounts", "Reference_Type_Table_Id", "bigint");
            AlterColumn(ref opp, "Customers_Accounts", "DayReg_List_Id", "bigint");
            AlterColumn(ref opp, "Customers_Accounts", "Item_Id", "bigint");
            AlterColumn(ref opp, "Customers_Accounts", "Reference_Id", "bigint");
            AlterColumn(ref opp, "Customers_Accounts", "ProcessNo", "bigint");


            AlterColumn(ref opp, "Suppliers_Accounts", "Id", "bigint");
            AlterColumn(ref opp, "Suppliers_Accounts", "Reference_Type_Table_Id", "bigint");
            AlterColumn(ref opp, "Suppliers_Accounts", "Reference_Id", "bigint");
            AlterColumn(ref opp, "Suppliers_Accounts", "DayReg_List_Id", "bigint");
            AlterColumn(ref opp, "Suppliers_Accounts", "Item_Id", "bigint");
            AlterColumn(ref opp, "Suppliers_Accounts", "Supplier_Id", "bigint");
            AlterColumn(ref opp, "Suppliers_Accounts", "ProcessNo", "bigint");

            AlterColumn(ref opp, "Account_Entry_Details", "Id", "bigint");
            AlterColumn(ref opp, "Account_Entry_Details", "Account_Entry_List_ID", "bigint");
            AlterColumn(ref opp, "Account_Entry_Details", "CostCenter_Id", "bigint");
            AlterColumn(ref opp, "Account_Entry_Details", "Submain_Account_ID", "bigint");
            AlterColumn(ref opp, "Account_Entry_Details", "Account_Code", "bigint");
            AlterColumn(ref opp, "Account_Entry_Details", "Transaction_Value", "decimal(18,3)");
            AlterColumn(ref opp, "Account_Entry_Details", "Equivalent_Transaction_Value", "decimal(18,3)");

            AlterColumn(ref opp, "Account_Entry_List", "Id", "bigint");
            AlterColumn(ref opp, "Account_Entry_List", "Entry_No", "bigint");
            AlterColumn(ref opp, "Account_Entry_List", "Order_Money_Id", "bigint");
            AlterColumn(ref opp, "Account_Entry_List", "Reference_Type_Table_Id", "bigint");

            AlterColumn(ref opp, "Supplier", "Gov_Country_ListId", "bigint");

            AlterColumn(ref opp, "Supplier", "Gov_City_ListId", "bigint");

            AlterColumn(ref opp, "Supplier", "Govern_Area_ListId", "bigint");
            AlterColumn(ref opp, "Supplier", "Govern_ListId", "bigint");
            AlterColumn(ref opp, "Banks", "User_Id", "bigint");

            AlterColumn(ref opp, "Banks", "User_Id", "bigint");
            AlterColumn(ref opp, "Banks", "Branch_Id", "bigint");
            AlterColumn(ref opp, "Banks", "Account_Branches_ID", "bigint");
            AlterColumn(ref opp, "Banks", "Account_Branches_ID", "bigint");
            AlterColumnChangeName(ref opp, "Banks", "Account_Branches_ID", "Branch_Id", "bigint", "0");

            AlterColumn(ref opp, "SysConfigMenusPERMATIONS", "MenuId", "bigint");
            AlterColumn(ref opp, "SysConfigMenusPERMATIONS", "ParentId", "bigint");
            AlterColumn(ref opp, "SysConfigMenusPERMATIONS", "UserId", "bigint");
            AlterColumn(ref opp, "SysConfigMenus", "Id", "bigint");
            AlterColumn(ref opp, "SysConfigMenus", "Parent_Id", "bigint");
            AlterColumn(ref opp, "USER_DETAILES_Branches_Permission", "Id", "bigint");
            AlterColumn(ref opp, "Total_of_Nazlla", "Id", "bigint");
            AlterColumn(ref opp, "Textile_Item_Dept_Balance", "Id", "bigint");
            AlterColumn(ref opp, "Matwa_Items", "Id", "bigint");
            AlterColumn(ref opp, "Maintaining_Item_List", "Id", "bigint");
            AlterColumn(ref opp, "LookUp_StoreOrderTypes", "Id", "bigint");
            AlterColumn(ref opp, "Item_Images", "Id", "bigint");
            AlterColumn(ref opp, "ConsService", "Id", "bigint");
            AlterColumn(ref opp, "Account_BranchesimagesLogo", "Id", "bigint");
            AlterColumn(ref opp, "Department", "Id", "bigint");





            AlterColumnAddPrimaryKey(ref opp, "Customer_Supplier_Group", "Id");
            AlterColumnAddPrimaryKey(ref opp, "Items_Group", "Id");
            AlterColumnAddPrimaryKey(ref opp, "Items_Types_Name", "Id");
            AlterColumnAddPrimaryKey(ref opp, "Currency_Factor", "ID");
            AlterColumnAddPrimaryKey(ref opp, "Currency", "ID");
            AlterColumnAddPrimaryKey(ref opp, "Lookup_CarType", "Id");
            AlterColumnAddPrimaryKey(ref opp, "Submain_Account", "SubAccount_Id", "Id");



            AlterBD_dropColumn(ref opp, "Invoices_List", "commission");
            AlterBD_dropColumn(ref opp, "Invoices_List_Reverse", "commission");

            AlterBD_dropColumn(ref opp, "Supplier", "PersonCompanyType");
            AlterBD_dropColumn(ref opp, "FixedAssetData", "Serial");
            AlterBD_dropColumn(ref opp, "Detailes_Receiving", "Serial");
            AlterBD_dropColumn(ref opp, "Order_Money", "Serial");
            AlterBD_dropColumn(ref opp, "Money_Transaction_Box", "Serial");
            AlterBD_dropColumn(ref opp, "Order_Money_Check", "Serial");
            AlterBD_dropColumn(ref opp, "Order_Receiving", "Serial");
            AlterBD_dropColumn(ref opp, "Customers_Accounts_Mokhtalat", "Serial");

            AlterBD_dropColumn(ref opp, "Submain_Account", "Submain_Account_Balance");
            AlterBD_dropColumn(ref opp, "Submain_Account", "destruction_date");
            AlterBD_dropColumn(ref opp, "Submain_Account", "destruction");
            AlterBD_dropColumn(ref opp, "Submain_Account", "Last_destruction");
            AlterBD_dropColumn(ref opp, "Submain_Account", "Import_Value");
            AlterBD_dropColumn(ref opp, "Submain_Account", "Export_Value");
            AlterBD_dropColumn(ref opp, "Submain_Account", "Account_Employee_Type_ID");
            AlterBD_dropColumn(ref opp, "Account_Entry_Details", "Account_Employee_Type_ID");



            AlterBD_dropTable(ref opp, "form_control");
            AlterBD_dropTable(ref opp, "SupplierLS11");
            AlterBD_dropTable(ref opp, "SupplierLS");
            AlterBD_dropTable(ref opp, "CustomerLS");
            AlterBD_dropTable(ref opp, "CustomerLS11");
            AlterBD_dropTable(ref opp, "UsersModulesPermission");
            AlterBD_dropTable(ref opp, "users");
            AlterBD_dropTable(ref opp, "application_form");
            AlterBD_dropTable(ref opp, "application");
            AlterBD_dropTable(ref opp, "Order_Purshing_old");
            AlterBD_dropTable(ref opp, "SysConfigMenusPERMATIONS_");



            
        }

        static void SetDataDictionaryProcessesNo(ref operations opp)
        {
            string str = "";
            try
            {
                str = @$" CREATE TABLE [dbo].[DataDictionaryProcessesNo](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[ProcessNo] [bigint] NULL,
	[TableName] [nvarchar](500) NULL
)  ";
                opp.insert_me(str);

                int Excp = 0;
                long ProcessNo = Common.Make_AccountsProcessNo(ref Excp, ref opp);

                str = @"INSERT INTO DataDictionaryProcessesNo(ProcessNo, TableName) VALUES (" + ProcessNo + ",'')";
                str = ExecuteInsertWithIdentityCheck("DataDictionaryProcessesNo", str);
                opp.insert_me(str);
            }
            catch
            {
            }
        }
        public static void AlterDBLastYear(ref operations opp, int d)
        {

            string str = "";

            try
            {
                UpdateSermekTable(ref opp);

            }
            catch
            {
            }



            CreateTable(ref opp);

            SetKeys(ref opp);
            try
            {
                str = "  alter table SysConfigMenusPERMATIONS add primary key(Id) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "  alter table SysConfigMenus add primary key(Id) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "  alter table SysConfig add primary key(Id) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                opp.insert_me(" ALTER TABLE Item   DROP COLUMN Image; ");
                str = " ALTER TABLE Item ADD Image bit  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "  ALTER TABLE Item   DROP COLUMN jobsTyeID;";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "  ALTER TABLE Employee   DROP COLUMN jobsTyeID;";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "  ALTER TABLE Employee   DROP COLUMN Image;";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                opp.insert_me("  drop  TABLE jobe_list");

            }
            catch
            {
            }
            try
            {
                opp.insert_me("  drop  TABLE job_list");

            }
            catch
            {
            }
            try
            {

                str = @$"  CREATE TABLE [dbo].[jobslist]( [id] [bigint] {IDENTITY} NOT NULL, 
[jobname] [nvarchar](50) NULL, [OperationDate] [datetime] NULL DEFAULT (getdate()), 
[User_Id] [bigint] NULL DEFAULT ((0
[Branch_Id] [bigint] NULL DEFAULT ((0)), [Insert_ServerDateTime] [datetime] NULL DEFAULT (getdate()), [Update_ServerDateTime] [datetime] NULL DEFAULT (getdate()), [EditOperationDate] [datetime] NULL DEFAULT (getdate()), [Edit_User_Id] [bigint] NULL DEFAULT ((0)), [ProcessNo] [bigint] NULL DEFAULT ((0)) ) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Takset_List]( [Id] [bigint] {IDENTITY} NOT NULL, [Customer_Id] [bigint] NULL, [Invoice_List_Id] [bigint] NULL, [Invoices_Details_Id] [bigint] NULL, [Total_Money] [decimal](18, 2) NULL, [First_PaymentMoney] [decimal](18, 2) NULL, [DateOfFirst_PaymentMoney] [datetime] NULL, [DateOfFirst_Takset] [datetime] NULL, [Statements_No] [nvarchar](50) NULL, [Notebooks_No] [nvarchar](50) NULL, [Guarantor_Name] [nvarchar](50) NULL, [Guarantor_Id] [nvarchar](50) NULL, [DateDay_Payment] [int] NULL, [Notes] [nvarchar](500) NULL, [Takset_Number] [int] NULL, [kest_Value_needed] [decimal](18, 2) NULL, [Increased_Values] [decimal](18, 2) NULL, [Total_Takset_Value] [decimal](18, 2) NULL, [DateOfProcess] [datetime] NULL )";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Takset_Details]( [Id] [bigint] {IDENTITY} NOT NULL, [Takset_List_Id] [bigint] NULL, [DateOfDue] [datetime] NULL, [kest_Value_needed] [decimal](18, 2) NULL, [Payed_Value] [decimal](18, 2) NULL, [Rest_Value] [decimal](18, 2) NULL, [DateOfPayment] [datetime] NULL, [Increased_Values] [decimal](18, 2) NULL, [Payment_Type] [int] NULL, [Payment_TypeStr] [nvarchar](50) NULL, [Notes] [nvarchar](500) NULL, [Order_Mony_Id] [bigint] NULL      DEFAULT ((0)), [Payed_ValueSaved] [decimal](18, 2) NULL      DEFAULT ((0)), [Rest_ValueSaved] [decimal](18, 2) NULL      DEFAULT ((0)), [Increased_ValuesSaved] [decimal](18, 2) NULL      DEFAULT ((0)), [Total_Value_Payed] [decimal](18, 2) NULL, [OrderNo] [nvarchar](50) NULL )  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Work_In_Details_Received]( [Id] [bigint] {IDENTITY} NOT NULL, [WorkDate] [datetime] NULL, [Work_In_Details_Id] [bigint] NOT NULL, [Process_No] [bigint] NULL DEFAULT ((0)), [Draw_Id] [bigint] NULL, [Draw_Name] [nvarchar](50) NULL, [Received_Qnty_Larg] [float] NULL, [Received_Qnty_Small] [float] NULL, [Received_Item_NO] [float] NULL, [Price] [float] NULL, [Total_Price] [float] NULL, [Price_Selected_Index] [int] NULL, [Customer_Id] [bigint] NULL, [Unit_Item_Table_Id] [bigint] NULL, [CustomerAccountBefor] [float] NULL, [CustomerAccountAfter] [float] NULL, [Item_Id] [int] NULL DEFAULT ((0)) ) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Work_In_Details_Balance]( [Id] [bigint] {IDENTITY} NOT NULL, [Unit_Item_Table_Id] [bigint] NULL, [Balance_Qnty_Larg] [float] NULL, [Balance_Qnty_Small] [float] NULL, [Balance_Item_NO] [float] NULL, [Customer_Id] [bigint] NULL, [Item_Id] [int] NULL DEFAULT ((0)) )  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Work_In_Details]( [Id] [bigint] {IDENTITY} NOT NULL, [Customer_Id] [int] NULL DEFAULT ((0)), [Request_Date] [datetime] NULL, [Unit_Item_Table_Id] [bigint] NULL DEFAULT ((0)), [Request_Qnty_Larg] [float] NULL DEFAULT ((0)), [Request_Qnty_Small] [float] NULL DEFAULT ((0)), [Request_Item_NO] [float] NULL DEFAULT ((0)), [Export_Qnty_Larg] [float] NULL DEFAULT ((0)), [Export_Qnty_Small] [float] NULL DEFAULT ((0)), [Export_Item_No] [float] NULL DEFAULT ((0)), [Item_Exported_Finish] [int] NULL DEFAULT ((0)), [Notes] [nvarchar](500) NULL, [Item_Id] [int] NULL DEFAULT ((0)) ) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Customers_Destruction]( [Id] [bigint] {IDENTITY} NOT NULL, [Customer_id] [bigint] NULL, [Customer_name] [nvarchar](100) NULL, [small_quantity] [decimal](18, 2) NULL, [large_quantity] [decimal](18, 2) NULL, [price] [decimal](18, 2) NULL, [total] [decimal](18, 2) NULL, [dt_date] [datetime] NULL, [Account_ID] [int] NULL, [Unit_Item_Table_Id] [int] NULL, [Notes] [nvarchar](100) NULL ) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[ItemRawEntagEstlamTemp]( [Item_Name] [nvarchar](200) NULL, [S_Id] [decimal](18, 2) NULL, [Id] [decimal](18, 2) NULL, [MainItem_Id] [decimal](18, 2) NULL, [SubItem_Id] [decimal](18, 2) NULL, [Full_qua] [decimal](18, 2) NULL, [D_Qua] [decimal](18, 2) NULL, [Quantity] [decimal](18, 2) NULL, [MainItem_Name] [nvarchar](200) NULL, [SubItem_Name] [nvarchar](200) NULL, [Big_unit] [nvarchar](200) NULL, [Small_unit] [nvarchar](200) NULL, [Unit_Id] [decimal](18, 2) NULL, [S_unit_Value] [decimal](18, 2) NULL, [Default_Store_Id] [decimal](18, 2) NULL, [Larg_Qnty] [decimal](18, 2) NULL, [Small_Qnty] [decimal](18, 2) NULL, [Qnty] [decimal](18, 2) NULL, [Entry_Larg_Qnty] [nvarchar](200) NULL, [Entry_Small_Qnty] [nvarchar](200) NULL, [Entry_Qnty] [decimal](18, 2) NULL, [Item_Index] [nvarchar](200) NULL, [Store_Name] [nvarchar](200) NULL, [SubFull_qua] [decimal](18, 2) NULL, [SubD_Qua] [decimal](18, 2) NULL, [SubItem_No] [decimal](18, 2) NULL, [SubQntyBalance] [decimal](18, 2) NULL, [CheckBalanceAllows] [decimal](18, 2) NULL ) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[ClinicUsed]( [Name] [nvarchar](255) NULL, [Is_Active] [int] NULL      DEFAULT ((1)), [Id] [bigint] {IDENTITY} NOT NULL,  CONSTRAINT [PK_ClinicUsed_1] PRIMARY KEY CLUSTERED  ( [Id] ASC )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] )  ";
                opp.insert_me(str);
            }
            catch (Exception)
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Tours_Program_Omra_Heg_List]( [Id] [bigint] {IDENTITY} NOT NULL, [Is_Omra_Heg] [int] NULL      DEFAULT ((0)), [Prog_Name] [nvarchar](100) NULL, [Travel_By] [nvarchar](100) NULL, [Ticket_Price] [decimal](18, 2) NULL      DEFAULT ((0)), [Tashera_Price] [decimal](18, 2) NULL      DEFAULT ((0)), [Sakn_Makka] [decimal](18, 2) NULL      DEFAULT ((0)), [Sakn_Madina] [decimal](18, 2) NULL      DEFAULT ((0)), [Transport_Eg] [decimal](18, 2) NULL      DEFAULT ((0)), [Transport_Sa] [decimal](18, 2) NULL      DEFAULT ((0)), [Eshraf_Price] [decimal](18, 2) NULL      DEFAULT ((0)), [OtherCost] [decimal](18, 2) NULL      DEFAULT ((0)), [Prog_Total] [decimal](18, 2) NULL      DEFAULT ((0)), [Days_Makka] [int] NULL      DEFAULT ((0)), [Days_Madina] [int] NULL      DEFAULT ((0)), [Arrival_Port] [nvarchar](100) NULL      DEFAULT ((0)), [Travel_Port] [nvarchar](100) NULL      DEFAULT ((0)), [Is_Active] [int] NULL      DEFAULT ((0)), [Sell_Price] [decimal](18, 2) NULL )  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Tours_Omra]( [Id] [bigint] {IDENTITY} NOT NULL, [Is_Omra_Heg] [int] NULL      DEFAULT ((0)), [Tours_Program_Omra_Heg_List_Id] [int] NULL, [Passenger_Name] [nvarchar](100) NULL, [Mandop_Name] [nvarchar](100) NULL, [DateOfReserved] [datetime] NULL, [DateOfTravel] [datetime] NULL, [DateOfReturn] [datetime] NULL, [PurchasePrice] [decimal](18, 2) NULL, [TicketPrice] [decimal](18, 2) NULL, [Commission] [decimal](18, 2) NULL, [Supplier_Id] [bigint] NULL, [Customer_Id] [bigint] NULL, [IsTicket_Reverse] [int] NULL      DEFAULT ((0)), [DateOfReverse] [datetime] NULL, [PriceReverse_ToSupplier] [decimal](18, 2) NULL      DEFAULT ((0)), [PriceReverse_FromCustomer] [decimal](18, 2) NULL      DEFAULT ((0)), [ReverseDiffCustomer] [decimal](18, 2) NULL      DEFAULT ((0)), [ReverseDiffSupplier] [decimal](18, 2) NULL      DEFAULT ((0)), [FinalReverseDiff] [decimal](18, 2) NULL      DEFAULT ((0)) )    ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"CREATE TABLE [dbo].[Tours_Ticketing_Land]( [Id] [bigint] {IDENTITY} NOT NULL, [DirectionTotal] [nvarchar](100) NULL, [Emp_Booking] [nvarchar](100) NULL, [Passenger_Name] [nvarchar](100) NULL, [DateOfReserved] [datetime] NULL, [DateOfTravel] [datetime] NULL, [PurchasePrice_Eg] [decimal](18, 2) NULL, [PurchasePrice_Sea] [decimal](18, 2) NULL, [PurchasePrice_Sa] [decimal](18, 2) NULL, [PurchasePrice_Masar] [decimal](18, 2) NULL, [TicketCostOther] [decimal](18, 2) NULL, [PurchasePrice_Total] [decimal](18, 2) NULL, [TicketPrice_Eg] [decimal](18, 2) NULL, [TicketPrice_Sea] [decimal](18, 2) NULL, [TicketPrice_Sa] [decimal](18, 2) NULL, [TicketPriceTotal] [decimal](18, 2) NULL, [Commission] [decimal](18, 2) NULL, [Supplier_Id_Eg] [bigint] NULL, [Supplier_Id_Sea] [bigint] NULL, [Supplier_Id_Sa] [bigint] NULL, [Customer_Id] [bigint] NULL, [IsTicket_Reverse] [int] NULL, [DateOfReverse] [datetime] NULL, [PriceReverse_ToSupplier] [decimal](18, 2) NULL, [PriceReverse_FromCustomer] [decimal](18, 2) NULL, [ReverseDiffCustomer] [decimal](18, 2) NULL, [ReverseDiffSupplier] [decimal](18, 2) NULL, [FinalReverseDiff] [decimal](18, 2) NULL, [Transport_Eg] [nvarchar](100) NULL, [Transport_Sea] [nvarchar](100) NULL, [Transport_Sa] [nvarchar](100) NULL )   ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"CREATE TABLE [dbo].[Tours_Ticketing_Air](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[DirectionFirst] [nvarchar](100) NULL,
	[DirectionSecond] [nvarchar](100) NULL,
	[DirectionThird] [nvarchar](100) NULL,
	[DirectionTotal] [nvarchar](100) NULL,
	[Ticket_No] [nvarchar](100) NULL,
	[Booking_No] [nvarchar](100) NULL,
	[Passenger_Name] [nvarchar](100) NULL,
	[ISOneWayRoundTrip] [int] NULL,
	[Carrier_Company] [nvarchar](100) NULL,
	[DateOfReserved] [datetime] NULL,
	[DateOfTravel] [datetime] NULL,
	[DateOfReturn] [datetime] NULL,
	[PurchasePrice] [decimal](18, 2) NULL,
	[TicketPrice] [decimal](18, 2) NULL,
	[Commission] [decimal](18, 2) NULL,
	[Emp_Booking] [nvarchar](100) NULL,
	[Supplier_Id] [bigint] NULL,
	[Customer_Id] [bigint] NULL,
	[Order_Money_Id] [bigint] NULL,
	[PaidValue] [decimal](18, 2) NULL,
	[RestValue] [decimal](18, 2) NULL,
	[IsTicket_Reverse] [int] NULL DEFAULT ((0)),
	[DateOfReverse] [datetime] NULL,
	[PriceReverse_ToSupplier] [decimal](18, 2) NULL DEFAULT ((0)),
	[PriceReverse_FromCustomer] [decimal](18, 2) NULL,
	[ReverseDiffCustomer] [decimal](18, 2) NULL,
	[ReverseDiffSupplier] [decimal](18, 2) NULL,
	[FinalReverseDiff] [decimal](18, 2) NULL,
	[Emp_Booking_Id] [bigint] NULL DEFAULT ((0)),
	[OperationDate] [datetime] NULL DEFAULT (getdate()),
	[User_Id] [bigint] NULL DEFAULT ((0)),
	[Branch_Id] [bigint] NULL DEFAULT ((0)),
	[Insert_ServerDateTime] [datetime] NULL  DEFAULT (getdate()) ,
	[Update_ServerDateTime] [datetime] NULL  DEFAULT (getdate()) ,
	[EditOperationDate] [datetime] NULL  DEFAULT (getdate()) ,
	[Edit_User_Id] [bigint] NULL DEFAULT ((0)),
	[ProcessNo] [bigint] NULL DEFAULT ((0)) 
) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Tours_Tasherat]( [Id] [bigint] {IDENTITY} NOT NULL, [Passenger_Name] [nvarchar](100) NULL, [Tasherat_Type] [nvarchar](100) NULL, [DateOfEstlamPassport] [datetime] NULL, [DateOfTaslem] [datetime] NULL, [PurchasePrice] [decimal](18, 2) NULL, [TicketPrice] [decimal](18, 2) NULL, [Commission] [decimal](18, 2) NULL, [Supplier_Id] [bigint] NULL, [Customer_Id] [bigint] NULL, [Order_Money_Id] [bigint] NULL, [IsTicket_Reverse] [int] NULL      DEFAULT ((0)), [DateOfReverse] [datetime] NULL, [PriceReverse_ToSupplier] [decimal](18, 2) NULL      DEFAULT ((0)), [PriceReverse_FromCustomer] [decimal](18, 2) NULL      DEFAULT ((0)), [ReverseDiffCustomer] [decimal](18, 2) NULL      DEFAULT ((0)), [ReverseDiffSupplier] [decimal](18, 2) NULL      DEFAULT ((0)), [FinalReverseDiff] [decimal](18, 2) NULL      DEFAULT ((0)) )  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"  CREATE TABLE [dbo].[Tours_Hotels]( [Id] [bigint] {IDENTITY} NOT NULL, [Passenger_Name] [nvarchar](100) NULL, [DateOfReserved] [datetime] NULL, [Hotel_Name] [nvarchar](100) NULL, [City] [nvarchar](100) NULL, [Country] [nvarchar](100) NULL, [DateOfEntry] [datetime] NULL, [DateOfLeave] [datetime] NULL, [PurchasePrice] [decimal](18, 2) NULL, [TicketPrice] [decimal](18, 2) NULL, [Commission] [decimal](18, 2) NULL, [Supplier_Id] [bigint] NULL, [Customer_Id] [bigint] NULL, [IsTicket_Reverse] [int] NULL DEFAULT ((0)), [DateOfReverse] [datetime] NULL, [PriceReverse_ToSupplier] [decimal](18, 2) NULL DEFAULT ((0)), [PriceReverse_FromCustomer] [decimal](18, 2) NULL DEFAULT ((0)), [ReverseDiffCustomer] [decimal](18, 2) NULL DEFAULT ((0)), [ReverseDiffSupplier] [decimal](18, 2) NULL DEFAULT ((0)), [FinalReverseDiff] [decimal](18, 2) NULL DEFAULT ((0)) )    ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " CREATE TABLE [dbo].[Item_Image_BarCode]( [Item_Id] [bigint] NULL, [ImageBarCode] [image] NULL )";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"   CREATE TABLE [dbo].[Item_BarCodeSetting]( [Id] [bigint] {IDENTITY} NOT NULL, [Item_Id] [nvarchar](50) NULL, [FontHeader] [nvarchar](50) NULL, [FontFooter] [nvarchar](50) NULL, [Medium] [nvarchar](50) NULL, [Center] [nvarchar](50) NULL, [Width] [nvarchar](50) NULL, [Height] [nvarchar](50) NULL, [BarCodeHeight] [nvarchar](50) NULL, [Space_X_Item] [nvarchar](50) NULL, [Space_Y_Item] [nvarchar](50) NULL, [TopMargin] [nvarchar](50) NULL, [LeftMargin] [nvarchar](50) NULL, [paperWidth] [nvarchar](50) NULL, [paperHeight] [nvarchar](50) NULL, [SetPaperSize] [nvarchar](50) NULL, [PaperTopMargin] [nvarchar](50) NULL, [PaperLeftMargin] [nvarchar](50) NULL, [PaperRightMargin] [nvarchar](50) NULL, [PaperBottomMargin] [nvarchar](50) NULL, [OperationDate] [datetime] NULL, [User_Id] [bigint] NULL, [Branch_Id] [bigint] NULL, [Insert_ServerDateTime] [datetime] NULL, [Update_ServerDateTime] [datetime] NULL, [EditOperationDate] [datetime] NULL, [Edit_User_Id] [bigint] NULL, [ProcessNo] [bigint] NULL, [TextUnderCode] [nvarchar](500) NULL )";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"CREATE TABLE [dbo].[Item_BarCodeSettingDefault](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[Item_Id] [nvarchar](50) NULL,
	[FontHeader] [nvarchar](50) NULL,
	[FontFooter] [nvarchar](50) NULL,
	[Medium] [nvarchar](50) NULL,
	[Center] [nvarchar](50) NULL,
	[Width] [nvarchar](50) NULL,
	[Height] [nvarchar](50) NULL,
	[BarCodeHeight] [nvarchar](50) NULL,
	[Space_X_Item] [nvarchar](50) NULL,
	[Space_Y_Item] [nvarchar](50) NULL,
	[TopMargin] [nvarchar](50) NULL,
	[LeftMargin] [nvarchar](50) NULL,
	[paperWidth] [nvarchar](50) NULL,
	[paperHeight] [nvarchar](50) NULL,
	[SetPaperSize] [nvarchar](50) NULL,
	[PaperTopMargin] [nvarchar](50) NULL,
	[PaperLeftMargin] [nvarchar](50) NULL,
	[PaperRightMargin] [nvarchar](50) NULL,
	[PaperBottomMargin] [nvarchar](50) NULL,
	[OperationDate] [datetime] NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[Update_ServerDateTime] [datetime] NULL,
	[EditOperationDate] [datetime] NULL,
	[Edit_User_Id] [bigint] NULL,
	[ProcessNo] [bigint] NULL,
	[TextUnderCode] [nvarchar](500) NULL
) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " DELETE FROM Supplier_Price WHERE (Price_PurchaseType IS NULL)";
                opp.insert_me(str);
            }
            catch
            {
            }



            try
            {
                string mysql = @$"   drop  TABLE [dbo].[LookUp_CheckStatus]

   ";
                opp.insert_me(mysql);





            }
            catch (Exception ee) { }

            try
            {
                string mysql = @$"   
  CREATE TABLE [dbo].[LookUp_CheckStatus](
	[Id] [bigint] NOT NULL  primary key,
	[StatusName] [nvarchar](50) NULL,
	[S_Value] [int] NULL,
	[Status] [int] NULL,
	[OrderBy] [int] NULL      DEFAULT ((1)),
	[IsProcessUsedToChange] [int] NULL      DEFAULT ((0)),
	[OperationDate] [datetime] NULL      DEFAULT (getdate()),
	[User_Id] [bigint] NULL      DEFAULT ((0)),
	[Branch_Id] [bigint] NULL      DEFAULT ((0)),
	[Insert_ServerDateTime] [datetime] NULL      DEFAULT (getdate()),
	[Update_ServerDateTime] [datetime] NULL      DEFAULT (getdate()),
	[EditOperationDate] [datetime] NULL      DEFAULT (getdate()),
	[Edit_User_Id] [bigint] NULL      DEFAULT ((0)),
	[SubAccount_Id] [bigint] NULL      DEFAULT ((0)),
	[ProcessNo] [bigint] NULL      DEFAULT ((0)),
	[Active] [int] NULL      DEFAULT ((1))
) 
";
                opp.insert_me(mysql);
            }
            catch
            {
            }
            try
            {

                string mysql = @$" 
 
 
   INSERT [dbo].[LookUp_CheckStatus] ([Id], [StatusName], [S_Value], [Status], [OrderBy], [IsProcessUsedToChange], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [SubAccount_Id], [ProcessNo], [Active]) VALUES (1, N'كل الشيكات', 0, 0, 0, 0, NULL, 0, 0, NULL, NULL, NULL, 0, 0, -1, 1)
INSERT [dbo].[LookUp_CheckStatus] ([Id], [StatusName], [S_Value], [Status], [OrderBy], [IsProcessUsedToChange], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [SubAccount_Id], [ProcessNo], [Active]) VALUES (2, N'فى حافظة الشيكات الواردة', 1, 1, 1, 1, NULL, 0, 0, NULL, NULL, NULL, 0, 0, -1, 1)
INSERT [dbo].[LookUp_CheckStatus] ([Id], [StatusName], [S_Value], [Status], [OrderBy], [IsProcessUsedToChange], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [SubAccount_Id], [ProcessNo], [Active]) VALUES (3, N'تحت المراجعة', 4, 1, 2, 1, NULL, 0, 0, NULL, NULL, NULL, 0, 0, -1, 1)
INSERT [dbo].[LookUp_CheckStatus] ([Id], [StatusName], [S_Value], [Status], [OrderBy], [IsProcessUsedToChange], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [SubAccount_Id], [ProcessNo], [Active]) VALUES (4, N'مصروفة من البنك', 10, 1, 4, 1, NULL, 0, 0, NULL, NULL, NULL, 0, 0, -1, 1)
INSERT [dbo].[LookUp_CheckStatus] ([Id], [StatusName], [S_Value], [Status], [OrderBy], [IsProcessUsedToChange], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [SubAccount_Id], [ProcessNo], [Active]) VALUES (5, N'مرتجع من البنك', 3, 1, 6, 1, NULL, 0, 0, NULL, NULL, NULL, 0, 0, -1, 1)
INSERT [dbo].[LookUp_CheckStatus] ([Id], [StatusName], [S_Value], [Status], [OrderBy], [IsProcessUsedToChange], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [SubAccount_Id], [ProcessNo], [Active]) VALUES (6, N'شيكات تحت التحصيل', 11, 1, 3, 1, NULL, 0, 0, NULL, NULL, NULL, 0, 0, -1, 1)
INSERT [dbo].[LookUp_CheckStatus] ([Id], [StatusName], [S_Value], [Status], [OrderBy], [IsProcessUsedToChange], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [SubAccount_Id], [ProcessNo], [Active]) VALUES (7, N'شيك مرتجع الى عميل', 12, 1, 8, 1, NULL, 0, 0, NULL, NULL, NULL, 0, 0, -1, 1)
INSERT [dbo].[LookUp_CheckStatus] ([Id], [StatusName], [S_Value], [Status], [OrderBy], [IsProcessUsedToChange], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [SubAccount_Id], [ProcessNo], [Active]) VALUES (8, N'مجيزة أو محولة', 30, 1, 11, 1, NULL, 0, 0, NULL, NULL, NULL, 0, 0, -1, 1)
INSERT [dbo].[LookUp_CheckStatus] ([Id], [StatusName], [S_Value], [Status], [OrderBy], [IsProcessUsedToChange], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [SubAccount_Id], [ProcessNo], [Active]) VALUES (9, N'شيك تحت المراجعة الأن', 31, 2, 13, 1, NULL, 0, 0, NULL, NULL, NULL, 0, 0, -1, 1)
INSERT [dbo].[LookUp_CheckStatus] ([Id], [StatusName], [S_Value], [Status], [OrderBy], [IsProcessUsedToChange], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [SubAccount_Id], [ProcessNo], [Active]) VALUES (10, N'حافظة الشيكات الصادرة', 32, 2, 12, 1, CAST(N'2023-01-15 21:33:50.570' AS DateTime), 0, 0, CAST(N'2023-01-15 21:33:50.570' AS DateTime), CAST(N'2023-01-15 21:33:50.570' AS DateTime), CAST(N'2023-01-15 21:33:50.570' AS DateTime), 0, 0, 0, 1)
INSERT [dbo].[LookUp_CheckStatus] ([Id], [StatusName], [S_Value], [Status], [OrderBy], [IsProcessUsedToChange], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [SubAccount_Id], [ProcessNo], [Active]) VALUES (11, N'شيك صادر تم صرفة من البنك', 13, 2, 16, 1, CAST(N'2023-01-25 04:17:27.843' AS DateTime), 0, 0, CAST(N'2023-01-25 04:17:27.843' AS DateTime), CAST(N'2023-01-25 04:17:27.843' AS DateTime), CAST(N'2023-01-25 04:17:27.843' AS DateTime), 0, 0, 0, 1)
INSERT [dbo].[LookUp_CheckStatus] ([Id], [StatusName], [S_Value], [Status], [OrderBy], [IsProcessUsedToChange], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [SubAccount_Id], [ProcessNo], [Active]) VALUES (12, N'شيك لم يصرف من البنك', 2, 2, 17, 1, CAST(N'2023-02-19 22:11:56.803' AS DateTime), 0, 0, CAST(N'2023-02-19 22:11:56.803' AS DateTime), CAST(N'2023-02-19 22:11:56.803' AS DateTime), CAST(N'2023-02-19 22:11:56.803' AS DateTime), 0, 0, 0, 1)
INSERT [dbo].[LookUp_CheckStatus] ([Id], [StatusName], [S_Value], [Status], [OrderBy], [IsProcessUsedToChange], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [SubAccount_Id], [ProcessNo], [Active]) VALUES (13, N'غير مرتجع إلى العميل', 38, 1, 9, 1, CAST(N'2023-05-21 21:42:40.303' AS DateTime), 0, 0, CAST(N'2023-05-21 21:42:40.303' AS DateTime), CAST(N'2023-05-21 21:42:40.303' AS DateTime), CAST(N'2023-05-21 21:42:40.303' AS DateTime), 0, 0, 0, 1)
INSERT [dbo].[LookUp_CheckStatus] ([Id], [StatusName], [S_Value], [Status], [OrderBy], [IsProcessUsedToChange], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [SubAccount_Id], [ProcessNo], [Active]) VALUES (14, N'مرتجع من المورد', 15, 2, 14, 1, CAST(N'2023-05-21 21:46:19.890' AS DateTime), 0, 0, CAST(N'2023-05-21 21:46:19.890' AS DateTime), CAST(N'2023-05-21 21:46:19.890' AS DateTime), CAST(N'2023-05-21 21:46:19.890' AS DateTime), 0, 0, 0, 1)
INSERT [dbo].[LookUp_CheckStatus] ([Id], [StatusName], [S_Value], [Status], [OrderBy], [IsProcessUsedToChange], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [SubAccount_Id], [ProcessNo], [Active]) VALUES (15, N'غير مرتجع من المورد', 16, 2, 15, 1, CAST(N'2023-05-21 21:46:53.420' AS DateTime), 0, 0, CAST(N'2023-05-21 21:46:53.420' AS DateTime), CAST(N'2023-05-21 21:46:53.420' AS DateTime), CAST(N'2023-05-21 21:46:53.420' AS DateTime), 0, 0, 0, 1)
INSERT [dbo].[LookUp_CheckStatus] ([Id], [StatusName], [S_Value], [Status], [OrderBy], [IsProcessUsedToChange], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [SubAccount_Id], [ProcessNo], [Active]) VALUES (16, N'شيك صادر يحصل على دفعات', 33, 2, 18, 1, CAST(N'2023-06-02 20:02:05.203' AS DateTime), 0, 0, CAST(N'2023-06-02 20:02:05.203' AS DateTime), CAST(N'2023-06-02 20:02:05.203' AS DateTime), CAST(N'2023-06-02 20:02:05.203' AS DateTime), 0, 0, 0, 1)
INSERT [dbo].[LookUp_CheckStatus] ([Id], [StatusName], [S_Value], [Status], [OrderBy], [IsProcessUsedToChange], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [SubAccount_Id], [ProcessNo], [Active]) VALUES (17, N'شيك وارد يحصل على دفعات', 34, 1, 10, 1, CAST(N'2023-06-02 20:02:36.277' AS DateTime), 0, 0, CAST(N'2023-06-02 20:02:36.277' AS DateTime), CAST(N'2023-06-02 20:02:36.277' AS DateTime), CAST(N'2023-06-02 20:02:36.277' AS DateTime), 0, 0, 0, 1)
INSERT [dbo].[LookUp_CheckStatus] ([Id], [StatusName], [S_Value], [Status], [OrderBy], [IsProcessUsedToChange], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [SubAccount_Id], [ProcessNo], [Active]) VALUES (18, N'مصروفة من البنك الى حساب الصندوق', 35, 1, 5, 1, CAST(N'2023-06-05 00:36:02.737' AS DateTime), 0, 0, CAST(N'2023-06-05 00:36:02.737' AS DateTime), CAST(N'2023-06-05 00:36:02.737' AS DateTime), CAST(N'2023-06-05 00:36:02.737' AS DateTime), 0, 0, 0, 1)
INSERT [dbo].[LookUp_CheckStatus] ([Id], [StatusName], [S_Value], [Status], [OrderBy], [IsProcessUsedToChange], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [SubAccount_Id], [ProcessNo], [Active]) VALUES (19, N'شيك لم يصرف من البنك', 36, 1, 7, 1, CAST(N'2023-06-05 00:37:46.247' AS DateTime), 0, 0, CAST(N'2023-06-05 00:37:46.247' AS DateTime), CAST(N'2023-06-05 00:37:46.247' AS DateTime), CAST(N'2023-06-05 00:37:46.247' AS DateTime), 0, 0, 0, 1)
INSERT [dbo].[LookUp_CheckStatus] ([Id], [StatusName], [S_Value], [Status], [OrderBy], [IsProcessUsedToChange], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [SubAccount_Id], [ProcessNo], [Active]) VALUES (20, N'شيك صادر تم استلامة', 37, 1, 19, 1, CAST(N'2023-06-14 00:37:58.010' AS DateTime), 0, 0, CAST(N'2023-06-14 00:37:58.010' AS DateTime), CAST(N'2023-06-14 00:37:58.010' AS DateTime), CAST(N'2023-06-14 00:37:58.010' AS DateTime), 0, 0, 0, 1)


";
                opp.insert_me(mysql);
            }
            catch (Exception ee) { }



            try
            {


            }
            catch
            {
            }
            try
            {
                string mysql = @$"  CREATE TABLE [dbo].[Item_First_Period_Balance](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[Item_Id] [bigint] NULL,
	[Unit_Item_Id] [bigint] NULL,
	[Larg_Qnty] [decimal](18, 2) NULL,
	[Small_Qnty] [decimal](18, 2) NULL,
	[Item_No] [decimal](18, 2) NULL,
	[Balance_Date] [datetime] NULL,
	[Active] [int] NULL,
	[OperationDate] [datetime] NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[Update_ServerDateTime] [datetime] NULL,
	[EditOperationDate] [datetime] NULL,
	[Edit_User_Id] [bigint] NULL,
	[ProcessNo] [bigint] NULL,
	[P_priceAvgCost] [decimal](18, 2) NULL,
	[S_priceAvgCost] [decimal](18, 2) NULL,
	[P_price] [decimal](18, 2) NULL,
	[S_price] [decimal](18, 2) NULL
) ";
                opp.insert_me(mysql);

                mysql = @$" INSERT INTO Item_First_Period_Balance
                         (Item_Id, Unit_Item_Id, Larg_Qnty, Small_Qnty, Item_No, Balance_Date)
SELECT        Unit_item.Item_id, Detailes_Purshing.Unit_item_Table_Id, Detailes_Purshing.qua, Detailes_Purshing.Part_qua, Detailes_Purshing.Item_No, Order_Purshing.Order_date
FROM            Order_Purshing INNER JOIN
                         Detailes_Purshing ON Order_Purshing.Id = Detailes_Purshing.OrderTable_id INNER JOIN
                         Unit_item ON Detailes_Purshing.Unit_item_Table_Id = Unit_item.id
WHERE        (Order_Purshing.Order_Purshing_No = - 1)  ";
                str = ExecuteInsertWithIdentityCheck("Item_First_Period_Balance", str);
                opp.insert_me(mysql);


            }
            catch (Exception ee) { }
            //if (Common.CompanyTitle == Common.CompanyTitle_Client12)
            //{
            //    DateTime time = new DateTime(0x7e1, 8, 0x16);
            //    try
            //    {
            //        if (DateTime.Now.Date < time)
            //        {
            //            opp.insert_me("DELETE FROM Item WHERE        (Active_flage = 1) AND (id NOT IN                              (SELECT        Item_id                                 FROM            Unit_item)) ");
            //            opp.insert_me(" Delete from  Bills_List where id= 143 Or id=161 ");
            //            str = " Delete from  Bills_Details where Invoice_Id = 143 Or Invoice_Id =161 ";
            //            opp.insert_me(str);
            //        }



            //    }
            //    catch
            //    {
            //    }
            //}
           




            AlterBD_(ref opp, "Invoices_Details", "Item_DeletedFlage", "int", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Item_DeletedFlage", "int", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Item_DeletedFlage", "int", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Item_DeletedFlage", "int", "0", "0");

            AlterBD_(ref opp, "Invoices_Details", "Item_ProcessedFlage", "int", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Item_ProcessedFlage", "int", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Item_ProcessedFlage", "int", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Item_ProcessedFlage", "int", "0", "0");



            AlterBD_(ref opp, "LookUp_CheckStatus", "Active", "int", "1", "1");

            AlterBD_(ref opp, "LookUp_CheckStatus", "SubAccount_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "ClinicPationtDetails", "ClinicUsed_Id", "int", "0", "0");
            AlterBD_(ref opp, "Item_BarCodeSetting", "PaperTopMargin", "nvarchar(50)");
            AlterBD_(ref opp, "Item_BarCodeSetting", "PaperLeftMargin", "nvarchar(50)");
            AlterBD_(ref opp, "Item_BarCodeSetting", "PaperRightMargin", "nvarchar(50)");
            AlterBD_(ref opp, "Item_BarCodeSetting", "PaperBottomMargin", "nvarchar(50)");
            AlterBD_(ref opp, "Price_Type_Sell_Values", "CommisionRat", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Price_Type_Sell_Values", "CommisionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Price_Type_Sell_Values", "Is_CommisionRat", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "Invoice_Item_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_Details", "NameSpecification", "nvarchar(500)");
            AlterColumn(ref opp, "Invoices_Details", "Specification", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_Details", "LargItemsQnty_Free", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "LargItemsQnty_Free", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "LargItemsQnty_Free", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "LargItemsQnty_Free", "decimal(18, " + d + ")", "0", "0");



            AlterBD_(ref opp, "Invoices_Details", "SmallItemsQnty_Free", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "SmallItemsQnty_Free", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "SmallItemsQnty_Free", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "SmallItemsQnty_Free", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "Is_Alternate_Item_CommissionValue", "int", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Is_Alternate_Item_CommissionValue", "int", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Is_Alternate_Item_CommissionValue", "int", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Is_Alternate_Item_CommissionValue", "int", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "Alternate_Item_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Alternate_Item_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Alternate_Item_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Alternate_Item_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "Alternate_Item_CommissionRate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Alternate_Item_CommissionRate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Alternate_Item_CommissionRate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Alternate_Item_CommissionRate", "decimal(18, " + d + ")", "0", "0");




            AlterBD_(ref opp, "Invoices_Details", "TotalSmall_Qnty", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "TotalSmall_Qnty", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "TotalSmall_Qnty", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "TotalSmall_Qnty", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Invoices_Details", "TotalSmall_QntyStr", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "TotalSmall_QntyStr", "nvarchar(500)");
            AlterBD_(ref opp, "Bills_Details", "TotalSmall_QntyStr", "nvarchar(500)");
            AlterBD_(ref opp, "Bills_Details_Reverse", "TotalSmall_QntyStr", "nvarchar(500)");

            AlterBD_(ref opp, "Invoices_Details", "TotalSmall_QntyStrColumn", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "TotalSmall_QntyStrColumn", "nvarchar(500)");
            AlterBD_(ref opp, "Bills_Details", "TotalSmall_QntyStrColumn", "nvarchar(500)");
            AlterBD_(ref opp, "Bills_Details_Reverse", "TotalSmall_QntyStrColumn", "nvarchar(500)");


            AlterColumn(ref opp, "Unit_item", "Full_qua", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Unit_item", "D_Qua", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Unit_item", "Item_No", "decimal(18, " + d + ")");

            AlterColumn(ref opp, "Unit_item", "Reserved_Balance_Larg_Qnty", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Unit_item", "Reserved_Balance_Small_Qnty", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Unit_item", "P_price", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Unit_item", "S_price", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Unit_item", "Upon_Request", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Unit_item", "Small_unit", "decimal(18, " + d + ")");


            AlterColumn(ref opp, "Invoices_List", "Customer_Id", "bigint");
            AlterColumn(ref opp, "Invoices_List", "Currency_Id", "int");
            AlterColumn(ref opp, "Invoices_List_Reverse", "Customer_Id", "bigint");
            AlterColumn(ref opp, "Invoices_List_Reverse", "Currency_Id", "int");

            AlterColumn(ref opp, "Bills_List", "Supplier_Id", "bigint");
            AlterColumn(ref opp, "Bills_List", "Currency_Id", "int");

            AlterColumn(ref opp, "Bills_List_Reverse", "Supplier_Id", "bigint");
            AlterColumn(ref opp, "Bills_List_Reverse", "Currency_Id", "int");
            AlterColumn(ref opp, "Unit", "Id", "bigint");
            AlterColumn(ref opp, "Unit", "ProcessNo", "bigint");

            AlterColumn(ref opp, "Unit", "B_Unit_Value", "int");
            AlterColumn(ref opp, "Unit", "S_unit_Value", "decimal(18, " + d + ")");





            AlterBD_(ref opp, "Bills_Details_Reverse", "Alternate_ItemCommissionTotal", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "Alternate_ItemCommissionTotal", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Alternate_ItemCommissionTotal", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Alternate_ItemCommissionTotal", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Alternate_ItemCommissionTotal", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "Is_Tax_Per_Item", "Int", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Is_Tax_Per_Item", "Int", "0", "0");
            AlterBD_(ref opp, "Bills_List", "Is_Tax_Per_Item", "Int", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "Is_Tax_Per_Item", "Int", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "Order_MoneyId", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Order_MoneyId", "bigint", "0", "0");
            AlterBD_(ref opp, "Bills_List", "Order_MoneyId", "bigint", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "Order_MoneyId", "bigint", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "BanksId", "Int", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "BanksId", "Int", "0", "0");
            AlterBD_(ref opp, "Bills_List", "BanksId", "Int", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "BanksId", "Int", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "Is_Load_Payment_OnTax", "Int", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Is_Load_Payment_OnTax", "Int", "0", "0");
            AlterBD_(ref opp, "Bills_List", "Is_Load_Payment_OnTax", "Int", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "Is_Load_Payment_OnTax", "Int", "0", "0");

            AlterBD_drop(ref opp, "Invoices_List", "Total_PriceWithoutTax_Load_Payment", "decimal(18, " + d + ")", "0", "0");
            AlterBD_drop(ref opp, "Invoices_List_Reverse", "Total_PriceWithoutTax_Load_Payment", "decimal(18, " + d + ")", "0", "0");
            AlterBD_drop(ref opp, "Bills_List", "Total_PriceWithoutTax_Load_Payment", "decimal(18, " + d + ")", "0", "0");
            AlterBD_drop(ref opp, "Bills_List_Reverse", "Total_PriceWithoutTax_Load_Payment", "decimal(18, " + d + ")", "0", "0");

            AlterBD_drop(ref opp, "Invoices_List", "PriceByTenns", "decimal(18, " + d + ")", "0", "0");
            AlterBD_drop(ref opp, "Invoices_List_Reverse", "PriceByTenns", "decimal(18, " + d + ")", "0", "0");
            AlterBD_drop(ref opp, "Bills_List", "PriceByTenns", "decimal(18, " + d + ")", "0", "0");
            AlterBD_drop(ref opp, "Bills_List_Reverse", "PriceByTenns", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Invoices_List", "Emp_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Emp_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Bills_List", "Emp_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Bills_List_Reverse", "Emp_Name", "nvarchar(500)");



            AlterBD_(ref opp, "Invoices_List", "Customer_Notes", "nvarchar(100)");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Customer_Notes", "nvarchar(100)");
            AlterBD_(ref opp, "Bills_List", "Supplier_Notes", "nvarchar(100)");
            AlterBD_(ref opp, "Bills_List_Reverse", "Supplier_Notes", "nvarchar(100)");

            AlterBD_(ref opp, "Invoices_List", "BanksNamePaymentMethod", "nvarchar(100)");
            AlterBD_(ref opp, "Invoices_List_Reverse", "BanksNamePaymentMethod", "nvarchar(100)");
            AlterBD_(ref opp, "Bills_List", "BanksNamePaymentMethod", "nvarchar(100)");
            AlterBD_(ref opp, "Bills_List_Reverse", "BanksNamePaymentMethod", "nvarchar(100)");

            AlterBD_(ref opp, "Invoices_List", "PaidValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "PaidValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List", "PaidValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "PaidValue", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Invoices_List", "RestValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "RestValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List", "RestValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "RestValue", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Invoices_List", "Is_Invoice_Agile", "Int", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Is_Invoice_Agile", "Int", "0", "0");
            AlterBD_(ref opp, "Bills_List", "Is_Invoice_Agile", "Int", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "Is_Invoice_Agile", "Int", "0", "0");


            AlterBD_(ref opp, "Invoices_List", "Is_Invoice_AgileStr", "nvarchar(100)");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Is_Invoice_AgileStr", "nvarchar(100)");
            AlterBD_(ref opp, "Bills_List", "Is_Invoice_AgileStr", "nvarchar(100)");
            AlterBD_(ref opp, "Bills_List_Reverse", "Is_Invoice_AgileStr", "nvarchar(100)");


            AlterBD_(ref opp, "Bills_Details_Reverse", "Net_Total_Item_Commission", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "Net_Total_Item_Commission", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Net_Total_Item_Commission", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Net_Total_Item_Commission", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Net_Total_Item_Commission", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Invoices_Details", "Store_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Store_Name", "nvarchar(500)");

            AlterBD_(ref opp, "Bills_Details", "Store_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Store_Name", "nvarchar(500)");



            AlterBD_(ref opp, "Invoices_Details", "Item_CommissionRate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Item_CommissionRate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Item_CommissionRate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Item_CommissionRate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "Unit_CommissionRate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Unit_CommissionRate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Unit_CommissionRate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Unit_CommissionRate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "Alternate_Unit_CommissionRate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Alternate_Unit_CommissionRate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Alternate_Unit_CommissionRate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Alternate_Unit_CommissionRate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "Alternate_Unit_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Alternate_Unit_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Alternate_Unit_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Alternate_Unit_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "Unit_Item_ExpireDate_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Unit_Item_ExpireDate_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Unit_Item_ExpireDate_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Unit_Item_ExpireDate_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "Merchant_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Merchant_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Bills_Details", "Merchant_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Merchant_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_Details", "Tash3ela", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Tash3ela", "nvarchar(500)");
            AlterBD_(ref opp, "Bills_Details", "Tash3ela", "nvarchar(500)");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Tash3ela", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Invoice_Item_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "NameSpecification", "nvarchar(500)");
            AlterColumn(ref opp, "Invoices_Details_Reverse", "Specification", "nvarchar(500)");
            AlterBD_(ref opp, "Bills_Details", "Invoice_Item_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Bills_Details", "NameSpecification", "nvarchar(500)");
            AlterColumn(ref opp, "Bills_Details", "Specification", "nvarchar(500)");
            AlterColumn(ref opp, "Invoices_List", "Notes", "nvarchar(MAX)");
            AlterColumn(ref opp, "Invoices_List_Reverse", "Notes", "nvarchar(MAX)");
            AlterColumn(ref opp, "Bills_List", "Notes", "nvarchar(MAX)");
            AlterColumn(ref opp, "Bills_List_Reverse", "Notes", "nvarchar(MAX)");







            AlterBD_(ref opp, "Bills_Details_Reverse", "Invoice_Item_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Bills_Details_Reverse", "NameSpecification", "nvarchar(500)");
            AlterColumn(ref opp, "Bills_Details_Reverse", "Specification", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_Details", "Item_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "Is_Item_CommissionValue", "int", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Item_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Is_Item_CommissionValue", "int", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Item_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Is_Item_CommissionValue", "int", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Item_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Is_Item_CommissionValue", "int", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "ItemCommissionTotal", "decimal(18, " + d + ")", "0", " Item_CommissionValue + Alternate_Item_CommissionValue ");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "ItemCommissionTotal", "decimal(18, " + d + ")", "0", " Item_CommissionValue + Alternate_Item_CommissionValue ");
            AlterBD_(ref opp, "Bills_Details", "ItemCommissionTotal", "decimal(18, " + d + ")", "0", " Item_CommissionValue + Alternate_Item_CommissionValue ");
            AlterBD_(ref opp, "Bills_Details_Reverse", "ItemCommissionTotal", "decimal(18, " + d + ")", "0", " Item_CommissionValue + Alternate_Item_CommissionValue ");
            AlterBD_(ref opp, "Invoices_Details", "QntyWidth", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "QntyLenght", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "QntyItemNo", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "QntyWidth", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "QntyLenght", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "QntyItemNo", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "Bills_Details_Id", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Bills_Details_Id", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "Purchase_Price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Purchase_Price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Larg_Qnty_Balance", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Small_Qnty_Balance", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Qnty_Another_Balance", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Is_BuyFrom_Bills", "int", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Is_BuyFrom_Bills", "int", "0", "0");
            AlterBD_(ref opp, "Bills_List", "Is_Buy_From_Bills_Items_Balance", "int", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "Is_Buy_From_Bills_Items_Balance", "int", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Is_Buy_From_Bills_Items_Balance", "int", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Is_Buy_From_Bills_Items_Balance", "int", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Audience_price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Item_Expire_Date", "datetime");
            AlterBD_(ref opp, "Invoices_Details", "Audience_price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "Item_Expire_Date", "datetime");
            AlterBD_(ref opp, "Invoices_Details", "Tash3ela", "nvarchar(100)");
            AlterBD_(ref opp, "Invoices_Details", "Item_No_Free", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "LargItemsQnty_Free", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "SmallItemsQnty_Free", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Audience_price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Item_Expire_Date", "datetime");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Item_No_Free", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Audience_price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Item_Expire_Date", "datetime");

            AlterBD_(ref opp, "Bills_Details_Reverse", "Larg_Qnty_Balance", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Small_Qnty_Balance", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Bills_Details", "Item_No_Free", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Audience_price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Item_No_Free", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Audience_price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List", "Total_PriceWithoutTax", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "Total_PriceWithoutTax", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Total_PriceWithoutTax", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Total_PriceWithoutTax", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List", "Invoice_DateTime", "datetime", "getdate()", "Invoice_Date");
            AlterBD_(ref opp, "Bills_List_Reverse", "Invoice_DateTime", "datetime", "getdate()", "Invoice_Date");
            AlterBD_(ref opp, "Invoices_List", "Invoice_DateTime", "datetime", "getdate()", "Invoice_Date");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Invoice_DateTime", "datetime", "getdate()", "Invoice_Date");
            AlterBD_(ref opp, "Bills_Details", "QntyWidth", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "QntyLenght", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "QntyItemNo", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "QntyWidth", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "QntyLenght", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "QntyItemNo", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Is_InvoiceItem_Kta3y", "Int", "0", "-1");
            AlterBD_(ref opp, "Bills_Details", "Is_InvoiceItem_Kta3y", "Int", "0", "-1");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Is_InvoiceItem_Kta3y", "Int", "0", "-1");
            AlterBD_(ref opp, "Invoices_Details", "Is_InvoiceItem_Kta3y", "Int", "0", "-1");
            AlterBD_(ref opp, "Bills_Details_Reverse", "StatementNo", "nvarchar(100)");
            AlterBD_(ref opp, "Bills_Details", "StatementNo", "nvarchar(100)");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "StatementNo", "nvarchar(100)");
            AlterBD_(ref opp, "Invoices_Details", "StatementNo", "nvarchar(100)");
            AlterBD_(ref opp, "Bills_Details_Reverse", "OperationNo", "nvarchar(100)");
            AlterBD_(ref opp, "Bills_Details", "OperationNo", "nvarchar(100)");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "OperationNo", "nvarchar(100)");
            AlterBD_(ref opp, "Invoices_Details", "OperationNo", "nvarchar(100)");
            AlterBD_(ref opp, " ReportSellPurchaseItems", "Invoice_Id_No", "int", "0", "0");

            AlterBD_(ref opp, "Invoices_Details", "Unit_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Unit_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Unit_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Unit_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details", "Unit_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Unit_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Unit_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Unit_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Money_Financial_Notice_Items_Details", "NotesDetails", "nvarchar(500)");

            AlterBD_(ref opp, " Money_Financial_Notice", "IS_Posted_Flage", "int", "0", "0");
            AlterBD_(ref opp, " Money_Financial_Notice_Items", "IS_Posted_Flage", "int", "0", "0");


            AlterBD_(ref opp, " Money_Financial_Notice", "Reference_Type", "int", "0", "0");
            AlterBD_(ref opp, " Money_Financial_Notice_Items", "Reference_Type", "int", "0", "0");


            AlterBD_(ref opp, " Money_Transaction_Box", "Invoice_TypeId", "int", "0", "0");
            AlterBD_(ref opp, " Money_Transaction_Box", "Invoice_Id", "bigint", "0", "0");

            AlterBD_(ref opp, " Money_Transaction_Box", "Is_ProcessedFlage", "int", "0", "0");


            AlterBD_(ref opp, " Money_Financial_Notice", "Account_Entry_List_ID", "bigint", "0", "0");
            AlterBD_(ref opp, " Money_Financial_Notice_Items", "Account_Entry_List_ID", "bigint", "0", "0");


            AlterBD_(ref opp, " Invoices_List", "Salesman_Id", "int", "0", "0");
            AlterBD_(ref opp, " Invoices_List_Reverse", "Salesman_Id", "int", "0", "0");
            AlterBD_(ref opp, " Bills_List", "Salesman_Id", "int", "0", "0");
            AlterBD_(ref opp, " Bills_List_Reverse", "Salesman_Id", "int", "0", "0");

            AlterBD_(ref opp, " Invoices_List", "Emp_Id", "bigint", "0", "0");
            AlterBD_(ref opp, " Invoices_List_Reverse", "Emp_Id", "bigint", "0", "0");
            AlterBD_(ref opp, " Bills_List", "Emp_Id", "bigint", "0", "0");
            AlterBD_(ref opp, " Bills_List_Reverse", "Emp_Id", "bigint", "0", "0");


            AlterBD_(ref opp, " Invoices_List", "Is_Tax_Per_Item", "int", "0", "0");
            AlterBD_(ref opp, " Invoices_List_Reverse", "Is_Tax_Per_Item", "int", "0", "0");
            AlterBD_(ref opp, " Bills_List", "Is_Tax_Per_Item", "int", "0", "0");
            AlterBD_(ref opp, " Bills_List_Reverse", "Is_Tax_Per_Item", "int", "0", "0");



            AlterBD_(ref opp, " Invoices_List", "OperationDate", "DateTime");
            AlterBD_(ref opp, " Invoices_List_Reverse", "OperationDate", "DateTime");
            AlterBD_(ref opp, " Bills_List", "OperationDate", "DateTime");
            AlterBD_(ref opp, " Bills_List_Reverse", "OperationDate", "DateTime");
            AlterBD_(ref opp, " Invoices_List", "IS_Make_Sarf_Item_BalanceFromInvoice", "int", "1", "1");
            AlterBD_(ref opp, " Invoices_List_Reverse", "IS_Make_Sarf_Item_BalanceFromInvoice", "int", "1", "1");
            AlterBD_(ref opp, " Bills_List", "IS_Make_Sarf_Item_BalanceFromInvoice", "int", "1", "1");
            AlterBD_(ref opp, " Bills_List_Reverse", "IS_Make_Sarf_Item_BalanceFromInvoice", "int", "1", "1");
            AlterBD_(ref opp, " Invoices_List", "Is_InvoiceKta3y", "int", "0", "0");
            AlterBD_(ref opp, " Invoices_List_Reverse", "Is_InvoiceKta3y", "int", "0", "0");
            AlterBD_(ref opp, " Bills_List", "Is_InvoiceKta3y", "int", "0", "0");
            AlterBD_(ref opp, " Bills_List_Reverse", "Is_InvoiceKta3y", "int", "0", "0");
            AlterBD_(ref opp, " Invoices_List", "Total_Item_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Invoices_List_Reverse", "Total_Item_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Bills_List", "Total_Item_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Bills_List_Reverse", "Total_Item_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Invoices_List", "AnotherPayment", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Invoices_List_Reverse", "AnotherPayment", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Invoices_List", "Total_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Invoices_List_Reverse", "Total_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Bills_List", "Total_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Bills_List_Reverse", "Total_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Customer", "VisaNo", "nvarchar(50)");
            AlterBD_(ref opp, "Customer", "VisaPassword", "nvarchar(50)");
            AlterBD_(ref opp, "Customer", "CustomerNotes", "nvarchar(500)");
            AlterBD_(ref opp, "Customer", "Commercial_Register", "nvarchar(500)");
            AlterBD_(ref opp, "Customer", "Tax_No", "nvarchar(500)");
            AlterBD_(ref opp, "Customer", "ContactPerson", "nvarchar(500)");

            AlterBD_(ref opp, "Supplier", "Commercial_Register", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_List", "GuId", "UNIQUEIDENTIFIER   default NEWID()");
            AlterBD_(ref opp, "Invoices_List_Reverse", "GuId", "UNIQUEIDENTIFIER   default NEWID()");
            AlterBD_(ref opp, " Bills_List", "GuId", "UNIQUEIDENTIFIER   default NEWID()");
            AlterBD_(ref opp, " Bills_List_Reverse", "GuId", "UNIQUEIDENTIFIER   default NEWID()");
            AlterColumn(ref opp, "Money_Transaction_Box", "Transaction_Value_", "nvarchar(500)");
            AlterColumn(ref opp, "Money_Transaction_Box", "Import_Value", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Money_Transaction_Box", "Export_Value", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Money_Transaction_Box", "Transaction_Value", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Money_Transaction_Box", "Bank_Balance", "decimal(18, " + d + ")");


            AlterColumn(ref opp, "Detailes_Purshing_Import_Export", "Machine_Id", "bigint");
            AlterColumn(ref opp, "Item_First_Period_Balance", "Item_Id", "bigint");
            AlterColumn(ref opp, "Item_First_Period_Balance", "Unit_Item_Id", "bigint");


            AlterColumn(ref opp, "Checks_Memory", "Check_Value_", "nvarchar(500)");
            AlterColumn(ref opp, "Checks_Memory", "Check_Value", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Checks_Memory", "Import_Value", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Checks_Memory", "Export_Value", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Checks_Memory", "Bank_Balance", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Checks_Memory", "Balance_Before", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Checks_Memory", "Balance_After", "decimal(18, " + d + ")");

            

            AlterColumn(ref opp, "Employee", "jobsListId", "bigint");
            AlterColumn(ref opp, "Employee", "Edit_User_Id", "bigint");
            AlterColumn(ref opp, "Employee", "SubDeptID", "bigint");
            AlterColumn(ref opp, "Employee", "SubAccount_Id", "bigint");
            AlterColumn(ref opp, "Employee", "Branch_Id", "bigint");
            AlterColumn(ref opp, "Employee", "User_Id", "bigint");



            //AlterBD_(ref opp, "", "", "", "0", "0");


            AlterBD_(ref opp, "Customer", "Gov_Country_ListId", "Int", "0");
            AlterBD_(ref opp, "Customer", "Gov_City_ListId", "Int", "0");

            AlterBD_(ref opp, "Supplier", "Gov_Country_ListId", "Int", "0");
            AlterBD_(ref opp, "Supplier", "Gov_City_ListId", "Int", "0");


            AlterBD_(ref opp, "Customer", "Govern_Area_ListId", "Int", "0");
            AlterBD_(ref opp, "Customer", "Govern_ListId", "Int", "0");
            //AlterBD_(ref opp, "Customer", "Govern_Area_Name", "nvarchar(200)");
            //AlterBD_(ref opp, "Customer", "Govern_Name", "nvarchar(200)");


            AlterBD_dropColumn(ref opp, "Customer", "Govern_Name");
            AlterBD_dropColumn(ref opp, "Customer", "Govern_Id");
            AlterBD_dropColumn(ref opp, "Customer", "Govern_Area_Name");
            AlterBD_dropColumn(ref opp, "Customer", "Govern_Area_Id");


            AlterBD_(ref opp, "Supplier", "Govern_Area_ListId", "Int", "0");
            AlterBD_(ref opp, "Supplier", "Govern_ListId", "Int", "0");
            AlterBD_(ref opp, "Supplier", "Govern_Area_Name", "nvarchar(200)");
            AlterBD_(ref opp, "Supplier", "Govern_Name", "nvarchar(200)");
            AlterColumn(ref opp, "Supplier", "Balance", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Supplier", "Balance_Actual", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Suppliers_Accounts", "Import", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Suppliers_Accounts", "Export", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Suppliers_Accounts", "Balance", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Suppliers_Accounts", "Balance_Actual", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Customers_Accounts", "Import", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Customers_Accounts", "Export", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Customers_Accounts", "Balance", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Customers_Accounts", "Balance_Actual", "decimal(18, " + d + ")");
            AlterBD_(ref opp, "Customer", "Group_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Supplier", "Group_Id", "bigint", "0", "0");
            //AlterBD_(ref opp, "Supplier", "Group_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Customers_Accounts", "Notes", "nvarchar(500)");
            AlterBD_(ref opp, "Suppliers_Accounts", "Notes", "nvarchar(500)");
            AlterBD_(ref opp, "Customers_Accounts", "BalanceImport", "nvarchar(100)");
            AlterBD_(ref opp, "Customers_Accounts", "BalanceExport", "nvarchar(100)");
            AlterBD_(ref opp, "Customers_Accounts", "Item_Name", "nvarchar(100)");
            AlterBD_(ref opp, "Customers_Accounts", "Item_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Customers_Accounts", "Larg_Qnty", "nvarchar(100)");
            AlterBD_(ref opp, "Customers_Accounts", "Small_Qnty", "nvarchar(100)");
            AlterBD_(ref opp, "Customers_Accounts", "Item_No", "nvarchar(100)");
            AlterBD_(ref opp, "Customers_Accounts", "Item_Size", "nvarchar(100)");
            AlterBD_(ref opp, "Customers_Accounts", "Qnty_Size", "nvarchar(100)");
            AlterBD_(ref opp, "Customers_Accounts", "Qnty_value", "nvarchar(100)");
            AlterBD_(ref opp, "Customers_Accounts", "Height_Width_Item", "nvarchar(100)");
            AlterBD_(ref opp, "Suppliers_Accounts", "Item_Name", "nvarchar(100)");
            AlterBD_(ref opp, "Suppliers_Accounts", "Item_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Suppliers_Accounts", "Larg_Qnty", "nvarchar(100)");
            AlterBD_(ref opp, "Suppliers_Accounts", "Small_Qnty", "nvarchar(100)");
            AlterBD_(ref opp, "Suppliers_Accounts", "Item_No", "nvarchar(100)");
            AlterBD_(ref opp, "Suppliers_Accounts", "Item_Size", "nvarchar(100)");
            AlterBD_(ref opp, "Suppliers_Accounts", "Qnty_Size", "nvarchar(100)");
            AlterBD_(ref opp, "Suppliers_Accounts", "Qnty_value", "nvarchar(100)");
            AlterBD_(ref opp, "Suppliers_Accounts", "Height_Width_Item", "nvarchar(100)");
            AlterBD_(ref opp, "Suppliers_Accounts", "BalanceImport", "nvarchar(100)");
            AlterBD_(ref opp, "Suppliers_Accounts", "BalanceExport", "nvarchar(100)");
            AlterBD_(ref opp, "Suppliers_Accounts", "Reference_Type_View", "nvarchar(100)");
            AlterBD_(ref opp, "Customers_Accounts", "Reference_Type_View", "nvarchar(100)");


            AlterBD_(ref opp, "Customers_Accounts", "Reference_Type_TableDetails_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Suppliers_Accounts", "Reference_Type_TableDetails_Id", "bigint", "0", "0");


            AlterBD_(ref opp, "Customers_Accounts", "ProcessNo", "bigint", "0", "0");
            AlterBD_(ref opp, "Suppliers_Accounts", "ProcessNo", "bigint", "0", "0");


            AlterBD_(ref opp, " Money_Transaction_Box", "Serial", "bigint", "0", "0");
            AlterBD_(ref opp, "Customer", "Ref_Cust_Supplier_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Supplier", "Ref_Cust_Supplier_Id", "bigint", "0", "0");
            AlterBD_(ref opp, " Invoices_List", "SubAccount_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "SubAccount_Name", "nvarchar(100)");
            AlterBD_(ref opp, " Invoices_List_Reverse", "SubAccount_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "SubAccount_Name", "nvarchar(100)");
            AlterBD_(ref opp, " Bills_List", "SubAccount_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Bills_List", "SubAccount_Name", "nvarchar(100)");
            AlterBD_(ref opp, " Bills_List_Reverse", "SubAccount_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "SubAccount_Name", "nvarchar(100)");
            AlterBD_(ref opp, "Invoices_List", "Equivalent_TaxSales10_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Equivalent_ATS05_Value", "decimal(18, " + d + ")", "0", "0");
            //AlterBD_(ref opp, "Invoices_List", "Equivalent_Net_Total", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Equivalent_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "Equivalent_Total", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Equivalent_TaxSales10_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Equivalent_ATS05_Value", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, " Invoices_List", "Lookup_OrderDeliveryTypesId", "bigint", "0", "0");
            AlterBD_(ref opp, " Invoices_List_Reverse", "Lookup_OrderDeliveryTypesId", "bigint", "0", "0");


            AlterBD_(ref opp, " Invoices_List", "Lookup_InvoiceKitchenStatusId", "bigint", "0", "0");
            AlterBD_(ref opp, " Invoices_List_Reverse", "Lookup_InvoiceKitchenStatusId", "bigint", "0", "0");


            AlterBD_(ref opp, " Invoices_List", "Lookup_ShiftsId", "bigint", "0", "0");
            AlterBD_(ref opp, " Invoices_List_Reverse", "Lookup_ShiftsId", "bigint", "0", "0");


            ///




            //AlterBD_(ref opp, "Invoices_List_Reverse", "Equivalent_Net_Total", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Equivalent_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "Equivalent_Total", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List", "Equivalent_TaxSales10_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List", "Equivalent_ATS05_Value", "decimal(18, " + d + ")", "0", "0");
            //AlterBD_(ref opp, "Bills_List", "Equivalent_Net_Total", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List", "Equivalent_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List", "Equivalent_Total", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "Equivalent_TaxSales10_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "Equivalent_ATS05_Value", "decimal(18, " + d + ")", "0", "0");
            //AlterBD_(ref opp, "Bills_List_Reverse", "Equivalent_Net_Total", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "Equivalent_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "Equivalent_Total", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "IS_Posted_Flage", "int", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "IS_Posted_Flage", "int", "0", "0");
            AlterBD_(ref opp, "Bills_List", "IS_Posted_Flage", "int", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "IS_Posted_Flage", "int", "0", "0");
            AlterBD_(ref opp, " Invoices_List", "Account_Entry_List_ID", "bigint", "0", "0");
            AlterBD_(ref opp, " Invoices_List_Reverse", "Account_Entry_List_ID", "bigint", "0", "0");
            AlterBD_(ref opp, " Bills_List", "Account_Entry_List_ID", "bigint", "0", "0");
            AlterBD_(ref opp, " Bills_List_Reverse", "Account_Entry_List_ID", "bigint", "0", "0");
            AlterBD_(ref opp, "Invoices_List", "IsCommission_Value", "int", "0", "0");
            AlterBD_(ref opp, "Invoices_List_Reverse", "IsCommission_Value", "int", "0", "0");
            AlterBD_(ref opp, "Bills_List", "IsCommission_Value", "int", "0", "0");
            AlterBD_(ref opp, "Bills_List_Reverse", "IsCommission_Value", "int", "0", "0");
            AlterBD_(ref opp, "Item_ImagesStore", "Item_Type", "int", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing", "Related_Detailes_Purshing_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Process_Date", "DateTime");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Notes_To", "nvarchar(100)");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "OperationDate", "DateTime");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "User_Id", "int", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Total_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Detailes_Purshing_Import_Export", "Invoice_No", "bigint", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing", "Notes_Item", "nvarchar(500)");
            AlterBD_(ref opp, "Item_Raw_MaterialsFor_Entage_Details", "Notes_Item", "nvarchar(500)");
            AlterBD_(ref opp, "Item_Raw_MaterialsFor_Entage", "Notes_Item", "nvarchar(500)");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Notes_Item", "nvarchar(500)");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Invoice_Type_Str", "nvarchar(500)");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Notes_From", "nvarchar(100)");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "IS_To_Converted", "int", "1", "0");
            AlterBD_(ref opp, "Order_Money", "IS_Posted_Flage", "int", "0", "0");
            AlterBD_(ref opp, "Order_Money", "Invoice_Type", "int", "0", "0");
            AlterBD_(ref opp, " Detailes_Purshing_Import_Export", "Invoice_Id", "bigint", "0", "0");
            AlterBD_(ref opp, " Detailes_Purshing_Import_Export", "Invoice_Type_Id", "bigint", "0", "0");
            AlterBD_(ref opp, " Detailes_Purshing_Import_Export", "Detail_Invoice_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Item", "AnotherName", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "Height_Width_Str", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "Items_Name_Somk_Str", "nvarchar(500)");
            AlterBD_(ref opp, "Item", "Unit_Used_for_Price", "int", "0", "0");
            AlterBD_(ref opp, "Item", "Lookup_CarTypeId", "bigint", "0", "0");

            AlterBD_(ref opp, "Lookup_CarType", "NameEn", "nvarchar(500)");
            AlterBD_(ref opp, "Lookup_CarType", "Code", "nvarchar(500)");




            AlterBD_drop(ref opp, "Detailes_Purshing_Import_Export", "Total_Value", "Item_Net_Total_Price", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "PriceUnitImport", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Item_Net_Total_PriceImport", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "PriceUnitExport", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Item_Net_Total_PriceExport", "decimal(18, " + d + ")", "0", "0");




            AlterBD_(ref opp, "Item", "IsMasna3iatKhamat", "int", "0", "0");

            AlterBD_(ref opp, "Item", "S_price_LargeUnit", "decimal(18, " + d + ")", "0", "S_price", "");
            AlterBD_(ref opp, "Item", "S_price_SmallUnit", "decimal(18, " + d + ")", "0", "S_price", "");
            AlterBD_(ref opp, "Item", "P_price_LargeUnit", "decimal(18, " + d + ")", "0", "P_price", "");
            AlterBD_(ref opp, "Item", "P_price_SmallUnit", "decimal(18, " + d + ")", "0", "P_price", "");


            AlterBD_(ref opp, "Item", "S_price_Item_No", "decimal(18, " + d + ")", "0", "S_price", "");
            AlterBD_(ref opp, "Item", "P_price_Item_No", "decimal(18, " + d + ")", "0", "S_price", "");


            AlterBD_(ref opp, "Item", "Unit_Used_For_S_price_Default", "int", "1", "1");
            AlterBD_(ref opp, "Item", "Unit_Used_For_P_price_Default", "int", "1", "1");

            AlterBD_(ref opp, "Unit_Item", "S_price_LargeUnit", "decimal(18, " + d + ")", "0", "S_price", "");
            AlterBD_(ref opp, "Unit_Item", "S_price_SmallUnit", "decimal(18, " + d + ")", "0", "S_price", "");
            AlterBD_(ref opp, "Unit_Item", "P_price_LargeUnit", "decimal(18, " + d + ")", "0", "P_price", "");
            AlterBD_(ref opp, "Unit_Item", "P_price_SmallUnit", "decimal(18, " + d + ")", "0", "P_price", "");

            AlterBD_(ref opp, "Unit_Item", "Unit_Used_For_S_price_Default", "int", "1", "1");
            AlterBD_(ref opp, "Unit_Item", "Unit_Used_For_P_price_Default", "int", "1", "1");

            AlterBD_(ref opp, "Unit_Item", "S_price_Item_No", "decimal(18, " + d + ")", "0", "0", "S_price");
            AlterBD_(ref opp, "Unit_Item", "P_price_Item_No", "decimal(18, " + d + ")", "0", "0", "P_price");



            AlterBD_(ref opp, "Invoices_Details", "S_price_LargeUnit", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "S_price_LargeUnit", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "P_price_LargeUnit", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "P_price_LargeUnit", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Invoices_Details", "S_price_SmallUnit", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "S_price_SmallUnit", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "P_price_SmallUnit", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "P_price_SmallUnit", "decimal(18, " + d + ")", "0", "0");



            AlterBD_(ref opp, "Item", "OperationDate", "DateTime");
            AlterBD_(ref opp, "Item", "S_Price", "decimal(18, " + d + ")", "0", "0", "UPDATE       Item  SET    S_Price = Unit_item.S_price  FROM  Item INNER JOIN  Unit_item ON Item.id = Unit_item.Item_id");
            AlterBD_(ref opp, "Item", "P_Price", "decimal(18, " + d + ")", "0", "0", "UPDATE       Item  SET    P_Price = Unit_item.P_price  FROM  Item INNER JOIN  Unit_item ON Item.id = Unit_item.Item_id");
            AlterBD_(ref opp, "Item", "StoreUsed_Type", "int", "1", "1", "UPDATE Item SET StoreUsed_Type = Store.StoreUsed_Type FROM Store INNER JOIN Item ON Store.id = Item.S_id");
            AlterBD_(ref opp, "Item", "item_Name_", "nvarchar(500)");








            AlterBD_(ref opp, "Item", "Is_ItemWithoutBalance", "int", "0", "0");
            AlterBD_(ref opp, "Unit_item", "Is_ItemWithoutBalance", "int", "0", "0");

            AlterBD_(ref opp, "Item", "Items_TypesDescription_Name", "nvarchar(200)");
            AlterBD_(ref opp, "Item", "Items_TypesDescriptionNameOther", "nvarchar(200)");
            AlterBD_(ref opp, "Item", "Items_TypesFactory_Name", "nvarchar(200)");
            AlterBD_(ref opp, "Item", "Items_TypesSize_Name", "nvarchar(200)");
            AlterBD_(ref opp, "Item", "Items_TypesColor_Name", "nvarchar(200)");
            AlterBD_(ref opp, "Item", "Items_TypesDegreeFarz_Name", "nvarchar(200)");


            AlterBD_(ref opp, "Item", "Items_TypesDescription_NameId", "int", "0", "0");
            AlterBD_(ref opp, "Item", "Items_TypesDescriptionNameOtherId", "int", "0", "0");
            AlterBD_(ref opp, "Item", "Items_TypesFactory_NameId", "int", "0", "0");
            AlterBD_(ref opp, "Item", "Items_TypesSize_NameId", "int", "0", "0");
            AlterBD_(ref opp, "Item", "Items_TypesColor_NameId", "int", "0", "0");
            AlterBD_(ref opp, "Item", "Items_TypesDegreeFarz_NameId", "int", "0", "0");

            AlterBD_(ref opp, "Invoices_Details", "Item_Total_WithoutTax", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Item_Total_WithoutTax", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Item_Total_WithoutTax", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Item_Total_WithoutTax", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, "Invoices_Details", "Balance_Checked", "Int", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Balance_Checked", "Int", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Balance_Checked", "Int", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Balance_Checked", "Int", "0", "0");


            AlterBD_(ref opp, "Invoices_Details", "Supplier_BalanceSet", "Int", "0", "0");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Supplier_BalanceSet", "Int", "0", "0");
            AlterBD_(ref opp, "Bills_Details", "Supplier_BalanceSet", "Int", "0", "0");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Supplier_BalanceSet", "Int", "0", "0");




            AlterBD_(ref opp, "Invoices_Details", "Item_ATS05", "decimal(18, " + d + ")", "0", "ATS05");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Item_ATS05", "decimal(18, " + d + ")", "0", "ATS05");
            AlterBD_(ref opp, "Bills_Details", "Item_ATS05", "decimal(18, " + d + ")", "0", "ATS05");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Item_ATS05", "decimal(18, " + d + ")", "0", "ATS05");


            AlterBD_(ref opp, "Invoices_Details", "Item_ATS05_Value", "decimal(18, " + d + ")", "0", "ATS05_Value");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Item_ATS05_Value", "decimal(18, " + d + ")", "0", "ATS05_Value");
            AlterBD_(ref opp, "Bills_Details", "Item_ATS05_Value", "decimal(18, " + d + ")", "0", "ATS05_Value");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Item_ATS05_Value", "decimal(18, " + d + ")", "0", "ATS05_Value");

            AlterBD_(ref opp, "Invoices_Details", "Item_TaxSales10", "decimal(18, " + d + ")", "0", "TaxSales10");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Item_TaxSales10", "decimal(18, " + d + ")", "0", "TaxSales10");
            AlterBD_(ref opp, "Bills_Details", "Item_TaxSales10", "decimal(18, " + d + ")", "0", "TaxSales10");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Item_TaxSales10", "decimal(18, " + d + ")", "0", "TaxSales10");


            AlterBD_(ref opp, "Invoices_Details", "Item_TaxSales10_Value", "decimal(18, " + d + ")", "0", "TaxSales10_Value");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Item_TaxSales10_Value", "decimal(18, " + d + ")", "0", "TaxSales10_Value");
            AlterBD_(ref opp, "Bills_Details", "Item_TaxSales10_Value", "decimal(18, " + d + ")", "0", "TaxSales10_Value");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Item_TaxSales10_Value", "decimal(18, " + d + ")", "0", "TaxSales10_Value");


            AlterBD_(ref opp, "Unit_item", "TaxSales10_Value", "decimal(18, " + d + ")", "0", "TaxSales_Value");



            //AlterBD_(ref opp, "Invoices_Details", "Item_Total", "decimal(18, " + d + ")", "0", "Total");
            //AlterBD_(ref opp, "Invoices_Details_Reverse", "Item_Total", "decimal(18, " + d + ")", "0", "Total");
            //AlterBD_(ref opp, "Bills_Details", "Item_Total", "decimal(18, " + d + ")", "0", "Total");
            //AlterBD_(ref opp, "Bills_Details_Reverse", "Item_Total", "decimal(18, " + d + ")", "0", "Total");


            AlterBD_(ref opp, "Invoices_Details", "Item_Net_Total_Price", "decimal(18, " + d + ")", "0", "Total");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Item_Net_Total_Price", "decimal(18, " + d + ")", "0", "Total");
            AlterBD_(ref opp, "Bills_Details", "Item_Net_Total_Price", "decimal(18, " + d + ")", "0", "Total");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Item_Net_Total_Price", "decimal(18, " + d + ")", "0", "Total");


            AlterBD_(ref opp, "Invoices_Details", "Item_Notes", "nvarchar(500)", "0", "Notes");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "Item_Notes", "nvarchar(500)", "0", "Notes");
            AlterBD_(ref opp, "Bills_Details", "Item_Notes", "nvarchar(500)", "0", "Notes");
            AlterBD_(ref opp, "Bills_Details_Reverse", "Item_Notes", "nvarchar(500)", "0", "Notes");

            AlterBD_(ref opp, "Invoices_Details", "QntyStr", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "QntyStr", "nvarchar(500)");
            AlterBD_(ref opp, "Bills_Details", "QntyStr", "nvarchar(500)");
            AlterBD_(ref opp, "Bills_Details_Reverse", "QntyStr", "nvarchar(500)");

            AlterBD_(ref opp, "Invoices_Details", "QntyStrColumn", "nvarchar(500)");
            AlterBD_(ref opp, "Invoices_Details_Reverse", "QntyStrColumn", "nvarchar(500)");
            AlterBD_(ref opp, "Bills_Details", "QntyStrColumn", "nvarchar(500)");
            AlterBD_(ref opp, "Bills_Details_Reverse", "QntyStrColumn", "nvarchar(500)");


            AlterBD_(ref opp, "Item", "No3eboa", "int", "0");
            AlterBD_(ref opp, "Item", "No3eboa_BigUnit", "int", "0");
            AlterBD_(ref opp, "Item", "No3eboa_SmallUnit", "int", "0");
            AlterBD_(ref opp, "Item", "Items_Types_Name_Id", "int", "0");
            AlterBD_(ref opp, "Item", "Items_Types_Name", "nvarchar(200)");
            AlterBD_(ref opp, "Item", "Width_", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Item", "Height_", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Item", "Somk", "decimal(18,2)", "0", "0");
            AlterBD_(ref opp, "Item", "Somk_Unit", "nvarchar(100)");
            AlterColumn(ref opp, "Item", "Somk", "decimal(18,2)");
            AlterColumn(ref opp, "Item", "Item_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Tours_Omra", "Emp_Booking", "nvarchar(200)");
            AlterBD_(ref opp, "Tours_Hotels", "Emp_Booking", "nvarchar(200)");
            AlterBD_(ref opp, "Tours_Tasherat", "Emp_Booking", "nvarchar(200)");
            AlterBD_(ref opp, "Tours_Ticketing_Air", "Emp_Booking", "nvarchar(200)");
            AlterBD_(ref opp, "Tours_Ticketing_Land", "Emp_Booking", "nvarchar(200)");
            AlterBD_(ref opp, "Tours_Ticketing_Land", "DirectionEgypt", "nvarchar(200)");
            AlterBD_(ref opp, "Tours_Ticketing_Land", "DirectionSea", "nvarchar(200)");
            AlterBD_(ref opp, "Tours_Ticketing_Land", "DirectionSa", "nvarchar(200)");
            AlterBD_(ref opp, "Tours_Omra", "Emp_Booking_Id", "Int", "0", "0");
            AlterBD_(ref opp, "Tours_Hotels", "Emp_Booking_Id", "Int", "0", "0");
            AlterBD_(ref opp, "Tours_Tasherat", "Emp_Booking_Id", "Int", "0", "0");
            AlterBD_(ref opp, "Tours_Ticketing_Air", "Emp_Booking_Id", "Int", "0", "0");
            AlterBD_(ref opp, "Tours_Ticketing_Land", "Emp_Booking_Id", "Int", "0", "0");


            AlterBD_(ref opp, "Checks_Memory", "IsAllowEdit", "Int", "1", "0");
            AlterBD_(ref opp, "Checks_Memory", "IsAllowDelete", "Int", "1", "0");


            AlterBD_(ref opp, "Checks_Memory", "IS_Posted_Flage", "Int", "0", "0");
            AlterBD_(ref opp, "Checks_Memory", "Currency_Id", "Int", "0", "0");
            AlterBD_(ref opp, "Checks_Memory", "Factor", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Checks_Memory", "SubAccount_Name_RelatedTo", "nvarchar(500)");
            AlterBD_(ref opp, "Checks_Memory", "Account_Entry_List_ID", "bigint", "0", "0");
            AlterBD_(ref opp, "Checks_Memory", "SubAccount_Id_RelatedTo", "bigint", "0", "0");

            AlterBD_(ref opp, "Checks_Memory", "SubAccount_Name_RelatedFrom", "nvarchar(500)");
            AlterBD_(ref opp, "Checks_Memory", "SubAccount_Id_RelatedFrom", "bigint", "0", "0");
            AlterBD_(ref opp, "Checks_MemoryHistory", "SubAccount_Name_RelatedFrom", "nvarchar(500)");
            AlterBD_(ref opp, "Checks_MemoryHistory", "SubAccount_Id_RelatedFrom", "bigint", "0", "0");


            AlterBD_(ref opp, "Checks_Memory", "Balance_BeginingPeriod", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Item_PriceUnit_List", "TMP_Hadafat_Prod", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Item_PriceUnit_List", "TMP_Nazalat_Prod", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Order_Purshing", "Is_Converted", "Int", "0", "0");
            AlterBD_(ref opp, "Order_Purshing", "NotesAll", "nvarchar(500)");
            AlterBD_(ref opp, "Detailes_Purshing", "Transaction_Type", "Int", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "qua_Import", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Part_Qua_Import", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Item_No_Import", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Quantity_Import", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "qua_Export", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Part_Qua_Export", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Item_No_Export", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Quantity_Export", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Quantity", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Detailes_Purshing_Import_Export", "Quantity_Balance", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Detailes_Purshing_Import_Export", "Detailes_Purshing_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "Equivalent_Value", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "Account_Code", "nvarchar(max)");
            AlterBD_(ref opp, "Money_Transaction_Box", "Account_Type", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "Currency_Id", "int", "0", "0");
            AlterBD_(ref opp, "Supplier", "Currency_Id", "int", "0", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "Cost_Center", "nvarchar(max)");

            AlterBD_(ref opp, " Order_Purshing", "MainOrderTable_id", "bigint", "0");
            AlterBD_(ref opp, "Order_Purshing", "Id_FromStr", "nvarchar(500)");
            AlterBD_(ref opp, "Order_Purshing", "Id_ToStr", "nvarchar(500)");
            AlterBD_(ref opp, " Detailes_Purshing", "Qua_Befor", "bigint", "0");
            AlterBD_(ref opp, " Detailes_Purshing", "Part_qua_Befor", "bigint", "0");
            AlterBD_(ref opp, " Detailes_Purshing", "Item_No_Befor", "bigint", "0");
            AlterBD_(ref opp, " Detailes_Purshing", "Qua_After", "bigint", "0");
            AlterBD_(ref opp, " Detailes_Purshing", "Part_qua_After", "bigint", "0");
            AlterBD_(ref opp, " Detailes_Purshing", "Item_No_After", "bigint", "0");
            AlterBD_(ref opp, "Price_Type_List", "Price_Name_Master", "nvarchar(50)");
            AlterBD_(ref opp, "Price_Type_List", "Price_Type_List_Master_Id", "int", "0", "0");
            AlterBD_(ref opp, "Price_Type_List", "Plus_Minus", "nchar(50)");
            AlterBD_(ref opp, "Price_Type_List", "Added_Value", "decimal(18, 0)", "0", "0");
            AlterBD_(ref opp, "Price_Type_List", "IS_Rate", "int", "0", "0");

            AlterBD_(ref opp, "Account_Entry_Master_Balance", "Factor", "decimal(18, 0)", "1", "1");
            AlterBD_(ref opp, "Account_Entry_Master_Balance_CostCenterTemp", "Cost_Center_Id", "bigint", "0");
            AlterBD_(ref opp, "Account_Entry_Master_Balance_CostCenterTemp", "Cost_Center_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Account_Entry_Master_Balance_CostCenter", "Cost_Center_Id", "bigint", "0");
            AlterBD_(ref opp, "Account_Entry_Master_Balance_CostCenter", "Cost_Center_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Account_Entry_Master_BalanceFirstAccount_Temp", "Cost_Center_Id", "bigint", "0");
            AlterBD_(ref opp, "Account_Entry_Master_BalanceFirstAccount_Temp", "Cost_Center_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Account_Entry_Master_TransactionCostCenterTemp", "Cost_Center_Id", "bigint", "0");
            AlterBD_(ref opp, "Account_Entry_Master_TransactionCostCenterTemp", "Cost_Center_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Account_Entry_Details", "Account_Name", "nvarchar(500)");
            AlterBD_(ref opp, "Account_Entry_Details", "Line_No", "int", "0");
            AlterBD_(ref opp, "Account_Entry_Details", "Account_Currency_ID", "int", "0");
            AlterBD_(ref opp, "Account_Entry_Details", "Entry_Currency_ID", "int", "0");
            AlterBD_(ref opp, "Account_Entry_Details", "Factor", "decimal(18, " + d + ")", "0");
            AlterBD_(ref opp, "Account_Entry_Details", "Master_Submain_Account_Name", "nvarchar(MAX)");
            AlterBD_(ref opp, "Account_Entry_Details", "Master_Submain_Account_Id", "nvarchar(MAX)");
            AlterBD_(ref opp, " Account_Entry_List", "Branch_Id_Entry", "bigint", "0");
            AlterBD_(ref opp, " Account_Entry_List", "Process_Date", "smalldatetime");
            AlterBD_(ref opp, " Account_Entry_List", "Doc_No", "nvarchar(MAX)");
            AlterBD_(ref opp, " Account_Entry_List", "User_Id_Login_Name", "nvarchar(MAX)");
            AlterBD_(ref opp, " Account_Entry_List", "User_ID_Login", "bigint", "0");
            AlterColumn(ref opp, "Account_Entry_List", "Entry_Notes", "nvarchar(MAX)");



            AlterBD_(ref opp, " Invoices_List", "Total_Item_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Invoices_List_Reverse", "Total_Item_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Bills_List", "Total_Item_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Bills_List_Reverse", "Total_Item_CommissionValue", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, " Invoices_List", "List_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Invoices_List_Reverse", "List_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Bills_List", "List_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Bills_List_Reverse", "List_CommissionValue", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, " Invoices_List", "List_CommissionRate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Invoices_List_Reverse", "List_CommissionRate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Bills_List", "List_CommissionRate", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Bills_List_Reverse", "List_CommissionRate", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, " Invoices_List", "List_Final_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Invoices_List_Reverse", "List_Final_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Bills_List", "List_Final_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Bills_List_Reverse", "List_Final_CommissionValue", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, " Invoices_List", "Net_Total_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Invoices_List_Reverse", "Net_Total_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Bills_List", "Net_Total_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Bills_List_Reverse", "Net_Total_CommissionValue", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, " Invoices_List", "Equivalent_Net_Total_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Invoices_List_Reverse", "Equivalent_Net_Total_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Bills_List", "Equivalent_Net_Total_CommissionValue", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Bills_List_Reverse", "Equivalent_Net_Total_CommissionValue", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, " Invoices_List", "Equivalent_Net_Total_Price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Invoices_List_Reverse", "Equivalent_Net_Total_Price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Bills_List", "Equivalent_Net_Total_Price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, " Bills_List_Reverse", "Equivalent_Net_Total_Price", "decimal(18, " + d + ")", "0", "0");


            AlterBD_(ref opp, " Textile_Process_Transaction", "Supplier_Id", "bigint", "0", "0");
            AlterBD_(ref opp, " Textile_Process_Transaction", "Is_Motarz", "int", "0", "0");





            AlterBD_(ref opp, "Customers_Accounts", "Larg_Unit_Price", "decimal(18, " + d + ")", "0", "0");
            //AlterBD_(ref opp, "Customers_Accounts_Mokhtalat", "Larg_Unit_Price", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Suppliers_Accounts", "Larg_Unit_Price", "decimal(18, " + d + ")", "0", "0");

            AlterBD_(ref opp, "Customers_Accounts", "Item_Name", "nvarchar(100)");
            //AlterBD_(ref opp, "Customers_Accounts_Mokhtalat", "BalanceDiff", "decimal(18, " + d + ")", "0", "0");
            //AlterBD_(ref opp, "Customers_Accounts_Mokhtalat", "Insert_ServerDateTime", "datetime");




            AlterBD_(ref opp, "Suppliers_Accounts", "Item_Name", "nvarchar(100)");

            AlterBD_(ref opp, "Account_Entry_List", "Reference_Type", "int", "0", "0");
            AlterBD_(ref opp, "Account_Entry_List", "Reference_Type_Table_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Account_Entry_List", "ProcessNo", "bigint", "0", "0");
            AlterBD_(ref opp, "Account_Entry_Details", "ProcessNo", "bigint", "0", "0");
            AlterBD_(ref opp, "Account_Entry_Master_Balance_MezanTemp", "Account_Follows", "int", "0", "0");
            AlterBD_(ref opp, "Account_Entry_Master_Balance_MezanTemp", "Head_Main_Account", "int", "0", "0");
            AlterColumn(ref opp, "Account_Entry_Master_Balance_MezanTemp", "MainCode", "nvarchar(100)");
            AlterColumn(ref opp, "Account_Entry_Master_Balance_MezanTemp", "Code", "nvarchar(100)");

            AlterColumn(ref opp, "Account_Entry_Master_TransactionTemp", "MainCode", "nvarchar(100)");
            AlterColumn(ref opp, "Account_Entry_Master_TransactionTemp", "Code", "nvarchar(100)");




            AlterBD_(ref opp, "Account_Entry_Master_Balance_MezanTemp", "Totals", "decimal(18, " + d + ")", "0", "0");


            AlterColumn(ref opp, "Item_PriceUnit_List", "ItemWeightTextile", "decimal(18, " + d + ")");
            AlterColumn(ref opp, "Order_Working_Textile_Details", "Weight", "decimal(18, " + d + ")");

            AlterBD_(ref opp, "Order_Money", "Account_Entry_List_ID", "bigint", "0");
            AlterBD_(ref opp, "Account_Entry_List", "User_ID_LoginUpdate", "int", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "SubAccount_Id", "bigint", "0");
            AlterBD_(ref opp, "Account_Entry_List", "Order_Money_Id", "bigint", "0");
            AlterBD_(ref opp, "Account_Entry_List", "Entry_Currency_Name", "nvarchar(100)");
            AlterBD_(ref opp, "Order_Money", "Entry_Currency_Name", "nvarchar(100)");
            AlterBD_(ref opp, "Account_Entry_List", "Entry_Currency_ID", "int", "1");
            AlterBD_(ref opp, "Account_Entry_List", "Factor", "decimal(18, " + d + ")", "0");
            AlterBD_(ref opp, "Order_Money", "Factor", "decimal(18, " + d + ")", "0");
            AlterBD_(ref opp, "Order_Money", "Entry_Currency_ID", "int", "1");



            AlterBD_(ref opp, "Order_Money_Check", "Entry_Currency_Name", "nvarchar(100)");

            AlterBD_(ref opp, "Order_Money_Check", "Factor", "decimal(18, " + d + ")", "0", "0");
            AlterBD_(ref opp, "Order_Money_Check", "Entry_Currency_ID", "int", "1", "0");
            AlterBD_(ref opp, "Order_Money_Check", "Invoice_Type", "int", "0", "0");
            AlterBD_(ref opp, "Order_Money_Check", "Account_Entry_List_ID", "bigint", "0", "0");

            AlterBD_(ref opp, "Money_Transaction_Box", "Factor", "decimal(18, " + d + ")", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "Entry_Currency_ID", "int", "1");
            AlterBD_(ref opp, "Money_Transaction_Box", "Account_Name", "nvarchar(100)");
            AlterBD_(ref opp, "Money_Transaction_Box", "CostCenter_Name", "nvarchar(100)");
            AlterBD_(ref opp, "Item_PriceUnit_List", "SubItem_Name", "nvarchar(100)");
            AlterBD_(ref opp, "Item_PriceUnit_List", "Main_Item_Category_Id_Lehaf_Wabara", "int", "1", "1");
            AlterBD_(ref opp, "Item_PriceUnit_List", "SubItem_No", "int", "0");


            AlterBD_(ref opp, "Employee", "Color_Name", "nvarchar(100)");
            AlterBD_(ref opp, "Employee", "code", "nvarchar(500)");
            AlterBD_(ref opp, "Employee", "Tel", "nvarchar(500)");
            AlterBD_(ref opp, "Employee", "Tel2", "nvarchar(500)");
            AlterBD_(ref opp, "Employee", "Mobile", "nvarchar(500)");
            AlterBD_(ref opp, "Employee", "E_Mail", "nvarchar(500)");
            AlterBD_(ref opp, "Employee", "Fax", "nvarchar(500)");
            AlterBD_(ref opp, "Employee", "Web_Site", "nvarchar(500)");

            //AlterBD_(ref opp, "Employee", "job_list_Id", "int", "0", "0");
            AlterBD_dropColumn(ref opp, "Employee", "job_list_Id");
            AlterBD_dropColumn(ref opp, "Employee", "Jobs");

            AlterBD_(ref opp, "Employee", "jobsListId", "bigint", "0", "0");
            AlterBD_(ref opp, "Employee", "Edit_User_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Employee", "SubDeptID", "bigint", "0", "0");
            AlterBD_(ref opp, "Employee", "SubAccount_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Employee", "Branch_Id", "bigint", "0", "0");
            AlterBD_(ref opp, "Employee", "User_Id", "bigint", "0", "0");




            AlterBD_(ref opp, "Textile_Production_Rate", "AtalaValue", "decimal(18, " + d + ")", "0");
            AlterBD_(ref opp, "Textile_Production_Rate", "Atalat_Id", "int", "0");
            AlterBD_(ref opp, "Textile_Production_Rate", "Atalat_Name", "nvarchar(100)");
            AlterBD_(ref opp, "Textile_Production_Rate", "Price_Befor_Atala", "decimal(18, " + d + ")", "0");
            AlterBD_(ref opp, "Textile_Production_Rate", "Price_After_Atala", "decimal(18, " + d + ")", "0");
            AlterBD_(ref opp, "Textile_Production_Rate", "Atala", "decimal(18, " + d + ")", "0");
            AlterBD_(ref opp, "Textile_Production_Rate", "Hadafat_Atala", "decimal(18, " + d + ")", "0");
            AlterBD_(ref opp, "Textile_Production_Rate", "Nazalat_Atala", "decimal(18, " + d + ")", "0");
            AlterBD_(ref opp, "Machin_list_Textile", "Atala_efficiency", "decimal(18, " + d + ")", "0");
            AlterBD_(ref opp, "Unit_Item_Balance_Date_Details", "S_price", "decimal(18, " + d + ")", "0");
            AlterBD_(ref opp, "Unit_Item_Balance_Date_Details", "P_price", "decimal(18, " + d + ")", "0");
            AlterBD_(ref opp, "Unit_Item_Balance_Date_Details", "Store_Id", "bigint", "0");
            AlterBD_(ref opp, "Unit_Item_Balance_Date_Details", "Unit_Id", "bigint", "0");
            AlterBD_(ref opp, "Textile_Production_Rate", "Order_Working_Textile_Id", "bigint", "0");
            AlterBD_(ref opp, "Item_PriceUnit_List", "Items_Bodies_Name_Id", "int", "0");
            AlterBD_(ref opp, "Item_PriceUnit_List", "Items_Bodies_Name", "nvarchar(50)");
            AlterBD_(ref opp, "Item_PriceUnit_List", "Items_Types_Name_Id", "int", "0");
            AlterBD_(ref opp, "Item_PriceUnit_List", "Items_Types_Name", "nvarchar(50)");
            AlterBD_(ref opp, "Order_Working_Textile", "Total_Nazalat", "int", "0");
            AlterBD_(ref opp, "Order_Working_Textile", "Total_Weight", "decimal(18, " + d + ")", "0");
            try
            {
                str = "  CREATE TABLE [dbo].[Item_Category]( [ID] [bigint] {IDENTITY} NOT NULL, [Category_Name] [nvarchar](50) COLLATE Arabic_CI_AI_KS_WS NULL, [Category_Type_Str] [nvarchar](50) COLLATE Arabic_CI_AI_KS_WS NULL, [Category_Type_Id] [int] NULL, [Main_Item_Category_Id] [int] NULL, [IS_Active] [int] NULL DEFAULT ((1)) )   ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "CREATE TABLE [dbo].[Textile_Item_Dept_Balance]( [ID] [int] {IDENTITY} NOT NULL, [Item_Id] [int] NULL, [Dept_Id] [int] NULL, [Dept_Name] [nvarchar](50) COLLATE Arabic_CI_AI_KS_WS NULL, [Qnty] [decimal](18, 2) NULL, [Weight] [decimal](18, 2) NULL ) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "CREATE TABLE [dbo].[Textile_Process_Transaction]( [Id] [bigint] {IDENTITY} NOT NULL, [Item_Id] [bigint] NULL, [Import_Qnty] [decimal](18, 2) NULL, [Import_Weight] [decimal](18, 2) NULL, [Export_Qnty] [decimal](18, 2) NULL, [Export_Weight] [decimal](18, 2) NULL, [Qnty_Transaction] [decimal](18, 2) NULL, [Weight_Transaction] [decimal](18, 2) NULL, [Qnty_Balance] [decimal](18, 2) NULL, [Weight_Balance] [decimal](18, 2) NULL, [Processed_Flage] [int] NULL      DEFAULT ((0)), [From_Dept_Id] [int] NULL, [To_Dept_Id] [int] NULL, [Transaction_Date] [datetime] NULL, [Textile_Process_Transaction_Id] [bigint] NULL, [Processed_Flage_Date] [datetime] NULL, [Textile_Item_Dept_Balance_Id] [int] NULL, [Textile_Production_Rate_Id] [bigint] NULL )  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Money_Financial_Notice_Items_Details]( [Id] [bigint] {IDENTITY} NOT NULL, [Money_Financial_Notice_Items_Id] [bigint] NULL, [Item_Name] [nvarchar](500) NULL, [Item_Id] [bigint] NULL, [Item_QntyLarge] [decimal](18, 2) NULL, [Item_QntySmall] [nchar](10) NULL, [Item_Price] [decimal](18, 2) NULL, [Item_ToTal] [decimal](18, 2) NULL, [Unit_Item_Id] [bigint] NULL, [Store_Id] [bigint] NULL )";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Customer_Supplier_Group]( [ID] [bigint] {IDENTITY}   NOT NULL, [GroupName] [nvarchar](500) NULL, [GroupType] [int] NULL,  CONSTRAINT [PK_Customer_Supplier_Group] PRIMARY KEY CLUSTERED  ( [ID] ASC )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] ) ON [PRIMARY] ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"CREATE TABLE [dbo].[Money_Financial_Notice_Items]( [Id] [bigint] {IDENTITY} NOT NULL, [OrderNo] [bigint] NULL, [OrderDate] [datetime] NULL, [Transaction_Type] [int] NULL, [Transaction_TypeStr] [nvarchar](50) NULL, [Order_Value] [decimal](18, 2) NULL, \t[Item_Qnty] [decimal](18, 2) NULL, \t\t[Item_Price] [decimal](18, 2) NULL, \t\t\t\t[Item_Id] [bigint] NULL, [Item_Name] [nvarchar](500) NULL, [Order_Value_Str] [nvarchar](500) NULL, [Account_Type] [int] NULL, [Account_Id] [bigint] NULL, [SubAccount_Id] [bigint] NULL, [Currency_Id] [int] NULL, [Notes] [nvarchar](500) NULL, [Compete_SubAccount_Id] [bigint] NULL, [Account_Selection] [int] NULL, [Customer_Id] [bigint] NULL, [Supplier_Id] [bigint] NULL ) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"CREATE TABLE [dbo].[Money_Financial_Notice]( [Id] [bigint] {IDENTITY} NOT NULL, [OrderNo] [bigint] NULL, [OrderDate] [datetime] NULL, [Transaction_Type] [int] NULL, [Transaction_TypeStr] [nvarchar](50) NULL, [Order_Value] [decimal](18, 2) NULL, [Order_Value_Str] [nvarchar](500) NULL, [Account_Type] [int] NULL, [Account_Id] [bigint] NULL, [SubAccount_Id] [bigint] NULL, [Currency_Id] [int] NULL, [Notes] [nvarchar](500) NULL, [Compete_SubAccount_Id] [bigint] NULL, [Account_Selection] [int] NULL, [Customer_Id] [bigint] NULL, [Supplier_Id] [bigint] NULL ) ";
                opp.insert_me(str);
            }
            catch (Exception)
            {
            }
            try
            {
                str = @$"CREATE TABLE [dbo].[Item_Raw_MaterialsFor_Entage]( [Id] [bigint] {IDENTITY} NOT NULL, [MainOrderTable_id] [bigint] NULL, [SubOrderTable_id] [bigint] NULL, [Process_Date] [datetime] NULL, [MainItem_Id] [bigint] NULL, [SubItem_Id] [bigint] NULL, [MainItem_Name] [nvarchar](200) NULL, [SubItem_Name] [nvarchar](200) NULL, [SubUnit_item_id] [bigint] NULL, [MainUnit_item_id] [bigint] NULL, [Big_unit] [nvarchar](50) NULL, [Small_unit] [nvarchar](50) NULL, [Unit_Id] [bigint] NULL, [S_unit_Value] [decimal](18, 2) NULL, [Raw_Full_qua] [decimal](18, 2) NULL, [Raw_D_Qua] [decimal](18, 2) NULL, [Raw_Quantity] [decimal](18, 2) NULL, [Larg_Qnty] [decimal](18, 2) NULL, [Small_Qnty] [decimal](18, 2) NULL, [Qnty] [decimal](18, 2) NULL, [Main_Larg_Qnty] [decimal](18, 2) NULL, [Main_Small_Qnty] [decimal](18, 2) NULL, [Main_Qnty] [decimal](18, 2) NULL, [MainStore_Id] [bigint] NULL, [SubStore_Id] [bigint] NULL, [SubStore_Name] [nvarchar](50) NULL )  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"CREATE TABLE [dbo].[Item_Raw_MaterialsFor_Entage_List]( [Id] [bigint] {IDENTITY} NOT NULL, [Process_No] [bigint] NULL, [Notes_List] [nvarchar](500) NULL, [Process_Date] [datetime] NULL )  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"CREATE TABLE [dbo].[Item_Raw_MaterialsFor_Entage_Details]( [Id] [bigint] {IDENTITY} NOT NULL, [Item_Raw_MaterialsFor_Entage_List_Id] [bigint] NULL, [Unit_Item_Id] [bigint] NULL, [Item_Id] [bigint] NULL, [Store_Id] [bigint] NULL, [Larg_Qnty] [decimal](18, 2) NULL, [Small_Qnty] [decimal](18, 2) NULL, [Order_Purshing_Id] [bigint] NULL, [Detailes_Purshing_Id] [bigint] NULL, [Order_Estlam_Sarf_Flage] [int] NULL )   ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"CREATE TABLE [dbo].[Item_Raw_Materials]( [Id] [bigint] {IDENTITY} NOT NULL, [MainItem_Id] [bigint] NULL, [SubItem_Id] [bigint] NULL, [Full_qua] [decimal](18, 2) NULL, [D_Qua] [decimal](18, 2) NULL, [Quantity] [decimal](18, 2) NULL, [MainItem_Name] [nvarchar](200) NULL, [SubItem_Name] [nvarchar](200) NULL, [Big_unit] [nvarchar](50) NULL, [Small_unit] [nvarchar](50) NULL, [Unit_Id] [bigint] NULL, [S_unit_Value] [decimal](18, 2) NULL, [Default_Store_Id] [int] NULL )";
                opp.insert_me(str);
            }
            catch
            {
            }


            AlterBD_(ref opp, "Textile_Production_Rate", "Main_Item_Category_Id_Lehaf_Wabara", "int", "1", "1");

            try
            {
                str = "UPDATE       Supplier SET SubAccount_Id = Submain_Account.Id FROM            Submain_Account INNER JOIN                          Supplier ON Submain_Account.Id = Supplier.SubAccount_Id ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " UPDATE       Customer SET SubAccount_Id = Submain_Account.Id FROM            Submain_Account INNER JOIN                          Customer ON Submain_Account.Id = Customer.SubAccount_Id ";
            }
            catch
            {
            }
            try
            {
                str = @$"CREATE TABLE [dbo].[Price_Purhase_Type_List]( [Id] [bigint] {IDENTITY} NOT NULL, [Price_Name] [nvarchar](100) NULL, [IS_Active] [int] NULL      DEFAULT ((1)), [Price_Type_List_Master_Id] [int] NULL      DEFAULT ((0)) ) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"CREATE TABLE [dbo].[Textile_Production_Rate_Atlat]( [Id] [bigint] {IDENTITY} NOT NULL, [Textile_Production_Rate_Id] [bigint] NULL, [Atalat_Id] [bigint] NULL, [Atalat_Name] [nvarchar](50) NULL, [Hour_No] [decimal](18, 2) NULL, [Atla_Value] [decimal](18, 2) NULL, [Hour_Price_Value] [decimal](18, 2) NULL ) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"CREATE TABLE [dbo].[Textile_Production_Rate_ItemDetails]( [Id] [bigint] {IDENTITY} NOT NULL, [Textile_Production_Rate_Id] [bigint] NULL, [Item_PriceUnit_List_Id] [bigint] NULL, [Item_Name] [nvarchar](50) NULL, [Nazalat_Prod] [decimal](18, 2) NULL, [Hadafat_Prod] [decimal](18, 2) NULL, [Hadafat_Prod_Net] [decimal](18, 2) NULL, [Dasta_No_In_Wardia] [decimal](18, 2) NULL, [Item_No_In_Wardia] [decimal](18, 2) NULL, [Hadafat_Matluba] [decimal](18, 2) NULL, [Nazalat_Matluba] [decimal](18, 2) NULL, [Hadafat_Zeiada] [decimal](18, 2) NULL, [Nazalat_Zeiada] [decimal](18, 2) NULL ) ";
                opp.insert_me(str);
            }
            catch
            {
            }

            try
            {
                str = " UPDATE       Account_Entry_Details SET                Account_Name = Submain_Account.SubAccount_Name FROM            Account_Entry_Details INNER JOIN                          Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                AlterBD_(ref opp, "Account_Entry_List", "Journal_Name", "nvarchar(300)");
                str = " UPDATE       Account_Entry_List SET                Journal_Name = Journal_Codes.Journal_Name FROM            Account_Entry_List INNER JOIN                          Journal_Codes ON Account_Entry_List.journal_Code = Journal_Codes.Code ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "UPDATE       Account_Branches SET                Branch_Name = N'نيوسكاى -فرع  طلعت حرب' WHERE        (Id = 301) AND (Branch_Name = N'نيوسكاى -فرع  طلعت جرب')";
                opp.insert_me(str);
            }
            catch
            {
            }


            try
            {
                str = "       CREATE TABLE  [Account_Entry_Master_TransactionTemp]( [Last_Balance_Zero] [bigint] NULL, [Submain_Account_ID] [bigint] NULL, [Account_Name] [nvarchar](100) NULL, [Final_Fist_Balance_Madin] [decimal](18, 3) NULL DEFAULT ((0)), [Final_Fist_Balance_Da2n] [decimal](18, 3) NULL DEFAULT ((0)), [Final_Last_Balance_Madin] [decimal](18, 3) NULL DEFAULT ((0)), [Final_Last_Balance_Da2n] [decimal](18, 3) NULL DEFAULT ((0)), [Final_Transaction_Madin] [decimal](18, 3) NULL DEFAULT ((0)), [Final_Transaction_Da2n] [decimal](18, 3) NULL DEFAULT ((0)), [SubAccount_NO] [nvarchar](100) NULL, [Code] [nvarchar](100) NULL, [Account_Code] [nvarchar](100) NULL, [MainAccount_Name] [nvarchar](100) NULL, [MainAccount_NO] [nvarchar](100) NULL, [Main_Account_Id] [bigint] NULL, [MainCode] nvarchar(100) NULL, [Equivalent_First_Balance_Madin] [decimal](18, 3) NULL DEFAULT ((0)), [Equivalent_First_Balance_Da2n] [decimal](18, 3) NULL DEFAULT ((0)), [First_Balance_Madin] [decimal](18, 3) NULL DEFAULT ((0)), [First_Balance_Da2n] [decimal](18, 3) NULL DEFAULT ((0)), [Equivalent_Account_Currency_Id] [int] NULL DEFAULT ((0)), [Final_Import_Value] [decimal](18, 3) NULL DEFAULT ((0)), [Final_Export_Value] [decimal](18, 3) NULL DEFAULT ((0)), [Prev_Import_Value] [decimal](18, 3) NULL      DEFAULT ((0)), [Prev_Export_Value] [decimal](18, 3) NULL      DEFAULT ((0)) ) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE  [Account_Entry_Master_BalanceFirstAccount_Temp]( [Id] [bigint] {IDENTITY} NOT NULL, [Submain_Account_ID] [bigint] NULL, [SubAccount_Name] [nvarchar](500) NULL, [Account_Currency_Id] [bigint] NULL )  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Account_Entry_Master_Balance_MezanTemp](
[Id] [bigint] NULL,
	[Last_Balance_Zero] [bigint] NULL,
	[Submain_Account_ID] [bigint] NULL,
	[Account_Name] [nvarchar](100) NULL,
	[Final_Fist_Balance_Madin] [decimal](18, 3) NULL,
	[Final_Fist_Balance_Da2n] [decimal](18, 3) NULL,
	[Final_Last_Balance_Madin] [decimal](18, 3) NULL,
	[Final_Last_Balance_Da2n] [decimal](18, 3) NULL,
	[Final_Transaction_Madin] [decimal](18, 3) NULL,
	[Final_Transaction_Da2n] [decimal](18, 3) NULL,
	[SubAccount_NO] [nvarchar](100) NULL,
	[Code] [nvarchar](100) NULL,
	[Account_Code] [nvarchar](100) NULL,
	[MainAccount_Name] [nvarchar](100) NULL,
	[MainAccount_NO] [nvarchar](100) NULL,
	[Main_Account_Id] [bigint] NULL,
	[MainCode] [nvarchar](100) NULL,
	[Equivalent_First_Balance_Madin] [decimal](18, 3) NULL,
	[Equivalent_First_Balance_Da2n] [decimal](18, 3) NULL,
	[First_Balance] [decimal](18, 3) NULL,
	[First_Balance_Madin] [decimal](18, 3) NULL,
	[First_Balance_Da2n] [decimal](18, 3) NULL,
	[Equivalent_Account_Currency_Id] [int] NULL,
	[Final_Import_Value] [decimal](18, 3) NULL,
	[Final_Export_Value] [decimal](18, 3) NULL,
	[Prev_Import_Value] [decimal](18, 3) NULL,
	[Prev_Export_Value] [decimal](18, 3) NULL,
	[Year_Financial_Period_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Account_Currency_Id] [int] NULL,
	[AccountCode1] [int] NULL,
	[AccountCode2] [int] NULL,
	[AccountCode3] [int] NULL,
	[AccountCode4] [int] NULL,
	[AccountCode5] [int] NULL,
	[AccountCode6] [int] NULL,
	[Account_Follows] [int] NULL,
	[Totals] [decimal](18, 3) NULL,
	[OperationDate] [datetime] NULL,
	[User_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[Update_ServerDateTime] [datetime] NULL,
	[EditOperationDate] [datetime] NULL,
	[Edit_User_Id] [bigint] NULL,
	[ProcessNo] [bigint] NULL,
	[Head_Main_Account] [int] NULL
) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"  CREATE TABLE [dbo].[Banks_Balance]( [Id] [bigint] {IDENTITY} NOT NULL, [Bank_Id] [int] NULL, [Balance_Date] [datetime] NULL, [First_Balance] [decimal](18, 2) NULL, [Last_Balance] [decimal](18, 2) NULL )  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Production_Atalat]( [Id] [bigint] {IDENTITY} NOT NULL , [Atalat_Name] [nvarchar](100) NULL, [Hour_Price_Value] [decimal](18, 2) NULL, [Types] [int] NULL , PRIMARY KEY ( Id)   )  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"CREATE TABLE [dbo].[Account_Branches]( [Id] [bigint] NULL,
[Branch_Name] [nvarchar](50) NULL, [Address] [nvarchar](50) NULL, [Code] [nvarchar](50) NULL ) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"CREATE TABLE  [Order_Working_Textile_Details]( [Id] [bigint] {IDENTITY} NOT NULL, [Order_Working_Textile_Id] [bigint] NULL, [Nazalat_Count] [int] NULL, [Color_Name] [nvarchar](50) NULL, [Items_TypesColor_Name_Id] [int] NULL, [Weight] [decimal](18, 2) NULL, PRIMARY KEY ( Id) )";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Items_Color_Name](
    [Id][bigint] {IDENTITY} NOT NULL,

    [Color_Name] [nvarchar](50) NULL,
	[OperationDate] [datetime] NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[Update_ServerDateTime] [datetime] NULL,
	[EditOperationDate] [datetime] NULL,
	[Edit_User_Id] [bigint] NULL,
	[ProcessNo] [bigint] NULL )   ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " CREATE TABLE  [Items_Types_Name]( [Id] [bigint] {IDENTITY} NOT NULL, [ItemsType_Name] [nvarchar](50) NULL, PRIMARY KEY ( Id) )";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " CREATE TABLE  [Items_Bodies_Name]( [Id] [bigint] {IDENTITY} NOT NULL, [ItemBody_Name] [nvarchar](50) NULL, PRIMARY KEY ( Id) )";
                opp.insert_me(str);
            }
            catch
            {
            }
            AlterBD_(ref opp, "Textile_Production_Rate", "Hadafat_Matluba", "decimal(18, " + d + ")", "0");
            AlterBD_(ref opp, "Textile_Production_Rate", "Nazalat_Matluba", "decimal(18, " + d + ")", "0");
            AlterBD_(ref opp, "Textile_Production_Rate", "Hadafat_Zeiada", "decimal(18, " + d + ")", "0");
            AlterBD_(ref opp, "Textile_Production_Rate", "Nazalat_Zeiada", "decimal(18, " + d + ")", "0");
            AlterBD_(ref opp, "USER_DETAILES", "Active", "int", "0", "1");


            AlterBD_(ref opp, "Account_Type_Main_Account_Id", "Account_Type_Name", "nvarchar(100)");
            AlterBD_(ref opp, "Account_Type_Main_Account_Id", "Account_Name", "nvarchar(100)");
            AlterBD_(ref opp, "Account_Type_Main_Account_Id", "Account_Follows", "Int", "0", "0");
            AlterBD_(ref opp, "Account_Type_Main_Account_Id", "Account_Type_ValueGeneralCustomer", "Int", "0", "0");
            AlterBD_(ref opp, "Submain_Account", "Account_Type_ValueGeneralCustomer", "int", "0", "0");


            AlterBD_(ref opp, "Account_Type", "Account_Follows", "Int", "0", "0");


            //AlterBD_(ref opp, "Order_Money", "Serial", "bigint", "0");
            //AlterBD_(ref opp, "Money_Transaction_Box", "Serial", "bigint", "0");
            AlterBD_(ref opp, "Financial_Period_Begining_Balance", "Account_Type_Value", "int", "0");
            AlterBD_(ref opp, "Financial_Period_Begining_Balance", "Branch_Id", "int", "0");
            AlterBD_(ref opp, "Banks", "Account_Type_Value", "int", "0");
            AlterBD_(ref opp, "Banks", "CompanyLogoType", "int", "0", "0");
            AlterBD_(ref opp, "Banks", "Branch_Name", "nvarchar(100)");
            AlterBD_(ref opp, "Banks", "Bank_Name_En", "nvarchar(200)", "0", " 'Bank_' + CAST(id AS VARCHAR(200))");
            AlterBD_(ref opp, "FORMS_DETAILES", "IS_Menu_Saved", "int", "0");
            AlterBD_(ref opp, "Item", "Item_Category_ID", "int", "0");

            AlterBD_(ref opp, "Customer", "price_type_list_Id", "int", "0");
            AlterBD_(ref opp, "USER_DETAILES", "Name", "nvarchar(50)");
            AlterBD_(ref opp, "Account_Entry_List", "IS_Make_AutomaticFromTar7eel", "int", "0");



            AlterBD_(ref opp, "Money_Transaction_Box", "Purchase_Ticket_Value", "decimal(18, " + d + ")", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "Commission_Value", "decimal(18, " + d + ")", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "User_Id", "int", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "Branch_Id", "int", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "Tafkeet_Value", "nvarchar(100)");
            AlterBD_(ref opp, "Money_Transaction_Box", "TotalValueWillPaid", "decimal(18, " + d + ")", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "Rest_Value", "decimal(18, " + d + ")", "0");
            AlterBD_(ref opp, "Money_Transaction_Box", "Account_Cost_Center_Tree_Id", "int", "0");
            AlterBD_(ref opp, "Order_Money", "Year_Financial_Period_Id", "int", "0");
            AlterBD_(ref opp, "Order_Money", "Ticket_Supplier_Id", "int", "0");
            AlterBD_(ref opp, "Order_Money", "Order_Tafkeet_Value", "nvarchar(100)");
            AlterBD_(ref opp, "Order_Money", "Order_Notes", "nvarchar(500)");
            AlterBD_(ref opp, "Order_Money", "Sandouk_Cost_Center_Tree_Id", "int", "0");
            AlterBD_(ref opp, "Customer_Type", "Supplier", "int", "0");

            AlterBD_(ref opp, "Checks_Memory", "SubAccount_Name_RelatedToBefor", "nvarchar(100)");

            AlterBD_(ref opp, "Checks_Memory", "SubAccount_Id_RelatedToBefor", "bigint", "0");



            try
            {
                str = " Alter Table Order_Money  Add  Tafkeet_Value nvarchar(100)    ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Order_Money  Add  Year_Financial_Period_Id bigint  Default(1) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                opp.insert_me(" Alter Table Financial_Period  Add  IS_Active int  Default(0) ");
                str = " Alter Table Financial_Period  Add  IS_Finished_Posted int  Default(0) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Machin_list_Textile  Add  Machine_Stitch_No decimal(18, 3)  Default(0)";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Machin_list_Textile  Add  Price_Day_Alf_Hadfa decimal(18, 3)  Default(0)";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Textile_Production_Rate  Add  Price_Alf_Hadfa decimal(18, 3)  Default(0)";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Employee_Production_Rate  Add  Dasta_No decimal(18, 3)  Default(0)";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                opp.insert_me(" Alter Table Item  Add  Unit_Id bigint  Default(0) ");
                str = "UPDATE    Item SET              Unit_Id = Unit_item.unit_id FROM         Item INNER JOIN                       Unit_item ON Item.id = Unit_item.Item_id";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Work_Out_Details_Received  Add  Item_Id int  Default(0) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Work_Out_Details_Balance  Add  Item_Id int  Default(0) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Work_Out_Details  Add  Item_Id int  Default(0) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Employee_Production_Rate  Add  Dasta_No decimal](18, 2)  Default(0) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Machin_list_Textile  Add  Machine_Stitch_No decimal](18, 2)  Default(0) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Textile_Process_Transaction  Add  Fa7s_Result_Id int  Default(-1) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Textile_Process_Transaction  Add  Fa7s_Result_Name nvarchar(50)  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Textile_Process_Transaction  Add  Notes nvarchar(100)  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Textile_Process_Transaction  Add  Item_Type int  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Textile_Process_Transaction  Add  Dept_Id_Transaction int  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Textile_Item_Dept_Balance Add  Item_Type int Default(1)";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                opp.insert_me(" Alter Table Item_Category Add  IS_Active int Default(1)");
                str = " Update Item_Category set  IS_Active=1";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                opp.insert_me(" Alter Table Item_Category Add  Is_ItemOrService int Default(0)");
                str = " Update Item_Category set  Is_ItemOrService=0";
                opp.insert_me(str);
            }
            catch
            {
            }

            try
            {
                opp.insert_me(" Alter Table Item_Category Add  Is_Default int Default(0)");
                str = " Update Item_Category set  Is_Default=0";
                opp.insert_me(str);
            }
            catch
            {
            }

            try
            {
                str = " Alter Table Textile_Production_Rate Add  Item_Actuall_Weight_Total  decimal(18, 3) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Department Add Dept_Id  bigint  Default(0) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Textile_Production_Rate Add Item_Send_To  int  Default(0)   Alter Table Textile_Production_Rate Add Item_Send_To_Str   nvarchar(50) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                opp.insert_me(" Alter Table Price_Type_List Add IS_Active  int  Default(1)  ");
                str = " Update Price_Type_List set IS_Active=1 ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Store Add IS_ItemSecond_degree  int  Default(0)  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Textile_Production_Rate Add Item_Type_Str nvarchar(50) Alter Table Textile_Production_Rate Add Item_Type_Id int  Default(0)  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Textile_Production_Rate drop  column  Production_weigh_In_Wardia    ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Textile_Production_Rate Add Item_Actuall_Weight decimal(18, 3) Default(0)  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Textile_Production_Rate Add Nazary_Production_weigh_In_Wardia decimal(18, 3) Default(0)  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Textile_Production_Rate Add Actually_Production_weigh_In_Wardia decimal(18, 3) Default(0)  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Item Add Upon_Request decimal(18, 3) Default(0) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "   Alter Table Store  Add IS_Sell_SecondUnit int  default(0)   ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " Alter Table Store  Add Store_Code nvarchar(50)  Alter Table Store  Add IS_Default_Store int  default(0) Alter Table Store  Add IS_Sell_SecondUnit int  default(0)   ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "Alter Table Bills_Details  Add Order_Purshing_Id_Automatic numeric(18, 0)  default(0)  Alter Table Bills_Details_Reverse  Add Order_Purshing_Id_Automatic numeric(18, 0)  default(0)  Alter Table Invoices_Details  Add Order_Purshing_Id_Automatic numeric(18, 0)  default(0)  Alter Table Invoices_Details_Reverse  Add Order_Purshing_Id_Automatic numeric(18, 0)  default(0)   ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "Alter Table Bills_Details  Add Store_Id int  default(0)  Alter Table Bills_Details_Reverse  Add Store_Id int  default(0)  Alter Table Invoices_Details  Add Store_Id int  default(0)  Alter Table Invoices_Details_Reverse  Add Store_Id int  default(0)   ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "Alter Table Item_ImagesStore  Add Default_Img_Flage bit default(0) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                opp.insert_me("Alter table Customer Add General_Customer_Flage int default(0) ");
                opp.insert_me("Update  Customer set General_Customer_Flage=0   ");
                str = "Update  Customer set General_Customer_Flage=1 where id=180 or id=102 ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                opp.insert_me(@$" CREATE TABLE [dbo].[Item_Main_Category]( [ID] [bigint] {IDENTITY} NOT NULL, [Main_Category_Name] [nvarchar](50) COLLATE Arabic_CI_AI_KS_WS NULL, [Category_Type_Id] [int] NULL, [Category_Type_Str] [nvarchar](50) COLLATE Arabic_CI_AI_KS_WS NULL    ) ");
                opp.insert_me("Insert into Item_Main_Category(Main_Category_Name,Category_Type_Id,Category_Type_Str) Values('وبرة',1,'مبيعات')");
                str = "Insert into Item_Main_Category(Main_Category_Name,Category_Type_Id,Category_Type_Str) Values('كوفرتة',1,'مبيعات')";

                str = ExecuteInsertWithIdentityCheck("Item_Main_Category", str);
                opp.insert_me(str);
            }
            catch (Exception)
            {
            }
            try
            {
                str = " alter table Item_Category Add Main_Item_Category_Id int  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = (" alter table Item_PriceUnit_List add Hadafat_Tash3heel decimal(18, 3)  " + " alter table Item_PriceUnit_List add Item_Width decimal(18, 3) ") + " alter table Item_PriceUnit_List add Item_Length  decimal(18, 3) " + " alter table Item_PriceUnit_List add Item_Weight_Per_Square_meter  decimal(18, 3)  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " alter table supplier add Tax_Registration_No nvarchar(100) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " alter table Currency add Currency_Name_EN  nvarchar(50)";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " alter table Currency add Value  int";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " alter table Submain_Account add Account_Code nvarchar(50)  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "alter table Submain_Account add Company_ID_For_GL  int Default(0) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "alter table Submain_Account add Company_Name_For_GL  nvarchar(50) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "alter table Submain_Account add Account_Customer_Type_Value int Default(0) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "alter table Submain_Account add Account_Customer_Type_ID int Default(0) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " alter table Submain_Account add Transform_Type int Default(0)";
                opp.insert_me(str);
            }
            catch
            {
            }

            try
            {
                str = "alter table Submain_Account add Branch_ID int Default(0) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = " alter table Submain_Account add ConnectWith_Gl_Flage int Default(0) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "CREATE TABLE [dbo].[Cost_Center_Tree]( [ID] [bigint] NULL, [Code] [nvarchar](50) COLLATE Arabic_CI_AI_KS_WS NULL, [Name] [nvarchar](50) COLLATE Arabic_CI_AI_KS_WS NULL, [Cost_Type] [int] NULL ) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "CREATE TABLE [dbo].[Journal_Codes]( [ID] [bigint] {IDENTITY} NOT NULL, [Journal_Name] [nvarchar](50) COLLATE Arabic_CI_AI_KS_WS NULL, [Code] [bigint] NULL )  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "CREATE TABLE [dbo].[Account_Entry_List]( [ID] [bigint] {IDENTITY} NOT NULL, [Currency_ID] [bigint] NULL, [Entry_No] [bigint] NULL, [Entry_Date] [datetime] NULL, [journal_Code] [bigint] NULL, [Entry_Notes] [nvarchar](50) COLLATE Arabic_CI_AI_KS_WS NULL, [User_ID_Login] [bigint] NULL, [Branch_ID_Login] [bigint] NULL, [Entry_Total] [decimal](18, 0) NULL, [Transform_Type] [int] NULL, Year_Financial_Period_Id int NULL, [Transfer_To_Gl_Flage] [int] NULL DEFAULT ((0)) )  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Account_Entry_Details]( [ID] [bigint] {IDENTITY} NOT NULL, [Account_Entry_List_ID] [bigint] NULL, [Submain_Account_ID] [bigint] NULL, [SubAccount_NO] [bigint] NULL, [Import_Value] [decimal](18, 0) NULL, [Export_Value] [decimal](18, 0) NULL, [Transaction_Value] [decimal](18, 0) NULL, [Notes_Details] [nvarchar](50) COLLATE Arabic_CI_AI_KS_WS NULL, [Equivalent_Currency_Value] [decimal](18, 0) NULL, [Transaction_Type] [int] NULL, [Account_Code] [bigint] NULL, [Account_Customer_Type_Value] [int] NULL, [Account_Customer_Type_ID] [int] NULL,  [Transaction_DateFrom] [datetime] NULL, [Transaction_DateTo] [datetime] NULL, [Equivalent_Import_Value] [decimal](18, 0) NULL DEFAULT ((0)), [Equivalent_Export_Value] [decimal](18, 0) NULL DEFAULT ((0)), [Equivalent_Transaction_Value] [decimal](18, 0) NULL DEFAULT ((0)), [Currency_ID_Primary] [bigint] NULL DEFAULT ((0)), [Currency_ID_Secondary] [bigint] NULL DEFAULT ((0)), [CostCenter_Id] [int] NULL DEFAULT ((0)), [CostCenter_Name] [nvarchar](50) COLLATE Arabic_CI_AI_KS_WS NULL        )  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            //try
            //{
            //    str = "CREATE TABLE [dbo].[Account_Type]( [ID] [bigint] {IDENTITY} NOT NULL, [Name] [nvarchar](50) COLLATE Arabic_CI_AI_KS_WS NULL, [Value] [int] NULL,[Active] [int] NULL ) ON [PRIMARY]";
            //    opp.insert_me(str);
            //}
            //catch
            //{
            //}
            try
            {
                str = @$" CREATE TABLE [dbo].[Unit_Item_Balance_Date_Details]( [ID] [bigint] {IDENTITY} NOT NULL, [Unit_Item_Balance_Date_List_Id] [bigint] NULL, [Balance_Date] [datetime] NULL, [Item_Id] [bigint] NULL, [Unit_Item_Id] [bigint] NULL, [Large_Balance] [decimal](18, 2) NULL, [Small_Balance] [decimal](18, 2) NULL, [Item_No] [decimal](18, 2) NULL )";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                opp.insert_me("alter table Invoices_List add User_Id bigint default(0) ");
                opp.insert_me("alter table Invoices_List add Branch_Id bigint default(0) ");
                opp.insert_me("alter table Invoices_List_Reverse add User_Id bigint default(0) ");
                opp.insert_me("alter table Invoices_List_Reverse add Branch_Id bigint default(0) ");
                opp.insert_me("alter table Bills_List add User_Id bigint default(0) ");
                opp.insert_me("alter table Bills_List add Branch_Id bigint default(0) ");
                opp.insert_me("alter table Bills_List_Reverse add User_Id bigint default(0) ");
                opp.insert_me("alter table Bills_List_Reverse add Branch_Id bigint default(0) ");
                opp.insert_me("alter table Order_Money add User_Id bigint default(0) ");
                str = "alter table Order_Money add Branch_Id bigint default(0) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "alter table Banks add Static_Flage int default(0) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "alter table Customer add Static_Flage int default(0) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "alter table Unit_item add TaxSales10_Value decimal(18, 3) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "alter table Employee_Production_Rate add Wardia_Id int ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "alter table Employee_Production_Rate add Machine_Id int ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"CREATE TABLE [dbo].[Detailes_Purshing_Import_Export]( [Id] [bigint] {IDENTITY} NOT NULL, Detailes_Purshing_Id [bigint] NOT NULL, [OrderTable_id] [bigint] NOT NULL, [Unit_item_Table_Id] [bigint] NULL, [qua] [bigint] NOT NULL, [Part_qua] [int] NOT NULL, [Item_No] [float] NULL, [Balance_Larg_Qnty] [float] NULL, [Balance_Small_Qnty] [float] NULL, [Balance_Item_No] [float] NULL, [Notes_From_To] [nvarchar](50) NULL, [Notes] [nvarchar](500) NULL, [Machine_Id] [int] NULL DEFAULT ((0)), [Transaction_Type] [int] NULL )";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "Alter Table Item_PriceUnit_List add Abalek_No numeric(18, 2) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "Alter Table Item_PriceUnit_List add Ableka_Price numeric(18, 2) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "Alter Table Customer add Tax_No nvarchar(100) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                opp.insert_me("Alter Table Banks add Default_Bank int default(0)");
                opp.insert_me("Update Banks set Default_Bank=0");
                opp.insert_me("Alter Table Order_Money add Invoice_Id numeric(18, 0) default(1)");
                opp.insert_me("Update Order_Money set Invoice_Id=0");
                opp.insert_me("Alter Table Supplier add Balance_Air decimal(18, 3) default(1)");
                opp.insert_me("Update Supplier set Balance_Air=1");
                opp.insert_me("Alter Table Supplier add Balance_Road  decimal(18, 3) default(1)");
                opp.insert_me("Update Supplier set Balance_Road=1");
                opp.insert_me("Alter Table USER_DETAILES add Balance_Air decimal(18, 3) default(1)");
                opp.insert_me("Update USER_DETAILES set Balance_Air=1");
                opp.insert_me("Alter Table USER_DETAILES add Balance_Road  decimal(18, 3) default(1)");
                opp.insert_me("Update USER_DETAILES set Balance_Road=1");
                opp.insert_me("Alter Table Customer add Balance_Air decimal(18, 3) default(1)");
                opp.insert_me("Update Customer set Balance_Air=1");
                opp.insert_me("Alter Table Customer add Balance_Road  decimal(18, 3) default(1)");
                opp.insert_me("Update Customer set Balance_Road=1");
                opp.insert_me("Alter Table Customer add Account_Code int default(1)");
                opp.insert_me("Update Customer set Account_Code=1");
                opp.insert_me("Alter Table Supplier add Account_Code int default(1)");
                opp.insert_me("Update Supplier set Account_Code=1");
                opp.insert_me("Alter Table Banks add Account_Code int default(1)");
                opp.insert_me("Update Banks set Account_Code=1");
                opp.insert_me("Alter Table Customer add Currency_Id int default(1)");
                opp.insert_me("Update Customer set Currency_Id=1");
                opp.insert_me("Alter Table Supplier add Currency_Id int default(1)");
                str = "Update Supplier set Currency_Id=1";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                opp.insert_me("Alter Table Detailes_Purshing add Notes_From nvarchar(50)");
                opp.insert_me("Alter Table Detailes_Purshing add Notes_To nvarchar(50)");
                str = "Alter Table Detailes_Purshing add Notes nvarchar(500)";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"CREATE TABLE [dbo].[Item_Images]( [ID] [int] {IDENTITY} NOT NULL,                     [Item_Id] [int] NULL,                     [Picture] [image] NULL,                     [Description] [nvarchar](100) NULL                     ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                opp.insert_me("Alter Table Bills_Details_Reverse add Invoice_Item_Name nvarchar(100)");
                opp.insert_me("Alter Table Invoices_Details add Invoice_Item_Name nvarchar(100)");
                opp.insert_me("Alter Table Invoices_Details_Reverse add Invoice_Item_Name nvarchar(100)");
                str = "Alter Table Bills_Details add Invoice_Item_Name nvarchar(100)";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                opp.insert_me("Alter Table Submain_Account add Account_Nature_Type int default(1)");
                opp.insert_me("Alter Table Submain_Account add Accept_Cost_Center_Flage  int default(0)");
                opp.insert_me("Alter Table Submain_Account add Cost_Center_Tree_ID int default(0)");
                str = @$"CREATE TABLE [dbo].[Cost_Center_Tree]( [ID] [bigint] {IDENTITY} NOT NULL, [Code] [nvarchar](50) NULL, [Name] [nvarchar](50) NULL, [Cost_Type] [int] NULL ) ON [PRIMARY]";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "Alter Table Submain_Account add Account_Level int default(1)";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "Alter Table Machin_list_Textile add Machine_Id int default(0)";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                //opp.insert_me("CREATE TABLE [dbo].[OrderTypes]( [ID] [bigint] {IDENTITY} NOT NULL, [Order_Name] [nvarchar](100) NULL, [OrderTypes_Value] [int] NULL, [BtnRecive_Text] [nvarchar](50) NULL, [Order_Estlam_Sarf] [int] NULL ) ON [PRIMARY]");
                str = "UPDATE Order_Purshing SET                Order_Type = - 1 WHERE        (type_From = - 1) AND (Id_From = - 1) AND (Type_To = - 1) AND (Id_To = - 1)";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                //opp.insert_me("CREATE TABLE [dbo].[OrderTypes_Label]( [TypesID] [int] NULL, [LabelFrom] [nvarchar](50) NULL, [LabelTo] [nvarchar](50) NULL, [Label_Name] [nvarchar](50) NULL ) ON [PRIMARY]");
                opp.insert_me("UPDATE Order_Purshing SET                Order_Type = - 1 WHERE        (type_From = - 1) AND (Id_From = - 1) AND (Type_To = - 1) AND (Id_To = - 1)");
            }
            catch
            {
            }
        }
        public static void AlterDBVersion(ref int Excp, ref string Msg, int d)
        {
            operations opp = new operations();
            string str = "";
            string Database = opp.conn.Database.ToString();

            try
            {
                str = $" ALTER DATABASE  \"{Database}\" SET COMPATIBILITY_LEVEL =    110   ";
                opp.insert_me(str);
                Excp = 0;
                Msg = "DB Altered";
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = str + "\n\r" + ee.Message;
            }

        }
        public static void UpdateItem_ProcessedFlage()
        {
            operations opp = new operations();
            string str = "";


            try
            {
                str = @$" UPDATE       Bills_Details
SET                Item_ProcessedFlage = 100, Item_DeletedFlage = 100 ";

                str = str + " UPDATE       Invoices_Details SET                Item_ProcessedFlage = 100, Item_DeletedFlage = 100  ";
                str = str + "  UPDATE        Bills_Details_Reverse SET                Item_ProcessedFlage = 100, Item_DeletedFlage = 100  ";
                str = str + "  UPDATE           Invoices_Details_Reverse SET                Item_ProcessedFlage = 100, Item_DeletedFlage = 100  ";
                str = str + "  ";
                opp.insert_me(str);
            }
            catch
            {
            }
        }
        public static void AlterDB(int d)
        {
            operations opp = new operations();
            string str = "";


            try
            {
                str = @$" ";
                opp.insert_me(str);
            }
            catch
            {
            }



            try
            {
                str = @$" UPDATE Unit_item
SET TaxSales10_Value = 0
WHERE(TaxSales10_Value IS NULL) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" UPDATE       LookUp_CheckStatus
SET                IsProcessUsedToChange=1 
WHERE       ID=8  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" UPDATE       Checks_Memory
SET                Receipt=1 
WHERE       Receipt=0  ";
                opp.insert_me(str);
            }
            catch
            {
            }

            try
            {
                str = @$"  CREATE TABLE [dbo].[ShipmentCompany](
	[Id] [bigint] {IDENTITY}  primary key,
	[Name] [nvarchar](500) NULL
	)";
                opp.insert_me(str);
            }
            catch
            {
            }


            TruncateALogError(ref opp);
            //AlterDBLastYear(ref opp);
            AlterDBThisYear(ref opp, d);
            StockEngineSchemaCLS.EnsureSchema();
        }


        public static void ChangeType(ref operations opp, string DATA_TYPE, int d)
        {
            string mysql = @"SELECT 
TABLE_CATALOG,
TABLE_SCHEMA,
TABLE_NAME, 
COLUMN_NAME, 
DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS Where DATA_TYPE='" + DATA_TYPE + "'";

            DataTable dt = opp.return_DataTable(mysql);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string TABLE_NAME = dt.Rows[i]["TABLE_NAME"].ToString();
                string COLUMN_NAME = dt.Rows[i]["COLUMN_NAME"].ToString();
                DATA_TYPE = dt.Rows[i]["DATA_TYPE"].ToString();

                if (DATA_TYPE == "numeric")
                {
                    AlterColumn(ref opp, TABLE_NAME, COLUMN_NAME, "bigint");
                }
                if (DATA_TYPE == "float")
                {
                    AlterColumn(ref opp, TABLE_NAME, COLUMN_NAME, "decimal(18, " + d + ")");
                }
                if (DATA_TYPE == "decimal")
                {
                    AlterColumn(ref opp, TABLE_NAME, COLUMN_NAME, "decimal(18, " + d + ")");
                }
            }
        }
        public static void ChangeTypeToLongUserId(ref operations opp)
        {
            string mysql = @$" SELECT 
    TABLE_CATALOG,
    TABLE_SCHEMA,
    TABLE_NAME, 
    COLUMN_NAME, 
    DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE DATA_TYPE ='int' and  COLUMN_NAME IN (
    'Currency_Id',
    'User_Id',
    'Branch_Id',
    'Edit_User_Id',
    'Entry_Currency_ID',
    'Year_Financial_Period_Id',
    'User_ID_Login',
    'Branch_ID_Login',
    'User_ID_LoginUpdate',
    'Machine_Id',
    'Account_Currency_ID',
    'SalesMan_Id'
) ";
              
            DataTable dt = opp.return_DataTable(mysql);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string TABLE_NAME = dt.Rows[i]["TABLE_NAME"].ToString();
                string COLUMN_NAME = dt.Rows[i]["COLUMN_NAME"].ToString();
                string DATA_TYPE = dt.Rows[i]["DATA_TYPE"].ToString();

                if (DATA_TYPE == "int")
                {
                    AlterColumn(ref opp, TABLE_NAME, COLUMN_NAME, "bigint");
                }

            }


        }

        static void SetKeys(ref operations opp)
        {
            string str = "";

            try
            {
                str = @$"  ";
                opp.insert_me(str);

                //str = @$"  ";
                //opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"  
ALTER TABLE [dbo].[SysConfig] 
ALTER COLUMN [Id] [bigint] NOT NULL;

 ALTER TABLE [dbo].[SysConfig] 
ADD CONSTRAINT [PK_SysConfig_Id] PRIMARY KEY CLUSTERED ([Id] ASC); ";
                opp.insert_me(str);


            }
            catch
            {
            }
            try
            {
                str = @$"  
ALTER TABLE [dbo].[SysConfigMenus] 
ALTER COLUMN [Id] [bigint] NOT NULL;

 ALTER TABLE [dbo].[SysConfigMenus] 
ADD CONSTRAINT [PK_SysConfigMenus_Id] PRIMARY KEY CLUSTERED ([Id] ASC); ";
                opp.insert_me(str);


            }
            catch
            {
            }
        }
        public static  string ExecuteInsertWithIdentityCheck(string tableName, string insertCommand)
        {
            string sql = Common.Use_Auto_Identity_Column == 1
                ? $"SET IDENTITY_INSERT {tableName} ON; {insertCommand} SET IDENTITY_INSERT {tableName} OFF;"
                : insertCommand;
            return sql;
           // opp.insert_me(sql);
        }
        static void CreateTable(ref operations opp)
        {
            string str = "";

            try
            {
                str = @$"  ";
                opp.insert_me(str);

                //str = @$"  ";
                //opp.insert_me(str);
            }
            catch
            {
            }

            try
            {
                try
                {
                    str = "   ";
                    opp.insert_me(str);
                }
                catch (Exception)
                {
                }

                // الجدول Customers_Accounts_Mokhtalat IDENTITY(1,1)
                str = @$"  drop table   Customers_Accounts_Mokhtalat

CREATE TABLE [dbo].[Customers_Accounts_Mokhtalat](
	[Id] [bigint] IDENTITY(1,1) NOT  NULL,
	[Customers_AccountsId] [bigint] NULL,
	[ProcessNo] [bigint] NULL,
	[Customer_Id] [bigint] NULL,
	[Import] [decimal](18, 2) NULL,
	[Export] [decimal](18, 2) NULL,
	[Balance] [decimal](18, 2) NULL,
	[Balance_Actual] [decimal](18, 2) NULL,
	[Transaction_Type] [int] NULL,
	[Transaction_Date] [datetime] NULL,
	[Reference_Id] [bigint] NULL,
	[Reference_Type] [int] NULL,
	[Reference_Type_Table_Id] [bigint] NULL,
	[Reference_Type_TableDetails_Id] [bigint] NULL,
	[Payment_Type] [int] NULL,
	[Notes] [nvarchar](4000) NULL,
	[Day_Registration_flage] [int] NOT NULL,
	[DayReg_List_Id] [bigint] NULL,
	[Flage_View] [int] NULL,
	[BalanceImport] [nvarchar](100) NULL,
	[BalanceExport] [nvarchar](100) NULL,
	[Reference_Type_View] [nvarchar](100) NULL,
	[Item_Name] [nvarchar](100) NULL,
	[Item_Id] [bigint] NULL,
	[Larg_Qnty] [nvarchar](100) NULL,
	[Small_Qnty] [nvarchar](100) NULL,
	[Item_No] [nvarchar](100) NULL,
	[Item_Size] [nvarchar](100) NULL,
	[Qnty_Size] [nvarchar](100) NULL,
	[Qnty_value] [nvarchar](100) NULL,
	[Height_Width_Item] [nvarchar](100) NULL,
	[Larg_Unit_Price] [decimal](18, 2) NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[BalanceDiff] [decimal](18, 2) NULL,
	[WorkOrderNumber] [bigint] NULL,
	[CustomerName] [nvarchar](500) NULL,
	[CarsWorkOrdersId] [bigint] NULL,
	[TaxInternalId] [nvarchar](500) NULL,
	[CarsId] [bigint] NULL,
	[CarNumber] [nvarchar](500) NULL,
	[Customer_Id_RelatedToAccount] [bigint] NULL
)  
 ";

                opp.insert_me(str);
            }
            catch (Exception)
            {
            }

            try
            {
                str = @$" drop TABLE [dbo].[Currency]
CREATE TABLE [dbo].[Currency](
	[ID] [bigint] {IDENTITY} NOT NULL,
	[Currency_Name] [nvarchar](50) NULL,
	[Currency_Name_EN] [nvarchar](50) NULL,
	[Value] [int] NOT NULL,
	[Active] [int] NOT NULL,
	[IS_GL_Currency_Flage] [int] NULL,
	[OperationDate] [datetime] NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[Update_ServerDateTime] [datetime] NULL,
	[EditOperationDate] [datetime] NULL,
	[Edit_User_Id] [bigint] NULL,
	[ProcessNo] [bigint] NULL,
	[code] [nvarchar](500) NULL,
	[rate] [decimal](18, 2) NULL,
	[Desc_en] [nvarchar](500) NULL,
	[Desc_ar] [nvarchar](500) NULL,
	[Tax_BranchesId] [bigint] NULL
) ";
                opp.insert_me(str);

                str = @$" 
INSERT INTO [dbo].[Currency] 
(
    [ID], [Currency_Name], [Currency_Name_EN], [Value], [Active], 
    [IS_GL_Currency_Flage], [OperationDate], [User_Id], [Branch_Id], 
    [Insert_ServerDateTime], [Update_ServerDateTime], [ProcessNo], 
    [code], [rate], [Desc_en], [Desc_ar], [Tax_BranchesId]
)
VALUES
-- 1. العملة المحلية (الجنيه المصري)
(1, N'جنيه مصري', N'Egyptian Pound', 1, 1, 1, GETDATE(), 0, 1, GETDATE(), GETDATE(), 0, N'EGP', 1.00, N'Egyptian Pound', N'الجنيه المصري', 0),

-- 2. الدولار الأمريكي
(2, N'دولار أمريكي', N'US Dollar', 2, 1, 0, GETDATE(), 0, 1, GETDATE(), GETDATE(), 0, N'USD', 48.50, N'US Dollar', N'الدولار الأمريكي', 0),

-- 3. اليورو الأوروبي
(3, N'يورو', N'Euro', 3, 1, 0, GETDATE(), 0, 1, GETDATE(), GETDATE(), 0, N'EUR', 52.30, N'Euro', N'اليورو الأوروبي', 0),

-- 4. الريال السعودي
(4, N'ريال سعودي', N'Saudi Riyal', 4, 1, 0, GETDATE(), 0, 1, GETDATE(), GETDATE(), 0, N'SAR', 12.90, N'Saudi Riyal', N'الريال السعودي', 0),

-- 5. الدرهم الإماراتي
(5, N'درهم إماراتي', N'UAE Dirham', 5, 1, 0, GETDATE(), 0, 1, GETDATE(), GETDATE(), 0, N'AED', 13.20, N'UAE Dirham', N'الدرهم الإماراتي', 0),

-- 6. الدينار الكويتي
(6, N'دينار كويتي', N'Kuwaiti Dinar', 6, 1, 0, GETDATE(), 0, 1, GETDATE(), GETDATE(), 0, N'KWD', 158.00, N'Kuwaiti Dinar', N'الدينار الكويتي', 0),

-- 7. الريال القطري
(7, N'ريال قطري', N'Qatari Riyal', 7, 1, 0, GETDATE(), 0, 1, GETDATE(), GETDATE(), 0, N'QAR', 13.30, N'Qatari Riyal', N'الريال القطري', 0),

-- 8. الجنيه الإسترليني
(8, N'جنيه إسترليني', N'British Pound', 8, 1, 0, GETDATE(), 0, 1, GETDATE(), GETDATE(), 0, N'GBP', 63.80, N'British Pound', N'الجنيه الإسترليني', 0),

-- 9. الدينار الأردني
(9, N'دينار أردني', N'Jordanian Dinar', 9, 1, 0, GETDATE(), 0, 1, GETDATE(), GETDATE(), 0, N'JOD', 68.40, N'Jordanian Dinar', N'الدينار الأردني', 0),

-- 10. الدينار البحريني
(10, N'دينار بحريني', N'Bahraini Dinar', 10, 1, 0, GETDATE(), 0, 1, GETDATE(), GETDATE(), 0, N'BHD', 128.70, N'Bahraini Dinar', N'الدينار البحريني', 0),

-- 11. الريال العماني
(11, N'ريال عماني', N'Omani Rial', 11, 1, 0, GETDATE(), 0, 1, GETDATE(), GETDATE(), 0, N'OMR', 126.00, N'Omani Rial', N'الريال العماني', 0),

-- 12. الدينار الليبي
(12, N'دينار ليبي', N'Libyan Dinar', 12, 1, 0, GETDATE(), 0, 1, GETDATE(), GETDATE(), 0, N'LYD', 10.10, N'Libyan Dinar', N'الدينار الليبي', 0),

-- 13. الدينار التونسي
(13, N'دينار تونسي', N'Tunisian Dinar', 13, 1, 0, GETDATE(), 0, 1, GETDATE(), GETDATE(), 0, N'TND', 15.60, N'Tunisian Dinar', N'الدينار التونسي', 0),

-- 14. الدرهم المغربي
(14, N'درهم مغربي', N'Moroccan Dirham', 14, 1, 0, GETDATE(), 0, 1, GETDATE(), GETDATE(), 0, N'MAD', 4.90, N'Moroccan Dirham', N'الدرهم المغربي', 0),

-- 15. جرام الذهب (عيار 24)
(15, N'جرام ذهب', N'Gold Gram', 15, 1, 0, GETDATE(), 0, 1, GETDATE(), GETDATE(), 0, N'XAU', 3500.00, N'Gold Gram 24K', N'جرام الذهب عيار 24', 0);
  ";

                str = ExecuteInsertWithIdentityCheck("Currency", str);
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" drop TABLE [dbo].[Currency_Factor]
CREATE TABLE [dbo].[Currency_Factor](
	[ID] [bigint] {IDENTITY} NOT NULL,
	[Currency_Primary_Name] [nvarchar](50) NULL,
	[Currency_Primary_ISOne] [int] NULL,
	[Currency_ID_Primary] [bigint] NULL,
	[Factor] [decimal](18, 3) NULL,
	[Currency_ID_Secondary] [bigint] NULL,
	[Currency_Secondary_Name] [nvarchar](50) NULL,
	[IS_TheSameCurrencies] [int] NULL,
	[OperationDate] [datetime] NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[Update_ServerDateTime] [datetime] NULL,
	[EditOperationDate] [datetime] NULL,
	[Edit_User_Id] [bigint] NULL,
	[ProcessNo] [bigint] NULL 
 
) ";
                opp.insert_me(str);


                str = @$"  
 

INSERT INTO [dbo].[Currency_Factor] 
(
    [ID], [Currency_Primary_Name], [Currency_Primary_ISOne], [Currency_ID_Primary], 
    [Factor], [Currency_ID_Secondary], [Currency_Secondary_Name], [IS_TheSameCurrencies], 
    [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], 
    [EditOperationDate], [Edit_User_Id], [ProcessNo]
)
VALUES
-- =================== 1. الجنيه المصري (ID = 1) ===================
-- جنيه إلى جنيه
(1, N'جنيه مصري', 1, 1, 1.0000, 1, N'جنيه مصري', 1, GETDATE(), 0, 1, GETDATE(), GETDATE(), NULL, 0, 0),

-- =================== 2. الدولار الأمريكي (ID = 2) ===================
-- دولار إلى جنيه
(2, N'دولار أمريكي', 1, 2, 48.5000, 1, N'جنيه مصري', 0, GETDATE(), 0, 1, GETDATE(), GETDATE(), NULL, 0, 0),
-- دولار إلى دولار
(3, N'دولار أمريكي', 1, 2, 1.0000, 2, N'دولار أمريكي', 1, GETDATE(), 0, 1, GETDATE(), GETDATE(), NULL, 0, 0),

-- =================== 3. اليورو الأوروبي (ID = 3) ===================
-- يورو إلى جنيه
(4, N'يورو', 1, 3, 52.3000, 1, N'جنيه مصري', 0, GETDATE(), 0, 1, GETDATE(), GETDATE(), NULL, 0, 0),
-- يورو إلى يورو
(5, N'يورو', 1, 3, 1.0000, 3, N'يورو', 1, GETDATE(), 0, 1, GETDATE(), GETDATE(), NULL, 0, 0),

-- =================== 4. الريال السعودي (ID = 4) ===================
-- ريال إلى جنيه
(6, N'ريال سعودي', 1, 4, 12.9000, 1, N'جنيه مصري', 0, GETDATE(), 0, 1, GETDATE(), GETDATE(), NULL, 0, 0),
-- ريال إلى ريال
(7, N'ريال سعودي', 1, 4, 1.0000, 4, N'ريال سعودي', 1, GETDATE(), 0, 1, GETDATE(), GETDATE(), NULL, 0, 0);
 
                      ";
                str = ExecuteInsertWithIdentityCheck("Currency_Factor", str);
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [USER_DETAILES_Branches_Permission]( [ID] [int] {IDENTITY} NOT NULL, [USER_DETAILES_Id] [int] NULL, [Branch_Id] [bigint] NULL )";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"  CREATE TABLE  [Account_Type_Main_Account_Id]( [Id] [bigint] {IDENTITY} NOT NULL, [Account_Type_Value] [int] NULL,Account_Type_Name [nvarchar(100)], [Account_Branch_Id] [bigint] NULL, [Main_Account_Id] [bigint] NULL )";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Tax_Branches](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[Account_BranchesId] [bigint] NULL,
	[Code] [nvarchar](500) NULL,
	[Name] [nvarchar](500) NULL,
	[TaxVatNumber] [nvarchar](500) NULL,
	[TaxRegistrationNumber] [nvarchar](500) NULL,
	[Commercial_Register] [nchar](10) NULL,
	[Address] [nvarchar](500) NULL,
	[BranchID] [nvarchar](500) NULL,
	[Country] [nvarchar](50) NULL,
	[Governate] [nvarchar](500) NULL,
	[RegionCity] [nvarchar](500) NULL,
	[Street] [nvarchar](500) NULL,
	[buildingNumber] [nvarchar](500) NULL,
	[postalCode] [nvarchar](500) NULL,
	[room] [nvarchar](500) NULL,
	[floor] [nvarchar](500) NULL,
	[lastuuid] [nvarchar](500) NULL,
	[landmark] [nvarchar](500) NULL,
	[additionalInformation] [nvarchar](500) NULL,
	[Phone1] [nvarchar](500) NULL,
	[Phone2] [nvarchar](500) NULL,
	[Phone3] [nvarchar](500) NULL,
	[Fax] [nvarchar](500) NULL,
	[Email] [nvarchar](500) NULL,
	[BankName] [nvarchar](500) NULL,
	[AccountNo] [nvarchar](500) NULL,
	[IBANNumber] [nvarchar](500) NULL,
	[BankAddress] [nvarchar](500) NULL,
	[Active] [int] NULL,
	[Logo] [image] NULL,
	[User_Id] [bigint] NULL DEFAULT ((1)),
	[Branch_Id] [bigint] NULL DEFAULT ((1)),
	[IsDefault] [int] NULL,
 CONSTRAINT [PK_Tax_Branches] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY] ";
                opp.insert_me(str);

                //str = @$"  ";
                //opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"
            IF OBJECT_ID(N'dbo.DataDictionaryDbSyncLogs', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.DataDictionaryDbSyncLogs (
                    Id BIGINT {IDENTITY} PRIMARY KEY,
                    TableName NVARCHAR(128) NOT NULL,
                    RecordId BIGINT NOT NULL,
                    ActionType VARCHAR(10) NOT NULL,
                    ActionType2 VARCHAR(10)   NULL,
                    Status VARCHAR(10) NOT NULL  DEFAULT 'Pending',
                    CreatedAt DATETIME NOT NULL  DEFAULT GETDATE(),
                    ProcessedAt DATETIME NULL,
                    RetryCount INT NOT NULL  DEFAULT 0,
                    LastError NVARCHAR(MAX) NULL,
                    payload  NVARCHAR(MAX) NULL,
                    mysqlCmd NVARCHAR(MAX) NULL,
                    Branch_Id BIGINT NOT NULL Default(0),
                    User_Id  BIGINT NOT NULL Default(0),
                     RecordIdStr NVARCHAR(MAX) NULL,
                    IsStatus  INT NOT NULL 
                );

                CREATE NONCLUSTERED INDEX IX_DataDictionaryDbSyncLogs_Status_Id 
                ON dbo.DataDictionaryDbSyncLogs (Status, Id) 
                INCLUDE (TableName, RecordId, ActionType);
            END";
                opp.insert_me(str);

                //str = @$"  ";
                //opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"   

 
CREATE TABLE [dbo].[Lookup_CarEntryTaskeen](
	[Id] [bigint]  {IDENTITY}  NOT NULL  primary key,
	[Name] [nvarchar](500) NULL ) 
	 

CREATE TABLE [dbo].[Lookup_CarShape](
	[Id] [bigint] {IDENTITY} NOT NULL  primary key,
	[Name] [nvarchar](50) NULL )  

 
CREATE TABLE [dbo].[LookUp_CarStatusLocation](
	[Id] [bigint] {IDENTITY} NOT NULL primary key,
	[Name] [nvarchar](500) NULL
)  
 
CREATE TABLE [dbo].[Lookup_CarType](
	[Id] [bigint] {IDENTITY} NOT NULL  primary key,
	[Name] [nvarchar](255) NULL,
	[NameEn] [nvarchar](500) NULL,
	[Code] [nvarchar](500) NULL 
  )  

 

CREATE TABLE [dbo].[Lookup_CarTypeModel](
	[Id] [bigint] {IDENTITY} NOT NULL  primary key,
	[Name] [nvarchar](500) NULL,
	[Lookup_CarTypeId] [bigint] NULL,
	[CarType] [nvarchar](500) NULL 
 
) 
 
 
CREATE TABLE [dbo].[Lookup_CashPaymentMethod](
	[Id] [bigint] {IDENTITY} NOT NULL  primary key,
	[Name] [nvarchar](500) NULL 
	) ";
                opp.insert_me(str);

                //str = @$"  ";
                //opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"  CREATE TABLE [dbo].[SysPrinterSettings] (
    
    [Id] INT {IDENTITY} NOT NULL, 
    
    [InvoicePrinterName] NVARCHAR(250) NOT NULL DEFAULT 'POS 80',
    [KitchenPrinterName] NVARCHAR(250) NOT NULL DEFAULT 'POS 80',
    [PaperWidth] INT NOT NULL DEFAULT 290,
    [InvoiceWidth] INT NOT NULL DEFAULT 270,
    
   
    [MarginLeft] REAL NOT NULL DEFAULT 10,
    [StartHeightY] REAL NOT NULL DEFAULT 10,
    
    
    [ShowPreview] BIT NOT NULL DEFAULT 0,
    [PrintKitchenSlip] BIT NOT NULL DEFAULT 0,
    
    
    [FontHeader] NVARCHAR(MAX) NULL DEFAULT ' ' 
) ";
                opp.insert_me(str);
               
                //str = @$"  ";
                //opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"  
CREATE TABLE [dbo].[CarsWorkOrders](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[StoreId] [bigint] NULL,
	[CarsId] [bigint] NULL,
	[CustomerId] [bigint] NULL,
	[WorkOrderNumber] [bigint] NULL,
	[CustomerSalesMan] [nvarchar](200) NULL,
	[Tel] [nvarchar](200) NULL,
	[Mobile] [nvarchar](200) NULL,
	[Address] [nvarchar](200) NULL,
	[CarMeterOnEntry] [nvarchar](200) NULL,
	[WorkOrdersDate] [datetime] NULL,
	[EntryDateTime] [datetime] NULL,
	[EntryDateOnly] [date] NULL,
	[EntryTimeOnly] [time](7) NULL,
	[StartWorkDateTime] [datetime] NULL,
	[ExpectedEndWorkDate] [datetime] NULL,
	[CustomerComplaint] [nvarchar](max) NULL,
	[RequiredWorks] [nvarchar](max) NULL,
	[ExaminationReport] [nvarchar](max) NULL,
	[ReceptionEngineerId] [bigint] NULL,
	[ReceptionEngineerName] [nvarchar](200) NULL,
	[ReceptionManName] [nvarchar](200) NULL,
	[IsWorkOrderClosed] [int] NULL,
	[WorkOrderClosedDate] [datetime] NULL,
	[ReceptionManId] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[User_Id] [bigint] NULL,
	[Lookup_CarTypeModeLId] [bigint] NULL,
	[Lookup_CarTypeId] [bigint] NULL,
	[Last_Invoice_ListId] [bigint] NULL,
	[Lookup_CarEntryTaskeenId] [bigint] NULL,
	[Lookup_CarEntryTaskeenName] [nvarchar](100) NULL,
	[LookUp_CarStatusLocationId] [bigint] NULL,
	[LookUp_CarStatusLocationName] [nvarchar](100) NULL,
	[WorkOrderClosed_Reason] [nvarchar](max) NULL,
	[CustomerOrderNumberPO] [nvarchar](max) NULL,
	[TaxInvoiceNo] [bigint] NULL,
	[TaxInternalId] [nvarchar](500) NULL,
	[IsUploadToPortal] [int] NULL,
	[KhamatQnty_Value] [decimal](18, 2) NULL,
	[TaxReverseInternalId] [nvarchar](500) NULL,
	[TaxReverseInvoiceNo] [bigint] NULL,
	[Last_Invoices_List_ReverseId] [bigint] NULL,
	[CustomerId_] [bigint] NULL,
	[CustomerId_CarOwner] [bigint] NULL,
	[IsInsuranceCompanyStaus] [int] NULL,
	[CustomerOrderNumberGR] [nvarchar](max) NULL,
	[Customer_Id_RelatedToAccount] [bigint] NULL,
	[Repairs_NotDone] [nvarchar](max) NULL,
	[IsInvoiceTaxInfo] [int] NULL )

 ";
                opp.insert_me(str);

                //str = @$"  ";
                //opp.insert_me(str);
            }
            catch
            {
            }

            try
            {
                str = @$"  

CREATE TABLE [dbo].[Tax_Branches](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[Account_BranchesId] [bigint] NULL,
	[Code] [nvarchar](500) NULL,
	[Name] [nvarchar](500) NULL,
	[TaxVatNumber] [nvarchar](500) NULL,
	[TaxRegistrationNumber] [nvarchar](500) NULL,
	[Commercial_Register] [nchar](10) NULL,
	[Address] [nvarchar](500) NULL,
	[BranchID] [nvarchar](500) NULL,
	[Country] [nvarchar](50) NULL,
	[Governate] [nvarchar](500) NULL,
	[RegionCity] [nvarchar](500) NULL,
	[Street] [nvarchar](500) NULL,
	[buildingNumber] [nvarchar](500) NULL,
	[postalCode] [nvarchar](500) NULL,
	[room] [nvarchar](500) NULL,
	[floor] [nvarchar](500) NULL,
	[lastuuid] [nvarchar](500) NULL,
	[landmark] [nvarchar](500) NULL,
	[additionalInformation] [nvarchar](500) NULL,
	[Phone1] [nvarchar](500) NULL,
	[Phone2] [nvarchar](500) NULL,
	[Phone3] [nvarchar](500) NULL,
	[Fax] [nvarchar](500) NULL,
	[Email] [nvarchar](500) NULL,
	[BankName] [nvarchar](500) NULL,
	[AccountNo] [nvarchar](500) NULL,
	[IBANNumber] [nvarchar](500) NULL,
	[BankAddress] [nvarchar](500) NULL,
	[Active] [int] NULL,
	[Logo] [image] NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[IsDefault] [int] NULL )  

CREATE TABLE [dbo].[Tax_BranchesConfig](
	[Tax_BranchesId] [bigint] NOT NULL,
	[Account_BranchesId] [bigint] NULL,
	[ClientId] [nvarchar](500) NULL,
	[ClientSecret] [nvarchar](500) NULL,
	[ActivityCode] [nvarchar](500) NULL,
	[TokenPin] [nvarchar](500) NULL,
	[IssuerId] [nvarchar](500) NULL,
	[IssuerName] [nvarchar](500) NULL,
	[IssuerType] [nvarchar](50) NULL,
	[documentTypeVersion] [nvarchar](500) NULL,
	[RowCode] [nvarchar](500) NULL,
	[dataBaseType] [nvarchar](500) NULL,
	[ConnectionString] [nvarchar](500) NULL,
	[IsSign] [int] NULL,
	[testtype] [nvarchar](500) NULL,
	[databasename] [nvarchar](500) NULL,
	[ClientIdPOS] [nvarchar](500) NULL,
	[ClientSecretPOS] [nvarchar](500) NULL,
	[SerialDevice] [nvarchar](500) NULL,
	[SystemVersion] [nvarchar](500) NULL,
	[documentTypeVersionPOS] [nvarchar](500) NULL,
	[PortalUrl] [nvarchar](500) NULL,
	[TokenUrl] [nvarchar](500) NULL,
	[SubmitUrl] [nvarchar](500) NULL,
	[CancelUrl] [nvarchar](500) NULL,
	[Host] [nvarchar](500) NULL,
	[RowStatus] [int] NULL,
	[RegistrationTaxFlag] [int] NULL,
	[TaxInvoiceStatus] [nvarchar](500) NULL,
	[IsDefault] [int] NULL,
	[portalType] [nvarchar](500) NULL
)  

CREATE TABLE [dbo].[Taxdocuments](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[TaxInvoices_ListId] [bigint] NULL,
	[invoices_ListId] [bigint] NULL,
	[internalId] [nvarchar](max) NULL,
	[submissionId] [nvarchar](max) NULL,
	[uuid] [nvarchar](max) NULL,
	[hashKey] [nvarchar](max) NULL,
	[longId] [nvarchar](max) NULL,
	[Errors] [nvarchar](max) NULL,
	[IsSuccessded] [int] NULL,
	[DateOfSent] [datetime] NULL,
	[StausStr] [nvarchar](50) NULL,
	[Staus] [int] NULL,
	[TaxInvoiceNo] [bigint] NULL
) 

CREATE TABLE [dbo].[TaxdocumentsReverse](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[TaxInvoices_ListId] [bigint] NULL,
	[invoices_ListId] [bigint] NULL,
	[internalId] [nvarchar](max) NULL,
	[submissionId] [nvarchar](max) NULL,
	[uuid] [nvarchar](max) NULL,
	[hashKey] [nvarchar](max) NULL,
	[longId] [nvarchar](max) NULL,
	[Errors] [nvarchar](max) NULL,
	[IsSuccessded] [int] NULL,
	[DateOfSent] [datetime] NULL,
	[StausStr] [nvarchar](50) NULL,
	[Staus] [int] NULL,
	[TaxInvoiceNo] [bigint] NULL
)  

CREATE TABLE [dbo].[TaxInvoices_ItemsTaxes](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[TaxInvoices_ListId] [bigint] NOT NULL,
	[TaxInvoices_LineItemsId] [bigint] NOT NULL,
	[itemCode] [nvarchar](max) NULL,
	[taxType] [nvarchar](max) NULL,
	[amount] [decimal](18, 2) NULL,
	[subType] [nvarchar](max) NULL,
	[rate] [decimal](18, 2) NULL
)  

CREATE TABLE [dbo].[TaxInvoices_LineItems](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[TaxInvoices_ListId] [bigint] NULL,
	[ItemCodeTaxAcceptedId] [bigint] NULL,
	[Item_Id] [bigint] NULL,
	[Unit_Item_Id] [bigint] NULL,
	[CurrencyId] [bigint] NULL,
	[description] [nvarchar](max) NULL,
	[itemType] [nvarchar](max) NULL,
	[itemCode] [nvarchar](max) NULL,
	[unitType] [nvarchar](max) NULL,
	[quantity] [decimal](18, 2) NULL,
	[weightUnitType] [nvarchar](max) NULL,
	[weightQuantity] [decimal](18, 2) NULL,
	[unitValue_currencySold] [nvarchar](max) NULL,
	[unitValue_amountSold] [decimal](18, 2) NULL,
	[unitValue_amountEGP] [decimal](18, 2) NULL,
	[unitValue_currencyExchangeRate] [decimal](18, 2) NULL,
	[factoryUnitValue] [nvarchar](max) NULL,
	[salesTotal] [decimal](18, 2) NULL,
	[discount_rate] [decimal](18, 2) NULL,
	[discount_amount] [decimal](18, 2) NULL,
	[internalCode] [nvarchar](max) NULL,
	[itemsDiscount] [decimal](18, 2) NULL,
	[netTotal] [decimal](18, 2) NULL,
	[totalTaxableFees] [decimal](18, 2) NULL,
	[valueDifference] [decimal](18, 2) NULL,
	[total] [decimal](18, 2) NULL,
	[amountEGP] [decimal](18, 2) NULL,
	[currencySold] [nvarchar](200) NULL,
	[amountSold] [decimal](18, 2) NULL,
	[currencyExchangeRate] [decimal](18, 2) NULL
)  

CREATE TABLE [dbo].[TaxInvoices_List](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[Tax_BranchesId] [bigint] NULL,
	[Invoices_ListId] [bigint] NULL,
	[TaxInvoiceNo] [bigint] NULL,
	[internalID] [nvarchar](max) NULL,
	[CustomerId] [bigint] NULL,
	[Receiver_type] [nvarchar](max) NULL,
	[Receiver_id] [nvarchar](max) NULL,
	[Receiver_name] [nvarchar](max) NULL,
	[issuer_type] [nvarchar](max) NULL,
	[issuer_id] [nvarchar](max) NULL,
	[issuer_name] [nvarchar](max) NULL,
	[documentType] [nvarchar](max) NULL,
	[documentTypeVersion] [nvarchar](max) NULL,
	[dateTimeIssued] [datetime] NULL,
	[taxpayerActivityCode] [nvarchar](max) NULL,
	[purchaseOrderReference] [nvarchar](max) NULL,
	[purchaseOrderDescription] [nvarchar](max) NULL,
	[salesOrderReference] [nvarchar](max) NULL,
	[salesOrderDescription] [nvarchar](max) NULL,
	[proformaInvoiceNumber] [nvarchar](max) NULL,
	[totalDiscountAmount] [decimal](18, 2) NULL,
	[totalSalesAmount] [decimal](18, 2) NULL,
	[netAmount] [decimal](18, 2) NULL,
	[taxTotals] [decimal](18, 2) NULL,
	[totalAmount] [decimal](18, 2) NULL,
	[extraDiscountAmount] [decimal](18, 2) NULL,
	[totalItemsDiscountAmount] [decimal](18, 2) NULL,
	[submissionId] [nvarchar](max) NULL
)  

CREATE TABLE [dbo].[TaxInvoices_TaxesTotal](
	[id] [bigint] {IDENTITY} NOT NULL,
	[TaxInvoices_ListId] [bigint] NOT NULL,
	[taxType] [nvarchar](max) NULL,
	[amount] [decimal](18, 2) NULL
) 

CREATE TABLE [dbo].[TaxReverseInvoices_ItemsTaxes](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[TaxInvoices_ListId] [bigint] NOT NULL,
	[TaxReverseInvoices_LineItemsId] [bigint] NOT NULL,
	[itemCode] [nvarchar](max) NULL,
	[taxType] [nvarchar](max) NULL,
	[amount] [decimal](18, 2) NULL,
	[subType] [nvarchar](max) NULL,
	[rate] [decimal](18, 2) NULL
)  

CREATE TABLE [dbo].[TaxReverseInvoices_LineItems](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[TaxInvoices_ListId] [bigint] NULL,
	[ItemCodeTaxAcceptedId] [bigint] NULL,
	[Item_Id] [bigint] NULL,
	[Unit_Item_Id] [bigint] NULL,
	[CurrencyId] [bigint] NULL,
	[description] [nvarchar](max) NULL,
	[itemType] [nvarchar](max) NULL,
	[itemCode] [nvarchar](max) NULL,
	[unitType] [nvarchar](max) NULL,
	[quantity] [decimal](18, 2) NULL,
	[weightUnitType] [nvarchar](max) NULL,
	[weightQuantity] [decimal](18, 2) NULL,
	[unitValue_currencySold] [nvarchar](max) NULL,
	[unitValue_amountSold] [decimal](18, 2) NULL,
	[unitValue_amountEGP] [decimal](18, 2) NULL,
	[unitValue_currencyExchangeRate] [decimal](18, 2) NULL,
	[factoryUnitValue] [nvarchar](max) NULL,
	[salesTotal] [decimal](18, 2) NULL,
	[discount_rate] [decimal](18, 2) NULL,
	[discount_amount] [decimal](18, 2) NULL,
	[internalCode] [nvarchar](max) NULL,
	[itemsDiscount] [decimal](18, 2) NULL,
	[netTotal] [decimal](18, 2) NULL,
	[totalTaxableFees] [decimal](18, 2) NULL,
	[valueDifference] [decimal](18, 2) NULL,
	[total] [decimal](18, 2) NULL,
	[amountEGP] [decimal](18, 2) NULL,
	[currencySold] [nvarchar](200) NULL,
	[amountSold] [decimal](18, 2) NULL,
	[currencyExchangeRate] [decimal](18, 2) NULL
)  

CREATE TABLE [dbo].[TaxReverseInvoices_List](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[Tax_BranchesId] [bigint] NULL,
	[Invoices_ListId] [bigint] NULL,
	[TaxInvoiceNo] [bigint] NULL,
	[internalID] [nvarchar](max) NULL,
	[Customer_Id] [bigint] NULL,
	[Receiver_type] [nvarchar](max) NULL,
	[Receiver_id] [nvarchar](max) NULL,
	[Receiver_name] [nvarchar](max) NULL,
	[issuer_type] [nvarchar](max) NULL,
	[issuer_id] [nvarchar](max) NULL,
	[issuer_name] [nvarchar](max) NULL,
	[documentType] [nvarchar](max) NULL,
	[documentTypeVersion] [nvarchar](max) NULL,
	[dateTimeIssued] [datetime] NULL,
	[taxpayerActivityCode] [nvarchar](max) NULL,
	[purchaseOrderReference] [nvarchar](max) NULL,
	[purchaseOrderDescription] [nvarchar](max) NULL,
	[salesOrderReference] [nvarchar](max) NULL,
	[salesOrderDescription] [nvarchar](max) NULL,
	[proformaInvoiceNumber] [nvarchar](max) NULL,
	[totalDiscountAmount] [decimal](18, 2) NULL,
	[totalSalesAmount] [decimal](18, 2) NULL,
	[netAmount] [decimal](18, 2) NULL,
	[taxTotals] [decimal](18, 2) NULL,
	[totalAmount] [decimal](18, 2) NULL,
	[extraDiscountAmount] [decimal](18, 2) NULL,
	[totalItemsDiscountAmount] [decimal](18, 2) NULL,
	[submissionId] [nvarchar](max) NULL
)  

CREATE TABLE [dbo].[TaxReverseInvoices_TaxesTotal](
	[id] [bigint] {IDENTITY} NOT NULL,
	[TaxInvoices_ListId] [bigint] NOT NULL,
	[taxType] [nvarchar](max) NULL,
	[amount] [decimal](18, 2) NULL
)  
 ";
                opp.insert_me(str);

                //str = @$"  ";
                //opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"  

CREATE TABLE [dbo].[Invoices_DetailsTemp](
	[Id] [bigint] {IDENTITY} NOT   NULL,
	[Invoice_Id] [bigint] NULL,
	[Item_Id] [bigint] NULL,
	[Unit_Item_Table_Id] [bigint] NULL,
	[Specification] [nvarchar](500) NULL,
	[Larg_Qnty] [decimal](18, 2) NULL,
	[Small_Qnty] [decimal](18, 2) NULL,
	[Qnty_Another] [decimal](18, 2) NULL,
	[Larg_Unit_Price] [decimal](18, 2) NULL,
	[Small_Unit_Price] [decimal](18, 2) NULL,
	[Unit_Used_for_Price] [int] NULL,
	[Qnty_value] [decimal](18, 2) NULL,
	[ATS05] [decimal](18, 2) NULL,
	[TaxSales10] [decimal](18, 2) NULL,
	[ATS05_Value] [decimal](18, 2) NULL,
	[TaxSales10_Value] [decimal](18, 2) NULL,
	[Total] [decimal](18, 2) NULL,
	[Notes] [nvarchar](max) NULL,
	[Unit_Id] [bigint] NULL,
	[code] [nvarchar](50) NULL,
	[Make_Order] [int] NULL,
	[Balance_Checked] [int] NULL,
	[Invoice_Item_Name] [nvarchar](100) NULL,
	[Order_Purshing_Id_Automatic] [bigint] NULL,
	[Store_Id] [bigint] NULL,
	[NameSpecification] [nvarchar](500) NULL,
	[Item_CommissionValue] [decimal](18, 2) NULL,
	[Is_Item_CommissionValue] [int] NULL,
	[Unit_CommissionValue] [decimal](18, 2) NULL,
	[QntyWidth] [decimal](18, 2) NULL,
	[QntyLenght] [decimal](18, 2) NULL,
	[QntyItemNo] [decimal](18, 2) NULL,
	[Bills_Details_Id] [bigint] NULL,
	[Purchase_Price] [decimal](18, 2) NULL,
	[Audience_price] [decimal](18, 2) NULL,
	[Item_Expire_Date] [datetime] NULL,
	[Is_InvoiceItem_Kta3y] [int] NULL,
	[LargItemsQnty_Free] [decimal](18, 2) NULL,
	[SmallItemsQnty_Free] [decimal](18, 2) NULL,
	[Is_Alternate_Item_CommissionValue] [int] NULL,
	[Alternate_Item_CommissionValue] [decimal](18, 2) NULL,
	[Alternate_Item_CommissionRate] [decimal](18, 2) NULL,
	[Alternate_ItemCommissionTotal] [decimal](18, 2) NULL,
	[Net_Total_Item_Commission] [decimal](18, 2) NULL,
	[Item_CommissionRate] [decimal](18, 2) NULL,
	[Unit_CommissionRate] [decimal](18, 2) NULL,
	[Alternate_Unit_CommissionRate] [decimal](18, 2) NULL,
	[Alternate_Unit_CommissionValue] [decimal](18, 2) NULL,
	[Unit_Item_ExpireDate_Id] [bigint] NULL,
	[Merchant_Name] [nvarchar](500) NULL,
	[Tash3ela] [nvarchar](500) NULL,
	[ItemCommissionTotal] [decimal](18, 2) NULL,
	[Item_No_Free] [decimal](18, 2) NULL,
	[StatementNo] [nvarchar](100) NULL,
	[OperationNo] [nvarchar](100) NULL,
	[Item_Total_WithoutTax] [decimal](18, 2) NULL,
	[Supplier_BalanceSet] [int] NULL,
	[Item_ATS05] [decimal](18, 2) NULL,
	[Item_ATS05_Value] [decimal](18, 2) NULL,
	[Item_TaxSales10] [decimal](18, 2) NULL,
	[Item_TaxSales10_Value] [decimal](18, 2) NULL,
	[Item_Net_Total_Price] [decimal](18, 2) NULL,
	[Item_Notes] [nvarchar](max) NULL,
	[QntyStr] [nvarchar](500) NULL,
	[QntyStrColumn] [nvarchar](500) NULL,
	[OperationDate] [datetime] NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[Update_ServerDateTime] [datetime] NULL,
	[EditOperationDate] [datetime] NULL,
	[Edit_User_Id] [bigint] NULL,
	[ProcessNo] [bigint] NULL,
	[Item_DeletedFlage] [int] NULL,
	[Item_ProcessedFlage] [int] NULL,
	[TotalSmall_Qnty] [decimal](18, 2) NULL,
	[TotalSmall_QntyStr] [nvarchar](500) NULL,
	[TotalSmall_QntyStrColumn] [nvarchar](500) NULL,
	[Store_Name] [nvarchar](500) NULL,
	[S_price_LargeUnit] [decimal](18, 2) NULL,
	[S_price_SmallUnit] [decimal](18, 2) NULL,
	[AppointmentsId] [bigint] NULL,
	[AppointmentServicesId] [bigint] NULL,
	[CarsWorkOrdersItemProcessedDetailsId] [bigint] NULL,
	[Unit_Item_SalesDateId] [bigint] NULL,
	[Qnty] [decimal](18, 2) NULL,
	[Available_Larg_Balance] [decimal](18, 2) NULL,
	[Available_Small_Balance] [decimal](18, 2) NULL,
	[Larg_Qnty_Balance] [decimal](18, 2) NULL,
	[Small_Qnty_Balance] [decimal](18, 2) NULL,
	[PriceUnit] [decimal](18, 2) NULL,
	[UnitStr] [nvarchar](500) NULL,
	[UnitSmallStr] [nvarchar](500) NULL,
	[Qnty_FreeStr] [nvarchar](500) NULL,
	[Qnty_Free] [decimal](18, 2) NULL,
	[Total3Mola] [decimal](18, 2) NULL,
	[Unit3Mola] [decimal](18, 2) NULL,
	[WaiterManId] [bigint] NULL,
	[ProductCost_Price] [decimal](18, 2) NULL,
	[P_priceAvgCost] [decimal](18, 2) NULL,
	[S_priceAvgCost] [decimal](18, 2) NULL,
	[InvoiceKitchenStatusId] [bigint] NULL
)  


 ";
                opp.insert_me(str);

                //str = @$"  ";
                //opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Invoices_ListTemp](
	[ID] [bigint] {IDENTITY}   NOT NULL,
	[Invoice_Id] [bigint] NULL,
	[Customer_Id] [bigint] NULL,
	[License_Id] [int] NULL,
	[Invoice_Date] [datetime] NULL,
	[Total] [decimal](18, 2) NULL,
	[TaxSales10] [decimal](18, 2) NULL,
	[ATS05] [decimal](18, 2) NULL,
	[TaxSales10_Value] [decimal](18, 2) NULL,
	[ATS05_Value] [decimal](18, 2) NULL,
	[Net_Total] [decimal](18, 2) NULL,
	[Currency] [nvarchar](50) NULL,
	[Currency_id] [bigint] NULL,
	[Notes] [nvarchar](max) NULL,
	[Invoice_Type] [int] NULL,
	[Editable] [int] NULL,
	[Deleted] [int] NULL,
	[Deleted_Date] [datetime] NULL,
	[Control_Cusstomer_Account] [int] NULL,
	[Customer_BalanceSet] [int] NULL,
	[Make_Order_Purshing] [int] NULL,
	[PolicaNo] [nvarchar](50) NULL,
	[troodContainerNo] [nvarchar](50) NULL,
	[Day_Registration_flage] [int] NOT NULL,
	[DAMGHAT] [decimal](18, 2) NULL,
	[commissionValue] [decimal](18, 2) NULL,
	[DayReg_List_Id] [bigint] NULL,
	[CustomerAccountBefor] [decimal](18, 2) NULL,
	[CustomerAccountAfter] [decimal](18, 2) NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Total_Item_CommissionValue] [decimal](18, 2) NULL,
	[Total_CommissionValue] [decimal](18, 2) NULL,
	[SubAccount_Id] [bigint] NULL,
	[SubAccount_Name] [nvarchar](100) NULL,
	[Equivalent_TaxSales10_Value] [decimal](18, 2) NULL,
	[Equivalent_ATS05_Value] [decimal](18, 2) NULL,
	[Equivalent_Net_Total] [decimal](18, 2) NULL,
	[Equivalent_CommissionValue] [decimal](18, 2) NULL,
	[Equivalent_Total] [decimal](18, 2) NULL,
	[IS_Posted_Flage] [int] NULL,
	[Account_Entry_List_ID] [bigint] NULL,
	[IsCommission_Value] [int] NULL,
	[Salesman_Id] [bigint] NULL,
	[AnotherPayment] [decimal](18, 2) NULL,
	[Is_Buy_From_Bills_Items_Balance] [int] NULL,
	[OperationDate] [datetime] NULL,
	[IS_Make_Sarf_Item_BalanceFromInvoice] [int] NULL,
	[Is_InvoiceKta3y] [int] NULL,
	[Is_Tax_Per_Item] [int] NULL,
	[Is_Load_Payment_OnTax] [int] NULL,
	[Total_PriceWithoutTax_Load_Payment] [decimal](18, 2) NULL,
	[PriceByTenns] [decimal](18, 2) NULL,
	[Total_PriceWithoutTax] [decimal](18, 2) NULL,
	[Invoice_DateTime] [datetime] NULL,
	[GuId] [uniqueidentifier] NULL,
	[List_CommissionValue] [decimal](18, 2) NULL,
	[List_CommissionRate] [decimal](18, 2) NULL,
	[List_Final_CommissionValue] [decimal](18, 2) NULL,
	[Net_Total_CommissionValue] [decimal](18, 2) NULL,
	[Equivalent_Net_Total_CommissionValue] [decimal](18, 2) NULL,
	[Equivalent_Net_Total_Price] [decimal](18, 2) NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[Update_ServerDateTime] [datetime] NULL,
	[EditOperationDate] [datetime] NULL,
	[Edit_User_Id] [bigint] NULL,
	[ProcessNo] [bigint] NULL,
	[Order_MoneyId] [bigint] NULL,
	[BanksId] [bigint] NULL,
	[BanksNamePaymentMethod] [nvarchar](100) NULL,
	[PaidValue] [decimal](18, 2) NULL,
	[RestValue] [decimal](18, 2) NULL,
	[Emp_Name] [nvarchar](500) NULL,
	[Customer_Notes] [nvarchar](100) NULL,
	[Is_Invoice_Agile] [int] NULL,
	[Is_Invoice_AgileStr] [nvarchar](100) NULL,
	[Emp_Id] [bigint] NULL,
	[AppointmentsId] [bigint] NULL,
	[CustomerAccountAfterNakdia] [decimal](18, 2) NULL,
	[PaymentValueFor_Is_Invoice_Agile] [decimal](18, 2) NULL,
	[OverPaidAmount] [decimal](18, 2) NULL,
	[IsUseExtraAmountToPayDebt] [bit] NULL,
	[IsInvoiceTaxInfo] [int] NULL,
	[Is_DeletedProcessed] [int] NULL,
	[IsMaintainceInvoice] [int] NULL,
	[TaxInvoices_ListId] [bigint] NULL,
	[IsUploadToPortal] [int] NULL,
	[TaxInternalId] [nvarchar](500) NULL,
	[CarNumber] [nvarchar](500) NULL,
	[CarsId] [bigint] NULL,
	[CarsWorkOrdersId] [bigint] NULL,
	[WorkOrderNumber] [bigint] NULL,
	[CustomerName] [nvarchar](500) NULL,
	[Qnty_ValueSparePartsOils] [decimal](18, 2) NULL,
	[ATS05ListSparePartsOils_Rate] [decimal](18, 2) NULL,
	[ATS05ListSparePartsOils_Value] [decimal](18, 2) NULL,
	[TaxSalesListSparePartsOils_Rate] [decimal](18, 2) NULL,
	[TaxSalesListSparePartsOils_Value] [decimal](18, 2) NULL,
	[CommissionListSparePartsOils_Rate] [decimal](18, 2) NULL,
	[CommissionListSparePartsOils_Value] [decimal](18, 2) NULL,
	[Total_CommissionListSparePartsOils] [decimal](18, 2) NULL,
	[Total_ListPriceWithoutTax_SparePartsOils] [decimal](18, 2) NULL,
	[NetInvoiceTotal_SparePartsOils] [decimal](18, 2) NULL,
	[Item_Net_TotalSparePartsOils] [decimal](18, 2) NULL,
	[Total_ListPriceAfterTaxSales_SparePartsOils] [decimal](18, 2) NULL,
	[Qnty_ValueRawMaterials] [decimal](18, 2) NULL,
	[TaxInvoiceNo] [bigint] NULL,
	[Total_ListPriceWithoutTax_RawMaterial] [decimal](18, 2) NULL,
	[NetInvoiceTotal_RawMaterial] [decimal](18, 2) NULL,
	[Total_ListPriceAfterTaxSales_RawMaterial] [decimal](18, 2) NULL,
	[Customer_Id_RelatedToAccount] [bigint] NULL,
	[TafkeetNet_Total] [nvarchar](max) NULL,
	[Qnty_Value] [decimal](18, 2) NULL,
	[Qnty_ValueWorkmanship] [decimal](18, 2) NULL,
	[Qnty_ValueSpareParts] [decimal](18, 2) NULL,
	[Qnty_ValueOils] [decimal](18, 2) NULL,
	[ATS05ListWorkmanship_Rate] [decimal](18, 2) NULL,
	[ATS05ListSpareParts_Rate] [decimal](18, 2) NULL,
	[ATS05ListOils_Rate] [decimal](18, 2) NULL,
	[ATS05ListWorkmanship_Value] [decimal](18, 2) NULL,
	[ATS05ListSpareParts_Value] [decimal](18, 2) NULL,
	[ATS05ListOils_Value] [decimal](18, 2) NULL,
	[TotalListATS05_Value] [decimal](18, 2) NULL,
	[TaxSalesListWorkmanship_Rate] [decimal](18, 2) NULL,
	[TaxSalesListSpareParts_Rate] [decimal](18, 2) NULL,
	[TaxSalesListOils_Rate] [decimal](18, 2) NULL,
	[TaxSalesListWorkmanship_Value] [decimal](18, 2) NULL,
	[TaxSalesListSpareParts_Value] [decimal](18, 2) NULL,
	[TaxSalesListOils_Value] [decimal](18, 2) NULL,
	[TotalListTaxSales10_Value] [decimal](18, 2) NULL,
	[CommissionListWorkmanship_Rate] [decimal](18, 2) NULL,
	[CommissionListSpareParts_Rate] [decimal](18, 2) NULL,
	[CommissionListOils_Rate] [decimal](18, 2) NULL,
	[CommissionListWorkmanship_Value] [decimal](18, 2) NULL,
	[CommissionListSpareParts_Value] [decimal](18, 2) NULL,
	[CommissionListOils_Value] [decimal](18, 2) NULL,
	[Total_CommissionListWorkmanship] [decimal](18, 2) NULL,
	[Total_CommissionListSpareParts] [decimal](18, 2) NULL,
	[Total_CommissionListOils] [decimal](18, 2) NULL,
	[Total_CommissionList] [decimal](18, 2) NULL,
	[Total_ListPriceWithoutTax_Workmanship] [decimal](18, 2) NULL,
	[Total_ListPriceWithoutTax_SpareParts] [decimal](18, 2) NULL,
	[Total_ListPriceWithoutTax_Oils] [decimal](18, 2) NULL,
	[Total_ListPriceWithoutTax] [decimal](18, 2) NULL,
	[NetInvoiceTotal_Workmanship] [decimal](18, 2) NULL,
	[NetInvoiceTotal_SpareParts] [decimal](18, 2) NULL,
	[NetInvoiceTotal_Oils] [decimal](18, 2) NULL,
	[NetInvoiceTotal] [decimal](18, 2) NULL,
	[Item_Net_TotalWorkmanship] [decimal](18, 2) NULL,
	[Item_Net_TotalSpareParts] [decimal](18, 2) NULL,
	[Item_Net_TotalOils] [decimal](18, 2) NULL,
	[Item_Net_Total] [decimal](18, 2) NULL,
	[Total_ItemPriceWithoutTax] [decimal](18, 2) NULL,
	[Total_ListPriceAfterTaxSales_Workmanship] [decimal](18, 2) NULL,
	[Total_ListPriceAfterTaxSales_SpareParts] [decimal](18, 2) NULL,
	[Total_ListPriceAfterTaxSales_Oils] [decimal](18, 2) NULL,
	[Total_ListPriceAfterTaxSales] [decimal](18, 2) NULL,
	[Item_Net_TotalRawMaterials] [decimal](18, 2) NULL,
	[Total_List3Mola] [decimal](18, 2) NULL,
	[WahbaValue] [decimal](18, 2) NULL,
	[NawlounValue] [decimal](18, 2) NULL,
	[Total3MolaRate] [decimal](18, 2) NULL,
	[Value_3Mola] [decimal](18, 2) NULL,
	[ShipmentCompany_Id] [bigint] NULL,
	[ShipmentCompany_Name] [nvarchar](500) NULL,
	[Shipment_Value] [decimal](18, 2) NULL,
	[InvoiceDueDate] [datetime] NULL,
	[Lookup_OrderDeliveryTypesId] [bigint] NULL,
	[Lookup_InvoiceKitchenStatusId] [bigint] NULL,
	[Lookup_ShiftsId] [bigint] NULL,
	[CustomerPhone] [nvarchar](max) NULL,
	[Delivery_Address] [nvarchar](max) NULL,
	[Service_Rate] [decimal](18, 2) NULL,
	[Service_Amount] [decimal](18, 2) NULL,
	[Delivery_Fee] [decimal](18, 2) NULL,
	[TableId] [bigint] NULL,
	[DeliveryMan_DriverId] [bigint] NULL,
	[Lookup_DiscountTypesId] [bigint] NULL,
	[Is_Printed] [int] NULL,
	[WaiterManId] [bigint] NULL,
	[CustomerDeliveryName] [nvarchar](500) NULL,
	[IsDeliveryFeeAddedToInvoiceTotal] [int] NULL,
	[IsDeliveryFeeAddedToInvoiceTotalAfterTax] [int] NULL,
	[Is_Reconciled_With_ManDriver] [int] NULL,
	[Invoices_ListTempId] [bigint] NULL,
	[InvoiceStatus] [int] NULL,
	[InvoiceStatus_Reason] [nvarchar](max) NULL,
	[RestValueFromCashNakdia] [decimal](18, 2) NULL,
	[PaidValueFromCashNakdy] [decimal](18, 2) NULL,
	[Money_Transaction_BoxId] [bigint] NULL
)   ";
                opp.insert_me(str);

                //str = @$"  ";
                //opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[WorkOrderList](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[WorkOrder_No] [bigint] NOT NULL,
	[WorkOrderDate] [datetime2](7) NULL,
	[Status] [int] NULL,
	[StatusStr] [nvarchar](250) NULL,
	[WorkOrderType] [int] NULL,
	[WorkOrderTypeStr] [nvarchar](250) NULL,
	[StoreId] [bigint] NULL,
	[StoreName] [nvarchar](250) NULL,
	[CustomerId] [bigint] NULL,
	[CustomerName] [nvarchar](250) NULL,
	[MachineId] [bigint] NULL,
	[MachineName] [nvarchar](250) NULL,
	[EmployeeId] [bigint] NULL,
	[EmployeeName] [nvarchar](250) NULL,
	[NotesAll] [nvarchar](max) NULL,
	[OperationDate] [datetime2](7) NULL,
	[Insert_ServerDateTime] [datetime2](7) NULL,
	[Update_ServerDateTime] [datetime2](7) NULL,
	[EditOperationDate] [datetime2](7) NULL,
	[Edit_User_Id] [bigint] NULL )  ";
                opp.insert_me(str);

                //str = @$"  ";
                //opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[WorkOrderDetail](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[WorkOrderListId] [bigint] NOT NULL,
	[Item_Id] [bigint] NOT NULL,
	[Item_Name] [nvarchar](250) NULL,
	[UnitId] [bigint] NOT NULL,
	[UnitName] [nvarchar](100) NULL,
	[RequiredQuantity] [decimal](18, 4) NULL,
	[IssuedQuantity] [decimal](18, 4) NULL,
	[ReceivedQuantity] [decimal](18, 4) NULL,
	[Notes] [nvarchar](max) NULL,
	[OperationDate] [datetime2](7) NULL,
	[Insert_ServerDateTime] [datetime2](7) NULL,
	[Update_ServerDateTime] [datetime2](7) NULL,
	[EditOperationDate] [datetime2](7) NULL,
	[Edit_User_Id] [bigint] NULL
)   ";
                opp.insert_me(str);

                //str = @$"  ";
                //opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Lookup_Tables] (
    [Id]            BIGINT        {IDENTITY} NOT NULL, -- يبدأ من الصفر
    [Name] NVARCHAR (500) NOT NULL,                -- اسم الطاولة بالعربية (مثل: طاولة 1، صالون عائلات)
    [Name_En] NVARCHAR (500) NOT NULL,                -- اسم الطاولة بالإنجليزية
    [Is_Active]     BIT           DEFAULT ((1)) NOT NULL   -- حالة الطاولة (1 = متاحة للعمل، 0 = مغلقة للصيانة أو ملغاة)
    
    
); ";
                opp.insert_me(str);

                str = @$"  
INSERT INTO [dbo].[Lookup_Tables] ([Id], [Name], [Name_En], [Is_Active]) 
VALUES 
(0, N'غير محدد', N'Undefined', 1),
(1, N'طاولة 1', N'Table 1', 1),
(2, N'طاولة 2', N'Table 2', 1),
(3, N'طاولة 3', N'Table 3', 1),
(4, N'طاولة 4', N'Table 4', 1),
(5, N'صالون عائلات 1', N'Family Salon 1', 1),
(6, N'صالون عائلات 2', N'Family Salon 2', 1),
(7, N'صالون عائلات 3', N'Family Salon 3', 1),
(8, N'صالون عائلات 4', N'Family Salon 4', 1);
  ";

                str = ExecuteInsertWithIdentityCheck("Lookup_Tables", str);

                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Lookup_DiscountTypes] (
    [Id] BIGINT {IDENTITY} NOT NULL, -- يبدأ من الصفر لتوحيد المعايير
    [Name] NVARCHAR (50) NOT NULL,                -- اسم الخصم بالعربية (مثل: خصم موظفين، ضيافة)
    [Name_En] NVARCHAR (50) NOT NULL                 -- اسم الخصم بالإنجليزية
    
  
); ";
                opp.insert_me(str);

                str = @$"  
INSERT INTO [dbo].[Lookup_DiscountTypes] ([Id], [Name], [Name_En]) 
VALUES 
(0, N'بدون خصم', N'No Discount'),
(1, N'خصم موظفين', N'Staff Discount'),
(2, N'ضيافة / مدير', N'Hospitality'),
(3, N'عروض لفترة محدودة', N'Promotion');
  ";

                str = ExecuteInsertWithIdentityCheck("Lookup_DiscountTypes", str);
                opp.insert_me(str);
            }
            catch
            {
            }

            try
            {
                str = @$" CREATE TABLE [dbo].[Lookup_Shifts] (
    [Id]            bigint           {IDENTITY} NOT NULL, -- يبدأ من صفر
    [Name] NVARCHAR (500) NOT NULL,
    [Name_En] NVARCHAR (500) NOT NULL,
    [Start_Time]    TIME (7)      NOT NULL, 
    [End_Time]      TIME (7)      NOT NULL 
);
 
";
                opp.insert_me(str);

                str = @$" 
 
INSERT INTO [dbo].[Lookup_Shifts] ([Id], [Name], [Name_En], [Start_Time], [End_Time]) VALUES 
(0, N'غير محدد', N'Undefined', '00:00:00', '00:00:00'), -- الوردية الافتراضية صفريّة الوقت
(1, N'الوردية الصباحية', N'Morning Shift', '08:00:00', '16:00:00'),
(2, N'الوردية المسائية', N'Evening Shift', '16:00:00', '00:00:00'),
(3, N'وردية السهر / الإضافي', N'Night Shift', '00:00:00', '08:00:00');
  ";

                str = ExecuteInsertWithIdentityCheck("Lookup_Shifts", str);
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Lookup_InvoiceKitchenStatus] (
    [Id]             bigint           {IDENTITY} NOT NULL,
    [Name] NVARCHAR (500) NOT NULL,
    [Name_En] NVARCHAR (500) NOT NULL 
     
);
 ";
                opp.insert_me(str);

                str = @$" 

-- زرع الحالات المتسلسلة للدورة الموحدة
 
INSERT INTO [dbo].[Lookup_InvoiceKitchenStatus] ([Id], [Name], [Name_En]) VALUES 
(0, N'غير محدد', N'Undefined'),
(1, N'قيد التحضير في المطبخ', N'Preparing in Kitchen'), -- كاشير حفظ، والمطبخ شغال
(2, N'جاهز للتسليم / مع الطيار', N'Ready / Out for Delivery'), -- المطبخ انتهى وسلمها للطيار
(3, N'مدفوعة ومكتملة', N'Paid & Settled'),        -- الطيار رجع وسدد، أو صالة اندفعت
(4, N'مرتجعة / ملغاة', N'Canceled');               -- تم إلغاء الطلب
 
   ";

                str = ExecuteInsertWithIdentityCheck("Lookup_InvoiceKitchenStatus", str);
                opp.insert_me(str);
            }
            catch
            {
            }




            try
            {
                str = @$" CREATE TABLE [dbo].[Money_Transaction_BoxForInvoices](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[Money_Transaction_BoxId] [bigint] NULL,
	[Order_Mony_Id] [bigint] NULL,
	[Order_No] [bigint] NULL,
	[Invoices_ListId] [bigint] NULL,
	[Invoice_TypeId] [int] NULL,
	[Bills_ListId] [bigint] NULL,
	[Bank_Id] [bigint] NULL,
	[Reference_Type_Table_Id] [bigint] NULL,
	[DataDictionaryProcessesListsId] [bigint] NULL,
	[Transaction_Date] [datetime] NULL,
	[Order_Total_Value] [decimal](18, 2) NULL,
	[Transaction_Value] [decimal](18, 2) NULL,
	[Import_Value] [decimal](18, 2) NULL,
	[Export_Value] [decimal](18, 2) NULL,
	[Customer_Id] [bigint] NULL,
	[Supplier_Id] [bigint] NULL,
	[SubAccount_Id] [bigint] NULL,
	[Transaction_Type] [int] NULL ) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" 
  
  
CREATE TABLE Appointments (
    Id BIGINT {IDENTITY} PRIMARY KEY,
	AppointmentNumber BIGINT NULL,
    AppointmentDateTime DATETIME   NULL,
	 AppointmentDate DATETIME   NULL,
	  AppointmentTime DATETIME   NULL,
    Customer_Id BIGINT NULL,
    CustomerName NVARCHAR(150),
    CustomerPhone NVARCHAR(50),
	AppointmentNotes NVARCHAR(MAX) ,
	IsCompleteStatus int default(0),
);

CREATE TABLE AppointmentServices (
    Id BIGINT {IDENTITY} PRIMARY KEY,
    AppointmentId BIGINT   NULL,
    ServiceItem_Id BIGINT   NULL,
Unit_Item_Table_Id  BIGINT   NULL,
	ServiceItem_Name NVARCHAR(MAX) ,
    Qnty DECIMAL(18,2) DEFAULT 1,
    S_Price DECIMAL(18,2) DEFAULT 0,
	QntyValue DECIMAL(18,2) DEFAULT 0,
    DurationMinutes INT  DEFAULT 0,
    AppointmentServiceNotes NVARCHAR(MAX) ,
 EmployeeNames  NVARCHAR(MAX),
 IsProcessedAsInvoice int default(0),

Is_ItemOrService int default(0),
ItemOrServiceStr   NVARCHAR(MAX) ,
 Invoices_List_Id BIGINT   NULL,
  Invoices_Details_Id BIGINT   NULL 


);

CREATE TABLE AppointmentServiceEmployees (
    Id BIGINT {IDENTITY} PRIMARY KEY,
    
    AppointmentServiceId BIGINT   NULL,
    EmployeeId BIGINT   NULL,
    ContributionPercentage DECIMAL(5,2) default(0),
IsContributionPercentage  int default(0),
ContributionValue  DECIMAL(5,2) default(0), 
    AppointmentServiceEmployeeNotes NVARCHAR(max) ,
	IsCompletePayed int default(0),
 
); 
";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @"CREATE TABLE [dbo].[LookUp_ItemServiceCategory](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[ServiceCategoryName] [nvarchar](500) NULL,
	[Notes] [nvarchar](500) NULL,
	[IsActive] [int] NULL,
	[OrderIndex] [int] NULL,
	GenderTypeId [int] NULL,
 GenderTypeDescription  [nvarchar](500) NULL 

) ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Account_BranchesimagesLogo](
	[Id] [bigint] NULL,
	[ImageLogoPath] [nvarchar](500) NULL,
	[ImageData] [image] NULL
)   ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[CarsExaminations](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[Paint_Report] [nvarchar](max) NULL,
	[Mechanics_Report] [nvarchar](max) NULL,
	[Suspension_Report] [nvarchar](max) NULL,
	[ExaminationNotes] [nvarchar](max) NULL,
	[CarNumber] [nvarchar](max) NULL,
	[ChassisNumber] [nvarchar](max) NULL,
	[CustomerId] [bigint] NULL,
	[CustomerName] [nvarchar](max) NULL,
	[CarsId] [bigint] NULL,
	[ExaminationDate] [datetime] NULL,
	[ExaminationNumber] [bigint] NULL,
	[CarMeterOnEntry] [nvarchar](500) NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL
)  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[ItemsForCars](
	[Id] [bigint] {IDENTITY} NOT NULL primary key,
	[Item_Id] [bigint] NULL,
	[Lookup_CarTypeId] [bigint] NULL,
	[Lookup_CarTypeModeLId] [bigint] NULL,
	[IsMain] [int] NULL  
)  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"  CREATE TABLE [dbo].[Unit_itemEmployeeBalance](
	[id] [bigint] {IDENTITY} NOT NULL,
	[Item_id] [bigint] NULL,
	[EmployeeId] [bigint] NULL,
	[Store_Type] [int] NULL,
	[Code] [nvarchar](max) NULL,
	[unit_id] [bigint] NULL,
	[Full_qua] [decimal](18, 2) NULL,
	[D_Qua] [decimal](18, 2) NULL,
	[Item_No] [decimal](18, 2) NULL,
	[Reserved_Balance_Larg_Qnty] [decimal](18, 2) NULL,
	[Reserved_Balance_Small_Qnty] [decimal](18, 2) NULL,
	[Big_unit] [int] NULL,
	[Small_unit] [decimal](18, 2) NULL,
	[P_price] [decimal](18, 2) NULL,
	[S_price] [decimal](18, 2) NULL,
	[Specification] [nvarchar](100) NULL,
	[Upon_Request] [decimal](18, 2) NULL,
	[Currency] [nvarchar](50) NULL,
	[active_flage] [int] NULL,
	[TaxSales_Value] [decimal](18, 2) NULL,
	[New2] [int] NULL,
	[OperationDate] [datetime] NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[Update_ServerDateTime] [datetime] NULL,
	[EditOperationDate] [datetime] NULL,
	[Edit_User_Id] [bigint] NULL,
	[ProcessNo] [bigint] NULL,
	[TaxSales10_Value] [decimal](18, 2) NULL,
	[S_price_LargeUnit] [decimal](18, 2) NULL,
	[S_price_SmallUnit] [decimal](18, 2) NULL,
	[P_price_LargeUnit] [decimal](18, 2) NULL,
	[P_price_SmallUnit] [decimal](18, 2) NULL,
	[Unit_Used_For_S_price_Default] [int] NULL,
	[Unit_Used_For_P_price_Default] [int] NULL,
	[S_price_Item_No] [decimal](18, 2) NULL,
	[P_price_Item_No] [decimal](18, 2) NULL,
	[Is_ItemWithoutBalance] [int] NULL
)  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" 
CREATE TABLE [dbo].[ItemCodeTaxAccepted](
	[Id] [bigint] {IDENTITY} primary key NOT NULL,
	[Tax_BranchesId] [bigint] NULL,
	[Item_Id] [bigint] NULL,
	[Barcode] [nvarchar](max) NULL,
	[ParentCode] [nvarchar](100) NULL,
	[CurrencyId] [bigint] NULL,
	[internalCode] [nvarchar](max) NULL,
	[CodeName] [nvarchar](max) NULL,
	[CodeNameAr] [nvarchar](max) NULL,
	[DescriptionEn] [nvarchar](max) NULL,
	[DescriptionAr] [nvarchar](max) NULL,
	[ItemCode] [nvarchar](max) NULL,
	[ItemCodeType] [nvarchar](max) NULL,
	[Price] [decimal](18, 2) NULL,
	[UnitCode] [nvarchar](max) NULL,
	[UnitDescEn] [nvarchar](max) NULL,
	[UnitDescAr] [nvarchar](max) NULL,
	[activeFrom] [datetime] NULL,
	[activeTo] [datetime] NULL,
	[requestReason] [nvarchar](100) NULL,
	[linkedCode] [nvarchar](100) NULL,
	[IsRelatedToLocalItem] [bit] NULL,
	[ParentCodeLookupValue] [nvarchar](500) NULL,
	[codeTypeID] [nvarchar](500) NULL,
	[codeTypeLevelID] [nvarchar](500) NULL,
	[codeTypeLevelNamePrimaryLang] [nvarchar](500) NULL,
	[codeTypeLevelNameSecondaryLang] [nvarchar](500) NULL,
	[parentCodeNamePrimaryLang] [nvarchar](500) NULL,
	[parentCodeNameSecondaryLang] [nvarchar](500) NULL,
	[parentLevelName] [nvarchar](500) NULL,
	[codeTypeNameSecondaryLang] [nvarchar](500) NULL,
	[active] [nvarchar](500) NULL,
	[codeCategorization] [nvarchar](max) NULL,
	[pageNo] [int] NULL,
	[ps_in_pageNo] [int] NULL,
	[Is_Repeated] [int] NULL
)  

 

CREATE TABLE [dbo].[ItemCodeTaxNotUploaded](
	[Id] [bigint] {IDENTITY} primary key  NOT NULL,
	[Item_Id] [bigint] NULL,
	[internalCode] [nvarchar](100) NULL,
	[Tax_BranchesId] [bigint] NULL,
	[CodeType] [nvarchar](100) NULL,
	[ParentCode] [nvarchar](100) NULL,
	[ItemCode] [nvarchar](100) NULL,
	[CodeName] [nvarchar](100) NULL,
	[CodeNameAr] [nvarchar](100) NULL,
	[activeFrom] [datetime] NULL,
	[activeTo] [datetime] NULL,
	[description] [nvarchar](100) NULL,
	[descriptionAr] [nvarchar](100) NULL,
	[requestReason] [nvarchar](100) NULL,
	[linkedCode] [nvarchar](100) NULL,
	[IsUploaded] [bit] NULL
)  
 ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"  CREATE TABLE [dbo].[Lookup_CarEntryTaskeen](
	[Id] [bigint] {IDENTITY} primary key  NOT NULL,
	[Name] [nvarchar](50) NULL,
  )   ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"  CREATE TABLE [dbo].[Lookup_IsInsuranceCompanyStaus](
	[Id] [bigint]   primary key  NOT NULL,
	[Name] [nvarchar](50) NULL,
  )   ";

                str = str + " insert into [dbo].[Lookup_IsInsuranceCompanyStaus](Id,Name) values (1,'صيانة')";
                str = str + "insert into [dbo].[Lookup_IsInsuranceCompanyStaus](Id,Name) values (2,'شركات تأمين')";

                str = ExecuteInsertWithIdentityCheck("Lookup_IsInsuranceCompanyStaus", str);
                opp.insert_me(str);
            }
            catch
            {
            }

            try
            {
                str = @$" CREATE TABLE [dbo].[LookUp_CarStatusLocation](
	[Id] [bigint] {IDENTITY} NOT NULL primary key,
	[Name] [nvarchar](100) NULL 
)   ";
                opp.insert_me(str);
            }
            catch
            {
            }


            try
            {
                str = @$" CREATE TABLE [dbo].[Lookup_CashPaymentMethod](
	[Id] [bigint] {IDENTITY} primary key  NOT NULL,
	[Name] [nvarchar](500) NULL,
  )  
 ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" 
CREATE TABLE [dbo].[DataDictionaryLogerExceptions](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[Exception] [nvarchar](max) NULL,
	[Message] [nvarchar](max) NULL,
	[InnerException] [nvarchar](max) NULL,
	[Source] [nvarchar](max) NULL,
	[StackTrace] [nvarchar](max) NULL,
	[MethodName] [nvarchar](max) NULL,
	[ExcpDateTime] [datetime] NULL
)  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"  

CREATE TABLE [dbo].[Account_BranchesTaxInfo](
	[Id] [bigint] {IDENTITY} NOT NULL primary key,
	[testtype] [nvarchar](500) NULL,
	[CompanyName] [nvarchar](500) NULL,
	[RowCode] [nvarchar](500) NULL,
	[TaxNumber] [nvarchar](500) NULL,
	[RegistrationNumber] [nvarchar](500) NULL,
	[Phone1] [nvarchar](500) NULL,
	[buildingNumber] [nvarchar](500) NULL,
	[postalCode] [nvarchar](500) NULL,
	[room] [nvarchar](500) NULL,
	[Address] [nvarchar](500) NULL,
	[floor] [nvarchar](500) NULL,
	[BranchID] [nvarchar](500) NULL,
	[RegionCity] [nvarchar](500) NULL,
	[databasename] [nvarchar](500) NULL,
	[lastuuid] [nvarchar](500) NULL,
	[landmark] [nvarchar](500) NULL,
	[additionalInformation] [nvarchar](500) NULL,
	[Governate] [nvarchar](500) NULL,
	[Street] [nvarchar](500) NULL,
	[BankName] [nvarchar](500) NULL,
	[AccountNo] [nvarchar](500) NULL,
	[IBANNumber] [nvarchar](500) NULL,
	[BankAddress] [nvarchar](500) NULL,
	[ClientId] [nvarchar](500) NULL,
	[ClientSecret] [nvarchar](500) NULL,
	[ActivityCode] [nvarchar](500) NULL,
	[TokenPin] [nvarchar](500) NULL,
	[documentTypeVersion] [nvarchar](500) NULL,
	[ClientIdPOS] [nvarchar](500) NULL,
	[ClientSecretPOS] [nvarchar](500) NULL,
	[SerialDevice] [nvarchar](500) NULL,
	[SystemVersion] [nvarchar](500) NULL,
	[documentTypeVersionPOS] [nvarchar](500) NULL,
  
)  
 

 ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"  
CREATE TABLE [dbo].[LookUp_CodeTaxType](
	[Id] [bigint] primary key NOT NULL,
	[Name] [nvarchar](50) NULL,
  )  
   
CREATE TABLE [dbo].Items_Group(
	[Id] [bigint] {IDENTITY} NOT NULL,
	[Name] [nvarchar](50) NULL,
  ) 
  
CREATE TABLE [dbo].[LookUp_TaxableSubTypes](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[Code] [nvarchar](200) NULL,
	[NameAr] [nvarchar](200) NULL,
	[NameEn] [nvarchar](200) NULL,
	[LookUp_TaxableTypeId] [bigint] NULL
)  

CREATE TABLE [dbo].[LookUp_TaxableTypes](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[Code] [nvarchar](200) NULL,
	[NameAr] [nvarchar](200) NULL,
	[NameEn] [nvarchar](200) NULL
)  

CREATE TABLE [dbo].LookUp_NonTaxableType(
	[Id] [bigint] {IDENTITY} NOT NULL,
	[Code] [nvarchar](200) NULL,
	[NameAr] [nvarchar](200) NULL,
	[NameEn] [nvarchar](200) NULL
)  
 ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo]. Items_Group(
 
	[Id] [bigint] {IDENTITY} primary key NOT NULL,
	[ ItemsGroup_Name] [nvarchar](100) NULL,
	[OperationDate] [datetime] NULL      DEFAULT (getdate()),
	[User_Id] [bigint] NULL      DEFAULT ((0)),
	[Branch_Id] [bigint] NULL      DEFAULT ((0)),
	[Insert_ServerDateTime] [datetime] NULL      DEFAULT (getdate()),
	[Update_ServerDateTime] [datetime] NULL      DEFAULT (getdate()),
	[EditOperationDate] [datetime] NULL      DEFAULT (getdate()),
	[Edit_User_Id] [bigint] NULL      DEFAULT ((0)),
	[ProcessNo] [bigint] NULL      DEFAULT ((0)),
	[IsActive] [int] NULL DEFAULT ((1))
)  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"  CREATE TABLE[dbo].Items_Nature(

    [Id][bigint] IDENTITY(1, 1) primary key NOT NULL,
    [ItemsNature_Name][nvarchar](100) NULL,
    [OperationDate][datetime] NULL      DEFAULT(getdate()),
    [User_Id][int] NULL      DEFAULT((0)),
    [Branch_Id][int] NULL      DEFAULT((0)),
    [Insert_ServerDateTime][datetime] NULL      DEFAULT(getdate()),
    [Update_ServerDateTime][datetime] NULL      DEFAULT(getdate()),
    [EditOperationDate][datetime] NULL      DEFAULT(getdate()),
    [Edit_User_Id][int] NULL      DEFAULT((0)),
    [ProcessNo][bigint] NULL      DEFAULT((0)),
    [IsActive][int] NULL DEFAULT((1))
) ";
                opp.insert_me(str);
            }
            catch
            {
            }



            try
            {
                str = @$"  CREATE TABLE [dbo].[Gov_City_List](
	[Id] [bigint] {IDENTITY} primary key NOT NULL,
	[NameAr] [nvarchar](100) NULL,
	[NameEn] [nvarchar](100) NULL,
	[Code] [nvarchar](100) NULL,
	[OperationDate] [datetime] NULL      DEFAULT (getdate()),
	[User_Id] [bigint] NULL      DEFAULT ((0)),
	[Branch_Id] [bigint] NULL      DEFAULT ((0)),
	[Insert_ServerDateTime] [datetime] NULL      DEFAULT (getdate()),
	[Update_ServerDateTime] [datetime] NULL      DEFAULT (getdate()),
	[EditOperationDate] [datetime] NULL      DEFAULT (getdate()),
	[Edit_User_Id] [bigint] NULL      DEFAULT ((0)),
	[ProcessNo] [bigint] NULL      DEFAULT ((0)),
	[Gov_Country_ListId] [bigint] NULL      DEFAULT ((0)),
	[Govern_Area_ListId] [bigint] NULL      DEFAULT ((0)),
	[Govern_ListId] [bigint] NULL      DEFAULT ((0)),
 
) 
";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"   

CREATE TABLE [dbo].[Gov_City_List](
	[Id] [bigint] {IDENTITY} primary key NOT NULL,
	[NameAr] [nvarchar](100) NULL,
	[NameEn] [nvarchar](100) NULL,
	[Code] [nvarchar](100) NULL,
	[OperationDate] [datetime] NULL DEFAULT (getdate()) ,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL DEFAULT (getdate()) ,
	[Update_ServerDateTime] [datetime] NULL DEFAULT (getdate()) ,
	[EditOperationDate] [datetime] NULL DEFAULT (getdate()) ,
	[Edit_User_Id] [bigint] NULL,
	[ProcessNo] [bigint] NULL,
	[Gov_Country_ListId] [bigint] NULL,
	[Govern_Area_ListId] [bigint] NULL,
	[Govern_ListId] [int] NULL 
) 
 
 
";
                opp.insert_me(str);
            }
            catch
            {
            }

            try
            {
                str = @$"   
 
 

CREATE TABLE [dbo].[Gov_Country_List](
	[Id] [bigint] {IDENTITY} primary key  NOT NULL,
	[NameAr] [nvarchar](100) NULL,
	[NameEn] [nvarchar](100) NULL,
	[Code] [nvarchar](100) NULL,
	[OperationDate] [datetime] NULL DEFAULT (getdate()) ,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL DEFAULT (getdate()) ,
	[Update_ServerDateTime] [datetime] NULL DEFAULT (getdate()) ,
	[EditOperationDate] [datetime] NULL DEFAULT (getdate()) ,
	[Edit_User_Id] [bigint] NULL,
	[ProcessNo] [bigint] NULL 
) 
 

 
";
                opp.insert_me(str);
            }
            catch
            {
            }


            try
            {
                str = @$"   
 
CREATE TABLE [dbo].[Govern_Area_List](
	[Id] [bigint] {IDENTITY}  primary key NOT NULL,
	[Area_Name] [nvarchar](100) NULL,
	[OperationDate] [datetime] NULL DEFAULT (getdate()) ,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL DEFAULT (getdate()) ,
	[Update_ServerDateTime] [datetime] NULL DEFAULT (getdate()) ,
	[EditOperationDate] [datetime] NULL DEFAULT (getdate()) ,
	[Edit_User_Id] [bigint] NULL,
	[ProcessNo] [bigint] NULL,
	[Gov_Country_ListId] [bigint] NULL 
) 

 
";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"   
 
CREATE TABLE [dbo].[Govern_List](
	[Id] [bigint] {IDENTITY}  primary key NOT NULL,
	[Gov_Name] [nvarchar](100) NULL,
	[Code] [nvarchar](100) NULL,
	[OperationDate] [datetime] NULL DEFAULT (getdate()) ,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL DEFAULT (getdate()) ,
	[Update_ServerDateTime] [datetime] NULL DEFAULT (getdate()) ,
	[EditOperationDate] [datetime] NULL DEFAULT (getdate()) ,
	[Edit_User_Id] [bigint] NULL,
	[ProcessNo] [bigint] NULL,
	[Gov_Country_ListId] [bigint] NULL,
	[Govern_Area_ListId] [bigint] NULL 
)
";
                opp.insert_me(str);
            }
            catch
            {
            }


            try
            {
                str = @$"  
CREATE TABLE [dbo].[Cars](
	[Id] [bigint] {IDENTITY} PRIMARY KEY NOT NULL,
	[CustomerId] [bigint] NULL,
	[ChassisNumber] [nvarchar](50) NULL,
	[CarNumber] [nvarchar](50) NULL,
	[CarType] [nvarchar](50) NULL,
	[Model] [nvarchar](50) NULL,
	[EngineNumber] [nvarchar](50) NULL,
	[FuelType] [nvarchar](50) NULL,
	[Capacity] [nvarchar](50) NULL,
	[Year] [nvarchar](50) NULL,
	[TransmissionType] [nvarchar](50) NULL,
	[ColorCode] [nvarchar](50) NULL,
	[CarColor] [nvarchar](50) NULL,
	[CustomerName] [nvarchar](100) NULL,
	[WarrantyStartDate] [datetime] NULL,
	[WarrantyEndDate] [datetime] NULL,
	[InsuranceCompany] [nvarchar](100) NULL,
	[Notes] [nvarchar](max) NULL,
	[Lookup_CarShapeId] [bigint] NULL 
  
) 
";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Checks_MemoryMainDataList](
	[Id] [bigint] {IDENTITY} NOT NULL primary key,
	[Check_No] [nvarchar](50) NULL,
	[Day_Date] [datetime] NULL,
	[Due_Date] [datetime] NULL,
	[Bank_id] [bigint] NULL,
	[Check_Value] [decimal](18, 2) NULL,
	[Check_Value_] [nvarchar](500) NULL,
	[Due_Check] [nvarchar](500) NULL,
	[Value_Receipt_Date] [datetime] NULL,
	[Notes] [nvarchar](500) NULL,
	[Exchanger_Name] [nvarchar](500) NULL,
	[First_Drawer_Name] [nvarchar](500) NULL,
	[Last_Drawer_Name] [nvarchar](500) NULL,
	[Bank_Name] [nvarchar](500) NULL,
[Receipt] [int] NULL,
	[Check_Notes] [nvarchar](500) NULL)
";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @"CREATE TABLE [dbo].[ALogError](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[mysql] [nvarchar](1000) NULL,
	[myDate] [datetime] NULL CONSTRAINT [DF_ALogError_myDate]  DEFAULT (getdate()),
	[message] [nvarchar](1000) NULL
)
";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @"CREATE TABLE [dbo].[Checks_MemoryHistory](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[Checks_MemoryId] [bigint] NULL,
	[Account_Entry_List_ID] [bigint] NULL,
	[SubAccount_Id_RelatedTo] [bigint] NULL,
	[SubAccount_Name_RelatedTo] [nvarchar](500) NULL,
	[ProcessNo] [bigint] NULL,
	[SubAccount_Id_RelatedToBefor] [bigint] NULL,
	[SubAccount_Name_RelatedToBefor] [nvarchar](500) NULL
) ";
                opp.insert_me(str);
            }
            catch
            {
            }

            Set_Account_Type_Table(ref opp);
            try
            {
                str = @$"  
CREATE TABLE [dbo].[Work_In_Details](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[Customer_Id] [int] NULL,
	[Request_Date] [datetime] NULL,
	[Unit_Item_Table_Id] [bigint] NULL,
	[Request_Qnty_Larg] [float] NULL,
	[Request_Qnty_Small] [float] NULL,
	[Request_Item_NO] [float] NULL,
	[Export_Qnty_Larg] [float] NULL,
	[Export_Qnty_Small] [float] NULL,
	[Export_Item_No] [float] NULL,
	[Item_Exported_Finish] [int] NULL,
	[Notes] [nvarchar](500) NULL,
	[Item_Id] [int] NULL,
	[OperationDate] [datetime] NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[Update_ServerDateTime] [datetime] NULL,
	[EditOperationDate] [datetime] NULL,
	[Edit_User_Id] [bigint] NULL
)  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$"
CREATE TABLE [dbo].[Work_In_Details_Balance](
	[Id] [bigint]  {IDENTITY} NOT NULL,
	[Unit_Item_Table_Id] [bigint] NULL,
	[Balance_Qnty_Larg] [float] NULL,
	[Balance_Qnty_Small] [float] NULL,
	[Balance_Item_NO] [float] NULL,
	[Customer_Id] [bigint] NULL,
	[Item_Id] [int] NULL,
	[OperationDate] [datetime] NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[Update_ServerDateTime] [datetime] NULL,
	[EditOperationDate] [datetime] NULL,
	[Edit_User_Id] [bigint] NULL
)   ";
                opp.insert_me(str);
            }
            catch
            {
            }

            try
            {
                str = @$"
   
CREATE TABLE [dbo].[Work_In_Details_Received](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[WorkDate] [datetime] NULL,
	[Work_In_Details_Id] [bigint] NOT NULL,
	[Process_No] [bigint] NULL,
	[Draw_Id] [bigint] NULL,
	[Draw_Name] [nvarchar](50) NULL,
	[Received_Qnty_Larg] [float] NULL,
	[Received_Qnty_Small] [float] NULL,
	[Received_Item_NO] [float] NULL,
	[Price] [float] NULL,
	[Total_Price] [float] NULL,
	[Price_Selected_Index] [int] NULL,
	[Customer_Id] [bigint] NULL,
	[Unit_Item_Table_Id] [bigint] NULL,
	[CustomerAccountBefor] [float] NULL,
	[CustomerAccountAfter] [float] NULL,
	[Item_Id] [int] NULL,
	[OperationDate] [datetime] NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[Update_ServerDateTime] [datetime] NULL,
	[EditOperationDate] [datetime] NULL,
	[Edit_User_Id] [bigint] NULL
)   ";
                opp.insert_me(str);
            }
            catch
            {
            }

            try
            {
                str = @$"

CREATE TABLE [dbo].[Work_Out_Details](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[Supplier_Id] [int] NULL,
	[Request_Date] [datetime] NULL,
	[Unit_Item_Table_Id] [bigint] NULL,
	[Request_Qnty_Larg] [float] NULL,
	[Request_Qnty_Small] [float] NULL,
	[Request_Item_NO] [float] NULL,
	[Export_Qnty_Larg] [float] NULL,
	[Export_Qnty_Small] [float] NULL,
	[Export_Item_No] [float] NULL,
	[Item_Exported_Finish] [int] NULL,
	[Notes] [nvarchar](500) NULL,
	[Item_Id] [int] NULL,
	[OperationDate] [datetime] NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[Update_ServerDateTime] [datetime] NULL,
	[EditOperationDate] [datetime] NULL,
	[Edit_User_Id] [bigint] NULL
)   ";
                opp.insert_me(str);
            }
            catch
            {
            }

            try
            {
                str = @$"  CREATE TABLE [dbo].[LookUp_TagerKontra](
	[Id] [bigint] {IDENTITY}   primary key,
	[Name] [nvarchar](100) NULL )
 
  ";
                opp.insert_me(str);
            }
            catch
            {
            }





            try
            {
                str = @$"
CREATE TABLE [dbo].[Work_Out_Details_Balance](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[Unit_Item_Table_Id] [bigint] NULL,
	[Balance_Qnty_Larg] [float] NULL,
	[Balance_Qnty_Small] [float] NULL,
	[Balance_Item_NO] [float] NULL,
	[Supplier_Id] [bigint] NULL,
	[Item_Id] [int] NULL,
	[OperationDate] [datetime] NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[Update_ServerDateTime] [datetime] NULL,
	[EditOperationDate] [datetime] NULL,
	[Edit_User_Id] [bigint] NULL
)    ";
                opp.insert_me(str);
            }
            catch
            {
            }

            try
            {
                str = @$"
 
CREATE TABLE [dbo].[Work_Out_Details_Received](
	[Id] [bigint] {IDENTITY} NOT NULL,
	[WorkDate] [datetime] NULL,
	[Work_Out_Details_Id] [bigint] NOT NULL,
	[Process_No] [bigint] NULL,
	[Draw_Id] [bigint] NULL,
	[Draw_Name] [nvarchar](50) NULL,
	[Received_Qnty_Larg] [float] NULL,
	[Received_Qnty_Small] [float] NULL,
	[Received_Item_NO] [float] NULL,
	[Price] [float] NULL,
	[Total_Price] [float] NULL,
	[Price_Selected_Index] [int] NULL,
	[Supplier_Id] [bigint] NULL,
	[Unit_Item_Table_Id] [bigint] NULL,
	[CustomerAccountBefor] [float] NULL,
	[CustomerAccountAfter] [float] NULL,
	[Item_Id] [int] NULL,
	[OperationDate] [datetime] NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[Update_ServerDateTime] [datetime] NULL,
	[EditOperationDate] [datetime] NULL,
	[Edit_User_Id] [bigint] NULL
)    ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[Unit_Item_ExpireDate]( [Id] [bigint] {IDENTITY} NOT NULL, [Item_id] [bigint] NULL, [Unit_Item_Table_Id] [bigint] NULL, [Tash3ela] [nvarchar](50) NULL, [Item_Expire_Date] [datetime] NULL, [Larg_Balance] [decimal](18, 2) NULL, [Small_Balance] [decimal](18, 2) NULL, [Item_No] [decimal](18, 2) NULL )";
                opp.insert_me(str);
            }
            catch
            {
            }
            //Set_TaxInfo(ref opp);
            try
            {
                str = @$"  CREATE TABLE [dbo].[Account_Entry_Master_Balance]( [Id] [bigint] {IDENTITY} NOT NULL, [Submain_Account_ID] [bigint] NULL      DEFAULT ((0)), [SubAccount_Name] [nvarchar](100) NULL, [Year_Financial_Period_Id] [int] NULL      DEFAULT ((0)), [Branch_Id] [bigint] NULL, [Account_Currency_Id] [int] NULL      DEFAULT ((0)), [Equivalent_Account_Currency_Id] [int] NULL, [First_Balance] [decimal](18, 3) NULL      DEFAULT ((0)), [Equivalent_First_Balance] [decimal](18, 3) NULL      DEFAULT ((0)), [First_Balance_Madin] [decimal](18, 3) NULL      DEFAULT ((0)), [First_Balance_Da2n] [decimal](18, 3) NULL      DEFAULT ((0)), [Madin_Value] [decimal](18, 3) NULL      DEFAULT ((0)), [Da2n_Value] [decimal](18, 3) NULL      DEFAULT ((0)), [Balance] [decimal](18, 3) NULL      DEFAULT ((0)), [Balance_Madin] [decimal](18, 3) NULL      DEFAULT ((0)), [Balance_Da2n] [decimal](18, 3) NULL      DEFAULT ((0)), [Equivalent_First_Balance_Madin] [decimal](18, 3) NULL      DEFAULT ((0)), [Equivalent_First_Balance_Da2n] [decimal](18, 3) NULL      DEFAULT ((0)), [Equivalent_Madin_Value] [decimal](18, 3) NULL      DEFAULT ((0)), [Equivalent_Da2n_Value] [decimal](18, 3) NULL      DEFAULT ((0)), [Equivalent_Balance_Madin] [decimal](18, 3) NULL      DEFAULT ((0)), [Equivalent_Balance_Da2n] [decimal](18, 3) NULL      DEFAULT ((0)), [IS_Madin_Da2n] [int] NULL      DEFAULT ((0)), [Cost_Center_Tree_ID] [int] NULL      DEFAULT ((0)) )  ";
                opp.insert_me(str);
            }
            catch
            {
            }


            try
            {
                str = @$" CREATE TABLE [dbo].[DataDictionary]( [ID] [bigint] {IDENTITY} NOT NULL, [SQLText] [nvarchar](max) NULL, [Read_Flage] [bigint] NULL, [Insert_ServerDateTime] [datetime] NULL      DEFAULT (getdate()), [User_Id] [bigint] NULL )  ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = @$" CREATE TABLE [dbo].[ReportSellPurchaseItems]( [Id] [bigint] {IDENTITY} NOT NULL, [Item_Id] [bigint] NULL, [Unit_Item_Table_Id] [bigint] NULL, [Invoices_List_Id] [bigint] NULL, [Invoice_Date] [datetime] NULL, [Item_Name] [nvarchar](500) NULL, [Larg_Qnty] [decimal](18, 2) NULL, [Small_Qnty] [decimal](18, 2) NULL, [Qnty_Another] [decimal](18, 2) NULL, [Customer_Id] [bigint] NULL, [Customer_Name] [nvarchar](500) NULL, [actual_S_price] [decimal](18, 2) NULL, [Total_SellPrice] [decimal](18, 2) NULL, [actual_P_price] [decimal](18, 2) NULL, [Total_PurhasePrice] [decimal](18, 2) NULL, [Diff_Total] [decimal](18, 2) NULL, [Invoice_Type] [int] NULL ) ";
                opp.insert_me(str);
            }
            catch
            {
            }
        }
        static void AlterBD_(ref operations opp, string TableName, string ColumnName, string DataType, string Default, string UpdateValue)
        {
            string mysql = "";
            try
            {
                mysql = @$" Alter Table " + TableName + "  Add  " + ColumnName + " " + DataType + " Default(" + Default + ")  NULL  ";
                int c = opp.insert_me(mysql);

                try
                {


                    mysql = @"Update " + TableName + "  Set  " + ColumnName + " = " + UpdateValue + "";
                    c = opp.insert_me(mysql);
                }
                catch (Exception ee) { }
            }
            catch (Exception ee) { }

        }
        static void AlterBD__(ref operations opp, string TableName, string ColumnName, string DataType, string msql)
        {
            string mysql = "";
            try
            {
                mysql = @$" Alter Table " + TableName + "  Add  " + ColumnName + " " + DataType + "   ";
                int c = opp.insert_me(mysql);

                try
                {


                    //mysql = @"Update " + TableName + "  Set  " + ColumnName + " = " + UpdateValue + "";
                    c = opp.insert_me(msql);
                }
                catch (Exception ee) { }
            }
            catch (Exception ee) { }

        }
        static void ChangeTableName(ref operations opp, string TableNameOld, string TableNameNew)
        {
            string mysql = "";
            try
            {
                mysql = "EXEC sp_rename '" + TableNameOld + "', '" + TableNameNew + "'";
                // opp.db.Database.ExecuteSqlRaw(mysql);
                opp.insert_me(mysql);

            }
            catch (Exception ee) { }

        }
        static void AlterBD_(ref operations opp, string TableName, string ColumnName, string DataType, string Default, string UpdateValue, bool dropUpdateValue)
        {
            
            string mysql = "";
            try
            {
                mysql = @$" Alter Table " + TableName + "  Add  " + ColumnName + " " + DataType + " Default(" + Default + ")    ";
                int c = opp.insert_me(mysql);

                mysql = @"Update " + TableName + "  Set  " + ColumnName + " = " + UpdateValue + "";
                c = opp.insert_me(mysql);

                try
                {

                    mysql = @$" Alter Table " + TableName + "  drop column  " + UpdateValue + "  ";
                    opp.insert_me(mysql);
                }
                catch (Exception ee) { }
            }
            catch (Exception ee) { }
        }
        static void AlterBD_dropColumn(ref operations opp, string TableName, string ColumnName)
        {
            string mysql = "";
            try
            {
                mysql = @$" Alter Table " + TableName + "  DROP COLUMN   " + ColumnName + "   ";
                opp.insert_me(mysql);
            }
            catch { }
        }
        static void AlterBD_dropTable(ref operations opp, string TableName)
        {
            string mysql = "";
            try
            {

                mysql = @"DROP TABLE IF EXISTS dbo." + TableName + "     ";
                //mysql = @$" drop Table " + TableName + "     ";
                opp.insert_me(mysql);
            }
            catch { }
        }
        static void AlterBD_drop(ref operations opp, string TableName, string ColumnNameOldToDrop, string DataType, string Default, string UpdateValue)
        {
            string mysql = "";
            try
            {
                mysql = @$" Alter Table " + TableName + "  DROP COLUMN   " + ColumnNameOldToDrop + "   ";
                opp.insert_me(mysql);
            }
            catch { }

            try
            {
                mysql = @$" Alter Table " + TableName + "  Add  " + ColumnNameOldToDrop + " " + DataType + " Default(" + Default + ")    ";
                opp.insert_me(mysql);

                mysql = @"Update " + TableName + "  Set  " + ColumnNameOldToDrop + " = " + UpdateValue + "";
                opp.insert_me(mysql);
            }
            catch { }
        }
        static void AlterBD_drop(ref operations opp, string TableName, string ColumnNameOldToDrop, string ColumnNameNew, string DataType, string Default, string UpdateValue)
        {
            string mysql = "";


            try
            {
                mysql = @$" Alter Table " + TableName + "  Add  " + ColumnNameNew + " " + DataType + " Default(" + Default + ")    ";
                opp.insert_me(mysql);

                mysql = @"Update " + TableName + "  Set  " + ColumnNameNew + " = " + UpdateValue + "";
                opp.insert_me(mysql);
            }
            catch { }
            try
            {
                mysql = @$" Alter Table " + TableName + "  DROP COLUMN   " + ColumnNameOldToDrop + "   ";
                opp.insert_me(mysql);
            }
            catch { }
        }

        static void AlterBD_(ref operations opp, string TableName, string ColumnName, string DataType, string Default, string UpdateValue, string Update_Query)
        {
            string mysql = "";
            try
            {
                mysql = @$" Alter Table " + TableName + "  Add  " + ColumnName + " " + DataType + " Default(" + Default + ")    ";
                opp.insert_me(mysql);

                mysql = @"Update " + TableName + "  Set  " + ColumnName + " = " + UpdateValue + "";
                opp.insert_me(mysql);


                opp.insert_me(Update_Query);

            }
            catch { }
        }


        static void AlterBD_(ref operations opp, string TableName, string ColumnName, string DataType, string Default)
        {
            string mysql = "";
            try
            {
                mysql = @$" Alter Table " + TableName + "  Add  " + ColumnName + " " + DataType + " Default(" + Default + ")    ";
                opp.insert_me(mysql);
            }
            catch { }
        }
        static void AlterBD_(ref operations opp, string TableName, string ColumnName, string DataType)
        {
            string mysql = "";
            try
            {
                mysql = @$" Alter Table " + TableName + "  Add  " + ColumnName + " " + DataType + "   ";
                opp.insert_me(mysql);
                Thread.Sleep(xmiliseconds);
            }
            catch (Exception ee) { }
        }


        private static void AlterBD_1(ref operations opp, string TableName, string ColumnName, string DataType)
        {
            string str = "";
            try
            {
                string[] textArray1 = new string[] { " Alter Table ", TableName, "  Add  ", ColumnName, " ", DataType, "   " };
                str = string.Concat(textArray1);
                opp.insert_me(str);
            }
            catch
            {
            }
        }

        private static void AlterBD_1(ref operations opp, string TableName, string ColumnName, string DataType, string Default)
        {
            string str = "";
            try
            {
                string[] textArray1 = new string[9];
                textArray1[0] = " Alter Table ";
                textArray1[1] = TableName;
                textArray1[2] = "  Add  ";
                textArray1[3] = ColumnName;
                textArray1[4] = " ";
                textArray1[5] = DataType;
                textArray1[6] = " Default(";
                textArray1[7] = Default;
                textArray1[8] = ")    ";
                str = string.Concat(textArray1);
                opp.insert_me(str);
            }
            catch
            {
            }
        }

        private static void AlterBD_1(ref operations opp, string TableName, string ColumnName, string DataType, string Default, string UpdateValue)
        {
            string str = "";
            try
            {
                string[] textArray1 = new string[9];
                textArray1[0] = " Alter Table ";
                textArray1[1] = TableName;
                textArray1[2] = "  Add  ";
                textArray1[3] = ColumnName;
                textArray1[4] = " ";
                textArray1[5] = DataType;
                textArray1[6] = " Default(";
                textArray1[7] = Default;
                textArray1[8] = ")    ";
                str = string.Concat(textArray1);
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                string[] textArray2 = new string[] { "Update ", TableName, "  Set  ", ColumnName, " = ", UpdateValue };
                str = string.Concat(textArray2);
                opp.insert_me(str);
            }
            catch
            {
            }
        }

        private static void AlterBD_1(ref operations opp, string TableName, string ColumnName, string DataType, string Default, string UpdateValue, string Update_Query)
        {
            string str = "";
            try
            {
                string[] textArray1 = new string[9];
                textArray1[0] = " Alter Table ";
                textArray1[1] = TableName;
                textArray1[2] = "  Add  ";
                textArray1[3] = ColumnName;
                textArray1[4] = " ";
                textArray1[5] = DataType;
                textArray1[6] = " Default(";
                textArray1[7] = Default;
                textArray1[8] = ")    ";
                str = string.Concat(textArray1);
                opp.insert_me(str);
                string[] textArray2 = new string[] { "Update ", TableName, "  Set  ", ColumnName, " = ", UpdateValue };
                str = string.Concat(textArray2);
                opp.insert_me(str);
                opp.insert_me(Update_Query);
            }
            catch
            {
            }
        }
        public static void AlterColumn(ref operations opp, string TableName, string ColumnName, string DataType, string ValueBeforEdit, string ValueAfterEdit)
        {
            string mysql = "";
            try
            {
                mysql = "Update " + TableName + " Set " + ColumnName + "= " + ValueBeforEdit + "";
                opp.insert_me(mysql);
            }
            catch (Exception)
            {

            }

            AlterColumn(ref opp, TableName, ColumnName, DataType);
            try
            {
                mysql = "Update " + TableName + " Set " + ColumnName + "= " + ValueAfterEdit + "";
                opp.insert_me(mysql);
            }
            catch (Exception)
            {

            }

        }

        public static void AlterColumnChangeName(ref operations opp, string TableName, string ColumnNameOld, string ColumnNameNew, string DataType, string DefaultValue)
        {
            string mysql = "";

            mysql = " Select * from  " + TableName + " Where Id=0";
            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Columns.Contains(ColumnNameNew))
            {

                return;
            }
            AlterBD_(ref opp, TableName, ColumnNameNew, DataType, DefaultValue);

            try
            {
                mysql = "Update " + TableName + " Set " + ColumnNameNew + "= " + ColumnNameOld + "";
                opp.insert_me(mysql);
            }
            catch (Exception)
            {

            }

            AlterBD_dropColumn(ref opp, TableName, ColumnNameOld);


        }



        public static void AlterColumnAddPrimaryKey(ref operations opp, string TableName, string ColumnName)
        {
            string mysql = "";
            try
            {

                mysql = @$" Alter Table " + TableName + "    ADD PRIMARY KEY(" + ColumnName + ")  ";
                opp.insert_me(mysql);
            }
            catch (Exception)
            {

            }


        }
        public static void AlterColumnAddPrimaryKey(ref operations opp, string TableName, string ColumnNameOld, string ColumnNameNew)
        {

            try
            {
                // استخدام $ مع وضع أسماء المتغيرات مباشرة بين الأقواس المعقوفة
                string formattedScript = $@"    
-- 1. حذف الـ Primary Key القديم تلقائياً
DECLARE @ConstraintName NVARCHAR(200);
SELECT @ConstraintName = CONSTRAINT_NAME
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
WHERE TABLE_NAME = '{TableName}' AND CONSTRAINT_TYPE = 'PRIMARY KEY';

IF @ConstraintName IS NOT NULL
BEGIN
    DECLARE @Sql NVARCHAR(MAX) = 'ALTER TABLE [dbo].[{TableName}] DROP CONSTRAINT ' + QUOTENAME(@ConstraintName);
    EXEC sp_executesql @Sql;
END;

-- 2. إعادة تسمية العمود
EXEC sp_rename '[dbo].[{TableName}].[{ColumnNameOld}]', '{ColumnNameNew}', 'COLUMN';

-- 3. جعل العمود الجديد NOT NULL
ALTER TABLE [dbo].[{TableName}] 
ALTER COLUMN [{ColumnNameNew}] BIGINT NOT NULL; 

-- 4. إنشاء الـ Primary Key الجديد
ALTER TABLE [dbo].[{TableName}] 
ADD CONSTRAINT [PK_{TableName}_{ColumnNameNew}] PRIMARY KEY CLUSTERED ([{ColumnNameNew}]);
";

                opp.insert_me(formattedScript);
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ لمعرفة سبب الفشل في SQL
                AccountModelsLib.operations.DataDictionaryLogerException(ex);
                throw;
            }/*
            string mysql = "";
            try
            {
                mysql = @$"    
                
    -- 1. حذف الـ Primary Key القديم تلقائياً
    DECLARE @ConstraintName NVARCHAR(200);
    SELECT @ConstraintName = CONSTRAINT_NAME
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_NAME = '{0}' AND CONSTRAINT_TYPE = 'PRIMARY KEY';

    IF @ConstraintName IS NOT NULL
    BEGIN
        DECLARE @Sql NVARCHAR(MAX) = 'ALTER TABLE dbo.{0} DROP CONSTRAINT ' + QUOTENAME(@ConstraintName);
        EXEC sp_executesql @Sql;
    END
    --GO

    -- 2. إعادة تسمية العمود من id2 إلى id
    EXEC sp_rename 'dbo.{0}.{1}', '{2}', 'COLUMN';
    --GO

    -- 3. جعل العمود الجديد NOT NULL
    ALTER TABLE dbo.{0} 
    ALTER COLUMN {2} BIGINT NOT NULL; 
    --GO

    -- 4. إنشاء الـ Primary Key الجديد على عمود id
    ALTER TABLE dbo.{0} 
    ADD CONSTRAINT PK_{0}_id PRIMARY KEY ({2});
    --GO";

                // تعبئة اسم الجدول في السكريبت
                string formattedScript = string.Format(mysql, TableName, ColumnNameOld, ColumnNameNew);

                // تقسيم السكريبت إلى أوامر منفصلة بناءً على كلمة GO
                //string[] commandBatches = Regex.Split(
                //    formattedScript,
                //    @"^\s*GO\s*$",
                //    RegexOptions.IgnoreCase | RegexOptions.Multiline
                //);


                opp.insert_me(formattedScript);
            }
            catch (Exception)
            {

            }
            */

        }
        public static void AlterColumnAddKey(ref operations opp, string TableName, string ColumnName)
        {
            IsColumnAltered = false;
            string mysql = "";
            try
            {


                mysql = @$" Alter Table " + TableName + "  ADD CONSTRAINT  PK_" + TableName + "  PRIMARY KEY(" + ColumnName + ")   ";
                opp.insert_me(mysql);
                IsColumnAltered = true;
                Thread.Sleep(xmiliseconds);
            }
            catch (Exception ee)
            {
                Thread.Sleep(xmiliseconds);



            }
        }

        public static void AlterColumn(ref operations opp, string TableName, string ColumnName, string DataType)
        {
            IsColumnAltered = false;
            string mysql = "";
            try
            {

                mysql = @$" Alter Table " + TableName + "  alter column  " + ColumnName + " " + DataType + "   ";
                opp.insert_me(mysql);
                IsColumnAltered = true;
                Thread.Sleep(xmiliseconds);
            }
            catch (Exception ee)
            {
                Thread.Sleep(xmiliseconds);
                ALogError(ref opp, mysql, ee.Message);

                string[] words = ee.Message.Split('\'');
                try
                {
                    mysql = @$" Alter Table " + TableName + "  Drop CONSTRAINT  " + words[1] + "   ";
                    opp.insert_me(mysql);

                    mysql = @$" Alter Table " + TableName + "  Drop CONSTRAINT DF_" + TableName + "_" + ColumnName + "   ";
                    opp.insert_me(mysql);

                    AlterColumn(ref opp, TableName, ColumnName, DataType);
                }
                catch (Exception eee)
                {
                    ALogError(ref opp, mysql, eee.Message);
                }


            }
        }
        static void ALogError(ref operations opp, string mysql, string Errmessage)
        {
            try
            {
                mysql = mysql.Replace("'", " ");
                Errmessage = Errmessage.Replace("'", " ");
                mysql = @$"  Insert Into ALogError(mysql,message) values(N'" + mysql + "',N'" + Errmessage + "') ";
                mysql = ExecuteInsertWithIdentityCheck("ALogError", mysql);

                opp.insert_me(mysql);
            }
            catch (Exception ee)
            {

            }
        }
        static void TruncateALogError(ref operations opp)
        {
            try
            {
                string mysql = @$"  Truncate Table ALogError  ";
                opp.insert_me(mysql);
            }
            catch (Exception ee)
            {

            }
        }
        public static void Set_Account_Type_Table()
        {
            operations opp = new operations();
            Set_Account_Type_Table(ref opp);
        }
        public static void Set_Account_Type_Table(ref operations opp)
        {
            //string str = "";
            //try
            //{
            //    string str = @$" ";
            //    opp.insert_me(str);

            //}
            //catch
            //{
            //}

            try
            {
                string str = @$" drop TABLE [dbo].[OrderTypes_Label]";
                opp.insert_me(str);

            }
            catch
            {
            }

            try
            {


                string str = @$"    
CREATE TABLE [dbo].[OrderTypes_Label](
	[TypesID] [bigint] NOT NULL,
	[LabelFrom] [nvarchar](50) NULL,
	[LabelTo] [nvarchar](50) NULL,
	[Label_Name] [nvarchar](50) NULL,
	[OperationDate] [datetime] NULL DEFAULT (getdate()),
	[User_Id] [bigint] NULL DEFAULT ((0)),
	[Branch_Id] [bigint] NULL DEFAULT ((0)),
	[Insert_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
	[Update_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
	[EditOperationDate] [datetime] NULL DEFAULT (getdate()),
	[Edit_User_Id] [bigint] NULL DEFAULT ((0)),
	[ProcessNo] [bigint] NULL DEFAULT ((0)) 
  )  

 
INSERT [dbo].[OrderTypes_Label] ([TypesID], [LabelFrom], [LabelTo], [Label_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (-2, N'تعديل رصيد أول المدة', N'تعديل رصيد أول المدة', N'تعديل رصيد أول المدة', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
INSERT [dbo].[OrderTypes_Label] ([TypesID], [LabelFrom], [LabelTo], [Label_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (-1, N'رصيد أول المدة', N'رصيد أول المدة', N'رصيد أول المدة', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
INSERT [dbo].[OrderTypes_Label] ([TypesID], [LabelFrom], [LabelTo], [Label_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (0, N'من قسم', N'إلى قسم', NULL, NULL, 0, 0, NULL, NULL, NULL, 0, -1)
INSERT [dbo].[OrderTypes_Label] ([TypesID], [LabelFrom], [LabelTo], [Label_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (1, N'من مورد', N'إلى مورد', NULL, NULL, 0, 0, NULL, NULL, NULL, 0, -1)
INSERT [dbo].[OrderTypes_Label] ([TypesID], [LabelFrom], [LabelTo], [Label_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (2, N'من عميل', N'إلى عميل', NULL, NULL, 0, 0, NULL, NULL, NULL, 0, -1)
INSERT [dbo].[OrderTypes_Label] ([TypesID], [LabelFrom], [LabelTo], [Label_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (3, N'من', N'إلى', N'الهالك', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
INSERT [dbo].[OrderTypes_Label] ([TypesID], [LabelFrom], [LabelTo], [Label_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (4, N'من مخزن', N'إلى مخزن', NULL, NULL, 0, 0, NULL, NULL, NULL, 0, -1)
INSERT [dbo].[OrderTypes_Label] ([TypesID], [LabelFrom], [LabelTo], [Label_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (7, N'من', N'إلى', N'تسوية بالزيادة', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
INSERT [dbo].[OrderTypes_Label] ([TypesID], [LabelFrom], [LabelTo], [Label_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (8, N'من', N'إلى', N'تسوية بالنقص', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
INSERT [dbo].[OrderTypes_Label] ([TypesID], [LabelFrom], [LabelTo], [Label_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (9, N'من', N'إلى', N'إستلام من الإنتاج', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
INSERT [dbo].[OrderTypes_Label] ([TypesID], [LabelFrom], [LabelTo], [Label_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (10, N'من', N'إلى', N'الإنتاج / التشغيل', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
INSERT [dbo].[OrderTypes_Label] ([TypesID], [LabelFrom], [LabelTo], [Label_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (12, N'من', N'إلى', N'صرف قطع غيار', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
INSERT [dbo].[OrderTypes_Label] ([TypesID], [LabelFrom], [LabelTo], [Label_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (18, N'من', N'إلى', N'صرف للورشة', CAST(N'2024-03-16 12:08:25.650' AS DateTime), 0, 0, CAST(N'2024-03-16 12:08:25.650' AS DateTime), CAST(N'2024-03-16 12:08:25.650' AS DateTime), CAST(N'2024-03-16 12:08:25.650' AS DateTime), 0, 0)

";
                opp.insert_me(str);

            }
            catch
            {
            }

            try
            {
                string str = @$" drop TABLE [dbo].[LookUp_StoreOrderTypes] ";
                opp.insert_me(str);

            }
            catch
            {
            }

            try
            {
                string str = @$" 


CREATE TABLE [dbo].[LookUp_StoreOrderTypes](
	[Id] [bigint] NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Type] [int] NULL,
	[IsHaveSelectedId] [int] NULL,
	[TableName] [nvarchar](50) NULL,
	[IsStoreMainType] [int] NULL
)  

 
INSERT [dbo].[LookUp_StoreOrderTypes] ([Id], [Name], [Type], [IsHaveSelectedId], [TableName], [IsStoreMainType]) VALUES (4, N'مخزن', 4, 1, N'Store', 1)
INSERT [dbo].[LookUp_StoreOrderTypes] ([Id], [Name], [Type], [IsHaveSelectedId], [TableName], [IsStoreMainType]) VALUES (20, N'قسم', 20, 1, N'Store', 1)
INSERT [dbo].[LookUp_StoreOrderTypes] ([Id], [Name], [Type], [IsHaveSelectedId], [TableName], [IsStoreMainType]) VALUES (2, N'عميل', 2, 1, N'Customer', 0)
INSERT [dbo].[LookUp_StoreOrderTypes] ([Id], [Name], [Type], [IsHaveSelectedId], [TableName], [IsStoreMainType]) VALUES (1, N'مورد', 1, 1, N'Supplier', 0)
INSERT [dbo].[LookUp_StoreOrderTypes] ([Id], [Name], [Type], [IsHaveSelectedId], [TableName], [IsStoreMainType]) VALUES (7, N'تسوية بالزيادة', 7, 0, NULL, 0)
INSERT [dbo].[LookUp_StoreOrderTypes] ([Id], [Name], [Type], [IsHaveSelectedId], [TableName], [IsStoreMainType]) VALUES (8, N'تسوية بالنقص', 8, 0, NULL, 0)
INSERT [dbo].[LookUp_StoreOrderTypes] ([Id], [Name], [Type], [IsHaveSelectedId], [TableName], [IsStoreMainType]) VALUES (9, N'إستلام إنتاج', 9, 0, NULL, 0)
INSERT [dbo].[LookUp_StoreOrderTypes] ([Id], [Name], [Type], [IsHaveSelectedId], [TableName], [IsStoreMainType]) VALUES (10, N'صرف للتشغيل / للإنتاج', 10, 0, NULL, 0)
INSERT [dbo].[LookUp_StoreOrderTypes] ([Id], [Name], [Type], [IsHaveSelectedId], [TableName], [IsStoreMainType]) VALUES (19, N'إستلام من الورشة', 19, 0, NULL, 0)
INSERT [dbo].[LookUp_StoreOrderTypes] ([Id], [Name], [Type], [IsHaveSelectedId], [TableName], [IsStoreMainType]) VALUES (18, N'صرف للورشة', 18, 0, NULL, 0)
INSERT [dbo].[LookUp_StoreOrderTypes] ([Id], [Name], [Type], [IsHaveSelectedId], [TableName], [IsStoreMainType]) VALUES (12, N'صرف قطع غيار', 12, 0, NULL, 0)
INSERT [dbo].[LookUp_StoreOrderTypes] ([Id], [Name], [Type], [IsHaveSelectedId], [TableName], [IsStoreMainType]) VALUES (3, N'هالك', 3, 0, NULL, 0)
INSERT [dbo].[LookUp_StoreOrderTypes] ([Id], [Name], [Type], [IsHaveSelectedId], [TableName], [IsStoreMainType]) VALUES (21, N'صالة انتاج', 21, 1, N'Store', 1)
INSERT [dbo].[LookUp_StoreOrderTypes] ([Id], [Name], [Type], [IsHaveSelectedId], [TableName], [IsStoreMainType]) VALUES (22, N'الورشة', 22, 1, N'Store', 1)


";
                opp.insert_me(str);
            }
            catch
            {
            }

            try
            {
                string str = @$"  drop TABLE [dbo].[DataDictionaryProcessesLists] ";
                opp.insert_me(str);

            }
            catch
            {
            }
            //////////// DataDictionaryProcessesLists
            try
            {

                string str = @$"  

CREATE TABLE [dbo].[DataDictionaryProcessesLists](
	[Id] [bigint] NOT NULL primary key,
	[ProcessName] [nvarchar](500) NULL,
	[OrderTypes_Value] [bigint] NULL,
	[BtnRecive_Text] [nvarchar](50) NULL,
	[Order_Estlam_Sarf] [int] NULL,
	[Case_Name] [nvarchar](50) NULL
)  

 
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (-9, N'شيك مرتجع', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (-8, N'تعديل رصيد أول المدة', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (-7, N'رصيد أول المدة', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (-1, N'فاتورة تصنيع للغير', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (0, N'رصيد سابق', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1, N'فاتورة مبيعات', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (2, N'فاتورة مشتريات', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (3, N'إستلام نقدية', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (4, N'صرف نقدية', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (5, N'إستلام شيك', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (6, N'صرف شيك', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (7, N'مرتجع مبيعات', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (8, N'مرتجع مشتريات', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (13, N'تعديل وتسوية الرصيد', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (20, N'تسوية ماليه', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (21, N'حذف إستلام النقدية', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (22, N'حذف صرف النقدية', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (25, N'تسوية مع الحساب المختلط', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (26, N'تسوية  بالصنف', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (27, N'تعديل إستلام نقدية', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (28, N'تعديل صرف نقدية', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (29, N'تسوية  بالصنف', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (31, N'إستلام نقدية محذوف', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (32, N'صرف نقدية محذوف', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (35, N'تعديل إستلام شيك', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (36, N'تعديل صرف شيك', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (37, N'إستلام شيك محذوف', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (38, N'صرف شيك محذوف', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (40, N'حجز تذاكر طيران', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (41, N'مرتجع تذاكر طيران', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (42, N'حجز تذاكر البرى', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (43, N'مرتجع تذاكر البرى', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (44, N'تأشيرات السياحة', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (45, N'مرتجع تأشيرات السياحة', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (46, N'برنامج عمرة', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (47, N'مرتجع برنامج عمرة', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (48, N'برنامج حج', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (49, N'مرتجع برنامج حج', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (50, N'حجز فندق', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (51, N'حجز باص', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (52, N'مرتجع حجز باص', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (53, N'مبيعات دشت', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (100, N'قيد محاسبى', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1001, N'إذن صرف / تحويل لمخزن أو قسم', 1000, N'صرف', 2, N'صرف')
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1006, N'إذن مرتجع صرف / تحويل  لمخزن أو قسم', 1000, N'صرف', 2, N'صرف')
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1002, N'إذن صرف لعميل', 1000, N'صرف', 2, N'صرف')
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1003, N'إذن مرتجع صرف من عميل', 1000, N'إستلام', 1, N'مرتجع صرف')
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1004, N'إذن إستلام من مورد', 1000, N'إستلام', 1, N'إستلام')
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1005, N'إذن مرتجع  لمورد', 1000, N'صرف', 2, N'مرتجع إستلام')
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1007, N'إذن تسوية بالزيادة', 1000, N'إستلام', 1, N'إستلام')
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1008, N'إذن تسوية بالنقص', 1000, N'صرف', 2, N'صرف')
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1009, N'إذن إستلام من الإنتاج', 1000, N'إستلام', 1, N'إستلام')
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1010, N'إذن صرف إلى الإنتاج / التشغيل', 1000, N'صرف', 2, N'صرف')
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1000, N'رصيد أول المدة', 1000, N'إستلام', -1, N'إستلام')
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1012, N'إذن صرف قطع غيار', 1000, N'صرف', 2, N'صرف')
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1013, N'صرف للهالك', 1000, N'صرف', 2, N'صرف')
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1014, N'إذن إستلام من مخزن خارجى', 1000, N'إستلام', 1, N'إستلام')
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1015, N'إذن صرف إلى مخزن خارجى', 1000, N'صرف', 2, N'صرف')
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1016, N'إذن إستلام من المخزن', 1000, N'إستلام', 1, N'إستلام')
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1017, N'إذن صرف إلى  المخزن', 1000, N'صرف', 2, N'صرف')
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1018, N'إذن صرف للورشة', 1000, N'صرف', 2, N'صرف')
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1019, N'إذن استلام / مرتجع صرف من الورشة', 1000, N'إستلام', 1, N'إستلام')
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1020, N'إذن صرف لموظف', 1000, N'صرف', 2, N'صرف')
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1021, N'إذن إستلام من موظف', 1000, N'إستلام', 1, N'إستلام')
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1022, N'إذن صرف قطع الغيار للماكينات وخطوط الإنتاج', 1000, N'صرف', 2, N'صرف')
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (1023, N'إذن مرتجع صرف قطع الغيار للماكينات وخطوط الإنتاج', 1000, N'إستلام', 1, N'إستلام')


INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (9, N'تعديل فاتورة مبيعات', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (10, N'تعديل فاتورة  مرتجع مبيعات', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (11, N'تعديل فاتورة مشتريات', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (12, N'تعديل فاتورة  مرتجع مشتريات', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (14, N'اصدار تقرير فحص سيارة', NULL, NULL, NULL, NULL)
INSERT [dbo].[DataDictionaryProcessesLists] ([Id], [ProcessName], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name]) VALUES (15, N'تعديل تقرير فحص سيارة', NULL, NULL, NULL, NULL)
";
                opp.insert_me(str);


            }
            catch
            {
            }
            /////////DataDictionaryLogerProcesses
            try
            {

                string str = @"";
                str = @$"  
 CREATE TABLE [dbo].[DataDictionaryLogerProcesses](
	[Id] [bigint] {IDENTITY} primary key NOT NULL,
	[ProcessId] [bigint] NULL,
	[ProcessName] [nvarchar](500) NULL,
	[SubProcessId] [bigint] NULL,
	[SubProcessName] [nvarchar](500) NULL,
	[ProcessDocNo] [bigint] NULL,
	[ProcessTableId] [bigint] NULL,
	[User_Id] [bigint] NULL,
	[UserName] [nvarchar](500) NULL,
	[Branch_Id] [bigint] NULL,
	[ProcessDateTime] [datetime] NULL,
	[ProcessNotes] [nvarchar](500) NULL,
	[Item_Name] [nvarchar](500) NULL,
	[Qnty] [nvarchar](500) NULL,
	[Item_Id] [bigint] NULL,
	[Unit_Item_Id] [bigint] NULL
  )   ";
                opp.insert_me(str);
            }
            catch
            {
            }

            //Account_Type
            try
            {

                string str = @"";
                str = @$" drop  TABLE [dbo].[Account_Type] 
CREATE TABLE [dbo].[Account_Type](
    [ID][bigint] NOT NULL,

   [Name] [nvarchar](50) NULL,
	[Value] [int] NULL,
	[Account_Follows] [int] NULL,
	[Head_Main_Account] [int] NULL,
	[Active] [int] NULL,
	[Main_Account_Id] [int] NULL,
	[Cost_Center_Tree_ID] [int] NULL,
	[Customer_Type] [int] NULL DEFAULT((0)),
	[Table_Name] [nvarchar](50) NULL,
	[OperationDate] [datetime] NULL DEFAULT(getdate()),
	[User_Id] [bigint] NULL DEFAULT((0)),
	[Branch_Id] [bigint] NULL DEFAULT((0)),
[ProcessNo] [bigint] NULL DEFAULT((0)),
	[Insert_ServerDateTime] [datetime] NULL DEFAULT(getdate()),
	[Update_ServerDateTime] [datetime] NULL DEFAULT(getdate()),
	[EditOperationDate] [datetime] NULL DEFAULT(getdate()),
	[Edit_User_Id] [bigint] NULL DEFAULT((0))
)  ";
                opp.insert_me(str);
            }
            catch
            {
            }

            ///Account_Type
            try
            {
                string str = @$"   

INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(1 AS Numeric(18, 0)), N'حقوق الملكيه', 15, 4, 3, 1, 0, 0, 0, NULL, NULL, 0, 0, NULL, NULL, NULL, 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(2 AS Numeric(18, 0)), N'حساب  موظف', 4, 4, 1, 1, 0, 0, 0, N'Employee', NULL, 0, 0, NULL, NULL, NULL, 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(3 AS Numeric(18, 0)), N'حساب  العملاء ', 2, 4, 1, 1, 0, 0, 2, N'Customer', NULL, 0, 0, NULL, NULL, NULL, 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(4 AS Numeric(18, 0)), N'حساب الفرع', 2, 4, 1, 0, 0, 0, 1, N'Customer', NULL, 0, 0, NULL, NULL, NULL, 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(5 AS Numeric(18, 0)), N'شركه السياحه', 2, 4, 1, 0, 0, 0, 3, N'Customer', NULL, 0, 0, NULL, NULL, NULL, 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(6 AS Numeric(18, 0)), N'حساب موردين', 5, 4, 2, 1, 0, 0, 0, N'Supplier', NULL, 0, 0, NULL, NULL, NULL, 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(7 AS Numeric(18, 0)), N'حسابات الصندوق ', 6, 4, 1, 1, 0, 0, 0, N'Banks', NULL, 0, 0, NULL, NULL, NULL, 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(8 AS Numeric(18, 0)), N'حساب البنك', 7, 4, 1, 1, 0, 0, 0, N'Banks', NULL, 0, 0, NULL, NULL, NULL, 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(9 AS Numeric(18, 0)), N'الأصول الثابته', 8, 4, 1, 1, 0, 0, 0, NULL, NULL, 0, 0, NULL, NULL, NULL, 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(10 AS Numeric(18, 0)), N'حسابات أخرى', 10, 4, 1, 1, 0, 0, 0, NULL, NULL, 0, 0, NULL, NULL, NULL, 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(11 AS Numeric(18, 0)), N'حساب مبيعات', 11, 3, 5, 1, 0, 0, 0, NULL, NULL, 0, 0, NULL, NULL, NULL, 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(12 AS Numeric(18, 0)), N'حساب مرتجع مبيعات', 12, 3, 5, 1, 0, 0, 0, NULL, NULL, 0, 0, NULL, NULL, NULL, 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(13 AS Numeric(18, 0)), N'حساب مشتريات', 13, 3, 4, 1, 0, 0, 0, NULL, NULL, 0, 0, NULL, NULL, NULL, 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(14 AS Numeric(18, 0)), N'حساب مرتجع مشتريات', 14, 3, 4, 1, 0, NULL, 0, NULL, NULL, 0, 0, NULL, NULL, NULL, 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(15 AS Numeric(18, 0)), N'مصروفات', 16, 3, 4, 1, NULL, NULL, 0, NULL, NULL, 0, 0, NULL, NULL, NULL, 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(16 AS Numeric(18, 0)), N'إيرادات', 17, 3, 5, 1, NULL, NULL, 0, NULL, NULL, 0, 0, NULL, NULL, NULL, 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(17 AS Numeric(18, 0)), N'حساب ارباح تجارية وصناعية - مبيعات', 18, 3, 1, 1, NULL, NULL, 0, NULL, CAST(N'2022-09-16 00:02:15.727' AS DateTime), 0, 0, CAST(N'2022-09-16 00:02:15.727' AS DateTime), CAST(N'2022-09-16 00:02:15.727' AS DateTime), CAST(N'2022-09-16 00:02:15.727' AS DateTime), 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(18 AS Numeric(18, 0)), N'حساب ضرايب القيمة المضافة مبيعات', 19, 3, 1, 1, NULL, NULL, 0, NULL, CAST(N'2022-09-16 00:03:06.237' AS DateTime), 0, 0, CAST(N'2022-09-16 00:03:06.237' AS DateTime), CAST(N'2022-09-16 00:03:06.237' AS DateTime), CAST(N'2022-09-16 00:03:06.237' AS DateTime), 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(19 AS Numeric(18, 0)), N'حساب الدمغه', 20, 3, 4, 1, NULL, NULL, 0, NULL, CAST(N'2022-09-16 00:03:23.170' AS DateTime), 0, 0, CAST(N'2022-09-16 00:03:23.170' AS DateTime), CAST(N'2022-09-16 00:03:23.170' AS DateTime), CAST(N'2022-09-16 00:03:23.170' AS DateTime), 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(20 AS Numeric(18, 0)), N'حساب الخصم المسموح بة', 21, 3, 4, 1, NULL, NULL, 0, NULL, CAST(N'2022-09-16 00:03:36.703' AS DateTime), 0, 0, CAST(N'2022-09-16 00:03:36.703' AS DateTime), CAST(N'2022-09-16 00:03:36.703' AS DateTime), CAST(N'2022-09-16 00:03:36.703' AS DateTime), 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(21 AS Numeric(18, 0)), N'حساب مصروفات التحميل مشتريات', 22, 3, 4, 1, NULL, NULL, 0, NULL, CAST(N'2022-09-16 00:03:49.020' AS DateTime), 0, 0, CAST(N'2022-09-16 00:03:49.020' AS DateTime), CAST(N'2022-09-16 00:03:49.020' AS DateTime), CAST(N'2022-09-16 00:03:49.020' AS DateTime), 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(22 AS Numeric(18, 0)), N'حساب مصروفات أخرى مبيعات', 25, 3, 4, 1, NULL, NULL, 0, NULL, CAST(N'2022-09-16 00:04:01.737' AS DateTime), 0, 0, CAST(N'2022-09-16 00:04:01.737' AS DateTime), CAST(N'2022-09-16 00:04:01.737' AS DateTime), CAST(N'2022-09-16 00:04:01.737' AS DateTime), 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(23 AS Numeric(18, 0)), N'حساب التسوية المالية للعملاء والموردين', 26, 4, 1, 1, NULL, NULL, 0, NULL, CAST(N'2022-09-18 22:46:55.537' AS DateTime), 0, 0, CAST(N'2022-09-18 22:46:55.537' AS DateTime), CAST(N'2022-09-18 22:46:55.537' AS DateTime), CAST(N'2022-09-18 22:46:55.537' AS DateTime), 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(24 AS Numeric(18, 0)), N'خصم مكتسب', 27, 3, 5, 1, NULL, NULL, 0, NULL, CAST(N'2022-10-31 23:11:54.130' AS DateTime), 0, 0, CAST(N'2022-10-31 23:11:54.130' AS DateTime), CAST(N'2022-10-31 23:11:54.130' AS DateTime), CAST(N'2022-10-31 23:11:54.130' AS DateTime), 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(25 AS Numeric(18, 0)), N'حافظة الشيكات الواردة', 30, 4, 1, 1, NULL, NULL, 0, NULL, CAST(N'2023-01-14 03:48:36.890' AS DateTime), 0, 0, CAST(N'2023-01-14 03:48:36.890' AS DateTime), CAST(N'2023-01-14 03:48:36.890' AS DateTime), CAST(N'2023-01-14 03:48:36.890' AS DateTime), 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(26 AS Numeric(18, 0)), N'حافظة الشيكات الصادرة', 31, 4, 1, 1, NULL, NULL, 0, NULL, CAST(N'2023-01-14 03:49:05.100' AS DateTime), 0, 0, CAST(N'2023-01-14 03:49:05.100' AS DateTime), CAST(N'2023-01-14 03:49:05.100' AS DateTime), CAST(N'2023-01-14 03:49:05.100' AS DateTime), 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(27 AS Numeric(18, 0)), N' إيرادات أخرى', 32, 3, 5, 1, NULL, NULL, 0, NULL, CAST(N'2023-01-14 03:50:39.027' AS DateTime), 0, 0, CAST(N'2023-01-14 03:50:39.027' AS DateTime), CAST(N'2023-01-14 03:50:39.027' AS DateTime), CAST(N'2023-01-14 03:50:39.027' AS DateTime), 0, 0)

INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (CAST(28 AS Numeric(18, 0)), N'حساب التسوية المالية للعملاء  لخدمات المبانى', 28, 3, 5, 1, 0, NULL, 0, NULL, CAST(N'2022-11-09 19:33:14.453' AS DateTime), 0, 0, CAST(N'2022-11-09 19:33:14.453' AS DateTime), CAST(N'2022-11-09 19:33:14.453' AS DateTime), CAST(N'2022-11-09 19:33:14.453' AS DateTime), 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (CAST(29 AS Numeric(18, 0)), N'حساب فرق العملة', 29, 3, 5, 1, NULL, NULL, 0, NULL, CAST(N'2022-11-09 19:46:22.430' AS DateTime), 0, 0, CAST(N'2022-11-09 19:46:22.430' AS DateTime), CAST(N'2022-11-09 19:46:22.430' AS DateTime), CAST(N'2022-11-09 19:46:22.430' AS DateTime), 0)

INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(30 AS Numeric(18, 0)), N'حساب ارباح تجارية وصناعية - مشتريات', 33, 3, 1, 1, NULL, NULL, 0, NULL, CAST(N'2022-09-16 00:02:15.727' AS DateTime), 0, 0, CAST(N'2022-09-16 00:02:15.727' AS DateTime), CAST(N'2022-09-16 00:02:15.727' AS DateTime), CAST(N'2022-09-16 00:02:15.727' AS DateTime), 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(31 AS Numeric(18, 0)), N'حساب ضرايب القيمة المضافة مشتريات', 34, 3, 1, 1, NULL, NULL, 0, NULL, CAST(N'2022-09-16 00:03:06.237' AS DateTime), 0, 0, CAST(N'2022-09-16 00:03:06.237' AS DateTime), CAST(N'2022-09-16 00:03:06.237' AS DateTime), CAST(N'2022-09-16 00:03:06.237' AS DateTime), 0, 0)

INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(32 AS Numeric(18, 0)), N'حساب ايرادات تقارير الفحص', 54, 3, 5, 1, NULL, NULL, 0, NULL, CAST(N'2022-09-16 00:03:06.237' AS DateTime), 0, 0, CAST(N'2022-09-16 00:03:06.237' AS DateTime), CAST(N'2022-09-16 00:03:06.237' AS DateTime), CAST(N'2022-09-16 00:03:06.237' AS DateTime), 0, 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(33 AS Numeric(18, 0)), N'عمولة البيع - المشتريات وكالة الخضار', 55, 3, 5, 1, NULL, NULL, 0, NULL, CAST(N'2022-09-16 00:03:06.237' AS DateTime), 0, 0, CAST(N'2022-09-16 00:03:06.237' AS DateTime), CAST(N'2022-09-16 00:03:06.237' AS DateTime), CAST(N'2022-09-16 00:03:06.237' AS DateTime), 0, 0)

INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [ProcessNo], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (34, N'حساب مخزن انتاج تام ', 56, 4, 1, 1, NULL, NULL, 0, NULL, CAST(N'2026-07-26T22:04:55.760' AS DateTime), 0, 0, 0, CAST(N'2026-07-26T22:04:55.760' AS DateTime), CAST(N'2026-07-26T22:04:55.760' AS DateTime), CAST(N'2026-07-26T22:04:55.760' AS DateTime), 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [ProcessNo], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (35, N'حساب مخزن خامات', 57, 4, 1, 1, NULL, NULL, 0, NULL, CAST(N'2026-07-26T22:05:28.730' AS DateTime), 0, 0, 0, CAST(N'2026-07-26T22:05:28.730' AS DateTime), CAST(N'2026-07-26T22:05:28.730' AS DateTime), CAST(N'2026-07-26T22:05:28.730' AS DateTime), 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [ProcessNo], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (36, N'حساب مخزن إنتاج غير مكتمل', 58, 4, 1, 1, NULL, NULL, 0, NULL, CAST(N'2026-07-26T22:05:56.260' AS DateTime), 0, 0, 0, CAST(N'2026-07-26T22:05:56.260' AS DateTime), CAST(N'2026-07-26T22:05:56.260' AS DateTime), CAST(N'2026-07-26T22:05:56.260' AS DateTime), 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [ProcessNo], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (37, N'حساب الإنتاج تحت التشغيل (WIP)', 59, 4, 1, 1, NULL, NULL, 0, NULL, CAST(N'2026-07-26T22:05:59.733' AS DateTime), 0, 0, 0, CAST(N'2026-07-26T22:05:59.733' AS DateTime), CAST(N'2026-07-26T22:05:59.733' AS DateTime), CAST(N'2026-07-26T22:05:59.733' AS DateTime), 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [ProcessNo], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (38, N'حساب بضاعة مستلمة غير مفوترة (GRIR)', 60, 4, 2, 1, NULL, NULL, 0, NULL, CAST(N'2026-07-26T22:06:28.197' AS DateTime), 0, 0, 0, CAST(N'2026-07-26T22:06:28.197' AS DateTime), CAST(N'2026-07-26T22:06:28.197' AS DateTime), CAST(N'2026-07-26T22:06:28.197' AS DateTime), 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [ProcessNo], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (39, N'حساب تكلفة البضاعة المباعة (COGS)', 61, 3, 4, 1, NULL, NULL, 0, NULL, CAST(N'2026-07-26T22:06:50.650' AS DateTime), 0, 0, 0, CAST(N'2026-07-26T22:06:50.650' AS DateTime), CAST(N'2026-07-26T22:06:50.650' AS DateTime), CAST(N'2026-07-26T22:06:50.650' AS DateTime), 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [ProcessNo], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (40, N'حساب تسويات وتسفية العجز والزيادة بالمخزون', 62, 3, 4, 1, NULL, NULL, 0, NULL, CAST(N'2026-07-26T22:07:09.080' AS DateTime), 0, 0, 0, CAST(N'2026-07-26T22:07:09.080' AS DateTime), CAST(N'2026-07-26T22:07:09.080' AS DateTime), CAST(N'2026-07-26T22:07:09.080' AS DateTime), 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [ProcessNo], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (41, N'حساب فروق أسعار الشراء (PPV)', 63, 3, 4, 1, NULL, NULL, 0, NULL, CAST(N'2026-07-26T22:07:09.080' AS DateTime), 0, 0, 0, CAST(N'2026-07-26T22:07:09.080' AS DateTime), CAST(N'2026-07-26T22:07:09.080' AS DateTime), CAST(N'2026-07-26T22:07:09.080' AS DateTime), 0)
INSERT [dbo].[Account_Type] ([ID], [Name], [Value], [Account_Follows], [Head_Main_Account], [Active], [Main_Account_Id], [Cost_Center_Tree_ID], [Customer_Type], [Table_Name], [OperationDate], [User_Id], [Branch_Id], [ProcessNo], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (42, N'حساب الأرصدة الافتتاحية (التأسيس)', 64, 4, 3, 1, NULL, NULL, 0, NULL, CAST(N'2026-07-26T22:07:09.080' AS DateTime), 0, 0, 0, CAST(N'2026-07-26T22:07:09.080' AS DateTime), CAST(N'2026-07-26T22:07:09.080' AS DateTime), CAST(N'2026-07-26T22:07:09.080' AS DateTime), 0)

";
                opp.insert_me(str);

            }
            catch
            {
            }


            try
            {
                string str = @$" drop TABLE [dbo].[OrderTypes]";
                opp.insert_me(str);

            }
            catch
            {
            }

            /*
               //OrderTypes
                try
                {

                    string str = @$"  
    //drop TABLE [dbo].[OrderTypes]
    CREATE TABLE [dbo].[OrderTypes](
        [ID] [bigint] NOT NULL,
        [Order_Name] [nvarchar](100) NULL,
        [OrderTypes_Value] [int] NULL,
        [BtnRecive_Text] [nvarchar](50) NULL,
        [Order_Estlam_Sarf] [int] NULL,
        [Case_Name] [nvarchar](50) NULL,
        [OperationDate] [datetime] NULL,
        [User_Id] [bigint] NULL,
        [Branch_Id] [bigint] NULL,
        [Insert_ServerDateTime] [datetime] NULL,
        [Update_ServerDateTime] [datetime] NULL,
        [EditOperationDate] [datetime] NULL,
        [Edit_User_Id] [bigint] NULL,
        [ProcessNo] [bigint] NULL 

    ) 


    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (1, N'100إذن صرف / تحويل لمخزن أو قسم', 1, N'صرف', 2, N'صرف', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (2, N'إذن مرتجع صرف / تحويل  لمخزن أو قسم', 1006, N'صرف', 2, N'صرف', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (3, N'إذن صرف لعميل', 1002, N'صرف', 2, N'صرف', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (4, N'100إذن مرتجع صرف من عميل', 3, N'إستلام', 1, N'مرتجع صرف', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (5, N'إذن إستلام من مورد', 1004, N'إستلام', 1, N'إستلام', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (6, N'إذن مرتجع إستلام لمورد', 1005, N'صرف', 2, N'مرتجع إستلام', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (7, N'إذن تسوية بالزيادة', 1007, N'إستلام', 1, N'إستلام', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (8, N'إذن تسوية بالنقص', 1008, N'صرف', 2, N'صرف', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (9, N'100إذن إستلام من الإنتاج', 9, N'إستلام', 1, N'إستلام', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (10, N'إذن صرف إلى الإنتاج / التشغيل', 1010, N'صرف', 2, N'صرف', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (11, N'100رصيد أول المدة', -1, N'إستلام', -1, N'إستلام', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (12, N'إذن صرف قطع غيار', 1012, N'صرف', 2, N'صرف', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (13, N'10صرف للهالك', 13, N'صرف', 2, N'صرف', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (14, N'إذن إستلام من مخزن خارجى', 1014, N'إستلام', 1, N'إستلام', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (15, N'10إذن صرف إلى مخزن خارجى', 15, N'صرف', 2, N'صرف', NULL, 0, 0, NULL, NULL, NULL, 0, -1)
    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (16, N'إذن إستلام من المخزن', 1016, N'إستلام', 1, N'إستلام', CAST(N'2024-03-04 19:50:04.230' AS DateTime), 0, 0, CAST(N'2024-03-04 19:50:04.230' AS DateTime), CAST(N'2024-03-04 19:50:04.230' AS DateTime), CAST(N'2024-03-04 19:50:04.230' AS DateTime), 0, 0)
    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (17, N'إذن صرف إلى  المخزن', 1017, N'صرف', 2, N'صرف', CAST(N'2024-03-04 19:50:14.427' AS DateTime), 0, 0, CAST(N'2024-03-04 19:50:14.427' AS DateTime), CAST(N'2024-03-04 19:50:14.427' AS DateTime), CAST(N'2024-03-04 19:50:14.427' AS DateTime), 0, 0)
    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (18, N'10إذن صرف للورشة', 18, N'صرف', 2, N'صرف', CAST(N'2024-03-04 19:50:14.427' AS DateTime), 0, 0, CAST(N'2024-03-04 19:50:14.427' AS DateTime), CAST(N'2024-03-04 19:50:14.427' AS DateTime), CAST(N'2024-03-04 19:50:14.427' AS DateTime), 0, 0)
    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (19, N'10إذن استلام / مرتجع صرف من الورشة', 19, N'إستلام', 1, N'إستلام', CAST(N'2024-03-04 19:50:04.230' AS DateTime), 0, 0, CAST(N'2024-03-04 19:50:04.230' AS DateTime), CAST(N'2024-03-04 19:50:04.230' AS DateTime), CAST(N'2024-03-04 19:50:04.230' AS DateTime), 0, 0)
    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (20, N'10إذن صرف لموظف', 20, N'صرف', 2, N'صرف', CAST(N'2024-11-14 16:21:47.490' AS DateTime), 0, 0, CAST(N'2024-11-14 16:21:47.490' AS DateTime), CAST(N'2024-11-14 16:21:47.490' AS DateTime), CAST(N'2024-11-14 16:21:47.490' AS DateTime), 0, 0)
    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (21, N'10إذن إستلام من موظف', 21, N'إستلام', 1, N'إستلام', CAST(N'2024-11-14 16:22:31.667' AS DateTime), 0, 0, CAST(N'2024-11-14 16:22:31.667' AS DateTime), CAST(N'2024-11-14 16:22:31.667' AS DateTime), CAST(N'2024-11-14 16:22:31.667' AS DateTime), 0, 0)
    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (22, N'إذن صرف قطع الغيار للماكينات وخطوط الإنتاج', 1022, N'صرف', 2, N'صرف', CAST(N'2024-11-14 16:23:24.270' AS DateTime), 0, 0, CAST(N'2024-11-14 16:23:24.270' AS DateTime), CAST(N'2024-11-14 16:23:24.270' AS DateTime), CAST(N'2024-11-14 16:23:24.270' AS DateTime), 0, 0)
    INSERT [dbo].[OrderTypes] ([ID], [Order_Name], [OrderTypes_Value], [BtnRecive_Text], [Order_Estlam_Sarf], [Case_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (23, N'10إذن مرتجع صرف قطع الغيار للماكينات وخطوط الإنتاج', 23, N'إستلام', 1, N'إستلام', CAST(N'2024-11-14 16:24:25.210' AS DateTime), 0, 0, CAST(N'2024-11-14 16:24:25.210' AS DateTime), CAST(N'2024-11-14 16:24:25.210' AS DateTime), CAST(N'2024-11-14 16:24:25.210' AS DateTime), 0, 0)

    ";
                    opp.insert_me(str);


                }
                catch
                {
                }
             */



            try
            {
                string str = @$"  

CREATE TABLE [dbo].[CarEstimationsValuesDetails](
	[Id] [bigint]  {IDENTITY} primary key  NOT NULL,
	[CarEstimationsValuesListId] [bigint] NULL,
	[Item_Id] [bigint] NULL,
	[Unit_ItemsId] [bigint] NULL,
	[ItemCode] [nvarchar](100) NULL,
	[Item_Category_ID] [bigint] NULL,
	[Qnty] [decimal](18, 2) NULL,
	[Item_Name] [nvarchar](500) NULL,
	[Invoice_Item_Name] [nvarchar](100) NULL,
	[Specification] [nvarchar](500) NULL,
	[Larg_Qnty] [decimal](18, 2) NULL,
	[Small_Qnty] [decimal](18, 2) NULL,
	[Item_No] [decimal](18, 2) NULL,
	[PriceUnit] [decimal](18, 2) NULL,
	[Larg_Unit_Price] [decimal](18, 2) NULL,
	[Small_Unit_Price] [decimal](18, 2) NULL,
	[Unit_Used_for_Price] [int] NULL,
	[S_price_LargeUnit] [decimal](18, 2) NULL,
	[S_price_SmallUnit] [decimal](18, 2) NULL,
	[Qnty_value] [decimal](18, 2) NULL,
	[Item_Total_WithoutTax] [decimal](18, 2) NULL,
	[Item_ATS05] [decimal](18, 2) NULL,
	[Item_ATS05_Value] [decimal](18, 2) NULL,
	[Item_TaxSales10] [decimal](18, 2) NULL,
	[Item_TaxSales10_Value] [decimal](18, 2) NULL,
	[Total] [decimal](18, 2) NULL,
	[Is_Item_CommissionValue] [int] NULL,
	[Item_CommissionValue] [decimal](18, 2) NULL,
	[Unit_CommissionValue] [decimal](18, 2) NULL,
	[Item_CommissionRate] [decimal](18, 2) NULL,
	[Unit_CommissionRate] [decimal](18, 2) NULL,
	[Net_Total_Item_Commission] [decimal](18, 2) NULL,
	[ItemCommissionTotal] [decimal](18, 2) NULL,
	[Item_Net_Total_Price] [decimal](18, 2) NULL,
	[QntyStr] [nvarchar](500) NULL,
	[QntyStrColumn] [nvarchar](500) NULL,
	[Item_Notes] [nvarchar](500) NULL,
	[Order_PurshingId] [bigint] NULL,
	[StoreId] [bigint] NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[Update_ServerDateTime] [datetime] NULL,
	[EditOperationDate] [datetime] NULL,
	[Edit_User_Id] [bigint] NULL,
	[ProcessNo] [bigint] NULL,
	[OperationDate] [datetime] NULL,
	[Store_Id] [bigint] NULL,
	[Unit_Id] [bigint] NULL,
	[UnitStr] [nvarchar](500) NULL,
	[Is_NotShowOnInvoice] [int] NULL,
	[Is_SarfFromStore] [int] NULL,
	[Is_AddToInvoice] [int] NULL,
	[Item_DeletedFlage] [int] NULL,
	[Item_ProcessedFlage] [int] NULL,
	[Item_Category_Name] [nvarchar](500) NULL
)  

 

CREATE TABLE [dbo].[CarEstimationsValuesLists](
	[Id] [bigint]  {IDENTITY} primary key  NOT NULL,
	[StoreId] [bigint] NULL,
	[CarsId] [bigint] NULL,
	[CustomerId] [bigint] NULL,
	[EstimationsNumber] [bigint] NULL,
	[EstimationsDate] [datetime] NULL,
	[Notes] [nvarchar](max) NULL,
	[Total_ListPriceWithoutTax_Workmanship] [decimal](18, 2) NULL,
	[Total_ListPriceWithoutTax_SpareParts] [decimal](18, 2) NULL,
	[Total_ListPriceWithoutTax_Oils] [decimal](18, 2) NULL,
	[Total_ListPriceWithoutTax] [decimal](18, 2) NULL,
	[NetInvoiceTotal_Workmanship] [decimal](18, 2) NULL,
	[NetInvoiceTotal_SpareParts] [decimal](18, 2) NULL,
	[NetInvoiceTotal_Oils] [decimal](18, 2) NULL,
	[NetInvoiceTotal] [decimal](18, 2) NULL,
	[TaxSalesListWorkmanship_Rate] [decimal](18, 2) NULL,
	[TaxSalesListSpareParts_Rate] [decimal](18, 2) NULL,
	[TaxSalesListOils_Rate] [decimal](18, 2) NULL,
	[TaxSalesListWorkmanship_Value] [decimal](18, 2) NULL,
	[TaxSalesListSpareParts_Value] [decimal](18, 2) NULL,
	[TaxSalesListOils_Value] [decimal](18, 2) NULL,
	[TotalListTaxSales10_Value] [decimal](18, 2) NULL,
	[TafkeetNet_Total] [nvarchar](max) NULL,
	[ReceptionEngineerName] [nvarchar](100) NULL,
	[ReceptionEngineerId] [bigint] NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Qnty_ValueSpareParts] [decimal](18, 2) NULL,
	[Item_Net_TotalSpareParts] [decimal](18, 2) NULL,
	[Qnty_ValueWorkmanship] [decimal](18, 2) NULL,
	[Item_Net_TotalOils] [decimal](18, 2) NULL,
	[Item_Net_TotalWorkmanship] [decimal](18, 2) NULL,
	[Total_Item_CommissionValue] [decimal](18, 2) NULL,
	[Net_Total] [decimal](18, 2) NULL,
	[Qnty_ValueOils] [decimal](18, 2) NULL,
	[Total_ItemPriceWithoutTax] [decimal](18, 2) NULL,
	[Currency_id] [int] NULL,
	[Equivalent_Net_Total_Price] [decimal](18, 2) NULL,
	[TaxSales10_Value] [decimal](18, 2) NULL,
	[Total_PriceWithoutTax] [decimal](18, 2) NULL,
	[Total] [decimal](18, 2) NULL,
	[ATS05ListWorkmanship_Rate] [decimal](18, 2) NULL,
	[ATS05ListSpareParts_Rate] [decimal](18, 2) NULL,
	[ATS05ListOils_Rate] [decimal](18, 2) NULL,
	[ATS05ListWorkmanship_Value] [decimal](18, 2) NULL,
	[ATS05ListSpareParts_Value] [decimal](18, 2) NULL,
	[ATS05ListOils_Value] [decimal](18, 2) NULL,
	[TotalListATS05_Value] [decimal](18, 2) NULL,
	[CommissionListOils_Rate] [decimal](18, 2) NULL,
	[CommissionListWorkmanship_Value] [decimal](18, 2) NULL,
	[CommissionListSpareParts_Value] [decimal](18, 2) NULL,
	[CommissionListOils_Value] [decimal](18, 2) NULL,
	[Total_CommissionListWorkmanship] [decimal](18, 2) NULL,
	[Total_CommissionListSpareParts] [decimal](18, 2) NULL,
	[Total_CommissionListOils] [decimal](18, 2) NULL,
	[Total_CommissionList] [decimal](18, 2) NULL,
	[Net_Total_CommissionValue] [decimal](18, 2) NULL,
	[List_Final_CommissionValue] [decimal](18, 2) NULL,
	[ATS05_Value] [decimal](18, 2) NULL,
	[List_CommissionRate] [decimal](18, 2) NULL,
	[List_CommissionValue] [decimal](18, 2) NULL,
	[Equivalent_Net_Total_CommissionValue] [decimal](18, 2) NULL,
	[CommissionListWorkmanship_Rate] [decimal](18, 2) NULL,
	[CommissionListSpareParts_Rate] [decimal](18, 2) NULL
)  
CREATE TABLE [dbo].[Cars](
	[Id] [bigint]  {IDENTITY} primary key  NOT NULL,
	[CustomerId] [bigint] NULL,
	[CustomerId_] [bigint] NULL,
	[Cars_ID] [bigint] NULL,
	[CustomerIdOld] [bigint] NULL,
	[Acc_No_Cust] [nvarchar](50) NULL,
	[CarNumber] [nvarchar](500) NULL,
	[CarColor] [nvarchar](500) NULL,
	[ColorCode] [nvarchar](500) NULL,
	[CarType] [nvarchar](500) NULL,
	[Model] [nvarchar](500) NULL,
	[Lookup_CarTypeId] [bigint] NULL,
	[Lookup_CarTypeModeLId] [bigint] NULL,
	[EngineNumber] [nvarchar](500) NULL,
	[ChassisNumber] [nvarchar](500) NULL,
	[FuelType] [nvarchar](500) NULL,
	[Capacity] [nvarchar](500) NULL,
	[Year] [nvarchar](500) NULL,
	[TransmissionType] [nvarchar](500) NULL,
	[Lookup_TransmissionTypeId] [bigint] NULL,
	[Lookup_CarShapeId] [bigint] NULL,
	[Lookup_CarShape] [nvarchar](500) NULL,
	[Notes] [nvarchar](500) NULL,
	[WarrantyStartDate] [datetime] NULL,
	[WarrantyEndDate] [datetime] NULL,
	[InsuranceCompany] [nvarchar](500) NULL,
	[Key_Code] [nvarchar](500) NULL,
	[Car_Purch_Date] [datetime] NULL,
	[Car_Purch_KM] [int] NULL,
	[CarAddress] [nvarchar](500) NULL,
	[DriverTel] [nvarchar](500) NULL,
	[DriverName] [nvarchar](500) NULL,
	[CustomerName] [nvarchar](500) NULL,
	[Tax_No] [nvarchar](500) NULL,
	[Active] [int] NULL,
	[Repeat] [int] NULL,
	[RepeatN] [int] NULL,
	[IsChangeOwner] [int] NULL,
	[OldId] [bigint] NULL,
	[NewId] [bigint] NULL,
	[customeridNew] [int] NULL
)  

 

CREATE TABLE [dbo].[CarsExaminations](
	[Id] [bigint]  {IDENTITY} primary key  NOT NULL,
	[Paint_Report] [nvarchar](max) NULL,
	[Mechanics_Report] [nvarchar](max) NULL,
	[Suspension_Report] [nvarchar](max) NULL,
	[ExaminationNotes] [nvarchar](max) NULL,
	[CarNumber] [nvarchar](max) NULL,
	[ChassisNumber] [nvarchar](max) NULL,
	[CustomerId] [bigint] NULL,
	[CustomerName] [nvarchar](max) NULL,
	[CarsId] [bigint] NULL,
	[ExaminationDate] [datetime] NULL,
	[ExaminationNumber] [bigint] NULL,
	[CarMeterOnEntry] [nvarchar](500) NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[IsCarsExaminationClosed] [int] NULL,
	[CarsExaminationClosedDate] [datetime] NULL,
	[CarsExaminationStatus] [nvarchar](500) NULL,
	[Order_MoneyId] [bigint] NULL,
	[Transaction_Type] [int] NULL,
	[IS_Posted_Flage] [int] NULL,
	[Account_Entry_List_ID] [bigint] NULL,
	[SubAccount_Id] [bigint] NULL,
	[Currency_Id] [bigint] NULL,
	[CarsExaminationValue] [decimal](18, 2) NULL,
	[Notes] [nvarchar](max) NULL,
	[ProcessNo] [bigint] NULL
)     

 

CREATE TABLE [dbo].[CarsTemp](
	[Id] [bigint]  {IDENTITY} primary key  NOT NULL,
	[CarNumber] [nvarchar](50) NULL,
	[Count] [int] NULL
)  

 

CREATE TABLE [dbo].[CarsWorkOrders](
	[Id] [bigint]  {IDENTITY} primary key  NOT NULL,
	[StoreId] [bigint] NULL,
	[CarsId] [bigint] NULL,
	[CustomerId] [bigint] NULL,
	[WorkOrderNumber] [bigint] NULL,
	[CustomerSalesMan] [nvarchar](200) NULL,
	[Tel] [nvarchar](200) NULL,
	[Mobile] [nvarchar](200) NULL,
	[Address] [nvarchar](200) NULL,
	[CarMeterOnEntry] [nvarchar](200) NULL,
	[WorkOrdersDate] [datetime] NULL,
	[EntryDateTime] [datetime] NULL,
	[EntryDateOnly] [date] NULL,
	[EntryTimeOnly] [time](7) NULL,
	[StartWorkDateTime] [datetime] NULL,
	[ExpectedEndWorkDate] [datetime] NULL,
	[CustomerComplaint] [nvarchar](max) NULL,
	[RequiredWorks] [nvarchar](max) NULL,
	[ExaminationReport] [nvarchar](max) NULL,
	[ReceptionEngineerId] [bigint] NULL,
	[ReceptionEngineerName] [nvarchar](200) NULL,
	[ReceptionManName] [nvarchar](200) NULL,
	[IsWorkOrderClosed] [int] NULL,
	[WorkOrderClosedDate] [datetime] NULL,
	[ReceptionManId] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[User_Id] [bigint] NULL,
	[Lookup_CarTypeModeLId] [bigint] NULL,
	[Lookup_CarTypeId] [bigint] NULL,
	[Last_Invoice_ListId] [bigint] NULL,
	[Lookup_CarEntryTaskeenId] [bigint] NULL,
	[Lookup_CarEntryTaskeenName] [nvarchar](100) NULL,
	[LookUp_CarStatusLocationId] [bigint] NULL,
	[LookUp_CarStatusLocationName] [nvarchar](100) NULL,
	[WorkOrderClosed_Reason] [nvarchar](max) NULL,
	[CustomerOrderNumberPO] [nvarchar](max) NULL,
	[TaxInvoiceNo] [bigint] NULL,
	[TaxInternalId] [nvarchar](500) NULL,
	[IsUploadToPortal] [int] NULL,
	[KhamatQnty_Value] [decimal](18, 2) NULL,
	[TaxReverseInternalId] [nvarchar](500) NULL,
	[TaxReverseInvoiceNo] [bigint] NULL,
	[Last_Invoices_List_ReverseId] [bigint] NULL,
	[CustomerId_] [bigint] NULL,
	[CustomerId_CarOwner] [bigint] NULL,
	[IsInsuranceCompanyStaus] [int] NULL,
	[CustomerOrderNumberGR] [nvarchar](max) NULL,
	[Customer_Id_RelatedToAccount] [bigint] NULL,
	[Repairs_NotDone] [nvarchar](max) NULL,
	[IsInvoiceTaxInfo] [int] NULL 
)    

 

CREATE TABLE [dbo].[CarsWorkOrdersDeliverDetails](
	[Id] [bigint]  {IDENTITY} primary key  NOT NULL,
	[CarsWorkOrdersId] [bigint] NULL,
	[Item_Id] [bigint] NULL,
	[Unit_ItemsId] [bigint] NULL,
	[ItemCode] [nvarchar](100) NULL,
	[Qnty] [decimal](18, 2) NULL,
	[Item_Name] [nvarchar](500) NULL,
	[Invoice_Item_Name] [nvarchar](500) NULL,
	[Specification] [nvarchar](500) NULL,
	[Larg_Qnty] [decimal](18, 2) NULL,
	[Small_Qnty] [decimal](18, 2) NULL,
	[Item_No] [decimal](18, 2) NULL,
	[PriceUnit] [decimal](18, 2) NULL,
	[Larg_Unit_Price] [decimal](18, 2) NULL,
	[Small_Unit_Price] [decimal](18, 2) NULL,
	[Unit_Used_for_Price] [int] NULL,
	[S_price_LargeUnit] [decimal](18, 2) NULL,
	[S_price_SmallUnit] [decimal](18, 2) NULL,
	[Qnty_value] [decimal](18, 2) NULL,
	[Item_Total_WithoutTax] [decimal](18, 2) NULL,
	[Item_ATS05] [decimal](18, 2) NULL,
	[Item_ATS05_Value] [decimal](18, 2) NULL,
	[Item_TaxSales10] [decimal](18, 2) NULL,
	[Item_TaxSales10_Value] [decimal](18, 2) NULL,
	[Total] [decimal](18, 2) NULL,
	[Is_Item_CommissionValue] [int] NULL,
	[Item_CommissionValue] [decimal](18, 2) NULL,
	[Unit_CommissionValue] [decimal](18, 2) NULL,
	[Item_CommissionRate] [decimal](18, 2) NULL,
	[Unit_CommissionRate] [decimal](18, 2) NULL,
	[Net_Total_Item_Commission] [decimal](18, 2) NULL,
	[ItemCommissionTotal] [decimal](18, 2) NULL,
	[Item_Net_Total_Price] [decimal](18, 2) NULL,
	[QntyStr] [nvarchar](500) NULL,
	[QntyStrColumn] [nvarchar](500) NULL,
	[Item_Notes] [nvarchar](500) NULL,
	[Order_PurshingId] [bigint] NULL,
	[StoreId] [bigint] NULL,
	[Is_SarfFromStore] [int] NULL,
	[Is_AddToInvoice] [int] NULL,
	[Item_DeletedFlage] [int] NULL,
	[Item_ProcessedFlage] [int] NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[Update_ServerDateTime] [datetime] NULL,
	[EditOperationDate] [datetime] NULL,
	[Edit_User_Id] [bigint] NULL,
	[ProcessNo] [bigint] NULL,
	[OperationDate] [datetime] NULL,
	[Store_Id] [bigint] NULL,
	[Unit_Id] [bigint] NULL,
	[UnitStr] [nvarchar](500) NULL,
	[Is_NotShowOnInvoice] [int] NULL,
	[Detailes_PurshingId] [bigint] NULL,
	[CarsWorkOrdersRequestedDetailsId] [bigint] NULL,
	[Item_Category_Id] [bigint] NULL,
	[Item_Category_Name] [nvarchar](500) NULL,
	[P_Price] [decimal](18, 2) NULL,
	[P_priceAvgCost] [decimal](18, 2) NULL,
	[Order_Purshing_No] [bigint] NULL,
	[Order_Purshing_Date] [datetime] NULL
)  

 

CREATE TABLE [dbo].[CarsWorkOrdersDeliverItemBalances](
	[Id] [bigint]  {IDENTITY} primary key  NOT NULL,
	[WarshaStoresId] [bigint] NULL,
	[CarsWorkOrdersId] [bigint] NULL,
	[Item_Id] [bigint] NULL,
	[ItemCode] [nvarchar](200) NULL,
	[Qnty] [decimal](18, 2) NULL,
	[Larg_Qnty] [decimal](18, 2) NULL,
	[Small_Qnty] [decimal](18, 2) NULL,
	[Item_No] [decimal](18, 2) NULL,
	[PriceUnit] [decimal](18, 2) NULL,
	[Larg_Unit_Price] [decimal](18, 2) NULL,
	[Small_Unit_Price] [decimal](18, 2) NULL,
	[Unit_Used_for_Price] [int] NULL,
	[Unit_Id] [bigint] NULL,
	[UnitStr] [nvarchar](500) NULL,
	[Is_NotShowOnInvoice] [int] NULL,
	[P_Price] [decimal](18, 2) NULL,
	[P_priceAvgCost] [decimal](18, 2) NULL
)  

 

CREATE TABLE [dbo].[CarsWorkOrdersItemProcessedDetails](
	[Id] [bigint]  {IDENTITY} primary key  NOT NULL,
	[CarsWorkOrdersId] [bigint] NULL,
	[Item_Id] [bigint] NULL,
	[Unit_ItemsId] [bigint] NULL,
	[Detailes_PurshingId] [bigint] NULL,
	[ItemCode] [nvarchar](100) NULL,
	[CarsWorkOrdersRequestedDetailsId] [bigint] NULL,
	[CarsWorkOrdersDeliverItemBalancesId] [bigint] NULL,
	[CarsWorkOrdersDeliverDetailsId] [bigint] NULL,
	[Qnty] [decimal](18, 2) NULL,
	[Item_Name] [nvarchar](500) NULL,
	[Invoice_Item_Name] [nvarchar](100) NULL,
	[Specification] [nvarchar](500) NULL,
	[Larg_Qnty] [decimal](18, 2) NULL,
	[Small_Qnty] [decimal](18, 2) NULL,
	[Item_No] [decimal](18, 2) NULL,
	[PriceUnit] [decimal](18, 2) NULL,
	[Larg_Unit_Price] [decimal](18, 2) NULL,
	[Small_Unit_Price] [decimal](18, 2) NULL,
	[Unit_Used_for_Price] [int] NULL,
	[S_price_LargeUnit] [decimal](18, 2) NULL,
	[S_price_SmallUnit] [decimal](18, 2) NULL,
	[Qnty_value] [decimal](18, 2) NULL,
	[Item_Total_WithoutTax] [decimal](18, 2) NULL,
	[Item_ATS05] [decimal](18, 2) NULL,
	[Item_ATS05_Value] [decimal](18, 2) NULL,
	[Item_TaxSales10] [decimal](18, 2) NULL,
	[Item_TaxSales10_Value] [decimal](18, 2) NULL,
	[Total] [decimal](18, 2) NULL,
	[Is_Item_CommissionValue] [int] NULL,
	[Item_CommissionValue] [decimal](18, 2) NULL,
	[Unit_CommissionValue] [decimal](18, 2) NULL,
	[Item_CommissionRate] [decimal](18, 2) NULL,
	[Unit_CommissionRate] [decimal](18, 2) NULL,
	[Net_Total_Item_Commission] [decimal](18, 2) NULL,
	[ItemCommissionTotal] [decimal](18, 2) NULL,
	[Item_Net_Total_Price] [decimal](18, 2) NULL,
	[QntyStr] [nvarchar](500) NULL,
	[QntyStrColumn] [nvarchar](500) NULL,
	[Item_Notes] [nvarchar](500) NULL,
	[Order_PurshingId] [bigint] NULL,
	[StoreId] [bigint] NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[Update_ServerDateTime] [datetime] NULL,
	[EditOperationDate] [datetime] NULL,
	[Edit_User_Id] [bigint] NULL,
	[ProcessNo] [bigint] NULL,
	[OperationDate] [datetime] NULL,
	[Store_Id] [bigint] NULL,
	[Unit_Id] [bigint] NULL,
	[UnitStr] [nvarchar](500) NULL,
	[Is_NotShowOnInvoice] [int] NULL,
	[Is_SarfFromStore] [int] NULL,
	[Is_AddToInvoice] [int] NULL,
	[Item_DeletedFlage] [int] NULL,
	[Item_ProcessedFlage] [int] NULL,
	[Item_Category_Id] [bigint] NULL,
	[Item_Category_Name] [nvarchar](500) NULL,
	[Emp_Id] [bigint] NULL,
	[Emp_Name] [nvarchar](500) NULL,
	[IsMasna3iatKhamat] [int] NULL,
	[P_Price] [decimal](18, 2) NULL,
	[P_priceAvgCost] [decimal](18, 2) NULL,
	[Order_Purshing_No] [bigint] NULL,
	[Order_Purshing_Date] [datetime] NULL,
	[Qnty_Value_P_Price] [decimal](18, 2) NULL,
	[Profits] [decimal](18, 2) NULL
)  

 
CREATE TABLE [dbo].[CarsWorkOrdersItemProcessedEmployees](
	[Id] [bigint]  {IDENTITY} primary key  NOT NULL,
	[CarsWorkOrdersItemProcessedDetailsId] [bigint] NULL,
	[CarsWorkOrdersId] [bigint] NULL,
	[CarsId] [bigint] NULL,
	[Emp_Id] [bigint] NULL,
	[Emp_Name] [nvarchar](500) NULL,
	[Ratio] [decimal](18, 2) NULL,
	[Value] [decimal](18, 2) NULL,
	[Notes] [nvarchar](max) NULL,
	[CarsWorkOrdersItemReversedProcessedDetailsId] [bigint] NULL
)     

 

CREATE TABLE [dbo].[CarsWorkOrdersItemReversedProcessedDetails](
	[Id] [bigint]  {IDENTITY} primary key  NOT NULL,
	[CarsWorkOrdersId] [bigint] NULL,
	[Item_Id] [bigint] NULL,
	[Unit_ItemsId] [bigint] NULL,
	[Detailes_PurshingId] [bigint] NULL,
	[ItemCode] [nvarchar](100) NULL,
	[CarsWorkOrdersRequestedDetailsId] [bigint] NULL,
	[CarsWorkOrdersDeliverItemBalancesId] [bigint] NULL,
	[CarsWorkOrdersDeliverDetailsId] [bigint] NULL,
	[Qnty] [decimal](18, 2) NULL,
	[Item_Name] [nvarchar](500) NULL,
	[Invoice_Item_Name] [nvarchar](100) NULL,
	[Specification] [nvarchar](500) NULL,
	[Larg_Qnty] [decimal](18, 2) NULL,
	[Small_Qnty] [decimal](18, 2) NULL,
	[Item_No] [decimal](18, 2) NULL,
	[PriceUnit] [decimal](18, 2) NULL,
	[Larg_Unit_Price] [decimal](18, 2) NULL,
	[Small_Unit_Price] [decimal](18, 2) NULL,
	[Unit_Used_for_Price] [int] NULL,
	[S_price_LargeUnit] [decimal](18, 2) NULL,
	[S_price_SmallUnit] [decimal](18, 2) NULL,
	[Qnty_value] [decimal](18, 2) NULL,
	[Item_Total_WithoutTax] [decimal](18, 2) NULL,
	[Item_ATS05] [decimal](18, 2) NULL,
	[Item_ATS05_Value] [decimal](18, 2) NULL,
	[Item_TaxSales10] [decimal](18, 2) NULL,
	[Item_TaxSales10_Value] [decimal](18, 2) NULL,
	[Total] [decimal](18, 2) NULL,
	[Is_Item_CommissionValue] [int] NULL,
	[Item_CommissionValue] [decimal](18, 2) NULL,
	[Unit_CommissionValue] [decimal](18, 2) NULL,
	[Item_CommissionRate] [decimal](18, 2) NULL,
	[Unit_CommissionRate] [decimal](18, 2) NULL,
	[Net_Total_Item_Commission] [decimal](18, 2) NULL,
	[ItemCommissionTotal] [decimal](18, 2) NULL,
	[Item_Net_Total_Price] [decimal](18, 2) NULL,
	[QntyStr] [nvarchar](500) NULL,
	[QntyStrColumn] [nvarchar](500) NULL,
	[Item_Notes] [nvarchar](500) NULL,
	[Order_PurshingId] [bigint] NULL,
	[StoreId] [bigint] NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[Update_ServerDateTime] [datetime] NULL,
	[EditOperationDate] [datetime] NULL,
	[Edit_User_Id] [bigint] NULL,
	[ProcessNo] [bigint] NULL,
	[OperationDate] [datetime] NULL,
	[Store_Id] [bigint] NULL,
	[Unit_Id] [bigint] NULL,
	[UnitStr] [nvarchar](500) NULL,
	[Is_NotShowOnInvoice] [int] NULL,
	[Is_SarfFromStore] [int] NULL,
	[Is_AddToInvoice] [int] NULL,
	[Item_DeletedFlage] [int] NULL,
	[Item_ProcessedFlage] [int] NULL,
	[Item_Category_Id] [bigint] NULL,
	[Item_Category_Name] [nvarchar](500) NULL,
	[Emp_Id] [bigint] NULL,
	[Emp_Name] [nvarchar](500) NULL,
	[IsMasna3iatKhamat] [int] NULL,
	[P_Price] [decimal](18, 2) NULL,
	[P_priceAvgCost] [decimal](18, 2) NULL,
	[Order_Purshing_No] [bigint] NULL,
	[Order_Purshing_Date] [datetime] NULL,
	[Qnty_Value_P_Price] [decimal](18, 2) NULL,
	[Profits] [decimal](18, 2) NULL
)  

 

CREATE TABLE [dbo].[CarsWorkOrdersRequestedDetails](
	[Id] [bigint]  {IDENTITY} primary key    NOT NULL,
	[CarsWorkOrdersId] [bigint] NULL,
	[Item_Id] [bigint] NULL,
	[Unit_ItemsId] [bigint] NULL,
	[ItemCode] [nvarchar](100) NULL,
	[Item_Name] [nvarchar](500) NULL,
	[Invoice_Item_Name] [nvarchar](500) NULL,
	[Specification] [nvarchar](500) NULL,
	[Larg_Qnty] [decimal](18, 2) NULL,
	[Small_Qnty] [decimal](18, 2) NULL,
	[Item_No] [decimal](18, 2) NULL,
	[Qnty] [decimal](18, 2) NULL,
	[PriceUnit] [decimal](18, 2) NULL,
	[Larg_Unit_Price] [decimal](18, 2) NULL,
	[Small_Unit_Price] [decimal](18, 2) NULL,
	[Unit_Used_for_Price] [int] NULL,
	[S_price_LargeUnit] [decimal](18, 2) NULL,
	[S_price_SmallUnit] [decimal](18, 2) NULL,
	[Qnty_value] [decimal](18, 2) NULL,
	[Item_Total_WithoutTax] [decimal](18, 2) NULL,
	[Item_ATS05] [decimal](18, 2) NULL,
	[Item_ATS05_Value] [decimal](18, 2) NULL,
	[Item_TaxSales10] [decimal](18, 2) NULL,
	[Item_TaxSales10_Value] [decimal](18, 2) NULL,
	[Total] [decimal](18, 2) NULL,
	[Is_Item_CommissionValue] [int] NULL,
	[Item_CommissionValue] [decimal](18, 2) NULL,
	[Unit_CommissionValue] [decimal](18, 2) NULL,
	[Item_CommissionRate] [decimal](18, 2) NULL,
	[Unit_CommissionRate] [decimal](18, 2) NULL,
	[Net_Total_Item_Commission] [decimal](18, 2) NULL,
	[ItemCommissionTotal] [decimal](18, 2) NULL,
	[Item_Net_Total_Price] [decimal](18, 2) NULL,
	[QntyStr] [nvarchar](500) NULL,
	[QntyStrColumn] [nvarchar](500) NULL,
	[Unit_Id] [bigint] NULL,
	[UnitStr] [nvarchar](500) NULL,
	[Item_Notes] [nvarchar](500) NULL,
	[StoreId] [bigint] NULL,
	[Is_SarfFromStore] [int] NULL,
	[Is_AddToInvoice] [int] NULL,
	[Item_DeletedFlage] [int] NULL,
	[Item_ProcessedFlage] [int] NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[Update_ServerDateTime] [datetime] NULL,
	[EditOperationDate] [datetime] NULL,
	[Edit_User_Id] [bigint] NULL,
	[ProcessNo] [bigint] NULL,
	[OperationDate] [datetime] NULL,
	[Is_ItemDeliver] [bit] NULL,
	[Larg_Qnty_Deliver] [decimal](18, 2) NULL,
	[Small_Qnty_Deliver] [decimal](18, 2) NULL,
	[Item_No_Deliver] [decimal](18, 2) NULL,
	[Qnty_Deliver] [decimal](18, 2) NULL,
	[Store_Id] [bigint] NULL,
	[Is_NotShowOnInvoice] [int] NULL,
	[Order_PurshingId] [bigint] NULL,
	[Detailes_PurshingId] [bigint] NULL,
	[CarsWorkOrdersDeliverDetailsId] [bigint] NULL,
	[Item_Category_Id] [bigint] NULL,
	[Item_Category_Name] [nvarchar](500) NULL,
	[P_Price] [decimal](18, 2) NULL,
	[P_priceAvgCost] [decimal](18, 2) NULL,
	[Order_Purshing_No] [bigint] NULL,
	[Order_Purshing_Date] [datetime] NULL
)  

 
  ";
                opp.insert_me(str);

            }
            catch
            {
            }
        }
        public static void AlterDB_DateTime()
        {
            operations opp = new operations();
            opp.conn.Open();
            DataTable schema = opp.conn.GetSchema("Tables");
            opp.conn.Close();
            foreach (DataRow row in schema.Rows)
            {
                string tableName = (string)row[2];
                AlterBD_(ref opp, tableName, "OperationDate", "DateTime", "getdate()");
                AlterBD_(ref opp, tableName, "User_Id", "Int", "0", "0");
                AlterBD_(ref opp, tableName, "Branch_Id", "Int", "0", "0");
                AlterBD_(ref opp, tableName, "Insert_ServerDateTime", "DateTime", "getdate()");
                AlterBD_(ref opp, tableName, "Update_ServerDateTime", "DateTime", "getdate()");
                AlterBD_(ref opp, tableName, "EditOperationDate", "DateTime", "getdate()");
                AlterBD_(ref opp, tableName, "Edit_User_Id", "Int", "0", "0");
                try
                {
                    opp.insert_me("  ALTER TABLE Item   DROP COLUMN ServerDateTime;");
                }
                catch
                {
                }
            }
        }
        public static void AlterDB_ProcessNo()
        {
            operations opp = new operations();
            opp.conn.Open();
            DataTable schema = opp.conn.GetSchema("Tables");
            opp.conn.Close();

            foreach (DataRow row in schema.Rows)
            {
                string tableName = (string)row[2];
                AlterBD_(ref opp, tableName, "ProcessNo", "bigint", "0", "-1");

            }


        }
        public static void Set_TaxInfo(double Taxx = 0, double ATS = 0)
        {
            operations opp = new operations();
            Set_TaxInfo(ref opp, Taxx, ATS);
        }

        private static void Set_TaxInfo(ref operations opp, double Taxx = 0, double ATS = 0)
        {
            string str = "";
            try
            {
                str = "ALTER TABLE[AccountTaxInfo] DROP CONSTRAINT PK_AccountTaxInfo; ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {
                str = "  drop table  AccountTaxInfo ";
                opp.insert_me(str);
            }
            catch
            {
            }
            try
            {

                try
                {
                    str = " CREATE TABLE [dbo].[AccountTaxInfo]( [Id] bigint {IDENTITY} primary key NOT NULL, [TaxName_Ar] [nvarchar](500) NULL, [TaxName_En] [nvarchar](500) NULL, [TaxRate] [decimal](18, 2) NULL, [MainCode] [nvarchar](500) NULL, [SubCode] [nvarchar](500) NULL   ,MinAllowedValueToComputeTax [decimal] null )   ";
                    opp.insert_me(str);


                    str = " INSERT [dbo].[AccountTaxInfo] (  [TaxName_Ar], [TaxName_En], [TaxRate],MainCode  ,SubCode  ) VALUES (  N'نسبة القيمة المضافة', N'VAT rate', CAST(" + Taxx + " AS decimal(18, 3)),'T1','')   ";
                    str = str + " INSERT [dbo].[AccountTaxInfo] (  [TaxName_Ar], [TaxName_En], [TaxRate],MainCode  ,SubCode) VALUES (  N' نسبة أرباح تجارية وصناعية - مواد خام - توريدات - قطع الغيار', N'Commercial and industrial profits', CAST(" + ATS + " AS decimal(18, 3)),'T4','W002')";
                    str = str + " INSERT [dbo].[AccountTaxInfo] (  [TaxName_Ar], [TaxName_En], [TaxRate],MainCode  ,SubCode) VALUES (  N'نسبة أرباح تجارية وصناعية -  الصيانة والخدمات ', N'Commercial and industrial profits', CAST(" + ATS + " AS decimal(18, 3)),'T4','W004')";
                    //str = str + " INSERT [dbo].[AccountTaxInfo] (  [TaxName_Ar], [TaxName_En], [TaxRate],MainCode  ,SubCode) VALUES (  N'نسبة أرباح تجارية وصناعية -  الاستشارات سواء كانت هندسية أو مالية  ', N'Commercial and industrial profits', CAST(" + ATS + " AS decimal(18, 3)),'T4','W004')";
                    opp.insert_me(str);
                }
                catch
                {
                }
                try
                {
                    str = " ";
                    opp.insert_me(str);
                }
                catch
                {
                }
            }
            catch
            {
            }
        }


        public static void Set_Lookup_OrderDeliveryTypes()
        {
            operations opp = new operations();
            ReSetCreate_Lookup_OrderDeliveryTypes(ref opp);
        }

        private static void ReSetCreate_Lookup_OrderDeliveryTypes(ref operations opp)
        {
            string str = "";

            try
            {
                str = "ALTER TABLE[Lookup_OrderDeliveryTypes] DROP CONSTRAINT DF__Lookup_Or__Defau__3EC810AE; ";
                opp.insert_me(str);
            }
            catch
            {
            }

            try
            {
                str = "ALTER TABLE[Lookup_OrderDeliveryTypes] DROP CONSTRAINT DF__Lookup_Or__Defau__3FBC34E7; ";
                opp.insert_me(str);
            }
            catch
            {
            }

            try
            {
                str = "  drop table  Lookup_OrderDeliveryTypes ";
                opp.insert_me(str);
            }
            catch
            {
            }

            Create_Lookup_OrderDeliveryTypes(ref opp);
        }
        private static void Create_Lookup_OrderDeliveryTypes(ref operations opp)
        {
            try
            {
                string str = @$"  
CREATE TABLE [dbo].[Lookup_OrderDeliveryTypes] (
    [Id] bigint {IDENTITY} NOT NULL PRIMARY KEY ,
    [Name]    NVARCHAR (500)   NOT NULL,
    [Name_En]    NVARCHAR (500)   NOT NULL,
    [Default_Service_Rate] DECIMAL (18, 2)   DEFAULT ((0.00)) NOT NULL, -- نسبة الخدمة مئوية للصالة
    [Default_Delivery_Fee] DECIMAL (18, 2)   DEFAULT ((0.00)) NOT NULL  -- رسوم التوصيل الثابتة
    
);
 
 ";
                opp.insert_me(str);


                str = @$"   

 
 

INSERT INTO [dbo].[Lookup_OrderDeliveryTypes] ([Id], [Name], [Name_En], [Default_Service_Rate], [Default_Delivery_Fee]) 
VALUES 
(0, N'غير محدد', N'Undefined', 0.00, 0.00),
(1, N'صالة', N'Dine-In', 10.00, 0.00),  -- صالة مع 12% خدمة افتراضية
(2, N'سفري', N'Takeaway', 0.00, 0.00),  -- سفري بدون رسوم إضافية
(3, N'توصيل', N'Delivery', 0.00, 15.00); -- توصيل مع 15 كقيمة شحن افتراضية

 
 ";
                str = ExecuteInsertWithIdentityCheck("Lookup_OrderDeliveryTypes", str);
                opp.insert_me(str);
            }
            catch
            {
            }



        }
        private static void Update_(ref operations opp, string TableName, string ColumnName, string UpdateValue)
        {
            string str = "";
            try
            {
                string[] textArray1 = new string[] { "Update ", TableName, "  Set  ", ColumnName, " = ", UpdateValue };
                str = string.Concat(textArray1);
                opp.insert_me(str);
            }
            catch
            {
            }
        }
        private static void Update(ref operations opp, string TableName, string ColumnName, string UpdateValue)
        {
            string mysql = "";
            try
            {


                mysql = @"Update " + TableName + "  Set  " + ColumnName + " = " + UpdateValue + "";
                opp.insert_me(mysql);




            }
            catch { }
        }
        private static void Update(ref operations opp, string TableName, string ColumnName, string UpdateValue, string Where_)
        {
            string mysql = "";
            try
            {


                mysql = @"Update " + TableName + "  Set  " + ColumnName + " = " + UpdateValue + " Where " + Where_ + "";
                opp.insert_me(mysql);




            }
            catch { }


            AlterBD_(ref opp, "SysConfigMenus", "IsShowToUser", "int", "1", "1");



            mysql = " UPDATE       SysConfigMenus SET IsShowToUser =0 WHERE (Parent_Id = 0) AND (Id = 321) ";
            mysql = mysql + "  UPDATE       SysConfigMenusPERMATIONS SET                IsShowToUser = 0 FROM            SysConfigMenusPERMATIONS INNER JOIN  SysConfigMenus ON SysConfigMenusPERMATIONS.Menu_ID = SysConfigMenus.Id WHERE        (SysConfigMenusPERMATIONS.Parent_Id = 321) ";
            AlterBD_(ref opp, "SysConfigMenusPERMATIONS", "IsShowToUser", "int", "1", "1", mysql);
        }

        static void UpdateSermekTable(ref operations opp)
        {
            #region mysql
            string mysql = @"
 
CREATE TABLE [dbo].[Items_Bodies_Name](
	[Id] [bigint] {IDENTITY} NOT NULL PRIMARY KEY,
	[ItemBody_Name] [nvarchar](50) NULL,
	[OperationDate] [datetime] NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[Update_ServerDateTime] [datetime] NULL,
	[EditOperationDate] [datetime] NULL,
	[Edit_User_Id] [bigint] NULL,
	[ProcessNo] [bigint] NULL 

	)
";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"
CREATE TABLE [dbo].[Items_Color_Name](
	[Id] [bigint] {IDENTITY} NOT NULL PRIMARY KEY,
	[Color_Name] [nvarchar](50) NULL,
	[OperationDate] [datetime] NULL,
	[User_Id] [bigint] NULL,
	[Branch_Id] [bigint] NULL,
	[Insert_ServerDateTime] [datetime] NULL,
	[Update_ServerDateTime] [datetime] NULL,
	[EditOperationDate] [datetime] NULL,
	[Edit_User_Id] [bigint] NULL,
	[ProcessNo] [bigint] NULL 
  )
 ";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"
CREATE TABLE [dbo].[Items_Types_Name](
	[Id] [bigint] {IDENTITY} NOT NULL PRIMARY KEY,
	[ItemsType_Name] [nvarchar](50) NULL,
	[OperationDate] [datetime] NULL DEFAULT (getdate()),
	[User_Id] [bigint] NULL DEFAULT ((0)),
	[Branch_Id] [bigint] NULL DEFAULT ((0)),
	[Insert_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
	[Update_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
	[EditOperationDate] [datetime] NULL DEFAULT (getdate()),
	[Edit_User_Id] [bigint] NULL DEFAULT ((0)),
	[ProcessNo] [bigint] NULL DEFAULT ((0)) 
 )  

";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"
CREATE TABLE [dbo].[Items_TypesColor_Name](
	[Id] [bigint] {IDENTITY} NOT NULL PRIMARY KEY,
	[Type_Name] [nvarchar](50) NULL,
	[OperationDate] [datetime] NULL DEFAULT (getdate()),
	[User_Id] [bigint] NULL DEFAULT ((0)),
	[Branch_Id] [bigint] NULL DEFAULT ((0)),
	[Insert_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
	[Update_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
	[EditOperationDate] [datetime] NULL DEFAULT (getdate()),
	[Edit_User_Id] [bigint] NULL DEFAULT ((0)),
	[ProcessNo] [bigint] NULL DEFAULT ((0)))

 ";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"
CREATE TABLE [dbo].[Items_TypesDegreeFarz_Name](
	[Id] [bigint] {IDENTITY} NOT NULL PRIMARY KEY,
	[Type_Name] [nvarchar](50) NULL,
	[OperationDate] [datetime] NULL DEFAULT (getdate()),
	[User_Id] [bigint] NULL DEFAULT ((0)),
	[Branch_Id] [bigint] NULL DEFAULT ((0)),
	[Insert_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
	[Update_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
	[EditOperationDate] [datetime] NULL DEFAULT (getdate()),
	[Edit_User_Id] [bigint] NULL DEFAULT ((0)),
	[ProcessNo] [bigint] NULL DEFAULT ((0))
	)

 ";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"
CREATE TABLE [dbo].[Items_TypesDescription_Name](
	[Id] [bigint] {IDENTITY} NOT NULL PRIMARY KEY,
	[Type_Name] [nvarchar](50) NULL,
	[OperationDate] [datetime] NULL DEFAULT (getdate()),
	[User_Id] [bigint] NULL DEFAULT ((0)),
	[Branch_Id] [bigint] NULL DEFAULT ((0)),
	[Insert_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
	[Update_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
	[EditOperationDate] [datetime] NULL DEFAULT (getdate()),
	[Edit_User_Id] [bigint] NULL DEFAULT ((0)),
	[ProcessNo] [bigint] NULL DEFAULT ((0)) 
	)
";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"

CREATE TABLE [dbo].[Items_TypesDescriptionNameOther](
	[Id] [bigint] {IDENTITY} NOT NULL PRIMARY KEY,
	[Type_Name] [nvarchar](50) NULL,
	[OperationDate] [datetime] NULL DEFAULT (getdate()),
	[User_Id] [bigint] NULL DEFAULT ((0)),
	[Branch_Id] [bigint] NULL DEFAULT ((0)),
	[Insert_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
	[Update_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
	[EditOperationDate] [datetime] NULL DEFAULT (getdate()),
	[Edit_User_Id] [bigint] NULL DEFAULT ((0)),
	[ProcessNo] [bigint] NULL DEFAULT ((0))
	)

";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"
CREATE TABLE [dbo].[Items_TypesFactory_Name](
	[Id] [bigint] {IDENTITY} NOT NULL PRIMARY KEY,
	[Type_Name] [nvarchar](50) NULL,
	[OperationDate] [datetime] NULL DEFAULT (getdate()),
	[User_Id] [bigint] NULL DEFAULT ((0)),
	[Branch_Id] [bigint] NULL DEFAULT ((0)),
	[Insert_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
	[Update_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
	[EditOperationDate] [datetime] NULL DEFAULT (getdate()),
	[Edit_User_Id] [bigint] NULL DEFAULT ((0)),
	[ProcessNo] [bigint] NULL DEFAULT ((0)) 
	)

";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"
CREATE TABLE [dbo].[Items_TypesSize_Name](
	[Id] [bigint] {IDENTITY} NOT NULL PRIMARY KEY,
	[Type_Name] [nvarchar](50) NULL,
	[OperationDate] [datetime] NULL DEFAULT (getdate()),
	[User_Id] [bigint] NULL DEFAULT ((0)),
	[Branch_Id] [bigint] NULL DEFAULT ((0)),
	[Insert_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
	[Update_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
	[EditOperationDate] [datetime] NULL DEFAULT (getdate()),
	[Edit_User_Id] [bigint] NULL DEFAULT ((0)),
	[ProcessNo] [bigint] NULL DEFAULT ((0)) 

	)

 ";

            mysql = @"
ALTER TABLE [dbo].[Items_Bodies_Name] ADD  DEFAULT ((0)) FOR [User_Id]
";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"
ALTER TABLE [dbo].[Items_Bodies_Name] ADD  DEFAULT ((0)) FOR [Branch_Id]
";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"
ALTER TABLE [dbo].[Items_Bodies_Name] ADD  DEFAULT (getdate()) FOR [Insert_ServerDateTime]
";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"
ALTER TABLE [dbo].[Items_Bodies_Name] ADD  DEFAULT (getdate()) FOR [Update_ServerDateTime]
";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"
ALTER TABLE [dbo].[Items_Bodies_Name] ADD  DEFAULT (getdate()) FOR [EditOperationDate]
";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"
ALTER TABLE [dbo].[Items_Bodies_Name] ADD  DEFAULT ((0)) FOR [Edit_User_Id]
";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"
ALTER TABLE [dbo].[Items_Bodies_Name] ADD  DEFAULT ((0)) FOR [ProcessNo]
";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"
ALTER TABLE [dbo].[Items_Color_Name] ADD  DEFAULT (getdate()) FOR [OperationDate]
";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"
ALTER TABLE [dbo].[Items_Color_Name] ADD  DEFAULT ((0)) FOR [User_Id]
";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"
ALTER TABLE [dbo].[Items_Color_Name] ADD  DEFAULT ((0)) FOR [Branch_Id]
";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"
ALTER TABLE [dbo].[Items_Color_Name] ADD  DEFAULT (getdate()) FOR [Insert_ServerDateTime]
";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"
ALTER TABLE [dbo].[Items_Color_Name] ADD  DEFAULT (getdate()) FOR [Update_ServerDateTime]
";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"
ALTER TABLE [dbo].[Items_Color_Name] ADD  DEFAULT (getdate()) FOR [EditOperationDate]
";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"
ALTER TABLE [dbo].[Items_Color_Name] ADD  DEFAULT ((0)) FOR [Edit_User_Id]
";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"
ALTER TABLE [dbo].[Items_Color_Name] ADD  DEFAULT ((0)) FOR [ProcessNo]
";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }



            #endregion
        }
        public static void SetSeramekData()
        {
            operations opp = new operations();
            string mysql = "";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"

SET IDENTITY_INSERT [dbo].[Items_Types_Name] ON 

INSERT [dbo].[Items_Types_Name] ([Id], [ItemsType_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(1 AS Numeric(18, 0)), N'غير محدد', CAST(N'2023-05-10 07:41:48.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:41:48.160' AS DateTime), CAST(N'2023-05-10 07:41:48.160' AS DateTime), CAST(N'2023-05-10 07:41:48.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_Types_Name] ([Id], [ItemsType_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(2 AS Numeric(18, 0)), N'بورسلين', CAST(N'2023-05-10 07:41:48.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:41:48.160' AS DateTime), CAST(N'2023-05-10 07:41:48.160' AS DateTime), CAST(N'2023-05-10 07:41:48.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_Types_Name] ([Id], [ItemsType_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(3 AS Numeric(18, 0)), N'ديكور', CAST(N'2023-05-10 07:41:48.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:41:48.160' AS DateTime), CAST(N'2023-05-10 07:41:48.160' AS DateTime), CAST(N'2023-05-10 07:41:48.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_Types_Name] ([Id], [ItemsType_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(4 AS Numeric(18, 0)), N'سيراميك', CAST(N'2023-05-10 07:41:48.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:41:48.160' AS DateTime), CAST(N'2023-05-10 07:41:48.160' AS DateTime), CAST(N'2023-05-10 07:41:48.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_Types_Name] ([Id], [ItemsType_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(5 AS Numeric(18, 0)), N'صحي اطقم', CAST(N'2023-05-10 07:41:48.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:41:48.160' AS DateTime), CAST(N'2023-05-10 07:41:48.160' AS DateTime), CAST(N'2023-05-10 07:41:48.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_Types_Name] ([Id], [ItemsType_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(6 AS Numeric(18, 0)), N'صحي مفردات', CAST(N'2023-05-10 07:41:48.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:41:48.160' AS DateTime), CAST(N'2023-05-10 07:41:48.160' AS DateTime), CAST(N'2023-05-10 07:41:48.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_Types_Name] ([Id], [ItemsType_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(7 AS Numeric(18, 0)), N'كابينة', CAST(N'2023-05-10 07:41:48.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:41:48.160' AS DateTime), CAST(N'2023-05-10 07:41:48.160' AS DateTime), CAST(N'2023-05-10 07:41:48.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_Types_Name] ([Id], [ItemsType_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(8 AS Numeric(18, 0)), N'لاصق سيراميك', CAST(N'2023-05-10 07:41:48.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:41:48.160' AS DateTime), CAST(N'2023-05-10 07:41:48.160' AS DateTime), CAST(N'2023-05-10 07:41:48.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_Types_Name] ([Id], [ItemsType_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(9 AS Numeric(18, 0)), N'ماده سيراميك', CAST(N'2023-05-10 07:41:48.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:41:48.160' AS DateTime), CAST(N'2023-05-10 07:41:48.160' AS DateTime), CAST(N'2023-05-10 07:41:48.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_Types_Name] ([Id], [ItemsType_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(10 AS Numeric(18, 0)), N'نواكل', CAST(N'2023-05-10 07:41:48.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:41:48.160' AS DateTime), CAST(N'2023-05-10 07:41:48.160' AS DateTime), CAST(N'2023-05-10 07:41:48.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_Types_Name] ([Id], [ItemsType_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(11 AS Numeric(18, 0)), N'وحدات', CAST(N'2023-05-10 07:41:48.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:41:48.160' AS DateTime), CAST(N'2023-05-10 07:41:48.160' AS DateTime), CAST(N'2023-05-10 07:41:48.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_Types_Name] ([Id], [ItemsType_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(12 AS Numeric(18, 0)), N'بانيوهات', CAST(N'2023-05-10 07:42:41.547' AS DateTime), 0, 0, CAST(N'2023-05-10 07:42:41.547' AS DateTime), CAST(N'2023-05-10 07:42:41.547' AS DateTime), CAST(N'2023-05-10 07:42:41.547' AS DateTime), 0, 0)
SET IDENTITY_INSERT [dbo].[Items_Types_Name] OFF

";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"

SET IDENTITY_INSERT [dbo].[Items_TypesColor_Name] ON 

INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(1 AS Numeric(18, 0)), NULL, CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(2 AS Numeric(18, 0)), N'ابيض', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(3 AS Numeric(18, 0)), N'ابيض عادي', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(4 AS Numeric(18, 0)), N'ابيض ليزر', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(5 AS Numeric(18, 0)), N'ابيض* اسود', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(6 AS Numeric(18, 0)), N'ازرق فاتح', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(7 AS Numeric(18, 0)), N'اسود', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(8 AS Numeric(18, 0)), N'اسود عادي', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(9 AS Numeric(18, 0)), N'اسود ليزر', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(10 AS Numeric(18, 0)), N'بدون لون', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(11 AS Numeric(18, 0)), N'برجمون', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(12 AS Numeric(18, 0)), N'بني', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(13 AS Numeric(18, 0)), N'بيج', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(14 AS Numeric(18, 0)), N'بيج *ازرق', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(15 AS Numeric(18, 0)), N'بيج حجاري', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(16 AS Numeric(18, 0)), N'بيج حجري', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(17 AS Numeric(18, 0)), N'بيج فاتح', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(18 AS Numeric(18, 0)), N'بيج مطبخ', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(19 AS Numeric(18, 0)), N'ذهبي', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(20 AS Numeric(18, 0)), N'رصاصي', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(21 AS Numeric(18, 0)), N'رصاصي حجري', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(22 AS Numeric(18, 0)), N'رصاصي غامق', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(23 AS Numeric(18, 0)), N'رصاصي فاتح', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(24 AS Numeric(18, 0)), N'رصاصي مطبخ', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(25 AS Numeric(18, 0)), N'رمادي', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(26 AS Numeric(18, 0)), N'عادي', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(27 AS Numeric(18, 0)), N'عسلي', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(28 AS Numeric(18, 0)), N'غامق', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(29 AS Numeric(18, 0)), N'فاتح', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(30 AS Numeric(18, 0)), N'فضي', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(31 AS Numeric(18, 0)), N'كافيه غامق', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(32 AS Numeric(18, 0)), N'كافيه فاتح', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesColor_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(33 AS Numeric(18, 0)), N'ليزر', CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), CAST(N'2023-05-10 07:50:16.663' AS DateTime), 0, 0)
SET IDENTITY_INSERT [dbo].[Items_TypesColor_Name] OFF

";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"

SET IDENTITY_INSERT [dbo].[Items_TypesDegreeFarz_Name] ON 

INSERT [dbo].[Items_TypesDegreeFarz_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(1 AS Numeric(18, 0)), NULL, CAST(N'2023-05-10 07:50:46.760' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:46.760' AS DateTime), CAST(N'2023-05-10 07:50:46.760' AS DateTime), CAST(N'2023-05-10 07:50:46.760' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDegreeFarz_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(2 AS Numeric(18, 0)), N'- 1', CAST(N'2023-05-10 07:50:46.760' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:46.760' AS DateTime), CAST(N'2023-05-10 07:50:46.760' AS DateTime), CAST(N'2023-05-10 07:50:46.760' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDegreeFarz_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(3 AS Numeric(18, 0)), N'- 2', CAST(N'2023-05-10 07:50:46.760' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:46.760' AS DateTime), CAST(N'2023-05-10 07:50:46.760' AS DateTime), CAST(N'2023-05-10 07:50:46.760' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDegreeFarz_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(4 AS Numeric(18, 0)), N'- 3', CAST(N'2023-05-10 07:50:46.760' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:46.760' AS DateTime), CAST(N'2023-05-10 07:50:46.760' AS DateTime), CAST(N'2023-05-10 07:50:46.760' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDegreeFarz_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(5 AS Numeric(18, 0)), N'- 4', CAST(N'2023-05-10 07:50:46.760' AS DateTime), 0, 0, CAST(N'2023-05-10 07:50:46.760' AS DateTime), CAST(N'2023-05-10 07:50:46.760' AS DateTime), CAST(N'2023-05-10 07:50:46.760' AS DateTime), 0, 0)
SET IDENTITY_INSERT [dbo].[Items_TypesDegreeFarz_Name] OFF

";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"

SET IDENTITY_INSERT [dbo].[Items_TypesDescription_Name] ON 

INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(1 AS Numeric(18, 0)), NULL, CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(2 AS Numeric(18, 0)), N'ارضيات', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(3 AS Numeric(18, 0)), N'بواقي', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(4 AS Numeric(18, 0)), N'تابلوه', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(5 AS Numeric(18, 0)), N'تابلوه 2 بلاطه', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(6 AS Numeric(18, 0)), N'تابلوه 4 بلاطه', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(8 AS Numeric(18, 0)), N'تابلوه 6 بلاطه', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(9 AS Numeric(18, 0)), N'جانب وشاسيه', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(10 AS Numeric(18, 0)), N'حلة', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(11 AS Numeric(18, 0)), N'حوائط', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(12 AS Numeric(18, 0)), N'حوض', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(13 AS Numeric(18, 0)), N'حوض قلب', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(14 AS Numeric(18, 0)), N'خلاط بانيو', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(15 AS Numeric(18, 0)), N'خلاط حوض', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(16 AS Numeric(18, 0)), N'خلاط مطبخ', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(17 AS Numeric(18, 0)), N'ديكور', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(18 AS Numeric(18, 0)), N'ديكور حمام', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(19 AS Numeric(18, 0)), N'ديكور مطبخ', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(20 AS Numeric(18, 0)), N'زوي', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(21 AS Numeric(18, 0)), N'شاور', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(22 AS Numeric(18, 0)), N'شريط', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(23 AS Numeric(18, 0)), N'شطاف', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(24 AS Numeric(18, 0)), N'طقم اكسسوار', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(25 AS Numeric(18, 0)), N'طقم كامل', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(26 AS Numeric(18, 0)), N'غطاء خزان', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(27 AS Numeric(18, 0)), N'غطاء قاعده', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(28 AS Numeric(18, 0)), N'كمبنشن معلق', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(29 AS Numeric(18, 0)), N'كمبنيشن', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(30 AS Numeric(18, 0)), N'لقمه', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(31 AS Numeric(18, 0)), N'ماتيدا', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(32 AS Numeric(18, 0)), N'ماده', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(33 AS Numeric(18, 0)), N'مرايا', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(34 AS Numeric(18, 0)), N'مرحاض عربي', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(35 AS Numeric(18, 0)), N'مروحة', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(36 AS Numeric(18, 0)), N'مسطرة', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(38 AS Numeric(18, 0)), N'ميكس ', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(39 AS Numeric(18, 0)), N'ميكس اسلامي', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(40 AS Numeric(18, 0)), N'ميكس ثري دي', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(41 AS Numeric(18, 0)), N'ميكس حمام', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(42 AS Numeric(18, 0)), N'ميكس مطبخ', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(43 AS Numeric(18, 0)), N'ميكس هندسي', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(44 AS Numeric(18, 0)), N'نصف عامود', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescription_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(45 AS Numeric(18, 0)), N'وحده زجاجيه', CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0, CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), CAST(N'2023-05-10 07:45:05.490' AS DateTime), 0, 0)
SET IDENTITY_INSERT [dbo].[Items_TypesDescription_Name] OFF
";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"
SET IDENTITY_INSERT [dbo].[Items_TypesDescriptionNameOther] ON 

INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(1 AS Numeric(18, 0)), NULL, CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(2 AS Numeric(18, 0)), N' استانلس', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(3 AS Numeric(18, 0)), N' بلاستيك', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(4 AS Numeric(18, 0)), N'116', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(5 AS Numeric(18, 0)), N'5002', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(6 AS Numeric(18, 0)), N'5003', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(7 AS Numeric(18, 0)), N'5005', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(8 AS Numeric(18, 0)), N'6007', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(9 AS Numeric(18, 0)), N'6009', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(10 AS Numeric(18, 0)), N'6016', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(11 AS Numeric(18, 0)), N'6020', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(12 AS Numeric(18, 0)), N'ابيض* اسود', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(13 AS Numeric(18, 0)), N'اثينا', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(14 AS Numeric(18, 0)), N'اجرا', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(15 AS Numeric(18, 0)), N'اركيديا', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(16 AS Numeric(18, 0)), N'ازمير', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(17 AS Numeric(18, 0)), N'اسوره ضفيره', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(18 AS Numeric(18, 0)), N'اصفهان ', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(19 AS Numeric(18, 0)), N'اصفهان ليزر', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(20 AS Numeric(18, 0)), N'اطفال', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(21 AS Numeric(18, 0)), N'اكسسوار', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(22 AS Numeric(18, 0)), N'الاسكا', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(23 AS Numeric(18, 0)), N'الامارات', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(24 AS Numeric(18, 0)), N'البرقوقه', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(25 AS Numeric(18, 0)), N'الشناوي', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(26 AS Numeric(18, 0)), N'الصلاه', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(27 AS Numeric(18, 0)), N'الطفل', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(28 AS Numeric(18, 0)), N'القدس', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(29 AS Numeric(18, 0)), N'القهوه', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(30 AS Numeric(18, 0)), N'الماني', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(31 AS Numeric(18, 0)), N'المجلس العربي', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(32 AS Numeric(18, 0)), N'المركب', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(33 AS Numeric(18, 0)), N'النمر', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(34 AS Numeric(18, 0)), N'اليس', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(35 AS Numeric(18, 0)), N'امبرادور', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(36 AS Numeric(18, 0)), N'اوبرا', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(37 AS Numeric(18, 0)), N'اولد وود', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(38 AS Numeric(18, 0)), N'أيات قرءانيه', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(39 AS Numeric(18, 0)), N'ايطالي', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(40 AS Numeric(18, 0)), N'ايميلي', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(41 AS Numeric(18, 0)), N'بابلز', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(42 AS Numeric(18, 0)), N'برادو', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(43 AS Numeric(18, 0)), N'بريشيا', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(44 AS Numeric(18, 0)), N'بواقي', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(45 AS Numeric(18, 0)), N'بوخارست', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(46 AS Numeric(18, 0)), N'بوسطن', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(47 AS Numeric(18, 0)), N'تارجو', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(48 AS Numeric(18, 0)), N'تربو', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(49 AS Numeric(18, 0)), N'تفلون', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(50 AS Numeric(18, 0)), N'تيمبو', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(51 AS Numeric(18, 0)), N'جرافيتي', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(52 AS Numeric(18, 0)), N'جلاكسي', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(53 AS Numeric(18, 0)), N'جلبة تطويل', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(54 AS Numeric(18, 0)), N'جلوبال ليزر', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(55 AS Numeric(18, 0)), N'حلايب', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(56 AS Numeric(18, 0)), N'حلة 2 عين', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(57 AS Numeric(18, 0)), N'حلة دوران', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(58 AS Numeric(18, 0)), N'حنفية غسالة', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(59 AS Numeric(18, 0)), N'خلاط شعبي دش', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(60 AS Numeric(18, 0)), N'خلاط شعبي سدر', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(61 AS Numeric(18, 0)), N'خلاط شعبي غرز', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(62 AS Numeric(18, 0)), N'دريم', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(63 AS Numeric(18, 0)), N'دريم 103', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(64 AS Numeric(18, 0)), N'دريم 123', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(65 AS Numeric(18, 0)), N'دنمارك', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(66 AS Numeric(18, 0)), N'دهب ليزر', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(67 AS Numeric(18, 0)), N'دودج', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(68 AS Numeric(18, 0)), N'ديكور', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(69 AS Numeric(18, 0)), N'رفال', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(70 AS Numeric(18, 0)), N'روبا', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(71 AS Numeric(18, 0)), N'روكيت', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(72 AS Numeric(18, 0)), N'ريتاج جي', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(73 AS Numeric(18, 0)), N'ريو', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(74 AS Numeric(18, 0)), N'سادة', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(76 AS Numeric(18, 0)), N'سانتوس ', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(77 AS Numeric(18, 0)), N'سانتوس ليزر', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(78 AS Numeric(18, 0)), N'ساندي', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(79 AS Numeric(18, 0)), N'ستانلي', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(80 AS Numeric(18, 0)), N'سليبس', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(81 AS Numeric(18, 0)), N'سمر', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(82 AS Numeric(18, 0)), N'سوستة ', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(83 AS Numeric(18, 0)), N'سيليكون', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(84 AS Numeric(18, 0)), N'شعاع', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(85 AS Numeric(18, 0)), N'شعبي', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(86 AS Numeric(18, 0)), N'صرف حلة', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(87 AS Numeric(18, 0)), N'صرف حوض', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(88 AS Numeric(18, 0)), N'صوفيا', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(89 AS Numeric(18, 0)), N'طاليانو', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(90 AS Numeric(18, 0)), N'طقم خلاط 3 قطعة رويال', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(91 AS Numeric(18, 0)), N'طقم خلاط سوبر', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(92 AS Numeric(18, 0)), N'عامود دش', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(93 AS Numeric(18, 0)), N'عامود فواطة', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(94 AS Numeric(18, 0)), N'غطاء بلاعه', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(95 AS Numeric(18, 0)), N'فاظة', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(96 AS Numeric(18, 0)), N'فالكون', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(97 AS Numeric(18, 0)), N'فاميلي', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(98 AS Numeric(18, 0)), N'فلاك', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(99 AS Numeric(18, 0)), N'فيكتوريا', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(100 AS Numeric(18, 0)), N'قطعة خلاط', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
 
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(101 AS Numeric(18, 0)), N'كاريزما', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(102 AS Numeric(18, 0)), N'كرارة ليزر', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(103 AS Numeric(18, 0)), N'كريستاله', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(104 AS Numeric(18, 0)), N'كليوباترا', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(105 AS Numeric(18, 0)), N'كوع نزل', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(106 AS Numeric(18, 0)), N'كوكي', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(107 AS Numeric(18, 0)), N'لاجونا', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(108 AS Numeric(18, 0)), N'لافينتي عادي', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(109 AS Numeric(18, 0)), N'لافينتي ليزر', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(110 AS Numeric(18, 0)), N'لاكور غاز دكر', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(111 AS Numeric(18, 0)), N'لشبونة', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(112 AS Numeric(18, 0)), N'لوتس', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(113 AS Numeric(18, 0)), N'لوسبندا', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(114 AS Numeric(18, 0)), N'لوسيندا', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(115 AS Numeric(18, 0)), N'ليجاسي', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(116 AS Numeric(18, 0)), N'ماتركس', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(117 AS Numeric(18, 0)), N'ماده', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(118 AS Numeric(18, 0)), N'ماربيلا', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(119 AS Numeric(18, 0)), N'ماكينة', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(120 AS Numeric(18, 0)), N'ماكينة فرست', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(121 AS Numeric(18, 0)), N'مانيجا سوستة', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(122 AS Numeric(18, 0)), N'ماين', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(123 AS Numeric(18, 0)), N'متنوع', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(124 AS Numeric(18, 0)), N'مرايا', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(125 AS Numeric(18, 0)), N'مسطرة دش 1 * 1', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(126 AS Numeric(18, 0)), N'مسمار حوض', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(127 AS Numeric(18, 0)), N'مسمار قاعده', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(128 AS Numeric(18, 0)), N'مسمار قاعده كليوباترا', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(129 AS Numeric(18, 0)), N'مصفى شاور', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(130 AS Numeric(18, 0)), N'ميجان', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(131 AS Numeric(18, 0)), N'ميلانو', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(132 AS Numeric(18, 0)), N'ميمو ليزر', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(133 AS Numeric(18, 0)), N'نجكه', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(134 AS Numeric(18, 0)), N'ندا', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(135 AS Numeric(18, 0)), N'نصف عامود ', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(136 AS Numeric(18, 0)), N'نيو تربو', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(137 AS Numeric(18, 0)), N'همسه', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(138 AS Numeric(18, 0)), N'وحده', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(139 AS Numeric(18, 0)), N'ورده', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(140 AS Numeric(18, 0)), N'ورق شجر', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(141 AS Numeric(18, 0)), N'وش حنفية', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(142 AS Numeric(18, 0)), N'وعاء شامبو', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesDescriptionNameOther] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(143 AS Numeric(18, 0)), N'ويندر', CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0, CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), CAST(N'2023-05-10 07:47:42.227' AS DateTime), 0, 0)
SET IDENTITY_INSERT [dbo].[Items_TypesDescriptionNameOther] OFF

";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"

SET IDENTITY_INSERT [dbo].[Items_TypesFactory_Name] ON 

INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(1 AS Numeric(18, 0)), N'لا يوجد', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(2 AS Numeric(18, 0)), N'اسباني', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(3 AS Numeric(18, 0)), N'الالمانيه', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(4 AS Numeric(18, 0)), N'الشناوي', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(5 AS Numeric(18, 0)), N'الطيب', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(6 AS Numeric(18, 0)), N'المنار', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(7 AS Numeric(18, 0)), N'ايديال', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(8 AS Numeric(18, 0)), N'ايطالي', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(9 AS Numeric(18, 0)), N'ايطاليا', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(10 AS Numeric(18, 0)), N'اينوفا', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(11 AS Numeric(18, 0)), N'بلازا', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(12 AS Numeric(18, 0)), N'بليزا', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(13 AS Numeric(18, 0)), N'بيليزا', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(14 AS Numeric(18, 0)), N'تركي', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(15 AS Numeric(18, 0)), N'جلامور', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(16 AS Numeric(18, 0)), N'روبكس', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(17 AS Numeric(18, 0)), N'روندي', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(18 AS Numeric(18, 0)), N'ريماس', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(19 AS Numeric(18, 0)), N'عروض', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(20 AS Numeric(18, 0)), N'فينيسيا', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(21 AS Numeric(18, 0)), N'كليوباترا', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(22 AS Numeric(18, 0)), N'لابوتيه', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(24 AS Numeric(18, 0)), N'ليسيكو', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(25 AS Numeric(18, 0)), N'مايوركا', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(26 AS Numeric(18, 0)), N'متنوع', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(27 AS Numeric(18, 0)), N'مستورد', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(28 AS Numeric(18, 0)), N'مصري', CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), CAST(N'2023-05-10 07:49:25.160' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesFactory_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(29 AS Numeric(18, 0)), N'أدوات صحية', CAST(N'2023-05-10 08:16:48.033' AS DateTime), 0, 0, CAST(N'2023-05-10 08:16:48.033' AS DateTime), CAST(N'2023-05-10 08:16:48.033' AS DateTime), CAST(N'2023-05-10 08:16:48.033' AS DateTime), 0, 0)
SET IDENTITY_INSERT [dbo].[Items_TypesFactory_Name] OFF

";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"

SET IDENTITY_INSERT [dbo].[Items_TypesSize_Name] ON 

INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(1 AS Numeric(18, 0)), NULL, CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(2 AS Numeric(18, 0)), N'10*10', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(3 AS Numeric(18, 0)), N'11 سم', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(4 AS Numeric(18, 0)), N'130*130', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(5 AS Numeric(18, 0)), N'15*15', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(6 AS Numeric(18, 0)), N'15*60', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(7 AS Numeric(18, 0)), N'170*70', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(8 AS Numeric(18, 0)), N'2*1 فرداني', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(9 AS Numeric(18, 0)), N'2*60', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(10 AS Numeric(18, 0)), N'2.8 سم', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(11 AS Numeric(18, 0)), N'20*20', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(12 AS Numeric(18, 0)), N'20*50', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(13 AS Numeric(18, 0)), N'25*50', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(14 AS Numeric(18, 0)), N'3*60', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(15 AS Numeric(18, 0)), N'30*60', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(16 AS Numeric(18, 0)), N'30.5*60.5', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(17 AS Numeric(18, 0)), N'35*35', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(18 AS Numeric(18, 0)), N'35*40', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(19 AS Numeric(18, 0)), N'40*45', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(20 AS Numeric(18, 0)), N'43*57', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(21 AS Numeric(18, 0)), N'45*38', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(22 AS Numeric(18, 0)), N'45*70', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(23 AS Numeric(18, 0)), N'49*65', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(24 AS Numeric(18, 0)), N'49*84', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(25 AS Numeric(18, 0)), N'5*60', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(26 AS Numeric(18, 0)), N'50*50', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(27 AS Numeric(18, 0)), N'50*80', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(28 AS Numeric(18, 0)), N'52*52', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(29 AS Numeric(18, 0)), N'60*60', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(30 AS Numeric(18, 0)), N'61*61', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(31 AS Numeric(18, 0)), N'زجاج', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(32 AS Numeric(18, 0)), N'شعبي', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(33 AS Numeric(18, 0)), N'صحي', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(34 AS Numeric(18, 0)), N'غطاء كمبنيشن', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
INSERT [dbo].[Items_TypesSize_Name] ([Id], [Type_Name], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id], [ProcessNo]) VALUES (CAST(35 AS Numeric(18, 0)), N'مسطره', CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0, CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), CAST(N'2023-05-10 07:49:52.863' AS DateTime), 0, 0)
SET IDENTITY_INSERT [dbo].[Items_TypesSize_Name] OFF

";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
            mysql = @"

ALTER TABLE [dbo].[Items_Bodies_Name] ADD  DEFAULT (getdate()) FOR [OperationDate]
";
            try
            {
                opp.insert_me(mysql);
            }
            catch { }
        }

    }
}

