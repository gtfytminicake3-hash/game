import os
import re

# UIPanelType enum mapping
panel_types = {
    0: "None", 1: "MainScreen", 2: "HeroInfo", 3: "Breeding", 4: "Hospital", 
    5: "Arena", 6: "WorldMap", 7: "SquadSelection", 8: "HeroPicker", 9: "Inventory", 
    10: "ArenaShop", 11: "ProfessionSelection", 12: "Recruitment", 13: "Settings", 
    14: "BuildingUpgrade", 15: "Tutorial", 16: "Bootloader", 17: "Mailbox", 
    18: "Barrack", 19: "Battle", 20: "PopulationManager", 21: "Quest", 
    22: "BossBattle", 23: "Tower", 24: "POI_Info", 25: "Menu"
}

guid = "43070fd65c050c54892b8f6a88b1a418"
assets_path = r"d:\game\legendofblood\legacyofblood_unity_mobile_portraitframe\Assets"

# Regex patterns
game_object_pattern = re.compile(r"--- !u!1 &(\d+).*?\nGameObject:.*?m_Name: (.*?)\n", re.DOTALL)
component_pattern = re.compile(r"- component: {fileID: (\d+)}")
mono_behaviour_pattern = re.compile(r"--- !u!114 &(\d+).*?\nMonoBehaviour:.*?m_GameObject: {fileID: (\d+)}.*?m_Script: {fileID: \d+, guid: " + guid + ".*?\ntargetPanel: (\d+).*?\nisBackButton: (\d+)", re.DOTALL)
mono_behaviour_pattern_alt = re.compile(r"--- !u!114 &(\d+).*?\nMonoBehaviour:.*?m_Script: {fileID: \d+, guid: " + guid + ".*?\ntargetPanel: (\d+).*?\nisBackButton: (\d+).*?\nm_GameObject: {fileID: (\d+)}", re.DOTALL)


results = []

for root, dirs, files in os.walk(assets_path):
    for file in files:
        if file.endswith(".prefab") or file.endswith(".unity"):
            file_path = os.path.join(root, file)
            with open(file_path, "r", encoding="utf-8", errors="ignore") as f:
                content = f.read()
                
            if guid not in content:
                continue
                
            # Extract GameObjects map { fileID: name }
            game_objects = {}
            for match in game_object_pattern.finditer(content):
                game_objects[match.group(1)] = match.group(2)
                
            # Find all UIPanelNavButton usages
            mono_blocks = content.split("--- !u!114 &")
            for block in mono_blocks[1:]:
                if "guid: " + guid in block:
                    # Find m_GameObject fileID
                    go_match = re.search(r"m_GameObject: {fileID: (\d+)}", block)
                    target_match = re.search(r"targetPanel: (\d+)", block)
                    back_match = re.search(r"isBackButton: (\d+)", block)
                    
                    if go_match:
                        go_id = go_match.group(1)
                        go_name = game_objects.get(go_id, "Unknown")
                        target_val = int(target_match.group(1)) if target_match else 0
                        is_back = back_match.group(1) == "1" if back_match else False
                        
                        target_name = panel_types.get(target_val, "Unknown")
                        if is_back:
                            target_name = "BACK (Return to previous)"
                            
                        results.append(f"[{file}] Button GameObject '{go_name}' -> Panels: {target_name}")

if results:
    print("Found UI Button Mappings via UIPanelNavButton:")
    for r in results:
        print(r)
else:
    print("No direct mappings found using UIPanelNavButton.")
