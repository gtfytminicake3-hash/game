import zipfile
import os

docx_path = r"C:\Users\admin\Downloads\Bao_cao_tien_doI_LegendOfBlood.docx"
extract_dir = r"D:\game\legendofblood\doc_images"
os.makedirs(extract_dir, exist_ok=True)

with zipfile.ZipFile(docx_path, 'r') as zip_ref:
    for item in zip_ref.namelist():
        if item.startswith('word/media/'):
            zip_ref.extract(item, extract_dir)
            print(f"Extracted: {item}")
