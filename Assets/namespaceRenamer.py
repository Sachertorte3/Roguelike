import os
import re

def replace_namespace_in_file(file_path, old_ns, new_ns):
    with open(file_path, 'r') as f:
        content = f.read()

    # 正規表現パターンを定義
    pattern = re.compile(f'{re.escape(old_ns)}:')
    
    # デバッグ用に一致する部分を表示
    matches = pattern.findall(content)
    if matches:
        print(f"Matches found in {file_path}: {matches}")
    else:
        print(f"No matches found in {file_path}")
        return

    # new_contentの置換部分を修正
    new_content = pattern.sub(f'{new_ns}:', content)

    with open(file_path, 'w', encoding='utf-8') as file:
        file.write(new_content)

def replace_namespace_in_folder(folder_path, old_ns, new_ns):
    for root, _, files in os.walk(folder_path):
        print(files)
        for file in files:
            if file.endswith('.asset'):
                file_path = os.path.join(root, file)
                replace_namespace_in_file(file_path, old_ns, new_ns)

if __name__ == "__main__":
    folder_path = r"C:\Users\Torte\Documents\Unity\LogRogue\Assets\Database"  # 対象のフォルダーのパスを指定
    old_ns = "Log"
    new_ns = "<Log>k__BackingField"
    replace_namespace_in_folder(folder_path, old_ns, new_ns)