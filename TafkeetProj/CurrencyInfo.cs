using System;

namespace TafkeetProj
{
    public class CurrencyInfo
    {
        public enum Currencies { Egypt=1, USA=2,EURP=3, SaudiArabia=4, UAE=5,   Syria = 6, Tunisia = 7, Gold = 8 };

        #region Constructors

        public CurrencyInfo(Currencies currency)
        {
            switch (currency)
            {
                case Currencies.Egypt:
                    CurrencyID = 1;
                    CurrencyCode = "LE";
                    IsCurrencyNameFeminine = true;
                    EnglishCurrencyName = "Egyptian Pound";
                    EnglishPluralCurrencyName = "Egyptian Pounds";
                    EnglishCurrencyPartName = "Piaster";
                    EnglishPluralCurrencyPartName = "Piasteres";
                    Arabic1CurrencyName = "جنيه مصرى";
                    Arabic2CurrencyName = "جنيهان مصرى";
                    //Arabic310CurrencyName = "جنيهات مصره";
                    //Arabic1199CurrencyName = "جنيهان مصرى";
                    Arabic310CurrencyName = "جنيه مصره";
                    Arabic1199CurrencyName = "جنيه مصرى";
                    Arabic1CurrencyPartName = "قرش";
                    Arabic2CurrencyPartName = "قرشان";
                    Arabic310CurrencyPartName = "قروش";
                    Arabic1199CurrencyPartName = "قرشاً";
                    PartPrecision = 3;
                    IsCurrencyPartNameFeminine = false;
                    break;
                case Currencies.USA:
                    CurrencyID = 2;
                    CurrencyCode = "$";
                    IsCurrencyNameFeminine = false;
                    EnglishCurrencyName = "$";
                    EnglishPluralCurrencyName = "$";
                    EnglishCurrencyPartName = "Sent";
                    EnglishPluralCurrencyPartName = "Sent";
                    Arabic1CurrencyName = "دولار";
                    Arabic2CurrencyName = "دولاران";
                    Arabic310CurrencyName = "دولارات";
                    Arabic1199CurrencyName = "دولاراً";
                    Arabic1CurrencyPartName = "سنت";
                    Arabic2CurrencyPartName = "سنتان";
                    Arabic310CurrencyPartName = "سنت";
                    Arabic1199CurrencyPartName = "سنتا";
                    PartPrecision = 3;
                    IsCurrencyPartNameFeminine = false;
                    break;
                case Currencies.Syria:
                    CurrencyID = 0;
                    CurrencyCode = "SYP";
                    IsCurrencyNameFeminine = true;
                    EnglishCurrencyName = "Syrian Pound";
                    EnglishPluralCurrencyName = "Syrian Pounds";
                    EnglishCurrencyPartName = "Piaster";
                    EnglishPluralCurrencyPartName = "Piasteres";
                    Arabic1CurrencyName = "ليرة سورية";
                    Arabic2CurrencyName = "ليرتان سوريتان";
                    Arabic310CurrencyName = "ليرات سورية";
                    Arabic1199CurrencyName = "ليرة سورية";
                    Arabic1CurrencyPartName = "قرش";
                    Arabic2CurrencyPartName = "قرشان";
                    Arabic310CurrencyPartName = "قروش";
                    Arabic1199CurrencyPartName = "قرشاً";
                    PartPrecision = 3;
                    IsCurrencyPartNameFeminine = false;
                    break;
                
                //case Currencies.UAE:
                //    CurrencyID = 11;
                //    CurrencyCode = "AED";
                //    IsCurrencyNameFeminine = false;
                //    EnglishCurrencyName = "UAE Dirham";
                //    EnglishPluralCurrencyName = "UAE Dirhams";
                //    EnglishCurrencyPartName = "Fils";
                //    EnglishPluralCurrencyPartName = "Fils";
                //    Arabic1CurrencyName = "درهم إماراتي";
                //    Arabic2CurrencyName = "درهمان إماراتيان";
                //    Arabic310CurrencyName = "دراهم إماراتية";
                //    Arabic1199CurrencyName = "درهماً إماراتياً";
                //    Arabic1CurrencyPartName = "فلس";
                //    Arabic2CurrencyPartName = "فلسان";
                //    Arabic310CurrencyPartName = "فلوس";
                //    Arabic1199CurrencyPartName = "فلساً";
                //    PartPrecision = 2;
                //    IsCurrencyPartNameFeminine = false;
                //    break;

                case Currencies.SaudiArabia:
                    CurrencyID = 3;
                    CurrencyCode = "SAR";
                    IsCurrencyNameFeminine = false;
                    EnglishCurrencyName = "Saudi Riyal";
                    EnglishPluralCurrencyName = "Saudi Riyals";
                    EnglishCurrencyPartName = "Halala";
                    EnglishPluralCurrencyPartName = "Halalas";
                    Arabic1CurrencyName = "ريال سعودي";
                    Arabic2CurrencyName = "ريالان سعوديان";
                    Arabic310CurrencyName = "ريالات سعودية";
                    Arabic1199CurrencyName = "ريالاً سعودياً";
                    Arabic1CurrencyPartName = "هللة";
                    Arabic2CurrencyPartName = "هللتان";
                    Arabic310CurrencyPartName = "هللات";
                    Arabic1199CurrencyPartName = "هللة";
                    PartPrecision = 3;
                    IsCurrencyPartNameFeminine = true;
                    break;

                //case Currencies.Tunisia:
                //    CurrencyID = 4;
                //    CurrencyCode = "TND";
                //    IsCurrencyNameFeminine = false;
                //    EnglishCurrencyName = "Tunisian Dinar";
                //    EnglishPluralCurrencyName = "Tunisian Dinars";
                //    EnglishCurrencyPartName = "milim";
                //    EnglishPluralCurrencyPartName = "millimes";
                //    Arabic1CurrencyName = "درهم إماراتي";
                //    Arabic2CurrencyName = "درهمان إماراتيان";
                //    Arabic310CurrencyName = "دراهم إماراتية";
                //    Arabic1199CurrencyName = "درهماً إماراتياً";
                //    Arabic1CurrencyPartName = "فلس";
                //    Arabic2CurrencyPartName = "فلسان";
                //    Arabic310CurrencyPartName = "فلوس";
                //    Arabic1199CurrencyPartName = "فلساً";
                //    PartPrecision = 3;
                //    IsCurrencyPartNameFeminine = false;
                //    break;

                case Currencies.Gold:
                    CurrencyID = 4;
                    CurrencyCode = "XAU";
                    IsCurrencyNameFeminine = false;
                    EnglishCurrencyName = "Gram";
                    EnglishPluralCurrencyName = "Grams";
                    EnglishCurrencyPartName = "Milligram";
                    EnglishPluralCurrencyPartName = "Milligrams";
                    Arabic1CurrencyName = "جرام";
                    Arabic2CurrencyName = "جرامان";
                    Arabic310CurrencyName = "جرامات";
                    Arabic1199CurrencyName = "جراماً";
                    Arabic1CurrencyPartName = "ملجرام";
                    Arabic2CurrencyPartName = "ملجرامان";
                    Arabic310CurrencyPartName = "ملجرامات";
                    Arabic1199CurrencyPartName = "ملجراماً";
                    PartPrecision =3;
                    IsCurrencyPartNameFeminine = false;
                    break;

            }
        }

