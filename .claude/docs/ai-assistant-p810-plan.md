## P8.10 — خادم MCP للقراءة — خطة تفصيلية

كُتبت 2026-11-15 (Opus) بعد P8.9.5. تحل محل مخطط P8.10. المرحلة «حسب طلب العملاء»: الخطة جاهزة، والتنفيذ عند طلب عميل أو موافقة صريحة. القرارات م1… محسومة بالموصى به.

> ملاحظات قبل التنفيذ:
> - **قراءة فقط:** يُكشف فقط ما فى `AiToolRegistry` من أدوات القراءة؛ لا مسودات ولا `ask_user` ولا `extract_document`.
> - **موقوف افتراضياً** (`McpEnabled=0`) ويحتاج تفعيلاً من المدير ثم توكن شخصياً لكل مستخدم.
> - لا أسرار فى السجلات: التوكن يُعرض مرة واحدة عند الإنشاء ويُخزَّن **هاش** فقط.
> - ويب فقط (نقطة HTTP فى `AccountERP.Web`).

### حقائق من الكود

- `AiToolRegistry` (`Razors/AccountERP.AI/Tools/AiToolRegistry.cs`): `ToolsFor(AiToolContext ctx) → List<AiToolDef>` (سطر 326)، `ExecuteAsync(name, inputJson, ctx)` (سطر 353)، `TitleOf`, `FormatResult`. كل أداة تعلن شاشاتها (`PermissionGate`) وتفحص `Allowed(ctx)`. أدوات القراءة: `search_schema`, `run_readonly_query`, `find_entity`, `get_periods_and_branches`, `get_trial_balance`, `get_financial_statement`, `get_collections_priority`, (`render_report`/`open_screen`/`find_screen` واجهية).
- `AiUserContextFactory.ForUser(userId) → AiServerUser` (`AccountModelsLib/BusinessLibShared/Ai/AiUserContextFactory.cs` سطر 50): يبنى سياق المستخدم والصلاحيات بلا جلسة واجهة (استُعمل فى الموجز والمهام).
- `ReadOnlySqlExecutor` بتسجيل دخول قراءة فقط و`SessionFor` (`ai_box_perm`, `ai_tax`).
- التدقيق `AiToolAuditCLS.Insert(...)`.
- `Program.cs` (`AccountERP.Web/AccountERP.Web/Program.cs`): `AddRateLimiter` سطر 81 (سياسة نافذة ثابتة لكل IP)، `UseRateLimiter` سطر 165، نمط `MapSalesRepMobileApi(app)` (توكن Bearer عبر DataProtection، `SalesRepDeviceAuthException`).

### القرارات (م)

| # | القرار | السبب |
|---|---|---|
| م1 | النقل: **MCP Streamable HTTP** (POST JSON-RPC 2.0 على `/mcp`، استجابة JSON واحدة بلا SSE فى هذه المرحلة) | المعيار الحالى لـ MCP البعيد؛ لا حاجة لبث |
| م2 | التنفيذ: الحزمة الرسمية `ModelContextProtocol.AspNetCore` إن وُجد إصدار مستقر ≥ 1.0 يعمل على net10 (فحص 1)، وإلا معالج JSON-RPC يدوى صغير (`initialize`, `notifications/initialized`, `ping`, `tools/list`, `tools/call`) | الرسمى أولاً؛ اليدوى بسيط ومعروف الحدود |
| م3 | المصادقة: توكن شخصى `erp_mcp_<32 بايت عشوائى base64url>` فى `Authorization: Bearer`؛ يُخزَّن SHA-256 فقط فى `AI_ApiTokens (Id, User_Id, Name, TokenHash, Prefix, Scopes, CreatedAt, ExpiresAt, LastUsedAt, LastUsedIp, RevokedAt)` | نمط GitHub PAT؛ لا OAuth الآن (P0.2 مؤجل) |
| م4 | صلاحية التوكن الافتراضية 90 يوماً، حد أقصى 365، حد 5 توكنات نشطة لكل مستخدم | أمان |
| م5 | كل طلب: هاش ← توكن نشط ← `AiUserContextFactory.ForUser(User_Id)` ← المستخدم نشط وما زال يملك «المساعد الذكى» ← `ToolsFor(ctx)`؛ لا ذاكرة تخزين للصلاحيات أكثر من 60 ث | سحب الصلاحية فورى تقريباً |
| م6 | الأدوات المكشوفة = قائمة سماح ثابتة (`McpToolAllowList`): أدوات القراءة السبع أعلاه فقط؛ `run_readonly_query` يحتاج scope `sql` منفصلاً يختاره المستخدم عند إنشاء التوكن | تقليل السطح |
| م7 | المخرج: نص `FormatResult` (حد 50 صفاً كما فى المحادثة) + `structuredContent` JSON | متوافق مع العملاء |
| م8 | Rate limit: 60 طلباً/دقيقة لكل توكن + 10 فشل مصادقة/دقيقة لكل IP (السياسة الموجودة) | حماية |
| م9 | التدقيق: كل `tools/call` فى `AI_ToolCalls_Audit` بـ `Conversation_Id = 0` و`toolName = "mcp:" + name`؛ عمود جديد اختيارى غير لازم | لا تعديل جداول قديمة |
| م10 | `DataMinimization` لا يُطبق (المستخدم يقرأ بياناته بصلاحياته) — مكتوب صراحة فى الشاشة | كالمحادثة |
| م11 | الإعداد `McpEnabled` (0) + `McpAllowSqlScope` (0) + `McpTokenMaxDays` (365)؛ عند الإيقاف `/mcp` يعيد 404 | موقوف افتراضياً |
| م12 | واجهة: تبويب «الوصول الخارجى (MCP)» فى `/settings/ai` للمدير (تفعيل، توكنات الجميع، إلغاء)، وقسم «توكناتى» فى قائمة المستخدم (إنشاء، نسخ مرة واحدة، إلغاء، آخر استعمال) + مثال إعداد Claude Desktop/Claude Code | الاستعمال الفعلى |
| م13 | HTTPS إلزامى خارج localhost؛ رفض `Origin` غير موثوق (حماية DNS rebinding حسب مواصفة MCP) | توصية المواصفة |

