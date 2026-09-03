using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AccountModelsLib.Models 
{
    public class ServiceLogic
    {
       public  operations opp;
        public ServiceLogic( )
        {
            opp = new operations();
        }
        public ServiceLogic(operations operation)
        {
            opp = operation;
        }

        void SetItemServiceData(ref Items im,ref Unit_item um)
        {
            StoreCLS storeCLS = new StoreCLS(ref opp);
            Store st = storeCLS.CheckServiceStoreFound();

            unitCLS unitCLS = new unitCLS(ref opp);
            unit _unit= unitCLS.GetUnitIS_Default();

            ItemCategoryCLS item_CategoryCLS = new ItemCategoryCLS();
            Item_Category item_Category = item_CategoryCLS.GetServiceDefault_Category( );
            

            long Store_id = st.Id;
             
            int Is_ItemWithoutBalance = 1;


            //item.Item_Name = txtName.Text;
            //item.AnotherName = txtName.Text;
            //item.S_Price = decimal.Parse(numPrice.Text);
            //item.ServiceDurationInMinutes = (int)numDuration.Value;
            //item.IsCommissionEmployeeBased = Common.Get_TrueOrFalseValue(chkIsCommissionEmployeeBased.Checked);
            //item.CommissionEmployeeValue = decimal.Parse(numCommissionValue.Text);
            //item.ServiceCategoryId = (long)cmbCategory.SelectedValue;
            //item.Is_ItemOrService = 2;


            im.OperationDate = DateTime.Now.Date;

            im.Height_Width_Str = "";// txtHeight_Width_Str.Text;
            im.Items_Name_Somk_Str = "";// txtItems_Name_Somk_Str.Text;
            //im.AnotherName = txtAnontherName.Text;
             
             
            im.code ="0";
            //im.Item_Name = txtItemName.Text;
            im.ItemName_OnInvoice = im.Item_Name;
            //im.AnotherName = im.Item_Name;

            im.S_id = Store_id;
            im.Item_Category_ID = item_Category.ID;
            
            im.No3eboa = 0;
            im.Unit_Id = _unit.id;
            im.UnitStr = _unit.B_Unit;
            //im.S_Price = txtS_price_BigUnit.Value; 
            im.S_priceAvgCost = 0;
            im.S_price_Item_No = 0;
            im.S_price_LargeUnit = im.S_Price;
            im.S_price_SmallUnit = im.S_Price;

            im.P_Price = 0;
            im.P_priceAvgCost = 0;
            im.P_price_Item_No = 0;
            im.P_price_LargeUnit = im.P_Price;
            im.P_price_SmallUnit = im.P_Price;


            im.Upon_Request =0;
            im.Width_ = 0;// txtWidth.Value;
            im.Height_ = 0;//txtHeight.Value;
            im.Somk = 0;// txtSomk.Value;
            im.Somk_Unit = "";// txtSomk_Unit.Text;

            im.Items_Types_Name_Id = 0;
            im.Items_TypesDescription_NameId = 0;
            im.Items_TypesDescriptionNameOtherId = 0;
            im.Items_TypesFactory_NameId = 0;
            im.Items_TypesSize_NameId = 0;
            im.Items_TypesColor_NameId = 0;
            im.Items_TypesDegreeFarz_NameId = 0;



            //im.Items_Types_Name = items_Types_NameUSR1.SelectedText;
            //im.Items_TypesDescription_Name = items_TypesDescription_NameUSR1.SelectedText;
            //im.Items_TypesDescriptionNameOther = items_TypesDescriptionNameOtherUSR1.SelectedText;
            //im.Items_TypesFactory_Name = items_TypesFactory_NameUSR1.SelectedText;
            //im.Items_TypesSize_Name = items_TypesSize_NameUSR1.SelectedText;
            //im.Items_TypesColor_Name = items_TypesColor_NameUSR1.SelectedText;
            //im.Items_TypesDegreeFarz_Name = items_TypesDegreeFarz_NameUSR1.SelectedText;
            im.Is_ItemWithoutBalance = Is_ItemWithoutBalance;

         




            um.Store_id = Store_id;
            um.Store_Type = 4;
            um.Code ="0";
            um.unit_id = _unit.id;
            um.Full_qua = 0;
            um.D_Qua = 0;
            um.Item_No = 0;
            um.Reserved_Balance_Larg_Qnty = 0;
            um.Reserved_Balance_Small_Qnty = 0;
             

                um.Big_unit = _unit.B_Unit_Value;
            
            um.Small_unit = _unit.S_unit_Value;
            um.P_price =0;
            um.S_price = im.S_Price;


            um.S_priceAvgCost = 0;
            um.S_price_Item_No = 0;
            um.S_price_LargeUnit = im.S_Price;  
            um.S_price_SmallUnit = im.S_Price;


            um.P_priceAvgCost = 0;
            um.P_price_Item_No = 0;
            um.P_price_LargeUnit = 0;
            um.P_price_SmallUnit = 0;

           
            
            um.Upon_Request =0;
            um.TaxSales10_Value = 0; 

            um.Is_ItemWithoutBalance = Is_ItemWithoutBalance;



            im.Unit_Used_For_S_price_Default = 1;
            im.Unit_Used_For_P_price_Default = 1;

            im.Unit_Used_for_Price = 1;

             

            


            um.Unit_Used_For_S_price_Default = 1;
            um.Unit_Used_For_P_price_Default = 1;

            um.active_flage = im.Active_flage;



            Common.SetItemPrices(ref im, ref um);


        }

        void EditItemServiceData(ref Items im, ref Unit_item um)
        {
            

             um.active_flage=im.Active_flage;
             
            im.S_price_LargeUnit = im.S_Price;
            im.S_price_SmallUnit = im.S_Price;

            im.P_Price = 0;
            im.P_priceAvgCost = 0;
            im.P_price_Item_No = 0;
            im.P_price_LargeUnit = im.P_Price;
            im.P_price_SmallUnit = im.P_Price;


            im.Upon_Request = 0;
            im.Width_ = 0;// txtWidth.Value;
            im.Height_ = 0;//txtHeight.Value;
            im.Somk = 0;// txtSomk.Value;
            im.Somk_Unit = "";// txtSomk_Unit.Text;

            im.Items_Types_Name_Id = 0;
            im.Items_TypesDescription_NameId = 0;
            im.Items_TypesDescriptionNameOtherId = 0;
            im.Items_TypesFactory_NameId = 0;
            im.Items_TypesSize_NameId = 0;
            im.Items_TypesColor_NameId = 0;
            im.Items_TypesDegreeFarz_NameId = 0;

             
 
            um.S_price_LargeUnit = im.S_Price;
            um.S_price_SmallUnit = im.S_Price;
             
            Common.SetItemPrices(ref im, ref um);


        }
        public void AddServiceOrEdit(ref int Excp, ref string  Msg, Items item)
        {
            try
            {
                ItemCLS itemCLS = new ItemCLS(ref opp);
                if (item.id == 0)
                {
                    Unit_item um = new Unit_item();
                    SetItemServiceData(ref item, ref um);
                   

                    itemCLS.AddService (ref Excp, ref Msg, item, um);
                }
                else
                {
                    Unit_item um = opp.db.Unit_item. Where(x => x.Item_Id == item.id && x.Store_id == item.S_id).FirstOrDefault();
                    EditItemServiceData(ref item, ref   um);
                    itemCLS.EditService (ref Excp, ref Msg, item , um);
                }

                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg=ee.Message; 
            }
        }
        public List<LookUp_ItemServiceCategory> LoadCategories()
        {
          return  opp.db.LookUp_ItemServiceCategory.ToList();
            
        }

        public Items GetServicesByIdNoTracking(long Id)
        { 
        ItemCLS itemCLS=new ItemCLS(ref opp);
            return  itemCLS.GetItemByIdNoTracking(Id);
        }
        public List<ServiceViewModel> GetAllServices()
        {
            return opp.db.Item
                .Where(x => x.Is_ItemOrService==2)
                .Select(x => new ServiceViewModel
                {
                    Id = x.id,
                    Name = x.Item_Name,
                    S_Price = x.S_Price??0,
                     ServiceDurationInMinutes = x.ServiceDurationInMinutes ?? 0,
                    IsCommissionEmployeeBased = x.IsCommissionEmployeeBased ?? 0,
                    IsCommissionEmployeeBased_True = x.IsCommissionEmployeeBased ?? 0,
                    CommissionEmployeeValue = x.CommissionEmployeeValue ?? 0,
                    ItemServiceCategory = x.ItemServiceCategory != null ? x.ItemServiceCategory.ServiceCategoryName : ""
                })
                .ToList();
        }

        public void DeleteService(long id)
        {
            var item = opp.db.Item.Find(id);
            if (item != null)
            {
                opp.db.Item.Remove(item);
                opp.db.SaveChanges();
            }
        }
    }

}
