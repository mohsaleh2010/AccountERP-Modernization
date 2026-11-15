## P8.11 — الإدخال الصوتى بالعربية — خطة تفصيلية

كُتبت 2026-11-15 (Opus) بعد P8.9.5. تحل محل مخطط P8.11. «حسب طلب العملاء». القرارات ص1… محسومة بالموصى به.

> ملاحظات قبل التنفيذ:
> - **النص يُعرض للتعديل قبل الإرسال دائماً**؛ لا إرسال تلقائى.
> - **الصوت لا يُخزَّن** بعد التفريغ (يُحذف من الذاكرة فوراً)؛ لا جدول جديد للصوت.
> - ويب أولاً؛ MAUI لاحقاً (أذونات الميكروفون مختلفة).

### حقائق من الكود

- `AiTaskKeys` (`AccountModelsLib/BusinessLibShared/Ai/AiModels.cs` سطر 54): `chat, reasoning, vision, fast, embedding` — لا `speech`. جدول `AI_TaskRouting.TaskKey` نصى ⇒ إضافة مفتاح بلا تغيير مخطط.
- البروتوكولات: `openai`, `anthropic`, `gemini` (+ ollama عبر openai). Anthropic بلا واجهة تفريغ صوت. OpenAI-compatible: `POST /audio/transcriptions` (multipart). Gemini: صوت inline فى `generateContent`.
- `Providers/HttpProviderAdapters.cs`, `HttpEmbeddings.cs` (نمط استدعاء غير محادثة)، `ProviderRegistry`، `AiHttp`، `AiUrlPolicy`.
- `AssistantChat.razor`: مؤلف بـ `InputFile` (سطر 651)، وحدة JS `_module` (`bindPaste` سطر 787)، `Chat.SendAsync(text, context)` سطر 814، `Chat.CanUpload`, `Chat.UploadLimits`.
- `IAiChatGateway` فيه `Upload/Remove/GetUploadLimits` (P6).

### القرارات (ص)

| # | القرار | السبب |
|---|---|---|
| ص1 | مفتاح مهمة جديد `speech` فى `AiTaskKeys.All`، يظهر فى تبويب الربط | نفس آلية التوجيه |
| ص2 | المحوّلات: OpenAI-compatible `/audio/transcriptions` (يغطى OpenAI وGroq وخوادم Whisper المحلية المتوافقة مثل faster-whisper-server/LocalAI)، وGemini (صوت inline + تعليمات «فرّغ حرفياً»). Anthropic غير مدعوم ويُخفى | ما يتوفر فعلاً |
| ص3 | التسجيل فى المتصفح بـ `MediaRecorder` (webm/opus، أو mp4 على Safari)، حد 60 ث و5 MB، زر ضغط/إفلات أو نقرة بدء/إيقاف | بسيط ومدعوم |
| ص4 | `Web Speech API` للمتصفح **ممنوع افتراضياً** (الصوت يذهب لمزوّد المتصفح خارج إعدادات الشركة)؛ إعداد `SpeechAllowBrowser` (0) للمدير | احترام LocalOnly وتقليل البيانات |
| ص5 | `LocalOnly=1` ⇒ يُسمح فقط بمزوّد محلى للمهمة `speech` | قرار المرحلة P0 |
| ص6 | اللغة: `ar` افتراضياً (لهجة مصرية مقبولة)، تلميح مفردات قصير من أسماء الشاشات/المصطلحات (prompt ≤ 200 حرف) | تحسين الدقة |
| ص7 | النص يوضع فى المؤلف (لا يستبدل نصاً مكتوباً؛ يُلحق) مع تمييز مؤقت؛ المستخدم يعدّل ثم يرسل | قرار المخطط |
| ص8 | التكلفة تُسجَّل فى نفس جدول الاستعمال/التكلفة الموجود للمهمة (`speech`) بالثوانى | قياس |
| ص9 | إعدادات: `SpeechEnabled` (0)، `SpeechMaxSeconds` (60)، `SpeechAllowBrowser` (0)، `SpeechLanguage` (ar) | موقوف افتراضياً |
| ص10 | لوحة الأوامر Ctrl+K (P8.2) تحصل على نفس الميكروفون فى مرحلة لاحقة فقط | تقليل النطاق |

### 1) الكود

- `Providers/HttpTranscription.cs`: `TranscribeAsync(route, bytes, mime, lang, prompt, ct)` لكل بروتوكول + اختبار الاتصال.
- `Chat/AiChatGateway`: `TranscribeAsync(stream, mime, ct)` على `IAiChatGateway` (حد الحجم، التوجيه، المهلة 30 ث، تسجيل التكلفة).
- JS: `startRecording/stopRecording` فى وحدة `AssistantChat` (الأذونات، الرفض، المؤقت).
- `AssistantChat.razor`: زر ميكروفون بجانب 📎 بحالات (جاهز، يسجّل مع عداد، يفرّغ، خطأ).
- تبويب الربط: المهمة `speech` + اختبار بتسجيل قصير.

### 2) الأجزاء

- **P8.11.1 — المحوّلات والبوابة:** `speech`، `HttpTranscription`، `TranscribeAsync`، الإعدادات + harness بمزوّد mock (multipart لـ openai، inline لـ gemini، رفض anthropic، LocalOnly، الحدود).
- **P8.11.2 — الواجهة + مفاتيح `AIV_` + الاختبارات** م-626…م-635.

### 3) اختبارات المستخدم (م-626…م-635)

- م-626 الإدخال الصوتى — ربط مهمة «الصوت» بنموذج تفريغ واختبارها.
- م-627 الإدخال الصوتى — ظهور زر الميكروفون فى المحادثة بعد التفعيل.
- م-628 الإدخال الصوتى — طلب إذن الميكروفون أول مرة ورفضه يعرض رسالة واضحة.
- م-629 الإدخال الصوتى — جملة عربية مصرية قصيرة تظهر نصاً فى خانة الكتابة دون إرسال.
- م-630 الإدخال الصوتى — تعديل النص ثم الإرسال يعمل كالمعتاد.
- م-631 الإدخال الصوتى — التسجيل يتوقف تلقائياً عند الحد الأقصى.
- م-632 الإدخال الصوتى — نص مكتوب سابقاً لا يُمسح عند الإلحاق.
- م-633 الإدخال الصوتى — مزوّد Anthropic لا يظهر خياراً للصوت.
- م-634 الإدخال الصوتى — وضع «محلى فقط» يرفض مزوّداً سحابياً للصوت.
- م-635 الإدخال الصوتى — إيقاف الميزة يخفى الزر.

### 4) المخاطر

- Safari لا يدعم webm ⇒ mp4/aac؛ بعض خوادم Whisper المحلية لا تقبل mp4 ⇒ فحص 2.
- دقة اللهجة المصرية متفاوتة ⇒ النص دائماً قابل للتعديل.

### 5) فحوص مفتوحة

1. أين يُسجَّل الاستعمال/التكلفة حالياً (جدول/أعمدة) لإضافة وحدة «ثانية».
2. صيغ الصوت المقبولة لدى المزوّد المحلى المستهدف.
3. سياسة أذونات الميكروفون فى WebView لـ MAUI (لاحقاً).

### Critical Files

`AccountModelsLib/BusinessLibShared/Ai/AiModels.cs`، `Razors/AccountERP.AI/Providers/HttpProviderAdapters.cs`، `Providers/HttpEmbeddings.cs`، `Chat/AiChatGateway.cs`، `Razors/AccountERP.SharedUI/Components/Assistant/AssistantChat.razor` (+ ملف JS الخاص به).

### تم التنفيذ

(فارغ)
