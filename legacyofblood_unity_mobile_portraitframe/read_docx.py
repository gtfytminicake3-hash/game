import sys
import subprocess
import codecs
try:
    import docx
except ImportError:
    subprocess.check_call([sys.executable, "-m", "pip", "install", "python-docx"])
    import docx

doc = docx.Document(r"C:\Users\admin\Downloads\Bao_cao_tien_doI_LegendOfBlood.docx")
with codecs.open("docx_output.txt", "w", "utf-8") as f:
    for para in doc.paragraphs:
        f.write(para.text + "\n")
