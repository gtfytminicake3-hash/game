import re
import os

file_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Assets", "Scripts", "Editor", "buildpanelmissng.cs")

with open(file_path, 'r', encoding='utf-8') as f:
    text = f.read()

# Replacements for direct assignment to AssignPrivateField
pattern = r'(script|reportScript|newScript|slotScript)\.([a-zA-Z0-9_]+)\s*=\s*([^;]+);'

def repl(m):
    obj = m.group(1)
    field = m.group(2)
    val = m.group(3)
    return f'AssignPrivateField({obj}, "{field}", {val});'

new_text = re.sub(pattern, repl, text)

# Add AssignPrivateField to each of the three classes: AutoUIBuilder, AutoPanelBuilder, AutoPanelBuilderPart3
helper_method = '''
    static void AssignPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
        }
        else
        {
            UnityEngine.Debug.LogError($"Field {fieldName} not found in {target.GetType()}");
        }
    }
'''

# We need to insert the helper_method before the last closing brace of each class.
new_text = new_text.replace('}\n\npublic class AutoPanelBuilder : EditorWindow', helper_method + '}\n\npublic class AutoPanelBuilder : EditorWindow')
new_text = new_text.replace('}\n\npublic class AutoPanelBuilderPart3 : EditorWindow', helper_method + '}\n\npublic class AutoPanelBuilderPart3 : EditorWindow')
if not new_text.endswith(helper_method + '}'):
    # add to the very end of file
    new_text = new_text.rstrip()
    if new_text.endswith('}'):
        new_text = new_text[:-1] + helper_method + '}'

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(new_text)

print('Replaced')
