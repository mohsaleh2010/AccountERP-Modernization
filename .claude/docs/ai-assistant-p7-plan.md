# P7 — تحسين المعرفة والتقييم (خطة تفصيلية)

Written 2026-11-14 by the Fable 5.2 planning agent, grounded in the code after P6 (0599874). Parent plan: `ai-assistant-plan.md` (P7 section, §13).

## Code facts
- Knowledge lives in code: `Tools/AiSchemaCatalog.cs` (14 `AiViewDoc` + 14 `AiKnowledgeItem`: 5 glossary, 4 rule, 5 example). `Search()` = normalized word containment, top 6, used by `AiToolRegistry.SearchSchema`.
- `AiViewDoc.Routes` drives `AiToolContext.ViewsFor`; `SystemPrompt` embeds `Describe(views)` (static → prompt cache friendly).
- No embedding call exists: `IAiProviderAdapter` has `ListModelsAsync` + `StreamChatAsync` only. `AI_Models.SupportsEmbeddings`, `AiTaskKeys.Embedding` and routing UI already exist.
- `AiChatGateway.RunAsync` (≈234–426) mixes the loop with persistence; `Candidates()` reads only chat routing.
- Routing unique index `(TaskKey, Priority)`; `SaveRouting` renumbers from 1 → Priority 0 is free for harness rows.
- `AccountModelsLib` cannot reference `AccountERP.AI` → catalog seed sync runs from the AI project.
- `AiSettingsPage.razor` 1017 lines, tabs Providers/Models/Audit, lazy load in `OnTabChangedAsync`.

## Decisions (recommended option, reason)
| # | Decision | Reason |
|---|---|---|
| D1 | `AI_Knowledge.Embedding varbinary(max)` float32 LE L2-normalized + `EmbeddingModel_Id`, `EmbeddingDim`, `ContentHash`, `EmbeddedAt` | few hundred chunks; SQL 2025 VECTOR not guaranteed |
| D2 | In-memory cosine in singleton `AiKnowledgeIndex` (cache, invalidate on write, 5 min TTL) | brute force < 5k vectors is µs |
| D3 | `EmbedAsync` on adapters; model from routing TaskKey `embedding`, lowest Priority, LocalOnly/enabled respected; Anthropic throws clear Arabic error | reuse routing |
| D4 | Degrade to keyword: no route / provider error / 3 s timeout / row model ≠ routed model; 5 min circuit breaker; `search_schema` never fails | chat must not depend on 2nd provider |
| D5 | View docs stay in code, mirrored as `Kind=view` each sync; glossary/rule/example seeded once, then table is truth | views tied to `AiReadViewsCLS` + permissions |
| D6 | Chunks: view, glossary, rule, example, screen (`AiScreenCatalog` static). No raw `ERP_V1_3.md` | model may only query `ai.*` |
| D7 | `ViewsCsv` (needs all) + `RoutesCsv` (any) filtered before ranking; example ViewsCsv derived via ScriptDom | keep "allowed views only" |
| D8 | Hybrid = RRF k=60 of keyword rank + cosine rank (top 20 each) → top 6 | no score calibration |
| D9 | Query embedded automatically (LRU 200); documents only via «بناء الفهرس» or after admin save/accept; nothing at startup | no silent spend |
| D10 | ~60 seed cases in `AiEvalSeed.cs`, insert-missing by `CaseKey`; entity-independent | works on any DB, no data audits |
| D11 | Deterministic judge by CheckType: result / tool / financial / ask / refuse | reproducible, free |
| D12 | Reference SQL/args executed live in the same run and user context | no frozen test DB |
| D13 | LLM judge deferred | not needed |
| D14 | Split `RunAsync` → persistence-free `LoopAsync` + save tail; eval passes one explicit candidate | never touches routing/conversations |
| D15 | `AiToolContext.IsEval` hides drafts + extract_document; eval reports deleted; ask_user ends case; audit gets `EvalRun_Id` | no leakage |
| D16 | Singleton background `AiEvalRunner`, sequential, per-case persistence, UI polls 2 s, cancellable, one active group; stale Running → «انقطع» | survives circuit loss |
| D17 | Launch dialog: count, model, estimated tokens/cost, required checkbox; server rejects without Confirmed + admin; optional `EvalMaxCostPerRun` | real credit needs admin approval |
| D18 | 1–2 models per launch, same `Group_Id` + case snapshot; history compare on CaseKey+CaseVersion intersection, warn on PromptHash diff | fair paired comparison |
| D19 | `AI_Feedback` one row per (message,user) with snapshot | outlives deleted conversations |
| D20 | Admin review queue: 👍 → «اعتماد كمثال» / «إضافة لمجموعة التقييم»; 👎 → eval case with corrected SQL or reject; nothing enters knowledge without review | human-in-the-loop |
| D21 | 3 new lazy tabs in `/settings/ai`: «المعرفة», «التقييم», «التقييمات والملاحظات», components under `Components/Assistant/Settings/` | no new menu/permission rows |
| D22 | Optional dynamic few-shot: top-3 examples appended to model-only user turn; `KnowledgeAutoExamples` default 0 until eval shows gain | keep system prompt cached |
| D23 | Single-turn cases only; no `find_entity` placeholder cases | deterministic judge |

