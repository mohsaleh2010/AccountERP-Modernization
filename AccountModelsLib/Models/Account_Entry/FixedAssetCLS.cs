using IdGen;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
 
using System.Drawing;
   
using System.IO;
using System.Linq;
using System.Text;

namespace AccountModelsLib.Models
{
    public class FixedAssetCLS 
    {
        public FixedAssetData fxd = new FixedAssetData();
        public FixedAssetCLS()
        { }
        ////public long Id { get; set; }
        ////public int Branch_Id { get; set; }
        ////public string Code { get; set; } = "";
        ////public string Name_Ar { get; set; }
        ////public string Name_En { get; set; } = "";
        ////public DateTime? Purchase_Date { get; set; }
        ////public int Account_Ehlak_Status { get; set; } = 0;
        ////public string Account_Ehlak_Status_Str { get; set; } = "";
        ////public long Emp_Id { get; set; } = 0;
        ////public DateTime? Emp_Receive_Date { get; set; }
        ////public string Model { get; set; } = "";
        ////public string Industry_Factory { get; set; } = "";
        ////public string Fixed_Serial_No { get; set; } = "";
        ////public string Location { get; set; } = "";
        ////public DateTime? Factory_warrantyFinal_Date { get; set; }
        ////public string Supplier_Name { get; set; } = "";
        ////public string Factory_warranty_Details { get; set; } = "";
        ////public int Account_Ehlak_Type { get; set; }
        ////public string Account_Ehlak_Type_Str { get; set; } = "";
        ////public decimal Main_Value { get; set; } = 0;
        ////public decimal Accumulated_Depreciation_Value { get; set; }
        ////public decimal Rate_of_depreciation { get; set; }
        ////public decimal Book_value { get; set; } = 0;
        ////public DateTime? beginning_service_Date { get; set; }
        ////public decimal Price_scrap_Value { get; set; }
        ////public decimal Last_Modified_Value { get; set; }
        ////public decimal Default_Age { get; set; }
        ////public long SubAccount_Id { get; set; }
        ////// public string  { get; set; } = "";
        //public string Account_Code { get; set; } = "";
        public Submain_AccountCLS sa = new Submain_AccountCLS();

