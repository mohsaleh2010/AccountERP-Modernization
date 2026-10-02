-- ============================================================================
-- Customer Orders (طلبيات العملاء) - new tables. DRAFT FOR REVIEW - NOT YET APPLIED.
-- Idempotent: each table is created only if missing. Requests_Details is untouched (legacy).
-- Ids are NOT identity: the app generates them (Common.GETAutoNumber -> Snowflake when Use_Auto_Identity_Column=0), like the rest of the schema.
-- ============================================================================

-- 1) Header
IF OBJECT_ID(N'dbo.Customer_Orders', N'U') IS NULL
CREATE TABLE [dbo].[Customer_Orders](
    [Id]                     [bigint] NOT NULL,
    [Order_No]               [bigint] NULL,            -- per Branch_Id + year sequence
    [Order_Date]             [datetime] NULL,
    [Expected_Delivery_Date] [datetime] NULL,
    [Customer_Id]            [bigint] NULL,
    [Store_Id]               [bigint] NULL,
    [Currency_Id]            [bigint] NULL,
    [Sales_Person_Id]        [bigint] NULL,
    [Status]                 [int] NULL DEFAULT ((0)), -- 0 draft,1 confirmed,2 partial,3 fulfilled,4 cancelled,5 closed
    [Gross_Total]            [decimal](18, 2) NULL DEFAULT ((0)),
    [Discount_Total]         [decimal](18, 2) NULL DEFAULT ((0)),
    [Tax_Total]              [decimal](18, 2) NULL DEFAULT ((0)),
    [Net_Total]              [decimal](18, 2) NULL DEFAULT ((0)),
    [Notes]                  [nvarchar](500) NULL,
    [Cancel_Reason]          [nvarchar](500) NULL,
    [Cancelled_By]           [bigint] NULL,
    [Cancelled_Date]         [datetime] NULL,
    [OperationDate]          [datetime] NULL,
    [User_Id]                [bigint] NULL DEFAULT ((0)),
    [Branch_Id]              [bigint] NULL DEFAULT ((0)),
    [Insert_ServerDateTime]  [datetime] NULL DEFAULT (getdate()),
    [Update_ServerDateTime]  [datetime] NULL DEFAULT (getdate()),
    [EditOperationDate]      [datetime] NULL,
    [Edit_User_Id]           [bigint] NULL DEFAULT ((0)),
    [ProcessNo]              [bigint] NULL,
    CONSTRAINT [PK_Customer_Orders] PRIMARY KEY CLUSTERED ([Id])
);

-- 2) Lines (one row per item/unit on the order)
IF OBJECT_ID(N'dbo.Customer_Order_Lines', N'U') IS NULL
CREATE TABLE [dbo].[Customer_Order_Lines](
    [Id]                     [bigint] NOT NULL,
    [Order_Id]               [bigint] NOT NULL,        -- -> Customer_Orders.Id
    [Line_No]                [int] NULL,
    [Unit_Item_Table_Id]     [bigint] NULL,            -- -> Unit_item.id (same key the invoices use)
    [Qty_Larg]               [decimal](18, 2) NULL DEFAULT ((0)),
    [Qty_Small]              [decimal](18, 2) NULL DEFAULT ((0)),
    [Unit_Price]             [decimal](18, 2) NULL DEFAULT ((0)),
    [Discount_Percent]       [decimal](18, 2) NULL DEFAULT ((0)),
    [Discount_Value]         [decimal](18, 2) NULL DEFAULT ((0)),
    [Tax_Value]              [decimal](18, 2) NULL DEFAULT ((0)),
    [Line_Total]             [decimal](18, 2) NULL DEFAULT ((0)),
    [Fulfilled_Qty_Larg]     [decimal](18, 2) NULL DEFAULT ((0)),  -- maintained from Customer_Order_Fulfillments
    [Fulfilled_Qty_Small]    [decimal](18, 2) NULL DEFAULT ((0)),
    [Cancelled_Qty_Larg]     [decimal](18, 2) NULL DEFAULT ((0)),
    [Cancelled_Qty_Small]    [decimal](18, 2) NULL DEFAULT ((0)),
    [Line_Status]            [int] NULL DEFAULT ((0)), -- 0 open,1 partial,2 fulfilled,3 cancelled
    [Notes]                  [nvarchar](500) NULL,
    [OperationDate]          [datetime] NULL,
    [User_Id]                [bigint] NULL DEFAULT ((0)),
    [Branch_Id]              [bigint] NULL DEFAULT ((0)),
    [Insert_ServerDateTime]  [datetime] NULL DEFAULT (getdate()),
    [Update_ServerDateTime]  [datetime] NULL DEFAULT (getdate()),
    [EditOperationDate]      [datetime] NULL,
    [Edit_User_Id]           [bigint] NULL DEFAULT ((0)),
    [ProcessNo]              [bigint] NULL,
    CONSTRAINT [PK_Customer_Order_Lines] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Customer_Order_Lines_Order] FOREIGN KEY ([Order_Id]) REFERENCES [dbo].[Customer_Orders]([Id])
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Customer_Order_Lines_Order_Id')
    CREATE INDEX [IX_Customer_Order_Lines_Order_Id] ON [dbo].[Customer_Order_Lines]([Order_Id]);

