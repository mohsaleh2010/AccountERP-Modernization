using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.Models
{
    // 🟢 يقابل PrintItemsBarCodeSettingFor_User.GetsavedData/SaveSetData/LoadDefaultData_Saved (WinForms،
    // Store_LIB\ItemsList_Store_\BarCode\) - يحتفظ بنفس السلوك بالضبط: صف واحد فقط دائماً بمفتاح ثابت
    // Item_Id="0" (إعداد عام واحد للشركة، رغم اسم الجدول) فى Item_BarCodeSetting، وصف مكافئ اختيارى فى
    // Item_BarCodeSettingDefault يُحمَّل عند عدم وجود إعداد محفوظ. الأصل لا يملأ أعمدة التدقيق
    // (User_Id/Branch_Id/Edit_User_Id/EditOperationDate) إطلاقاً رغم وجودها فى الجدولين - هذه فجوة فى
    // الأصل لا تُنقَل، طبقاً للقاعدة القياسية الحالية (راجع الذاكرة audit-columns-user-branch-id).
    public class BarCodeSettingCLS
    {
        private const string SingletonItemId = "0";

        public Item_BarCodeSetting GetCurrent(ref operations opp)
        {
            return opp.db.Item_BarCodeSetting.AsNoTracking()
                .FirstOrDefault(r => r.Item_Id == SingletonItemId);
        }

        public Item_BarCodeSettingDefault GetDefault(ref operations opp)
        {
            return opp.db.Item_BarCodeSettingDefault.AsNoTracking()
                .FirstOrDefault(r => r.Item_Id == SingletonItemId);
        }

        public void SaveCurrent(ref operations opp, Item_BarCodeSetting model)
        {
            var existing = opp.db.Item_BarCodeSetting.Where(r => r.Item_Id == SingletonItemId);
            opp.db.Item_BarCodeSetting.RemoveRange(existing);

            model.Id = CallSnowflakeIdGenerate.GenerateNewId();
            model.Item_Id = SingletonItemId;
            model.User_Id = Common.User_Id;
            model.Branch_Id = Common.Branch_Id;
            model.OperationDate = DateTime.Now;
            model.Insert_ServerDateTime = DateTime.Now;
            opp.db.Item_BarCodeSetting.Add(model);
            opp.db.SaveChanges();
        }

        public void SaveDefault(ref operations opp, Item_BarCodeSettingDefault model)
        {
            var existing = opp.db.Item_BarCodeSettingDefault.Where(r => r.Item_Id == SingletonItemId);
            opp.db.Item_BarCodeSettingDefault.RemoveRange(existing);

            model.Id = CallSnowflakeIdGenerate.GenerateNewId();
            model.Item_Id = SingletonItemId;
            model.User_Id = Common.User_Id;
            model.Branch_Id = Common.Branch_Id;
            model.OperationDate = DateTime.Now;
            model.Insert_ServerDateTime = DateTime.Now;
            opp.db.Item_BarCodeSettingDefault.Add(model);
            opp.db.SaveChanges();
        }
    }
}
