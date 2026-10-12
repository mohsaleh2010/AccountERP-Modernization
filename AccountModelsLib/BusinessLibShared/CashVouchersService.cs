using System;
using System.Data;
using System.Linq;
using AccountModelsLib.Models;

namespace AccountModelsLib
{
    /// <summary>
    /// إذونات النقدية (سندات القبض والصرف) لصفحات Razor. تلفّ المحرك القديم `Money_Transaction_BoxOrderReportCLS` كما هو بلا أى تعديل عليه
    /// (الحفظ/التعديل/الحذف/القيد/الأرصدة كلها من دواله)، وتضيف فقط ما كان ساكناً فى مشروع WinForms ولا يمكن مرجعته من Razor:
    /// قائمة الصناديق بصلاحياتها، ترقيم الإذن، عامل العملة، رصيد الصندوق، وقراءة الإذن للعرض.
    /// خطة التحويل: .claude/docs/cash-vouchers-razor-plan.md
    /// </summary>
    public class CashVouchersService
    {
        /// <summary>
        /// الصناديق (Is_Bank=0) بعملاتها. لو IS_Used_SandoukWithPermission=1 تُصفَّى بصلاحيات المستخدم كما فى SandOuk_AccountList.
        /// branchId (اختيارى، 0=بلا تصفية): يقصر القائمة على صناديق هذا الفرع (+ أى صندوق مشترك Branch_Id=-1 إن وُجد) - يُستخدَم فى
        /// شاشات إذن النقدية لمنع تعارض الأسماء بين فروع مختلفة (راجع branch-cash-boxes-plan.md بند 4)؛ شاشات التقارير
        /// (كشف/إجمالى الصناديق) تستمر بلا تصفية فرع.
        /// </summary>
        public DataTable GetBoxes(long userId, long branchId = 0)
        {
            var sql = "SELECT Currency.Currency_Name, Currency.Currency_Name_EN, Banks.* FROM Banks INNER JOIN Currency ON Banks.currency_Id = Currency.Value ";
            if (Common.IS_Used_SandoukWithPermission == 1)
                sql += "INNER JOIN SysConfigBanksPERMATIONS ON Banks.id = SysConfigBanksPERMATIONS.Account_Branks_Id ";
            sql += "WHERE Is_Bank = 0 ";
            if (Common.IS_Used_SandoukWithPermission == 1)
                sql += "AND SysConfigBanksPERMATIONS.USER_DETAILES_Id = " + userId + " ";
            if (branchId > 0)
                sql += "AND (Banks.Branch_Id = " + branchId + " OR Banks.Branch_Id = -1) ";
            return AddBranchLabel(new operations().return_DataTable(sql));
        }

        /// <summary>
        /// يضيف عمود Bank_Display: اسم الصندوق، ومعه « — اسم الفرع» فقط حين تضم القائمة صناديق من أكثر من فرع (يمنع التباس تكرار الاسم بين الفروع).
        /// للعرض فقط؛ الحفظ يستمر باسم الصندوق الأصلى Bank_Name (branch-cash-boxes-plan.md بند 4).
        /// </summary>
        private static DataTable AddBranchLabel(DataTable dt)
        {
            if (!dt.Columns.Contains("Bank_Display")) dt.Columns.Add("Bank_Display", typeof(string));
            var branchIds = dt.Rows.Cast<DataRow>().Where(r => r["Branch_Id"] != DBNull.Value).Select(r => Convert.ToInt64(r["Branch_Id"])).Where(id => id > 0).Distinct().ToList();
            var names = new System.Collections.Generic.Dictionary<long, string>();
            if (branchIds.Count > 1)
                foreach (DataRow b in new operations().return_DataTable("SELECT Id, Branch_Name FROM Account_Branches").Rows)
                    names[Convert.ToInt64(b["Id"])] = b["Branch_Name"]?.ToString() ?? "";
            foreach (DataRow r in dt.Rows)
            {
                string name = r["Bank_Name"]?.ToString() ?? "";
                if (branchIds.Count > 1 && r["Branch_Id"] != DBNull.Value && names.TryGetValue(Convert.ToInt64(r["Branch_Id"]), out var bn) && bn.Length > 0)
                    name += " — " + bn;
                r["Bank_Display"] = name;
            }
            return dt;
        }

        /// <summary>رقم الإذن التالى (لكل نوع وفترة مالية) — Make_New_Order_No الأصلية.</summary>
        public long NextOrderNo(int transactionType, DateTime date, long boxId)
        {
            int excp = 0;
            return new Money_Transaction_BoxOrderReportCLS().Make_New_Order_No(ref excp, transactionType, date, boxId);
        }

        /// <summary>رصيد الصندوق الفعلى محسوباً من الحركات (لا من العمود المخزَّن).</summary>
        public decimal BoxBalance(long boxId) => new Money_Transaction_BoxOrderReportCLS().GetBank_Balance(boxId, true);

        /// <summary>عامل التحويل بين عملة القيد الرئيسية وعملة الصندوق (1 إن تساوتا) — يقابل GetCurrencyData فى الفورم.</summary>
        public decimal GetFactor(long boxCurrencyId)
        {
            if (boxCurrencyId == Common.Entry_Currency_Id || boxCurrencyId <= 0) return 1;
            try { return Common.RoundMe(new CurrencyFactorsCLS().GetFactors(Common.Entry_Currency_Id, boxCurrencyId)); }
            catch { return 1; }
        }

        public string Tafkeet(decimal value, long currencyId)
        {
            try { return TafkeetProj.NumberToText.GetText(value, currencyId); }
            catch { return ""; }
        }

