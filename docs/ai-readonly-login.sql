/*
  المساعد الذكى — حساب قاعدة بيانات للقراءة فقط (ai-assistant-plan.md §2.1، الطبقة 1).
  يشغّله مسؤول قاعدة البيانات يدوياً مرة واحدة لكل عميل. لا يُنفَّذ تلقائياً من النظام.

  1) غيّر <DatabaseName> لاسم قاعدة العميل، و<StrongPassword> لكلمة مرور قوية (لا تحفظها فى المستودع).
  2) شغّل السكربت على الخادم.
  3) أضف فى appsettings.Production.json (أو متغير البيئة ConnectionStrings__AiReadOnly):
       "ConnectionStrings": { "AiReadOnly": "Server=***;Database=<DatabaseName>;User Id=***;Password=***;TrustServerCertificate=True;" }
  4) أعد تشغيل الويب. شاشة «إعدادات الذكاء الاصطناعى» ← «سجل التدقيق» تُظهر أن الحساب المخصص مستخدم.

  النتيجة: الحساب يقرأ Views المساعد (schema ai) فقط، وكل ما عداها ممنوع حتى لو تجاوز استعلامٌ الفحص البرمجى.
  الـ Views مملوكة لـ dbo فتقرأ الجداول الأصلية عبر سلسلة الملكية دون منح الحساب أى صلاحية عليها.
*/
USE [master];
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'erp_ai_reader')
    CREATE LOGIN [erp_ai_reader] WITH PASSWORD = N'<StrongPassword>', CHECK_POLICY = ON, DEFAULT_DATABASE = [<DatabaseName>];
GO
USE [<DatabaseName>];
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'erp_ai_reader')
    CREATE USER [erp_ai_reader] FOR LOGIN [erp_ai_reader] WITH DEFAULT_SCHEMA = [ai];
GO
GRANT SELECT ON SCHEMA::[ai] TO [erp_ai_reader];
DENY SELECT, INSERT, UPDATE, DELETE, EXECUTE, ALTER ON SCHEMA::[dbo] TO [erp_ai_reader];
DENY INSERT, UPDATE, DELETE, EXECUTE, ALTER ON SCHEMA::[ai] TO [erp_ai_reader];
GO
