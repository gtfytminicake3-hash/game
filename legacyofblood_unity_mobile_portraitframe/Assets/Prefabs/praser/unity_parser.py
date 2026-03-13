import sys
import re
import os
import glob

def parse_unity_yaml(filepath, output_path):
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
    except Exception as e:
        print(f"Error reading {filepath}: {e}")
        return

    docs = content.split('--- !u!')
    
    objects = {}
    
    for doc in docs[1:]:
        lines = doc.splitlines()
        if not lines: continue
        
        header = lines[0].strip()
        parts = header.split()
        if len(parts) >= 2:
            obj_type_id = parts[0].replace('!u!', '')
            obj_id = parts[1].replace('&', '')
            
            obj_type_name = "Unknown"
            if len(lines) > 1:
                obj_type_name = lines[1].split(':')[0].strip()
            
            obj_data = {'id': obj_id, 'type_id': obj_type_id, 'type_name': obj_type_name, 'lines': lines[2:]}
            objects[obj_id] = obj_data

    gameobjects = {}
    transforms = {}
    components = {}
    prefabs = {}

    for obj_id, obj in objects.items():
        if obj['type_name'] == 'GameObject':
            go_data = {'name': 'Unknown', 'components': []}
            for line in obj['lines']:
                line_s = line.strip()
                if line_s.startswith('m_Name:'):
                    go_data['name'] = line_s.split(':', 1)[1].strip()
                elif '- component:' in line_s:
                    match = re.search(r'fileID:\s*(\d+)', line_s)
                    if match:
                        go_data['components'].append(match.group(1))
            gameobjects[obj_id] = go_data
            
        elif obj['type_name'] in ['Transform', 'RectTransform']:
            t_data = {'father': None, 'go': None}
            for line in obj['lines']:
                line_s = line.strip()
                if line_s.startswith('m_Father:'):
                    match = re.search(r'fileID:\s*(\d+)', line_s)
                    if match and match.group(1) != '0':
                        t_data['father'] = match.group(1)
                elif line.startswith('m_GameObject:'):
                    match = re.search(r'fileID:\s*(\d+)', line_s)
                    if match:
                        t_data['go'] = match.group(1)
            transforms[obj_id] = t_data
            
        elif obj['type_name'] in ['MonoBehaviour', 'Image', 'Button', 'Text', 'TextMeshProUGUI']:
            script_guid = None
            props = []
            for line in obj['lines']:
                line_s = line.strip()
                if line_s.startswith('m_Script:'):
                    match = re.search(r'guid:\s*([a-f0-9]+)', line_s)
                    if match:
                        script_guid = match.group(1)
                elif ':' in line_s and not line_s.startswith('m_') and not line_s.startswith('serializedVersion') and not 'fileID' in line_s:
                    props.append(line_s)
            
            components[obj_id] = {'type': obj['type_name'], 'script_guid': script_guid, 'props': props}
            
        elif obj['type_name'] == 'PrefabInstance':
            prefabs[obj_id] = {'modifications': []}
            father = None
            expecting_name = False
            for line in obj['lines']:
                line_s = line.strip()
                if 'm_TransformParent:' in line_s:
                    match = re.search(r'fileID:\s*(\d+)', line_s)
                    if match and match.group(1) != '0':
                        father = match.group(1)
                elif 'propertyPath: m_Name' in line_s:
                    expecting_name = True
                elif expecting_name and line_s.startswith('value:'):
                    prefabs[obj_id]['name'] = line_s.split(':', 1)[1].strip()
                    expecting_name = False
            prefabs[obj_id]['father'] = father
                    
    children = {}
    roots = []
    
    go_to_transform = {}
    for t_id, t in transforms.items():
        if t['go']:
            go_to_transform[t['go']] = t_id
            father_id = t['father']
            if father_id:
                if father_id not in children:
                    children[father_id] = []
                children[father_id].append(t['go'])
    
    for go_id, go in gameobjects.items():
        t_id = go_to_transform.get(go_id)
        if t_id:
            father_id = transforms[t_id]['father']
            if not father_id:
                roots.append(go_id)
                children[t_id] = children.get(t_id, [])
        else:
            roots.append(go_id)

    prefab_roots = []
    for p_id, p in prefabs.items():
        father_id = p.get('father')
        if father_id:
            if father_id not in children:
                children[father_id] = []
            children[father_id].append(f'PREFAB_{p_id}')
        else:
            prefab_roots.append(p_id)

    os.makedirs(os.path.dirname(output_path), exist_ok=True)
    with open(output_path, 'w', encoding='utf-8') as out:
        def print_tree(go_id, indent=0):
            if str(go_id).startswith('PREFAB_'):
                p_id = str(go_id).replace('PREFAB_', '')
                p = prefabs.get(p_id, {})
                name = p.get('name', f'PrefabInstance_{p_id}')
                out.write('  ' * indent + f'[Prefab] {name} (ID: {p_id})\n')
                return

            go = gameobjects.get(go_id)
            if not go: return
            
            out.write('  ' * indent + f'GameObject: {go["name"]} (ID: {go_id})\n')
            
            for c_id in go['components']:
                if c_id in components:
                    comp = components[c_id]
                    script = comp['script_guid'] and f" (Script GUID: {comp['script_guid']})" or ""
                    out.write('  ' * (indent+1) + f'- {comp["type"]}{script}\n')
                    for prop in comp['props']:
                        if len(prop) < 80 and not '{' in prop:
                            match = re.match(r'^([a-zA-Z0-9_]+):\s*(.*)$', prop)
                            if match:
                                key = match.group(1)
                                val = match.group(2)
                                if val:
                                    out.write('  ' * (indent+2) + f'{key}: {val}\n')
            
            t_id = go_to_transform.get(go_id)
            if t_id and t_id in children:
                for child_go in children[t_id]:
                    print_tree(child_go, indent + 1)
                    
        out.write(f"====== PREFAB/SCENE STRUCTURE FOR {os.path.basename(filepath)} ======\n")
        out.write("========================================================\n")
        for root in roots:
            print_tree(root, 0)
            
        for root in prefab_roots:
            p = prefabs.get(root, {})
            name = p.get('name', f'PrefabInstance_{root}')
            out.write(f'[Prefab Root] {name} (ID: {root})\n')

def batch_process(input_dir, output_dir):
    search_pattern = os.path.join(input_dir, '**', '*.prefab')
    files = glob.glob(search_pattern, recursive=True)
    print(f"Found {len(files)} prefabs in {input_dir}")
    
    os.makedirs(output_dir, exist_ok=True)
    
    for filepath in files:
        filename = os.path.basename(filepath)
        output_filename = filename + '.txt'
        output_path = os.path.join(output_dir, output_filename)
        print(f"Parsing {filename} -> {output_path}")
        parse_unity_yaml(filepath, output_path)
        
    # Also parse GameClient.unity
    gameclient_path = os.path.join(os.path.dirname(input_dir), 'Scenes', 'GameClient.unity')
    if os.path.exists(gameclient_path):
        gc_out = os.path.join(output_dir, 'GameClient.unity.txt')
        print(f"Parsing GameClient.unity -> {gc_out}")
        parse_unity_yaml(gameclient_path, gc_out)
        
if __name__ == '__main__':
    if len(sys.argv) >= 3:
        batch_process(sys.argv[1], sys.argv[2])
    else:
        print("Usage: python unity_parser.py <input_prefab_dir> <output_dir>")
