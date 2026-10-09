-- ============================================================================
-- Plan 03 / Phase 1 — Store order type settings (data instead of hard-coded rules). APPLIED to ClientNew 2026-10-09 (22 rows).
-- Target: ClientNew. Idempotent. Run with sqlcmd -I.
-- The seed reproduces EXACTLY the current hard-coded behavior:
--   * parties per type and defaults  <- Store_LIB\OrdersNew\NewLast\FillcbOrderStorCLS.cs (Load_Combo / Fillcb_From_To_N)
--   * party lists                    <- connectionit\UsrCntrl\StoreOrders.cs  (4 store, 20 department, 2 customer, 1 supplier,
--                                       23 machine/line, 40 employee, 3 damaged, 7 adj+, 8 adj-, 9 from production,
--                                       10 to production, 12 spare parts, 18 to workshop, 19 from workshop)
--   * direction                      <- DataDictionaryProcessesLists.Order_Estlam_Sarf (1 in, 2 out)
-- Columns marked [proposal] are NOT in the current code; they are this plan's proposal and are listed for review.
-- ============================================================================

IF OBJECT_ID(N'dbo.Store_Order_Type_Settings', N'U') IS NULL
CREATE TABLE [dbo].[Store_Order_Type_Settings](
    [Order_Type]                    [int] NOT NULL,             -- StoreOrderType (1001..1023), unchanged codes
    [Name_Ar]                       [nvarchar](200) NOT NULL,
    [Direction]                     [int] NOT NULL,             -- 1 inbound to the store, 2 outbound from the store, 3 transfer between stores/departments
    [Store_Side]                    [int] NOT NULL,             -- legacy lookUpStoreId: 1 = "from" is the store, 2 = "to" is the store
    [From_Party_Types]              [nvarchar](50) NOT NULL,    -- comma list of party codes allowed on the "from" side
    [Default_From_Party]            [int] NOT NULL,
    [To_Party_Types]                [nvarchar](50) NOT NULL,
    [Default_To_Party]              [int] NOT NULL,
    [Is_Virtual_Outer_From]         [bit] NOT NULL DEFAULT ((0)),  -- 1014: "from" is a virtual outer store
    [Is_Virtual_Outer_To]           [bit] NOT NULL DEFAULT ((0)),  -- 1015: "to" is a virtual outer store
    [Price_Source]                  [int] NOT NULL DEFAULT ((1)),  -- [proposal] 0 none, 1 branch average, 2 last purchase price, 3 manual entry
    [Updates_Avg_Cost]              [bit] NOT NULL DEFAULT ((0)),  -- [proposal] inbound with its own cost updates the branch average
    [Is_Purchase]                   [bit] NOT NULL DEFAULT ((0)),  -- [proposal] sets the last purchase price
    [Is_Transfer]                   [bit] NOT NULL DEFAULT ((0)),  -- uses the transfer mode setting (plan 01 §2.7)
    [Is_Active]                     [bit] NOT NULL DEFAULT ((1)),
    [OperationDate]                 [datetime] NULL,
    [User_Id]                       [bigint] NULL DEFAULT ((0)),
    [Insert_ServerDateTime]         [datetime] NULL DEFAULT (getdate()),
    [Update_ServerDateTime]         [datetime] NULL,
    [EditOperationDate]             [datetime] NULL,
    [Edit_User_Id]                  [bigint] NULL,
    CONSTRAINT [PK_Store_Order_Type_Settings] PRIMARY KEY CLUSTERED ([Order_Type] ASC)
);
GO

