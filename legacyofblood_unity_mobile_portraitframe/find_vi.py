import os
import re

search_dir = r"e:\game\legendofblood\legacyofblood_unity_mobile_portraitframe\Assets\Scripts"
out_file = r"C:\Users\NGOCMINHPC\.gemini\antigravity\brain\47e74976-f154-41b3-b331-75fb9d9d891f\vi_strings_found.md"

# Broad regex for any string literal containing Vietnamese diacritics
vi_pattern = re.compile(r'\"([^\"]*[àáãạảăắằẳẵặâấầẩẫậèéẹẻẽêềếểễệđìíĩỉịòóõọỏôốồổỗộơớờởỡợùúũụủưứừửữựỳỵỷỹýÀÁÃẠẢĂẮẰẲẴẶÂẤẦẨẪẬÈÉẸẺẼÊỀẾỂỄỆĐÌÍĨỈỊÒÓÕỌỎÔỐỒỔỖỘƠỚỜỞỠỢÙÚŨỤỦƯỨỪỬỮỰỲỴỶỸÝ][^\"]*)\"')

found_items = []

for root, dirs, files in os.walk(search_dir):
    for file in files:
        if file.endswith('.cs'):
            filepath = os.path.join(root, file)
            with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
                lines = f.readlines()
                for i, line in enumerate(lines):
                    if "Debug." in line or "DebugFormat" in line or "throw new" in line or "MenuItem(" in line:
                        continue
                    
                    matches = vi_pattern.findall(line)
                    for m in matches:
                        rel_path = os.path.relpath(filepath, search_dir)
                        found_items.append(f"- `{rel_path}:{i+1}` => \"{m}\"")

with open(out_file, 'w', encoding='utf-8') as f:
    f.write(f"# Found {len(found_items)} Vietnamese strings\n\n")
    for item in found_items:
        f.write(item + "\n")