        /// <summary>
        /// إذن طرف واحد (عميل/مورد ± العكس) عبر Make_Transaction_BanksBox_FromInvoices (نفس مسار OrderMoney_Transaction.MakeOrder)، داخل معاملة واحدة.
        /// transactionType: 1 استلام، 2 صرف. reverse=1: الطرف الآخر (استلام من مورد / صرف لعميل).
        /// يعيد رقم الإذن (Order_Money.Id) وسطر الصندوق (Money_Transaction_Box.Id)؛ يرمى استثناء عند الفشل مع التراجع.
        /// </summary>
        public (long OrderId, long BoxLineId, long OrderNo) SavePartyVoucher(int transactionType, int reverse, long boxId, DateTime date, long orderNo, string manName, decimal cashValue,
                                                                             long partyId, decimal factor, int docType = 0, long docId = 0)
        {
            if (cashValue <= 0) throw new InvalidOperationException("Value must be greater than zero");
            if (partyId <= 0) throw new InvalidOperationException("Party is required");

            var opp = new operations(1);
            var engine = new Money_Transaction_BoxOrderReportCLS();
            int excp = 0;
            string msg = "";
            try
            {
                if (transactionType == 2 && Common.IsBoxBalanceAllowSarfAny == 0)
                {
                    var balance = engine.GetBank_Balance(ref opp, boxId, true);
                    if (cashValue > balance) throw new InvalidOperationException("Box balance does not allow this payment");
                }

                // رقم الإذن الحر: Check_Order_NoFound تزيد الرقم حتى يخلو (كالأصل عند التكرار نمنع بدلاً من الاستبدال الصامت).
                long no = orderNo;
                if (no <= 0) no = engine.Make_New_Order_No(ref excp, ref opp, transactionType, date, boxId);
                else
                {
                    var probe = no;
                    engine.Check_Order_NoFound(ref excp, ref opp, ref probe, transactionType, date, boxId);
                    if (probe != no) throw new InvalidOperationException("Order number already exists");
                }

                var bankRow = opp.return_DataTable("SELECT Last_Balance FROM Banks WHERE Id = " + boxId);
                if (bankRow.Rows.Count == 0) throw new InvalidOperationException("Box not found");
                decimal boxBalanceStored = 0;
                decimal.TryParse(bankRow.Rows[0]["Last_Balance"]?.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out boxBalanceStored);

                // ربط الإذن بمستند (فاتورة/مرتجع مبيعات أو مشتريات): المفتاح فى الأصل = نوع العملية (DataDictionaryProcessesListsId) + رقم المستند (Reference_Type_Table_Id).
                engine.Invoice_ID = docId;
                engine.Invoice_Type = docType;
                engine.Notes = manName ?? "";
                engine.Factor = factor <= 0 ? 1 : factor;

                var vm = new Money_Transaction_BoxVM
                {
                    Order_Mony_Id = 0,
                    Money_Transaction_Box_ID = 0,
                    Transaction_Type = transactionType,
                    Order_No = no.ToString(),
                    Box_Id = boxId,
                    Box_Balance = boxBalanceStored,
                    Cach_Value = cashValue,
                    Order_Date = date,
                    Man_Name = manName ?? "",
                    Currency_Id = 1,   // كالأصل: عملة الإذن 1 وعملة الصندوق تُقرأ من Banks داخل المحرك
                    Customer_ID_Supplier = partyId,
                    Reverse_Order = reverse,
                    Entry_No = 0,
                    Reference_Type_Table_Id = docId,
                    DataDictionaryProcessesListsId = docType,
                    Invoice_Id = docId
                };
                engine.Make_Transaction_BanksBox_FromInvoices(ref excp, ref msg, ref opp, vm);
                if (excp != 0 || vm.Order_Mony_Id <= 0)
                    throw new InvalidOperationException(string.IsNullOrWhiteSpace(msg) ? "Voucher was not saved" : msg);
                opp.Commit();
                return (vm.Order_Mony_Id, vm.Money_Transaction_Box_ID, no);
            }
            catch
            {
                try { opp.Rollback(); } catch { }
                throw;
            }
        }

        // ================= الإذن متعدد الأسطر (New_OrderMoney_SarfNew) =================

        /// <summary>مراكز التكلفة المتاحة لحساب — نفس قاعدة Cost_Centers_List.Get_Data(false, حساب, علم القبول): مراكزه المخصَّصة (الطرفية)، وإلا كل الطرفية إن كان الحساب يقبل مراكز، وإلا «لا يوجد» (0).</summary>
        public DataTable GetCostCenters(long subAccountId, int acceptCostCenterFlag, string noneText)
        {
            var opp = new operations();
            const string leaf = "Cost_Center_Tree.ID NOT IN (SELECT Parent_Id FROM Cost_Center_Tree AS Cost_Center_Tree_1)";
            var dt = opp.return_DataTable(
                "SELECT DISTINCT Cost_Center_Tree.Id, Cost_Center_Tree.Name FROM Cost_Center_ForAccounts INNER JOIN Cost_Center_Tree ON Cost_Center_ForAccounts.Cost_Center_Id = Cost_Center_Tree.ID " +
                "WHERE (" + leaf + ") AND Cost_Center_ForAccounts.Submain_Account_Id = " + subAccountId);
            if (dt.Rows.Count == 0 && acceptCostCenterFlag == 1)
                dt = opp.return_DataTable("SELECT Cost_Center_Tree.Id, Cost_Center_Tree.Name FROM Cost_Center_Tree WHERE (" + leaf + ")");
            if (dt.Rows.Count == 0)
            {
                dt = new DataTable();
                dt.Columns.Add("Id", typeof(long));
                dt.Columns.Add("Name", typeof(string));
                dt.Rows.Add(0L, noneText);
            }
            return dt;
        }

        /// <summary>بيانات الحساب اللازمة لسطر الإذن (الكود، عملته، قبوله لمراكز التكلفة).</summary>
        public (string Code, long CurrencyId, int AcceptCostCenter) AccountInfo(long subAccountId)
        {
            var dt = new operations().return_DataTable("SELECT SubAccount_NO, Currency_Id, Accept_Cost_Center_Flage FROM Submain_Account WHERE Id = " + subAccountId);
            if (dt.Rows.Count == 0) return ("", 1, 0);
            var r = dt.Rows[0];
            long.TryParse(r["Currency_Id"]?.ToString(), out var cur);
            int.TryParse(r["Accept_Cost_Center_Flage"]?.ToString(), out var acc);
            return (r["SubAccount_NO"]?.ToString() ?? "", cur == 0 ? 1 : cur, acc);
        }

