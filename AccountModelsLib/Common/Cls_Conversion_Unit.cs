using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace AccountModelsLib
{

    public class Cls_Conversion_Unit
    {
        // by programmer hossam shaaban shaaban  eissa 
        // in 26 1 2007

        public void ConVersionQuntity_ADD_ToItem(float InTon_new, float inKilo_new, float InTon_Old, float inKilo_Old, out float OutTon_, out float OutKilo_, int Unit_Id)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataSet Unit_ds = new DataSet();
            Unit_ds = opp.return_dataset("select * from unit where id= " + Unit_Id);
            int S_unit_value = int.Parse(Unit_ds.Tables[0].Rows[0]["S_unit_value"].ToString());
            if (inKilo_new + inKilo_Old >= S_unit_value)
            {
                InTon_new += 1;
                OutKilo_ = inKilo_new + inKilo_Old - S_unit_value;
            }
            else
            {
                OutKilo_ = inKilo_new + inKilo_Old;
            }

            OutTon_ = InTon_new + InTon_Old;

        }

        public void ConVersionQuntity_Push_FromItem(float InBig_new, float inSmall_new, float InBig_Old, float inSmall_Old, out float Remain_big, out float Remain_inSmall, int Unit_Id)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataSet Unit_ds = new DataSet();
            Unit_ds = opp.return_dataset("select * from unit where id= " + Unit_Id);
            int S_unit_value = int.Parse(Unit_ds.Tables[0].Rows[0]["S_unit_value"].ToString());

            float TotalQuntity_With_Small_unit_exist = InBig_Old * S_unit_value + inSmall_Old;
            float TotalQuntity_With_Small_unit_Needed = InBig_new * S_unit_value + inSmall_new;
            Remain_big = (int)((TotalQuntity_With_Small_unit_exist - TotalQuntity_With_Small_unit_Needed) / S_unit_value);
            Remain_inSmall = (TotalQuntity_With_Small_unit_exist - TotalQuntity_With_Small_unit_Needed) - Remain_big * S_unit_value;


        }

        public void ConVersionQuntity_ADD_ToItem(decimal LargQnty, decimal SmallQnty, decimal LargBalance, decimal SmallBalance, ref decimal LargBalance_, ref decimal SmallBalance_, decimal S_unit_value)
        {
            decimal SmallTotalQnty;
            decimal SmallTotalLarg;
            decimal SmallTotal;
            long LargBalanceFinal = 0;
            int SmallBalanceFinal = 0;

            SmallTotalLarg = SmallBalance + LargBalance * S_unit_value;
            SmallTotalQnty = SmallQnty + LargQnty * S_unit_value;

            SmallTotal = SmallTotalQnty + SmallTotalLarg;

            LargBalanceFinal = Math.DivRem((int)SmallTotal, (int)S_unit_value, out SmallBalanceFinal);
            LargBalance_ = (decimal)LargBalanceFinal;
            SmallBalance_ = (decimal)SmallBalanceFinal;


        }

        /// <summary>
        /// Check IF can subtract
        /// </summary>
        /// <param name="CheckOnly"></param>
        /// <param name="LargQnty"></param>
        /// <param name="SmallQnty"></param>
        /// <param name="LargBalance"></param>
        /// <param name="SmallBalance"></param>
        /// <param name="LargBalance_"></param>
        /// <param name="SmallBalance_"></param>
        /// <param name="S_unit_value"></param>
        /// <returns></returns>
        public bool ConVersionQuntity_Subtract_ToItem(bool CheckOnly, decimal LargQnty, decimal SmallQnty, decimal LargBalance, decimal SmallBalance, ref decimal LargBalance_, ref decimal SmallBalance_, decimal S_unit_value)
        {
            decimal SmallTotalQnty;
            decimal SmallTotalLarg;
            decimal SmallTotal;
            long LargBalanceFinal = 0;
            int SmallBalanceFinal = 0;

            SmallTotalLarg = SmallBalance + LargBalance * S_unit_value;
            SmallTotalQnty = SmallQnty + LargQnty * S_unit_value;

            SmallTotal = SmallTotalLarg - SmallTotalQnty;
            if (SmallTotal < 0)
            {
                return false;
            }
            else
            {

                if (!CheckOnly)
                {
                    long SmallBalanceFinal_ = 0;
                    try
                    {
                        LargBalanceFinal = Math.DivRem((int)SmallTotal, (int)S_unit_value, out SmallBalanceFinal);
                    }
                    catch
                    {
                        long SmallTotal_ = 0;

                        long S_unit_value_ = long.Parse(S_unit_value.ToString());

                        try
                        {
                            SmallTotal_ = long.Parse(SmallTotal.ToString());
                        }
                        catch
                        {
                            //SmallTotal = SmallTotal / 1000;
                            SmallTotal = Common.RoundMe(SmallTotal, 0);
                            SmallTotal_ = long.Parse(SmallTotal.ToString());

                        }
                        LargBalanceFinal = Math.DivRem(SmallTotal_, S_unit_value_, out SmallBalanceFinal_);


                    }
                    LargBalance_ = (decimal)LargBalanceFinal;
                    SmallBalance_ = decimal.Parse(SmallBalanceFinal_.ToString());
                    return true;
                }
                return true;
            }


        }

        /// <summary>
        /// return the value that can show if > or < 0
        /// </summary>
        /// <param name="LargQnty"></param>
        /// <param name="SmallQnty"></param>
        /// <param name="LargBalance"></param>
        /// <param name="SmallBalance"></param>
        /// <param name="LargBalance_"></param>
        /// <param name="SmallBalance_"></param>
        /// <param name="S_unit_value"></param>
        /// <returns></returns>
        public decimal ConVersionQuntity_Subtract(bool CheckOnly, decimal LargQnty, decimal SmallQnty, decimal LargBalance, decimal SmallBalance, ref decimal LargBalance_, ref decimal SmallBalance_, decimal S_unit_value)
        {
            decimal SmallTotalQnty;
            decimal SmallTotalLarg;
            decimal SmallTotal;
            long LargBalanceFinal = 0;
            int SmallBalanceFinal = 0;

            SmallTotalLarg = SmallBalance + LargBalance * S_unit_value;
            SmallTotalQnty = SmallQnty + LargQnty * S_unit_value;

            SmallTotal = SmallTotalQnty - SmallTotalLarg;


            LargBalanceFinal = Math.DivRem((int)SmallTotal, (int)S_unit_value, out SmallBalanceFinal);
            LargBalance_ = (decimal)LargBalanceFinal;
            SmallBalance_ = (decimal)SmallBalanceFinal;

            if (LargBalance_ < 0 | SmallBalance_ < 0)
            {
                LargBalance_ = LargBalance_ * -1;
                SmallBalance_ = SmallBalance_ * -1;
            }

            return SmallTotal;

        }
        /// <summary>
        /// /«·€«¡ «·ﬂ”Ê— „‰ «·ﬂ„Ì« 
        /// </summary>
        /// <param name="Larg_Qnty"></param>
        /// <param name="Small_Qnty"></param>
        /// <param name="S_unit_Value"></param>
        public static void ConVert_By_S_UnitValue(ref decimal Larg_Qnty, ref decimal Small_Qnty, decimal S_unit_Value)
        {
            if (Common.Compute_ItemBalance_With_Fraction == 0)
            {
                Small_Qnty = Small_Qnty + (Larg_Qnty * S_unit_Value);


                long Larg_Qnty_ = (long)Larg_Qnty;
                long Small_Qnty_ = (long)Small_Qnty;

                Larg_Qnty_ = Math.DivRem(Small_Qnty_, (long)S_unit_Value, out Small_Qnty_);
                Larg_Qnty = (decimal)Larg_Qnty_;
                Small_Qnty = (decimal)Small_Qnty_;
            }
            else
            {
                Small_Qnty = Small_Qnty + (Larg_Qnty * S_unit_Value);


                long Larg_Qnty_ = (long)Larg_Qnty;
                long Small_Qnty_ = (long)Small_Qnty;


                Larg_Qnty = Small_Qnty / S_unit_Value;

                Larg_Qnty = Common.RoundMePositive(Larg_Qnty);
                Small_Qnty = 0;

            }

        }

        public static decimal GetTotalSmallQntity(decimal Larg_Qnty, decimal Small_Qnty, decimal S_unit_Value)
        {
            Small_Qnty = Small_Qnty + (Larg_Qnty * S_unit_Value);
            Small_Qnty = Common.RoundMePositive(Small_Qnty);

            return Small_Qnty;

        }


        public static decimal GetTotalLargeQntity(decimal Larg_Qnty, decimal Small_Qnty, decimal S_unit_Value)
        {
            if (S_unit_Value == 0)
            { S_unit_Value = 1; }

            Small_Qnty = Small_Qnty + (Larg_Qnty * S_unit_Value);
            Small_Qnty = Common.RoundMePositive(Small_Qnty);

            decimal LargeTotal = Small_Qnty / S_unit_Value;
            LargeTotal = Common.RoundMePositive(LargeTotal);
            return LargeTotal;

        }

    }
}
