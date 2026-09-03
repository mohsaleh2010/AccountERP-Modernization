using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.Models
{
    public class DataDictionaryLogerProcessCLS
    {
        operations opp;
        public DataDictionaryLogerProcessCLS()
        {
            opp = new operations();
        }
        public DataDictionaryLogerProcessCLS(ref operations opp_)
        {
            opp = opp_;
        }
        public void AddNew(DataDictionaryLogerProcess dl)
        {
            try
            {

                opp.db.DataDictionaryLogerProcesses.Add(dl);
                opp.db.SaveChanges();
            }
            catch (Exception eee)
            {
            }

        }
        public void AddNew(long ProcessId, string ProcessNotes)
        {
            try
            {
                DataDictionaryLogerProcess dl = new DataDictionaryLogerProcess();
                dl.User_Id = Common.User_Id;
                dl.UserName = Common.User_Name;
                dl.Branch_Id = Common.Branch_Id;
                dl.ProcessId = ProcessId;
                dl.ProcessName = GetProcessName(dl.ProcessId);
                dl.SubProcessId = 0;
                dl.SubProcessName = "";

                dl.ProcessTableId = 0;
                dl.ProcessDocNo = 0;
                dl.ProcessDateTime = DateTime.Now;

                dl.Item_Name = "";
                dl.Qnty = "";

                dl.ProcessNotes = ProcessNotes;

                opp.db.DataDictionaryLogerProcesses.Add(dl);
                opp.db.SaveChanges();
            }
            catch (Exception eee)
            {
            }

        }

        public void AddNew(long SubProcessId, Order_Purshing order_Purshing, string ProcessNotes)
        {
            try
            {
                DataDictionaryLogerProcess dl = new DataDictionaryLogerProcess();
                dl.User_Id = Common.User_Id;
                dl.UserName = Common.User_Name;
                dl.Branch_Id = Common.Branch_Id;
                dl.ProcessId = order_Purshing.Order_Type ?? 0;
                dl.ProcessName = GetProcessName(dl.ProcessId);
                dl.SubProcessId = SubProcessId;
                dl.SubProcessName = GetSubProcessName(SubProcessId);

                dl.ProcessTableId = order_Purshing.Id;
                dl.ProcessDocNo = order_Purshing.Order_Purshing_No;
                dl.ProcessDateTime = DateTime.Now;

                dl.Item_Name = "";
                dl.Qnty = "";

                dl.ProcessNotes = ProcessNotes;

                opp.db.DataDictionaryLogerProcesses.Add(dl);
                opp.db.SaveChanges();
            }
            catch (Exception eee)
            {
            }

        }


        void AddNew(long ProcessId, long SubProcessId, long ProcessTableId, long ProcessDocNo, long SubProcessTableId, long Item_Id, long Unit_Item_Id, string Item_Name, string Qnty, string ProcessNotes)
        {
            try
            {
                DataDictionaryLogerProcess dl = new DataDictionaryLogerProcess();
                dl.User_Id = Common.User_Id;
                dl.UserName = Common.User_Name;
                dl.Branch_Id = Common.Branch_Id;
                dl.ProcessId = ProcessId;
                dl.ProcessName = GetProcessName(ProcessId);
                dl.SubProcessId = SubProcessId;
                dl.SubProcessName = GetSubProcessName(SubProcessId);

                dl.ProcessTableId = ProcessTableId;
                dl.ProcessDocNo = ProcessDocNo;
                dl.ProcessDateTime = DateTime.Now;

                dl.Item_Name = Item_Name;
                dl.Qnty = Qnty;

                dl.ProcessNotes = ProcessNotes;

                opp.db.DataDictionaryLogerProcesses.Add(dl);
                opp.db.SaveChanges();
            }
            catch (Exception eee)
            {
            }

        }
        string GetSubProcessName(long SubProcessId)
        {
            string SubProcessName = "";
            if (SubProcessId == 1)
            {
                SubProcessName = "إضافة";
            }
            if (SubProcessId == 2)
            {
                SubProcessName = "تعديل";
            }
            if (SubProcessId == 3)
            {
                SubProcessName = "حذف";
            }
            if (SubProcessId == 4)
            {
                SubProcessName = "بحث - عرض";
            }
            if (SubProcessId == 5)
            {
                SubProcessName = "طباعة";
            }
            if (SubProcessId == 6)
            {
                SubProcessName = "إضافة صنف";
            }
            return SubProcessName;
        }
        string GetProcessName(long ProcessId)
        {
            string ProcessName = "";
            DataDictionaryProcessesList dLst = opp.db.DataDictionaryProcessesLists.AsNoTracking().Where(i => i.Id == ProcessId).FirstOrDefault();
            if (dLst != null)
            {
                ProcessName = dLst.ProcessName;
            }
            return ProcessName;
        }

        public void AddNew(long SubProcessId, Order_Purshing order_Purshing, Detailes_Purshing dp)
        {
            //ddlpCLS.AddNew(order_Purshing.Order_Type.Value, 6, order_Purshing.Id, order_Purshing.Order_Purshing_No, dp_New.Id, dp_New.Item_Name);
            long ProcessId = order_Purshing.Order_Type ?? 0;
            long ProcessTableId = order_Purshing.Id;
            long ProcessDocNo = order_Purshing.Order_Purshing_No;
            long SubProcessTableId = dp.Id;
            long Item_Id = dp.Item_Id;
            long Unit_Item_Id = dp.Unit_item_Table_Id;
            string Item_Name = dp.Item_Name;
            string Qnty = dp.Qnty.ToString();
            AddNew(ProcessId, SubProcessId, ProcessTableId, ProcessDocNo, SubProcessTableId, Item_Id, Unit_Item_Id, Item_Name, Qnty, "");
        }
    }
}