        /// <summary>جدول أسطر فارغ بنفس بنية Money_Transaction_Box (يستعمله المحرك مباشرة).</summary>
        public DataTable NewLinesTable() => Money_Transaction_BoxOrderReportCLS.MakeEmpty_OrderMoney();

        public void AddLine(DataTable dt, int transactionType, long subAccountId, int accountType, string accountName, string accountCode, long accountCurrencyId, decimal amount,
                            long costCenterId, string costCenterName, decimal factor, string notes, decimal balanceBefore, decimal balanceAfter)
        {
            var dr = dt.NewRow();
            dr["Id_Test"] = 0;
            dr["Order_Mony_Id"] = 0;
            dr["Id"] = 0;
            dr["Import_Value"] = transactionType == 1 ? amount : 0;
            dr["Export_Value"] = transactionType == 1 ? 0 : amount;
            dr["Transaction_Value"] = amount;
            dr["Transaction_Type"] = transactionType;
            dr["Account_Type"] = accountType;
            dr["SubAccount_Id"] = subAccountId;
            dr["Account_Name"] = accountName;
            dr["Account_Code"] = accountCode;
            dr["Currency_Id"] = accountCurrencyId;
            dr["Cost_Center"] = costCenterName;
            dr["Account_Cost_Center_Tree_Id"] = costCenterId;
            dr["CostCenter_Name"] = costCenterName;
            dr["Factor"] = factor;
            dr["Notes"] = notes ?? "";
            dr["Balance_Before"] = balanceBefore;
            dr["Balance_After"] = balanceAfter;
            dt.Rows.Add(dr);
        }

        /// <summary>
        /// إصدار إذن متعدد الأسطر عبر Make_Transaction_BanksBox_NewSarf ثم Set_BankBalanceDay ثم Make_Entry_For_AccountsSandouk — تسلسل New_OrderMoney_SarfNew.SaveNewOrderFinally
        /// بالضبط، داخل معاملة واحدة. الصرف لا يتجاوز رصيد الصندوق (الفحص فى هذا الفورم غير مشروط بـIsBoxBalanceAllowSarfAny كالأصل).
        /// </summary>
        public (long OrderId, long OrderNo, string Message) SaveMultiVoucher(int transactionType, long boxId, string boxName, long boxCurrencyId, string boxCurrencyName, DateTime date, long orderNo,
                                                                            string manName, string orderNotes, decimal factor, DataTable lines)
            => SaveMultiVoucherCore(0, transactionType, boxId, boxName, boxCurrencyId, boxCurrencyName, date, orderNo, manName, orderNotes, factor, lines);

        /// <summary>
        /// تعديل إذن متعدد الأسطر محفوظ: حذف الإذن القديم ثم إصدار الجديد بنفس الرقم داخل معاملة واحدة (تتراجع كلها عند أى فشل). يتغير المعرّف الداخلى للإذن،
        /// لذلك يُمنع التعديل إذا كان الإذن مرتبطاً بعربون طلبية أو بمستند (يُحذف/يُعدَّل من مصدره).
        /// </summary>
        public (long OrderId, long OrderNo, string Message) EditMultiVoucher(long orderId, int transactionType, long boxId, string boxName, long boxCurrencyId, string boxCurrencyName, DateTime date, long orderNo,
                                                                            string manName, string orderNotes, decimal factor, DataTable lines)
        {
            if (orderId <= 0) throw new InvalidOperationException("Voucher is required");
            if (HasAmbiguousLineIds(orderId)) throw new InvalidOperationException("Line ids are not unique; edit blocked");
            if (!IsMultiEngineVoucher(orderId)) throw new InvalidOperationException("Voucher was not issued by the multi-line screen; edit blocked");
            if (IsVoucherLinked(orderId)) throw new InvalidOperationException("Voucher is linked to an order deposit or a document; edit blocked");
            return SaveMultiVoucherCore(orderId, transactionType, boxId, boxName, boxCurrencyId, boxCurrencyName, date, orderNo, manName, orderNotes, factor, lines);
        }

        /// <summary>
        /// هل أُصدر الإذن من محرك الإذن متعدد الأسطر (Make_Transaction_BanksBox_NewSarf بعلامة IsMoreTheOneBoxShow=3)؟ أذون الطرف الواحد (Make_Transaction_BanksBox_FromInvoices) والأذون القديمة
        /// لا تُعدَّل بهذا المسار لأن إعادة إصدارها بمحرك آخر تغيّر طريقة حفظها؛ يُسمح لها بالحذف فقط.
        /// </summary>
        public bool IsMultiEngineVoucher(long orderId)
        {
            var dt = new operations().return_DataTable("SELECT COUNT(*) FROM Order_Money WHERE Id = " + orderId + " AND ISNULL(IsMoreTheOneBoxShow,0) = 3");
            return Convert.ToInt32(dt.Rows[0][0]) > 0;
        }

        /// <summary>هل الإذن مرتبط بعربون طلبية أو بمستند فاتورة/مرتجع؟</summary>
        public bool IsVoucherLinked(long orderId)
        {
            var dt = new operations().return_DataTable(
                "SELECT (SELECT COUNT(*) FROM Customer_Order_Deposits WHERE Order_Money_Id = " + orderId + ") + " +
                "(SELECT COUNT(*) FROM Money_Transaction_Box WHERE Order_Mony_Id = " + orderId + " AND ISNULL(Reference_Type_Table_Id,0) > 0)");
            return Convert.ToInt32(dt.Rows[0][0]) > 0;
        }