Check types: **result** — any produced dataset contains reference rows (multiset, ignore column order/names, extra columns OK, tolerance 0.01; 1×1 may appear in text with Arabic digits normalized). **tool** — an ExpectedTool succeeded and no ForbiddenTool. **financial** — expected tool called and its dataset equals direct call with `ReferenceArgsJson`. **ask** — ask_user called, or no successful data tool and answer ends with «؟». **refuse** — no draft tool and no successful data query (+ optional ExpectedText). `ToolOk` recorded per case.

## Pieces (each committable; console harness with mock provider, Priority 0 routing rows, own cleanup, never ReplaceRouting)

### P7.1 — `AI_Knowledge` + catalog migration (keyword only, no behaviour change)
Columns: Kind nvarchar(20), KeyName nvarchar(150), TitleAr nvarchar(300), Content nvarchar(max), Keywords nvarchar(1000) NULL, ViewsCsv nvarchar(500) NULL, RoutesCsv nvarchar(1000) NULL, Source nvarchar(30) DEFAULT 'seed', Source_Ref_Id bigint NULL, Version int DEFAULT 1, IsEdited bit DEFAULT 0, IsActive bit DEFAULT 1, ContentHash char(64) NULL, Embedding varbinary(max) NULL, EmbeddingModel_Id bigint NULL, EmbeddingDim int NULL, EmbeddedAt datetime NULL. Unique `(Kind, KeyName)`.
Add `AiKnowledgeCLS.cs`, `AiKnowledgeModels.cs`, `Knowledge/AiKnowledgeService.cs` (SyncSeed once per process; views overwritten; others insert-missing / update only when Source=seed, IsEdited=0, higher version; cache; Search). Change schema, `AiSchemaCatalog` (KeyName/Version; Search = fallback), `AiToolRegistry.SearchSchema`, DI.
Done: same search output for existing 28 chunks; no duplicates on re-sync; deactivated row hidden; edited row survives. Harness: counts, old vs new top hits ~10 queries, sales-only user filter, DB failure fallback.

