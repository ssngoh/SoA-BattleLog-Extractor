setlocal enabledelayedexpansion
set currentDir=%cd%
set imagesStr=
set imagesStr2=
set imagesStr3=
set imagesStr4=
set imagesStr5=
set loop=0
set currentPage=1
set totalPages=%1
set /a totalPages=%totalPages%+1

:GoNextFolder
cd "%currentDir%/Images/%currentPage%"
set /a currentPage=%currentPage%+1
set /a loop=0
set imagesStr=
set imagesStr2=
set imagesStr3=
set imagesStr4=
set imagesStr5=
set imagesStr6=
set imagesStr7=

goto ParseImage

:ParseImage
for /r %%i in (*) do (
	rem 1537 x 1509
	magick convert %%~ni.png -gravity North -chop 0x383 +repage %%~ni.png
	magick convert %%~ni.png -gravity South -chop 0x409 +repage %%~ni.png
	magick convert %%~ni.png -gravity East -chop 100x0 +repage %%~ni.png
	magick convert %%~ni.png -fuzz 40%% -fill white +opaque black %%~ni.png

	rem Used for pre 8.0.0 version logs
	rem set "imagesStr=!imagesStr! !loop!.png"

	if %%~ni EQU 26 magick convert %%~ni.png -gravity South -chop 0x1104 +repage 26_1.png
	if !loop! LSS 26 set "imagesStr=!imagesStr! !loop!.png"
	if !loop! EQU 26 set "imagesStr=!imagesStr! 26_1.png"

	if !loop! GEQ 26 (
		if !loop! LSS 52 set "imagesStr2=!imagesStr2! !loop!.png"
	)

	if %%~ni EQU 52 magick convert %%~ni.png -gravity South -chop 0x1104 +repage 52_1.png
	if !loop! EQU 52 set "imagesStr2=!imagesStr2! 52_1.png"

	if !loop! GEQ 52 (
		if !loop! LSS 78 set "imagesStr3=!imagesStr3! !loop!.png"
	)

	if %%~ni EQU 78 magick convert %%~ni.png -gravity South -chop 0x1104 +repage 78_1.png
	if !loop! EQU 78 set "imagesStr3=!imagesStr3! 78_1.png"

	if !loop! GEQ 78 (
		if !loop! LSS 104 set "imagesStr4=!imagesStr4! !loop!.png"
	)

	if %%~ni EQU 104 magick convert %%~ni.png -gravity South -chop 0x1104 +repage 104_1.png
	if !loop! EQU 104 set "imagesStr4=!imagesStr4! 104_1.png"

	if !loop! GEQ 104 (
		if !loop! LSS 130 set "imagesStr5=!imagesStr5! !loop!.png"
	)

	if %%~ni EQU 130 magick convert %%~ni.png -gravity South -chop 0x1104 +repage 130_1.png
	if !loop! EQU 130 set "imagesStr5=!imagesStr5! 130_1.png"

	if !loop! GEQ 130 (
		if !loop! LSS 156 set "imagesStr6=!imagesStr6! !loop!.png"
	)


	if %%~ni EQU 156 magick convert %%~ni.png -gravity South -chop 0x1104 +repage 156_1.png
	if !loop! EQU 156 set "imagesStr6=!imagesStr6! 156_1.png"

	if !loop! GEQ 156 set "imagesStr7=!imagesStr7! !loop!.png"

	set /a loop=!loop!+1

)

echo !imagesStr!
magick convert -append !imagesStr! out.png
magick convert out.png -bordercolor White -border 10x10 out.png
magick convert out.png -alpha off out.png
magick convert -units PixelsPerInch out.png -density 300 out.png

magick convert out.png -morphology erode diamond:1 out.png

magick convert out.png -sharpen 0x1 out.png

echo !imagesStr2!
magick convert -append !imagesStr2! out2.png
magick convert out2.png -bordercolor White -border 10x10 out2.png
magick convert out2.png -alpha off out2.png
magick convert -units PixelsPerInch out2.png -density 300 out2.png

magick convert out2.png -morphology erode diamond:1 out2.png

magick convert out2.png -sharpen 0x1 out2.png

echo !imagesStr3!
magick convert -append !imagesStr3! out3.png
magick convert out3.png -bordercolor White -border 10x10 out3.png
magick convert out3.png -alpha off out3.png
magick convert -units PixelsPerInch out3.png -density 300 out3.png

magick convert out3.png -morphology erode diamond:1 out3.png

magick convert out3.png -sharpen 0x1 out3.png

echo !imagesStr4!
magick convert -append !imagesStr4! out4.png
magick convert out4.png -bordercolor White -border 10x10 out4.png
magick convert out4.png -alpha off out4.png
magick convert -units PixelsPerInch out4.png -density 300 out4.png

magick convert out4.png -morphology erode diamond:1 out4.png

magick convert out4.png -sharpen 0x1 out4.png

echo !imagesStr5!
magick convert -append !imagesStr5! out5.png
magick convert out5.png -bordercolor White -border 10x10 out5.png
magick convert out5.png -alpha off out5.png
magick convert -units PixelsPerInch out5.png -density 300 out5.png

magick convert out5.png -morphology erode diamond:1 out5.png

magick convert out5.png -sharpen 0x1 out5.png

echo !imagesStr6!
magick convert -append !imagesStr6! out6.png
magick convert out6.png -bordercolor White -border 10x10 out6.png
magick convert out6.png -alpha off out6.png
magick convert -units PixelsPerInch out6.png -density 300 out6.png

magick convert out6.png -morphology erode diamond:1 out6.png

magick convert out6.png -sharpen 0x1 out6.png

echo !imagesStr7!
magick convert -append !imagesStr7! out7.png
magick convert out7.png -bordercolor White -border 10x10 out7.png
magick convert out7.png -alpha off out7.png
magick convert -units PixelsPerInch out7.png -density 300 out7.png

magick convert out7.png -morphology erode diamond:1 out7.png

magick convert out7.png -sharpen 0x1 out7.png

if %currentPage% LSS %totalPages% goto GoNextFolder
cd %currentDir%
set /a totalPages=%totalPages%-1

python LogReader.py %totalPages%