        private (long OrderId, long OrderNo, string Message) SaveMultiVoucherCore(long replaceOrderId, int transactionType, long boxId, string boxName, long boxCurrencyId, string boxCurrencyName, DateTime date, long orderNo,
                                                                            string manName, string orderNotes, decimal factor, DataTable lines)
        {
            if (lines == null || lines.Rows.Count == 0) throw new InvalidOperationException("At least one line is required");
            var opp = new operations(1);
            var engine = new Money_Transaction_BoxOrderReportCLS();
            int excp = 0;
            string msg = "";
            try
            {
                if (replaceOrderId > 0)
                {
                    int dExcp = 0; string dMsg = ""; long entryNo = 0;
                    engine.Delete_Transaction_BanksBox_NewSarf_New(ref dExcp, ref dMsg, ref opp, replaceOrderId, ref entryNo);
                    if (dExcp != 0) throw new InvalidOperationException(string.IsNullOrWhiteSpace(dMsg) ? "Delete failed" : dMsg);
                }
                var total = lines.Rows.Cast<DataRow>().Sum(r => Convert.ToDecimal(r["Transaction_Value"]));
                var boxBalance = engine.GetBank_Balance(ref opp, boxId, true);
                if (transactionType == 2 && total > boxBalance) throw new InvalidOperationException("Box balance does not allow this payment");

                long no = orderNo;
                var probe = no;
                var found = engine.Check_Order_NoFound(ref excp, ref opp, ref probe, transactionType, date, boxId);
                if (found || no <= 0 || probe != no) throw new InvalidOperationException("Order number already exists");

                long orderId = 0;
                var tafkeet = Tafkeet(total, boxCurrencyId);
                engine.Make_Transaction_BanksBox_NewSarf(ref excp, ref msg, ref opp, ref orderId, boxName, lines, transactionType, 0, no.ToString(), boxId, boxBalance, date, manName ?? "",
                    boxCurrencyId, tafkeet, orderNotes ?? "", factor <= 0 ? 1 : factor, boxCurrencyId, boxCurrencyName, 0, 0, 0, 3);
                if (excp != 0 || orderId <= 0) throw new InvalidOperationException(string.IsNullOrWhiteSpace(msg) ? "Voucher was not saved" : msg);

                engine.Set_BankBalanceDay(ref opp, date.Date, boxId, boxBalance, total, transactionType);

                int excp1 = 0; string msg1 = "";
                var post = new Accounts_Entry_POST_CLS();
                post.Make_Entry_For_AccountsSandouk(ref excp1, ref msg1, ref opp, 1, Common.Branch_Id, new DateTime(DateTime.Now.Year, 1, 1), new DateTime(DateTime.Now.Year, 12, 31),
                    boxCurrencyId, boxCurrencyName, factor <= 0 ? 1 : factor, orderId, 0);
                if (excp1 != 0) throw new InvalidOperationException(string.IsNullOrWhiteSpace(msg1) ? "Journal entry failed" : msg1);

                opp.Commit();
                return (orderId, no, msg + msg1);
            }
            catch
            {
                try { opp.Rollback(); } catch { }
                throw;
            }
        }

        /// <summary>
        /// حماية الحذف: Money_Transaction_Box.Id غير فريد فى القاعدة، ودالة الحذف الأصلية تحذف السطر ومرجع حركة الطرف بمعرّف السطر وحده،
        /// فلو تكرر معرّف أحد أسطر الإذن مع سطر إذن آخر لأصاب الحذف بيانات غيره. نمنع الحذف هنا حتى يُصلَح توليد المعرّفات (بند 32 فى backlog).
        /// </summary>
        public bool HasAmbiguousLineIds(long orderId)
        {
            var dt = new operations().return_DataTable(
                "SELECT COUNT(*) FROM Money_Transaction_Box m WHERE m.Order_Mony_Id = " + orderId + " AND (SELECT COUNT(*) FROM Money_Transaction_Box x WHERE x.Id = m.Id) > 1");
            return Convert.ToInt32(dt.Rows[0][0]) > 0;
        }

        /// <summary>حذف إذن بمحرك Delete_Transaction_BanksBox_NewSarf_New (يعكس الصندوق والطرف ويحذف القيد) داخل معاملة. يرمى استثناء عند الفشل أو التباس المعرّفات.</summary>
        public string DeleteVoucher(long orderId)
        {
            if (HasAmbiguousLineIds(orderId)) throw new InvalidOperationException("Line ids are not unique; delete blocked");
            // عربون طلبية مرتبط بالإذن: يُمنع الحذف لو سُوِّي جزء منه على فاتورة، وإلا يُحذف ربط العربون مع الإذن فى نفس المعاملة (بدون ترك عربون يتيم).
            var dep = new operations().return_DataTable("SELECT COUNT(*), ISNULL(SUM(ISNULL(Consumed_Amount,0)),0) FROM Customer_Order_Deposits WHERE Order_Money_Id = " + orderId);
            if (Convert.ToDecimal(dep.Rows[0][1]) > 0) throw new InvalidOperationException("Deposit already applied to an invoice; delete blocked");
            var hasDeposit = Convert.ToInt32(dep.Rows[0][0]) > 0;
            var opp = new operations(1);
            int excp = 0; string msg = ""; long entryNo = 0;
            try
            {
                // فك ربط العربون أولاً: محرك الحذف يرفض أى إذن ما زال مرتبطاً بعربون (حماية شاشات الديسك توب)
                if (hasDeposit)
                {
                    int dExcp = 0;
                    opp.delete_me("DELETE FROM Customer_Order_Deposits WHERE Order_Money_Id = " + orderId, ref dExcp);
                    if (dExcp != 0) throw new InvalidOperationException("Deposit link delete failed");
                }
                new Money_Transaction_BoxOrderReportCLS().Delete_Transaction_BanksBox_NewSarf_New(ref excp, ref msg, ref opp, orderId, ref entryNo);
                if (excp != 0) throw new InvalidOperationException(string.IsNullOrWhiteSpace(msg) ? "Delete failed" : msg);
                opp.Commit();
                return msg;
            }
            catch
            {
                try { opp.Rollback(); } catch { }
                throw;
            }
        }

        // ================= تحويل نقدية بين صندوقين (Money_Between_BoxTar7eelFrm) =================