- **تم التنفيذ (2026-11-14):** جدول `AI_Knowledge` فى `AiAssistantSchemaCLS`؛ `AccountModelsLib/BusinessLibShared/Ai/AiKnowledgeCLS.cs` (ListAll/Get/Insert/UpdateFromSeed/SetActive؛ تغيّر المحتوى يمسح المتجه)؛ `Razors/AccountERP.AI/Knowledge/AiKnowledgeService.cs` (مزامنة مرة لكل عملية مع إعادة المحاولة بعد دقيقة، كاش 5 دقائق + Invalidate، فلترة Visible ثم KeywordRank بنفس منطق P2، والكود احتياطى عند فشل القراءة)؛ `AiSchemaCatalog` صار لكل مقطع Key ثابت وVersion و`Normalize` عامة؛ `search_schema` يقرأ من الخدمة؛ تسجيل Singleton.
  - **قرارات أثناء التنفيذ (الموصى به):** (1) مقاطع الشاشات (Kind=screen) مؤجّلة — موصوفة أصلاً فى أداة open_screen، وإضافتها تزيح نتائج P2. (2) ViewsCsv للأمثلة المزروعة مستنتج بـ regex على `ai.v_*` (الأمثلة اليدوية تمر بـ ReadOnlySqlExecutor.Validate فى P7.3 فيكفى نفس الاستنتاج). (3) تشديد مقصود: مثال يحتاج View غير مسموح لم يعد يظهر (كان يظهر فى P2). (4) اسم المفتاح يدخل نص البحث للـ Views فقط حتى تطابق النتائج P2 حرفياً.
  - **تحقق:** بناء الويب ناجح؛ اختبار خدمة 30/30 (scratchpad p7test): الجدول وإعادة التشغيل، 28 صفاً بلا تكرار، تطابق حرفى مع بحث P2 فى 12 استعلاماً وعبر أداة search_schema، مستخدم المبيعات لا يرى View/أمثلة المشتريات، الصف اليدوى والتعطيل، بقاء المعدَّل واستعادة صف view، التحديث بنسخة أعلى، الاحتياطى عند تعطّل القاعدة. لا اختبارات مستخدم لهذا الجزء (لا تغيير ظاهر)؛ م-424…م-426 تأتى مع تبويب المعرفة P7.3.

### P7.2 — Embeddings adapter + index + hybrid
`EmbedAsync` per protocol (OpenAI-compatible `/embeddings`, Azure deployments, Gemini `batchEmbedContents` with taskType, Ollama `api/embed`, Anthropic error); `AiHttp.PostJsonAsync`; `Knowledge/AiEmbeddingService.cs` (picker, batch 64, LRU, 3 s, breaker); `Knowledge/AiKnowledgeIndex.cs` (cosine + RRF); `search_schema` async; admin `GetKnowledgeIndexStatus` / `RebuildKnowledgeIndexAsync`.
Done: colloquial paraphrase finds glossary chunk; no route → identical to P7.1; stale model rows skipped. Harness: mock trigram vectors, request shapes all protocols, rebuild, RRF fixture, all fallbacks, breaker.

### P7.3 — «المعرفة» tab
`Components/Assistant/Settings/AiKnowledgeTab.razor`: status strip, «بناء الفهرس» with confirm, grid by kind + search, add/edit glossary/rule/example, toggle, views read-only badge, «تجربة البحث» with source badge. Gateway CRUD (example SQL validated, ViewsCsv derived). ~30 resx keys ×4. ui-ux-pro-max.

### P7.4 — Eval engine (no UI)
Tables `AI_EvalCases` (CaseKey unique, Question, Category, Dialect, CheckType, ExpectedTools, ForbiddenTools, ReferenceSql, ReferenceArgsJson, ExpectedText, Tags, Notes, Source, Source_Ref_Id, Version, IsEdited, IsActive), `AI_EvalRuns` (Group_Id, User_Id, Model_Id, ModelName, ProviderName, Effort, MaxTokens, Status, FilterJson, ConfigJson, PromptHash, Total/Done/Passed/Failed/Error/ToolOk cases, Input/OutputTokens, CostEstimate, DurationMs, StartedAt, FinishedAt, ErrorText), `AI_EvalResults` (Run_Id, Case_Id, CaseKey, CaseVersion, Outcome, ToolOk, FailReason, AnswerText, ToolsJson, Rounds, tokens, CostEstimate, DurationMs; unique Run_Id+Case_Id); column `AI_ToolCalls_Audit.EvalRun_Id`; settings `EvalMaxCasesPerRun=100`, `EvalMaxCostPerRun`.
Add `AiEvalCLS`, `AiEvalModels`, `AiEvalSeed` (~60: sales 12, purchases 8, stock 8, accounts 8, parties 5, screens 5, ambiguous 6, refuse 8; half Egyptian), `Eval/AiEvalJudge`, `Eval/AiEvalRunner`, `Eval/AiEvalGateway`. Pure-extraction split of `RunAsync` → `LoopAsync`; `IsEval` filter.
Harness: re-run P2/P4 loop assertions first; seed SQL valid; judge fixtures; scripted mock pass/fail/refuse/ask/error; no conversations/drafts/reports leak; cancel, unconfirmed, non-admin, cost cap, concurrent start.

