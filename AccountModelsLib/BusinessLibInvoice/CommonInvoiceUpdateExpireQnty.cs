using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AccountModelsLib;
using AccountModelsLib.Models;

namespace AccountModelsLib
{
    public class InvoiceUpdateExpireQnty
    {
        public long Unit_Item_ExpireDate_Id { get; set; } = 0;
        public long Unit_Item_Table_Id { get; set; }
        public long Item_Id { get; set; }
        public string Tash3ela { get; set; }
        public DateTime Item_Expire_Date { get; set; }

        public long Bills_Details_Id { get; set; }
        public decimal Larg_Unit_Price { get; set; }



        public decimal Larg_Qnty { get; set; }
        public decimal Small_Qnty { get; set; }
        public decimal Qnty_Another { get; set; }
        public decimal LargItemsQnty_Free { get; set; }


        public decimal SmallItemsQnty_Free { get; set; }
        public decimal Item_No_Free { get; set; }


        public static int Is_Item_Expire_Date_WithBalance { get; set; }
        public static int Is_Tash3ela_WithBalance { get; set; }
        public static int Is_Bills_DetailsId_WithUnit_Item_ExpireDate { get; set; }
        public static int Is_P_Price_WithUnit_Item_ExpireDate { get; set; }



        public static InvoiceUpdateExpireQnty GetInvoiceUpdateExpireQnty(IInvoices_Details invd, bool IsReverse = false)
        {
            InvoiceUpdateExpireQnty inv = new InvoiceUpdateExpireQnty();

            inv.Unit_Item_ExpireDate_Id = invd.Unit_Item_ExpireDate_Id ?? 0;
            inv.Unit_Item_Table_Id = invd.Unit_Item_Table_Id ?? 0;
            inv.Item_Id = invd.Item_Id ?? 0;
            inv.Tash3ela = invd.Tash3ela;
            inv.Item_Expire_Date = invd.Item_Expire_Date.Value;

            inv.Bills_Details_Id = invd.Bills_Details_Id ?? 0;
            inv.Larg_Unit_Price = invd.Larg_Unit_Price ?? 0;



            inv.Larg_Qnty = invd.Larg_Qnty ?? 0;
            inv.Small_Qnty = invd.Small_Qnty ?? 0;
            inv.Qnty_Another = invd.Qnty_Another ?? 0;
            inv.LargItemsQnty_Free = invd.LargItemsQnty_Free ?? 0;


            inv.SmallItemsQnty_Free = invd.SmallItemsQnty_Free ?? 0;
            inv.Item_No_Free = invd.Item_No_Free ?? 0;

            SetSearchParameters(IsReverse);
            return inv;

        }

        public static InvoiceUpdateExpireQnty GetInvoiceUpdateExpireQnty_(IInvoices_Details invD, bool IsReverse = false)
        {
            InvoiceUpdateExpireQnty inv = new InvoiceUpdateExpireQnty();

            inv.Unit_Item_ExpireDate_Id = invD.Unit_Item_ExpireDate_Id ?? 0;
            inv.Unit_Item_Table_Id = invD.Unit_Item_Table_Id ?? 0;
            inv.Item_Id = invD.Item_Id ?? 0;
            inv.Tash3ela = invD.Tash3ela;
            inv.Item_Expire_Date = invD.Item_Expire_Date.Value;

            inv.Bills_Details_Id = invD.Bills_Details_Id ?? 0;
            inv.Larg_Unit_Price = invD.Larg_Unit_Price ?? 0;



            inv.Larg_Qnty = invD.Larg_Qnty ?? 0;
            inv.Small_Qnty = invD.Small_Qnty ?? 0;
            inv.Qnty_Another = invD.Qnty_Another ?? 0;
            inv.LargItemsQnty_Free = invD.LargItemsQnty_Free ?? 0;


            inv.SmallItemsQnty_Free = invD.SmallItemsQnty_Free ?? 0;
            inv.Item_No_Free = invD.Item_No_Free ?? 0;

            SetSearchParameters(IsReverse);
            return inv;

        }

