import os
import re

print("="*50)
print("BẮT ĐẦU KIỂM TRA KEY LOCALIZATION")
print("="*50)

# Paths
lang_dir = r"d:\game\legendofblood\legacyofblood_unity_mobile_portraitframe\Assets\Resources\Localization"
scripts_dir = r"d:\game\legendofblood\legacyofblood_unity_mobile_portraitframe\Assets\Scripts"

vi_path = os.path.join(lang_dir, "vi.txt")
en_path = os.path.join(lang_dir, "en.txt")

# 1. Parse Keys from Text Files
def get_keys(filepath):
    keys = set()
    try:
        with open(filepath, 'r', encoding='utf-8-sig') as f:
            for line_idx, line in enumerate(f):
                line = line.strip()
                if line and not line.startswith('#'):
                    parts = line.split('=', 1)
                    if len(parts) >= 2:
                        keys.add(parts[0].strip())
    except Exception as e:
        print(f"Error reading {filepath}: {e}")
    return keys

vi_keys = get_keys(vi_path)
en_keys = get_keys(en_path)

print(f"- Số lượng key ở vi.txt: {len(vi_keys)}")
print(f"- Số lượng key ở en.txt: {len(en_keys)}")

only_in_vi = vi_keys - en_keys
only_in_en = en_keys - vi_keys

if only_in_vi:
    print(f"\n[CẢNH BÁO] Có {len(only_in_vi)} KEY có trong Tiếng Việt nhưng THIẾU trong Tiếng Anh:")
    for k in sorted(only_in_vi):
        print(f"  + {k}")
else:
    print("\n[TỐT] Không có key nào Tiếng Việt có mà Tiếng Anh thiếu.")

if only_in_en:
    print(f"\n[CẢNH BÁO] Có {len(only_in_en)} KEY có trong Tiếng Anh nhưng THIẾU trong Tiếng Việt:")
    for k in sorted(only_in_en):
        print(f"  + {k}")
else:
    print("\n[TỐT] Không có key nào Tiếng Anh có mà Tiếng Việt thiếu.")


# 2. Parse Keys from C# Scripts
print("\nĐang quét C# code để tìm LocalizationSystem.GetText(\"...\")...")
cs_keys = set()
gettext_pattern = re.compile(r'LocalizationSystem\.GetText\(\s*"([^"]+)"\s*\)')

scanned_files = 0
for root, dirs, files in os.walk(scripts_dir):
    for file in files:
        if file.endswith('.cs'):
            scanned_files += 1
            fpath = os.path.join(root, file)
            with open(fpath, 'r', encoding='utf-8', errors='ignore') as f:
                content = f.read()
                matches = gettext_pattern.findall(content)
                for m in matches:
                    cs_keys.add(m)

print(f"- Đã quét {scanned_files} file C# và tìm thấy {len(cs_keys)} hardcoded keys.")

# Compare C# keys with Text keys
all_text_keys = vi_keys.union(en_keys)
missing_in_text = cs_keys - all_text_keys

if missing_in_text:
    print(f"\n[NGUY HIỂM] Có {len(missing_in_text)} KEY được gọi trong C# code nhưng KHÔNG CÓ TRONG CẢ 2 FILE DỊCH:")
    for k in sorted(missing_in_text):
        print(f"  ! {k}")
else:
    print("\n[RẤT TỐT] Tất cả các keys được gọi bằng code GetText(\"...\") đều đã tồn tại trong file dịch thuật.")

print("="*50)
print("HOÀN TẤT KIỂM TRA.")
print("="*50)