        public CurrencyInfo(int Currency_Id)
        {
            switch (Currency_Id)
            {
                case 1:
                    CurrencyID = 1;
                    CurrencyCode = "LE";
                    IsCurrencyNameFeminine = true;
                    EnglishCurrencyName = "Egyptian Pound";
                    EnglishPluralCurrencyName = "Egyptian Pounds";
                    EnglishCurrencyPartName = "Piaster";
                    EnglishPluralCurrencyPartName = "Piasteres";
                    Arabic1CurrencyName = "جنيه مصرى";
                    Arabic2CurrencyName = "جنيهان مصرى";
                    //Arabic310CurrencyName = "جنيهات مصرى";
                    //Arabic1199CurrencyName = "جنيهان مصرى";
                    Arabic310CurrencyName = "جنيهات مصرىة";
                    Arabic1199CurrencyName = "جنيهاً مصرياً";
                    Arabic1CurrencyPartName = "قرش";
                    Arabic2CurrencyPartName = "قرشان";
                    Arabic310CurrencyPartName = "قروش";
                    Arabic1199CurrencyPartName = "قرشاً";
                    PartPrecision = 3;
                    IsCurrencyPartNameFeminine = false;
                    break;
                case 2:
                    CurrencyID = 2;
                    CurrencyCode = "$";
                    IsCurrencyNameFeminine = false;
                    EnglishCurrencyName = "$";
                    EnglishPluralCurrencyName = "$";
                    EnglishCurrencyPartName = "Cent";
                    EnglishPluralCurrencyPartName = "Cents";
                    Arabic1CurrencyName = "دولار";
                    Arabic2CurrencyName = "دولاران";
                    Arabic310CurrencyName = "دولارات";
                    Arabic1199CurrencyName = "دولاراً";
                    Arabic1CurrencyPartName = "سنت";
                    Arabic2CurrencyPartName = "سنتان";
                    Arabic310CurrencyPartName = "سنت";
                    Arabic1199CurrencyPartName = "سنتا";
                    PartPrecision = 3;
                    IsCurrencyPartNameFeminine = false;
                    break;
                case 3:
                    CurrencyID = 0;
                    CurrencyCode = "EUR";
                    IsCurrencyNameFeminine = true;
                    EnglishCurrencyName = "EUR";
                    EnglishPluralCurrencyName = "EUR";
                    EnglishCurrencyPartName = "Piaster";
                    EnglishPluralCurrencyPartName = "Piasteres";
                    Arabic1CurrencyName = "يورو";
                    Arabic2CurrencyName = "يورو";
                    Arabic310CurrencyName = "يورو";
                    Arabic1199CurrencyName = "يورو";
                    Arabic1CurrencyPartName = "Cent";
                    Arabic2CurrencyPartName = "Cents";
                    Arabic310CurrencyPartName = "Cents";
                    Arabic1199CurrencyPartName = "Cent";
                    PartPrecision =3;
                    IsCurrencyPartNameFeminine = false;
                    break;

              

                case 4:
                    CurrencyID = 4;
                    CurrencyCode = "SAR";
                    IsCurrencyNameFeminine = false;
                    EnglishCurrencyName = "Saudi Riyal";
                    EnglishPluralCurrencyName = "Saudi Riyals";
                    EnglishCurrencyPartName = "Halala";
                    EnglishPluralCurrencyPartName = "Halalas";
                    Arabic1CurrencyName = "ريال سعودي";
                    Arabic2CurrencyName = "ريالان سعوديان";
                    Arabic310CurrencyName = "ريالات سعودية";
                    Arabic1199CurrencyName = "ريالاً سعودياً";
                    Arabic1CurrencyPartName = "هللة";
                    Arabic2CurrencyPartName = "هللتان";
                    Arabic310CurrencyPartName = "هللات";
                    Arabic1199CurrencyPartName = "هللة";
                    PartPrecision =3;
                    IsCurrencyPartNameFeminine = true;
                    break;

               

                
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Currency ID
        /// </summary>
        public int CurrencyID { get; set; }

        /// <summary>
        /// Standard Code
        /// Syrian Pound: SYP
        /// UAE Dirham: AED
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Is the currency name feminine ( Mua'anath مؤنث)
        /// ليرة سورية : مؤنث = true
        /// درهم : مذكر = false
        /// </summary>
        public Boolean IsCurrencyNameFeminine { get; set; }

        /// <summary>
        /// English Currency Name for single use
        /// Syrian Pound
        /// UAE Dirham
        /// </summary>
        public string EnglishCurrencyName { get; set; }

        /// <summary>
        /// English Plural Currency Name for Numbers over 1
        /// Syrian Pounds
        /// UAE Dirhams
        /// </summary>
        public string EnglishPluralCurrencyName { get; set; }

        /// <summary>
        /// Arabic Currency Name for 1 unit only
        /// ليرة سورية
        /// درهم إماراتي
        /// </summary>
        public string Arabic1CurrencyName { get; set; }

        /// <summary>
        /// Arabic Currency Name for 2 units only
        /// ليرتان سوريتان
        /// درهمان إماراتيان
        /// </summary>
        public string Arabic2CurrencyName { get; set; }

        /// <summary>
        /// Arabic Currency Name for 3 to 10 units
        /// خمس ليرات سورية
        /// خمسة دراهم إماراتية
        /// </summary>
        public string Arabic310CurrencyName { get; set; }

        /// <summary>
        /// Arabic Currency Name for 11 to 99 units
        /// خمس و سبعون ليرةً سوريةً
        /// خمسة و سبعون درهماً إماراتياً
        /// </summary>
        public string Arabic1199CurrencyName { get; set; }

        /// <summary>
        /// Decimal Part Precision
        /// for Syrian Pounds: 2 ( 1 SP = 100 parts)
        /// for Tunisian Dinars: 3 ( 1 TND = 1000 parts)
        /// </summary>
        public Byte PartPrecision { get; set; }

        /// <summary>
        /// Is the currency part name feminine ( Mua'anath مؤنث)
        /// هللة : مؤنث = true
        /// قرش : مذكر = false
        /// </summary>
        public Boolean IsCurrencyPartNameFeminine { get; set; }

        /// <summary>
        /// English Currency Part Name for single use
        /// Piaster
        /// Fils
        /// </summary>
        public string EnglishCurrencyPartName { get; set; }

        /// <summary>
        /// English Currency Part Name for Plural
        /// Piasters
        /// Fils
        /// </summary>
        public string EnglishPluralCurrencyPartName { get; set; }

        /// <summary>
        /// Arabic Currency Part Name for 1 unit only
        /// قرش
        /// هللة
        /// </summary>
        public string Arabic1CurrencyPartName { get; set; }

        /// <summary>
        /// Arabic Currency Part Name for 2 unit only
        /// قرشان
        /// هللتان
        /// </summary>
        public string Arabic2CurrencyPartName { get; set; }

        /// <summary>
        /// Arabic Currency Part Name for 3 to 10 units
        /// قروش
        /// هللات
        /// </summary>
        public string Arabic310CurrencyPartName { get; set; }

        /// <summary>
        /// Arabic Currency Part Name for 11 to 99 units
        /// قرشاً
        /// هللةً
        /// </summary>
        public string Arabic1199CurrencyPartName { get; set; }
        #endregion
    }
}
