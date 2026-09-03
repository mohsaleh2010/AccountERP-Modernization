using System;
 
using System.Collections.Generic;
using System.Text;
   
using System.Data;
using System.Collections;
namespace AccountModelsLib.Models
{
    public class Order_DetailsCls
    {

        public long Order_Purshing_No;
        public DateTime Order_Date;
        public int Order_Type;
        public int Type_From;
        public long Id_From;
        public string Id_FromStr;
        public int Type_To;
        public long Id_To;
        public string Id_ToStr;
        public long InvoiceTable_ID;
        public int Machine_Id = 0;/// —ﬁ„ «·„Ìﬂ‰… ›Ï Õ«·… ’—› ﬁÿ⁄ €Ì«—
        public long MainOrderTable_id = 0;
        public long OrderTable_id_C = 0;
        public int Is_Converted = 0;
        public string NotesAll = "";
        public string OrderTitle = "";
        public int? IsMoreTheOneOrderShow = 2;
        
        public Order_DetailsCls()
        {

        }

        public void Make_New_Order_FromInvoice(ref int Excp, ref operations opp, ref long ID)
        {
            try
            {
                string mysql = Get_mySql_Make_New_Order();
                ID = opp.Insert_me_Return_Last_Identify(mysql, ref Excp);


                Excp = 0;

            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }

        }

        public void Make_New_Order(ref int Excp, ref long ID)
        {
            try
            {
                string mysql = Get_mySql_Make_New_Order();

                operations opp = new operations();
                ID = opp.Insert_me_Return_Last_Identify(mysql, ref Excp);

                opp = null;
                Excp = 0;

            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }

        }
        string Get_mySql_Make_New_Order()
        {
            
            string mysql = " INSERT INTO Order_Purshing (Order_Purshing_No, Order_Date, Order_Type, Type_From, Id_From, Type_To, Id_To, Invoice_ID,User_Id,Branch_ID,Id_FromStr,Id_ToStr,MainOrderTable_id,Is_Converted,NotesAll,IsMoreTheOneOrderShow) ";
            mysql = mysql + " VALUES   (" + Order_Purshing_No + ",'" + operations.dateFormat(Order_Date) + "'," + Order_Type + "," + Type_From + "," + Id_From + "," + Type_To + "," + Id_To + "," + InvoiceTable_ID + "," + Common.User_Id + "," + Common.Branch_Id + ",'" + Id_FromStr + "','" + Id_ToStr + "'," + MainOrderTable_id + "," + Is_Converted + ",N'" + NotesAll + "',"+ IsMoreTheOneOrderShow + ") ";

            return mysql;


        }
        public void Get_OrderTypes_Label_FromTo(ref int Excp, string FLAG, int Type_ID, long ID_From_To, ref string LabelFromTo, ref string Label_FromTo)
        {
            try
            {
                LabelFromTo = "";
                Label_FromTo = "";
                operations opp = new operations();
                string mysql = "Select * from OrderTypes_Label Where TypesID=" + Type_ID;

                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count == 0)
                {
                    LabelFromTo = "Error";
                    return;
                }
                if (FLAG == "From")
                {
                    LabelFromTo = dt.Rows[0]["LabelFrom"].ToString();

                }
                else
                {
                    LabelFromTo = dt.Rows[0]["LabelTo"].ToString();
                }
                Label_FromTo = dt.Rows[0]["Label_Name"].ToString();

                /////////////Get  txt Label_Name  if dept,store,customer,supplier
                if (Type_ID == 0)
                {
                    mysql = "Select * from Department Where ID=" + ID_From_To;
                    dt.Clear();
                    dt = opp.return_DataTable(mysql);
                    if (dt.Rows.Count > 0)
                    {
                        Label_FromTo = dt.Rows[0]["dname"].ToString();
                    }

                }
                if (Type_ID == 1)
                {
                    mysql = "Select * from Supplier Where ID=" + ID_From_To;
                    dt.Clear();
                    dt = opp.return_DataTable(mysql);
                    if (dt.Rows.Count > 0)
                    {
                        Label_FromTo = dt.Rows[0]["Name"].ToString();
                    }

                }
                if (Type_ID == 2)
                {
                    mysql = "Select * from Customer Where ID=" + ID_From_To;
                    dt.Clear();
                    dt = opp.return_DataTable(mysql);
                    if (dt.Rows.Count > 0)
                    {
                        Label_FromTo = dt.Rows[0]["Name"].ToString();
                    }

                }
                if (Type_ID == 4)
                {
                    mysql = "Select * from Store Where ID=" + ID_From_To;
                    dt.Clear();
                    dt = opp.return_DataTable(mysql);
                    if (dt.Rows.Count > 0)
                    {
                        Label_FromTo = dt.Rows[0]["Store_Name"].ToString();
                    }

                }





                opp = null;
                Excp = 0;

            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }

        }

