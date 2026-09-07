using System;
using System.IO;
using System.Data;
using System.Xml;

namespace AccountCommonDataLIB
{
	/// <summary>
	/// 
	/// </summary>
	public class ConnConfigInfoXML
	{
		//Class Members
		private DataSet dsSource=new DataSet();	//Stores XML dataset
		private DataTable dtInformation;	//Stores the table in the XML dataset
		private DataTable dtConfig;		//Stores the table containing Configuration
		private DataTable dtDateFormat;	// Date formate string
		private DataTable dtCalender;	// Calender settings
	
		private string sFileName;	//Stores XML file name
		public string m_sFileName
		{
			get
			{
				return sFileName;
			}
			set
			{
				sFileName = value;
			}
		}

		public ConnConfigInfoXML(ref long ReturnExcep)
		{			
			string xmlFilePath = "\\" + "ConnInfo.xml";
			string _exePath = System.Windows.Forms.Application.ExecutablePath;
			string _exeDirectory = Path.GetDirectoryName(_exePath);
			
			xmlFilePath = _exeDirectory +  "\\CommonFiles" + xmlFilePath;
			m_sFileName = xmlFilePath;
			
			if(!File.Exists(m_sFileName))
				ReturnExcep = -1;
			else
			{
				ReturnExcep = 1;
				dsSource = new DataSet(); 
				dtInformation = new DataTable();	//Stores the table in the XML dataset
				dtConfig = new DataTable();	
				dtDateFormat = new DataTable();
				dtCalender = new DataTable();
			}
		}
		
		public ConnConfigInfoXML()
		{			
			//m_sFileName = @"..\..\ConnInfo.xml";
			//m_sFileName = Directory.GetCurrentDirectory() + "\\ConnInfo.xml";
			string xmlFilePath = "\\" + "ConnInfo.xml";
			string _exePath = System.Windows.Forms.Application.ExecutablePath;
			string _exeDirectory = Path.GetDirectoryName(_exePath);
			
			xmlFilePath = _exeDirectory + "\\CommonFiles" + xmlFilePath;
			m_sFileName = xmlFilePath;
			dsSource = new DataSet(); 
			dtInformation = new DataTable();	//Stores the table in the XML dataset
			dtConfig = new DataTable();	
			dtDateFormat = new DataTable();
			dtCalender = new DataTable();
		}

		//Class Methods

		/// <summary>
		/// This method is responsible for reading the XML schema and initializes 
		/// the Info and Config tables in the XML file, it doesn't take parameters
		/// </summary>
		/// <returns> A data set containing both tables</returns>
		public DataSet dsReadXML()	
		{
			dsSource.Clear();
			//dsSource.ReadXmlSchema(@"..\..\Information.xsd");
			//dsSource.ReadXmlSchema(Directory.GetCurrentDirectory() + "\\Information.xsd");
			try
			{
				string xmlFilePath = "\\" + "ConnInfo.xml";
				string _exePath = System.Windows.Forms.Application.ExecutablePath;
				string _exeDirectory = Path.GetDirectoryName(_exePath);
				 
				xmlFilePath = _exeDirectory + "\\CommonFiles" + xmlFilePath;
				dsSource.ReadXmlSchema(xmlFilePath);
				//			dsSource.Tables["Info"].PrimaryKey = new DataColumn[] {dsSource.Tables["Info"].Columns["ID"]};
				dsSource.Tables["Config"].PrimaryKey = new DataColumn[] { dsSource.Tables["Config"].Columns["ID"] };
				dsSource.Tables["DateFormat"].PrimaryKey = new DataColumn[] { dsSource.Tables["DateFormat"].Columns["ID"] };
				dsSource.ReadXml(m_sFileName, XmlReadMode.IgnoreSchema);
			}
			catch (Exception ee)
			{
				  
			}	
			return dsSource;
		}
      

        /// <summary>
        /// This method is responsible for reading the XML schema and initializes 
        /// the Info and Config tables in the XML file, it doesn't take parameters and don't return values
        /// </summary>
        

