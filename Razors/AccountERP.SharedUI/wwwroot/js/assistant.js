// 🤖 المساعد الذكى P1: مساعدات صغيرة لشاشة المحادثة (Enter للإرسال، تمرير تلقائى للأسفل، نسخ الرد، تكبير حقل الكتابة).

// Enter يرسل وShift+Enter سطر جديد؛ لا يتدخل أثناء كتابة IME (تجميع الحروف).
export function bindComposer(textarea, dotnet) {
    if (!textarea || textarea.__aiBound) return;
    textarea.__aiBound = true;
    const resize = () => {
        textarea.style.height = 'auto';
        textarea.style.height = Math.min(textarea.scrollHeight, 200) + 'px';
    };
    textarea.addEventListener('input', resize);
    textarea.addEventListener('keydown', e => {
        if (e.key === 'Enter' && !e.shiftKey && !e.isComposing && !e.ctrlKey && !e.altKey) {
            e.preventDefault();
            dotnet.invokeMethodAsync('SubmitFromKeyboard', textarea.value);
        }
    });
    textarea.__aiResize = resize;
    resize();
}

export function clearComposer(textarea) {
    if (!textarea) return;
    textarea.value = '';
    textarea.__aiResize && textarea.__aiResize();
}

export function setComposer(textarea, text) {
    if (!textarea) return;
    textarea.value = text || '';
    textarea.__aiResize && textarea.__aiResize();
    textarea.focus();
}

export function getValue(textarea) { return textarea ? textarea.value : ''; }

export function focus(el) { el && el.focus({ preventScroll: true }); }

// يمرّر للأسفل فقط إن كان المستخدم قريباً من الأسفل (لا يقاطع من يقرأ رداً أقدم)، أو force.
export function scrollToBottom(el, force) {
    if (!el) return;
    const near = el.scrollHeight - el.scrollTop - el.clientHeight < 120;
    if (force || near) el.scrollTop = el.scrollHeight;
}

export async function copyText(text) {
    try { await navigator.clipboard.writeText(text); return true; }
    catch {
        const ta = document.createElement('textarea');
        ta.value = text; ta.style.position = 'fixed'; ta.style.opacity = '0';
        document.body.appendChild(ta); ta.select();
        let ok = false;
        try { ok = document.execCommand('copy'); } catch { }
        ta.remove();
        return ok;
    }
}

// Escape يغلق النافذة العائمة.
export function bindEscape(el, dotnet) {
    if (!el || el.__aiEsc) return;
    el.__aiEsc = true;
    el.addEventListener('keydown', e => {
        if (e.key === 'Escape') { e.stopPropagation(); dotnet.invokeMethodAsync('CloseFromKeyboard'); }
    });
}

// P6: معاينة مرفق المساعد: الملف يصل من الدائرة كتيار (لا رابط عام له)، يُفتح فى تبويب جديد، وإن منع المتصفح النافذة يُنزَّل.
export async function openBlob(streamRef, mime, name) {
    const buf = await streamRef.arrayBuffer();
    const url = URL.createObjectURL(new Blob([buf], { type: mime || 'application/octet-stream' }));
    const win = window.open(url, '_blank');
    if (!win) {
        const a = document.createElement('a');
        a.href = url; a.download = name || 'file'; document.body.appendChild(a); a.click(); a.remove();
    }
    setTimeout(() => URL.revokeObjectURL(url), 60000);
}

// P6: لصق صورة (لقطة شاشة/صورة فاتورة) فى حقل الكتابة = رفعها كمرفق عبر نفس InputFile (نفس التحقق على الخادم).
export function bindPaste(textarea, inputId) {
    if (!textarea || textarea.__aiPaste) return;
    textarea.__aiPaste = true;
    textarea.addEventListener('paste', e => {
        const files = e.clipboardData && e.clipboardData.files;
        const input = document.getElementById(inputId);
        if (!files || files.length === 0 || !input || input.disabled) return;
        e.preventDefault();
        const dt = new DataTransfer();
        for (const f of files) dt.items.add(f);
        input.files = dt.files;
        input.dispatchEvent(new Event('change', { bubbles: true }));
    });
}