-- 3) Deposits (each row links an existing cash receipt in Money_Transaction_Box)
IF OBJECT_ID(N'dbo.Customer_Order_Deposits', N'U') IS NULL
CREATE TABLE [dbo].[Customer_Order_Deposits](
    [Id]                        [bigint] NOT NULL,
    [Order_Id]                  [bigint] NOT NULL,
    [Money_Transaction_Box_Id]  [bigint] NULL,
    [Amount]                    [decimal](18, 2) NULL DEFAULT ((0)),
    [Consumed_Amount]           [decimal](18, 2) NULL DEFAULT ((0)),  -- applied to invoices so far
    [Deposit_Date]              [datetime] NULL,
    [Notes]                     [nvarchar](500) NULL,
    [OperationDate]             [datetime] NULL,
    [User_Id]                   [bigint] NULL DEFAULT ((0)),
    [Branch_Id]                 [bigint] NULL DEFAULT ((0)),
    [Insert_ServerDateTime]     [datetime] NULL DEFAULT (getdate()),
    [Update_ServerDateTime]     [datetime] NULL DEFAULT (getdate()),
    [EditOperationDate]         [datetime] NULL,
    [Edit_User_Id]              [bigint] NULL DEFAULT ((0)),
    [ProcessNo]                 [bigint] NULL,
    CONSTRAINT [PK_Customer_Order_Deposits] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Customer_Order_Deposits_Order] FOREIGN KEY ([Order_Id]) REFERENCES [dbo].[Customer_Orders]([Id])
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Customer_Order_Deposits_Order_Id')
    CREATE INDEX [IX_Customer_Order_Deposits_Order_Id] ON [dbo].[Customer_Order_Deposits]([Order_Id]);

-- 4) Fulfillments (order line <-> invoice line link; enables partial delivery)
IF OBJECT_ID(N'dbo.Customer_Order_Fulfillments', N'U') IS NULL
CREATE TABLE [dbo].[Customer_Order_Fulfillments](
    [Id]                     [bigint] NOT NULL,
    [Order_Line_Id]          [bigint] NOT NULL,        -- -> Customer_Order_Lines.Id
    [Invoices_List_Id]       [bigint] NULL,            -- the sales invoice that consumed the quantity
    [Invoices_Details_Id]    [bigint] NULL,
    [Qty_Larg]               [decimal](18, 2) NULL DEFAULT ((0)),
    [Qty_Small]              [decimal](18, 2) NULL DEFAULT ((0)),
    [Fulfill_Date]           [datetime] NULL,
    [OperationDate]          [datetime] NULL,
    [User_Id]                [bigint] NULL DEFAULT ((0)),
    [Branch_Id]              [bigint] NULL DEFAULT ((0)),
    [Insert_ServerDateTime]  [datetime] NULL DEFAULT (getdate()),
    [Update_ServerDateTime]  [datetime] NULL DEFAULT (getdate()),
    [EditOperationDate]      [datetime] NULL,
    [Edit_User_Id]           [bigint] NULL DEFAULT ((0)),
    [ProcessNo]              [bigint] NULL,
    CONSTRAINT [PK_Customer_Order_Fulfillments] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Customer_Order_Fulfillments_Line] FOREIGN KEY ([Order_Line_Id]) REFERENCES [dbo].[Customer_Order_Lines]([Id])
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Customer_Order_Fulfillments_Line')
    CREATE INDEX [IX_Customer_Order_Fulfillments_Line] ON [dbo].[Customer_Order_Fulfillments]([Order_Line_Id]);

-- 5) (2026-09-25) Deposits link to the cash voucher header: Money_Transaction_Box.Id is NOT unique in the legacy schema.
IF COL_LENGTH('dbo.Customer_Order_Deposits','Order_Money_Id') IS NULL
    ALTER TABLE [dbo].[Customer_Order_Deposits] ADD [Order_Money_Id] [bigint] NULL;

GO
-- 5) Deposit applications (which part of an order deposit settled which invoice) — added 2026-09-26
IF OBJECT_ID(N'dbo.Customer_Order_Deposit_Applications', N'U') IS NULL
CREATE TABLE [dbo].[Customer_Order_Deposit_Applications](
    [Id]                 [bigint] NOT NULL,
    [Deposit_Id]         [bigint] NOT NULL,
    [Order_Id]           [bigint] NOT NULL,
    [Invoices_List_Id]   [bigint] NOT NULL,
    [Amount]             [decimal](18, 2) NOT NULL,
    [Applied_Date]       [datetime] NULL,
    [User_Id]            [bigint] NULL,
    [Branch_Id]          [bigint] NULL,
    [Insert_ServerDateTime] [datetime] NULL,
    CONSTRAINT [PK_Customer_Order_Deposit_Applications] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_COD_Applications_Deposit] FOREIGN KEY ([Deposit_Id]) REFERENCES [dbo].[Customer_Order_Deposits]([Id])
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_COD_Applications_Invoice')
    CREATE INDEX [IX_COD_Applications_Invoice] ON [dbo].[Customer_Order_Deposit_Applications]([Invoices_List_Id]);
