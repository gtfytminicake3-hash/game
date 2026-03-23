import os
import glob
import re
import codecs

lang_dir = r"d:\game\legendofblood\legacyofblood_unity_mobile_portraitframe\Assets\Resources\Localization"
output_dir = r"d:\game\legendofblood\legacyofblood_unity_mobile_portraitframe\Assets\Prefabs\praser\output"

vi_path = os.path.join(lang_dir, "vi.txt")
en_path = os.path.join(lang_dir, "en.txt")

def parse_lang(filepath):
    keys = set()
    val_to_key = {}
    try:
        with open(filepath, 'r', encoding='utf-8-sig') as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith('#'):
                    parts = line.split('=', 1)
                    if len(parts) >= 2:
                        k = parts[0].strip()
                        v = parts[1].strip()
                        keys.add(k)
                        val_to_key[v] = k
    except Exception as e:
        print(f"Error reading {filepath}: {e}")
    return keys, val_to_key

vi_keys, vi_val_to_key = parse_lang(vi_path)
en_keys, en_val_to_key = parse_lang(en_path)

def decode_unity_string(s):
    s = s.strip()
    if s.startswith('"') and s.endswith('"'):
        s = s[1:-1]
        try:
            s = codecs.decode(s, 'unicode_escape')
        except:
            pass
    return s

all_texts = set()
file_to_texts = {}

for fpath in glob.glob(os.path.join(output_dir, '*.txt')):
    filename = os.path.basename(fpath)
    file_to_texts[filename] = set()
    with open(fpath, 'r', encoding='utf-8') as f:
        for line in f:
            line_s = line.strip()
            if line_s.startswith('m_text:') or line_s.startswith('m_Text:'):
                val = line_s.split(':', 1)[1]
                decoded = decode_unity_string(val)
                if decoded and not decoded.isspace():
                    file_to_texts[filename].add(decoded)
                    all_texts.add((filename, decoded))

# Categories
using_keys = []
using_hardcoded_values = []
unknown_unlocalized = []

for filename, txt in all_texts:
    # Bỏ qua các chuỗi quá ngắn hoặc chỉ toàn số/kí tự đặc biệt
    if len(txt) <= 1 or re.match(r'^[\d\s\+\-\*\/\%\.,:]+$', txt):
        continue
    # Bỏ qua các format string như "{0}/{1}" hoặc "x{0}"
    if re.match(r'^[\d\s\w\{\}\:\/\-]+$', txt) and '{0}' in txt:
         # unless it's an actual key
         if txt not in vi_keys:
             continue
    
    if txt in vi_keys or txt in en_keys:
        using_keys.append((filename, txt))
    elif txt in vi_val_to_key:
        using_hardcoded_values.append((filename, txt, vi_val_to_key[txt]))
    else:
        # Check if it resembles a key (snake_case or lowercase with _)
        if re.match(r'^[a-z0-9_]+$', txt) and len(txt) > 4:
            # It's a missing key!
            pass
        unknown_unlocalized.append((filename, txt))

with open(r"C:\Users\admin\.gemini\antigravity\brain\738c89fd-6a92-41b3-bab5-f7cedde7cb7d\localization_check_report.md", "w", encoding="utf-8") as out_f:
    out_f.write("# Phân Tích Localization Trong Prefabs và Scene\n\n")

    out_f.write(f"## 1. UI components sử dụng chuẩn format KEY ({len(using_keys)})\n")
    for f, t in sorted(using_keys):
        out_f.write(f"- `{f}`: **{t}**\n")

    out_f.write(f"\n## 2. [CẢNH BÁO] UI components dùng GIÁ TRỊ CỨNG (Hardcoded) thay vì KEY ({len(using_hardcoded_values)})\n")
    out_f.write("Thay vì gõ thẳng tiếng Việt vào Text, hãy điền Key tương ứng.\n\n")
    for f, t, k in sorted(using_hardcoded_values):
        out_f.write(f"- `{f}`: \"{t}\" -> Khuyên dùng key: **`{k}`**\n")

    out_f.write(f"\n## 3. [NGUY HIỂM] Các văn bản (text) hoặc nút bấm MỚI chưa hề có key/dịch ({len(unknown_unlocalized)})\n")
    out_f.write("Đây có thể là các nút/text mới thêm vào game nhưng chưa thiết lập Localization.\n\n")
    for f, t in sorted(unknown_unlocalized):
        if len(t) < 3 and t.isupper(): continue 
        out_f.write(f"- `{f}`: \"{t}\"\n")
