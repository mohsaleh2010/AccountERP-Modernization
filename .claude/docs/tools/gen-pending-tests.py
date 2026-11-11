# -*- coding: utf-8 -*-
"""Regenerate the test dashboards.

Source of truth: master-user-test-plan.md (م- rows) and testing-plan.md (old screens, ب- rows).
1) Rewrites the "لوحة المتابعة" table in master-user-test-plan.md from its sections.
2) Rewrites pending-tests-all.md with every row whose status is ⬜ / ⚠️ / 🔁.
Run from anywhere:  python -I .claude/docs/tools/gen-pending-tests.py
Record results ONLY in the two source files, then re-run this script.
"""
import re, sys, os, datetime

sys.stdout.reconfigure(encoding='utf-8')
D = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
MASTER = os.path.join(D, 'master-user-test-plan.md')
OLD = os.path.join(D, 'testing-plan.md')
OUT = os.path.join(D, 'pending-tests-all.md')
PENDING = ('⬜', '⚠', '🔁')
RX_M = re.compile(r'^\|\s*م-(\d+)\s*\|')
RX_B = re.compile(r'^\|\s*(\d+)\s*\|')


def read(p):
    return open(p, encoding='utf-8').read().split('\n')


def status(line):
    return [c.strip() for c in line.rstrip().strip('|').split('|')][-1]


def sections(lines, rx, start_marker=None, stop_marker=None):
    """[(heading, table_header_lines, [(num, line)])] in file order."""
    out, sec, hdr, active = [], None, None, start_marker is None
    for l in lines:
        if start_marker and l.startswith(start_marker):
            active = True
        if stop_marker and l.startswith(stop_marker):
            active = False
        if not active:
            continue
        if l.startswith('## ') or l.startswith('### '):
            sec = l
        elif l.startswith('| #') or l.startswith('| الرقم'):
            hdr = l
        m = rx.match(l)
        if m and sec:
            if not out or out[-1][0] != sec:
                out.append((sec, hdr, []))
            out[-1][2].append((int(m.group(1)), l))
    return out


def ranges(nums):
    nums = sorted(nums)
    parts, a = [], nums[0]
    for p, n in zip(nums, nums[1:] + [None]):
        if n != p + 1:
            parts.append('م-%d' % a if a == p else 'م-%d … م-%d' % (a, p))
            a = n
    return '، '.join(parts)


def title(h):
    return re.sub(r'^#+\s*', '', h).strip()


def main():
    today = datetime.date.today().isoformat()
    M = read(MASTER)
    secs = sections(M, RX_M, stop_marker='## ٧)')
    nums = [n for _, _, r in secs for n, _ in r]
    dup = sorted({n for n in nums if nums.count(n) > 1})
    gaps = [n for n in range(1, max(nums) + 1) if n not in nums]
    if dup or gaps:
        print('WARNING duplicates:', dup, 'gaps:', gaps)

    # 1) dashboard in master
    dash = ['| القسم | الأرقام | تم | الباقى | الحالة |', '|---|---|---|---|---|']
    tot_done = 0
    for h, _, r in secs:
        done = sum(1 for _, l in r if not status(l).startswith(PENDING))
        tot_done += done
        icon = '✅' if done == len(r) else ('⬜' if done == 0 else '◐')
        dash.append('| %s | %s | %d | %d | %s |' % (title(h), ranges([n for n, _ in r]), done, len(r) - done, icon))
    dash.append('| **الإجمالى** | م-1 … م-%d | %d | %d | |' % (max(nums), tot_done, len(nums) - tot_done))
    dash.append('| ٧ — الاختبارات اليدوية القديمة | ملف `testing-plan.md` (أرقام ب-) | | | |')
    i = next(k for k, l in enumerate(M) if l.startswith('## لوحة المتابعة'))
    a = next(k for k in range(i, len(M)) if M[k].startswith('|'))
    b = a
    while b < len(M) and M[b].startswith('|'):
        b += 1
    M[a:b] = dash
    M[i + 1:a] = ['', '> مولَّدة آلياً (%s) بـ `tools/gen-pending-tests.py` — لا تعدّلها يدوياً.' % today, '']
    open(MASTER, 'w', encoding='utf-8', newline='\n').write('\n'.join(M))

    # 2) pending file
    out = ['# كل الاختبارات المؤجلة وغير المنفذة — خطة واحدة (%s)' % today, '',
           '**المصدر:** مولَّد آلياً بـ `.claude/docs/tools/gen-pending-tests.py` من `master-user-test-plan.md` (الجزء أ: بنود م-) '
           'و`testing-plan.md` (الجزء ب: الشاشات القديمة، أرقامها ب-N). لا يحوى إلا ما حالته ⬜ (لم يُختبر) أو ⚠️ (ناقص) أو 🔁 (إعادة اختبار بعد إصلاح). '
           '**لا تسجّل النتائج هنا:** أخبرنى بالرقم ونتيجته (م-12 أو ب-7 …) فأسجّلها فى الملف الأصلى ثم أعيد التوليد.', '',
           '**قبل البدء:** نسخة احتياطية من القاعدة؛ شغّل الموقع `dotnet run --launch-profile https` من `Razors/AccountERP.Web/AccountERP.Web`؛ '
           'اضبط الفترة المالية 6 مفتوحة ونشطة من شاشة الفترات (وإلا ظهر تحذير PERIODCLOSED الخاطئ).', '',
           '## الجزء أ — بنود الخطة الموحَّدة (م-)', '']
    ca = 0
    for h, hdr, r in secs:
        p = [l for _, l in r if status(l).startswith(PENDING)]
        if not p:
            continue
        ca += len(p)
        out += ['### ' + title(h), '', hdr, '|---|---|---|---|'] + p + ['']
    out += ['**عدد بنود الجزء أ: %d** (من %d)' % (ca, len(nums)), '',
            '## الجزء ب — الاختبارات القديمة (`testing-plan.md`، ترقيمها يبدأ بالحرف ب)', '']
    O = read(OLD)
    cb, tb = 0, 0
    for h, hdr, r in sections(O, RX_B, start_marker='## 1)'):
        tb += len(r)
        p = [RX_B.sub(lambda m: '| ب-%s |' % m.group(1), l, count=1) for _, l in r if status_old(l).startswith(PENDING)]
        if not p:
            continue
        cb += len(p)
        out += ['### ' + title(h), '', hdr, '|---|---|---|---|---|'] + p + ['']
    out += ['**عدد بنود الجزء ب: %d** (من %d)' % (cb, tb), '', '**الإجمالى: %d اختباراً.**' % (ca + cb), '']
    open(OUT, 'w', encoding='utf-8', newline='\n').write('\n'.join(out))
    print('master rows', len(nums), 'done', tot_done, '| pending A', ca, 'B', cb, '/', tb)


def status_old(line):
    # testing-plan.md columns: # | test | path | status | notes
    c = [x.strip() for x in line.rstrip().strip('|').split('|')]
    return c[3] if len(c) > 3 else ''


if __name__ == '__main__':
    main()
