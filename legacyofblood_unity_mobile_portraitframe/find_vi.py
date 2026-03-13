import os
import re

search_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'Assets', 'Scripts')
out_file = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'vi_strings_found.md')

# Broad regex for any string literal containing Vietnamese diacritics
vi_pattern = re.compile(r'\"([^\"]*[Ã Ã¡Ã£áº¡áº£Äƒáº¯áº±áº³áºµáº·Ã¢áº¥áº§áº©áº«áº­Ã¨Ã©áº¹áº»áº½Ãªá»áº¿á»ƒá»…á»‡Ä‘Ã¬Ã­Ä©á»‰á»‹Ã²Ã³Ãµá»á»Ã´á»‘á»“á»•á»—á»™Æ¡á»›á»á»Ÿá»¡á»£Ã¹ÃºÅ©á»¥á»§Æ°á»©á»«á»­á»¯á»±á»³á»µá»·á»¹Ã½Ã€ÃÃƒáº áº¢Ä‚áº®áº°áº²áº´áº¶Ã‚áº¤áº¦áº¨áºªáº¬ÃˆÃ‰áº¸áººáº¼ÃŠá»€áº¾á»‚á»„á»†ÄÃŒÃÄ¨á»ˆá»ŠÃ’Ã“Ã•á»Œá»ŽÃ”á»á»’á»”á»–á»˜Æ á»šá»œá»žá» á»¢Ã™ÃšÅ¨á»¤á»¦Æ¯á»¨á»ªá»¬á»®á»°á»²á»´á»¶á»¸Ã][^\"]*)\"')

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

