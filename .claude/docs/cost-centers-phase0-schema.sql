-- مراكز التكلفة — المرحلة 0 (cost-centers-razor-plan.md §4). إضافات فقط، آمن للتكرار. نفس ما ينفذه CostCenterSchemaCLS.
-- الإعداد CostCenter_Validation_Mode يُنشأ من الكود (SysConfig مشفَّر) ولا يُنشأ هنا.

IF COL_LENGTH(N'dbo.Submain_Account', N'Cost_Center_Policy') IS NULL
    ALTER TABLE [dbo].[Submain_Account] ADD [Cost_Center_Policy] [tinyint] NULL;  -- NULL = اقرأ Accept_Cost_Center_Flage؛ 0 ممنوع 1 اختيارى 2 إلزامى
GO
IF OBJECT_ID(N'dbo.Cost_Center_Project', N'U') IS NULL
CREATE TABLE [dbo].[Cost_Center_Project](
    [Cost_Center_Id] [bigint] NOT NULL,
    [Customer_Id] [bigint] NULL,
    [Contract_Value] [decimal](18, 4) NOT NULL DEFAULT ((0)),
    [Start_Date] [datetime] NULL,
    [End_Date] [datetime] NULL,
    [Status] [tinyint] NOT NULL DEFAULT ((1)),
    [Manager_User_Id] [bigint] NULL,
    [Retention_Percent] [decimal](9, 4) NOT NULL DEFAULT ((0)),
    [Currency_Id] [int] NULL,
    [Notes] [nvarchar](1024) NULL,
    [Insert_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
    [User_Id] [bigint] NULL,
    [EditOperationDate] [datetime] NULL,
    [Edit_User_Id] [bigint] NULL,
    CONSTRAINT [PK_Cost_Center_Project] PRIMARY KEY CLUSTERED ([Cost_Center_Id] ASC)
);
GO
IF OBJECT_ID(N'dbo.Cost_Center_State', N'U') IS NULL
CREATE TABLE [dbo].[Cost_Center_State](
    [Cost_Center_Id] [bigint] NOT NULL,
    [Status] [tinyint] NOT NULL DEFAULT ((1)),   -- 1 نشط 2 موقوف 3 مغلق؛ لا صف = نشط
    [Changed_Date] [datetime] NULL DEFAULT (getdate()),
    [Changed_User_Id] [bigint] NULL,
    CONSTRAINT [PK_Cost_Center_State] PRIMARY KEY CLUSTERED ([Cost_Center_Id] ASC)
);
GO

/* ===== ROLLBACK (يدوى) =====
DROP TABLE IF EXISTS dbo.Cost_Center_State;
DROP TABLE IF EXISTS dbo.Cost_Center_Project;
ALTER TABLE dbo.Submain_Account DROP COLUMN Cost_Center_Policy;
-- و DELETE من SysConfig للصف المشفَّر CostCenter_Validation_Mode (يُحدَّد بفك التشفير)
*/
