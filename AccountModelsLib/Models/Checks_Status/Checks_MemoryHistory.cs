using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
//using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient;

namespace AccountModelsLib.Models
{
    public class ChecksMemoryHistory
    {
        public long Id { get; set; }
        public long ChecksMemoryId { get; set; }
        public long AccountEntryListId { get; set; }
        public long SubAccountIdRelatedTo { get; set; }
        public string SubAccountNameRelatedTo { get; set; }
        public long ProcessNo { get; set; }
        public long SubAccountIdRelatedToBefore { get; set; }
        public string SubAccountNameRelatedToBefore { get; set; }

        public void Insert()
        {

            using (AccountModelsLib.operations opp = new AccountModelsLib.operations())
            {
                string mysql = "INSERT INTO Checks_MemoryHistory (Checks_MemoryId, Account_Entry_List_ID, SubAccount_Id_RelatedTo, SubAccount_Name_RelatedTo, ProcessNo, SubAccount_Id_RelatedToBefor, SubAccount_Name_RelatedToBefor) VALUES (@ChecksMemoryId, @AccountEntryListId, @SubAccountIdRelatedTo, @SubAccountNameRelatedTo, @ProcessNo, @SubAccountIdRelatedToBefore, @SubAccountNameRelatedToBefore)";

                opp.cmd.Parameters.AddWithValue("@ChecksMemoryId", ChecksMemoryId);
                opp.cmd.Parameters.AddWithValue("@AccountEntryListId", AccountEntryListId);
                opp.cmd.Parameters.AddWithValue("@SubAccountIdRelatedTo", SubAccountIdRelatedTo);
                opp.cmd.Parameters.AddWithValue("@SubAccountNameRelatedTo", SubAccountNameRelatedTo);
                opp.cmd.Parameters.AddWithValue("@ProcessNo", ProcessNo);
                opp.cmd.Parameters.AddWithValue("@SubAccountIdRelatedToBefore", SubAccountIdRelatedToBefore);
                opp.cmd.Parameters.AddWithValue("@SubAccountNameRelatedToBefore", SubAccountNameRelatedToBefore);

                opp.insert_me(mysql);

            }
        }

        public void Update()
        {
            using (AccountModelsLib.operations opp = new AccountModelsLib.operations())
            {
                string mysql = "UPDATE Checks_MemoryHistory SET Checks_MemoryId = @ChecksMemoryId, Account_Entry_List_ID = @AccountEntryListId, SubAccount_Id_RelatedTo = @SubAccountIdRelatedTo, SubAccount_Name_RelatedTo = @SubAccountNameRelatedTo, ProcessNo = @ProcessNo, SubAccount_Id_RelatedToBefor = @SubAccountIdRelatedToBefore, SubAccount_Name_RelatedToBefor = @SubAccountNameRelatedToBefore WHERE Id = @Id";

                opp.cmd.Parameters.AddWithValue("@Id", Id);
                opp.cmd.Parameters.AddWithValue("@ChecksMemoryId", ChecksMemoryId);
                opp.cmd.Parameters.AddWithValue("@AccountEntryListId", AccountEntryListId);
                opp.cmd.Parameters.AddWithValue("@SubAccountIdRelatedTo", SubAccountIdRelatedTo);
                opp.cmd.Parameters.AddWithValue("@SubAccountNameRelatedTo", SubAccountNameRelatedTo);
                opp.cmd.Parameters.AddWithValue("@ProcessNo", ProcessNo);
                opp.cmd.Parameters.AddWithValue("@SubAccountIdRelatedToBefore", SubAccountIdRelatedToBefore);
                opp.cmd.Parameters.AddWithValue("@SubAccountNameRelatedToBefore", SubAccountNameRelatedToBefore);

                opp.insert_me(mysql);
            }
        }

        public void Delete()
        {
            using (AccountModelsLib.operations opp = new AccountModelsLib.operations())
            {
                string mysql = "DELETE FROM Checks_MemoryHistory WHERE Id = @Id";
                opp.cmd.Parameters.AddWithValue("@Id", Id);
                opp.insert_me(mysql);

            }
        }
        public static List<ChecksMemoryHistory> GetAll()
        {
            List<ChecksMemoryHistory> checksMemoryHistories = new List<ChecksMemoryHistory>();

            using (AccountModelsLib.operations opp = new AccountModelsLib.operations())
            {
                string mysql = "SELECT * FROM Checks_MemoryHistory";

                using (SqlDataReader reader = opp.ReturnDataReader(mysql))
                {
                    while (reader.Read())
                    {
                        ChecksMemoryHistory checksMemoryHistory = new ChecksMemoryHistory();
                        checksMemoryHistory.Id = (long)reader["Id"];
                        checksMemoryHistory.ChecksMemoryId = (long)reader["Checks_MemoryId"];
                        checksMemoryHistory.AccountEntryListId = (long)reader["Account_Entry_List_ID"];
                        checksMemoryHistory.SubAccountIdRelatedTo = (long)reader["SubAccount_Id_RelatedTo"];
                        checksMemoryHistory.SubAccountNameRelatedTo = (string)reader["SubAccount_Name_RelatedTo"];
                        checksMemoryHistory.ProcessNo = (long)reader["ProcessNo"];
                        checksMemoryHistory.SubAccountIdRelatedToBefore = (long)reader["SubAccount_Id_RelatedToBefor"];
                        checksMemoryHistory.SubAccountNameRelatedToBefore = (string)reader["SubAccount_Name_RelatedToBefor"];
                        checksMemoryHistories.Add(checksMemoryHistory);
                    }
                }
            }
            return checksMemoryHistories;
        }

        public static ChecksMemoryHistory GetById(long id)
        {
            ChecksMemoryHistory checksMemoryHistory = null;

            using (AccountModelsLib.operations opp = new AccountModelsLib.operations())
            {
               string mysql = "SELECT * FROM Checks_MemoryHistory WHERE Id = @Id";
                    opp.cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = opp.ReturnDataReader(mysql))
                    {
                        if (reader.Read())
                        {
                            checksMemoryHistory = new ChecksMemoryHistory();
                            checksMemoryHistory.Id = (long)reader["Id"];
                            checksMemoryHistory.ChecksMemoryId = (long)reader["Checks_MemoryId"];
                            checksMemoryHistory.AccountEntryListId = (long)reader["Account_Entry_List_ID"];
                            checksMemoryHistory.SubAccountIdRelatedTo = (long)reader["SubAccount_Id_RelatedTo"];
                            checksMemoryHistory.SubAccountNameRelatedTo = (string)reader["SubAccount_Name_RelatedTo"];
                            checksMemoryHistory.ProcessNo = (long)reader["ProcessNo"];
                            checksMemoryHistory.SubAccountIdRelatedToBefore = (long)reader["SubAccount_Id_RelatedToBefor"];
                            checksMemoryHistory.SubAccountNameRelatedToBefore = (string)reader["SubAccount_Name_RelatedToBefor"];
                        }
                    }
                 
            }
            return checksMemoryHistory;
        }
    }
}
