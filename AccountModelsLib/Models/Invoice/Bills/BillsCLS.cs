using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    public class BillsCLS
    {
        AccountModelsLib.operations opp;
        public unit un = new unit();
        public BillsCLS()
        {
            opp = new AccountModelsLib.operations();
        }
        public BillsCLS(ref AccountModelsLib.operations opp_)
        {
            opp = opp_;
        }

        public List<Bills_Details> GetBills_Details(long Bills_ListId)
        {
            List<Bills_Details> Bills_DetailsList = new List<Bills_Details>();
            Bills_DetailsList = opp.db.Bills_Details.Where(b => b.Invoice_Id == Bills_ListId).ToList();
            return Bills_DetailsList;
        }

        void UpdatePrice(ref int excp, ref string msg, Bills_Details umVM)
        {
            umVM.Edit_User_Id = Common.User_Id;
            umVM.EditOperationDate = DateTime.Now;
            opp.db.Entry(umVM).State = EntityState.Modified;
            opp.db.SaveChanges();

            ///////
            ///
            ItemCLS itmcls = new ItemCLS(ref opp);
            itmcls.UpdateS_Price(ref excp, ref msg, umVM);
        }
        public void UpdatePriceOne(ref int excp, ref string msg, Bills_Details umVM)
        {
            try
            {
                excp = 0;
                opp.BeginTransaction();

                UpdatePrice(ref excp, ref msg, umVM);
                if (excp == 0)
                {
                    opp.Commit();
                }
                else
                {
                    opp.Rollback();
                }
            }
            catch (Exception ee)
            {
                opp.Rollback();
                excp = 1;

            }
        }
        public void UpdatePriceAll(ref int excp, ref string msg, List<Bills_Details> umVMList)
        {
            try
            {
                excp = 0;
                opp.BeginTransaction();
                foreach (var umVM in umVMList)
                {
                     
                    UpdatePrice(ref excp, ref msg, (Bills_Details)umVM);
                    if(excp==1)
                    {
                        break;
                    }
                }
               
                if (excp == 0)
                {
                    opp.Commit();
                }
                else
                {
                    opp.Rollback();
                }
            }
            catch (Exception ee)
            {
                opp.Rollback();
                excp = 1;

            }
        }
    }
}