        public void Get_FixedAssetData(ref operations opp, long Id)
        {
            fxd = opp.db.fixedAssetData.AsNoTracking().Where(f => f.Id == Id).FirstOrDefault();
            //string mysql = " Select * from FixedAssetData Where Id=" + Id;
            //DataTable dt = opp.return_DataTable(mysql);

            //if (dt.Rows.Count > 0)
            //{
            //    ReadData(dt, 0);
            //}
        }
        public DataTable Get_FixedAsset(long SubAccount_Id)
        {
            DataTable dt = new DataTable();
            try
            {
                string mysql;

                mysql = @" SELECT DISTINCT FixedAssetData.*, Submain_Account.*
FROM            Submain_Account INNER JOIN
                         FixedAssetData ON Submain_Account.Id = FixedAssetData.SubAccount_Id
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


        public DataTable Get_FixedAsset_Submain_Account(long SubAccount_Id)
        {
            DataTable dt = new DataTable();
            try
            {
                string mysql;

                mysql = @" SELECT    *
FROM            Submain_Account  
WHERE        ( SubAccount_Id =  " + SubAccount_Id + ")  ";


                operations opp = new operations();
                dt = opp.return_DataTable(mysql);

                return dt;
            }
            catch (Exception ee)
            {



            }
            return dt;
        }


        public DataTable GetFixedAsset(ref operations opp, long SubAccount_Id)
        {
            string mysql = "SELECT * FROM FixedAssetData where SubAccount_Id =" + SubAccount_Id;
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public void GetFixedAssetData(ref operations opp, long SubAccount_Id)
        {
            fxd = opp.db.fixedAssetData.AsNoTracking().Where(f => f.SubAccount_Id == SubAccount_Id).FirstOrDefault();
            //string mysql = "SELECT * FROM FixedAsset where SubAccount_Id =" + SubAccount_Id;
            //DataTable dt = opp.return_DataTable(mysql);
            //if (dt.Rows.Count > 0)
            //{
            //    ReadData(dt, 0);
            //}

        }
        void ReadData(DataTable dt, int index)
        {
            //Id = long.Parse(dt.Rows[index]["Id"].ToString());
            //Branch_Id = int.Parse(dt.Rows[index]["Branch_Id"].ToString());
            //Code = dt.Rows[index]["Code"].ToString();
            //Name_Ar = dt.Rows[index]["Name_Ar"].ToString();
            //Name_En = dt.Rows[index]["Name_En"].ToString();

            //Account_Ehlak_Status = int.Parse(dt.Rows[index]["Account_Ehlak_Status"].ToString());
            //Account_Ehlak_Status_Str = dt.Rows[index]["Account_Ehlak_Status_Str"].ToString();
            //Emp_Id = long.Parse(dt.Rows[index]["Emp_Id"].ToString());

            //Model = dt.Rows[index]["Model"].ToString();
            //Industry_Factory = dt.Rows[index]["Industry_Factory"].ToString();
            //Fixed_Serial_No = dt.Rows[index]["Fixed_Serial_No"].ToString();
            //Location = dt.Rows[index]["Location"].ToString();
            //Supplier_Name = dt.Rows[index]["Supplier_Name"].ToString();
            //Factory_warranty_Details = dt.Rows[index]["Factory_warranty_Details"].ToString();
            //Account_Ehlak_Type = int.Parse(dt.Rows[index]["Account_Ehlak_Type"].ToString());
            //Account_Ehlak_Type_Str = dt.Rows[index]["Account_Ehlak_Type_Str"].ToString();
            //Main_Value = decimal.Parse(dt.Rows[index]["Main_Value"].ToString());
            //Accumulated_Depreciation_Value = decimal.Parse(dt.Rows[index]["Accumulated_Depreciation_Value"].ToString());
            //Rate_of_depreciation = decimal.Parse(dt.Rows[index]["Rate_of_depreciation"].ToString());
            //Book_value = decimal.Parse(dt.Rows[index]["Book_value"].ToString());


            //Price_scrap_Value = decimal.Parse(dt.Rows[index]["Price_scrap_Value"].ToString());
            //Last_Modified_Value = decimal.Parse(dt.Rows[index]["Last_Modified_Value"].ToString());
            //Default_Age = decimal.Parse(dt.Rows[index]["Default_Age"].ToString());
            //SubAccount_Id = long.Parse(dt.Rows[index]["SubAccount_Id"].ToString());
            //Account_Code = dt.Rows[index]["Account_Code"].ToString();

            ////beginning_service_Date = DateTime.Parse(dt.Rows[index]["beginning_service_Date"].ToString());
            ////Purchase_Date = DateTime.Parse(dt.Rows[index]["Purchase_Date"].ToString());
            ////Emp_Receive_Date = DateTime.Parse(dt.Rows[index]["Emp_Receive_Date"].ToString());
            ////Factory_warrantyFinal_Date = DateTime.Parse(dt.Rows[index]["Factory_warrantyFinal_Date"].ToString());


            //beginning_service_Date = Common.GetColumnValueDateTimeNullable(dt.Rows[index]["beginning_service_Date"] );
            //Purchase_Date = Common.GetColumnValueDateTimeNullable(dt.Rows[index]["Purchase_Date"] );
            //Emp_Receive_Date = Common.GetColumnValueDateTimeNullable(dt.Rows[index]["Emp_Receive_Date"] );
            //Factory_warrantyFinal_Date = Common.GetColumnValueDateTimeNullable(dt.Rows[index]["Factory_warrantyFinal_Date"] );


        }
        // 🟢 دالتا بحث جديدتان (تقنيتان فقط): تقابلان حرفياً استعلامَى Connection4It.Employees_List.
        // Get_Employees_Data_Branch وConnection4It.Account_Ehlak_Type.Get_Data (عنصرا تحكم WinForms فقط)
        // المستخدَمين فى FixedAssetsUSCTRL الأصلية لتعبئة قائمتَى الموظف المؤتمن على الأصل وطريقة الإهلاك.
        public DataTable Get_EmployeesList(long Branch_Id)
        {
            operations opp = new operations();
            string mysql = "Select * from Employee where Active=1 ";
            if (Branch_Id > 0)
            {
                mysql = mysql + " and Branch_Id=" + Branch_Id + " OR Branch_Id=-1";
            }
            mysql = mysql + "  ORDER BY name ";
            return opp.return_DataTable(mysql);
        }

        public DataTable Get_DepreciationTypes()
        {
            operations opp = new operations();
            string mysql = " Select * from Account_Ehlak_Type ";
            return opp.return_DataTable(mysql);
        }

        public DataTable Get_FixedAssetData_ImagesStore(long FixedAssetData_Id)
        {
            DataTable dt = new DataTable();
            try
            {
                string mysql;

                mysql = @" SELECT  * FROM   FixedAssetData_ImagesStore WHERE  (FixedAssetData_Id =  " + FixedAssetData_Id + ")  ";


                operations opp = new operations();
                dt = opp.return_DataTable(mysql);

                return dt;
            }
            catch (Exception ee)
            {



            }
            return dt;

        }
        public void InsertFixedAssetFromForm(ref int Excp, ref string Msg, ref long New_Submain_Account_Id, ref long FixedAssetData_Id, string Account_Follow_Id, long Main_Account_Id)
        {



            //operations opp = new operations(1);
            //int Currency_Id = 1;
            //string Account_Code = "";
            //Sub_Account acc = new Sub_Account();
            //int Account_Type_Value = Common.Assoul_Sabta_Account_Type;
            //New_Submain_Account_Id = -1;
            //acc.Account_Follows = int.Parse(Account_Follow_Id);
            //acc.Main_Account_Id = Main_Account_Id;
            //acc.Add_AccountSupplierCustomerOther(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, ref Account_Code, Account_Type_Value, Name_Ar, Branch_Id, Currency_Id);



             //long Id = Common.GETAutoNumber(ref opp, "FixedAssetData", "Id", Branch_Id);

            //FixedAssetData_Id = Id;

            //string mysql = " ";
            //mysql = "INSERT INTO FixedAssetData(Id,Branch_Id,User_Id, Code, Name_Ar, Name_En, Purchase_Date, Account_Ehlak_Status, Account_Ehlak_Status_Str, Emp_Id, Emp_Receive_Date, Model, Industry_Factory, Fixed_Serial_No, Location, Factory_warrantyFinal_Date, Supplier_Name, Factory_warranty_Details, Account_Ehlak_Type, Account_Ehlak_Type_Str, Main_Value, Accumulated_Depreciation_Value, Rate_of_depreciation, Book_value, beginning_service_Date, Price_scrap_Value, Last_Modified_Value, Default_Age,SubAccount_Id,Account_Code) ";
            //mysql = mysql + " VALUES(@Id,@Branch_Id,@User_Id,@Code,@Name_Ar,@Name_En,@Purchase_Date,@Account_Ehlak_Status,@Account_Ehlak_Status_Str,@Emp_Id,@Emp_Receive_Date,@Model,@Industry_Factory,@Fixed_Serial_No,@Location,@Factory_warrantyFinal_Date,@Supplier_Name,@Factory_warranty_Details,@Account_Ehlak_Type,@Account_Ehlak_Type_Str,@Main_Value,@Accumulated_Depreciation_Value,@Rate_of_depreciation,@Book_value,@beginning_service_Date,@Price_scrap_Value,@Last_Modified_Value,@Default_Age,@SubAccount_Id,@Account_Code)";
            //try
            //{
            //    opp.cmd.Parameters.AddWithValue("@Id", Id);
            //    opp.cmd.Parameters.AddWithValue("@Code", Code);
            //    opp.cmd.Parameters.AddWithValue("@Branch_Id", Branch_Id);
            //    opp.cmd.Parameters.AddWithValue("@User_Id", Common.User_Id);
            //    opp.cmd.Parameters.AddWithValue("@Name_Ar", Name_Ar);
            //    opp.cmd.Parameters.AddWithValue("@Name_En", Name_En);
            //    opp.cmd.Parameters.AddWithValue("@Purchase_Date", Common.GetColumnValueDateTimeNullable(Purchase_Date));
            //    opp.cmd.Parameters.AddWithValue("@Account_Ehlak_Status", Account_Ehlak_Status);
            //    opp.cmd.Parameters.AddWithValue("@Account_Ehlak_Status_Str", Account_Ehlak_Status_Str);
            //    opp.cmd.Parameters.AddWithValue("@Emp_Id", Emp_Id);
            //    opp.cmd.Parameters.AddWithValue("@Emp_Receive_Date", Common.GetColumnValueDateTimeNullable(Emp_Receive_Date));
            //    opp.cmd.Parameters.AddWithValue("@Model", Model);
            //    opp.cmd.Parameters.AddWithValue("@Industry_Factory", Industry_Factory);
            //    opp.cmd.Parameters.AddWithValue("@Fixed_Serial_No", Fixed_Serial_No);
            //    opp.cmd.Parameters.AddWithValue("@Location", Location);
            //    opp.cmd.Parameters.AddWithValue("@Factory_warrantyFinal_Date", Common.GetColumnValueDateTimeNullable(Factory_warrantyFinal_Date));
            //    opp.cmd.Parameters.AddWithValue("@Supplier_Name", Supplier_Name);
            //    opp.cmd.Parameters.AddWithValue("@Factory_warranty_Details", Factory_warranty_Details);
            //    opp.cmd.Parameters.AddWithValue("@Account_Ehlak_Type", Account_Ehlak_Type);
            //    opp.cmd.Parameters.AddWithValue("@Account_Ehlak_Type_Str", Account_Ehlak_Type_Str);
            //    opp.cmd.Parameters.AddWithValue("@Main_Value", Main_Value);
            //    opp.cmd.Parameters.AddWithValue("@Accumulated_Depreciation_Value", Accumulated_Depreciation_Value);
            //    opp.cmd.Parameters.AddWithValue("@Rate_of_depreciation", Rate_of_depreciation);
            //    opp.cmd.Parameters.AddWithValue("@Book_value", Book_value);
            //    opp.cmd.Parameters.AddWithValue("@beginning_service_Date", Common.GetColumnValueDateTimeNullable(beginning_service_Date));
            //    opp.cmd.Parameters.AddWithValue("@Price_scrap_Value", Price_scrap_Value);
            //    opp.cmd.Parameters.AddWithValue("@Last_Modified_Value", Last_Modified_Value);
            //    opp.cmd.Parameters.AddWithValue("@Default_Age", Default_Age);
            //    opp.cmd.Parameters.AddWithValue("@SubAccount_Id", New_Submain_Account_Id);
            //    opp.cmd.Parameters.AddWithValue("@Account_Code", Account_Code);
            //    opp.insert_me(mysql);

            //    opp.cmd.Parameters.Clear();
            //    opp.Commit();
            //    Excp = 0;

            //}
            //catch (Exception ee)
            //{
            //    Excp = 1;
            //    Msg = ee.Message;
            //    opp.Rollback();
            //}

        }
        public void InsertFixedAsset(ref operations opp)
        {
            opp.db.fixedAssetData.Add(fxd);
            opp.db.SaveChanges();

            

        }

        public long InsertFixedAsset(ref int excp, ref string msg, ref operations opp, Submain_Account sa_)
        {
            if (Common.Use_Auto_Identity_Column == 0)
            {
                fxd.Id = sa_.Id;

            }
            fxd.Name_Ar = sa_.SubAccount_Name;
            fxd.SubAccount_Id = sa_.Id;
            fxd.Account_Code = sa_.Account_Code;
            fxd.Branch_Id = sa_.Branch_Id;
          
            opp.db.fixedAssetData.Add(fxd);
            opp.db.SaveChanges();

            return fxd.Id;
        }

        public void Update_NewFixedAsset(ref int Excp, ref string Msg)
        {
            operations opp = new operations(1);

            opp.db.Entry(fxd).State = EntityState.Deleted;
            opp.db.SaveChanges();

            string mysql = " ";
            //mysql = "Update   FixedAssetData Set Code=@Code , Name_Ar=@Name_Ar , Name_En=@Name_En , Purchase_Date=@Purchase_Date , Account_Ehlak_Status=@Account_Ehlak_Status , Account_Ehlak_Status_Str=@Account_Ehlak_Status_Str , Emp_Id=@Emp_Id , Emp_Receive_Date=@Emp_Receive_Date , Model=@Model , Industry_Factory=@Industry_Factory , Fixed_Serial_No=@Fixed_Serial_No , Location=@Location , Factory_warrantyFinal_Date=@Factory_warrantyFinal_Date , Supplier_Name=@Supplier_Name , Factory_warranty_Details=@Factory_warranty_Details , Account_Ehlak_Type=@Account_Ehlak_Type , Account_Ehlak_Type_Str=@Account_Ehlak_Type_Str , Main_Value=@Main_Value , Accumulated_Depreciation_Value=@Accumulated_Depreciation_Value , Rate_of_depreciation=@Rate_of_depreciation , Book_value=@Book_value , beginning_service_Date=@beginning_service_Date , Price_scrap_Value=@Price_scrap_Value , Last_Modified_Value=@Last_Modified_Value , Default_Age=@Default_Age ,  Account_Code=@Account_Code ";
            //mysql = mysql + " Where Id=@Id";
            try
            {
                //    opp.cmd.Parameters.AddWithValue("@Id", fxd.Id);
                //    opp.cmd.Parameters.AddWithValue("@Code", fxd.Code);

                //    opp.cmd.Parameters.AddWithValue("@Name_Ar", fxd.Name_Ar);
                //    opp.cmd.Parameters.AddWithValue("@Name_En", fxd.Name_En);
                //    opp.cmd.Parameters.AddWithValue("@Purchase_Date", fxd.Purchase_Date);
                //    opp.cmd.Parameters.AddWithValue("@Account_Ehlak_Status", fxd.Account_Ehlak_Status);
                //    opp.cmd.Parameters.AddWithValue("@Account_Ehlak_Status_Str", fxd.Account_Ehlak_Status_Str);
                //    opp.cmd.Parameters.AddWithValue("@Emp_Id", fxd.Emp_Id);
                //    opp.cmd.Parameters.AddWithValue("@Emp_Receive_Date", fxd.Emp_Receive_Date);
                //    opp.cmd.Parameters.AddWithValue("@Model", fxd.Model);
                //    opp.cmd.Parameters.AddWithValue("@Industry_Factory", fxd.Industry_Factory);
                //    opp.cmd.Parameters.AddWithValue("@Fixed_Serial_No", fxd.Fixed_Serial_No);
                //    opp.cmd.Parameters.AddWithValue("@Location", fxd.Location);
                //    opp.cmd.Parameters.AddWithValue("@Factory_warrantyFinal_Date", fxd.Factory_warrantyFinal_Date);
                //    opp.cmd.Parameters.AddWithValue("@Supplier_Name", fxd.Supplier_Name);
                //    opp.cmd.Parameters.AddWithValue("@Factory_warranty_Details", fxd.Factory_warranty_Details);
                //    opp.cmd.Parameters.AddWithValue("@Account_Ehlak_Type", fxd.Account_Ehlak_Type);
                //    opp.cmd.Parameters.AddWithValue("@Account_Ehlak_Type_Str", fxd.Account_Ehlak_Type_Str);
                //    opp.cmd.Parameters.AddWithValue("@Main_Value", fxd.Main_Value);
                //    opp.cmd.Parameters.AddWithValue("@Accumulated_Depreciation_Value", fxd.Accumulated_Depreciation_Value);
                //    opp.cmd.Parameters.AddWithValue("@Rate_of_depreciation", fxd.Rate_of_depreciation);
                //    opp.cmd.Parameters.AddWithValue("@Book_value", fxd.Book_value);
                //    opp.cmd.Parameters.AddWithValue("@beginning_service_Date", fxd.beginning_service_Date);
                //    opp.cmd.Parameters.AddWithValue("@Price_scrap_Value", fxd.Price_scrap_Value);
                //    opp.cmd.Parameters.AddWithValue("@Last_Modified_Value", fxd.Last_Modified_Value);
                //    opp.cmd.Parameters.AddWithValue("@Default_Age", fxd.Default_Age);
                //    opp.cmd.Parameters.AddWithValue("@SubAccount_Id", fxd.SubAccount_Id);
                //    opp.cmd.Parameters.AddWithValue("@Account_Code", fxd.Account_Code);
                //    opp.insert_me(mysql);

                //    opp.cmd.Parameters.Clear();

                Submain_AccountCLS sa = new Submain_AccountCLS();
                sa.UpdateSubmain_Account(ref opp, fxd.Name_Ar, fxd.SubAccount_Id??0);

                opp.Commit();
                Excp = 0;

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
                opp.Rollback();
            }

        }
        // 🟢 دالة جديدة (Blazor فقط - بموافقة صريحة من المستخدم): Update_NewFixedAsset الأصلية (المستخدمة
        // فعلاً من FixedAssetsUSCTRL.UpdateFixedAsset فى الشاشة الأصلية) تحذف فعلياً صف FixedAssetData
        // بالكامل (opp.db.Entry(fxd).State = EntityState.Deleted ثم SaveChanges) ثم تُحدِّث فقط إسم الحساب -
        // كود الـ UPDATE الفعلى لباقى الحقول موجود فى الأصل لكنه مُعلَّق بالكامل (كود ميت لم يُفعَّل قط)، وقد
        // تأكَّدنا من هذا السلوك حياً ضد قاعدة التطوير أثناء اختبار هذه الشاشة (زر "تعديل" الأصلى يحذف بيانات
        // الأصل فعلياً بدل تحديثها). بطلب المستخدم صراحة: هذا خلل حقيقى فى الشاشة الأصلية لا يُصلَح هناك (خارج
        // نطاق هذا الترحيل) لكن Blazor لا يجب أن يفقد بيانات المستخدم عند كل تعديل، فأُضيفت هذه الدالة الجديدة
        // التى تُنفِّذ فعلياً استعلام الـ UPDATE الكامل الموجود أصلاً (معلَّقاً) فى Update_NewFixedAsset -
        // نفس الأعمدة بالضبط، فقط مُفعَّل هذه المرة - ثم نفس مزامنة إسم الحساب الأصلية.
        public void Update_FixedAssetData_Full(ref int Excp, ref string Msg)
        {
            operations opp = new operations(1);
            try
            {
                string mysql = @"Update FixedAssetData Set Code=@Code , Name_Ar=@Name_Ar , Name_En=@Name_En , Purchase_Date=@Purchase_Date , Account_Ehlak_Status=@Account_Ehlak_Status , Account_Ehlak_Status_Str=@Account_Ehlak_Status_Str , Emp_Id=@Emp_Id , Emp_Receive_Date=@Emp_Receive_Date , Model=@Model , Industry_Factory=@Industry_Factory , Fixed_Serial_No=@Fixed_Serial_No , Location=@Location , Factory_warrantyFinal_Date=@Factory_warrantyFinal_Date , Supplier_Name=@Supplier_Name , Factory_warranty_Details=@Factory_warranty_Details , Account_Ehlak_Type=@Account_Ehlak_Type , Account_Ehlak_Type_Str=@Account_Ehlak_Type_Str , Main_Value=@Main_Value , Accumulated_Depreciation_Value=@Accumulated_Depreciation_Value , Rate_of_depreciation=@Rate_of_depreciation , Book_value=@Book_value , beginning_service_Date=@beginning_service_Date , Price_scrap_Value=@Price_scrap_Value , Last_Modified_Value=@Last_Modified_Value , Default_Age=@Default_Age
Where Id=@Id";

                opp.cmd.Parameters.Add(new SqlParameter("@Id", fxd.Id));
                opp.cmd.Parameters.Add(new SqlParameter("@Code", (object)fxd.Code ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Name_Ar", (object)fxd.Name_Ar ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Name_En", (object)fxd.Name_En ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Purchase_Date", (object)fxd.Purchase_Date ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Account_Ehlak_Status", (object)fxd.Account_Ehlak_Status ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Account_Ehlak_Status_Str", (object)fxd.Account_Ehlak_Status_Str ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Emp_Id", (object)fxd.Emp_Id ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Emp_Receive_Date", (object)fxd.Emp_Receive_Date ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Model", (object)fxd.Model ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Industry_Factory", (object)fxd.Industry_Factory ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Fixed_Serial_No", (object)fxd.Fixed_Serial_No ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Location", (object)fxd.Location ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Factory_warrantyFinal_Date", (object)fxd.Factory_warrantyFinal_Date ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Supplier_Name", (object)fxd.Supplier_Name ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Factory_warranty_Details", (object)fxd.Factory_warranty_Details ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Account_Ehlak_Type", (object)fxd.Account_Ehlak_Type ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Account_Ehlak_Type_Str", (object)fxd.Account_Ehlak_Type_Str ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Main_Value", (object)fxd.Main_Value ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Accumulated_Depreciation_Value", (object)fxd.Accumulated_Depreciation_Value ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Rate_of_depreciation", (object)fxd.Rate_of_depreciation ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Book_value", (object)fxd.Book_value ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@beginning_service_Date", (object)fxd.beginning_service_Date ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Price_scrap_Value", (object)fxd.Price_scrap_Value ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Last_Modified_Value", (object)fxd.Last_Modified_Value ?? DBNull.Value));
                opp.cmd.Parameters.Add(new SqlParameter("@Default_Age", (object)fxd.Default_Age ?? DBNull.Value));

                opp.insert_me(mysql);
                opp.cmd.Parameters.Clear();

                Submain_AccountCLS sa = new Submain_AccountCLS();
                sa.UpdateSubmain_Account(ref opp, fxd.Name_Ar, fxd.SubAccount_Id ?? 0);

                opp.Commit();
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
                opp.Rollback();
            }
        }

        public long  UpdateFixedAsset(ref operations opp)
        {
            opp.db.Entry(fxd).State = EntityState.Deleted;
            opp.db.SaveChanges();

            return fxd.Id;

        }

        public void SaveImage(ref int Excp, string ImagePath, long FixedAssetData_Id, string Desc)
        {
            operations opp = new operations();
            SaveImage(ref Excp, ref opp, ImagePath, FixedAssetData_Id, Desc);
        }
        public void SaveImage(ref int Excp, ref operations opp, string ImagePath, long FixedAssetData_Id, string Desc)
        {
            try
            {
                ////Read Image Bytes into a byte array
                byte[] imageData = ReadFile(ImagePath);
                SaveImageData(ref Excp, ref opp, imageData, FixedAssetData_Id, Desc, ImagePath);
            }
            catch (Exception ee)
            {
                Excp = 1;
            }
        }

        // 🟢 دالة جديدة (تقنية فقط): نفس SaveImage بالضبط، لكن تستقبل بيانات الصورة كـ byte[] مباشرة بدل
        // مسار ملف محلى على القرص - Blazor (خادم أو WebAssembly) لا يملك "مسار ملف محلى" لِما يرفعه المستخدم
        // من متصفحه (بعكس WinForms)، فالمستخدم يرفع الملف مباشرة إلى الذاكرة (MudFileUpload/IBrowserFile)
        // ثم يُمرَّر byte[] الناتج هنا مباشرة - نفس جدول FixedAssetData_ImagesStore ونفس الأعمدة بالضبط.
        public void SaveImage(ref int Excp, ref operations opp, byte[] imageData, long FixedAssetData_Id, string Desc, string OriginalFileName = "")
        {
            SaveImageData(ref Excp, ref opp, imageData, FixedAssetData_Id, Desc, OriginalFileName);
        }

        private void SaveImageData(ref int Excp, ref operations opp, byte[] imageData, long FixedAssetData_Id, string Desc, string OriginalPath)
        {
            try
            {
                string mysql = "insert into FixedAssetData_ImagesStore (FixedAssetData_Id,Description,OriginalPath,ImageData) values(@FixedAssetData_Id,@Description,@OriginalPath, @ImageData)";

                opp.cmd.Parameters.Add(new SqlParameter("@OriginalPath", (object)OriginalPath));
                opp.cmd.Parameters.Add(new SqlParameter("@ImageData", (object)imageData));
                opp.cmd.Parameters.Add(new SqlParameter("@FixedAssetData_Id", (object)FixedAssetData_Id));
                opp.cmd.Parameters.Add(new SqlParameter("@Description", (object)Desc));

                opp.insert_me(mysql);

                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
            }
        }
        public void Delete_FixedAssetData_ImagesStore(string ImageId)
        {
            operations opp = new operations();
            //Set insert query
            string mysql = "Delete From   FixedAssetData_ImagesStore Where ImageId=" + ImageId;//
            opp.insert_me(mysql);

        }




        byte[] ReadFile(string sPath)
        {
            //Initialize byte array with a null value initially.
            byte[] data = null;

            //Use FileInfo object to get file size.
            FileInfo fInfo = new FileInfo(sPath);
            long numBytes = fInfo.Length;

            //Open FileStream to read file
            FileStream fStream = new FileStream(sPath, FileMode.Open, FileAccess.Read);

            //Use BinaryReader to read file stream into byte array.
            BinaryReader br = new BinaryReader(fStream);

            //When you use BinaryReader, you need to supply number of bytes to read from file.
            //In this case we want to read entire file. So supplying total number of bytes.
            data = br.ReadBytes((int)numBytes);
            return data;
        }


        public void AddNewFixedAssetsWithImg(ref int Excp, ref string Msg, FixedAssetCLS fx, string ImagePath, string Desc)
        {
            operations opp = new operations(1);

            try
            {
                AddNewFixedAssets(ref Excp, ref Msg, ref opp, fx);
                if (Excp == 0)
                {
                    Excp = 0;
                    acc.UpdateSubmain_SubAccount_Id(ref Excp, ref Msg);


                    ////////////حفظ الصوره
                    //fNew = new frmNewImage();
                    SaveImage(ref Excp, ref opp, ImagePath, fxd.Id, Desc);
                    if (Excp == 0)
                    {
                        opp.Commit();




                    }
                    else
                    {
                        opp.Rollback();
                    }
                }
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
                opp.Rollback();
            }
        }
        Sub_Account acc;
        public void AddNewFixedAssets(ref int Excp, ref string Msg, ref operations opp, FixedAssetCLS fx)
        {

            Excp = 0;
            Msg = "";
            string Account_Code = "";
            acc = new Sub_Account();
            int Account_Type_Value = Common.Assoul_Sabta_Account_Type;
            int Currency_Id = 1;
            int Account_Branch_Id = Common.Branch_Id;
            long New_Submain_Account_Id = -1;
            acc.fixedAssetCLS = fx;
            acc.Add_AccountSupplierCustomerOther(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, ref Account_Code, Account_Type_Value,   fxd.Name_Ar, Account_Branch_Id, Currency_Id);

            fxd.Id = acc.New_Account_Id;
            fxd.SubAccount_Id = New_Submain_Account_Id;



        }
        

        public void UpdateFixedAsset(ref int Excp, ref operations opp, Submain_Account  sa)
        {

            DataTable dt = GetFixedAsset(ref opp, sa.Id);
            if (dt.Rows.Count == 0)
            {
                fxd.Name_Ar = sa.SubAccount_Name;
                fxd.SubAccount_Id = sa.Id;
                fxd.Account_Code = sa.Account_Code;
                fxd.Branch_Id = sa.Branch_Id;

                InsertFixedAsset(ref opp);
            }
            else
            {
                fxd.Id = long.Parse(dt.Rows[0]["Id"].ToString());
                Get_FixedAssetData(ref opp, fxd.Id);

                fxd.Name_Ar = sa.SubAccount_Name;
                fxd.SubAccount_Id = sa.Id;
                fxd.Account_Code = sa.Account_Code;
                fxd.Branch_Id = sa.Branch_Id;

                UpdateFixedAsset(ref opp);
            }
        }

        public void DeleteFixedAsset(ref operations opp, long Id)
        {

            string mysql = "";
            if (Common.Update_DeleteSupplier == 0)
            {
                mysql = "Update FixedAssetData set active=0 Where Id= " + Id;
                opp.insert_me(mysql);

            }
            if (Common.Update_DeleteSupplier == 1)
            {
                mysql = "delete from FixedAssetData where Id= " + Id;
                opp.insert_me(mysql);
                Delete_FixedAssetData_ImagesStore(ref opp, Id);
            }
        }
        public void MakeDeleteFixedAssetData(ref operations opp, ref int Excp, ref string Msg, long Id, long SubAccount_Id)
        {
            string mysql = "";
            if (Common.Update_DeleteSupplier == 0)
            {
                mysql = "Update FixedAssetData set active=0 Where Id= " + Id;
                mysql = mysql + "Update FixedAssetData set active=0 Where SubAccount_Id= " + SubAccount_Id;
            }
            if (Common.Update_DeleteSupplier == 1)
            {
                mysql = "delete from FixedAssetData where Id= " + Id;
                mysql = mysql + "delete from FixedAssetData where SubAccount_Id= " + SubAccount_Id;

                Delete_FixedAssetData_ImagesStore(ref opp, Id);
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

        public void Delete_FixedAssetData_ImagesStore(ref operations opp, long FixedAssetData_Id)
        {

            string mysql = "Delete From   FixedAssetData_ImagesStore Where FixedAssetData_Id=" + FixedAssetData_Id;
            opp.insert_me(mysql);

        }

        
    }
}