		/*
		/// <summary>
		/// This methods is responsible for updating the XML file by the given 
		/// values in the parameters for a specfic table
		/// </summary>
		/// <param name="dtNewValues">A data table containing the new or updated values</param>
		/// <param name="sTable"> A string specifying which table to be updated</param>
		public void vUpdateXML(DataTable dtNewValues, string sTable)	
		{
			dsSource.AcceptChanges();
			if (sTable == "Info")
				foreach (DataRow drRow in dtNewValues.Rows)						 
					dtInformation.LoadDataRow(drRow.ItemArray , true);
			else
				foreach (DataRow drRow in dtNewValues.Rows)						 
					dtConfig.LoadDataRow(drRow.ItemArray , true);

			dsSource.WriteXml(m_sFileName);
		}
		*/
		public void vUpdateXML()	
		{
			dsSource.AcceptChanges();
			 
			dsSource.WriteXml(m_sFileName);
		}
		/// <summary>
		/// This method is resposible for searching by the given parameters
		/// sID is the wanted ID of the record, sTable is the table name to search in
		/// </summary>
		/// <param name="sID">The ID of the wanted record</param>
		/// <returns>A data table containing the wanted record or null if no match found</returns>
		public DataTable dtGetRecord(string sID, string sTable)	
		{
			DataTable dtSource = new DataTable();
			DataTable dtResult = new DataTable();
			DataRow[] drSelRow;
			string sQuery;
			dtSource = dsSource.Tables[sTable];
			sQuery = "ID = "+sID;
			drSelRow = dtSource.Select(sQuery);
			if (drSelRow.Length < 1 )
			{
				return null;
			}			
			dtResult=dtSource.Clone();
			dtResult.ImportRow(drSelRow[0]); 
			return dtResult;			
		}


		 
			 

		/*
		/// <summary>
		/// This method is responsible for returning a Label value from the Info 
		/// table based on a specific record using its ID
		/// </summary>
		/// <param name="sID">The ID of the wanted record</param>
		/// <param name="nLang">The wanted language (0 for English, 1 for Arabic)</param>
		/// <returns>A string containing Label or null if no match found</returns>
		public string sGetLabel(string sID, int nLang)	
		{
			string sResult;
			DataRow[] drSelRow;
			string sQuery;
			dtInformation = dsSource.Tables["Info"];
			sQuery = "ID = "+sID;
			drSelRow = dtInformation.Select(sQuery);
			if (drSelRow.Length < 1 )
			{
				return null;
			}			
			sResult = drSelRow[0][nLang+1].ToString();
			return sResult;			
		}
		*/

		/// <summary>
		/// This method is responsible for returning a Connection string value from the Config 
		/// table based on a specific record using its ID 
		/// </summary>
		/// <param name="sID">The ID of the wanted record</param>
		/// <returns>A string containing the connection string or null if no match found</returns>

		/// <summary>
		/// Get the connection string from xml file
		/// </summary>
		/// <param name="sID">The type of the database</param>
		/// <returns>Connection string</returns>
		public string sGetConnection(string sID)	
		{
			string sResult;
			DataRow[] drSelRow;
			string sQuery;
			dtConfig = dsSource.Tables["Config"];
			sQuery = "ID = "+sID;
			drSelRow = dtConfig.Select(sQuery);
			if (drSelRow.Length < 1 )
			{
				return null;
			}			
			sResult = drSelRow[0][1].ToString();
			/////
			///

			string sResultTest = drSelRow[0]["Desc"].ToString();
			////
			return sResult;			
		}

	
		/// <summary>
		/// Get the date mask or formatte to be used in Formatting Date
		/// </summary>
		/// <param name="sID">Database Type</param>
		/// <returns>Date Mask</returns>
		public string sGetDateFormat(int sID)	
		{
			string sResult;
			DataRow[] drSelRow;
			string sQuery;
			dtDateFormat = dsSource.Tables["DateFormat"];
			sQuery = "ID = "+ sID;
			drSelRow = dtDateFormat.Select(sQuery);
			if (drSelRow.Length < 1 )
			{
				return null;
			}	
		
			sResult = drSelRow[0][1].ToString();
			return sResult;			
		}
		
		
		/// <summary>
		/// Get the keyword of the date type of the current database
		/// to be used in create tables statements
		/// </summary>
		/// <param name="sID">Database type</param>
		/// <returns>the keyword</returns>
		public string sGetDateType(int sID)	
		{
			string sResult;
			DataRow[] drSelRow;
			string sQuery;
			dtDateFormat = dsSource.Tables["DateFormat"];
			sQuery = "ID = "+ sID;
			drSelRow = dtDateFormat.Select(sQuery);
			if (drSelRow.Length < 1 )
			{
				return null;
			}	
		
			sResult = drSelRow[0]["Type"].ToString();
			return sResult;			
		}

		/// <summary>
		/// Get the type of the calender set in the xml file
		/// </summary>
		/// <returns>The type of calender</returns>
		public string GetCalenderType()
		{
			string retType = "";

			if(!dsSource.Tables.Contains("Calender"))
				return "";

			dtCalender = dsSource.Tables["Calender"];
			DataRow retDR = dtCalender.Rows[0];

			retType = retDR["Type"].ToString();

			return retType;
		}

		/// <summary>
		/// Get the assigned calender adjust from the xml file
		/// </summary>
		/// <returns>The number in the xml file</returns>
		public int GetCalenderAdjust()
		{
			int retAdj = 0;

			if(!dsSource.Tables.Contains("Calender"))
				return 0;

			dtCalender = dsSource.Tables["Calender"];
			DataRow retDR = dtCalender.Rows[0];

			retAdj = int.Parse(retDR["Adj"].ToString());

			return retAdj;
		}
	}	
}
