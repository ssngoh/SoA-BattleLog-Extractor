try:
    from PIL import Image
except ImportError:
    import Image
import pytesseract
import sys

# CHANGE THIS TO YOUR IMAGE PATH
imgPath="C:/Users/Onsla/OneDrive/Desktop/Sinoalice SQL/Images/"

# If you don't have tesseract executable in your PATH, include the following:
pytesseract.pytesseract.tesseract_cmd = r'C:\\Program Files\\Tesseract-OCR\\tesseract.exe'
# Example tesseract_cmd = r'C:\Program Files (x86)\Tesseract-OCR\tesseract'

# Simple image to string
#print(pytesseract.image_to_string(Image.open('image.png')))

#Use this to change to other languages. Not recommended though
#custom_oem_psm_config='-l eng+kor+chi_sim load_system_dawg 0 load_freq_dawg 0'
# custom_oem_psm_config='-l eng load_system_dawg 0 load_freq_dawg 0'
custom_oem_psm_config='-l eng load_system_dawg 0 load_freq_dawg 0'

for x in range(1,int(sys.argv[1])+1):
	path=imgPath+str(x)
	print(path)

	with open(path+"/tesseractOutput.txt", "w", encoding='utf-8') as textFile:
		textFile.write(pytesseract.image_to_string(Image.open(path+'/out.png'),config=custom_oem_psm_config))
		textFile.write('\n')
		textFile.close()

	with open(path+"/tesseractOutput.txt", "a", encoding='utf-8') as textFile:
		textFile.write(pytesseract.image_to_string(Image.open(path+'/out2.png'),config=custom_oem_psm_config))
		textFile.write('\n')
		textFile.close()

	with open(path+"/tesseractOutput.txt", "a", encoding='utf-8') as textFile:
		textFile.write(pytesseract.image_to_string(Image.open(path+'/out3.png'),config=custom_oem_psm_config))
		textFile.write('\n')
		textFile.close()

	with open(path+"/tesseractOutput.txt", "a", encoding='utf-8') as textFile:
		textFile.write(pytesseract.image_to_string(Image.open(path+'/out4.png'),config=custom_oem_psm_config))
		textFile.write('\n')
		textFile.close()

	with open(path+"/tesseractOutput.txt", "a", encoding='utf-8') as textFile:
		textFile.write(pytesseract.image_to_string(Image.open(path+'/out5.png'),config=custom_oem_psm_config))
		textFile.write('\n')
		textFile.close()

	with open(path+"/tesseractOutput.txt", "a", encoding='utf-8') as textFile:
		textFile.write(pytesseract.image_to_string(Image.open(path+'/out6.png'),config=custom_oem_psm_config))
		textFile.write('\n')
		textFile.close()

	with open(path+"/tesseractOutput.txt", "a", encoding='utf-8') as textFile:
		textFile.write(pytesseract.image_to_string(Image.open(path+'/out7.png'),config=custom_oem_psm_config))
		textFile.write('\n')
		textFile.close()

# French text image to string
#print(pytesseract.image_to_string(Image.open('test-european.jpg'), lang='fra'))

# In order to bypass the image conversions of pytesseract, just use relative or absolute image path
# NOTE: In this case you should provide tesseract supported images or tesseract will return error
#print(pytesseract.image_to_string('test.jpg'))

# Batch processing with a single file containing the list of multiple image file paths
#print(pytesseract.image_to_string('images.txt'))

# Timeout/terminate the tesseract job after a period of time
# try:
#     print(pytesseract.image_to_string('test.jpg', timeout=2)) # Timeout after 2 seconds
#     print(pytesseract.image_to_string('test.jpg', timeout=0.5)) # Timeout after half a second
# except RuntimeError as timeout_error:
#     # Tesseract processing is terminated
#     pass

# Get bounding box estimates
#print(pytesseract.image_to_boxes(Image.open('test.jpg')))

# Get verbose data including boxes, confidences, line and page numbers
#print(pytesseract.image_to_data(Image.open('test.jpg')))

# Get information about orientation and script detection
#print(pytesseract.image_to_osd(Image.open('test.jpg')))

# Get a searchable PDF
#pdf = pytesseract.image_to_pdf_or_hocr('test.jpg', extension='pdf')
#with open('test.pdf', 'w+b') as f:
#    f.write(pdf) # pdf type is bytes by default

# Get HOCR output
#hocr = pytesseract.image_to_pdf_or_hocr('test.jpg', extension='hocr')

# Get ALTO XML output
#xml = pytesseract.image_to_alto_xml('test.jpg')