-- ============================================================================
-- APPLIED to ClientNew 2026-10-09 WITHOUT orphan deletion (decision). Run with sqlcmd -I (QUOTED_IDENTIFIER ON) because of the filtered index.
-- Plan 02 / Phases 1-2 — Item opening balances: cleanup + fiscal period columns.
--
-- Read-only report (2026-10-09) that this script is based on:
--   3811 rows; 2416 orphans (Unit_Item_Id no longer in Unit_item), of which 153 are non-zero;
--   ALL 153 non-zero orphans belong to items that no longer exist in Item (dates 2023-01-02..2023-08-27),
--   so none can be re-attached. The 1395 valid rows are all zero quantity.
--   Every Balance_Date maps to exactly one Financial_Period (Ids 2..5); 0 duplicates per (period, Unit_Item_Id);
--   0 rows whose Item_Id differs from its Unit_item.Item_id.
-- ============================================================================

-- 1) Full backup copy (kept; never merged back automatically)
IF OBJECT_ID(N'dbo.Item_First_Period_Balance_Backup_20261009', N'U') IS NULL
    SELECT * INTO [dbo].[Item_First_Period_Balance_Backup_20261009] FROM [dbo].[Item_First_Period_Balance];
GO

-- 2) Orphans (stock line deleted) are KEPT by decision 2026-10-09 (applied without deletion).
--    They get no period (the backfill below joins Unit_item), so the filtered unique index and the engine ignore them.
--    To delete them later:
--    DELETE b FROM [dbo].[Item_First_Period_Balance] b WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Unit_item] u WHERE u.id = b.Unit_Item_Id);

-- 3) New columns (plan 02 §2.1, decision m-02-1)
IF COL_LENGTH(N'dbo.Item_First_Period_Balance', N'Year_Financial_Period_Id') IS NULL
    ALTER TABLE [dbo].[Item_First_Period_Balance] ADD [Year_Financial_Period_Id] [bigint] NULL;   -- Financial_Period.Id
IF COL_LENGTH(N'dbo.Item_First_Period_Balance', N'Store_Id') IS NULL
    ALTER TABLE [dbo].[Item_First_Period_Balance] ADD [Store_Id] [bigint] NULL;
IF COL_LENGTH(N'dbo.Item_First_Period_Balance', N'Unit_Cost') IS NULL
    ALTER TABLE [dbo].[Item_First_Period_Balance] ADD [Unit_Cost] [decimal](18, 6) NULL;          -- per SMALL unit
IF COL_LENGTH(N'dbo.Item_First_Period_Balance', N'Source') IS NULL
    ALTER TABLE [dbo].[Item_First_Period_Balance] ADD [Source] [int] NULL;                         -- 0 legacy, 1 manual, 2 import, 3 carried forward
GO

-- 4) Backfill from existing data
UPDATE b SET
    b.Year_Financial_Period_Id = (SELECT TOP 1 p.Id FROM [dbo].[Financial_Period] p
                                  WHERE b.Balance_Date >= p.DateFrom AND b.Balance_Date < DATEADD(day, 1, p.DateTo)),
    b.Store_Id  = u.Store_id,
    b.Unit_Cost = ISNULL(b.P_priceAvgCost, 0) / CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END,
    b.Source    = 0
FROM [dbo].[Item_First_Period_Balance] b
INNER JOIN [dbo].[Unit_item] u ON u.id = b.Unit_Item_Id
WHERE b.Year_Financial_Period_Id IS NULL;
GO

-- 5) One opening row per (period, stock line) from now on
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Item_First_Period_Balance_Period_Line')
CREATE UNIQUE NONCLUSTERED INDEX [UX_Item_First_Period_Balance_Period_Line]
    ON [dbo].[Item_First_Period_Balance]([Year_Financial_Period_Id] ASC, [Unit_Item_Id] ASC)
    WHERE [Year_Financial_Period_Id] IS NOT NULL;
GO

-- Rollback: DROP INDEX UX_Item_First_Period_Balance_Period_Line ON dbo.Item_First_Period_Balance;
--           ALTER TABLE dbo.Item_First_Period_Balance DROP COLUMN Year_Financial_Period_Id, Store_Id, Unit_Cost, Source;
--           DELETE dbo.Item_First_Period_Balance; INSERT INTO dbo.Item_First_Period_Balance SELECT * FROM dbo.Item_First_Period_Balance_Backup_20261009;
-- NOTE: the legacy screen Item_FirstBalance ("Add all") inserts rows without Year_Financial_Period_Id; the filtered unique
--       index ignores those rows, so the legacy screen keeps working until it is replaced (plan 02 phase 3).
