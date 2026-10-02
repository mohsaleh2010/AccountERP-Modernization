using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib
{
    public class AlterMenu_Config_HIS
    {
        public AlterMenu_Config_HIS()
        {
        }
        public static string GetUpdateTablesConfigStr()
        {
            string mysql = GetSetConfigStr();

            return mysql;
        }
        public static string GetCreateNewTablesConfigStr()
        {
            string mysql = @"



drop table [SysConfig]

drop table [SysConfigMenus]


 
CREATE TABLE [dbo].[SysConfig](
	[Id] [numeric](18, 0) IDENTITY(1,1) NOT NULL  PRIMARY KEY,
	[KeysData] [nvarchar](100) NULL,
	[DefaultValue] [nvarchar](500) NULL,
	[Value] [nvarchar](500) NULL,
	[DefaultDescription] [nvarchar](500) NULL,
	[Description] [nvarchar](500) NULL,
	[Client_B] [nvarchar](500) NULL,
	[Client_A] [nvarchar](500) NULL,
	[POS] [nvarchar](500) NULL,
	[Market] [nvarchar](500) NULL,
	[Factory] [nvarchar](500) NULL,
	[AlFawzan] [nvarchar](500) NULL,
	[Tourism] [nvarchar](500) NULL
) ON [PRIMARY]

 
 CREATE TABLE [dbo].[SysConfigMenus](
	[Id] [numeric](18, 0) IDENTITY(1,1) NOT NULL  PRIMARY KEY,
	[Parent_Id] [int] NULL DEFAULT ((0)),
	[KeysData] [nvarchar](100) NULL,
	[Description] [nvarchar](500) NULL,
	[PROJECT_ID] [int] NULL,
	[Value] [nvarchar](500) NULL,
	[DefaultValue] [nvarchar](500) NULL,
	[DefaultDescription] [nvarchar](500) NULL,
	[POS] [nvarchar](500) NULL,
	[Market] [nvarchar](500) NULL,
	[Factory] [nvarchar](500) NULL,
	[Tourism] [nvarchar](500) NULL,
	[AlFawzan] [nvarchar](500) NULL,
	[Client_A] [nvarchar](500) NULL,
	[Client_B] [nvarchar](500) NULL,
	[UserSells] [nvarchar](50) NULL
) ON [PRIMARY]
";
            mysql = mysql + "  " + GetSetConfigStr();
            return mysql;
        }

        public static string GetSetConfigStr()
        {
            string mysql = @"  

 
SET IDENTITY_INSERT [dbo].[SysConfig] ON 

INSERT [dbo].[SysConfig] ([Id], [KeysData], [DefaultValue], [Value], [DefaultDescription], [Description], [Client_B], [Client_A], [POS], [Market], [Factory], [AlFawzan], [Tourism]) VALUES (CAST(10101 AS Numeric(18, 0)), N'HISCompanyTitle ', N'aasasas', N'aasasas', N'', N'', N'aasasas', N'aasasas', N'aasasas', N'aasasas', N'aasasas', N'aasasas', N'0')
 



  SET IDENTITY_INSERT [dbo].[SysConfig] OFF
SET IDENTITY_INSERT [dbo].[SysConfigMenus] ON 

INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(10101 AS Numeric(18, 0)), 0, N'settingsToolStripMenuItem', N'Setup', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(10102 AS Numeric(18, 0)), 10101, N'mnuHospitalProfile', N'Hospital Profile', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(10103 AS Numeric(18, 0)), 10101, N'wardSetupToolStripMenuItem', N'Ward (Floor) Setup', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(10104 AS Numeric(18, 0)), 10101, N'roomSetupToolStripMenuItem', N'Room Setup', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(10105 AS Numeric(18, 0)), 10101, N'bedSetupToolStripMenuItem', N'Bed Setup', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(10106 AS Numeric(18, 0)), 10101, N'servicesSettingToolStripMenuItem', N'Services Setting', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(10107 AS Numeric(18, 0)), 10101, N'subServiceSettingToolStripMenuItem', N'Sub Service Setting', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(10108 AS Numeric(18, 0)), 10101, N'toolStripMenuItemSubServiceItems', N'Sub Service Items Setting', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(10109 AS Numeric(18, 0)), 10101, N'toolStripMenuItemSubServiceValues', N'Sub Service Items Values', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101010 AS Numeric(18, 0)), 10101, N'toolStripMenuItemCompanyContract', N'Company For Contract', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101011 AS Numeric(18, 0)), 10101, N'doctorSpecializationToolStripMenuItem', N'Doctor Specialization', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101012 AS Numeric(18, 0)), 10101, N'labsToolStripMenuItem', N'Labs', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101013 AS Numeric(18, 0)), 10101, N'deseaseSToolStripMenuItem', N'Desease Setup', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101014 AS Numeric(18, 0)), 10101, N'departmentSetupToolStripMenuItem', N'Department Setup', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101015 AS Numeric(18, 0)), 10101, N'toolStripMenuItem26', N'Medicine Setup', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101016 AS Numeric(18, 0)), 10101, N'mnuUser', N'User Management', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101017 AS Numeric(18, 0)), 10101, N'pathologistToolStripMenuItem', N'Pathologist', 10, N'1', N'1',N'1')


INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101018 AS Numeric(18, 0)), 0, N'mnuDoctor', N'Doctors', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101019 AS Numeric(18, 0)), 101018, N'doctorMasterToolStripMenuItem', N'Doctor Master', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101020 AS Numeric(18, 0)), 101018, N'doctorAdvanceToolStripMenuItem', N'Doctor Advance', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101021 AS Numeric(18, 0)), 101018, N'doctorListToolStripMenuItem', N'Doctor List', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101022 AS Numeric(18, 0)), 101018, N'doctorSalaryToolStripMenuItem', N'Doctor Salary', 10, N'1', N'1',N'1')

 
 
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101033 AS Numeric(18, 0)), 0, N'mnuIP', N'Indoor Patient', 10, N'1', N'1',N'1')
 
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101034 AS Numeric(18, 0)), 101033, N'patientAdmissionToolStripMenuItem', N'Patient Admission', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101035 AS Numeric(18, 0)), 101033, N'dailyVisitScheduleToolStripMenuItem', N'Patient Servies &  Billing', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101036 AS Numeric(18, 0)), 101033, N'bedShiftmentToolStripMenuItem', N'Bed Shiftment', 10, N'1', N'1',N'1') 
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101037 AS Numeric(18, 0)), 101033, N'bedFeeToolStripMenuItem', N'Bed Fee', 10, N'1', N'1',N'1') 
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101038 AS Numeric(18, 0)), 101033, N'dischargePatientToolStripMenuItem', N'Discharge Patient', 10, N'1', N'1',N'1') 
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101039 AS Numeric(18, 0)), 101033, N'patientListToolStripMenuItem', N'Patient List', 10, N'1', N'1',N'1') 
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101040 AS Numeric(18, 0)), 101033, N'medicalTreatmentToolStripMenuItem', N'Medical Treatment', 10, N'1', N'1',N'1') 
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101041 AS Numeric(18, 0)), 101033, N'monthlyAdmissionDischargesToolStripMenuItem', N'Monthly Admission && Discharges', 10, N'1', N'1',N'1') 
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101042 AS Numeric(18, 0)), 101033, N'recieveFromPatientToolStripMenuItem', N'Recieve From Patient', 10, N'1', N'1',N'1') 
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101043 AS Numeric(18, 0)), 101033, N'patientInformationToolStripMenuItem', N'Doctor Wise Patient', 10, N'1', N'1',N'1') 
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101044 AS Numeric(18, 0)), 101033, N'OnedailyVisitScheduleToolStripMenuItem', N'Patient One Servies && Billing', 10, N'1', N'1',N'1')


INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101050 AS Numeric(18, 0)), 0, N'ToolStripMenuItemSandoukTransaction_HIS', N'حركة الخزينه', 10, N'1', N'1',N'1')
 
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101051 AS Numeric(18, 0)), 101050, N'ToolStripMenuItemEstlamMoney_HIS', N'إذن إستلام نقديه', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101052 AS Numeric(18, 0)), 101050, N'ToolStripMenuItemSarfMoney_HIS', N'إذن صرف نقديه', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101053 AS Numeric(18, 0)), 101050, N'toolStripMenuItemMoneyBetweenBanks_HIS', N'تحويل نقديه من صندوق إلى أخر', 10, N'1', N'1',N'1') 
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101054 AS Numeric(18, 0)), 101050, N'aToolStripMenuItemMoneyDay_HIS', N'كشف حركة الخزينة عن يوم', 10, N'1', N'1',N'1') 
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101055 AS Numeric(18, 0)), 101050, N'ToolStripMenuItemSandoukDay_HIS', N'حركه الخزينه اليومى', 10, N'1', N'1',N'1') 
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101056 AS Numeric(18, 0)), 101050, N'ToolStripMenuItemOrderList_HIS', N'بحث أذونات النقديه', 10, N'1', N'1',N'1') 
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101057 AS Numeric(18, 0)), 101050, N'menuItemMoneybanks_HIS', N'بيانات الصندوق - الخزينة', 10, N'1', N'1',N'1') 
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101058 AS Numeric(18, 0)), 101050, N'ToolStripMenuItemBoxTrasactionHIS', N'حركه الخزينه', 10, N'1', N'1',N'1') 
 



INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101060 AS Numeric(18, 0)), 0, N'mnuAccountMain_', N'الحسابات', 10, N'1', N'1',N'1')
 
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101061 AS Numeric(18, 0)), 101060, N'accountTreeToolStripMenuItem_HIS_', N'شجرة الحسابات', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101062 AS Numeric(18, 0)), 101060, N'ToolStripMenuItemOstazByPeriod_HIS_', N'الأستاذ المساعد  بالفتره الماليه', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101063 AS Numeric(18, 0)), 101060, N'ToolStripMenuItemOstazByDate_HIS_', N'الأستاذ المساعد عن فتره', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101064 AS Numeric(18, 0)), 101060, N'ToolStripMenuItemNewEntry_HIS_', N'إدخال القيود', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101065 AS Numeric(18, 0)), 101060, N'ToolStripMenuItemEntryReview_HIS_', N'مراجعة القيود', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101066 AS Numeric(18, 0)), 101060, N'account', N'accountSetupToolStripMenuItem_', 10, N'1', N'1',N'1')


INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101070 AS Numeric(18, 0)), 0, N'toolStripMenuItemServicesItems', N'Reports Details', 10, N'1', N'1',N'1')
INSERT  [SysConfigMenus] ([Id], [Parent_Id], [KeysData], [Description], [PROJECT_ID], [Value], [DefaultValue],[Market]) VALUES (CAST(101071 AS Numeric(18, 0)), 101070, N'patientServicesToolStripMenuItem', N'Patient Services', 10, N'1', N'1',N'1')




SET IDENTITY_INSERT [dbo].[SysConfigMenus] OFF
";

            return mysql;
        }
    }
}
