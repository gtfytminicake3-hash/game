import os

lang_dir = r"d:\game\legendofblood\legacyofblood_unity_mobile_portraitframe\Assets\Resources\Localization"
en_path = os.path.join(lang_dir, "en.txt")
vi_path = os.path.join(lang_dir, "vi.txt")

def clean_file(filepath):
    lines = []
    with open(filepath, 'r', encoding='utf-8-sig') as f:
        skip_mode = False
        for line in f:
            line_s = line.strip()
            
            # The script previously appended '# --- Missing from Vietnamese ---' and '# --- Prefab Extracted UI Keys ---'
            # Those lines and everything after them should be wiped from BOTH files to restore clean state.
            if line_s == "# --- Missing from Vietnamese ---" or line_s == "# --- Missing from English ---" or line_s == "# --- Prefab Extracted UI Keys ---":
                skip_mode = True
                
            if not skip_mode:
                lines.append(line)

    # Let's write them back
    with open(filepath, 'w', encoding='utf-8-sig') as f:
         for l in lines:
             f.write(l)

clean_file(en_path)
clean_file(vi_path)
print("Cleaned both files by removing the previously appended sections.")