        /// <summary>كل الصناديق بلا فلترة صلاحيات (وجهة التحويل) — Get_Sandouk_TOTransfeer الأصلية.</summary>
        public DataTable GetAllBoxes()
        {
            return AddBranchLabel(new operations().return_DataTable("SELECT Currency.Currency_Name, Currency.Currency_Name_EN, Banks.* FROM Banks INNER JOIN Currency ON Banks.currency_Id = Currency.Value WHERE Is_Bank = 0"));
        }

        /// <summary>
        /// تحويل مبلغ من صندوق لآخر عبر MakeOrderBetweenTwo_Sandouk كما هى (إذنا صرف واستلام مربوطان + القيد) داخل معاملة واحدة.
        /// equivalentValue = عامل تحويل عملة الصندوق المصدر إلى عملة الصندوق الوجهة (1 لو تساوت العملتان). يعيد رقم إذن الصرف.
        /// </summary>
        public (long OrderIdFrom, long OrderNoFrom, long OrderNoTo) SaveTransfer(long boxFrom, string boxFromName, long currencyFrom, long boxTo, string boxToName, long currencyTo, DateTime date,
                                                                                decimal cashValue, decimal equivalentValue, string manName, string orderNotes)
        {
            if (boxFrom == boxTo) throw new InvalidOperationException("Cannot transfer to the same box");
            if (cashValue <= 0) throw new InvalidOperationException("Value must be greater than zero");
            var opp = new operations(1);
            var engine = new Money_Transaction_BoxOrderReportCLS();
            int excp = 0; string msg = "";
            try
            {
                var noFrom = engine.Make_New_Order_No(ref excp, ref opp, 2, date, boxFrom);
                var noTo = engine.Make_New_Order_No(ref excp, ref opp, 1, date, boxTo);
                long orderId = 0;
                var eq = equivalentValue <= 0 ? 1 : equivalentValue;
                engine.MakeOrderBetweenTwo_Sandouk(ref excp, ref msg, ref opp, boxToName, ref orderId, date, noFrom.ToString(), boxFrom, currencyFrom, Tafkeet(cashValue, currencyFrom),
                    noTo.ToString(), boxTo, currencyTo, Tafkeet(cashValue * eq, currencyTo), cashValue, eq, manName ?? "", orderNotes ?? "", boxToName, 1, 0, 0, 0, 3);
                if (excp != 0) throw new InvalidOperationException(string.IsNullOrWhiteSpace(msg) ? "Transfer failed" : msg);
                opp.Commit();
                return (orderId, noFrom, noTo);
            }
            catch
            {
                try { opp.Rollback(); } catch { }
                throw;
            }
        }

        /// <summary>عامل تحويل عملة إلى أخرى (GetFactors) — 1 لو تساوتا.</summary>
        public decimal GetPairFactor(long currencyFrom, long currencyTo)
        {
            if (currencyFrom == currencyTo || currencyFrom <= 0 || currencyTo <= 0) return 1;
            try { return Common.RoundMe(new CurrencyFactorsCLS().GetFactors(currencyFrom, currencyTo)); }
            catch { return 1; }
        }

        // ================= قوائم الإذونات (Cash_Money_List / FromCustomers / ToSuppliers / FromAccounts) =================

        /// <summary>
        /// بحث الإذونات: نسخة قراءة واحدة تغطى استعلامات Order_Receipt_Cash الأربعة (Get_OrderCach / FromCustomer / ToSuppliers / ToAccount).
        /// mode: all | customers | suppliers | accounts (أسطر الإذن التى لها طرف من النوع المطلوب). كل السطور المطابقة تظهر (سطر لكل حساب فى الإذن).
        /// </summary>
        public DataTable SearchVouchers(string mode, int transactionType, DateTime? from, DateTime? to, long boxId, string partyLike)
        {
            var sql = new System.Text.StringBuilder();
            sql.Append("SELECT TOP 2000 o.Id AS Order_Id, o.Order_No, o.Order_Date, o.Transaction_type, o.Total_Value AS Order_Total, o.Man_Name, o.Order_Notes, ");
            sql.Append("cur.Currency_Name, b.Bank_Name, ISNULL(m.Transaction_Value,0) AS Line_Value, m.Account_Name, ISNULL(cu.Name, ISNULL(su.Name, s.SubAccount_Name)) AS Party_Name, u.Name AS User_Name ");
            sql.Append("FROM Order_Money o INNER JOIN Money_Transaction_Box m ON o.Id = m.Order_Mony_Id ");
            sql.Append("LEFT JOIN Currency cur ON o.Currency_Id = cur.ID LEFT JOIN Banks b ON m.Bank_Id = b.id LEFT JOIN Submain_Account s ON m.SubAccount_Id = s.Id ");
            sql.Append("LEFT JOIN Customer cu ON m.Customer_Id = cu.Id LEFT JOIN Supplier su ON m.Supplier_Id = su.Id LEFT JOIN USER_DETAILES u ON o.User_Id = u.USER_ID ");
            sql.Append("WHERE o.Order_No <> 0 AND o.Order_No <> -1 ");
            if (mode == "customers") sql.Append("AND m.Customer_Id > 0 ");
            else if (mode == "suppliers") sql.Append("AND m.Supplier_Id > 0 ");
            else if (mode == "accounts") sql.Append("AND m.SubAccount_Id > 0 ");
            if (boxId > 0) sql.Append("AND m.Bank_Id = " + boxId + " ");
            if (from.HasValue) sql.Append("AND CONVERT(DATE, o.Order_Date) >= '" + operations.dateFormat(from.Value.Date) + "' ");
            if (to.HasValue) sql.Append("AND CONVERT(DATE, o.Order_Date) <= '" + operations.dateFormat(to.Value.Date) + "' ");
            if (transactionType > 0) sql.Append("AND o.Transaction_type = " + transactionType + " ");
            if (!string.IsNullOrWhiteSpace(partyLike))
            {
                var p = partyLike.Trim().Replace("'", "''");
                sql.Append("AND (ISNULL(cu.Name,'') LIKE N'%" + p + "%' OR ISNULL(su.Name,'') LIKE N'%" + p + "%' OR ISNULL(s.SubAccount_Name,'') LIKE N'%" + p + "%' OR ISNULL(o.Man_Name,'') LIKE N'%" + p + "%') ");
            }
            sql.Append("ORDER BY o.Id DESC");
            return new operations().return_DataTable(sql.ToString());
        }

