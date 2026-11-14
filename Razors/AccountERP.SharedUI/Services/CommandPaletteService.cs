using System;
using System.Collections.Generic;
using System.Linq;
using AccountModelsLib;
using Microsoft.Extensions.DependencyInjection;

namespace AccountERP.SharedUI.Services
{
    /// <summary>
    /// 🔎 لوحة البحث السريع Ctrl+K (P8.2، ai-assistant-p82-plan.md). Scoped لكل دائرة.
    /// P8.2.2: مجموعة الشاشات فقط، فى الذاكرة وبلا قاعدة بيانات ولا نموذج (س1، س3، س5).
    /// المصدر = MenuState.UserAllowedMenus (نفس صلاحيات القائمة الجانبية)، الأوراق المبنية فعلاً (KnownRoutes.IsImplemented).
    /// بوابة بحث البيانات وخدمة المساعد اختياريتان (null على MAUI) وتُستعملان فى P8.2.4/P8.2.5.
    /// </summary>
    public sealed class CommandPaletteService : IDisposable
    {
        private readonly MenuStateService _menu;
        private readonly LanguageService _lang;
        private readonly IAiSearchGateway? _gw;
        private readonly AssistantSessionService? _chat;

        private List<ScreenEntry> _index = new();
        private List<MenuViewModel>? _indexSource;
        private int _indexCount = -1;
        private string _indexLang = "";

        public CommandPaletteService(IServiceProvider sp, MenuStateService menu, LanguageService lang)
        {
            _menu = menu;
            _lang = lang;
            _gw = sp.GetService<IAiSearchGateway>();
            _chat = sp.GetService<AssistantSessionService>();
            _menu.OnAuthChanged += Invalidate;
            _menu.OnMainMenuChanged += Invalidate;
            _lang.OnLanguageChanged += Invalidate;
        }

        /// <summary>اللوحة متاحة لمستخدم مسجّل دخوله وله قائمة محمَّلة.</summary>
        public bool IsAvailable => _menu.IsAuthenticated && _menu.IsLoaded;

        /// <summary>هل يوجد بحث بيانات (ويب فقط)؛ يُفعَّل فعلياً فى P8.2.4.</summary>
        public bool HasDataGateway => _gw != null && _chat != null;

        /// <summary>يُضبط من palette.js (init) لعرض ⌘ K بدل Ctrl K على أجهزة Apple.</summary>
        public bool IsMac { get; private set; }

        public event Action? OpenRequested;
        public event Action? Changed;

        public void RequestOpen()
        {
            if (IsAvailable) OpenRequested?.Invoke();
        }

        public void SetIsMac(bool isMac)
        {
            if (IsMac == isMac) return;
            IsMac = isMac;
            Changed?.Invoke();
        }

        /// <summary>الشاشات المطابقة للنص (فارغ ⇒ لا شئ). hitCounts (RefKey ⇒ عدد) دفعة «الأخيرة» من P8.2.4.</summary>
        public List<PaletteItem> SearchScreens(string text, IDictionary<string, int>? hitCounts)
        {
            var result = new List<PaletteItem>();
            if (string.IsNullOrWhiteSpace(text) || !IsAvailable) return result;
            string q = text.Trim();
            if (q.Length > AiSearchConst.MaxQueryChars) q = q.Substring(0, AiSearchConst.MaxQueryChars);

            var scored = new List<(ScreenEntry E, int S)>();
            foreach (var e in EnsureIndex())
            {
                int hits = 0;
                if (hitCounts != null) hitCounts.TryGetValue(e.RefKey, out hits);
                int s = AiSearchText.Score(q, e.Title, e.AllTitles, e.ParentAll, hits);
                if (s > 0) scored.Add((e, s));
            }

            foreach (var (e, _) in scored
                         .OrderByDescending(x => x.S)
                         .ThenBy(x => x.E.Title.Length)
                         .ThenBy(x => x.E.Title, StringComparer.Ordinal)
                         .Take(AiSearchConst.ScreensMax))
            {
                result.Add(new PaletteItem
                {
                    Kind = AiSearchKinds.Screen,
                    Title = e.Title,
                    Subtitle = e.ParentLocalized,
                    Icon = e.Icon,
                    Url = e.Route,
                    RefKey = e.RefKey,
                });
            }
            return result;
        }

        private void Invalidate()
        {
            _indexSource = null;
            _indexCount = -1;
        }

        private List<ScreenEntry> EnsureIndex()
        {
            var menus = _menu.UserAllowedMenus ?? new List<MenuViewModel>();
            string lang = _lang.CurrentLanguage ?? "";
            if (ReferenceEquals(menus, _indexSource) && menus.Count == _indexCount && lang == _indexLang) return _index;

            var parents = new HashSet<long>(menus.Where(m => m?.ParentId != null && m.ParentId != 0).Select(m => m.ParentId!.Value));
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var list = new List<ScreenEntry>();
            foreach (var m in menus)
            {
                if (m == null || parents.Contains(m.Menu_ID)) continue;   // الأوراق فقط
                string route = m.GetEffectiveRoute();
                if (!KnownRoutes.IsImplemented(route)) continue;          // «قيد الإنشاء» لا تظهر
                string key = route.Trim().ToLowerInvariant();
                if (!seen.Add(key)) continue;

                var chain = menus.BuildAncestorPath(m);
                var ancestors = chain.Take(Math.Max(0, chain.Count - 1)).ToList();
                list.Add(new ScreenEntry
                {
                    Route = route.Trim(),
                    RefKey = key,
                    Title = m.GetLocalizedDescription(lang),
                    AllTitles = new[] { m.Description, m.Description_En, m.Description_Fr, m.Description_DE }
                        .Where(s => !string.IsNullOrWhiteSpace(s)).ToArray(),
                    ParentLocalized = string.Join(" › ", ancestors.Select(a => a.GetLocalizedDescription(lang))),
                    ParentAll = string.Join(" ", ancestors.SelectMany(a => new[] { a.Description, a.Description_En, a.Description_Fr, a.Description_DE })
                        .Where(s => !string.IsNullOrWhiteSpace(s))),
                    Icon = MenuIcons.GetLeafIcon(m.Description),
                });
            }

            _index = list;
            _indexSource = menus;
            _indexCount = menus.Count;
            _indexLang = lang;
            return _index;
        }

        public void Dispose()
        {
            _menu.OnAuthChanged -= Invalidate;
            _menu.OnMainMenuChanged -= Invalidate;
            _lang.OnLanguageChanged -= Invalidate;
        }

        private sealed class ScreenEntry
        {
            public string Route = "";
            public string RefKey = "";
            public string Title = "";
            public string[] AllTitles = Array.Empty<string>();
            public string ParentLocalized = "";
            public string ParentAll = "";
            public string Icon = "";
        }
    }

    /// <summary>صف فى لوحة البحث السريع (شاشة الآن؛ كيانات ومستندات و«اسأل المساعد» لاحقاً).</summary>
    public sealed class PaletteItem
    {
        public string Kind { get; set; } = "";
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public string Trailing { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Url { get; set; } = "";
        public string RefKey { get; set; } = "";
        public long Id { get; set; }
    }
}