-- Seed (insert missing types only; existing rows are never overwritten)
;WITH s AS (
SELECT * FROM (VALUES
 -- type, name,                                                        dir, side, from,   defFrom, to,     defTo, vFrom, vTo, price, avg, purch, transfer
 (1001, N'إذن صرف / تحويل لمخزن أو قسم',                               3, 1, N'4,20', 4,  N'4,20', 4,  0, 0, 1, 0, 0, 1),
 (1002, N'إذن صرف لعميل',                                              2, 1, N'4,20', 4,  N'2',    2,  0, 0, 1, 0, 0, 0),
 (1003, N'إذن مرتجع صرف من عميل',                                      1, 2, N'2',    2,  N'4,20', 4,  0, 0, 1, 0, 0, 0),
 (1004, N'إذن إستلام من مورد',                                         1, 2, N'1',    1,  N'4,20', 4,  0, 0, 3, 1, 1, 0),
 (1005, N'إذن مرتجع لمورد',                                            2, 1, N'4,20', 4,  N'1',    1,  0, 0, 1, 0, 0, 0),
 (1006, N'إذن مرتجع صرف / تحويل لمخزن أو قسم',                         3, 1, N'4,20', 4,  N'4,20', 4,  0, 0, 1, 0, 0, 1),
 (1007, N'إذن تسوية بالزيادة',                                         1, 2, N'7',    7,  N'4,20', 4,  0, 0, 1, 0, 0, 0),
 (1008, N'إذن تسوية بالنقص',                                           2, 1, N'4,20', 4,  N'8',    8,  0, 0, 1, 0, 0, 0),
 (1009, N'إذن إستلام من الإنتاج',                                      1, 2, N'9',    9,  N'4,20', 4,  0, 0, 1, 0, 0, 0),
 (1010, N'إذن صرف إلى الإنتاج / التشغيل',                              2, 1, N'4,20', 4,  N'10',   10, 0, 0, 1, 0, 0, 0),
 (1012, N'إذن صرف قطع غيار',                                           2, 1, N'4,20', 4,  N'12',   12, 0, 0, 1, 0, 0, 0),
 (1013, N'صرف للهالك',                                                 2, 1, N'4,20', 4,  N'3',    3,  0, 0, 1, 0, 0, 0),
 (1014, N'إذن إستلام من مخزن خارجى',                                   1, 2, N'4,20', 4,  N'4,20', 4,  1, 0, 1, 0, 0, 0),
 (1015, N'إذن صرف إلى مخزن خارجى',                                     2, 1, N'4,20', 4,  N'4,20', 4,  0, 1, 1, 0, 0, 0),
 (1016, N'إذن إستلام من المخزن',                                       1, 1, N'4,20', 4,  N'4,20', 4,  0, 0, 1, 0, 0, 1),
 (1017, N'إذن صرف إلى المخزن',                                         2, 1, N'4,20', 4,  N'4,20', 4,  0, 0, 1, 0, 0, 1),
 (1018, N'إذن صرف للورشة',                                             2, 1, N'4,20', 4,  N'18',   18, 0, 0, 1, 0, 0, 0),
 (1019, N'إذن مرتجع صرف من الورشة',                                    1, 2, N'19',   19, N'4,20', 4,  0, 0, 1, 0, 0, 0),
 (1020, N'إذن صرف لموظف',                                              2, 1, N'4,20', 4,  N'40',   40, 0, 0, 1, 0, 0, 0),
 (1021, N'إذن إستلام من موظف',                                         1, 2, N'40',   40, N'4,20', 4,  0, 0, 1, 0, 0, 0),
 (1022, N'إذن صرف قطع الغيار للماكينات وخطوط الإنتاج',                 2, 1, N'4,20', 4,  N'23',   23, 0, 0, 1, 0, 0, 0),
 (1023, N'إذن مرتجع صرف قطع الغيار للماكينات وخطوط الإنتاج',           1, 2, N'23',   23, N'4,20', 4,  0, 0, 1, 0, 0, 0)
) v(Order_Type, Name_Ar, Direction, Store_Side, From_Party_Types, Default_From_Party, To_Party_Types, Default_To_Party,
    Is_Virtual_Outer_From, Is_Virtual_Outer_To, Price_Source, Updates_Avg_Cost, Is_Purchase, Is_Transfer))
INSERT INTO [dbo].[Store_Order_Type_Settings]
    (Order_Type, Name_Ar, Direction, Store_Side, From_Party_Types, Default_From_Party, To_Party_Types, Default_To_Party,
     Is_Virtual_Outer_From, Is_Virtual_Outer_To, Price_Source, Updates_Avg_Cost, Is_Purchase, Is_Transfer, OperationDate)
SELECT s.Order_Type, s.Name_Ar, s.Direction, s.Store_Side, s.From_Party_Types, s.Default_From_Party, s.To_Party_Types, s.Default_To_Party,
       s.Is_Virtual_Outer_From, s.Is_Virtual_Outer_To, s.Price_Source, s.Updates_Avg_Cost, s.Is_Purchase, s.Is_Transfer, CAST(GETDATE() AS date)
FROM s WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Store_Order_Type_Settings] t WHERE t.Order_Type = s.Order_Type);
GO

-- Notes for review:
--  * 1001/1006/1016/1017 are store-to-store movements (Is_Transfer = 1): they follow the transfer mode setting
--    (always confirm / always direct / by permission), plan 01 §2.7 — built in plan 03 phase 3.
--  * The two empty menu items «إذن صرف لقسم» / «إذن إستلام من قسم» need no new type: they open 1001 with
--    the department (20) preselected on the "to" / "from" side (plan 03 decision m-03-5).
--  * 1009 (receipt from production) keeps Price_Source = average until production costing exists (plan 05, deferred).
-- Rollback: DROP TABLE dbo.Store_Order_Type_Settings;
