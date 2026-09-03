     using AccountModelsLib;  
using AccountModelsLib.Models;
 
 
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
 
namespace AccountModelsLib.Models
{


    public class ItemCategoryCLS
    {
        operations opp;
        DataSet machineds;
        public ItemCategoryCLS()
        {
            opp = new operations();
            machineds = new DataSet();
        }
         
        public ItemCategoryCLS(ref operations _opp)
        {
            opp = _opp;
            machineds = new DataSet();
        }

        public DataSet GetMainCategory_Name_list()
        {
            string mysql = "SELECT Item_Main_Category.* FROM Item_Main_Category Order By Category_Type_Id, Main_Category_Name";
            machineds = opp.return_dataset(mysql);
            return machineds;
        }




        /// ////////////////////////////////////

        public void Add_MainCategory_Name(Item_Main_Category ic)
        {
            string sql;
            sql = "INSERT INTO Item_Main_Category(Main_Category_Name,Category_Type_Id,Category_Type_Str)";
            sql += "VALUES('" + ic.Main_Category_Name + " '," + ic.Category_Type_Id + ",'" + ic.Category_Type_Str + "')";
            opp.insert_me(sql);

        }



        public void Update_MainCategory_Name(Item_Main_Category ic)
        {

            string mysql = "UPDATE Item_Main_Category SET Main_Category_Name = '" + ic.Main_Category_Name + "',Category_Type_Id=" + ic.Category_Type_Id + " ,Category_Type_Str='" + ic.Category_Type_Str + "' Where id=" + ic.ID;
            opp.update_me(mysql);
        }

        public void Delete_MainCategory_Name(int id)
        {

            string mysql = " Delete from Item_Main_Category   Where id=" + id;
            opp.update_me(mysql);
        }


        /// <summary>
        /// /////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <returns></returns>
        public DataSet GetSubCategory_Name_list()
        {
            string mysql = "SELECT Item_Category.* FROM Item_Category Order By  Category_Name,Category_Type_Id";
            machineds = opp.return_dataset(mysql);
            return machineds;
        }



        public DataSet Get_Main_Sub_Category_Name_list(int OrderBy, long Main_Item_Category_Id, int Category_Type_Id)
        {
            string mysql = @"SELECT        Item_Category.*, Item_Main_Category.Main_Category_Name  
                        
FROM            Item_Category LEFT OUTER JOIN
                         Item_Main_Category ON Item_Category.Main_Item_Category_Id = Item_Main_Category.ID
 ";
            mysql = mysql + " Where Item_Category.ID > 0 ";
            if (Main_Item_Category_Id > 0)
            {
                mysql = mysql + " AND Item_Category.Main_Item_Category_Id=" + Main_Item_Category_Id + "";
            }
            if (Category_Type_Id > 0)
            {
                mysql = mysql + " AND Item_Category.Category_Type_Id=" + Category_Type_Id + "";

            }
            if (OrderBy == 1)
            {
                mysql = mysql + "  ORDER BY Item_Main_Category.Category_Type_Id, Item_Category.Category_Type_Id, Item_Main_Category.Main_Category_Name ";

            }
            else
            {
                // mysql = mysql + "  ORDER BY Item_Category.,Item_Main_Category.Category_Type_Id, Item_Category.Category_Type_Id, Item_Main_Category.Main_Category_Name ";

                mysql = mysql + "  Order By Item_Category.Category_Type_Id,Item_Category.IS_Active Desc ";
            }


            machineds = opp.return_dataset(mysql);
            return machineds;
        }

        public void Add_SubCategory_Name(Item_Category ic)
        {

            //My_DbContext db = new My_DbContext();
            //db.item_Category.Add(ic);

            string sql;
            sql = "INSERT INTO Item_Category(Category_Name,Category_Type_Id,Category_Type_Str,Main_Item_Category_Id)";
            sql += "VALUES('" + ic.Category_Name + " '," + ic.Category_Type_Id + ",'" + ic.Category_Type_Str + "'," + ic.Main_Item_Category_Id + ")";
            opp.insert_me(sql);

        }

        public void Update_SubCategory_Name(Item_Category ic)
        {

            string mysql = "UPDATE Item_Category SET Main_Item_Category_Id=" + ic.Main_Item_Category_Id + ",Category_Name = '" + ic.Category_Name + "',Category_Type_Id=" + ic.Category_Type_Id + " ,Category_Type_Str='" + ic.Category_Type_Str + "' Where id=" + ic.ID;
            opp.update_me(mysql);
        }
        public void Delete_SubCategory_Name(int id)
        {

            string mysql = "Delete From Item_Category   Where id=" + id;
            opp.update_me(mysql);
        }

        public void Update_SubCategory_Name(int id, int Value)
        {

            string mysql = "UPDATE Item_Category SET IS_Active=" + Value + "    Where id=" + id;
            opp.update_me(mysql);


            mysql = @"UPDATE       Item
SET                  Active_flage =-1
FROM            Item INNER JOIN
                         Unit_item ON Item.id = Unit_item.Item_id AND Item.S_id = Unit_item.Store_id INNER JOIN
                         Item_Category ON Item.Item_Category_ID = Item_Category.ID
WHERE        (Item_Category.ID = " + id + ")";
            opp.update_me(mysql);

            mysql = @"UPDATE       Unit_item
SET                active_flage = - 1
FROM            Item INNER JOIN
                         Unit_item ON Item.id = Unit_item.Item_id AND Item.S_id = Unit_item.Store_id INNER JOIN
                         Item_Category ON Item.Item_Category_ID = Item_Category.ID
WHERE        (Item_Category.ID =  " + id + ")";
            opp.update_me(mysql);
        }

        public List<Item_Category> GetItem_Category()
        {
            operations opp = new operations();
            return GetItem_Category(ref opp);
        }

        public List<Item_Category> GetItem_Category(ref operations opp)
        {
            List<Item_Category> Item_CategoryList = opp.db.item_Category.ToList();
            return Item_CategoryList;
        }

        public Item_Category GetServiceDefault_Category()
        {
            Item_Category item_Category = opp.db.item_Category.Where(i => i.Is_ItemOrService == 2 && i.Is_Default == 1).FirstOrDefault();
            if (item_Category == null)
            {
                  item_Category = new Item_Category();
                int Excp = 0;

                //long Value = (int)txtCode.Value;
                item_Category.Category_Name = "فئة الخدمات";

                item_Category.Is_ItemOrService = 2;


                item_Category.Is_Default = 1;

                item_Category.Category_Type_Id = 1;

                item_Category.Category_Type_Str = "مبيعات";
                item_Category.Main_Item_Category_Id = 1;
                opp.db.item_Category.Add(item_Category);
                opp.db.SaveChanges();
            }
            return item_Category;
        }
    }

   
}