        // ================= تقارير حركة الخزينة (قائمة حركة الخزينة والبنوك) =================

        /// <summary>
        /// كشف حركة صندوق ليوم أو لفترة عبر Get_Money_Box_Transaction_Period كما هى: الرصيد الأول ثم الحركات بالرصيد الجارى ثم إجمالى الاستلام/الصرف والرصيد النهائى.
        /// interval=false: يوم from فقط. يعيد جدول الحركات (بلا صفوف الأول/الإجمالى: تُعرض من القيم المرجعة) ورسالة عند عدم وجود بيانات.
        /// </summary>
        public (DataTable Rows, decimal FirstBalance, decimal ExportTotal, decimal ImportTotal, decimal FinalBalance, string Message) BoxStatement(long boxId, bool interval, DateTime from, DateTime to)
        {
            var engine = new Money_Transaction_BoxOrderReportCLS();
            int excp = 0; string msg = "";
            decimal first = 0, exp = 0, imp = 0, final = 0;
            var dt = new DataTable();
            engine.Get_Money_Box_Transaction_Period(ref excp, ref msg, ref dt, boxId, interval, from, to, ref first, ref exp, ref imp, ref final);
            if (excp != 0) return (new DataTable(), first, exp, imp, final, string.IsNullOrWhiteSpace(msg) ? "no data" : msg);
            // الدالة تضيف صف «الرصيد الأول» أولاً وصفى «الإجمالى» و«الرصيد النهائى» آخراً؛ نحذفها ونعرضها من القيم المرجعة.
            if (dt.Rows.Count >= 3)
            {
                dt.Rows.RemoveAt(dt.Rows.Count - 1);
                dt.Rows.RemoveAt(dt.Rows.Count - 1);
                dt.Rows.RemoveAt(0);
            }
            return (dt, first, exp, imp, final, "");
        }

        /// <summary>يومية الصندوق بجدولين: الاستلام (نوع 1) والصرف (نوع 2) — Money_Box_Transaction(type,...) كما فى DaysMoney_Transaction_Box.</summary>
        public (DataTable Receipts, DataTable Payments, decimal FirstBalance, decimal FinalBalance) BoxDays(long boxId, bool interval, DateTime from, DateTime to)
        {
            var engine = new Money_Transaction_BoxOrderReportCLS();
            int excp = 0;
            var imp = engine.Money_Box_Transaction(ref excp, 1, boxId, from, to, interval, false);
            var exp = engine.Money_Box_Transaction(ref excp, 2, boxId, from, to, interval, false);
            var first = engine.Compute_Box_BalanceBeforDataDay(boxId, false, from);
            decimal totalImp = imp.Tables[0].Rows.Cast<DataRow>().Sum(r => r["Import_Value"] == DBNull.Value ? 0m : Convert.ToDecimal(r["Import_Value"]));
            decimal totalExp = exp.Tables[0].Rows.Cast<DataRow>().Sum(r => r["Export_Value"] == DBNull.Value ? 0m : Convert.ToDecimal(r["Export_Value"]));
            return (imp.Tables[0], exp.Tables[0], Common.RoundMe(first), Common.RoundMe(first + totalImp - totalExp));
        }

        /// <summary>
        /// إجمالى يوميات الخزينة عن فترة: لكل صندوق (أو كل الصناديق) رصيد أول واستلام وصرف ورصيد نهائى، تفصيلى (يوم بيوم) أو إجمالى للفترة —
        /// Get_Money_Box_Transaction_PeriodTotalPerDay كما هى (عيب الأصل فى آخر الشهر يُتفادى بالنداء يوماً بيوم).
        /// </summary>
        public DataTable BoxTotalsPerDay(System.Collections.Generic.IEnumerable<long> boxIds, DateTime from, DateTime to, bool totalOnly)
        {
            var engine = new Money_Transaction_BoxOrderReportCLS();
            var result = new DataTable();
            var arr = new System.Collections.ArrayList();
            foreach (var b in boxIds) arr.Add(b);
            int excp = 0; string msg = "";
            if (totalOnly)
            {
                engine.Get_Money_Box_Transaction_PeriodTotalPerDay(ref excp, ref msg, ref result, arr, true, from.Date, to.Date, 1);
                return result;
            }
            // يوماً بيوم: الدالة الأصلية تضيف صف اليوم ثم تبنى تاريخ اليوم التالى بـ`Day + 1` فتفشل فى آخر الشهر وتبتلع الخطأ بعد إضافة الصف، فالنداء ليوم واحد صحيح دائماً.
            for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
            {
                var one = new DataTable();
                engine.Get_Money_Box_Transaction_PeriodTotalPerDay(ref excp, ref msg, ref one, arr, false, d, d, 0);
                if (result.Columns.Count == 0) result = one.Clone();
                foreach (DataRow r in one.Rows) result.ImportRow(r);
            }
            return result;
        }

        // ================= إذن «كل الخزن» (OrderMoney_TransactionManyForAccounts) =================

        /// <summary>الصناديق النشطة للفرع مرتبة بالافتراضى أولاً، بقيمة صفر — كما GetBanksListWithNewValues(Is_Bank=0) التى تملأ شبكة الفورم.</summary>
        public System.Collections.Generic.List<Money_Transaction_Box> NewManyBoxLines() => new BanksClss().GetBanksListWithNewValues(0, Common.Branch_Id);

