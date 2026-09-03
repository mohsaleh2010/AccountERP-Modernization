using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{

    public class Tax_BranchCLS
    {
        operations opp = new operations();
        public Tax_Branch tb = new Tax_Branch();
        public List<Tax_Branch> tbList = new List<Tax_Branch>();
        public Tax_BranchConfig tbConfig = new Tax_BranchConfig();
        public Tax_BranchCLS()
        {
            tb = new Tax_Branch();
        }

        public List<Tax_Branch> GetTax_Branches()
        {
            try
            {
                tbList = opp.db.Tax_Branches.ToList();


            }
            catch (Exception ee)
            {

            }
            return tbList;
        }
        public  Tax_Branch  GetTax_BranchById(long Id)
        {
            Tax_Branch tb  = opp.db.Tax_Branches.FirstOrDefault();

 
            return tb;
        }

        public void GetAllBranchData(long id)
        {
            tb = opp.db.Tax_Branches.Where(i => i.Id == id).FirstOrDefault();
            if (tb != null)
            {
                tbConfig = opp.db.Tax_BranchesConfig.Where(i => i.Tax_BranchesId == id).FirstOrDefault();

            }

        }
        public void GetAllBranchDataForConfig(long id)
        {
            try
            {
                tb = opp.db.Tax_Branches.AsNoTracking().Where(i => i.Id == id).FirstOrDefault();
                if (tb != null)
                {
                    tbConfig = opp.db.Tax_BranchesConfig.AsNoTracking().Where(i => i.Tax_BranchesId == id).FirstOrDefault();

                }
            }
            catch (Exception ee)
            {

                
            }

        }
        public List<Tax_BranchVM> GetTax_BranchVMList11()
        {
            if (opp == null)
            {
                opp = new operations();
            }
            var tb1 = opp.db.Tax_Branches.AsNoTracking();


            List<Tax_BranchVM> taxVMList = new List<Tax_BranchVM>();

            foreach (var item in tb1)
            {
                tbConfig = opp.db.Tax_BranchesConfig.AsNoTracking().Where(i => i.Tax_BranchesId == item.Id).FirstOrDefault();


                Tax_BranchVM taxVM = new Tax_BranchVM();
                taxVM.Id = item.Id;
                taxVM.Name = item.Name;
                taxVM.BranchID = item.BranchID;
                taxVM.ActivityCode = tbConfig.ActivityCode;
                taxVMList.Add(taxVM);
            }
            return taxVMList;
        }
        public List<Tax_BranchVM> GetTax_BranchVMList()
        {
            //List<Tax_Branch> Tax_BranchList = new List<Tax_Branch>();

            var taxBranchList = opp.db.Tax_Branches.Include(a => a.Tax_BranchConfig).AsNoTracking();
            List<Tax_BranchVM> taxVMList = new List<Tax_BranchVM>();

            foreach (var item in taxBranchList)
            {
                Tax_BranchVM taxVM = new Tax_BranchVM();
                taxVM.Id = item.Id;
                taxVM.Name = item.Name;
                taxVM.BranchID = item.BranchID;
                taxVM.ActivityCode = item.Tax_BranchConfig.ActivityCode;
                taxVMList.Add(taxVM);
            }
            return taxVMList;
        }
        void SetDefaultCompany(Tax_Branch tb)
        {
            tb.IsDefault = tb.IsDefault ?? 0;
            if (tb.IsDefault == 1)
            {
                string mysql = "Update Tax_Branches Set IsDefault=0";
                opp.insert_me(mysql);

                mysql = "Update Tax_Branches Set IsDefault=1 Where Id=" + tb.Id;
                opp.insert_me(mysql);
            }
        }
        public void EditBranch(ref int Excp, ref string Msg)
        {
            try
            {
                opp.BeginTransaction(2);
                opp.db.Entry(tb).State = EntityState.Modified;
                opp.db.SaveChanges();

                opp.db.Entry(tbConfig).State = EntityState.Modified;
                opp.db.SaveChanges();

                SetDefaultCompany(tb);
                Excp = 0;
                Msg = "";
                opp.Commit();
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
                opp.Rollback();
            }

        }

        public void AddNewBranch(ref int Excp, ref string Msg, Tax_Branch tbNew, Tax_BranchConfig tbConfigNew)
        {
            try
            {
                opp.BeginTransaction();
                opp.db.Tax_Branches.Add(tbNew);
                opp.db.SaveChanges();

                tbConfigNew.Tax_BranchesId = tbNew.Id;
                tbConfigNew.Account_BranchesId = tbNew.Account_BranchesId;

                opp.db.Tax_BranchesConfig.Add(tbConfigNew);
                opp.db.SaveChanges();

                SetDefaultCompany(tbNew);
                Excp = 0;
                Msg = "";
                opp.Commit();
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
                opp.Rollback();
            }
        }

        public void DeleteBranch(ref int Excp, ref string Msg, long Id, Tax_Branch tbToDelete)
        {
            try
            {
                opp.BeginTransaction();

                CkechIsDefaultWhenDelete(tbToDelete);

                opp.db.Tax_Branches.Remove(tbToDelete);
                opp.db.SaveChanges();

                Tax_BranchConfig tbConfig = opp.db.Tax_BranchesConfig.Where(i => i.Tax_BranchesId == Id).FirstOrDefault();
                if (tbConfig != null)
                {
                    opp.db.Tax_BranchesConfig.Remove(tbConfig);
                    opp.db.SaveChanges();
                }


                Excp = 0;
                Msg = "";
                opp.Commit();
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
                opp.Rollback();
            }
        }
        void CkechIsDefaultWhenDelete(Tax_Branch tbToDelete)
        {
            if (tb.IsDefault == 1)
            {
                List<Tax_Branch> tbL = new List<Tax_Branch>();
                tbL = opp.db.Tax_Branches.Where(i => i.Id != tbToDelete.Id).ToList();
                if (tbL.Count > 0)
                {
                    foreach (var item in tbL)
                    {
                        if (item.IsDefault == 1)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    Tax_Branch tb1 = new Tax_Branch();
                    tb1 = tbL[0];
                    tb1.IsDefault = 1;

                    opp.db.Entry(tb1).State = EntityState.Modified;
                    opp.db.SaveChanges();
                }
            }
        }
    }
}
