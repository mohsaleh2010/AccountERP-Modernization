// AccountERP.Models/Menus/SysConfigMenus.cs
using AccountModelsLib.Models;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
namespace AccountModelsLib.Models
{

    public class DataDictionaryDbSyncLogsCLS
    {
        DataDictionaryDbSyncLogs dd=new DataDictionaryDbSyncLogs ();

        //public void AddNew(ref operations opp, DataDictionaryDbSyncLogs dd)
        //{
        //    AddNewOnly(  opp.db, dd);
        //}
        public void AddNewOnly(  My_DbContext  db, DataDictionaryDbSyncLogs dd)
        {
             db.DataDictionaryDbSyncLogs.Add(dd);
              //db.SaveChanges();
        }


        public DataDictionaryDbSyncLogs GetPendingDataDictionaryDbSyncLogs(My_DbContext db,string tableName, long recordId, string recordIdStr)
        {
            var eq = db.DataDictionaryDbSyncLogs.Where(d => d.TableName == tableName &&
                            d.Status == "Pending" &&
                            (d.RecordId == recordId || (d.RecordIdStr == recordIdStr && recordIdStr != null)))
               .FirstOrDefault();
            return eq;
        }
        public DataDictionaryDbSyncLogs GetPendingDataDictionaryDbSyncLogs(My_DbContext db, string tableName, long recordId )
        {
            var eq = db.DataDictionaryDbSyncLogs.Where(d => d.TableName == tableName &&
                            d.Status == "Pending" &&
                            (d.RecordId == recordId    ))
               .FirstOrDefault();
            return eq;
        }
    }
}