        public static InvoiceUpdateExpireQnty GetInvoiceUpdateExpireQnty(BillOperationCLS invd, bool IsReverse = false)
        {
            InvoiceUpdateExpireQnty inv = new InvoiceUpdateExpireQnty();


            inv.Unit_Item_Table_Id = invd.Unit_Item_Table_Id;
            inv.Item_Id = invd.Item_Id;
            inv.Tash3ela = invd.Tash3ela;
            inv.Item_Expire_Date = invd.Item_Expire_Date;

            inv.Bills_Details_Id = invd.Bills_Details_Id;
            inv.Larg_Unit_Price = invd.Larg_Unit_Price;



            inv.Larg_Qnty = invd.Larg_Qnty;
            inv.Small_Qnty = invd.Small_Qnty;
            inv.Qnty_Another = invd.Qnty_Another;
            inv.LargItemsQnty_Free = invd.LargItemsQnty_Free;


            inv.SmallItemsQnty_Free = invd.SmallItemsQnty_Free;
            inv.Item_No_Free = invd.Item_No_Free;

            SetSearchParameters(IsReverse);
            return inv;

        }
        public static InvoiceUpdateExpireQnty GetInvoiceUpdateExpireQnty(IBills_Details invd, bool IsReverse = false)
        {
            InvoiceUpdateExpireQnty inv = new InvoiceUpdateExpireQnty();


            inv.Unit_Item_Table_Id = invd.Unit_Item_Table_Id ?? 0;
            inv.Item_Id = invd.Item_Id ?? 0;
            inv.Tash3ela = invd.Tash3ela;
            inv.Item_Expire_Date = invd.Item_Expire_Date.Value;

            inv.Bills_Details_Id = invd.Id;
            inv.Larg_Unit_Price = invd.Larg_Unit_Price ?? 0;



            inv.Larg_Qnty = invd.Larg_Qnty ?? 0;
            inv.Small_Qnty = invd.Small_Qnty ?? 0;
            inv.Qnty_Another = invd.Qnty_Another ?? 0;
            inv.LargItemsQnty_Free = invd.LargItemsQnty_Free ?? 0;


            inv.SmallItemsQnty_Free = invd.SmallItemsQnty_Free ?? 0;
            inv.Item_No_Free = invd.Item_No_Free ?? 0;

            SetSearchParameters(IsReverse);
            return inv;

        }
        public static InvoiceUpdateExpireQnty GetInvoiceUpdateExpireQnty(Bills_Details invd, bool IsReverse = false)
        {
            InvoiceUpdateExpireQnty inv = new InvoiceUpdateExpireQnty();


            inv.Unit_Item_Table_Id = invd.Unit_Item_Table_Id ?? 0;
            inv.Item_Id = invd.Item_Id ?? 0;
            inv.Tash3ela = invd.Tash3ela;
            inv.Item_Expire_Date = invd.Item_Expire_Date.Value;

            inv.Bills_Details_Id = invd.Id;
            inv.Larg_Unit_Price = invd.Larg_Unit_Price ?? 0;



            inv.Larg_Qnty = invd.Larg_Qnty ?? 0;
            inv.Small_Qnty = invd.Small_Qnty ?? 0;
            inv.Qnty_Another = invd.Qnty_Another ?? 0;
            inv.LargItemsQnty_Free = invd.LargItemsQnty_Free ?? 0;


            inv.SmallItemsQnty_Free = invd.SmallItemsQnty_Free ?? 0;
            inv.Item_No_Free = invd.Item_No_Free ?? 0;

            SetSearchParameters(IsReverse);
            return inv;

        }


        public static void SetSearchParameters(bool IsReverse = false)
        {
            Is_Item_Expire_Date_WithBalance = 0;
            Is_Tash3ela_WithBalance = 0;
            Is_Bills_DetailsId_WithUnit_Item_ExpireDate = 0;
            Is_P_Price_WithUnit_Item_ExpireDate = 0;


            if (Common.Is_Item_Expire_Date_WithBalance == 1)
            {
                Is_Item_Expire_Date_WithBalance = 1;
            }
            if (Common.Is_Tash3ela_WithBalance == 1)
            {
                Is_Tash3ela_WithBalance = 1;
            }

            if (Common.Is_Bills_DetailsId_WithUnit_Item_ExpireDate == 1 & IsReverse == false)
            {
                Is_Bills_DetailsId_WithUnit_Item_ExpireDate = 1;

            }
            if (Common.Is_P_Price_WithUnit_Item_ExpireDate == 1)
            {
                Is_P_Price_WithUnit_Item_ExpireDate = 1;
            }
        }

    }
}