### P7.5 — «التقييم» tab (م-380, م-381)
`AiEvalTab.razor` (history grid, KPIs, per-category bars with `AiChart`, per-case table, cases sub-view), `AiEvalRunDialog.razor` (1–2 models, filters, estimate, consent checkbox), `AiEvalCompareView.razor` (deltas, «الاختلافات فقط», mismatch banner). Live progress + «إيقاف». ~60 resx keys ×4.

### P7.6 — Feedback 👍/👎 + review queue
Table `AI_Feedback` (Message_Id, Conversation_Id, User_Id, Rating, Reason, Comment, QuestionText, AnswerText, SqlText, ToolsJson, Model_Id, ModelName, ReviewStatus DEFAULT 'New', ReviewedBy, ReviewedAt, Knowledge_Id, EvalCase_Id; unique Message_Id+User_Id). `SetFeedback` (owner, ok assistant messages, upsert, toggle while New); buttons beside Copy in `AssistantChat.razor`; `AiFeedbackTab.razor` queue with accept-as-example / add-as-eval-case / reject; badge count.

### P7.7 — Dynamic few-shot (optional, measured)
`KnowledgeAutoExamples` default 0; flag in ConfigJson; flip only if eval improves.

### Risks
RunAsync split (pure extraction, re-run old assertions); hosts without `/embeddings` (surface error, keep keyword); 429s → error + one retry; results depend on launching admin's permissions (show user on run row).

### Open checks
Admin gating in `AiSettingsPage`/`AiAdminGateway`; `AiScreenCatalog` entry shape; `AiToolAuditCLS.Insert` signature; whether `AiReportEngine` can validate without persisting.

## User tests
م-380 تشغيل مجموعة التقييم وعرض نسبة النجاح · م-381 مقارنة نموذجين على نفس المجموعة · م-424 ظهور تبويب «المعرفة» وعرض المقاطع · م-425 إضافة مصطلح للمسرد وعثور المساعد عليه · م-426 تعطيل مقطع فلا يظهر فى البحث · م-427 ربط نموذج تضمين وبناء الفهرس «مفهرس n من m» · م-428 البحث الهجين يجد المقطع بصياغة عامية · م-429 بلا نموذج تضمين يعمل البحث بالكلمات · م-430 تغيير نموذج التضمين يطلب إعادة البناء · م-431 تشغيل التقييم يطلب موافقة مع التكلفة التقديرية · م-432 إيقاف التقييم فى منتصفه وحفظ الجزئى · م-433 تفاصيل حالة فاشلة · م-434 إضافة حالة تقييم وتشغيلها · م-435 التقييم لا ينشئ محادثات ولا مسودات ولا يغيّر الربط · م-436 مقارنة تشغيلين سابقين «الاختلافات فقط» · م-437 إعجاب/عدم إعجاب مع سبب يبقى بعد التحميل · م-438 اعتماد رد كمثال واستخدامه · م-439 تحويل رد غير مُرضٍ لحالة تقييم · م-440 رفض ملاحظة · م-441 غير المدير لا يرى تبويبات P7 · م-442 تبويبات P7 باللغات والداكن والموبايل · م-443 «أمثلة تلقائية» قبل/بعد (P7.7)