        public void Get_Order_Label(ref int Excp, int OrderTypes, ref string Order_Name, ref string BtnRecive_Text, ref string Order_Estlam_Sarf_Flage)
        {
            try
            {


                operations opp = new operations();
                //string mysql = "Select * from OrderTypes Where OrderTypes_Value=" + OrderTypes;
                string mysql = "Select * from DataDictionaryProcessesLists Where Id=" + OrderTypes;

                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    Order_Name = dt.Rows[0]["ProcessName"].ToString();
                    //Order_Name = dt.Rows[0]["Order_Name"].ToString();
                    BtnRecive_Text = dt.Rows[0]["BtnRecive_Text"].ToString();
                    Order_Estlam_Sarf_Flage = dt.Rows[0]["Order_Estlam_Sarf"].ToString();
                }
                else
                {
                    Order_Name = "Error";
                    BtnRecive_Text = "Error";
                }



                opp = null;
                Excp = 0;

            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }

        }
        /**/

        public void Make_Order_Details_Automatic_Invoice(ref int Excp, ref operations opp, ref long Related_Detailes_Purshing_Id, string Order_Estlam_Sarf_Flage, long OrderTable_id, long Unit_Item_Table_Id, long Item_id, decimal Larg_Qnty, decimal Small_Qnty, decimal ItemNO, decimal Balance_Now_Larg_Qnty, decimal Balance_Now_Small_Qnty, decimal Balance_Now_Item_No, decimal S_unit_Value, string Notes_From, string Notes_To, string Notes, long FromStoreId, long ToStoreId, string Notes_Item = "")
        {

            string Msg = "";
            Make_Order_Details_(ref Excp,ref Msg, ref opp, ref Related_Detailes_Purshing_Id, Order_Estlam_Sarf_Flage, OrderTable_id, Unit_Item_Table_Id, Item_id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, Balance_Now_Item_No, S_unit_Value, Notes_From, Notes_To, Notes, Notes_Item, FromStoreId, ToStoreId);

        }
       
        public void Make_Order_Details_AllOneClick(ref int Excp, ref operations opp, ref long Related_Detailes_Purshing_Id, string Order_Estlam_Sarf_Flage, long OrderTable_id, long Unit_Item_Table_Id, long Item_id, decimal Larg_Qnty, decimal Small_Qnty, decimal ItemNO, decimal Balance_Now_Larg_Qnty, decimal Balance_Now_Small_Qnty, decimal Balance_Now_Item_No, decimal S_unit_Value, string Notes_From, string Notes_To, string Notes, string Notes_Item, long FromStoreId, long ToStoreId)
        {


            try
            {
                string Msg = "";
                Make_Order_Details_(ref Excp,ref Msg, ref opp, ref Related_Detailes_Purshing_Id, Order_Estlam_Sarf_Flage, OrderTable_id, Unit_Item_Table_Id, Item_id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, Balance_Now_Item_No, S_unit_Value, Notes_From, Notes_To, Notes, Notes_Item, FromStoreId, ToStoreId);


                Excp = 0;
            }
            catch (Exception ee)
            {
                //MessageBox.Show(ee.Message);
                Excp = 1;
            }


            //opp = null;

        }
        void Make_Order_Details_(ref int Excp, ref string Msg, ref operations opp, ref long Related_Detailes_Purshing_Id, string Order_Estlam_Sarf_Flage, long OrderTable_id, long Unit_Item_Table_Id, long Item_id, decimal Larg_Qnty, decimal Small_Qnty, decimal ItemNO, decimal Balance_Now_Larg_Qnty, decimal Balance_Now_Small_Qnty, decimal Balance_Now_Item_No, decimal S_unit_Value, string Notes_From, string Notes_To, string Notes, string Notes_Item, long FromStoreId, long ToStoreId)
        {
            // ⁄„»· „⁄  €ÌÌ— ÂÊÌ… ’‰›

            string mysql = "";

            decimal Qua_Befor = 0;
            decimal Part_qua_Befor = 0;
            decimal Item_No_Befor = 0;
            decimal Qua_After = 0;
            decimal Part_qua_After = 0;
            decimal Item_No_After = 0;
            Related_Detailes_Purshing_Id = 0;




            DataSet dss;
            Cls_Conversion_Unit clc = new Cls_Conversion_Unit();
            ////›Ï Õ«·… “Ì«œ… «·—’Ìœ

            decimal Balance_Id_From_Larg_Qnty = 0;
            decimal Balance_Id_From_Small_Qnty = 0;
            decimal Balance_Id_From_Item_No = Balance_Now_Item_No - ItemNO;

            ////›Ï Õ«·… ‰ﬁ’ «·—’Ìœ
            decimal Balance_Id_To_Larg_Qnty = 0;
            decimal Balance_Id_To_Small_Qnty = 0;
            decimal Balance_Id_To_Item_No = Balance_Now_Item_No + ItemNO;


            decimal Unit_Item_Table_Larg_Qnty = 0;
            decimal Unit_Item_Table__Small_Qnty = 0;
            decimal Unit_Item_Table_Item_No = 0;
            long To_Store_Unit_Item_Table_Id = 0;
            //decimal Larg_Qnty, decimal Small_Qnty, decimal 
            if (ItemNO == 0)
            {
                if (Common.Compute_3eboaFrom_EznSarf_Estlam == 1)
                {
                    ItemNO = Common.Get3ebowa(Larg_Qnty, Item_id);
                }
            }
            //«·€«¡ «·ﬂ”Ê— „‰ «·ﬂ„Ì«   
            Cls_Conversion_Unit.ConVert_By_S_UnitValue(ref Larg_Qnty, ref Small_Qnty, S_unit_Value);
            #region Transform           ÕÊÌ·
            if (Order_Type == 1 | Order_Type == 6)             //«· ÕÊÌ·
            {



                //// ⁄œÌ· »Ì«‰«  «·„Œ“‰ «·„‰’—› „‰… »‰ﬁ’ «·—’Ìœ

                clc.ConVersionQuntity_Subtract_ToItem(false, Larg_Qnty, Small_Qnty, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, ref Balance_Id_From_Larg_Qnty, ref Balance_Id_From_Small_Qnty, S_unit_Value);

                Unit_Item_Table_Larg_Qnty = Balance_Id_From_Larg_Qnty;
                Unit_Item_Table__Small_Qnty = Balance_Id_From_Small_Qnty;
                Unit_Item_Table_Item_No = Balance_Id_From_Item_No;

                mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                //arr.Add(mysql);
                opp.insert_me(mysql);

                //mysql = " UPDATE Unit_item  SET Full_qua =" + Unit_Item_Table_Larg_Qnty + ", D_Qua =" + Unit_Item_Table__Small_Qnty + ", Item_No =" + Unit_Item_Table_Item_No + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                ////arr.Add(mysql);
                //opp.insert_me(mysql);


                ////////////////////////////////////////////////////////////
                Qua_Befor = Balance_Now_Larg_Qnty;
                Part_qua_Befor = Balance_Now_Small_Qnty;
                Item_No_Befor = Balance_Now_Item_No;

                Qua_After = Unit_Item_Table_Larg_Qnty;
                Part_qua_After = Unit_Item_Table__Small_Qnty;
                Item_No_After = Balance_Id_To_Item_No;

                //////////////////// «·»ÕÀ ⁄‰ —’Ìœ «·„Œ“‰ «·„ÕÊ· «·ÌÂ »“Ì«œ… «·—’Ìœ
                decimal To_Store_LargBalance = 0;
                decimal To_Store_Small_Balance = 0;
                decimal To_Store_Item_No = 0;

                if (!Check_ItemFoundOnStore(ref Excp, ref Msg, Item_id, ref To_Store_Unit_Item_Table_Id, ref To_Store_LargBalance, ref To_Store_Small_Balance, ref To_Store_Item_No, Unit_Item_Table_Id))
                {
                    Excp = 1;
                    return;
                }


                Balance_Id_To_Larg_Qnty = To_Store_LargBalance + Larg_Qnty;

                Balance_Id_To_Small_Qnty = To_Store_Small_Balance + Small_Qnty;
                Balance_Id_To_Item_No = To_Store_Item_No + ItemNO;


                Related_Detailes_Purshing_Id = InsertToDetailes_Purshing(ref opp, Order_Estlam_Sarf_Flage, Order_Type, OrderTable_id, Unit_Item_Table_Id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Id_From_Larg_Qnty, Balance_Id_From_Small_Qnty, Balance_Id_From_Item_No, Balance_Id_To_Larg_Qnty, Balance_Id_To_Small_Qnty, Balance_Id_To_Item_No, To_Store_Unit_Item_Table_Id, Notes_From, Notes_To, Notes, Machine_Id, Qua_Befor, Part_qua_Befor, Item_No_Befor, Qua_After, Part_qua_After, Item_No_After, S_unit_Value, 0, Notes_Item, FromStoreId, ToStoreId);






                ////“Ì«œ… «·—’Ìœ Ê ›⁄Ì· «·«–‰ «Ê Ê„« Ìﬂ «Ï «‰‘«¡ «·«–‰ ﬂ√‰… «–‰ ’—› „‰ Ê«–‰ «” ·«„ «·Ï
                if (Common.Make_Store_Order_Sarf_Estlam_Ta7weel_Automatic_Between_Stores == 1)/////
                {//////////////////////////////////////////////////////////

                    //// ⁄œÌ· »Ì«‰«  «·„Œ“‰ «·„‰’—› ≈·Ì… »“Ì«œ… «·—’Ìœ
                    clc.ConVersionQuntity_ADD_ToItem(Larg_Qnty, Small_Qnty, To_Store_LargBalance, To_Store_Small_Balance, ref Balance_Id_To_Larg_Qnty, ref Balance_Id_To_Small_Qnty, S_unit_Value);

                    decimal To_Unit_Item_Table_Larg_Qnty = Balance_Id_To_Larg_Qnty;
                    decimal To_Unit_Item_Table__Small_Qnty = Balance_Id_To_Small_Qnty;
                    decimal To_Unit_Item_Table_Item_No = To_Store_Item_No + ItemNO;
                    decimal To_Balance_Id_To_Item_No = Unit_Item_Table_Item_No;

                    mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + "  WHERE (id = " + To_Store_Unit_Item_Table_Id + ") ";
                    //arr.Add(mysql);
                    opp.insert_me(mysql);

                    //mysql = " UPDATE Unit_item  SET Full_qua =" + To_Unit_Item_Table_Larg_Qnty + ", D_Qua =" + To_Unit_Item_Table__Small_Qnty + ", Item_No =" + To_Balance_Id_To_Item_No + "  WHERE (id = " + To_Store_Unit_Item_Table_Id + ") ";
                    ////arr.Add(mysql);
                    //opp.insert_me(mysql);


                    decimal To_Qua_Befor = To_Store_LargBalance;
                    decimal To_Part_qua_Befor = To_Store_Small_Balance;
                    decimal To_Item_No_Befor = To_Store_Item_No;

                    decimal To_Qua_After = To_Unit_Item_Table_Larg_Qnty;
                    decimal To_Part_qua_After = To_Unit_Item_Table__Small_Qnty;
                    decimal To_Item_No_After = To_Balance_Id_To_Item_No;


                    Balance_Id_From_Larg_Qnty = Balance_Now_Larg_Qnty - Larg_Qnty;

                    Balance_Id_From_Small_Qnty = Balance_Now_Small_Qnty - Small_Qnty;
                    Balance_Id_From_Item_No = Balance_Now_Item_No - ItemNO;


                    string Order_Estlam_Sarf_Flage_1 = "1";
                    if (Order_Estlam_Sarf_Flage == "1")
                    {
                        Order_Estlam_Sarf_Flage_1 = "2";
                    }

                    //        InsertToDetailes_Purshing(ref opp, Order_Estlam_Sarf_Flage, Order_Type, OrderTable_id, Unit_Item_Table_Id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Id_From_Larg_Qnty, Balance_Id_From_Small_Qnty, Balance_Id_From_Item_No, Balance_Id_To_Larg_Qnty, Balance_Id_To_Small_Qnty, Balance_Id_To_Item_No, To_Store_Unit_Item_Table_Id, Notes_From, Notes_To, Notes, Machine_Id, Qua_Befor, Part_qua_Befor, Item_No_Befor, Qua_After, Part_qua_After, Item_No_After, S_unit_Value,0);

                    long Sub_Related_Detailes_Purshing_Id = InsertToDetailes_Purshing(ref opp, Order_Estlam_Sarf_Flage_1, Order_Type, OrderTable_id_C, To_Store_Unit_Item_Table_Id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Id_To_Larg_Qnty, Balance_Id_To_Small_Qnty, Balance_Id_To_Item_No, Balance_Id_From_Larg_Qnty, Balance_Id_From_Small_Qnty, Balance_Id_From_Item_No, Unit_Item_Table_Id, Notes_From, Notes_To, Notes, Machine_Id, To_Qua_Befor, To_Part_qua_Befor, To_Item_No_Befor, To_Qua_After, To_Part_qua_After, To_Item_No_After, S_unit_Value, Related_Detailes_Purshing_Id, Notes_Item, FromStoreId, ToStoreId);

                    mysql = " UPDATE Detailes_Purshing  SET  Related_Detailes_Purshing_Id=" + Related_Detailes_Purshing_Id + ",To_Store_Unit_Item_Table_Id=" + To_Store_Unit_Item_Table_Id + "  WHERE (id = " + Sub_Related_Detailes_Purshing_Id + ") ";
                    //arr.Add(mysql);
                    opp.insert_me(mysql);

                    mysql = " UPDATE Detailes_Purshing  SET  Related_Detailes_Purshing_Id=" + Sub_Related_Detailes_Purshing_Id + "   WHERE (id = " + Related_Detailes_Purshing_Id + ") ";
                    //arr.Add(mysql);
                    opp.insert_me(mysql);

                }
                else
                {
                    To_Store_Unit_Item_Table_Id = 0;
                    Balance_Id_To_Larg_Qnty = 0;
                    Balance_Id_To_Small_Qnty = 0;
                    Balance_Id_To_Item_No = 0;

                }


                //mysql = " INSERT INTO Detailes_Purshing (OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Balance_Id_From_Larg_Qnty, Balance_Id_From_Small_Qnty,Balance_Id_From_Item_No,Balance_Id_To_Larg_Qnty,Balance_Id_To_Small_Qnty,Balance_Id_To_Item_No,To_Store_Unit_Item_Table_Id , Notes_From,Notes_To,Notes,Machine_Id) ";
                //mysql = mysql + "  VALUES (" + OrderTable_id + "," + Unit_Item_Table_Id + "," + Larg_Qnty + "," + Small_Qnty + "," + ItemNO + "," + Balance_Id_From_Larg_Qnty + "," + Balance_Id_From_Small_Qnty + "," + Balance_Id_From_Item_No + "," + Balance_Id_To_Larg_Qnty + "," + Balance_Id_To_Small_Qnty + "," + Balance_Id_To_Item_No + "," + To_Store_Unit_Item_Table_Id + " , '" + Notes_From + "','" + Notes_To + "','" + Notes + "',"+Machine_Id+")";
                ////arr.Add(mysql);
                //opp.insert_me(mysql);



            }
            #endregion

            #region Transform          - «” ·«„ „‰ «·«‰ «Ã  ”ÊÌ… »«·“Ì«œ…
            //if (Order_Type == 3 | Order_Type == 4 | Order_Type == 7 |  Order_Type == 9)             // “Ì«œ… „‰
            //else if(  Order_Estlam_Sarf_Flage=="1")
            // {
            //     long To_Store_Unit_Item_Table_Id = 0;
            //     decimal To_Store_LargBalance = 0;
            //     decimal To_Store_Small_Balance = 0;
            //     decimal To_Store_Item_No = 0;

            //     //if (!Check_ItemFoundOnStore(Item_id, ref To_Store_Unit_Item_Table_Id, ref To_Store_LargBalance, ref To_Store_Smal l_Balance, ref   To_Store_Item_No, Unit_Item_Table_Id))
            //     //{
            //     //    Excp = 1;
            //     //    return;
            //     //}

            //     //// ⁄œÌ· »Ì«‰«  «·„Œ“‰ «·„‰’—› ≈·Ì… »“Ì«œ… «·—’Ìœ
            //     clc.ConVersionQuntity_ADD_ToItem(Larg_Qnty, Small_Qnty, To_Store_LargBalance, To_Store_Small_Balance, ref Balance_Id_To_Larg_Qnty, ref  Balance_Id_To_Small_Qnty, S_unit_Value);

            //     Unit_Item_Table_Larg_Qnty = Balance_Id_To_Larg_Qnty;
            //     Unit_Item_Table__Small_Qnty = Balance_Id_To_Small_Qnty;
            //     Unit_Item_Table_Item_No = To_Store_Item_No + ItemNO;
            //     Balance_Id_To_Item_No = Unit_Item_Table_Item_No;

            //     mysql = " UPDATE Unit_item  SET Full_qua =" + Unit_Item_Table_Larg_Qnty + ", D_Qua =" + Unit_Item_Table__Small_Qnty + ", Item_No =" + Unit_Item_Table_Item_No + "  WHERE (id = " + To_Store_Unit_Item_Table_Id + ") ";
            //     arr.Add(mysql);

            //      //mysql = " INSERT INTO Detailes_Purshing (OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Balance_Id_From_Larg_Qnty, Balance_Id_From_Small_Qnty,Balance_Id_From_Item_No,Balance_Id_To_Larg_Qnty,Balance_Id_To_Small_Qnty,Balance_Id_To_Item_No,To_Store_Unit_Item_Table_Id) ";
            //     //mysql = mysql + "  VALUES (" + OrderTable_id + "," + Unit_Item_Table_Id + "," + Larg_Qnty + "," + Small_Qnty + "," + ItemNO + "," + Balance_Id_From_Larg_Qnty + "," + Balance_Id_From_Small_Qnty + "," + Balance_Id_From_Item_No + "," + Balance_Id_To_Larg_Qnty + "," + Balance_Id_To_Small_Qnty + "," + Balance_Id_To_Item_No + "," + To_Store_Unit_Item_Table_Id + ")";
            //     //arr.Add(mysql);

            //     mysql = " INSERT INTO Detailes_Purshing (OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Balance_Id_TO_Larg_Qnty, Balance_Id_TO_Small_Qnty,Balance_Id_TO_Item_No,Balance_Id_From_Larg_Qnty,Balance_Id_From_Small_Qnty,Balance_Id_From_Item_No,To_Store_Unit_Item_Table_Id, Notes_From,Notes_To,Notes) ";
            //     mysql = mysql + "  VALUES (" + OrderTable_id + "," + Unit_Item_Table_Id + "," + Larg_Qnty + "," + Small_Qnty + "," + ItemNO + "," + Balance_Id_From_Larg_Qnty + "," + Balance_Id_From_Small_Qnty + "," + Balance_Id_From_Item_No + "," + Balance_Id_To_Larg_Qnty + "," + Balance_Id_To_Small_Qnty + "," + Balance_Id_To_Item_No + "," + To_Store_Unit_Item_Table_Id + ", '"+Notes_From+"','"+Notes_To+"','"+Notes+"')";
            //     arr.Add(mysql);
            // }
            #endregion

            #region    mortaga3 sarf or  estlam
            //else if (Order_Type == 3 | Order_Type == 4)
            else if (Order_Estlam_Sarf_Flage == "1")
            {
                //// ⁄œÌ· »Ì«‰«  «·„Œ“‰ «·„‰’—› ≈·Ì… »“Ì«œ… «·—’Ìœ
                clc.ConVersionQuntity_ADD_ToItem(Larg_Qnty, Small_Qnty, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, ref Balance_Id_To_Larg_Qnty, ref Balance_Id_To_Small_Qnty, S_unit_Value);

                Unit_Item_Table_Larg_Qnty = Balance_Id_To_Larg_Qnty;
                Unit_Item_Table__Small_Qnty = Balance_Id_To_Small_Qnty;
                Unit_Item_Table_Item_No = Balance_Id_To_Item_No;

                mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                //arr.Add(mysql);
                opp.insert_me(mysql);


                //mysql = " UPDATE Unit_item  SET Full_qua =" + Unit_Item_Table_Larg_Qnty + ", D_Qua =" + Unit_Item_Table__Small_Qnty + ", Item_No =" + Unit_Item_Table_Item_No + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                ////arr.Add(mysql);
                //opp.insert_me(mysql);

                To_Store_Unit_Item_Table_Id = -1;
                //Balance_Id_From_Larg_Qnty = 0;
                //Balance_Id_From_Small_Qnty = 0;
                //Balance_Id_From_Item_No = 0;
                Balance_Id_From_Larg_Qnty = Balance_Id_To_Larg_Qnty;
                Balance_Id_From_Small_Qnty = Balance_Id_To_Small_Qnty;
                Balance_Id_From_Item_No = Balance_Id_To_Item_No;
                ////////////////////////////////////////////////////////////
                Qua_Befor = Balance_Now_Larg_Qnty;
                Part_qua_Befor = Balance_Now_Small_Qnty;
                Item_No_Befor = Balance_Now_Item_No;

                Qua_After = Unit_Item_Table_Larg_Qnty;
                Part_qua_After = Unit_Item_Table__Small_Qnty;
                Item_No_After = Balance_Id_To_Item_No;

                //////////////////////////////////////////////////////////


                Related_Detailes_Purshing_Id = InsertToDetailes_Purshing(ref opp, Order_Estlam_Sarf_Flage, Order_Type, OrderTable_id, Unit_Item_Table_Id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Id_From_Larg_Qnty, Balance_Id_From_Small_Qnty, Balance_Id_From_Item_No, Balance_Id_To_Larg_Qnty, Balance_Id_To_Small_Qnty, Balance_Id_To_Item_No, To_Store_Unit_Item_Table_Id, Notes_From, Notes_To, Notes, Machine_Id, Qua_Befor, Part_qua_Befor, Item_No_Befor, Qua_After, Part_qua_After, Item_No_After, S_unit_Value, 0, Notes_Item, FromStoreId, ToStoreId);

                ////mysql = " INSERT INTO Detailes_Purshing (OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Balance_Id_From_Larg_Qnty, Balance_Id_From_Small_Qnty,Balance_Id_From_Item_No,Balance_Id_To_Larg_Qnty,Balance_Id_To_Small_Qnty,Balance_Id_To_Item_No,To_Store_Unit_Item_Table_Id, Notes_From,Notes_To,Machine_Id) ";
                ////mysql = mysql + "  VALUES (" + OrderTable_id + "," + Unit_Item_Table_Id + "," + Larg_Qnty + "," + Small_Qnty + "," + ItemNO + ",0,0,0," + Balance_Id_To_Larg_Qnty + "," + Balance_Id_To_Small_Qnty + "," + Balance_Id_To_Item_No + ",-1, '" + Notes_From + "','" + Notes_To + "'," + Machine_Id + ")";
                //////arr.Add(mysql);
                ////opp.insert_me(mysql);

                //mysql = " INSERT INTO Detailes_Purshing (OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Balance_Id_From_Larg_Qnty, Balance_Id_From_Small_Qnty,Balance_Id_From_Item_No,Balance_Id_To_Larg_Qnty,Balance_Id_To_Small_Qnty,Balance_Id_To_Item_No,To_Store_Unit_Item_Table_Id, Notes_From,Notes_To,Machine_Id) ";
                //mysql = mysql + "  VALUES (" + OrderTable_id + "," + Unit_Item_Table_Id + "," + Larg_Qnty + "," + Small_Qnty + "," + ItemNO + "," + Balance_Id_From_Larg_Qnty + "," + Balance_Id_From_Small_Qnty + "," + Balance_Id_From_Item_No + "," + Balance_Id_To_Larg_Qnty + "," + Balance_Id_To_Small_Qnty + "," + Balance_Id_To_Item_No + "," + To_Store_Unit_Item_Table_Id + ", '" + Notes_From + "','" + Notes_To + "'," + Machine_Id + ")";
                ////arr.Add(mysql);
                //opp.insert_me(mysql);
            }
            #endregion

            #region     sarf or mortaga3 estlam
            //else if (Order_Type == 2 | Order_Type == 5 | Order_Type == 8 | Order_Type == 10)
            else if (Order_Estlam_Sarf_Flage == "2")
            {

                //// ⁄œÌ· »Ì«‰«  «·„Œ“‰ «·„‰’—› „‰… »‰ﬁ’ «·—’Ìœ
                clc.ConVersionQuntity_Subtract_ToItem(false, Larg_Qnty, Small_Qnty, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, ref Balance_Id_From_Larg_Qnty, ref Balance_Id_From_Small_Qnty, S_unit_Value);

                Unit_Item_Table_Larg_Qnty = Balance_Id_From_Larg_Qnty;
                Unit_Item_Table__Small_Qnty = Balance_Id_From_Small_Qnty;
                Unit_Item_Table_Item_No = Balance_Id_From_Item_No;

                mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                //arr.Add(mysql);
                opp.insert_me(mysql);

                //mysql = " UPDATE Unit_item  SET Full_qua =" + Unit_Item_Table_Larg_Qnty + ", D_Qua =" + Unit_Item_Table__Small_Qnty + ", Item_No =" + Unit_Item_Table_Item_No + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                ////arr.Add(mysql);
                //opp.insert_me(mysql);

                To_Store_Unit_Item_Table_Id = -1;
                //Balance_Id_To_Larg_Qnty = 0;
                //Balance_Id_To_Small_Qnty = 0;
                //Balance_Id_To_Item_No = 0;

                Balance_Id_To_Larg_Qnty = Balance_Id_From_Larg_Qnty;
                Balance_Id_To_Small_Qnty = Balance_Id_From_Larg_Qnty;
                Balance_Id_To_Item_No = Balance_Id_From_Item_No;

                ////////////////////////////////////////////////////////////
                Qua_Befor = Balance_Now_Larg_Qnty;
                Part_qua_Befor = Balance_Now_Small_Qnty;
                Item_No_Befor = Balance_Now_Item_No;

                Qua_After = Unit_Item_Table_Larg_Qnty;
                Part_qua_After = Unit_Item_Table__Small_Qnty;
                Item_No_After = Balance_Id_To_Item_No;

                //////////////////////////////////////////////////////////


                Related_Detailes_Purshing_Id = InsertToDetailes_Purshing(ref opp, Order_Estlam_Sarf_Flage, Order_Type, OrderTable_id, Unit_Item_Table_Id, Larg_Qnty, Small_Qnty, ItemNO, Balance_Id_From_Larg_Qnty, Balance_Id_From_Small_Qnty, Balance_Id_From_Item_No, Balance_Id_To_Larg_Qnty, Balance_Id_To_Small_Qnty, Balance_Id_To_Item_No, To_Store_Unit_Item_Table_Id, Notes_From, Notes_To, Notes, Machine_Id, Qua_Befor, Part_qua_Befor, Item_No_Befor, Qua_After, Part_qua_After, Item_No_After, S_unit_Value, 0, Notes_Item, FromStoreId, ToStoreId);

                ////mysql = " INSERT INTO Detailes_Purshing (OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Balance_Id_From_Larg_Qnty, Balance_Id_From_Small_Qnty,Balance_Id_From_Item_No,Balance_Id_To_Larg_Qnty,Balance_Id_To_Small_Qnty,Balance_Id_To_Item_No,To_Store_Unit_Item_Table_Id, Notes_From,Notes_To,Notes,Machine_Id) ";
                ////mysql = mysql + "  VALUES (" + OrderTable_id + "," + Unit_Item_Table_Id + "," + Larg_Qnty + "," + Small_Qnty + "," + ItemNO + "," + Balance_Id_From_Larg_Qnty + "," + Balance_Id_From_Small_Qnty + "," + Balance_Id_From_Item_No + ",0,0,0,-1, '" + Notes_From + "','" + Notes_To + "','" + Notes + "'," + Machine_Id + ")";
                //////arr.Add(mysql);
                ////opp.insert_me(mysql);
                //mysql = " INSERT INTO Detailes_Purshing (OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Balance_Id_From_Larg_Qnty, Balance_Id_From_Small_Qnty,Balance_Id_From_Item_No,Balance_Id_To_Larg_Qnty,Balance_Id_To_Small_Qnty,Balance_Id_To_Item_No,To_Store_Unit_Item_Table_Id, Notes_From,Notes_To,Notes,Machine_Id) ";
                //mysql = mysql + "  VALUES (" + OrderTable_id + "," + Unit_Item_Table_Id + "," + Larg_Qnty + "," + Small_Qnty + "," + ItemNO + "," + Balance_Id_From_Larg_Qnty + "," + Balance_Id_From_Small_Qnty + "," + Balance_Id_From_Item_No + "," + Balance_Id_To_Larg_Qnty + "," + Balance_Id_To_Small_Qnty + "," + Balance_Id_To_Item_No + "," + To_Store_Unit_Item_Table_Id + ", '" + Notes_From + "','" + Notes_To + "','" + Notes + "'," + Machine_Id + ")";
                ////arr.Add(mysql);
                //opp.insert_me(mysql);

            }
            #endregion



        }
        long InsertToDetailes_Purshing(ref operations opp, string Order_Estlam_Sarf_Flage, int Order_Type, long OrderTable_id, long Unit_Item_Table_Id, decimal Larg_Qnty, decimal Small_Qnty, decimal ItemNO, decimal Balance_Id_From_Larg_Qnty, decimal Balance_Id_From_Small_Qnty, decimal Balance_Id_From_Item_No, decimal Balance_Id_To_Larg_Qnty, decimal Balance_Id_To_Small_Qnty, decimal Balance_Id_To_Item_No, decimal To_Store_Unit_Item_Table_Id, string Notes_From, string Notes_To, string Notes, int Machine_Id, decimal Qua_Befor, decimal Part_qua_Befor, decimal Item_No_Befor, decimal Qua_After, decimal Part_qua_After, decimal Item_No_After, decimal S_Unit_Value, long Related_Detailes_Purshing_Id, string Notes_Item, long FromStoreId, long ToStoreId)
        {

            decimal qua_Import = 0;
            decimal Part_Qua_Import = 0;
            decimal Item_No_Import = 0;
            decimal Quantity_Import = 0;

            decimal qua_Export = 0;
            decimal Part_Qua_Export = 0;
            decimal Item_No_Export = 0;
            decimal Quantity_Export = 0;

            decimal Quantity = 0;
            decimal Quantity_Balance_From = 0;

            decimal Quantity_Balance_To = 0;
            decimal Quantity_Balance = 0;

            Common.Get_PositiveNumber(ref Larg_Qnty);
            Common.Get_PositiveNumber(ref Small_Qnty);
            Common.Get_PositiveNumber(ref ItemNO);
            Common.Get_PositiveNumber(ref Balance_Id_From_Larg_Qnty);
            Common.Get_PositiveNumber(ref Balance_Id_From_Small_Qnty);
            Common.Get_PositiveNumber(ref Balance_Id_From_Item_No);
            Common.Get_PositiveNumber(ref Balance_Id_To_Larg_Qnty);
            Common.Get_PositiveNumber(ref Balance_Id_To_Small_Qnty);
            Common.Get_PositiveNumber(ref Balance_Id_To_Item_No);




            string mysql = "";


            Quantity = Common.Get_Item_Quantity(Larg_Qnty, Small_Qnty, S_Unit_Value);
            Quantity_Balance_From = Common.Get_Item_Quantity(Balance_Id_From_Larg_Qnty, Balance_Id_From_Small_Qnty, S_Unit_Value);
            Quantity_Balance_To = Common.Get_Item_Quantity(Balance_Id_To_Larg_Qnty, Balance_Id_To_Small_Qnty, S_Unit_Value);


            mysql = " INSERT INTO Detailes_Purshing (OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Balance_Id_From_Larg_Qnty, Balance_Id_From_Small_Qnty,Balance_Id_From_Item_No,Balance_Id_To_Larg_Qnty,Balance_Id_To_Small_Qnty,Balance_Id_To_Item_No,To_Store_Unit_Item_Table_Id , Notes_From,Notes_To,Notes,Machine_Id, Qua_Befor,Part_qua_Befor,Item_No_Befor,Qua_After,Part_qua_After,Item_No_After,Related_Detailes_Purshing_Id,Transaction_Type,Notes_Item, FromStoreId,ToStoreId) ";
            mysql = mysql + "  VALUES (" + OrderTable_id + "," + Unit_Item_Table_Id + "," + Larg_Qnty + "," + Small_Qnty + "," + ItemNO + "," + Balance_Id_From_Larg_Qnty + "," + Balance_Id_From_Small_Qnty + "," + Balance_Id_From_Item_No + "," + Balance_Id_To_Larg_Qnty + "," + Balance_Id_To_Small_Qnty + "," + Balance_Id_To_Item_No + "," + To_Store_Unit_Item_Table_Id + " , N'" + Notes_From + "',N'" + Notes_To + "',N'" + Notes + "'," + Machine_Id + " ," + Qua_Befor + "," + Part_qua_Befor + "," + Item_No_Befor + "," + Qua_After + "," + Part_qua_After + "," + Item_No_After + "," + Related_Detailes_Purshing_Id + "," + Order_Estlam_Sarf_Flage + ",N'" + Notes_Item + "'," + FromStoreId + "," + ToStoreId + ")";
            //arr.Add(mysql);
            long Detailes_Purshing_Id = opp.Insert_me_Return_Last_Identify(mysql);


            decimal Balance_Larg_Qnty = 0;
            decimal Balance_Small_Qnty = 0;
            decimal Balance_Item_No = 0;

            Balance_Larg_Qnty = Balance_Id_From_Larg_Qnty;
            Balance_Small_Qnty = Balance_Id_From_Small_Qnty;
            Balance_Item_No = Balance_Id_From_Item_No;
            string Notes_From_To = "";
            if (Order_Estlam_Sarf_Flage == "1")
            {
                qua_Import = Larg_Qnty;
                Part_Qua_Import = Small_Qnty;
                Item_No_Import = ItemNO;
                Quantity_Import = Common.Get_Item_Quantity(Larg_Qnty, Small_Qnty, S_Unit_Value);

                qua_Export = 0;
                Part_Qua_Export = 0;
                Item_No_Export = 0;
                Quantity_Export = 0;
                Quantity_Balance = Quantity_Balance_From;
                Notes_From_To = Notes_From;
                //Balance_Larg_Qnty = Balance_Id_To_Larg_Qnty;
                //Balance_Small_Qnty = Balance_Id_To_Small_Qnty;
                //Balance_Item_No = Balance_Id_To_Item_No;

            }
            else
            {

                qua_Import = 0;
                Part_Qua_Import = 0;
                Item_No_Import = 0;
                Quantity_Import = 0;

                qua_Export = Larg_Qnty;
                Part_Qua_Export = Small_Qnty;
                Item_No_Export = ItemNO;
                Quantity_Export = Common.Get_Item_Quantity(Larg_Qnty, Small_Qnty, S_Unit_Value);
                Quantity_Balance = Quantity_Balance_From;
                Notes_From_To = Notes_To;



            }
            //
            //
            ////From
            int IS_To_Converted = 0;
            long Invoice_Id = OrderTable_id;
            long Invoice_Type_Id = Order_Type;
            

            string NotesTitle = OrderTitle;

            //mysql = "INSERT INTO Detailes_Purshing_Import_Export(OrderTable_id,Detailes_Purshing_Id, Unit_item_Table_Id, qua, Part_qua, Item_No, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Notes_From_To,Notes_From,Notes_To, Notes, Machine_Id,Transaction_Type, qua_Import ,Part_Qua_Import ,Item_No_Import,Quantity_Import ,qua_Export ,Part_Qua_Export,Item_No_Export,Quantity_Export ,Quantity ,Quantity_Balance,IS_To_Converted,Process_Date,Notes_Item, Invoice_Id,Invoice_Type_Id,Invoice_Type_Str,User_Id,OperationDate) ";
            //mysql = mysql + " VALUES (" + OrderTable_id + "," + Detailes_Purshing_Id + "," + Unit_Item_Table_Id + "," + Larg_Qnty + "," + Small_Qnty + "," + ItemNO + "," + Balance_Larg_Qnty + "," + Balance_Small_Qnty + "," + Balance_Item_No + ",'" + Notes_From_To + "  -  " + Notes_To + "','" + Notes_From + "','" + Notes_To + "','" + NotesTitle + "'," + Machine_Id + "," + Order_Estlam_Sarf_Flage + "," + qua_Import + "," + Part_Qua_Import + "," + Item_No_Import + "," + Quantity_Import + "," + qua_Export + "," + Part_Qua_Export + "," + Item_No_Export + "," + Quantity_Export + "," + Quantity + "," + Quantity_Balance + "," + IS_To_Converted + ",'" + operations.dateFormat(Order_Date) + "',N'" + Notes_Item + "', " + Invoice_Id + "," + Invoice_Type_Id + ",N'" + Invoice_Type_Str + "', " + Common.User_Id + "," + operations.Get_DateTimeToInsert(DateTime.Now) + ") ";
            //opp.insert_me(mysql);

           
            Detailes_Purshing_Import_Export die = new Detailes_Purshing_Import_Export();
            die.OrderTable_id = OrderTable_id; 
            die.Detailes_Purshing_Id= Detailes_Purshing_Id; 
            die. Unit_item_Table_Id= Unit_Item_Table_Id;  
            die. qua= Larg_Qnty ; 
            die. Part_qua= Small_Qnty ; 
            die. Item_No= ItemNO ;
            die. Balance_Larg_Qnty= Balance_Larg_Qnty ; 
            die. Balance_Small_Qnty=Balance_Small_Qnty  ; 
            die. Balance_Item_No=Balance_Item_No  ;
            
            die. Machine_Id=Machine_Id  ; 
            die.Transaction_Type=int.Parse(Order_Estlam_Sarf_Flage)  ; 
            die. qua_Import = qua_Import ; 
            die.Part_Qua_Import = Part_Qua_Import ;
            die.Item_No_Import=Item_No_Import  ; 
            die.Quantity_Import =  Quantity_Import;
            die.qua_Export = qua_Export ; 
            die.Part_Qua_Export=Part_Qua_Export  ;
            die.Item_No_Export= Item_No_Export ; 
            die.Quantity_Export = Quantity_Export ; 
            die.Quantity = Quantity ;  
            die.Quantity_Balance= Quantity_Balance ; 
            die.IS_To_Converted= IS_To_Converted ; 
            die.Process_Date= Order_Date ;
           
           
            die.User_Id=  Common.User_Id ; 
            die.OperationDate=DateTime.Now ;
            die.Invoice_Id = Invoice_Id;
            die.Invoice_Type_Id = Invoice_Type_Id;
            die.Invoice_Type_Str = NotesTitle;
            die.ProcessName = NotesTitle;
            die.Notes_From_To = Notes_From_To;
            die.Notes_From = Notes_From;
            die.Notes_To = Notes_To;
            die.Notes = NotesTitle;
            die.Notes_Item = Notes_Item;

            /*die.Notes_From_To = Notes_Item;
           
            die.Notes = Notes;
            die.Notes_To = Notes_To;
            die.Notes_From = Notes_From;
            die.ProcessName = Notes;
            die.Notes_Item = Notes_Item;*/
            Detailes_Purshing_Import_ExportCLS dieCls = new Detailes_Purshing_Import_ExportCLS();
            dieCls.Add(ref opp, die);
            //if (To_Store_Unit_Item_Table_Id != -1)
            //{
            //    ////To

            //    qua_Import = Larg_Qnty;
            //    Part_Qua_Import = Small_Qnty;
            //    Item_No_Import = ItemNO;
            //    Quantity_Import = Common.Get_Item_Quantity(Larg_Qnty, Small_Qnty, S_Unit_Value);

            //    qua_Export = 0;
            //    Part_Qua_Export = 0;
            //    Item_No_Export = 0;
            //    Quantity_Export = 0;
            //   Quantity_Balance = Quantity_Balance_To;

            //      IS_To_Converted =1;
            //    mysql = "INSERT INTO Detailes_Purshing_Import_Export(OrderTable_id,Detailes_Purshing_Id, Unit_item_Table_Id, qua, Part_qua, Item_No, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Notes_From_To, Notes, Machine_Id,Transaction_Type, qua_Import ,Part_Qua_Import ,Item_No_Import,Quantity_Import ,qua_Export ,Part_Qua_Export,Item_No_Export,Quantity_Export ,Quantity ,Quantity_Balance,IS_To_Converted) ";
            //    mysql = mysql + " VALUES (" + OrderTable_id + "," + Detailes_Purshing_Id + "," + To_Store_Unit_Item_Table_Id + "," + Larg_Qnty + "," + Small_Qnty + "," + ItemNO + "," + Balance_Id_To_Larg_Qnty + "," + Balance_Id_To_Small_Qnty + "," + Balance_Id_To_Item_No + ",'" + Notes_From + "','" + Notes + "'," + Machine_Id + "," + Transaction_Type1 + "," + qua_Import + "," + Part_Qua_Import + "," + Item_No_Import + "," + Quantity_Import + "," + qua_Export + "," + Part_Qua_Export + "," + Item_No_Export + "," + Quantity_Export + "," + Quantity + "," + Quantity_Balance + ","+ IS_To_Converted + ") ";
            //    opp.insert_me(mysql);
            //}

            return Detailes_Purshing_Id;
        }
        public bool Check_ItemFoundOnStore(ref int Excp, ref string Msg,long Item_id, ref long To_Store_Unit_Item_Table_Id, ref decimal To_Store_LargBalance, ref decimal To_Store_Small_Balance, ref decimal To_Store_Item_No, long Unit_Item_Table_Id)
        {
            try
            {
                if (Type_To == 3 | Type_To == 10 | Type_To == 8)  //  Â«·ﬂ -  ‘€Ì· -  ”ÊÌ… »«·‰ﬁ’
                {
                    To_Store_Unit_Item_Table_Id = 0;
                    return true;
                }
                operations opp = new operations();
                string mysql = "";
                DataSet ds = new DataSet();
                ds = opp.return_dataset("select * from Unit_item WHERE (Item_id = " + Item_id + ") AND  (Store_id = " + Id_To + ") AND (Store_Type = " + Type_To + ")  ");

                if (ds.Tables[0].Rows.Count == 0)
                {
                    ds.Clear();
                    ds = opp.return_dataset("select * from Unit_item WHERE ( id = " + Unit_Item_Table_Id + ")");
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        mysql = " INSERT INTO Unit_item (Item_id, Code, unit_id, Full_qua, D_Qua, Item_No, Reserved_Balance_Larg_Qnty, Reserved_Balance_Small_Qnty, Big_unit, Small_unit, P_price, S_price, Specification, Upon_Request , Store_id, Store_Type, Currency) ";
                        mysql = mysql + " VALUES (" + ds.Tables[0].Rows[0]["Item_id"].ToString() + ",'" + ds.Tables[0].Rows[0]["Code"].ToString() + "'," + ds.Tables[0].Rows[0]["unit_id"].ToString() + ", 0, 0,0, 0, 0," + ds.Tables[0].Rows[0]["Big_unit"].ToString() + "," + ds.Tables[0].Rows[0]["Small_unit"].ToString() + "," + ds.Tables[0].Rows[0]["P_price"].ToString() + "," + ds.Tables[0].Rows[0]["S_price"].ToString() + " ,'" + ds.Tables[0].Rows[0]["Specification"].ToString() + "'," + ds.Tables[0].Rows[0]["Upon_Request"].ToString() + " ," + Id_To + "," + Type_To + ",'" + ds.Tables[0].Rows[0]["Currency"].ToString() + "') ";
                          Excp = 0;
                        To_Store_Unit_Item_Table_Id = opp.Insert_me_Return_Last_Identify(mysql, ref Excp);
                        To_Store_LargBalance = 0;
                        To_Store_Small_Balance = 0;
                        To_Store_Item_No = 0;
                    }
                    else
                    {
                        To_Store_Unit_Item_Table_Id = -1;
                    }

                }
                else
                {
                    To_Store_Unit_Item_Table_Id = long.Parse(ds.Tables[0].Rows[0]["id"].ToString());
                    To_Store_LargBalance = decimal.Parse(ds.Tables[0].Rows[0]["Full_qua"].ToString());
                    To_Store_Small_Balance = decimal.Parse(ds.Tables[0].Rows[0]["D_Qua"].ToString());
                    To_Store_Item_No = decimal.Parse(ds.Tables[0].Rows[0]["Item_No"].ToString());
                }
                return true;
            }
            catch (Exception ee)
            {
                 Msg=ee.Message;
                To_Store_Unit_Item_Table_Id = 0;
                return false;
            }


        }





       

        public void Update_No3eboa_To_1_From0()
        {
            try
            {
                operations opp = new operations();
                string mysql = " Update Item Set No3eboa=1 where No3eboa=0";
                opp.insert_me(mysql);
            }
            catch { }
        }
        string Get_QueryOrder()
        {

            string mysql = @"  SELECT    ' '  AS QntyStr ,     unit.u_name, unit.B_Unit, unit.S_unit, Unit_item.id, Unit_item.Item_id, Item.No3eboa, Unit_item.Store_id, Unit_item.Store_Type, Unit_item.Code, Unit_item.unit_id, Unit_item.Reserved_Balance_Larg_Qnty, 
                         Unit_item.Reserved_Balance_Small_Qnty, Unit_item.Big_unit, Unit_item.Small_unit, Unit_item.P_price, Unit_item.S_price, Unit_item.Specification, Unit_item.Upon_Request, Unit_item.Currency, Item.Item_Name, 
                         Unit_item.id AS Unit_itemTable_id, Unit_item.Item_No AS Balance_Now_Item_No, Unit_item.D_Qua AS Balance_Now_Small_Qnty, Unit_item.Full_qua AS Balance_Now_Larg_Qnty, 
                         Order_Purshing.Order_Purshing_No,Order_Purshing.NotesAll, Order_Purshing.Order_date, Order_Purshing.Order_Type, Order_Purshing.type_From, Order_Purshing.Id_From, Order_Purshing.Type_To, Order_Purshing.Id_To, 
                         Order_Purshing.Invoice_ID, Detailes_Purshing.Balance_Id_From_Larg_Qnty, Detailes_Purshing.Balance_Id_From_Small_Qnty, Detailes_Purshing.Balance_Id_From_Item_No, 
                         Detailes_Purshing.Balance_Id_To_Larg_Qnty, Detailes_Purshing.Balance_Id_To_Small_Qnty, Detailes_Purshing.Balance_Id_To_Item_No, Detailes_Purshing.Id AS Detailes_Purshing_Id, 
                         Detailes_Purshing.To_Store_Unit_Item_Table_Id, Detailes_Purshing.Unit_item_Table_Id, Detailes_Purshing.Notes_From, Detailes_Purshing.Notes_To, Detailes_Purshing.Notes, Detailes_Purshing.Qua_Befor, 
                         Detailes_Purshing.Part_qua_Befor, Detailes_Purshing.Item_No_Befor, Detailes_Purshing.Qua_After, Detailes_Purshing.Part_qua_After, Detailes_Purshing.Item_No_After, 
                         Detailes_Purshing.Item_No AS Detailes_Purshing_Item_No, Detailes_Purshing.Part_qua AS Detailes_Purshing_Part_qua, Detailes_Purshing.qua AS Detailes_Purshing_qua, unit.B_Unit_Value, unit.S_unit_Value, 
                         ROUND(Detailes_Purshing.qua + Detailes_Purshing.Part_qua / unit.S_unit_Value, 2) AS Qantity, ROUND(Detailes_Purshing.Qua_After + Detailes_Purshing.Part_qua_After / unit.S_unit_Value, 2) AS Quantity_After, 
                         ROUND(Detailes_Purshing.Qua_Befor + Detailes_Purshing.Part_qua_Befor / unit.S_unit_Value, 2) AS Quantity_Befor, ROUND((Detailes_Purshing.qua + Detailes_Purshing.Part_qua / unit.S_unit_Value) 
                         / Item.No3eboa, 2) AS Item_No_No3eboa, ROUND((Detailes_Purshing.Qua_After + Detailes_Purshing.Part_qua_After / unit.S_unit_Value) / Item.No3eboa, 2) AS Item_No_No3eboa_After, 
                         ROUND((Detailes_Purshing.Qua_Befor + Detailes_Purshing.Part_qua_Befor / unit.S_unit_Value) / Item.No3eboa, 2) AS Item_No_No3eboa_Befor, Detailes_Purshing.OrderTable_id, Detailes_Purshing.Machine_Id, 
                         Detailes_Purshing.qua AS Larg_Qnty, Detailes_Purshing.Part_qua AS Small_Qnty, Detailes_Purshing.Item_No, '' AS Order_Estlam_Sarf_Flage, '' AS QntyStr, '' AS Item_NoStr, '' AS QntyStr_After, 
                         '' AS Item_NoStr_After, '' AS QntyStr_Befor, '' AS Item_NoStr_Befor,0 AS  IS_Editable , Detailes_Purshing.Related_Detailes_Purshing_Id, Detailes_Purshing.Transaction_Type 
FROM            Item INNER JOIN
                         Unit_item ON Item.id = Unit_item.Item_id INNER JOIN
                         unit ON Unit_item.unit_id = unit.id INNER JOIN
                         Detailes_Purshing ON Unit_item.id = Detailes_Purshing.Unit_item_Table_Id INNER JOIN
                         Order_Purshing ON Detailes_Purshing.OrderTable_id = Order_Purshing.Id
            ";
            return mysql;
        }
        public DataSet Get_Order_Items_list(ref int Excp, long OrderTable_id)
        {
            string mysql;
            mysql = Get_QueryOrder();
            //            mysql = @"  SELECT        unit.u_name, unit.B_Unit, unit.S_unit, Unit_item.id, Unit_item.Item_id, Item.No3eboa, Unit_item.Store_id, Unit_item.Store_Type, Unit_item.Code, Unit_item.unit_id, Unit_item.Reserved_Balance_Larg_Qnty, 
            //                         Unit_item.Reserved_Balance_Small_Qnty, Unit_item.Big_unit, Unit_item.Small_unit, Unit_item.P_price, Unit_item.S_price, Unit_item.Specification, Unit_item.Upon_Request, Unit_item.Currency, Item.Item_Name, 
            //                         Unit_item.id AS Unit_itemTable_id, Unit_item.Item_No, Unit_item.D_Qua, Unit_item.Full_qua, Order_Purshing.Order_Purshing_No, Order_Purshing.Order_date, Order_Purshing.Order_Type, 
            //                         Order_Purshing.type_From, Order_Purshing.Id_From, Order_Purshing.Type_To, Order_Purshing.Id_To, Order_Purshing.Invoice_ID, Detailes_Purshing.Balance_Id_From_Larg_Qnty, 
            //                         Detailes_Purshing.Balance_Id_From_Small_Qnty, Detailes_Purshing.Balance_Id_From_Item_No, Detailes_Purshing.Balance_Id_To_Larg_Qnty, Detailes_Purshing.Balance_Id_To_Small_Qnty, 
            //                         Detailes_Purshing.Balance_Id_To_Item_No, Detailes_Purshing.Id AS Detailes_Purshing_Id, Detailes_Purshing.To_Store_Unit_Item_Table_Id, Detailes_Purshing.Unit_item_Table_Id, 
            //                         Detailes_Purshing.Notes_From, Detailes_Purshing.Notes_To, Detailes_Purshing.Notes, Detailes_Purshing.Qua_Befor, Detailes_Purshing.Part_qua_Befor, Detailes_Purshing.Item_No_Befor, 
            //                         Detailes_Purshing.Qua_After, Detailes_Purshing.Part_qua_After, Detailes_Purshing.Item_No_After, Detailes_Purshing.Item_No AS Detailes_Purshing_Item_No, 
            //                         Detailes_Purshing.Part_qua AS Detailes_Purshing_Part_qua, Detailes_Purshing.qua AS Detailes_Purshing_qua, unit.B_Unit_Value, unit.S_unit_Value, 
            //                         ROUND(Detailes_Purshing.qua + Detailes_Purshing.Part_qua / unit.S_unit_Value, 2) AS Qantity, ROUND(Detailes_Purshing.Qua_After + Detailes_Purshing.Part_qua_After / unit.S_unit_Value, 2) AS Quantity_After, 
            //                         ROUND(Detailes_Purshing.Qua_Befor + Detailes_Purshing.Part_qua_Befor / unit.S_unit_Value, 2) AS Quantity_Befor, ROUND((Detailes_Purshing.qua + Detailes_Purshing.Part_qua / unit.S_unit_Value) 
            //                         / Item.No3eboa, 2) AS Item_No_No3eboa, ROUND((Detailes_Purshing.Qua_After + Detailes_Purshing.Part_qua_After / unit.S_unit_Value) / Item.No3eboa, 2) AS Item_No_No3eboa_After, 
            //                         ROUND((Detailes_Purshing.Qua_Befor + Detailes_Purshing.Part_qua_Befor / unit.S_unit_Value) / Item.No3eboa, 2) AS Item_No_No3eboa_Befor

            //, Detailes_Purshing.OrderTable_id, Detailes_Purshing.Machine_Id, 
            //                         Detailes_Purshing.qua AS Larg_Qnty, Detailes_Purshing.Part_qua AS Small_Qnty, Detailes_Purshing.Item_No, '' AS Order_Estlam_Sarf_Flage, '' AS QntyStr, '' AS Item_NoStr, '' AS QntyStr_After, 
            //                         '' AS Item_NoStr_After, '' AS QntyStr_Befor, '' AS Item_NoStr_Befor,0 AS  IS_Editable , Detailes_Purshing.Related_Detailes_Purshing_Id, Detailes_Purshing.Transaction_Type 
            //FROM            Item INNER JOIN
            //                         Unit_item ON Item.id = Unit_item.Item_id INNER JOIN
            //                         unit ON Unit_item.unit_id = unit.id INNER JOIN
            //                         Detailes_Purshing ON Unit_item.id = Detailes_Purshing.Unit_item_Table_Id INNER JOIN
            //                         Order_Purshing ON Detailes_Purshing.OrderTable_id = Order_Purshing.Id
            //            ";
            mysql = mysql + " WHERE (Detailes_Purshing.OrderTable_id = " + OrderTable_id + ")  ";
            //, Detailes_Purshing.Related_Detailes_Purshing_Id, Detailes_Purshing.Transaction_Type 
            //  mysql = mysql + " AND (Detailes_Purshing.Flage_Not_Show <= 0)  ";


            return Get_Items_Data(ref Excp, mysql);

        }
        public DataTable Get_Order_Items_list(ref int Excp, ref operations opp, long OrderTable_id)
        {
            string mysql;
            mysql = Get_QueryOrder();

            mysql = mysql + " WHERE (Detailes_Purshing.OrderTable_id = " + OrderTable_id + ")  ";


            return Get_Items_DataTable(ref Excp, ref opp, mysql);

        }
        public DataTable Get_Order_Detailes_Purshing_Id(ref int Excp, ref operations opp, long OrderDetailes_Table_id)
        {
            string mysql;
            mysql = Get_QueryOrder();
            //            mysql = @"  SELECT        unit.u_name, unit.B_Unit, unit.S_unit, Unit_item.id, Unit_item.Item_id, Item.No3eboa, Unit_item.Store_id, Unit_item.Store_Type, Unit_item.Code, Unit_item.unit_id, Unit_item.Reserved_Balance_Larg_Qnty, 
            //                         Unit_item.Reserved_Balance_Small_Qnty, Unit_item.Big_unit, Unit_item.Small_unit, Unit_item.P_price, Unit_item.S_price, Unit_item.Specification, Unit_item.Upon_Request, Unit_item.Currency, Item.Item_Name, 
            //                         Unit_item.id AS Unit_itemTable_id, Unit_item.Item_No AS Balance_Now_Item_No, Unit_item.D_Qua AS Balance_Now_Small_Qnty, Unit_item.Full_qua AS Balance_Now_Larg_Qnty, 
            //                         Order_Purshing.Order_Purshing_No, Order_Purshing.Order_date, Order_Purshing.Order_Type, Order_Purshing.type_From, Order_Purshing.Id_From, Order_Purshing.Type_To, Order_Purshing.Id_To, 
            //                         Order_Purshing.Invoice_ID, Detailes_Purshing.Balance_Id_From_Larg_Qnty, Detailes_Purshing.Balance_Id_From_Small_Qnty, Detailes_Purshing.Balance_Id_From_Item_No, 
            //                         Detailes_Purshing.Balance_Id_To_Larg_Qnty, Detailes_Purshing.Balance_Id_To_Small_Qnty, Detailes_Purshing.Balance_Id_To_Item_No, Detailes_Purshing.Id AS Detailes_Purshing_Id, 
            //                         Detailes_Purshing.To_Store_Unit_Item_Table_Id, Detailes_Purshing.Unit_item_Table_Id, Detailes_Purshing.Notes_From, Detailes_Purshing.Notes_To, Detailes_Purshing.Notes, Detailes_Purshing.Qua_Befor, 
            //                         Detailes_Purshing.Part_qua_Befor, Detailes_Purshing.Item_No_Befor, Detailes_Purshing.Qua_After, Detailes_Purshing.Part_qua_After, Detailes_Purshing.Item_No_After, 
            //                         Detailes_Purshing.Item_No AS Detailes_Purshing_Item_No, Detailes_Purshing.Part_qua AS Detailes_Purshing_Part_qua, Detailes_Purshing.qua AS Detailes_Purshing_qua, unit.B_Unit_Value, unit.S_unit_Value, 
            //                         ROUND(Detailes_Purshing.qua + Detailes_Purshing.Part_qua / unit.S_unit_Value, 2) AS Qantity, ROUND(Detailes_Purshing.Qua_After + Detailes_Purshing.Part_qua_After / unit.S_unit_Value, 2) AS Quantity_After, 
            //                         ROUND(Detailes_Purshing.Qua_Befor + Detailes_Purshing.Part_qua_Befor / unit.S_unit_Value, 2) AS Quantity_Befor, ROUND((Detailes_Purshing.qua + Detailes_Purshing.Part_qua / unit.S_unit_Value) 
            //                         / Item.No3eboa, 2) AS Item_No_No3eboa, ROUND((Detailes_Purshing.Qua_After + Detailes_Purshing.Part_qua_After / unit.S_unit_Value) / Item.No3eboa, 2) AS Item_No_No3eboa_After, 
            //                         ROUND((Detailes_Purshing.Qua_Befor + Detailes_Purshing.Part_qua_Befor / unit.S_unit_Value) / Item.No3eboa, 2) AS Item_No_No3eboa_Befor, Detailes_Purshing.OrderTable_id, Detailes_Purshing.Machine_Id, 
            //                         Detailes_Purshing.qua AS Larg_Qnty, Detailes_Purshing.Part_qua AS Small_Qnty, Detailes_Purshing.Item_No, '' AS Order_Estlam_Sarf_Flage, '' AS QntyStr, '' AS Item_NoStr, '' AS QntyStr_After, 
            //                         '' AS Item_NoStr_After, '' AS QntyStr_Befor, '' AS Item_NoStr_Befor,0 AS  IS_Editable , Detailes_Purshing.Related_Detailes_Purshing_Id, Detailes_Purshing.Transaction_Type 
            //FROM            Item INNER JOIN
            //                         Unit_item ON Item.id = Unit_item.Item_id INNER JOIN
            //                         unit ON Unit_item.unit_id = unit.id INNER JOIN
            //                         Detailes_Purshing ON Unit_item.id = Detailes_Purshing.Unit_item_Table_Id INNER JOIN
            //                         Order_Purshing ON Detailes_Purshing.OrderTable_id = Order_Purshing.Id
            //            ";
            mysql = mysql + " WHERE (Detailes_Purshing.Id = " + OrderDetailes_Table_id + ")  ";

            //  mysql = mysql + " AND (Detailes_Purshing.Flage_Not_Show <= 0)  ";


            return Get_Items_DataTable(ref Excp, ref opp, mysql);

        }
        public DataTable Get_Order_Items_listEdit(ref int Excp, long OrderTable_id)
        {
            string mysql;
            mysql = @"  SELECT    Store.Store_Name,    unit.u_name, unit.B_Unit, unit.S_unit, Unit_item.id, Unit_item.Item_id, Item.No3eboa, Unit_item.Store_id, Unit_item.Store_Type, Unit_item.Code, Unit_item.unit_id, Unit_item.Reserved_Balance_Larg_Qnty, 
                         Unit_item.Reserved_Balance_Small_Qnty, Unit_item.Big_unit, Unit_item.Small_unit, Unit_item.P_price, Unit_item.S_price, Unit_item.Specification, Unit_item.Upon_Request, Unit_item.Currency, Item.Item_Name, 
                         Unit_item.id AS Unit_itemTable_id, Unit_item.Item_No AS Balance_Now_Item_No, Unit_item.D_Qua AS Balance_Now_Small_Qnty, Unit_item.Full_qua AS Balance_Now_Larg_Qnty, 
                         Order_Purshing.Order_Purshing_No,Order_Purshing.NotesAll, Order_Purshing.Order_date, Order_Purshing.Order_Type, Order_Purshing.type_From, Order_Purshing.Id_From, Order_Purshing.Type_To, Order_Purshing.Id_To, 
                         Order_Purshing.Invoice_ID, Detailes_Purshing.Balance_Id_From_Larg_Qnty, Detailes_Purshing.Balance_Id_From_Small_Qnty, Detailes_Purshing.Balance_Id_From_Item_No, 
                         Detailes_Purshing.Balance_Id_To_Larg_Qnty, Detailes_Purshing.Balance_Id_To_Small_Qnty, Detailes_Purshing.Balance_Id_To_Item_No, Detailes_Purshing.Id AS Detailes_Purshing_Id, 
                         Detailes_Purshing.To_Store_Unit_Item_Table_Id, Detailes_Purshing.Unit_item_Table_Id, Detailes_Purshing.Notes_From, Detailes_Purshing.Notes_To, Detailes_Purshing.Notes, Detailes_Purshing.Qua_Befor, 
                         Detailes_Purshing.Part_qua_Befor, Detailes_Purshing.Item_No_Befor, Detailes_Purshing.Qua_After, Detailes_Purshing.Part_qua_After, Detailes_Purshing.Item_No_After, 
                         Detailes_Purshing.Item_No AS Detailes_Purshing_Item_No, Detailes_Purshing.Part_qua AS Detailes_Purshing_Part_qua, Detailes_Purshing.qua AS Detailes_Purshing_qua, unit.B_Unit_Value, unit.S_unit_Value, 
                         ROUND(Detailes_Purshing.qua + Detailes_Purshing.Part_qua / unit.S_unit_Value, 2) AS Qantity, ROUND(Detailes_Purshing.Qua_After + Detailes_Purshing.Part_qua_After / unit.S_unit_Value, 2) AS Quantity_After, 
                         ROUND(Detailes_Purshing.Qua_Befor + Detailes_Purshing.Part_qua_Befor / unit.S_unit_Value, 2) AS Quantity_Befor, ROUND((Detailes_Purshing.qua + Detailes_Purshing.Part_qua / unit.S_unit_Value) 
                         / Item.No3eboa, 2) AS Item_No_No3eboa, ROUND((Detailes_Purshing.Qua_After + Detailes_Purshing.Part_qua_After / unit.S_unit_Value) / Item.No3eboa, 2) AS Item_No_No3eboa_After, 
                         ROUND((Detailes_Purshing.Qua_Befor + Detailes_Purshing.Part_qua_Befor / unit.S_unit_Value) / Item.No3eboa, 2) AS Item_No_No3eboa_Befor, Detailes_Purshing.OrderTable_id, Detailes_Purshing.Machine_Id, 
                         Detailes_Purshing.qua AS Larg_Qnty, Detailes_Purshing.Part_qua AS Small_Qnty, Detailes_Purshing.Item_No, '' AS Order_Estlam_Sarf_Flage, '' AS QntyStr, '' AS Item_NoStr, '' AS QntyStr_After, 
                         '' AS Item_NoStr_After, '' AS QntyStr_Befor, '' AS Item_NoStr_Befor,0 AS  IS_Editable , Detailes_Purshing.Related_Detailes_Purshing_Id, Detailes_Purshing.Transaction_Type ,    Order_Purshing.MainOrderTable_id,
Detailes_Purshing.Notes_Item
FROM            Item INNER JOIN
                         Unit_item ON Item.id = Unit_item.Item_id INNER JOIN
                         unit ON Unit_item.unit_id = unit.id INNER JOIN
                         Detailes_Purshing ON Unit_item.id = Detailes_Purshing.Unit_item_Table_Id INNER JOIN
                         Order_Purshing ON Detailes_Purshing.OrderTable_id = Order_Purshing.Id INNER JOIN
                         Store ON Unit_item.Store_id = Store.id
            ";
            mysql = mysql + " WHERE (Detailes_Purshing.OrderTable_id = " + OrderTable_id + ")  ";

            //  mysql = mysql + " AND (Detailes_Purshing.Flage_Not_Show <= 0)  ";


            return Get_Items_DataTable(ref Excp, mysql);

        }
        public DataTable Get_Item_Raw_MaterialsFor_Entage(ref int Excp, ref operations opp, long OrderTable_id, long Item_Id)
        {
            string mysql;
            mysql = @"  SELECT        Item_Raw_MaterialsFor_Entage.MainItem_Id, Order_Purshing.*, Detailes_Purshing.*, Detailes_Purshing.Id AS Detailes_Purshing_Id
,       Item_Raw_MaterialsFor_Entage.Id AS Item_Raw_MaterialsFor_Entage_Id 
,Detailes_Purshing.qua AS Detailes_Purshing_qua, Detailes_Purshing.Part_qua AS Detailes_Purshing_Part_qua, Detailes_Purshing.Item_No AS Detailes_Purshing_Item_No, 
                         Item_Raw_MaterialsFor_Entage.S_unit_Value, Item_Raw_MaterialsFor_Entage.SubItem_Id AS Item_id
FROM            Item_Raw_MaterialsFor_Entage INNER JOIN
                         Order_Purshing ON Item_Raw_MaterialsFor_Entage.SubOrderTable_id = Order_Purshing.Id INNER JOIN
                         Detailes_Purshing ON Detailes_Purshing.OrderTable_id = Order_Purshing.Id

            ";
            mysql = mysql + " WHERE        (Item_Raw_MaterialsFor_Entage.MainOrderTable_id = " + OrderTable_id + ") AND (Item_Raw_MaterialsFor_Entage.MainItem_Id = " + Item_Id + ") ";

            //  mysql = mysql + " AND (Detailes_Purshing.Flage_Not_Show <= 0)  ";


            return Get_Items_DataTable(ref Excp, ref opp, mysql);

        }
        public DataTable Get_Item_Raw_MaterialsFor_Entage(ref int Excp, long OrderTable_id)
        {
            string mysql;
            mysql = @"  SELECT        Item.S_id AS MainStore_Id, Item_Raw_MaterialsFor_Entage.Id, Item_Raw_MaterialsFor_Entage.MainOrderTable_id, Item_Raw_MaterialsFor_Entage.SubOrderTable_id, Item_Raw_MaterialsFor_Entage.Process_Date, 
                         Item_Raw_MaterialsFor_Entage.MainItem_Id, Item_Raw_MaterialsFor_Entage.SubItem_Id, Item_Raw_MaterialsFor_Entage.MainItem_Name, Item_Raw_MaterialsFor_Entage.SubItem_Name, 
                         Item_Raw_MaterialsFor_Entage.Unit_Id, Item_Raw_MaterialsFor_Entage.S_unit_Value, Item_Raw_MaterialsFor_Entage.Raw_Full_qua, Item_Raw_MaterialsFor_Entage.Raw_D_Qua, 
                         Item_Raw_MaterialsFor_Entage.Raw_Quantity, Item_Raw_MaterialsFor_Entage.Larg_Qnty, Item_Raw_MaterialsFor_Entage.Small_Qnty, Item_Raw_MaterialsFor_Entage.Qnty, 
                         Item_Raw_MaterialsFor_Entage.Main_Larg_Qnty, Item_Raw_MaterialsFor_Entage.Main_Small_Qnty, Item_Raw_MaterialsFor_Entage.Main_Qnty, Item_Raw_MaterialsFor_Entage.Raw_Full_qua AS Full_qua, 
                         Item_Raw_MaterialsFor_Entage.Raw_D_Qua AS D_Qua, Item_Raw_MaterialsFor_Entage.Raw_Quantity AS Quantity, Item_Raw_MaterialsFor_Entage.Main_Larg_Qnty AS Entry_Larg_Qnty, 
                         Item_Raw_MaterialsFor_Entage.Main_Small_Qnty AS Entry_Small_Qnty, Item_Raw_MaterialsFor_Entage.Main_Qnty AS Entry_Qnty, Unit_item.Full_qua AS SubFull_qua, Unit_item.D_Qua AS SubD_Qua, 
                         Unit_item.Item_No AS SubItem_No, Item_Raw_MaterialsFor_Entage.Big_unit, Item_Raw_MaterialsFor_Entage.Small_unit, 0 AS SubQntyBalance, 1 AS CheckBalanceAllows, Unit_item.id AS SubUnit_item_id,0 AS  IS_Editable, 0 AS Entry_Edit_Diff,0 AS Raw_Diff_Balance,0 AS Qnty_AfterEdit , Cast(Item_Raw_MaterialsFor_Entage.SubStore_Id as int ) AS S_Id, Item_Raw_MaterialsFor_Entage.SubStore_Name AS Store_Name , Item_Raw_MaterialsFor_Entage.SubUnit_item_id 
FROM            Unit_item INNER JOIN
                         Item_Raw_MaterialsFor_Entage ON Unit_item.id = Item_Raw_MaterialsFor_Entage.SubUnit_item_id INNER JOIN
                         Item ON Unit_item.Item_id = Item.id
            ";
            mysql = mysql + " WHERE (Item_Raw_MaterialsFor_Entage.MainOrderTable_id = " + OrderTable_id + ")  ";

            //  mysql = mysql + " AND (Detailes_Purshing.Flage_Not_Show <= 0)  ";


            return Get_Items_DataTable(ref Excp, mysql);

        }
        public DataSet Get_Order_Items_listFromInvoice(ref int Excp, ArrayList arr_OrderTable_id)
        {
            string mysql;
            mysql = @"  SELECT        unit.u_name, unit.B_Unit, unit.S_unit, Unit_item.id, Unit_item.Item_id, Item.No3eboa, Unit_item.Store_id, Unit_item.Store_Type, Unit_item.Code, Unit_item.unit_id, Unit_item.Reserved_Balance_Larg_Qnty, 
                         Unit_item.Reserved_Balance_Small_Qnty, Unit_item.Big_unit, Unit_item.Small_unit, Unit_item.P_price, Unit_item.S_price, Unit_item.Specification, Unit_item.Upon_Request, Unit_item.Currency, Item.Item_Name, 
                         Unit_item.id AS Unit_itemTable_id, Unit_item.Item_No, Unit_item.D_Qua, Unit_item.Full_qua, Order_Purshing.Order_Purshing_No, Order_Purshing.Order_date, Order_Purshing.Order_Type, 
                         Order_Purshing.type_From, Order_Purshing.Id_From, Order_Purshing.Type_To, Order_Purshing.Id_To, Order_Purshing.Invoice_ID, Detailes_Purshing.Balance_Id_From_Larg_Qnty, 
                         Detailes_Purshing.Balance_Id_From_Small_Qnty, Detailes_Purshing.Balance_Id_From_Item_No, Detailes_Purshing.Balance_Id_To_Larg_Qnty, Detailes_Purshing.Balance_Id_To_Small_Qnty, 
                         Detailes_Purshing.Balance_Id_To_Item_No, Detailes_Purshing.Id AS Detailes_Purshing_Id, Detailes_Purshing.To_Store_Unit_Item_Table_Id, Detailes_Purshing.Unit_item_Table_Id, 
                         Detailes_Purshing.Notes_From, Detailes_Purshing.Notes_To, Detailes_Purshing.Notes, Detailes_Purshing.Qua_Befor, Detailes_Purshing.Part_qua_Befor, Detailes_Purshing.Item_No_Befor, 
                         Detailes_Purshing.Qua_After, Detailes_Purshing.Part_qua_After, Detailes_Purshing.Item_No_After, Detailes_Purshing.Item_No AS Detailes_Purshing_Item_No, 
                         Detailes_Purshing.Part_qua AS Detailes_Purshing_Part_qua, Detailes_Purshing.qua AS Detailes_Purshing_qua, unit.B_Unit_Value, unit.S_unit_Value, 
                         ROUND(Detailes_Purshing.qua + Detailes_Purshing.Part_qua / unit.S_unit_Value, 2) AS Qantity, ROUND(Detailes_Purshing.Qua_After + Detailes_Purshing.Part_qua_After / unit.S_unit_Value, 2) AS Quantity_After, 
                         ROUND(Detailes_Purshing.Qua_Befor + Detailes_Purshing.Part_qua_Befor / unit.S_unit_Value, 2) AS Quantity_Befor, ROUND((Detailes_Purshing.qua + Detailes_Purshing.Part_qua / unit.S_unit_Value) 
                         / Item.No3eboa, 2) AS Item_No_No3eboa, ROUND((Detailes_Purshing.Qua_After + Detailes_Purshing.Part_qua_After / unit.S_unit_Value) / Item.No3eboa, 2) AS Item_No_No3eboa_After, 
                         ROUND((Detailes_Purshing.Qua_Befor + Detailes_Purshing.Part_qua_Befor / unit.S_unit_Value) / Item.No3eboa, 2) AS Item_No_No3eboa_Befor, Store.Store_Name
FROM            Item INNER JOIN
                         Unit_item ON Item.id = Unit_item.Item_id INNER JOIN
                         unit ON Unit_item.unit_id = unit.id INNER JOIN
                         Detailes_Purshing ON Unit_item.id = Detailes_Purshing.Unit_item_Table_Id INNER JOIN
                         Order_Purshing ON Detailes_Purshing.OrderTable_id = Order_Purshing.Id INNER JOIN
                         Store ON Order_Purshing.Id_From = Store.id
            ";
            mysql = mysql + " WHERE (Detailes_Purshing.OrderTable_id = " + arr_OrderTable_id[0] + ")  ";

            for (int x = 1; x < arr_OrderTable_id.Count; x++)
            {
                mysql = mysql + " OR  (Detailes_Purshing.OrderTable_id = " + arr_OrderTable_id[x] + ")";
            }

            mysql = mysql + " Order by  Detailes_Purshing.OrderTable_id ";


            return Get_Items_Data(ref Excp, mysql);

        }
        public DataSet Get_All_OrderData(ref int Excp, long Order_ID)
        {
            string mysql = " SELECT * FROM Order_Purshing ";
            mysql = mysql + "  WHERE (Id = " + Order_ID + ")";
            return Get_Items_Data(ref Excp, mysql);

        }

        public DataTable Get_Item_Raw_MaterialsFor_Entage_Details(ref int Excp, long Item_Raw_MaterialsFor_Entage_List_ID)
        {
            string mysql = " SELECT *  FROM Item_Raw_MaterialsFor_Entage_Details ";
            mysql = mysql + "  WHERE (Item_Raw_MaterialsFor_Entage_List_Id = " + Item_Raw_MaterialsFor_Entage_List_ID + ")";

            DataTable dt = Get_Items_DataTable(ref Excp, mysql);
            return dt;
        }
        public DataTable Get_Item_Raw_MaterialsFor_Entage_List(ref int Excp, long Item_Raw_MaterialsFor_Entage_List_ID)
        {
            string mysql = " SELECT *  FROM Item_Raw_MaterialsFor_Entage_List ";
            mysql = mysql + "  WHERE (Id = " + Item_Raw_MaterialsFor_Entage_List_ID + ")";

            DataTable dt = Get_Items_DataTable(ref Excp, mysql);
            return dt;
        }
        public DataTable Get_Item_Raw_MaterialsFor_Entage_Details(ref int Excp, long Item_Raw_MaterialsFor_Entage_List_ID, string Order_Estlam_Sarf_Flage)
        {
            string mysql = " SELECT Distinct Order_Purshing_Id  FROM Item_Raw_MaterialsFor_Entage_Details ";
            mysql = mysql + "  WHERE (Item_Raw_MaterialsFor_Entage_List_Id = " + Item_Raw_MaterialsFor_Entage_List_ID + ") AND Order_Estlam_Sarf_Flage=" + Order_Estlam_Sarf_Flage + "";

            DataTable dt = Get_Items_DataTable(ref Excp, mysql);
            return dt;
        }
        public DataTable Get_Item_Raw_MaterialsFor_Entage_Details(ref int Excp, ref operations opp, long Item_Raw_MaterialsFor_Entage_List_ID)
        {
            string mysql = " SELECT Distinct Order_Purshing_Id ,Order_Estlam_Sarf_Flage FROM Item_Raw_MaterialsFor_Entage_Details ";
            mysql = mysql + "  WHERE (Item_Raw_MaterialsFor_Entage_List_Id = " + Item_Raw_MaterialsFor_Entage_List_ID + ")  ";

            DataTable dt = Get_Items_DataTable(ref Excp, ref opp, mysql);
            return dt;
        }
        private DataSet Get_Items_Data(ref int Excp, string mysql)
        {
            DataSet G_DS = new DataSet();
            try
            {
                operations opp = new operations();
                G_DS = opp.return_dataset(mysql);
                Excp = 1;
                opp = null;
                return G_DS;
            }
            catch (Exception ee)
            {

                Excp = 1;
                //MessageBox.Show(ee.Message);
                return G_DS;
            }
        }
        private DataTable Get_Items_DataTable(ref int Excp, ref operations opp, string mysql)
        {
            DataTable dt = new DataTable();
            try
            {

                dt = opp.return_DataTable(mysql);
                Excp = 0;

                return dt;
            }
            catch (Exception ee)
            {

                Excp = 1;
                //MessageBox.Show(ee.Message);
                return dt;
            }
        }
        private DataTable Get_Items_DataTable(ref int Excp, string mysql)
        {
            DataTable dt = new DataTable();
            try
            {
                operations opp = new operations();
                dt = opp.return_DataTable(mysql);
                Excp = 0;
                opp = null;
                return dt;
            }
            catch (Exception ee)
            {

                Excp = 1;
                //MessageBox.Show(ee.Message);
                return dt;
            }
        }

        public void Delete_Order_Details(ref int Excp, long OrderDetailes_Table_id, long Unit_Item_Table_Id, long To_Store_Unit_Item_Table_Id, decimal Larg_Qnty, decimal Small_Qnty, decimal ItemNO, decimal S_unit_Value, long Item_id)
        {
            string mysql = "";
            operations opp = new operations();
            DataSet dss;

            ArrayList arr = new ArrayList();

            try
            {
                #region Transform           ÕÊÌ·
                if (Order_Type == 1 | Order_Type == 6)             //«· ÕÊÌ·
                {
                    mysql = " Delete from Detailes_Purshing  where (id=" + OrderDetailes_Table_id + ") ";
                    //  mysql = " UPDATE Detailes_Purshing Set Flage_Not_Show=1 where (id=" + OrderDetailes_Table_id + ") ";
                    arr.Add(mysql);

                    mysql = " Delete from Detailes_Purshing_Import_Export  where (Detailes_Purshing_Id=" + OrderDetailes_Table_id + ") ";
                    //  mysql = " UPDATE Detailes_Purshing Set Flage_Not_Show=1 where (id=" + OrderDetailes_Table_id + ") ";
                    arr.Add(mysql);

                    //// ⁄œÌ· »Ì«‰«  «·„Œ“‰ «·„‰’—› „‰… »“Ì«œ… «·—’Ìœ
                    //Unit_Item_Table_Id
                    mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                    arr.Add(mysql);


                    //// ⁄œÌ· »Ì«‰«  «·„Œ“‰ «·„‰’—› ≈·Ì… »‰ﬁ’  «·—’Ìœ
                    //To_Store_Unit_Item_Table_Id
                    mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + ItemNO + "  WHERE (id = " + To_Store_Unit_Item_Table_Id + ") ";
                    arr.Add(mysql);

                }
                #endregion

                #region     sarf or mortaga3 estlam
                else if (Order_Type == 2 | Order_Type == 5)
                {
                    mysql = " Delete from Detailes_Purshing  where (id=" + OrderDetailes_Table_id + ") ";
                    //   mysql = " UPDATE Detailes_Purshing Set Flage_Not_Show=1 where (id=" + OrderDetailes_Table_id + ") ";
                    arr.Add(mysql);

                    //// ⁄œÌ· »Ì«‰«  «·„Œ“‰ «·„‰’—› „‰… »“Ì«œ… «·—’Ìœ
                    //Unit_Item_Table_Id
                    mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                    arr.Add(mysql);


                }
                #endregion

                #region    mortaga3 sarf or  estlam
                else if (Order_Type == 3 | Order_Type == 4 | Order_Type == 9)
                {
                    mysql = " Delete from Detailes_Purshing  where (id=" + OrderDetailes_Table_id + ") ";
                    //  mysql = " UPDATE Detailes_Purshing Set Flage_Not_Show=1 where (id=" + OrderDetailes_Table_id + ") ";
                    arr.Add(mysql);

                    //// ⁄œÌ· »Ì«‰«  «·„Œ“‰ «·„‰’—› ≈·Ì… »‰ﬁ’  «·—’Ìœ
                    //To_Store_Unit_Item_Table_Id
                    mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + ItemNO + "  WHERE (id = " + Unit_Item_Table_Id + ") ";
                    arr.Add(mysql);
                }
                #endregion

                #region Edit Rest

                mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty + " + Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty + " + Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  + " + ItemNO + "  where (id>" + OrderDetailes_Table_id + ") AND Unit_item_Table_Id=" + Unit_Item_Table_Id + " ";
                arr.Add(mysql);

                mysql = " UPDATE  Detailes_Purshing SET  Balance_Id_To_Larg_Qnty=Balance_Id_To_Larg_Qnty  -  " + Larg_Qnty + ",Balance_Id_To_Small_Qnty=Balance_Id_To_Small_Qnty  - " + Small_Qnty + ",Balance_Id_To_Item_No=Balance_Id_To_Item_No - " + ItemNO + "  where (id>" + OrderDetailes_Table_id + ")  AND To_Store_Unit_Item_Table_Id=" + To_Store_Unit_Item_Table_Id + "  ";
                arr.Add(mysql);


                mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty - " + Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty - " + Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  - " + ItemNO + "  where (id>" + OrderDetailes_Table_id + ") AND Unit_item_Table_Id=" + To_Store_Unit_Item_Table_Id + " ";
                arr.Add(mysql);




                mysql = " UPDATE  Detailes_Purshing SET  Balance_Id_To_Larg_Qnty=Balance_Id_To_Larg_Qnty  +  " + Larg_Qnty + ",Balance_Id_To_Small_Qnty=Balance_Id_To_Small_Qnty  + " + Small_Qnty + ",Balance_Id_To_Item_No=Balance_Id_To_Item_No + " + ItemNO + "  where (id>" + OrderDetailes_Table_id + ")  AND To_Store_Unit_Item_Table_Id=" + Unit_Item_Table_Id + "  ";
                arr.Add(mysql);

                ////////

                mysql = " UPDATE Detailes_Purshing SET Balance_Id_From_Larg_Qnty = FLOOR((" + S_unit_Value + " * Balance_Id_From_Larg_Qnty + Balance_Id_From_Small_Qnty) / " + S_unit_Value + ") ,  ";
                mysql = mysql + " Balance_Id_From_Small_Qnty =FlOOR(((" + S_unit_Value + " * Balance_Id_From_Larg_Qnty + Balance_Id_From_Small_Qnty)   / " + S_unit_Value + " - FLOOR((" + S_unit_Value + " * Balance_Id_From_Larg_Qnty + Balance_Id_From_Small_Qnty) / " + S_unit_Value + ")) * " + S_unit_Value + " )";
                arr.Add(mysql);

                mysql = " UPDATE Detailes_Purshing SET Balance_Id_To_Larg_Qnty = FLOOR((" + S_unit_Value + " * Balance_Id_To_Larg_Qnty + Balance_Id_To_Small_Qnty) / " + S_unit_Value + ") ,  ";
                mysql = mysql + " Balance_Id_To_Small_Qnty = FLOOR(((" + S_unit_Value + " * Balance_Id_To_Larg_Qnty + Balance_Id_To_Small_Qnty)   / " + S_unit_Value + " - FLOOR((" + S_unit_Value + " * Balance_Id_To_Larg_Qnty + Balance_Id_To_Small_Qnty) / " + S_unit_Value + ")) * " + S_unit_Value + " )";
                arr.Add(mysql);


                #endregion
                int Status = opp.insert_me_transaction(arr);
                if (Status == 1)
                {
                    Excp = 0;
                }
                else
                {
                    Excp = 1;
                }
                opp = null;

            }
            catch (Exception e)
            {
                opp = null;
                Excp = 1;
                throw (e);
            }

        }
        public void Delete_Item_Raw_MaterialsFor_Entage_FromEdit(ref int Excp, ref operations opp, long Item_id, long OrderDetailes_Table_id)
        {
            DataTable dt = Get_Item_Raw_MaterialsFor_Entage(ref Excp, ref opp, OrderDetailes_Table_id, Item_id);

            for (int x = 0; x < dt.Rows.Count; x++)
            {
                //////delete sub raw item from details order
                Delete_Order_DetailsFromEdit(ref Excp, ref opp, dt, x);
                string Item_Raw_MaterialsFor_Entage_Id = dt.Rows[x]["Item_Raw_MaterialsFor_Entage_Id"].ToString();

                string mysql = "Delete From Item_Raw_MaterialsFor_Entage Where Id = " + Item_Raw_MaterialsFor_Entage_Id;
                opp.insert_me(mysql);
            }

        }
        public void Delete_Order_DetailsFromEdit(ref int Excp, ref operations opp, DataTable dt_NewOrder, int index)
        {
            long Order_Purshing_Id = long.Parse(dt_NewOrder.Rows[index]["OrderTable_id"].ToString());
            long Detailes_Purshing_Id = long.Parse(dt_NewOrder.Rows[index]["Detailes_Purshing_Id"].ToString());
            long Unit_Item_Table_Id = long.Parse(dt_NewOrder.Rows[index]["Unit_item_Table_Id"].ToString());
            long To_Store_Unit_Item_Table_Id = long.Parse(dt_NewOrder.Rows[index]["To_Store_Unit_Item_Table_Id"].ToString());
            decimal Larg_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Detailes_Purshing_qua"].ToString());
            decimal Small_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Detailes_Purshing_Part_qua"].ToString());
            decimal ItemNO = decimal.Parse(dt_NewOrder.Rows[index]["Detailes_Purshing_Item_No"].ToString());
            decimal S_unit_Value = decimal.Parse(dt_NewOrder.Rows[index]["S_unit_Value"].ToString());
            long Item_Id = long.Parse(dt_NewOrder.Rows[index]["Item_id"].ToString());
            int Order_Type = int.Parse(dt_NewOrder.Rows[index]["Order_Type"].ToString());
            int Transaction_Type = int.Parse(dt_NewOrder.Rows[index]["Transaction_Type"].ToString());
            long Related_Detailes_Purshing_Id = long.Parse(dt_NewOrder.Rows[index]["Related_Detailes_Purshing_Id"].ToString());
            long MainOrderTable_id = 0;
            try { MainOrderTable_id = long.Parse(dt_NewOrder.Rows[index]["MainOrderTable_id"].ToString()); }
            catch { }
            string mysql = "";



            try
            {
                #region Transform           ÕÊÌ·
                if (Order_Type == 1 | Order_Type == 6)             //«· ÕÊÌ·
                {
                    /////////////Õ–› «·’‰› «·–Ï  „  ÕœÌœ Õ–›…
                    mysql = " Delete from Detailes_Purshing  where (id=" + Detailes_Purshing_Id + ") ";
                    opp.insert_me(mysql);

                    mysql = " Delete from Detailes_Purshing_Import_Export  where (Detailes_Purshing_Id=" + Detailes_Purshing_Id + ") ";
                    opp.insert_me(mysql);

                    /////////////Õ–› «·’‰› «·–Ï  „  ÕÊÌ·Â „‰ «·«–‰ „»«‘—
                    mysql = " Delete from Detailes_Purshing  where (  Id=" + Related_Detailes_Purshing_Id + ") ";
                    opp.insert_me(mysql);

                    mysql = " Delete from Detailes_Purshing_Import_Export  where (Detailes_Purshing_Id=" + Related_Detailes_Purshing_Id + ") ";
                    opp.insert_me(mysql);


                    if (Transaction_Type == 1)
                    {

                        //// ⁄œÌ· »Ì«‰«  «·„Œ“‰ «·„‰’—› ≈·Ì… »‰ﬁ’  «·—’Ìœ
                        //To_Store_Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + ItemNO + "  WHERE (id = " + Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                        //// ⁄œÌ· »Ì«‰«  «·„Œ“‰ «·„‰’—› „‰… »“Ì«œ… «·—’Ìœ
                        //Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + " WHERE (id = " + To_Store_Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);



                    }
                    else
                    {

                        //// ⁄œÌ· »Ì«‰«  «·„Œ“‰ «·„‰’—› ≈·Ì… »‰ﬁ’  «·—’Ìœ
                        //To_Store_Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + ItemNO + "  WHERE (id = " + To_Store_Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                        //// ⁄œÌ· »Ì«‰«  «·„Œ“‰ «·„‰’—› „‰… »“Ì«œ… «·—’Ìœ
                        //Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);



                    }

                }
                #endregion

                #region     sarf or mortaga3 estlam
                //else if (Order_Type == 2 | Order_Type == 5)
                else if (Transaction_Type == 2)
                {
                    mysql = " Delete from Detailes_Purshing  where (id=" + Detailes_Purshing_Id + ") ";
                    //   mysql = " UPDATE Detailes_Purshing Set Flage_Not_Show=1 where (id=" + Detailes_Purshing_Id + ") ";
                    opp.insert_me(mysql);

                    mysql = " Delete from Detailes_Purshing_Import_Export  where (Detailes_Purshing_Id=" + Detailes_Purshing_Id + ") ";
                    opp.insert_me(mysql);
                    //// ⁄œÌ· »Ì«‰«  «·„Œ“‰ «·„‰’—› „‰… »“Ì«œ… «·—’Ìœ
                    //Unit_Item_Table_Id
                    mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                    opp.insert_me(mysql);


                }
                #endregion

                #region    mortaga3 sarf or  estlam
                //else if (Order_Type == 3 | Order_Type == 4 | Order_Type == 9)
                else if (Transaction_Type == 1)
                {
                    mysql = " Delete from Detailes_Purshing  where (id=" + Detailes_Purshing_Id + ") ";
                    //  mysql = " UPDATE Detailes_Purshing Set Flage_Not_Show=1 where (id=" + Detailes_Purshing_Id + ") ";
                    opp.insert_me(mysql);

                    mysql = " Delete from Detailes_Purshing_Import_Export  where (Detailes_Purshing_Id=" + Detailes_Purshing_Id + ") ";
                    opp.insert_me(mysql);
                    //// ⁄œÌ· »Ì«‰«  «·„Œ“‰ «·„‰’—› ≈·Ì… »‰ﬁ’  «·—’Ìœ
                    //To_Store_Unit_Item_Table_Id
                    mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Larg_Qnty + ", D_Qua =D_Qua - " + Small_Qnty + ", Item_No =Item_No - " + ItemNO + "  WHERE (id = " + Unit_Item_Table_Id + ") ";
                    opp.insert_me(mysql);
                }
                #endregion

                #region Edit Rest
                if (Transaction_Type == 2)
                {
                    mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty + " + Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty + " + Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  + " + ItemNO + "  where (id>=" + Detailes_Purshing_Id + ") AND Unit_item_Table_Id=" + Unit_Item_Table_Id + " ";
                    opp.insert_me(mysql);
                    mysql = " UPDATE  Detailes_Purshing SET  Balance_Id_To_Larg_Qnty=Balance_Id_To_Larg_Qnty  -  " + Larg_Qnty + ",Balance_Id_To_Small_Qnty=Balance_Id_To_Small_Qnty  - " + Small_Qnty + ",Balance_Id_To_Item_No=Balance_Id_To_Item_No - " + ItemNO + "  where (id>=" + Detailes_Purshing_Id + ")  AND To_Store_Unit_Item_Table_Id=" + Unit_Item_Table_Id + "  ";
                    opp.insert_me(mysql);


                    mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty - " + Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty - " + Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  - " + ItemNO + "  where (id>=" + Detailes_Purshing_Id + ") AND Unit_item_Table_Id=" + To_Store_Unit_Item_Table_Id + " ";
                    opp.insert_me(mysql);

                    mysql = " UPDATE  Detailes_Purshing SET  Balance_Id_To_Larg_Qnty=Balance_Id_To_Larg_Qnty  +  " + Larg_Qnty + ",Balance_Id_To_Small_Qnty=Balance_Id_To_Small_Qnty  + " + Small_Qnty + ",Balance_Id_To_Item_No=Balance_Id_To_Item_No + " + ItemNO + "  where (id>=" + Detailes_Purshing_Id + ")  AND To_Store_Unit_Item_Table_Id=" + To_Store_Unit_Item_Table_Id + "  ";
                    opp.insert_me(mysql);

                }


                if (Transaction_Type == 1)
                {



                    mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty - " + Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty - " + Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  - " + ItemNO + "  where (id>=" + Detailes_Purshing_Id + ") AND Unit_item_Table_Id=" + Unit_Item_Table_Id + " ";
                    opp.insert_me(mysql);

                    mysql = " UPDATE  Detailes_Purshing SET  Balance_Id_To_Larg_Qnty=Balance_Id_To_Larg_Qnty  +  " + Larg_Qnty + ",Balance_Id_To_Small_Qnty=Balance_Id_To_Small_Qnty  + " + Small_Qnty + ",Balance_Id_To_Item_No=Balance_Id_To_Item_No + " + ItemNO + "  where (id>=" + Detailes_Purshing_Id + ")  AND To_Store_Unit_Item_Table_Id=" + Unit_Item_Table_Id + "  ";
                    opp.insert_me(mysql);

                    mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty + " + Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty + " + Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  + " + ItemNO + "  where (id>=" + Detailes_Purshing_Id + ") AND Unit_item_Table_Id=" + To_Store_Unit_Item_Table_Id + " ";
                    opp.insert_me(mysql);
                    mysql = " UPDATE  Detailes_Purshing SET  Balance_Id_To_Larg_Qnty=Balance_Id_To_Larg_Qnty  -  " + Larg_Qnty + ",Balance_Id_To_Small_Qnty=Balance_Id_To_Small_Qnty  - " + Small_Qnty + ",Balance_Id_To_Item_No=Balance_Id_To_Item_No - " + ItemNO + "  where (id>=" + Detailes_Purshing_Id + ")  AND To_Store_Unit_Item_Table_Id=" + To_Store_Unit_Item_Table_Id + "  ";
                    opp.insert_me(mysql);

                }





                ////////

                mysql = " UPDATE Detailes_Purshing SET Balance_Id_From_Larg_Qnty = FLOOR((" + S_unit_Value + " * Balance_Id_From_Larg_Qnty + Balance_Id_From_Small_Qnty) / " + S_unit_Value + ") ,  ";
                mysql = mysql + " Balance_Id_From_Small_Qnty =FlOOR(((" + S_unit_Value + " * Balance_Id_From_Larg_Qnty + Balance_Id_From_Small_Qnty)   / " + S_unit_Value + " - FLOOR((" + S_unit_Value + " * Balance_Id_From_Larg_Qnty + Balance_Id_From_Small_Qnty) / " + S_unit_Value + ")) * " + S_unit_Value + " )";
                opp.insert_me(mysql);

                mysql = " UPDATE Detailes_Purshing SET Balance_Id_To_Larg_Qnty = FLOOR((" + S_unit_Value + " * Balance_Id_To_Larg_Qnty + Balance_Id_To_Small_Qnty) / " + S_unit_Value + ") ,  ";
                mysql = mysql + " Balance_Id_To_Small_Qnty = FLOOR(((" + S_unit_Value + " * Balance_Id_To_Larg_Qnty + Balance_Id_To_Small_Qnty)   / " + S_unit_Value + " - FLOOR((" + S_unit_Value + " * Balance_Id_To_Larg_Qnty + Balance_Id_To_Small_Qnty) / " + S_unit_Value + ")) * " + S_unit_Value + " )";
                opp.insert_me(mysql);


                #endregion


                #region Delete Order_Purshing Where all Item Deleted In Detailes_Purshing

                mysql = @" SELECT Order_Purshing.* FROM Order_Purshing INNER JOIN
                         Detailes_Purshing ON Detailes_Purshing.OrderTable_id = Order_Purshing.Id 
 WHERE (Order_Purshing.Id = " + Order_Purshing_Id + ") ";

                DataTable dt_ = opp.return_DataTable(mysql);
                if (dt_.Rows.Count == 0)
                {
                    mysql = "Delete From Order_Purshing Where Id=" + Order_Purshing_Id;
                    opp.insert_me(mysql);

                    if (Order_Type == 10)
                    {
                        mysql = "DELETE FROM  Item_Raw_MaterialsFor_Entage WHERE(SubOrderTable_id =" + Order_Purshing_Id + ")";
                        opp.insert_me(mysql);
                    }


                }


                if (Order_Type == 1 | Order_Type == 6)             //«· ÕÊÌ·
                {

                    mysql = @" SELECT Order_Purshing.* FROM Order_Purshing INNER JOIN
                         Detailes_Purshing ON Detailes_Purshing.OrderTable_id = Order_Purshing.Id
WHERE(Order_Purshing.Id = " + MainOrderTable_id + ") ";

                    dt_ = opp.return_DataTable(mysql);
                    if (dt_.Rows.Count == 0)
                    {
                        mysql = "Delete From Order_Purshing Where Id=" + MainOrderTable_id;
                        opp.insert_me(mysql);
                    }
                }

                #endregion

                //try
                //{
                #region Delete Item_Raw_MaterialsFor_Entage_Details


                mysql = @"SELECT        Item_Raw_MaterialsFor_Entage_List_Id FROM Item_Raw_MaterialsFor_Entage_Details 
WHERE(Item_Raw_MaterialsFor_Entage_Details.Detailes_Purshing_Id = " + Detailes_Purshing_Id + ")";
                long Item_Raw_MaterialsFor_Entage_List_Id = 0;
                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    Item_Raw_MaterialsFor_Entage_List_Id = long.Parse(dt.Rows[0]["Item_Raw_MaterialsFor_Entage_List_Id"].ToString());
                }
                mysql = "DELETE FROM Item_Raw_MaterialsFor_Entage_Details WHERE(Detailes_Purshing_Id =" + Detailes_Purshing_Id + ")";
                opp.insert_me(mysql);


                mysql = @"SELECT Item_Raw_MaterialsFor_Entage_List_Id FROM Item_Raw_MaterialsFor_Entage_Details 
WHERE(Item_Raw_MaterialsFor_Entage_Details.Item_Raw_MaterialsFor_Entage_List_Id = " + Item_Raw_MaterialsFor_Entage_List_Id + ")";

                dt_ = opp.return_DataTable(mysql);
                if (dt_.Rows.Count == 0)
                {
                    mysql = "Delete From Item_Raw_MaterialsFor_Entage_List Where Id=" + Item_Raw_MaterialsFor_Entage_List_Id;
                    opp.insert_me(mysql);
                }

                //}catch { }


                #endregion
            }
            catch (Exception e)
            {

                Excp = 1;

            }

        }
        public void Edit_Order_DetailsFromEdit(ref int Excp, ref operations opp, DataTable dt_NewOrder, int index, decimal New_Larg_Qnty, decimal New_Small_Qnty, decimal New_ItemNO)
        {
            long Detailes_Purshing_Import_Export_Id = 0;
            long Detailes_Purshing_Id = long.Parse(dt_NewOrder.Rows[index]["Detailes_Purshing_Id"].ToString());
            long Unit_Item_Table_Id = long.Parse(dt_NewOrder.Rows[index]["Unit_item_Table_Id"].ToString());
            long To_Store_Unit_Item_Table_Id = long.Parse(dt_NewOrder.Rows[index]["To_Store_Unit_Item_Table_Id"].ToString());
            decimal Larg_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Detailes_Purshing_qua"].ToString());
            decimal Small_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Detailes_Purshing_Part_qua"].ToString());
            decimal ItemNO = decimal.Parse(dt_NewOrder.Rows[index]["Detailes_Purshing_Item_No"].ToString());
            decimal S_unit_Value = decimal.Parse(dt_NewOrder.Rows[index]["S_unit_Value"].ToString());
            long Item_Id = long.Parse(dt_NewOrder.Rows[index]["Item_id"].ToString());
            int Order_Type = int.Parse(dt_NewOrder.Rows[index]["Order_Type"].ToString());
            int Transaction_Type = int.Parse(dt_NewOrder.Rows[index]["Transaction_Type"].ToString());
            long Related_Detailes_Purshing_Id = long.Parse(dt_NewOrder.Rows[index]["Related_Detailes_Purshing_Id"].ToString());
            long MainOrderTable_id = long.Parse(dt_NewOrder.Rows[index]["MainOrderTable_id"].ToString());
            long OrderTable_id = long.Parse(dt_NewOrder.Rows[index]["OrderTable_id"].ToString());
            decimal Qua_After = 0;
            try
            {
                Qua_After = long.Parse(dt_NewOrder.Rows[index]["Qua_After"].ToString());
            }
            catch { }
            if (New_Larg_Qnty == 0 & New_Small_Qnty == 0)
            {
                New_Larg_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Larg_Qnty"].ToString());
                New_Small_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Small_Qnty"].ToString());
                New_ItemNO = decimal.Parse(dt_NewOrder.Rows[index]["Item_NO"].ToString());
            }
            decimal Diff_Larg_Qnty = New_Larg_Qnty - Larg_Qnty;
            decimal Diff_Small_Qnty = New_Small_Qnty - Small_Qnty;
            decimal Diff_ItemNO = New_ItemNO - ItemNO;
            decimal Diff_Larg_Qnty_ = Diff_Larg_Qnty;
            decimal Diff_Small_Qnty_ = Diff_Small_Qnty;
            Common.Get_PositiveNumber(ref Diff_Larg_Qnty_);
            Common.Get_PositiveNumber(ref Diff_Small_Qnty_);
            decimal Diff_Quantity = Common.Get_Item_Quantity(Diff_Larg_Qnty_, Diff_Small_Qnty_, S_unit_Value);
            decimal Quantity = Common.Get_Item_Quantity(New_Larg_Qnty, New_Small_Qnty, S_unit_Value);
            decimal Quantity_Balance = 0;


            string mysql = "";


            mysql = " Update Detailes_Purshing Set qua=" + New_Larg_Qnty + " ,Part_qua=" + New_Small_Qnty + ",Item_No=" + New_ItemNO + "   ";
            mysql = mysql + " ,  Qua_After =Qua_After + " + Diff_Larg_Qnty + " , ";
            mysql = mysql + " Part_qua_After =Part_qua_After + " + Diff_Small_Qnty + " , ";
            mysql = mysql + " Item_No_After = Item_No_After + " + Diff_ItemNO + "   ";

            mysql = mysql + "   where(id = " + Detailes_Purshing_Id + ") ";
            opp.insert_me(mysql);

            mysql = " Update Detailes_Purshing Set qua=" + New_Larg_Qnty + " ,Part_qua=" + New_Small_Qnty + ",Item_No=" + New_ItemNO + " ";
            mysql = mysql + " ,  Qua_After =Qua_After - " + Diff_Larg_Qnty + " , ";
            mysql = mysql + " Part_qua_After =Part_qua_After - " + Diff_Small_Qnty + " , ";
            mysql = mysql + " Item_No_After = Item_No_After - " + Diff_ItemNO + "   ";

            mysql = mysql + " where (id=" + Related_Detailes_Purshing_Id + ") ";
            opp.insert_me(mysql);


            //mysql = " Update Detailes_Purshing_Import_Export Set qua=" + New_Larg_Qnty + " ,Part_qua=" + New_Small_Qnty + ",Item_No=" + New_ItemNO + "  where (Detailes_Purshing_Id=" + Detailes_Purshing_Id + ") ";
            //opp.insert_me(mysql);

            //mysql = " Update Detailes_Purshing_Import_Export Set  Balance_Larg_Qnty = Balance_Larg_Qnty + " + Diff_Larg_Qnty+", Balance_Small_Qnty = Balance_Small_Qnty + "+Diff_Small_Qnty+ ", Balance_Item_No = Balance_Item_No  + "+Diff_ItemNO+" where(Detailes_Purshing_Id = " + Detailes_Purshing_Id + ") ";
            //opp.insert_me(mysql);

            try
            {
                #region Transform           ÕÊÌ·
                if (Order_Type == 1 | Order_Type == 6)             //«· ÕÊÌ·
                {
                    //mysql = " Update Detailes_Purshing_Import_Export Set  Balance_Larg_Qnty = Balance_Larg_Qnty - " + Diff_Larg_Qnty + ", Balance_Small_Qnty = Balance_Small_Qnty - " + Diff_Small_Qnty + ", Balance_Item_No = Balance_Item_No  - " + Diff_ItemNO + " where(Detailes_Purshing_Id >= " + Detailes_Purshing_Id + ") ";
                    //opp.insert_me(mysql);

                    if (Transaction_Type == 1)
                    {


                        mysql = " UPDATE Detailes_Purshing SET Balance_Id_From_Larg_Qnty =Balance_Id_From_Larg_Qnty + " + Diff_Larg_Qnty + " , ";
                        mysql = mysql + "  Balance_Id_From_Small_Qnty = Balance_Id_From_Small_Qnty  + " + Diff_Small_Qnty + " , ";
                        mysql = mysql + " Balance_Id_From_Item_No = Balance_Id_From_Item_No + " + Diff_ItemNO + " , ";
                        mysql = mysql + " Balance_Id_To_Larg_Qnty =Balance_Id_To_Larg_Qnty-  " + Diff_Larg_Qnty + " , ";
                        mysql = mysql + "Balance_Id_To_Small_Qnty =Balance_Id_To_Small_Qnty - " + Diff_Small_Qnty + " , ";
                        mysql = mysql + " Balance_Id_To_Item_No =Balance_Id_To_Item_No -  " + Diff_ItemNO + " , ";
                        mysql = mysql + " Qua_After =Qua_After + " + Diff_Larg_Qnty + " , ";
                        mysql = mysql + " Part_qua_After =Part_qua_After + " + Diff_Small_Qnty + " , ";
                        mysql = mysql + " Item_No_After = Item_No_After + " + Diff_ItemNO + "   ";
                        mysql = mysql + " WHERE( Id = " + Detailes_Purshing_Id + ")";
                        opp.insert_me(mysql);




                        decimal Quantity_Import = Common.Get_Item_Quantity(New_Larg_Qnty, New_Small_Qnty, S_unit_Value);


                        mysql = "Select * from Detailes_Purshing_Import_Export Where Detailes_Purshing_Id  =" + Detailes_Purshing_Id + " ORDER BY Id DESC ";

                        DataTable dt = opp.return_DataTable(mysql);
                        decimal Balance_Larg_Qnty = 0;
                        decimal Balance_Small_Qnty = 0;
                        decimal Balance_Item_No = 0;
                        if (dt.Rows.Count > 0)
                        {
                            Detailes_Purshing_Import_Export_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                            Balance_Larg_Qnty = decimal.Parse(dt.Rows[0]["Balance_Larg_Qnty"].ToString());
                            Balance_Small_Qnty = decimal.Parse(dt.Rows[0]["Balance_Small_Qnty"].ToString());
                            Balance_Item_No = decimal.Parse(dt.Rows[0]["Balance_Item_No"].ToString());

                            Balance_Larg_Qnty = Balance_Larg_Qnty + Diff_Larg_Qnty;
                            Balance_Small_Qnty = Balance_Small_Qnty + Diff_Small_Qnty;
                            Balance_Item_No = Balance_Item_No + Diff_ItemNO;
                            Quantity_Balance = Common.Get_Item_Quantity(Balance_Larg_Qnty, Balance_Small_Qnty, S_unit_Value);

                            string Notes_From_To = dt.Rows[0]["Notes_From_To"].ToString();
                            string Notes = dt.Rows[0]["Notes"].ToString();
                            string Notes_From = dt.Rows[0]["Notes_From"].ToString();
                            string Notes_To = dt.Rows[0]["Notes_To"].ToString();
                            Notes = "  „  ⁄œÌ· «·ﬁÌ„… «·„‰’—›Â „‰ «·’‰›";

                            int Machine_Id = int.Parse(dt.Rows[0]["Machine_Id"].ToString());
                            mysql = " INSERT INTO Detailes_Purshing_Import_Export(Detailes_Purshing_Id, OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Quantity, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Quantity_Balance, qua_Import, Part_Qua_Import,Item_No_Import, Quantity_Import, qua_Export, Part_Qua_Export, Item_No_Export, Quantity_Export, Notes_From_To, Notes, Notes_From, Notes_To, Machine_Id, Transaction_Type,Process_Date,User_Id,OperationDate)";
                            mysql = mysql + " VALUES(" + Detailes_Purshing_Id + "," + OrderTable_id + "," + Unit_Item_Table_Id + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ", " + Balance_Larg_Qnty + "," + Balance_Small_Qnty + ", " + Balance_Item_No + "," + Quantity_Balance + ",0,0,0,0, " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ",'" + Notes_From_To + "','" + Notes + "','" + Notes_From + "','" + Notes_To + "'," + Machine_Id + ",1,'" + operations.dateFormat(DateTime.Now.Date) + "',," + Common.User_Id + "," + operations.Get_DateTimeToInsert(DateTime.Now) + ")";
                            opp.insert_me(mysql);

                            mysql = "Delete from Detailes_Purshing_Import_Export Where ID  =" + Detailes_Purshing_Import_Export_Id + "   ";
                            opp.insert_me(mysql);
                        }

                        /////////////////////////////////////////////////////////////////////////////
                        //// ⁄œÌ· «·’‰› «·«Œ— Related_Detailes_Purshing_Id 
                        mysql = " UPDATE Detailes_Purshing SET Balance_Id_From_Larg_Qnty =Balance_Id_From_Larg_Qnty - " + Diff_Larg_Qnty + " , ";
                        mysql = mysql + "  Balance_Id_From_Small_Qnty = Balance_Id_From_Small_Qnty  - " + Diff_Small_Qnty + " , ";
                        mysql = mysql + " Balance_Id_From_Item_No = Balance_Id_From_Item_No - " + Diff_ItemNO + " , ";
                        mysql = mysql + " Balance_Id_To_Larg_Qnty =Balance_Id_To_Larg_Qnty +  " + Diff_Larg_Qnty + " , ";
                        mysql = mysql + "Balance_Id_To_Small_Qnty =Balance_Id_To_Small_Qnty + " + Diff_Small_Qnty + " , ";
                        mysql = mysql + " Balance_Id_To_Item_No =Balance_Id_To_Item_No +  " + Diff_ItemNO + " , ";
                        mysql = mysql + " Qua_After =Qua_After - " + Diff_Larg_Qnty + " , ";
                        mysql = mysql + " Part_qua_After =Part_qua_After - " + Diff_Small_Qnty + " , ";
                        mysql = mysql + " Item_No_After = Item_No_After - " + Diff_ItemNO + "   ";
                        mysql = mysql + " WHERE( Id = " + Related_Detailes_Purshing_Id + ")";
                        opp.insert_me(mysql);


                        mysql = "Select * from Detailes_Purshing_Import_Export Where Detailes_Purshing_Id  =" + Related_Detailes_Purshing_Id + " ORDER BY Id DESC "; ;
                        dt = opp.return_DataTable(mysql);
                        Balance_Larg_Qnty = 0;
                        Balance_Small_Qnty = 0;
                        Balance_Item_No = 0;
                        if (dt.Rows.Count > 0)
                        {
                            Detailes_Purshing_Import_Export_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                            Balance_Larg_Qnty = decimal.Parse(dt.Rows[0]["Balance_Larg_Qnty"].ToString());
                            Balance_Small_Qnty = decimal.Parse(dt.Rows[0]["Balance_Small_Qnty"].ToString());
                            Balance_Item_No = decimal.Parse(dt.Rows[0]["Balance_Item_No"].ToString());

                            Balance_Larg_Qnty = Balance_Larg_Qnty - Diff_Larg_Qnty;
                            Balance_Small_Qnty = Balance_Small_Qnty - Diff_Small_Qnty;
                            Balance_Item_No = Balance_Item_No - Diff_ItemNO;
                            Quantity_Balance = Common.Get_Item_Quantity(Balance_Larg_Qnty, Balance_Small_Qnty, S_unit_Value);

                            string Notes_From_To = dt.Rows[0]["Notes_From_To"].ToString();
                            string Notes = dt.Rows[0]["Notes"].ToString();
                            string Notes_From = dt.Rows[0]["Notes_From"].ToString();
                            string Notes_To = dt.Rows[0]["Notes_To"].ToString();

                            Notes = "  „  ⁄œÌ· «·ﬁÌ„… «·Ê«—… „‰ «·’‰›";
                            int Machine_Id = int.Parse(dt.Rows[0]["Machine_Id"].ToString());
                            mysql = " INSERT INTO Detailes_Purshing_Import_Export(Detailes_Purshing_Id, OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Quantity, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Quantity_Balance, qua_Import, Part_Qua_Import,Item_No_Import, Quantity_Import, qua_Export, Part_Qua_Export, Item_No_Export, Quantity_Export, Notes_From_To, Notes, Notes_From, Notes_To, Machine_Id, Transaction_Type,Process_Date,user_Id,OperationDate)";
                            mysql = mysql + " VALUES(" + Related_Detailes_Purshing_Id + "," + MainOrderTable_id + "," + To_Store_Unit_Item_Table_Id + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ", " + Balance_Larg_Qnty + "," + Balance_Small_Qnty + ", " + Balance_Item_No + "," + Quantity_Balance + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ",0,0,0,0,'" + Notes_From_To + "','" + Notes + "','" + Notes_From + "','" + Notes_To + "'," + Machine_Id + ",2,'" + operations.dateFormat(DateTime.Now.Date) + "'," + Common.User_Id + "," + operations.Get_DateTimeToInsert(DateTime.Now) + ")";
                            opp.insert_me(mysql);
                            //Detailes_Purshing_Import_Export_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                            mysql = "Delete from Detailes_Purshing_Import_Export Where ID  =" + Detailes_Purshing_Import_Export_Id + "   ";
                            opp.insert_me(mysql);
                        }


                        mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty + " + Diff_Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty + " + Diff_Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  + " + Diff_ItemNO + "  where (id>   " + Detailes_Purshing_Id + ") And Unit_item_Table_Id=" + Unit_Item_Table_Id + " ";
                        opp.insert_me(mysql);


                        mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty - " + Diff_Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty - " + Diff_Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  - " + Diff_ItemNO + "  where (id>  " + Related_Detailes_Purshing_Id + ")    And Unit_item_Table_Id=" + To_Store_Unit_Item_Table_Id + " ";
                        opp.insert_me(mysql);


                    }
                    if (Transaction_Type == 2)
                    {
                        //Diff_Larg_Qnty = Diff_Larg_Qnty * -1;





                        mysql = " UPDATE Detailes_Purshing SET Balance_Id_From_Larg_Qnty =Balance_Id_From_Larg_Qnty - " + Diff_Larg_Qnty + " , ";
                        mysql = mysql + "  Balance_Id_From_Small_Qnty = Balance_Id_From_Small_Qnty  - " + Diff_Small_Qnty + " , ";
                        mysql = mysql + " Balance_Id_From_Item_No = Balance_Id_From_Item_No - " + Diff_ItemNO + " , ";
                        mysql = mysql + " Balance_Id_To_Larg_Qnty =Balance_Id_To_Larg_Qnty +  " + Diff_Larg_Qnty + " , ";
                        mysql = mysql + "Balance_Id_To_Small_Qnty =Balance_Id_To_Small_Qnty + " + Diff_Small_Qnty + " , ";
                        mysql = mysql + " Balance_Id_To_Item_No =Balance_Id_To_Item_No +  " + Diff_ItemNO + " , ";
                        mysql = mysql + " Qua_After =Qua_After - " + Diff_Larg_Qnty + " , ";
                        mysql = mysql + " Part_qua_After =Part_qua_After - " + Diff_Small_Qnty + " , ";
                        mysql = mysql + " Item_No_After = Item_No_After - " + Diff_ItemNO + "   ";
                        mysql = mysql + " WHERE( Id = " + Detailes_Purshing_Id + ")";
                        opp.insert_me(mysql);




                        decimal Quantity_Import = Common.Get_Item_Quantity(New_Larg_Qnty, New_Small_Qnty, S_unit_Value);


                        mysql = "Select * from Detailes_Purshing_Import_Export Where Detailes_Purshing_Id  =" + Detailes_Purshing_Id + " ORDER BY Id DESC "; ;
                        DataTable dt = opp.return_DataTable(mysql);
                        decimal Balance_Larg_Qnty = 0;
                        decimal Balance_Small_Qnty = 0;
                        decimal Balance_Item_No = 0;
                        if (dt.Rows.Count > 0)
                        {
                            Balance_Larg_Qnty = decimal.Parse(dt.Rows[0]["Balance_Larg_Qnty"].ToString());
                            Balance_Small_Qnty = decimal.Parse(dt.Rows[0]["Balance_Small_Qnty"].ToString());
                            Balance_Item_No = decimal.Parse(dt.Rows[0]["Balance_Item_No"].ToString());

                            Balance_Larg_Qnty = Balance_Larg_Qnty - Diff_Larg_Qnty;
                            Balance_Small_Qnty = Balance_Small_Qnty - Diff_Small_Qnty;
                            Balance_Item_No = Balance_Item_No - Diff_ItemNO;
                            Quantity_Balance = Common.Get_Item_Quantity(Balance_Larg_Qnty, Balance_Small_Qnty, S_unit_Value);

                            string Notes_From_To = dt.Rows[0]["Notes_From_To"].ToString();
                            string Notes = dt.Rows[0]["Notes"].ToString();
                            string Notes_From = dt.Rows[0]["Notes_From"].ToString();
                            string Notes_To = dt.Rows[0]["Notes_To"].ToString();
                            Notes = "  „  ⁄œÌ· «·ﬁÌ„… «·„‰’—›Â „‰ «·’‰›";

                            int Machine_Id = int.Parse(dt.Rows[0]["Machine_Id"].ToString());
                            mysql = " INSERT INTO Detailes_Purshing_Import_Export(Detailes_Purshing_Id, OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Quantity, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Quantity_Balance, qua_Import, Part_Qua_Import,Item_No_Import, Quantity_Import, qua_Export, Part_Qua_Export, Item_No_Export, Quantity_Export, Notes_From_To, Notes, Notes_From, Notes_To, Machine_Id, Transaction_Type,Process_Date,User_Id,OperationDate)";
                            mysql = mysql + " VALUES(" + Detailes_Purshing_Id + "," + OrderTable_id + "," + Unit_Item_Table_Id + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ", " + Balance_Larg_Qnty + "," + Balance_Small_Qnty + ", " + Balance_Item_No + "," + Quantity_Balance + ",0,0,0,0, " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ",'" + Notes_From_To + "','" + Notes + "','" + Notes_From + "','" + Notes_To + "'," + Machine_Id + ",2,'" + operations.dateFormat(DateTime.Now.Date) + "',," + Common.User_Id + "," + operations.Get_DateTimeToInsert(DateTime.Now) + ")";
                            opp.insert_me(mysql);

                            Detailes_Purshing_Import_Export_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                            mysql = "Delete from Detailes_Purshing_Import_Export Where ID  =" + Detailes_Purshing_Import_Export_Id + "   ";
                            opp.insert_me(mysql);
                        }

                        /////////////////////////////////////////////////////////////////////////////
                        //// ⁄œÌ· «·’‰› «·«Œ— Related_Detailes_Purshing_Id 
                        mysql = " UPDATE Detailes_Purshing SET Balance_Id_From_Larg_Qnty =Balance_Id_From_Larg_Qnty + " + Diff_Larg_Qnty + " , ";
                        mysql = mysql + "  Balance_Id_From_Small_Qnty = Balance_Id_From_Small_Qnty  + " + Diff_Small_Qnty + " , ";
                        mysql = mysql + " Balance_Id_From_Item_No = Balance_Id_From_Item_No + " + Diff_ItemNO + " , ";
                        mysql = mysql + " Balance_Id_To_Larg_Qnty =Balance_Id_To_Larg_Qnty -  " + Diff_Larg_Qnty + " , ";
                        mysql = mysql + "Balance_Id_To_Small_Qnty =Balance_Id_To_Small_Qnty - " + Diff_Small_Qnty + " , ";
                        mysql = mysql + " Balance_Id_To_Item_No =Balance_Id_To_Item_No -  " + Diff_ItemNO + " , ";
                        mysql = mysql + " Qua_After =Qua_After + " + Diff_Larg_Qnty + " , ";
                        mysql = mysql + " Part_qua_After =Part_qua_After + " + Diff_Small_Qnty + " , ";
                        mysql = mysql + " Item_No_After = Item_No_After + " + Diff_ItemNO + "   ";
                        mysql = mysql + " WHERE( Id = " + Related_Detailes_Purshing_Id + ")";
                        opp.insert_me(mysql);


                        mysql = "Select * from Detailes_Purshing_Import_Export Where Detailes_Purshing_Id  =" + Related_Detailes_Purshing_Id + " ORDER BY Id DESC "; ;
                        dt = opp.return_DataTable(mysql);
                        Balance_Larg_Qnty = 0;
                        Balance_Small_Qnty = 0;
                        Balance_Item_No = 0;
                        if (dt.Rows.Count > 0)
                        {
                            Balance_Larg_Qnty = decimal.Parse(dt.Rows[0]["Balance_Larg_Qnty"].ToString());
                            Balance_Small_Qnty = decimal.Parse(dt.Rows[0]["Balance_Small_Qnty"].ToString());
                            Balance_Item_No = decimal.Parse(dt.Rows[0]["Balance_Item_No"].ToString());

                            Balance_Larg_Qnty = Balance_Larg_Qnty + Diff_Larg_Qnty;
                            Balance_Small_Qnty = Balance_Small_Qnty + Diff_Small_Qnty;
                            Balance_Item_No = Balance_Item_No + Diff_ItemNO;
                            Quantity_Balance = Common.Get_Item_Quantity(Balance_Larg_Qnty, Balance_Small_Qnty, S_unit_Value);

                            string Notes_From_To = dt.Rows[0]["Notes_From_To"].ToString();
                            string Notes = dt.Rows[0]["Notes"].ToString();
                            string Notes_From = dt.Rows[0]["Notes_From"].ToString();
                            string Notes_To = dt.Rows[0]["Notes_To"].ToString();

                            Notes = "  „  ⁄œÌ· «·ﬁÌ„… «·„‰’—›Â „‰ «·’‰›";
                            int Machine_Id = int.Parse(dt.Rows[0]["Machine_Id"].ToString());
                            mysql = " INSERT INTO Detailes_Purshing_Import_Export(Detailes_Purshing_Id, OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Quantity, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Quantity_Balance, qua_Import, Part_Qua_Import,Item_No_Import, Quantity_Import, qua_Export, Part_Qua_Export, Item_No_Export, Quantity_Export, Notes_From_To, Notes, Notes_From, Notes_To, Machine_Id, Transaction_Type,Process_Date,User_Id,OperationDate)";
                            mysql = mysql + " VALUES(" + Related_Detailes_Purshing_Id + "," + MainOrderTable_id + "," + To_Store_Unit_Item_Table_Id + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ", " + Balance_Larg_Qnty + "," + Balance_Small_Qnty + ", " + Balance_Item_No + "," + Quantity_Balance + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ",0,0,0,0,'" + Notes_From_To + "','" + Notes + "','" + Notes_From + "','" + Notes_To + "'," + Machine_Id + ",1,'" + operations.dateFormat(DateTime.Now.Date) + "'," + Common.User_Id + "," + operations.Get_DateTimeToInsert(DateTime.Now) + ")";
                            opp.insert_me(mysql);

                            Detailes_Purshing_Import_Export_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                            mysql = "Delete from Detailes_Purshing_Import_Export Where ID  =" + Detailes_Purshing_Import_Export_Id + "   ";
                            opp.insert_me(mysql);
                        }
                        mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty - " + Diff_Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty -" + Diff_Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  -  " + Diff_ItemNO + "  where (id>   " + Detailes_Purshing_Id + ") And Unit_item_Table_Id=" + Unit_Item_Table_Id + " ";
                        opp.insert_me(mysql);

                        mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty + " + Diff_Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty + " + Diff_Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  + " + Diff_ItemNO + "  where (id>  " + Related_Detailes_Purshing_Id + ")    And Unit_item_Table_Id=" + To_Store_Unit_Item_Table_Id + " ";
                        opp.insert_me(mysql);

                    }
                    if (Transaction_Type == 1)
                    {


                        //// ⁄œÌ· »Ì«‰«  «·„Œ“‰ «·„‰’—› ≈·Ì… »‰ﬁ’  «·—’Ìœ
                        //To_Store_Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + "  WHERE (id = " + Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);


                        //// ⁄œÌ· »Ì«‰«  «·„Œ“‰ «·„‰’—› „‰… »“Ì«œ… «·—’Ìœ
                        //Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Diff_Larg_Qnty + ", D_Qua =D_Qua - " + Diff_Small_Qnty + ", Item_No =Item_No - " + Diff_ItemNO + " WHERE (id = " + To_Store_Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);



                    }
                    if (Transaction_Type == 2)
                    {

                        //// ⁄œÌ· »Ì«‰«  «·„Œ“‰ «·„‰’—› „‰… »“Ì«œ… «·—’Ìœ
                        //Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Diff_Larg_Qnty + ", D_Qua =D_Qua - " + Diff_Small_Qnty + ", Item_No =Item_No - " + Diff_ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);

                        //// ⁄œÌ· »Ì«‰«  «·„Œ“‰ «·„‰’—› ≈·Ì… »‰ﬁ’  «·—’Ìœ
                        //To_Store_Unit_Item_Table_Id
                        mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Larg_Qnty + ", D_Qua =D_Qua + " + Small_Qnty + ", Item_No =Item_No + " + ItemNO + "  WHERE (id = " + To_Store_Unit_Item_Table_Id + ") ";
                        opp.insert_me(mysql);
                    }

                }
                #endregion

                #region     sarf or mortaga3 estlam
                //else if (Order_Type == 2 | Order_Type == 5)
                else if (Transaction_Type == 2)
                {


                    //// ⁄œÌ· »Ì«‰«  «·„Œ“‰ «·„‰’—› „‰… »“Ì«œ… «·—’Ìœ
                    //Unit_Item_Table_Id
                    mysql = " UPDATE Unit_item  SET Full_qua =Full_qua - " + Diff_Larg_Qnty + ", D_Qua =D_Qua - " + Diff_Small_Qnty + ", Item_No =Item_No - " + Diff_ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                    opp.insert_me(mysql);

                    mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty + " + Diff_Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty + " + Diff_Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  + " + Diff_ItemNO + "  where (id>   " + Detailes_Purshing_Id + ") And Unit_item_Table_Id=" + Unit_Item_Table_Id + " ";
                    opp.insert_me(mysql);


                    decimal Quantity_Import = Common.Get_Item_Quantity(New_Larg_Qnty, New_Small_Qnty, S_unit_Value);


                    mysql = "Select * from Detailes_Purshing_Import_Export Where Detailes_Purshing_Id  =" + Detailes_Purshing_Id + " ORDER BY Id DESC "; ;
                    DataTable dt = opp.return_DataTable(mysql);
                    decimal Balance_Larg_Qnty = 0;
                    decimal Balance_Small_Qnty = 0;
                    decimal Balance_Item_No = 0;
                    if (dt.Rows.Count > 0)
                    {
                        Balance_Larg_Qnty = decimal.Parse(dt.Rows[0]["Balance_Larg_Qnty"].ToString());
                        Balance_Small_Qnty = decimal.Parse(dt.Rows[0]["Balance_Small_Qnty"].ToString());
                        Balance_Item_No = decimal.Parse(dt.Rows[0]["Balance_Item_No"].ToString());

                        Balance_Larg_Qnty = Balance_Larg_Qnty - Diff_Larg_Qnty;
                        Balance_Small_Qnty = Balance_Small_Qnty - Diff_Small_Qnty;
                        Balance_Item_No = Balance_Item_No - Diff_ItemNO;
                        Quantity_Balance = Common.Get_Item_Quantity(Balance_Larg_Qnty, Balance_Small_Qnty, S_unit_Value);

                        string Notes_From_To = dt.Rows[0]["Notes_From_To"].ToString();
                        string Notes = dt.Rows[0]["Notes"].ToString();
                        string Notes_From = dt.Rows[0]["Notes_From"].ToString();
                        string Notes_To = dt.Rows[0]["Notes_To"].ToString();
                        Notes = "  „  ⁄œÌ· «·ﬁÌ„… «·„‰’—›Â „‰ «·’‰›";

                        int Machine_Id = int.Parse(dt.Rows[0]["Machine_Id"].ToString());
                        mysql = " INSERT INTO Detailes_Purshing_Import_Export(Detailes_Purshing_Id, OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Quantity, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Quantity_Balance, qua_Import, Part_Qua_Import,Item_No_Import, Quantity_Import, qua_Export, Part_Qua_Export, Item_No_Export, Quantity_Export, Notes_From_To, Notes, Notes_From, Notes_To, Machine_Id, Transaction_Type,Process_Date,User_Id,OperationDate)";
                        mysql = mysql + " VALUES(" + Detailes_Purshing_Id + "," + OrderTable_id + "," + Unit_Item_Table_Id + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ", " + Balance_Larg_Qnty + "," + Balance_Small_Qnty + ", " + Balance_Item_No + "," + Quantity_Balance + ",0,0,0,0, " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ",'" + Notes_From_To + "','" + Notes + "','" + Notes_From + "','" + Notes_To + "'," + Machine_Id + ",2,'" + operations.dateFormat(DateTime.Now.Date) + "'," + Common.User_Id + "," + operations.Get_DateTimeToInsert(DateTime.Now) + ")";
                        opp.insert_me(mysql);

                        Detailes_Purshing_Import_Export_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                        mysql = "Delete from Detailes_Purshing_Import_Export Where ID  =" + Detailes_Purshing_Import_Export_Id + "   ";
                        opp.insert_me(mysql);
                    }

                }
                #endregion

                #region    mortaga3 sarf or  estlam
                //else if (Order_Type == 3 | Order_Type == 4 | Order_Type == 9)
                else if (Transaction_Type == 1)
                {

                    //// ⁄œÌ· »Ì«‰«  «·„Œ“‰ «·„‰’—› ≈·Ì… »‰ﬁ’  «·—’Ìœ
                    //To_Store_Unit_Item_Table_Id
                    mysql = " UPDATE Unit_item  SET Full_qua =Full_qua + " + Diff_Larg_Qnty + ", D_Qua =D_Qua + " + Diff_Small_Qnty + ", Item_No =Item_No + " + Diff_ItemNO + " WHERE (id = " + Unit_Item_Table_Id + ") ";
                    opp.insert_me(mysql);

                    mysql = " UPDATE  Detailes_Purshing SET Balance_Id_From_Larg_Qnty=Balance_Id_From_Larg_Qnty - " + Diff_Larg_Qnty + ",Balance_Id_From_Small_Qnty=Balance_Id_From_Small_Qnty -" + Diff_Small_Qnty + ",Balance_Id_From_Item_No=Balance_Id_From_Item_No  -  " + Diff_ItemNO + "  where (id>   " + Detailes_Purshing_Id + ") And Unit_item_Table_Id=" + Unit_Item_Table_Id + " ";
                    opp.insert_me(mysql);


                    decimal Quantity_Import = Common.Get_Item_Quantity(New_Larg_Qnty, New_Small_Qnty, S_unit_Value);


                    mysql = "Select * from Detailes_Purshing_Import_Export Where Detailes_Purshing_Id  =" + Detailes_Purshing_Id + " ORDER BY Id DESC ";
                    DataTable dt = opp.return_DataTable(mysql);
                    decimal Balance_Larg_Qnty = 0;
                    decimal Balance_Small_Qnty = 0;
                    decimal Balance_Item_No = 0;
                    if (dt.Rows.Count > 0)
                    {
                        Balance_Larg_Qnty = decimal.Parse(dt.Rows[0]["Balance_Larg_Qnty"].ToString());
                        Balance_Small_Qnty = decimal.Parse(dt.Rows[0]["Balance_Small_Qnty"].ToString());
                        Balance_Item_No = decimal.Parse(dt.Rows[0]["Balance_Item_No"].ToString());

                        Balance_Larg_Qnty = Balance_Larg_Qnty + Diff_Larg_Qnty;
                        Balance_Small_Qnty = Balance_Small_Qnty + Diff_Small_Qnty;
                        Balance_Item_No = Balance_Item_No + Diff_ItemNO;
                        Quantity_Balance = Common.Get_Item_Quantity(Balance_Larg_Qnty, Balance_Small_Qnty, S_unit_Value);

                        string Notes_From_To = dt.Rows[0]["Notes_From_To"].ToString();
                        string Notes = dt.Rows[0]["Notes"].ToString();
                        string Notes_From = dt.Rows[0]["Notes_From"].ToString();
                        string Notes_To = dt.Rows[0]["Notes_To"].ToString();
                        Notes = "  „  ⁄œÌ· «·ﬁÌ„… «·„‰’—›Â „‰ «·’‰›";

                        int Machine_Id = int.Parse(dt.Rows[0]["Machine_Id"].ToString());
                        mysql = " INSERT INTO Detailes_Purshing_Import_Export(Detailes_Purshing_Id, OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Quantity, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Quantity_Balance, qua_Import, Part_Qua_Import,Item_No_Import, Quantity_Import, qua_Export, Part_Qua_Export, Item_No_Export, Quantity_Export, Notes_From_To, Notes, Notes_From, Notes_To, Machine_Id, Transaction_Type,Process_Date,User_Id,OperationDate)";
                        mysql = mysql + " VALUES(" + Detailes_Purshing_Id + "," + OrderTable_id + "," + Unit_Item_Table_Id + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ", " + Balance_Larg_Qnty + "," + Balance_Small_Qnty + ", " + Balance_Item_No + "," + Quantity_Balance + ", " + New_Larg_Qnty + "," + New_Small_Qnty + "," + New_ItemNO + "," + Quantity + ",0,0,0,0,'" + Notes_From_To + "','" + Notes + "','" + Notes_From + "','" + Notes_To + "'," + Machine_Id + ",1,'" + operations.dateFormat(DateTime.Now.Date) + "'," + Common.User_Id + "," + operations.Get_DateTimeToInsert(DateTime.Now) + ")";
                        opp.insert_me(mysql);

                        Detailes_Purshing_Import_Export_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                        mysql = "Delete from Detailes_Purshing_Import_Export Where ID  =" + Detailes_Purshing_Import_Export_Id + "   ";
                        opp.insert_me(mysql);
                    }
                }
                #endregion

                #region Edit Rest




                ////////

                mysql = " UPDATE Detailes_Purshing SET Balance_Id_From_Larg_Qnty = FLOOR((" + S_unit_Value + " * Balance_Id_From_Larg_Qnty + Balance_Id_From_Small_Qnty) / " + S_unit_Value + ") ,  ";
                mysql = mysql + " Balance_Id_From_Small_Qnty =FlOOR(((" + S_unit_Value + " * Balance_Id_From_Larg_Qnty + Balance_Id_From_Small_Qnty)   / " + S_unit_Value + " - FLOOR((" + S_unit_Value + " * Balance_Id_From_Larg_Qnty + Balance_Id_From_Small_Qnty) / " + S_unit_Value + ")) * " + S_unit_Value + " )";
                opp.insert_me(mysql);

                mysql = " UPDATE Detailes_Purshing SET Balance_Id_To_Larg_Qnty = FLOOR((" + S_unit_Value + " * Balance_Id_To_Larg_Qnty + Balance_Id_To_Small_Qnty) / " + S_unit_Value + ") ,  ";
                mysql = mysql + " Balance_Id_To_Small_Qnty = FLOOR(((" + S_unit_Value + " * Balance_Id_To_Larg_Qnty + Balance_Id_To_Small_Qnty)   / " + S_unit_Value + " - FLOOR((" + S_unit_Value + " * Balance_Id_To_Larg_Qnty + Balance_Id_To_Small_Qnty) / " + S_unit_Value + ")) * " + S_unit_Value + " )";
                opp.insert_me(mysql);


                #endregion



            }
            catch (Exception e)
            {

                Excp = 1;

            }

        }
        public void Delete_Order_With_Change_Item(ref int Excp, ref string Msg, long Item_Raw_MaterialsFor_Entage_List_Id)
        {
            Excp = 0;
            Msg = "";
            operations opp = new operations(1);

            DataTable dt = Get_Item_Raw_MaterialsFor_Entage_Details(ref Excp, ref opp, Item_Raw_MaterialsFor_Entage_List_Id);

            for (int x = 0; x < dt.Rows.Count; x++)
            {
                long Order_Purshing_Id = long.Parse(dt.Rows[x]["Order_Purshing_Id"].ToString());
                DeleteOrder(ref Excp, ref Msg, ref opp, Order_Purshing_Id);
                string Order_Estlam_Sarf_Flage = dt.Rows[x]["Order_Estlam_Sarf_Flage"].ToString();
                if (Order_Estlam_Sarf_Flage == "1")
                {
                    string mysql = "Delete From Item_Raw_MaterialsFor_Entage Where MainOrderTable_id=" + Order_Purshing_Id;
                    opp.insert_me(mysql);
                }
                if (Order_Estlam_Sarf_Flage == "2")
                {
                    string mysql = "Delete From Item_Raw_MaterialsFor_Entage Where SubOrderTable_id=" + Order_Purshing_Id;
                    opp.insert_me(mysql);
                }
            }

            if (Excp == 0)
            { opp.Commit(); }
            else
            {
                opp.Rollback();
            }


        }
        void DeleteOrder(ref int Excp, ref string Msg, ref operations opp, long OrderTable_id)
        {
            DataTable dt = Get_Order_Items_list(ref Excp, ref opp, OrderTable_id);
            for (int x = 0; x < dt.Rows.Count; x++)
            {
                Delete_Order_DetailsFromEdit(ref Excp, ref opp, dt, x);
            }

        }

        void DeleteOrderFrom_Item_Raw_MaterialsFor_Entage(ref int Excp, ref string Msg, ref operations opp, long MainOrderTable_id)
        {
            string mysql = "Select * From  Item_Raw_MaterialsFor_Entage Where MainOrderTable_id = " + MainOrderTable_id;
            DataTable dt = opp.return_DataTable(mysql);
            for (int x = 0; x < dt.Rows.Count; x++)
            {
                long SubOrderTable_id = long.Parse(dt.Rows[x]["SubOrderTable_id"].ToString());
                DeleteOrder(ref Excp, ref Msg, ref opp, SubOrderTable_id);
            }

            mysql = "Delete From Item_Raw_MaterialsFor_Entage Where MainOrderTable_id = " + MainOrderTable_id;
            opp.insert_me(mysql);
        }
        public void Edit_OrderFromOK(ref int Excp, ref string Msg, ArrayList ArrDeleted_Id, DataTable dt_NewOrder, DataTable dt_RawMaterials, DataTable dt_SubItem, Order_DetailsCls od, ref long OrderTable_id, DateTime dt_Order, string Order_Estlam_Sarf_Flage, ref bool MakeOrderTable_id, int OrderType, bool IS_EstlamRawMaterial)
        {
            //if (OrderType != 3 & OrderType != 4 & OrderType != 7)              /// ·Ì”  „— Ã⁄ ’—› √Ê  «” ·«„
            //{
            Excp = 0;
            string mysql = "";
            operations opp = new operations(1);

            //////Õ–› ≈–‰ «·Œ«„«  «·„’—Ê›… „⁄ «·«‰ «Ã
            mysql = " SELECT Order_Purshing.* FROM Order_Purshing WHERE(MainOrderTable_id = " + OrderTable_id + ") ";
            DataTable dttt = opp.return_DataTable(mysql);
            for (int x = 0; x < dttt.Rows.Count; x++)
            {
                long SubOrderTable_id = long.Parse(dttt.Rows[x]["Id"].ToString());
                int Order_Type = int.Parse(dttt.Rows[x]["Order_Type"].ToString());
                if (Order_Type == 10)
                {
                    DeleteOrder(ref Excp, ref Msg, ref opp, SubOrderTable_id);

                    mysql = "DELETE FROM Item_Raw_MaterialsFor_Entage WHERE(SubOrderTable_id =" + SubOrderTable_id + ")";
                    opp.insert_me(mysql);
                }

            }
            mysql = " Update Order_Purshing Set Order_date='" + operations.dateFormat(Order_Date) + "',NotesAll=N'" + NotesAll + "' Where Id=" + OrderTable_id;
            opp.insert_me(mysql);
            try
            {
                #region Make Deleted Item 
                for (int x = 0; x < ArrDeleted_Id.Count; x++)
                {
                    long Detailes_Purshing_Id = long.Parse(ArrDeleted_Id[x].ToString());



                    DataTable dt = Get_Order_Detailes_Purshing_Id(ref Excp, ref opp, Detailes_Purshing_Id);
                    long Item_id = long.Parse(dt.Rows[0]["Item_id"].ToString());
                    long Order_Type = long.Parse(dt.Rows[0]["Order_Type"].ToString());
                    long Order_Purshing_Id = long.Parse(dt.Rows[0]["OrderTable_id"].ToString());



                    ////Delete Main Item from Order
                    Delete_Order_DetailsFromEdit(ref Excp, ref opp, dt, 0);
                    if (Order_Type == 9)
                    {

                        //Delete_Item_Raw_MaterialsFor_Entage_FromEdit(ref Excp, ref opp, Item_id, Order_Purshing_Id);
                    }
                }

                #endregion


                for (int x = 0; x < dt_NewOrder.Rows.Count; x++)
                {
                    int IS_Editable = int.Parse(dt_NewOrder.Rows[x]["IS_Editable"].ToString());

                    if (IS_Editable == 1) ///  ⁄œÌ·
                    {
                        Edit_Order_DetailsFromEdit(ref Excp, ref opp, dt_NewOrder, x, 0, 0, 0);
                        if (Order_Type == 9)
                        {


                            //Edit_Item_Raw_MaterialsFor_Entage_FromEdit(ref Excp, ref opp,x, dt_NewOrder,   dt_RawMaterials,   dt_SubItem);
                        }
                    }
                    else if (IS_Editable == 2) /// ≈÷«›…
                    {
                        Add_NewItemIN_Order(ref Excp, ref opp, x, dt_NewOrder, OrderTable_id, Order_Estlam_Sarf_Flage);
                    }
                    else
                    {
                        continue;
                    }
                }



                //DeleteOrderFrom_Item_Raw_MaterialsFor_Entage(ref Excp, ref Msg, ref opp, OrderTable_id);
                if (Excp == 0)
                {
                    opp.Commit();
                    Msg = " „  ⁄œÌ· «·≈–‰ »‰Ã«Õ";
                    if (Order_Type == 9)
                    {
                        operations opp2 = new operations();
                        mysql = "SELECT Item_Raw_MaterialsFor_Entage_List_Id FROM Item_Raw_MaterialsFor_Entage_Details WHERE(Order_Purshing_Id = " + OrderTable_id + ")";
                        DataTable dtt = opp2.return_DataTable(mysql);
                        long Item_Raw_MaterialsFor_Entage_List_Id = 0;
                        if (dtt.Rows.Count > 0)
                        {
                            Item_Raw_MaterialsFor_Entage_List_Id = long.Parse(dtt.Rows[0]["Item_Raw_MaterialsFor_Entage_List_Id"].ToString());
                        }

                        string Notes_List = " „ ≈’œ«— «·≈–‰ Ê«·≈— »«ÿ »‘ﬂ· «Ê Ê„« ÌﬂÏ „‰ „⁄«œ·… «·«‰ «Ã";
                        Notes_List = Notes_List + " .. " + NotesAll;
                        mysql = " Update Item_Raw_MaterialsFor_Entage_List  Set Process_Date='" + operations.dateFormat(Order_Date) + "',Notes_List=N'" + Notes_List + "' Where Id=" + Item_Raw_MaterialsFor_Entage_List_Id;
                        opp2.insert_me(mysql);

                        mysql = "DELETE FROM Item_Raw_MaterialsFor_Entage_Details WHERE(Order_Purshing_Id = " + OrderTable_id + ")";
                        opp2.insert_me(mysql);
                        /////////////////////////////////

                        //intsert Estlam from entag
                        for (int index = 0; index < dt_NewOrder.Rows.Count; index++)
                        {
                            long Unit_Item_Table_Id = long.Parse(dt_NewOrder.Rows[index]["Unit_Item_Table_Id"].ToString());
                            long Item_id = long.Parse(dt_NewOrder.Rows[index]["Item_id"].ToString());
                            decimal Larg_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Larg_Qnty"].ToString());
                            decimal Small_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Small_Qnty"].ToString());
                            decimal Item_NO = decimal.Parse(dt_NewOrder.Rows[index]["Item_NO"].ToString());

                            decimal Balance_Now_Larg_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Balance_Now_Larg_Qnty"].ToString());
                            decimal Balance_Now_Small_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Balance_Now_Small_Qnty"].ToString());
                            decimal Balance_Now_Item_No = decimal.Parse(dt_NewOrder.Rows[index]["Balance_Now_Item_No"].ToString());
                            decimal S_unit_Value = decimal.Parse(dt_NewOrder.Rows[index]["S_unit_Value"].ToString());

                            string Item_Name = dt_NewOrder.Rows[index]["Item_Name"].ToString();
                            string B_Unit = dt_NewOrder.Rows[index]["B_Unit"].ToString();
                            string S_unit = dt_NewOrder.Rows[index]["S_unit"].ToString();
                            string Notes_From = dt_NewOrder.Rows[index]["Notes_From"].ToString();
                            string Notes_To = dt_NewOrder.Rows[index]["Notes_To"].ToString();
                            string Notes = dt_NewOrder.Rows[index]["Notes"].ToString();

                            Machine_Id = int.Parse(dt_NewOrder.Rows[index]["Machine_Id"].ToString()); ;
                            long Detailes_Purshing_Id_ = long.Parse(dt_NewOrder.Rows[index]["Detailes_Purshing_Id"].ToString());

                            long Related_Detailes_Purshing_Id = 0;

                            if (OrderType == 9 & IS_EstlamRawMaterial == true) //  «” ·«„ „‰ «·«‰ «Ã
                            {
                                mysql = "  INSERT INTO Item_Raw_MaterialsFor_Entage_Details(Item_Raw_MaterialsFor_Entage_List_Id, Unit_Item_Id, Item_Id, Store_Id, Larg_Qnty, Small_Qnty, Order_Purshing_Id, Detailes_Purshing_Id, Order_Estlam_Sarf_Flage)";
                                mysql = mysql + " VALUES(" + Item_Raw_MaterialsFor_Entage_List_Id + "," + Unit_Item_Table_Id + "," + Item_id + "," + Id_To + "," + Larg_Qnty + "," + Small_Qnty + "," + OrderTable_id + "," + Detailes_Purshing_Id_ + "," + Order_Estlam_Sarf_Flage + ") ";
                                opp2.insert_me(mysql);
                            }

                            if (Excp == 1)
                            {
                                Excp = 1;
                                Msg = "  Œÿ√ ›Ï ≈‰‘«¡ «·≈–‰";
                                opp2.Rollback();
                                return;


                            }
                        }
                        /////////////////////////////////
                        MakeNewOrderTOEntag_RawMaterials(ref Excp, ref Msg, ref opp2, dt_RawMaterials, dt_SubItem, OrderTable_id, dt_Order, Item_Raw_MaterialsFor_Entage_List_Id);
                        if (Excp == 0)
                        {
                            opp2.Commit();

                            Msg = " „  ⁄œÌ· «·≈–‰ »‰Ã«Õ";

                        }
                        Msg = " „  ⁄œÌ· «·≈–‰ »‰Ã«Õ";

                    }


                }



                if (Excp == 1)
                {
                    Excp = 1;
                    Msg = "  Œÿ√ ›Ï ≈‰‘«¡ «·≈–‰";
                    opp.Rollback();
                    return;
                }
                //if (Excp == 0)
                //{
                //    opp.Commit();
                //    operations opp2 = new operations();
                //    MakeNewOrderTOEntag_RawMaterials(ref Excp, ref Msg, ref opp2, dt_RawMaterials, dt_SubItem, OrderTable_id, dt_Order);
                //    if (Excp == 0)
                //    {
                //        opp2.Commit();

                //        Msg = " „  ⁄œÌ· «·≈–‰ »‰Ã«Õ";

                //    }
                //    Msg = " „  ⁄œÌ· «·≈–‰ »‰Ã«Õ";

                //}
            }
            catch (Exception ee)
            {

                Excp = 1;
                Msg = "  Œÿ√ ›Ï ≈‰‘«¡ «·≈–‰";
                opp.Rollback();
                return;




            }
        }
        public void Edit_Item_Raw_MaterialsFor_Entage_FromEdit(ref int Excp, ref operations opp, int x, DataTable dt_NewOrder, DataTable dt_RawMaterials, DataTable dt_SubItem)
        {

            long Order_Purshing_Id = long.Parse(dt_NewOrder.Rows[x]["OrderTable_id"].ToString());
            long Detailes_Purshing_Id = long.Parse(dt_NewOrder.Rows[x]["Detailes_Purshing_Id"].ToString());
            long Item_id = long.Parse(dt_NewOrder.Rows[x]["Item_id"].ToString());

            DataTable dt = Get_Item_Raw_MaterialsFor_Entage(ref Excp, ref opp, Order_Purshing_Id, Item_id);

            for (int y = 0; y < dt.Rows.Count; y++)
            {
                decimal New_Larg_Qnty = 0;
                decimal New_Small_Qnty = 0;
                decimal New_ItemNO = 0;
                long SubItem_Id = long.Parse(dt.Rows[y]["Item_id"].ToString());
                string expression = "SubItem_Id= " + SubItem_Id + "";
                DataRow[] foundRows;

                foundRows = dt_SubItem.Select(expression);
                //////delete sub raw item from details order
                if (foundRows.Length > 0)
                {
                    New_Larg_Qnty = decimal.Parse(foundRows[0]["Larg_Qnty"].ToString());
                    New_Small_Qnty = decimal.Parse(foundRows[0]["Small_Qnty"].ToString());
                    New_ItemNO = 0;
                }
                Edit_Order_DetailsFromEdit(ref Excp, ref opp, dt, y, New_Larg_Qnty, New_Small_Qnty, New_ItemNO);
                string Item_Raw_MaterialsFor_Entage_Id = dt.Rows[y]["Item_Raw_MaterialsFor_Entage_Id"].ToString();

                //string mysql = "Delete From Item_Raw_MaterialsFor_Entage Where Id = " + Item_Raw_MaterialsFor_Entage_Id;
                //opp.insert_me(mysql);
            }

        }
        public void UpdateNewBalanceOnTable(ref int Excp, ref DataTable dt_NewOrder_Estlam, ref DataTable dt_NewOrder_Sarf)
        {

            operations opp = new operations();

            for (int x = 0; x < dt_NewOrder_Estlam.Rows.Count; x++)
            {
                long Unit_itemTable_id = long.Parse(dt_NewOrder_Estlam.Rows[x]["Unit_Item_Table_Id"].ToString());
                string mysql = "Select Full_qua,D_Qua,Item_No from Unit_item Where Id=" + Unit_itemTable_id;
                DataTable dt = opp.return_DataTable(mysql);
                decimal Balance_Now_Larg_Qnty = decimal.Parse(dt.Rows[0]["Full_qua"].ToString());
                decimal Balance_Now_Small_Qnty = decimal.Parse(dt.Rows[0]["D_Qua"].ToString());
                decimal Balance_Now_Item_No = decimal.Parse(dt.Rows[0]["Item_No"].ToString());

                dt_NewOrder_Estlam.Rows[x]["Balance_Now_Larg_Qnty"] = Balance_Now_Larg_Qnty;
                dt_NewOrder_Estlam.Rows[x]["Balance_Now_Small_Qnty"] = Balance_Now_Small_Qnty;
                dt_NewOrder_Estlam.Rows[x]["Balance_Now_Item_No"] = Balance_Now_Item_No;

            }

            for (int x = 0; x < dt_NewOrder_Sarf.Rows.Count; x++)
            {
                long Unit_itemTable_id = long.Parse(dt_NewOrder_Sarf.Rows[x]["Unit_Item_Table_Id"].ToString());
                string mysql = "Select Full_qua,D_Qua,Item_No from Unit_item Where Id=" + Unit_itemTable_id;
                DataTable dt = opp.return_DataTable(mysql);
                decimal Balance_Now_Larg_Qnty = decimal.Parse(dt.Rows[0]["Full_qua"].ToString());
                decimal Balance_Now_Small_Qnty = decimal.Parse(dt.Rows[0]["D_Qua"].ToString());
                decimal Balance_Now_Item_No = decimal.Parse(dt.Rows[0]["Item_No"].ToString());

                dt_NewOrder_Sarf.Rows[x]["Balance_Now_Larg_Qnty"] = Balance_Now_Larg_Qnty;
                dt_NewOrder_Sarf.Rows[x]["Balance_Now_Small_Qnty"] = Balance_Now_Small_Qnty;
                dt_NewOrder_Sarf.Rows[x]["Balance_Now_Item_No"] = Balance_Now_Item_No;

            }


        }

        public void EditOrderFromOK(ref int Excp, ref string Msg, ref long Item_Raw_MaterialsFor_Entage_List_Id, ref long OrderTable_id, string Process_No, string Notes_List, DataTable dt_NewOrder_Estlam, DataTable dt_NewOrder_Sarf, DateTime dt_Order)
        {
            //OrderChoiceCLS oc = new OrderChoiceCLS();
            //Process_No = oc.GetNewItem_Raw_MaterialsFor_Entage_List().ToString();
            string mysql = "";
            operations opp = new operations(1);
            mysql = " Select * From Item_Raw_MaterialsFor_Entage_List Where Id=" + Item_Raw_MaterialsFor_Entage_List_Id;
            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                mysql = " Update Item_Raw_MaterialsFor_Entage_List Set   Process_No=" + Process_No + ", Notes_List='" + Notes_List + "', Process_Date='" + operations.dateFormat(dt_Order) + "' ";
                mysql = mysql + "  Where ID=" + Item_Raw_MaterialsFor_Entage_List_Id;
                opp.insert_me(mysql);
            }
            else
            {
                mysql = " INSERT INTO Item_Raw_MaterialsFor_Entage_List (Process_No, Notes_List, Process_Date) ";
                mysql = mysql + " VALUES(" + Process_No + ",'" + Notes_List + "','" + operations.dateFormat(dt_Order) + "')";
                Item_Raw_MaterialsFor_Entage_List_Id = opp.Insert_me_Return_Last_Identify(mysql);

            }
            //long Item_Raw_MaterialsFor_Entage_List_Id = opp.Insert_me_Return_Last_Identify(mysql);
            MakeNewOrderTOEntag_Estlam(ref Excp, ref Msg, ref opp, Item_Raw_MaterialsFor_Entage_List_Id, dt_NewOrder_Estlam, dt_Order);
            MakeNewOrderTOEntag_Sarf(ref Excp, ref Msg, ref opp, Item_Raw_MaterialsFor_Entage_List_Id, dt_NewOrder_Sarf, dt_Order);
            OrderTable_id = MainOrderTable_id;
            if (Excp == 0)
            {
                opp.Commit();
            }
            else
            {
                opp.Rollback();
            }
        }
        public void Make_NewOrderFromOK(ref int Excp, ref string Msg, ref long OrderTable_id, string Process_No, string Notes_List, DataTable dt_NewOrder_Estlam, DataTable dt_NewOrder_Sarf, DateTime dt_Order)
        {

            OrderChoiceCLS oc = new OrderChoiceCLS();
            Process_No = oc.GetNewItem_Raw_MaterialsFor_Entage_List().ToString();

            operations opp = new operations(1);
            string mysql = " INSERT INTO Item_Raw_MaterialsFor_Entage_List (Process_No, Notes_List, Process_Date) ";
            mysql = mysql + " VALUES(" + Process_No + ",'" + Notes_List + "','" + operations.dateFormat(dt_Order) + "')";
            long Item_Raw_MaterialsFor_Entage_List_Id = opp.Insert_me_Return_Last_Identify(mysql);
            MakeNewOrderTOEntag_Estlam(ref Excp, ref Msg, ref opp, Item_Raw_MaterialsFor_Entage_List_Id, dt_NewOrder_Estlam, dt_Order);
            MakeNewOrderTOEntag_Sarf(ref Excp, ref Msg, ref opp, Item_Raw_MaterialsFor_Entage_List_Id, dt_NewOrder_Sarf, dt_Order);
            OrderTable_id = MainOrderTable_id;
            if (Excp == 0)
            {
                opp.Commit();
            }
            else
            {
                opp.Rollback();
            }
        }
        void MakeNewOrderTOEntag_Estlam(ref int Excp, ref string Msg, ref operations opp, long Item_Raw_MaterialsFor_Entage_List_Id, DataTable dt_NewOrder, DateTime OrderDate)
        {
            //TransferItems id = new TransferItems(10);
            //id.Text = "≈–‰ ’—› ≈·Ï «·≈‰ «Ã";

            string Order_Estlam_Sarf_Flage = "1";
            Order_Type = 9;


            Type_To = 4;
            Id_To = 0;

            Type_From = 9;
            Id_From = 0;

            /////«·»ÕÀ ⁄‰ «—ﬁ«„ «·„Œ«“‰ ·⁄„· «–‰ ·ﬂ· „‰Â« 
            DataView view = new DataView(dt_NewOrder);
            DataTable dt_Stores = view.ToTable(true, "Store_Id");

            for (int y = 0; y < dt_Stores.Rows.Count; y++)
            {
                Id_To = int.Parse(dt_Stores.Rows[y]["Store_Id"].ToString());
                Excp = 0;
                // when creating new order ,it execute first time click button

                OrderChoiceCLS oc = new OrderChoiceCLS();
                long Sub_OrderTable_idNew = 0;
                long OrderNo = oc.GetNewOrderNo(ref opp, Order_Type);

                Order_Date = OrderDate;
                // od.Order_Purshing_No = long.Parse(txtOrderNo.Text);
                Make_New_Order_FromInvoice(ref Excp, ref opp, ref Sub_OrderTable_idNew);

                MakeItem_Raw_MaterialsFor_Entage_FromChange(ref Excp, ref Msg, ref opp, dt_NewOrder, Sub_OrderTable_idNew, OrderDate, Order_Estlam_Sarf_Flage);
                if (MainOrderTable_id != 0)
                {
                    MainOrderTable_id = Sub_OrderTable_idNew;
                }

                #region Insert Sub Item
                for (int index = 0; index < dt_NewOrder.Rows.Count; index++)
                {
                    long Store_id = long.Parse(dt_NewOrder.Rows[index]["Store_Id"].ToString());
                    string Store_Name = dt_NewOrder.Rows[index]["Store_Name"].ToString();
                    if (Store_id != Id_To)
                    {
                        continue;
                    }

                    long Item_id = long.Parse(dt_NewOrder.Rows[index]["Item_Id"].ToString());
                    //int Item_id = Convert.ToInt32(Item_id_);

                    long Unit_Item_Table_Id = 0;
                    decimal Balance_Now_Larg_Qnty = 0;
                    decimal Balance_Now_Small_Qnty = 0;
                    decimal Balance_Now_Item_No = 0;
                    //string mysql = "Select * from Unit_Item Where Item_Id=" + Item_id + " And Store_Id=" + Store_id;
                    //operations opp2 = new operations();
                    //DataTable dt2 = opp2.return_DataTable(mysql);
                    //if (dt2.Rows.Count > 0)
                    //{
                    //    Unit_Item_Table_Id = long.Parse(dt2.Rows[0]["Id"].ToString());
                    //    Balance_Now_Larg_Qnty = decimal.Parse(dt2.Rows[0]["Full_Qua"].ToString());
                    //    Balance_Now_Small_Qnty = decimal.Parse(dt2.Rows[0]["D_Qua"].ToString());
                    //    Balance_Now_Item_No = decimal.Parse(dt2.Rows[0]["Item_No"].ToString());

                    //}


                    Unit_Item_Table_Id = long.Parse(dt_NewOrder.Rows[index]["Unit_Item_Table_Id"].ToString());
                    Item_id = long.Parse(dt_NewOrder.Rows[index]["Item_id"].ToString());
                    decimal Larg_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Larg_Qnty"].ToString());
                    decimal Small_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Small_Qnty"].ToString());
                    decimal Item_NO = decimal.Parse(dt_NewOrder.Rows[index]["Item_NO"].ToString());

                    Balance_Now_Larg_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Balance_Now_Larg_Qnty"].ToString());
                    Balance_Now_Small_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Balance_Now_Small_Qnty"].ToString());
                    Balance_Now_Item_No = decimal.Parse(dt_NewOrder.Rows[index]["Balance_Now_Item_No"].ToString());
                    decimal S_unit_Value = decimal.Parse(dt_NewOrder.Rows[index]["S_unit_Value"].ToString());

                    string Item_Name = dt_NewOrder.Rows[index]["Item_Name"].ToString();
                    string B_Unit = dt_NewOrder.Rows[index]["B_Unit"].ToString();
                    string S_unit = dt_NewOrder.Rows[index]["S_unit"].ToString();
                    //string Notes_From = dt_NewOrder.Rows[index]["Notes_From"].ToString();
                    //string Notes_To = dt_NewOrder.Rows[index]["Notes_To"].ToString();
                    string Notes = dt_NewOrder.Rows[index]["Notes"].ToString();

                    Machine_Id = 0;// int.Parse(dt_NewOrder.Rows[index]["Machine_Id"].ToString()); ;
                    long FromStoreId = Common.GetColumnValueLong(dt_NewOrder, "FromStoreId", index);
                    long ToStoreId = Common.GetColumnValueLong(dt_NewOrder, "ToStoreId", index);

                    
                   


                    string Notes_Item = dt_NewOrder.Rows[index]["Notes_Item"].ToString();
                    string Notes_From = "  ≈” ·«„ „‰ «·≈‰ «Ã  ";
                    string Notes_To = "≈·Ï „Œ“‰" + Store_Name;
                    //string Notes = " „ ≈’œ«— «·≈–‰ «Ê Ê„« ÌﬂÌ« ";

                    long Related_Detailes_Purshing_Id = 0;

                    Make_Order_Details_AllOneClick(ref Excp, ref opp, ref Related_Detailes_Purshing_Id, Order_Estlam_Sarf_Flage, Sub_OrderTable_idNew, Unit_Item_Table_Id, Item_id, Larg_Qnty, Small_Qnty, Item_NO, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, Balance_Now_Item_No, S_unit_Value, Notes_From, Notes_To, Notes, Notes_Item,   FromStoreId, ToStoreId);

                    string mysql = "  INSERT INTO Item_Raw_MaterialsFor_Entage_Details(Item_Raw_MaterialsFor_Entage_List_Id, Unit_Item_Id, Item_Id, Store_Id, Larg_Qnty, Small_Qnty, Order_Purshing_Id, Detailes_Purshing_Id, Order_Estlam_Sarf_Flage,Notes_Item)";
                    mysql = mysql + " VALUES(" + Item_Raw_MaterialsFor_Entage_List_Id + "," + Unit_Item_Table_Id + "," + Item_id + "," + Store_id + "," + Larg_Qnty + "," + Small_Qnty + "," + Sub_OrderTable_idNew + "," + Related_Detailes_Purshing_Id + "," + Order_Estlam_Sarf_Flage + ",N'" + Notes_Item + "') ";
                    opp.insert_me(mysql);
                    //if (Excp == 1)
                    //{
                    //    Excp = 1;
                    //    Msg = "  Œÿ√ ›Ï ≈‰‘«¡ «·≈–‰";
                    //    opp.Rollback();
                    //    return;


                    //}
                }
                #endregion




            }


            ////////////////////////

            //if (Excp == 0)
            //{
            //    opp.Commit();
            //    Msg = " „ ≈’œ«— «·≈–‰ »‰Ã«Õ";

            //}
        }
        void MakeNewOrderTOEntag_Sarf(ref int Excp, ref string Msg, ref operations opp, long Item_Raw_MaterialsFor_Entage_List_Id, DataTable dt_NewOrder, DateTime OrderDate)
        {
            //TransferItems id = new TransferItems(10);
            //id.Text = "≈–‰ ’—› ≈·Ï «·≈‰ «Ã";

            string Order_Estlam_Sarf_Flage = "2";
            Order_Type = 10;

            Type_To = 10;
            Id_To = 10;

            Type_From = 4;
            Id_From = 0;

            /////«·»ÕÀ ⁄‰ «—ﬁ«„ «·„Œ«“‰ ·⁄„· «–‰ ·ﬂ· „‰Â« 
            DataView view = new DataView(dt_NewOrder);
            DataTable dt_Stores = view.ToTable(true, "Store_Id");

            for (int y = 0; y < dt_Stores.Rows.Count; y++)
            {
                Id_To = int.Parse(dt_Stores.Rows[y]["Store_Id"].ToString());
                Excp = 0;
                // when creating new order ,it execute first time click button

                OrderChoiceCLS oc = new OrderChoiceCLS();
                long Sub_OrderTable_idNew = 0;
                long OrderNo = oc.GetNewOrderNo(ref opp, Order_Type);

                Order_Date = OrderDate;
                // od.Order_Purshing_No = long.Parse(txtOrderNo.Text);
                Make_New_Order_FromInvoice(ref Excp, ref opp, ref Sub_OrderTable_idNew);
                if (MainOrderTable_id != 0)
                {
                    MainOrderTable_id = Sub_OrderTable_idNew;
                }

                MakeItem_Raw_MaterialsFor_Entage_FromChange(ref Excp, ref Msg, ref opp, dt_NewOrder, Sub_OrderTable_idNew, OrderDate, Order_Estlam_Sarf_Flage);
                #region Insert Sub Item
                for (int index = 0; index < dt_NewOrder.Rows.Count; index++)
                {
                    long Store_id = long.Parse(dt_NewOrder.Rows[index]["Store_Id"].ToString());
                    string Store_Name = dt_NewOrder.Rows[index]["Store_Name"].ToString();
                    if (Store_id != Id_To)
                    {
                        continue;
                    }

                    long Item_id = long.Parse(dt_NewOrder.Rows[index]["Item_Id"].ToString());
                    //int Item_id = Convert.ToInt32(Item_id_);

                    long Unit_Item_Table_Id = 0;
                    decimal Balance_Now_Larg_Qnty = 0;
                    decimal Balance_Now_Small_Qnty = 0;
                    decimal Balance_Now_Item_No = 0;
                    //string mysql = "Select * from Unit_Item Where Item_Id=" + Item_id + " And Store_Id=" + Store_id;
                    //operations opp2 = new operations();
                    //DataTable dt2 = opp2.return_DataTable(mysql);
                    //if (dt2.Rows.Count > 0)
                    //{
                    //    Unit_Item_Table_Id = long.Parse(dt2.Rows[0]["Id"].ToString());
                    //    Balance_Now_Larg_Qnty = decimal.Parse(dt2.Rows[0]["Full_Qua"].ToString());
                    //    Balance_Now_Small_Qnty = decimal.Parse(dt2.Rows[0]["D_Qua"].ToString());
                    //    Balance_Now_Item_No = decimal.Parse(dt2.Rows[0]["Item_No"].ToString());

                    //}

                    string Notes_Item = dt_NewOrder.Rows[index]["Notes_Item"].ToString();
                    Unit_Item_Table_Id = long.Parse(dt_NewOrder.Rows[index]["Unit_Item_Table_Id"].ToString());
                    Item_id = long.Parse(dt_NewOrder.Rows[index]["Item_id"].ToString());
                    decimal Larg_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Larg_Qnty"].ToString());
                    decimal Small_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Small_Qnty"].ToString());
                    decimal Item_NO = decimal.Parse(dt_NewOrder.Rows[index]["Item_NO"].ToString());

                    Balance_Now_Larg_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Balance_Now_Larg_Qnty"].ToString());
                    Balance_Now_Small_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Balance_Now_Small_Qnty"].ToString());
                    Balance_Now_Item_No = decimal.Parse(dt_NewOrder.Rows[index]["Balance_Now_Item_No"].ToString());
                    decimal S_unit_Value = decimal.Parse(dt_NewOrder.Rows[index]["S_unit_Value"].ToString());

                    string Item_Name = dt_NewOrder.Rows[index]["Item_Name"].ToString();
                    string B_Unit = dt_NewOrder.Rows[index]["B_Unit"].ToString();
                    string S_unit = dt_NewOrder.Rows[index]["S_unit"].ToString();
                    //string Notes_From = dt_NewOrder.Rows[index]["Notes_From"].ToString();
                    //string Notes_To = dt_NewOrder.Rows[index]["Notes_To"].ToString();
                    string Notes = dt_NewOrder.Rows[index]["Notes"].ToString();

                    Machine_Id = 0;// int.Parse(dt_NewOrder.Rows[index]["Machine_Id"].ToString()); ;


                    string Notes_From = "„‰ „Œ“‰  " + Store_Name; ;
                    string Notes_To = "’—› ≈·Ï «·≈‰ «Ã";
                    //string Notes = " „ ≈’œ«— «·≈–‰ «Ê Ê„« ÌﬂÌ« ";

                    long FromStoreId = Common.GetColumnValueLong(dt_NewOrder, "FromStoreId", index);
                    long ToStoreId = Common.GetColumnValueLong(dt_NewOrder, "ToStoreId", index);
                     
                   


                    long Related_Detailes_Purshing_Id = 0;

                    Make_Order_Details_AllOneClick(ref Excp, ref opp, ref Related_Detailes_Purshing_Id, Order_Estlam_Sarf_Flage, Sub_OrderTable_idNew, Unit_Item_Table_Id, Item_id, Larg_Qnty, Small_Qnty, Item_NO, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, Balance_Now_Item_No, S_unit_Value, Notes_From, Notes_To, Notes, Notes_Item, FromStoreId, ToStoreId);

                    string mysql = "  INSERT INTO Item_Raw_MaterialsFor_Entage_Details(Item_Raw_MaterialsFor_Entage_List_Id, Unit_Item_Id, Item_Id, Store_Id, Larg_Qnty, Small_Qnty, Order_Purshing_Id, Detailes_Purshing_Id, Order_Estlam_Sarf_Flage,Notes_Item)";
                    mysql = mysql + " VALUES(" + Item_Raw_MaterialsFor_Entage_List_Id + "," + Unit_Item_Table_Id + "," + Item_id + "," + Store_id + "," + Larg_Qnty + "," + Small_Qnty + "," + Sub_OrderTable_idNew + "," + Related_Detailes_Purshing_Id + "," + Order_Estlam_Sarf_Flage + ",N'" + Notes_Item + "') ";
                    opp.insert_me(mysql);
                    //if (Excp == 1)
                    //{
                    //    Excp = 1;
                    //    Msg = "  Œÿ√ ›Ï ≈‰‘«¡ «·≈–‰";
                    //    opp.Rollback();
                    //    return;


                    //}
                }
                #endregion




            }


            ////////////////////////

            //if (Excp == 0)
            //{
            //    opp.Commit();
            //    Msg = " „ ≈’œ«— «·≈–‰ »‰Ã«Õ";

            //}
        }

        public void Make_NewOrderFromOK(ref int Excp, ref string Msg, DataTable dt_NewOrder, DataTable dt_RawMaterials, DataTable dt_SubItem, Order_DetailsCls od, ref long OrderTable_id, DateTime dt_Order, string Order_Estlam_Sarf_Flage, ref bool MakeOrderTable_id, int OrderType, bool IS_EstlamRawMaterial)
        {
            //if (OrderType != 3 & OrderType != 4 & OrderType != 7)              /// ·Ì”  „— Ã⁄ ’—› √Ê  «” ·«„
            //{
            operations opp = new operations(1);
            try
            {

                //«· √ﬂœ „‰ —’Ìœ «·’‰› ›Ï Õ«·… «·’—›
                if (Order_Estlam_Sarf_Flage == "2")
                {
                    for (int index = 0; index < dt_NewOrder.Rows.Count; index++)
                    {
                        long Unit_Item_Table_Id = long.Parse(dt_NewOrder.Rows[index]["Unit_Item_Table_Id"].ToString());
                        decimal Larg_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Larg_Qnty"].ToString());
                        decimal Small_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Small_Qnty"].ToString());
                        string Item_Name = dt_NewOrder.Rows[index]["Item_Name"].ToString();


                        string mysql = "Select * from Unit_Item Where Id=" + Unit_Item_Table_Id;
                        DataTable dt = opp.return_DataTable(mysql);
                        if (dt.Rows.Count == 0)
                        {
                            Excp = 1;
                            opp.Rollback();
                            Msg="Œÿ√ ›Ï ≈÷«›… ’‰› ≈·Ï «·≈–‰" ;
                            return;
                        }


                        decimal Balance_Now_Larg_Qnty = decimal.Parse(dt.Rows[0]["Full_qua"].ToString());
                        decimal Balance_Now_Small_Qnty = decimal.Parse(dt.Rows[0]["D_Qua"].ToString());

                        decimal S_unit_Value = decimal.Parse(dt.Rows[0]["Small_unit"].ToString());

                        decimal X1 = 0;

                        Cls_Conversion_Unit clc = new Cls_Conversion_Unit();
                        if (!clc.ConVersionQuntity_Subtract_ToItem(true, Larg_Qnty, Small_Qnty, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, ref X1, ref X1, S_unit_Value))
                        {
                            Excp = 1;
                            Msg = " —’Ìœ «·’‰› €Ì— ﬂ«›Ï " + Item_Name;
                            opp.Rollback();
                            return;
                        }
                    }

                }
                ///////////////////////////////////////////////////////////////////////////////////////
                Excp = 0;
                // when creating new order ,it execute first time click button

                OrderChoiceCLS oc = new OrderChoiceCLS();
                OrderTable_id = 0;

                long OrderNo = oc.GetNewOrderNo(ref opp, OrderType);
                Order_Date = dt_Order;
                Order_Purshing_No = OrderNo;
                Make_New_Order_FromInvoice(ref Excp, ref opp, ref OrderTable_id);
                long Item_Raw_MaterialsFor_Entage_List_Id = 0;
                if (OrderType == 9 & IS_EstlamRawMaterial == true) //  «” ·«„ „‰ «·«‰ «Ã
                {
                    //OrderChoiceCLS oc = new OrderChoiceCLS();
                    string Process_No = oc.GetNewItem_Raw_MaterialsFor_Entage_List().ToString();
                    string Notes_List = " „ ≈’œ«— «·≈–‰ Ê«·≈— »«ÿ »‘ﬂ· «Ê Ê„« ÌﬂÏ „‰ „⁄«œ·… «·«‰ «Ã";
                    Notes_List = Notes_List + " .. " + NotesAll;

                    string mysql = " INSERT INTO Item_Raw_MaterialsFor_Entage_List (Process_No, Notes_List, Process_Date) ";
                    mysql = mysql + " VALUES(" + Process_No + ",'" + Notes_List + "','" + operations.dateFormat(dt_Order) + "')";
                    Item_Raw_MaterialsFor_Entage_List_Id = opp.Insert_me_Return_Last_Identify(mysql);
                }
                if (Excp > 0)
                {
                    Excp = 1;
                    Msg = "  Œÿ√ ›Ï ≈‰‘«¡ «·≈–‰";
                    opp.Rollback();
                    return;
                }
                MakeOrderTable_id = true;
                if (OrderType == 1 | OrderType == 6)
                {
                    if (Common.Make_Store_Order_Sarf_Estlam_Ta7weel_Automatic_Between_Stores == 1)
                    {
                        Order_DetailsCls od_C = new Order_DetailsCls();
                        od_C.NotesAll = NotesAll;
                        long OrderNo_C = oc.GetNewOrderNo(ref opp, OrderType);
                        od_C.Order_Purshing_No = OrderNo_C;
                        od_C.Order_Date = Order_Date;
                        od_C.Order_Type = Order_Type;
                        od_C.Type_From = Type_To;
                        od_C.Id_From = Id_To;
                        od_C.Type_To = Type_From;
                        od_C.Id_To = Id_From;
                        od_C.Id_FromStr = Id_ToStr;
                        od_C.Id_ToStr = Id_FromStr;

                        od_C.InvoiceTable_ID = InvoiceTable_ID;
                        od_C.MainOrderTable_id = OrderTable_id;
                        od_C.Is_Converted = 1;
                        od_C.Make_New_Order_FromInvoice(ref Excp, ref opp, ref OrderTable_id_C);

                        string mysql = "Update Order_Purshing Set  MainOrderTable_id =" + OrderTable_id_C + " Where Id=" + OrderTable_id;
                        opp.insert_me(mysql);

                        //long OrderTable_id_C = 0;
                        //bool MakeOrderTable_id_C = false;
                        //int OrderType_C = Order_Type;
                        //bool IS_EstlamRawMaterial_C = false;
                        //string Order_Estlam_Sarf_Flage_C = "1";
                        //od_C.Make_NewOrderFromOK(ref Excp, ref Msg, dt_NewOrder, dt_RawMaterials, dt_SubItem, od_C, ref OrderTable_id_C, dt_Order, Order_Estlam_Sarf_Flage_C, ref MakeOrderTable_id_C, OrderType_C, IS_EstlamRawMaterial_C);

                    }
                }
                ////Create Order Details
                for (int index = 0; index < dt_NewOrder.Rows.Count; index++)
                {
                    long Unit_Item_Table_Id = long.Parse(dt_NewOrder.Rows[index]["Unit_Item_Table_Id"].ToString());
                    long Item_id = long.Parse(dt_NewOrder.Rows[index]["Item_id"].ToString());
                    decimal Larg_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Larg_Qnty"].ToString());
                    decimal Small_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Small_Qnty"].ToString());
                    decimal Item_NO = decimal.Parse(dt_NewOrder.Rows[index]["Item_NO"].ToString());

                    decimal Balance_Now_Larg_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Balance_Now_Larg_Qnty"].ToString());
                    decimal Balance_Now_Small_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Balance_Now_Small_Qnty"].ToString());
                    decimal Balance_Now_Item_No = decimal.Parse(dt_NewOrder.Rows[index]["Balance_Now_Item_No"].ToString());
                    decimal S_unit_Value = decimal.Parse(dt_NewOrder.Rows[index]["S_unit_Value"].ToString());

                    string Item_Name = dt_NewOrder.Rows[index]["Item_Name"].ToString();
                    string B_Unit = dt_NewOrder.Rows[index]["B_Unit"].ToString();
                    string S_unit = dt_NewOrder.Rows[index]["S_unit"].ToString();
                    string Notes_From = dt_NewOrder.Rows[index]["Notes_From"].ToString();
                    string Notes_To = dt_NewOrder.Rows[index]["Notes_To"].ToString();
                    string Notes = dt_NewOrder.Rows[index]["Notes"].ToString();

                    Machine_Id = int.Parse(dt_NewOrder.Rows[index]["Machine_Id"].ToString());
                    long FromStoreId = Common.GetColumnValueLong(dt_NewOrder, "FromStoreId", index);
                    long ToStoreId = Common.GetColumnValueLong(dt_NewOrder, "ToStoreId", index);
                     
                    

                    long Related_Detailes_Purshing_Id = 0;
                    Make_Order_Details_AllOneClick(ref Excp, ref opp, ref Related_Detailes_Purshing_Id, Order_Estlam_Sarf_Flage, OrderTable_id, Unit_Item_Table_Id, Item_id, Larg_Qnty, Small_Qnty, Item_NO, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, Balance_Now_Item_No, S_unit_Value, Notes_From, Notes_To, Notes, "", FromStoreId, ToStoreId);
                    if (OrderType == 9 & IS_EstlamRawMaterial == true) //  «” ·«„ „‰ «·«‰ «Ã
                    {
                        string mysql = "  INSERT INTO Item_Raw_MaterialsFor_Entage_Details(Item_Raw_MaterialsFor_Entage_List_Id, Unit_Item_Id, Item_Id, Store_Id, Larg_Qnty, Small_Qnty, Order_Purshing_Id, Detailes_Purshing_Id, Order_Estlam_Sarf_Flage)";
                        mysql = mysql + " VALUES(" + Item_Raw_MaterialsFor_Entage_List_Id + "," + Unit_Item_Table_Id + "," + Item_id + "," + Id_To + "," + Larg_Qnty + "," + Small_Qnty + "," + OrderTable_id + "," + Related_Detailes_Purshing_Id + "," + Order_Estlam_Sarf_Flage + ") ";
                        opp.insert_me(mysql);
                    }

                    if (Excp == 1)
                    {
                        Excp = 1;
                        Msg = "  Œÿ√ ›Ï ≈‰‘«¡ «·≈–‰";
                        opp.Rollback();
                        return;


                    }
                }

                ////////////////////////
                if (OrderType == 9 & IS_EstlamRawMaterial == true) //  «” ·«„ „‰ «·«‰ «Ã
                {
                    MakeNewOrderTOEntag_RawMaterials(ref Excp, ref Msg, ref opp, dt_RawMaterials, dt_SubItem, OrderTable_id, dt_Order, Item_Raw_MaterialsFor_Entage_List_Id);
                    if (Excp == 0)
                    {
                        //MakeItem_Raw_MaterialsFor_Entage(ref Excp, ref Msg, ref opp, dt_RawMaterials, OrderTable_id,0, dt_Order);
                    }

                }
                if (Excp == 1)
                {
                    Excp = 1;
                    Msg = "  Œÿ√ ›Ï ≈‰‘«¡ «·≈–‰";
                    opp.Rollback();
                    return;
                }
                if (Excp == 0)
                {
                    opp.Commit();
                    Msg = " „ ≈’œ«— «·≈–‰ »‰Ã«Õ";

                }
            }
            catch (Exception ee)
            {

                Excp = 1;
                Msg = "  Œÿ√ ›Ï ≈‰‘«¡ «·≈–‰";
                opp.Rollback();
                return;




            }
        }

        void Add_NewItemIN_Order(ref int Excp, ref operations opp, int index, DataTable dt_NewOrder, long OrderTable_id, string Order_Estlam_Sarf_Flage)
        {

            long Unit_Item_Table_Id = long.Parse(dt_NewOrder.Rows[index]["Unit_Item_Table_Id"].ToString());
            long Item_id = long.Parse(dt_NewOrder.Rows[index]["Item_id"].ToString());
            decimal Larg_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Larg_Qnty"].ToString());
            decimal Small_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Small_Qnty"].ToString());
            decimal Item_NO = decimal.Parse(dt_NewOrder.Rows[index]["Item_NO"].ToString());

            decimal Balance_Now_Larg_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Balance_Now_Larg_Qnty"].ToString());
            decimal Balance_Now_Small_Qnty = decimal.Parse(dt_NewOrder.Rows[index]["Balance_Now_Small_Qnty"].ToString());
            decimal Balance_Now_Item_No = decimal.Parse(dt_NewOrder.Rows[index]["Balance_Now_Item_No"].ToString());
            decimal S_unit_Value = decimal.Parse(dt_NewOrder.Rows[index]["S_unit_Value"].ToString());

            string Item_Name = dt_NewOrder.Rows[index]["Item_Name"].ToString();
            string B_Unit = dt_NewOrder.Rows[index]["B_Unit"].ToString();
            string S_unit = dt_NewOrder.Rows[index]["S_unit"].ToString();
            string Notes_From = dt_NewOrder.Rows[index]["Notes_From"].ToString();
            string Notes_To = dt_NewOrder.Rows[index]["Notes_To"].ToString();
            string Notes = dt_NewOrder.Rows[index]["Notes"].ToString();
            long FromStoreId = Common.GetColumnValueLong(dt_NewOrder, "FromStoreId", index);
            long ToStoreId = Common.GetColumnValueLong(dt_NewOrder, "ToStoreId", index);
                     
                  
            Machine_Id = int.Parse(dt_NewOrder.Rows[index]["Machine_Id"].ToString()); ;

            long Related_Detailes_Purshing_Id = 0;

            Make_Order_Details_AllOneClick(ref Excp, ref opp, ref Related_Detailes_Purshing_Id, Order_Estlam_Sarf_Flage, OrderTable_id, Unit_Item_Table_Id, Item_id, Larg_Qnty, Small_Qnty, Item_NO, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, Balance_Now_Item_No, S_unit_Value, Notes_From, Notes_To, Notes, "", FromStoreId, ToStoreId);

        }
        void MakeNewOrderTOEntag_RawMaterials(ref int Excp, ref string Msg, ref operations opp, DataTable dt_RawMaterials, DataTable dt_subItem, long MainOrderTable_id_, DateTime OrderDate, long Item_Raw_MaterialsFor_Entage_List_Id)
        {
            //TransferItems id = new TransferItems(10);
            //id.Text = "≈–‰ ’—› ≈·Ï «·≈‰ «Ã";

            string Order_Estlam_Sarf_Flage = "2";
            Order_Type = 10;

            Type_To = 10;
            Id_To = 10;

            Type_From = 4;
            Id_From = 0;

            /////«·»ÕÀ ⁄‰ «—ﬁ«„ «·„Œ«“‰ ·⁄„· «–‰ ·ﬂ· „‰Â« 
            DataView view = new DataView(dt_RawMaterials);
            DataTable dt_Stores = view.ToTable(true, "S_id");

            for (int y = 0; y < dt_Stores.Rows.Count; y++)
            {
                Id_From = int.Parse(dt_Stores.Rows[y]["S_Id"].ToString());
                Excp = 0;
                // when creating new order ,it execute first time click button

                OrderChoiceCLS oc = new OrderChoiceCLS();
                long Sub_OrderTable_idNew = 0;
                Order_Purshing_No = oc.GetNewOrderNo(ref opp, Order_Type);
                MainOrderTable_id = MainOrderTable_id_;
                Order_Date = OrderDate;
                // od.Order_Purshing_No = long.Parse(txtOrderNo.Text);
                Make_New_Order_FromInvoice(ref Excp, ref opp, ref Sub_OrderTable_idNew);


                #region Insert Sub Item
                for (int index = 0; index < dt_subItem.Rows.Count; index++)
                {
                    long Store_id = long.Parse(dt_subItem.Rows[index]["S_id"].ToString());
                    string Store_Name = dt_subItem.Rows[index]["Store_Name"].ToString();
                    if (Store_id != Id_From)
                    {
                        continue;
                    }

                    decimal Item_id_ = decimal.Parse(dt_subItem.Rows[index]["SubItem_Id"].ToString());
                    int Item_id = Convert.ToInt32(Item_id_);

                    long Unit_Item_Table_Id = 0;
                    decimal Balance_Now_Larg_Qnty = 0;
                    decimal Balance_Now_Small_Qnty = 0;
                    decimal Balance_Now_Item_No = 0;
                    string mysql = "Select * from Unit_Item Where Item_Id=" + Item_id + " And Store_Id=" + Store_id;
                    operations opp2 = new operations();
                    DataTable dt2 = opp2.return_DataTable(mysql);
                    if (dt2.Rows.Count > 0)
                    {
                        Unit_Item_Table_Id = long.Parse(dt2.Rows[0]["Id"].ToString());
                        Balance_Now_Larg_Qnty = decimal.Parse(dt2.Rows[0]["Full_Qua"].ToString());
                        Balance_Now_Small_Qnty = decimal.Parse(dt2.Rows[0]["D_Qua"].ToString());
                        Balance_Now_Item_No = decimal.Parse(dt2.Rows[0]["Item_No"].ToString());

                    }


                    decimal Larg_Qnty = decimal.Parse(dt_subItem.Rows[index]["Larg_Qnty"].ToString());
                    decimal Small_Qnty = decimal.Parse(dt_subItem.Rows[index]["Small_Qnty"].ToString());
                    decimal Item_NO = 0;
                    decimal Qnty = decimal.Parse(dt_subItem.Rows[index]["Qnty"].ToString());

                    decimal S_unit_Value = decimal.Parse(dt_subItem.Rows[index]["S_unit_Value"].ToString());

                    string Item_Name = dt_subItem.Rows[index]["SubItem_Name"].ToString();
                    string B_Unit = dt_subItem.Rows[index]["Big_unit"].ToString();
                    string S_unit = dt_subItem.Rows[index]["Small_unit"].ToString();
                    string Notes_From = "„‰ „Œ“‰  " + Store_Name; ;
                    string Notes_To = "’—› ≈·Ï «·≈‰ «Ã";
                    string Notes = " „ ≈’œ«— «·≈–‰ «Ê Ê„« ÌﬂÌ« ";


                    long Related_Detailes_Purshing_Id = 0;
                    long FromStoreId = Common.GetColumnValueLong(dt_subItem, "FromStoreId", index);
                    long ToStoreId = Common.GetColumnValueLong(dt_subItem, "ToStoreId", index);
                    
                    Make_Order_Details_AllOneClick(ref Excp, ref opp, ref Related_Detailes_Purshing_Id, Order_Estlam_Sarf_Flage, Sub_OrderTable_idNew, Unit_Item_Table_Id, Item_id, Larg_Qnty, Small_Qnty, Item_NO, Balance_Now_Larg_Qnty, Balance_Now_Small_Qnty, Balance_Now_Item_No, S_unit_Value, Notes_From, Notes_To, Notes, "", FromStoreId, ToStoreId);

                    mysql = "  INSERT INTO Item_Raw_MaterialsFor_Entage_Details(Item_Raw_MaterialsFor_Entage_List_Id, Unit_Item_Id, Item_Id, Store_Id, Larg_Qnty, Small_Qnty, Order_Purshing_Id, Detailes_Purshing_Id, Order_Estlam_Sarf_Flage)";
                    mysql = mysql + " VALUES(" + Item_Raw_MaterialsFor_Entage_List_Id + "," + Unit_Item_Table_Id + "," + Item_id + "," + Store_id + "," + Larg_Qnty + "," + Small_Qnty + "," + Sub_OrderTable_idNew + "," + Related_Detailes_Purshing_Id + "," + Order_Estlam_Sarf_Flage + ") ";

                    opp.insert_me(mysql);
                    //if (Excp == 1)
                    //{
                    //    Excp = 1;
                    //    Msg = "  Œÿ√ ›Ï ≈‰‘«¡ «·≈–‰";
                    //    opp.Rollback();
                    //    return;


                    //}
                }
                #endregion


                DataRow[] foundRows;


                foundRows = dt_RawMaterials.Select("S_Id=" + Id_From + "");
                DataTable dt = foundRows.CopyToDataTable();
                MakeItem_Raw_MaterialsFor_Entage(ref Excp, ref Msg, ref opp, dt, MainOrderTable_id_, Sub_OrderTable_idNew, OrderDate);
            }


            ////////////////////////

            //if (Excp == 0)
            //{
            //    opp.Commit();
            //    Msg = " „ ≈’œ«— «·≈–‰ »‰Ã«Õ";

            //}
        }

        void MakeItem_Raw_MaterialsFor_Entage(ref int Excp, ref string Msg, ref operations opp, DataTable dt_RawMaterials, long MainOrderTable_id_, long SubOrderTable_id, DateTime Process_Date)
        {
            //
            try
            {
                long MainOrderTable_id = MainOrderTable_id_;


                for (int x = 0; x < dt_RawMaterials.Rows.Count; x++)
                {
                    long SubUnit_item_id = long.Parse(dt_RawMaterials.Rows[x]["SubUnit_item_id"].ToString());

                    long MainItem_Id = long.Parse(dt_RawMaterials.Rows[x]["MainItem_Id"].ToString());
                    long SubItem_Id = long.Parse(dt_RawMaterials.Rows[x]["SubItem_Id"].ToString());
                    string MainItem_Name = dt_RawMaterials.Rows[x]["MainItem_Name"].ToString();
                    string SubItem_Name = dt_RawMaterials.Rows[x]["SubItem_Name"].ToString();
                    string Big_unit = dt_RawMaterials.Rows[x]["Big_unit"].ToString();
                    string Small_unit = dt_RawMaterials.Rows[x]["Small_unit"].ToString();
                    int Unit_Id = int.Parse(dt_RawMaterials.Rows[x]["Unit_Id"].ToString());
                    decimal S_unit_Value = decimal.Parse(dt_RawMaterials.Rows[x]["S_unit_Value"].ToString());

                    decimal Larg_Qnty = decimal.Parse(dt_RawMaterials.Rows[x]["Larg_Qnty"].ToString());
                    decimal Small_Qnty = decimal.Parse(dt_RawMaterials.Rows[x]["Small_Qnty"].ToString());
                    decimal Qnty = decimal.Parse(dt_RawMaterials.Rows[x]["Qnty"].ToString());


                    decimal Raw_Full_qua = decimal.Parse(dt_RawMaterials.Rows[x]["Full_qua"].ToString());
                    decimal Raw_D_Qua = decimal.Parse(dt_RawMaterials.Rows[x]["D_Qua"].ToString());
                    decimal Raw_Quantity = decimal.Parse(dt_RawMaterials.Rows[x]["Quantity"].ToString());

                    decimal Main_Larg_Qnty = decimal.Parse(dt_RawMaterials.Rows[x]["Entry_Larg_Qnty"].ToString());
                    decimal Main_Small_Qnty = decimal.Parse(dt_RawMaterials.Rows[x]["Entry_Small_Qnty"].ToString());
                    decimal Main_Qnty = decimal.Parse(dt_RawMaterials.Rows[x]["Entry_Qnty"].ToString());

                    string SubStore_Name = dt_RawMaterials.Rows[x]["Store_Name"].ToString();
                    string SubStore_Id = dt_RawMaterials.Rows[x]["S_Id"].ToString();
                    string MainStore_Id = dt_RawMaterials.Rows[x]["MainStore_Id"].ToString();
                    string mysql = "INSERT INTO Item_Raw_MaterialsFor_Entage(MainOrderTable_id,SubOrderTable_id, Process_Date, MainItem_Id, SubItem_Id, MainItem_Name, SubItem_Name, Big_unit, Small_unit, Unit_Id, S_unit_Value, Raw_Full_qua, Raw_D_Qua, Raw_Quantity, Larg_Qnty, Small_Qnty,Qnty, Main_Larg_Qnty, Main_Small_Qnty, Main_Qnty,SubUnit_item_id, MainStore_Id, SubStore_Id,SubStore_Name) ";
                    mysql = mysql + " VALUES(" + MainOrderTable_id + "," + SubOrderTable_id + ",'" + operations.dateFormat(Process_Date) + "'," + MainItem_Id + "," + SubItem_Id + ",'" + MainItem_Name + "','" + SubItem_Name + "','" + Big_unit + "','" + Small_unit + "'," + Unit_Id + "," + S_unit_Value + "," + Raw_Full_qua + "," + Raw_D_Qua + "," + Raw_Quantity + "," + Larg_Qnty + "," + Small_Qnty + "," + Qnty + "," + Main_Larg_Qnty + "," + Main_Small_Qnty + "," + Main_Qnty + "," + SubUnit_item_id + "," + MainStore_Id + ", " + SubStore_Id + ",'" + SubStore_Name + "')";
                    opp.insert_me(mysql);
                }
            }
            catch (Exception ee)
            {

                Excp = 1;
                Msg = ee.Message;
            }
        }

        void MakeItem_Raw_MaterialsFor_Entage_FromChange(ref int Excp, ref string Msg, ref operations opp, DataTable dt_NewOrder, long OrderTable_id, DateTime Process_Date, string Order_Estlam_Sarf_Flage)
        {
            long SubUnit_item_id = 0;

            long MainItem_Id = 0;
            long SubItem_Id = 0;
            string MainItem_Name = "";
            string SubItem_Name = "";
            string Big_unit = "";
            string Small_unit = "";
            int Unit_Id = 0;
            decimal S_unit_Value = 0;

            decimal Larg_Qnty = 0;
            decimal Small_Qnty = 0;
            decimal Qnty = 0;


            decimal Raw_Full_qua = 0;
            decimal Raw_D_Qua = 0;
            decimal Raw_Quantity = 0;

            decimal Main_Larg_Qnty = 0;
            decimal Main_Small_Qnty = 0;
            decimal Main_Qnty = 0;

            string SubStore_Name = "";
            string SubStore_Id = "";
            string MainStore_Id = "";

            long Unit_Item_Table_Id = 0;

            long MainOrderTable_id = 0;
            long SubOrderTable_id = 0;

            //
            try
            {



                for (int x = 0; x < dt_NewOrder.Rows.Count; x++)
                {
                    try
                    {
                        Unit_Id = int.Parse(dt_NewOrder.Rows[x]["Unit_Id"].ToString());
                    }
                    catch
                    {
                        Unit_Id = 0;
                    }
                    string Notes_Item = dt_NewOrder.Rows[x]["Notes_Item"].ToString();
                    if (Order_Estlam_Sarf_Flage == "1")
                    {

                        SubUnit_item_id = 0;
                        SubItem_Id = 0;
                        SubItem_Name = "";
                        SubStore_Name = "";
                        SubStore_Id = "0";


                        MainItem_Id = long.Parse(dt_NewOrder.Rows[x]["Item_id"].ToString());
                        MainItem_Name = dt_NewOrder.Rows[x]["Item_Name"].ToString();

                        Big_unit = dt_NewOrder.Rows[x]["B_Unit"].ToString();
                        Small_unit = dt_NewOrder.Rows[x]["S_unit"].ToString();

                        S_unit_Value = decimal.Parse(dt_NewOrder.Rows[x]["S_unit_Value"].ToString());
                        Larg_Qnty = decimal.Parse(dt_NewOrder.Rows[x]["Larg_Qnty"].ToString());
                        Small_Qnty = decimal.Parse(dt_NewOrder.Rows[x]["Small_Qnty"].ToString());
                        Qnty = Common.Get_Item_Quantity(Larg_Qnty, Small_Qnty, S_unit_Value);

                        Main_Larg_Qnty = Larg_Qnty;
                        Main_Small_Qnty = Small_Qnty;
                        Main_Qnty = Qnty;
                        MainStore_Id = dt_NewOrder.Rows[x]["Store_Id"].ToString();
                        Larg_Qnty = 0;
                        Small_Qnty = 0;
                        Qnty = 0;
                        Unit_Item_Table_Id = long.Parse(dt_NewOrder.Rows[x]["Unit_Item_Table_Id"].ToString());
                        MainOrderTable_id = OrderTable_id;
                        SubOrderTable_id = 0;

                    }
                    if (Order_Estlam_Sarf_Flage == "2")
                    {
                        Unit_Item_Table_Id = long.Parse(dt_NewOrder.Rows[x]["Unit_Item_Table_Id"].ToString());
                        SubUnit_item_id = Unit_Item_Table_Id;
                        SubItem_Id = long.Parse(dt_NewOrder.Rows[x]["Item_id"].ToString());
                        SubItem_Name = dt_NewOrder.Rows[x]["Item_Name"].ToString();
                        SubStore_Name = "";
                        SubStore_Id = dt_NewOrder.Rows[x]["Store_Id"].ToString();


                        MainItem_Id = 0;
                        MainItem_Name = "";

                        Big_unit = dt_NewOrder.Rows[x]["B_Unit"].ToString();
                        Small_unit = dt_NewOrder.Rows[x]["S_unit"].ToString();

                        S_unit_Value = decimal.Parse(dt_NewOrder.Rows[x]["S_unit_Value"].ToString());
                        Larg_Qnty = decimal.Parse(dt_NewOrder.Rows[x]["Larg_Qnty"].ToString());
                        Small_Qnty = decimal.Parse(dt_NewOrder.Rows[x]["Small_Qnty"].ToString());
                        Qnty = Common.Get_Item_Quantity(Larg_Qnty, Small_Qnty, S_unit_Value);

                        Main_Larg_Qnty = 0;
                        Main_Small_Qnty = 0;
                        Main_Qnty = 0;
                        MainStore_Id = "0";
                        MainOrderTable_id = 0;
                        SubOrderTable_id = OrderTable_id;
                    }
                    string mysql = "INSERT INTO Item_Raw_MaterialsFor_Entage(MainOrderTable_id,SubOrderTable_id, Process_Date, MainItem_Id, SubItem_Id, MainItem_Name, SubItem_Name, Big_unit, Small_unit, Unit_Id, S_unit_Value, Raw_Full_qua, Raw_D_Qua, Raw_Quantity, Larg_Qnty, Small_Qnty,Qnty, Main_Larg_Qnty, Main_Small_Qnty, Main_Qnty,SubUnit_item_id, MainStore_Id, SubStore_Id,SubStore_Name,Notes_Item) ";
                    mysql = mysql + " VALUES(" + MainOrderTable_id + "," + SubOrderTable_id + ",'" + operations.dateFormat(Process_Date) + "'," + MainItem_Id + "," + SubItem_Id + ",'" + MainItem_Name + "','" + SubItem_Name + "','" + Big_unit + "','" + Small_unit + "'," + Unit_Id + "," + S_unit_Value + "," + Raw_Full_qua + "," + Raw_D_Qua + "," + Raw_Quantity + "," + Larg_Qnty + "," + Small_Qnty + "," + Qnty + "," + Main_Larg_Qnty + "," + Main_Small_Qnty + "," + Main_Qnty + "," + SubUnit_item_id + "," + MainStore_Id + ", " + SubStore_Id + ",'" + SubStore_Name + "',N'" + Notes_Item + "')";
                    opp.insert_me(mysql);
                }
            }
            catch (Exception ee)
            {

                Excp = 1;
                Msg = ee.Message;
            }
        }

    }
}