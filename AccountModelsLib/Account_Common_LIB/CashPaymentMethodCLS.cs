using System;
using System.Data;
using AccountModelsLib.Models;
using Microsoft.Data.SqlClient;

namespace AccountModelsLib
{
    public class CashPaymentMethodCLS
    {
        private DataTable dataTable = null;
        operations opp = new operations();
        public DataTable GetData()
        {
             
                string mysql = "SELECT * FROM Lookup_CashPaymentMethod";
                dataTable = opp.return_DataTableWithBuilder(mysql);

                // Listen for newly added rows from the DataGridView
                dataTable.TableNewRow -= DataTable_TableNewRow;
                dataTable.TableNewRow += DataTable_TableNewRow;

                return dataTable;
             
        }

        private void DataTable_TableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            // Assign a unique Snowflake ID to the primary key column for every new row
            // Replace 'SnowflakeIdGenerate.GenerateId()' with your actual generator method name
            e.Row["Id"] = CallSnowflakeIdGenerate.GenerateNewId();
        }

        public void UpdateDataTable()
        {
            //using (var opp = new operations())
            //{
            //    // Ensures all pending edits in the DataGridView are committed to the DataTable
            //    opp.ada.Update(dataTable);
            //}
            if (opp.ada != null && dataTable != null)
            {
                // توليد أوامر الـ INSERT/UPDATE/DELETE تلقائياً بناءً على opp.ada.SelectCommand
                SqlCommandBuilder builder = new SqlCommandBuilder(opp.ada);

                // حفظ التغييرات مباشرة في قاعدة البيانات
                opp.ada.Update(dataTable);
            }
        }

        public void DeleteNature_Items(ref int Excp, ref string Msg, long id)
        {
            try
            {
                 
                    // Fixed table name matching your SQL table 'Lookup_CashPaymentMethod'
                    string mysql = "DELETE FROM Lookup_CashPaymentMethod WHERE Id = " + id;
                    opp.insert_me(mysql);
                    Excp = 0;
                 
            }
            catch (Exception ex)
            {
                Excp = 1;
                Msg = ex.Message;
            }
        }
    }
}