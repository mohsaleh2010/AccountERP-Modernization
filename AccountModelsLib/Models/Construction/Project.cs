using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
namespace AccountModelsLib.Models
{
    public class Project
    {
         
        public long Id { get; set; } = 0;
        public string Name { get; set; }
        public string Address { get; set; }
        public DataTable Get_Projects()
        {
            operations opp = new operations();
            return Get_Projects(ref opp);
        }
        public DataTable Get_Projects(ref operations opp, long Id =0)
        {
            string mysql = " Select * from ConsProject  ";
            if(Id>0)
            {
                mysql = mysql + " Where Id=" + Id;
            }
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public void GetProjectById(ref operations opp, long Id)
        {

            DataTable dt = Get_Projects(ref opp, Id);
            if (dt.Rows.Count > 0)
            {
                ReadData(dt, 0);
            }

        }
        public void ReadData(DataTable dt, int index)
        {
            Id = int.Parse(dt.Rows[index]["Id"].ToString());
            Name = dt.Rows[index]["Name"].ToString() ;
            Address = dt.Rows[index]["Address"].ToString();

        }
        public DataTable Get_ProjectsByName(ref operations opp)
        {
            string mysql = " Select * from ConsProject Where  Name='" + Name + "'  ";
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public void InsertProjects(ref operations opp)
        {

            string mysql = @"INSERT INTO ConsProject (Name ,Address ) ";
            mysql = mysql + " VALUES ('" + Name + "', '" + Address + "'  )";

            opp.insert_me(mysql);

        }
    }
}