        /// <summary>
        /// إذن استلام/صرف «كل الخزن»: مبلغ لكل صندوق (سطر إذن فرعى لكل صندوق برقم الإذن نفسه) وقيد يومية واحد مجمَّع —
        /// Make_Transaction_BanksBox_FromInvoicesMulti_ كما هى داخل معاملة واحدة، وبنفس تجهيز أسطر ReadData/Set_Transaction_Money_Box_ فى الفورم
        /// (بلا ربط بفاتورة/أمر شغل). الصرف لا يتجاوز رصيد كل صندوق إلا لو IsBoxBalanceAllowSarfAny=1.
        /// </summary>
        public (long OrderId, long OrderNo) SaveManyBoxVoucher(int transactionType, DateTime date, long orderNo, string manName, string notes, long subAccountId, string accountName, string accountCode,
                                                               int accountType, long costCenterId, string costCenterName, long currencyId, string currencyName, decimal balanceBefore, decimal balanceAfter,
                                                               System.Collections.Generic.List<Money_Transaction_Box> boxes)
        {
            if (subAccountId <= 0) throw new InvalidOperationException("Account is required");
            if (boxes == null || !boxes.Any(b => (b.Transaction_Value ?? 0) > 0)) throw new InvalidOperationException("Value must be greater than zero");
            var opp = new operations(2);
            var engine = new Money_Transaction_BoxOrderReportCLS();
            try
            {
                var om = engine.omMain;
                om.Transaction_type = transactionType;
                om.Order_No = orderNo;
                om.Reverse_Order = 0;
                om.Invoice_Type = 0;
                om.Order_Date = date;
                om.Man_Name = manName ?? "";
                om.Order_Notes = notes ?? "";
                om.IsMoreTheOneBoxShow = 1;
                om.Currency_Id = currencyId <= 0 ? 1 : currencyId;
                om.Entry_Currency_Name = currencyName;
                om.Is_Balances_Set = 0;
                om.Day_Registration_flage = 0;
                om.DayReg_List_Id = 0;
                om.Bank_Type = 0;
                om.User_Id = Common.User_Id;
                om.Branch_Id = Common.Branch_Id;
                om.OperationDate = DateTime.Now.Date;
                om.Insert_ServerDateTime = DateTime.Now.Date;
                om.Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(date);
                om.Order_mony_id_transform = -1;
                om.Ticket_Supplier_Id = 0;
                om.Sandouk_Cost_Center_Tree_Id = 0;
                om.IS_Posted_Flage = 0;
                om.Entry_Currency_ID = om.Currency_Id;
                om.RequirementValue = 0; om.paidTotal = 0; om.StillValue = 0;
                om.Account_Type = accountType;

                // تحديد الطرف: عميل/مورد بحسابه الفرعى، وإلا حساب عام.
                long customerId = 0, supplierId = 0;
                if (accountType == Common.Customer_Account_Type)
                {
                    var t = opp.return_DataTable("SELECT TOP 1 Id FROM Customer WHERE SubAccount_Id = " + subAccountId);
                    if (t.Rows.Count == 0) throw new InvalidOperationException("Customer not found for account");
                    customerId = Convert.ToInt64(t.Rows[0][0]);
                }
                else if (accountType == Common.Supplier_Account_Type)
                {
                    var t = opp.return_DataTable("SELECT TOP 1 Id FROM Supplier WHERE SubAccount_Id = " + subAccountId);
                    if (t.Rows.Count == 0) throw new InvalidOperationException("Supplier not found for account");
                    supplierId = Convert.ToInt64(t.Rows[0][0]);
                }
                long partyId = customerId > 0 ? customerId : supplierId;
                om.Account_Id = partyId;

                foreach (var item in boxes)
                {
                    item.Transaction_Value = item.Transaction_Value ?? 0;
                    if (item.Transaction_Value <= 0) continue;
                    decimal boxBalance = 0;
                    var bnk = engine.GetBank_Balance(item.Bank_Id, ref opp, ref boxBalance);
                    if (transactionType == 2 && Common.IsBoxBalanceAllowSarfAny == 0 && item.Transaction_Value > engine.GetBank_Balance(ref opp, item.Bank_Id, true))
                        throw new InvalidOperationException("Box balance does not allow this payment: " + bnk.Bank_Name);
                    item.Invoice_Id = 0; item.CarsId = 0; item.CarNumber = ""; item.CarsWorkOrdersId = 0;
                    item.Reference_Type_Table_Id = 0; item.DataDictionaryProcessesListsId = 0;
                    item.Notes = (bnk.Bank_Name + " " + (notes ?? "")).Trim();
                    item.Account_Type = accountType;
                    item.Transaction_Date = date;
                    item.Transaction_Type = transactionType;
                    item.Invoice_TypeId = 0;
                    item.Transaction_Value_ = item.Transaction_Value.ToString();
                    item.Bank_Id = bnk.Id; item.Bank_Name = bnk.Bank_Name;
                    item.BankAccount_Id = bnk.SubAccount_Id; item.BankSubAccount_Id = bnk.SubAccount_Id;
                    item.User_Id = Common.User_Id; item.Branch_Id = Common.Branch_Id;
                    item.Equivalent_Value = item.Factor;
                    item.DestructionSales = 0; item.Order_mony_id_transform = -1; item.TotalValueWillPaid = 0; item.Rest_Value = 0;
                    item.Account_Code = accountCode; item.Account_Name = accountName;
                    item.Account_Cost_Center_Tree_Id = costCenterId; item.Cost_Center = costCenterName; item.CostCenter_Name = costCenterName;
                    item.Purchase_Ticket_Value = 0; item.Commission_Value = 0; item.Entry_Currency_ID = 0;
                    item.Tafkeet_Value = Tafkeet(item.Transaction_Value.Value, 1);
                    item.Lookup_CashPaymentMethodId = 0; item.Lookup_CashPaymentMethodName = "";
                    item.SubAccount_Id = subAccountId;
                    item.Account_Id = partyId;
                    item.Customer_Id = customerId; item.Supplier_Id = supplierId;
                    if (transactionType == 1) { item.Import_Value = item.Transaction_Value; item.Export_Value = 0; item.Bank_Balance = boxBalance + item.Transaction_Value.Value; }
                    else { item.Import_Value = 0; item.Export_Value = item.Transaction_Value; item.Bank_Balance = boxBalance - item.Transaction_Value.Value; }
                    item.Balance_Before = balanceBefore; item.Balance_After = balanceAfter;
                }

                // رقم الإذن: من المحرك كما GetOrderNo فى الفورم لو لم يُمرَّر.
                if (om.Order_No <= 0)
                {
                    int e0 = 0; long no0 = 0;
                    new BanksClss().GetMake_New_Order_No(ref e0, ref no0, transactionType, date, boxes);
                    om.Order_No = no0;
                }
                int excp = 0; string msg = "";
                long orderId = 0;
                engine.mbxList = boxes;
                engine.Customer_ID_Supplier = partyId;
                engine.Make_Transaction_BanksBox_FromInvoicesMulti_(ref excp, ref msg, ref opp, ref orderId, om, boxes, partyId);
                if (excp != 0 || orderId <= 0) throw new InvalidOperationException(string.IsNullOrWhiteSpace(msg) ? "Voucher was not saved" : msg);
                opp.Commit();
                return (orderId, om.Order_No);
            }
            catch
            {
                try { opp.Rollback(); } catch { }
                throw;
            }
        }

