//using AccountModelsLib.DBModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 

namespace AccountModelsLib.Models
{


    public class ItemCodeTaxAcceptedCLS
    {
        operations opp;
        public ItemCodeTaxAccepted itmProduct = new ItemCodeTaxAccepted();
        public List<ItemCodeTaxAccepted> itmProductList = new List<ItemCodeTaxAccepted>();
        public ItemCodeTaxAcceptedCLS()
        {
            opp = new operations();
            //activeFrom = new DateTime();
            //codeName = "";
        }
        public ItemCodeTaxAcceptedCLS(ref operations opp_)
        {
            opp = opp_;

        }

        public List<ItemCodeTaxAccepted> GetItemCodeTaxAccepted()
        {
            itmProductList = opp.db.ItemCodeTaxAccepted.AsNoTracking().ToList();
            return itmProductList;
        }
        public void GetItemCodeTaxAcceptedById(long Id)
        {
            itmProduct = opp.db.ItemCodeTaxAccepted.Where(i => i.Id == Id).FirstOrDefault();

        }
        public List<ItemCodeTaxAccepted> GetItemCodeTaxAcceptedByRepeated(int Is_Repeated)
        {

            itmProductList = opp.db.ItemCodeTaxAccepted.Where(i => i.Is_Repeated == Is_Repeated & i.Is_SendToERP==0).ToList();
            return itmProductList;
        }
        public ItemCodeTaxAccepted GetItemCodeTaxAcceptedBy_CodeName(string CodeName)
        {
            return opp.db.ItemCodeTaxAccepted.Where(i => i.CodeName == CodeName).FirstOrDefault();

        }
        public void SaveItemCodeTaxAccepted(ref int Excp, ref string Msg)
        {
            //MyDbContext.ConnStr = operations.conn_str_;
            //MyDbContext db = new MyDbContext();
            //operations opp = new operations();
            try
            {
                if (itmProduct.Id == 0)
                {
                    opp.db.ItemCodeTaxAccepted.Add(itmProduct);
                }
                else
                {
                    opp.db.Entry(itmProduct).State = EntityState.Modified;
                }
                opp.db.SaveChanges();
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }
        }
        public void DeleteItemCodeTaxAccepted(ref int Excp, ref string Msg, ItemCodeTaxAccepted itemCodeTax)
        {
            //MyDbContext.ConnStr = operations.conn_str_;
            //MyDbContext db = new MyDbContext();
            //operations opp = new operations();
            try
            {
                if (itemCodeTax.Id == 0)
                {
                    opp.db.ItemCodeTaxAccepted.Remove(itmProduct);
                }

                opp.db.SaveChanges();
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }
        }

