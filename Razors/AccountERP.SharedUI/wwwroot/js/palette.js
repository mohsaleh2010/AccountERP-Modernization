// 🔎 لوحة البحث السريع Ctrl+K (P8.2.2، ai-assistant-p82-plan.md س23–س28).
// كل التنقل بالأسهم داخل المتصفح (رحلة .NET لكل سهم بطيئة على Blazor Server)؛ Enter فقط يستدعى .NET برقم الصف.
// نمط ARIA combobox: التركيز يبقى فى الحقل والصف النشط عبر aria-activedescendant.

const S = window.__cmdkState || (window.__cmdkState = {
    dotnet: null, shortcuts: true, bound: false,
    open: false, panel: null, input: null, list: null, active: -1, count: 0,
    restore: null, bodyOverflow: '', panelKey: null, panelMove: null
});

const isMac = /Mac|iPhone|iPad|iPod/i.test(navigator.platform || navigator.userAgent || '');

async function call(method, ...args) {
    if (!S.dotnet) return;
    try { await S.dotnet.invokeMethodAsync(method, ...args); } catch { /* دائرة منقطعة */ }
}

function isTypingTarget(el) {
    if (!el) return false;
    if (el.isContentEditable) return true;
    const tag = (el.tagName || '').toLowerCase();
    return tag === 'input' || tag === 'textarea' || tag === 'select';
}

function dialogOpen() { return !!document.querySelector('.mud-dialog-container'); }

function onGlobalKey(e) {
    if (e.isComposing || e.repeat || !S.dotnet) return;
    const mod = e.ctrlKey || e.metaKey;
    if (mod && !e.shiftKey && !e.altKey && e.code === 'KeyK') {
        if (!S.shortcuts && !S.open) return;
        if (!S.open && dialogOpen()) return;
        e.preventDefault();
        e.stopPropagation();
        call(S.open ? 'CloseFromKeyboard' : 'OpenFromShortcut');
        return;
    }
    if (!S.shortcuts || S.open || mod || e.altKey) return;
    if ((e.key === '/' || (e.code === 'Slash' && !e.shiftKey)) && !isTypingTarget(e.target) && !dialogOpen()) {
        e.preventDefault();
        call('OpenFromShortcut');
    }
}

// يُستدعى مرة لكل دائرة؛ إعادة الاتصال تستبدل المرجع فقط ولا تضيف مستمعاً ثانياً.
export function init(dotnet, options) {
    S.dotnet = dotnet;
    S.shortcuts = !options || options.shortcuts !== false;
    if (!S.bound) {
        document.addEventListener('keydown', onGlobalKey, true);
        S.bound = true;
    }
    return isMac;
}

export function setOptions(options) {
    S.shortcuts = !options || options.shortcuts !== false;
}

function options() { return S.list ? Array.from(S.list.querySelectorAll('[role="option"]')) : []; }

function setActive(i, scroll) {
    const opts = options();
    S.count = opts.length;
    if (!opts.length) {
        S.active = -1;
        S.input && S.input.removeAttribute('aria-activedescendant');
        return;
    }
    if (i < 0) i = 0;
    if (i >= opts.length) i = opts.length - 1;
    S.active = i;
    opts.forEach((o, k) => o.setAttribute('aria-selected', k === i ? 'true' : 'false'));
    const el = opts[i];
    S.input && S.input.setAttribute('aria-activedescendant', el.id);
    if (scroll !== false) el.scrollIntoView({ block: 'nearest' });
}

