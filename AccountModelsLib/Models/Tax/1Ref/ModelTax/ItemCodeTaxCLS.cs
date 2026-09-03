using RestSharp;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
//using AccountModelsLib.ModelJson;
 

namespace AccountModelsLib.Models
{
    public class ItemCodeTaxCLS
    {
        operations opp = new operations();

        //My_DbContext db = new My_DbContext();
        public ItemCodeTaxCLS()
        {

        }
        public async Task<string> CreateEGSCodeUsage()
        {
            string Msg = "";
            List<ItemCodeToSendPortal> iList = new List<ItemCodeToSendPortal>();
            List<ItemCodeTaxNotUploaded> itemCodeTaxList = opp.db.ItemCodeTaxNotUploaded.Where(x => x.IsUploaded == false).ToList();
            bool IsMyRestResponse = false;


            var universalTime = DateTime.Now.ToUniversalTime();
            foreach (var itm in itemCodeTaxList)
            {
                ItemCodeToSendPortal item = new ItemCodeToSendPortal();

                item.codeType = itm.CodeType;
                item.parentCode = itm.ParentCode;
                item.itemCode = itm.ItemCode;
                item.codeName = itm.CodeName;
                item.codeNameAr = itm.CodeNameAr;

                item.activeFrom = universalTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
                item.activeTo = null;


                item.description = itm.description;
                item.descriptionAr = itm.descriptionAr;

                item.requestReason = itm.requestReason;
                item.linkedCode = null;

                iList.Add(item);
            }

            RestResponse respose = await CreateEGSCodeUsage(iList);

            if (respose.StatusCode == HttpStatusCode.Accepted)
            {
                IsMyRestResponse = true;
                Msg = "تم الارسال بنجاح";
            }
            else
            {
                Msg = respose.StatusCode.ToString();// respose.Content;
            }

            if (IsMyRestResponse == true)
            {
                UpdateSendCode(itemCodeTaxList);
            }
            return Msg;

        }
        public async Task<RestResponse> CreateEGSCodeUsage(List<ItemCodeToSendPortal> itemCodeToSendPortal)
        {
            string Msg = "";
            bool IsMyRestResponse = API.IsAllMyRestResponse;
            API aPI = new API();
            RestResponse respose = await aPI.CreateEGSCodeUsage(itemCodeToSendPortal);
            // IRestResponse respose = aPI.restResponse;

            return respose;

        }
        
         

       
        public async void UpdateSendCode(List<ItemCodeTaxNotUploaded> itemCodeTaxList)
        {
            try
            {
                itemCodeTaxList.ForEach(a => a.IsUploaded = true);
                //db.Entry(itemCodeTaxList).State = EntityState.Modified;
                //db.SaveChanges();

                foreach (var item in itemCodeTaxList)
                {
                    item.IsUploaded = true;
                    opp.db.Entry(item).State = EntityState.Modified;
                }
                opp.db.SaveChanges();
                //  await db.SaveChangesAsync();

            }
            catch (Exception ee)
            {


            }
        }

    }

}
