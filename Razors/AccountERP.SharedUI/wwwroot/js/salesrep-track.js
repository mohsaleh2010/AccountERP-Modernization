// خريطة «مسار المندوبين»: Leaflet + OpenStreetMap يُحمَّلان عند فتح الصفحة فقط (لا تثقل باقى النظام).
// المسار خط، والبداية/النهاية والزيارات (داخل/خارج النطاق) والتوقفات علامات بنص وصفى (لا يعتمد على اللون وحده).
const LEAFLET_CSS = { href: 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.css', integrity: 'sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY=' };
const LEAFLET_JS = { src: 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.js', integrity: 'sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo=' };
let loading = null;
const maps = {};

function loadLeaflet() {
    if (window.L) return Promise.resolve();
    if (loading) return loading;
    loading = new Promise((resolve, reject) => {
        const css = document.createElement('link');
        css.rel = 'stylesheet'; css.href = LEAFLET_CSS.href; css.integrity = LEAFLET_CSS.integrity; css.crossOrigin = '';
        document.head.appendChild(css);
        const js = document.createElement('script');
        js.src = LEAFLET_JS.src; js.integrity = LEAFLET_JS.integrity; js.crossOrigin = '';
        js.onload = () => resolve();
        js.onerror = () => { loading = null; reject(new Error('leaflet')); };
        document.head.appendChild(js);
    });
    return loading;
}

function pin(color, label) {
    return L.divIcon({
        className: 'srt-pin',
        html: `<span style="background:${color}" aria-hidden="true"></span><b>${label}</b>`,
        iconSize: [28, 28], iconAnchor: [14, 14],
    });
}

function esc(s) { const d = document.createElement('div'); d.textContent = s ?? ''; return d.innerHTML; }

// data: { points: [[lat,lng,label]], visits: [{lat,lng,title,inside}], stops: [{lat,lng,title}], labels: {start,end} }
export async function render(id, data) {
    try { await loadLeaflet(); } catch { return false; }
    const el = document.getElementById(id);
    if (!el) return false;
    if (maps[id]) { maps[id].remove(); delete maps[id]; }
    const map = L.map(el, { zoomControl: true, attributionControl: true });
    maps[id] = map;
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 19, attribution: '&copy; OpenStreetMap' }).addTo(map);
    const bounds = [];
    const css = getComputedStyle(document.documentElement);
    const v = (name, fallback) => (css.getPropertyValue(name) || '').trim() || fallback;
    const c = {
        route: v('--mud-palette-primary', '#1e88e5'), start: v('--mud-palette-success', '#2e7d32'), end: v('--mud-palette-error', '#c62828'),
        stop: v('--mud-palette-warning', '#ef6c00'), inside: v('--mud-palette-success', '#2e7d32'), outside: v('--mud-palette-warning', '#ef6c00'),
    };
    if (data.points.length > 0) {
        const line = data.points.map(p => [p[0], p[1]]);
        L.polyline(line, { color: c.route, weight: 4, opacity: .85 }).addTo(map);
        line.forEach(p => bounds.push(p));
        const first = data.points[0], last = data.points[data.points.length - 1];
        L.marker([first[0], first[1]], { icon: pin(c.start, 'A'), title: data.labels.start }).bindPopup(esc(data.labels.start + ' ' + first[2])).addTo(map);
        L.marker([last[0], last[1]], { icon: pin(c.end, 'B'), title: data.labels.end }).bindPopup(esc(data.labels.end + ' ' + last[2])).addTo(map);
    }
    data.stops.forEach(s => {
        L.circleMarker([s.lat, s.lng], { radius: 9, color: c.stop, fillColor: c.stop, fillOpacity: .35, weight: 2 })
            .bindPopup(esc(s.title)).addTo(map);
        bounds.push([s.lat, s.lng]);
    });
    data.visits.forEach((v, i) => {
        L.marker([v.lat, v.lng], { icon: pin(v.inside === false ? c.outside : c.inside, String(i + 1)), title: v.title })
            .bindPopup(esc(v.title)).addTo(map);
        bounds.push([v.lat, v.lng]);
    });
    if (bounds.length > 0) map.fitBounds(bounds, { padding: [24, 24], maxZoom: 17 });
    else map.setView([30.0444, 31.2357], 10);
    setTimeout(() => map.invalidateSize(), 50);
    return true;
}

export function dispose(id) { if (maps[id]) { maps[id].remove(); delete maps[id]; } }
