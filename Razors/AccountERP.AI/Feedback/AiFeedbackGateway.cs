using AccountERP.AI.SignIn;
using AccountModelsLib;

namespace AccountERP.AI.Feedback
{
    /// <summary>
    /// P7.6: قائمة مراجعة ملاحظات 👍/👎 (تبويب «الملاحظات» فى /settings/ai). كل دالة للمدير فقط (صلاحية شاشة /settings/ai).
    /// «اعتماد كمثال» = مقطع example فى AI_Knowledge (يمر بنفس تحقق تبويب «المعرفة»: SELECT واحد على Views المساعد)،
    /// «حالة تقييم» = حالة فى AI_EvalCases (نفس تحقق تبويب «التقييم»). الملاحظة نفسها لا تُحذف؛ تتغير حالة مراجعتها فقط.
    /// </summary>
    public sealed class AiFeedbackGateway : IAiFeedbackGateway
    {
        private readonly AiProvidersCLS _providers = new();
        private readonly AiFeedbackCLS _db;
        private readonly Knowledge.AiKnowledgeService _knowledge;
        private readonly IAiEvalGateway _eval;

        public AiFeedbackGateway(Knowledge.AiKnowledgeService knowledge, IAiEvalGateway eval) : this(knowledge, eval, null) { }

        internal AiFeedbackGateway(Knowledge.AiKnowledgeService knowledge, IAiEvalGateway eval, AiFeedbackCLS? db)
        {
            _knowledge = knowledge;
            _eval = eval;
            _db = db ?? new AiFeedbackCLS();
        }

        private void EnsureAdmin(long userId)
        {
            if (userId <= 0 || !_providers.UserHasMenu(userId, AiSignInService.MenuKey))
                throw new InvalidOperationException("مراجعة الملاحظات متاحة لمن يملك صلاحية شاشة «إعدادات الذكاء الاصطناعى» فقط.");
        }

        public List<AiFeedbackRow> GetFeedback(string status, int rating, string search, int top = 300)
        {
            string st = status is AiFeedbackStatus.New or AiFeedbackStatus.Accepted or AiFeedbackStatus.Rejected ? status : "";
            int rt = rating > 0 ? AiFeedbackRatings.Up : rating < 0 ? AiFeedbackRatings.Down : 0;
            return _db.List(st, rt, search ?? "", top);
        }

        public AiFeedbackCounts GetCounts() => _db.Counts();

        private AiFeedbackRow Load(long id) => _db.Get(id) ?? throw new InvalidOperationException("الملاحظة غير موجودة.");

        public long AcceptAsExample(AiFeedbackExampleRequest r, long userId)
        {
            EnsureAdmin(userId);
            if (r == null) throw new InvalidOperationException("طلب غير صالح.");
            var f = Load(r.FeedbackId);
            if (f.ReviewStatus == AiFeedbackStatus.Rejected) throw new InvalidOperationException("الملاحظة مرفوضة؛ أعد فتحها أولاً.");
            if (f.Knowledge_Id != null) throw new InvalidOperationException("اعتُمدت هذه الملاحظة مثالاً من قبل.");
            long id = _knowledge.SaveAdmin(new AiKnowledgeSaveRequest
            {
                Kind = AiKnowledgeKinds.Example,
                TitleAr = string.IsNullOrWhiteSpace(r.TitleAr) ? Cut((f.QuestionText ?? "").Trim(), Knowledge.AiKnowledgeService.MaxTitle) : r.TitleAr,
                Content = r.Sql ?? "",
                Keywords = r.Keywords ?? "",
                Source = AiKnowledgeSources.Feedback,
                Source_Ref_Id = f.Id,
            }, userId);
            _db.MarkAccepted(f.Id, id, null, userId);
            return id;
        }

        public long AddAsEvalCase(long feedbackId, AiEvalCaseSaveRequest r, long userId)
        {
            EnsureAdmin(userId);
            if (r == null) throw new InvalidOperationException("طلب غير صالح.");
            var f = Load(feedbackId);
            if (f.ReviewStatus == AiFeedbackStatus.Rejected) throw new InvalidOperationException("الملاحظة مرفوضة؛ أعد فتحها أولاً.");
            if (f.EvalCase_Id != null) throw new InvalidOperationException("أُضيفت هذه الملاحظة حالة تقييم من قبل.");
            r.Id = 0;
            r.Source = "feedback";
            r.Source_Ref_Id = f.Id;
            long id = _eval.SaveCase(r, userId);
            _db.MarkAccepted(f.Id, null, id, userId);
            return id;
        }

        private static string Cut(string s, int max) => s.Length > max ? s[..max] : s;

        public void Reject(long feedbackId, string note, long userId)
        {
            EnsureAdmin(userId);
            var f = Load(feedbackId);
            if (f.ReviewStatus == AiFeedbackStatus.Accepted) throw new InvalidOperationException("الملاحظة معتمدة؛ عطّل المثال أو الحالة الناتجة من تبويبها بدلاً من رفضها.");
            string n = (note ?? "").Trim();
            if (n.Length > 1000) throw new InvalidOperationException("سبب الرفض أطول من 1000 حرف.");
            _db.SetStatus(f.Id, AiFeedbackStatus.Rejected, n.Length == 0 ? null : n, userId);
        }

        public void Reopen(long feedbackId, long userId)
        {
            EnsureAdmin(userId);
            var f = Load(feedbackId);
            if (f.ReviewStatus != AiFeedbackStatus.Rejected) throw new InvalidOperationException("تُعاد فتح الملاحظات المرفوضة فقط.");
            _db.SetStatus(f.Id, AiFeedbackStatus.New, null, userId);
        }
    }
}
