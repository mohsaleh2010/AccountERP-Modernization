// AccountERP.Models/Menus/SysConfigMenus.cs
using AccountModelsLib.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using AccountModelsLib;
namespace AccountModelsLib.Models
{



    public class Account_BrancheCLS
    {
        
        List<Account_BranchesVM> account_BranchesLstVM = new   List<Account_BranchesVM>  ();
        public DataTable dt_Branch;
        bool Branch_Enabled = true;
        operations opp;
        public Account_BrancheCLS(  operations _opp)
        {
            opp = _opp;
        }
        public Account_BrancheCLS()
        {
            opp = new operations();
        }

        public List<Account_BranchesVM> Getaccount_BranchesLst()
        {
            if (!dt_Branch.Columns.Contains("USER_DETAILES_Id"))
            {
                // يمكنك تحديد نوع البيانات للعمود (مثلاً int أو long) حسب رغبتك
                dt_Branch.Columns.Add("USER_DETAILES_Id", typeof(int));
            }
            //using (var reader = dt_Branch.CreateDataReader())
            //{
            //    var parser = reader.GetRowParser<Account_Branches>();

            //    while (reader.Read())
            //    {
            //        account_BranchesLst.Add(parser(reader));
            //    }
            //}

            foreach (DataRow row in dt_Branch.Rows)
            {
                var branch = new Account_BranchesVM();

                // نمر على كل الخصائص الموجودة في الموديل الخاص بك
                foreach (var prop in typeof(Account_BranchesVM).GetProperties())
                {
                    // نتحقق أولاً إذا كان الجدول يحتوي على عمود بنفس اسم الخاصية، وأن القيمة ليست NULL
                    if (dt_Branch.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                    {
                        // تحويل القيمة لنوع البيانات الصحيح (خصوصاً الأنواع الـ Nullable مثل ?long أو ?int)
                        var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        var safeValue = Convert.ChangeType(row[prop.Name], targetType);

                        prop.SetValue(branch, safeValue, null);
                    }
                }

                account_BranchesLstVM.Add(branch);
            }
            return account_BranchesLstVM ;
        }
        public DataTable Get_Data(bool Add_All)
        {
            string mysql = "";

            mysql = " Select * from Account_Branches   ORDER BY Branch_Name   ";

            dt_Branch= Get_Data_(mysql, Add_All);
            //Getaccount_BranchesLst();
            return dt_Branch;

        }
        public DataTable Get_DataUSER_DETAILES_Branches_Permission(bool Add_All, long USER_DETAILES_Id)
        {
            string mysql = "";

            mysql = @" SELECT     Account_Branches.*, USER_DETAILES_Branches_Permission.USER_DETAILES_Id
FROM         Account_Branches INNER JOIN
                      USER_DETAILES_Branches_Permission ON Account_Branches.Id = USER_DETAILES_Branches_Permission.Branch_Id ";
            mysql = mysql + " WHERE     (USER_DETAILES_Branches_Permission.USER_DETAILES_Id =  " + USER_DETAILES_Id + ")   ";

            if (Common.Is_User_Admin == 1)
            {
                mysql = " Select * from Account_Branches      ";

            }

            mysql= mysql + " ORDER BY Account_Branches.Id   ";  
            dt_Branch = Get_Data_(mysql, Add_All);
             
            return dt_Branch;



        }
        public DataTable  Get_Data(bool Add_All, bool Branch_Enabled_)
        {
            string mysql = "";
            Branch_Enabled = Branch_Enabled_;
            mysql = " Select * from Account_Branches   ORDER BY Branch_Name   ";

             


            dt_Branch = Get_Data_(mysql, Add_All);
            
            return dt_Branch;


        }
        //public DataTable  Get_Data(long Branch_Id)
        //{
        //    string mysql = "";

        //    mysql = " Select * from Account_Branches   Where Id=" + Branch_Id;

           
        //    dt_Branch = Get_Data_(mysql, false );
            
        //    return dt_Branch;



        //}
        public static DataTable Get_DataAccount_Branches()
        {
            string mysql = "";

            mysql = " Select * from Account_Branches   ORDER BY Branch_Name   ";



             AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt_Branch = new DataTable();

            dt_Branch = opp.return_DataTable(mysql);
           
            
           
            return dt_Branch;


        }
        public static DataTable Get_USER_DETAILES_Branches_Permission(long USER_DETAILES_Id)
        {
            string mysql = "";

            mysql = " Select * from USER_DETAILES_Branches_Permission   Where USER_DETAILES_Id=" + USER_DETAILES_Id;



            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt_Branch = new DataTable();

            dt_Branch = opp.return_DataTable(mysql);

            return dt_Branch;


        }
        private DataTable Get_Data_(string mysql, bool Add_All)
        {

            //AccountModelsLib.operations opp = new AccountModelsLib.operations();
            dt_Branch = new DataTable();

            dt_Branch = opp.return_DataTable(mysql);
            return dt_Branch;
        }
        /*
        private void Get_Data_(string mysql, bool Add_All)
        {

            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            dt_Branch = new DataTable();
            this.cmbBranch.DataSource = null;
            dt_Branch = opp.return_DataTable(mysql);
            if (dt_Branch.Rows.Count > 0)
            {

                cmbBranch.Enabled = Branch_Enabled;


                if (Add_All)
                {
                    DataRow dr = dt_Branch.NewRow();
                    dr["Branch_Name"] = "كل الفروع";
                    dr["Id"] = 0;
                    dt_Branch.Rows.InsertAt(dr, 0);
                }

                this.cmbBranch.DataSource = dt_Branch;
                cmbBranch.DisplayMember = "Branch_Name";
                cmbBranch.ValueMember = "Id";

                opp = null;
                try
                {
                    DataRow[] dr = dt_Branch.Select("Id=" + AccountCommonDataLIB.Common.Branch_Id);
                    //dt_Branch
                    //DataView dv;
                    //dv = dt_Branch.DefaultView;

                    if (dr.Length > 0)
                    {
                        cmbBranch.SelectedValue = dr[0]["Id"].ToString();
                        cmbBranch.Text = dr[0]["Branch_Name"].ToString();

                    }
                }
                catch { }

            }
            else
            {
                DataRow dr = dt_Branch.NewRow();
                dr["Branch_Name"] = "لا يوجد";
                dr["Id"] = -1;
                dt_Branch.Rows.InsertAt(dr, 0);

                this.cmbBranch.DataSource = dt_Branch;
                cmbBranch.DisplayMember = "Branch_Name";
                cmbBranch.ValueMember = "Id";
            }
        }
        */
    }
}