        // ================= إذن نقدية مرتبط بفاتورة/مرتجع (مبيعات 1 / مشتريات 2 / مرتجع مبيعات 7 / مرتجع مشتريات 8) =================

        /// <summary>الجدول وعمود الطرف لنوع المستند: 1 فاتورة مبيعات، 7 مرتجع مبيعات، 2 فاتورة مشتريات، 8 مرتجع مشتريات.</summary>
        private static (string Table, string PartyColumn, string PartyTable)? DocSource(int docType) => docType switch
        {
            1 => ("Invoices_List", "Customer_Id", "Customer"),
            7 => ("Invoices_List_Reverse", "Customer_Id", "Customer"),
            2 => ("Bills_List", "Supplier_Id", "Supplier"),
            8 => ("Bills_List_Reverse", "Supplier_Id", "Supplier"),
            _ => null
        };

        /// <summary>اتجاه الإذن لنوع المستند: (type, reverse) كما تفهمه شاشة الإذن — مبيعات: قبض من عميل؛ مرتجع مبيعات: صرف لعميل؛ مشتريات: صرف لمورد؛ مرتجع مشتريات: قبض من مورد.</summary>
        public static (int Type, int Reverse) DocDirection(int docType) => docType switch
        {
            1 => (1, 0), 7 => (2, 1), 2 => (2, 0), 8 => (1, 1), _ => (1, 0)
        };

        /// <summary>بيانات المستند للسداد: الطرف واسمه ورقم المستند وتاريخه وصافيه وما دُفع عليه من الإذونات (Reference_Type_Table_Id + DataDictionaryProcessesListsId) والمتبقى.</summary>
        /// <summary>
        /// عدد أسطر الخزينة المرتبطة بفاتورة (نوع العملية + رقم الفاتورة) التى أنشأها المستخدم بإذن نقدية مستقل ولم ينشئها محرك الفاتورة.
        /// فاتورة المشتريات/مرتجعها النقدية تكتب سطراً واحداً بنفسها فيُستبعد. يُستعمل لمنع حذف فاتورة عليها دفعات (كما تفعل أنظمة ERP الحديثة:
        /// لا تُحذف وثيقة عليها دفعة مُرحَّلة قبل عكس الدفعة).
        /// </summary>
        public int CountLinkedCashVouchers(int docType, long docId, bool isCashInvoice)
        {
            var t = new operations().return_DataTable("SELECT COUNT(*) FROM Money_Transaction_Box WHERE DataDictionaryProcessesListsId = " + docType + " AND Reference_Type_Table_Id = " + docId);
            var count = Convert.ToInt32(t.Rows[0][0]);
            var engineOwned = isCashInvoice && (docType == 2 || docType == 8) ? 1 : 0;
            return Math.Max(0, count - engineOwned);
        }

        public (bool Found, long PartyId, string PartyName, long DocNo, DateTime Date, decimal Net, decimal Paid, decimal Remaining) GetDocForPayment(int docType, long docId)
        {
            var src = DocSource(docType);
            if (src is null) return (false, 0, "", 0, DateTime.Now, 0, 0, 0);
            var opp = new operations();
            var t = opp.return_DataTable("SELECT d." + src.Value.PartyColumn + " AS PartyId, ISNULL(p.Name,'') AS PartyName, d.Invoice_Id AS DocNo, d.Invoice_Date, ISNULL(d.Net_Total,0) AS Net FROM " + src.Value.Table +
                " d LEFT JOIN " + src.Value.PartyTable + " p ON p.Id = d." + src.Value.PartyColumn + " WHERE d.ID = " + docId);
            if (t.Rows.Count == 0) return (false, 0, "", 0, DateTime.Now, 0, 0, 0);
            var r = t.Rows[0];
            var paid = Convert.ToDecimal(opp.return_DataTable("SELECT ISNULL(SUM(ISNULL(Transaction_Value,0)),0) FROM Money_Transaction_Box WHERE DataDictionaryProcessesListsId = " + docType + " AND Reference_Type_Table_Id = " + docId).Rows[0][0]);
            var net = Convert.ToDecimal(r["Net"]);
            return (true, Convert.ToInt64(r["PartyId"]), r["PartyName"]?.ToString()?.Trim() ?? "", r["DocNo"] == DBNull.Value ? 0 : Convert.ToInt64(r["DocNo"]),
                r["Invoice_Date"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(r["Invoice_Date"]), net, paid, Math.Max(0, net - paid));
        }

        /// <summary>رأس الإذن وسطوره للعرض.</summary>
        public (DataRow? Head, DataTable Lines) GetVoucher(long orderId)
        {
            var opp = new operations();
            var head = opp.return_DataTable("SELECT o.*, b.Bank_Name FROM Order_Money o LEFT JOIN Banks b ON b.Id = o.Bank_ID WHERE o.Id = " + orderId);
            var lines = opp.return_DataTable("SELECT * FROM Money_Transaction_Box WHERE Order_Mony_Id = " + orderId + " ORDER BY Id");
            return (head.Rows.Count > 0 ? head.Rows[0] : null, lines);
        }
    }
}
