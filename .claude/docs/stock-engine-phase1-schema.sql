-- ============================================================================
-- Plan 01 / Phase 1 — Stock movement engine schema. DRAFT FOR REVIEW - NOT YET APPLIED.
-- Target: ClientNew (decision 2026-10-08). Idempotent: every change runs only if missing.
-- No existing column is renamed or dropped; existing rows keep their behavior.
-- Ids are NOT identity: the app generates them (CallSnowflakeIdGenerate), like the other new tables.
-- ============================================================================

-- ----------------------------------------------------------------------------
-- 1) Item_Branch_Cost — moving weighted average per (item x branch)  [plan 01 §2.3, decision m-01-4]
--    Source of truth for the average. The engine also copies Avg_Cost into
--    Unit_item.P_priceAvgCost for every Unit_item row of the item in the branch,
--    so existing screens/reports that read that column stay correct.
-- ----------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.Item_Branch_Cost', N'U') IS NULL
CREATE TABLE [dbo].[Item_Branch_Cost](
    [Id]                    [bigint] NOT NULL,
    [Item_Id]               [bigint] NOT NULL,
    [Branch_Id]             [bigint] NOT NULL,
    [Avg_Cost]              [decimal](18, 6) NOT NULL DEFAULT ((0)),  -- per SMALL unit
    [Qty_Small]             [decimal](18, 4) NOT NULL DEFAULT ((0)),  -- branch quantity in small units used by the average formula
    [Last_Purchase_Price]   [decimal](18, 6) NULL,                     -- per SMALL unit
    [Last_Movement_Date]    [datetime] NULL,
    [OperationDate]         [datetime] NULL,
    [User_Id]               [bigint] NULL DEFAULT ((0)),
    [Insert_ServerDateTime] [datetime] NULL DEFAULT (getdate()),
    [Update_ServerDateTime] [datetime] NULL,
    [EditOperationDate]     [datetime] NULL,
    [Edit_User_Id]          [bigint] NULL,
    [ProcessNo]             [bigint] NULL,
    CONSTRAINT [PK_Item_Branch_Cost] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Item_Branch_Cost_Item_Branch')
CREATE UNIQUE NONCLUSTERED INDEX [UX_Item_Branch_Cost_Item_Branch]
    ON [dbo].[Item_Branch_Cost]([Item_Id] ASC, [Branch_Id] ASC);
GO

-- ----------------------------------------------------------------------------
-- 2) Order_Purshing — document cycle: draft -> posted -> reversed  [decision m-01-8]
--    Doc_Status: 0 draft (no stock effect), 1 posted, 2 reversed (a reversal document exists), 3 cancelled draft.
--    Existing rows are legacy posted documents -> backfilled to 1.
-- ----------------------------------------------------------------------------
IF COL_LENGTH(N'dbo.Order_Purshing', N'Doc_Status') IS NULL
    ALTER TABLE [dbo].[Order_Purshing] ADD [Doc_Status] [int] NULL;
IF COL_LENGTH(N'dbo.Order_Purshing', N'Reversal_Of_Id') IS NULL
    ALTER TABLE [dbo].[Order_Purshing] ADD [Reversal_Of_Id] [bigint] NULL;      -- on a reversal doc: the original Order_Purshing.Id
IF COL_LENGTH(N'dbo.Order_Purshing', N'Reversed_By_Id') IS NULL
    ALTER TABLE [dbo].[Order_Purshing] ADD [Reversed_By_Id] [bigint] NULL;      -- on an original doc: its reversal Order_Purshing.Id
IF COL_LENGTH(N'dbo.Order_Purshing', N'Reverse_Reason') IS NULL
    ALTER TABLE [dbo].[Order_Purshing] ADD [Reverse_Reason] [nvarchar](500) NULL;
IF COL_LENGTH(N'dbo.Order_Purshing', N'Posted_Date') IS NULL
    ALTER TABLE [dbo].[Order_Purshing] ADD [Posted_Date] [datetime] NULL;
IF COL_LENGTH(N'dbo.Order_Purshing', N'Posted_By') IS NULL
    ALTER TABLE [dbo].[Order_Purshing] ADD [Posted_By] [bigint] NULL;
GO
UPDATE [dbo].[Order_Purshing] SET [Doc_Status] = 1 WHERE [Doc_Status] IS NULL;   -- legacy rows = posted
GO

-- ----------------------------------------------------------------------------
-- 3) Detailes_Purshing_Import_Export (DPIE, the stock ledger) — precise cost snapshot + reversal link
--    Existing P_priceAvgCost / PriceUnit columns are decimal(18,2) and are kept as-is (legacy readers).
-- ----------------------------------------------------------------------------
IF COL_LENGTH(N'dbo.Detailes_Purshing_Import_Export', N'Unit_Cost') IS NULL
    ALTER TABLE [dbo].[Detailes_Purshing_Import_Export] ADD [Unit_Cost] [decimal](18, 6) NULL;        -- cost per SMALL unit of this movement
IF COL_LENGTH(N'dbo.Detailes_Purshing_Import_Export', N'Qty_Small') IS NULL
    ALTER TABLE [dbo].[Detailes_Purshing_Import_Export] ADD [Qty_Small] [decimal](18, 4) NULL;        -- signed quantity in small units (+ in / - out)
IF COL_LENGTH(N'dbo.Detailes_Purshing_Import_Export', N'Avg_Cost_Before') IS NULL
    ALTER TABLE [dbo].[Detailes_Purshing_Import_Export] ADD [Avg_Cost_Before] [decimal](18, 6) NULL;
IF COL_LENGTH(N'dbo.Detailes_Purshing_Import_Export', N'Avg_Cost_After') IS NULL
    ALTER TABLE [dbo].[Detailes_Purshing_Import_Export] ADD [Avg_Cost_After] [decimal](18, 6) NULL;
IF COL_LENGTH(N'dbo.Detailes_Purshing_Import_Export', N'Reversal_Of_Id') IS NULL
    ALTER TABLE [dbo].[Detailes_Purshing_Import_Export] ADD [Reversal_Of_Id] [bigint] NULL;            -- the DPIE row this one reverses
IF COL_LENGTH(N'dbo.Detailes_Purshing_Import_Export', N'Engine_Posted') IS NULL
    ALTER TABLE [dbo].[Detailes_Purshing_Import_Export] ADD [Engine_Posted] [bit] NULL;                -- 1 = written by StockMovementService (vs legacy writers)
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DPIE_UnitItem_Date')
CREATE NONCLUSTERED INDEX [IX_DPIE_UnitItem_Date]
    ON [dbo].[Detailes_Purshing_Import_Export]([Unit_item_Table_Id] ASC, [Process_Date] ASC)
    INCLUDE ([Transaction_Type], [qua], [Part_qua], [Item_No], [Qty_Small]);   -- GetComputedBalance / item card
GO

-- ----------------------------------------------------------------------------
-- Rollback (manual, only if the phase is abandoned before any engine posting):
--   DROP TABLE dbo.Item_Branch_Cost;
--   ALTER TABLE dbo.Order_Purshing DROP COLUMN Doc_Status, Reversal_Of_Id, Reversed_By_Id, Reverse_Reason, Posted_Date, Posted_By;
--   DROP INDEX IX_DPIE_UnitItem_Date ON dbo.Detailes_Purshing_Import_Export;
--   ALTER TABLE dbo.Detailes_Purshing_Import_Export DROP COLUMN Unit_Cost, Qty_Small, Avg_Cost_Before, Avg_Cost_After, Reversal_Of_Id, Engine_Posted;
-- ============================================================================