        public async Task<bool> SaveItemFromTaxPortal(JArray jasonarr, string pageNo)
        {
            List<ItemCodeTaxAccepted> itmList = new List<ItemCodeTaxAccepted>();
            int ps_in_pageNo = 1;
            foreach (JObject item in jasonarr)
            {
                int Is_Repeated = 0;
                ItemCodeTaxAccepted itmS = new ItemCodeTaxAccepted();
                string CodeName = item.SelectToken("codeNamePrimaryLang").ToString();
                itmS = GetItemCodeTaxAcceptedBy_CodeName(CodeName);
                if (itmS != null)
                {
                    //continue;
                    Is_Repeated = 1;
                }


                //else
                //{
                ItemCodeTaxAccepted itm = new ItemCodeTaxAccepted();
                itm.Is_Repeated = Is_Repeated;
                itm.Barcode = "";
                itm.ParentCode = item.SelectToken("parentCodeID").ToString();  // "";
                itm.ParentCodeLookupValue = item.SelectToken("parentCodeLookupValue").ToString();  // "";

                itm.ItemCode = item.SelectToken("codeLookupValue").ToString();
                itm.ItemCodeType = item.SelectToken("codeTypeNamePrimaryLang").ToString();// "EGS";
                itm.internalCode = item.SelectToken("codeID").ToString();
                itm.CodeName = item.SelectToken("codeNamePrimaryLang").ToString();
                itm.CodeNameAr = item.SelectToken("codeNameSecondaryLang").ToString();
                itm.DescriptionAr = item.SelectToken("codeDescriptionSecondaryLang").ToString();
                itm.DescriptionEn = item.SelectToken("codeDescriptionPrimaryLang").ToString();
                try
                {
                    itm.activeFrom = DateTime.Parse(item.SelectToken("activeFrom").ToString());
                }
                catch (Exception ee)
                {
                    itm.activeFrom = null;
                }

                try
                {
                    itm.activeTo = DateTime.Parse(item.SelectToken("activeTo").ToString());
                }
                catch (Exception ee)
                {
                    itm.activeTo = null;
                }
                itm.codeTypeID = item.SelectToken("codeTypeID").ToString();
                itm.codeTypeLevelID = item.SelectToken("codeTypeLevelID").ToString();
                itm.codeTypeLevelNamePrimaryLang = item.SelectToken("codeTypeLevelNamePrimaryLang").ToString();
                itm.codeTypeLevelNameSecondaryLang = item.SelectToken("codeTypeLevelNameSecondaryLang").ToString();
                itm.parentCodeNamePrimaryLang = item.SelectToken("parentCodeNamePrimaryLang").ToString();
                itm.parentCodeNameSecondaryLang = item.SelectToken("parentCodeNameSecondaryLang").ToString();
                itm.parentLevelName = item.SelectToken("parentLevelName").ToString();
                itm.codeTypeNameSecondaryLang = item.SelectToken("codeTypeNameSecondaryLang").ToString();
                itm.active = item.SelectToken("active").ToString();



                itm.linkedCode = item.SelectToken("linkedCode").ToString();
                itm.codeCategorization = item.SelectToken("codeCategorization").ToString();

                itm.CurrencyId = 1;
                itm.Price = 0;
                itm.UnitCode = "";// item.SelectToken("codeTypeName").ToString();
                itm.UnitDescEn = "";// "each (ST) ( ST )";
                itm.UnitDescAr = "";
                itm.Tax_BranchesId = Common.CommonTaxConfig.Tax_BranchesId;
                try
                {
                    itm.pageNo = int.Parse(pageNo.ToString()); ;
                }
                catch (Exception)
                {
                    itm.pageNo = 0;
                }
                try
                {
                    itm.ps_in_pageNo = int.Parse(ps_in_pageNo.ToString());
                }
                catch (Exception)
                {
                    itm.ps_in_pageNo = 0;
                }

                ps_in_pageNo = ps_in_pageNo + 1;
                itmList.Add(itm);



                //}


            }
            if (itmList.Count > 0)
            {
                opp.db.ItemCodeTaxAccepted.AddRange(itmList);
                opp.db.SaveChanges();
            }

            bool IsSaved = false;// await  SaveItemCodeTaxAccepted(  itmList);
            return IsSaved;
            //if (itmList.Count > 0)
            //{
            //    opp.db.ItemCodeTaxAccepted.AddRange(itmList);
            //    opp.db.SaveChanges();
            //}
        }

        private async Task<bool> SaveItemCodeTaxAccepted(List<ItemCodeTaxAccepted> itmList)
        {
            bool IsSaved = false;
            try
            {
                if (itmList.Count > 0)
                {
                    opp.db.ItemCodeTaxAccepted.AddRange(itmList);
                    opp.db.SaveChanges();
                    IsSaved = true;
                }
                return IsSaved;
            }
            catch (Exception ee)
            {
                IsSaved = false;
                return IsSaved;
            }
        }
       

        void SaveItemsFromAcceptedToERP(ref int Excp, ref string Msg, ItemCodeTaxAccepted itmAccepted, Items item, Unit_item um)
        {
            ItemCLS imcls = new ItemCLS(ref opp);
            imcls.itcCls.itemCodeTaxNotUploaded = null;
            string ItemCode = itmAccepted.ItemCode;
            if (itmAccepted.Is_RepeatedExcluded == 0 | itmAccepted.Is_RepeatedExcluded == null)
            {
                Items itmSearch = imcls.GetItemByItemCode(ItemCode);
                if (itmSearch != null)
                {
                    long Item_Id = itmSearch.id;
                    itmAccepted.Item_Id = Item_Id;

                    opp.db.Entry(itmAccepted).State = EntityState.Modified;
                    opp.db.SaveChanges();
                    return;
                }
            }

            opp.BeginTransaction(2);
            try
            {
                imcls.Save_Item(ref Excp, ref Msg, ref opp, item, um);
                if (Excp == 1)
                {


                }
                if (Excp == 0)
                {
                    long Item_Id = item.id;
                    itmAccepted.Item_Id = Item_Id;
                    itmAccepted.Is_SendToERP = 1;

                    opp.db.Entry(itmAccepted).State = EntityState.Modified;
                    opp.db.SaveChanges();
                }

                opp.Commit();
                return;
            }
            catch (Exception ee)
            {
                opp.Rollback();
                return;
            }
        }
    }
}
