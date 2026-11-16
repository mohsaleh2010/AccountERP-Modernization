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

// P8.11.2 (ai-assistant-p811-plan.md ص3، ص7): تسجيل الصوت بـ MediaRecorder (webm/opus أو mp4 على Safari). لا Web Speech API (ص4).
let _rec = null;
let _lastBlob = null;

export async function startRecording(dotnet, maxSeconds) {
    if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia || typeof MediaRecorder === 'undefined') return 'unsupported';
    if (_rec) return 'busy';
    let stream;
    try { stream = await navigator.mediaDevices.getUserMedia({ audio: true }); }
    catch (e) { return (e && e.name === 'NotAllowedError') ? 'denied' : 'nodevice'; }
    const types = ['audio/webm;codecs=opus', 'audio/webm', 'audio/mp4', 'audio/ogg;codecs=opus', 'audio/ogg'];
    const mime = types.find(t => MediaRecorder.isTypeSupported && MediaRecorder.isTypeSupported(t)) || '';
    let rec;
    try { rec = new MediaRecorder(stream, mime ? { mimeType: mime } : undefined); }
    catch { stream.getTracks().forEach(t => t.stop()); return 'unsupported'; }
    const chunks = [];
    rec.ondataavailable = e => { if (e.data && e.data.size) chunks.push(e.data); };
    const done = new Promise(res => {
        rec.onstop = () => {
            stream.getTracks().forEach(t => t.stop());
            res(new Blob(chunks, { type: rec.mimeType || mime || 'audio/webm' }));
        };
    });
    rec.start(250);
    const timer = setTimeout(() => {
        if (rec.state !== 'inactive') { try { dotnet.invokeMethodAsync('OnRecordingAutoStopped'); } catch { } }
    }, Math.max(5, maxSeconds || 60) * 1000);
    _rec = { rec, done, timer };
    return 'ok';
}

export async function stopRecording() {
    if (!_rec) return null;
    const r = _rec; _rec = null;
    clearTimeout(r.timer);
    if (r.rec.state !== 'inactive') r.rec.stop();
    _lastBlob = await r.done;
    return { mime: _lastBlob.type || 'audio/webm', size: _lastBlob.size };
}

export async function takeRecording() {
    const b = _lastBlob; _lastBlob = null;
    if (!b) return new Uint8Array(0);
    return new Uint8Array(await b.arrayBuffer());
}

export function cancelRecording() {
    if (!_rec) return;
    const r = _rec; _rec = null;
    clearTimeout(r.timer);
    try { if (r.rec.state !== 'inactive') r.rec.stop(); } catch { }
    _lastBlob = null;
}

export function appendComposer(textarea, text) {
    if (!textarea || !text) return;
    const cur = textarea.value || '';
    textarea.value = cur.length && !/\s$/.test(cur) ? cur + ' ' + text : cur + text;
    textarea.dispatchEvent(new Event('input', { bubbles: true }));
    textarea.focus({ preventScroll: true });
    textarea.setSelectionRange(textarea.value.length, textarea.value.length);
}