function onPanelKey(e) {
    if (e.isComposing) return;
    const n = S.count;
    switch (e.key) {
        case 'ArrowDown':
            e.preventDefault();
            if (n) setActive(S.active < 0 ? 0 : (S.active + 1) % n);
            break;
        case 'ArrowUp':
            e.preventDefault();
            if (n) setActive(S.active <= 0 ? n - 1 : S.active - 1);
            break;
        case 'PageDown':
            e.preventDefault();
            if (n) setActive(Math.min(n - 1, Math.max(0, S.active) + 5));
            break;
        case 'PageUp':
            e.preventDefault();
            if (n) setActive(Math.max(0, S.active - 5));
            break;
        case 'Home':
        case 'End':
            // داخل الحقل: Home/End تتحرك فى القائمة فقط مع Ctrl أو والحقل فارغ، وإلا يبقى سلوك تحريك المؤشر.
            if (n && (e.ctrlKey || (S.input && !S.input.value))) {
                e.preventDefault();
                setActive(e.key === 'Home' ? 0 : n - 1);
            }
            break;
        case 'Enter':
            e.preventDefault();
            call('ActivateFromKeyboard', S.active);
            break;
        case 'Escape':
            e.preventDefault();
            e.stopPropagation();
            call('CloseFromKeyboard');
            break;
        case 'Tab': {
            // مصيدة تركيز: يدور بين عناصر اللوحة القابلة للتركيز (الحقل وزر الإغلاق وأى زر لاحق).
            const f = Array.from(S.panel.querySelectorAll('input, button:not([disabled]), [tabindex]:not([tabindex="-1"])'))
                .filter(x => x.offsetParent !== null);
            if (!f.length) return;
            const idx = f.indexOf(document.activeElement);
            e.preventDefault();
            const next = e.shiftKey ? (idx <= 0 ? f.length - 1 : idx - 1) : (idx < 0 || idx >= f.length - 1 ? 0 : idx + 1);
            f[next].focus();
            break;
        }
    }
}

function onPanelMove(e) {
    const opt = e.target && e.target.closest ? e.target.closest('[role="option"]') : null;
    if (!opt) return;
    const i = parseInt(opt.getAttribute('data-idx'), 10);
    if (!isNaN(i) && i !== S.active) setActive(i, false);
}

export function opened(panel, input, list) {
    if (!panel || !input) return;
    if (!S.open) {
        const a = document.activeElement;
        S.restore = a && a !== document.body ? a : null;
        S.bodyOverflow = document.body.style.overflow;
        document.body.style.overflow = 'hidden';
    }
    if (S.panel) {
        S.panel.removeEventListener('keydown', S.panelKey);
        S.panel.removeEventListener('mousemove', S.panelMove);
    }
    S.open = true;
    S.panel = panel;
    S.input = input;
    S.list = list || panel.querySelector('[role="listbox"]');
    S.panelKey = onPanelKey;
    S.panelMove = onPanelMove;
    panel.addEventListener('keydown', S.panelKey);
    panel.addEventListener('mousemove', S.panelMove);
    input.value = '';
    input.focus({ preventScroll: true });
    setActive(0, false);
}

// بعد كل رسم للنتائج: الصف الأول نشط. keep (P8.2.4: وصول نتائج البيانات بعد الشاشات) يُبقى الصف النشط ومكان التمرير.
export function sync(keep) {
    if (!S.open) return;
    if (keep && S.active >= 0) {
        setActive(S.active, false);
        return;
    }
    setActive(0);
    if (S.list) S.list.scrollTop = 0;
}

// بعد «مسح الأخيرة»: التركيز يعود للحقل (الزر اختفى).
export function focusInput() {
    if (S.open && S.input) S.input.focus({ preventScroll: true });
}

export function closed() {
    if (S.panel) {
        S.panel.removeEventListener('keydown', S.panelKey);
        S.panel.removeEventListener('mousemove', S.panelMove);
    }
    if (S.open) document.body.style.overflow = S.bodyOverflow || '';
    S.open = false;
    S.panel = S.input = S.list = null;
    S.active = -1;
    S.count = 0;
    const r = S.restore;
    S.restore = null;
    if (r && r.isConnected && typeof r.focus === 'function') {
        try { r.focus({ preventScroll: true }); } catch { }
    }
}

// عند الإغلاق بالتنقل لشاشة أخرى: لا نعيد التركيز لعنصر الشاشة القديمة.
export function forgetRestore() { S.restore = null; }

export function dispose() {
    if (S.open) closed();
    S.dotnet = null;
    if (S.bound) {
        document.removeEventListener('keydown', onGlobalKey, true);
        S.bound = false;
    }
}
