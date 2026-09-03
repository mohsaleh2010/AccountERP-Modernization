using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Connection4It;  using Microsoft.EntityFrameworkCore; using AccountModelsLib;using Microsoft.EntityFrameworkCore; using AccountModelsLib;
namespace Connection4It
{
    public class ShipmentCompanyCLS
    {
        public ShipmentCompanyCLS()
        {
        }
        private DataTable dataTable = null;
        AccountModelsLib.operations opp = new AccountModelsLib.operations();
        public DataTable GetData()
        {
           
            string mysql = "Select * from ShipmentCompany";
            dataTable = opp.return_DataTableWithBuilder(mysql);

            return dataTable;

        }

        public void  UpdateDataTable()
        {
            opp.ada.Update(dataTable);
             

        }

    }
}