### 1) الكود

- `Razors/AccountERP.AI/Mcp/AiMcpTokens.cs` (إنشاء/هاش/تحقق/إلغاء، pure للهاش) + `AiApiTokensCLS` (AccountModelsLib).
- `Razors/AccountERP.AI/Mcp/AiMcpServer.cs`: `HandleAsync(JsonNode request, AiServerUser user, ct)` (pure-ish، قابل للاختبار بلا HTTP).
- `AccountERP.Web/Api/AiMcpEndpoint.cs`: `MapAiMcp(app)` + سياسة rate limit `AiMcpPolicy`.
- `Components/Assistant/Settings/AiMcpTab.razor`، `Components/Account/MyApiTokens.razor`.

### 2) الأجزاء

- **P8.10.1 — التوكنات:** الجدول، الإعدادات، الإنشاء/التحقق/الإلغاء + harness (هاش، انتهاء، إلغاء، حد 5).
- **P8.10.2 — الخادم:** `AiMcpServer` + النقطة + rate limit + التدقيق + harness (initialize، tools/list حسب الصلاحية، tools/call، أداة غير مسموحة، توكن ملغى، Origin).
- **P8.10.3 — الواجهة + مفاتيح `AIM_` + الاختبارات** م-614…م-625.

### 3) اختبارات المستخدم (م-614…م-625)

- م-614 خادم MCP — تفعيل الميزة من تبويب الوصول الخارجى.
- م-615 خادم MCP — إنشاء توكن شخصى ونسخه مرة واحدة.
- م-616 خادم MCP — التوكن لا يظهر مرة أخرى بعد إغلاق الحوار.
- م-617 خادم MCP — ربط Claude Desktop بالعنوان والتوكن وظهور الأدوات.
- م-618 خادم MCP — سؤال من Claude Desktop عن ميزان المراجعة يعيد أرقام الشاشة نفسها.
- م-619 خادم MCP — مستخدم بلا صلاحية ميزان المراجعة لا يرى الأداة.
- م-620 خادم MCP — توكن بلا نطاق sql لا يرى `run_readonly_query`.
- م-621 خادم MCP — إلغاء التوكن يوقف الوصول فوراً.
- م-622 خادم MCP — توكن منتهى الصلاحية يُرفض.
- م-623 خادم MCP — إيقاف الميزة يعيد 404.
- م-624 خادم MCP — سجل التدقيق يظهر الطلبات بعلامة mcp.
- م-625 خادم MCP — تجاوز 60 طلباً فى الدقيقة يُرفض مؤقتاً.

### 4) المخاطر

- تسرب التوكن ⇒ هاش فقط، انتهاء، إلغاء، آخر استعمال/IP مرئى.
- عملاء MCP قديمون يتوقعون SSE ⇒ خارج النطاق، يوثَّق.

### 5) فحوص مفتوحة

1. وجود `ModelContextProtocol.AspNetCore` مستقر ومتوافق مع net10 وإصدار عمره ≥ أسبوعين.
2. هل `ForUser` يفحص نشاط المستخدم (`USER_DETAILES` Active)؟ وإلا يُضاف الفحص فى طبقة MCP.
3. سلوك `render_report`/`open_screen` خارج الواجهة (مستبعدة فى القائمة).

### Critical Files

`Razors/AccountERP.AI/Tools/AiToolRegistry.cs`، `AccountModelsLib/BusinessLibShared/Ai/AiUserContextFactory.cs`، `AccountERP.Web/AccountERP.Web/Program.cs`، `AccountERP.Web/AccountERP.Web/Api/SalesRepMobileApi.cs` (نمط)، `AiToolAuditCLS`.

### تم التنفيذ

(فارغ)
